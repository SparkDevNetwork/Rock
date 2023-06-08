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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Lists interactions with a particular page.
    /// </summary>
    [DisplayName( "Page Views" )]
    [Category( "Administration" )]
    [Description( "Lists interactions with a particular page." )]

    [Rock.SystemGuid.BlockTypeGuid( "38C775A7-5CDC-415E-9595-76221354A999" )]
    public partial class PageViews : RockBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Page Param Keys
        /// </summary>
        private static class PageParamKey
        {
            /// <summary>
            /// The page identifier
            /// </summary>
            public const string Page = "Page";
        }

        /// <summary>
        /// Keys for the grid filters
        /// </summary>
        private static class FilterKey
        {
            /// <summary>
            /// The date range
            /// </summary>
            public const string DateRange = "DateRange";

            /// <summary>
            /// The login status
            /// </summary>
            public const string LoginStatus = "LoginStatus";

            /// <summary>
            /// The URL contains
            /// </summary>
            public const string UrlContains = "UrlContains";
        }

        #endregion Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gInteractions.DataKeyNames = new string[] { "InteractionId" };
            gInteractions.GridRebind += gInteractions_GridRebind;
            gInteractions.RowItemText = "Page View";
            gInteractions.AllowSorting = true;

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                SetBlockTitle();
                BindLoggedInStatuses();
                InitializeFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Grid

        /// <summary>
        /// Initializes the filter.
        /// </summary>
        private void InitializeFilter()
        {
            var pageCache = GetPageCache();

            if ( pageCache != null )
            {
                rFilter.PreferenceKeyPrefix = string.Format( "{0}-", pageCache.Guid );
            }

            BindFilter();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            sdrpDateRange.DelimitedValues = rFilter.GetFilterPreference( FilterKey.DateRange );
            rblIsAuthenticated.SelectedValue = rFilter.GetFilterPreference( FilterKey.LoginStatus );
            tbUrlContains.Text = rFilter.GetFilterPreference( FilterKey.UrlContains );
        }

        /// <summary>
        /// Handles the GridRebind event of the gInteractions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gInteractions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var viewModelQuery = GetViewModelQuery();

            // Filter the results by the date range
            var startDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDateRange.DelimitedValues );

            if ( startDateRange.Start.HasValue )
            {
                viewModelQuery = viewModelQuery.Where( vm => vm.InteractionDateTime >= startDateRange.Start.Value );
            }

            if ( startDateRange.End.HasValue )
            {
                viewModelQuery = viewModelQuery.Where( vm => vm.InteractionDateTime <= startDateRange.End.Value );
            }

            // Filter by the authenticated state
            var isAuthenticated = rblIsAuthenticated.SelectedValue.AsBooleanOrNull();

            if ( isAuthenticated.HasValue )
            {
                viewModelQuery = viewModelQuery.Where( vm => vm.PersonAliasId.HasValue == isAuthenticated.Value );
            }

            // Filter by URL
            var url = tbUrlContains.Text;

            if ( !url.IsNullOrWhiteSpace() )
            {
                viewModelQuery = viewModelQuery.Where( vm => vm.Url.Contains( url ) );
            }

            // Sort the results
            var sortProperty = gInteractions.SortProperty ?? new SortProperty
            {
                Direction = SortDirection.Descending,
                Property = "InteractionDateTime"
            };

            viewModelQuery = viewModelQuery.Sort( sortProperty );
            gInteractions.SetLinqDataSource( viewModelQuery );
            gInteractions.DataBind();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( FilterKey.DateRange, "Date Range", sdrpDateRange.DelimitedValues );
            rFilter.SetFilterPreference( FilterKey.LoginStatus, "Login Status", rblIsAuthenticated.SelectedValue.AsBooleanOrNull().ToStringSafe() );
            rFilter.SetFilterPreference( FilterKey.UrlContains, "URL Contains", tbUrlContains.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case FilterKey.DateRange:
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    break;
                case FilterKey.LoginStatus:
                    var boolValue = e.Value.AsBooleanOrNull();
                    e.Value =
                        boolValue == null ? "Both" :
                        boolValue == true ? "Logged In" :
                        "Not Logged In";
                    break;
                case FilterKey.UrlContains:
                    break;
                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        #endregion Grid

        #region Internal Methods

        /// <summary>
        /// Sets the block title.
        /// </summary>
        private void SetBlockTitle()
        {
            var pageCache = GetPageCache();

            if ( pageCache == null )
            {
                lBlockTitle.Text = "Page Views";
                return;
            }

            lBlockTitle.Text = string.Format( "{0} Page Views", pageCache.InternalName );
        }

        /// <summary>
        /// Binds the logged in statuses.
        /// </summary>
        private void BindLoggedInStatuses()
        {
            rblIsAuthenticated.DataSource = new List<KeyValuePair<string, bool?>>
            {
                new KeyValuePair<string, bool?>("Logged In", true),
                new KeyValuePair<string, bool?>("Not Logged In", false),
                new KeyValuePair<string, bool?>("Both", null)
            };

            rblIsAuthenticated.DataBind();
        }

        #endregion Internal Methods

        #region Data Interface

        /// <summary>
        /// Gets the page cache.
        /// </summary>
        /// <returns></returns>
        private PageCache GetPageCache()
        {
            if ( _pageCache == null )
            {
                var pageId = PageParameter( PageParamKey.Page ).AsInteger();
                _pageCache = PageCache.Get( pageId );
            }

            return _pageCache;
        }
        private PageCache _pageCache = null;

        /// <summary>
        /// Gets the interaction component ids.
        /// </summary>
        /// <returns></returns>
        private List<int> GetInteractionComponentIds()
        {
            if ( _interactionComponentIds == null )
            {
                var pageCache = GetPageCache();

                if ( pageCache == null )
                {
                    return null;
                }

                var rockContext = new RockContext();
                var interactionComponentService = new InteractionComponentService( rockContext );
                var channelMediumTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE ).Id;

                _interactionComponentIds = interactionComponentService.Queryable()
                    .AsNoTracking()
                    .Where( ic => ic.InteractionChannel.ChannelTypeMediumValueId == channelMediumTypeValueId )
                    .Where( ic => ic.EntityId == pageCache.Id )
                    .Select( ic => ic.Id )
                    .ToList();
            }

            return _interactionComponentIds;
        }
        private List<int> _interactionComponentIds = null;

        /// <summary>
        /// Gets the view model query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<GridRowViewModel> GetViewModelQuery()
        {
            // Get the components that this page uses. This is a separate query so the interaction query does not time out
            var componentIds = GetInteractionComponentIds();

            if ( componentIds == null || !componentIds.Any() )
            {
                return Enumerable.Empty<GridRowViewModel>().AsQueryable();
            }

            var rockContext = new RockContext();
            var interactionService = new InteractionService( rockContext );

            return interactionService.Queryable()
                .AsNoTracking()
                .Where( i => componentIds.Contains( i.InteractionComponentId ) )
                .Select( i => new GridRowViewModel
                {
                    InteractionId = i.Id,
                    InteractionDateTime = i.InteractionDateTime,
                    PersonAliasId = i.PersonAliasId,
                    PersonNickName = ( i.PersonAlias.Person.NickName == null || i.PersonAlias.Person.NickName.Length == 0 ) ?
                        i.PersonAlias.Person.FirstName :
                        i.PersonAlias.Person.NickName,
                    PersonLastName = i.PersonAlias.Person.LastName,
                    TimeToServe = i.InteractionTimeToServe,
                    Url = i.InteractionData
                } );
        }

        #endregion Data Interface

        #region View Models

        /// <summary>
        /// Grid Row View Model
        /// </summary>
        private class GridRowViewModel
        {
            /// <summary>
            /// Gets or sets the interaction identifier.
            /// </summary>
            /// <value>
            /// The interaction identifier.
            /// </value>
            public int InteractionId { get; set; }

            /// <summary>
            /// Gets or sets the interaction date time.
            /// </summary>
            /// <value>
            /// The interaction date time.
            /// </value>
            public DateTime InteractionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the time to serve.
            /// </summary>
            /// <value>
            /// The time to serve.
            /// </value>
            public double? TimeToServe { get; set; }

            /// <summary>
            /// Gets the time to serve formatted.
            /// </summary>
            /// <value>
            /// The time to serve formatted.
            /// </value>
            public string TimeToServeFormatted
            {
                get
                {
                    return TimeToServe.HasValue ?
                        string.Format( "{0:N2}s", TimeToServe.Value ) :
                        string.Empty;
                }
            }

            /// <summary>
            /// Gets or sets the last name of the person.
            /// </summary>
            /// <value>
            /// The last name of the person.
            /// </value>
            public string PersonLastName { get; set; }

            /// <summary>
            /// Gets or sets the name of the person nick.
            /// </summary>
            /// <value>
            /// The name of the person nick.
            /// </value>
            public string PersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            /// <value>
            /// The person alias identifier.
            /// </value>
            public int? PersonAliasId { get; set; }

            /// <summary>
            /// Gets the full name of the person.
            /// </summary>
            /// <value>
            /// The full name of the person.
            /// </value>
            public string PersonFullName
            {
                get
                {
                    return string.Format( "{0} {1}", PersonNickName, PersonLastName );
                }
            }

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            /// <value>
            /// The URL.
            /// </value>
            public string Url { get; set; }
        }

        #endregion View Models
    }
}