using System;
using NLog.Config;
using NLog.Targets;
using NLog;
using RimworldModTranslator.Models;
using System.Windows;
using RimworldModTranslator.ViewModels;
using RimworldModTranslator.Views;

namespace RimworldModTranslator;

/// <summary>  
/// Interaction logic for App.xaml  
/// </summary>  
public partial class App
{
    public App()
    {
        var mainViewModel = new MainViewModel();
        var view = new MainView
        {
            DataContext = mainViewModel
        };

        var uiTarget = new UILogTarget
        {
            LogAction = message =>
            {
                view.Dispatcher.Invoke(() =>
                {
                    if (mainViewModel.Messages.Count > 100)
                    {
                        mainViewModel.Messages.RemoveAt(0);
                    }
                    mainViewModel.Messages.Add(message);
                });
            },
            Layout = "${message}" // Fix: Add Layout
        };

        var fileTarget = new FileTarget("file")
        {
            FileName = "log.txt",
            Layout = "${longdate} ${level} ${message}"
        };

        var config = new LoggingConfiguration();
        config.AddTarget("ui", uiTarget);
        config.AddTarget("file", fileTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, "ui");
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, "file");
        LogManager.Configuration = config;

        var logger = LogManager.GetCurrentClassLogger();
        logger.Info("!!!"); // Test message

        view.Show();
    }
}
