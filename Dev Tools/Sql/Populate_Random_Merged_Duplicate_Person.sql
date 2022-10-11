-- =====================================================================================================
-- Author:      Carl Roske
-- Create Date: 10/11/2022
-- Description: Adds random duplicate person records and merges them into their matching person records.
--
--  To be used with Followings testing.
--
-- ##Special Note##: Run the RockCleanup immediately after running this script so that any inconsistencies on Group or Person can get cleaned up
-- Change History:
--   
-- =====================================================================================================

SET NOCOUNT ON

DECLARE @maxPerson INT = 1000
    ,@genderInt INT
    ,@countryCode nvarchar(3) = null
    ,@personRecordType INT = (
        SELECT id
        FROM DefinedValue
        WHERE guid = '36CF10D6-C695-413D-8E7C-4546EFEF385E'
        )
    ,@recordStatusDefinedTypeId INT = (
        SELECT id
        FROM DefinedType
        WHERE guid = '8522BADD-2871-45A5-81DD-C76DA07E2E7E'
        )
    ,@homePhone INT = (
        SELECT id
        FROM DefinedValue
        WHERE guid = 'AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303'
        )
    ,@mobilePhone int = (SELECT id
        FROM DefinedValue
        WHERE guid = '407E7E45-7B2E-4FCD-9605-ECB1339F2453')
    ,@maritalStatusMarried INT = (
        SELECT id
        FROM DefinedValue
        WHERE guid = '5FE5A540-7D9F-433E-B47E-4229D1472248'
        )
	,@maritalStatusSingle INT = (
        SELECT id
        FROM DefinedValue
        WHERE guid = 'F19FC180-FE8F-4B72-A59C-8013E3B0EB0D'
        )
    ,@connectionStatusDefinedTypeId INT = (
        SELECT id
        FROM DefinedType
        WHERE guid = '2E6540EA-63F0-40FE-BE50-F2A84735E600'
    )
    ,@personId INT
    ,@personGuid UNIQUEIDENTIFIER
    ,@spousePersonId INT
    ,@spousePersonGuid UNIQUEIDENTIFIER
    ,@firstName NVARCHAR(50)
    ,@lastName NVARCHAR(50)
    ,@email NVARCHAR(75)
    ,@phoneNumber NVARCHAR(20)
    ,@phoneNumberFormatted NVARCHAR(50)
    ,@adultBirthYear INT
	,@childBirthYear INT
    ,@month INT
    ,@day INT
    ,@personCounter INT = 0
	,@kidCountMax int
	,@kidCounter int
	,@kidPersonId int
	,@kidPersonGuid uniqueidentifier
    ,@familyGroupTypeId INT = (
        SELECT id
        FROM GroupType
        WHERE guid = '790E3215-3B10-442B-AF69-616C0DCB998E'
        )
    ,@adultRole INT = (
        SELECT id
        FROM GroupTypeRole
        WHERE guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
        )
	,@childRole INT = (
        SELECT id
        FROM GroupTypeRole
        WHERE guid = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'
        )
    ,@groupId INT
    ,@locationId INT
    ,@locationTypeValueHome INT = (
        SELECT id
        FROM DefinedValue
        WHERE guid = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
        )
    ,@streetAddress INT
    ,@zipCode INT
	,@geoPoint Geography

	    BEGIN TRANSACTION
BEGIN

    DECLARE @connectionStatusValueId INT;
    DECLARE @recordStatusValueId INT;
    DECLARE @ageClassificationAdult INT = 1;
    DECLARE @ageClassificationChild INT = 2;
	DECLARE @clonedPersonId INT;

    WHILE @personCounter < @maxPerson
    BEGIN
        -- get a random person
		SELECT TOP 1 @personId = Id,
		@firstName = FirstName, 
		@lastName = LastName,
		@email = Email,
		@adultBirthYear = BirthYear,
		@month = BirthMonth,
		@day = BirthDay,
		@genderInt = Gender
		FROM Person ORDER BY NEWID();

        SET @personGuid = NEWID();

		SET @connectionStatusValueId = (select top 1 id from DefinedValue where DefinedTypeId = @connectionStatusDefinedTypeId order by NEWID())
        SET @recordStatusValueId = (select top 1 id from DefinedValue where DefinedTypeId = @recordStatusDefinedTypeId order by NEWID())

        INSERT INTO [Person] (
            [IsSystem]
            ,[FirstName]
            ,[NickName]
            ,[LastName]
            ,[BirthDay]
            ,[BirthMonth]
            ,[BirthYear]
			,[MaritalStatusValueId]
            ,[Gender]
            ,[AgeClassification]
            ,[Email]
            ,[IsEmailActive]
            ,[EmailPreference]
            ,[Guid]
            ,[RecordTypeValueId]
            ,[RecordStatusValueId]
			,[ConnectionStatusValueId]
			,[IsDeceased]
            ,[CreatedDateTime]
            )
        VALUES (
            0
            ,@firstName
            ,@firstName
            ,@lastName
            ,@day
            ,@month
            ,@adultBirthYear
			,@maritalStatusMarried
            ,@genderInt
            ,@ageClassificationAdult
            ,@email
            ,1
            ,0
            ,@personGuid
            ,@personRecordType
            ,@recordStatusValueId
			,@connectionStatusValueId
			,0
            ,SYSDATETIME()
            )

        SET @clonedPersonId = SCOPE_IDENTITY()

        INSERT INTO [PersonAlias] (
            PersonId
            ,AliasPersonId
            ,AliasPersonGuid
            ,[Guid]
            )
        VALUES (
            @clonedPersonId
            ,@clonedPersonId
            ,@personGuid
            ,NEWID()
            );

         IF (@clonedPersonId%10 = 0) BEGIN
            DECLARE @userName NVARCHAR(255) = concat(@lastName, substring(@firstName, 1, 2));
            IF NOT EXISTS (SELECT * FROM UserLogin WHERE UserName = @userName) BEGIN
                INSERT INTO [UserLogin] (UserName, [Password], [EntityTypeId], [PersonId], [Guid]) 
                    VALUES (@userName, NEWID(),27, @clonedPersonId, NEWID())
            END
         END

        SET @phoneNumber = cast(convert(BIGINT, ROUND(rand() * 0095551212, 0) + 6230000000) AS NVARCHAR(20));
        SET @phoneNumberFormatted = '(' + substring(@phoneNumber, 1, 3) + ') ' + substring(@phoneNumber, 4, 3) + '-' + substring(@phoneNumber, 7, 4);

        INSERT INTO [PhoneNumber] (
            IsSystem
            ,PersonId
            ,CountryCode
            ,Number
            ,FullNumber
            ,NumberFormatted
            ,IsMessagingEnabled
            ,IsUnlisted
            ,[Guid]
            ,NumberTypeValueId
            )
        VALUES (
            0
            ,@clonedPersonId
            ,@countryCode
            ,@phoneNumber
            ,concat(@countryCode, @phoneNumber)
            ,@phoneNumberFormatted
            ,0
            ,0
            ,newid()
            ,@homePhone
            );

        SET @phoneNumber = cast(convert(BIGINT, ROUND(rand() * 0095551212, 0) + 6230000000) AS NVARCHAR(20));
        SET @phoneNumberFormatted = '(' + substring(@phoneNumber, 1, 3) + ') ' + substring(@phoneNumber, 4, 3) + '-' + substring(@phoneNumber, 7, 4);

        INSERT INTO [PhoneNumber] (
            IsSystem
            ,PersonId
            ,CountryCode
            ,Number
            ,FullNumber
            ,NumberFormatted
            ,IsMessagingEnabled
            ,IsUnlisted
            ,[Guid]
            ,NumberTypeValueId
            )
        VALUES (
            0
            ,@clonedPersonId
            ,@countryCode
            ,@phoneNumber
            ,concat(@countryCode, @phoneNumber)
            ,@phoneNumberFormatted
            ,1
            ,0
            ,newid()
            ,@mobilePhone
            );

        declare @randomCampusId int = (select top 1 Id from Campus order by newid())

		-- create family
		INSERT INTO [Group] (
            IsSystem
            ,GroupTypeId
            ,NAME
            ,IsSecurityRole
            ,IsActive
            ,CampusId
            ,[Guid]
            ,[Order]
            )
        VALUES (
            0
            ,@familyGroupTypeId
            ,@lastName + ' Family'
            ,0
            ,1
            ,@randomCampusId
            ,NEWID()
            ,0
            )

        SET @groupId = SCOPE_IDENTITY()

        INSERT INTO [GroupMember] (
            IsSystem
            ,GroupId
            ,PersonId
            ,GroupRoleId
            ,GroupTypeId
            ,[Guid]
            ,GroupMemberStatus
			,DateTimeAdded
            )
        VALUES (
            0
            ,@groupId
            ,@clonedPersonId
            ,@adultRole
            ,@familyGroupTypeId
            ,newid()
            ,1
			,SYSDATETIME()
            )

        if (RAND()*5 > 1)
        begin
            -- have about 80% of first adult person as part of a giving group. 
            -- then about 90% of those 80% that also have their spouse as part of the same giving group
            update Person set GivingGroupId = @groupId where Id = @clonedPersonId;
        end

        SET @zipCode = ROUND(rand() * 9999, 0) + 80000;
        SET @streetAddress = ROUND(rand() * 9999, 0) + 100;

		set @geoPoint = concat('POINT (', (rand()*4)-114, ' ',  + (rand()*4)+30, ')');

        INSERT INTO [Location] (
            Street1
            ,Street2
            ,City
            ,[State]
            ,PostalCode
            ,IsActive
			,GeoPoint
			,Country
            ,[Guid]
            )
        VALUES (
            CONVERT(VARCHAR(max), @streetAddress) + ' Random Street'
            ,''
            ,'Phoenix'
            ,'AZ'
            ,@zipCode
            ,1
			,@geoPoint
			,'US'
            ,NEWID()
            )

        SET @locationId = SCOPE_IDENTITY()

        INSERT INTO [GroupLocation] (
            GroupId
            ,LocationId
            ,GroupLocationTypeValueId
            ,[Guid]
            ,IsMailingLocation
            ,IsMappedLocation
            )
        VALUES (
            @groupId
            ,@locationId
            ,@locationTypeValueHome
            ,NEWID()
            ,1
            ,1
            )
        
		if (@personCounter % 500 = 0)
		begin
		  COMMIT TRANSACTION
		  BEGIN TRANSACTION
  		  print concat(@personCounter, '/', @maxPerson);
        end

        SET @personCounter += 1;

		-- Decide which person gets kept with the automatic merge.
		-- spCrm_PersonMerge(@OldId, @NewId)
		-- @OldId is the Person to remove, @NewId is the Person to keep.
		EXEC [dbo].[spCrm_PersonMerge] @clonedPersonId, @personId;
    END

    COMMIT TRANSACTION
	SELECT TOP (@maxPerson) * FROM Person ORDER BY Id DESC

END
