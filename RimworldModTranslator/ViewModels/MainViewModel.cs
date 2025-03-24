using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using RimworldModTranslator.Services;
using System.Collections.ObjectModel;
using RimworldModTranslator.Helpers;

namespace RimworldModTranslator.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ViewModelBase> TabViewModels { get; } = [];

        private readonly SettingsService settingsService;

        [ObservableProperty]
        private ViewModelBase selectedTab;

        public MainViewModel()
        {
            settingsService = new SettingsService();
            TabViewModels.Add(new ModListViewModel(settingsService));
            TabViewModels.Add(new TranslationEditorViewModel(settingsService));
            TabViewModels.Add(new SettingsViewModel(settingsService));

            selectedTab = TabViewModels[2]; // Select the settings tab by default
        }
    }
}