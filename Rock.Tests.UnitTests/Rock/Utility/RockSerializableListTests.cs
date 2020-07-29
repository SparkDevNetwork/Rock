using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Utility;

namespace Rock.Tests.Rock.Utility
{
    [TestClass]
    public class RockSerializableListTests
    {
        [TestMethod]
        public void RockSerializableList_SerializedSingleEntry_HasCorrectFormat()
        {
            var list = new List<string>();

            list.Add( "Value1" );

            var sd = new RockSerializableList( list );

            var uriString = sd.ToUriEncodedString();

            Assert.AreEqual( "Value1", uriString );
        }

        [TestMethod]
        public void RockSerializableList_SerializedMultipleEntries_HasCorrectFormat()
        {
            var list = new List<string>();

            list.Add( "Value1" );
            list.Add( "Value2" );
            list.Add( "Value3" );

            var sd = new RockSerializableList( list );

            var uriString = sd.ToUriEncodedString();

            Assert.AreEqual( "Value1|Value2|Value3", uriString );
        }

        [TestMethod]
        public void RockSerializableList_ToUriEncodedString_EncodesDelimitersCorrectly()
        {
            var list = new List<string>();

            list.Add( "Value1" );
            list.Add( "Value2|WithEmbedded|Delimiters" );
            list.Add( "Value3" );

            var sd = new RockSerializableList( list );

            var uriString = sd.ToUriEncodedString();

            Assert.AreEqual( "Value1|Value2%7CWithEmbedded%7CDelimiters|Value3", uriString );
        }

        [TestMethod]
        public void RockSerializableList_UriEncodedString_CanRoundtrip()
        {
            var list = new List<string>();

            list.Add( "Value1" );
            list.Add( @"Value2|,/([!-/:-@[-`{-~]|\sand\s)+/i" );
            list.Add( "Value3" );

            var sd1 = new RockSerializableList( list );

            var sd2 = RockSerializableList.FromUriEncodedString( sd1.ToUriEncodedString() );

            Assert.AreEqual( "Value1", sd2.List[0] );
            Assert.AreEqual( @"Value2|,/([!-/:-@[-`{-~]|\sand\s)+/i", sd2.List[1] );
            Assert.AreEqual( "Value3", sd2.List[2] );
        }

        [TestMethod]
        public void RockSerializableList_ToUriEncodedString_EscapesSpecialCharacters()
        {
            var list = new List<string>();

            list.Add( "Value1" );
            list.Add( @"Value2|,/([!-/:-@[-`{-~]|\sand\s)+/i" );
            list.Add( "Value3" );

            var sd = new RockSerializableList( list );

            var uriString = sd.ToUriEncodedString();

            Assert.AreEqual( "Value1|Value2%7C%2C%2F%28%5B%21-%2F%3A-%40%5B-%60%7B-~%5D%7C%5Csand%5Cs%29%2B%2Fi|Value3", uriString );
        }
    }
}
