// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Globalization;
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
using CsvHelper;
using System.IO;
using System.Text;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Web;


namespace RockWeb.Plugins.com_bemaservices.CustomBlocks.SUCH.Finance
{
    [DisplayName( "Transaction Import" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Allows you to upload a CSV  and import all the Financial Transactions" )]
    [LinkedPage( "Batch Detail", "Page that displays the contents of a batch", true, Rock.SystemGuid.Page.FINANCIAL_BATCH_DETAIL )]
    [KeyValueListField( "Keyword Account Mapping", "This is the mapping for Kindrid Keywords to Rock Accounts", true, "UNDESIGNATED^1", "Keyword", "Rock Account Id", "", "", "Account", 2 )]
    [AccountField( "Default Account", "Default Financial Account that will be used when a match is not found or account is not specified.", true, Rock.SystemGuid.FinancialAccount.GENERAL_FUND, "Configuration", 0 )]
    [FinancialGatewayField( "Financial Gateway", "Financial Gateway all Kindrid transaction need to be tied to.", false, "", "Configuration",1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "Transaction Type", "Transaction Type, Transactions will be imported with.", true, false, Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION, "Configuration", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Currency Type", "Currency Type transaction will be imported with", true, false, Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_UNKNOWN, "Configuration", 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Transaction Source", "Source transaction will be imported with", true, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION, "Configuration", 4 )]
    public partial class TransactionImport : RockBlock
    {
        private List<KindridTransaction> kindridTransactions = new List<KindridTransaction>();
        private int _financialGatewayId;
        private int _financialTransactionTypeId;
        private int _financialTransactionSourceId;
        private int _currencyTypeId;
        private Dictionary<string, string> _keywordAccountMappings = new Dictionary<string, string>();
        private int _defaultAccountId;
        private decimal _totalAmount;
        private int _totalTransactions;
        public int _batchId;
        private int _matchedImports;
        private int _nonmatchedImports;
        private int _notImported;



        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            LoadBlockAttributes();
        }

        protected void LoadBlockAttributes()
        {
            RockContext rockContext = new RockContext();

            // Getting Financial Gateway Attribute
            if ( GetAttributeValue( "FinancialGateway" ).AsGuidOrNull() != null )
            {
                FinancialGatewayService financialGatewayService = new FinancialGatewayService( rockContext );
                financialGatewayService.Queryable();
                _financialGatewayId = financialGatewayService.Get( GetAttributeValue( "FinancialGateway" ).AsGuid() ).Id;
            }
            else
            {
                pEntry.Visible = false;
                pConfirmation.Visible = false;
                lMessages.Text = @"<div class='alert alert-warning'>Financial Gateway <strong>MUST</strong> be configured via Block Attributes.</div>";
            }

            // Getting Currency Type Attribute
            if ( GetAttributeValue( "CurrencyType" ).AsGuidOrNull() != null )
            {
                _currencyTypeId = DefinedValueCache.Read( GetAttributeValue( "CurrencyType" ).ToString() ).Id;
                
            }
            else
            {
                pEntry.Visible = false;
                pConfirmation.Visible = false;
                lMessages.Text = @"<div class='alert alert-warning'>Currency Type <strong>MUST</strong> be configured via Block Attributess.</div>";
            }

            // Getting Default Account Attribute
            if ( GetAttributeValue( "DefaultAccount" ).AsGuidOrNull() != null )
            {
                FinancialAccountService financialAccountService = new FinancialAccountService( rockContext );
                _defaultAccountId = financialAccountService.Get( GetAttributeValue( "DefaultAccount" ).AsGuid() ).Id;
            }
            else
            {
                pEntry.Visible = false;
                pConfirmation.Visible = false;
                lMessages.Text = @"<div class='alert alert-warning'>Default Account <strong>MUST</strong> be configured via Block Attributess.</div>";
            }

            // Getting Keyword Account Mapping Attribute
            if ( GetAttributeValue( "KeywordAccountMapping" ) != null )
            {
                _keywordAccountMappings = GetAttributeValue( "KeywordAccountMapping" ).AsDictionaryOrNull();
            }

            // Getting Transaction Type Attribute
            if ( GetAttributeValue( "TransactionType" ).AsGuidOrNull() != null )
            {
                _financialTransactionTypeId = DefinedValueCache.Read( GetAttributeValue( "TransactionType" ).AsGuid() ).Id;
            }

            // Getting Transaction Source Attribute
            if ( GetAttributeValue( "TransactionSource" ).AsGuidOrNull() != null )
            {
                _financialTransactionSourceId = DefinedValueCache.Read( GetAttributeValue( "TransactionSource" ).AsGuid() ).Id;
            }
        }

        /// <summary>
        /// Handles file uploads of CSV files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void FileUpload_FileUploaded( object sender, Rock.Web.UI.Controls.FileUploaderEventArgs e )
        {
            // Reload Block Attributes
            LoadBlockAttributes();

            BinaryFile binaryFile = LoadFileFromService();

            if ( binaryFile != null )
            {
                if ( !binaryFile.FileName.EndsWith( "csv" ) )
                {
                    pEntry.Visible = false;
                    pConfirmation.Visible = false;

                    // File is not a CSV
                    lMessages.Text = @"<div class='alert alert-warning'>Uploaded file must be a CSV.</div>";
                }
            }
        }

        /// <summary>
        /// Load the file from the Rock file service
        /// </summary>
        private BinaryFile LoadFileFromService()
        {
            RockContext rockContext = new RockContext();

            var binaryFileService = new BinaryFileService( rockContext );
            BinaryFile binaryFile = null;

            if ( FileUpload.BinaryFileId.HasValue )
            {
                binaryFile = binaryFileService.Get( FileUpload.BinaryFileId.Value );
            }

            if ( binaryFile != null )
            {
                if ( binaryFile.BinaryFileTypeId.HasValue )
                {
                    binaryFile.BinaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFile.BinaryFileTypeId.Value );

                    ConvertCSVIntoList( binaryFile, rockContext );
                }
            }
            else
            {
                lMessages.Text = @"<div class='alert alert-warning'>File appears to be blank.</div>";
            }

            return binaryFile;
        }

        private void ConvertCSVIntoList( BinaryFile binaryFile, RockContext rockContext )
        {
            var csvContents = binaryFile.ContentStream;
            TextReader textReader = new StreamReader( csvContents );

            using ( var csv = new CsvReader( textReader ) )
            {
                // Configuring Model
                csv.Configuration.RegisterClassMap<KindridTransactionsMap>();
                try
                {
                    kindridTransactions.AddRange( csv.GetRecords<KindridTransaction>().OrderByDescending( t => t.PostDate ).ToList() );

                    // Validating all transactions
                    kindridTransactions = ValidateTransactions( kindridTransactions, rockContext );

                    rTransactions.DataSource = kindridTransactions;
                    rTransactions.DataBind();

                    // Hiding Panels
                    pEntry.Visible = false;
                    pConfirmation.Visible = true;
                }
                catch ( Exception ex )
                {
                    lMessages.Text = @"<div class='alert alert-warning'>There was a problem parsing the CSV. Please validate the contents and try again.<br/>" + ex.Message + "</div>";
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                }
            }
        }

        /// <summary>
        /// Checks to see if transactions were previously imported or match existing people
        /// </summary>
        /// <param name="transactions">List of Kindrid Transactions</param>
        /// <param name="rockContext">A rockContext to use for querying the DB</param>
        private List<KindridTransaction> ValidateTransactions( List<KindridTransaction> transactions, RockContext rockContext )
        {
            FinancialTransactionService financialTransactionService = new FinancialTransactionService( rockContext );
            PersonService personService = new PersonService( rockContext );
            financialTransactionService.Queryable();
            personService.Queryable();
            AttributeValueService attributeValueService = new AttributeValueService(rockContext);


            _totalTransactions = transactions.Count;

            for ( var i = 0; i < transactions.Count; i++ )
            {
                // Checking to see if transaction was already imported
                //if ( financialTransactionService.GetByTransactionCode( _financialGatewayId, transactions[i].Id ) != null )
                //{
                //    transactions[i].PreviouslyImported = true;
                //    transactions[i].CurrentStatus = "Previously Imported";
                //}
                //else
                //{
                    transactions[i].PreviouslyImported = false;
                    transactions[i].CurrentStatus = "Person Not Matched";


                    //var person = personService.Queryable()
                    //    .Where(p => p.Attributes.AsQueryable(d => ) == transactions[i].Envelope)
                    //    .FirstOrDefault();

                    string test = transactions[i].Envelope;
                    var personAttEnvelope = attributeValueService.Queryable().Where(a => a.AttributeId == 3459 && a.Value == test).FirstOrDefault();

                    if (personAttEnvelope != null)
                    {
                        var person = personService.Get(personAttEnvelope.EntityId.Value);

                        transactions[i].RockPersonAliasId = person.Aliases.FirstOrDefault().Id;
                        transactions[i].CurrentStatus = "Person Matched";
                        transactions[i].MatchedPersonName = person.FullName;
                    }
                    
				
				string testFund = transactions[i].FundCode;
				var accountFundMatch = attributeValueService.Queryable().Where(a => a.AttributeId == 4521 && a.Value == testFund).FirstOrDefault();
				

				string transactionNonDeductible = transactions[i].TransactionType.Contains("Non-Deductible").ToTrueFalse();
                string accountName = transactions[i].FundDescription;

                transactions[i].TransactionType = transactionNonDeductible;
				
                if (transactionNonDeductible == "True")
                {
					if(accountFundMatch != null)
					{
						
					 var accountMatch = new FinancialAccountService(new RockContext()).Get(accountFundMatch.EntityId.Value);
						
					 if(accountMatch.IsTaxDeductible.ToTrueFalse() == "False")
					 {
						if (accountFundMatch == null)
							transactions[i].MatchedAccount = "No Account Matched";
						else
							transactions[i].MatchedAccount = accountFundMatch.EntityId.Value.ToString();
					 }
					 else
					 {
						accountName = transactions[i].FundDescription + " Non-Deductible";
						
						var accounts = new FinancialAccountService(new RockContext()).Queryable().Where(a => a.Name == accountName).FirstOrDefault();
						if (accounts == null)
							transactions[i].MatchedAccount = "No Account Matched";
						else
							transactions[i].MatchedAccount = accounts.Id.ToString();
					 }
					 
					}
					 else
					 {
						accountName = transactions[i].FundDescription + " Non-Deductible";
						
						var accounts = new FinancialAccountService(new RockContext()).Queryable().Where(a => a.Name == accountName).FirstOrDefault();
						if (accounts == null)
							transactions[i].MatchedAccount = "No Account Matched";
						else
							transactions[i].MatchedAccount = accounts.Id.ToString();
					 }
					

                }
				else 
				{
					if (accountFundMatch == null)
						transactions[i].MatchedAccount = "No Account Matched";
					else
						transactions[i].MatchedAccount = accountFundMatch.EntityId.Value.ToString();
				}
 

               
                    
                    //// Checking to see if person with same name exists
                    //List<PersonName> personNames = ParseName( transactions[i].Name );
                    //foreach ( var person in personNames )
                    //{
                    //    var personRecords = personService.GetByFirstLastName( person.FirstName, person.LastName, false, false ).ToList();
                    //    if( personRecords.Any() )
                    //    {
                    //        // Checking for Address Matches
                    //        var addressMatches = personRecords.Where( x => x.GetHomeLocation( rockContext ) != null &&
                    //                                                     x.GetHomeLocation( rockContext ).Street1 == transactions[i].DonorAddress ).ToList();

                    //        if ( addressMatches.Count == 1 )
                    //        {
                    //            transactions[i].RockPersonAliasId = addressMatches.First().PrimaryAliasId.Value;
                    //            transactions[i].CurrentStatus = "Person Matched";
                    //        }
                    //    }
                    //}
               // }
            }

            return transactions;
        }

        /// <summary>
        /// Parses Name string from kindrid file and converts into list on name(s)
        /// </summary>
        /// <param name="nameString">The string of names</param>
        private List<PersonName> ParseName( string nameString )
        {
            List<PersonName> personNames = new List<PersonName>();

            // Does personName contain two people
            if ( nameString.Contains( "&" ) || nameString.Contains( " and " ) )
            {
                string[] parts = null;

                if ( nameString.Contains( "&" ) )
                {
                    parts = nameString.Split( new string[] { " & " }, StringSplitOptions.None );
                }
                else if ( nameString.Contains( " and " ) )
                {
                    parts = nameString.Split( new string[] { " and " }, StringSplitOptions.None );
                }
                else
                {
                    throw new Exception( "Donor Name is formatted in an unapproved format." );
                }

                foreach ( var part in parts )
                {
                    PersonName personName = new PersonName();

                    // Finding first space
                    var space = part.IndexOf( ' ' );
                    if ( space > 0 )
                    {
                        personName.FirstName = part.Substring( 0, space );

                        // Finding last space
                        space = part.LastIndexOf( ' ' );
                        personName.LastName = part.Substring( space + 1, part.Count() - space - 1 );

                        // Pushing into list
                        personNames.Add( personName );
                    }
                }

                return personNames;
            }
            else
            {
                PersonName personName = new PersonName();

                // Finding first space
                var space = nameString.IndexOf( ' ' );
                personName.FirstName = nameString.Substring( 0, space );

                // Finding last space
                space = nameString.LastIndexOf( ' ' );
                personName.LastName = nameString.Substring( space + 1, nameString.Count() - space - 1 );

                // Pushing into list
                personNames.Add( personName );

                return personNames;
            }

        }

        /// <summary>
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImport_Click( object sender, EventArgs e )
        {
            // Resetting Total
            _totalAmount = 0.0M;
            _matchedImports = 0;
            _nonmatchedImports = 0;
            _notImported = 0;

            // Creating context and services
            using ( var rockContext = new RockContext() )
            {
                // Re-Loading transactions from Birnary Service
                BinaryFile binaryFile = LoadFileFromService();
                LoadBlockAttributes();

                var importableTransactions = kindridTransactions.Where( x => x.PreviouslyImported == false ).Count();
                if ( importableTransactions > 0 )
                {
                    FinancialBatchService financialBatchService = new FinancialBatchService( rockContext );

                    // Creating Batch
                    FinancialBatch financialBatch = new FinancialBatch();
                    financialBatch.BatchStartDateTime = DateTime.Today;
                    financialBatch.BatchEndDateTime = DateTime.Today.AddHours( 23 ).AddMinutes( 59 );
                    financialBatch.ControlAmount = 0;
                    financialBatch.CreatedByPersonAliasId = CurrentPersonAliasId;
                    financialBatch.CreatedDateTime = DateTime.Now;
                    financialBatch.Name = string.Format( "Giving Import - {0}", DateTime.Now.ToString( "MM/dd/yyyy HH:mm" ) );

                    // Saving Batch
                    financialBatchService.Add( financialBatch );
                    rockContext.SaveChanges();

                    _batchId = financialBatch.Id;

                    // Looping through all transactions
                    foreach ( var transaction in kindridTransactions )
                    {
                        ProcessStatus status = CreateTransaction( transaction, _batchId );

                        switch ( status )
                        {
                            case ProcessStatus.Matched:
                                _matchedImports++;
                                break;
                            case ProcessStatus.Unmatched:
                                _nonmatchedImports++;
                                break;
                            case ProcessStatus.NotImported:
                                _notImported++;
                                break;
                        }
                    }

                    // Getting Batch
                    var batch = financialBatchService.Get( _batchId );

                    // Updating batch Control Amount
                    batch.ControlAmount = _totalAmount;
                    rockContext.SaveChanges();

                    // Displaying results
                    SetResultsData();

                }
                else
                {
                    lMessages.Text = string.Format("<div class='row'><div class='alert alert-warning'>No Transactions to Import.<br/><strong>{0}</strong>/<strong>{0}</strong> transaction already imported</div></div>", kindridTransactions.Count );
                }

            }

        }

        /// <summary>
        /// Handles the creation of Financial Transactions
        /// </summary>
        /// <param name="transaction">The kindrid transaction object</param>
        /// <param name="batchId">The batch ID the transaction needs to be tied to</param>
        private ProcessStatus CreateTransaction( KindridTransaction transaction, int batchId )
        {
            ProcessStatus status;

            using ( var rockContext = new RockContext() )
            {
                if ( !transaction.PreviouslyImported )
                {
                    // Processing Kindrid Keyword
                    int accountId = _keywordAccountMappings.ContainsKey( transaction.FundDescription.ToUpper()) ? _keywordAccountMappings[transaction.FundDescription.ToUpper()].AsInteger() : _defaultAccountId;
                    int currencyId = _currencyTypeId;

                    string transactionNonDeductible = transaction.TransactionType;
                    string accountName = transaction.FundDescription;

					

                    if (transaction.MatchedAccount ==  "No Account Matched")
                        accountId = _defaultAccountId;
                    else
                        accountId = Int32.Parse(transaction.MatchedAccount);

                    var currency = new DefinedValueService(new RockContext()).Queryable().Where(a => a.DefinedTypeId == 10 && a.Value.Contains(transaction.PaymentType)).FirstOrDefault();

                    if (currency != null)
                        currencyId = currency.Id;

                    // Updating total counter
                    _totalAmount += Decimal.Parse(transaction.Amount, NumberStyles.Currency);
                    

                    // Creating Transaction
                    var newTransaction = new FinancialTransaction
                    {
                        TransactionCode = transaction.Id,
                        TransactionDateTime = transaction.PostDate,
                        FinancialGatewayId = _financialGatewayId,
                        TransactionTypeValueId = _financialTransactionTypeId,
                        BatchId = batchId,
                        SourceTypeValueId = _financialTransactionSourceId
                    };

                    // Creeating Payment Detail
                    var paymentDetail = new FinancialPaymentDetail
                    {
                        CreatedDateTime = transaction.PostDate,
                        ModifiedDateTime = transaction.PostDate,
                        CurrencyTypeValueId = currencyId
                    };

                    newTransaction.FinancialPaymentDetail = paymentDetail;

                    // Creating TransactionDetail
                    var newTransactionDetail = new FinancialTransactionDetail
                    {
                        Amount = Decimal.Parse(transaction.Amount, NumberStyles.Currency),
                        AccountId = accountId,
                        CreatedDateTime = transaction.PostDate
                    };

                    // Adding Transaction Detail onto Transaction
                    newTransaction.TransactionDetails.Add( newTransactionDetail );

                    if ( transaction.RockPersonAliasId != null )
                    {
                        newTransaction.AuthorizedPersonAliasId = transaction.RockPersonAliasId;
                        status = ProcessStatus.Matched;
                    }
                    else
                    {
                        status = ProcessStatus.Unmatched;
                    }

                    newTransaction.Summary = string.Format( "Donor Name: {0}\nDonor, Type: {1}, FundCode: {2}, Amount: {3}",
                                                   transaction.ImportedName,
                                                   transaction.PaymentType,
                                                   transaction.FundDescription,
                                                   transaction.Amount
                                          
                                                   
                                               );

                    // Creating Transaction in DB
                    rockContext.FinancialTransactions.Add( newTransaction );
                    rockContext.SaveChanges();

                    return status;
                }
                else
                {
                    return ProcessStatus.NotImported;
                }
            }
        }

        /// <summary>
        /// Handles the updating of results screen
        /// </summary>
        private void SetResultsData()
        {
            string matchedWidth;
            string nonmatchedWidth;
            string existingWidth;

            // Building widths for progress bars
            if ( _matchedImports > 0 )
                matchedWidth = Math.Truncate( ( _matchedImports / ( decimal ) _totalTransactions ) * 100 ) + "%";
            else
                matchedWidth = "0%";
            if ( _nonmatchedImports > 0 )
                nonmatchedWidth = Math.Truncate( ( _nonmatchedImports / ( decimal ) _totalTransactions ) * 100 ) + "%";
            else
                nonmatchedWidth = "0%";
            if ( _notImported > 0 )
                existingWidth = Math.Truncate( ( _notImported / ( decimal ) _totalTransactions ) * 100 ) + "%";
            else
                existingWidth = "0%";

            // Updating Progress Bar Widths
            progMatching.Style.Add( "width", matchedWidth );
            progMatching.InnerText = matchedWidth;
            progNotMatching.Style.Add( "width", nonmatchedWidth );
            progNotMatching.InnerText = nonmatchedWidth;
            progExisting.Style.Add( "width", existingWidth );
            progExisting.InnerText = existingWidth;

            // Updating Ratios
            lMatchingRatio.InnerText = string.Format( "{0}/{1}", _matchedImports, _totalTransactions );
            lNonMatchingRatio.InnerText = string.Format( "{0}/{1}", _nonmatchedImports, _totalTransactions );
            lNotImportedRatio.InnerText = string.Format( "{0}/{1}", _notImported, _totalTransactions );

            // Updating Text
            if ( GetAttributeValue( "BatchDetail" ).AsGuidOrNull() != null )
            {
                var pageId = PageCache.Read( GetAttributeValue( "BatchDetail" ).AsGuid() ).Id;
                lStatus.InnerHtml = string.Format( "<p>Below you will find the import statistics.");
                btnViewBatch.HRef = string.Format( "/page/{0}?batchId={1}", pageId.ToString(), _batchId );
                btnViewBatch.Visible = true;
            }
            else
            {
                btnViewBatch.Visible = false;
                lStatus.InnerHtml = "<p>Below you will find the import statistics.";
            }

            // Displaying Summary Panel
            pResults.Visible = true;
            pConfirmation.Visible = false;
        }

        private enum ProcessStatus
        {
            Matched = 0,
            Unmatched = 1,
            NotImported = 2
        }

    }

    public class KindridTransaction
    {
        //public string Id { get; set; }
        //public DateTime Date { get; set; }
        //public string Name { get; set; }
        //public string DonorAddress { get; set; }
        //public string DonorCity { get; set; }
        //public string DonorState { get; set; }
        //public int? DonorZip { get; set; }
        //public string DonorId { get; set; }
        //public string DonorEmail { get; set; }
        //public decimal GrossAmount { get; set; }
        //public decimal NetAmount { get; set; }
        //public decimal Fee { get; set; }
        //public string Number { get; set; }
        //public string Keyword { get; set; }
        //public string Status { get; set; }
        //public string Source { get; set; }
        //public string CCampus { get; set; }
        public bool PreviouslyImported { get; set; }
        public int? RockPersonAliasId { get; set; }
        public string CurrentStatus { get; set; }
        public string MatchedPersonName { get; set; }
		public string MatchedAccount { get; set; }

        public string Envelope { get; set; }
        public string ImportedName { get; set; }
        public DateTime PostDate { get; set; }
        public string PaymentType { get; set; }
		public string TransactionType { get; set; }
        public string FundCode { get; set; }
        public string FundDescription { get; set; }
        public string Amount { get; set; }
        public string SortField { get; set; }
        public string Id { get; set; }


    }

    public sealed class KindridTransactionsMap : CsvClassMap<KindridTransaction>
    {
        public KindridTransactionsMap()
        {
           // Map( m => m.Id ).Name("Individual ID");
            Map(m => m.PostDate).Name("Post Date");
            Map(m => m.PaymentType).Name("Payment Type");
			Map(m => m.TransactionType).Name("Transaction Type");
            Map(m => m.FundCode).Name("Fund Code");
            Map(m => m.FundDescription).Name("Fund Description");
            Map(m => m.Amount).Name("Gift Amount");
            //Map(m => m.SortField).Name("Sortfield");
            Map(m => m.Envelope).Name("Envelope Number");
            Map(m => m.ImportedName).Name("Label Name");
            //Map( m => m.Date ).Name( "Date" );
            //Map( m => m.Name ).Name( "Name" );
            //Map( m => m.DonorAddress ).Name( "Donor Address" );
            //Map( m => m.DonorCity ).Name( "Donor City" );
            //Map( m => m.DonorState ).Name( "Donor State" );
            //Map( m => m.DonorZip ).Name( "Donor Zip" );
            //Map( m => m.DonorId ).Name( "Donor Id" );
            //Map( m => m.DonorEmail ).Name( "Donor Email" );
            //Map( m => m.GrossAmount ).Name( "Gross Amount" );
            //Map( m => m.Fee ).Name( "Fee" );
            //Map( m => m.Number ).Name( "Number" );
            //Map( m => m.Keyword ).Name( "Keyword" );
            //Map( m => m.Status ).Name( "Status" );
            //Map( m => m.Source ).Name( "Source" );
            //Map( m => m.NetAmount ).Name( "Net Amount" );
            //Map( m => m.CCampus ).Name( "C Campus" );
        }
    }

    public class PersonName
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}