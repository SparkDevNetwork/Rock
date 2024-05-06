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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Lists scheduled transactions either for current person (Person Detail Page) or all scheduled transactions.
    /// </summary>
    [DisplayName( "Scheduled Transaction List" )]
    [Category( "Finance" )]
    [Description( "Lists scheduled transactions either for current person (Person Detail Page) or all scheduled transactions." )]

    [LinkedPage( "View Page",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.ViewPage )]

    [LinkedPage( "Add Page",
        DefaultValue = "",
        IsRequired = false,
        Key = AttributeKey.AddPage )]

    [AccountsField( "Accounts",
        Description = "Limit the results to scheduled transactions that match the selected accounts.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.Accounts )]

    [IntegerField( "Person Token Expire Minutes",
        Description = "When adding a new scheduled transaction from a person detail page, the number of minutes the person token for the transaction is valid after it is issued.",
        IsRequired = true,
        DefaultIntegerValue = 60,
        Order = 3,
        Key = AttributeKey.PersonTokenExpireMinutes )]

    [IntegerField( "Person Token Usage Limit",
        Description = "When adding a new scheduled transaction from a person detail page, the maximum number of times the person token for the transaction can be used.",
        IsRequired = false,
        DefaultIntegerValue = 1,
        Order = 4,
        Key = AttributeKey.PersonTokenUsageLimit )]

    [BooleanField( "Show Transaction Type Column",
        Description = "Show the Transaction Type column.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Order = 5,
        Key = AttributeKey.ShowTransactionTypeColumn )]

    [ContextAware]
    [Rock.SystemGuid.BlockTypeGuid( "694FF260-8C6F-4A59-93C9-CF3793FE30E6" )]
    public partial class ScheduledTransactionList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The view page
            /// </summary>
            public const string ViewPage = "ViewPage";
            /// <summary>
            /// The add page
            /// </summary>
            public const string AddPage = "AddPage";
            /// <summary>
            /// The accounts
            /// </summary>
            public const string Accounts = "Accounts";
            /// <summary>
            /// The person token expire minutes
            /// </summary>
            public const string PersonTokenExpireMinutes = "PersonTokenExpireMinutes";
            /// <summary>
            /// The person token usage limit
            /// </summary>
            public const string PersonTokenUsageLimit = "PersonTokenUsageLimit";
            /// <summary>
            /// The show transaction type column attribute key
            /// </summary>
            public const string ShowTransactionTypeColumn = "ShowTransactionTypeColumn";
        }

        #endregion Keys

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
            gList.Actions.ShowAdd = canEdit && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.AddPage ) );
            gList.Actions.AddClick += gList_Add;

            gList.IsDeleteEnabled = canEdit;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.ViewPage ) ) )
            {
                gList.RowSelected += gList_Edit;
            }

            gList.GridRebind += gList_GridRebind;
            gList.RowDataBound += gList_RowDataBound;
            TargetPerson = ContextEntity<Person>();
        }

        private void gList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var txn = e.Row.DataItem as FinancialScheduledTransaction;

                if ( txn != null )
                {
                    var lTotalAmount = e.Row.FindControl( "lTotalAmount" ) as Literal;

                    if ( lTotalAmount != null )
                    {
                        lTotalAmount.Text = txn.TotalAmount.FormatAsCurrency( txn.ForeignCurrencyCodeValueId );
                    }
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
            gfSettings.SetFilterPreference( "Amount", nreAmount.DelimitedValues );
            gfSettings.SetFilterPreference( "Frequency", dvpFrequency.SelectedValue != All.Id.ToString() ? dvpFrequency.SelectedValue : string.Empty );
            gfSettings.SetFilterPreference( "Created", drpDates.DelimitedValues );
            gfSettings.SetFilterPreference( "Account", ddlAccount.SelectedValue != All.Id.ToString() ? ddlAccount.SelectedValue : string.Empty );
            gfSettings.SetFilterPreference( "Include Inactive", cbIncludeInactive.Checked ? "Yes" : string.Empty );
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
                        var definedValue = DefinedValueCache.Get( definedValueId );
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
                    if ( int.TryParse( e.Value, out accountId ) && ddlAccount.Visible )
                    {
                        var account = FinancialAccountCache.Get( accountId );
                        if ( account != null )
                        {
                            e.Value = account.Name;
                        }
                    }
                    else
                    {
                        e.Value = string.Empty;
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
            NavigateToLinkedPage( AttributeKey.ViewPage, parms );
        }

        /// <summary>
        /// Handles the Add event of the gList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gList_Add( object sender, EventArgs e )
        {
            var addScheduledTransactionPage = new Rock.Web.PageReference( GetAttributeValue( AttributeKey.AddPage ) );
            if ( addScheduledTransactionPage != null )
            {
                if ( !this.TargetPerson.IsPersonTokenUsageAllowed() )
                {
                    mdWarningAlert.Show( $"Due to their protection profile level you cannot add a transaction on behalf of this person.", ModalAlertType.Warning );
                    return;
                }

                // create a limited-use personkey that will last long enough for them to go thru all the 'postbacks' while posting a transaction
                var personKey = this.TargetPerson.GetImpersonationToken(
                    RockDateTime.Now.AddMinutes( this.GetAttributeValue( AttributeKey.PersonTokenExpireMinutes ).AsIntegerOrNull() ?? 60 ), this.GetAttributeValue( AttributeKey.PersonTokenUsageLimit ).AsIntegerOrNull(), addScheduledTransactionPage.PageId );

                if ( personKey.IsNotNullOrWhiteSpace() )
                {
                    addScheduledTransactionPage.QueryString["Person"] = personKey;
                    Response.Redirect( addScheduledTransactionPage.BuildUrl() );
                }
            }
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
            nreAmount.DelimitedValues = gfSettings.GetFilterPreference( "Amount" );

            dvpFrequency.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY.AsGuid() ).Id;
            dvpFrequency.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            string freqPreference = gfSettings.GetFilterPreference( "Frequency" );
            if ( !string.IsNullOrWhiteSpace( freqPreference ) )
            {
                dvpFrequency.SetValue( freqPreference );
            }

            drpDates.DelimitedValues = gfSettings.GetFilterPreference( "Created" );

            var accountService = new FinancialAccountService( new RockContext() );
            var accounts = accountService
                .Queryable().AsNoTracking()
                .Where( a => a.IsActive );

            ddlAccount.Visible = string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.Accounts ) );
            ddlAccount.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( FinancialAccount account in accounts.OrderBy( a => a.Order ) )
            {
                ListItem li = new ListItem( account.Name, account.Id.ToString() );
                li.Selected = account.Id.ToString() == gfSettings.GetFilterPreference( "Account" );
                ddlAccount.Items.Add( li );
            }

            cbIncludeInactive.Checked = !string.IsNullOrWhiteSpace( gfSettings.GetFilterPreference( "Include Inactive" ) );
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
                int personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
                if ( !ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) )
                {
                    validRequest = true;
                }
            }

            if ( validRequest )
            {
                var rockContext = new RockContext();
                var qry = new FinancialScheduledTransactionService( rockContext )
                    .Queryable()
                    .Include( t => t.ScheduledTransactionDetails )
                    .Include( t => t.FinancialPaymentDetail.CurrencyTypeValue )
                    .Include( t => t.FinancialPaymentDetail.CreditCardTypeValue );

                // Show/hide the "Transaction Type" column.
                var dvfTransactionTypeValue = gList.ColumnsOfType<DefinedValueField>().FirstOrDefault( c => c.DataField == "TransactionTypeValueId" );
                if ( GetAttributeValue( AttributeKey.ShowTransactionTypeColumn ).AsBoolean() )
                {
                    // Include the TransactionTypeValue when the column should be shown.
                    qry = qry.Include( t => t.TransactionTypeValue );

                    if ( dvfTransactionTypeValue != null )
                    {
                        // Show the column.
                        dvfTransactionTypeValue.Visible = true;
                    }
                }
                else
                {
                    if ( dvfTransactionTypeValue != null )
                    {
                        // Hide the column.
                        dvfTransactionTypeValue.Visible = false;
                    }
                }

                qry = qry.AsNoTracking();

                // Valid Accounts
                var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
                if ( accountGuids.Any() )
                {
                    qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => accountGuids.Contains( d.Account.Guid ) ) );
                }

                // Amount Range
                var nre = new NumberRangeEditor();
                nre.DelimitedValues = gfSettings.GetFilterPreference( "Amount" );
                if ( nre.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.ScheduledTransactionDetails.Sum( d => d.Amount ) >= nre.LowerValue.Value );
                }

                if ( nre.UpperValue.HasValue )
                {
                    qry = qry.Where( t => t.ScheduledTransactionDetails.Sum( d => d.Amount ) <= nre.UpperValue.Value );
                }

                // Frequency
                int? frequencyTypeId = gfSettings.GetFilterPreference( "Frequency" ).AsIntegerOrNull();
                if ( frequencyTypeId.HasValue )
                {
                    qry = qry.Where( t => t.TransactionFrequencyValueId == frequencyTypeId.Value );
                }

                // Date Range
                var drp = new DateRangePicker();
                drp.DelimitedValues = gfSettings.GetFilterPreference( "Created" );
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
                if ( int.TryParse( gfSettings.GetFilterPreference( "Account" ), out accountId ) && ddlAccount.Visible )
                {
                    qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => d.AccountId == accountId ) );
                }

                // Active only (no filter)
                if ( string.IsNullOrWhiteSpace( gfSettings.GetFilterPreference( "Include Inactive" ) ) )
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
                            qry = qry.OrderBy( t => t.ScheduledTransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.00M );
                        }
                        else
                        {
                            qry = qry.OrderByDescending( t => t.ScheduledTransactionDetails.Sum( d => ( decimal? ) d.Amount ) ?? 0.0M );
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
                var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
                var summary = txn.ScheduledTransactionDetails
                    .Select( d => new
                    {
                        IsOther = accountGuids.Any() && !accountGuids.Contains( d.Account.Guid ),
                        Order = d.Account.Order,
                        Name = d.Account.Name,
                        Amount = d.Amount
                    } )
                    .OrderBy( d => d.IsOther )
                    .ThenBy( d => d.Order )
                    .Select( d => string.Format( "{0}: {1}",
                        !d.IsOther ? d.Name : "Other",
                        d.Amount.FormatAsCurrency( txn.ForeignCurrencyCodeValueId ) ) )
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
            if ( !this.TargetPerson.IsPersonTokenUsageAllowed() )
            {
                mdWarningAlert.Show( $"Due to their protection profile level you cannot add a transaction on behalf of this person.", ModalAlertType.Warning );
                return;
            }

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