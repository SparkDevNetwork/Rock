using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn.v2.Labels;
using Rock.Enums.CheckIn.Labels;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Labels
{
    [TestClass]
    public class TextFieldConfigurationTests
    {
        // These dataKey values should match what is in Obsidian.
        [TestMethod]
        [DataRow( nameof( TextFieldConfiguration.SourceKey ), "testText", "sourceKey", "testText" )]
        [DataRow( nameof( TextFieldConfiguration.FormatterOptionKey ), "testText", "formatterOptionKey", "testText" )]
        [DataRow( nameof( TextFieldConfiguration.CollectionFormat ), TextCollectionFormat.TwoColumn, "collectionFormat", "3" )]
        [DataRow( nameof( TextFieldConfiguration.PlaceholderText ), "testText", "placeholderText", "testText" )]
        [DataRow( nameof( TextFieldConfiguration.IsDynamicText ), true, "isDynamicText", "true" )]
        [DataRow( nameof( TextFieldConfiguration.StaticText ), "testText", "staticText", "testText" )]
        [DataRow( nameof( TextFieldConfiguration.DynamicTextTemplate ), "testText", "dynamicTextTemplate", "testText" )]
        [DataRow( nameof( TextFieldConfiguration.FontSize ), 28.5, "fontSize", "28.5" )]
        // AdaptiveFontSize is tested seperately
        [DataRow( nameof( TextFieldConfiguration.HorizontalAlignment ), HorizontalTextAlignment.Right, "horizontalAlignment", "2" )]
        [DataRow( nameof( TextFieldConfiguration.IsBold ), true, "isBold", "true" )]
        [DataRow( nameof( TextFieldConfiguration.IsColorInverted ), true, "isColorInverted", "true" )]
        [DataRow( nameof( TextFieldConfiguration.IsCondensed ), true, "isCondensed", "true" )]
        [DataRow( nameof( TextFieldConfiguration.MaxLength ), 17, "maxLength", "17" )]
        public void Initialize_WithSingleValue_InitializesProperty( string propertyName, object expectedValue, string dataKey, string dataValue )
        {
            var data = new Dictionary<string, string>
            {
                [dataKey] = dataValue
            };

            var instance = new TextFieldConfiguration();
            instance.Initialize( data );

            var propertyValue = instance.GetPropertyValue( propertyName );

            Assert.AreEqual( expectedValue, propertyValue );
        }

        [TestMethod]
        public void Initialize_WithValidAdaptiveFontSize_ParsesCorrectly()
        {
            var data = new Dictionary<string, string>
            {
                ["adaptiveFontSize"] = "6=20;10=14"
            };

            var instance = new TextFieldConfiguration();
            instance.Initialize( data );

            Assert.IsNotNull( instance.AdaptiveFontSize );
            Assert.AreEqual( 2, instance.AdaptiveFontSize.Count );
            Assert.IsTrue( instance.AdaptiveFontSize.ContainsKey( 6 ) );
            Assert.AreEqual( 20, instance.AdaptiveFontSize[ 6 ] );
            Assert.IsTrue( instance.AdaptiveFontSize.ContainsKey( 10 ) );
            Assert.AreEqual( 14, instance.AdaptiveFontSize [ 10 ] );
        }

        [TestMethod]
        public void DeclaredType_HasExpectedPropertyCount()
        {
            // This is a simple test to help us know when new properties are
            // added so we can update the other tests to check for those
            // properties.
            var type = typeof( TextFieldConfiguration );
            var expectedPropertyCount = 14;

            var propertyCount = type.GetProperties().Length;

            Assert.AreEqual( expectedPropertyCount, propertyCount );
        }
    }
}
