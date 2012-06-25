using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Xunit;
using Rock.CMS;

namespace Rock.Tests.CMS
{
    public class PageTests
    {
        public class TheExportMethod
        {
            [Fact]
            public void ShouldNotBeEmpty()
            {
                var page = new Page();
                var result = page.Export();
                Assert.NotEmpty(result);
            }

            [Fact]
            public void ShouldExportAsXml()
            {
                var page = new Page()
                {
                    Title = "FooPage"
                };
                var result = page.Export();
                Assert.Contains("<Title>FooPage</Title>", result);
            }
        }

        public class TheImportMethod
        {

        }
    }
}
