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
    public partial class AddRemoteAuthenticationSession : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.RemoteAuthenticationSession",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 20),
                        AuthorizedPersonAliasId = c.Int(),
                        SessionStartDateTime = c.DateTime(),
                        SessionAuthenticatedDateTime = c.DateTime(),
                        SessionEndDateTime = c.DateTime(),
                        ClientIpAddress = c.String(maxLength: 45),
                        AuthenticationIpAddress = c.String(maxLength: 45),
                        DeviceUniqueIdentifier = c.String(maxLength: 45),
                        SiteId = c.Int(),
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
                .ForeignKey("dbo.PersonAlias", t => t.AuthorizedPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Site", t => t.SiteId)
                .Index(t => t.AuthorizedPersonAliasId)
                .Index(t => t.SiteId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.RemoteAuthenticationSession", "SiteId", "dbo.Site");
            DropForeignKey("dbo.RemoteAuthenticationSession", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RemoteAuthenticationSession", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RemoteAuthenticationSession", "AuthorizedPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RemoteAuthenticationSession", new[] { "Guid" });
            DropIndex("dbo.RemoteAuthenticationSession", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RemoteAuthenticationSession", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RemoteAuthenticationSession", new[] { "SiteId" });
            DropIndex("dbo.RemoteAuthenticationSession", new[] { "AuthorizedPersonAliasId" });
            DropTable("dbo.RemoteAuthenticationSession");
        }
    }
}
