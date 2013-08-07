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
    /// A group option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInGroup : DotLiquid.ILiquidizable
    {
        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public Group Group { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInGroup" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked into this group for any of the schedules
        /// </summary>
        /// <value>
        /// The last check in.
        /// </value>
        [DataMember]
        public DateTime? LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the locations that are available for the current group
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        [DataMember]
        public List<CheckInLocation> Locations { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInGroup" /> class.
        /// </summary>
        public CheckInGroup()
            : base()
        {
            Locations = new List<CheckInLocation>();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Group != null ? Group.ToString() : string.Empty;
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add( "Group", Group );
            dictionary.Add( "Selected", Selected );
            dictionary.Add( "LastCheckIn", LastCheckIn );
            dictionary.Add( "Locations", Locations );
            return dictionary;
        }
    }
}