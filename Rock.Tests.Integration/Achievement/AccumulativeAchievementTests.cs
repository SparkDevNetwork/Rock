using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Achievement;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.RockTests.Model
{
    /// <summary>
    /// Tests for Accumulative Achievements that use the database
    /// </summary>
    [TestClass]
    public class AccumulativeAchievementTests
    {
        private const string ComponentEntityTypeName = "Rock.Achievement.Component.AccumulativeAchievement";
        private const string StreakTypeGuidString = "93050DB0-82FC-4EBE-9AB8-8BB8BADFB2F0";

        private static RockContext _rockContext { get; set; }
        private static StreakTypeService _streakTypeService { get; set; }
        private static AchievementTypeService _achievementTypeService { get; set; }

        private static int _streakTypeId { get; set; }
        private static int _streakId { get; set; }
        private static int _personAliasId { get; set; }
        private static int _personId { get; set; }

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
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();
            var personAlias = new PersonAliasService( _rockContext ).Queryable().First( pa => pa.Person.Guid == tedDeckerGuid );
            _personAliasId = personAlias.Id;
            _personId = personAlias.PersonId;

            var streakType = new StreakType
            {
                Guid = new Guid( StreakTypeGuidString ),
                OccurrenceMap = new byte[] { 0b_1111_1111, 0b_1111_1111, 0b_1111_1111 },
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = new DateTime( 2019, 1, 1 ),
                Name = "Testing StreakType"
            };

            var streak = new Streak
            {
                EnrollmentDate = new DateTime( 2019, 1, 1 ),
                PersonAliasId = _personAliasId,
                EngagementMap = new byte[] { 0b_0010_1010, 0b_1010_0100, 0b_1000_0011 }
            };

            streakType.Streaks.Add( streak );
            _streakTypeService.Add( streakType );
            _rockContext.SaveChanges( true );

            _streakTypeId = streakType.Id;
            _streakId = streak.Id;
        }

        /// <summary>
        /// Delete the streak type created by this test class
        /// </summary>
        private static void DeleteTestData()
        {
            var streakTypeGuid = new Guid( StreakTypeGuidString );
            var streakType = _streakTypeService.Queryable().FirstOrDefault( st => st.Guid == streakTypeGuid );

            if ( streakType != null )
            {
                _streakTypeService.Delete( streakType );
            }

            _streakTypeId = default;
            _rockContext.SaveChanges();
        }

        /// <summary>
        /// Creates the achievement type data.
        /// </summary>
        private static void CreateAchievementTypeData()
        {
            var achievementType = new AchievementType
            {
                Name = "Test Achievement",
                IsActive = true,
                ComponentEntityTypeId = EntityTypeCache.GetId( ComponentEntityTypeName ) ?? 0,
                MaxAccomplishmentsAllowed = 2,
                AllowOverAchievement = false,
                ComponentConfigJson = "{\"StreakType\": \"" + StreakTypeGuidString + "\"}"
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
            CreateStreakTypeData();
            CreateAchievementTypeData();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            DeleteTestData();
            _rockContext = null;
            _streakTypeService = null;
        }

        [TestCleanup]
        public void TestCleanup()
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
            var attemptsQuery = new AchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == _achievementTypeId && saa.AchieverEntityId == _personAliasId )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );
            _rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
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
            var attemptsQuery = new AchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == _achievementTypeId && saa.AchieverEntityId == _personAliasId )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            // This attempt should get deleted since it is wrong and after any successful attempt
            var attempt = new AchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 3, 1 ),
                Progress = .25m,
                AchieverEntityId = _personAliasId,
                AchievementTypeId = _achievementTypeId
            };
            var attemptService = new AchievementAttemptService( _rockContext );
            attemptService.Add( attempt );
            Assert.That.AreEqual( 0, attempt.Id );
            _rockContext.SaveChanges();

            // There should now be one attempt
            Assert.That.AreEqual( 1, attemptsQuery.Count() );
            Assert.That.IsTrue( attempt.Id > 0 );

            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );
            _rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
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
            var attemptsQuery = new AchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.AchievementTypeId == _achievementTypeId && saa.AchieverEntityId == _personAliasId )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.That.AreEqual( 0, attemptsQuery.Count() );

            var attempt1 = new AchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 3, 1 ),
                Progress = .25m,
                AchieverEntityId = _personAliasId,
                AchievementTypeId = _achievementTypeId
            };

            var attempt2 = new AchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 4, 1 ),
                AchievementAttemptEndDateTime = new DateTime( 2019, 4, 5 ),
                Progress = 1m,
                AchieverEntityId = _personAliasId,
                AchievementTypeId = _achievementTypeId,
                IsClosed = true,
                IsSuccessful = true
            };

            var attemptService = new AchievementAttemptService( _rockContext );
            attemptService.Add( attempt1 );
            attemptService.Add( attempt2 );
            Assert.That.AreEqual( 0, attempt1.Id );
            Assert.That.AreEqual( 0, attempt2.Id );
            _rockContext.SaveChanges();

            // There should now be two attempts
            Assert.That.AreEqual( 2, attemptsQuery.Count() );
            Assert.That.IsTrue( attempt1.Id > 0 );
            Assert.That.IsTrue( attempt2.Id > 0 );

            var originalId1 = attempt1.Id;
            var originalId2 = attempt2.Id;

            var achievementTypeCache = AchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );
            _rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.That.IsNotNull( attempts );
            Assert.That.AreEqual( 2, attempts.Count );

            Assert.That.AreEqual( attempt1.Id, attempts[0].Id );
            Assert.That.AreEqual( originalId1, attempts[0].Id );
            Assert.That.AreEqual( new DateTime( 2019, 3, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.That.IsNull( attempts[0].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( .25m, attempts[0].Progress );
            Assert.That.IsFalse( attempts[0].IsClosed );
            Assert.That.IsFalse( attempts[0].IsSuccessful );

            Assert.That.AreEqual( attempt2.Id, attempts[1].Id );
            Assert.That.AreEqual( originalId2, attempts[1].Id );
            Assert.That.AreEqual( new DateTime( 2019, 4, 1 ), attempts[1].AchievementAttemptStartDateTime );
            Assert.That.AreEqual( new DateTime( 2019, 4, 5 ), attempts[1].AchievementAttemptEndDateTime );
            Assert.That.AreEqual( 1m, attempts[1].Progress );
            Assert.That.IsTrue( attempts[1].IsClosed );
            Assert.That.IsTrue( attempts[1].IsSuccessful );
        }
    }
}
