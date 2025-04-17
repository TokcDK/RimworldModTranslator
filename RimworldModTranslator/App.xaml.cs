using System;
using NLog.Config;
using NLog.Targets;
using NLog;
using RimworldModTranslator.Models;
using System.Windows;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Views;
using RimworldModTranslator.Services;
using RimworldModTranslator.Helpers;

namespace RimworldModTranslator;
 
public partial class App
{
    private readonly SettingsService _settingsService;

    public App()
    {
        _settingsService = new SettingsService();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var view = new MainView();

        AppHelper.SetupLogger(view, _settingsService);

        var mainViewModel = new MainViewModel(_settingsService);
        view.DataContext = mainViewModel;

        view.Show();
    }
}
