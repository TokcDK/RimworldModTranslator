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
        public string[] TransatableLanguageDirs { get; } = [ "DefInjected", "Keyed", "Strings" ];
        public string[] ExtractableModSubDirs { get; } = [ "Defs", "Languages" ];

        private readonly Regex VersionDirRegex = new(@"[0-9]+\.[0-9]+", RegexOptions.Compiled);

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

            GetTranslatableSubDirs(fullPath);

            if(HasExtractableStringsDir(fullPath))
            {
                Folders.Add(mod!.DirectoryName!);
            }
        }

        private void GetTranslatableSubDirs(string fullPath)
        {
            foreach(var folder in Directory.GetDirectories(fullPath)
                        .Select(Path.GetFileName)
                        .Where(d => d != null
                            && VersionDirRegex.IsMatch(d)
                            && HasExtractableStringsDir(Path.Combine(fullPath, d))
                        ))
            {
                Folders.Add(folder!);
            }
        }

        private bool HasExtractableStringsDir(string dir)
        {
            return ExtractableModSubDirs.Any(subdir => Directory.Exists(Path.Combine(dir, subdir)));
        }

        /// <summary>
        /// load strings from Languages dirs for each language dir
        /// </summary>
        [RelayCommand]
        private void LoadLanguages()
        {
            if(SelectedFolder == null) return;

            Languages.Clear();

            string languagesDirPath = Path.Combine(game!.ModsDirPath!, mod!.DirectoryName!, VersionDirRegex.IsMatch(SelectedFolder) ? SelectedFolder : "", "Languages");
            if (!Directory.Exists(languagesDirPath)) return;

            var langDirNames = Directory.GetDirectories(languagesDirPath).Where(d => HaveTranslatableDirs(d)).Select(Path.GetFileName).ToList();
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

            LoadStringsFromStringsDir(langDirNames, languagesDirPath);
        }

        private void LoadStringsFromStringsDir(List<string?> langDirNames, string languagesDirPath)
        {
            var filesDictFull = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            foreach (var language in langDirNames)
            {
                if (language == null)
                    continue;

                string langPath = Path.Combine(languagesDirPath, language);
                string langTxtDirPath = Path.Combine(langPath, "Strings");
                if (!Directory.Exists(langTxtDirPath))
                    continue;

                Dictionary<string, string> strings = [];
                foreach (var file in Directory.GetFiles(langTxtDirPath, "*.txt", SearchOption.AllDirectories))
                {
                    string txtSubPath = Path.GetRelativePath(langPath, file);

                    if (!filesDictFull.TryGetValue(txtSubPath, out Dictionary<string, Dictionary<string, string>>? stringByKeyForEachLanguage))
                    {
                        stringByKeyForEachLanguage = [];
                        filesDictFull[txtSubPath] = stringByKeyForEachLanguage;
                    }

                    var lines = File.ReadAllLines(file);

                    var fileName = Path.GetFileNameWithoutExtension(file);
                    int idIndex = 0;
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string key = $"{fileName}.{idIndex++}";
                        if (!stringByKeyForEachLanguage.TryGetValue(key, out Dictionary<string, string>? value))
                        {
                            value = [];
                            stringByKeyForEachLanguage[key] = value;
                        }

                        value[language] = line;
                    }

                }
            }

            EditorHelper.FillTranslationRows(filesDictFull, TranslationRows);
        }

        private void CreateTranslationsTable()
        {
            if (TranslationRows.Count == 0) return;

            // Создаем новый DataTable
            var translationsTable = new DataTable();

            // Добавляем первые две колонки: SubPath и ID
            translationsTable.Columns.Add("SubPath", typeof(string));
            translationsTable.Columns.Add("ID", typeof(string));

            // Собираем все уникальные языки из TranslationRows
            var languageSet = new HashSet<string>();
            foreach (var row in TranslationRows)
            {
                foreach (var langValue in row.Translations)
                {
                    if (string.IsNullOrEmpty(langValue.Language)) continue;
                    if (languageSet.Contains(langValue.Language)) continue;

                    languageSet.Add(langValue.Language!);
                }
            }

            // Добавляем колонки для каждого языка
            foreach (var lang in languageSet)
            {
                translationsTable.Columns.Add(lang, typeof(string));
            }

            // Заполняем строки DataTable
            foreach (var translationRow in TranslationRows)
            {
                var dataRow = translationsTable.NewRow();
                dataRow["SubPath"] = translationRow.SubPath ?? string.Empty;
                dataRow["ID"] = translationRow.Key ?? string.Empty;

                // Заполняем языковые колонки
                foreach (var lang in languageSet)
                {
                    // Находим значение для данного языка
                    var languageValue = translationRow.Translations.FirstOrDefault(t => t.Language == lang);
                    dataRow[lang] = languageValue?.Value;
                }

                translationsTable.Rows.Add(dataRow);
            }

            InitTranslationsTable(dataTableToRelink: translationsTable);
        }

        private bool HaveTranslatableDirs(string languageDir)
        {
            return TransatableLanguageDirs.Any(d => Directory.Exists(Path.Combine(languageDir, d)));
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

            CreateTranslationsTable();
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