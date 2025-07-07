// <copyright>
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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 255, "17.2" )]
    public class MigrationRollupsForV17_2_1 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ObsoleteFunction_ufnUtility_CsvToTable();
            UpdateGroupTypeScheduleConfirmationEmailUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

        }

        #region Up Methods

        #region NA: Re-obsolete the ufnUtility_CsvToTable function
        private void ObsoleteFunction_ufnUtility_CsvToTable()
        {
            Sql( @"
/*
<doc>
	<summary>

        *** THIS FUNCTION IS OBSOLETE.  PLEASE USE STRING_SPLIT(@YourString, ',') INSTEAD. ***

 		This function converts a comma-delimited string of values into a table of values
        The original version came from http://www.sqlservercentral.com/articles/Tally+Table/72993/
	</summary>
	<returns>
		* id
	</returns>
	<remarks>
        (Previously) Used by:
            * spFinance_ContributionStatementQuery
            * spFinance_GivingAnalyticsQuery_AccountTotals
            * spFinance_GivingAnalyticsQuery_PersonSummary
            * spFinance_GivingAnalyticsQuery_TransactionData
            * spCheckin_AttendanceAnalyticsQuery_AttendeeDates
            * spCheckin_AttendanceAnalyticsQuery_AttendeeFirstDates
            * spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance
            * spCheckin_AttendanceAnalyticsQuery_Attendees
            * spCheckin_AttendanceAnalyticsQuery_NonAttendees
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnUtility_CsvToTable]('1,3,7,11,13') 
	</code>
</doc>
*/

/* #Obsolete# - Use STRING_SPLIT() instead */

ALTER FUNCTION [dbo].[ufnUtility_CsvToTable] 
(
 @pString VARCHAR(MAX)
)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN
    SELECT Item = TRY_CONVERT(INT, LTRIM(RTRIM(value)))
    FROM STRING_SPLIT(@pString, ',');
" );
        }

        #endregion

        #region SC: New SystemCommunication - Scheduling Confirmation Email (One Button)
        private void UpdateGroupTypeScheduleConfirmationEmailUp()
        {
            Sql( @"DECLARE @CategoryId INT = (SELECT [Id] FROM [Category] WHERE [Guid] = 'FA8F586E-4F71-4EB9-823C-E5E9576397AC');
DECLARE @Guid UNIQUEIDENTIFIER = 'BA1716E0-6B31-4E93-ABA1-42B3C81FDBDC';
DECLARE @Title NVARCHAR(100) = N'Scheduling Confirmation Email (One Button)';
DECLARE @Subject NVARCHAR(1000) = N'Scheduling Confirmation';
DECLARE @Body NVARCHAR(MAX) = N'{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1>Scheduling Confirmation</h1>
<p>Hi {{  Attendance.PersonAlias.Person.NickName  }}!</p>

<p>You have been added to the schedule for the following dates and times. Please let us know if you''ll be attending as soon as possible.</p>

<p>Thanks!</p>
{{ Attendance.ScheduledByPersonAlias.Person.FullName  }}
<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}

<table>
{% for attendance in Attendances %}
    <tr><td>&nbsp;</td></tr>
    <tr><td><h5>{{attendance.Occurrence.OccurrenceDate | Date:''dddd, MMMM d, yyyy''}}</h5></td></tr>
    <tr><td>{{ attendance.Occurrence.Group.Name }}</td></tr>
    <tr><td>{{ attendance.Location.Name }}&nbsp;{{ attendance.Schedule.Name }}</td></tr>
    {% if forloop.first  %}
    <tr><td>
        <!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:''ScheduleConfirm''}}"" style=""height:38px;v-text-anchor:middle;width:275px;"" arcsize=""5%"" strokecolor=""#009ce3"" fillcolor=""#33cfe3"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:18px;font-weight:normal;"">Required: Confirm or Decline</center>
    		  </v:roundrect>
    		<![endif]--><a style=""mso-hide:all; background-color:#009ce3;border:1px solid ##33cfe3;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:18px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:275px;-webkit-text-size-adjust:none;mso-hide:all;"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}ScheduleConfirmation?attendanceId={{attendance.Id}}&Person={{Attendance.PersonAlias.Person | PersonActionIdentifier:''ScheduleConfirm''}}"">Required: Confirm or Decline</a>&nbsp;
    </td></tr>
    <tr><td>&nbsp;</td></tr>
	{% endif %}
{% endfor %}
</table>

<br/>

{{ ''Global'' | Attribute:''EmailFooter'' }}';

IF NOT EXISTS (
    SELECT 1
    FROM [SystemCommunication]
    WHERE [Guid] = @Guid
)
BEGIN
	INSERT INTO [SystemCommunication] ([IsSystem], [IsActive], [CategoryId], [Guid], [CssInliningEnabled], [Title], [Subject], [Body])
	VALUES (1, 1, @CategoryId, @Guid, 0, @Title, @Subject, @Body);
END;

DECLARE @SystemCommunicationId INT = (SELECT [Id] FROM [SystemCommunication] WHERE [Guid] = @Guid);

UPDATE [GroupType] SET [ScheduleConfirmationSystemCommunicationId] = @SystemCommunicationId WHERE [ScheduleConfirmationSystemCommunicationId] IS NOT NULL;
    " );
        }

        #endregion

        #endregion

        #region Down Methods

        #endregion
    }
}