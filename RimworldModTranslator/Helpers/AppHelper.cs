using NLog;
using NLog.Config;
using NLog.Targets;
using RimworldModTranslator.Models;
using RimworldModTranslator.Translations;
using System.IO;

namespace RimworldModTranslator.Helpers
{
    class AppHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static void OpenLogFile()
        {
            var logFilePath = Logger.Factory.Configuration.FindTargetByName<FileTarget>("file")?.FileName.Render(new LogEventInfo());
            if (logFilePath != null && File.Exists(logFilePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", logFilePath);
            }
            else
            {
                Logger.Error(Translation.LogFileNotFound, logFilePath);
            }
        }

        internal static void SetupLogger(Views.MainView view, Services.SettingsService settingsService)
        {
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
        }
    }
}
