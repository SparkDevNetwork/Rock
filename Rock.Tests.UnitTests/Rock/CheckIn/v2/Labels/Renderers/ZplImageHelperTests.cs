using System.IO;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2.Labels;
using Rock.CheckIn.v2.Labels.Renderers;
using Rock.Configuration;
using Rock.Tests.Shared;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Labels.Renderers
{
    [TestClass]
    public class ZplImageHelperTests
    {
        #region CreateImage

        [TestMethod]
        public void CreateImage_WithTransparentImage_ReturnsNoFillBits()
        {
            var image = new Image<Rgba32>( 24, 24, new Rgba32( 0, 0, 0, 0 ) );
            var options = new ZplImageOptions
            {
                Width = 24,
                Height = 24
            };

            using ( var ms = new MemoryStream() )
            {
                image.SaveAsPng( ms );
                ms.Position = 0;

                var imageCache = ZplImageHelper.CreateImage( ms, options );
                var isAllZeroBits = imageCache.ImageData.All( d => d == 0 );

                Assert.That.IsTrue( isAllZeroBits );
            }
        }

        [TestMethod]
        public void CreateImage_WithWidthAsMultipleOf8_DoesNotIncludePadding()
        {
            var expectedDataSize = 24 * 24 / 8;
            var image = new Image<Rgba32>( 24, 24, new Rgba32( 0, 0, 0, 0 ) );
            var options = new ZplImageOptions
            {
                Width = 24,
                Height = 24
            };

            using ( var ms = new MemoryStream() )
            {
                image.SaveAsPng( ms );
                ms.Position = 0;

                var imageCache = ZplImageHelper.CreateImage( ms, options );

                Assert.That.AreEqual( expectedDataSize, imageCache.ImageData.Length );
            }
        }

        [TestMethod]
        public void CreateImage_WithWidthAsNotMultipleOf8_IncludesPadding()
        {
            // It is expected that the width of 20 be rounded up to 24 for padding.
            var expectedDataSize = 24 * 20 / 8;
            var image = new Image<Rgba32>( 20, 20, new Rgba32( 0, 0, 0, 0 ) );
            var options = new ZplImageOptions
            {
                Width = 20,
                Height = 20
            };

            using ( var ms = new MemoryStream() )
            {
                image.SaveAsPng( ms );
                ms.Position = 0;

                var imageCache = ZplImageHelper.CreateImage( ms, options );

                Assert.That.AreEqual( expectedDataSize, imageCache.ImageData.Length );
            }
        }

        [TestMethod]
        public void CreateImage_WithTallResizeRequest_ReturnsRequestedSize()
        {
            var expectedWidth = 24;
            var expectedHeight = 32;
            var expectedDataSize = expectedWidth * expectedHeight / 8;
            var image = new Image<Rgba32>( 100, 100, new Rgba32( 0, 0, 0, 0 ) );
            var options = new ZplImageOptions
            {
                Width = expectedWidth,
                Height = expectedHeight
            };

            using ( var ms = new MemoryStream() )
            {
                image.SaveAsPng( ms );
                ms.Position = 0;

                var imageCache = ZplImageHelper.CreateImage( ms, options );

                Assert.That.AreEqual( expectedDataSize, imageCache.ImageData.Length );
                Assert.That.AreEqual( expectedWidth, imageCache.Width );
                Assert.That.AreEqual( expectedHeight, imageCache.Height );
            }
        }

        [TestMethod]
        public void CreateImage_WithWideResizeRequest_ReturnsRequestedSize()
        {
            var expectedWidth = 32;
            var expectedHeight = 24;
            var expectedDataSize = expectedWidth * expectedHeight / 8;
            var image = new Image<Rgba32>( 100, 100, new Rgba32( 0, 0, 0, 0 ) );
            var options = new ZplImageOptions
            {
                Width = expectedWidth,
                Height = expectedHeight
            };

            using ( var ms = new MemoryStream() )
            {
                image.SaveAsPng( ms );
                ms.Position = 0;

                var imageCache = ZplImageHelper.CreateImage( ms, options );

                Assert.That.AreEqual( expectedDataSize, imageCache.ImageData.Length );
                Assert.That.AreEqual( expectedWidth, imageCache.Width );
                Assert.That.AreEqual( expectedHeight, imageCache.Height );
            }
        }

        [TestMethod]
        public void CreateImage_WithWhiteImageAndBrightnessOf0_ReturnsAllFillBits()
        {
            var image = new Image<Rgba32>( 24, 24, new Rgba32( 255, 255, 255, 255 ) );
            var options = new ZplImageOptions
            {
                Width = 24,
                Height = 24,
                Brightness = 0
            };

            using ( var ms = new MemoryStream() )
            {
                image.SaveAsPng( ms );
                ms.Position = 0;

                var imageCache = ZplImageHelper.CreateImage( ms, options );

                var isAllOneBits = imageCache.ImageData.All( d => d == 255 );

                Assert.That.IsTrue( isAllOneBits );
            }
        }

        #endregion

        #region CreateIconFontCollection

        [TestMethod]
        public void CreateIconFontCollection_WithValidPath_LoadsAllFonts()
        {
            using ( var scope = TestHelper.CreateScopedRockApp() )
            {
                var expectedFontCount = 2;
                var fontCollection = ZplImageHelper.CreateIconFontCollection();

                Assert.That.AreEqual( expectedFontCount, fontCollection.Families.Count() );
            }
        }

        [TestMethod]
        public void CreateIconFontCollection_WithInvalidPath_DoesNotThrow()
        {
            var expectedFontCount = 0;

            var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );
            var badDirectory = Path.Combine( Directory.GetCurrentDirectory(), "BadDirectory" );

            hostingMock.Setup( a => a.WebRootPath ).Returns( badDirectory );

            using ( var scope = TestHelper.CreateScopedRockApp( cfg => cfg.AddSingleton( hostingMock.Object ) ) )
            {
                var fontCollection = ZplImageHelper.CreateIconFontCollection();

                Assert.That.AreEqual( expectedFontCount, fontCollection.Families.Count() );
            }
        }

        #endregion

        #region CreateIcon

        [TestMethod]
        public void CreateIcon_WithMissingFonts_ReturnsNoFillBits()
        {
            ZplImageHelper.ClearIconFontCache();

            var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );
            var badDirectory = Path.Combine( Directory.GetCurrentDirectory(), "BadDirectory" );

            hostingMock.Setup( a => a.WebRootPath ).Returns( badDirectory );

            using ( var scope = TestHelper.CreateScopedRockApp( cfg => cfg.AddSingleton( hostingMock.Object ) ) )
            {
                var icon = new LabelIcon( "undefined", "undefined", "\uF005", true ); // fa-star
                var image = ZplImageHelper.CreateIcon( 8, 8, icon );

                var isAllNoFillBits = image.ImageData.All( d => d == 0 );

                Assert.That.IsTrue( isAllNoFillBits );
            }
        }

        [TestMethod]
        public void CreateIcon_WithBoldIcon_ReturnsSomeFillBits()
        {
            ZplImageHelper.ClearIconFontCache();

            using ( var scope = TestHelper.CreateScopedRockApp() )
            {
                var icon = new LabelIcon( "undefined", "undefined", "\uF005", true ); // fa-star
                var image = ZplImageHelper.CreateIcon( 8, 8, icon );

                var isSomeFillBits = image.ImageData.Any( d => d != 0 );

                Assert.That.IsTrue( isSomeFillBits );
            }
        }

        [TestMethod]
        public void CreateIcon_WithRegularIcon_ReturnsSomeFillBits()
        {
            ZplImageHelper.ClearIconFontCache();

            using ( var scope = TestHelper.CreateScopedRockApp() )
            {
                var icon = new LabelIcon( "undefined", "undefined", "\uF005", false ); // fa-star
                var image = ZplImageHelper.CreateIcon( 8, 8, icon );

                var isSomeFillBits = image.ImageData.Any( d => d != 0 );

                Assert.That.IsTrue( isSomeFillBits );
            }
        }

        #endregion
    }
}
