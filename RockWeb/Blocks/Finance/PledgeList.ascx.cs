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
    [DisplayName( "Pledge List" )]
    [Category( "Finance" )]
    [Description( "Generic list of all pledges in the system." )]

    [LinkedPage( "Detail Page", "", false )]
    [BooleanField("Show Account Column", "Allows the account column to be hidden.", true, "", 1)]
    [BooleanField("Show Last Modified Date Column", "Allows the Last Modified Date column to be hidden.", true, "", 2)]
    [BooleanField( "Show Group Column", "Allows the group column to be hidden.", false, "", 3 )]
    [BooleanField( "Limit Pledges To Current Person", "Limit the results to pledges for the current person.", false, "", 4)]
    [BooleanField( "Show Account Summary", "Should the account summary be displayed at the bottom of the list?", false, order: 5 )]
    [AccountsField( "Accounts", "Limit the results to pledges that match the selected accounts.", false, "", "", 5 )]
    [BooleanField( "Show Person Filter", "Allows person filter to be hidden.", true, "Display Filters", 0)]
    [BooleanField( "Show Account Filter", "Allows account filter to be hidden.", true, "Display Filters", 1 )]
    [BooleanField( "Show Date Range Filter", "Allows date range filter to be hidden.", true, "Display Filters", 2 )]
    [BooleanField( "Show Last Modified Filter", "Allows last modified filter to be hidden.", true, "Display Filters", 3 )]

    [ContextAware]
    public partial class PledgeList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Properties

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <value>
        /// The target person.
        /// </value>
        protected Person TargetPerson { get; private set; }

        protected List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gPledges.DataKeyNames = new[] { "id" };
            gPledges.RowDataBound += GPledges_RowDataBound;
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DetailPage" ) ) )
            {
                gPledges.Actions.AddClick += gPledges_Add;
                gPledges.RowSelected += gPledges_Edit;
            }
            gPledges.GridRebind += gPledges_GridRebind;
            gfPledges.ApplyFilterClick += gfPledges_ApplyFilterClick;
            gfPledges.DisplayFilterValue += gfPledges_DisplayFilterValue;

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPledges.Actions.ShowAdd = canAddEditDelete && !string.IsNullOrWhiteSpace( GetAttributeValue( "DetailPage" ) );
            gPledges.IsDeleteEnabled = canAddEditDelete;

            if ( GetAttributeValue( "LimitPledgesToCurrentPerson" ).AsBoolean() )
            {
                TargetPerson = this.CurrentPerson;
            }
            else
            {
                TargetPerson = ContextEntity<Person>();
            }

            // hide the person column and filter if a person context exists 
            if ( TargetPerson != null )
            {
                var personField = gPledges.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Person" );
                if ( personField != null )
                {
                    personField.Visible = false;
                }
                gfPledges.Visible = false;
            }

            var forField = gPledges.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "For" );
            if ( forField != null )
            {
                // show/hide the group column
                forField.Visible = GetAttributeValue( "ShowGroupColumn" ).AsBoolean();
            }

            var accountField = gPledges.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.HeaderText == "Account" );
            if ( accountField != null )
            {
                // show/hide the account column
                accountField.Visible = GetAttributeValue( "ShowAccountColumn" ).AsBoolean();
            }

            var modifiedDateField = gPledges.ColumnsOfType<DateField>().FirstOrDefault( a => a.DataField == "ModifiedDateTime" );
            if ( modifiedDateField != null )
            {
                // show/hide the last modified date column
                modifiedDateField.Visible = GetAttributeValue( "ShowLastModifiedDateColumn" ).AsBoolean();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( GetAttributeValue( "LimitPledgesToCurrentPerson" ).AsBoolean() && this.CurrentPerson == null )
                {
                    this.Visible = false;
                }
                else
                {

                    BindAttributes();
                    AddDynamicControls();
                    BindFilter();
                    BindGrid();
                }
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

        #endregion

        #region Events

        private void GPledges_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var pledge = ( FinancialPledge ) e.Row.DataItem;
                if ( pledge.StartDate == DateTime.MinValue )
                {
                    var cell = e.Row.Cells[4].Text = string.Empty;
                }

                if ( pledge.EndDate.ToShortDateString() == DateTime.MaxValue.ToShortDateString() )
                {
                    var cell = e.Row.Cells[5].Text = string.Empty;
                }
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            // only show the Person filter if context is not set to a specific person
            if ( TargetPerson == null && GetAttributeValue( "ShowPersonFilter" ).AsBoolean() )
            {
                ppFilterPerson.Visible = true;
                ppFilterPerson.SetValue( new PersonService( new RockContext() ).Get( gfPledges.GetUserPreference( "Person" ).AsInteger() ) );
            }
            else
            {
                ppFilterPerson.Visible = false;
            }

            // show/hide filters
            apFilterAccount.Visible = GetAttributeValue( "ShowAccountFilter" ).AsBoolean() && string.IsNullOrWhiteSpace( GetAttributeValue( "Accounts" ) );
            drpDates.Visible = GetAttributeValue( "ShowDateRangeFilter" ).AsBoolean();
            drpLastModifiedDates.Visible = GetAttributeValue( "ShowLastModifiedFilter" ).AsBoolean();

            // hide the filter dropdown if there aren't any filters
            if ( !ppFilterPerson.Visible && !apFilterAccount.Visible && !drpDates.Visible && !drpLastModifiedDates.Visible )
            {
                gfPledges.Visible = false;
            }
            else
            {
                gfPledges.Visible = true;
            }

            drpDates.DelimitedValues = gfPledges.GetUserPreference( "Date Range" );
            drpLastModifiedDates.DelimitedValues = gfPledges.GetUserPreference( "Last Modified" );
            apFilterAccount.SetValues( gfPledges.GetUserPreference( "Accounts" ).Split( ',' ).AsIntegerList() );
           
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPledges_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPledges_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PledgeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPledges_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PledgeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPledges_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var pledgeService = new FinancialPledgeService( rockContext );
            var pledge = pledgeService.Get( e.RowKeyId );
            string errorMessage;

            if ( pledge == null )
            {
                return;
            }

            if ( !pledgeService.CanDelete( pledge, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            pledgeService.Delete( pledge );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Gfs the pledges_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gfPledges_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            if ( AvailableAttributes != null )
            {
                var attribute = AvailableAttributes.FirstOrDefault( a => "Attribute_" + a.Key == e.Key );
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
                    if ( drpDates.Visible )
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                    break;

                case "Last Modified":
                    if ( drpLastModifiedDates.Visible )
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                    break;

                case "Person":
                    int? personId = e.Value.AsIntegerOrNull();
                    if ( personId != null && ppFilterPerson.Visible )
                    {
                        var person = new PersonService( new RockContext() ).Get( personId.Value );
                        if ( person != null )
                        {
                            e.Value = person.ToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                    break;

                case "Accounts":

                    var accountIdList = e.Value.Split( ',' ).AsIntegerList();
                    if ( accountIdList.Any() && apFilterAccount.Visible )
                    {
                        var service = new FinancialAccountService( new RockContext() );
                        var accounts = service.GetByIds( accountIdList );
                        if ( accounts != null && accounts.Any() )
                        {
                            e.Value = accounts.Select( a => a.Name ).ToList().AsDelimited( "," );
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfPledges_ApplyFilterClick( object sender, EventArgs e )
        {
            gfPledges.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            gfPledges.SaveUserPreference( "Last Modified", drpLastModifiedDates.DelimitedValues );
            gfPledges.SaveUserPreference( "Person", ppFilterPerson.PersonId.ToString() );
            gfPledges.SaveUserPreference( "Accounts", apFilterAccount.SelectedValues.ToList().AsDelimited(",") );
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
                            gfPledges.SaveUserPreference( "Attribute_" + attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
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
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var pledgeService = new FinancialPledgeService( rockContext );
            var sortProperty = gPledges.SortProperty;
            var pledges = pledgeService.Queryable();

            Person person = null;
            if ( TargetPerson != null )
            {
                person = TargetPerson;
            }
            else
            {
                int? personId = gfPledges.GetUserPreference( "Person" ).AsIntegerOrNull();
                if ( personId.HasValue && ppFilterPerson.Visible )
                {
                    person = new PersonService( rockContext ).Get( personId.Value );
                }
            }
            
            if ( person != null )
            {
                // if a person is specified, get pledges for that person ( and also anybody in their GivingUnit )
                pledges = pledges.Where( p => p.PersonAlias.Person.GivingId == person.GivingId );
            }

            // Filter by configured limit accounts if specified.
            var accountGuids = GetAttributeValue( "Accounts" ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                pledges = pledges.Where( p => accountGuids.Contains( p.Account.Guid ) );
            }

            // get the accounts and make sure they still exist by checking the database
            var accountIds = gfPledges.GetUserPreference( "Accounts" ).Split( ',' ).AsIntegerList();
            accountIds = new FinancialAccountService( rockContext ).GetByIds( accountIds ).Select( a => a.Id ).ToList();

            if ( accountIds.Any() && apFilterAccount.Visible )
            {
                pledges = pledges.Where( p => p.AccountId.HasValue && accountIds.Contains( p.AccountId.Value ) );
            }

            // Date Range
            DateRange filterDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( gfPledges.GetUserPreference( "Date Range" ) );
            var filterStartDate = filterDateRange.Start ?? DateTime.MinValue;
            var filterEndDate = filterDateRange.End ?? DateTime.MaxValue;

            /****
             * Include any pledges whose Start/EndDates overlap with the Filtered Date Range
             * 
             * * Pledge1 Range 1/1/2011 - 12/31/2011
             * * Pledge2 Range 1/1/0000 - 1/1/9999
             * * Pledge3 Range 6/1/2011 - 6/1/2012
             * 
             * Filter1 Range 1/1/2010 - 1/1/2013
             * * * All Pledges should show
             * Filter1 Range 1/1/2012 - 1/1/2013
             * * * Pledge2 and Pledge3 should show
             * Filter2 Range 5/1/2012 - 5/2/2012
             * * * Pledge2 and Pledge3 should show
             * Filter3 Range 5/1/2012 - 1/1/9999
             * * * Pledge2 and Pledge3 should show
             * Filter4 Range 5/1/2010 - 5/1/2010
             * * * Pledge2 should show
             ***/

            // exclude pledges that start after the filter's end date or end before the filter's start date
            if ( drpDates.Visible && ( filterDateRange.Start.HasValue || filterDateRange.End.HasValue ) )
            {
                pledges = pledges.Where( p => !(p.StartDate > filterEndDate) && !(p.EndDate < filterStartDate) );
            }

            // Filter query by any configured attribute filters
            if ( AvailableAttributes != null && AvailableAttributes.Any() )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    pledges = attribute.FieldType.Field.ApplyAttributeQueryFilter( pledges, filterControl, attribute, pledgeService, Rock.Reporting.FilterMode.SimpleFilter );
                }
            }

            // Last Modified
            DateRange filterModifiedDateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( gfPledges.GetUserPreference( "Last Modified" ) );
            var filterModifiedStartDate = filterModifiedDateRange.Start ?? DateTime.MinValue;
            var filterModifiedEndDate = filterModifiedDateRange.End ?? DateTime.MaxValue;

            if ( drpLastModifiedDates.Visible && ( filterModifiedDateRange.Start.HasValue || filterModifiedDateRange.End.HasValue ) )
            {
                pledges = pledges.Where( p => !( p.ModifiedDateTime >= filterModifiedEndDate ) && !( p.ModifiedDateTime <= filterModifiedStartDate ) );
            }

            gPledges.DataSource = sortProperty != null ? pledges.Sort( sortProperty ).ToList() : pledges.OrderBy( p => p.AccountId ).ToList();
            gPledges.DataBind();

            var showAccountSummary = this.GetAttributeValue( "ShowAccountSummary" ).AsBoolean();
            if ( showAccountSummary || TargetPerson == null )
            {
                pnlSummary.Visible = true;

                var summaryList = pledges
                                .GroupBy( a => a.AccountId )
                                .Select( a => new AccountSummaryRow
                                {
                                    AccountId = a.Key.Value,
                                    TotalAmount = a.Sum( x => x.TotalAmount ),
                                    Name = a.Where( p => p.Account != null ).Select( p => p.Account.Name ).FirstOrDefault(),
                                    Order = a.Where( p => p.Account != null ).Select( p => p.Account.Order ).FirstOrDefault()
                                } ).ToList();

                var grandTotalAmount = ( summaryList.Count > 0 ) ? summaryList.Sum( a => a.TotalAmount ) : 0;
                lGrandTotal.Text = grandTotalAmount.FormatAsCurrency();
                rptAccountSummary.DataSource = summaryList.Select( a => new { a.Name, TotalAmount = a.TotalAmount.FormatAsCurrency() } ).ToList();
                rptAccountSummary.DataBind();
            }
            else
            {
                pnlSummary.Visible = false;
            }
        }

        /// <summary>
        /// Adds filters and columns for any Pledge attributes marked as Show In Grid
        /// </summary>
        protected void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Remove attribute columns
            foreach ( var column in gPledges.Columns.OfType<AttributeField>().ToList() )
            {
                gPledges.Columns.Remove( column );
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

                        string savedValue = gfPledges.GetUserPreference( "Attribute_" + attribute.Key );
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

                    bool columnExists = gPledges.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
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

                        gPledges.Columns.Add( boundField );
                    }
                }
            }

            var deleteField = new DeleteField();
            gPledges.Columns.Add( deleteField );
            deleteField.Click += gPledges_Delete;

        }


        /// <summary>
        /// Binds the attributes
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            AvailableAttributes = new List<AttributeCache>();

            int entityTypeId = new FinancialPledge().TypeId;
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

        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        private class AccountSummaryRow
        {
            public int AccountId { get; internal set; }
            public string Name { get; internal set; }
            public int Order { get; internal set; }
            public decimal TotalAmount { get; set; }
        }
    }
}