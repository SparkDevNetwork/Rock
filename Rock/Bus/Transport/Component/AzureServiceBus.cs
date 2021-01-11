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
using MassTransit.AzureServiceBusTransport;
using Rock.Attribute;
using Rock.Security;

namespace Rock.Bus.Transport
{
    /// <summary>
    /// Bus Transport using Azure Service Bus
    /// </summary>
    [Description( "Use Azure Service Bus as the bus transport" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Azure Service Bus" )]

    [TextField(
        "Host",
        Description = "Enter the host URL for the Azure Service Bus server. Ex: rocksolidchurch.servicebus.windows.net",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Host )]

    [EncryptedTextField(
        "Primary Key",
        Description = "Enter the primary key for the Azure Service Bus server. Ex: 5q8vtDWu5ahWF2WjqwXXANLTRJ++VlRKTInSm75Tcwx=",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.AccessKey )]

    public class AzureServiceBus : TransportComponent
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Component Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string AccessKey = "AccessKey";
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
            return MassTransit.Bus.Factory.CreateUsingAzureServiceBus( configurator =>
            {
                var url = $"Endpoint=sb://{GetHost()};SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={GetAccessKey()}";
                configurator.Host( url, host => { } );
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
            var url = $"sb://{GetHost()}{queueName}";
            return bus.GetSendEndpoint( new Uri( url ) ).Result;
        }

        /// <summary>
        /// Gets the host.
        /// </summary>
        /// <returns></returns>
        private string GetHost()
        {
            return GetAttributeValue( AttributeKey.Host ).EnsureTrailingForwardslash();
        }

        /// <summary>
        /// Gets the access key.
        /// </summary>
        /// <returns></returns>
        private string GetAccessKey()
        {
            var encryptedValue = GetAttributeValue( AttributeKey.AccessKey );
            var value = Encryption.DecryptString( encryptedValue );
            return value;
        }
    }
}
