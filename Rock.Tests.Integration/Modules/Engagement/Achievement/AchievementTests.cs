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
using System.Data.Entity;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Achievement;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Engagement.Achievements
{
    /// <summary>
    /// Tests for Achievements that use the database
    /// </summary>
    [TestClass]
    public class AchievementTests : DatabaseTestsBase
    {
        private const string ComponentEntityTypeName = "Rock.Achievement.Component.StreakAchievement";
        private const string StreakTypeGuidString = "93050DB0-82FC-4EBE-9AB8-8BB8BADFB2F0";
        private const string TestRecordForeignKey = "Test.Streak.Achievement";
        private const int NUMBER_OF_ALIASES = 2;

        private const string StreakGuidString = "6BE5BC0D-0FB1-4FAD-894D-F81DD21C4248";

        private static int _firstPersonAliasId { get; set; }
        private static int _lastPersonAliasId { get; set; }
        private static int _achievementTypeId { get; set; }

        /*
         * Occurrences    00100111111111100001111110000111
         * Engagements    00100010101111100001100110000011
         * Exclusions     00000001010000000000001000000100
         * Day            11098765432109876543210987654321
         * Month          2                              1
         */

        #region Setup Methods

        /// <summary>
        /// Create the streak type used to test
        /// </summary>
        private static void CreateStreakTypeData()
        {
            var rockContext = new RockContext();

            var streakType = new StreakType
            {
                Guid = new Guid( StreakTypeGuidString ),
                OccurrenceMap = new byte[] { 0b_0010_0111, 0b_1111_1110, 0b_0001_1111, 0b_1000_0111 },
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = new DateTime( 2019, 1, 1 ),
                Name = "Testing StreakType",
                ForeignKey = TestRecordForeignKey
            };

            var streak = new Streak
            {
                Guid = StreakGuidString.AsGuid(),
                EnrollmentDate = new DateTime( 2019, 1, 1 ),
                PersonAliasId = _lastPersonAliasId,
                EngagementMap = new byte[] { 0b_0010_0010, 0b_1011_1110, 0b_0000_0000, 0b_0000_0000 },
                ExclusionMap = new byte[] { 0b_0000_0010, 0b_0000_0100 },
                ForeignKey = TestRecordForeignKey
            };

            var streak2 = new Streak
            {
                EnrollmentDate = new DateTime( 2019, 1, 1 ),
                PersonAliasId = _firstPersonAliasId,
                EngagementMap = new byte[] { 0b_0000_0000, 0b_0000_0000, 0b_0001_1001, 0b_1000_0011 },
                ExclusionMap = new byte[] { 0b_0000_0010, 0b_0000_0100 },
                ForeignKey = TestRecordForeignKey
            };

            var exclusion = new StreakTypeExclusion
            {
                ExclusionMap = new byte[] { 0b_0000_0001, 0b_0100_0000, 0b_0000_0000, 0b_0000_0000 }
            };

            streakType.StreakTypeExclusions.Add( exclusion );
            streakType.Streaks.Add( streak );
            streakType.Streaks.Add( streak2 );

            var streakTypeService = new StreakTypeService( rockContext );

            streakTypeService.Add( streakType );

            rockContext.SaveChanges( true );
        }

        private static string TerryTestPersonGuid = "198283AB-A6C2-427A-95BA-51CE4CA8BED1";

        /// <summary>
        /// Creates the person alias data.
        /// </summary>
        private static void CreatePersonData()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var person = new Person
            {
                Guid = TerryTestPersonGuid.AsGuid(),
                NickName = "Bill",
                LastName = "Bailey",
                ForeignKey = TestRecordForeignKey
            };

            PersonService.SaveNewPerson( person, rockContext );

            rockContext.SaveChanges();

            var terryTestGuid = TerryTestPersonGuid.AsGuid();

            var personAliasService = new PersonAliasService( rockContext );
            var personAliases = personAliasService.Queryable().Where( pa => pa.Person.Guid == terryTestGuid ).ToList();
            var personAliasesToAdd = NUMBER_OF_ALIASES - personAliases.Count;

            var aliasPersonId = personService.Queryable().Max( p => p.Id ) + 1;
            personAliasService.DeleteRange( personAliasService.Queryable().Where( pa => pa.AliasPersonId >= aliasPersonId ) );

            rockContext.SaveChanges();

            person = personService.Get( terryTestGuid );

            for ( var i = personAliasesToAdd; i > 0; i-- )
            {
                personAliasService.Add( new PersonAlias { Person = person, AliasPersonGuid = Guid.NewGuid(), AliasPersonId = aliasPersonId + i, ForeignKey = TestRecordForeignKey } );
            }
            rockContext.SaveChanges();

            var personAlias = personAliasService.Queryable().Where( pa => pa.Person.Guid == terryTestGuid ).Take( NUMBER_OF_ALIASES ).Select( p => p.Id ).ToList();
            _firstPersonAliasId = personAlias.First();
            _lastPersonAliasId = personAlias.Last();
        }

        /// <summary>
        /// Creates the achievement type data.
        /// </summary>
        private static void CreateAchievementTypeData()
        {
            // Create the component so it sets up the attributes.
            AchievementContainer.Instance.Refresh();
            _ = AchievementContainer.GetComponent( ComponentEntityTypeName );

            var rockContext = new RockContext();

            var achievementType = new AchievementType
            {
                Name = "Test Achievement",
                IsActive = true,
                ComponentEntityTypeId = EntityTypeCache.GetId( ComponentEntityTypeName ) ?? 0,
                MaxAccomplishmentsAllowed = 2,
                AllowOverAchievement = false,
                AchieverEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id
            };

            var achievementTypeService = new AchievementTypeService( rockContext );

            achievementTypeService.Add( achievementType );
            rockContext.SaveChanges( true );

            achievementType.LoadAttributes();
            achievementType.SetAttributeValue( Rock.Achievement.Component.StreakAchievement.AttributeKey.TimespanInDays, 7.ToString() );
            achievementType.SetAttributeValue( Rock.Achievement.Component.StreakAchievement.AttributeKey.NumberToAchieve, 4.ToString() );
            achievementType.SaveAttributeValues();

            _achievementTypeId = achievementType.Id;
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            CreatePersonData();
            CreateStreakTypeData();
            CreateAchievementTypeData();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DeleteAchievementsData();
        }

        private static void DeleteAchievementsData()
        {
            var rockContext = new RockContext();

            var service = new AchievementAttemptService( rockContext );
            service.DeleteRange( service.Queryable().Where( saa => saa.AchievementTypeId == _achievementTypeId ) );
            rockContext.SaveChanges();
        }

        #endregion Setup Methods

        /// <summary>
        /// Tests StreakAchievementProcess
        /// </summary>
        [TestMethod]
        public void StreakAchievementProcess()
        {
            var rockContext = new RockContext();

            var attemptsQuery = new AchievementAttemptService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == _achievementTypeId
                    && (saa.AchieverEntityId == _firstPersonAliasId || saa.AchieverEntityId == _lastPersonAliasId) )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( rockContext ).Get( StreakGuidString.AsGuid() );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( rockContext, achievementTypeCache, streak );

            rockContext.SaveChanges();

            // Force the post-save change hook to be executed immediately so we can be certain it is completed.
            // This could be more elegantly implemented if the async behavior could be disabled for a service instance.

            StreakService.HandlePostSaveChanges( streak.Id );

            rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 7, attempts.Count );

            Assert.That.AreEqual( new DateTime( 2019, 1, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 2 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .5m, attempts[0].Progress );
            Assert.That.IsTrue( attempts[0].IsClosed );
            Assert.That.IsFalse( attempts[0].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 8 ), attempts[1].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 9 ), attempts[1].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .5m, attempts[1].Progress );
            Assert.That.IsTrue( attempts[1].IsClosed );
            Assert.That.IsFalse( attempts[1].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 12 ), attempts[2].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 18 ), attempts[2].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[2].Progress );
            Assert.That.IsTrue( attempts[2].IsClosed );
            Assert.That.IsFalse( attempts[2].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 13 ), attempts[3].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 19 ), attempts[3].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[3].Progress );
            Assert.That.IsTrue( attempts[3].IsClosed );
            Assert.That.IsFalse( attempts[3].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 18 ), attempts[4].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 21 ), attempts[4].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( 1m, attempts[4].Progress );
            Assert.That.IsTrue( attempts[4].IsClosed );
            Assert.That.IsTrue( attempts[4].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 22 ), attempts[5].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 26 ), attempts[5].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[5].Progress );
            Assert.That.IsTrue( attempts[5].IsClosed );
            Assert.That.IsFalse( attempts[5].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 30 ), attempts[6].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 30 ), attempts[6].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .25m, attempts[6].Progress );
            Assert.That.IsTrue( attempts[6].IsClosed );
            Assert.That.IsFalse( attempts[6].IsSuccessful );
        }

        /// <summary>
        /// Tests StreakAchievementProcess with open attempt
        /// </summary>
        [TestMethod]
        public void StreakAchievementProcessWithOpenAttempt()
        {
            DeleteAchievementsData();

            var rockContext = new RockContext();

            var attemptsQuery = new AchievementAttemptService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == _achievementTypeId
                    && ( saa.AchieverEntityId == _firstPersonAliasId || saa.AchieverEntityId == _lastPersonAliasId ) )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var attempt = new AchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 1, 1 ),
                AchievementAttemptEndDateTime = new DateTime( 2019, 1, 2 ),
                Progress = .5m,
                AchieverEntityId = _firstPersonAliasId,
                AchievementTypeId = _achievementTypeId,
                ForeignKey = TestRecordForeignKey
            };
            var attemptService = new AchievementAttemptService( rockContext );
            attemptService.Add( attempt );
            Assert.That.AreEqual( 0, attempt.Id );
            rockContext.SaveChanges();

            // There should now be one attempt
            Assert.That.AreEqual( 1, attemptsQuery.Count() );
            Assert.That.IsTrue( attempt.Id > 0 );

            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( rockContext ).Get( StreakGuidString.AsGuid() );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( rockContext, achievementTypeCache, streak );
            rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 7, attempts.Count );

            Assert.That.AreEqual( attempt.Id, attempts[0].Id );
            Assert.That.AreEqual( new DateTime( 2019, 1, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 2 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .5m, attempts[0].Progress );
            Assert.That.IsTrue( attempts[0].IsClosed );
            Assert.That.IsFalse( attempts[0].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 8 ), attempts[1].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 9 ), attempts[1].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .5m, attempts[1].Progress );
            Assert.That.IsTrue( attempts[1].IsClosed );
            Assert.That.IsFalse( attempts[1].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 12 ), attempts[2].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 18 ), attempts[2].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[2].Progress );
            Assert.That.IsTrue( attempts[2].IsClosed );
            Assert.That.IsFalse( attempts[2].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 13 ), attempts[3].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 19 ), attempts[3].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[3].Progress );
            Assert.That.IsTrue( attempts[3].IsClosed );
            Assert.That.IsFalse( attempts[3].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 18 ), attempts[4].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 21 ), attempts[4].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( 1m, attempts[4].Progress );
            Assert.That.IsTrue( attempts[4].IsClosed );
            Assert.That.IsTrue( attempts[4].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 22 ), attempts[5].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 26 ), attempts[5].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[5].Progress );
            Assert.That.IsTrue( attempts[5].IsClosed );
            Assert.That.IsFalse( attempts[5].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 30 ), attempts[6].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 30 ), attempts[6].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .25m, attempts[6].Progress );
            Assert.That.IsTrue( attempts[6].IsClosed );
            Assert.That.IsFalse( attempts[6].IsSuccessful );
        }

        /// <summary>
        /// Tests StreakAchievementProcess with open attempt that doesn't make sense with the data and it should be closed. 
        /// </summary>
        [TestMethod]
        public void StreakAchievementProcessWithSuccessfulAttempt()
        {
            DeleteAchievementsData();

            var rockContext = new RockContext();

            var attemptsQuery = new AchievementAttemptService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == _achievementTypeId
                    && ( saa.AchieverEntityId == _firstPersonAliasId || saa.AchieverEntityId == _lastPersonAliasId ) )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var attempt = new AchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 2, 1 ),
                AchievementAttemptEndDateTime = new DateTime( 2019, 2, 1 ),
                Progress = .5m,
                AchieverEntityId = _firstPersonAliasId,
                AchievementTypeId = _achievementTypeId,
                IsClosed = true,
                IsSuccessful = true,
                Guid = TestRecordForeignKey.AsGuid()
            };
            var attemptService = new AchievementAttemptService( rockContext );
            attemptService.Add( attempt );
            Assert.That.AreEqual( 0, attempt.Id );
            rockContext.SaveChanges();

            // There should now be one attempt
            Assert.That.AreEqual( 1, attemptsQuery.Count() );
            Assert.That.IsTrue( attempt.Id > 0 );

            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( rockContext ).Get( StreakGuidString.AsGuid() );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( rockContext, achievementTypeCache, streak );
            rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 1, attempts.Count );

            Assert.That.AreEqual( attempt.Id, attempts[0].Id );
            Assert.That.AreEqual( new DateTime( 2019, 2, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 2, 1 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( 0.5m, attempts[0].Progress );
            Assert.That.IsTrue( attempts[0].IsClosed );
            Assert.That.IsTrue( attempts[0].IsSuccessful );
        }
    }
}
