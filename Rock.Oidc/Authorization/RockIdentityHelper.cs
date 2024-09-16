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
using System.Linq;
using System.Security.Claims;
using System.Web;

using AspNet.Security.OpenIdConnect.Primitives;

using Owin.Security.OpenIdConnect.Extensions;
using Owin.Security.OpenIdConnect.Server;

using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Oidc.Authorization
{
    /// <summary>
    /// A class with helper functions to get client allowed scopes, claims and claim identity.
    /// </summary>
    public static class RockIdentityHelper
    {
        /// <summary>
        /// Returns a ClaimsIdentity that is populated with the allowed claims.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="allowedClaims">The allowed claims.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public static ClaimsIdentity GetRockClaimsIdentity( UserLogin user, IDictionary<string, string> allowedClaims, string clientId )
        {
            var identity = new ClaimsIdentity(
                OpenIdConnectServerDefaults.AuthenticationType,
                OpenIdConnectConstants.Claims.Name,
                OpenIdConnectConstants.Claims.Role );

            var handledScopes = new HashSet<string> { OpenIdConnectConstants.Scopes.OpenId };

            // Note: the subject claim is always included in both identity and
            // access tokens, even if an explicit destination is not specified.
            identity.AddClaim( new Claim( OpenIdConnectConstants.Claims.Subject, user.Person.PrimaryAlias.Guid.ToString() )
                    .SetDestinations( OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken ) );

            // Add the client id so we can get the client id for api authorization.
            identity.AddClaim( new Claim( OpenIdConnectConstants.Claims.ClientId, clientId )
                .SetDestinations( OpenIdConnectConstants.Destinations.AccessToken ) );

            identity.AddClaim( new Claim( OpenIdConnectConstants.Claims.Username, user.UserName )
                .SetDestinations( OpenIdConnectConstants.Destinations.AccessToken ) );

            var definedClaimValues = new Dictionary<string, Func<Person, string>>
            {
                {
                    OpenIdConnectConstants.Claims.Address, (p) =>
                    {
                        var userAddress = user.Person.GetMailingLocation();
                        var claimAddress = string.Empty;
                        if ( userAddress != null )
                        {
                            var address = new
                            {
                                formatted = userAddress.FormattedAddress,
                                street_address = userAddress.Street1 + " " + userAddress.Street2,
                                locality = userAddress.City,
                                region = userAddress.State,
                                country = userAddress.Country
                            };
                            claimAddress = address.ToJson();
                                return claimAddress;
                        }

                        return string.Empty;
                    }
                },
                {
                    OpenIdConnectConstants.Claims.Email, (p) => p.Email
                },
                {
                    OpenIdConnectConstants.Claims.PhoneNumber, (p) =>
                    {
                        var claimPhoneNumber = string.Empty;

                        if ( p.PhoneNumbers != null )
                        {
                            var phoneNumber = p.PhoneNumbers.Where( ph => !ph.IsUnlisted ).FirstOrDefault();
                            claimPhoneNumber = phoneNumber?.NumberFormattedWithCountryCode ?? string.Empty;
                        }

                        return claimPhoneNumber;
                    }
                },
                {
                    OpenIdConnectConstants.Claims.PreferredUsername, (p) => p.FullName
                },
                {
                    OpenIdConnectConstants.Claims.Name, (p) => p.FullName
                },
                {
                    OpenIdConnectConstants.Claims.GivenName, (p) => p.FirstName
                },
                {
                    OpenIdConnectConstants.Claims.MiddleName, (p) => p.MiddleName ?? string.Empty
                },
                {
                    OpenIdConnectConstants.Claims.FamilyName, (p) => p.LastName
                },
                {
                    OpenIdConnectConstants.Claims.Nickname, (p) => p.NickName
                },
                {
                    OpenIdConnectConstants.Claims.Picture, (p) =>
                    {
                        if ( user.Person.PhotoId != null )
                        {
                            var photoGuid = user.Person.Photo.Guid;
                            var publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );
                            var options = new GetImageUrlOptions { PublicAppRoot = publicAppRoot };

                            return FileUrlHelper.GetImageUrl( photoGuid, options );
                        }

                        return string.Empty;
                    }
                },
                {
                    OpenIdConnectConstants.Claims.Gender, (p) => p.Gender.ToString()
                },
            };

            // Handle custom scopes
            var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, user.Person );

            foreach ( var unprocessedClaim in allowedClaims )
            {
                var claimValue = unprocessedClaim.Value;

                if ( definedClaimValues.ContainsKey( unprocessedClaim.Key ) )
                {
                    claimValue = definedClaimValues[unprocessedClaim.Key]( user.Person );
                }
                else
                {
                    claimValue = unprocessedClaim.Value.ResolveMergeFields( mergeFields );
                }

                identity.AddClaim( new Claim( unprocessedClaim.Key, claimValue )
                            .SetDestinations( OpenIdConnectConstants.Destinations.IdentityToken ) );
            }

            return identity;
        }

        /// <summary>
        /// Narrows the requested scopes to approved scopes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="requestedScopes">The requested scopes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static IEnumerable<string> NarrowRequestedScopesToApprovedScopes( RockContext rockContext, string clientId, IEnumerable<string> requestedScopes )
        {
            if ( rockContext == null )
            {
                throw new ArgumentException( $"{nameof( rockContext )} cannot be null." );
            }

            if ( clientId.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( $"{nameof( clientId )} cannot be null or empty." );
            }

            if ( requestedScopes == null || requestedScopes.Count() == 0 )
            {
                return new List<string>();
            }

            var allowedScopes = GetAllowedClientScopes( rockContext, clientId );
            return requestedScopes.Intersect( allowedScopes );
        }

        /// <summary>
        /// Gets the allowed client scopes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static IEnumerable<string> GetAllowedClientScopes( RockContext rockContext, string clientId )
        {
            if ( rockContext == null )
            {
                throw new ArgumentException( $"{nameof( rockContext )} cannot be null." );
            }

            if ( clientId.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( $"{nameof( clientId )} cannot be null or empty." );
            }

            // The OpenId is required and should always be allowed.
            var emptyScopeList = new List<string> { };
            var authClientService = new AuthClientService( rockContext );

            var enabledClientScopes = authClientService
                .Queryable()
                .Where( ac => ac.ClientId == clientId )
                .Select( ac => ac.AllowedScopes )
                .FirstOrDefault();
            if ( enabledClientScopes.IsNullOrWhiteSpace() )
            {
                return emptyScopeList;
            }

            var parsedClientScopes = enabledClientScopes.FromJsonOrNull<List<string>>();
            if ( parsedClientScopes == null )
            {
                return emptyScopeList;
            }

            var activeClientScopes = GetActiveAuthScopes( rockContext );

            return parsedClientScopes.Intersect( activeClientScopes );
        }

        /// <summary>
        /// Gets the active client scopes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static IEnumerable<string> GetActiveAuthScopes( RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentException( $"{nameof( rockContext )} cannot be null." );
            }

            var activeAuthScopes = new AuthScopeService( rockContext )
                .Queryable()
                .Where( s => s.IsActive )
                .Select( s => s.Name );

            return activeAuthScopes;
        }

        /// <summary>
        /// Gets the allowed client claims.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="allowedClientScopes">The allowed client scopes.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static IDictionary<string, string> GetAllowedClientClaims( RockContext rockContext, string clientId, IEnumerable<string> allowedClientScopes )
        {
            if ( rockContext == null )
            {
                throw new ArgumentException( $"{nameof( rockContext )} cannot be null." );
            }

            if ( clientId.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( $"{nameof( clientId )} cannot be null or empty." );
            }

            var allowedClaimList = new Dictionary<string, string>();
            var authClientService = new AuthClientService( rockContext );
            var allowedClaims = authClientService.Queryable().Where( ac => ac.ClientId == clientId ).Select( ac => ac.AllowedClaims ).FirstOrDefault();
            if ( allowedClaims.IsNullOrWhiteSpace() )
            {
                return allowedClaimList;
            }

            var parsedClaims = allowedClaims.FromJsonOrNull<List<string>>();
            if ( parsedClaims == null )
            {
                return allowedClaimList;
            }

            return new AuthClaimService( rockContext )
                .Queryable()
                .Where( ac => parsedClaims.Contains( ac.Name ) )
                .Where( ac => ac.IsActive )
                .Where( ac => allowedClientScopes.Contains( ac.Scope.Name ) )
                .Where( ac => ac.Scope.IsActive )
                .ToDictionary( vc => vc.Name, vc => vc.Value );
        }

        /// <summary>
        /// Gets the active authentication claims.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="activeClientScopes">The active client scopes.</param>
        /// <returns>IEnumerable&lt;System.String&gt;.</returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static IEnumerable<string> GetActiveAuthClaims( RockContext rockContext, IEnumerable<string> activeClientScopes )
        {
            if ( rockContext == null )
            {
                throw new ArgumentException( $"{nameof( rockContext )} cannot be null." );
            }

            return new AuthClaimService( rockContext )
                .Queryable()
                .Where( ac => ac.IsActive )
                .Where( ac => activeClientScopes.Contains( ac.Scope.Name ) )
                .Where( ac => ac.Scope.IsActive )
                .Select( a => a.Name );
        }
    }
}
