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
#if WEBFORMS
    /// <summary>
    /// A forward compatible class for the way ASP.Net Core will handle response
    /// type decorations.
    /// </summary>
    public class ProducesResponseTypeAttribute : Swashbuckle.Swagger.Annotations.SwaggerResponseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesResponseTypeAttribute"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public ProducesResponseTypeAttribute( int statusCode )
            : base( statusCode )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesResponseTypeAttribute"/> class.
        /// </summary>
        /// <param name="type">The type of the response object.</param>
        /// <param name="statusCode">The status code.</param>
        public ProducesResponseTypeAttribute( Type type, int statusCode )
            : base( statusCode, null, type )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesResponseTypeAttribute"/> class.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        public ProducesResponseTypeAttribute( HttpStatusCode statusCode )
            : base( statusCode )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesResponseTypeAttribute"/> class.
        /// </summary>
        /// <param name="type">The type of the response object.</param>
        /// <param name="statusCode">The status code.</param>
        public ProducesResponseTypeAttribute( Type type, HttpStatusCode statusCode )
            : base( statusCode, null, type )
        {
        }
    }
#endif
}
