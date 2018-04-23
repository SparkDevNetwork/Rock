using Rock.Model;
using System;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class PageRouteTests
    {
        /// <summary>
        /// Should perform a shallow copy of a PageRoute object, resulting in a new PageRoute.
        /// </summary>
        [Fact]
        public void ShallowClone()
        {
            var pageRoute = new PageRoute { Guid = Guid.NewGuid() };
            var result = pageRoute.Clone( false );
            Assert.Equal( result.Guid, pageRoute.Guid );
        }

        /// <summary>
        /// Should serialize a PageRoute into a non-empty string.
        /// </summary>
        [Fact]
        public void ToJson()
        {
            var pageRoute = new PageRoute { Guid = Guid.NewGuid() };
            dynamic result = pageRoute.ToJson();
            Assert.NotEmpty( result );
        }

        /// <summary>
        /// Shoulds serialize a PageRoute into a JSON string.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ExportJson()
        {
            var guid = Guid.NewGuid();
            var pageRoute = new PageRoute
            {
                Guid = guid
            };

            var result = pageRoute.ToJson();
            var key = string.Format( "\"Guid\":\"{0}\"", guid );
            Assert.NotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a Rock.Model.PageRoute instance
        /// </summary>
        [Fact]
        public void ImportJson()
        {
            var obj = new PageRoute
            {
                Route = "/some/path",
                IsSystem = true
            };

            var json = obj.ToJson();
            var pageRoute = PageRoute.FromJson( json );
            Assert.Equal( obj.Route, pageRoute.Route );
            Assert.Equal( obj.IsSystem, pageRoute.IsSystem );
        }
    }
}
