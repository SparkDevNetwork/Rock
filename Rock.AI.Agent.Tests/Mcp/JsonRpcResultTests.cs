using System.IO;

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

            Assert.AreEqual( "{\"jsonrpc\":\"2.0\",\"id\":123,\"result\":\"test\"}", json );
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

            Assert.AreEqual( "{\"jsonrpc\":\"2.0\",\"id\":123,\"error\":{\"code\":456,\"message\":\"test\"}}", json );
        }
    }
}
