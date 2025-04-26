using NLog;
using NLog.Filters;
using RimworldModTranslator.Models;
using RimworldModTranslator.Models.EditorColumns;
using RimworldModTranslator.Models.LanguageXmlReader;
using RimworldModTranslator.Properties;
using RimworldModTranslator.Services;
using RimworldModTranslator.Translations;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Formats.Tar;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using static RimworldModTranslator.ViewModels.TranslationEditorViewModel;

namespace RimworldModTranslator.Helpers
{
    internal partial class EditorHelper
    {
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        public static string[] TransatableLanguageDirs { get; } = [DEFINJECTED_DIR_NAME, KEYED_DIR_NAME, STRINGS_DIR_NAME];
        public static string[] ExtractableModSubDirs { get; } = [DEFS_DIR_NAME, LANGUAGES_DIR_NAME];

        public static readonly Regex VersionDirRegex = new(@"[0-9]+\.[0-9]+$", RegexOptions.Compiled);

        internal static EditorStringsDBCache StringsDBCache = new();

        internal const string LANGUAGES_DIR_NAME = "Languages";
        internal const string STRINGS_DIR_NAME = "Strings";
        internal const string KEYED_DIR_NAME = "Keyed";
        internal const string DEFS_DIR_NAME = "Defs";
        internal const string DEFINJECTED_DIR_NAME = "DefInjected";
        internal const string ROOT_DIR_NAME = "/";
        internal const string ALL_IN_FOLDER_NAME = "*";
        internal const char COMMENT_MAR_CHAR_STRING = ';';

        static readonly HashSet<char> invalidChars = ['\\', '/', ':', '<', '>', '|', '*', '?', '\"'];

        public static FolderColumnData FolderColumnData { get; } = new();
        public static SubPathColumnData SubPathColumnData { get; } = new();
        public static IdColumnData IdColumnData { get; } = new();

        public static IColumnData[] EditorColumns { get; } =
        [
            FolderColumnData,
            SubPathColumnData,
            IdColumnData
        ];

        private static List<string> _defsXmlTags =
            // default values
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
        private static readonly string[] _tagFilePaths =
        [
            Path.Combine("RES", "data", "tags2extract.txt"),
            Path.Combine("RES", "data", "usertags.txt")
        ];
        private static bool _isDefsXmlTagsLoaded = false;
        public static List<string> DefsXmlTags 
        {
            get
            {
                return GetTagsToExtract();
            }
        }

        private static List<string> GetTagsToExtract()
        {
            if (!_isDefsXmlTagsLoaded)
            {
                string defaultPath = Path.Combine("RES", "data", "tags2extract.txt");

                foreach (var path in _tagFilePaths)
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            bool isDefaultPath = string.Equals(path, defaultPath, StringComparison.OrdinalIgnoreCase);
                            var tags = ReadTagsFile(path, !isDefaultPath);
                            if (isDefaultPath)
                            {
                                _defsXmlTags = [.. tags]; // replace default tags from external file
                            }
                            else
                            {
                                _defsXmlTags.AddRange(tags);
                            }
                            Logger.Info(Translation.LoadedTagsFrom0, path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, Translation.ErrorLoadingTagsFrom0, path);
                    }
                }

                _isDefsXmlTagsLoaded = true;
            }

            return _defsXmlTags;
        }

        private static IEnumerable<string> ReadTagsFile(string path, bool onlyNewTags = true)
        {
            return File.ReadAllLines(path)
                .Select(l => l.Split(new[] { COMMENT_MAR_CHAR_STRING }, 2)[0]) // split by ';' comment char and take the first part
                .Select(l => l.Trim()) // trim spaces

                // remove empty lines and optional existing tags
                .Where(l => !string.IsNullOrWhiteSpace(l)
                            && (!onlyNewTags || (onlyNewTags && !_defsXmlTags.Contains(l))));
        }
        internal static void SaveModDB(ObservableCollection<FolderData> folders, ModData? mod)
        {
            if (mod == null || string.IsNullOrWhiteSpace(mod.ParentGame.ModsDirPath) || string.IsNullOrEmpty(mod.DirectoryName) || folders.Count == 0)
            {
                Logger.Debug("Invalid mod or folder data.");
                return;
            }

            string modDirectoryPath = Path.Combine(mod.ParentGame.ModsDirPath, mod.DirectoryName);
            if (!Directory.Exists(modDirectoryPath))
                Directory.CreateDirectory(modDirectoryPath);

            string outputFilePath = Path.Combine(modDirectoryPath, $"RMT.DB.xml");

            using var dataSet = new DataSet(mod.DirectoryName);

            foreach (var folder in folders)
            {
                if (folder.TranslationsTable != null)
                {
                    var dataTable = folder.TranslationsTable.Copy();
                    dataTable.TableName = folder.Name;
                    dataSet.Tables.Add(dataTable);
                }
            }

            dataSet.WriteXml(outputFilePath, XmlWriteMode.WriteSchema);

            Logger.Info(Translation.SaveModFileWasWrote, outputFilePath);
        }

        internal static bool LoadModDB(ObservableCollection<FolderData> folders, ModData? mod, bool forceReplaceTables = false)
        {
            if (mod == null || string.IsNullOrWhiteSpace(mod.ParentGame.ModsDirPath) || string.IsNullOrEmpty(mod.DirectoryName) || folders.Count == 0)
            {
                Logger.Debug("Invalid mod or folder data.");
                return false;
            }

            string modDirectoryPath = Path.Combine(mod.ParentGame.ModsDirPath, mod.DirectoryName);
            string outputFilePath = Path.Combine(modDirectoryPath, $"RMT.DB.xml");

            if (!File.Exists(outputFilePath))
            {
                Logger.Info(Translation.DBFileNotFoundAt0, outputFilePath);
                return false;
            }

            try
            {
                var dataSet = new DataSet();
                dataSet.ReadXml(outputFilePath);

                var folderColumn = FolderColumnData;
                foreach (DataTable table in dataSet.Tables)
                {
                    var folder = folders.FirstOrDefault(f => folders[0].Name == ALL_IN_FOLDER_NAME || f.Name == table.TableName);
                    if (folder != null)
                    {
                        if (forceReplaceTables 
                            && table.Columns.Contains(folderColumn.Name!)) // check folder column back compatibility with old db
                        {
                            folder.TranslationsTable = table.Copy();
                        }
                        else
                        {
                            FillTableValues(table, folder);
                        }
                        Logger.Info(Translation.LoadedDBForFolder0, folder.Name);
                    }
                    else
                    {
                        Logger.Debug("No matching folder found for table: {0} in RMT.DB.xml.", table.TableName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, Translation.ErrorLoadingDBFileFrom0, outputFilePath);
            }

            Logger.Info(Translation.LoadedDBFrom0, outputFilePath);

            return true;
        }

        private static void FillTableValues(DataTable table, FolderData folder)
        {
            if (folder.TranslationsTable == null)
            {
                return; // no table to fill

                // OR.. copy the table structure
                //folder.TranslationsTable = table.Copy();
            }
            else
            {
                // set the values by string ID

                Dictionary<string, DataRow> RowsByID = table.Rows.Cast<DataRow>()
                    .ToDictionary(r => r[IdColumnData.Name!] + "", r => r);

                AddMissingColumns(table, folder);

                foreach (DataRow row in folder.TranslationsTable.Rows)
                {
                    string? subPath = row.Field<string>(SubPathColumnData.Name!);
                    if (string.IsNullOrEmpty(subPath))
                    {
                        continue;
                    }
                    string? id = row.Field<string>(IdColumnData.Name!);
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    if(!RowsByID.TryGetValue(id, out DataRow? foundRow))
                    {
                        continue;
                    }

                    foreach (DataColumn column in table.Columns)
                    {
                        if(IsReadOnlyColumn(column.ColumnName))
                        {
                            continue;
                        }

                        string? value = foundRow.Field<string>(column.ColumnName);
                        if (string.IsNullOrEmpty(value)) // we first check if the value is empty but maybe it will be optional
                        {
                            continue;
                        }
                        row[column.ColumnName] = value;
                    }
                }
            }
        }

        private static void AddMissingColumns(DataTable table, FolderData folder)
        {
            if (folder.TranslationsTable == null) return;

            foreach (DataColumn column in table.Columns)
            {
                if (IsReadOnlyColumn(column.ColumnName))
                {
                    continue;
                }
                if (!folder.TranslationsTable.Columns.Contains(column.ColumnName))
                {
                    DataColumn newColumn = new(column.ColumnName, column.DataType)
                    {
                        Caption = column.Caption
                    };
                    folder.TranslationsTable.Columns.Add(newColumn);
                }
            }
        }

        public static void GetTranslatableSubDirs(string fullPath, IList<FolderData> folders)
        {
            foreach (var folder in Directory.GetDirectories(fullPath)
                        .Select(Path.GetFileName)
                        .Where(d => d != null
                            && VersionDirRegex.IsMatch(d)
                            && HasExtractableStringsDir(Path.Combine(fullPath, d))
                        ))
            {
                folders.Add(new FolderData() { Name = folder! });
            }
        }

        public static void GetTranslatableFolders(IList<FolderData> folders, string modPath)
        {
            if(IsLoadedFromLoadFolders(folders, modPath))
            {
                return;
            }

            EditorHelper.GetTranslatableSubDirs(modPath, folders);

            if (EditorHelper.HasExtractableStringsDir(modPath))
            {
                folders.Add(new FolderData() { Name = ROOT_DIR_NAME });
            }
        }

        private static bool IsLoadedFromLoadFolders(IList<FolderData> folders, string modPath)
        {
            string loadFoldersPath = Path.Combine(modPath, "LoadFolders.xml");
            if (!File.Exists(loadFoldersPath)) return false;

            try
            {
                var loadFoldersDoc = XDocument.Load(loadFoldersPath);

                var supportedVersions = loadFoldersDoc.Element("loadFolders")?.Elements();

                if(supportedVersions == null) return false;

                Dictionary<string, FolderData> folderNames = new();
                foreach (var versionElement in supportedVersions)
                {
                    string versionName = versionElement.Name.LocalName;

                    foreach (var li in versionElement.Elements("li"))
                    {
                        string folderName = li.Value;
                        if (!folderNames.TryGetValue(folderName, out var folderData))
                        {
                            string folderPath = Path.Combine(modPath, EditorHelper.GetTranslatableFolderName(folderName));
                            if (!EditorHelper.HasExtractableStringsDir(folderPath))
                            {
                                continue;
                            }

                            folderData = new FolderData() { Name = folderName };
                            folderNames[folderName] = folderData;
                        }
                        else
                        {
                            if (folderData.SupportedVersions.Contains(versionName))
                            {
                                continue;
                            }
                        }
                        folderData.SupportedVersions.Add(versionName);
                    }
                }

                foreach (var folderData in folderNames.Values)
                {
                    folders.Add(folderData);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static bool HasExtractableStringsDir(string dir)
        {
            return ExtractableModSubDirs.Any(subdir => Directory.Exists(Path.Combine(dir, subdir)));
        }

        public static bool HaveTranslatableDirs(string languageDir)
        {
            return TransatableLanguageDirs.Any(d => Directory.Exists(Path.Combine(languageDir, d)));
        }

        public static async Task LoadStringsCacheInternal(
            ObservableCollection<FolderData> folders,
            ModData? mod,
            SettingsService settingsService
        )
        {
            if (EditorHelper.LoadModDB(folders, mod))
            {
                Logger.Info(Translation.LoadedStringsFromDBFileLogMessage);

                if (!EditorHelper.HaveAnyEmptyLanguageString(folders))
                {
                    return;
                }
            }

            if (settingsService.ForceLoadTranslationsCache || StringsDBCache.IdCache == null || StringsDBCache.ValueCache == null)
            {
                var stringsData = await EditorHelper.LoadAllModsStringsData(settingsService.SelectedGame);

                if (stringsData == null) return;

                (StringsDBCache.IdCache, StringsDBCache.ValueCache) = await EditorHelper.FillCache(stringsData);

                string message = Translation.LoadedStringsCacheFromXLogMessage;
                if (Directory.Exists(settingsService.SelectedGame?.GameDirPath))
                {
                    Logger.Info(message, settingsService.SelectedGame?.GameDirPath);
                }
                Logger.Info(message, settingsService.SelectedGame?.ModsDirPath);
            }

            await EditorHelper.SetTranslationsbyCache(StringsDBCache, folders);
        }

        #region load mod strings
        /// <summary>
        /// Инициализирует данные мода
        /// </summary>
        /// <returns>True, если инициализация прошла успешно, иначе False</returns>
        internal static bool LoadStringsInitModData(ref ModData? mod, SettingsService settingsService)
        {
            bool isChangedMod = mod != settingsService.SelectedMod;

            if (isChangedMod || mod == null)
            {
                // Загружаем только если мод не был установлен или изменился
                mod = settingsService.SelectedMod;
                if (mod == null)
                {
                    Logger.Warn(Translation.ModIsNotSetWarnLogMessage);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Загружает папки переводов для текущего мода
        /// </summary>
        /// <returns>True, если загрузка прошла успешно, иначе False</returns>
        internal static bool LoadModTranslatableFolders(ModData? mod, ObservableCollection<FolderData> folders)
        {
            string modPath = Path.Combine(mod!.ParentGame.ModsDirPath!, mod.DirectoryName!);
            if (!Directory.Exists(modPath))
            {
                Logger.Warn(Translation.ModsPathIsNotSetWarnLogMessage);
                return false;
            }

            folders.Clear();
            EditorHelper.GetTranslatableFolders(folders, modPath);
            return true;
        }

        internal static void LoadStringsForAllFolders(ObservableCollection<FolderData> folders, ModData? mod)
        {
            int totalStringsLoaded = 0;

            foreach (var folder in folders)
            {
                folder.StringsData = LoadStringsForFolder(folder, mod, null);

                totalStringsLoaded += folder.StringsData.loadedStringsCount;

                //folder.TranslationsTable = translationsTable;
            }

            Logger.Info(Translation.LoadedTotal0StringsForAllFoldersLogMessage, totalStringsLoaded);
        }

        /// <summary>
        /// Загружает строки перевода для указанной папки
        /// </summary>
        /// <param name="folder">Папка для загрузки строк</param>
        /// <returns>Количество загруженных строк</returns>
        internal static EditorStringsData LoadStringsForFolder(FolderData folder, ModData? mod, EditorStringsData? stringsData = null)
        {
            string folderName = folder.Name;
            string translatableDir = Path.Combine(mod!.ParentGame.ModsDirPath!, mod.DirectoryName!,
                                                 EditorHelper.GetTranslatableFolderName(folderName));

            stringsData ??= new();

            return EditorHelper.LoadStringsDataFromTheLanguageDir(translatableDir, stringsData);
        }

        public static void LoadStringsFromXmlsAsTxtDir(List<string?> languageNames, string languagesDirPath, EditorStringsData stringsData)
        {
            foreach (var languageName in languageNames)
            {
                if (languageName == null)
                    continue;

                stringsData.Languages.Add(languageName);

                string languageDirPath = Path.Combine(languagesDirPath, languageName);

                if (Directory.Exists(languageDirPath))
                {
                    new DirXmlReader(languageName, languageDirPath, stringsData).ProcessFiles();
                }
                else if (File.Exists(languageDirPath + ".tar"))
                {
                    using var tar = new TarXmlReader(languageName, languageDirPath, stringsData);
                    tar.ProcessFiles();
                }
            }
        }

        // Регулярное выражение для поиска строк с xml тегами, которые начинаются и заканчиваются одинаково.
        // Пример: <OvipositorF.stages.5.label>Бездна</OvipositorF.stages.5.label>
        static Regex regex = new Regex(@"^\s*<(?<tag>[^>]+)>(?<value>.*)</\k<tag>>\s*$", RegexOptions.Compiled);
        internal static int ReadFromTheStringsArray(string[] lines, string language, StringsIdsBySubPath stringIdsList, bool skipMissingIds = false)
        {
            int loadedStringsCount = 0;

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
                        if(skipMissingIds) continue;

                        langList = new();
                        stringIdsList.StringIdLanguageValuePairsList[key] = langList;
                    }
                    langList.LanguageValuePairs[language] = NormalizeNewLines(value);
                    loadedStringsCount++;
                }
            }

            return loadedStringsCount;
        }

        /// <summary>
        /// Replace new line symbols with "\\n" in the string if found.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NormalizeNewLines(string value)
        {
            if (!value.Contains('\n') && !value.Contains('\r'))
                return value;

            return value
                .Replace("\r\n", "\\n")
                .Replace("\r", "\\n")
                .Replace("\n", "\\n");
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

                            value[language] = NormalizeNewLines(pair.Value);
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
                string langTxtDirPath = Path.Combine(langPath, STRINGS_DIR_NAME);
                if (!Directory.Exists(langTxtDirPath))
                    continue;

                if (Directory.Exists(langTxtDirPath))
                {
                    new DirTxtReader(language, langTxtDirPath, stringsData).ProcessFiles();
                }
                else if (File.Exists(langTxtDirPath + ".tar"))
                {
                    using var tar = new TarTxtReader(language, langTxtDirPath, stringsData);
                    tar.ProcessFiles();
                }
            }
        }

        internal static int ReadTxtStringsFile(string[] lines, string fileName, string language, StringsIdsBySubPath stringIdsList)
        {
            int loadedStringsCount = 0;
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
                loadedStringsCount++;
            }

            return loadedStringsCount;
        }

        #endregion

        internal static string GetTranslatableFolderName(string selectedFolder)
        {
            return selectedFolder != ROOT_DIR_NAME ? selectedFolder : "";
        }

        public static DataTable? CreateTranslationsTable(EditorStringsData? stringsData, ObservableCollection<FolderData>? folders = null)
        {
            bool isFoldersEmpty = folders == null || folders.Count == 0;

            if (isFoldersEmpty && stringsData?.loadedStringsCount == 0) return null;

            var translationsTable = new DataTable();

            var nameColumn = new DataColumn(FolderColumnData.Name, typeof(string))
            {
                Caption = FolderColumnData.Caption,
                ReadOnly = FolderColumnData.IsReadOnly
            };
            var subPathColumn = new DataColumn(SubPathColumnData.Name, typeof(string))
            {
                Caption = SubPathColumnData.Caption,
                ReadOnly = SubPathColumnData.IsReadOnly
            };
            var idColumn = new DataColumn(IdColumnData.Name, typeof(string))
            {
                Caption = IdColumnData.Caption,
                ReadOnly = IdColumnData.IsReadOnly
            };

            if (!isFoldersEmpty) translationsTable.Columns.Add(nameColumn);
            translationsTable.Columns.Add(subPathColumn);
            translationsTable.Columns.Add(idColumn);

            var languageSet =
                isFoldersEmpty 
                ? GetUniqueLanguages(stringsData!)
                : folders!.Where(f => IsValidNotAllInFolder(f))
                .SelectMany(f => f.StringsData!.Languages).Distinct();

            // Add column for each language
            foreach (var lang in languageSet)
            {
                translationsTable.Columns.Add(lang, typeof(string));
            }

            foreach (var folder in folders!.Skip(1))
            {
                if (!IsValidNotAllInFolder(folder)) continue;

                stringsData = folder.StringsData;

                // fill DataTable
                foreach (var subPathStringIds in stringsData!.SubPathStringIdsList)
                {
                    string? subPath = subPathStringIds.Key;
                    var stringIdsLanguageValuePairsList = subPathStringIds.Value.StringIdLanguageValuePairsList;

                    foreach (var stringIdsLanguageValuePairs in stringIdsLanguageValuePairsList)
                    {
                        string? stringId = stringIdsLanguageValuePairs.Key;

                        var dataRow = translationsTable.NewRow();
                        dataRow[nameColumn.ColumnName] = folder.Name;
                        dataRow[subPathColumn.ColumnName] = subPath ?? string.Empty;
                        dataRow[idColumn.ColumnName] = stringId ?? string.Empty;

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
            }

            return translationsTable;
        }

        private static bool IsValidNotAllInFolder(FolderData folder)
        {
            return folder != null
                    && folder.StringsData != null
                    && folder.StringsData.loadedStringsCount > 0
                    && folder.Name != ALL_IN_FOLDER_NAME;
        }

        private static HashSet<string> GetUniqueLanguages(EditorStringsData stringsData)
        {
            return stringsData.Languages;
        }

        public static bool LoadDefKeyedStringsFromTheDir(string selectedTranslatableDir, EditorStringsData stringsData, bool loadStringsTxt = true)
        {
            string languagesDirPath = Path.Combine(selectedTranslatableDir, LANGUAGES_DIR_NAME);
            if (!Directory.Exists(languagesDirPath)) return false;

            var langDirNames = Directory.GetDirectories(languagesDirPath)
                                        .Where(d => EditorHelper.HaveTranslatableDirs(d))
                                        .Select(Path.GetFileNameWithoutExtension)
                                        .Concat(GetValidTarFileNames(languagesDirPath))
                                        .ToList();

            EditorHelper.LoadStringsFromXmlsAsTxtDir(langDirNames, languagesDirPath, stringsData);

            if(loadStringsTxt)
            {
                EditorHelper.LoadStringsFromStringsDir(langDirNames, languagesDirPath, stringsData);
            }

            return stringsData.SubPathStringIdsList.Count > 0;
        }

        internal static IEnumerable<string> GetValidTarFileNames(string languagesDirPath)
        {
            foreach (var file in Directory.GetFiles(languagesDirPath, "*.tar"))
            {
                using var tarArchive = TarArchive.Open(file);
                foreach (var entry in tarArchive.Entries)
                {
                    if (entry.IsDirectory && entry.Key != null && entry.Key == "DefInjected/" || entry.Key! == "Keyed/")
                    {
                        yield return Path.GetFileNameWithoutExtension(file);
                        break;
                    }
                }
            }
        }

        public static void ExtractStrings(string selectedLanguageDir, EditorStringsData stringsData)
        {
            var defsDir = Path.Combine(selectedLanguageDir, DEFS_DIR_NAME);
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
                    var root = doc.Element(DEFS_DIR_NAME);
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


                            string subPath = $"{DEFINJECTED_DIR_NAME}\\{folderName}\\{xmlFileName}";
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
                            langList.LanguageValuePairs[language] = NormalizeNewLines(stringValue);
                            stringsData.loadedStringsCount++;
                        }
                    }
                }
                catch (Exception)
                {
                    // При необходимости можно добавить логирование ошибки
                }
            }
        }

        public static Dictionary<string, Dictionary<string, Dictionary<string, string>>>? FillTranslationsData(DataTable? translationsTable, string targetModLanguagesPath, string? name)
        {
            // Структура: Dictionary<LanguageName, Dictionary<SubPath, Dictionary<StringId, StringValue>>>
            var translationsData = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

            if (translationsTable == null)
                return null;

            var subPathColumn = SubPathColumnData;
            var idColumn = IdColumnData;
            var folderColumn = FolderColumnData;

            DataTable? folderTable = translationsTable.Clone();
            foreach(DataRow row in translationsTable.Rows)
            {
                if (row.Field<string>(folderColumn.Name) != name) continue;

                folderTable.Rows.Add(row.ItemArray);
            }

            foreach (DataRow row in folderTable.Rows)
            {
                string? subPath = row.Field<string>(subPathColumn.Name);
                string? stringId = row.Field<string>(idColumn.Name);
                if (string.IsNullOrEmpty(subPath) 
                    || string.IsNullOrEmpty(stringId)) continue;

                foreach (DataColumn column in translationsTable.Columns)
                {
                    if (EditorHelper.IsReadOnlyColumn(column.ColumnName))
                        continue;

                    string? stringValue = row.Field<string>(column.ColumnName);
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
                new XElement("modVersion", modAboutData.ModVersion ?? string.Empty),
                new XElement("packageId", modAboutData.PackageId ?? string.Empty),
                new XElement("supportedVersions", supportedVersionsList),
                new XElement("modDependencies"),
                new XElement("loadAfter", new XElement("li", loadAfterPackageId)),
                new XElement("description", modAboutData.Description ?? string.Empty)
            );

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), modMetaData);
            doc.Save(aboutXmlPath);
        }

        internal static ModData? SaveTranslatedStrings(IEnumerable<FolderData> folders, ModData? mod)
        {
            if (mod == null) return null;

            string targetModDirPath = Path.Combine(mod.ParentGame.ModsDirPath!, $"{mod.DirectoryName!}_Translated");

            int index = 0;
            while (Directory.Exists(targetModDirPath))
            {
                targetModDirPath = Path.Combine(mod.ParentGame.ModsDirPath!, $"{mod.DirectoryName!}_Translated{index++}");
            }

            if(!WriteTranslatedFolders(targetModDirPath, folders, mod))
            {
                return null;
            }

            string name = Properties.Settings.Default.TargetModName;
            string packageId = Properties.Settings.Default.TargetModPackageID;
            string author = Properties.Settings.Default.TargetModAuthor;
            string version = Properties.Settings.Default.TargetModVersion;
            string supportedVersions = Properties.Settings.Default.TargetModSupportedVersions;
            string supportedVersionsFromFolders = string.Join(",", EnumerateSupportedVersions(folders));
            string description = Properties.Settings.Default.TargetModDescription;
            string url = Properties.Settings.Default.TargetModUrl;
            var modAboutData = new ModAboutData
            {
                SourceMod = mod,
                Name = !string.IsNullOrWhiteSpace(name) ? name : $"{mod.About?.Name} Translation",
                PackageId = !string.IsNullOrWhiteSpace(packageId) ? packageId : $"{mod.About?.PackageId}.translation",
                Author = !string.IsNullOrWhiteSpace(author) ? author : $"{mod.About?.Author},Anonimous",
                ModVersion = !string.IsNullOrWhiteSpace(version) ? version : "1.0",
                SupportedVersions = 
                supportedVersionsFromFolders.Length > 0 
                    ? supportedVersionsFromFolders 
                    : !string.IsNullOrWhiteSpace(supportedVersions) 
                        ? supportedVersions
                        : mod.About?.SupportedVersions != null 
                            ? string.Join(",", mod.About?.SupportedVersions!) 
                            : "",
                
                Description = !string.IsNullOrWhiteSpace(description) ? description : $"{mod.About?.Name} Translation",
                Url = !string.IsNullOrWhiteSpace(url) ? url : "",
                Preview = Properties.Settings.Default.TargetModPreview
            };

            EditorHelper.WriteAbout(targetModDirPath, modAboutData);

            EditorHelper.WriteLoadFoldersXml(targetModDirPath, modAboutData, folders);

            Logger.Info(Translation.SavedTranslatedFilesTo0, targetModDirPath);

            return new ModData(mod.ParentGame) 
            {
                DirectoryName = $"{mod.DirectoryName!}_Translated",
                About = new AboutData()
                {
                    Name = modAboutData.Name,
                    PackageId = modAboutData.PackageId,
                    Author = modAboutData.Author,
                    ModVersion = modAboutData.ModVersion,
                    SupportedVersions = modAboutData.SupportedVersions.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
                    Description = modAboutData.Description,
                    Url = modAboutData.Url
                }
            };
        }

        private static bool WriteTranslatedFolders(string targetModDirPath, IEnumerable<FolderData> folders, ModData mod)
        {
            bool isAnyFolderFileWrote = false;
            DataTable mergedDataTable = folders.First().TranslationsTable!;
            foreach (var folder in folders.Skip(1))
            {
                string targetModLanguagesPath = Path.Combine(targetModDirPath, EditorHelper.GetTranslatableFolderName(folder.Name!), LANGUAGES_DIR_NAME);

                var translationsData = EditorHelper.FillTranslationsData(mergedDataTable, targetModLanguagesPath, folder.Name);
                if (translationsData == null)
                    continue;

                bool isAnyFileWrote = EditorHelper.WriteFiles(translationsData, targetModLanguagesPath);

                if (isAnyFileWrote)
                {
                    isAnyFolderFileWrote = true;
                }
            }

            if (!isAnyFolderFileWrote)
            {
                Directory.Delete(targetModDirPath, true);
                Logger.Warn(Translation.NoTranslatedFilesToSave);
                
                return false;
            }

            return true;
        }

        internal static IEnumerable<string> EnumerateSupportedVersions(IEnumerable<FolderData> folders)
        {
            return folders
                .SelectMany(f => f.SupportedVersions)
                .Select(s => s.StartsWith('v') ? s[1..] : s)
                .Distinct();
        }

        internal static bool IsVersionDir(string s)
        {
            return VersionDirRegex.IsMatch(s);
        }

        private static void WriteLoadFoldersXml(string targetModDirPath, ModAboutData modAboutData, IEnumerable<FolderData> folders)
        {
            var loadFoldersPath = Path.Combine(targetModDirPath, "LoadFolders.xml");

            var loadFoldersElement = new XElement("loadFolders");

            foreach (var folder in folders)
            {
                foreach (var version in folder.SupportedVersions)
                {
                    string v = (version.StartsWith('v') ? "" : "v");

                    var versionElement = loadFoldersElement.Element($"{v}{version}");
                    if (versionElement == null)
                    {
                        versionElement = new XElement($"{v}{version}");
                        loadFoldersElement.Add(versionElement);
                    }

                    versionElement.Add(new XElement("li", folder.Name));
                }
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), loadFoldersElement);
            doc.Save(loadFoldersPath);
        }

        internal static void ClearSelectedCells(IList<DataGridCellInfo>? selectedCells)
        {
            if (selectedCells == null || selectedCells.Count == 0) return;

            foreach (var (rowItem, column) in GetValidSelectedCells(selectedCells))
            {
                rowItem.Row[column.SortMemberPath] = null;
            }
            Logger.Info(Translation.ClearXSelectedCellsLogMessage, selectedCells.Count);
        }
        internal static void PasteStringsInSelectedCells(IList<DataGridCellInfo>? selectedCells)
        {
            if (selectedCells == null || selectedCells.Count == 0) return;

            // Read string lines from the clipboard
            string clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(clipboardText))
            {
                return;
            }

            string[] clipboardLines = clipboardText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int clipboardLineIndex = 0;

            foreach (var (rowItem, column) in GetValidSelectedCells(selectedCells).ToArray())
            {
                if (clipboardLineIndex >= clipboardLines.Length) break;

                var cellContent = rowItem.Row[column.SortMemberPath];
                if (cellContent == null || string.IsNullOrEmpty(cellContent + ""))
                {
                    // Write the string lines to empty SelectedCells
                    rowItem.Row[column.SortMemberPath] = clipboardLines[clipboardLineIndex++];
                }
            }

            Logger.Info(Translation.PastedXStringsToSelectedCellsLogMessage, clipboardLineIndex);
        }

        internal static void CutSelectedCells(IList<DataGridCellInfo>? selectedCells, bool onlyCopy = false)
        {
            if (selectedCells == null || selectedCells.Count == 0) return;

            List<string> strings = new();
            foreach (var (rowItem, column) in (onlyCopy 
                ? EnumerateValidSelectedCells(selectedCells) // enumerate when copy to prevent extra operation
                : GetValidSelectedCells(selectedCells))) 
            {
                string rowValue = rowItem.Row[column.SortMemberPath] + "";
                strings.Add(rowValue);
                if(!onlyCopy) rowItem.Row[column.SortMemberPath] = null;
            }

            if (strings.Count == 0)
            {
                return;
            }

            Clipboard.SetText(string.Join("\r\n", strings));

            string actionName = onlyCopy ? Translation.PrefixCopiedText : Translation.PrefixCutOutText;
            Logger.Info(T._("{0} {1} selected cells."), actionName, strings.Count);
        }

        /// <summary>
        /// get the whole list of selected cells
        /// for selected cellst using it will fix issues when values of selected cells are changing and the column of one selected cell is sorted
        /// </summary>
        /// <param name="selectedCells"></param>
        /// <returns></returns>
        internal static IEnumerable<(DataRowView row, DataGridColumn column)> GetValidSelectedCells(IList<DataGridCellInfo> selectedCells)
        {
            return EnumerateValidSelectedCells(selectedCells).ToArray();
        }
        internal static IEnumerable<(DataRowView row, DataGridColumn column)> EnumerateValidSelectedCells(IList<DataGridCellInfo> selectedCells)
        {
            foreach (var cell in selectedCells)
            {
                if (cell.Item is not DataRowView rowItem)
                {
                    continue;
                }

                if (cell.Column is not DataGridColumn column)
                {
                    continue;
                }

                if (column.IsReadOnly) continue;

                yield return (rowItem, column);
            }
        }

        public static EditorStringsData LoadStringsDataFromTheLanguageDir(string selectedTranslatableDir, EditorStringsData? stringsData = null)
        {
            stringsData ??= new();

            EditorHelper.ExtractStrings(selectedTranslatableDir, stringsData);
            EditorHelper.LoadDefKeyedStringsFromTheDir(selectedTranslatableDir, stringsData);

            return stringsData;
        }

        internal static Task<EditorStringsData?> LoadAllModsStringsData(Game? selectedGame)
        {
            if (selectedGame == null) return Task.FromResult<EditorStringsData?>(null);

            EditorStringsData overallStringsData = new();

            if (Directory.Exists(selectedGame.GameDirPath)) {
                foreach(var dlcDir in Directory.EnumerateDirectories(Path.Combine(selectedGame.GameDirPath, "Data")))
                {
                    EditorHelper.LoadDefKeyedStringsFromTheDir(dlcDir, overallStringsData, false);
                }
            }

            var modDirPaths = Directory.GetDirectories(selectedGame.ModsDirPath!);

            Parallel.ForEach(modDirPaths, modDirPath =>
            {
                EditorStringsData modStringsData = new();
                List<FolderData> folders = [];
                
                EditorHelper.GetTranslatableFolders(folders, modDirPath);

                foreach (var folder in folders)
                {
                    var selectedTranslatableDir = Path.Combine(modDirPath, EditorHelper.GetTranslatableFolderName(folder.Name));

                    if (Directory.Exists(Path.Combine(selectedTranslatableDir, LANGUAGES_DIR_NAME)))
                    {
                        EditorHelper.LoadDefKeyedStringsFromTheDir(selectedTranslatableDir, modStringsData, false);
                    }
                }

                lock (overallStringsData)
                {
                    LoadToOverallStringsData(modStringsData, overallStringsData);
                }
            });

            return Task.FromResult<EditorStringsData?>(overallStringsData);
        }

        private static void LoadToOverallStringsData(EditorStringsData modStringsData, EditorStringsData overallStringsData)
        {
            foreach (var stringIdLanguagesList in modStringsData.SubPathStringIdsList)
            {
                if (!overallStringsData.SubPathStringIdsList.TryGetValue(stringIdLanguagesList.Key, out StringsIdsBySubPath? stringIds))
                {
                    overallStringsData.SubPathStringIdsList[stringIdLanguagesList.Key] = stringIdLanguagesList.Value;
                }
                else
                {
                    foreach (var stringIdLanguagePair in stringIdLanguagesList.Value.StringIdLanguageValuePairsList)
                    {
                        if (!stringIds.StringIdLanguageValuePairsList.TryGetValue(stringIdLanguagePair.Key, out LanguageValuePairsData? languagePairs))
                        {
                            stringIds.StringIdLanguageValuePairsList[stringIdLanguagePair.Key] = stringIdLanguagePair.Value;
                        }
                        else
                        {
                            foreach (var langValuePair in stringIdLanguagePair.Value.LanguageValuePairs)
                            {
                                languagePairs.LanguageValuePairs[langValuePair.Key] = langValuePair.Value;
                            }
                        }
                    }
                }
            }

            foreach (var lang in modStringsData.Languages)
            {
                overallStringsData.Languages.Add(lang);
            }
        }

        internal static Task<(Dictionary<string, LanguageValuePairsData> cacheByStringId, Dictionary<string, LanguageValuePairsData> cacheByStringValue)> FillCache(EditorStringsData stringsData)
        {
            // Dictionary<string id, Dictionary<language name, string value>>
            var idCache = new Dictionary<string, LanguageValuePairsData>();

            // Dictionary<string value any language, Dictionary<language name, string value>>
            var valueCache = new Dictionary<string, LanguageValuePairsData>();

            foreach (var subPathStringIds in stringsData.SubPathStringIdsList.Values)
            {
                foreach (var stringIdLanguageValuePairs in subPathStringIds.StringIdLanguageValuePairsList)
                {
                    var stringId = stringIdLanguageValuePairs.Key;
                    if (!idCache.TryGetValue(stringId, out var idCachePairs))
                    {
                        idCachePairs = new();
                        idCache[stringId] = idCachePairs;
                    }

                    foreach (var languageValuePair in stringIdLanguageValuePairs.Value.LanguageValuePairs)
                    {
                        if (string.IsNullOrEmpty(languageValuePair.Key)
                            || string.IsNullOrEmpty(languageValuePair.Value)) continue;

                        string stringValue = languageValuePair.Value;

                        // add language pir by string value
                        if (!valueCache.TryGetValue(stringValue, out var valueCachePairs))
                        {
                            valueCachePairs = new();
                            valueCache[stringValue] = valueCachePairs;
                        }

                        idCachePairs.LanguageValuePairs[languageValuePair.Key] = languageValuePair.Value;
                        valueCachePairs.LanguageValuePairs[languageValuePair.Key] = languageValuePair.Value;

                    }
                }
            }

            return Task.FromResult((idCache, valueCache));
        }

        internal static bool IsReadOnlyColumn(string? columnName)
        {
            return columnName != null && EditorColumns.Any(c => c.Name == columnName);
        }

        internal static void TrySetTranslationByStringValue(Dictionary<string, LanguageValuePairsData> valueCache, DataRow row, DataColumnCollection columns)
        {
            var pairs = new LanguageValuePairsData();
            foreach (DataColumn column in columns)
            {
                if (EditorHelper.IsReadOnlyColumn(column.ColumnName))
                    continue; // skip readonly columns

                string? stringValue = row.Field<string>(column) + "";
                string language = column.ColumnName;

                pairs.LanguageValuePairs.Add(language, stringValue);
            }

            foreach (var pair in pairs.LanguageValuePairs)
            {
                if (!string.IsNullOrEmpty(pair.Value)) continue;

                // try get language pairs by string value
                if (!valueCache.TryGetValue(pair.Value, out var valueCachePairs)) continue;

                // try get exist translation by language
                if (valueCachePairs.LanguageValuePairs.TryGetValue(pair.Key, out var stringValue))
                {
                    row[pair.Key] = stringValue;
                    continue;
                }
            }
        }

        internal static bool TrySetTranslationByStringID(Dictionary<string, LanguageValuePairsData> idCache, DataRow row, DataColumnCollection columns)
        {
            string? stringId = row.Field<string>(IdColumnData.Name!);
            if (string.IsNullOrEmpty(stringId) || !idCache.TryGetValue(stringId, out var idCachePairs))
            {
                return false;
            }

            bool isAllFound = true; // by default is true but if there is some missing translation it will be false
            foreach (DataColumn column in columns)
            {
                if (EditorHelper.IsReadOnlyColumn(column.ColumnName))
                    continue; // skip readonly columns

                if (!row.IsNull(column)
                    && !string.IsNullOrEmpty(row.Field<string>(column)))
                {
                    // need only empty rows
                    continue;
                }

                var language = column.ColumnName;
                if (!idCachePairs.LanguageValuePairs.TryGetValue(language, out var stringValue))
                {
                    isAllFound = false;
                    continue;
                }

                row[column] = stringValue;
            }

            return isAllFound;
        }

        internal static Task SetTranslationsbyCache(EditorStringsDBCache cache, ObservableCollection<FolderData> folders)
        {
            if(cache.IdCache == null || cache.ValueCache == null)
            {
                Logger.Debug("Cache is empty. No translations to set.");
                return Task.CompletedTask;
            }   

            Parallel.ForEach(folders, folder =>
            {
                if (folder.TranslationsTable == null) return;

                foreach (DataRow row in folder.TranslationsTable.Rows)
                {
                    if (!EditorHelper.TrySetTranslationByStringID(cache.IdCache, row, folder.TranslationsTable.Columns))
                    {
                        EditorHelper.TrySetTranslationByStringValue(cache.ValueCache, row, folder.TranslationsTable.Columns);
                    }
                }
            });

            Logger.Info(Translation.SetTanslationsByCacheXFoldersLogMessage, folders.Count);
            return Task.CompletedTask;
        }

        internal static bool IsValidFolderName(string folderName)
        {
            // Check if the folder name contains any invalid characters
            return !folderName.Any(c => invalidChars.Contains(c));
        }

        internal static bool HaveAnyEmptyLanguageString(ObservableCollection<FolderData> folders)
        {
            foreach (var folder in folders)
            {
                if (folder.TranslationsTable == null) continue;
                foreach (DataRow row in folder.TranslationsTable.Rows)
                {
                    foreach (DataColumn column in folder.TranslationsTable.Columns)
                    {
                        if (EditorHelper.IsReadOnlyColumn(column.ColumnName))
                            continue; // skip readonly columns
                        string? stringValue = row.Field<string>(column);
                        if (string.IsNullOrEmpty(stringValue))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static void ClearSort(object? sortedCollection)
        {
            if (sortedCollection == null) return;

            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(sortedCollection);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                view.Refresh();
            }
        }

        internal static void RemoveAllButFirstFolder(ObservableCollection<FolderData> folders)
        {
            foreach(var folder in folders.Skip(1).ToArray())
            {
                if (folder != null)
                {
                    folders.Remove(folder);
                }
            }
        }
    }
}
