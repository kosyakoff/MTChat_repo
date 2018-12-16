using NLog.Common;
using NLog.Targets;
using System;

namespace MTChat.Server.Helpers
{
    /// <summary>
    /// Своя цель для nLog. 
    /// Используется для отображения сообщений в окна сервера
    /// </summary>
    [Target("NLogViewer")]
    public class NLogViewerTarget : TargetWithLayout
    {
        public event Action<string> LogReceived;

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            string logMessage = Layout.Render(logEvent.LogEvent);

            LogReceived?.Invoke(logMessage);
        }
    }
}
