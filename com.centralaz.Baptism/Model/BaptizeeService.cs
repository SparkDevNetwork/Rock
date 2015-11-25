using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using com.centralaz.Baptism.Data;
using Rock.Model;

namespace com.centralaz.Baptism.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class BaptizeeService : BaptismService<Baptizee>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaptizeeService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public BaptizeeService( BaptismContext context ) : base( context ) { }

        /// <summary>
        /// Creates a list of the baptizees within a certain date range
        /// </summary>
        /// <param name="startDate">The day baptisms need to be held after</param>
        /// <param name="endDate">The day baptisms need to be held before</param>
        /// <param name="groupId">The campus the baptisms must be held on</param>
        /// <param name="showDeleted">Whether to show deleted baptisms</param>
        /// <returns>a list of baptizees that match the above criteria</returns>
        public List<Baptizee> GetBaptizeesByDateRange( DateTime startDate, DateTime endDate, int groupId, bool showDeleted = false )
        {
            // For a little extra safety, since we are doing a Date comparison (not a DateTime comparison)
            // get just the Date portion without the time (just in case).
            startDate = startDate.Date;
            endDate = endDate.Date;

            // Get the records equal to and greater than the start date, but add a whole day to 
            // the selected end date since users will expect to see all the stuff that happened 
            // on the end date up until the very end of that day.

            // calculate the query end date before including it in the qry statement to avoid Linq error
            endDate = endDate.AddDays( 1 );

            List<Baptizee> baptizeeList = Queryable()
                .Where( b =>
                    ( b.BaptismDateTime >= startDate && b.BaptismDateTime <= endDate ) &&
                    b.GroupId == groupId &&
                    ( b.IsDeleted == false || b.IsDeleted == null || showDeleted ) )
                    .OrderBy( b => b.BaptismDateTime )
                .ToList();
            return baptizeeList;
        }

        /// <summary>
        /// Creates a list of all the baptizees on a certain campus
        /// </summary>
        /// <param name="groupId">The campus the baptisms must be held on</param>
        /// <param name="showDeleted">Whether to show deleted baptisms</param>
        /// <returns>A list of baptizees that match the above criteria</returns>
        public List<Baptizee> GetAllBaptizees( int groupId, bool showDeleted = false )
        {
            List<Baptizee> baptizeeList = Queryable()
                .Where( b =>
                    b.GroupId == groupId &&
                    ( b.IsDeleted == false || showDeleted ) )
                .ToList();
            return baptizeeList;
        }
    }
}
