using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RimworldModTranslator.Models;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RimworldModTranslator.Services;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Data;
using RimworldModTranslator.Helpers;
using System.Windows.Controls;
using System.Collections.Specialized;
using RimworldModTranslator.Views;
using System.Windows;
using System.Data.Common;
using System.Threading.Tasks;
using NLog;

namespace RimworldModTranslator.ViewModels
{
    // subfolders and xml file naming
    // For Defs: Languages\%LanguageCode%\DefInjected\XmlParentTagNameInsideOfRootDefsTag\ParentXmlName.xml
    // For keyed (each xml tag value, only from exist language dir): Languages\%LanguageCode%\Keyed\Keyed_%LanguageCode%.xml
    // For common Strings (in txt each line, only from exist language dir): Languages\LanguageCode\Strings\Names\*.txt
    // 
    // key name in xml
    // <%defName_key_value%.%translatable_key_name%>%translatable_key_value%</%defName_key_value%.%translatable_key_name%>

    // defs language xml structure
    // %defName_key_value2% includes the all sub tags before translatable tag
    //
    // <?xml version="1.0" encoding="utf-8"?>
    //<LanguageData>
    //
    //<%defName_key_value1%.%translatable_key_name%>%translatable_key_value%</%defName_key_value1%.%translatable_key_name%>
    //<%defName_key_value1%.%translatable_key_name%>%translatable_key_value%</%defName_key_value1%.%translatable_key_name%>
    //
    //<%defName_key_value2%.%translatable_key_name%>%translatable_key_value%</%defName_key_value2%.%translatable_key_name%>
    //<%defName_key_value2%.%translatable_key_name%>%translatable_key_value%</%defName_key_value2%.%translatable_key_name%>
    //
    //</LanguageData>

    // for editor extra functions to insert most often using replacers
    // replacers: https://rimworldwiki.com/wiki/Modding_Tutorials/GrammarResolver
    public partial class TranslationEditorViewModel : ViewModelBase
    {
        #region Fields
        private ModData? _mod;
        private Game? _game;
        private readonly SettingsService _settingsService;
        private string? _previousSelectedFolder;
        #endregion

        #region ToolTips
        public string EditorTableToolTip { get; } =
            "Help.\r\r" +
            "Move the mouse cursor over any elements to get the tooltip for it\r" +
            "\r\r" +
            "HotKeys:\r" +
            "Ctrl+C - Copy selected cells value\r" +
            "Ctrl+X - Cut selected cells value\r" +
            "Ctrl+V - Paste clipboard string lines into selected empty cells\r" +
            "Ctrl+D - Clear selected cells";
        public string FolderSelectionToolTip { get; } = "Select folder to translate.";
        public string AddNewLanguageToolTip { get; } = "Enter the new language folder name and press add to add the new column.";
        public string LoadStringsCacheToolTip { get; } = "Load strings from all exist game(when the game dir path is set) dlcs and mods";
        public string LoadStringsToolTip { get => _settingsService.LoadStringsToolTip; }
        public string SaveStringsToolTip { get; } = "Save strings from of selected mod to a new mod";
        #endregion

        #region Constructors
        public TranslationEditorViewModel(SettingsService settingsService)
        {
            this._settingsService = settingsService;
            Folders.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsFoldersEnabled));

            InitTranslationsTable();
        }
        #endregion

        #region Properties
        public string Header { get; } = "Editor";

        private static Logger _logger { get; } = LogManager.GetCurrentClassLogger();

        public bool IsTranslatorEnabled { get => IsTheTranslatorEnabled(); }

        public bool IsAddNewLanguageEnabled { get => IsAddNewLanguageButtonEnabled(); }

        public bool IsFoldersEnabled { get => IsTheFoldersEnabled(); }

        public string? ModDisplayingName => _mod != null && Folders.Count > 0 ? _mod.ModDisplayingName : _settingsService.SelectedMod?.ModDisplayingName;

        public ObservableCollection<FolderData> Folders { get; } = new();

        private IList<DataGridCellInfo>? selectedCells;
        public IList<DataGridCellInfo>? SelectedCells
        {
            get => selectedCells;
            set
            {
                SetProperty(ref selectedCells, value);
            }
        }

        public DataTable? TranslationsTable
        {
            get => SelectedFolder?.TranslationsTable;
            set
            {
                if (SelectedFolder == null || SelectedFolder.TranslationsTable == value)
                {
                    return;
                }
                SelectedFolder.TranslationsTable = value;
            }
        }

        public Dictionary<string, LanguageValuePairsData>? IdCache { get; private set; }
        public Dictionary<string, LanguageValuePairsData>? ValueCache { get; private set; }

        #endregion

        #region Observable Properties
        [ObservableProperty]
        private FolderData? selectedFolder;
        partial void OnSelectedFolderChanged(FolderData? value)
        {
            if (value?.Name == _previousSelectedFolder) return;

            _previousSelectedFolder = value?.Name;
            InitTranslationsTable(dataTableToRelink: value?.TranslationsTable);
        }

        [ObservableProperty]
        private string? newLanguageName;
        partial void OnNewLanguageNameChanged(string? value)
        {
            OnPropertyChanged(nameof(IsAddNewLanguageEnabled));
        }

        [ObservableProperty]
        private ObservableCollection<string> languages = new();

        [ObservableProperty]
        private DataView? translationsView;

        // will not fire with SelectionUnit="CellOrRowHeader" selected cells but still can be used to select row programmatically
        [ObservableProperty]
        private DataRowView? selectedRow;
        partial void OnSelectedRowChanged(DataRowView? value)
        {
        }
        [ObservableProperty]
        private int selectedRowIndex;
        partial void OnSelectedRowIndexChanged(int value)
        {
        }
        #endregion

        #region Commands
        [RelayCommand]
        private async Task LoadStrings()
        {
            await LoadTheSelectedModStrings();
        }

        [RelayCommand]
        private async Task LoadStringsCache()
        {
            if(_settingsService.ForceLoadTranslationsCache || IdCache == null || ValueCache == null)
            {
                var stringsData = await EditorHelper.LoadAllModsStringsData(_settingsService.SelectedGame);

                if (stringsData == null) return;

                (IdCache, ValueCache) = await EditorHelper.FillCache(stringsData);

                if(Directory.Exists(_settingsService.SelectedGame?.GameDirPath))
                {
                    _logger.Info($"Loaded strings cache from {_settingsService.SelectedGame?.GameDirPath}.");
                }
                _logger.Info($"Loaded strings cache from {_settingsService.SelectedGame?.ModsDirPath}.");
            }

            await EditorHelper.SetTranslationsbyCache(IdCache, ValueCache, Folders);
        }

        public async Task LoadTheSelectedModStrings()
        {
            if (_game == null || _game != _settingsService.SelectedGame)
            {
                // load only when game was not set or changed
                _game = _settingsService.SelectedGame;
                if (_game == null)
                {
                    _logger.Warn("Game is not set. Please select the game.");
                    return;
                }
            }

            bool isChangedMod = _mod != _settingsService.SelectedMod;

            if (isChangedMod || _mod == null)
            {
                // load only when mod was not set or changed
                _mod = _settingsService.SelectedMod;
                if (_mod == null)
                {
                    _logger.Warn("Mod is not set. Please select the mod.");
                    return;
                }
            }

            //if(!isChangedMod && previousSelectedFolder == SelectedFolder)
            //{
            //    // dont need? to reload strings for the same mod folder again
            //    return;
            //}
            if (isChangedMod || Folders.Count == 0)
            {
                string modPath = Path.Combine(_game!.ModsDirPath!, _mod.DirectoryName!);
                if (!Directory.Exists(modPath)) return;

                Folders.Clear();

                EditorHelper.GetTranslatableFolders(Folders, modPath);
            }

            if (Folders.Count == 0) return;

            SelectedFolder ??= Folders[0];

            string selectedFolderName = SelectedFolder!.Name;

            var selectedTranslatableDir = Path.Combine(_game!.ModsDirPath!, _mod!.DirectoryName!, EditorHelper.GetTranslatableFolderName(selectedFolderName));

            var stringsData = EditorHelper.LoadStringsDataFromTheLanguageDir(selectedTranslatableDir);

            var translationsTable = EditorHelper.CreateTranslationsTable(stringsData);

            if (translationsTable == null || translationsTable.Columns.Count == 0)
            {
                SelectedFolder = Folders.FirstOrDefault(f => f.Name == _previousSelectedFolder);
            }

            if (SelectedFolder == null) return;

            SelectedFolder.TranslationsTable = translationsTable;
            InitTranslationsTable(dataTableToRelink: translationsTable);

            OnPropertyChanged(nameof(ModDisplayingName));

            _logger.Info($"Loaded strings from {selectedTranslatableDir}.");
        }

        [RelayCommand]
        private void SaveStrings()
        {
            if (_game == null) return;
            if (_mod == null) return;

            SaveLanguages();
        }

        [RelayCommand]
        private void AddNewLanguage()
        {
            if (_game == null) return;
            if (_mod == null) return;

            if (string.IsNullOrEmpty(NewLanguageName)) return;
            string newLang = NewLanguageName!.Trim();
            if (TranslationsTable!.Columns.Contains(newLang)) return;

            TranslationsTable.Columns.Add(newLang, typeof(string));

            NewLanguageName = string.Empty;

            InitTranslationsTable(false);
        }

        [RelayCommand]
        private void SaveLanguages()
        {
            EditorHelper.SaveTranslatedStrings(Folders, _game, _mod);
        }

        [RelayCommand]
        private void OpenSearchWindow()
        {
            if (TranslationsTable == null || TranslationsTable.Rows.Count == 0)
            {
                return;
            }

            var searchViewModel = new SearchWindowViewModel(TranslationsTable, this);
            var searchWindow = new SearchWindow { DataContext = searchViewModel };
            searchWindow.Show();
        }

        [RelayCommand]
        private void PasteStringsInSelectedCells()
        {
            if (TranslationsTable == null) return;

            EditorHelper.PasteStringsInSelectedCells(SelectedCells);
        }

        [RelayCommand]
        private void ClearSelectedCells()
        {
            EditorHelper.ClearSelectedCells(SelectedCells);
        }

        [RelayCommand]
        private void CopySelectedCells()
        {
            EditorHelper.CutSelectedCells(SelectedCells, true);
        }

        [RelayCommand]
        private void CutSelectedCells()
        {
            EditorHelper.CutSelectedCells(SelectedCells);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Init Translations table and view
        /// </summary>
        /// <param name="fullInit">when false, will be recreated only DataView. TranslationsTable will not be recreated.</param>
        private void InitTranslationsTable(bool fullInit = true, DataTable? dataTableToRelink = null)
        {
            if (fullInit)
            {
                TranslationsTable = dataTableToRelink ?? new DataTable();
            }

            TranslationsView = new DataView(TranslationsTable);
        }

        private bool IsTheFoldersEnabled()
        {
            return _game != null
                   && _mod != null
                   && Folders.Count > 1;
        }

        private bool IsTheTranslatorEnabled()
        {
            return (_settingsService.SelectedGame != null || _game != null)
                   && (_settingsService.SelectedMod != null || _mod != null);
        }

        private bool IsAddNewLanguageButtonEnabled()
        {
            return _game != null
                   && _mod != null
                   && !string.IsNullOrWhiteSpace(NewLanguageName)
                   && TranslationsTable?.Columns.Count > 0
                   && !TranslationsTable.Columns.Contains(NewLanguageName)
                   && EditorHelper.IsValidFolderName(NewLanguageName);
        }
        #endregion
    }
}