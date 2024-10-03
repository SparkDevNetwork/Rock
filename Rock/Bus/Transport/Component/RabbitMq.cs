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
using System.ComponentModel;
using System.ComponentModel.Composition;
using MassTransit;
using Rock.Attribute;
using Rock.Security;

namespace Rock.Bus.Transport
{
    /// <summary>
    /// Bus Transport using RabbitMQ
    /// </summary>
    [Description( "Use RabbitMq as the bus transport" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "RabbitMQ" )]

    [TextField(
        "User",
        Description = "Enter the username for the Rabbit MQ server.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.User )]

    [TextField(
        "Host",
        Description = "Enter the host URL for the Rabbit MQ server.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Host )]

    [EncryptedTextField(
        "Password",
        Description = "Enter the password for the Rabbit MQ server.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Password )]

    [Rock.SystemGuid.EntityTypeGuid( "64437280-5BB3-4274-B39F-EE31782438DE")]
    public class RabbitMq : TransportComponent
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Component Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string User = "User";
            public const string Password = "Password";
            public const string Host = "Host";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Gets the bus control.
        /// </summary>
        /// <param name="configureEndpoints">Call this within your configuration function to add the
        /// endpoints with appropriate queues.</param>
        /// <returns></returns>
        public override IBusControl GetBusControl( Action<IBusFactoryConfigurator> configureEndpoints )
        {
            return MassTransit.Bus.Factory.CreateUsingRabbitMq( configurator =>
            {
                var user = GetUser();
                var url = $"amqps://{user}:{GetPassword()}@{GetHost()}/{user}";
                configurator.Host( new Uri( url ), host => { } );
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
            /* 
                 4/6/2022 - SMC

                 This URL should probably be changed to use the amqps:// protocol prefix (like
                 the one used in the GetBusControl() method above) , instead of rabbitmq://
                 with the TLS port number (5671) hard-coded, but it is being left in a
                 known-working configuration for the moment.

            */

            var address = GetDestinationAddressForQueue( bus, queueName );
            return bus.GetSendEndpoint( address ).Result;
        }


        /// <inheritdoc/>
        public override Uri GetDestinationAddressForQueue( IBusControl bus, string queueName )
        {
            return new Uri( $"rabbitmq://{GetHost()}:5671/{GetUser()}/{queueName}" );
        }

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <returns></returns>
        private string GetHost()
        {
            return GetAttributeValue( AttributeKey.Host );
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <returns></returns>
        private string GetPassword()
        {
            var encryptedValue = GetAttributeValue( AttributeKey.Password );
            var value = Encryption.DecryptString( encryptedValue );
            return value;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <returns></returns>
        private string GetUser()
        {
            return GetAttributeValue( AttributeKey.User );
        }
    }
}
