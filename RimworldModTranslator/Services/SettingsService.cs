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
    public partial class SettingsService
    {
        #region Shared ToolTips
        // Editor tab
        public string LoadStringsToolTip { get; } = "Load strings from the selected mod"; // Modlist,
        #endregion

        readonly char _gamesListPathsSeparator = '*';
        readonly char _gamesListTheGamePathsSeparator = '|';

        public Game? SelectedGame { get; internal set; }

        public ModData? SelectedMod { get; internal set; }

        public ObservableCollection<Game> GamesList { get; internal set; } = [];

        /// <summary>
        /// Shared modlist for Mod list tab
        /// </summary>
        public ObservableCollection<ModData> ModsList { get; } = [];
        public bool TryLoadTranslationsCache { get; internal set; } = false;
        
        public bool _forceLoadTranslationsCache = Properties.Settings.Default.ForceLoadTranslationsCache;
        public bool ForceLoadTranslationsCache 
        { 
            get => _forceLoadTranslationsCache; 
            internal set
            {
                if(value != _forceLoadTranslationsCache)
                {
                    Properties.Settings.Default.ForceLoadTranslationsCache = _forceLoadTranslationsCache = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

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
                    try
                    {
                        var gameParts = game.Split(_gamesListTheGamePathsSeparator);
                        if (gameParts.Length < 2 || gameParts.Length > 3) continue;

                        gameParts[0] = GameHelper.CheckCorrectModsPath(gameParts[0]);

                        var newGame = new Game
                        {
                            ModsDirPath = gameParts[0],
                            ConfigDirPath = gameParts[1],
                            GameDirPath = gameParts.Length == 2 ? "" : gameParts[2]
                        };

                        if (GameHelper.IsValidGame(newGame))
                        {
                            GamesList.Add(newGame);
                        }
                    }
                    catch
                    {
                        // skip errors
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
                GamesList.Select(g => 
                $"{g.ModsDirPath}{_gamesListTheGamePathsSeparator}" +
                $"{g.ConfigDirPath}{_gamesListTheGamePathsSeparator}" +
                $"{g.GameDirPath}"));
            Properties.Settings.Default.GamesList = gamesListString;
            Properties.Settings.Default.SelectedGameIndex = SelectedGame != null ? GamesList.IndexOf(SelectedGame) : -1;
            Properties.Settings.Default.Save();
        }
    }
}
