using System.IO;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Mcp;

namespace Rock.AI.Agent.Tests.Mcp
{
    [TestClass]
    public class JsonRpcResultTests
    {
        [TestMethod]
        public void ToJson_WithSuccessResult_ReturnsExpectedJson()
        {
            var result = new JsonRpcResult( 123, "test" );
            var ms = new MemoryStream();

            result.ToJson( ms );

            ms.Position = 0;
            using var reader = new StreamReader( ms );
            var json = reader.ReadToEnd();

            var element = JsonSerializer.Deserialize<JsonElement>( json );

            Assert.AreEqual( "test", element.GetProperty( "result" ).GetString() );
            Assert.IsFalse( element.TryGetProperty( "error", out _ ) );
        }

        [TestMethod]
        public void ToJson_WithErrorResult_ReturnsExpectedJson()
        {
            var result = new JsonRpcResult( 123, 456, "test" );
            var ms = new MemoryStream();

            result.ToJson( ms );

            ms.Position = 0;
            using var reader = new StreamReader( ms );
            var json = reader.ReadToEnd();

            var element = JsonSerializer.Deserialize<JsonElement>( json );

            Assert.AreEqual( 456, element.GetProperty( "error" ).GetProperty( "code" ).GetInt32() );
            Assert.AreEqual( "test", element.GetProperty( "error" ).GetProperty( "message" ).GetString() );
            Assert.IsFalse( element.TryGetProperty( "result", out _ ) );
        }
    }
}
