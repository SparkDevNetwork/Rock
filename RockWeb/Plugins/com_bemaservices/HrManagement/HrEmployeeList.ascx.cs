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
    [DisplayName( "HR Employee List" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Lists all the employees along with their PTO information." )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON
        , "Person Hired Date Attribute"
        , "The Person Attribute that contains the Person's Hired Date.  This will be used to determine if the person is currently staff or not."
        , true
        , false
        , ""
        , ""
        , 0
        , "HireDate" )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON
        , "Person Fired Date Attribute"
        , "The Person Attribute that contains the Person's Fired Date.  This will be used to determine if the person is currently staff or not."
        , true
        , false
        , ""
        , ""
        , 1
        , "FireDate" )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON
        , "Person Supervisor Attribute"
        , "The Person Attribute that contains the Person's Supervisor."
        , true
        , false
        , ""
        , ""
        , 2
        , "Supervisor" )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON
        , "Person Ministry Area Attribute"
        , "The Person Attribute that contains the Person's Ministry Area."
        , true
        , false
        , ""
        , ""
        , 3
        , "MinistryArea" )]
    [LinkedPage( "Detail Page", order: 4 )]

    public partial class HrEmployeeList : RockBlock, ICustomGridColumns
    {

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

            gfEmployeeFilter.ApplyFilterClick += gfEmployeeFilter_ApplyFilterClick;
            gfEmployeeFilter.ClearFilterClick += gfEmployeeFilter_ClearFilterClick;
            gfEmployeeFilter.DisplayFilterValue += gfEmployeeFilter_DisplayFilterValue;

            gEmployeeList.DataKeyNames = new string[] { "Id" };
            gEmployeeList.Actions.ShowAdd = false;
            gEmployeeList.GridRebind += gEmployeeList_GridRebind;
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfEmployeeFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfEmployeeFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfEmployeeFilter.DeleteUserPreferences();
            BindFilter();
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
        /// Handles the DisplayFilterValue event of the gfEmployeeFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs"/> instance containing the event data.</param>
        protected void gfEmployeeFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
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

                case "Supervisor":
                    {
                        var personAliasId = e.Value.AsIntegerOrNull();
                        if ( personAliasId.HasValue )
                        {
                            e.Value = ppSupervisor.PersonName;
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
        /// Handles the ApplyFilterClick event of the gfEmployeeFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfEmployeeFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfEmployeeFilter.SaveUserPreference( "Fiscal Year End", ddlFiscalYearEnd.SelectedValue );
            gfEmployeeFilter.SaveUserPreference( "Pto Type", ddlPtoType.SelectedValue );
            gfEmployeeFilter.SaveUserPreference( "Supervisor", ppSupervisor.PersonId.ToString() );
            gfEmployeeFilter.SaveUserPreference( "Ministry Area", tbMinistryArea.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gEmployeeList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gEmployeeList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "PersonId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gEmployeeList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gEmployeeList_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        protected void gEmployeeList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var person = e.Row.DataItem as Person;
                if ( person != null )
                {
                    var rockContext = new RockContext();
                    var ptoAllocationService = new PtoAllocationService( rockContext );
                    var ptoTypeService = new PtoTypeService( rockContext );
                    var personAliasService = new PersonAliasService( rockContext );

                    var lName = e.Row.FindControl( "lName" ) as Literal;
                    var lSupervisor = e.Row.FindControl( "lSupervisor" ) as Literal;
                    var lMinistryArea = e.Row.FindControl( "lMinistryArea" ) as Literal;
                    var lAllocation = e.Row.FindControl( "lAllocation" ) as Literal;
                    var lTotalAccrued = e.Row.FindControl( "lTotalAccrued" ) as Literal;
                    var lTotalTaken = e.Row.FindControl( "lTotalTaken" ) as Literal;
                    var lRemaining = e.Row.FindControl( "lRemaining" ) as Literal;

                    var ptoTypes = ptoTypeService.Queryable().AsNoTracking().Where( pto => pto.IsActive == true );

                    // filter by Pto Type
                    var ptoTypeId = gfEmployeeFilter.GetUserPreference( "Pto Type" ).AsIntegerOrNull();
                    if ( ptoTypeId.HasValue && ptoTypeId != -1 )
                    {
                        ptoTypes = ptoTypes.Where( p => p.Id == ptoTypeId.Value );
                    }

                    var ptoTypeIds = ptoTypes.Select( pto => pto.Id ).ToList();

                    // Get Allocations
                    var allocationQry = ptoAllocationService
                                        .Queryable()
                                        .AsNoTracking()
                                        .Where( a => ( !ptoTypeIds.Any() || ptoTypeIds.Contains( a.PtoTypeId ) ) &&
                                                    a.PersonAlias.PersonId == person.Id &&
                                                    a.PtoAllocationStatus == PtoAllocationStatus.Active );

                    // Filter by fiscal year
                    var fiscalYearEnd = gfEmployeeFilter.GetUserPreference( "Fiscal Year End" ).AsIntegerOrNull();
                    if ( fiscalYearEnd.HasValue )
                    {
                        //Get the currect fiscal start date to set the allocations dates.
                        var fiscalStartDateValue = GlobalAttributesCache.Value( AttributeCache.Get( com.bemaservices.HrManagement.SystemGuid.Attribute.FISCAL_YEAR_START_DATE_ATTRIBUTE ).Key );
                        var fiscalStartDate = DateTime.Parse( fiscalStartDateValue );

                        var fiscalStartMonth = fiscalStartDate.Month;
                        var fiscalStartDay = fiscalStartDate.Day;
                        var calculatedFiscalStartDate = new DateTime( fiscalYearEnd.Value - 1, fiscalStartMonth, fiscalStartDay );
                        var calculatedFiscalEndDate = calculatedFiscalStartDate.AddYears( 1 ).AddDays( -1 );

                        allocationQry = allocationQry.Where( a =>
                            ( a.StartDate >= calculatedFiscalStartDate && a.StartDate <= calculatedFiscalEndDate ) ||
                            ( a.EndDate.HasValue && a.EndDate >= calculatedFiscalStartDate && a.EndDate <= calculatedFiscalEndDate ) ||
                            ( !a.EndDate.HasValue && a.StartDate <= calculatedFiscalEndDate )
                            );
                    }


                    var allocations = allocationQry.OrderByDescending( b => b.StartDate ).ThenByDescending( b => b.EndDate ).ToList();
                    var allocationPtoTypes = allocations.GroupBy( a => a.PtoTypeId );

                    var allocationItems = allocations.Select( a => string.Format( "{0}: {1} - {2} ({3} hrs)", a.PtoType.Name, a.StartDate.ToShortDateString(), a.EndDate.HasValue ? a.EndDate.Value.ToShortDateString() : "N/A", a.Hours ) ).ToList();
                    var accruedItems = new List<string>();
                    var takenItems = new List<string>();
                    var remainingItems = new List<string>();

                    foreach ( var ptoType in ptoTypes.ToList() )
                    {
                        var name = ptoType.Name;
                        decimal accruedHours = 0;
                        decimal takenHours = 0;
                        decimal remainingHours = 0;

                        var allocationPtoType = allocationPtoTypes.Where( a => a.Key == ptoType.Id ).FirstOrDefault();
                        if ( allocationPtoType != null )
                        {
                            accruedHours = allocationPtoType.Sum( pt => pt.Hours );
                            takenHours = allocationPtoType.Sum( pt => pt.PtoRequests.Sum( pr => pr.Hours ) );
                            remainingHours = accruedHours - takenHours;
                        }

                        var isLimitedToAllocations = gfEmployeeFilter.GetUserPreference( "LimitToAllocations" ).AsBoolean();
                        if ( !isLimitedToAllocations || accruedHours > 0 || takenHours > 0 )
                        {
                            accruedItems.Add( String.Format( "{0}: {1}", name, accruedHours ) );
                            takenItems.Add( String.Format( "{0}: {1}", name, takenHours ) );
                            remainingItems.Add( String.Format( "{0}: <span style='color:{1};'>{2}</span>", name, remainingHours < 0 ? "red" : "", remainingHours ) );
                        }
                    }

                    person.LoadAttributes();
                    lName.Text = person.FullNameReversed;

                    var supervisorAttributeGuid = GetAttributeValue( "Supervisor" ).AsGuidOrNull();
                    if ( lSupervisor != null && supervisorAttributeGuid.HasValue )
                    {
                        var supervisorAttribute = AttributeCache.Get( supervisorAttributeGuid.Value );
                        if ( supervisorAttribute != null )
                        {
                            var supervisorAliasGuid = person.GetAttributeValue( supervisorAttribute.Key ).AsGuid();
                            var supervisorAlias = personAliasService.Get( supervisorAliasGuid );
                            if ( supervisorAlias != null )
                            {
                                lSupervisor.Text = supervisorAlias.Person.FullNameReversed;
                            }
                        }
                    }

                    var ministryAreaAttributeGuid = GetAttributeValue( "MinistryArea" ).AsGuidOrNull();
                    if ( lMinistryArea != null && ministryAreaAttributeGuid.HasValue )
                    {
                        var ministryAreaAttribute = AttributeCache.Get( ministryAreaAttributeGuid.Value );
                        if ( ministryAreaAttribute != null )
                        {
                            lMinistryArea.Text = person.GetAttributeValue( ministryAreaAttribute.Key );
                        }
                    }

                    lAllocation.Text = allocationItems.AsDelimited( "<br/>" );
                    lTotalAccrued.Text = accruedItems.AsDelimited( "<br/>" );
                    lTotalTaken.Text = takenItems.AsDelimited( "<br/>" );
                    lRemaining.Text = remainingItems.AsDelimited( "<br/>" );
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
            var ptoTypes = new PtoTypeService( new RockContext() ).Queryable().AsNoTracking().Where( x => x.IsActive == true ).ToList();

            ddlPtoType.DataSource = ptoTypes;
            ddlPtoType.SetValue( gfEmployeeFilter.GetUserPreference( "Pto Type" ) );
            ddlPtoType.DataBind();
            ddlPtoType.Items.Insert( 0, Rock.Constants.All.ListItem );

            ddlFiscalYearEnd.Items.Clear();
            for ( int yearOffset = -5; yearOffset <= 5; yearOffset++ )
            {
                ddlFiscalYearEnd.Items.Add( new ListItem( RockDateTime.Now.AddYears( yearOffset ).Year.ToString() ) );
            }
            ddlFiscalYearEnd.Items.Insert( 0, Rock.Constants.None.ListItem );
            ddlFiscalYearEnd.SetValue( gfEmployeeFilter.GetUserPreference( "Fiscal Year End" ) );

            var ministryAreaAttributeGuid = GetAttributeValue( "MinistryArea" ).AsGuidOrNull();
            if ( ministryAreaAttributeGuid.HasValue )
            {
                tbMinistryArea.Text = gfEmployeeFilter.GetUserPreference( "Ministry Area" );
            }
            else
            {
                tbMinistryArea.Visible = false;
            }

            var supervisorAttributeGuid = GetAttributeValue( "Supervisor" ).AsGuidOrNull();
            if ( supervisorAttributeGuid.HasValue )
            {
                var personId = gfEmployeeFilter.GetUserPreference( "Supervisor" ).AsIntegerOrNull();
                ppSupervisor.PersonId = personId;
                if ( personId.HasValue )
                {
                    ppSupervisor.PersonName = new PersonService( new RockContext() ).Get( personId.Value ).FullName;
                }
            }
            else
            {
                ppSupervisor.Visible = false;
            }


        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( bool isExporting = false )
        {
            var rockContext = new RockContext();

            var personService = new PersonService( rockContext );
            var ptoAllocationService = new PtoAllocationService( rockContext );
            var ptoTypeService = new PtoTypeService( rockContext );

            rockContext.Database.CommandTimeout = 90;

            DateTime todayOffset = RockDateTime.Now;
            //Get the currect fiscal start date to set the allocations dates.
            var fiscalStartDateValue = GlobalAttributesCache.Value( AttributeCache.Get( com.bemaservices.HrManagement.SystemGuid.Attribute.FISCAL_YEAR_START_DATE_ATTRIBUTE ).Key );
            var fiscalStartDate = DateTime.Parse( fiscalStartDateValue );

            var fiscalStartMonth = fiscalStartDate.Month;
            var fiscalStartDay = fiscalStartDate.Day;
            var fiscalStartYear = todayOffset.AddMonths( ( fiscalStartMonth - 1 ) * -1 ).AddDays( ( fiscalStartDay - 1 ) * -1 ).Year;
            var calculatedFiscalStartDate = new DateTime( fiscalStartYear, fiscalStartMonth, fiscalStartDay );
            var calculatedFiscalEndDate = calculatedFiscalStartDate.AddYears( 1 ).AddDays( -1 );

            // Filter by fiscal year
            var fiscalYearEnd = gfEmployeeFilter.GetUserPreference( "Fiscal Year End" ).AsIntegerOrNull();
            if ( fiscalYearEnd.HasValue )
            {
                calculatedFiscalStartDate = new DateTime( fiscalYearEnd.Value - 1, fiscalStartMonth, fiscalStartDay );
                calculatedFiscalEndDate = calculatedFiscalStartDate.AddYears( 1 ).AddDays( -1 );
            }

            var hireDateAttributeGuid = GetAttributeValue( "HireDate" ).AsGuidOrNull();
            var fireDateAttributeGuid = GetAttributeValue( "FireDate" ).AsGuidOrNull();
            var supervisorAttributeGuid = GetAttributeValue( "Supervisor" ).AsGuidOrNull();
            var ministryAreaAttributeGuid = GetAttributeValue( "MinistryArea" ).AsGuidOrNull();

            //Get Employees during specified year
            var hiredPeople = new List<int>();
            var firedPeople = new List<int>();

            if ( hireDateAttributeGuid.HasValue )
            {
                hiredPeople = personService.Queryable().AsNoTracking()
                               .WhereAttributeValue( rockContext, x => x.Attribute.Guid == hireDateAttributeGuid.Value && x.ValueAsDateTime <= calculatedFiscalEndDate )
                               .Select( p => p.Id )
                               .ToList();
            }

            if ( fireDateAttributeGuid.HasValue )
            {
                firedPeople = personService.Queryable().AsNoTracking()
                               .WhereAttributeValue( rockContext, x => x.Attribute.Guid == fireDateAttributeGuid.Value && x.ValueAsDateTime <= calculatedFiscalStartDate )
                               .Select( p => p.Id )
                               .ToList();
            }

            var qry = personService.Queryable().AsNoTracking();

            if ( hiredPeople.Any() )
            {
                qry = qry.Where( p => hiredPeople.Contains( p.Id ) );
            }

            if ( firedPeople.Any() )
            {
                qry = qry.Where( p => !firedPeople.Contains( p.Id ) );
            }

            // filter by supervisor
            if ( supervisorAttributeGuid.HasValue )
            {
                var personId = gfEmployeeFilter.GetUserPreference( "Supervisor" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    qry = qry.WhereAttributeValue( rockContext, x => x.Attribute.Guid == supervisorAttributeGuid.Value && x.ValueAsPersonId == personId.Value );
                }
            }
            else
            {
                var supervisorCol = gEmployeeList.ColumnsOfType<RockLiteralField>().FirstOrDefault( c => c.ID == "lSupervisor" );
                if ( supervisorCol != null )
                {
                    supervisorCol.Visible = false;
                }
            }

            // filter by ministry area
            if ( ministryAreaAttributeGuid.HasValue )
            {
                var ministryArea = gfEmployeeFilter.GetUserPreference( "Ministry Area" );
                if ( ministryArea.IsNotNullOrWhiteSpace() )
                {
                    qry = qry.WhereAttributeValue( rockContext, x => x.Attribute.Guid == ministryAreaAttributeGuid.Value && x.Value == ministryArea );
                }
            }
            else
            {
                var ministryAreaCol = gEmployeeList.ColumnsOfType<RockLiteralField>().FirstOrDefault( c => c.ID == "lMinistryArea" );
                if ( ministryAreaCol != null )
                {
                    ministryAreaCol.Visible = false;
                }
            }

            IOrderedQueryable<Person> sortedQry = null;

            SortProperty sortProperty = gEmployeeList.SortProperty;
            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "Name" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        sortedQry = qry
                            .OrderBy( b => b.LastName )
                            .ThenBy( p => p.FirstName );
                    }
                    else
                    {
                        sortedQry = qry
                            .OrderByDescending( b => b.LastName )
                            .ThenByDescending( p => p.FirstName );
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
                    .OrderBy( b => b.LastName )
                    .ThenBy( p => p.FirstName );
            }

            gEmployeeList.ObjectList = sortedQry.ToList().ToDictionary( k => k.Id.ToString(), v => v as object );
            gEmployeeList.SetLinqDataSource( sortedQry.AsNoTracking() );
            gEmployeeList.EntityTypeId = EntityTypeCache.Get<Person>().Id;
            gEmployeeList.DataBind();
        }

        #endregion
    }
}
