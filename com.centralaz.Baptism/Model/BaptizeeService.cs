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
        /// <param name="firstDay">The day baptisms need to be held after</param>
        /// <param name="lastDay">The day baptisms need to be held before</param>
        /// <param name="groupId">the campus the baptisms must be held on</param>
        /// <returns>a list of baptizees that match the above criteria</returns>
        public List<Baptizee> GetBaptizeesByDateRange( DateTime firstDay, DateTime lastDay, int groupId )
        {
            List<Baptizee> baptizeeList = Queryable()
                .Where( b => b.BaptismDateTime.Day >= firstDay.Day && b.BaptismDateTime.Day <= lastDay.Day && b.GroupId == groupId)
                .ToList();
            return baptizeeList;
        }

        /// <summary>
        /// Creates a list of all the baptizees on a certain campus
        /// </summary>
        /// <param name="groupId">The campus the baptisms must be held on</param>
        /// <returns>A list of baptizees that match the above criteria</returns>
        public List<Baptizee> GetAllBaptizees(int groupId)
        {
            List<Baptizee> baptizeeList = Queryable()
                .Where(b=> b.GroupId==groupId)
                .ToList();
            return baptizeeList;
        }
    }
}
