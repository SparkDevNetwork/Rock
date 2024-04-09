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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Utility;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Streak Type in Rock.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "StreakType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "66203975-2A7A-4000-870E-76457DF3C920")]
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
        [CodeGenExclude( CodeGenFeature.ViewModelFile )]
        public byte[] OccurrenceMap { get; set; }

        /// <summary>
        /// Gets or sets the structure settings JSON.
        /// </summary>
        /// <value>The structure settings JSON.</value>
        [DataMember]
        public string StructureSettingsJSON
        {
            get
            {
                return StructureSettings?.ToJson();
            }

            set
            {
                StructureSettings = value.FromJsonOrNull<Rock.Model.Engagement.StreakType.StreakTypeSettings>() ?? new Rock.Model.Engagement.StreakType.StreakTypeSettings();
            }
        }

        #endregion Entity Properties

        #region IHasActiveFlag

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion IHasActiveFlag

        #region Navigation Properties

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

        #endregion Navigation Properties

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    #region Entity Configuration

    /// <summary>
    /// StreakType Configuration Class
    /// </summary>
    public partial class StreakTypeConfiguration : EntityTypeConfiguration<StreakType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StreakTypeConfiguration"/> class.
        /// </summary>
        public StreakTypeConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
