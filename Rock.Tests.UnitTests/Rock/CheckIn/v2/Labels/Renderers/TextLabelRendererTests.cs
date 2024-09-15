using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.CheckIn.v2.Labels;
using Rock.CheckIn.v2.Labels.Renderers;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.CheckIn.v2.Labels.Renderers
{
    /// <summary>
    /// Unit tests for <see cref="TextLabelRenderer"/>.
    /// </summary>
    [TestClass]
    public class TextLabelRendererTests
    {
        [TestMethod]
        public void Dispose_LeavesOriginalStreamOpen()
        {
            var renderer = new Mock<TextLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            var request = new PrintLabelRequest();

            using ( var stream = new MemoryStream() )
            {
                renderer.Object.BeginLabel( stream, request );
                renderer.Object.EndLabel();
                renderer.Object.Dispose();

                Assert.That.IsTrue( stream.CanRead, "Stream was closed." );
            }
        }

        [TestMethod]
        public void BeginLabel_CalledTwiceWithoutEndLabel_ThrowsException()
        {
            var renderer = new Mock<TextLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            var request1 = new PrintLabelRequest();
            var request2 = new PrintLabelRequest();

            using ( var stream1 = new MemoryStream() )
            {
                renderer.Object.BeginLabel( stream1, request1 );

                using ( var stream2 = new MemoryStream() )
                {
                    Assert.That.ThrowsException<InvalidOperationException>( () => renderer.Object.BeginLabel( stream2, request2 ) );
                }
            }
        }

        [TestMethod]
        public void BeginLabel_WithDefaults_CreatesTextWriterWithoutBom()
        {
            var utf8ByteOrderMark = 0xEF;

            var request = new PrintLabelRequest();
            var renderer = new Mock<TextLabelRenderer>( MockBehavior.Loose )
            {
                CallBase = true
            };

            using ( var stream = new MemoryStream() )
            {
                renderer.Object.BeginLabel( stream, request );
                renderer.Object.Dispose();

                stream.Position = 0;
                var actualByte = stream.ReadByte();

                Assert.That.AreNotEqual( utf8ByteOrderMark, actualByte, "Byte order mark was present." );
            }
        }
    }
}
