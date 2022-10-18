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
using System.Linq;
using System.Security.Claims;
using System.Threading;
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
    [RockInternal]
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
            object topicInstance;

            topicInstance = RealTimeHelper.GetTopicInstance( this, topicIdentifier );

            if ( topicInstance == null )
            {
                throw new HubException( $"RealTime topic '{topicIdentifier}' was not found." );
            }

            var matchingMethods = topicInstance.GetType()
                .GetMethods( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance )
                .Where( m => m.Name.Equals( messageName, StringComparison.OrdinalIgnoreCase ) )
                .ToList();

            if ( matchingMethods.Count <= 0 )
            {
                throw new HubException( $"Message '{messageName}' was not found on topic '{topicIdentifier}'." );
            }
            else if ( matchingMethods.Count > 1 )
            {
                throw new HubException( $"Message '{messageName}' matched multiple methods on topic '{topicIdentifier}'." );
            }

            var mi = matchingMethods[0];
            var methodParameters = mi.GetParameters();
            var parms = new object[methodParameters.Length];

            try
            {
                for ( int i = 0; i < methodParameters.Length; i++ )
                {
                    if ( methodParameters[i].ParameterType == typeof( CancellationToken ) )
                    {
                        parms[i] = CancellationToken.None;
                    }
                    else
                    {
                        parms[i] = parameters[i].ToObject( methodParameters[i].ParameterType );
                    }
                }
            }
            catch
            {
                throw new HubException( $"Incorrect parameters passed to message '{messageName}'." );
            }

            var result = mi.Invoke( topicInstance, parms );

            if ( result is Task resultTask )
            {
                await resultTask;

                // Task<T> is not covariant, so we can't just cast to Task<object>.
                if ( resultTask.GetType().GetProperty( "Result" ) != null )
                {
                    result = ( ( dynamic ) resultTask ).Result;
                }
                else
                {
                    result = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Requests that the client be connected to the specified topic.
        /// </summary>
        /// <param name="topicIdentifier">The identifier of the topic to be connected to.</param>
        public async Task ConnectToTopic( string topicIdentifier )
        {
            await RealTimeHelper.Engine.ConnectToTopic( this, topicIdentifier, Context.ConnectionId );
        }

        /// <inheritdoc/>
        public override Task OnConnected()
        {
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
                        Groups.Add( Context.ConnectionId, $"rock:person:{personId}" );
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
                        Groups.Add( Context.ConnectionId, $"rock:visitor:{visitorId}" );
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task OnDisconnected( bool stopCalled )
        {
            await RealTimeHelper.Engine.ClientDisconnected( this, Context.ConnectionId );
        }
    }
}
