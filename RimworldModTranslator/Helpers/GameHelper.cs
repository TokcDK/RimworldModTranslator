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
        internal static void LoadGameData(Game? game, SettingsService settings)
        {
            if (game == null) return;

            game.ModsList.Clear();
            if (string.IsNullOrEmpty(game.GamePath)
                || !Directory.Exists(game.GamePath)) return;

            // Load mods from a "Mods" folder in the game path
            string modsDir = Path.Combine(game.GamePath, "Mods");
            if (!Directory.Exists(modsDir)) return;

            // Use the default ModsConfig.xml path located in LocalLow
            if (string.IsNullOrEmpty(game.ConfigPath) || !Directory.Exists(game.ConfigPath))
            {
                game.ConfigPath = Path.GetDirectoryName(settings.DefaultModsConfigXmlPath);
            }
            var modsConfigXmlPath = Path.Combine(game.ConfigPath!, "ModsConfig.xml");
            var modsConfig = ModHelper.LoadModsConfig(modsConfigXmlPath);
            if (modsConfig == null) return;

            foreach (var dir in Directory.EnumerateDirectories(modsDir))
            {
                var mod = ModHelper.LoadModData(dir);
                if (mod != null) game.ModsList.Add(mod);
            }

            foreach (var mod in game.ModsList)
            {
                mod.IsActive = !string.IsNullOrWhiteSpace(mod!.About!.PackageId)
                    && modsConfig.ActiveMods.Contains(mod!.About!.PackageId);
            }
        }
    }
}
