using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using LPC_Reach = org.lakepointe.ReachDataImport;
using org.lakepointe.ReachDataImport.Model;


namespace RockWeb.Plugins.org_lakepointe.Finance
{
    [DisplayName( "Reach Contribution Import" )]
    [Category( "Lake Pointe > Finance" )]
    [Description( "Imports Reach Contribution Transactions into RockRMS" )]
    [TextField( "Upload File Title", "Title for the Upload File panel.", false, "Upload File", "Appearance" )]
    [TextField( "File Detail Title", "Title for the File Detail panel.", false, "File Details", "Appearance" )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch.", true, "Child Sponsorship", "Financial", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Transaction Source", "The source for all financial transactions that are imported.", true, false, "", "Financial", 1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE, "Transaction Type", "The transaction type for all imported transactions.", true, false, Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION, "Financial", 2 )]
    [CustomRadioListField( "Error Action", "What action to perform when errors are found in data file.", "Stop^Cancel Upload,Skip^Skip Error Records", true, "Stop", "Actions" )]


    public partial class ReachDonationImport : RockBlock
    {

        List<ReachDonation> reachDonations;
        int mBatchId = 0;
        string mFileName = null;
        bool skipErrors = false;

        DefinedValueCache mTransactionType = null;
        DefinedValueCache mTransactionSource = null;


        #region Base Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.BlockUpdated += ReachDonationImport_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlReachImport );
        }



        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Set timeout for up to 30 minutes (just like installer)
            Server.ScriptTimeout = 1800;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 1800;

            if ( !Page.IsPostBack )
            {
                ConfigureControls();
                LoadDetails();
            }
        }
        #endregion

        #region Events
        private void ReachDonationImport_BlockUpdated( object sender, EventArgs e )
        {
            LoadDetails();
        }
        protected void btnUpload_Click( object sender, EventArgs e )
        {

            mTransactionSource = DefinedValueCache.Get( GetAttributeValue( "TransactionSource" ).AsGuid() );
            mTransactionType = DefinedValueCache.Get( GetAttributeValue( "TransactionType" ).AsGuid() );
            nbWarningMessage.Visible = false;
            int? binaryFileId = fuReachFile.BinaryFileId;
            string errorMessage = null;

            var rawDonations = ReachDonationImportItem.LoadRawContributions( binaryFileId.Value, out errorMessage );

            var rockContext = new RockContext();
            var personAliasService = new PersonAliasService( rockContext );
            var reachPaymentTypes = ChildSponsorshipPaymentTypes.GetPaymentTypes( true );
            var reachCountries = ChildSponsorshipCountry.GetCountries( rockContext, true );
            var reachAccounts = ReachImportFinancialAccount.LoadAccounts( rockContext );
            var financialTransactionService = new FinancialTransactionService( rockContext );

            mFileName = new BinaryFileService( rockContext ).Get( binaryFileId.Value ).FileName;

            reachDonations = new List<ReachDonation>();
            foreach ( var rawDonation in rawDonations )
            {
                var isValid = true;
                var donation = new ReachDonation();
                donation.RowId = rawDonation.RowId;
                var transactionDateTime = rawDonation.Date.AsDateTime();
                donation.TransactionDate = transactionDateTime.HasValue ? transactionDateTime.Value : DateTime.Now.Date;

                var transactionAmount = rawDonation.TotalAmount.AsDecimal();
                donation.TransactionAmount = transactionAmount;
                donation.TransactionStatus = rawDonation.Status;
                donation.Confirmation = rawDonation.Confirmation;


                if ( !String.IsNullOrWhiteSpace( rawDonation.Sponsorship ) )
                {
                    donation.SponsorshipCode = rawDonation.Sponsorship.Substring( 0, rawDonation.Sponsorship.IndexOf( " " ) );
                }

                if ( String.IsNullOrWhiteSpace( rawDonation.SponsorshipType ) && String.IsNullOrWhiteSpace( rawDonation.ShareType ) )
                {
                    donation.StatusMessage = "Sponsorship Type or Share Type is not valid.";
                    isValid = false;
                }


                if ( !String.IsNullOrWhiteSpace( rawDonation.ShareType ) )
                {
                    donation.AccountId = reachAccounts
                                            .Where( a => rawDonation.ShareType.Equals( a.GLCode, StringComparison.InvariantCultureIgnoreCase ) )
                                            .Select( a => a.AccountId )
                                            .FirstOrDefault();
                }

                ChildSponsorshipCountry country = null;
                if ( donation.AccountId == null )
                {
                    if ( !String.IsNullOrWhiteSpace( rawDonation.SponsorshipType ) )
                    {
                        country = reachCountries.FirstOrDefault( c => rawDonation.SponsorshipType.Contains( c.Name ) );

                        if ( country != null )
                        {
                            donation.AccountId = country.FinancialAccountId;
                        }
                    }
                }

                if ( isValid && !donation.AccountId.HasValue && donation.AccountId.Value <= 0  )
                {
                    donation.StatusMessage = "Account not found. Please verify Sponsorship Type and Share Type values.";
                    isValid = false;
                }

                foreach ( var payMethod in reachPaymentTypes )
                {
                    if ( System.Text.RegularExpressions.Regex.IsMatch( rawDonation.PaymentType, payMethod.RegExFilter ) )
                    {
                        donation.CurrencyTypeID = payMethod.CurrencyTypeId;
                        donation.ReachPaymentMethodId = payMethod.DefinedValueID;
                        break;
                    }
                }

                if ( !donation.CurrencyTypeID.HasValue )
                {
                    donation.StatusMessage = "Currency Type not valid.";
                    isValid = false;
                }

                var rockId = rawDonation.RockId.AsInteger();
                var pa = personAliasService.Queryable().AsNoTracking()
                     .Where( a => a.AliasPersonId == rockId )
                     .FirstOrDefault();
                if ( pa != null && pa.Id > 0 )
                {
                    donation.PersonAliasId = pa.Id;
                }
                else
                {
                    donation.StatusMessage = "Contributor not found.";
                    isValid = false;
                }

                if ( !isValid )
                {
                    donation.Status = ReachDonationStatus.Error;
                }

                var doesTransactionExist = financialTransactionService.Queryable().AsNoTracking()
                 .Where( t => t.AuthorizedPersonAliasId == donation.PersonAliasId )
                 .Where( t => t.TransactionTypeValueId == mTransactionType.Id )
                 .Where( t => t.SourceTypeValueId == mTransactionSource.Id )
                 .Where( t => t.TransactionCode == donation.ReachIdentifier )
                 .Count() > 0;

                if ( doesTransactionExist )
                {
                    donation.Status = ReachDonationStatus.Previously_Downloaded;
                }


                reachDonations.Add( donation );
            }
            var selectedPaymentTypeIds = dvpPaymentTypes.SelectedDefinedValuesId.ToList();
            selectedPaymentTypeIds.Add( -1 );  // want to keep the unknowns.
            reachDonations.RemoveAll( d => !selectedPaymentTypeIds.Contains( d.ReachPaymentMethodId ?? -1 ) );

            if ( GetAttributeValue( "ErrorAction" ).Equals( "Stop", StringComparison.InvariantCultureIgnoreCase )
                    && reachDonations.Where( d => d.Status == ReachDonationStatus.Error ).Count() > 0 )
            {
                skipErrors = false;
            }
            else
            {
                skipErrors = true;
                CreateFinancialTransactions();
            }

            DeleteImportFile( binaryFileId.Value );

            if ( !String.IsNullOrWhiteSpace( errorMessage ) )
            {
                nbWarningMessage.Visible = true;
                nbWarningMessage.Title = "File Import Error";
                nbWarningMessage.Text = errorMessage;

                return;
            }

            LoadDetails();
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( fuReachFile.BinaryFileId.HasValue )
            {
                DeleteImportFile( fuReachFile.BinaryFileId.Value );
            }
            LoadDetails();
        }


        #endregion

        #region Private Methods


        private void CreateFinancialTransactions()
        {
            var rockContext = new RockContext();


            var financialBatchService = new FinancialBatchService( rockContext );
            FinancialBatch batch = new FinancialBatch();
            batch.Name = string.Format( "{0} {1:yyyyMMdd}", GetAttributeValue( "BatchNamePrefix" ), dpBatchDate.SelectedDate );
            batch.BatchStartDateTime = dpBatchDate.SelectedDate;
            batch.BatchEndDateTime = dpBatchDate.SelectedDate.Value.AddDays( 1 );
            batch.Status = BatchStatus.Pending;
            financialBatchService.Add( batch );
            rockContext.SaveChanges( true );

            int counter = 0;
            decimal batchAmount = 0;
            var financialTransactionService = new FinancialTransactionService( rockContext );
            foreach ( var donation in reachDonations.Where( d => d.Status == ReachDonationStatus.Pending ) )
            {

                counter++;
                var financialTransaction = new FinancialTransaction();
                financialTransaction.BatchId = batch.Id;
                financialTransaction.TransactionDateTime = donation.TransactionDate;
                financialTransaction.AuthorizedPersonAliasId = donation.PersonAliasId;
                financialTransaction.SourceTypeValueId = mTransactionSource.Id;
                financialTransaction.TransactionTypeValueId = mTransactionType.Id;
                financialTransaction.TransactionCode = donation.ReachIdentifier;
                financialTransaction.Guid = Guid.NewGuid();
                financialTransaction.TransactionDetails = new List<FinancialTransactionDetail>();
                financialTransaction.TransactionDetails.Add( new FinancialTransactionDetail()
                {
                    AccountId = donation.AccountId.Value,
                    Amount = donation.TransactionAmount,
                    Guid = Guid.NewGuid(),
                } );
                financialTransaction.FinancialPaymentDetail = new FinancialPaymentDetail()
                {
                    CurrencyTypeValueId = donation.CurrencyTypeID,
                    Guid = Guid.NewGuid()
                };
                batchAmount += donation.TransactionAmount;
                financialTransactionService.Add( financialTransaction );

                donation.Status = ReachDonationStatus.Imported;

                if ( counter % 10 == 0 )
                {
                    rockContext.SaveChanges( true );
                }
            }

            batch = financialBatchService.Get( batch.Id );
            batch.ControlAmount = batchAmount;
            batch.Status = BatchStatus.Open;
            mBatchId = batch.Id;

            rockContext.SaveChanges( true );

        }

        private void ConfigureControls()
        {
            var rockContext = new RockContext();
            var reachPaymentTypeDefinedType = DefinedTypeCache.Get( LPC_Reach.SystemGuid.DefinedType.REACH_PAYMENT_TYPES.AsGuid() );

            if ( reachPaymentTypeDefinedType != null )
            {
                dvpPaymentTypes.DefinedTypeId = reachPaymentTypeDefinedType.Id;
            }

            fuReachFile.BinaryFileTypeGuid = LPC_Reach.SystemGuid.BinaryFileType.REACH_DONATION_IMPORT.AsGuid();

        }

        private void DeleteImportFile( int fileId )
        {

            var rockContext = new RockContext();
            var binaryFileSvc = new BinaryFileService( rockContext );
            var binaryfile = binaryFileSvc.Get( fileId );
            try
            {
                var path = HttpContext.Current.Server.MapPath( binaryfile.Path );
                System.IO.File.Delete( path );
            }
            catch
            {
                //intentionally do nothing
            }

            binaryFileSvc.Delete( binaryfile );
            rockContext.SaveChanges();

        }

        private void LoadFileDetails()
        {

            StringBuilder sbMessage = new StringBuilder();
            bool showBatch = false;
            if ( reachDonations.Count( d => d.Status == ReachDonationStatus.Error ) > 0 )
            {
                if ( !skipErrors )
                {
                    //has errors and import canceled
                    nbDetailMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbDetailMessage.Title = "<i class=\"fa fa-exclamation-triangle\"></i> File Import Canceled";

                    sbMessage.Append( "File not imported due to the following errors." );
                }
                else
                {
                    showBatch = true;
                    //has error with error records skipped
                    nbDetailMessage.NotificationBoxType = NotificationBoxType.Warning;
                    nbDetailMessage.Title = "<i class=\"fa fa-exclamation-triangle\"></i> File Imported With Errors";
                    sbMessage.Append( "File Imported with the following rows skipped." );
                }



                sbMessage.Append( "<ul type=\"disc\">" );
                foreach ( var item in reachDonations.Where( d => d.Status == ReachDonationStatus.Error ).OrderBy( d => d.RowId ) )
                {
                    sbMessage.AppendFormat( "<li>Row {0} - {1}</li>", item.RowId + 1, item.StatusMessage );
                }
                sbMessage.AppendFormat( "</ul>" );
                nbDetailMessage.Text = sbMessage.ToString();

            }
            else
            {
                showBatch = true;
                nbDetailMessage.NotificationBoxType = NotificationBoxType.Success;
                nbDetailMessage.Title = "<i class=\"fa fa-check\"></i>File Imported";
                nbDetailMessage.Text = "File imported successfully.";
            }
            pnlFileDetails.Visible = true;
            lFileDetailsTitle.Text = GetAttributeValue( "FileDetailTitle" );

            if ( !showBatch )
            {
                pnlBatch.Visible = false;
                return;
            }
            pnlBatch.Visible = true;
            var rockContext = new RockContext();
            var batch = new FinancialBatchService( rockContext ).Get( mBatchId );
            rlFileName.Text = mFileName;
            rlBatchDate.Text = batch.BatchStartDateTime.Value.ToShortDateString();
            rlBatchName.Text = batch.Name;
            rlTotalRows.Text = reachDonations.Count.ToString();
            rlErrors.Text = ( reachDonations.Count - reachDonations.Count( d => d.Status == ReachDonationStatus.Imported ) ).ToString();
            rlImportedTransactions.Text = reachDonations.Count( d => d.Status == ReachDonationStatus.Imported ).ToString();
        }

        private void LoadDetails()
        {
            pnlUploadFile.Visible = false;
            pnlFileDetails.Visible = false;

            if ( reachDonations == null || reachDonations.Count <= 0 )
            {
                LoadUploadPanel();
            }
            else
            {
                LoadFileDetails();
            }
        }

        private void LoadUploadPanel()
        {
            lUploadTitle.Text = GetAttributeValue( "UploadFileTitle" );
            dpBatchDate.SelectedDate = DateTime.Now.Date;
            fuReachFile.BinaryFileId = null;

            var reachPaymentTypes = DefinedTypeCache.Get( LPC_Reach.SystemGuid.DefinedType.REACH_PAYMENT_TYPES.AsGuid() ).DefinedValues;
            var preSelectedPaymentTypes = new List<int>();

            foreach ( var paymentType in reachPaymentTypes )
            {
                if ( paymentType.GetAttributeValue( "Import" ).AsBoolean() )
                {
                    preSelectedPaymentTypes.Add( paymentType.Id );
                }
            }

            dvpPaymentTypes.SelectedDefinedValuesId = preSelectedPaymentTypes.ToArray();
            pnlUploadFile.Visible = true;
        }
        #endregion  


    }

    public class ReachImportFinancialAccount
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string PublicName { get; set; }
        public string GLCode { get; set; }
        public bool IsActive { get; set; }


        public static List<ReachImportFinancialAccount> LoadAccounts( RockContext context )
        {
            var reachAccounts = new List<ReachImportFinancialAccount>();
            var entityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.FINANCIAL_ACCOUNT.AsGuid(), context ).Id;
            var revenueAcctAttribute = new AttributeService( context ).Get( entityTypeId, null, null, "rocks.kfs.ShelbyFinancials.CreditAccount" );

            if (revenueAcctAttribute == null)
            {
                return reachAccounts;
            }

            string acctSQL =
                @"SELECT 
                 fa.[Id] as AccountId,
                 fa.[Name],
                 fa.[PublicName],
                 revAcct.[Value] as GLCode,
                 fa.[IsActive] 
                FROM 
                 [FinancialAccount] fa
                INNER JOIN [AttributeValue] revAcct ON fa.Id = revAcct.EntityId and revAcct.AttributeId = @RevenueAccountAttributeId
                WHERE fa.[IsActive] = 1";


            reachAccounts.AddRange(
                    context.Database.SqlQuery<ReachImportFinancialAccount>( acctSQL,
                    new SqlParameter( "@RevenueAccountAttributeId", revenueAcctAttribute.Id ) ) );

            return reachAccounts;
        }
    }
}