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

    [LinkedPage(
        name: "Detail Page",
        required: false,
        order: 0,
        key: AttributeKey.DetailPage )]

    [TextField(
        name: "Title",
        description: "Title to display above the grid. Leave blank to hide.",
        required: false,
        order: 1,
        key: AttributeKey.Title )]

    [BooleanField(
        name: "Show Only Active Accounts on Filter",
        description: "If account filter is displayed, only list active accounts",
        defaultValue: false,
        category: "",
        order: 2,
        key: AttributeKey.ActiveAccountsOnlyFilter )]

    [BooleanField(
        name: "Show Options",
        description: "Show an Options button in the title panel for showing images or summary.",
        defaultValue: false,
        order: 3,
        key: AttributeKey.ShowOptions )]

    [IntegerField(
        name: "Image Height",
        description: "If the Show Images option is selected, the image height",
        required: false,
        defaultValue: 200,
        order: 4,
        key: AttributeKey.ImageHeight )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        name: "Transaction Types",
        description: "Optional list of transaction types to limit the list to (if none are selected all types will be included).",
        required: false,
        allowMultiple: true,
        defaultValue: "",
        category: "",
        order: 5,
        key: AttributeKey.TransactionTypes )]

    [CustomDropdownListField(
        name: "Default Transaction View",
        description: "Select whether you want to initially see Transactions or Transaction Details",
        listSource: "Transactions,Transaction Details",
        required: false,
        defaultValue: "Transactions",
        category: "",
        order: 6,
        key: AttributeKey.DefaultTransactionView )]

    [LinkedPage(
        name: "Batch Page",
        required: false,
        order: 7,
        key: AttributeKey.BatchPage )]

    [BooleanField(
        name: "Show Foreign Key",
        description: "Should the transaction foreign key column be displayed?",
        defaultValue: false,
        order: 8,
        key: AttributeKey.ShowForeignKey )]

    [BooleanField(
        name: "Show Account Summary",
        description: "Should the account summary be displayed at the bottom of the list?",
        defaultValue: false,
        order: 9,
        key: AttributeKey.ShowAccountSummary )]

    [AccountsField(
        name: "Accounts",
        description: "Limit the results to transactions that match the selected accounts.",
        required: false,
        defaultValue: "",
        category: "",
        order: 10,
        key: AttributeKey.Accounts )]

    [BooleanField(
        name: "Show Future Transactions",
        description: "Should future transactions (transactions scheduled to be charged) be shown in this list?",
        defaultValue: false,
        order: 10,
        key: AttributeKey.ShowFutureTransactions )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        name: "Source Types",
        description: "Optional list of financial source types to limit the list to (if none are selected all types will be included).",
        required: false,
        allowMultiple: true,
        defaultValue: "",
        category: "",
        order: 11,
        key: AttributeKey.SourceTypes )]

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
        private Dictionary<int, FinancialAccount> _financialAccountLookup;
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
                var userViewMode = this.GetBlockUserPreference( "TransactionViewMode" );
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

            int currentBatchId = PageParameter( "batchId" ).AsInteger();

            if ( _canEdit )
            {
                _ddlMove.ID = "ddlMove";
                _ddlMove.CssClass = "pull-left input-width-xl";
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
                _lbReassign.CssClass = "btn btn-default btn-sm pull-left";
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
            gfTransactions.DeleteUserPreferences();
            BindFilter();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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
    $('#{0}').change(function( e ){{
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
", _ddlMove.ClientID, gTransactions.ClientID, Page.ClientScript.GetPostBackEventReference( this, "MoveTransactions" ), hfMoveToBatchId.ClientID);
                ScriptManager.RegisterStartupScript( _ddlMove, _ddlMove.GetType(), "moveTransaction", script, true );
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
                        var service = new FinancialAccountService( new RockContext() );
                        var accountNames = service.GetByIds( accountIds ).OrderBy( a => a.Order ).OrderBy( a => a.Name ).Select( a => a.Name ).ToList().AsDelimited( ", ", " or " );
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
            gfTransactions.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            gfTransactions.SaveUserPreference( "Amount Range", nreAmount.DelimitedValues );
            gfTransactions.SaveUserPreference( "Transaction Code", tbTransactionCode.Text );
            gfTransactions.SaveUserPreference( "Foreign Key", tbForeignKey.Text );
            gfTransactions.SaveUserPreference( "Account", apAccount.SelectedValue != All.Id.ToString() ? apAccount.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Transaction Type", dvpTransactionType.SelectedValue != All.Id.ToString() ? dvpTransactionType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Currency Type", dvpCurrencyType.SelectedValue != All.Id.ToString() ? dvpCurrencyType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Credit Card Type", dvpCreditCardType.SelectedValue != All.Id.ToString() ? dvpCreditCardType.SelectedValue : string.Empty );
            gfTransactions.SaveUserPreference( "Source Type", dvpSourceType.SelectedValue != All.Id.ToString() ? dvpSourceType.SelectedValue : string.Empty );

            // Campus of Batch
            gfTransactions.SaveUserPreference( "Campus", campCampusBatch.SelectedValue );

            // Campus of Account
            gfTransactions.SaveUserPreference( "CampusAccount", campCampusAccount.SelectedValue );

            gfTransactions.SaveUserPreference( "Person", ppPerson.SelectedValue.ToString() );

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
                            gfTransactions.SaveUserPreference( attribute.Key, attribute.Name, attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter ).ToJson() );
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
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var txn = e.Row.DataItem as FinancialTransactionRow;

                if ( txn != null )
                {
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
                                string imageSrc = string.Format( "~/GetImage.ashx?id={0}&height={1}", firstImageId, _imageHeight );
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
                    if ( lAccounts != null)
                    {
                        lAccounts.Text = this.GetAccounts( txn, isExporting );
                    }
                }
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
                            pageRef.Parameters.Add( "batchid", newBatch.Id.ToString() );
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
                                accountChanges.AddChange(History.HistoryVerb.Add, History.HistoryChangeType.Record, "Acct/Routing information" );


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
            drpDates.DelimitedValues = gfTransactions.GetUserPreference( "Date Range" );
            nreAmount.DelimitedValues = gfTransactions.GetUserPreference( "Amount Range" );

            if ( GetAttributeValue( AttributeKey.ShowForeignKey ).AsBoolean() )
            {
                tbForeignKey.Text = gfTransactions.GetUserPreference( "Foreign Key" );
                tbForeignKey.Visible = true;
            }
            else
            {
                tbForeignKey.Visible = false;
            }

            tbTransactionCode.Text = gfTransactions.GetUserPreference( "Transaction Code" );

            apAccount.Visible = string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.Accounts ) );
            apAccount.DisplayActiveOnly = GetAttributeValue( AttributeKey.ActiveAccountsOnlyFilter ).AsBoolean();

            var accountIds = ( gfTransactions.GetUserPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
            if ( accountIds.Any() )
            {
                var service = new FinancialAccountService( new RockContext() );
                var accounts = service.GetByIds( accountIds ).OrderBy( a => a.Order ).OrderBy( a => a.Name ).ToList();
                apAccount.SetValues( accounts );
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
                campCampusBatch.SetValue( gfTransactions.GetUserPreference( "Campus" ) );

                campCampusAccount.Campuses = campusi;
                campCampusAccount.SetValue( gfTransactions.GetUserPreference( "CampusAccount" ) );
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
                var personId = gfTransactions.GetUserPreference( "Person" ).AsIntegerOrNull();
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

            if ( !string.IsNullOrWhiteSpace( gfTransactions.GetUserPreference( userPreferenceKey ) ) )
            {
                dvpControl.SelectedValue = gfTransactions.GetUserPreference( userPreferenceKey );
            }
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters 
            _availableAttributes = new List<AttributeCache>();

            int entityTypeId = new FinancialTransaction().TypeId;
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
            if (summaryCol != null )
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

                        string savedValue = gfTransactions.GetUserPreference( attribute.Key );
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
                    var batchId = PageParameter( "batchId" );
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

            var rockContext = new RockContext();
            _financialAccountLookup = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().ToList().ToDictionary( k => k.Id, v => v );

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

                if ( sortProperty != null && sortProperty.Property == "_PERSONNAME_" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        financialTransactionDetailQry = financialTransactionDetailQry
                            .OrderBy( a => a.Transaction.AuthorizedPersonAlias.Person.LastName ).ThenBy( a => a.Transaction.AuthorizedPersonAlias.Person.NickName )
                            .ThenByDescending( t => t.Transaction.FutureProcessingDateTime ).ThenByDescending( t => t.Transaction.TransactionDateTime ).ThenByDescending( t => t.TransactionId );
                    }
                    else
                    {
                        financialTransactionDetailQry = financialTransactionDetailQry
                            .OrderByDescending( a => a.Transaction.AuthorizedPersonAlias.Person.LastName ).ThenByDescending( a => a.Transaction.AuthorizedPersonAlias.Person.NickName )
                            .ThenByDescending( t => t.Transaction.FutureProcessingDateTime ).ThenByDescending( t => t.Transaction.TransactionDateTime ).ThenByDescending( t => t.TransactionId );
                    }
                }

                qry = financialTransactionDetailQry.Select( a => new FinancialTransactionRow
                {
                    Id = a.TransactionId,
                    BatchId = a.Transaction.BatchId,
                    TransactionTypeValueId = a.Transaction.TransactionTypeValueId,
                    ScheduledTransactionId = a.Transaction.ScheduledTransactionId,
                    AuthorizedPersonAliasId = a.Transaction.AuthorizedPersonAliasId,
                    TransactionDateTime = a.Transaction.TransactionDateTime ?? a.Transaction.FutureProcessingDateTime.Value,
                    FutureProcessingDateTime = a.Transaction.FutureProcessingDateTime,
                    SourceTypeValueId = a.Transaction.SourceTypeValueId,
                    TotalAmount = a.Amount,
                    TransactionCode = a.Transaction.TransactionCode,
                    ForeignKey = a.Transaction.ForeignKey,
                    Status = a.Transaction.Status,
                    SettledDate = a.Transaction.SettledDate,
                    SettledGroupId = a.Transaction.SettledGroupId,
                    TransactionDetail = new DetailInfo
                    {
                        AccountId = a.AccountId,
                        Amount = a.Amount,
                        EntityId = a.EntityId,
                        EntityTypeId = a.EntityId
                    },
                    Summary = a.Transaction.FutureProcessingDateTime.HasValue ? "[charge pending] " + a.Summary : a.Transaction.Summary,
                    FinancialPaymentDetail = new PaymentDetailInfo
                    {
                        CreditCardTypeValueId = a.Transaction.FinancialPaymentDetail.CreditCardTypeValueId,
                        CurrencyTypeValueId = a.Transaction.FinancialPaymentDetail.CurrencyTypeValueId
                    }
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

                if ( sortProperty != null && sortProperty.Property == "_PERSONNAME_" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        financialTransactionQry = financialTransactionQry.OrderBy( a => a.AuthorizedPersonAlias.Person.LastName ).ThenBy( a => a.AuthorizedPersonAlias.Person.NickName )
                            .ThenByDescending( t => t.FutureProcessingDateTime ).ThenByDescending( t => t.TransactionDateTime ).ThenByDescending( t => t.Id );
                    }
                    else
                    {
                        financialTransactionQry = financialTransactionQry.OrderByDescending( a => a.AuthorizedPersonAlias.Person.LastName ).ThenByDescending( a => a.AuthorizedPersonAlias.Person.NickName )
                            .ThenByDescending( t => t.FutureProcessingDateTime ).ThenByDescending( t => t.TransactionDateTime ).ThenByDescending( t => t.Id );
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
                        TransactionDateTime = a.TransactionDateTime ?? a.FutureProcessingDateTime.Value,
                        FutureProcessingDateTime = a.FutureProcessingDateTime,
                        TransactionDetails = a.TransactionDetails.Select( d => new DetailInfo { AccountId = d.AccountId, Amount = d.Amount, EntityId = d.EntityId, EntityTypeId = d.EntityTypeId } ),
                        SourceTypeValueId = a.SourceTypeValueId,
                        TotalAmount = a.TransactionDetails.Sum( d => (decimal?)d.Amount ),
                        TransactionCode = a.TransactionCode,
                        ForeignKey = a.ForeignKey,
                        Status = a.Status,
                        SettledDate = a.SettledDate,
                        SettledGroupId = a.SettledGroupId,
                        Summary = a.FutureProcessingDateTime.HasValue ? "[charge pending] " + a.Summary : a.Summary,
                        FinancialPaymentDetail = new PaymentDetailInfo { CreditCardTypeValueId = a.FinancialPaymentDetail.CreditCardTypeValueId, CurrencyTypeValueId = a.FinancialPaymentDetail.CurrencyTypeValueId }
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
                drp.DelimitedValues = gfTransactions.GetUserPreference( "Date Range" );
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
                nre.DelimitedValues = gfTransactions.GetUserPreference( "Amount Range" );
                if ( nre.LowerValue.HasValue )
                {
                    qry = qry.Where( t => t.TotalAmount >= nre.LowerValue.Value );
                }

                if ( nre.UpperValue.HasValue )
                {
                    qry = qry.Where( t => t.TotalAmount <= nre.UpperValue.Value );
                }

                // Transaction Code
                string transactionCode = gfTransactions.GetUserPreference( "Transaction Code" );
                if ( !string.IsNullOrWhiteSpace( transactionCode ) )
                {
                    qry = qry.Where( t => t.TransactionCode == transactionCode.Trim() );
                }

                // Foreign Key
                if ( GetAttributeValue( AttributeKey.ShowForeignKey ).AsBoolean() )
                {
                    string foreingKey = gfTransactions.GetUserPreference( "Foreign Key" );
                    if ( !string.IsNullOrWhiteSpace( foreingKey ) )
                    {
                        qry = qry.Where( t => t.ForeignKey == foreingKey.Trim() );
                    }
                }

                // Account Id
                var accountIds = ( gfTransactions.GetUserPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
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
                if ( int.TryParse( gfTransactions.GetUserPreference( "Transaction Type" ), out transactionTypeId ) )
                {
                    qry = qry.Where( t => t.TransactionTypeValueId == transactionTypeId );
                }

                // Currency Type
                int currencyTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Currency Type" ), out currencyTypeId ) )
                {
                    qry = qry.Where( t => t.FinancialPaymentDetail != null && t.FinancialPaymentDetail.CurrencyTypeValueId == currencyTypeId );
                }

                // Credit Card Type
                int creditCardTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Credit Card Type" ), out creditCardTypeId ) )
                {
                    qry = qry.Where( t => t.FinancialPaymentDetail != null && t.FinancialPaymentDetail.CreditCardTypeValueId == creditCardTypeId );
                }

                // Source Type
                int sourceTypeId = int.MinValue;
                if ( int.TryParse( gfTransactions.GetUserPreference( "Source Type" ), out sourceTypeId ) )
                {
                    qry = qry.Where( t => t.SourceTypeValueId == sourceTypeId );
                }

                // Campus of Batch and/or Account
                if ( this.ContextEntity() == null )
                {
                    var campusOfBatch = CampusCache.Get( gfTransactions.GetUserPreference( "Campus" ).AsInteger() );
                    if ( campusOfBatch != null )
                    {
                        var qryBatchesForCampus = new FinancialBatchService( rockContext ).Queryable().Where( a => a.CampusId.HasValue && a.CampusId == campusOfBatch.Id ).Select( a => a.Id );
                        qry = qry.Where( t => qryBatchesForCampus.Contains( t.BatchId ?? 0 ) );
                    }
                    var campusOfAccount = CampusCache.Get( gfTransactions.GetUserPreference( "CampusAccount" ).AsInteger() );
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
                    var filterPersonId = gfTransactions.GetUserPreference( "Person" ).AsIntegerOrNull();
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

            // NOTE: We sort by _PERSONNAME_  above so don't do it here
            if ( sortProperty != null )
            {
                if ( sortProperty.Property != "_PERSONNAME_" )
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
                    .Select( a => new { a.TransactionId, a.BinaryFileId } ).GroupBy( a => a.TransactionId ).ToList()
                    .ToDictionary( k => k.Key, v => v.Select( x => x.BinaryFileId ).ToList() );
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
                    Order = _financialAccountLookup[a.AccountId].Order,
                    Name = _financialAccountLookup[a.AccountId].Name,
                    TotalAmount = a.TotalAmount
                } );

                // check for filtered accounts
                var accountIds = ( gfTransactions.GetUserPreference( "Account" ) ?? "" ).SplitDelimitedValues().AsIntegerList().Where( a => a > 0 ).ToList();
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
                    string summaryLine = string.Format( "{0}: {1}", _financialAccountLookup[txn.TransactionDetail.AccountId].Name, txn.TransactionDetail.Amount.FormatAsCurrency() );
                    summary = new List<string>();
                    summary.Add( summaryLine );
                }
                else if ( txn.TransactionDetails != null )
                {
                    var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
                    summary = txn.TransactionDetails.Select( a => new { Account = _financialAccountLookup[a.AccountId], a.Amount } )
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
                qryParams.Add( "batchId", _batch.Id.ToString() );
                qryParams.Add( "transactionId", id.ToString() );
                NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
            }
            else if ( _person != null )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "personId", _person.Id.ToString() );
                qryParams.Add( "transactionId", id.ToString() );
                NavigateToLinkedPage( AttributeKey.DetailPage, qryParams );
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, "transactionId", id );
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

            this.SetBlockUserPreference( "TransactionViewMode", hfTransactionViewMode.Value );

            BindGrid();
        }

        /// <summary>
        /// Special classes so that we can have a Transactions and TransactionDetail mode with minimal special case logic
        /// </summary>
        private class FinancialTransactionRow : DotLiquid.Drop
        {
            public int Id { get; set; }
            public int? AuthorizedPersonAliasId { get; internal set; }
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
        }

        private class DetailInfo : DotLiquid.Drop
        {
            public int AccountId { get; internal set; }
            public decimal Amount { get; internal set; }
            public int? EntityId { get; internal set; }
            public int? EntityTypeId { get; internal set; }
        }

        private class PaymentDetailInfo : DotLiquid.Drop
        {
            public int? CreditCardTypeValueId { get; internal set; }
            public int? CurrencyTypeValueId { get; internal set; }
        }

        private class PersonDetail
        {
            public int PersonAliasId { get; set; }
            public int PersonId { get; set; }
            public string FullName { get; set; }
        }
    }
}