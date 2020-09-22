// <copyright>
// Copyright by BEMA Software Services
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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 4, "1.9.4" )]
    public class UpdateFieldType : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                    Delete
                    From FieldType
                    Where Class = 'com.bemaservices.RoomManagement.Field.Types.ReservationFieldType'
                " );
            UpdateFieldTypeByGuid( "Reservation", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7" );

            Sql( @"
                    Delete
                    From FieldType
                    Where Class = 'com.bemaservices.RoomManagement.Field.Types.ReservationFieldType'
                " );
            UpdateFieldTypeByGuid( "ReservationStatus", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReservationStatusFieldType", "D3D17BE3-33BF-4CDF-89E1-F70C57317B4E" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteFieldType( "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7" );
            RockMigrationHelper.DeleteFieldType( "D3D17BE3-33BF-4CDF-89E1-F70C57317B4E" );
        }

        public void UpdateFieldTypeByGuid( string name, string description, string assembly, string className, string guid, bool IsSystem = true )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{4}' )
                BEGIN
                    UPDATE [FieldType] SET
                        [Name] = '{0}',
                        [Description] = '{1}',
                        [Guid] = '{4}',
                        [IsSystem] = {5},
                        [Assembly] = '{2}',
                        [Class] = '{3}'
                    WHERE [Guid] = '{4}'
                END
                ELSE
                BEGIN
                    DECLARE @Id int
                    SET @Id = (SELECT [Id] FROM [FieldType] WHERE [Assembly] = '{2}' AND [Class] = '{3}')
                    IF @Id IS NULL
                    BEGIN
                        INSERT INTO [FieldType] (
                            [Name],[Description],[Assembly],[Class],[Guid],[IsSystem])
                        VALUES(
                            '{0}','{1}','{2}','{3}','{4}',{5})
                    END
                    ELSE
                    BEGIN
                        UPDATE [FieldType] SET
                            [Name] = '{0}',
                            [Description] = '{1}',
                            [Guid] = '{4}',
                            [IsSystem] = {5}
                        WHERE [Assembly] = '{2}'
                        AND [Class] = '{3}'
                    END
                END
",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    assembly,
                    className,
                    guid,
                    IsSystem ? "1" : "0" ) );
        }
    }
}
