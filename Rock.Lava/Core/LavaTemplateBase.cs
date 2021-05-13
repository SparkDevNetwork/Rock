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
        /// Try to render the template.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <returns></returns>
        public string Render()
        {
            return Render( context:null );
        }

        /// <summary>
        /// Try to render the template using the provided context values.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public string Render( IDictionary<string, object> values )
        {
            string output;
            List<Exception> errors;

            var isValid = TryRender( values, out output, out errors );

            if ( !isValid )
            {
                // Append error messages to the output.
                foreach ( var error in errors )
                {
                    output += "\nLava Error: " + error.Message;
                }
            }

            return output;
        }

        /// <summary>
        /// Try to render the template using the provided context.
        /// Errors will be included in the rendered output.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string Render( ILavaRenderContext context )
        {
            string output;
            List<Exception> errors;

            var isValid = TryRender( context, out output, out errors );

            if ( !isValid )
            {
                // Append error messages to the output.
                foreach ( var error in errors )
                {
                    output += "\nLava Error: " + error.Message;
                }
            }

            return output;
        }

        /// <summary>
        /// Try to render the template using the provided merge fields.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool TryRender( IDictionary<string, object> values, out string output, out List<Exception> errors )
        {
            var context = LavaEngine.CurrentEngine.NewRenderContext( values );

            return TryRender( context, out output, out errors );
        }

        /// <summary>
        /// Try to render the template using the provided context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool TryRender( ILavaRenderContext context, out string output, out List<Exception> errors )
        {
            var parameters = new LavaRenderParameters { Context = context };

            return TryRender( parameters, out output, out errors );
        }

        /// <summary>
        /// Try to render the template using the provided settings.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool TryRender( LavaRenderParameters parameters, out string output, out List<Exception> errors )
        {
            if ( parameters == null )
            {
                parameters = new LavaRenderParameters();
            }

            return TryRenderInternal( parameters, out output, out errors );
        }

        /// <summary>
        /// Render the template using the Lava Engine.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        private bool TryRenderInternal( LavaRenderParameters parameters, out string output, out List<Exception> errors )
        {
            errors = new List<Exception>();

            var result = LavaEngine.CurrentEngine.RenderTemplate( this, parameters );

            output = result.Text;
            errors = result.Errors;

            return result.HasErrors;
        }
    }
}
