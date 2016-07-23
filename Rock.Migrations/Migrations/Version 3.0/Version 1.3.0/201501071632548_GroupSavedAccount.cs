﻿// <copyright>
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
    public partial class GroupSavedAccount : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.FinancialPersonSavedAccount", "PersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "PersonAliasId" });
            AddColumn("dbo.FinancialPersonSavedAccount", "GroupId", c => c.Int());
            AlterColumn("dbo.FinancialPersonSavedAccount", "PersonAliasId", c => c.Int());
            CreateIndex("dbo.FinancialPersonSavedAccount", "PersonAliasId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "GroupId");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "GroupId", "dbo.Group", "Id");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "PersonAliasId", "dbo.PersonAlias", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialPersonSavedAccount", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "GroupId", "dbo.Group");
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "GroupId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "PersonAliasId" });
            AlterColumn("dbo.FinancialPersonSavedAccount", "PersonAliasId", c => c.Int(nullable: false));
            DropColumn("dbo.FinancialPersonSavedAccount", "GroupId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "PersonAliasId");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "PersonAliasId", "dbo.PersonAlias", "Id", cascadeDelete: true);
        }
    }
}
