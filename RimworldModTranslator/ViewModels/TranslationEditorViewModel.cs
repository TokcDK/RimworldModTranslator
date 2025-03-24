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

namespace RimworldModTranslator.ViewModels
{
    public partial class TranslationEditorViewModel(SettingsService settingsService) : ViewModelBase
    {
        private readonly ModData? mod;

        [ObservableProperty]
        private string selectedFolder;

        [ObservableProperty]
        private ObservableCollection<string> languages = new();

        [ObservableProperty]
        private ObservableCollection<TranslationRow> translationRows = new();

        private string GetLatestVersionFolder(string modDirectory)
        {
            string fullPath = Path.Combine(modDirectory, "Languages");
            if (!Directory.Exists(fullPath)) return modDirectory;

            var versionDirs = Directory.GetDirectories(modDirectory)
                .Select(Path.GetFileName)
                .Where(d => d != null && d.All(c => char.IsDigit(c) || c == '.'))
                .OrderByDescending(v => v, Comparer<string?>.Create(static (a, b) => Version.Parse(a).CompareTo(Version.Parse(b))))
                .FirstOrDefault();
            return versionDirs ?? modDirectory;
        }

        [RelayCommand]
        private void LoadLanguages()
        {
            Languages.Clear();
            TranslationRows.Clear();

            string langDir = Path.Combine(mod.DirectoryName, SelectedFolder, "Languages");
            if (!Directory.Exists(langDir)) return;

            var langFolders = Directory.GetDirectories(langDir).Select(Path.GetFileName).ToList();
            foreach (var lang in langFolders)
            {
                if(lang == null) continue;

                Languages.Add(lang);
            }

            // Aggregate all translation keys across languages
            var allKeys = new HashSet<string>();
            var translations = new Dictionary<string, Dictionary<string, string>>();

            foreach (var lang in langFolders)
            {
                string langPath = Path.Combine(langDir, lang);
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
                            translations[key][lang] = pair.Value;
                        }
                    }
                    catch { /* Ignore parsing errors */ }
                }
            }

            foreach (var key in allKeys)
            {
                var row = new TranslationRow { Key = key };
                foreach (var lang in Languages)
                {
                    row.Translations[lang] = translations.TryGetValue(key, out var dict) && dict.TryGetValue(lang, out var value) ? value : string.Empty;
                }
                TranslationRows.Add(row);
            }
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