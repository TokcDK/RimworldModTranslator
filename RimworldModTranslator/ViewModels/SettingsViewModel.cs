using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RimworldModTranslator.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace RimworldModTranslator.ViewModels
{
    public partial class SettingsViewModel(GameViewModel gameViewModel) : ViewModelBase
    {
        private readonly GameViewModel gameViewModel = gameViewModel;

        [ObservableProperty]
        public Game? selectedGame;

        ObservableCollection<Game> gamesList = [];

        [RelayCommand]
        private void AddNewGame()
        {
            var newGame = new Game();
            gamesList.Add(newGame);
            SelectedGame = newGame;
        }
    }
}