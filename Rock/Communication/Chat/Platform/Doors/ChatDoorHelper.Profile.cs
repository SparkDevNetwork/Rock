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
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Session;
using Rock.Data;
using Rock.Lava;
using Rock.Logging;
using Rock.Model;
using Rock.ViewModels.Blocks.Communication.Chat.ChatShell;
using Rock.Web.Cache;

namespace Rock.Communication.Chat.Platform.Doors
{
    /// <summary>
    /// The read behind a person's profile card: the contact details and the church's own template
    /// that Rock alone holds, read as the card opens and never copied to the chat platform.
    /// </summary>
    /// <remarks>
    /// A partial of its own because, unlike every other door, it changes nothing and renders a
    /// church's Lava for another person to see, and the mobile app will be its second caller.
    /// </remarks>
    internal static partial class ChatDoorHelper
    {
        /// <summary>
        /// How many different people's details one person may read in a minute. Someone opening
        /// cards by hand reaches a handful; the bound is there so the contact details of a whole
        /// church cannot be read off in one sitting by anyone who can chat. An estimate, not a
        /// measurement.
        /// </summary>
        internal const int MaxProfilesReadPerMinute = 30;

        private static readonly TimeSpan ProfileReadPeriod = TimeSpan.FromMinutes( 1 );

        /// <summary>
        /// The contact details and template output a person shows on their profile card, for the
        /// person looking. Everything is decided here from Rock: the browser names only whose card
        /// it is, and nothing it or the chat platform says about that person is trusted.
        /// </summary>
        /// <param name="viewer">The person looking.</param>
        /// <param name="personAliasGuid">The person on the card, by any of their aliases.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="mergeFields">The page's common merge fields for the person looking, which the template sees beside the person on the card.</param>
        /// <param name="rockContext">The context read in.</param>
        /// <returns>The details, or an empty answer wherever chat would not show this person to this viewer.</returns>
        internal static ChatProfileDetailsBag GetProfileDetails( Person viewer, Guid personAliasGuid, ChatPlatformConfiguration configuration, IDictionary<string, object> mergeFields, RockContext rockContext )
        {
            // The session gates and nothing more: the Direct Message Access data view is not read,
            // since nothing here starts a conversation.
            var context = new ChatSessionContext { Configuration = configuration };

            if ( !ChatSessionHelper.Evaluate( viewer, context, rockContext ).Success )
            {
                return new ChatProfileDetailsBag();
            }

            // Counted before anything about the person is read, so a refused read costs nothing.
            if ( !ProfileReadsOf( viewer.Id ).TryAdmit( personAliasGuid, MaxProfilesReadPerMinute ) )
            {
                return new ChatProfileDetailsBag();
            }

            // Any alias, so a card opened under an alias merged away still finds the person.
            var target = new PersonAliasService( rockContext ).Queryable()
                .Where( a => a.Guid == personAliasGuid )
                .Select( a => a.Person )
                .FirstOrDefault();

            // Nobody chat itself refuses, by the same gates, and nobody chat has never seen: without
            // the second, a church that shows details by default would hand out the email of any
            // person whose alias leaked from another page.
            if ( target == null
                || !ChatSessionHelper.Evaluate( target, context, rockContext ).Success
                || !IsEnrolledInChat( target.Id, rockContext )
                || !IsShowingProfileDetails( target.IsChatProfilePublic, configuration.AreChatProfilesVisible ) )
            {
                return new ChatProfileDetailsBag();
            }

            var mobile = ReadListedMobilePhone( target.Id, rockContext );

            return new ChatProfileDetailsBag
            {
                IsShown = true,

                // An address Rock knows no longer works is left out; one the person asked not to be
                // sent bulk email at is still theirs to be reached at one to one.
                Email = !string.IsNullOrWhiteSpace( target.Email ) && target.IsEmailActive ? target.Email.Trim() : null,
                MobilePhone = mobile?.NumberFormatted,
                MobilePhoneUri = mobile != null ? ToTelUri( mobile.CountryCode, mobile.Number ) : null,
                LavaHtml = RenderProfileTemplate( configuration, mergeFields, viewer, target )
            };
        }

        /// <summary>
        /// Whether a person's profile details show to others: their own setting where they chose
        /// one, else the church's default. The same words the projection sends to the platform,
        /// so the card's details block and Rock's answer agree.
        /// </summary>
        /// <param name="personSetting">The person's own setting, or null when they never chose.</param>
        /// <param name="churchDefault">The church's default.</param>
        /// <returns>True when the details show.</returns>
        internal static bool IsShowingProfileDetails( bool? personSetting, bool churchDefault )
        {
            return personSetting ?? churchDefault;
        }

        /// <summary>
        /// The tel: address a phone dials from a card, in international form when Rock knows the
        /// country code.
        /// </summary>
        /// <param name="countryCode">The number's country code, or blank.</param>
        /// <param name="number">The number as Rock stores it.</param>
        /// <returns>The address, or null when the number has no digits.</returns>
        internal static string ToTelUri( string countryCode, string number )
        {
            var digits = new string( ( number ?? string.Empty ).Where( char.IsDigit ).ToArray() );
            if ( digits.Length == 0 )
            {
                return null;
            }

            var country = new string( ( countryCode ?? string.Empty ).Where( char.IsDigit ).ToArray() );
            return country.Length > 0 ? "tel:+" + country + digits : "tel:" + digits;
        }

        /// <summary>
        /// Whether Rock enrols this person in chat, by the projection's own three arms: the Chat
        /// People marker in any state; an active membership of the Ban List or the Chat
        /// Administrators; or an active membership of any group chat is or was enabled for.
        /// </summary>
        /// <param name="personId">The person.</param>
        /// <param name="rockContext">The context read in.</param>
        /// <returns>True when the projection would send this person to the platform.</returns>
        internal static bool IsEnrolledInChat( int personId, RockContext rockContext )
        {
            var chatPeople = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_PEOPLE );
            var banList = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST );
            var administrators = Guid.Parse( Rock.SystemGuid.Group.GROUP_CHAT_ADMINISTRATORS );

            return rockContext.Set<GroupMember>().Any( m =>
                m.PersonId == personId
                && ( m.Group.Guid == chatPeople
                    || ( m.GroupMemberStatus == GroupMemberStatus.Active
                        && !m.IsArchived
                        && ( m.Group.Guid == banList
                            || m.Group.Guid == administrators
                            || ( m.Group.ChatChannelFirstEnabledDateTime != null
                                || ( m.Group.GroupType.IsChatAllowed
                                    && ( m.Group.IsChatEnabledOverride ?? m.Group.GroupType.IsChatEnabledForAllGroups ) ) ) ) ) ) );
        }

        /// <summary>
        /// The person's mobile phone, unless Rock marks it unlisted.
        /// </summary>
        /// <param name="personId">The person.</param>
        /// <param name="rockContext">The context read in.</param>
        /// <returns>The number, or null.</returns>
        private static PhoneNumber ReadListedMobilePhone( int personId, RockContext rockContext )
        {
            var mobileTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( !mobileTypeId.HasValue )
            {
                return null;
            }

            return new PhoneNumberService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( p => p.PersonId == personId && p.NumberTypeValueId == mobileTypeId.Value && !p.IsUnlisted )
                .OrderBy( p => p.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// The church's profile card template rendered for this person and this viewer, made safe
        /// to draw in the viewer's page, or null when there is none or it failed.
        /// </summary>
        /// <param name="configuration">The church's chat settings, which hold the template and its commands.</param>
        /// <param name="mergeFields">The page's common merge fields for the viewer.</param>
        /// <param name="viewer">The person looking.</param>
        /// <param name="target">The person on the card.</param>
        /// <returns>The safe HTML, or null.</returns>
        private static string RenderProfileTemplate( ChatPlatformConfiguration configuration, IDictionary<string, object> mergeFields, Person viewer, Person target )
        {
            var template = configuration.ProfileCardLavaTemplate;
            if ( string.IsNullOrWhiteSpace( template ) )
            {
                return null;
            }

            // A copy, so the caller's fields are not changed; the person on the card is the one
            // field this adds, and CurrentPerson stays the viewer.
            var fields = mergeFields != null ? new Dictionary<string, object>( mergeFields ) : new Dictionary<string, object>();
            fields["CurrentPerson"] = viewer;
            fields["Person"] = target;

            var parameters = new LavaRenderParameters
            {
                Context = LavaService.NewRenderContext( fields, configuration.ProfileCardEnabledLavaCommands ?? new List<string>() ),

                // An error's text is meant for the person who wrote the template, never for
                // everyone who opens a card, so it is logged instead.
                ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Ignore
            };

            var result = LavaService.RenderTemplate( template, parameters );
            if ( result.HasErrors )
            {
                RockLogger.LoggerFactory.CreateLogger( typeof( ChatDoorHelper ).FullName )
                    .LogWarning( "The chat profile card template failed for person {PersonId}: {Error}", target.Id, result.Error?.Message );
                return null;
            }

            var html = ChatProfileHtml.Sanitize( result.Text );
            return string.IsNullOrWhiteSpace( html ) ? null : html;
        }

        /// <summary>
        /// The people this viewer has read in the current minute, held in Rock's cache.
        /// </summary>
        /// <param name="viewerId">The person looking.</param>
        /// <returns>The viewer's window, new when the last one has ended.</returns>
        private static ProfileReadWindow ProfileReadsOf( int viewerId )
        {
            return ( ProfileReadWindow ) RockCache.GetOrAddExisting(
                "ChatProfileReads:" + viewerId,
                "Chat",
                () => new ProfileReadWindow(),
                ProfileReadPeriod );
        }

        /// <summary>
        /// The different people one viewer has read in one window. Reading the same person again
        /// costs nothing, so reopening a card is never refused.
        /// </summary>
        internal sealed class ProfileReadWindow
        {
            private readonly HashSet<Guid> _read = new HashSet<Guid>();

            /// <summary>
            /// Counts a read of this person, unless the window already holds the most allowed.
            /// </summary>
            /// <param name="personAliasGuid">The person read.</param>
            /// <param name="max">The most different people a window allows.</param>
            /// <returns>True when the read is allowed.</returns>
            internal bool TryAdmit( Guid personAliasGuid, int max )
            {
                lock ( _read )
                {
                    if ( _read.Contains( personAliasGuid ) )
                    {
                        return true;
                    }

                    if ( _read.Count >= max )
                    {
                        return false;
                    }

                    _read.Add( personAliasGuid );
                    return true;
                }
            }
        }
    }
}
