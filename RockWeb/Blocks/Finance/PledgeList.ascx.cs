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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Pledge List" )]
    [Category( "Finance" )]
    [Description( "Generic list of all pledges in the system." )]

    [LinkedPage( "Detail Page" )]
    [BooleanField("Show Account Column", "Allows the account column to be hidden.", true, "", 1)]
    [BooleanField("Show Last Modified Date Column", "Allows the Last Modified Date column to be hidden.", true, "", 2)]
    [BooleanField( "Show Group Column", "Allows the group column to be hidden.", false, "", 3 )]
    [BooleanField( "Limit Pledges To Current Person", "Limit the results to pledges for the current person.", false, "", 4)]

    [BooleanField( "Show Person Filter", "Allows person filter to be hidden.", true, "Display Filters", 0)]
    [BooleanField( "Show Account Filter", "Allows account filter to be hidden.", true, "Display Filters", 1 )]
    [BooleanField( "Show Date Range Filter", "Allows date range filter to be hidden.", true, "Display Filters", 2 )]
    [BooleanField( "Show Last Modified Filter", "Allows last modified filter to be hidden.", true, "Display Filters", 3 )]

    [ContextAware]
    public partial class PledgeList : RockBlock, ISecondaryBlock
    {
        #region Properties

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <value>
        /// The target person.
        /// </value>
        protected Person TargetPerson { get; private set; }

        #endregion
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gPledges.DataKeyNames = new[] { "id" };
            gPledges.RowDataBound += GPledges_RowDataBound;
            gPledges.Actions.AddClick += gPledges_Add;
            gPledges.GridRebind += gPledges_GridRebind;
            gfPledges.ApplyFilterClick += gfPledges_ApplyFilterClick;
            gfPledges.DisplayFilterValue += gfPledges_DisplayFilterValue;

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPledges.Actions.ShowAdd = canAddEditDelete;
            gPledges.IsDeleteEnabled = canAddEditDelete;

            AddAttributeColumns();

            var deleteField = new DeleteField();
            gPledges.Columns.Add( deleteField );
            deleteField.Click += gPledges_Delete;

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
                gPledges.Columns[0].Visible = false;
                gfPledges.Visible = false;
            }

            // show/hide the group column
            gPledges.Columns[1].Visible = GetAttributeValue( "ShowGroupColumn" ).AsBoolean();

            // show/hide the account column
            gPledges.Columns[2].Visible = GetAttributeValue( "ShowAccountColumn" ).AsBoolean();

            // show/hide the last modified date column
            gPledges.Columns[7].Visible = GetAttributeValue( "ShowLastModifiedDateColumn" ).AsBoolean();
        }

        private void GPledges_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var pledge = (FinancialPledge)e.Row.DataItem;
                if (pledge.StartDate == DateTime.MinValue )
                {
                    var cell = e.Row.Cells[4].Text = string.Empty;
                }

                if ( pledge.EndDate.ToShortDateString() == DateTime.MaxValue.ToShortDateString() )
                {
                    var cell = e.Row.Cells[5].Text = string.Empty;
                }
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
                    BindFilter();
                    BindGrid();
                }
            }

            base.OnLoad( e );
        }

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
            apFilterAccount.Visible = GetAttributeValue( "ShowAccountFilter" ).AsBoolean();
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
            NavigateToLinkedPage( "DetailPage", "pledgeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPledges_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "pledgeId", e.RowKeyId );
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

            // get the accounts and make sure they still exist by checking the database
            var accountIds = gfPledges.GetUserPreference( "Accounts" ).Split( ',' ).AsIntegerList();
            accountIds = new FinancialAccountService( rockContext ).GetByIds( accountIds ).Select( a => a.Id ).ToList();

            if ( accountIds.Any() && apFilterAccount.Visible )
            {
                pledges = pledges.Where( p => p.AccountId.HasValue && accountIds.Contains( p.AccountId.Value ) );
            }

            // Date Range
            var drp = new DateRangePicker();
            drp.DelimitedValues = gfPledges.GetUserPreference( "Date Range" );
            var filterStartDate = drp.LowerValue ?? DateTime.MinValue;
            var filterEndDate = drp.UpperValue ?? DateTime.MaxValue;

            if (filterEndDate != DateTime.MaxValue)
            {
                filterEndDate = filterEndDate.AddDays( 1 );
            }

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
            if ( drpDates.Visible )
            {
                pledges = pledges.Where( p => !(p.StartDate > filterEndDate) && !(p.EndDate < filterStartDate) );
            }

            // Last Modified
            drp.DelimitedValues = gfPledges.GetUserPreference( "Last Modified" );
            var filterModifedStartDate = drp.LowerValue ?? DateTime.MinValue;
            var filterModifiedEndDate = drp.UpperValue ?? DateTime.MaxValue;

            if ( filterModifiedEndDate != DateTime.MaxValue)
            {
                filterModifiedEndDate = filterModifiedEndDate.AddDays( 1 );
            }

            if ( drpLastModifiedDates.Visible )
            {
                pledges = pledges.Where( p => !(p.ModifiedDateTime >= filterModifiedEndDate ) && !(p.ModifiedDateTime <= filterModifedStartDate ) );
            }

            gPledges.DataSource = sortProperty != null ? pledges.Sort( sortProperty ).ToList() : pledges.OrderBy( p => p.AccountId ).ToList();
            gPledges.DataBind();
        }

        /// <summary>
        /// Adds columns for any Pledge attributes marked as Show In Grid
        /// </summary>
        protected void AddAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gPledges.Columns.OfType<AttributeField>().ToList() )
            {
                gPledges.Columns.Remove( column );
            }

            int entityTypeId = new FinancialPledge().TypeId;
            foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn
                   )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = attribute.Key;
                bool columnExists = gPledges.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.AttributeId = attribute.Id;
                    boundField.HeaderText = attribute.Name;

                    var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                    if ( attributeCache != null )
                    {
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                    }

                    gPledges.Columns.Add( boundField );
                }
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
    }
}