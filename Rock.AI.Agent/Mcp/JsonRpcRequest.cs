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
//

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Mcp
{
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

        #endregion

        #region Properties

        /// <summary>
        /// The version of the JSON-RPC protocol used in this request.
        /// </summary>
        public string Version => _rootElement.GetProperty( "jsonrpc" ).GetString();

        /// <summary>
        /// The unique identifier for the request. Will be <c>null</c> for
        /// notification messages that do not require a response.
        /// </summary>
        public long? Id => _rootElement.TryGetProperty( "id", out var idProperty ) ? idProperty.GetInt64() : ( long? ) null;

        /// <summary>
        /// The method name of the request, which indicates the action to be
        /// performed.
        /// </summary>
        public string Method => _rootElement.GetProperty( "method" ).GetString();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRpcRequest"/> class
        /// by deserializing a JSON payload from the specified stream.
        /// </summary>
        /// <param name="stream">The input stream containing the JSON payload to deserialize. Must not be <see langword="null"/>.</param>
        public JsonRpcRequest( Stream stream )
        {
            _rootElement = JsonSerializer.Deserialize<JsonElement>( stream, McpServer.JsonSerializerOptions );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the parameters of the request as a strongly typed object.
        /// </summary>
        /// <typeparam name="T">The type of object to decode the parameters into.</typeparam>
        /// <returns>An instance of <typeparamref name="T"/> or the default value, such as <c>null</c>, if it could not be parsed.</returns>
        public T GetParameters<T>()
        {
            if ( _rootElement.TryGetProperty( "params", out var parameters ) )
            {
                try
                {
                    return JsonSerializer.Deserialize<T>( parameters.GetRawText(), McpServer.JsonSerializerOptions );
                }
                catch
                {
                    // If deserialization fails, we can return default value.
                    // This allows for cases where parameters are not required or are empty.
                    return default;
                }
            }

            return default;
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
}
