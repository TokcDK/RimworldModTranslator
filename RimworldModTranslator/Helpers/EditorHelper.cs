using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RimworldModTranslator.Helpers
{
    internal class EditorHelper
    {
        public static string[] TransatableLanguageDirs { get; } = ["DefInjected", "Keyed", "Strings"];
        public static string[] ExtractableModSubDirs { get; } = ["Defs", "Languages"];

        public static readonly Regex VersionDirRegex = new(@"[0-9]+\.[0-9]+", RegexOptions.Compiled);

        public static void GetTranslatableSubDirs(string fullPath, ObservableCollection<string> folders)
        {
            foreach (var folder in Directory.GetDirectories(fullPath)
                        .Select(Path.GetFileName)
                        .Where(d => d != null
                            && VersionDirRegex.IsMatch(d)
                            && HasExtractableStringsDir(Path.Combine(fullPath, d))
                        ))
            {
                folders.Add(folder!);
            }
        }

        public static bool HasExtractableStringsDir(string dir)
        {
            return ExtractableModSubDirs.Any(subdir => Directory.Exists(Path.Combine(dir, subdir)));
        }

        public static bool HaveTranslatableDirs(string languageDir)
        {
            return TransatableLanguageDirs.Any(d => Directory.Exists(Path.Combine(languageDir, d)));
        }

        /// <summary>
        /// The variant when the each language xml file
        /// parsing as txt file with xml tags
        /// because of some xml structure was broken and usual pasing fails to read xml
        /// </summary>
        /// <param name="xmlDirName"></param>
        /// <param name="langDirNames"></param>
        /// <param name="languagesDirPath"></param>
        public static void LoadStringsFromTheXmlAsTxtDir(string xmlDirName, List<string?> langDirNames, string languagesDirPath, System.Collections.ObjectModel.ObservableCollection<TranslationRow> translationRows)
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

            FillTranslationRows(filesDictFull, translationRows);
        }

        public static void LoadStringsFromTheXmlDir(string xmlDirName, List<string?> langDirNames, string languagesDirPath, System.Collections.ObjectModel.ObservableCollection<TranslationRow> translationRows)
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

            FillTranslationRows(filesDictFull, translationRows);
        }

        public static void LoadStringsFromStringsDir(List<string?> langDirNames, string languagesDirPath, System.Collections.ObjectModel.ObservableCollection<TranslationRow> translationRows)
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

            EditorHelper.FillTranslationRows(filesDictFull, translationRows);
        }

        public static void FillTranslationRows(Dictionary<string, Dictionary<string, Dictionary<string, string>>> filesDictFull, System.Collections.ObjectModel.ObservableCollection<TranslationRow> translationRows)
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

                    translationRows.Add(translationRow);
                }
            }
        }

        internal static string GetLanguageFolderIfNeed(string selectedFolder)
        {
            return VersionDirRegex.IsMatch(selectedFolder) ? selectedFolder : "";
        }
    }
}
