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
using System.Linq;
using System.Linq.Expressions;
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
    #region Block Attributes
    [DisplayName( "Benevolence Request List" )]
    [Category( "Finance" )]
    [Description( "Block used to list Benevolence Requests." )]

    [ContextAware( typeof( Person ) )]
    [SecurityRoleField( "Case Worker Role", "The security role to draw case workers from", true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE )]

    [LinkedPage( "Detail Page",
        Description = "Page used to modify and create benevolence requests.",
        IsRequired = true,
        Order = 1,
        DefaultValue = Rock.SystemGuid.Page.BENEVOLENCE_REQUEST_DETAIL,
        Key = AttributeKey.BenevolenceRequestDetailPageKey )]
    [LinkedPage(
        "Configuration Page",
        Description = "Page used to modify and create benevolence type.",
        IsRequired = true,
        Order = 2,
        DefaultValue = Rock.SystemGuid.Page.BENEVOLENCE_TYPES,
        Key = AttributeKey.ConfigurationPage )]
    [CustomCheckboxListField( "Hide Columns on Grid",
        Description = "The grid columns that should be hidden.",
        ListSource = "Assigned To, Government Id, Total Amount, Total Results",
        IsRequired = false,
        Order = 3,
        RepeatColumns = 3,
        RepeatDirection = RepeatDirection.Horizontal,
        Key = AttributeKey.HideColumnsAttributeKey )]
    [CustomCheckboxListField( "Include Benevolence Types",
        Description = "The benevolence types to display in the list.<br/><i>If none are selected, all types will be included.<i>",
       ListSource = FilterBenevolenceTypesSql,
        IsRequired = false,
        Order = 4,
        RepeatColumns = 3,
        RepeatDirection = RepeatDirection.Horizontal,
        Key = AttributeKey.FilterBenevolenceTypesAttributeKey )]
    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "3131C55A-8753-435F-85F3-DF777EFBD1C8" )]
    public partial class BenevolenceRequestList : RockBlock, ICustomGridColumns
    {
        #region SQL Constants
        private const string FilterBenevolenceTypesSql = @"SELECT
                                                             bt.[Guid] AS [Value],
                                                             bt.[Name] AS [Text]
                                                           FROM BenevolenceType AS bt";
        #endregion SQL Constants

        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string ConfigurationPage = "ConfigurationPage";
            public const string HideColumnsAttributeKey = "HideColumnsAttributeKey";
            public const string FilterBenevolenceTypesAttributeKey = "FilterBenevolenceTypesAttributeKey";
            public const string BenevolenceRequestDetailPageKey = "BenevolenceRequestDetail";
        }
        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <value>The target person.</value>
        protected Person TargetPerson { get; private set; }

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>The available attributes.</value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        /// <summary>
        /// Holds whether or not the person can add, edit, and delete.
        /// </summary>
        protected bool CanAddEditDelete { get; private set; }
        #endregion Properties

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

            lbBenevolenceTypes.Visible = UserCanAdministrate;

            gList.DataKeyNames = new string[] { "Id" };
            gList.GridRebind += gList_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.ClearFilterClick += rFilter_ClearFilterClick;
            CanAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gList.RowDataBound += gList_RowDataBound;
            gList.Actions.AddClick += gList_AddClick;
            gList.GridRebind += gList_GridRebind;
            gList.Actions.ShowAdd = CanAddEditDelete;
            gList.IsDeleteEnabled = CanAddEditDelete;

            // in case this is used as a Person Block, set the TargetPerson 
            TargetPerson = ContextEntity<Person>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindAttributes();
                AddDynamicControls();
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

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
                AvailableAttributes.Add( AttributeCache.Get( attributeModel ) );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindFilter();
            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        private void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();
            BindFilter( clearFilter: true );
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( "Start Date", "Start Date", drpDate.LowerValue.HasValue ? drpDate.LowerValue.Value.ToString( "o" ) : string.Empty );
            rFilter.SetFilterPreference( "End Date", "End Date", drpDate.UpperValue.HasValue ? drpDate.UpperValue.Value.ToString( "o" ) : string.Empty );
            rFilter.SetFilterPreference( "First Name", "First Name", tbFirstName.Text );
            rFilter.SetFilterPreference( "Last Name", "Last Name", tbLastName.Text );
            rFilter.SetFilterPreference( "Government ID", "Government ID", tbGovernmentId.Text );
            rFilter.SetFilterPreference( "Case Worker", "Case Worker", ddlCaseWorker.SelectedItem.Value );
            rFilter.SetFilterPreference( "Result", "Result", dvpResult.SelectedItem.Value );
            rFilter.SetFilterPreference( "Status", "Status", dvpStatus.SelectedItem.Value );
            rFilter.SetFilterPreference( "Benevolence Types", "Benevolence Types", cblBenevolenceType.SelectedValues.AsDelimited( "|" ) );
            rFilter.SetFilterPreference( "Campus", "Campus", cpCampus.SelectedCampusId.ToString() );

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
                            rFilter.SetFilterPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
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
                        if ( campusId.HasValue )
                        {
                            e.Value = CampusCache.Get( campusId.Value ).Name;
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
                        var definedValue = DefinedValueCache.Get( definedValueId.Value );
                        if ( definedValue != null )
                        {
                            e.Value = definedValue.Value;
                        }
                    }

                    return;
                case "Benevolence Types":
                    var benevolencTypeValueIds = e.Value.SplitDelimitedValues().Select( v => v.ToIntSafe() )?.ToList();
                    if ( benevolencTypeValueIds?.Count() > 0 )
                    {
                        var benevolenceTypes = new BenevolenceTypeService( new RockContext() ).GetByIds( benevolencTypeValueIds );
                        if ( benevolenceTypes != null )
                        {
                            e.Value = benevolenceTypes.Select( v => v.Name ).ToList().AsDelimited( ", " );
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
        public void gList_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

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
                                stringBuilder.Append( string.Format( "<div class='row'>{0} ({1})</div>", result.ResultTypeValue, result.Amount.FormatAsCurrency() ) );
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
                            default:
                                hlStatus.Text = benevolenceRequest.RequestStatusValue.Value;
                                hlStatus.LabelType = LabelType.Info;
                                return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gList control.
        /// </summary>
        protected void gList_AddClick( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "BenevolenceRequestId", 0.ToString() );
            if ( TargetPerson != null )
            {
                qryParams.Add( "PersonId", TargetPerson.Id.ToString() );
            }

            NavigateToLinkedPage( AttributeKey.BenevolenceRequestDetailPageKey, qryParams );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gGrid control.
        /// </summary>
        private void gGrid_DeleteClick( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var benevolenceRequestService = new BenevolenceRequestService( rockContext );
            var benevolenceRequest = benevolenceRequestService.Get( e.RowKeyId );
            string errorMessage;

            if ( benevolenceRequest == null )
            {
                return;
            }

            if ( !benevolenceRequestService.CanDelete( benevolenceRequest, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            benevolenceRequestService.Delete( benevolenceRequest );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the View event of the gList control.
        /// </summary>
        protected void gList_View( object sender, RowEventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "BenevolenceRequestId", e.RowKeyId.ToString() );
            if ( TargetPerson != null )
            {
                qryParams.Add( "PersonId", TargetPerson.Id.ToString() );
            }

            NavigateToLinkedPage( AttributeKey.BenevolenceRequestDetailPageKey, qryParams );
        }

        /// <summary>
        /// Handles the Delete event of the gList control.
        /// </summary>
        protected void dfBenevolenceRequest_Click( object sender, RowEventArgs e )
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
        /// Handles the Click event of the lbBenevolenceTypes control.
        /// </summary>
        protected void lbBenevolenceTypes_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.ConfigurationPage );
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        private void gList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }
        #endregion Events

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        /// <param name="clearFilter">if set to <c>true</c> [clear filter].</param>
        private void BindFilter( bool clearFilter = false )
        {
            drpDate.LowerValue = rFilter.GetFilterPreference( "Start Date" ).AsDateTime();
            drpDate.UpperValue = rFilter.GetFilterPreference( "End Date" ).AsDateTime();

            cpCampus.Campuses = CampusCache.All();
            cpCampus.SelectedCampusId = rFilter.GetFilterPreference( "Campus" ).AsInteger();

            // hide the First/Last name filter if this is being used as a Person block
            tbFirstName.Visible = TargetPerson == null;
            tbLastName.Visible = TargetPerson == null;

            tbFirstName.Text = rFilter.GetFilterPreference( "First Name" );
            tbLastName.Text = rFilter.GetFilterPreference( "Last Name" );
            tbGovernmentId.Text = rFilter.GetFilterPreference( "Government ID" );

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
            ddlCaseWorker.SetValue( rFilter.GetFilterPreference( "Case Worker" ) );

            dvpResult.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) ).Id;
            dvpResult.SetValue( rFilter.GetFilterPreference( "Result" ) );

            dvpStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS ) ).Id;
            dvpStatus.SetValue( rFilter.GetFilterPreference( "Status" ) );

            cblBenevolenceType.Items.Clear();

            cblBenevolenceType.DataSource = new BenevolenceTypeService( new RockContext() ).Queryable()
                .ToList();
            cblBenevolenceType.DataTextField = "Name";
            cblBenevolenceType.DataValueField = "Id";
            cblBenevolenceType.DataBind();

            cblBenevolenceType.SetValues( rFilter.GetFilterPreference( "Benevolence Types" ).SplitDelimitedValues() );
        }

        /// <summary>
        /// Adds the dynamic controls.
        /// </summary>
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

                        string savedValue = rFilter.GetFilterPreference( attribute.Key );
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

                        var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gList.Columns.Add( boundField );
                    }
                }
            }

            if ( CanAddEditDelete )
            {
                var deleteField = new DeleteField();
                deleteField.HeaderText = "&nbsp;";
                deleteField.Visible = true;
                gList.Columns.Add( deleteField );
                deleteField.Click += gGrid_DeleteClick;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            phSummary.Controls.Clear();
            rFilter.Visible = true;
            gList.Visible = true;

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gList.Actions.ShowAdd = canAddEditDelete;
            gList.IsDeleteEnabled = canAddEditDelete;

            var rockContext = new RockContext();
            var benevolenceRequestService = new BenevolenceRequestService( rockContext );

            var benevolenceRequests = benevolenceRequestService
                .Queryable( "BenevolenceResults,RequestedByPersonAlias,RequestedByPersonAlias.Person,CaseWorkerPersonAlias,CaseWorkerPersonAlias.Person" ).AsNoTracking();

            var hideGridColumns = GetAttributeValue( AttributeKey.HideColumnsAttributeKey )?.Split( ',' )?.Select( v => v.ToUpper() );
            var benevolenceTypeFilter = GetAttributeValue( AttributeKey.FilterBenevolenceTypesAttributeKey )
                ?.Split( ',' )
                ?.Where( v => v.IsNotNullOrWhiteSpace() )
                ?.Select( v => new Guid( v ) );

            // Filter by Start Date
            DateTime? startDate = drpDate.LowerValue;
            if ( startDate != null )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.RequestDateTime >= startDate );
            }

            // Filter by End Date
            DateTime? endDate = drpDate.UpperValue;
            if ( endDate != null )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.RequestDateTime <= endDate );
            }

            // Filter by Campus
            if ( cpCampus.SelectedCampusId.HasValue )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.CampusId == cpCampus.SelectedCampusId );
            }

            if ( TargetPerson != null )
            {
                // show benevolence request for the target person and also for their family members
                var qryFamilyMembers = TargetPerson.GetFamilyMembers( true, rockContext );
                benevolenceRequests = benevolenceRequests.Where( a => a.RequestedByPersonAliasId.HasValue && qryFamilyMembers.Any( b => b.PersonId == a.RequestedByPersonAlias.PersonId ) );
            }
            else
            {
                // Filter by First Name 
                string firstName = tbFirstName.Text;
                if ( !string.IsNullOrWhiteSpace( firstName ) )
                {
                    benevolenceRequests = benevolenceRequests.Where( b => b.FirstName.StartsWith( firstName ) );
                }

                // Filter by Last Name 
                string lastName = tbLastName.Text;
                if ( !string.IsNullOrWhiteSpace( lastName ) )
                {
                    benevolenceRequests = benevolenceRequests.Where( b => b.LastName.StartsWith( lastName ) );
                }
            }

            // Filter by Government Id
            string governmentId = tbGovernmentId.Text;
            if ( !string.IsNullOrWhiteSpace( governmentId ) )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.GovernmentId.StartsWith( governmentId ) );
            }

            // Filter by Case Worker
            int? caseWorkerPersonAliasId = ddlCaseWorker.SelectedItem.Value.AsIntegerOrNull();
            if ( caseWorkerPersonAliasId != null )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.CaseWorkerPersonAliasId == caseWorkerPersonAliasId );
            }

            // Filter by Result
            int? resultTypeValueId = dvpResult.SelectedItem.Value.AsIntegerOrNull();
            if ( resultTypeValueId != null )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.BenevolenceResults.Where( r => r.ResultTypeValueId == resultTypeValueId ).Count() > 0 );
            }

            // Filter by Request Status
            int? requestStatusValueId = dvpStatus.SelectedItem.Value.AsIntegerOrNull();
            if ( requestStatusValueId != null )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.RequestStatusValueId == requestStatusValueId );
            }

            // Filter by Benevolence Types
            var benevolenceTypeIds = cblBenevolenceType.SelectedValues.Where( v => v.ToIntSafe() > 0 ).Select( v => v.ToIntSafe() );

            if ( benevolenceTypeIds?.Count() > 0 )
            {
                benevolenceRequests = benevolenceRequests.Where( b => benevolenceTypeIds.Contains( b.BenevolenceTypeId ) );
            }

            if ( benevolenceTypeFilter?.Count() > 0 )
            {
                benevolenceRequests = benevolenceRequests.Where( b => benevolenceTypeFilter.Contains( b.BenevolenceType.Guid ) );
            }

            SortProperty sortProperty = gList.SortProperty;
            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "TotalAmount" )
                {
                    if ( sortProperty.Direction == SortDirection.Descending )
                    {
                        benevolenceRequests = benevolenceRequests.OrderByDescending( a => a.BenevolenceResults.Sum( b => b.Amount ) );
                    }
                    else
                    {
                        benevolenceRequests = benevolenceRequests.OrderBy( a => a.BenevolenceResults.Sum( b => b.Amount ) );
                    }
                }
            }
            else
            {
                benevolenceRequests = benevolenceRequests.OrderByDescending( a => a.RequestDateTime ).ThenByDescending( a => a.Id );
            }

            // Filter query by any configured attribute filters
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    benevolenceRequests = attribute.FieldType.Field.ApplyAttributeQueryFilter( benevolenceRequests, filterControl, attribute, benevolenceRequestService, Rock.Reporting.FilterMode.SimpleFilter );
                }
            }

            if ( sortProperty != null )
            {
                gList.DataSource = benevolenceRequests.Sort( sortProperty ).ToList();
            }
            else
            {
                gList.DataSource = benevolenceRequests.OrderByDescending( r => r.RequestDateTime ).ThenByDescending( r => r.Id ).ToList();
            }

            // Hide the campus column if the campus filter is not visible.
            gList.ColumnsOfType<RockBoundField>().First( c => c.DataField == "Campus.Name" ).Visible = cpCampus.Visible;

            gList.EntityTypeId = EntityTypeCache.Get<BenevolenceRequest>().Id;
            gList.DataBind();

            // Hide columns and specific fields if the hide column attributes are set on the block.
            if ( hideGridColumns?.Count() > 0 )
            {
                foreach ( DataControlField controlField in gList.Columns )
                {
                    controlField.Visible = !hideGridColumns.Contains( controlField.HeaderText.ToUpper() );
                }

                pnlSummary.Visible = !hideGridColumns.Contains( "TOTAL RESULTS" );
            }

            // Hide the campus column if the campus filter is not visible.
            gList.ColumnsOfType<RockBoundField>().First( c => c.DataField == "Campus.Name" ).Visible = cpCampus.Visible;

            // Builds the Totals section
            var definedTypeCache = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.BENEVOLENCE_RESULT_TYPE ) );
            Dictionary<string, decimal> resultTotals = new Dictionary<string, decimal>();
            decimal grandTotal = 0;
            foreach ( BenevolenceRequest request in benevolenceRequests )
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
                phSummary.Controls.Add( new LiteralControl( string.Format( "<div class='row'><div class='col-xs-8'>{0}: </div><div class='col-xs-4 text-right'>{1}</div></div>", keyValuePair.Key, keyValuePair.Value.FormatAsCurrency() ) ) );
            }

            phSummary.Controls.Add( new LiteralControl( string.Format( "<div class='row'><div class='col-xs-8'><b>Total: </div><div class='col-xs-4 text-right'>{0}</b></div></div>", grandTotal.FormatAsCurrency() ) ) );
        }

        #endregion Methods
    }
}