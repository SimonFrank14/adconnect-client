using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;

namespace ConnectClient.Gui.NLog
{
    [Target("ListView")]
    public class ListViewTarget : TargetWithLayout
    {
        private readonly object lockObject = new object();

        public bool EnableDebugOutput = false;

        public ObservableCollection<LogEventInfo> Events { get; } = new ObservableCollection<LogEventInfo>();

        public ListViewTarget()
        {
            BindingOperations.EnableCollectionSynchronization(Events, lockObject);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent.Level == LogLevel.Debug && EnableDebugOutput == false)
            {
                return;
            }

            lock (lockObject)
            {
                Events.Add(logEvent);
            }
        }
    }
}
