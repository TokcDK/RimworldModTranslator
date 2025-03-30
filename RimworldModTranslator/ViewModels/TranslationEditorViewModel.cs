using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RimworldModTranslator.Models;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Services;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Data;
using RimworldModTranslator.Helpers;
using System.Windows.Controls;
using System.Collections.Specialized;
using RimworldModTranslator.Views;
using System.Windows;
using System.Data.Common;

namespace RimworldModTranslator.ViewModels
{
    // subfolders and xml file naming
    // For Defs: Languages\%LanguageCode%\DefInjected\XmlParentTagNameInsideOfRootDefsTag\ParentXmlName.xml
    // For keyed (each xml tag value, only from exist language dir): Languages\%LanguageCode%\Keyed\Keyed_%LanguageCode%.xml
    // For common Strings (in txt each line, only from exist language dir): Languages\LanguageCode\Strings\Names\*.txt
    // 
    // key name in xml
    // <%defName_key_value%.%translatable_key_name%>%translatable_key_value%</%defName_key_value%.%translatable_key_name%>

    // defs language xml structure
    // %defName_key_value2% includes the all sub tags before translatable tag
    //
    // <?xml version="1.0" encoding="utf-8"?>
    //<LanguageData>
    //
    //<%defName_key_value1%.%translatable_key_name%>%translatable_key_value%</%defName_key_value1%.%translatable_key_name%>
    //<%defName_key_value1%.%translatable_key_name%>%translatable_key_value%</%defName_key_value1%.%translatable_key_name%>
    //
    //<%defName_key_value2%.%translatable_key_name%>%translatable_key_value%</%defName_key_value2%.%translatable_key_name%>
    //<%defName_key_value2%.%translatable_key_name%>%translatable_key_value%</%defName_key_value2%.%translatable_key_name%>
    //
    //</LanguageData>

    // for editor extra functions to insert most often using replacers
    // replacers: https://rimworldwiki.com/wiki/Modding_Tutorials/GrammarResolver
    public partial class TranslationEditorViewModel : ViewModelBase
    {
        #region Fields
        private ModData? mod;
        private Game? game;
        private readonly SettingsService settingsService;
        string? previousSelectedFolder;
        #endregion

        #region Constructors
        public TranslationEditorViewModel(SettingsService settingsService)
        {
            this.settingsService = settingsService;
            Folders.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsFoldersEnabled));

            InitTranslationsTable();
        }
        #endregion

        #region Properties
        public string Header { get; } = "Editor";

        public bool IsTranslatorEnabled { get => IsTheTranslatorEnabled(); }

        public bool IsAddNewLanguageEnabled { get => IsAddNewLanguageButtonEnabled(); }

        public bool IsFoldersEnabled { get => IsTheFoldersEnabled(); }

        public string? ModDisplayingName => mod != null && Folders.Count > 0 ? mod.ModDisplayingName : settingsService.SelectedMod?.ModDisplayingName;

        public ObservableCollection<FolderData> Folders { get; } = new();

        private IList<DataGridCellInfo>? selectedCells;
        public IList<DataGridCellInfo>? SelectedCells
        {
            get => selectedCells;
            set
            {
                SetProperty(ref selectedCells, value);
            }
        }

        public DataTable? TranslationsTable
        {
            get => SelectedFolder?.TranslationsTable;
            set
            {
                if (SelectedFolder == null || SelectedFolder.TranslationsTable == value)
                {
                    return;
                }
                SelectedFolder.TranslationsTable = value;
            }
        }

        #endregion

        #region Observable Properties
        [ObservableProperty]
        private FolderData? selectedFolder;
        partial void OnSelectedFolderChanged(FolderData? value)
        {
            if (value?.Name == previousSelectedFolder) return;

            previousSelectedFolder = value?.Name;
            InitTranslationsTable(dataTableToRelink: value?.TranslationsTable);
        }

        [ObservableProperty]
        private string? newLanguageName;
        partial void OnNewLanguageNameChanged(string? value)
        {
            OnPropertyChanged(nameof(IsAddNewLanguageEnabled));
        }

        [ObservableProperty]
        private ObservableCollection<string> languages = new();

        [ObservableProperty]
        private DataView? translationsView;

        // will not fire with SelectionUnit="CellOrRowHeader" selected cells but still can be used to select row programmatically
        [ObservableProperty]
        private DataRowView? selectedRow;
        partial void OnSelectedRowChanged(DataRowView? value)
        {
        }
        [ObservableProperty]
        private int selectedRowIndex;
        partial void OnSelectedRowIndexChanged(int value)
        {
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void LoadStrings()
        {
            LoadTheSelectedModStrings();
        }

        public void LoadTheSelectedModStrings()
        {
            if (game == null || game != settingsService.SelectedGame)
            {
                // load only when game was not set or changed
                game = settingsService.SelectedGame;
                if (game == null) return;
            }

            bool isChangedMod = mod != settingsService.SelectedMod;

            if (isChangedMod || mod == null)
            {
                // load only when mod was not set or changed
                mod = settingsService.SelectedMod;
                if (mod == null) return;
            }

            //if(!isChangedMod && previousSelectedFolder == SelectedFolder)
            //{
            //    // dont need? to reload strings for the same mod folder again
            //    return;
            //}
            if (isChangedMod || Folders.Count == 0)
            {
                EditorHelper.GetTranslatableFolders(Folders, game!.ModsDirPath!, mod.DirectoryName!);
            }

            if (Folders.Count == 0) return;

            SelectedFolder ??= Folders[0];

            string selectedFolderName = SelectedFolder!.Name;

            var selectedLanguageDir = Path.Combine(game!.ModsDirPath!, mod!.DirectoryName!, EditorHelper.GetLanguageFolderName(selectedFolderName));

            var stringsData = EditorHelper.LoadStringsDataFromTheLanguageDir(selectedLanguageDir);

            var translationsTable = EditorHelper.CreateTranslationsTable(stringsData);

            if (translationsTable == null || translationsTable.Columns.Count == 0)
            {
                SelectedFolder = Folders.FirstOrDefault(f => f.Name == previousSelectedFolder);
                return;
            }

            SelectedFolder.TranslationsTable = translationsTable;
            InitTranslationsTable(dataTableToRelink: translationsTable);

            OnPropertyChanged(nameof(ModDisplayingName));
        }

        [RelayCommand]
        private void SaveStrings()
        {
            if (game == null) return;
            if (mod == null) return;

            SaveLanguages();
        }

        [RelayCommand]
        private void AddNewLanguage()
        {
            if (game == null) return;
            if (mod == null) return;

            if (string.IsNullOrEmpty(NewLanguageName)) return;
            string newLang = NewLanguageName!.Trim();
            if (TranslationsTable!.Columns.Contains(newLang)) return;

            TranslationsTable.Columns.Add(newLang, typeof(string));

            NewLanguageName = string.Empty;

            InitTranslationsTable(false);
        }

        [RelayCommand]
        private void SaveLanguages()
        {
            EditorHelper.SaveTranslatedStrings(Folders, game, mod);
        }

        [RelayCommand]
        private void OpenSearchWindow()
        {
            if (TranslationsTable == null || TranslationsTable.Rows.Count == 0)
            {
                return;
            }

            var searchViewModel = new SearchWindowViewModel(TranslationsTable, this);
            var searchWindow = new SearchWindow { DataContext = searchViewModel };
            searchWindow.Show();
        }

        [RelayCommand]
        private void PasteStringsInSelectedCells()
        {
            if (TranslationsTable == null) return;

            EditorHelper.PasteStringsInSelectedCells(SelectedCells);
        }

        [RelayCommand]
        private void ClearSelectedCells()
        {
            EditorHelper.ClearSelectedCells(SelectedCells);
        }

        [RelayCommand]
        private void CutSelectedCells()
        {
            EditorHelper.CutSelectedCells(SelectedCells);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Init Translations table and view
        /// </summary>
        /// <param name="fullInit">when false, will be recreated only DataView. TranslationsTable will not be recreated.</param>
        private void InitTranslationsTable(bool fullInit = true, DataTable? dataTableToRelink = null)
        {
            if (fullInit)
            {
                TranslationsTable = dataTableToRelink ?? new DataTable();
            }

            TranslationsView = new DataView(TranslationsTable);
        }

        private bool IsTheFoldersEnabled()
        {
            return game != null
                   && mod != null
                   && Folders.Count > 1;
        }

        private bool IsTheTranslatorEnabled()
        {
            return (settingsService.SelectedGame != null || game != null)
                   && (settingsService.SelectedMod != null || mod != null);
        }

        private bool IsAddNewLanguageButtonEnabled()
        {
            return game != null
                   && mod != null
                   && !string.IsNullOrWhiteSpace(NewLanguageName)
                   && TranslationsTable?.Columns.Count > 0
                   && !TranslationsTable.Columns.Contains(NewLanguageName);
        }
        #endregion
    }

    public class FolderData
    {
        public string Name { get; set; }
        public DataTable? TranslationsTable { get; set; }
    }
}