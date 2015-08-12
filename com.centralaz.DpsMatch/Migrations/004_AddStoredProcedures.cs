using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.DpsMatch.Migrations
{
    [MigrationNumber( 4, "1.0.14" )]
    public class AddStoredProcedures : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
            CREATE Proc [dbo].[_com_centralaz_spDpsMatch_Offender]
            @KeyString nvarchar(100),
            @LastName nvarchar(50),
            @FirstName nvarchar(50),
            @MiddleInitial char(1),
            @Age int,
            @Height int,
            @Weight int,
            @Race nvarchar(50),
            @Sex nvarchar(50),
            @Hair nvarchar(50),
            @Eyes nvarchar(50),
            @ResidentialAddress nvarchar(100),
            @ResidentialCity nvarchar(50),
            @ResidentialState nvarchar(50),
            @ResidentialZip int,
            @VerificationDate datetime,
            @Offense nvarchar(500),
            @OffenseLevel int,
            @Absconder bit,
            @ConvictingJurisdiction nvarchar(100),
            @Unverified bit
            AS
	            DECLARE @UpdateDateTime DateTime SET @UpdateDateTime = GETDATE()	
	            IF NOT EXISTS(
		            SELECT * FROM dbo._com_centralaz_DpsMatch_Offender
		            WHERE [KeyString] = @KeyString
		            )		
	            BEGIN	
		            INSERT INTO [dbo].[_com_centralaz_DpsMatch_Offender]
                       ([KeyString]
                       ,[LastName]
                       ,[FirstName]
                       ,[MiddleInitial]
                       ,[Age]
                       ,[Height]
                       ,[Weight]
                       ,[Race]
                       ,[Sex]
                       ,[Hair]
                       ,[Eyes]
                       ,[ResidentialAddress]
                       ,[ResidentialCity]
                       ,[ResidentialState]
                       ,[ResidentialZip]
                       ,[VerificationDate]
                       ,[Offense]
                       ,[OffenseLevel]
                       ,[Absconder]
                       ,[ConvictingJurisdiction]
                       ,[Unverified]
                       ,[Guid]
                       ,[CreatedDateTime]
                       ,[ModifiedDateTime]
                       ,[CreatedByPersonAliasId]
                       ,[ModifiedByPersonAliasId]
                       ,[ForeignId])
                 VALUES
                       (@KeyString
                       ,@LastName
                       ,@FirstName
                       ,@MiddleInitial
                       ,@Age
                       ,@Height
                       ,@Weight
                       ,@Race
                       ,@Sex
                       ,@Hair
                       ,@Eyes
                       ,@ResidentialAddress
                       ,@ResidentialCity
                       ,@ResidentialState
                       ,@ResidentialZip
                       ,@VerificationDate
                       ,@Offense
                       ,@OffenseLevel
                       ,@Absconder
                       ,@ConvictingJurisdiction
                       ,@Unverified
                       ,NEWID()
                       ,@UpdateDateTime
                       ,@UpdateDateTime
                       ,null
                       ,null
                       ,null)
	            END
	            ELSE
	            BEGIN
            UPDATE [dbo].[_com_centralaz_DpsMatch_Offender]
               SET [MiddleInitial] = @MiddleInitial
                  ,[Age] = @Age
                  ,[Height] = @Height
                  ,[Weight] = @Weight
                  ,[ResidentialAddress] = @ResidentialAddress
                  ,[ResidentialCity] = @ResidentialCity
                  ,[ResidentialState] = @ResidentialState
                  ,[ResidentialZip] = @ResidentialZip
                  ,[VerificationDate] = @VerificationDate
                  ,[Offense] = @Offense
                  ,[OffenseLevel] = @OffenseLevel
                  ,[Absconder] = @Absconder
                  ,[ConvictingJurisdiction] = @ConvictingJurisdiction
                  ,[Unverified] = @Unverified
                  ,[ModifiedDateTime] = @UpdateDateTime
             WHERE KeyString=@KeyString
	            END
" );

            Sql( @"
CREATE PROCEDURE [dbo].[_com_centralaz_spDpsMatch_Match]
            AS
            BEGIN
                SET NOCOUNT ON

                BEGIN TRANSACTION

                -- prevent stored proc from running simultaneously since we are using #temp tables
                EXEC sp_getapplock '_com_centralaz_spDpsMatch_Match'
                    ,'Exclusive'
                    ,'Transaction'
                    ,0

                -- Flags that enable the various compare functions
                DECLARE @compareByPartialName BIT = 1
                    ,@compareByFullFirstName BIT = 1
                    ,@compareByFullLastName BIT = 1
                    ,@compareByPostalCode BIT = 1
                    ,@compareByGender BIT = 1
                --
                -- Scores
                -- ones marked /**/ only added to score if already a potential match
                DECLARE @cScoreWeightPartialName INT = 10
                    ,@cScoreWeightFullFirstName INT = 15 /**/
			        ,@cScoreWeightFullLastName INT = 20 /**/
                    ,@cScoreWeightPostalCode INT = 20
                    ,@cScoreWeightGender INT = 30 /**/
                DECLARE @TotalCapacity INT = @cScoreWeightPartialName + @cScoreWeightFullFirstName + @cScoreWeightFullFirstName + @cScoreWeightPostalCode + @cScoreWeightGender
                --
                -- Guids that this proc uses
                DECLARE @cGROUPTYPE_FAMILY_GUID UNIQUEIDENTIFIER = '790E3215-3B10-442B-AF69-616C0DCB998E'
                    ,@cLOCATION_TYPE_HOME_GUID UNIQUEIDENTIFIER = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
                --
                -- Other Declarations
                DECLARE @processDateTime DATETIME = SYSDATETIME()
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
                Populate Temporary Tables for each match criteria (PartialName, Address)
                */

                -- Find Matches by looking at people with the exact same lastname and same first 2 chars of First/Nick name
                CREATE TABLE #OffenderMatchByNameTable (
                    Id INT NOT NULL IDENTITY(1, 1)
                    ,OffenderId INT NOT NULL
                    ,PersonAliasId INT NOT NULL
                    ,CONSTRAINT [pk_OffenderMatchByNameTable] PRIMARY KEY CLUSTERED (Id)
                    );

                INSERT INTO #OffenderMatchByNameTable (
                    OffenderId
                    ,PersonAliasId
                    )
                SELECT [e].[OffenderId]
                    ,[pa].[Id] [PersonAliasId]
                FROM (
                    SELECT [a].[First2]
                        ,[a].[LastName]
			            , [a].[OffenderId]
						,[a].[Sex]
                    FROM (
                        SELECT SUBSTRING([FirstName], 1, 2) [First2]
                            ,[LastName]
				            , [Id] [OffenderId]
							,[Sex]
                        FROM [_com_centralaz_DpsMatch_Offender] [p]
                        WHERE isnull([LastName], '') != ''
                            AND [FirstName] IS NOT NULL
                            AND LEN([FirstName]) >= 2
                        GROUP BY [LastName]
                            ,SUBSTRING([FirstName], 1, 2)
							,[Sex]
				            , [Id]
                        ) [a]
                    ) [e]
                JOIN [Person] [p] ON [p].[LastName] = [e].[LastName]
				AND (([p].[Gender]=1 and [e].[Sex]='M') or ([p].[Gender]=2 and [e].[Sex]='F'))
                    AND [p].[FirstName] LIKE (e.First2 + '%')
                JOIN [PersonAlias] [pa] ON [pa].[PersonId] = [p].[Id]
                WHERE [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
                    AND @compareByPartialName = 1

                -- Find Matches by looking at people with the exact same address (Location)
                CREATE TABLE #OffenderMatchByAddressTable (
                    Id INT NOT NULL IDENTITY(1, 1)
                    ,OffenderId NVARCHAR(100) NOT NULL
                    ,PersonAliasId INT NOT NULL
                    ,CONSTRAINT [pk_OffenderMatchByAddressTable] PRIMARY KEY CLUSTERED (Id)
                    );

                INSERT INTO #OffenderMatchByAddressTable (
                    OffenderId
                    ,PersonAliasId
                    )
                -- from the locations that have multiple potential matches, select the person records that are associated with that location
                -- in case the person has multiple home addresses, get the first one with preference given to MapLocation=true
                SELECT [so].[Id] [OffenderId]
                    ,[a].[Id] [PersonAliasId]
                FROM (
                    SELECT SUBSTRING([l].[PostalCode],0, 6) AS PostalCode
						,[p].[LastName]
						,[p].[Gender]
			            ,[pa].[Id]
                    FROM [Person] [p]
                    JOIN [GroupMember] [gm] ON [gm].[PersonId] = [p].[Id]
                    JOIN [Group] [g] ON [gm].[GroupId] = [g].[Id]
		            JOIN [PersonAlias] [pa] ON [gm].[PersonId] = [pa].[PersonId]
                    JOIN [GroupLocation] [gl] ON [gl].[GroupId] = [g].[id]
                    JOIN [Location] [l] ON [l].[Id] = [gl].[LocationId]
                        AND [g].[GroupTypeId] = @cGROUPTYPE_FAMILY_ID
                    WHERE [gl].[GroupLocationTypeValueId] = @cLOCATION_TYPE_HOME_ID
			            AND [pa].[AliasPersonId] = [pa].[PersonId] -- limit to only the primary alias
                    GROUP BY [gl].[LocationID]
			            ,[l].[PostalCode]
						,[p].[LastName]
						,[p].[Gender]
			            ,[pa].[Id]
                    ) [a]
                JOIN [_com_centralaz_DpsMatch_Offender] [so] ON CAST([so].[ResidentialZip] AS NVARCHAR(5)) = [a].[PostalCode]
				AND (([a].[Gender]=1 and [so].[Sex]='M') or ([a].[Gender]=2 and [so].[Sex]='F'))
				and [so].[LastName] = [a].[LastName]
	            WHERE @compareByPostalCode = 1

                -- get the original ConfidenceScore of the IgnoreUntilScoreChanges records so that we can un-ignore the ones that have a changed score
                DECLARE @IgnoreUntilScoreChangesTable TABLE (
                    Id INT
                    ,MatchPercentage INT
                    );

                INSERT INTO @IgnoreUntilScoreChangesTable
                SELECT [Id]
                    ,[MatchPercentage]
                FROM [_com_centralaz_DpsMatch_Match]
                WHERE [IsMatch] = 0;

                /* 
                Reset Scores for everybody. (We want to preserve each record's [IsMatch] value)
                */
                UPDATE [_com_centralaz_DpsMatch_Match]
                SET [MatchPercentage] = 0;

                /*
                Merge Results of Matches into Match Table
                */

                -- Update Match table with results of partial name match
                MERGE [_com_centralaz_DpsMatch_Match] AS TARGET
                USING (
                    SELECT [e1].[PersonAliasId] [PersonAliasId]
                        ,[e1].[OffenderId] [OffenderId]
                    FROM #OffenderMatchByNameTable [e1]
                    ) AS source(PersonAliasId, OffenderId)
                    ON (target.PersonAliasId = source.PersonAliasId)
			            AND (target.OffenderId = source.OffenderId)
                WHEN MATCHED
                    THEN
                        UPDATE
			            SET [MatchPercentage] = [MatchPercentage] + @cScoreWeightPartialName
				            ,[ModifiedDateTime] = @processDateTime
				WHEN NOT MATCHED
					THEN
						INSERT (
							PersonAliasId
							,OffenderId
							,MatchPercentage
							,ModifiedDateTime
							,CreatedDateTime
							,[Guid]
							)
						VALUES (
							source.PersonAliasId
							,source.OffenderId
							,@cScoreWeightPartialName
							,@processDateTime
							,@processDateTime
							,NEWID()
							);
                -- Update Match table with results of address (location) match
                MERGE [_com_centralaz_DpsMatch_Match] AS TARGET
                USING (
                    SELECT [e1].[PersonAliasId] [PersonAliasId]
                        ,[e1].[OffenderId] [OffenderId]
                    FROM #OffenderMatchByAddressTable [e1]
                    ) AS source(PersonAliasId, OffenderId)
                    ON (target.PersonAliasId = source.PersonAliasId)
			            AND (target.OffenderId = source.OffenderId)
                WHEN MATCHED
                    THEN
                        UPDATE
			            SET [MatchPercentage] = [MatchPercentage] + @cScoreWeightPostalCode
				            ,[ModifiedDateTime] = @processDateTime
				WHEN NOT MATCHED
					THEN
						INSERT (
							PersonAliasId
							,OffenderId
							,MatchPercentage
							,ModifiedDateTime
							,CreatedDateTime
							,[Guid]
							)
						VALUES (
							source.PersonAliasId
							,source.OffenderId
							,@cScoreWeightPostalCode
							,@processDateTime
							,@processDateTime
							,NEWID()
							);

                /*
                Add additional scores to people that are already potential matches 
                */
                -- Increment the score on potential matches that have the same FirstName (or NickName)
                -- put the updated ids into a temp list first to speed things up
                DECLARE @updatedIdsFirstName TABLE (id INT);

                INSERT INTO @updatedIdsFirstName
                SELECT pm.Id
                FROM _com_centralaz_DpsMatch_Match pm
                JOIN PersonAlias pa ON pa.Id = pm.PersonAliasId
                JOIN _com_centralaz_DpsMatch_Offender so ON so.Id = pm.OffenderId
                JOIN Person p ON p.Id = pa.PersonId
                WHERE (
                        (
                            p.FirstName = so.FirstName
                            AND isnull(p.FirstName, '') != ''
                            )
                        OR (
                            p.NickName = so.FirstName
                            AND isnull(p.NickName, '') != ''
                            )
                        )
                    AND @compareByFullFirstName = 1

                UPDATE [_com_centralaz_DpsMatch_Match]
                SET [MatchPercentage] = [MatchPercentage] + @cScoreWeightFullFirstName
                WHERE Id IN (
                        SELECT Id
                        FROM @updatedIdsFirstName
                        )

                -- Increment the score on potential matches that have the same LastName
                -- put the updated ids into a temp list first to speed things up
                DECLARE @updatedIdsLastName TABLE (id INT);

                INSERT INTO @updatedIdsLastName
                SELECT pm.Id
                FROM _com_centralaz_DpsMatch_Match pm
                JOIN PersonAlias pa ON pa.Id = pm.PersonAliasId
                JOIN _com_centralaz_DpsMatch_Offender so ON so.Id = pm.OffenderId
                JOIN Person p ON p.Id = pa.PersonId
                WHERE p.LastName = so.LastName
                    AND isnull(p.LastName, '') != ''
                    AND @compareByFullLastName = 1

                UPDATE [_com_centralaz_DpsMatch_Match]
                SET [MatchPercentage] = [MatchPercentage] + @cScoreWeightFullLastName
                WHERE Id IN (
                        SELECT Id
                        FROM @updatedIdsLastName
                        )

                -- Increment the score on potential matches that have the same gender
                -- put the updated ids into a temp list first to speed things up
                DECLARE @updatedIdsGender TABLE (id INT);

                INSERT INTO @updatedIdsGender
                SELECT pm.Id
					FROM _com_centralaz_DpsMatch_Match pm
					JOIN PersonAlias pa ON pa.Id = pm.PersonAliasId
					JOIN _com_centralaz_DpsMatch_Offender so ON so.Id = pm.OffenderId
					JOIN Person p ON p.Id = pa.PersonId
					WHERE ((p.Gender=1 and so.Sex='M') or (p.Gender=2 and so.Sex='F'))
						AND @compareByGender = 1

                UPDATE [_com_centralaz_DpsMatch_Match]
                SET [MatchPercentage] = [MatchPercentage] + @cScoreWeightGender
                WHERE Id IN (
                        SELECT Id
                        FROM @updatedIdsGender
                        )
                /*
                Explicitly clean up temp tables before the proc exits (vs. have SQL Server do it for us after the proc is done)
                */
                DROP TABLE #OffenderMatchByNameTable;

                DROP TABLE #OffenderMatchByAddressTable;

                COMMIT
            END

" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql( @"
                DROP PROCEDURE [dbo].[_com_centralaz_spDpsMatch_Match]
                DROP PROCEDURE [dbo].[_com_centralaz_spDpsMatch_Offender]
" );
        }
    }
}
