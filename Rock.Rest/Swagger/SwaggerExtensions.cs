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
using System.Collections.Generic;

using Swashbuckle.Swagger;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// 
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Adds a new parameter and returns a reference to parameter list
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="name">The name.</param>
        /// <param name="in">The in.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The type.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="format">The format.</param>
        /// <param name="enum">The enum.</param>
        /// <returns></returns>
        public static IList<Parameter> Parameter(this IList<Parameter> parameters, string name, string @in, string description, string @type, bool required, string format = null, object[] @enum = null )
        {
            parameters.Add( new Parameter
            {
                name = name,
                @type = @type,
                @in = @in,
                description = description,
                required = required,
                format = format,
                @enum = @enum
            } );

            return parameters;
        }
    }
}
