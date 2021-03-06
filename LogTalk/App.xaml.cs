﻿using LogTalk.Views;
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
            containerRegistry.RegisterSingleton<Services.IAppSettingsService, Services.AppSettingsService>();
            containerRegistry.RegisterSingleton<Services.IOpenFileDialogService, Services.OpenFileDialogService>();
            containerRegistry.RegisterSingleton<Services.ITextReadService, Services.TextReadService>();
            containerRegistry.RegisterSingleton<Services.ITalkService, Services.TalkService>();
            containerRegistry.RegisterSingleton<Services.ITalkQueueService, Services.TalkQueueService>();

            containerRegistry.Register<Utils.ITranslator, Utils.KatakotoTranslator>();
        }
    }
}
