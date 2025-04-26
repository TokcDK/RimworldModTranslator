using NLog;
using RimworldModTranslator.Models;
using RimworldModTranslator.Translations;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace RimworldModTranslator.Helpers
{
    class GameHelper
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // Set default ModsConfig.xml path to the Windows LocalLow directory.
        public static string DefaultModsConfigXmlPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "..", "LocalLow", "Ludeon Studios", "RimWorld by Ludeon Studios", "Config", "ModsConfig.xml");

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

        internal static string? GetSelectedModPath(Game? selectedGame, ModData? selectedMod)
        {
            if (selectedGame == null) return null;
            if (selectedGame.ModsDirPath == null) return null;
            if (selectedMod == null) return null;
            if (selectedMod.DirectoryName == null) return null;

            return Path.Combine(selectedGame.ModsDirPath, selectedMod.DirectoryName);
        }

        internal static bool IsValidGame(Game? game)
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
                game.ConfigDirPath = Path.GetDirectoryName(DefaultModsConfigXmlPath);
            }

            return true;
        }

        internal static bool SortMod(ModData? inputModToAdd, ModData? inputModToSortAfter)
        {
            if (inputModToAdd == null || inputModToSortAfter == null) return false;
            if (!inputModToSortAfter.IsActive) return false; // dont need to sort when mod is not active

            var game = inputModToSortAfter.ParentGame;
            if (!IsValidGame(game)) return false; // game is not valid

            var modToAdd = game.ModsList.FirstOrDefault(m => m.About?.PackageId == inputModToAdd.About!.PackageId);
            if (modToAdd != null) return false; // already added

            int indexA = game.ModsList.IndexOf(inputModToSortAfter);
            if (indexA == -1) return false; // not found

            inputModToAdd.IsActive = true; // enable to be written as active mod

            game.ModsList.Insert(indexA + 1, inputModToAdd!);

            if (!SaveGameData(game)) return false; // failed save

            return true;
        }

        internal static bool SaveGameData(Game game)
        {
            if (!IsValidGame(game)) return false;

            string modsConfigXmlPath = Path.Combine(game.ConfigDirPath!, "ModsConfig.xml");

            try
            {
                // Создаем XML документ
                var xmlDoc = new System.Xml.XmlDocument();

                // Добавляем XML декларацию
                var xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                // Создаем корневой элемент
                var rootElement = xmlDoc.CreateElement("ModsConfigData");
                xmlDoc.AppendChild(rootElement);

                // Добавляем версию
                var versionElement = xmlDoc.CreateElement("version");
                versionElement.InnerText = game!.ModsConfig!.Version!;
                rootElement.AppendChild(versionElement);

                // Добавляем активные моды
                var activeModsElement = xmlDoc.CreateElement("activeMods");
                rootElement.AppendChild(activeModsElement);

                foreach (var mod in game.ModsList.Where(m => m.IsActive && m.About?.PackageId != null))
                {
                    var liElement = xmlDoc.CreateElement("li");
                    liElement.InnerText = mod.About!.PackageId!.ToLowerInvariant();
                    activeModsElement.AppendChild(liElement);
                }

                // Добавляем известные расширения (если такая информация есть)
                var knownExpansionsElement = xmlDoc.CreateElement("knownExpansions");
                rootElement.AppendChild(knownExpansionsElement);

                foreach (var expansion in game.ModsConfig.KnownExpansions)
                {
                    var liElement = xmlDoc.CreateElement("li");
                    liElement.InnerText = expansion;
                    knownExpansionsElement.AppendChild(liElement);
                }

                // Сохраняем XML в файл
                xmlDoc.Save(modsConfigXmlPath);

                _logger.Info(Translation.SavedModConfigurationTo0, modsConfigXmlPath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, Translation.FailedToSaveModsConfigTo0, ex.Message);
                return false; // save error
            }
        }

        internal static bool LoadGameData(Game? game)
        {
            if (!IsValidGame(game)) return false;

            game.ModsList.Clear();

            string modsDir = game!.ModsDirPath!;
            foreach (var dir in Directory.EnumerateDirectories(modsDir))
            {
                var mod = ModHelper.LoadModData(dir, game);
                if (mod != null) game.ModsList.Add(mod);
            }

            var modsConfigXmlPath = Path.Combine(game.ConfigDirPath!, "ModsConfig.xml");
            var modsConfig = ModHelper.LoadModsConfig(modsConfigXmlPath);
            if (modsConfig == null) return false;

            game.ModsConfig = modsConfig;

            foreach (var mod in game.ModsList)
            {
                mod.IsActive = mod.About != null && !string.IsNullOrWhiteSpace(mod.About.PackageId)
                    && modsConfig.ActiveMods.Contains(mod.About.PackageId.ToLowerInvariant());
            }

            game.ModsList = [.. game.ModsList.OrderBy(g => modsConfig.ActiveMods.IndexOf((g.About == null || g.About.PackageId == null ? "" : g.About.PackageId).ToLowerInvariant()))];

            _logger.Info(Translation.Loaded0ModsFrom1, game.ModsList.Count, game.ModsDirPath);

            return true;
        }

        internal static bool TryExploreDirectory(string? directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                _logger.Warn(Translation.DirPathIsNotSetWarnLogMessage, directoryPath);
                return false;
            }
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = directoryPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, Translation.OpenDirErrorLogMessage, directoryPath);
                return false;
            }

            return true;
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
