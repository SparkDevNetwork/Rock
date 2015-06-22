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
    public partial class EnsurePersonAlias : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ensure that there aren't any PersonAlias records that point to a Person that doesn't exist (so we don't break the new FK constraint)
            //Sql( "delete from PersonAlias where AliasPersonId != PersonId and AliasPersonId not in (select Id from Person)" );
            //AddForeignKey("dbo.PersonAlias", "AliasPersonId", "dbo.Person", "Id");

            Sql( MigrationSQL._201506162223455_EnsurePersonAlias_UpdateCheckinLabels );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonAlias", "AliasPersonId", "dbo.Person");
        }
    }
}
