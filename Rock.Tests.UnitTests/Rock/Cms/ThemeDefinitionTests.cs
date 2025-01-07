using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Cms;
using Rock.Cms.ThemeFields;
using Rock.Enums.Cms;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Cms
{
    [TestClass]
    public class ThemeDefinitionTests
    {
        #region TryParse

        [TestMethod]
        public void TryParse_WithInvalidJson_ReturnsFalse()
        {
            var json = "[]";

            var result = ThemeDefinition.TryParse( json, out _ );

            Assert.That.IsFalse( result );
        }

        [TestMethod]
        public void TryParse_WithValidJson_ReturnsTrue()
        {
            var json = "{\"name\": \"test\"}";

            var result = ThemeDefinition.TryParse( json, out _ );

            Assert.That.IsTrue( result );
        }

        #endregion

        #region Parse

        [TestMethod]
        public void Parse_WithNullJson_ThrowsException()
        {
            string json = null;

            Assert.That.ThrowsException<ArgumentNullException>( () =>
            {
                ThemeDefinition.Parse( json );
            } );
        }

        [TestMethod]
        public void Parse_WithEmptyStringJson_ThrowsException()
        {
            var json = string.Empty;
            var expectedError = "Invalid theme definition.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void Parse_WithInvalidJson_ThrowsException()
        {
            var json = "[]";
            var expectedError = "Invalid theme definition.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void Parse_WithoutName_ThrowsException()
        {
            var json = "{}";
            var expectedError = "Theme is missing 'name' property.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void Parse_WithOnlyName_Succeeds()
        {
            var expectedName = "test";
            var json = $"{{\"name\": \"{expectedName}\"}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( expectedName, theme.Name );
            Assert.That.AreEqual( string.Empty, theme.Description );
            Assert.That.IsNotNull( theme.Fields );
            Assert.That.IsEmpty( theme.Fields );
        }

        [TestMethod]
        public void Parse_WithDescription_UsesProvidedValue()
        {
            var expectedDescription = "test";
            var json = $"{{\"name\": \"test\", \"description\": \"{expectedDescription}\"}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( expectedDescription, theme.Description );
        }

        [TestMethod]
        public void Parse_WithoutPurpose_ReturnsWebTheme()
        {
            var json = "{\"name\": \"test\"}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( ThemePurpose.Web, theme.Purpose );
        }

        [TestMethod]
        public void Parse_WithWebPurpose_ReturnsWebTheme()
        {
            var json = "{\"name\": \"test\", \"purpose\": \"web\"}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( ThemePurpose.Web, theme.Purpose );
        }

        [TestMethod]
        public void Parse_WithCheckinPurpose_ReturnsCheckinTheme()
        {
            var json = "{\"name\": \"test\", \"purpose\": \"checkin\"}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( ThemePurpose.Checkin, theme.Purpose );
        }

        [TestMethod]
        public void Parse_WithInvalidPurpose_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"purpose\": \"error\"}";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, "Theme purpose 'error' is not valid." );
        }

        [TestMethod]
        public void Parse_WithoutAvailableIconSets_ReturnsFontAwesome()
        {
            var json = "{\"name\": \"test\"}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( ThemeIconSet.FontAwesome, theme.AvailableIconSets );
        }

        [TestMethod]
        public void Parse_WithEmptyAvailableIconSets_ReturnsNoIcons()
        {
            var json = "{\"name\": \"test\", \"availableIconSets\": []}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 0, ( int ) theme.AvailableIconSets );
        }

        [TestMethod]
        public void Parse_WithFontAwesomeIconSet_ReturnsFontAwesome()
        {
            var json = "{\"name\": \"test\", \"availableIconSets\": [\"fontawesome\"]}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( ThemeIconSet.FontAwesome, theme.AvailableIconSets );
        }

        [TestMethod]
        public void Parse_WithTablerIconSet_ReturnsTabler()
        {
            var json = "{\"name\": \"test\", \"availableIconSets\": [\"tabler\"]}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( ThemeIconSet.Tabler, theme.AvailableIconSets );
        }

        [TestMethod]
        public void Parse_WithTwoIconSets_ReturnsBothIconSets()
        {
            var json = "{\"name\": \"test\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( ThemeIconSet.FontAwesome | ThemeIconSet.Tabler, theme.AvailableIconSets );
        }

        [TestMethod]
        public void Parse_WithInvalidAvailableIconSetValue_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"availableIconSets\": [\"error\"]}";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, "Theme icon set 'error' is not valid." );
        }

        [TestMethod]
        public void Parse_WithStringAvailableIconSetValue_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"availableIconSets\": \"error\"}";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, "Property 'availableIconSets' must be an array." );
        }

        [TestMethod]
        public void Parse_WithInvalidFieldsProperty_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"fields\": \"\"}";
            var expectedError = "Theme 'fields' property was expected to be an array but was String.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void Parse_WithInvalidFieldObject_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"fields\": [\"\"]}";
            var expectedError = "Theme field was expected to be an object but was String.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void Parse_WithEmptyFieldObject_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"fields\": [{}]}";
            var expectedError = "Unknown field type '' found.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        #endregion

        #region ParseField

        [TestMethod]
        public void ParseField_WithLiteralField_ReturnsLiteralThemeField()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"literal\", \"name\": \"test\", \"variable\": \"test\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType( theme.Fields[0], typeof( LiteralThemeField ) );
        }

        [TestMethod]
        public void ParseField_WithColorField_ReturnsColorThemeField()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"color\", \"name\": \"test\", \"variable\": \"test\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType( theme.Fields[0], typeof( ColorThemeField ) );
        }

        [TestMethod]
        public void ParseField_WithImageField_ReturnsImageThemeField()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"image\", \"name\": \"test\", \"variable\": \"test\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType( theme.Fields[0], typeof( ImageThemeField ) );
        }

        [TestMethod]
        public void ParseField_WithTextField_ReturnsTextThemeField()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"text\", \"name\": \"test\", \"variable\": \"test\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType( theme.Fields[0], typeof( TextThemeField ) );
        }

        [TestMethod]
        public void ParseField_WithFileField_ReturnsFileThemeField()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"file\", \"name\": \"test\", \"variable\": \"test\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType( theme.Fields[0], typeof( FileThemeField ) );
        }

        [TestMethod]
        public void ParseField_WithSwitchField_ReturnsSwitchThemeField()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"switch\", \"name\": \"test\", \"variable\": \"test\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType( theme.Fields[0], typeof( SwitchThemeField ) );
        }

        #endregion
    }
}
