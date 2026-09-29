using System.IO;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Mcp;
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent.Tests.Mcp;

[TestClass]
public class JsonRpcResultTests
{
    [TestMethod]
    public void ToJson_WithSuccessResult_ReturnsExpectedJson()
    {
        var result = new JsonRpcResult( ToId( "123" ), "test" );
        var ms = new MemoryStream();

        result.ToJson( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

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
        var result = new JsonRpcResult( ToId( "123" ), 456, "test" );
        var ms = new MemoryStream();

        result.ToJson( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        ms.Position = 0;
        using var reader = new StreamReader( ms );
        var json = reader.ReadToEnd();

        var element = JsonSerializer.Deserialize<JsonElement>( json );

        Assert.AreEqual( 456, element.GetProperty( "error" ).GetProperty( "code" ).GetInt32() );
        Assert.AreEqual( "test", element.GetProperty( "error" ).GetProperty( "message" ).GetString() );
        Assert.IsFalse( element.TryGetProperty( "result", out _ ) );
    }

    [TestMethod]
    public void ToJson_WithNumberId_WritesNumberId()
    {
        var element = Serialize( new JsonRpcResult( ToId( "123" ), "test" ) );

        Assert.AreEqual( JsonValueKind.Number, element.GetProperty( "id" ).ValueKind );
        Assert.AreEqual( 123, element.GetProperty( "id" ).GetInt64() );
    }

    [TestMethod]
    public void ToJson_WithStringId_WritesStringId()
    {
        var element = Serialize( new JsonRpcResult( ToId( "\"req_abc123\"" ), "test" ) );

        Assert.AreEqual( JsonValueKind.String, element.GetProperty( "id" ).ValueKind );
        Assert.AreEqual( "req_abc123", element.GetProperty( "id" ).GetString() );
    }

    [TestMethod]
    public void CreateErrorResult_WithoutId_WritesNullId()
    {
        var element = Serialize( JsonRpcResult.CreateErrorResult( JsonRpcErrorCode.ParseError, "test" ) );

        Assert.IsTrue( element.TryGetProperty( "id", out var id ) );
        Assert.AreEqual( JsonValueKind.Null, id.ValueKind );
        Assert.AreEqual( JsonRpcErrorCode.ParseError, element.GetProperty( "error" ).GetProperty( "code" ).GetInt32() );
    }

    /// <summary>
    /// Creates the identifier element a result is constructed with.
    /// </summary>
    /// <param name="json">The raw JSON value of the identifier.</param>
    /// <returns>The element that represents the identifier.</returns>
    private static JsonElement ToId( string json )
    {
        return JsonSerializer.Deserialize<JsonElement>( json );
    }

    /// <summary>
    /// Serializes the result and reads it back so the written JSON can be
    /// inspected.
    /// </summary>
    /// <param name="result">The result to serialize.</param>
    /// <returns>The element that represents the written JSON.</returns>
    private static JsonElement Serialize( JsonRpcResult result )
    {
        var ms = new MemoryStream();

        result.ToJson( ms, AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public ) );

        ms.Position = 0;
        using var reader = new StreamReader( ms );

        return JsonSerializer.Deserialize<JsonElement>( reader.ReadToEnd() );
    }
}
