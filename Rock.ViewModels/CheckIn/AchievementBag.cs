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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Defines the details of an achievement for the check-in process.
    /// </summary>
    public class AchievementBag
    {
        /// <summary>
        /// Gets or sets the identifier of the achievement attempt.
        /// </summary>
        /// <value>The identifier of the achievement attempt.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the achievement type.
        /// </summary>
        /// <value>The identifier of the achievement type.</value>
        public string AchievementTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the achievement type.
        /// </summary>
        /// <value>The name of the achievement type.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the progress with <c>1.0</c> being fully achieved.
        /// This value may exceep <c>1.0</c> if over-achievement is allowed.
        /// </summary>
        /// <value>The progress.</value>
        public decimal Progress { get; set; }

        /// <summary>
        /// Gets or sets the target count of things that must be done for this
        /// achievement to be considered accomplished.
        /// </summary>
        /// <value>
        /// The number of things that must be accomplished to complete this achievement or <c>null</c> if not known.
        /// </value>
        public int? TargetCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this achievement was successful.
        /// </summary>
        /// <value><c>true</c> if this achievement was successful; otherwise, <c>false</c>.</value>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this achievement is closed.
        /// A closed achievement will not be updated further.
        /// </summary>
        /// <value><c>true</c> if this achievement is closed; otherwise, <c>false</c>.</value>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>The start date time.</value>
        public DateTimeOffset StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>The end date time.</value>
        public DateTimeOffset? EndDateTime { get; set; }
    }
}
