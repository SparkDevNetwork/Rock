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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Escapes the display text emitted by the Defined Value Picker and Campus Picker
    /// Lava shortcodes so values containing an apostrophe are no longer truncated. Fix for issue #7003.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 317, "19.5" )]
    public class FixPickerShortcodeApostropheTruncation7003 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                8/31/2026 - NA

                The Defined Value Picker and Campus Picker shortcodes build their options by emitting
                a nested [[ item text:'{{ value }}' ]] element, where the display text is wrapped in
                single quotes. The shortcode engine re-parses those child parameters with the pattern
                :'[^']+', which stops at the first apostrophe, so a value like "Men's Ministry" was
                truncated to "Men". Our general solution used elsewhere (for example the KPI shortcode)
                is to pipe such values through the Escape filter, which converts the apostrophe to
                &#39; - the parser no longer treats it as a delimiter and the browser renders it back
                as an apostrophe, matching how the equivalent C# controls behave.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/7003
            */

            // Escape the Defined Value Picker display text (both the Value and Description variants,
            // used across the checkbox-list and dropdown branches).
            Sql( @"
UPDATE [LavaShortcode]
SET [Markup] = REPLACE(
        REPLACE([Markup], '{{ definedvalue.Value }}', '{{ definedvalue.Value | Escape }}'),
        '{{ definedvalue.Description }}', '{{ definedvalue.Description | Escape }}'
        )
WHERE [Guid] = 'E2FC377F-EDCE-4FD3-B734-D06939E65210'
" );

            // Escape the Campus Picker display text (used across the checkbox-list and dropdown branches).
            Sql( @"
UPDATE [LavaShortcode]
SET [Markup] = REPLACE([Markup],
    '{{ campus.Name }}', '{{ campus.Name | Escape }}')
WHERE [Guid] = 'E787B188-2E0F-479E-A855-0E4ABA75C91B'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
