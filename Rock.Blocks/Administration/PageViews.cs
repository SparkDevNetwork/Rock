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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Administration.PageViews;
using Rock.ViewModels.Controls;
using Rock.Web.Cache;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Lists the website interactions (page views) recorded for a particular page.
    /// </summary>
    [DisplayName( "Page Views" )]
    [Category( "Administration" )]
    [Description( "Lists interactions with a particular page." )]
    [IconCssClass( "ti ti-file" )]
    [SupportedSiteTypes( SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "0D351126-3E71-427D-91E7-B2E522504F19" )]
    // Was [Rock.SystemGuid.BlockTypeGuid( "32639026-CD8E-4D6A-BDFD-812C01B0BDAF" )]
    [Rock.SystemGuid.BlockTypeGuid( "38C775A7-5CDC-415E-9595-76221354A999" )]
    [CustomizedGrid]
    public class PageViews : RockListBlockType<PageViews.PageViewInteraction>
    {
        #region Keys

        /// <summary>
        /// Keys for the page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The identifier of the page whose interactions are being listed.
            /// </summary>
            public const string Page = "Page";
        }

        /// <summary>
        /// Keys for the filter person preferences.
        /// </summary>
        private static class PersonPreferenceKey
        {
            /// <summary>
            /// The sliding date range applied to the interaction date.
            /// </summary>
            public const string DateRange = "date-range";

            /// <summary>
            /// The login status (logged in, not logged in, or both).
            /// </summary>
            public const string LoginStatus = "login-status";

            /// <summary>
            /// The text the interaction URL must contain.
            /// </summary>
            public const string UrlContains = "url-contains";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the prefix applied to each filter preference key so that the filter
        /// selections are remembered independently for each page being viewed.
        /// </summary>
        private string PreferenceKeyPrefix
        {
            get
            {
                var page = GetInteractionPage();

                return page != null ? $"{page.Guid}-" : string.Empty;
            }
        }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<PageViewsOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private PageViewsOptionsBag GetBoxOptions()
        {
            var page = GetInteractionPage();

            return new PageViewsOptionsBag
            {
                Title = page != null ? $"{page.InternalName} Page Views" : "Page Views",
                PreferenceKeyPrefix = PreferenceKeyPrefix
            };
        }

        /// <summary>
        /// Gets the page whose interactions are being listed, based on the page parameter.
        /// </summary>
        /// <remarks>
        /// The page parameter may be supplied as an Id, IdKey, or Guid. Integer identifiers
        /// are only honored when the site has not disabled predictable ids.
        /// </remarks>
        /// <returns>The <see cref="PageCache"/> for the requested page, or <see langword="null"/> if it cannot be resolved.</returns>
        private PageCache GetInteractionPage()
        {
            var pageKey = PageParameter( PageParameterKey.Page );
            var allowIntegerIdentifier = !PageCache.Layout.Site.DisablePredictableIds;

            return PageCache.Get( pageKey, allowIntegerIdentifier );
        }

        /// <inheritdoc/>
        protected override IQueryable<PageViewInteraction> GetListQueryable( RockContext rockContext )
        {
            var page = GetInteractionPage();

            if ( page == null )
            {
                return Enumerable.Empty<PageViewInteraction>().AsQueryable();
            }

            var websiteMediumValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() )?.Id;

            if ( !websiteMediumValueId.HasValue )
            {
                return Enumerable.Empty<PageViewInteraction>().AsQueryable();
            }

            /*
                6/26/26 - MSE

                The interaction component ids for the page are resolved in their own query and
                materialized before the interaction query runs. Folding this into a single query
                against the (often very large) Interaction table produces a plan that can time out,
                whereas a page only maps to a handful of components.

                Reason: Avoid an Interaction query timeout on high-traffic pages.
            */
            var componentIds = new InteractionComponentService( rockContext )
                .Queryable()
                .Where( ic => ic.InteractionChannel.ChannelTypeMediumValueId == websiteMediumValueId.Value )
                .Where( ic => ic.EntityId == page.Id )
                .Select( ic => ic.Id )
                .ToList();

            if ( !componentIds.Any() )
            {
                return Enumerable.Empty<PageViewInteraction>().AsQueryable();
            }

            var interactionQuery = new InteractionService( rockContext )
                .Queryable()
                .Where( i => componentIds.Contains( i.InteractionComponentId ) );

            interactionQuery = ApplyFilters( interactionQuery );

            return interactionQuery.Select( i => new PageViewInteraction
            {
                Id = i.Id,
                InteractionDateTime = i.InteractionDateTime,
                TimeToServe = i.InteractionTimeToServe,
                PersonId = ( int? ) i.PersonAlias.Person.Id,
                PersonNickName = i.PersonAlias.Person.NickName,
                PersonFirstName = i.PersonAlias.Person.FirstName,
                PersonLastName = i.PersonAlias.Person.LastName,
                Url = i.InteractionData
            } );
        }

        /// <summary>
        /// Applies the saved filter preferences to the interaction query.
        /// </summary>
        /// <param name="interactionQuery">The interaction query to filter.</param>
        /// <returns>The filtered interaction query.</returns>
        private IQueryable<Interaction> ApplyFilters( IQueryable<Interaction> interactionQuery )
        {
            var preferences = GetBlockPersonPreferences();
            var prefix = PreferenceKeyPrefix;

            // Apply the sliding date range, defaulting to the last 90 days when no valid
            // range has been selected so the grid payload stays bounded on busy pages.
            var defaultDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Day,
                TimeValue = 90
            };

            var dateRange = preferences
                .GetValue( prefix + PersonPreferenceKey.DateRange )
                .ToSlidingDateRangeBagOrNull()
                .Validate( defaultDateRange )
                .ActualDateRange;

            if ( dateRange?.Start.HasValue == true )
            {
                interactionQuery = interactionQuery.Where( i => i.InteractionDateTime >= dateRange.Start.Value );
            }

            if ( dateRange?.End.HasValue == true )
            {
                interactionQuery = interactionQuery.Where( i => i.InteractionDateTime <= dateRange.End.Value );
            }

            // Apply the login status filter (logged in, not logged in, or both).
            var isAuthenticated = preferences.GetValue( prefix + PersonPreferenceKey.LoginStatus ).AsBooleanOrNull();

            if ( isAuthenticated.HasValue )
            {
                interactionQuery = interactionQuery.Where( i => i.PersonAliasId.HasValue == isAuthenticated.Value );
            }

            // Apply the URL contains filter.
            var urlContains = preferences.GetValue( prefix + PersonPreferenceKey.UrlContains );

            if ( urlContains.IsNotNullOrWhiteSpace() )
            {
                interactionQuery = interactionQuery.Where( i => i.InteractionData.Contains( urlContains ) );
            }

            return interactionQuery;
        }

        /// <inheritdoc/>
        protected override IQueryable<PageViewInteraction> GetOrderedListQueryable( IQueryable<PageViewInteraction> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.InteractionDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<PageViewInteraction> GetGridBuilder()
        {
            return new GridBuilder<PageViewInteraction>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.Id.AsIdKey() )
                .AddDateTimeField( "interactionDateTime", a => a.InteractionDateTime )
                .AddField( "timeToServe", a => a.TimeToServe )
                .AddPersonField( "loggedInUser", a =>
                {
                    if ( !a.PersonId.HasValue )
                    {
                        return null;
                    }

                    // Fall back to the first name when no nickname is recorded.
                    var nickName = a.PersonNickName.IsNotNullOrWhiteSpace() ? a.PersonNickName : a.PersonFirstName;

                    return new Person
                    {
                        Id = a.PersonId.Value,
                        NickName = nickName,
                        LastName = a.PersonLastName
                    };
                } )
                .AddTextField( "url", a => a.Url );
        }

        #endregion Methods

        #region Support Classes

        /// <summary>
        /// A lightweight projection of an <see cref="Interaction"/> used to populate the grid.
        /// </summary>
        public class PageViewInteraction
        {
            /// <summary>
            /// Gets or sets the interaction identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the date and time the interaction occurred.
            /// </summary>
            public DateTime InteractionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the number of seconds the server took to serve the page.
            /// </summary>
            public double? TimeToServe { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person who generated the interaction, if any.
            /// </summary>
            public int? PersonId { get; set; }

            /// <summary>
            /// Gets or sets the nickname of the person who generated the interaction, if any.
            /// </summary>
            public string PersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the first name of the person who generated the interaction, if any.
            /// </summary>
            public string PersonFirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person who generated the interaction, if any.
            /// </summary>
            public string PersonLastName { get; set; }

            /// <summary>
            /// Gets or sets the URL that was viewed.
            /// </summary>
            public string Url { get; set; }
        }

        #endregion Support Classes
    }
}
