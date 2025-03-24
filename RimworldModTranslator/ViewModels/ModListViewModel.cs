using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using System.Collections.ObjectModel;

namespace RimworldModTranslator.ViewModels
{
    public partial class ModListViewModel(SettingsViewModel settingsViewModel) : ViewModelBase
    {
        private readonly SettingsViewModel settingsViewModel = settingsViewModel;

        [ObservableProperty]
        private ModData? selectedMod;

        public ObservableCollection<ModData> ModsList => settingsViewModel.ModsList;
    }
}