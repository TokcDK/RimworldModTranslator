using CommunityToolkit.Mvvm.ComponentModel;
using RimworldModTranslator.Helpers;
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
        
        readonly char _gamesListPathsSeparator = '*';
        readonly char _gamesListTheGamePathsSeparator = '|';

        [ObservableProperty]
        private Game? selectedGame;

        public ObservableCollection<Game> GamesList { get; internal set; } = [];

        /// <summary>
        /// Shared modlist for Mod list tab
        /// </summary>
        public ObservableCollection<ModData> ModsList { get; } = [];

        public SettingsService()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            LoadGamesListSettings();
        }

        private void LoadGamesListSettings()
        {
            var gamesListString = Properties.Settings.Default.GamesList;
            if (!string.IsNullOrEmpty(gamesListString))
            {
                var gamesList = gamesListString.Split(_gamesListPathsSeparator);
                foreach (var game in gamesList)
                {
                    var gameParts = game.Split(_gamesListTheGamePathsSeparator);
                    if (gameParts.Length != 2) continue;

                    var newGame = new Game
                    {
                        GameDirPath = gameParts[0],
                        ConfigDirPath = gameParts[1]
                    };

                    if (GameHelper.IsValidGame(newGame, this))
                    {
                        GamesList.Add(newGame);
                    }
                }
            }

            var selectedGameIndex = Properties.Settings.Default.SelectedGameIndex;
            if (selectedGameIndex > -1 && GamesList.Count > 0)
            {
                var selectedGame = selectedGameIndex >= 0 && selectedGameIndex < GamesList.Count
                    ? GamesList[selectedGameIndex]
                    : null;

                this.SelectedGame = selectedGame;
            }
        }

        public void SaveSettings()
        {
            SaveGamesList();
        }

        public void SaveGamesList()
        {
            var gamesListString = string.Join(_gamesListPathsSeparator,
                GamesList.Select(g => $"{g.GameDirPath}{_gamesListTheGamePathsSeparator}{g.ConfigDirPath}"));
            Properties.Settings.Default.GamesList = gamesListString;
            Properties.Settings.Default.SelectedGameIndex = SelectedGame != null ? GamesList.IndexOf(SelectedGame) : -1;
            Properties.Settings.Default.Save();
        }
    }
}
