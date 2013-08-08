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
    /// A location option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInLocation : DotLiquid.ILiquidizable
    {
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInLocation" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked into any of the groups for this location and group type
        /// </summary>
        /// <value>
        /// The last check-in.
        /// </value>
        [DataMember]
        public DateTime? LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the schedules that are available for the current group location
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        [DataMember]
        public List<CheckInSchedule> Schedules { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInLocation" /> class.
        /// </summary>
        public CheckInLocation()
            : base()
        {
            Schedules = new List<CheckInSchedule>();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Location != null ? Location.ToString() : string.Empty;
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary.Add( "Location", Location );
            dictionary.Add( "Selected", Selected );
            dictionary.Add( "LastCheckIn", LastCheckIn );
            dictionary.Add( "Schedules", Schedules );
            return dictionary;
        }
    }
}