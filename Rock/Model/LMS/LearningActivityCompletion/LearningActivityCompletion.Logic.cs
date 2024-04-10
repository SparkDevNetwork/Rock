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


using System.Collections.Generic;
using System.Linq;

namespace Rock.Model
{
    public partial class LearningActivityCompletion
    {
        /// <summary>
        /// Gets the grade for the activity.
        /// </summary>
        /// <param name="scales">
        ///     The list of <see cref="LearningGradingSystemScale">Scales</see> for the Activity.
        ///     Assumes the scales are ordered by ThresholdPercentage descending so the first match can be taken.
        /// </param>
        /// <returns>A string representing the text for the percentage and earned grade.</returns>
        public string Grade(IEnumerable<LearningGradingSystemScale> scales)
        {
            var percent = PointsEarned / LearningActivity.Points;
            var grade = scales.FirstOrDefault( s => s.ThresholdPercentage >= percent );

            return grade?.Name.Length > 0 ? $"{grade?.Name} ({percent}%)" : $"{percent}%";
        }
    }
}
