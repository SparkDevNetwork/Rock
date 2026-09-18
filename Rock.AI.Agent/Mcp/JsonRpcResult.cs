// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Rock.AI.Agent.Mcp.Protocol;

namespace Rock.AI.Agent.Mcp;

/// <summary>
/// Represents the result of a JSON-RPC request.
/// </summary>
internal class JsonRpcResult
{
    #region Fields

    /// <summary>
    /// An identifier of JSON <c>null</c>. JSON-RPC 2.0 requires a response to
    /// use this when the request could not be read well enough to know what
    /// its identifier was.
    /// </summary>
    private static readonly JsonElement NullId = JsonSerializer.Deserialize<JsonElement>( "null" );

    #endregion

    #region Properties

    /// <summary>
    /// The version of the JSON-RPC protocol being used.
    /// </summary>
    [JsonPropertyName( "jsonrpc" )]
    public string Version { get; } = "2.0";

    /// <summary>
    /// The identifier of the request this result corresponds to. This holds
    /// the raw element from the request so that both the value and its JSON
    /// type are echoed back unchanged, as JSON-RPC 2.0 requires.
    /// </summary>
    public JsonElement Id { get; }

    /// <summary>
    /// The result of the request, if successful.
    /// </summary>
    public object Result { get; }

    /// <summary>
    /// The error information, if the request failed.
    /// </summary>
    public JsonRpcError Error { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates an instance of <see cref="JsonRpcResult"/> with a
    /// successful result.
    /// </summary>
    /// <param name="id">The identifier of the JSON-RPC request this result corresponds to.</param>
    /// <param name="result">The result value of the JSON-RPC operation.</param>
    internal JsonRpcResult( JsonElement id, object result )
    {
        Id = id;
        Result = result;
    }

    /// <summary>
    /// Creates an instance of <see cref="JsonRpcResult"/> with an error.
    /// </summary>
    /// <param name="id">The identifier of the JSON-RPC request this result corresponds to.</param>
    /// <param name="errorCode">The code that identifies what type of error happened.</param>
    /// <param name="errorMessage">A concise description of the error.</param>
    internal JsonRpcResult( JsonElement id, int errorCode, string errorMessage )
    {
        Id = id;
        Error = new JsonRpcError
        {
            Code = errorCode,
            Message = errorMessage
        };
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates an error result for a request whose identifier could not be
    /// determined, such as one that was not valid JSON. The identifier of the
    /// response is <c>null</c> in this case.
    /// </summary>
    /// <param name="errorCode">The code that identifies what type of error happened.</param>
    /// <param name="errorMessage">A concise description of the error.</param>
    /// <returns>A new instance of <see cref="JsonRpcResult"/> that represents the error.</returns>
    internal static JsonRpcResult CreateErrorResult( int errorCode, string errorMessage )
    {
        return new JsonRpcResult( NullId, errorCode, errorMessage );
    }

    /// <summary>
    /// Writes the JSON-RPC result to a stream in JSON format.
    /// </summary>
    /// <param name="stream">The stream that the result should be written to.</param>
    /// <param name="serializerOptions">The options to use when serializing the result.</param>
    public void ToJson( Stream stream, JsonSerializerOptions serializerOptions )
    {
        JsonSerializer.Serialize( stream, this, serializerOptions );
        stream.Flush();
    }

    #endregion
}
