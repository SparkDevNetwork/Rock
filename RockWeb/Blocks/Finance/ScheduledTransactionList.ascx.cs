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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Collections.Generic;
using Rock.Security;

namespace RockWeb.Blocks.Finance 
{
    /// <summary>
    /// Lists scheduled transactions either for current person (Person Detail Page) or all scheduled transactions.
    /// </summary>
    [DisplayName( "Scheduled Transaction List" )]
    [Category( "Finance" )]
    [Description( "Lists scheduled transactions either for current person (Person Detail Page) or all scheduled transactions." )]

    [LinkedPage( "View Page" )]
    [LinkedPage( "Add Page" )]
    [ContextAware]
    public partial class ScheduledTransactionList : Rock.Web.UI.RockBlock, ISecondaryBlock
    {
        private bool _isExporting = false;

        #region Properties

        /// <summary>
        /// Gets the target person.
        /// </summary>
        /// <value>
        /// The target person.
        /// </value>
        protected Person TargetPerson { get; private set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            gList.DataKeyNames = new string[] { "Id" };
            gList.Actions.ShowAdd = canEdit && !string.IsNullOrWhiteSpace( GetAttributeValue( "AddPage" ) );
            gList.IsDeleteEnabled = canEdit;

            gList.Actions.AddClick += gList_Add;
            gList.GridRebind += gList_GridRebind;

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
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Amount", nreAmount.DelimitedValues );
            gfSettings.SaveUserPreference( "Frequency", ddlFrequency.SelectedValue != All.Id.ToString() ? ddlFrequency.SelectedValue : string.Empty );
            gfSettings.SaveUserPreference( "Created", drpDates.DelimitedValues );
            gfSettings.SaveUserPreference( "Account", ddlAccount.SelectedValue != All.Id.ToString() ? ddlAccount.SelectedValue : string.Empty );
            gfSettings.SaveUserPreference( "Include Inactive", cbIncludeInactive.Checked ? "Yes" : string.Empty );
            BindGrid();
        }

        /// <summary>
        /// Gfs the settings_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfSettings_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Amount":
                    e.Value = NumberRangeEditor.FormatDelimitedValues( e.Value, "N2" );
                    break;

                case "Frequency":
                    int definedValueId = 0;
                    if ( int.TryParse( e.Value, out definedValueId ) )
                    {
                        var definedValue = DefinedValueCache.Read( definedValueId );
                        if ( definedValue != null )
                        {
                            e.Value = definedValue.Value;
                        }
                    }

                    break;

                case "Created":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Account":

                    int accountId = 0;
                    if ( int.TryParse( e.Value, out accountId ) )
                    {
                        var service = new FinancialAccountService( new RockContext() );
                        var account = service.Get( accountId );
                        if ( account != null )
                        {
                            e.Value = account.Name;
                        }
                    }

                    break;

                case "Include Inactive":
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gList_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ScheduledTransactionId", e.RowKeyId.ToString() );
            NavigateToLinkedPage( "ViewPage", parms );
        }

        /// <summary>
        /// Handles the Add event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "Person", TargetPerson.UrlEncodedKey );
            NavigateToLinkedPage( "AddPage", parms );
        }

        /// <summary>
        /// Handles the GridRebind event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gList_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            nreAmount.DelimitedValues = gfSettings.GetUserPreference( "Amount" );

            ddlFrequency.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY.AsGuid() ) );
            ddlFrequency.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            string freqPreference = gfSettings.GetUserPreference( "Frequency" );
            if ( !string.IsNullOrWhiteSpace( freqPreference ))
            {
                ddlFrequency.SetValue( freqPreference );
            }

            drpDates.DelimitedValues = gfSettings.GetUserPreference( "Created" );

            var accountService = new FinancialAccountService( new RockContext() );
            var accounts = accountService
                .Queryable().AsNoTracking()
                .Where( a => a.IsActive );

            ddlAccount.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( FinancialAccount account in accounts.OrderBy( a => a.Order ) )
            {
                ListItem li = new ListItem( account.Name, account.Id.ToString() );
                li.Selected = account.Id.ToString() == gfSettings.GetUserPreference( "Account" );
                ddlAccount.Items.Add( li );
            }

            cbIncludeInactive.Checked = !string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Include Inactive" ) );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( bool isExporting = false )
        {
            int? personId = null;
            int? givingGroupId = null;

            bool validRequest = false;

            if ( TargetPerson != null )
            {
                personId = TargetPerson.Id;
                givingGroupId = TargetPerson.GivingGroupId;
                validRequest = true;
            }
            else
            {
                int personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                if ( !ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) )
                {
                    validRequest = true;
                }
            }

            if ( validRequest )
            {
                var rockContext = new RockContext();
                var qry = new FinancialScheduledTransactionService( rockContext )
                    .Queryable( "ScheduledTransactionDetails,FinancialPaymentDetail.CurrencyTypeValue,FinancialPaymentDetail.CreditCardTypeValue" )
                    .AsNoTracking();

                // Amount Range
                var nre = new NumberRangeEditor();
                nre.DelimitedValues = gfSettings.GetUserPreference( "Amount" );
                if ( nre.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.ScheduledTransactionDetails.Sum( d => d.Amount ) >= nre.LowerValue.Value );
                }

                if ( nre.UpperValue.HasValue )
                {
                    qry = qry.Where( t => t.ScheduledTransactionDetails.Sum( d => d.Amount ) <= nre.UpperValue.Value );
                }

                // Frequency
                int? frequencyTypeId = gfSettings.GetUserPreference( "Frequency" ).AsIntegerOrNull();
                if ( frequencyTypeId.HasValue )
                {
                    qry = qry.Where( t => t.TransactionFrequencyValueId == frequencyTypeId.Value );
                }

                // Date Range
                var drp = new DateRangePicker();
                drp.DelimitedValues = gfSettings.GetUserPreference( "Created" );
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.CreatedDateTime >= drp.LowerValue.Value );
                }

                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.CreatedDateTime < upperDate );
                }

                // Account Id
                int accountId = int.MinValue;
                if ( int.TryParse( gfSettings.GetUserPreference( "Account" ), out accountId ) )
                {
                    qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => d.AccountId == accountId ) );
                }

                // Active only (no filter)
                if ( string.IsNullOrWhiteSpace( gfSettings.GetUserPreference( "Include Inactive" ) ) )
                {
                    qry = qry.Where( t => t.IsActive );
                }

                if ( givingGroupId.HasValue )
                {
                    //  Person contributes with family
                    qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingGroupId == givingGroupId );
                }
                else if ( personId.HasValue )
                {
                    // Person contributes individually
                    qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == personId );
                }

                SortProperty sortProperty = gList.SortProperty;
                if ( sortProperty != null )
                {
                    if ( sortProperty.Property == "Amount" )
                    {
                        if ( sortProperty.Direction == SortDirection.Ascending )
                        {
                            qry = qry.OrderBy( t => t.ScheduledTransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.00M );
                        }
                        else
                        {
                            qry = qry.OrderByDescending( t => t.ScheduledTransactionDetails.Sum( d => (decimal?)d.Amount ) ?? 0.0M );
                        }
                    }
                    else
                    {
                        qry = qry.Sort( sortProperty );
                    }
                }
                else
                {
                    qry = qry
                        .OrderBy( t => t.AuthorizedPersonAlias.Person.LastName )
                        .ThenBy( t => t.AuthorizedPersonAlias.Person.NickName )
                        .ThenByDescending( t => t.IsActive )
                        .ThenByDescending( t => t.StartDate );
                }

                _isExporting = isExporting;

                gList.SetLinqDataSource<FinancialScheduledTransaction>( qry );
                gList.DataBind();

                _isExporting = false;
            }
        }

        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        protected string GetAccounts( object dataItem )
        {
            var txn = dataItem as FinancialScheduledTransaction;
            if ( txn != null )
            {
                var summary  = txn.ScheduledTransactionDetails
                    .OrderBy( d => d.Account.Order )
                    .Select( d => string.Format( "{0}: {1}", d.Account.Name, d.Amount.FormatAsCurrency() ) )
                    .ToList();
                if ( summary.Any() )
                {
                    if ( _isExporting )
                    {
                        return summary.AsDelimited( Environment.NewLine );
                    }
                    else
                    {
                        return "<small>" + summary.AsDelimited( "<br/>" ) + "</small>";
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "ScheduledTransactionId", id.ToString() );
            parms.Add( "Person", TargetPerson.UrlEncodedKey );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}
