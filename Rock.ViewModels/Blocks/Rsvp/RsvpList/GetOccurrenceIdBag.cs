using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Rsvp.RsvpList
{
    public class GetOccurrenceIdBag
    {
        /// <summary>
        /// Date of the occurrence to get the Id for. This will be used to find an existing occurrence or create a new one if it doesn't exist.
        /// </summary>
        public DateTime OccurrenceDate { get; set;  }

        /// <summary>
        /// Id of the location used for the occurrence
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Id of the schedule used for the occurrence
        /// </summary>
        public int? ScheduleId { get; set; }
    }
}
