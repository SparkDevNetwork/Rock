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
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Cms.ActiveUsers;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of active users of a website.
    /// </summary>

    [DisplayName( "Active Users" )]
    [Category( "CMS" )]
    [Description( "Displays a list of active users of a website." )]
    [IconCssClass( "fa fa-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [SiteField(
        "Site",
        Description = "Site to show current active users for.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.Site )]

    [BooleanField(
        "Show Site Name As Title",
        Description = "Determine whether to show the name of the site as a title above the list.",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.ShowSiteNameAsTitle )]

    [BooleanField(
        "Show Guest Visitors",
        Description = "Displays the number of guests visiting the site. (Guests are considered users not logged in.)",
        DefaultBooleanValue = true,
        Order = 2,
        Key = AttributeKey.ShowGuestVisitors )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page reference to the person profile page you would like to use as a link. Not providing a reference will suppress the creation of a link.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.PersonProfilePage )]

    [IntegerField(
        "Page View Count",
        Description = "The number of past page views to show on roll-over. A value of 0 will disable the roll-over.",
        IsRequired = true,
        DefaultIntegerValue = 5,
        Order = 4,
        Key = AttributeKey.PageViewCount )]

    #endregion Block Attributes

    [InitialBlockHeight( 0 )]
    [Rock.SystemGuid.EntityTypeGuid( "8EDB4E83-67CD-4B5E-A5EB-2DF1236E5E6F" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "12EF037C-ECBC-48E4-8285-7D6FBF7E18EA" )]
    [Rock.SystemGuid.BlockTypeGuid( "3E7033EE-31A3-4484-AFA9-240C856A500C" )]
    public class ActiveUsers : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Site = "Site";
            public const string ShowSiteNameAsTitle = "ShowSiteNameAsTitle";
            public const string ShowGuestVisitors = "ShowGuestVisitors";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string PageViewCount = "PageViewCount";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return BuildBox();
        }

        /// <summary>
        /// Builds the initialization box that will drive the block's rendered output.
        /// </summary>
        /// <returns>An <see cref="ActiveUsersInitializationBox"/> ready for serialization to the client.</returns>
        private ActiveUsersInitializationBox BuildBox()
        {
            var box = new ActiveUsersInitializationBox
            {
                ActiveUsers = new List<ActiveUserBag>()
            };

            var siteId = GetAttributeValue( AttributeKey.Site ).AsIntegerOrNull();
            var site = siteId.HasValue ? SiteCache.Get( siteId.Value ) : null;

            if ( site == null )
            {
                box.ErrorMessage = "No site is currently configured.";
                return box;
            }

            box.SiteName = site.Name;
            box.ShowSiteName = GetAttributeValue( AttributeKey.ShowSiteNameAsTitle ).AsBoolean();
            box.ShowGuestVisitors = GetAttributeValue( AttributeKey.ShowGuestVisitors ).AsBoolean();

            if ( !site.EnablePageViews )
            {
                box.ErrorMessage = $"Active {site.Name} users not available because page views are not enabled for site.";
                return box;
            }

            var pageViewCount = GetAttributeValue( AttributeKey.PageViewCount ).AsIntegerOrNull() ?? 0;
            box.ShowTooltip = pageViewCount > 0;

            box.ActiveUsers = GetActiveUsers( site.Id, pageViewCount );

            if ( box.ActiveUsers.Count == 0 )
            {
                box.EmptyMessage = $"There are no logged in users on the {site.Name} site.";
            }

            if ( box.ShowGuestVisitors )
            {
                GetGuestCounts( site.Id, out var recentGuests, out var inactiveGuests );
                box.RecentGuestCount = recentGuests;
                box.InactiveGuestCount = inactiveGuests;
            }

            return box;
        }

        /// <summary>
        /// Returns the list of active users currently browsing the specified site.
        /// Uses a correlated subquery to bound each user's page-view rows to <c>pageViewCount</c>
        /// (or 1 when tooltips are disabled), matching the WebForms block's row-count envelope.
        /// </summary>
        /// <param name="siteId">The identifier of the site to scope page-view matches to.</param>
        /// <param name="pageViewCount">The maximum number of recent page titles to include per user. <c>0</c> skips title fetching.</param>
        /// <returns>The ordered list of <see cref="ActiveUserBag"/> entries to display.</returns>
        internal List<ActiveUserBag> GetActiveUsers( int siteId, int pageViewCount )
        {
            var currentPersonId = RequestContext.CurrentPerson?.Id;

            var last24Hours = RockDateTime.Now.AddDays( -1 );

            /*
                pageViewTakeCount is always at least 1: even when pageViewCount is 0 (tooltips
                disabled) we still need the most recent interaction per user to decide whether
                that user is currently on the configured site.
            */
            var pageViewTakeCount = pageViewCount > 0 ? pageViewCount : 1;

            var pageViewQry = new InteractionService( RockContext ).Queryable()
                .Where( pv => pv.PersonAliasId.HasValue && pv.InteractionDateTime > last24Hours );

            var activeSessionsQuery = new PersonSessionService( RockContext ).Queryable()
                .Where( s => s.IsActive );

            if ( currentPersonId.HasValue )
            {
                activeSessionsQuery = activeSessionsQuery.Where( s => s.PersonAlias.PersonId != currentPersonId.Value );
            }

            // Collapse multiple sessions per person to a single row keyed on
            // the most recent LastActivityDateTime so each person shows up at
            // most once in the list. The Person fields are projected from the
            // PersonAlias.Person navigation; the GroupBy is materialized
            // server-side and a follow-up join pulls the projected Person
            // columns.
            var personQuery = new PersonService( RockContext ).Queryable();

            var activePeopleQuery = activeSessionsQuery
                .GroupBy( s => s.PersonAlias.PersonId )
                .Select( g => new
                {
                    PersonId = g.Key,
                    LastActivityDateTime = g.Max( s => s.LastActivityDateTime )
                } );

            // The inner .Take( pageViewTakeCount ) caps each user's row count, preventing
            // unbounded fetches on heavily-trafficked sites where admins may rack up thousands
            // of interactions per day.
            var activeLogins = activePeopleQuery
                .Join(
                    personQuery,
                    sessionRow => sessionRow.PersonId,
                    person => person.Id,
                    ( sessionRow, person ) => new ActiveLogin
                    {
                        LastActivityDateTime = sessionRow.LastActivityDateTime,
                        PersonId = sessionRow.PersonId,
                        NickName = person.NickName,
                        LastName = person.LastName,
                        SuffixValueId = person.SuffixValueId,
                        RecordTypeValueId = person.RecordTypeValueId,
                        PageViews = pageViewQry
                            .Where( pv => pv.PersonAlias.PersonId == sessionRow.PersonId )
                            .OrderByDescending( pv => pv.InteractionDateTime )
                            .Take( pageViewTakeCount )
                            .Select( pv => new ActiveLoginPageView
                            {
                                ChannelEntityId = pv.InteractionComponent.InteractionChannel.ChannelEntityId,
                                InteractionSessionId = pv.InteractionSessionId,
                                Title = pv.InteractionComponent.Name
                            } )
                            .ToList()
                    } )
                .OrderByDescending( l => l.LastActivityDateTime )
                .ToList();

            // Pre-resolve the profile page setting so we don't re-read it per iteration.
            var personProfilePageSetting = GetAttributeValue( AttributeKey.PersonProfilePage );
            var hasProfilePage = personProfilePageSetting.IsNotNullOrWhiteSpace();

            var results = new List<ActiveUserBag>();

            foreach ( var login in activeLogins )
            {
                if ( login.PageViews.Count == 0 )
                {
                    continue;
                }

                var mostRecent = login.PageViews[0];
                if ( mostRecent.ChannelEntityId != siteId )
                {
                    // Only show active logins whose most recent page view was for the configured site.
                    continue;
                }

                /*
                    4/13/26 - MSE

                    The original WebForms block compared TimeSpan.Minutes (0-59) instead of TotalMinutes,
                    which mislabeled users active more than an hour ago as "recent" whenever the minutes
                    component happened to be ≤ 5. Using TotalMinutes matches the user-visible intent
                    ("active in the last 5 minutes") stated in the guest-badge tooltip.

                    Reason: Preserve the block's documented behavior rather than its buggy implementation.
                */
                var timeSinceLastActivity = login.LastActivityDateTime.HasValue
                    ? RockDateTime.Now.Subtract( login.LastActivityDateTime.Value )
                    : TimeSpan.MaxValue;
                var isRecent = timeSinceLastActivity.TotalMinutes <= 5;

                var fullName = Person.FormatFullName( login.NickName, login.LastName, login.SuffixValueId, login.RecordTypeValueId );

                string profileUrl = null;
                if ( hasProfilePage )
                {
                    var url = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, new Dictionary<string, string>
                    {
                        [PageParameterKey.PersonId] = IdHasher.Instance.GetHash( login.PersonId )
                    } );

                    profileUrl = url.IsNullOrWhiteSpace() ? null : url;
                }

                List<string> pageTitles = null;
                if ( pageViewCount > 0 )
                {
                    var latestSessionId = mostRecent.InteractionSessionId;

                    // login.PageViews is already capped at pageViewTakeCount — filter to the
                    // latest session for the tooltip.
                    pageTitles = login.PageViews
                        .Where( pv => pv.InteractionSessionId == latestSessionId )
                        .Select( pv => pv.Title )
                        .ToList();
                }

                results.Add( new ActiveUserBag
                {
                    FullName = fullName,
                    ProfileUrl = profileUrl,
                    IsRecent = isRecent,
                    PageTitles = pageTitles
                } );
            }

            return results;
        }

        /// <summary>
        /// Counts guest (anonymous) visitor sessions for the site over the last 15 minutes,
        /// split into "recent" (≤ 5 min) and "inactive" (5-15 min) buckets.
        /// </summary>
        /// <param name="siteId">The identifier of the site to scope guest sessions to.</param>
        /// <param name="recentGuestCount">Outputs the number of guest sessions active in the last 5 minutes.</param>
        /// <param name="inactiveGuestCount">Outputs the number of guest sessions whose most recent activity was between 5 and 15 minutes ago.</param>
        private void GetGuestCounts( int siteId, out int recentGuestCount, out int inactiveGuestCount )
        {
            var last5Minutes = RockDateTime.Now.AddMinutes( -5 );
            var last15Minutes = RockDateTime.Now.AddMinutes( -15 );

            var guestSessions = new InteractionService( RockContext ).Queryable()
                .Where( i =>
                    i.InteractionComponent.InteractionChannel.ChannelEntityId == siteId &&
                    i.InteractionDateTime > last15Minutes &&
                    i.PersonAliasId == null &&
                    i.InteractionSession.DeviceType.ClientType != "Other" &&
                    i.InteractionSession.DeviceType.ClientType != "Crawler" )
                .GroupBy( i => i.InteractionSessionId )
                .Select( g => new
                {
                    LastVisit = g.Max( i => i.InteractionDateTime )
                } )
                .ToList();

            // Every session in guestSessions has LastVisit within the last 15 minutes
            // (from the query filter), so it's either recent (≥ last5Minutes) or inactive.
            // Count once and derive the other via subtraction to avoid a second pass.
            recentGuestCount = guestSessions.Count( g => g.LastVisit >= last5Minutes );
            inactiveGuestCount = guestSessions.Count - recentGuestCount;
        }

        #endregion Methods

        #region Supporting Types

        /// <summary>
        /// Internal query projection for an active person session together with
        /// that person's most recent page-view interactions. Shape is driven by
        /// <see cref="GetActiveUsers( int, int )"/>; not intended for use outside this block.
        /// </summary>
        private class ActiveLogin
        {
            /// <summary>
            /// Gets or sets the maximum LastActivityDateTime across the
            /// person's active <see cref="PersonSession"/> rows — drives the
            /// recent vs not-recent indicator color.
            /// </summary>
            public DateTime? LastActivityDateTime { get; set; }

            /// <summary>
            /// Gets or sets the Person identifier tied to this UserLogin.
            /// </summary>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the person's nick name.
            /// </summary>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the person's last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the person's suffix DefinedValue identifier.
            /// </summary>
            public int? SuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's record type DefinedValue identifier — lets
            /// <see cref="Person.FormatFullName(string, string, int?, int?)"/> handle
            /// Business/Nameless records correctly.
            /// </summary>
            public int? RecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets this user's most recent page-view interactions, ordered
            /// most-recent first and capped at the block's Page View Count setting
            /// (or 1 when the setting is 0).
            /// </summary>
            public List<ActiveLoginPageView> PageViews { get; set; }
        }

        /// <summary>
        /// Internal query projection for a single recent page-view interaction belonging to an
        /// <see cref="ActiveLogin"/>.
        /// </summary>
        private class ActiveLoginPageView
        {
            /// <summary>
            /// Gets or sets the ChannelEntityId of the interaction's InteractionChannel —
            /// compared against the configured Site.Id to decide whether the user is
            /// currently on this site.
            /// </summary>
            public int? ChannelEntityId { get; set; }

            /// <summary>
            /// Gets or sets the InteractionSessionId — used to scope the tooltip titles to
            /// the user's latest session only.
            /// </summary>
            public int? InteractionSessionId { get; set; }

            /// <summary>
            /// Gets or sets the interaction component name (the page title).
            /// </summary>
            public string Title { get; set; }
        }

        #endregion Supporting Types
    }
}
