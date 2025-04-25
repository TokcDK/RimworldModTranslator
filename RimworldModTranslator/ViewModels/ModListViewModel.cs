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
using RimworldModTranslator.Translations;
using System.IO;

namespace RimworldModTranslator.ViewModels
{
    public partial class ModListViewModel(SettingsService settingsService) : ViewModelBase
    {
        public static string Header { get => Translation.ModsName; }

        #region Names and Tooltips
        public static string NameColumnName { get => Translation.NameName; }
        public static string ActiveColumnName { get => Translation.ActiveName; }
        public static string LoadStringsName { get => Translation.LoadStringsName; }
        public static string OpenModDirName { get => Translation.OpenModDirName; }
        public static string LoadStringsToolTip { get => Translation.LoadStringsToolTip; }
        public static string RefreshModListName { get => Translation.RefreshModListName; }
        public static string RefreshModListToolTip { get => Translation.RefreshModListName; }
        public static string ClearSortName { get => Translation.ClearSortName; }
        public static string ClearSortToolTip { get => Translation.ClearSortToolTip; }
        #endregion

        [ObservableProperty]
        private ModData? selectedMod;

        partial void OnSelectedModChanged(ModData? value)
        {
            settingsService.SelectedMod = value;
        }

        public ObservableCollection<ModData> ModsList { get => settingsService.ModsList; }


        [RelayCommand]
        private void ClearSort()
        {
            EditorHelper.ClearSort(settingsService.ModsList);
        }

        [RelayCommand]
        private void RefreshModList()
        {
            if(settingsService.SelectedGame == null) return;

            var game = settingsService.SelectedGame;

            if (GameHelper.LoadGameData(game))
            {
                GameHelper.UpdateSharedModList(settingsService.ModsList, game.ModsList);
            }
        }
        [RelayCommand]
        private void LoadStrings()
        {
            if(SelectedMod == null) return;

            WeakReferenceMessenger.Default.Send(new LoadSelectedModStringsMessage(SelectedMod));
        }
        [RelayCommand]
        private void OpenModDir()
        {            
            GameHelper.TryExploreDirectory(GameHelper.GetSelectedModPath(settingsService.SelectedGame, SelectedMod));
        }
    }
}