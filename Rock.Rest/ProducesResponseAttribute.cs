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

namespace Rock.Rest
{
    /// <summary>
    /// Identifies which response(s) the APi endpoint can return.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true )]
    public class ProducesResponseAttribute : System.Attribute
    {
        /// <summary>
        /// The status code of this response.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// The type of the response object.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The description of the response.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesResponseAttribute"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public ProducesResponseAttribute( int statusCode )
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesResponseAttribute"/> class.
        /// </summary>
        /// <param name="type">The type of the response object.</param>
        /// <param name="statusCode">The status code.</param>
        public ProducesResponseAttribute( Type type, int statusCode )
        {
            Type = type;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesResponseAttribute"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public ProducesResponseAttribute( HttpStatusCode statusCode )
        {
            StatusCode = ( int ) statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesResponseAttribute"/> class.
        /// </summary>
        /// <param name="type">The type of the response object.</param>
        /// <param name="statusCode">The status code.</param>
        public ProducesResponseAttribute( Type type, HttpStatusCode statusCode )
        {
            Type = type;
            StatusCode = ( int ) statusCode;
        }
    }
}
