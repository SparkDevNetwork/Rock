using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

using Rock.CloudPrint.Shared;

namespace Rock.CloudPrint.Service;

class NamedPipeService : BackgroundService
{
    private readonly ILogger _logger;

    public NamedPipeService( ILogger<NamedPipeService> logger )
    {
        _logger = logger;
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
                                await pipe.WriteAsync( new PipeStatusResponse { IsConnected = true }, stoppingToken );
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
        catch ( TaskCanceledException ) when ( stoppingToken.IsCancellationRequested )
        {
            return;
        }
    }
}
