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
using Rock.Model;

namespace Rock.Lms
{
    /// <summary>
    /// POCO for encapsulating <see cref="LearningClassActivity"/> completions statistics for a given <see cref="LearningClass"/>.
    /// </summary>
    public class LearningClassActivityCompletionStatistics
    {
        /// <summary>
        /// Gets or sets the number of students who have completed this activity for this class.
        /// </summary>
        public int Complete { get; set; }

        /// <summary>
        /// Gets or sets the number of students who have not completed this activity for this class.
        /// </summary>
        public int Incomplete { get; set; }

        /// <summary>
        /// Gets or sets the percentage of students who've completed this activity in this class.
        /// </summary>
        public double PercentComplete { get; set; }

        /// <summary>
        /// Gets or sets the average score for students who've completed this activity for this class.
        /// </summary>
        public double AverageGradePercent { get; set; }

        /// <summary>
        /// Gets or sets the average number of points for students who've completed this activity for this class.
        /// </summary>
        public int AveragePoints { get; set; }

        /// <summary>
        /// Gets or sets the average grade for students who've completed this activity for this class.
        /// </summary>
        public LearningGradingSystemScale AverageGrade { get; set; }
    }
}
