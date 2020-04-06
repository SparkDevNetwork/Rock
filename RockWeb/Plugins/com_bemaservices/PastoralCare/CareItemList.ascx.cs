// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using com.bemaservices.PastoralCare.Model;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemaservices.PastoralCare
{
    /// <summary>
    /// Block to display the care items that user is authorized to view, and the items that are currently assigned to the user.
    /// </summary>
    [DisplayName( "Care Item List" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "Block to display the care items." )]

    [ContextAware]
    [LinkedPage( "Configuration Page", "Page used to modify and create connection careTypes.", false, "", "", 0 )]
    [LinkedPage( "Detail Page", "Page used to view details of an careItems.", true, "", "", 1 )]

    public partial class CareItemList : Rock.Web.UI.RockBlock, ICustomGridColumns
    {
        #region Fields
        private const string SELECTED_TYPE_SETTING = "MyCareTypes_SelectedType";
        DateTime _midnightToday = RockDateTime.Today.AddDays( 1 );
        // cache the DeleteField and ColumnIndex since it could get called many times in GridRowDataBound
        private DeleteField _deleteField = null;
        private int? _deleteFieldColumnIndex = null;
        private Person _person = null;


        #endregion

        #region Properties
        public List<AttributeCache> AvailableAttributes { get; set; }
        protected int? SelectedTypeId { get; set; }
        protected List<TypeSummary> SummaryState { get; set; }
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
            SelectedTypeId = ViewState["SelectedTypeId"] as int?;
            SummaryState = ViewState["SummaryState"] as List<TypeSummary>;

            AddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbCareTypes.Visible = UserCanAdministrate;

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            gItems.DataKeyNames = new string[] { "Id" };
            gItems.Actions.AddClick += gItems_Add;
            gItems.GridRebind += gItems_GridRebind;
            gItems.ShowConfirmDeleteDialog = true;
            gItems.PersonIdField = "PersonId";

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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

            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                SelectedTypeId = GetUserPreference( SELECTED_TYPE_SETTING ).AsIntegerOrNull();

                // Reset the state filter on every initial request to be Active and Past Due future follow up
                rFilter.SaveUserPreference( "State", "State", "0;-2" );

                GetSummaryData();

                RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.visible.min.js" ) );
            }
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
            ViewState["SelectedTypeId"] = SelectedTypeId;
            ViewState["SummaryState"] = SummaryState;

            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GetSummaryData();
        }

        #endregion

        #region Events

        #region Summary Panel Events

        /// <summary>
        /// Handles the Click event of the lbCareTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCareTypes_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ConfigurationPage" );
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptCareTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCareTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            string selectedTypeValue = e.CommandArgument.ToString();
            SetUserPreference( SELECTED_TYPE_SETTING, selectedTypeValue );

            SelectedTypeId = selectedTypeValue.AsIntegerOrNull();

            BindAttributes();
            AddDynamicControls();

            BindSummaryData();

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "ScrollToGrid",
                "scrollToGrid();",
                true );
        }

        #endregion

        #region Request Grid/Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "DateRange", "Date Range", sdrpDateRange.DelimitedValues );
            int? personId = ppPerson.PersonId;
            rFilter.SaveUserPreference( "Person", "Person", personId.HasValue ? personId.Value.ToString() : string.Empty );

            personId = ppContactor.PersonId;
            rFilter.SaveUserPreference( "Contactor", "Contactor", personId.HasValue ? personId.Value.ToString() : string.Empty );
            rFilter.SaveUserPreference( "Status", "Status", ddlStatus.SelectedValue );

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
                    else
                    {
                        // no filter control, so clear out the user preference
                        rFilter.SaveUserPreference( attribute.Key, attribute.Name, null );
                    }
                }
            }
            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( AvailableAttributes != null )
                {
                    var attribute = AvailableAttributes.FirstOrDefault( a => a.Key == e.Key );
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

                if ( e.Key == "Person" )
                {
                    string personName = string.Empty;
                    int? personId = e.Value.AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            personName = person.FullName;
                        }
                    }
                    e.Value = personName;
                }
                else if ( e.Key == "Contactor" )
                {
                    string personName = string.Empty;
                    int? personId = e.Value.AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            personName = person.FullName;
                        }
                    }
                    e.Value = personName;
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
        }

        /// <summary>
        /// Handles the Add event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gItems_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "CareItemId", 0, "CareTypeId", SelectedTypeId );
        }

        /// <summary>
        /// Handles the Edit event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Edit( object sender, RowEventArgs e )
        {
            int? careItemId = null;
            using ( RockContext rockContext = new RockContext() )
            {
                var service = new CareTypeItemService( rockContext );
                var careTypeItem = service.Get( e.RowKeyId );
                if ( careTypeItem != null )
                {
                    careItemId = careTypeItem.CareItemId;
                }
            }

            if ( careItemId != null )
            {
                NavigateToLinkedPage( "DetailPage", "CareItemId", careItemId.Value, "CareTypeId", SelectedTypeId );
            }
        }

        protected void gItems_Delete( object sender, RowEventArgs e )
        {
            int? careItemId = null;
            using ( RockContext rockContext = new RockContext() )
            {
                var service = new CareTypeItemService( rockContext );
                var careTypeItem = service.Get( e.RowKeyId );
                if ( careTypeItem != null )
                {
                    careItemId = careTypeItem.CareItemId;
                }
            }

            if ( careItemId.HasValue )
            {
                using ( RockContext rockContext = new RockContext() )
                {
                    var service = new CareItemService( rockContext );
                    var contactService = new CareContactService( rockContext );
                    var careItem = service.Get( careItemId.Value );
                    if ( careItem != null )
                    {
                        string errorMessage;
                        if ( !service.CanDelete( careItem, out errorMessage ) )
                        {
                            mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                            return;
                        }

                        rockContext.WrapTransaction( () =>
                        {
                            contactService.DeleteRange( careItem.CareContacts );
                            service.Delete( careItem );
                            rockContext.SaveChanges();
                        } );
                    }
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gItems_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gItems_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                CareTypeItem careTypeItem = e.Row.DataItem as CareTypeItem;

                if ( careTypeItem != null )
                {
                    int careItemId = careTypeItem.CareItem.Id;

                    if ( _deleteField != null && _deleteField.Visible )
                    {
                        LinkButton deleteButton = null;
                        HtmlGenericControl buttonIcon = null;

                        if ( !_deleteFieldColumnIndex.HasValue )
                        {
                            _deleteFieldColumnIndex = gItems.GetColumnIndex( gItems.Columns.OfType<DeleteField>().First() );
                        }

                        if ( _deleteFieldColumnIndex.HasValue && _deleteFieldColumnIndex > -1 )
                        {
                            deleteButton = e.Row.Cells[_deleteFieldColumnIndex.Value].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                        }

                        if ( deleteButton != null )
                        {
                            buttonIcon = deleteButton.ControlsOfTypeRecursive<HtmlGenericControl>().FirstOrDefault();
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets the summary data.
        /// </summary>
        private void GetSummaryData()
        {
            SummaryState = new List<TypeSummary>();

            var rockContext = new RockContext();
            var careTypes = new CareTypeService( rockContext )
                .Queryable().AsNoTracking();

            careTypes = careTypes.Where( t => t.IsActive );

            // Loop through careTypes
            foreach ( var careType in careTypes )
            {
                // Check to see if person can edit the careType because of edit rights to this block or edit rights to
                // the careType
                bool canEdit = UserCanEdit || careType.IsAuthorized( Authorization.EDIT, CurrentPerson );

                var canView = canEdit || careType.IsAuthorized( Authorization.VIEW, CurrentPerson );

                // Is user is authorized to view this careType type...
                if ( canView )
                {
                    // Check if the careType's type has been added to summary yet, and if not, add it
                    var typeSummary = SummaryState.Where( c => c.Id == careType.Id ).FirstOrDefault();
                    if ( typeSummary == null )
                    {
                        // Add the careType
                        typeSummary = new TypeSummary
                        {
                            Id = careType.Id,
                            Name = careType.Name,
                            IsActive = careType.IsActive,
                            CanEdit = canEdit
                        };

                        SummaryState.Add( typeSummary );
                    }
                }
            }

            BindSummaryData();
        }

        /// <summary>
        /// Binds the summary data.
        /// </summary>
        private void BindSummaryData()
        {
            if ( SummaryState == null )
            {
                GetSummaryData();
            }

            var viewableTypeIds = SummaryState
                .Select( t => t.Id )
                .ToList();

            // Make sure that the selected careType is actually one that is being displayed
            if ( SelectedTypeId.HasValue && !viewableTypeIds.Contains( SelectedTypeId.Value ) )
            {
                SelectedTypeId = null;
            }

            pnlNoTypes.Visible = !viewableTypeIds.Any();

            rptCareTypes.DataSource = SummaryState.Where( t => viewableTypeIds.Contains( t.Id ) );
            rptCareTypes.DataBind();

            if ( SelectedTypeId.HasValue )
            {
                SetFilter();
                BindGrid();
                pnlGrid.Visible = true;
            }
            else
            {
                pnlGrid.Visible = false;
            }

        }

        /// <summary>
        /// Sets the filter.
        /// </summary>
        private void SetFilter()
        {
            using ( var rockContext = new RockContext() )
            {
                sdrpDateRange.DelimitedValues = rFilter.GetUserPreference( "DateRange" );
                var personService = new PersonService( rockContext );
                int? personId = rFilter.GetUserPreference( "Person" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    ppPerson.SetValue( personService.Get( personId.Value ) );
                }

                rFilter.AdditionalFilterDisplay.Clear();

                ppContactor.Visible = true;
                personId = rFilter.GetUserPreference( "Contactor" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    ppContactor.SetValue( personService.Get( personId.Value ) );
                }
                ddlStatus.SetValue( rFilter.GetUserPreference( "Status" ) );

            }

            BindAttributes();
            AddDynamicControls();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var configurationValue = GetAttributeValue( "ConfigurationPage" );
            lbCareTypes.Visible = configurationValue.IsNotNullOrWhiteSpace();

            // If configured for a person and person is null, return
            int personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && _person == null )
            {
                return;
            }

            TypeSummary typeSummary = null;

            if ( SelectedTypeId.HasValue && SummaryState != null )
            {
                typeSummary = SummaryState.Where( t => t.Id == SelectedTypeId.Value ).FirstOrDefault();
            }

            if ( typeSummary != null )
            {
                gItems.Actions.ShowAdd = typeSummary.CanEdit;
                gItems.IsDeleteEnabled = typeSummary.CanEdit;
                gItems.ColumnsOfType<DeleteField>().First().Visible = typeSummary.CanEdit;

                using ( var rockContext = new RockContext() )
                {
                    var careTypeItemService = new CareTypeItemService( rockContext );

                    // Get queryable of all careItems that belong to the selected careType
                    var careTypeItems = careTypeItemService
                        .Queryable().AsNoTracking()
                        .Where( r =>
                            r.CareTypeId == SelectedTypeId.Value );

                    // Filter by Status
                    string statusFilter = ddlStatus.SelectedValue;
                    if ( statusFilter == "Active" )
                    {
                        careTypeItems = careTypeItems
                            .Where( i => i.CareItem.IsActive );
                    }
                    else if ( statusFilter == "Inactive" )
                    {
                        careTypeItems = careTypeItems
                            .Where( i => !i.CareItem.IsActive );
                    }

                    // Filter by  Date.
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDateRange.DelimitedValues );
                    if ( dateRange.Start.HasValue )
                    {
                        careTypeItems = careTypeItems.Where( i => i.CareItem.ContactDateTime >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        careTypeItems = careTypeItems.Where( i => i.CareItem.ContactDateTime <= dateRange.End.Value );
                    }

                    // only communications for the selected recipient (_person)
                    if ( _person != null )
                    {
                        careTypeItems = careTypeItems
                            .Where( i =>
                                i.CareItem.PersonAlias.PersonId == _person.Id );
                    }

                    // Filter by Person
                    if ( ppPerson.PersonId.HasValue )
                    {
                        careTypeItems = careTypeItems
                            .Where( r =>
                                r.CareItem.PersonAlias != null &&
                                r.CareItem.PersonAlias.PersonId == ppPerson.PersonId.Value );
                    }

                    // Filter by Contactor
                    if ( ppContactor.PersonId.HasValue )
                    {
                        careTypeItems = careTypeItems
                            .Where( i =>
                                i.CareItem.ContactorPersonAlias != null &&
                                i.CareItem.ContactorPersonAlias.PersonId == ppContactor.PersonId.Value );
                    }

                    // Filter query by any configured attribute filters
                    if ( AvailableAttributes != null && AvailableAttributes.Any() )
                    {
                        foreach ( var attribute in AvailableAttributes )
                        {
                            var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                            careTypeItems = attribute.FieldType.Field.ApplyAttributeQueryFilter( careTypeItems, filterControl, attribute, careTypeItemService, Rock.Reporting.FilterMode.SimpleFilter );
                        }
                    }

                    SortProperty sortProperty = gItems.SortProperty;
                    if ( sortProperty != null && sortProperty.Property != "LastContactDate" )
                    {
                        careTypeItems = careTypeItems.Sort( sortProperty );
                    }
                    else
                    {
                        careTypeItems = careTypeItems
                            .OrderByDescending( i => i.CareItem.ContactDateTime )
                            .ThenBy( i => i.CareItem.PersonAlias.Person.LastName )
                            .ThenBy( i => i.CareItem.PersonAlias.Person.NickName );
                    }

                    var itemList = careTypeItems.ToList();

                    gItems.ObjectList = new Dictionary<string, object>();
                    itemList.ForEach( m => gItems.ObjectList.Add( m.Id.ToString(), m ) );
                    gItems.EntityTypeId = EntityTypeCache.Get( "72352815-30F3-46FB-86C0-69AC284D9ED2".AsGuid() ).Id;

                    var datasource = itemList
                   .Select( r => new
                   {
                       r.Id,
                       r.Guid,
                       CareItemId = r.CareItem.Id,
                       CareItemGuid = r.CareItem.Guid,
                       PersonId = r.CareItem.PersonAlias.PersonId,
                       Name = r.CareItem.PersonAlias.Person.FullNameReversed,
                       ContactorPersonId = r.CareItem.ContactorPersonAlias.PersonId,
                       ContactorName = r.CareItem.ContactorPersonAlias.Person.FullNameReversed ?? "",
                       ContactDateTime = r.CareItem.ContactDateTime.ToShortDateTimeString() ?? "",
                       LastContactDate = r.CareItem.CareContacts.OrderByDescending( c => c.ContactDateTime ).FirstOrDefault() != null ? r.CareItem.CareContacts.OrderByDescending( c => c.ContactDateTime ).FirstOrDefault().ContactDateTime.ToShortDateTimeString() : "",
                       LastContactor = r.CareItem.CareContacts.OrderByDescending( c => c.ContactDateTime ).FirstOrDefault() != null ? r.CareItem.CareContacts.OrderByDescending( c => c.ContactDateTime ).FirstOrDefault().ContactorPersonAlias.Person.FullNameReversed : "",
                       LastContactNote = r.CareItem.CareContacts.OrderByDescending( c => c.ContactDateTime ).FirstOrDefault() != null ? r.CareItem.CareContacts.OrderByDescending( c => c.ContactDateTime ).FirstOrDefault().Description : ""
                   } )
                  .ToList();

                    if ( sortProperty != null && sortProperty.Property == "LastContactDate" )
                    {
                        if ( sortProperty.Direction == SortDirection.Descending )
                        {
                            datasource = datasource.OrderByDescending( a => a.LastContactDate ).ToList();
                        }
                        else
                        {
                            datasource = datasource.OrderBy( a => a.LastContactDate ).ToList();
                        }
                    }

                    gItems.DataSource = datasource;
                    gItems.DataBind();

                    lCareItem.Text = String.Format( "{0} Care Items", typeSummary.Name );
                }
            }
            else
            {
                pnlGrid.Visible = false;
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Clear dynamic controls so we can re-add them
            RemoveAttributeColumns();

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

                        string savedValue = rFilter.GetUserPreference( attribute.Key );
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

                    bool columnExists = gItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.ExcelExportBehavior = ExcelExportBehavior.IncludeIfVisible;
                        boundField.HeaderText = attribute.Name;
                        boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;

                        gItems.Columns.Add( boundField );
                    }
                }
            }

            // Add delete column
            _deleteField = new DeleteField();
            _deleteField.Click += gItems_Delete;
            gItems.Columns.Add( _deleteField );
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            AvailableAttributes = new List<AttributeCache>();
            var rockContext = new RockContext();

            // Parse the attribute filters 
            if ( SelectedTypeId != null )
            {
                int entityTypeId = new CareTypeItem().TypeId;
                string groupTypeQualifier = SelectedTypeId.ToString();

                foreach ( var attribute in new AttributeService( rockContext ).GetByEntityTypeQualifier( entityTypeId, "", "", true )
                .Where( a => a.IsGridColumn )
                .OrderByDescending( a => a.EntityTypeQualifierColumn )
                .ThenBy( a => a.Order )
                .ThenBy( a => a.Name ).ToAttributeCacheList() )
                {
                    if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        AvailableAttributes.Add( attribute );
                    }
                }

                foreach ( var attribute in new AttributeService( rockContext ).GetByEntityTypeQualifier( entityTypeId, "CareTypeId", groupTypeQualifier, true )
                    .Where( a => a.IsGridColumn )
                    .OrderByDescending( a => a.EntityTypeQualifierColumn )
                    .ThenBy( a => a.Order )
                    .ThenBy( a => a.Name ).ToAttributeCacheList() )
                {
                    if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        AvailableAttributes.Add( attribute );
                    }
                }
            }
        }

        private void RemoveAttributeColumns()
        {
            // Remove added button columns
            DataControlField buttonColumn = gItems.Columns.OfType<DeleteField>().FirstOrDefault( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gItems.Columns.Remove( buttonColumn );
            }

            // Remove attribute columns
            foreach ( var column in gItems.Columns.OfType<AttributeField>().ToList() )
            {
                gItems.Columns.Remove( column );
            }
        }
        #endregion

        #region Helper Classes

        [Serializable]
        public class TypeSummary
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public bool CanEdit { get; set; }
        }

        #endregion
    }
}