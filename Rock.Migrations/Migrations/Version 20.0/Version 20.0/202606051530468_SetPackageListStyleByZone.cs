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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class SetPackageListStyleByZone : Rock.Migrations.RockMigration
    {
        private const string BlockTypeGuid = "470C6EFF-091C-4593-848C-49547D0EBEEE";

        // Display Style block setting attribute.
        private const string DisplayStyleAttributeGuid = "3768DBF1-F41A-49ED-A1AF-91032CC58EE8";

        // Single Select field type (backs the Sidebar/Header radio list).
        private const string SingleSelectFieldTypeGuid = Rock.SystemGuid.FieldType.SINGLE_SELECT;

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Ensure the Display Style attribute exists on the block type before setting values.
            // This migration can run before Rock reconciles the block's code-defined attributes
            // at startup, so we register it here idempotently.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                BlockTypeGuid,
                SingleSelectFieldTypeGuid,
                "Display Style",
                "DisplayStyle",
                "Display Style",
                "Determines how the category list is rendered: a vertical sidebar list or a horizontal header pill bar.",
                2,
                "Sidebar",
                DisplayStyleAttributeGuid );

            // Set Display Style on each existing block from its zone. Only blocks without an
            // existing value are touched, so any later admin choice is preserved on re-run.
            Sql( $@"
                    DECLARE @BlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{BlockTypeGuid}' );
                    DECLARE @DisplayStyleAttributeId INT = (
                        SELECT [Id] FROM [Attribute]
                        WHERE [EntityTypeQualifierColumn] = 'BlockTypeId'
                            AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS NVARCHAR( 20 ) )
                            AND [Key] = 'DisplayStyle' );

                    IF @BlockTypeId IS NOT NULL AND @DisplayStyleAttributeId IS NOT NULL
                    BEGIN
                        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
                        SELECT 0,
                            @DisplayStyleAttributeId,
                            [b].[Id],
                            CASE WHEN [b].[Zone] LIKE '%Sidebar%' THEN 'Sidebar' ELSE 'Header' END,
                            NEWID()
                        FROM [Block] AS [b]
                        WHERE [b].[BlockTypeId] = @BlockTypeId
                            AND NOT EXISTS (
                                SELECT 1 FROM [AttributeValue] AS [av]
                                WHERE [av].[AttributeId] = @DisplayStyleAttributeId
                                    AND [av].[EntityId] = [b].[Id] );
                    END
                " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}

