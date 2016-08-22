using System;
using Fen.DllCopy.Properties;

namespace Fen.DllCopy
{
    public partial class App
    {
        public App()
        {
            StartupUri = Settings.Default.FirstUse
                ? new Uri("SettingsPage.xaml", UriKind.Relative)
                : new Uri("MainWindow.xaml", UriKind.Relative);
        }
    }
}
