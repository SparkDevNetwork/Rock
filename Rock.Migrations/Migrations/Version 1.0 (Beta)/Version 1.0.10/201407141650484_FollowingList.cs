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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class FollowingList : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Following", "", "A6AE67F7-0B46-4F9A-9C96-054E1E82F784", "" ); // Site:Rock RMS
            Sql (@"
    UPDATE [Page] SET [Order] = 3 WHERE [Guid] = 'A6AE67F7-0B46-4F9A-9C96-054E1E82F784'
    UPDATE [Page] SET [Order] = 4 WHERE [Guid] = '2654EBE9-F585-4E64-93F3-102357F89660'
    UPDATE [Page] SET [Order] = 5 WHERE [Guid] = 'F0C4E25F-83DF-44FF-AB5A-EF6C3044FAD3'
");

            RockMigrationHelper.UpdateBlockType( "Person Following List", "Block for displaying people that current person follows.", "~/Blocks/Crm/PersonFollowingList.ascx", "CRM", "BD548744-DC6D-4870-9FED-BB9EA24E709B" );

            // Add Block to Page: Following, Site: Rock RMS
            RockMigrationHelper.AddBlock( "A6AE67F7-0B46-4F9A-9C96-054E1E82F784", "", "BD548744-DC6D-4870-9FED-BB9EA24E709B", "Person Following List", "Main", "", "", 0, "69A3E2AA-C9A5-4172-95BD-01E0B43377C2" );

            // Attrib for BlockType: Person Following List:Person Profile Page
            RockMigrationHelper.AddBlockTypeAttribute( "BD548744-DC6D-4870-9FED-BB9EA24E709B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "The person profile page.", 0, @"", "0232707A-7D0B-486E-BBE0-A8EC2A0028CB" );

            // Attrib Value for Block:Person Following List, Attribute:Person Profile Page Page: Following, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A3E2AA-C9A5-4172-95BD-01E0B43377C2", "0232707A-7D0B-486E-BBE0-A8EC2A0028CB", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );

            // Delete old layouts
            Sql (@"
    DELETE FROM [Layout] WHERE [Guid] IN ('195BCD57-1C10-4969-886F-7324B6287B75', '6AC471A3-9B0E-459B-ADA2-F6E18F970803', 'BACA6FF2-A228-4C47-9577-2BBDFDFD26BA')
");

            // Update Context settings for blocks on person profile that now require a type of context to be selected
            RockMigrationHelper.AddBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "375F7220-04C6-4E41-B99A-A2CE494FD74A" );
            RockMigrationHelper.AddBlockTypeAttribute( "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "B0500ABD-20C0-466A-B2D0-427B197C6001" );
            RockMigrationHelper.AddBlockAttributeValue( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0", "375F7220-04C6-4E41-B99A-A2CE494FD74A", "72657ED8-D16E-492E-AC12-144C5E7567E7" );
            RockMigrationHelper.AddBlockAttributeValue( "CD99F432-DFB4-4AA2-8B79-83B469448F98", "B0500ABD-20C0-466A-B2D0-427B197C6001", "72657ED8-D16E-492E-AC12-144C5E7567E7" );

            // Fix the Defined Type Help Text that were all updated incorrectly in previous migration
            Sql( @"
UPDATE [DefinedType] SET [HelpText] = NULL WHERE [IsSystem] = 1 AND [Guid] <> 'FC684FD7-FE68-493F-AF38-1656FBF67E6B'
UPDATE [DefinedType] SET [HelpText] = 'Label merge fields are defined with a liquid syntax. Click the ''Show Merge Fields'' button below to view the available merge fields.

<p>
    <a data-toggle=""collapse""  href=""#collapseMergeFields"" class=''btn btn-action btn-xs''>Show/Hide Merge Fields</a>
</p>

<div id=""collapseMergeFields"" class=""panel-collapse collapse"">
<pre>
{
  GlobalAttribute: {
    ContentFiletypeBlacklist: ascx,ashx,aspx,ascx.cs,ashx.cs,aspx.cs,cs,aspx.cs,php,exe,dll,
    ContentImageFiletypeWhitelist: jpg,png,gif,bmp,svg,
    CurrencySymbol: $,
    EmailExceptionsList: ,
    EmailFooter: ''redacted for simplicity''
    EmailHeader: ''redacted for simplicity''
    EmailHeaderLogo: assets/images/email-header.jpg,
    EnablePageViewTracking: Yes,
    GoogleAPIKey: ,
    GradeTransitionDate: 6/1,
    InternalApplicationRoot: http://rock.organization.com/Rock/,
    JobPulse: 7/13/2012 4:58:30 PM,
    Log404AsException: No,
    OrganizationAddress: 3120 W Cholla St Phoenix, AZ 85029,
    OrganizationEmail: info@organizationname.com,
    OrganizationName: Rock Solid Church,
    OrganizationPhone: ,
    OrganizationWebsite: www.organization.com,
    PasswordRegexFriendlyDescription: Password must be at least 6 characters long.,
    PasswordRegularExpression: \\\\w{6,255},
    PublicApplicationRoot: http://www.organization.com/Rock/,
    UpdateServerUrl: http://update.rockrms.com/F/rock/api/v2/
  },
  Person: {
    IsSystem: false,
    RecordTypeValueId: 1,
    RecordStatusValueId: 3,
    RecordStatusReasonValueId: null,
    ConnectionStatusValueId: 146,
    ReviewReasonValueId: null,
    IsDeceased: false,
    TitleValueId: null,
    FirstName: Alexis,
    NickName: Alex,
    MiddleName: null,
    LastName: Decker,
    SuffixValueId: null,
    PhotoId: 33,
    BirthDay: 10,
    BirthMonth: 2,
    BirthYear: 2007,
    Gender: 2,
    MaritalStatusValueId: null,
    AnniversaryDate: null,
    GraduationDate: null,
    GivingGroupId: null,
    Email: null,
    IsEmailActive: null,
    EmailNote: null,
    EmailPreference: 0,
    ReviewReasonNote: null,
    InactiveReasonNote: null,
    SystemNote: null,
    ViewedCount: null,
    PrimaryAliasId: null,
    FullName: Alex Decker,
    BirthdayDayOfWeek: Monday,
    BirthdayDayOfWeekShort: Mon,
    PhotoUrl: /GetImage.ashx?id=33,
    Users: [],
    PhoneNumbers: [],
    MaritalStatusValue: null,
    ConnectionStatusValue: null,
    ReviewReasonValue: null,
    RecordStatusValue: null,
    RecordStatusReasonValue: null,
    RecordTypeValue: null,
    SuffixValue: null,
    TitleValue: null,
    Photo: null,
    BirthDate: 2007-02-10T00:00:00,
    Age: 7,
    DaysToBirthday: 300,
    Grade: null,
    GradeFormatted: ,
    CreatedDateTime: null,
    ModifiedDateTime: null,
    CreatedByPersonAliasId: null,
    ModifiedByPersonAliasId: null,
    Id: 5,
    Guid: 27919690-3cce-4fa6-95c4-cd21419eb51f,
    ForeignId: null,
    UrlEncodedKey: EAAAAFzZCyahIp479hY272hHcUS6SR2hC2b4KTJdjUEFfFa9vXU!2bBVqMz0bw6Tv!2fP75RxKlB5!2fHOmg!2fbWURHFTOZ3ew!3d,
    AbilityLevel: ,
    AbilityLevel_unformatted: ,
    Allergy: ,
    Allergy_unformatted: ,
    Employer: ,
    Employer_unformatted: ,
    FirstVisit: ,
    FirstVisit_unformatted: null,
    LegalNotes: ,
    LegalNotes_unformatted: ,
    MembershipDate: ,
    MembershipDate_unformatted: ,
    Position: ,
    Position_unformatted: ,
    PreviousChurch: ,
    PreviousChurch_unformatted: ,
    School: ,
    School_unformatted: ,
    SecondVisit: ,
    SecondVisit_unformatted: null,
    SourceofVisit: ,
    SourceofVisit_unformatted: ,
    BaptismDate: ,
    BaptismDate_unformatted: ,
    BaptizedHere: No,
    BaptizedHere_unformatted: False,
    FamilyMember: true,
    LastCheckIn: 2014-04-16T11:37:46.487,
    SecurityCode: CNS
  },
  GroupType: {
    IsSystem: false,
    Name: Check-in Test Area,
    Description: Is used for testing the check-in system when there are no events occurring.,
    GroupTerm: Group,
    GroupMemberTerm: Member,
    DefaultGroupRoleId: null,
    AllowMultipleLocations: true,
    ShowInGroupList: false,
    ShowInNavigation: false,
    IconCssClass: ,
    TakesAttendance: true,
    AttendanceRule: 0,
    AttendancePrintTo: 0,
    Order: 0,
    InheritedGroupTypeId: null,
    LocationSelectionMode: 0,
    GroupTypePurposeValueId: null,
    Groups: [
      {
        IsSystem: false,
        ParentGroupId: null,
        GroupTypeId: 18,
        CampusId: null,
        Name: Test Group,
        Description: null,
        IsSecurityRole: false,
        IsActive: true,
        Order: 0,
        GroupType: null,
        Campus: null,
        Groups: [],
        Members: [],
        GroupLocations: [],
        CreatedDateTime: null,
        ModifiedDateTime: null,
        CreatedByPersonAliasId: null,
        ModifiedByPersonAliasId: null,
        Id: 24,
        Guid: cbbbeee0-de95-4876-9fef-5eb68fa67853,
        ForeignId: null,
        UrlEncodedKey: EAAAACQM7XsOo25!2bsF8gQ0!2b6lMb30gISI2TdxFsnd6reWQbsEbTW2axJ812FPOzgzkB5IAd1KWJIdl!2frRxXAJ!2fH!2fwAo!3d,
        LastCheckIn: 2014-04-16T11:37:46.487,
        Locations: [
          {
            ParentLocationId: 3,
            Name: Bunnies Room,
            IsActive: true,
            LocationTypeValueId: null,
            GeoPoint: null,
            GeoFence: null,
            Street1: null,
            Street2: null,
            City: null,
            State: null,
            Country: null,
            Zip: null,
            AssessorParcelId: null,
            StandardizeAttemptedDateTime: null,
            StandardizeAttemptedServiceType: null,
            StandardizeAttemptedResult: null,
            StandardizedDateTime: null,
            GeocodeAttemptedDateTime: null,
            GeocodeAttemptedServiceType: null,
            GeocodeAttemptedResult: null,
            GeocodedDateTime: null,
            IsGeoPointLocked: null,
            PrinterDeviceId: null,
            IsNamedLocation: true,
            LocationTypeValue: null,
            ChildLocations: [],
            PrinterDevice: null,
            CreatedDateTime: null,
            ModifiedDateTime: null,
            CreatedByPersonAliasId: null,
            ModifiedByPersonAliasId: null,
            Id: 4,
            Guid: 844336f4-88b4-4894-b416-769c95a4702d,
            ForeignId: null,
            UrlEncodedKey: EAAAAG5Idc5CTbXfvgtbQbTDUcwRqdWYVp!2fQw86KCKKrzbVWtPuPk51IWD5U6g6pVZFJcXt2tHAegPT0l8oeF4YATNU!3d,
            LastCheckIn: null,
            CurrentCount: 0,
            Schedules: [
              {
                Name: 4:30 (test),
                Description: null,
                iCalendarContent: BEGIN:VCALENDAR\\r\\nBEGIN:VEVENT\\r\\nDTEND:20130501T235900\\r\\nDTSTART:20130501T000100\\r\\nRRULE:FREQ=DAILY\\r\\nEND:VEVENT\\r\\nEND:VCALENDAR,
                CheckInStartOffsetMinutes: 0,
                CheckInEndOffsetMinutes: 1439,
                EffectiveStartDate: 2013-05-01T00:01:00Z,
                EffectiveEndDate: null,
                CategoryId: 50,
                Category: null,
                CreatedDateTime: null,
                ModifiedDateTime: null,
                CreatedByPersonAliasId: null,
                ModifiedByPersonAliasId: null,
                Id: 6,
                Guid: a5c81078-eb8c-46aa-bb91-1e2ba8ba76ae,
                ForeignId: null,
                UrlEncodedKey: EAAAAAbnxepCoI6YqY1pyaIR7iqQod364OEbwHYp8aIrd6NKKxqdrOkOFujtAo!2fZDmz1wxI0PortkoxhQ4sAHBubL4k!3d,
                LastCheckIn: null
              },
              {
                Name: 6:00 (test),
                Description: null,
                iCalendarContent: BEGIN:VCALENDAR\\r\\nBEGIN:VEVENT\\r\\nDTEND:20130501T235900\\r\\nDTSTART:20130501T000100\\r\\nRRULE:FREQ=DAILY\\r\\nEND:VEVENT\\r\\nEND:VCALENDAR,
                CheckInStartOffsetMinutes: 0,
                CheckInEndOffsetMinutes: 1439,
                EffectiveStartDate: 2013-05-01T00:01:00Z,
                EffectiveEndDate: null,
                CategoryId: 50,
                Category: null,
                CreatedDateTime: null,
                ModifiedDateTime: null,
                CreatedByPersonAliasId: null,
                ModifiedByPersonAliasId: null,
                Id: 7,
                Guid: c8b7beb4-54e2-4473-822f-f5d0f8ce19d7,
                ForeignId: null,
                UrlEncodedKey: EAAAAPLsBPGDcl8jssl6DRNR!2f9tEfwz85aKG1nEU!2f7ztvhdOJO3l!2bR0f6dpdD5nBFTGTs3!2fq!2fu!2b9R3YaS4RSBmaEysU!3d,
                LastCheckIn: null
              }
            ]
          }
        ]
      }
    ],
    ChildGroupTypes: [],
    Roles: [],
    LocationTypes: [],
    DefaultGroupRole: null,
    GroupTypePurposeValue: null,
    CreatedDateTime: null,
    ModifiedDateTime: null,
    CreatedByPersonAliasId: null,
    ModifiedByPersonAliasId: null,
    Id: 18,
    Guid: caaf4f9b-58b9-45b4-aabc-9188347948b7,
    ForeignId: null,
    UrlEncodedKey: EAAAAFYZtWH5zckUJnP!2bEslOBPP8C9fP8rFXixUtF3TdNoinVI7Si!2fQ3!2fCoQ2b2lhq1k71O2JjUpb4EoBSkd7i621lM!3d,
    LastCheckIn: 2014-04-16T11:37:46.487
  }
}
</pre>
</div>' WHERE [Guid] = 'E4D289A9-70FA-4381-913E-2A757AD11147'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Person Following List:Person Profile Page
            RockMigrationHelper.DeleteAttribute( "0232707A-7D0B-486E-BBE0-A8EC2A0028CB" );

            // Remove Block: Person Following List, from Page: Following, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "69A3E2AA-C9A5-4172-95BD-01E0B43377C2" );
            RockMigrationHelper.DeleteBlockType( "BD548744-DC6D-4870-9FED-BB9EA24E709B" ); // Person Following List
            RockMigrationHelper.DeletePage( "A6AE67F7-0B46-4F9A-9C96-054E1E82F784" ); // Page: FollowingLayout: Full Width, Site: Rock RMS
        }
    }
}
