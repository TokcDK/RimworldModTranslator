using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;
using RimworldModTranslator.Helpers;
using RimworldModTranslator.Services;



namespace RimworldModTranslator.ViewModels
{
    public partial class SettingsViewModel(SettingsService settingsService) : ViewModelBase
    {
        private readonly ObservableCollection<Game> gamesList = settingsService.GamesList;

        [ObservableProperty]
        private readonly Game? selectedGame;
        partial void OnSelectedGameChanged(Game? value)
        {
            settingsService.SelectedGame = value;
            GameHelper.LoadGameData(value);
        }

        [RelayCommand]
        private void AddNewGame()
        {
            var newGame = new Game();
            gamesList.Add(newGame);
            SelectedGame = newGame;
        }
    }
}