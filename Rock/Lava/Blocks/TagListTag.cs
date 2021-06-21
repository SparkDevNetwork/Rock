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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Tag which allows you to list all of the registered Lava commands on your server.
    /// </summary>
    public class TagListTag : LavaTagBase
    {
        /// <summary>
        /// Renders the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            var tags = LavaService.GetRegisteredElements();

            if ( !tags.Any() )
            {
                return;
            }

            var tagList = new StringBuilder();

            tagList.Append( "<strong>Lava Tag List</strong>" );
            tagList.Append( "<ul>" );

            foreach ( var kvp in tags.OrderBy( t => t.Key ) )
            {
                var tag = kvp.Value;

                var tagName = LavaUtilityHelper.GetShortcodeNameFromLiquidElementName( tag.Name );

                tagList.Append( $"<li>{tagName} - {tag.SystemTypeName}</li>" );
            }

            tagList.Append( "</ul>" );

            result.Write( tagList.ToString() );

            base.OnRender( context, result );
        }
    }
}