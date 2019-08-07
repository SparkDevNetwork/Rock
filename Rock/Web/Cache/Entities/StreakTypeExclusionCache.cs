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
    /// Information about a StreakTypeExclusion that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class StreakTypeExclusionCache : ModelCache<StreakTypeExclusionCache, StreakTypeExclusion>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="StreakType"/> to which this exclusion map belongs. This property is required.
        /// </summary>
        [DataMember]
        public int StreakTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the location identifier by which the streak type's exclusions will be associated.
        /// </summary>
        [DataMember]
        public int? LocationId { get; private set; }

        /// <summary>
        /// The sequence of bits that represent exclusions. The least significant bit is representative of the Streak Type's StartDate.
        /// More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        public byte[] ExclusionMap { get; private set; }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var exclusion = entity as StreakTypeExclusion;
            if ( exclusion == null )
            {
                return;
            }

            StreakTypeId = exclusion.StreakTypeId;
            LocationId = exclusion.LocationId;
            ExclusionMap = exclusion.ExclusionMap;
        }

        #endregion
    }
}