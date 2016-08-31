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
    public partial class ExceptionCreatedInfo : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE E SET 
        [CreatedDateTime] = E.[ExceptionDateTime], 
        [CreatedByPersonAliasId] = A.[Id]
    FROM [ExceptionLog] E
    INNER JOIN [PersonAlias] A ON A.[AliasPersonId] = E.[CreatedByPersonId]
" );
            DropForeignKey( "dbo.ExceptionLog", "CreatedByPersonId", "dbo.Person" );
            DropIndex("dbo.ExceptionLog", new[] { "CreatedByPersonId" });
            DropColumn("dbo.ExceptionLog", "ExceptionDateTime");
            DropColumn("dbo.ExceptionLog", "CreatedByPersonId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.ExceptionLog", "CreatedByPersonId", c => c.Int());
            AddColumn("dbo.ExceptionLog", "ExceptionDateTime", c => c.DateTime(nullable: false));
            CreateIndex("dbo.ExceptionLog", "CreatedByPersonId");
            AddForeignKey("dbo.ExceptionLog", "CreatedByPersonId", "dbo.Person", "Id", cascadeDelete: true);

            Sql( @"
    UPDATE E SET 
        [ExceptionDateTime] = E.[CreatedDateTime], 
        [CreatedByPersonId] = A.[PersonId]
    FROM [ExceptionLog] E
    INNER JOIN [PersonAlias] A ON A.[Id] = E.[CreatedByPersonAliasId]
" );
        }
    }
}
