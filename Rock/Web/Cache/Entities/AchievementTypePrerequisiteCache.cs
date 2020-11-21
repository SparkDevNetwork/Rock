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
    /// Cache object for <see cref="AchievementTypePrerequisite" />
    /// </summary>
    [Serializable]
    [DataContract]
    public class AchievementTypePrerequisiteCache : ModelCache<AchievementTypePrerequisiteCache, AchievementTypePrerequisite>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Model.AchievementType"/>.
        /// </summary>
        [DataMember]
        public int AchievementTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the prerequisite <see cref="Model.AchievementType"/>
        /// </summary>
        [DataMember]
        public int PrerequisiteAchievementTypeId { get; private set; }

        #endregion Entity Properties

        #region Related Cache Objects

        /// <summary>
        /// Gets the Achievement Type Cache.
        /// </summary>
        public AchievementTypeCache AchievementType
            => AchievementTypeCache.Get( AchievementTypeId );

        /// <summary>
        /// Gets the Prerequisite Achievement Type Cache.
        /// </summary>
        public AchievementTypeCache PrerequisiteAchievementType
             => AchievementTypeCache.Get( PrerequisiteAchievementTypeId );

        #endregion Related Cache Objects

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity"></param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var prerequisite = entity as AchievementTypePrerequisite;

            if ( prerequisite == null )
            {
                return;
            }

            PrerequisiteAchievementTypeId = prerequisite.PrerequisiteAchievementTypeId;
            AchievementTypeId = prerequisite.AchievementTypeId;
        }

        #endregion Public Methods
    }
}
