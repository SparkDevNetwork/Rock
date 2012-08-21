//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.CMS;
using Xunit;

namespace Rock.Tests.CMS
{
    public class HtmlContentTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyEntity()
            {
                var html = new HtmlContent() { Content = "Foo" };
                dynamic result = html.ExportObject();
                Assert.Equal( result.Content, html.Content );
            }
        }

        public class TheExportJsonMethod
        {
            [Fact]
            public void ShouldNotBeEmpty()
            {
                var html = new HtmlContent() { Content = "Foo" };
                var result = html.ExportJson();
                Assert.NotEmpty( result );
            }
        }
    }
}
