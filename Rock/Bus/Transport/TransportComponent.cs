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
using MassTransit;

namespace Rock.Bus.Transport
{
    /// <summary>
    /// Base class for transport components
    /// </summary>
    public abstract class TransportComponent : Extension.Component
    {
        #region Attributes

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
                { BaseAttributeKey.Active, false.ToString() },
                { BaseAttributeKey.Order, 0.ToString() }
            };
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get => 0;
        }

        #endregion Attributes

        #region Abstract Methods

        /// <summary>
        /// Gets the bus control.
        /// </summary>
        /// <param name="configureEndpoints">Call this within your configuration function to add the
        /// endpoints with appropriate queues.</param>
        /// <returns></returns>
        public abstract IBusControl GetBusControl( Action<IBusFactoryConfigurator> configureEndpoints );

        /// <summary>
        /// Gets the send endpoint.
        /// </summary>
        /// <param name="bus">The bus.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        public abstract ISendEndpoint GetSendEndpoint( IBusControl bus, string queueName );

        #endregion Abstract Methods

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportComponent"/> class.
        /// </summary>
        public TransportComponent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportComponent" /> class.
        /// </summary>
        /// <param name="updateAttributes">if set to <c>true</c> then attributes will be loaded from database.</param>
        public TransportComponent( bool updateAttributes )
            : base( updateAttributes )
        {
        }

        #endregion
    }
}
