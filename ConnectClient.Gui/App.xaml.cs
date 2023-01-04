using ModernWpf;
using System.Windows;
using System.Windows.Media;

namespace ConnectClient.Gui
{
    public partial class App : Application
    {
        public App()
        {
            ThemeManager.Current.AccentColor = (Color)ColorConverter.ConvertFromString("#0078D7");
        }
    }
}
