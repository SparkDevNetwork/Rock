//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class KioskGroupType
    {
        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public GroupType GroupType { get; set; }

        /// <summary>
        /// Next time that a location/group/schedule will be active for
        /// this group type.  If the group type has locations, this time
        /// will be in the past, if there are no locations, this time would
        /// be in the future
        /// </summary>
        /// <value>
        /// The next active time.
        /// </value>
        [DataMember]
        public DateTimeOffset NextActiveTime { get; set; }

        /// <summary>
        /// All groups with active schedules
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        [DataMember]
        public List<KioskGroup> KioskGroups { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroupType" /> class.
        /// </summary>
        public KioskGroupType()
            : base()
        {
            KioskGroups = new List<KioskGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KioskGroupType" /> class.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        public KioskGroupType( GroupType groupType )
            : base()
        {
            GroupType = groupType.Clone( false );
            KioskGroups = new List<KioskGroup>();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GroupType.ToString();
        }
    }
}