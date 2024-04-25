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
    /// Tests for Accumulative Achievements that use the database
    /// </summary>
    [TestClass]
    public class AccumulativeAchievementTests : DatabaseTestsBase
    {
        private const string ComponentEntityTypeName = "Rock.Achievement.Component.AccumulativeAchievement";
        private const string StreakTypeGuidString = "93050DB0-82FC-4EBE-9AB8-8BB8BADFB2F0";
        private const string TestRecordForeignKey = "Test.AccumulativeAchievement";
        private const int NUMBER_OF_ALIASES = 2;

        private static RockContext _rockContext { get; set; }
        private static StreakTypeService _streakTypeService { get; set; }
        private static AchievementTypeService _achievementTypeService { get; set; }

        private static int _streakTypeId { get; set; }
        private static int _streakId { get; set; }
        private static int _firstPersonAliasId { get; set; }
        private static int _lastPersonAliasId { get; set; }

        private static int _achievementTypeId { get; set; }

        /*
         * Occurrences    111111111111111111111111
         * Engagements    001010101010010010000011
         * Day            432109876543210987654321
         * Month                                 1
         */

        #region Setup Methods

        /// <summary>
        /// Create the streak type used to test
        /// </summary>
        private static void CreateStreakTypeData()
        {
            var streakType = new StreakType
            {
                Guid = new Guid( StreakTypeGuidString ),
                OccurrenceMap = new byte[] { 0b_1111_1111, 0b_1111_1111, 0b_1111_1111 },
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = new DateTime( 2019, 1, 1 ),
                Name = "Testing StreakType",
                ForeignKey = TestRecordForeignKey
            };

            var streak = new Streak
            {
                EnrollmentDate = new DateTime( 2019, 1, 1 ),
                PersonAliasId = _lastPersonAliasId,
                EngagementMap = new byte[] { 0b_0010_1010, 0b_1010_0100, 0b_1000_0011 },
                ForeignKey = TestRecordForeignKey
            };

            streakType.Streaks.Add( streak );
            _streakTypeService.Add( streakType );
            _rockContext.SaveChanges( true );

            _streakTypeId = streakType.Id;
            _streakId = streak.Id;
        }

        /// <summary>
        /// Creates the person alias data.
        /// </summary>
        private static void CreatePersonAliasData()
        {
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();

            var personAliasService = new PersonAliasService( _rockContext );
            var personAliases = personAliasService.Queryable().Where( pa => pa.Person.Guid == tedDeckerGuid ).ToList();
            var personAliasesToAdd = NUMBER_OF_ALIASES - personAliases.Count;

            var personService = new PersonService( _rockContext );
            var aliasPersonId = personService.Queryable().Max( p => p.Id ) + 1;
            personAliasService.DeleteRange( personAliasService.Queryable().Where( pa => pa.AliasPersonId >= aliasPersonId ) );

            _rockContext.SaveChanges();
            var tedDecker = personService.Get( tedDeckerGuid );

            for ( var i = personAliasesToAdd; i > 0; i-- )
            {
                personAliasService.Add( new PersonAlias { Person = tedDecker, AliasPersonGuid = Guid.NewGuid(), AliasPersonId = aliasPersonId + i } );
            }
            _rockContext.SaveChanges();

            var personAlias = personAliasService.Queryable().Where( pa => pa.Person.Guid == tedDeckerGuid ).Take( NUMBER_OF_ALIASES ).Select( p => p.Id ).ToList();
            _firstPersonAliasId = personAlias.First();
            _lastPersonAliasId = personAlias.Last();
        }
        
        /// <summary>
        /// Delete the streak type created by this test class
        /// </summary>
        private static void DeleteTestData()
        {
            DeleteAchievementsData();

            // Remove Streak Type.
            var streakTypeGuid = new Guid( StreakTypeGuidString );
            var streakType = _streakTypeService.Queryable().FirstOrDefault( st => st.Guid == streakTypeGuid );

            if ( streakType != null )
            {
                _streakTypeService.Delete( streakType );
            }

            _streakTypeId = default;
            _rockContext.SaveChanges();

            // Remove Streaks.
            var streakService = new StreakService( _rockContext );

            var testStreaks = streakService.Queryable().Where( x => x.ForeignKey == TestRecordForeignKey ).ToList();
            streakService.DeleteRange( testStreaks );

            _rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the achievement type data.
        /// </summary>
        private static void CreateAchievementTypeData()
        {
            // Create the component so it sets up the attributes.
            AchievementContainer.Instance.Refresh();
            _ = AchievementContainer.GetComponent( ComponentEntityTypeName );

            var achievementType = new AchievementType
            {
                Name = "Test Achievement",
                IsActive = true,
                ComponentEntityTypeId = EntityTypeCache.GetId( ComponentEntityTypeName ) ?? 0,
                MaxAccomplishmentsAllowed = 2,
                AllowOverAchievement = false,
                ComponentConfigJson = "{\"StreakType\": \"" + StreakTypeGuidString + "\"}",
                AchieverEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id
            };

            _achievementTypeService.Add( achievementType );
            _rockContext.SaveChanges( true );

            achievementType.LoadAttributes();
            achievementType.SetAttributeValue( Rock.Achievement.Component.AccumulativeAchievement.AttributeKey.StreakType, StreakTypeGuidString );
            achievementType.SetAttributeValue( Rock.Achievement.Component.AccumulativeAchievement.AttributeKey.TimespanInDays, 7.ToString() );
            achievementType.SetAttributeValue( Rock.Achievement.Component.AccumulativeAchievement.AttributeKey.NumberToAccumulate, 4.ToString() );
            achievementType.SaveAttributeValues();

            _achievementTypeId = achievementType.Id;
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            _rockContext = new RockContext();
            _streakTypeService = new StreakTypeService( _rockContext );
            _achievementTypeService = new AchievementTypeService( _rockContext );

            DeleteTestData();
            CreatePersonAliasData();
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
            var service = new AchievementAttemptService( _rockContext );
            service.DeleteRange( service.Queryable().Where( saa => saa.AchievementTypeId == _achievementTypeId ) );
            _rockContext.SaveChanges();
        }

        #endregion Setup Methods

        /// <summary>
        /// Tests AccumulativeAchievementProcess
        /// </summary>
        [TestMethod]
        public void AccumulativeAchievementProcess()
        {
            var achievementAttemptService = new AchievementAttemptService( _rockContext );
            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var attemptsQuery = achievementAttemptService
                .GetOrderedAchieverAttempts( achievementAttemptService.Queryable().AsNoTracking(), achievementTypeCache, _firstPersonAliasId );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );
            _rockContext.SaveChanges();

            var attempts = achievementAttemptService
                .GetOrderedAchieverAttempts( achievementAttemptService.Queryable().AsNoTracking(), achievementTypeCache, _firstPersonAliasId )
                .OrderBy( aa => aa.AchievementAttemptStartDateTime )
                .ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 5, attempts.Count );

            Assert.That.AreEqual( new DateTime( 2019, 1, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 2 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .5m, attempts[0].Progress );
            Assert.That.IsTrue( attempts[0].IsClosed );
            Assert.That.IsFalse( attempts[0].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 8 ), attempts[1].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 14 ), attempts[1].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[1].Progress );
            Assert.That.IsTrue( attempts[1].IsClosed );
            Assert.That.IsFalse( attempts[1].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 11 ), attempts[2].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 16 ), attempts[2].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[2].Progress );
            Assert.That.IsTrue( attempts[2].IsClosed );
            Assert.That.IsFalse( attempts[2].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 14 ), attempts[3].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 20 ), attempts[3].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( 1m, attempts[3].Progress );
            Assert.That.IsTrue( attempts[3].IsClosed );
            Assert.That.IsTrue( attempts[3].IsSuccessful );

            Assert.That.AreEqual( new DateTime( 2019, 1, 22 ), attempts[4].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 22 ), attempts[4].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .25m, attempts[4].Progress );
            Assert.That.IsTrue( attempts[4].IsClosed );
            Assert.That.IsFalse( attempts[4].IsSuccessful );
        }

        /// <summary>
        /// Tests AccumulativeAchievementProcess with open attempt
        /// </summary>
        [TestMethod]
        public void AccumulativeAchievementProcessWithOpenAttempt()
        {
            var achievementAttemptService = new AchievementAttemptService( _rockContext );
            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var attemptsQuery = achievementAttemptService
                .GetOrderedAchieverAttempts( achievementAttemptService.Queryable().AsNoTracking(), achievementTypeCache, _firstPersonAliasId );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            // This attempt should get deleted since it is wrong and after any successful attempt
            var attempt = new AchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 3, 1 ),
                Progress = .25m,
                AchieverEntityId = _firstPersonAliasId,
                AchievementTypeId = _achievementTypeId,
                Guid = TestRecordForeignKey.AsGuid()
            };
            var attemptService = new AchievementAttemptService( _rockContext );
            attemptService.Add( attempt );
            Assert.That.AreEqual( 0, attempt.Id );
            _rockContext.SaveChanges();

            // There should now be one attempt
            attemptsQuery = achievementAttemptService
                .GetOrderedAchieverAttempts( achievementAttemptService.Queryable().AsNoTracking(), achievementTypeCache, _firstPersonAliasId );
            Assert.That.AreEqual( 1, attemptsQuery.Count() );
            Assert.That.IsTrue( attempt.Id > 0 );

            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );

            // Force the post-save changes to execute synchronously so we can see the result immediately.
            StreakService.HandlePostSaveChanges( streak.Id );

            _rockContext.SaveChanges();

            var attempts = achievementAttemptService
                .GetOrderedAchieverAttempts( achievementAttemptService.Queryable().AsNoTracking(), achievementTypeCache, _firstPersonAliasId )
                .OrderBy(aa => aa.AchievementAttemptStartDateTime)
                .ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 5, attempts.Count );

            Assert.That.AreNotEqual( attempt.Id, attempts[0].Id );
            Assert.That.AreEqual( new DateTime( 2019, 1, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 2 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .5m, attempts[0].Progress );
            Assert.That.IsTrue( attempts[0].IsClosed );
            Assert.That.IsFalse( attempts[0].IsSuccessful );

            Assert.That.AreNotEqual( attempt.Id, attempts[1].Id );
            Assert.That.AreEqual( new DateTime( 2019, 1, 8 ), attempts[1].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 14 ), attempts[1].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[1].Progress );
            Assert.That.IsTrue( attempts[1].IsClosed );
            Assert.That.IsFalse( attempts[1].IsSuccessful );

            Assert.That.AreNotEqual( attempt.Id, attempts[2].Id );
            Assert.That.AreEqual( new DateTime( 2019, 1, 11 ), attempts[2].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 16 ), attempts[2].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .75m, attempts[2].Progress );
            Assert.That.IsTrue( attempts[2].IsClosed );
            Assert.That.IsFalse( attempts[2].IsSuccessful );

            Assert.That.AreNotEqual( attempt.Id, attempts[3].Id );
            Assert.That.AreEqual( new DateTime( 2019, 1, 14 ), attempts[3].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 20 ), attempts[3].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( 1m, attempts[3].Progress );
            Assert.That.IsTrue( attempts[3].IsClosed );
            Assert.That.IsTrue( attempts[3].IsSuccessful );

            Assert.That.AreNotEqual( attempt.Id, attempts[4].Id );
            Assert.That.AreEqual( new DateTime( 2019, 1, 22 ), attempts[4].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 1, 22 ), attempts[4].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .25m, attempts[4].Progress );
            Assert.That.IsTrue( attempts[4].IsClosed );
            Assert.That.IsFalse( attempts[4].IsSuccessful );
        }

        /// <summary>
        /// Tests AccumulativeAchievementProcess with successful attempt. No changes should be made
        /// because there is no data after the most recent successful attempt.
        /// </summary>
        [TestMethod]
        public void AccumulativeAchievementProcessWithSuccessfulAttempt()
        {
            var achievementAttemptService = new AchievementAttemptService( _rockContext );
            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var attemptsQuery = achievementAttemptService
                .GetOrderedAchieverAttempts( achievementAttemptService.Queryable().AsNoTracking(), achievementTypeCache, _firstPersonAliasId );
            
            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var attempt1 = new AchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 3, 1 ),
                Progress = .25m,
                AchieverEntityId = _firstPersonAliasId,
                AchievementTypeId = _achievementTypeId,
                Guid = TestRecordForeignKey.AsGuid()
            };

            var attempt2 = new AchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 4, 1 ),
                AchievementAttemptEndDateTime = new DateTime( 2019, 4, 5 ),
                Progress = 1m,
                AchieverEntityId = _lastPersonAliasId,
                AchievementTypeId = _achievementTypeId,
                IsClosed = true,
                IsSuccessful = true,
                Guid = TestRecordForeignKey.AsGuid()
            };

            var attemptService = new AchievementAttemptService( _rockContext );
            attemptService.Add( attempt1 );
            attemptService.Add( attempt2 );
            Assert.That.AreEqual( 0, attempt1.Id );
            Assert.That.AreEqual( 0, attempt2.Id );
            _rockContext.SaveChanges();

            // There should now be two attempts
            attemptsQuery = achievementAttemptService
                .GetOrderedAchieverAttempts( achievementAttemptService.Queryable().AsNoTracking(), achievementTypeCache, _firstPersonAliasId );
            Assert.That.AreEqual( 2, attemptsQuery.Count() );
            Assert.That.IsTrue( attempt1.Id > 0 );
            Assert.That.IsTrue( attempt2.Id > 0 );

            var originalId1 = attempt1.Id;
            var originalId2 = attempt2.Id;

            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );
            _rockContext.SaveChanges();

            var attempts = achievementAttemptService
                .GetOrderedAchieverAttempts( achievementAttemptService.Queryable().AsNoTracking(), achievementTypeCache, _firstPersonAliasId );
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 2, attempts.Count );

            Assert.That.AreEqual( attempt1.Id, attempts[1].Id );
            Assert.That.AreEqual( originalId1, attempts[1].Id );
            Assert.That.AreEqual( new DateTime( 2019, 3, 1 ), attempts[1].AchievementAttemptStartDateTime );
            Assert.That.IsNull( attempts[1].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .25m, attempts[1].Progress );
            Assert.That.IsFalse( attempts[1].IsClosed );
            Assert.That.IsFalse( attempts[1].IsSuccessful );

            Assert.That.AreEqual( attempt2.Id, attempts[0].Id );
            Assert.That.AreEqual( originalId2, attempts[0].Id );
            Assert.That.AreEqual( new DateTime( 2019, 4, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 4, 5 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( 1m, attempts[0].Progress );
            Assert.That.IsTrue( attempts[0].IsClosed );
            Assert.That.IsTrue( attempts[0].IsSuccessful );
        }
    }
}
