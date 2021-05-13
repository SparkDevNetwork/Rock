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

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Processes the Lava code in the source string.
        /// </summary>
        /// <param name="input">The lava source to process.</param>
        /// <example><![CDATA[
        /// {% capture lava %}{% raw %}{% assign test = "hello" %}{{ test }}{% endraw %}{% endcapture %}
        /// {{ lava | RunLava }}
        /// ]]></example>
        public static string RunLava( ILavaRenderContext context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            var result = LavaEngine.CurrentEngine.RenderTemplate( input.ToString(), new LavaRenderParameters { Context = context } );

            return result.Text;
        }
    }
}
