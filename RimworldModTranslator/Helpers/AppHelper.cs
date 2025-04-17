using NLog.Config;
using NLog.Targets;
using NLog;
using RimworldModTranslator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Helpers
{
    class AppHelper
    {
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
