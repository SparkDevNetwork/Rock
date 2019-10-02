using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.BulkImport
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Used with POST to ~/api/Attendances/Import" )]
    public class AttendanceImport
    {
        /// <summary>
        /// The Id of the <see cref="Rock.Model.Group"/> that the <see cref="Rock.Model.Person"/> attended/checked in to.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets the Id of the <see cref="Rock.Model.Location"/> that the <see cref="Rock.Model.Person"/> attended/checked in to.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int? LocationId { get; set; }

        /// <summary>
        /// The Id of the <see cref="Rock.Model.Schedule" /> that the <see cref="Rock.Model.Person"/> attended/checked in to.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// The date of the Attendance. Only the date is used.
        /// </summary>
        /// <value>
        /// The occurrence date.
        /// </value>
        public DateTime OccurrenceDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time that person attended/checked in
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// The Id of the <seealso cref="Rock.Model.Person"/> that attended/checked in.
        /// NOTE: This can be left null if PersonAliasId is specified.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// If PersonId is not specified, the specified PersonAliasId will be used to identify the person that checked in
        /// NOTE: If PersonId is specified, the PersonAliasId will be ignored (PersonID takes precedence if both are specified)
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Returns a string which is combination of pipe delimited GroupId,LocationId,ScheduleId,OccurenceDate (for quick lookup)
        /// </summary>
        /// <returns></returns>
        internal string GetOccurrenceLookupKey()
        {
            return AttendanceImport.GetOccurrenceLookupKey( this.GroupId, this.LocationId, this.ScheduleId, this.OccurrenceDate );
        }

        /// <summary>
        /// Returns a string which is combination of pipe delimited GroupId,LocationId,ScheduleId,OccurenceDate (for quick lookup)
        /// </summary>
        /// <returns></returns>
        internal static string GetOccurrenceLookupKey(int? groupId, int? locationId, int? scheduleId, DateTime occurrenceDate )
        {
            return $"{groupId}|{locationId}|{scheduleId}|{occurrenceDate}";
        }
    }
}
