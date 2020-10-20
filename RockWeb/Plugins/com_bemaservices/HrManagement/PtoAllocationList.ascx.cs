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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.bemaservices.HrManagement.Model;

namespace RockWeb.Plugins.com_bemaservices.HrManagement
{
    [DisplayName( "PTO Allocation List" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Lists all the PTO Allocations." )]
    [LinkedPage( "Detail Page", order: 0 )]

    [ContextAware]
    public partial class PtoAllocationList : RockBlock, IPostBackEventHandler, ICustomGridColumns
    {

        #region Fields
        private Person _person = null;

        private RockDropDownList ddlAction;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gfPtoAllocationFilter.ApplyFilterClick += gfPtoAllocationFilter_ApplyFilterClick;
            gfPtoAllocationFilter.ClearFilterClick += gfPtoAllocationFilter_ClearFilterClick;
            gfPtoAllocationFilter.DisplayFilterValue += gfPtoAllocationFilter_DisplayFilterValue;

            gPtoAllocationList.DataKeyNames = new string[] { "Id" };
            gPtoAllocationList.Actions.ShowAdd = UserCanEdit;
            gPtoAllocationList.Actions.AddClick += gPtoAllocationList_Add;
            gPtoAllocationList.GridRebind += gPtoAllocationList_GridRebind;
            gPtoAllocationList.RowDataBound += gPtoAllocationList_RowDataBound;
            gPtoAllocationList.IsDeleteEnabled = UserCanEdit;
            gPtoAllocationList.ShowConfirmDeleteDialog = false;

            AddDynamicControls();

            ddlAction = new RockDropDownList();
            ddlAction.ID = "ddlAction";
            ddlAction.CssClass = "pull-left input-width-lg";
            ddlAction.Items.Add( new ListItem( "-- Select Action --", string.Empty ) );
            ddlAction.Items.Add( new ListItem( "Activate", "ACTIVATE" ) );
            ddlAction.Items.Add( new ListItem( "Inactivate", "INACTIVATE" ) );

            string deleteScript = @"
                $('table.js-grid-allocation-list a.grid-delete-button').click(function( e ){
                    var $btn = $(this);
                    e.preventDefault();
                    Rock.dialogs.confirm('Are you sure you want to delete this allocation and all PTO Requests tied to it?', function (result) {
                        if(result) {
                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                        }
                    });
                });";

            ScriptManager.RegisterStartupScript( gPtoAllocationList, gPtoAllocationList.GetType(), "deleteAllocationScript", deleteScript, true );

            gPtoAllocationList.Actions.AddCustomActionControl( ddlAction );
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfPtoAllocationFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfPtoAllocationFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfPtoAllocationFilter.DeleteUserPreferences();
            ppPerson.PersonId = null;
            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbResult.Visible = false;

            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is Person )
                {
                    _person = contextEntity as Person;
                }
            }

            if ( !Page.IsPostBack )
            {
                bool canView = GetViewRights();

                if ( canView )
                {
                    BindFilter();
                    BindGrid();
                }
                else
                {
                    upnlContent.Visible = false;
                }

            }
            else
            {
                var temp = Page.Request.Form["__EVENTTARGET"];
            }
        }
        private bool GetViewRights()
        {
            var canView = this.UserCanEdit;

            if ( _person != null )
            {
                if ( CurrentPerson.Id == _person.Id )
                {
                    canView = true;
                }

                if ( canView == false )
                {
                    _person.LoadAttributes();
                    var supervisorAliasGuid = _person.GetAttributeValue( "Supervisor" ).AsGuidOrNull();
                    if ( supervisorAliasGuid != null )
                    {
                        var personAlias = new PersonAliasService( new RockContext() ).Get( supervisorAliasGuid.Value );
                        if ( personAlias != null )
                        {
                            if ( personAlias.PersonId == CurrentPerson.Id )
                            {
                                canView = true;
                            }
                        }
                    }
                }
            }
            else
            {
                canView = true;
            }

            return canView;
        }
        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

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
            return base.SaveViewState();
        }

        /// <summary>
        /// Registers the java script for grid actions.
        /// NOTE: This needs to be done after the BindGrid
        /// </summary>
        private void RegisterJavaScriptForGridActions()
        {
            string scriptFormat = @"
                $('#{0}').change(function( e ){{
                    var $ddl = $(this);
                    var count = $(""#{1} input[id$='_cbSelect_0']:checked"").length;
                    if (count == 0) {{
                        $('#{3}').val($ddl.val());
                        window.location = ""javascript:{2}"";
                    }}
                    else
                    {{
                        var $ddl = $(this);
                        if ($ddl.val() != '') {{
                            Rock.dialogs.confirm('Are you sure you want to ' + ($ddl.val() == 'ACTIVATE' ? 'activate' : 'inactivate') + ' the selected allocations?', function (result) {{
                                if (result) {{
                                    $('#{3}').val($ddl.val());
                                    window.location = ""javascript:{2}"";
                                }}
                                $ddl.val('');
                            }});
                        }}
                    }}
                }});";

            string script = string.Format(
                scriptFormat,
                ddlAction.ClientID, // {0}
                gPtoAllocationList.ClientID,  // {1}
                Page.ClientScript.GetPostBackEventReference( this, "StatusUpdate" ),  // {2}
                hfAction.ClientID // {3}
                );

            ScriptManager.RegisterStartupScript( ddlAction, ddlAction.GetType(), "ConfirmStatusChange", script, true );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfPtoAllocationFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs"/> instance containing the event data.</param>
        protected void gfPtoAllocationFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }

                case "Status":
                    {
                        var status = e.Value.ConvertToEnumOrNull<PtoAllocationStatus>();
                        if ( status.HasValue )
                        {
                            e.Value = status.ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Pto Type":
                    {
                        var ptoTypeId = e.Value.AsIntegerOrNull();
                        if ( ptoTypeId.HasValue )
                        {
                            var ptoType = new PtoTypeService( new RockContext() ).Get( ptoTypeId.Value );
                            e.Value = ptoType != null ? ptoType.Name : string.Empty;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Source Type":
                    {
                        var sourceType = e.Value.ConvertToEnumOrNull<PtoAllocationSourceType>();
                        if ( sourceType.HasValue )
                        {
                            e.Value = sourceType.ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case "Person":
                    {
                        var personAliasId = e.Value.AsIntegerOrNull();
                        if ( personAliasId.HasValue )
                        {
                            e.Value = ppPerson.PersonName;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfPtoAllocationFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfPtoAllocationFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfPtoAllocationFilter.SaveUserPreference( "Date Range", drpAllocationDate.DelimitedValues );
            gfPtoAllocationFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gfPtoAllocationFilter.SaveUserPreference( "Pto Type", ddlPtoType.SelectedValue );
            gfPtoAllocationFilter.SaveUserPreference( "Source Type", ddlSourceType.SelectedValue );
            gfPtoAllocationFilter.SaveUserPreference( "Person", ppPerson.PersonId.ToString() );

            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gPtoAllocationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPtoAllocationList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var allocationService = new PtoAllocationService( rockContext );
            var ptoRequestService = new PtoRequestService( rockContext );
            var allocation = allocationService.Get( e.RowKeyId );
            if ( allocation != null )
            {
                if ( UserCanEdit )
                {
                    rockContext.WrapTransaction( () =>
                    {

                        var changes = new History.HistoryChangeList();

                        var ptoRequests = allocation.PtoRequests.ToList();
                        foreach ( var ptoRequest in ptoRequests )
                        {
                            var requestChanges = new History.HistoryChangeList();

                            requestChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "PtoRequest" );
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( PtoRequest ),
                                Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                                ptoRequest.Id,
                                requestChanges );

                            ptoRequestService.Delete( ptoRequest );
                        }

                        changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "PtoAllocation" );
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( PtoAllocation ),
                            Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                            allocation.Id,
                            changes );

                        allocationService.Delete( allocation );

                        rockContext.SaveChanges();
                    } );
                }
                else
                {
                    mdGridWarning.Show( "You are not authorized to delete allocations.", ModalAlertType.Warning );
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gPtoAllocationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gPtoAllocationList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var allocationRow = e.Row.DataItem as AllocationRow;
                if ( UserCanEdit )
                {
                    var deleteField = gPtoAllocationList.Columns.OfType<DeleteField>().First();
                    if ( deleteField != null )
                    {
                        var cell = ( e.Row.Cells[gPtoAllocationList.GetColumnIndex( deleteField )] as DataControlFieldCell ).Controls[0];
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gPtoAllocationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPtoAllocationList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ptoAllocationId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Add event of the gPtoAllocationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gPtoAllocationList_Add( object sender, EventArgs e )
        {
            if ( _person != null )
            {
                NavigateToLinkedPage( "DetailPage", "PersonId", _person.Id );
            }
            else
            {
                NavigateToLinkedPage( "DetailPage" );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gPtoAllocationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPtoAllocationList_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "StatusUpdate" && hfAction.Value.IsNotNullOrWhiteSpace() )
            {
                var allocationsSelected = new List<int>();

                gPtoAllocationList.SelectedKeys.ToList().ForEach( b => allocationsSelected.Add( b.ToString().AsInteger() ) );

                if ( allocationsSelected.Any() )
                {
                    var newStatus = hfAction.Value == "ACTIVATE" ? PtoAllocationStatus.Active : PtoAllocationStatus.Inactive;
                    var rockContext = new RockContext();
                    var allocationService = new PtoAllocationService( rockContext );
                    var allocationsToUpdate = allocationService.Queryable()
                        .Where( b =>
                            allocationsSelected.Contains( b.Id ) &&
                            b.PtoAllocationStatus != newStatus )
                        .ToList();

                    foreach ( var allocation in allocationsToUpdate )
                    {
                        var changes = new History.HistoryChangeList();
                        History.EvaluateChange( changes, "Status", allocation.PtoAllocationStatus, newStatus );

                        allocation.PtoAllocationStatus = newStatus;

                        if ( !allocation.IsValid )
                        {
                            string message = string.Format( "Unable to update status for the selected allocations.<br/><br/>{0}", allocation.ValidationResults.AsDelimited( "<br/>" ) );
                            maWarningDialog.Show( message, ModalAlertType.Warning );
                            return;
                        }

                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                            allocation.Id,
                            changes,
                            false );
                    }

                    rockContext.SaveChanges();

                    nbResult.Text = string.Format(
                        "{0} allocations were {1}.",
                        allocationsToUpdate.Count().ToString( "N0" ),
                        newStatus == PtoAllocationStatus.Active ? "activated" : "inactivated" );

                    nbResult.NotificationBoxType = NotificationBoxType.Success;
                    nbResult.Visible = true;
                }
                else
                {
                    nbResult.Text = string.Format( "There were not any allocations selected." );
                    nbResult.NotificationBoxType = NotificationBoxType.Warning;
                    nbResult.Visible = true;
                }

                ddlAction.SelectedIndex = 0;
                hfAction.Value = string.Empty;
                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the delete column.
        /// </summary>
        private void AddDynamicControls()
        {
            if ( UserCanEdit )
            {
                // Add delete column
                var deleteField = new DeleteField();
                gPtoAllocationList.Columns.Add( deleteField );
                deleteField.Click += gPtoAllocationList_Delete;
            }

        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlStatus.BindToEnum<PtoAllocationStatus>();
            ddlStatus.Items.Insert( 0, Rock.Constants.All.ListItem );
            string statusFilter = gfPtoAllocationFilter.GetUserPreference( "Status" );
            if ( !string.IsNullOrWhiteSpace( statusFilter ) )
            {
                ddlStatus.SetValue( statusFilter );
            }

            var ptoTypes = new PtoTypeService( new RockContext() ).Queryable().AsNoTracking().Where( x => x.IsActive == true ).ToList();

            ddlPtoType.DataSource = ptoTypes;
            ddlPtoType.DataBind();
            ddlPtoType.Items.Insert( 0, Rock.Constants.All.ListItem );
            ddlPtoType.SetValue( gfPtoAllocationFilter.GetUserPreference( "Pto Type" ) );

            ddlSourceType.BindToEnum<PtoAllocationSourceType>();
            ddlSourceType.Items.Insert( 0, Rock.Constants.All.ListItem );
            string sourceType = gfPtoAllocationFilter.GetUserPreference( "Source Type" );
            if ( !string.IsNullOrWhiteSpace( sourceType ) )
            {
                ddlSourceType.SetValue( sourceType );
            }

            drpAllocationDate.DelimitedValues = gfPtoAllocationFilter.GetUserPreference( "Date Range" );

            var personId = gfPtoAllocationFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
            ppPerson.PersonId = personId;
            if ( personId.HasValue )
            {
                ppPerson.PersonName = new PersonService( new RockContext() ).Get( personId.Value ).FullName;
            }
            else
            {
                ppPerson.PersonName = "";
            }

        }

        /// <summary>
        /// Handles the DataBound event of the lBatchStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lAllocationStatus_DataBound( object sender, RowEventArgs e )
        {
            Literal lAllocationStatus = sender as Literal;
            AllocationRow allocationRow = e.Row.DataItem as AllocationRow;
            lAllocationStatus.Text = string.Format( "<span class='{0}'>{1}</span>", allocationRow.StatusLabelClass, allocationRow.StatusText );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( bool isExporting = false )
        {
            // If configured for a person and person is null, return
            int personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && _person == null )
            {
                return;
            }

            //try
            //{
            var rockContext = new RockContext();

            var ptoAllocationQry = GetQuery( rockContext ).AsNoTracking();

            var allocationRowQry = ptoAllocationQry.Select( b => new AllocationRow
            {
                Id = b.Id,
                Name = b.PersonAlias.Person.NickName + " " + b.PersonAlias.Person.LastName,
                PtoType = b.PtoType,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Hours = b.Hours,
                Status = b.PtoAllocationStatus,
                SourceType = b.PtoAllocationSourceType,
                AccrualSchedule = b.PtoAccrualSchedule
            } );


            gPtoAllocationList.ObjectList = ptoAllocationQry.ToList().ToDictionary( k => k.Id.ToString(), v => v as object );

            gPtoAllocationList.SetLinqDataSource( allocationRowQry.AsNoTracking() );
            gPtoAllocationList.EntityTypeId = EntityTypeCache.Get<PtoAllocation>().Id;
            gPtoAllocationList.DataBind();

            RegisterJavaScriptForGridActions();

            //}
            //catch ( Exception ex )
            //{
            //    nbWarningMessage.Text = ex.Message;
            //}
        }

        /// <summary>
        /// Gets the query.  Set the timeout to 90 seconds in case the user
        /// has not set any filters and they've imported N years worth of
        /// batch data into Rock.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IOrderedQueryable<PtoAllocation> GetQuery( RockContext rockContext )
        {
            var allocationService = new PtoAllocationService( rockContext );
            rockContext.Database.CommandTimeout = 90;
            var qry = allocationService.Queryable();

            // only allocations for the selected recipient (_person)
            if ( _person != null )
            {
                qry = qry.Where( a => a.PersonAlias.PersonId == _person.Id );
            }

            // filter by date
            string dateRangeValue = gfPtoAllocationFilter.GetUserPreference( "Date Range" );
            if ( !string.IsNullOrWhiteSpace( dateRangeValue ) )
            {
                var drp = new DateRangePicker();
                drp.DelimitedValues = dateRangeValue;
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( b => b.StartDate >= drp.LowerValue.Value );
                }

                if ( drp.UpperValue.HasValue )
                {
                    var endOfDay = drp.UpperValue.Value.AddDays( 1 );
                    qry = qry.Where( b => b.EndDate < endOfDay );
                }
            }

            // filter by status
            var status = gfPtoAllocationFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<PtoAllocationStatus>();
            if ( status.HasValue )
            {
                qry = qry.Where( b => b.PtoAllocationStatus == status );
            }

            // filter by Pto Type
            var ptoTypeId = gfPtoAllocationFilter.GetUserPreference( "Pto Type" ).AsIntegerOrNull();
            if ( ptoTypeId.HasValue && ptoTypeId != -1 )
            {
                qry = qry.Where( a => a.PtoTypeId == ptoTypeId.Value );
            }

            // filter Source Type
            var sourceType = gfPtoAllocationFilter.GetUserPreference( "Source Type" ).ConvertToEnumOrNull<PtoAllocationSourceType>();
            if ( sourceType.HasValue )
            {
                qry = qry.Where( a => a.PtoAllocationSourceType == sourceType.Value );
            }

            // filter by person
            var personId = gfPtoAllocationFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                qry = qry.Where( b => b.PersonAlias.PersonId == personId.Value );
            }

            IOrderedQueryable<PtoAllocation> sortedQry = null;

            SortProperty sortProperty = gPtoAllocationList.SortProperty;
            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "Status" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        sortedQry = qry.OrderBy( b => b.PtoAllocationStatus );
                    }
                    else
                    {
                        sortedQry = qry.OrderByDescending( b => b.PtoAllocationStatus );
                    }
                }
                else
                {
                    sortedQry = qry.Sort( sortProperty );
                }
            }
            else
            {
                sortedQry = qry
            .OrderByDescending( b => b.StartDate ).ThenByDescending( b => b.EndDate );
            }

            return sortedQry;
        }

        #endregion

        #region Helper Class

        public class AllocationRow : DotLiquid.Drop
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Hours { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public PtoAllocationStatus Status { get; set; }
            public PtoType PtoType { get; set; }
            public PtoAllocationSourceType SourceType { get; set; }
            public PtoAccrualSchedule AccrualSchedule { get; set; }
            public int PersonId { get; set; }

            public string StatusText
            {
                get
                {
                    return Status.ConvertToString();
                }
            }

            public string StatusLabelClass
            {
                get
                {
                    switch ( Status )
                    {
                        case PtoAllocationStatus.Inactive:
                            return "label label-default";
                        case PtoAllocationStatus.Active:
                            return "label label-info";
                        case PtoAllocationStatus.Pending:
                            return "label label-warning";
                        case PtoAllocationStatus.Denied:
                            return "label label-danger";
                    }

                    return string.Empty;
                }
            }

        }

        #endregion
    }
}
