using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using RimworldModTranslator.Services;
using System.Collections.ObjectModel;

namespace RimworldModTranslator.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ViewModelBase> TabViewModels { get; } = [];

        private readonly SettingsService settingsService;

        public MainViewModel()
        {
            settingsService = new SettingsService();
            TabViewModels.Add(new ModListViewModel(settingsService));
            TabViewModels.Add(new TranslationEditorViewModel(settingsService));
            TabViewModels.Add(new SettingsViewModel(settingsService));
        }
    }
}