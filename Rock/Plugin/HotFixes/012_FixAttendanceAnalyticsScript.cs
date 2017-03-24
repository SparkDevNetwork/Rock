﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 12, "1.6.0" )]
    public class FixAttendanceAnalyticsScript : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
//  Moved to core migration: 201612121647292_HotFixesFrom6_1
//            Sql( @"
//    IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates]') AND type in (N'P', N'PC'))
//        DROP PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates]
//" );

//            Sql( @"

//    /*
//    <doc>
//	    <summary>
// 		    This function returns attendee person ids and the dates they attended based on selected filter criteria
//	    </summary>

//	    <returns>
//		    * PersonId
//		    * SundayDate
//		    * MonthDate
//		    * Year Date
//	    </returns>
//	    <param name='GroupTypeId' datatype='int'>The Check-in Area Group Type Id (only attendance for this are will be included</param>
//	    <param name='StartDate' datatype='datetime'>Beginning date range filter</param>
//	    <param name='EndDate' datatype='datetime'>Ending date range filter</param>
//	    <param name='GroupIds' datatype='varchar(max)'>Optional list of group ids to limit attendance to</param>
//	    <param name='CampusIds' datatype='varchar(max)'>Optional list of campus ids to limit attendance to</param>
//	    <param name='IncludeNullCampusIds' datatype='bit'>Flag indicating if attendance not tied to campus should be included</param>
//	    <remarks>	
//	    </remarks>
//	    <code>
//		    EXEC [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates] '15,16,17,18,19,20,21,22', '2015-01-01 00:00:00', '2015-12-31 23:59:59', null, 0
//	    </code>
//    </doc>
//    */

//    CREATE PROCEDURE [dbo].[spCheckin_AttendanceAnalyticsQuery_AttendeeDates]
//	      @GroupIds varchar(max)
//	    , @StartDate datetime = NULL
//	    , @EndDate datetime = NULL
//	    , @CampusIds varchar(max) = NULL
//	    , @IncludeNullCampusIds bit = 0
//	    , @ScheduleIds varchar(max) = NULL
//	    WITH RECOMPILE

//    AS

//    BEGIN

//        -- Manipulate dates to only be those dates who's SundayDate value would fall between the selected date range ( so that sunday date does not need to be used in where clause )
//	    SET @StartDate = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), @StartDate ) % 7 ), CONVERT( date, @StartDate ) ), '1900-01-01' )
//	    SET @EndDate = COALESCE( DATEADD( day, ( 0 - DATEDIFF( day, CONVERT( datetime, '19000107', 112 ), @EndDate ) % 7 ), @EndDate ), '2100-01-01' )
//        IF @EndDate < @StartDate SET @EndDate = DATEADD( day, 6 + DATEDIFF( day, @EndDate, @StartDate ), @EndDate )
//	    SET @EndDate = DATEADD( second, -1, DATEADD( day, 1, @EndDate ) )

//	    -- Get all the attendance
//	    SELECT 
//		    PA.[PersonId],
//		    A.[SundayDate],
//		    DATEADD( day, -( DATEPART( day, [SundayDate] ) ) + 1, [SundayDate] ) AS [MonthDate],
//		    DATEADD( day, -( DATEPART( dayofyear, [SundayDate] ) ) + 1, [SundayDate] ) AS [YearDate]
//	    FROM (
//		    SELECT 
//			    [PersonAliasId],
//			    [GroupId],
//			    [CampusId],
//			    DATEADD( day, ( 6 - ( DATEDIFF( day, CONVERT( datetime, '19000101', 112 ), [StartDateTime] ) % 7 ) ), CONVERT( date, [StartDateTime] ) ) AS [SundayDate]
//		    FROM [Attendance] A
//            WHERE A.[GroupId] in ( SELECT * FROM ufnUtility_CsvToTable( @GroupIds ) ) 
//            AND [StartDateTime] BETWEEN @StartDate AND @EndDate
//		    AND [DidAttend] = 1
//		    AND ( 
//			    ( @CampusIds IS NULL OR A.[CampusId] in ( SELECT * FROM ufnUtility_CsvToTable( @CampusIds ) ) ) OR  
//			    ( @IncludeNullCampusIds = 1 AND A.[CampusId] IS NULL ) 
//		    )
//		    AND ( @ScheduleIds IS NULL OR A.[ScheduleId] IN ( SELECT * FROM ufnUtility_CsvToTable( @ScheduleIds ) ) )
//	    ) A 
//	    INNER JOIN [PersonAlias] PA ON PA.[Id] = A.[PersonAliasId]

//    END
//" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
