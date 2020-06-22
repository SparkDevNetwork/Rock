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
    /// Cache object for <see cref="StreakTypeAchievementTypePrerequisite" />
    /// </summary>
    [Serializable]
    [DataContract]
    public class StreakTypeAchievementTypePrerequisiteCache : ModelCache<StreakTypeAchievementTypePrerequisiteCache, StreakTypeAchievementTypePrerequisite>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Model.StreakTypeAchievementType"/>.
        /// </summary>
        [DataMember]
        public int StreakTypeAchievementTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the prerequisite <see cref="StreakTypeAchievementType"/>
        /// </summary>
        [DataMember]
        public int PrerequisiteStreakTypeAchievementTypeId { get; private set; }

        #endregion Entity Properties

        #region Related Cache Objects

        /// <summary>
        /// Gets the Achievement Type Cache.
        /// </summary>
        public StreakTypeAchievementTypeCache StreakTypeAchievementType
            => StreakTypeAchievementTypeCache.Get( StreakTypeAchievementTypeId );

        /// <summary>
        /// Gets the Prerequisite Achievement Type Cache.
        /// </summary>
        public StreakTypeAchievementTypeCache PrerequisiteStreakTypeAchievementType
             => StreakTypeAchievementTypeCache.Get( PrerequisiteStreakTypeAchievementTypeId );

        #endregion Related Cache Objects

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity"></param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var prerequisite = entity as StreakTypeAchievementTypePrerequisite;

            if ( prerequisite == null )
            {
                return;
            }

            PrerequisiteStreakTypeAchievementTypeId = prerequisite.PrerequisiteStreakTypeAchievementTypeId;
            StreakTypeAchievementTypeId = prerequisite.StreakTypeAchievementTypeId;
        }

        #endregion Public Methods
    }
}
