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
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Rest.Jwt
{
    /// <summary>
    /// Methods used to work with JWTs
    /// </summary>
    public static class JwtHelper
    {
        /// <summary>
        /// Validates the JSON WebToken and returns the UserLogin associated to the specified JSON Web Token.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="jwtString">The JWT string.</param>
        /// <returns>UserLogin.</returns>
        public static UserLogin GetUserLoginByJSONWebToken( RockContext rockContext, string jwtString )
        {
            var jwtPrefix = HeaderTokens.JwtPrefix;

            // It is standard to prefix JWTs with "Bearer ", but JwtSecurityTokenHandler.ValidateToken will
            // say the token is malformed if the prefix is not removed
            if ( jwtString.StartsWith( jwtPrefix ) )
            {
                jwtString = jwtString.Substring( jwtPrefix.Length );
            }

            var jwtConfigs = GetJwtConfigs();
            if ( !jwtConfigs.Any() )
            {
                // if there aren't any GetJwtConfigs defined, do all the standard validation checks, but don't do Audience or Issuer validation
                var unvalidatedToken = new JwtSecurityToken( jwtString );
                var issurer = unvalidatedToken.Payload?.Iss ?? ( string ) unvalidatedToken.Header["iss"];
                if ( issurer.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                jwtConfigs.Add( new JwtConfig
                {
                    OpenIdConfigurationUrl = $"{issurer}.well-known/openid-configuration"
                } );
            }

            JwtSecurityToken validatedToken = null;

            foreach ( var jwtConfig in jwtConfigs )
            {
                validatedToken = ValidateToken( jwtConfig, jwtString );
                if ( validatedToken != null )
                {
                    break;
                }
            }

            var subject = validatedToken?.Subject;

            if ( subject.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var userLoginService = new UserLoginService( rockContext );

            // try finding UserName by prefixing jwt.subject with the prefixes that Rock uses for these
            string[] userLoginPrefixes = { "AUTH0_", "OIDC_" };
            foreach ( var userLoginPrefix in userLoginPrefixes )
            {
                string userName = $"{userLoginPrefix}{subject}";

                var userLogin = userLoginService.GetByUserName( userName );
                if ( userLogin != null )
                {
                    return userLogin;
                }
            }

            return null;
        }

        /// <summary>
        /// If the JWT is valid, the person claimed by that token will be returned. This method uses the configured validation parameters from the
        /// JSON Web Token Configuration Defined Type.
        /// </summary>
        /// <param name="jwtString"></param>
        /// <returns></returns>
        public static Person GetPersonFromJWTPersonSearchKey( string jwtString )
        {
            /*
              2/16/2022 MDP

            Note that this feature is not very well documented, see why at https://app.asana.com/0/0/1201611176520656/f 

            */

            if ( jwtString.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Get the configs from the JSON Web Token Configuration defined values
            var configs = GetJwtConfigs();

            if ( configs == null )
            {
                return null;
            }

            // The configs are required to specify a person search key type. The subject of the JWT should match a search key value so that we know
            // which Person the sender of the token claims to be.
            var rockContext = new RockContext();
            var personSearchKeyService = new PersonSearchKeyService( rockContext );
            var query = personSearchKeyService.Queryable().AsNoTracking();

            // Try each config in order, which are pre-ordered. The SearchTypeValueId is required (if null we couldn't match a person even if the
            // token validates)
            foreach ( var config in configs.Where( c => c.SearchTypeValueId.HasValue ) )
            {
                // If the token is valid, this method will return it as an object that we can pull subject from. Even if the token is valid,
                // if the subject is not set, we cannot match it to a person search key
                var jwt = ValidateToken( config, jwtString );

                if ( jwt == null || jwt.Subject.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                // Get all the people (it is possible to get more than one) that have the matching search key
                var people = query
                    .Where( psk => psk.SearchTypeValueId == config.SearchTypeValueId.Value && psk.SearchValue == jwt.Subject )
                    .Select( psk => psk.PersonAlias.Person )
                    .ToList();

                // If there are more than one match, then the Rock admin needs to take action and fix the data as this could be a security hole and we
                // cannot tell which person is the bearer of the JWT
                if ( people.Count > 1 )
                {
                    ExceptionLogService.LogException( $"{people.Count} people matched the JWT subject '{jwt.Subject}' for search value id '{config.SearchTypeValueId.Value}'" );
                    continue;
                }

                // If there is a single match, then this method is done and there is no need to check more configurations
                if ( people.Count == 1 )
                {
                    return people.Single();
                }
            }

            // None of the configurations was able to validate the token and find a matching person
            return null;
        }

        /// <summary>
        /// Get the ordered JWT configurations for validating tokens
        /// </summary>
        /// <returns></returns>
        private static List<JwtConfig> GetJwtConfigs()
        {
            // JWT configs are stored as defined values. The value is the OpenID configuration URL
            var definedTypeCache = DefinedTypeCache.Get( SystemGuid.DefinedType.JWT_CONFIGURATION );

            if ( definedTypeCache == null || !definedTypeCache.IsActive || definedTypeCache.DefinedValues == null )
            {
                return null;
            }

            // Additional JWT configuration properties are stored as attributes of the defined value
            var issuerAttributeCache = AttributeCache.Get( SystemGuid.Attribute.DEFINED_VALUE_JWT_ISSUER );
            var audienceAttributeCache = AttributeCache.Get( SystemGuid.Attribute.DEFINED_VALUE_JWT_AUDIENCE );
            var searchKeyAttributeCache = AttributeCache.Get( SystemGuid.Attribute.DEFINED_VALUE_JWT_SEARCH_KEY );

            // The configs should be ordered since they will be attempted in the order given here. The Rock admin may want them
            // tried in a specific order.
            return definedTypeCache.DefinedValues.OrderBy( dv => dv.Order ).ThenBy( dv => dv.Id ).Select( dv =>
            {
                var config = new JwtConfig
                {
                    OpenIdConfigurationUrl = dv.Value
                };

                if ( dv.AttributeValues != null )
                {
                    if ( issuerAttributeCache != null && dv.AttributeValues.ContainsKey( issuerAttributeCache.Key ) )
                    {
                        config.Issuer = dv.AttributeValues[issuerAttributeCache.Key]?.Value;
                    }

                    if ( audienceAttributeCache != null && dv.AttributeValues.ContainsKey( audienceAttributeCache.Key ) )
                    {
                        config.Audience = dv.AttributeValues[audienceAttributeCache.Key]?.Value;
                    }

                    if ( searchKeyAttributeCache != null && dv.AttributeValues.ContainsKey( searchKeyAttributeCache.Key ) )
                    {
                        var searchKeyGuid = dv.AttributeValues[searchKeyAttributeCache.Key]?.Value;
                        config.SearchTypeValueId = DefinedValueCache.Get( searchKeyGuid )?.Id;
                    }
                }

                return config;
            } ).ToList();
        }

        /// <summary>
        /// The lifetime validation method for the JWT. The method confirms that the token is not expired and allowed to be used at this moment.
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
        /// Validates the JWT using the provided configuration and returns the parsed token if valid
        /// </summary>
        /// <param name="jwtString">The token.</param>
        /// <param name="jwtConfig">The configuration on how to validate the token</param>
        /// <returns></returns>
        private static JwtSecurityToken ValidateToken( JwtConfig jwtConfig, string jwtString )
        {
            if ( jwtString.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( jwtConfig == null )
            {
                return null;
            }

            // It is standard to prefix JWTs with "Bearer ", but JwtSecurityTokenHandler.ValidateToken will
            // say the token is malformed if the prefix is not removed
            if ( jwtString.StartsWith( HeaderTokens.JwtPrefix ) )
            {
                jwtString = jwtString.Substring( HeaderTokens.JwtPrefix.Length );
            }

            // Retrieve the configuration manager, which is cached according to the jwtConfig.JwksJsonFileUrl
            var configurationManager = GetConfigurationManager( jwtConfig.OpenIdConfigurationUrl );

            if ( configurationManager == null )
            {
                return null;
            }

            // The configuration manager handles caching the configuration documents and keys, which are from another
            // server or provider like Auth0.
            var openIdConnectConfiguration = AsyncHelper.RunSync( () => configurationManager.GetConfigurationAsync() );

            if ( openIdConnectConfiguration == null || openIdConnectConfiguration.SigningKeys == null || !openIdConnectConfiguration.SigningKeys.Any() )
            {
                return null;
            }

            // Validate the items that are configured to be validated
            var validateAudience = !jwtConfig.Audience.IsNullOrWhiteSpace();
            var validateIssuer = !jwtConfig.Issuer.IsNullOrWhiteSpace();

            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = validateAudience,
                ValidAudience = validateAudience ? jwtConfig.Audience : null,
                ValidateIssuer = validateIssuer,
                ValidIssuer = validateIssuer ? jwtConfig.Issuer : null,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openIdConnectConfiguration.SigningKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes( 1 ) // Allow a minute of play in server times since we're dealing with a third party key provider
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken( jwtString, validationParameters, out var validatedToken );

                // If the principal identity is null, we should not accept this as a validated token
                if ( principal == null || principal.Identity == null )
                {
                    return null;
                }

                var jwtToken = validatedToken as JwtSecurityToken;
                return jwtToken;
            }
            catch ( Exception ex )
            {
                // The JWT was not well formed or did not validate in some other way
                ExceptionLogService.LogException( ex );
                Debug.WriteLine( ex.Message );
                return null;
            }
        }

        /// <summary>
        /// Gets the configuration manager for the configured JWKS endpoint. This is cached in memory since it shouldn't change often
        /// </summary>
        /// <param name="jwksJsonFileUrl">The JWKS JSON file URL.</param>
        /// <returns></returns>
        private static ConfigurationManager<OpenIdConnectConfiguration> GetConfigurationManager( string jwksJsonFileUrl )
        {
            if ( !_configurationManagerCache.ContainsKey( jwksJsonFileUrl ) )
            {
                var httpDocumentRetriever = new HttpDocumentRetriever();
                httpDocumentRetriever.RequireHttps = !System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment;
                var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    jwksJsonFileUrl,
                    new OpenIdConnectConfigurationRetriever(),
                   httpDocumentRetriever );

                _configurationManagerCache[jwksJsonFileUrl] = configurationManager;
            }

            return _configurationManagerCache[jwksJsonFileUrl];
        }

        private static Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _configurationManagerCache = new Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();
    }
}
