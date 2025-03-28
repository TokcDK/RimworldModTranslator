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

        public ObservableCollection<string> Folders { get; } = [];

        private IList<DataGridCellInfo> selectedCells;
        public IList<DataGridCellInfo> SelectedCells
        {
            get => selectedCells;
            set
            {
                SetProperty(ref selectedCells, value);

                // for debug
                //if (selectedCells != null)
                //{
                //    foreach (var cell in selectedCells)
                //    {
                //        var rowItem = cell.Item as DataRowView;
                //        int index = rowItem == null ? -1 : rowItem.Row.Table.Rows.IndexOf(rowItem.Row);
                //        var column = cell.Column as DataGridColumn;
                //        if (rowItem != null && column != null)
                //        {
                //            var cellValue = column.GetCellContent(rowItem)?.GetValue(TextBlock.TextProperty);
                //            //System.Diagnostics.Debug.WriteLine($"Selected Cell: Row={rowItem.Name}, Value={cellValue}");
                //        }
                //    }
                //}
            }
        }

        #endregion

        #region Observable Properties
        [ObservableProperty]
        private string? selectedFolder;
        partial void OnSelectedFolderChanged(string? value)
        {
            if (value == previousSelectedFolder) return;

            previousSelectedFolder = value;
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
        private DataTable? translationsTable;

        [ObservableProperty]
        private DataView? translationsView;

        // will not fire with SelectionUnit="CellOrRowHeader" selected cells but still can be used to select row programmatically
        [ObservableProperty]
        private DataRowView? selectedRow;
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

            if(!isChangedMod && previousSelectedFolder == SelectedFolder)
            {
                // dont need? to reload strings for the same mod folder again
                return;
            }

            if (isChangedMod || Folders.Count == 0)
            {
                EditorHelper.GetTranslatableFolders(Folders, game!.ModsDirPath!, mod.DirectoryName!);
            }

            if (Folders.Count == 0) return; // no translatable folders

            SelectedFolder ??= Folders[0];

            string selectedFolder = SelectedFolder!;

            var selectedLanguageDir = Path.Combine(game!.ModsDirPath!, mod!.DirectoryName!, EditorHelper.GetLanguageFolderIfNeed(selectedFolder));

            EditorStringsData stringsData = new();

            EditorHelper.LoadLanguages(selectedLanguageDir, stringsData);
            EditorHelper.ExtractStrings(selectedLanguageDir, stringsData);

            var translationsTable = EditorHelper.CreateTranslationsTable(stringsData);
            InitTranslationsTable(dataTableToRelink: translationsTable);

            if (translationsTable == null || translationsTable.Columns.Count == 0)
            {
                SelectedFolder = previousSelectedFolder;
                return;
            }

            OnPropertyChanged(nameof(ModDisplayingName)); // update current mod for editor
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
            if (TranslationsTable.Columns.Contains(newLang)) return;

            TranslationsTable.Columns.Add(newLang, typeof(string));

            NewLanguageName = string.Empty;

            InitTranslationsTable(false);
        }

        [RelayCommand]
        private void SaveLanguages()
        {
            if (game == null) return;
            if (mod == null) return;
            if (SelectedFolder == null) return;

            string targetModDirPath = Path.Combine(game.ModsDirPath!, $"{mod.DirectoryName!}_Translated");

            int index = 0;
            while(Directory.Exists(targetModDirPath))
            {
                targetModDirPath = Path.Combine(game.ModsDirPath!, $"{mod.DirectoryName!}_Translated{index++}");
            }

            SaveTranslations(targetModDirPath);
        }

        private void SaveTranslations(string targetModDirPath)
        {
            // target path for languages: targetModLanguagesPath
            string targetModLanguagesPath = Path.Combine(targetModDirPath, "Languages", SelectedFolder == mod!.DirectoryName ? "" : SelectedFolder!);

            var translationsData = EditorHelper.FillTranslationsData(TranslationsTable, targetModLanguagesPath);
            if(translationsData == null)
                return;

            // Для каждого языка и каждого под-пути, записываем файлы соответствующим образом
            bool isAnyFileWrote = EditorHelper.WriteFiles(translationsData, targetModLanguagesPath);

            if (!isAnyFileWrote)
            {
                // no files to write and dont need the mod dir
                Directory.Delete(targetModDirPath, true);
                return;
            }

            // write the About.xml and othe required data to the targetModDirPath

            var modAboutData = new ModAboutData();
            modAboutData.Name = mod.About?.Name;
            modAboutData.PackageId = mod.About?.PackageId;
            modAboutData.Author = mod.About?.Author;
            modAboutData.ModVersion = "1";
            modAboutData.SupportedVersions = mod.About?.SupportedVersions != null ? string.Join(",", mod.About ?.SupportedVersions!) : "";
            
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
                   && TranslationsTable.Columns.Count > 0
                   && !TranslationsTable.Columns.Contains(NewLanguageName);
        }
        #endregion
    }
}