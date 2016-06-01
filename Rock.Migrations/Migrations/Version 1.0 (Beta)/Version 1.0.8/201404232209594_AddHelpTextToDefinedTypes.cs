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
    public partial class AddHelpTextToDefinedTypes : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.DefinedType", "HelpText", c => c.String());
            AddGlobalAttribute( "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "", "", "Safe Sender Domains", "Delimited list of domains that can be used to send emails.  If an Email communication is created with a From Address that is not from one of these domains, the Organization Email global attribute value will be used instead for the From Address and the original value will be used as the Reply To address.  This is to help reduce the likelihood of communications being rejected by the receiving email servers.", 0, "", "CDD29C51-5D33-435F-96AB-2C06BA772F88" );

            Sql( @"  UPDATE [DefinedType]
  SET [HelpText] = 'Label merge fields are defined with a liquid syntax. Click the ''Show Merge Fields'' button below to view the available merge fields.

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
</div>'
  WHERE [Guid] = 'E4D289A9-70FA-4381-913E-2A757AD11147'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.DefinedType", "HelpText");
            DeleteAttribute( "CDD29C51-5D33-435F-96AB-2C06BA772F88" );
        }
    }
}
