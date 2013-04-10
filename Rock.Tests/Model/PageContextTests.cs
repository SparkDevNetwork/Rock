//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using NUnit.Framework;
using Rock.Model;

namespace Rock.Tests.Model
{
    [TestFixture]
    public class PageContextTests
    {
        public class TheCopyPropertiesFromMethod
        {
            [Test]
            public void ShouldCopyEntity()
            {
                var pageContext = new PageContext { Guid = Guid.NewGuid() };
                var result = pageContext.Clone( false );
                Assert.AreEqual( result.Guid, pageContext.Guid );
            }
        }

        public class TheToJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var pageContext = new PageContext { Guid = Guid.NewGuid() };
                var result = pageContext.ToJson();
                Assert.IsNotEmpty( result );
            }

            [Test]
            public void ShouldExportAsJson()
            {
                var guid = Guid.NewGuid();
                var pageContext = new PageContext
                {
                    Guid = guid
                };
                var result = pageContext.ToJson();
                var key = string.Format( "\"Guid\": \"{0}\"", guid );
                Assert.Greater( result.IndexOf( key ), -1, string.Format( "'{0}' was not found in '{1}'.", key, result ) );
            }
        }

        public class TheFromJsonMethod
        {
            [Test]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new PageContext
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
