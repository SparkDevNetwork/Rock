// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    /// A group option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInGroup : Lava.ILiquidizable
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
        /// Gets or sets a value indicating whether this <see cref="CheckInGroup" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets the available for schedule.
        /// </summary>
        /// <value>
        /// The available for schedule.
        /// </value>
        [DataMember]
        public List<int> AvailableForSchedule { get; set; }

        /// <summary>
        /// Gets or sets the selected for schedule.
        /// </summary>
        /// <value>
        /// The selected for schedule.
        /// </value>
        [DataMember]
        public List<int> SelectedForSchedule { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked into this group for any of the schedules
        /// </summary>
        /// <value>
        /// The last check-in.
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
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        [DataMember]
        public string Notes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInGroup" /> class.
        /// </summary>
        public CheckInGroup()
            : base()
        {
            Locations = new List<CheckInLocation>();
            SelectedForSchedule = new List<int>();
            AvailableForSchedule = new List<int>();
        }

        /// <summary>
        /// Clears the filtered exclusions.
        /// </summary>
        public void ClearFilteredExclusions()
        {
            ExcludedByFilter = false;
            foreach ( var location in Locations )
            {
                location.ClearFilteredExclusions();
            }
        }

        /// <summary>
        /// Returns the selected locations
        /// </summary>
        /// <param name="currentSchedule">The current schedule.</param>
        /// <returns></returns>
        public List<CheckInLocation> SelectedLocations( CheckInSchedule currentSchedule )
        {
            return ( currentSchedule != null && currentSchedule.Schedule != null ) ?
                Locations.Where( l => l.SelectedForSchedule.Contains( currentSchedule.Schedule.Id ) ).ToList() :
                Locations.Where( l => l.Selected ).ToList();
        }

        /// <summary>
        /// Gets the locations.
        /// </summary>
        /// <param name="selectedOnly">if set to <c>true</c> [selected only].</param>
        /// <returns></returns>
        public List<CheckInLocation> GetLocations( bool selectedOnly )
        {
            if ( selectedOnly )
            {
                return Locations.Where( l => l.Selected || l.SelectedForSchedule.Any( s => SelectedForSchedule.Contains( s ) ) ).ToList();
            }

            return Locations;
        }

        /// <summary>
        /// Gets the available locations.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        public List<CheckInLocation> GetAvailableLocations( CheckInSchedule schedule )
        {
            var locations = Locations.Where( t => !t.ExcludedByFilter );
            if ( schedule != null )
            {
                locations = locations.Where( t => t.AvailableForSchedule.Contains( schedule.Schedule.Id ) );
            }
            return locations.ToList();
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
            return this;
        }

        /// <summary>
        /// Gets the available keys (for debugging info).
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
                if ( this.Group != null )
                {
                    availableKeys.AddRange( this.Group.AvailableKeys );
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
                    case "Locations": return GetLocations( true );
                    default: return Group[key];
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
            var additionalKeys = new List<string> { "LastCheckIn", "Locations" };
            if ( additionalKeys.Contains( key.ToStringSafe() ) )
            {
                return true;
            }
            else
            {
                return Group.ContainsKey( key );
            }
        }
    }
}