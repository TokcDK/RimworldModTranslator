using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace RimworldModTranslator.ViewModels
{
    public partial class SearchWindowViewModel : ObservableObject
    {
        public IEnumerable<string> ColumnNames => TranslationsTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName);

        [ObservableProperty]
        private DataTable _translationsTable;

        private int _currentRowIndex = -1;

        [ObservableProperty]
        private ObservableCollection<SearchOptionsData> _searchOptions = new();

        [ObservableProperty]
        private DataRow? _currentSelectedRow;

        [ObservableProperty]
        private ObservableCollection<DataRowView> _foundItems = new();

        private readonly TranslationEditorViewModel _parentViewModel;

        public SearchWindowViewModel(DataTable translationsTable, TranslationEditorViewModel parentViewModel)
        {
            TranslationsTable = translationsTable ?? throw new ArgumentNullException(nameof(translationsTable));
            _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
            
            // Subscribe to collection changes to refresh CanExecute
            SearchOptions.CollectionChanged += (s, e) =>
            {
                SearchCommandsNotifyCanExecuteChanged();
                RemoveTabCommand.NotifyCanExecuteChanged();
            };

            AddSearchOption();
        }

        private void SearchCommandsNotifyCanExecuteChanged()
        {
            SearchCommand.NotifyCanExecuteChanged();
            SearchAllCommand.NotifyCanExecuteChanged();
            ReplaceCommand.NotifyCanExecuteChanged();
            ReplaceAllCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void AddTab()
        {
            AddSearchOption();
        }

        private void AddSearchOption()
        {
            var newOpt = new SearchOptionsData();
            newOpt.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName is nameof(SearchOptionsData.SearchWhat) or nameof(SearchOptionsData.SelectedColumn))
                {
                    SearchCommandsNotifyCanExecuteChanged();
                }
            };
            SearchOptions.Add(newOpt);
        }

        [RelayCommand(CanExecute = nameof(CanRemoveTab))]
        private void RemoveTab()
        {
            if (SearchOptions.Count > 1)
            {
                SearchOptions.RemoveAt(SearchOptions.Count - 1);
            }
        }

        private bool CanRemoveTab() => SearchOptions.Count > 1;

        [RelayCommand(CanExecute = nameof(CanExecuteSearchOrReplace))]
        private void Search()
        {
            int startIndex = (_currentRowIndex + 1) % TranslationsTable.Rows.Count;
            for (int i = startIndex; i < TranslationsTable.Rows.Count; i++)
            {
                if (IsRowMatch(TranslationsTable.Rows[i]))
                {
                    CurrentSelectedRow = TranslationsTable.Rows[i];
                    _currentRowIndex = i;
                    _parentViewModel.SelectedRow = TranslationsTable.DefaultView[_currentRowIndex];
                    _parentViewModel.SelectedRowIndex = _currentRowIndex;
                    return;
                }
            }
            // Wrap around if no match found
            for (int i = 0; i < startIndex; i++)
            {
                if (IsRowMatch(TranslationsTable.Rows[i]))
                {
                    CurrentSelectedRow = TranslationsTable.Rows[i];
                    _currentRowIndex = i;
                    _parentViewModel.SelectedRow = TranslationsTable.DefaultView[_currentRowIndex];
                    return;
                }
            }
            CurrentSelectedRow = null;
            _parentViewModel.SelectedRow = null;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteSearchOrReplace))]
        private void SearchAll()
        {
            var matchingRows = TranslationsTable.Rows.Cast<DataRow>()
                .Where(row => IsRowMatch(row))
                .ToList();

            FoundItems.Clear();
            if (matchingRows.Count != 0)
            {
                CurrentSelectedRow = matchingRows[0];
                _currentRowIndex = TranslationsTable.Rows.IndexOf(CurrentSelectedRow);
                _parentViewModel.SelectedRow = TranslationsTable.DefaultView[_currentRowIndex];

                // Populate FoundItems with DataRowView objects from DefaultView
                foreach (var row in matchingRows)
                {
                    int index = TranslationsTable.Rows.IndexOf(row);
                    FoundItems.Add(TranslationsTable.DefaultView[index]);
                }
            }
            else
            {
                CurrentSelectedRow = null;
                _parentViewModel.SelectedRow = null;
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteSearchOrReplace))]
        private void Replace()
        {
            if (CurrentSelectedRow != null && IsRowMatch(CurrentSelectedRow))
            {
                PerformReplacement(CurrentSelectedRow);
                Search();
            }
        }

        [RelayCommand(CanExecute = nameof(CanExecuteSearchOrReplace))]
        private void ReplaceAll()
        {
            foreach (DataRow row in TranslationsTable.Rows)
            {
                if (IsRowMatch(row))
                {
                    PerformReplacement(row);
                }
            }
            // Refresh FoundItems after replacement
            SearchAll();
        }

        private bool CanExecuteSearchOrReplace()
        {
            return SearchOptions.Any(opt => IsValidSearchOrReplaceOption(opt));
        }

        private static bool IsValidSearchOrReplaceOption(SearchOptionsData opt)
        {
            return !string.IsNullOrEmpty(opt.SearchWhat) &&
                   !string.IsNullOrEmpty(opt.SelectedColumn);
        }

        private bool IsRowMatch(DataRow row)
        {
            var groups = SearchOptions
                .Where(opt => IsValidSearchOrReplaceOption(opt))
                .GroupBy(opt => opt.SelectedColumn);
            foreach (var group in groups)
            {
                string column = group.Key;
                if (!TranslationsTable.Columns.Contains(column)) continue;

                string cellValue = row[column]?.ToString() ?? string.Empty;
                foreach (var opt in group)
                {
                    string pattern = opt.IsRegexSearch ? opt.SearchWhat : Regex.Escape(opt.SearchWhat);
                    var regexOptions = opt.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                    if (!Regex.IsMatch(cellValue, pattern, regexOptions))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void PerformReplacement(DataRow row)
        {
            foreach (var opt in SearchOptions)
            {
                string column = opt.SelectedColumn;
                if (!TranslationsTable.Columns.Contains(column)) continue;

                string cellValue = row[column]?.ToString() ?? string.Empty;
                string pattern = opt.IsRegexSearch ? opt.SearchWhat : Regex.Escape(opt.SearchWhat);
                var regexOptions = opt.IsCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                string newValue = Regex.Replace(cellValue, pattern, opt.ReplaceWith ?? string.Empty, regexOptions);
                row[column] = newValue;
            }
        }

        // Method to handle selection from the search DataGrid
        public void OnFoundItemSelected(DataRowView selectedItem)
        {
            if (selectedItem != null)
            {
                _currentRowIndex = TranslationsTable.Rows.IndexOf(selectedItem.Row);
                CurrentSelectedRow = selectedItem.Row;
                _parentViewModel.SelectedRow = selectedItem;
                _parentViewModel.SelectedRowIndex = _currentRowIndex;
            }
        }
    }
}