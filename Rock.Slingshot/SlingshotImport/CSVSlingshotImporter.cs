using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Rock.Data;
using Rock.Model;

// Alias Slingshot.Core namespace to avoid conflict with Rock.Slingshot.*
using SlingshotCore = global::Slingshot.Core;

namespace Rock.Slingshot
{
    public class CSVSlingshotImporter : SlingshotImporter
    {
        private const string CSV_IMPORT_ERROR_COLUMN_NAME = "CSV Import Errors";
        private readonly string CSV_IMPORT_SUCCESS_COLUMN_NAME = "CSV Import Success";
        private const string ERROR_CSV_FILENAME = "errors/errors.csv";

        public string ErrorCSVfilename { get; private set; }

        private readonly SlingshotDataValidator dataValidator;

        public bool HasErrors { get; private set; }

        private readonly struct PersonImportErrorMessageStruct
        {
            public PersonImportErrorMessageStruct( int id, string message )
            {
                Id = id;
                Message = message;
            }

            /// <summary>
            /// The Id of the person in the foreign system.
            /// </summary>
            /// <value>
            /// The foreign id.
            /// </value>
            public int Id { get; }
            /// <summary>
            /// The Message from the Exception that occurred when importing the CSV
            /// </summary>
            /// <value>
            /// The message.
            /// </value>
            public string Message { get; }
        }

        private readonly List<PersonImportErrorMessageStruct> personImportErrorMessageStructs = new List<PersonImportErrorMessageStruct>();

        public CSVSlingshotImporter( string uploadedPersonCSVFileName, string foreignSystemKey, string csvDataType, BulkImporter.ImportUpdateType importUpdateType, EventHandler<object> onProgress = null ) : base()
        {
            this.Results = new Dictionary<string, string>();

            if ( onProgress != null )
            {
                OnProgress += onProgress;
            }

            SlingshotFileName = uploadedPersonCSVFileName;
            ForeignSystemKey = foreignSystemKey;
            SlingshotDirectoryName = Path.Combine( Path.GetDirectoryName( uploadedPersonCSVFileName ), "slingshots", Path.GetFileNameWithoutExtension( this.SlingshotFileName ) );

            ReportProgress( 0, "Preparing CSV Files..." );
            var slingshotFilesDirectory = new DirectoryInfo( this.SlingshotDirectoryName );
            var slingshotFilesDirectoryParent = slingshotFilesDirectory.Parent;
            if ( slingshotFilesDirectoryParent.Exists )
            {
                slingshotFilesDirectoryParent.Delete( true );
            }

            ReportProgress( 0, "Extracting Main Slingshot File..." );
            slingshotFilesDirectory.Create();
            slingshotFilesDirectory.CreateSubdirectory( "errors" );

            BulkImporter = new BulkImporter
            {
                OnProgress = BulkImporter_OnProgress,
                ImportUpdateOption = importUpdateType
            };
            this.SlingshotLogFile = Path.Combine( Path.GetDirectoryName( this.SlingshotFileName ), "slingshot-errors.log" );
            BulkImporter.SlingshotLogFile = this.SlingshotLogFile;
            ErrorCSVfilename = SlingshotDirectoryName.EnsureTrailingBackslash() + ERROR_CSV_FILENAME;

            dataValidator = new SlingshotDataValidator();
        }

        /// <summary>
        /// From the CSV file that was uploaded, create the intermediate CSV files which would used by the SlingshotImporter to Populate the Database.
        /// </summary>
        public void CreateIntermediateCSVFiles( Dictionary<string, string> headerMapper, EventHandler<object> onLineRead )
        {
            string personCsvFilePath = SlingshotDirectoryName.EnsureTrailingBackslash()
                + ( new SlingshotCore.Model.Person() ).GetFileName();
            string personAddressCsvFilePath = SlingshotDirectoryName.EnsureTrailingBackslash()
                + ( new SlingshotCore.Model.PersonAddress() ).GetFileName();
            string personPhoneCsvFilePath = SlingshotDirectoryName.EnsureTrailingBackslash()
                + ( new SlingshotCore.Model.PersonPhone() ).GetFileName();
            string personAttributeValueCsvFilePath = SlingshotDirectoryName.EnsureTrailingBackslash()
                + ( new SlingshotCore.Model.PersonAttributeValue() ).GetFileName();

            using ( StreamWriter personCSVFileStream = new StreamWriter( personCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personCSVWriter = new CsvWriter( personCSVFileStream ) )
            using ( StreamWriter personAddressCSVFileStream = new StreamWriter( personAddressCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personAddressCSVWriter = new CsvWriter( personAddressCSVFileStream ) )
            using ( StreamWriter personPhoneCSVFileStream = new StreamWriter( personPhoneCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personPhoneCSVWriter = new CsvWriter( personPhoneCSVFileStream ) )
            using ( StreamWriter personAttributeValueCSVFileStream = new StreamWriter( personAttributeValueCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personAttributeValueCSVWriter = new CsvWriter( personAttributeValueCSVFileStream ) )
            using ( StreamWriter uploadedPersonCsvErrorsFileStream = new StreamWriter( File.Create( ErrorCSVfilename ), Encoding.UTF8 ) )
            using ( CsvWriter uploadedPersonCsvErrorsWriter = new CsvWriter( uploadedPersonCsvErrorsFileStream ) )
            using ( StreamReader uploadedCSVFileStream = File.OpenText( SlingshotFileName ) )
            using ( CsvReader csvReader = new CsvReader( uploadedCSVFileStream ) )
            {
                personCSVWriter.WriteHeader<SlingshotCore.Model.Person>();
                personAddressCSVWriter.WriteHeader<SlingshotCore.Model.PersonAddress>();
                personPhoneCSVWriter.WriteHeader<SlingshotCore.Model.PersonPhone>();
                personAttributeValueCSVWriter.WriteHeader<SlingshotCore.Model.PersonAttributeValue>();
                long rowCount = uploadedCSVFileStream.BaseStream.Length;

                // write the headers in the errors.csv file
                csvReader.ReadHeader();
                Array.ForEach( csvReader.FieldHeaders, header => uploadedPersonCsvErrorsWriter.WriteField( header ) );
                uploadedPersonCsvErrorsWriter.WriteField( CSV_IMPORT_SUCCESS_COLUMN_NAME );
                uploadedPersonCsvErrorsWriter.WriteField( CSV_IMPORT_ERROR_COLUMN_NAME );
                uploadedPersonCsvErrorsWriter.NextRecord();

                foreach ( var csvEntry in csvReader.GetRecords<dynamic>() )
                {
                    IDictionary<string, object> csvEntryLookup = ( IDictionary<string, object> ) csvEntry;
                    SlingshotCore.Model.Person person = PersonCSVMapper.Map( csvEntryLookup, headerMapper );
                    SlingshotCore.Model.PersonAddress personAddress = PersonAddressCSVMapper.Map( csvEntryLookup, headerMapper );
                    List<SlingshotCore.Model.PersonPhone> personPhones = PersonPhoneCSVMapper.Map( csvEntryLookup, headerMapper );
                    List<SlingshotCore.Model.PersonAttributeValue> personAttributeValues = PersonAttributeValueCSVMapper
                        .Map( csvEntryLookup, headerMapper );

                    # region Create Error CSV File

                    // check if basic data is present to add the person to the database
                    try
                    {
                        dataValidator.ValidatePerson( person );
                    }
                    catch ( UploadedPersonCSVInvalidException exception )
                    {
                        string errorMessage = exception.Message + $": The {headerMapper["Id"]} Column and the {headerMapper["Family Id"]} column should not contain 0 or empty values";
                        csvEntryLookup.Values
                            .ToList()
                            .ForEach( field => uploadedPersonCsvErrorsWriter.WriteField( field ) );
                        uploadedPersonCsvErrorsWriter.WriteField( false );
                        uploadedPersonCsvErrorsWriter.WriteField( errorMessage );
                        uploadedPersonCsvErrorsWriter.NextRecord();
                        HasErrors = true;
                        continue;
                    }

                    var errorMessages = new List<string>();

                    // check address is valid
                    try
                    {
                        dataValidator.ValidateAddress( personAddress );
                    }
                    catch ( UploadedPersonCSVInvalidException exception )
                    {
                        errorMessages.Add( exception.Message );
                        personAddress = new SlingshotCore.Model.PersonAddress(); // nullify the entry for address to the slingshot
                    }

                    // check campus is valid
                    try
                    {
                        dataValidator.ValidateCampus( person );
                    }
                    catch ( UploadedPersonCSVInvalidException exception )
                    {
                        errorMessages.Add( exception.Message );

                        // pass no campus for the person to the slingshot if it is invalid
                        person.Campus.CampusId = 0;
                        person.Campus.CampusName = "";
                    }

                    // Add the final message to the error column of errors.csv
                    foreach ( string errorMessage in errorMessages )
                    {
                        personImportErrorMessageStructs.Add( new PersonImportErrorMessageStruct( person.Id, errorMessage ) );
                    }

                    string finalPersonImportErrorMessage = string.Join( "; ", errorMessages );
                    if ( finalPersonImportErrorMessage.IsNotNullOrWhiteSpace() )
                    {
                        csvEntryLookup.Values
                                .ToList()
                                .ForEach( field => uploadedPersonCsvErrorsWriter.WriteField( field ) );
                        uploadedPersonCsvErrorsWriter.WriteField( true );
                        uploadedPersonCsvErrorsWriter.WriteField( finalPersonImportErrorMessage );
                        uploadedPersonCsvErrorsWriter.NextRecord();
                    }

                    #endregion Create Error CSV File

                    int currentRowNumber = csvReader.Parser.Row - 1;
                    onLineRead( this, currentRowNumber );

                    personCSVWriter.WriteRecord( person );
                    personAddressCSVWriter.WriteRecord( personAddress );
                    personPhones.ForEach( personPhone =>
                    {
                        personPhoneCSVWriter.WriteRecord( personPhone );
                    } );
                    personAttributeValues.ForEach( personAttributeValue =>
                    {
                        personAttributeValueCSVWriter.WriteRecord( personAttributeValue );
                    } );
                }
            }
            HasErrors = personImportErrorMessageStructs.Count > 0;
        }

        public void ClearRedundantFilesAfterImport()
        {
            File.Delete( SlingshotFileName );
            // delete all files except the errors.csv file
            Array.ForEach( Directory.GetFiles( SlingshotDirectoryName ), delegate ( string path )
            { File.Delete( path ); } );

        }

        /// <summary>
        /// Add the CSV Import ErrorMessages to the Person Profile of the Corresponding Persons.
        /// </summary>
        public void AddPersonCSVImportErrorNotes()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                PersonService personService = new PersonService( rockContext );
                NoteService noteService = new NoteService( rockContext );
                int personCSVImportErrorNoteTypeId = new NoteTypeService( rockContext )
                    .Get( SystemGuid.NoteType.PERSON_CSV_IMPORT_ERROR_NOTE )
                    .Id;

                foreach ( PersonImportErrorMessageStruct personImportErrorMessage in personImportErrorMessageStructs )
                {
                    int personId = personService
                        .FromForeignSystem( ForeignSystemKey, personImportErrorMessage.Id )
                        .Id;
                    Note note = new Note
                    {
                        NoteTypeId = personCSVImportErrorNoteTypeId,
                        IsAlert = true,
                        IsSystem = true,
                        Text = personImportErrorMessage.Message,
                        EntityId = personId
                    };
                    noteService.Add( note );
                }
                rockContext.SaveChanges();
            }
        }

    }
}
