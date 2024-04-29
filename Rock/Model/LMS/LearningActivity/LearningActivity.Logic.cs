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

using Rock.Enums.Lms;

namespace Rock.Model
{
    public partial class LearningActivity
    {
        /// <summary>
        /// A description of <see cref="Rock.Model.LearningActivity.AvailableDateCalculated"/>.
        /// </summary>
        public string AvailableDateDescription
        {
            get
            {
                switch ( AvailableDateCalculationMethod )
                {
                    case AvailableDateCalculationMethod.Specific:
                        if ( AvailableDateDefault.HasValue )
                        {
                            return AvailableDateDefault.Value.ToShortDateString();
                        }
                        break;
                    case AvailableDateCalculationMethod.ClassStartOffset:
                        if ( !LearningClass.LearningSemester.StartDate.HasValue )
                        {
                            return $"+{AvailableDateOffset ?? 0} {"day".PluralizeIf( AvailableDateOffset != 1 )} after class start.";
                        }
                        break;
                    case AvailableDateCalculationMethod.EnrollmentOffset:
                        if ( LearningClass.CreatedDateTime.HasValue )
                        {
                            return $"+{AvailableDateOffset ?? 0} {"day".PluralizeIf( AvailableDateOffset != 1 )} after class enrollment.";
                        }
                        break;
                    case AvailableDateCalculationMethod.AfterPreviousCompleted:
                        return "After Previous";
                    case AvailableDateCalculationMethod.AlwaysAvailable:
                        return "Open";
                }

                return null;
            }
        }

        /// <summary>
        /// A description of the Due Date.
        /// </summary>
        public string DueDateDescription
        {
            get
            {
                switch ( DueDateCalculationMethod )
                {
                    case DueDateCalculationMethod.Specific:
                        if ( DueDateDefault.HasValue )
                        {
                            return DueDateDefault.Value.ToShortDateString();
                        }
                        break;
                    case DueDateCalculationMethod.ClassStartOffset:
                        if ( LearningClass.LearningSemester.StartDate.HasValue )
                        {
                            return $"+{DueDateOffset ?? 0} {"day".PluralizeIf( DueDateOffset != 1 )} after class start.";
                        }
                        break;
                    case DueDateCalculationMethod.EnrollmentOffset:
                        if ( LearningClass.CreatedDateTime.HasValue )
                        {
                            return $"+{DueDateOffset ?? 0} {"day".PluralizeIf( DueDateOffset != 1 )} after class enrollment.";
                        }
                        break;
                    case DueDateCalculationMethod.NoDate:
                        return "Optional";
                }

                return null;
            }
        }

        /// <summary>
        /// The result of the calculated available date.
        /// </summary>
        /// <remarks>
        /// The AfterPreviousCompleted calculation method will return null since we cannot determine an exact date
        /// When an AVailableDateOffset is required, but is null zero will be used for the calculation.
        /// for a Course.
        /// </remarks>
        public DateTime? AvailableDateCalculated
        {
            get
            {
                switch ( AvailableDateCalculationMethod )
                {
                    case AvailableDateCalculationMethod.Specific:
                        if ( AvailableDateDefault.HasValue )
                        {
                            return AvailableDateDefault.Value;
                        }
                        break;
                    case AvailableDateCalculationMethod.ClassStartOffset:
                        if ( LearningClass.LearningSemester.StartDate.HasValue )
                        {
                            return LearningClass.LearningSemester.StartDate.Value.AddDays( AvailableDateOffset ?? 0 );
                        }
                        break;
                    case AvailableDateCalculationMethod.EnrollmentOffset:
                        if ( LearningClass.CreatedDateTime.HasValue )
                        {
                            return LearningClass.CreatedDateTime.Value.AddDays( AvailableDateOffset ?? 0 );
                        }
                        break;
                    case AvailableDateCalculationMethod.AlwaysAvailable:
                        return DateTime.MinValue;
                }

                return null;
            }
        }

        /// <summary>
        /// The result of the calculated due date.
        /// </summary>
        /// <remarks>
        /// The NoDate calculation method will return null indicating there is no due date.
        /// When a DueDateOffset is required, but is null zero will be used for the calculation
        /// </remarks>
        public DateTime? DueDateCalculated
        {
            get
            {
                switch ( DueDateCalculationMethod )
                {
                    case DueDateCalculationMethod.Specific:
                        if ( DueDateDefault.HasValue )
                        {
                            return DueDateDefault.Value;
                        }
                    break;
                    case DueDateCalculationMethod.ClassStartOffset:
                        if ( LearningClass.LearningSemester.StartDate.HasValue )
                        {
                            return LearningClass.LearningSemester.StartDate.Value.AddDays( DueDateOffset ?? 0 );
                        }
                        break;
                    case DueDateCalculationMethod.EnrollmentOffset:
                        if ( LearningClass.CreatedDateTime.HasValue )
                        {
                            return LearningClass.CreatedDateTime.Value.AddDays( DueDateOffset ?? 0 );
                        }
                        break;
                }

                return null;
            }
        }

        /// <summary>
        /// A textual description of the available and due dates for the activity.
        /// </summary>
        public string DatesDescription
        {
            get
            {
                return $"{AvailableDateDescription} - {DueDateDescription}";
            }
        }

        /// <summary>
        /// <c>true</c> if the calculated due date is in the past; otherwise <c>false</c>.
        /// </summary>
        public bool IsPastDue
        {
            get
            {
                return DueDateCalculated.HasValue && DueDateCalculated.Value.IsPast();
            }
        }
    }

    /// <summary>
    /// POCO for encapsulating <see cref="LearningActivity"/> completions statistics for a given <see cref="LearningClass"/>.
    /// </summary>
    public class LearningActivityCompletionStatistics
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
