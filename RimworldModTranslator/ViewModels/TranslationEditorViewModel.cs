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

namespace RimworldModTranslator.ViewModels
{
    public partial class TranslationEditorViewModel : ViewModelBase
    {
        public TranslationEditorViewModel(SettingsService settingsService)
        {
            this.settingsService = settingsService;
            Folders.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsFoldersEnabled));

            InitTranslationsTable();
        }

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

        public string Header { get; } = "Editor";

        private ModData? mod;
        private Game? game;
        private readonly SettingsService settingsService;


        // enable some editor controls by condition
        public bool IsTranslatorEnabled { get => IsTheTranslatorEnabled(); }
        public bool IsAddNewLanguageEnabled { get => IsAddNewLanguageButtonEnabled(); }
        public bool IsFoldersEnabled { get => IsTheFoldersEnabled(); }

        public string? ModDisplayingName { get => settingsService.SelectedMod?.ModDisplayingName; }

        public ObservableCollection<string> Folders { get; } = [];

        [ObservableProperty]
        private string? selectedFolder;
        partial void OnSelectedFolderChanged(string? value)
        {
        }

        [ObservableProperty]
        private string? newLanguageName;
        partial void OnNewLanguageNameChanged(string? value)
        {
            OnPropertyChanged(nameof(IsAddNewLanguageEnabled));
        }

        [ObservableProperty]
        private ObservableCollection<string> languages = new();

        public ObservableCollection<TranslationRow> TranslationRows = new(); 
        
        [ObservableProperty]
        private DataTable? translationsTable;

        [ObservableProperty]
        private DataView? translationsView;

        public ObservableCollection<string> DefsXmlTags { get; } =
        [
            "adjective",
            "baseDesc",
            "baseInspectLine",
            "commandDesc",
            "commandLabel",
            "customLabel",
            "customLetterLabel",
            "customLetterText",
            "deathMessage",
            "desc",
            "description",
            "headerTip",
            "ideoName",
            "ingestCommandString",
            "ingestReportString",
            "jobString",
            "label",
            "labelNoun",
            "labelPlural",
            "leaderTitle",
            "letterText",
            "member",
            "name",
            "outOfFuelMessage",
            "pawnSingular",
            "pawnsPlural",
            "reportString",
            "slateRef",
            "structureLabel",
            "stuffAdjective",
            "summary",
            "text",
            "theme",
            "title",
            "titleshort",
            "titleFemale",
            "titleshortFemale",
            "verb"
        ];

        /// <summary>
        /// Init Translations table and view
        /// </summary>
        /// <param name="fullInit">when false, will be recreated only DataView. TranslationsTable will not be recreated.</param>
        private void InitTranslationsTable(bool fullInit = true, DataTable? dataTableToRelink = null)
        {
            if(fullInit)
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

        private void GetTranslatableFolders()
        {
            string fullPath = Path.Combine(game!.ModsDirPath!, mod!.DirectoryName!);
            if (!Directory.Exists(fullPath)) return;

            Folders.Clear();

             EditorHelper.GetTranslatableSubDirs(fullPath, Folders);

            if(EditorHelper.HasExtractableStringsDir(fullPath))
            {
                Folders.Add(mod!.DirectoryName!);
            }
        }

        /// <summary>
        /// load strings from Languages dirs for each language dir
        /// </summary>
        [RelayCommand]
        private void LoadLanguages()
        {
            if(SelectedFolder == null) return;

            Languages.Clear();

            string languagesDirPath = Path.Combine(game!.ModsDirPath!, mod!.DirectoryName!, EditorHelper.GetLanguageFolderIfNeed(SelectedFolder), "Languages");
            if (!Directory.Exists(languagesDirPath)) return;

            var langDirNames = Directory.GetDirectories(languagesDirPath).Where(d => EditorHelper.HaveTranslatableDirs(d)).Select(Path.GetFileName).ToList();
            foreach (var langDirName in langDirNames)
            {
                if(langDirName == null) continue;

                Languages.Add(langDirName);
            }

            TranslationRows.Clear();

            var xmlDirNames = new string[2] { "DefInjected", "Keyed" };
            foreach (var xmlDirName in xmlDirNames)
            {
                //LoadStringsFromTheXmlDir(xmlDirName, langDirNames, languagesDirPath);
                EditorHelper.LoadStringsFromTheXmlAsTxtDir(xmlDirName, langDirNames, languagesDirPath, TranslationRows);
            }

            EditorHelper.LoadStringsFromStringsDir(langDirNames, languagesDirPath, TranslationRows);
        }

        [RelayCommand]
        private void LoadStrings()
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

            if(isChangedMod || Folders.Count == 0)
            {
                GetTranslatableFolders();
            }

            if(Folders.Count == 0) return; // no translatable folders

            SelectedFolder ??= Folders[0];

            LoadLanguages();

            var translationsTable = EditorHelper.CreateTranslationsTable(TranslationRows);
            InitTranslationsTable(dataTableToRelink: translationsTable);
        }

        [RelayCommand]
        private void SaveStrings()
        {
            if (game == null) return;
            if (mod == null) return;
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

            string langDir = Path.Combine(game.ModsDirPath!, mod.DirectoryName!, SelectedFolder, "Languages");
            Directory.CreateDirectory(langDir);  
        }
    }
}