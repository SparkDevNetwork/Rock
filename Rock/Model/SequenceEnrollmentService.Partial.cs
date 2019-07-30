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
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="SequenceEnrollment"/> entity objects.
    /// </summary>
    public partial class SequenceEnrollmentService
    {
        /// <summary>
        /// Get the person's enrollments in the sequence
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public IQueryable<SequenceEnrollment> GetBySequenceAndPerson( int sequenceId, int personId )
        {
            return Queryable().Where( se => se.SequenceId == sequenceId && se.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Is the person enrolled in the sequence
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public bool IsEnrolled( int sequenceId, int personId )
        {
            return Queryable().AsNoTracking().Any( se => se.SequenceId == sequenceId && se.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Start an async task to calculate steak data and then copy it to the enrollment model
        /// </summary>
        /// <param name="sequenceEnrollmentId"></param>
        public static void UpdateStreakPropertiesAsync( int sequenceEnrollmentId )
        {
            Task.Run( () =>
            {
                var rockContext = new RockContext();
                var sequenceEnrollmentService = new SequenceEnrollmentService( rockContext );
                var sequenceService = new SequenceService( rockContext );

                // Get the streak data and validate it
                var streakData = sequenceService.GetSequenceStreakData( sequenceEnrollmentId, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    ExceptionLogService.LogException( errorMessage );
                    return;
                }

                if ( streakData == null )
                {
                    ExceptionLogService.LogException( "Streak Data was null, but no error was specified" );
                    return;
                }

                // Get the enrollment and apply updated information to it
                var sequenceEnrollment = sequenceEnrollmentService.Get( sequenceEnrollmentId );
                if ( sequenceEnrollment == null )
                {
                    ExceptionLogService.LogException( "The sequence enrollment was null" );
                    return;
                }

                CopyStreakDataToEnrollment( streakData, sequenceEnrollment );
                rockContext.SaveChanges();
            } );
        }

        /// <summary>
        /// Copy streak data to an enrollment model
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyStreakDataToEnrollment( SequenceStreakData source, SequenceEnrollment target )
        {
            if ( source == null || target == null )
            {
                return;
            }

            target.EngagementCount = source.EngagementCount;

            target.LongestStreakCount = source.LongestStreakCount;
            target.LongestStreakStartDate = source.LongestStreakStartDate;
            target.LongestStreakEndDate = source.LongestStreakEndDate;

            target.CurrentStreakCount = source.CurrentStreakCount;
            target.CurrentStreakStartDate = source.CurrentStreakStartDate;
        }
    }
}