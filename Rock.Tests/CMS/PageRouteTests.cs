using System;
using Rock.CMS;
using Xunit;

namespace Rock.Tests.CMS
{
    public class PageRouteTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyDTO()
            {
                var pageRoute = new PageRoute { Guid = Guid.NewGuid() };
                dynamic result = pageRoute.ExportObject();
                Assert.Equal( result.Guid, pageRoute.Guid );
            }
        }

        public class TheExportJsonMethod
        {
            [Fact]
            public void ShouldNotBeEmpty()
            {
                var pageRoute = new PageRoute { Guid = Guid.NewGuid() };
                dynamic result = pageRoute.ExportJson();
                Assert.NotEmpty( result );
            }
        }
    }
}
