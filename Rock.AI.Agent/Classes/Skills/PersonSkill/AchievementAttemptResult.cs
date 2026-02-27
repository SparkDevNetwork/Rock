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

using System;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.PersonSkill
{
    /// <summary>
    /// Represents a single achievement attempt record.
    /// </summary>
    internal class AchievementAttemptResult : EntityResultBase
    {
        /// <summary>
        /// The type of achievement that was attempted.
        /// </summary>
        public KeyNameResult AchievementType { get; set; }

        /// <summary>
        /// The date and time the attempt was started.
        /// </summary>
        public DateTime? AttemptStartDateTime { get; set; }

        /// <summary>
        /// The date and time the attempt was last updated.
        /// </summary>
        public DateTime? AttemptEndDateTime { get; set; }

        /// <summary>
        /// The progress on a scale of 0 - 100.
        /// </summary>
        public double? Progress { get; set; }

        /// <summary>
        /// If closed, the achievement attempt will not be updated again.
        /// </summary>
        public bool? IsClosed { get; set; }

        /// <summary>
        /// Determines if the achievement attempt was successful.
        /// </summary>
        public bool? IsSuccessful { get; set; }
    }
}
