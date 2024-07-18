// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.IO.Pipes;
using System.ServiceProcess;
using System.Windows.Controls;

using Rock.CloudPrint.Shared;

using Wpf.Ui.Controls;

namespace Rock.CloudPrint.Desktop.Pages
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
                var service = new ServiceController( "Rock Cloud Print" );

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
                    var pipeClient = new NamedPipeClientStream( "RockCloudPrintService" );
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
