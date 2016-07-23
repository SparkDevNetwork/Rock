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

using Rock.Attribute;
using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A person option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInPerson : Lava.ILiquidizable, IHasAttributesWrapper
    {
        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public Person Person { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this person is a part of the family (vs. from a relationship).
        /// </summary>
        /// <value>
        ///   <c>true</c> if family member; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool FamilyMember { get; set; }

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
        /// Gets or sets a value indicating whether this <see cref="CheckInPerson" /> was selected for check-in.
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInPerson"/> is processed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Processed { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked in to any of the GroupTypes
        /// </summary>
        /// <value>
        /// The last check-in.
        /// </value>
        [DataMember]
        public DateTime? LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the person's first time checking in.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [first checkin]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool FirstTime { get; set; }

        /// <summary>
        /// Gets or sets the the unique code for check-in labels
        /// </summary>
        /// <value>
        /// The security code.
        /// </value>
        [DataMember]
        public string SecurityCode { get; set; }

        /// <summary>
        /// Gets or sets the group types available for the current person.
        /// </summary>
        /// <value>
        /// The group types.
        /// </value>
        [DataMember]
        public List<CheckInGroupType> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the possible schedules.
        /// </summary>
        /// <value>
        /// The possible schedules.
        /// </value>
        [DataMember]
        public List<CheckInSchedule> PossibleSchedules { get; set; }

        /// <summary>
        /// Gets or sets state parameters which can be used by workflow actions to track state of current person's check-in
        /// </summary>
        /// <value>
        /// The state parameters.
        /// </value>
        [DataMember]
        public Dictionary<string, string> StateParameters { get; set; }

        /// <summary>
        /// Gets the selected schedules.
        /// </summary>
        /// <value>
        /// The selected schedules.
        /// </value>
        public List<CheckInSchedule> SelectedSchedules
        {
            get { return PossibleSchedules.Where( s => s.Selected == true ).ToList(); }
        }

        /// <summary>
        /// Gets the current schedule if using family check-in mode.
        /// </summary>
        /// <value>
        /// The current schedule.
        /// </value>
        public CheckInSchedule CurrentSchedule
        {
            get
            {
                return SelectedSchedules.FirstOrDefault( s => !s.Processed );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInPerson" /> class.
        /// </summary>
        public CheckInPerson()
            : base()
        {
            GroupTypes = new List<CheckInGroupType>();
            PossibleSchedules = new List<CheckInSchedule>();
            StateParameters = new Dictionary<string, string>();
        }

        /// <summary>
        /// Clears the filtered exclusions.
        /// </summary>
        public void ClearFilteredExclusions()
        {
            ExcludedByFilter = false;
            foreach ( var groupType in GroupTypes )
            {
                groupType.ClearFilteredExclusions();
            }
        }


        /// <summary>
        /// Returns the selected group types.
        /// </summary>
        /// <param name="currentSchedule">The current schedule.</param>
        /// <returns></returns>
        public List<CheckInGroupType> SelectedGroupTypes( CheckInSchedule currentSchedule )
        {
            return ( currentSchedule != null && currentSchedule.Schedule != null ) ?
                GroupTypes.Where( t => t.SelectedForSchedule.Contains( currentSchedule.Schedule.Id ) ).ToList() :
                GroupTypes.Where( t => t.Selected ).ToList();
        }

        /// <summary>
        /// Gets the group types.
        /// </summary>
        /// <param name="selectedOnly">if set to <c>true</c> [selected only].</param>
        /// <returns></returns>
        public List<CheckInGroupType> GetGroupTypes( bool selectedOnly )
        {
            if ( selectedOnly )
            {
                if ( PossibleSchedules.Any() )
                {
                    var selectedScheduleIds = SelectedSchedules.Select( s => s.Schedule.Id ).ToList();
                    return GroupTypes.Where( t => t.SelectedForSchedule.Any( s => selectedScheduleIds.Contains( s ) ) ).ToList();
                }

                return GroupTypes.Where( t => t.Selected ).ToList();
            }

            return GroupTypes;
        }

        /// <summary>
        /// Gets the available group types.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        public List<CheckInGroupType> GetAvailableGroupTypes( CheckInSchedule schedule )
        {
            var groupTypes = GroupTypes.Where( t => !t.ExcludedByFilter );
            if ( schedule != null )
            {
                groupTypes = groupTypes.Where( t => t.AvailableForSchedule.Contains( schedule.Schedule.Id ) );
            }
            return groupTypes.ToList();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Person != null ? Person.ToString() : string.Empty;
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
                var availableKeys = new List<string> { "FamilyMember", "LastCheckIn", "FirstTime", "SecurityCode" };
                if ( this.Person != null )
                {
                    availableKeys.AddRange( this.Person.AvailableKeys );
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
               switch( key.ToStringSafe() )
               {
                   case "FamilyMember": return FamilyMember;
                   case "LastCheckIn": return LastCheckIn;
                   case "FirstTime": return FirstTime;
                   case "SecurityCode": return SecurityCode;
                   default: return Person[key];
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
            var additionalKeys = new List<string> { "FamilyMember", "LastCheckIn", "FirstTime", "SecurityCode" };
            if ( additionalKeys.Contains( key.ToStringSafe() ) )
            {
                return true;
            }
            else
            {
                return Person.ContainsKey( key );
            }
        }

        /// <summary>
        /// Gets the property that has attributes.
        /// </summary>
        public IHasAttributes HasAttributesEntity
        {
            get { return Person; }
        }
    }
}