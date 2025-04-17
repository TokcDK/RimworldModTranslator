using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using RimworldModTranslator.Services;
using System.Collections.ObjectModel;
using RimworldModTranslator.Helpers;
using RimworldModTranslator.Messages;
using NLog;
using RimworldModTranslator.Translations;
using System.IO;
using NLog.Targets;

namespace RimworldModTranslator.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IRecipient<LoadSelectedModStringsMessage>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ObservableCollection<string> Messages { get => _settingsService!.Messages; }

        public ObservableCollection<ViewModelBase> TabViewModels { get; } = []; 

        [ObservableProperty]
        private ViewModelBase _selectedTab;

        private readonly SettingsService? _settingsService;

        public MainViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;

            Logger.Info(Translation.AppStartedLogMessage);

            // TabViewModels.Add(new WelcomeViewModel(settingsService)); // Not implemented yet, add there some info possibly
            TabViewModels.Add(new ModListViewModel(settingsService));
            TabViewModels.Add(new TranslationEditorViewModel(settingsService));
            TabViewModels.Add(new SettingsViewModel(settingsService));

            _selectedTab = TabViewModels[2]; // Select the settings tab by default

            WeakReferenceMessenger.Default.Register<LoadSelectedModStringsMessage>(this);
        }

        async void IRecipient<LoadSelectedModStringsMessage>.Receive(LoadSelectedModStringsMessage message)
        {
            if (TabViewModels[1] is not TranslationEditorViewModel editor) return;

            // switch to editor tab
            SelectedTab = editor;

            // load strings in editor
            await editor.LoadTheSelectedModStrings();
        }

        [RelayCommand]
        private static void OpenLogFile()
        {
            var logFilePath = Logger.Factory.Configuration.FindTargetByName<FileTarget>("file")?.FileName.Render(new LogEventInfo());
            if (logFilePath != null && File.Exists(logFilePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", logFilePath);
            }
            else
            {
                Logger.Error(Translation.LogFileNotFound, logFilePath);
            }
        }
    }
}