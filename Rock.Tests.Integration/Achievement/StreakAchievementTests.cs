﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Achievement;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.RockTests.Model
{
    /// <summary>
    /// Tests for Streak Achievements that use the database
    /// </summary>
    [TestClass]
    public class StreakAchievementTests
    {
        private const string ComponentEntityTypeName = "Rock.Achievement.Component.StreakAchievement";
        private const string StreakTypeGuidString = "93050DB0-82FC-4EBE-9AB8-8BB8BADFB2F0";

        private static RockContext _rockContext { get; set; }
        private static StreakTypeService _streakTypeService { get; set; }
        private static StreakTypeAchievementTypeService _achievementTypeService { get; set; }

        private static int _streakTypeId { get; set; }
        private static int _streakId { get; set; }
        private static int _personAliasId { get; set; }
        private static int _personId { get; set; }

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
            var personAlias = new PersonAliasService( _rockContext ).Queryable().First( pa => pa.Person.Guid == TestPeople.TedDeckerPersonGuid );
            _personAliasId = personAlias.Id;
            _personId = personAlias.PersonId;

            var streakType = new StreakType
            {
                Guid = new Guid( StreakTypeGuidString ),
                OccurrenceMap = new byte[] { 0b_0010_0111, 0b_1111_1110, 0b_0001_1111, 0b_1000_0111 },
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = new DateTime( 2019, 1, 1 ),
                Name = "Testing StreakType"
            };

            var streak = new Streak
            {
                EnrollmentDate = new DateTime( 2019, 1, 1 ),
                PersonAliasId = _personAliasId,
                EngagementMap = new byte[] { 0b_0010_0010, 0b_1011_1110, 0b_0001_1001, 0b_1000_0011 },
                ExclusionMap = new byte[] { 0b_0000_0010, 0b_0000_0100 }
            };

            var exclusion = new StreakTypeExclusion
            {
                ExclusionMap = new byte[] { 0b_0000_0001, 0b_0100_0000, 0b_0000_0000, 0b_0000_0000 }
            };

            streakType.StreakTypeExclusions.Add( exclusion );
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
            var achievementType = new StreakTypeAchievementType
            {
                Name = "Test Achievement",
                IsActive = true,
                StreakTypeId = _streakTypeId,
                AchievementEntityTypeId = EntityTypeCache.GetId( ComponentEntityTypeName ) ?? 0,
                MaxAccomplishmentsAllowed = 2,
                AllowOverAchievement = false
            };

            _achievementTypeService.Add( achievementType );
            _rockContext.SaveChanges( true );

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
            _rockContext = new RockContext();
            _streakTypeService = new StreakTypeService( _rockContext );
            _achievementTypeService = new StreakTypeAchievementTypeService( _rockContext );

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
            var service = new StreakAchievementAttemptService( _rockContext );
            service.DeleteRange( service.Queryable().Where( saa => saa.StreakTypeAchievementTypeId == _achievementTypeId ) );
            _rockContext.SaveChanges();
        }

        #endregion Setup Methods

        /// <summary>
        /// Tests StreakAchievementProcess
        /// </summary>
        [TestMethod]
        public void StreakAchievementProcess()
        {
            var attemptsQuery = new StreakAchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.StreakTypeAchievementTypeId == _achievementTypeId && saa.StreakId == _streakId )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.AreEqual( 0, attemptsQuery.Count() );

            var achievementTypeCache = StreakTypeAchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );
            _rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.IsNotNull( attempts );
            Assert.AreEqual( 7, attempts.Count );

            Assert.AreEqual( new DateTime( 2019, 1, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 2 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.AreEqual( .5m, attempts[0].Progress );
            Assert.IsTrue( attempts[0].IsClosed );
            Assert.IsFalse( attempts[0].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 8 ), attempts[1].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 9 ), attempts[1].AchievementAttemptEndDateTime );
            Assert.AreEqual( .5m, attempts[1].Progress );
            Assert.IsTrue( attempts[1].IsClosed );
            Assert.IsFalse( attempts[1].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 12 ), attempts[2].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 18 ), attempts[2].AchievementAttemptEndDateTime );
            Assert.AreEqual( .75m, attempts[2].Progress );
            Assert.IsTrue( attempts[2].IsClosed );
            Assert.IsFalse( attempts[2].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 13 ), attempts[3].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 19 ), attempts[3].AchievementAttemptEndDateTime );
            Assert.AreEqual( .75m, attempts[3].Progress );
            Assert.IsTrue( attempts[3].IsClosed );
            Assert.IsFalse( attempts[3].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 18 ), attempts[4].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 21 ), attempts[4].AchievementAttemptEndDateTime );
            Assert.AreEqual( 1m, attempts[4].Progress );
            Assert.IsTrue( attempts[4].IsClosed );
            Assert.IsTrue( attempts[4].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 22 ), attempts[5].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 26 ), attempts[5].AchievementAttemptEndDateTime );
            Assert.AreEqual( .75m, attempts[5].Progress );
            Assert.IsTrue( attempts[5].IsClosed );
            Assert.IsFalse( attempts[5].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 30 ), attempts[6].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 30 ), attempts[6].AchievementAttemptEndDateTime );
            Assert.AreEqual( .25m, attempts[6].Progress );
            Assert.IsTrue( attempts[6].IsClosed );
            Assert.IsFalse( attempts[6].IsSuccessful );
        }

        /// <summary>
        /// Tests StreakAchievementProcess with open attempt
        /// </summary>
        [TestMethod]
        public void StreakAchievementProcessWithOpenAttempt()
        {
            var attemptsQuery = new StreakAchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.StreakTypeAchievementTypeId == _achievementTypeId && saa.StreakId == _streakId )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.AreEqual( 0, attemptsQuery.Count() );

            var attempt = new StreakAchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 1, 1 ),
                AchievementAttemptEndDateTime = new DateTime( 2019, 1, 2 ),
                Progress = .5m,
                StreakId = _streakId,
                StreakTypeAchievementTypeId = _achievementTypeId,
            };
            var attemptService = new StreakAchievementAttemptService( _rockContext );
            attemptService.Add( attempt );
            Assert.AreEqual( 0, attempt.Id );
            _rockContext.SaveChanges();

            // There should now be one attempt
            Assert.AreEqual( 1, attemptsQuery.Count() );
            Assert.IsTrue( attempt.Id > 0 );

            var achievementTypeCache = StreakTypeAchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );
            _rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.IsNotNull( attempts );
            Assert.AreEqual( 7, attempts.Count );

            Assert.AreEqual( attempt.Id, attempts[0].Id );
            Assert.AreEqual( new DateTime( 2019, 1, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 2 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.AreEqual( .5m, attempts[0].Progress );
            Assert.IsTrue( attempts[0].IsClosed );
            Assert.IsFalse( attempts[0].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 8 ), attempts[1].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 9 ), attempts[1].AchievementAttemptEndDateTime );
            Assert.AreEqual( .5m, attempts[1].Progress );
            Assert.IsTrue( attempts[1].IsClosed );
            Assert.IsFalse( attempts[1].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 12 ), attempts[2].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 18 ), attempts[2].AchievementAttemptEndDateTime );
            Assert.AreEqual( .75m, attempts[2].Progress );
            Assert.IsTrue( attempts[2].IsClosed );
            Assert.IsFalse( attempts[2].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 13 ), attempts[3].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 19 ), attempts[3].AchievementAttemptEndDateTime );
            Assert.AreEqual( .75m, attempts[3].Progress );
            Assert.IsTrue( attempts[3].IsClosed );
            Assert.IsFalse( attempts[3].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 18 ), attempts[4].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 21 ), attempts[4].AchievementAttemptEndDateTime );
            Assert.AreEqual( 1m, attempts[4].Progress );
            Assert.IsTrue( attempts[4].IsClosed );
            Assert.IsTrue( attempts[4].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 22 ), attempts[5].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 26 ), attempts[5].AchievementAttemptEndDateTime );
            Assert.AreEqual( .75m, attempts[5].Progress );
            Assert.IsTrue( attempts[5].IsClosed );
            Assert.IsFalse( attempts[5].IsSuccessful );

            Assert.AreEqual( new DateTime( 2019, 1, 30 ), attempts[6].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 1, 30 ), attempts[6].AchievementAttemptEndDateTime );
            Assert.AreEqual( .25m, attempts[6].Progress );
            Assert.IsTrue( attempts[6].IsClosed );
            Assert.IsFalse( attempts[6].IsSuccessful );
        }

        /// <summary>
        /// Tests StreakAchievementProcess with open attempt that doesn't make sense with the data and it should be closed. 
        /// </summary>
        [TestMethod]
        public void StreakAchievementProcessWithSuccessfulAttempt()
        {
            var attemptsQuery = new StreakAchievementAttemptService( _rockContext ).Queryable()
                .AsNoTracking()
                .Where( saa => saa.StreakTypeAchievementTypeId == _achievementTypeId && saa.StreakId == _streakId )
                .OrderBy( saa => saa.AchievementAttemptStartDateTime );

            // There should be no attempts
            Assert.AreEqual( 0, attemptsQuery.Count() );

            var attempt = new StreakAchievementAttempt
            {
                AchievementAttemptStartDateTime = new DateTime( 2019, 2, 1 ),
                AchievementAttemptEndDateTime = new DateTime( 2019, 2, 1 ),
                Progress = .5m,
                StreakId = _streakId,
                StreakTypeAchievementTypeId = _achievementTypeId,
                IsClosed = true,
                IsSuccessful = true
            };
            var attemptService = new StreakAchievementAttemptService( _rockContext );
            attemptService.Add( attempt );
            Assert.AreEqual( 0, attempt.Id );
            _rockContext.SaveChanges();

            // There should now be one attempt
            Assert.AreEqual( 1, attemptsQuery.Count() );
            Assert.IsTrue( attempt.Id > 0 );

            var achievementTypeCache = StreakTypeAchievementTypeCache.Get( _achievementTypeId );
            var streak = new StreakService( _rockContext ).Get( _streakId );
            var component = AchievementContainer.GetComponent( ComponentEntityTypeName );

            component.Process( _rockContext, achievementTypeCache, streak );
            _rockContext.SaveChanges();

            var attempts = attemptsQuery.ToList();
            Assert.IsNotNull( attempts );
            Assert.AreEqual( 1, attempts.Count );

            Assert.AreEqual( attempt.Id, attempts[0].Id );
            Assert.AreEqual( new DateTime( 2019, 2, 1 ), attempts[0].AchievementAttemptStartDateTime );
            Assert.AreEqual( new DateTime( 2019, 2, 1 ), attempts[0].AchievementAttemptEndDateTime );
            Assert.AreEqual( 0.5m, attempts[0].Progress );
            Assert.IsTrue( attempts[0].IsClosed );
            Assert.IsTrue( attempts[0].IsSuccessful );
        }
    }
}
