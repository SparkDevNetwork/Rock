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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Rock.Data;
using Rock.Enums.Lms;

namespace Rock.Model
{
    public partial class LearningActivityCompletion
    {
        /// <summary>
        /// Gets the grade as a percentage for the student <see cref="LearningActivityCompletion">Activity</see>.
        /// If no points are configured or <see cref="LearningActivity" /> is null then 100 is returned.
        /// </summary>
        [NotMapped]
        public decimal? GradePercent
        {
            get
            {
                if ( !PointsEarned.HasValue )
                {
                    return null;
                }

                if ( LearningActivity == null || LearningActivity.Points <= 0 )
                {
                    return 100;
                }

                return Math.Round( PointsEarned.Value * 100.0m / LearningActivity.Points, 3 );
            }
        }

        /// <summary>
        /// Gets the grade text for the activity.
        /// </summary>
        /// <param name="scales">
        ///     The list of <see cref="LearningGradingSystemScale">Scales</see> for the Activity.
        ///     Assumes the scales are ordered by ThresholdPercentage descending so the first match can be taken.
        /// </param>
        /// <param name="decimalPlaces">The number of decimal places to round to.</param>
        /// <returns>A string representing the text for the percentage and earned grade.</returns>
        public string GetGradeText( IEnumerable<LearningGradingSystemScale> scales = null, int decimalPlaces = 0 )
        {
            if ( !IsStudentCompleted && !IsFacilitatorCompleted )
            {
                // If incomplete return an empty string.
                return string.Empty;
            }

            var grade = GetGrade( scales );

            if ( !GradePercent.HasValue )
            {
                return grade?.Name.Length > 0
                    ? grade.Name
                    : string.Empty;
            }

            var percent = Math.Round( GradePercent.Value, decimalPlaces );
            var percentString = decimalPlaces == 0
                ? percent.ToIntSafe().ToString()
                : percent.ToString();

            return grade?.Name.Length > 0 ? $"{grade?.Name} ({percentString}%)" : $"{percentString}%";
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
        public LearningGradingSystemScale GetGrade( IEnumerable<LearningGradingSystemScale> scales = null )
        {
            if ( scales == null )
            {
                scales = Student?.LearningClass?.LearningGradingSystem?.LearningGradingSystemScales.OrderByDescending( s => s.ThresholdPercentage );
            }

            if ( scales == null )
            {
                return null;
            }

            var gradePercent = GradePercent;
            return scales.FirstOrDefault( s => gradePercent >= s.ThresholdPercentage );
        }

        /// <summary>
        /// Determines if the individual was given an extension on the activity <see cref="DueDate"/>.
        /// </summary>
        [NotAudited]
        public bool HadExtension => LearningActivity?.DueDateCalculated != null && DueDate.HasValue && DueDate.Value.Date != LearningActivity.DueDateCalculated.Value.Date;

        /// <summary>
        /// Determine if the activity has a student comment.
        /// </summary>
        [NotAudited]
        public bool HasStudentComment
        {
            get
            {
                return StudentComment.IsNotNullOrWhiteSpace();
            }
        }

        /// <summary>
        /// Determine if the activity has a facilitator comment.
        /// </summary>
        [NotAudited]
        public bool HasFacilitatorComment
        {
            get
            {
                return FacilitatorComment.IsNotNullOrWhiteSpace();
            }
        }

        /// <summary>
        /// Determines if the activity was completed late or is currently incomplete and late.
        /// </summary>
        [NotAudited]
        public bool IsLate
        {
            get
            {
                // If this is not student assigned or has no due date it can't be late.
                if ( LearningActivity.AssignTo != AssignTo.Student || !DueDate.HasValue )
                {
                    return false;
                }

                // If the student completed it, but was late.
                if ( IsStudentCompleted && !WasCompletedOnTime )
                {
                    return true;
                }

                // If the student hasn't completed it yet and the due date is in the past.
                if ( !IsStudentCompleted )
                {
                    // We don't allow setting the time portion
                    // so compare as a Date (excluding time).
                    return DueDate.Value.Date < RockDateTime.Today.Date;
                }

                return false;
            }
        }

        /// <summary>
        /// The activity has points, is assigned to the facilitator and hasn't been completed.
        /// </summary>
        [NotAudited]
        public bool RequiresFacilitatorCompletion
        {
            get
            {
                return LearningActivity.Points > 0
                    && LearningActivity.AssignTo == AssignTo.Facilitator
                    && !IsFacilitatorCompleted;
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
                if ( this.LearningActivity?.Id > 0 )
                {
                    return this.LearningActivity;
                }
                else
                {
                    return this.LearningActivityId > 0 ?
                        new LearningActivityService( new Data.RockContext() ).Get( this.LearningActivityId ) :
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
    }
}
