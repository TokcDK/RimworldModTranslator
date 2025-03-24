using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Services
{
    class SettingsService : ObservableObject
    {
        // Set default ModsConfig.xml path to the Windows LocalLow directory.
        public readonly string DefaultModsConfigXmlPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "..", "LocalLow", "Ludeon Studios", "RimWorld by Ludeon Studios", "Config", "ModsConfig.xml");
    }
}
