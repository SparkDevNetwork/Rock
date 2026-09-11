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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Converts the HTML Content block's "Is Secondary Block" attribute into the
    /// per-instance Block Role used by Obsidian blocks. Instances that had the
    /// attribute set to True are given the Secondary role and the attribute is
    /// then removed, following the Page Menu conversion in
    /// 202512081542399_UpdatePageMenuSecondaryRoleHandlingAndRemove.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 321, "21.0" )]
    public class ConvertHtmlContentIsSecondaryBlockToBlockRole : Migration
    {
        /// <summary>
        /// The HTML Content block type.
        /// </summary>
        private const string HtmlContentBlockTypeGuid = "19B61D65-37E3-459F-A44F-DEF0089118A3";

        /// <summary>
        /// The "Is Secondary Block" attribute that belonged to the WebForms HTML Content block.
        /// </summary>
        private const string IsSecondaryBlockAttributeGuid = "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
                UPDATE [b]
                SET [b].[Role] = {( int ) Rock.Enums.Cms.BlockRole.Secondary}
                FROM [AttributeValue] AS [av]
                INNER JOIN [Attribute] AS [a] ON [a].[Id] = [av].[AttributeId]
                INNER JOIN [BlockType] AS [bt] ON [bt].[Id] = [a].[EntityTypeQualifierValue]
                INNER JOIN [Block] AS [b] ON [b].[Id] = [av].[EntityId]
                WHERE [a].[Guid] = '{IsSecondaryBlockAttributeGuid}'
                    AND [a].[Key] = 'IsSecondaryBlock'
                    AND [bt].[Guid] = '{HtmlContentBlockTypeGuid}'
                    AND [av].[Value] = 'True'" );

            // The Obsidian block no longer declares this attribute, so remove it rather than leave an orphan.
            RockMigrationHelper.DeleteAttribute( IsSecondaryBlockAttributeGuid );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
