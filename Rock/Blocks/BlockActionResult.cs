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
using System.Net;

namespace Rock.Blocks
{
    /// <summary>
    /// Describes the result of a BlockAction in a platform agnostic way.
    /// </summary>
    public class BlockActionResult
    {
        #region Properties

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        /// <value>
        /// The HTTP status code.
        /// </value>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the CLR type of the content.
        /// </summary>
        /// <value>
        /// The CLR type of the content.
        /// </value>
        public Type ContentClrType { get; private set; }

        /// <summary>
        /// Gets the content data to be sent back in the response body.
        /// </summary>
        /// <value>
        /// The content to be sent back in the response body.
        /// </value>
        public object Content { get; set; }

        /// <summary>
        /// Gets the error message to be sent back.
        /// </summary>
        /// <value>
        /// The error message to be sent back.
        /// </value>
        public string Error { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockActionResult"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public BlockActionResult( HttpStatusCode statusCode )
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockActionResult"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The content.</param>
        public BlockActionResult( HttpStatusCode statusCode, object content )
        {
            StatusCode = statusCode;
            Content = content;
            ContentClrType = content.GetType();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockActionResult"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="content">The content.</param>
        /// <param name="clrType">Type of the content.</param>
        public BlockActionResult( HttpStatusCode statusCode, object content, Type clrType )
        {
            StatusCode = statusCode;
            Content = content;
            ContentClrType = clrType;
        }

        #endregion
    }
}
