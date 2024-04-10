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
                        if ( !AvailableDateDefault.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "AvailableDateDefault",
                                "AvailableDateCalculationMethod" );
                        }

                        return AvailableDateDefault.Value.ToShortDateString();
                    case AvailableDateCalculationMethod.ClassStartOffset:
                        if ( !LearningClass.LearningSemester.StartDate.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "LearningClass.LearningSemester.StartDate",
                                "AvailableDateCalculationMethod" );
                        }

                        return $"{AvailableDateOffset ?? 0} days after class start.";
                    case AvailableDateCalculationMethod.EnrollmentOffset:
                        if ( !LearningClass.CreatedDateTime.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "LearningClass.CreatedDateTime",
                                "AvailableDateCalculationMethod" );
                        }

                        return $"{AvailableDateOffset ?? 0} days after class enrollment.";
                    case AvailableDateCalculationMethod.AfterPreviousCompleted:
                        return "After Previous";
                    case AvailableDateCalculationMethod.AlwaysAvailable:
                        return "Open";
                    default:
                        throw new InvalidCalculatedDateException(
                            $"Invalid or Unknown AvailableDateCalculationMethod value: {AvailableDateCalculationMethod}" );
                }
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
                        if ( !DueDateDefault.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "DueDateDefault",
                                "DueDateCalculationMethod" );
                        }

                        return DueDateDefault.Value.ToShortDateString();
                    case DueDateCalculationMethod.ClassStartOffset:
                        if ( !LearningClass.LearningSemester.StartDate.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "LearningClass.LearningSemester.StartDate",
                                "DueDateCalculationMethod" );
                        }

                        return $"{DueDateOffset ?? 0} days after class start.";
                    case DueDateCalculationMethod.EnrollmentOffset:
                        if ( !LearningClass.CreatedDateTime.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "LearningClass.CreatedDateTime",
                                "DueDateCalculationMethod" );
                        }

                        return $"{DueDateOffset ?? 0} days after class enrollment.";
                    case DueDateCalculationMethod.NoDate:
                        return "Optional";
                    default:
                        throw new InvalidCalculatedDateException(
                            $"Invalid or Unknown DueDateCalculationMethod value: {DueDateCalculationMethod}" );
                }
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
                        if ( !AvailableDateDefault.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "AvailableDateDefault",
                                "AvailableDateCalculationMethod" );
                        }

                        return AvailableDateDefault.Value;
                    case AvailableDateCalculationMethod.ClassStartOffset:
                        if ( !LearningClass.LearningSemester.StartDate.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "LearningClass.LearningSemester.StartDate",
                                "AvailableDateCalculationMethod" );
                        }

                        return LearningClass.LearningSemester.StartDate.Value.AddDays( AvailableDateOffset ?? 0 );
                    case AvailableDateCalculationMethod.EnrollmentOffset:
                        if ( !LearningClass.CreatedDateTime.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "LearningClass.CreatedDateTime",
                                "AvailableDateCalculationMethod" );
                        }

                        return LearningClass.CreatedDateTime.Value.AddDays( AvailableDateOffset ?? 0 );
                    case AvailableDateCalculationMethod.AfterPreviousCompleted:
                        return null;
                    case AvailableDateCalculationMethod.AlwaysAvailable:
                        return DateTime.MinValue;
                    default:
                        throw new InvalidCalculatedDateException(
                            $"Invalid or Unknown AvailableDateCalculationMethod value: {AvailableDateCalculationMethod}" );
                }
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
                        if ( !DueDateDefault.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "DueDateDefault",
                                "DueDateCalculationMethod" );
                        }

                        return DueDateDefault.Value;
                    case DueDateCalculationMethod.ClassStartOffset:
                        if ( !LearningClass.LearningSemester.StartDate.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "LearningClass.LearningSemester.StartDate",
                                "DueDateCalculationMethod" );
                        }

                        return LearningClass.LearningSemester.StartDate.Value.AddDays( DueDateOffset ?? 0 );
                    case DueDateCalculationMethod.EnrollmentOffset:
                        if ( !LearningClass.CreatedDateTime.HasValue )
                        {
                            throw new InvalidCalculatedDateException(
                                "LearningClass.CreatedDateTime",
                                "DueDateCalculationMethod" );
                        }

                        return LearningClass.CreatedDateTime.Value.AddDays( DueDateOffset ?? 0 );
                    case DueDateCalculationMethod.NoDate:
                        return null;
                    default:
                        throw new InvalidCalculatedDateException(
                            $"Invalid or Unknown DueDateCalculationMethod value: {DueDateCalculationMethod}" );
                }
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
    }

    /// <summary>
    /// Exception to throw if Available or DueDate Calculation is invalid
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidCalculatedDateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCalculatedDateException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidCalculatedDateException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCalculatedDateException"/> class with a formatted message.
        /// </summary>
        /// <param name="emptyPropertyName">
        ///     The name of property that should have a non-null value
        ///     (e.g. <see cref="Rock.Model.LearningActivity.AvailableDateDefault"/>).
        /// </param>
        /// <param name="calculationPropertyName">
        ///     The name of the property whose value requires configuration from other properties
        ///     (e.g. <see cref="Rock.Model.LearningActivity.AvailableDateCalculationMethod"/> or
        ///     <see cref="Rock.Model.LearningActivity.DueDateCalculationMethod"/>).
        /// </param>
        public InvalidCalculatedDateException( string emptyPropertyName, string calculationPropertyName ) : base(
            $"{emptyPropertyName} is missing or empty, but {calculationPropertyName} expects a value."
            )
        {

        }
    }
}
