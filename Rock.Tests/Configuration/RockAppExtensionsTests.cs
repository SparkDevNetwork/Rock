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

        [TestMethod]
        [DataRow( "", "" )]
        [DataRow( " ", "" )]
        [DataRow( "\t", "" )]
        [DataRow( "\r\n", "" )]
        [DataRow( "\n~/page/1\n", "/page/1" )]
        [DataRow( "  ~~/Styles/x.css  ", "/Themes/Rock/Styles/x.css" )]
        [DataRow( " https://example.com/x ", "https://example.com/x" )]
        [DataRow( "~\\", "/" )]
        [DataRow( "~\\abc\\def", "/abc/def" )]
        [DataRow( "~//abc", "/abc" )]
        [DataRow( "~/abc//def/", "/abc/def/" )]
        [DataRow( "~/./abc", "/abc" )]
        [DataRow( "~/abc/../def", "/def" )]
        [DataRow( "~/abc/..", "/" )]
        [DataRow( "~~/../abc", "/Themes/abc" )]
        [DataRow( "~?a=1", "/?a=1" )]
        [DataRow( "~~#top", "/Themes/Rock#top" )]
        [DataRow( "~/page/1?returnUrl=/a/../b", "/page/1?returnUrl=/a/../b" )]
        [DataRow( "~/a/../b#c/../d", "/b#c/../d" )]
        [DataRow( "~/a b%20c", "/a b%20c" )]
        [DataRow( "mailto:a@example.com", "mailto:a@example.com" )]
        [DataRow( "/", "/" )]
        [DataRow( "/a/../b", "/a/../b" )]
        [DataRow( "http://example.com/a/../b", "http://example.com/a/../b" )]
        [DataRow( "//cdn.example.com/x.js", "//cdn.example.com/x.js" )]
        public void ResolveRockUrl_TranslatesUrls_Correctly( string source, string expected )
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                var actual = appScope.App.ResolveRockUrl( source, "Rock" );

                Assert.AreEqual( expected, actual );
            }
        }

        [TestMethod]
        public void ResolveRockUrl_Null_ReturnsNull()
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                Assert.IsNull( appScope.App.ResolveRockUrl( null, "Rock" ) );
            }
        }

        [TestMethod]
        [DataRow( "~/.." )]
        [DataRow( "~/../outside" )]
        [DataRow( "~/abc/../../outside" )]
        [DataRow( "~~/../../../outside" )]
        public void ResolveRockUrl_AboveRoot_ThrowsArgumentException( string url )
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                Assert.ThrowsExactly<ArgumentException>( () => appScope.App.ResolveRockUrl( url, "Rock" ) );
            }
        }

        [TestMethod]
        [DataRow( "page/1" )]
        [DataRow( "../page/1" )]
        [DataRow( "./page/1" )]
        [DataRow( "x.txt" )]
        [DataRow( "~foo" )]
        [DataRow( "~foo/bar?a=1" )]
        [DataRow( "~~foo" )]
        [DataRow( "~~Styles/x.css" )]
        [DataRow( "?a=1" )]
        [DataRow( "#top" )]
        [DataRow( "#" )]
        [DataRow( "?a=1#top" )]
        public void ResolveRockUrl_RelativeUrl_ThrowsArgumentException( string url )
        {
            using ( var appScope = TestHelper.CreateScopedRockApp() )
            {
                Assert.ThrowsExactly<ArgumentException>( () => appScope.App.ResolveRockUrl( url, "Rock" ) );
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
