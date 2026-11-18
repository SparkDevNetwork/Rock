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

using Microsoft.Extensions.Logging;

using Rock.AI.Provider;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Encrypts secret attribute values that were saved as plain text before
    /// their attribute became an encrypted text field. Safe to run on every
    /// start: values that are already encrypted are left alone.
    /// </summary>
    internal static class SecretAttributeEncryptor
    {
        /// <summary>
        /// Encrypts the plain text values of every registered secret attribute.
        /// Does nothing when no data encryption key is configured.
        /// </summary>
        internal static void EncryptAll()
        {
            if ( !Encryption.TryEncryptString( "x", out _ ) )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                EncryptRegisteredAttributes( rockContext );
            }
        }

        /// <summary>
        /// Encrypts the values of each registered secret attribute. Add new
        /// secret attributes here.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private static void EncryptRegisteredAttributes( RockContext rockContext )
        {
            EncryptComponentAttribute( "Rock.Communication.Transport.MailgunHttp", "APIKey", rockContext );
            EncryptComponentAttribute( "Rock.Communication.Transport.MailgunHttp", "HTTPWebhookSigningKey", rockContext );
            EncryptComponentAttribute( "Rock.Communication.Transport.Twilio", Rock.SystemKey.TwilioAttributeKey.AuthToken, rockContext );
            EncryptComponentAttribute( "Rock.Communication.Transport.Firebase", "ServerKey", rockContext );
            EncryptComponentAttribute( "Rock.Communication.Transport.Firebase", "ServiceAccountJson", rockContext );
            EncryptComponentAttribute( "Rock.Communication.Transport.RockMobilePush", "ServerKey", rockContext );
            EncryptComponentAttribute( "Rock.Communication.Transport.RockMobilePush", "ServiceAccountJson", rockContext );
            EncryptComponentAttribute( "Rock.Communication.Transport.SMTP", "Password", rockContext );
            EncryptComponentAttribute( "Rock.Communication.Transport.MandrillSmtp", "Password", rockContext );
        }

        /// <summary>
        /// Encrypts the plain text values of a component attribute. A failure
        /// is logged and does not stop the other attributes.
        /// </summary>
        /// <param name="componentTypeName">The full name of the component's entity type.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void EncryptComponentAttribute( string componentTypeName, string attributeKey, RockContext rockContext )
        {
            try
            {
                var entityType = EntityTypeCache.Get( componentTypeName, false, rockContext );
                if ( entityType == null )
                {
                    return;
                }

                var attributeIds = new AttributeService( rockContext )
                    .Get( entityType.Id, null, null )
                    .Where( a => a.Key == attributeKey )
                    .Select( a => a.Id )
                    .ToList();

                foreach ( var attributeId in attributeIds )
                {
                    EncryptAttributeValues( attributeId, rockContext );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
            }
        }

        /// <summary>
        /// Encrypts every plain text value of the attribute.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void EncryptAttributeValues( int attributeId, RockContext rockContext )
        {
            var attribute = AttributeCache.Get( attributeId );
            var attributeValues = new AttributeValueService( rockContext )
                .Queryable()
                .Where( av => av.AttributeId == attributeId
                    && av.EntityId.HasValue
                    && av.Value != null
                    && av.Value != string.Empty )
                .Select( av => new
                {
                    EntityId = av.EntityId.Value,
                    av.Value
                } )
                .ToList();

            foreach ( var attributeValue in attributeValues )
            {
                if ( Encryption.DecryptString( attributeValue.Value ) != null )
                {
                    continue;
                }

                // A value that looks encrypted but does not decrypt was most
                // likely encrypted with a key this server lacks. Encrypting it
                // again would wrap it twice and lose it.
                if ( AIProviderContainer.IsRockCiphertextShape( attributeValue.Value ) )
                {
                    var message = $"Skipped encrypting the value of attribute {attributeId} for entity {attributeValue.EntityId} because it looks encrypted but could not be decrypted. Check the DataEncryptionKey and OldDataEncryptionKeys settings.";

                    RockLogger.LoggerFactory.CreateLogger( typeof( SecretAttributeEncryptor ).FullName ).LogWarning( message );
                    ExceptionLogService.LogException( new Exception( message ) );
                    continue;
                }

                // Skip a value that changed since it was read, so a value
                // saved at the same moment is not overwritten.
                var currentValue = new AttributeValueService( rockContext )
                    .Queryable()
                    .Where( av => av.AttributeId == attributeId
                        && av.EntityId == attributeValue.EntityId )
                    .Select( av => av.Value )
                    .FirstOrDefault();

                if ( currentValue != attributeValue.Value )
                {
                    continue;
                }

                Rock.Attribute.Helper.SaveAttributeValue( attributeValue.EntityId,
                    attribute,
                    Encryption.EncryptString( attributeValue.Value ),
                    rockContext );
            }
        }
    }
}
