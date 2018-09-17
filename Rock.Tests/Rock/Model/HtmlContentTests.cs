using Rock.Model;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    public class HtmlContentTests
    {
        /// <summary>
        /// Should perform a shallow copy of a HtmlContent object, resulting in a new HtmlContent.
        /// </summary>
        [Fact]
        public void ShallowClone()
        {
            var html = new HtmlContent { Content = "Foo" };
            var result = html.Clone( false );
            Assert.Equal( result.Content, html.Content );
        }

        /// <summary>
        /// Should take a JSON string and copy its contents to a Rock.Model.HtmlContent instance
        /// </summary>
        [Fact]
        public void Clone()
        {
            var obj = new HtmlContent
            {
                EntityValue = "Some Value",
                IsApproved = true
            };

            var json = obj.ToJson();
            var htmlContent = HtmlContent.FromJson( json );
            Assert.Equal( obj.EntityValue, htmlContent.EntityValue );
            Assert.Equal( obj.IsApproved, htmlContent.IsApproved );
        }

        /// <summary>
        /// Should serialize a HtmlContent into a non-empty string.
        /// </summary>
        [Fact]
        public void ToJson()
        {
            var html = new HtmlContent { Content = "Foo" };
            var result = html.ToJson();
            Assert.NotEmpty( result );
        }

        /// <summary>
        /// Shoulds serialize a HtmlContent into a JSON string.
        /// </summary>
        [Fact( Skip = "Need a mock for Global Attributes" )]
        public void ExportJson()
        {
            var html = new HtmlContent
            {
                Content = "Foo"
            };

            var result = html.ToJson();
            const string key = "\"Content\":\"Foo\"";
            Assert.NotEqual( result.IndexOf( key ), -1 );
        }
    }
}
