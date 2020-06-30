using Autofac;
using ConnectClient.ActiveDirectory;
using ConnectClient.Core.Settings;
using ConnectClient.Core.Sync;
using ConnectClient.Rest;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectClient.Gui.ViewModel
{
    public class ViewModelLocator
    {
        private static IContainer container;

        static ViewModelLocator()
        {
            RegisterServices();
        }

        public static void RegisterServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<SettingsManager>().AsSelf().SingleInstance().OnActivated(x => x.Instance.LoadSettings());

            builder.RegisterType<Client>().As<IClient>().SingleInstance();
            builder.RegisterType<LdapUserProvider>().As<ILdapUserProvider>().SingleInstance();

            builder.RegisterType<SyncEngine>().As<ISyncEngine>().SingleInstance();

            builder.RegisterGeneric(typeof(Logger<>)).As(typeof(ILogger<>));
            builder.RegisterType<NLogLoggerFactory>().AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<Messenger>().As<IMessenger>().SingleInstance();

            builder.RegisterType<MainViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<SettingsViewModel>().AsSelf().SingleInstance().OnActivated(x => x.Instance.LoadSettings());
            builder.RegisterType<AboutViewModel>().AsSelf().SingleInstance();

            container = builder.Build();
        }

        public IMessenger Messenger { get { return container.Resolve<IMessenger>(); } }

        public MainViewModel Main { get { return container.Resolve<MainViewModel>(); } }

        public SettingsViewModel Settings { get { return container.Resolve<SettingsViewModel>(); } }

        public AboutViewModel About { get { return container.Resolve<AboutViewModel>(); } }
    }
}
