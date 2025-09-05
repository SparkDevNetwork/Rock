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
using System.Text;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Formats instruction settings for skills and tools.
    /// </summary>
    internal static class InstructionFormatter
    {
        /// <summary>
        /// Appends a single item to the string builder if it is not empty.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> instance.</param>
        /// <param name="elementName">The name of the XML element to wrap the <paramref name="item"/> in.</param>
        /// <param name="item">The item to be appended to <paramref name="builder"/>.</param>
        private static void AppendItem( StringBuilder builder, string elementName, string item )
        {
            if ( item.IsNotNullOrWhiteSpace() )
            {
                builder.Append( $"<{elementName}>{item}</{elementName}>" );
            }
        }

        /// <summary>
        /// Appends a set of items to the string builder if they are not empty.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> instance.</param>
        /// <param name="elementName">The name of the XML element to wrap the <paramref name="item"/> in.</param>
        /// <param name="items">The items to be appended to <paramref name="builder"/>.</param>
        private static void AppendItems( StringBuilder builder, string elementName, ICollection<string> items )
        {
            if ( items == null || items.Count == 0 )
            {
                return;
            }

            builder.AppendLine( $"<{elementName}s>" );

            foreach ( var item in items )
            {
                if ( item.IsNotNullOrWhiteSpace() )
                {
                    builder.AppendLine( $" <{elementName}>{item}</{elementName}>" );
                }
            }

            builder.AppendLine( $"</{elementName}s>" );
        }

        /// <summary>
        /// Formats the instructions for a skill based on the provided settings.
        /// </summary>
        /// <param name="settings">The settings that contain all the instruction details.</param>
        /// <returns>A string that will be sent to the language model that contains all the instructions.</returns>
        public static string FormatInstructions( SkillInstructionSettings settings )
        {
            if ( settings == null )
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            AppendItems( builder, "purpose", settings.Purposes );
            AppendItems( builder, "usage", settings.Usages );
            AppendItems( builder, "guardrail", settings.Guardrails );

            return builder.ToString();
        }

        /// <summary>
        /// Formats the instructions for a tool based on the provided settings.
        /// </summary>
        /// <param name="settings">The settings that contain all the instruction details.</param>
        /// <returns>A string that will be sent to the language model that contains all the instructions.</returns>
        public static string FormatInstructions( ToolInstructionSettings settings )
        {
            if ( settings == null )
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            AppendItems( builder, "purpose", settings.Purposes );
            AppendItem( builder, "returnDescription", settings.ReturnDescription );
            AppendItems( builder, "usage", settings.Usages );
            AppendItems( builder, "guardrail", settings.Guardrails );
            AppendItems( builder, "prerequisite", settings.Prerequisites );
            AppendItems( builder, "example", settings.Examples );

            return builder.ToString();
        }
    }
}
