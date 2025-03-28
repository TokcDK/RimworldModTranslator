using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace RimworldModTranslator.ViewModels
{
    public partial class SearchWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private DataTable _translationsTable;

        private int _currentRowIndex = -1;

        [ObservableProperty]
        private ObservableCollection<SearchOptionsData> _searchOptions = new();

        [ObservableProperty]
        private DataRow? _currentSelectedRow;

        public SearchWindowViewModel(DataTable translationsTable)
        {
            TranslationsTable = translationsTable ?? throw new ArgumentNullException(nameof(translationsTable));
            SearchOptions.Add(new SearchOptionsData()); // Add initial tab

            // Subscribe to collection changes to refresh CanExecute
            SearchOptions.CollectionChanged += (s, e) =>
            {
                SearchCommandsNotifyCanExecuteChanged();
                RemoveTabCommand.NotifyCanExecuteChanged();
            };

            // Subscribe to property changes in each SearchOptionsData
            foreach (var opt in SearchOptions)
            {
                opt.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName is nameof(SearchOptionsData.SearchWhat) or nameof(SearchOptionsData.SelectedColumn))
                    {
                        SearchCommandsNotifyCanExecuteChanged();
                    }
                };
            }
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
                    return;
                }
            }
            CurrentSelectedRow = null; // No match found
        }

        [RelayCommand(CanExecute = nameof(CanExecuteSearchOrReplace))]
        private void SearchAll()
        {
            var matchingRows = TranslationsTable.Rows.Cast<DataRow>()
                .Where(row => IsRowMatch(row))
                .ToList();
            if (matchingRows.Any())
            {
                CurrentSelectedRow = matchingRows[0];
                _currentRowIndex = TranslationsTable.Rows.IndexOf(CurrentSelectedRow);
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
        }

        private bool CanExecuteSearchOrReplace()
        {
            return SearchOptions.Any(opt =>
                !string.IsNullOrEmpty(opt.SearchWhat) &&
                !string.IsNullOrEmpty(opt.SelectedColumn));
        }

        private bool IsRowMatch(DataRow row)
        {
            var groups = SearchOptions.GroupBy(opt => opt.SelectedColumn);
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
    }
}
