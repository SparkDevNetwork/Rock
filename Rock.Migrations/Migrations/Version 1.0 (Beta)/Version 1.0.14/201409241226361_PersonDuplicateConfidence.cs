// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class PersonDuplicateConfidence : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.PersonDuplicate", "IgnoreUntilScoreChanges", c => c.Boolean(nullable: false));
            AddColumn("dbo.PersonDuplicate", "TotalCapacity", c => c.Int());
            Sql( @"ALTER TABLE PersonDuplicate ADD ConfidenceScore AS sqrt ((Capacity / (TotalCapacity * .01)) * (Score / (Capacity * .01))) PERSISTED" );
            CreateIndex( "dbo.PersonDuplicate", "ConfidenceScore" );

            Sql( @" DECLARE @ObjectName varchar(50) = 'spCrm_PersonDuplicateFinder'
                    DECLARE @AlterSql nvarchar(MAX)
                    DECLARE @SchemaOwner varchar(12) = (SELECT TOP 1 s.name
										                    FROM sys.schemas AS s
											                    INNER JOIN sys.all_objects AS o ON s.[schema_id] = o.[schema_id]
										                    WHERE o.name = @ObjectName)

                    IF (@SchemaOwner != 'dbo')
	                    BEGIN
		                    SELECT @AlterSql = 'ALTER SCHEMA dbo TRANSFER [' + @SchemaOwner + '].' + @ObjectName
		                    EXEC sp_executesql @AlterSql
	                    END" );

            Sql( @"
/*
<doc>
	<summary>
 		This stored procedure detects potential duplicate person records and stores the results in [PersonDuplicate]
	</summary>
	
	<remarks>	
		Uses the following constants:
			* Group Type - Family: '790E3215-3B10-442B-AF69-616C0DCB998E'
            * Location Type - Home: '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
            * Phone Type - Home: '407E7E45-7B2E-4FCD-9605-ECB1339F2453'
            * Phone Type - Cell: 'AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303'
	</remarks>
	<code>
		EXEC [dbo].[spCrm_PersonDuplicateFinder]
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spCrm_PersonDuplicateFinder]
AS
BEGIN
    SET NOCOUNT ON

    BEGIN TRANSACTION

    -- prevent stored proc from running simultaneously since we are using #temp tables
    EXEC sp_getapplock 'spCrm_PersonDuplicateFinder'
        ,'Exclusive'
        ,'Transaction'
        ,0

    -- Flags that enable the various compare functions
    DECLARE @compareByEmail BIT = 1
        ,@compareByPartialName BIT = 1
        ,@compareByFullFirstName BIT = 1
        ,@compareByFullLastName BIT = 1
        ,@compareByPhone BIT = 1
        ,@compareByAddress BIT = 1
        ,@compareByBirthDate BIT = 1
        ,@compareByGender BIT = 1
        ,@compareByCampus BIT = 1
        ,@compareByMaritalStatus BIT = 1
    --
    -- Scores
    -- ones marked /**/ only added to score if already a potential match
    DECLARE @cScoreWeightEmail INT = 4
        ,@cScoreWeightPartialName INT = 1
        ,@cScoreWeightFullFirstName INT = 3 /**/
        ,@cScoreWeightFullLastName INT = 3 /**/
        ,@cScoreWeightCellPhoneNumber INT = 4
        ,@cScoreWeightNonCellPhoneNumber INT = 2
        ,@cScoreWeightAddress INT = 2
        ,@cScoreWeightBirthdate INT = 3 /**/
        ,@cScoreWeightGender INT = 1 /**/
        ,@cScoreWeightCampus INT = 1 /**/
        ,@cScoreWeightMaritalStatus INT = 1 /**/
    DECLARE @TotalCapacity INT = @cScoreWeightEmail + @cScoreWeightPartialName + @cScoreWeightFullFirstName + @cScoreWeightFullLastName + @cScoreWeightCellPhoneNumber + @cScoreWeightNonCellPhoneNumber + @cScoreWeightAddress + @cScoreWeightBirthdate + @cScoreWeightGender + @cScoreWeightCampus + @cScoreWeightMaritalStatus
    --
    -- Guids that this proc uses
    DECLARE @cGROUPTYPE_FAMILY_GUID UNIQUEIDENTIFIER = '790E3215-3B10-442B-AF69-616C0DCB998E'
        ,@cLOCATION_TYPE_HOME_GUID UNIQUEIDENTIFIER = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
        ,@cHOME_PHONENUMBER_DEFINEDVALUE_GUID UNIQUEIDENTIFIER = '407E7E45-7B2E-4FCD-9605-ECB1339F2453'
        ,@cCELL_PHONENUMBER_DEFINEDVALUE_GUID UNIQUEIDENTIFIER = 'AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303'
    --
    -- Other Declarations
    DECLARE @processDateTime DATETIME = SYSDATETIME()
        ,@cHOME_PHONENUMBER_DEFINEDVALUE_ID INT = (
            SELECT TOP 1 [Id]
            FROM [DefinedValue]
            WHERE [Guid] = @cHOME_PHONENUMBER_DEFINEDVALUE_GUID
            )
        ,@cCELL_PHONENUMBER_DEFINEDVALUE_ID INT = (
            SELECT TOP 1 [Id]
            FROM [DefinedValue]
            WHERE [Guid] = @cCELL_PHONENUMBER_DEFINEDVALUE_GUID
            )
        ,@cGROUPTYPE_FAMILY_ID INT = (
            SELECT TOP 1 [Id]
            FROM GroupType
            WHERE [Guid] = @cGROUPTYPE_FAMILY_GUID
            )
        ,@cLOCATION_TYPE_HOME_ID INT = (
            SELECT TOP 1 [Id]
            FROM DefinedValue
            WHERE [Guid] = @cLOCATION_TYPE_HOME_GUID
            )

    /*
    Populate Temporary Tables for each match criteria (Email, PartialName, PhoneNumber, Address)
    */
    -- Find Duplicates by looking at people with the exact same email address
    CREATE TABLE #PersonDuplicateByEmailTable (
        Id INT NOT NULL IDENTITY(1, 1)
        ,Email NVARCHAR(75) NOT NULL
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByEmailTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByEmailTable (
        Email
        ,PersonAliasId
        )
    SELECT [e].[Email] [Email]
        ,[pa].[Id] [PersonAliasId]
    FROM (
        SELECT [a].[Email]
        FROM (
            SELECT [Email]
                ,COUNT(*) [EmailCount]
            FROM [Person] [p]
            WHERE isnull([Email], '') != ''
            GROUP BY [Email]
            ) [a]
        WHERE [a].[EmailCount] > 1
        ) [e]
    JOIN [Person] [p] ON [p].[Email] = [e].[Email]
    JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [p].[Id]
    WHERE [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
        AND @compareByEmail = 1

    -- Find Duplicates by looking at people with the exact same lastname and same first 2 chars of First/Nick name
    CREATE TABLE #PersonDuplicateByNameTable (
        Id INT NOT NULL IDENTITY(1, 1)
        ,LastName NVARCHAR(50) NOT NULL
        ,First2 NVARCHAR(50) NOT NULL -- intentionally 50 vs 2 for performance reasons (sql server spends time on the length constraint if it's shorter than the source column)
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByNameTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByNameTable (
        LastName
        ,First2
        ,PersonAliasId
        )
    SELECT [e].[LastName]
        ,[e].[First2]
        ,[pa].[Id] [PersonAliasId]
    FROM (
        SELECT [a].[First2]
            ,[a].[LastName]
        FROM (
            SELECT SUBSTRING([FirstName], 1, 2) [First2]
                ,[LastName]
                ,COUNT(*) [MatchCount]
            FROM [Person] [p]
            WHERE isnull([LastName], '') != ''
                AND [FirstName] IS NOT NULL
                AND LEN([FirstName]) >= 2
            GROUP BY [LastName]
                ,SUBSTRING([FirstName], 1, 2)
            ) [a]
        WHERE [a].[MatchCount] > 1
        ) [e]
    JOIN [Person] [p] ON [p].[LastName] = [e].[LastName]
        AND [p].[FirstName] LIKE (e.First2 + '%')
    JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [p].[Id]
    WHERE [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
        AND @compareByPartialName = 1

    -- Find Duplicates by looking at people with the exact same phone number
    CREATE TABLE #PersonDuplicateByPhoneTable (
        Id INT NOT NULL IDENTITY(1, 1)
        ,Number NVARCHAR(20) NOT NULL
        ,Extension NVARCHAR(20) NOT NULL
        ,CountryCode NVARCHAR(3) NOT NULL
        ,NumberTypeValueId INT NOT NULL
        ,Gender INT NOT NULL
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByPhoneTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByPhoneTable (
        Number
        ,Extension
        ,CountryCode
        ,NumberTypeValueId
        ,Gender
        ,PersonAliasId
        )
    SELECT [m].[Number]
        ,isnull([m].[Extension], '')
        ,isnull([m].[CountryCode], '')
        ,[m].[NumberTypeValueId]
        ,[p].[Gender]
        ,[pa].[Id] [PersonAliasId]
    FROM (
        SELECT DISTINCT [Number]
            ,[Extension]
            ,[CountryCode]
            ,[NumberTypeValueId]
            ,[Gender]
        FROM (
            SELECT [pn].[Number]
                ,[pn].[Extension]
                ,[pn].[CountryCode]
                ,[pn].[NumberTypeValueId]
                ,[p].[Gender]
                ,COUNT(*) [MatchCount]
            FROM [PhoneNumber] [pn]
            JOIN [Person] [p] ON [p].[Id] = [pn].[PersonId]
            WHERE ISNUMERIC([pn].[Number]) = 1
                AND [pn].[NumberTypeValueId] IN (
                    @cHOME_PHONENUMBER_DEFINEDVALUE_ID
                    ,@cCELL_PHONENUMBER_DEFINEDVALUE_ID
                    )
            GROUP BY [pn].[Number]
                ,[pn].[Extension]
                ,[pn].[CountryCode]
                ,[pn].[NumberTypeValueId]
                ,[p].[Gender]
            ) [a]
        WHERE [a].[MatchCount] > 1
        ) [m]
    JOIN [PhoneNumber] [pn] ON [pn].[Number] = [m].[Number]
        AND ISNULL([pn].[Extension], '') = ISNULL([m].[Extension], '')
        AND ISNULL([pn].[CountryCode], '') = ISNULL([m].[CountryCode], '')
        AND [pn].[NumberTypeValueId] = [m].[NumberTypeValueId]
    JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [pn].[PersonId]
    JOIN [Person] [p] ON [p].[Id] = [pa].[PersonId]
    WHERE [m].[Gender] = [p].[Gender] -- only consider it a potential duplicate person if the gender and the phone number are the same (to reduce false matches)
        AND [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
        AND @compareByPhone = 1

    -- Find Duplicates by looking at people with the exact same address (Location)
    CREATE TABLE #PersonDuplicateByAddressTable (
        Id INT NOT NULL IDENTITY(1, 1)
        ,LocationId INT NOT NULL
        ,Gender INT NOT NULL
        ,GroupRoleId INT NOT NULL
        ,PersonAliasId INT NOT NULL
        ,CONSTRAINT [pk_PersonDuplicateByAddressTable] PRIMARY KEY CLUSTERED (Id)
        );

    INSERT INTO #PersonDuplicateByAddressTable (
        LocationId
        ,Gender
        ,GroupRoleId
        ,PersonAliasId
        )
    -- from the locations that have multiple potential duplicate persons, select the person records (along with gender and family role) that are associated with that location
    -- in case the person has multiple home addresses, get the first one with preference given to MapLocation=true
    SELECT CASE 
            WHEN a.MaxMappedLocationId > 0
                THEN a.MaxMappedLocationId
            ELSE a.MaxNotMappedLocationId
            END [LocationId]
        ,[Gender]
        ,[GroupRoleId]
        ,[PersonAliasId]
    FROM (
        SELECT max(CASE 
                    WHEN [IsMappedLocation] = 1
                        THEN [LocationId]
                    ELSE 0
                    END) [MaxMappedLocationId]
            ,max(CASE 
                    WHEN [IsMappedLocation] = 0
                        THEN [LocationId]
                    ELSE 0
                    END) [MaxNotMappedLocationId]
            ,[Gender]
            ,[GroupRoleId]
            ,[PersonAliasId]
        FROM (
            SELECT [m].[LocationId]
                ,[gl].[IsMappedLocation]
                ,[m].[Gender]
                ,[m].[GroupRoleId]
                ,[pa].[Id] [PersonAliasId]
            FROM (
                SELECT [LocationId]
                    ,[Gender]
                    ,[GroupRoleId]
                    ,[MatchCount]
                FROM (
                    -- consider it a potential duplicate person if multiple people have the same address (LocationId), Gender and GroupRole (adult, child)
                    SELECT [gl].[LocationId]
                        ,[gm].[GroupRoleId]
                        ,[p].[Gender]
                        ,COUNT(*) [MatchCount]
                    FROM [Person] [p]
                    JOIN [GroupMember] [gm] ON [gm].[PersonId] = [p].[Id]
                    JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
                    JOIN [GroupLocation] [gl] ON [gl].[GroupId] = [g].[id]
                    JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
                        AND [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
                    WHERE [gl].[GroupLocationTypeValueId] = @cLOCATION_TYPE_HOME_ID
                    GROUP BY [gl].[LocationID]
                        ,[p].[Gender]
                        ,[gm].[GroupRoleId]
                    ) [a]
                WHERE [MatchCount] > 1
                ) [m]
            JOIN [GroupLocation] [gl] ON [gl].[LocationId] = [m].[LocationId]
            JOIN [Group] [g] ON [g].[Id] = [gl].[GroupId]
            JOIN [GroupMember] [gm] ON [gm].[GroupId] = [g].[Id]
            JOIN [PersonAlias] [pa] ON [gm].[PersonId] = [pa].[PersonId]
            JOIN [Person] [p] ON [p].[Id] = [pa].[PersonId]
            WHERE [m].[Gender] = [p].[Gender]
                AND [m].[GroupRoleId] = [gm].[GroupRoleId] -- only consider it a potential duplicate person if the gender,role and location are the same (to reduce false matches due to married couples)
                AND [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
                AND @compareByAddress = 1
            ) [a]
        GROUP BY [PersonAliasId]
            ,[Gender]
            ,[GroupRoleId]
        ) a

    -- get the original ConfidenceScore of the IgnoreUntilScoreChanges records so that we can un-ignore the ones that have a changed score
    DECLARE @IgnoreUntilScoreChangesTable TABLE (
        Id INT
        ,ConfidenceScore float
        );

    INSERT INTO @IgnoreUntilScoreChangesTable
    SELECT [Id]
        ,[ConfidenceScore]
    FROM [PersonDuplicate]
    WHERE [IgnoreUntilScoreChanges] = 1;

    /* 
    Reset Scores for everybody. (We want to preserve each record's [IsConfirmedAsNotDuplicate] value)
    */
    UPDATE [PersonDuplicate]
    SET [Score] = 0
        ,[ScoreDetail] = '';

    /*
    Merge Results of Matches into PersonDuplicate Table
    */
    -- Update PersonDuplicate table with results of email match
    MERGE [PersonDuplicate] AS TARGET
    USING (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,[e1].[Email] [Email]
        FROM #PersonDuplicateByEmailTable [e1]
        JOIN #PersonDuplicateByEmailTable [e2] ON [e1].[email] = [e2].[email]
            AND [e1].[Id] != [e2].[Id]
            AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
        ) AS source(PersonAliasId, DuplicatePersonAliasId, Email)
        ON (target.PersonAliasId = source.PersonAliasId)
            AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED
        THEN
            UPDATE
            SET [Score] = [Score] + @cScoreWeightEmail
                ,[ScoreDetail] += '|Email'
                ,[ModifiedDateTime] = @processDateTime
    WHEN NOT MATCHED
        THEN
            INSERT (
                PersonAliasId
                ,DuplicatePersonAliasId
                ,Score
                ,ScoreDetail
                ,IsConfirmedAsNotDuplicate
                ,ModifiedDateTime
                ,CreatedDateTime
                ,[Guid]
                )
            VALUES (
                source.PersonAliasId
                ,source.DuplicatePersonAliasId
                ,@cScoreWeightEmail
                ,'|Email'
                ,0
                ,@processDateTime
                ,@processDateTime
                ,NEWID()
                );

    -- Update PersonDuplicate table with results of partial name match
    MERGE [PersonDuplicate] AS TARGET
    USING (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,[e1].[First2] [First2]
            ,[e1].[LastName] [LastName]
        FROM #PersonDuplicateByNameTable [e1]
        JOIN #PersonDuplicateByNameTable [e2] ON [e1].[First2] = [e2].[First2]
            AND [e1].[LastName] = [e2].[LastName]
            AND [e1].[Id] != [e2].[Id]
            AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
        ) AS source(PersonAliasId, DuplicatePersonAliasId, LastName, First2)
        ON (target.PersonAliasId = source.PersonAliasId)
            AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED
        THEN
            UPDATE
            SET [Score] = [Score] + @cScoreWeightPartialName
                ,[ScoreDetail] += '|PartialName'
                ,[ModifiedDateTime] = @processDateTime
    WHEN NOT MATCHED
        THEN
            INSERT (
                PersonAliasId
                ,DuplicatePersonAliasId
                ,Score
                ,ScoreDetail
                ,IsConfirmedAsNotDuplicate
                ,ModifiedDateTime
                ,CreatedDateTime
                ,[Guid]
                )
            VALUES (
                source.PersonAliasId
                ,source.DuplicatePersonAliasId
                ,@cScoreWeightPartialName
                ,'|PartialName'
                ,0
                ,@processDateTime
                ,@processDateTime
                ,NEWID()
                );

    --  Update PersonDuplicate table with results of phonenumber match for each number type. 
    --  For example, if both the Cell and Home phone match, that should be a higher score 
    DECLARE @PhoneNumberTypeValueId INT
    DECLARE @PhoneNumberTypeScore INT
    DECLARE @PhoneNumberTypeText VARCHAR(50)

    DECLARE phoneNumberTypeCursor CURSOR FAST_FORWARD
    FOR
    SELECT [Id]
    FROM [DefinedValue]
    WHERE [Id] IN (
            @cHOME_PHONENUMBER_DEFINEDVALUE_ID
            ,@cCELL_PHONENUMBER_DEFINEDVALUE_ID
            )
    ORDER BY [Id]

    OPEN phoneNumberTypeCursor

    FETCH NEXT
    FROM phoneNumberTypeCursor
    INTO @PhoneNumberTypeValueId

    -- loop thru each of the phone number types (home, cell)
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF (@PhoneNumberTypeValueId = @cCELL_PHONENUMBER_DEFINEDVALUE_ID)
        BEGIN
            SET @PhoneNumberTypeScore = @cScoreWeightCellPhoneNumber
            SET @PhoneNumberTypeText = '|CellPhone'
        END
        ELSE
        BEGIN
            SET @PhoneNumberTypeScore = @cScoreWeightNonCellPhoneNumber
            SET @PhoneNumberTypeText = '|Phone'
        END

        MERGE [PersonDuplicate] AS TARGET
        USING (
            SELECT [e1].[PersonAliasId] [PersonAliasId]
                ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
                ,[e1].[Number]
                ,[e1].[Extension]
                ,[e1].[CountryCode]
                ,[e1].[NumberTypeValueId]
                ,[e1].[Gender]
            FROM #PersonDuplicateByPhoneTable [e1]
            JOIN #PersonDuplicateByPhoneTable [e2] ON [e1].[Number] = [e2].[Number]
                AND [e1].[Extension] = [e2].[Extension]
                AND [e1].[CountryCode] = [e2].[CountryCode]
                AND [e1].[NumberTypeValueId] = [e2].[NumberTypeValueId]
                AND [e1].[Gender] = [e2].[Gender]
                AND [e1].[Id] != [e2].[Id]
                AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
            WHERE [e1].[NumberTypeValueId] = @PhoneNumberTypeValueId
            ) AS source(PersonAliasId, DuplicatePersonAliasId, Number, Extension, CountryCode, NumberTypeValueId, Gender)
            ON (target.PersonAliasId = source.PersonAliasId)
                AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
        WHEN MATCHED
            THEN
                UPDATE
                SET [Score] = [Score] + @PhoneNumberTypeScore
                    ,[ScoreDetail] += @PhoneNumberTypeText
                    ,[ModifiedDateTime] = @processDateTime
        WHEN NOT MATCHED
            THEN
                INSERT (
                    PersonAliasId
                    ,DuplicatePersonAliasId
                    ,Score
                    ,ScoreDetail
                    ,IsConfirmedAsNotDuplicate
                    ,ModifiedDateTime
                    ,CreatedDateTime
                    ,[Guid]
                    )
                VALUES (
                    source.PersonAliasId
                    ,source.DuplicatePersonAliasId
                    ,@PhoneNumberTypeScore
                    ,@PhoneNumberTypeText
                    ,0
                    ,@processDateTime
                    ,@processDateTime
                    ,NEWID()
                    );

        FETCH NEXT
        FROM phoneNumberTypeCursor
        INTO @PhoneNumberTypeValueId
    END

    CLOSE phoneNumberTypeCursor

    DEALLOCATE phoneNumberTypeCursor

    -- Update PersonDuplicate table with results of address (location) match
    MERGE [PersonDuplicate] AS TARGET
    USING (
        SELECT [e1].[PersonAliasId] [PersonAliasId]
            ,[e2].[PersonAliasId] [DuplicatePersonAliasId]
            ,[e1].[LocationId]
            ,[e1].[Gender]
            ,[e1].[GroupRoleId]
        FROM #PersonDuplicateByAddressTable [e1]
        JOIN #PersonDuplicateByAddressTable [e2] ON [e1].[LocationId] = [e2].[LocationId]
            AND [e1].[Gender] = [e2].[Gender]
            AND [e1].[GroupRoleId] = [e2].[GroupRoleId]
            AND [e1].[Id] != [e2].[Id]
            AND [e1].[PersonAliasId] > [e2].[PersonAliasId] -- we only need the matched pair in there once (don't need both PersonA == PersonB and PersonB == PersonA)
        ) AS source(PersonAliasId, DuplicatePersonAliasId, LocationId, Gender, GroupRoleId)
        ON (target.PersonAliasId = source.PersonAliasId)
            AND (target.DuplicatePersonAliasId = source.DuplicatePersonAliasId)
    WHEN MATCHED
        THEN
            UPDATE
            SET [Score] = [Score] + @cScoreWeightAddress
                ,[ScoreDetail] += '|Address'
                ,[ModifiedDateTime] = @processDateTime
    WHEN NOT MATCHED
        THEN
            INSERT (
                PersonAliasId
                ,DuplicatePersonAliasId
                ,Score
                ,ScoreDetail
                ,IsConfirmedAsNotDuplicate
                ,ModifiedDateTime
                ,CreatedDateTime
                ,[Guid]
                )
            VALUES (
                source.PersonAliasId
                ,source.DuplicatePersonAliasId
                ,@cScoreWeightAddress
                ,'|Address'
                ,0
                ,@processDateTime
                ,@processDateTime
                ,NEWID()
                );

    /* Calculate Capacities before we do the additional scores
    */
    -- set base capacity to include MaritalStatus and Gender, since everybody has values for those
    UPDATE [PersonDuplicate]
    SET [Capacity] = CASE 
            WHEN @compareByGender = 1
                THEN @cScoreWeightGender
            ELSE 0
            END + CASE 
            WHEN @compareByMaritalStatus = 1
                THEN @cScoreWeightMaritalStatus
            ELSE 0
            END;

    -- increment capacity values for Email, PartialName, Birthdate (do in one Update statement since these are all person fields)
    UPDATE [PersonDuplicate]
    SET [Capacity] += CASE 
            -- add the Email capacity
            WHEN @compareByEmail = 1
                AND isnull(p.Email, '') != ''
                THEN @cScoreWeightEmail
            ELSE 0
            END + CASE 
            -- add partial name capacity
            WHEN @compareByPartialName = 1
                AND (
                    isnull(p.LastName, '') != ''
                    OR isnull(p.FirstName, '') != ''
                    )
                THEN @cScoreWeightPartialName
            ELSE 0
            END + CASE 
            -- full first name capacity
            WHEN @compareByFullFirstName = 1
                AND (
                    isnull(p.FirstName, '') != ''
                    OR isnull(p.NickName, '') != ''
                    )
                THEN @cScoreWeightFullFirstName
            ELSE 0
            END + CASE 
            -- full last name capacity
            WHEN @compareByFullLastName = 1
                AND isnull(p.LastName, '') != ''
                THEN @cScoreWeightFullFirstName
            ELSE 0
            END + CASE 
            -- add the Birthday Capacity
            WHEN @compareByBirthDate = 1
                AND p.BirthDate IS NOT NULL
                THEN @cScoreWeightBirthdate
            ELSE 0
            END
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId

    ---- NOTE Phone Capacity is higher if BOTH Home and Cell Phone are available
    -- increment capacity values for Home Phone
    UPDATE [PersonDuplicate]
    SET [Capacity] += @cScoreWeightNonCellPhoneNumber
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
    WHERE p.Id IN (
            SELECT PersonId
            FROM PhoneNumber
            WHERE NumberTypeValueId = @cHOME_PHONENUMBER_DEFINEDVALUE_ID
            )
        AND @compareByPhone = 1

    -- increment capacity values for Cell Phone
    UPDATE [PersonDuplicate]
    SET [Capacity] += @cScoreWeightCellPhoneNumber
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
    WHERE p.Id IN (
            SELECT PersonId
            FROM PhoneNumber
            WHERE NumberTypeValueId = @cCELL_PHONENUMBER_DEFINEDVALUE_ID
            )
        AND @compareByPhone = 1

    -- increment capacity values for Address
    UPDATE [PersonDuplicate]
    SET [Capacity] += @cScoreWeightAddress
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
    WHERE p.Id IN (
            SELECT PersonId
            FROM [GroupMember] [gm]
            JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
            JOIN [GroupLocation] [gl] ON [gl].[GroupId] = [g].[id]
            JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
                AND [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
            WHERE [gl].[GroupLocationTypeValueId] = @cLOCATION_TYPE_HOME_ID
            )
        AND @compareByAddress = 1

    -- increment capacity values for Campus
    UPDATE [PersonDuplicate]
    SET [Capacity] += @cScoreWeightCampus
    FROM PersonDuplicate pd
    JOIN PersonAlias pa ON pa.Id = pd.PersonAliasId
    JOIN Person p ON p.Id = pa.PersonId
    WHERE p.Id IN (
            SELECT PersonId
            FROM [GroupMember] [gm]
            JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
            WHERE [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
                AND g.CampusId IS NOT NULL
            )
        AND @compareByCampus = 1

    /*
    Add additional scores to people that are already potential matches 
    */
    -- Increment the score on potential matches that have the same FirstName (or NickName)
    -- put the updated ids into a temp list first to speed things up
    DECLARE @updatedIdsFirstName TABLE (id INT);

    INSERT INTO @updatedIdsFirstName
    SELECT pd.Id
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE (
            (
                p1.FirstName = p2.FirstName
                AND isnull(p1.FirstName, '') != ''
                )
            OR (
                p1.NickName = p2.NickName
                AND isnull(p1.NickName, '') != ''
                )
            OR (
                p1.FirstName = p2.NickName
                AND isnull(p1.NickName, '') != ''
                AND isnull(p1.FirstName, '') != ''
                )
            OR (
                p1.NickName = p2.FirstName
                AND isnull(p1.NickName, '') != ''
                AND isnull(p1.FirstName, '') != ''
                )
            )
        AND @compareByFullFirstName = 1

    UPDATE [PersonDuplicate]
    SET [Score] = [Score] + @cScoreWeightFullFirstName
        ,[ScoreDetail] += '|FirstName'
    WHERE Id IN (
            SELECT Id
            FROM @updatedIdsFirstName
            )

    -- Increment the score on potential matches that have the same LastName
    -- put the updated ids into a temp list first to speed things up
    DECLARE @updatedIdsLastName TABLE (id INT);

    INSERT INTO @updatedIdsLastName
    SELECT pd.Id
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.LastName = p2.LastName
        AND isnull(p1.LastName, '') != ''
        AND @compareByFullLastName = 1

    UPDATE [PersonDuplicate]
    SET [Score] = [Score] + @cScoreWeightFullFirstName
        ,[ScoreDetail] += '|LastName'
    WHERE Id IN (
            SELECT Id
            FROM @updatedIdsLastName
            )

    -- Increment the score on potential matches that have the same birthday
    -- put the updated ids into a temp list first to speed things up
    DECLARE @updatedIdsBirthdate TABLE (id INT);

    INSERT INTO @updatedIdsBirthdate
    SELECT pd.Id
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.BirthDate = p2.BirthDate
        AND @compareByBirthDate = 1

    UPDATE [PersonDuplicate]
    SET [Score] = [Score] + @cScoreWeightBirthdate
        ,[ScoreDetail] += '|Birthdate'
    WHERE Id IN (
            SELECT Id
            FROM @updatedIdsBirthdate
            )

    -- Increment the score on potential matches that have the same gender
    -- put the updated ids into a temp list first to speed things up
    DECLARE @updatedIdsGender TABLE (id INT);

    INSERT INTO @updatedIdsGender
    SELECT pd.Id
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.Gender = p2.Gender
        AND @compareByGender = 1

    UPDATE [PersonDuplicate]
    SET [Score] = [Score] + @cScoreWeightGender
        ,[ScoreDetail] += '|Gender'
    WHERE Id IN (
            SELECT Id
            FROM @updatedIdsGender
            )

    -- Increment the score on potential matches that have the same campus
    -- put the updated ids into a temp list first to speed things up
    DECLARE @updatedIdsCampus TABLE (id INT);

    INSERT INTO @updatedIdsCampus
    SELECT pd.Id
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    JOIN [GroupMember] [gm1] ON [gm1].[PersonId] = [p1].[Id]
    JOIN [Group] [g1] ON [gm1].[GroupId] = [g1].[Id]
    JOIN [GroupMember] [gm2] ON [gm2].[PersonId] = [p2].[Id]
    JOIN [Group] [g2] ON [gm2].[GroupId] = [g2].[Id]
    WHERE g1.CampusId = g2.CampusId
        AND g1.CampusId IS NOT NULL
        AND g2.CampusId IS NOT NULL
        AND [g1].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
        AND [g2].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
        AND @compareByCampus = 1

    UPDATE PersonDuplicate
    SET [Score] = [Score] + @cScoreWeightCampus
        ,[ScoreDetail] += '|Campus'
    WHERE Id IN (
            SELECT Id
            FROM @updatedIdsCampus
            )

    -- Increment the score on potential matches that have the same marital status
    -- put the updated ids into a temp list first to speed things up
    DECLARE @updatedIdsMarital TABLE (id INT);

    INSERT INTO @updatedIdsMarital
    SELECT pd.Id
    FROM PersonDuplicate pd
    JOIN PersonAlias pa1 ON pa1.Id = pd.PersonAliasId
    JOIN PersonAlias pa2 ON pa2.Id = pd.DuplicatePersonAliasId
    JOIN Person p1 ON p1.Id = pa1.PersonId
    JOIN Person p2 ON p2.Id = pa2.PersonId
    WHERE p1.MaritalStatusValueId = p2.MaritalStatusValueId
        AND @compareByMaritalStatus = 1

    UPDATE PersonDuplicate
    SET [Score] = [Score] + @cScoreWeightMaritalStatus
        ,[ScoreDetail] += '|Marital'
    WHERE Id IN (
            SELECT Id
            FROM @updatedIdsMarital
            )

    /* 
    Clean up records that no longer are duplicates 
    */
    DECLARE @staleCount INT;

    SELECT @staleCount = count(*)
    FROM [PersonDuplicate]
    WHERE [ModifiedDateTime] < @processDateTime

    IF (@staleCount != 0)
    BEGIN
        DELETE
        FROM [PersonDuplicate]
        WHERE [ModifiedDateTime] < @processDateTime
    END

    UPDATE [PersonDuplicate]
    SET [TotalCapacity] = @TotalCapacity;

    UPDATE [PersonDuplicate]
    set IgnoreUntilScoreChanges = 0
    from [PersonDuplicate] [pd]
    join @IgnoreUntilScoreChangesTable [g]
    on pd.Id = g.id
    where pd.ConfidenceScore != g.ConfidenceScore; 

    /*
    Explicitly clean up temp tables before the proc exits (vs. have SQL Server do it for us after the proc is done)
    */
    DROP TABLE #PersonDuplicateByEmailTable;

    DROP TABLE #PersonDuplicateByNameTable;

    DROP TABLE #PersonDuplicateByPhoneTable;

    DROP TABLE #PersonDuplicateByAddressTable;

    COMMIT
END
" );

            Sql( @" DECLARE @ObjectName varchar(50) = 'spCheckin_BadgeAttendance'
                    DECLARE @AlterSql nvarchar(MAX)
                    DECLARE @SchemaOwner varchar(12) = (SELECT TOP 1 s.name
										                    FROM sys.schemas AS s
											                    INNER JOIN sys.all_objects AS o ON s.[schema_id] = o.[schema_id]
										                    WHERE o.name = @ObjectName)

                    IF (@SchemaOwner != 'dbo')
	                    BEGIN
		                    SELECT @AlterSql = 'ALTER SCHEMA dbo TRANSFER [' + @SchemaOwner + '].' + @ObjectName
		                    EXEC sp_executesql @AlterSql
	                    END" );

            Sql( @" DECLARE @ObjectName varchar(50) = 'spCheckin_BadgeAttendance'
                    DECLARE @AlterSql nvarchar(MAX)
                    DECLARE @SchemaOwner varchar(12) = (SELECT TOP 1 s.name
										                    FROM sys.schemas AS s
											                    INNER JOIN sys.all_objects AS o ON s.[schema_id] = o.[schema_id]
										                    WHERE o.name = @ObjectName)

                    IF (@SchemaOwner != 'dbo')
	                    BEGIN
		                    SELECT @AlterSql = 'ALTER SCHEMA dbo TRANSFER [' + @SchemaOwner + '].' + @ObjectName
		                    EXEC sp_executesql @AlterSql
	                    END" );

            Sql( @"
/*
<doc>
	<summary>
 		This function returns the attendance data needed for the Attendance Badge. If no family role (adult/child)
		is given it is looked up.  If the individual is an adult it will return family attendance if it's a child
		it will return the individual's attendance. If a person is in two families once as a child once as an
		adult it will pick the first role it finds.
	</summary>

	<returns>
		* AttendanceCount
		* SundaysInMonth
		* Month
		* Year
	</returns>
	<param name=""PersonId"" datatype=""int"">Person the badge is for</param>
	<param name=""Role Guid"" datatype=""uniqueidentifier"">The role of the person in the family (optional)</param>
	<param name=""Reference Date"" datatype=""datetime"">A date in the last month for the badge (optional, default is today)</param>
	<param name=""Number of Months"" datatype=""int"">Number of months to display (optional, default is 24)</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spCheckin_BadgeAttendance] 2 -- Ted Decker (adult)
		EXEC [dbo].[spCheckin_BadgeAttendance] 4 -- Noah Decker (child)
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCheckin_BadgeAttendance]
	@PersonId int 
	, @RoleGuid uniqueidentifier = null
	, @ReferenceDate datetime = null
	, @MonthCount int = 24
AS
BEGIN
	DECLARE @cROLE_ADULT uniqueidentifier = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
	DECLARE @cROLE_CHILD uniqueidentifier = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'
	DECLARE @cGROUP_TYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'
	DECLARE @StartDay datetime
	DECLARE @LastDay datetime

	-- if role (adult/child) is unknown determine it
	IF (@RoleGuid IS NULL)
	BEGIN
		SELECT TOP 1 @RoleGuid =  gtr.[Guid] 
			FROM [GroupTypeRole] gtr
				INNER JOIN [GroupMember] gm ON gm.[GroupRoleId] = gtr.[Id]
				INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]
			WHERE gm.[PersonId] = @PersonId 
				AND g.[GroupTypeId] = (SELECT [ID] FROM [GroupType] WHERE [Guid] = @cGROUP_TYPE_FAMILY)
	END

	-- if start date null get today's date
	IF @ReferenceDate is null
		SET @ReferenceDate = getdate()

	-- set data boundaries
	SET @LastDay = dbo.ufnUtility_GetLastDayOfMonth(@ReferenceDate) -- last day is most recent day
	SET @StartDay = DATEADD(month, (@MonthCount * -1), @LastDay) -- start day is the oldest day

	-- make sure last day is not in future (in case there are errant checkin data)
	IF (@LastDay > getdate())
	BEGIN
		SET @LastDay = getdate()
	END

	--PRINT 'Last Day: ' + CONVERT(VARCHAR, @LastDay, 101) 
	--PRINT 'Start Day: ' + CONVERT(VARCHAR, @StartDay, 101) 

    declare @familyMemberPersonIds table (personId int); 
    declare @groupIds table (groupId int);

    insert into @familyMemberPersonIds SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId);
    insert into @groupIds SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]();

	-- query for attendance data
	IF (@RoleGuid = @cROLE_ADULT)
	BEGIN
		SELECT 
			COUNT([Attended]) AS [AttendanceCount]
			, (SELECT dbo.ufnUtility_GetNumberOfSundaysInMonth(DATEPART(year, [SundayDate]), DATEPART(month, [SundayDate]), 'True' )) AS [SundaysInMonth]
			, DATEPART(month, [SundayDate]) AS [Month]
			, DATEPART(year, [SundayDate]) AS [Year]
		FROM (

			SELECT s.[SundayDate], [Attended]
				FROM dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
				LEFT OUTER JOIN (	
						SELECT 
							DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) AS [AttendedSunday],
							1 as [Attended]
						FROM
							[Attendance] a
							INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
						WHERE 
							[GroupId] IN (select groupId from @groupIds)
							AND pa.[PersonId] IN (select PersonId from @familyMemberPersonIds) 
							AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
						) a ON [AttendedSunday] = s.[SundayDate]

		) [CheckinDates]
		GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
		OPTION (MAXRECURSION 1000)
	END
	ELSE
	BEGIN
		SELECT 
			COUNT([Attended]) AS [AttendanceCount]
			, (SELECT dbo.ufnUtility_GetNumberOfSundaysInMonth(DATEPART(year, [SundayDate]), DATEPART(month, [SundayDate]), 'True' )) AS [SundaysInMonth]
			, DATEPART(month, [SundayDate]) AS [Month]
			, DATEPART(year, [SundayDate]) AS [Year]
		FROM (

			SELECT s.[SundayDate], [Attended]
				FROM dbo.ufnUtility_GetSundaysBetweenDates(@StartDay, @LastDay) s
				LEFT OUTER JOIN (	
						SELECT 
							DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) AS [AttendedSunday],
							1 as [Attended]
						FROM
							[Attendance] a
							INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
						WHERE 
							[GroupId] IN (select groupId from @groupIds)
							AND pa.[PersonId] = @PersonId 
							AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
						) a ON [AttendedSunday] = s.[SundayDate]

		) [CheckinDates]
		GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
		OPTION (MAXRECURSION 1000)
	END

	
END

" );

            // Update transaction list add on person profile to link to new scheduled transaction page
            RockMigrationHelper.AddBlockAttributeValue( "9382B285-3EF6-47F7-94BB-A47C498196A3", "C6D07A89-84C9-412A-A584-E37E59506566", @"b1ca86dc-9890-4d26-8ebd-488044e1b3dd" );
            Sql( @"
    UPDATE [Page] SET
	      [InternalName] = 'Add Transaction'
	    , [PageTitle] = 'Add Transaction'
	    , [BrowserTitle] = 'Add Transaction'
    WHERE [Guid] = 'B1CA86DC-9890-4D26-8EBD-488044E1B3DD'
" );

            // Give staff and staff-like users access to edit person entity type
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Person", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", 0, "10200730-ABAE-4368-A16C-D10F4E64B9D1" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.Person", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", 0, "361EE78F-AFEA-486A-AF6D-363A3E19B028" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.PersonDuplicate", new[] { "ConfidenceScore" });
            DropColumn("dbo.PersonDuplicate", "ConfidenceScore");
            DropColumn("dbo.PersonDuplicate", "TotalCapacity");
            DropColumn("dbo.PersonDuplicate", "IgnoreUntilScoreChanges");
        }
    }
}
