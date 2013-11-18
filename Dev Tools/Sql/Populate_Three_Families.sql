DECLARE @PersonRecordType int
SET @PersonRecordType = (SELECT id FROM DefinedValue WHERE guid = '36CF10D6-C695-413D-8E7C-4546EFEF385E')

DECLARE @ActiveRecordStatus int
SET @ActiveRecordStatus = (SELECT id FROM DefinedValue WHERE guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E')

DECLARE @FamilyGroupType int
SET @FamilyGroupType = (SELECT id FROM GroupType WHERE guid = '790E3215-3B10-442B-AF69-616C0DCB998E')

DECLARE @AdultRole int
SET @AdultRole = (SELECT id FROM GroupTypeRole WHERE guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42')

DECLARE @ChildRole int
SET @ChildRole = (SELECT id FROM GroupTypeRole WHERE guid = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9')

DECLARE @PrimaryPhone int
SET @PrimaryPhone = (SELECT id FROM DefinedValue WHERE guid = '407E7E45-7B2E-4FCD-9605-ECB1339F2453')

DECLARE @LocationTypeValueHome int = (select id from DefinedValue where guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC')

DECLARE @MaritalStatus int
SET @MaritalStatus = (select Id from DefinedValue where Guid = '5FE5A540-7D9F-433E-B47E-4229D1472248')


DECLARE @GroupId int
DECLARE @PersonId int
DECLARE @LocationId int

begin transaction

-- Turner Family
SET @GroupId = (SELECT [Id] FROM [Group] WHERE Guid = 'CCFAC929-8086-4A07-B7B3-81A23C6A5FC3')
IF @GroupId IS NOT NULL
BEGIN
    DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [FinancialTransaction] WHERE AuthorizedPersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)    
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
    DELETE [GroupLocation] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid, [Order])
VALUES (0, @FamilyGroupType, 'Turner Family', 0, 1, 'CCFAC929-8086-4A07-B7B3-81A23C6A5FC3', 0)
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId], [MaritalStatusValueId],[GivingGroupId])
VALUES (0, 'David', 'Turner', 30, 1, 1966, 1, 'david@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus, @MaritalStatus,@GroupId)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid(), 0)
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6234512120', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId], [MaritalStatusValueId],[GivingGroupId])
VALUES (0, 'Jan', 'Turner', 8, 8, 1967, 2, 'jan@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus, @MaritalStatus,@GroupId)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid(), 0)
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6234513336', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Hannah', 'Turner', 2, 6, 1999, 2, 'hannah@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid(), 0)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Sam', 'Turner', 26, 6, 2002, 1, 'sam@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid(), 0)

-- Baby's birthdate, 12 months from today, also useful for testing "birthday is today" features...
DECLARE @BabyBirthDate DateTime
SET @BabyBirthDate = DATEADD(month, -12, GetDate());

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Joey', 'Turner', DAY(@BabyBirthDate), MONTH(@BabyBirthDate), YEAR( @BabyBirthDate), 1, 'joey@azturners.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid(), 0)
	-- Set Joey with an "Infant" Ability Level
	DECLARE @AbilityLevelAttributeId int
	SET @AbilityLevelAttributeId = ( SELECT Id FROM [Attribute] WHERE Guid = '4abf0bf2-49ba-4363-9d85-ac48a0f7e92a' )
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Order], [Value], [Guid] )
	VALUES (0, @AbilityLevelAttributeId, @PersonId, 0, 'C4550426-ED87-4CB0-957E-C6E0BC96080F', NEWID() )

-- Edmiston Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = '4DCC3392-9CA6-4FA1-A59E-FF085E0750B4')
IF @GroupId IS NOT NULL
BEGIN
	DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [FinancialTransaction] WHERE AuthorizedPersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)    
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
    DELETE [GroupLocation] WHERE Id = @GroupId  
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid, [Order])
VALUES (0, @FamilyGroupType, 'Edmiston Family', 0, 1, '4DCC3392-9CA6-4FA1-A59E-FF085E0750B4', 0)
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId], [MaritalStatusValueId],[GivingGroupId])
VALUES (0, 'Jon', 'Edmiston', 10, 2, 1973, 1, 'jonedmiston@ccvonline.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus, @MaritalStatus, @GroupId)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid(), 0)
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6232982911', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId], [MaritalStatusValueId],[GivingGroupId])
VALUES (0, 'Heidi', 'Edmiston', 9, 4, 1974, 2, 'heidi.edmiston@gmail.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus, @MaritalStatus, @GroupId)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid(), 0)
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6028191804', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Alex', 'Edmiston', 8, 8, 2002, 1, 'alex.edmiston@gmail.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid(), 0)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Adam', 'Edmiston', 14, 1, 2004, 1, 'adam.edmiston@gmail.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid(), 0)

-- Peterson Family
SET @GroupId = (SELECT Id FROM [Group] WHERE Guid = 'B9813A13-A5B3-47E3-AF32-208D785287F4')
IF @GroupId IS NOT NULL
BEGIN
	DELETE [PhoneNumber] WHERE PersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)	
    DELETE [FinancialTransaction] WHERE AuthorizedPersonId IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)    
    DELETE [Person] WHERE Id IN (SELECT PersonId FROM [GroupMember] WHERE GroupId = @GroupId)
	DELETE [Group] WHERE Id = @GroupId
    DELETE [GroupLocation] WHERE Id = @GroupId
END
INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid, [Order])
VALUES (0, @FamilyGroupType, 'Peterson Family', 0, 1, 'B9813A13-A5B3-47E3-AF32-208D785287F4', 0)
SET @GroupId = SCOPE_IDENTITY()

INSERT INTO [Location] (Street1, Street2, City, [State], Zip, IsActive, Guid, IsNamedLocation)
VALUES ('6515 W Lariat Ln', '', 'Phoenix', 'AZ', '85083', 1, NEWID(), 0)
SET @LocationId = SCOPE_IDENTITY()

INSERT INTO [GroupLocation] (GroupId, LocationId, GroupLocationTypeValueId, Guid, IsMailingLocation, IsMappedLocation)
VALUES (@GroupId, @LocationId, @LocationTypeValueHome, NEWID(), 1, 1)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId], [MaritalStatusValueId],[GivingGroupId])
VALUES (0, 'Mike', 'Peterson', 5, 11, 1971, 1, 'mikepeterson@ccvonline.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus, @MaritalStatus, @GroupId)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid(), 0)
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6234442282', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId], [MaritalStatusValueId],[GivingGroupId])
VALUES (0, 'April', 'Peterson', 4, 4, 1974, 2, 'april@mikeapril.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus, @MaritalStatus, @GroupId)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @AdultRole, newid(), 0)
INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
VALUES (0, @PersonId, '6234442282', 1, 0, newid(), @PrimaryPhone)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
VALUES (0, 'Nicolas', 'Peterson', 7, 11, 1998, 1, 'nicolas@mikeapril.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid(), 0)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId],[GivingGroupId])
VALUES (0, 'Violet', 'Peterson', 22, 1, 2003, 2, 'violet@mikeapril.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus, @GroupId)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid(), 0)

INSERT INTO [Person] ([IsSystem],[GivenName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId],[GivingGroupId])
VALUES (0, 'Sven', 'Peterson', 19, 12, 2005, 1, 'sven@mikeapril.com', 1, 0, NEWID(), @PersonRecordType, @ActiveRecordStatus, @GroupId)
SET @PersonId = SCOPE_IDENTITY()
INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
VALUES (0, @GroupId, @PersonId, @ChildRole, newid(), 0)

commit transaction