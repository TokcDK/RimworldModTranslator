using RimworldModTranslator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Translations
{
    internal static class Translation
    {
        public static string? FolderColumnName { get; } = T._("Folder");
        public static string? IdColumnName { get; } = T._("ID");
        public static string? SubPathColumnName { get; } = T._("SubPath");
        public static string ClearSortName { get; } = T._("Clear sorting");
        public static string ClearSortToolTip { get; } = T._("Clear corting by any column if was sorted.");
        public static string AboutDataIsNull { get; } = T._("About data is null.");
        public static string ErrorWritingAboutXml { get; } = T._("Error writing About.xml file: {0}");
        public static string ErrorWritingLoadFoldersXml { get; } = T._("Error writing LoadFolders.xml file: {0}");
        #region ui names and tooltips
        internal static string NameName { get; } = T._("Name");
        internal static string ActiveName { get; } = T._("☑");
        internal static string OpenModDirName { get; } = T._("Open mod dir");
        internal static string ModsName { get; } = T._("Mods");
        internal static string RefreshModListName { get; } = T._("Refresh");

        internal static string EditorName { get; } = T._("Editor");
        internal static string LoadStringsName { get; } = T._("Load strings"); // Modlist, Editor
        internal static string LoadStringsToolTip { get; } = T._("Load strings from the selected mod"); // Modlist, Editor
        internal static string SaveStringsName { get; } = T._("Save strings");
        internal static string SaveStringsTooltip { get; } = T._("Save strings from of selected mod to a new mod");
        internal static string EditorTableToolTip { get; } =
            $"{T._("Help")}.\n\n" +
            $"{T._("Move the mouse cursor over any elements to get the tooltip for it")}\n" +
            "\n\n" +
            $"{T._("HotKeys")}:\n" +
            $"Ctrl+C - {CopySelectedRowsToolTip}\n" +
            $"Ctrl+X - {CutSelectedRowsToolTip}\n" +
            $"Ctrl+V - {PasteToSelectedRowsToolTip}\n" +
            $"Ctrl+D - {ClearSelectedRowsToolTip}\n" +
            $"Ctrl+S - {SaveModDBToolTip}";
        internal static string FolderSelectionToolTip { get; } = T._("Select folder to translate.");
        internal static string AddNewLanguageToolTip { get; } = T._("Enter the new language folder name and press add to add the new column.");
        internal static string LoadStringsCacheToolTip { get; } = T._("Load strings from all exist game(when the game dir path is set) dlcs and mods");
        internal static string FolderName { get; } = T._("Folder");
        internal static string AddLanguageName { get; } = T._("Add new language");
        
        internal static string SettingsName { get; } = T._("Settings");        
        // general
        internal static string GameName { get; } = T._("Game");
        internal static string GeneralName { get; } = T._("General");
        internal static string SuffixOptionallName { get => T._("(optional)"); }
        internal static string ModsDirPathName { get => T._("Mods dir path"); }
        internal static string ConfigDirPathName { get => T._("Config dir path"); }
        internal static string GameDirPathName { get => T._("Game dir path"); }
        internal static string AddGameName { get => T._("Add game"); }
        internal static string AddNewGameToolTip { get => T._("Add Mods and Config directory paths of the new game. If Config dir path is not set then will be used default in appdata"); }
        internal static string ExtractedLanguageNameName { get => T._("Extracted strings dir name"); }
        internal static string ExtractedLanguageNameToolTip { get => T._("The name of the folder where the extracted strings will be saved. Default is 'Extracted'.\nWarning: If will be same name as any exist language folder name, the language name will be skipped!"); }
        internal static string TargetModPreviewToolTip { get => T._("Optional target mod preview path. Default: No preview. When empty will try to find 'Preview.png' next to the app exe. "); }
        internal static string ForceLoadTranslationsCacheName { get; } = T._("Force load translations from exist mods (Default: only once)");
        internal static string ForceLoadTranslationsCacheToolTip { get; } = T._("When enabled the translations of all dlcs and mods will be load each time. (slower, default: only 1st time and dont unload before the app restart)");
        internal static string LoadOnlyStringsForExtractedIdsName { get; } = T._("Load DefInjected strings only for exist extracted string ids");
        internal static string LoadOnlyStringsForExtractedIdsToolTip { get; } = T._("When enabled Load strings will load DefInjected strings from language dir for only ids which was extracted from defs.");
        // target mod data
        internal static string TargetModDataName { get; } = T._("Target mod data");
        internal static string TargetModDataTitleName { get; } = T._("Target mod data (for About.xml)");
        internal static string TargetModDataToolTip { get; } = T._("Target mod data for About.xml file of the new target mod");
        internal static string TargetModNameName { get => T._("Name"); }
        internal static string TargetModNameToolTip { get => T._("Target mod displaying name. Default: '{Source mode name} Translation'"); }
        internal static string TargetModPackageIDName { get => T._("PackageID"); }
        internal static string TargetModPackageIDToolTip { get => T._("Target mod PackageID. Default: '{Source mode PackageID}.translation'"); }
        internal static string TargetModAuthorName { get => T._("Author"); }
        internal static string TargetModAuthorToolTip { get => T._("Target mod Author. Default: '{Source mod authors},Anonimous'"); }
        internal static string TargetModVersionName { get => T._("Version"); }
        internal static string TargetModVersionToolTip { get => T._("Target mod version. Default: '1.0'"); }
        internal static string TargetModSupportedVersionsName { get => T._("Supported versions"); }
        internal static string TargetModSupportedVersionsToolTip { get => T._("Target mod supported game versions. Default: {Source mod supported versions}"); }
        internal static string TargetModDescriptionName { get => T._("Description"); }
        internal static string TargetModDescriptionToolTip { get => T._("Optional target mod description. Default: '{Source mode name} Translation'"); }
        internal static string TargetModUrlName { get => T._("Url"); }
        internal static string TargetModUrlToolTip { get => T._("Optional target mod web page URL. Default: No Url"); }
        internal static string TargetModPreviewName { get => T._("Preview path"); }
        internal static string CutSelectedRowsName { get; } = T._("Cut selected rows");
        internal static string CopySelectedRowsName { get; } = T._("Copy selected rows");
        internal static string PasteToSelectedRowsName { get; } = T._("Paste to selected rows");
        internal static string ClearSelectedRowsName { get; } = T._("Clear selected rows");
        internal static string LoadModDBName { get; } = T._("Load mod DB");
        internal static string LoadModDBReplaceName { get; } = T._("Load mod DB (replace)");
        internal static string SaveModDBName { get; } = T._("Save mod DB");
        internal static string CutSelectedRowsToolTip { get; } = T._("Cut values of selected rows to clipboard.");
        internal static string CopySelectedRowsToolTip { get; } = T._("Copy values of selected rows to clipboard.");
        internal static string PasteToSelectedRowsToolTip { get; } = T._("Paste values from clipboard to selected empty rows.");
        internal static string ClearSelectedRowsToolTip { get; } = T._("Clear values of selected rows.");
        internal static string SaveModDBToolTip { get; } = T._("Save mod DB to the selected mod folder.");
        internal static string LoadModDBToolTip { get; } = T._("Load mod DB from the selected mod folder.");
        internal static string LoadModDBReplaceToolTip { get; } = T._("Load mod DB from the selected mod folder and replace each folder table by the table from DB.\n(use only when the DB is for this version of mod)");
        internal static string EditorAutoSaveTimePeriodName { get; } = T._("Editor autosave time period (sec)");
        internal static string EditorAutoSaveTimePeriodToolTip { get; } = T._("The time in seconds after which the currently editing strings will be autosaved.");
        #endregion

        #region log messages
        internal static string Loaded0ModsFrom1 { get; } = T._("Loaded {0} mods from {1}.");
        internal static string PastedXStringsToSelectedCellsLogMessage { get; } = T._("Pasted {0} strings to selected cells.");
        internal static string PrefixCopiedText { get; } = T._("Copied");
        internal static string PrefixCutOutText { get; } = T._("Cut out");
        internal static string SetTanslationsByCacheXFoldersLogMessage { get; } = T._("Set translations by cache. {0} folders.");
        internal static object AppStartedLogMessage { get; } = T._("Application started");
        internal static string Loaded0StringsFrom1LogMessage { get; } = T._("Loaded {0} strings from {1}.");
        internal static string LoadedStringsCacheFromXLogMessage { get; } = T._("Loaded strings cache from {0}.");
        internal static string ClearXSelectedCellsLogMessage { get; } = T._("Cleared {0} selected cells.");
        internal static string LogFileNotFound { get; } = T._("Log file not found: {0}");
        internal static string ModIsNotSetWarnLogMessage { get; } = T._("Mod is not set. Please select mod to load strings.");
        internal static string ModsPathIsNotSetWarnLogMessage { get; } = T._("Mods path is not set. Please select the mods path.");
        internal static string NoTranslatableFoldersFoundLogMessage { get; } = T._("No translatable folders found.");
        internal static string NothingToLoadFromXLogMessage { get; } = T._("Nothing to load from {0}.");
        internal static string ErrorLoadingTagsFrom0 { get; } = T._("Error loading tags from {0}.");
        internal static string LoadedTagsFrom0 { get; } = T._("Loaded tags from {0}.");
        internal static string LoadedDefaultTags { get; } = T._("Loaded default tags.");
        internal static string SavedTranslatedFilesTo0 { get; } = T._("Saved translated files to {0}.");
        internal static string NoTranslatedFilesToSave { get; } = T._("No translated files to save.");
        internal static string SaveModFileWasWrote { get; } = T._("Mod DB saved to {0}");
        internal static string LoadedDBForFolder0 { get; } = T._("Loaded DB for folder {0}.");
        internal static string DBFileNotFoundAt0 { get; } = T._("DB file not found at {0}");
        internal static string ErrorLoadingDBFileFrom0 { get; } = T._("Error loading DB file from {0}.");
        internal static string LoadedDBFrom0 { get; } = T._("Loaded DB from {0}.");
        internal static string LoadedStringsFromDBFileLogMessage { get; } = T._("Loaded strings from DB file.");
        internal static string LoadedTotal0StringsForAllFoldersLogMessage { get; } = T._("Loaded total {0} strings for all folders.");
        internal static string SavedModConfigurationTo0 { get; } = T._("Saved mod configuration to {0}");
        internal static string FailedToSaveModsConfigTo0 { get; } = T._("Failed to save mods configuration: {0}");
        internal static string OpenDirErrorLogMessage { get; } = T._("Error opening directory: {0}");
        internal static string DirPathIsNotSetWarnLogMessage { get; } = T._("Directory path is not set. Path: {0}");
        internal static string NothingToTranslateLogMessage { get; } = T._("Nothing to translate.");
        #endregion
    }
}
