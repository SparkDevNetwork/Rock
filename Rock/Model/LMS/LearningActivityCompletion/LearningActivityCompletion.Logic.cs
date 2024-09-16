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

namespace Rock.Model
{
    public partial class LearningActivityCompletion
    {
        /// <summary>
        /// Gets the grade as a percentage for the student <see cref="LearningActivityCompletion">Activity</see>.
        /// </summary>
        public decimal GradePercent => LearningActivity?.Points > 0 ? Math.Round( ( decimal ) PointsEarned / ( decimal ) LearningActivity.Points * 100, 3 ) : 0;

        /// <summary>
        /// Gets the grade text for the activity.
        /// </summary>
        /// <param name="scales">
        ///     The list of <see cref="LearningGradingSystemScale">Scales</see> for the Activity.
        ///     Assumes the scales are ordered by ThresholdPercentage descending so the first match can be taken.
        /// </param>
        /// <returns>A string representing the text for the percentage and earned grade.</returns>
        public string GradeText( IEnumerable<LearningGradingSystemScale> scales = null )
        {
            var percent = GradePercent;
            var grade = Grade( scales );

            return grade?.Name.Length > 0 ? $"{grade?.Name} ({percent}%)" : $"{percent}%";
        }

        /// <summary>
        /// Gets the <see cref="LearningGradingSystemScale"/> for the student <see cref="LearningActivityCompletion">Activity</see>.
        /// </summary>
        /// <param name="scales">
        ///     The list of <see cref="LearningGradingSystemScale">Scales</see> for the Activity.
        ///     Assumes the scales are ordered by ThresholdPercentage descending so the first match can be taken.
        ///     If none are provided the navigation property is used to get them.
        /// </param>
        /// <returns>A string representing the text for the percentage and earned grade.</returns>
        public LearningGradingSystemScale Grade( IEnumerable<LearningGradingSystemScale> scales = null )
        {
            if ( scales == null )
            {
                scales = Student?.LearningClass?.LearningGradingSystem?.LearningGradingSystemScales.OrderByDescending( s => s.ThresholdPercentage );
            }

            var gradePercent = GradePercent;
            return scales.FirstOrDefault( s => gradePercent >= s.ThresholdPercentage );
        }
    }
}
