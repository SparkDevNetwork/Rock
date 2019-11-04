using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.RockTests.Model
{
    /// <summary>
    /// Tests for StreakTypeService that use the database
    /// </summary>
    [TestClass]
    public class StreakTypeServiceTests
    {
        private const string StreakTypeGuidString = "93050DB0-82FC-4EBE-9AB8-8BB8BADFB2F0";
        private const string EmptyStreakTypeGuidString = "84161DB0-82FC-4EBE-9AB8-8BB8BADFB2A2";

        private static RockContext _rockContext { get; set; }
        private static StreakTypeService _streakTypeService { get; set; }
        private static int _streakTypeId { get; set; }
        private static int _emptyStreakTypeId { get; set; }
        private static int _streakId { get; set; }
        private static int _emptyStreakId { get; set; }
        private static int _personAliasId { get; set; }
        private static int _personId { get; set; }

        /*
         * Occurrences    01110011110011110011110011110011110011110011110011110011
         * Engagements    01001010100011000011010000001111111100101111111111110001
         * Exclusions     01000010000000000000000000000000000000110000000000000000
         * Result          1XX  1X1X  21XX  21X1  XXXX  4321  XX9E  8765  4321  X1
         * Day            54321098765432109876543211098765432109876543210987654321
         * Month                                  2                              1
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
                OccurrenceMap = new byte[] { 0b_0111_0011, 0b_1100_1111, 0b_0011_1100, 0b_1111_0011, 0b_1100_1111, 0b_0011_1100, 0b_1111_0011 },
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = new DateTime( 2019, 1, 1 ),
                Name = "Testing StreakType"
            };

            var streak = new Streak
            {
                PersonAliasId = _personAliasId,
                EngagementMap = new byte[] { 0b_0100_1010, 0b_1000_1100, 0b_0011_0100, 0b_0000_1111, 0b_1111_0010, 0b_1111_1111, 0b_1111_0001 },
                ExclusionMap = new byte[] { 0b_0100_0010, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0011, 0b_0000_0000, 0b_0000_0000 }
            };

            var emptyStreakType = new StreakType
            {
                Guid = new Guid( EmptyStreakTypeGuidString ),
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = RockDateTime.Today,
                Name = "Empty Testing StreakType"
            };

            streakType.Streaks.Add( streak );
            _streakTypeService.Add( streakType );

            var emptyStreak = new Streak
            {
                PersonAliasId = _personAliasId,
            };

            emptyStreakType.Streaks.Add( emptyStreak );
            _streakTypeService.Add( emptyStreakType );

            _rockContext.SaveChanges();

            _streakTypeId = streakType.Id;
            _streakId = streak.Id;
            _emptyStreakTypeId = emptyStreakType.Id;
            _emptyStreakId = emptyStreak.Id;
        }

        /// <summary>
        /// Delete the streak type created by this test class
        /// </summary>
        private static void DeleteStreakTypeData()
        {
            var streakTypeGuid = new Guid( StreakTypeGuidString );
            var emptyStreakTypeGuid = new Guid( EmptyStreakTypeGuidString );
            var guids = new List<Guid>() { streakTypeGuid, emptyStreakTypeGuid };
            var streakTypeQuery = _streakTypeService.Queryable().Where( st => guids.Contains( st.Guid ) );
            _streakTypeService.DeleteRange( streakTypeQuery );

            _rockContext.SaveChanges();

            _streakTypeId = default;
            _streakId = default;

            _emptyStreakTypeId = default;
            _emptyStreakId = default;
        }

        /// <summary>
        /// Runs before any tests in this class are executed.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize( TestContext testContext )
        {
            _rockContext = new RockContext();
            _streakTypeService = new StreakTypeService( _rockContext );
            DeleteStreakTypeData();
            CreateStreakTypeData();
        }

        /// <summary>
        /// Runs after all tests in this class is executed.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            DeleteStreakTypeData();
            _rockContext = null;
            _streakTypeService = null;
        }

        #endregion Setup Methods

        #region GetStreakData

        /// <summary>
        /// Tests GetStreakData
        /// </summary>
        [TestMethod]
        public void GetStreakData()
        {
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 2, 4 );
            var result = _streakTypeService.GetStreakData( StreakTypeCache.Get( _streakTypeId ), _personId, out string errorMessage,
                startDate, endDate, true, true, 100 );

            Assert.AreEqual( string.Empty, errorMessage );
            Assert.IsNotNull( result );

            Assert.AreEqual( 9, result.LongestStreakCount );
            Assert.AreEqual( new DateTime( 2019, 1, 5 ), result.LongestStreakStartDate );
            Assert.AreEqual( new DateTime( 2019, 1, 18 ), result.LongestStreakEndDate );

            Assert.AreEqual( 1, result.CurrentStreakCount );
            Assert.AreEqual( new DateTime( 2019, 2, 4 ), result.CurrentStreakStartDate );

            Assert.AreEqual( 4, result.ComputedStreaks.Count );

            Assert.AreEqual( 1, result.ComputedStreaks[0].Count );
            Assert.AreEqual( new DateTime( 2019, 1, 1 ), result.ComputedStreaks[0].StartDate );
            Assert.AreEqual( new DateTime( 2019, 1, 1 ), result.ComputedStreaks[0].EndDate );

            Assert.AreEqual( 9, result.ComputedStreaks[1].Count );
            Assert.AreEqual( new DateTime( 2019, 1, 5 ), result.ComputedStreaks[1].StartDate );
            Assert.AreEqual( new DateTime( 2019, 1, 18 ), result.ComputedStreaks[1].EndDate );

            Assert.AreEqual( 4, result.ComputedStreaks[2].Count );
            Assert.AreEqual( new DateTime( 2019, 1, 23 ), result.ComputedStreaks[2].StartDate );
            Assert.AreEqual( new DateTime( 2019, 1, 26 ), result.ComputedStreaks[2].EndDate );

            Assert.AreEqual( 1, result.ComputedStreaks[3].Count );
            Assert.AreEqual( new DateTime( 2019, 2, 4 ), result.ComputedStreaks[3].StartDate );
            Assert.IsNull( result.ComputedStreaks[3].EndDate );

            Assert.AreEqual( 0, result.EngagementsThisMonth );
            Assert.AreEqual( RockDateTime.Now.Year == 2019 ? 22 : 0, result.EngagementsThisYear );
            Assert.AreEqual( new DateTime( 2019, 2, 24 ), result.MostRecentEngagementDate );
            Assert.AreEqual( new DateTime( 2019, 2, 24 ), result.MostRecentOccurrenceDate );
            Assert.IsTrue( result.EngagedAtMostRecentOccurrence );
        }

        /// <summary>
        /// Tests GetStreakData with empty maps and today as a map start date
        /// </summary>
        [TestMethod]
        public void GetStreakDataWithEmptyMaps()
        {
            var startDate = RockDateTime.Today;
            var endDate = RockDateTime.Today;
            var result = _streakTypeService.GetStreakData( StreakTypeCache.Get( _emptyStreakTypeId ), _personId, out string errorMessage,
                startDate, endDate, true, true, 100 );

            Assert.AreEqual( string.Empty, errorMessage );
            Assert.IsNotNull( result );

            Assert.AreEqual( 0, result.LongestStreakCount );
            Assert.IsNull( result.LongestStreakStartDate );
            Assert.IsNull( result.LongestStreakEndDate );

            Assert.AreEqual( 0, result.CurrentStreakCount );
            Assert.IsNull( result.CurrentStreakStartDate );

            Assert.AreEqual( 0, result.ComputedStreaks.Count );

            Assert.AreEqual( 0, result.EngagementsThisMonth );
            Assert.AreEqual( 0, result.EngagementsThisYear );
            Assert.IsNull( result.MostRecentEngagementDate );
            Assert.IsNull( result.MostRecentOccurrenceDate );
            Assert.IsFalse( result.EngagedAtMostRecentOccurrence );
        }

        #endregion GetStreakData
    }
}
