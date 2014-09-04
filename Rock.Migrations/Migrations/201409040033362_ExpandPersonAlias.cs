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
        }
    }
}
