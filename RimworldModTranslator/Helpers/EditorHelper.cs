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
using static RimworldModTranslator.ViewModels.TranslationEditorViewModel;

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
        /// Variant: Each language xml file is parsed as txt file with xml tags
        /// because some xml structures are broken and usual parsing fails.
        /// Refactored to fill stringsData directly.
        /// </summary>
        public static void LoadStringsFromTheXmlAsTxtDir(string xmlDirName, List<string?> langDirNames, string languagesDirPath, EditorStringsData stringsData)
        {
            // Регулярное выражение для поиска строк с xml тегами, которые начинаются и заканчиваются одинаково.
            // Пример: <OvipositorF.stages.5.label>Бездна</OvipositorF.stages.5.label>
            var regex = new Regex(@"^\s*<(?<tag>[^>]+)>(?<value>.*)</\k<tag>>\s*$", RegexOptions.Compiled);

            foreach (var language in langDirNames)
            {
                if (language == null)
                    continue;

                stringsData.Languages.Add(language);

                string langPath = Path.Combine(languagesDirPath, language);
                string langXmlDirPath = Path.Combine(langPath, xmlDirName);
                if (!Directory.Exists(langXmlDirPath))
                    continue;

                foreach (var file in Directory.GetFiles(langXmlDirPath, "*.xml", SearchOption.AllDirectories))
                {
                    // Вычисляем подкаталог относительно текущей папки языка
                    string xmlSubPath = Path.GetRelativePath(langPath, file);
                    // Получаем или создаем список для данного xmlSubPath
                    if (!stringsData.SubPathStringIdsList.TryGetValue(xmlSubPath, out StringsIdsBySubPath? stringIdsList))
                    {
                        stringIdsList = new();
                        stringsData.SubPathStringIdsList[xmlSubPath] = stringIdsList;
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

                                if (!stringIdsList.StringIdLanguageValuePairsList.TryGetValue(key, out LanguageValuePairsData? langList))
                                {
                                    langList = new();
                                    stringIdsList.StringIdLanguageValuePairsList[key] = langList;
                                }
                                langList.LanguageValuePairs[language] = value;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ошибка чтения файла как текстового, можно добавить логирование при необходимости
                    }
                }
            }
        }

        public static void LoadStringsFromTheXmlDir(string xmlDirName, ObservableCollection<string?> langDirNames, string languagesDirPath, EditorStringsData stringsData)
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
                        stringByKeyForEachLanguage = new Dictionary<string, Dictionary<string, string>>();
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
                                value = new Dictionary<string, string>();
                                stringByKeyForEachLanguage[key] = value;
                            }

                            value[language] = pair.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignore xml parse errors
                    }
                }
            }
        }

        public static void LoadStringsFromStringsDir(List<string?> langDirNames, string languagesDirPath, EditorStringsData stringsData)
        {
            foreach (var language in langDirNames)
            {
                if (language == null)
                    continue;

                stringsData.Languages.Add(language);

                string langPath = Path.Combine(languagesDirPath, language);
                string langTxtDirPath = Path.Combine(langPath, "Strings");
                if (!Directory.Exists(langTxtDirPath))
                    continue;

                foreach (var file in Directory.GetFiles(langTxtDirPath, "*.txt", SearchOption.AllDirectories))
                {
                    string txtSubPath = Path.GetRelativePath(langPath, file);
                    // Получить или создать список для данного txtSubPath
                    if (!stringsData.SubPathStringIdsList.TryGetValue(txtSubPath, out StringsIdsBySubPath? stringIdsList))
                    {
                        stringIdsList = new();
                        stringsData.SubPathStringIdsList[txtSubPath] = stringIdsList;
                    }

                    var lines = File.ReadAllLines(file);
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    int idIndex = 0;
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        string key = $"{fileName}.{idIndex++}";
                        if (!stringIdsList.StringIdLanguageValuePairsList.TryGetValue(key, out LanguageValuePairsData? langList))
                        {
                            langList = new();
                            stringIdsList.StringIdLanguageValuePairsList[key] = langList;
                        }

                        langList.LanguageValuePairs[language] = line;
                    }
                }
            }
        }

        internal static string GetLanguageFolderIfNeed(string selectedFolder)
        {
            return VersionDirRegex.IsMatch(selectedFolder) ? selectedFolder : "";
        }

        public static DataTable? CreateTranslationsTable(EditorStringsData stringsData)
        {
            if (stringsData.SubPathStringIdsList.Count == 0) return null;

            var translationsTable = new DataTable();

            translationsTable.Columns.Add("SubPath", typeof(string));
            translationsTable.Columns.Add("ID", typeof(string));

            var languageSet = GetUniqueLanguages(stringsData);

            // Add column for each language
            foreach (var lang in languageSet)
            {
                translationsTable.Columns.Add(lang, typeof(string));
            }

            // fill DataTable
            foreach (var subPathStringIds in stringsData.SubPathStringIdsList)
            {
                string? subPath = subPathStringIds.Key;
                var stringIdsLanguageValuePairsList = subPathStringIds.Value.StringIdLanguageValuePairsList;

                foreach (var stringIdsLanguageValuePairs in stringIdsLanguageValuePairsList)
                {
                    string? stringId = stringIdsLanguageValuePairs.Key;

                    var dataRow = translationsTable.NewRow();
                    dataRow["SubPath"] = subPath ?? string.Empty;
                    dataRow["ID"] = stringId ?? string.Empty;

                    // Заполняем языковые колонки
                    foreach (var langValuePair in stringIdsLanguageValuePairs.Value.LanguageValuePairs)
                    {
                        string lang = langValuePair.Key;
                        string? languageValue = langValuePair.Value;

                        dataRow[lang] = languageValue;
                    }

                    translationsTable.Rows.Add(dataRow);
                }
            }

            return translationsTable;
        }

        private static HashSet<string> GetUniqueLanguages(EditorStringsData stringsData)
        {
            return stringsData.Languages;
        }

        public static bool LoadLanguages(string selectedLanguageDir, EditorStringsData stringsData)
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
                EditorHelper.LoadStringsFromTheXmlAsTxtDir(xmlDirName, langDirNames, languagesDirPath, stringsData);
            }

            EditorHelper.LoadStringsFromStringsDir(langDirNames, languagesDirPath, stringsData);

            return stringsData.SubPathStringIdsList.Count > 0;
        }

        public static void ExtractStrings(string selectedLanguageDir, EditorStringsData stringsData)
        {
            var defsDir = Path.Combine(selectedLanguageDir, "Defs");
            if (!Directory.Exists(defsDir)) return;

            string language = Properties.Settings.Default.ExtractedStringsLanguageFolderName;

            stringsData.Languages.Add(language);

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

                        var matchingElements = category.Descendants()
                                                       .Where(e => defsXmlTags.Contains(e.Name.LocalName));
                        foreach (var element in matchingElements)
                        {
                            var ancestors = element.Ancestors().TakeWhile(e => e != category).Reverse().ToList();
                            var segments = new List<string>();

                            foreach (var anc in ancestors)
                            {
                                if (anc.Name.LocalName == "li")
                                {
                                    var liSiblings = anc.Parent!.Elements("li").ToList();
                                    int index = liSiblings.IndexOf(anc);
                                    segments.Add(index.ToString());
                                }
                                else
                                {
                                    segments.Add(anc.Name.LocalName);
                                }
                            }
                            segments.Add(element.Name.LocalName);


                            string subPath = $"DefInjected\\{folderName}\\{xmlFileName}";
                            string stringId = stringIdRootName + "." + string.Join(".", segments);
                            string stringValue = element.Value.Trim();

                            if (!stringsData.SubPathStringIdsList.TryGetValue(subPath, out StringsIdsBySubPath? stringsBySubPath))
                            {
                                stringsBySubPath = new();
                                stringsData.SubPathStringIdsList[subPath] = stringsBySubPath;
                            }
                            if (!stringsBySubPath.StringIdLanguageValuePairsList.TryGetValue(stringId, out LanguageValuePairsData? langList))
                            {
                                langList = new LanguageValuePairsData();
                                stringsBySubPath.StringIdLanguageValuePairsList[stringId] = langList;
                            }
                            langList.LanguageValuePairs[language] = stringValue;
                        }
                    }
                }
                catch (Exception)
                {
                    // При необходимости можно добавить логирование ошибки
                }
            }
        }

        public static Dictionary<string, Dictionary<string, Dictionary<string, string>>>? FillTranslationsData(DataTable? translationsTable, string targetModLanguagesPath)
        {
            // Структура: Dictionary<LanguageName, Dictionary<SubPath, Dictionary<StringId, StringValue>>>
            var translationsData = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            // Предполагаем, что в TranslationsTable есть столбцы "SubPath" и "StringId",
            // а остальные столбцы отвечают за языки.
            if (translationsTable == null)
                return null;

            foreach (DataRow row in translationsTable.Rows)
            {
                string subPath = row["SubPath"]?.ToString() ?? "";
                string stringId = row["ID"]?.ToString() ?? "";

                foreach (DataColumn column in translationsTable.Columns)
                {
                    if (column.ColumnName == "SubPath" || column.ColumnName == "ID")
                        continue;

                    string stringValue = row[column]?.ToString() ?? "";
                    if (string.IsNullOrEmpty(stringValue)) continue; // skip empty strings

                    string languageName = column.ColumnName;

                    if (!translationsData.TryGetValue(languageName, out var files))
                    {
                        files = [];
                        translationsData[languageName] = files;
                    }
                    if (!files.TryGetValue(subPath, out var strings))
                    {
                        strings = [];
                        files[subPath] = strings;
                    }

                    strings[stringId] = stringValue;
                }
            }

            return translationsData;
        }

        public static bool WriteFiles(Dictionary<string, Dictionary<string, Dictionary<string, string>>> translationsData, string targetModLanguagesPath)
        {
            bool isAnyWrote = false;
            foreach (var languagePair in translationsData)
            {
                string languageName = languagePair.Key;
                string languageFolderPath = Path.Combine(targetModLanguagesPath, languageName);

                foreach (var subPathPair in languagePair.Value)
                {
                    string subPath = subPathPair.Key;
                    string filePath = Path.Combine(languageFolderPath, subPath);
                    string extension = Path.GetExtension(filePath).ToLowerInvariant();

                    // Создать директорию, если не существует
                    string? fileDirectory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    string content = "";

                    if (extension == ".txt")
                    {
                        // Записываем только значения каждой строки, по одному значению в строке
                        content = string.Join(Environment.NewLine, subPathPair.Value.Values);
                    }
                    else if (extension == ".xml")
                    {
                        // Создаем XML по заданному шаблону
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                        sb.AppendLine("<LanguageData>");
                        foreach (var kvp in subPathPair.Value)
                        {
                            string id = kvp.Key;
                            string value = kvp.Value;
                            sb.AppendLine($"  <{id}>{value}</{id}>");
                        }
                        sb.AppendLine("</LanguageData>");
                        content = sb.ToString();
                    }


                    // Записываем контент в файл
                    File.WriteAllText(filePath, content);
                    isAnyWrote = true;
                }
            }

            return isAnyWrote;
        }

        internal static void WriteAbout(string targetModDirPath, ModAboutData modAboutData)
        {
            WriteAboutXml(targetModDirPath, modAboutData);

            // Copy Preview.png
            if (!string.IsNullOrWhiteSpace(modAboutData.Preview) && File.Exists(Path.GetFullPath(modAboutData.Preview)))
            {
                var previewPath = Path.Combine(targetModDirPath, "About", "Preview.png");
                File.Copy(modAboutData.Preview, previewPath, true);
            }
        }

        private static void WriteAboutXml(string targetModDirPath, ModAboutData modAboutData)
        {
            var aboutXmlPath = Path.Combine(targetModDirPath, "About", "About.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(aboutXmlPath)!);

            // Обработка SupportedVersions - разбить по запятой и добавить li элементы
            var supportedVersionsList = new List<XElement>();
            if (!string.IsNullOrWhiteSpace(modAboutData.SupportedVersions))
            {
                var versions = modAboutData.SupportedVersions.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries);
                foreach (var version in versions)
                {
                    supportedVersionsList.Add(new XElement("li", version.Trim()));
                }
            }

            // Значение для loadAfter, используя SourceMod.About.PackageId, если оно доступно
            string loadAfterPackageId = modAboutData.SourceMod?.About?.PackageId ?? string.Empty;

            var modMetaData = new XElement("ModMetaData",
                new XElement("name", modAboutData.Name ?? string.Empty),
                new XElement("author", modAboutData.Author ?? string.Empty),
                new XElement("url", modAboutData.Url ?? string.Empty),
                new XElement("packageId", modAboutData.PackageId ?? string.Empty),
                new XElement("supportedVersions", supportedVersionsList),
                new XElement("modDependencies"),
                new XElement("loadAfter", new XElement("li", loadAfterPackageId)),
                new XElement("description", modAboutData.Description ?? string.Empty)
            );

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), modMetaData);
            doc.Save(aboutXmlPath);
        }
    }
}
