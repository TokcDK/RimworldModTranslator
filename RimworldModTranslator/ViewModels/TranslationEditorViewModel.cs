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

namespace RimworldModTranslator.ViewModels
{
    public partial class TranslationEditorViewModel : ViewModelBase
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
        public string[] ExtractableModSubDirs { get; } = [ "Defs", "Language" ];

        private readonly Regex VersionDirRegex = new(@"[0-9]+\.[0-9]+", RegexOptions.Compiled);

        private ModData? mod;
        private Game? game;
        private readonly SettingsService settingsService;

        public string? ModDisplayingName { get => settingsService.SelectedMod?.ModDisplayingName; }

        [ObservableProperty]
        private ObservableCollection<string> folders = new();

        [ObservableProperty]
        private string? selectedFolder;

        [ObservableProperty]
        private string? newLanguageName;

        [ObservableProperty]
        private ObservableCollection<string> languages = new();

        [ObservableProperty]
        private ObservableCollection<TranslationRow> translationRows = new();

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

        public TranslationEditorViewModel(SettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        private void GetTranslatableFolders()
        {
            string fullPath = Path.Combine(game!.GameDirPath!, "Mods", mod!.DirectoryName!);
            if (!Directory.Exists(fullPath)) return;

            Folders = GetTranslatableSubDirs(fullPath);

            if(HasExtractableStringsDir(fullPath))
            {
                Folders.Add(mod!.DirectoryName!);
            }

            if(Folders.Count > 0)
            {
                SelectedFolder = Folders[0];
            }
        }

        private ObservableCollection<string> GetTranslatableSubDirs(string fullPath)
        {
            return [..Directory.GetDirectories(fullPath)
                        .Select(Path.GetFileName)
                        .Where(d => d != null
                            && VersionDirRegex.IsMatch(d)
                            && HasExtractableStringsDir(d)
                        )
                   ];
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
            TranslationRows.Clear();

            string languagesDirPath = Path.Combine(game!.GameDirPath!, "Mods", mod!.DirectoryName!, VersionDirRegex.IsMatch(SelectedFolder) ? SelectedFolder : "", "Languages");
            if (!Directory.Exists(languagesDirPath)) return;

            var langDirNames = Directory.GetDirectories(languagesDirPath).Where(d => HaveTranslatableDirs(d)).Select(Path.GetFileName).ToList();
            foreach (var langDirName in langDirNames)
            {
                if(langDirName == null) continue;

                Languages.Add(langDirName);
            }

            var xmlDirNames = new string[2] { "DefInjected", "Keyed" };
            foreach (var xmlDirName in xmlDirNames)
            {
                LoadStringsFromTheXmlDir(xmlDirName, langDirNames, languagesDirPath);
            }

            int aaai = 0;
        }

        private void LoadStringsFromTheXmlDir(string xmlDirName, List<string?> langDirNames, string languagesDirPath)
        {
            // Aggregate all translation keys across languages
            var allKeys = new HashSet<string>();
            var translations = new Dictionary<string, Dictionary<string, string>>();

            foreach (var languageDirName in langDirNames)
            {
                string langPath = Path.Combine(languagesDirPath, languageDirName!, xmlDirName);
                foreach (var file in Directory.GetFiles(langPath, "*.xml", SearchOption.AllDirectories))
                {
                    try
                    {
                        XDocument doc = XDocument.Load(file);
                        var pairs = doc.Descendants().Where(e => e.HasElements == false && !string.IsNullOrWhiteSpace(e.Value));
                        foreach (var pair in pairs)
                        {
                            string key = pair.Name.LocalName;
                            allKeys.Add(key);
                            if (!translations.ContainsKey(key))
                                translations[key] = new Dictionary<string, string>();
                            translations[key][languageDirName] = pair.Value;
                        }
                    }
                    catch { /* Ignore parsing errors */ }
                }
            }

            foreach (var key in allKeys)
            {
                var row = new TranslationRow { Key = key, XmlDirName = xmlDirName };
                foreach (var lang in Languages)
                {
                    row.Translations[lang] = translations.TryGetValue(key, out var dict) && dict.TryGetValue(lang, out var value) ? value : string.Empty;
                }
                TranslationRows.Add(row);
            }
        }

        private bool HaveTranslatableDirs(string languageDir)
        {
            return TransatableLanguageDirs.Any(d => Directory.Exists(Path.Combine(languageDir, d)));
        }

        [RelayCommand]
        private void LoadStrings()
        {
            game = settingsService.SelectedGame;
            if (game == null) return;

            mod = settingsService.SelectedMod;
            if (mod == null) return;

            GetTranslatableFolders();

            if(Folders.Count == 0) return; // no translatable folders

            LoadLanguages();
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
            // Simple example: Add a new language (e.g., prompt user in real app)
            string newLang = $"NewLanguage_{Languages.Count + 1}";
            Languages.Add(newLang);
            foreach (var row in TranslationRows)
            {
                row.Translations[newLang] = string.Empty;
            }
        }

        [RelayCommand]
        private void SaveLanguages()
        {
            string langDir = Path.Combine(mod.DirectoryName, SelectedFolder, "Languages");
            Directory.CreateDirectory(langDir);

            foreach (var lang in Languages)
            {
                string langPath = Path.Combine(langDir, lang);
                Directory.CreateDirectory(langPath);

                // Simple save: one file per language (adjust structure as needed)
                var doc = new XDocument(new XElement("LanguageData"));
                foreach (var row in TranslationRows)
                {
                    if (!string.IsNullOrEmpty(row.Translations[lang]))
                    {
                        doc.Root.Add(new XElement(row.Key, row.Translations[lang]));
                    }
                }
                doc.Save(Path.Combine(langPath, "Translations.xml"));
            }
        }
    }
}