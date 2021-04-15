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

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a named <see cref="Rock.Model.Schedule"/> that is required by the rendering engine.
    /// This information will be cached by the engine.
    /// </summary>
    [Serializable]
    [DataContract]
    public class NamedScheduleCache : ModelCache<NamedScheduleCache, Rock.Model.Schedule>
    {
        #region Properties

        /// <inheritdoc cref="Rock.Model.Schedule.Name" />
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.CategoryId" />
        [DataMember]
        public int? CategoryId { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.FriendlyScheduleText" />
        [DataMember]
        public string FriendlyScheduleText { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.StartTimeOfDay" />
        [DataMember]
        public TimeSpan StartTimeOfDay { get; private set; }

        /// <inheritdoc cref="Rock.Model.Schedule.Category" />
        public CategoryCache Category => this.CategoryId.HasValue ? CategoryCache.Get( CategoryId.Value ) : null;

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// The amount of time that this item will live in the cache before expiring. If null, then the
        /// default lifespan is used.
        /// </summary>
        public override TimeSpan? Lifespan
        {
            get
            {
                if ( Name.IsNullOrWhiteSpace() )
                {
                    // just in case this isn't a named Schedule, expire after 10 minutes
                    return new TimeSpan( 0, 10, 0 );
                }

                return base.Lifespan;
            }
        }
        
        private string iCalendarContent { get; set; }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            Rock.Model.Schedule schedule = entity as Rock.Model.Schedule;
            if ( schedule == null )
            {
                return;
            }

            this.Name = schedule.Name;
            this.CategoryId = schedule.CategoryId;
            this.IsActive = schedule.IsActive;
            this.FriendlyScheduleText = schedule.ToFriendlyScheduleText();
            this.iCalendarContent = schedule.iCalendarContent;
            this.StartTimeOfDay = schedule.StartTimeOfDay;
        }

        /// <summary>
        /// returns <see cref="FriendlyScheduleText"/>
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.FriendlyScheduleText;
        }

        /// <inheritdoc cref="Rock.Model.Schedule.GetICalOccurrences(DateTime, DateTime?, DateTime?)" />
        public IList<Ical.Net.DataTypes.Occurrence> GetICalOccurrences( DateTime beginDateTime, DateTime? endDateTime, DateTime? scheduleStartDateTimeOverride )
        {
            return InetCalendarHelper.GetOccurrences( iCalendarContent, beginDateTime, endDateTime, scheduleStartDateTimeOverride );
        }

        #endregion Public Methods
    }
}
