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
    public partial class AddMapMarkersDefinedType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            @Sql( $@"IF NOT EXISTS(SELECT 1 FROM DefinedType WHERE [Guid] = '{Rock.SystemGuid.DefinedType.MAP_MARKERS}')
                    BEGIN
                        DECLARE @DefinedTypeEntityTypeId int = (
                            SELECT TOP 1 [Id]
                            FROM [EntityType]
                            WHERE [Name] = 'Rock.Model.DefinedType' )

                        DECLARE @CategoryId int = (
                            SELECT TOP 1 [Id] FROM [Category]
                            WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
                            AND [Name] = 'Global' )

	                    INSERT INTO DefinedType (IsSystem, FieldTypeId, [Order], [Name], [Description], [Guid], HelpText, CategoryId, IsActive)
	                    VALUES (1, 1, (SELECT MAX([Order]) FROM DefinedType), 'Map Markers', 'Markers that can be used by the group finder block.', '{Rock.SystemGuid.DefinedType.MAP_MARKERS}', '', @CategoryId, 1)

	                    DECLARE @definedValueId AS INT
	                    SET @definedValueId = @@IDENTITY

	                    INSERT INTO DefinedValue (IsSystem, DefinedTypeId, [Order], [Value], [Description], [Guid], IsActive)
	                    VALUES 
	                    (1, @definedValueId, 0, 'Pin', 'M-2.8-9.2V4.3l1.9,2.8c0.4,0.6,1.3,0.6,1.7,0l1.9-2.8V-9.2C1.9-9,0.9-8.9,0-8.9S-1.9-9-2.8-9.2z M0-36.4 c-6.8,0-12.4,5.5-12.4,12.4S-6.8-11.7,0-11.7s12.4-5.5,12.4-12.4S6.8-36.4,0-36.4z', '{Rock.SystemGuid.DefinedValue.MAP_MARKER_PIN}', 1),
	                    (1, @definedValueId, 1, 'Marker', 'M-1.8,11.2c-13.5-19.5-16-21.5-16-28.7c0-9.8,8-17.8,17.8-17.8s17.8,8,17.8,17.8c0,7.2-2.5,9.2-16,28.7 C0.9,12.5-0.9,12.5-1.8,11.2L-1.8,11.2z', '{Rock.SystemGuid.DefinedValue.MAP_MARKER_MARKER}', 1),
	                    (1, @definedValueId, 2, 'Marker With Dot', 'M-1.8,11.2c-13.5-19.5-16-21.5-16-28.7c0-9.8,8-17.8,17.8-17.8s17.8,8,17.8,17.8c0,7.2-2.5,9.2-16,28.7 C0.9,12.5-0.9,12.5-1.8,11.2L-1.8,11.2z M0-10.1c4.1,0,7.4-3.3,7.4-7.4S4.1-24.9,0-24.9s-7.4,3.3-7.4,7.4S-4.1-10.1,0-10.1z', '{Rock.SystemGuid.DefinedValue.MAP_MARKER_MARKER_WITH_DOT}', 1),
	                    (1, @definedValueId, 3, 'Circle', 'M0-11.3c-6.3,0-11.3,5.1-11.3,11.3S-6.3,11.3,0,11.3S11.3,6.3,11.3,0S6.3-11.3,0-11.3z', '{Rock.SystemGuid.DefinedValue.MAP_MARKER_CIRCLE}', 1)
                    END" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
