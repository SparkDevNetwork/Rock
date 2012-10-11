//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock.Cms;
using Xunit;

namespace Rock.Tests.Cms
{
    public class PageRouteTests
    {
        public class TheExportObjectMethod
        {
            [Fact]
            public void ShouldCopyEntity()
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
