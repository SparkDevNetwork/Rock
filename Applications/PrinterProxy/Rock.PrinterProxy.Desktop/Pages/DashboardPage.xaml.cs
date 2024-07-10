using System.IO.Pipes;
using System.ServiceProcess;
using System.Windows.Controls;

using Rock.PrinterProxy.Shared;

using Wpf.Ui.Controls;

namespace Rock.PrinterProxy.Desktop.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private readonly ServiceController? _service;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public DashboardPage()
        {
            InitializeComponent();

            try
            {
                var service = new ServiceController( "Rock Printer Proxy" );

                // Force a check to see if the service is valid.
                _ = service.Status;

                _service = service;
            }
            catch ( InvalidOperationException )
            {
                _service = null;
            }

            UpdateStatus();

            Unloaded += DashboardPage_Unloaded;

            Task.Run( () => RunDashboardAsync( _cancellationTokenSource.Token ), _cancellationTokenSource.Token );
        }

        private void DashboardPage_Unloaded( object sender, System.Windows.RoutedEventArgs e )
        {
            _cancellationTokenSource.Cancel();
        }

        private void UpdateStatus()
        {
            if ( _service == null )
            {
                ServiceAction.Visibility = System.Windows.Visibility.Collapsed;
                ServiceState.Message = "Unable to determine service state, it may not be installed correctly.";
                ServiceState.Severity = InfoBarSeverity.Error;

                return;
            }

            _service.Refresh();

            if ( _service.Status == ServiceControllerStatus.Stopped )
            {
                ServiceAction.Visibility = System.Windows.Visibility.Visible;
                ServiceAction.Content = "Start";
                ServiceState.Message = "Service is stopped.";
                ServiceState.Severity = InfoBarSeverity.Warning;
            }
            else if ( _service.Status == ServiceControllerStatus.Running )
            {
                ServiceAction.Visibility = System.Windows.Visibility.Visible;
                ServiceAction.Content = "Stop";
                ServiceState.Message = "Service is running.";
                ServiceState.Severity = InfoBarSeverity.Success;
            }
            else
            {
                ServiceAction.Visibility = System.Windows.Visibility.Collapsed;
                ServiceState.Message = "Service in unknown state.";
                ServiceState.Severity = InfoBarSeverity.Informational;
            }

            // Until we fix permission requirement.
            ServiceAction.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ServiceAction_Click( object sender, System.Windows.RoutedEventArgs e )
        {
            if ( _service == null )
            {
                return;
            }

            if ( _service.Status == ServiceControllerStatus.Stopped )
            {
                _service.Start();
            }
            else if ( _service.Status == ServiceControllerStatus.Running )
            {
                _service.Stop();
            }
        }

        private async Task RunDashboardAsync( CancellationToken stoppingToken )
        {
            while ( !stoppingToken.IsCancellationRequested )
            {
                try
                {
                    var pipeClient = new NamedPipeClientStream( "RockPrintProxyService" );
                    await pipeClient.ConnectAsync( stoppingToken );
                    var pipe = new PipeObjectStream( pipeClient );

                    while ( !stoppingToken.IsCancellationRequested )
                    {
                        // Do something.

                        await Task.Delay( 1_000, stoppingToken );
                    }
                    //await pipe.WriteAsync( new PipeRequest { Type = 0 } );
                    //var result = await pipe.ReadAsync<PipeStatusResponse>();
                }
                catch ( TaskCanceledException ) when ( stoppingToken.IsCancellationRequested )
                {
                    break;
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( ex );
                }
            }
        }
    }
}
