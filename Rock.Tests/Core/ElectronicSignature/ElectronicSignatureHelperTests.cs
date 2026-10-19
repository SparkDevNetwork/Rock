using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.ElectronicSignature;
using Rock.Model;

namespace Rock.Tests.Core.ElectronicSignature
{
    /// <summary>
    /// Tests for <see cref="ElectronicSignatureHelper"/>.
    /// </summary>
    [TestClass]
    public class ElectronicSignatureHelperTests
    {
        #region IsValidDrawnSignatureDataUrl

        [TestMethod]
        public void IsValidDrawnSignatureDataUrl_WithSampleSignature_ReturnsTrue()
        {
            Assert.IsTrue( ElectronicSignatureHelper.IsValidDrawnSignatureDataUrl( ElectronicSignatureHelper.SampleSignatureDataURL ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "https://evil.test/x.png" )]
        [DataRow( "data:image/png;base64,AAAA'onerror='x" )]
        [DataRow( "data:image/svg+xml;base64,AAAA" )]
        [DataRow( "data:image/png;base64,AAAA\n" )]
        [DataRow( "data:image/png,AAAA" )]
        public void IsValidDrawnSignatureDataUrl_WithInvalidValue_ReturnsFalse( string dataUrl )
        {
            Assert.IsFalse( ElectronicSignatureHelper.IsValidDrawnSignatureDataUrl( dataUrl ) );
        }

        #endregion

        #region GetSignatureInformationHtml

        [TestMethod]
        public void GetSignatureInformationHtml_WithDrawnRemoteUrl_OmitsImage()
        {
            var html = ElectronicSignatureHelper.GetSignatureInformationHtml( new GetSignatureInformationHtmlOptions
            {
                SignatureType = SignatureType.Drawn,
                SignedName = "Ted Decker",
                DrawnSignatureDataUrl = "https://evil.test/x.png"
            } );

            Assert.IsFalse( html.Contains( "evil.test" ) );
            Assert.IsFalse( html.Contains( "<img" ) );
        }

        [TestMethod]
        public void GetSignatureInformationHtml_WithDrawnAttributeBreakout_OmitsImage()
        {
            var html = ElectronicSignatureHelper.GetSignatureInformationHtml( new GetSignatureInformationHtmlOptions
            {
                SignatureType = SignatureType.Drawn,
                SignedName = "Ted Decker",
                DrawnSignatureDataUrl = "data:image/png;base64,AAAA' onerror='alert(1)"
            } );

            Assert.IsFalse( html.Contains( "onerror" ) );
        }

        [TestMethod]
        public void GetSignatureInformationHtml_WithDrawnSampleSignature_IncludesImage()
        {
            var html = ElectronicSignatureHelper.GetSignatureInformationHtml( new GetSignatureInformationHtmlOptions
            {
                SignatureType = SignatureType.Drawn,
                SignedName = "Ted Decker",
                DrawnSignatureDataUrl = ElectronicSignatureHelper.SampleSignatureDataURL
            } );

            StringAssert.Contains( html, "<img src='data:image/png;base64," );
        }

        [TestMethod]
        public void GetSignatureInformationHtml_WithMarkupInPersonName_EncodesName()
        {
            var html = ElectronicSignatureHelper.GetSignatureInformationHtml( new GetSignatureInformationHtmlOptions
            {
                SignatureType = SignatureType.Typed,
                SignedName = "Ted Decker",
                SignedByPerson = new Person
                {
                    NickName = "<b>N</b>",
                    LastName = "L"
                }
            } );

            StringAssert.Contains( html, "&lt;b&gt;N&lt;/b&gt; L" );
            Assert.IsFalse( html.Contains( "<b>N</b>" ) );
        }

        #endregion
    }
}
