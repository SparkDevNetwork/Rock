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
using System.Collections.Generic;

namespace Rock.RealTime
{
    /// <summary>
    /// Tracks all possible topic implementations and dynamically creates
    /// instances of those topic proxies.
    /// </summary>
    /// <typeparam name="TProxy"></typeparam>
    internal class TopicProxyFactory<TProxy>
        where TProxy : class
    {
        #region Fields

        /// <summary>
        /// The known proxy factories that are registered.
        /// </summary>
        private readonly Lazy<Dictionary<string, Func<TProxy, string, object>>> _factories;

        /// <summary>
        /// The known topic configurations that should be used when generating
        /// the proxies.
        /// </summary>
        private readonly List<TopicConfiguration> _configurations;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="TopicProxyFactory{TProxy}"/> that
        /// will build proxy instances on request.
        /// </summary>
        public TopicProxyFactory( List<TopicConfiguration> topicConfigurations )
        {
            _configurations = topicConfigurations;
            _factories = new Lazy<Dictionary<string, Func<TProxy, string, object>>>( GetFactories );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all the known proxy factory methods.
        /// </summary>
        /// <returns>A dictionary of topic identifiers and the proxy factory functions.</returns>
        private Dictionary<string, Func<TProxy, string, object>> GetFactories()
        {
            var factories = new Dictionary<string, Func<TProxy, string, object>>();

            foreach ( var configuration in _configurations )
            {
                var proxyBuilderType = typeof( TopicProxyBuilder<,> )
                    .MakeGenericType( configuration.ClientInterfaceType, typeof( TProxy ) );

                var buildFactoryMethod = proxyBuilderType.GetMethod( "BuildFactory" );
                var factory = ( Func<TProxy, string, object> ) buildFactoryMethod.Invoke( null, Array.Empty<object>() );

                factories.Add( configuration.TopicIdentifier, factory );
            }

            return factories;
        }

        /// <summary>
        /// Get the dynamic proxy for the native client proxy and topic identifier.
        /// </summary>
        /// <typeparam name="T">The expected proxy type.</typeparam>
        /// <param name="clientProxy">The native client proxy used to send messages to clients.</param>
        /// <param name="topicIdentifier">The identifier of the topic.</param>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        public T GetDynamicProxy<T>( TProxy clientProxy, string topicIdentifier )
        {
            return ( T ) _factories.Value[topicIdentifier].Invoke( clientProxy, topicIdentifier );
        }

        #endregion
    }
}
