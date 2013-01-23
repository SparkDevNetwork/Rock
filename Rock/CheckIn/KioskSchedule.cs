//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class KioskSchedule
    {
        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public Schedule Schedule { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskSchedule" /> class.
        /// </summary>
        public KioskSchedule()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskSchedule" /> class.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        public KioskSchedule( Schedule schedule )
            : base()
        {
            Schedule = schedule.Clone( false );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Schedule.ToString();
        }
    }
}