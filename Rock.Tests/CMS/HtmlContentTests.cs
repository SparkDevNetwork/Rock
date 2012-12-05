//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using NUnit.Framework;
using Rock.Model;

namespace Rock.Tests.Cms
{
    [TestFixture]
    public class HtmlContentTests
    {
        public class TheExportObjectMethod
        {
            [Test]
            public void ShouldCopyEntity()
            {
                var html = new HtmlContent() { Content = "Foo" };
                dynamic result = html.ExportObject();
                Assert.AreEqual( result.Content, html.Content );
            }
        }

        public class TheExportJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var html = new HtmlContent() { Content = "Foo" };
                var result = html.ExportJson();
                Assert.IsNotEmpty( result );
            }
        }

        public class TheImportJsonMethod
        {
            [Test]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new
                {
                    EntityValue = "Some Value",
                    IsApproved = true
                };

                var json = obj.ToJSON();
                var htmlContent = new HtmlContent();
                htmlContent.ImportJson( json );
                Assert.AreEqual( obj.EntityValue, htmlContent.EntityValue );
                Assert.AreEqual( obj.IsApproved, htmlContent.IsApproved );
            }
        }
    }
}
