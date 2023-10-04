using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.lakepointe.ReachDataImport.Model
{
    public class ReachDonationImportItem
    {
        private int mRowId = 0;

        public int RowId
        {
            get { return mRowId; }
            set { mRowId = value; }
        }
        public string All { get; set; }
        public string Date { get; set; }
        public string Supporter { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PreferredName { get; set; }
        public string Organization { get; set; }
        public string Permalink { get; set; }
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postal { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Groups { get; set; }
        public string Status { get; set; }
        public string PaymentType { get; set; }
        public string CheckNumber { get; set; }
        public string Confirmation { get; set; }
        public string Item { get; set; }
        public string Projects { get; set; }
        public string Places { get; set; }
        public string Sponsorship { get; set; }
        public string SponsorshipType { get; set; }
        public string ShareType { get; set; }
        public string Affiliate { get; set; }
        public string TaxDeductible { get; set; }
        public string ItemQuantity { get; set; }
        public string ItemAmount { get; set; }
        public string TransactionFees { get; set; }
        public string DeductibleAmount { get; set; }
        public string TotalAmount { get; set; }
        public string GiftAidAmount { get; set; }
        public string Currency { get; set; }
        public string BaseItemAmount { get; set; }
        public string BaseTrasactionFees { get; set; }
        public string BaseDeductibleAmount { get; set; }
        public string BaseTotalAmount { get; set; }
        public string BaseGiftAidAmount { get; set; }
        public string BaseCurrency { get; set; }
        public string ExchangeRate { get; set; }
        public string CardType { get; set; }
        public string CardExpiration { get; set; }
        public string RecurringPeriod { get; set; }
        public string NextDonation { get; set; }
        public string SoftCredits { get; set; }
        public string ExternalAPI { get; set; }
        public string HonoreeName { get; set; }
        public string HonoreeEmail { get; set; }
        public string HonoreeNote { get; set; }
        public string Notes { get; set; }
        public string AdminNotes { get; set; }
        public string AdminComments { get; set; }
        public string RockId { get; set; }
        public string ECSLifeGroups { get; set; }
        public string EgyptSponsors { get; set; }
        public string GCSLifeGroups { get; set; }
        public string GhanaSponsors { get; set; }
        public string HaitiSponsors { get; set; }
        public string KS { get; set; }
        public string LPESponsors { get; set; }
        public string MexicoSponsors { get; set; }
        public string MSCLifeGroups { get; set; }

        public ReachDonationImportItem() { }

        public static List<ReachDonationImportItem> LoadRawContributions( int binaryFileId, out string errorMessage )
        {
            var rawDonations = new List<ReachDonationImportItem>();
            errorMessage = null;

            var rockContext = new RockContext();
            var reachImportFileType = new BinaryFileTypeService( rockContext ).Get( SystemGuid.BinaryFileType.REACH_DONATION_IMPORT.AsGuid() );
            var importFile = new BinaryFileService( rockContext ).Get( binaryFileId );

            if ( importFile == null )
            {
                errorMessage = "Reach Import File Not Found.";
                return rawDonations;
            }

            if ( importFile.BinaryFileTypeId != reachImportFileType.Id )
            {
                errorMessage = "Reach Import File does not match expected type.";
                return rawDonations;
            }

            try
            {
                string importPath = String.Empty;
                if ( System.Web.HttpContext.Current != null )
                {
                    importPath = System.Web.HttpContext.Current.Server.MapPath( importFile.Path );
                }
                else
                {
                    importPath = importFile.Path;
                }

                using ( var reader = new StreamReader( importPath ) )
                {
                    using ( var csv = new CsvHelper.CsvReader( reader ) )
                    {
                        csv.Configuration.Delimiter = ",";
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.Quote = '\"';
                        csv.Configuration.RegisterClassMap( new ReachDontationImportMap() );
                        csv.Configuration.TrimFields = true;
                        csv.Configuration.WillThrowOnMissingField = false;
                

                        rawDonations = csv.GetRecords<ReachDonationImportItem>().ToList();
                    }
                }

                int i = 1;
                foreach ( var donation in rawDonations )
                {
                    donation.RowId = i;
                    i++;
                }
            }
            catch ( Exception ex )
            {
                errorMessage = string.Format( "An error occurred while loading Reach Contributions. {0}", ex.Message );
                throw ex;
            }

            return rawDonations;
        }
    }

    public class ChildSponsorshipPaymentTypes
    {
        public int DefinedValueID { get; set; }
        public string Value { get; set; }
        public string RegExFilter { get; set; }
        public int? CurrencyTypeId { get; set; }
        public bool IsActive { get; set; }

        public static List<ChildSponsorshipPaymentTypes> GetPaymentTypes( bool activeOnly = false )
        {
            var paymentTypeValues = DefinedTypeCache.Get( SystemGuid.DefinedType.REACH_PAYMENT_TYPES.AsGuid() ).DefinedValues;
            var paymentTypes = new List<ChildSponsorshipPaymentTypes>();
            var currencyTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ).DefinedValues;

            foreach ( var value in paymentTypeValues )
            {
                if ( activeOnly && !value.IsActive )
                {
                    continue;
                }

                var paymentType = new ChildSponsorshipPaymentTypes();
                paymentType.DefinedValueID = value.Id;
                paymentType.Value = value.Value;
                paymentType.RegExFilter = value.GetAttributeValue( "RegexFilter" );
                var currencyType = currencyTypes.First( ct => ct.Guid.Equals( value.GetAttributeValue( "CurrencyType" ).AsGuid() ) );

                if ( currencyType != null )
                {
                    paymentType.CurrencyTypeId = currencyType.Id;
                }

                paymentType.IsActive = value.IsActive;

                paymentTypes.Add( paymentType );
            }

            return paymentTypes;

        }
    }

    public class ChildSponsorshipCountry
    {
        public int DefinedValueId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int FinancialAccountId { get; set; }

        public static List<ChildSponsorshipCountry> GetCountries( RockContext context, bool activeOnly = false )
        {
            var countryValues = DefinedTypeCache.Get( SystemGuid.DefinedType.REACH_COUNTRY_MAPPING.AsGuid() ).DefinedValues;
            var countries = new List<ChildSponsorshipCountry>();
            var financialAccountService = new FinancialAccountService( context );

            foreach ( var value in countryValues )
            {
                if ( activeOnly && !value.IsActive )
                {
                    continue;
                }
                var country = new ChildSponsorshipCountry();
                country.DefinedValueId = value.Id;
                country.Name = value.Value;
                country.FinancialAccountId = financialAccountService.Get( value.GetAttributeValue( "FinancialAccount" ).AsGuid() ).Id;
                country.IsActive = value.IsActive;
                countries.Add( country );
            }

            return countries;
        }
    }

    public class ReachDontationImportMap : CsvClassMap<ReachDonationImportItem>
    {
        public ReachDontationImportMap()
        {
            AutoMap();
            Map( m => m.RowId ).Ignore();
            Map( m => m.FirstName ).Name( "First Name" );
            Map( m => m.LastName ).Name( "Last Name" );
            Map(m => m.PreferredName).Name("Preferred Name");
            Map( m => m.PaymentType ).Name( "Payment Type" );
            Map( m => m.CheckNumber ).Name( "Check Number" );
            Map( m => m.SponsorshipType ).Name( "Sponsorship Type" );
            Map( m => m.ShareType ).Name( "Share Type" );
            Map(m => m.TaxDeductible).Name("Tax Deductible");//.TypeConverter<BooleanYesNoConverter>();
            Map( m => m.ItemQuantity ).Name( "Item Quantity" );
            Map( m => m.ItemAmount ).Name( "Item Amount" );
            Map( m => m.TransactionFees ).Name( "Transaction Fees" );
            Map( m => m.DeductibleAmount ).Name( "Deductible Amount" );
            Map( m => m.TotalAmount ).Name( "Total Amount" );
            Map( m => m.GiftAidAmount ).Name( "Gift Aid Amount" );
            Map( m => m.BaseItemAmount ).Name( "Base Item Amount" );
            Map( m => m.BaseTrasactionFees ).Name( "Base Transaction Fees" );
            Map( m => m.BaseDeductibleAmount ).Name( "Base Deductible Amount" );
            Map( m => m.BaseTotalAmount ).Name( "Base Total Amount" );
            Map( m => m.BaseGiftAidAmount ).Name( "Base Gift Aid Amount" );
            Map( m => m.BaseCurrency ).Name( "Base Currency" );
            Map( m => m.ExchangeRate ).Name( "Exchange Rate" );
            Map( m => m.CardType ).Name( "Card Type" );
            Map(m => m.CardExpiration).Name("Card Expiration");
            Map( m => m.RecurringPeriod ).Name( "Recurring Period" );
            Map( m => m.NextDonation ).Name( "Next Donation" );
            Map( m => m.SoftCredits ).Name( "Soft Credits" );
            Map( m => m.ExternalAPI ).Name( "External API" );
            Map(m => m.HonoreeName).Name("Honoree Name");
            Map(m => m.HonoreeEmail).Name("Honoree Email");
            Map(m => m.HonoreeNote).Name("Honoree Note");
            Map( m => m.AdminNotes ).Name( "Admin Notes" );
            Map( m => m.AdminComments ).Name( "Admin Comments" );
            Map( m => m.RockId ).Name( "ROCK ID" );
            Map( m => m.ECSLifeGroups ).Name( "ECS Life Groups" );
            Map( m => m.EgyptSponsors ).Name( "Egypt Sponsors" );
            Map( m => m.GCSLifeGroups ).Name( "GCS Life Groups" );
            Map( m => m.GhanaSponsors ).Name( "Ghana Sponsors" );
            Map( m => m.HaitiSponsors ).Name( "Haiti Sponsors" );
            Map( m => m.KS ).Name( "K & S" );
            Map( m => m.LPESponsors ).Name( "LPE Sponsors" );
            Map( m => m.MexicoSponsors ).Name( "Mexico Sponsors" );
            Map( m => m.MSCLifeGroups ).Name( "MSC Life Groups" );

        }
    }

    public class BooleanYesNoConverter : CsvHelper.TypeConversion.DefaultTypeConverter
    {
        public override object ConvertFromString( TypeConverterOptions options, string text )
        {
            if ( text.IsNullOrWhiteSpace() )
            {
                return null;
            }
            bool value = false;
            if ( text.Equals( "yes", StringComparison.InvariantCultureIgnoreCase ) )
            {
                value = true;
            }

            return value;
        }
        public override string ConvertToString( TypeConverterOptions options, object value )
        {
            if ( value == null )
            {
                return null;
            }

            return ( (bool)value ) ? "Yes" : "No";
        }
    }
    public class DecimalNullableConveter : CsvHelper.TypeConversion.DefaultTypeConverter
    {
        public override object ConvertFromString( TypeConverterOptions options, string text )
        {
            return base.ConvertFromString( options, text );
        }
    }
    public class ReachDonation
    {
        private ReachDonationStatus mStatus = ReachDonationStatus.Pending;
        public int RowId { get; set; }
        public string ReachIdentifier
        {
            get
            {
                string id = null;
                if ( String.IsNullOrWhiteSpace( Confirmation ) )
                {
                    id = string.Format( "{0:yyyyMMdd}_{1}", TransactionDate, RowId );
                }
                else
                {
                    id = Confirmation;
                }
                return id;
            }
        }
        public int? PersonAliasId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public int? AccountId { get; set; }
        public int? CurrencyTypeID { get; set; }
        public int? FinancialTransactionId { get; set; }
        public int? ReachPaymentMethodId { get; set; }
        public string SponsorshipCode { get; set; }
        public string TransactionStatus { get; set; }
        public string Confirmation { get; set; }

        public bool HasBeenSettled
        {
            get { return TransactionStatus.Equals( "Complete", StringComparison.InvariantCultureIgnoreCase ); }
        }

        public ReachDonationStatus Status
        {
            get { return mStatus; }
            set { mStatus = value; }
        }
        public string StatusMessage { get; set; }
    }

    public enum ReachDonationStatus
    {
        Pending,
        Imported,
        Error,
        Previously_Downloaded

    }
}
