using System;
using Rock.Data;
using Rock.Model;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    /// <summary>
    /// Tests for the Streak Type Service methods
    /// </summary>
    public class StreakTypeServiceTests
    {
        #region IsBitSet

        /// <summary>
        /// Checks if bits are set in the byte map that is daily occurrences
        /// </summary>
        [Fact]
        public void IsBitSetIsCorrectForDailyMap()
        {
            // Day Offset             3210 9876     5432 1098     7654 3210
            var map = new byte[] { 0b_1000_0000, 0b_0010_0000, 0b_0000_0100 };
            var mapStartDate = new DateTime( 2019, 1, 1 );

            for ( var dayOffset = -5; dayOffset < 100; dayOffset++ )
            {
                var date = mapStartDate.AddDays( dayOffset );
                var isSet = StreakTypeService.IsBitSet( map, mapStartDate, date, StreakOccurrenceFrequency.Daily, out var errorMessage );

                if ( dayOffset < 0 )
                {
                    // Should get error about checking a bit that is pre-start-date
                    Assert.NotEmpty( errorMessage );
                }
                else
                {
                    Assert.Empty( errorMessage );

                    // The day of the month is the offset + 1 since offset 0 is Jan 1, 2019
                    if ( date.Year == 2019 && date.Month == 1 && ( date.Day == 3 || date.Day == 14 || date.Day == 24 ) )
                    {
                        Assert.True( isSet );
                    }
                    else
                    {
                        Assert.False( isSet );
                    }
                }
            }
        }

        /// <summary>
        /// Checks if bits are set in the byte map that is weekly occurrences
        /// </summary>
        [Fact]
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
                    Assert.NotEmpty( errorMessage );
                }
                else
                {
                    Assert.Empty( errorMessage );

                    // Bit index 2 is week of Jan 14-20
                    if ( date.Year == 2019 && date.Month == 1 && date.Day >= 14 && date.Day <= 20 )
                    {
                        Assert.True( isSet );
                    }
                    // Bit index 13 is week of Apr 1-7
                    else if ( date.Year == 2019 && date.Month == 4 && date.Day >= 1 && date.Day <= 7 )
                    {
                        Assert.True( isSet );
                    }
                    // Bit index 23 is week of Jun 10-16
                    else if ( date.Year == 2019 && date.Month == 6 && date.Day >= 10 && date.Day <= 16 )
                    {
                        Assert.True( isSet );
                    }
                    else
                    {
                        Assert.False( isSet );
                    }
                }
            }
        }

        #endregion IsBitSet

        #region SetBit

        /// <summary>
        /// Setting bits works correctly for daily maps
        /// </summary>
        [Fact]
        public void SetBitWorksForDailyMap()
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
            var startDate = new DateTime( 2019, 1, 1 );
            var frequency = StreakOccurrenceFrequency.Daily;

            // Set a bit before the start date and get an error
            var result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( -1 ), frequency, true, out var errorMessage );
            Assert.NotEmpty( errorMessage ); // Verify error occurred
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte0, result[0] ); // Verify no changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte2, result[2] ); // Verify no changes

            // Set a bit that is already set
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 2 ), frequency, true, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no error
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte0, result[0] ); // Verify no changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte2, result[2] ); // Verify no changes

            // Set the least significant bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 0 ), frequency, true, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte2 | lsb, result[2] ); // Verify change
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte0, result[0] ); // Verify no changes

            // Set the most significant bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 23 ), frequency, true, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte2 | lsb, result[2] ); // Verify no additional changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte0 | msb, result[0] ); // Verify change

            // Set a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 24 ), frequency, true, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.NotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.True( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.Equal( byte2 | lsb, result[newLength - 1] ); // Verify no additional changes
            Assert.Equal( byte1, result[newLength - 2] ); // Verify no changes
            Assert.Equal( byte0 | msb, result[newLength - 3] ); // Verify no additional changes
            Assert.Equal( lsb, result[newLength - 4] ); // Verify first bit in first new byte is set

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 4 ); i++ )
            {
                Assert.Equal( 0, result[i] );
            }
        }

        /// <summary>
        /// Setting bits works correctly for weekly maps
        /// </summary>
        [Fact]
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
            Assert.NotEmpty( errorMessage ); // Verify error occurred
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte0, result[0] ); // Verify no changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte2, result[2] ); // Verify no changes

            // Set a bit that is already set
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 2 * 7 ), frequency, true, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no error
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte0, result[0] ); // Verify no changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte2, result[2] ); // Verify no changes

            // Set the least significant bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 0 * 7 ), frequency, true, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte2 | lsb, result[2] ); // Verify change
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte0, result[0] ); // Verify no changes

            // Set the most significant bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 23 * 7 ), frequency, true, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte2 | lsb, result[2] ); // Verify no additional changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte0 | msb, result[0] ); // Verify change

            // Set a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 24 * 7 ), frequency, true, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.NotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.True( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.Equal( byte2 | lsb, result[newLength - 1] ); // Verify no additional changes
            Assert.Equal( byte1, result[newLength - 2] ); // Verify no changes
            Assert.Equal( byte0 | msb, result[newLength - 3] ); // Verify no additional changes
            Assert.Equal( lsb, result[newLength - 4] ); // Verify first bit in first new byte is set

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 4 ); i++ )
            {
                Assert.Equal( 0, result[i] );
            }
        }

        /// <summary>
        /// Resetting bits works correctly for daily maps
        /// </summary>
        [Fact]
        public void ResetBitWorksForDailyMap()
        {
            // Offset             7654 3210
            const byte byte2 = 0b_0000_0100;

            // Offset             5432 1098
            const byte byte1 = 0b_0010_0000;

            // Offset             3210 9876
            const byte byte0 = 0b_1000_0000;

            var map = new byte[] { byte0, byte1, byte2 };
            var startDate = new DateTime( 2019, 1, 1 );
            var frequency = StreakOccurrenceFrequency.Daily;

            // Reset a bit before the start date and get an error
            var result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( -1 ), frequency, false, out var errorMessage );
            Assert.NotEmpty( errorMessage ); // Verify error occurred
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte0, result[0] ); // Verify no changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte2, result[2] ); // Verify no changes

            // Reset a bit that is already 0
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 0 ), frequency, false, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no error
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte0, result[0] ); // Verify no changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte2, result[2] ); // Verify no changes

            // Reset the first set bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 2 ), frequency, false, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( 0, result[2] ); // Verify change
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte0, result[0] ); // Verify no changes

            // Reset a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 24 ), frequency, false, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.NotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.True( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.Equal( 0, result[newLength - 1] ); // Verify no additional changes
            Assert.Equal( byte1, result[newLength - 2] ); // Verify no changes
            Assert.Equal( byte0, result[newLength - 3] ); // Verify no changes

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 3 ); i++ )
            {
                Assert.Equal( 0, result[i] );
            }
        }

        /// <summary>
        /// Resetting bits works correctly for weekly maps
        /// </summary>
        [Fact]
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
            Assert.NotEmpty( errorMessage ); // Verify error occurred
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte0, result[0] ); // Verify no changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte2, result[2] ); // Verify no changes

            // Reset a bit that is already 0
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 0 * 7 ), frequency, valueForReset, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no error
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( byte0, result[0] ); // Verify no changes
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte2, result[2] ); // Verify no changes

            // Reset the first set bit
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 2 * 7 ), frequency, valueForReset, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.Same( result, map ); // Verify in-place operation
            Assert.Equal( 0, result[2] ); // Verify change
            Assert.Equal( byte1, result[1] ); // Verify no changes
            Assert.Equal( byte0, result[0] ); // Verify no changes

            // Reset a bit beyond the array and force it to grow
            result = StreakTypeService.SetBit( map, startDate, startDate.AddDays( 24 * 7 ), frequency, valueForReset, out errorMessage );
            Assert.Empty( errorMessage ); // Verify no errors
            Assert.NotSame( result, map ); // Verify memory allocation occurred for new array
            var newLength = 128;
            Assert.True( result.Length == newLength ); // Verify the array grew to the next multiple of 128            
            Assert.Equal( 0, result[newLength - 1] ); // Verify no additional changes
            Assert.Equal( byte1, result[newLength - 2] ); // Verify no changes
            Assert.Equal( byte0, result[newLength - 3] ); // Verify no changes

            // Verify all other bytes are unset
            for ( var i = 0; i < ( newLength - 3 ); i++ )
            {
                Assert.Equal( 0, result[i] );
            }
        }

        #endregion SetBit

        #region GetMapFromHexDigitString

        /// <summary>
        /// Getting a map works correctly
        /// </summary>
        [Fact]
        public void GetMapFromHexDigitStringWorksCorrectly()
        {
            var expectedBytes = new byte[] { 0x_ab, 0x_cd, 0x_ef, 0x_01, 0x_23, 0x_45, 0x_67, 0x_89 };
            var hexString = "ABCDEF0123456789";

            var result = StreakTypeService.GetMapFromHexDigitString( hexString );
            Assert.Equal( expectedBytes.Length, result.Length );

            for ( var i = 0; i < expectedBytes.Length; i++ )
            {
                Assert.Equal( expectedBytes[i], result[i] );
            }
        }

        #endregion GetMapFromHexDigitString

        #region GetHexDigitStringFromMap

        /// <summary>
        /// Getting a string from a map works correctly
        /// </summary>
        [Fact]
        public void GetHexDigitStringFromMapWorksCorrectly()
        {
            var map = new byte[] { 0x_ab, 0x_cd, 0x_ef, 0x_01, 0x_23, 0x_45, 0x_67, 0x_89 };
            var expectedString = "ABCDEF0123456789";

            var result = StreakTypeService.GetHexDigitStringFromMap( map );
            Assert.Equal( expectedString, result );
        }

        #endregion GetHexDigitStringFromMap

        #region GetFrequencyUnitDifference

        /// <summary>
        /// Calculating the difference in daily dates inclusively works correctly
        /// </summary>
        [Fact]
        public void GetFrequencyUnitDifferenceInclusiveDaily()
        {
            var frequency = StreakOccurrenceFrequency.Daily;
            var isInclusive = true;

            // Month of January is 31 days long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( 31, result );

            // Year of 2019 is 365 days long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( 365, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( -2, result );

            // Same day calculation is 1 day because of inclusiveness
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, frequency, isInclusive );
            Assert.Equal( 1, result );
        }

        /// <summary>
        /// Calculating the difference in daily dates exclusively works correctly
        /// </summary>
        [Fact]
        public void GetFrequencyUnitDifferenceExclusiveDaily()
        {
            var frequency = StreakOccurrenceFrequency.Daily;
            var isInclusive = false;

            // Month of January is 31 days long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( 30, result );

            // Year of 2019 is 365 days long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( 364, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( -1, result );

            // Same day calculation is 0
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, frequency, isInclusive );
            Assert.Equal( 0, result );
        }

        /// <summary>
        /// Calculating the difference in weekly dates inclusively works correctly
        /// </summary>
        [Fact]
        public void GetFrequencyUnitDifferenceInclusiveWeekly()
        {
            var frequency = StreakOccurrenceFrequency.Weekly;
            var isInclusive = true;

            // Month of January is 4 weeks long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( 5, result );

            // Year of 2019 is 52 weeks long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( 53, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 26 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( -2, result );

            // Same day calculation is 1 day because of inclusiveness
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, frequency, isInclusive );
            Assert.Equal( 1, result );
        }

        /// <summary>
        /// Calculating the difference in daily dates exclusively works correctly
        /// </summary>
        [Fact]
        public void GetFrequencyUnitDifferenceExclusiveWeekly()
        {
            var frequency = StreakOccurrenceFrequency.Weekly;
            var isInclusive = false;

            // Month of January is 4 weeks long
            var startDate = new DateTime( 2019, 1, 1 );
            var endDate = new DateTime( 2019, 1, 31 );
            var result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( 4, result );

            // Year of 2019 is 52 weeks long
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2019, 12, 31 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( 52, result );

            // Negative calculation is okay
            startDate = new DateTime( 2019, 1, 1 );
            endDate = new DateTime( 2018, 12, 26 );
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, endDate, frequency, isInclusive );
            Assert.Equal( -1, result );

            // Same week calculation is 0
            result = StreakTypeService.GetFrequencyUnitDifference( startDate, startDate, frequency, isInclusive );
            Assert.Equal( 0, result );
        }

        #endregion GetFrequencyUnitDifference

        #region GetAggregateMap

        /// <summary>
        /// Getting an aggregate map functions correctly
        /// </summary>
        [Fact]
        public void GetAggregateMap()
        {
            var map1 = new byte[] { 0b_1000_0000, 0b_0010_0000, 0b_1000_0100 };
            var map2 = new byte[] { 0b_1001_0000, 0b_0010_0100, 0b_0000_0100, 0b_1111_0101 };
            var map3 = new byte[] { };

            var result = StreakTypeService.GetAggregateMap( new byte[][] { map1, map2, map3 } );

            // Verify map 1 didn't change
            Assert.Equal( 3, map1.Length );
            Assert.Equal( 0b_1000_0000, map1[0] );
            Assert.Equal( 0b_0010_0000, map1[1] );
            Assert.Equal( 0b_1000_0100, map1[2] );

            // Verify map 2 didn't change
            Assert.Equal( 4, map2.Length );
            Assert.Equal( 0b_1001_0000, map2[0] );
            Assert.Equal( 0b_0010_0100, map2[1] );
            Assert.Equal( 0b_0000_0100, map2[2] );
            Assert.Equal( 0b_1111_0101, map2[3] );

            // Verify map3 didn't change
            Assert.Empty( map3 );

            // Verify that the result is a new array, consisting of bytes OR'ed together
            Assert.Equal( 128, result.Length );
            Assert.Equal( map1[2] | map2[3], result[128 - 1] );
            Assert.Equal( map1[1] | map2[2], result[128 - 2] );
            Assert.Equal( map1[0] | map2[1], result[128 - 3] );
            Assert.Equal( map2[0], result[128 - 4] );

            // Check all the other bytes are 0
            for ( var i = 0; i < ( 128 - 4 ); i++ )
            {
                Assert.Equal( 0, result[i] );
            }
        }

        #endregion GetAggregateMap

        #region GetMostRecentEngagementBits

        /// <summary>
        /// Getting the most recent bits for a daily map functions correctly
        /// </summary>
        [Fact]
        public void GetMostRecentEngagementBitsForDailyMap()
        {
            // Offset                         3210 9876     5432 1098     7654 3210
            var engagements = new byte[] { 0b_1000_0101, 0b_0010_1000, 0b_1000_0100 };
            var occurrences = new byte[] { 0b_1000_1001, 0b_0010_1010, 0b_1000_0100 };
            // Streak Status                  1    X  3       2  1 X      2     1

            var mapStartDate = new DateTime( 2019, 1, 1 );
            var frequency = StreakOccurrenceFrequency.Daily;
            var units = 24;

            var result = StreakTypeService.GetMostRecentEngagementBits( engagements, occurrences, mapStartDate, frequency, units );

            Assert.Equal( units, result.Length ); // There are 8 occurrence bits set in the map, but always returns unit count requested

            Assert.Equal( new DateTime( 2019, 1, 24 ), result[0].DateTime );
            Assert.True( result[0].HasEngagement );

            Assert.Equal( new DateTime( 2019, 1, 20 ), result[1].DateTime );
            Assert.False( result[1].HasEngagement );

            Assert.Equal( new DateTime( 2019, 1, 17 ), result[2].DateTime );
            Assert.True( result[2].HasEngagement );

            Assert.Equal( new DateTime( 2019, 1, 14 ), result[3].DateTime );
            Assert.True( result[3].HasEngagement );

            Assert.Equal( new DateTime( 2019, 1, 12 ), result[4].DateTime );
            Assert.True( result[4].HasEngagement );

            Assert.Equal( new DateTime( 2019, 1, 10 ), result[5].DateTime );
            Assert.False( result[5].HasEngagement );

            Assert.Equal( new DateTime( 2019, 1, 8 ), result[6].DateTime );
            Assert.True( result[6].HasEngagement );

            Assert.Equal( new DateTime( 2019, 1, 3 ), result[7].DateTime );
            Assert.True( result[7].HasEngagement );

            for ( var i = units; i < units; i++ )
            {
                Assert.Null( result[i] );
            }
        }

        /// <summary>
        /// Getting the most recent bits for a weekly map functions correctly
        /// </summary>
        [Fact]
        public void GetMostRecentEngagementBitsForWeeklyMap()
        {
            // Offset                         3210 9876     5432 1098     7654 3210
            var engagements = new byte[] { 0b_1000_0101, 0b_0010_1000, 0b_1000_0100 };
            var occurrences = new byte[] { 0b_1000_1001, 0b_0010_1010, 0b_1000_0100 };
            // Streak Status                  1    X  3       2  1 X      2     1

            var mapStartDate = new DateTime( 2019, 1, 6 );
            var frequency = StreakOccurrenceFrequency.Weekly;
            var units = 4;

            var result = StreakTypeService.GetMostRecentEngagementBits( engagements, occurrences, mapStartDate, frequency, units );

            Assert.Equal( units, result.Length ); // There are 8 occurrence bits set in the map, but always returns unit count requested

            Assert.Equal( new DateTime( 2019, 6, 16 ), result[0].DateTime );
            Assert.True( result[0].HasEngagement );

            Assert.Equal( new DateTime( 2019, 5, 19 ), result[1].DateTime );
            Assert.False( result[1].HasEngagement );

            Assert.Equal( new DateTime( 2019, 4, 28 ), result[2].DateTime );
            Assert.True( result[2].HasEngagement );

            Assert.Equal( new DateTime( 2019, 4, 7 ), result[3].DateTime );
            Assert.True( result[3].HasEngagement );
        }

        #endregion GetMostRecentEngagementBits
    }
}