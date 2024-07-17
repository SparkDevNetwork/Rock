using System.Windows;

namespace Rock.CloudPrint.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup( StartupEventArgs e )
        {
#if RELEASE
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal( identity );
            var assembly = System.Reflection.Assembly.GetEntryAssembly();

            if ( assembly != null && !principal.IsInRole( System.Security.Principal.WindowsBuiltInRole.Administrator ) )
            {
                var objProcessInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = System.IO.Path.Join( AppContext.BaseDirectory, "Rock.CloudPrint.Desktop.exe" ),
                    Verb = "runas"
                };

                try
                {
                    var proc = System.Diagnostics.Process.Start( objProcessInfo );
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
