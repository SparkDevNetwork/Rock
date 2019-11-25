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

        #region Weekly Frequency Tests

        // Since these tests use SundayDate to calculate a date to identify the weeks by, these tests have to be here in the integration
        // test project. This is because SundayDate loads attributes to know what the configured first day of the week is.

        /// <summary>
        /// Checks if bits are set in the byte map that is weekly occurrences
        /// </summary>
        [TestMethod]
        public void IsBitSetIsCorrectForWeeklyMap()
        {
            // Week Offset            3210 9876     5432 1098     7654 3210
            var map = new byte[] { 0b_1000_0000, 0b_0010_0000, 0b_0000_0100 };
            var mapStartDate = new DateTime( 2019, 1, 6 );

            for ( var dayOffset = -5; dayOffset < 250; dayOffset++ )
            {
                var date = mapStartDate.AddDays( dayOffset );
                var isSet = StreakTypeService.IsBitSet( map, mapStartDate, date, StreakOccurrenceFrequency.Weekly, out var errorMessage );

                if ( dayOffset < 0 )
                {
                    // Should get error about checking a bit that is pre-start-date
                    Assert.IsFalse( errorMessage.IsNullOrWhiteSpace() );
                }
                else
                {
                    Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() );

                    // Bit index 2 is week of Jan 14-20
                    if ( date.Year == 2019 && date.Month == 1 && date.Day >= 14 && date.Day <= 20 )
                    {
                        Assert.IsTrue( isSet );
                    }
                    // Bit index 13 is week of Apr 1-7
                    else if ( date.Year == 2019 && date.Month == 4 && date.Day >= 1 && date.Day <= 7 )
                    {
                        Assert.IsTrue( isSet );
                    }
                    // Bit index 23 is week of Jun 10-16
                    else if ( date.Year == 2019 && date.Month == 6 && date.Day >= 10 && date.Day <= 16 )
                    {
                        Assert.IsTrue( isSet );
                    }
                    else
                    {
                        Assert.IsFalse( isSet );
                    }
                }
            }
        }

        /// <summary>
        /// Setting bits works correctly for weekly maps
        /// </summary>
        [TestMethod]
        public void SetBitWorksForWeeklyMap()
        {
            const byte lsb = 0b_0000_0001; // Least significant bit
            const byte msb = 0b_1000_0000; // Most significant bit

            // Offset             7654 3210
            const byte byte2 = 0b_0000_0100;

            // Offset             5432 1098
            const byte byte1 = 0b_0010_0000;

            // Offset             3210 9876
            const byte byte0 = 0b_1000_0000;

            var map = new byte[] { byte0, byte1, byte2 };
            var startDate = new DateTime( 2019, 1, 6 );
            var frequency = StreakOccurrenceFrequency.Weekly;

            // Set a bit before the start date and get an error
            var result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( -1 * 7 ), frequency, true, out var errorMessage );
            Assert.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Set a bit that is already set
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 2 * 7 ), frequency, true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Set the least significant bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 0 * 7 ), frequency, true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte2 | lsb, result[2] ); // Verify change
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0, result[0] ); // Verify no changes

            // Set the most significant bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 23 * 7 ), frequency, true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte2 | lsb, result[2] ); // Verify no additional changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0 | msb, result[0] ); // Verify change

            // Set a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 24 * 7 ), frequency, true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.IsTrue( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.AreEqual( byte2 | lsb, result[newLength - 1] ); // Verify no additional changes
            Assert.AreEqual( byte1, result[newLength - 2] ); // Verify no changes
            Assert.AreEqual( byte0 | msb, result[newLength - 3] ); // Verify no additional changes
            Assert.AreEqual( lsb, result[newLength - 4] ); // Verify first bit in first new byte is set

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 4 ); i++ )
            {
                Assert.AreEqual( 0, result[i] );
            }
        }

        /// <summary>
        /// Resetting bits works correctly for weekly maps
        /// </summary>
        [TestMethod]
        public void ResetBitWorksForWeeklyMap()
        {
            // Offset             7654 3210
            const byte byte2 = 0b_0000_0100;

            // Offset             5432 1098
            const byte byte1 = 0b_0010_0000;

            // Offset             3210 9876
            const byte byte0 = 0b_1000_0000;

            var map = new byte[] { byte0, byte1, byte2 };
            var startDate = new DateTime( 2019, 1, 6 );
            var frequency = StreakOccurrenceFrequency.Weekly;
            var valueForReset = false;

            // Reset a bit before the start date and get an error
            var result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( -1 * 7 ), frequency, valueForReset, out var errorMessage );
            Assert.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset a bit that is already 0
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 0 * 7 ), frequency, valueForReset, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset the first set bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 2 * 7 ), frequency, valueForReset, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( 0, result[2] ); // Verify change
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0, result[0] ); // Verify no changes

            // Reset a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 24 * 7 ), frequency, valueForReset, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.IsTrue( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.AreEqual( 0, result[newLength - 1] ); // Verify no additional changes
            Assert.AreEqual( byte1, result[newLength - 2] ); // Verify no changes
            Assert.AreEqual( byte0, result[newLength - 3] ); // Verify no changes

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 3 ); i++ )
            {
                Assert.AreEqual( 0, result[i] );
            }
        }

        /// <summary>
        /// Calculating the difference in weekly dates inclusively works correctly
        /// </summary>
        [TestMethod]
        public void GetFrequencyUnitDifferenceInclusiveWeekly()
        {
            var frequency = StreakOccurrenceFrequency.Weekly;
            var isInclusive = true;

            // Month of January is 4 weeks long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.AreEqual( 5, result );

            // Year of 2019 is 52 weeks long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.AreEqual( 53, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 26 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.AreEqual( -2, result );

            // Same day calculation is 1 day because of inclusiveness
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, frequency, isInclusive );
            Assert.AreEqual( 1, result );
        }

        /// <summary>
        /// Calculating the difference in daily dates exclusively works correctly
        /// </summary>
        [TestMethod]
        public void GetFrequencyUnitDifferenceExclusiveWeekly()
        {
            var frequency = StreakOccurrenceFrequency.Weekly;
            var isInclusive = false;

            // Month of January is 4 weeks long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.AreEqual( 4, result );

            // Year of 2019 is 52 weeks long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.AreEqual( 52, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 26 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.AreEqual( -1, result );

            // Same week calculation is 0
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, frequency, isInclusive );
            Assert.AreEqual( 0, result );
        }

        #endregion Weekly Frequency Tests
    }
}
