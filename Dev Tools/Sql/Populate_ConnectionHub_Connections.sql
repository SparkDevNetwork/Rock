-- Run the populate plethora of persons script before running this.

-- Set to 0 after your first run.
DECLARE @MaxCampusesPerOpportunity INT = 3;
-- Set to 0 after your first run.
DECLARE @ConnectorCount INT = 10;

-- Connection Request variables
-- Requests per opportunity
DECLARE @MinRequests INT = 20;
DECLARE @MaxRequests INT = 35;
DECLARE @CreatedDateRangeInDays INT = 20;

/*
    Change this value to control how often a connector is assigned.
    Example: 0.5 = 50 percent of requests get a connector
*/
DECLARE @ConnectorAssignmentRate FLOAT = 0.9;
DECLARE @CampusAssignmentRate FLOAT = 0.7;
DECLARE @GroupAssignmentRate FLOAT = 0.7;

-----------------------
-- CONNECTION TYPES
-----------------------
DECLARE @Now DATETIME = GETDATE();

-- CARE
IF NOT EXISTS ( SELECT 1 FROM ConnectionType WHERE Guid = 'A8F8C5E3-7B55-4F5D-9A9E-6C7B7A9F1C01' )
BEGIN

	UPDATE ConnectionType
	SET
		DueDateCalculationMode = 0,          -- FixedDaysFromStartTypeLevel
		RequestDueDateOffsetInDays = 21,      -- Due in 21 days
		RequestDueSoonOffsetInDays = 5,       -- Due soon at 5 days
		EnabledFeatures = 5,                  -- Reminder | GroupPlacement
		EnabledViews = 27,                    -- List | Board | Snapshot | Analytics
		ModifiedDateTime = GETDATE()
	WHERE Guid = 'DD565087-A4BE-4943-B123-BF22777E8426';

    INSERT INTO ConnectionType (
        Name,
        Description,
        EnableFutureFollowup,
        EnableFullActivityList,
        IsActive,
        DueDateCalculationMode,
        EnabledFeatures,
        EnabledViews,
        Guid,
        CreatedDateTime,
        ModifiedDateTime,
		IconCssClass
    )
    VALUES (
        'Care',
        'Pastoral care, benevolence, and personal support requests.',
        1,
        1,
        1,
        2,      -- DurationPerStatus
        3,      -- Reminder | Celebration
        11,     -- List | Board | Snapshot
        'A8F8C5E3-7B55-4F5D-9A9E-6C7B7A9F1C01',
        @Now,
        @Now,
		'ti ti-heart-handshake'
    );
END

-- NEXT STEPS
IF NOT EXISTS ( SELECT 1 FROM ConnectionType WHERE Guid = 'B4D1C0A2-2E9F-4B6A-8E7C-3C9E8A5E4D02' )
BEGIN
    INSERT INTO ConnectionType (
        Name,
        Description,
        EnableFutureFollowup,
        EnableFullActivityList,
        IsActive,
        DueDateCalculationMode,
        RequestDueDateOffsetInDays,
        RequestDueSoonOffsetInDays,
        EnabledFeatures,
        EnabledViews,
        Guid,
        CreatedDateTime,
        ModifiedDateTime,
		IconCssClass
    )
    VALUES (
        'Next Steps',
        'Spiritual and assimilation next steps for individuals.',
        1,
        1,
        1,
        0,      -- FixedDaysFromStartTypeLevel
        14,     -- Due in 14 days
        3,      -- Due soon at 3 days
        1,      -- Reminder
        13,     -- List | Grid | Snapshot
        'B4D1C0A2-2E9F-4B6A-8E7C-3C9E8A5E4D02',
        @Now,
        @Now,
		'ti ti-arrow-right-circle'
    );
END

-- SUPPORT
IF NOT EXISTS ( SELECT 1 FROM ConnectionType WHERE Guid = 'C9E2F1B7-8D4A-4C3F-9B6A-1E7D5F8A3C03' )
BEGIN
    INSERT INTO ConnectionType (
        Name,
        Description,
        EnableFutureFollowup,
        EnableFullActivityList,
        IsActive,
        DueDateCalculationMode,
        EnabledFeatures,
        EnabledViews,
        Guid,
        CreatedDateTime,
        ModifiedDateTime,
		IconCssClass
    )
    VALUES (
        'Support',
        'Operational and practical help requests.',
        1,
        1,
        1,
        1,      -- FixedDaysFromStartOpportunityLevel
        5,      -- Reminder | GroupPlacement
        25,     -- List | Snapshot | Analytics
        'C9E2F1B7-8D4A-4C3F-9B6A-1E7D5F8A3C03',
        @Now,
        @Now,
		'ti ti-lifebuoy'
    );
END

-----------------------
-- CONNECTION TYPE SOURCES
-----------------------

INSERT INTO [dbo].[ConnectionTypeSource]
(
    [Name],
    [ConnectionTypeId],
    [CreatedDateTime],
    [ModifiedDateTime],
    [Guid]
)
SELECT
    src.[Name],
    ct.[Id] AS [ConnectionTypeId],
    @Now,
    @Now,
    NEWID()
FROM [dbo].[ConnectionType] ct
CROSS JOIN
(
    SELECT N'Connection Card' AS [Name]
    UNION ALL
    SELECT N'Website'
) src
WHERE NOT EXISTS
(
    SELECT 1
    FROM [dbo].[ConnectionTypeSource] cts
    WHERE
        cts.ConnectionTypeId = ct.Id
        AND cts.[Name] = src.[Name]
);

-----------------------
-- CONNECTION STATUSES
-----------------------

DECLARE 
    @CareTypeId INT = ( SELECT Id FROM ConnectionType WHERE Guid = 'A8F8C5E3-7B55-4F5D-9A9E-6C7B7A9F1C01' ),
    @NextStepsTypeId INT = ( SELECT Id FROM ConnectionType WHERE Guid = 'B4D1C0A2-2E9F-4B6A-8E7C-3C9E8A5E4D02' ),
    @SupportTypeId INT = ( SELECT Id FROM ConnectionType WHERE Guid = 'C9E2F1B7-8D4A-4C3F-9B6A-1E7D5F8A3C03' );

-----------------------
-- CARE (DurationPerStatus)
-----------------------

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'No Contact' AND ConnectionTypeId = @CareTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    RequestStatusDueDateOffsetInDays, RequestStatusDueSoonOffsetInDays,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'No Contact', @CareTypeId, 1, 1, 1,
    3, 1,
    0, NEWID(), @Now, @Now, '#F43F5E'
);

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'In Progress' AND ConnectionTypeId = @CareTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    RequestStatusDueDateOffsetInDays, RequestStatusDueSoonOffsetInDays,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'In Progress', @CareTypeId, 1, 0, 1,
    7, 2,
    1, NEWID(), @Now, @Now, '#84CC16'
);

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'Completed' AND ConnectionTypeId = @CareTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    AutoFutureFollowUpPauseInDays,
    IsNoteRequiredOnCompletion,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'Completed', @CareTypeId, 0, 0, 1,
    30,
    1,
    2, NEWID(), @Now, @Now, '#0EA5E9'
);

-----------------------
-- NEXT STEPS (Type-level due dates)
-----------------------

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'New' AND ConnectionTypeId = @NextStepsTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'New', @NextStepsTypeId, 1, 1, 1,
    0, NEWID(), @Now, @Now, '#F43F5E'
);

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'Contacted' AND ConnectionTypeId = @NextStepsTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'Contacted', @NextStepsTypeId, 1, 0, 1,
    1, NEWID(), @Now, @Now, '#84CC16'
);

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'Completed' AND ConnectionTypeId = @NextStepsTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    AutoFutureFollowUpPauseInDays,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'Completed', @NextStepsTypeId, 0, 0, 1,
    14,
    2, NEWID(), @Now, @Now, '#0EA5E9'
);

-----------------------
-- SUPPORT (Opportunity-level due dates)
-----------------------

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'Open' AND ConnectionTypeId = @SupportTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'Open', @SupportTypeId, 1, 1, 1,
    0, NEWID(), @Now, @Now, '#F43F5E'
);

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'Assigned' AND ConnectionTypeId = @SupportTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'Assigned', @SupportTypeId, 1, 0, 1,
    1, NEWID(), @Now, @Now, '#84CC16'
);

IF NOT EXISTS ( SELECT 1 FROM ConnectionStatus WHERE Name = 'Resolved' AND ConnectionTypeId = @SupportTypeId )
INSERT INTO ConnectionStatus (
    Name, ConnectionTypeId, IsCritical, IsDefault, IsActive,
    [Order], Guid, CreatedDateTime, ModifiedDateTime, HighlightColor
)
VALUES (
    'Resolved', @SupportTypeId, 0, 0, 1,
    2, NEWID(), @Now, @Now, '#0EA5E9'
);

-----------------------
-- CONNECTION OPPORTUNITIES
-----------------------

DECLARE @CareOps TABLE (
    Name NVARCHAR(50),
    Summary NVARCHAR(255),
    IconCssClass NVARCHAR(100),
    Ord INT,
    Guid UNIQUEIDENTIFIER
);

INSERT INTO @CareOps VALUES
('Prayer Request',       'Receive prayer and spiritual support from our care team.', 'ti ti-pray',        0, '5F4A7D91-8E53-4C8F-BE91-3E6A0C1D9B21'),
('Hospital Visit',       'Request a visit or follow-up during a hospital stay.',      'ti ti-hospital',    1, 'D3C9A6E2-7B48-4A52-9F77-0E2E98A4F4A3'),
('Benevolence Request',  'Get assistance with short-term financial or material needs.','ti ti-heart-handshake',  2, '9E5C42C8-41E7-4E7B-9C2B-7D9B1C5F6E14'),
('Counseling Follow-Up', 'Connect with someone for pastoral or counseling support.',   'ti ti-message-heart',3,'A2F8E3C9-6C5A-4D3A-9B1E-54E7F9A8C123'),
('Pastoral Check-In',    'Request a personal follow-up with a pastor or leader.',      'ti ti-user-heart',  4, '4C7E9B5A-3D12-4F7E-8A9B-2E6D8F3C1A77');

INSERT INTO ConnectionOpportunity (
    Name, PublicName, Summary, IconCssClass,
    ConnectionTypeId, IsActive,
    [Order], ShowConnectButton, Guid,
    CreatedDateTime, ModifiedDateTime
)
SELECT
    o.Name,
    o.Name,
    o.Summary,
    o.IconCssClass,
    @CareTypeId,
    1,
    o.Ord,
    1,
    o.Guid,
    @Now,
    @Now
FROM @CareOps o
WHERE NOT EXISTS (
    SELECT 1 FROM ConnectionOpportunity WHERE Guid = o.Guid
);

-----------------------
-- NEXT STEPS
-----------------------

DECLARE @NextOps TABLE (
    Name NVARCHAR(50),
    Summary NVARCHAR(255),
    IconCssClass NVARCHAR(100),
    Ord INT,
    Guid UNIQUEIDENTIFIER
);

INSERT INTO @NextOps VALUES
('Baptism',        'Take the next step of faith through baptism.',                'ti ti-droplet',     0, '7E6F2B19-4A2E-4E7C-9D5C-1C8B9F3D2E11'),
('Membership',     'Learn more about becoming a member of the church.',            'ti ti-id-badge',    1, '2C9D6A41-5F87-4E6B-8D3A-9F6E2C1A7B44'),
('Starting Point', 'Explore faith and church life in a safe, guided environment.', 'ti ti-compass',     2, 'B4E9C7A2-8D4A-4F6C-9A3E-5D7F2C1E8B09'),
('Meet a Pastor',  'Schedule a conversation with a member of the pastoral team.',  'ti ti-user-scan',   3, 'F1A9E6C2-3D7B-4A8F-9C5E-2B6D4E8A7C55');

INSERT INTO ConnectionOpportunity (
    Name, PublicName, Summary, IconCssClass,
    ConnectionTypeId, IsActive,
    [Order], ShowConnectButton, Guid,
    CreatedDateTime, ModifiedDateTime
)
SELECT
    o.Name,
    o.Name,
    o.Summary,
    o.IconCssClass,
    @NextStepsTypeId,
    1,
    o.Ord,
    1,
    o.Guid,
    @Now,
    @Now
FROM @NextOps o
WHERE NOT EXISTS (
    SELECT 1 FROM ConnectionOpportunity WHERE Guid = o.Guid
);

-----------------------
-- SUPPORT (Opportunity-level due dates)
-----------------------

DECLARE @SupportOps TABLE (
    Name NVARCHAR(50),
    Summary NVARCHAR(255),
    IconCssClass NVARCHAR(100),
    Ord INT,
    DueDays INT,
    SoonDays INT,
    Guid UNIQUEIDENTIFIER
);

INSERT INTO @SupportOps VALUES
('Facilities Help', 'Request help with building or room setup needs.',        'ti ti-building',     0,  5, 2, '6B2F9C4E-3A8D-4F5C-9E17-7A8C2D4B6E91'),
('Tech Support',    'Get assistance with audio, video, or technical issues.', 'ti ti-device-desktop',1, 3, 1, '8E3D4C7B-6A5F-4B9C-91E2-A7D6F8C2B1A4'),
('Transportation',  'Request help with transportation to or from events.',   'ti ti-bus',          2,  4, 1, '1A7E4C9B-5D6F-4E2A-8C91-3B6F2D7A8E44'),
('Meals Ministry',  'Coordinate meal support during times of need.',         'ti ti-soup',         3,  7, 2, '9F8E3A7D-4C5B-4E2F-8D91-6A7C2B5E4F33'),
('Childcare Help',  'Request childcare assistance for ministry or events.',  'ti ti-baby-carriage',4,  3, 1, 'C4A9E8D7-2B3F-4A6E-9C51-7F8D1B2E6A55'),
('Event Setup',     'Get help setting up or tearing down for events.',        'ti ti-tools',       5, 10, 3, '5D9F2A8E-6C4B-4E7D-91A3-8F7C1E2B6D99');

INSERT INTO ConnectionOpportunity (
    Name, PublicName, Summary, IconCssClass,
    ConnectionTypeId, IsActive,
    RequestDueDateOffsetInDays,
    RequestDueSoonOffsetInDays,
    [Order], ShowConnectButton,
    Guid, CreatedDateTime, ModifiedDateTime
)
SELECT
    o.Name,
    o.Name,
    o.Summary,
    o.IconCssClass,
    @SupportTypeId,
    1,
    o.DueDays,
    o.SoonDays,
    o.Ord,
    1,
    o.Guid,
    @Now,
    @Now
FROM @SupportOps o
WHERE NOT EXISTS (
    SELECT 1 FROM ConnectionOpportunity WHERE Guid = o.Guid
);

-------------------------------------
-- CONNECTION OPPORTUNITY CAMPUSES
-------------------------------------

;WITH CampusCounts AS (
    SELECT
        o.Id AS ConnectionOpportunityId,
        ABS(CHECKSUM(NEWID())) % (@MaxCampusesPerOpportunity + 1) AS CampusCount
    FROM ConnectionOpportunity o
),
OpportunityCampuses AS (
    SELECT
        cc.ConnectionOpportunityId,
        c.Id AS CampusId,
        ROW_NUMBER() OVER (
            PARTITION BY cc.ConnectionOpportunityId
            ORDER BY NEWID()
        ) AS CampusRow
    FROM CampusCounts cc
    JOIN Campus c
        ON 1 = 1
)
INSERT INTO ConnectionOpportunityCampus (
    ConnectionOpportunityId,
    CampusId,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    oc.ConnectionOpportunityId,
    oc.CampusId,
    NEWID(),
    @Now,
    @Now
FROM OpportunityCampuses oc
JOIN CampusCounts cc
    ON cc.ConnectionOpportunityId = oc.ConnectionOpportunityId
WHERE oc.CampusRow <= cc.CampusCount
  AND NOT EXISTS (
      SELECT 1
      FROM ConnectionOpportunityCampus existing
      WHERE existing.ConnectionOpportunityId = oc.ConnectionOpportunityId
        AND existing.CampusId = oc.CampusId
  );

-----------------------
-- GROUP TYPES
-----------------------

-- CARE TEAMS
IF NOT EXISTS ( SELECT 1 FROM GroupType WHERE Guid = '6A9E4E1C-8F31-4E8E-B0F6-91C7B56F6A41' )
INSERT INTO GroupType (
    IsSystem,
    Name,
    Description,
    GroupTerm,
    GroupMemberTerm,
    AllowMultipleLocations,
    ShowInGroupList,
    ShowInNavigation,
    TakesAttendance,
    AttendanceRule,
    AttendancePrintTo,
    [Order],
    LocationSelectionMode,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
VALUES (
    0,
    'Care Teams',
    'Teams that handle care-related connection requests.',
    'Team',
    'Care Team Member',
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    '6A9E4E1C-8F31-4E8E-B0F6-91C7B56F6A41',
    @Now,
    @Now
);

-- NEXT STEPS TEAMS
IF NOT EXISTS ( SELECT 1 FROM GroupType WHERE Guid = 'E3A6D2F4-1F7B-49E6-9B35-BC5E7D8F9A22' )
INSERT INTO GroupType (
    IsSystem,
    Name,
    Description,
    GroupTerm,
    GroupMemberTerm,
    AllowMultipleLocations,
    ShowInGroupList,
    ShowInNavigation,
    TakesAttendance,
    AttendanceRule,
    AttendancePrintTo,
    [Order],
    LocationSelectionMode,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
VALUES (
    0,
    'Next Steps Teams',
    'Teams that guide people through next steps.',
    'Team',
    'Guide',
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    'E3A6D2F4-1F7B-49E6-9B35-BC5E7D8F9A22',
    @Now,
    @Now
);

-- SUPPORT TEAMS
IF NOT EXISTS ( SELECT 1 FROM GroupType WHERE Guid = 'B1D7A9C8-4E32-4B94-8F71-3D2E9F6C4A55' )
INSERT INTO GroupType (
    IsSystem,
    Name,
    Description,
    GroupTerm,
    GroupMemberTerm,
    AllowMultipleLocations,
    ShowInGroupList,
    ShowInNavigation,
    TakesAttendance,
    AttendanceRule,
    AttendancePrintTo,
    [Order],
    LocationSelectionMode,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
VALUES (
    0,
    'Support Teams',
    'Teams that fulfill operational support requests.',
    'Team',
    'Volunteer',
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    0,
    'B1D7A9C8-4E32-4B94-8F71-3D2E9F6C4A55',
    @Now,
    @Now
);

-------------------------------------------------
-- GROUP TYPE ROLES
-------------------------------------------------

DECLARE
    @CareGroupTypeId INT = ( SELECT Id FROM GroupType WHERE Guid = '6A9E4E1C-8F31-4E8E-B0F6-91C7B56F6A41' ),
    @NextStepsGroupTypeId INT = ( SELECT Id FROM GroupType WHERE Guid = 'E3A6D2F4-1F7B-49E6-9B35-BC5E7D8F9A22' ),
    @SupportGroupTypeId INT = ( SELECT Id FROM GroupType WHERE Guid = 'B1D7A9C8-4E32-4B94-8F71-3D2E9F6C4A55' );

-------------------------------------------------
-- CARE TEAMS -> MEMBER ROLE
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM GroupTypeRole
    WHERE GroupTypeId = @CareGroupTypeId
      AND Name = 'Member'
)
INSERT INTO GroupTypeRole (
    IsSystem,
    GroupTypeId,
    Name,
    [Order],
    IsLeader,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
VALUES (
    0,
    @CareGroupTypeId,
    'Member',
    0,
    0,
    'A3F9C2E1-6D5B-4E7A-9C8F-2B1E5D6A7C01',
    @Now,
    @Now
);

-------------------------------------------------
-- NEXT STEPS TEAMS -> MEMBER ROLE
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM GroupTypeRole
    WHERE GroupTypeId = @NextStepsGroupTypeId
      AND Name = 'Member'
)
INSERT INTO GroupTypeRole (
    IsSystem,
    GroupTypeId,
    Name,
    [Order],
    IsLeader,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
VALUES (
    0,
    @NextStepsGroupTypeId,
    'Member',
    0,
    0,
    'B4D7A9E2-1C6F-4E8A-9F53-7C2D1E5A6B02',
    @Now,
    @Now
);

-------------------------------------------------
-- SUPPORT TEAMS -> MEMBER ROLE
-------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM GroupTypeRole
    WHERE GroupTypeId = @SupportGroupTypeId
      AND Name = 'Member'
)
INSERT INTO GroupTypeRole (
    IsSystem,
    GroupTypeId,
    Name,
    [Order],
    IsLeader,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
VALUES (
    0,
    @SupportGroupTypeId,
    'Member',
    0,
    0,
    'C8E1F4A6-9D5B-4E7A-8C2F-1A6D7B3E5C03',
    @Now,
    @Now
);

-----------------------
-- GROUPS
-----------------------

-------------------------------------------------
-- CARE GROUPS
-------------------------------------------------

DECLARE @CareGroups TABLE ( Name NVARCHAR(100), Ord INT, Guid UNIQUEIDENTIFIER );

INSERT INTO @CareGroups VALUES
('Prayer Request Team',       0, 'F1A7C92E-4D31-4F7E-8C92-1E6A9B7D2F11'),
('Hospital Visit Team',       1, 'A4D9F1B6-8E2C-4A71-9F6E-3C2B7E1A5D22'),
('Benevolence Request Team',  2, '9C3E6F1A-2B8D-4D7A-9E51-7F4A1B2C8D33'),
('Counseling Follow-Up Team', 3, 'B7F3E2C4-9A1D-4C8E-8F26-1A5D9B3C7E44'),
('Pastoral Check-In Team',    4, '12949B82-9977-446F-97D6-1F78F837FCF8');

INSERT INTO [Group] (
    IsSystem,
    GroupTypeId,
    Name,
    IsSecurityRole,
    IsActive,
    [Order],
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    0,
    @CareGroupTypeId,
    g.Name,
    0,
    1,
    g.Ord,
    g.Guid,
    @Now,
    @Now
FROM @CareGroups g
WHERE NOT EXISTS ( SELECT 1 FROM [Group] WHERE Guid = g.Guid );

-------------------------------------------------
-- NEXT STEPS GROUPS
-------------------------------------------------

DECLARE @NextGroups TABLE ( Name NVARCHAR(100), Ord INT, Guid UNIQUEIDENTIFIER );

INSERT INTO @NextGroups VALUES
('Baptism Team',        0, '2E8F7A1C-9D4B-4C6E-A7F2-3D1B5E9C4A66'),
('Membership Team',     1, '7C1A5D9E-2B8F-4E6A-9D3F-1E4C7B2A8F77'),
('Starting Point Team', 2, 'D6F9A8C1-5B2E-4A7F-8E93-1C2B4D7A5E88'),
('Meet a Pastor Team',  3, '813BB825-87EE-4EFD-AC4D-8C9D4BAD5605');

INSERT INTO [Group] (
    IsSystem,
    GroupTypeId,
    Name,
    IsSecurityRole,
    IsActive,
    [Order],
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    0,
    @NextStepsGroupTypeId,
    g.Name,
    0,
    1,
    g.Ord,
    g.Guid,
    @Now,
    @Now
FROM @NextGroups g
WHERE NOT EXISTS ( SELECT 1 FROM [Group] WHERE Guid = g.Guid );

-------------------------------------------------
-- SUPPORT GROUPS
-------------------------------------------------

DECLARE @SupportGroups TABLE ( Name NVARCHAR(100), Ord INT, Guid UNIQUEIDENTIFIER );

INSERT INTO @SupportGroups VALUES
('Facilities Help Team', 0, 'A9C2D8F1-4B6E-4E7A-9F3C-1D2B8A7E5F10'),
('Tech Support Team',    1, '1D5E5496-8DF5-481C-BE89-D0AAFF67CFD6'),
('Transportation Team',  2, 'C8A7F1D3-9B5E-4E2A-8F6C-1B2D4A7E9312'),
('Meals Ministry Team',  3, '4E7A2D9C-BF3A-4C8E-9F15-6D1B7A2E5C44'),
('Childcare Help Team',  4, 'D2E6A1F9-4C8B-4A7E-9F53-B7C1D5E8A663'),
('Event Setup Team',     5, '7F2B9E4C-A1D8-4E6F-9C35-2A1D7B8E5F77');

INSERT INTO [Group] (
    IsSystem,
    GroupTypeId,
    Name,
    IsSecurityRole,
    IsActive,
    [Order],
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    0,
    @SupportGroupTypeId,
    g.Name,
    0,
    1,
    g.Ord,
    g.Guid,
    @Now,
    @Now
FROM @SupportGroups g
WHERE NOT EXISTS ( SELECT 1 FROM [Group] WHERE Guid = g.Guid );


-------------------------------------------------
-- CONNECTION OPPORTUNITY GROUP CONFIG
-------------------------------------------------

DECLARE
    @CareMemberRoleId INT = (
        SELECT Id FROM GroupTypeRole
        WHERE GroupTypeId = ( SELECT Id FROM GroupType WHERE Guid = '6A9E4E1C-8F31-4E8E-B0F6-91C7B56F6A41' )
          AND Name = 'Member'
    ),
    @NextStepsMemberRoleId INT = (
        SELECT Id FROM GroupTypeRole
        WHERE GroupTypeId = ( SELECT Id FROM GroupType WHERE Guid = 'E3A6D2F4-1F7B-49E6-9B35-BC5E7D8F9A22' )
          AND Name = 'Member'
    ),
    @SupportMemberRoleId INT = (
        SELECT Id FROM GroupTypeRole
        WHERE GroupTypeId = ( SELECT Id FROM GroupType WHERE Guid = 'B1D7A9C8-4E32-4B94-8F71-3D2E9F6C4A55' )
          AND Name = 'Member'
    );

-------------------------------------------------
-- CARE OPPORTUNITIES
-------------------------------------------------

INSERT INTO ConnectionOpportunityGroupConfig (
    ConnectionOpportunityId,
    GroupTypeId,
    GroupMemberRoleId,
    GroupMemberStatus,
    UseAllGroupsOfType,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    o.Id,
    @CareGroupTypeId,
    @CareMemberRoleId,
    2,      -- Pending
    0,
    NEWID(),
    @Now,
    @Now
FROM ConnectionOpportunity o
WHERE o.ConnectionTypeId = @CareTypeId
  AND NOT EXISTS (
      SELECT 1
      FROM ConnectionOpportunityGroupConfig c
      WHERE c.ConnectionOpportunityId = o.Id
        AND c.GroupTypeId = @CareGroupTypeId
  );

-------------------------------------------------
-- NEXT STEPS OPPORTUNITIES
-------------------------------------------------

INSERT INTO ConnectionOpportunityGroupConfig (
    ConnectionOpportunityId,
    GroupTypeId,
    GroupMemberRoleId,
    GroupMemberStatus,
    UseAllGroupsOfType,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    o.Id,
    @NextStepsGroupTypeId,
    @NextStepsMemberRoleId,
    2,
    0,
    NEWID(),
    @Now,
    @Now
FROM ConnectionOpportunity o
WHERE o.ConnectionTypeId = @NextStepsTypeId
  AND NOT EXISTS (
      SELECT 1
      FROM ConnectionOpportunityGroupConfig c
      WHERE c.ConnectionOpportunityId = o.Id
        AND c.GroupTypeId = @NextStepsGroupTypeId
  );

-------------------------------------------------
-- SUPPORT OPPORTUNITIES
-------------------------------------------------

INSERT INTO ConnectionOpportunityGroupConfig (
    ConnectionOpportunityId,
    GroupTypeId,
    GroupMemberRoleId,
    GroupMemberStatus,
    UseAllGroupsOfType,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    o.Id,
    @SupportGroupTypeId,
    @SupportMemberRoleId,
    2,
    0,
    NEWID(),
    @Now,
    @Now
FROM ConnectionOpportunity o
WHERE o.ConnectionTypeId = @SupportTypeId
  AND NOT EXISTS (
      SELECT 1
      FROM ConnectionOpportunityGroupConfig c
      WHERE c.ConnectionOpportunityId = o.Id
        AND c.GroupTypeId = @SupportGroupTypeId
  );

-------------------------------------------------
-- CONNECTION OPPORTUNITY GROUPS
-------------------------------------------------

INSERT INTO ConnectionOpportunityGroup (
    ConnectionOpportunityId,
    GroupId,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    o.Id,
    g.Id,
    NEWID(),
    @Now,
    @Now
FROM ConnectionOpportunity o
JOIN [Group] g
    ON g.Name = o.Name + ' Team'
WHERE NOT EXISTS (
    SELECT 1
    FROM ConnectionOpportunityGroup og
    WHERE og.ConnectionOpportunityId = o.Id
      AND og.GroupId = g.Id
);

-------------------------------------------------
-- CONNECTION OPPORTUNITY CONNECTOR GROUPS
-------------------------------------------------

INSERT INTO ConnectionOpportunityConnectorGroup (
    ConnectionOpportunityId,
    ConnectorGroupId,
    Guid,
    CreatedDateTime,
    ModifiedDateTime
)
SELECT
    o.Id,
    g.Id,
    NEWID(),
    @Now,
    @Now
FROM ConnectionOpportunity o
CROSS JOIN [Group] g
WHERE g.Guid = '7A2AFC10-BCFA-4CCF-9B6C-C2367D6BE7BE'
  AND NOT EXISTS (
      SELECT 1
      FROM ConnectionOpportunityConnectorGroup cog
      WHERE cog.ConnectionOpportunityId = o.Id
        AND cog.ConnectorGroupId = g.Id
  );

-------------------------------------------------
-- INSERTING CONNECTORS
-------------------------------------------------

INSERT INTO GroupMember (
    IsSystem,
    GroupId,
    PersonId,
    GroupRoleId,
    GroupMemberStatus,
    Guid,
    CreatedDateTime,
    ModifiedDateTime,
    DateTimeAdded,
    GroupTypeId
)
SELECT TOP (@ConnectorCount)
    0,
    g.Id,
    p.Id,
    r.Id,
    1,          -- Active
    NEWID(),
    @Now,
    @Now,
    @Now,
    gt.Id
FROM Person p
CROSS JOIN [Group] g
JOIN GroupType gt
    ON gt.Id = g.GroupTypeId
JOIN GroupTypeRole r
    ON r.GroupTypeId = gt.Id
   AND r.Name = 'Member'
WHERE g.Guid = '7A2AFC10-BCFA-4CCF-9B6C-C2367D6BE7BE'
  AND NOT EXISTS (
      SELECT 1
      FROM GroupMember gm
      WHERE gm.GroupId = g.Id
        AND gm.PersonId = p.Id
  )
ORDER BY NEWID();

-------------------------------------------------
-- INSERTING CONNECTION REQUESTS
-------------------------------------------------

CREATE TABLE #InsertedRequests
(
    ConnectionRequestId INT NOT NULL,
    ConnectionOpportunityId INT NOT NULL,
    ConnectorPersonAliasId INT NULL,
	CreatedDateTime DATETIME NOT NULL,
    CreatedDateKey INT NOT NULL
);

/*
    You must set this to a valid ConnectionActivityTypeId
    Example: "Assigned" or similar
*/
DECLARE @AssignmentActivityTypeId INT =
(
    SELECT TOP 1 Id
    FROM ConnectionActivityType
    ORDER BY Id
);

;WITH ConnectorAliases AS (
    SELECT DISTINCT
        p.PrimaryAliasId AS PersonAliasId
    FROM [Group] g
    JOIN GroupMember gm
        ON gm.GroupId = g.Id
    JOIN Person p
        ON p.Id = gm.PersonId
    WHERE g.Guid = '7A2AFC10-BCFA-4CCF-9B6C-C2367D6BE7BE'
),
OpportunityCounts AS (
    SELECT
        o.Id AS ConnectionOpportunityId,
        o.ConnectionTypeId,
        ABS(CHECKSUM(NEWID())) % (@MaxRequests - @MinRequests + 1) + @MinRequests AS RequestCount
    FROM ConnectionOpportunity o
),
Numbers AS (
    SELECT TOP (50)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
    FROM sys.objects
),
Requests AS (
    SELECT
        oc.ConnectionOpportunityId,
        oc.ConnectionTypeId,
        ROW_NUMBER() OVER (
            PARTITION BY oc.ConnectionOpportunityId
            ORDER BY NEWID()
        ) AS RequestRow
    FROM OpportunityCounts oc
    JOIN Numbers n
        ON n.n < oc.RequestCount
),
Statuses AS (
    SELECT
        cs.Id AS ConnectionStatusId,
		cs.RequestStatusDueDateOffsetInDays,
		cs.RequestStatusDueSoonOffsetInDays,
        cs.ConnectionTypeId,
        ROW_NUMBER() OVER (
            PARTITION BY cs.ConnectionTypeId
            ORDER BY cs.[Order], cs.Id
        ) AS StatusRow
    FROM ConnectionStatus cs
    WHERE cs.IsActive = 1
)
INSERT INTO ConnectionRequest (
    ConnectionOpportunityId,
    ConnectionTypeId,
    PersonAliasId,
    ConnectorPersonAliasId,
	CampusId,
	AssignedGroupId,
    ConnectionStatusId,
    ConnectionState,
    Comments,
    CreatedDateTime,
    ModifiedDateTime,
    CreatedDateKey,
	ConnectionTypeSourceId,
    Guid,
	DueDate,
	DueSoonDate
)
OUTPUT
    INSERTED.Id,
    INSERTED.ConnectionOpportunityId,
    INSERTED.ConnectorPersonAliasId,
	INSERTED.CreatedDateTime,
	INSERTED.CreatedDateKey
INTO #InsertedRequests (
    ConnectionRequestId,
    ConnectionOpportunityId,
    ConnectorPersonAliasId,
	CreatedDateTime,
	CreatedDateKey
)
SELECT
    r.ConnectionOpportunityId,
    r.ConnectionTypeId,
    pa.Id,
    ca.PersonAliasId,
	campus.CampusId,
	grp.GroupId,
    s.ConnectionStatusId,
    0,
    'Connection request submitted.',
    dt.CreatedDateTime,
    dt.CreatedDateTime,
    CONVERT(INT, FORMAT(dt.CreatedDateTime, 'yyyyMMdd')),
	src.ConnectionTypeSourceId,
    NEWID(),
	DATEADD(DAY, offset.DueDateOffset, dt.CreatedDateTime) AS DueDate,
	DATEADD(
		DAY,
		offset.DueDateOffset - offset.DueSoonOffset,
		dt.CreatedDateTime
	) AS DueSoonDate
FROM Requests r
JOIN ConnectionType ct
    ON ct.Id = r.ConnectionTypeId
JOIN ConnectionOpportunity co
    ON co.Id = r.ConnectionOpportunityId
JOIN Statuses s
    ON s.ConnectionTypeId = r.ConnectionTypeId
   AND s.StatusRow = r.RequestRow
CROSS APPLY (
    SELECT TOP 1 Id
    FROM PersonAlias
    ORDER BY CHECKSUM(NEWID(), r.ConnectionOpportunityId, r.RequestRow)
) pa
CROSS APPLY (
    SELECT
        DueDateOffset =
            CASE ct.DueDateCalculationMode
                WHEN 0 THEN ct.RequestDueDateOffsetInDays
                WHEN 1 THEN co.RequestDueDateOffsetInDays
                WHEN 2 THEN s.RequestStatusDueDateOffsetInDays
            END,
        DueSoonOffset =
            CASE ct.DueDateCalculationMode
                WHEN 0 THEN ct.RequestDueSoonOffsetInDays
                WHEN 1 THEN co.RequestDueSoonOffsetInDays
                WHEN 2 THEN s.RequestStatusDueSoonOffsetInDays
            END
) offset
OUTER APPLY (
    SELECT
        CASE
            WHEN ABS(CHECKSUM(
                NEWID(),
                r.ConnectionOpportunityId,
                r.RequestRow
            )) / 2147483647.0 < @ConnectorAssignmentRate
            THEN (
                SELECT TOP 1 ca.PersonAliasId
                FROM ConnectorAliases ca
                ORDER BY CHECKSUM(
                    NEWID(),
                    r.ConnectionOpportunityId,
                    r.RequestRow,
                    ca.PersonAliasId
                )
            )
            ELSE NULL
        END AS PersonAliasId
) ca
OUTER APPLY (
    SELECT
        CASE
            WHEN ABS(CHECKSUM(
                NEWID(),
                r.ConnectionOpportunityId,
                r.RequestRow,
                'Campus'
            )) / 2147483647.0 < @CampusAssignmentRate
            THEN (
                SELECT TOP 1 coc.CampusId
                FROM ConnectionOpportunityCampus coc
                WHERE coc.ConnectionOpportunityId = r.ConnectionOpportunityId
                ORDER BY CHECKSUM(
                    NEWID(),
                    r.ConnectionOpportunityId,
                    r.RequestRow,
                    coc.CampusId
                )
            )
            ELSE NULL
        END AS CampusId
) campus
OUTER APPLY (
    SELECT
        CASE
            WHEN ABS(CHECKSUM(
                NEWID(),
                r.ConnectionOpportunityId,
                r.RequestRow,
                'Group'
            )) / 2147483647.0 < @GroupAssignmentRate
            THEN (
                SELECT TOP 1 cog.GroupId
                FROM ConnectionOpportunityGroup cog
                WHERE cog.ConnectionOpportunityId = r.ConnectionOpportunityId
                ORDER BY CHECKSUM(
                    NEWID(),
                    r.ConnectionOpportunityId,
                    r.RequestRow,
                    cog.GroupId
                )
            )
            ELSE NULL
        END AS GroupId
) grp
CROSS APPLY (
    SELECT
        DATEADD(
            MINUTE,
            ABS(CHECKSUM(
                NEWID(),
                r.ConnectionOpportunityId,
                r.RequestRow,
                'CreatedDate'
            )) % (@CreatedDateRangeInDays * 24 * 60),
            DATEADD(DAY, -@CreatedDateRangeInDays, @Now)
        ) AS CreatedDateTime
) dt
OUTER APPLY (
    SELECT TOP 1 cts.Id AS ConnectionTypeSourceId
    FROM ConnectionTypeSource cts
    WHERE cts.ConnectionTypeId = r.ConnectionTypeId
    ORDER BY CHECKSUM(
        NEWID(),
        r.ConnectionOpportunityId,
        r.RequestRow,
        cts.Id
    )
) src;


---------------------------------------------
-- INSERTING CONNECTION REQUEST ACTIVITIES
---------------------------------------------

INSERT INTO ConnectionRequestActivity (
    ConnectionRequestId,
    ConnectionActivityTypeId,
    ConnectorPersonAliasId,
    ConnectionOpportunityId,
    Note,
    CreatedDateTime,
    ModifiedDateTime,
    Guid
)
SELECT
    ir.ConnectionRequestId,
    @AssignmentActivityTypeId,
    ir.ConnectorPersonAliasId,
    ir.ConnectionOpportunityId,
    'Connector assigned automatically.',
    DATEADD(
        MINUTE,
        ABS(CHECKSUM(
            NEWID(),
            ir.ConnectionRequestId,
            'Activity'
        )) % 120 + 5,
        ir.CreatedDateTime
    ),
    DATEADD(
        MINUTE,
        ABS(CHECKSUM(
            NEWID(),
            ir.ConnectionRequestId,
            'ActivityMod'
        )) % 120 + 5,
        ir.CreatedDateTime
    ),
    NEWID()
FROM #InsertedRequests ir
WHERE ir.ConnectorPersonAliasId IS NOT NULL;

DROP TABLE #InsertedRequests;

/*
delete from GroupMember where GroupId = 60
delete from ConnectionStatus where Id > 2
delete from ConnectionRequestActivity
delete from ConnectionRequest where Id > 5
*/