using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class PageTests
    {
        /// <summary>
        /// Should perform a shallow copy of a Page object, resulting in a new Page.
        /// </summary>
        [Fact]
        public void ShallowClone()
        {
            var page = new Page { InternalName = "SomePage" };
            var result = page.Clone( false );
            Assert.Equal( result.InternalName, page.InternalName );
        }

        /// <summary>
        /// Should perform a copy of a Page, including its collection of child Pages.
        /// </summary>
        [Fact]
        public void CloneChildren()
        {
            var children = new List<Page> { new Page() };
            var parent = new Page { Pages = children };
            var result = parent.Clone() as Page;
            Assert.NotEmpty( result.Pages );
        }

        /// <summary>
        /// Should perform a copy of a Page, including any child Pages, recursively.
        /// </summary>
        [Fact]
        public void CloneChildrenRecursively()
        {
            var parent = new Page();
            var child = new Page();
            var grandchild = new Page();
            parent.Pages = new List<Page> { child };
            child.Pages = new List<Page> { grandchild };
            var result = parent.Clone() as Page;
            Assert.NotEmpty( result.Pages );
            Assert.NotEmpty( result.Pages.FirstOrDefault().Pages );
        }

        /// <summary>
        /// Should perform a copy of a Page, including its collection of Blocks.
        /// </summary>
        [Fact]
        public void CloneBlocks()
        {
            var page = new Page { Blocks = new List<Block>() };
            page.Blocks.Add( new Block() );
            var result = page.Clone() as Page;
            Assert.NotNull( result.Blocks );
            Assert.NotEmpty( result.Blocks );
        }

        /// <summary>
        /// Should perform a copy of a Page, including its collection of PageRoutes.
        /// </summary>
        [Fact]
        public void ClonePageRoutes()
        {
            var page = new Page { PageRoutes = new List<PageRoute>() };
            page.PageRoutes.Add( new PageRoute() );
            var result = page.Clone() as Page;
            Assert.NotNull( result.PageRoutes );
            Assert.NotEmpty( result.PageRoutes );
        }

        /// <summary>
        /// Should perform a copy of a Page, including its collection of PageContexts.
        /// </summary>
        [Fact]
        public void ClonePageContexts()
        {
            var page = new Page { PageContexts = new List<PageContext>() };
            page.PageContexts.Add( new PageContext() );
            var result = page.Clone() as Page;
            Assert.NotNull( result.PageContexts );
            Assert.NotEmpty( result.PageContexts );
        }

        /// <summary>
        /// Should serialize a Page into a non-empty string.
        /// </summary>
        [Fact]
        public void ToJson()
        {
            var page = new Page();
            var result = page.ToJson();
            Assert.NotEmpty( result );
        }

        /// <summary>
        /// Shoulds serialize a Page into a JSON string.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ExportJson()
        {
            var page = new Page
            {
                PageTitle = "FooPage"
            };

            var result = page.ToJson();
            const string key = "\"PageTitle\":\"FooPage\"";
            Assert.NotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should serialize a Pages collection of child Pages in the JSON string.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ExportChildPages()
        {
            var page = new Page
            {
                PageTitle = "FooPage",
                Pages = new List<Page> { new Page { PageTitle = "BarPage" } }
            };

            var result = page.ToJson();
            result = result.Substring( result.IndexOf( "\"Pages\":" ) + 7 );
            const string key = "\"PageTitle\":\"BarPage\"";
            Assert.NotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should recursively serialize a Pages collection of child Pages in the JSON string.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ExportChildPagesRecursively()
        {
            var parent = new Page { PageTitle = "Parent" };
            var child = new Page { PageTitle = "Child" };
            var grandchild = new Page { PageTitle = "Grandchild" };
            parent.Pages = new List<Page> { child };
            child.Pages = new List<Page> { grandchild };
            var result = parent.ToJson();
            const string parentKey = "\"PageTitle\":\"Parent\"";
            const string childKey = "\"PageTitle\":\"Child\"";
            const string grandChildKey = "\"PageTitle\":\"Grandchild\"";
            Assert.NotEqual( result.IndexOf( parentKey ), -1 );
            Assert.NotEqual( result.IndexOf( childKey ), -1 );
            Assert.NotEqual( result.IndexOf( grandChildKey ), -1 );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a Rock.Model.Page instance
        /// </summary>
        [Fact]
        public void FromJson()
        {
            var obj = new Page
            {
                InternalName = "Foo Page",
                IsSystem = true,
            };

            string json = obj.ToJson();
            var page = Page.FromJson( json );
            Assert.Equal( obj.InternalName, page.InternalName );
            Assert.Equal( obj.IsSystem, page.IsSystem );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a Page's collection of child Pages.
        /// </summary>
        [Fact]
        public void ImportJson()
        {
            var obj = new Page { InternalName = "Parent" };
            obj.Pages.Add( new Page { InternalName = "Child" } );

            var json = obj.ToJson();
            var page = Page.FromJson( json );
            Assert.NotNull( page.Pages );
            Assert.NotEmpty( page.Pages );
            Assert.Equal( page.Pages.First().InternalName, obj.Pages.First().InternalName );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a Page's collection of child Pages, recursively.
        /// </summary>
        [Fact]
        public void ImportJsonRecursively()
        {
            const string PAGE_NAME = "Child Page";

            var childPage = new Page { InternalName = PAGE_NAME };
            var parentPage = new Page { InternalName = "Parent Page" };
            var grandparentPage = new Page { InternalName = "Grandparent Page" };

            parentPage.Pages.Add( childPage );
            grandparentPage.Pages.Add( parentPage );

            var json = grandparentPage.ToJson();
            var page = Page.FromJson( json );
            var childPages = page.Pages.First().Pages;
            Assert.NotNull( childPages );
            Assert.NotEmpty( childPages );
            Assert.Equal( childPages.First().InternalName, PAGE_NAME );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a Page's collection of child Blocks.
        /// </summary>
        [Fact]
        public void ImportBlocks()
        {
            var obj = new Page { InternalName = "Some Page" };
            obj.Blocks.Add( new Block { Name = "Some Block" } );
            var json = obj.ToJson();
            var page = Page.FromJson( json );
            Assert.NotNull( page.Blocks );
            Assert.NotEmpty( page.Blocks );
            Assert.Equal( page.Blocks.First().Name, obj.Blocks.First().Name );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a Page's collection of child PageRoutes.
        /// </summary>
        [Fact]
        public void ImportPageRoutes()
        {
            var obj = new Page { InternalName = "Some Page" };
            obj.PageRoutes.Add( new PageRoute { Route = "/some/route" } );
            var json = obj.ToJson();
            var page = Page.FromJson( json );
            Assert.NotNull( page.PageRoutes );
            Assert.NotEmpty( page.PageRoutes );
            Assert.Equal( page.PageRoutes.First().Route, obj.PageRoutes.First().Route );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a Page's collection of child PageContexts.
        /// </summary>
        [Fact]
        public void ImportPageContexts()
        {
            Random random = new Random();
            var id = random.Next();
            var obj = new Page { InternalName = "Some Page" };
            obj.PageContexts.Add( new PageContext { PageId = id } );
            var json = obj.ToJson();
            var page = Page.FromJson( json );
            Assert.NotNull( page.PageContexts );
            Assert.NotEmpty( page.PageContexts );
            Assert.Equal( page.PageContexts.First().PageId, id );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a Page's collection of Attributes.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ImportAttributes()
        {
            var obj = new Page
            {
                InternalName = "Some Page",
                Attributes = new Dictionary<string, AttributeCache> { { "foobar", null } }
            };

            // the AttributeCacheJsonConverter won't convert null to AttributeCache
            var json = obj.ToJson().Replace( "\"foobar\":null", "\"foobar\":{}" );

            var page = Page.FromJson( json );
            Assert.NotNull( page.Attributes );
            Assert.NotEmpty( page.Attributes );
        }

        /// <summary>
        /// Should deserialize a JSON string and restore a Page's collection of AttributeValues.
        /// </summary>
        [Fact]
        public void ImportAttributeValues()
        {
            var obj = new Page
            {
                InternalName = "Some Page",
                AttributeValues = new Dictionary<string, AttributeValueCache>()
                {
                    { "foobar",  new AttributeValueCache( new AttributeValue { Value = "baz" } ) }
                }
            };

            var json = obj.ToJson();
            var page = Page.FromJson( json );
            Assert.NotNull( page.AttributeValues );
            Assert.NotEmpty( page.AttributeValues );
            Assert.Equal( "baz", page.AttributeValues.First().Value.Value );
        }
    }
}
