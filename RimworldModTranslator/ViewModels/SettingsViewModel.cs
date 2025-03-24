using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Helpers;
using RimworldModTranslator.Services;



namespace RimworldModTranslator.ViewModels
{
    public partial class SettingsViewModel(SettingsService settingsService) : ViewModelBase
    {
        public string Header { get; } = "Settings";

        public ObservableCollection<Game> GamesList { get => settingsService.GamesList; }

        [ObservableProperty]
        private Game? selectedGame;

        partial void OnSelectedGameChanged(Game? value)
        {
            var oldSelectedGame = value; // Save the old value in case the new value is invalid

            settingsService.SelectedGame = value;
            if(!GameHelper.LoadGameData(value, settingsService) && value != null)
            {
                settingsService.GamesList.Remove(value);
                SelectedGame = oldSelectedGame;
            }
        }

        [ObservableProperty]
        private string? newGameDirPath;
        [ObservableProperty]
        private string? newGameConfigDirPath;

        [RelayCommand]
        private void AddNewGame()
        {
            var newGame = new Game
            {
                GameDirPath = NewGameDirPath,
                ConfigDirPath = NewGameConfigDirPath
            };

            if (!GameHelper.IsValidGame(newGame, settingsService)) return;

            GamesList.Add(newGame);
            SelectedGame = newGame;
        }
    }
}