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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class CheckinResult : RockDynamic
    {
        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public CheckInPerson Person { get; set; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public CheckInGroup Group { get; set; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        [DataMember]
        public Location Location { get; set; }

        /// <summary>
        /// Gets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public CheckInSchedule Schedule { get; set; }

        /// <summary>
        /// Gets the detail message.
        /// </summary>
        /// <value>
        /// The detail message.
        /// </value>
        [DataMember]
        public string DetailMessage => $"{Person} was checked into {Group} in {Location.Name} at {Schedule}";

        /// <summary>
        /// Gets the in progress achievement attempts.
        /// </summary>
        /// <value>
        /// The in progress achievement attempts.
        /// </value>
        [DataMember]
        public AchievementAttempt[] InProgressAchievementAttempts { get; internal set; }

        /// <summary>
        /// Gets the completed achievement attempts.
        /// </summary>
        /// <value>
        /// The completed achievement attempts.
        /// </value>
        [DataMember]
        public AchievementAttempt[] CompletedAchievementAttempts { get; internal set; }

        /// <summary>
        /// Gets the just completed achievement attempts.
        /// </summary>
        /// <value>
        /// The just completed achievement attempts.
        /// </value>
        [DataMember]
        public AchievementAttempt[] JustCompletedAchievementAttempts { get; internal set; }

        /// <summary>
        /// Gets the person achievement types.
        /// </summary>
        /// <param name="includeJustCompleted">if set to <c>true</c> [include just completed].</param>
        /// <returns></returns>
        public PersonAchievementType[] GetPersonAchievementTypes( bool includeJustCompleted )
        {
            // For each checkin, we only want to show one achievement per AchievementType
            // If there is one in Progress, include that
            // otherwise add the first Completed one of each AchievementType
            List<AchievementAttempt> achievementAttempts = new List<AchievementAttempt>();
            var checkinResult = this;

            if ( checkinResult.InProgressAchievementAttempts?.Any() == true )
            {
                achievementAttempts.AddRange( checkinResult.InProgressAchievementAttempts );
            }

            if ( checkinResult.CompletedAchievementAttempts?.Any() == true )
            {
                achievementAttempts.AddRange( checkinResult.CompletedAchievementAttempts );
            }

            if ( achievementAttempts.Any() == true )
            {
                PersonAchievementType[] personAchievementTypes = achievementAttempts
                    .GroupBy( a => a.AchievementTypeId )
                    .Select( a =>
                    {
                        var achievmentTypeId = a.Key;
                        var achievementType = AchievementTypeCache.Get( achievmentTypeId );
                        var person = checkinResult.Person.Person;
                        AchievementAttempt justCompleted = null;
                        if ( includeJustCompleted )
                        {
                            justCompleted = this.JustCompletedAchievementAttempts.Where( j => j.AchievementTypeId == achievmentTypeId ).FirstOrDefault();
                        }

                        return new PersonAchievementType( person, achievementType, a.ToArray(), justCompleted );
                    } ).ToArray();

                return personAchievementTypes;
            }

            return new PersonAchievementType[0];
        }

        /// <summary>
        /// Updates the achievement fields.
        /// </summary>
        /// <param name="successfullyCompletedAchievementIdsPriorToCheckin">The successfully completed achievement ids prior to checkin.</param>
        /// <param name="achievementsStateAfterCheckin">The achievements state after checkin.</param>
        public void UpdateAchievementFields( int[] successfullyCompletedAchievementIdsPriorToCheckin, AchievementAttemptService.AchievementAttemptWithPersonAlias[] achievementsStateAfterCheckin )
        {
            if ( achievementsStateAfterCheckin == null )
            {
                return;
            }

            var checkinResult = this;
            var person = this.Person.Person;
            var achievementsStateAfterCheckinForPerson = achievementsStateAfterCheckin.Where( a => a.AchieverPersonAlias.PersonId == person.Id ).ToArray();

            checkinResult.InProgressAchievementAttempts = achievementsStateAfterCheckinForPerson
                .Where( a => !a.AchievementAttempt.IsSuccessful && !a.AchievementAttempt.IsClosed )
                .Select( a => a.AchievementAttempt )
                .ToArray();

            var completedAchievementAttempts = achievementsStateAfterCheckinForPerson
                .Where( a => a.AchievementAttempt.IsSuccessful && a.AchievementAttempt.IsClosed )

                .Select( a => a.AchievementAttempt )
                .ToArray();


            checkinResult.JustCompletedAchievementAttempts = completedAchievementAttempts
                .Where( a => !successfullyCompletedAchievementIdsPriorToCheckin.Contains( a.Id ) )
                .ToArray();


            checkinResult.CompletedAchievementAttempts = completedAchievementAttempts;
        }
    }
}