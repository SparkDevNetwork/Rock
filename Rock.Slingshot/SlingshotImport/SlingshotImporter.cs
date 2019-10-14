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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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
    /// Prepares slingshot files for import into Rock's BulkImport system
    /// </summary>
    public class SlingshotImporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Importer"/> class.
        /// </summary>
        /// <param name="slingshotFileName">Name of the slingshot file.</param>
        public SlingshotImporter( string slingshotFileName, string foreignSystemKey, BulkImporter.ImportUpdateType importUpdateType, EventHandler<object> onProgress = null )
        {
            this.Results = new Dictionary<string, string>();

            if ( onProgress != null )
            {
                this.OnProgress = onProgress;
            }

            SlingshotFileName = slingshotFileName;
            ForeignSystemKey = foreignSystemKey;
            SlingshotDirectoryName = Path.Combine( Path.GetDirectoryName( this.SlingshotFileName ), "slingshots", Path.GetFileNameWithoutExtension( this.SlingshotFileName ) );

            ReportProgress( 0, "Preparing Slingshot..." );
            var slingshotFilesDirectory = new DirectoryInfo( this.SlingshotDirectoryName );
            if ( slingshotFilesDirectory.Exists )
            {
                slingshotFilesDirectory.Delete( true );
            }

            ReportProgress( 0, "Extracting Main Slingshot File..." );
            slingshotFilesDirectory.Create();
            if ( File.Exists( this.SlingshotFileName ) )
            {
                ZipFile.ExtractToDirectory( this.SlingshotFileName, slingshotFilesDirectory.FullName );
            }

            this.SlingshotImageFileNames = new List<string>();

            // list any matching images.slingshot files that either got uploaded or manually placed in the SlingshotDirectory
            var mainFileNamePrefix = Path.GetFileNameWithoutExtension( this.SlingshotFileName );
            var imageSlingshotFiles = Directory.EnumerateFiles( Path.GetDirectoryName( this.SlingshotFileName ), mainFileNamePrefix + "*.images.slingshot" ).ToList();
            var extractedImagesFolder = Path.Combine( slingshotFilesDirectory.FullName, "Images" );
            if ( !Directory.Exists( extractedImagesFolder ) )
            {
                Directory.CreateDirectory( extractedImagesFolder );
            }

            foreach ( var imageSlingshotFile in imageSlingshotFiles )
            {
                var imageZipFile = ZipFile.Open( imageSlingshotFile, ZipArchiveMode.Read );

                ReportProgress( 0, $"Extracting Image Slingshot File: {Path.GetFileName( imageSlingshotFile )}" );
                // extract one at a time just in case some of them are corrupt
                imageZipFile.Entries.ToList().ForEach( a =>
                 {
                     try
                     {
                         a.ExtractToFile( Path.Combine( extractedImagesFolder, Path.GetFileName( a.FullName ) ) );
                     }
                     catch ( Exception ex )
                     {
                         System.Diagnostics.Debug.WriteLine( $"Unable to extract {a.FullName} from imageZipFile: {ex.Message}" );
                     }
                 } );
            }

            var imageFilesInFolder = Directory.EnumerateFiles( extractedImagesFolder );
            this.SlingshotImageFileNames.AddRange( imageFilesInFolder );

            BulkImporter = new BulkImporter();
            BulkImporter.ImportUpdateOption = importUpdateType;
            BulkImporter.OnProgress = BulkImporter_OnProgress;
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

        private List<string> _samplePhotoLocalUrls = null;

        /* Person Related */
        private Dictionary<Guid, DefinedValueCache> PersonRecordTypeValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> PersonRecordStatusValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> RecordStatusReasonValues { get; set; }

        private Dictionary<string, DefinedValueCache> PersonConnectionStatusValues { get; set; }

        private Dictionary<string, DefinedValueCache> PersonTitleValues { get; set; }

        private Dictionary<string, DefinedValueCache> PersonSuffixValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> PersonMaritalStatusValues { get; set; }

        private Dictionary<string, DefinedValueCache> PhoneNumberTypeValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> GroupLocationTypeValues { get; set; }

        private Dictionary<Guid, DefinedValueCache> LocationTypeValues { get; set; }

        private Dictionary<string, AttributeCache> PersonAttributeKeyLookup { get; set; }

        private Dictionary<string, AttributeCache> FamilyAttributeKeyLookup { get; set; }

        private Dictionary<string, AttributeCache> GroupAttributeKeyLookup { get; set; }

        private List<SlingshotCore.Model.PersonAttribute> SlingshotPersonAttributes { get; set; }

        private List<SlingshotCore.Model.FamilyAttribute> SlingshotFamilyAttributes { get; set; }

        private List<SlingshotCore.Model.GroupAttribute> SlingshotGroupAttributes { get; set; }

        private List<SlingshotCore.Model.Person> SlingshotPersonList { get; set; }

        /* Core  */
        private Dictionary<string, FieldTypeCache> FieldTypeLookup { get; set; }

        // GroupType Lookup by ForeignKey for the ForeignSystemKey
        private Dictionary<int, GroupTypeCache> GroupTypeLookupByForeignId { get; set; }

        // Lookup for Campus by UpperCase of Slingshot File's CampusId
        private Dictionary<int, CampusCache> CampusLookupByForeignId { get; set; }

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

        /* Financial Pledges */
        public List<SlingshotCore.Model.FinancialPledge> SlingshotFinancialPledgeList { get; private set; }

        /* Notes */
        public List<SlingshotCore.Model.FamilyNote> SlingshotFamilyNoteList { get; private set; }

        public List<SlingshotCore.Model.PersonNote> SlingshotPersonNoteList { get; private set; }

        /* Businesses */
        public List<SlingshotCore.Model.Business> SlingshotBusinessList { get; private set; }

        public List<SlingshotCore.Model.BusinessAttribute> SlingshotBusinessAttributes { get; private set; }

        /* */
        private string SlingshotFileName { get; set; }

        /// <summary>
        /// Gets or sets the foreign system key.
        /// </summary>
        /// <value>
        /// The foreign system key.
        /// </value>
        private string ForeignSystemKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the slingshot directory.
        /// </summary>
        /// <value>
        /// The name of the slingshot directory.
        /// </value>
        private string SlingshotDirectoryName { get; set; }

        /// <summary>
        /// Gets or sets list of Images found in *.images.slingshot folders
        /// </summary>
        /// <value>
        /// The slingshot image file names.
        /// </value>
        private List<string> SlingshotImageFileNames { get; set; }

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

        /// <summary>
        /// Reports the progress.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <param name="progressData">The progress data.</param>
        public void ReportProgress( int progress, object progressData )
        {
            if ( OnProgress != null )
            {
                OnProgress( this, progressData );
            }
        }

        /// <summary>
        /// Occurs when [on progress].
        /// </summary>
        public event EventHandler<object> OnProgress;

        /// <summary>
        /// Gets or sets the bulk importer.
        /// </summary>
        /// <value>
        /// The bulk importer.
        /// </value>
        private BulkImporter BulkImporter { get; set; }

        /// <summary>
        /// Does the import.
        /// </summary>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        public void DoImport()
        {
            this.Results.Clear();

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
            AddLocationTypes();
            AddAttributeCategories();
            AddPersonAttributes();
            AddFamilyAttributes();

            AddGroupTypes();
            AddGroupAttributes();
            AddBusinessAttributes();

            // load lookups again in case we added some new ones
            this.ReportProgress( 0, "Reloading Rock Lookups..." );
            LoadLookups();

            SubmitPersonImport();
            SubmitBusinessImport();

            // Attendance Related
            SubmitLocationImport();
            SubmitGroupImport();
            SubmitScheduleImport();
            SubmitAttendanceImport();

            // Financial Transaction Related
            SubmitFinancialAccountImport();
            SubmitFinancialBatchImport();
            SubmitFinancialTransactionImport();

            // Financial Pledges
            SubmitFinancialPledgeImport();

            // Person Notes
            SubmitEntityNotesImport<Rock.Model.Person>( this.SlingshotPersonNoteList, null );

            // Family Notes
            SubmitEntityNotesImport<Rock.Model.Group>( this.SlingshotFamilyNoteList, true );
        }

        private const string PREPARE_PHOTO_DATA = "Prepare Photo Data:";
        private const string IMPORTING_PHOTO_DATA = "Importing Photo Data:";
        private const string UPLOAD_PHOTO_STATS = "Log:";

        /// <summary>
        /// Does the import photos.
        /// </summary>
        public void DoImportPhotos()
        {
            // NOTE: Images can either be a URL or FileName specified in Person or Family import,
            // or in *.images.slingshot folders in the following format:
            /*
                exportfilename.slingshot
                exportfilename_1.images.slingshot( max size 100MB )
                  - FinancialTransaction_{ Id}[_{ImageNum}].jpg/dif
                  - Person_{ForeignPersonId}.jpg/dif
                  - Family_{ForeignFamilyId}Id}.jpg/dif
                exportfilename_2.images.slingshot
                exportfilename_n.images.slingshot
            */

            BulkImporter.OnProgress = BulkImporter_OnProgress;

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

            Dictionary<Guid, string> mimetypeLookup = ImageCodecInfo.GetImageDecoders().ToDictionary( k => k.FormatID, v => v.MimeType );

            int slingshotImageCount = this.SlingshotImageFileNames.Count();
            long photoLoadProgress = 0;
            long photoImportProgress = 0;
            int totalCount = slingshotImageCount + slingshotPersonsWithPhotoList.Where( a => !string.IsNullOrWhiteSpace( a.PersonPhotoUrl ) ).Count()
                + slingshotPersonsWithPhotoList.Where( a => a.FamilyId.HasValue && !string.IsNullOrWhiteSpace( a.FamilyImageUrl ) ).Select( a => a.FamilyId ).Distinct().Count();

            int totalPhotoDataBytes = 0;
            if ( !this.PhotoBatchSizeMB.HasValue || this.PhotoBatchSizeMB.Value < 1 )
            {
                this.PhotoBatchSizeMB = 100;
            }

            int maxPhotoBatchSize = this.PhotoBatchSizeMB.Value * 1024 * 1024;

            foreach ( var imageFileInfo in this.SlingshotImageFileNames.Select( a => new FileInfo( a ) ).ToList() )
            {
                var photoImport = new Rock.Slingshot.Model.PhotoImport();

                // NOTE: Use full filename for now so we can load the PhotoData later as needed
                photoImport.FileName = imageFileInfo.Name;

                using ( var fileStream = File.OpenRead( imageFileInfo.FullName ) )
                {
                    photoImport.PhotoData = Convert.ToBase64String( fileStream.ReadBytesToEnd() );
                    Interlocked.Increment( ref photoLoadProgress );
                    try
                    {
                        using ( var image = new Bitmap( fileStream ) )
                        {
                            photoImport.MimeType = mimetypeLookup.GetValueOrNull( image.RawFormat.Guid );
                        }
                    }
                    catch ( Exception ex )
                    {
                        // ignore and just get mimetype from filename instead
                        System.Diagnostics.Debug.WriteLine( "Error Getting MimeType from FileData: " + ex.Message );
                    }
                    if ( string.IsNullOrEmpty( photoImport.MimeType ) )
                    {
                        photoImport.MimeType = System.Web.MimeMapping.GetMimeMapping( imageFileInfo.FullName );
                    }
                }

                if ( photoImport.FileName.StartsWith( "FinancialTransaction_" ) )
                {
                    photoImport.PhotoType = Slingshot.Model.PhotoImport.PhotoImportType.FinancialTransaction;
                }
                else if ( photoImport.FileName.StartsWith( "Person_" ) )
                {
                    photoImport.PhotoType = Slingshot.Model.PhotoImport.PhotoImportType.Person;
                }
                else if ( photoImport.FileName.StartsWith( "Family_" ) )
                {
                    photoImport.PhotoType = Slingshot.Model.PhotoImport.PhotoImportType.Family;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine( "Unknown PhotoType: " + photoImport.FileName );
                    continue;
                }

                photoImport.ForeignId = Path.GetFileNameWithoutExtension( photoImport.FileName ).Split( '_' )[1].AsInteger();
                if ( photoImport.ForeignId == 0 )
                {
                    throw new Exception( "Unable to determine ForeignId for Photo" + photoImport.FileName );
                }

                photoImportList.Add( photoImport );

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
                    this.ReportProgress( 0, "Importing Images..." );
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

            HashSet<int> importedFamilyPhotos = new HashSet<int>();
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
            var result = BulkImporter.BulkPhotoImport( photoImportList, this.ForeignSystemKey );
            this.Results[UPLOAD_PHOTO_STATS] = result + "<br />";
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
                    HttpWebRequest imageRequest = (HttpWebRequest)HttpWebRequest.Create( photoUri );
                    HttpWebResponse imageResponse = (HttpWebResponse)imageRequest.GetResponse();
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
                if ( !photoFile.Exists )
                {
                    // if the file doesn't exist, it might be a relative path
                    photoFile = new FileInfo( Path.Combine( this.SlingshotDirectoryName, photoUrl ) );
                }

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

        #region Person and Family Notes

        /// <summary>
        /// Submits the entity notes import.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slingshotEntityNoteList">The slingshot entity note list.</param>
        /// <param name="groupEntityIsFamily">If this is a GroupEntity, is it a Family GroupType?</param>
        /// <exception cref="System.Exception">Unexpected Note EntityType</exception>
        private void SubmitEntityNotesImport<T>( IEnumerable<SlingshotCore.Data.EntityNote> slingshotEntityNoteList, bool? groupEntityIsFamily ) where T : Rock.Data.IEntity
        {
            var entityType = EntityTypeCache.Get<T>();

            string entityFriendlyName = entityType.FriendlyName;
            if ( entityType.Id == EntityTypeCache.GetId<Rock.Model.Group>().Value )
            {
                if ( groupEntityIsFamily.Value )
                {
                    entityFriendlyName = "Family";
                }
            }

            this.ReportProgress( 0, $"Preparing {entityFriendlyName} Notes Import..." );

            var noteImportList = new List<Rock.Slingshot.Model.NoteImport>();
            var rockContext = new RockContext();
            var noteTypeService = new Rock.Model.NoteTypeService( rockContext );

            var noteTypeLookup = noteTypeService.Queryable().Where( a => a.EntityTypeId == entityType.Id ).Select( a => new
            {
                a.Id,
                a.Name
            } ).ToList().DistinctBy( a => a.Name ).ToDictionary( k => k.Name, v => v.Id );

            var slingshotNoteTypeNames = slingshotEntityNoteList.Select( a => a.NoteType ).Distinct().ToList();
            foreach ( var noteTypeName in slingshotNoteTypeNames )
            {
                if ( !noteTypeLookup.ContainsKey( noteTypeName ) )
                {
                    var noteType = new NoteType();
                    noteType.IsSystem = false;
                    noteType.EntityTypeId = entityType.Id;
                    noteType.EntityTypeQualifierColumn = string.Empty;
                    noteType.EntityTypeQualifierValue = string.Empty;
                    noteType.Name = noteTypeName;
                    noteType.UserSelectable = true;
                    noteType.IconCssClass = string.Empty;
                    noteTypeService.Add( noteType );
                    rockContext.SaveChanges();

                    noteTypeLookup.Add( noteType.Name, noteType.Id );
                }
            }

            foreach ( var slingshotEntityNote in slingshotEntityNoteList )
            {
                var noteImport = new Rock.Slingshot.Model.NoteImport();
                noteImport.NoteForeignId = slingshotEntityNote.Id;
                noteImport.NoteTypeId = noteTypeLookup[slingshotEntityNote.NoteType];
                if ( slingshotEntityNote is SlingshotCore.Model.PersonNote )
                {
                    noteImport.EntityForeignId = ( slingshotEntityNote as SlingshotCore.Model.PersonNote ).PersonId;
                }
                else if ( slingshotEntityNote is SlingshotCore.Model.FamilyNote )
                {
                    noteImport.EntityForeignId = ( slingshotEntityNote as SlingshotCore.Model.FamilyNote ).FamilyId;
                }
                else
                {
                    throw new Exception( "Unexpected Note EntityType" );
                }

                noteImport.Caption = slingshotEntityNote.Caption;
                noteImport.IsAlert = slingshotEntityNote.IsAlert;
                noteImport.IsPrivateNote = slingshotEntityNote.IsPrivateNote;
                noteImport.Text = slingshotEntityNote.Text;
                noteImport.DateTime = slingshotEntityNote.DateTime;
                noteImport.CreatedByPersonForeignId = slingshotEntityNote.CreatedByPersonId;
                noteImportList.Add( noteImport );
            }

            this.ReportProgress( 0, $"Bulk Importing {entityFriendlyName} Notes..." );
            var result = BulkImporter.BulkNoteImport( noteImportList, entityType.Id, this.ForeignSystemKey, groupEntityIsFamily );
            Results.Add( $"{entityFriendlyName ?? entityType.FriendlyName} Note Import", result );
        }

        #endregion Person and Family Notes

        #region Financial Pledges

        /// <summary>
        /// Submits the financial pledge import.
        /// </summary>
        private void SubmitFinancialPledgeImport()
        {
            this.ReportProgress( 0, "Preparing FinancialPledgeImport..." );
            var financialPledgeImportList = new List<Rock.Slingshot.Model.FinancialPledgeImport>();
            foreach ( var slingshotFinancialPledge in this.SlingshotFinancialPledgeList )
            {
                var financialPledgeImport = new Rock.Slingshot.Model.FinancialPledgeImport();
                financialPledgeImport.FinancialPledgeForeignId = slingshotFinancialPledge.Id;
                financialPledgeImport.PersonForeignId = slingshotFinancialPledge.PersonId;
                financialPledgeImport.FinancialAccountForeignId = slingshotFinancialPledge.AccountId;
                financialPledgeImport.GroupForeignId = null;
                financialPledgeImport.TotalAmount = slingshotFinancialPledge.TotalAmount;
                switch ( slingshotFinancialPledge.PledgeFrequency )
                {
                    case SlingshotCore.Model.PledgeFrequency.OneTime:
                        financialPledgeImport.PledgeFrequencyValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME )?.Id;
                        break;

                    case SlingshotCore.Model.PledgeFrequency.Weekly:
                        financialPledgeImport.PledgeFrequencyValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_WEEKLY )?.Id;
                        break;

                    case SlingshotCore.Model.PledgeFrequency.BiWeekly:
                        financialPledgeImport.PledgeFrequencyValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_BIWEEKLY )?.Id;
                        break;

                    case SlingshotCore.Model.PledgeFrequency.TwiceAMonth:
                        financialPledgeImport.PledgeFrequencyValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEMONTHLY )?.Id;
                        break;

                    case SlingshotCore.Model.PledgeFrequency.Monthly:
                        financialPledgeImport.PledgeFrequencyValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_MONTHLY )?.Id;
                        break;

                    case SlingshotCore.Model.PledgeFrequency.Quarterly:
                        financialPledgeImport.PledgeFrequencyValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_QUARTERLY )?.Id;
                        break;

                    case SlingshotCore.Model.PledgeFrequency.TwiceAYear:
                        financialPledgeImport.PledgeFrequencyValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_TWICEYEARLY )?.Id;
                        break;

                    case SlingshotCore.Model.PledgeFrequency.Yearly:
                        financialPledgeImport.PledgeFrequencyValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_YEARLY )?.Id;
                        break;
                }

                financialPledgeImport.StartDate = slingshotFinancialPledge.StartDate ?? DateTime.MinValue;
                financialPledgeImport.EndDate = slingshotFinancialPledge.EndDate ?? DateTime.MaxValue;
                financialPledgeImport.CreatedDateTime = slingshotFinancialPledge.CreatedDateTime;
                financialPledgeImport.ModifiedDateTime = slingshotFinancialPledge.ModifiedDateTime;
                financialPledgeImportList.Add( financialPledgeImport );
            }

            this.ReportProgress( 0, "Bulk Importing FinancialPledges..." );
            var result = BulkImporter.BulkFinancialPledgeImport( financialPledgeImportList, this.ForeignSystemKey );
            Results.Add( "FinancialPledge Import", result );
        }

        #endregion Financial Pledges

        #region Financial Transaction Related

        /// <summary>
        /// Submits the financial account import.
        /// </summary>
        private void SubmitFinancialAccountImport()
        {
            this.ReportProgress( 0, "Preparing FinancialAccountImport..." );
            var financialAccountImportList = new List<Rock.Slingshot.Model.FinancialAccountImport>();
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
                    financialAccountImport.CampusId = this.CampusLookupByForeignId[slingshotFinancialAccount.CampusId.Value]?.Id;
                }

                financialAccountImport.ParentFinancialAccountForeignId = slingshotFinancialAccount.ParentAccountId == 0 ? (int?)null : slingshotFinancialAccount.ParentAccountId;

                financialAccountImportList.Add( financialAccountImport );
            }

            this.ReportProgress( 0, "Bulk Importing FinancialAccounts..." );
            var result = BulkImporter.BulkFinancialAccountImport( financialAccountImportList, this.ForeignSystemKey );
            Results.Add( "FinancialAccount Import", result );
        }

        /// <summary>
        /// Submits the financial batch import.
        /// </summary>
        private void SubmitFinancialBatchImport()
        {
            this.ReportProgress( 0, "Preparing FinancialBatchImport..." );
            var financialBatchImportList = new List<Rock.Slingshot.Model.FinancialBatchImport>();
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

                financialBatchImport.CampusId = slingshotFinancialBatch.CampusId.HasValue ? this.CampusLookupByForeignId[slingshotFinancialBatch.CampusId.Value]?.Id : (int?)null;

                financialBatchImportList.Add( financialBatchImport );
            }

            this.ReportProgress( 0, "Bulk Importing FinancialBatches..." );
            var result = BulkImporter.BulkFinancialBatchImport( financialBatchImportList, this.ForeignSystemKey );
            Results.Add( "FinancialBatch Import", result );
        }

        /// <summary>
        /// Submits the financial transaction import.
        /// </summary>
        private void SubmitFinancialTransactionImport()
        {
            this.ReportProgress( 0, "Preparing FinancialTransactionImport..." );
            var financialTransactionImportList = new List<Rock.Slingshot.Model.FinancialTransactionImport>();
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
                var result = BulkImporter.BulkFinancialTransactionImport( postChunk, this.ForeignSystemKey );

                if ( this.FinancialTransactionChunkSize.HasValue )
                {
                    if ( financialTransactionImportList.Count < postChunkSize )
                    {
                        financialTransactionImportList.Clear();
                    }
                    else
                    {
                        financialTransactionImportList.RemoveRange( 0, postChunkSize );
                    }
                }
                else
                {
                    financialTransactionImportList.Clear();
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
            var attendanceImportList = new List<Rock.Slingshot.Model.AttendanceImport>();
            HashSet<int> attendanceIds = new HashSet<int>();
            foreach ( var slingshotAttendance in this.SlingshotAttendanceList )
            {
                var attendanceImport = new Rock.Slingshot.Model.AttendanceImport();

                if ( slingshotAttendance.AttendanceId > 0 )
                {
                    attendanceImport.AttendanceForeignId = slingshotAttendance.AttendanceId;
                }
                else
                {
                    MD5 md5Hasher = MD5.Create();
                    var hashed = md5Hasher.ComputeHash( Encoding.UTF8.GetBytes( $@"
 {slingshotAttendance.PersonId}
 {slingshotAttendance.StartDateTime}
 {slingshotAttendance.LocationId}
 {slingshotAttendance.ScheduleId}
 {slingshotAttendance.GroupId}
" ) );
                    attendanceImport.AttendanceForeignId = Math.Abs( BitConverter.ToInt32( hashed, 0 ) ); // used abs to ensure positive number */
                }

                if ( !attendanceIds.Add( attendanceImport.AttendanceForeignId.Value ) )
                {
                    // shouldn't happen (but if it does, it'll be treated as a duplicate and not imported)
                    System.Diagnostics.Debug.WriteLine( $"#### Duplicate AttendanceId detected:{attendanceImport.AttendanceForeignId.Value} ####" );
                }

                attendanceImport.PersonForeignId = slingshotAttendance.PersonId;
                attendanceImport.GroupForeignId = slingshotAttendance.GroupId;
                attendanceImport.LocationForeignId = slingshotAttendance.LocationId;
                attendanceImport.ScheduleForeignId = slingshotAttendance.ScheduleId;
                attendanceImport.StartDateTime = slingshotAttendance.StartDateTime;
                attendanceImport.EndDateTime = slingshotAttendance.EndDateTime;
                attendanceImport.Note = slingshotAttendance.Note;
                if ( slingshotAttendance.CampusId.HasValue )
                {
                    attendanceImport.CampusId = this.CampusLookupByForeignId[slingshotAttendance.CampusId.Value]?.Id;
                }

                attendanceImportList.Add( attendanceImport );
            }

            this.ReportProgress( 0, "Bulk Importing Attendance..." );
            var result = BulkImporter.BulkAttendanceImport( attendanceImportList, this.ForeignSystemKey );

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
            var result = BulkImporter.BulkScheduleImport( scheduleImportList, this.ForeignSystemKey );
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
            var result = BulkImporter.BulkLocationImport( locationImportList, this.ForeignSystemKey );
            Results.Add( "Location Import", result );
        }

        /// <summary>
        /// Submits the group import.
        /// </summary>
        private void SubmitGroupImport()
        {
            this.ReportProgress( 0, "Preparing GroupImport..." );
            var groupImportList = new List<Rock.Slingshot.Model.GroupImport>();
            foreach ( var slingshotGroup in this.SlingshotGroupList )
            {
                var groupImport = new Rock.Slingshot.Model.GroupImport();
                groupImport.GroupForeignId = slingshotGroup.Id;
                groupImport.GroupTypeId = this.GroupTypeLookupByForeignId[slingshotGroup.GroupTypeId].Id;

                groupImport.Name = slingshotGroup.Name;
                groupImport.Description = slingshotGroup.Description;
                if ( string.IsNullOrWhiteSpace( slingshotGroup.Name ) )
                {
                    groupImport.Name = "Unnamed Group";
                }

                groupImport.IsActive = slingshotGroup.IsActive;
                groupImport.IsPublic = slingshotGroup.IsPublic;
                groupImport.Capacity = slingshotGroup.Capacity;
                groupImport.MeetingDay = slingshotGroup.MeetingDay;
                groupImport.MeetingTime = slingshotGroup.MeetingTime;

                groupImport.Order = slingshotGroup.Order;
                if ( slingshotGroup.CampusId.HasValue )
                {
                    groupImport.CampusId = this.CampusLookupByForeignId[slingshotGroup.CampusId.Value]?.Id;
                }

                groupImport.ParentGroupForeignId = slingshotGroup.ParentGroupId == 0 ? (int?)null : slingshotGroup.ParentGroupId;
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

                // Addresses
                groupImport.Addresses = new List<Rock.Slingshot.Model.GroupAddressImport>();
                foreach ( var slingshotGroupAddress in slingshotGroup.Addresses )
                {
                    if ( !string.IsNullOrEmpty( slingshotGroupAddress.Street1 ) )
                    {
                        int? groupLocationTypeValueId = null;
                        switch ( slingshotGroupAddress.AddressType )
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

                            case SlingshotCore.Model.AddressType.Other:
                                var locationTypeOther = this.GroupLocationTypeValues.Values.FirstOrDefault( t => t.Value.Equals( "Other" ) );
                                groupLocationTypeValueId = locationTypeOther != null ? this.GroupLocationTypeValues[locationTypeOther.Guid].Id :
                                    this.GroupLocationTypeValues[Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid()].Id;
                                break;
                        }

                        if ( groupLocationTypeValueId.HasValue )
                        {
                            var addressImport = new Rock.Slingshot.Model.GroupAddressImport()
                            {
                                GroupLocationTypeValueId = groupLocationTypeValueId.Value,
                                IsMailingLocation = slingshotGroupAddress.IsMailing,
                                IsMappedLocation = slingshotGroupAddress.AddressType == SlingshotCore.Model.AddressType.Home,
                                Street1 = slingshotGroupAddress.Street1.Left( 100 ),
                                Street2 = slingshotGroupAddress.Street2.Left( 100 ),
                                City = slingshotGroupAddress.City.Left( 50 ),
                                State = slingshotGroupAddress.State.Left( 50 ),
                                Country = slingshotGroupAddress.Country.Left( 50 ),
                                PostalCode = slingshotGroupAddress.PostalCode.Left( 50 ),
                                Latitude = slingshotGroupAddress.Latitude.AsDoubleOrNull(),
                                Longitude = slingshotGroupAddress.Longitude.AsDoubleOrNull()
                            };

                            groupImport.Addresses.Add( addressImport );
                        }
                        else
                        {
                            throw new Exception( $"Unexpected Address Type: {slingshotGroupAddress.AddressType}" );
                        }
                    }
                }

                // Attribute Values
                groupImport.AttributeValues = new List<Rock.Slingshot.Model.AttributeValueImport>();
                foreach ( var slingshotGroupAttributeValue in slingshotGroup.Attributes )
                {
                    int attributeId = this.GroupAttributeKeyLookup[slingshotGroupAttributeValue.AttributeKey].Id;
                    var attributeValueImport = new Rock.Slingshot.Model.AttributeValueImport { AttributeId = attributeId, Value = slingshotGroupAttributeValue.AttributeValue };
                    groupImport.AttributeValues.Add( attributeValueImport );
                }

                groupImportList.Add( groupImport );
            }

            this.ReportProgress( 0, "Bulk Importing Groups..." );
            var result = BulkImporter.BulkGroupImport( groupImportList, this.ForeignSystemKey );

            Results.Add( "Group Import", result );
        }

        #endregion Attendance Related

        #region Business Related

        /// <summary>
        /// Submits the business import.
        /// </summary>
        private void SubmitBusinessImport()
        {
            this.ReportProgress( 0, "Preparing BusinessImport..." );
            List<Rock.Slingshot.Model.PersonImport> businessImportList = GetBusinessImportList();

            this.ReportProgress( 0, "Bulk Importing Person..." );

            var result = BulkImporter.BulkBusinessImport( businessImportList, this.ForeignSystemKey );

            Results.Add( "Business Import", result );
        }

        /// <summary>
        /// Gets the business import list.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">personImport.PersonForeignId must be greater than 0
        /// or
        /// personImport.FamilyForeignId must be greater than 0 or null
        /// or</exception>
        private List<Rock.Slingshot.Model.PersonImport> GetBusinessImportList()
        {
            List<Rock.Slingshot.Model.PersonImport> businessImportList = new List<Rock.Slingshot.Model.PersonImport>();

            var familyRolesLookup = GroupTypeCache.GetFamilyGroupType().Roles.ToDictionary( k => k.Guid );

            foreach ( var slingshotBusiness in this.SlingshotBusinessList )
            {
                var businessImport = new Rock.Slingshot.Model.PersonImport();
                businessImport.RecordTypeValueId = this.PersonRecordTypeValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid()].Id;
                businessImport.PersonForeignId = slingshotBusiness.Id;
                businessImport.FamilyForeignId = slingshotBusiness.Id;

                if ( businessImport.PersonForeignId <= 0 )
                {
                    throw new Exception( "personImport.PersonForeignId must be greater than 0" );
                }

                if ( businessImport.FamilyForeignId <= 0 )
                {
                    throw new Exception( "personImport.FamilyForeignId must be greater than 0 or null" );
                }

                businessImport.FamilyName = slingshotBusiness.Name;

                businessImport.GroupRoleId = familyRolesLookup[Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid()].Id;

                if ( ( slingshotBusiness.Campus?.CampusId ?? 0 ) != 0 )
                {
                    businessImport.CampusId = this.CampusLookupByForeignId[slingshotBusiness.Campus.CampusId]?.Id;
                }

                switch ( slingshotBusiness.RecordStatus )
                {
                    case SlingshotCore.Model.RecordStatus.Active:
                        businessImport.RecordStatusValueId = this.PersonRecordStatusValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid()]?.Id;
                        break;

                    case SlingshotCore.Model.RecordStatus.Inactive:
                        businessImport.RecordStatusValueId = this.PersonRecordStatusValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid()]?.Id;
                        break;

                    case SlingshotCore.Model.RecordStatus.Pending:
                        businessImport.RecordStatusValueId = this.PersonRecordStatusValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid()]?.Id;
                        break;
                }

                businessImport.InactiveReasonNote = slingshotBusiness.InactiveReason;
                businessImport.IsDeceased = false;
                businessImport.RecordStatusReasonValueId = this.RecordStatusReasonValues.Values.FirstOrDefault( v => v.Value.Equals( slingshotBusiness.InactiveReason ) )?.Id;

                businessImport.LastName = slingshotBusiness.Name;

                businessImport.Gender = ( int ) Rock.Model.Gender.Unknown;

                businessImport.Email = slingshotBusiness.Email;

                // slingshot doesn't include an IsEmailActive, so default it to True
                businessImport.IsEmailActive = true;

                switch ( slingshotBusiness.EmailPreference )
                {
                    case SlingshotCore.Model.EmailPreference.EmailAllowed:
                        businessImport.EmailPreference = ( int ) Rock.Model.EmailPreference.EmailAllowed;
                        break;

                    case SlingshotCore.Model.EmailPreference.DoNotEmail:
                        businessImport.EmailPreference = ( int ) Rock.Model.EmailPreference.DoNotEmail;
                        break;

                    case SlingshotCore.Model.EmailPreference.NoMassEmails:
                        businessImport.EmailPreference = ( int ) Rock.Model.EmailPreference.NoMassEmails;
                        break;
                }

                businessImport.CreatedDateTime = slingshotBusiness.CreatedDateTime;
                businessImport.ModifiedDateTime = slingshotBusiness.ModifiedDateTime;

                businessImport.Note = slingshotBusiness.Note;
                businessImport.GivingIndividually = false;

                // Phone Numbers
                businessImport.PhoneNumbers = new List<Rock.Slingshot.Model.PhoneNumberImport>();
                foreach ( var slingshotBusinessPhone in slingshotBusiness.PhoneNumbers )
                {
                    var phoneNumberImport = new Rock.Slingshot.Model.PhoneNumberImport();
                    phoneNumberImport.NumberTypeValueId = this.PhoneNumberTypeValues[slingshotBusinessPhone.PhoneType].Id;
                    phoneNumberImport.Number = slingshotBusinessPhone.PhoneNumber;
                    phoneNumberImport.IsMessagingEnabled = slingshotBusinessPhone.IsMessagingEnabled ?? false;
                    phoneNumberImport.IsUnlisted = slingshotBusinessPhone.IsUnlisted ?? false;
                    businessImport.PhoneNumbers.Add( phoneNumberImport );
                }

                // Addresses
                businessImport.Addresses = new List<Rock.Slingshot.Model.PersonAddressImport>();
                foreach ( var slingshotPersonAddress in slingshotBusiness.Addresses )
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

                            case SlingshotCore.Model.AddressType.Other:
                                var locationTypeOther = this.GroupLocationTypeValues.Values.FirstOrDefault( t => t.Value.Equals( "Other" ) );
                                groupLocationTypeValueId = locationTypeOther != null ? this.GroupLocationTypeValues[locationTypeOther.Guid].Id :
                                    this.GroupLocationTypeValues[Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid()].Id;
                                break;
                        }

                        if ( groupLocationTypeValueId.HasValue )
                        {
                            var addressImport = new Rock.Slingshot.Model.PersonAddressImport()
                            {
                                GroupLocationTypeValueId = groupLocationTypeValueId.Value,
                                IsMailingLocation = slingshotPersonAddress.IsMailing,
                                IsMappedLocation = slingshotPersonAddress.AddressType == SlingshotCore.Model.AddressType.Home,
                                Street1 = slingshotPersonAddress.Street1.Left( 100 ),
                                Street2 = slingshotPersonAddress.Street2.Left( 100 ),
                                City = slingshotPersonAddress.City.Left( 50 ),
                                State = slingshotPersonAddress.State.Left( 50 ),
                                Country = slingshotPersonAddress.Country.Left( 50 ),
                                PostalCode = slingshotPersonAddress.PostalCode.Left( 50 ),
                                Latitude = slingshotPersonAddress.Latitude.AsDoubleOrNull(),
                                Longitude = slingshotPersonAddress.Longitude.AsDoubleOrNull()
                            };

                            businessImport.Addresses.Add( addressImport );
                        }
                        else
                        {
                            throw new Exception( $"Unexpected Address Type: {slingshotPersonAddress.AddressType}" );
                        }
                    }
                }

                // Attribute Values
                businessImport.AttributeValues = new List<Rock.Slingshot.Model.AttributeValueImport>();
                foreach ( var slingshotBusinessAttributeValue in slingshotBusiness.Attributes )
                {
                    int attributeId = this.PersonAttributeKeyLookup[slingshotBusinessAttributeValue.AttributeKey].Id;
                    var attributeValueImport = new Rock.Slingshot.Model.AttributeValueImport { AttributeId = attributeId, Value = slingshotBusinessAttributeValue.AttributeValue };
                    businessImport.AttributeValues.Add( attributeValueImport );
                }

                businessImportList.Add( businessImport );
            }

            return businessImportList;
        }

        #endregion

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

            var result = BulkImporter.BulkPersonImport( personImportList, this.ForeignSystemKey );

            Results.Add( "Person Import", result );
        }

        

        /// <summary>
        /// Bulks the importer on progress.
        /// </summary>
        /// <param name="e">The e.</param>
        private void BulkImporter_OnProgress( string e )
        {
            ReportProgress( 0, e );
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

            var gradeOffsetLookupFromDescription = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() )?.DefinedValues.ToDictionary( k => k.Description, v => v.Value.AsInteger(), StringComparer.OrdinalIgnoreCase );
            var gradeOffsetLookupFromAbbreviation = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() )?.DefinedValues
                .Select( a => new { Value = a.Value.AsInteger(), Abbreviation = a.AttributeValues["Abbreviation"]?.Value } )
                .Where( a => !string.IsNullOrWhiteSpace( a.Abbreviation ) ).ToDictionary( k => k.Abbreviation, v => v.Value, StringComparer.OrdinalIgnoreCase );

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

                if ( ( slingshotPerson.Campus?.CampusId ?? 0 ) != 0 )
                {
                    personImport.CampusId = this.CampusLookupByForeignId[slingshotPerson.Campus.CampusId]?.Id;
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
                personImport.IsDeceased = slingshotPerson.InactiveReason.Equals( "Deceased" );
                personImport.RecordStatusReasonValueId = this.RecordStatusReasonValues.Values.FirstOrDefault( v => v.Value.Equals( slingshotPerson.InactiveReason ) )?.Id;

                if ( !string.IsNullOrEmpty( slingshotPerson.ConnectionStatus ) )
                {
                    personImport.ConnectionStatusValueId = this.PersonConnectionStatusValues[slingshotPerson.ConnectionStatus]?.Id;
                }

                if ( !string.IsNullOrEmpty( slingshotPerson.Salutation ) )
                {
                    personImport.TitleValueId = this.PersonTitleValues[slingshotPerson.Salutation.ToLower()]?.Id;
                }

                if ( !string.IsNullOrEmpty( slingshotPerson.Suffix ) )
                {
                    personImport.SuffixValueId = this.PersonSuffixValues[slingshotPerson.Suffix.ToLower()]?.Id;
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
                    personImport.BirthYear = slingshotPerson.Birthdate.Value.Year == slingshotPerson.BirthdateNoYearMagicYear ? (int?)null : slingshotPerson.Birthdate.Value.Year;
                }

                switch ( slingshotPerson.Gender )
                {
                    case SlingshotCore.Model.Gender.Male:
                        personImport.Gender = (int)Rock.Model.Gender.Male;
                        break;

                    case SlingshotCore.Model.Gender.Female:
                        personImport.Gender = (int)Rock.Model.Gender.Female;
                        break;

                    case SlingshotCore.Model.Gender.Unknown:
                        personImport.Gender = (int)Rock.Model.Gender.Unknown;
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

                // do a case-insensitive lookup GradeOffset from either the Description ("Kindergarten", "1st Grade", etc) or Abbreviation ("K", "1st", etc)
                personImport.Grade = slingshotPerson.Grade;
                int? gradeOffset = null;
                if ( !string.IsNullOrWhiteSpace( personImport.Grade ) )
                {
                    gradeOffset = gradeOffsetLookupFromDescription.GetValueOrNull( personImport.Grade );
                    if ( gradeOffset == null )
                    {
                        gradeOffset = gradeOffsetLookupFromAbbreviation.GetValueOrNull( personImport.Grade );
                    }
                }

                if ( gradeOffset.HasValue )
                {
                    personImport.GraduationYear = Person.GraduationYearFromGradeOffset( gradeOffset );
                }

                personImport.Email = slingshotPerson.Email;

                // slingshot doesn't include an IsEmailActive, so default it to True
                personImport.IsEmailActive = true;

                switch ( slingshotPerson.EmailPreference )
                {
                    case SlingshotCore.Model.EmailPreference.EmailAllowed:
                        personImport.EmailPreference = (int)Rock.Model.EmailPreference.EmailAllowed;
                        break;

                    case SlingshotCore.Model.EmailPreference.DoNotEmail:
                        personImport.EmailPreference = (int)Rock.Model.EmailPreference.DoNotEmail;
                        break;

                    case SlingshotCore.Model.EmailPreference.NoMassEmails:
                        personImport.EmailPreference = (int)Rock.Model.EmailPreference.NoMassEmails;
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

                            case SlingshotCore.Model.AddressType.Other:
                                var locationTypeOther = this.GroupLocationTypeValues.Values.FirstOrDefault( t => t.Value.Equals( "Other" ) );
                                groupLocationTypeValueId = locationTypeOther != null ? this.GroupLocationTypeValues[locationTypeOther.Guid].Id :
                                    this.GroupLocationTypeValues[Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid()].Id;
                                break;
                        }

                        if ( groupLocationTypeValueId.HasValue )
                        {
                            var addressImport = new Rock.Slingshot.Model.PersonAddressImport()
                            {
                                GroupLocationTypeValueId = groupLocationTypeValueId.Value,
                                IsMailingLocation = slingshotPersonAddress.IsMailing,
                                IsMappedLocation = slingshotPersonAddress.AddressType == SlingshotCore.Model.AddressType.Home,
                                Street1 = slingshotPersonAddress.Street1.Left( 100 ),
                                Street2 = slingshotPersonAddress.Street2.Left( 100 ),
                                City = slingshotPersonAddress.City.Left( 50 ),
                                State = slingshotPersonAddress.State.Left( 50 ),
                                Country = slingshotPersonAddress.Country.Left( 50 ),
                                PostalCode = slingshotPersonAddress.PostalCode.Left( 50 ),
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
            List<SlingshotCore.Model.Campus> importCampuses = new List<SlingshotCore.Model.Campus>();
            foreach ( var campus in this.SlingshotPersonList.Select( a => a.Campus ).Where( a => a.CampusId > 0 ) )
            {
                if ( !importCampuses.Any( a => a.CampusId == campus.CampusId ) )
                {
                    importCampuses.Add( campus );
                }
            }

            var rockContext = new RockContext();
            var campusService = new CampusService( rockContext );

            // Flush the campuscache just in case it was updated in the Database without rock knowing about it
            CampusCache.Clear();

            // Rock has a Unique Constraint on Campus.Name so, make sure campus name is unique and rename it if a new campus happens to have the same name as an existing campus
            var usedCampusNames = CampusCache.All().Select( a => a.Name ).ToList();

            foreach ( var importCampus in importCampuses.Where( a => !CampusCache.All().Any( c => c.ForeignId == a.CampusId && c.ForeignKey == this.ForeignSystemKey ) ) )
            {
                var campusToAdd = new Rock.Model.Campus();
                campusToAdd.ForeignId = importCampus.CampusId;
                campusToAdd.ForeignKey = this.ForeignSystemKey;
                if ( usedCampusNames.Any( a => a.Equals( importCampus.CampusName ) ) )
                {
                    campusToAdd.Name = importCampus.CampusName + $" ({this.ForeignSystemKey})";
                }
                else
                {
                    campusToAdd.Name = importCampus.CampusName;
                }

                campusToAdd.IsActive = true;

                usedCampusNames.Add( campusToAdd.Name );

                campusToAdd.Guid = Guid.NewGuid();
                campusService.Add( campusToAdd );
                rockContext.SaveChanges();
            }

            // Flush the campuscache to force to reload
            CampusCache.Clear();
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
                var groupTypeToAdd = new Rock.Model.GroupType();
                groupTypeToAdd.ForeignId = importGroupType.Id;
                groupTypeToAdd.ForeignKey = this.ForeignSystemKey;
                groupTypeToAdd.Name = importGroupType.Name;
                groupTypeToAdd.Guid = Guid.NewGuid();
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
            int entityTypeIdGroup = EntityTypeCache.GetId<Rock.Model.Group>().Value;
            var personCategoryNames = this.SlingshotPersonAttributes.Where( a => !string.IsNullOrWhiteSpace( a.Category ) ).Select( a => a.Category ).Distinct().ToList();
            personCategoryNames.AddRange( this.SlingshotFamilyAttributes.Where( a => !string.IsNullOrWhiteSpace( a.Category ) ).Select( a => a.Category ).Distinct().ToList() );

            var rockContext = new RockContext();
            var categoryService = new CategoryService( rockContext );

            var attributeCategoryList = categoryService.Queryable().Where( a => a.EntityTypeId == entityTypeIdAttribute ).ToList();

            foreach ( var slingshotAttributeCategoryName in personCategoryNames.Distinct().ToList() )
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
        }

        /// <summary>
        /// Adds the person attributes.
        /// </summary>
        private void AddBusinessAttributes()
        {
            int entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>().Value;

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );

            var entityTypeIdAttribute = EntityTypeCache.GetId<Rock.Model.Attribute>().Value;

            var attributeCategoryList = new CategoryService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdAttribute ).ToList();

            // Add any Business Attributes to Rock that aren't in Rock yet
            // NOTE: For now, just match by Attribute.Key. Don't try to do a customizable match
            foreach ( var slingshotBusinessAttribute in this.SlingshotBusinessAttributes )
            {

                if ( !this.PersonAttributeKeyLookup.Keys.Any( a => a.Equals( slingshotBusinessAttribute.Key, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    var rockBusinessAttribute = new Rock.Model.Attribute();
                    rockBusinessAttribute.Key = slingshotBusinessAttribute.Key;
                    rockBusinessAttribute.Name = slingshotBusinessAttribute.Name;
                    rockBusinessAttribute.Guid = Guid.NewGuid();
                    rockBusinessAttribute.EntityTypeId = entityTypeIdPerson;
                    rockBusinessAttribute.FieldTypeId = this.FieldTypeLookup[slingshotBusinessAttribute.FieldType].Id;
                    rockBusinessAttribute.EntityTypeQualifierColumn = "RecordTypeValueId";
                    rockBusinessAttribute.EntityTypeQualifierValue = this.PersonRecordTypeValues[Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid()].Id.ToString();

                    if ( !string.IsNullOrWhiteSpace( slingshotBusinessAttribute.Category ) )
                    {
                        var attributeCategory = attributeCategoryList.FirstOrDefault( a => a.Name.Equals( slingshotBusinessAttribute.Category, StringComparison.OrdinalIgnoreCase ) );
                        if ( attributeCategory != null )
                        {
                            rockBusinessAttribute.Categories = new List<Rock.Model.Category>();
                            rockBusinessAttribute.Categories.Add( attributeCategory );
                        }
                    }

                    attributeService.Add( rockBusinessAttribute );
                }
            }

            rockContext.SaveChanges();
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
        /// Adds the group attributes.
        /// </summary>
        private void AddGroupAttributes()
        {
            int entityTypeIdGroup = EntityTypeCache.GetId<Rock.Model.Group>().Value;

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var entityTypeIdAttribute = EntityTypeCache.GetId<Rock.Model.Attribute>().Value;
            var attributeCategoryList = new CategoryService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdAttribute ).ToList();

            // Add any Group Attributes to Rock that aren't in Rock yet
            foreach ( var slingshotGroupAttribute in this.SlingshotGroupAttributes )
            {
                slingshotGroupAttribute.Key = slingshotGroupAttribute.Key;

                if ( !this.GroupAttributeKeyLookup.Keys.Any( a => a.Equals( slingshotGroupAttribute.Key, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    // the group attribute category targets the grouptype
                    var rockGroupAttribute = new Rock.Model.Attribute();
                    rockGroupAttribute.Key = slingshotGroupAttribute.Key;
                    rockGroupAttribute.Name = slingshotGroupAttribute.Name;
                    rockGroupAttribute.Guid = Guid.NewGuid();
                    rockGroupAttribute.EntityTypeId = entityTypeIdGroup;
                    rockGroupAttribute.EntityTypeQualifierColumn = "GroupTypeId";
                    rockGroupAttribute.EntityTypeQualifierValue = this.SlingshotGroupTypeList.FirstOrDefault( gt => gt.Name.Equals( slingshotGroupAttribute.Category ) )?.Id.ToString();
                    rockGroupAttribute.FieldTypeId = this.FieldTypeLookup[slingshotGroupAttribute.FieldType].Id;

                    if ( !string.IsNullOrWhiteSpace( slingshotGroupAttribute.Category ) )
                    {
                        var attributeCategory = attributeCategoryList.FirstOrDefault( a => a.Name.Equals( slingshotGroupAttribute.Category, StringComparison.OrdinalIgnoreCase ) );
                        if ( attributeCategory != null )
                        {
                            rockGroupAttribute.Categories = new List<Rock.Model.Category>();
                            rockGroupAttribute.Categories.Add( attributeCategory );
                        }
                    }

                    attributeService.Add( rockGroupAttribute );
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
            AddDefinedValues( this.SlingshotPersonList.Select( a => a.Salutation.ToLower() ).Where( a => !string.IsNullOrWhiteSpace( a ) ).Distinct().ToList(), this.PersonTitleValues );
        }

        /// <summary>
        /// Adds the person suffixes.
        /// </summary>
        private void AddPersonSuffixes()
        {
            AddDefinedValues( this.SlingshotPersonList.Select( a => a.Suffix.ToLower() ).Where( a => !string.IsNullOrWhiteSpace( a ) ).Distinct().ToList(), this.PersonSuffixValues );
        }

        /// <summary>
        /// Adds the phone types.
        /// </summary>
        private void AddPhoneTypes()
        {
            AddDefinedValues( this.SlingshotPersonList.SelectMany( a => a.PhoneNumbers ).Select( a => a.PhoneType ).Distinct().ToList(), this.PhoneNumberTypeValues );
        }

        /// <summary>
        /// Adds the location types.
        /// </summary>
        private void AddLocationTypes()
        {
            // temporarily convert to Dictionary<ValueName, ValueCache> for the Add method
            var groupLocationTypeValues = this.GroupLocationTypeValues.Values.ToDictionary( k => k.Value, p => p );
            AddDefinedValues( this.SlingshotPersonList.SelectMany( a => a.Addresses ).Select( a => Enum.GetName( typeof( SlingshotCore.Model.AddressType ), a.AddressType ) ).Distinct().ToList(), groupLocationTypeValues );
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
        }

        /// <summary>
        /// Loads all the slingshot lists
        /// </summary>
        /// <returns></returns>
        private void LoadSlingshotLists()
        {
            LoadPersonSlingshotLists();

            // Family Attributes
            this.SlingshotFamilyAttributes = LoadSlingshotListFromFile<SlingshotCore.Model.FamilyAttribute>();

            /* Attendance */
            this.SlingshotAttendanceList = LoadSlingshotListFromFile<SlingshotCore.Model.Attendance>( false );

            /* Groups (non-family) (note: there might be duplicates, so just get the distinct ones */
            LoadGroupSlingshotLists();

            /* Group Members*/
            var groupMemberList = LoadSlingshotListFromFile<SlingshotCore.Model.GroupMember>().GroupBy( a => a.GroupId ).ToDictionary( k => k.Key, v => v.ToList() );
            var groupLookup = this.SlingshotGroupList.ToDictionary( k => k.Id, v => v );
            foreach ( var groupIdMembers in groupMemberList )
            {
                groupLookup[groupIdMembers.Key].GroupMembers = groupIdMembers.Value;
            }

            /* Group Type*/
            this.SlingshotGroupTypeList = LoadSlingshotListFromFile<SlingshotCore.Model.GroupType>();

            /* Locations (note: there might be duplicates, so just get the distinct ones */
            this.SlingshotLocationList = LoadSlingshotListFromFile<SlingshotCore.Model.Location>().DistinctBy( a => a.Id ).ToList();

            /* Schedules (note: there might be duplicates, so just get the distinct ones */
            this.SlingshotScheduleList = LoadSlingshotListFromFile<SlingshotCore.Model.Schedule>().DistinctBy( a => a.Id ).ToList();

            /* Financial Accounts */
            this.SlingshotFinancialAccountList = LoadSlingshotListFromFile<SlingshotCore.Model.FinancialAccount>();

            /* Financial Transactions and FinancialTransactionDetail*/
            this.SlingshotFinancialTransactionList = LoadSlingshotListFromFile<SlingshotCore.Model.FinancialTransaction>();
            var slingshotFinancialTransactionDetailList = LoadSlingshotListFromFile<SlingshotCore.Model.FinancialTransactionDetail>();
            var slingshotFinancialTransactionLookup = this.SlingshotFinancialTransactionList.ToDictionary( k => k.Id, v => v );
            foreach ( var slingshotFinancialTransactionDetail in slingshotFinancialTransactionDetailList )
            {
                slingshotFinancialTransactionLookup[slingshotFinancialTransactionDetail.TransactionId].FinancialTransactionDetails.Add( slingshotFinancialTransactionDetail );
            }

            /* Financial Batches */
            this.SlingshotFinancialBatchList = LoadSlingshotListFromFile<SlingshotCore.Model.FinancialBatch>();
            var transactionsByBatch = this.SlingshotFinancialTransactionList.GroupBy( a => a.BatchId ).ToDictionary( k => k.Key, v => v.ToList() );
            foreach ( var slingshotFinancialBatch in this.SlingshotFinancialBatchList )
            {
                if ( transactionsByBatch.ContainsKey( slingshotFinancialBatch.Id ) )
                {
                    slingshotFinancialBatch.FinancialTransactions = transactionsByBatch[slingshotFinancialBatch.Id];
                }
            }

            /* Financial Pledges */
            this.SlingshotFinancialPledgeList = LoadSlingshotListFromFile<SlingshotCore.Model.FinancialPledge>( false );

            /* Person Notes */
            this.SlingshotPersonNoteList = LoadSlingshotListFromFile<SlingshotCore.Model.PersonNote>();

            /* Family Notes */
            this.SlingshotFamilyNoteList = LoadSlingshotListFromFile<SlingshotCore.Model.FamilyNote>();

            /* Businesses */
            LoadBusinessSlingshotLists();

            /* Business Contacts */
            var businessContactList = LoadSlingshotListFromFile<SlingshotCore.Model.BusinessContact>().GroupBy( a => a.BusinessId ).ToDictionary( k => k.Key, v => v.ToList() );
            var businessLookup = this.SlingshotBusinessList.ToDictionary( k => k.Id, v => v );
            foreach ( var busisnessContact in businessContactList )
            {
                businessLookup[busisnessContact.Key].Contacts = busisnessContact.Value;
            }


        }

        /// <summary>
        /// Loads the slingshot list from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="willThrowOnMissingField">The will throw on missing field.</param>
        /// <returns></returns>
        private List<T> LoadSlingshotListFromFile<T>( bool? willThrowOnMissingField = null ) where T : SlingshotCore.Model.IImportModel, new()
        {
            var fileName = Path.Combine( this.SlingshotDirectoryName, new T().GetFileName() );
            if ( File.Exists( fileName ) )
            {
                try
                {
                    using ( var slingshotFileStream = File.OpenText( fileName ) )
                    {
                        CsvReader csvReader = new CsvReader( slingshotFileStream );
                        csvReader.Configuration.HasHeaderRecord = true;
                        if ( willThrowOnMissingField.HasValue )
                        {
                            csvReader.Configuration.WillThrowOnMissingField = willThrowOnMissingField.Value;
                        }

                        return csvReader.GetRecords<T>().ToList();
                    }
                }
                catch
                {
                    var exceptions = AnalyzeImportFileExceptions<T>( willThrowOnMissingField, fileName );
                    var exception = new AggregateException( $"File '{Path.GetFileName( fileName )}' cannot be properly read during Slingshot import. See InnerExceptions for line number(s).", exceptions );
                    ExceptionLogService.LogException( exception );
                    throw exception;
                }
            }
            else
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Process the import file and log all the lines where exceptions occur.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="willThrowOnMissingField">The will throw on missing field.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Exceptions</returns>
        private static List<Exception> AnalyzeImportFileExceptions<T>( bool? willThrowOnMissingField, string fileName ) where T : SlingshotCore.Model.IImportModel, new()
        {
            List<Exception> exceptions = new List<Exception>();

            using ( var slingshotFileStream = File.OpenText( fileName ) )
            {
                // Pre process file to see if there are errors.
                CsvReader csvReader = new CsvReader( slingshotFileStream );
                csvReader.Configuration.HasHeaderRecord = true;
                if ( willThrowOnMissingField.HasValue )
                {
                    csvReader.Configuration.WillThrowOnMissingField = willThrowOnMissingField.Value;
                    csvReader.Configuration.IgnoreReadingExceptions = true;
                }

                // We're just reading these to spot any problems on a particular row.
                int i = 1; // start count at the header row
                while ( csvReader.Read() )
                {
                    i++;
                    try
                    {
                        var record = csvReader.GetRecord<T>();
                    }
                    catch ( Exception ex )
                    {
                        exceptions.Add( new CsvBadDataException( $"Line {i} cannot be properly read during Slingshot import.", ex ) );
                    }
                }
            }

            return exceptions;
        }


        /// <summary>
        /// Loads the person slingshot lists.
        /// </summary>
        private void LoadPersonSlingshotLists()
        {
            this.SlingshotPersonList = LoadSlingshotListFromFile<SlingshotCore.Model.Person>( false );

            Dictionary<int, List<SlingshotCore.Model.PersonAddress>> slingshotPersonAddressListLookup = LoadSlingshotListFromFile<SlingshotCore.Model.PersonAddress>( false ).GroupBy( a => a.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );
            Dictionary<int, List<SlingshotCore.Model.PersonAttributeValue>> slingshotPersonAttributeValueListLookup = LoadSlingshotListFromFile<SlingshotCore.Model.PersonAttributeValue>().GroupBy( a => a.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );
            Dictionary<int, List<SlingshotCore.Model.PersonPhone>> slingshotPersonPhoneListLookup = LoadSlingshotListFromFile<SlingshotCore.Model.PersonPhone>().GroupBy( a => a.PersonId ).ToDictionary( k => k.Key, v => v.ToList() );

            foreach ( var slingshotPerson in this.SlingshotPersonList )
            {
                slingshotPerson.Addresses = slingshotPersonAddressListLookup.ContainsKey( slingshotPerson.Id ) ? slingshotPersonAddressListLookup[slingshotPerson.Id] : new List<SlingshotCore.Model.PersonAddress>();
                slingshotPerson.Attributes = slingshotPersonAttributeValueListLookup.ContainsKey( slingshotPerson.Id ) ? slingshotPersonAttributeValueListLookup[slingshotPerson.Id].ToList() : new List<SlingshotCore.Model.PersonAttributeValue>();
                slingshotPerson.PhoneNumbers = slingshotPersonPhoneListLookup.ContainsKey( slingshotPerson.Id ) ? slingshotPersonPhoneListLookup[slingshotPerson.Id].ToList() : new List<SlingshotCore.Model.PersonPhone>();
            }

            this.SlingshotPersonAttributes = LoadSlingshotListFromFile<SlingshotCore.Model.PersonAttribute>().ToList();
        }

        /// <summary>
        /// Loads the person slingshot lists.
        /// </summary>
        private void LoadBusinessSlingshotLists()
        {
            this.SlingshotBusinessList = LoadSlingshotListFromFile<SlingshotCore.Model.Business>( false );

            Dictionary<int, List<SlingshotCore.Model.BusinessAddress>> slingshotBusinessAddressListLookup = LoadSlingshotListFromFile<SlingshotCore.Model.BusinessAddress>( false ).GroupBy( a => a.BusinessId ).ToDictionary( k => k.Key, v => v.ToList() );
            Dictionary<int, List<SlingshotCore.Model.BusinessAttributeValue>> slingshotBusinessAttributeValueListLookup = LoadSlingshotListFromFile<SlingshotCore.Model.BusinessAttributeValue>().GroupBy( a => a.BusinessId ).ToDictionary( k => k.Key, v => v.ToList() );
            Dictionary<int, List<SlingshotCore.Model.BusinessPhone>> slingshotBusinessPhoneListLookup = LoadSlingshotListFromFile<SlingshotCore.Model.BusinessPhone>().GroupBy( a => a.BusinessId ).ToDictionary( k => k.Key, v => v.ToList() );

            foreach ( var slingshotBusiness in this.SlingshotBusinessList )
            {
                slingshotBusiness.Addresses = slingshotBusinessAddressListLookup.ContainsKey( slingshotBusiness.Id ) ? slingshotBusinessAddressListLookup[slingshotBusiness.Id] : new List<SlingshotCore.Model.BusinessAddress>();
                slingshotBusiness.Attributes = slingshotBusinessAttributeValueListLookup.ContainsKey( slingshotBusiness.Id ) ? slingshotBusinessAttributeValueListLookup[slingshotBusiness.Id].ToList() : new List<SlingshotCore.Model.BusinessAttributeValue>();
                slingshotBusiness.PhoneNumbers = slingshotBusinessPhoneListLookup.ContainsKey( slingshotBusiness.Id ) ? slingshotBusinessPhoneListLookup[slingshotBusiness.Id].ToList() : new List<SlingshotCore.Model.BusinessPhone>();
            }

            this.SlingshotBusinessAttributes = LoadSlingshotListFromFile<SlingshotCore.Model.BusinessAttribute>().ToList();
        }

        /// <summary>
        /// Loads the group slingshot lists.
        /// </summary>
        private void LoadGroupSlingshotLists()
        {
            this.SlingshotGroupList = LoadSlingshotListFromFile<SlingshotCore.Model.Group>( false ).DistinctBy( a => a.Id ).ToList();

            var groupAddressLookup = LoadSlingshotListFromFile<SlingshotCore.Model.GroupAddress>().GroupBy( a => a.GroupId ).ToDictionary( k => k.Key, v => v.ToList() );
            var groupAttributeValueLookup = LoadSlingshotListFromFile<SlingshotCore.Model.GroupAttributeValue>().GroupBy( a => a.GroupId ).ToDictionary( k => k.Key, v => v.ToList() );

            foreach ( var slingshotGroup in this.SlingshotGroupList )
            {
                slingshotGroup.Addresses = groupAddressLookup.ContainsKey( slingshotGroup.Id ) ? groupAddressLookup[slingshotGroup.Id] : new List<SlingshotCore.Model.GroupAddress>();
                slingshotGroup.Attributes = groupAttributeValueLookup.ContainsKey( slingshotGroup.Id ) ? groupAttributeValueLookup[slingshotGroup.Id].ToList() : new List<SlingshotCore.Model.GroupAttributeValue>();
            }

            this.SlingshotGroupAttributes = LoadSlingshotListFromFile<SlingshotCore.Model.GroupAttribute>().ToList();
        }

        /// <summary>
        /// Ensures that the defined values that we need exist on the Rock Server
        /// </summary>
        private void EnsureDefinedValues()
        {
            List<Rock.Model.DefinedValue> definedValuesToAdd = new List<Rock.Model.DefinedValue>();
            int definedTypeIdCurrencyType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ).Id;
            int definedTypeIdTransactionSourceType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() ).Id;

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
        }

        /// <summary>
        /// Loads the lookups.
        /// </summary>
        private void LoadLookups()
        {
            this.PersonRecordTypeValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_RECORD_TYPE.AsGuid() );
            this.PersonRecordStatusValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid() );
            this.RecordStatusReasonValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON.AsGuid() );
            this.PersonConnectionStatusValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ).Select( a => a.Value ).ToDictionary( k => k.Value, v => v );
            this.PersonTitleValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ).Select( a => a.Value ).ToDictionary( k => k.Value.ToLower(), v => v );
            this.PersonSuffixValues = LoadDefinedValues( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ).Select( a => a.Value ).ToDictionary( k => k.Value.ToLower(), v => v );
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
            var personAttributes = new AttributeService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdPerson ).Select( a => a.Id ).ToList().Select( a => AttributeCache.Get( a ) ).ToList();
            this.PersonAttributeKeyLookup = personAttributes.ToDictionary( k => k.Key, v => v );

            // Family Attributes
            string groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id.ToString();

            var familyAttributes = new AttributeService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdGroup && a.EntityTypeQualifierColumn == "GroupTypeId" && a.EntityTypeQualifierValue == groupTypeIdFamily ).Select( a => a.Id ).ToList().Select( a => AttributeCache.Get( a ) ).ToList();
            this.FamilyAttributeKeyLookup = familyAttributes.ToDictionary( k => k.Key, v => v );

            // FieldTypes
            this.FieldTypeLookup = new FieldTypeService( rockContext ).Queryable().Select( a => a.Id ).ToList().Select( a => FieldTypeCache.Get( a ) ).ToDictionary( k => k.Class, v => v );

            // Group Attributes
            var groupAttributes = new AttributeService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeIdGroup ).Select( a => a.Id ).ToList().Select( a => AttributeCache.Get( a ) ).ToList();
            this.GroupAttributeKeyLookup = groupAttributes.ToDictionary( k => k.Key, v => v );

            // GroupTypes
            this.GroupTypeLookupByForeignId = new GroupTypeService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == this.ForeignSystemKey ).ToList().Select( a => GroupTypeCache.Get( a ) ).ToDictionary( k => k.ForeignId.Value, v => v );

            // Campuses
            this.CampusLookupByForeignId = CampusCache.All().Where( a => a.ForeignId.HasValue && a.ForeignKey == this.ForeignSystemKey ).ToList().ToDictionary( k => k.ForeignId.Value, v => v );
        }

        /// <summary>
        /// Loads the defined values.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type unique identifier.</param>
        /// <returns></returns>
        private Dictionary<Guid, DefinedValueCache> LoadDefinedValues( Guid definedTypeGuid )
        {
            return DefinedTypeCache.Get( definedTypeGuid ).DefinedValues.ToDictionary( k => k.Guid );
        }
    }
}