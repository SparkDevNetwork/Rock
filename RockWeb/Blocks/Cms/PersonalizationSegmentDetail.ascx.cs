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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Personalization.SegmentFilters;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Personalization Segment Detail" )]
    [Category( "Cms" )]
    [Description( "Displays the details of a personalization segment." )]

    #region Block Attributes

    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "1F0A0A57-952D-4774-8760-52C6D56B9DB5" )]
    public partial class PersonalizationSegmentDetail : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DatabaseTimeoutSeconds = "DatabaseTimeout";
        }

        #endregion Attribute Keys

        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string AdditionalFilterConfigurationJson = "AdditionalFilterConfigurationJson";
        }

        #endregion ViewState Keys

        #region PageParameter Keys

        private static class PageParameterKey
        {
            public const string PersonalizationSegmentId = "PersonalizationSegmentId";
        }

        #endregion PageParameter Keys

        private Rock.Personalization.PersonalizationSegmentAdditionalFilterConfiguration AdditionalFilterConfiguration { get; set; }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dvpFilterDataView.EntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON.AsGuid() );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gSessionCountFilters.DataKeyNames = new string[] { "Guid" };
            gSessionCountFilters.Actions.ShowAdd = true;
            gSessionCountFilters.Actions.AddClick += gSessionCountFilters_AddClick;

            gPageViewFilters.DataKeyNames = new string[] { "Guid" };
            gPageViewFilters.Actions.ShowAdd = true;
            gPageViewFilters.Actions.AddClick += gPageViewFilters_AddClick;

            gInteractionFilters.DataKeyNames = new string[] { "Guid" };
            gInteractionFilters.Actions.ShowAdd = true;
            gInteractionFilters.Actions.AddClick += gInteractionFilters_AddClick;

            //// Set postback timeout and request-timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// We'll want to do this in this block because the PersonalizationData is recalcuated when the configuration is saved.
            int databaseTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.PersonalizationSegmentId ).AsInteger() );
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var additionalFilterConfigurationJson = this.ViewState[ViewStateKey.AdditionalFilterConfigurationJson] as string;

            this.AdditionalFilterConfiguration = additionalFilterConfigurationJson.FromJsonOrNull<Rock.Personalization.PersonalizationSegmentAdditionalFilterConfiguration>() ?? new Rock.Personalization.PersonalizationSegmentAdditionalFilterConfiguration();
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
        /// <param name="personalizationSegmentId">The segment identifier.</param>
        public void ShowDetail( int personalizationSegmentId )
        {
            var rockContext = new RockContext();

            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );
            PersonalizationSegment personalizationSegment = null;

            if ( personalizationSegmentId > 0 )
            {
                personalizationSegment = personalizationSegmentService.Get( personalizationSegmentId );
            }

            if ( personalizationSegment == null )
            {
                personalizationSegment = new PersonalizationSegment();
                pdAuditDetails.Visible = false;
            }
            else
            {
                pdAuditDetails.SetEntity( personalizationSegment, ResolveRockUrl( "~" ) );
                pdAuditDetails.Visible = true;
            }

            if ( personalizationSegment.Id == 0 )
            {
                lPanelTitle.Text = ActionTitle.Add( PersonalizationSegment.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lPanelTitle.Text = personalizationSegment.Name;
            }

            /* Name, etc */
            hfPersonalizationSegmentId.Value = personalizationSegment.Id.ToString();
            tbName.Text = personalizationSegment.Name;
            tbSegmentKey.Text = personalizationSegment.SegmentKey;
            tbDescription.Text = personalizationSegment.Description;
            hlInactive.Visible = !personalizationSegment.IsActive;
            cbIsActive.Checked = personalizationSegment.IsActive;
            hfExistingSegmentKeyNames.Value = personalizationSegmentService.Queryable().Where( a => a.Id != personalizationSegment.Id ).Select( a => a.SegmentKey ).ToList().ToJson();

            this.AdditionalFilterConfiguration = personalizationSegment.AdditionalFilterConfiguration ?? new Rock.Personalization.PersonalizationSegmentAdditionalFilterConfiguration();

            // Person Filters
            dvpFilterDataView.SetValue( personalizationSegment.FilterDataViewId );
            
            // Session Filters
            tglSessionCountFiltersAllAny.Checked = AdditionalFilterConfiguration.SessionFilterExpressionType == FilterExpressionType.GroupAll;
            BindSessionCountFiltersGrid();

            // Page View Filters
            tglPageViewFiltersAllAny.Checked = AdditionalFilterConfiguration.PageViewFilterExpressionType == FilterExpressionType.GroupAll;
            BindPageViewFiltersGrid();

            // Interaction Filters
            tglInteractionFiltersAllAny.Checked = AdditionalFilterConfiguration.InteractionFilterExpressionType == FilterExpressionType.GroupAll;
            BindInteractionFiltersGrid();
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
            var personalizationSegmentId = hfPersonalizationSegmentId.Value.AsInteger();

            // validate if request filter key unique
            var isKeyDuplicate = PersonalizationSegmentCache.All()
                .Where( ps => ps.Id != personalizationSegmentId && ps.SegmentKey == tbSegmentKey.Text )
                .Any();
            if ( isKeyDuplicate )
            {
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbWarningMessage.Text = $"Key '{tbSegmentKey.Text}' is already present. Please choose a different key";
                return;
            }

            var rockContext = new RockContext();

            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );
            PersonalizationSegment personalizationSegment;

            if ( personalizationSegmentId == 0 )
            {
                personalizationSegment = new PersonalizationSegment();
                personalizationSegment.Id = personalizationSegmentId;
                personalizationSegmentService.Add( personalizationSegment );
            }
            else
            {
                personalizationSegment = personalizationSegmentService.Get( personalizationSegmentId );
            }

            if ( personalizationSegment == null )
            {
                return;
            }

            personalizationSegment.Name = tbName.Text;
            personalizationSegment.IsActive = cbIsActive.Checked;
            personalizationSegment.SegmentKey = tbSegmentKey.Text;
            personalizationSegment.Description = tbDescription.Text;
            personalizationSegment.FilterDataViewId = dvpFilterDataView.SelectedValueAsId();

            if ( tglSessionCountFiltersAllAny.Checked )
            {
                AdditionalFilterConfiguration.SessionFilterExpressionType = FilterExpressionType.GroupAll;
            }
            else
            {
                AdditionalFilterConfiguration.SessionFilterExpressionType = FilterExpressionType.GroupAny;
            }

            if ( tglPageViewFiltersAllAny.Checked )
            {
                AdditionalFilterConfiguration.PageViewFilterExpressionType = FilterExpressionType.GroupAll;
            }
            else
            {
                AdditionalFilterConfiguration.PageViewFilterExpressionType = FilterExpressionType.GroupAny;
            }

            if ( tglInteractionFiltersAllAny.Checked )
            {
                AdditionalFilterConfiguration.InteractionFilterExpressionType = FilterExpressionType.GroupAll;
            }
            else
            {
                AdditionalFilterConfiguration.InteractionFilterExpressionType = FilterExpressionType.GroupAny;
            }

            personalizationSegment.AdditionalFilterConfiguration = this.AdditionalFilterConfiguration;

            rockContext.SaveChanges();

            try
            {
                var updatePersonalizationRockContext = new RockContext();
                updatePersonalizationRockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
                new PersonalizationSegmentService( updatePersonalizationRockContext ).UpdatePersonAliasPersonalizationData( PersonalizationSegmentCache.Get( personalizationSegment.Id ) );
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );
                if ( sqlTimeoutException != null )
                {
                    nbSegmentDataUpdateError.NotificationBoxType = NotificationBoxType.Warning;
                    nbSegmentDataUpdateError.Text = "This segment filter personalization data could not be calculated in a timely manner. You can try again or adjust the timeout setting of this block.";
                    return;
                }

                nbSegmentDataUpdateError.NotificationBoxType = NotificationBoxType.Danger;
                nbSegmentDataUpdateError.Text = "An error occurred when updating personalization data";
                nbSegmentDataUpdateError.Details = ex.Message;
                return;
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

        #region Session Filters Related

        /// <summary>
        /// Binds the session count filters grid.
        /// </summary>
        private void BindSessionCountFiltersGrid()
        {
            var sessionCountFilters = this.AdditionalFilterConfiguration.SessionSegmentFilters;
            gSessionCountFilters.DataSource = sessionCountFilters.OrderBy( a => a.GetDescription() );
            gSessionCountFilters.DataBind();
        }

        /// <summary>
        /// Handles the AddClick event of the gSessionCountFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gSessionCountFilters_AddClick( object sender, EventArgs e )
        {
            ShowSessionCountFilterDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gSessionCountFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gSessionCountFilters_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var sessionSegmentFilterGuid = ( Guid ) e.RowKeyValue;
            var sessionSegmentFilter = this.AdditionalFilterConfiguration.SessionSegmentFilters.FirstOrDefault( a => a.Guid == sessionSegmentFilterGuid );
            ShowSessionCountFilterDialog( sessionSegmentFilter );
        }

        /// <summary>
        /// Shows the session count filter dialog.
        /// </summary>
        /// <param name="sessionCountSegmentFilter">The session count segment filter.</param>
        private void ShowSessionCountFilterDialog( Rock.Personalization.SegmentFilters.SessionCountSegmentFilter sessionCountSegmentFilter )
        {
            if ( sessionCountSegmentFilter == null )
            {
                sessionCountSegmentFilter = new SessionCountSegmentFilter();
                sessionCountSegmentFilter.Guid = Guid.NewGuid();
                mdSessionCountFilterConfiguration.Title = "Add Session Filter";
            }
            else
            {
                mdSessionCountFilterConfiguration.Title = "Edit Session Filter";
            }

            hfSessionCountFilterGuid.Value = sessionCountSegmentFilter.Guid.ToString();

            lstSessionCountFilterWebSites.Items.Clear();
            foreach ( var site in SiteCache.All().Where( a => a.IsActive ) )
            {
                lstSessionCountFilterWebSites.Items.Add( new ListItem( site.Name, site.Guid.ToString() ) );
            }

            ComparisonHelper.PopulateComparisonControl( ddlSessionCountFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypesRequired, true );
            ddlSessionCountFilterComparisonType.SetValue( sessionCountSegmentFilter.ComparisonType.ConvertToInt() );
            nbSessionCountFilterCompareValue.Text = sessionCountSegmentFilter.ComparisonValue.ToString();
            drpSessionCountFilterSlidingDateRange.DelimitedValues = sessionCountSegmentFilter.SlidingDateRangeDelimitedValues;
            lstSessionCountFilterWebSites.SetValues( sessionCountSegmentFilter.SiteGuids );

            mdSessionCountFilterConfiguration.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSessionCountFilterConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSessionCountFilterConfiguration_SaveClick( object sender, EventArgs e )
        {
            var sessionSegmentFilterGuid = hfSessionCountFilterGuid.Value.AsGuid();
            var sessionSegmentFilter = this.AdditionalFilterConfiguration.SessionSegmentFilters.Where( a => a.Guid == sessionSegmentFilterGuid ).FirstOrDefault();
            if ( sessionSegmentFilter == null )
            {
                sessionSegmentFilter = new SessionCountSegmentFilter();
                sessionSegmentFilter.Guid = hfSessionCountFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.SessionSegmentFilters.Add( sessionSegmentFilter );
            }

            sessionSegmentFilter.ComparisonType = ddlSessionCountFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            sessionSegmentFilter.ComparisonValue = nbSessionCountFilterCompareValue.Text.AsInteger();
            sessionSegmentFilter.SiteGuids = lstSessionCountFilterWebSites.SelectedValuesAsGuid;

            sessionSegmentFilter.SlidingDateRangeDelimitedValues = drpSessionCountFilterSlidingDateRange.DelimitedValues;
            mdSessionCountFilterConfiguration.Hide();
            BindSessionCountFiltersGrid();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gSessionCountFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gSessionCountFilters_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var sessionSegmentFilterGuid = ( Guid ) e.RowKeyValue;
            var sessionSegmentFilter = this.AdditionalFilterConfiguration.SessionSegmentFilters.Where( a => a.Guid == sessionSegmentFilterGuid ).FirstOrDefault();
            if ( sessionSegmentFilter != null )
            {
                this.AdditionalFilterConfiguration.SessionSegmentFilters.Remove( sessionSegmentFilter );
            }

            BindSessionCountFiltersGrid();
        }

        /// <summary>
        /// Handles the DataBound event of the lSessionCountFilterDescription control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void lSessionCountFilterDescription_DataBound( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            SessionCountSegmentFilter sessionCountSegmentFilter = e.Row.DataItem as SessionCountSegmentFilter;
            var lSessionCountFilter = sender as Literal;
            if ( sessionCountSegmentFilter == null || lSessionCountFilter == null )
            {
                return;
            }

            lSessionCountFilter.Text = sessionCountSegmentFilter.GetDescription();
        }

        #endregion Session Filters Related

        #region Page View Filters Related

        /// <summary>
        /// Binds the page views filters grid.
        /// </summary>
        private void BindPageViewFiltersGrid()
        {
            var pageViewFilters = this.AdditionalFilterConfiguration.PageViewSegmentFilters;
            gPageViewFilters.DataSource = pageViewFilters.OrderBy( a => a.GetDescription() );
            gPageViewFilters.DataBind();
        }

        /// <summary>
        /// Handles the AddClick event of the gPageViewFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPageViewFilters_AddClick( object sender, EventArgs e )
        {
            ShowPageViewFilterDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gPageViewFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gPageViewFilters_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var pageViewFilterGuid = ( Guid ) e.RowKeyValue;
            var pageViewFilter = this.AdditionalFilterConfiguration.PageViewSegmentFilters.Where( a => a.Guid == pageViewFilterGuid ).FirstOrDefault();
            ShowPageViewFilterDialog( pageViewFilter );
        }

        /// <summary>
        /// Shows the page view filter dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The page view filter segment filter.</param>
        private void ShowPageViewFilterDialog( PageViewSegmentFilter pageViewFilterSegmentFilter )
        {
            if ( pageViewFilterSegmentFilter == null )
            {
                pageViewFilterSegmentFilter = new PageViewSegmentFilter();
                pageViewFilterSegmentFilter.Guid = Guid.NewGuid();

                pageViewFilterSegmentFilter.PageUrlComparisonType = ComparisonType.StartsWith;
                pageViewFilterSegmentFilter.PageReferrerComparisonType = ComparisonType.StartsWith;
                pageViewFilterSegmentFilter.SourceComparisonType = ComparisonType.StartsWith;
                pageViewFilterSegmentFilter.MediumComparisonType = ComparisonType.StartsWith;
                pageViewFilterSegmentFilter.CampaignComparisonType = ComparisonType.StartsWith;
                pageViewFilterSegmentFilter.ContentComparisonType = ComparisonType.StartsWith;
                pageViewFilterSegmentFilter.TermComparisonType = ComparisonType.StartsWith;

                mdPageViewFilterConfiguration.Title = "Add Page View Filter";
            }
            else
            {
                mdPageViewFilterConfiguration.Title = "Edit Page View Filter";
                cbIncludeChildPages.Checked = pageViewFilterSegmentFilter.IncludeChildPages;
            }

            hfPageViewFilterGuid.Value = pageViewFilterSegmentFilter.Guid.ToString();

            lstPageViewFilterWebSites.Items.Clear();
            foreach ( var site in SiteCache.All().Where( a => a.IsActive ) )
            {
                lstPageViewFilterWebSites.Items.Add( new ListItem( site.Name, site.Guid.ToString() ) );
            }

            ComparisonHelper.PopulateComparisonControl( ddlPageViewFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypesRequired, true );
            ddlPageViewFilterComparisonType.SetValue( pageViewFilterSegmentFilter.ComparisonType.ConvertToInt() );
            nbPageViewFilterCompareValue.Text = pageViewFilterSegmentFilter.ComparisonValue.ToString();
            drpPageViewFilterSlidingDateRange.DelimitedValues = pageViewFilterSegmentFilter.SlidingDateRangeDelimitedValues;
            lstPageViewFilterWebSites.SetValues( pageViewFilterSegmentFilter.SiteGuids );

            ppPageViewFilterPages.SetValues( pageViewFilterSegmentFilter.GetSelectedPages().Select( a => a.Id ) );

            ComparisonHelper.PopulateComparisonControl( ddlPageUrlFilterComparisonType, ComparisonHelper.StringFilterComparisonTypes, true );
            ddlPageUrlFilterComparisonType.SetValue( pageViewFilterSegmentFilter.PageUrlComparisonType.ConvertToInt() );
            rtbPageUrlCompareValue.Text = pageViewFilterSegmentFilter.PageUrlComparisonValue;

            ComparisonHelper.PopulateComparisonControl( ddlPageReferrerFilterComparisonType, ComparisonHelper.StringFilterComparisonTypes, true );
            ddlPageReferrerFilterComparisonType.SetValue( pageViewFilterSegmentFilter.PageReferrerComparisonType.ConvertToInt() );
            rtbPageReferrerCompareValue.Text = pageViewFilterSegmentFilter.PageReferrerComparisonValue;

            ComparisonHelper.PopulateComparisonControl( ddlPageSourceFilterComparisonType, ComparisonHelper.StringFilterComparisonTypes, true );
            ddlPageSourceFilterComparisonType.SetValue( pageViewFilterSegmentFilter.SourceComparisonType.ConvertToInt() );
            rtbPageSourceCompareValue.Text = pageViewFilterSegmentFilter.SourceComparisonValue;

            ComparisonHelper.PopulateComparisonControl( ddlMediumComparisonType, ComparisonHelper.StringFilterComparisonTypes, true );
            ddlMediumComparisonType.SetValue( pageViewFilterSegmentFilter.MediumComparisonType.ConvertToInt() );
            rtbMediumCompareValue.Text = pageViewFilterSegmentFilter.MediumComparisonValue;

            ComparisonHelper.PopulateComparisonControl( ddlCampaignComparisonType, ComparisonHelper.StringFilterComparisonTypes, true );
            ddlCampaignComparisonType.SetValue( pageViewFilterSegmentFilter.CampaignComparisonType.ConvertToInt() );
            rtbCampaignCompareValue.Text = pageViewFilterSegmentFilter.CampaignComparisonValue;

            ComparisonHelper.PopulateComparisonControl( ddlContentComparisonType, ComparisonHelper.StringFilterComparisonTypes, true );
            ddlContentComparisonType.SetValue( pageViewFilterSegmentFilter.ContentComparisonType.ConvertToInt() );
            rtbContentCompareValue.Text = pageViewFilterSegmentFilter.ContentComparisonValue;

            ComparisonHelper.PopulateComparisonControl( ddlTermComparisonType, ComparisonHelper.StringFilterComparisonTypes, true );
            ddlTermComparisonType.SetValue( pageViewFilterSegmentFilter.TermComparisonType.ConvertToInt() );
            rtbTermCompareValue.Text = pageViewFilterSegmentFilter.TermComparisonValue;

            if ( pageViewFilterSegmentFilter.PageUrlComparisonValue.IsNotNullOrWhiteSpace() || pageViewFilterSegmentFilter.PageReferrerComparisonValue.IsNotNullOrWhiteSpace() || pageViewFilterSegmentFilter.SourceComparisonValue.IsNotNullOrWhiteSpace() || pageViewFilterSegmentFilter.MediumComparisonValue.IsNotNullOrWhiteSpace() || pageViewFilterSegmentFilter.CampaignComparisonValue.IsNotNullOrWhiteSpace() || pageViewFilterSegmentFilter.ContentComparisonValue.IsNotNullOrWhiteSpace() || pageViewFilterSegmentFilter.TermComparisonValue.IsNotNullOrWhiteSpace() )
            {
                lbPageViewAdvancedOptions.Visible = false;
                pnlAdvancedOptions.Visible = true;
            }
            else
            {
                lbPageViewAdvancedOptions.Visible = true;
                pnlAdvancedOptions.Visible = false;
            }

            mdPageViewFilterConfiguration.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdPageViewFilterConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdPageViewFilterConfiguration_SaveClick( object sender, EventArgs e )
        {
            var pageViewFilterGuid = hfPageViewFilterGuid.Value.AsGuid();
            var pageViewFilter = this.AdditionalFilterConfiguration.PageViewSegmentFilters.FirstOrDefault( a => a.Guid == pageViewFilterGuid );

            if ( pageViewFilter == null )
            {
                pageViewFilter = new PageViewSegmentFilter();
                pageViewFilter.Guid = pageViewFilterGuid;
                this.AdditionalFilterConfiguration.PageViewSegmentFilters.Add( pageViewFilter );
            }

            pageViewFilter.ComparisonType = ddlPageViewFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            pageViewFilter.ComparisonValue = nbPageViewFilterCompareValue.Text.AsInteger();

            pageViewFilter.SiteGuids = lstPageViewFilterWebSites.SelectedValuesAsGuid;
            pageViewFilter.PageGuids = ppPageViewFilterPages.SelectedIds.Select( a => PageCache.Get( a )?.Guid ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();

            pageViewFilter.PageUrlComparisonType = ddlPageUrlFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.StartsWith;
            pageViewFilter.PageUrlComparisonValue = rtbPageUrlCompareValue.Text;

            pageViewFilter.PageReferrerComparisonType = ddlPageReferrerFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.StartsWith;
            pageViewFilter.PageReferrerComparisonValue = rtbPageReferrerCompareValue.Text;

            pageViewFilter.SlidingDateRangeDelimitedValues = drpPageViewFilterSlidingDateRange.DelimitedValues;

            pageViewFilter.SourceComparisonType = ddlPageSourceFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.StartsWith;
            pageViewFilter.SourceComparisonValue = rtbPageSourceCompareValue.Text;

            pageViewFilter.MediumComparisonType = ddlMediumComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.StartsWith;
            pageViewFilter.MediumComparisonValue = rtbMediumCompareValue.Text;

            pageViewFilter.CampaignComparisonType = ddlCampaignComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.StartsWith;
            pageViewFilter.CampaignComparisonValue = rtbCampaignCompareValue.Text;

            pageViewFilter.ContentComparisonType = ddlContentComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.StartsWith;
            pageViewFilter.ContentComparisonValue = rtbContentCompareValue.Text;

            pageViewFilter.TermComparisonType = ddlTermComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.StartsWith;
            pageViewFilter.TermComparisonValue = rtbTermCompareValue.Text;

            pageViewFilter.IncludeChildPages = cbIncludeChildPages.Checked;

            mdPageViewFilterConfiguration.Hide();
            BindPageViewFiltersGrid();
        }
        /// <summary>
        /// Handles the DeleteClick event of the gPageViewFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gPageViewFilters_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var pageViewFilterGuid = ( Guid ) e.RowKeyValue;
            var pageViewFilter = this.AdditionalFilterConfiguration.PageViewSegmentFilters.Where( a => a.Guid == pageViewFilterGuid ).FirstOrDefault();
            if ( pageViewFilter != null )
            {
                this.AdditionalFilterConfiguration.PageViewSegmentFilters.Remove( pageViewFilter );
            }

            BindPageViewFiltersGrid();
        }

        /// <summary>
        /// Handles the DataBound event of the lPageViewFilterDescription control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void lPageViewFilterDescription_DataBound( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var pageViewFilterSegmentFilter = e.Row.DataItem as PageViewSegmentFilter;
            var lPageViewFilter = sender as Literal;
            if ( pageViewFilterSegmentFilter == null || lPageViewFilter == null )
            {
                return;
            }

            lPageViewFilter.Text = pageViewFilterSegmentFilter.GetDescription();
        }

        /// <summary>
        /// Handles the Click event of the lbPageViewAdvancedOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPageViewAdvancedOptions_Click( object sender, EventArgs e )
        {
            pnlAdvancedOptions.Visible = true;
            lbPageViewAdvancedOptions.Visible = false;
        }

        #endregion Page View Filters Related

        #region Interaction Filter Related

        /// <summary>
        /// Binds the interactions views filters grid.
        /// </summary>
        private void BindInteractionFiltersGrid()
        {
            var interactionSegmentFilters = this.AdditionalFilterConfiguration.InteractionSegmentFilters;
            var interactionSegmentDataSource = interactionSegmentFilters.Select( a => new
            {
                a.Guid,
                InteractionChannelName = InteractionChannelCache.Get( a.InteractionChannelGuid )?.Name,
                InteractionComponentName = a.InteractionComponentGuid.HasValue ? InteractionComponentCache.Get( a.InteractionComponentGuid.Value )?.Name : "*",
                Operation = a.Operation.IfEmpty( "*" ),
                ComparisonText = $"{a.ComparisonType.ConvertToString()} {a.ComparisonValue}",
                DateRangeText = SlidingDateRangePicker.FormatDelimitedValues( a.SlidingDateRangeDelimitedValues )
            } );

            gInteractionFilters.DataSource = interactionSegmentDataSource.OrderBy( a => a.InteractionChannelName ).ThenBy( a => a.InteractionComponentName ).ToList();
            gInteractionFilters.DataBind();
        }

        /// <summary>
        /// Handles the AddClick event of the gInteractionFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gInteractionFilters_AddClick( object sender, EventArgs e )
        {
            ShowInteractionFilterDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gInteractionFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gInteractionFilters_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var interactionSegmentFilterGuid = ( Guid ) e.RowKeyValue;
            var interactionSegmentFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters.Where( a => a.Guid == interactionSegmentFilterGuid ).FirstOrDefault();
            ShowInteractionFilterDialog( interactionSegmentFilter );
        }

        /// <summary>
        /// Shows the interactionfilter dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowInteractionFilterDialog( Rock.Personalization.SegmentFilters.InteractionSegmentFilter interactionSegmentFilter )
        {
            if ( interactionSegmentFilter == null )
            {
                interactionSegmentFilter = new InteractionSegmentFilter();
                interactionSegmentFilter.Guid = Guid.NewGuid();
                mdInteractionFilterConfiguration.Title = "Add Interaction Filter";
            }
            else
            {
                mdInteractionFilterConfiguration.Title = "Edit Interaction Filter";
            }

            hfInteractionFilterGuid.Value = interactionSegmentFilter.Guid.ToString();

            ComparisonHelper.PopulateComparisonControl( ddlInteractionFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypesRequired, true );
            ddlInteractionFilterComparisonType.SetValue( interactionSegmentFilter.ComparisonType.ConvertToInt() );
            nbInteractionFilterCompareValue.Text = interactionSegmentFilter.ComparisonValue.ToString();

            var interactionChannelId = InteractionChannelCache.GetId( interactionSegmentFilter.InteractionChannelGuid );

            pInteractionFilterInteractionChannel.SetValue( interactionChannelId );
            pInteractionFilterInteractionComponent.InteractionChannelId = interactionChannelId;

            var interactionComponentId = interactionSegmentFilter.InteractionComponentGuid.HasValue ? InteractionComponentCache.GetId( interactionSegmentFilter.InteractionComponentGuid.Value ) : null;
            pInteractionFilterInteractionComponent.SetValue( interactionComponentId );
            tbInteractionFilterOperation.Text = interactionSegmentFilter.Operation;

            drpInteractionFilterSlidingDateRange.DelimitedValues = interactionSegmentFilter.SlidingDateRangeDelimitedValues;

            mdInteractionFilterConfiguration.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdInteractionFilterConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdInteractionFilterConfiguration_SaveClick( object sender, EventArgs e )
        {
            var interactionFilterGuid = hfInteractionFilterGuid.Value.AsGuid();
            var interactionFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters.Where( a => a.Guid == interactionFilterGuid ).FirstOrDefault();
            if ( interactionFilter == null )
            {
                interactionFilter = new InteractionSegmentFilter();
                interactionFilter.Guid = hfInteractionFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.InteractionSegmentFilters.Add( interactionFilter );
            }

            var interactionChannelId = pInteractionFilterInteractionChannel.SelectedValueAsId();
            if ( interactionChannelId == null )
            {
                return;
            }

            interactionFilter.ComparisonType = ddlInteractionFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            interactionFilter.ComparisonValue = nbInteractionFilterCompareValue.Text.AsInteger();
            interactionFilter.InteractionChannelGuid = InteractionChannelCache.Get( interactionChannelId.Value ).Guid;

            var interactionComponentId = pInteractionFilterInteractionComponent.SelectedValueAsId();
            if ( interactionComponentId.HasValue )
            {
                interactionFilter.InteractionComponentGuid = InteractionComponentCache.Get( interactionComponentId.Value )?.Guid;
            }
            else
            {
                interactionFilter.InteractionComponentGuid = null;
            }

            interactionFilter.SlidingDateRangeDelimitedValues = drpInteractionFilterSlidingDateRange.DelimitedValues;
            interactionFilter.Operation = tbInteractionFilterOperation.Text;
            mdInteractionFilterConfiguration.Hide();
            BindInteractionFiltersGrid();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gInteractionFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gInteractionFilters_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var interactionFilterGuid = ( Guid ) e.RowKeyValue;
            var interactionFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters.FirstOrDefault( a => a.Guid == interactionFilterGuid );
            if ( interactionFilter != null )
            {
                this.AdditionalFilterConfiguration.InteractionSegmentFilters.Remove( interactionFilter );
            }

            BindInteractionFiltersGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the pInteractionFilterInteractionChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pInteractionFilterInteractionChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            pInteractionFilterInteractionComponent.InteractionChannelId = pInteractionFilterInteractionChannel.SelectedValueAsId();
        }

        #endregion Interaction Filter Related
    }
}