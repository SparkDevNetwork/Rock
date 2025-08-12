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

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Rock.AI.Agent.Mcp.Protocol;

namespace Rock.AI.Agent.Mcp
{
    /// <summary>
    /// Represents the result of a JSON-RPC request.
    /// </summary>
    internal class JsonRpcResult
    {
        #region Properties

        /// <summary>
        /// The version of the JSON-RPC protocol being used.
        /// </summary>
        [JsonPropertyName( "jsonrpc" )]
        public string Version { get; } = "2.0";

        /// <summary>
        /// The identifier of the request this result corresponds to.
        /// </summary>
        public long Id { get; }

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
        internal JsonRpcResult( long id, object result )
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
        internal JsonRpcResult( long id, int errorCode, string errorMessage )
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
        /// Writes the JSON-RPC result to a stream in JSON format.
        /// </summary>
        /// <param name="stream">The stream that the result should be written to.</param>
        public void ToJson( Stream stream )
        {
            JsonSerializer.Serialize( stream, this, McpServer.JsonSerializerOptions );
            stream.Flush();
        }

        #endregion
    }
}
