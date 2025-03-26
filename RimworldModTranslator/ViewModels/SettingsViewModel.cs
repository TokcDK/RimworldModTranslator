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
    public partial class SettingsViewModel : ViewModelBase
    {
        public string Header { get; } = "Settings";

        public ObservableCollection<Game> GamesList { get => settingsService.GamesList; }

        [ObservableProperty]
        private Game? selectedGame;

        partial void OnSelectedGameChanged(Game? value)
        {
            var oldSelectedGame = value; // Save the old value in case the new value is invalid

            if(!GameHelper.LoadGameData(value, settingsService) && value != null)
            {
                settingsService.GamesList.Remove(value);
                SelectedGame = oldSelectedGame;
            }
            else
            {
                settingsService.SelectedGame = value;
                GameHelper.UpdateSharedModList(settingsService.ModsList, value!.ModsList);
                settingsService.SaveGamesList();
            }
        }

        [ObservableProperty]
        private string? newModsDirPath;
        [ObservableProperty]
        private string? newConfigDirPath;
        private readonly SettingsService settingsService;

        public SettingsViewModel(SettingsService settingsService)
        {
            this.settingsService = settingsService;
            if(settingsService.GamesList.Count > 0)
            {
                SelectedGame = settingsService.SelectedGame ?? settingsService.GamesList[0];
            }
        }

        [RelayCommand]
        private void AddNewGame()
        {
            if(NewModsDirPath == null)
            {
                return;
            }

            if (IsAlreadyAddedGame())
            {
                return;
            }

            NewModsDirPath = GameHelper.CheckCorrectModsPath(NewModsDirPath!);

            var newGame = new Game
            {
                ModsDirPath = NewModsDirPath,
                ConfigDirPath = NewConfigDirPath
            };

            if (!GameHelper.IsValidGame(newGame, settingsService)) return;

            GamesList.Add(newGame);
            SelectedGame = newGame;
        }

        private bool IsAlreadyAddedGame()
        {
            bool isInvalidConfigDirPath = string.IsNullOrWhiteSpace(NewConfigDirPath) || !Directory.Exists(NewConfigDirPath);
            string defaultConfigDirPath = Path.GetDirectoryName(settingsService.DefaultModsConfigXmlPath)!;
            NewConfigDirPath = isInvalidConfigDirPath ? defaultConfigDirPath : NewConfigDirPath;
            
            if (GamesList.Any(g => g.ModsDirPath == NewModsDirPath
                && g.ConfigDirPath == NewConfigDirPath))
            {
                return true;
            }

            return false;
        }
    }
}