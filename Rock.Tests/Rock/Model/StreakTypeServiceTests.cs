using System;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    /// <summary>
    /// Tests for the Streak Type Service methods
    /// </summary>
    public class StreakTypeServiceTests
    {
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
    }
}