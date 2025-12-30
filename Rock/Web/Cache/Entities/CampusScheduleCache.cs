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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Represents a schedule that is tied to a campus for a specific purpose.
    /// </summary>
    [Serializable]
    [DataContract]
    public class CampusScheduleCache : ModelCache<CampusScheduleCache, CampusSchedule>
    {
        #region Properties

        /// <inheritdoc cref="CampusSchedule.CampusId"/>
        [DataMember]
        public int CampusId { get; private set; }

        /// <inheritdoc cref="CampusSchedule.ScheduleId"/>
        [DataMember]
        public int ScheduleId { get; private set; }

        /// <inheritdoc cref="CampusSchedule.ScheduleTypeValueId"/>
        [DataMember]
        public int? ScheduleTypeValueId { get; private set; }

        /// <inheritdoc cref="CampusSchedule.Order"/>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets a <see cref="CampusCache"/> that represents the campus
        /// associated with this instance.
        /// </summary>
        [DataMember]
        public CampusCache Campus => CampusCache.Get( CampusId );

        /// <summary>
        /// Gets a <see cref="NamedScheduleCache"/> that represents the schedule
        /// associated with this instance.
        /// </summary>
        [DataMember]
        public NamedScheduleCache Schedule => NamedScheduleCache.Get( ScheduleId );

        /// <summary>
        /// Gets a <see cref="DefinedValueCache"/> that represents the schedule type
        /// of this instance.
        /// </summary>
        [DataMember]
        public DefinedValueCache ScheduleTypeValue => ScheduleTypeValueId.HasValue ? DefinedValueCache.Get( ScheduleTypeValueId.Value ) : null;

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is CampusSchedule campus ) )
            {
                return;
            }

            CampusId = campus.CampusId;
            ScheduleId = campus.ScheduleId;
            ScheduleTypeValueId = campus.ScheduleTypeValueId;
            Order = campus.Order;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Schedule.ToStringSafe()} at {Campus.ToStringSafe()}";
        }

        #endregion
    }
}