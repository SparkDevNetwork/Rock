using System;
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

        private static RockContext _rockContext { get; set; }
        private static StreakTypeService _streakTypeService { get; set; }
        private static int _streakTypeId { get; set; }
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

            streakType.Streaks.Add( streak );
            _streakTypeService.Add( streakType );
            _rockContext.SaveChanges();
            _streakTypeId = streakType.Id;
        }

        /// <summary>
        /// Delete the streak type created by this test class
        /// </summary>
        private static void DeleteStreakTypeData()
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
        /// Tests GetRecentEngagementBits
        /// </summary>
        [TestMethod]
        public void GetRecentEngagementBits()
        {
            var unitCount = 5;
            var result = _streakTypeService.GetRecentEngagementBits( _streakTypeId, _personId, unitCount, out var errorMessage );

            Assert.AreEqual( string.Empty, errorMessage );
            Assert.IsNotNull( result );
            Assert.AreEqual( unitCount, result.Length );

            Assert.AreEqual( new DateTime( 2019, 2, 24 ), result[0].DateTime );
            Assert.IsTrue( result[0].HasEngagement );
            Assert.IsTrue( result[0].HasExclusion );

            Assert.AreEqual( new DateTime( 2019, 2, 23 ), result[1].DateTime );
            Assert.IsFalse( result[1].HasEngagement );
            Assert.IsFalse( result[1].HasExclusion );

            Assert.AreEqual( new DateTime( 2019, 2, 22 ), result[2].DateTime );
            Assert.IsFalse( result[2].HasEngagement );
            Assert.IsFalse( result[2].HasExclusion );

            Assert.AreEqual( new DateTime( 2019, 2, 19 ), result[3].DateTime );
            Assert.IsTrue( result[3].HasEngagement );
            Assert.IsTrue( result[3].HasExclusion );

            Assert.AreEqual( new DateTime( 2019, 2, 18 ), result[4].DateTime );
            Assert.IsFalse( result[4].HasEngagement );
            Assert.IsFalse( result[4].HasExclusion );
        }
    }
}
