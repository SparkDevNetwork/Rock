-- =====================================================================================================
-- Author:      Carl Roske
-- Create Date: 10/11/2022
-- Description: Adds Following records to randomly selected people (as followers) for randomly selected people (as followed) in your Rock database.
--
--  Takes less than 1 second to run on the demo database.
--	Took 2.5 minutes for 10K against a Person table of 100K.
--
-- Change History:
--   
-- =====================================================================================================

-- NOTE: Set @maxPersonFollowings to the number of Following records you want to create. The more people in your demo database, the longer the script takes.
DECLARE @maxPersonFollowings INT = 1000;

-- NOTE: How many days back a Following created / modified date COULD be set. Created / Modified date will be set between 0 and @dateOffset days ago.
DECLARE @dateOffset INT = 365;

-- NOTE: Get EntityTypeId for 'Person Alias'.
DECLARE 
	@PersonAliasEntityTypeId int = (select top 1 Id from EntityType where Guid = '90F5E87B-F0D5-4617-8AE9-EB57E673F36F');

-- NOTE: Fill a list of PurposeKeys that you want to include in the Followings.
DECLARE @tempPurposeKeyTable table(PurposeKey NVARCHAR(100))
INSERT INTO @tempPurposeKeyTable values('')
-- NOTE: Change this value (and uncomment) to whatever Following 'Purpose Key' you would like included in the randomizer.
-- INSERT INTO @tempPurposeKeyTable values('P')

-------------------------------------------------------------------------------------------------------

SET NOCOUNT ON
    DECLARE @followingCounter INT = 0
BEGIN

    BEGIN TRANSACTION

    DECLARE @followerPersonAliasId INT;
	DECLARE @followedPersonAliasId INT;
	DECLARE @createdModifiedDate DATETIME;
	DECLARE @followingGuid UNIQUEIDENTIFIER;
	DECLARE @purposeKey NVARCHAR(100);

    WHILE @followingCounter < @maxPersonFollowings
    BEGIN
	    -- NOTE: These SELECT queries DO NOT exclude any PersonAliases, with the intention of causing 'inaccurate' Followings 
		-- BECAUSE Person1 may have been merged with Person2 yet the Following records were not.
		
		/* Set follower PersonAlias ID -- this will be used for the PersonAliasId value in Following.*/
        select @followerPersonAliasId = (SELECT TOP 1 Id FROM [PersonAlias] ORDER BY NEWID());
		/* Set followed PersonAlias ID -- this will be used for the EntityId value in Following.*/
		select @followedPersonAliasId = (SELECT TOP 1 Id FROM [PersonAlias] ORDER BY NEWID());

		SET @followingGuid = NEWID();
        SET @purposeKey = (SELECT TOP 1 PurposeKey FROM @tempPurposeKeyTable ORDER BY NEWID());
		SET @createdModifiedDate = DATEADD(DAY, (ABS(CHECKSUM(NEWID())) % @dateOffset) * -1, SYSDATETIME());
        INSERT INTO [ Following] (
            [EntityTypeId]
            ,[EntityId] 
			,[PersonAliasId]
            ,[CreatedDateTime]
			,[ModifiedDateTime]
			,[CreatedByPersonAliasId]
			,[ModifiedByPersonAliasId]
			,[Guid]
			,[PurposeKey]
            )
        VALUES (
            @PersonAliasEntityTypeId
            ,@followedPersonAliasId
            ,@followerPersonAliasId
            ,@createdModifiedDate
			,@createdModifiedDate
			,@followerPersonAliasId
			,@followerPersonAliasId
			,@followingGuid
			,@purposeKey
            )

		IF (@followingCounter % 500 = 0)
		BEGIN
		  COMMIT TRANSACTION
		  BEGIN TRANSACTION
  		  PRINT concat(@followingCounter, '/', @maxPersonFollowings);
        END

        SET @followingCounter += 1;
    END

	COMMIT TRANSACTION

END

SELECT PersonAliasId, COUNT(*) AS PeopleYouAreFollowingCount FROM [Following] WHERE [EntityTypeId] = @PersonAliasEntityTypeId GROUP BY PersonAliasId ORDER BY COUNT(*) DESC