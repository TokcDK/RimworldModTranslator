using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using System.Collections.ObjectModel;
using RimworldModTranslator.Services;
using System.Collections.Generic;
using RimworldModTranslator.Helpers;
using System;
using RimworldModTranslator.Messages;

namespace RimworldModTranslator.ViewModels
{
    public partial class ModListViewModel(SettingsService settingsService) : ViewModelBase
    {
        public string Header { get; } = "Mods";

        #region ToolTips
        public string LoadStringsToolTip { get => settingsService.LoadStringsToolTip; }
        public string RefreshModListToolTip { get; } = "Refresh mod list";
        #endregion

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
        [RelayCommand]
        private void LoadStrings()
        {
            if(SelectedMod == null) return;

            WeakReferenceMessenger.Default.Send(new LoadSelectedModStringsMessage());
        }
        [RelayCommand]
        private void OpenModDir()
        {
            if(SelectedMod == null) return;


        }
    }
}