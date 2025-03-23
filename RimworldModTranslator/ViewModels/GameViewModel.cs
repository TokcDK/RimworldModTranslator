using CommunityToolkit.Mvvm.ComponentModel;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;

namespace RimworldModTranslator.ViewModels
{
    public partial class GameViewModel : ViewModelBase
    {
        // Set default ModsConfig.xml path to the Windows LocalLow directory.
        private readonly string DefaultModsConfigXmlPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "..", "LocalLow", "Ludeon Studios", "RimWorld by Ludeon Studios", "Config", "ModsConfig.xml");

        [ObservableProperty]
        private readonly Game? selectedGame;

        [ObservableProperty]
        private ObservableCollection<ModData> modsList = [];

        [ObservableProperty]
        private ObservableCollection<ModData> selectedMods = [];

        partial void OnSelectedGameChanged(Game? value)
        {
            LoadGameData(value);
        }

        private void LoadGameData(Game? game)
        {
            ModsList.Clear();
            if (game == null) return;
            if (string.IsNullOrEmpty(game.GamePath)
                || !Directory.Exists(game.GamePath)) return;

            // Load mods from a "Mods" folder in the game path
            string modsDir = Path.Combine(game.GamePath, "Mods");
            if (!Directory.Exists(modsDir)) return;

            // Use the default ModsConfig.xml path located in LocalLow
            if(string.IsNullOrEmpty(game.ConfigPath) || !Directory.Exists(game.ConfigPath))
            {
                game.ConfigPath = Path.GetDirectoryName(DefaultModsConfigXmlPath);
            }
            var modsConfigXmlPath = Path.Combine(game.ConfigPath!, "ModsConfig.xml");
            var modsConfig = LoadModsConfig(modsConfigXmlPath);
            if (modsConfig == null) return;

            foreach (var dir in Directory.EnumerateDirectories(modsDir))
            {
                var mod = LoadModData(dir);
                if (mod != null) ModsList.Add(mod);
            }

            foreach (var mod in ModsList)
            {
                mod.IsActive = !string.IsNullOrWhiteSpace(mod!.About!.PackageId)
                    && modsConfig.ActiveMods.Contains(mod!.About!.PackageId);
            }
        }

        private static ModData? LoadModData(string modDir)
        {
            var about = LoadAboutData(modDir);

            return new ModData
            {
                DirectoryName = Path.GetFileName(modDir),
                About = about,
                IsActive = false // Assume disabled by default; adjust based on game config if available
            };
        }

        private static AboutData? LoadAboutData(string modDir)
        {
            string aboutPath = Path.Combine(modDir, "About", "About.xml");
            if (!File.Exists(aboutPath)) return null;

            try
            {
                XDocument doc = XDocument.Load(aboutPath);
                var meta = doc.Element("ModMetaData");
                if (meta == null) return null;

                var about = new AboutData
                {
                    Name = meta.Element("name")?.Value,
                    Author = meta.Element("author")?.Value,
                    Url = meta.Element("url")?.Value,
                    SupportedVersions = meta.Element("supportedVersions")?.Elements("li").Select(e => e.Value).ToList() ?? new List<string>(),
                    PackageId = meta.Element("packageId")?.Value,
                    Description = meta.Element("description")?.Value,
                    LoadAfter = meta.Element("loadAfter")?.Elements("li").Select(e => e.Value).ToList() ?? [],
                    ModDependencies = meta.Element("modDependencies")?.Elements("li").Select(d => new ModDependency
                    {
                        PackageId = d.Element("packageId")?.Value,
                        DisplayName = d.Element("displayName")?.Value,
                        SteamWorkshopUrl = d.Element("steamWorkshopUrl")?.Value,
                        DownloadUrl = d.Element("downloadUrl")?.Value
                    }).ToList() ?? []
                };

                return about;
            }
            catch
            {
                return null; // Handle parsing errors gracefully
            }
        }

        private static ModsConfigData? LoadModsConfig(string? modsConfigXmlPath)
        {
            if (string.IsNullOrWhiteSpace(modsConfigXmlPath)) return null;
            if (!File.Exists(modsConfigXmlPath)) return null;

            try
            {
                XDocument doc = XDocument.Load(modsConfigXmlPath);
                var meta = doc.Element("ModsConfigData");
                if (meta == null) return null;

                var modsConfigData = new ModsConfigData
                {
                    Version = meta.Element("version")?.Value,
                    ActiveMods = meta.Element("activeMods")?.Elements("li").Select(e => e.Value).ToList() ?? [],
                    KnownExpansions = meta.Element("knownExpansions")?.Elements("li").Select(e => e.Value).ToList() ?? []
                };

                return modsConfigData;
            }
            catch
            {
                return null; // Handle parsing errors gracefully
            }
        }
    }
}