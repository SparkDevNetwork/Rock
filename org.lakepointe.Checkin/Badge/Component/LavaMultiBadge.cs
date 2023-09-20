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

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using Rock;
using Rock.Attribute;
using Rock.Badge;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.Lakepointe.Checkin.Badge.Component
{
    /// <summary>
    /// Badge that displays a person's Assessments results.
    /// </summary>
    [Description( "Badge driven by a Lava block that can display multiple mini-badges." )]
    [Export( typeof( BadgeComponent ) )]
    [ExportMetadata( "ComponentName", "Lava Multi Badge" )]

    [CodeEditorField("Badge Algorithm",
        description: "Lava to be called for each attribute. Attribute is input; output (html) is a colored icon.",
        mode: Rock.Web.UI.Controls.CodeEditorMode.Lava,
        theme: Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        height: 200,
        required: false,
        order: 1)]

    [LavaCommandsField("Enabled Lava Commands",
        "Lava commands that are enabled.",
        required: false,
        order: 3)]

    public class LavaMultiBadge : BadgeComponent
    {
        private class AttributeKeys
        {
            public const string BadgeAlgorithm = "BadgeAlgorithm";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        /// <summary>
        /// Determines if this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public override bool DoesApplyToEntityType( string type )
        {
            return type.IsNullOrWhiteSpace() || typeof( Person ).FullName == type;
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public override void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
            if ( entity == null )
            {
                return;
            }

            var badgeAlgorithm = GetAttributeValue(badge, AttributeKeys.BadgeAlgorithm);
            if (badgeAlgorithm.IsNullOrWhiteSpace())
            {
                return;
            }

            var enabledLavaCommands = GetAttributeValue(badge, AttributeKeys.EnabledLavaCommands);

            var mergeValues = new Dictionary<string, object>
            {
                { "Person", entity }
            };

            writer.Write(badgeAlgorithm.ResolveMergeFields(mergeValues, enabledLavaCommands));
        }
    }
}
