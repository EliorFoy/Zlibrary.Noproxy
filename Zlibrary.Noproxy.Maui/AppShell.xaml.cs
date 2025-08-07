using Zlibrary.Noproxy.Maui.Views;

namespace Zlibrary.Noproxy.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("DataBackupPage", typeof(DataBackupPage));
            Routing.RegisterRoute("FluidNavigationDemoPage", typeof(FluidNavigationDemoPage));
        }
    }
}
