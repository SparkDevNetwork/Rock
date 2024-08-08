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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Personalization;
using Rock.Reporting;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Request Filter Detail" )]
    [Category( "Cms" )]
    [Description( "Displays the details of a request filter." )]

    [Rock.SystemGuid.BlockTypeGuid( "0CE221F6-EECE-46F9-A703-FCD09DEBC653" )]
    public partial class RequestFilterDetails : Rock.Web.UI.RockBlock
    {
        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string AdditionalFilterConfigurationJson = "AdditionalFilterConfigurationJson";
        }

        #endregion ViewState Keys

        #region PageParameter Keys

        private static class PageParameterKey
        {
            public const string RequestFilterId = "RequestFilterId";
        }

        #endregion PageParameter Keys

        private Rock.Personalization.PersonalizationRequestFilterConfiguration AdditionalFilterConfiguration { get; set; }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gIPAddress.DataKeyNames = new string[] { "Guid" };
            gIPAddress.Actions.ShowAdd = true;
            gIPAddress.Actions.AddClick += gIpAddress_AddClick;

            gBrowser.DataKeyNames = new string[] { "Guid" };
            gBrowser.Actions.ShowAdd = true;
            gBrowser.Actions.AddClick += gBrowser_AddClick;

            gCookie.DataKeyNames = new string[] { "Guid" };
            gCookie.Actions.ShowAdd = true;
            gCookie.Actions.AddClick += gCookie_AddClick;

            gQueryStringFilter.DataKeyNames = new string[] { "Guid" };
            gQueryStringFilter.Actions.ShowAdd = true;
            gQueryStringFilter.Actions.AddClick += gQueryStringFilter_AddClick;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.RequestFilterId ).AsInteger() );
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var additionalFilterConfigurationJson = this.ViewState[ViewStateKey.AdditionalFilterConfigurationJson] as string;

            this.AdditionalFilterConfiguration = additionalFilterConfigurationJson.FromJsonOrNull<Rock.Personalization.PersonalizationRequestFilterConfiguration>() ?? new Rock.Personalization.PersonalizationRequestFilterConfiguration();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.</returns>
        protected override object SaveViewState()
        {
            this.ViewState[ViewStateKey.AdditionalFilterConfigurationJson] = this.AdditionalFilterConfiguration?.ToJson();
            return base.SaveViewState();
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="requestFilterId">The request Filter identifier.</param>
        public void ShowDetail( int requestFilterId )
        {
            var rockContext = new RockContext();

            var requestFilterService = new RequestFilterService( rockContext );
            RequestFilter requestFilter = null;

            if ( requestFilterId > 0 )
            {
                requestFilter = requestFilterService.Get( requestFilterId );
            }

            if ( requestFilter == null )
            {
                requestFilter = new RequestFilter();
            }

            if ( requestFilter.Id == 0 )
            {
                lPanelTitle.Text = ActionTitle.Add( RequestFilter.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lPanelTitle.Text = requestFilter.Name;
            }

            /* Name, etc */
            hfRequestFilterId.Value = requestFilter.Id.ToString();
            tbName.Text = requestFilter.Name;
            tbKey.Text = requestFilter.RequestFilterKey;
            hlInactive.Visible = !requestFilter.IsActive;
            cbIsActive.Checked = requestFilter.IsActive;
            hfExistingRequestFilterKeyNames.Value = RequestFilterCache.All()
                .Where( a => a.Id != requestFilter.Id )
                .Select( a => a.RequestFilterKey )
                .ToList()
                .ToJson();

            // insert values to the site drop-down
            ddlSiteKey.Items.Clear();
            ListItem siteNotListedOption = new ListItem();
            ddlSiteKey.Items.Add( siteNotListedOption ); // add the option of site not listed to the start of the drop-down
            ListItem[] sites = SiteCache.GetAllActiveSites()
                .Select( site => new ListItem( site.Name, site.Id.ToString() ) )
                .ToArray();
            ddlSiteKey.Items.AddRange( sites );
            ddlSiteKey.SetValue( requestFilter.SiteId );

            this.AdditionalFilterConfiguration = requestFilter.FilterConfiguration ?? new Rock.Personalization.PersonalizationRequestFilterConfiguration();

            cblDeviceTypes.BindToEnum<DeviceTypeRequestFilter.DeviceType>();
            cblDeviceTypes.SetValues( this.AdditionalFilterConfiguration.DeviceTypeRequestFilter.DeviceTypes.Select( a => a.ConvertToInt() ) );
            cblPreviousActivity.BindToEnum<PreviousActivityRequestFilter.PreviousActivityType>();
            cblPreviousActivity.SetValues( this.AdditionalFilterConfiguration.PreviousActivityRequestFilter.PreviousActivityTypes.Select( a => a.ConvertToInt() ) );
            dowDaysOfWeek.SetValues( this.AdditionalFilterConfiguration.EnvironmentRequestFilter.DaysOfWeek.Select( a => a.ConvertToInt() ) );
            tpTimeOfDayFrom.SelectedTime = this.AdditionalFilterConfiguration.EnvironmentRequestFilter.BeginningTimeOfDay;
            tpTimeOfDayTo.SelectedTime = this.AdditionalFilterConfiguration.EnvironmentRequestFilter.EndingTimeOfDay;
            BindQueryStringFilterToGrid();
            BindCookieFilterToGrid();
            BindBrowserFilterToGrid();
            BindIPAddressFilterToGrid();
        }

        #endregion Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var requestFilterId = hfRequestFilterId.Value.AsInteger();

            // validate if request filter key unique
            var isKeyDuplicate = RequestFilterCache.All()
                .Where( rf => rf.Id != requestFilterId && rf.RequestFilterKey == tbKey.Text )
                .Any();
            if ( isKeyDuplicate )
            {
                nbWarningMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                nbWarningMessage.Text = $"Key '{tbKey.Text}' is already present. Please choose a different key";
                return;
            }

            if ( tpTimeOfDayTo.SelectedTime.HasValue && tpTimeOfDayFrom.SelectedTime.HasValue && tpTimeOfDayTo.SelectedTime < tpTimeOfDayFrom.SelectedTime )
            {
                nbWarningMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                nbWarningMessage.Text = $"Time of Day Beginning must always be smaller than Ending.";
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var requestFilterService = new RequestFilterService( rockContext );
                RequestFilter requestFilter;

                if ( requestFilterId == 0 )
                {
                    requestFilter = new RequestFilter();
                    requestFilter.Id = requestFilterId;
                    requestFilterService.Add( requestFilter );
                }
                else
                {
                    requestFilter = requestFilterService.Get( requestFilterId );
                }

                if ( requestFilter == null )
                {
                    return;
                }

                requestFilter.Name = tbName.Text;
                requestFilter.SiteId = ddlSiteKey.SelectedValue?.AsIntegerOrNull();
                requestFilter.RequestFilterKey = tbKey.Text;
                requestFilter.IsActive = cbIsActive.Checked;

                AdditionalFilterConfiguration.PreviousActivityRequestFilter.PreviousActivityTypes = cblPreviousActivity.SelectedValues
                    .Select( v => v.ConvertToEnum<PreviousActivityRequestFilter.PreviousActivityType>() )
                    .ToArray();

                AdditionalFilterConfiguration.DeviceTypeRequestFilter.DeviceTypes = cblDeviceTypes.SelectedValues
                    .Select( v => v.ConvertToEnum<DeviceTypeRequestFilter.DeviceType>() )
                    .ToArray();

                AdditionalFilterConfiguration.QueryStringRequestFilterExpressionType =
                    tglQueryStringFiltersAllAny.Checked ? FilterExpressionType.GroupAll : FilterExpressionType.GroupAny;

                AdditionalFilterConfiguration.CookieRequestFilterExpressionType =
                    tglCookiesAllAny.Checked ? FilterExpressionType.GroupAll : FilterExpressionType.GroupAny;

                AdditionalFilterConfiguration.EnvironmentRequestFilter.DaysOfWeek = dowDaysOfWeek.SelectedDaysOfWeek.ToArray();
                AdditionalFilterConfiguration.EnvironmentRequestFilter.BeginningTimeOfDay = tpTimeOfDayFrom.SelectedTime;
                AdditionalFilterConfiguration.EnvironmentRequestFilter.EndingTimeOfDay = tpTimeOfDayTo.SelectedTime;

                requestFilter.FilterConfiguration = AdditionalFilterConfiguration;

                rockContext.SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Query String Filter

        private void gQueryStringFilter_AddClick( object sender, EventArgs e )
        {
            ShowQueryStringFilterDialog( null );
        }

        /// <summary>
        /// The Edit Click event of the gQueryStringFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gQueryStringFilter_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryStringFilterGuid = ( Guid ) e.RowKeyValue;
            var queryStringFilter = AdditionalFilterConfiguration.QueryStringRequestFilters
                .Where( a => a.Guid == queryStringFilterGuid )
                .FirstOrDefault();

            ShowQueryStringFilterDialog( queryStringFilter );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gQueryStringFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gQueryStringFilter_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryStringFilterGuid = ( Guid ) e.RowKeyValue;
            var queryStringFilter = this.AdditionalFilterConfiguration.QueryStringRequestFilters
                .FirstOrDefault( a => a.Guid == queryStringFilterGuid );
            if ( queryStringFilter != null )
            {
                this.AdditionalFilterConfiguration.QueryStringRequestFilters.Remove( queryStringFilter );
            }

            BindQueryStringFilterToGrid();
        }

        /// <summary>
        /// Shows the query filter string dialog.
        /// </summary>
        /// <param name="queryStringRequestFilter">The query string filter.</param>
        private void ShowQueryStringFilterDialog( QueryStringRequestFilter queryStringRequestFilter )
        {
            if ( queryStringRequestFilter == null )
            {
                queryStringRequestFilter = new QueryStringRequestFilter();
                queryStringRequestFilter.Guid = Guid.NewGuid();
                mdQueryStringFilter.Title = "Add Query String Filter";
            }
            else
            {
                mdQueryStringFilter.Title = "Edit Query String Filter";
            }

            hfQueryStringFilterGuid.Value = queryStringRequestFilter.Guid.ToString();

            ComparisonHelper.PopulateComparisonControl( ddlQueryStringFilterComparisonType, ComparisonHelper.StringFilterComparisonTypesRequired, true );

            // populate the modal
            tbQueryStringFilterParameter.Text = queryStringRequestFilter.Key;
            ddlQueryStringFilterComparisonType.SetValue( queryStringRequestFilter.ComparisonType.ConvertToInt() );
            tbQueryStringFilterComparisonValue.Text = queryStringRequestFilter.ComparisonValue;

            mdQueryStringFilter.Show();
        }

        protected void mdQueryStringFilter_SaveClick( object sender, EventArgs e )
        {
            var queryStringFilterGuid = hfQueryStringFilterGuid.Value.AsGuid();

            var queryStringFilter = this.AdditionalFilterConfiguration.QueryStringRequestFilters
                .Where( a => a.Guid == queryStringFilterGuid )
                .FirstOrDefault();

            if ( queryStringFilter == null )
            {
                queryStringFilter = new QueryStringRequestFilter();
                queryStringFilter.Guid = hfQueryStringFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.QueryStringRequestFilters.Add( queryStringFilter );
            }

            queryStringFilter.Key = tbQueryStringFilterParameter.Text;
            queryStringFilter.ComparisonType = ddlQueryStringFilterComparisonType.SelectedValue.ConvertToEnum<ComparisonType>();
            queryStringFilter.ComparisonValue = tbQueryStringFilterComparisonValue.Text;

            mdQueryStringFilter.Hide();
            BindQueryStringFilterToGrid();
        }

        private void BindQueryStringFilterToGrid()
        {
            var queryStringFilter = this.AdditionalFilterConfiguration.QueryStringRequestFilters;
            gQueryStringFilter.DataSource = queryStringFilter
                .OrderBy( q => q.Key )
                .ToList();
            gQueryStringFilter.DataBind();
        }

        #endregion Query String Filter

        #region Cookie Filter

        private void gCookie_AddClick( object sender, EventArgs e )
        {
            ShowCookieDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gCookie control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gCookie_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var cookieGuid = ( Guid ) e.RowKeyValue;
            var cookie = this.AdditionalFilterConfiguration.CookieRequestFilters
                .Where( a => a.Guid == cookieGuid )
                .FirstOrDefault();

            ShowCookieDialog( cookie );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gCookie control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gCookie_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var cookieFilterGuid = ( Guid ) e.RowKeyValue;
            var cookieFilter = this.AdditionalFilterConfiguration.CookieRequestFilters
                .FirstOrDefault( a => a.Guid == cookieFilterGuid );
            if ( cookieFilter != null )
            {
                this.AdditionalFilterConfiguration.CookieRequestFilters.Remove( cookieFilter );
            }

            BindCookieFilterToGrid();
        }

        /// <summary>
        /// Shows the Cookie Filter dialog.
        /// </summary>
        /// <param name="cookieRequestFilter">The cookie filter.</param>
        private void ShowCookieDialog( Rock.Personalization.CookieRequestFilter cookieRequestFilter )
        {
            if ( cookieRequestFilter == null )
            {
                cookieRequestFilter = new CookieRequestFilter();
                cookieRequestFilter.Guid = Guid.NewGuid();
                mdCookie.Title = "Add Cookie Filter";
            }
            else
            {
                mdCookie.Title = "Edit Cookie Filter";
            }


            ComparisonHelper.PopulateComparisonControl( ddlCookieFilterComparisonType, ComparisonHelper.StringFilterComparisonTypesRequired, true );

            // populate the modal
            hfCookieFilterGuid.Value = cookieRequestFilter.Guid.ToString();
            tbCookieParameter.Text = cookieRequestFilter.Key;
            ddlCookieFilterComparisonType.SetValue( cookieRequestFilter.ComparisonType.ConvertToInt() );
            tbCookieFilterComparisonValue.Text = cookieRequestFilter.ComparisonValue;

            mdCookie.Show();
        }

        protected void mdCookie_SaveClick( object sender, EventArgs e )
        {
            var cookieGuid = hfCookieFilterGuid.Value.AsGuid();
            var cookieFilter = this.AdditionalFilterConfiguration.CookieRequestFilters
                .Where( a => a.Guid == cookieGuid )
                .FirstOrDefault();

            if ( cookieFilter == null )
            {
                cookieFilter = new Rock.Personalization.CookieRequestFilter();
                cookieFilter.Guid = hfCookieFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.CookieRequestFilters.Add( cookieFilter );
            }

            cookieFilter.Key = tbCookieParameter.Text;
            cookieFilter.ComparisonType = ddlCookieFilterComparisonType.SelectedValue.ConvertToEnum<ComparisonType>();
            cookieFilter.ComparisonValue = tbCookieFilterComparisonValue.Text;

            mdCookie.Hide();
            BindCookieFilterToGrid();
        }

        private void BindCookieFilterToGrid()
        {
            var cookieFilter = this.AdditionalFilterConfiguration.CookieRequestFilters;
            gCookie.DataSource = cookieFilter.OrderBy( c => c.Key ).ToList();
            gCookie.DataBind();
        }

        #endregion Cookie Filter

        #region Browser Filter

        private void gBrowser_AddClick( object sender, EventArgs e )
        {
            ShowBrowserDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gBrowser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gBrowser_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var browserGuid = ( Guid ) e.RowKeyValue;
            var browser = this.AdditionalFilterConfiguration
                .BrowserRequestFilters
                .Where( a => a.Guid == browserGuid )
                .FirstOrDefault();
            ShowBrowserDialog( browser );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gBrowser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gBrowser_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var browserFilterGuid = ( Guid ) e.RowKeyValue;
            var browserFilter = this.AdditionalFilterConfiguration.BrowserRequestFilters
                .FirstOrDefault( a => a.Guid == browserFilterGuid );
            if ( browserFilter != null )
            {
                this.AdditionalFilterConfiguration.BrowserRequestFilters.Remove( browserFilter );
            }

            BindBrowserFilterToGrid();
        }

        /// <summary>
        /// Shows the Browser Filter dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowBrowserDialog( Rock.Personalization.BrowserRequestFilter browserRequestFilter )
        {
            if ( browserRequestFilter == null )
            {
                browserRequestFilter = new Rock.Personalization.BrowserRequestFilter();
                browserRequestFilter.Guid = Guid.NewGuid();
                mdBrowser.Title = "Add Browser Filter";
            }
            else
            {
                mdBrowser.Title = "Edit Browser Filter";
            }

            ComparisonHelper.PopulateComparisonControl( ddlBrowserVersionComparisonType, ComparisonHelper.NumericFilterComparisonTypesRequired, true );
            ddlBrowserFamily.BindToEnum<BrowserRequestFilter.BrowserFamilyEnum>();

            // populate the modal
            hfBrowserFilterGuid.Value = browserRequestFilter.Guid.ToString();
            ddlBrowserFamily.SetValue( browserRequestFilter.BrowserFamily.ConvertToInt() );
            ddlBrowserVersionComparisonType.SetValue( browserRequestFilter.VersionComparisonType.ConvertToInt() );
            nbBrowserVersionCompareValue.Text = browserRequestFilter.MajorVersion.ToString();

            mdBrowser.Show();
        }

        protected void mdBrowser_SaveClick( object sender, EventArgs e )
        {
            var browserGuid = hfBrowserFilterGuid.Value.AsGuid();
            var browserFilter = this.AdditionalFilterConfiguration.BrowserRequestFilters
                .Where( a => a.Guid == browserGuid )
                .FirstOrDefault();

            if ( browserFilter == null )
            {
                browserFilter = new Rock.Personalization.BrowserRequestFilter();
                browserFilter.Guid = hfBrowserFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.BrowserRequestFilters.Add( browserFilter );
            }

            browserFilter.BrowserFamily = ddlBrowserFamily.SelectedValue.ConvertToEnum<BrowserRequestFilter.BrowserFamilyEnum>();
            browserFilter.VersionComparisonType = ddlBrowserVersionComparisonType.SelectedValue.ConvertToEnum<ComparisonType>();
            browserFilter.MajorVersion = nbBrowserVersionCompareValue.Text.AsInteger();

            mdBrowser.Hide();
            BindBrowserFilterToGrid();
        }

        private void BindBrowserFilterToGrid()
        {
            var browserFilter = this.AdditionalFilterConfiguration.BrowserRequestFilters;
            gBrowser.DataSource = browserFilter;
            gBrowser.DataBind();
        }

        #endregion Browser Filter

        #region IP Address Filter

        private void gIpAddress_AddClick( object sender, EventArgs e )
        {
            ShowIPAddressDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gIpAddress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gIpAddress_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var ipAddressGuid = ( Guid ) e.RowKeyValue;
            var ipAddress = this.AdditionalFilterConfiguration.IPAddressRequestFilters
                .Where( a => a.Guid == ipAddressGuid )
                .FirstOrDefault();

            ShowIPAddressDialog( ipAddress );
        }


        /// <summary>
        /// Handles the DeleteClick event of the gIPAddress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gIpAddress_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var ipAddressFilterGuid = ( Guid ) e.RowKeyValue;
            var ipAddress = this.AdditionalFilterConfiguration.IPAddressRequestFilters
                .FirstOrDefault( a => a.Guid == ipAddressFilterGuid );
            if ( ipAddress != null )
            {
                this.AdditionalFilterConfiguration.IPAddressRequestFilters.Remove( ipAddress );
            }

            BindIPAddressFilterToGrid();
        }

        /// <summary>
        /// Shows the IP Address Filter dialog.
        /// </summary>
        /// <param name="ipAddressRequestFilter">The IP Address filter.</param>
        private void ShowIPAddressDialog( Rock.Personalization.IPAddressRequestFilter ipAddressRequestFilter )
        {
            if ( ipAddressRequestFilter == null )
            {
                ipAddressRequestFilter = new Rock.Personalization.IPAddressRequestFilter();
                ipAddressRequestFilter.Guid = Guid.NewGuid();
                mdIPAddress.Title = "Add IP Address Filter";
            }
            else
            {
                mdIPAddress.Title = "Edit IP Address Filter";
            }

            // populate the modal
            hfIPAddressFilterGuid.Value = ipAddressRequestFilter.Guid.ToString();
            tbIPAddressStartRange.Text = ipAddressRequestFilter.BeginningIPAddress;
            tbIPAddressEndRange.Text = ipAddressRequestFilter.EndingIPAddress;
            tglIPAddressRange.Checked = ipAddressRequestFilter.MatchType == IPAddressRequestFilter.RangeType.NotInRange;

            mdIPAddress.Show();
        }

        protected void mdIpAddress_SaveClick( object sender, EventArgs e )
        {
            var ipAddressGuid = hfIPAddressFilterGuid.Value.AsGuid();
            var ipAddressFilter = this.AdditionalFilterConfiguration.IPAddressRequestFilters
                .Where( a => a.Guid == ipAddressGuid )
                .FirstOrDefault();

            if ( ipAddressFilter == null )
            {
                ipAddressFilter = new Rock.Personalization.IPAddressRequestFilter();
                ipAddressFilter.Guid = hfIPAddressFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.IPAddressRequestFilters.Add( ipAddressFilter );
            }

            ipAddressFilter.BeginningIPAddress = tbIPAddressStartRange.Text;
            ipAddressFilter.EndingIPAddress = tbIPAddressEndRange.Text;
            ipAddressFilter.MatchType = tglIPAddressRange.Checked ? IPAddressRequestFilter.RangeType.NotInRange : IPAddressRequestFilter.RangeType.InRange;

            mdIPAddress.Hide();
            BindIPAddressFilterToGrid();
        }

        private void BindIPAddressFilterToGrid()
        {
            var ipAddressFilter = this.AdditionalFilterConfiguration.IPAddressRequestFilters;
            gIPAddress.DataSource = ipAddressFilter;
            gIPAddress.DataBind();
        }

        #endregion IP Address Filter
    }
}