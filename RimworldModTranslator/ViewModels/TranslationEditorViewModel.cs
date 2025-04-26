using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using RimworldModTranslator.Helpers;
using RimworldModTranslator.Messages;
using RimworldModTranslator.Models;
using RimworldModTranslator.Services;
using RimworldModTranslator.Translations;
using RimworldModTranslator.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace RimworldModTranslator.ViewModels
{
    #region Info
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
    #endregion

    public partial class TranslationEditorViewModel : ViewModelBase, IRecipient<ChangedEditorAutosaveTimePeriodSettingMessage>
    {
        #region Fields
        private System.Timers.Timer? _autoSaveTimer;
        private ModData? _mod;
        private readonly SettingsService _settingsService;
        private string? _previousSelectedFolder;
        #endregion

        #region Static Properties - General
        public static string Header { get => Translation.EditorName; }
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        #endregion

        #region Static Properties - Control nanes and tooltips
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

        #region Static Properties - Menu names and tooltips
        public static string CutSelectedRowsName { get => Translation.CutSelectedRowsName; }
        public static string CutSelectedRowsToolTip { get => Translation.CutSelectedRowsToolTip; }
        public static string CopySelectedRowsName { get => Translation.CopySelectedRowsName; }
        public static string CopySelectedRowsToolTip { get => Translation.CopySelectedRowsToolTip; }
        public static string PasteToSelectedRowsName { get => Translation.PasteToSelectedRowsName; }
        public static string PasteToSelectedRowsToolTip { get => Translation.PasteToSelectedRowsToolTip; }
        public static string ClearSelectedRowsName { get => Translation.ClearSelectedRowsName; }
        public static string ClearSelectedRowsToolTip { get => Translation.ClearSelectedRowsToolTip; }
        public static string SaveModDBName { get => Translation.SaveModDBName; }
        public static string SaveModDBToolTip { get => Translation.SaveModDBToolTip; }
        public static string LoadModDBName { get => Translation.LoadModDBName; }
        public static string LoadModDBToolTip { get => Translation.LoadModDBToolTip; }
        public static string LoadModDBReplaceName { get => Translation.LoadModDBReplaceName; }
        public static string LoadModDBReplaceToolTip { get => Translation.LoadModDBReplaceToolTip; }
        public static string ClearSortName { get => Translation.ClearSortName; }
        public static string ClearSortToolTip { get => Translation.ClearSortToolTip; }
        #endregion

        #region Properties
        public bool IsTranslatorEnabled { get => IsTheTranslatorEnabled(); }
        public bool IsAddNewLanguageEnabled { get => IsAddNewLanguageButtonEnabled(); }
        public bool IsFoldersEnabled { get => IsTheFoldersEnabled(); }
        public string? ModDisplayingName => _mod != null && Folders.Count > 0 ? _mod.ModDisplayingName : _settingsService.SelectedMod?.ModDisplayingName;
        public ObservableCollection<FolderData> Folders { get; } = new();
        private IList<DataGridCellInfo>? selectedCells;
        public IList<DataGridCellInfo>? SelectedCells
        {
            get => selectedCells;
            set => SetProperty(ref selectedCells, value);
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

        [ObservableProperty]
        private ListCollectionView? translationsColl;

        // will not fire with SelectionUnit="CellOrRowHeader" selected cells but still can be used to select row programmatically
        [ObservableProperty]
        private DataRowView? selectedRow;

        [ObservableProperty]
        private int selectedRowIndex;
        #endregion

        #region Constructors
        public TranslationEditorViewModel(SettingsService settingsService)
        {
            this._settingsService = settingsService;
            Folders.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsFoldersEnabled));
            InitTranslationsTable();
        }
        #endregion

        #region Commands
        [RelayCommand]
        private void LoadStrings()
        {
            if (!EditorHelper.LoadStringsInitModData(ref _mod, _settingsService))
            {
                return;
            }
            LoadTheSelectedModStrings(_mod!);
        }

        [RelayCommand]
        private void ClearSort()
        {
            EditorHelper.ClearSort(TranslationsColl);
        }

        [RelayCommand]
        private async Task LoadStringsCache()
        {
            bool previousLoadOnlyStringsForExtractedIds = Properties.Settings.Default.LoadOnlyStringsForExtractedIds;
            Properties.Settings.Default.LoadOnlyStringsForExtractedIds = false;
            await EditorHelper.LoadStringsCacheInternal(Folders, _mod, _settingsService);
            Properties.Settings.Default.LoadOnlyStringsForExtractedIds = previousLoadOnlyStringsForExtractedIds;
        }

        [RelayCommand]
        private void SaveStrings()
        {
            SaveLanguages();
        }

        [RelayCommand]
        private void AddNewLanguage()
        {
            if (_mod == null || string.IsNullOrEmpty(NewLanguageName)) return;
            string newLang = NewLanguageName!.Trim();
            if (TranslationsTable!.Columns.Contains(newLang)) return;

            TranslationsTable.Columns.Add(newLang, typeof(string));
            NewLanguageName = string.Empty;
            InitTranslationsTable(false);
        }

        [RelayCommand]
        private void SaveModDB()
        {
            EditorHelper.SaveModDB(Folders, _mod);
            RestartAutosave(); // restart because db was save just now
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
        private void StartAutoSave()
        {
            if (Properties.Settings.Default.EditorAutosaveTimePeriod < 1)
            {
                return;
            }

            _autoSaveTimer = new System.Timers.Timer(Properties.Settings.Default.EditorAutosaveTimePeriod * 1000);
            _autoSaveTimer.Elapsed += (s, e) => SaveModDB();
            _autoSaveTimer.AutoReset = true;
            _autoSaveTimer.Start();

            WeakReferenceMessenger.Default.Register<ChangedEditorAutosaveTimePeriodSettingMessage>(this);
        }

        private void StopAutoSave()
        {
            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.Stop();
                _autoSaveTimer.Dispose();
                _autoSaveTimer = null;

                WeakReferenceMessenger.Default.Unregister<ChangedEditorAutosaveTimePeriodSettingMessage>(this);
            }
        }

        private void RestartAutosave()
        {
            StopAutoSave();
            StartAutoSave();
        }

        private void SaveLanguages()
        {
            if (_mod == null) return;

            SaveModDB();

            var translationMod = EditorHelper.SaveTranslatedStrings(Folders, _mod);
            if (translationMod == null)
            {
                return;
            }

            GameHelper.TryExploreDirectory(Path.Combine(translationMod.ParentGame.ModsDirPath!, translationMod!.DirectoryName!));

            if (!GameHelper.SortMod(translationMod, _mod)) return;

            GameHelper.UpdateSharedModList(_settingsService.ModsList, _mod.ParentGame.ModsList);
        }

        public void LoadTheSelectedModStrings(ModData mod)
        {
            StopAutoSave(); // stop auto-save timer if it was started

            _mod = mod; // retarget translating mod

            if (!EditorHelper.LoadModTranslatableFolders(mod, Folders))
            {
                return;
            }

            // check folders to parse
            if (Folders.Count == 0)
            {
                Logger.Warn(Translation.NoTranslatableFoldersFoundLogMessage);
                return;
            }

            EditorHelper.LoadStringsForAllFolders(Folders, mod);
            if (!Folders.Any(f => f.StringsData != null && f.StringsData.loadedStringsCount > 0))
            {
                Logger.Warn(Translation.NothingToTranslateLogMessage);
                return;
            }

            // insert all in foldet as first
            var AllInFolder = new FolderData
            {
                Name = EditorHelper.ALL_IN_FOLDER_NAME,
            };
            Folders.Insert(0, AllInFolder);

            // select 1st dir when not selected
            SelectedFolder ??= Folders.FirstOrDefault();

            // for overall table
            AllInFolder.TranslationsTable = EditorHelper.CreateTranslationsTable(null, Folders);
            var supportedVersions = EditorHelper.EnumerateSupportedVersions(Folders);
            AllInFolder.SupportedVersions = supportedVersions.ToList();
            // EditorHelper.RemoveAllButFirstFolder(Folders); // save for later using datas
            foreach (var folder in Folders.Skip(1))
            {
                // reset datas, need only supported versions
                folder.StringsData = null;
                folder.TranslationsTable = null;
            }

            // load mod db if exist
            EditorHelper.LoadModDB(Folders, mod);

            // refresh selected table
            InitTranslationsTable(dataTableToRelink: SelectedFolder?.TranslationsTable);

            OnPropertyChanged(nameof(ModDisplayingName));

            // Call StartAutoSave() to enable the auto-save functionality
            StartAutoSave();
        }

        private void InitTranslationsTable(bool fullInit = true, DataTable? dataTableToRelink = null)
        {
            if (fullInit)
            {
                TranslationsTable = dataTableToRelink ?? new DataTable();
            }

            TranslationsView = new DataView(TranslationsTable);
            // Rebind to fix dotnet/datagridextensions notsupported error
            // maybe later change to use the datatable's filter?
            // it also made the column sort to not react to the cells changing which is better of what was before
            // I lazy to implement it now.. Maybe will be better solution.
            TranslationsColl = new ListCollectionView(TranslationsView);
        }

        private bool IsTheFoldersEnabled()
        {
            return _mod != null
                   && Folders.Count > 1 && Folders[0].Name != EditorHelper.ALL_IN_FOLDER_NAME;
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

        void IRecipient<ChangedEditorAutosaveTimePeriodSettingMessage>.Receive(ChangedEditorAutosaveTimePeriodSettingMessage message)
        {
            RestartAutosave();
        }
        #endregion
    }
}