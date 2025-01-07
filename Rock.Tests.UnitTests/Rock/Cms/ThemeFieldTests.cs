using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using Moq.Protected;

using Newtonsoft.Json.Linq;

using Rock.Cms;
using Rock.Cms.ThemeFields;
using Rock.Configuration;
using Rock.Enums.Cms;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Cms
{
    [TestClass]
    public class ThemeFieldTests
    {
        #region VariableThemeField

        // VariableThemeField is abstract, so we actually text with a text
        // field type.

        [TestMethod]
        public void VariableThemeField_WithoutName_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"fields\": [{\"type\": \"text\"}]}";
            var expectedError = "Text field is missing 'name' property.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void VariableThemeField_WithoutVariable_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"fields\": [{\"type\": \"text\", \"name\": \"test\"}]}";
            var expectedError = "Text field is missing 'variable' property.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void VariableThemeField_WithNameAndVariable_Succeeds()
        {
            var expectedName = "testName";
            var expectedVariable = "testVariable";
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"text\", \"name\": \"{expectedName}\", \"variable\": \"{expectedVariable}\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType( theme.Fields[0], typeof( VariableThemeField ) );

            var variableField = ( VariableThemeField ) theme.Fields[0];

            Assert.That.AreEqual( ThemeFieldType.Text, variableField.Type );
            Assert.That.AreEqual( expectedName, variableField.Name );
            Assert.That.AreEqual( expectedVariable, variableField.Variable );
            Assert.That.AreEqual( string.Empty, variableField.Description );
            Assert.That.AreEqual( string.Empty, variableField.DefaultValue );
        }

        [TestMethod]
        public void VariableThemeField_WithDescriptionProperty_UsesProvidedValue()
        {
            var expectedDescription = "testDescription";
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"text\", \"name\": \"test\", \"variable\": \"test\", \"description\": \"{expectedDescription}\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            var variableField = ( VariableThemeField ) theme.Fields[0];

            Assert.That.AreEqual( expectedDescription, variableField.Description );
        }

        [TestMethod]
        public void VariableThemeField_WithDefaultValueProperty_UsesProvidedValue()
        {
            var expectedDefaultValue = "testValue";
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"text\", \"name\": \"test\", \"variable\": \"test\", \"default\": \"{expectedDefaultValue}\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            var variableField = ( VariableThemeField ) theme.Fields[0];

            Assert.That.AreEqual( expectedDefaultValue, variableField.DefaultValue );
        }

        [TestMethod]
        public void VariableThemeField_WithNoValue_ProvidesDefaultValue()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "testValue";
            var json = $"{{\"type\": \"literal\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\", \"default\": \"{expectedValue}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
            } );

            var literalField = new LiteralThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Literal );

            literalField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        [TestMethod]
        public void VariableThemeField_WithNullValue_ProvidesDefaultValue()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "testValue";
            var json = $"{{\"type\": \"literal\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\", \"default\": \"{expectedValue}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = null
            } );

            var literalField = new LiteralThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Literal );

            literalField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        #endregion

        #region LiteralThemeField

        [TestMethod]
        public void ParseLiteralField_WithNameAndVariable_Succeeds()
        {
            var json = "{\"name\": \"test\", \"fields\": [{\"type\": \"literal\", \"name\": \"testName\", \"variable\": \"test-variable\"}]}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType<LiteralThemeField>( theme.Fields[0], out var literalField );
            Assert.That.AreEqual( ThemeFieldType.Literal, literalField.Type );
        }

        [TestMethod]
        public void LiteralThemeField_WithEmptyValue_DoesNotAddVariable()
        {
            var json = "{\"type\": \"literal\", \"name\": \"testName\", \"variable\": \"test-variable\"}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = string.Empty
            } );

            var literalField = new LiteralThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Literal );

            literalField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( It.IsAny<string>(), It.IsAny<string>() ), Times.Never() );
        }

        [TestMethod]
        public void LiteralThemeField_WithValue_AddsVariable()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "testValue";
            var json = $"{{\"type\": \"literal\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = expectedValue
            } );

            var literalField = new LiteralThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Literal );

            literalField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        #endregion

        #region ColorThemeField

        [TestMethod]
        public void ColorThemeField_WithNameAndVariable_Succeeds()
        {
            var json = "{\"type\": \"color\", \"name\": \"testName\", \"variable\": \"test-variable\"}";

            var colorField = new ColorThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Color );

            Assert.That.AreEqual( ThemeFieldType.Color, colorField.Type );
        }

        [TestMethod]
        public void ColorThemeField_WithEmptyValue_DoesNotAddVariable()
        {
            var json = "{\"type\": \"color\", \"name\": \"testName\", \"variable\": \"test-variable\"}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = string.Empty
            } );

            var colorField = new ColorThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Color );

            colorField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( It.IsAny<string>(), It.IsAny<string>() ), Times.Never() );
        }

        [TestMethod]
        public void ColorThemeField_WithValue_AddsVariable()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "testValue";
            var json = $"{{\"type\": \"color\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = expectedValue
            } );

            var colorField = new ColorThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Color );

            colorField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        #endregion

        #region ImageThemeField

        [TestMethod]
        public void ImageThemeField_WithNameAndVariable_Succeeds()
        {
            var json = "{\"type\": \"image\", \"name\": \"testName\", \"variable\": \"test-variable\"}";

            var imageField = new ImageThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Image );

            Assert.That.AreEqual( ThemeFieldType.Image, imageField.Type );
        }

        [TestMethod]
        public void ImageThemeField_WithEmptyValue_DoesNotAddVariable()
        {
            var json = "{\"type\": \"image\", \"name\": \"testName\", \"variable\": \"test-variable\"}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = string.Empty
            } );

            var imageField = new ImageThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Image );

            imageField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( It.IsAny<string>(), It.IsAny<string>() ), Times.Never() );
        }

        [TestMethod]
        public void ImageThemeField_WithAbsoluteUrlValue_AddsVariable()
        {
            var rawValue = "/abc.jpg";
            var expectedVariable = "test-variable";
            var expectedValue = $"url('{rawValue}')";
            var json = $"{{\"type\": \"image\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = rawValue
            } );

            var imageField = new ImageThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Image );

            imageField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        [TestMethod]
        public void ImageThemeField_WithDoubleTildeValue_AddsVariable()
        {
            var rawValue = "~~/abc.jpg";
            var expectedVariable = "test-variable";
            var expectedValue = $"url('/Themes/TestTheme/{rawValue.Substring( 3 )}')";
            var json = $"{{\"type\": \"image\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.ThemeName ).Returns( "TestTheme" );
            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = rawValue
            } );

            using ( TestHelper.CreateScopedRockApp() )
            {
                var imageField = new ImageThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Image );

                imageField.AddCssOverrides( builderMock.Object );

                builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
            }
        }

        [TestMethod]
        public void ImageThemeField_WithGuidValue_AddsVariable()
        {
            var rawValue = "0f2d6465-b502-4f6f-acc7-10dc684a72a5";
            var expectedVariable = "test-variable";
            var expectedValue = $"url('/GetImage.ashx?guid={rawValue}')";
            var json = $"{{\"type\": \"image\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.ThemeName ).Returns( "TestTheme" );
            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = rawValue
            } );

            using ( TestHelper.CreateScopedRockApp() )
            {
                var imageField = new ImageThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Image );

                imageField.AddCssOverrides( builderMock.Object );

                builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
            }
        }

        #endregion

        #region TextThemeField

        [TestMethod]
        public void TextThemeField_WithNameAndVariable_Succeeds()
        {
            var json = "{\"type\": \"text\", \"name\": \"testName\", \"variable\": \"test-variable\"}";

            var textField = new TextThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Text );

            Assert.That.AreEqual( ThemeFieldType.Text, textField.Type );
            Assert.That.IsFalse( textField.IsMultiline );
            Assert.That.AreEqual( string.Empty, textField.Width );
        }

        [TestMethod]
        public void TextThemeField_WithMultiline_UsesProvidedValue()
        {
            var json = "{\"name\": \"test\", \"fields\": [{\"type\": \"text\", \"name\": \"testName\", \"variable\": \"test-variable\", \"multiline\": true}]}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType<TextThemeField>( theme.Fields[0], out var textField );
            Assert.That.IsTrue( textField.IsMultiline );
        }

        [TestMethod]
        public void TextThemeField_WithWidth_UsesProvidedValue()
        {
            var expectedWidth = "md";
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"text\", \"name\": \"testName\", \"variable\": \"test-variable\", \"width\": \"{expectedWidth}\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType<TextThemeField>( theme.Fields[0], out var textField );
            Assert.That.AreEqual( expectedWidth, textField.Width );
        }

        [TestMethod]
        public void TextThemeField_WithEmptyValue_AddsEmptyStringVariable()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "''";
            var json = $"{{\"type\": \"text\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = string.Empty
            } );

            var textField = new TextThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Text );

            textField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        [TestMethod]
        public void TextThemeField_WithValue_AddsVariable()
        {
            var expectedVariable = "test-variable";
            var rawValue = "testValue";
            var expectedValue = $"'{rawValue}'";
            var json = $"{{\"type\": \"text\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = rawValue
            } );

            var textField = new TextThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Text );

            textField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        [TestMethod]
        public void TextThemeField_WithSingleQuoteValue_EscapesVariable()
        {
            var expectedVariable = "test-variable";
            var rawValue = "te'st";
            var expectedValue = $"'te\\'st'";
            var json = $"{{\"type\": \"text\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = rawValue
            } );

            var textField = new TextThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Text );

            textField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        [TestMethod]
        public void TextThemeField_WithNewLineValue_EscapesVariable()
        {
            var expectedVariable = "test-variable";
            var rawValue = "te\nst";
            var expectedValue = "'te\\Ast'";
            var json = $"{{\"type\": \"text\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = rawValue
            } );

            var textField = new TextThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Text );

            textField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        [TestMethod]
        public void TextThemeField_WithBackslashValue_EscapesVariable()
        {
            var expectedVariable = "test-variable";
            var rawValue = "te\\st";
            var expectedValue = "'te\\\\st'";
            var json = $"{{\"type\": \"text\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = rawValue
            } );

            var textField = new TextThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Text );

            textField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        #endregion

        #region FileThemeField

        [TestMethod]
        public void FileThemeField_WithNameAndVariable_Succeeds()
        {
            var json = "{\"type\": \"file\", \"name\": \"testName\", \"variable\": \"test-variable\"}";

            var fileField = new FileThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.File );

            Assert.That.AreEqual( ThemeFieldType.File, fileField.Type );
        }

        [TestMethod]
        public void FileThemeField_WithGuidValue_AddsVariableWithGetFileHandler()
        {
            var rawValue = "0f2d6465-b502-4f6f-acc7-10dc684a72a5";
            var expectedVariable = "test-variable";
            var expectedValue = $"url('/GetFile.ashx?guid={rawValue}')";
            var json = $"{{\"type\": \"file\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.ThemeName ).Returns( "TestTheme" );
            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = rawValue
            } );

            using ( TestHelper.CreateScopedRockApp() )
            {
                var imageField = new FileThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.File );

                imageField.AddCssOverrides( builderMock.Object );

                builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
            }
        }

        #endregion

        #region SwitchThemeField

        [TestMethod]
        public void SwitchThemeField_WithNumberInOnImport_ThrowsException()
        {
            var json = "{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\", \"onImport\": 2}";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );
            }, "Import URLs must be of type string or an array of strings." );
        }

        [TestMethod]
        public void SwitchThemeField_WithArrayOfNumbersObjectInOnImport_ThrowsException()
        {
            var json = "{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\", \"onImport\": [2]}";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );
            }, "Import URLs must be of type string or an array of strings." );
        }

        [TestMethod]
        public void SwitchThemeField_WithNameAndVariable_Succeeds()
        {
            var json = "{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\"}";

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            Assert.That.AreEqual( ThemeFieldType.Switch, field.Type );
        }

        [TestMethod]
        public void SwitchThemeField_WithFalse_AddsVariableWithOffValue()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "testValue";
            var json = $"{{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\", \"offValue\": \"{expectedValue}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = "false"
            } );

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            field.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        [TestMethod]
        public void SwitchThemeField_WithFalse_AddsCustomContentWithOffContent()
        {
            var expectedContent = "testValue";
            var json = $"{{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\", \"offContent\": \"{expectedContent}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = "false"
            } );

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            field.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddCustomContent( expectedContent ), Times.Once() );
        }

        [TestMethod]
        public void SwitchThemeField_WithFalseAndSingleImport_AddsImportWithOffImport()
        {
            var expectedUrl = "off.css";
            var json = $"{{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\", \"offImport\": \"{expectedUrl}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = "false"
            } );

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            field.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddImport( expectedUrl ), Times.Once() );
        }

        [TestMethod]
        public void SwitchThemeField_WithFalseAndTwoImports_AddsImportsWithOffImport()
        {
            var expectedUrl1 = "off1.css";
            var expectedUrl2 = "off2.css";
            var json = $"{{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\", \"offImport\": [\"{expectedUrl1}\", \"{expectedUrl2}\"]}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = "false"
            } );

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            field.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddImport( expectedUrl1 ), Times.Once() );
            builderMock.Verify( m => m.AddImport( expectedUrl2 ), Times.Once() );
        }

        [TestMethod]
        public void SwitchThemeField_WithTrue_AddsVariableWithOnValue()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "testValue";
            var json = $"{{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\", \"onValue\": \"{expectedValue}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = "true"
            } );

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            field.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        [TestMethod]
        public void SwitchThemeField_WithTrue_AddsCustomContentWithOnContent()
        {
            var expectedContent = "testValue";
            var json = $"{{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\", \"onContent\": \"{expectedContent}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = "true"
            } );

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            field.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddCustomContent( expectedContent ), Times.Once() );
        }

        [TestMethod]
        public void SwitchThemeField_WithTrueAndSingleImport_AddsImportWithOffImport()
        {
            var expectedUrl = "on.css";
            var json = $"{{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\", \"onImport\": \"{expectedUrl}\"}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = "true"
            } );

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            field.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddImport( expectedUrl ), Times.Once() );
        }

        [TestMethod]
        public void SwitchThemeField_WithTrueAndTwoImports_AddsImportsWithOffImport()
        {
            var expectedUrl1 = "on1.css";
            var expectedUrl2 = "on2.css";
            var json = $"{{\"type\": \"switch\", \"name\": \"testName\", \"variable\": \"test-variable\", \"onImport\": [\"{expectedUrl1}\", \"{expectedUrl2}\"]}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                ["test-variable"] = "true"
            } );

            var field = new SwitchThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Switch );

            field.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddImport( expectedUrl1 ), Times.Once() );
            builderMock.Verify( m => m.AddImport( expectedUrl2 ), Times.Once() );
        }

        #endregion

        #region HeadingThemeField

        [TestMethod]
        public void HeadingThemeField_WithoutTitle_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"fields\": [{\"type\": \"heading\"}]}";
            var expectedError = "Heading field is missing 'title' property.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void HeadingThemeField_WithTitle_Succeeds()
        {
            var expectedTitle = "testTitle";
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"heading\", \"title\": \"{expectedTitle}\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType<HeadingThemeField>( theme.Fields[0], out var headingField );
            Assert.That.AreEqual( expectedTitle, headingField.Name );
            Assert.That.AreEqual( string.Empty, headingField.Description );
        }

        [TestMethod]
        public void ParseHeadingField_WithDescriptionProperty_UsesProvidedValue()
        {
            var expectedDescription = "testDescription";
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"heading\", \"title\": \"test\", \"description\": \"{expectedDescription}\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType<HeadingThemeField>( theme.Fields[0], out var headingField );
            Assert.That.AreEqual( expectedDescription, headingField.Description );
        }

        #endregion

        #region SpacerThemeField

        [TestMethod]
        public void SpacerThemeField_WithNoValues_Succeeds()
        {
            var json = "{\"name\": \"test\", \"fields\": [{\"type\": \"spacer\"}]}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType( theme.Fields[0], typeof( SpacerThemeField ) );
        }

        #endregion

        #region PanelThemeField

        [TestMethod]
        public void PanelThemeField_WithoutTitle_ThrowsException()
        {
            var json = "{\"name\": \"test\", \"fields\": [{\"type\": \"panel\"}]}";
            var expectedError = "Panel field is missing 'title' property.";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, expectedError );
        }

        [TestMethod]
        public void PanelThemeField_WithTitle_Succeeds()
        {
            var expectedTitle = "testTitle";
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"panel\", \"title\": \"{expectedTitle}\"}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType<PanelThemeField>( theme.Fields[0], out var panelField );
            Assert.That.AreEqual( expectedTitle, panelField.Name );
            Assert.That.IsFalse( panelField.IsExpanded );
            Assert.That.IsNotNull( panelField.Fields );
            Assert.That.IsEmpty( panelField.Fields );
        }

        [TestMethod]
        public void PanelThemeField_WithExpandedProperty_UsesProvidedValue()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"panel\", \"title\": \"test\", \"expanded\": true}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType<PanelThemeField>( theme.Fields[0], out var panelField );
            Assert.That.IsTrue( panelField.IsExpanded );
        }

        [TestMethod]
        public void PanelThemeField_WithChildSpacerField_Succeeds()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"panel\", \"title\": \"test\", \"fields\": [{{\"type\": \"spacer\"}}]}}]}}";

            var theme = ThemeDefinition.Parse( json );

            Assert.That.AreEqual( 1, theme.Fields.Count );
            Assert.That.IsInstanceOfType<PanelThemeField>( theme.Fields[0], out var panelField );
            Assert.That.IsNotEmpty( panelField.Fields );
        }

        [TestMethod]
        public void PanelThemeField_WithChildPanelField_ThrowsException()
        {
            var json = $"{{\"name\": \"test\", \"fields\": [{{\"type\": \"panel\", \"title\": \"test\", \"fields\": [{{\"type\": \"panel\", \"title\": \"test2\"}}]}}]}}";

            Assert.That.ThrowsExceptionWithMessage<FormatException>( () =>
            {
                ThemeDefinition.Parse( json );
            }, "Panel 'test' may not have nested panels." );
        }

        [TestMethod]
        public void PanelThemeField_WithChildPanelField_AddsVariable()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "testValue";
            var json = $"{{\"type\": \"panel\", \"title\": \"testName\", \"fields\": [{{\"type\": \"literal\", \"name\": \"testName\", \"variable\": \"{expectedVariable}\"}}]}}";
            var builderMock = new Mock<IThemeOverrideBuilder>();

            builderMock.Setup( m => m.VariableValues ).Returns( new Dictionary<string, string>
            {
                [expectedVariable] = expectedValue
            } );

            var panelField = new PanelThemeField( json.FromJsonOrThrow<JObject>(), ThemeFieldType.Panel );

            panelField.AddCssOverrides( builderMock.Object );

            builderMock.Verify( m => m.AddVariable( expectedVariable, expectedValue ), Times.Once() );
        }

        #endregion
    }
}
