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
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Checkr;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Utility;

namespace RockWeb.Blocks.Security.BackgroundCheck
{
    [DisplayName( "Checkr Request List" )]
    [Category( "Security > Background Check" )]
    [Description( "Lists all the Checkr background check requests." )]

    [LinkedPage( "Workflow Detail Page", "The page to view details about the background check workflow" )]
    [Rock.SystemGuid.BlockTypeGuid( "53A28B56-B7B4-472C-9305-1DC66693A6C6" )]
    public partial class CheckrRequestList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            fRequest.ApplyFilterClick += fRequest_ApplyFilterClick;
            fRequest.DisplayFilterValue += fRequest_DisplayFilterValue;

            gRequest.DataKeyNames = new string[] { "Id" };
            gRequest.Actions.ShowAdd = false;
            gRequest.IsDeleteEnabled = false;
            gRequest.GridRebind += gRequest_GridRebind;
            gRequest.RowDataBound += gRequest_RowDataBound;
            gRequest.RowSelected += gRequest_RowSelected;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fRequest_ApplyFilterClick( object sender, EventArgs e )
        {
            fRequest.SetFilterPreference( "First Name", tbFirstName.Text );
            fRequest.SetFilterPreference( "Last Name", tbLastName.Text );
            fRequest.SetFilterPreference( "Request Date Range", drpRequestDates.DelimitedValues );
            fRequest.SetFilterPreference( "Response Date Range", drpResponseDates.DelimitedValues );
            fRequest.SetFilterPreference( "Report Status", tbReportStatus.Text );
            fRequest.SetFilterPreference( "Record Found", ddlRecordFound.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the current filters
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fRequest_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Request Date Range":
                case "Response Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gRequest_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRequest_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                BackgroundCheckRow request = e.Row.DataItem as BackgroundCheckRow;
                if ( !request.HasWorkflow )
                {
                    foreach ( var lbWorkflow in e.Row.Cells[6].ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lbWorkflow.Visible = false;
                    }
                }

                if ( !request.RecordFound.HasValue || request.RecordFound.Value == false )
                {
                    foreach ( var lbReport in e.Row.Cells[5].ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lbReport.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequest_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null && bc.PersonAlias != null )
                {
                    int personId = e.RowKeyId;
                    try
                    {
                        Response.Redirect( string.Format( "~/Person/{0}", bc.PersonAlias.PersonId ), false );
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    catch ( ThreadAbortException )
                    {
                        // Can safely ignore this exception
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Data event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequest_Data( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null )
                {
                    string url = new Checkr().GetReportUrl( bc.ResponseId );
                    if ( url.IsNotNullOrWhiteSpace() && url != "Unauthorized" )
                    {
                        try
                        {
                            Response.Redirect( url, false );
                            Context.ApplicationInstance.CompleteRequest(); // https://blogs.msdn.microsoft.com/tmarq/2009/06/25/correct-use-of-system-web-httpresponse-redirect/
                        }
                        catch ( ThreadAbortException )
                        {
                            // Can safely ignore this exception
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Handles the ViewWorkflow event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequest_ViewWorkflow( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null )
                {
                    var qryParms = new Dictionary<string, string> { { "WorkflowId", bc.WorkflowId.Value.ToString() } };
                    NavigateToLinkedPage( "WorkflowDetailPage", qryParms );
                }
            }
        }

        #endregion
        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbFirstName.Text = fRequest.GetFilterPreference( "First Name" );
            tbLastName.Text = fRequest.GetFilterPreference( "Last Name" );
            drpRequestDates.DelimitedValues = fRequest.GetFilterPreference( "Request Date Range" );
            drpResponseDates.DelimitedValues = fRequest.GetFilterPreference( "Response Date Range" );
            tbReportStatus.Text = fRequest.GetFilterPreference( "Report Status" );
            ddlRecordFound.SetValue( fRequest.GetFilterPreference( "ddlRecordFound" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new BackgroundCheckService( rockContext )
                    .Queryable( "PersonAlias.Person" ).AsNoTracking()
                    .Where( g =>
                        g.PersonAlias != null &&
                        g.PersonAlias.Person != null )
                    .Where( g =>
                        g.ForeignId == 2);

                // FirstName
                string firstName = fRequest.GetFilterPreference( "First Name" );
                if ( !string.IsNullOrWhiteSpace( firstName ) )
                {
                    qry = qry.Where( t =>
                        t.PersonAlias.Person.FirstName.StartsWith( firstName ) ||
                        t.PersonAlias.Person.NickName.StartsWith( firstName ) );
                }

                // LastName
                string lastName = fRequest.GetFilterPreference( "Last Name" );
                if ( !string.IsNullOrWhiteSpace( lastName ) )
                {
                    qry = qry.Where( t =>
                        t.PersonAlias.Person.LastName.StartsWith( lastName ) );
                }

                // Request Date Range
                var drpRequestDates = new DateRangePicker();
                drpRequestDates.DelimitedValues = fRequest.GetFilterPreference( "Request Date Range" );
                if ( drpRequestDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.RequestDate >= drpRequestDates.LowerValue.Value );
                }

                if ( drpRequestDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpRequestDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.RequestDate < upperDate );
                }

                // Response Date Range
                var drpResponseDates = new DateRangePicker();
                drpResponseDates.DelimitedValues = fRequest.GetFilterPreference( "Response Date Range" );
                if ( drpResponseDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.ResponseDate >= drpResponseDates.LowerValue.Value );
                }

                if ( drpResponseDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpResponseDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.ResponseDate < upperDate );
                }

                // Report Status
                string reportStatus = fRequest.GetFilterPreference( "Report Status" );
                if ( !string.IsNullOrWhiteSpace( reportStatus ) )
                {
                    qry = qry.Where( t => t.Status == reportStatus );
                }

                // Record Found
                string recordFound = fRequest.GetFilterPreference( "Record Found" );
                if ( !string.IsNullOrWhiteSpace( recordFound ) )
                {
                    if ( recordFound == "Yes" )
                    {
                        qry = qry.Where( t =>
                            t.RecordFound.HasValue &&
                            t.RecordFound.Value );
                    }
                    else if ( recordFound == "No" )
                    {
                        qry = qry.Where( t =>
                            t.RecordFound.HasValue &&
                            !t.RecordFound.Value );
                    }
                }

                List<Rock.Model.BackgroundCheck> items = null;
                SortProperty sortProperty = gRequest.SortProperty;
                if ( sortProperty != null )
                {
                    items = qry.Sort( sortProperty ).ToList();
                }
                else
                {
                    items = qry.OrderByDescending( d => d.RequestDate ).ToList();
                }

                gRequest.DataSource = items.Select( b => new BackgroundCheckRow
                    {
                        Name = b.PersonAlias.Person.LastName + ", " + b.PersonAlias.Person.NickName,
                        Id = b.Id,
                        PersonId = b.PersonAlias.PersonId,
                        HasWorkflow = b.WorkflowId.HasValue,
                        RequestDate = b.RequestDate,
                        ResponseDate = b.ResponseDate,
                        RecordFound = b.RecordFound,
                        RecordFoundLabel = b.RecordFound.HasValue ? (
                            b.RecordFound.Value ?
                                "<span class='label label-warning'>Yes</span>" :
                                "<span class='label label-success'>No</span>" ) :
                            string.Empty,
                        HasResponseData = !string.IsNullOrWhiteSpace( b.ResponseData ),
                        ResponseDocumentText = b.ResponseDocumentId.HasValue ? "<i class='fa fa-file-pdf-o fa-lg'></i>" : "",
                    ResponseId = b.ResponseId,
                    ReportStatus = b.Status.SplitCase()
                    } ).ToList();

                gRequest.DataBind();
            }
        }

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        /// <summary>
        /// The Checkr table row columns tags
        /// </summary>
        private class BackgroundCheckRow : RockDynamic
        {
            public string Name { get; set; }

            public int Id { get; set; }

            public int PersonId { get; set; }

            public bool HasWorkflow { get; set; }

            public DateTime RequestDate { get; set; }

            public DateTime? ResponseDate { get; set; }

            public bool? RecordFound { get; set; }

            public string RecordFoundLabel { get; set; }

            public bool HasResponseData { get; set; }

            public string ResponseDocumentText { get; set; }

            public string ResponseId { get; set; }

            public string ReportStatus { get; set; }
        }
        #endregion
    }
}