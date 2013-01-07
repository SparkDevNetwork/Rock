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

DECLARE @GroupId int
DECLARE @PersonId int

-- Turner Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = 'CCFAC929-8086-4A07-B7B3-81A23C6A5FC3')
IF @GroupId IS NOT NULL
BEGIN
	DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Turner Family', 0, 1, 'CCFAC929-8086-4A07-B7B3-81A23C6A5FC3')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'David', 'Turner', 30, 1, 1966, 1, 'david@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6234512120', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Jan', 'Turner', 8, 8, 1967, 2, 'jan@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6234513336', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Hannah', 'Turner', 2, 6, 1999, 2, 'hannah@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Sam', 'Turner', 26, 6, 2002, 1, 'sam@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())


-- Edmiston Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '4DCC3392-9CA6-4FA1-A59E-FF085E0750B4')
IF @GroupId IS NOT NULL
BEGIN
	DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid)
VALUES (0, @FamilyGroupType, 'Edmiston Family', 0, 1, '4DCC3392-9CA6-4FA1-A59E-FF085E0750B4')
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Jon', 'Edmiston', 10, 2, 1973, 1, 'jonedmiston@ccvonline.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6232982911', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Heidi', 'Edmiston', 9, 4, 1974, 2, 'heidi.edmiston@gmail.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid())
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6028191804', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Alex', 'Edmiston', 8, 8, 2002, 1, 'alex.edmiston@gmail.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Adam', 'Edmiston', 14, 1, 2004, 1, 'adam.edmiston@gmail.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid())
