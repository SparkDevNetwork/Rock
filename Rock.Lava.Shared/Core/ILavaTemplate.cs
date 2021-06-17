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
using System.Collections.Generic;

namespace Rock.Lava
{
    /// <summary>
    /// Represents a Lava Template that has been parsed and compiled, and is ready to be rendered.
    /// </summary>
    public interface ILavaTemplate
    {
        /// <summary>
        /// Try to render the template using the provided context values.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        string Render( IDictionary<string, object> values );

        /// <summary>
        /// Try to render the template using the provided context.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        string Render( ILavaRenderContext context );

        /// <summary>
        /// Try to render the template using the provided render parameters.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        LavaRenderResult Render( LavaRenderParameters parameters );
    }
}
