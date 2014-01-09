SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_com_rocksoliddemochurch_spCreateSampleData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[_com_rocksoliddemochurch_spCreateSampleData]
GO

CREATE PROCEDURE [dbo].[_com_rocksoliddemochurch_spCreateSampleData]
AS
/**********************************************************************
* Description:	Creates the Rock Solid Demo Church data as per
*               https://github.com/SparkDevNetwork/Rock/wiki/z.-Rock-Solid-Demo-Church-Specification
* Created By:	Rock Team
* Date Created:	1/8/2014
*
* $Workfile: $
* $Revision: $ 
* $Header: $
* 
* $Log: $
**********************************************************************/

BEGIN TRY

   DECLARE 
    @i INT,
    @male INT = 1,
    @female INT = 2,
	@personRecordType INT = (SELECT Id FROM [DefinedValue] WHERE Guid = '36CF10D6-C695-413D-8E7C-4546EFEF385E'),
	@activeRecordStatus INT = (SELECT Id FROM [DefinedValue] WHERE Guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'),
	@primaryPhone INT = (SELECT Id FROM [DefinedValue] WHERE Guid = '407E7E45-7B2E-4FCD-9605-ECB1339F2453'),
	@personId INT,
	@firstName NVARCHAR(50),
	@lastName NVARCHAR(50),
	@email NVARCHAR(75),
	@phoneNumber DECIMAL,
    @todayDate DATETIME = GETDATE(),
	@todayDay INT = DATEPART( dd, GETDATE() ),
    @todayMonth INT = DATEPART( mm, GETDATE() ),
    @todayYear INT = DATEPART( yyyy, GETDATE() ),
    @nextMonth INT = DATEPART( mm, DATEADD( month, 1, GETDATE() ) ),
    @lastMonth INT = DATEPART( mm, DATEADD( month, -1, GETDATE() ) ),
        
	@year INT,
	@month INT,
	@day INT,

    @married INT = (SELECT Id FROM [DefinedValue] WHERE Guid = '5FE5A540-7D9F-433E-B47E-4229D1472248'),
    @single INT = (SELECT Id FROM [DefinedValue] WHERE Guid = 'F19FC180-FE8F-4B72-A59C-8013E3B0EB0D'),
    @familyGroupType INT = (SELECT Id FROM [GroupType] WHERE Guid = '790E3215-3B10-442B-AF69-616C0DCB998E'),
    @adultRole INT = (SELECT Id FROM [GroupTypeRole] WHERE Guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'),
    @childRole INT = (SELECT Id FROM [GroupTypeRole] WHERE Guid = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'),
    @groupId INT,
    @locationId INT,
    @locationTypeValueHome INT = (SELECT Id FROM [DefinedValue] WHERE Guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'),
    @streetAddress INT,
    @zipCode INT
    
    BEGIN TRANSACTION

		--IF SOMEERROR_CONDITION
		--BEGIN
		--  RAISERROR (15001,1,-1, 'assigning blank to blank');
		--END
		
		-- Decker family
		SET @groupId = (SELECT [Id] FROM [Group] WHERE Guid = '53A02527-C2A7-4F36-8585-71A85B8E4601')
		IF @groupId IS NOT NULL
		BEGIN
			DELETE [PhoneNumber] WHERE [PersonId] IN (SELECT [PersonId] FROM [GroupMember] WHERE [GroupId] = @groupId)	
			DELETE [FinancialTransaction] WHERE [AuthorizedPersonId] IN (SELECT PersonId FROM [GroupMember] WHERE [GroupId] = @groupId)    
			DELETE [Person] WHERE [Id] IN (SELECT [PersonId] FROM [GroupMember] WHERE [GroupId] = @groupId)
			DELETE [Group] WHERE [Id] = @groupId
			DELETE [GroupLocation] WHERE [Id] = @groupId
		END
		
		INSERT INTO [Group] (IsSystem, GroupTypeId, Name, IsSecurityRole, IsActive, Guid, [Order])
		VALUES (0, @FamilyGroupType, 'Decker Family', 0, 1, '53A02527-C2A7-4F36-8585-71A85B8E4601', 0)
		SET @groupId = SCOPE_IDENTITY()

			-- Decker home address
            INSERT INTO [Location] (Street1, Street2, City, [State], Zip, IsActive, [Guid], IsNamedLocation)
            VALUES ( '11624 N 31st Dr', '', 'Phoenix', 'AZ', '85029', 1, NEWID(), 0)
            SET @locationId = SCOPE_IDENTITY()

            INSERT INTO [GroupLocation] (GroupId, LocationId, GroupLocationTypeValueId, [Guid], IsMailingLocation, IsMappedLocation)
            VALUES (@groupId, @locationId, @locationTypeValueHome, NEWID(), 1, 0)     
        
        -- Decker family members
		INSERT INTO [Person] ([IsSystem],[FirstName],[NickName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId], [MaritalStatusValueId],[GivingGroupId])
		VALUES (0, 'Ted', 'Ted', 'Decker', 27, @lastMonth, @todayYear - 38, @male, 'ted@rocksoliddemochurch.com', 1, 0, NEWID(), @personRecordType, @activeRecordStatus, @married, @groupId)
		SET @personId = SCOPE_IDENTITY()
		INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
		VALUES (0, @groupId, @personId, @adultRole, newid(), 0)
		INSERT INTO [PhoneNumber] (IsSystem, PersonId, Number, IsMessagingEnabled, IsUnlisted, Guid, NumberTypeValueId)
		VALUES (0, @personId, '6235553322', 1, 0, newid(), @PrimaryPhone)

		INSERT INTO [Person] ([IsSystem],[FirstName],[NickName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId], [MaritalStatusValueId],[GivingGroupId])
		VALUES (0, 'Cindy', 'Cindy', 'Decker', 8, @lastMonth, @todayYear - 37, @female, 'cindy@rocksoliddemochurch.com', 1, 0, NEWID(), @personRecordType, @activeRecordStatus, @married,@groupId)
		SET @personId = SCOPE_IDENTITY()
		INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
		VALUES (0, @groupId, @personId, @adultRole, newid(), 0)

		INSERT INTO [Person] ([IsSystem],[FirstName],[NickName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
		VALUES (0, 'Noah', 'Noah', 'Decker', 26, @lastMonth, @todayYear - 10, @male, 'noah@rocksoliddemochurch.com', 1, 0, NEWID(), @personRecordType, @activeRecordStatus)
		SET @personId = SCOPE_IDENTITY()
		INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
		VALUES (0, @groupId, @personId, @childRole, newid(), 0)
		
		INSERT INTO [Person] ([IsSystem],[FirstName],[NickName],[LastName],[BirthDay],[BirthMonth],[BirthYear],[Gender],[Email],[IsEmailActive],[DoNotEmail],[Guid],[RecordTypeValueId],[RecordStatusValueId])
		VALUES (0, 'Alexis', 'Alexis', 'Decker', 2, @lastMonth, @todayYear - 7, @female, 'alexis@rocksoliddemochurch.com', 1, 0, NEWID(), @personRecordType, @activeRecordStatus)
		SET @personId = SCOPE_IDENTITY()
		INSERT INTO [GroupMember] (IsSystem, GroupId, PersonId, GroupRoleId, Guid, GroupMemberStatus)
		VALUES (0, @groupId, @personId, @childRole, newid(), 0)

		
    COMMIT TRAN -- transaction success!
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN --rollback in case of error

    RAISERROR (15000,1,-1, 'running [_com_rocksoliddemochurch_spCreateSampleData]');
END CATCH
