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
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    public partial class LearningGradingSystemScaleService
    {
        /// <summary>
        /// Gets all scales used by the specified grading system.
        /// </summary>
        /// <param name="learningGradingSystemId">The identifier of the <see cref="LearningGradingSystem"/>.</param>
        /// <returns>An IQueryable of LearningGradingSystemScale.</returns>
        public IQueryable<LearningGradingSystemScale> GetSystemScales( int learningGradingSystemId )
        {
            return Queryable().Where( s => s.LearningGradingSystemId == learningGradingSystemId );
        }

        /// <summary>
        /// Gets the scale with the highest threshold that the specified gradePercent does not exceed.
        /// </summary>
        /// <param name="learningGradingSystemId">The identifier of the <see cref="LearningGradingSystem"/>.</param>
        /// <param name="gradePercent">
        /// The grade percent to compare to the scale's threshold.
        /// This value should match the format of the LearningGradingSystemScale.ThresholdPercentage (a decimal number between 0 and 100).
        /// </param>
        /// <returns>A LearningGradingSystemScale.</returns>
        public LearningGradingSystemScale GetEarnedScale( int learningGradingSystemId, decimal gradePercent )
        {
            return Queryable()
                .AsNoTracking()
                .OrderByDescending( s => s.ThresholdPercentage )
                .FirstOrDefault( s =>
                    s.LearningGradingSystemId == learningGradingSystemId
                    && s.ThresholdPercentage >= gradePercent
                );
        }
    }
}