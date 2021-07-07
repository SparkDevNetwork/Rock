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
	                    INSERT INTO DefinedType (IsSystem, FieldTypeId, [Order], [Name], [Description], [Guid], HelpText, CategoryId, IsActive)
	                    VALUES (1, 1, (SELECT MAX([Order]) FROM DefinedType), 'Map Markers', 'Markers that can be used by the group finder block.', '{Rock.SystemGuid.DefinedType.MAP_MARKERS}', '', NULL, 1)

	                    DECLARE @definedValueId AS INT
	                    SET @definedValueId = @@IDENTITY

	                    INSERT INTO DefinedValue (IsSystem, DefinedTypeId, [Order], [Value], [Description], [Guid], IsActive)
	                    VALUES 
	                    (1, @definedValueId, 0, 'Pin', 'M112 316.94v156.69l22.02 33.02c4.75 7.12 15.22 7.12 19.97 0L176 473.63V316.94c-10.39 1.92-21.06 3.06-32 3.06s-21.61-1.14-32-3.06zM144 0C64.47 0 0 64.47 0 144s64.47 144 144 144 144-64.47 144-144S223.53 0 144 0zm0 76c-37.5 0-68 30.5-68 68 0 6.62-5.38 12-12 12s-12-5.38-12-12c0-50.73 41.28-92 92-92 6.62 0 12 5.38 12 12s-5.38 12-12 12z', '{Rock.SystemGuid.DefinedValue.MAP_MARKER_PIN}', 1),
	                    (1, @definedValueId, 1, 'Marker', 'M172.268 501.67C26.97 291.031 0 269.413 0 192 0 85.961 85.961 0 192 0s192 85.961 192 192c0 77.413-26.97 99.031-172.268 309.67-9.535 13.774-29.93 13.773-39.464 0z', '{Rock.SystemGuid.DefinedValue.MAP_MARKER_MARKER}', 1),
	                    (1, @definedValueId, 2, 'Marker With Dot', 'M172.268 501.67C26.97 291.031 0 269.413 0 192 0 85.961 85.961 0 192 0s192 85.961 192 192c0 77.413-26.97 99.031-172.268 309.67-9.535 13.774-29.93 13.773-39.464 0zM192 272c44.183 0 80-35.817 80-80s-35.817-80-80-80-80 35.817-80 80 35.817 80 80 80z', '{Rock.SystemGuid.DefinedValue.MAP_MARKER_MARKER_WITH_DOT}', 1),
	                    (1, @definedValueId, 3, 'Circle', 'M256 8C119 8 8 119 8 256s111 248 248 248 248-111 248-248S393 8 256 8z', '{Rock.SystemGuid.DefinedValue.MAP_MARKER_CIRCLE}', 1)
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
