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
    public partial class RemoveDefaultLegacySupportLevelOption : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RemoveLegacyFromLavaSupportLevelUp();            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // No can do. 'Legacy' option is gone for good.
        }

        /// <summary>
        /// Removes the 'Legacy' from the Lava Support Level Global Attribute and
        /// the following:
        ///   * Remove 'Legacy' option from 'Lava Support Level' global attribute.
        ///   * Set new default to 'NoLegacy'
        ///   * Change existing 'Legacy' setting value to 'LegacyWithWarning'
        ///   * If no value previously existed, create an explicit setting value of 'LegacyWithWarning'
        /// </summary>
        private void RemoveLegacyFromLavaSupportLevelUp()
        {
            Sql( @"
-- Remove 'Legacy' option from 'Lava Support Level' global attribute.
-- Set new default to 'NoLegacy'
-- Change existing 'Legacy' setting value to 'LegacyWithWarning'
-- If no value previously existed, create an explicit setting value of 'LegacyWithWarning'

DECLARE @LavaSupportLevelAttributeId int = ( SELECT TOP 1 [Id] FROM[Attribute] WHERE [Guid] = 'C8E30F2B-7476-4B02-86D4-3E5057F03FD5' )

UPDATE[AttributeQualifier]
    SET[IsSystem] = 1, [Value] = 'LegacyWithWarning, NoLegacy'
    WHERE[AttributeId] = @LavaSupportLevelAttributeId AND[Key] = 'values'

UPDATE[Attribute]
    SET[DefaultValue] = 'NoLegacy', [Description] = 'NoLegacy is fastest, but the old Lava syntax is supported if set to LegacyWithWarning.'
    WHERE[Id] = @LavaSupportLevelAttributeId 

-- If there was no explicit attributevalue, it would mean they were running on default 'Legacy'.
-- In this case insert a new record setting, them explicitly 'LegacyWithWarning'.
IF( NOT EXISTS (SELECT[Value] FROM [AttributeValue] WHERE [AttributeId] = @LavaSupportLevelAttributeId ) )
BEGIN
    INSERT INTO AttributeValue( [IsSystem], [AttributeId], [Value], [Guid] )
    VALUES(
    0--[IsSystem]
    , @LavaSupportLevelAttributeId--[AttributeId]
    , 'LegacyWithWarning'--[Value]
    , NEWID()--[Guid]
    )
END
ELSE
BEGIN
	-- If there was row AND it was 'Legacy', update it to 'LegacyWithWarning'
	IF( (SELECT[Value] FROM [AttributeValue] WHERE [AttributeId] = @LavaSupportLevelAttributeId ) = 'Legacy')
	BEGIN
        UPDATE[AttributeValue]

        SET[Value] = 'LegacyWithWarning'
		WHERE[AttributeId] = @LavaSupportLevelAttributeId AND[Value] = 'Legacy'

    END
END
" );
        }
    }
}
