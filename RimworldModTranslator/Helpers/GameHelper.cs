using RimworldModTranslator.Models;
using RimworldModTranslator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Helpers
{
    class GameHelper
    {
        internal static string CheckCorrectModsPath(string modsPathToCheck)
        {
            if (Directory.Exists(Path.Combine(modsPathToCheck, "RimWorldWin64_Data")))
            {
                // was wrongly set game path
                if (Directory.Exists(Path.Combine(modsPathToCheck, "Mods")))
                {
                    // set mods path to the one inside of ame directory
                    return Path.Combine(modsPathToCheck, "Mods");
                }
            }

            return modsPathToCheck; // seems to be correct
        }

        internal static bool IsValidGame(Game? game, SettingsService settings)
        {
            if (game == null) return false;

            if (string.IsNullOrEmpty(game.ModsDirPath)
                || !Directory.Exists(game.ModsDirPath)) return false;

            // Load mods from a "Mods" folder in the game path
            string modsDir = game.ModsDirPath;
            if (!Directory.Exists(modsDir)) return false;

            // Use the default ModsConfig.xml path located in LocalLow
            if (string.IsNullOrEmpty(game.ConfigDirPath) || !Directory.Exists(game.ConfigDirPath))
            {
                game.ConfigDirPath = Path.GetDirectoryName(settings.DefaultModsConfigXmlPath);
            }

            return true;
        }

        internal static bool LoadGameData(Game? game, SettingsService settings)
        {
            if (!IsValidGame(game, settings)) return false;

            game.ModsList.Clear();

            string modsDir = game!.ModsDirPath!;
            foreach (var dir in Directory.EnumerateDirectories(modsDir))
            {
                var mod = ModHelper.LoadModData(dir);
                if (mod != null) game.ModsList.Add(mod);
            }

            var modsConfigXmlPath = Path.Combine(game.ConfigDirPath!, "ModsConfig.xml");
            var modsConfig = ModHelper.LoadModsConfig(modsConfigXmlPath);
            if (modsConfig == null) return false;

            foreach (var mod in game.ModsList)
            {
                mod.IsActive = mod.About != null &&!string.IsNullOrWhiteSpace(mod.About.PackageId)
                    && modsConfig.ActiveMods.Contains(mod.About.PackageId.ToLowerInvariant());
            }

            game.ModsList = [.. game.ModsList.OrderBy(g => modsConfig.ActiveMods.IndexOf((g.About == null || g.About.PackageId == null ? "" : g.About.PackageId).ToLowerInvariant()))];

            return true;
        }

        internal static void TryLoadSettings(SettingsService settingsService)
        {
            throw new NotImplementedException();
        }

        internal static void UpdateSharedModList(ObservableCollection<ModData> modlistToUpdate, ObservableCollection<ModData> modlistSource)
        {
            modlistToUpdate.Clear();

            foreach (var mod in modlistSource)
            {
                modlistToUpdate.Add(mod);
            }
        }
    }
}
