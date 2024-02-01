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
using Rock.Lava;
using Rock.Model;

namespace Rock.CheckIn
{
    /// <summary>
    /// A person option for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInPerson : ILavaDataDictionary, Lava.ILiquidizable, IHasAttributesWrapper
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
        /// Gets or sets a value indicating whether this <see cref="CheckInPerson" /> was automatically selected by a check-in action.
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

        /// <inheritdoc cref="Attendance.IsFirstTime"/>
        [DataMember]
        public bool FirstTime { get; set; } 

        /// <summary>
        /// Gets or sets the unique code for check-in labels
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
        /// Gets whether this person has any possible schedules available for check-in.
        /// </summary>
        /// <value>
        /// Whether this person has any possible schedules available for check-in.
        /// </value>
        public bool AnyPossibleSchedules => this.PossibleSchedules?.Any() == true;

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
        /// Gets the selected options.
        /// </summary>
        /// <value>
        /// The selected options.
        /// </value>
        public List<CheckInPersonSummary> SelectedOptions
        {
            get
            {
                return _selectedOptions;
            }
        }
        private List<CheckInPersonSummary> _selectedOptions = null;

        /// <summary>
        /// Gets or sets the reason this person has no check-in options.
        /// Note: this property may only be set once and will thereafter become immutable.
        /// </summary>
        /// <value>
        /// The reason this person has no check-in options.
        /// </value>
        public string NoOptionReason
        {
            get { return _noOptionReason; }
            set
            {
                if ( _noOptionReason.IsNullOrWhiteSpace() )
                {
                    _noOptionReason = value;
                }
            }
        }

        private string _noOptionReason;

        /// <summary>
        /// Sets the options.
        /// </summary>
        /// <param name="label">The label.</param>
        public void SetOptions( KioskLabel label )
        {
            _selectedOptions = new List<CheckInPersonSummary>();
            foreach ( var schedule in SelectedSchedules.Where( s => s.StartTime.HasValue ).OrderBy( s => s.StartTime ) )
            {
                var groupType = SelectedGroupTypes( schedule ).FirstOrDefault();
                if ( groupType != null )
                {
                    var group = groupType.SelectedGroups( schedule ).FirstOrDefault();
                    if ( group != null )
                    {
                        var location = group.SelectedLocations( schedule ).FirstOrDefault();
                        if ( location != null )
                        {
                            _selectedOptions.Add( new CheckInPersonSummary( label, schedule, groupType, group, location ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the available options for a person.
        /// </summary>
        /// <param name="onlyPreSelected">if set to <c>true</c> [only automatically/previously selected].</param>
        /// <param name="onlyOneOptionPerSchedule">if set to <c>true</c> [only one option per schedule].</param>
        /// <returns></returns>
        public List<CheckInPersonSummary> GetOptions( bool onlyPreSelected, bool onlyOneOptionPerSchedule )
        {
            var options = new List<CheckInPersonSummary>();

            foreach ( var possibleSchedule in PossibleSchedules )
            {
                var scheduleId = possibleSchedule.Schedule.Id;

                foreach ( var groupType in GroupTypes.Where( t => t.AvailableForSchedule.Contains( scheduleId ) && ( t.PreSelected || !onlyPreSelected ) ) )
                {
                    foreach ( var group in groupType.Groups.Where( t => t.AvailableForSchedule.Contains( scheduleId ) && ( t.PreSelected || !onlyPreSelected ) ) )
                    {
                        foreach ( var location in group.Locations.Where( t => t.AvailableForSchedule.Contains( scheduleId ) && ( t.PreSelected || !onlyPreSelected ) ) )
                        {
                            foreach ( var schedule in location.Schedules.Where( s => s.Schedule.Id == scheduleId && ( s.PreSelected || !onlyPreSelected ) ) )
                            {
                                if ( location.AvailableForSchedule.Contains( schedule.Schedule.Id ) && 
                                    ( !onlyOneOptionPerSchedule || !options.Any( o => o.Schedule.Schedule.Id == schedule.Schedule.Id ) ) )
                                {
                                    options.Add( new CheckInPersonSummary( schedule, groupType, group, location ) );
                                }
                            }
                        }
                    }
                }
            }

            return options;
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
                var selectedScheduleIds = SelectedSchedules.Select( s => s.Schedule.Id ).ToList();
                return GroupTypes.Where( t => t.Selected || t.SelectedForSchedule.Any( s => selectedScheduleIds.Contains( s ) ) ).ToList();
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
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string> { "FamilyMember", "LastCheckIn", "FirstTime", "SecurityCode", "GroupTypes", "SelectedOptions" };
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
        [LavaHidden]
        public object this[object key]
        {
            get
            {
                switch ( key.ToStringSafe() )
                {
                    case "FamilyMember": return FamilyMember;
                    case "LastCheckIn": return LastCheckIn;
                    case "FirstTime": return FirstTime;
                    case "SecurityCode": return SecurityCode;
                    case "GroupTypes": return GetGroupTypes( true );
                    case "SelectedOptions": return SelectedOptions;
                    default: return Person[key];
                }
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
        public object GetValue( string key )
        {
            return this[key];
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( string key )
        {
            var additionalKeys = new List<string> { "FamilyMember", "LastCheckIn", "FirstTime", "SecurityCode", "GroupTypes", "SelectedOptions" };
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

        #region ILiquidizable

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( object key )
        {
            var additionalKeys = new List<string> { "FamilyMember", "LastCheckIn", "FirstTime", "SecurityCode", "GroupTypes", "SelectedOptions" };
            if ( additionalKeys.Contains( key.ToStringSafe() ) )
            {
                return true;
            }
            else
            {
                return Person.ContainsKey( key.ToStringSafe() );
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
        public object GetValue( object key )
        {
            return this[key];
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

        #endregion

    }

    /// <summary>
    /// Helper class for summarizing the selected check-in
    /// </summary>
    [LavaType( "Schedule", "GroupType", "Group", "Location", "GroupTypeConfiguredForLabel" )]
    [DotLiquid.LiquidType( "Schedule", "GroupType", "Group", "Location", "GroupTypeConfiguredForLabel" )]
    public class CheckInPersonSummary
    {
        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        public CheckInSchedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        public CheckInGroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public CheckInGroup Group { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public CheckInLocation Location { get; set; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public String Key
        {
            get
            {
                return $"{GroupType?.GroupType.Id}|{Group?.Group.Id}|{Location?.Location.Id}|{Schedule?.Schedule.Id}";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CheckInPersonSummary"/> is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        public bool Selected {  get { return Schedule?.PreSelected ?? false; } }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInPersonSummary"/> is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if disabled; otherwise, <c>false</c>.
        /// </value>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [group type configured for label].
        /// </summary>
        /// <value>
        /// <c>true</c> if [group type configured for label]; otherwise, <c>false</c>.
        /// </value>
        public bool GroupTypeConfiguredForLabel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInPersonSummary"/> class.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="group">The group.</param>
        /// <param name="location">The location.</param>
        public CheckInPersonSummary( CheckInSchedule schedule, CheckInGroupType groupType, CheckInGroup group, CheckInLocation location )
        {
            Schedule = schedule;
            GroupType = groupType;
            Group = group;
            Location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInPersonSummary" /> class.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="schedule">The schedule.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="group">The group.</param>
        /// <param name="location">The location.</param>
        public CheckInPersonSummary( KioskLabel label, CheckInSchedule schedule, CheckInGroupType groupType, CheckInGroup group, CheckInLocation location )
            : this( schedule, groupType, group, location )
        {
            if ( groupType != null && groupType.GroupType != null && label != null )
            {
                if ( groupType.GroupType.Attributes == null )
                {
                    // shouldn't happen since GroupType is a ModelCache<,> type
                    return;
                }

                foreach ( var attribute in groupType.GroupType.Attributes.OrderBy( a => a.Value.Order ) )
                {
                    if ( attribute.Value.FieldType.Guid == SystemGuid.FieldType.LABEL.AsGuid() )
                    {
                        Guid? binaryFileGuid = groupType.GroupType.GetAttributeValue( attribute.Key ).AsGuidOrNull();
                        if ( binaryFileGuid.HasValue && binaryFileGuid.Value == label.Guid )
                        {
                            GroupTypeConfiguredForLabel = true;
                            break;
                        }
                    }
                }
            }
        }


    }
}