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
using System.ComponentModel;
using System.Linq;

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
    public partial class PledgeList : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gPledges.DataKeyNames = new[] { "id" };
            gPledges.Actions.AddClick += gPledges_Add;
            gPledges.GridRebind += gPledges_GridRebind;
            gfPledges.ApplyFilterClick += gfPledges_ApplyFilterClick;
            gfPledges.DisplayFilterValue += gfPledges_DisplayFilterValue;

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPledges.Actions.ShowAdd = canAddEditDelete;
            gPledges.IsDeleteEnabled = canAddEditDelete;
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

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            drpDates.DelimitedValues = gfPledges.GetUserPreference( "Date Range" );
            apFilterAccount.SetValues( gfPledges.GetUserPreference( "Accounts" ).Split(',').AsIntegerList());
            ppFilterPerson.SetValue( new PersonService( new RockContext() ).Get( gfPledges.GetUserPreference( "Person" ).AsInteger() ) );
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
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Person":
                    int? personId = e.Value.AsIntegerOrNull();
                    if ( personId != null )
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
                    if ( accountIdList.Any() )
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

            int? personId = gfPledges.GetUserPreference("Person").AsIntegerOrNull();

            if ( personId.HasValue )
            {
                var person = new PersonService( rockContext ).Get( personId.Value );
                if ( person != null )
                {
                    pledges = pledges.Where( p => p.PersonAlias.PersonId == person.Id );
                }
            }

            // get the accounts and make sure they still exist by checking the database
            var accountIds = gfPledges.GetUserPreference( "Accounts" ).Split( ',' ).AsIntegerList();
            accountIds = new FinancialAccountService( rockContext ).GetByIds( accountIds ).Select( a => a.Id ).ToList();

            if ( accountIds.Any() )
            {
                pledges = pledges.Where( p => p.AccountId.HasValue && accountIds.Contains( p.AccountId.Value ) );
            }

            // Date Range
            var drp = new DateRangePicker();
            drp.DelimitedValues = gfPledges.GetUserPreference( "Date Range" );
            var filterStartDate = drp.LowerValue ?? DateTime.MinValue;
            var filterEndDate = drp.UpperValue ?? DateTime.MaxValue;
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
            pledges = pledges.Where( p => !( p.StartDate > filterEndDate ) && !( p.EndDate < filterStartDate ) );

            gPledges.DataSource = sortProperty != null ? pledges.Sort( sortProperty ).ToList() : pledges.OrderBy( p => p.AccountId ).ToList();
            gPledges.DataBind();
        }
    }
}