using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Quartz;
using Rock;
using Rock.Data;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Field.Types;
using Rock.Model;
using System.Data.SqlClient;

namespace org.lakepointe.RockJobCustomizations
{
    [DateField(
        name: "Start Date",
        description: "Earliest Occurrence Date of occurrences to verify/update.",
        required: true,
        order: 0 )]
    [DateField(
        name: "End Date",
        description: "Latest Occurrence Date of occurrences to verify/update",
        required: false,
        order: 1 )]
    [GroupTypeField(
        name: "Group Type",
        description: "Optional Group Type to limit the Occurrence Groups to.",
        required: false,
        order: 2 )]
    [PersonField(
        name: "Modified By Person",
        description: "The person who should be logged as who performed the update.",
        required: false,
        order: 3)]
    [DisallowConcurrentExecution]
    public class AttendanceCleanup : IJob
    {
        #region Fields
        DateTime _startDate;
        DateTime? _endDate;
        GroupTypeCache _groupTypeFilter = null;
        int? _personAliasId = null;

        #endregion

        #region Public Methods
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int attendanceRecordsUpdated = 0;
            SetJobAttributeValues( dataMap );

            var rockContext = new RockContext();
            var occurrenceService = new AttendanceOccurrenceService( rockContext );

            var occurrenceQry = occurrenceService.Queryable( "Schedule" )
                .Where( o => o.ScheduleId.HasValue )
                .Where( o => o.OccurrenceDate >= _startDate );

            if (_endDate.HasValue)
            {
                occurrenceQry = occurrenceQry.Where( o => o.OccurrenceDate <= _endDate.Value );
            }

            if (_groupTypeFilter != null && _groupTypeFilter.Id > 0)
            {
                occurrenceQry = occurrenceQry.Where( o => o.Group.GroupTypeId == _groupTypeFilter.Id );
            }

            var occurrences = occurrenceQry.Where( o => o.Attendees.Count( a => !a.AttendanceCodeId.HasValue ) > 0 )
                .OrderBy( o => o.OccurrenceDate )
                .ToList();

            foreach (var o in occurrences)
            {
                var occurrenceStart = o.Schedule != null && o.Schedule.HasSchedule() ? o.OccurrenceDate.Date.Add( o.Schedule.StartTimeOfDay ) : o.OccurrenceDate;

                var sql = @"
                    UPDATE [dbo].[Attendance]
                    SET
                        [StartDateTime] = @StartDateTime,
                        [ModifiedDateTime] = GETDATE(),
                        [ModifiedByPersonAliasId] = @ModifiedByAliasId
                    WHERE
                        [OccurrenceId] = @OccurrenceId
                        AND [AttendanceCodeId] IS NULL
                        AND [StartDateTime] != @StartDateTime

                    SELECT @@RowCount
                ";

                var startDateTime = new SqlParameter( "@StartDateTime", occurrenceStart );
                var occurrenceId = new SqlParameter( "@OccurrenceId", o.Id );

                var modifiedBy = new SqlParameter();
                modifiedBy.ParameterName = "@ModifiedByAliasId";

                if (_personAliasId.HasValue)
                {
                    modifiedBy.Value = _personAliasId.Value;
                }
                else
                {
                    modifiedBy.Value = DBNull.Value;
                }


                attendanceRecordsUpdated += rockContext.Database.SqlQuery<int>( sql, startDateTime, occurrenceId, modifiedBy ).FirstOrDefault();
            }

            context.Result = string.Format( "{0} {1} updated.", attendanceRecordsUpdated, "attendee".PluralizeIf( attendanceRecordsUpdated != 1) );

        }
        #endregion

        #region Private Methods
        private void SetJobAttributeValues( JobDataMap map )
        {
            var context = new RockContext();
            _startDate = map.GetString( "StartDate" ).AsDateTime().Value;
            _endDate = map.GetString( "EndDate" ).AsDateTime();
            var groupTypeGuid = map.GetString( "GroupType" ).AsGuidOrNull();

            if (groupTypeGuid.HasValue)
            {
                _groupTypeFilter = GroupTypeCache.Get( groupTypeGuid.Value, context );
            }

            var personAliasGuid = map.GetString( "ModifiedByPerson" ).AsGuidOrNull();

            if (personAliasGuid.HasValue)
            {
                var alias = new PersonAliasService( context ).Get( personAliasGuid.Value );

                if (alias != null && alias.Id > 0)
                {
                    _personAliasId = alias.Id;
                }
            }
        }
        #endregion


    }

}
