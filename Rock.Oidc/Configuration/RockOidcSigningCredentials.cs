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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Rock.SystemKey;
using Rock.Web;

namespace Rock.Oidc.Configuration
{
    internal class RockOidcSigningCredentials : IRockOidcSigningCredentials
    {
        private RockOidcSettings _rockOidcSettings = null;
        private List<RSA> rsas = null;
        private List<SerializedRockRsaKey> serializedRsaKeys = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockOidcSigningCredentials"/> class.
        /// </summary>
        /// <param name="rockOidcSettings">The rock oidc settings.</param>
        public RockOidcSigningCredentials( RockOidcSettings rockOidcSettings )
        {
            _rockOidcSettings = rockOidcSettings;
            serializedRsaKeys = GetSerializedParametersList();
            rsas = GetRsaKeys( serializedRsaKeys );
        }

        /// <summary>
        /// Gets the signing keys.
        /// </summary>
        /// <value>
        /// The signing keys.
        /// </value>
        public ReadOnlyCollection<RSA> SigningKeys => rsas.AsReadOnly();

        private List<SerializedRockRsaKey> GetSerializedParametersList()
        {
            var jsonParameters = SystemSettings.GetValue( SystemSetting.OPEN_ID_CONNECT_RSA_KEYS );
            List<SerializedRockRsaKey> keys = null;

            if ( jsonParameters.IsNotNullOrWhiteSpace() )
            {
                keys = JsonConvert.DeserializeObject<List<SerializedRockRsaKey>>(
                            jsonParameters
                            , new JsonSerializerSettings { ContractResolver = new RockOidcSigningCredentialCollectionContractResolver() }
                        ).OrderByDescending( k => k.KeyCreatedDate ).ToList();
            }
            else
            {
                keys = new List<SerializedRockRsaKey>( 1 );
            }

            // Remove old expired keys.
            keys.RemoveAll( k => k.KeyCreatedDate.AddSeconds( _rockOidcSettings.SigningKeyLifetime + GetMaxTokenAge() ) < DateTime.UtcNow );

            return keys;
        }

        private void SetSerializedParametersList( List<SerializedRockRsaKey> serializedRsaKeys )
        {
            var rsaKeys = JsonConvert.SerializeObject(
                            serializedRsaKeys
                            , new JsonSerializerSettings { ContractResolver = new RockOidcSigningCredentialCollectionContractResolver() } );

            SystemSettings.SetValue( SystemSetting.OPEN_ID_CONNECT_RSA_KEYS, rsaKeys );
        }

        private int GetMaxTokenAge()
        {
            var ages = new List<int> { _rockOidcSettings.AccessTokenLifetime, _rockOidcSettings.IdentityTokenLifetime, _rockOidcSettings.RefreshTokenLifetime };
            return ages.Max();
        }

        private List<RSA> GetRsaKeys( List<SerializedRockRsaKey> serializedRsaKeys )
        {
            var listSize = serializedRsaKeys.Count == 0 ? 1 : serializedRsaKeys.Count;

            var rsaKeys = new List<RSA>( listSize );
            var maxTokenAge = GetMaxTokenAge();

            // Add new key because the other ones are old or no key exists.
            if ( !serializedRsaKeys.Any( k => k.KeyCreatedDate.AddSeconds( _rockOidcSettings.SigningKeyLifetime ) >= DateTime.UtcNow ) )
            {
                var serializedRsaKey = new SerializedRockRsaKey();
                using ( var rsaKey = GenerateRsaKey( 2048 ) )
                {
                    serializedRsaKey.Parameters = rsaKey.ExportParameters( true );
                }

                if ( serializedRsaKeys.Count == 0 )
                {
                    serializedRsaKeys.Add( serializedRsaKey );
                }
                else
                {
                    serializedRsaKeys.Insert( 0, serializedRsaKey );
                }

            }

            SetSerializedParametersList( serializedRsaKeys );

            foreach ( var serializedRsaKey in serializedRsaKeys )
            {
                rsaKeys.Add( GenerateRsaKey( 2048, serializedRsaKey.Parameters ) );
            }

            return rsaKeys;
        }

        private RSA GenerateRsaKey( int size, RSAParameters parameters )
        {
            // Note: a 1024-bit key might be returned by RSA.Create() on .NET Desktop/Mono,
            // where RSACryptoServiceProvider is still the default implementation and
            // where custom implementations can be registered via CryptoConfig.
            // To ensure the key size is always acceptable, replace it if necessary.
            var rsa = RSA.Create();

            if ( rsa.KeySize < size )
            {
                rsa.KeySize = size;
            }

            if ( rsa.KeySize < size && rsa is RSACryptoServiceProvider )
            {
                rsa.Dispose();
                rsa = new RSACryptoServiceProvider( size );
                rsa.ImportParameters( parameters );
            }

            if ( rsa.KeySize < size )
            {
                throw new InvalidOperationException( "The RSA key generation failed." );
            }

            return rsa;
        }

        private static RSA GenerateRsaKey( int size )
        {
            // Note: a 1024-bit key might be returned by RSA.Create() on .NET Desktop/Mono,
            // where RSACryptoServiceProvider is still the default implementation and
            // where custom implementations can be registered via CryptoConfig.
            // To ensure the key size is always acceptable, replace it if necessary.
            var rsa = RSA.Create();

            if ( rsa.KeySize < size )
            {
                rsa.KeySize = size;
            }

            if ( rsa.KeySize < size && rsa is RSACryptoServiceProvider )
            {
                rsa.Dispose();
                rsa = new RSACryptoServiceProvider( size );
            }

            if ( rsa.KeySize < size )
            {
                throw new InvalidOperationException( "The RSA key generation failed." );
            }

            return rsa;
        }

    }
}
