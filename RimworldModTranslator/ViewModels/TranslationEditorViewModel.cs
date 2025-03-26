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

namespace RimworldModTranslator.ViewModels
{
    public partial class TranslationEditorViewModel(SettingsService settingsService) : ViewModelBase
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

        public string Header { get; } = "Editor";
        public string[] TransatableLanguageDirs { get; } = [ "DefInjected", "Keyed", "Strings" ];
        public string[] ExtractableModSubDirs { get; } = [ "Defs", "Languages" ];

        private readonly Regex VersionDirRegex = new(@"[0-9]+\.[0-9]+", RegexOptions.Compiled);

        private ModData? mod;
        private Game? game;
        private readonly SettingsService settingsService = settingsService;

        public string? ModDisplayingName { get => settingsService.SelectedMod?.ModDisplayingName; }

        public ObservableCollection<string> Folders { get; } = [];

        [ObservableProperty]
        private string? selectedFolder;
        partial void OnSelectedFolderChanged(string? value)
        {
        }

        [ObservableProperty]
        private string? newLanguageName;

        // enable some editor controls by condition
        public bool IsTranslatorEnabled { get => IsTheTranslatorEnabled(); }
        public bool IsAddNewLanguageEnabled { get => IsAddNewLanguageButtonEnabled(); }
        public bool IsFoldersEnabled { get => IsTheFoldersEnabled(); }

        [ObservableProperty]
        private ObservableCollection<string> languages = new();

        public ObservableCollection<TranslationRow> TranslationRows = new(); 
        
        [ObservableProperty]
        private DataTable _translationsTable = new();

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
                LoadStringsFromTheXmlAsTxtDir(xmlDirName, langDirNames, languagesDirPath);
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

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string key = line;
                        if (!stringByKeyForEachLanguage.TryGetValue(key, out Dictionary<string, string>? value))
                        {
                            value = [];
                            stringByKeyForEachLanguage[key] = value;
                        }

                        value[language] = line;
                    }

                }
            }

            FillTranslationRows(filesDictFull);
        }

        /// <summary>
        /// The variant when the each language xml file
        /// parsing as txt file with xml tags
        /// because of some xml structure was broken and usual pasing fails to read xml
        /// </summary>
        /// <param name="xmlDirName"></param>
        /// <param name="langDirNames"></param>
        /// <param name="languagesDirPath"></param>
        private void LoadStringsFromTheXmlAsTxtDir(string xmlDirName, List<string?> langDirNames, string languagesDirPath)
        {
            // Создаем словарь с вложенной структурой:
            // Dictionary<subPath, Dictionary<key, Dictionary<language, value>>>
            var filesDictFull = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            // Регулярное выражение для поиска строк с xml тегами, которые начинаются и заканчиваются одинаково.
            // Пример: <OvipositorF.stages.5.label>Бездна</OvipositorF.stages.5.label>
            var regex = new Regex(@"^\s*<(?<tag>[^>]+)>(?<value>.*)</\k<tag>>\s*$", RegexOptions.Compiled);

            foreach (var language in langDirNames)
            {
                if (language == null)
                    continue;

                string langPath = Path.Combine(languagesDirPath, language);
                string langXmlDirPath = Path.Combine(langPath, xmlDirName);
                if (!Directory.Exists(langXmlDirPath))
                    continue;

                foreach (var file in Directory.GetFiles(langXmlDirPath, "*.xml", SearchOption.AllDirectories))
                {
                    // Вычисление подкаталога относительно текущей папки языка
                    string xmlSubPath = Path.GetRelativePath(langPath, file);

                    if (!filesDictFull.TryGetValue(xmlSubPath, out Dictionary<string, Dictionary<string, string>>? stringByKeyForEachLanguage))
                    {
                        stringByKeyForEachLanguage = new Dictionary<string, Dictionary<string, string>>();
                        filesDictFull[xmlSubPath] = stringByKeyForEachLanguage;
                    }

                    try
                    {
                        var lines = File.ReadAllLines(file);
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                string key = match.Groups["tag"].Value;
                                string value = match.Groups["value"].Value;

                                if (!stringByKeyForEachLanguage.TryGetValue(key, out Dictionary<string, string>? translations))
                                {
                                    translations = new Dictionary<string, string>();
                                    stringByKeyForEachLanguage[key] = translations;
                                }

                                translations[language] = value;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ошибка чтения файла текстом, логирование при необходимости
                    }
                }
            }

            FillTranslationRows(filesDictFull);
        }

        private void LoadStringsFromTheXmlDir(string xmlDirName, List<string?> langDirNames, string languagesDirPath)
        {
            // Создаем словарь с вложенной структурой:
            // Dictionary<subPath, Dictionary<key, Dictionary<language, value>>>
            var filesDictFull = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            foreach (var language in langDirNames)
            {
                if (language == null)
                    continue;

                string langPath = Path.Combine(languagesDirPath, language);
                string langXmlDirPath = Path.Combine(langPath, xmlDirName);
                if (!Directory.Exists(langXmlDirPath))
                    continue;

                foreach (var file in Directory.GetFiles(langXmlDirPath, "*.xml", SearchOption.AllDirectories))
                {
                    // Вычисление подкаталога относительно текущей папки языка
                    string xmlSubPath = Path.GetRelativePath(langPath, file);

                    if (!filesDictFull.TryGetValue(xmlSubPath, out Dictionary<string, Dictionary<string, string>>? stringByKeyForEachLanguage))
                    {
                        stringByKeyForEachLanguage = [];
                        filesDictFull[xmlSubPath] = stringByKeyForEachLanguage;
                    }

                    try
                    {
                        XDocument doc = XDocument.Load(file);
                        // Получить все переведенные элементы (без вложенных элементов и пустых значений)
                        var pairs = doc.Descendants().Where(e => !e.HasElements && !string.IsNullOrWhiteSpace(e.Value));
                        
                        foreach (var pair in pairs)
                        {
                            string key = pair.Name.LocalName;
                            if (!stringByKeyForEachLanguage.TryGetValue(key, out Dictionary<string, string>? value))
                            {
                                value = [];
                                stringByKeyForEachLanguage[key] = value;
                            }

                            value[language] = pair.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignore xml arse errors
                        // some xml have error parse with error like "{"The 'blabla.labelNoun' start tag on line 17 position 4 does not match the end tag of 'LanguageData'. Line 37, position 3."}"
                        // it happens because translator accidentally removed '<' or '>' symbol in one of starting or closing tag for a string
                        // or translator used gpt and gpt made xml structure broken, but more likely it was made manually
                    }
                }
            }

            FillTranslationRows(filesDictFull);
        }

        private void FillTranslationRows(Dictionary<string, Dictionary<string, Dictionary<string, string>>> filesDictFull)
        {
            // Dictionary<subPath, Dictionary<key, Dictionary<language, value>>> filesDictFull
            foreach (var (subPath, keyValues) in filesDictFull)
            {
                foreach (var (key, langValues) in keyValues)
                {
                    var translationRow = new TranslationRow(subPath);
                    translationRow.SetKey(key);

                    foreach (var (lang, value) in langValues)
                    {
                        translationRow.Translations.Add(new LanguageValueData(lang, value));
                    }

                    TranslationRows.Add(translationRow);
                }
            }
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

            TranslationsTable = translationsTable;
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

            // Simple example: Add a new language (e.g., prompt user in real app)
            string newLang = NewLanguageName!;
            if (string.IsNullOrEmpty(newLang)) return;
            if (TranslationsTable.Columns.Contains(newLang)) return;

            TranslationsTable.Columns.Add(newLang, typeof(string));
        }

        [RelayCommand]
        private void SaveLanguages()
        {
            if (game == null) return;
            if (mod == null) return;

            //string langDir = Path.Combine(mod.DirectoryName, SelectedFolder, "Languages");
            //Directory.CreateDirectory(langDir);

            //foreach (var lang in Languages)
            //{
            //    string langPath = Path.Combine(langDir, lang);
            //    Directory.CreateDirectory(langPath);

            //    // Simple save: one file per language (adjust structure as needed)
            //    var doc = new XDocument(new XElement("LanguageData"));
            //    foreach (var row in TranslationRows)
            //    {
            //        if (!string.IsNullOrEmpty(row.Translations[lang]))
            //        {
            //            doc.Root.Add(new XElement(row.Key, row.Translations[lang]));
            //        }
            //    }
            //    doc.Save(Path.Combine(langPath, "Translations.xml"));
            //}
        }
    }
}