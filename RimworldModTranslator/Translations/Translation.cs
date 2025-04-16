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
        internal static string ModsName { get; } = T._("Mods");
        internal static string RefreshModListName { get; } = T._("Refresh mod list");

        internal static string EditorName { get; } = T._("Editor");
        internal static string LoadStringsName { get; } = T._("Load strings"); // Modlist, Editor
        internal static string LoadStringsToolTip { get; } = T._("Load strings from the selected mod"); // Modlist, Editor
        internal static string SaveStringsName { get; } = T._("Save strings");
        internal static string SaveStringsTooltip { get; } = T._("Save strings from of selected mod to a new mod");
        internal static string EditorTableToolTip { get; } =
            T._("Help.\n\n" +
            "Move the mouse cursor over any elements to get the tooltip for it\n" +
            "\n\n" +
            "HotKeys:\n" +
            "Ctrl+C - Copy selected cells value\n" +
            "Ctrl+X - Cut selected cells value\n" +
            "Ctrl+V - Paste clipboard string lines into selected empty cells\n" +
            "Ctrl+D - Clear selected cells");
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
        internal static string ExtractedLanguageNameToolTip { get => T._("The name of the folder where the extracted strings will be saved. Default is 'Extracted'."); }
        internal static string TargetModPreviewToolTip { get => T._("Optional target mod preview path. Default: No preview. When empty will try to find 'Preview.png' next to the app exe. "); }
        internal static string ForceLoadTranslationsCacheName { get; } = T._("Force load translations from exist mods (Default: only once)");
        internal static string ForceLoadTranslationsCacheToolTip { get; } = T._("When enabled the translations of all dlcs and mods will be load each time. (slower, default: only 1st time and dont unload before the app restart)");
        internal static string LoadOnlyStringsForExtractedIdsName { get; } = T._("Load DefInjected strings only for exist extracted string ids");
        internal static string LoadOnlyStringsForExtractedIdsToolTip { get; } = T._("When enabled Load strings will load definjected strings from language dir for only ids which was extracted from defs.");
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
        internal static string TargetModSupportedVersionsName { get => T._("Supported versions}"); }
        internal static string TargetModSupportedVersionsToolTip { get => T._("Target mod supported game versions. Default: {Source mod supported versions}"); }
        internal static string TargetModDescriptionName { get => T._("Description"); }
        internal static string TargetModDescriptionToolTip { get => T._("Optional target mod description. Default: '{Source mode name} Translation'"); }
        internal static string TargetModUrlName { get => T._("Url"); }
        internal static string TargetModUrlToolTip { get => T._("Optional target mod web page URL. Default: No Url"); }
        internal static string TargetModPreviewName { get => T._("Preview path"); }
    }
}
