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

using Rock.Data;
using Rock.Model;

namespace Rock.Achievement
{
    /// <summary>
    /// Achiever Attempt Item
    /// </summary>
    public sealed class AchieverAttemptItem
    {
        /// <summary>
        /// Gets or sets the achievement attempt.
        /// </summary>
        public AchievementAttempt AchievementAttempt { get; set; }

        /// <summary>
        /// Gets or sets the achiever.
        /// </summary>
        public IEntity Achiever { get; set; }

        /// <summary>
        /// Gets or sets the name of the achiever.
        /// </summary>
        public string AchieverName { get; set; }
    }
}
