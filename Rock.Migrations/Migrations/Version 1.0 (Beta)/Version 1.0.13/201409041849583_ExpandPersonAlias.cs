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
    public partial class ExpandPersonAlias : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Rollups
            Sql( @"
    UPDATE [DefinedValue] set [Order] = 0 where [Guid] = '8B086A19-405A-451F-8D44-174E92D6B402' -- Check
    UPDATE [DefinedValue] set [Order] = 1 where [Guid] = 'F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93' -- Cash
    UPDATE [DefinedValue] set [Order] = 2 where [Guid] = '928A2E04-C77B-4282-888F-EC549CEE026A' -- Credit Card
    UPDATE [DefinedValue] set [Order] = 3 where [Guid] = 'DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6' -- ACH

    UPDATE [Attribute] SET [DefaultValue] = 'http://rock.organization.com/' WHERE [Guid] = '06E0E3FC-9A1C-43AF-8B3B-C760F9951012'
    UPDATE [Attribute] SET [DefaultValue] = 'http://www.organization.com/' WHERE [Guid] = '49AD7AD6-9BAC-4743-B1E8-B917F6271924'
" );

            Sql( @"
    /*
    <doc>
	    <summary>
 		    This function returns the primary person alias id for the person id given.
	    </summary>

	    <returns>
		    Int of the primary person alias id
	    </returns>
	    <remarks>
		
	    </remarks>
	    <code>
		    SELECT [dbo].[ufnUtility_GetPrimaryPersonAliasId](1)
	    </code>
    </doc>
    */

    CREATE FUNCTION [dbo].[ufnUtility_GetPrimaryPersonAliasId](@PersonId int) 

    RETURNS int AS

    BEGIN

	    RETURN ( 
			SELECT TOP 1 [Id] FROM [PersonAlias]
			WHERE [PersonId] = @PersonId AND [AliasPersonId] = @PersonId
		)

    END
" );

            AddColumn( "dbo.CommunicationRecipient", "PersonAliasId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.Communication", "SenderPersonAliasId", c => c.Int() );
            AddColumn( "dbo.Communication", "ReviewerPersonAliasId", c => c.Int() );
            AddColumn( "dbo.PrayerRequest", "RequestedByPersonAliasId", c => c.Int() );
            AddColumn( "dbo.PrayerRequest", "ApprovedByPersonAliasId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "AuthorizedPersonAliasId", c => c.Int() );
            AddColumn( "dbo.FinancialScheduledTransaction", "AuthorizedPersonAliasId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialPersonBankAccount", "PersonAliasId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialPersonSavedAccount", "PersonAliasId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialPledge", "PersonAliasId", c => c.Int() );
            AddColumn( "dbo.Auth", "PersonAliasId", c => c.Int() );
            AddColumn( "dbo.HtmlContent", "ApprovedByPersonAliasId", c => c.Int() );
            AddColumn( "dbo.MarketingCampaign", "ContactPersonAliasId", c => c.Int() );
            AddColumn( "dbo.Attendance", "PersonAliasId", c => c.Int() );
            AddColumn( "dbo.GroupLocation", "GroupMemberPersonAliasId", c => c.Int() );
            AddColumn( "dbo.Tag", "OwnerPersonAliasId", c => c.Int() );

            Sql( @"
    UPDATE [CommunicationRecipient] SET
          [PersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [PersonId] )

    UPDATE [Communication] SET
          [SenderPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [SenderPersonId] )
        , [ReviewerPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [ReviewerPersonId] )

    UPDATE [PrayerRequest] SET
          [RequestedByPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [RequestedByPersonId] )
        , [ApprovedByPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [ApprovedByPersonId] )

    UPDATE [FinancialTransaction] SET
          [AuthorizedPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [AuthorizedPersonId] )

    UPDATE [FinancialScheduledTransaction] SET
          [AuthorizedPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [AuthorizedPersonId] )
    
    UPDATE [FinancialPersonBankAccount] SET
          [PersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [PersonId] )
    
    UPDATE [FinancialPersonSavedAccount] SET
          [PersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [PersonId] )
    
    UPDATE [FinancialPledge] SET
          [PersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [PersonId] )

    UPDATE [Auth] SET
          [PersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [PersonId] )

    UPDATE [HtmlContent] SET
          [ApprovedByPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [ApprovedByPersonId] )

    UPDATE [MarketingCampaign] SET
          [ContactPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [ContactPersonId] )

    UPDATE [Attendance] SET
          [PersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [PersonId] )

    UPDATE [GroupLocation] SET
          [GroupMemberPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [GroupMemberPersonId] )

    UPDATE [Tag] SET
          [OwnerPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [OwnerId] )
" );
            DropForeignKey( "dbo.Communication", "ReviewerPersonId", "dbo.Person" );
            DropForeignKey( "dbo.Communication", "SenderPersonId", "dbo.Person" );
            DropForeignKey( "dbo.CommunicationRecipient", "PersonId", "dbo.Person" );
            DropForeignKey( "dbo.PrayerRequest", "ApprovedByPersonId", "dbo.Person" );
            DropForeignKey( "dbo.PrayerRequest", "RequestedByPersonId", "dbo.Person" );
            DropForeignKey( "dbo.FinancialTransaction", "AuthorizedPersonId", "dbo.Person" );
            DropForeignKey( "dbo.FinancialScheduledTransaction", "AuthorizedPersonId", "dbo.Person" );
            DropForeignKey( "dbo.FinancialPersonBankAccount", "PersonId", "dbo.Person" );
            DropForeignKey( "dbo.FinancialPersonSavedAccount", "PersonId", "dbo.Person" );
            DropForeignKey( "dbo.FinancialPledge", "PersonId", "dbo.Person" );
            DropForeignKey( "dbo.Auth", "PersonId", "dbo.Person" );
            DropForeignKey( "dbo.HtmlContent", "ApprovedByPersonId", "dbo.Person" );
            DropForeignKey( "dbo.MarketingCampaign", "ContactPersonId", "dbo.Person" );
            DropForeignKey( "dbo.Attendance", "PersonId", "dbo.Person" );
            DropForeignKey( "dbo.GroupLocation", "GroupMemberPersonId", "dbo.Person" );
            DropForeignKey( "dbo.Tag", "OwnerId", "dbo.Person" );

            DropIndex( "dbo.CommunicationRecipient", new[] { "PersonId" } );
            DropIndex( "dbo.Communication", new[] { "SenderPersonId" } );
            DropIndex( "dbo.Communication", new[] { "ReviewerPersonId" } );
            DropIndex( "dbo.PrayerRequest", new[] { "RequestedByPersonId" } );
            DropIndex( "dbo.PrayerRequest", new[] { "ApprovedByPersonId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "AuthorizedPersonId" } );
            DropIndex( "dbo.FinancialScheduledTransaction", new[] { "AuthorizedPersonId" } );
            DropIndex( "dbo.FinancialPersonBankAccount", new[] { "PersonId" } );
            DropIndex( "dbo.FinancialPersonSavedAccount", new[] { "PersonId" } );
            DropIndex( "dbo.FinancialPledge", new[] { "PersonId" } );
            DropIndex( "dbo.Auth", new[] { "PersonId" } );
            DropIndex( "dbo.HtmlContent", new[] { "ApprovedByPersonId" } );
            DropIndex( "dbo.MarketingCampaign", new[] { "ContactPersonId" } );
            DropIndex( "dbo.Attendance", new[] { "PersonId" } );
            DropIndex( "dbo.GroupLocation", new[] { "GroupMemberPersonId" } );
            DropIndex( "dbo.Tag", new[] { "OwnerId" } );

            DropColumn( "dbo.CommunicationRecipient", "PersonId" );
            DropColumn( "dbo.Communication", "SenderPersonId" );
            DropColumn( "dbo.Communication", "ReviewerPersonId" );
            DropColumn( "dbo.PrayerRequest", "RequestedByPersonId" );
            DropColumn( "dbo.PrayerRequest", "ApprovedByPersonId" );
            DropColumn( "dbo.FinancialTransaction", "AuthorizedPersonId" );
            DropColumn( "dbo.FinancialScheduledTransaction", "AuthorizedPersonId" );
            DropColumn( "dbo.FinancialPersonBankAccount", "PersonId" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "PersonId" );
            DropColumn( "dbo.FinancialPledge", "PersonId" );
            DropColumn( "dbo.Auth", "PersonId" );
            DropColumn( "dbo.HtmlContent", "ApprovedByPersonId" );
            DropColumn( "dbo.MarketingCampaign", "ContactPersonId" );
            DropColumn( "dbo.Attendance", "PersonId" );
            DropColumn( "dbo.GroupLocation", "GroupMemberPersonId" );
            DropColumn( "dbo.Tag", "OwnerId" );

            CreateIndex( "dbo.CommunicationRecipient", "PersonAliasId" );
            CreateIndex( "dbo.Communication", "SenderPersonAliasId" );
            CreateIndex( "dbo.Communication", "ReviewerPersonAliasId" );
            CreateIndex( "dbo.PrayerRequest", "RequestedByPersonAliasId" );
            CreateIndex( "dbo.PrayerRequest", "ApprovedByPersonAliasId" );
            CreateIndex( "dbo.FinancialTransaction", "AuthorizedPersonAliasId" );
            CreateIndex( "dbo.FinancialScheduledTransaction", "AuthorizedPersonAliasId" );
            CreateIndex( "dbo.FinancialPersonBankAccount", "PersonAliasId" );
            CreateIndex( "dbo.FinancialPersonSavedAccount", "PersonAliasId" );
            CreateIndex( "dbo.FinancialPledge", "PersonAliasId" );
            CreateIndex( "dbo.Auth", "PersonAliasId" );
            CreateIndex( "dbo.HtmlContent", "ApprovedByPersonAliasId" );
            CreateIndex( "dbo.MarketingCampaign", "ContactPersonAliasId" );
            CreateIndex( "dbo.Attendance", "PersonAliasId" );
            CreateIndex( "dbo.GroupLocation", "GroupMemberPersonAliasId" );
            CreateIndex( "dbo.Tag", "OwnerPersonAliasId" );

            AddForeignKey( "dbo.Communication", "ReviewerPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.Communication", "SenderPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.CommunicationRecipient", "PersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.PrayerRequest", "ApprovedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.PrayerRequest", "RequestedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "AuthorizedPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.FinancialScheduledTransaction", "AuthorizedPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.FinancialPersonBankAccount", "PersonAliasId", "dbo.PersonAlias", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.FinancialPersonSavedAccount", "PersonAliasId", "dbo.PersonAlias", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.FinancialPledge", "PersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.Auth", "PersonAliasId", "dbo.PersonAlias", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.HtmlContent", "ApprovedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.MarketingCampaign", "ContactPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.Attendance", "PersonAliasId", "dbo.PersonAlias", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.GroupLocation", "GroupMemberPersonAliasId", "dbo.PersonAlias", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.Tag", "OwnerPersonAliasId", "dbo.PersonAlias", "Id" );

            // change scheme owner if not dbo
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
							[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
							AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId)) 
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
							[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
							AND pa.[PersonId] = @PersonId 
							AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
						) a ON [AttendedSunday] = s.[SundayDate]

		) [CheckinDates]
		GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
		OPTION (MAXRECURSION 1000)
	END

	
END
" );

            // change scheme owner if not dbo
            Sql( @" DECLARE @ObjectName varchar(50) = 'spCheckin_WeeksAttendedInDuration'
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
    -- create stored proc for attendance duration
    /*
    <doc>
	    <summary>
 		    This function returns the number of weekends a member of a family has attended a weekend service
		    in the last X weeks.
	    </summary>

	    <returns>
		    * Number of weeks
	    </returns>
	    <param name=""PersonId"" datatype=""int"">The person id to use</param>
	    <param name=""WeekDuration"" datatype=""int"">The number of weeks to use as the duration (default 16)</param>
	    <remarks>	
	    </remarks>
	    <code>
		    EXEC [dbo].[spCheckin_WeeksAttendedInDuration] 2 -- Ted Decker
	    </code>
    </doc>
    */

    ALTER PROCEDURE [dbo].[spCheckin_WeeksAttendedInDuration]
	    @PersonId int
	    ,@WeekDuration int = 16
    AS
    BEGIN
	
        DECLARE @LastSunday datetime 

        SET @LastSunday = [dbo].[ufnUtility_GetPreviousSundayDate]()

        SELECT 
	        COUNT(DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) )
        FROM
	        [Attendance] a
	        INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
        WHERE 
	        [GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
	        AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId))
	        AND a.[StartDateTime] BETWEEN DATEADD(WEEK, (@WeekDuration * -1), @LastSunday) AND @LastSunday 

    END
" );

            Sql( @" DECLARE @ObjectName varchar(50) = 'spCrm_PersonMerge'
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
 			This procedure merges the data from the non-primary person to the primary person.  It
			is used when merging people in Rock and should never be used outside of that process. 
		</summary>

		<returns>
		</returns>
		<param name=""Old Id"" datatype=""int"">The person id of the non-primary Person being merged</param>
		<param name=""New Id"" datatype=""int"">The person id of the primary Person being merged</param>
		<remarks>	
			Uses the following constants:
				* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
				* Group Role - Known Relationship Owner: 7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42
				* Group Role - Implied Relationship Owner: CB9A0E14-6FCF-4C07-A49A-D7873F45E196
		</remarks>
		<code>
		</code>
	</doc>
	*/

	ALTER PROCEDURE [dbo].[spCrm_PersonMerge]
		@OldId int
		, @NewId int

	AS
	BEGIN

		DECLARE @OldGuid uniqueidentifier
		DECLARE @NewGuid uniqueidentifier

		SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
		SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

		IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
		BEGIN

			DECLARE @PersonEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )
			DECLARE @PersonFieldTypeId INT = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )

			-- Move/Update Known Relationships
			EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42'

			-- Move/Update Implied Relationships
			EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, 'CB9A0E14-6FCF-4C07-A49A-D7873F45E196'

			-- Group Member
			-----------------------------------------------------------------------------------------------
			-- Update any group members associated to old person to the new person where the new is not 
			-- already in the group with the same role
			UPDATE GMO
				SET [PersonId] = @NewId
			FROM [GroupMember] GMO
				INNER JOIN [GroupTypeRole] GTR
					ON GTR.[Id] = GMO.[GroupRoleId]
				LEFT OUTER JOIN [GroupMember] GMN
					ON GMN.[GroupId] = GMO.[GroupId]
					AND GMN.[PersonId] = @NewId
					AND (GTR.[MaxCount] <= 1 OR GMN.[GroupRoleId] = GMO.[GroupRoleId])
			WHERE GMO.[PersonId] = @OldId
				AND GMN.[Id] IS NULL

			-- Delete any group members not updated (already existed with new id)
			DELETE [GroupMember]
			WHERE [PersonId] = @OldId

			-- User Login
			-----------------------------------------------------------------------------------------------
			-- Update any user logins associated with old id to be associated with primary person
			UPDATE [UserLogin]
			SET [PersonId] = @NewId
			WHERE [PersonId] = @OldId

			-- Attribute
			-----------------------------------------------------------------------------------------------
			-- Update any attribute value that is associated to the old person to be associated to the new 
			-- person. The 'PersonAttribute' stores it's values as person alias ids, so really shouldn't be
			-- any values found here
			UPDATE V
				SET [EntityId] = @NewId
			FROM [Attribute] A
				INNER JOIN [Attributevalue] V
					ON V.[AttributeId] = A.[Id]
					AND V.[EntityId] = @OldId
				LEFT OUTER JOIN [Attributevalue] NV
					ON NV.[AttributeId] = A.[Id]
					AND NV.[EntityId] = @NewId
			WHERE A.[EntityTypeId] = @PersonEntityTypeId
				AND NV.[Id] IS NULL

			DELETE V
			FROM [Attribute] A
				INNER JOIN [Attributevalue] V
					ON V.[AttributeId] = A.[Id]
					AND V.[EntityId] = @OldId
			WHERE A.[EntityTypeId] = @PersonEntityTypeId

			-- Audit
			-----------------------------------------------------------------------------------------------
			-- Update any audit records that were associated to the old person to be associated to the new person
			UPDATE [Audit] SET [EntityId] = @NewId
			WHERE [EntityTypeId] = @PersonEntityTypeId
			AND [EntityId] = @OldId

			-- Auth
			-----------------------------------------------------------------------------------------------
			-- Update any auth records that were associated to the old person to be associated to the new person
			-- There is currently not any UI to set security associated to person, so really shouldn't be
			-- any values here to update
			UPDATE A
				SET [EntityId] = @NewId
			FROM [Auth] A
				LEFT OUTER JOIN [Auth] NA
					ON NA.[EntityTypeId] = A.[EntityTypeId]
					AND NA.[EntityId] = @NewId
					AND NA.[Action] = A.[Action]
			WHERE A.[EntityTypeId] = @PersonEntityTypeId
				AND A.[EntityId] = @OldId
				AND NA.[Id] IS NULL

			DELETE [Auth]
			WHERE [EntityTypeId] = @PersonEntityTypeId
			AND [EntityId] = @OldId

			-- Entity Set
			-----------------------------------------------------------------------------------------------
			-- Update any entity set items that are associated to the old person to be associated to the new 
			-- person. 
			UPDATE I
				SET [EntityId] = @NewId
			FROM [EntitySet] S
				INNER JOIN [EntitySetItem] I
					ON I.[EntitySetId] = S.[Id]
					AND I.[EntityId] = @OldId
				LEFT OUTER JOIN [EntitySetItem] NI
					ON NI.[EntitySetId] = S.[Id]
					AND NI.[EntityId] = @NewId
			WHERE S.[EntityTypeId] = @PersonEntityTypeId
				AND NI.[Id] IS NULL

			DELETE I
			FROM [EntitySet] S
				INNER JOIN [EntitySetItem] I
					ON I.[EntitySetId] = S.[Id]
					AND I.[EntityId] = @OldId
			WHERE S.[EntityTypeId] = @PersonEntityTypeId

			-- Following
			-----------------------------------------------------------------------------------------------
			-- Update any followings that are associated to the old person to be associated to the new 
			-- person. 
			UPDATE F
				SET [EntityId] = @NewId
			FROM [Following] F
				LEFT OUTER JOIN [Following] NF
					ON NF.[EntityTypeId] = F.[EntityTypeId]
					AND NF.[EntityId] = @NewId
					AND NF.[PersonAliasId] = F.[PersonAliasId]
			WHERE F.[EntityTypeId] = @PersonEntityTypeId
				AND F.[EntityId] = @OldId
				AND NF.[Id] IS NULL

			DELETE [Following]
			WHERE [EntityTypeId] = @PersonEntityTypeId
			AND [EntityId] = @OldId

			-- History
			-----------------------------------------------------------------------------------------------
			-- Update any history that is associated to the old person to be associated to the new person
			UPDATE [History] SET [EntityId] = @NewId
			WHERE [EntityTypeId] = @PersonEntityTypeId
			AND [EntityId] = @OldId

			-- Note
			-----------------------------------------------------------------------------------------------
			-- Update any note that is associated to the old person to be associated to the new person
			UPDATE N
				SET [EntityId] = @NewId
			FROM [NoteType] NT
				INNER JOIN [Note] N
					ON N.[NoteTypeId] = NT.[Id]
					AND N.[EntityId] = @OldId
			WHERE NT.[EntityTypeId] = @PersonEntityTypeId
		
			-- Tags
			-----------------------------------------------------------------------------------------------
			-- Update any tags associated to the old person to be associated to the new person as long as 
			-- same tag does not already exist for new person
			UPDATE TIO
				SET [EntityGuid] = @NewGuid
			FROM [Tag] T
				INNER JOIN [TaggedItem] TIO
					ON TIO.[TagId] = T.[Id]
					AND TIO.[EntityGuid] = @OldGuid
				LEFT OUTER JOIN [TaggedItem] TIN
					ON TIN.[TagId] = T.[Id]
					AND TIN.[EntityGuid] = @NewGuid
			WHERE T.[EntityTypeId] = @PersonEntityTypeId
				AND TIN.[Id] IS NULL

			-- Delete any tagged items still associated with old person (new person had same tag)
			DELETE TIO
			FROM [Tag] T
				INNER JOIN [TaggedItem] TIO
					ON TIO.[TagId] = T.[Id]
					AND TIO.[EntityGuid] = @OldGuid
			WHERE T.[EntityTypeId] = @PersonEntityTypeId


			-- Remaining Tables
			-----------------------------------------------------------------------------------------------
			-- Update any column on any table that has a foreign key relationship to the Person table's Id
			-- column ( Core tables are handled explicitely above, so this should only include custom tables )

			DECLARE @Sql varchar(max)

			DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
			SELECT 
				' UPDATE ' + tso.name +
				' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
				' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
			FROM sys.foreign_key_columns kc
				INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
				INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
				INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
				INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
				INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
			WHERE so.name = 'Person'
				AND rac.name = 'Id'
				AND tso.name NOT IN (
					 'GroupMember'
					,'PhoneNumber'
					,'UserLogin'
				)

			OPEN ForeignKeyCursor

			FETCH NEXT
			FROM ForeignKeyCursor
			INTO @Sql

			WHILE (@@FETCH_STATUS <> -1)
			BEGIN

				IF (@@FETCH_STATUS = 0)
				BEGIN

					EXEC(@Sql)
			
				END
		
				FETCH NEXT
				FROM ForeignKeyCursor
				INTO @Sql

			END

			CLOSE ForeignKeyCursor
			DEALLOCATE ForeignKeyCursor


			-- Person
			-----------------------------------------------------------------------------------------------
			-- Delete the old person record.  By this time it should not have any relationships 
			-- with other tables 

			DELETE Person
			WHERE [Id] = @OldId

		END

	END
" );

            Sql( @" DECLARE @ObjectName varchar(50) = 'spPersonMerge'
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
    DROP PROCEDURE [dbo].[spPersonMerge]
" );

            Sql( @" DECLARE @ObjectName varchar(50) = 'spFinance_ContributionStatementQuery'
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
 			This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions
			The StatementGenerator utility uses this procedure along with querying transactions thru REST to generate statements
		</summary>

		<returns>
			* PersonId
			* GroupId
			* AddressPersonNames
			* Street1
			* Street2
			* City
			* State
			* PostalCode
			* StartDate
			* EndDate
			* CustomMessage1
			* CustomMessage2
		</returns>
		<param name=""StartDate"" datatype=""datetime"">The starting date of the date range</param>
		<param name=""EndDate"" datatype=""datetime"">The ending date of the date range</param>
		<param name=""AccountIds"" datatype=""varchar(max)"">Comma delimited list of account ids. NULL means all</param>
		<param name=""PersonId"" datatype=""int"">Person the statement if for. NULL means all persons that have transactions for the date range</param>
		<param name=""OrderByPostalCode"" datatype=""int"">Set to 1 to have the results sorted by PostalCode, 0 for no particular order</param>
		<remarks>	
			Uses the following constants:
				* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
				* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
				* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
		</remarks>
		<code>
			EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1  -- year 2014 statements for all persons
		</code>
	</doc>
	*/
	ALTER PROCEDURE [dbo].[spFinance_ContributionStatementQuery]
		@StartDate datetime
		, @EndDate datetime
		, @AccountIds varchar(max) 
		, @PersonId int -- NULL means all persons
		, @OrderByPostalCode bit
	AS
	BEGIN
		DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'	
		DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'

		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;

		;WITH tranListCTE
		AS
		(
			SELECT  
				[pa].[PersonId] 
			FROM 
				[FinancialTransaction] [ft]
			INNER JOIN 
				[FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
			INNER JOIN 
				[PersonAlias] [pa] ON [pa].[id] = [ft].[AuthorizedPersonAliasId]
			WHERE 
				([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
			AND 
				(
					(@AccountIds is null)
					OR
					(ftd.[AccountId] in (select * from ufnUtility_CsvToTable(@AccountIds)))
				)
		)

		SELECT 
			  [pg].[PersonId]
			, [pg].[GroupId]
			, [pn].[PersonNames] [AddressPersonNames]
			, [l].[Street1]
			, [l].[Street2]
			, [l].[City]
			, [l].[State]
			, [l].[PostalCode]
			, @StartDate [StartDate]
			, @EndDate [EndDate]
			, null [CustomMessage1]
			, null [CustomMessage2]
		FROM (
			-- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
			-- These are Persons that give as part of a Group.  For example, Husband and Wife
			SELECT DISTINCT
				null [PersonId] 
				, [g].[Id] [GroupId]
			FROM 
				[Person] [p]
			INNER JOIN 
				[Group] [g] ON [p].[GivingGroupId] = [g].[Id]
			WHERE 
				[p].[Id] in (SELECT * FROM tranListCTE)
			UNION
			-- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
			-- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
			-- to determine which address(es) the statements need to be mailed to 
			SELECT  
				[p].[Id] [PersonId],
				[g].[Id] [GroupId]
			FROM
				[Person] [p]
			JOIN 
				[GroupMember] [gm]
			ON 
				[gm].[PersonId] = [p].[Id]
			JOIN 
				[Group] [g]
			ON 
				[gm].[GroupId] = [g].[Id]
			WHERE
				[p].[GivingGroupId] is null
			AND
				[g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
			AND [p].[Id] IN (SELECT * FROM tranListCTE)
		) [pg]
		CROSS APPLY 
			[ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId]) [pn]
		JOIN 
			[GroupLocation] [gl] 
		ON 
			[gl].[GroupId] = [pg].[GroupId]
		JOIN
			[Location] [l]
		ON 
			[l].[Id] = [gl].[LocationId]
		WHERE 
			[gl].[IsMailingLocation] = 1
		AND
			[gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
		AND
			(
				(@personId is null) 
			OR 
				([pg].[PersonId] = @personId)
			)
		ORDER BY
		CASE WHEN @OrderByPostalCode = 1 THEN PostalCode END
	END
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.Communication", "ReviewerPersonId", c => c.Int() );
            AddColumn( "dbo.Communication", "SenderPersonId", c => c.Int() );
            AddColumn( "dbo.CommunicationRecipient", "PersonId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.PrayerRequest", "ApprovedByPersonId", c => c.Int() );
            AddColumn( "dbo.PrayerRequest", "RequestedByPersonId", c => c.Int() );
            AddColumn( "dbo.FinancialPledge", "PersonId", c => c.Int() );
            AddColumn( "dbo.FinancialPersonSavedAccount", "PersonId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialPersonBankAccount", "PersonId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialScheduledTransaction", "AuthorizedPersonId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.FinancialTransaction", "AuthorizedPersonId", c => c.Int() );
            AddColumn( "dbo.MarketingCampaign", "ContactPersonId", c => c.Int() );
            AddColumn( "dbo.HtmlContent", "ApprovedByPersonId", c => c.Int() );
            AddColumn( "dbo.Auth", "PersonId", c => c.Int() );
            AddColumn( "dbo.Attendance", "PersonId", c => c.Int() );
            AddColumn( "dbo.Tag", "OwnerId", c => c.Int() );
            AddColumn( "dbo.GroupLocation", "GroupMemberPersonId", c => c.Int() );

            Sql( @"
    UPDATE [CommunicationRecipient] SET
          [PersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [PersonAliasId] )

    UPDATE [Communication] SET
          [SenderPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [SenderPersonAliasId] )
        , [ReviewerPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [ReviewerPersonAliasId] )

    UPDATE [PrayerRequest] SET
          [RequestedByPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [RequestedByPersonAliasId] )
        , [ApprovedByPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [ApprovedByPersonAliasId] )
    
    UPDATE [FinancialPledge] SET
          [PersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [PersonAliasId] )

    UPDATE [FinancialPersonSavedAccount] SET
          [PersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [PersonAliasId] )

    UPDATE [FinancialPersonBankAccount] SET
          [PersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [PersonAliasId] )

    UPDATE [FinancialScheduledTransaction] SET
          [AuthorizedPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [AuthorizedPersonAliasId] )

    UPDATE [FinancialTransaction] SET
          [AuthorizedPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [AuthorizedPersonAliasId] )

    UPDATE [MarketingCampaign] SET
          [ContactPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [ContactPersonAliasId] )

    UPDATE [HtmlContent] SET
          [ApprovedByPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [ApprovedByPersonAliasId] )

    UPDATE [Auth] SET
          [PersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [PersonAliasId] )

    UPDATE [Attendance] SET
          [PersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [PersonAliasId] )

    UPDATE [Tag] SET
          [OwnerId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [OwnerPersonAliasId] )

    UPDATE [GroupLocation] SET
          [GroupMemberPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [GroupMemberPersonAliasId] )
" );

            DropForeignKey( "dbo.CommunicationRecipient", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Communication", "SenderPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Communication", "ReviewerPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PrayerRequest", "RequestedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PrayerRequest", "ApprovedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.FinancialPledge", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.FinancialPersonSavedAccount", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.FinancialPersonBankAccount", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.FinancialScheduledTransaction", "AuthorizedPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.FinancialTransaction", "AuthorizedPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MarketingCampaign", "ContactPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.HtmlContent", "ApprovedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Auth", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Attendance", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Tag", "OwnerPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupLocation", "GroupMemberPersonAliasId", "dbo.PersonAlias" );

            DropIndex( "dbo.Communication", new[] { "ReviewerPersonAliasId" } );
            DropIndex( "dbo.Communication", new[] { "SenderPersonAliasId" } );
            DropIndex( "dbo.CommunicationRecipient", new[] { "PersonAliasId" } );
            DropIndex( "dbo.PrayerRequest", new[] { "ApprovedByPersonAliasId" } );
            DropIndex( "dbo.PrayerRequest", new[] { "RequestedByPersonAliasId" } );
            DropIndex( "dbo.FinancialPledge", new[] { "PersonAliasId" } );
            DropIndex( "dbo.FinancialPersonSavedAccount", new[] { "PersonAliasId" } );
            DropIndex( "dbo.FinancialPersonBankAccount", new[] { "PersonAliasId" } );
            DropIndex( "dbo.FinancialScheduledTransaction", new[] { "AuthorizedPersonAliasId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "AuthorizedPersonAliasId" } );
            DropIndex( "dbo.MarketingCampaign", new[] { "ContactPersonAliasId" } );
            DropIndex( "dbo.HtmlContent", new[] { "ApprovedByPersonAliasId" } );
            DropIndex( "dbo.Auth", new[] { "PersonAliasId" } );
            DropIndex( "dbo.Attendance", new[] { "PersonAliasId" } );
            DropIndex( "dbo.Tag", new[] { "OwnerPersonAliasId" } );
            DropIndex( "dbo.GroupLocation", new[] { "GroupMemberPersonAliasId" } );

            DropColumn( "dbo.Communication", "ReviewerPersonAliasId" );
            DropColumn( "dbo.Communication", "SenderPersonAliasId" );
            DropColumn( "dbo.CommunicationRecipient", "PersonAliasId" );
            DropColumn( "dbo.PrayerRequest", "ApprovedByPersonAliasId" );
            DropColumn( "dbo.PrayerRequest", "RequestedByPersonAliasId" );
            DropColumn( "dbo.FinancialPledge", "PersonAliasId" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "PersonAliasId" );
            DropColumn( "dbo.FinancialPersonBankAccount", "PersonAliasId" );
            DropColumn( "dbo.FinancialScheduledTransaction", "AuthorizedPersonAliasId" );
            DropColumn( "dbo.FinancialTransaction", "AuthorizedPersonAliasId" );
            DropColumn( "dbo.MarketingCampaign", "ContactPersonAliasId" );
            DropColumn( "dbo.HtmlContent", "ApprovedByPersonAliasId" );
            DropColumn( "dbo.Auth", "PersonAliasId" );
            DropColumn( "dbo.Attendance", "PersonAliasId" );
            DropColumn( "dbo.Tag", "OwnerPersonAliasId" );
            DropColumn( "dbo.GroupLocation", "GroupMemberPersonAliasId" );

            CreateIndex( "dbo.Communication", "ReviewerPersonId" );
            CreateIndex( "dbo.Communication", "SenderPersonId" );
            CreateIndex( "dbo.CommunicationRecipient", "PersonId" );
            CreateIndex( "dbo.PrayerRequest", "ApprovedByPersonId" );
            CreateIndex( "dbo.PrayerRequest", "RequestedByPersonId" );
            CreateIndex( "dbo.FinancialPledge", "PersonId" );
            CreateIndex( "dbo.FinancialPersonSavedAccount", "PersonId" );
            CreateIndex( "dbo.FinancialPersonBankAccount", "PersonId" );
            CreateIndex( "dbo.FinancialScheduledTransaction", "AuthorizedPersonId" );
            CreateIndex( "dbo.FinancialTransaction", "AuthorizedPersonId" );
            CreateIndex( "dbo.MarketingCampaign", "ContactPersonId" );
            CreateIndex( "dbo.HtmlContent", "ApprovedByPersonId" );
            CreateIndex( "dbo.Auth", "PersonId" );
            CreateIndex( "dbo.Attendance", "PersonId" );
            CreateIndex( "dbo.Tag", "OwnerId" );
            CreateIndex( "dbo.GroupLocation", "GroupMemberPersonId" );

            AddForeignKey( "dbo.CommunicationRecipient", "PersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.Communication", "SenderPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.Communication", "ReviewerPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.PrayerRequest", "RequestedByPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.PrayerRequest", "ApprovedByPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.FinancialPledge", "PersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.FinancialPersonSavedAccount", "PersonId", "dbo.Person", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.FinancialPersonBankAccount", "PersonId", "dbo.Person", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.FinancialScheduledTransaction", "AuthorizedPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "AuthorizedPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.MarketingCampaign", "ContactPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.HtmlContent", "ApprovedByPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.Auth", "PersonId", "dbo.Person", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.Attendance", "PersonId", "dbo.Person", "Id", cascadeDelete: true );
            AddForeignKey( "dbo.Tag", "OwnerId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.GroupLocation", "GroupMemberPersonId", "dbo.Person", "Id", cascadeDelete: true );

            Sql( @"
    DROP FUNCTION [dbo].[ufnUtility_GetPrimaryPersonAliasId]
" );

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
						WHERE 
							[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
							AND a.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId)) 
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
						WHERE 
							[GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
							AND a.[PersonId] = @PersonId 
							AND a.[StartDateTime] BETWEEN @StartDay AND @LastDay
						) a ON [AttendedSunday] = s.[SundayDate]

		) [CheckinDates]
		GROUP BY DATEPART(month, [SundayDate]), DATEPART(year, [SundayDate])
		OPTION (MAXRECURSION 1000)
	END

END
" );

            Sql( @"
    -- create stored proc for attendance duration
    /*
    <doc>
	    <summary>
 		    This function returns the number of weekends a member of a family has attended a weekend service
		    in the last X weeks.
	    </summary>

	    <returns>
		    * Number of weeks
	    </returns>
	    <param name=""PersonId"" datatype=""int"">The person id to use</param>
	    <param name=""WeekDuration"" datatype=""int"">The number of weeks to use as the duration (default 16)</param>
	    <remarks>	
	    </remarks>
	    <code>
		    EXEC [dbo].[spCheckin_WeeksAttendedInDuration] 2 -- Ted Decker
	    </code>
    </doc>
    */

    ALTER PROCEDURE [dbo].[spCheckin_WeeksAttendedInDuration]
	    @PersonId int
	    ,@WeekDuration int = 16
    AS
    BEGIN
	
        DECLARE @LastSunday datetime 

        SET @LastSunday = [dbo].[ufnUtility_GetPreviousSundayDate]()

        SELECT 
	        COUNT(DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) )
        FROM
	        [Attendance] a
        WHERE 
	        [GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
	        AND a.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId))
	        AND a.[StartDateTime] BETWEEN DATEADD(WEEK, (@WeekDuration * -1), @LastSunday) AND @LastSunday 

    END
" );

            Sql( @"
    /*
    <doc>
	    <summary>
 		    This procedure merges the data from the non-primary person to the primary person.  It
		    is used when merging people in Rock and should never be used outside of that process. 
	    </summary>

	    <returns>
	    </returns>
	    <param name=""Old Id"" datatype=""int"">The person id of the non-primary Person being merged</param>
	    <param name=""New Id"" datatype=""int"">The person id of the rimary Person being merged</param>
	    <remarks>	
		    Uses the following constants:
			    * Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			    * Group Role - Known Relationship Owner: 7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42
			    * Group Role - Implied Relationship Owner: CB9A0E14-6FCF-4C07-A49A-D7873F45E196
	    </remarks>
	    <code>
	    </code>
    </doc>
    */

    ALTER PROCEDURE [dbo].[spCrm_PersonMerge]
        @OldId int
	    , @NewId int

    AS
    BEGIN

	    DECLARE @OldGuid uniqueidentifier
	    DECLARE @NewGuid uniqueidentifier

	    SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
	    SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

	    IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
	    BEGIN

		    DECLARE @PersonEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )
		    DECLARE @PersonFieldTypeId INT = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )

		    -- Authorization
		    -----------------------------------------------------------------------------------------------
		    -- Update any authorizations associated to old person that do not already have a matching 
		    -- authorization for the new person
		    UPDATE AO
			    SET [PersonId] = @NewId
		    FROM [Auth] AO
			    LEFT OUTER JOIN [Auth] AN
				    ON AN.[PersonId] = @NewId
				    AND AN.[EntityTypeId] = AO.[EntityTypeId]
				    AND AN.[EntityId] = AO.[EntityId]
				    AND AN.[Action] = AO.[Action]
				    AND AN.[AllowOrDeny] = AO.[AllowOrDeny]
				    AND AN.[SpecialRole] = AO.[SpecialRole]
		    WHERE AO.[PersonId] = @OldId
			    AND AN.[Id] IS NULL

		    -- Delete any authorizations not updated to new person
		    DELETE [Auth]
		    WHERE [PersonId] = @OldId

		    -- Category
		    -----------------------------------------------------------------------------------------------
		    -- Currently UI does not allow categorizing people, but if it does in the future, would need 
		    -- to add script to handle merge


		    -- Communication Recipient
		    -----------------------------------------------------------------------------------------------
		    -- Update any communication recipients associated to old person to the new person where the new
		    -- person does not already have the recipient record
		    UPDATE CRO
			    SET [PersonId] = @NewId
		    FROM [CommunicationRecipient] CRO
			    LEFT OUTER JOIN [CommunicationRecipient] CRN
				    ON CRN.[CommunicationId] = CRO.[CommunicationId]
				    AND CRN.[PersonId] = @NewId
		    WHERE CRO.[PersonId] = @OldId
			    AND CRN.[Id] IS NULL

		    -- Delete any remaining recipents that were not updated
		    DELETE [CommunicationRecipient]
		    WHERE [PersonId] = @OldId

		    -- Move/Update Known Relationships
		    EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42'

		    -- Move/Update Implied Relationships
		    EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, 'CB9A0E14-6FCF-4C07-A49A-D7873F45E196'

		    -- Group Member
		    -----------------------------------------------------------------------------------------------
		    -- Update any group members associated to old person to the new person where the new is not 
		    -- already in the group with the same role
		    UPDATE GMO
			    SET [PersonId] = @NewId
		    FROM [GroupMember] GMO
			    INNER JOIN [GroupTypeRole] GTR
				    ON GTR.[Id] = GMO.[GroupRoleId]
			    LEFT OUTER JOIN [GroupMember] GMN
				    ON GMN.[GroupId] = GMO.[GroupId]
				    AND GMN.[PersonId] = @NewId
				    AND (GTR.[MaxCount] <= 1 OR GMN.[GroupRoleId] = GMO.[GroupRoleId])
		    WHERE GMO.[PersonId] = @OldId
			    AND GMN.[Id] IS NULL

		    -- Delete any group members not updated (already existed with new id)
		    DELETE [GroupMember]
		    WHERE [PersonId] = @OldId
		
		    -- Note
		    -----------------------------------------------------------------------------------------------
		    -- Update any note that is associated to the old person to be associated to the new person
		    UPDATE N
			    SET [EntityId] = @NewId
		    FROM [NoteType] NT
			    INNER JOIN [Note] N
				    ON N.[NoteTypeId] = NT.[Id]
				    AND N.[EntityId] = @OldId
		    WHERE NT.[EntityTypeId] = @PersonEntityTypeId


		    -- History
		    -----------------------------------------------------------------------------------------------
		    -- Update any history that is associated to the old person to be associated to the new person
		    UPDATE [History] SET [EntityId] = @NewId
		    WHERE [EntityTypeId] = @PersonEntityTypeId
		    AND [EntityId] = @OldId

		    -- Tags
		    -----------------------------------------------------------------------------------------------
		    -- Update any tags associated to the old person to be associated to the new person as long as 
		    -- same tag does not already exist for new person
		    UPDATE TIO
			    SET [EntityGuid] = @NewGuid
		    FROM [Tag] T
			    INNER JOIN [TaggedItem] TIO
				    ON TIO.[TagId] = T.[Id]
				    AND TIO.[EntityGuid] = @OldGuid
			    LEFT OUTER JOIN [TaggedItem] TIN
				    ON TIN.[TagId] = T.[Id]
				    AND TIN.[EntityGuid] = @NewGuid
		    WHERE T.[EntityTypeId] = @PersonEntityTypeId
			    AND TIN.[Id] IS NULL

		    -- Delete any tagged items still associated with old person (new person had same tag)
		    DELETE TIO
		    FROM [Tag] T
			    INNER JOIN [TaggedItem] TIO
				    ON TIO.[TagId] = T.[Id]
				    AND TIO.[EntityGuid] = @OldGuid
		    WHERE T.[EntityTypeId] = @PersonEntityTypeId

		    -- If old person and new person have tags with the same name for the same entity type,
		    -- update the old person's tagged items to use the new person's tag
		    UPDATE TIO
			    SET [TagId] = TIN.[Id]
		    FROM [Tag] T
			    INNER JOIN [Tag] TN
				    ON TN.[EntityTypeId] = T.[EntityTypeId]
				    AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
				    AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
				    AND TN.[Name] = T.[Name]
				    AND TN.[OwnerId] = @NewId
			    INNER JOIN [TaggedItem] TIO
				    ON TIO.[TagId] = T.[Id]
			    LEFT OUTER JOIN [TaggedItem] TIN
				    ON TIN.[TagId] = TN.[Id]
		    WHERE T.[OwnerId] = @OldId
			    AND TIN.[Id] IS NULL

		    -- Delete any of the old person's tags that have the same name and are associated to same 
		    -- entity type as a tag used bo the new person
		    DELETE T
		    FROM [Tag] T
			    INNER JOIN [Tag] TN
				    ON TN.[EntityTypeId] = T.[EntityTypeId]
				    AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
				    AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
				    AND TN.[Name] = T.[Name]
				    AND TN.[OwnerId] = @NewId
		    WHERE T.[OwnerId] = @OldId


		    -- Remaining Tables
		    -----------------------------------------------------------------------------------------------
		    -- Update any column on any table that has a foreign key relationship to the Person table's Id
		    -- column  

		    DECLARE @Sql varchar(max)

		    DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
		    SELECT 
			    ' UPDATE ' + tso.name +
			    ' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
			    ' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
		    FROM sys.foreign_key_columns kc
			    INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
			    INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
			    INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
			    INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
			    INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
		    WHERE so.name = 'Person'
			    AND rac.name = 'Id'
			    AND tso.name NOT IN (
					    'Auth'
				    ,'CommunicationRecipient'
				    ,'GroupMember'
				    ,'PhoneNumber'
			    )

		    OPEN ForeignKeyCursor

		    FETCH NEXT
		    FROM ForeignKeyCursor
		    INTO @Sql

		    WHILE (@@FETCH_STATUS <> -1)
		    BEGIN

			    IF (@@FETCH_STATUS = 0)
			    BEGIN

				    EXEC(@Sql)
			
			    END
		
			    FETCH NEXT
			    FROM ForeignKeyCursor
			    INTO @Sql

		    END

		    CLOSE ForeignKeyCursor
		    DEALLOCATE ForeignKeyCursor


		    -- Person
		    -----------------------------------------------------------------------------------------------
		    -- Delete the old person record.  By this time it should not have any relationships 
		    -- with other tables 

		    DELETE Person
		    WHERE [Id] = @OldId

	    END

    END
" );

            Sql( @"
    CREATE PROCEDURE [dbo].[spPersonMerge]
    @OldId int, 
    @NewId int,
    @DeleteOldPerson bit

    AS

    DECLARE @OldGuid uniqueidentifier
    DECLARE @NewGuid uniqueidentifier

    SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
    SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

    IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
    BEGIN

	    DECLARE @PersonEntityTypeId INT
	    SET @PersonEntityTypeId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )

	    DECLARE @PersonFieldTypeId INT
	    SET @PersonFieldTypeId = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )


	    --BEGIN TRANSACTION


	    -- Attribute Value
	    -----------------------------------------------------------------------------------------------
	    -- Update Attribute Values associated with person 
	    -- The new user's attribute value will only get updated if the old user has a value, and the 
	    -- new user does not (determining the correct value will eventually be decided by user in a UI)
	    UPDATE AVO
		    SET [EntityId] = @NewId
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AVO
		    ON AVO.[EntityId] = @OldId
		    AND AVO.[AttributeId] = A.[Id]
	    LEFT OUTER JOIN [AttributeValue] AVN
		    ON AVO.[EntityId] = @NewId
		    AND AVN.[AttributeId] = A.[Id]
	    WHERE A.[EntityTypeId] = @PersonEntityTypeId
	    AND AVN.[Id] IS NULL

	    -- Delete any attribute values that were not updated (due to new person already having existing 
	    -- value)
	    DELETE AV
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AV
		    ON AV.[EntityId] = @OldId
		    AND AV.[AttributeId] = A.[Id]
	    WHERE A.[EntityTypeId] = @PersonEntityTypeId

	    -- Update Attribute Values that have person as a value
	    -- NOTE: BECAUSE VALUE IS A VARCHAR(MAX) COLUMN WE CANT ADD AN INDEX FOR ATTRIBUTEID AND
	    -- VALUE.  THIS UPDATE COULD POTENTIALLY BE A BOTTLE-NECK FOR MERGES
	    UPDATE AV
		    SET [Value] = CAST( @NewGuid AS VARCHAR(64) )
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AV
		    ON AV.[AttributeId] = A.[Id]
		    AND AV.[Value] = CAST( @OldGuid AS VARCHAR(64) )
	    WHERE A.[FieldTypeId] = @PersonFieldTypeId


	    -- Authorization
	    -----------------------------------------------------------------------------------------------
	    -- Update any authorizations associated to old person that do not already have a matching 
	    -- authorization for the new person
	    UPDATE AO
		    SET [PersonId] = @NewId
	    FROM [Auth] AO
	    LEFT OUTER JOIN [Auth] AN
		    ON AN.[PersonId] = @NewId
		    AND AN.[EntityTypeId] = AO.[EntityTypeId]
		    AND AN.[EntityId] = AO.[EntityId]
		    AND AN.[Action] = AO.[Action]
		    AND AN.[AllowOrDeny] = AO.[AllowOrDeny]
		    AND AN.[SpecialRole] = AO.[SpecialRole]
	    WHERE AO.[PersonId] = @OldId
	    AND AN.[Id] IS NULL

	    -- Delete any authorizations not updated to new person
	    DELETE [Auth]
	    WHERE [PersonId] = @OldId


	    -- Category
	    -----------------------------------------------------------------------------------------------
	    -- Currently UI does not allow categorizing people, but if it does in the future, would need 
	    -- to add script to handle merge


	    -- Communication Recipient
	    -----------------------------------------------------------------------------------------------
	    -- Update any communication recipients associated to old person to the new person where the new
	    -- person does not already have the recipient record
	    UPDATE CRO
		    SET [PersonId] = @NewId
	    FROM [CommunicationRecipient] CRO
	    LEFT OUTER JOIN [CommunicationRecipient] CRN
		    ON CRN.[CommunicationId] = CRO.[CommunicationId]
		    AND CRN.[PersonId] = @NewId
	    WHERE CRO.[PersonId] = @OldId
	    AND CRN.[Id] IS NULL

	    -- Delete any remaining recipents that were not updated
	    DELETE [CommunicationRecipient]
	    WHERE [PersonId] = @OldId

	    -- Group Member
	    -----------------------------------------------------------------------------------------------
	    -- Update any group members associated to old person to the new person where the new is not 
	    -- already in the group with the same role
	    UPDATE GMO
		    SET [PersonId] = @NewId
	    FROM [GroupMember] GMO
	    LEFT OUTER JOIN [GroupMember] GMN
		    ON GMN.[GroupId] = GMO.[GroupId]
		    AND GMN.[PersonId] = @NewId
		    AND GMN.[GroupRoleId] = GMO.[GroupRoleId] -- If person can be in group twice with diff role
	    WHERE GMO.[PersonId] = @OldId
	    AND GMN.[Id] IS NULL

	    -- Delete any group members not updated (already existed with new id)
	    DELETE [GroupMember]
	    WHERE [PersonId] = @OldId


	    -- Note
	    -----------------------------------------------------------------------------------------------
	    -- Update any note that is associated to the old person to be associated to the new person
	    UPDATE N
		    SET [EntityId] = @NewId
	    FROM [NoteType] NT
	    INNER JOIN [Note] N
		    ON N.[NoteTypeId] = NT.[Id]
		    AND N.[EntityId] = @OldId
	    WHERE NT.[EntityTypeId] = @PersonEntityTypeId


	    -- Phone Numbers
	    -----------------------------------------------------------------------------------------------
	    -- Update any phone numbers associated to the old person that do not already exist for the new
	    -- person
	    UPDATE PNO
		    SET [PersonId] = @NewId
	    FROM [PhoneNumber] PNO
	    INNER JOIN [PhoneNumber] PNN
		    ON PNN.[PersonId] = @NewId
		    AND PNN.[Number] = PNO.[Number]
		    AND PNN.[Extension] = PNO.[Extension]
		    AND PNN.[NumberTypeValueId] = PNO.[NumberTypeValueId]
	    WHERE PNO.[PersonId] = @OldId
	    AND PNN.[Id] IS NULL

	    -- Delete any numbers not updated (new person already had same number)
	    DELETE [PhoneNumber]
	    WHERE [PersonId] = @OldId


	    -- Tags
	    -----------------------------------------------------------------------------------------------
	    -- Update any tags associated to the old person to be associated to the new person as long as 
	    -- same tag does not already exist for new person
	    UPDATE TIO
		    SET [EntityGuid] = @NewGuid
	    FROM [Tag] T
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
		    AND TIO.[EntityGuid] = @OldGuid
	    LEFT OUTER JOIN [TaggedItem] TIN
		    ON TIN.[TagId] = T.[Id]
		    AND TIN.[EntityGuid] = @NewGuid
	    WHERE T.[EntityTypeId] = @PersonEntityTypeId
	    AND TIN.[Id] IS NULL

	    -- Delete any tagged items still associated with old person (new person had same tag)
	    DELETE TIO
	    FROM [Tag] T
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
		    AND TIO.[EntityGuid] = @OldGuid
	    WHERE T.[EntityTypeId] = @PersonEntityTypeId

	    -- If old person and new person have tags with the same name for the same entity type,
	    -- update the old person's tagged items to use the new person's tag
	    UPDATE TIO
		    SET [TagId] = TIN.[Id]
	    FROM [Tag] T
	    INNER JOIN [Tag] TN
		    ON TN.[EntityTypeId] = T.[EntityTypeId]
		    AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		    AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		    AND TN.[Name] = T.[Name]
		    AND TN.[OwnerId] = @NewId
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
	    LEFT OUTER JOIN [TaggedItem] TIN
		    ON TIN.[TagId] = TN.[Id]
	    WHERE T.[OwnerId] = @OldId
	    AND TIN.[Id] IS NULL

	    -- Delete any of the old person's tags that have the same name and are associated to same 
	    -- entity type as a tag used bo the new person
	    DELETE T
	    FROM [Tag] T
	    INNER JOIN [Tag] TN
		    ON TN.[EntityTypeId] = T.[EntityTypeId]
		    AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		    AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		    AND TN.[Name] = T.[Name]
		    AND TN.[OwnerId] = @NewId
	    WHERE T.[OwnerId] = @OldId


	    -- Remaining Tables
	    -----------------------------------------------------------------------------------------------
	    -- Update any column on any table that has a foreign key relationship to the Person table's Id
	    -- column  

	    DECLARE @Sql varchar(max)

	    DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
	    SELECT 
		    ' UPDATE ' + tso.name +
		    ' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
		    ' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
	    FROM sys.foreign_key_columns kc
	    INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
	    INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
	    INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
	    INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
	    INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
	    WHERE so.name = 'Person'
	    AND rac.name = 'Id'
	    AND tso.name NOT IN (
		     'Auth'
		    ,'CommunicationRecipient'
		    ,'GroupMember'
		    ,'PhoneNumber'
	    )

	    OPEN ForeignKeyCursor

	    FETCH NEXT
	    FROM ForeignKeyCursor
	    INTO @Sql

	    WHILE (@@FETCH_STATUS <> -1)
	    BEGIN

		    IF (@@FETCH_STATUS = 0)
		    BEGIN

			    EXEC(@Sql)
			
		    END
		
		    FETCH NEXT
		    FROM ForeignKeyCursor
		    INTO @Sql

	    END

	    CLOSE ForeignKeyCursor
	    DEALLOCATE ForeignKeyCursor


	    -- Person
	    -----------------------------------------------------------------------------------------------
	    -- Optionally delete the old person record.  By this time it should not have any relationships 
	    -- with other tables (if Rock is being synced with other data source, the sync may handle the
	    -- delete)
	
	    IF @DeleteOldPerson = 1 
	    BEGIN
		    DELETE Person
		    WHERE [Id] = @OldId
	    END
	
	    --COMMIT TRANSACTION


    END
" );

            Sql( @"
    /*
    <doc>
	    <summary>
 		    This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions
            The StatementGenerator utility uses this procedure along with querying transactions thru REST to generate statements
	    </summary>

	    <returns>
		    * PersonId
            * GroupId
            * AddressPersonNames
            * Street1
            * Street2
            * City
            * State
            * PostalCode
            * StartDate
            * EndDate
            * CustomMessage1
            * CustomMessage2
	    </returns>
	    <param name=""StartDate"" datatype=""datetime"">The starting date of the date range</param>
        <param name=""EndDate"" datatype=""datetime"">The ending date of the date range</param>
	    <param name=""AccountIds"" datatype=""varchar(max)"">Comma delimited list of account ids. NULL means all</param>
	    <param name=""PersonId"" datatype=""int"">Person the statement if for. NULL means all persons that have transactions for the date range</param>
	    <param name=""OrderByPostalCode"" datatype=""int"">Set to 1 to have the results sorted by PostalCode, 0 for no particular order</param>
	    <remarks>	
		    Uses the following constants:
			    * Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			    * Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			    * Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	    </remarks>
	    <code>
		    EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1  -- year 2014 statements for all persons
	    </code>
    </doc>
    */
    ALTER PROCEDURE [spFinance_ContributionStatementQuery]
	    @StartDate datetime
        , @EndDate datetime
        , @AccountIds varchar(max) 
        , @PersonId int -- NULL means all persons
        , @OrderByPostalCode bit
    AS
    BEGIN
        DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'	
        DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'

        -- SET NOCOUNT ON added to prevent extra result sets from
	    -- interfering with SELECT statements.
	    SET NOCOUNT ON;

        ;WITH tranListCTE
        AS
        (
            SELECT  
                [AuthorizedPersonId] 
            FROM 
                [FinancialTransaction] [ft]
            INNER JOIN 
                [FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
            WHERE 
                ([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
            AND 
                (
                    (@AccountIds is null)
                    OR
                    (ftd.[AccountId] in (select * from ufnUtility_CsvToTable(@AccountIds)))
                )
        )

        SELECT 
            [pg].[PersonId]
            , [pg].[GroupId]
            , [pn].[PersonNames] [AddressPersonNames]
            , [l].[Street1]
            , [l].[Street2]
            , [l].[City]
            , [l].[State]
            , [l].[PostalCode]
            , @StartDate [StartDate]
            , @EndDate [EndDate]
            , null [CustomMessage1]
            , null [CustomMessage2]
        FROM (
            -- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
            -- These are Persons that give as part of a Group.  For example, Husband and Wife
            SELECT DISTINCT
                null [PersonId] 
                , [g].[Id] [GroupId]
            FROM 
                [Person] [p]
            INNER JOIN 
                [Group] [g] ON [p].[GivingGroupId] = [g].[Id]
            WHERE 
                [p].[Id] in (SELECT * FROM tranListCTE)
            UNION
            -- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
            -- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
            -- to determine which address(es) the statements need to be mailed to 
            SELECT  
                [p].[Id] [PersonId],
                [g].[Id] [GroupId]
            FROM
                [Person] [p]
            JOIN 
                [GroupMember] [gm]
            ON 
                [gm].[PersonId] = [p].[Id]
            JOIN 
                [Group] [g]
            ON 
                [gm].[GroupId] = [g].[Id]
            WHERE
                [p].[GivingGroupId] is null
            AND
                [g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
            AND [p].[Id] IN (SELECT * FROM tranListCTE)
        ) [pg]
        CROSS APPLY 
            [ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId]) [pn]
        JOIN 
            [GroupLocation] [gl] 
        ON 
            [gl].[GroupId] = [pg].[GroupId]
        JOIN
            [Location] [l]
        ON 
            [l].[Id] = [gl].[LocationId]
        WHERE 
            [gl].[IsMailingLocation] = 1
        AND
            [gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
        AND
            (
                (@personId is null) 
            OR 
                ([pg].[PersonId] = @personId)
            )
        ORDER BY
        CASE WHEN @OrderByPostalCode = 1 THEN PostalCode END
    END
" );

        }
    }
}
