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
    public partial class AuditPersonAlias : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.Audit", "PersonId", "dbo.Person" );
            DropIndex("dbo.Audit", new[] { "PersonId" });

            AddColumn( "dbo.Audit", "PersonAliasId", c => c.Int() );

            Sql( @"
    UPDATE T SET [PersonAliasId] = A.[Id]
    FROM [Audit] T
    INNER JOIN [PersonAlias] A ON A.[AliasPersonId] = T.[PersonId]
" );

            CreateIndex( "dbo.Audit", "PersonAliasId" );
            AddForeignKey( "dbo.Audit", "PersonAliasId", "dbo.PersonAlias", "Id", cascadeDelete: true );
            DropColumn( "dbo.Audit", "PersonId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.Audit", "PersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.Audit", new[] { "PersonAliasId" } );

            AddColumn("dbo.Audit", "PersonId", c => c.Int());

            Sql( @"
    UPDATE T SET [PersonId] = A.[PersonId]
    FROM [Audit] T
    INNER JOIN [PersonAlias] A ON A.[Id] = T.[PersonAliasId]
" ); 
            
            CreateIndex("dbo.Audit", "PersonId");
            AddForeignKey("dbo.Audit", "PersonId", "dbo.Person", "Id", cascadeDelete: true);
            DropColumn( "dbo.Audit", "PersonAliasId" );
        }
    }
}
