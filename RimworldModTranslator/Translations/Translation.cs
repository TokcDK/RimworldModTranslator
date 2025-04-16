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
        public static string RefreshModListName { get; } = T._("Refresh mod list");

        internal static string Header { get; } = T._("Editor");
        internal static string LoadStringsName { get; } = T._("Load strings"); // Modlist, Editor
        internal static string LoadStringsToolTip { get; } = T._("Load strings from the selected mod"); // Modlist, Editor
        internal static string SaveStringsName { get; } = T._("Save strings from of selected mod to a new mod");
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
    }
}
