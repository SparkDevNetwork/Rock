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
    public partial class RegistrationSession : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.RegistrationSession",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegistrationInstanceId = c.Int(),
                        RegistrationCount = c.Int(nullable: false),
                        SessionStartDateTime = c.DateTime(nullable: false),
                        ExpirationDateTime = c.DateTime(nullable: false),
                        ClientIpAddress = c.String(maxLength: 45),
                        RegistrationData = c.String(),
                        PaymentGatewayReference = c.String(maxLength: 16),
                        SessionStatus = c.Int(nullable: false),
                        RegistrationId = c.Int(),
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
                .ForeignKey("dbo.Registration", t => t.RegistrationId)
                .ForeignKey("dbo.RegistrationInstance", t => t.RegistrationInstanceId)
                .Index(t => t.RegistrationInstanceId)
                .Index(t => t.RegistrationId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.RegistrationInstance", "ExternalGatewayMerchantId", c => c.Int());
            AddColumn("dbo.RegistrationInstance", "ExternalGatewayFundId", c => c.Int());
            AddColumn("dbo.RegistrationInstance", "RegistrationMeteringThreshold", c => c.Int());
            AddColumn("dbo.RegistrationTemplate", "IsRegistrationMeteringEnabled", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.RegistrationSession", "RegistrationInstanceId", "dbo.RegistrationInstance");
            DropForeignKey("dbo.RegistrationSession", "RegistrationId", "dbo.Registration");
            DropForeignKey("dbo.RegistrationSession", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationSession", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RegistrationSession", new[] { "Guid" });
            DropIndex("dbo.RegistrationSession", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationSession", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationSession", new[] { "RegistrationId" });
            DropIndex("dbo.RegistrationSession", new[] { "RegistrationInstanceId" });
            DropColumn("dbo.RegistrationTemplate", "IsRegistrationMeteringEnabled");
            DropColumn("dbo.RegistrationInstance", "RegistrationMeteringThreshold");
            DropColumn("dbo.RegistrationInstance", "ExternalGatewayFundId");
            DropColumn("dbo.RegistrationInstance", "ExternalGatewayMerchantId");
            DropTable("dbo.RegistrationSession");
        }
    }
}
