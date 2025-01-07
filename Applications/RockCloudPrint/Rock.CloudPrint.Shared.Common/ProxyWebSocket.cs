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
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rock.CloudPrint.Shared
{
    /// <summary>
    /// Wraps a <see cref="WebSocket"/> in a way that provides a common protocol
    /// between client and server for the cloud print system.
    /// </summary>
    internal abstract class ProxyWebSocket
    {
        #region Events

        /// <summary>
        /// Triggered when the underlying web socket has been closed.
        /// </summary>
#if NET
        public event EventHandler? Closed;
#else
        public event EventHandler Closed;
#endif

        #endregion

        #region Fields

        /// <summary>
        /// The underlying web socket we communicate over.
        /// </summary>
        private readonly WebSocket _socket;

        /// <summary>
        /// Only one thread can call SendAsync on the socket at one time. This
        /// is used to ensure only one thread can send concurrently.
        /// </summary>
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim( 1 );

        /// <summary>
        /// This contains the message identifiers that have been sent and are
        /// waiting for responses to be returned.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, IPendingResponse> _pendingResponses = new ConcurrentDictionary<Guid, IPendingResponse>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyWebSocket"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="WebSocket"/> object to be used for proxy communication.</param>
        public ProxyWebSocket( WebSocket socket )
        {
            _socket = socket;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Runs the communication loop for this socket. This reads any incoming
        /// messages and then calls the approriate message handler.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        public async Task RunAsync( CancellationToken cancellationToken )
        {
            var messageBuffer = new byte[1024 * 1024]; // 1MB max size.
            var messageLength = 0;
            var cts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );

            try
            {
                while ( !cancellationToken.IsCancellationRequested && !_socket.CloseStatus.HasValue )
                {
                    if ( messageLength >= messageBuffer.Length )
                    {
                        _socket.Abort();

                        return;
                    }

                    var buffer = new ArraySegment<byte>( messageBuffer, messageLength, messageBuffer.Length - messageLength );
                    var result = await _socket.ReceiveAsync( buffer, cancellationToken );

                    messageLength += result.Count;

                    if ( cancellationToken.IsCancellationRequested || result.CloseStatus.HasValue || result.MessageType == WebSocketMessageType.Close )
                    {
                        break;
                    }

                    if ( result.EndOfMessage )
                    {
                        var data = new byte[messageLength];
                        Array.Copy( messageBuffer, 0, data, 0, messageLength );
                        messageLength = 0;

                        await OnMessageAsync( data, result.MessageType, cancellationToken );
                    }
                }

                await _socket.CloseAsync( WebSocketCloseStatus.NormalClosure, "Closing", cts.Token );
            }
            catch ( WebSocketException ex )
            {
                // Swallow websocket errors, but log them to the debugger.
                System.Diagnostics.Debug.WriteLine( $"{typeof( ProxyWebSocket ).FullName} [{ex.GetType().Name}]: {ex.Message}" );
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"{typeof( ProxyWebSocket ).FullName} [{ex.GetType().Name}]: {ex.Message}" );
                await _socket.CloseAsync( WebSocketCloseStatus.InternalServerError, ex.Message, cancellationToken );

                return;
            }
            finally
            {
                OnClosed();
                cts.Cancel();
            }
        }

        /// <summary>
        /// Posts a message to the remote endpoint. This sends the message
        /// without waiting for a response. The returned <see cref="Task"/>
        /// indicates when the message has been sent, not that it was
        /// received by the remote endpoint.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        public Task PostMessageAsync( CloudPrintMessage message, CancellationToken cancellationToken )
        {
            return PostMessageAsync( message, null, cancellationToken );
        }

        /// <summary>
        /// Posts a message to the remote endpoint. This sends the message
        /// without waiting for a response. The returned <see cref="Task"/>
        /// indicates when the message has been sent, not that it was
        /// received by the remote endpoint.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="extraData">Extra data to be sent after the message in binary format.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
#if NET
        public Task PostMessageAsync( CloudPrintMessage message, byte[]? extraData, CancellationToken cancellationToken )
#else
        public Task PostMessageAsync( CloudPrintMessage message, byte[] extraData, CancellationToken cancellationToken )
#endif
        {
            // Force as object so JSON.Net doesn't use the type information
            // from the generic and ignore properties.
            var json = JsonSerializer.Serialize<object>( message );
            var dataLength = Encoding.UTF8.GetByteCount( json );
            var extraDataLength = extraData?.Length ?? 0;
            var data = new byte[1 + dataLength + extraDataLength];

            data[0] = ( byte ) message.Type;
            Encoding.UTF8.GetBytes( json, 0, json.Length, data, 1 );

            if ( extraDataLength > 0 && extraData != null )
            {
                Array.Copy( extraData, 0, data, 1 + dataLength, extraDataLength );
            }

            return SendAsync( data, WebSocketMessageType.Binary, cancellationToken );
        }

        /// <summary>
        /// Posts a response message to the remote endpoint. The returned
        /// <see cref="Task"/> indicates when the response has been sent, not
        /// that it was received by the remote endpoint.
        /// </summary>
        /// <param name="message">The original message associated with the response.</param>
        /// <param name="response">The response to be posted.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
#if NET
        protected Task PostResponseAsync( CloudPrintMessage message, object? response, CancellationToken cancellationToken )
#else
        protected Task PostResponseAsync( CloudPrintMessage message, object response, CancellationToken cancellationToken )
#endif
        {
            var messageResponse = new CloudPrintMessageResponse( message )
            {
                Result = response
            };

            return PostMessageAsync( messageResponse, cancellationToken );
        }

        /// <summary>
        /// Sends a message to the remote endpoint and waits for a response to
        /// be sent back. The returned <see cref="Task"/> indicates that the
        /// response was received.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        public Task<T> SendMessageAsync<T>( CloudPrintMessage message, CancellationToken cancellationToken )
        {
            return SendMessageAsync<T>( message, null, cancellationToken );
        }

        /// <summary>
        /// Sends a message to the remote endpoint and waits for a response to
        /// be sent back. The returned <see cref="Task"/> indicates that the
        /// response was received.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <param name="extraData">Extra data to be sent after the message in binary format.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
#if NET
        public async Task<T> SendMessageAsync<T>( CloudPrintMessage message, byte[]? extraData, CancellationToken cancellationToken )
#else
        public async Task<T> SendMessageAsync<T>( CloudPrintMessage message, byte[] extraData, CancellationToken cancellationToken )
#endif
        {
            // Force abort if we haven't heard back in 60s.
            var cts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
            cts.CancelAfter( 60_000 );

            var pendingResponse = new PendingResponse<T>( message.Id, typeof( T ), cts.Token );
            _pendingResponses.AddOrUpdate( message.Id, ( id, pr ) => pr, ( id, old, pr ) => pr, pendingResponse );

            try
            {
                await PostMessageAsync( message, extraData, cts.Token );

                return await pendingResponse.Task;
            }
            finally
            {
                _pendingResponses.TryRemove( message.Id, out _ );
            }
        }

        /// <summary>
        /// Aborts the connection and cancels any pending IO operations.
        /// </summary>
        public void Abort()
        {
            _socket.Abort();
        }

        /// <summary>
        /// Performs a clean closure of the socket.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        public Task CloseAsync( CancellationToken cancellationToken )
        {
            return _socket.CloseAsync( WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken );
        }

        /// <summary>
        /// Sends data over a socket while ensuring that only one send operation
        /// can happen concurrently.
        /// </summary>
        /// <param name="data">The byte array containing the data to send.</param>
        /// <param name="messageType">The type of message being sent (text, binary, close).</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        private async Task SendAsync( byte[] data, WebSocketMessageType messageType, CancellationToken cancellationToken )
        {
            await _sendLock.WaitAsync( cancellationToken );

            try
            {
                await _socket.SendAsync( new ArraySegment<byte>( data ), messageType, true, cancellationToken );
            }
            finally
            {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// Called when the socket has closed for any reason.
        /// </summary>
        protected virtual void OnClosed()
        {
            Closed?.Invoke( this, EventArgs.Empty );
        }

        /// <summary>
        /// Returns a subclass of <see cref="CloudPrintMessageType"/> based
        /// on JSON data inside <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data that represents the start of the message.</param>
        /// <param name="messageType">The type of message represented by <paramref name="data"/>.</param>
        /// <returns>The created message object.</returns>
        private static (CloudPrintMessage, int) GetMessageObject( ReadOnlySpan<byte> data, CloudPrintMessageType messageType )
        {
            if ( !CloudPrintMessage.MessageLookup.TryGetValue( messageType, out var type ) || type == null )
            {
                throw new Exception( "Invalid message received." );
            }

            var reader = new Utf8JsonReader( data );
            var message = JsonSerializer.Deserialize( ref reader, type ) as CloudPrintMessage
                ?? throw new Exception( "Invalid message received." );

            return (message, ( int ) reader.BytesConsumed);
        }

#if NET
        private static void SetResponse( IPendingResponse pendingResponse, object? response )
        {
            object? value = null;

            if ( response is JsonElement element )
            {
                value = element.Deserialize( pendingResponse.ResponseType );
            }

            // Run this on another task so we don't wait for
            // the response to be processed.
            _ = Task.Run( () => pendingResponse.TrySetResult( value ) );
        }
#else
        /// <summary>
        /// Sets the response on the pending response. This will release the
        /// task waiting on the response to complete.
        /// </summary>
        /// <param name="pendingResponse">The pending response object to set the response on.</param>
        /// <param name="response">The response object to set.</param>
        private static void SetResponse( IPendingResponse pendingResponse, object response )
        {
            object value = null;

            if ( response is JsonElement element )
            {
                using ( var ms = new System.IO.MemoryStream() )
                {
                    using ( var writer = new Utf8JsonWriter( ms ) )
                    {
                        element.WriteTo( writer );
                    }

                    value = JsonSerializer.Deserialize( ms.ToArray(), pendingResponse.ResponseType );
                }
            }

            // Run this on another task so we don't wait for
            // the response to be processed.
            _ = Task.Run( () => pendingResponse.TrySetResult( value ) );
        }
#endif

        /// <summary>
        /// Called whenever a message has been received in full from the socket.
        /// This will validate the message and then call the parsed message
        /// handler.
        /// </summary>
        /// <param name="data">The byte array containing the message data.</param>
        /// <param name="type">The type of the message received.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        private Task OnMessageAsync( byte[] data, WebSocketMessageType type, CancellationToken cancellationToken )
        {
            if ( type == WebSocketMessageType.Binary && data.Length >= 1 )
            {
                var messageType = ( CloudPrintMessageType ) data[0];
                var messageData = new ReadOnlySpan<byte>( data, 1, data.Length - 1 );
                var (message, consumed) = GetMessageObject( messageData, messageType );

                if ( message is CloudPrintMessageResponse response )
                {
                    if ( _pendingResponses.TryGetValue( response.Id, out var pendingResponse ) )
                    {
                        SetResponse( pendingResponse, response.Result );
                    }

                    return Task.CompletedTask;
                }
                else
                {
                    var extraData = new ReadOnlyMemory<byte>( data, 1 + consumed, data.Length - consumed - 1 );

                    return OnMessageAsync( message, extraData, cancellationToken );
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Called when a message has been received and successfully parsed.
        /// This should process the message and send any responses as required.
        /// </summary>
        /// <param name="message">The message to be processed.</param>
        /// <param name="extraData">Additional data that was sent after the message.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that indicates when the operation has completed.</returns>
        protected abstract Task OnMessageAsync( CloudPrintMessage message, ReadOnlyMemory<byte> extraData, CancellationToken cancellationToken );

#endregion

        #region Support Types

        private interface IPendingResponse
        {
            CancellationToken CancellationToken { get; }

            Type ResponseType { get; }

#if NET
            bool TrySetResult( object? result );
#else
            bool TrySetResult( object result );
#endif
        }

        private class PendingResponse<T> : IPendingResponse
        {
            protected readonly TaskCompletionSource<T> _response = new TaskCompletionSource<T>();

            public Guid Id { get; }

            public Type ResponseType { get; }

            public CancellationToken CancellationToken { get; }

            public Task<T> Task => _response.Task;

            public PendingResponse( Guid id, Type responseType, CancellationToken cancellationToken )
            {
                Id = id;
                ResponseType = responseType;
                CancellationToken = cancellationToken;

                cancellationToken.Register( () => _response.TrySetCanceled() );
            }

#if NET
            public bool TrySetResult( object? result )
            {
                return _response.TrySetResult( ( T ) result! );
            }
#else
            public bool TrySetResult( object result )
            {
                return _response.TrySetResult( ( T ) result );
            }
#endif
        }

        #endregion
    }
}
