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
    public partial class AddPersonSearchKeys : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonSearchKey",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        SearchTypeValueId = c.Int(nullable: false),
                        SearchValue = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .ForeignKey("dbo.DefinedValue", t => t.SearchTypeValueId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.SearchTypeValueId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.AddDefinedType( "Person", "Person Search Keys", "List of Person Keys to search.", Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS, "Email", "", Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonSearchKey", "SearchTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.PersonSearchKey", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonSearchKey", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonSearchKey", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.PersonSearchKey", new[] { "Guid" });
            DropIndex("dbo.PersonSearchKey", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonSearchKey", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.PersonSearchKey", new[] { "SearchTypeValueId" });
            DropIndex("dbo.PersonSearchKey", new[] { "PersonAliasId" });
            DropTable("dbo.PersonSearchKey");

            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL );
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.PERSON_SEARCH_KEYS );

        }
    }
}
