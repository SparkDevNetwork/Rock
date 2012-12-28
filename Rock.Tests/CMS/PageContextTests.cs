//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using NUnit.Framework;
using Rock.Model;

namespace Rock.Tests.Cms
{
    [TestFixture]
    public class PageContextTests
    {
        public class TheExportObjectMethod
        {
            [Test]
            public void ShouldCopyEntity()
            {
                var pageContext = new PageContext { Guid = Guid.NewGuid() };
                var result = new PageContext();
                result.CopyPropertiesFrom( pageContext );
                Assert.AreEqual( result.Guid, pageContext.Guid );
            }
        }

        public class TheExportJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var pageContext = new PageContext() { Guid = Guid.NewGuid() };
                var result = pageContext.ToJson();
                Assert.IsNotEmpty( result );
            }
        }

        public class TheImportJsonMethod
        {
            [Test]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new PageContext()
                    {
                        Guid = Guid.NewGuid(),
                        IsSystem = false
                    };

                var json = obj.ToJson();
                var pageContext = PageContext.FromJson( json );
                Assert.AreEqual( obj.Guid, pageContext.Guid );
                Assert.AreEqual( obj.IsSystem, pageContext.IsSystem );
            }
        }
    }
}
