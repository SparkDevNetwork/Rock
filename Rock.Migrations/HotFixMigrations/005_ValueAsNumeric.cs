// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using Rock.Plugin;

namespace Rock.Migrations.HotFixMigrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 5, "1.4.1" )]
    public class ValueAsNumeric : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DROP INDEX [IX_ValueAsNumeric] ON [AttributeValue]
    ALTER TABLE AttributeValue DROP COLUMN ValueAsNumeric
    ALTER TABLE AttributeValue ADD ValueAsNumeric AS (
        CASE 
            WHEN len([value]) < (100)
                THEN CASE 
                        WHEN (
                                isnumeric([value]) = (1)
                                AND NOT [value] LIKE '%[^0-9.]%'
                                )
                            THEN TRY_CAST([value] AS [decimal](29,4))
                        END
            END
        ) PERSISTED
    CREATE NONCLUSTERED INDEX [IX_ValueAsNumeric] ON [AttributeValue] ([ValueAsNumeric] )
" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
