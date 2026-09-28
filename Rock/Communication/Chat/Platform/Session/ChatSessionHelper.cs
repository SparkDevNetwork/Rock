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

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.Web.Cache;

namespace Rock.Communication.Chat.Platform.Session
{
    /// <summary>
    /// Gates a person, mints a short-lived ES256 church token, writes the
    /// Chat People marker on first open, and records a birthdate chat asked for.
    /// </summary>
    /// <remarks>
    /// A chat block makes one call here per thing it answers, so the rules live in this
    /// assembly, where the unit tests reach them, rather than in the blocks assembly, where a
    /// rule could be removed with every test green.
    /// </remarks>
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
        /// <param name="context">The church settings, and the people the DM Access Data View resolved to.</param>
        /// <param name="rockContext">Used for Ban List and record-status lookups.</param>
        /// <returns>The gate outcome, with no token.</returns>
        public static ChatMintResult Evaluate( Person person, ChatSessionContext context, RockContext rockContext )
        {
            if ( rockContext == null )
            {
                throw new ArgumentNullException( nameof( rockContext ),
                    "The record status and ban gates read the database, so a caller that passes no context would be granted a token that neither gate had looked at." );
            }

            context = context ?? new ChatSessionContext();
            var configuration = context.Configuration ?? new ChatPlatformConfiguration();

            if ( person == null || person.Id <= 0 )
            {
                return Fail( ChatMintGate.SignInRequired );
            }

            if ( !configuration.IsConfigured )
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

            if ( configuration.MinimumAge.HasValue && configuration.MinimumAge.Value > 0 )
            {
                if ( !person.Age.HasValue )
                {
                    return Fail( ChatMintGate.AgeVerificationRequired );
                }

                if ( person.Age.Value < configuration.MinimumAge.Value )
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
                TenantId = configuration.TenantId,
                CanStartDm = CanStartDirectMessage( person, context )
            };
        }

        /// <summary>
        /// Evaluates the gates and, if they pass, signs a person church token.
        /// </summary>
        /// <param name="person">The Rock person opening chat.</param>
        /// <param name="context">The church settings, and the people the DM Access Data View resolved to.</param>
        /// <param name="rockContext">Used for Ban List and record-status lookups.</param>
        /// <returns>A signed token on success, or the failing gate with no token and no exception text.</returns>
        public static ChatMintResult TryMintChurchToken( Person person, ChatSessionContext context, RockContext rockContext )
        {
            var result = Evaluate( person, context, rockContext );
            if ( result.Gate != ChatMintGate.Ok )
            {
                return result;
            }

            return Sign( result, context.Configuration, PersonScope, person.PrimaryAliasGuid );
        }

        /// <summary>
        /// Signs a sync-scope church token. No person gates.
        /// </summary>
        /// <param name="context">The church settings, and the people the DM Access Data View resolved to.</param>
        /// <returns>A signed token on success, or NotConfigured / InvalidKey with no exception text.</returns>
        public static ChatMintResult TryMintSyncToken( ChatSessionContext context )
        {
            var configuration = context?.Configuration ?? new ChatPlatformConfiguration();
            if ( !configuration.IsConfigured )
            {
                return Fail( ChatMintGate.NotConfigured );
            }

            var result = new ChatMintResult
            {
                Gate = ChatMintGate.Ok,
                TenantId = configuration.TenantId
            };

            // The platform exchanges a sync token as it does a person's and refuses one with no
            // subject. No person stands behind a sync, so the church itself is the subject.
            return Sign( result, configuration, SyncScope, configuration.TenantId );
        }

        /// <summary>
        /// Evaluates the gates and, if they pass, writes one Chat People marker
        /// on first open. Later opens write none. A failed gate enrols nobody.
        /// </summary>
        /// <param name="person">The Rock person opening chat.</param>
        /// <param name="context">The church settings, and the people the DM Access Data View resolved to.</param>
        /// <param name="rockContext">Used for gates and the marker write.</param>
        /// <returns>The gate outcome. Marker insert is a side effect on success.</returns>
        public static ChatMintResult EnsureEnrollment( Person person, ChatSessionContext context, RockContext rockContext )
        {
            var result = Evaluate( person, context, rockContext );
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

        /// <summary>
        /// Runs the session gates for the person opening chat, enrols them on their first open,
        /// and describes the outcome for the browser. Never carries a token or a key.
        /// </summary>
        /// <param name="person">The person opening chat, or null when nobody is signed in.</param>
        /// <param name="context">The church's settings and the direct message access result.</param>
        /// <param name="rockContext">Used by the gates and the enrolment write.</param>
        /// <returns>The gate outcome and, when it passed, the public platform settings.</returns>
        internal static ChatShellSessionBag OpenSession( Person person, ChatSessionContext context, RockContext rockContext )
        {
            var result = EnsureEnrollment( person, context, rockContext );

            if ( result.Gate != ChatMintGate.Ok )
            {
                return new ChatShellSessionBag { Gate = ToGateCode( result.Gate ) };
            }

            var configuration = context.Configuration;

            return new ChatShellSessionBag
            {
                Gate = ToGateCode( result.Gate ),
                ProjectUrl = configuration.ProjectUrl,
                PublishableKey = configuration.PublishableKey,
                TenantId = result.TenantId,
                PersonAliasGuid = result.PersonAliasGuid,
                CanStartDm = result.CanStartDm
            };
        }

        /// <summary>
        /// Re-runs every session gate and, when they pass, signs a church token for the person.
        /// </summary>
        /// <param name="person">The person asking, or null when nobody is signed in.</param>
        /// <param name="context">The church's settings and the direct message access result.</param>
        /// <param name="rockContext">Used by the gates.</param>
        /// <returns>The token and its expiry, or the gate that refused it.</returns>
        internal static ChatChurchTokenBag MintToken( Person person, ChatSessionContext context, RockContext rockContext )
        {
            var result = TryMintChurchToken( person, context, rockContext );

            if ( result.Gate != ChatMintGate.Ok )
            {
                return new ChatChurchTokenBag { Gate = ToGateCode( result.Gate ) };
            }

            return new ChatChurchTokenBag
            {
                Gate = ToGateCode( result.Gate ),
                ChurchToken = result.ChurchToken,
                ExpiresAt = result.ExpiresAtUtc
            };
        }

        /// <summary>
        /// Writes the person's birthdate when chat is asking for it and Rock does not already
        /// hold it, then describes the session as it now stands. It writes only what Rock does
        /// not already know, so it can never be used to change a recorded age.
        /// </summary>
        /// <param name="personId">The signed-in person, or null when nobody is signed in.</param>
        /// <param name="year">The year given, or zero when none was.</param>
        /// <param name="month">The month given, or zero when none was.</param>
        /// <param name="day">The day given, or zero when none was.</param>
        /// <param name="context">The church's settings and the direct message access result.</param>
        /// <param name="rockContext">Used to read and write the person, and by the gates.</param>
        /// <returns>What happened, and the session after it.</returns>
        internal static ChatBirthdateResultBag SaveBirthdate( int? personId, int year, int month, int day, ChatSessionContext context, RockContext rockContext )
        {
            var person = personId.HasValue ? new PersonService( rockContext ).Get( personId.Value ) : null;

            if ( person == null )
            {
                return BirthdateRefused( ChatBirthdateCode.SignInRequired, null, context, rockContext );
            }

            if ( person.BirthYear.HasValue && person.BirthMonth.HasValue && person.BirthDay.HasValue )
            {
                return BirthdateRefused( ChatBirthdateCode.BirthdateRecorded, person, context, rockContext );
            }

            // Every gate ahead of the age gate runs first, so a person chat would refuse for any
            // other reason is never asked, and nothing is written for them.
            if ( Evaluate( person, context, rockContext ).Gate != ChatMintGate.AgeVerificationRequired )
            {
                return BirthdateRefused( ChatBirthdateCode.NotAsked, person, context, rockContext );
            }

            var birthDate = ToBirthDate( year, month, day );

            if ( !birthDate.HasValue )
            {
                return BirthdateRefused( ChatBirthdateCode.InvalidDate, person, context, rockContext );
            }

            var isRecordedPartDifferent = ( person.BirthYear.HasValue && person.BirthYear.Value != year )
                || ( person.BirthMonth.HasValue && person.BirthMonth.Value != month )
                || ( person.BirthDay.HasValue && person.BirthDay.Value != day );

            if ( isRecordedPartDifferent )
            {
                return BirthdateRefused( ChatBirthdateCode.BirthdateRecorded, person, context, rockContext );
            }

            person.SetBirthDate( birthDate.Value );
            rockContext.SaveChanges();

            return new ChatBirthdateResultBag
            {
                Code = ChatBirthdateCode.Saved,
                Session = OpenSession( person, context, rockContext )
            };
        }

        /// <summary>
        /// Opens chat for the person as a chat block's page load does, reading the church's
        /// settings and whether the person may start a direct message.
        /// </summary>
        /// <param name="person">The person opening chat, or null when nobody is signed in.</param>
        /// <param name="rockContext">Used by the gates, the data view and the enrolment write.</param>
        /// <returns>The gate outcome and, when it passed, the public platform settings.</returns>
        public static ChatShellSessionBag OpenSession( Person person, RockContext rockContext )
        {
            return OpenSession( person, BuildSessionContext( person, ChatPlatformConfigurationService.Read(), rockContext ), rockContext );
        }

        /// <summary>
        /// Signs a church token for the person as a chat block's token action does, after running
        /// every gate again.
        /// </summary>
        /// <param name="person">The person asking, or null when nobody is signed in.</param>
        /// <param name="rockContext">Used by the gates.</param>
        /// <returns>The token and its expiry, or the gate that refused it.</returns>
        public static ChatChurchTokenBag MintToken( Person person, RockContext rockContext )
        {
            // Whether the person may start a direct message is not in the token, so the data
            // view is left out: every open person asks for a token every few minutes, and a data
            // view that fails must not stop chat for everyone.
            var context = new ChatSessionContext { Configuration = ChatPlatformConfigurationService.Read() };

            return MintToken( person, context, rockContext );
        }

        /// <summary>
        /// Records the birthdate chat asked the person for, as a chat block's birthdate action
        /// does, reading the church's settings and whether the person may start a direct message.
        /// </summary>
        /// <param name="person">The signed-in person, or null when nobody is signed in.</param>
        /// <param name="year">The year given, or zero when none was.</param>
        /// <param name="month">The month given, or zero when none was.</param>
        /// <param name="day">The day given, or zero when none was.</param>
        /// <param name="rockContext">Used to read and write the person, by the gates and the data view.</param>
        /// <returns>What happened, and the session after it.</returns>
        public static ChatBirthdateResultBag SaveBirthdate( Person person, int year, int month, int day, RockContext rockContext )
        {
            var context = BuildSessionContext( person, ChatPlatformConfigurationService.Read(), rockContext );

            return SaveBirthdate( person?.Id, year, month, day, context, rockContext );
        }

        /// <summary>
        /// Pairs the church's settings with whether this person is in the church's Direct Message
        /// Access data view. Only this person is looked for, so the data view is never read whole
        /// for one open.
        /// </summary>
        /// <param name="person">The person opening chat, or null.</param>
        /// <param name="configuration">The church's settings.</param>
        /// <param name="rockContext">The context the data view is read in.</param>
        /// <returns>The session context the gates read.</returns>
        internal static ChatSessionContext BuildSessionContext( Person person, ChatPlatformConfiguration configuration, RockContext rockContext )
        {
            var context = new ChatSessionContext { Configuration = configuration };

            if ( person == null || !configuration.DirectMessageAccessDataViewGuid.HasValue )
            {
                return context;
            }

            // A data view that no longer exists admits nobody, so a deleted data view never
            // opens direct messages to everyone.
            context.DirectMessageAccessPersonIds = new HashSet<int>();

            var dataView = DataViewCache.Get( configuration.DirectMessageAccessDataViewGuid.Value, rockContext );
            if ( dataView == null )
            {
                return context;
            }

            var isInDataView = dataView.GetQuery( new GetQueryableOptions { DbContext = rockContext } )
                .Any( entity => entity.Id == person.Id );

            if ( isInDataView )
            {
                context.DirectMessageAccessPersonIds.Add( person.Id );
            }

            return context;
        }

        /// <summary>
        /// The stable snake_case code a gate outcome is known by outside Rock.
        /// </summary>
        /// <param name="gate">The gate outcome.</param>
        /// <returns>The code.</returns>
        public static string ToGateCode( ChatMintGate gate )
        {
            // The enum's own names, in the snake_case every code outside Rock uses, so a new
            // gate has a code the moment it exists and two gates can never share one.
            return System.Text.RegularExpressions.Regex.Replace( gate.ToString(), "(?<=[a-z0-9])([A-Z])", "_$1" ).ToLowerInvariant();
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
        private static bool CanStartDirectMessage( Person person, ChatSessionContext context )
        {
            if ( !context.Configuration.DirectMessageAccessDataViewGuid.HasValue )
            {
                return true;
            }

            return context.DirectMessageAccessPersonIds != null
                && context.DirectMessageAccessPersonIds.Contains( person.Id );
        }

        /// <summary>
        /// Signs an ES256 church token. On a bad key, returns InvalidKey with no exception text.
        /// </summary>
        private static ChatMintResult Sign( ChatMintResult result, ChatPlatformConfiguration config, string scope, Guid? subject )
        {
            try
            {
                var jwk = new JsonWebKey( config.PrivateKey );
                if ( !ChatSigningKey.IsUsable( jwk ) )
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
        /// Builds a failed result with no token and no exception text.
        /// </summary>
        private static ChatMintResult Fail( ChatMintGate gate )
        {
            return new ChatMintResult { Gate = gate };
        }

        /// <summary>
        /// The date the three parts name, when they name a real one no later than today.
        /// </summary>
        /// <param name="year">The year given.</param>
        /// <param name="month">The month given.</param>
        /// <param name="day">The day given.</param>
        /// <returns>The date, or null when the parts do not make one chat may record.</returns>
        private static DateTime? ToBirthDate( int year, int month, int day )
        {
            // Rock stores year 1 as "no year", so a date in it would be saved without one.
            if ( year <= DateTime.MinValue.Year || year > 9999 || month < 1 || month > 12 )
            {
                return null;
            }

            if ( day < 1 || day > DateTime.DaysInMonth( year, month ) )
            {
                return null;
            }

            var date = new DateTime( year, month, day );

            return date > RockDateTime.Today ? ( DateTime? ) null : date;
        }

        /// <summary>
        /// A birthdate refusal, carrying the session as it stands so the shell can follow it.
        /// </summary>
        /// <param name="code">Why nothing was written.</param>
        /// <param name="person">The person, or null when nobody is signed in.</param>
        /// <param name="context">The church's settings and the direct message access result.</param>
        /// <param name="rockContext">Used by the gates.</param>
        /// <returns>The refusal.</returns>
        private static ChatBirthdateResultBag BirthdateRefused( string code, Person person, ChatSessionContext context, RockContext rockContext )
        {
            return new ChatBirthdateResultBag
            {
                Code = code,
                Session = OpenSession( person, context, rockContext )
            };
        }

        #endregion Private Methods
    }

    #region DTOs

    /// <summary>
    /// The stable codes a birthdate save answers with.
    /// </summary>
    internal static class ChatBirthdateCode
    {
        /// <summary>
        /// The birthdate was written.
        /// </summary>
        public const string Saved = "saved";

        /// <summary>
        /// Rock already holds a birthdate, or a part of one this date disagrees with.
        /// </summary>
        public const string BirthdateRecorded = "birthdate_recorded";

        /// <summary>
        /// Chat is not asking this person for a birthdate.
        /// </summary>
        public const string NotAsked = "not_asked";

        /// <summary>
        /// The date is incomplete, does not exist, or is after today.
        /// </summary>
        public const string InvalidDate = "invalid_date";

        /// <summary>
        /// Nobody is signed in.
        /// </summary>
        public const string SignInRequired = "sign_in_required";
    }

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

    #endregion DTOs
}
