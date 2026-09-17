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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;

using Rock.Data;
using Rock.Model;

namespace Rock.Communication.Chat.Platform.Session
{
    /// <summary>
    /// Gates a person, mints a short-lived ES256 church token, and writes the
    /// Chat People marker on first open.
    /// </summary>
    internal static class ChatSessionHelper
    {
        #region Constants

        /// <summary>
        /// Lifetime of a church token, in minutes.
        /// </summary>
        public const int ChurchTokenLifetimeMinutes = 5;

        /// <summary>
        /// Scope claim on a person church token.
        /// </summary>
        public const string PersonScope = "chat.session";

        /// <summary>
        /// Scope claim on a sync church token.
        /// </summary>
        public const string SyncScope = "sync";

        #endregion Constants

        #region Methods

        /// <summary>
        /// Runs the session gates in order and fails closed. Does not mint.
        /// </summary>
        /// <param name="person">The Rock person opening chat, or null if unsigned in.</param>
        /// <param name="config">Church connection and age / DM-access settings.</param>
        /// <param name="rockContext">Used for Ban List and record-status lookups.</param>
        /// <returns>The gate outcome, with no token.</returns>
        public static ChatMintResult Evaluate( Person person, ChatSessionConfiguration config, RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ),
                    "The record status and ban gates read the database, so a caller that passes no context would be granted a token that neither gate had looked at." );
            }

            config = config ?? new ChatSessionConfiguration();

            if ( person == null || person.Id <= 0 )
            {
                return Fail( ChatMintGate.SignInRequired );
            }

            if ( !config.IsConfigured )
            {
                return Fail( ChatMintGate.NotConfigured );
            }

            if ( person.IsDeceased )
            {
                return Fail( ChatMintGate.Deceased );
            }

            var isInactive = IsInactive( person, rockContext );
            if ( !isInactive.HasValue )
            {
                return Fail( ChatMintGate.GateUnavailable );
            }

            if ( isInactive.Value )
            {
                return Fail( ChatMintGate.Inactive );
            }

            var isOnBanList = IsOnBanList( person, rockContext );
            if ( !isOnBanList.HasValue )
            {
                return Fail( ChatMintGate.GateUnavailable );
            }

            if ( isOnBanList.Value )
            {
                return Fail( ChatMintGate.Banned );
            }

            if ( config.MinimumAge.HasValue && config.MinimumAge.Value > 0 )
            {
                if ( !person.Age.HasValue )
                {
                    return Fail( ChatMintGate.AgeVerificationRequired );
                }

                if ( person.Age.Value < config.MinimumAge.Value )
                {
                    return Fail( ChatMintGate.AgeRestricted );
                }
            }

            if ( !person.PrimaryAliasGuid.HasValue )
            {
                return Fail( ChatMintGate.NoPrimaryAlias );
            }

            return new ChatMintResult
            {
                Gate = ChatMintGate.Ok,
                PersonAliasGuid = person.PrimaryAliasGuid,
                TenantId = config.TenantId,
                CanStartDm = CanStartDirectMessage( person, config )
            };
        }

        /// <summary>
        /// Evaluates the gates and, if they pass, signs a person church token.
        /// </summary>
        /// <param name="person">The Rock person opening chat.</param>
        /// <param name="config">Church connection settings, including the private JWK.</param>
        /// <param name="rockContext">Used for Ban List and record-status lookups.</param>
        /// <returns>A signed token on success, or the failing gate with no token and no exception text.</returns>
        public static ChatMintResult TryMintChurchToken( Person person, ChatSessionConfiguration config, RockContext rockContext )
        {
            var result = Evaluate( person, config, rockContext );
            if ( result.Gate != ChatMintGate.Ok )
            {
                return result;
            }

            return Sign( result, config, PersonScope, person.PrimaryAliasGuid );
        }

        /// <summary>
        /// Signs a sync-scope church token. No person gates.
        /// </summary>
        /// <param name="config">Church connection settings, including the private JWK.</param>
        /// <returns>A signed token on success, or NotConfigured / InvalidKey with no exception text.</returns>
        public static ChatMintResult TryMintSyncToken( ChatSessionConfiguration config )
        {
            config = config ?? new ChatSessionConfiguration();
            if ( !config.IsConfigured )
            {
                return Fail( ChatMintGate.NotConfigured );
            }

            var result = new ChatMintResult
            {
                Gate = ChatMintGate.Ok,
                TenantId = config.TenantId
            };

            return Sign( result, config, SyncScope, null );
        }

        /// <summary>
        /// Evaluates the gates and, if they pass, writes one Chat People marker
        /// on first open. Later opens write none. A failed gate enrols nobody.
        /// </summary>
        /// <param name="person">The Rock person opening chat.</param>
        /// <param name="config">Church connection and age / DM-access settings.</param>
        /// <param name="rockContext">Used for gates and the marker write.</param>
        /// <returns>The gate outcome. Marker insert is a side effect on success.</returns>
        public static ChatMintResult EnsureEnrollment( Person person, ChatSessionConfiguration config, RockContext rockContext )
        {
            var result = Evaluate( person, config, rockContext );
            if ( result.Gate != ChatMintGate.Ok )
            {
                return result;
            }

            var chatPeopleGuid = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE );
            var group = rockContext.Set<Group>().FirstOrDefault( g => g.Guid == chatPeopleGuid );
            if ( group == null )
            {
                return result;
            }

            var alreadyEnrolled = rockContext.Set<GroupMember>()
                .Any( m => m.GroupId == group.Id && m.PersonId == person.Id );
            if ( alreadyEnrolled )
            {
                return result;
            }

            var roleId = group.GroupType != null && group.GroupType.DefaultGroupRoleId.HasValue
                ? group.GroupType.DefaultGroupRoleId.Value
                : 0;
            if ( roleId == 0 )
            {
                return result;
            }

            rockContext.Set<GroupMember>().Add( new GroupMember
            {
                GroupId = group.Id,
                GroupTypeId = group.GroupTypeId,
                PersonId = person.Id,
                GroupRoleId = roleId,
                GroupMemberStatus = GroupMemberStatus.Active,
                IsSystem = true
            } );

            rockContext.SaveChanges();
            return result;
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// True when the person's record status is Inactive, false when it is not, and null
        /// when the Inactive status is missing from this database so the question cannot be
        /// answered at all. A gate with no answer is not a gate that passed.
        /// </summary>
        private static bool? IsInactive( Person person, RockContext rockContext )
        {
            if ( !person.RecordStatusValueId.HasValue )
            {
                return false;
            }

            var inactiveGuid = Guid.Parse( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE );
            var inactive = rockContext.Set<DefinedValue>()
                .FirstOrDefault( v => v.Guid == inactiveGuid );
            if ( inactive == null )
            {
                return null;
            }

            return person.RecordStatusValueId == inactive.Id;
        }

        /// <summary>
        /// True when the person has an Active, not-archived Chat Ban List membership, and null
        /// when the Ban List group itself is absent. Reading an absent list as an empty one
        /// would clear the ban gate for every person in that church at once.
        /// </summary>
        private static bool? IsOnBanList( Person person, RockContext rockContext )
        {
            var banGuid = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST );
            var banGroup = rockContext.Set<Group>().FirstOrDefault( g => g.Guid == banGuid );
            if ( banGroup == null )
            {
                return null;
            }

            return rockContext.Set<GroupMember>().Any( m =>
                m.GroupId == banGroup.Id
                && m.PersonId == person.Id
                && m.GroupMemberStatus == GroupMemberStatus.Active
                && !m.IsArchived );
        }

        /// <summary>
        /// True when the DM Access Data View is blank or the person is in the
        /// already-resolved id set. Does not re-query the Data View.
        /// </summary>
        private static bool CanStartDirectMessage( Person person, ChatSessionConfiguration config )
        {
            if ( !config.DirectMessageAccessDataViewGuid.HasValue )
            {
                return true;
            }

            return config.DirectMessageAccessPersonIds != null
                && config.DirectMessageAccessPersonIds.Contains( person.Id );
        }

        /// <summary>
        /// Signs an ES256 church token. On a bad key, returns InvalidKey with no exception text.
        /// </summary>
        private static ChatMintResult Sign( ChatMintResult result, ChatSessionConfiguration config, string scope, Guid? subject )
        {
            try
            {
                var jwk = new JsonWebKey( config.PrivateKey );
                if ( !IsUsablePrivateKey( jwk ) )
                {
                    return Fail( ChatMintGate.InvalidKey );
                }

                var kid = jwk.Kid;
                var now = DateTime.UtcNow;
                var expires = now.AddMinutes( ChurchTokenLifetimeMinutes );

                var claims = new List<Claim>
                {
                    new Claim( "tid", config.TenantId.Value.ToString() ),
                    new Claim( "kid", kid ),
                    new Claim( "scp", scope )
                };

                if ( subject.HasValue )
                {
                    claims.Add( new Claim( JwtRegisteredClaimNames.Sub, subject.Value.ToString() ) );
                }

                var credentials = new SigningCredentials( jwk, SecurityAlgorithms.EcdsaSha256 );
                var descriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity( claims ),
                    NotBefore = now,
                    IssuedAt = now,
                    Expires = expires,
                    SigningCredentials = credentials
                };

                var handler = new JwtSecurityTokenHandler();
                var token = handler.CreateJwtSecurityToken( descriptor );
                token.Header["kid"] = kid;
                token.Header["typ"] = "JWT";

                var written = handler.WriteToken( token );
                var parsed = handler.ReadJwtToken( written );

                result.ChurchToken = written;
                result.Kid = kid;
                result.ExpiresAtUtc = new DateTimeOffset( parsed.ValidTo, TimeSpan.Zero );
                result.ErrorMessage = null;
                return result;
            }
            catch
            {
                return Fail( ChatMintGate.InvalidKey );
            }
        }

        /// <summary>
        /// True when the JWK is EC P-256 with a key id and a private part.
        /// </summary>
        private static bool IsUsablePrivateKey( JsonWebKey jwk )
        {
            if ( jwk == null )
            {
                return false;
            }

            if ( string.IsNullOrWhiteSpace( jwk.Kid ) || string.IsNullOrWhiteSpace( jwk.D ) )
            {
                return false;
            }

            if ( !string.Equals( jwk.Kty, "EC", StringComparison.Ordinal ) )
            {
                return false;
            }

            if ( !string.Equals( jwk.Crv, "P-256", StringComparison.Ordinal ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Builds a failed result with no token and no exception text.
        /// </summary>
        private static ChatMintResult Fail( ChatMintGate gate )
        {
            return new ChatMintResult { Gate = gate };
        }

        #endregion Private Methods
    }

    #region DTOs

    /// <summary>
    /// Ordered session gates that run before a church token is minted.
    /// </summary>
    internal enum ChatMintGate
    {
        Ok,
        SignInRequired,
        NotConfigured,
        Deceased,
        Inactive,
        Banned,
        AgeVerificationRequired,
        AgeRestricted,
        NoPrimaryAlias,
        InvalidKey,
        GateUnavailable
    }

    /// <summary>
    /// Outcome of a session gate, mint, or enrolment. Never carries exception text.
    /// </summary>
    internal sealed class ChatMintResult
    {
        public bool Success => Gate == ChatMintGate.Ok;

        public ChatMintGate Gate { get; set; }

        public string ChurchToken { get; set; }

        public DateTimeOffset? ExpiresAtUtc { get; set; }

        public Guid? PersonAliasGuid { get; set; }

        public Guid? TenantId { get; set; }

        public string Kid { get; set; }

        /// <summary>
        /// Whether this person may start a direct message. Evaluated once at
        /// session open from the DM Access Data View. Not a token claim.
        /// </summary>
        public bool CanStartDm { get; set; }

        /// <summary>
        /// Always left null. Failures are the <see cref="Gate"/> value, never
        /// an exception message that could leak key material.
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Church-side values the session helper needs until the settings POCO
    /// ships with Chat Configuration. Injected so tests do not go through
    /// Enable Chat.
    /// </summary>
    internal sealed class ChatSessionConfiguration
    {
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Private JWK JSON for ES256, including the private part. Never logged
        /// and never placed on a view-model bag.
        /// </summary>
        public string PrivateKey { get; set; }

        public string ProjectUrl { get; set; }

        public string PublishableKey { get; set; }

        public string Kid { get; set; }

        public int? MinimumAge { get; set; }

        public Guid? DirectMessageAccessDataViewGuid { get; set; }

        /// <summary>
        /// Person ids already resolved from the DM Access Data View. The helper
        /// does not run the Data View; the caller evaluates it once at session
        /// open. Null means the Data View is blank.
        /// </summary>
        public ISet<int> DirectMessageAccessPersonIds { get; set; }

        public bool IsConfigured =>
            TenantId.HasValue
            && !string.IsNullOrWhiteSpace( PrivateKey )
            && !string.IsNullOrWhiteSpace( ProjectUrl )
            && !string.IsNullOrWhiteSpace( PublishableKey );
    }

    #endregion DTOs
}
