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
using System.IO;

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
            if(settingsService.SelectedGame == null) return;
            if(settingsService.SelectedGame.ModsDirPath == null) return;
            if(SelectedMod == null) return;
            if(SelectedMod.DirectoryName == null) return;

            string modPath = Path.Combine(settingsService.SelectedGame.ModsDirPath, SelectedMod.DirectoryName);
            if (System.IO.Directory.Exists(modPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = modPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                //System.Windows.MessageBox.Show("Mod directory does not exist.");
            }
        }
    }
}