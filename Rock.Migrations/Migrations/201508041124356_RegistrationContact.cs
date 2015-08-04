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
    public partial class RegistrationContact : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.RegistrationInstance", "ContactPersonAliasId", c => c.Int());
            CreateIndex("dbo.RegistrationInstance", "ContactPersonAliasId");
            AddForeignKey("dbo.RegistrationInstance", "ContactPersonAliasId", "dbo.PersonAlias", "Id");
            DropColumn("dbo.RegistrationInstance", "ContactName");

            // JE: Delete redundant defined type
            RockMigrationHelper.DeleteDefinedType( "0F48CB3F-8A48-249A-412A-2DCA7648706F" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.RegistrationInstance", "ContactName", c => c.String(maxLength: 200));
            DropForeignKey("dbo.RegistrationInstance", "ContactPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RegistrationInstance", new[] { "ContactPersonAliasId" });
            DropColumn("dbo.RegistrationInstance", "ContactPersonAliasId");
        }
    }
}
