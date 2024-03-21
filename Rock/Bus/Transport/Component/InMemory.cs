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
using System.ComponentModel;
using System.ComponentModel.Composition;
using MassTransit;

namespace Rock.Bus.Transport
{
    /// <summary>
    /// Bus Transport using InMemory
    /// </summary>
    [Description( "Use InMemory as the bus transport (only works with a single Rock server)" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "InMemory" )]

    [Rock.SystemGuid.EntityTypeGuid( "D6AE6233-BCD4-43BC-9D4B-5D70A0A1A9BB")]
    public class InMemory : TransportComponent
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get => new Dictionary<string, string>
            {
                { BaseAttributeKey.Active, true.ToString() },
                { BaseAttributeKey.Order, 0.ToString() }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemory"/> class.
        /// </summary>
        public InMemory()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemory" /> class.
        /// </summary>
        /// <param name="updateAttributes">if set to <c>true</c> then attributes will be loaded from database.</param>
        public InMemory( bool updateAttributes )
            : base( updateAttributes )
        {
        }

        /// <summary>
        /// Gets the bus control.
        /// </summary>
        /// <param name="configureEndpoints">Call this within your configuration function to add the
        /// endpoints with appropriate queues.</param>
        /// <returns></returns>
        public override IBusControl GetBusControl( Action<IBusFactoryConfigurator> configureEndpoints )
        {
            return MassTransit.Bus.Factory.CreateUsingInMemory( configurator =>
            {
                configureEndpoints( configurator );
            } );
        }

        /// <summary>
        /// Gets the send endpoint.
        /// </summary>
        /// <param name="bus">The bus.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        public override ISendEndpoint GetSendEndpoint( IBusControl bus, string queueName )
        {
            var url = $"{bus.Address.AbsoluteUri}/{queueName}";
            return bus.GetSendEndpoint( new Uri( url ) ).Result;
        }
    }
}
