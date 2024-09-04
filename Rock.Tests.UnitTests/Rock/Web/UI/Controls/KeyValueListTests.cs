using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Tests.Shared;
using Rock.Web.UI.Controls;

namespace Rock.Tests.UnitTests.Rock.Web.UI.Controls
{
    [TestClass]
    public class KeyValueListTests
    {
        [TestMethod]
        [DataRow( ',', "%2C" )]
        [DataRow( '^', "%5E" )]
        [DataRow( '|', "%7C" )]
        public void SetValueDictionary_WithSpecialCharacterInKey_EncodesValue( char specialCharacter, string encodedValue )
        {
            var expectedValue = $"Ke{encodedValue}y^Value";
            var key = $"Ke{specialCharacter}y";

            var control = new KeyValueList();

            control.SetValue( new Dictionary<string, string>
            {
                [key] = "Value"
            } );

            Assert.That.AreEqual( expectedValue, control.Value );
        }

        [TestMethod]
        [DataRow( ',', "%2C" )]
        [DataRow( '^', "%5E" )]
        [DataRow( '|', "%7C" )]
        public void SetValueDictionary_WithSpecialCharacterInValue_EncodesValue( char specialCharacter, string encodedValue )
        {
            var expectedValue = $"Key^Val{encodedValue}ue";
            var value = $"Val{specialCharacter}ue";

            var control = new KeyValueList();

            control.SetValue( new Dictionary<string, string>
            {
                ["Key"] = value
            } );

            Assert.That.AreEqual( expectedValue, control.Value );
        }

        [TestMethod]
        public void SetValueDictionary_WithSpecialCharactersInMultipleItems_EncodesValue()
        {
            var expectedValue = $"K%2Ce%5Ey%7C^Value%7C|Key%5E^Val%2Cue";
            var key1 = "K,e^y|";
            var value1 = "Value|";
            var key2 = "Key^";
            var value2 = "Val,ue";

            var control = new KeyValueList();

            control.SetValue( new Dictionary<string, string>
            {
                [key1] = value1,
                [key2] = value2
            } );

            Assert.That.AreEqual( expectedValue, control.Value );
        }

        [TestMethod]
        public void SetValue_WithNullDictionary_SetsValueToEmptyString()
        {
            var control = new KeyValueList
            {
                Value = "initial value"
            };

            control.SetValue( null );

            Assert.That.IsEmpty( control.Value );
        }

        [TestMethod]
        public void SetValue_WithEmptyDictionary_SetsValueToEmptyString()
        {
            var control = new KeyValueList
            {
                Value = "initial value"
            };

            control.SetValue( new Dictionary<string, string>() );

            Assert.That.IsEmpty( control.Value );
        }

        [TestMethod]
        [DataRow( ',', "%2C" )]
        [DataRow( '^', "%5E" )]
        [DataRow( '|', "%7C" )]
        public void GetValueAsDictionary_WithSpecialCharacterInKey_DecodesValue( char specialCharacter, string encodedValue )
        {
            var expectedValue = $"Ke{specialCharacter}y";
            var value = $"Ke{encodedValue}y^Value";

            var control = new KeyValueList
            {
                Value = value
            };

            Assert.That.AreEqual( expectedValue, control.GetValueAsDictionary().First().Key );
        }

        [TestMethod]
        [DataRow( ',', "%2C" )]
        [DataRow( '^', "%5E" )]
        [DataRow( '|', "%7C" )]
        public void GetValueAsDictionary_WithSpecialCharacterInValue_DecodesValue( char specialCharacter, string encodedValue )
        {
            var expectedValue = $"Val{specialCharacter}ue";
            var value = $"Key^Val{encodedValue}ue";

            var control = new KeyValueList
            {
                Value = value
            };

            Assert.That.AreEqual( expectedValue, control.GetValueAsDictionary().First().Value );
        }

        [TestMethod]
        public void GetValueAsDictionary_WithSpecialCharactersInMultipleItems_DecodesValue()
        {
            var expectedKey1 = "K,e^y|";
            var expectedValue1 = "Value|";
            var expectedKey2 = "Key^";
            var expectedValue2 = "Val,ue";
            var value = $"K%2Ce%5Ey%7C^Value%7C|Key%5E^Val%2Cue";

            var control = new KeyValueList
            {
                Value = value
            };

            var expectedValue = new Dictionary<string, string>
            {
                [expectedKey1] = expectedValue1,
                [expectedKey2] = expectedValue2
            };

            CollectionAssert.AreEquivalent(expectedValue, control.GetValueAsDictionary() );
        }

        [TestMethod]
        public void GetValueAsDictionary_WithNullValue_ReturnsEmptyDictionary()
        {
            var expectedValue = new Dictionary<string, string>();

            var control = new KeyValueList
            {
                Value = null
            };

            CollectionAssert.AreEquivalent( expectedValue, control.GetValueAsDictionary() );
        }

        [TestMethod]
        public void GetValueAsDictionary_WithEmptyValue_ReturnsEmptyDictionary()
        {
            var expectedValue = new Dictionary<string, string>();

            var control = new KeyValueList
            {
                Value = string.Empty
            };

            CollectionAssert.AreEquivalent( expectedValue, control.GetValueAsDictionary() );
        }

        [TestMethod]
        public void GetValueAsDictionaryOrNull_WithNullValue_ReturnsNull()
        {
            var control = new KeyValueList
            {
                Value = null
            };

            Assert.That.IsNull( control.GetValueAsDictionaryOrNull() );
        }

        [TestMethod]
        public void GetValueAsDictionaryOrNull_WithEmptyValue_ReturnsNull()
        {
            var control = new KeyValueList
            {
                Value = string.Empty
            };

            Assert.That.IsNull( control.GetValueAsDictionaryOrNull() );
        }

        [TestMethod]
        [DynamicData( nameof( GetValueAsDictionary_WithPredefinedValue_DecodesValue_DataSource ), DynamicDataSourceType.Method )]
        public void GetValueAsDictionary_WithPredefinedValue_DecodesValue( string value, Dictionary<string, string> expectedValue )
        {
            var control = new KeyValueList
            {
                Value = value
            };

            CollectionAssert.AreEquivalent( expectedValue, control.GetValueAsDictionary() );
        }

        public static IEnumerable<object[]> GetValueAsDictionary_WithPredefinedValue_DecodesValue_DataSource()
        {
            // This provides data of just some values found in databases that
            // seemed like good candidates to make sure nothing breaks in the
            // future.
            return new[]
            {
                new object[]
                {
                    "Title^{{ QueryString['Title'] }}|PostDescription^{{ QueryString['Description'] }}|Link^{{ QueryString['Link'] }}",
                    new Dictionary<string, string>
                    {
                        ["Title"] = "{{ QueryString['Title'] }}",
                        ["PostDescription"] = "{{ QueryString['Description'] }}",
                        ["Link"] = "{{ QueryString['Link'] }}"
                    }
                },
                new object[]
                {
                    "Task 1^10%|Task 2^40%|Task 3^14%",
                    new Dictionary<string, string>
                    {
                        ["Task 1"] = "10%",
                        ["Task 2"] = "40%",
                        ["Task 3"] = "14%"
                    }
                }
            };
        }
    }
}
