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
    public partial class AddIndexes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.PersonAlias", "PersonId", "dbo.Person");
            AddForeignKey("dbo.PersonAlias", "PersonId", "dbo.Person", "Id");

            CreateIndex( "dbo.Audit", new string[] { "EntityTypeId", "EntityId" } );
            CreateIndex( "dbo.Auth", new string[] { "EntityTypeId", "EntityId" } );
            CreateIndex( "dbo.Following", new string[] { "EntityTypeId", "EntityId" } );
            CreateIndex( "dbo.History", new string[] { "EntityTypeId", "EntityId" } );
            CreateIndex( "dbo.EntitySetItem", new string[] { "EntitySetId", "EntityId" } );
            CreateIndex( "dbo.Note", new string[] { "NoteTypeId", "EntityId" } );
            CreateIndex( "dbo.TaggedItem", new string[] { "TagId", "EntityGuid" } );
            CreateIndex( "dbo.PersonDuplicate", "DuplicatePersonAliasId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.Audit", new string[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.Auth", new string[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.Following", new string[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.History", new string[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.EntitySetItem", new string[] { "EntitySetId", "EntityId" } );
            DropIndex( "dbo.Note", new string[] { "NoteTypeId", "EntityId" } );
            DropIndex( "dbo.TaggedItem", new string[] { "TagId", "EntityGuid" } );
            DropIndex( "dbo.PersonDuplicate", "DuplicatePersonAliasId" );

            DropForeignKey( "dbo.PersonAlias", "PersonId", "dbo.Person" );
            AddForeignKey("dbo.PersonAlias", "PersonId", "dbo.Person", "Id", cascadeDelete: true);
        }
    }
}
