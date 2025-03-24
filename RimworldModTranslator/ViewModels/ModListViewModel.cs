using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using System.Collections.ObjectModel;
using RimworldModTranslator.Services;
using System.Collections.Generic;
using RimworldModTranslator.Helpers;

namespace RimworldModTranslator.ViewModels
{
    public partial class ModListViewModel(SettingsService settingsService) : ViewModelBase
    {
        public string Header { get; } = "Mods";

        [ObservableProperty]
        private ModData? selectedMod;

        public ObservableCollection<ModData> ModsList { get => settingsService.ModsList; }

        [RelayCommand]
        private void RefreshModList()
        {
            if(settingsService.SelectedGame == null) return;

            GameHelper.LoadGameData(settingsService.SelectedGame, settingsService);
        }
    }
}