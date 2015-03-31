using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MugenMvvmToolkit;
using MugenMvvmToolkit.Infrastructure;
using Tracker.Core.ViewModels;

namespace Tracker.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            new Bootstrapper<LoginViewModel>(this, new AutofacContainer());
        }
    }
}
