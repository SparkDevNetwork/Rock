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
    public partial class NoteCreatedInfo : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE N SET 
        [CreatedDateTime] = N.[CreationDateTime], 
        [CreatedByPersonAliasId] = A.[Id]
    FROM [Note] N
    INNER JOIN [PersonAlias] A ON A.[AliasPersonId] = N.[CreatedByPersonId]
" ); 

            DropForeignKey( "dbo.Note", "CreatedByPersonId", "dbo.Person" );
            DropIndex("dbo.Note", new[] { "CreatedByPersonId" });
            DropColumn("dbo.Note", "CreatedByPersonId");
            DropColumn("dbo.Note", "CreationDateTime");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Note", "CreationDateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.Note", "CreatedByPersonId", c => c.Int());
            CreateIndex("dbo.Note", "CreatedByPersonId");
            AddForeignKey("dbo.Note", "CreatedByPersonId", "dbo.Person", "Id");

            Sql( @"
    UPDATE N SET 
        [CreationDateTime] = N.[CreatedDateTime], 
        [CreatedByPersonId] = A.[PersonId]
    FROM [Note] N
    INNER JOIN [PersonAlias] A ON A.[Id] = N.[CreatedByPersonAliasId]
" );
        }
    }
}
