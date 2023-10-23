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
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Request List" )]
    [Category( "Prayer" )]
    [Description( "Displays a list of prayer requests for the configured top-level group category." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve prayer requests and comments." )]

    [LinkedPage( "Detail Page", "", false, Order = 0 )]
    [IntegerField( "Expires After (days)", "Number of days until the request will expire.", false, 14, "", 1, "ExpireDays" )]
    [BooleanField( "Show Prayer Count", "If enabled, the block will show the current prayer count for each request in the list.", false, "", 2 )]
    [BooleanField( "Show 'Approved' column", "If enabled, the Approved column will be shown with a Yes/No toggle button.", true, "", 3, "ShowApprovedColumn" )]
    [BooleanField( "Show Grid Filter", "If enabled, the grid filter will be visible.", true, "", 4 )]
    [BooleanField( "Show Public Only", "If enabled, it will limit the list only to the prayer requests that are public.", false, order: 5 )]

    [ContextAware( typeof( Rock.Model.Person ) )]
    [Rock.SystemGuid.BlockTypeGuid( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74" )]
    public partial class PrayerRequestList : RockBlock, ICustomGridColumns
    {
        #region Fields

        /// <summary>
        /// The prayer request key parameter used in the QueryString for detail page.
        /// </summary>
        private static readonly string _PrayerRequestKeyParameter = "PrayerRequestId";

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Filter's User Preference Setting Keys
        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            public static readonly string PrayerCategory = "Prayer Category";
            public static readonly string PrayerCampus = "Prayer Campus";
            public static readonly string DateRange = "Date Range";
            public static readonly string ApprovalStatus = "Approval Status";
            public static readonly string UrgentStatus = "Urgent Status";
            public static readonly string ActiveStatus = "Active Status";
            public static readonly string PublicStatus = "Public/Private";
            public static readonly string Comments = "Comments";
            public static readonly string ShowExpired = "Show Expired";
        }
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();

            gfFilter.Visible = this.GetAttributeValue( "ShowGridFilter" ).AsBooleanOrNull() ?? true;

            gPrayerRequests.DataKeyNames = new string[] { "Id" };
            gPrayerRequests.Actions.AddClick += gPrayerRequests_Add;
            gPrayerRequests.GridRebind += gPrayerRequests_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            var canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPrayerRequests.Actions.ShowAdd = canAddEditDelete;
            gPrayerRequests.IsDeleteEnabled = canAddEditDelete;

            // If there is a Person as the ContextEntity, there is no need to show the Name column
            gPrayerRequests.GetColumnByHeaderText( "Name" ).Visible = this.ContextEntity<Rock.Model.Person>() == null;
            gPrayerRequests.GetColumnByHeaderText( "Prayer Count" ).Visible = GetAttributeValue( "ShowPrayerCount" ).AsBoolean();

            // Is the Approved columnn supposed to show?
            var showApprovedColumn = GetAttributeValue( "ShowApprovedColumn" ).AsBoolean();
            gPrayerRequests.GetColumnByHeaderText( "Approved?" ).Visible = showApprovedColumn;

            // But if showing, check if the person is not authorized to approve and hide the column anyhow
            if ( showApprovedColumn && !IsUserAuthorized( Authorization.APPROVE ) )
            {
                gPrayerRequests.GetColumnByHeaderText( "Approved?" ).Visible = false;
            }

            AddDynamicControls();
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
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

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            //AddDynamicControls();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = AvailableAttributes;

            return base.SaveViewState();
        }

        #endregion

        #region Grid Filter
        /// <summary>
        /// Binds any needed data to the Grid Filter also using the user's stored
        /// preferences.
        /// </summary>
        private void BindFilter()
        {
            // set the date range filter
            drpDateRange.DelimitedValues = gfFilter.GetFilterPreference( FilterSetting.DateRange );

            // Set the Approval Status filter
            ddlApprovedFilter.SetValue( gfFilter.GetFilterPreference( FilterSetting.ApprovalStatus ) );

            // Set the Urgent Status filter
            ddlUrgentFilter.SetValue( gfFilter.GetFilterPreference( FilterSetting.UrgentStatus ) );

            // Set the Public Status filter
            ddlPublicFilter.Visible = !( this.GetAttributeValue( "ShowPublicOnly" ).AsBooleanOrNull() ?? false );
            if ( !ddlPublicFilter.Visible )
            {
                gfFilter.SetFilterPreference( FilterSetting.PublicStatus, string.Empty );
            }

            // Set the Active Status filter
            ddlActiveFilter.SetValue( gfFilter.GetFilterPreference( FilterSetting.ActiveStatus ) );

            // Set the Allow Comments filter
            ddlAllowCommentsFilter.SetValue( gfFilter.GetFilterPreference( FilterSetting.Comments ) );

            // Set the category picker's selected value
            int selectedPrayerCategoryId = gfFilter.GetFilterPreference( FilterSetting.PrayerCategory ).AsInteger();
            Category prayerCategory = new CategoryService( new RockContext() ).Get( selectedPrayerCategoryId );
            catpPrayerCategoryFilter.SetValue( prayerCategory );

            int selectedPrayerCampusId = gfFilter.GetFilterPreference( FilterSetting.PrayerCampus ).AsInteger();
            cpPrayerCampusFilter.Campuses = CampusCache.All( false );
            cpPrayerCampusFilter.SetValue( new CampusService( new RockContext() ).Get( selectedPrayerCampusId ) );

            // Set the Show Expired filter
            cbShowExpired.Checked = gfFilter.GetFilterPreference( FilterSetting.ShowExpired ).AsBooleanOrNull() ?? false;

            BindAttributes();
        }

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SetFilterPreference( FilterSetting.DateRange, drpDateRange.DelimitedValues );

            // only save settings that are not the default "all" preference...
            if ( ddlApprovedFilter.SelectedValue == "all" )
            {
                gfFilter.SetFilterPreference( FilterSetting.ApprovalStatus, string.Empty );
            }
            else
            {
                gfFilter.SetFilterPreference( FilterSetting.ApprovalStatus, ddlApprovedFilter.SelectedValue );
            }

            if ( ddlUrgentFilter.SelectedValue == "all" )
            {
                gfFilter.SetFilterPreference( FilterSetting.UrgentStatus, string.Empty );
            }
            else
            {
                gfFilter.SetFilterPreference( FilterSetting.UrgentStatus, ddlUrgentFilter.SelectedValue );
            }

            if ( ddlPublicFilter.SelectedValue == "all" )
            {
                gfFilter.SetFilterPreference( FilterSetting.PublicStatus, string.Empty );
            }
            else
            {
                gfFilter.SetFilterPreference( FilterSetting.PublicStatus, ddlPublicFilter.SelectedValue );
            }

            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfFilter.SetFilterPreference( FilterSetting.ActiveStatus, string.Empty );
            }
            else
            {
                gfFilter.SetFilterPreference( FilterSetting.ActiveStatus, ddlActiveFilter.SelectedValue );
            }

            if ( ddlAllowCommentsFilter.SelectedValue == "all" )
            {
                gfFilter.SetFilterPreference( FilterSetting.Comments, string.Empty );
            }
            else
            {
                gfFilter.SetFilterPreference( FilterSetting.Comments, ddlAllowCommentsFilter.SelectedValue );
            }

            gfFilter.SetFilterPreference( FilterSetting.PrayerCategory, catpPrayerCategoryFilter.SelectedValue == Rock.Constants.None.IdValue ? string.Empty : catpPrayerCategoryFilter.SelectedValue );
            gfFilter.SetFilterPreference( FilterSetting.PrayerCampus, cpPrayerCampusFilter.SelectedValue == Rock.Constants.None.IdValue ? string.Empty : cpPrayerCampusFilter.SelectedValue );

            gfFilter.SetFilterPreference( FilterSetting.ShowExpired, cbShowExpired.Checked ? "True" : string.Empty );

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            gfFilter.SetFilterPreference( "attribute_" + attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
                        }
                        catch
                        {
                            // intentionally ignore
                        }
                    }
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles displaying the stored filter values.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e as DisplayFilterValueArgs (hint: e.Key and e.Value).</param>
        protected void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => ( "Attribute_" + a.Key ) == e.Key );
                if ( attribute != null )
                {
                    try
                    {
                        var values = JsonConvert.DeserializeObject<List<string>>( e.Value );
                        e.Value = attribute.FieldType.Field.FormatFilterValues( attribute.QualifierValues, values );
                        return;
                    }
                    catch
                    {
                        // intentionally ignore
                    }
                }
            }

            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                // don't display dead setting
                case "From Date":
                    e.Value = string.Empty;
                    break;

                // don't display dead setting
                case "To Date":
                    e.Value = string.Empty;
                    break;

                case "Prayer Category":

                    int categoryId = e.Value.AsIntegerOrNull() ?? All.Id;
                    if ( categoryId == All.Id )
                    {
                        e.Value = "All";
                    }
                    else
                    {
                        var category = CategoryCache.Get( categoryId );
                        if ( category != null )
                        {
                            e.Value = category.Name;
                        }
                    }

                    break;

                case "Prayer Campus":

                    var campus = CampusCache.Get( e.Value.AsInteger() );
                    e.Value = campus != null ? campus.Name : string.Empty;

                    break;
            }
        }

        #endregion

        #region Prayer Request Grid

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();

            int entityTypeId = new PrayerRequest().TypeId;
            foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Remove attribute columns
            foreach ( var column in gPrayerRequests.Columns.OfType<AttributeField>().ToList() )
            {
                gPrayerRequests.Columns.Remove( column );
            }

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = ( IRockControl ) control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }

                        string savedValue = gfFilter.GetFilterPreference( "attribute_" + attribute.Key );
                        if ( !string.IsNullOrWhiteSpace( savedValue ) )
                        {
                            try
                            {
                                var values = JsonConvert.DeserializeObject<List<string>>( savedValue );
                                attribute.FieldType.Field.SetFilterValues( control, attribute.QualifierValues, values );
                            }
                            catch
                            {
                                // intentionally ignore
                            }
                        }
                    }

                    bool columnExists = gPrayerRequests.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gPrayerRequests.Columns.Add( boundField );
                    }
                }
            }

            // Add delete column
            var deleteField = new DeleteField();
            gPrayerRequests.Columns.Add( deleteField );
            deleteField.Click += gPrayerRequests_Delete;
        }

        /// <summary>
        /// Binds the grid to a list of Prayer Requests.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
            SortProperty sortProperty = gPrayerRequests.SortProperty;

            var prayerRequests = prayerRequestService.Queryable().AsNoTracking();

            // Filter by prayer category if one is selected...
            int selectedPrayerCategoryID = catpPrayerCategoryFilter.SelectedValue.AsIntegerOrNull() ?? All.Id;
            if ( selectedPrayerCategoryID != All.Id && selectedPrayerCategoryID != None.Id )
            {
                prayerRequests = prayerRequests.Where( c => c.CategoryId == selectedPrayerCategoryID
                    || ( c.CategoryId.HasValue && c.Category.ParentCategoryId == selectedPrayerCategoryID ) );
            }

            // Filter by Campus if one is selected...
            int? selectedPrayerCampusID = cpPrayerCampusFilter.SelectedCampusId;
            if ( selectedPrayerCampusID.HasValue && selectedPrayerCampusID.Value > 0)
            {
                prayerRequests = prayerRequests.Where( c => c.CampusId == selectedPrayerCampusID );
            }

            // Filter by approved/unapproved
            if ( ddlApprovedFilter.SelectedIndex > -1 )
            {
                if ( ddlApprovedFilter.SelectedValue == "unapproved" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsApproved == false || !a.IsApproved.HasValue );
                }
                else if ( ddlApprovedFilter.SelectedValue == "approved" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsApproved == true );
                }
            }

            // Filter by urgent/non-urgent
            if ( ddlUrgentFilter.SelectedIndex > -1 )
            {
                if ( ddlUrgentFilter.SelectedValue == "non-urgent" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsUrgent == false || !a.IsUrgent.HasValue );
                }
                else if ( ddlUrgentFilter.SelectedValue == "urgent" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsUrgent == true );
                }
            }

            // Filter by public/non-public
            if ( !ddlPublicFilter.Visible )
            {
                prayerRequests = prayerRequests.Where( a => a.IsPublic == true );
            }
            else
            {
                if ( ddlPublicFilter.SelectedIndex > -1 )
                {
                    if ( ddlPublicFilter.SelectedValue == "non-public" )
                    {
                        prayerRequests = prayerRequests.Where( a => a.IsPublic == false || !a.IsPublic.HasValue );
                    }
                    else if ( ddlPublicFilter.SelectedValue == "public" )
                    {
                        prayerRequests = prayerRequests.Where( a => a.IsPublic == true );
                    }
                }
            }

            // Filter by active/inactive
            if ( ddlActiveFilter.SelectedIndex > -1 )
            {
                if ( ddlActiveFilter.SelectedValue == "inactive" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsActive == false || !a.IsActive.HasValue );
                }
                else if ( ddlActiveFilter.SelectedValue == "active" )
                {
                    prayerRequests = prayerRequests.Where( a => a.IsActive == true );
                }
            }

            // Filter by active/inactive
            if ( ddlAllowCommentsFilter.SelectedIndex > -1 )
            {
                if ( ddlAllowCommentsFilter.SelectedValue == "unallow" )
                {
                    prayerRequests = prayerRequests.Where( a => a.AllowComments == false || !a.AllowComments.HasValue );
                }
                else if ( ddlAllowCommentsFilter.SelectedValue == "allow" )
                {
                    prayerRequests = prayerRequests.Where( a => a.AllowComments == true );
                }
            }

            // Filter by Date Range
            if ( drpDateRange.LowerValue.HasValue )
            {
                DateTime startDate = drpDateRange.LowerValue.Value.Date;
                prayerRequests = prayerRequests.Where( a => a.EnteredDateTime >= startDate );
            }

            if ( drpDateRange.UpperValue.HasValue )
            {
                // Add one day in order to include everything up to the end of the selected datetime.
                var endDate = drpDateRange.UpperValue.Value.AddDays( 1 );
                prayerRequests = prayerRequests.Where( a => a.EnteredDateTime < endDate );
            }

            // Don't show expired prayer requests.
            if ( !cbShowExpired.Checked )
            {
                prayerRequests = prayerRequests.Where( a => a.ExpirationDate == null || RockDateTime.Today <= a.ExpirationDate );
            }

            // Filter query by any configured attribute filters
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    prayerRequests = attribute.FieldType.Field.ApplyAttributeQueryFilter( prayerRequests, filterControl, attribute, prayerRequestService, Rock.Reporting.FilterMode.SimpleFilter );
                }
            }

            var personContext = this.ContextEntity<Person>();

            if ( personContext != null )
            {
                prayerRequests = prayerRequests.Where( a => a.RequestedByPersonAlias.PersonId == personContext.Id );
            }

            if ( sortProperty != null )
            {
                gPrayerRequests.DataSource = prayerRequests.Sort( sortProperty ).ToList();
            }
            else
            {
                gPrayerRequests.DataSource = prayerRequests.OrderByDescending( p => p.EnteredDateTime ).ThenByDescending( p => p.Id ).ToList();
            }

            // Hide the campus column if the campus filter is not visible.
            gPrayerRequests.ColumnsOfType<RockBoundField>().First( c => c.DataField == "Campus.Name" ).Visible = cpPrayerCampusFilter.Visible;

            gPrayerRequests.EntityTypeId = EntityTypeCache.Get<PrayerRequest>().Id;
            gPrayerRequests.DataBind();
        }

        /// <summary>
        /// Handles the Add event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( e.RowKeyId );
        }

        private void NavigateToDetailPage(int requestId)
        {
            var queryParms = new Dictionary<string, string> {{ _PrayerRequestKeyParameter, requestId.ToString() }};

            var personContext = ContextEntity<Person>();
            if ( personContext != null )
            {
                queryParms.Add( "PersonId", personContext.Id.ToString() );
            }

            NavigateToLinkedPage( "DetailPage", queryParms );
        }

        /// <summary>
        /// Handles the CheckChanged event of the gPrayerRequests IsApproved field.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_CheckChanged( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                var rockContext = new RockContext();
                PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
                PrayerRequest prayerRequest = prayerRequestService.Get( e.RowKeyId );

                if ( prayerRequest != null )
                {
                    failure = false;

                    // if it was approved, set it to unapproved... otherwise
                    if ( prayerRequest.IsApproved ?? false )
                    {
                        prayerRequest.IsApproved = false;
                    }
                    else
                    {
                        prayerRequest.IsApproved = true;
                        prayerRequest.ApprovedByPersonAliasId = CurrentPersonAliasId;
                        prayerRequest.ApprovedOnDateTime = RockDateTime.Now;

                        // reset the flag count only to zero ONLY if it had a value previously.
                        if ( prayerRequest.FlagCount.HasValue && prayerRequest.FlagCount > 0 )
                        {
                            prayerRequest.FlagCount = 0;
                        }

                        var expireDays = Convert.ToDouble( GetAttributeValue( "ExpireDays" ) );
                        prayerRequest.ExpirationDate = RockDateTime.Now.AddDays( expireDays );
                    }

                    rockContext.SaveChanges();
                }

                BindGrid();
            }

            if ( failure )
            {
                maGridWarning.Show( "Unable to approve that prayer request", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
            PrayerRequest prayerRequest = prayerRequestService.Get( e.RowKeyId );

            if ( prayerRequest != null )
            {
                DeleteAllRelatedNotes( prayerRequest, rockContext );

                string errorMessage;
                if ( !prayerRequestService.CanDelete( prayerRequest, out errorMessage ) )
                {
                    maGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                prayerRequestService.Delete( prayerRequest );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Deletes all related notes.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        private void DeleteAllRelatedNotes( PrayerRequest prayerRequest, RockContext rockContext )
        {
            var noteTypeService = new NoteTypeService( rockContext );
            var noteType = noteTypeService.Get( Rock.SystemGuid.NoteType.PRAYER_COMMENT.AsGuid() );
            var noteService = new NoteService( rockContext );
            var prayerComments = noteService.Get( noteType.Id, prayerRequest.Id );
            foreach ( Note prayerComment in prayerComments )
            {
                noteService.Delete( prayerComment );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gPrayerRequests_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles disabling the Toggle fields if the user does not have Approval rights.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var prayerRequest = e.Row.DataItem as PrayerRequest;

                Literal lFullname = e.Row.FindControl( "lFullname" ) as Literal;
                if (lFullname != null && prayerRequest != null)
                {
                    lFullname.Text = prayerRequest.FirstName + " " + prayerRequest.LastName;
                }
            }
        }

        #endregion

        /// <summary>
        /// Handles the ClearFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfFilter.DeleteFilterPreferences();
            BindFilter();
        }
    }
}