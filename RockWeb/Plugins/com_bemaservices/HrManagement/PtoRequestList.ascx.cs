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
    [DisplayName( "PTO Request List" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Lists all the PTO Requests." )]
    [WorkflowTypeField( "PTO Request Workflow", "The Workflow used to add, modify, and delete PTO Requests.", false, true, "EBF1D986-8BBD-4888-8A7E-43AF5914751C" )]

    [ContextAware]
    public partial class PtoRequestList : RockBlock, ICustomGridColumns
    {

        #region Fields
        private Person _person = null;
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

            gfPtoRequestFilter.ApplyFilterClick += gfPtoRequestFilter_ApplyFilterClick;
            gfPtoRequestFilter.ClearFilterClick += gfPtoRequestFilter_ClearFilterClick;
            gfPtoRequestFilter.DisplayFilterValue += gfPtoRequestFilter_DisplayFilterValue;

            gPtoRequestList.DataKeyNames = new string[] { "Id" };
            gPtoRequestList.Actions.ShowAdd = true;
            gPtoRequestList.Actions.AddClick += gPtoRequestList_Add;
            gPtoRequestList.IsDeleteEnabled = GetViewRights();
            gPtoRequestList.ShowConfirmDeleteDialog = false;

            AddDynamicControls();

            gPtoRequestList.GridRebind += gPtoRequestList_GridRebind;
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfPtoRequestFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfPtoRequestFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfPtoRequestFilter.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
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

                /* old logic 
                if ( canView == false )
                {
                    var adminGuid = "628C51A8-4613-43ED-A18D-4A6FB999273E".AsGuid();
                    var hrGuid = "6F8AABA3-5BC8-468B-90DD-F0686F38E373".AsGuid();
                    var inReviewGroup = CurrentPerson.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active && gm.Group.IsActive == true &&
                             ( gm.Group.Guid == adminGuid || gm.Group.Guid == hrGuid ) ).Any();
                    if ( inReviewGroup )
                    {
                        canView = true;
                    }
                }
                */

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
        /// Handles the DisplayFilterValue event of the gfPtoRequestFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs"/> instance containing the event data.</param>
        protected void gfPtoRequestFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
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
                        var status = e.Value.ConvertToEnumOrNull<PtoRequestApprovalState>();
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
        /// Handles the ApplyFilterClick event of the gfPtoRequestFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfPtoRequestFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfPtoRequestFilter.SaveUserPreference( "Date Range", drpRequestDate.DelimitedValues );
            gfPtoRequestFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gfPtoRequestFilter.SaveUserPreference( "Pto Type", ddlPtoType.SelectedValue );
            gfPtoRequestFilter.SaveUserPreference( "Person", ppPerson.PersonId.ToString() );

            BindGrid();
        }

        protected void gPtoRequestList_Delete( object sender, RowEventArgs e )
        {
            var workflowType = WorkflowTypeCache.Get( GetAttributeValue( "PTORequestWorkflow" ).AsGuid() );
            var ptoRequest = new PtoRequestService( new RockContext() ).Get( e.RowKeyId );
            if ( workflowType != null && ptoRequest != null )
            {
                var url = string.Format( "/WorkflowEntry/{0}?PTORequest={1}&CancelRequest=Yes", workflowType.Id, ptoRequest.Guid.ToString() );
                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gPtoRequestList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPtoRequestList_Edit( object sender, RowEventArgs e )
        {
            var workflowType = WorkflowTypeCache.Get( GetAttributeValue( "PTORequestWorkflow" ).AsGuid() );
            var ptoRequest = new PtoRequestService( new RockContext() ).Get( e.RowKeyId );
            if ( workflowType != null && ptoRequest != null )
            {
                var url = string.Format( "/WorkflowEntry/{0}?PTORequest={1}", workflowType.Id, ptoRequest.Guid.ToString() );
                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Handles the Add event of the gPtoRequestList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gPtoRequestList_Add( object sender, EventArgs e )
        {
            var workflowType = WorkflowTypeCache.Get( GetAttributeValue( "PTORequestWorkflow" ).AsGuid() );
            if ( workflowType != null )
            {
                var url = string.Format( "/WorkflowEntry/{0}", workflowType.Id );
                if ( _person != null )
                {
                    url += string.Format( "?Person={0}", _person.PrimaryAlias.Guid );
                }

                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gPtoRequestList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPtoRequestList_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        #endregion

        #region Methods

        private void AddDynamicControls()
        {
            var deleteCol = gPtoRequestList.Columns.OfType<DeleteField>().FirstOrDefault();
            if ( deleteCol != null )
            {
                gPtoRequestList.Columns.Remove( deleteCol );
            }

            if ( GetViewRights() )
            {
                // Add delete column
                var deleteField = new DeleteField();
                gPtoRequestList.Columns.Add( deleteField );
                deleteField.Click += gPtoRequestList_Delete;
            }

        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlStatus.BindToEnum<PtoRequestApprovalState>();
            ddlStatus.Items.Insert( 0, Rock.Constants.All.ListItem );
            string statusFilter = gfPtoRequestFilter.GetUserPreference( "Status" );
            if ( string.IsNullOrWhiteSpace( statusFilter ) )
            {
                statusFilter = PtoRequestApprovalState.Approved.ConvertToInt().ToString();
            }

            ddlStatus.SetValue( statusFilter );

            var ptoTypes = new PtoTypeService( new RockContext() ).Queryable().AsNoTracking().Where( x => x.IsActive == true ).ToList();

            ddlPtoType.DataSource = ptoTypes;
            ddlPtoType.SetValue( gfPtoRequestFilter.GetUserPreference( "Pto Type" ) );
            ddlPtoType.DataBind();
            ddlPtoType.Items.Insert( 0, Rock.Constants.All.ListItem );

            drpRequestDate.DelimitedValues = gfPtoRequestFilter.GetUserPreference( "Date Range" );

            var personId = gfPtoRequestFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
            ppPerson.PersonId = personId;
            if ( personId.HasValue )
            {
                ppPerson.PersonName = new PersonService( new RockContext() ).Get( personId.Value ).FullName;
            }

        }

        /// <summary>
        /// Handles the DataBound event of the lBatchStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lRequestStatus_DataBound( object sender, RowEventArgs e )
        {
            Literal lRequestStatus = sender as Literal;
            RequestRow requestRow = e.Row.DataItem as RequestRow;
            lRequestStatus.Text = string.Format( "<span class='{0}'>{1}</span>", requestRow.StatusLabelClass, requestRow.StatusText );
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
            var rockContext = new RockContext();

            var ptoRequestQry = GetQuery( rockContext ).AsNoTracking();

            var requestRowQry = ptoRequestQry.Select( b => new RequestRow
            {
                Id = b.Id,
                Guid = b.Guid,
                Name = b.PtoAllocation.PersonAlias.Person.NickName + " " + b.PtoAllocation.PersonAlias.Person.LastName,
                PtoType = b.PtoAllocation.PtoType,
                RequestDate = b.RequestDate,
                Hours = b.Hours,
                Status = b.PtoRequestApprovalState,
                Reason = b.Reason
            } );

            gPtoRequestList.ObjectList = ptoRequestQry.ToList().ToDictionary( k => k.Id.ToString(), v => v as object );

            gPtoRequestList.DataSource = requestRowQry.AsNoTracking().ToList();
            gPtoRequestList.EntityTypeId = EntityTypeCache.Get<PtoRequest>().Id;
            gPtoRequestList.DataBind();
        }

        /// <summary>
        /// Gets the query.  Set the timeout to 90 seconds in case the user
        /// has not set any filters and they've imported N years worth of
        /// batch data into Rock.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private IOrderedQueryable<PtoRequest> GetQuery( RockContext rockContext )
        {
            var requestService = new PtoRequestService( rockContext );
            rockContext.Database.CommandTimeout = 90;
            var qry = requestService.Queryable();

            // only pto requests for the selected recipient (_person)
            if ( _person != null )
            {
                qry = qry.Where( a => a.PtoAllocation.PersonAlias.PersonId == _person.Id );
            }

            // filter by date
            string dateRangeValue = gfPtoRequestFilter.GetUserPreference( "Date Range" );
            if ( !string.IsNullOrWhiteSpace( dateRangeValue ) )
            {
                var drp = new DateRangePicker();
                drp.DelimitedValues = dateRangeValue;
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( b => b.RequestDate >= drp.LowerValue.Value );
                }

                if ( drp.UpperValue.HasValue )
                {
                    var endOfDay = drp.UpperValue.Value.AddDays( 1 );
                    qry = qry.Where( b => b.RequestDate < endOfDay );
                }
            }

            // filter by status
            var status = gfPtoRequestFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<PtoRequestApprovalState>();
            if ( status.HasValue )
            {
                qry = qry.Where( b => b.PtoRequestApprovalState == status );
            }

            // filter by Pto Type
            var ptoTypeId = gfPtoRequestFilter.GetUserPreference( "Pto Type" ).AsIntegerOrNull();
            if ( ptoTypeId.HasValue && ptoTypeId != -1 )
            {
                qry = qry.Where( a => a.PtoAllocation.PtoTypeId == ptoTypeId.Value );
            }

            // filter by person
            var personId = gfPtoRequestFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                qry = qry.Where( b => b.PtoAllocation.PersonAlias.PersonId == personId.Value );
            }

            IOrderedQueryable<PtoRequest> sortedQry = null;

            SortProperty sortProperty = gPtoRequestList.SortProperty;
            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "Status" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        sortedQry = qry.OrderBy( b => b.PtoRequestApprovalState );
                    }
                    else
                    {
                        sortedQry = qry.OrderByDescending( b => b.PtoRequestApprovalState );
                    }
                }
                else if ( sortProperty.Property == "PersonAlias.Person.LastName,PersonAlias.Person.LastName" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        sortedQry = qry.OrderBy( b => b.PtoAllocation.PersonAlias.Person.LastName ).ThenBy( b => b.PtoAllocation.PersonAlias.Person.FirstName );
                    }
                    else
                    {
                        sortedQry = qry.OrderByDescending( b => b.PtoAllocation.PersonAlias.Person.LastName ).ThenByDescending( b => b.PtoAllocation.PersonAlias.Person.FirstName );
                    }
                }
                else if ( sortProperty.Property == "PtoType.Name" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        sortedQry = qry.OrderBy( b => b.PtoAllocation.PtoType.Name );
                    }
                    else
                    {
                        sortedQry = qry.OrderByDescending( b => b.PtoAllocation.PtoType.Name );
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
                    .OrderByDescending( b => b.RequestDate )
                    .ThenBy( b => b.PtoAllocation.PersonAlias.Person.LastName );
            }

            return sortedQry;
        }

        #endregion

        #region Helper Class

        public class RequestRow : DotLiquid.Drop
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public string Name { get; set; }
            public decimal Hours { get; set; }
            public PtoRequestApprovalState Status { get; set; }
            public PtoType PtoType { get; set; }
            public DateTime RequestDate { get; set; }
            public int PersonId { get; set; }
            public string Reason { get; set; }

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
                        case PtoRequestApprovalState.Pending:
                            return "label label-warning";
                        case PtoRequestApprovalState.Approved:
                            return "label label-success";
                        case PtoRequestApprovalState.Denied:
                            return "label label-danger";
                        case PtoRequestApprovalState.Cancelled:
                            return "label label-default";
                    }

                    return string.Empty;
                }
            }

        }

        #endregion
    }
}
