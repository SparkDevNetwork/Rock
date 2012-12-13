//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class KioskGroup : GroupDto
    {
        /// <summary>
        /// The schedules that are currently active
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        [DataMember]
        public List<KioskSchedule> Schedules { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroup" /> class.
        /// </summary>
        public KioskGroup()
            : base()
        {
            Schedules = new List<KioskSchedule>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroup" /> class.
        /// </summary>
        /// <param name="group">The group.</param>
        public KioskGroup( Group group )
            : base( group )
        {
            Schedules = new List<KioskSchedule>();
        }
    }
}