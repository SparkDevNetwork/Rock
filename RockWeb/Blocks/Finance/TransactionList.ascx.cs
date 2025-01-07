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
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Utility;
using System.Web.Util;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Transaction List
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    /// <seealso cref="Rock.Web.UI.ISecondaryBlock" />
    /// <seealso cref="System.Web.UI.IPostBackEventHandler" />
    /// <seealso cref="Rock.Web.UI.ICustomGridColumns" />
    [DisplayName( "Transaction List" )]
    [Category( "Finance" )]
    [Description( "Builds a list of all financial transactions which can be filtered by date, account, transaction type, etc." )]

    [ContextAware]
    [SecurityAction( "FilterByPerson", "The roles and/or users that can filter transactions by person." )]

    [LinkedPage( "Detail Page",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.DetailPage )]

    [TextField( "Title",
        Description = "Title to display above the grid. Leave blank to hide.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.Title )]

    [BooleanField( "Show Only Active Accounts on Filter",
        Description = "If account filter is displayed, only list active accounts",
        DefaultBooleanValue = false,
        Order = 2,
        Key = AttributeKey.ActiveAccountsOnlyFilter )]

    [BooleanField( "Show Options",
        Description = "Show an Options button in the title panel for showing images or summary.",
        DefaultBooleanValue = false,
        Order = 3,
        Key = AttributeKey.ShowOptions )]

    [IntegerField( "Image Height",
        Description = "If the Show Images option is selected, the image height",
        IsRequired = false,
        DefaultIntegerValue = 200,
        Order = 4,
        Key = AttributeKey.ImageHeight )]

    [DefinedValueField( "Transaction Types",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        Description = "Optional list of transaction types to limit the list to (if none are selected all types will be included).",
        IsRequired = false,
        AllowMultiple = true,
        DefaultValue = "",
        Order = 5,
        Key = AttributeKey.TransactionTypes )]

    [CustomDropdownListField( "Default Transaction View",
        Description = "Select whether you want to initially see Transactions or Transaction Details",
        ListSource = "Transactions,Transaction Details",
        IsRequired = false,
        DefaultValue = "Transactions",
        Order = 6,
        Key = AttributeKey.DefaultTransactionView )]

    [LinkedPage( "Batch Page",
        IsRequired = false,
        Order = 7,
        Key = AttributeKey.BatchPage )]

    [BooleanField( "Show Foreign Key",
        Description = "Should the transaction foreign key column be displayed?",
        DefaultBooleanValue = false,
        Order = 8,
        Key = AttributeKey.ShowForeignKey )]

    [BooleanField( "Show Account Summary",
        Description = "Should the account summary be displayed at the bottom of the list?",
        DefaultBooleanValue = false,
        Order = 9,
        Key = AttributeKey.ShowAccountSummary )]

    [AccountsField( "Accounts",
        Description = "Limit the results to transactions that match the selected accounts.",
        IsRequired = false,
        DefaultValue = "",
        Order = 10,
        Key = AttributeKey.Accounts )]

    [BooleanField( "Show Future Transactions",
        Description = "Should future transactions (transactions scheduled to be charged) be shown in this list?",
        DefaultBooleanValue = false,
        Order = 10,
        Key = AttributeKey.ShowFutureTransactions )]

    [DefinedValueField( "Source Types",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        Description = "Optional list of financial source types to limit the list to (if none are selected all types will be included).",
        IsRequired = false,
        AllowMultiple = true,
        DefaultValue = "",
        Order = 11,
        Key = AttributeKey.SourceTypes )]

    [BooleanField( "Enable Foreign Currency",
        Description = "Shows the transaction's currency code field if enabled.",
        DefaultBooleanValue = false,
        Order = 12,
        Key = AttributeKey.EnableForeignCurrency )]

    [BooleanField( "Show Days Since Last Transaction",
        Description = "Show the number of days between the transaction and the transaction listed next to the transaction",
        DefaultBooleanValue = false,
        Order = 12,
        Key = AttributeKey.ShowDaysSinceLastTransaction
        )]

    [BooleanField( "Hide Transactions in Pending Batches",
        Description = "When enabled, transactions in a batch whose status is 'Pending' will be filtered out from the list.",
        DefaultBooleanValue = false,
        Order = 13,
        Key = AttributeKey.HideTransactionsInPendingBatches
        )]

    [Rock.SystemGuid.BlockTypeGuid( "E04320BC-67C3-452D-9EF6-D74D8C177154" )]
    public partial class TransactionList : Rock.Web.UI.RockBlock, ISecondaryBlock, IPostBackEventHandler, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The detail page
            /// </summary>
            public const string DetailPage = "DetailPage";

            /// <summary>
            /// The title
            /// </summary>
            public const string Title = "Title";

            /// <summary>
            /// The active accounts only filter
            /// </summary>
            public const string ActiveAccountsOnlyFilter = "ActiveAccountsOnlyFilter";

            /// <summary>
            /// The show options
            /// </summary>
            public const string ShowOptions = "ShowOptions";

            /// <summary>
            /// The image height
            /// </summary>
            public const string ImageHeight = "ImageHeight";

            /// <summary>
            /// The transaction types
            /// </summary>
            public const string TransactionTypes = "TransactionTypes";

            /// <summary>
            /// The default transaction view
            /// </summary>
            public const string DefaultTransactionView = "DefaultTransactionView";

            /// <summary>
            /// The batch page
            /// </summary>
            public const string BatchPage = "BatchPage";

            /// <summary>
            /// The show foreign key
            /// </summary>
            public const string ShowForeignKey = "ShowForeignKey";

            /// <summary>
            /// The show account summary
            /// </summary>
            public const string ShowAccountSummary = "ShowAccountSummary";

            /// <summary>
            /// The accounts
            /// </summary>
            public const string Accounts = "Accounts";

            /// <summary>
            /// The show future transactions
            /// </summary>
            public const string ShowFutureTransactions = "ShowFutureTransactions";

            /// <summary>
            /// The source types
            /// </summary>
            public const string SourceTypes = "SourceTypes";

            /// <summary>
            /// The enable foreign currency
            /// </summary>
            public const string EnableForeignCurrency = "EnableForeignCurrency";

            /// <summary>
            /// The show days since last transaction
            /// </summary>
            public const string ShowDaysSinceLastTransaction = "ShowDaysSinceLastTransaction";

            /// <summary>
            /// The hide transactions in pending batches
            /// </summary>
            public const string HideTransactionsInPendingBatches = "HideTransactionsInPendingBatches";
        }

        #endregion Keys

        #region Fields

        private bool _canEdit = false;
        private int _imageHeight = 200;
        private FinancialBatch _batch = null;
        private Person _person = null;
        private FinancialScheduledTransaction _scheduledTxn = null;
        private Registration _registration = null;

        private RockDropDownList _ddlMove = new RockDropDownList();
        private LinkButton _lbReassign = new LinkButton();

        public List<AttributeCache> _availableAttributes { get; set; }

        // Dictionaries to cache values for databinding performance
        private Dictionary<int, string> _currencyTypes;
        private Dictionary<int, string> _creditCardTypes;
        private List<PersonDetail> _personDetails;
        private Dictionary<int, List<int>> _imageBinaryFileIdLookupByTransactionId;

        private string _batchPageRoute = null;

        #endregion Fields

        /// <summary>
        /// Gets or sets a value indicating whether [show delete button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show delete button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowDeleteButton
        {
            get
            {
                return ViewState["ShowDeleteButton"] as bool? ?? false;
            }

            set
            {
                ViewState["ShowDeleteButton"] = value;
            }
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( hfTransactionViewMode.Value.IsNullOrWhiteSpace() )
            {
                var preferences = GetBlockPersonPreferences();
                var userViewMode = preferences.GetValue( "TransactionViewMode" );
                var defaultViewMode = this.GetAttributeValue( AttributeKey.DefaultTransactionView );
                hfTransactionViewMode.Value = userViewMode.IsNullOrWhiteSpace() ? defaultViewMode : userViewMode;
            }

            gfTransactions.ApplyFilterClick += gfTransactions_ApplyFilterClick;
            gfTransactions.ClearFilterClick += gfTransactions_ClearFilterClick;
            gfTransactions.DisplayFilterValue += gfTransactions_DisplayFilterValue;

            SetBlockOptions();

            _canEdit = UserCanEdit;

            gTransactions.DataKeyNames = new string[] { "Id" };
            gTransactions.Actions.ShowAdd = _canEdit && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.DetailPage ) );
            gTransactions.Actions.AddClick += gTransactions_Add;
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.DetailPage ) ) )
            {
                gTransactions.RowSelected += gTransactions_Edit;
            }
            gTransactions.GridRebind += gTransactions_GridRebind;
            gTransactions.RowDataBound += gTransactions_RowDataBound;
            gTransactions.IsDeleteEnabled = _canEdit;

            this._batchPageRoute = LinkedPageRoute( "BatchPage" );

            int currentBatchId = PageParameter( "BatchId" ).AsInteger();

            if ( _canEdit )
            {
                _ddlMove.ID = "ddlMove";
                _ddlMove.CssClass = "pull-left input-width-xl input-xs";
                _ddlMove.DataValueField = "Id";
                _ddlMove.DataTextField = "Name";
                _ddlMove.DataSource = new FinancialBatchService( new RockContext() )
                    .Queryable()
                    .Where( b =>
                        b.Status == BatchStatus.Open &&
                        b.BatchStartDateTime.HasValue &&
                        b.Id != currentBatchId )
                    .OrderBy( b => b.Id )
                    .Select( b => new
                    {
                        b.Id,
                        b.Name,
                        b.BatchStartDateTime
                    } )
                    .ToList()
                    .Select( b => new
                    {
                        b.Id,
                        Name = string.Format( "#{0} {1} ({2})", b.Id, b.Name, b.BatchStartDateTime.Value.ToString( "d" ) )
                    } )
                    .ToList();
                _ddlMove.DataBind();
                _ddlMove.Items.Insert( 0, new ListItem( "-- Move Transactions To Batch --", "" ) );
                gTransactions.Actions.AddCustomActionControl( _ddlMove );

                _lbReassign.ID = "lbReassign";
                _lbReassign.CssClass = "btn btn-default btn-sm btn-grid-custom-action pull-left";
                _lbReassign.Click += _lbReassign_Click;
                _lbReassign.Text = "Reassign Transactions";
                gTransactions.Actions.AddCustomActionControl( _lbReassign );
            }

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upTransactions );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _availableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
        }

        /// <summary>
        /// Sets the block options.
        /// </summary>
        private void SetBlockOptions()
        {
            string title = GetAttributeValue( AttributeKey.Title );
            if ( string.IsNullOrWhiteSpace( title ) )
            {
                title = "Transaction List";
            }

            bddlOptions.Visible = GetAttributeValue( AttributeKey.ShowOptions ).AsBooleanOrNull() ?? false;
            _imageHeight = GetAttributeValue( AttributeKey.ImageHeight ).AsIntegerOrNull() ?? 200;

            lTitle.Text = title;
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfTransactions_ClearFilterClick( object sender, EventArgs e )
        {
            gfTransactions.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbClosedWarning.Visible = false;
            nbResult.Visible = false;

            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is Person )
                {
                    _person = contextEntity as Person;
                }
                else if ( contextEntity is FinancialBatch )
                {
                    _batch = contextEntity as FinancialBatch;
                    gfTransactions.Visible = false;
                }
                else if ( contextEntity is FinancialScheduledTransaction )
                {
                    _scheduledTxn = contextEntity as FinancialScheduledTransaction;
                    gfTransactions.Visible = false;
                }
                else if ( contextEntity is Registration )
                {
                    _registration = contextEntity as Registration;
                    gfTransactions.Visible = false;
                }

            }

            SetupGridActionControls();

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
            else
            {
                ShowDialog();
            }

            if ( _canEdit && _batch != null )
            {
                string script = string.Format( @"
    $('#{0}').on('change', function( e ){{
        var count = $(""#{1} input[id$='_cbSelect_0']:checked"").length;
        if (count == 0) {{
            $('#{3}').val($ddl.val());
            window.location = ""javascript:{2}"";
        }}
        else
        {{
            var $ddl = $(this);
            if ($ddl.val() != '') {{
                Rock.dialogs.confirm('Are you sure you want to move the selected transactions to a new batch (the control amounts on each batch will be updated to reflect the moved transaction\'s amounts)?', function (result) {{
                    if (result) {{
                        $('#{3}').val($ddl.val());
                        window.location = ""javascript:{2}"";
                    }}
                    $ddl.val('');
                }});
            }}
        }}
    }});
", _ddlMove.ClientID, gTransactions.ClientID, Page.ClientScript.GetPostBackEventReference( this, "MoveTransactions" ), hfMoveToBatchId.ClientID );
                ScriptManager.RegisterStartupScript( _ddlMove, _ddlMove.GetType(), "moveTransaction", script, true );
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributes"] = _availableAttributes;

            return base.SaveViewState();
        }

        /// <summary>
        /// Setups the grid action controls.
        /// </summary>
        private void SetupGridActionControls()
        {
            bool showSelectColumn = false;

            this.ShowDeleteButton = true;

            // Set up the selection filter
            if ( _canEdit && _batch != null )
            {
                if ( _batch.Status == BatchStatus.Closed )
                {
                    nbClosedWarning.Visible = true;
                    _ddlMove.Visible = false;
                }
                else
                {
                    nbClosedWarning.Visible = false;
                    showSelectColumn = true;
                    _ddlMove.Visible = true;
                }

                // If the batch is closed, do not allow any editing of the transactions
                // NOTE that gTransactions_Delete click will also check if the transaction is part of a closed batch
                if ( _batch.Status != BatchStatus.Closed && _canEdit )
                {
                    gTransactions.Actions.ShowAdd = _canEdit;
                    gTransactions.IsDeleteEnabled = _canEdit;
                }
                else
                {
                    gTransactions.Actions.ShowAdd = false;
                    gTransactions.IsDeleteEnabled = false;
                }
            }
            else
            {
                nbClosedWarning.Visible = false;
                _ddlMove.Visible = false;

                // not in batch mode, so don't allow Add, and don't show the DeleteButton
                gTransactions.Actions.ShowAdd = false;
                this.ShowDeleteButton = false;
            }

            if ( _canEdit && _person != null )
            {
                showSelectColumn = true;
                _lbReassign.Visible = true;
            }
            else
            {
                _lbReassign.Visible = false;
            }

            var selectColumn = gTransactions.ColumnsOfType<SelectField>().FirstOrDefault();
            if ( selectColumn != null )
            {
                selectColumn.Visible = showSelectColumn;
            }

            // don't show the BatchId column if we are in the context of a single Batch
            var batchIdColumn = gTransactions.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lBatchId" ).FirstOrDefault();
            if ( batchIdColumn != null )
            {
                batchIdColumn.Visible = _batch == null;
            }

            var foreignCurrencySymbolColumn = gTransactions.ColumnsOfType<RockLiteralField>().Where( a => a.ID == "lForeignCurrencySymbol" ).FirstOrDefault();
            if ( foreignCurrencySymbolColumn != null )
            {
                foreignCurrencySymbolColumn.Visible = GetAttributeValue( AttributeKey.EnableForeignCurrency ).AsBoolean();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetBlockOptions();
            SetupGridActionControls();
            BindGrid();
        }

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gfTransactions_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            if ( _availableAttributes != null )
            {
                var attribute = _availableAttributes.FirstOrDefault( a => a.Key == e.Key );
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
                case "Row Limit":
                    // row limit filter was removed, so hide it just in case
                    e.Value = null;
                    break;

                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Amount Range":
                    e.Value = NumberRangeEditor.FormatDelimitedValues( e.Value, "N2" );
                    break;

                case "Account":

                    var accountIds = e.Value.SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
                    if ( accountIds.Any() && apAccount.Visible )
                    {
                        var accountNames = FinancialAccountCache.GetByIds( accountIds ).OrderBy( a => a.Order ).OrderBy( a => a.Name ).Select( a => a.Name ).ToList().AsDelimited( ", ", " or " );
                        e.Value = accountNames;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    break;

                case "Transaction Type":
                case "Currency Type":
                case "Credit Card Type":
                case "Source Type":

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

                case "Campus":
                case "CampusAccount":
                    var campus = CampusCache.Get( e.Value.AsInteger() );
                    if ( campus != null )
                    {
                        e.Value = campus.Name;
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }

                    if ( e.Key == "Campus" )
                    {
                        e.Name = "Campus (of Batch)";
                    }
                    else if ( e.Key == "CampusAccount" )
                    {
                        e.Name = "Campus (of Account)";
                    }

                    break;

                case "Person":
                    if ( !( this.ContextEntity() is Person ) )
                    {
                        var person = new PersonService( new RockContext() ).Get( e.Value.AsInteger() );
                        if ( person != null )
                        {
                            e.Value = person.FullName;
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
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfTransactions_ApplyFilterClick( object sender, EventArgs e )
        {
            gfTransactions.SetFilterPreference( "Date Range", drpDates.DelimitedValues );
            gfTransactions.SetFilterPreference( "Amount Range", nreAmount.DelimitedValues );
            gfTransactions.SetFilterPreference( "Transaction Code", tbTransactionCode.Text );
            gfTransactions.SetFilterPreference( "Foreign Key", tbForeignKey.Text );
            gfTransactions.SetFilterPreference( "Account", apAccount.SelectedValue != All.Id.ToString() ? apAccount.SelectedValue : string.Empty );
            gfTransactions.SetFilterPreference( "Transaction Type", dvpTransactionType.SelectedValue != All.Id.ToString() ? dvpTransactionType.SelectedValue : string.Empty );
            gfTransactions.SetFilterPreference( "Currency Type", dvpCurrencyType.SelectedValue != All.Id.ToString() ? dvpCurrencyType.SelectedValue : string.Empty );
            gfTransactions.SetFilterPreference( "Credit Card Type", dvpCreditCardType.SelectedValue != All.Id.ToString() ? dvpCreditCardType.SelectedValue : string.Empty );
            gfTransactions.SetFilterPreference( "Source Type", dvpSourceType.SelectedValue != All.Id.ToString() ? dvpSourceType.SelectedValue : string.Empty );

            // Campus of Batch
            gfTransactions.SetFilterPreference( "Campus", campCampusBatch.SelectedValue );

            // Campus of Account
            gfTransactions.SetFilterPreference( "CampusAccount", campCampusAccount.SelectedValue );

            gfTransactions.SetFilterPreference( "Person", ppPerson.SelectedValue.ToString() );

            if ( _availableAttributes != null )
            {
                foreach ( var attribute in _availableAttributes )
                {
                    var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                    if ( filterControl != null )
                    {
                        try
                        {
                            var values = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                            gfTransactions.SetFilterPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
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
        /// Handles the RowDataBound event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gTransactions_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var txn = e.Row.DataItem as FinancialTransactionRow;

            if ( txn == null )
            {
                return;
            }

            string currencyType = string.Empty;
            string creditCardType = string.Empty;

            var lPersonFullNameReversed = e.Row.FindControl( "lPersonFullNameReversed" ) as Literal;
            var lPersonId = e.Row.FindControl( "lPersonId" ) as Literal;
            if ( lPersonFullNameReversed != null && lPersonId != null && txn.AuthorizedPersonAliasId.HasValue )
            {
                var personDetail = _personDetails.FirstOrDefault( a => a.PersonAliasId == txn.AuthorizedPersonAliasId.Value );
                if ( personDetail != null )
                {
                    lPersonId.Text = personDetail.PersonId.ToString();
                    lPersonFullNameReversed.Text = personDetail.FullName;
                }
            }

            if ( txn.FinancialPaymentDetail != null && txn.FinancialPaymentDetail.CurrencyTypeValueId.HasValue )
            {
                int currencyTypeId = txn.FinancialPaymentDetail.CurrencyTypeValueId.Value;
                if ( _currencyTypes.ContainsKey( currencyTypeId ) )
                {
                    currencyType = _currencyTypes[currencyTypeId];
                }
                else
                {
                    var currencyTypeValue = DefinedValueCache.Get( currencyTypeId );
                    currencyType = currencyTypeValue != null ? currencyTypeValue.Value : string.Empty;
                    _currencyTypes.Add( currencyTypeId, currencyType );
                }

                var lCurrencyType = e.Row.FindControl( "lCurrencyType" ) as Literal;
                if ( lCurrencyType != null )
                {
                    if ( txn.FinancialPaymentDetail.CreditCardTypeValueId.HasValue )
                    {
                        int creditCardTypeId = txn.FinancialPaymentDetail.CreditCardTypeValueId.Value;
                        if ( _creditCardTypes.ContainsKey( creditCardTypeId ) )
                        {
                            creditCardType = _creditCardTypes[creditCardTypeId];
                        }
                        else
                        {
                            var creditCardTypeValue = DefinedValueCache.Get( creditCardTypeId );
                            creditCardType = creditCardTypeValue != null ? creditCardTypeValue.Value : string.Empty;
                            _creditCardTypes.Add( creditCardTypeId, creditCardType );
                        }

                        lCurrencyType.Text = string.Format( "{0} - {1}", currencyType, creditCardType );
                    }
                    else
                    {
                        lCurrencyType.Text = currencyType;
                    }
                }
            }

            var lTransactionImage = e.Row.FindControl( "lTransactionImage" ) as Literal;
            if ( lTransactionImage != null && lTransactionImage.Visible )
            {
                if ( _imageBinaryFileIdLookupByTransactionId.ContainsKey( txn.Id ) )
                {
                    int? firstImageId = _imageBinaryFileIdLookupByTransactionId[txn.Id].FirstOrDefault();
                    if ( firstImageId != null )
                    {
                        var options = new GetImageUrlOptions
                        {
                            Height = _imageHeight
                        };
                        string imageSrc = FileUrlHelper.GetImageUrl( firstImageId.Value, options );
                        lTransactionImage.Text = string.Format( "<image src='{0}' />", this.ResolveUrl( imageSrc ) );
                    }
                }
            }

            bool isExporting = false;
            if ( e is RockGridViewRowEventArgs )
            {
                isExporting = ( e as RockGridViewRowEventArgs ).IsExporting;
            }

            var lBatchId = e.Row.FindControl( "lBatchId" ) as Literal;
            if ( lBatchId != null )
            {
                if ( _batchPageRoute.IsNotNullOrWhiteSpace() && txn.BatchId.HasValue && !isExporting )
                {
                    var cell = e.Row.Cells.OfType<DataControlFieldCell>().Where( a => a == lBatchId.FirstParentControlOfType<DataControlFieldCell>() ).First();
                    cell.RemoveCssClass( "grid-select-cell" );
                    lBatchId.Text = string.Format( "<a href='{0}?BatchId={1}'>{1}</a>", _batchPageRoute, txn.BatchId );
                }
                else
                {
                    lBatchId.Text = txn.BatchId.ToString();
                }
            }

            var lAccounts = e.Row.FindControl( "lAccounts" ) as Literal;
            if ( lAccounts != null )
            {
                lAccounts.Text = this.GetAccounts( txn, isExporting );
            }

            var lForeignCurrencySymbol = e.Row.FindControl( "lForeignCurrencySymbol" ) as Literal;
            if ( lForeignCurrencySymbol != null && txn.ForeignCurrencyCodeValueId != null )
            {
                var currencyCode = DefinedValueCache.Get( txn.ForeignCurrencyCodeValueId.Value );
                if ( currencyCode != null )
                {
                    var currencySymbol = currencyCode.GetAttributeValue( "Symbol" );
                    lForeignCurrencySymbol.Text = currencyCode.Value + " " + currencySymbol;
                }
            }

            // Calculate the days since the last transaction. This is done as a C# calculate so that the
            // block doesn't sacrifice much in query performance to get this value. The previous date could be
            // previous or next in the row order depending on how the data is sorted
            var lDaysSinceLastTransaction = e.Row.FindControl( "lDaysSinceLastTransaction" ) as Literal;
            if ( lDaysSinceLastTransaction != null )
            {
                var transactionsShown = gTransactions.DataSourceAsList;
                var transactionsShownCount = transactionsShown?.Count ?? 0;
                var currentDate = txn.TransactionDateTime;

                var nextTransactionIndex = e.Row.RowIndex + 1;
                var nextTransaction = ( nextTransactionIndex >= 0 && nextTransactionIndex < transactionsShownCount ) ?
                    transactionsShown[nextTransactionIndex] as FinancialTransactionRow :
                    null;
                var nextDate = nextTransaction?.TransactionDateTime;

                var prevTransactionIndex = e.Row.RowIndex - 1;
                var prevTransaction = ( prevTransactionIndex >= 0 && prevTransactionIndex < transactionsShownCount ) ?
                    transactionsShown[prevTransactionIndex] as FinancialTransactionRow :
                    null;
                var prevDate = prevTransaction?.TransactionDateTime;
                int? daysSinceLastTransaction = null;

                if ( nextDate.HasValue && nextDate.Value < currentDate && txn.Id != nextTransaction.Id )
                {
                    daysSinceLastTransaction = ( int ) Math.Round( ( currentDate - nextDate.Value ).TotalDays, 0 );
                }
                else if ( prevDate.HasValue && prevDate.Value < currentDate && txn.Id != prevTransaction.Id )
                {
                    daysSinceLastTransaction = ( int ) Math.Round( ( currentDate - prevDate.Value ).TotalDays, 0 );
                }

                lDaysSinceLastTransaction.Text = daysSinceLastTransaction?.ToString();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gTransactions_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the Add event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gTransactions_Add( object sender, EventArgs e )
        {
            ShowDetailForm( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gTransactions_Edit( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            ShowDetailForm( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gTransactions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void gTransactions_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var transactionService = new FinancialTransactionService( rockContext );
            var transaction = transactionService.Get( e.RowKeyId );
            if ( transaction != null )
            {
                string errorMessage;
                if ( !transactionService.CanDelete( transaction, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                // prevent deleting a Transaction that is in closed batch
                if ( transaction.Batch != null )
                {
                    if ( transaction.Batch.Status == BatchStatus.Closed )
                    {
                        mdGridWarning.Show( string.Format( "This {0} is assigned to a closed {1}", FinancialTransaction.FriendlyTypeName, FinancialBatch.FriendlyTypeName ), ModalAlertType.Information );
                        return;
                    }
                }

                if ( transaction.BatchId.HasValue )
                {
                    string caption = ( transaction.AuthorizedPersonAlias != null && transaction.AuthorizedPersonAlias.Person != null ) ?
                        transaction.AuthorizedPersonAlias.Person.FullName :
                        string.Format( "Transaction: {0}", transaction.Id );

                    var changes = new History.HistoryChangeList();
                    changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" );

                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( FinancialBatch ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                        transaction.BatchId.Value,
                        changes,
                        caption,
                        typeof( FinancialTransaction ),
                        transaction.Id,
                        false
                    );
                }

                transactionService.Delete( transaction );

                rockContext.SaveChanges();

                RockPage.UpdateBlocks( "~/Blocks/Finance/BatchDetail.ascx" );
            }

            BindGrid();
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( _canEdit && _batch != null )
            {
                int? moveToBatchId = hfMoveToBatchId.Value.AsIntegerOrNull();
                if ( eventArgument == "MoveTransactions" && moveToBatchId.HasValue )
                {
                    var txnsSelected = new List<int>();
                    gTransactions.SelectedKeys.ToList().ForEach( b => txnsSelected.Add( b.ToString().AsInteger() ) );

                    if ( txnsSelected.Any() )
                    {
                        var rockContext = new RockContext();
                        var batchService = new FinancialBatchService( rockContext );

                        var newBatch = batchService.Get( moveToBatchId.Value );
                        var oldBatch = batchService.Get( _batch.Id );

                        if ( oldBatch != null && newBatch != null && newBatch.Status == BatchStatus.Open )
                        {
                            var txnService = new FinancialTransactionService( rockContext );
                            var txnsToUpdate = txnService.Queryable( "AuthorizedPersonAlias.Person" )
                                .Where( t => txnsSelected.Contains( t.Id ) )
                                .ToList();

                            decimal oldBatchControlAmount = oldBatch.ControlAmount;
                            decimal newBatchControlAmount = newBatch.ControlAmount;

                            foreach ( var txn in txnsToUpdate )
                            {
                                txn.BatchId = newBatch.Id;
                                oldBatchControlAmount -= txn.TotalAmount;
                                newBatchControlAmount += txn.TotalAmount;
                            }

                            var oldBatchChanges = new History.HistoryChangeList();
                            History.EvaluateChange( oldBatchChanges, "Control Amount", oldBatch.ControlAmount.FormatAsCurrency(), oldBatchControlAmount.FormatAsCurrency() );
                            oldBatch.ControlAmount = oldBatchControlAmount;

                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( FinancialBatch ),
                                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                                oldBatch.Id,
                                oldBatchChanges,
                                false
                            );

                            var newBatchChanges = new History.HistoryChangeList();
                            History.EvaluateChange( newBatchChanges, "Control Amount", newBatch.ControlAmount.FormatAsCurrency(), newBatchControlAmount.FormatAsCurrency() );
                            newBatch.ControlAmount = newBatchControlAmount;

                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( FinancialBatch ),
                                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                                newBatch.Id,
                                newBatchChanges,
                                false
                            );

                            rockContext.SaveChanges();

                            var pageRef = new Rock.Web.PageReference( RockPage.PageId );
                            pageRef.Parameters = new Dictionary<string, string>();
                            pageRef.Parameters.Add( "BatchId", newBatch.Id.ToString() );
                            string newBatchLink = string.Format( "<a href='{0}'>{1}</a>",
                                pageRef.BuildUrl(), newBatch.Name );

                            RockPage.UpdateBlocks( "~/Blocks/Finance/BatchDetail.ascx" );

                            nbResult.Text = string.Format( "{0} transactions were moved to the '{1}' batch.",
                                txnsToUpdate.Count().ToString( "N0" ), newBatchLink );
                            nbResult.NotificationBoxType = NotificationBoxType.Success;
                            nbResult.Visible = true;
                        }
                        else
                        {
                            nbResult.Text = string.Format( "The selected batch does not exist, or is no longer open." );
                            nbResult.NotificationBoxType = NotificationBoxType.Danger;
                            nbResult.Visible = true;
                        }
                    }
                    else
                    {
                        nbResult.Text = string.Format( "There were not any transactions selected." );
                        nbResult.NotificationBoxType = NotificationBoxType.Warning;
                        nbResult.Visible = true;
                    }
                }

                _ddlMove.SelectedIndex = 0;
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the _lbReassign control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbReassign_Click( object sender, EventArgs e )
        {
            if ( _canEdit && _person != null )
            {
                var txnsSelected = new List<int>();
                gTransactions.SelectedKeys.ToList().ForEach( b => txnsSelected.Add( b.ToString().AsInteger() ) );

                if ( txnsSelected.Any() )
                {
                    ShowDialog( "Reassign" );
                }
                else
                {
                    nbResult.Text = string.Format( "There were not any transactions selected." );
                    nbResult.NotificationBoxType = NotificationBoxType.Warning;
                    nbResult.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgReassign control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgReassign_SaveClick( object sender, EventArgs e )
        {
            if ( _canEdit && _person != null )
            {
                int? personId = ppReassign.PersonId;
                int? personAliasId = ppReassign.PersonAliasId;
                var txnsSelected = new List<int>();
                gTransactions.SelectedKeys.ToList().ForEach( b => txnsSelected.Add( b.ToString().AsInteger() ) );

                if ( txnsSelected.Any() && personAliasId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        foreach ( var txn in new FinancialTransactionService( rockContext )
                            .Queryable( "AuthorizedPersonAlias.Person" )
                            .Where( t => txnsSelected.Contains( t.Id ) )
                            .ToList() )
                        {
                            txn.AuthorizedPersonAliasId = personAliasId.Value;
                        }
                        rockContext.SaveChanges();

                        string acctAction = rblReassignBankAccounts.SelectedValue;
                        if ( acctAction != "NONE" && personId.HasValue && _person != null )
                        {
                            var bankAcctService = new FinancialPersonBankAccountService( rockContext );

                            var bankAccts = bankAcctService.Queryable()
                                .Where( a => a.PersonAlias != null && a.PersonAlias.PersonId == _person.Id )
                                .ToList();

                            var existingAccts = bankAcctService.Queryable()
                                .Where( a => a.PersonAlias != null && a.PersonAlias.PersonId == personId.Value )
                                .Select( a => a.AccountNumberSecured )
                                .ToList();

                            if ( bankAccts.Any() )
                            {
                                foreach ( var bankAcct in bankAccts )
                                {
                                    if ( acctAction == "MOVE" )
                                    {
                                        if ( existingAccts.Contains( bankAcct.AccountNumberSecured ) )
                                        {
                                            bankAcctService.Delete( bankAcct );
                                        }
                                        else
                                        {
                                            bankAcct.PersonAliasId = personAliasId.Value;
                                        }
                                    }
                                    else
                                    {
                                        if ( !existingAccts.Contains( bankAcct.AccountNumberSecured ) )
                                        {
                                            var newBankAcct = new FinancialPersonBankAccount();
                                            bankAcctService.Add( newBankAcct );
                                            newBankAcct.PersonAliasId = personAliasId.Value;
                                            newBankAcct.AccountNumberMasked = bankAcct.AccountNumberMasked;
                                            newBankAcct.AccountNumberSecured = bankAcct.AccountNumberSecured;
                                        }
                                    }
                                }

                                rockContext.SaveChanges();

                                if ( acctAction == "MOVE" )
                                {
                                    var moveChanges = new History.HistoryChangeList();
                                    moveChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Acct/Routing information" );
                                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(), _person.Id,
                                        moveChanges, true, CurrentPersonAliasId );
                                }

                                var accountChanges = new History.HistoryChangeList();
                                accountChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Acct/Routing information" );


                                HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(), personId.Value,
                                    accountChanges, true, CurrentPersonAliasId );

                                RockPage.UpdateBlocks( "~/Blocks/Crm/PersonDetail/BankAccountList.ascx" );
                            }
                        }

                    }
                }
            }

            HideDialog();
            BindGrid();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            gfTransactions.PreferenceKeyPrefix = hfTransactionViewMode.Value;
            drpDates.DelimitedValues = gfTransactions.GetFilterPreference( "Date Range" );
            nreAmount.DelimitedValues = gfTransactions.GetFilterPreference( "Amount Range" );

            if ( GetAttributeValue( AttributeKey.ShowForeignKey ).AsBoolean() )
            {
                tbForeignKey.Text = gfTransactions.GetFilterPreference( "Foreign Key" );
                tbForeignKey.Visible = true;
            }
            else
            {
                tbForeignKey.Visible = false;
            }

            tbTransactionCode.Text = gfTransactions.GetFilterPreference( "Transaction Code" );

            apAccount.Visible = string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.Accounts ) );
            apAccount.DisplayActiveOnly = GetAttributeValue( AttributeKey.ActiveAccountsOnlyFilter ).AsBoolean();

            var accountIds = ( gfTransactions.GetFilterPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
            if ( accountIds.Any() )
            {
                var accounts = FinancialAccountCache.GetByIds( accountIds ).OrderBy( a => a.Order ).OrderBy( a => a.Name ).ToList();
                apAccount.SetValuesFromCache( accounts );
            }
            else
            {
                apAccount.SetValue( 0 );
            }

            BindDefinedTypeDropdown( dvpTransactionType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE ), "Transaction Type" );
            BindDefinedTypeDropdown( dvpCurrencyType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE ), "Currency Type" );
            BindDefinedTypeDropdown( dvpCreditCardType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ), "Credit Card Type" );
            BindDefinedTypeDropdown( dvpSourceType, new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE ), "Source Type" );

            if ( this.ContextEntity() == null )
            {
                var campusi = CampusCache.All();
                campCampusBatch.Campuses = campusi;
                campCampusBatch.SetValue( gfTransactions.GetFilterPreference( "Campus" ) );

                campCampusAccount.Campuses = campusi;
                campCampusAccount.SetValue( gfTransactions.GetFilterPreference( "CampusAccount" ) );
            }
            else
            {
                campCampusBatch.Visible = false;
                campCampusAccount.Visible = false;
            }

            // don't show the person picker if the the current context is already a specific person
            if ( this.ContextEntity() is Person || !IsUserAuthorized( "FilterByPerson" ) )
            {
                ppPerson.Visible = false;
            }
            else
            {
                ppPerson.Visible = true;
                var personId = gfTransactions.GetFilterPreference( "Person" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var person = new PersonService( new RockContext() ).Get( personId.Value );
                    ppPerson.SetValue( person );
                }
                else
                {
                    ppPerson.SetValue( null );
                }
            }

            BindAttributes();
            AddDynamicControls();
        }

        /// <summary>
        /// Binds the defined type dropdown.
        /// </summary>
        /// <param name="ListControl">The list control.</param>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="userPreferenceKey">The user preference key.</param>
        private void BindDefinedTypeDropdown( DefinedValuePicker dvpControl, Guid definedTypeGuid, string userPreferenceKey )
        {
            dvpControl.DefinedTypeId = DefinedTypeCache.Get( definedTypeGuid ).Id;
            dvpControl.SelectedValue = gfTransactions.GetFilterPreference( userPreferenceKey );
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters
            _availableAttributes = new List<AttributeCache>();

            int entityTypeId;
            if ( hfTransactionViewMode.Value == "Transactions" )
            {
                entityTypeId = new FinancialTransaction().TypeId;
            }
            else
            {
                entityTypeId = new FinancialTransactionDetail().TypeId;
            }

            foreach ( var attributeModel in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn &&
                    ( a.EntityTypeQualifierColumn == null || a.EntityTypeQualifierColumn == "" ) &&
                    ( a.EntityTypeQualifierValue == null || a.EntityTypeQualifierValue == "" ) )
                .OrderByDescending( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                _availableAttributes.Add( AttributeCache.Get( attributeModel ) );
            }
        }

        /// <summary>
        /// Adds the dynamic controls.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            // Remove attribute columns
            foreach ( var column in gTransactions.Columns.OfType<AttributeField>().ToList() )
            {
                gTransactions.Columns.Remove( column );
            }

            // Remove summary column
            var summaryCol = gTransactions.Columns.OfType<RockBoundField>().FirstOrDefault( c => c.DataField == "Summary" );
            if ( summaryCol != null )
            {
                gTransactions.Columns.Remove( summaryCol );
            }

            // Remove image column
            var imageCol = gTransactions.Columns.OfType<RockLiteralField>().FirstOrDefault( c => c.HeaderText == "Image" );
            if ( imageCol != null )
            {
                gTransactions.Columns.Remove( imageCol );
            }

            // Remove delete column
            var deleteCol = gTransactions.Columns.OfType<DeleteField>().FirstOrDefault();
            if ( deleteCol != null )
            {
                gTransactions.Columns.Remove( deleteCol );
            }

            if ( _availableAttributes != null )
            {
                foreach ( var attribute in _availableAttributes )
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

                        string savedValue = gfTransactions.GetFilterPreference( attribute.Key );
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

                    bool columnExists = gTransactions.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;
                        boundField.SortExpression = string.Empty;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gTransactions.Columns.Add( boundField );
                    }
                }
            }

            summaryCol = new RockBoundField();
            summaryCol.DataField = "Summary";
            summaryCol.HeaderText = "Summary";
            summaryCol.SortExpression = "Summary";
            summaryCol.ColumnPriority = ColumnPriority.DesktopLarge;
            summaryCol.HtmlEncode = false;
            gTransactions.Columns.Add( summaryCol );

            imageCol = new RockLiteralField();
            imageCol.ID = "lTransactionImage";
            imageCol.HeaderText = "Image";
            gTransactions.Columns.Add( imageCol );

            deleteCol = new DeleteField();
            deleteCol.Visible = this.ShowDeleteButton;
            gTransactions.Columns.Add( deleteCol );
            deleteCol.Click += gTransactions_Delete;
        }

        /// <summary>
        /// Refreshes the list. Public method...can be called from other blocks.
        /// </summary>
        public void RefreshList()
        {
            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is FinancialBatch )
                {
                    var batchId = PageParameter( "BatchId" );
                    var batch = new FinancialBatchService( new RockContext() ).Get( int.Parse( batchId ) );
                    _batch = batch;
                    BindGrid();
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( bool isExporting = false )
        {
            if ( hfTransactionViewMode.Value == "Transactions" )
            {
                btnTransactionDetails.CssClass = "btn btn-xs btn-outline-primary";
                btnTransactions.CssClass = "btn btn-xs btn-primary";
            }
            else
            {
                btnTransactionDetails.CssClass = "btn btn-xs btn-primary";
                btnTransactions.CssClass = "btn btn-xs btn-outline-primary";
            }

            _currencyTypes = new Dictionary<int, string>();
            _creditCardTypes = new Dictionary<int, string>();

            // If configured for a registration and registration is null, return
            int registrationEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == registrationEntityTypeId ) && _registration == null )
            {
                return;
            }

            // If configured for a person and person is null, return
            int personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && _person == null )
            {
                return;
            }

            // If configured for a batch and batch is null, return
            int batchEntityTypeId = EntityTypeCache.Get( "Rock.Model.FinancialBatch" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == batchEntityTypeId ) && _batch == null )
            {
                return;
            }

            // If configured for a batch and batch is null, return
            int scheduledTxnEntityTypeId = EntityTypeCache.Get( "Rock.Model.FinancialScheduledTransaction" ).Id;
            if ( ContextTypesRequired.Any( e => e.Id == scheduledTxnEntityTypeId ) && _scheduledTxn == null )
            {
                return;
            }

            gTransactions.ColumnsOfType<RockBoundField>().First( c => c.DataField == "ForeignKey" ).Visible =
                GetAttributeValue( AttributeKey.ShowForeignKey ).AsBoolean();

            var lDaysSinceLastTransactionGridField = gTransactions.ColumnsOfType<RockLiteralField>().FirstOrDefault( c => c.ID == "lDaysSinceLastTransaction" );
            if ( lDaysSinceLastTransactionGridField != null )
            {

                lDaysSinceLastTransactionGridField.Visible = GetAttributeValue( AttributeKey.ShowDaysSinceLastTransaction ).AsBoolean();
            }

            var hideTransactionsInPendingBatches = GetAttributeValue( AttributeKey.HideTransactionsInPendingBatches ).AsBoolean();
            var rockContext = new RockContext();

            SortProperty sortProperty = gTransactions.SortProperty;

            // Qry
            IQueryable<FinancialTransactionRow> qry;
            if ( hfTransactionViewMode.Value == "Transaction Details" )
            {
                gTransactions.RowItemText = "Transaction Detail";
                var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
                var financialTransactionDetailQry = financialTransactionDetailService.Queryable().AsNoTracking();

                var includeFutureTransactions = GetAttributeValue( AttributeKey.ShowFutureTransactions ).AsBooleanOrNull() ?? false;
                if ( includeFutureTransactions )
                {
                    financialTransactionDetailQry = financialTransactionDetailQry.Where( a =>
                        a.Transaction.TransactionDateTime.HasValue ||
                        a.Transaction.FutureProcessingDateTime.HasValue );
                }
                else
                {
                    financialTransactionDetailQry = financialTransactionDetailQry.Where( a => a.Transaction.TransactionDateTime.HasValue );
                }

                if ( hideTransactionsInPendingBatches )
                {
                    financialTransactionDetailQry = financialTransactionDetailQry.Where( a => a.Transaction.Batch == null || a.Transaction.Batch.Status != BatchStatus.Pending );
                }

                if ( _availableAttributes != null && _availableAttributes.Any() )
                {
                    foreach ( var attribute in _availableAttributes )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        financialTransactionDetailQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( financialTransactionDetailQry, filterControl, attribute, financialTransactionDetailService, Rock.Reporting.FilterMode.SimpleFilter );
                    }
                }

                // Filter to configured accounts.
                var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
                if ( accountGuids.Any() )
                {
                    financialTransactionDetailQry = financialTransactionDetailQry
                        .Where( d => accountGuids.Contains( d.Account.Guid ) );
                }

                qry = financialTransactionDetailQry
                    .Select( a => new FinancialTransactionRow
                    {
                        Id = a.TransactionId,
                        BatchId = a.Transaction.BatchId,
                        TransactionTypeValueId = a.Transaction.TransactionTypeValueId,
                        ScheduledTransactionId = a.Transaction.ScheduledTransactionId,
                        AuthorizedPersonAliasId = a.Transaction.AuthorizedPersonAliasId,
                        AuthorizedPersonLastName = ( a.Transaction.AuthorizedPersonAlias == null ) ? string.Empty : a.Transaction.AuthorizedPersonAlias.Person.LastName,
                        AuthorizedPersonNickName = ( a.Transaction.AuthorizedPersonAlias == null ) ? string.Empty : a.Transaction.AuthorizedPersonAlias.Person.NickName,
                        TransactionDateTime = a.Transaction.TransactionDateTime ?? a.Transaction.FutureProcessingDateTime.Value,
                        FutureProcessingDateTime = a.Transaction.FutureProcessingDateTime,
                        SourceTypeValueId = a.Transaction.SourceTypeValueId,
                        TotalAmount = a.Amount,
                        TransactionCode = a.Transaction.TransactionCode,
                        ForeignKey = a.Transaction.ForeignKey,
                        Status = a.Transaction.Status,
                        SettledDate = a.Transaction.SettledDate,
                        SettledGroupId = a.Transaction.SettledGroupId,
                        FinancialGatewayId = a.Transaction.FinancialGatewayId,
                        IsReconciled = a.Transaction.IsReconciled,
                        IsSettled = a.Transaction.IsSettled,
                        NonCashAssetTypeValueId = a.Transaction.NonCashAssetTypeValueId,
                        ProcessedDateTime = a.Transaction.ProcessedDateTime,
                        ShowAsAnonymous = a.Transaction.ShowAsAnonymous,
                        StatusMessage = a.Transaction.StatusMessage,
                        TransactionDetail = new DetailInfo
                        {
                            AccountId = a.AccountId,
                            Amount = a.Amount,
                            EntityId = a.EntityId,
                            EntityTypeId = a.EntityTypeId
                        },
                        Summary = a.Transaction.FutureProcessingDateTime.HasValue ? "[charge pending] " + a.Summary : a.Transaction.Summary,
                        FinancialPaymentDetail = a.Transaction.FinancialPaymentDetailId != null ? new PaymentDetailInfo
                        {
                            Id = a.Transaction.FinancialPaymentDetail.Id,
                            CreditCardTypeValueId = a.Transaction.FinancialPaymentDetail.CreditCardTypeValueId,
                            CurrencyTypeValueId = a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId
                        } : null,
                        ForeignCurrencyCodeValueId = a.Transaction.ForeignCurrencyCodeValueId
                    } );
            }
            else
            {
                gTransactions.RowItemText = "Transactions";
                var financialTransactionService = new FinancialTransactionService( rockContext );
                var financialTransactionQry = financialTransactionService.Queryable().AsNoTracking();

                var includeFutureTransactions = GetAttributeValue( AttributeKey.ShowFutureTransactions ).AsBooleanOrNull() ?? false;
                if ( includeFutureTransactions )
                {
                    financialTransactionQry = financialTransactionQry.Where( a =>
                        a.TransactionDateTime.HasValue ||
                        a.FutureProcessingDateTime.HasValue );
                }
                else
                {
                    financialTransactionQry = financialTransactionQry.Where( a => a.TransactionDateTime.HasValue );
                }

                if ( hideTransactionsInPendingBatches )
                {
                    financialTransactionQry = financialTransactionQry.Where( a => a.Batch == null || a.Batch.Status != BatchStatus.Pending );
                }

                // Filter to configured accounts.
                var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
                if ( accountGuids.Any() )
                {
                    financialTransactionQry = financialTransactionQry
                        .Where( t => t.TransactionDetails.Any( d => accountGuids.Contains( d.Account.Guid ) ) );
                }

                if ( _availableAttributes != null && _availableAttributes.Any() )
                {
                    foreach ( var attribute in _availableAttributes )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        financialTransactionQry = attribute.FieldType.Field.ApplyAttributeQueryFilter( financialTransactionQry, filterControl, attribute, financialTransactionService, Rock.Reporting.FilterMode.SimpleFilter );
                    }
                }

                qry = financialTransactionQry
                    .Select( a => new FinancialTransactionRow
                    {
                        Id = a.Id,
                        BatchId = a.BatchId,
                        TransactionTypeValueId = a.TransactionTypeValueId,
                        ScheduledTransactionId = a.ScheduledTransactionId,
                        AuthorizedPersonAliasId = a.AuthorizedPersonAliasId,
                        AuthorizedPersonLastName = ( a.AuthorizedPersonAlias == null ) ? string.Empty : a.AuthorizedPersonAlias.Person.LastName,
                        AuthorizedPersonNickName = ( a.AuthorizedPersonAlias == null ) ? string.Empty : a.AuthorizedPersonAlias.Person.NickName,
                        TransactionDateTime = a.TransactionDateTime ?? a.FutureProcessingDateTime.Value,
                        FutureProcessingDateTime = a.FutureProcessingDateTime,
                        TransactionDetails = a.TransactionDetails.Select( d => new DetailInfo { AccountId = d.AccountId, Amount = d.Amount, EntityId = d.EntityId, EntityTypeId = d.EntityTypeId } ),
                        Refunds = a.Refunds.Select( r => new RefundInfo { TransactionId = r.Id, TransactionCode = r.FinancialTransaction.TransactionCode } ),
                        RefundForTransactionId = ( a.RefundDetails == null ) ? null : a.RefundDetails.OriginalTransactionId,
                        SourceTypeValueId = a.SourceTypeValueId,
                        TotalAmount = a.TransactionDetails.Sum( d => ( decimal? ) d.Amount ),
                        TransactionCode = a.TransactionCode,
                        ForeignKey = a.ForeignKey,
                        Status = a.Status,
                        SettledDate = a.SettledDate,
                        SettledGroupId = a.SettledGroupId,
                        Summary = a.FutureProcessingDateTime.HasValue ? "[charge pending] " + a.Summary : a.Summary,
                        FinancialPaymentDetail = new PaymentDetailInfo { Id = a.Id, CreditCardTypeValueId = a.FinancialPaymentDetail.CreditCardTypeValueId, CurrencyTypeValueId = a.FinancialPaymentDetail.CurrencyTypeValueId },
                        ForeignCurrencyCodeValueId = a.ForeignCurrencyCodeValueId,
                        FinancialGatewayId = a.FinancialGatewayId,
                        IsReconciled = a.IsReconciled,
                        IsSettled = a.IsSettled,
                        NonCashAssetTypeValueId = a.NonCashAssetTypeValueId,
                        ProcessedDateTime = a.ProcessedDateTime,
                        ShowAsAnonymous = a.ShowAsAnonymous,
                        StatusMessage = a.StatusMessage
                    } );
            }

            // Transaction Types
            var transactionTypeValueIdList = GetAttributeValue( AttributeKey.TransactionTypes ).SplitDelimitedValues().AsGuidList().Select( a => DefinedValueCache.Get( a ) ).Where( a => a != null ).Select( a => a.Id ).ToList();

            if ( transactionTypeValueIdList.Any() )
            {
                qry = qry.Where( t => transactionTypeValueIdList.Contains( t.TransactionTypeValueId ) );
            }

            // Transaction Sources
            var sourceValueIdList = GetAttributeValue( AttributeKey.SourceTypes ).SplitDelimitedValues().AsGuidList().Select( a => DefinedValueCache.Get( a ) ).Where( a => a != null ).Select( a => a.Id ).ToList();

            if ( sourceValueIdList.Any() )
            {
                qry = qry.Where( t => t.SourceTypeValueId.HasValue && sourceValueIdList.Contains( t.SourceTypeValueId.Value ) );
            }

            // Set up the selection filter
            if ( _batch != null )
            {
                // If transactions are for a batch, the filter is hidden so only check the batch id
                qry = qry.Where( t => t.BatchId.HasValue && t.BatchId.Value == _batch.Id );

                // If the batch is closed, do not allow any editing of the transactions
                if ( _batch.Status != BatchStatus.Closed && _canEdit )
                {
                    gTransactions.IsDeleteEnabled = _canEdit;
                }
                else
                {
                    gTransactions.IsDeleteEnabled = false;
                }
            }
            else if ( _scheduledTxn != null )
            {
                // If transactions are for a batch, the filter is hidden so only check the batch id
                qry = qry.Where( t => t.ScheduledTransactionId.HasValue && t.ScheduledTransactionId.Value == _scheduledTxn.Id );

                gTransactions.IsDeleteEnabled = false;
            }
            else if ( _registration != null )
            {
                if ( hfTransactionViewMode.Value == "Transactions" )
                {
                    qry = qry
                        .Where( t => t.TransactionDetails
                            .Any( d =>
                                d.EntityTypeId.HasValue &&
                                d.EntityTypeId.Value == registrationEntityTypeId &&
                                d.EntityId.HasValue &&
                                d.EntityId.Value == _registration.Id ) );
                }
                else
                {
                    qry = qry
                        .Where( t => t.TransactionDetail.EntityTypeId.HasValue &&
                                t.TransactionDetail.EntityTypeId.Value == registrationEntityTypeId &&
                                t.TransactionDetail.EntityId.HasValue &&
                                t.TransactionDetail.EntityId.Value == _registration.Id );
                }

                gTransactions.IsDeleteEnabled = false;
            }
            else    // Person
            {
                // otherwise set the selection based on filter settings
                if ( _person != null )
                {
                    // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
                    var personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == _person.GivingId ).Select( a => a.Id ).ToList();

                    // get the transactions for the person or all the members in the person's giving group (Family)
                    qry = qry.Where( t => t.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );
                }

                // Date Range
                var drp = new DateRangePicker();
                drp.DelimitedValues = gfTransactions.GetFilterPreference( "Date Range" );
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.TransactionDateTime >= drp.LowerValue.Value );
                }

                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( t => t.TransactionDateTime < upperDate );
                }

                // Amount Range
                var nre = new NumberRangeEditor();
                nre.DelimitedValues = gfTransactions.GetFilterPreference( "Amount Range" );
                if ( nre.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.TotalAmount >= nre.LowerValue.Value );
                }

                if ( nre.UpperValue.HasValue )
                {
                    qry = qry.Where( t => t.TotalAmount <= nre.UpperValue.Value );
                }

                // Transaction Code
                string transactionCode = gfTransactions.GetFilterPreference( "Transaction Code" );
                if ( !string.IsNullOrWhiteSpace( transactionCode ) )
                {
                    qry = qry.Where( t => t.TransactionCode == transactionCode.Trim() );
                }

                // Foreign Key
                if ( GetAttributeValue( AttributeKey.ShowForeignKey ).AsBoolean() )
                {
                    string foreingKey = gfTransactions.GetFilterPreference( "Foreign Key" );
                    if ( !string.IsNullOrWhiteSpace( foreingKey ) )
                    {
                        qry = qry.Where( t => t.ForeignKey == foreingKey.Trim() );
                    }
                }

                // Account Id
                var accountIds = ( gfTransactions.GetFilterPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
                accountIds = accountIds.Distinct().ToList();

                if ( accountIds.Any() && apAccount.Visible )
                {
                    if ( hfTransactionViewMode.Value == "Transactions" )
                    {
                        qry = qry.Where( t => t.TransactionDetails.Any( d => accountIds.Contains( d.AccountId ) ) );
                    }
                    else
                    {
                        qry = qry.Where( a => accountIds.Contains( a.TransactionDetail.AccountId ) );
                    }
                }

                // Transaction Type
                int transactionTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetFilterPreference( "Transaction Type" ), out transactionTypeId ) )
                {
                    qry = qry.Where( t => t.TransactionTypeValueId == transactionTypeId );
                }

                // Currency Type
                int currencyTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetFilterPreference( "Currency Type" ), out currencyTypeId ) )
                {
                    qry = qry.Where( t => t.FinancialPaymentDetail != null && t.FinancialPaymentDetail.CurrencyTypeValueId == currencyTypeId );
                }

                // Credit Card Type
                int creditCardTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetFilterPreference( "Credit Card Type" ), out creditCardTypeId ) )
                {
                    qry = qry.Where( t => t.FinancialPaymentDetail != null && t.FinancialPaymentDetail.CreditCardTypeValueId == creditCardTypeId );
                }

                // Source Type
                int sourceTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetFilterPreference( "Source Type" ), out sourceTypeId ) )
                {
                    qry = qry.Where( t => t.SourceTypeValueId == sourceTypeId );
                }

                // Campus of Batch and/or Account
                if ( this.ContextEntity() == null )
                {
                    var campusOfBatch = CampusCache.Get( gfTransactions.GetFilterPreference( "Campus" ).AsInteger() );
                    if ( campusOfBatch != null )
                    {
                        var qryBatchesForCampus = new FinancialBatchService( rockContext ).Queryable().Where( a => a.CampusId.HasValue && a.CampusId == campusOfBatch.Id ).Select( a => a.Id );
                        qry = qry.Where( t => qryBatchesForCampus.Contains( t.BatchId ?? 0 ) );
                    }
                    var campusOfAccount = CampusCache.Get( gfTransactions.GetFilterPreference( "CampusAccount" ).AsInteger() );
                    if ( campusOfAccount != null )
                    {
                        var qryAccountsForCampus = new FinancialAccountService( rockContext ).Queryable().Where( a => a.CampusId.HasValue && a.CampusId == campusOfAccount.Id ).Select( a => a.Id );
                        if ( hfTransactionViewMode.Value == "Transactions" )
                        {
                            qry = qry.Where( t => t.TransactionDetails.Any( d => qryAccountsForCampus.Contains( d.AccountId ) ) );
                        }
                        else
                        {
                            qry = qry.Where( a => qryAccountsForCampus.Contains( a.TransactionDetail.AccountId ) );
                        }
                    }
                }

                if ( !( this.ContextEntity() is Person ) )
                {
                    var filterPersonId = gfTransactions.GetFilterPreference( "Person" ).AsIntegerOrNull();
                    if ( filterPersonId.HasValue )
                    {
                        // get the transactions for the person or all the members in the person's giving group (Family)
                        var filterPerson = new PersonService( rockContext ).Get( filterPersonId.Value );
                        if ( filterPerson != null )
                        {
                            // fetch all the possible PersonAliasIds that have this GivingID to help optimize the SQL
                            var personAliasIds = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.GivingId == filterPerson.GivingId ).Select( a => a.Id ).ToList();

                            // get the transactions for the person or all the members in the person's giving group (Family)
                            qry = qry.Where( t => t.AuthorizedPersonAliasId.HasValue && personAliasIds.Contains( t.AuthorizedPersonAliasId.Value ) );
                        }
                    }
                }
            }

            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "_PERSONNAME_" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        qry = qry.OrderBy( a => a.AuthorizedPersonLastName )
                            .ThenBy( a => a.AuthorizedPersonNickName )
                            .ThenByDescending( a => a.FutureProcessingDateTime )
                            .ThenByDescending( a => a.TransactionDateTime )
                            .ThenByDescending( a => a.Id );
                    }
                    else
                    {
                        qry = qry.OrderByDescending( a => a.AuthorizedPersonLastName )
                            .ThenByDescending( a => a.AuthorizedPersonNickName )
                            .ThenByDescending( a => a.FutureProcessingDateTime )
                            .ThenByDescending( a => a.TransactionDateTime )
                            .ThenByDescending( a => a.Id );
                    }
                }
                else
                {
                    qry = qry.Sort( sortProperty );
                }
            }
            else
            {

                // Default sort by Id if the transactions are seen via the batch,
                // otherwise sort by descending date time.
                if ( ContextTypesRequired.Any( e => e.Id == batchEntityTypeId ) )
                {
                    qry = qry.OrderBy( t => t.Id );
                }
                else
                {
                    qry = qry.OrderByDescending( t => t.FutureProcessingDateTime ).ThenByDescending( t => t.TransactionDateTime ).ThenByDescending( t => t.Id );
                }
            }

            var lTransactionImageField = gTransactions.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.ID == "lTransactionImage" );
            var summaryField = gTransactions.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "Summary" );
            var showImages = bddlOptions.SelectedValue.AsIntegerOrNull() == 1;
            if ( lTransactionImageField != null )
            {
                lTransactionImageField.Visible = showImages;
            }

            if ( summaryField != null )
            {
                summaryField.Visible = !showImages;
            }

            var qryPersonAlias = new PersonAliasService( rockContext ).Queryable();
            _personDetails = qryPersonAlias.Where( a => qry.Any( q => q.AuthorizedPersonAliasId.HasValue && q.AuthorizedPersonAliasId == a.Id ) ).Select( a => new
            {
                a.Id,
                PersonId = a.Person.Id,
                a.Person.LastName,
                a.Person.NickName,
                a.Person.SuffixValueId,
                a.Person.RecordTypeValueId
            } ).AsNoTracking().ToList().Select( k => new PersonDetail
            {
                PersonAliasId = k.Id,
                PersonId = k.PersonId,
                FullName = Person.FormatFullNameReversed( k.LastName, k.NickName, k.SuffixValueId, k.RecordTypeValueId )
            } ).ToList();

            if ( showImages )
            {
                _imageBinaryFileIdLookupByTransactionId = new FinancialTransactionImageService( rockContext ).Queryable().Where( a => qry.Any( q => q.Id == a.TransactionId ) )
                    .Select( a => new { a.TransactionId, a.BinaryFileId, a.Order } )
                    .GroupBy( a => a.TransactionId )
                    .ToDictionary( k => k.Key, v => v.OrderBy( x => x.Order ).Select( x => x.BinaryFileId ).ToList() );
            }
            else
            {
                _imageBinaryFileIdLookupByTransactionId = new Dictionary<int, List<int>>();
            }

            if ( _availableAttributes.Any() )
            {
                gTransactions.ObjectList = new Dictionary<string, object>();
                var txns = new FinancialTransactionService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t => qry.Select( q => q.Id ).Contains( t.Id ) )
                    .ToList();
                txns.ForEach( t => gTransactions.ObjectList.Add( t.Id.ToString(), t ) );
            }

            gTransactions.EntityTypeId = EntityTypeCache.GetId<Rock.Model.FinancialTransaction>();
            gTransactions.SetLinqDataSource( qry.AsNoTracking() );
            gTransactions.DataBind();

            var showAccountSummary = this.GetAttributeValue( AttributeKey.ShowAccountSummary ).AsBoolean();
            if ( showAccountSummary ||
                _scheduledTxn == null &&
                _registration == null &&
                _person == null &&
                !isExporting )
            {
                pnlSummary.Visible = true;

                // No context - show account summary

                IQueryable<DetailInfo> qryAccountDetails;
                if ( hfTransactionViewMode.Value == "Transactions" )
                {
                    qryAccountDetails = qry.SelectMany( a => a.TransactionDetails );
                }
                else
                {
                    qryAccountDetails = qry.Select( a => a.TransactionDetail );
                }

                var accountSummaryQry1 = qryAccountDetails.Select( a => new
                {
                    Id = a.AccountId,
                    Amount = a.Amount
                } ).GroupBy( a => a.Id ).Select( a => new
                {
                    AccountId = a.Key,
                    TotalAmount = a.Sum( x => x.Amount )
                } );

                var summaryQryList = accountSummaryQry1.ToList().Select( a => new AccountSummaryRow
                {
                    AccountId = a.AccountId,
                    Order = FinancialAccountCache.Get( a.AccountId )?.Order ?? 0,
                    Name = FinancialAccountCache.Get( a.AccountId )?.Name,
                    TotalAmount = a.TotalAmount
                } );

                // check for filtered accounts
                var accountIds = ( gfTransactions.GetFilterPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
                if ( accountIds.Any() && apAccount.Visible )
                {
                    summaryQryList = summaryQryList.Where( a => accountIds.Contains( a.AccountId ) ).OrderBy( a => a.Order );
                    lbFiltered.Text = "Filtered Account List";
                    lbFiltered.Visible = true;
                }
                else
                {
                    lbFiltered.Visible = false;
                }

                var summaryList = summaryQryList.ToList();
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
        ///
        /// </summary>
        private class AccountSummaryRow
        {
            public int AccountId { get; internal set; }
            public string Name { get; internal set; }
            public int Order { get; internal set; }
            public decimal TotalAmount { get; set; }
        }

        /// <summary>
        /// Gets the accounts.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns></returns>
        private string GetAccounts( FinancialTransactionRow txn, bool isExporting )
        {
            if ( txn != null )
            {
                List<string> summary = null;
                if ( txn.TransactionDetail != null )
                {
                    string summaryLine = string.Format( "{0}: {1}", FinancialAccountCache.Get( txn.TransactionDetail.AccountId )?.Name, txn.TransactionDetail.Amount.FormatAsCurrency() );
                    summary = new List<string>();
                    summary.Add( summaryLine );
                }
                else if ( txn.TransactionDetails != null )
                {
                    var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
                    summary = txn.TransactionDetails.Select( a => new { Account = FinancialAccountCache.Get( a.AccountId ), a.Amount } )
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
                        d.Amount.FormatAsCurrency() ) )
                    .ToList();
                }

                if ( summary != null && summary.Any() )
                {
                    if ( isExporting )
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
            if ( _batch != null )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "BatchId", _batch.Id.ToString() );
                qryParams.Add( "TransactionId", id.ToString() );
                NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
            }
            else if ( _person != null )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "PersonId", _person.Id.ToString() );
                qryParams.Add( "TransactionId", id.ToString() );
                NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, "TransactionId", id );
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "REASSIGN":
                    dlgReassign.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "REASSIGN":
                    dlgReassign.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion Internal Methods

        /// <summary>
        /// Handles the SelectionChanged event of the bddlOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlOptions_SelectionChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnTransactions and btnTransactionDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTransactionsViewMode_Click( object sender, EventArgs e )
        {
            if ( sender == btnTransactions )
            {
                hfTransactionViewMode.Value = "Transactions";
            }
            else
            {
                hfTransactionViewMode.Value = "Transaction Details";
            }

            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( "TransactionViewMode", hfTransactionViewMode.Value );
            preferences.Save();

            BindFilter();
            BindGrid();
        }

        /// <summary>
        /// Special classes so that we can have a Transactions and TransactionDetail mode with minimal special case logic
        /// </summary>
        private class FinancialTransactionRow : RockDynamic
        {
            public int Id { get; set; }
            public int? AuthorizedPersonAliasId { get; internal set; }
            public string AuthorizedPersonLastName { get; set; }
            public string AuthorizedPersonNickName { get; set; }
            public int? BatchId { get; internal set; }
            public int? ScheduledTransactionId { get; internal set; }
            public DateTime TransactionDateTime { get; internal set; }
            public DateTime? FutureProcessingDateTime { get; internal set; }
            public int TransactionTypeValueId { get; internal set; }
            public PaymentDetailInfo FinancialPaymentDetail { get; internal set; }
            public string TransactionCode { get; internal set; }
            public string ForeignKey { get; internal set; }
            public int? SourceTypeValueId { get; internal set; }
            public decimal? TotalAmount { get; set; }
            public string Summary { get; set; }
            public string Status { get; set; }
            public DateTime? SettledDate { get; set; }
            public string SettledGroupId { get; set; }
            public int? FinancialGatewayId { get; set; }
            public bool? IsReconciled { get; set; }
            public bool? IsSettled { get; set; }
            public int? NonCashAssetTypeValueId { get; set; }
            public DateTime? ProcessedDateTime { get; set; }
            public bool ShowAsAnonymous { get; set; }
            public string StatusMessage { get; set; }

            /// <summary>
            /// NOTE: This will only be used in "Transaction Details" mode
            /// </summary>
            /// <value>
            /// The transaction detail.
            /// </value>
            public DetailInfo TransactionDetail { get; set; }

            /// <summary>
            /// NOTE: This will only be used in "Transaction" mode
            /// </summary>
            /// <value>
            /// The transaction details.
            /// </value>
            public IEnumerable<DetailInfo> TransactionDetails { get; set; }
            public int? ForeignCurrencyCodeValueId { get; set; }

            public IEnumerable<RefundInfo> Refunds { get; set; }

            public int? RefundForTransactionId { get; set; }
        }

        private class DetailInfo : RockDynamic
        {
            public int AccountId { get; internal set; }
            public decimal Amount { get; internal set; }
            public int? EntityId { get; internal set; }
            public int? EntityTypeId { get; internal set; }
        }

        private class PaymentDetailInfo : RockDynamic
        {
            public int Id { get; set; }
            public int? CreditCardTypeValueId { get; internal set; }
            public int? CurrencyTypeValueId { get; internal set; }
        }

        private class PersonDetail
        {
            public int PersonAliasId { get; set; }
            public int PersonId { get; set; }
            public string FullName { get; set; }
        }

        private class RefundInfo : RockDynamic
        {
            public int TransactionId { get; set; }
            public string TransactionCode { get; set; }
        }
    }
}