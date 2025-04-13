using System;
using NLog.Config;
using NLog.Targets;
using NLog;
using RimworldModTranslator.Models;
using System.Windows;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Views;
using RimworldModTranslator.Services;

namespace RimworldModTranslator;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App
{
    private readonly SettingsService settingsService;

    public App()
    {
        settingsService = new SettingsService();

        var view = new MainView();

        var uiTarget = new UILogTarget
        {
            LogAction = message =>
            {
                view.Dispatcher.Invoke(() =>
                {
                    if (settingsService.Messages.Count > 100)
                    {
                        settingsService.Messages.RemoveAt(0);
                    }
                    settingsService.Messages.Add(message);
                });
            },
            Layout = "${longdate}: (${level}) ${message}"
        };

        var fileTarget = new FileTarget("file")
        {
            FileName = "log.txt",
            Layout = "${longdate}: (${level}) ${message}"
        };

        var config = new LoggingConfiguration();
        config.AddTarget("ui", uiTarget);
        config.AddTarget("file", fileTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, "ui");
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, "file");
        LogManager.Configuration = config;

        var mainViewModel = new MainViewModel(settingsService);
        view.DataContext = mainViewModel;

        view.Show();
    }
}
