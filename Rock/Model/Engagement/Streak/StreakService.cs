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
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Streak"/> entity objects.
    /// </summary>
    public partial class StreakService
    {
        /// <summary>
        /// Get the person's streaks in the streak type
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public IQueryable<Streak> GetByStreakTypeAndPerson( int streakTypeId, int personId )
        {
            return Queryable().Where( se => se.StreakTypeId == streakTypeId && se.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Is the person enrolled in the streak type
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public bool IsEnrolled( int streakTypeId, int personId )
        {
            return Queryable().AsNoTracking().Any( se => se.StreakTypeId == streakTypeId && se.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Start an async task to do things that should run after a streak has been updated
        /// </summary>
        /// <param name="streakId"></param>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use the HandlePostSaveChanges method instead.", true )]
        public static void HandlePostSaveChangesAsync( int streakId )
        {
            Task.Run( () => HandlePostSaveChanges( streakId ) );
        }

        /// <summary>
        /// Complete tasks that should be done after a streak or related data has been changed
        /// </summary>
        /// <param name="streakId"></param>
        public static void HandlePostSaveChanges( int streakId )
        {
            RefreshStreakDenormalizedProperties( streakId );
        }

        /// <summary>
        /// Start an async task to calculate steak data and then copy it to the enrollment model
        /// </summary>
        /// <param name="streakId">The streak identifier.</param>
        public static void RefreshStreakDenormalizedProperties( int streakId )
        {
            var rockContext = new RockContext();
            var streakService = new StreakService( rockContext );
            var streakTypeService = new StreakTypeService( rockContext );

            // Get the streak data and validate it
            var streakData = streakTypeService.GetStreakData( streakId, out var errorMessage );

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

            // Get the streak and apply updated information to it
            var streak = streakService.Get( streakId );
            if ( streak == null )
            {
                ExceptionLogService.LogException( $"The streak with id {streakId} was not found (it may have been deleted)" );
                return;
            }

            CopyStreakDataToStreakModel( streakData, streak );
            rockContext.SaveChanges( true );
        }

        /// <summary>
        /// Copy streak data to an enrollment model
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private static void CopyStreakDataToStreakModel( StreakData source, Streak target )
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

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="currentPerson">The logged in person who is performing the delete</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Streak item, Person currentPerson, out string errorMessage )
        {
            // persons with no administrator priviledge should not be allowed to delete streak detail
            if ( !item.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, currentPerson ) )
            {
                errorMessage = "You are not authorized to delete this item.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }
    }
}