-- =====================================================================================================
-- Author:        Rock
-- Create Date:   08-23-24
-- Description:   Duplicates the current db's history table data (adds semi-random records first if necessary ).
--				  This script only handles 3 related entity types (see lines  331 to 347 and 416 to 435). 
--				  If more EntityTypes should be supported these lines will need to be updated accordingly.
--
-- Change History:
--                 
-- ======================================================================================================
-- TODO: Enter the # of times to double your History table size after the GO statement at the end of the script.
-- Entering 10 with no initial History rows would create 204,800 records. An additional run would result in 409,600 etc.

-- Variables for getting random data from current db.
DECLARE 
    @now DATETIMEOFFSET = SYSDATETIMEOFFSET(),
    @tenYears INT = 365 * 10,
	@maxAttributeId INT = (SELECT MAX(Id) FROM Attribute), 
	@maxGroupId INT = (SELECT MAX(Id) FROM [Group]), 
	@maxUserLoginId INT = (SELECT MAX(Id) FROM UserLogin),
	@maxPersonId INT = (SELECT MAX(Id) FROM Person),
	@maxGroupMemberId INT = (SELECT MAX(Id) FROM GroupMember),
	@maxPersonAliasId INT = (SELECT MAX(Id) FROM PersonAlias);

/* Ensure that are at least some rows in the history table. */
DECLARE @currentHistoryRowCount BIGINT = (SELECT COUNT_BIG(*) FROM History)
IF @currentHistoryRowCount < 50
BEGIN
	DECLARE @HistorySample TABLE (
		IsSystem BIT NOT NULL
		, CategoryGuid UNIQUEIDENTIFIER  NOT NULL
		, EntityTypeGuid UNIQUEIDENTIFIER  NOT NULL
		, Caption NVARCHAR(200)
		, RelatedEntityTypeGuid UNIQUEIDENTIFIER  NOT NULL
		, CreatedDateTime DATETIME
		, ModifiedDateTime DATETIME
		, CreatedByPersonAliasId INT
		, ModifiedByPersonAliasId INT
		, Verb NVARCHAR(50)
		, RelatedData NVARCHAR(MAX)
		, ChangeType NVARCHAR(20)
		, ValueName NVARCHAR(250)
		, NewValue NVARCHAR(MAX)
		, OldValue NVARCHAR(MAX)
		, IsSensitive BIT
		, SourceOfChange NVARCHAR(MAX)
		, NewRawValue NVARCHAR(250)
		, OldRawValue NVARCHAR(250)
	)

    -- Category Guids
    DECLARE 
        @activityCategoryGuid UNIQUEIDENTIFIER = '0836845E-5ED8-4ABE-8787-3B61EF2F0FA5',
        @groupChangesCategoryGuid UNIQUEIDENTIFIER = '089EB47D-D0EF-493E-B867-DC51BCDEF319',
        @groupMembershipCategoryGuid UNIQUEIDENTIFIER = '325278A4-FACA-4F38-A405-9C090B3BAA34',
        @familyChangesCategoryGuid UNIQUEIDENTIFIER = '5C4CCE5A-D7D0-492F-A241-96E13A3F7DF8';

    -- Entity Type Guids
    DECLARE 
        @personEntityTypeGuid UNIQUEIDENTIFIER = '72657ED8-D16E-492E-AC12-144C5E7567E7',
        @groupMemberEntityTypeGuid UNIQUEIDENTIFIER = '49668B95-FEDC-43DD-8085-D2B0D6343C48',
        @groupEntityTypeGuid UNIQUEIDENTIFIER = '9BBFDA11-0D22-40D5-902F-60ADFBC88987',
        @userLoginEntityTypeGuid UNIQUEIDENTIFIER = '0FA592F1-728C-4885-BE38-60ED6C0D834F';

	INSERT @HistorySample (
		IsSystem
		, CategoryGuid
		, EntityTypeGuid
		, Caption
		, RelatedEntityTypeGuid
		, CreatedDateTime
		, ModifiedDateTime
		, CreatedByPersonAliasId
		, ModifiedByPersonAliasId
		, Verb
		, RelatedData
		, ChangeType
		, ValueName
		, NewValue
		, OldValue
		, IsSensitive
		, SourceOfChange
		, NewRawValue
		, OldRawValue ) 
	VALUES
		(0, @activityCategoryGuid, @personEntityTypeGuid, 'Derch', @userLoginEntityTypeGuid, 'Aug 19 2024 4:02PM', 'Aug 19 2024 4:02PM', NULL, NULL, 'MODIFY', NULL, 'Property', 'Password', NULL, NULL, 1, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Norman Marquez', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', 'Norman Marquez', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', '''Known Relationship'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', '''Peer Network'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Norman Marquez', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', 'Norman Marquez', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024 12:52PM', 'Aug 19 2024 12:52PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', '''Known Relationship'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Lura Marquez', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', 'Lura Marquez', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Lura Marquez', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', 'Lura Marquez', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', '''Peer Network'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 19 2024  1:00PM', 'Aug 19 2024  1:00PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Rossie Muro', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', 'Rossie Muro', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', '''Known Relationship'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', '''Peer Network'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Rossie Muro', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', 'Rossie Muro', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024 11:47AM', 'Aug 20 2024 11:47AM', 10, NULL, 'MODIFY', NULL, 'Property', 'Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Yu Mulligan', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', 'Yu Mulligan', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', '''Known Relationship'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Known Relationship', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Known Relationship Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Yu Mulligan', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', 'Yu Mulligan', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'ADDEDTOGROUP', NULL, 'Record', '''Peer Network'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Role', 'Owner', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Peer Network', @groupEntityTypeGuid, 'Aug 20 2024  4:01PM', 'Aug 20 2024  4:01PM', 10, NULL, 'MODIFY', NULL, 'Property', 'Peer Network Communication Preference', 'Recipient Preference', NULL, 0, NULL, NULL, NULL), 
		(0, @activityCategoryGuid, @personEntityTypeGuid, 'nhjlara', @userLoginEntityTypeGuid, 'Jul 17 2024  3:13PM', 'Jul 17 2024  3:13PM', 200, 170, 'MODIFY', NULL, 'Property', 'Password', NULL, NULL, 1, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TX12', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 93, 110, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TX12 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Jeremiah Fraug', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 222, 144, 'REMOVEDFROMGROUP', NULL, 'Record', 'Jeremiah Fraug', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'George Weston', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 89, 193, 'REMOVEDFROMGROUP', NULL, 'Record', 'George Weston', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TX12', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 319, 110, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TX12 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Mariah Courson', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 174, 161, 'REMOVEDFROMGROUP', NULL, 'Record', 'Mariah Courson', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TX12', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 202, 57, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TX12 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Chelsea Flair', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 222, 152, 'REMOVEDFROMGROUP', NULL, 'Record', 'Chelsea Flair', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TX12', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 102, 339, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TX12 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Kara Travis', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 66, 131, 'REMOVEDFROMGROUP', NULL, 'Record', 'Kara Travis', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TX12', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 96, 116, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TX12 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Yasmine Georgetta', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 149, 262, 'REMOVEDFROMGROUP', NULL, 'Record', 'Yasmine Georgetta', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TX12', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 227, 229, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TX12 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Jeremiah Fraug', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 60, 315, 'REMOVEDFROMGROUP', NULL, 'Record', 'Jeremiah Fraug', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT 2010', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 243, 215, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT 2010 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT 2010', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 195, 372, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT 2010 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'George Weston', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 156, 177, 'REMOVEDFROMGROUP', NULL, 'Record', 'George Weston', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Mariah Courson', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 266, 281, 'REMOVEDFROMGROUP', NULL, 'Record', 'Mariah Courson', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT 2010', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 385, 170, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT 2010 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Chelsea Flair', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 176, 82, 'REMOVEDFROMGROUP', NULL, 'Record', 'Chelsea Flair', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT 2010', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 231, 97, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT 2010 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT 2010', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 96, 132, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT 2010 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Kara Travis', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 142, 316, 'REMOVEDFROMGROUP', NULL, 'Record', 'Kara Travis', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Yasmine Georgetta', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 119, 276, 'REMOVEDFROMGROUP', NULL, 'Record', 'Yasmine Georgetta', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT 2010', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 314, 139, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT 2010 Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Jeremiah Fraug', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 330, 213, 'REMOVEDFROMGROUP', NULL, 'Record', 'Jeremiah Fraug', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 30, 127, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'George Weston', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 119, 102, 'REMOVEDFROMGROUP', NULL, 'Record', 'George Weston', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 67, 162, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Mariah Courson', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 130, 176, 'REMOVEDFROMGROUP', NULL, 'Record', 'Mariah Courson', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 233, 149, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Chelsea Flair', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 199, 215, 'REMOVEDFROMGROUP', NULL, 'Record', 'Chelsea Flair', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 180, 34, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupChangesCategoryGuid, @groupMemberEntityTypeGuid, 'Kara Travis', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 151, 266, 'REMOVEDFROMGROUP', NULL, 'Record', 'Kara Travis', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'SUB - TMNT', @groupEntityTypeGuid, 'Jul 17 2024  3:00PM', 'Jul 17 2024  3:00PM', 241, 147, 'REMOVEDFROMGROUP', NULL, 'Record', 'SUB - TMNT Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Morning Church', @groupEntityTypeGuid, 'Jul 17 2024 12:53PM', 'Jul 17 2024 12:53PM', 215, 118, 'MODIFY', NULL, 'Property', 'Morning Church Status', 'Active', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Morning Church', @groupEntityTypeGuid, 'Jul 17 2024 12:53PM', 'Jul 17 2024 12:53PM', 145, 184, 'MODIFY', NULL, 'Property', 'Morning Church Role', 'Pending Member', NULL, 0, NULL, NULL, NULL), 
		(0, @groupMembershipCategoryGuid, @personEntityTypeGuid, 'Morning Church', @groupEntityTypeGuid, 'Jul 17 2024 12:53PM', 'Jul 17 2024 12:53PM', 88, 96, 'ADDEDTOGROUP', NULL, 'Record', '''Morning Church'' Group', NULL, NULL, 0, NULL, NULL, NULL), 
		(0, @activityCategoryGuid, @personEntityTypeGuid, 'Google_123143123123123124654762', @userLoginEntityTypeGuid, 'Jul 17 2024 12:52PM', 'Jul 17 2024 12:52PM', 176, 229, 'MODIFY', NULL, 'Property', 'Is Password Change Required', 'False', NULL, 0, NULL, NULL, NULL), 
		(0, @activityCategoryGuid, @personEntityTypeGuid, 'Google_123143123123123124654762', @userLoginEntityTypeGuid, 'Jul 17 2024 12:52PM', 'Jul 17 2024 12:52PM', 222, 278, 'MODIFY', NULL, 'Property', 'Is Confirmed', 'True', NULL, 0, NULL, NULL, NULL), 
		(0, @activityCategoryGuid, @personEntityTypeGuid, 'Google_123143123123123124654762', @userLoginEntityTypeGuid, 'Jul 17 2024 12:52PM', 'Jul 17 2024 12:52PM', 137, 124, 'MODIFY', NULL, 'Property', 'User Login', 'Google_123143123123123124654762', NULL, 0, NULL, NULL, NULL), 
		(0, @activityCategoryGuid, @personEntityTypeGuid, 'Google_123143123123123124654762', @userLoginEntityTypeGuid, 'Jul 17 2024 12:52PM', 'Jul 17 2024 12:52PM', 173, 81, 'ADD', NULL, 'Record', 'Authentication Provider', 'Google', NULL, 0, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Hair Family', @groupEntityTypeGuid, 'Feb  9 2014  2:04PM', 'Feb  9 2014  2:04PM', 153, 151, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Feb 10 2014  4:44PM', 'Feb 10 2014  4:44PM', 192, 179, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Rather Family', @groupEntityTypeGuid, 'Feb 10 2014 11:13PM', 'Feb 10 2014 11:13PM', 222, 77, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Tyler Family', @groupEntityTypeGuid, 'Feb 14 2014 10:38AM', 'Feb 14 2014 10:38AM', 244, 26, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Sorensen Family', @groupEntityTypeGuid, 'Feb 17 2014  4:14PM', 'Feb 17 2014  4:14PM', 208, 211, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Truman Family', @groupEntityTypeGuid, 'Feb 17 2014 10:09PM', 'Feb 17 2014 10:09PM', 131, 91, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Truman Family', @groupEntityTypeGuid, 'Feb 17 2014 10:09PM', 'Feb 17 2014 10:09PM', 153, 207, 'MODIFY', NULL, 'Property', 'Family', 'Truman Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Norst Family', @groupEntityTypeGuid, 'Feb 18 2014  7:07PM', 'Feb 18 2014  7:07PM', 100, 195, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Norst Family', @groupEntityTypeGuid, 'Feb 18 2014  7:07PM', 'Feb 18 2014  7:07PM', 118, 75, 'MODIFY', NULL, 'Property', 'Family', 'Norst Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Gourgia Family', @groupEntityTypeGuid, 'Feb 19 2014  8:45AM', 'Feb 19 2014  8:45AM', 171, 127, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Gourgia Family', @groupEntityTypeGuid, 'Feb 19 2014  8:45AM', 'Feb 19 2014  8:45AM', 201, 136, 'MODIFY', NULL, 'Property', 'Family', 'Gourgia Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Clyde Family', @groupEntityTypeGuid, 'Feb 20 2014  5:17PM', 'Feb 20 2014  5:17PM', 216, 292, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Clyde Family', @groupEntityTypeGuid, 'Feb 20 2014  5:17PM', 'Feb 20 2014  5:17PM', 108, 50, 'MODIFY', NULL, 'Property', 'Family', 'Clyde Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Estin Family', @groupEntityTypeGuid, 'Feb 20 2014  5:24PM', 'Feb 20 2014  5:24PM', 161, 96, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Estin Family', @groupEntityTypeGuid, 'Feb 20 2014  5:24PM', 'Feb 20 2014  5:24PM', 183, 86, 'MODIFY', NULL, 'Property', 'Family', 'Estin Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Higgins Family', @groupEntityTypeGuid, 'Feb 20 2014  5:24PM', 'Feb 20 2014  5:24PM', 234, 227, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Higgins Family', @groupEntityTypeGuid, 'Feb 20 2014  5:24PM', 'Feb 20 2014  5:24PM', 114, 144, 'MODIFY', NULL, 'Property', 'Family', 'Higgins Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Mortensen Family', @groupEntityTypeGuid, 'Feb 20 2014  5:25PM', 'Feb 20 2014  5:25PM', 158, 342, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Mortensen Family', @groupEntityTypeGuid, 'Feb 20 2014  5:25PM', 'Feb 20 2014  5:25PM', 128, 90, 'MODIFY', NULL, 'Property', 'Family', 'Mortensen Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Norman Family', @groupEntityTypeGuid, 'Feb 20 2014  5:26PM', 'Feb 20 2014  5:26PM', 129, 146, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Norman Family', @groupEntityTypeGuid, 'Feb 20 2014  5:26PM', 'Feb 20 2014  5:26PM', 108, 120, 'MODIFY', NULL, 'Property', 'Family', 'Norman Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Rather Family', @groupEntityTypeGuid, 'Feb 20 2014  5:37PM', 'Feb 20 2014  5:37PM', 130, 101, 'MODIFY', NULL, 'Property', 'Family Name', 'Rather Family', 'Rather Family', NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Rather Family', @groupEntityTypeGuid, 'Feb 20 2014  5:37PM', 'Feb 20 2014  5:37PM', 277, 293, 'MODIFY', NULL, 'Property', 'Campus', 'Main Campus', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Peterson Family', @groupEntityTypeGuid, 'Feb 20 2014  5:37PM', 'Feb 20 2014  5:37PM', 329, 180, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Peterson Family', @groupEntityTypeGuid, 'Feb 20 2014  5:37PM', 'Feb 20 2014  5:37PM', 127, 226, 'MODIFY', NULL, 'Property', 'Family', 'Peterson Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Klayst Family', @groupEntityTypeGuid, 'Feb 21 2014  2:14AM', 'Feb 21 2014  2:14AM', 79, 105, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Klayst Family', @groupEntityTypeGuid, 'Feb 21 2014  2:14AM', 'Feb 21 2014  2:14AM', 333, 175, 'MODIFY', NULL, 'Property', 'Family', 'Klayst Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Stirrup Family', @groupEntityTypeGuid, 'Feb 21 2014 10:19PM', 'Feb 21 2014 10:19PM', 129, 66, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Stirrup Family', @groupEntityTypeGuid, 'Feb 21 2014 10:19PM', 'Feb 21 2014 10:19PM', 144, 51, 'MODIFY', NULL, 'Property', 'Family', 'Stirrup Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Derch Family', @groupEntityTypeGuid, 'Feb 22 2014  4:39PM', 'Feb 22 2014  4:39PM', 187, 296, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Derch Family', @groupEntityTypeGuid, 'Feb 22 2014  4:39PM', 'Feb 22 2014  4:39PM', 202, 49, 'MODIFY', NULL, 'Property', 'Family', 'Derch Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Minor Family', @groupEntityTypeGuid, 'Feb 23 2014  2:41PM', 'Feb 23 2014  2:41PM', 168, 70, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Minor Family', @groupEntityTypeGuid, 'Feb 23 2014  2:41PM', 'Feb 23 2014  2:41PM', 91, 79, 'MODIFY', NULL, 'Property', 'Family', 'Minor Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Alna Family', @groupEntityTypeGuid, 'Feb 24 2014  3:16AM', 'Feb 24 2014  3:16AM', 195, 193, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Alna Family', @groupEntityTypeGuid, 'Feb 24 2014  3:16AM', 'Feb 24 2014  3:16AM', 59, 125, 'MODIFY', NULL, 'Property', 'Family', 'Alna Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Nesterly Family', @groupEntityTypeGuid, 'Feb 25 2014 10:24AM', 'Feb 25 2014 10:24AM', 208, 151, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Nesterly Family', @groupEntityTypeGuid, 'Feb 25 2014 10:24AM', 'Feb 25 2014 10:24AM', 402, 181, 'MODIFY', NULL, 'Property', 'Family', 'Nesterly Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Grimes Family', @groupEntityTypeGuid, 'Feb 26 2014  7:45PM', 'Feb 26 2014  7:45PM', 114, 136, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Grimes Family', @groupEntityTypeGuid, 'Feb 26 2014  7:45PM', 'Feb 26 2014  7:45PM', 60, 168, 'MODIFY', NULL, 'Property', 'Family', 'Grimes Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Truist Family', @groupEntityTypeGuid, 'Feb 28 2014 10:01PM', 'Feb 28 2014 10:01PM', 110, 88, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Truist Family', @groupEntityTypeGuid, 'Feb 28 2014 10:01PM', 'Feb 28 2014 10:01PM', 109, 269, 'MODIFY', NULL, 'Property', 'Family', 'Truist Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Riley Family', @groupEntityTypeGuid, 'Mar  3 2014  8:17PM', 'Mar  3 2014  8:17PM', 193, 76, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Riley Family', @groupEntityTypeGuid, 'Mar  3 2014  8:17PM', 'Mar  3 2014  8:17PM', 204, 219, 'MODIFY', NULL, 'Property', 'Family', 'Riley Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Riley Family', @groupEntityTypeGuid, 'Mar  3 2014  8:17PM', 'Mar  3 2014  8:17PM', 223, 224, 'MODIFY', NULL, 'Property', 'Campus', 'Main Campus', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Weeks Family', @groupEntityTypeGuid, 'Mar  3 2014  9:47PM', 'Mar  3 2014  9:47PM', 182, 104, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Weeks Family', @groupEntityTypeGuid, 'Mar  3 2014  9:47PM', 'Mar  3 2014  9:47PM', 233, 113, 'MODIFY', NULL, 'Property', 'Family', 'Weeks Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Noterly Family', @groupEntityTypeGuid, 'Mar  5 2014  4:03AM', 'Mar  5 2014  4:03AM', 308, 72, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Noterly Family', @groupEntityTypeGuid, 'Mar  5 2014  4:03AM', 'Mar  5 2014  4:03AM', 179, 139, 'MODIFY', NULL, 'Property', 'Family', 'Noterly Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Mar  5 2014  5:42AM', 'Mar  5 2014  5:42AM', 85, 59, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Mar  5 2014  5:42AM', 'Mar  5 2014  5:42AM', 191, 83, 'MODIFY', NULL, 'Property', 'Family', 'Edwards Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Wendy Family', @groupEntityTypeGuid, 'Mar  7 2014  8:32AM', 'Mar  7 2014  8:32AM', 244, 155, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Wendy Family', @groupEntityTypeGuid, 'Mar  7 2014  8:32AM', 'Mar  7 2014  8:32AM', 193, 90, 'MODIFY', NULL, 'Property', 'Family', 'Wendy Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Mar  9 2014  5:43AM', 'Mar  9 2014  5:43AM', 29, 409, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Mar  9 2014  5:43AM', 'Mar  9 2014  5:43AM', 220, 88, 'MODIFY', NULL, 'Property', 'Family', 'Edwards Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Mar  9 2014  5:46AM', 'Mar  9 2014  5:46AM', 100, 317, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Mar  9 2014  5:46AM', 'Mar  9 2014  5:46AM', 73, 273, 'MODIFY', NULL, 'Property', 'Family', 'Edwards Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Mar  9 2014  5:57AM', 'Mar  9 2014  5:57AM', 158, 250, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Edwards Family', @groupEntityTypeGuid, 'Mar  9 2014  5:57AM', 'Mar  9 2014  5:57AM', 44, 179, 'MODIFY', NULL, 'Property', 'Family', 'Edwards Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Dister Family', @groupEntityTypeGuid, 'Mar  9 2014  1:14PM', 'Mar  9 2014  1:14PM', 162, 16, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Dister Family', @groupEntityTypeGuid, 'Mar  9 2014  1:14PM', 'Mar  9 2014  1:14PM', 105, 173, 'MODIFY', NULL, 'Property', 'Family', 'Dister Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Dister Family', @groupEntityTypeGuid, 'Mar  9 2014  1:15PM', 'Mar  9 2014  1:15PM', 159, 22, 'MODIFY', NULL, 'Property', 'Family', NULL, 'Dister Family', NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Jittle Family', @groupEntityTypeGuid, 'Mar 10 2014  5:04AM', 'Mar 10 2014  5:04AM', 203, 202, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Jittle Family', @groupEntityTypeGuid, 'Mar 10 2014  5:04AM', 'Mar 10 2014  5:04AM', 150, 110, 'MODIFY', NULL, 'Property', 'Family', 'Jittle Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Fien Family', @groupEntityTypeGuid, 'Mar 10 2014  7:14AM', 'Mar 10 2014  7:14AM', 110, 120, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Fien Family', @groupEntityTypeGuid, 'Mar 10 2014  7:14AM', 'Mar 10 2014  7:14AM', 163, 105, 'MODIFY', NULL, 'Property', 'Family', 'Fien Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Klein Family', @groupEntityTypeGuid, 'Mar 10 2014  7:51AM', 'Mar 10 2014  7:51AM', 119, 187, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Klein Family', @groupEntityTypeGuid, 'Mar 10 2014  7:51AM', 'Mar 10 2014  7:51AM', 213, 244, 'MODIFY', NULL, 'Property', 'Family', 'Klein Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Lorne Family', @groupEntityTypeGuid, 'Mar 10 2014 10:37AM', 'Mar 10 2014 10:37AM', 229, 137, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Lorne Family', @groupEntityTypeGuid, 'Mar 10 2014 10:37AM', 'Mar 10 2014 10:37AM', 288, 134, 'MODIFY', NULL, 'Property', 'Family', 'Lorne Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Dorst Family', @groupEntityTypeGuid, 'Mar 10 2014 11:55AM', 'Mar 10 2014 11:55AM', 174, 253, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Dorst Family', @groupEntityTypeGuid, 'Mar 10 2014 11:55AM', 'Mar 10 2014 11:55AM', 264, 165, 'MODIFY', NULL, 'Property', 'Family', 'Dorst Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Looper Family', @groupEntityTypeGuid, 'Mar 10 2014 12:33PM', 'Mar 10 2014 12:33PM', 148, 177, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Looper Family', @groupEntityTypeGuid, 'Mar 10 2014 12:33PM', 'Mar 10 2014 12:33PM', 152, 139, 'MODIFY', NULL, 'Property', 'Family', 'Looper Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Strickland Family', @groupEntityTypeGuid, 'Mar 10 2014 12:49PM', 'Mar 10 2014 12:49PM', 45, 227, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Strickland Family', @groupEntityTypeGuid, 'Mar 10 2014 12:49PM', 'Mar 10 2014 12:49PM', 159, 219, 'MODIFY', NULL, 'Property', 'Family', 'Strickland Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Boarder Family', @groupEntityTypeGuid, 'Mar 10 2014  1:16PM', 'Mar 10 2014  1:16PM', 310, 251, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Boarder Family', @groupEntityTypeGuid, 'Mar 10 2014  1:16PM', 'Mar 10 2014  1:16PM', 147, 223, 'MODIFY', NULL, 'Property', 'Family', 'Boarder Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Hughes Family', @groupEntityTypeGuid, 'Mar 10 2014  1:22PM', 'Mar 10 2014  1:22PM', 137, 236, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Hughes Family', @groupEntityTypeGuid, 'Mar 10 2014  1:22PM', 'Mar 10 2014  1:22PM', 199, 36, 'MODIFY', NULL, 'Property', 'Family', 'Hughes Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Forst Family', @groupEntityTypeGuid, 'Mar 10 2014  1:31PM', 'Mar 10 2014  1:31PM', 140, 61, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Forst Family', @groupEntityTypeGuid, 'Mar 10 2014  1:31PM', 'Mar 10 2014  1:31PM', 88, 82, 'MODIFY', NULL, 'Property', 'Family', 'Forst Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Twinner Family', @groupEntityTypeGuid, 'Mar 10 2014  1:44PM', 'Mar 10 2014  1:44PM', 121, 27, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Twinner Family', @groupEntityTypeGuid, 'Mar 10 2014  1:44PM', 'Mar 10 2014  1:44PM', 193, 84, 'MODIFY', NULL, 'Property', 'Family', 'Twinner Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Rester Family', @groupEntityTypeGuid, 'Mar 10 2014  2:09PM', 'Mar 10 2014  2:09PM', 119, 19, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Rester Family', @groupEntityTypeGuid, 'Mar 10 2014  2:09PM', 'Mar 10 2014  2:09PM', 114, 273, 'MODIFY', NULL, 'Property', 'Family', 'Rester Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Cleveland Family', @groupEntityTypeGuid, 'Mar 10 2014  2:12PM', 'Mar 10 2014  2:12PM', 96, 145, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Cleveland Family', @groupEntityTypeGuid, 'Mar 10 2014  2:12PM', 'Mar 10 2014  2:12PM', 195, 204, 'MODIFY', NULL, 'Property', 'Family', 'Cleveland Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Moore Family', @groupEntityTypeGuid, 'Mar 10 2014  3:41PM', 'Mar 10 2014  3:41PM', 195, 120, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Moore Family', @groupEntityTypeGuid, 'Mar 10 2014  3:41PM', 'Mar 10 2014  3:41PM', 26, 47, 'MODIFY', NULL, 'Property', 'Family', 'Moore Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Adendorf Family', @groupEntityTypeGuid, 'Mar 10 2014  3:42PM', 'Mar 10 2014  3:42PM', 209, 195, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Adendorf Family', @groupEntityTypeGuid, 'Mar 10 2014  3:42PM', 'Mar 10 2014  3:42PM', 36, 19, 'MODIFY', NULL, 'Property', 'Family', 'Adendorf Family', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Story Family', @groupEntityTypeGuid, 'Mar 10 2014  3:48PM', 'Mar 10 2014  3:48PM', 64, 369, 'MODIFY', NULL, 'Property', 'Role', 'Adult', NULL, NULL, NULL, NULL, NULL), 
		(0, @familyChangesCategoryGuid, @personEntityTypeGuid, 'Story Family', @groupEntityTypeGuid, 'Mar 10 2014  3:48PM', 'Mar 10 2014  3:48PM', 225, 292, 'MODIFY', NULL, 'Property', 'Family', 'Story Family', NULL, NULL, NULL, NULL, NULL)

	INSERT History (
		IsSystem
		, CategoryId
		, EntityTypeId
		, EntityId
		, Caption
		, RelatedEntityTypeId
		, RelatedEntityId
		, CreatedDateTime
		, ModifiedDateTime
		, CreatedByPersonAliasId
		, ModifiedByPersonAliasId
		, Verb
		, RelatedData
		, ChangeType
		, ValueName
		, NewValue
		, OldValue
		, IsSensitive
		, SourceOfChange
		, NewRawValue
		, OldRawValue
		, [Guid])
	SELECT 
		s.IsSystem
		, c.Id
		, et.Id

		-- Get a valid EntityId from the current database.
		, CASE et.Name
			WHEN 'Rock.Model.GroupMember' THEN (
				SELECT TOP 1 Id
				FROM GroupMember
				ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxGroupMemberId)), 0, 1)
			)
			WHEN 'Rock.Model.Person' THEN (
				SELECT TOP 1 Id
				FROM Person
				ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxPersonId)), 0, 1)
			)
		END EntityId
		, Caption
		, rEt.Id
		-- Get a valid RelatedEntityId from the current database.
		, CASE rEt.Name
			WHEN 'Rock.Model.Attribute' THEN (
				SELECT TOP 1 Id
				FROM Attribute
				ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxAttributeId)), 0, 1)
			 )
			WHEN 'Rock.Model.Group' THEN (
				SELECT TOP 1 Id
				FROM [Group]
				ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxGroupId)), 0, 1)
			 )
			WHEN 'Rock.Model.UserLogin' THEN (
				SELECT TOP 1 Id
				FROM UserLogin
				ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxUserLoginId)), 0, 1)
			 )
		  END RelatedEntityId
	    , DATEADD(DAY, -FLOOR(RAND(CHECKSUM(NEWID())) * @tenYears), s.CreatedDateTime) CreatedDateTime
	    , DATEADD(DAY, -FLOOR(RAND(CHECKSUM(NEWID())) * @tenYears), s.ModifiedDateTime) ModifiedDateTime
		, (SELECT TOP 1 Id
						FROM PersonAlias
						ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxPersonAliasId)), 0, 1))
		, (SELECT TOP 1 Id
						FROM PersonAlias
						ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxPersonAliasId)), 0, 1))
		, Verb
		, RelatedData
		, ChangeType
		, ValueName
		, NewValue
		, OldValue
		, IsSensitive
		, SourceOfChange
		, NewRawValue
		, OldRawValue,
		NEWID()
	FROM @HistorySample s
	JOIN Category c ON c.[Guid] = s.CategoryGuid
	JOIN EntityType et on et.[Guid] = s.EntityTypeGuid
	JOIN EntityType rEt on ret.[Guid] = s.RelatedEntityTypeGuid
END

-- Duplicate all history records, but with random identifiers.
INSERT History (
	IsSystem
	, CategoryId
	, EntityTypeId
	, EntityId
	, Caption
	, RelatedEntityTypeId
	, RelatedEntityId
	, CreatedDateTime
	, ModifiedDateTime
	, CreatedByPersonAliasId
	, ModifiedByPersonAliasId
	, Verb
	, RelatedData
	, ChangeType
	, ValueName
	, NewValue
	, OldValue
	, IsSensitive
	, SourceOfChange
	, NewRawValue
	, OldRawValue
	, [Guid])
SELECT 
	s.IsSystem
	, c.Id
	, et.Id

	-- Get a valid EntityId from the current database.
	, CASE et.Name
		WHEN 'Rock.Model.GroupMember' THEN (
			SELECT TOP 1 Id
			FROM GroupMember
			ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxGroupMemberId)), 0, 1)
		)
		WHEN 'Rock.Model.Person' THEN (
			SELECT TOP 1 Id
			FROM Person
			ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxPersonId)), 0, 1)
		)
	END EntityId
	, Caption
	-- We only have code to handle these 3 types. If the related entity is something else ignore it rather than add incorrect data.
	, IIF(rEt.Name IN ('Rock.Model.Attribute', 'Rock.Model.Group', 'Rock.Model.UserLogin'), rEt.Id, NULL) RelatedEntityTypeId
	-- Get a valid RelatedEntityId from the current database.
	, CASE rEt.Name
		WHEN 'Rock.Model.Attribute' THEN (
			SELECT TOP 1 Id
			FROM Attribute
			ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxAttributeId)), 0, 1)
		 )
		WHEN 'Rock.Model.Group' THEN (
			SELECT TOP 1 Id
			FROM [Group]
			ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxGroupId)), 0, 1)
		 )
		WHEN 'Rock.Model.UserLogin' THEN (
			SELECT TOP 1 Id
			FROM UserLogin
			ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxUserLoginId)), 0, 1)
		 )
	  END RelatedEntityId
	    , DATEADD(DAY, -FLOOR(RAND(CHECKSUM(NEWID())) * @tenYears), @now) CreatedDateTime
	    , DATEADD(DAY, -FLOOR(RAND(CHECKSUM(NEWID())) * @tenYears), @now) ModifiedDateTime
	, (SELECT TOP 1 Id
					FROM PersonAlias
					ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxPersonAliasId)), 0, 1))
	, (SELECT TOP 1 Id
					FROM PersonAlias
					ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxPersonAliasId)), 0, 1))
	, Verb
	, RelatedData
	, ChangeType
	, ValueName
	, NewValue
	, OldValue
	, IsSensitive
	, SourceOfChange
	, NewRawValue
	, OldRawValue,
	NEWID()
FROM History s
JOIN Category c ON c.Id = s.CategoryId
JOIN EntityType et on et.Id = s.EntityTypeId
JOIN EntityType rEt on ret.Id = s.RelatedEntityTypeId

-- If you get a syntax error ignore it and run anyway. This will cause the procedure to be run # times.
GO 1