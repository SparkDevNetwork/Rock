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
        /// Attempts to calculate the available date or provides a textual description if unable to calculate.
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
                        if ( LearningClass?.LearningSemester?.StartDate.HasValue == true )
                        {
                            return LearningClass.LearningSemester.StartDate.Value.AddDays( AvailableDateOffset.Value ).ToShortDateString();
                        }
                        else
                        {
                            return $"{DateOffsetText( AvailableDateOffset )} class start.";
                        }
                    case AvailableDateCalculationMethod.EnrollmentOffset:
                        if ( LearningClass?.CreatedDateTime.HasValue == true )
                        {
                            return LearningClass.CreatedDateTime.Value.AddDays( AvailableDateOffset.Value ).ToShortDateString();
                        }
                        else
                        {
                            return $"{DateOffsetText( AvailableDateOffset )} class enrollment.";
                        }
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
                        if ( LearningClass?.LearningSemester?.StartDate.HasValue == true )
                        {
                            return LearningClass.LearningSemester.StartDate.Value.AddDays( DueDateOffset.Value ).ToShortDateString();
                        }
                        else
                        {
                            return $"{DateOffsetText( DueDateOffset )} class start.";
                        }
                    case DueDateCalculationMethod.EnrollmentOffset:
                        if ( LearningClass.CreatedDateTime.HasValue )
                        {
                            return LearningClass.CreatedDateTime.Value.AddDays( DueDateOffset.Value ).ToShortDateString();
                        }
                        else
                        {
                            return $"{DateOffsetText( DueDateOffset )} class enrollment.";
                        }
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
        public DateTime? AvailableDateCalculated => CalculateAvailableDate(
                AvailableDateCalculationMethod,
                AvailableDateDefault,
                AvailableDateOffset,
                LearningClass?.LearningSemester?.StartDate,
                LearningClass?.CreatedDateTime
            );

        /// <summary>
        /// Calculates the available date based on the provided parameters.
        /// </summary>
        /// <param name="method">The <see cref="AvailableDateCalculationMethod"/> to be used.</param>
        /// <param name="defaultDate">The default/initial date value to be used for calculations which use an offset.</param>
        /// <param name="offset">The number of days to offset for calculations which use an offset.</param>
        /// <param name="semesterStart">The start date of the semester to be used for class start offset calculations.</param>
        /// <param name="enrollmentDate">The date the student enrolled in the class to be used for enrollment offset calculations.</param>
        /// <remarks>
        /// The AfterPreviousCompleted calculation method will return null since we cannot determine an exact date
        /// When an AvailableDateOffset is required, but is null zero will be used for the calculation.
        /// </remarks>
        public static DateTime? CalculateAvailableDate( AvailableDateCalculationMethod method, DateTime? defaultDate, int? offset, DateTime? semesterStart, DateTime? enrollmentDate )
        {
            switch ( method )
            {
                case AvailableDateCalculationMethod.Specific:
                    if ( defaultDate.HasValue )
                    {
                        return defaultDate.Value;
                    }
                    break;
                case AvailableDateCalculationMethod.ClassStartOffset:
                    if ( semesterStart.HasValue )
                    {
                        return semesterStart.Value.AddDays( offset ?? 0 );
                    }
                    break;
                case AvailableDateCalculationMethod.EnrollmentOffset:
                    if ( enrollmentDate.HasValue )
                    {
                        return enrollmentDate.Value.AddDays( offset ?? 0 );
                    }
                    break;
                case AvailableDateCalculationMethod.AlwaysAvailable:
                    return null;
            }

            return null;
        }

        /// <summary>
        /// Calculates the due date based on the provided parameters.
        /// </summary>
        /// <param name="method">The <see cref="DueDateCalculationMethod"/> to be used.</param>
        /// <param name="defaultDate">The default/initial date value to be used for calculations which use an offset.</param>
        /// <param name="offset">The number of days to offset for calculations which use an offset.</param>
        /// <param name="semesterStart">The start date of the semester to be used for class start offset calculations.</param>
        /// <param name="enrollmentDate">The date the student enrolled in the class to be used for enrollment offset calculations.</param>
        /// <remarks>
        /// The NoDate calculation method will return null indicating there is no due date.
        /// When a DueDateOffset is required, but is null - zero will be used for the calculation
        /// </remarks>
        public static DateTime? CalculateDueDate( DueDateCalculationMethod method, DateTime? defaultDate, int? offset, DateTime? semesterStart, DateTime? enrollmentDate )
        {
            switch ( method )
            {
                case DueDateCalculationMethod.Specific:
                    if ( defaultDate.HasValue )
                    {
                        return defaultDate.Value;
                    }
                    break;
                case DueDateCalculationMethod.ClassStartOffset:
                    if ( semesterStart.HasValue )
                    {
                        return semesterStart.Value.AddDays( offset ?? 0 );
                    }
                    break;
                case DueDateCalculationMethod.EnrollmentOffset:
                    if ( enrollmentDate.HasValue )
                    {
                        return enrollmentDate.Value.AddDays( offset ?? 0 );
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// The result of the calculated due date.
        /// </summary>
        /// <remarks>
        /// The NoDate calculation method will return null indicating there is no due date.
        /// When a DueDateOffset is required, but is null - zero will be used for the calculation
        /// </remarks>
        public DateTime? DueDateCalculated =>
                CalculateDueDate(
                    DueDateCalculationMethod,
                    DueDateDefault,
                    DueDateOffset,
                    LearningClass?.LearningSemester?.StartDate,
                    LearningClass?.CreatedDateTime
                );

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

        #region Private methods

        /// <summary>
        /// Gets the starting string for a textual representation of a calculated date (due or available).
        /// </summary>
        /// <param name="dateOffset">The Available or Due Date offset to use in the text.</param>
        /// <returns>The human readable string to use for the specified offset.</returns>
        private string DateOffsetText( int? dateOffset )
        {
            var unsignedOffset = Math.Abs( dateOffset ?? 0 );

            if ( unsignedOffset == 0 )
            {
                return "At "; // e.g. 'At Class Start' or 'At class enrollment'.
            }

            // If the sign of the offset is negative use 'before' otherwise 'after'. Zero is already accounted for.
            var beforeOrAfter = dateOffset < 0 ? "before" : "after";
            if ( unsignedOffset == 1 )
            {
                return $"1 day {beforeOrAfter} ";
            }

            if ( unsignedOffset > 1 )
            {
                return $"{unsignedOffset} days {beforeOrAfter} ";
            }

            return string.Empty;
        }

        #endregion
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
