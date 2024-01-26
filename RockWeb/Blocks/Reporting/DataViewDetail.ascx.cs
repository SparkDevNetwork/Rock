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
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Shows the details of the given data view.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Data View Detail" )]
    [Category( "Reporting" )]
    [Description( "Shows the details of the given data view." )]

    [BooleanField( "Add Administrate Security to Item Creator",
        Key = AttributeKey.AddAdministrateSecurityToItemCreator,
        Description = "If enabled, the person who creates a new item will be granted 'Administrate' security rights to the item.  This was the behavior in previous versions of Rock.  If disabled, the item creator will not be able to edit the Data View, its security settings, or possibly perform other functions without the Rock administrator settings up a role that is allowed to perform such functions.",
        DefaultBooleanValue = false,
        Order = 0 )]

    [LinkedPage(
        "Data View Detail Page",
        Key = AttributeKey.DataViewDetailPage,
        Description = "The page to display a data view.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Report Detail Page",
        Key = AttributeKey.ReportDetailPage,
        Description = "The page used to view or create a report.",
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Group Detail Page",
        Key = AttributeKey.GroupDetailPage,
        Description = "The page to display a group (when showing group syncs that use this data view) .",
        IsRequired = false,
        Order = 3 )]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 4 )]

    [Rock.SystemGuid.BlockTypeGuid( "EB279DF9-D817-4905-B6AC-D9883F0DA2E4" )]
    public partial class DataViewDetail : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string AddAdministrateSecurityToItemCreator = "AddAdministrateSecurityToItemCreator";
            public const string DatabaseTimeoutSeconds = "DatabaseTimeout";
            public const string DataViewDetailPage = "DataViewDetailPage";
            public const string ReportDetailPage = "ReportDetailPage";
            public const string GroupDetailPage = "GroupDetailPage";
        }

        #endregion Attribute Keys

        #region PageParameterKey

        private static class PageParameterKey
        {
            public const string DataViewId = "DataViewId";
            public const string ParentCategoryId = "ParentCategoryId";
            public const string CategoryId = "CategoryId";

            public const string ReportId = "ReportId";
            public const string GroupId = "GroupId";
        }

        #endregion PageParameterKey

        #region ViewStateKey

        private static class ViewStateKey
        {
            public const string EntityTypeId = "EntityTypeId";
            public const string DataViewFilter = "DataViewFilter";
        }

        #endregion ViewStateKey

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Switch does not automatically initialize again after a partial-postback.  This script 
            // looks for any switch elements that have not been initialized and re-initializes them.
            string script = @"
$(document).ready(function() {
    $('.switch > input').each( function () {
        $(this).parent().switch('init');
    });
});
";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "toggle-switch-init", script, true );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", DataView.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.DataView ) ).Id;

            //// Set postback timeout and request-timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// We'll want to do this in this block just in case this data view is set to persisted and we'll be waiting for it to persist
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
                string itemId = PageParameter( PageParameterKey.DataViewId );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( itemId.AsInteger(), PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            RockContext rockContext = new RockContext();
            int? entityTypeId = ViewState[ViewStateKey.EntityTypeId] as int?;
            var dataViewFilter = DataViewFilter.FromJson( ViewState[ViewStateKey.DataViewFilter].ToString() );

            CreateFilterControl( entityTypeId, dataViewFilter, false, rockContext );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.DataViewFilter] = ReportingHelper.GetFilterFromControls( phFilters ).ToJson();
            ViewState[ViewStateKey.EntityTypeId] = etpEntityType.SelectedEntityTypeId;
            return base.SaveViewState();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var service = new DataViewService( new RockContext() );
            var item = service.Get( int.Parse( hfDataViewId.Value ) );
            BindReadOnlyContextControls( service.ReadOnlyContextEnabled, item.DisableUseOfReadOnlyContext );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Handles the Click event of the Copy button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            // Create a new Data View using the current item as a template.
            var id = int.Parse( hfDataViewId.Value );

            var dataViewService = new DataViewService( new RockContext() );

            var newItem = dataViewService.GetNewFromTemplate( id );

            if ( newItem == null )
            {
                return;
            }

            newItem.Name += " (Copy)";

            // Reset the stored identifier for the active Data View.
            hfDataViewId.Value = "0";

            ShowEditDetails( newItem );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            cvSecurityError.IsValid = true;
            var addAdminToCreator = GetAttributeValue( AttributeKey.AddAdministrateSecurityToItemCreator ).AsBoolean();
            var dvCategory = CategoryCache.Get( cpCategory.SelectedValueAsInt( false ).Value );

            if ( !addAdminToCreator && !dvCategory.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                cvSecurityError.IsValid = false;
                cvSecurityError.ErrorMessage = string.Format( "You are not authorized to create or edit Data Views for the Category '{0}'.", dvCategory.Name );
                return;
            }

            DataView dataView = null;

            var rockContext = new RockContext();
            DataViewService service = new DataViewService( rockContext );

            int dataViewId = hfDataViewId.Value.AsInteger();
            int? origDataViewFilterId = null;

            if ( dataViewId == 0 )
            {
                dataView = new DataView();
                dataView.IsSystem = false;
            }
            else
            {
                dataView = service.Get( dataViewId );
                origDataViewFilterId = dataView.DataViewFilterId;
            }

            dataView.Name = tbName.Text;
            dataView.Description = tbDescription.Text;
            dataView.TransformEntityTypeId = ddlTransform.SelectedValueAsInt();
            dataView.EntityTypeId = etpEntityType.SelectedEntityTypeId;
            dataView.CategoryId = cpCategory.SelectedValueAsInt();
            dataView.IncludeDeceased = tglIncludeDeceased.Checked;
            dataView.DisableUseOfReadOnlyContext = cbDisableUseOfReadOnlyContext.Checked;
            dataView.IconCssClass = tbIconCssClass.Text;
            dataView.HighlightColor = cpIconColor.Text;

            // If the switch is set to "Persistence Schedule", and the Persistence Type radio button list is set to "Interval" then use the schedule interval picker,
            // otherwise set the value as null so that a "Schedule" Persistence Type selection will clear out the interval value.
            dataView.PersistedScheduleIntervalMinutes =
                swPersistDataView.Checked
                && rblPersistenceType.SelectedValue == PersistenceType.Interval.ConvertToInt().ToString() ? ipPersistedScheduleInterval.IntervalInMinutes : null;

            var newDataViewFilter = ReportingHelper.GetFilterFromControls( phFilters );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !dataView.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            var adding = dataView.Id.Equals( 0 );
            rockContext.WrapTransaction( () =>
            {
                // Determine if this DataView will have a persisted schedule.
                bool shouldHavePersistedSchedule = rblPersistenceType.SelectedValue == PersistenceType.Schedule.ConvertToInt().ToString();

                if ( swPersistDataView.Checked && shouldHavePersistedSchedule )
                {
                    var scheduleService = new ScheduleService( rockContext );
                    Schedule schedule = null;
                    if ( HasNamedSchedule() )
                    {
                        schedule = scheduleService.Get( ddlNamedSchedule.SelectedValueAsId() ?? 0 );
                    }
                    else if ( HasUniqueSchedule() )
                    {
                        // If the dataview has a persisted schedule without a name, use that id.
                        bool hasUnnamedSchedule = dataView.PersistedScheduleId.HasValue && dataView.PersistedSchedule.Name.IsNullOrWhiteSpace();
                        if ( hasUnnamedSchedule )
                        {
                            schedule = scheduleService.Get( dataView.PersistedSchedule.Id );
                        }
                    }

                    int dataviewScheduleCategoryId = new CategoryService( rockContext ).Get( Rock.SystemGuid.Category.SCHEDULE_PERSISTED_DATAVIEWS.AsGuid() ).Id;
                    if ( schedule == null )
                    {
                        schedule = new Schedule()
                        {
                            // make it an "Unnamed" dataview persisted schedule.
                            Name = string.Empty,
                            CategoryId = dataviewScheduleCategoryId
                        };
                    }

                    if ( HasUniqueSchedule() )
                    {
                        // if the schedule was a unique schedule, configured in the DataView UI, set the schedule's iCal content to the schedule builder UI's value.
                        schedule.iCalendarContent = sbUniqueSchedule.iCalendarContent;
                    }

                    var scheduleSelectionType = rblPersistenceSchedule.SelectedValue.AsIntegerOrNull();

                    if ( !schedule.HasSchedule() && scheduleSelectionType == PersistedScheduleType.Unique.ConvertToInt() )
                    {
                        // Don't save as an unnamed unique schedule if the schedule doesn't do anything.
                        schedule = null;
                    }
                    else
                    {
                        if ( schedule.Id == 0 )
                        {
                            scheduleService.Add( schedule );

                            // save to make sure we have a scheduleId
                            rockContext.SaveChanges();
                        }
                    }

                    if ( schedule != null )
                    {
                        dataView.PersistedScheduleId = schedule.Id;
                    }
                    else
                    {
                        dataView.PersistedScheduleId = null;
                    }
                }
                else
                {
                    // If the Persist Schedule switch is off, then clear out the Persisted Schedule Id.
                    dataView.PersistedScheduleId = null;
                }

                if ( adding )
                {
                    service.Add( dataView );

                    // We need to save the new data view so we can bind the data view filters.
                    rockContext.SaveChanges();
                }

                if ( origDataViewFilterId.HasValue )
                {
                    // delete old report filter so that we can add the new filter (but with original guids), then drop the old filter
                    DataViewFilterService dataViewFilterService = new DataViewFilterService( rockContext );
                    DataViewFilter origDataViewFilter = dataViewFilterService.Get( origDataViewFilterId.Value );

                    dataView.DataViewFilterId = null;

                    DeleteDataViewFilter( origDataViewFilter, dataViewFilterService, rockContext );
                }

                dataView.DataViewFilter = newDataViewFilter;
                rockContext.SaveChanges();
            } );

            if ( dataView.PersistedScheduleIntervalMinutes.HasValue )
            {
                try
                {
                    dataView.PersistResult( GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180 );
                    rockContext.SaveChanges();
                }
                catch ( Exception ex )
                {
                    this.LogException( ex );
                    var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );
                    if ( sqlTimeoutException != null )
                    {
                        nbPersistError.NotificationBoxType = NotificationBoxType.Warning;
                        nbPersistError.Text = "This data view did not persist in a timely manner. You can try again or adjust the timeout setting of this block.";
                        return;
                    }

                    nbPersistError.NotificationBoxType = NotificationBoxType.Danger;
                    nbPersistError.Text = "An error occurred when persisting the data view";
                    nbPreviewError.Details = ex.Message;
                    return;
                }
            }

            if ( adding && addAdminToCreator )
            {
                Rock.Security.Authorization.AllowPerson( dataView, Authorization.EDIT, this.CurrentPerson, rockContext );
                Rock.Security.Authorization.AllowPerson( dataView, Authorization.ADMINISTRATE, this.CurrentPerson, rockContext );
            }

            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.DataViewId] = dataView.Id.ToString();
            qryParams[PageParameterKey.ParentCategoryId] = null;
            NavigateToCurrentPageReference( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            // Check if we are editing an existing Data View.
            int dataViewId = hfDataViewId.Value.AsInteger();

            HideSecondaryBlocks( false );

            if ( dataViewId == 0 )
            {
                // If not, check if we are editing a new copy of an existing Data View.
                dataViewId = PageParameter( PageParameterKey.DataViewId ).AsInteger();
            }

            if ( dataViewId == 0 )
            {
                int? parentCategoryId = PageParameter( PageParameterKey.ParentCategoryId ).AsIntegerOrNull();
                if ( parentCategoryId.HasValue )
                {
                    // Canceling on Add, and we know the parentCategoryId, so we are probably in TreeView mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams[PageParameterKey.CategoryId] = parentCategoryId.ToString();
                    qryParams[PageParameterKey.DataViewId] = null;
                    qryParams[PageParameterKey.ParentCategoryId] = null;
                    NavigateToCurrentPageReference( qryParams );
                }
                else
                {
                    // Canceling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Canceling on Edit.  Return to Details
                DataViewService service = new DataViewService( new RockContext() );
                DataView item = service.Get( dataViewId );
                ShowReadonlyDetails( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.Get( int.Parse( hfDataViewId.Value ) );
            if ( dataView == null )
            {
                return;
            }

            string errorMessage;
            if ( !dataViewService.CanDelete( dataView, out errorMessage ) )
            {
                ShowReadonlyDetails( dataView );
                mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
            }
            else
            {
                var categoryId = dataView.CategoryId;

                // delete this DataView's DataViewFilter
                try
                {
                    DataViewFilterService dataViewFilterService = new DataViewFilterService( rockContext );
                    DeleteDataViewFilter( dataView.DataViewFilter, dataViewFilterService, rockContext );
                }
                catch
                {
                    // intentionally ignore if delete of DataViewFilter fails
                }

                dataViewService.Delete( dataView );
                rockContext.SaveChanges();

                // reload page, selecting the deleted data view's parent
                var qryParams = new Dictionary<string, string>();
                if ( categoryId != null )
                {
                    qryParams[PageParameterKey.CategoryId] = categoryId.ToString();
                }

                qryParams[PageParameterKey.DataViewId] = null;
                qryParams[PageParameterKey.ParentCategoryId] = null;
                NavigateToCurrentPageReference( qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCreateReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCreateReport_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( PageParameterKey.ReportId, "0" );
            if ( hfDataViewId.ValueAsInt() != default( int ) )
            {
                queryParams.Add( PageParameterKey.DataViewId, hfDataViewId.ValueAsInt().ToString() );
            }

            NavigateToLinkedPage( AttributeKey.ReportDetailPage, queryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbResetRunCount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbResetRunCount_Click( object sender, EventArgs e )
        {
            var dataViewId = hfDataViewId.ValueAsInt();
            if ( dataViewId == 0 )
            {
                return;
            }

            var rockContext = new RockContext();
            var dataViewService = new DataViewService( rockContext );
            var dataView = dataViewService.Get( dataViewId );

            if ( dataView == null )
            {
                return;
            }

            dataView.RunCount = 0;
            dataView.RunCountLastRefreshDateTime = RockDateTime.Now;
            rockContext.SaveChanges();
            ShowReadonlyDetails( dataView );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( DataView dataView )
        {
            var rockContext = new RockContext();
            etpEntityType.EntityTypes = new EntityTypeService( rockContext )
                .GetReportableEntities( this.CurrentPerson )
                .OrderBy( t => t.FriendlyName ).ToList();

            // Set the schedules available to only those with the schedule "Persisted DataViews" category.
            var persistedDataViewsScheduleCategoryId = CategoryCache.GetId( Rock.SystemGuid.Category.SCHEDULE_PERSISTED_DATAVIEWS.AsGuid() ).Value;

            var persistedNamedSchedules = new ScheduleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( s => s.CategoryId == persistedDataViewsScheduleCategoryId && s.Name != string.Empty )
                        .OrderBy( s => s.Name )
                        .ToList();

            ddlNamedSchedule.Items.Clear();
            foreach ( var schedule in persistedNamedSchedules )
            {
                ddlNamedSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Binds the data transformations.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public void BindDataTransformations( RockContext rockContext )
        {
            ddlTransform.Items.Clear();
            int? entityTypeId = etpEntityType.SelectedEntityTypeId;
            if ( entityTypeId.HasValue )
            {
                var filteredEntityType = EntityTypeCache.Get( entityTypeId.Value );
                foreach ( var component in DataTransformContainer.GetComponentsByTransformedEntityName( filteredEntityType.Name ).OrderBy( c => c.Title ) )
                {
                    if ( component.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                    {
                        var transformEntityType = EntityTypeCache.Get( component.TypeName );
                        ListItem li = new ListItem( component.Title, transformEntityType.Id.ToString() );
                        ddlTransform.Items.Add( li );
                    }
                }
            }

            ddlTransform.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        public void ShowDetail( int dataViewId )
        {
            ShowDetail( dataViewId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="parentCategoryId">The parent category id.</param>
        public void ShowDetail( int dataViewId, int? parentCategoryId )
        {
            pnlDetails.Visible = false;

            var rockContext = new RockContext();

            var dataViewService = new DataViewService( rockContext );
            DataView dataView = null;

            if ( !dataViewId.Equals( 0 ) )
            {
                dataView = dataViewService.Get( dataViewId );
                pdAuditDetails.SetEntity( dataView, ResolveRockUrl( "~" ) );
            }

            if ( dataView == null )
            {
                dataView = new DataView { Id = 0, IsSystem = false, CategoryId = parentCategoryId };
                dataView.Name = string.Empty;

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                string quickReturnLava = "{{ Dataview.Name | AddQuickReturn:'Data Views', 30 }}";
                var quickReturnMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );
                quickReturnMergeFields.Add( "Dataview", dataView );
                quickReturnLava.ResolveMergeFields( quickReturnMergeFields );
            }

            if ( !dataView.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfDataViewId.Value = dataView.Id.ToString();
            hlblEditDataViewId.Text = "Id: " + dataView.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;
            nbEditModeMessage.Text = string.Empty;

            string authorizationMessage = string.Empty;

            if ( !dataView.IsAuthorizedForAllDataViewComponents( Authorization.EDIT, CurrentPerson, rockContext, out authorizationMessage ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = authorizationMessage;
            }

            if ( dataView.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( DataView.FriendlyTypeName );
            }

            btnSecurity.Visible = dataView.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.Title = dataView.Name;
            btnSecurity.EntityId = dataView.Id;

            BindReadOnlyContextControls( dataViewService.ReadOnlyContextEnabled, dataView.DisableUseOfReadOnlyContext );
            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                lbResetRunCount.Visible = false;
                ShowReadonlyDetails( dataView );
            }
            else
            {
                btnEdit.Visible = true;
                string errorMessage = string.Empty;
                btnDelete.Enabled = dataViewService.CanDelete( dataView, out errorMessage );
                if ( !btnDelete.Enabled )
                {
                    btnDelete.ToolTip = errorMessage;
                    btnDelete.Attributes["onclick"] = null;
                }

                if ( dataView.Id > 0 )
                {
                    ShowReadonlyDetails( dataView );
                }
                else
                {
                    ShowEditDetails( dataView );
                }
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        public void ShowEditDetails( DataView dataView )
        {
            HideSecondaryBlocks( true );

            if ( dataView.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( DataView.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( DataView.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            if ( dataView.Id == default( int ) || string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.ReportDetailPage ) ) )
            {
                lbCreateReport.Visible = false;
            }

            SetEditMode( true );
            LoadDropDowns( dataView );

            if ( dataView.DataViewFilter == null || dataView.DataViewFilter.ExpressionType == FilterExpressionType.Filter )
            {
                dataView.DataViewFilter = new DataViewFilter();
                dataView.DataViewFilter.Guid = new Guid();
                dataView.DataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            }

            tbName.Text = dataView.Name;
            tbDescription.Text = dataView.Description;
            tbIconCssClass.Text = dataView.IconCssClass;
            cpIconColor.Text = dataView.HighlightColor;

            if ( dataView.EntityTypeId.HasValue )
            {
                etpEntityType.SelectedEntityTypeId = dataView.EntityTypeId;
                etpEntityType.Enabled = false;
            }
            else
            {
                etpEntityType.Enabled = true;
            }

            cpCategory.SetValue( dataView.CategoryId );

            ipPersistedScheduleInterval.IntervalInMinutes = dataView.PersistedScheduleIntervalMinutes;
            if ( dataView.PersistedScheduleIntervalMinutes.HasValue )
            {
                rblPersistenceType.SelectedValue = PersistenceType.Interval.ConvertToInt().ToString();

                // If the Interval type is set, hide the schedule controls.
                SetScheduleVisibility( false );

                // And show the interval controls.
                SetIntervalVisibility( true );
            }
            else if ( dataView.PersistedScheduleId.HasValue )
            {
                rblPersistenceType.SelectedValue = PersistenceType.Schedule.ConvertToInt().ToString();

                // If the Schedule type is set, hide the interval controls.
                SetIntervalVisibility( false );

                // And show the Schedule controls.
                SetScheduleVisibility( true );
                if ( dataView.PersistedSchedule.Name.IsNotNullOrWhiteSpace() )
                {
                    rblPersistenceSchedule.SelectedValue = PersistedScheduleType.NamedSchedule.ConvertToInt().ToString();
                    ddlNamedSchedule.SetValue( dataView.PersistedScheduleId );
                    sbUniqueSchedule.Visible = false;
                }
                else if ( dataView.PersistedSchedule.iCalendarContent.Length > 0 )
                {
                    rblPersistenceSchedule.SelectedValue = PersistedScheduleType.Unique.ConvertToInt().ToString();
                    sbUniqueSchedule.iCalendarContent = dataView.PersistedSchedule.iCalendarContent;
                    ddlNamedSchedule.Visible = false;
                }
            }

            SetPersistenceVisibility( dataView.PersistedScheduleIntervalMinutes > 0 || dataView.PersistedScheduleId.HasValue );

            // Hide the DataView ID and the Create report button.
            hlblEditDataViewId.Visible = false;
            lbCreateReport.Visible = false;

            lActionTitle.Text = dataView.Name;

            var rockContext = new RockContext();
            BindDataTransformations( rockContext );
            ddlTransform.SetValue( dataView.TransformEntityTypeId ?? 0 );

            BindIncludeDeceasedControl( dataView.EntityTypeId, dataView.IncludeDeceased );
            CreateFilterControl( dataView.EntityTypeId, dataView.DataViewFilter, true, rockContext );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        private void ShowReadonlyDetails( DataView dataView )
        {
            SetEditMode( false );
            hfDataViewId.SetValue( dataView.Id );
            lReadOnlyTitle.Text = dataView.Name.FormatAsHtmlTitle();
            hlblDataViewId.Text = "Id: " + dataView.Id.ToString();
            if ( dataView.Id == default( int ) || string.IsNullOrWhiteSpace( AttributeKey.ReportDetailPage ) )
            {
                lbViewCreateReport.Visible = false;
            }

            lDescription.Text = dataView.Description.ConvertMarkdownToHtml();

            DescriptionList descriptionListMain = new DescriptionList();

            if ( dataView.EntityType != null )
            {
                descriptionListMain.Add( "Applies To", dataView.EntityType.FriendlyName );
            }

            if ( dataView.Category != null )
            {
                descriptionListMain.Add( "Category", dataView.Category.Name );
            }

            if ( dataView.TransformEntityType != null )
            {
                descriptionListMain.Add( "Post-filter Transformation", dataView.TransformEntityType.FriendlyName );
            }

            if ( dataView.IncludeDeceased )
            {
                descriptionListMain.Add( "Include Deceased", dataView.IncludeDeceased.ToYesNo() );
            }

            lblMainDetails.Text = descriptionListMain.Html;

            SetupTimeToRunLabel( dataView );
            SetupNumberOfRuns( dataView );
            SetupLastRun( dataView );

            DescriptionList descriptionListFilters = new DescriptionList();

            if ( dataView.DataViewFilter != null && dataView.EntityTypeId.HasValue )
            {
                var entityTypeCache = EntityTypeCache.Get( dataView.EntityTypeId.Value );
                if ( entityTypeCache != null )
                {
                    var entityTypeType = entityTypeCache.GetEntityType();
                    if ( entityTypeType != null )
                    {
                        descriptionListFilters.Add( "Filter", dataView.DataViewFilter.ToString( entityTypeType ) );
                    }
                }
            }

            lFilters.Text = descriptionListFilters.Html;

            DescriptionList descriptionListPersisted = new DescriptionList();
            hlblPersisted.Visible = dataView.PersistedLastRefreshDateTime.HasValue
                && ( dataView.PersistedScheduleIntervalMinutes.HasValue || dataView.PersistedScheduleId.HasValue );
            if ( hlblPersisted.Visible )
            {
                hlblPersisted.Text = string.Format( "Persisted {0}", dataView.PersistedLastRefreshDateTime.ToElapsedString() );
                hlblPersisted.ToolTip = dataView.PersistedLastRefreshDateTime.Value.ToShortDateTimeString();
            }

            // If the dataview has a persisted schedule or interval, include the details in the description.
            if ( dataView.PersistedScheduleId.HasValue )
            {
                descriptionListPersisted.Add(
                    "Persisted Schedule",
                    dataView.PersistedSchedule.FriendlyScheduleText );
            }

            if ( dataView.PersistedScheduleIntervalMinutes.HasValue )
            {
                descriptionListPersisted.Add(
                    "Persisted Schedule",
                    string.Format( "Every {0}", new TimeIntervalSetting( dataView.PersistedScheduleIntervalMinutes, null ).ToString() ) );
            }

            lPersisted.Text = descriptionListPersisted.Html;

            DescriptionList descriptionListDataviews = new DescriptionList();

            var rockContext = new RockContext();
            DataViewService dataViewService = new DataViewService( rockContext );

            // Get any related DataViews (using RelatedDataViewId )
            var relatedDataViews = dataViewService.Queryable().AsNoTracking()
                .Where( d => d.DataViewFilter.ChildFilters.Any( f => f.RelatedDataViewId.HasValue && f.RelatedDataViewId == dataView.Id ) )
                .AsNoTracking().ToList();

            // get related DataViews that used the pre-v8 OtherDataViewFilter selection format
            var otherDataViewFilterComponentEntityId = EntityTypeCache.Get( typeof( Rock.Reporting.DataFilter.OtherDataViewFilter ) ).Id;
            var otherDataViewsLegacy = dataViewService.Queryable().AsNoTracking()
                 .Where( d => d.DataViewFilter.ChildFilters
                     .Any( f => f.Selection == dataView.Id.ToString()
                         && f.EntityTypeId == otherDataViewFilterComponentEntityId ) ).ToList();

            relatedDataViews.AddRange( otherDataViewsLegacy );
            relatedDataViews = relatedDataViews.DistinctBy( r => r.Id ).OrderBy( a => a.Name ).ToList();

            StringBuilder sbDataViews = new StringBuilder();
            var dataViewDetailPage = GetAttributeValue( AttributeKey.DataViewDetailPage );

            foreach ( var relatedDataView in relatedDataViews )
            {
                if ( !string.IsNullOrWhiteSpace( dataViewDetailPage ) )
                {
                    var dataViewDetailQueryParams = new Dictionary<string, string>()
                    {
                        { PageParameterKey.DataViewId, relatedDataView.Id.ToString() }
                    };

                    var dataViewDetailPageUrl = LinkedPageUrl( AttributeKey.DataViewDetailPage, dataViewDetailQueryParams );

                    sbDataViews.AppendFormat(
                        "<a href=\"{0}\">{1}</a><br>",
                        dataViewDetailPageUrl,
                        relatedDataView.Name );
                }
                else
                {
                    sbDataViews.Append( relatedDataView.Name + "<br>" );
                }
            }

            descriptionListDataviews.Add( "Data Views", sbDataViews );
            lDataViews.Text = descriptionListDataviews.Html;

            DescriptionList descriptionListReports = new DescriptionList();
            StringBuilder sbReports = new StringBuilder();

            ReportService reportService = new ReportService( rockContext );
            var reports = reportService.Queryable().AsNoTracking().Where( r => r.DataViewId == dataView.Id ).OrderBy( r => r.Name );
            var reportDetailPage = GetAttributeValue( AttributeKey.ReportDetailPage );

            foreach ( var report in reports )
            {
                if ( !string.IsNullOrWhiteSpace( reportDetailPage ) )
                {
                    var reportDetailQueryParams = new Dictionary<string, string>()
                    {
                        { PageParameterKey.ReportId, report.Id.ToString() }
                    };

                    var reportDetailPageUrl = LinkedPageUrl( AttributeKey.ReportDetailPage, reportDetailQueryParams );

                    sbReports.AppendFormat(
                        "<a href=\"{0}\">{1}</a><br>",
                        reportDetailPageUrl,
                        report.Name );
                }
                else
                {
                    sbReports.Append( report.Name + "<br>" );
                }
            }

            descriptionListReports.Add( "Reports", sbReports );
            lReports.Text = descriptionListReports.Html;

            // Group-Roles using DataView in Group Sync
            DescriptionList descriptionListGroupSync = new DescriptionList();
            StringBuilder sbGroups = new StringBuilder();

            GroupSyncService groupSyncService = new GroupSyncService( rockContext );
            var groupSyncs = groupSyncService
                .Queryable()
                .Where( a => a.SyncDataViewId == dataView.Id )
                .ToList();

            var groupDetailPage = GetAttributeValue( AttributeKey.GroupDetailPage );

            if ( groupSyncs.Count() > 0 )
            {
                foreach ( var groupSync in groupSyncs )
                {
                    string groupAndRole = string.Format(
                        "{0} - {1}",
                        groupSync.Group != null ? groupSync.Group.Name : "(Id: " + groupSync.GroupId.ToStringSafe() + ")",
                        groupSync.GroupTypeRole.Name );

                    if ( !string.IsNullOrWhiteSpace( groupDetailPage ) )
                    {
                        var groupDetailPageParameters = new Dictionary<string, string>()
                        {
                            { PageParameterKey.GroupId, groupSync.GroupId.ToString() }
                        };

                        var groupDetailPageUrl = LinkedPageUrl( AttributeKey.GroupDetailPage, groupDetailPageParameters );

                        sbGroups.AppendFormat(
                        "<a href=\"{0}\">{1}</a><br>",
                        groupDetailPageUrl,
                        groupAndRole );
                    }
                    else
                    {
                        sbGroups.Append( string.Format( "{0}<br/>", groupAndRole ) );
                    }
                }

                descriptionListGroupSync.Add( "Groups", sbGroups );
                lGroups.Text = descriptionListGroupSync.Html;
            }
        }

        /// <summary>
        /// Sets up the last run highlight label
        /// </summary>
        /// <param name="dataView">The data view.</param>
        private void SetupLastRun( DataView dataView )
        {
            if ( dataView.LastRunDateTime == null )
            {
                return;
            }

            hlLastRun.Text = string.Format( "Last Run: {0}", dataView.LastRunDateTime.ToShortDateString() );
            hlLastRun.LabelType = LabelType.Default;
        }

        /// <summary>
        /// Sets up the number of runs highlight label
        /// </summary>
        /// <param name="dataView">The data view.</param>
        private void SetupNumberOfRuns( DataView dataView )
        {
            hlRunSince.Text = "Not Run";
            hlRunSince.LabelType = LabelType.Info;

            var lastRefreshDateTime = dataView.CreatedDateTime;

            if ( dataView.RunCountLastRefreshDateTime != null )
            {
                lastRefreshDateTime = dataView.RunCountLastRefreshDateTime;
            }

            var status = "Since Creation";
            if ( lastRefreshDateTime != null )
            {
                status = string.Format( "Since {0}", lastRefreshDateTime.Value.ToShortDateString() );
            }

            if ( dataView.RunCount == null || dataView.RunCount.Value == 0 )
            {
                hlRunSince.LabelType = LabelType.Warning;
                hlRunSince.Text = string.Format( "Not Run {0}", status );
                return;
            }

            hlRunSince.Text = string.Format( "{0:0} Runs {1}", dataView.RunCount, status );
        }

        /// <summary>
        /// Sets up the time to run highlight label.
        /// </summary>
        /// <param name="dataView">The data view.</param>
        private void SetupTimeToRunLabel( DataView dataView )
        {
            hlTimeToRun.Text = string.Empty;
            hlTimeToRun.LabelType = LabelType.Default;
            bool isPersisted = false;
            if ( dataView == null )
            {
                return;
            }
            else if ( dataView.PersistedScheduleId != null || dataView.PersistedScheduleIntervalMinutes != null  )
            {
                // This is a persisted data view...
                if ( ! dataView.PersistedLastRunDurationMilliseconds.HasValue )
                {
                    return;
                }

                isPersisted = true;
            }
            else if ( ! dataView.TimeToRunDurationMilliseconds.HasValue )
            {
                // Otherwise it is not persisted data view, but there is no 'time to run duration' to show.
                return;
            }

            double labelValue;
            if ( isPersisted )
            {
                labelValue = dataView.PersistedLastRunDurationMilliseconds.Value;
            }
            else
            {
                labelValue = dataView.TimeToRunDurationMilliseconds.Value;
            }

            var labelUnit = "ms";
            var labelType = LabelType.Success;
            if ( labelValue > 1000 )
            {
                labelValue = labelValue / 1000;
                labelUnit = "s";

                if ( labelValue > 10 )
                {
                    labelType = LabelType.Warning;
                }
            }

            if ( labelValue > 60 && labelUnit == "s" )
            {
                labelValue = labelValue / 60;
                labelUnit = "m";

                if ( labelValue > 1 )
                {
                    labelType = LabelType.Danger;
                }
            }

            hlTimeToRun.LabelType = labelType;
            var isValueAWholeNumber = Math.Abs( labelValue % 1 ) < 0.01;
            if ( isValueAWholeNumber )
            {
                hlTimeToRun.Text = string.Format( "Time To Run: {0:0}{1}", labelValue, labelUnit );
            }
            else
            {
                hlTimeToRun.Text = string.Format( "Time To Run: {0:0.0}{1}", labelValue, labelUnit );
            }
        }

        /// <summary>
        /// Shows the preview.
        /// </summary>
        private void ShowPreview()
        {
            // create an temporary DataView record based on the current edited settings
            // it won't get saved to the database, and won't increment run counts, etc
            DataView dataView = new DataView();
            var rockContext = new RockContext();

            dataView.TransformEntityTypeId = ddlTransform.SelectedValueAsInt();
            if ( dataView.TransformEntityTypeId.HasValue )
            {
                dataView.TransformEntityType = new EntityTypeService( rockContext ).Get( dataView.TransformEntityTypeId.Value );
            }

            dataView.EntityTypeId = etpEntityType.SelectedEntityTypeId;
            if ( dataView.EntityTypeId.HasValue )
            {
                dataView.EntityType = new EntityTypeService( rockContext ).Get( dataView.EntityTypeId.Value );
            }
            else
            {
                return;
            }

            dataView.DataViewFilter = ReportingHelper.GetFilterFromControls( phFilters );
            dataView.IncludeDeceased = tglIncludeDeceased.Checked;
            dataView.DisableUseOfReadOnlyContext = cbDisableUseOfReadOnlyContext.Checked;

            // Just show the first 15 rows in the preview grid.
            int? fetchRowCount = 15;

            gPreview.DataSource = null;

            var dataviewEntityType = EntityTypeCache.Get( dataView.EntityTypeId.Value );

            if ( dataviewEntityType == null || dataviewEntityType.AssemblyName == null )
            {
                return;
            }

            Type dataviewEntityTypeType = dataviewEntityType.GetEntityType();
            if ( dataviewEntityTypeType == null )
            {
                return;
            }

            try
            {
                gPreview.CreatePreviewColumns( dataviewEntityTypeType );
                var dataViewGetQueryArgs = new DataViewGetQueryArgs
                {
                    SortProperty = gPreview.SortProperty,
                    DatabaseTimeoutSeconds = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180,
                    DataViewFilterOverrides = new DataViewFilterOverrides
                    {
                        ShouldUpdateStatics = false
                    }
                };

                var qry = dataView.GetQuery( dataViewGetQueryArgs );

                if ( fetchRowCount.HasValue )
                {
                    qry = qry.Take( fetchRowCount.Value );
                }

                gPreview.SetLinqDataSource( qry.AsNoTracking() );
                gPreview.DataBind();
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );

                if ( sqlTimeoutException != null )
                {
                    nbPreviewError.NotificationBoxType = NotificationBoxType.Warning;
                    nbPreviewError.Text = "This data view preview did not complete in a timely manner. You can try again or adjust the timeout setting of this block.";
                    return;
                }
                else
                {
                    if ( ex is RockDataViewFilterExpressionException )
                    {
                        RockDataViewFilterExpressionException rockDataViewFilterExpressionException = ex as RockDataViewFilterExpressionException;
                        nbPreviewError.Text = rockDataViewFilterExpressionException.GetFriendlyMessage( dataView );
                    }
                    else
                    {
                        nbPreviewError.Text = "There was a problem with one of the filters for this data view preview.";
                    }

                    nbPreviewError.NotificationBoxType = NotificationBoxType.Danger;

                    nbPreviewError.Details = ex.Message;
                    nbPreviewError.Visible = true;
                    return;
                }
            }

            if ( dataView.EntityTypeId.HasValue )
            {
                gPreview.RowItemText = EntityTypeCache.Get( dataView.EntityTypeId.Value ).FriendlyName;
            }

            if ( gPreview.DataSource != null )
            {
                gPreview.ExportFilename = dataView.Name;
            }

            modalPreview.Show();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;
        }

        protected void tbName_TextChanged( object sender, EventArgs e )
        {
            lActionTitle.Text = tbName.Text;
        }

        #endregion

        #region Activities and Actions

        /// <summary>
        /// Handles the Click event of the btnPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnPreview_Click( object sender, EventArgs e )
        {
            ShowPreview();
        }

        /// <summary>
        /// Handles the AddFilterClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddFilterClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterField filterField = new FilterField();
            filterField.ValidationGroup = this.BlockValidationGroup;
            filterField.IsFilterTypeEnhancedForLongLists = true;
            filterField.DataViewFilterGuid = Guid.NewGuid();
            filterField.DeleteClick += filterControl_DeleteClick;
            groupControl.Controls.Add( filterField );
            filterField.ID = string.Format( "ff_{0}", filterField.DataViewFilterGuid.ToString( "N" ) );
            filterField.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            filterField.Expanded = true;
        }

        /// <summary>
        /// Handles the AddGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_AddGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            FilterGroup childGroupControl = new FilterGroup();

            childGroupControl.AddFilterClick += groupControl_AddFilterClick;
            childGroupControl.AddGroupClick += groupControl_AddGroupClick;
            childGroupControl.DeleteGroupClick += groupControl_DeleteGroupClick;

            childGroupControl.DataViewFilterGuid = Guid.NewGuid();
            groupControl.Controls.Add( childGroupControl );
            childGroupControl.ID = string.Format( "fg_{0}", childGroupControl.DataViewFilterGuid.ToString( "N" ) );
            childGroupControl.FilteredEntityTypeName = groupControl.FilteredEntityTypeName;
            childGroupControl.FilterType = FilterExpressionType.GroupAll;
        }

        /// <summary>
        /// Handles the DeleteClick event of the filterControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void filterControl_DeleteClick( object sender, EventArgs e )
        {
            FilterField fieldControl = sender as FilterField;
            fieldControl.Parent.Controls.Remove( fieldControl );
        }

        /// <summary>
        /// Handles the DeleteGroupClick event of the groupControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void groupControl_DeleteGroupClick( object sender, EventArgs e )
        {
            FilterGroup groupControl = sender as FilterGroup;
            groupControl.Parent.Controls.Remove( groupControl );
        }

        /// <summary>
        /// Deletes the data view filter.
        /// </summary>
        /// <param name="dataViewFilter">The data view filter.</param>
        /// <param name="service">The service.</param>
        private void DeleteDataViewFilter( DataViewFilter dataViewFilter, DataViewFilterService service, RockContext rockContext )
        {
            if ( dataViewFilter == null )
            {
                return;
            }

            foreach ( var childFilter in dataViewFilter.ChildFilters.ToList() )
            {
                DeleteDataViewFilter( childFilter, service, rockContext );
            }

            dataViewFilter.DataViewId = null;
            dataViewFilter.RelatedDataViewId = null;

            rockContext.SaveChanges();

            service.Delete( dataViewFilter );
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="filteredEntityTypeId">The filtered entity type identifier.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( int? filteredEntityTypeId, DataViewFilter filter, bool setSelection, RockContext rockContext )
        {
            phFilters.Controls.Clear();
            if ( filter == null || !filteredEntityTypeId.HasValue )
            {
                return;
            }

            var filteredEntityType = EntityTypeCache.Get( filteredEntityTypeId.Value );
            CreateFilterControl( phFilters, filter, filteredEntityType.Name, setSelection, rockContext );

            var filtersWithErrors = phFilters.ControlsOfTypeRecursive<FilterField>().Where( a => a.HasFilterError ).ToList();
            nbFiltersError.Visible = false;
            if ( filtersWithErrors.Any() )
            {
                nbFiltersError.Visible = true;
                if ( filtersWithErrors.Count == 1 )
                {
                    var filterWithError = filtersWithErrors[0];
                    nbFiltersError.Text = "One of the data filters has an <a href='#filtererror'>error</a>.";
                }
                else
                {
                    nbFiltersError.Text = "There are data filter <a href='#filtererror'>errors</a>";
                }
            }
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="filteredEntityTypeName">Name of the filtered entity type.</param>
        /// <param name="setSelection">if set to <c>true</c> [set selection].</param>
        /// <param name="rockContext">The rock context.</param>
        private void CreateFilterControl( Control parentControl, DataViewFilter filter, string filteredEntityTypeName, bool setSelection, RockContext rockContext )
        {
            try
            {
                if ( filter.ExpressionType == FilterExpressionType.Filter )
                {
                    var filterControl = new FilterField();
                    filterControl.ValidationGroup = this.BlockValidationGroup;
                    filterControl.IsFilterTypeEnhancedForLongLists = true;
                    parentControl.Controls.Add( filterControl );
                    filterControl.DataViewFilterGuid = filter.Guid;
                    filterControl.ID = string.Format( "ff_{0}", filterControl.DataViewFilterGuid.ToString( "N" ) );
                    filterControl.FilteredEntityTypeName = filteredEntityTypeName;
                    if ( filter.EntityTypeId.HasValue )
                    {
                        var entityTypeCache = EntityTypeCache.Get( filter.EntityTypeId.Value, rockContext );
                        if ( entityTypeCache != null )
                        {
                            filterControl.FilterEntityTypeName = entityTypeCache.Name;
                        }
                    }

                    filterControl.Expanded = filter.Expanded;
                    if ( setSelection )
                    {
                        try
                        {
                            filterControl.SetSelection( filter.Selection );
                        }
                        catch ( Exception ex )
                        {
                            this.LogException( new Exception( "Exception setting selection for DataViewFilter: " + filter.Guid, ex ) );
                        }
                    }

                    filterControl.DeleteClick += filterControl_DeleteClick;
                }
                else
                {
                    var groupControl = new FilterGroup();
                    parentControl.Controls.Add( groupControl );
                    groupControl.DataViewFilterGuid = filter.Guid;
                    groupControl.ID = string.Format( "fg_{0}", groupControl.DataViewFilterGuid.ToString( "N" ) );
                    groupControl.FilteredEntityTypeName = filteredEntityTypeName;
                    groupControl.IsDeleteEnabled = parentControl is FilterGroup;
                    if ( setSelection )
                    {
                        groupControl.FilterType = filter.ExpressionType;
                    }

                    groupControl.AddFilterClick += groupControl_AddFilterClick;
                    groupControl.AddGroupClick += groupControl_AddGroupClick;
                    groupControl.DeleteGroupClick += groupControl_DeleteGroupClick;
                    foreach ( var childFilter in filter.ChildFilters )
                    {
                        CreateFilterControl( groupControl, childFilter, filteredEntityTypeName, setSelection, rockContext );
                    }
                }
            }
            catch ( Exception ex )
            {
                this.LogException( new Exception( "Exception creating FilterControl for DataViewFilter: " + filter.Guid, ex ) );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void etpEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var dataViewFilter = new DataViewFilter();
            dataViewFilter.Guid = Guid.NewGuid();
            dataViewFilter.ExpressionType = FilterExpressionType.GroupAll;
            var rockContext = new RockContext();

            BindDataTransformations( rockContext );

            var emptyFilter = new DataViewFilter();
            emptyFilter.ExpressionType = FilterExpressionType.Filter;
            emptyFilter.Guid = Guid.NewGuid();
            dataViewFilter.ChildFilters.Add( emptyFilter );

            BindIncludeDeceasedControl( etpEntityType.SelectedEntityTypeId );

            CreateFilterControl( etpEntityType.SelectedEntityTypeId, dataViewFilter, true, rockContext );
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// /// <param name="sender">The source of the event.</param>
        /// <param name="filteredEntityTypeId">The filtered entity type identifier.</param>
        /// <param name="includeDeceased">if set to <c>true</c> [editable].</param>
        private void BindIncludeDeceasedControl( int? filteredEntityTypeId, bool includeDeceased = false )
        {
            if ( !filteredEntityTypeId.HasValue )
            {
                return;
            }

            var filteredEntityType = EntityTypeCache.Get( filteredEntityTypeId.Value );
            if ( filteredEntityType == null )
            {
                return;
            }

            var isPersonDataView = filteredEntityType.Id == EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;
            tglIncludeDeceased.Visible = isPersonDataView;
            if ( isPersonDataView )
            {
                tglIncludeDeceased.Checked = includeDeceased;
            }
            else
            {
                tglIncludeDeceased.Checked = false;
            }
        }

        private void BindReadOnlyContextControls( bool readOnlyContextEnabled, bool disableUseOfReadOnlyContext )
        {
            if ( !readOnlyContextEnabled )
            {
                return;
            }

            cbDisableUseOfReadOnlyContext.Visible = true;
            cbDisableUseOfReadOnlyContext.Checked = disableUseOfReadOnlyContext;

            hlblReadOnlyContext.Visible = disableUseOfReadOnlyContext;
        }

        #endregion

        #region Persisted Schedule Settings

        /// <summary>
        /// Set and validate the persistence schedule settings.
        /// </summary>
        /// <param name="isEnabled"></param>
        /// <param name="persistedScheduleIntervalMinutes"></param>
        /// <param name="scheduleUnit">The schedule unit, or null if the unit should be determined by the interval.</param>
        private void SetPersistenceVisibility( bool isEnabled )
        {
            swPersistDataView.Checked = isEnabled;
            pnlPersistenceSchedule.Visible = isEnabled;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the swPersistDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void swPersistDataView_CheckedChanged( object sender, EventArgs e )
        {
            SetPersistenceVisibility( swPersistDataView.Checked );
        }

        private void SetIntervalVisibility( bool isEnabled )
        {
            pnlScheduleIntervalControls.Visible = isEnabled;
        }

        private void SetScheduleVisibility( bool isEnabled )
        {
            divPersistenceSchedule.Visible = isEnabled;
            divScheduleSelect.Visible = isEnabled;
            ddlNamedSchedule.Visible = isEnabled;
            sbUniqueSchedule.Visible = isEnabled;
        }

        protected void rblPersistenceType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblPersistenceType.SelectedValue == PersistenceType.Interval.ConvertToInt().ToString() )
            {
                // If the Interval type was selected, hide the schedule controls.
                SetScheduleVisibility( false );
                SetPersistenceScheduleTypeVisibility( false );

                // And show the interval controls.
                SetIntervalVisibility( true );
            }
            else if ( rblPersistenceType.SelectedValue == PersistenceType.Schedule.ConvertToInt().ToString() )
            {
                // If the Interval type was selected, hide the interval controls.
                SetIntervalVisibility( false );

                // And show the schedule controls.
                SetScheduleVisibility( true );

                SetPersistenceScheduleType();
            }
        }

        private bool HasNamedSchedule()
        {
            var isPersistedScheduleTypeSelected = rblPersistenceType.SelectedValue.ConvertToEnumOrNull<PersistenceType>() == PersistenceType.Schedule;
            if ( !isPersistedScheduleTypeSelected )
            {
                return false;
            }

            var isPersistedNamedScheduleSelected = rblPersistenceSchedule.SelectedValue.ConvertToEnumOrNull<PersistedScheduleType>() == PersistedScheduleType.NamedSchedule;
            return swPersistDataView.Checked && isPersistedScheduleTypeSelected && isPersistedNamedScheduleSelected && ddlNamedSchedule.SelectedValue.IsNotNullOrWhiteSpace();
        }

        private bool HasUniqueSchedule()
        {
            var isPersistedScheduleTypeSelected = rblPersistenceType.SelectedValue.ConvertToEnumOrNull<PersistenceType>() == PersistenceType.Schedule;
            if ( !isPersistedScheduleTypeSelected )
            {
                return false;
            }

            var isPersistedUniqueScheduleSelected = rblPersistenceSchedule.SelectedValue.ConvertToEnumOrNull<PersistedScheduleType>() == PersistedScheduleType.Unique;
            return swPersistDataView.Checked && isPersistedScheduleTypeSelected && isPersistedUniqueScheduleSelected && sbUniqueSchedule.iCalendarContent.IsNotNullOrWhiteSpace();
        }

        private void SetPersistenceScheduleTypeVisibility( bool isEnabled )
        {
            sbUniqueSchedule.Visible = isEnabled;
            ddlNamedSchedule.Visible = isEnabled;
        }

        protected void rblPersistenceSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetPersistenceScheduleType();
        }

        private void SetPersistenceScheduleType()
        {
            if ( rblPersistenceSchedule.SelectedValueAsEnumOrNull<PersistedScheduleType>() == PersistedScheduleType.Unique )
            {
                ddlNamedSchedule.Visible = false;
                sbUniqueSchedule.Visible = true;
                ddlNamedSchedule.Required = false;
            }
            else
            {
                // When the Persisted Schedule Type is 'Named' or it was not selected.
                sbUniqueSchedule.Visible = false;
                ddlNamedSchedule.Visible = true;
                ddlNamedSchedule.Required = true;
                rblPersistenceSchedule.SetValue( PersistedScheduleType.NamedSchedule.ConvertToInt() );
            }
        }

        private enum PersistenceType
        {
            Interval = 0,
            Schedule = 1
        }

        private enum PersistedScheduleType
        {
            Unique = 0,
            NamedSchedule = 1
        }

        #endregion
    }
}