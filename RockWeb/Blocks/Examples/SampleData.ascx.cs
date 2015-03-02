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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System.Globalization;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// Block that can load sample data into your Rock database.
    /// Dev note: You can set the XML Document Url setting to your local
    /// file when you're testing new data.  Something like C:\Misc\Rock\Documentation\sampledata.xml
    /// </summary>
    [DisplayName( "Rock Solid Church Sample Data" )]
    [Category( "Examples" )]
    [Description( "Loads the Rock Solid Church sample data into your Rock system." )]

    [TextField( "XML Document URL", "The URL for the input sample data XML document.", false, "http://storage.rockrms.com/sampledata/sampledata.xml", "", 1 )]
    [BooleanField( "Fabricate Attendance", "If true, then fake attendance data will be fabricated (if the right parameters are in the xml)", true, "", 2 )]
    [BooleanField( "Enable Stopwatch", "If true, a stopwatch will be used to time each of the major operations.", false, "", 3 )]
    public partial class SampleData : Rock.Web.UI.RockBlock
    {
        #region Fields

        /// <summary>
        /// Stopwatch used to measure time during certain operations.
        /// </summary>
        private Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// StringBuilder is used to build the stopwatch trace for certain operations.
        /// </summary>
        private StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// Holds the Person Image binary file type.
        /// </summary>
        private static BinaryFileType _binaryFileType = new BinaryFileTypeService( new RockContext() ).Get( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );

        /// <summary>
        /// The Binary file type settings
        /// </summary>
        private string _binaryFileTypeSettings = string.Empty;

        /// <summary>
        /// The id for the "child" role of a family.
        /// </summary>
        private static int _childRoleId = new GroupTypeRoleService( new RockContext() ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;

        /// <summary>
        /// The id for the "adult" role of a family.
        /// </summary>
        private static int _adultRoleId = new GroupTypeRoleService( new RockContext() ).Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;

        /// <summary>
        /// The Entity Type Id for the Person entities.
        /// </summary>
        private static int _personEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

        /// <summary>
        /// The storage type to use for the people photos.
        /// </summary>
        private static EntityType _storageEntityType = _binaryFileType.StorageEntityType;

        /// <summary>
        /// The Autnentication Database entity type.
        /// </summary>
        private static int _authenticationDatabaseEntityTypeId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id;

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
        private static List<ClassGroupLocation> _classes = new List<ClassGroupLocation>
        {
            new ClassGroupLocation { GroupId = 25, LocationId = 4, MinAge = 0.0, MaxAge = 3.0, Name = "Nursery - Bunnies Room" },
            new ClassGroupLocation { GroupId = 26, LocationId = 5, MinAge = 0.0, MaxAge = 3.99, Name = "Crawlers/Walkers - Kittens Room" },
            new ClassGroupLocation { GroupId = 27, LocationId = 6, MinAge = 0.0, MaxAge = 5.99, Name = "Preschool - Puppies Room" },
            new ClassGroupLocation { GroupId = 28, LocationId = 7, MinAge = 4.75, MaxAge = 8.75, Name = "Grades K-1 - Bears Room" },
            new ClassGroupLocation { GroupId = 29, LocationId = 8, MinAge = 6.0, MaxAge = 10.99, Name = "Grades 2-3 - Bobcats Room" },
            new ClassGroupLocation { GroupId = 30, LocationId = 9, MinAge = 8.0, MaxAge = 13.99, Name = "Grades 4-6 - Outpost Room" },
            new ClassGroupLocation { GroupId = 31, LocationId = 10, MinAge = 12.0, MaxAge = 15.0, Name = "Grades 7-8 - Warehouse" },
            new ClassGroupLocation { GroupId = 32, LocationId = 11, MinAge = 13.0, MaxAge = 19.0, Name = "Grades 9-12 - Garage" },
        };

        /// <summary>
        /// Holds a cached copy of the "start time" DateTime for any scheduleIds this block encounters.
        /// </summary>
        private Dictionary<int, DateTime> _scheduleTimes = new Dictionary<int, DateTime>();

        /// <summary>
        /// Holds a cached copy of the Id for each person Guid
        /// </summary>
        private Dictionary<Guid, int> _peopleDictionary = new Dictionary<Guid, int>();

        /// <summary>
        /// Holds a cached copy of the attribute Id for each person Guid
        /// </summary>
        private Dictionary<Guid, int> _peopleAliasDictionary = new Dictionary<Guid, int>();

        /// <summary>
        /// Holds a cache of the person object
        /// </summary>
        private Dictionary<Guid, Person> _personCache = new Dictionary<Guid, Person>();

        /// <summary>
        /// Holds a cached copy of the location Id for each family Guid
        /// </summary>
        private Dictionary<Guid, int> _familyLocationDictionary = new Dictionary<Guid, int>();

        /// <summary>
        /// A dictionary of a Person's login usernames.
        /// </summary>
        private Dictionary<Person, List<string>> _peopleLoginsDictionary = new Dictionary<Person, List<string>>();

        /// <summary>
        /// Holds the dictionary of person guids and a dictionary of their attribute names
        /// and values from the family section of the XML.
        /// </summary>
        private Dictionary<Guid, bool> _personWithAttributes = new Dictionary<Guid, bool>();

        /// <summary>
        /// Magic kiosk Id used for attendance data.
        /// </summary>
        private static int _kioskDeviceId = 2;

        #endregion

        #region Properties

        //// used for public / protected properties

        #endregion

        #region Base Control Methods

        ////  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
            Server.ScriptTimeout = 300;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 300;
            if ( ! IsPostBack )
            {
                VerifyXMLDocumentExists();
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
                string xmlFileUrl = GetAttributeValue( "XMLDocumentURL" );
                if ( DownloadFile( xmlFileUrl, saveFile ) )
                {
                    ProcessXml( saveFile );
                    nbMessage.Visible = true;
                    nbMessage.Title = "Success";
                    nbMessage.NotificationBoxType = NotificationBoxType.Success;
                    nbMessage.Text = string.Format(
@"<p>Happy tire-kicking! The data is in your database. Hint: try <a href='{0}'>searching for the Decker family</a>.</p>
<p>Here are some of the things you'll find in the sample data:</p>{1}",
                        ResolveRockUrl( "~/Person/Search/name/?SearchTerm=Decker" ),
                        GetStories( saveFile ) );
                    pnlInputForm.Visible = false;
                    RecordSuccess();
                }
            }
            catch ( Exception ex )
            {
                nbMessage.Visible = true;
                nbMessage.Title = "Oops!";
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Text = string.Format(
                    "That wasn't supposed to happen.  The error was:<br/>{0}<br/>{1}<br/>{2}",
                    ex.Message.ConvertCrLfToHtmlBr(),
                    FlattenInnerExceptions( ex.InnerException ),
                    ex.StackTrace.ConvertCrLfToHtmlBr() );
            }

            if ( File.Exists( saveFile ) )
            {
                File.Delete( saveFile );
            }
        }

        /// <summary>
        /// Records the current date into the SampleData system setting
        /// so that other blocks (such as the RockUpdate) can know that
        /// sample data has been loaded.
        /// </summary>
        private void RecordSuccess()
        {
            string xmlFileUrl = GetAttributeValue( "XMLDocumentURL" );
            if ( xmlFileUrl.StartsWith( "http://storage.rockrms.com/sampledata/" ) )
            {
                Rock.Web.SystemSettings.SetValue( SystemSettingKeys.SAMPLEDATA_DATE, RockDateTime.Now.ToString() );
            }
        }

        /// <summary>
        /// Extract the stories out of the XML comments and put them on the results page.
        /// </summary>
        /// <param name="saveFile"></param>
        /// <returns></returns>
        protected string GetStories( string saveFile )
        {
            var xdoc = XDocument.Load( saveFile );
            StringBuilder sb = new StringBuilder();
            sb.Append( "<ul>" );
            foreach ( var comment in xdoc.Element( "data" ).DescendantNodes().OfType<XComment>() )
            {
                sb.AppendFormat( "<li>{0}</li>", comment.ToString().Replace( "<!--", string.Empty ).Replace( "-->", string.Empty ) );
            }

            sb.Append( "</ul>" );
            return sb.ToString();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the SampleData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            VerifyXMLDocumentExists();
            nbMessage.Text = string.Empty;
            nbMessage.Visible = false;
            pnlInputForm.Visible = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Verify that the configured XML document exists.
        /// </summary>
        private void VerifyXMLDocumentExists()
        {
            bool fileExists = false;

            try
            {
                Uri fileUri = new Uri( GetAttributeValue( "XMLDocumentURL" ) );
                if ( fileUri.IsFile )
                {
                    fileExists = File.Exists( fileUri.LocalPath );
                }
                else
                {
                    var request = (HttpWebRequest)WebRequest.Create( GetAttributeValue( "XMLDocumentURL" ) );
                    request.Method = "HEAD";
                    var response = (HttpWebResponse)request.GetResponse();
                    fileExists = response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch ( Exception ex )
            {
                nbError.Text += string.Format( "<br/>{0} Error trying to check the sample data file. {1}", RockDateTime.Now.ToShortTimeString(), ex.Message );
            }

            if ( ! fileExists )
            {
                nbError.Visible = true;
                bbtnLoadData.Enabled = false;
            }
            else
            {
                nbError.Visible = false;
                bbtnLoadData.Enabled = true;
            }
        }

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
                Uri fileUri = new Uri( fileUrl );
                if ( fileUri.IsFile )
                {
                    File.Copy( fileUrl, fileOutput );
                }
                else
                {
                    using ( WebClient client = new WebClient() )
                    {
                        client.DownloadFile( fileUri, fileOutput );
                    }
                }

                isSuccess = true;
            }
            catch ( Exception ex )
            {
                nbMessage.Text = string.Format( "While trying to fetch {0}, {1} ", fileUrl, ex.Message );
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

            RockContext rockContext = new RockContext();
            rockContext.Configuration.AutoDetectChangesEnabled = false;

            var elemFamilies = xdoc.Element( "data" ).Element( "families" );
            var elemGroups = xdoc.Element( "data" ).Element( "groups" );
            var elemRelationships = xdoc.Element( "data" ).Element( "relationships" );
            var elemSecurityGroups = xdoc.Element( "data" ).Element( "securityRoles" );
            TimeSpan ts;

            //// First delete any sample data that might exist already 
            // using RockContext in case there are multiple saves (like Attributes)
            rockContext.WrapTransaction( () =>
            {
                // First we'll clean up by deleting any previously created data such as
                // families, addresses, people, photos, attendance data, etc.
                _stopwatch.Start();
                DeleteExistingGroups( elemGroups, rockContext );
                DeleteExistingFamilyData( elemFamilies, rockContext );
                //rockContext.ChangeTracker.DetectChanges();
                //rockContext.SaveChanges( disablePrePostProcessing: true );
                ts = _stopwatch.Elapsed;
                _sb.AppendFormat( "{0:00}:{1:00}.{2:00} data deleted <br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );
            } );

            // Import the sample data
            // using RockContext in case there are multiple saves (like Attributes)
            rockContext.WrapTransaction( () =>
            {
                // Now we can add the families (and people) and then groups.
                AddFamilies( elemFamilies, rockContext );
                ts = _stopwatch.Elapsed;
                _sb.AppendFormat( "{0:00}:{1:00}.{2:00} families added<br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );

                AddRelationships( elemRelationships, rockContext );
                ts = _stopwatch.Elapsed;
                _sb.AppendFormat( "{0:00}:{1:00}.{2:00} relationships added<br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );

                AddGroups( elemGroups, rockContext );
                ts = _stopwatch.Elapsed;
                _sb.AppendFormat( "{0:00}:{1:00}.{2:00} groups added<br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );

                AddToSecurityGroups( elemSecurityGroups, rockContext );
                ts = _stopwatch.Elapsed;
                _sb.AppendFormat( "{0:00}:{1:00}.{2:00} people added to security roles<br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );

                rockContext.ChangeTracker.DetectChanges();
                rockContext.SaveChanges( disablePrePostProcessing: true );
                ts = _stopwatch.Elapsed;
                _sb.AppendFormat( "{0:00}:{1:00}.{2:00} changes saved<br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );

                // add logins, but only if we were supplied a password
                if ( !string.IsNullOrEmpty( tbPassword.Text.Trim() ) )
                {
                    AddPersonLogins( rockContext );
                    ts = _stopwatch.Elapsed;
                    _sb.AppendFormat( "{0:00}:{1:00}.{2:00} person logins added<br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );
                }

                // Add Person Notes
                AddPersonNotes( elemFamilies, rockContext );
                rockContext.SaveChanges( disablePrePostProcessing: true );
                ts = _stopwatch.Elapsed;
                _sb.AppendFormat( "{0:00}:{1:00}.{2:00} notes added<br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );

                // Add Person Metaphone/Sounds-like stuff
                AddMetaphone();

            } );

            if ( GetAttributeValue( "EnableStopwatch" ).AsBoolean() )
            {
                lTime.Text = _sb.ToString();
            }
        }

        /// <summary>
        /// Adds a transaction to add the metaphone stuff for each person we've added.
        /// </summary>
        private void AddMetaphone()
        {
            foreach ( Person person in _personCache.Values )
            {
                //var person = pair.Value as Person;
                var transaction = new Rock.Transactions.SaveMetaphoneTransaction( person );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }
        }

        /// <summary>
        /// Adds any notes for any people given in the XML file.
        /// </summary>
        /// <param name="elemFamilies"></param>
        /// <param name="rockContext"></param>
        private void AddPersonNotes( XElement elemFamilies, RockContext rockContext )
        {
            var peopleWithNotes = from n in elemFamilies.Elements( "family" ).Elements( "members" ).Elements( "person" ).Elements( "notes" ).Elements( "note" )
                                  select new
                                  {
                                      PersonGuid = n.Parent.Parent.Attribute( "guid" ).Value,
                                      Type = n.Attribute( "type" ).Value,
                                      Text = n.Attribute( "text" ).Value,
                                      IsPrivate = n.Attribute( "isPrivate" ) != null ? n.Attribute( "isPrivate" ).Value : "false",
                                      ByPersonGuid = n.Attribute( "byGuid" ) != null ? n.Attribute( "byGuid" ).Value : null,
                                      Date = n.Attribute( "date" ) != null ? n.Attribute( "date" ).Value : null
                                  };

	        foreach ( var r in peopleWithNotes )
	        {
                int personId = _peopleDictionary[ r.PersonGuid.AsGuid() ];
                AddNote( personId, r.Type, r.Text, r.Date, r.ByPersonGuid, r.IsPrivate, rockContext );
	        }
        }

        /// <summary>
        /// Adds a KnownRelationship record between the two supplied Guids with the given 'is' relationship type:
        ///     
        ///     Role / inverse Role
        ///     ================================
        ///     step-parent     / step-child
        ///     grandparent     / grandchild
        ///     previous-spouse / previous-spouse
        ///     can-check-in    / allow-check-in-by
        ///     parent          / child
        ///     sibling         / sibling
        ///     invited         / invited-by
        ///     related         / related
        ///     
        /// ...for xml such as:
        /// <relationships>
        ///     <relationship a="Ben" personGuid="3C402382-3BD2-4337-A996-9E62F1BAB09D"
        ///     has="step-parent" forGuid="3D7F6605-3666-4AB5-9F4E-D7FEBF93278E" name="Brian" />
        ///  </relationships>
        ///  
        /// </summary>
        /// <param name="elemRelationships"></param>
        private void AddRelationships( XElement elemRelationships, RockContext rockContext )
        {
            if ( elemRelationships == null )
            {
                return;
            }

            Guid ownerRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();
            Guid knownRelationshipsGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid();
            var memberService = new GroupMemberService( rockContext );

            var groupTypeRoles = new GroupTypeRoleService( rockContext ).Queryable( "GroupType" )
                .Where( r => r.GroupType.Guid == knownRelationshipsGroupTypeGuid ).ToList();

            //// We have to create (or fetch existing) two groups for each relationship, adding the
            //// other person as a member of that group with the appropriate GroupTypeRole (GTR):
            ////   * a group with person as owner (GTR) and forPerson as type/role (GTR) 
            ////   * a group with forPerson as owner (GTR) and person as inverse-type/role (GTR)

            foreach ( var elemRelationship in elemRelationships.Elements( "relationship" ) )
            {
                // skip any illegally formatted items
                if ( elemRelationship.Attribute( "personGuid" ) == null || elemRelationship.Attribute( "forGuid" ) == null ||
                    elemRelationship.Attribute( "has" ) == null )
                {
                    continue;
                }

                Guid personGuid = elemRelationship.Attribute( "personGuid" ).Value.Trim().AsGuid();
                Guid forGuid = elemRelationship.Attribute( "forGuid" ).Value.Trim().AsGuid();
                int ownerPersonId = _peopleDictionary[personGuid];
                int forPersonId = _peopleDictionary[forGuid];

                string relationshipType = elemRelationship.Attribute( "has" ).Value.Trim();

                int roleId = -1;

                switch ( relationshipType )
                {
                    case "step-parent":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_STEP_PARENT.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "step-child":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_STEP_CHILD.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "can-check-in":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "allow-check-in-by":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_ALLOW_CHECK_IN_BY.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "grandparent":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_GRANDPARENT.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "grandchild":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_GRANDCHILD.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "invited":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_INVITED.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "invited-by":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_INVITED_BY.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "previous-spouse":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_PREVIOUS_SPOUSE.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "sibling":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_SIBLING.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "parent":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_PARENT.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "child":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CHILD.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    case "related":
                        roleId = groupTypeRoles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_IMPLIED_RELATIONSHIPS_RELATED.AsGuid() )
                            .Select( r => r.Id ).FirstOrDefault();
                        break;

                    default:
                        //// throw new NotSupportedException( string.Format( "unknown relationship type {0}", elemRelationship.Attribute( "has" ).Value ) );
                        // just skip unknown relationship types
                        continue;
                }

                // find the person's KnownRelationship "owner" group
                var knownRelationshipGroup = memberService.Queryable()
                    .Where( m =>
                        m.PersonId == ownerPersonId &&
                        m.GroupRole.Guid == ownerRoleGuid )
                    .Select( m => m.Group )
                    .FirstOrDefault();

                // create it if it does not yet exist
                if ( knownRelationshipGroup == null )
                {
                    var ownerRole = new GroupTypeRoleService( rockContext ).Get( ownerRoleGuid );
                    if ( ownerRole != null && ownerRole.GroupTypeId.HasValue )
                    {
                        var ownerGroupMember = new GroupMember();
                        ownerGroupMember.PersonId = ownerPersonId;
                        ownerGroupMember.GroupRoleId = ownerRole.Id;

                        knownRelationshipGroup = new Group();
                        knownRelationshipGroup.Name = ownerRole.GroupType.Name;
                        knownRelationshipGroup.GroupTypeId = ownerRole.GroupTypeId.Value;
                        knownRelationshipGroup.Members.Add( ownerGroupMember );

                        var groupService = new GroupService( rockContext );
                        groupService.Add( knownRelationshipGroup );
                        //rockContext.ChangeTracker.DetectChanges();
                        rockContext.SaveChanges( disablePrePostProcessing: true );

                        knownRelationshipGroup = groupService.Get( knownRelationshipGroup.Id );
                    }
                }

                // Now find (and add if not found) the forPerson as a member with the "has" role-type
                var groupMember = memberService.Queryable()
                    .Where( m =>
                        m.GroupId == knownRelationshipGroup.Id &&
                        m.PersonId == forPersonId &&
                        m.GroupRoleId == roleId )
                    .FirstOrDefault();

                if ( groupMember == null )
                {
                    groupMember = new GroupMember()
                    {
                        GroupId = knownRelationshipGroup.Id,
                        PersonId = forPersonId,
                        GroupRoleId = roleId,
                    };

                    rockContext.GroupMembers.Add( groupMember );
                }

                // Now create thee inverse relationship.
                //
                // (NOTE: Don't panic if your VS tooling complains that there is
                // an unused variable here.  There is no need to do anything with the
                // inverseGroupMember relationship because it was already added to the
                // context.  All we have to do below is save the changes to the context
                // when we're ready.)
                var inverseGroupMember = memberService.GetInverseRelationship( groupMember, createGroup: true, personAlias: CurrentPersonAlias );
            }
        }

        /// <summary>
        /// Handles adding families from the given XML element snippet
        /// </summary>
        /// <param name="elemFamilies">The xml element containing all the families.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddFamilies( XElement elemFamilies, RockContext rockContext )
        {
            if ( elemFamilies == null )
            {
                return;
            }

            // Persist the storage type's settings specific to the photo binary file type
            var settings = new Dictionary<string, string>();
            if ( _binaryFileType.Attributes == null )
            {
                _binaryFileType.LoadAttributes();
            }
            foreach ( var attributeValue in _binaryFileType.AttributeValues )
            {
                settings.Add( attributeValue.Key, attributeValue.Value.Value );
            }
            _binaryFileTypeSettings = settings.ToJson();

            bool fabricateAttendance = GetAttributeValue( "FabricateAttendance" ).AsBoolean();
            GroupService groupService = new GroupService( rockContext );
            var allFamilies = rockContext.Groups;

            List<Group> allGroups = new List<Group>();
            var attendanceData = new Dictionary<Guid, List<Attendance>>();

            // Next create the family along with its members.
            foreach ( var elemFamily in elemFamilies.Elements( "family" ) )
            {
                Guid guid = elemFamily.Attribute( "guid" ).Value.Trim().AsGuid();
                var familyMembers = BuildFamilyMembersFromXml( elemFamily.Element( "members" ), rockContext );

                // Call replica of groupService's SaveNewFamily method in an attempt to speed things up
                Group family = CreateNewFamily( familyMembers, campusId: 1 );
                family.Guid = guid;

                // add the family to the context's list of groups
                allFamilies.Add( family );

                // add the families address(es)
                AddFamilyAddresses( groupService, family, elemFamily.Element( "addresses" ), rockContext );

                // add their attendance data
                if ( fabricateAttendance )
                {
                    AddFamilyAttendance( family, elemFamily, rockContext, attendanceData );
                }

                allGroups.Add( family );

                _stopwatch.Stop();
                _sb.AppendFormat( "{0:00}:{1:00}.{2:00} added {3}<br/>", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds / 10, family.Name );
                _stopwatch.Start();
            }
            rockContext.ChangeTracker.DetectChanges();
            rockContext.SaveChanges( disablePrePostProcessing: true );

            // Now save each person's attributevalues (who had them defined in the XML)
            // and add each person's ID to a dictionary for use later.
            AttributeValueService attributeValueService = new AttributeValueService( rockContext );
            foreach ( var gm in allGroups.SelectMany( g => g.Members ) )
            {
                // Put the person's id into the people dictionary for later use.
                if ( !_peopleDictionary.ContainsKey( gm.Person.Guid ) )
                {
                    _peopleDictionary.Add( gm.Person.Guid, gm.Person.Id );
                }

                // Only save if the person had attributes, otherwise it will error.
                if ( _personWithAttributes.ContainsKey( gm.Person.Guid ) )
                {
                    foreach ( var attributeCache in gm.Person.Attributes.Select( a => a.Value ) )
                    {
                        var newValue = gm.Person.AttributeValues[attributeCache.Key];
                        if ( newValue != null )
                        {
                            newValue.EntityId = gm.Person.Id;
                            rockContext.AttributeValues.Add( newValue );
                        }
                    }
                }
            }

            _stopwatch.Stop();
            _sb.AppendFormat( "{0:00}:{1:00}.{2:00} saved attributes for everyone <br/>", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds / 10 );
            _stopwatch.Start();

            // Create person alias records for each person
            PersonService personService = new PersonService( rockContext );
            foreach ( var person in personService.Queryable( "Aliases" )
                .Where( p =>
                    _peopleDictionary.Keys.Contains( p.Guid ) &&
                    !p.Aliases.Any() ) )
            {
                person.Aliases.Add( new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid } );
            }
            rockContext.ChangeTracker.DetectChanges();
            rockContext.SaveChanges( disablePrePostProcessing: true );

            // Put the person alias ids into the people alias dictionary for later use.
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            foreach ( var personAlias in personAliasService.Queryable( "Person" )
                .Where( a =>
                    _peopleDictionary.Keys.Contains( a.Person.Guid ) &&
                    a.PersonId == a.AliasPersonId ) )
            { 
                _peopleAliasDictionary.Add( personAlias.Person.Guid, personAlias.Id );
            }

            _stopwatch.Stop();
            _sb.AppendFormat( "{0:00}:{1:00}.{2:00} added person aliases<br/>", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds / 10 );
            _stopwatch.Start();

            // Now that person aliases have been saved, save the attendance records
            var attendanceService = new AttendanceService( rockContext );
            var attendanceGuids = attendanceData.Select( a => a.Key ).ToList();
            foreach ( var aliasKeyValue in _peopleAliasDictionary
                .Where( a => attendanceGuids.Contains( a.Key )) )
            {
                foreach ( var attendance in attendanceData[aliasKeyValue.Key] )
                {
                    attendance.PersonAliasId = aliasKeyValue.Value;
                    attendanceService.Add( attendance );
                }
            }
            rockContext.ChangeTracker.DetectChanges();
            rockContext.SaveChanges( disablePrePostProcessing: true );

            _stopwatch.Stop();
            _sb.AppendFormat( "{0:00}:{1:00}.{2:00} added attendance records<br/>", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds / 10 );
            _stopwatch.Start();
        }

        /// <summary>
        /// Handles adding groups from the given XML element snippet.
        /// </summary>
        /// <param name="elemGroups">The elem groups.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        private void AddGroups( XElement elemGroups, RockContext rockContext )
        {
            // Add groups
            if ( elemGroups == null )
            {
                return;
            }

            GroupService groupService = new GroupService( rockContext );
            DefinedTypeCache smallGroupTopicType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SMALL_GROUP_TOPIC.AsGuid() );

            // Next create the group along with its members.
            foreach ( var elemGroup in elemGroups.Elements( "group" ) )
            {
                Guid guid = elemGroup.Attribute( "guid" ).Value.Trim().AsGuid();
                string type = elemGroup.Attribute( "type" ).Value;
                Group group = new Group()
                {
                    Guid = guid,
                    Name = elemGroup.Attribute( "name" ).Value.Trim()
                };

                // skip any where there is no group type given -- they are invalid entries.
                if ( string.IsNullOrEmpty( elemGroup.Attribute( "type" ).Value.Trim() ) )
                {
                    return;
                }

                int? roleId;
                GroupTypeCache groupType;
                switch ( elemGroup.Attribute( "type" ).Value.Trim() )
                {
                    case "serving":
                        groupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SERVING_TEAM.AsGuid() );
                        group.GroupTypeId = groupType.Id;
                        roleId = groupType.DefaultGroupRoleId;
                        break;
                    case "smallgroup":
                        groupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP.AsGuid() );
                        group.GroupTypeId = groupType.Id;
                        roleId = groupType.DefaultGroupRoleId;
                        break;
                    default:
                        throw new NotSupportedException( string.Format( "unknown group type {0}", elemGroup.Attribute( "type" ).Value.Trim() ) );
                }

                if ( elemGroup.Attribute( "description" ) != null )
                {
                    group.Description = elemGroup.Attribute( "description" ).Value;
                }

                if ( elemGroup.Attribute( "parentGroupGuid" ) != null )
                {
                    var parentGroup = groupService.Get( elemGroup.Attribute( "parentGroupGuid" ).Value.AsGuid() );
                    if ( parentGroup != null )
                    {
                        group.ParentGroupId = parentGroup.Id;
                    }
                }

                // Set the group's meeting location
                if ( elemGroup.Attribute( "meetsAtHomeOfFamily" ) != null )
                {
                    int meetingLocationValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_MEETING_LOCATION.AsGuid() ).Id;
                    var groupLocation = new GroupLocation()
                    {
                        IsMappedLocation = false,
                        IsMailingLocation = false,
                        GroupLocationTypeValueId = meetingLocationValueId,
                        LocationId = _familyLocationDictionary[elemGroup.Attribute( "meetsAtHomeOfFamily" ).Value.AsGuid()],
                    };

                    // Set the group location's GroupMemberPersonId if given (required?)
                    if ( elemGroup.Attribute( "meetsAtHomeOfPerson" ) != null )
                    {
                        groupLocation.GroupMemberPersonAliasId = _peopleAliasDictionary[elemGroup.Attribute( "meetsAtHomeOfPerson" ).Value.AsGuid()];
                    }

                    group.GroupLocations.Add( groupLocation );
                }

                group.LoadAttributes( rockContext );

                // Set the study topic
                if ( elemGroup.Attribute( "studyTopic" ) != null )
                {
                    var topic = elemGroup.Attribute( "studyTopic" ).Value;
                    DefinedValueCache smallGroupTopicDefinedValue = smallGroupTopicType.DefinedValues.FirstOrDefault( a => a.Value == topic );

                    // add it as new if we didn't find it.
                    if ( smallGroupTopicDefinedValue == null )
                    {
                        smallGroupTopicDefinedValue = AddDefinedTypeValue( topic, smallGroupTopicType, rockContext );
                    }

                    group.SetAttributeValue( "Topic", smallGroupTopicDefinedValue.Guid.ToString() );
                }

                // Set the schedule and meeting time
                if ( elemGroup.Attribute( "groupSchedule" ) != null )
                {
                    string[] schedule = elemGroup.Attribute( "groupSchedule" ).Value.SplitDelimitedValues( whitespace: false );

                    if ( schedule[0] == "weekly" )
                    {
                        var dow = schedule[1];
                        var time = schedule[2];
                        AddWeeklySchedule( group, dow, time );
                    }
                }

                // Add each person as a member
                foreach ( var elemPerson in elemGroup.Elements( "person" ) )
                {
                    Guid personGuid = elemPerson.Attribute( "guid" ).Value.Trim().AsGuid();

                    GroupMember groupMember = new GroupMember();
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;

                    if ( elemPerson.Attribute( "isLeader" ) != null )
                    {
                        bool isLeader = elemPerson.Attribute( "isLeader" ).Value.Trim().AsBoolean();
                        if ( isLeader )
                        {
                            var gtLeaderRole = groupType.Roles.Where( r => r.IsLeader ).FirstOrDefault();
                            if ( gtLeaderRole != null )
                            {
                                groupMember.GroupRoleId = gtLeaderRole.Id;
                            }
                        }
                    }
                    else
                    {
                        groupMember.GroupRoleId = roleId ?? -1;
                    }

                    groupMember.PersonId = _peopleDictionary[personGuid];
                    group.Members.Add( groupMember );
                }

                groupService.Add( group );
                // Now we have to save changes in order for the attributes to be saved correctly.
                rockContext.SaveChanges();
                group.SaveAttributeValues( rockContext );

                // Now add any group location schedules
                LocationService locationService = new LocationService( rockContext );
                ScheduleService scheduleService = new ScheduleService( rockContext );
                Guid locationTypeMeetingLocationGuid = new Guid( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_MEETING_LOCATION );
                var locationTypeMeetingLocationId = Rock.Web.Cache.DefinedValueCache.Read( locationTypeMeetingLocationGuid ).Id;

                foreach ( var elemLocation in elemGroup.Elements( "location" ) )
                {
                    Guid locationGuid = elemLocation.Attribute( "guid" ).Value.Trim().AsGuid();
                    Location location = locationService.Get( locationGuid );
                    GroupLocation groupLocation = new GroupLocation();
                    groupLocation.Location = location;
                    groupLocation.GroupLocationTypeValueId = locationTypeMeetingLocationId;
                    group.GroupLocations.Add( groupLocation );

                    foreach ( var elemSchedule in elemLocation.Elements( "schedule" ) )
                    {
                        try
                        {
                            Guid scheduleGuid = elemSchedule.Attribute( "guid" ).Value.Trim().AsGuid();
                            Schedule schedule = scheduleService.Get( scheduleGuid );
                            groupLocation.Schedules.Add( schedule );
                        }
                        catch
                        { }
                    }
                    TimeSpan ts = _stopwatch.Elapsed;
                    _sb.AppendFormat( "{0:00}:{1:00}.{2:00} group location schedules added<br/>", ts.Minutes, ts.Seconds, ts.Milliseconds / 10 );
                }
             }
        }

        /// <summary>
        /// Adds a Weekly schedule to the given group.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="dow"></param>
        /// <param name="time"></param>
        private void AddWeeklySchedule( Group group, string dayOfWeekName, string time )
        {
            group.Schedule = new Schedule();

            DayOfWeek dow = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), dayOfWeekName, true);

            group.Schedule.iCalendarContent = null;
            group.Schedule.WeeklyDayOfWeek = dow;

            TimeSpan timespan;
            if ( TimeSpan.TryParse( time, out timespan ))
            {
                group.Schedule.WeeklyTimeOfDay = timespan;
            }            
        }

        /// <summary>
        /// Adds a new defined value to a given DefinedType.
        /// </summary>
        /// <param name="topic">the string value of the new defined value</param>
        /// <param name="definedTypeCache">a defined type cache to which the defined value will be added.</param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private DefinedValueCache AddDefinedTypeValue( string topic, DefinedTypeCache definedTypeCache, RockContext rockContext )
        {
            DefinedValueService definedValueService = new DefinedValueService( rockContext );
            
            DefinedValue definedValue = new DefinedValue {
                Id = 0,
                IsSystem = false,
                Value = topic,
                Description = "",
                CreatedDateTime = RockDateTime.Now,
                DefinedTypeId = definedTypeCache.Id
            };
            definedValueService.Add( definedValue );
            rockContext.SaveChanges();

            Rock.Web.Cache.DefinedValueCache.Flush( definedValue.Id );
            Rock.Web.Cache.DefinedTypeCache.Flush( definedTypeCache.Id );

            return DefinedValueCache.Read( definedValue.Id, rockContext );
        }

        /// <summary>
        /// Handles adding people to the security groups from the given XML element snippet.
        /// </summary>
        /// <param name="elemSecurityGroups">The elem security groups.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddToSecurityGroups( XElement elemSecurityGroups, RockContext rockContext )
        {
            if ( elemSecurityGroups == null )
            {
                return;
            }

            GroupService groupService = new GroupService( rockContext );

            // Next find each group and add its members
            foreach ( var elemGroup in elemSecurityGroups.Elements( "group" ) )
            {
                int membersAdded = 0;
                Guid guid = elemGroup.Attribute( "guid" ).Value.Trim().AsGuid();
                Group securityGroup = groupService.GetByGuid( guid );
                if ( securityGroup == null )
                {
                    continue;
                }

                // Add each person as a member of the group
                foreach ( var elemPerson in elemGroup.Elements( "members" ).Elements( "person" ) )
                {
                    Guid personGuid = elemPerson.Attribute( "guid" ).Value.Trim().AsGuid();
                    int personId = _peopleDictionary[personGuid];

                    // Don't add if already in the group...
                    if ( securityGroup.Members.Where( p => p.PersonId == personId ).Count() > 0 )
                    {
                        continue;
                    }

                    membersAdded++;
                    GroupMember groupMember = new GroupMember();
                    groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                    groupMember.GroupRoleId = securityGroup.GroupType.DefaultGroupRoleId.Value;
                    groupMember.PersonId = personId;
                    securityGroup.Members.Add( groupMember );
                }
            }
        }

        /// <summary>
        /// Deletes the family's addresses, phone numbers, photos, viewed records, and people.
        /// TODO: delete attendance codes for attendance data that's about to be deleted when
        /// we delete the person record.
        /// </summary>
        /// <param name="families">The families.</param>
        /// <param name="rockContext">The rock context.</param>
        private void DeleteExistingFamilyData( XElement families, RockContext rockContext )
        {
            PersonService personService = new PersonService( rockContext );
            PhoneNumberService phoneNumberService = new PhoneNumberService( rockContext );
            PersonViewedService personViewedService = new PersonViewedService( rockContext );
            PageViewService pageViewService = new PageViewService( rockContext );
            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            PersonDuplicateService personDuplicateService = new PersonDuplicateService( rockContext );
            NoteService noteService = new NoteService( rockContext );
            AuthService authService = new AuthService( rockContext );
            CommunicationService communicationService = new CommunicationService( rockContext );
            CommunicationRecipientService communicationRecipientService = new CommunicationRecipientService( rockContext );

            foreach ( var elemFamily in families.Elements( "family" ) )
            {
                Guid guid = elemFamily.Attribute( "guid" ).Value.Trim().AsGuid();

                GroupService groupService = new GroupService( rockContext );
                Group family = groupService.Get( guid );
                if ( family != null )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    var members = groupMemberService.GetByGroupId( family.Id );

                    // delete the people records
                    string errorMessage;
                    List<int> photoIds = members.Select( m => m.Person ).Where( p => p.PhotoId != null ).Select( a => (int)a.PhotoId ).ToList();

                    foreach ( var person in members.Select( m => m.Person ) )
                    {
                        person.GivingGroup = null;
                        person.GivingGroupId = null;
                        person.PhotoId = null;

                        // delete phone numbers
                        foreach ( var phone in phoneNumberService.GetByPersonId( person.Id ) )
                        {
                            if ( phone != null )
                            {
                                phoneNumberService.Delete( phone );
                            }
                        }

                        // delete communication recipient
                        foreach ( var recipient in communicationRecipientService.Queryable().Where( r => r.PersonAlias.PersonId == person.Id ) )
                        {
                            communicationRecipientService.Delete( recipient );
                        }

                        // delete communication
                        foreach ( var communication in communicationService.Queryable().Where( c => c.SenderPersonAliasId == person.PrimaryAlias.Id ) )
                        {
                            communicationService.Delete( communication );
                        }

                        // delete person viewed records
                        foreach ( var view in personViewedService.GetByTargetPersonId( person.Id ) )
                        {
                            personViewedService.Delete( view );
                        }

                        // delete page viewed records
                        foreach ( var view in pageViewService.GetByPersonId( person.Id ) )
                        {
                            pageViewService.Delete( view );
                        }

                        // delete notes created by them or on their record.
                        foreach ( var note in noteService.Queryable().Where ( n => n.CreatedByPersonAlias.PersonId == person.Id
                            || (n.NoteType.EntityTypeId == _personEntityTypeId && n.EntityId == person.Id ) ) )
                        {
                            noteService.Delete( note );
                        }

                        //// delete any GroupMember records they have
                        //foreach ( var groupMember in groupMemberService.Queryable().Where( gm => gm.PersonId == person.Id ) )
                        //{
                        //    groupMemberService.Delete( groupMember );
                        //}

                        //// delete any Authorization data
                        //foreach ( var auth in authService.Queryable().Where( a => a.PersonId == person.Id ) )
                        //{
                        //    authService.Delete( auth );
                        //}

                        // delete their aliases
                        foreach ( var alias in personAliasService.Queryable().Where( a => a.PersonId == person.Id ) )
                        {
                            foreach ( var duplicate in personDuplicateService.Queryable().Where( d => d.DuplicatePersonAliasId == alias.Id ) )
                            {
                                personDuplicateService.Delete( duplicate );
                            }

                            personAliasService.Delete( alias );
                        }

                        //foreach ( var relationship in person.Gro)

                        // Save these changes so the CanDelete passes the check...
                        //rockContext.ChangeTracker.DetectChanges();
                        rockContext.SaveChanges( disablePrePostProcessing: true );

                        if ( personService.CanDelete( person, out errorMessage ) )
                        {
                            personService.Delete( person );
                            //rockContext.ChangeTracker.DetectChanges();
                            //rockContext.SaveChanges( disablePrePostProcessing: true );
                        }
                        else
                        {
                            throw new Exception( string.Format( "Trying to delete {0}, but: {1}", person.FullName, errorMessage ) );
                        }
                    }
                    //rockContext.ChangeTracker.DetectChanges();
                    rockContext.SaveChanges( disablePrePostProcessing: true );

                    // delete all member photos
                    foreach ( var photo in binaryFileService.GetByIds( photoIds ) )
                    {
                        binaryFileService.Delete( photo );
                    }

                    DeleteGroupAndMemberData( family, rockContext );
                }
            }
        }

        /// <summary>
        /// Generic method to delete the members of a group and then the group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <exception cref="System.InvalidOperationException">Unable to delete group:  + group.Name</exception>
        private void DeleteGroupAndMemberData( Group group, RockContext rockContext )
        {
            GroupService groupService = new GroupService( rockContext );

            // delete addresses
            GroupLocationService groupLocationService = new GroupLocationService( rockContext );
            if ( group.GroupLocations.Count > 0 )
            {
                foreach ( var groupLocations in group.GroupLocations.ToList() )
                {
                    group.GroupLocations.Remove( groupLocations );
                    groupLocationService.Delete( groupLocations );
                }
            }

            // delete members
            var groupMemberService = new GroupMemberService( rockContext );
            var members = group.Members;
            foreach ( var member in members.ToList() )
            {
                group.Members.Remove( member );
                groupMemberService.Delete( member );
            }

            // delete attribute values
            group.LoadAttributes( rockContext );
            if ( group.AttributeValues != null )
            {
                var attributeValueService = new AttributeValueService( rockContext );
                foreach ( var entry in group.AttributeValues )
                {
                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( entry.Value.AttributeId, group.Id );
                    if ( attributeValue != null )
                    {
                        attributeValueService.Delete( attributeValue );
                    }
                }
            }

            // now delete the group
            if ( groupService.Delete( group ) )
            {
                // ok
            }
            else
            {
                throw new InvalidOperationException( "Unable to delete group: " + group.Name );
            }
        }

        /// <summary>
        /// Delete all groups found in the given XML.
        /// </summary>
        /// <param name="elemGroups">The elem groups.</param>
        /// <param name="rockContext">The rock context.</param>
        private void DeleteExistingGroups( XElement elemGroups, RockContext rockContext )
        {
            if ( elemGroups == null )
            {
                return;
            }

            GroupService groupService = new GroupService( rockContext );
            foreach ( var elemGroup in elemGroups.Elements( "group" ) )
            {
                Guid guid = elemGroup.Attribute( "guid" ).Value.Trim().AsGuid();
                Group group = groupService.Get( guid );
                if ( group != null )
                {
                    DeleteGroupAndMemberData( group, rockContext );
                }
            }
        }

        /// <summary>
        /// Grabs the necessary parameters from the XML and then calls the CreateAttendance() method
        /// to generate all the attendance data for the family.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="elemFamily">The elem family.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="attendanceData">The attendance data.</param>
        private void AddFamilyAttendance( Group family, XElement elemFamily, RockContext rockContext, Dictionary<Guid, List<Attendance>> attendanceData )
        {
            // return from here if there's not startingAttendance date
            if ( elemFamily.Attribute( "startingAttendance" ) == null )
            {
                return;
            }

            // get some variables we'll need to create the attendance records
            DateTime startingDate = DateTime.Parse( elemFamily.Attribute( "startingAttendance" ).Value.Trim(), new CultureInfo( "en-US" ) );
            DateTime endDate = RockDateTime.Now;

            // If the XML specifies an endingAttendance date use it, otherwise use endingAttendanceWeeksAgo
            // to calculate the end date otherwise we'll just use the current date as the end date.
            if ( elemFamily.Attribute( "endingAttendance" ) != null )
            {
                DateTime.TryParse( elemFamily.Attribute( "endingAttendance" ).Value.Trim(), out endDate );
            }
            else if ( elemFamily.Attribute( "endingAttendanceWeeksAgo" ) != null )
            {
                int endingWeeksAgo = 0;
                int.TryParse( elemFamily.Attribute( "endingAttendanceWeeksAgo" ).Value.Trim(), out endingWeeksAgo );
                endDate = RockDateTime.Now.AddDays( -7 * endingWeeksAgo );
            }

            int pctAttendance = 100;
            if ( elemFamily.Attribute( "percentAttendance" ) != null )
            {
                int.TryParse( elemFamily.Attribute( "percentAttendance" ).Value.Trim(), out pctAttendance );
            }

            int pctAttendedRegularService = 100;
            if ( elemFamily.Attribute( "percentAttendedRegularService" ) != null )
            {
                int.TryParse( elemFamily.Attribute( "percentAttendedRegularService" ).Value.Trim(), out pctAttendedRegularService );
            }

            int scheduleId = 13;
            if ( elemFamily.Attribute( "attendingScheduleId" ) != null )
            {
                int.TryParse( elemFamily.Attribute( "attendingScheduleId" ).Value.Trim(), out scheduleId );
                if ( !_scheduleTimes.ContainsKey( scheduleId ) )
                {
                    Schedule schedule = new ScheduleService( rockContext ).Get( scheduleId );
                    if ( schedule == null )
                    {
                        // We're not going to continue if they are missing this schedule
                        return;
                    }

                    _scheduleTimes.Add( scheduleId, schedule.GetCalenderEvent().DTStart.Value );
                }
            }

            int altScheduleId = 4;
            if ( elemFamily.Attribute( "attendingAltScheduleId" ) != null )
            {
                int.TryParse( elemFamily.Attribute( "attendingAltScheduleId" ).Value.Trim(), out altScheduleId );
                if ( !_scheduleTimes.ContainsKey( altScheduleId ) )
                {
                    Schedule schedule = new ScheduleService( rockContext ).Get( altScheduleId );
                    if ( schedule == null )
                    {
                        // We're not going to continue if they are missing this schedule
                        return;
                    }

                    _scheduleTimes.Add( altScheduleId, schedule.GetCalenderEvent().DTStart.Value );
                }
            }

            CreateAttendance( family.Members, startingDate, endDate, pctAttendance, pctAttendedRegularService, scheduleId, altScheduleId, attendanceData );
        }

        /// <summary>
        /// Adds attendance data for each child for each weekend since the starting date up to
        /// the weekend ending X weeks ago (endingWeeksAgo).  It will randomly skip a weekend
        /// based on the percentage (pctAttendance) and it will vary which service they attend
        /// between the scheduleId and altScheduleId based on the percentage (pctAttendedRegularService)
        /// given.
        /// </summary>
        /// <param name="familyMembers">The family members.</param>
        /// <param name="startingDate">The first date of attendance</param>
        /// <param name="endDate">The end date of attendance</param>
        /// <param name="pctAttendance">The PCT attendance.</param>
        /// <param name="pctAttendedRegularService">The PCT attended regular service.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="altScheduleId">The alt schedule identifier.</param>
        /// <param name="attendanceData">The attendance data.</param>
        private void CreateAttendance( ICollection<GroupMember> familyMembers, DateTime startingDate, DateTime endDate, 
            int pctAttendance, int pctAttendedRegularService, int scheduleId, int altScheduleId, Dictionary<Guid, List<Attendance>> attendanceData )
        {
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

                var time = _scheduleTimes[serviceSchedId];

                DateTime dtTime = new DateTime( date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second );
                DateTime checkinDateTime = dtTime.AddMinutes( Convert.ToDouble( plusMinus * minutes ) ).AddSeconds( randomSeconds );

                // foreach child in the family
                foreach ( var member in familyMembers.Where( m => m.GroupRoleId == _childRoleId ) )
                {
                    // Find a class room (group location)
                    // TODO -- someday perhaps we will change this to actually find a real GroupLocationSchedule record
                    var item = ( from classroom in _classes
                                 where member.Person.AgePrecise >= classroom.MinAge
                                 && member.Person.AgePrecise <= classroom.MaxAge
                                 orderby classroom.MinAge, classroom.MaxAge
                                 select classroom ).FirstOrDefault();

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
                        AttendanceCode = attendanceCode,
                        StartDateTime = checkinDateTime,
                        EndDateTime = null,
                        DidAttend = true,
                        CampusId = 1
                    };

                    if ( !attendanceData.Keys.Contains( member.Person.Guid ))
                    {
                        attendanceData.Add( member.Person.Guid, new List<Attendance>());
                    }
                    attendanceData[member.Person.Guid].Add( attendance);
                }
            }
        }

        /// <summary>
        /// A little method to generate a random sequence of characters of a certain length.
        /// </summary>
        /// <param name="len">length of code to generate</param>
        /// <returns>a random sequence of alpha numeric characters</returns>
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns>a list of family members.</returns>
        private List<GroupMember> BuildFamilyMembersFromXml( XElement elemMembers, RockContext rockContext )
        {
            var familyMembers = new List<GroupMember>();

            // First add each person to the familyMembers collection
            foreach ( var personElem in elemMembers.Elements( "person" ) )
            {
                var groupMember = new GroupMember();
                Guid guid = Guid.Parse( personElem.Attribute( "guid" ).Value.Trim() );

                // Attempt to find an existing person...
                Person person = null;
                if ( _personCache.ContainsKey( guid ) )
                {
                    person = _personCache[guid];
                }

                if ( person == null )
                {
                    person = new Person();
                    person.Guid = guid;
                    person.FirstName = personElem.Attribute( "firstName" ).Value.Trim();
                    if ( personElem.Attribute( "nickName" ) != null )
                    {
                        person.NickName = personElem.Attribute( "nickName" ).Value.Trim();
                    }

                    if ( personElem.Attribute( "lastName" ) != null )
                    {
                        person.LastName = personElem.Attribute( "lastName" ).Value.Trim();
                    }

                    if ( personElem.Attribute( "birthDate" ) != null )
                    {
                        person.SetBirthDate( DateTime.Parse( personElem.Attribute( "birthDate" ).Value.Trim(), new CultureInfo( "en-US" ) ) );
                    }

                    if ( personElem.Attribute( "grade" ) != null )
                    {
                        int? grade = personElem.Attribute( "grade" ).Value.AsIntegerOrNull();
                        if (grade.HasValue)
                        {
                            // convert the grade (0-12 where 12 = Senior), to a GradeOffset (12-0 where 12 = K and 0 = Senior)
                            int gradeOffset = 12 - grade.Value;
                            person.GradeOffset = gradeOffset >= 0 ? gradeOffset : (int?)null;
                        }
                    }
                    else if ( personElem.Attribute( "graduationDate" ) != null )
                    {
                        person.GraduationYear = DateTime.Parse( personElem.Attribute( "graduationDate" ).Value.Trim(), new CultureInfo( "en-US" ) ).Year;
                    }

                    // Now, if their age was given we'll change the given birth year to make them
                    // be this age as of Today.
                    if ( personElem.Attribute( "age" ) != null )
                    {
                        int age = int.Parse( personElem.Attribute( "age" ).Value.Trim() );
                        int ageDiff = person.Age - age ?? 0;
                        person.SetBirthDate( person.BirthDate.Value.AddYears( ageDiff ) );
                    }

                    person.EmailPreference = EmailPreference.EmailAllowed;

                    if ( personElem.Attribute( "email" ) != null )
                    {
                        var emailAddress = personElem.Attribute( "email" ).Value.Trim();
                        if ( emailAddress.IsValidEmail() )
                        {
                            person.Email = emailAddress;
                            person.IsEmailActive = personElem.Attribute( "emailIsActive" ) != null && personElem.Attribute( "emailIsActive" ).Value.AsBoolean();
                            if ( personElem.Attribute( "emailDoNotEmail" ) != null && personElem.Attribute( "emailDoNotEmail" ).Value.AsBoolean() )
                            {
                                person.EmailPreference = EmailPreference.DoNotEmail;
                            }
                        }
                    }

                    if ( personElem.Attribute( "photoUrl" ) != null )
                    {
                        person.Photo = SavePhoto( personElem.Attribute( "photoUrl" ).Value.Trim(), rockContext );
                    }

                    if ( personElem.Attribute( "recordType" ) != null && personElem.Attribute( "recordType" ).Value.Trim() == "person" )
                    {
                        person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    }

                    if ( personElem.Attribute( "maritalStatus" ) != null && personElem.Attribute( "maritalStatus" ).Value.Trim() == "married" )
                    {
                        person.MaritalStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;
                    }

                    switch ( personElem.Attribute( "recordStatus" ).Value.Trim() )
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
                        switch ( personElem.Attribute( "gender" ).Value.Trim().ToLower() )
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
                        switch ( personElem.Attribute( "connectionStatus" ).Value.Trim().ToLower() )
                        {
                            case "member":
                                person.ConnectionStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_MEMBER.AsGuid() ).Id;
                                break;
                            case "attendee":
                                person.ConnectionStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_ATTENDEE.AsGuid() ).Id;
                                break;
                            case "visitor":
                            default:
                                person.ConnectionStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() ).Id;
                                break;
                        }
                    }

                    if ( personElem.Attribute( "homePhone" ) != null && !string.IsNullOrEmpty( personElem.Attribute( "homePhone" ).Value.Trim() ) )
                    {
                        var phoneNumber = new PhoneNumber
                        {
                            NumberTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).Id,
                            Number = PhoneNumber.CleanNumber( personElem.Attribute( "homePhone" ).Value.Trim() )
                        };

                        // Format number since default SaveChanges() is not being used.
                        phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number );

                        person.PhoneNumbers.Add( phoneNumber );
                    }

                    if ( personElem.Attribute( "mobilePhone" ) != null && !string.IsNullOrEmpty( personElem.Attribute( "mobilePhone" ).Value.Trim() ) )
                    {
                        var phoneNumber = new PhoneNumber
                        {
                            NumberTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id,
                            Number = PhoneNumber.CleanNumber( personElem.Attribute( "mobilePhone" ).Value.Trim() )
                        };

                        // Format number since default SaveChanges() is not being used.
                        phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number );

                        person.PhoneNumbers.Add( phoneNumber );
                    }

                    if ( personElem.Attribute( "workPhone" ) != null && !string.IsNullOrEmpty( personElem.Attribute( "workPhone" ).Value.Trim() ) )
                    {
                        var phoneNumber = new PhoneNumber
                        {
                            NumberTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).Id,
                            Number = PhoneNumber.CleanNumber( personElem.Attribute( "workPhone" ).Value.Trim() )
                        };

                        // Format number since default SaveChanges() is not being used.
                        phoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number );

                        person.PhoneNumbers.Add( phoneNumber );
                    }

                    _personCache[guid] = person;
                }

                groupMember.Person = person;

                if ( personElem.Attribute( "familyRole" ) != null && personElem.Attribute( "familyRole" ).Value.Trim().ToLower() == "adult" )
                {
                    groupMember.GroupRoleId = _adultRoleId;
                }
                else
                {
                    groupMember.GroupRoleId = _childRoleId;
                }

                // person attributes
                if ( personElem.Elements( "attributes" ).Any() )
                {
                    AddPersonAttributes( groupMember, personElem.Elements( "attributes" ), rockContext );
                }

                // person logins
                if ( personElem.Elements( "logins" ).Any() )
                {
                    // in here we are just going to store them in a dictionary for later
                    // saving because Rock requires that each person have a ID before
                    // we can call the UserLoginService.Create()
                    var logins = new List<string>();
                    foreach ( var login in personElem.Elements( "logins" ).Elements( "login" ) )
                    {
                        logins.Add( login.Attribute( "userName" ).Value );
                    }

                    _peopleLoginsDictionary.Add( groupMember.Person, logins );
                }

                familyMembers.Add( groupMember );
            }

            return familyMembers;
        }

        /// <summary>
        /// Add a note on the given person's record.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="noteTypeName"></param>
        /// <param name="noteText"></param>
        /// <param name="noteDate">(optional) The date the note was created</param>
        /// <param name="byPersonGuid">(optional) The guid of the person who created the note</param>
        /// <param name="rockContext"></param>
        private void AddNote( int personId, string noteTypeName, string noteText, string noteDate, string byPersonGuid, string isPrivate, RockContext rockContext )
        {
            var service = new NoteTypeService( rockContext );
            var noteType = service.Get( _personEntityTypeId, noteTypeName );

            // Find the person's alias
            int? createdByPersonAliasId = null;
            if ( byPersonGuid != null )
            {
                createdByPersonAliasId = _personCache[byPersonGuid.AsGuid()].PrimaryAliasId;
            }

            var noteService = new NoteService( rockContext );
            var note = new Note()
            {
                IsSystem = false,
                NoteTypeId = noteType.Id,
                EntityId = personId,
                Caption = string.Empty,
                CreatedByPersonAliasId = createdByPersonAliasId,
                Text = noteText,
                CreatedDateTime = string.IsNullOrWhiteSpace( noteDate ) ? RockDateTime.Now : DateTime.Parse( noteDate, new CultureInfo( "en-US" ) )
            };

            noteService.Add( note );

            if ( isPrivate.AsBoolean() )
            {
                rockContext.SaveChanges( disablePrePostProcessing: true );
                note.MakePrivate( Rock.Security.Authorization.VIEW, _personCache[byPersonGuid.AsGuid()], rockContext );
            }

        }

        /// <summary>
        /// Adds any logins stored in the _peopleLoginsDictionary.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void AddPersonLogins( RockContext rockContext )
        {
            var password = tbPassword.Text.Trim();
            var userLoginService = new UserLoginService( rockContext );

            foreach ( var set in _peopleLoginsDictionary )
            {
                foreach ( var userName in set.Value )
                {
                    var userLogin = userLoginService.GetByUserName( userName );

                    // only create the login if the username is not already taken
                    if ( userLogin == null )
                    {
                        UserLoginService.Create(
                                            rockContext,
                                            set.Key,
                                            Rock.Model.AuthenticationServiceType.Internal,
                                            _authenticationDatabaseEntityTypeId,
                                            userName,
                                            password,
                                            isConfirmed: true );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the person attributes and values found in the XML to the person's attribute-value collection.
        /// </summary>
        /// <param name="groupMember"></param>
        /// <param name="attributes"></param>
        private void AddPersonAttributes( GroupMember groupMember, IEnumerable<XElement> attributes, RockContext rockContext )
        {
            // In order to add attributes to the person, you have to first load them all
            groupMember.Person.LoadAttributes( rockContext );

            foreach ( var personAttribute in attributes.Elements( "attribute" ) )
            {
                // Add them to the list of people who need to have their attribute values saved.
                // This will be done after all the family groups have been saved.
                _personWithAttributes[groupMember.Person.Guid] = true;
                foreach ( var pa in personAttribute.Attributes() )
                {
                    groupMember.Person.SetAttributeValue( pa.Name.LocalName, pa.Value );
                }
            }
        }

        /// <summary>
        /// Fetches the given remote photoUrl and stores it locally in the binary file table
        /// then returns Id of the binary file.
        /// </summary>
        /// <param name="photoUrl">a URL to a photo (jpg, png, bmp, tiff).</param>
        /// <returns>Id of the binaryFile</returns>
        private BinaryFile SavePhoto( string photoUrl, RockContext context )
        {
            // always create a new BinaryFile record of IsTemporary when a file is uploaded
            BinaryFile binaryFile = new BinaryFile();
            binaryFile.IsTemporary = true;
            binaryFile.BinaryFileTypeId = _binaryFileType.Id;
            binaryFile.FileName = Path.GetFileName( photoUrl );

            var webClient = new WebClient();
            try
            {
                binaryFile.ContentStream = new MemoryStream( webClient.DownloadData( photoUrl ) );

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

                // Because prepost processing is disabled for this rockcontext, need to
                // manually have the storage provider save the contents of the binary file
                binaryFile.SetStorageEntityTypeId( _storageEntityType.Id );
                binaryFile.StorageEntitySettings = _binaryFileTypeSettings;
                if ( binaryFile.StorageProvider != null )
                {
                    binaryFile.StorageProvider.SaveContent( binaryFile );
                    binaryFile.Path = binaryFile.StorageProvider.GetPath( binaryFile );
                }

                var binaryFileService = new BinaryFileService( context );
                binaryFileService.Add( binaryFile );
                return binaryFile;
            }
            catch ( WebException )
            {
                return null;
            }
        }

        /// <summary>
        /// Adds the given addresses in the xml snippet to the given family.
        /// </summary>
        /// <param name="groupService">The group service.</param>
        /// <param name="family">The family.</param>
        /// <param name="addresses">The addresses.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        private void AddFamilyAddresses( GroupService groupService, Group family, XElement addresses, RockContext rockContext )
        {
            if ( addresses == null || addresses.Elements( "address" ) == null )
            {
                return;
            }

            // First add each person to the familyMembers collection
            foreach ( var addressElem in addresses.Elements( "address" ) )
            {
                var addressType = ( addressElem.Attribute( "type" ) != null ) ? addressElem.Attribute( "type" ).Value.Trim() : string.Empty;
                var street1 = ( addressElem.Attribute( "street1" ) != null ) ? addressElem.Attribute( "street1" ).Value.Trim() : string.Empty;
                var street2 = ( addressElem.Attribute( "street2" ) != null ) ? addressElem.Attribute( "street2" ).Value.Trim() : string.Empty;
                var city = ( addressElem.Attribute( "city" ) != null ) ? addressElem.Attribute( "city" ).Value.Trim() : string.Empty;
                var state = ( addressElem.Attribute( "state" ) != null ) ? addressElem.Attribute( "state" ).Value.Trim() : string.Empty;
                var postalCode = ( addressElem.Attribute( "postalCode" ) != null ) ? addressElem.Attribute( "postalCode" ).Value.Trim() : string.Empty;
                var country = ( addressElem.Attribute( "country" ) != null ) ? addressElem.Attribute( "country" ).Value.Trim() : "US";
                var lat = ( addressElem.Attribute( "lat" ) != null ) ? addressElem.Attribute( "lat" ).Value.Trim() : string.Empty;
                var lng = ( addressElem.Attribute( "long" ) != null ) ? addressElem.Attribute( "long" ).Value.Trim() : string.Empty;

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

                // Call replica of the groupService's AddNewFamilyAddress in an attempt to speed it up
                AddNewFamilyAddress( family, locationTypeGuid, street1, street2, city, state, postalCode, country, rockContext );

                var location = family.GroupLocations.Where( gl => gl.Location.Street1 == street1 ).Select( gl => gl.Location ).FirstOrDefault();

                // Set the address with the given latitude and longitude
                double latitude;
                double longitude;
                if ( !string.IsNullOrEmpty( lat ) && !string.IsNullOrEmpty( lng )
                    && double.TryParse( lat, out latitude ) && double.TryParse( lng, out longitude )
                    && location != null )
                {
                    location.SetLocationPointFromLatLong( latitude, longitude );
                }

                // Put the location id into the dictionary for later use.
                if ( location != null && !_familyLocationDictionary.ContainsKey( family.Guid ) )
                {
                    _familyLocationDictionary.Add( family.Guid, location.Id );
                }
            }
        }

        /// <summary>
        /// Creates a new family using the given set of people.
        /// </summary>
        /// <param name="familyMembers">The family members.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        public Group CreateNewFamily( List<GroupMember> familyMembers, int? campusId )
        {
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();

            if ( familyGroupType != null )
            {
                var familyGroup = new Group();
                familyGroup.GroupTypeId = familyGroupType.Id;
                familyGroup.Name = familyMembers.FirstOrDefault().Person.LastName + " Family";
                familyGroup.CampusId = campusId;

                foreach ( var familyMember in familyMembers )
                {
                    var person = familyMember.Person;
                    if ( person != null )
                    {
                        familyGroup.Members.Add( familyMember );
                    }
                }

                foreach ( var groupMember in familyMembers )
                {
                    var person = groupMember.Person;

                    //if ( !person.Aliases.Any( a => a.AliasPersonId == person.Id ) )
                    //{
                    //    person.Aliases.Add( new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid } );
                    //}

                    if ( groupMember.GroupRoleId != _childRoleId )
                    {
                        person.GivingGroup = familyGroup;
                    }
                }

                return familyGroup;
            }

            return null;
        }

        /// <summary>
        /// Adds the new family address.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="locationTypeGuid">The location type unique identifier.</param>
        /// <param name="street1">The street1.</param>
        /// <param name="street2">The street2.</param>
        /// <param name="city">The city.</param>
        /// <param name="state">The state.</param>
        /// <param name="postalCode">The zip.</param>
        /// <param name="rockContext">The rock context.</param>
        public void AddNewFamilyAddress( Group family, string locationTypeGuid, 
            string street1, string street2, string city, string state, string postalCode, string country,
            RockContext rockContext )
        {
            if ( !string.IsNullOrWhiteSpace( street1 ) ||
                 !string.IsNullOrWhiteSpace( street2 ) ||
                 !string.IsNullOrWhiteSpace( city ) ||
                 !string.IsNullOrWhiteSpace( postalCode ) ||
                 !string.IsNullOrWhiteSpace( country ) )
            {
                var groupLocation = new GroupLocation();

                // Get new or existing location and associate it with group
                var location = new LocationService( rockContext ).Get( street1, street2, city, state, postalCode, country );
                groupLocation.Location = location;
                groupLocation.IsMailingLocation = true;
                groupLocation.IsMappedLocation = true;

                Guid guid = Guid.Empty;
                if ( Guid.TryParse( locationTypeGuid, out guid ) )
                {
                    var locationType = Rock.Web.Cache.DefinedValueCache.Read( guid );
                    if ( locationType != null )
                    {
                        groupLocation.GroupLocationTypeValueId = locationType.Id;
                    }
                }

                family.GroupLocations.Add( groupLocation );
            }
        }

        /// <summary>
        /// Flattens exception's innerexceptions and returns an Html formatted string
        /// useful for debugging.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string FlattenInnerExceptions( Exception ex )
        {
            StringBuilder sb = new StringBuilder();
            while ( ex != null && ex.InnerException != null )
            {
                sb.AppendLine( ex.InnerException.Message.ConvertCrLfToHtmlBr() );
                ex = ex.InnerException;
            }

            return sb.ToString();
        }

        #endregion

        #region Helper Class
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