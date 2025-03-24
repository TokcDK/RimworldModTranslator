using CommunityToolkit.Mvvm.ComponentModel;
using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Services
{
    public partial class SettingsService : ObservableObject
    {
        // Set default ModsConfig.xml path to the Windows LocalLow directory.
        public readonly string DefaultModsConfigXmlPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "..", "LocalLow", "Ludeon Studios", "RimWorld by Ludeon Studios", "Config", "ModsConfig.xml");

        [ObservableProperty]
        private Game? selectedGame;

        public ObservableCollection<Game> GamesList { get; internal set; } = [];

        /// <summary>
        /// Shared modlist for Mod list tab
        /// </summary>
        public ObservableCollection<ModData> ModsList { get; } = [];
    }
}
