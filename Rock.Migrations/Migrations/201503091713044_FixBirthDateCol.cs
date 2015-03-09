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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class FixBirthDateCol : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    ALTER TABLE [dbo].[Person] DROP COLUMN [BirthDate]
    ALTER TABLE [dbo].[Person] ADD [BirthDate] AS CASE WHEN ISDATE(
        RIGHT('0000' + CAST([birthyear] as varchar(4)), 4) + RIGHT('00' + CAST([birthmonth] as varchar(2)), 2) + RIGHT('00' + CAST([birthday] as varchar(2)), 2)
        ) = 1 THEN CONVERT( [date],
        RIGHT('0000' + CAST([birthyear] as varchar(4)), 4) + RIGHT('00' + CAST([birthmonth] as varchar(2)), 2) + RIGHT('00' + CAST([birthday] as varchar(2)), 2)
        ) ELSE null END
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
