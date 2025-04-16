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
using RimworldModTranslator.Translations;



namespace RimworldModTranslator.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        public static string Header { get => Translation.SettingsName; }

        #region Names and Tooltips
        // General
        public static string GeneralName { get => Translation.GeneralName; }
        public static string GameName { get => Translation.GameName; }
        public static string ModsDirPathName { get => Translation.ModsDirPathName; }
        public static string ConfigDirPathName { get => $"{Translation.ConfigDirPathName} {Translation.SuffixOptionallName}"; }
        public static string GameDirPathName { get => $"{Translation.GameDirPathName} {Translation.SuffixOptionallName}"; }
        public static string AddGameName { get => Translation.AddGameName; }
        public static string AddNewGameToolTip { get => Translation.AddNewGameToolTip; }
        public static string ExtractedLanguageNameName { get => Translation.ExtractedLanguageNameName; }
        public static string ExtractedLanguageNameToolTip { get => Translation.ExtractedLanguageNameToolTip; }
        public static string ForceLoadTranslationsCacheName { get => Translation.ForceLoadTranslationsCacheName; }
        public static string ForceLoadTranslationsCacheToolTip { get => Translation.ForceLoadTranslationsCacheToolTip; }
        public static string LoadOnlyStringsForExtractedIdsName { get => Translation.LoadOnlyStringsForExtractedIdsName; }
        public static string LoadOnlyStringsForExtractedIdsToolTip { get => Translation.LoadOnlyStringsForExtractedIdsToolTip; }
        // Target mod data
        public static string TargetModDataName { get => Translation.TargetModDataName; }
        public static string TargetModDataTitleName { get => Translation.TargetModDataTitleName; }
        public static string TargetModDataToolTip { get => Translation.TargetModDataToolTip; }
        public static string TargetModNameToolTip { get => Translation.TargetModNameToolTip; }
        public static string TargetModNameName { get => Translation.TargetModNameName; }
        public static string TargetModPackageIDName { get => Translation.TargetModPackageIDName; }
        public static string TargetModPackageIDToolTip { get => Translation.TargetModPackageIDToolTip; }
        public static string TargetModAuthorName { get => Translation.TargetModAuthorName; }
        public static string TargetModAuthorToolTip { get => Translation.TargetModAuthorToolTip; }
        public static string TargetModVersionName { get => Translation.TargetModVersionName; }
        public static string TargetModVersionToolTip { get => Translation.TargetModVersionToolTip; }
        public static string TargetModSupportedVersionsName { get => Translation.TargetModSupportedVersionsName; }
        public static string TargetModSupportedVersionsToolTip { get => Translation.TargetModSupportedVersionsToolTip; }
        public static string TargetModDescriptionName { get => Translation.TargetModDescriptionName; }
        public static string TargetModDescriptionToolTip { get => Translation.TargetModDescriptionToolTip; }
        public static string TargetModUrlName { get => Translation.TargetModUrlName; }
        public static string TargetModUrlToolTip { get => Translation.TargetModUrlToolTip; }
        public static string TargetModPreviewName { get => Translation.TargetModPreviewName; }
        public static string TargetModPreviewToolTip { get => Translation.TargetModPreviewToolTip; }

        #endregion

        public ObservableCollection<Game> GamesList { get => settingsService.GamesList; }

        [ObservableProperty]
        private Game? selectedGame;

        partial void OnSelectedGameChanged(Game? value)
        {
            var oldSelectedGame = value; // Save the old value in case the new value is invalid

            if (!GameHelper.LoadGameData(value) && value != null)
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
        private bool _loadOnlyStringsForExtractedIds = Properties.Settings.Default.LoadOnlyStringsForExtractedIds;
        partial void OnLoadOnlyStringsForExtractedIdsChanged(bool value)
        {
            Properties.Settings.Default.LoadOnlyStringsForExtractedIds = value;
            Properties.Settings.Default.Save();
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
        private void CopySelectedGamePaths()
        {
            if(SelectedGame == null) return;

            NewModsDirPath = SelectedGame.ModsDirPath;
            NewConfigDirPath = SelectedGame.ConfigDirPath;
            NewGameDirPath = SelectedGame.GameDirPath;
        }

        [RelayCommand]
        private void AddNewGame()
        {
            if (NewModsDirPath == null)
            {
                return;
            }

            if (IsAlreadyAddedGame(out var foundGame))
            {
                if(string.IsNullOrWhiteSpace(foundGame!.GameDirPath)
                    && Directory.Exists(NewGameDirPath))

                // Setup game dir path for already exist game
                foundGame!.GameDirPath = NewGameDirPath;

                if(foundGame != SelectedGame)
                {
                    SelectedGame = foundGame;
                    GameHelper.UpdateSharedModList(settingsService.ModsList, SelectedGame!.ModsList);
                    settingsService.SaveGamesList();
                }

                return;
            }

            NewModsDirPath = GameHelper.CheckCorrectModsPath(NewModsDirPath!);

            var newGame = new Game
            {
                ModsDirPath = NewModsDirPath,
                ConfigDirPath = NewConfigDirPath,
                GameDirPath = NewGameDirPath
            };

            if (!GameHelper.IsValidGame(newGame)) return;

            GamesList.Add(newGame);
            SelectedGame = newGame;
        }

        private bool IsAlreadyAddedGame(out Game? game)
        {
            bool isInvalidConfigDirPath = string.IsNullOrWhiteSpace(NewConfigDirPath) || !Directory.Exists(NewConfigDirPath);
            string defaultConfigDirPath = Path.GetDirectoryName(GameHelper.DefaultModsConfigXmlPath)!;
            NewConfigDirPath = isInvalidConfigDirPath ? defaultConfigDirPath : NewConfigDirPath;


            game = GamesList.FirstOrDefault(g => 
                                    NewModsDirPath != null 
                                && 
                                    (g.ModsDirPath == NewModsDirPath 
                                    || g.ModsDirPath == Path.GetFullPath(NewModsDirPath))
                                && 
                                    (g.ConfigDirPath == NewConfigDirPath 
                                    || g.ConfigDirPath == Path.GetFullPath(NewConfigDirPath!))
                                && (string.IsNullOrWhiteSpace(g.GameDirPath) && Directory.Exists(NewGameDirPath) 
                                    || g.GameDirPath == NewGameDirPath 
                                    || g.GameDirPath == Path.GetFullPath(NewGameDirPath!))
                            );
            if (game != default)
            {
                return true;
            }

            return false;
        }
    }
}