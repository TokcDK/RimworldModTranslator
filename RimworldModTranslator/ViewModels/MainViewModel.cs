using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Models;

namespace RimworldModTranslator.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object currentPage;

        private readonly GameViewModel gameViewModel;

        public MainViewModel()
        {
            gameViewModel = new GameViewModel();
            CurrentPage = new ModListViewModel(gameViewModel);
            WeakReferenceMessenger.Default.Register<NavigateToTranslationEditorMessage>(this, OnNavigateToTranslationEditor);
        }

        private void OnNavigateToTranslationEditor(object recipient, NavigateToTranslationEditorMessage message)
        {
            CurrentPage = new TranslationEditorViewModel(message.Mod);
        }

        [RelayCommand]
        private void GoToModList()
        {
            CurrentPage = new ModListViewModel(gameViewModel);
        }

        [RelayCommand]
        private void GoToSettings()
        {
            CurrentPage = new SettingsViewModel(gameViewModel);
        }
    }

    // Navigation message for TranslationEditorPage
    public class NavigateToTranslationEditorMessage(ModData mod)
    {
        public ModData Mod { get; } = mod;
    }
}