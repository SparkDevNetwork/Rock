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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.AchievementAttemptDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class AchievementAttemptBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the achievement attempt end date time.
        /// </summary>
        public DateTimeOffset? AchievementAttemptEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the achievement attempt start date time.
        /// </summary>
        public DateTimeOffset AchievementAttemptStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.AchievementType of this attempt.
        /// </summary>
        public ListItemBag AchievementType { get; set; }

        /// <summary>
        /// Gets or sets the achiever entity identifier. The type of AchieverEntity is determined by Rock.Model.AchievementType.AchieverEntityTypeId.
        /// NOTE: In the case of a Person achievement, this could either by PersonAliasId or PersonId (but probably PersonAliasId)
        /// depending on Rock.Model.AchievementType.AchievementEntityType
        /// </summary>
        public string AchieverEntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attempt is closed.
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attempt was a success.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the progress. This is a percentage so .25 is 25% and 1 is 100%.
        /// </summary>
        public string Progress { get; set; }

        /// <summary>
        /// Gets or sets the achiever HTML.
        /// </summary>
        /// <value>
        /// The achiever HTML.
        /// </value>
        public string AchieverHtml { get; set; }

        /// <summary>
        /// Gets or sets the progress HTML.
        /// </summary>
        /// <value>
        /// The progress HTML.
        /// </value>
        public string ProgressHtml { get; set; }

        /// <summary>
        /// Gets or sets the attempt description.
        /// </summary>
        /// <value>
        /// The attempt description.
        /// </value>
        public string AttemptDescription { get; set; }

        /// <summary>
        /// Gets or sets the achievement page URL.
        /// </summary>
        /// <value>
        /// The achievement page URL.
        /// </value>
        public string AchievementPageUrl { get; set; }
    }
}
