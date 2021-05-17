using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Utility;

namespace Rock.Tests.Rock.Utility
{
    [TestClass]        
    public class RockSerializableDictionaryTests
    {
        [TestMethod]
        public void RockSerializableDictionary_SerializedSingleEntry_HasCorrectFormat()
        {
            var dict = new Dictionary<string, object>();

            dict.Add( "Key1", "Value1" );

            var sd = new RockSerializableDictionary( dict );

            var uriString = sd.ToUriEncodedString();

            Assert.AreEqual( "Key1^Value1", uriString );
        }

        [TestMethod]
        public void RockSerializableDictionary_SerializedMultipleEntries_HasCorrectFormat()
        {
            var dict = new Dictionary<string, object>();

            dict.Add( "Key1", "Value1" );
            dict.Add( "Key2", "Value2" );
            dict.Add( "Key3", "Value3" );

            var sd = new RockSerializableDictionary( dict );

            var uriString = sd.ToUriEncodedString();

            Assert.AreEqual( "Key1^Value1|Key2^Value2|Key3^Value3", uriString );
        }

        [TestMethod]
        public void RockSerializableDictionary_ToUriEncodedString_EncodesDelimitersCorrectly()
        {
            var sd = GetComplexDictionary();

            var uriString = sd.ToUriEncodedString();

            Assert.AreEqual( "Key1%7CWith%5ESpecial%2CCharacters^Value1a%7CValue1b%5EValue1c%2CValue1d|Key2%7CWith%5ESpecial%2CCharacters^Value2a%7CValue2b%5EValue2c%2CValue2d", uriString );
        }

        [TestMethod]
        public void RockSerializableDictionary_UriEncodedString_CanRoundtrip()
        {
            var sd1 = GetComplexDictionary();

            var uriString = sd1.ToUriEncodedString();

            var sd2 = RockSerializableDictionary.FromUriEncodedString( uriString );

            Assert.AreEqual( "Value1a|Value1b^Value1c,Value1d", sd2.Dictionary["Key1|With^Special,Characters"] );
        }

        [TestMethod]
        public void RockSerializableDictionary_ToUriEncodedString_EscapesSpecialCharacters()
        {
            const string testRegExDecoded = @",/([!-/:-@[-`{-~]|\sand\s)+/i";
            const string testRegExEncoded = @"%2C%2F%28%5B%21-%2F%3A-%40%5B-%60%7B-~%5D%7C%5Csand%5Cs%29%2B%2Fi";

            var dict = new Dictionary<string, object>();

            dict.Add( "RegEx1", testRegExDecoded );

            var sd = new RockSerializableDictionary( dict );

            var uriString = sd.ToUriEncodedString();

            Assert.AreEqual( "RegEx1^" + testRegExEncoded, uriString );
        }

        private RockSerializableDictionary GetComplexDictionary()
        {
            var dict = new Dictionary<string, object>();

            dict.Add( "Key1|With^Special,Characters", "Value1a|Value1b^Value1c,Value1d" );
            dict.Add( "Key2|With^Special,Characters", "Value2a|Value2b^Value2c,Value2d" );

            var sd = new RockSerializableDictionary( dict );

            return sd;
        }
    }
}
