using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Tests for the Streak Type Service methods
    /// </summary>
    [TestClass]
    public class StreakTypeServiceTests
    {
        /// <summary>
        /// The scoped RockApp providing the DI container and mocked context.
        /// StreakTypeCache.SetFromEntity resolves the entity type through
        /// EntityTypeCache (which uses RockApp.Current), so a scope must be active.
        /// </summary>
        private TestHelper.RockAppScope _rockAppScope;

        [TestInitialize]
        public void TestInitialize()
        {
            _rockAppScope = TestHelper.CreateScopedRockApp();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _rockAppScope?.Dispose();
        }

        #region GetMapFromHexDigitString

        /// <summary>
        /// Getting a map works correctly
        /// </summary>
        [TestMethod]
        public void GetMapFromHexDigitStringWorksCorrectly()
        {
            var expectedBytes = new byte[] { 0x_ab, 0x_cd, 0x_ef, 0x_01, 0x_23, 0x_45, 0x_67, 0x_89 };
            var hexString = "ABCDEF0123456789";

            var result = StreakTypeService.GetMapFromHexDigitString( hexString );
            Assert.HasCount( expectedBytes.Length, result );

            for ( var i = 0; i < expectedBytes.Length; i++ )
            {
                Assert.AreEqual( expectedBytes[i], result[i] );
            }
        }

        #endregion GetMapFromHexDigitString

        #region GetHexDigitStringFromMap

        /// <summary>
        /// Getting a string from a map works correctly
        /// </summary>
        [TestMethod]
        public void GetHexDigitStringFromMapWorksCorrectly()
        {
            var map = new byte[] { 0x_ab, 0x_cd, 0x_ef, 0x_01, 0x_23, 0x_45, 0x_67, 0x_89 };
            var expectedString = "ABCDEF0123456789";

            var result = StreakTypeService.GetHexDigitStringFromMap( map );
            Assert.AreEqual( expectedString, result );
        }

        #endregion GetHexDigitStringFromMap

        #region GetAggregateMap

        /// <summary>
        /// Getting an aggregate map functions correctly
        /// </summary>
        [TestMethod]
        public void GetAggregateMap()
        {
            var map1 = new byte[] { 0b_1000_0000, 0b_0010_0000, 0b_1000_0100 };
            var map2 = new byte[] { 0b_1001_0000, 0b_0010_0100, 0b_0000_0100, 0b_1111_0101 };
            var map3 = new byte[] { };

            var result = StreakTypeService.GetAggregateMap( new byte[][] { map1, map2, map3 } );

            // Verify map 1 didn't change
            Assert.HasCount( 3, map1 );
            Assert.AreEqual( 0b_1000_0000, map1[0] );
            Assert.AreEqual( 0b_0010_0000, map1[1] );
            Assert.AreEqual( 0b_1000_0100, map1[2] );

            // Verify map 2 didn't change
            Assert.HasCount( 4, map2 );
            Assert.AreEqual( 0b_1001_0000, map2[0] );
            Assert.AreEqual( 0b_0010_0100, map2[1] );
            Assert.AreEqual( 0b_0000_0100, map2[2] );
            Assert.AreEqual( 0b_1111_0101, map2[3] );

            // Verify map3 didn't change
            Assert.IsEmpty( map3 );

            // Verify that the result is a new array, consisting of bytes OR'ed together
            Assert.HasCount( 128, result );
            Assert.AreEqual( map1[2] | map2[3], result[128 - 1] );
            Assert.AreEqual( map1[1] | map2[2], result[128 - 2] );
            Assert.AreEqual( map1[0] | map2[1], result[128 - 3] );
            Assert.AreEqual( map2[0], result[128 - 4] );

            // Check all the other bytes are 0
            for ( var i = 0; i < ( 128 - 4 ); i++ )
            {
                Assert.AreEqual( 0, result[i] );
            }
        }

        #endregion GetAggregateMap

        #region Setup

        private const string StreakTypeGuidString = "93050DB0-82FC-4EBE-9AB8-8BB8BADFB2F0";
        private const string EmptyStreakTypeGuidString = "84161DB0-82FC-4EBE-9AB8-8BB8BADFB2A2";

        /*
         * Occurrences    01110011110011110011110011110011110011110011110011110011
         * Engagements    01001010100011000011010000001111111100101111111111110001
         * Exclusions     01000010000000000000000000000000000000110000000000000000
         * Result          1XX  1X1X  21XX  21X1  XXXX  4321  XX9E  8765  4321  X1
         * Day            54321098765432109876543211098765432109876543210987654321
         * Month                                  2                              1
         */

        /// <summary>
        /// Holds references to the streak data seeded for the GetStreakData tests.
        /// </summary>
        private class StreakTestData
        {
            public StreakTypeService StreakTypeService { get; set; }
            public int StreakTypeId { get; set; }
            public int EmptyStreakTypeId { get; set; }
            public int PersonId { get; set; }
        }

        /// <summary>
        /// Seeds a person, a populated streak type, and an empty streak type into the
        /// mocked context. Navigation properties are wired explicitly (e.g. Streak.PersonAlias)
        /// because the mocked context performs no FK or navigation-property fixup.
        /// </summary>
        private static StreakTestData SeedStreakData( RockContext rockContext )
        {
            var person = MockData.CreatePerson( rockContext );
            var personAlias = person.Aliases.First();

            var streakType = new StreakType
            {
                Id = 1,
                Guid = new Guid( StreakTypeGuidString ),
                OccurrenceMap = new byte[] { 0b_0111_0011, 0b_1100_1111, 0b_0011_1100, 0b_1111_0011, 0b_1100_1111, 0b_0011_1100, 0b_1111_0011 },
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = new DateTime( 2019, 1, 1 ),
                Name = "Testing StreakType"
            };
            var streak = new Streak
            {
                Id = 1,
                StreakTypeId = streakType.Id,
                PersonAliasId = personAlias.Id,
                PersonAlias = personAlias,
                EngagementMap = new byte[] { 0b_0100_1010, 0b_1000_1100, 0b_0011_0100, 0b_0000_1111, 0b_1111_0010, 0b_1111_1111, 0b_1111_0001 },
                ExclusionMap = new byte[] { 0b_0100_0010, 0b_0000_0000, 0b_0000_0000, 0b_0000_0000, 0b_0000_0011, 0b_0000_0000, 0b_0000_0000 }
            };
            rockContext.Set<StreakType>().Add( streakType );
            rockContext.Set<Streak>().Add( streak );

            var emptyStreakType = new StreakType
            {
                Id = 2,
                Guid = new Guid( EmptyStreakTypeGuidString ),
                OccurrenceFrequency = StreakOccurrenceFrequency.Daily,
                StartDate = RockDateTime.Today,
                Name = "Empty Testing StreakType"
            };
            var emptyStreak = new Streak
            {
                Id = 2,
                StreakTypeId = emptyStreakType.Id,
                PersonAliasId = personAlias.Id,
                PersonAlias = personAlias
            };
            rockContext.Set<StreakType>().Add( emptyStreakType );
            rockContext.Set<Streak>().Add( emptyStreak );

            return new StreakTestData
            {
                StreakTypeService = new StreakTypeService( rockContext ),
                StreakTypeId = streakType.Id,
                EmptyStreakTypeId = emptyStreakType.Id,
                PersonId = person.Id
            };
        }

        #endregion Setup

        #region GetStreakData

        /// <summary>
        /// Tests GetStreakData
        /// </summary>
        [TestMethod]
        public void GetStreakData()
        {
            var data = SeedStreakData( _rockAppScope.App.CreateRockContext() );

            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 2, 4 );
            var result = data.StreakTypeService.GetStreakData( StreakTypeCache.Get( data.StreakTypeId ), data.PersonId, out string errorMessage,
                startDate, endDate, true, true, 100 );

            Assert.AreEqual( string.Empty, errorMessage );
            Assert.IsNotNull( result );

            Assert.AreEqual( 9, result.LongestStreakCount );
            Assert.AreEqual( new DateTime( 2019, 1, 5 ), result.LongestStreakStartDate );
            Assert.AreEqual( new DateTime( 2019, 1, 18 ), result.LongestStreakEndDate );

            Assert.AreEqual( 1, result.CurrentStreakCount );
            Assert.AreEqual( new DateTime( 2019, 2, 4 ), result.CurrentStreakStartDate );

            Assert.HasCount( 4, result.ComputedStreaks );

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
            var data = SeedStreakData( _rockAppScope.App.CreateRockContext() );

            var startDate = RockDateTime.Today;
            var endDate = RockDateTime.Today;
            var result = data.StreakTypeService.GetStreakData( StreakTypeCache.Get( data.EmptyStreakTypeId ), data.PersonId, out string errorMessage,
                startDate, endDate, true, true, 100 );

            Assert.AreEqual( string.Empty, errorMessage );
            Assert.IsNotNull( result );

            Assert.AreEqual( 0, result.LongestStreakCount );
            Assert.IsNull( result.LongestStreakStartDate );
            Assert.IsNull( result.LongestStreakEndDate );

            Assert.AreEqual( 0, result.CurrentStreakCount );
            Assert.IsNull( result.CurrentStreakStartDate );

            Assert.IsEmpty( result.ComputedStreaks );

            Assert.AreEqual( 0, result.EngagementsThisMonth );
            Assert.AreEqual( 0, result.EngagementsThisYear );
            Assert.IsNull( result.MostRecentEngagementDate );
            Assert.IsNull( result.MostRecentOccurrenceDate );
            Assert.IsFalse( result.EngagedAtMostRecentOccurrence );
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
            Assert.AreEqual( 31, result );

            // Year of 2019 is 365 days long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.AreEqual( 365, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.AreEqual( -2, result );

            // Same day calculation is 1 day because of inclusiveness
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, streakTypeCache, isInclusive );
            Assert.AreEqual( 1, result );
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
            Assert.AreEqual( 30, result );

            // Year of 2019 is 365 days long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.AreEqual( 364, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.AreEqual( -1, result );

            // Same day calculation is 0
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, streakTypeCache, isInclusive );
            Assert.AreEqual( 0, result );
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
            Assert.AreEqual( 4, result );

            // Year of 2019 is 52 weeks long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.AreEqual( 52, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 26 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.AreEqual( -1, result );

            // Same week calculation is 0
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, streakTypeCache, isInclusive );
            Assert.AreEqual( 0, result );
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
            Assert.AreEqual( 5, result );

            // Year of 2019 is 52 weeks long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.AreEqual( 53, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 26 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, streakTypeCache, isInclusive );
            Assert.AreEqual( -2, result );

            // Same day calculation is 1 day because of inclusiveness
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, streakTypeCache, isInclusive );
            Assert.AreEqual( 1, result );
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
                    Assert.IsFalse( errorMessage.IsNullOrWhiteSpace() );
                }
                else
                {
                    Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() );

                    // The day of the month is the offset + 1 since offset 0 is Jan 1, 2019
                    if ( date.Year == 2019 && date.Month == 1 && ( date.Day == 3 || date.Day == 14 || date.Day == 24 ) )
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
        /// Checks if bits are set in the byte map that is weekly occurrences
        /// </summary>
        [TestMethod]
        [Ignore( "Fix needed. This test appears to be failing due to changes in how the first day of the week is specified." )]
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
            Assert.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset a bit that is already 0
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 0 ), false, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset the first set bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 2 ), false, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( 0, result[2] ); // Verify change
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0, result[0] ); // Verify no changes

            // Reset a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 24 ), false, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.HasCount( newLength, result ); // Verify the array grew to the next multiple of 128
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
            Assert.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset a bit that is already 0
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 0 * 7 ), valueForReset, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Reset the first set bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 2 * 7 ), valueForReset, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( 0, result[2] ); // Verify change
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0, result[0] ); // Verify no changes

            // Reset a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 24 * 7 ), valueForReset, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.HasCount( newLength, result ); // Verify the array grew to the next multiple of 128
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
            Assert.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Set a bit that is already set
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 2 ), true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Set the least significant bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 0 ), true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte2 | lsb, result[2] ); // Verify change
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0, result[0] ); // Verify no changes

            // Set the most significant bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 23 ), true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte2 | lsb, result[2] ); // Verify no additional changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0 | msb, result[0] ); // Verify change

            // Set a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 24 ), true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.HasCount( newLength, result ); // Verify the array grew to the next multiple of 128
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
            Assert.IsFalse( errorMessage.IsNullOrWhiteSpace() ); // Verify error occurred
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Set a bit that is already set
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 2 * 7 ), true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no error
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte0, result[0] ); // Verify no changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte2, result[2] ); // Verify no changes

            // Set the least significant bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 0 * 7 ), true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte2 | lsb, result[2] ); // Verify change
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0, result[0] ); // Verify no changes

            // Set the most significant bit
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 23 * 7 ), true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreSame( result, map ); // Verify in-place operation
            Assert.AreEqual( byte2 | lsb, result[2] ); // Verify no additional changes
            Assert.AreEqual( byte1, result[1] ); // Verify no changes
            Assert.AreEqual( byte0 | msb, result[0] ); // Verify change

            // Set a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( streakTypeCache, map, startDate.AddDays( 24 * 7 ), true, out errorMessage );
            Assert.IsTrue( errorMessage.IsNullOrWhiteSpace() ); // Verify no errors
            Assert.AreNotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.HasCount( newLength, result ); // Verify the array grew to the next multiple of 128
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

        #endregion SetBit
    }
}