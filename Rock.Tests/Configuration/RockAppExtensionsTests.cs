using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Tests.Shared;

namespace Rock.Tests.Configuration
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

        #region MapPath

        [TestMethod]
        [DataRow( "~", @"D:\RockWeb\" )]
        [DataRow( "~/", @"D:\RockWeb\" )]
        [DataRow( "~/Obsidian", @"D:\RockWeb\Obsidian" )]
        [DataRow( "~/Obsidian/", @"D:\RockWeb\Obsidian\" )]
        [DataRow( "~/Obsidian/test.txt", @"D:\RockWeb\Obsidian\test.txt" )]
        [DataRow( "~~", @"D:\RockWeb\Themes\Rock\" )]
        [DataRow( "~~/", @"D:\RockWeb\Themes\Rock\" )]
        [DataRow( "~~/Styles", @"D:\RockWeb\Themes\Rock\Styles" )]
        [DataRow( "~~/Styles/theme.css", @"D:\RockWeb\Themes\Rock\Styles\theme.css" )]
        public void MapPath_TranslatesPaths_Correctly( string source, string expected )
        {
            void configureApp( ServiceCollection services )
            {
                var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

                hostingMock.Setup( a => a.WebRootPath )
                    .Returns( "D:\\RockWeb" );

                services.AddSingleton( hostingMock.Object );
            }

            using ( var appScope = TestHelper.CreateScopedRockApp( configureApp ) )
            {
                var actual = appScope.App.MapPath( source, "Rock" );

                Assert.AreEqual( expected, actual );
            }
        }

        [TestMethod]
        public void MapPath_NullPath_ReturnsNull()
        {
            void configureApp( ServiceCollection services )
            {
                var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

                hostingMock.Setup( a => a.WebRootPath )
                    .Returns( "D:\\RockWeb" );

                services.AddSingleton( hostingMock.Object );
            }

            using ( var appScope = TestHelper.CreateScopedRockApp( configureApp ) )
            {
                var actual = appScope.App.MapPath( null, "Rock" );

                Assert.IsNull( actual );
            }
        }

        [TestMethod]
        public void MapPath_EmptyPath_ReturnsEmpty()
        {
            void configureApp( ServiceCollection services )
            {
                var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

                hostingMock.Setup( a => a.WebRootPath )
                    .Returns( "D:\\RockWeb" );

                services.AddSingleton( hostingMock.Object );
            }

            using ( var appScope = TestHelper.CreateScopedRockApp( configureApp ) )
            {
                var actual = appScope.App.MapPath( string.Empty, "Rock" );

                Assert.IsEmpty( actual );
            }
        }

        [TestMethod]
        public void MapPath_WithoutTilde_ReturnsOriginalPath()
        {
            void configureApp( ServiceCollection services )
            {
                var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

                hostingMock.Setup( a => a.WebRootPath )
                    .Returns( "D:\\RockWeb" );

                services.AddSingleton( hostingMock.Object );
            }

            using ( var appScope = TestHelper.CreateScopedRockApp( configureApp ) )
            {
                var actual = appScope.App.MapPath( "test", "Rock" );

                Assert.AreEqual( "test", actual );
            }
        }

        [TestMethod]
        public void MapPath_WithoutTheme_UsesRockForTheme()
        {
            void configureApp( ServiceCollection services )
            {
                var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

                hostingMock.Setup( a => a.WebRootPath )
                    .Returns( "D:\\RockWeb" );

                services.AddSingleton( hostingMock.Object );
            }

            using ( var appScope = TestHelper.CreateScopedRockApp( configureApp ) )
            {
                var actual = appScope.App.MapPath( "~~/test", "Rock" );

                Assert.AreEqual( @"D:\RockWeb\Themes\Rock\test", actual );
            }
        }

        [TestMethod]
        public void MapPath_WithTheme_UsesSpecifiedTheme()
        {
            void configureApp( ServiceCollection services )
            {
                var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

                hostingMock.Setup( a => a.WebRootPath )
                    .Returns( "D:\\RockWeb" );

                services.AddSingleton( hostingMock.Object );
            }

            using ( var appScope = TestHelper.CreateScopedRockApp( configureApp ) )
            {
                var actual = appScope.App.MapPath( "~~/test", "TestTheme" );

                Assert.AreEqual( @"D:\RockWeb\Themes\TestTheme\test", actual );
            }
        }

        #endregion
    }
}
