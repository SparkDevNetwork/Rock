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
using Microsoft.ServiceBus;
using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
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
        Order = 1,
        Key = AttributeKey.AccessKey )]

    [TextField(
        "Message Expiration TimeSpan",
        Description = "Enter an expiration in TimeSpan format 00:00:00:00 (i.e. day:hour:minute:second). If an invalid format is specified the default will be 7 days.",
        IsRequired = true,
        Order = 2,
        DefaultValue = "07:00:00:00",
        Key = AttributeKey.MessageExpiration)]

    [BooleanField(
        "Enable Dead Letter On Message Expiration",
        Description = "Specify true or false to indicate if a message should to go to a dead letter queue if a message expires before it is consumed.",
        IsRequired = true,
        Order = 3,
       DefaultBooleanValue = false,
        Key = AttributeKey.DeadLetterOnMessageExpiration )]

    [Rock.SystemGuid.EntityTypeGuid( "91130C54-D189-4B0B-B8CB-F92B6681A327")]
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
            public const string MessageExpiration = "MessageExpiration";
            public const string DeadLetterOnMessageExpiration = "DeadLetterOnMessageExpiration";
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
            TimeSpan messageExpiration;
            var messageExpirationString = GetAttributeValue( AttributeKey.MessageExpiration );
            messageExpirationString = string.IsNullOrEmpty( messageExpirationString ) ? "07:00:00:00" : messageExpirationString;

            // Catch bad data entries and default to 7 days if so and log it.
            if ( !TimeSpan.TryParse( messageExpirationString, out messageExpiration ) )
            {
                RockLogger.Log.Warning( RockLogDomains.Bus, $"{nameof( AzureServiceBus )}: An invalid Message Expiration TimeSpan value of {messageExpirationString} was specified. Defaulting to 07:00:00:00 (7 days)." );
                messageExpiration = TimeSpan.FromDays( 7 );

                SetAttributeValue( AttributeKey.MessageExpiration, $"{messageExpiration.Days:D2}:{messageExpiration.Hours:D2}:{messageExpiration.Minutes:D2}:{messageExpiration.Seconds:D2}" );
                this.SaveAttributeValue( AttributeKey.MessageExpiration, new RockContext() );
            }
                        
            var enableDeadletterOnMessageExpiration = GetAttributeValue( AttributeKey.DeadLetterOnMessageExpiration ).AsBoolean();

            return MassTransit.Bus.Factory.CreateUsingAzureServiceBus( configurator =>
            {
                var url = $"Endpoint=sb://{GetHost()};SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={GetAccessKey()}";
                configurator.DefaultMessageTimeToLive = messageExpiration;
                configurator.EnableDeadLetteringOnMessageExpiration = enableDeadletterOnMessageExpiration;

                configurator.Host( url, host => { } );

                configureEndpoints( configurator );

                ConfigureBusResources( messageExpiration, enableDeadletterOnMessageExpiration, url );

            } );
        }

        /// <summary>
        /// Configures the queue, topic, and subscription properties after the endpoints have been configured.
        /// </summary>
        /// <param name="messageExpiration"></param>
        /// <param name="enableDeadletterOnMessageExpiration"></param>
        /// <param name="url"></param>
        private static void ConfigureBusResources( TimeSpan messageExpiration, bool enableDeadletterOnMessageExpiration, string url )
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString( url );

            // Get all of the current queues
            var queues = namespaceManager.GetQueues();
            if ( queues != null )
            {
                foreach ( var queue in queues )
                {
                    if ( queue.DefaultMessageTimeToLive != messageExpiration || queue.EnableDeadLetteringOnMessageExpiration != enableDeadletterOnMessageExpiration )
                    {
                        queue.DefaultMessageTimeToLive = messageExpiration;
                        queue.EnableDeadLetteringOnMessageExpiration = enableDeadletterOnMessageExpiration;
                        namespaceManager.UpdateQueue( queue );
                    }
                }
            }

            // Get all of the current topics
            var topics = namespaceManager.GetTopics();
            if ( topics != null )
            {
                foreach ( var topic in topics )
                {
                    if ( topic.DefaultMessageTimeToLive != messageExpiration )
                    {
                        topic.DefaultMessageTimeToLive = messageExpiration;
                        namespaceManager.UpdateTopic( topic );
                    }

                    var subs = namespaceManager.GetSubscriptions( topic.Path );

                    // Get all of the topic subscriptions 
                    if ( subs != null )
                    {
                        foreach ( var sub in subs )
                        {
                            // If the topic specifies a smaller TTL than the subscription, the topic TTL is applied ⬆.

                            if ( sub.EnableDeadLetteringOnMessageExpiration != enableDeadletterOnMessageExpiration )
                            {
                                sub.EnableDeadLetteringOnMessageExpiration = enableDeadletterOnMessageExpiration;

                                namespaceManager.UpdateSubscription( sub );
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the send endpoint.
        /// </summary>
        /// <param name="bus">The bus.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns></returns>
        public override ISendEndpoint GetSendEndpoint( IBusControl bus, string queueName )
        {
            var address = GetDestinationAddressForQueue( bus, queueName );
            return bus.GetSendEndpoint( address ).Result;
        }

        /// <inheritdoc/>
        public override Uri GetDestinationAddressForQueue( IBusControl bus, string queueName )
        {
            return new Uri( $"sb://{GetHost()}{queueName}" );
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
