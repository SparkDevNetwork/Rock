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
    public partial class DropInteractionTablesToReorderFields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.InteractionChannel", "ChannelMediumValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.InteractionChannel", "ComponentEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.InteractionChannel", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionChannel", "InteractionEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.InteractionChannel", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionComponent", "ChannelId", "dbo.InteractionChannel");
            DropForeignKey("dbo.InteractionComponent", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionComponent", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionDeviceType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionDeviceType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Interaction", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Interaction", "InteractionComponentId", "dbo.InteractionComponent");
            DropForeignKey("dbo.InteractionSession", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionSession", "DeviceTypeId", "dbo.InteractionDeviceType");
            DropForeignKey("dbo.InteractionSession", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Interaction", "InteractionSessionId", "dbo.InteractionSession");
            DropForeignKey("dbo.Interaction", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Interaction", "PersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.InteractionComponent", new[] { "ChannelId" });
            DropIndex("dbo.InteractionComponent", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionComponent", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionComponent", new[] { "Guid" });
            DropIndex("dbo.InteractionChannel", new[] { "ComponentEntityTypeId" });
            DropIndex("dbo.InteractionChannel", new[] { "InteractionEntityTypeId" });
            DropIndex("dbo.InteractionChannel", new[] { "ChannelMediumValueId" });
            DropIndex("dbo.InteractionChannel", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionChannel", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionChannel", new[] { "Guid" });
            DropIndex("dbo.InteractionDeviceType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionDeviceType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionDeviceType", new[] { "Guid" });
            DropIndex("dbo.Interaction", new[] { "InteractionComponentId" });
            DropIndex("dbo.Interaction", new[] { "PersonAliasId" });
            DropIndex("dbo.Interaction", new[] { "InteractionSessionId" });
            DropIndex("dbo.Interaction", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Interaction", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Interaction", new[] { "Guid" });
            DropIndex("dbo.InteractionSession", new[] { "DeviceTypeId" });
            DropIndex("dbo.InteractionSession", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionSession", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionSession", new[] { "Guid" });
            DropTable("dbo.InteractionComponent");
            DropTable("dbo.InteractionChannel");
            DropTable("dbo.InteractionDeviceType");
            DropTable("dbo.Interaction");
            DropTable("dbo.InteractionSession");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateTable(
                "dbo.InteractionSession",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InteractionMode = c.String(maxLength: 25),
                        SessionData = c.String(),
                        DeviceTypeId = c.Int(),
                        IpAddress = c.String(maxLength: 45),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Interaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InteractionDateTime = c.DateTime(nullable: false),
                        Operation = c.String(maxLength: 25),
                        InteractionComponentId = c.Int(nullable: false),
                        EntityId = c.Int(),
                        PersonAliasId = c.Int(),
                        InteractionSessionId = c.Int(),
                        InteractionSummary = c.String(maxLength: 200),
                        InteractionData = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InteractionDeviceType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        DeviceTypeData = c.String(),
                        ClientType = c.String(maxLength: 25),
                        OperatingSystem = c.String(maxLength: 100),
                        Application = c.String(maxLength: 100),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InteractionChannel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ChannelData = c.String(),
                        ComponentEntityTypeId = c.Int(),
                        InteractionEntityTypeId = c.Int(),
                        ChannelMediumValueId = c.Int(),
                        RetentionDuration = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InteractionComponent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ComponentData = c.String(),
                        EntityId = c.Int(),
                        ChannelId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.InteractionSession", "Guid", unique: true);
            CreateIndex("dbo.InteractionSession", "ModifiedByPersonAliasId");
            CreateIndex("dbo.InteractionSession", "CreatedByPersonAliasId");
            CreateIndex("dbo.InteractionSession", "DeviceTypeId");
            CreateIndex("dbo.Interaction", "Guid", unique: true);
            CreateIndex("dbo.Interaction", "ModifiedByPersonAliasId");
            CreateIndex("dbo.Interaction", "CreatedByPersonAliasId");
            CreateIndex("dbo.Interaction", "InteractionSessionId");
            CreateIndex("dbo.Interaction", "PersonAliasId");
            CreateIndex("dbo.Interaction", "InteractionComponentId");
            CreateIndex("dbo.InteractionDeviceType", "Guid", unique: true);
            CreateIndex("dbo.InteractionDeviceType", "ModifiedByPersonAliasId");
            CreateIndex("dbo.InteractionDeviceType", "CreatedByPersonAliasId");
            CreateIndex("dbo.InteractionChannel", "Guid", unique: true);
            CreateIndex("dbo.InteractionChannel", "ModifiedByPersonAliasId");
            CreateIndex("dbo.InteractionChannel", "CreatedByPersonAliasId");
            CreateIndex("dbo.InteractionChannel", "ChannelMediumValueId");
            CreateIndex("dbo.InteractionChannel", "InteractionEntityTypeId");
            CreateIndex("dbo.InteractionChannel", "ComponentEntityTypeId");
            CreateIndex("dbo.InteractionComponent", "Guid", unique: true);
            CreateIndex("dbo.InteractionComponent", "ModifiedByPersonAliasId");
            CreateIndex("dbo.InteractionComponent", "CreatedByPersonAliasId");
            CreateIndex("dbo.InteractionComponent", "ChannelId");
            AddForeignKey("dbo.Interaction", "PersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.Interaction", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.Interaction", "InteractionSessionId", "dbo.InteractionSession", "Id");
            AddForeignKey("dbo.InteractionSession", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.InteractionSession", "DeviceTypeId", "dbo.InteractionDeviceType", "Id");
            AddForeignKey("dbo.InteractionSession", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.Interaction", "InteractionComponentId", "dbo.InteractionComponent", "Id");
            AddForeignKey("dbo.Interaction", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.InteractionDeviceType", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.InteractionDeviceType", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.InteractionComponent", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.InteractionComponent", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.InteractionComponent", "ChannelId", "dbo.InteractionChannel", "Id");
            AddForeignKey("dbo.InteractionChannel", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.InteractionChannel", "InteractionEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.InteractionChannel", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.InteractionChannel", "ComponentEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.InteractionChannel", "ChannelMediumValueId", "dbo.DefinedValue", "Id");
        }
    }
}
