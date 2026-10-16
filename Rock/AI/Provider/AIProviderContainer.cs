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
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.Data;
using Rock.Extension;
using Rock.Logging;
using Rock.Web.Cache;

namespace Rock.AI.Provider
{
    /// <summary>
    /// The container for AI components.
    /// </summary>
    public class AIProviderContainer : Container<AIProviderComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<AIProviderContainer> instance =
            new Lazy<AIProviderContainer>( () => new AIProviderContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static AIProviderContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static AIProviderComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Returns the active component
        /// </summary>
        /// <returns></returns>
        public static AIProviderComponent GetActiveComponent()
        {
            return Instance.Components.Select( c => c.Value.Value ).Where( c => c.IsActive ).FirstOrDefault();
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( AIProviderComponent ) )]
        protected override IEnumerable<Lazy<AIProviderComponent, IComponentData>> MEFComponents { get; set; }

        /// <summary>
        /// Forces a reloading of all the components
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            // Create any attributes that need to be created
            var providerEntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.AIProvider ) );

            foreach ( var component in this.Components )
            {
                /*
                    10/15/2026 - MSE

                    Every web farm node now loads this container at start
                    up, so two nodes can race to create the same attribute
                    rows. This runs from the constructor of the lazy
                    singleton, and a lazy value that throws keeps throwing
                    until the next restart. Catch and log any failure here
                    so the components still load; the attributes are
                    updated again on the next refresh or restart. Each
                    component gets its own context so a failed save does
                    not leave pending changes that break the next one.

                    Reason: Keep AI providers working after a transient
                    start up failure.
                */
                try
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var componentType = component.Value.Value.GetType();
                        var componentEntityTypeId = EntityTypeCache.GetId( componentType );

                        Rock.Attribute.Helper.UpdateAttributes( componentType,
                            providerEntityTypeId,
                            "ProviderComponentEntityTypeId",
                            componentEntityTypeId.ToString(),
                            rockContext );

                        /*
                            10/15/2026 - MSE

                            Secret keys used to be plain text attributes. Now
                            that they are encrypted fields, encrypt any value
                            that was saved before the change. Values that
                            already decrypt, or that look like they were
                            encrypted with a key this server does not have,
                            are left alone, so this is safe to run on every
                            start.

                            Reason: Keep AI provider secret keys encrypted at rest.
                        */
                        EncryptPlainTextSecretKeys( providerEntityTypeId, componentEntityTypeId, rockContext );
                    }
                }
                catch ( Exception ex )
                {
                    Rock.Model.ExceptionLogService.LogException( ex );
                }
            }
        }

        /// <summary>
        /// Encrypts any plain text values of the encrypted text attributes
        /// defined by the AI provider component.
        /// </summary>
        /// <param name="providerEntityTypeId">The entity type identifier of <see cref="Rock.Model.AIProvider"/>.</param>
        /// <param name="componentEntityTypeId">The entity type identifier of the provider component.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void EncryptPlainTextSecretKeys( int? providerEntityTypeId, int? componentEntityTypeId, RockContext rockContext )
        {
            if ( !providerEntityTypeId.HasValue || !componentEntityTypeId.HasValue )
            {
                return;
            }

            var encryptedTextFieldTypeId = FieldTypeCache.GetId( Rock.SystemGuid.FieldType.ENCRYPTED_TEXT.AsGuid() );
            if ( !encryptedTextFieldTypeId.HasValue )
            {
                return;
            }

            var attributeIds = new Rock.Model.AttributeService( rockContext )
                .Get( providerEntityTypeId, "ProviderComponentEntityTypeId", componentEntityTypeId.Value.ToString() )
                .Where( a => a.FieldTypeId == encryptedTextFieldTypeId.Value )
                .Select( a => a.Id )
                .ToList();

            foreach ( var attributeId in attributeIds )
            {
                var attribute = AttributeCache.Get( attributeId );
                var attributeValues = new Rock.Model.AttributeValueService( rockContext )
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
                    if ( Rock.Security.Encryption.DecryptString( attributeValue.Value ) != null )
                    {
                        continue;
                    }

                    // A value that has the shape of Rock ciphertext but does
                    // not decrypt was most likely encrypted with a key that
                    // is missing from this server's configuration. Encrypting
                    // it again would wrap it twice and break it for good.
                    if ( IsRockCiphertextShape( attributeValue.Value ) )
                    {
                        RockLogger.LoggerFactory.CreateLogger<AIProviderContainer>()
                            .LogWarning( "Skipped encrypting the AI provider attribute value for attribute {AttributeId} and AI provider {EntityId} because it looks encrypted but could not be decrypted. Check the DataEncryptionKey and OldDataEncryptionKeys settings.", attributeId, attributeValue.EntityId );

                        // Also write it to the exception log so it is seen
                        // without RockLogger configured. Never include the value.
                        Rock.Model.ExceptionLogService.LogException( new Exception( $"Skipped encrypting the AI provider attribute value for attribute {attributeId} and AI provider {attributeValue.EntityId} because it looks encrypted but could not be decrypted. Check the DataEncryptionKey and OldDataEncryptionKeys settings." ) );
                        continue;
                    }

                    // Only replace the value if nobody changed it after it
                    // was read above. This narrows, but does not close, the
                    // window where a key saved by an administrator at the
                    // same moment could be overwritten: a save between this
                    // check and the save below is still lost.
                    var currentValue = new Rock.Model.AttributeValueService( rockContext )
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
                        Rock.Security.Encryption.EncryptString( attributeValue.Value ),
                        rockContext );
                }
            }
        }

        /// <summary>
        /// Determines whether the value has the shape of text encrypted by
        /// <see cref="Rock.Security.Encryption.EncryptString(string)"/>: Base64
        /// of a 4 byte IV length of 16, a 16 byte IV, and one or more 16 byte
        /// AES blocks, followed by a 32 byte HMAC tag and a "V2" footer in
        /// the v2 format.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><c>true</c> if the value looks like Rock ciphertext; otherwise <c>false</c>.</returns>
        internal static bool IsRockCiphertextShape( string value )
        {
            const int blockLength = 16;
            const int headerLength = sizeof( int ) + blockLength;

            if ( string.IsNullOrWhiteSpace( value ) || value.Length % 4 != 0 )
            {
                return false;
            }

            byte[] bytes;

            try
            {
                bytes = Convert.FromBase64String( value );
            }
            catch ( FormatException )
            {
                return false;
            }

            if ( bytes.Length < headerLength + blockLength || BitConverter.ToInt32( bytes, 0 ) != blockLength )
            {
                return false;
            }

            /*
                10/15/2026 - MSE

                EncryptString now writes the v2 format, which adds a 32 byte
                HMAC tag and a "V2" footer after the AES blocks. Older values
                are still in the v1 format, so recognize both.

                Reason: Do not encrypt an already encrypted v2 secret key a
                second time.
            */
            const int v2TrailerLength = 32 + 2;

            var isV2 = bytes.Length >= headerLength + blockLength + v2TrailerLength
                && bytes[bytes.Length - 2] == ( byte ) 'V'
                && bytes[bytes.Length - 1] == ( byte ) '2'
                && ( bytes.Length - headerLength - v2TrailerLength ) % blockLength == 0;

            return isV2 || ( bytes.Length - headerLength ) % blockLength == 0;
        }
    }
}
