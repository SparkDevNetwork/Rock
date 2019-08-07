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

using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// A group type option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInGroupType : Lava.ILiquidizable
    {
        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public GroupTypeCache GroupType { get; set; }

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
        /// Gets or sets a value indicating whether this <see cref="CheckInGroupType" /> is selected for check-in
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
        /// Gets or sets the last time person checked in to any of the Locations for this group type
        /// </summary>
        /// <value>
        /// The last check-in.
        /// </value>
        [DataMember]
        public DateTime? LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets the groups that are of the current group type
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        [DataMember]
        public List<CheckInGroup> Groups { get; set; }

        /// <summary>
        /// Gets or sets the labels to be printed after successful check-in
        /// </summary>
        /// <value>
        /// The labels.
        /// </value>
        [DataMember]
        public List<CheckInLabel> Labels { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInGroupType" /> class.
        /// </summary>
        public CheckInGroupType()
            : base()
        {
            Groups = new List<CheckInGroup>();
            SelectedForSchedule = new List<int>();
            AvailableForSchedule = new List<int>();
        }

        /// <summary>
        /// Clears the filtered exclusions.
        /// </summary>
        public void ClearFilteredExclusions()
        {
            ExcludedByFilter = false;
            foreach ( var group in Groups )
            {
                group.ClearFilteredExclusions();
            }
        }

        /// <summary>
        /// Returns the selected groups.
        /// </summary>
        /// <param name="currentSchedule">The current schedule.</param>
        /// <returns></returns>
        public List<CheckInGroup> SelectedGroups( CheckInSchedule currentSchedule )
        {
            return ( currentSchedule != null && currentSchedule.Schedule != null ) ?
                Groups.Where( g => g.SelectedForSchedule.Contains( currentSchedule.Schedule.Id ) ).ToList() :
                Groups.Where( g => g.Selected ).ToList();
        }

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <param name="selectedOnly">if set to <c>true</c> [selected only].</param>
        /// <returns></returns>
        public List<CheckInGroup> GetGroups( bool selectedOnly)
        {
            if ( selectedOnly )
            {
                return Groups.Where( g => g.Selected || g.SelectedForSchedule.Any( s => SelectedForSchedule.Contains( s ) ) ).ToList();
            }

            return Groups;
        }

        /// <summary>
        /// Gets the available groups.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        public List<CheckInGroup> GetAvailableGroups( CheckInSchedule schedule )
        {
            var groups = Groups.Where( t => !t.ExcludedByFilter );
            if ( schedule != null )
            {
                groups = groups.Where( t => t.AvailableForSchedule.Contains( schedule.Schedule.Id ) );
            }
            return groups.ToList();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return GroupType != null ? GroupType.ToString() : string.Empty;
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
                var availableKeys = new List<string> { "LastCheckIn", "Groups" };
                if ( this.GroupType != null )
                {
                    availableKeys.AddRange( this.GroupType.AvailableKeys );
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
                    case "Groups": return GetGroups( true );
                    default: return GroupType[key];
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
            var additionalKeys = new List<string> { "LastCheckIn", "Groups" };
            if ( additionalKeys.Contains( key.ToStringSafe() ) )
            {
                return true;
            }
            else
            {
                return GroupType.ContainsKey( key );
            }
        }
    }
}