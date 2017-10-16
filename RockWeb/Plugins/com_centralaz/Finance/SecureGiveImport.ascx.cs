// <copyright>
// Copyright by Central Christian Church
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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "SecureGive Import" )]
    [Category( "com_centralaz > Finance" )]
    [Description( "Finance block to import contribution data from a SecureGive XML file (FellowshipONE format)." )]

    [TextField( "Batch Name", "The name that should be used for the batches created", true, "SecureGive Import", order: 0 )]
    [IntegerField( "Anonymous Giver PersonAliasID", "PersonAliasId to use in case of anonymous giver", true, order: 1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "TransactionType", "The means the transaction was submitted by", true, order: 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Default Transaction Source", "The default transaction source to use if a match is not found (Website, Kiosk, etc.).", true, order:3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Default Tender Type Value", "The default tender type if a match is not found (Cash, Credit Card, etc.).", true, order: 4 )]
    [BooleanField( "Use Negative Foreign Keys", "Indicates whether Rock uses the negative of the SecureGive reference ID for the contribution record's foreign key", false, order: 5 )]
    [TextField( "Source Mappings", "Held in SecureGive's ContributionSource field, these correspond to Rock's TransactionSource (DefinedType). If you don't want to rename your current transaction source types, just map them here. Delimit them with commas or semicolons, and write them in the format 'SecureGive_value=Rock_value'.", false, "", "Data Mapping", 1 )]
    [TextField( "Tender Mappings", "Held in the SecureGive's ContributionType field, these correspond to Rock's TenderTypes (DefinedType). If you don't want to clutter your tender types, just map them here. Delimit them with commas or semicolons, and write them in the format 'SecureGive_value=Rock_value'.", false, "", "Data Mapping", 2 )]
    [TextField( "Fund Code Mapping", "Held in the SecureGive's FundCode field, these correspond to Rock's Account IDs (integer). Each FundCode should be mapped to a matching AccountId otherwise Rock will just use the same value. Delimit them with commas or semicolons, and write them in the format 'SecureGive_value=Rock_value'.", false, "", "Data Mapping", 3 )]    
    [LinkedPage( "Batch Detail Page", "The page used to display the contributions for a specific batch", true, "", "Linked Pages", 0 )]
    [LinkedPage( "Contribution Detail Page", "The page used to display the contribution transaction details", true, "", "Linked Pages", 1 )]
    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180, order: 1 )]

    public partial class SecureGiveImport : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int _anonymousPersonAliasId = 0;
        private FinancialBatch _financialBatch;
        private List<string> _errors = new List<string>();
        private List<XElement> _errorElements = new List<XElement>();

        private Dictionary<int, FinancialAccount> _financialAccountCache = new Dictionary<int, FinancialAccount>();
        private Dictionary<string, DefinedValue> _tenderTypeDefinedValueCache = new Dictionary<string, DefinedValue>();
        private Dictionary<string, DefinedValue> _transactionSourceTypeDefinedValueCache = new Dictionary<string, DefinedValue>();

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gContributions.GridRebind += gContributions_GridRebind;
            gContributions.RowDataBound += gContributions_RowDataBound;
            gErrors.GridRebind += gErrors_GridRebind;
            gErrors.RowDataBound += gErrors_RowDataBound;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            //// set postback timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            //// note: this only makes a difference on Postback, not on the initial page visit
            int databaseTimeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }

            ScriptManager scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.RegisterPostBackControl( lbImport );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                var id = GetAttributeValue( "AnonymousGiverPersonAliasID" ).AsIntegerOrNull();

                if ( id == null || string.IsNullOrEmpty( GetAttributeValue( "ContributionDetailPage" ) )  || string.IsNullOrEmpty( GetAttributeValue( "BatchDetailPage" ) ) )
                {
                    nbMessage.Text = "Invalid block settings.";
                    return;
                }

                tbBatchName.Text = GetAttributeValue( "BatchName" );
                BindCampusPicker();
                BindGrid();
                BindErrorGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbImport_Click( object sender, EventArgs e )
        {
            if ( fuImport.HasFile )
            {
                // clear any old errors:
                _errors = new List<string>();
                _errorElements = new List<XElement>();
                nbMessage.Text = "";
                pnlErrors.Visible = false;

                RockContext rockContext = new RockContext();
                FinancialBatchService financialBatchService = new FinancialBatchService( rockContext );
                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                PersonAliasService personAliasService = new PersonAliasService( rockContext );
                FinancialAccountService financialAccountService = new FinancialAccountService( rockContext );
                PersonService personService = new PersonService( rockContext );

                var transactionType = DefinedValueCache.Read( GetAttributeValue( "TransactionType" ).AsGuid() );
                var defaultTransactionSource = DefinedValueCache.Read( GetAttributeValue( "DefaultTransactionSource" ).AsGuid() );
                var tenderDefinedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() );
                var sourceTypeDefinedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() );

                // Find/verify the anonymous person alias ID
                var anonPersonAlias = personAliasService.GetByAliasId( GetAttributeValue( "AnonymousGiverPersonAliasID" ).AsInteger() );
                if ( anonPersonAlias == null )
                {
                    nbMessage.Text = "Invalid AnonymousGiverPersonAliasID block setting.";
                    return;
                }
                else
                {
                    _anonymousPersonAliasId = anonPersonAlias.Id;
                }

                _financialBatch = new FinancialBatch();
                _financialBatch.Name = tbBatchName.Text;
                _financialBatch.BatchStartDateTime = Rock.RockDateTime.Now;

                int? campusId = cpCampus.SelectedCampusId;

                if ( campusId != null )
                {
                    _financialBatch.CampusId = campusId;
                }
                else
                {
                    var campuses = CampusCache.All();
                    _financialBatch.CampusId = campuses.FirstOrDefault().Id;
                }

                financialBatchService.Add( _financialBatch );
                rockContext.SaveChanges();

                Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
                dictionaryInfo.Add( "batchId", _financialBatch.Id.ToString() );
                string url = LinkedPageUrl( "BatchDetailPage", dictionaryInfo );
                String theString = String.Format( "Batch <a href=\"{0}\">{1}</a> was created.", url, _financialBatch.Id.ToString() );
                nbBatch.Text = theString;
                nbBatch.Visible = true;

                var xdoc = XDocument.Load( System.Xml.XmlReader.Create( fuImport.FileContent ) );
                var elemDonations = xdoc.Element( "Donation" );

                Dictionary<String, String> tenderMappingDictionary = Regex.Matches( GetAttributeValue( "TenderMappings" ), @"\s*(.*?)\s*=\s*(.*?)\s*(;|,|$)" )
                    .OfType<Match>()
                    .ToDictionary( m => m.Groups[1].Value, m => m.Groups[2].Value );

                Dictionary<String, String> sourceMappingDictionary = Regex.Matches( GetAttributeValue( "SourceMappings" ), @"\s*(.*?)\s*=\s*(.*?)\s*(;|,|$)" )
                    .OfType<Match>()
                    .ToDictionary( m => m.Groups[1].Value, m => m.Groups[2].Value );

                Dictionary<int, int> fundCodeMappingDictionary = Regex.Matches( GetAttributeValue( "FundCodeMapping" ), @"\s*(.*?)\s*=\s*(.*?)\s*(;|,|$)" )
                    .OfType<Match>()
                    .ToDictionary( m => m.Groups[1].Value.AsInteger(), m => m.Groups[2].Value.AsInteger() );

                foreach ( var elemGift in elemDonations.Elements( "Gift" ) )
                {
                    ProcessGift( definedValueService, personAliasService, financialAccountService, transactionType, defaultTransactionSource, tenderDefinedType, sourceTypeDefinedType, tenderMappingDictionary, sourceMappingDictionary, fundCodeMappingDictionary, elemGift );
                }

                rockContext.SaveChanges();

                BindGrid();

                if ( _errors.Count > 0 )
                {
                    nbMessage.Text = "Errors found.";
                    BindErrorGrid();
                }

                _financialAccountCache = null;
                _tenderTypeDefinedValueCache = null;
            }
        }

        private void ProcessGift( DefinedValueService definedValueService, PersonAliasService personAliasService, FinancialAccountService financialAccountService, DefinedValueCache transactionType, DefinedValueCache defaultTransactionSource, DefinedTypeCache tenderDefinedType, DefinedTypeCache sourceTypeDefinedType, Dictionary<string, string> tenderMappingDictionary, Dictionary<string, string> sourceMappingDictionary, Dictionary<int, int> fundCodeMappingDictionary, XElement elemGift )
        {
            try
            {
                FinancialTransaction financialTransaction = new FinancialTransaction()
                {
                    TransactionTypeValueId = transactionType.Id,
                    SourceTypeValueId = defaultTransactionSource.Id
                };

                if ( elemGift.Element( "ReceivedDate" ) != null )
                {
                    financialTransaction.ProcessedDateTime = Rock.RockDateTime.Now;
                    financialTransaction.TransactionDateTime = elemGift.Element( "ReceivedDate" ).Value.AsDateTime();
                }

                // Map the Contribution Source to a Rock TransactionSource
                if ( elemGift.Element( "ContributionSource" ) != null )
                {
                    string transactionSourceElemValue = elemGift.Element( "ContributionSource" ).Value.ToString();

                    // Convert to mapped value if one exists...
                    if ( sourceMappingDictionary.ContainsKey( transactionSourceElemValue ) )
                    {
                        transactionSourceElemValue = sourceMappingDictionary[transactionSourceElemValue];
                    }

                    // Now find the matching source type...
                    // Get the source type and put in cache if we've not encountered it before.
                    if ( _transactionSourceTypeDefinedValueCache.ContainsKey( transactionSourceElemValue ) )
                    {
                        var transactionSourceDefinedValue = _transactionSourceTypeDefinedValueCache[transactionSourceElemValue];
                        financialTransaction.SourceTypeValueId = transactionSourceDefinedValue.Id;
                    }
                    else
                    {
                        DefinedValue transactionSourceDefinedValue;
                        int id;
                        transactionSourceDefinedValue = definedValueService.Queryable()
                            .Where( d => d.DefinedTypeId == sourceTypeDefinedType.Id && d.Value == transactionSourceElemValue )
                            .FirstOrDefault();
                        if ( transactionSourceDefinedValue != null )
                        {
                            _transactionSourceTypeDefinedValueCache.Add( transactionSourceElemValue, transactionSourceDefinedValue );
                            id = transactionSourceDefinedValue.Id;
                            financialTransaction.SourceTypeValueId = transactionSourceDefinedValue.Id;
                        }
                    }
                }

                // Map the Contribution Type to a Rock TenderType
                if ( elemGift.Element( "ContributionType" ) != null )
                {
                    string contributionTypeElemValue = elemGift.Element( "ContributionType" ).Value.ToString();

                    // Convert to mapped value if one exists...
                    if ( tenderMappingDictionary.ContainsKey( contributionTypeElemValue ) )
                    {
                        contributionTypeElemValue = tenderMappingDictionary[contributionTypeElemValue];
                    }

                    // set up the necessary Financial Payment Detail record
                    if ( financialTransaction.FinancialPaymentDetail == null )
                    {
                        financialTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                    }

                    // Now find the matching tender type...
                    // Get the tender type and put in cache if we've not encountered it before.
                    if ( _tenderTypeDefinedValueCache.ContainsKey( contributionTypeElemValue ) )
                    {
                        var tenderTypeDefinedValue = _tenderTypeDefinedValueCache[contributionTypeElemValue];
                        financialTransaction.FinancialPaymentDetail.CurrencyTypeValueId = tenderTypeDefinedValue.Id;
                    }
                    else
                    {
                        DefinedValue tenderTypeDefinedValue;
                        int id;
                        tenderTypeDefinedValue = definedValueService.Queryable()
                            .Where( d => d.DefinedTypeId == tenderDefinedType.Id && d.Value == contributionTypeElemValue )
                            .FirstOrDefault();
                        if ( tenderTypeDefinedValue != null )
                        {
                            _tenderTypeDefinedValueCache.Add( contributionTypeElemValue, tenderTypeDefinedValue );
                            id = tenderTypeDefinedValue.Id;
                        }
                        else
                        {
                            // otherwise get and use the tender type default value
                            id = DefinedValueCache.Read( GetAttributeValue( "DefaultTenderTypeValue" ).AsGuid() ).Id;
                        }
                        financialTransaction.FinancialPaymentDetail.CurrencyTypeValueId = id;
                    }
                }

                if ( elemGift.Element( "TransactionID" ) != null )
                {
                    financialTransaction.TransactionCode = elemGift.Element( "TransactionID" ).Value.ToString();
                }

                if ( elemGift.Element( "IndividualID" ) != null && !elemGift.Element( "IndividualID" ).IsEmpty )
                {
                    int aliasId = elemGift.Element( "IndividualID" ).Value.AsInteger();

                    // verify that this is a real person alias by trying to fetch it.
                    var personAlias = personAliasService.GetByAliasId( aliasId );
                    if ( personAlias == null )
                    {
                        throw new Exception( string.Format( "Invalid person alias Id {0}", elemGift.Element( "IndividualID" ).Value ) );
                    }

                    financialTransaction.AuthorizedPersonAliasId = personAlias.Id;
                }
                else
                {
                    financialTransaction.AuthorizedPersonAliasId = _anonymousPersonAliasId;
                }

                string summary = string.Format( "{0} donated {1} on {2}",
                    elemGift.Element( "ContributorName" ).IsEmpty ? "Anonymous" : elemGift.Element( "ContributorName" ).Value,
                    elemGift.Element( "Amount" ).Value.AsDecimal().ToString( "C" )
                    , financialTransaction.TransactionDateTime.ToString() );
                financialTransaction.Summary = summary;

                FinancialAccount account = new FinancialAccount();

                if ( elemGift.Element( "FundCode" ) != null )
                {
                    int accountId = elemGift.Element( "FundCode" ).Value.AsInteger();

                    // Convert to mapped value if one exists...
                    if ( fundCodeMappingDictionary.ContainsKey( accountId ) )
                    {
                        accountId = fundCodeMappingDictionary[accountId];
                    }

                    // look in cache to see if we already fetched it
                    if ( !_financialAccountCache.ContainsKey( accountId ) )
                    {
                        account = financialAccountService.Queryable()
                        .Where( fa => fa.Id == accountId )
                        .FirstOrDefault();
                        if ( account != null )
                        {
                            _financialAccountCache.Add( accountId, account );
                        }
                        else
                        {
                            throw new Exception( "Fund Code (Rock Account) not found." );
                        }
                    }
                    account = _financialAccountCache[accountId];
                }

                FinancialTransactionDetail financialTransactionDetail = new FinancialTransactionDetail()
                {
                    AccountId = account.Id
                };

                if ( elemGift.Element( "Amount" ) != null )
                {
                    financialTransactionDetail.Amount = elemGift.Element( "Amount" ).Value.AsDecimal();
                }

                if ( elemGift.Element( "ReferenceNumber" ) != null )
                {
                    if ( !GetAttributeValue( "UseNegativeForeignKeys" ).AsBoolean() )
                    {
                        financialTransactionDetail.Summary = elemGift.Element( "ReferenceNumber" ).Value.ToString();
                    }
                    else
                    {
                        financialTransactionDetail.Summary = ( elemGift.Element( "ReferenceNumber" ).Value.AsInteger() * -1 ).ToString();
                    }
                }

                financialTransaction.TransactionDetails.Add( financialTransactionDetail );
                _financialBatch.Transactions.Add( financialTransaction );
            }
            catch ( Exception ex )
            {
                _errors.Add( elemGift.Element( "ReferenceNumber" ).Value.ToString() );
                elemGift.Add( new XElement( "Error", ex.Message ) );
                _errorElements.Add( elemGift );
                return;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gContributions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gErrors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gErrors_GridRebind( object sender, EventArgs e )
        {
            BindErrorGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gErrors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gErrors_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var elemError = e.Row.DataItem as XElement;
                if ( elemError != null )
                {
                    Literal lReferenceNumber = e.Row.FindControl( "lReferenceNumber" ) as Literal;
                    if ( lReferenceNumber != null && elemError.Element( "ReferenceNumber" ) != null )
                    {
                        lReferenceNumber.Text = elemError.Element( "ReferenceNumber" ).Value.ToString();
                    }

                    Literal lChurchCode = e.Row.FindControl( "lChurchCode" ) as Literal;
                    if ( lChurchCode != null && elemError.Element( "ChurchCode" ) != null )
                    {
                        lChurchCode.Text = elemError.Element( "ChurchCode" ).Value.ToString();
                    }

                    Literal lIndividualId = e.Row.FindControl( "lIndividualId" ) as Literal;
                    if ( lIndividualId != null && elemError.Element( "IndividualID" ) != null )
                    {
                        lIndividualId.Text = elemError.Element( "IndividualID" ).Value.ToString();
                    }

                    Literal lContributorName = e.Row.FindControl( "lContributorName" ) as Literal;
                    if ( lContributorName != null && elemError.Element( "ContributorName" ) != null )
                    {
                        lContributorName.Text = elemError.Element( "ContributorName" ).Value.ToString();
                    }

                    Literal lFundName = e.Row.FindControl( "lFundName" ) as Literal;
                    if ( lFundName != null && elemError.Element( "FundName" ) != null )
                    {
                        lFundName.Text = elemError.Element( "FundName" ).Value.ToString();
                    }

                    Literal lFundCode = e.Row.FindControl( "lFundCode" ) as Literal;
                    if ( lFundCode != null && elemError.Element( "FundCode" ) != null )
                    {
                        lFundCode.Text = elemError.Element( "FundCode" ).Value.ToString();
                    }

                    Literal lReceivedDate = e.Row.FindControl( "lReceivedDate" ) as Literal;
                    if ( lReceivedDate != null && elemError.Element( "ReceivedDate" ) != null )
                    {
                        DateTime receivedDate = DateTime.Parse( elemError.Element( "ReceivedDate" ).Value );
                        lReceivedDate.Text = receivedDate.ToString();
                    }

                    Literal lAmount = e.Row.FindControl( "lAmount" ) as Literal;
                    if ( lAmount != null && elemError.Element( "Amount" ) != null )
                    {
                        lAmount.Text = elemError.Element( "Amount" ).Value.ToString();
                    }

                    Literal lTransactionId = e.Row.FindControl( "lTransactionId" ) as Literal;
                    if ( lTransactionId != null && elemError.Element( "TransactionID" ) != null )
                    {
                        lTransactionId.Text = elemError.Element( "TransactionID" ).Value.ToString();
                    }

                    Literal lContributionType = e.Row.FindControl( "lContributionType" ) as Literal;
                    if ( lContributionType != null && elemError.Element( "ContributionType" ) != null )
                    {
                        lContributionType.Text = elemError.Element( "ContributionType" ).Value.ToString();
                    }

                    Literal lError = e.Row.FindControl( "lError" ) as Literal;
                    if ( lError != null && elemError.Element( "Error" ) != null )
                    {
                        lError.Text = elemError.Element( "Error" ).Value.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gContributions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gContributions_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                FinancialTransactionDetail financialTransactionDetail = e.Row.DataItem as FinancialTransactionDetail;
                if ( financialTransactionDetail != null )
                {
                    Literal lTransactionID = e.Row.FindControl( "lTransactionID" ) as Literal;
                    if ( lTransactionID != null )
                    {
                        Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
                        dictionaryInfo.Add( "transactionId", financialTransactionDetail.TransactionId.ToString() );
                        string url = LinkedPageUrl( "ContributionDetailPage", dictionaryInfo );
                        String theString = String.Format( "<a href=\"{0}\">{1}</a>", url, financialTransactionDetail.TransactionId.ToString() );
                        lTransactionID.Text = theString;
                    }

                    Literal lFullName = e.Row.FindControl( "lFullName" ) as Literal;
                    if ( lFullName != null )
                    {
                        String url = ResolveUrl( string.Format( "~/Person/{0}", financialTransactionDetail.Transaction.AuthorizedPersonAlias.PersonId ) );
                        String theString = String.Format( "<a href=\"{0}\">{1}</a>", url, financialTransactionDetail.Transaction.AuthorizedPersonAlias.Person.FullName );
                        lFullName.Text = theString;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            nbMessage.Text = "";
            var id = GetAttributeValue( "AnonymousGiverPersonAliasID" ).AsIntegerOrNull();
            if ( id == null || string.IsNullOrEmpty( GetAttributeValue( "ContributionDetailPage" ) ) || string.IsNullOrEmpty( GetAttributeValue( "BatchDetailPage" ) ) )
            {
                nbMessage.Text = "Invalid block settings.";
                return;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the campus picker.
        /// </summary>
        private void BindCampusPicker()
        {
            // load campus dropdown
            var campuses = CampusCache.All();
            cpCampus.Campuses = campuses;
            cpCampus.Visible = campuses.Count > 1;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            FinancialTransactionDetailService financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );

            if ( _financialBatch != null )
            {
                var qry = financialTransactionDetailService.Queryable()
                                        .Where( ftd => ftd.Transaction.BatchId == _financialBatch.Id )
                                       .ToList();
                gContributions.DataSource = qry;
            }
            gContributions.DataBind();

            gContributions.Actions.ShowExcelExport = false;
            pnlGrid.Visible = gContributions.Rows.Count > 0;
        }

        /// <summary>
        /// Binds the error grid.
        /// </summary>
        private void BindErrorGrid()
        {
            RockContext rockContext = new RockContext();
            FinancialTransactionDetailService financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );

            if ( _errorElements.Count > 0 )
            {
                gErrors.DataSource = _errorElements;
            }

            gErrors.DataBind();

            if ( gErrors.Rows.Count > 0 )
            {
                pnlErrors.Visible = true;
                gErrors.Visible = true;
            }
        }

        #endregion
    }
}