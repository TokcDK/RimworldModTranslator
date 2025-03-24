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
        partial void OnSelectedModChanged(ModData? value)
        {
            settingsService.SelectedMod = value;
        }

        public ObservableCollection<ModData> ModsList { get => settingsService.ModsList; }

        [RelayCommand]
        private void RefreshModList()
        {
            if(settingsService.SelectedGame == null) return;

            var game = settingsService.SelectedGame;

            if (GameHelper.LoadGameData(game, settingsService))
            {
                GameHelper.UpdateSharedModList(settingsService.ModsList, game.ModsList);
            }
        }
    }
}