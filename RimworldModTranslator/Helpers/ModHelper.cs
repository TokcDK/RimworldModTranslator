using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RimworldModTranslator.Helpers
{
    class ModHelper
    {
        internal static ModData? LoadModData(string modDir, Game game)
        {
            var about = LoadAboutData(modDir);

            return new ModData(game)
            {
                DirectoryName = Path.GetFileName(modDir),
                About = about,
                IsActive = false // Assume disabled by default; adjust based on game config if available
            };
        }

        internal static AboutData? LoadAboutData(string modDir)
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
                    ModVersion = meta.Element("modVersion")?.Value,
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

        internal static ModsConfigData? LoadModsConfig(string? modsConfigXmlPath)
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
