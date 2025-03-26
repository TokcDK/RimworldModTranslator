using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

        public static List<string> DefsXmlTags { get; } =
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

        public static void GetTranslatableFolders(ObservableCollection<string> folders, string modsPath, string modName)
        {
            string fullPath = Path.Combine(modsPath, modName);
            if (!Directory.Exists(fullPath)) return;

            folders.Clear();

            EditorHelper.GetTranslatableSubDirs(fullPath, folders);

            if (EditorHelper.HasExtractableStringsDir(fullPath))
            {
                folders.Add(modName);
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

        public static DataTable? CreateTranslationsTable(List<TranslationRow> translationRows)
        {
            if (translationRows.Count == 0) return null;

            // Создаем новый DataTable
            var translationsTable = new DataTable();

            // Добавляем первые две колонки: SubPath и ID
            translationsTable.Columns.Add("SubPath", typeof(string));
            translationsTable.Columns.Add("ID", typeof(string));

            // Собираем все уникальные языки из TranslationRows
            var languageSet = new HashSet<string>();
            foreach (var row in translationRows)
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
            foreach (var translationRow in translationRows)
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

            return translationsTable;
        }

        public static bool LoadLanguages(List<TranslationRow> translationRows, string selectedLanguageDir)
        {
            List<string> languages = [];

            string languagesDirPath = Path.Combine(selectedLanguageDir, "Languages");
            if (!Directory.Exists(languagesDirPath)) return false;

            var langDirNames = Directory.GetDirectories(languagesDirPath)
                                        .Where(d => EditorHelper.HaveTranslatableDirs(d))
                                        .Select(Path.GetFileName)
                                        .ToList();
            foreach (var langDirName in langDirNames)
            {
                if (langDirName == null) continue;

                languages.Add(langDirName);
            }

            var xmlDirNames = new string[2] { "DefInjected", "Keyed" };
            foreach (var xmlDirName in xmlDirNames)
            {
                //LoadStringsFromTheXmlDir(xmlDirName, langDirNames, languagesDirPath);
                EditorHelper.LoadStringsFromTheXmlAsTxtDir(xmlDirName, langDirNames, languagesDirPath, translationRows);
            }

            EditorHelper.LoadStringsFromStringsDir(langDirNames, languagesDirPath, translationRows);

            return translationRows.Count > 0;
        }

        public static void ExtractStrings(List<TranslationRow> translationRows, string selectedLanguageDir)
        {
            var defsDir = Path.Combine(selectedLanguageDir, "Defs");
            if (!Directory.Exists(defsDir)) return;

            var defsXmlTags = EditorHelper.DefsXmlTags;

            foreach (var xmlFile in Directory.GetFiles(defsDir, "*.xml", SearchOption.AllDirectories))
            {
                var xmlFileName = Path.GetFileName(xmlFile);

                try
                {
                    var doc = XDocument.Load(xmlFile);
                    var root = doc.Element("Defs");
                    if (root == null) continue;

                    foreach (var category in root.Elements())
                    {
                        string folderName = category.Name.LocalName;

                        var defNameElement = category.Element("defName");
                        if (defNameElement == null) continue;

                        string stringIdRootName = defNameElement.Value.Trim();

                        // Get chain of ancestors from the current element up to the category element
                        var matchingElements = category.Descendants()
                                                       .Where(e => defsXmlTags.Contains(e.Name.LocalName));
                        foreach (var element in matchingElements)
                        {
                            // Get the chain of ancestors from the current element up to the category element
                            var ancestors = element.Ancestors().TakeWhile(e => e != category).Reverse().ToList();
                            var segments = new List<string>();

                            foreach (var anc in ancestors)
                            {
                                if (anc.Name.LocalName == "li")
                                {
                                    // save index of li element in parent ul/ol
                                    var liSiblings = anc.Parent!.Elements("li").ToList();
                                    int index = liSiblings.IndexOf(anc);
                                    segments.Add(index.ToString());
                                }
                                else
                                {
                                    segments.Add(anc.Name.LocalName);
                                }
                            }
                            // Add the element itself; the tag name is used as the last part of the identifier
                            segments.Add(element.Name.LocalName);

                            string stringId = stringIdRootName + "." + string.Join(".", segments);
                            string stringValue = element.Value.Trim();

                            var translationRow = new TranslationRow(subPath: $"DefInjected\\{folderName}\\{xmlFileName}");
                            translationRow.SetKey(stringId);
                            translationRow.Translations.Add(new LanguageValueData("Extracted", stringValue));

                            translationRows.Add(translationRow);
                        }
                    }
                }
                catch (Exception)
                {
                    // Optionally: exception handling or logging
                }
            }
        }
    }
}
