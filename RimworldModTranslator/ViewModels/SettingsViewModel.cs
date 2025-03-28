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

        #region Tooltips for settings
        public string AddNewGameToolTip { get => "Add Mods and Config directory paths of the new game. If Config dir path is not set then will be used default in appdata"; }
        public string ExtractedLanguageNameToolTip { get => "The name of the folder where the extracted strings will be saved. Default is 'Extracted'."; }
        public string TargetModNameToolTip { get => "Target mod displaying name. Default: '{Source mode name} Translation'"; }
        public string TargetModPackageIDToolTip { get => "Target mod PackageID. Default: '{Source mode PackageID}.translation'"; }
        public string TargetModAuthorToolTip { get => "Target mod Author. Default: '{Source mod authors},Anonimous'"; }
        public string TargetModVersionToolTip { get => "Target mod version. Default: '1.0'"; }
        public string TargetModSupportedVersionsToolTip { get => "Target mod supported game version. Default: {Source mod supported versions}"; }
        
        #endregion

        public ObservableCollection<Game> GamesList { get => settingsService.GamesList; }

        [ObservableProperty]
        private Game? selectedGame;
        
        [ObservableProperty]
        private string? extractedLanguageName = Properties.Settings.Default.ExtractedStringsLanguageFolderName;
        partial void OnExtractedLanguageNameChanged(string? value)
        {
            if(!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.ExtractedStringsLanguageFolderName = value;

                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.ExtractedStringsLanguageFolderName = "Extracted";
            }
        }

        // target mod data
        [ObservableProperty]
        private string? targetModName = Properties.Settings.Default.TargetModName;
        partial void OnTargetModNameChanged(string? value) 
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.TargetModName = value;

                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.TargetModName = "";
            }
        }
        [ObservableProperty]
        private string? targetModPackageID = Properties.Settings.Default.TargetModPackageID;
        partial void OnTargetModPackageIdChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.TargetModPackageID = value;

                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.TargetModPackageID = "";
            }
        }
        [ObservableProperty]
        private string? targetModAuthor = Properties.Settings.Default.TargetModAuthor;
        partial void OnTargetModAuthorChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.TargetModAuthor = value;

                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.TargetModAuthor = "";
            }
        }
        [ObservableProperty]
        private string? targetModVersion = Properties.Settings.Default.TargetModVersion;
        partial void OnTargetModVersionChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.TargetModVersion = value;

                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.TargetModVersion = "";
            }
        }
        [ObservableProperty]
        private string? targetModSupportedVersions = Properties.Settings.Default.TargetModSupportedVersions;
        partial void OnTargetModSupportedVersionsChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.TargetModSupportedVersions = value;

                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.TargetModSupportedVersions = "";
            }
        }

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