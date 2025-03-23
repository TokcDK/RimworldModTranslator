using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RimworldModTranslator.ViewModels
{
    public partial class SettingsViewModel(GameViewModel gameViewModel) : ViewModelBase
    {
        private readonly GameViewModel gameViewModel = gameViewModel;

        public string? GamePath
        {
            get => gameViewModel.GamePath;
            set => gameViewModel.GamePath = value;
        }

        [RelayCommand]
        private void Browse()
        {
            //using var dialog = new FolderBrowserDialog();
            //if (dialog.ShowDialog() == DialogResult.OK)
            //{
            //    GamePath = dialog.SelectedPath;
            //}
        }
    }
}