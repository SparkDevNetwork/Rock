
/********************************************************************************************************************
 Sign-Ups - Add/remove sample data.

 NOTE: This script assumes Sample Data (Power Tools > Sample Data) has been added for Rock version 1.15.0 or higher.
*********************************************************************************************************************/

DECLARE @IsDeleteMode [bit] = 0; -- 0 to add new data; 1 to delete any data added by this script

/***************************
 DON'T EDIT ANYTHING BELOW.
****************************/

---------------------------------------------------------------------------------------------------
DECLARE @Groups TABLE
(
    [Id] [int] IDENTITY(1,1) NOT NULL
    , [Name] [nvarchar](100) NOT NULL
    , [Guid] [uniqueidentifier] NOT NULL
    , [ParentGroupGuid] [uniqueidentifier] NOT NULL
    , [GroupTypeGuid] [uniqueidentifier] NOT NULL
    , [ProjectTypeGuid] [uniqueidentifier] NOT NULL
    , [IsActive] [bit] NOT NULL
    , [CampusGuid] [uniqueidentifier] NULL
    , [Description] [nvarchar](max) NULL
);

--INSERT INTO @Groups
--VALUES
--(
--    '' -- [Name] [nvarchar](100) NOT NULL
--    , @GroupGuid -- [Guid] [uniqueidentifier] NOT NULL
--    , @ParentGroupGuid -- [ParentGroupGuid] [uniqueidentifier] NOT NULL
--    , @GroupTypeGuid_SignUpGroup -- [GroupTypeGuid] [uniqueidentifier] NOT NULL
--    , @DefinedValueGuid_ProjectType_InPerson -- [ProjectTypeGuid] [uniqueidentifier] NOT NULL
--    , 1 -- [IsActive] [bit] NOT NULL
--    , NULL -- [CampusGuid] [uniqueidentifier] NULL
--    , NULL -- [Description] [nvarchar](max) NULL
--);

---------------------------------------------------------------------------------------------------
DECLARE @Locations TABLE
(
    [Id] [int] IDENTITY(1,1) NOT NULL
    , [Guid] [uniqueidentifier] NOT NULL
    , [Name] [nvarchar](100) NULL
    , [GeoPoint] [geography] NULL
    , [Street1] [nvarchar](100) NULL
    , [Street2] [nvarchar](100) NULL
    , [City] [nvarchar](50) NULL
    , [State] [nvarchar](50) NULL
    , [Country] [nvarchar](50) NULL
    , [PostalCode] [nvarchar](50) NULL
    , [County] [varchar](50) NULL
);

--INSERT INTO @Locations
--VALUES
--(
--    @LocationGuid -- [Guid] [uniqueidentifier] NOT NULL
--    , NULL -- [Name] [nvarchar](100) NULL
--    , NULL -- [GeoPoint] [geography] NULL
--    , '' -- [Street1] [nvarchar](100) NULL
--    , NULL -- [Street2] [nvarchar](100) NULL
--    , '' -- [City] [nvarchar](50) NULL
--    , '' -- [State] [nvarchar](50) NULL
--    , '' -- [Country] [nvarchar](50) NULL
--    , '' -- [PostalCode] [nvarchar](50) NULL
--    , '' -- [County] [varchar](50) NULL
--);

---------------------------------------------------------------------------------------------------
DECLARE @Schedules TABLE
(
    [Id] [int] IDENTITY(1,1) NOT NULL
    , [Guid] [uniqueidentifier] NOT NULL
    , [iCalendarContent] [nvarchar](max) NOT NULL
    , [EffectiveStartDate] [date] NULL
    , [EffectiveEndDate] [date] NULL
    , [Name] [nvarchar](50) NULL
);

--INSERT INTO @Schedules
--VALUES
--(
--    @ScheduleGuid -- [Guid] [uniqueidentifier] NOT NULL
--    , '' -- [iCalendarContent] [nvarchar](max) NOT NULL
--    , NULL -- [EffectiveStartDate] [date] NULL
--    , NULL -- [EffectiveEndDate] [date] NULL
--    , NULL -- [Name] [nvarchar](50) NULL
--);

---------------------------------------------------------------------------------------------------
DECLARE @Opportunities TABLE
(
    [Id] [int] IDENTITY(1,1) NOT NULL
    , [Guid] [uniqueidentifier] NOT NULL
    , [GroupGuid] [uniqueidentifier] NOT NULL
    , [LocationGuid] [uniqueidentifier] NOT NULL
    , [ScheduleGuid] [uniqueidentifier] NOT NULL
    , [Name] [nvarchar](100) NULL
    , [MinimumCapacity] [int] NULL
    , [DesiredCapacity] [int] NULL
    , [MaximumCapacity] [int] NULL
    , [ConfirmationAdditionalDetails] [nvarchar](max) NULL
    , [ReminderAdditionalDetails] [nvarchar](max) NULL
);

--INSERT INTO @Opportunities
--VALUES
--(
--    @OpportunityGuid -- [Guid] [uniqueidentifier] NOT NULL
--    , @GroupGuid -- [GroupGuid] [uniqueidentifier] NOT NULL
--    , @LocationGuid -- [LocationGuid] [uniqueidentifier] NOT NULL
--    , @ScheduleGuid -- [ScheduleGuid] [uniqueidentifier] NOT NULL
--    , '' -- [Name] [nvarchar](100) NULL
--    , NULL -- [MinimumCapacity] [int] NULL
--    , NULL -- [DesiredCapacity] [int] NULL
--    , NULL -- [MaximumCapacity] [int] NULL
--    , NULL -- [ConfirmationAdditionalDetails] [nvarchar](max) NULL
--    , NULL -- [ReminderAdditionalDetails] [nvarchar](max) NULL
--);

---------------------------------------------------------------------------------------------------
DECLARE @Members TABLE
(
    [Id] [int] IDENTITY(1,1) NOT NULL
    , [OpportunityGuid] [uniqueidentifier] NOT NULL
    , [PersonGuid] [uniqueidentifier] NOT NULL
    , [IsLeader] [bit] NOT NULL
);

--INSERT INTO @Members
--VALUES
--(
--    OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
--    , '' -- [PersonGuid] [uniqueidentifier] NOT NULL
--    , 0 -- [IsLeader] [bit] NOT NULL
--);

--INSERT INTO @Members
--VALUES
--(
--    @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
--    , '' -- 
--    , 0 -- [IsLeader] [bit] NOT NULL
--)

---------------------------------------------------------------------------------------------------
-- These are well known Guids.
---------------------------------------------------------------------------------------------------
DECLARE @GroupTypeGuid_SignUpGroup [uniqueidentifier] = '499B1367-06B3-4538-9D56-56D53F55DCB1';
DECLARE @GroupTypeRoleGuid_SignUpGroup_Leader [uniqueidentifier] = '9F063DA9-CB3D-49EB-B4EC-BC909A00FDD4';
DECLARE @GroupTypeRoleGuid_SignUpGroup_Member [uniqueidentifier] = '3849B594-C079-4AB3-869A-E1DEC6790A34';

DECLARE @GroupGuid_SignUpGroups [uniqueidentifier] = 'D649638A-EF91-42D8-9B38-32172D614A5F';

DECLARE @CampusGuid_Main [uniqueidentifier] = '76882AE3-1CE8-42A6-A2B6-8C0B29CF8CF8';

DECLARE @DefinedTypeGuid_ProjectType [uniqueidentifier] = 'B7842AF3-6F04-495E-9A6C-F403D06C02F3';
DECLARE @DefinedValueGuid_ProjectType_InPerson [uniqueidentifier] = 'FF3F0C5C-9775-4A09-9CCF-94902DB99BF6';
DECLARE @DefinedValueGuid_ProjectType_ProjectDue [uniqueidentifier] = 'C999D489-5B8F-4892-BCC3-90DFFBC524F5';

DECLARE @Now [datetime] = (SELECT GETDATE());
DECLARE @DateTimeString_Now [char](15) = (SELECT FORMAT(@Now, 'yyyyMMddTHHmmss'));

DECLARE @DateTimeString_Today [char](8) = (SELECT FORMAT(@Now, 'yyyyMMdd'));

DECLARE @DateString_OneWeekInPast [char](8) = (SELECT FORMAT(DATEADD(week, -1, @Now), 'yyyyMMdd'));
DECLARE @DateString_OneWeekInFuture [char](8) = (SELECT FORMAT(DATEADD(week, 1, @Now), 'yyyyMMdd'));

DECLARE @DateString_TwoWeeksInPast [char](8) = (SELECT FORMAT(DATEADD(week, -2, @Now), 'yyyyMMdd'));
DECLARE @DateString_TwoWeeksInFuture [char](8) = (SELECT FORMAT(DATEADD(week, 2, @Now), 'yyyyMMdd'));

DECLARE @DateString_OneYearInPast [char](8) = (SELECT FORMAT(DATEADD(year, -1, @Now), 'yyyyMMdd'));
DECLARE @DateString_OneYearInFuture [char](8) = (SELECT FORMAT(DATEADD(year, 1, @Now), 'yyyyMMdd'));

DECLARE @ParentGroupGuid [uniqueidentifier]
    , @GroupGuid [uniqueidentifier]
    , @LocationGuid [uniqueidentifier]
    , @ScheduleGuid [uniqueidentifier]
    , @OpportunityGuid [uniqueidentifier];

---------------------------------------------------------------------------------------------------
BEGIN -- region Feed My Starving Children
    SET @ParentGroupGuid = @GroupGuid_SignUpGroups;
    -- All Guids below (EXCEPT those within the "INSERT INTO @Members" sections) are new: made specifically for this script.
    SET @GroupGuid = '353C7EEE-6257-4A0C-A081-87386F8F8134';

    INSERT INTO @Groups
    VALUES
    (
        'Feed My Starving Children' -- [Name] [nvarchar](100) NOT NULL
        , @GroupGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @ParentGroupGuid -- [ParentGroupGuid] [uniqueidentifier] NOT NULL
        , @GroupTypeGuid_SignUpGroup -- [GroupTypeGuid] [uniqueidentifier] NOT NULL
        , @DefinedValueGuid_ProjectType_InPerson -- [ProjectTypeGuid] [uniqueidentifier] NOT NULL
        , 1 -- [IsActive] [bit] NOT NULL
        , NULL -- [CampusGuid] [uniqueidentifier] NULL
        , 'Turn hunger into hope with your own two hands by packing nutritious meals for hungry children around the world.' -- [Description] [nvarchar](max) NULL
    );

    SET @LocationGuid = '31346174-0643-45E1-96D0-0A6FDE33A76A';
    SET @ScheduleGuid = 'EDF0FBD3-4F93-46CD-BEA8-8DD2351F5AB1';
    SET @OpportunityGuid = '719CF517-9516-4509-890F-081E8D681967';

    INSERT INTO @Locations
    VALUES
    (
        @LocationGuid -- [Guid] [uniqueidentifier] NOT NULL
        , NULL -- [Name] [nvarchar](100) NULL
        , 0xE6100000010C1973D712F2B1404042B28009DCF65BC0 -- [GeoPoint] [geography] NULL
        , '1345 S Alma School Rd' -- [Street1] [nvarchar](100) NULL
        , NULL -- [Street2] [nvarchar](100) NULL
        , 'Mesa' -- [City] [nvarchar](50) NULL
        , 'AZ' -- [State] [nvarchar](50) NULL
        , 'US' -- [Country] [nvarchar](50) NULL
        , '85210-2085' -- [PostalCode] [nvarchar](50) NULL
        , 'Maricopa' -- [County] [varchar](50) NULL
    );

    INSERT INTO @Schedules
    VALUES
    (
        @ScheduleGuid -- [Guid] [uniqueidentifier] NOT NULL
        , CONCAT('BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:', @DateString_OneYearInPast,'T110000
DTSTAMP:', @DateTimeString_Now,'
DTSTART:', @DateString_OneYearInPast, 'T090000
SEQUENCE:0
UID:3FC3D2DA-A89C-4958-8D73-6500C0D22ABC
END:VEVENT
END:VCALENDAR
') -- [iCalendarContent] [nvarchar](max) NOT NULL
        , @DateString_OneYearInPast -- [EffectiveStartDate] [date] NULL
        , @DateString_OneYearInPast -- [EffectiveEndDate] [date] NULL
        , NULL -- [Name] [nvarchar](50) NULL
    );

    INSERT INTO @Opportunities
    VALUES
    (
        @OpportunityGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @GroupGuid -- [GroupGuid] [uniqueidentifier] NOT NULL
        , @LocationGuid -- [LocationGuid] [uniqueidentifier] NOT NULL
        , @ScheduleGuid -- [ScheduleGuid] [uniqueidentifier] NOT NULL
        , 'Inagural FMSC Event - East Valley (Permanant Site)' -- [Name] [nvarchar](100) NULL
        , 5 -- [MinimumCapacity] [int] NULL
        , 8 -- [DesiredCapacity] [int] NULL
        , 10 -- [MaximumCapacity] [int] NULL
        , NULL -- [ConfirmationAdditionalDetails] [nvarchar](max) NULL
        , NULL -- [ReminderAdditionalDetails] [nvarchar](max) NULL
    );

    INSERT INTO @Members
    VALUES
    (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4' -- Ted Decker
        , 1 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'B71494DB-D809-451A-A950-28898D0FD92C' -- Cindy Decker
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '32AAB9E4-970D-4551-A17E-385E66113BD5' -- Noah Decker
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '27919690-3CCE-4FA6-95C4-CD21419EB51F' -- Alex Decker
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '69DC0FDC-B451-4303-BD91-EF17C0015D23' -- Alisha Marble
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '1EA811BB-3118-42D1-B020-32A82BC8081A' -- Bill Marble
        , 0 -- [IsLeader] [bit] NOT NULL
    );

    -----------------------------------------------------------------------------------------------
    SET @LocationGuid = '54E2318F-4372-4C0B-95B3-80BC79FDF5E5';
    SET @ScheduleGuid = '0A6E4901-77E6-431C-9CA3-AD9EE689AAD7';
    SET @OpportunityGuid = 'E21FECD5-A9E3-4C22-A24F-A69B0B6E07EE';

    INSERT INTO @Locations
    VALUES
    (
        @LocationGuid -- [Guid] [uniqueidentifier] NOT NULL
        , NULL -- [Name] [nvarchar](100) NULL
        , 0xE6100000010C1973D712F2B1404042B28009DCF65BC0 -- [GeoPoint] [geography] NULL
        , '1345 S Alma School Rd' -- [Street1] [nvarchar](100) NULL
        , NULL -- [Street2] [nvarchar](100) NULL
        , 'Mesa' -- [City] [nvarchar](50) NULL
        , 'AZ' -- [State] [nvarchar](50) NULL
        , 'US' -- [Country] [nvarchar](50) NULL
        , '85210-2085' -- [PostalCode] [nvarchar](50) NULL
        , 'Maricopa' -- [County] [varchar](50) NULL
    );

    -- Demonstrate an opportunity that was today, but has likely already passed since it started at 7:00 AM (so it shouldn't show up in the Sign-Up Finder block).
    INSERT INTO @Schedules
    VALUES
    (
        @ScheduleGuid -- [Guid] [uniqueidentifier] NOT NULL
        , CONCAT('BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:', @DateTimeString_Today,'T090000
DTSTAMP:', @DateTimeString_Now,'
DTSTART:', @DateTimeString_Today, 'T070000
SEQUENCE:0
UID:1f60c85e-dca2-4614-b09d-7c7b11d09b3e
END:VEVENT
END:VCALENDAR
') -- [iCalendarContent] [nvarchar](max) NOT NULL
        , @DateTimeString_Today -- [EffectiveStartDate] [date] NULL
        , @DateTimeString_Today -- [EffectiveEndDate] [date] NULL
        , NULL -- [Name] [nvarchar](50) NULL
    );

    INSERT INTO @Opportunities
    VALUES
    (
        @OpportunityGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @GroupGuid -- [GroupGuid] [uniqueidentifier] NOT NULL
        , @LocationGuid -- [LocationGuid] [uniqueidentifier] NOT NULL
        , @ScheduleGuid -- [ScheduleGuid] [uniqueidentifier] NOT NULL
        , 'FMSC - East Valley (Permanant Site)' -- [Name] [nvarchar](100) NULL
        , 30 -- [MinimumCapacity] [int] NULL
        , 50 -- [DesiredCapacity] [int] NULL
        , 60 -- [MaximumCapacity] [int] NULL
        , NULL -- [ConfirmationAdditionalDetails] [nvarchar](max) NULL
        , NULL -- [ReminderAdditionalDetails] [nvarchar](max) NULL
    );

    -----------------------------------------------------------------------------------------------
    SET @LocationGuid = '31346174-0643-45E1-96D0-0A6FDE33A76A';
    SET @ScheduleGuid = '53B79AD0-0304-4B5F-B15D-77FFFE570DA4';
    SET @OpportunityGuid = '67C8C422-5B92-4C9D-8953-421CD827DE87';

    INSERT INTO @Locations
    VALUES
    (
        @LocationGuid -- [Guid] [uniqueidentifier] NOT NULL
        , NULL -- [Name] [nvarchar](100) NULL
        , 0xE6100000010C1973D712F2B1404042B28009DCF65BC0 -- [GeoPoint] [geography] NULL
        , '1345 S Alma School Rd' -- [Street1] [nvarchar](100) NULL
        , NULL -- [Street2] [nvarchar](100) NULL
        , 'Mesa' -- [City] [nvarchar](50) NULL
        , 'AZ' -- [State] [nvarchar](50) NULL
        , 'US' -- [Country] [nvarchar](50) NULL
        , '85210-2085' -- [PostalCode] [nvarchar](50) NULL
        , 'Maricopa' -- [County] [varchar](50) NULL
    );

    INSERT INTO @Schedules
    VALUES
    (
        @ScheduleGuid -- [Guid] [uniqueidentifier] NOT NULL
        , CONCAT('BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:', @DateString_OneWeekInFuture,'T110000
DTSTAMP:', @DateTimeString_Now,'
DTSTART:', @DateString_OneWeekInFuture, 'T090000
SEQUENCE:0
UID:753f3bb6-f9dd-495a-94bd-97ac0901917c
END:VEVENT
END:VCALENDAR
') -- [iCalendarContent] [nvarchar](max) NOT NULL
        , @DateString_OneWeekInFuture -- [EffectiveStartDate] [date] NULL
        , @DateString_OneWeekInFuture -- [EffectiveEndDate] [date] NULL
        , NULL -- [Name] [nvarchar](50) NULL
    );

    INSERT INTO @Opportunities
    VALUES
    (
        @OpportunityGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @GroupGuid -- [GroupGuid] [uniqueidentifier] NOT NULL
        , @LocationGuid -- [LocationGuid] [uniqueidentifier] NOT NULL
        , @ScheduleGuid -- [ScheduleGuid] [uniqueidentifier] NOT NULL
        , 'FMSC - East Valley (Permanant Site)' -- [Name] [nvarchar](100) NULL
        , 30 -- [MinimumCapacity] [int] NULL
        , 50 -- [DesiredCapacity] [int] NULL
        , 60 -- [MaximumCapacity] [int] NULL
        , NULL -- [ConfirmationAdditionalDetails] [nvarchar](max) NULL
        , NULL -- [ReminderAdditionalDetails] [nvarchar](max) NULL
    );

    INSERT INTO @Members
    VALUES
    (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4' -- Ted Decker
        , 1 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'B71494DB-D809-451A-A950-28898D0FD92C' -- Cindy Decker
        , 1 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '32AAB9E4-970D-4551-A17E-385E66113BD5' -- Noah Decker
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '27919690-3CCE-4FA6-95C4-CD21419EB51F' -- Alex Decker
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '69DC0FDC-B451-4303-BD91-EF17C0015D23' -- Alisha Marble
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '1EA811BB-3118-42D1-B020-32A82BC8081A' -- Bill Marble
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '28C4F2C0-0691-468F-B40E-550BD2976B9E' -- Bob Greggs
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '1A5A4E5F-E0A4-4EFA-B7D7-6698153ED718' -- Lorraine Greggs
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '5DB35E3C-8A84-488B-9738-FB3CACD4C517' -- Tim Greggs
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'BDF4A42D-5C22-4E73-868D-17DEF16280D9' -- Jordan Greggs
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '146B2103-FB28-49C6-B1A4-218C397B739E' -- Jenny Greggs
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'C28DD35E-E58A-4D5F-81A9-47877B9AE03B' -- Brian Peterson
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '3E52CD9D-79D4-40FC-AC71-26688C084A27' -- Becky Peterson
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '8DE6710B-BDAB-4617-AE0C-83E77D3E77CC' -- Rob Tennant
        , 1 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '773AD0F9-369A-47BC-BEBB-2E8EE4C5FEF5' -- Sheryl Tennant
        , 0 -- [IsLeader] [bit] NOT NULL
    );

    -----------------------------------------------------------------------------------------------
    SET @LocationGuid = 'EBFBF7D0-F9DF-4079-8ABF-87FACD5B43E1';
    SET @ScheduleGuid = '41C3D6E0-E572-47AA-8D39-58F049EEF75D';
    SET @OpportunityGuid = 'FF35BB37-9F86-4D71-BC31-12EAC5C41052';

    INSERT INTO @Locations
    VALUES
    (
        @LocationGuid -- [Guid] [uniqueidentifier] NOT NULL
        , NULL -- [Name] [nvarchar](100) NULL
        , 0xE6100000010C1A51DA1B7CD5404022C32ADEC8165CC0 -- [GeoPoint] [geography] NULL
        , '13724 W Meeker Blvd' -- [Street1] [nvarchar](100) NULL
        , NULL -- [Street2] [nvarchar](100) NULL
        , 'Sun City West' -- [City] [nvarchar](50) NULL
        , 'AZ' -- [State] [nvarchar](50) NULL
        , 'US' -- [Country] [nvarchar](50) NULL
        , '85375-3730' -- [PostalCode] [nvarchar](50) NULL
        , 'Maricopa' -- [County] [varchar](50) NULL
    );

    INSERT INTO @Schedules
    VALUES
    (
        @ScheduleGuid -- [Guid] [uniqueidentifier] NOT NULL
        , CONCAT('BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:', @DateString_OneWeekInPast,'T110000
DTSTAMP:', @DateTimeString_Now,'
DTSTART:', @DateString_OneWeekInPast, 'T090000
SEQUENCE:0
UID:23AE92BE-80C7-486B-966D-01561A309EC0
END:VEVENT
END:VCALENDAR
') -- [iCalendarContent] [nvarchar](max) NOT NULL
        , @DateString_OneWeekInPast -- [EffectiveStartDate] [date] NULL
        , @DateString_OneWeekInPast -- [EffectiveEndDate] [date] NULL
        , NULL -- [Name] [nvarchar](50) NULL
    );

    INSERT INTO @Opportunities
    VALUES
    (
        @OpportunityGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @GroupGuid -- [GroupGuid] [uniqueidentifier] NOT NULL
        , @LocationGuid -- [LocationGuid] [uniqueidentifier] NOT NULL
        , @ScheduleGuid -- [ScheduleGuid] [uniqueidentifier] NOT NULL
        , 'FMSC - West Valley (MobilePack)' -- [Name] [nvarchar](100) NULL
        , 4 -- [MinimumCapacity] [int] NULL
        , 6 -- [DesiredCapacity] [int] NULL
        , 8 -- [MaximumCapacity] [int] NULL
        , NULL -- [ConfirmationAdditionalDetails] [nvarchar](max) NULL
        , NULL -- [ReminderAdditionalDetails] [nvarchar](max) NULL
    );

    INSERT INTO @Members
    VALUES
    (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'BE66C5D3-F43E-4F9A-BA62-B7C0103BF54C' -- Pete Foster
        , 1 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'F4450E80-F221-4556-881D-CB92B008C2DA' -- Pamela Foster
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '3C402382-3BD2-4337-A996-9E62F1BAB09D' -- Ben Jones
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '3D7F6605-3666-4AB5-9F4E-D7FEBF93278E' -- Brian Jones
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '00E0DBB3-5451-456E-ADAB-E12C5EF4C285' -- Marty Webb
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '6C42BB46-67F7-4D58-93F3-73A7FBE1ADBB' -- Deb Webb
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'FA38A268-6744-4507-9CDC-69A7AC6CC0E5' -- Sean Hansen
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '3BB26A8B-F004-43E4-82BB-620D1E657AF9' -- Nancy Turner
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '92D04619-3F97-4EFF-932E-61DA56441995' -- Brian Gilbert
        , 0 -- [IsLeader] [bit] NOT NULL
    );

    -----------------------------------------------------------------------------------------------
    SET @LocationGuid = 'EBFBF7D0-F9DF-4079-8ABF-87FACD5B43E1';
    SET @ScheduleGuid = 'F0F5C376-736A-4D18-9DBE-2C630C6B57F7';
    SET @OpportunityGuid = '585BE36F-785B-439E-BD03-B8D4A2DD04BF';

    INSERT INTO @Locations
    VALUES
    (
        @LocationGuid -- [Guid] [uniqueidentifier] NOT NULL
        , NULL -- [Name] [nvarchar](100) NULL
        , 0xE6100000010C1A51DA1B7CD5404022C32ADEC8165CC0 -- [GeoPoint] [geography] NULL
        , '13724 W Meeker Blvd' -- [Street1] [nvarchar](100) NULL
        , NULL -- [Street2] [nvarchar](100) NULL
        , 'Sun City West' -- [City] [nvarchar](50) NULL
        , 'AZ' -- [State] [nvarchar](50) NULL
        , 'US' -- [Country] [nvarchar](50) NULL
        , '85375-3730' -- [PostalCode] [nvarchar](50) NULL
        , 'Maricopa' -- [County] [varchar](50) NULL
    );

    INSERT INTO @Schedules
    VALUES
    (
        @ScheduleGuid -- [Guid] [uniqueidentifier] NOT NULL
        , CONCAT('BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:', @DateString_OneWeekInFuture,'T110000
DTSTAMP:', @DateTimeString_Now,'
DTSTART:', @DateString_OneWeekInFuture, 'T090000
SEQUENCE:0
UID:7C7F6AD7-A65E-43B1-A56D-AEA6A023C017
END:VEVENT
END:VCALENDAR
') -- [iCalendarContent] [nvarchar](max) NOT NULL
        , @DateString_OneWeekInFuture -- [EffectiveStartDate] [date] NULL
        , @DateString_OneWeekInFuture -- [EffectiveEndDate] [date] NULL
        , NULL -- [Name] [nvarchar](50) NULL
    );

    INSERT INTO @Opportunities
    VALUES
    (
        @OpportunityGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @GroupGuid -- [GroupGuid] [uniqueidentifier] NOT NULL
        , @LocationGuid -- [LocationGuid] [uniqueidentifier] NOT NULL
        , @ScheduleGuid -- [ScheduleGuid] [uniqueidentifier] NOT NULL
        , 'FMSC - West Valley (MobilePack)' -- [Name] [nvarchar](100) NULL
        , 10 -- [MinimumCapacity] [int] NULL
        , 20 -- [DesiredCapacity] [int] NULL
        , 30 -- [MaximumCapacity] [int] NULL
        , NULL -- [ConfirmationAdditionalDetails] [nvarchar](max) NULL
        , NULL -- [ReminderAdditionalDetails] [nvarchar](max) NULL
    );

    INSERT INTO @Members
    VALUES
    (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'BE66C5D3-F43E-4F9A-BA62-B7C0103BF54C' -- Pete Foster
        , 1 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'F4450E80-F221-4556-881D-CB92B008C2DA' -- Pamela Foster
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '3C402382-3BD2-4337-A996-9E62F1BAB09D' -- Ben Jones
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '3D7F6605-3666-4AB5-9F4E-D7FEBF93278E' -- Brian Jones
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '00E0DBB3-5451-456E-ADAB-E12C5EF4C285' -- Marty Webb
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '6C42BB46-67F7-4D58-93F3-73A7FBE1ADBB' -- Deb Webb
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'FA38A268-6744-4507-9CDC-69A7AC6CC0E5' -- Sean Hansen
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '3BB26A8B-F004-43E4-82BB-620D1E657AF9' -- Nancy Turner
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '92D04619-3F97-4EFF-932E-61DA56441995' -- Brian Gilbert
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'D35B865D-BF50-4A7D-848F-6C0ED7A2AE67' -- Wendy Gilbert
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'DBAE45C0-1980-48FA-8201-41C08910F654' -- James Sweeney
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '988195A3-1895-492E-839B-653C35307862' -- Nancy Sweeney
        , 0 -- [IsLeader] [bit] NOT NULL
    );

    -----------------------------------------------------------------------------------------------
    SET @LocationGuid = '31346174-0643-45E1-96D0-0A6FDE33A76A';
    SET @ScheduleGuid = '32B15116-6E09-4B24-8025-9175EF91B0C9';
    SET @OpportunityGuid = 'E4CB449F-4D5F-4100-838C-910866ACF215';

    INSERT INTO @Locations
    VALUES
    (
        @LocationGuid -- [Guid] [uniqueidentifier] NOT NULL
        , NULL -- [Name] [nvarchar](100) NULL
        , 0xE6100000010C1973D712F2B1404042B28009DCF65BC0 -- [GeoPoint] [geography] NULL
        , '1345 S Alma School Rd' -- [Street1] [nvarchar](100) NULL
        , NULL -- [Street2] [nvarchar](100) NULL
        , 'Mesa' -- [City] [nvarchar](50) NULL
        , 'AZ' -- [State] [nvarchar](50) NULL
        , 'US' -- [Country] [nvarchar](50) NULL
        , '85210-2085' -- [PostalCode] [nvarchar](50) NULL
        , 'Maricopa' -- [County] [varchar](50) NULL
    );

    INSERT INTO @Schedules
    VALUES
    (
        @ScheduleGuid -- [Guid] [uniqueidentifier] NOT NULL
        , CONCAT('BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:', @DateString_OneYearInFuture,'T110000
DTSTAMP:', @DateTimeString_Now,'
DTSTART:', @DateString_OneYearInFuture, 'T090000
SEQUENCE:0
UID:2346E461-A98C-4C40-86A2-91F4A2C829C4
END:VEVENT
END:VCALENDAR
') -- [iCalendarContent] [nvarchar](max) NOT NULL
        , @DateString_OneYearInFuture -- [EffectiveStartDate] [date] NULL
        , @DateString_OneYearInFuture -- [EffectiveEndDate] [date] NULL
        , NULL -- [Name] [nvarchar](50) NULL
    );

    INSERT INTO @Opportunities
    VALUES
    (
        @OpportunityGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @GroupGuid -- [GroupGuid] [uniqueidentifier] NOT NULL
        , @LocationGuid -- [LocationGuid] [uniqueidentifier] NOT NULL
        , @ScheduleGuid -- [ScheduleGuid] [uniqueidentifier] NOT NULL
        , 'FMSC Event - East Valley (Permanant Site)' -- [Name] [nvarchar](100) NULL
        , 30 -- [MinimumCapacity] [int] NULL
        , 50 -- [DesiredCapacity] [int] NULL
        , 60 -- [MaximumCapacity] [int] NULL
        , NULL -- [ConfirmationAdditionalDetails] [nvarchar](max) NULL
        , NULL -- [ReminderAdditionalDetails] [nvarchar](max) NULL
    );
END

---------------------------------------------------------------------------------------------------
BEGIN -- region Habitat for Humanity
    SET @ParentGroupGuid = @GroupGuid_SignUpGroups;
    SET @GroupGuid = 'CC712BEB-B74C-416F-B226-D524F5CF5381';

    INSERT INTO @Groups
    VALUES
    (
        'Habitat for Humanity' -- [Name] [nvarchar](100) NOT NULL
        , @GroupGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @ParentGroupGuid -- [ParentGroupGuid] [uniqueidentifier] NOT NULL
        , @GroupTypeGuid_SignUpGroup -- [GroupTypeGuid] [uniqueidentifier] NOT NULL
        , @DefinedValueGuid_ProjectType_InPerson -- [ProjectTypeGuid] [uniqueidentifier] NOT NULL
        , 1 -- [IsActive] [bit] NOT NULL
        , NULL -- [CampusGuid] [uniqueidentifier] NULL
        , 'Habitat for Humanity is a nonprofit organization that helps families build and improve places to call home. We believe affordable housing plays a critical role in strong and stable communities.' -- [Description] [nvarchar](max) NULL
    );

    -----------------------------------------------------------------------------------------------
    SET @ParentGroupGuid = @GroupGuid;
    SET @GroupGuid = 'D417EEF0-3AB7-4C56-B7B1-E3CEC4CD2BC3';

    INSERT INTO @Groups
    VALUES
    
    (
        'Home Preservation' -- [Name] [nvarchar](100) NOT NULL
        , @GroupGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @ParentGroupGuid -- [ParentGroupGuid] [uniqueidentifier] NOT NULL
        , @GroupTypeGuid_SignUpGroup -- [GroupTypeGuid] [uniqueidentifier] NOT NULL
        , @DefinedValueGuid_ProjectType_InPerson -- [ProjectTypeGuid] [uniqueidentifier] NOT NULL
        , 1 -- [IsActive] [bit] NOT NULL
        , NULL -- [CampusGuid] [uniqueidentifier] NULL
        , 'Home repair services like painting, landscaping, weatherization and minor repairs, ensuring that families live in safe, decent homes for years to come.' -- [Description] [nvarchar](max) NULL
    );

    -----------------------------------------------------------------------------------------------
    SET @GroupGuid = '997862DB-C58B-4438-8834-9E2C5A0D92CC';

    INSERT INTO @Groups
    VALUES
    
    (
        'Women Build' -- [Name] [nvarchar](100) NOT NULL
        , @GroupGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @ParentGroupGuid -- [ParentGroupGuid] [uniqueidentifier] NOT NULL
        , @GroupTypeGuid_SignUpGroup -- [GroupTypeGuid] [uniqueidentifier] NOT NULL
        , @DefinedValueGuid_ProjectType_InPerson -- [ProjectTypeGuid] [uniqueidentifier] NOT NULL
        , 1 -- [IsActive] [bit] NOT NULL
        , NULL -- [CampusGuid] [uniqueidentifier] NULL
        , 'Since 1991, Women Build volunteers from all walks of life have come together to build stronger, safer communities.' -- [Description] [nvarchar](max) NULL
    );

    SET @LocationGuid = 'A6535DAC-B0EC-46A1-BD82-59CA1DB4AC86';
    SET @ScheduleGuid = 'EE8EBC8B-DD64-41FA-82C5-529B7389D7FE';
    SET @OpportunityGuid = 'EFA0EC52-D711-4BBE-B489-22FF790AF003';

    INSERT INTO @Locations
    VALUES
    (
        @LocationGuid -- [Guid] [uniqueidentifier] NOT NULL
        , NULL -- [Name] [nvarchar](100) NULL
        , 0xE6100000010C79758E01D9CB40403563D17476105CC0 -- [GeoPoint] [geography] NULL
        , '9133 Grand Ave.' -- [Street1] [nvarchar](100) NULL
        , NULL -- [Street2] [nvarchar](100) NULL
        , 'Peoria' -- [City] [nvarchar](50) NULL
        , 'AZ' -- [State] [nvarchar](50) NULL
        , 'US' -- [Country] [nvarchar](50) NULL
        , '85345-8189' -- [PostalCode] [nvarchar](50) NULL
        , 'Maricopa' -- [County] [varchar](50) NULL
    );

    INSERT INTO @Schedules
    VALUES
    (
        @ScheduleGuid -- [Guid] [uniqueidentifier] NOT NULL
        , CONCAT('BEGIN:VCALENDAR
PRODID:-//github.com/rianjs/ical.net//NONSGML ical.net 4.0//EN
VERSION:2.0
BEGIN:VEVENT
DTEND:', @DateString_TwoWeeksInFuture,'T110000
DTSTAMP:', @DateTimeString_Now,'
DTSTART:', @DateString_TwoWeeksInFuture, 'T090000
SEQUENCE:0
UID:559C1673-8A09-4E8E-AF96-2948B5129E3F
END:VEVENT
END:VCALENDAR
') -- [iCalendarContent] [nvarchar](max) NOT NULL
        , @DateString_TwoWeeksInFuture -- [EffectiveStartDate] [date] NULL
        , @DateString_TwoWeeksInFuture -- [EffectiveEndDate] [date] NULL
        , NULL -- [Name] [nvarchar](50) NULL
    );

    INSERT INTO @Opportunities
    VALUES
    (
        @OpportunityGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @GroupGuid -- [GroupGuid] [uniqueidentifier] NOT NULL
        , @LocationGuid -- [LocationGuid] [uniqueidentifier] NOT NULL
        , @ScheduleGuid -- [ScheduleGuid] [uniqueidentifier] NOT NULL
        , 'Peoria ReStore Volunteer' -- [Name] [nvarchar](100) NULL
        , 4 -- [MinimumCapacity] [int] NULL
        , 6 -- [DesiredCapacity] [int] NULL
        , 10 -- [MaximumCapacity] [int] NULL
        , NULL -- [ConfirmationAdditionalDetails] [nvarchar](max) NULL
        , NULL -- [ReminderAdditionalDetails] [nvarchar](max) NULL
    );

    INSERT INTO @Members
    VALUES
    (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'B71494DB-D809-451A-A950-28898D0FD92C' -- Cindy Decker
        , 1 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '69DC0FDC-B451-4303-BD91-EF17C0015D23' -- Alisha Marble
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'F4450E80-F221-4556-881D-CB92B008C2DA' -- Pamela Foster
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '1A5A4E5F-E0A4-4EFA-B7D7-6698153ED718' -- Lorraine Greggs
        , 0 -- [IsLeader] [bit] NOT NULL
    ),    
    (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , 'D35B865D-BF50-4A7D-848F-6C0ED7A2AE67' -- Wendy Gilbert
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '3E52CD9D-79D4-40FC-AC71-26688C084A27' -- Becky Peterson
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '988195A3-1895-492E-839B-653C35307862' -- Nancy Sweeney
        , 0 -- [IsLeader] [bit] NOT NULL
    )
    , (
        @OpportunityGuid -- [OpportunityGuid] [uniqueidentifier] NOT NULL
        , '773AD0F9-369A-47BC-BEBB-2E8EE4C5FEF5' -- Sheryl Tennant
        , 0 -- [IsLeader] [bit] NOT NULL
    );

    -----------------------------------------------------------------------------------------------
    SET @GroupGuid = '46D9BD61-BCF3-4D04-B2D8-9B68B1665715';

    INSERT INTO @Groups
    VALUES
    (
        'Blanket Drive' -- [Name] [nvarchar](100) NOT NULL
        , @GroupGuid -- [Guid] [uniqueidentifier] NOT NULL
        , @ParentGroupGuid -- [ParentGroupGuid] [uniqueidentifier] NOT NULL
        , @GroupTypeGuid_SignUpGroup -- [GroupTypeGuid] [uniqueidentifier] NOT NULL
        , @DefinedValueGuid_ProjectType_ProjectDue -- [ProjectTypeGuid] [uniqueidentifier] NOT NULL
        , 1 -- [IsActive] [bit] NOT NULL
        , @CampusGuid_Main -- [CampusGuid] [uniqueidentifier] NULL
        , 'Help us serve our homeless community with warmth.' -- [Description] [nvarchar](max) NULL
    );
END

---------------------------------------------------------------------------------------------------
BEGIN -- region INSERT/DELETE records
    DECLARE @GroupTypeId_SignUpGroup [int] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = @GroupTypeGuid_SignUpGroup); -- If we want to seed other group types, this hard-coded value will have to be dynamic.
    DECLARE @AttributeId_SignUpGroup_ProjectType [int] = (SELECT [Id] FROM [Attribute] WHERE [EntityTypeQualifierColumn] = 'GroupTypeId'
                                                                                            AND [EntityTypeQualifierValue] = @GroupTypeId_SignUpGroup
                                                                                            AND [Key] = 'ProjectType');

    IF @IsDeleteMode = 1
    BEGIN
        DECLARE @GroupsToDelete TABLE
        (
            [Id] [int] NOT NULL
        );
        INSERT INTO @GroupsToDelete
        SELECT g.[Id]
        FROM [Group] g
        INNER JOIN @Groups groupsAdded
            ON groupsAdded.[Guid] = g.[Guid];

        DECLARE @LocationsToDelete TABLE
        (
            [Id] [int] NOT NULL
        );
        INSERT INTO @LocationsToDelete
        SELECT l.[Id]
        FROM [Location] l
        INNER JOIN @Locations locationsAdded
            ON locationsAdded.[Guid] = l.[Guid];

        DECLARE @SchedulesToDelete TABLE
        (
            [Id] [int] NOT NULL
        );
        INSERT INTO @SchedulesToDelete
        SELECT s.[Id]
        FROM [Schedule] s
        INNER JOIN @Schedules schedulesAdded
            ON schedulesAdded.[Guid] = s.[Guid]

        DECLARE @GroupMembersToDelete TABLE
        (
            [Id] [int] NOT NULL
        );
        INSERT INTO @GroupMembersToDelete
        SELECT gm.[Id]
        FROM [GroupMember] gm
        INNER JOIN @GroupsToDelete g
            ON g.[Id] = gm.[GroupId];

        DELETE FROM [GroupMemberAssignment]
        WHERE [GroupMemberId] IN (SELECT [Id] FROM @GroupMembersToDelete);

        DELETE FROM [GroupMember]
        WHERE [Id] IN (SELECT [Id] FROM @GroupMembersToDelete);

        DELETE FROM [GroupLocationScheduleConfig]
        WHERE [GroupLocationId] IN
            (
                SELECT [Id]
                FROM [GroupLocation]
                WHERE [GroupId] IN (SELECT [Id] FROM @GroupsToDelete)
                    AND [LocationId] IN (SELECT [Id] FROM @LocationsToDelete)
            )
            AND [ScheduleId] IN (SELECT [Id] FROM @SchedulesToDelete);

        DELETE FROM [GroupLocationSchedule]
        WHERE [GroupLocationId] IN
            (
                SELECT [Id]
                FROM [GroupLocation]
                WHERE [GroupId] IN (SELECT [Id] FROM @GroupsToDelete)
                    AND [LocationId] IN (SELECT [Id] FROM @LocationsToDelete)
            )
            AND [ScheduleId] IN (SELECT [Id] FROM @SchedulesToDelete);

        DELETE FROM [GroupLocation]
        WHERE [GroupId] IN (SELECT [Id] FROM @GroupsToDelete);
    
        DELETE FROM [AttributeValue]
        WHERE [AttributeId] = @AttributeId_SignUpGroup_ProjectType
            AND [EntityId] IN (SELECT [Id] FROM @GroupsToDelete);

        DELETE FROM [Group]
        WHERE [Id] IN (SELECT [Id] FROM @GroupsToDelete);

        DELETE FROM [Schedule]
        WHERE [Id] IN (SELECT [Id] FrOM @SchedulesToDelete);

        DELETE FROM [Location]
        WHERE [Id] IN (SELECT [Id] FROM @LocationsToDelete);
    END
    ELSE
    BEGIN
        DECLARE @PersonAliasId_Admin [int] = (SELECT [Id] FROM [PersonAlias] WHERE [Guid] = '996C8B72-C255-40E6-BB98-B1D5CF345F3B');

        DECLARE @GroupId [int]
            , @ProjectTypeGuid [uniqueidentifier];

        DECLARE @iGroup [int] = 1;
        DECLARE @iGroupMax [int] = (SELECT MAX([Id]) FROM @Groups);
        WHILE @iGroup <= @iGroupMax
        BEGIN
            SELECT @GroupGuid = [Guid]
                , @ProjectTypeGuid = [ProjectTypeGuid]
            FROM @Groups
            WHERE [Id] = @iGroup;

            SET @GroupId = (SELECT [Id] FROM [Group] WHERE [Guid] = @GroupGuid);

            IF @GroupId IS NULL
            BEGIN
                INSERT INTO [Group]
                (
                    [IsSystem]
                    , [ParentGroupId]
                    , [GroupTypeId]
                    , [CampusId]
                    , [Name]
                    , [Description]
                    , [IsSecurityRole]
                    , [IsActive]
                    , [Order]
                    , [Guid]
                    , [CreatedDateTime]
                    , [ModifiedDateTime]
                    , [CreatedByPersonAliasId]
                    , [ModifiedByPersonAliasId]
                )
                SELECT 0
                    , pGroup.[Id]
                    , gt.[Id]
                    , c.[Id]
                    , g.[Name]
                    , g.[Description]
                    , 0
                    , g.[IsActive]
                    , 0
                    , g.[Guid]
                    , @Now
                    , @Now
                    , @PersonAliasId_Admin
                    , @PersonAliasId_Admin
                FROM @Groups g
                LEFT OUTER JOIN [Group] pGroup
                    ON pGroup.[Guid] = [ParentGroupGuid]
                INNER JOIN [GroupType] gt
                    ON gt.[Guid] = [GroupTypeGuid]
                LEFT OUTER JOIN [Campus] c
                    ON c.[Guid] = [CampusGuid]
                WHERE g.[Id] = @iGroup;

                SET @GroupId = (SELECT @@IDENTITY);
            END

            IF NOT EXISTS
            (
                SELECT [Id]
                FROM [AttributeValue]
                WHERE [AttributeId] = @AttributeId_SignUpGroup_ProjectType
                    AND [EntityId] = @GroupId
            )
            BEGIN
                INSERT INTO [AttributeValue]
                (
                    [IsSystem]
                    , [AttributeId]
                    , [EntityId]
                    , [Value]
                    , [Guid]
                    , [CreatedDateTime]
                    , [ModifiedDateTime]
                    , [CreatedByPersonAliasId]
                    , [ModifiedByPersonAliasId]
                )
                VALUES
                (
                    0
                    , @AttributeId_SignUpGroup_ProjectType
                    , @GroupId
                    , @ProjectTypeGuid
                    , NEWID()
                    , @Now
                    , @Now
                    , @PersonAliasId_Admin
                    , @PersonAliasId_Admin
                );
            END

            SET @iGroup = @iGroup + 1;
        END

        DECLARE @iLocation [int] = 1;
        DECLARE @iLocationMax [int] = (SELECT MAX([Id]) FROM @Locations);
        WHILE @iLocation <= @iLocationMax
        BEGIN
            IF NOT EXISTS
            (
                SELECT [Id]
                FROM [Location]
                WHERE [Guid] = (SELECT [Guid] FROM @Locations WHERE [Id] = @iLocation)
            )
            BEGIN
                INSERT INTO [Location]
                (
                    [Name]
                    , [IsActive]
                    , [GeoPoint]
                    , [Street1]
                    , [Street2]
                    , [City]
                    , [State]
                    , [Country]
                    , [PostalCode]
                    , [County]
                    , [Guid]
                    , [CreatedDateTime]
                    , [ModifiedDateTime]
                    , [CreatedByPersonAliasId]
                    , [ModifiedByPersonAliasId]
                )
                SELECT [Name]
                    , 1
                    , [GeoPoint]
                    , [Street1]
                    , [Street2]
                    , [City]
                    , [State]
                    , [Country]
                    , [PostalCode]
                    , [County]
                    , [Guid]
                    , @Now
                    , @Now
                    , @PersonAliasId_Admin
                    , @PersonAliasId_Admin
                FROM @Locations
                WHERE [Id] = @iLocation;
            END

            SET @iLocation = @iLocation + 1;
        END

        DECLARE @iSchedule [int] = 1;
        DECLARE @iScheduleMax [int] = (SELECT MAX([Id]) FROM @Schedules);
        WHILE @iSchedule <= @iScheduleMax
        BEGIN
            IF NOT EXISTS
            (
                SELECT [Id]
                FROM [Schedule]
                WHERE [Guid] = (SELECT [Guid] FROM @Schedules WHERE [Id] = @iSchedule)
            )
            BEGIN
                INSERT INTO [Schedule]
                (
                    [Name]
                    , [iCalendarContent]
                    , [EffectiveStartDate]
                    , [EffectiveEndDate]
                    , [Guid]
                    , [CreatedDateTime]
                    , [ModifiedDateTime]
                    , [CreatedByPersonAliasId]
                    , [ModifiedByPersonAliasId]
                    , [IsActive]
                )
                SELECT [Name]
                    , [iCalendarContent]
                    , [EffectiveStartDate]
                    , [EffectiveEndDate]
                    , [Guid]
                    , @Now
                    , @Now
                    , @PersonAliasId_Admin
                    , @PersonAliasId_Admin
                    , 1
                FROM @Schedules
                WHERE [Id] = @iSchedule;
            END

            SET @iSchedule = @iSchedule + 1;
        END

        DECLARE @LocationId [int]
            , @ScheduleId [int]
            , @GroupLocationId [int]
            , @GroupLocationScheduleId [int];

        DECLARE @MembersToAssign TABLE
        (
            [Id] [int] NOT NULL
            , [OpportunityGuid] [uniqueidentifier] NOT NULL
            , [PersonGuid] [uniqueidentifier] NOT NULL
            , [IsLeader] [bit] NOT NULL
        );

        DECLARE @GroupTypeRoleId_SignUpGroup_Leader [int] = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = @GroupTypeRoleGuid_SignUpGroup_Leader);
        DECLARE @GroupTypeRoleId_SignUpGroup_Member [int] = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = @GroupTypeRoleGuid_SignUpGroup_Member);

        DECLARE @iMember [int]
            , @iMemberMax [int]
            , @PersonGuid [uniqueidentifier]
            , @PersonId [int]
            , @GroupMemberId [int]
            , @GroupRoleId [int]
            , @GroupTypeId [int];

        DECLARE @iOpportunity [int] = 1;
        DECLARE @iOpportunityMax [int] = (SELECT Max([Id]) FROM @Opportunities);
        WHILE @iOpportunity <= @iOpportunityMax
        BEGIN
            SELECT @OpportunityGuid = [Guid]
                , @GroupGuid = [GroupGuid]
                , @LocationGuid = [LocationGuid]
                , @ScheduleGuid = [ScheduleGuid]
            FROM @Opportunities
            WHERE [Id] = @iOpportunity;

            SELECT @GroupId = [Id]
                , @GroupTypeId = [GroupTypeId]
            FROM [Group] WHERE [Guid] = @GroupGuid;

            SET @LocationId = (SELECT [Id] FROM [Location] WHERE [Guid] = @LocationGuid);
            SET @GroupLocationId = (SELECT [Id] FROM [GroupLocation] WHERE [GroupId] = @GroupId AND [LocationId] = @LocationId);

            IF @GroupLocationId IS NULL
            BEGIN
                INSERT INTO [GroupLocation]
                (
                    [GroupId]
                    , [LocationId]
                    , [IsMailingLocation]
                    , [IsMappedLocation]
                    , [Guid]
                    , [CreatedDateTime]
                    , [ModifiedDateTime]
                    , [CreatedByPersonAliasId]
                    , [ModifiedByPersonAliasId]
                )
                VALUES
                (
                    @GroupId
                    , @LocationId
                    , 0
                    , 0
                    , NEWID()
                    , @Now
                    , @Now
                    , @PersonAliasId_Admin
                    , @PersonAliasId_Admin
                );

                SET @GroupLocationId = (SELECT @@IDENTITY);
            END

            SET @ScheduleId = (SELECT [Id] FROM [Schedule] WHERE [Guid] = @ScheduleGuid);

            IF NOT EXISTS
            (
                SELECT *
                FROM [GroupLocationSchedule]
                WHERE [GroupLocationId] = @GroupLocationId AND [ScheduleId] = @ScheduleId
            )
            BEGIN
                INSERT INTO [GroupLocationSchedule]
                (
                    [GroupLocationId]
                    , [ScheduleId]
                )
                VALUES
                (
                    @GroupLocationId
                    , @ScheduleId
                );
            END

            IF NOT EXISTS
            (
                SELECT *
                FROM [GroupLocationScheduleConfig]
                WHERE [GroupLocationId] = @GroupLocationId AND [ScheduleId] = @ScheduleId
            )
            BEGIN
                INSERT INTO [GroupLocationScheduleConfig]
                (
                    [GroupLocationId]
                    , [ScheduleId]
                    , [MinimumCapacity]
                    , [DesiredCapacity]
                    , [MaximumCapacity]
                    , [ConfirmationAdditionalDetails]
                    , [ConfigurationName]
                    , [ReminderAdditionalDetails]
                )
                SELECT @GroupLocationId
                    , @ScheduleId
                    , [MinimumCapacity]
                    , [DesiredCapacity]
                    , [MaximumCapacity]
                    , [ConfirmationAdditionalDetails]
                    , [Name]
                    , [ReminderAdditionalDetails]
                FROM @Opportunities
                WHERE [Id] = @iOpportunity;
            END

            DELETE FROM @MembersToAssign;
            INSERT INTO @MembersToAssign
            SELECT * FROM @Members WHERE [OpportunityGuid] = @OpportunityGuid;

            SET @iMember = (SELECT MIN([Id]) FROM @MembersToAssign);
            SET @iMemberMax = (SELECT MAX([Id]) FROM @MembersToAssign);
            WHILE @iMember <= @iMemberMax
            BEGIN
                SELECT @PersonGuid = [PersonGuid]
                    , @GroupRoleId = CASE WHEN [IsLeader] = 1 THEN @GroupTypeRoleId_SignUpGroup_Leader ELSE @GroupTypeRoleId_SignUpGroup_Member END
                FROM @MembersToAssign
                WHERE [Id] = @iMember;

                SET @PersonId = (SELECT [Id] FROM [Person] WHERE [Guid] = @PersonGuid);
                SET @GroupMemberId = (SELECT [Id] FROM [GroupMember] WHERE [GroupId] = @GroupId AND [PersonId] = @PersonId);

                IF @GroupMemberId IS NULL
                BEGIN
                    INSERT INTO [GroupMember]
                    (
                        [IsSystem]
                        , [GroupId]
                        , [PersonId]
                        , [GroupRoleId]
                        , [GroupMemberStatus]
                        , [Guid]
                        , [CreatedDateTime]
                        , [ModifiedDateTime]
                        , [CreatedByPersonAliasId]
                        , [ModifiedByPersonAliasId]
                        , [GroupTypeId]
                    )
                    VALUES
                    (
                        0
                        , @GroupId
                        , @PersonId
                        , @GroupRoleId
                        , 1
                        , NEWID()
                        , @Now
                        , @Now
                        , @PersonAliasId_Admin
                        , @PersonAliasId_Admin
                        , @GroupTypeId
                    );

                    SET @GroupMemberId = (SELECT @@IDENTITY);
                END

                IF NOT EXISTS
                (
                    SELECT [Id]
                    FROM [GroupMemberAssignment]
                    WHERE [GroupMemberId] = @GroupMemberId AND [LocationId] = @LocationId AND [ScheduleId] = @ScheduleId
                )
                BEGIN
                    INSERT INTO [GroupMemberAssignment]
                    (
                        [GroupId]
                        , [GroupMemberId]
                        , [LocationId]
                        , [ScheduleId]
                        , [CreatedDateTime]
                        , [ModifiedDateTime]
                        , [CreatedByPersonAliasId]
                        , [ModifiedByPersonAliasId]
                        , [Guid]
                    )
                    VALUES
                    (
                        @GroupId
                        , @GroupMemberId
                        , @LocationId
                        , @ScheduleId
                        , @Now
                        , @Now
                        , @PersonAliasId_Admin
                        , @PersonAliasId_Admin
                        , NEWID()
                    );
                END

                SET @iMember = @iMember + 1;
            END

            SET @iOpportunity = @iOpportunity + 1;
        END
    END
END