using ConnectClient.Gui.NLog;
using NLog.Targets;
using System.Windows;

namespace ConnectClient.Gui
{
    public partial class App : Application
    {
        public App()
        {
            Target.Register<ListViewTarget>("ListView");
        }
    }
}
