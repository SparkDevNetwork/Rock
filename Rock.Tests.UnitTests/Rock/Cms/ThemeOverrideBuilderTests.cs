using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Cms;
using Rock.Enums.Cms;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Cms
{
    [TestClass]
    public class ThemeOverrideBuilderTests
    {
        #region Constructor

        [TestMethod]
        public void Constructor_WithNullVariableValues_CreatesEmptyDictionary()
        {
            var builder = new ThemeOverrideBuilder( "TestTheme", null );

            Assert.That.IsNotNull( builder.VariableValues );
        }

        #endregion

        #region AddVariable

        [TestMethod]
        public void AddVariable_WithEmptyValue_DoesNotEmitVariable()
        {
            var expectedVariable = "test-variable";
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddVariable( expectedVariable, string.Empty );

            var content = builder.Build( string.Empty );

            Assert.That.DoesNotContain( content, expectedVariable );
        }

        [TestMethod]
        public void AddVariable_WithNullValue_DoesNotEmitVariable()
        {
            var expectedVariable = "test-variable";
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddVariable( expectedVariable, null );

            var content = builder.Build( string.Empty );

            Assert.That.DoesNotContain( content, expectedVariable );
        }

        [TestMethod]
        public void AddVariable_WithValue_EmitsVariable()
        {
            var expectedVariable = "test-variable";
            var expectedValue = "testValue";
            var expectedContent = $"--{expectedVariable}: {expectedValue};";
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddVariable( expectedVariable, expectedValue );

            var content = builder.Build( string.Empty );

            Assert.That.Contains( content, expectedContent );
        }

        #endregion

        #region AddCustomContent

        [TestMethod]
        public void AddCustomContent_WithValue_EmitsContent()
        {
            var expectedContent = "div.test { color: red }";
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddCustomContent( expectedContent );

            var content = builder.Build( string.Empty );

            Assert.That.Contains( content, expectedContent );
        }

        #endregion

        #region AddImport

        [TestMethod]
        public void AddImport_WithEmptyValue_DoesNotEmitImport()
        {
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddImport( string.Empty );

            var content = builder.Build( string.Empty );

            Assert.That.IsEmpty( content );
        }

        [TestMethod]
        public void AddImport_WithNullValue_DoesNotEmitVariable()
        {
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddImport( null );

            var content = builder.Build( string.Empty );

            Assert.That.IsEmpty( content );
        }

        [TestMethod]
        public void AddImport_WithValue_EmitsImport()
        {
            var expectedUrl = "on.css";
            var expectedContent = $"@import url('{expectedUrl}');";
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddImport( expectedUrl );

            var content = builder.Build( string.Empty );

            Assert.That.Contains( content, expectedContent );
        }

        [TestMethod]
        public void AddImport_WithDoubleTildeValue_ExpandsTilde()
        {
            var expectedUrl = "~~/Styles/on.css";
            var expectedContent = $"@import url('{expectedUrl.Replace( "~~", "/Themes/TestTheme" )}');";
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddImport( expectedUrl );

            using ( TestHelper.CreateScopedRockApp() )
            {
                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, expectedContent );
            }
        }

        #endregion

        #region AddFontIconSets

        [TestMethod]
        public void AddFontIconSets_WithNoEnabledIconSets_DoesNotEmitAnything()
        {
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
            var themeJson = "{\"name\": \"TestTheme\"}";

            var themeDefinition = ThemeDefinition.Parse( themeJson );
            var customization = new ThemeCustomizationSettings
            {
                EnabledIconSets = 0
            };

            builder.AddFontIconSets( themeDefinition, customization );

            var content = builder.Build( string.Empty );

            Assert.That.IsEmpty( content );
        }

        [TestMethod]
        public void AddFontIconSets_WithNullEnabledIconSets_ImportsAllSupportedFonts()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = null
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "fontawesome" );
                Assert.That.Contains( content, "tabler" );
            }
        }

        [TestMethod]
        public void AddFontIconSets_WithFontAwesome_ImportsBaseCss()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = ThemeIconSet.FontAwesome
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "/Styles/style-v2/icons/fontawesome-icon.css" );
            }
        }

        [TestMethod]
        public void AddFontIconSets_WithFontAwesomeDefaultSolid_ImportsSolidCss()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = ThemeIconSet.FontAwesome,
                    DefaultFontAwesomeWeight = ThemeFontAwesomeWeight.Solid
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "/Styles/style-v2/icons/fontawesome-solid.css" );
            }
        }

        [TestMethod]
        public void AddFontIconSets_WithFontAwesomeDefaultRegular_ImportsRegularCss()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = ThemeIconSet.FontAwesome,
                    DefaultFontAwesomeWeight = ThemeFontAwesomeWeight.Regular
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "/Styles/style-v2/icons/fontawesome-regular.css" );
            }
        }

        [TestMethod]
        public void AddFontIconSets_WithFontAwesomeDefaultLight_ImportsLightCss()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = ThemeIconSet.FontAwesome,
                    DefaultFontAwesomeWeight = ThemeFontAwesomeWeight.Light
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "/Styles/style-v2/icons/fontawesome-light.css" );
            }
        }

        [TestMethod]
        public void AddFontIconSets_WithFontAwesomeAdditionalSolid_ImportsSolidCss()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = ThemeIconSet.FontAwesome,
                    AdditionalFontAwesomeWeights = new List<ThemeFontAwesomeWeight>
                    {
                        ThemeFontAwesomeWeight.Solid
                    }
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "/Styles/style-v2/icons/fontawesome-solid.css" );
            }
        }

        [TestMethod]
        public void AddFontIconSets_WithFontAwesomeAdditionalRegular_ImportsRegularCss()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = ThemeIconSet.FontAwesome,
                    AdditionalFontAwesomeWeights = new List<ThemeFontAwesomeWeight>
                    {
                        ThemeFontAwesomeWeight.Regular
                    }
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "/Styles/style-v2/icons/fontawesome-regular.css" );
            }
        }

        [TestMethod]
        public void AddFontIconSets_WithFontAwesomeAdditionalLight_ImportsLightCss()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = ThemeIconSet.FontAwesome,
                    AdditionalFontAwesomeWeights = new List<ThemeFontAwesomeWeight>
                    {
                        ThemeFontAwesomeWeight.Light
                    }
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "/Styles/style-v2/icons/fontawesome-light.css" );
            }
        }

        [TestMethod]
        public void AddFontIconSets_WithTabler_ImportsBaseCss()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );
                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"fontawesome\", \"tabler\"]}";

                var themeDefinition = ThemeDefinition.Parse( themeJson );
                var customization = new ThemeCustomizationSettings
                {
                    EnabledIconSets = ThemeIconSet.Tabler
                };

                builder.AddFontIconSets( themeDefinition, customization );

                var content = builder.Build( string.Empty );

                Assert.That.Contains( content, "/Styles/style-v2/icons/tabler-icon.css" );
            }
        }

        #endregion

        #region Build

        [TestMethod]
        public void Build_WithNoVariables_DoesNotEmitRootScope()
        {
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            var content = builder.Build( string.Empty );

            Assert.That.IsEmpty( content );
        }

        [TestMethod]
        public void Build_WithVariable_EmitsRootScope()
        {
            var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

            builder.AddVariable( "test-var", "none" );

            var content = builder.Build( string.Empty );

            Assert.That.Contains( content, ":root {" );
        }

        [TestMethod]
        public void Build_WithAllOptions_EmitsInCorrectOrder()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var expectedContent = $@"{ThemeOverrideBuilder.TopOverrideStartMarker}
@import url('on.css');
@import url('/Styles/style-v2/icons/tabler-icon.css');
{ThemeOverrideBuilder.TopOverrideEndMarker}


{ThemeOverrideBuilder.BottomOverrideStartMarker}
:root {{
    --test-var: none;
}}

div {{ display: none; }}
{ThemeOverrideBuilder.BottomOverrideEndMarker}";

                // Fix newlines to match what the builder uses.
                expectedContent = expectedContent
                    .Replace( "\r", string.Empty )
                    .Replace( "\n", System.Environment.NewLine );

                var themeJson = "{\"name\": \"TestTheme\", \"availableIconSets\": [\"tabler\"]}";
                var themeDefinition = ThemeDefinition.Parse( themeJson );

                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

                builder.AddVariable( "test-var", "none" );
                builder.AddImport( "on.css" );
                builder.AddFontIconSets( themeDefinition, new ThemeCustomizationSettings() );
                builder.AddCustomContent( "div { display: none; }" );

                var content = builder.Build( string.Empty ).Trim();

                Assert.That.AreEqual( expectedContent, content );
            }
        }

        [TestMethod]
        public void Build_WithNoOptions_PreservesUserContent()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var expectedContent = @".body { color: white; }";
                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

                var content = builder.Build( expectedContent ).Trim();

                Assert.That.Contains( content, expectedContent );
            }
        }

        [TestMethod]
        public void Build_WithNoOptionsAndOldMarkers_StripsOldMarkers()
        {
            using ( TestHelper.CreateScopedRockApp() )
            {
                var expectedContent = @".body { color: white; }";
                var originalContent = $@"{ThemeOverrideBuilder.TopOverrideStartMarker}
@import url('on.css');
@import url('/Styles/style-v2/icons/tabler-icon.css');
{ThemeOverrideBuilder.TopOverrideEndMarker}

{expectedContent}

{ThemeOverrideBuilder.BottomOverrideStartMarker}
:root {{
    --test-var: none;
}}

div {{ display: none; }}
{ThemeOverrideBuilder.BottomOverrideEndMarker}";

                var builder = new ThemeOverrideBuilder( "TestTheme", new Dictionary<string, string>() );

                var content = builder.Build( originalContent ).Trim();

                Assert.That.Contains( content, expectedContent );
                Assert.That.DoesNotContain( content, ThemeOverrideBuilder.TopOverrideStartMarker );
                Assert.That.DoesNotContain( content, ThemeOverrideBuilder.TopOverrideEndMarker );
                Assert.That.DoesNotContain( content, ThemeOverrideBuilder.BottomOverrideStartMarker );
                Assert.That.DoesNotContain( content, ThemeOverrideBuilder.BottomOverrideEndMarker );
            }
        }

        #endregion
    }
}
