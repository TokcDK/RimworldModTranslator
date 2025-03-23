using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;
using System.Collections.ObjectModel;

namespace RimworldModTranslator.ViewModels
{
    public partial class ModListViewModel(GameViewModel gameViewModel) : ViewModelBase
    {
        private readonly GameViewModel gameViewModel = gameViewModel;
        [ObservableProperty]
        private ModData? selectedMod;

        public ObservableCollection<ModData> ModsList => gameViewModel.ModsList;

        [RelayCommand]
        private void EditTranslations()
        {
            if (SelectedMod != null)
            {
                WeakReferenceMessenger.Default.Send(new NavigateToTranslationEditorMessage(SelectedMod));
            }
        }
    }
}