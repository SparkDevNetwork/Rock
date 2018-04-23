using Rock.Model;
using System;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class PageContextTests
    {
        /// <summary>
        /// Should perform a shallow copy of a PageContext object, resulting in a new PageContext.
        /// </summary>
        [Fact]
        public void ShallowClone()
        {
            var pageContext = new PageContext { Guid = Guid.NewGuid() };
            var result = pageContext.Clone( false );
            Assert.Equal( result.Guid, pageContext.Guid );
        }

        /// <summary>
        /// Should serialize a PageContext into a non-empty string.
        /// </summary>
        [Fact]
        public void ToJson()
        {
            var pageContext = new PageContext { Guid = Guid.NewGuid() };
            var result = pageContext.ToJson();
            Assert.NotEmpty( result );
        }

        /// <summary>
        /// Shoulds serialize a PageContext into a JSON string.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ExportJson()
        {
            var guid = Guid.NewGuid();
            var pageContext = new PageContext
            {
                Guid = guid
            };

            var result = pageContext.ToJson();
            var key = string.Format( "\"Guid\":\"{0}\"", guid );
            Assert.NotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a Rock.Model.PageContext instance
        /// </summary>
        [Fact]
        public void ImportJson()
        {
            var obj = new PageContext
            {
                Guid = Guid.NewGuid(),
                IsSystem = false
            };

            var json = obj.ToJson();
            var pageContext = PageContext.FromJson( json );
            Assert.Equal( obj.Guid, pageContext.Guid );
            Assert.Equal( obj.IsSystem, pageContext.IsSystem );
        }
    }
}
