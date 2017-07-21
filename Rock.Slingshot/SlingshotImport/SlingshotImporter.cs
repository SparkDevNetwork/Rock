using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using CsvHelper;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{
    /// <summary>
    /// 
    /// </summary>
    public class Importer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Importer"/> class.
        /// </summary>
        /// <param name="slingshotFileName">Name of the slingshot file.</param>
        public Importer( string slingshotFileName )
        {
            SlingshotFileName = slingshotFileName;
            SlingshotDirectoryName = Path.Combine( Path.GetDirectoryName( this.SlingshotFileName ), "slingshots", Path.GetFileNameWithoutExtension( this.SlingshotFileName ) );

            var slingshotFilesDirectory = new DirectoryInfo( this.SlingshotDirectoryName );
            if ( slingshotFilesDirectory.Exists )
            {
                slingshotFilesDirectory.Delete( true );
            }

            slingshotFilesDirectory.Create();
            if ( File.Exists( this.SlingshotFileName ) )
            {
                ZipFile.ExtractToDirectory( this.SlingshotFileName, slingshotFilesDirectory.FullName );
            }

            this.Results = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use sample photos].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use sample photos]; otherwise, <c>false</c>.
        /// </value>
        public bool TEST_UseSampleRemotePhotos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [test use sample local photos].
        /// </summary>
        /// <value>
        /// <c>true</c> if [test use sample local photos]; otherwise, <c>false</c>.
        /// </value>
        public bool TEST_UseSampleLocalPhotos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [cancel photo import].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cancel photo import]; otherwise, <c>false</c>.
        /// </value>
        public bool CancelPhotoImport { get; set; }

        /// <summary>
        /// The sample photo urls
        /// </summary>
        private List<string> SamplePhotoRemoteUrls { get; set; } = new List<string>
        {
            { "http://storage.rockrms.com/sampledata/person-images/decker_ted.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/decker_cindy.png" },
            { "http://storage.rockrms.com/sampledata/person-images/decker_noah.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/decker_alexis.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/jones_ben.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/jones_brian.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/simmons_jim.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/simmons_sarah.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/jackson_mariah.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/lowe_madison.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/lowe_craig.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/lowe_tricia.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/marble_alisha.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/marble_bill.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/miller_tom.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/foster_peter.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/foster_pamela.jpg" },
            { "http://storage.rockrms.com/sampledata/person-images/michaels_jenny.jpg" }
        };

        private List<string> SamplePhotoLocalUrls
        {
            get
            {
                if ( _samplePhotoLocalUrls == null )
                {
                    _samplePhotoLocalUrls = Directory.GetFiles( @"C:\Users\admin\Downloads\slingshots\TESTPHOTOS", "*.jpg" ).ToList();
                }

                return _samplePhotoLocalUrls;
            }
        }

        List<string> _samplePhotoLocalUrls = null;

        /* Person Related */
        private Dictionary<Guid, DefinedValueCache> PersonRecordTypeValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> PersonRecordStatusValues { get; set; }

        private Dictionary<string, DefinedValueCache> PersonConnectionStatusValues { get; set; }

        private Dictionary<string, DefinedValueCache> PersonTitleValues { get; set; }

        private Dictionary<string, DefinedValueCache> PersonSuffixValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> PersonMaritalStatusValues { get; set; }

        private Dictionary<string, DefinedValueCache> PhoneNumberTypeValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> GroupLocationTypeValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> LocationTypeValues { get; set; }

        private Dictionary<string, AttributeCache> PersonAttributeKeyLookup { get; set; }

        private Dictionary<string, AttributeCache> FamilyAttributeKeyLookup { get; set; }

        private List<SlingshotCore.Model.PersonAttribute> SlingshotPersonAttributes { get; set; }

        private List<SlingshotCore.Model.FamilyAttribute> SlingshotFamilyAttributes { get; set; }

        private List<SlingshotCore.Model.Person> SlingshotPersonList { get; set; }

        /* Core  */
        private Dictionary<string, Rock.Model.FieldType> FieldTypeLookup { get; set; }

        // GroupType Lookup by ForeignId
        private Dictionary<int, Rock.Model.GroupType> GroupTypeLookupByForeignId { get; set; }

        /* Attendance */
        private List<SlingshotCore.Model.Attendance> SlingshotAttendanceList { get; set; }

        private List<SlingshotCore.Model.Group> SlingshotGroupList { get; set; }

        private List<SlingshotCore.Model.GroupType> SlingshotGroupTypeList { get; set; }

        private List<SlingshotCore.Model.Location> SlingshotLocationList { get; set; }

        private List<SlingshotCore.Model.Schedule> SlingshotScheduleList { get; set; }

        /* Financial Transactions */
        private List<SlingshotCore.Model.FinancialAccount> SlingshotFinancialAccountList { get; set; }

        private List<SlingshotCore.Model.FinancialBatch> SlingshotFinancialBatchList { get; set; }

        private List<SlingshotCore.Model.FinancialTransaction> SlingshotFinancialTransactionList { get; set; }

        private Dictionary<Guid, DefinedValueCache> TransactionSourceTypeValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> TransactionTypeValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> CurrencyTypeValues { get; set; }

        /* */
        private string SlingshotFileName { get; set; }

        private string SlingshotDirectoryName { get; set; }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>
        /// The results.
        /// </value>
        public Dictionary<string, string> Results { get; set; }

        /// <summary>
        /// The exceptions
        /// </summary>
        public List<Exception> Exceptions { get; set; } = new List<Exception>();

        /// <summary>
        /// Gets or sets the photo batch size mb.
        /// </summary>
        /// <value>
        /// The photo batch size mb.
        /// </value>
        public int? PhotoBatchSizeMB { get; set; }

        /// <summary>
        /// Gets or sets the size of the financial transaction chunk.
        /// Just in case the Target size reports a Timeout from the SqlBulkImport API.
        /// </summary>
        /// <value>
        /// The size of the financial transaction chunk.
        /// </value>
        public int? FinancialTransactionChunkSize { get; set; }

        public void ReportProgress( int progress, object progressData )
        {
            if ( OnProgress != null )
            {
                OnProgress( this, progressData );
            }
        }

        public event EventHandler<object> OnProgress;

        /// <summary>
        /// Does the import.
        /// </summary>
        public void DoImport()
        {
            // Load Slingshot Models from .slingshot
            this.ReportProgress( 0, "Loading Slingshot Models..." );
            LoadSlingshotLists();

            // Load Rock Lookups
            this.ReportProgress( 0, "Loading Rock Lookups..." );
            LoadLookups();

            EnsureDefinedValues();

            this.ReportProgress( 0, "Updating Rock Lookups..." );

            // Populate Rock with stuff that comes from the Slingshot file
            AddCampuses();
            AddConnectionStatuses();
            AddPersonTitles();
            AddPersonSuffixes();
            AddPhoneTypes();
            AddAttributeCategories();
            AddPersonAttributes();
            AddFamilyAttributes();

            AddGroupTypes();

            // load lookups again in case we added some new ones
            this.ReportProgress( 0, "Reloading Rock Lookups..." );
            LoadLookups();

            SubmitPersonImport();

            // Attendance Related
            SubmitLocationImport();
            SubmitGroupImport();
            SubmitScheduleImport();
            SubmitAttendanceImport();

            // Financial Transaction Related
            SubmitFinancialAccountImport();
            SubmitFinancialBatchImport();
            SubmitFinancialTransactionImport();
        }

        private const string PREPARE_PHOTO_DATA = "Prepare Photo Data:";
        private const string IMPORTING_PHOTO_DATA = "Importing Photo Data:";
        private const string UPLOAD_PHOTO_STATS = "Stats:";

        /// <summary>
        /// Does the import photos.
        /// </summary>
        public void DoImportPhotos()
        {
            this.Results.Clear();

            this.Results.Add( PREPARE_PHOTO_DATA, string.Empty );
            this.Results.Add( IMPORTING_PHOTO_DATA, string.Empty );
            this.Results.Add( UPLOAD_PHOTO_STATS, string.Empty );

            // Load Slingshot Models from .slingshot
            this.ReportProgress( 0, "Loading Person Slingshot Models..." );
            LoadPersonSlingshotLists();

            this.ReportProgress( 0, "Processing..." );

            if ( this.TEST_UseSampleLocalPhotos || this.TEST_UseSampleRemotePhotos )
            {
                var randomPhoto = new Random();
                var samplePhotoUrls = new List<string>();
                if ( this.TEST_UseSampleLocalPhotos )
                {
                    samplePhotoUrls.AddRange( this.SamplePhotoLocalUrls );
                }

                if ( this.TEST_UseSampleRemotePhotos )
                {
                    samplePhotoUrls.AddRange( this.SamplePhotoRemoteUrls );
                }

                int samplePhotoCount = samplePhotoUrls.Count();
                foreach ( var person in this.SlingshotPersonList )
                {
                    int randomPhotoIndex = randomPhoto.Next( samplePhotoCount );
                    person.PersonPhotoUrl = samplePhotoUrls[randomPhotoIndex];
                    randomPhotoIndex = randomPhoto.Next( samplePhotoCount );
                    person.FamilyImageUrl = samplePhotoUrls[randomPhotoIndex];
                }
            }

            var slingshotPersonsWithPhotoList = this.SlingshotPersonList.Where( a => !string.IsNullOrEmpty( a.PersonPhotoUrl ) || !string.IsNullOrEmpty( a.FamilyImageUrl ) ).ToList();

            var photoImportList = new ConcurrentBag<Rock.Slingshot.Model.PhotoImport>();

            HashSet<int> importedFamilyPhotos = new HashSet<int>();

            long photoLoadProgress = 0;
            long photoImportProgress = 0;
            int totalCount = slingshotPersonsWithPhotoList.Where( a => !string.IsNullOrWhiteSpace( a.PersonPhotoUrl ) ).Count()
                + slingshotPersonsWithPhotoList.Where( a => a.FamilyId.HasValue && !string.IsNullOrWhiteSpace( a.FamilyImageUrl ) ).Select( a => a.FamilyId ).Distinct().Count();

            int totalPhotoDataBytes = 0;
            if ( !this.PhotoBatchSizeMB.HasValue || this.PhotoBatchSizeMB.Value < 1 )
            {
                this.PhotoBatchSizeMB = 250;
            }

            int maxPhotoBatchSize = this.PhotoBatchSizeMB.Value * 1024 * 1024;
            foreach ( var slingshotPerson in slingshotPersonsWithPhotoList )
            {
                this.ReportProgress( 0, "Preparing Photos..." );
                if ( this.CancelPhotoImport )
                {
                    return;
                }

                if ( !string.IsNullOrEmpty( slingshotPerson.PersonPhotoUrl ) )
                {
                    var personPhotoImport = new Rock.Slingshot.Model.PhotoImport { PhotoType = Rock.Slingshot.Model.PhotoImport.PhotoImportType.Person };
                    personPhotoImport.ForeignId = slingshotPerson.Id;
                    if ( SetPhotoData( personPhotoImport, slingshotPerson.PersonPhotoUrl ) )
                    {
                        photoImportList.Add( personPhotoImport );
                    }

                    Interlocked.Increment( ref photoLoadProgress );
                }

                if ( !string.IsNullOrEmpty( slingshotPerson.FamilyImageUrl ) && slingshotPerson.FamilyId.HasValue )
                {
                    // make sure to only upload one photo per family
                    if ( !importedFamilyPhotos.Contains( slingshotPerson.FamilyId.Value ) )
                    {
                        importedFamilyPhotos.Add( slingshotPerson.FamilyId.Value );
                        var familyPhotoImport = new Rock.Slingshot.Model.PhotoImport { PhotoType = Rock.Slingshot.Model.PhotoImport.PhotoImportType.Family };
                        familyPhotoImport.ForeignId = slingshotPerson.FamilyId.Value;
                        if ( SetPhotoData( familyPhotoImport, slingshotPerson.FamilyImageUrl ) )
                        {
                            photoImportList.Add( familyPhotoImport );
                        }

                        Interlocked.Increment( ref photoLoadProgress );
                    }
                }

                totalPhotoDataBytes = photoImportList.Sum( a => a.PhotoData.Length );

                this.Results[PREPARE_PHOTO_DATA] = $"{Interlocked.Read( ref photoLoadProgress )} of {totalCount}";
                this.Results[IMPORTING_PHOTO_DATA] = $"{Interlocked.Read( ref photoImportProgress )} of {totalCount}";

                this.ReportProgress( 0, Results );

                if ( this.CancelPhotoImport )
                {
                    return;
                }

                if ( totalPhotoDataBytes > maxPhotoBatchSize )
                {
                    this.ReportProgress( 0, "Importing Photos..." );
                    var uploadList = photoImportList.ToList();
                    photoImportList = new ConcurrentBag<Rock.Slingshot.Model.PhotoImport>();
                    photoImportProgress += uploadList.Count();
                    UploadPhotoImports( uploadList );
                    this.Results[PREPARE_PHOTO_DATA] = $"{Interlocked.Read( ref photoLoadProgress )} of {totalCount}";
                    this.Results[IMPORTING_PHOTO_DATA] = $"{Interlocked.Read( ref photoImportProgress )} of {totalCount}";
                    this.ReportProgress( 0, Results );

                    GC.Collect();
                }
            }

            photoImportProgress += photoImportList.Count();

            UploadPhotoImports( photoImportList.ToList() );

            this.Results[PREPARE_PHOTO_DATA] = $"{Interlocked.Read( ref photoLoadProgress )} of {totalCount}";
            this.Results[IMPORTING_PHOTO_DATA] = $"{Interlocked.Read( ref photoImportProgress )} of {totalCount}";

            this.ReportProgress( 0, Results );
        }

        /// <summary>
        /// Uploads the photo imports.
        /// </summary>
        /// <param name="photoImportList">The photo import list.</param>
        /// <exception cref="SlingshotPOSTFailedException"></exception>
        private void UploadPhotoImports( List<Rock.Slingshot.Model.PhotoImport> photoImportList )
        {
            var result = BulkImportHelper.BulkPhotoImport( photoImportList );
            this.Results[UPLOAD_PHOTO_STATS] = result;
        }

        /// <summary>
        /// Gets the photo data.
        /// </summary>
        /// <param name="photoUrl">The photo URL.</param>
        /// <returns></returns>
        private bool SetPhotoData( Rock.Slingshot.Model.PhotoImport photoImport, string photoUrl )
        {
            Uri photoUri;
            if ( Uri.TryCreate( photoUrl, UriKind.Absolute, out photoUri ) && photoUri?.Scheme != "file" )
            {
                try
                {
                    HttpWebRequest imageRequest = ( HttpWebRequest ) HttpWebRequest.Create( photoUri );
                    HttpWebResponse imageResponse = ( HttpWebResponse ) imageRequest.GetResponse();
                    var imageStream = imageResponse.GetResponseStream();
                    using ( MemoryStream ms = new MemoryStream() )
                    {
                        imageStream.CopyTo( ms );
                        photoImport.MimeType = imageResponse.ContentType;
                        photoImport.PhotoData = Convert.ToBase64String( ms.ToArray(), Base64FormattingOptions.None );
                        photoImport.FileName = $"Photo{photoImport.ForeignId}";
                    }
                }
                catch ( Exception ex )
                {
                    Exceptions.Add( new Exception( "Photo Get Data Error " + photoUrl, ex ) );
                    return false;
                }
            }
            else
            {
                FileInfo photoFile = new FileInfo( photoUrl );
                if ( photoFile.Exists )
                {
                    photoImport.MimeType = System.Web.MimeMapping.GetMimeMapping( photoFile.FullName );
                    photoImport.PhotoData = Convert.ToBase64String( File.ReadAllBytes( photoFile.FullName ) );
                    photoImport.FileName = photoFile.Name;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        #region Financial Transaction Related

        /// <summary>
        /// Submits the financial account import.
        /// </summary>
        private void SubmitFinancialAccountImport()
        {
            this.ReportProgress( 0, "Preparing FinancialAccountImport..." );
            var financialAccountImportList = new List<Rock.Slingshot.Model.FinancialAccountImport>();
            var campusLookup = CampusCache.All().Where( a => a.ForeignId.HasValue ).ToDictionary( k => k.ForeignId.Value, v => v.Id );
            foreach ( var slingshotFinancialAccount in this.SlingshotFinancialAccountList )
            {
                var financialAccountImport = new Rock.Slingshot.Model.FinancialAccountImport();
                financialAccountImport.FinancialAccountForeignId = slingshotFinancialAccount.Id;

                financialAccountImport.Name = slingshotFinancialAccount.Name;
                if ( string.IsNullOrWhiteSpace( slingshotFinancialAccount.Name ) )
                {
                    financialAccountImport.Name = "Unnamed Financial Account";
                }

                financialAccountImport.IsTaxDeductible = slingshotFinancialAccount.IsTaxDeductible;

                if ( slingshotFinancialAccount.CampusId.HasValue )
                {
                    financialAccountImport.CampusId = campusLookup[slingshotFinancialAccount.CampusId.Value];
                }

                financialAccountImport.ParentFinancialAccountForeignId = slingshotFinancialAccount.ParentAccountId == 0 ? ( int? ) null : slingshotFinancialAccount.ParentAccountId;

                financialAccountImportList.Add( financialAccountImport );
            }

            this.ReportProgress( 0, "Bulk Importing FinancialAccounts..." );
            var result = BulkImportHelper.BulkFinancialAccountImport( financialAccountImportList );
            Results.Add( "FinancialAccount Import", result );
        }

        /// <summary>
        /// Submits the financial batch import.
        /// </summary>
        private void SubmitFinancialBatchImport()
        {
            this.ReportProgress( 0, "Preparing FinancialBatchImport..." );
            var financialBatchImportList = new List<Rock.Slingshot.Model.FinancialBatchImport>();
            var campusLookup = CampusCache.All().Where( a => a.ForeignId.HasValue ).ToDictionary( k => k.ForeignId.Value, v => v.Id );
            foreach ( var slingshotFinancialBatch in this.SlingshotFinancialBatchList )
            {
                var financialBatchImport = new Rock.Slingshot.Model.FinancialBatchImport();
                financialBatchImport.FinancialBatchForeignId = slingshotFinancialBatch.Id;

                financialBatchImport.Name = slingshotFinancialBatch.Name;
                if ( string.IsNullOrWhiteSpace( slingshotFinancialBatch.Name ) )
                {
                    financialBatchImport.Name = "Unnamed Financial Batch";
                }

                financialBatchImport.ControlAmount = slingshotFinancialBatch.ControlAmount;
                financialBatchImport.CreatedByPersonForeignId = slingshotFinancialBatch.CreatedByPersonId;
                financialBatchImport.CreatedDateTime = slingshotFinancialBatch.CreatedDateTime;
                financialBatchImport.EndDate = slingshotFinancialBatch.EndDate;
                financialBatchImport.ModifiedByPersonForeignId = slingshotFinancialBatch.ModifiedByPersonId;
                financialBatchImport.ModifiedDateTime = slingshotFinancialBatch.ModifiedDateTime;
                financialBatchImport.StartDate = slingshotFinancialBatch.StartDate;

                switch ( slingshotFinancialBatch.Status )
                {
                    case SlingshotCore.Model.BatchStatus.Closed:
                        financialBatchImport.Status = Rock.Slingshot.Model.FinancialBatchImport.BatchStatus.Closed;
                        break;
                    case SlingshotCore.Model.BatchStatus.Open:
                        financialBatchImport.Status = Rock.Slingshot.Model.FinancialBatchImport.BatchStatus.Open;
                        break;
                    case SlingshotCore.Model.BatchStatus.Pending:
                        financialBatchImport.Status = Rock.Slingshot.Model.FinancialBatchImport.BatchStatus.Pending;
                        break;
                }

                financialBatchImport.CampusId = slingshotFinancialBatch.CampusId.HasValue ? campusLookup[slingshotFinancialBatch.CampusId.Value] : ( int? ) null;

                financialBatchImportList.Add( financialBatchImport );
            }

            this.ReportProgress( 0, "Bulk Importing FinancialBatches..." );
            var result = BulkImportHelper.BulkFinancialBatchImport( financialBatchImportList );
            Results.Add( "FinancialBatch Import", result );
        }

        /// <summary>
        /// Submits the financial transaction import.
        /// </summary>
        private void SubmitFinancialTransactionImport()
        {
            this.ReportProgress( 0, "Preparing FinancialTransactionImport..." );
            var financialTransactionImportList = new List<Rock.Slingshot.Model.FinancialTransactionImport>();
            var campusLookup = CampusCache.All().Where( a => a.ForeignId.HasValue ).ToDictionary( k => k.ForeignId.Value, v => v.Id );
            foreach ( var slingshotFinancialTransaction in this.SlingshotFinancialTransactionList )
            {
                var financialTransactionImport = new Rock.Slingshot.Model.FinancialTransactionImport();
                financialTransactionImport.FinancialTransactionForeignId = slingshotFinancialTransaction.Id;

                financialTransactionImport.AuthorizedPersonForeignId = slingshotFinancialTransaction.AuthorizedPersonId;
                financialTransactionImport.BatchForeignId = slingshotFinancialTransaction.BatchId;

                switch ( slingshotFinancialTransaction.CurrencyType )
                {
                    case SlingshotCore.Model.CurrencyType.ACH:
                        financialTransactionImport.CurrencyTypeValueId = this.CurrencyTypeValues[Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.CurrencyType.Cash:
                        financialTransactionImport.CurrencyTypeValueId = this.CurrencyTypeValues[Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CASH.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.CurrencyType.Check:
                        financialTransactionImport.CurrencyTypeValueId = this.CurrencyTypeValues[Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.CurrencyType.CreditCard:
                        financialTransactionImport.CurrencyTypeValueId = this.CurrencyTypeValues[Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.CurrencyType.NonCash:
                        financialTransactionImport.CurrencyTypeValueId = this.CurrencyTypeValues[Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_NONCASH.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.CurrencyType.Unknown:
                        financialTransactionImport.CurrencyTypeValueId = this.CurrencyTypeValues[Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_UNKNOWN.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.CurrencyType.Other:
                        // TODO: Do we need to support this?
                        break;
                }

                switch ( slingshotFinancialTransaction.TransactionSource )
                {
                    case SlingshotCore.Model.TransactionSource.BankChecks:
                        financialTransactionImport.TransactionSourceValueId = this.TransactionSourceTypeValues[Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_BANK_CHECK.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.TransactionSource.Kiosk:
                        financialTransactionImport.TransactionSourceValueId = this.TransactionSourceTypeValues[Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_KIOSK.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.TransactionSource.MobileApplication:
                        financialTransactionImport.TransactionSourceValueId = this.TransactionSourceTypeValues[Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.TransactionSource.OnsiteCollection:
                        financialTransactionImport.TransactionSourceValueId = this.TransactionSourceTypeValues[Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.TransactionSource.Website:
                        financialTransactionImport.TransactionSourceValueId = this.TransactionSourceTypeValues[Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE.AsGuid()].Id;
                        break;
                }

                switch ( slingshotFinancialTransaction.TransactionType )
                {
                    case SlingshotCore.Model.TransactionType.Contribution:
                        financialTransactionImport.TransactionTypeValueId = this.TransactionTypeValues[Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.TransactionType.EventRegistration:
                        financialTransactionImport.TransactionTypeValueId = this.TransactionTypeValues[Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid()].Id;
                        break;
                }

                financialTransactionImport.FinancialTransactionDetailImports = new List<Rock.Slingshot.Model.FinancialTransactionDetailImport>();
                foreach ( var slingshotFinancialTransactionDetail in slingshotFinancialTransaction.FinancialTransactionDetails )
                {
                    var financialTransactionDetailImport = new Rock.Slingshot.Model.FinancialTransactionDetailImport();
                    financialTransactionDetailImport.FinancialAccountForeignId = slingshotFinancialTransactionDetail.AccountId;
                    financialTransactionDetailImport.Amount = slingshotFinancialTransactionDetail.Amount;
                    financialTransactionDetailImport.CreatedByPersonForeignId = slingshotFinancialTransactionDetail.CreatedByPersonId;
                    financialTransactionDetailImport.CreatedDateTime = slingshotFinancialTransactionDetail.CreatedDateTime;
                    financialTransactionDetailImport.FinancialTransactionDetailForeignId = slingshotFinancialTransactionDetail.Id;
                    financialTransactionDetailImport.ModifiedByPersonForeignId = slingshotFinancialTransactionDetail.ModifiedByPersonId;
                    financialTransactionDetailImport.ModifiedDateTime = slingshotFinancialTransactionDetail.ModifiedDateTime;
                    financialTransactionDetailImport.Summary = slingshotFinancialTransactionDetail.Summary;
                    financialTransactionImport.FinancialTransactionDetailImports.Add( financialTransactionDetailImport );
                }

                financialTransactionImport.Summary = slingshotFinancialTransaction.Summary;
                financialTransactionImport.TransactionCode = slingshotFinancialTransaction.TransactionCode;
                financialTransactionImport.TransactionDate = slingshotFinancialTransaction.TransactionDate;
                financialTransactionImport.CreatedByPersonForeignId = slingshotFinancialTransaction.CreatedByPersonId;
                financialTransactionImport.CreatedDateTime = slingshotFinancialTransaction.CreatedDateTime;
                financialTransactionImport.ModifiedByPersonForeignId = slingshotFinancialTransaction.ModifiedByPersonId;
                financialTransactionImport.ModifiedDateTime = slingshotFinancialTransaction.ModifiedDateTime;

                financialTransactionImportList.Add( financialTransactionImport );
            }

            int postChunkSize = this.FinancialTransactionChunkSize ?? int.MaxValue;

            while ( financialTransactionImportList.Any() )
            {
                var postChunk = financialTransactionImportList.Take( postChunkSize ).ToList();
                this.ReportProgress( 0, "Bulk Importing FinancialTransactions..." );
                var result = BulkImportHelper.BulkFinancialTransactionImport( postChunk );

                foreach ( var tran in postChunk.ToList() )
                {
                    financialTransactionImportList.Remove( tran );
                }

                if ( Results.ContainsKey( "FinancialTransaction Import" ) )
                {
                    Results["FinancialTransaction Import"] += "\n" + result;
                }
                else
                {
                    Results.Add( "FinancialTransaction Import", result );
                }
            }
        }

        #endregion Financial Transaction Related

        #region Attendance Related

        /// <summary>
        /// Submits the attendance import.
        /// </summary>
        private void SubmitAttendanceImport()
        {
            this.ReportProgress( 0, "Preparing AttendanceImport..." );
            var campusLookup = CampusCache.All().Where( a => a.ForeignId.HasValue ).ToDictionary( k => k.ForeignId.Value, v => v.Id );
            var attendanceImportList = new List<Rock.Slingshot.Model.AttendanceImport>();
            foreach ( var slingshotAttendance in this.SlingshotAttendanceList )
            {
                var attendanceImport = new Rock.Slingshot.Model.AttendanceImport();

                //// Note: There is no Attendance.Id in slingshotAttendance
                attendanceImport.PersonForeignId = slingshotAttendance.PersonId;
                attendanceImport.GroupForeignId = slingshotAttendance.GroupId;
                attendanceImport.LocationForeignId = slingshotAttendance.LocationId;
                attendanceImport.ScheduleForeignId = slingshotAttendance.ScheduleId;
                attendanceImport.StartDateTime = slingshotAttendance.StartDateTime;
                attendanceImport.EndDateTime = slingshotAttendance.EndDateTime;
                attendanceImport.Note = slingshotAttendance.Note;
                if ( slingshotAttendance.CampusId.HasValue )
                {
                    attendanceImport.CampusId = campusLookup[slingshotAttendance.CampusId.Value];
                }

                attendanceImportList.Add( attendanceImport );
            }

            this.ReportProgress( 0, "Bulk Importing Attendance..." );
            var result = BulkImportHelper.BulkAttendanceImport( attendanceImportList );

            Results.Add( "Attendance Import", result );
        }

        /// <summary>
        /// Submits the schedule import.
        /// </summary>
        private void SubmitScheduleImport()
        {
            this.ReportProgress( 0, "Preparing ScheduleImport..." );
            var scheduleImportList = new List<Rock.Slingshot.Model.ScheduleImport>();
            foreach ( var slingshotSchedule in this.SlingshotScheduleList )
            {
                var scheduleImport = new Rock.Slingshot.Model.ScheduleImport();
                scheduleImport.ScheduleForeignId = slingshotSchedule.Id;
                scheduleImport.Name = slingshotSchedule.Name;
                scheduleImportList.Add( scheduleImport );
            }

            this.ReportProgress( 0, "Bulk Importing Schedules..." );
            var result = BulkImportHelper.BulkScheduleImport( scheduleImportList );
            Results.Add( "Schedule Import", result );
        }

        /// <summary>
        /// Submits the location import.
        /// </summary>
        private void SubmitLocationImport()
        {
            this.ReportProgress( 0, "Preparing LocationImport..." );
            var locationImportList = new List<Rock.Slingshot.Model.LocationImport>();
            foreach ( var slingshotLocation in this.SlingshotLocationList )
            {
                var locationImport = new Rock.Slingshot.Model.LocationImport();
                locationImport.LocationForeignId = slingshotLocation.Id;
                locationImport.ParentLocationForeignId = slingshotLocation.ParentLocationId;
                locationImport.Name = slingshotLocation.Name;
                locationImport.IsActive = slingshotLocation.IsActive;

                // set LocationType to null since Rock usually leaves it null except for Campus, Building, and Room
                locationImport.LocationTypeValueId = null;
                locationImport.Street1 = slingshotLocation.Street1;
                locationImport.Street2 = slingshotLocation.Street2;
                locationImport.City = slingshotLocation.City;
                locationImport.County = slingshotLocation.County;
                locationImport.State = slingshotLocation.State;
                locationImport.Country = slingshotLocation.Country;
                locationImport.PostalCode = slingshotLocation.PostalCode;

                locationImportList.Add( locationImport );
            }

            this.ReportProgress( 0, "Bulk Importing Locations..." );
            var result = BulkImportHelper.BulkLocationImport( locationImportList );
            Results.Add( "Location Import", result );
        }

        /// <summary>
        /// Submits the group import.
        /// </summary>
        private void SubmitGroupImport()
        {
            this.ReportProgress( 0, "Preparing GroupImport..." );
            var groupImportList = new List<Rock.Slingshot.Model.GroupImport>();
            var campusLookup = CampusCache.All().Where( a => a.ForeignId.HasValue ).ToDictionary( k => k.ForeignId.Value, v => v.Id );
            foreach ( var slingshotGroup in this.SlingshotGroupList )
            {
                var groupImport = new Rock.Slingshot.Model.GroupImport();
                groupImport.GroupForeignId = slingshotGroup.Id;
                groupImport.GroupTypeId = this.GroupTypeLookupByForeignId[slingshotGroup.GroupTypeId].Id;

                groupImport.Name = slingshotGroup.Name;
                if ( string.IsNullOrWhiteSpace( slingshotGroup.Name ) )
                {
                    groupImport.Name = "Unnamed Group";
                }

                groupImport.Order = slingshotGroup.Order;
                if ( slingshotGroup.CampusId.HasValue )
                {
                    groupImport.CampusId = campusLookup[slingshotGroup.CampusId.Value];
                }

                groupImport.ParentGroupForeignId = slingshotGroup.ParentGroupId == 0 ? ( int? ) null : slingshotGroup.ParentGroupId;
                groupImport.GroupMemberImports = new List<Rock.Slingshot.Model.GroupMemberImport>();

                foreach ( var groupMember in slingshotGroup.GroupMembers )
                {
                    if ( !groupImport.GroupMemberImports.Any( gm => gm.PersonForeignId == groupMember.PersonId && gm.RoleName == groupMember.Role ) )
                    {
                        var groupMemberImport = new Rock.Slingshot.Model.GroupMemberImport();
                        groupMemberImport.PersonForeignId = groupMember.PersonId;
                        groupMemberImport.RoleName = groupMember.Role;
                        groupImport.GroupMemberImports.Add( groupMemberImport );
                    }
                }

                groupImportList.Add( groupImport );
            }

            this.ReportProgress( 0, "Bulk Importing Groups..." );
            var result = BulkImportHelper.BulkGroupImport( groupImportList );

            Results.Add( "Group Import", result );
        }

        #endregion Attendance Related

        #region Person Related

        /// <summary>
        /// Submits the person import.
        /// </summary>
        /// <param name="bwWorker">The bw worker.</param>
        private void SubmitPersonImport()
        {
            this.ReportProgress( 0, "Preparing PersonImport..." );
            List<Rock.Slingshot.Model.PersonImport> personImportList = GetPersonImportList();

            this.ReportProgress( 0, "Bulk Importing Person..." );
            var result = BulkImportHelper.BulkPersonImport( personImportList );

            Results.Add( "Person Import", result );
        }

        /// <summary>
        /// Gets the person import list.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">personImport.PersonForeignId must be greater than 0
        /// or
        /// personImport.FamilyForeignId must be greater than 0 or null
        /// or</exception>
        private List<Rock.Slingshot.Model.PersonImport> GetPersonImportList()
        {
            List<Rock.Slingshot.Model.PersonImport> personImportList = new List<Rock.Slingshot.Model.PersonImport>();

            var familyRolesLookup = GroupTypeCache.GetFamilyGroupType().Roles.ToDictionary( k => k.Guid );


            foreach ( var slingshotPerson in this.SlingshotPersonList )
            {
                var personImport = new Rock.Slingshot.Model.PersonImport();
                personImport.RecordTypeValueId = this.PersonRecordTypeValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()].Id;
                personImport.PersonForeignId = slingshotPerson.Id;
                personImport.FamilyForeignId = slingshotPerson.FamilyId;

                if ( personImport.PersonForeignId <= 0 )
                {
                    throw new Exception( "personImport.PersonForeignId must be greater than 0" );
                }

                if ( personImport.FamilyForeignId <= 0 )
                {
                    throw new Exception( "personImport.FamilyForeignId must be greater than 0 or null" );
                }

                personImport.FamilyName = slingshotPerson.FamilyName;

                switch ( slingshotPerson.FamilyRole )
                {
                    case SlingshotCore.Model.FamilyRole.Adult:
                        personImport.GroupRoleId = familyRolesLookup[Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.FamilyRole.Child:
                        personImport.GroupRoleId = familyRolesLookup[Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid()].Id;
                        break;
                }

                if ( !string.IsNullOrEmpty( slingshotPerson.Campus?.CampusName ) )
                {
                    var lookupCampus = CampusCache.All().Where( a => a.Name.Equals( slingshotPerson.Campus.CampusName, StringComparison.OrdinalIgnoreCase ) ).FirstOrDefault();
                    personImport.CampusId = lookupCampus?.Id;
                }

                switch ( slingshotPerson.RecordStatus )
                {
                    case SlingshotCore.Model.RecordStatus.Active:
                        personImport.RecordStatusValueId = this.PersonRecordStatusValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid()]?.Id;
                        break;
                    case SlingshotCore.Model.RecordStatus.Inactive:
                        personImport.RecordStatusValueId = this.PersonRecordStatusValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid()]?.Id;
                        break;
                    case SlingshotCore.Model.RecordStatus.Pending:
                        personImport.RecordStatusValueId = this.PersonRecordStatusValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid()]?.Id;
                        break;
                }

                personImport.InactiveReasonNote = slingshotPerson.InactiveReason;

                if ( !string.IsNullOrEmpty( slingshotPerson.ConnectionStatus ) )
                {
                    personImport.ConnectionStatusValueId = this.PersonConnectionStatusValues[slingshotPerson.ConnectionStatus]?.Id;
                }

                if ( !string.IsNullOrEmpty( slingshotPerson.Salutation ) )
                {
                    personImport.TitleValueId = this.PersonTitleValues[slingshotPerson.Salutation]?.Id;
                }

                if ( !string.IsNullOrEmpty( slingshotPerson.Suffix ) )
                {
                    personImport.SuffixValueId = this.PersonSuffixValues[slingshotPerson.Suffix]?.Id;
                }

                personImport.IsDeceased = slingshotPerson.IsDeceased;

                personImport.FirstName = slingshotPerson.FirstName;
                personImport.NickName = slingshotPerson.NickName;
                personImport.MiddleName = slingshotPerson.MiddleName;
                personImport.LastName = slingshotPerson.LastName;

                if ( slingshotPerson.Birthdate.HasValue )
                {
                    personImport.BirthMonth = slingshotPerson.Birthdate.Value.Month;
                    personImport.BirthDay = slingshotPerson.Birthdate.Value.Day;
                    personImport.BirthYear = slingshotPerson.Birthdate.Value.Year == slingshotPerson.BirthdateNoYearMagicYear ? ( int? ) null : slingshotPerson.Birthdate.Value.Year;
                }

                switch ( slingshotPerson.Gender )
                {
                    case SlingshotCore.Model.Gender.Male:
                        personImport.Gender = ( int ) Rock.Model.Gender.Male;
                        break;
                    case SlingshotCore.Model.Gender.Female:
                        personImport.Gender = ( int ) Rock.Model.Gender.Female;
                        break;
                    case SlingshotCore.Model.Gender.Unknown:
                        personImport.Gender = ( int ) Rock.Model.Gender.Unknown;
                        break;
                }

                switch ( slingshotPerson.MaritalStatus )
                {
                    case SlingshotCore.Model.MaritalStatus.Married:
                        personImport.MaritalStatusValueId = this.PersonMaritalStatusValues[Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.MaritalStatus.Single:
                        personImport.MaritalStatusValueId = this.PersonMaritalStatusValues[Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.MaritalStatus.Divorced:
                        personImport.MaritalStatusValueId = this.PersonMaritalStatusValues[Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_DIVORCED.AsGuid()].Id;
                        break;
                    case SlingshotCore.Model.MaritalStatus.Unknown:
                        personImport.MaritalStatusValueId = null;
                        break;
                }

                personImport.AnniversaryDate = slingshotPerson.AnniversaryDate;

                personImport.Grade = slingshotPerson.Grade;
                personImport.Email = slingshotPerson.Email;

                // slingshot doesn't include an IsEmailActive, so default it to True
                personImport.IsEmailActive = true;

                switch ( slingshotPerson.EmailPreference )
                {
                    case SlingshotCore.Model.EmailPreference.EmailAllowed:
                        personImport.EmailPreference = ( int ) Rock.Model.EmailPreference.EmailAllowed;
                        break;
                    case SlingshotCore.Model.EmailPreference.DoNotEmail:
                        personImport.EmailPreference = ( int ) Rock.Model.EmailPreference.DoNotEmail;
                        break;
                    case SlingshotCore.Model.EmailPreference.NoMassEmails:
                        personImport.EmailPreference = ( int ) Rock.Model.EmailPreference.NoMassEmails;
                        break;
                }

                personImport.CreatedDateTime = slingshotPerson.CreatedDateTime;
                personImport.ModifiedDateTime = slingshotPerson.ModifiedDateTime;

                personImport.Note = slingshotPerson.Note;
                personImport.GivingIndividually = slingshotPerson.GiveIndividually;

                // Phone Numbers
                personImport.PhoneNumbers = new List<Rock.Slingshot.Model.PhoneNumberImport>();
                foreach ( var slingshotPersonPhone in slingshotPerson.PhoneNumbers )
                {
                    var phoneNumberImport = new Rock.Slingshot.Model.PhoneNumberImport();
                    phoneNumberImport.NumberTypeValueId = this.PhoneNumberTypeValues[slingshotPersonPhone.PhoneType].Id;
                    phoneNumberImport.Number = slingshotPersonPhone.PhoneNumber;
                    phoneNumberImport.IsMessagingEnabled = slingshotPersonPhone.IsMessagingEnabled ?? false;
                    phoneNumberImport.IsUnlisted = slingshotPersonPhone.IsUnlisted ?? false;
                    personImport.PhoneNumbers.Add( phoneNumberImport );
                }

                // Addresses
                personImport.Addresses = new List<Rock.Slingshot.Model.PersonAddressImport>();
                foreach ( var slingshotPersonAddress in slingshotPerson.Addresses )
                {
                    if ( !string.IsNullOrEmpty( slingshotPersonAddress.Street1 ) )
                    {
                        int? groupLocationTypeValueId = null;
                        switch ( slingshotPersonAddress.AddressType )
                        {
                            case SlingshotCore.Model.AddressType.Home:
                                groupLocationTypeValueId = this.GroupLocationTypeValues[Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid()].Id;
                                break;
                            case SlingshotCore.Model.AddressType.Previous:
                                groupLocationTypeValueId = this.GroupLocationTypeValues[Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid()].Id;
                                break;
                            case SlingshotCore.Model.AddressType.Work:
                                groupLocationTypeValueId = this.GroupLocationTypeValues[Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid()].Id;
                                break;
                        }

                        if ( groupLocationTypeValueId.HasValue )
                        {
                            var addressImport = new Rock.Slingshot.Model.PersonAddressImport()
                            {
                                GroupLocationTypeValueId = groupLocationTypeValueId.Value,
                                IsMailingLocation = slingshotPersonAddress.AddressType == SlingshotCore.Model.AddressType.Home,
                                IsMappedLocation = slingshotPersonAddress.AddressType == SlingshotCore.Model.AddressType.Home,
                                Street1 = slingshotPersonAddress.Street1,
                                Street2 = slingshotPersonAddress.Street2,
                                City = slingshotPersonAddress.City,
                                State = slingshotPersonAddress.State,
                                Country = slingshotPersonAddress.Country,
                                PostalCode = slingshotPersonAddress.PostalCode,
                                Latitude = slingshotPersonAddress.Latitude.AsDoubleOrNull(),
                                Longitude = slingshotPersonAddress.Longitude.AsDoubleOrNull()
                            };

                            personImport.Addresses.Add( addressImport );
                        }
                        else
                        {
                            throw new Exception( $"Unexpected Address Type: {slingshotPersonAddress.AddressType}" );
                        }
                    }
                }

                // Attribute Values
                personImport.AttributeValues = new List<Rock.Slingshot.Model.AttributeValueImport>();
                foreach ( var slingshotPersonAttributeValue in slingshotPerson.Attributes )
                {
                    int attributeId = this.PersonAttributeKeyLookup[slingshotPersonAttributeValue.AttributeKey].Id;
                    var attributeValueImport = new Rock.Slingshot.Model.AttributeValueImport { AttributeId = attributeId, Value = slingshotPersonAttributeValue.AttributeValue };
                    personImport.AttributeValues.Add( attributeValueImport );
                }

                personImportList.Add( personImport );
            }

            return personImportList;
        }

        #endregion Person Related

        /// <summary>
        /// Add any campuses that aren't in Rock yet
        /// </summary>
        private void AddCampuses()
        {
            Dictionary<int, SlingshotCore.Model.Campus> importCampuses = new Dictionary<int, SlingshotCore.Model.Campus>();
            foreach ( var campus in this.SlingshotPersonList.Select( a => a.Campus ).Where( a => a.CampusId > 0 ) )
            {
                if ( !importCampuses.ContainsKey( campus.CampusId ) )
                {
                    importCampuses.Add( campus.CampusId, campus );
                }
            }

            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );

            foreach ( var importCampus in importCampuses.Where( a => !CampusCache.All().Any( c => c.Name.Equals( a.Value.CampusName, StringComparison.OrdinalIgnoreCase ) ) ).Select( a => a.Value ) )
            {
                var campusToAdd = new Rock.Model.Campus { ForeignId = importCampus.CampusId, Name = importCampus.CampusName, Guid = Guid.NewGuid() };
                campusService.Add( campusToAdd );
                rockContext.SaveChanges();

                Rock.Web.Cache.CampusCache.Flush( campusToAdd.Id );
            }
        }

        /// <summary>
        /// Add any GroupTypes that aren't in Rock yet
        /// </summary>
        private void AddGroupTypes()
        {
            var rockContext = new RockContext();
            var groupTypeService = new GroupTypeService( rockContext );

            foreach ( var importGroupType in this.SlingshotGroupTypeList.Where( a => !this.GroupTypeLookupByForeignId.ContainsKey( a.Id ) ) )
            {
                var groupTypeToAdd = new Rock.Model.GroupType { ForeignId = importGroupType.Id, Name = importGroupType.Name, Guid = Guid.NewGuid() };
                groupTypeToAdd.ShowInGroupList = true;
                groupTypeToAdd.ShowInNavigation = true;
                groupTypeToAdd.GroupTerm = "Group";
                groupTypeToAdd.GroupMemberTerm = "Member";

                groupTypeService.Add( groupTypeToAdd );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Adds any attribute categories that are in the slingshot files (person and family attributes)
        /// </summary>
        private void AddAttributeCategories()
        {
            int entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>().Value;
            int entityTypeIdAttribute = EntityTypeCache.GetId<Rock.Model.Attribute>().Value;
            var attributeCategoryNames = this.SlingshotPersonAttributes.Where( a => !string.IsNullOrWhiteSpace( a.Category ) ).Select( a => a.Category ).Distinct().ToList();
            attributeCategoryNames.AddRange( this.SlingshotFamilyAttributes.Where( a => !string.IsNullOrWhiteSpace( a.Category ) ).Select( a => a.Category ).Distinct().ToList() );

            var rockContext = new RockContext();
            var categoryService = new CategoryService( rockContext );

            var attributeCategoryList = categoryService.Queryable().Where( a => a.EntityTypeId == entityTypeIdAttribute ).ToList();

            foreach ( var slingshotAttributeCategoryName in attributeCategoryNames.Distinct().ToList() )
            {
                if ( !attributeCategoryList.Any( a => a.Name.Equals( slingshotAttributeCategoryName, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    Rock.Model.Category attributeCategory = new Rock.Model.Category();
                    attributeCategory.Name = slingshotAttributeCategoryName;
                    attributeCategory.EntityTypeId = entityTypeIdAttribute;
                    attributeCategory.EntityTypeQualifierColumn = "EntityTypeId";
                    attributeCategory.EntityTypeQualifierValue = entityTypeIdPerson.ToString();
                    attributeCategory.Guid = Guid.NewGuid();

                    categoryService.Add( attributeCategory );
                    attributeCategoryList.Add( attributeCategory );
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Adds the person attributes.
        /// </summary>
        private void AddPersonAttributes()
        {
            int entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>().Value;

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            var entityTypeIdAttribute = EntityTypeCache.GetId<Rock.Model.Attribute>().Value;

            var attributeCategoryList = new CategoryService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdAttribute ).ToList();

            // Add any Person Attributes to Rock that aren't in Rock yet
            // NOTE: For now, just match by Attribute.Key. Don't try to do a customizable match
            foreach ( var slingshotPersonAttribute in this.SlingshotPersonAttributes )
            {
                slingshotPersonAttribute.Key = slingshotPersonAttribute.Key;

                if ( !this.PersonAttributeKeyLookup.Keys.Any( a => a.Equals( slingshotPersonAttribute.Key, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    var rockPersonAttribute = new Rock.Model.Attribute();
                    rockPersonAttribute.Key = slingshotPersonAttribute.Key;
                    rockPersonAttribute.Name = slingshotPersonAttribute.Name;
                    rockPersonAttribute.Guid = Guid.NewGuid();
                    rockPersonAttribute.EntityTypeId = entityTypeIdPerson;
                    rockPersonAttribute.FieldTypeId = this.FieldTypeLookup[slingshotPersonAttribute.FieldType].Id;

                    if ( !string.IsNullOrWhiteSpace( slingshotPersonAttribute.Category ) )
                    {
                        var attributeCategory = attributeCategoryList.FirstOrDefault( a => a.Name.Equals( slingshotPersonAttribute.Category, StringComparison.OrdinalIgnoreCase ) );
                        if ( attributeCategory != null )
                        {
                            rockPersonAttribute.Categories = new List<Rock.Model.Category>();
                            rockPersonAttribute.Categories.Add( attributeCategory );
                        }
                    }

                    attributeService.Add( rockPersonAttribute );
                }
            }

            rockContext.SaveChanges();

            AttributeCache.FlushEntityAttributes();
        }

        /// <summary>
        /// Adds the family attributes.
        /// </summary>
        private void AddFamilyAttributes()
        {
            int entityTypeIdGroup = EntityTypeCache.GetId<Rock.Model.Group>().Value;

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var entityTypeIdAttribute = EntityTypeCache.GetId<Rock.Model.Attribute>().Value;
            var attributeCategoryList = new CategoryService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdAttribute ).ToList();
            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;

            // Add any Family Attributes to Rock that aren't in Rock yet
            // NOTE: For now, just match by Attribute.Key. Don't try to do a customizable match
            foreach ( var slingshotFamilyAttribute in this.SlingshotFamilyAttributes )
            {
                slingshotFamilyAttribute.Key = slingshotFamilyAttribute.Key;

                if ( !this.FamilyAttributeKeyLookup.Keys.Any( a => a.Equals( slingshotFamilyAttribute.Key, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    var rockFamilyAttribute = new Rock.Model.Attribute();
                    rockFamilyAttribute.Key = slingshotFamilyAttribute.Key;
                    rockFamilyAttribute.Name = slingshotFamilyAttribute.Name;
                    rockFamilyAttribute.Guid = Guid.NewGuid();
                    rockFamilyAttribute.EntityTypeId = entityTypeIdGroup;
                    rockFamilyAttribute.EntityTypeQualifierColumn = "GroupTypeId";
                    rockFamilyAttribute.EntityTypeQualifierValue = groupTypeIdFamily.ToString();
                    rockFamilyAttribute.FieldTypeId = this.FieldTypeLookup[slingshotFamilyAttribute.FieldType].Id;

                    if ( !string.IsNullOrWhiteSpace( slingshotFamilyAttribute.Category ) )
                    {
                        var attributeCategory = attributeCategoryList.FirstOrDefault( a => a.Name.Equals( slingshotFamilyAttribute.Category, StringComparison.OrdinalIgnoreCase ) );
                        if ( attributeCategory != null )
                        {
                            rockFamilyAttribute.Categories = new List<Rock.Model.Category>();
                            rockFamilyAttribute.Categories.Add( attributeCategory );
                        }
                    }

                    attributeService.Add( rockFamilyAttribute );
                }
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Adds the connection statuses.
        /// </summary>
        private void AddConnectionStatuses()
        {
            AddDefinedValues( this.SlingshotPersonList.Select( a => a.ConnectionStatus ).Where( a => !string.IsNullOrWhiteSpace( a ) ).Distinct().ToList(), this.PersonConnectionStatusValues );
        }

        /// <summary>
        /// Adds the person titles.
        /// </summary>
        private void AddPersonTitles()
        {
            AddDefinedValues( this.SlingshotPersonList.Select( a => a.Salutation ).Where( a => !string.IsNullOrWhiteSpace( a ) ).Distinct().ToList(), this.PersonTitleValues );
        }

        /// <summary>
        /// Adds the person suffixes.
        /// </summary>
        private void AddPersonSuffixes()
        {
            AddDefinedValues( this.SlingshotPersonList.Select( a => a.Suffix ).Where( a => !string.IsNullOrWhiteSpace( a ) ).Distinct().ToList(), this.PersonSuffixValues );
        }

        /// <summary>
        /// Adds the phone types.
        /// </summary>
        private void AddPhoneTypes()
        {
            AddDefinedValues( this.SlingshotPersonList.SelectMany( a => a.PhoneNumbers ).Select( a => a.PhoneType ).Distinct().ToList(), this.PhoneNumberTypeValues );
        }

        /// <summary>
        /// Adds the defined values.
        /// </summary>
        /// <param name="importDefinedValues">The import defined values.</param>
        /// <param name="currentValues">The current values.</param>
        private void AddDefinedValues( List<string> importDefinedValues, Dictionary<string, DefinedValueCache> currentValues )
        {
            var definedTypeId = currentValues.Select( a => a.Value.DefinedTypeId ).First();

            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );

            foreach ( var importDefinedValue in importDefinedValues.Where( value => !currentValues.Keys.Any( k => k.Equals( value, StringComparison.OrdinalIgnoreCase ) ) ) )
            {
                var definedValueToAdd = new Rock.Model.DefinedValue { DefinedTypeId = definedTypeId, Value = importDefinedValue, Guid = Guid.NewGuid() };

                definedValueService.Add( definedValueToAdd );
            }

            rockContext.SaveChanges();

            DefinedTypeCache.Flush( definedTypeId );
        }

        /// <summary>
        /// Loads all the slingshot lists
        /// </summary>
        /// <returns></returns>
        private void LoadSlingshotLists()
        {
            LoadPersonSlingshotLists();

            var familyAttributesFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.FamilyAttribute().GetFileName() );
            if ( File.Exists( familyAttributesFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( familyAttributesFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    this.SlingshotFamilyAttributes = csvReader.GetRecords<SlingshotCore.Model.FamilyAttribute>().ToList();
                }
            }
            else
            {
                this.SlingshotFamilyAttributes = new List<SlingshotCore.Model.FamilyAttribute>();
            }

            /* Attendance */
            var attendanceFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.Attendance().GetFileName() );
            if ( File.Exists( attendanceFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( attendanceFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    this.SlingshotAttendanceList = csvReader.GetRecords<SlingshotCore.Model.Attendance>().ToList();
                }
            }
            else
            {
                this.SlingshotAttendanceList = new List<SlingshotCore.Model.Attendance>();
            }

            var groupFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.Group().GetFileName() );
            if ( File.Exists( groupFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( groupFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    var uniqueGroups = new Dictionary<int, SlingshotCore.Model.Group>();

                    foreach ( var group in csvReader.GetRecords<SlingshotCore.Model.Group>().ToList() )
                    {
                        if ( !uniqueGroups.ContainsKey( group.Id ) )
                        {
                            uniqueGroups.Add( group.Id, group );
                        }
                    }

                    this.SlingshotGroupList = uniqueGroups.Select( a => a.Value ).ToList();
                }
            }
            else
            {
                this.SlingshotGroupList = new List<SlingshotCore.Model.Group>();
            }

            var groupMemberFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.GroupMember().GetFileName() );
            if ( File.Exists( groupMemberFileName ) )
            {
                var groupLookup = this.SlingshotGroupList.ToDictionary( k => k.Id, v => v );
                using ( var slingshotFileStream = File.OpenText( groupMemberFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;

                    var groupMemberList = csvReader.GetRecords<SlingshotCore.Model.GroupMember>().ToList().GroupBy( a => a.GroupId ).ToDictionary( k => k.Key, v => v.ToList() );
                    foreach ( var groupIdMembers in groupMemberList )
                    {
                        groupLookup[groupIdMembers.Key].GroupMembers = groupIdMembers.Value;
                    }
                }
            }

            var groupTypeFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.GroupType().GetFileName() );
            if ( File.Exists( groupTypeFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( groupTypeFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    this.SlingshotGroupTypeList = csvReader.GetRecords<SlingshotCore.Model.GroupType>().ToList();
                }
            }
            else
            {
                this.SlingshotGroupTypeList = new List<SlingshotCore.Model.GroupType>();
            }

            var locationFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.Location().GetFileName() );
            if ( File.Exists( locationFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( locationFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    var uniqueLocations = new Dictionary<int, SlingshotCore.Model.Location>();
                    foreach ( var location in csvReader.GetRecords<SlingshotCore.Model.Location>().ToList() )
                    {
                        if ( !uniqueLocations.ContainsKey( location.Id ) )
                        {
                            uniqueLocations.Add( location.Id, location );
                        }
                    }

                    this.SlingshotLocationList = uniqueLocations.Select( a => a.Value ).ToList();
                }
            }
            else
            {
                this.SlingshotLocationList = new List<SlingshotCore.Model.Location>();
            }

            var scheduleFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.Schedule().GetFileName() );
            if ( File.Exists( scheduleFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( scheduleFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;

                    var uniqueSchedules = new Dictionary<int, SlingshotCore.Model.Schedule>();
                    foreach ( var schedule in csvReader.GetRecords<SlingshotCore.Model.Schedule>().ToList() )
                    {
                        if ( !uniqueSchedules.ContainsKey( schedule.Id ) )
                        {
                            uniqueSchedules.Add( schedule.Id, schedule );
                        }
                    }

                    this.SlingshotScheduleList = uniqueSchedules.Select( a => a.Value ).ToList();
                }
            }
            else
            {
                this.SlingshotScheduleList = new List<SlingshotCore.Model.Schedule>();
            }


            /* Financial Transactions */
            var financialAccountFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.FinancialAccount().GetFileName() );
            if ( File.Exists( financialAccountFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( financialAccountFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    this.SlingshotFinancialAccountList = csvReader.GetRecords<SlingshotCore.Model.FinancialAccount>().ToList();
                }
            }
            else
            {
                this.SlingshotFinancialAccountList = new List<SlingshotCore.Model.FinancialAccount>();
            }

            var financialTransactionFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.FinancialTransaction().GetFileName() );
            if ( File.Exists( financialTransactionFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( financialTransactionFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    this.SlingshotFinancialTransactionList = csvReader.GetRecords<SlingshotCore.Model.FinancialTransaction>().ToList();
                }
            }
            else
            {
                this.SlingshotFinancialTransactionList = new List<SlingshotCore.Model.FinancialTransaction>();
            }

            var financialTransactionDetailFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.FinancialTransactionDetail().GetFileName() );
            if ( File.Exists( financialTransactionDetailFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( financialTransactionDetailFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    var slingshotFinancialTransactionDetailList = csvReader.GetRecords<SlingshotCore.Model.FinancialTransactionDetail>().ToList();
                    var slingshotFinancialTransactionLookup = this.SlingshotFinancialTransactionList.ToDictionary( k => k.Id, v => v );
                    foreach ( var slingshotFinancialTransactionDetail in slingshotFinancialTransactionDetailList )
                    {
                        slingshotFinancialTransactionLookup[slingshotFinancialTransactionDetail.TransactionId].FinancialTransactionDetails.Add( slingshotFinancialTransactionDetail );
                    }
                }
            }

            var financialBatchFileName = Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.FinancialBatch().GetFileName() );
            if ( File.Exists( financialBatchFileName ) )
            {
                using ( var slingshotFileStream = File.OpenText( financialBatchFileName ) )
                {
                    CsvReader csvReader = new CsvReader( slingshotFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    this.SlingshotFinancialBatchList = csvReader.GetRecords<SlingshotCore.Model.FinancialBatch>().ToList();
                    var transactionsByBatch = this.SlingshotFinancialTransactionList.GroupBy( a => a.BatchId ).ToDictionary( k => k.Key, v => v.ToList() );
                    foreach ( var slingshotFinancialBatch in this.SlingshotFinancialBatchList )
                    {
                        if ( transactionsByBatch.ContainsKey( slingshotFinancialBatch.Id ) )
                        {
                            slingshotFinancialBatch.FinancialTransactions = transactionsByBatch[slingshotFinancialBatch.Id];
                        }
                    }
                }
            }
            else
            {
                this.SlingshotFinancialBatchList = new List<SlingshotCore.Model.FinancialBatch>();
            }
        }

        /// <summary>
        /// Loads the person slingshot lists.
        /// </summary>
        private void LoadPersonSlingshotLists()
        {
            Dictionary<int, List<SlingshotCore.Model.PersonAddress>> slingshotPersonAddressListLookup;
            Dictionary<int, List<SlingshotCore.Model.PersonAttributeValue>> slingshotPersonAttributeValueListLookup;
            Dictionary<int, List<SlingshotCore.Model.PersonPhone>> slingshotPersonPhoneListLookup;

            using ( var slingshotFileStream = File.OpenText( Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.Person().GetFileName() ) ) )
            {
                CsvReader csvReader = new CsvReader( slingshotFileStream );
                csvReader.Configuration.HasHeaderRecord = true;
                csvReader.Configuration.WillThrowOnMissingField = false;
                this.SlingshotPersonList = csvReader.GetRecords<SlingshotCore.Model.Person>().ToList();
            }

            using ( var slingshotFileStream = File.OpenText( Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.PersonAddress().GetFileName() ) ) )
            {
                CsvReader csvReader = new CsvReader( slingshotFileStream );
                csvReader.Configuration.HasHeaderRecord = true;
                slingshotPersonAddressListLookup = csvReader.GetRecords<SlingshotCore.Model.PersonAddress>().GroupBy( a => a.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );
            }

            using ( var slingshotFileStream = File.OpenText( Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.PersonAttributeValue().GetFileName() ) ) )
            {
                CsvReader csvReader = new CsvReader( slingshotFileStream );
                csvReader.Configuration.HasHeaderRecord = true;
                slingshotPersonAttributeValueListLookup = csvReader.GetRecords<SlingshotCore.Model.PersonAttributeValue>().GroupBy( a => a.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );
            }

            using ( var slingshotFileStream = File.OpenText( Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.PersonPhone().GetFileName() ) ) )
            {
                CsvReader csvReader = new CsvReader( slingshotFileStream );
                csvReader.Configuration.HasHeaderRecord = true;
                slingshotPersonPhoneListLookup = csvReader.GetRecords<SlingshotCore.Model.PersonPhone>().GroupBy( a => a.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );
            }

            foreach ( var slingshotPerson in this.SlingshotPersonList )
            {
                slingshotPerson.Addresses = slingshotPersonAddressListLookup.ContainsKey( slingshotPerson.Id ) ? slingshotPersonAddressListLookup[slingshotPerson.Id] : new List<SlingshotCore.Model.PersonAddress>();
                slingshotPerson.Attributes = slingshotPersonAttributeValueListLookup.ContainsKey( slingshotPerson.Id ) ? slingshotPersonAttributeValueListLookup[slingshotPerson.Id].ToList() : new List<SlingshotCore.Model.PersonAttributeValue>();
                slingshotPerson.PhoneNumbers = slingshotPersonPhoneListLookup.ContainsKey( slingshotPerson.Id ) ? slingshotPersonPhoneListLookup[slingshotPerson.Id].ToList() : new List<SlingshotCore.Model.PersonPhone>();
            }

            using ( var slingshotFileStream = File.OpenText( Path.Combine( this.SlingshotDirectoryName, new SlingshotCore.Model.PersonAttribute().GetFileName() ) ) )
            {
                CsvReader csvReader = new CsvReader( slingshotFileStream );
                csvReader.Configuration.HasHeaderRecord = true;
                this.SlingshotPersonAttributes = csvReader.GetRecords<SlingshotCore.Model.PersonAttribute>().ToList();
            }
        }

        /// <summary>
        /// Ensures that the defined values that we need exist on the Rock Server
        /// </summary>
        private void EnsureDefinedValues()
        {
            List<Rock.Model.DefinedValue> definedValuesToAdd = new List<Rock.Model.DefinedValue>();
            int definedTypeIdCurrencyType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ).Id;
            int definedTypeIdTransactionSourceType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() ).Id;

            // The following DefinedValues are not IsSystem, but are potentionally needed to do an import, so make sure they exist on the server
            if ( !this.CurrencyTypeValues.ContainsKey( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_NONCASH.AsGuid() ) )
            {
                definedValuesToAdd.Add( new Rock.Model.DefinedValue
                {
                    DefinedTypeId = definedTypeIdCurrencyType,
                    Guid = Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_NONCASH.AsGuid(),
                    Value = "Non-Cash",
                    Description = "Used to track non-cash transactions."
                } );
            }

            if ( !this.CurrencyTypeValues.ContainsKey( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_UNKNOWN.AsGuid() ) )
            {
                definedValuesToAdd.Add( new Rock.Model.DefinedValue
                {
                    DefinedTypeId = definedTypeIdCurrencyType,
                    Guid = Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_UNKNOWN.AsGuid(),
                    Value = "Unknown",
                    Description = "The currency type is unknown. For example, it might have been imported from a system that doesn't indicate currency type."
                } );
            }

            if ( !this.TransactionSourceTypeValues.ContainsKey( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_BANK_CHECK.AsGuid() ) )
            {
                definedValuesToAdd.Add( new Rock.Model.DefinedValue
                {
                    DefinedTypeId = definedTypeIdTransactionSourceType,
                    Guid = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_BANK_CHECK.AsGuid(),
                    Value = "Bank Checks",
                    Description = "Transactions that originated from a bank's bill pay system"
                } );
            }

            if ( !this.TransactionSourceTypeValues.ContainsKey( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_KIOSK.AsGuid() ) )
            {
                definedValuesToAdd.Add( new Rock.Model.DefinedValue
                {
                    DefinedTypeId = definedTypeIdTransactionSourceType,
                    Guid = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_KIOSK.AsGuid(),
                    Value = "Kiosk",
                    Description = "Transactions that originated from a kiosk"
                } );
            }

            if ( !this.TransactionSourceTypeValues.ContainsKey( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION.AsGuid() ) )
            {
                definedValuesToAdd.Add( new Rock.Model.DefinedValue
                {
                    DefinedTypeId = definedTypeIdTransactionSourceType,
                    Guid = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_MOBILE_APPLICATION.AsGuid(),
                    Value = "Mobile Application",
                    Description = "Transactions that originated from a mobile application"
                } );
            }

            if ( !this.TransactionSourceTypeValues.ContainsKey( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION.AsGuid() ) )
            {
                definedValuesToAdd.Add( new Rock.Model.DefinedValue
                {
                    DefinedTypeId = definedTypeIdTransactionSourceType,
                    Guid = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION.AsGuid(),
                    Value = "On-Site Collection",
                    Description = "Transactions that were collected on-site"
                } );
            }

            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );
            definedValueService.AddRange( definedValuesToAdd );
            rockContext.SaveChanges();

            foreach ( var definedTypeId in definedValuesToAdd.Select( a => a.DefinedTypeId ).Distinct().ToList() )
            {
                DefinedTypeCache.Flush( definedTypeId );
            }
        }

        /// <summary>
        /// Loads the lookups.
        /// </summary>
        private void LoadLookups()
        {
            this.PersonRecordTypeValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_RECORD_TYPE.AsGuid() );
            this.PersonRecordStatusValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid() );
            this.PersonConnectionStatusValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Select( a => a.Value ).ToDictionary( k => k.Value, v => v );
            this.PersonTitleValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ).Select( a => a.Value ).ToDictionary( k => k.Value, v => v );
            this.PersonSuffixValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ).Select( a => a.Value ).ToDictionary( k => k.Value, v => v );
            this.PersonMaritalStatusValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() );
            this.PhoneNumberTypeValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE.AsGuid() ).Select( a => a.Value ).ToDictionary( k => k.Value, v => v );
            this.GroupLocationTypeValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() );
            this.LocationTypeValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.LOCATION_TYPE.AsGuid() );
            this.CurrencyTypeValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() );
            this.TransactionSourceTypeValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() );
            this.TransactionTypeValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() );

            int entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>().Value;
            int entityTypeIdGroup = EntityTypeCache.GetId<Rock.Model.Group>().Value;
            int entityTypeIdAttribute = EntityTypeCache.GetId<Rock.Model.Attribute>().Value;

            var rockContext = new RockContext();

            // Person Attributes
            var personAttributes = new AttributeService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdPerson ).Select( a => a.Id ).ToList().Select( a => AttributeCache.Read( a ) ).ToList();
            this.PersonAttributeKeyLookup = personAttributes.ToDictionary( k => k.Key, v => v );

            // Family Attributes
            var familyAttributes = new AttributeService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdGroup ).Select( a => a.Id ).ToList().Select( a => AttributeCache.Read( a ) ).ToList();
            this.FamilyAttributeKeyLookup = familyAttributes.ToDictionary( k => k.Key, v => v );

            // FieldTypes
            this.FieldTypeLookup = new FieldTypeService( rockContext ).Queryable().AsNoTracking().ToList().ToDictionary( k => k.Class, v => v );

            // GroupTypes
            this.GroupTypeLookupByForeignId = new GroupTypeService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue ).AsNoTracking().ToList().ToDictionary( k => k.ForeignId.Value, v => v );
        }

        /// <summary>
        /// Loads the defined values.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <returns></returns>
        private Dictionary<Guid, DefinedValueCache> LoadDefinedValues( Guid definedTypeGuid )
        {
            return DefinedTypeCache.Read( definedTypeGuid ).DefinedValues.ToDictionary( k => k.Guid );
        }
    }
}
