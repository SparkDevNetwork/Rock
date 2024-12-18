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
using System.ComponentModel.DataAnnotations.Schema;

using Rock.Data;
using Rock.Enums.Lms;

namespace Rock.Model
{
    public partial class LearningActivity
    {
        /// <summary>
        /// Attempts to calculate the available date or provides a textual description if unable to calculate.
        /// </summary>
        [NotAudited]
        public string AvailableDateDescription
        {
            get
            {
                switch ( AvailabilityCriteria )
                {
                    case AvailabilityCriteria.SpecificDate:
                        if ( AvailableDateDefault.HasValue )
                        {
                            return AvailableDateDefault.Value.ToShortDateString();
                        }
                        break;
                    case AvailabilityCriteria.ClassStartOffset:
                        if ( LearningClass?.LearningSemester?.StartDate.HasValue == true )
                        {
                            return LearningClass.LearningSemester.StartDate.Value.Date.AddDays( AvailableDateOffset.Value ).ToShortDateString();
                        }
                        else
                        {
                            return $"{DateOffsetText( AvailableDateOffset )} class start";
                        }
                    case AvailabilityCriteria.EnrollmentOffset:
                        var daysOffset = AvailableDateOffset.ToIntSafe();
                        if ( daysOffset == 0 )
                        {
                            return "At Enrollment";
                        }
                        else
                        {
                            var daysText = "Day".PluralizeIf( daysOffset != 1 );
                            return $"{daysOffset} {daysText} After Enrollment";
                        }
                    case AvailabilityCriteria.AfterPreviousCompleted:
                        return "After Previous";
                    case AvailabilityCriteria.AlwaysAvailable:
                        return "Open";
                }

                return null;
            }
        }

        /// <summary>
        /// A description of the Due Date.
        /// </summary>
        [NotAudited]
        public string DueDateDescription
        {
            get
            {
                switch ( DueDateCriteria )
                {
                    case DueDateCriteria.SpecificDate:
                        if ( DueDateDefault.HasValue )
                        {
                            return DueDateDefault.Value.ToShortDateString();
                        }
                        break;
                    case DueDateCriteria.ClassStartOffset:
                        if ( LearningClass?.LearningSemester?.StartDate.HasValue == true )
                        {
                            return LearningClass.LearningSemester.StartDate.Value.Date.AddDays( DueDateOffset.Value ).ToShortDateString();
                        }
                        else
                        {
                            return $"{DateOffsetText( DueDateOffset )} class start";
                        }
                    case DueDateCriteria.EnrollmentOffset:
                        var daysOffset = DueDateOffset.ToIntSafe();
                        if ( daysOffset == 0 )
                        {
                            return "At Enrollment";
                        }
                        else
                        {
                            var daysText = "Day".PluralizeIf( daysOffset != 1 );
                            return $"{daysOffset} {daysText} After Enrollment";
                        }
                    case DueDateCriteria.NoDate:
                        return string.Empty;
                }

                return null;
            }
        }

        /// <summary>
        /// The result of the calculated available date.
        /// </summary>
        /// <remarks>
        /// The AfterPreviousCompleted calculation criteria will return null since we cannot determine an exact date
        /// When an AVailableDateOffset is required, but is null zero will be used for the calculation.
        /// for a Course.
        /// </remarks>
        [NotAudited]
        public DateTime? AvailableDateCalculated => CalculateAvailableDate(
                AvailabilityCriteria,
                AvailableDateDefault,
                AvailableDateOffset,
                LearningClass?.LearningSemester?.StartDate,
                null
            );

        /// <summary>
        /// Calculates the available date based on the provided parameters.
        /// </summary>
        /// <param name="criteria">The <see cref="AvailabilityCriteria"/> to be used.</param>
        /// <param name="defaultDate">The default/initial date value to be used for calculations which use an offset.</param>
        /// <param name="offset">The number of days to offset for calculations which use an offset.</param>
        /// <param name="semesterStart">The start date of the semester to be used for class start offset calculations.</param>
        /// <param name="enrollmentDate">The date the student enrolled in the class to be used for enrollment offset calculations.</param>
        /// <remarks>
        /// The AfterPreviousCompleted calculation criteria will return null since we cannot determine an exact date
        /// When an AvailableDateOffset is required, but is null zero will be used for the calculation.
        /// </remarks>
        public static DateTime? CalculateAvailableDate( AvailabilityCriteria criteria, DateTime? defaultDate, int? offset, DateTime? semesterStart, DateTime? enrollmentDate )
        {
            switch ( criteria )
            {
                case AvailabilityCriteria.SpecificDate:
                    if ( defaultDate.HasValue )
                    {
                        return defaultDate.Value;
                    }
                    break;
                case AvailabilityCriteria.ClassStartOffset:
                    if ( semesterStart.HasValue )
                    {
                        return semesterStart.Value.Date.AddDays( offset ?? 0 );
                    }
                    break;
                case AvailabilityCriteria.EnrollmentOffset:
                    if ( enrollmentDate.HasValue )
                    {
                        return enrollmentDate.Value.Date.AddDays( offset ?? 0 );
                    }
                    break;
                case AvailabilityCriteria.AlwaysAvailable:
                    return null;
            }

            return null;
        }

        /// <summary>
        /// Calculates the due date based on the provided parameters.
        /// </summary>
        /// <param name="criteria">The <see cref="DueDateCriteria"/> to be used.</param>
        /// <param name="defaultDate">The default/initial date value to be used for calculations which use an offset.</param>
        /// <param name="offset">The number of days to offset for calculations which use an offset.</param>
        /// <param name="semesterStart">The start date of the semester to be used for class start offset calculations.</param>
        /// <param name="enrollmentDate">The date the student enrolled in the class to be used for enrollment offset calculations.</param>
        /// <remarks>
        /// The NoDate calculation criteria will return null indicating there is no due date.
        /// When a DueDateOffset is required, but is null - zero will be used for the calculation
        /// </remarks>
        public static DateTime? CalculateDueDate( DueDateCriteria criteria, DateTime? defaultDate, int? offset, DateTime? semesterStart, DateTime? enrollmentDate )
        {
            switch ( criteria )
            {
                case DueDateCriteria.SpecificDate:
                    if ( defaultDate.HasValue )
                    {
                        return defaultDate.Value;
                    }
                    break;
                case DueDateCriteria.ClassStartOffset:
                    if ( semesterStart.HasValue )
                    {
                        return semesterStart.Value.Date.AddDays( offset ?? 0 );
                    }
                    break;
                case DueDateCriteria.EnrollmentOffset:
                    if ( enrollmentDate.HasValue )
                    {
                        return enrollmentDate.Value.Date.AddDays( offset ?? 0 );
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// The result of the calculated due date.
        /// </summary>
        /// <remarks>
        /// The NoDate calculation criteria will return null indicating there is no due date.
        /// When a DueDateOffset is required, but is null - zero will be used for the calculation
        /// </remarks>
        [NotAudited]
        public DateTime? DueDateCalculated =>
                CalculateDueDate(
                    DueDateCriteria,
                    DueDateDefault,
                    DueDateOffset,
                    LearningClass?.LearningSemester?.StartDate,
                    null
                );

        /// <summary>
        /// A textual description of the available and due dates for the activity.
        /// </summary>
        [NotAudited]
        public string DatesDescription
        {
            get
            {
                if ( DueDateDescription.IsNotNullOrWhiteSpace() )
                {
                    return $"{AvailableDateDescription} - {DueDateDescription}";
                }
                else
                {
                    return AvailableDateDescription;
                }
            }
        }

        /// <summary>
        /// <c>true</c> if the calculated due date is in the past; otherwise <c>false</c>.
        /// </summary>
        /// <remarks>
        /// If the DueDate is today this will return <c>false</c> since we don't allow setting a time.
        /// </remarks>
        [NotAudited]
        public bool IsPastDue
        {
            get
            {
                // We don't allow setting the time portion
                // so compare as a Date (excluding time).
                return DueDateCalculated.HasValue
                    && DueDateCalculated.Value.Date < RockDateTime.Today.Date;
            }
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        [NotMapped]
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.LearningClass?.Id > 0 )
                {
                    return this.LearningClass;
                }
                else
                {
                    return this.LearningClassId > 0 ?
                        new LearningClassService( new Data.RockContext() ).Get( this.LearningClassId ) :
                        base.ParentAuthority;
                }
            }
        }

        /// <inheritdoc/>
        public override bool IsAuthorized( string action, Rock.Model.Person person )
        {
            // Defer to the parent authority.
            // We don't add any logic to the authorization process
            // that's not already included in that logic.
            return ParentAuthority.IsAuthorized( action, person );
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
