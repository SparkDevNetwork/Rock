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
    public partial class UpdateLegacyLava : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._201803201440110_UpdateLegacyLava );
            UpdateLavaSupportLevel();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Updates the lava support level to LegacyWithWarning if it is not set or is set to Legacy
        /// </summary>
        public void UpdateLavaSupportLevel()
        {
            Sql( @"DECLARE @LavaSupportLevelAttributeId INT = ( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C8E30F2B-7476-4B02-86D4-3E5057F03FD5' )

                DECLARE @LavaSupportLevelValueId int = ( SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @LavaSupportLevelAttributeId )

                --First check to see if a value exists and create it if not
                IF( @LavaSupportLevelValueId IS NULL )
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
                    -- If there is a row update it if it is Legacy, otherwise leave it alone

                    IF( ( SELECT [Value] FROM [AttributeValue] WHERE Id = @LavaSupportLevelValueId ) = 'Legacy')
	                BEGIN
                        UPDATE [AttributeValue]
                        SET [Value] = 'LegacyWithWarning'
                        WHERE [Id] = @LavaSupportLevelValueId
                    END
                END" );
        }
    }
}
