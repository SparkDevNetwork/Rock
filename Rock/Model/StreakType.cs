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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Streak Type in Rock.
    /// </summary>
    [RockDomain( "Streaks" )]
    [Table( "StreakType" )]
    [DataContract]
    public partial class StreakType : Model<StreakType>, IHasActiveFlag, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Streak Type. This property is required.
        /// </summary>
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the Streak Type.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the attendance association (<see cref="Rock.Model.StreakStructureType"/>). If not set, this streak type
        /// will not be associated with attendance.
        /// </summary>
        [DataMember]
        public StreakStructureType? StructureType { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Entity associated with attendance for this streak type. If not set, this streak type
        /// will account for any attendance record.
        /// </summary>
        [DataMember]
        public int? StructureEntityId { get; set; }

        /// <summary>
        /// This determines whether the streak type will write attendance records when marking someone as present or
        /// if it will just update the enrolled individual’s map.
        /// </summary>
        [DataMember]
        public bool EnableAttendance { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this streak type requires explicit enrollment. If not set, a person can be
        /// implicitly enrolled through attendance.
        /// </summary>
        [DataMember]
        public bool RequiresEnrollment { get; set; }

        /// <summary>
        /// Gets or sets the timespan that each map bit represents (<see cref="Rock.Model.StreakOccurrenceFrequency"/>).
        /// </summary>
        [DataMember( IsRequired = true )]
        [Required]
        public StreakOccurrenceFrequency OccurrenceFrequency { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> associated with the least significant bit of all maps in this streak type.
        /// </summary>
        [DataMember]
        [Required]
        [Column( TypeName = "Date" )]
        public DateTime StartDate
        {
            get => _startDate;
            set => _startDate = value.Date;
        }
        private DateTime _startDate = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the first day of the week for <see cref="StreakOccurrenceFrequency.Weekly"/> streak type calculations.
        /// Leave this null to assume the system setting, which is accessed via <see cref="RockDateTime.FirstDayOfWeek"/>.
        /// </summary>
        /// <value>
        /// The first day of week.
        /// </value>
        [DataMember]
        public DayOfWeek? FirstDayOfWeek { get; set; }

        /// <summary>
        /// The sequence of bits that represent occurrences where engagement was possible. The least significant bit (right side) is
        /// representative of the StartDate. More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        public byte[] OccurrenceMap { get; set; }

        #endregion Entity Properties

        #region IHasActiveFlag

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion IHasActiveFlag

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return StreakTypeCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            StreakTypeCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Streak">Streaks</see> that are of this streak type.
        /// </summary>
        [DataMember]
        public virtual ICollection<Streak> Streaks
        {
            get => _streaks ?? ( _streaks = new Collection<Streak>() );
            set => _streaks = value;
        }
        private ICollection<Streak> _streaks;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="StreakTypeExclusion">StreakTypeExclusions</see>
        /// that are of this streak type.
        /// </summary>
        [DataMember]
        public virtual ICollection<StreakTypeExclusion> StreakTypeExclusions
        {
            get => _streakTypeExclusions ?? ( _streakTypeExclusions = new Collection<StreakTypeExclusion>() );
            set => _streakTypeExclusions = value;
        }
        private ICollection<StreakTypeExclusion> _streakTypeExclusions;

        #endregion Virtual Properties
    }

    #region Enumerations

    /// <summary>
    /// Represents the attendance association of a <see cref="StreakType"/>.
    /// </summary>
    public enum StreakStructureType
    {
        /// <summary>
        /// The <see cref="StreakType"/> is associated with any attendance record.
        /// </summary>
        AnyAttendance = 0,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with attendance to a single group.
        /// </summary>
        Group = 1,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with attendance to groups of a given type.
        /// </summary>
        GroupType = 2,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with attendance to groups within group types of a common purpose (defined type).
        /// </summary>
        GroupTypePurpose = 3,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with attendance specified by a check-in configuration.
        /// </summary>
        CheckInConfig = 4,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with interactions in a certain channel.
        /// </summary>
        InteractionChannel = 5,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with interactions in a certain component.
        /// </summary>
        InteractionComponent = 6,

        /// <summary>
        /// The <see cref="StreakType"/> is associated with interactions over a certain.
        /// </summary>
        InteractionMedium = 7
    }

    /// <summary>
    /// Represents the timespan represented by each of the <see cref="StreakType"/> bits.
    /// </summary>
    public enum StreakOccurrenceFrequency
    {
        /// <summary>
        /// The <see cref="StreakType"/> has bits that represent a day.
        /// </summary>
        Daily = 0,

        /// <summary>
        /// The <see cref="StreakType"/> has bits that represent a week.
        /// </summary>
        Weekly = 1
    }

    #endregion
}
