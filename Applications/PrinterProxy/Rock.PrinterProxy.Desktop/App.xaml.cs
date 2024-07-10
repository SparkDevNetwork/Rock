using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Windows;

namespace Rock.PrinterProxy.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup( StartupEventArgs e )
        {
#if RELEASE
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal( identity );
            var assembly = Assembly.GetEntryAssembly();

            if ( assembly != null && !principal.IsInRole( WindowsBuiltInRole.Administrator ) )
            {
                var objProcessInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = Path.Join( AppContext.BaseDirectory, "Rock.PrinterProxy.Desktop.exe" ),
                    Verb = "runas"
                };

                try
                {
                    var proc = Process.Start( objProcessInfo );
                    Current.Shutdown();
                }
                catch
                {
                }
            }
#endif
        }
    }
}
