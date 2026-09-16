using System;
using System.IO;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Mcp;
using Rock.AI.Agent.Mcp.Protocol;
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent.Tests.Mcp;

[TestClass]
public class JsonRpcRequestTests
{
    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithJsonRpcVersion_DecodesCorrectly()
    {
        var ms = ToStream( "{\"jsonrpc\":\"1.2.3\"}" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.AreEqual( "1.2.3", request.Version );
    }

    [TestMethod]
    public void Constructor_WithNumberIdValue_DecodesCorrectly()
    {
        var ms = ToStream( "{\"id\":123}" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.IsTrue( request.Id.HasValue );
        Assert.AreEqual( JsonValueKind.Number, request.Id.Value.ValueKind );
        Assert.AreEqual( 123, request.Id.Value.GetInt64() );
        Assert.IsTrue( request.IsIdValid );
    }

    [TestMethod]
    public void Constructor_WithStringIdValue_DecodesCorrectly()
    {
        var ms = ToStream( "{\"id\":\"req_abc123\"}" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.IsTrue( request.Id.HasValue );
        Assert.AreEqual( JsonValueKind.String, request.Id.Value.ValueKind );
        Assert.AreEqual( "req_abc123", request.Id.Value.GetString() );
        Assert.IsTrue( request.IsIdValid );
    }

    [TestMethod]
    public void Constructor_WithExplicitNullIdValue_DecodesAsInvalidId()
    {
        var ms = ToStream( "{\"id\":null}" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.IsTrue( request.Id.HasValue );
        Assert.AreEqual( JsonValueKind.Null, request.Id.Value.ValueKind );
        Assert.IsFalse( request.IsIdValid );
    }

    [TestMethod]
    public void Constructor_WithObjectIdValue_DecodesAsInvalidId()
    {
        var ms = ToStream( "{\"id\":{}}" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.IsFalse( request.IsIdValid );
    }

    [TestMethod]
    public void Constructor_WithMissingIdValue_DecodesAsNull()
    {
        var ms = ToStream( "{}" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.IsNull( request.Id );
        Assert.IsFalse( request.IsIdValid );
    }

    [TestMethod]
    public void Constructor_WithMissingMethod_DecodesAsNull()
    {
        var ms = ToStream( "{\"id\":123}" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.IsNull( request.Method );
    }

    [TestMethod]
    public void Constructor_WithArrayPayload_DecodesWithoutThrowing()
    {
        var ms = ToStream( "[{\"id\":123}]" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.IsFalse( request.IsRequestObject );
        Assert.IsNull( request.Id );
        Assert.IsNull( request.Method );
    }

    [TestMethod]
    public void Constructor_WithMethodValue_DecodesCorrectly()
    {
        var ms = ToStream( "{\"method\":\"listtest\"}" );

        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.AreEqual( "listtest", request.Method );
    }

    #endregion

    #region GetParameters Tests

    [TestMethod]
    public void GetParameters_WithValidJson_DecodesCorrectly()
    {
        var ms = ToStream( "{\"params\":{\"cursor\":\"testvalue\"}}" );
        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        var parameters = request.GetParameters<ListToolsParameters>();

        Assert.AreEqual( "testvalue", parameters.Cursor );
    }

    [TestMethod]
    public void GetParameters_WithInvalidJson_ReturnsNewInstance()
    {
        var ms = ToStream( "{\"params\":123}" );
        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        var parameters = request.GetParameters<ListToolsParameters>();

        Assert.IsNotNull( parameters );
    }

    [TestMethod]
    public void GetParameters_WithMissingJsonNode_ReturnsNewInstance()
    {
        var ms = ToStream( "{}" );
        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        var parameters = request.GetParameters<ListToolsParameters>();

        Assert.IsNotNull( parameters );
    }

    #endregion

    #region CreateResult Tests

    [TestMethod]
    public void CreateResult_WithoutIdValue_ThrowsException()
    {
        var ms = ToStream( "{}" );
        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.Throws<InvalidOperationException>( () => request.CreateResult( "test" ) );
    }

    [TestMethod]
    public void CreateResult_WithValue_CreatesResultObject()
    {
        var ms = ToStream( "{\"id\": 123}" );
        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        var result = request.CreateResult( "test" );

        Assert.AreEqual( "test", result.Result );
    }

    #endregion

    #region CreateErrorResult Tests

    [TestMethod]
    public void CreateErrorResult_WithoutIdValue_ThrowsException()
    {
        var ms = ToStream( "{}" );
        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        Assert.Throws<InvalidOperationException>( () => request.CreateErrorResult( 123, "test" ) );
    }

    [TestMethod]
    public void CreateErrorResult_WithValue_CreatesResultObject()
    {
        var ms = ToStream( "{\"id\": 123}" );
        var request = new JsonRpcRequest( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

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
