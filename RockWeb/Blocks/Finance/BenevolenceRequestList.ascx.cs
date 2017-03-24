﻿// <copyright>
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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block used to list Benevolence Requests
    /// </summary>
    [DisplayName( "Benevolence Request List" )]
    [Category( "Finance" )]
    [Description( "Block used to list Benevolence Requests." )]

    [ContextAware( typeof( Person ) )]
    [LinkedPage( "Detail Page" )]
    [SecurityRoleField( "Case Worker Role", "The security role to draw case workers from", true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE )]
    public partial class BenevolenceRequestList : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets the target person
        /// </summary>
        /// <value>
        /// The target person.
        /// </value>
        protected Person TargetPerson { get; private set; }

        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Private Members

        /// <summary>
        /// Holds whether or not the person can add, edit, and delete.
        /// </summary>
        private bool _canAddEditDelete = false;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
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

            _canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gList.DataKeyNames = new string[] { "Id" };
            gList.RowDataBound += gList_RowDataBound;
            gList.Actions.AddClick += gList_AddClick;
            gList.GridRebind += gList_GridRebind;
            gList.Actions.ShowAdd = _canAddEditDelete;
            gList.IsDeleteEnabled = _canAddEditDelete;

            // in case this is used as a Person Block, set the TargetPerson 
            TargetPerson = ContextEntity<Person>();
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

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();
            
            int entityTypeId = new BenevolenceRequest().TypeId;
            foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn )
                .OrderByDescending( a => a.EntityTypeQualifierColumn )
                .ThenBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                AvailableAttributes.Add( AttributeCache.Read( attributeModel ) );
            }
            
        }

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
            rFilter.SaveUserPreference( "Start Date", "Start Date", drpDate.LowerValue.HasValue ? drpDate.LowerValue.Value.ToString( "o" ) : string.Empty );
            rFilter.SaveUserPreference( "End Date", "End Date", drpDate.UpperValue.HasValue ? drpDate.UpperValue.Value.ToString( "o" ) : string.Empty );
            rFilter.SaveUserPreference( "First Name", "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( "Last Name", "Last Name", tbLastName.Text );
            rFilter.SaveUserPreference( "Government ID", "Government ID", tbGovernmentId.Text );
            rFilter.SaveUserPreference( "Case Worker", "Case Worker", ddlCaseWorker.SelectedItem.Value );
            rFilter.SaveUserPreference( "Result", "Result", ddlResult.SelectedItem.Value );
            rFilter.SaveUserPreference( "Status", "Status", ddlStatus.SelectedItem.Value );
            rFilter.SaveUserPreference( "Campus", "Campus", cpCampus.SelectedCampusId.ToString() );

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
                            rFilter.SaveUserPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
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
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Start Date":
                case "End Date":
                    var dateTime = e.Value.AsDateTime();
                    if ( dateTime.HasValue )
                    {
                        e.Value = dateTime.Value.ToShortDateString();
                    }
                    else
                    {
                        e.Value = null;
                    }

                    return;

                case "First Name":
                    return;

                case "Last Name":
                    return;

                case "Campus":
                    {
                        int? campusId = e.Value.AsIntegerOrNull();
                        if( campusId.HasValue )
                        {
                            e.Value = CampusCache.Read( campusId.Value ).Name;
                        }
                        return;
                    }

                case "Government ID":
                    return;

                case "Case Worker":
                    int? personAliasId = e.Value.AsIntegerOrNull();
                    if ( personAliasId.HasValue )
                    {
                        var personAlias = new PersonAliasService( new RockContext() ).Get( personAliasId.Value );
                        if ( personAlias != null )
                        {
                            e.Value = personAlias.Person.FullName;
                        }
                    }

                    return;

                case "Result":
                case "Status":
                    var definedValueId = e.Value.AsIntegerOrNull();
                    if ( definedValueId.HasValue )
                    {
                        var definedValue = DefinedValueCache.Read( definedValueId.Value );
                        if ( definedValue != null )
                        {
                            e.Value = definedValue.Value;
                        }
                    }

                    return;

                default:
                    e.Value = string.Empty;
                    return;
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
                            lName.Text = string.Format( "<a href=\"{0}\">{1}</a>", ResolveUrl( string.Format( "~/Person/{0}", benevolenceRequest.RequestedByPersonAlias.PersonId ) ), benevolenceRequest.RequestedByPersonAlias.Person.FullName ?? string.Empty );
                        }
                        else
                        {
                            lName.Text = string.Format( "{0} {1}", benevolenceRequest.FirstName, benevolenceRequest.LastName );
                        }
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
                                stringBuilder.Append( string.Format( "<div class='row'>{0} ({1}{2:0.00})</div>", result.ResultTypeValue, GlobalAttributesCache.Value( "CurrencySymbol" ), result.Amount ) );
                            }
                            else
                            {
                                stringBuilder.Append( string.Format( "<div class='row'>{0}</div>", result.ResultTypeValue ) );
                            }
                        }

                        stringBuilder.Append( "</div>" );
                        lResults.Text = stringBuilder.ToString();
                    }

                    HighlightLabel hlStatus = e.Row.FindControl( "hlStatus" ) as HighlightLabel;
                    if ( hlStatus != null )
                    {
                        switch ( benevolenceRequest.RequestStatusValue.Value )
                        {
                            case "Approved":
                                hlStatus.Text = "Approved";
                                hlStatus.LabelType = LabelType.Success;
                                return;
                            case "Denied":
                                hlStatus.Text = "Denied";
                                hlStatus.LabelType = LabelType.Danger;
                                return;
                            case "Pending":
                                hlStatus.Text = "Pending";
                                hlStatus.LabelType = LabelType.Default;
                                return;
                        }
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
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "BenevolenceRequestId", 0.ToString() );
            if ( TargetPerson != null )
            {
                qryParams.Add( "PersonId", TargetPerson.Id.ToString() );
            }

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Edit event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "BenevolenceRequestId", e.RowKeyId.ToString() );
            if ( TargetPerson != null )
            {
                qryParams.Add( "PersonId", TargetPerson.Id.ToString() );
            }

            NavigateToLinkedPage( "DetailPage", qryParams );
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            BenevolenceRequestService service = new BenevolenceRequestService( rockContext );
            BenevolenceRequest benevolenceRequest = service.Get( e.RowKeyId );
            if ( benevolenceRequest != null )
            {
                string errorMessage;
                if ( !service.CanDelete( benevolenceRequest, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                service.Delete( benevolenceRequest );
                rockContext.SaveChanges();
            }

            BindGrid();
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

            cpCampus.Campuses = CampusCache.All();
            cpCampus.SelectedCampusId = rFilter.GetUserPreference( "Campus" ).AsInteger();

            // hide the First/Last name filter if this is being used as a Person block
            tbFirstName.Visible = TargetPerson == null;
            tbLastName.Visible = TargetPerson == null;

            tbFirstName.Text = rFilter.GetUserPreference( "First Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Last Name" );
            tbLastName.Text = rFilter.GetUserPreference( "Government ID" );

            Guid groupGuid = GetAttributeValue( "CaseWorkerRole" ).AsGuid();
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

            // set attribute filters
            BindAttributes();
            AddDynamicControls();
        }

        private void AddDynamicControls()
        {
            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = (IRockControl)control;
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

                        string savedValue = rFilter.GetUserPreference( attribute.Key);
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

                    bool columnExists = gList.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gList.Columns.Add( boundField );
                    }
                }
            }

            // Add delete column
            var deleteField = new DeleteField();
            gList.Columns.Add( deleteField );
            deleteField.Click += gList_Delete;
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
            var qry = benevolenceRequestService.Queryable( "BenevolenceResults,RequestedByPersonAlias,RequestedByPersonAlias.Person,CaseWorkerPersonAlias,CaseWorkerPersonAlias.Person" );

            // Filter by Start Date
            DateTime? startDate = drpDate.LowerValue;
            if ( startDate != null )
            {
                qry = qry.Where( b => b.RequestDateTime >= startDate );
            }

            // Filter by End Date
            DateTime? endDate = drpDate.UpperValue;
            if ( endDate != null )
            {
                qry = qry.Where( b => b.RequestDateTime <= endDate );
            }

            // Filter by Campus
            if ( cpCampus.SelectedCampusId.HasValue )
            {
                qry = qry.Where( b => b.CampusId == cpCampus.SelectedCampusId );
            }

            if ( TargetPerson != null )
            {
                // show benevolence request for the target person and also for their family members
                var qryFamilyMembers = TargetPerson.GetFamilyMembers( true, rockContext );
                qry = qry.Where( a => a.RequestedByPersonAliasId.HasValue && qryFamilyMembers.Any( b => b.PersonId == a.RequestedByPersonAlias.PersonId ) );
            }
            else
            {
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
            }

            // Filter by Government Id
            string governmentId = tbGovernmentId.Text;
            if ( !string.IsNullOrWhiteSpace( governmentId ) )
            {
                qry = qry.Where( b => b.GovernmentId.StartsWith( governmentId ) );
            }

            // Filter by Case Worker
            int? caseWorkerPersonAliasId = ddlCaseWorker.SelectedItem.Value.AsIntegerOrNull();
            if ( caseWorkerPersonAliasId != null )
            {
                qry = qry.Where( b => b.CaseWorkerPersonAliasId == caseWorkerPersonAliasId );
            }

            // Filter by Result
            int? resultTypeValueId = ddlResult.SelectedItem.Value.AsIntegerOrNull();
            if ( resultTypeValueId != null )
            {
                qry = qry.Where( b => b.BenevolenceResults.Where( r => r.ResultTypeValueId == resultTypeValueId ).Count() > 0 );
            }

            // Filter by Request Status
            int? requestStatusValueId = ddlStatus.SelectedItem.Value.AsIntegerOrNull();
            if ( requestStatusValueId != null )
            {
                qry = qry.Where( b => b.RequestStatusValueId == requestStatusValueId );
            }

            SortProperty sortProperty = gList.SortProperty;
            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "TotalAmount" )
                {
                    if ( sortProperty.Direction == SortDirection.Descending )
                    {
                        qry = qry.OrderByDescending( a => a.BenevolenceResults.Sum( b => b.Amount ) );
                    }
                    else
                    {
                        qry = qry.OrderBy( a => a.BenevolenceResults.Sum( b => b.Amount ) );
                    }
                }
                else
                {
                    qry = qry.Sort( sortProperty );
                }
            }
            else
            {
                qry = qry.OrderByDescending( a => a.RequestDateTime ).ThenByDescending( a => a.Id );
            }

            // Filter query by any configured attribute filters
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                var parameterExpression = attributeValueService.ParameterExpression;

                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                        var expression = attribute.FieldType.Field.AttributeFilterExpression( attribute.QualifierValues, filterValues, parameterExpression );
                        if ( expression != null )
                        {
                            var attributeValues = attributeValueService
                                .Queryable()
                                .Where( v => v.Attribute.Id == attribute.Id );

                            attributeValues = attributeValues.Where( parameterExpression, expression, null );

                            qry = qry.Where( w => attributeValues.Select( v => v.EntityId ).Contains( w.Id ) );
                        }
                    }
                }
            }

            var list = qry.ToList();

            gList.DataSource = list;
            gList.DataBind();

            // Builds the Totals section
            var definedTypeCache = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) );
            Dictionary<string, decimal> resultTotals = new Dictionary<string, decimal>();
            decimal grandTotal = 0;
            foreach ( BenevolenceRequest request in list )
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
                phSummary.Controls.Add( new LiteralControl( string.Format( "<div class='row'><div class='col-xs-8'>{0}: </div><div class='col-xs-4 text-right'>{1}{2:#,##0.00}</div></div>", keyValuePair.Key, GlobalAttributesCache.Value( "CurrencySymbol" ), keyValuePair.Value ) ) );
            }

            phSummary.Controls.Add( new LiteralControl( string.Format( "<div class='row'><div class='col-xs-8'><b>Total: </div><div class='col-xs-4 text-right'>{0}{1:#,##0.00}</b></div></div>", GlobalAttributesCache.Value( "CurrencySymbol" ), grandTotal ) ) );
        }

        #endregion
    }
}