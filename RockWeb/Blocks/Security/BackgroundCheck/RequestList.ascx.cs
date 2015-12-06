// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security.BackgroundCheck
{
    [DisplayName( "Request List" )]
    [Category( "Security > Background Check" )]
    [Description( "Lists all the background check requests." )]

    [LinkedPage("Workflow Detail Page", "The page to view details about the background check workflow")]
    public partial class RequestList : RockBlock, ISecondaryBlock
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
            fRequest.SaveUserPreference( "First Name", tbFirstName.Text );
            fRequest.SaveUserPreference( "Last Name", tbLastName.Text );
            fRequest.SaveUserPreference( "Request Date Range", drpRequestDates.DelimitedValues );
            fRequest.SaveUserPreference( "Response Date Range", drpResponseDates.DelimitedValues );
            fRequest.SaveUserPreference( "Record Found", ddlRecordFound.SelectedValue );

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
        private void gRequest_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        void gRequest_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                dynamic request = e.Row.DataItem;

                if ( !request.HasWorkflow )
                {
                    foreach ( var lb in e.Row.Cells[6].ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lb.Visible = false;
                    }
                }

                if ( !request.HasResponseXml )
                {
                    foreach( var lb in e.Row.Cells[5].ControlsOfTypeRecursive<LinkButton>() )
                    {
                        lb.Visible = false;
                    }
                }

            }
        }

        void gRequest_RowSelected( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null && bc.PersonAlias != null )
                {
                    int personId = e.RowKeyId;
                    Response.Redirect( string.Format( "~/Person/{0}", bc.PersonAlias.PersonId ), false );
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
        }


        /// <summary>
        /// Handles the XML event of the gRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequest_XML( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var bc = new BackgroundCheckService( rockContext ).Get( e.RowKeyId );
                if ( bc != null )
                {
                    tbResponseXml.Text = bc.ResponseXml;
                    dlgResponse.Show();
                }
            }
        }

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
            tbFirstName.Text = fRequest.GetUserPreference( "First Name" );
            tbLastName.Text = fRequest.GetUserPreference( "Last Name" );
            drpRequestDates.DelimitedValues = fRequest.GetUserPreference( "Request Date Range" );
            drpResponseDates.DelimitedValues = fRequest.GetUserPreference( "Response Date Range" );
            ddlRecordFound.SetValue( fRequest.GetUserPreference( "ddlRecordFound" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var qry = new BackgroundCheckService( rockContext )
                    .Queryable("PersonAlias.Person").AsNoTracking()
                    .Where( g => 
                        g.PersonAlias != null && 
                        g.PersonAlias.Person != null );

                // FirstName
                string firstName = fRequest.GetUserPreference( "First Name" );
                if ( !string.IsNullOrWhiteSpace( firstName ) )
                {
                    qry = qry.Where( t =>
                        t.PersonAlias.Person.FirstName.StartsWith( firstName ) ||
                        t.PersonAlias.Person.NickName.StartsWith( firstName ) );
                }

                // LastName
                string lastName = fRequest.GetUserPreference( "Last Name" );
                if ( !string.IsNullOrWhiteSpace( lastName ) )
                {
                    qry = qry.Where( t =>
                        t.PersonAlias.Person.LastName.StartsWith( lastName ) );
                }

                // Request Date Range
                var drpRequestDates = new DateRangePicker();
                drpRequestDates.DelimitedValues = fRequest.GetUserPreference( "Request Date Range" );
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
                drpResponseDates.DelimitedValues = fRequest.GetUserPreference( "Response Date Range" );
                if ( drpResponseDates.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.ResponseDate >= drpResponseDates.LowerValue.Value );
                }

                if ( drpResponseDates.UpperValue.HasValue )
                {
                    DateTime upperDate = drpResponseDates.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.ResponseDate < upperDate );
                }

                // Record Found
                string recordFound = fRequest.GetUserPreference( "Record Found" );
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

                gRequest.DataSource = items.Select( b => new
                    {
                        Name = b.PersonAlias.Person.LastName + ", " + b.PersonAlias.Person.NickName,
                        b.Id,
                        PersonId = b.PersonAlias.PersonId,
                        HasWorkflow = b.WorkflowId.HasValue,
                        b.RequestDate,
                        b.ResponseDate,
                        b.RecordFound,
                        RecordFoundLabel = b.RecordFound.HasValue ? (
                            b.RecordFound.Value ? 
                                "<span class='label label-warning'>Yes</span>" : 
                                "<span class='label label-success'>No</span>" ) : 
                            string.Empty,
                        HasResponseXml = !string.IsNullOrWhiteSpace( b.ResponseXml ),
                        ResponseDocumentText = b.ResponseDocumentId.HasValue ? "<i class='fa fa-file-pdf-o fa-lg'></i>" : "",
                        ResponseDocumentId = b.ResponseDocumentId ?? 0
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

        #endregion

}
}