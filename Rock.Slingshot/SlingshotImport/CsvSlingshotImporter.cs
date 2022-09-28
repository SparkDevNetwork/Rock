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
    public class CsvSlingshotImporter : SlingshotImporter
    {
        private const string CSV_IMPORT_ERROR_COLUMN_NAME = "CSV Import Errors";
        private readonly string CSV_IMPORT_SUCCESS_COLUMN_NAME = "CSV Import Success";
        private const string ERROR_CSV_FILENAME = "errors/errors.csv";

        // variable to perform validation on the Grade field
        private readonly IEnumerable<string> VALID_GRADE_DESCRIPTIONS = Web.Cache.DefinedTypeCache.Get( SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() )
            .DefinedValues.Select( definedValue => definedValue.Description );

        private readonly IEnumerable<string> VALID_GRADES_ABBREVIATIONS = Web.Cache.DefinedTypeCache.Get( SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() )?.DefinedValues
            .Select( a => a.AttributeValues["Abbreviation"]?.Value )
            .Where( a => !string.IsNullOrWhiteSpace( a ) );

        private readonly ICollection<string> VALID_GRADES;

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

        public CsvSlingshotImporter( string uploadedPersonCSVFileName, string foreignSystemKey, string csvDataType, BulkImporter.ImportUpdateType importUpdateType, EventHandler<object> onProgress = null ) : base()
        {
            this.Results = new Dictionary<string, string>();
            VALID_GRADES = VALID_GRADE_DESCRIPTIONS.Concat( VALID_GRADES_ABBREVIATIONS ).ToHashSet();

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
            string personNotesCsvFilePath = SlingshotDirectoryName.EnsureTrailingBackslash()
                + ( new SlingshotCore.Model.PersonNote() ).GetFileName();

            using ( StreamWriter personCSVFileStream = new StreamWriter( personCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personCSVWriter = new CsvWriter( personCSVFileStream ) )
            using ( StreamWriter personAddressCSVFileStream = new StreamWriter( personAddressCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personAddressCSVWriter = new CsvWriter( personAddressCSVFileStream ) )
            using ( StreamWriter personPhoneCSVFileStream = new StreamWriter( personPhoneCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personPhoneCSVWriter = new CsvWriter( personPhoneCSVFileStream ) )
            using ( StreamWriter personAttributeValueCSVFileStream = new StreamWriter( personAttributeValueCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personAttributeValueCSVWriter = new CsvWriter( personAttributeValueCSVFileStream ) )
            using ( StreamWriter personNotesCSVFileStream = new StreamWriter( personNotesCsvFilePath, false, Encoding.UTF8 ) )
            using ( CsvWriter personNotesCSVWriter = new CsvWriter( personNotesCSVFileStream ) )
            using ( StreamWriter uploadedPersonCsvErrorsFileStream = new StreamWriter( File.Create( ErrorCSVfilename ), Encoding.UTF8 ) )
            using ( CsvWriter uploadedPersonCsvErrorsWriter = new CsvWriter( uploadedPersonCsvErrorsFileStream ) )
            using ( StreamReader uploadedCSVFileStream = File.OpenText( SlingshotFileName ) )
            using ( CsvReader csvReader = new CsvReader( uploadedCSVFileStream ) )
            {
                personCSVWriter.WriteHeader<SlingshotCore.Model.Person>();
                personAddressCSVWriter.WriteHeader<SlingshotCore.Model.PersonAddress>();
                personPhoneCSVWriter.WriteHeader<SlingshotCore.Model.PersonPhone>();
                personAttributeValueCSVWriter.WriteHeader<SlingshotCore.Model.PersonAttributeValue>();
                personNotesCSVWriter.WriteHeader<SlingshotCore.Model.PersonNote>();
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
                    SlingshotCore.Model.Person person = PersonCsvMapper.Map( csvEntryLookup, headerMapper, out HashSet<string> errorMessages );
                    SlingshotCore.Model.PersonAddress personAddress = PersonAddressCsvMapper.Map( csvEntryLookup, headerMapper );
                    List<SlingshotCore.Model.PersonPhone> personPhones = PersonPhoneCsvMapper.Map( csvEntryLookup, headerMapper, ref errorMessages );
                    List<SlingshotCore.Model.PersonAttributeValue> personAttributeValues = PersonAttributeValueCsvMapper
                        .Map( csvEntryLookup, headerMapper );
                    SlingshotCore.Model.PersonNote personNote = PersonNoteCsvMapper.Map( csvEntryLookup, headerMapper, ref errorMessages );

                    # region Create Error CSV File

                    // check if basic data is present to add the person to the database
                    {
                        if ( !dataValidator.ValidatePerson( person, out string errorMessage ) )
                        {
                            csvEntryLookup.Values
                                .ToList()
                                .ForEach( field => uploadedPersonCsvErrorsWriter.WriteField( field ) );
                            uploadedPersonCsvErrorsWriter.WriteField( false );
                            uploadedPersonCsvErrorsWriter.WriteField( errorMessage );
                            uploadedPersonCsvErrorsWriter.NextRecord();
                            HasErrors = true;
                            continue;
                        }
                    }

                    // check address is valid
                    {
                        var addressInvalidErrorMessage = string.Empty;
                        if ( !dataValidator.ValidateAddress( personAddress, out addressInvalidErrorMessage ) )
                        {
                            errorMessages.Add( addressInvalidErrorMessage );
                            personAddress = new SlingshotCore.Model.PersonAddress(); // nullify the entry for address to the slingshot
                        }
                    }

                    // check campus is valid
                    {
                        if ( !dataValidator.ValidateCampus( person, out string errorMessage ) )
                        {
                            errorMessages.Add( errorMessage );

                            // pass no campus for the person to the slingshot if it is invalid
                            person.Campus.CampusId = 0;
                            person.Campus.CampusName = "";
                        }
                    }

                    // check if phone numbers are valid and eliminate the invalid ones
                    {
                        var invalidPhoneNumbers = new HashSet<SlingshotCore.Model.PersonPhone>();

                        foreach ( SlingshotCore.Model.PersonPhone personPhone in personPhones )
                        {
                            if ( !dataValidator.ValidatePhoneNumber( personPhone, out string errorMessage ) )
                            {
                                errorMessages.Add( errorMessage );
                                invalidPhoneNumbers.Add( personPhone );
                            }
                        }

                        personPhones.RemoveAll( invalidPhoneNumbers );
                    }

                    // check if grade is valid
                    {
                        if ( !string.IsNullOrEmpty( person.Grade ) && !VALID_GRADES.Contains( person.Grade ) )
                        {
                            errorMessages.Add( $"Could not find the Grade {person.Grade}" );
                        }
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

                    if ( personNote != null && personNote.Text.IsNotNullOrWhiteSpace() )
                    {
                        personNotesCSVWriter.WriteRecord( personNote );
                    }
                }
            }
            HasErrors = HasErrors || personImportErrorMessageStructs.Count > 0;
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

                // Group the error messages by person so we only create one "error" alert note per person.
                var errorsGroupedByPerson = personImportErrorMessageStructs.GroupBy( em => em.Id );
                foreach ( var personImportErrorMessageGroup in errorsGroupedByPerson )
                {
                    var errorsText = new StringBuilder();
                    Person person = null;

                    // Now collect all the messages into one string:
                    foreach ( PersonImportErrorMessageStruct personImportErrorMessage in personImportErrorMessageGroup )
                    {
                        // Fetch this only once per person grouping...
                        if ( person == null )
                        {
                            person = personService.FromForeignSystem( ForeignSystemKey, personImportErrorMessage.Id );
                        }

                        errorsText.Append( personImportErrorMessage.Message + System.Environment.NewLine );
                    }

                    // If we didn't find a person, then skip to the next group.
                    if ( person == null )
                    {
                        break;
                    }

                    var note = new Note
                    {
                        NoteTypeId = personCSVImportErrorNoteTypeId,
                        IsAlert = true,
                        IsSystem = true,
                        EntityId = person.Id,
                        Text = errorsText.ToString()
                    };

                    noteService.Add( note );
                    rockContext.SaveChanges();
                }
            }
        }

    }
}
