using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MugenMvvmToolkit;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models.IoC;
using Tracker.Core.Services;
using Tracker.Core.ViewModels;
using Tracker.WPF.Services;

namespace Tracker.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var container = new AutofacContainer();
            container.BindToConstant(typeof(IServerService), new ServerService("http://alexv.changeip.net/"));
            container.Bind<IDialogService, DialogService>(DependencyLifecycle.SingleInstance);
            new Bootstrapper<LoginViewModel>(this, container);
        }
    }
}
