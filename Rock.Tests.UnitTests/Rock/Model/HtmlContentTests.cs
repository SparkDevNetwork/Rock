using Rock.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.Rock.Model
{
    [TestClass]
    public class HtmlContentTests
    {
        /// <summary>
        /// Should perform a shallow copy of a HtmlContent object, resulting in a new HtmlContent.
        /// </summary>
        [TestMethod]
        public void ShallowClone()
        {
            var html = new HtmlContent { Content = "Foo" };
            var result = html.Clone( false );
            Assert.That.AreEqual( result.Content, html.Content );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a Rock.Model.HtmlContent instance
        /// </summary>
        [TestMethod]
        public void Clone()
        {
            var obj = new HtmlContent
            {
                EntityValue = "Some Value",
                IsApproved = true
            };

            var json = obj.ToJson();
            var htmlContent = HtmlContent.FromJson( json );
            Assert.That.AreEqual( obj.EntityValue, htmlContent.EntityValue );
            Assert.That.AreEqual( obj.IsApproved, htmlContent.IsApproved );
        }

        /// <summary>
        /// Should serialize a HtmlContent into a non-empty string.
        /// </summary>
        [TestMethod]
        public void ToJson()
        {
            var html = new HtmlContent { Content = "Foo" };
            var result = html.ToJson();
            Assert.That.IsNotEmpty( result );
        }

        /// <summary>
        /// Shoulds serialize a HtmlContent into a JSON string.
        /// </summary>
        [TestMethod]
        public void ExportJson()
        {
            var html = new HtmlContent
            {
                Content = "Foo"
            };

            var result = html.ToJson();
            const string key = "\"Content\":\"Foo\"";
            Assert.That.AreNotEqual( result.IndexOf( key ), -1 );
        }
    }
}