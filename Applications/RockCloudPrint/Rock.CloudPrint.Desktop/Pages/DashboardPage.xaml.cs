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
        private bool _isRunning = false;
        private PipeStatusResponse? _lastStatus;

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
                ServiceAction.Visibility = System.Windows.Visibility.Collapsed;
            }

            UpdateStatus();

            Unloaded += DashboardPage_Unloaded;

            Task.Run( () => RunPipeAsync( _cancellationTokenSource.Token ), _cancellationTokenSource.Token );
            Task.Run( () => RunDashboardAsync( _cancellationTokenSource.Token ), _cancellationTokenSource.Token );
        }

        private void DashboardPage_Unloaded( object sender, System.Windows.RoutedEventArgs e )
        {
            _cancellationTokenSource.Cancel();
        }

        private void UpdateStatus()
        {
            var status = _lastStatus;

            if ( _isRunning && status != null )
            {
                if ( status.IsConnected )
                {
                    ServiceState.Message = "Service is running and connected.";
                    ServiceState.Severity = InfoBarSeverity.Success;

                    ServiceInformationPanel.Visibility = System.Windows.Visibility.Visible;

                    StartTime.Text = status.StartedDateTime.ToString();
                    ConnectedSince.Text = status.ConnectedDateTime != null ? status.ConnectedDateTime.ToString() : "Unknown";
                    TotalLabels.Text = status.TotalLabelsPrinted.ToString( "n0" );
                }
                else
                {
                    ServiceState.Message = "Service is running but not connected to a Rock server.";
                    ServiceState.Severity = InfoBarSeverity.Warning;

                    ServiceInformationPanel.Visibility = System.Windows.Visibility.Collapsed;
                }

                ServiceAction.Content = "Stop";
            }
            else
            {
                ServiceState.Message = "Service does not appear to be running.";
                ServiceState.Severity = InfoBarSeverity.Error;

                ServiceAction.Content = "Start";
            }
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
                    await Task.Delay( 1_000, stoppingToken );

                    await Dispatcher.InvokeAsync( UpdateStatus );
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

        private async Task RunPipeAsync( CancellationToken stoppingToken )
        {
            while ( !stoppingToken.IsCancellationRequested )
            {
                try
                {
                    var pipeClient = new NamedPipeClientStream( "RockCloudPrintService" );
                    await pipeClient.ConnectAsync( stoppingToken );

                    var pipe = new PipeObjectStream( pipeClient );
                    await UpdateStatusFromServer( pipe, stoppingToken );

                    _isRunning = true;

                    while ( !stoppingToken.IsCancellationRequested )
                    {
                        await UpdateStatusFromServer( pipe, stoppingToken );

                        await Task.Delay( 1_000, stoppingToken );
                    }
                }
                catch ( TaskCanceledException ) when ( stoppingToken.IsCancellationRequested )
                {
                    break;
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( ex );
                }

                _isRunning = false;

                try
                {
                    await Task.Delay( 5_000, stoppingToken );
                }
                catch ( TaskCanceledException ) when ( stoppingToken.IsCancellationRequested )
                {
                    break;
                }
            }
        }

        private async Task UpdateStatusFromServer( PipeObjectStream pipe, CancellationToken cancellationToken )
        {
            await pipe.WriteAsync( new PipeRequest { Type = 0 }, cancellationToken );

            _lastStatus = await pipe.ReadAsync<PipeStatusResponse>( cancellationToken );
        }
    }
}
