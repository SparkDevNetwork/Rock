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
    public partial class PostalCode : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Location", "PostalCode", c => c.String(maxLength: 50));
            Sql( @"
    UPDATE [Location] SET [PostalCode] = [Zip]
" );
            DropColumn("dbo.Location", "Zip");

            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_0_10.xml" );

            Sql( @"
/*
<doc>
	<summary>
 		This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions
        The StatementGenerator utility uses this procedure along with querying transactions thru REST to generate statements
	</summary>

	<returns>
		* PersonId
        * GroupId
        * AddressPersonNames
        * Street1
        * Street2
        * City
        * State
        * PostalCode
        * StartDate
        * EndDate
        * CustomMessage1
        * CustomMessage2
	</returns>
	<param name=""StartDate"" datatype=""datetime"">The starting date of the date range</param>
    <param name=""EndDate"" datatype=""datetime"">The ending date of the date range</param>
	<param name=""AccountIds"" datatype=""varchar(max)"">Comma delimited list of account ids. NULL means all</param>
	<param name=""PersonId"" datatype=""int"">Person the statement if for. NULL means all persons that have transactions for the date range</param>
	<param name=""OrderByPostalCode"" datatype=""int"">Set to 1 to have the results sorted by PostalCode, 0 for no particular order</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1  -- year 2014 statements for all persons
	</code>
</doc>
*/
ALTER PROCEDURE [spFinance_ContributionStatementQuery]
	@StartDate datetime
    , @EndDate datetime
    , @AccountIds varchar(max) 
    , @PersonId int -- NULL means all persons
    , @OrderByPostalCode bit
AS
BEGIN
    DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'	
    DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'

    -- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    ;WITH tranListCTE
    AS
    (
        SELECT  
            [AuthorizedPersonId] 
        FROM 
            [FinancialTransaction] [ft]
        INNER JOIN 
            [FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
        WHERE 
            ([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
        AND 
            (
                (@AccountIds is null)
                OR
                (ftd.[AccountId] in (select * from ufnUtility_CsvToTable(@AccountIds)))
            )
    )

    SELECT 
        [pg].[PersonId]
        , [pg].[GroupId]
        , [pn].[PersonNames] [AddressPersonNames]
        , [l].[Street1]
        , [l].[Street2]
        , [l].[City]
        , [l].[State]
        , [l].[PostalCode]
        , @StartDate [StartDate]
        , @EndDate [EndDate]
        , null [CustomMessage1]
        , null [CustomMessage2]
    FROM (
        -- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
        -- These are Persons that give as part of a Group.  For example, Husband and Wife
        SELECT DISTINCT
            null [PersonId] 
            , [g].[Id] [GroupId]
        FROM 
            [Person] [p]
        INNER JOIN 
            [Group] [g] ON [p].[GivingGroupId] = [g].[Id]
        WHERE 
            [p].[Id] in (SELECT * FROM tranListCTE)
        UNION
        -- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
        -- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
        -- to determine which address(es) the statements need to be mailed to 
        SELECT  
            [p].[Id] [PersonId],
            [g].[Id] [GroupId]
        FROM
            [Person] [p]
        JOIN 
            [GroupMember] [gm]
        ON 
            [gm].[PersonId] = [p].[Id]
        JOIN 
            [Group] [g]
        ON 
            [gm].[GroupId] = [g].[Id]
        WHERE
            [p].[GivingGroupId] is null
        AND
            [g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
        AND [p].[Id] IN (SELECT * FROM tranListCTE)
    ) [pg]
    CROSS APPLY 
        [ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId]) [pn]
    JOIN 
        [GroupLocation] [gl] 
    ON 
        [gl].[GroupId] = [pg].[GroupId]
    JOIN
        [Location] [l]
    ON 
        [l].[Id] = [gl].[LocationId]
    WHERE 
        [gl].[IsMailingLocation] = 1
    AND
        [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
    AND
        (
            (@personId is null) 
        OR 
            ([pg].[PersonId] = @personId)
        )
    ORDER BY
    CASE WHEN @OrderByPostalCode = 1 THEN PostalCode END
END
" );

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
            PostalCode: null,
            Country: null,
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
  WHERE [Guid] = 'E4D289A9-70FA-4381-913E-2A757AD11147'
" );

            // Attrib for BlockType: Group Map:Info Window Contents
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Info Window Contents", "InfoWindowContents", "", "Liquid template for the info window. To suppress the window provide a blank template.", 6, @"
<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4> 
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>
    
    <div class='clearfix'>
		{% if Location.Street1 and Location.Street1 != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Address }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        <br>{{ GroupMember.ConnectionStatus }}
					{% endif %}
					{% if GroupMember.Email != '' %}
						<br>{{ GroupMember.Email }}
					{% endif %}
					{% for Phone in GroupMember.Person.PhoneNumbers %}
						<br>{{ Phone.NumberTypeValue.Name }}: {{ Phone.NumberFormatted }}
					{% endfor %}
				</div>
				<br>
			{% endfor -%}
		{% endif %}
    </div>
    
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ MapPageUrl }}'>View Map</a>
	{% endif %}

</div>
", "92B339D5-D8AF-4810-A7F8-09373DC5D0DE" );

            // Attrib Value for Block:Group Map, Attribute:Info Window Contents Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "92B339D5-D8AF-4810-A7F8-09373DC5D0DE", @"<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4> 
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>
    
    <div class='clearfix'>
		{% if Location.Street1 and Location.Street1 != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Address }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        ({{ GroupMember.ConnectionStatus }})
					{% endif %}
				</div>
			{% endfor -%}
		{% endif %}
    </div>
    
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ MapPageUrl }}'>View Map</a>
	{% endif %}

</div>" );

            Sql( @"
    -- Update the default address format
    DECLARE @AttributeId int = (SELECT [ID] FROM [Attribute] WHERE [Guid] = 'B6EF4138-C488-4043-A628-D35F91503843')
    DECLARE @crlf varchar(2) = char(13) + char(10)
    UPDATE [Attribute]
    SET [DefaultValue] = '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ City }}, {{ State }} {{ PostalCode }}' + @crlf + '{{ Country }}'
    WHERE [Id] = @AttributeId

    UPDATE [AttributeValue]
        SET [Value] = REPLACE([Value], '{{ Zip }}', '{{ PostalCode }}')
    WHERE [AttributeId] = @AttributeId
" );


            // Migration Rollups
            Sql( @"
    -- Update check-in kiosks to default to Print From Server (instead of Client) 
    DECLARE @KioskDeviceType int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'BC809626-1389-4543-B8BB-6FAC79C27AFD') 
    UPDATE [Device] SET [PrintFrom] = 1 WHERE [DeviceTypeValueId] = @KioskDeviceType

    -- Update filter to select members and attendees
    DECLARE @ParentFilterId int = (SELECT [DataViewFilterId] FROM [DataView] WHERE [Guid] = '0DA5F82F-CFFE-45AF-B725-49B3899A1F72')
    DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')
    DECLARE @FilterId int = (SELECT [Id] FROM [DataViewFilter] WHERE [ParentId] = @ParentFilterId AND [EntityTypeId] = @EntityTypeId AND [Selection] LIKE '%ConnectionStatusValueId%')
    UPDATE [DataViewFilter] SET [Selection] = '[ ""ConnectionStatusValueId"", ""[\r\n  \""39f491c5-d6ac-4a9b-8ac0-c431cb17d588\"",\r\n  \""41540783-d9ef-4c70-8f1d-c9e83d91ed5f\""\r\n]"" ]' WHERE [Id] = @FilterId

    -- Update filter to correctly select family adult role
    SET @EntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '0E239967-6D33-4205-B19F-08AD8FF6ED0B')
    SET @FilterId = (SELECT [Id] FROM [DataViewFilter] WHERE [ParentId] = @ParentFilterId AND [EntityTypeId] = @EntityTypeId )
    UPDATE [DataViewFilter] SET [Selection] = '790e3215-3b10-442b-af69-616c0dcb998e|2639f9a5-2aae-4e48-a8c3-4ffe86681e42' WHERE [Id] = @FilterId
" );

            RockMigrationHelper.AddBlockTypeAttribute( "04712F3D-9667-4901-A49D-4507573EF7AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "My Profile Page", "MyProfilePage", "", "Page for user to view their person profile (if blank option will not be displayed)", 0, "", "6CFDDF63-0B21-48FC-90AE-362C0E73420B" );
            RockMigrationHelper.AddBlockAttributeValue( "19C2140D-498A-4675-B8A2-18B281736F6E", "6CFDDF63-0B21-48FC-90AE-362C0E73420B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
            RockMigrationHelper.AddBlockAttributeValue( "82AF461F-022D-4ADB-BB12-F220CD605459", "6CFDDF63-0B21-48FC-90AE-362C0E73420B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
            RockMigrationHelper.AddBlockAttributeValue( "791A6AA0-D498-4795-BB5F-21609175826F", "6CFDDF63-0B21-48FC-90AE-362C0E73420B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
            RockMigrationHelper.AddBlockAttributeValue( "2356DEDC-803F-4782-A8E9-D0D88393EC2E", "6CFDDF63-0B21-48FC-90AE-362C0E73420B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
            RockMigrationHelper.AddBlockAttributeValue( "373DE813-5080-491B-BCB6-AAECEA87B27B", "6CFDDF63-0B21-48FC-90AE-362C0E73420B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Location", "Zip", c => c.String(maxLength: 10));
            Sql( @"
    UPDATE [Location] SET [Zip] = [PostalCode]
" );
            DropColumn( "dbo.Location", "PostalCode" );

            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata.xml" );

            Sql( @"
/*
<doc>
	<summary>
 		This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions
        The StatementGenerator utility uses this procedure along with querying transactions thru REST to generate statements
	</summary>

	<returns>
		* PersonId
        * GroupId
        * AddressPersonNames
        * Street1
        * Street2
        * City
        * State
        * Zip
        * StartDate
        * EndDate
        * CustomMessage1
        * CustomMessage2
	</returns>
	<param name=""StartDate"" datatype=""datetime"">The starting date of the date range</param>
    <param name=""EndDate"" datatype=""datetime"">The ending date of the date range</param>
	<param name=""AccountIds"" datatype=""varchar(max)"">Comma delimited list of account ids. NULL means all</param>
	<param name=""PersonId"" datatype=""int"">Person the statement if for. NULL means all persons that have transactions for the date range</param>
	<param name=""OrderByZipCode"" datatype=""int"">Set to 1 to have the results sorted by ZipCode, 0 for no particular order</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1  -- year 2014 statements for all persons
	</code>
</doc>
*/
ALTER PROCEDURE [spFinance_ContributionStatementQuery]
	@StartDate datetime
    , @EndDate datetime
    , @AccountIds varchar(max) 
    , @PersonId int -- NULL means all persons
    , @OrderByZipCode bit
AS
BEGIN
    DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'	
    DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'

    -- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    ;WITH tranListCTE
    AS
    (
        SELECT  
            [AuthorizedPersonId] 
        FROM 
            [FinancialTransaction] [ft]
        INNER JOIN 
            [FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
        WHERE 
            ([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
        AND 
            (
                (@AccountIds is null)
                OR
                (ftd.[AccountId] in (select * from ufnUtility_CsvToTable(@AccountIds)))
            )
    )

    SELECT 
        [pg].[PersonId]
        , [pg].[GroupId]
        , [pn].[PersonNames] [AddressPersonNames]
        , [l].[Street1]
        , [l].[Street2]
        , [l].[City]
        , [l].[State]
        , [l].[Zip]
        , @StartDate [StartDate]
        , @EndDate [EndDate]
        , null [CustomMessage1]
        , null [CustomMessage2]
    FROM (
        -- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
        -- These are Persons that give as part of a Group.  For example, Husband and Wife
        SELECT DISTINCT
            null [PersonId] 
            , [g].[Id] [GroupId]
        FROM 
            [Person] [p]
        INNER JOIN 
            [Group] [g] ON [p].[GivingGroupId] = [g].[Id]
        WHERE 
            [p].[Id] in (SELECT * FROM tranListCTE)
        UNION
        -- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
        -- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
        -- to determine which address(es) the statements need to be mailed to 
        SELECT  
            [p].[Id] [PersonId],
            [g].[Id] [GroupId]
        FROM
            [Person] [p]
        JOIN 
            [GroupMember] [gm]
        ON 
            [gm].[PersonId] = [p].[Id]
        JOIN 
            [Group] [g]
        ON 
            [gm].[GroupId] = [g].[Id]
        WHERE
            [p].[GivingGroupId] is null
        AND
            [g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
        AND [p].[Id] IN (SELECT * FROM tranListCTE)
    ) [pg]
    CROSS APPLY 
        [ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId]) [pn]
    JOIN 
        [GroupLocation] [gl] 
    ON 
        [gl].[GroupId] = [pg].[GroupId]
    JOIN
        [Location] [l]
    ON 
        [l].[Id] = [gl].[LocationId]
    WHERE 
        [gl].[IsMailingLocation] = 1
    AND
        [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
    AND
        (
            (@personId is null) 
        OR 
            ([pg].[PersonId] = @personId)
        )
    ORDER BY
    CASE WHEN @orderByZipCode = 1 THEN Zip END
END
" );

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
  WHERE [Guid] = 'E4D289A9-70FA-4381-913E-2A757AD11147'
" );

            // Attrib for BlockType: Group Map:Info Window Contents
            RockMigrationHelper.AddBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Info Window Contents", "InfoWindowContents", "", "Liquid template for the info window. To suppress the window provide a blank template.", 6, @"
<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4> 
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>
    
    <div class='clearfix'>
		{% if Location.Street1 and Location.Street1 != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Street1 }}
			<br>{{ Location.City }}, {{ Location.State }} {{ Location.Zip }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        <br>{{ GroupMember.ConnectionStatus }}
					{% endif %}
					{% if GroupMember.Email != '' %}
						<br>{{ GroupMember.Email }}
					{% endif %}
					{% for Phone in GroupMember.Person.PhoneNumbers %}
						<br>{{ Phone.NumberTypeValue.Name }}: {{ Phone.NumberFormatted }}
					{% endfor %}
				</div>
				<br>
			{% endfor -%}
		{% endif %}
    </div>
    
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ MapPageUrl }}'>View Map</a>
	{% endif %}

</div>
", "92B339D5-D8AF-4810-A7F8-09373DC5D0DE" );

            // Attrib Value for Block:Group Map, Attribute:Info Window Contents Page: Group Map, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80F0BB81-2209-4744-A6D9-747C6CE10760", "92B339D5-D8AF-4810-A7F8-09373DC5D0DE", @"<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4> 
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>
    
    <div class='clearfix'>
		{% if Location.Street1 and Location.Street1 != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Street1 }}
			<br>{{ Location.City }}, {{ Location.State }} {{ Location.Zip }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        ({{ GroupMember.ConnectionStatus }})
					{% endif %}
				</div>
			{% endfor -%}
		{% endif %}
    </div>
    
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ MapPageUrl }}'>View Map</a>
	{% endif %}

</div>" );

            Sql( @"
    -- Update the default address format
    DECLARE @AttributeId int = (SELECT [ID] FROM [Attribute] WHERE [Guid] = 'B6EF4138-C488-4043-A628-D35F91503843')
    DECLARE @crlf varchar(2) = char(13) + char(10)
    UPDATE [Attribute]
    SET [DefaultValue] = '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ City }}, {{ State }} {{ Zip }}' + @crlf + '{{ Country }}'
    WHERE [Id] = @AttributeId

    UPDATE [AttributeValue]
        SET [Value] = REPLACE([Value], '{{ PostalCode }}', '{{ Zip }}')
    WHERE [AttributeId] = @AttributeId
" );

        }
    }
}
