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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

using Rock.Attribute;

namespace Rock.RealTime.AspNet
{
    /// <summary>
    /// The SignalR hub implementation for SignalR v2 on ASP.Net.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [HubName( "realTime" )]
    [RockInternal( "1.14.1", true )]
    public sealed class RealTimeHub : Hub<IRockHubClientProxy>
    {
        /// <summary>
        /// Posts a message to the specified topic.
        /// </summary>
        /// <param name="topicIdentifier">The identifier of the topic.</param>
        /// <param name="messageName">The name of the message.</param>
        /// <param name="parameters">The parameters to be passed to the message handler.</param>
        /// <returns>The value returned by the message handler.</returns>
        public async Task<object> PostMessage( string topicIdentifier, string messageName, Newtonsoft.Json.Linq.JToken[] parameters )
        {
            try
            {
                return await Engine.ExecuteTopicMessageAsync( this, Context.ConnectionId, topicIdentifier, messageName, parameters, ConvertParameterToken );
            }
            catch ( RealTimeException ex )
            {
                throw new HubException( ex.Message );
            }
            catch ( Exception ex )
            {
                Rock.Model.ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Requests that the client be connected to the specified topic.
        /// </summary>
        /// <param name="topicIdentifier">The identifier of the topic to be connected to.</param>
        public async Task ConnectToTopic( string topicIdentifier )
        {
            var joined = await RealTimeHelper.Engine.ConnectToTopic( this, topicIdentifier, Context.ConnectionId );

            if ( joined )
            {
                await Groups.Add( Context.ConnectionId, Engine.GetQualifiedAllChannelName( topicIdentifier ) );

                if ( Context.User is ClaimsPrincipal claimsPrincipal )
                {
                    var personClaim = claimsPrincipal.Claims.FirstOrDefault( c => c.Type == "rock:person" );

                    // If we have a claim that specifies the logged in PersonId then
                    // add this connection to a special group to track people by their Id.
                    if ( personClaim != null )
                    {
                        var personId = personClaim.Value.AsIntegerOrNull();

                        if ( personId.HasValue )
                        {
                            RealTimeHelper.Engine.ClientAddedToChannel( Context.ConnectionId, topicIdentifier, Engine.GetPersonChannelName( personId.Value ) );
                            await Groups.Add( Context.ConnectionId, Engine.GetQualifiedPersonChannelName( topicIdentifier, personId.Value ) );
                        }
                    }

                    var visitorClaim = claimsPrincipal.Claims.FirstOrDefault( c => c.Type == "rock:visitor" );

                    // If we have a claim that specifies a known visitor then add
                    // this connection to a special group to track visitors by their Id.
                    if ( visitorClaim != null )
                    {
                        var visitorId = Rock.Utility.IdHasher.Instance.GetId( visitorClaim.Value );

                        if ( visitorId.HasValue )
                        {
                            RealTimeHelper.Engine.ClientAddedToChannel( Context.ConnectionId, topicIdentifier, Engine.GetVisitorChannelName( visitorId.Value ) );
                            await Groups.Add( Context.ConnectionId, Engine.GetQualifiedPersonChannelName( topicIdentifier, visitorId.Value ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Requests that the client be disconnected from the specified topic.
        /// </summary>
        /// <param name="topicIdentifier">The identifier of the topic to be disconnected from.</param>
        public async Task DisconnectFromTopic( string topicIdentifier )
        {
            await RealTimeHelper.Engine.DisconnectFromTopicAsync( this, topicIdentifier, Context.ConnectionId );
        }

        /// <summary>
        /// Determines whether this connection is still valid. A connection can
        /// become invalid if the Rock server restarts. In this case the SignalR
        /// connection resumes as a reconnect without ever calling the
        /// <see cref="OnConnected"/> method. This also means all connection
        /// state is lost.
        /// </summary>
        /// <returns><c>true</c> if the connection is valid; otherwise <c>false</c>.</returns>
        public Task<bool> IsConnectionValid()
        {
            return Task.FromResult( RealTimeHelper.Engine.HasConnectionState<EngineConnectionState>( Context.ConnectionId ) );
        }

        /// <inheritdoc/>
        public override async Task OnConnected()
        {
            await RealTimeHelper.Engine.ClientConnectedAsync( this, Context.ConnectionId );
        }

        /// <inheritdoc/>
        public override async Task OnDisconnected( bool stopCalled )
        {
            await RealTimeHelper.Engine.ClientDisconnectedAsync( this, Context.ConnectionId );
        }

        /// <summary>
        /// Converts the JSON token into the target type requested by the engine.
        /// </summary>
        /// <param name="token">The token parameter that should be converted.</param>
        /// <param name="targetType">The type the token should be converted into.</param>
        /// <returns>An object of type <paramref name="targetType"/>.</returns>
        private static object ConvertParameterToken( object token, Type targetType )
        {
            return ( token as Newtonsoft.Json.Linq.JToken ).ToObject( targetType );
        }
    }
}
