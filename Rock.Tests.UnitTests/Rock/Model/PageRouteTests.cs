using Rock.Model;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Model
{
    [TestClass]
    public class PageRouteTests
    {
        /// <summary>
        /// Should perform a shallow copy of a PageRoute object, resulting in a new PageRoute.
        /// </summary>
        [TestMethod]
        public void ShallowClone()
        {
            var pageRoute = new PageRoute { Guid = Guid.NewGuid() };
            var result = pageRoute.Clone( false );
            Assert.That.AreEqual( result.Guid, pageRoute.Guid );
        }

        /// <summary>
        /// Should serialize a PageRoute into a non-empty string.
        /// </summary>
        [TestMethod]
        public void ToJson()
        {
            var pageRoute = new PageRoute { Guid = Guid.NewGuid() };
            dynamic result = pageRoute.ToJson();
            Assert.That.IsNotEmpty( result as string );
        }

        /// <summary>
        /// Shoulds serialize a PageRoute into a JSON string.
        /// </summary>
        [TestMethod]
        public void ExportJson()
        {
            var guid = Guid.NewGuid();
            var pageRoute = new PageRoute
            {
                Guid = guid
            };

            var result = pageRoute.ToJson();
            var key = string.Format( "\"Guid\":\"{0}\"", guid );
            Assert.That.AreNotEqual( result.IndexOf( key ), -1 );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a Rock.Model.PageRoute instance
        /// </summary>
        [TestMethod]
        public void ImportJson()
        {
            var obj = new PageRoute
            {
                Route = "/some/path",
                IsSystem = true
            };

            var json = obj.ToJson();
            var pageRoute = PageRoute.FromJson( json );
            Assert.That.AreEqual( obj.Route, pageRoute.Route );
            Assert.That.AreEqual( obj.IsSystem, pageRoute.IsSystem );
        }
    }
}