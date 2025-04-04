using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using RimworldModTranslator.Services;
using System.Collections.ObjectModel;
using RimworldModTranslator.Helpers;
using RimworldModTranslator.Messages;

namespace RimworldModTranslator.ViewModels
{
    public partial class MainViewModel : ViewModelBase, IRecipient<LoadSelectedModStringsMessage>
    {
        public ObservableCollection<ViewModelBase> TabViewModels { get; } = []; 
        
        private readonly SettingsService settingsService;

        [ObservableProperty]
        private ViewModelBase selectedTab;

        public MainViewModel()
        {
            settingsService = new SettingsService();
            // TabViewModels.Add(new WelcomeViewModel(settingsService)); // Not implemented yet, add there some info possibly
            TabViewModels.Add(new ModListViewModel(settingsService));
            TabViewModels.Add(new TranslationEditorViewModel(settingsService));
            TabViewModels.Add(new SettingsViewModel(settingsService));

            selectedTab = TabViewModels[2]; // Select the settings tab by default

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
    }
}