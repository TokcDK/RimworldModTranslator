using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RimworldModTranslator.Models;

namespace RimworldModTranslator.ViewModels
{
    public partial class SettingsViewModel(GameViewModel gameViewModel) : ViewModelBase
    {
        private readonly GameViewModel gameViewModel = gameViewModel;

        public Game? SelectedGame
        {
            get => gameViewModel.SelectedGame;
            set => gameViewModel.SelectedGame = value;
        }

        [RelayCommand]
        private void AddNewGame()
        {
            gameViewModel.AddNewGame();
        }
    }
}