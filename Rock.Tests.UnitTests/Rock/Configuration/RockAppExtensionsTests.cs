using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Configuration
{
    [TestClass]
    public class RockAppExtensionsTests
    {
        #region ResolveRockUrl

        [TestMethod]
        public void ResolveRockUrl_BareTilde_ReturnsRootFolderWithSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/";

                var actualValue = appScope.App.ResolveRockUrl( "~" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithoutThemeOrPath_ReturnsRootFolderWithSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/";

                var actualValue = appScope.App.ResolveRockUrl( "~/" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithoutThemeButWithPath_ReturnsRootFolderAndPath()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/abc";

                var actualValue = appScope.App.ResolveRockUrl( "~/abc" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithoutThemeButWithPathAndTrailingSlash_IncludesTrailingSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/abc/";

                var actualValue = appScope.App.ResolveRockUrl( "~/abc/" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_BareDoubleTilde_ReturnsThemeFolderWithoutSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock";

                var actualValue = appScope.App.ResolveRockUrl( "~~" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithoutTheme_ReturnsRockAsDefaultTheme()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock";

                var actualValue = appScope.App.ResolveRockUrl( "~~" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithThemeButWithoutPath_ReturnsThemeFolderWithSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock/";

                var actualValue = appScope.App.ResolveRockUrl( "~~/" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithThemeAndPath_ReturnsthemeFolderAndPath()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock/abc";

                var actualValue = appScope.App.ResolveRockUrl( "~~/abc" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithThemeAndPathAndTrailingSlash_IncludesTrailingSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock/abc/";

                var actualValue = appScope.App.ResolveRockUrl( "~~/abc/" );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithSpecifiedTheme_UsesCustomThemeName()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var customTheme = "MyCustomTheme";
                var expectedValue = $"/Themes/{customTheme}/abc/";

                var actualValue = appScope.App.ResolveRockUrl( "~~/abc/", customTheme );

                Assert.That.AreEqual( expectedValue, actualValue );
            }
        }

        #endregion
    }
}
