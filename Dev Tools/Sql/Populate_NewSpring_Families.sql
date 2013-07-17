DECLARE @PersonRecordType int
SET @PersonRecordType = (SELECT id FROM DefinedValue WHERE guid = '36CF10D6-C695-413D-8E7C-4546EFEF385E')

DECLARE @ActiveRecordStatus int
SET @ActiveRecordStatus = (SELECT id FROM DefinedValue WHERE guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E')

DECLARE @FamilyGroupType int
SET @FamilyGroupType = (SELECT id FROM GroupType WHERE guid = '790E3215-3B10-442B-AF69-616C0DCB998E')

DECLARE @AdultRole int
SET @AdultRole = (SELECT id FROM GroupRole WHERE guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42')

DECLARE @ChildRole int
SET @ChildRole = (SELECT id FROM GroupRole WHERE guid = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9')

DECLARE @PrimaryPhone int
SET @PrimaryPhone = (SELECT id FROM DefinedValue WHERE guid = '407E7E45-7B2E-4FCD-9605-ECB1339F2453')

DECLARE @KnownRelationshipGroup int
SET @KnownRelationshipGroup = (SELECT id FROM [Group] WHERE [Name] = 'Relationships')

DECLARE @KnownRelationshipGroupRole int
SET @KnownRelationshipGroupRole = (SELECT id FROM GroupRole WHERE Guid = '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42')

DECLARE @LocationTypeValueHome int = (select id from DefinedValue where guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC')

DECLARE @GroupId int
DECLARE @PersonId int
DECLARE @LocationId int

-- Dubay Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '431B7918-447F-40F0-9719-A2DE67910B73')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Dubay Family', 0, 1, '431B7918-447F-40F0-9719-A2DE67910B73')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Richard', 'Dubay', 1, 1, 1974, 1, 'richard.dubay@newspring.cc', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '4787182289', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Heather', 'Dubay', 6, 4, 1974, 2, 'dtroftheking@gmail.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '4787182631', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Trae', 'Dubay', 19, 1, 2001, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Aiden', 'Dubay', 9, 3, 2004, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Dunagan Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = 'F6B1A753-C5E2-4A8E-8EF5-0379D456FDCA')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Dunagan Family', 0, 1, 'F6B1A753-C5E2-4A8E-8EF5-0379D456FDCA')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Chris', 'Dunagan', 6, 7, 1975, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Jessica', 'Dunagan', 22, 10, 1980, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Clinton', 'Dunagan', 16, 11, 2007, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Gabrielle', 'Dunagan', 27, 7, 2009, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Dylan', 'Dunagan', 23, 7, 2011, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Swift Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '47AAE4C1-AFA4-481D-8753-64F805DF1BE8')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Swift Family', 0, 1, '47AAE4C1-AFA4-481D-8753-64F805DF1BE8')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Micah', 'Swift', 18, 11, 1979, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Suzanne', 'Swift', 18, 9, 1982, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Emma Kate', 'Swift', 30, 1, 2007, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Addie', 'Swift', 26, 5, 2009, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Ella', 'Swift', 26, 5, 2009, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Trotter Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '421C9AF1-A789-48EB-A10F-15C1CCCEF5CD')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Trotter Family', 0, 1, '421C9AF1-A789-48EB-A10F-15C1CCCEF5CD')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Philip', 'Trotter', 11, 7, 1982, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Casey', 'Trotter', 3, 7, 1980, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Kingston', 'Trotter', 15, 6, 2012, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Barden Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '37288A64-2C3E-4979-AFCB-55886817928E')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Barden Family', 0, 1, '37288A64-2C3E-4979-AFCB-55886817928E')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Jared', 'Barden', 5, 5, 1982, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Jennifer', 'Barden', 14, 9, 1983, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Ellis', 'Barden', 22, 3, 2010, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Wyatt', 'Barden', 27, 12, 2011, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Thomas Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = 'E44770FC-D044-4787-A861-F29110024CC7')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Thomas Family', 0, 1, 'E44770FC-D044-4787-A861-F29110024CC7')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Ben', 'Thomas', 13, 6, 1978, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Skye', 'Thomas', 16, 6, 1978, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Benjamin', 'Thomas', 28, 12, 2009, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Weidenhammer Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '67210141-E697-46F5-A609-4B2B32D322F6')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Weidenhammer Family', 0, 1, '67210141-E697-46F5-A609-4B2B32D322F6')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Brian', 'Weidenhammer', 16, 12, 1982, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Katie', 'Weidenhammer', 22, 3, 1981, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Eli', 'Weidenhammer', 8, 10, 2011, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Emily', 'Weidenhammer', 18, 3, 2013, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Cox Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '616F25E9-8BEB-4614-9821-E47C82CC0DA3')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Cox Family', 0, 1, '616F25E9-8BEB-4614-9821-E47C82CC0DA3')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Trevor', 'Cox', 2, 6, 1974, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Crystal', 'Cox', 17, 1, 1976, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Jae', 'Cox', 3, 11, 2000, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Abby', 'Ledet', 10, 8, 2001, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Katelynn', 'Cox', 24, 10, 2003, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Craig Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '044939ED-E1CD-4FC5-9435-0A129F5D33B6')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Craig Family', 0, 1, '044939ED-E1CD-4FC5-9435-0A129F5D33B6')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Cliff', 'Craig', 12, 6, 1982, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Lauren', 'Craig', 17, 3, 1985, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Emerson', 'Craig', 21, 10, 2007, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'McKenna', 'Craig', 10, 9, 2009, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Sophia', 'Craig', 8, 6, 2012, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Cawthon Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '4AAA55EE-8E94-48C5-9BF9-41F4C4D1BA92')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Cawthon Family', 0, 1, '4AAA55EE-8E94-48C5-9BF9-41F4C4D1BA92')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Chris', 'Cawthon', 24, 8, 1972, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Cookie', 'Cawthon', 3, 7, 1973, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Carson', 'Cawthon', 17, 4, 2002, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Campbell', 'Cawthon', 13, 5, 2005, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Frist Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '23065FA7-FD9E-4386-B5C4-4F832C86CB9A')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Frist Family', 0, 1, '23065FA7-FD9E-4386-B5C4-4F832C86CB9A')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Josh', 'Frist', 20, 2, 1980, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Lindsay', 'Frist', 15, 8, 1982, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Barrett', 'Frist', 7, 1, 2012, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Frist Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = 'D37BDCC4-9F36-4660-A65B-11254F05EF37')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Frist Family', 0, 1, 'D37BDCC4-9F36-4660-A65B-11254F05EF37')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Howard', 'Frist', 12, 9, 1976, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Consie', 'Frist', 10, 11, 1978, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Christian', 'Frist', 26, 4, 1999, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Caroline', 'Frist', 29, 7, 2000, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Ryan', 'Frist', 18, 5, 2002, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Immanuel', 'Frist', 15, 7, 2003, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Charity', 'Frist', 4, 6, 2005, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Hydrick Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = 'B79627A3-2484-4A93-A607-1A7281F26081')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Hydrick Family', 0, 1, 'B79627A3-2484-4A93-A607-1A7281F26081')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Matt', 'Hydrick', 28, 11, 1976, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Ria', 'Hydrick', 30, 9, 1981, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'McCoy', 'Hydrick', 22, 3, 2009, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Micah', 'Hydrick', 30, 9, 2011, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Long Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '3EC7CD58-77AB-4AAE-A2CB-940EABD483EB')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Long Family', 0, 1, '3EC7CD58-77AB-4AAE-A2CB-940EABD483EB')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Brandon', 'Long', 31, 3, 1985, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Meagan', 'Long', 16, 5, 1985, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Ellie Claire', 'Long', 22, 5, 2012, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Bailey Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = 'E5CD0A47-EB6E-4592-B71E-F70DEA1D1756')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Bailey Family', 0, 1, 'E5CD0A47-EB6E-4592-B71E-F70DEA1D1756')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Matt', 'Bailey', 27, 4, 1981, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Katie', 'Bailey', 8, 10, 1978, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Cameron', 'Bailey', 20, 2, 2004, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Claire', 'Bailey', 8, 9, 2007, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Elle', 'Bailey', 12, 11, 2010, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Mullikin Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '7CA985F5-78D9-4987-B9D2-687BC4B6C5A9')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Mullikin Family', 0, 1, '7CA985F5-78D9-4987-B9D2-687BC4B6C5A9')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Michael', 'Mullikin', 16, 4, 1974, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Lori', 'Mullikin', 10, 8, 1975, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Ben', 'Mullikin', 25, 4, 2002, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Mark', 'Mullikin', 7, 7, 2004, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Sarah', 'Mullikin', 1, 3, 2010, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

-- Beaty Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = 'FF2E3045-5C37-4592-BB87-0DCF87907F73')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Beaty Family', 0, 1, 'FF2E3045-5C37-4592-BB87-0DCF87907F73')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Jake', 'Beaty', 4, 6, 1978, 1, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Suzanne', 'Beaty', 29, 6, 1977, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Peyton', 'Beaty', 14, 5, 2003, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Lauren', 'Beaty', 25, 3, 2005, 2, '', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @KnownRelationshipGroup, @PersonId, @KnownRelationshipGroupRole, newid())
