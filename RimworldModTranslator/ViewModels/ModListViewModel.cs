using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using System.Collections.ObjectModel;
using RimworldModTranslator.Services;
using System.Collections.Generic;

namespace RimworldModTranslator.ViewModels
{
    public partial class ModListViewModel(SettingsService settingsService) : ViewModelBase
    {
        [ObservableProperty]
        private ModData? selectedMod;

        [ObservableProperty]
        public ObservableCollection<ModData> modsList = [];
    }
}