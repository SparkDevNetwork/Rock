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
using System.Security.AccessControl;
using System.Security.Principal;

using Rock.CloudPrint.Shared;

namespace Rock.CloudPrint.Service;

class NamedPipeService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ProxyStatus _status;

    public NamedPipeService( ILogger<NamedPipeService> logger, ProxyStatus status )
    {
        _logger = logger;
        _status = status;
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        try
        {
            while ( !stoppingToken.IsCancellationRequested )
            {
                NamedPipeServerStream pipeServer;

                if ( OperatingSystem.IsWindows() )
                {
                    var pipeSecurity = new PipeSecurity();
                    pipeSecurity.AddAccessRule( new PipeAccessRule(
                        new SecurityIdentifier( WellKnownSidType.BuiltinUsersSid, null ),
                            PipeAccessRights.ReadWrite,
                            AccessControlType.Allow ) );
                    pipeServer = NamedPipeServerStreamAcl.Create( "RockCloudPrintService", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough, 0, 0, pipeSecurity );
                }
                else
                {
                    pipeServer = new NamedPipeServerStream( "RockCloudPrintService", PipeDirection.InOut, 1 );
                }

                try
                {
                    await pipeServer.WaitForConnectionAsync( stoppingToken );
                    var pipe = new PipeObjectStream( pipeServer );

                    try
                    {
                        while ( !stoppingToken.IsCancellationRequested )
                        {
                            var request = await pipe.ReadAsync<PipeRequest>( stoppingToken );

                            if ( request.Type == 0 )
                            {
                                await pipe.WriteAsync( new PipeStatusResponse
                                {
                                    IsConnected = _status.IsConnected,
                                    StartedDateTime = _status.StartedDateTime,
                                    ConnectedDateTime = _status.ConnectedDateTime,
                                    TotalLabelsPrinted = _status.TotalPrinted
                                }, stoppingToken );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        if ( ex is not IOException && ex is not EndOfStreamException )
                        {
                            _logger.LogError( ex, "Error processing command channel." );
                        }

                        pipeServer.Close();
                    }
                }
                finally
                {
                    pipeServer.Dispose();
                }
            }
        }
        catch ( UnauthorizedAccessException )
        {
            return; 
        }
        catch ( TaskCanceledException ) when ( stoppingToken.IsCancellationRequested )
        {
            return;
        }
    }
}
