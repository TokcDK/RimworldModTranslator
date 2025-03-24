using RimworldModTranslator.Models;
using RimworldModTranslator.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Helpers
{
    class GameHelper
    {
        internal static bool IsValidGame(Game? game, SettingsService settings)
        {
            if (game == null) return false;

            game.ModsList.Clear();
            if (string.IsNullOrEmpty(game.GameDirPath)
                || !Directory.Exists(game.GameDirPath)) return false;

            // Load mods from a "Mods" folder in the game path
            string modsDir = Path.Combine(game.GameDirPath, "Mods");
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

            string modsDir = Path.Combine(game!.GameDirPath!, "Mods");
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

            settings.ModsList = game.ModsList;

            return true;
        }
    }
}
