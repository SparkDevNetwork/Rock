using Rock.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Model
{
    /// <summary>
    /// Tests for the Streak Type Service methods
    /// </summary>
    [TestClass]
    public class StreakTypeServiceTests
    {
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
            Assert.That.AreEqual( expectedBytes.Length, result.Length );

            for ( var i = 0; i < expectedBytes.Length; i++ )
            {
                Assert.That.AreEqual( expectedBytes[i], result[i] );
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
            Assert.That.AreEqual( expectedString, result );
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
            Assert.That.AreEqual( 3, map1.Length );
            Assert.That.AreEqual( 0b_1000_0000, map1[0] );
            Assert.That.AreEqual( 0b_0010_0000, map1[1] );
            Assert.That.AreEqual( 0b_1000_0100, map1[2] );

            // Verify map 2 didn't change
            Assert.That.AreEqual( 4, map2.Length );
            Assert.That.AreEqual( 0b_1001_0000, map2[0] );
            Assert.That.AreEqual( 0b_0010_0100, map2[1] );
            Assert.That.AreEqual( 0b_0000_0100, map2[2] );
            Assert.That.AreEqual( 0b_1111_0101, map2[3] );

            // Verify map3 didn't change
            Assert.That.AreEqual( 0, map3.Length );

            // Verify that the result is a new array, consisting of bytes OR'ed together
            Assert.That.AreEqual( 128, result.Length );
            Assert.That.AreEqual( map1[2] | map2[3], result[128 - 1] );
            Assert.That.AreEqual( map1[1] | map2[2], result[128 - 2] );
            Assert.That.AreEqual( map1[0] | map2[1], result[128 - 3] );
            Assert.That.AreEqual( map2[0], result[128 - 4] );

            // Check all the other bytes are 0
            for ( var i = 0; i < ( 128 - 4 ); i++ )
            {
                Assert.That.AreEqual( 0, result[i] );
            }
        }

        #endregion GetAggregateMap
    }
}