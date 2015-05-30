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
    public partial class PersonGivingId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
ALTER TABLE PERSON ADD [GivingId] as (
	CASE WHEN [GivingGroupId] IS NOT NULL 
		THEN 'G' + CAST([GivingGroupId] AS varchar) 
		ELSE 'P' + CAST([Id] AS varchar)
	END
)
" );
            CreateIndex( "dbo.Person", "GivingId" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.Person", new string[] { "GivingId" } );
            DropColumn( "dbo.Person", "GivingId" );
        }
    }
}
