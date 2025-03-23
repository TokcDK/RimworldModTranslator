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
        [ObservableProperty]
        private string? gamePath;

        [ObservableProperty]
        private ObservableCollection<ModData> modsList = [];

        [ObservableProperty]
        private ObservableCollection<ModData> selectedMods = [];

        partial void OnGamePathChanged(string? value)
        {
            LoadModsList(value);
        }

        private void LoadModsList(string? gamePath)
        {
            ModsList.Clear();
            if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath)) return;

            // Example: Load mods from a "Mods" folder in the game path
            string modsDir = System.IO.Path.Combine(gamePath, "Mods");
            if (!Directory.Exists(modsDir)) return;

            foreach (var dir in Directory.GetDirectories(modsDir))
            {
                var mod = LoadModData(dir);
                if (mod != null) ModsList.Add(mod);
            }
        }

        private static ModData? LoadModData(string modDir)
        {
            var about = LoadAboutData(modDir);

            return new ModData
            {
                DirectoryName = System.IO.Path.GetFileName(modDir),
                About = about,
                IsActive = false // Assume disabled by default; adjust based on game config if available
            };
        }

        private static AboutData? LoadAboutData(string modDir)
        {
            string aboutPath = System.IO.Path.Combine(modDir, "About", "About.xml");
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
    }
}