﻿// <copyright>
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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block used to list Benevolence Requests
    /// </summary>
    [DisplayName( "Benevolence Request List" )]
    [Category( "Finance" )]
    [Description( "Block used to list Benevolence Requests." )]
    [LinkedPage( "Detail Page" )]
    [GroupField( "Case Worker Group", "The group to draw case workers from", true, "26E7148C-2059-4F45-BCFE-32230A12F0DC" )]
    public partial class BenevolenceRequestList : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gList.GridRebind += gList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            gList.DataKeyNames = new string[] { "Id" };
            gList.RowDataBound += gList_RowDataBound;
            gList.Actions.AddClick += gList_AddClick;
            gList.GridRebind += gList_GridRebind;
            gList.Actions.ShowAdd = true;
            gList.IsDeleteEnabled = true;

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
                SetFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetFilter();
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Start Date", "Start Date", ( drpDate.LowerValue.HasValue ) ? drpDate.LowerValue.Value.ToString( "o" ) : "" );
            rFilter.SaveUserPreference( "End Date", "End Date", ( drpDate.UpperValue.HasValue ) ? drpDate.UpperValue.Value.ToString( "o" ) : "" );
            rFilter.SaveUserPreference( "First Name", "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( "Last Name", "Last Name", tbLastName.Text );
            rFilter.SaveUserPreference( "Government ID", "Government ID", tbGovernmentId.Text );
            rFilter.SaveUserPreference( "Case Worker", "Case Worker", ddlCaseWorker.SelectedItem.Value );
            rFilter.SaveUserPreference( "Result", "Result", ddlResult.SelectedItem.Value );
            rFilter.SaveUserPreference( "Status", "Status", ddlStatus.SelectedItem.Value );

            BindGrid();
        }

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( e.Key == "Start Date" )
            {
                e.Value = DateTime.Parse( e.Value ).ToShortDateString();
                return;
            }
            else if ( e.Key == "End Date" )
            {
                e.Value = DateTime.Parse( e.Value ).ToShortDateString();
                return;
            }
            else if ( e.Key == "First Name" )
            {
                return;
            }
            else if ( e.Key == "Last Name" )
            {
                return;
            }
            else if ( e.Key == "Government ID" )
            {
                return;
            }
            else if ( e.Key == "Case Worker" )
            {
                int personAliasId = 0;
                if ( int.TryParse( e.Value, out personAliasId ) )
                {
                    var personAlias = new PersonAliasService( new RockContext() ).Get( personAliasId );
                    if ( personAlias != null )
                    {
                        e.Value = personAlias.Person.FullName;
                    }
                }
                return;
            }
            else if ( e.Key == "Result" )
            {
                int definedValueId = 0;
                if ( int.TryParse( e.Value, out definedValueId ) )
                {
                    var definedValue = DefinedValueCache.Read( definedValueId );
                    if ( definedValue != null )
                    {
                        e.Value = definedValue.Value;
                    }
                }
                return;
            }
            else if ( e.Key == "Status" )
            {
                int definedValueId = 0;
                if ( int.TryParse( e.Value, out definedValueId ) )
                {
                    var definedValue = DefinedValueCache.Read( definedValueId );
                    if ( definedValue != null )
                    {
                        e.Value = definedValue.Value;
                    }
                }
                return;
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        public void gList_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                BenevolenceRequest benevolenceRequest = e.Row.DataItem as BenevolenceRequest;
                if ( benevolenceRequest != null )
                {
                    Literal lName = e.Row.FindControl( "lName" ) as Literal;
                    if ( lName != null )
                    {
                        if ( benevolenceRequest.RequestedByPersonAlias != null )
                        {
                            lName.Text = String.Format( "<a href=\"{0}\">{1}</a>", ResolveUrl( string.Format( "~/Person/{0}", benevolenceRequest.RequestedByPersonAlias.PersonId ) ), benevolenceRequest.RequestedByPersonAlias.Person.FullName ?? "" );
                        }
                        else
                        {
                            lName.Text = String.Format( "{0} {1}", benevolenceRequest.FirstName, benevolenceRequest.LastName );
                        }
                    }

                    Literal lCaseWorker = e.Row.FindControl( "lCaseWorker" ) as Literal;
                    if ( lCaseWorker != null && benevolenceRequest.CaseWorkerPersonAliasId != null )
                    {
                        lCaseWorker.Text = String.Format( "<a href=\"{0}\">{1}</a>", ResolveUrl( string.Format( "~/Person/{0}", benevolenceRequest.CaseWorkerPersonAlias.PersonId ) ), benevolenceRequest.CaseWorkerPersonAlias.Person.FullName ?? "" );
                    }

                    Literal lResults = e.Row.FindControl( "lResults" ) as Literal;
                    if ( lResults != null )
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append( "<div class='col-md-12'>" );
                        foreach ( BenevolenceResult result in benevolenceRequest.BenevolenceResults )
                        {
                            if ( result.Amount != null )
                            {
                                stringBuilder.Append( String.Format( "<div class='row'>{0} ({1:C})</div>", result.ResultTypeValue, result.Amount ) );
                            }
                            else
                            {
                                stringBuilder.Append( String.Format( "<div class='row'>{0}</div>", result.ResultTypeValue ) );
                            }
                        }
                        stringBuilder.Append( "</div>" );
                        lResults.Text = stringBuilder.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gList_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "BenevolenceRequestId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "BenevolenceRequestId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            drpDate.LowerValue = rFilter.GetUserPreference( "Start Date" ).AsDateTime();
            drpDate.UpperValue = rFilter.GetUserPreference( "End Date" ).AsDateTime();

            tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Government ID" );

            Guid groupGuid = GetAttributeValue( "CaseWorkerGroup" ).AsGuid();
            var listData = new GroupMemberService( new RockContext() ).Queryable( "Person, Group" )
                .Where( gm => gm.Group.Guid == groupGuid )
                .Select( gm => gm.Person )
                .ToList();
            ddlCaseWorker.DataSource = listData;
            ddlCaseWorker.DataTextField = "FullName";
            ddlCaseWorker.DataValueField = "PrimaryAliasId";
            ddlCaseWorker.DataBind();
            ddlCaseWorker.Items.Insert( 0, new ListItem() );
            ddlCaseWorker.SetValue( rFilter.GetUserPreference( "Case Worker" ) );

            ddlResult.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) ), true );
            ddlResult.SetValue( rFilter.GetUserPreference( "Result" ) );

            ddlStatus.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS ) ), true );
            ddlStatus.SetValue( rFilter.GetUserPreference( "Status" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            phSummary.Controls.Clear();
            rFilter.Visible = true;
            gList.Visible = true;
            RockContext rockContext = new RockContext();
            BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );
            var qry = benevolenceRequestService.Queryable( "BenevolenceResults,BenevolenceResults.ResultTypeValue,RequestedByPersonAlias,RequestedByPersonAlias.Person,CaseWorkerPersonAlias,CaseWorkerPersonAlias.Person" );

            //Filter by Start Date
            DateTime? startDate = drpDate.LowerValue;
            if ( startDate != null )
            {
                qry = qry.Where( b => b.RequestDateTime >= startDate );
            }

            //Filter by End Date
            DateTime? endDate = drpDate.UpperValue;
            if ( endDate != null )
            {
                qry = qry.Where( b => b.RequestDateTime <= endDate );
            }

            // Filter by First Name
            string firstName = tbFirstName.Text;
            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                qry = qry.Where( b => b.FirstName.StartsWith( firstName ) );
            }

            // Filter by Last Name
            string lastName = tbLastName.Text;
            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                qry = qry.Where( b => b.LastName.StartsWith( lastName ) );
            }

            // Filter by Government Id
            string governmentId = tbGovernmentId.Text;
            if ( !string.IsNullOrWhiteSpace( governmentId ) )
            {
                qry = qry.Where( b => b.GovernmentId.StartsWith( governmentId ) );
            }

            //Filter by Case Worker
            int? caseWorkerPersonAliasId = ddlCaseWorker.SelectedItem.Value.AsIntegerOrNull();
            if ( caseWorkerPersonAliasId != null )
            {
                qry = qry.Where( b => b.CaseWorkerPersonAliasId == caseWorkerPersonAliasId );
            }

            //Filter by Result
            int? resultTypeValueId = ddlResult.SelectedItem.Value.AsIntegerOrNull();
            if ( resultTypeValueId != null )
            {
                qry = qry.Where( b => b.BenevolenceResults.Where( r => r.ResultTypeValueId == resultTypeValueId ).Count() > 0 );
            }

            //Filter by Request Status
            int? requestStatusValueId = ddlStatus.SelectedItem.Value.AsIntegerOrNull();
            if ( requestStatusValueId != null )
            {
                qry = qry.Where( b => b.RequestStatusValueId == requestStatusValueId );
            }

            gList.DataSource = qry.ToList();
            gList.DataBind();

            //Builds the Totals section
            var definedTypeCache = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) );
            Dictionary<string, decimal> resultTotals = new Dictionary<string, decimal>();
            decimal grandTotal = 0;
            foreach ( BenevolenceRequest request in qry.ToList() )
            {
                foreach ( BenevolenceResult result in request.BenevolenceResults )
                {
                    if ( result.Amount != null )
                    {
                        if ( resultTotals.ContainsKey( result.ResultTypeValue.Value ) )
                        {
                            resultTotals[result.ResultTypeValue.Value] += result.Amount.Value;
                        }
                        else
                        {
                            resultTotals.Add( result.ResultTypeValue.Value, result.Amount.Value );
                        }
                        grandTotal += result.Amount.Value;
                    }
                }
            }
            foreach ( KeyValuePair<string, decimal> keyValuePair in resultTotals )
            {
                phSummary.Controls.Add( new LiteralControl( String.Format( "<div class='row'><div class='col-md-12'>{0}: {1:C}</div></div>", keyValuePair.Key, keyValuePair.Value ) ) );
            }
            phSummary.Controls.Add( new LiteralControl( String.Format( "<div class='row'><div class='col-md-12'><b>Grand Total: {0:C}</b></div></div>", grandTotal ) ) );
        }

        #endregion
    }
}