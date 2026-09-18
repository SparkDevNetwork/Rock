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

using System;
using System.IO;
using System.Text.Json;

namespace Rock.AI.Agent.Mcp;

/// <summary>
/// A request in the JSON-RPC protocol format.
/// </summary>
internal class JsonRpcRequest
{
    #region Fields

    /// <summary>
    /// The root element of the JSON-RPC request.
    /// </summary>
    private readonly JsonElement _rootElement;

    /// <summary>
    /// The serializer options to use when deserializing JSON data.
    /// </summary>
    private readonly JsonSerializerOptions _serializerOptions;

    #endregion

    #region Properties

    /// <summary>
    /// The version of the JSON-RPC protocol used in this request.
    /// </summary>
    public string Version => TryGetRootProperty( "jsonrpc", out var versionProperty ) ? versionProperty.GetString() : null;

    /// <summary>
    /// The unique identifier for the request. Will be <c>null</c> for
    /// notification messages that do not require a response. JSON-RPC 2.0
    /// allows this to be either a string or a number, so the raw element is
    /// kept and echoed back in the response exactly as it was received.
    /// </summary>
    public JsonElement? Id => TryGetRootProperty( "id", out var idProperty ) ? idProperty : ( JsonElement? ) null;

    /// <summary>
    /// Determines if the identifier of this request is one that JSON-RPC 2.0
    /// permits. Only a string or a number is valid, an explicit <c>null</c>
    /// or any other JSON type is not.
    /// </summary>
    public bool IsIdValid => Id.HasValue
        && ( Id.Value.ValueKind == JsonValueKind.String || Id.Value.ValueKind == JsonValueKind.Number );

    /// <summary>
    /// Determines if the payload was a JSON object, which is the only shape
    /// a single JSON-RPC request may take. Anything else, such as the array
    /// used by a batch request, is not supported.
    /// </summary>
    public bool IsRequestObject => _rootElement.ValueKind == JsonValueKind.Object;

    /// <summary>
    /// The method name of the request, which indicates the action to be
    /// performed. Will be <c>null</c> if the request did not specify one.
    /// </summary>
    public string Method => TryGetRootProperty( "method", out var methodProperty ) && methodProperty.ValueKind == JsonValueKind.String
        ? methodProperty.GetString()
        : null;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonRpcRequest"/> class
    /// by deserializing a JSON payload from the specified stream.
    /// </summary>
    /// <param name="stream">The input stream containing the JSON payload to deserialize. Must not be <see langword="null"/>.</param>
    /// <param name="serializerOptions">The serializer options to use when deserializing the JSON data.</param>
    /// <param name="audienceType">The type of audience to serialize and deserialize for.</param>
    public JsonRpcRequest( Stream stream, JsonSerializerOptions serializerOptions )
    {
        _serializerOptions = serializerOptions;
        _rootElement = JsonSerializer.Deserialize<JsonElement>( stream, _serializerOptions );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a property from the root element of the request. This tolerates a
    /// payload that is not a JSON object, which would otherwise throw.
    /// </summary>
    /// <param name="propertyName">The name of the property to look for.</param>
    /// <param name="value">On return, contains the property if it was found.</param>
    /// <returns><c>true</c> if the property was found; otherwise <c>false</c>.</returns>
    private bool TryGetRootProperty( string propertyName, out JsonElement value )
    {
        if ( _rootElement.ValueKind != JsonValueKind.Object )
        {
            value = default;

            return false;
        }

        return _rootElement.TryGetProperty( propertyName, out value );
    }

    /// <summary>
    /// Gets the parameters of the request as a strongly typed object.
    /// </summary>
    /// <typeparam name="T">The type of object to decode the parameters into.</typeparam>
    /// <returns>An instance of <typeparamref name="T"/> or a new instance if no parameters were provided.</returns>
    public T GetParameters<T>()
        where T : new()
    {
        if ( TryGetRootProperty( "params", out var parameters ) )
        {
            try
            {
                return JsonSerializer.Deserialize<T>( parameters.GetRawText(), _serializerOptions );
            }
            catch
            {
                // If deserialization fails, we can return default value.
                // This allows for cases where parameters are not required or are empty.
                return new T();
            }
        }

        return new T();
    }

    /// <summary>
    /// Creates a response to this request with the specified result.
    /// </summary>
    /// <param name="result">The object that represents the result of the request.</param>
    /// <returns>A new instance of <see cref="JsonRpcResult"/> that represents the result.</returns>
    public JsonRpcResult CreateResult( object result )
    {
        if ( !Id.HasValue )
        {
            throw new InvalidOperationException( "Cannot create a result to a notification message." );
        }

        return new JsonRpcResult( Id.Value, result );
    }

    /// <summary>
    /// Creats an error result to the request with the specified error
    /// code and message.
    /// </summary>
    /// <param name="errorCode">The error code that indicates what went wrong.</param>
    /// <param name="errorMessage">A concise message that describes what caused the error.</param>
    /// <returns>A new instance of <see cref="JsonRpcResult"/> that represents the result.</returns>
    public JsonRpcResult CreateErrorResult( int errorCode, string errorMessage )
    {
        if ( !Id.HasValue )
        {
            throw new InvalidOperationException( "Cannot create an error result to a notification message." );
        }

        return new JsonRpcResult( Id.Value, errorCode, errorMessage );
    }

    #endregion
}
