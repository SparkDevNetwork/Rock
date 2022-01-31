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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Helper class that includes the <see cref="Rock.Model.Person" /> with the <seealso cref="Rock.Model.AchievementAttempt">Achievements</seealso> for the specified <seealso cref="Rock.Model.AchievementType"/>
    /// </summary>
    [DataContract]
    public class PersonAchievementType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAchievementType"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="achievementType">Type of the achievement.</param>
        /// <param name="achievementAttempts">The achievement attempts.</param>
        /// <param name="justCompletedAchievementAttempt">The just completed achievement attempt.</param>
        public PersonAchievementType( Person person, AchievementTypeCache achievementType, AchievementAttempt[] achievementAttempts, AchievementAttempt justCompletedAchievementAttempt )
        {
            Person = person;
            AchievementType = achievementType;
            JustCompletedAchievementAttempt = justCompletedAchievementAttempt;

            CurrentInProgressAchievement = achievementAttempts.Where( a => !a.IsSuccessful && !a.IsClosed ).FirstOrDefault();
            AchievementAttempts = achievementAttempts;
        }

        /// <summary>
        /// Gets the type of the achievement.
        /// </summary>
        /// <value>
        /// The type of the achievement.
        /// </value>
        [DataMember]
        public AchievementTypeCache AchievementType { get; }

        /// <summary>
        /// Gets the just completed achievement attempt.
        /// </summary>
        /// <value>
        /// The just completed achievement attempt.
        /// </value>
        [DataMember]
        public AchievementAttempt JustCompletedAchievementAttempt { get; }

        /// <summary>
        /// Gets a value indicating whether [just completed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [just completed]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool JustCompleted => JustCompletedAchievementAttempt != null;

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public Person Person { get; }

        /// <summary>
        /// Gets the current in progress achievement.
        /// </summary>
        /// <value>
        /// The current in progress achievement.
        /// </value>
        [DataMember]
        public AchievementAttempt CurrentInProgressAchievement { get; }

        /// <summary>
        /// Gets the achievement attempts.
        /// </summary>
        /// <value>
        /// The achievement attempts.
        /// </value>
        [DataMember]
        public AchievementAttempt[] AchievementAttempts { get; }
    }
}
