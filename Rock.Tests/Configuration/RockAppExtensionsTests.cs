using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

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

                Assert.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithoutThemeOrPath_ReturnsRootFolderWithSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/";

                var actualValue = appScope.App.ResolveRockUrl( "~/" );

                Assert.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithoutThemeButWithPath_ReturnsRootFolderAndPath()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/abc";

                var actualValue = appScope.App.ResolveRockUrl( "~/abc" );

                Assert.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithoutThemeButWithPathAndTrailingSlash_IncludesTrailingSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/abc/";

                var actualValue = appScope.App.ResolveRockUrl( "~/abc/" );

                Assert.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_BareDoubleTilde_ReturnsThemeFolderWithoutSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock";

                var actualValue = appScope.App.ResolveRockUrl( "~~" );

                Assert.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithoutTheme_ReturnsRockAsDefaultTheme()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock";

                var actualValue = appScope.App.ResolveRockUrl( "~~" );

                Assert.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithThemeButWithoutPath_ReturnsThemeFolderWithSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock/";

                var actualValue = appScope.App.ResolveRockUrl( "~~/" );

                Assert.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithThemeAndPath_ReturnsthemeFolderAndPath()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock/abc";

                var actualValue = appScope.App.ResolveRockUrl( "~~/abc" );

                Assert.AreEqual( expectedValue, actualValue );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_WithThemeAndPathAndTrailingSlash_IncludesTrailingSlash()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var expectedValue = "/Themes/Rock/abc/";

                var actualValue = appScope.App.ResolveRockUrl( "~~/abc/" );

                Assert.AreEqual( expectedValue, actualValue );
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

                Assert.AreEqual( expectedValue, actualValue );
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
        [DataRow( "/", @"D:\RockWeb\" )]
        [DataRow( "/Obsidian", @"D:\RockWeb\Obsidian" )]
        [DataRow( "/Obsidian/", @"D:\RockWeb\Obsidian\" )]
        [DataRow( "/Obsidian/test.txt", @"D:\RockWeb\Obsidian\test.txt" )]
        [DataRow( "~\\", @"D:\RockWeb\" )]
        [DataRow( "~\\Obsidian\\test.txt", @"D:\RockWeb\Obsidian\test.txt" )]
        [DataRow( "~/Obsidian//test.txt", @"D:\RockWeb\Obsidian\test.txt" )]
        [DataRow( "~//Obsidian", @"D:\RockWeb\Obsidian" )]
        [DataRow( "//Obsidian", @"D:\RockWeb\Obsidian" )]
        [DataRow( "/Obsidian/\\test.txt", @"D:\RockWeb\Obsidian\test.txt" )]
        [DataRow( "~/Obsidian/./test.txt", @"D:\RockWeb\Obsidian\test.txt" )]
        [DataRow( "~/Obsidian/sub/../test.txt", @"D:\RockWeb\Obsidian\test.txt" )]
        [DataRow( "~/Obsidian/..", @"D:\RockWeb" )]
        [DataRow( "~/Obsidian/../", @"D:\RockWeb\" )]
        [DataRow( "~~/../Styles", @"D:\RockWeb\Themes\Styles" )]
        [DataRow( "~/Obsidian/a#b %20.txt", @"D:\RockWeb\Obsidian\a#b %20.txt" )]
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
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( " " )]
        [DataRow( "\t" )]
        public void MapPath_BlankPath_ThrowsArgumentNullException( string path )
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
                Assert.ThrowsExactly<ArgumentNullException>( () => appScope.App.MapPath( path, "Rock" ) );
            }
        }

        [TestMethod]
        [DataRow( "~/Obsidian/file?query=1" )]
        [DataRow( "~/Obsidian/a*b.txt" )]
        [DataRow( "~/Obsidian/a:b.txt" )]
        [DataRow( "~/a:b" )]
        [DataRow( "~/Obsidian/a<b>.txt" )]
        [DataRow( "~/Obsidian/a|b.txt" )]
        [DataRow( "~/Obsidian/a\"b.txt" )]
        [DataRow( "~/Obsidian/a\tb.txt" )]
        [DataRow( "~/Obsidian/a\0b.txt" )]
        [DataRow( "~~/a?b" )]
        [DataRow( "/a:b" )]
        public void MapPath_InvalidCharacters_ThrowsArgumentException( string path )
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
                Assert.ThrowsExactly<ArgumentException>( () => appScope.App.MapPath( path, "Rock" ) );
            }
        }

        [TestMethod]
        [DataRow( "~/.." )]
        [DataRow( "~/../outside" )]
        [DataRow( "~/Obsidian/../../outside" )]
        [DataRow( "/../outside" )]
        [DataRow( "~~/../../../outside" )]
        public void MapPath_AboveRoot_ThrowsArgumentException( string path )
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
                Assert.ThrowsExactly<ArgumentException>( () => appScope.App.MapPath( path, "Rock" ) );
            }
        }

        [TestMethod]
        [DataRow( "test" )]
        [DataRow( "./test" )]
        [DataRow( "../test" )]
        [DataRow( "Content/test" )]
        [DataRow( "C:\\RockWeb\\test" )]
        [DataRow( "\\\\server\\share\\test" )]
        [DataRow( "http://example.com/test" )]
        [DataRow( "~test" )]
        [DataRow( "~Obsidian/test.txt" )]
        [DataRow( "~~test" )]
        [DataRow( "~~~" )]
        public void MapPath_WithoutTildeOrSlash_ThrowsArgumentException( string path )
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
                Assert.ThrowsExactly<ArgumentException>( () => appScope.App.MapPath( path, "Rock" ) );
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
