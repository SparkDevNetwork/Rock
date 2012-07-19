using Rock.CMS;
using Xunit;

namespace Rock.Tests.CMS
{
    public class HtmlContentTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyDTO()
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
