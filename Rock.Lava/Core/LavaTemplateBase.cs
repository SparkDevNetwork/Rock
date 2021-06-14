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
    /// Provides base functionality for a Lava template.
    /// </summary>
    public abstract class LavaTemplateBase : ILavaTemplate
    {
        /// <summary>
        /// Try to render the template using the provided context values.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public string Render( IDictionary<string, object> values )
        {
            var context = LavaService.NewRenderContext( values );

            var result = Render( new LavaRenderParameters { Context = context } );

            return result.Text;
        }

        /// <summary>
        /// Try to render the template using the provided context.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string Render( ILavaRenderContext context )
        {
            var result = Render( new LavaRenderParameters { Context = context } );

            return result.Text;
        }

        /// <summary>
        /// Try to render the template using the provided settings.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public LavaRenderResult Render( LavaRenderParameters parameters )
        {
            return LavaService.RenderTemplate( this, parameters );
        }
    }
}
