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



namespace RimworldModTranslator.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private readonly Game? selectedGame;

        [ObservableProperty]
        private ObservableCollection<ModData> modsList = [];

        [ObservableProperty]
        private ObservableCollection<ModData> selectedMods = [];

        partial void OnSelectedGameChanged(Game? value)
        {
            GameHelper.LoadGameData(value);
        }

        private readonly ObservableCollection<Game> gamesList = [];

        [RelayCommand]
        private void AddNewGame()
        {
            var newGame = new Game();
            gamesList.Add(newGame);
            SelectedGame = newGame;
        }
    }
}