using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Modules.Core.Model
{
    /// <summary>
    /// Tests for StreakTypeService that use the database
    /// </summary>
    [TestClass]
    public class StreakTypeServiceTests : DatabaseTestsBase
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
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();
            var personAlias = new PersonAliasService( _rockContext ).Queryable().First( pa => pa.Person.Guid == tedDeckerGuid );
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

            Assert.That.AreEqual( string.Empty, errorMessage );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( 9, result.LongestStreakCount );
            Assert.That.AreEqual( new DateTime( 2019, 1, 5 ), result.LongestStreakStartDate );
            Assert.That.AreEqual( new DateTime( 2019, 1, 18 ), result.LongestStreakEndDate );

            Assert.That.AreEqual( 1, result.CurrentStreakCount );
            Assert.That.AreEqual( new DateTime( 2019, 2, 4 ), result.CurrentStreakStartDate );

            Assert.That.AreEqual( 4, result.ComputedStreaks.Count );

            Assert.That.AreEqual( 1, result.ComputedStreaks[0].Count );
            Assert.That.AreEqual( new DateTime( 2019, 1, 1 ), result.ComputedStreaks[0].StartDate );
            Assert.That.AreEqual( new DateTime( 2019, 1, 1 ), result.ComputedStreaks[0].EndDate );

            Assert.That.AreEqual( 9, result.ComputedStreaks[1].Count );
            Assert.That.AreEqual( new DateTime( 2019, 1, 5 ), result.ComputedStreaks[1].StartDate );
            Assert.That.AreEqual( new DateTime( 2019, 1, 18 ), result.ComputedStreaks[1].EndDate );

            Assert.That.AreEqual( 4, result.ComputedStreaks[2].Count );
            Assert.That.AreEqual( new DateTime( 2019, 1, 23 ), result.ComputedStreaks[2].StartDate );
            Assert.That.AreEqual( new DateTime( 2019, 1, 26 ), result.ComputedStreaks[2].EndDate );

            Assert.That.AreEqual( 1, result.ComputedStreaks[3].Count );
            Assert.That.AreEqual( new DateTime( 2019, 2, 4 ), result.ComputedStreaks[3].StartDate );
            Assert.That.IsNull( result.ComputedStreaks[3].EndDate );

            Assert.That.AreEqual( 0, result.EngagementsThisMonth );
            Assert.That.AreEqual( RockDateTime.Now.Year == 2019 ? 22 : 0, result.EngagementsThisYear );
            Assert.That.AreEqual( new DateTime( 2019, 2, 24 ), result.MostRecentEngagementDate );
            Assert.That.AreEqual( new DateTime( 2019, 2, 24 ), result.MostRecentOccurrenceDate );
            Assert.That.IsTrue( result.EngagedAtMostRecentOccurrence );
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

            Assert.That.AreEqual( string.Empty, errorMessage );
            Assert.That.IsNotNull( result );

            Assert.That.AreEqual( 0, result.LongestStreakCount );
            Assert.That.IsNull( result.LongestStreakStartDate );
            Assert.That.IsNull( result.LongestStreakEndDate );

            Assert.That.AreEqual( 0, result.CurrentStreakCount );
            Assert.That.IsNull( result.CurrentStreakStartDate );

            Assert.That.AreEqual( 0, result.ComputedStreaks.Count );

            Assert.That.AreEqual( 0, result.EngagementsThisMonth );
            Assert.That.AreEqual( 0, result.EngagementsThisYear );
            Assert.That.IsNull( result.MostRecentEngagementDate );
            Assert.That.IsNull( result.MostRecentOccurrenceDate );
            Assert.That.IsFalse( result.EngagedAtMostRecentOccurrence );
        }

        #endregion GetStreakData

        #region GetFrequencyUnitDifference

        /// <summary>
        /// Calculating the difference in daily dates inclusively works correctly
        /// </summary>
        [TestMethod]
        public void GetFrequencyUnitDifferenceInclusiveDaily()
        {
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily
            } );

            var isInclusive = true;

            // Month of January is 31 days long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 31, result );

            // Year of 2019 is 365 days long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 365, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( -2, result );

            // Same day calculation is 1 day because of inclusiveness
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 1, result );
        }

        /// <summary>
        /// Calculating the difference in daily dates exclusively works correctly
        /// </summary>
        [TestMethod]
        public void GetFrequencyUnitDifferenceExclusiveDaily()
        {
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily
            } );

            var isInclusive = false;

            // Month of January is 31 days long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 30, result );

            // Year of 2019 is 365 days long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 364, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( -1, result );

            // Same day calculation is 0
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 0, result );
        }

        /// <summary>
        /// Calculating the difference in daily dates exclusively works correctly
        /// </summary>
        [TestMethod]
        public void GetFrequencyUnitDifferenceExclusiveWeekly()
        {
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Weekly
            } );

            var isInclusive = false;

            // Month of January is 4 weeks long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 4, result );

            // Year of 2019 is 52 weeks long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 52, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 26 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( -1, result );

            // Same week calculation is 0
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 0, result );
        }

        /// <summary>
        /// Calculating the difference in weekly dates inclusively works correctly
        /// </summary>
        [TestMethod]
        public void GetFrequencyUnitDifferenceInclusiveWeekly()
        {
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Weekly
            } );

            var isInclusive = true;

            // Month of January is 4 weeks long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 5, result );

            // Year of 2019 is 52 weeks long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 53, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 26 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( -2, result );

            // Same day calculation is 1 day because of inclusiveness
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, streakTypeCache, isInclusive );
            Assert.That.AreEqual( 1, result );
        }

        #endregion GetFrequencyUnitDifference

        #region IsBitSet

        /// <summary>
        /// Checks if bits are set in the byte map that is daily occurrences
        /// </summary>
        [TestMethod]
        public void IsBitSetIsCorrectForDailyMap()
        {
            var mapStartDate = new DateTime( 2019, 1, 1 );
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = mapStartDate
            } );

            // Day Offset             3210 9876     5432 1098     7654 3210
            var map = new byte[] { 0b_1000_0000, 0b_0010_0000, 0b_0000_0100 };

            for ( var dayOffset = -5; dayOffset < 100; dayOffset++ )
            {
                var date = mapStartDate.AddDays( dayOffset );
                var isSet = StreakTypeService.IsBitSet( streakTypeCache, map, date, out var errorMessage );

                if ( dayOffset < 0 )
                {
                    // Should get error about checking a bit that is pre-start-date
                    Assert.That.IsFalse( errorMessage.IsNullOrWhiteSpace() );
                }
                else
                {
                    Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() );

                    // The day of the month is the offset + 1 since offset 0 is Jan 1, 2019
                    if ( date.Year == 2019 && date.Month == 1 && ( date.Day == 3 || date.Day == 14 || date.Day == 24 ) )
                    {
                        Assert.That.IsTrue( isSet );
                    }
                    else
                    {
                        Assert.That.IsFalse( isSet );
                    }
                }
            }
        }

        /// <summary>
        /// Checks if bits are set in the byte map that is weekly occurrences
        /// </summary>
        [TestMethod]
        [Ignore("Fix needed. This test appears to be failing due to changes in how the first day of the week is specified.")]
        public void IsBitSetIsCorrectForWeeklyMap()
        {
            var startDate = new DateTime( 2019, 1, 6 );
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Weekly,
                StartDate = startDate
            } );

            // Week Offset            3210 9876     5432 1098     7654 3210
            var map = new byte[] { 0b_1000_0000, 0b_0010_0000, 0b_0000_0100 };

            for ( var dayOffset = -5; dayOffset < 250; dayOffset++ )
            {
                var date = startDate.AddDays( dayOffset );
                var isSet = StreakTypeService.IsBitSet( streakTypeCache, map, date, out var errorMessage );

                if ( dayOffset < 0 )
                {
                    // Should get error about checking a bit that is pre-start-date
                    Assert.That.IsFalse( errorMessage.IsNullOrWhiteSpace() );
                }
                else
                {
                    Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() );

                    // Bit index 2 is week of Jan 14-20
                    if ( date.Year == 2019 && date.Month == 1 && date.Day >= 14 && date.Day <= 20 )
                    {
                        Assert.That.IsTrue( isSet );
                    }
                    // Bit index 13 is week of Apr 1-7
                    else if ( date.Year == 2019 && date.Month == 4 && date.Day >= 1 && date.Day <= 7 )
                    {
                        Assert.That.IsTrue( isSet );
                    }
                    // Bit index 23 is week of Jun 10-16
                    else if ( date.Year == 2019 && date.Month == 6 && date.Day >= 10 && date.Day <= 16 )
                    {
                        Assert.That.IsTrue( isSet );
                    }
                    else
                    {
                        Assert.That.IsFalse( isSet );
                    }
                }
            }
        }

        #endregion IsBitSet

        #region SetBit

        /// <summary>
        /// Resetting bits works correctly for daily maps
        /// </summary>
        [TestMethod]
        public void ResetBitWorksForDailyMap()
        {
            var startDate = new DateTime( 2019, 1, 1 );
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = startDate
            } );

            // Offset             7654 3210
            const byte byte2 = 0b_0000_0100;

            // Offset             5432 1098
            const byte byte1 = 0b_0010_0000;

            // Offset             3210 9876
            const byte byte0 = 0b_1000_0000;

            var map = new byte[] { byte0, byte1, byte2 };

            // Reset a bit before the start date and get an error
            var result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( -1 ), false, out var errorMessage );
            Assert.That.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset a bit that is already 0
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 0 ), false, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset the first set bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 2 ), false, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( 0, result[2] ); // Verify change
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes

            // Reset a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 24 ), false, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.That.IsTrue( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.That.AreEqual( 0, result[newLength - 1] ); // Verify no additional changes
            Assert.That.AreEqual( byte1, result[newLength - 2] ); // Verify no changes
            Assert.That.AreEqual( byte0, result[newLength - 3] ); // Verify no changes

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 3 ); i++ )
            {
                Assert.That.AreEqual( 0, result[i] );
            }
        }

        /// <summary>
        /// Resetting bits works correctly for weekly maps
        /// </summary>
        [TestMethod]
        public void ResetBitWorksForWeeklyMap()
        {
            var startDate = new DateTime( 2019, 1, 6 );
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Weekly,
                StartDate = startDate
            } );

            // Offset             7654 3210
            const byte byte2 = 0b_0000_0100;

            // Offset             5432 1098
            const byte byte1 = 0b_0010_0000;

            // Offset             3210 9876
            const byte byte0 = 0b_1000_0000;

            var map = new byte[] { byte0, byte1, byte2 };
            var valueForReset = false;

            // Reset a bit before the start date and get an error
            var result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( -1 * 7 ), valueForReset, out var errorMessage );
            Assert.That.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset a bit that is already 0
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 0 * 7 ), valueForReset, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset the first set bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 2 * 7 ), valueForReset, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( 0, result[2] ); // Verify change
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes

            // Reset a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 24 * 7 ), valueForReset, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.That.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.That.IsTrue( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.That.AreEqual( 0, result[newLength - 1] ); // Verify no additional changes
            Assert.That.AreEqual( byte1, result[newLength - 2] ); // Verify no changes
            Assert.That.AreEqual( byte0, result[newLength - 3] ); // Verify no changes

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 3 ); i++ )
            {
                Assert.That.AreEqual( 0, result[i] );
            }
        }

        /// <summary>
        /// Setting bits works correctly for daily maps
        /// </summary>
        [TestMethod]
        public void SetBitWorksForDailyMap()
        {
            var startDate = new DateTime( 2019, 1, 1 );
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = startDate
            } );

            const byte lsb = 0b_0000_0001; // Least significant bit
            const byte msb = 0b_1000_0000; // Most significant bit

            // Offset             7654 3210
            const byte byte2 = 0b_0000_0100;

            // Offset             5432 1098
            const byte byte1 = 0b_0010_0000;

            // Offset             3210 9876
            const byte byte0 = 0b_1000_0000;

            var map = new byte[] { byte0, byte1, byte2 };

            // Set a bit before the start date and get an error
            var result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( -1 ), true, out var errorMessage );
            Assert.That.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte2, result[2] ); // Verify no changes

            // Set a bit that is already set
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 2 ), true, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte2, result[2] ); // Verify no changes

            // Set the least significant bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 0 ), true, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte2 | lsb, result[2] ); // Verify change
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes

            // Set the most significant bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 23 ), true, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte2 | lsb, result[2] ); // Verify no additional changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte0 | msb, result[0] ); // Verify change

            // Set a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 24 ), true, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.That.IsTrue( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.That.AreEqual( byte2 | lsb, result[newLength - 1] ); // Verify no additional changes
            Assert.That.AreEqual( byte1, result[newLength - 2] ); // Verify no changes
            Assert.That.AreEqual( byte0 | msb, result[newLength - 3] ); // Verify no additional changes
            Assert.That.AreEqual( lsb, result[newLength - 4] ); // Verify first bit in first new byte is set

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 4 ); i++ )
            {
                Assert.That.AreEqual( 0, result[i] );
            }
        }

        /// <summary>
        /// Setting bits works correctly for weekly maps
        /// </summary>
        [TestMethod]
        public void SetBitWorksForWeeklyMap()
        {
            var startDate = new DateTime( 2019, 1, 6 );
            var streakTypeCache = new StreakTypeCache();
            streakTypeCache.SetFromEntity( new StreakType
            {
                OccurrenceFrequency = StreakOccurrenceFrequency.Weekly,
                StartDate = startDate
            } );

            const byte lsb = 0b_0000_0001; // Least significant bit
            const byte msb = 0b_1000_0000; // Most significant bit

            // Offset             7654 3210
            const byte byte2 = 0b_0000_0100;

            // Offset             5432 1098
            const byte byte1 = 0b_0010_0000;

            // Offset             3210 9876
            const byte byte0 = 0b_1000_0000;

            var map = new byte[] { byte0, byte1, byte2 };

            // Set a bit before the start date and get an error
            var result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( -1 * 7 ), true, out var errorMessage );
            Assert.That.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte2, result[2] ); // Verify no changes

            // Set a bit that is already set
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 2 * 7 ), true, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte2, result[2] ); // Verify no changes

            // Set the least significant bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 0 * 7 ), true, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte2 | lsb, result[2] ); // Verify change
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte0, result[0] ); // Verify no changes

            // Set the most significant bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 23 * 7 ), true, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.That.AreSame( result, map ); // Verify in-place operation
            Assert.That.AreEqual( byte2 | lsb, result[2] ); // Verify no additional changes
            Assert.That.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.That.AreEqual( byte0 | msb, result[0] ); // Verify change

            // Set a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 24 * 7 ), true, out errorMessage );
            Assert.That.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.That.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.That.IsTrue( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.That.AreEqual( byte2 | lsb, result[newLength - 1] ); // Verify no additional changes
            Assert.That.AreEqual( byte1, result[newLength - 2] ); // Verify no changes
            Assert.That.AreEqual( byte0 | msb, result[newLength - 3] ); // Verify no additional changes
            Assert.That.AreEqual( lsb, result[newLength - 4] ); // Verify first bit in first new byte is set

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 4 ); i++ )
            {
                Assert.That.AreEqual( 0, result[i] );
            }
        }

        #endregion SetBit
    }
}
