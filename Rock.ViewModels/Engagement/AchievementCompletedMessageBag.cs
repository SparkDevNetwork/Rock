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

namespace Rock.ViewModels.Engagement
{
    /// <summary>
    /// Details about an achievement completion that is transmitted over
    /// the RealTime engine.
    /// </summary>
    public class AchievementCompletedMessageBag
    {
        /// <summary>
        /// Gets or sets the achievement attempt unique identifier.
        /// </summary>
        /// <value>The achievement attempt unique identifier.</value>
        public Guid AchievementAttemptGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the achievement type that
        /// was completed.
        /// </summary>
        /// <value>The unique identifier of the achievement type that was completed.</value>
        public Guid AchievementTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the achievement type.
        /// </summary>
        /// <value>The name of the achievement type.</value>
        public string AchievementTypeName { get; set; }

        /// <summary>
        /// Gets or sets the achievement type image URL.
        /// </summary>
        /// <value>The achievement type image URL.</value>
        public string AchievementTypeImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the achievement type alternate image URL.
        /// </summary>
        /// <value>The achievement type alternate image URL.</value>
        public string AchievementTypeAlternateImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the entity that completed
        /// the achievement.
        /// </summary>
        /// <value>The unique identifier of the entity that completed the achievement.</value>
        public Guid EntityGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        /// <value>The name of the entity.</value>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the photo URL of the entity. This is null for most
        /// entities.
        /// </summary>
        /// <value>The photo URL of the entity.</value>
        public string EntityPhotoUrl { get; set; }
    }
}
