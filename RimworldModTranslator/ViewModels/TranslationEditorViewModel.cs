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
using RimworldModTranslator.Translations;
using NLog.Fluent;

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
        private readonly SettingsService _settingsService;
        private string? _previousSelectedFolder;
        #endregion

        #region Constructors
        public TranslationEditorViewModel(SettingsService settingsService)
        {
            this._settingsService = settingsService;
            Folders.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsFoldersEnabled));

            InitTranslationsTable();
        }
        #endregion

        #region ToolTips
        public static string EditorTableToolTip { get => Translation.EditorTableToolTip; }
        public static string FolderSelectionToolTip { get => Translation.FolderSelectionToolTip; }
        public static string AddNewLanguageToolTip { get => Translation.AddNewLanguageToolTip; }
        public static string LoadStringsCacheToolTip { get => Translation.LoadStringsCacheToolTip; }
        public static string LoadStringsName { get => Translation.LoadStringsName; }
        public static string LoadStringsToolTip { get => Translation.LoadStringsToolTip; }
        public static string SaveStringsName { get => Translation.SaveStringsName; }
        public static string SaveStringsToolTip { get => Translation.SaveStringsTooltip; }
        public static string FolderName { get => Translation.FolderName; }
        public static string AddLanguageName { get => Translation.AddLanguageName; }
        #endregion

        #region Properties
        public static string Header { get => Translation.EditorName; }

        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

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

        [ObservableProperty]
        private int selectedRowIndex;
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
            await EditorHelper.LoadStringsCacheInternal(Folders, _mod, _settingsService);
        }

        public Task LoadTheSelectedModStrings()
        {
            bool isChangedMod = _mod != _settingsService.SelectedMod;

            if (isChangedMod || _mod == null)
            {
                // load only when mod was not set or changed
                _mod = _settingsService.SelectedMod;
                if (_mod == null)
                {
                    Logger.Warn(Translation.ModIsNotSetWarnLogMessage);
                    return Task.CompletedTask;
                }
            }

            if (isChangedMod || Folders.Count == 0)
            {
                string modPath = Path.Combine(_mod.ParentGame.ModsDirPath!, _mod.DirectoryName!);
                if (!Directory.Exists(modPath))
                {
                    Logger.Warn(Translation.ModsPathIsNotSetWarnLogMessage);
                    return Task.CompletedTask;
                }

                Folders.Clear();

                EditorHelper.GetTranslatableFolders(Folders, modPath);
            }

            if (Folders.Count == 0)
            {
                Logger.Warn(Translation.NoTranslatableFoldersFoundLogMessage);
                return Task.CompletedTask;
            }

            // Загрузка строк для всех папок
            int totalStringsLoaded = 0;
            foreach (var folder in Folders)
            {
                string folderName = folder.Name;
                string translatableDir = Path.Combine(_mod.ParentGame.ModsDirPath!, _mod!.DirectoryName!, EditorHelper.GetTranslatableFolderName(folderName));

                var stringsData = EditorHelper.LoadStringsDataFromTheLanguageDir(translatableDir);
                var translationsTable = EditorHelper.CreateTranslationsTable(stringsData);

                folder.TranslationsTable = translationsTable;

                if (translationsTable != null && translationsTable.Rows.Count > 0)
                {
                    totalStringsLoaded += translationsTable.Rows.Count;
                    Logger.Info(Translation.Loaded0StringsFrom1LogMessage, translationsTable.Rows.Count, translatableDir);
                }
                else
                {
                    Logger.Info(Translation.NothingToLoadFromXLogMessage, translatableDir);
                }
            }

            // Если папка не выбрана, выберем первую
            SelectedFolder ??= Folders.FirstOrDefault();

            // Загружаем базу данных мода
            EditorHelper.LoadModDB(Folders, _mod);

            // Инициализируем таблицу переводов для выбранной папки
            InitTranslationsTable(dataTableToRelink: SelectedFolder?.TranslationsTable);

            OnPropertyChanged(nameof(ModDisplayingName));

            if (totalStringsLoaded > 0)
            {
                Logger.Info($"Загружено всего {totalStringsLoaded} строк перевода для всех папок мода");
            }

            return Task.CompletedTask;
        }

        [RelayCommand]
        private void SaveStrings()
        {
            if (_mod == null) return;

            SaveLanguages();
        }

        [RelayCommand]
        private void AddNewLanguage()
        {
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
            EditorHelper.SaveTranslatedStrings(Folders, _mod);
        }

        [RelayCommand]
        private void SaveModDB()
        {
            EditorHelper.SaveModDB(Folders, _mod);
        }

        [RelayCommand]
        private void LoadModDB()
        {
            EditorHelper.LoadModDB(Folders, _mod);
            InitTranslationsTable(true, SelectedFolder!.TranslationsTable);
        }

        [RelayCommand]
        private void LoadModDBForce()
        {
            EditorHelper.LoadModDB(Folders, _mod, true);
            InitTranslationsTable(true, SelectedFolder!.TranslationsTable);
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
            return _mod != null
                   && Folders.Count > 1;
        }

        private bool IsTheTranslatorEnabled()
        {
            return (_settingsService.SelectedMod != null || _mod != null);
        }

        private bool IsAddNewLanguageButtonEnabled()
        {
            return _mod != null
                   && !string.IsNullOrWhiteSpace(NewLanguageName)
                   && TranslationsTable?.Columns.Count > 0
                   && !TranslationsTable.Columns.Contains(NewLanguageName)
                   && EditorHelper.IsValidFolderName(NewLanguageName);
        }
        #endregion
    }
}