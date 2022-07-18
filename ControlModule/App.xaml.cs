using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Threading;

using ControlModule.Views;
using System.Reflection;
namespace ControlModule
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static Mutex mutex;
        private App()
        {
            InitializeComponent();
        }
        [STAThread]
        private static void Main()
        {
            try
            {
                Mutex.OpenExisting("ControlModule");
                MessageBox.Show("Application Running...", "Saoviet Loading System", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            catch
            {
                App.mutex = new Mutex(true, "ControlModule");
                App app = new App();
                app.Run(new MainWindow());
                mutex.ReleaseMutex();
            }

            //Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
            //// Get the connectionStrings section. 
            //ConfigurationSection configSection = config.GetSection("connectionStrings");
            ////Ensures that the section is not already protected.
            //if (configSection.SectionInformation.IsProtected == false)
            //{
            //    //Uses the Windows Data Protection API (DPAPI) to encrypt the configuration section using a machine-specific secret key.
            //    configSection.SectionInformation.ProtectSection(
            //        "DataProtectionConfigurationProvider");
            //    config.Save();
            //}
        }
    }
}
