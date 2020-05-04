using LogTalk.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;

namespace LogTalk
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<System.IO.FileSystemWatcher>();
            containerRegistry.RegisterSingleton<Services.TalkService>();
        }
    }
}
