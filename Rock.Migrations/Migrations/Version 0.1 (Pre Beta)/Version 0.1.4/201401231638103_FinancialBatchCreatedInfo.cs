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
    public partial class FinancialBatchCreatedInfo : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE B SET 
        [CreatedByPersonAliasId] = A.[Id]
    FROM [FinancialBatch] B
    INNER JOIN [PersonAlias] A ON A.[AliasPersonId] = B.[CreatedByPersonId]
" );

            DropForeignKey( "dbo.FinancialBatch", "CreatedByPersonId", "dbo.Person" );
            DropIndex("dbo.FinancialBatch", new[] { "CreatedByPersonId" });
            DropColumn("dbo.FinancialBatch", "CreatedByPersonId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialBatch", "CreatedByPersonId", c => c.Int(nullable: false));
            CreateIndex("dbo.FinancialBatch", "CreatedByPersonId");
            AddForeignKey("dbo.FinancialBatch", "CreatedByPersonId", "dbo.Person", "Id");

            Sql( @"
    UPDATE B SET 
        [CreatedByPersonId] = A.[PersonId]
    FROM [FinancialBatch] B
    INNER JOIN [PersonAlias] A ON A.[Id] = B.[CreatedByPersonAliasId]
" );
        }
    }
}
