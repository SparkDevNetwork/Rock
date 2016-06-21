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
    public partial class ClearPreviousMigrations5 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DROP INDEX[IX_ValueAsNumeric] ON[AttributeValue]
    ALTER TABLE AttributeValue DROP COLUMN ValueAsNumeric
    ALTER TABLE AttributeValue ADD ValueAsNumeric AS(
        CASE
            WHEN len([value] ) < ( 100 )
                THEN CASE
                        WHEN(
                                isnumeric([value] ) = ( 1 )
                                AND NOT[value] LIKE '%[^0-9.]%'
                                )
                            THEN TRY_CAST([value] AS[decimal]( 29, 4 ))
                        END
            END
        ) PERSISTED
    CREATE NONCLUSTERED INDEX[IX_ValueAsNumeric] ON[AttributeValue]([ValueAsNumeric] )

    UPDATE [__MigrationHistory] SET [Model] = 0x
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
