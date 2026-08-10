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
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 302, "18.4" )]
    public class MigrationRollupsForV18_4_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChangeConnectionStatusColorAttributeToColorPickerPart2_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        private void ChangeConnectionStatusColorAttributeToColorPickerPart2_Up()
        {
            // Fix to address migration https://github.com/SparkDevNetwork/Rock/blame/980777e51eb4e3343f3707e9d173cddcf6d3c666/Rock.Migrations/Migrations/Version%2018.0/Version%2018.0/202509301628253_Rollup_20250930.cs#L594
            // in the event that an old custom Rock instance did was built without the original 1121 attributeId from original v1.0 beta migration 201407091948108_GroupMap.cs
            Sql( @"DECLARE @ColorAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '23777A50-E000-4F29-994F-26635A357160' )

-- add a new [AttributeQualifier] of 'selectiontype' (Key) and 'Color Picker' (Value)
IF NOT EXISTS (
    SELECT 1
    FROM AttributeQualifier
    WHERE AttributeId = @ColorAttributeId
      AND [Key] = 'selectiontype'
)
BEGIN
    INSERT INTO AttributeQualifier ([IsSystem], [AttributeId], [Key], [Value], [Guid])
    VALUES (1, @ColorAttributeId, 'selectiontype', 'Color Picker', NEWID() );
END" );
        }
    }
}
