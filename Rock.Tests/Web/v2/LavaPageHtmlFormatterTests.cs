using System.Threading.Tasks;

using AngleSharp.Html.Parser;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageHtmlFormatterTests
    {
        [TestMethod]
        public void RendersRockZoneContentAsUnescapedHtml()
        {
            var expectedHtml = "<div class=\"test\">Hello World</div>";
            var parser = new HtmlParser();
            var document = parser.ParseDocument( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>" );

            var zone = document.QuerySelector( "rock\\:zone" );

            zone.AppendChild( document.CreateTextNode( expectedHtml ) );

            using ( var writer = new System.IO.StringWriter() )
            {
                document.DocumentElement.ToHtml( writer, new LavaPageHtmlFormatter() );

                var html = writer.ToString();

                Assert.Contains( expectedHtml, html );
            }
        }

        [TestMethod]
        public void RendersZoneNameContainer()
        {
            var expectedHtml = "<div id=\"zone-main\" class=\"zone-instance\">";
            var parser = new HtmlParser();
            var document = parser.ParseDocument( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>" );

            var zone = document.QuerySelector( "rock\\:zone" );

            zone.AppendChild( document.CreateTextNode( "<div class=\"test\">Hello World</div>" ) );

            using ( var writer = new System.IO.StringWriter() )
            {
                document.DocumentElement.ToHtml( writer, new LavaPageHtmlFormatter() );

                var html = writer.ToString();

                Assert.Contains( expectedHtml, html );
            }
        }

        [TestMethod]
        public void RendersZoneContentContainer()
        {
            var expectedHtml = "<div class=\"zone-content\">";
            var parser = new HtmlParser();
            var document = parser.ParseDocument( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>" );

            var zone = document.QuerySelector( "rock\\:zone" );

            zone.AppendChild( document.CreateTextNode( "<div class=\"test\">Hello World</div>" ) );

            using ( var writer = new System.IO.StringWriter() )
            {
                document.DocumentElement.ToHtml( writer, new LavaPageHtmlFormatter() );

                var html = writer.ToString();

                Assert.Contains( expectedHtml, html );
            }
        }

        [TestMethod]
        public void EscapesTextNodeOutsideZone()
        {
            var expectedHtml = "&lt;div class=\"test\"&gt;Hello World&lt;/div&gt;";
            var parser = new HtmlParser();
            var document = parser.ParseDocument( "<html><body></body></html>" );

            var body = document.QuerySelector( "body" );

            body.AppendChild( document.CreateTextNode( "<div class=\"test\">Hello World</div>" ) );

            using ( var writer = new System.IO.StringWriter() )
            {
                document.DocumentElement.ToHtml( writer, new LavaPageHtmlFormatter() );

                var html = writer.ToString();

                Assert.Contains( expectedHtml, html );
            }
        }

        [TestMethod]
        public void EscapesTextNodeWithoutParent()
        {
            var expectedHtml = "&lt;div class=\"test\"&gt;Hello World&lt;/div&gt;";
            var parser = new HtmlParser();
            var document = parser.ParseDocument( "<html><body></body></html>" );

            var node = document.CreateTextNode( "<div class=\"test\">Hello World</div>" );

            using ( var writer = new System.IO.StringWriter() )
            {
                node.ToHtml( writer, new LavaPageHtmlFormatter() );

                var html = writer.ToString();

                Assert.Contains( expectedHtml, html );
            }
        }
    }
}
