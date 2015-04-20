// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A location option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInLocation : Lava.ILiquidizable
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
        /// Gets or sets the campu identifier.
        /// </summary>
        /// <value>
        /// The campu identifier.
        /// </value>
        [DataMember]
        public int? CampuId { get; set; }

        /// <summary>
        /// Gets or sets the current count.
        /// </summary>
        /// <value>
        /// The current count.
        /// </value>
        [DataMember]
        [Obsolete( "Not Used. This will be removed post McKinley." )]
        public int CurrentCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [excluded by filter].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [excluded by filter]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ExcludedByFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInPerson" /> was pre-selected by a check-in action.
        /// </summary>
        /// <value>
        ///   <c>true</c> if preselected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PreSelected { get; set; }

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
        /// Clears the filtered exclusions.
        /// </summary>
        public void ClearFilteredExclusions()
        {
            ExcludedByFilter = false;
            foreach ( var schedule in Schedules )
            {
                schedule.ExcludedByFilter = false;
            }
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
            return this;
        }

        /// <summary>
        /// Gets the available keys (for debuging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [Rock.Data.LavaIgnore]
        public List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string> { "LastCheckIn", "Locations" };
                if ( this.Location != null )
                {
                    availableKeys.AddRange( this.Location.AvailableKeys );
                }
                return availableKeys;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [Rock.Data.LavaIgnore]
        public object this[object key]
        {
            get
            {
                switch ( key.ToStringSafe() )
                {
                    case "LastCheckIn": return LastCheckIn;
                    case "Schedules": return Schedules.Where( s => s.Selected ).ToList();
                    default: return Location[key];
                }
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( object key )
        {
            var additionalKeys = new List<string> { "LastCheckIn", "Schedules" };
            if ( additionalKeys.Contains( key.ToStringSafe() ) )
            {
                return true;
            }
            else
            {
                return Location.ContainsKey( key );
            }
        }
    }
}