//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using NUnit.Framework;
using Rock.Model;

namespace Rock.Tests.Model
{
    [TestFixture]
    public class HtmlContentTests
    {
        public class TheCopyPropertiesFromMethod
        {
            [Test]
            public void ShouldCopyEntity()
            {
                var html = new HtmlContent { Content = "Foo" };
                var result = new HtmlContent();
                result.CopyPropertiesFrom( html );
                Assert.AreEqual( result.Content, html.Content );
            }
        }

        public class TheToJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var html = new HtmlContent { Content = "Foo" };
                var result = html.ToJson();
                Assert.IsNotEmpty( result );
            }
        }

        public class TheFromJsonMethod
        {
            [Test]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new HtmlContent
                {
                    EntityValue = "Some Value",
                    IsApproved = true
                };

                var json = obj.ToJson();
                var htmlContent = HtmlContent.FromJson( json );
                Assert.AreEqual( obj.EntityValue, htmlContent.EntityValue );
                Assert.AreEqual( obj.IsApproved, htmlContent.IsApproved );
            }
        }
    }
}
