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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a sequence that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class SequenceCache : ModelCache<SequenceCache, Sequence>
    {
        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the name of the sequence. This property is required.
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a description of the sequence.
        /// </summary>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the attendance association (<see cref="Rock.Model.SequenceStructureType"/>). If not set, this sequence
        /// will not produce attendance records.
        /// </summary>
        [DataMember]
        public SequenceStructureType? StructureType { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the Entity associated with attendance for this sequence.
        /// </summary>
        [DataMember]
        public int? StructureEntityId { get; private set; }

        /// <summary>
        /// This determines whether the sequence will write attendance records when marking someone as present or
        /// if it will just update the enrolled individual’s map.
        /// </summary>
        [DataMember]
        public bool EnableAttendance { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating if this sequence requires explicit enrollment. If not set, a person can be
        /// implicitly enrolled through attendance.
        /// </summary>
        [DataMember]
        public bool RequiresEnrollment { get; private set; }

        /// <summary>
        /// Gets or sets the timespan that each map bit represents (<see cref="Rock.Model.SequenceOccurrenceFrequency"/>).
        /// </summary>
        [DataMember]
        public SequenceOccurrenceFrequency OccurrenceFrequency { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> associated with the first bit of this sequence.
        /// </summary>
        [DataMember]
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// The sequence of bits that represent occurrences where attendance was possible. The least significant bit (right side) is
        /// representative of the StartDate. More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        public byte[] OccurrenceMap { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Sequence is active.
        /// </summary>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets the Sequence Occurrence Exclusions
        /// </summary>
        /// <value>
        /// The exclusion values.
        /// </value>
        public List<SequenceOccurrenceExclusionCache> SequenceOccurrenceExclusions
        {
            get
            {
                var sequenceOccurrenceExclusions = new List<SequenceOccurrenceExclusionCache>();

                if ( _sequenceOccurrenceExclusionIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _sequenceOccurrenceExclusionIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _sequenceOccurrenceExclusionIds = new SequenceOccurrenceExclusionService( rockContext )
                                    .GetBySequenceId( Id )
                                    .AsNoTracking()
                                    .Select( soe => soe.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                foreach ( var id in _sequenceOccurrenceExclusionIds )
                {
                    var sequenceOccurrenceExclusion = SequenceOccurrenceExclusionCache.Get( id );
                    if ( sequenceOccurrenceExclusion != null )
                    {
                        sequenceOccurrenceExclusions.Add( sequenceOccurrenceExclusion );
                    }
                }

                return sequenceOccurrenceExclusions;
            }
        }
        private List<int> _sequenceOccurrenceExclusionIds = null;

        /// <summary>
        /// Reloads the exclusion values.
        /// </summary>
        public void ReloadOccurrenceExclusions()
        {
            // set _sequenceOccurrenceExclusionIds to null so it load them all at once on demand
            _sequenceOccurrenceExclusionIds = null;
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

            var sequence = entity as Sequence;
            if ( sequence == null )
            {
                return;
            }

            Name = sequence.Name;
            Description = sequence.Description;
            IsActive = sequence.IsActive;
            StructureType = sequence.StructureType;
            StructureEntityId = sequence.StructureEntityId;
            EnableAttendance = sequence.EnableAttendance;
            RequiresEnrollment = sequence.RequiresEnrollment;
            OccurrenceFrequency = sequence.OccurrenceFrequency;
            StartDate = sequence.StartDate;
            OccurrenceMap = sequence.OccurrenceMap;

            ReloadOccurrenceExclusions();
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