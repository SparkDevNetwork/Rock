//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Rock.Model;
using Assert = NUnit.Framework.Assert;

namespace Rock.Tests.Model
{
    /// <summary>
    /// Unit tests for the Rock.Model.Page class
    /// </summary>
    [TestFixture]
    public class PageTests
    {
        /// <summary>
        /// Tests for the CopyPropertiesFrom method
        /// </summary>
        [TestClass]
        public class TheCopyPropertiesFromMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a Page object, resulting in a new Page.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldCopyEntity()
            {
                var page = new Page { Name = "SomePage" };
                var result = page.Clone( false );
                Assert.AreEqual( result.Name, page.Name );
            }
        }

        /// <summary>
        /// Tests for the Clone method
        /// </summary>
        [TestClass]
        public class TheCloneMethod
        {
            /// <summary>
            /// Should perform a shallow copy of a Page, including its collection of child Pages.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldCopyPages()
            {
                var children = new List<Page> { new Page() };
                var parent = new Page { Pages = children };
                var result = parent.Clone() as Page;
                Assert.IsNotEmpty( result.Pages );
            }

            /// <summary>
            /// Should perform a shallow copy of a Page, including any child Pages, recursively.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldCopyPagesRecursively()
            {
                var parent = new Page();
                var child = new Page();
                var grandchild = new Page();
                parent.Pages = new List<Page> { child };
                child.Pages = new List<Page> { grandchild };
                var result = parent.Clone() as Page;
                Assert.IsNotEmpty( result.Pages );
                Assert.IsNotEmpty( result.Pages.FirstOrDefault().Pages );
            }

            /// <summary>
            /// Should perform a shallow copy of a Page, including its collection of Blocks.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldCopyBlocks()
            {
                var page = new Page { Blocks = new List<Block>() };
                page.Blocks.Add( new Block() );
                var result = page.Clone() as Page;
                Assert.NotNull( result.Blocks );
                Assert.IsNotEmpty( result.Blocks );
            }

            /// <summary>
            /// Should perform a shallow copy of a Page, including its collection of PageRoutes.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldCopyPageRoutes()
            {
                var page = new Page { PageRoutes = new List<PageRoute>() };
                page.PageRoutes.Add( new PageRoute() );
                var result = page.Clone() as Page;
                Assert.NotNull( result.PageRoutes );
                Assert.IsNotEmpty( result.PageRoutes );
            }

            /// <summary>
            /// Should perform a shallow copy of a Page, including its collection of PageContexts.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldCopyPageContexts()
            {
                var page = new Page { PageContexts = new List<PageContext>() };
                page.PageContexts.Add( new PageContext() );
                var result = page.Clone() as Page;
                Assert.NotNull( result.PageContexts );
                Assert.IsNotEmpty( result.PageContexts );
            }
        }

        /// <summary>
        /// Tests for the ToJson method
        /// </summary>
        [TestClass]
        public class TheToJsonMethod
        {
            /// <summary>
            /// Should serialize a Page into a non-empty string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldNotBeEmpty()
            {
                var page = new Page();
                var result = page.ToJson();
                Assert.IsNotEmpty( result );
            }

            /// <summary>
            /// Shoulds serialize a Page into a JSON string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldExportAsJson()
            {
                var page = new Page
                    {
                        Title = "FooPage"
                    };

                var result = page.ToJson();
                const string key = "\"Title\": \"FooPage\"";
                Assert.Greater( result.IndexOf( key ), -1, string.Format( "'{0}' was not found in '{1}'.", key, result ) );
            }

            /// <summary>
            /// Should serialize a Pages collection of child Pages in the JSON string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldExportChildPages()
            {
                var page = new Page
                    {
                        Title = "FooPage",
                        Pages = new List<Page> { new Page { Title = "BarPage" } }
                    };

                var result = page.ToJson();
                result = result.Substring( result.IndexOf( "\"Pages\":" ) + 7 );
                const string key = "\"Title\": \"BarPage\"";
                Assert.Greater( result.IndexOf( key ), -1 );
            }

            /// <summary>
            /// Should recursively serialize a Pages collection of child Pages in the JSON string.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldExportChildPagesRecursively()
            {
                var parent = new Page { Title = "Parent" };
                var child = new Page { Title = "Child" };
                var grandchild = new Page { Title = "Grandchild" };
                parent.Pages = new List<Page> { child };
                child.Pages = new List<Page> { grandchild };
                var result = parent.ToJson( );
                const string parentKey = "\"Title\": \"Parent\"";
                const string childKey = "\"Title\": \"Child\"";
                const string grandChildKey = "\"Title\": \"Grandchild\"";
                Assert.Greater( result.IndexOf( parentKey ), -1 );
                Assert.Greater( result.IndexOf( childKey ), -1 );
                Assert.Greater( result.IndexOf( grandChildKey ), -1 );
            }
        }

        /// <summary>
        /// Tests for the FromJson method
        /// </summary>
        [TestClass]
        public class TheFromJsonMethod
        {
            /// <summary>
            /// Should take a JSON string and copy its contents to a Rock.Model.Page instance
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldCopyPropertiesToEntity()
            {
                var obj = new Page
                {
                    Name = "Foo Page",
                    IsSystem = true,
                };

                string json = obj.ToJson();
                var page = Page.FromJson( json );
                Assert.AreEqual( obj.Name, page.Name );
                Assert.AreEqual( obj.IsSystem, page.IsSystem );
            }

            /// <summary>
            /// Should deserialize a JSON string and restore a Page's collection of child Pages.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldImportChildPages()
            {
                var obj = new Page { Name = "Parent" };
                obj.Pages.Add ( new Page { Name = "Child" } );
                
                var json = obj.ToJson();
                var page = Page.FromJson( json );
                Assert.IsNotNull( page.Pages );
                Assert.IsNotEmpty( page.Pages );
                Assert.AreEqual( page.Pages.First().Name, obj.Pages.First().Name );
            }

            /// <summary>
            /// Should deserialize a JSON string and restore a Page's collection of child Pages, recursively.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldImportPagesRecursively()
            {
                const string PAGE_NAME = "Child Page";

                var childPage = new Page { Name = PAGE_NAME };
                var parentPage = new Page { Name = "Parent Page" };
                var grandparentPage = new Page { Name = "Grandparent Page" };

                parentPage.Pages.Add(childPage);
                grandparentPage.Pages.Add(parentPage);
 
                var json = grandparentPage.ToJson();
                var page = Page.FromJson( json );
                var childPages = page.Pages.First().Pages;
                Assert.IsNotNull( childPages );
                Assert.IsNotEmpty( childPages );
                Assert.AreEqual( childPages.First().Name, PAGE_NAME );
            }

            /// <summary>
            /// Should deserialize a JSON string and restore a Page's collection of child Blocks.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldImportBlocks()
            {
                var obj = new Page { Name = "Some Page" };
                obj.Blocks.Add(new Block { Name = "Some Block" } );
                var json = obj.ToJson();
                var page = Page.FromJson( json );
                Assert.IsNotNull( page.Blocks );
                Assert.IsNotEmpty( page.Blocks );
                Assert.AreEqual( page.Blocks.First().Name, obj.Blocks.First().Name );
            }

            /// <summary>
            /// Should deserialize a JSON string and restore a Page's collection of child PageRoutes.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldImportPageRoutes()
            {
                var obj = new Page { Name = "Some Page" };
                obj.PageRoutes.Add( new PageRoute { Route = "/some/route" } );
                var json = obj.ToJson();
                var page = Page.FromJson( json );
                Assert.IsNotNull( page.PageRoutes );
                Assert.IsNotEmpty( page.PageRoutes );
                Assert.AreEqual( page.PageRoutes.First().Route, obj.PageRoutes.First().Route );
            }

            /// <summary>
            /// Should deserialize a JSON string and restore a Page's collection of child PageContexts.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldImportPageContexts()
            {
                Random random = new Random();
                var id = random.Next();
                var obj = new Page { Name = "Some Page" };
                obj.PageContexts.Add( new PageContext { PageId = id } );
                var json = obj.ToJson();
                var page = Page.FromJson( json );
                Assert.IsNotNull( page.PageContexts );
                Assert.IsNotEmpty( page.PageContexts );
                Assert.AreEqual( page.PageContexts.First().PageId, id );
            }

            /// <summary>
            /// Should deserialize a JSON string and restore a Page's collection of Attributes.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldImportAttributes()
            {
                var obj = new Page
                    {
                        Name = "Some Page",
                        Attributes = new Dictionary<string, Web.Cache.AttributeCache> { { "foobar", null } }
                    };

                var json = obj.ToJson();
                var page = Page.FromJson( json );
                Assert.IsNotNull( page.Attributes );
                Assert.IsNotEmpty( page.Attributes );
                Assert.IsNull( page.Attributes.First().Value );
            }

            /// <summary>
            /// Should deserialize a JSON string and restore a Page's collection of AttributeValues.
            /// </summary>
            [Test]
            [TestMethod]
            [TestCategory( "Rock.Model.Page" )]
            public void ShouldImportAttributeValues()
            {
                var obj = new Page
                    {
                        Name = "Some Page",
                        AttributeValues = new Dictionary<string, List<AttributeValue>>
                            {
                                { "foobar", new List<AttributeValue> { new AttributeValue { Value = "baz" } } }
                            }
                    };

                var json = obj.ToJson();
                var page = Page.FromJson( json );
                Assert.IsNotNull( page.AttributeValues );
                Assert.IsNotEmpty( page.AttributeValues );
                Assert.AreEqual( page.AttributeValues.First().Value.First().Value, "baz" );
            }
        }
    }
}
