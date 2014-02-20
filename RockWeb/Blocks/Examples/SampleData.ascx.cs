// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.IO;
using System.Linq;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// Block that can load sample data into your Rock database.
    /// </summary>
    [DisplayName( "Rock Solid Church Sample Data" )]
    [Category( "Examples" )]
    [Description( "Loads the Rock Solid Church sample data into your Rock system." )]
    public partial class SampleData : Rock.Web.UI.RockBlock
    {
        #region Fields
        /// <summary>
        /// The Url to the sample data
        /// </summary>
        private static string _xmlFileUrl = "http://storage.rockrms.com/sampledata/sampledata.xml";

        /// <summary>
        /// Holds the Person Image binary file type.
        /// </summary>
        private static BinaryFileType _binaryFileType = new BinaryFileTypeService().Get( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );

        /// <summary>
        /// The storage type to use for the people photos.
        /// </summary>
        private static EntityTypeCache _storageEntityType = EntityTypeCache.Read( Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE.AsGuid() );

        /// <summary>
        /// Percent of additional time someone tends to NOT attend during the summer months (7-9)
        /// </summary>
        private int summerPercentFactor = 30;

        /// <summary>
        /// A random number generator for use when calculating random attendance data.
        /// </summary>
        private static Random _random = new Random( (int)DateTime.Now.Ticks );

        /// <summary>
        /// The number of characters (length) that security codes should be.
        /// </summary>
        private static int _securityCodeLength = 5;

        /// <summary>
        /// A little lookup list for finding a group/location appropriate for the child's attendance data
        /// </summary>
        protected static List<ClassGroupLocation> _classes = new List<ClassGroupLocation>
        {
            new ClassGroupLocation { GroupId = 25, LocationId = 4, MinAge =  0.0, MaxAge = 3.0,   Name = "Nursery - Bunnies Room"  },
            new ClassGroupLocation { GroupId = 26, LocationId = 5, MinAge =  0.0, MaxAge = 3.99,  Name = "Crawlers/Walkers - Kittens Room" },
            new ClassGroupLocation { GroupId = 27, LocationId = 6, MinAge =  0.0, MaxAge = 5.99,  Name = "Preschool - Puppies Room" },
            new ClassGroupLocation { GroupId = 28, LocationId = 7, MinAge =  4.75, MaxAge = 8.75, Name = "Grades K-1 - Bears Room" },
            new ClassGroupLocation { GroupId = 29, LocationId = 8, MinAge =   6.0, MaxAge = 10.99, Name = "Grades 2-3 - Bobcats Room" },
            new ClassGroupLocation { GroupId = 30, LocationId = 9, MinAge =   8.0, MaxAge = 13.99, Name = "Grades 4-6 - Outpost Room" },
            new ClassGroupLocation { GroupId = 31, LocationId = 10, MinAge = 12.0, MaxAge = 15.0,  Name = "Grades 7-8 - Warehouse" },
            new ClassGroupLocation { GroupId = 32, LocationId = 11, MinAge = 13.0, MaxAge = 19.0,  Name = "Grades 9-12 - Garage" },
        };

        public static Dictionary<string, int> locations = new Dictionary<string, int>()
        {
	        { "Bunnies Room", 4 },
	        { "Kittens Room", 5 },
	        { "Puppies Room", 6 },
	        { "Bears Room",   7 },
	        { "Bobcats Room", 8 },
	        { "Outpost Room", 9 },
	        { "the Warehouse", 10 },
	        { "the Garage",    11 }
        };

        public static Dictionary<string, int> groups = new Dictionary<string, int>()
        {
	        { "Nursery",          25 },
	        { "Crawlers/Walkers", 26 },
	        { "Preschool",        27 },
	        { "Grades K-1",       28 },
	        { "Grades 2-3",       29 },
	        { "Grades 4-6",       30 },
	        { "Grades 7-8",       31 },
	        { "Grades 9-12",      32 }
        };

        /// <summary>
        /// Magic kiosk Id used for attendance data.
        /// </summary>
        private static int _kioskDeviceId = 2;
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

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                // added for your convience
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// This is the entry point for when the user clicks the "load data" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bbtnLoadData_Click( object sender, EventArgs e )
        {
            string saveFile = Path.Combine( MapPath( "~" ), "sampledata1.xml" );

            try
            {
                if ( DownloadFile( _xmlFileUrl, saveFile ) )
                {
                    ProcessXml( saveFile );
                    nbMessage.Visible = true;
                    nbMessage.Title = "Success";
                    nbMessage.NotificationBoxType = NotificationBoxType.Success;
                    nbMessage.Text = string.Format( "Happy tire-kicking! The data is in your database. Hint: try <a href='{0}'>searching for the Decker family</a>.", ResolveRockUrl( "~/Person/Search/name/Decker" ) );
                    bbtnLoadData.Visible = false;
                }
            }
            catch ( Exception ex )
            {
                nbMessage.Visible = true;
                nbMessage.Title = "Oops!";
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Text = string.Format( "That wasn't supposed to happen.  The error was:<br/>{0}<br/>{1}<br/>{2}", ex.Message.ConvertCrLfToHtmlBr(),
                    ( ex.InnerException != null ) ? ex.InnerException.Message.ConvertCrLfToHtmlBr() : "", ex.StackTrace.ConvertCrLfToHtmlBr() );
            }

            if ( File.Exists( saveFile ) )
            {
                File.Delete( saveFile );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the PageLiquid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Download the given fileUrl and store it at the fileOutput.
        /// </summary>
        /// <param name="fileUrl">The file Url to fetch.</param>
        /// <param name="fileOutput">The full path location to store the file.</param>
        /// <returns></returns>
        private bool DownloadFile( string fileUrl, string fileOutput )
        {
            bool isSuccess = false;
            try
            {
                using ( WebClient client = new WebClient() )
                {
                    client.DownloadFile( fileUrl, fileOutput );
                }
                isSuccess = true;
            }
            catch ( WebException ex )
            {
                nbMessage.Text = ex.Message;
                nbMessage.Visible = true;
            }

            return isSuccess;
        }

        /// <summary>
        /// Process all the data in the XML file; deleting stuff and then adding stuff.
        /// as per https://github.com/SparkDevNetwork/Rock/wiki/z.-Rock-Solid-Demo-Church-Specification#wiki-xml-data
        /// </summary>
        /// <param name="sampleXmlFile"></param>
        private void ProcessXml( string sampleXmlFile )
        {
            var xdoc = XDocument.Load( sampleXmlFile );

            RockTransactionScope.WrapTransaction( () =>
            {
                using ( new UnitOfWorkScope() )
                {
                    var families = xdoc.Element( "data" ).Element( "families" );
                    if ( families != null )
                    {
                        // First we'll clean up and delete any previously created data such as
                        // families, addresses, people, photos, attendance data, etc.
                        PersonService personService = new PersonService();
                        PhoneNumberService phoneNumberService = new PhoneNumberService();
                        PersonViewedService personViewedService = new PersonViewedService();
                        BinaryFileService binaryFileService = new BinaryFileService();

                        DeleteExistingFamilyData( families, personService, phoneNumberService, personViewedService, binaryFileService );
                    }

                    // Now create the family along with its members.
                    foreach ( var elemFamily in families.Elements( "family" ) )
                    {
                        Guid guid = Guid.Parse( elemFamily.Attribute( "guid" ).Value );
                        var familyMembers = FamilyMembersFromXml( elemFamily.Element( "members" ) );

                        GroupService groupService = new GroupService();

                        Group family = groupService.SaveNewFamily( familyMembers, 1, false, CurrentPersonAlias );
                        family.Guid = guid;
                        //groupService.Save( family );

                        // Add the families address(es)
                        AddFamilyAddresses( groupService, family, elemFamily.Element( "addresses" ) );

                        // Add their attendance data
                        AddFamilyAttendance( family, elemFamily );

                        groupService.Save( family, CurrentPersonAlias );
                    }
                }
            } );
        }

        /// <summary>
        /// Deletes the family's addresses, phone numbers, photos, viewed records, and people.
        /// </summary>
        /// <param name="families"></param>
        /// <param name="personService"></param>
        /// <param name="phoneNumberService"></param>
        /// <param name="personViewedService"></param>
        /// <param name="binaryFileService"></param>
        private void DeleteExistingFamilyData( XElement families, PersonService personService, PhoneNumberService phoneNumberService, PersonViewedService personViewedService, BinaryFileService binaryFileService )
        {
            foreach ( var elemFamily in families.Elements( "family" ) )
            {
                Guid guid = Guid.Parse( elemFamily.Attribute( "guid" ).Value );
                List<Guid> peopleGuids = new List<Guid>();

                GroupService groupService = new GroupService();
                Group family = groupService.Get( guid );
                if ( family != null )
                {
                    // Delete addresses
                    GroupLocationService groupLocationService = new GroupLocationService();
                    if ( family.GroupLocations.Count > 0 )
                    {
                        foreach ( var familyAddress in family.GroupLocations.ToList() )
                        {
                            family.GroupLocations.Remove( familyAddress );
                            groupLocationService.Delete( familyAddress, CurrentPersonAlias );
                            groupLocationService.Save( familyAddress, CurrentPersonAlias );
                        }
                    }

                    // Delete family members
                    var familyMemberService = new GroupMemberService();
                    var familyMembers = familyMemberService.GetByGroupId( family.Id );
                    foreach ( var member in familyMembers.ToList() )
                    {
                        peopleGuids.Add( member.Person.Guid );
                        family.Members.Remove( member );
                        familyMemberService.Delete( member );
                        familyMemberService.Save( member, CurrentPersonAlias );
                    }

                    // Delete the people records
                    var people = personService.GetByGuids( peopleGuids );
                    string errorMessage;

                    foreach ( var person in people )
                    {
                        // delete the photos
                        List<int> photoIds = people.Where( p => p.PhotoId != null ).Select( a => (int)a.PhotoId ).ToList();
                        foreach ( var photo in binaryFileService.GetByIds( photoIds ) )
                        {
                            binaryFileService.Delete( photo );
                            binaryFileService.Save( photo );
                        }

                        // delete phone numbers
                        foreach ( var phone in phoneNumberService.GetByPersonId( person.Id ) )
                        {
                            if ( phone != null )
                            {
                                phoneNumberService.Delete( phone, CurrentPersonAlias );
                                phoneNumberService.Save( phone, CurrentPersonAlias );
                            }
                        }

                        // delete person viewed records
                        foreach ( var view in personViewedService.GetByTargetPersonId( person.Id ) )
                        {
                            personViewedService.Delete( view );
                            personViewedService.Save( view );
                        }

                        // person.GivingGroup = null;
                        if ( personService.CanDelete( person, out errorMessage ) )
                        {
                            personService.Delete( person, CurrentPersonAlias );
                        }
                        personService.Save( person, CurrentPersonAlias );
                    }

                    // Now delete the family
                    groupService.Delete( family, CurrentPersonAlias );
                    groupService.Save( family, CurrentPersonAlias );
                }
            }
        }

        /// <summary>
        /// Grabs the necessary parameters from the XML and then calls the CreateAttendance() method
        /// to generate all the attendance data for the family.
        /// </summary>
        /// <param name="family"></param>
        /// <param name="elemFamily"></param>
        private void AddFamilyAttendance( Group family, XElement elemFamily )
        {
            // return from here if there's not startingAttendance date
            if ( elemFamily.Attribute( "startingAttendance" ).Value == null )
            {
                return;
            }

            // get some variables we'll need to create the attendance records
            DateTime startingDate = DateTime.Parse( elemFamily.Attribute( "startingAttendance" ).Value );
            DateTime endDate = RockDateTime.Now;

            // If the XML specifies an endingAttendance date use it, otherwise use endingAttendanceWeeksAgo
            // to calculate the end date otherwise we'll just use the current date as the end date.
            if ( elemFamily.Attribute( "endingAttendance" ) != null )
            {
                DateTime.TryParse( elemFamily.Attribute( "endingAttendance" ).Value, out endDate );
            }
            else if ( elemFamily.Attribute( "endingAttendanceWeeksAgo" ) != null )
            {
                int endingWeeksAgo = 0;
                int.TryParse( elemFamily.Attribute( "endingAttendanceWeeksAgo" ).Value, out endingWeeksAgo );
                endDate = RockDateTime.Now.AddDays( -7 * endingWeeksAgo );
            }

            int pctAttendance = 100;
            if ( elemFamily.Attribute( "percentAttendance" ) != null )
            {
                int.TryParse( elemFamily.Attribute( "percentAttendance" ).Value, out pctAttendance );
            }

            int pctAttendedRegularService = 100;
            if ( elemFamily.Attribute( "percentAttendedRegularService" ) != null )
            {
                int.TryParse( elemFamily.Attribute( "percentAttendedRegularService" ).Value, out pctAttendedRegularService );
            }

            int scheduleId = 3;
            if ( elemFamily.Attribute( "attendingScheduleId" ) != null )
            {
                int.TryParse( elemFamily.Attribute( "attendingScheduleId" ).Value, out scheduleId );
            }

            int altScheduleId = 4;
            if ( elemFamily.Attribute( "attendingAltScheduleId" ) != null )
            {
                int.TryParse( elemFamily.Attribute( "attendingAltScheduleId" ).Value, out altScheduleId );
            }

            CreateAttendance( family.Members, startingDate, endDate, pctAttendance, pctAttendedRegularService, scheduleId, altScheduleId );
        }

        /// <summary>
        /// Adds attendance data for each child for each weekend since the starting date up to
        /// the weekend ending X weeks ago (endingWeeksAgo).  It will randomly skip a weekend
        /// based on the percentage (pctAttendance) and it will vary which service they attend
        /// between the scheduleId and altScheduleId based on the percentage (pctAttendedRegularService)
        /// given.
        /// </summary>
        /// <param name="familyMembers"></param>
        /// <param name="startingDate">The first date of attendance</param>
        /// <param name="endDate">The end date of attendance</param>
        /// <param name="pctAttendance"></param>
        /// <param name="pctAttendedRegularService"></param>
        /// <param name="scheduleId"></param>
        /// <param name="altScheduleId"></param>
        private void CreateAttendance( ICollection<GroupMember> familyMembers, DateTime startingDate, DateTime endDate, int pctAttendance, int pctAttendedRegularService, int scheduleId, int altScheduleId )
        {
            Guid childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

            // foreach weekend between the starting and ending date...
            for ( DateTime date = startingDate; date <= endDate; date = date.AddDays( 7 ) )
            {
                // set an additional factor 
                int summerFactor = ( 7 <= date.Month && date.Month <= 9 ) ? summerPercentFactor : 0;
                if ( _random.Next( 0, 100 ) > pctAttendance - summerFactor )
                {
                    continue; // skip this week
                }

                // which service did they attend
                int serviceSchedId = ( _random.Next( 0, 100 ) > pctAttendedRegularService ) ? scheduleId : altScheduleId;

                // randomize check-in time slightly by +- 0-15 minutes (and 1 out of 4 times being late)
                int minutes = _random.Next( 0, 15 );
                int plusMinus = ( _random.Next( 0, 4 ) == 0 ) ? 1 : -1;
                int randomSeconds = _random.Next( 0, 60 );

                DateTime checkinDateTime = date.AddMinutes( Convert.ToDouble( plusMinus * minutes ) ).AddSeconds( randomSeconds );

                // foreach child in the family
                foreach ( var member in familyMembers.Where( m => m.GroupRole.Guid == childGuid ) )
                {
                    // Find a class room (group location)
                    // TODO -- someday perhaps we will change this to actually find a real GroupLocationSchedule record
                    var item = (from classroom in _classes
                                    where member.Person.AgePrecise >= classroom.MinAge
                                    && member.Person.AgePrecise <= classroom.MaxAge
                                    orderby classroom.MinAge, classroom.MaxAge
                                    select classroom).FirstOrDefault();

                    // If no suitable classroom was found, skip
                    if ( item == null )
                    {
                        continue;
                    }

                    // Only create one attendance record per day for each person/schedule/group/location
                    AttendanceCode attendanceCode = new AttendanceCode()
                    {
                        Code = GenerateRandomCode( _securityCodeLength ),
                        IssueDateTime = RockDateTime.Now,
                    };

                    Attendance attendance = new Attendance()
                    {
                        ScheduleId = scheduleId,
                        GroupId = item.GroupId,
                        LocationId = item.LocationId,
                        DeviceId = _kioskDeviceId,
                        PersonId = member.PersonId,
                        AttendanceCode = attendanceCode,
                        StartDateTime = checkinDateTime,
                        EndDateTime = null,
                        DidAttend = true
                    };

                    member.Person.Attendances.Add( attendance );
                }
            }
        }

        private static string GenerateRandomCode( int len )
        {
            string chars = "BCDFGHJKMNPQRTVWXYZ0123456789";
            var code = Enumerable.Range( 0, len ).Select( x => chars[_random.Next( 0, chars.Length )] );
            return new string( code.ToArray() );
        }

        /// <summary>
        /// Takes the given XML element and creates a family member collection.
        /// If the person already exists, their record will be loaded otherwise
        /// a new person will be created using all the attributes for the given
        /// 'person' tag.
        /// </summary>
        /// <param name="elemMembers"></param>
        /// <returns></returns>
        private List<GroupMember> FamilyMembersFromXml( XElement elemMembers )
        {
            var familyMembers = new List<GroupMember>();

            // First add each person to the familyMembers collection
            foreach ( var personElem in elemMembers.Elements( "person" ) )
            {
                var groupMember = new GroupMember();
                Guid guid = Guid.Parse( personElem.Attribute( "guid" ).Value );

                // Attempt to find an existing person...
                Person person = new PersonService().Get( guid );
                if ( person == null )
                {
                    person = new Person();
                    person.Guid = guid;
                    person.FirstName = personElem.Attribute( "firstName" ).Value;
                    if ( personElem.Attribute( "nickName" ) != null )
                    {
                        person.NickName = personElem.Attribute( "nickName" ).Value;
                    }

                    if ( personElem.Attribute( "lastName" ) != null )
                    {
                        person.LastName = personElem.Attribute( "lastName" ).Value;
                    }

                    //person.Age = int.Parse( personElem.Attribute( "age" ).Value );
                    if ( personElem.Attribute( "birthDate" ) != null )
                    {
                        person.BirthDate = DateTime.Parse( personElem.Attribute( "birthDate" ).Value );
                    }

                    if ( personElem.Attribute( "email" ) != null )
                    {
                        var emailAddress = personElem.Attribute( "email" ).Value;
                        if ( emailAddress.IsValidEmail() )
                        {
                            person.Email = personElem.Attribute( "email" ).Value;
                            person.IsEmailActive = personElem.Attribute( "emailIsActive" ) != null && personElem.Attribute( "emailIsActive" ).Value.FromTrueFalse();
                            person.DoNotEmail = personElem.Attribute( "emailDoNotEmail" ) != null && personElem.Attribute( "emailDoNotEmail" ).Value.FromTrueFalse();
                        }
                    }

                    if ( personElem.Attribute( "photoUrl" ) != null )
                    {
                        person.PhotoId = SavePhoto( personElem.Attribute( "photoUrl" ).Value );
                    }

                    if ( personElem.Attribute( "recordType" ) != null && personElem.Attribute( "recordType" ).Value == "person" )
                    {
                        person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    }

                    if ( personElem.Attribute( "maritalStatus" ) != null && personElem.Attribute( "maritalStatus" ).Value == "married" )
                    {
                        person.MaritalStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;
                    }

                    switch ( personElem.Attribute( "recordStatus" ).Value )
                    {
                        case "active":
                            person.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                            break;
                        case "inactive":
                            person.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;
                            break;
                        default:
                            person.RecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;
                            break;
                    }

                    if ( personElem.Attribute( "gender" ) != null )
                    {
                        switch ( personElem.Attribute( "gender" ).Value.ToLower() )
                        {
                            case "m":
                            case "male":
                                person.Gender = Gender.Male;
                                break;
                            case "f":
                            case "female":
                                person.Gender = Gender.Female;
                                break;
                            default:
                                person.Gender = Gender.Unknown;
                                break;
                        }
                    }
                    else
                    {
                        person.Gender = Gender.Unknown;
                    }

                    if ( personElem.Attribute( "connectionStatus" ) != null )
                    {
                        switch ( personElem.Attribute( "connectionStatus" ).Value.ToLower() )
                        {
                            case "member":
                                person.ConnectionStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER.AsGuid() ).Id;
                                break;
                            case "attendee":
                                person.ConnectionStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_ATTENDEE.AsGuid() ).Id;
                                break;
                            default:
                                person.ConnectionStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() ).Id;
                                break;
                        }
                    }

                    if ( personElem.Attribute( "homePhone" ) != null && !string.IsNullOrEmpty( personElem.Attribute( "homePhone" ).Value.Trim( '"' ) ) )
                    {
                        var phoneNumber = new PhoneNumber
                        {
                            NumberTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).Id,
                            Number = personElem.Attribute( "homePhone" ).Value
                        };
                        person.PhoneNumbers.Add( phoneNumber );
                    }

                    if ( personElem.Attribute( "mobilePhone" ) != null && !string.IsNullOrEmpty( personElem.Attribute( "mobilePhone" ).Value.Trim( '"' ) ) )
                    {
                        var phoneNumber = new PhoneNumber
                        {
                            NumberTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id,
                            Number = personElem.Attribute( "mobilePhone" ).Value
                        };
                        person.PhoneNumbers.Add( phoneNumber );
                    }

                    if ( personElem.Attribute( "workPhone" ) != null && !string.IsNullOrEmpty( personElem.Attribute( "workPhone" ).Value.Trim( '"' ) ) )
                    {
                        var phoneNumber = new PhoneNumber
                        {
                            NumberTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).Id,
                            Number = personElem.Attribute( "workPhone" ).Value
                        };
                        person.PhoneNumbers.Add( phoneNumber );
                    }
                }

                groupMember.Person = person;

                if ( personElem.Attribute( "familyRole" ) != null && personElem.Attribute( "familyRole" ).Value.ToLower() == "adult" )
                {
                    groupMember.GroupRoleId = new GroupTypeRoleService().Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
                }
                else
                {
                    groupMember.GroupRoleId = new GroupTypeRoleService().Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;
                }

                familyMembers.Add( groupMember );
            }

            return familyMembers;
        }

        /// <summary>
        /// Fetches the given remote photoUrl and stores it locally in the binary file table
        /// then returns Id of the binary file.
        /// </summary>
        /// <param name="photoUrl">a URL to a photo (jpg, png, bmp, tiff).</param>
        /// <returns>Id of the binaryFile</returns>
        private int? SavePhoto( string photoUrl )
        {
            // always create a new BinaryFile record of IsTemporary when a file is uploaded
            BinaryFile binaryFile = new BinaryFile();
            binaryFile.IsTemporary = true;
            binaryFile.BinaryFileTypeId = _binaryFileType.Id;
            binaryFile.FileName = Path.GetFileName( photoUrl );
            binaryFile.Data = new BinaryFileData();
            binaryFile.SetStorageEntityTypeId( _storageEntityType.Id );

            var webClient = new WebClient();
            try
            {
                binaryFile.Data.Content = webClient.DownloadData( photoUrl );

                if ( webClient.ResponseHeaders != null )
                {
                    binaryFile.MimeType = webClient.ResponseHeaders["content-type"];
                }
                else
                {
                    switch ( Path.GetExtension( photoUrl ) )
                    {
                        case ".jpg":
                        case ".jpeg":
                            binaryFile.MimeType = "image/jpg";
                            break;
                        case ".png":
                            binaryFile.MimeType = "image/png";
                            break;
                        case ".gif":
                            binaryFile.MimeType = "image/gif";
                            break;
                        case ".bmp":
                            binaryFile.MimeType = "image/bmp";
                            break;
                        case ".tiff":
                            binaryFile.MimeType = "image/tiff";
                            break;
                        case ".svg":
                        case ".svgz":
                            binaryFile.MimeType = "image/svg+xml";
                            break;
                        default:
                            throw new NotSupportedException( string.Format( "unknown MimeType for {0}", photoUrl ) );
                    }
                }

                var binaryFileService = new BinaryFileService();
                binaryFileService.Add( binaryFile );
                binaryFileService.Save( binaryFile );
                return binaryFile.Id;
            }
            catch ( WebException )
            {
                return null;
            }
        }

        /// <summary>
        /// Adds the given addresses in the xml snippet to the given family.
        /// </summary>
        /// <param name="groupService"></param>
        /// <param name="family"></param>
        /// <param name="addresses"></param>
        private void AddFamilyAddresses( GroupService groupService, Group family, XElement addresses )
        {
            // First add each person to the familyMembers collection
            foreach ( var addressElem in addresses.Elements( "address" ) )
            {
                var addressType = addressElem.Attribute( "type" ).Value;
                var street1 = addressElem.Attribute( "street1" ).Value;
                var street2 = addressElem.Attribute( "street2" ).Value;
                var city = addressElem.Attribute( "city" ).Value;
                var state = addressElem.Attribute( "state" ).Value;
                var zip = addressElem.Attribute( "zip" ).Value;
                var lat = addressElem.Attribute( "lat" ).Value;
                var lng = addressElem.Attribute( "long" ).Value;

                string locationTypeGuid;

                switch ( addressType )
                {
                    case "home":
                        locationTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME;
                        break;
                    case "work":
                        locationTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK;
                        break;
                    case "previous":
                        locationTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS;
                        break;
                    default:
                        throw new NotSupportedException( string.Format( "unknown addressType: {0}", addressType ) );
                }

                // TODO add latitude and longitude
                groupService.AddNewFamilyAddress( family, locationTypeGuid, street1, street2, city, state, zip, CurrentPersonAlias );
            }
        }

        #endregion

        # region Helper Class
        protected class ClassGroupLocation
        {
            public string Name { get; set; }
            public int GroupId { get; set; }
            public int LocationId { get; set; }
            public double MinAge { get; set; }
            public double MaxAge { get; set; }
        }
        #endregion
    }
}