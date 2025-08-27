using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Mcp;
using Rock.AI.Agent.Mcp.Protocol;

namespace Rock.AI.Agent.Tests.Mcp
{
    [TestClass]
    public class JsonRpcRequestTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithJsonRpcVersion_DecodesCorrectly()
        {
            var ms = ToStream( "{\"jsonrpc\":\"1.2.3\"}" );

            var request = new JsonRpcRequest( ms );

            Assert.AreEqual( "1.2.3", request.Version );
        }

        [TestMethod]
        public void Constructor_WithIdValue_DecodesCorrectly()
        {
            var ms = ToStream( "{\"id\":123}" );

            var request = new JsonRpcRequest( ms );

            Assert.AreEqual( 123, request.Id );
        }

        [TestMethod]
        public void Constructor_WithNullIdValue_DecodesAsNull()
        {
            var ms = ToStream( "{}" );

            var request = new JsonRpcRequest( ms );

            Assert.IsNull( request.Id );
        }

        [TestMethod]
        public void Constructor_WithMethodValue_DecodesCorrectly()
        {
            var ms = ToStream( "{\"method\":\"listtest\"}" );

            var request = new JsonRpcRequest( ms );

            Assert.AreEqual( "listtest", request.Method );
        }

        #endregion

        #region GetParameters Tests

        [TestMethod]
        public void GetParameters_WithValidJson_DecodesCorrectly()
        {
            var ms = ToStream( "{\"params\":{\"cursor\":\"testvalue\"}}" );
            var request = new JsonRpcRequest( ms );

            var parameters = request.GetParameters<ListToolsParameters>();

            Assert.AreEqual( "testvalue", parameters.Cursor );
        }

        [TestMethod]
        public void GetParameters_WithInvalidJson_ReturnsNewInstance()
        {
            var ms = ToStream( "{\"params\":123}" );
            var request = new JsonRpcRequest( ms );

            var parameters = request.GetParameters<ListToolsParameters>();

            Assert.IsNotNull( parameters );
        }

        [TestMethod]
        public void GetParameters_WithMissingJsonNode_ReturnsNewInstance()
        {
            var ms = ToStream( "{}" );
            var request = new JsonRpcRequest( ms );

            var parameters = request.GetParameters<ListToolsParameters>();

            Assert.IsNotNull( parameters );
        }

        #endregion

        #region CreateResult Tests

        [TestMethod]
        public void CreateResult_WithoutIdValue_ThrowsException()
        {
            var ms = ToStream( "{}" );
            var request = new JsonRpcRequest( ms );

            Assert.Throws<InvalidOperationException>( () => request.CreateResult( "test" ) );
        }

        [TestMethod]
        public void CreateResult_WithValue_CreatesResultObject()
        {
            var ms = ToStream( "{\"id\": 123}" );
            var request = new JsonRpcRequest( ms );

            var result = request.CreateResult( "test" );

            Assert.AreEqual( "test", result.Result );
        }

        #endregion

        #region CreateErrorResult Tests

        [TestMethod]
        public void CreateErrorResult_WithoutIdValue_ThrowsException()
        {
            var ms = ToStream( "{}" );
            var request = new JsonRpcRequest( ms );

            Assert.Throws<InvalidOperationException>( () => request.CreateErrorResult( 123, "test" ) );
        }

        [TestMethod]
        public void CreateErrorResult_WithValue_CreatesResultObject()
        {
            var ms = ToStream( "{\"id\": 123}" );
            var request = new JsonRpcRequest( ms );

            var result = request.CreateErrorResult( 123, "test" );

            Assert.IsNotNull( result.Error );
            Assert.AreEqual( 123, result.Error.Code );
            Assert.AreEqual( "test", result.Error.Message );
        }

        #endregion

        #region Support Methods

        internal static MemoryStream ToStream( string json )
        {
            var ms = new MemoryStream();
            using var writer = new StreamWriter( ms, System.Text.Encoding.UTF8, 1024, true );

            writer.Write( json );
            writer.Flush();

            ms.Position = 0;

            return ms;
        }

        #endregion
    }
}
