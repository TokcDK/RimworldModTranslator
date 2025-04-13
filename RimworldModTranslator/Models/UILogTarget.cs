using NLog;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimworldModTranslator.Models
{
    public class UILogTarget : Target
    {
        public required Action<string> LogAction { get; set; }
        public Layout? Layout { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            var message = Layout?.Render(logEvent);
            LogAction?.Invoke(message!);
        }
    }
}
