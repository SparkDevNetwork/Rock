
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Text;


using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Web;

namespace RockWeb.Plugins.com_centralaz.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "SecureGive Import" )]
    [Category( "com_centralaz > Finance" )]
    [Description( "Finance block to import contribution data from a SecureGive XML file (FellowshipONE format)." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Transaction Source", "The transaction source", true )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "TransactionType", "The means the transaction was submitted by", true )]
    [IntegerField( "Anonymous Giver PersonAliasID", "PersonAliasId to use in case of anonymous giver", true )]
    [BooleanField( "Use Negative Foreign Keys", "Indicates whether Rock uses the negative of the SecureGive reference ID for the contribution record's foreign key", false )]
    [TextField( "Batch Name", "The name that should be used for the batches created", true, "SecureGive Import" )]
    [LinkedPage( "Batch Detail Page", "The page used to display the contributions for a specific batch", true, "", "", 0 )]
    [LinkedPage( "Contribution Detail Page", "The page used to display the contribution details", true, "", "", 1 )]
    public partial class SecureGiveImport : Rock.Web.UI.RockBlock
    {
        #region Fields

        private FinancialBatch _financialBatch;
        private List<string> _errors = new List<string>();
        private List<XElement> _errorElements = new List<XElement>();
        private Dictionary<int, FinancialAccount> _financialAccountCache = new Dictionary<int, FinancialAccount>();
        private Dictionary<string, DefinedValue> _tenderTypeDefinedValueCache = new Dictionary<string, DefinedValue>();
        private int _anonymousPersonAliasId = 0;
        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
                tbBatchName.Text = GetAttributeValue( "BatchName" );
                BindGrid();
                BindErrorGrid();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block
        protected void lbImport_Click( object sender, EventArgs e )
        {
            if ( fuImport.HasFile )
            {
                RockContext rockContext = new RockContext();
                FinancialBatchService financialBatchService = new FinancialBatchService( rockContext );
                DefinedValueService definedValueService = new DefinedValueService( rockContext );
                PersonAliasService personAliasService = new PersonAliasService( rockContext );

                // Find/verify the anonymous person alias ID
                var personAlias = personAliasService.GetByAliasId( GetAttributeValue( "AnonymousGiverPersonAliasID" ).AsInteger() );
                if ( personAlias == null )
                {
                    nbMessage.Text = "Invalid AnonymousGiverPersonAliasID block setting.";
                    return;
                }
                else
                {
                    _anonymousPersonAliasId = personAlias.Id;
                }

                _financialBatch = new FinancialBatch();
                _financialBatch.Name = tbBatchName.Text;
                _financialBatch.BatchStartDateTime = Rock.RockDateTime.Now;

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

                foreach ( var elemGift in elemDonations.Elements( "Gift" ) )
                {
                    ProcessGift( elemGift, rockContext );
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

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

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

        private void gErrors_GridRebind( object sender, EventArgs e )
        {
            BindErrorGrid();
        }

        #endregion

        #region Methods

        private void ProcessGift( XElement elemGift, RockContext rockContext )
        {
            FinancialAccountService financialAccountService = new FinancialAccountService( rockContext );
            DefinedValueService definedValueService = new DefinedValueService( rockContext );
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            PersonService personService = new PersonService( rockContext );

            var transactionSource = DefinedValueCache.Read( GetAttributeValue( "TransactionSource" ).AsGuid() );
            var transactionType = DefinedValueCache.Read( GetAttributeValue( "TransactionType" ).AsGuid() );
            var tenderType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() );

            try
            {
                FinancialTransaction financialTransaction = new FinancialTransaction()
                {
                    TransactionTypeValueId = transactionType.Id,
                    SourceTypeValueId = transactionSource.Id
                };

                if ( elemGift.Element( "ReceivedDate" ) != null )
                {
                    financialTransaction.ProcessedDateTime = Rock.RockDateTime.Now;
                    financialTransaction.TransactionDateTime = elemGift.Element( "ReceivedDate" ).Value.AsDateTime();
                }

                if ( elemGift.Element( "ContributionType" ) != null )
                {
                    string elemValue = elemGift.Element( "ContributionType" ).Value.ToString();
                    DefinedValue contributionTenderType;
                    // fetch the tender type from cache if we've encountered it before.
                    if ( !_tenderTypeDefinedValueCache.ContainsKey( elemValue ) )
                    {
                        contributionTenderType = definedValueService.Queryable()
                            .Where( d => d.DefinedTypeId == tenderType.Id && d.Value == elemValue )
                            .FirstOrDefault();
                        if ( contributionTenderType != null )
                        {
                            _tenderTypeDefinedValueCache.Add( elemValue, contributionTenderType );
                        }
                        else
                        {
                            throw new Exception( string.Format( "Unable to match contribution type {0}", elemValue ) );
                        }
                    }
                    else
                    {
                        contributionTenderType = _tenderTypeDefinedValueCache[elemValue];
                    }

                    if ( financialTransaction.FinancialPaymentDetail == null )
                    {
                        financialTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                    }

                    financialTransaction.FinancialPaymentDetail.CurrencyTypeValue = contributionTenderType;

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
                        throw new Exception( string.Format( "Invalid person alias Id {0}", aliasId ) );
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

                    // look in cache to see if we already fetched it
                    if ( !_financialAccountCache.ContainsKey( accountId ) )
                    {
                        account = financialAccountService.Queryable()
                        .Where( fa => fa.Id == accountId )
                        .FirstOrDefault();
                        _financialAccountCache.Add( accountId, account );
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
            catch ( Exception )
            {
                _errors.Add( elemGift.Element( "ReferenceNumber" ).Value.ToString() );
                _errorElements.Add( elemGift );
                return;
            }
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
        protected void gErrors_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var elemError = e.Row.DataItem as XElement;
                if ( elemError != null )
                {
                    Literal lReferenceNumber = e.Row.FindControl( "lReferenceNumber" ) as Literal;
                    if ( lReferenceNumber != null )
                    {
                        lReferenceNumber.Text = elemError.Element( "ReferenceNumber" ).Value.ToString();
                    }

                    Literal lChurchCode = e.Row.FindControl( "lChurchCode" ) as Literal;
                    if ( lChurchCode != null && elemError.Element( "ChurchCode" ) != null )
                    {
                        lChurchCode.Text = elemError.Element( "ChurchCode" ).Value.ToString();
                    }

                    Literal lIndividualId = e.Row.FindControl( "lIndividualId" ) as Literal;
                    if ( lIndividualId != null )
                    {
                        lIndividualId.Text = elemError.Element( "IndividualID" ).Value.ToString();
                    }

                    Literal lContributorName = e.Row.FindControl( "lContributorName" ) as Literal;
                    if ( lContributorName != null )
                    {
                        lContributorName.Text = elemError.Element( "ContributorName" ).Value.ToString();
                    }

                    Literal lFundName = e.Row.FindControl( "lFundName" ) as Literal;
                    if ( lFundName != null )
                    {
                        lFundName.Text = elemError.Element( "FundName" ).Value.ToString();
                    }

                    Literal lFundCode = e.Row.FindControl( "lFundCode" ) as Literal;
                    if ( lFundCode != null )
                    {
                        lFundCode.Text = elemError.Element( "FundCode" ).Value.ToString();
                    }

                    Literal lReceivedDate = e.Row.FindControl( "lReceivedDate" ) as Literal;
                    if ( lReceivedDate != null )
                    {
                        DateTime receivedDate = DateTime.Parse( elemError.Element( "ReceivedDate" ).Value );
                        lReceivedDate.Text = receivedDate.ToString();
                    }

                    Literal lAmount = e.Row.FindControl( "lAmount" ) as Literal;
                    if ( lAmount != null )
                    {
                        lAmount.Text = elemError.Element( "Amount" ).Value.ToString();
                    }

                    Literal lTransactionId = e.Row.FindControl( "lTransactionId" ) as Literal;
                    if ( lTransactionId != null )
                    {
                        lTransactionId.Text = elemError.Element( "TransactionID" ).Value.ToString();
                    }

                    Literal lContributionType = e.Row.FindControl( "lContributionType" ) as Literal;
                    if ( lContributionType != null )
                    {
                        lContributionType.Text = elemError.Element( "ContributionType" ).Value.ToString();
                    }
                }
            }
        }

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
    }
}