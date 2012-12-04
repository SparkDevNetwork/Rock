//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rock.Cms;

namespace Rock.Tests.Cms
{
    [TestFixture]
    public class PageTests
    {
        public class TheExportObjectMethod
        {
            [Test]
            public void ShouldCopyEntity()
            {
                var page = new Page() { Name = "SomePage" };
                dynamic result = page.ExportObject();
                Assert.AreEqual( result.Name, page.Name );
            }

            [Test]
            public void ShouldCopyPages()
            {
                var children = new List<Page>() { new Page() };
                var parent = new Page() { Pages = children };
                dynamic result = parent.ExportObject();
                Assert.IsNotEmpty( result.Pages );
            }

            [Test]
            public void ShouldCopyPagesRecursively()
            {
                var parent = new Page();
                var child = new Page();
                var grandchild = new Page();
                parent.Pages = new List<Page> { child };
                child.Pages = new List<Page> { grandchild };
                dynamic result = parent.ExportObject();
                Assert.IsNotEmpty( result.Pages );
                Assert.IsNotEmpty( result.Pages[ 0 ].Pages );
            }

            [Test]
            public void ShouldCopyBlocks()
            {
                var page = new Page() { Blocks = new List<Block>() };
                page.Blocks.Add( new Block() );
                dynamic result = page.ExportObject();
                Assert.NotNull( result.Blocks );
                Assert.IsNotEmpty( result.Blocks );
            }

            [Test]
            public void ShouldCopyPageRoutes()
            {
                var page = new Page() { PageRoutes = new List<PageRoute>() };
                page.PageRoutes.Add( new PageRoute());
                dynamic result = page.ExportObject();
                Assert.NotNull( result.PageRoutes );
                Assert.IsNotEmpty( result.PageRoutes );
            }

            [Test]
            public void ShouldCopyPageContexts()
            {
                var page = new Page() { PageContexts = new List<PageContext>() };
                page.PageContexts.Add( new PageContext() );
                dynamic result = page.ExportObject();
                Assert.NotNull( result.PageContexts );
                Assert.IsNotEmpty( result.PageContexts );
            }
        }

        public class TheExportJsonMethod
        {
            [Test]
            public void ShouldNotBeEmpty()
            {
                var page = new Page();
                var result = page.ExportJson();
                Assert.IsNotEmpty( result );
            }

            [Test]
            public void ShouldExportAsJson()
            {
                var page = new Page()
                {
                    Title = "FooPage"
                };
                var result = page.ExportJson();
                const string key = "\"Title\":\"FooPage\"";
                Assert.Greater( result.IndexOf( key ), -1 );
            }

            [Test]
            public void ShouldExportChildPages()
            {
                var page = new Page()
                {
                    Title = "FooPage",
                    Pages = new List<Page> { new Page { Title = "BarPage" } }
                };
                var result = page.ExportJson();
                result = result.Substring( result.IndexOf( "\"Pages\":" ) + 7 );
                const string key = "\"Title\":\"BarPage\"";
                Assert.Greater( result.IndexOf( key ), -1 );
            }

            [Test]
            public void ShouldExportChildPagesRecursively()
            {
                var parent = new Page() { Title = "Parent" };
                var child = new Page() { Title = "Child" };
                var grandchild = new Page() { Title = "Grandchild" };
                parent.Pages = new List<Page> { child };
                child.Pages = new List<Page> { grandchild };
                var result = parent.ExportJson();
                const string parentKey = "\"Title\":\"Parent\"";
                const string childKey = "\"Title\":\"Child\"";
                const string grandChildKey = "\"Title\":\"Grandchild\"";
                Assert.Greater( result.IndexOf( parentKey ), -1 );
                Assert.Greater( result.IndexOf( childKey ), -1 );
                Assert.Greater( result.IndexOf( grandChildKey ), -1 );
            }
        }

        public class TheImportJsonMethod
        {
            [Test]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new
                    {
                        Name = "Foo Page",
                        IsSystem = true
                    };

                var json = obj.ToJSON();
                var page = new Page();
                page.ImportJson( json );
                Assert.AreEqual( obj.Name, page.Name );
                Assert.AreEqual( obj.IsSystem, page.IsSystem );
            }

            [Test]
            public void ShouldImportChildPages()
            {
                var obj = new
                    {
                        Name = "Parent",
                        Pages = new List<dynamic> { new { Name = "Child" } }
                    };

                var json = obj.ToJSON();
                var page = new Page();
                page.ImportJson( json );
                Assert.IsNotNull( page.Pages );
                Assert.IsNotEmpty( page.Pages );
                Assert.AreEqual( page.Pages.First().Name, obj.Pages[0].Name );
            }

            [Test]
            public void ShouldImportPagesRecursively()
            {
                const string PAGE_NAME = "Child Page";
                var obj = new
                    {
                        Name = "Grandparent Page",
                        Pages = new List<dynamic>
                            {
                                new
                                    {
                                        Name = "Parent Page",
                                        Pages = new List<dynamic> { new { Name = PAGE_NAME } }
                                    }
                            }
                    };

                var json = obj.ToJSON();
                var page = new Page();
                page.ImportJson( json );
                var childPages = page.Pages.First().Pages;
                Assert.IsNotNull( childPages );
                Assert.IsNotEmpty( childPages );
                Assert.AreEqual( childPages.First().Name, PAGE_NAME );
            }

            [Test]
            public void ShouldImportBlocks()
            {
                var obj = new
                    {
                        Name = "Some Page",
                        Blocks = new List<dynamic> { new { Name = "Some Block" } }
                    };

                var json = obj.ToJSON();
                var page = new Page();
                page.ImportJson( json );
                Assert.IsNotNull( page.Blocks );
                Assert.IsNotEmpty( page.Blocks );
                Assert.AreEqual( page.Blocks.First().Name, obj.Blocks[0].Name );
            }

            [Test]
            public void ShouldImportPageRoutes()
            {
                var obj = new
                    {
                        Name = "Some Page",
                        PageRoutes = new List<dynamic> { new { Route = "/some/route" } }
                    };

                var json = obj.ToJSON();
                var page = new Page();
                page.ImportJson( json );
                Assert.IsNotNull( page.PageRoutes );
                Assert.IsNotEmpty( page.PageRoutes );
                Assert.AreEqual( page.PageRoutes.First().Route, obj.PageRoutes[0].Route );
            }

            [Test]
            public void ShouldImportPageContexts()
            {
                Random random = new Random();
                var id = random.Next();
                var obj = new
                    {
                        Name = "Some Page",
                        PageContexts = new List<dynamic> { new { PageId = id } }
                    };

                var json = obj.ToJSON();
                var page = new Page();
                page.ImportJson( json );
                Assert.IsNotNull( page.PageContexts );
                Assert.IsNotEmpty( page.PageContexts );
                Assert.AreEqual( page.PageContexts.First().PageId, id );
            }

            [Test]
            public void ShouldImportAttributes()
            {
                var obj = new
                    {
                        Name = "Some Page",
                        Attributes = new Dictionary<string, dynamic> { { "foobar", null } }
                    };

                var json = obj.ToJSON();
                var page = new Page();
                page.ImportJson( json );
                Assert.IsNotNull( page.Attributes );
                Assert.IsNotEmpty( page.Attributes );
                Assert.IsNull( page.Attributes.First().Value );
            }

            [Test]
            public void ShouldImportAttributeValues()
            {
                var obj = new
                    {
                        Name = "Some Page",
                        AttributeValues =
                            new Dictionary<string, List<dynamic>> { { "foobar", new List<dynamic> { new { Value = "baz" } } } }
                    };

                var json = obj.ToJSON();
                var page = new Page();
                page.ImportJson( json );
                Assert.IsNotNull( page.AttributeValues );
                Assert.IsNotEmpty( page.AttributeValues );
                Assert.AreEqual( page.AttributeValues.First().Value.First().Value, "baz" );
            }
        }
    }
}
