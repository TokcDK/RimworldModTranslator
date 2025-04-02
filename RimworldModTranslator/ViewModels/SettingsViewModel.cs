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
        public static string AddNewGameToolTip { get => "Add Mods and Config directory paths of the new game. If Config dir path is not set then will be used default in appdata"; }
        public static string ExtractedLanguageNameToolTip { get => "The name of the folder where the extracted strings will be saved. Default is 'Extracted'."; }
        public static string TargetModNameToolTip { get => "Target mod displaying name. Default: '{Source mode name} Translation'"; }
        public static string TargetModPackageIDToolTip { get => "Target mod PackageID. Default: '{Source mode PackageID}.translation'"; }
        public static string TargetModAuthorToolTip { get => "Target mod Author. Default: '{Source mod authors},Anonimous'"; }
        public static string TargetModVersionToolTip { get => "Target mod version. Default: '1.0'"; }
        public static string TargetModSupportedVersionsToolTip { get => "Target mod supported game version. Default: {Source mod supported versions}"; }
        public static string TargetModDescriptionToolTip { get => "Optional target mod description. Default: '{Source mode name} Translation'"; }
        public static string TargetModUrlToolTip { get => "Optional target mod web page URL. Default: No Url"; }
        public static string TargetModPreviewToolTip { get => "Optional target mod preview path. Default: No preview. When empty will try to find 'Preview.png' next to the app exe. "; }
        #endregion

        public ObservableCollection<Game> GamesList { get => settingsService.GamesList; }

        [ObservableProperty]
        private Game? selectedGame;

        [ObservableProperty]
        private bool tryLoadTranslationsCache = false;
        partial void OnTryLoadTranslationsCacheChanged(bool value)
        {
            settingsService.TryLoadTranslationsCache = value;
        }

        [ObservableProperty]
        private bool forceLoadTranslationsCache = false;
        partial void OnForceLoadTranslationsCacheChanged(bool value)
        {
            settingsService.ForceLoadTranslationsCache = value;
        }

        [ObservableProperty]
        private string? extractedLanguageName = Properties.Settings.Default.ExtractedStringsLanguageFolderName;
        partial void OnExtractedLanguageNameChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.ExtractedStringsLanguageFolderName = value;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.ExtractedStringsLanguageFolderName = "Extracted";
            }
        }

        #region Target mod data
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
        partial void OnTargetModPackageIDChanged(string? value)
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
        [ObservableProperty]
        private string? targetModDescription = Properties.Settings.Default.TargetModDescription;
        partial void OnTargetModDescriptionChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.TargetModDescription = value;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.TargetModDescription = "";
            }
        }
        [ObservableProperty]
        private string? targetModUrl = Properties.Settings.Default.TargetModUrl;
        partial void OnTargetModUrlChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.TargetModUrl = value;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.TargetModUrl = "";
            }
        }
        [ObservableProperty]
        private string? targetModPreview = Properties.Settings.Default.TargetModPreview;
        partial void OnTargetModPreviewChanged(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Properties.Settings.Default.TargetModPreview = value;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.TargetModPreview = "";
            }
        }
        #endregion

        partial void OnSelectedGameChanged(Game? value)
        {
            var oldSelectedGame = value; // Save the old value in case the new value is invalid

            if (!GameHelper.LoadGameData(value, settingsService) && value != null)
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
        [ObservableProperty]
        private string? newGameDirPath;
        private readonly SettingsService settingsService;

        public SettingsViewModel(SettingsService settingsService)
        {
            this.settingsService = settingsService;
            if (settingsService.GamesList.Count > 0)
            {
                SelectedGame = settingsService.SelectedGame ?? settingsService.GamesList[0];
            }
        }

        [RelayCommand]
        private void AddNewGame()
        {
            if (NewModsDirPath == null)
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
                ConfigDirPath = NewConfigDirPath,
                GameDirPath = NewGameDirPath
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
                                && g.ConfigDirPath == NewConfigDirPath 
                                && g.GameDirPath == NewGameDirPath
                            )
            )
            {
                return true;
            }

            return false;
        }
    }
}