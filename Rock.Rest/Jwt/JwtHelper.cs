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
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Rest.Jwt
{
    /// <summary>
    /// Methods and constants used to work with JWTs
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// If the token is valid, the person claimed by that token will be returned
        /// </summary>
        /// <param name="jwtString"></param>
        /// <returns></returns>
        public async static Task<Person> GetPerson( string jwtString )
        {
            if ( jwtString.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var configs = GetJwtConfigs();

            if ( configs == null )
            {
                return null;
            }

            var rockContext = new RockContext();
            var personSearchKeyService = new PersonSearchKeyService( rockContext );
            var query = personSearchKeyService.Queryable().AsNoTracking();

            foreach ( var config in configs.Where( c => c.SearchTypeValueId.HasValue ) )
            {
                var jwt = await ValidateToken( config, jwtString );

                if ( jwt == null || jwt.Subject.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var people = query
                    .Where( psk => psk.SearchTypeValueId == config.SearchTypeValueId.Value && psk.SearchValue == jwt.Subject )
                    .Select( psk => psk.PersonAlias.Person )
                    .ToList();

                if ( people.Count > 1 )
                {
                    ExceptionLogService.LogException( $"{people.Count} people matched the JWT subject '{jwt.Subject}' for search value id '{config.SearchTypeValueId.Value}'" );
                    continue;
                }

                if ( people.Count == 1 )
                {
                    return people.Single();
                }
            }

            return null;
        }

        /// <summary>
        /// Get the JWT configurations for validating tokens
        /// </summary>
        /// <returns></returns>
        private static List<JwtConfig> GetJwtConfigs()
        {
            var definedTypeCache = DefinedTypeCache.Get( SystemGuid.DefinedType.OPEN_ID_CONFIGURATION_URL );

            if ( definedTypeCache == null || !definedTypeCache.IsActive || definedTypeCache.DefinedValues == null )
            {
                return null;
            }

            var configs = new List<JwtConfig>( definedTypeCache.DefinedValues.Count );
            var issuerAttributeCache = AttributeCache.Get( SystemGuid.Attribute.DEFINED_VALUE_JWT_ISSUER );
            var audienceAttributeCache = AttributeCache.Get( SystemGuid.Attribute.DEFINED_VALUE_JWT_AUDIENCE );
            var searchKeyAttributeCache = AttributeCache.Get( SystemGuid.Attribute.DEFINED_VALUE_JWT_SEARCH_KEY );

            foreach ( var definedValueCache in definedTypeCache.DefinedValues.OrderBy( dv => dv.Order ).ThenBy( dv => dv.Id ) )
            {
                var config = new JwtConfig
                {
                    JwksJsonFileUrl = definedValueCache.Value
                };

                if ( definedValueCache.AttributeValues != null )
                {
                    if ( issuerAttributeCache != null && definedValueCache.AttributeValues.ContainsKey( issuerAttributeCache.Key ) )
                    {
                        config.Issuer = definedValueCache.AttributeValues[issuerAttributeCache.Key]?.Value;
                    }

                    if ( audienceAttributeCache != null && definedValueCache.AttributeValues.ContainsKey( audienceAttributeCache.Key ) )
                    {
                        config.Audience = definedValueCache.AttributeValues[audienceAttributeCache.Key]?.Value;
                    }

                    if ( searchKeyAttributeCache != null && definedValueCache.AttributeValues.ContainsKey( searchKeyAttributeCache.Key ) )
                    {
                        var searchKeyGuid = definedValueCache.AttributeValues[searchKeyAttributeCache.Key]?.Value;
                        config.SearchTypeValueId = DefinedValueCache.Get( searchKeyGuid )?.Id;
                    }
                }

                configs.Add( config );
            }

            return configs;
        }

        /// <summary>
        /// The lifetime validation method for the JWT
        /// </summary>
        /// <param name="notBefore"></param>
        /// <param name="expires"></param>
        /// <param name="securityToken"></param>
        /// <param name="validationParameters"></param>
        /// <returns></returns>
        private static bool JwtLifetimeValidator( DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters )
        {
            return
                expires.HasValue && RockDateTime.Now < expires.Value &&
                ( !notBefore.HasValue || RockDateTime.Now > notBefore.Value );
        }

        /// <summary>
        /// Validates the JWT and returns the parsed token
        /// </summary>
        /// <param name="jwtString">The token.</param>
        /// <param name="jwtConfig">The configuration on how to validate the token</param>
        /// <returns></returns>
        private async static Task<JwtSecurityToken> ValidateToken( JwtConfig jwtConfig, string jwtString )
        {
            if ( jwtString.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( jwtString.StartsWith( HeaderTokens.JwtPrefix ) )
            {
                jwtString = jwtString.Substring( HeaderTokens.JwtPrefix.Length );
            }

            if ( jwtConfig == null )
            {
                return null;
            }

            var configurationManager = GetConfigurationManager( jwtConfig.JwksJsonFileUrl );

            if ( configurationManager == null )
            {
                return null;
            }

            var discoveryDocument = await configurationManager.GetConfigurationAsync();

            if ( discoveryDocument == null || discoveryDocument.SigningKeys == null || !discoveryDocument.SigningKeys.Any() )
            {
                return null;
            }

            var validateAudience = !jwtConfig.Audience.IsNullOrWhiteSpace();
            var validateIssuer = !jwtConfig.Issuer.IsNullOrWhiteSpace();

            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = validateAudience,
                ValidAudience = validateAudience ? jwtConfig.Audience : null,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuer = validateIssuer,
                ValidIssuer = validateIssuer ? jwtConfig.Issuer : null,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = discoveryDocument.SigningKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes( 1 )
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken( jwtString, validationParameters, out var validatedToken );

                if ( principal == null || principal.Identity == null )
                {
                    return null;
                }

                var jwtToken = validatedToken as JwtSecurityToken;
                return jwtToken;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the configuration manager for the configured JWKS endpoint.
        /// </summary>
        /// <param name="jwksJsonFileUrl">The JWKS JSON file URL.</param>
        /// <returns></returns>
        private static ConfigurationManager<OpenIdConnectConfiguration> GetConfigurationManager( string jwksJsonFileUrl )
        {
            if ( !_configurationManagers.ContainsKey( jwksJsonFileUrl ) )
            {
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    jwksJsonFileUrl,
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever() );

                _configurationManagers[jwksJsonFileUrl] = configurationManager;
            }

            return _configurationManagers[jwksJsonFileUrl];
        }
        private static Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _configurationManagers = new Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();
    }
}
