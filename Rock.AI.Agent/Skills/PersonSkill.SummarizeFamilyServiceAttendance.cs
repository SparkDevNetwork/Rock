using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class PersonSkill
    {
        #region Tool(s)
        [Description( "Provide attendance information for members of the selected person." )]
        [AgentToolGuid( "544F23D7-6D28-41EA-BD43-249C976BEBA0" )]
        [AgentPurpose( "Fetches service attendance for a person's family." )]
        [AgentToolReturnDescription( "Returns the family's last recorded Sunday date and the list of family check-ins from that service week. Also includes the family's: monthly completion, first-time check-in, and the number of weeks attended out of the last 16." )]
        public RockToolResult SummarizeFamilyServiceAttendance( string personIdKey )
        {
            using var rockContext = _rockContextFactory.CreateRockContext();
            var personService = new PersonService( rockContext );
            var person = personService.Get( personIdKey, false );

            if ( person == null )
            {
                return RockToolResult.Error( "No person could be found with the provided personIdKey." )
                    .WithInstructions( "You can call SearchPerson to find the corresponding key." );
            }

            if ( !person.PrimaryAliasId.HasValue )
            {
                return RockToolResult.Error( "The person does not have a primary alias." );
            }

            var parameters = new Dictionary<string, object>
            {
                { "@MonthCount", 12 },
                { "@PersonId", person.Id }
            };

            DataSet ds = ExecuteFamilyAttendanceQuery( parameters );
            if ( ds == null || ds.Tables.Count < 6 )
            {
                return RockToolResult.Error( "There was an unexpected error executing the query." );
            }

            var result = ParseFamilyAttendanceResults( ds );
            if ( result == null )
            {
                return RockToolResult.NoData();
            }

            var hasAnyCheckIns = result.CheckIns?.Any() ?? false;
            var providedPersonHasCheckedIn = hasAnyCheckIns && result.CheckIns.Any( ci => ci.Person != null && ci.Person.Id == person.Id );

            if ( !hasAnyCheckIns )
            {
                return RockToolResult.NoData()
                    .WithInstructions( "No one in the family has checked in for a service." );
            }

            var instructions = string.Empty;

            if ( !providedPersonHasCheckedIn )
            {
                instructions += "Note that the provided person has not checked in during that service week, although someone from their family has.";
            }
            else
            {
                instructions += "Provided is the attendance data for the given family. The person and potentially members of their family were present.";
            }

            return RockToolResult.Success( result )
                .WithInstructions( instructions )
                .WithoutHistoryContent(); // BC TODO: Figure out what to store in history.
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Executes the family attendance summary SQL and returns the resulting <see cref="DataSet"/>.
        /// </summary>
        /// <param name="parameters">
        /// SQL parameters for the query. Expected keys include:
        /// <c>@MonthCount</c> (int) and <c>@PersonId</c> (int).
        /// </param>
        /// <returns>
        /// A <see cref="DataSet"/> containing the attendance result tables on success; <c>null</c> if the
        /// query fails or an exception is thrown.
        /// </returns>
        /// <remarks>
        /// Logs any thrown exception and converts it to a <c>null</c> return.
        /// The command timeout is fixed at 60 seconds.
        /// </remarks>
        private DataSet ExecuteFamilyAttendanceQuery( Dictionary<string, object> parameters )
        {
            try
            {
                return DbService.GetDataSet( _familyAttendanceSummarySql, CommandType.Text, parameters, 60 );
            }
            catch ( Exception ex )
            {
                _logger.LogError( ex, "Failed to run attendance summary SQL." );
                return null;
            }
        }

        /// <summary>
        /// Parses the attendance <see cref="DataSet"/> into a strongly-typed summary object.
        /// </summary>
        /// <param name="ds">
        /// The <see cref="DataSet"/> produced by <see cref="ExecuteFamilyAttendanceQuery"/>.
        /// Must contain at least five tables in this order:
        /// 0 = SundayDate, 1 = CheckIns, 2 = CheckInSummary, 3 = FirstTimeCheckIn, 4 = WeeksAttended.
        /// </param>
        /// <returns>
        /// A populated <see cref="SummarizedFamilyServiceAttendanceResult"/>. If no check-ins are present,
        /// the <see cref="SummarizedFamilyServiceAttendanceResult.CheckIns"/> list will be empty.
        /// </returns>
        /// <remarks>
        /// This method assumes table indexes and column names match the expected schema. Validate
        /// the <see cref="DataSet"/> structure before calling to avoid index/column errors.
        /// </remarks>
        private SummarizedFamilyServiceAttendanceResult ParseFamilyAttendanceResults( DataSet ds )
        {
            var sundayDateTable = ds.Tables[0];
            var checkInsTable = ds.Tables[1];
            var checkInSummaryTable = ds.Tables[2];
            var firstTimeCheckInTable = ds.Tables[3];
            var weeksAttendedTable = ds.Tables[4];

            var checkIns = ParseCheckIns( checkInsTable, sundayDateTable, out var sundayDate );

            var result = new SummarizedFamilyServiceAttendanceResult
            {
                CheckIns = checkIns,
                CheckInSummary = ParseCheckInSummary( checkInSummaryTable ),
                FirstTimeCheckIn = ParseFirstTimeCheckIn( firstTimeCheckInTable ),
                WeeksAttendedLast16Weeks = ParseWeeksAttended( weeksAttendedTable ),
                SundayDate = sundayDate
            };

            return result;
        }

        /// <summary>
        /// Parses individual family member check-ins and resolves the service week Sunday date.
        /// </summary>
        /// <param name="checkInsTable">
        /// Table containing check-in rows. Expected columns include:
        /// <c>Date</c> (optional), <c>CampusId</c>/<c>CampusName</c>, <c>LocationId</c>/<c>LocationName</c>,
        /// <c>ScheduleId</c>/<c>ScheduleName</c>, <c>GroupId</c>/<c>GroupName</c>, <c>AreaId</c>/<c>AreaName</c>,
        /// <c>PersonId</c>/<c>PersonName</c>.
        /// </param>
        /// <param name="sundayDateTable">
        /// Table with a single row containing <c>SundayDate</c>. Used as the default date when a check-in
        /// has a missing <c>Date</c> value.
        /// </param>
        /// <param name="sundayDate">
        /// Outputs the most recent Sunday date for which data was returned, or <c>null</c> when unavailable.
        /// </param>
        /// <returns>
        /// A list of <see cref="FamilyMemberCheckInResult"/>. Returns:
        /// <list type="bullet">
        /// <item><description><c>null</c> if <paramref name="sundayDateTable"/> has no rows or <c>SundayDate</c> is <see cref="DBNull"/>.</description></item>
        /// <item><description>An empty list if there were no check-in rows for the week.</description></item>
        /// <item><description>A populated list when check-ins exist; missing row dates default to <paramref name="sundayDate"/>.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Converts dates using <see cref="Convert.ToDateTime(object)"/>. Invalid date values may throw.
        /// </remarks>
        private List<FamilyMemberCheckInResult> ParseCheckIns( DataTable checkInsTable, DataTable sundayDateTable, out DateTime? sundayDate )
        {
            var checkIns = new List<FamilyMemberCheckInResult>();
            sundayDate = null;

            if ( sundayDateTable.Rows.Count == 0 )
            {
                return null;
            }

            var sundayDateValue = sundayDateTable.Rows[0]["SundayDate"];
            if ( sundayDateValue == DBNull.Value )
            {
                return null;
            }

            if ( checkInsTable.Rows.Count == 0 )
            {
                return checkIns;
            }

            sundayDate = Convert.ToDateTime( sundayDateValue );

            foreach ( DataRow row in checkInsTable.Rows )
            {
                var date = row.Table.Columns.Contains( "Date" ) && row["Date"] != DBNull.Value
                    ? ( DateTime ) row["Date"]
                    : sundayDate.Value;

                checkIns.Add( new FamilyMemberCheckInResult
                {
                    Date = date,
                    Campus = KeyName( row, "CampusId", "CampusName" ),
                    Location = KeyName( row, "LocationId", "LocationName" ),
                    Schedule = KeyName( row, "ScheduleId", "ScheduleName" ),
                    Group = KeyName( row, "GroupId", "GroupName" ),
                    Area = KeyName( row, "AreaId", "AreaName" ),
                    Person = KeyName( row, "PersonId", "PersonName" )
                } );
            }

            return checkIns;
        }

        /// <summary>
        /// Parses monthly check-in summary data and calculates completion percentages.
        /// </summary>
        /// <param name="checkInSummaryTable">
        /// Table with columns <c>Month</c> (int), <c>Year</c> (int),
        /// <c>AttendanceCount</c> (int), <c>SundaysInMonth</c> (int).
        /// </param>
        /// <returns>
        /// A list of <see cref="CheckInSummaryMonthResult"/> where
        /// <see cref="CheckInSummaryMonthResult.CompletionPercentage"/> is capped at 100
        /// and set to 0 when year is 0 or <c>SundaysInMonth</c> ≤ 0.
        /// </returns>
        /// <remarks>
        /// This overload stores the numeric month value (1–12) as returned; formatting to month names
        /// should be performed by the caller if needed.
        /// </remarks>
        private List<CheckInSummaryMonthResult> ParseCheckInSummary( DataTable checkInSummaryTable )
        {
            var checkInSummary = new List<CheckInSummaryMonthResult>();

            foreach ( DataRow row in checkInSummaryTable.Rows )
            {
                var month = row["Month"].ToIntSafe( 0 );
                var year = row["Year"].ToIntSafe( 0 );
                var attendanceCount = row["AttendanceCount"].ToIntSafe( 0 );
                var sundaysInMonth = row["SundaysInMonth"].ToIntSafe( 4 );

                checkInSummary.Add( new CheckInSummaryMonthResult
                {
                    Month = month,
                    Year = year,
                    CompletionPercentage = year == 0 || sundaysInMonth <= 0
                        ? 0
                        : Math.Min( CalcPct( attendanceCount, sundaysInMonth ), 100 )
                } );
            }

            return checkInSummary;
        }

        /// <summary>
        /// Parses the first recorded check-in date for the family.
        /// </summary>
        /// <param name="firstTimeCheckInTable">
        /// Table expected to contain column <c>FirstTimeCheckIn</c> in the first row.
        /// </param>
        /// <returns>
        /// The first-time check-in <see cref="DateTime"/> if present; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        /// If the table is empty or the expected column is missing, returns <c>null</c>.
        /// </remarks>
        private DateTime? ParseFirstTimeCheckIn( DataTable firstTimeCheckInTable )
        {
            if ( firstTimeCheckInTable.Rows.Count > 0 && firstTimeCheckInTable.Columns.Contains( "FirstTimeCheckIn" ) )
            {
                var value = firstTimeCheckInTable.Rows[0]["FirstTimeCheckIn"];
                return value == DBNull.Value ? ( DateTime? ) null : Convert.ToDateTime( value );
            }

            return null;
        }

        /// <summary>
        /// Parses the number of attended weeks within the last 16 weeks.
        /// </summary>
        /// <param name="weeksAttendedTable">
        /// Table whose first column contains an integer attendance count in the first row.
        /// </param>
        /// <returns>
        /// The number of attended weeks; returns 0 if the table is empty or the value is missing/invalid.
        /// </returns>
        private int ParseWeeksAttended( DataTable weeksAttendedTable )
        {
            if ( weeksAttendedTable.Rows.Count > 0 )
            {
                var col = weeksAttendedTable.Columns[0];
                return weeksAttendedTable.Rows[0][col].ToIntSafe( 0 );
            }

            return 0;
        }

        /// <summary>
        /// Creates a <see cref="KeyNameResult"/> from a row's ID and name columns.
        /// </summary>
        /// <param name="row">The source <see cref="DataRow"/>.</param>
        /// <param name="idCol">The column name containing the numeric identifier.</param>
        /// <param name="nameCol">The column name containing the display name.</param>
        /// <returns>
        /// A <see cref="KeyNameResult"/> when either a positive ID or a non-empty name is present; otherwise <c>null</c>.
        /// </returns>
        /// <remarks>
        /// Missing columns or <see cref="DBNull"/> values are treated as absent.
        /// </remarks>
        private static KeyNameResult KeyName( DataRow row, string idCol, string nameCol )
        {
            var id = row.Table.Columns.Contains( idCol ) && row[idCol] != DBNull.Value ? row[idCol].ToIntSafe( 0 ) : 0;
            var name = row.Table.Columns.Contains( nameCol ) && row[nameCol] != DBNull.Value ? row[nameCol].ToStringSafe() : string.Empty;

            return id > 0 || !string.IsNullOrWhiteSpace( name )
                ? new KeyNameResult { Id = id, Name = name }
                : null;
        }

        /// <summary>
        /// Calculates a percentage of Sundays attended, rounded to a whole number.
        /// </summary>
        /// <param name="attended">The count of attended Sundays.</param>
        /// <param name="sundays">The total Sundays in the period.</param>
        /// <returns>
        /// A whole-number percentage as <see cref="decimal"/>. Returns 0 when <paramref name="sundays"/> ≤ 0.
        /// </returns>
        /// <remarks>
        /// Uses <see cref="Math.Round(decimal, int)"/> to 0 decimal places.
        /// </remarks>
        private static decimal CalcPct( int attended, int sundays ) => sundays <= 0 ? 0m : Math.Round( ( attended * 100m ) / sundays, 0 );

        #endregion

        #region SQL

        private const string _familyAttendanceSummarySql = @"-- For clarity; mirror C# param names
DECLARE @pMonthCount INT = @MonthCount;
DECLARE @pPersonId   INT = @PersonId;

-- =======================
-- Constants (Rock GUIDs)
-- =======================
DECLARE @cROLE_ADULT        UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';
DECLARE @cROLE_CHILD        UNIQUEIDENTIFIER = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9';
DECLARE @cGROUP_TYPE_FAMILY UNIQUEIDENTIFIER = '790E3215-3B10-442B-AF69-616C0DCB998E';

-- ==========================================
-- Determine the person’s family role (adult/child)
-- ==========================================
DECLARE @RoleGuid UNIQUEIDENTIFIER =
(
    SELECT TOP (1) gtr.[Guid]
    FROM GroupMember gm
    JOIN GroupTypeRole gtr ON gtr.Id = gm.GroupRoleId
    JOIN [Group] g        ON g.Id = gm.GroupId
    WHERE gm.PersonId = @pPersonId
      AND g.GroupTypeId = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUP_TYPE_FAMILY)
    ORDER BY gm.Id DESC
);

-- Treat NULL as ADULT so we default to family scope
SET @RoleGuid = ISNULL(@RoleGuid, @cROLE_ADULT);

-- ==========================================
-- Build scoped people/aliases: adult => family, child => self only
-- (no UDFs; resolve family via GroupMember/Group)
-- ==========================================
DECLARE @FamilyPersonIds TABLE (PersonId INT PRIMARY KEY);

INSERT INTO @FamilyPersonIds (PersonId)
SELECT DISTINCT gm2.PersonId
FROM GroupMember gm
JOIN [Group] g  ON g.Id = gm.GroupId
JOIN GroupMember gm2 ON gm2.GroupId = gm.GroupId
WHERE gm.PersonId = @pPersonId
  AND g.GroupTypeId = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUP_TYPE_FAMILY);

IF (@RoleGuid = @cROLE_CHILD)
BEGIN
    DELETE f FROM @FamilyPersonIds f WHERE f.PersonId <> @pPersonId;
END

DECLARE @ScopedAliases TABLE (PersonAliasId INT PRIMARY KEY);
INSERT INTO @ScopedAliases (PersonAliasId)
SELECT pa.Id
FROM PersonAlias pa
JOIN @FamilyPersonIds f ON f.PersonId = pa.PersonId;

-- ==========================
-- Service groups (weekly services)
-- ==========================
DECLARE @ServiceGroups TABLE (GroupId INT PRIMARY KEY);
INSERT INTO @ServiceGroups (GroupId)
SELECT Id FROM dbo.ufnCheckin_WeeklyServiceGroups();

-- ==========================================
-- Error bucket (OPTIONAL 6th result set)
-- ==========================================
DECLARE @Errors TABLE
(
    Code NVARCHAR(64) PRIMARY KEY,
    Message NVARCHAR(400)
);

-- ==========================================
-- Find the most recent SundayDate any scoped member attended
-- ==========================================
DECLARE @LastSundayDate DATE;

SELECT TOP (1)
    @LastSundayDate = o.SundayDate
FROM Attendance a
JOIN AttendanceOccurrence o ON o.Id = a.OccurrenceId
WHERE a.DidAttend = 1
  AND EXISTS (SELECT 1 FROM @ScopedAliases sa WHERE sa.PersonAliasId = a.PersonAliasId)
  AND (o.DidNotOccur IS NULL OR o.DidNotOccur = 0)
  AND EXISTS (SELECT 1 FROM @ServiceGroups sg WHERE sg.GroupId = o.GroupId)
ORDER BY a.StartDateTime DESC, a.Id DESC;

IF (@LastSundayDate IS NULL)
BEGIN
    INSERT INTO @Errors(Code, Message)
    VALUES ('NO_LAST_SUNDAY', 'No recent service attendance found for the scoped family.');
END

-- =========================================================
-- Result Set 1: SundayDate (single row, may be NULL)
-- =========================================================
SELECT @LastSundayDate AS SundayDate;

-- =========================================================
-- Result Set 2: CheckIns (rows; empty if @LastSundayDate is NULL)
-- =========================================================
SELECT
    CAST(a.StartDateTime AS DATE) AS [Date],
    c.Id                  AS CampusId,
    c.Name                AS CampusName,
    o.LocationId          AS LocationId,
    l.Name                AS LocationName,
    o.ScheduleId          AS ScheduleId,
    s.Name                AS ScheduleName,
    g.Id                  AS GroupId,
    g.Name                AS GroupName,
    g.GroupTypeId         AS AreaId,
    area.Name             AS AreaName,
    p.Id                  AS PersonId,
    p.NickName + ' ' + p.LastName AS PersonName,
    o.RootGroupTypeId     AS RootLevelGroupTypeId
FROM Attendance a
JOIN AttendanceOccurrence o ON o.Id = a.OccurrenceId
JOIN PersonAlias pa         ON pa.Id = a.PersonAliasId
JOIN Person p               ON p.Id = pa.PersonId
JOIN [Group] g              ON g.Id = o.GroupId
LEFT JOIN [GroupType] area  ON area.Id = g.GroupTypeId
LEFT JOIN [Location]  l     ON l.Id  = o.LocationId
LEFT JOIN [Schedule]  s     ON s.Id  = o.ScheduleId
LEFT JOIN [Campus]    c     ON c.Id  = a.CampusId
WHERE @LastSundayDate IS NOT NULL
  AND o.SundayDate = @LastSundayDate
  AND a.DidAttend = 1
  AND (o.DidNotOccur IS NULL OR o.DidNotOccur = 0)
  AND EXISTS (SELECT 1 FROM @ScopedAliases sa WHERE sa.PersonAliasId = a.PersonAliasId)
  AND EXISTS (SELECT 1 FROM @ServiceGroups sg WHERE sg.GroupId = o.GroupId)
ORDER BY a.StartDateTime ASC, a.Id ASC;

-- =========================================================
-- Result Set 3: CheckInSummary (family) — existing SP
-- =========================================================
EXEC dbo.spCheckin_BadgeAttendance
     @PersonId     = @pPersonId,
     @RoleGuid     = @RoleGuid,
     @MonthCount   = @pMonthCount;

-- =========================================================
-- Result Set 4: FirstTimeCheckIn (family) — MIN across family
--   + Fallback to earliest attended service (service groups)
-- =========================================================
DECLARE @FirstTimeCheckIn DATE;

-- Attribute-based min across family
SELECT
    @FirstTimeCheckIn = MIN(TRY_CONVERT(DATE, av.[Value]))
FROM Attribute a
JOIN AttributeValue av ON av.AttributeId = a.Id
WHERE a.[Key] = 'FirstVisit'
  AND a.EntityTypeId = (SELECT Id FROM EntityType WHERE [Name] = 'Rock.Model.Person')
  AND av.EntityId IN (SELECT PersonId FROM @FamilyPersonIds)
  AND NULLIF(av.[Value], '') IS NOT NULL;

SELECT @FirstTimeCheckIn AS FirstTimeCheckIn;

-- =========================================================
-- Result Set 5: AttendanceInLast16Weeks (family) — existing SP
-- =========================================================
EXEC dbo.spCheckin_WeeksAttendedInDuration
     @PersonId     = @pPersonId,
     @WeekDuration = 16;

-- =========================================================
-- OPTIONAL Result Set 6: Errors (0..N rows)
--   (Safe to ignore; consume if you want structured warnings)
--   Columns: Code, Message
-- =========================================================
SELECT Code, Message FROM @Errors;";

        #endregion
    }
}
