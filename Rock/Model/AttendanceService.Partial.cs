//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Attendance POCO Service class
    /// </summary>
    public partial class AttendanceService
    {
        /// <summary>
        /// Gets the specified attendance record.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="locationId">The location id.</param>
        /// <param name="scheduleId">The schedule id.</param>
        /// <param name="groupId">The group id.</param>
        /// <param name="personId">The person id.</param>
        /// <returns></returns>
        public Attendance Get( DateTime date, int locationId, int scheduleId, int groupId, int personId )
        {
            DateTime beginDate = date.Date;
            DateTime endDate = beginDate.AddDays( 1 );

            return this.Repository.AsQueryable()
                .Where( a =>
                    a.StartDateTime >= beginDate &&
                    a.StartDateTime < endDate &&
                    a.LocationId == locationId &&
                    a.ScheduleId == scheduleId &&
                    a.GroupId == groupId &&
                    a.PersonId == personId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the by date and location.
        /// </summary>
        /// <param name="locationId">The location id.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public IQueryable<Attendance> GetByDateAndLocation( DateTime date, int locationId )
        {
            DateTime beginDate = date.Date;
            DateTime endDate = beginDate.AddDays( 1 );

            return this.Repository.AsQueryable()
                .Where( a =>
                    a.StartDateTime >= beginDate &&
                    a.StartDateTime < endDate &&
                    !a.EndDateTime.HasValue &&
                    a.LocationId == locationId &&
                    a.DidAttend );
        }


    }
}
