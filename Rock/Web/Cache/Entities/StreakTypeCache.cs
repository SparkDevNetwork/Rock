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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a streak type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class StreakTypeCache : ModelCache<StreakTypeCache, StreakType>
    {
        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the name of the streak type. This property is required.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a description of the streak type.
        /// </summary>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the attendance association (<see cref="Rock.Model.StreakStructureType"/>). If not set, this streak type
        /// will not produce attendance records.
        /// </summary>
        [DataMember]
        public StreakStructureType? StructureType { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the Entity associated with attendance for this streak type.
        /// </summary>
        [DataMember]
        public int? StructureEntityId { get; private set; }

        /// <summary>
        /// This determines whether the streak type will write attendance records when marking someone as present or
        /// if it will just update the enrolled individual’s map.
        /// </summary>
        [DataMember]
        public bool EnableAttendance { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if this streak type requires explicit enrollment. If not set, a person can be
        /// implicitly enrolled through attendance.
        /// </summary>
        [DataMember]
        public bool RequiresEnrollment { get; private set; }

        /// <summary>
        /// Gets or sets the timespan that each map bit represents (<see cref="Rock.Model.StreakOccurrenceFrequency"/>).
        /// </summary>
        [DataMember]
        public StreakOccurrenceFrequency OccurrenceFrequency { get; private set; }

        /// <summary>
        /// Gets or sets the first day of the week for Weekly streak types. Default to system setting if null.
        /// </summary>
        [DataMember]
        public DayOfWeek? FirstDayOfWeek { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> associated with the first bit of this streak type.
        /// </summary>
        [DataMember]
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// The streak type of bits that represent occurrences where attendance was possible. The least significant bit (right side) is
        /// representative of the StartDate. More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        public byte[] OccurrenceMap { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this streak type is active.
        /// </summary>
        [DataMember]
        public bool IsActive { get; private set; }

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

        /// <summary>
        /// Gets or sets the structure settings.
        /// </summary>
        /// <value>The structure settings.</value>
        [NotMapped]
        public Rock.Model.Engagement.StreakType.StreakTypeSettings StructureSettings { get; set; } = new Rock.Model.Engagement.StreakType.StreakTypeSettings();

        /// <summary>
        /// Gets a value indicating whether this streak type is releated to interactions.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance related to interactions; otherwise, <c>false</c>.
        /// </value>
        public bool IsInteractionRelated
        {
            get
            {
                return (
                        StructureType == StreakStructureType.InteractionChannel
                        || StructureType == StreakStructureType.InteractionComponent
                        || StructureType == StreakStructureType.InteractionMedium
                       );
            }
        }

        /// <summary>
        /// Gets the Streak Type Exclusions
        /// </summary>
        /// <value>
        /// The exclusion values.
        /// </value>
        public List<StreakTypeExclusionCache> StreakTypeExclusions
        {
            get
            {
                var streakTypeExclusions = new List<StreakTypeExclusionCache>();

                if ( _streakTypeExclusionIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _streakTypeExclusionIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _streakTypeExclusionIds = new StreakTypeExclusionService( rockContext )
                                    .GetByStreakTypeId( Id )
                                    .AsNoTracking()
                                    .Select( soe => soe.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                foreach ( var id in _streakTypeExclusionIds )
                {
                    var streakTypeExclusionCache = StreakTypeExclusionCache.Get( id );
                    if ( streakTypeExclusionCache != null )
                    {
                        streakTypeExclusions.Add( streakTypeExclusionCache );
                    }
                }

                return streakTypeExclusions;
            }
        }
        private List<int> _streakTypeExclusionIds = null;

        /// <summary>
        /// Reloads the exclusion values.
        /// </summary>
        [Obsolete("This will not work with a distributed cache system such as Redis. The StreakType will need to be flushed from cache.")]
        [RockObsolete("1.10")]
        public void ReloadOccurrenceExclusions()
        {
            // set ids to null so it load them all at once on demand
            _streakTypeExclusionIds = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var sourceModel = entity as StreakType;
            if ( sourceModel == null )
            {
                return;
            }

            Name = sourceModel.Name;
            Description = sourceModel.Description;
            IsActive = sourceModel.IsActive;
            StructureType = sourceModel.StructureType;
            StructureEntityId = sourceModel.StructureEntityId;
            EnableAttendance = sourceModel.EnableAttendance;
            RequiresEnrollment = sourceModel.RequiresEnrollment;
            OccurrenceFrequency = sourceModel.OccurrenceFrequency;
            StartDate = sourceModel.StartDate;
            OccurrenceMap = sourceModel.OccurrenceMap;
            FirstDayOfWeek = sourceModel.FirstDayOfWeek;
            StructureSettingsJSON = sourceModel.StructureSettingsJSON;

            _streakTypeExclusionIds = null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}