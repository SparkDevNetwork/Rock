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
    public partial class InteractionModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.InteractionComponent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.InteractionChannel", t => t.ChannelId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ChannelId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.InteractionChannel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DefinedValue", t => t.ChannelMediumValueId)
                .ForeignKey("dbo.EntityType", t => t.ComponentEntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.InteractionEntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ComponentEntityTypeId)
                .Index(t => t.InteractionEntityTypeId)
                .Index(t => t.ChannelMediumValueId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.InteractionDeviceType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.Interaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InteractionDateTime = c.DateTime(nullable: false),
                        Operation = c.String(maxLength: 25),
                        InteractionComponentId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.InteractionComponent", t => t.InteractionComponentId)
                .ForeignKey("dbo.InteractionSession", t => t.InteractionSessionId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.InteractionComponentId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.InteractionSessionId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.InteractionDeviceType", t => t.DeviceTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.DeviceTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.AddDefinedType( "Global", "Interaction Medium", "Mediums to classify channels with.", SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM, "Website", "Used for tracking page views for various websites.", SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM, "Communication", "Used for emails, SMS, etc.", SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_COMMUNICATION );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM, "Content Channel", "Used for tracking content channels.", SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Interaction", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Interaction", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Interaction", "InteractionSessionId", "dbo.InteractionSession");
            DropForeignKey("dbo.InteractionSession", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionSession", "DeviceTypeId", "dbo.InteractionDeviceType");
            DropForeignKey("dbo.InteractionSession", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Interaction", "InteractionComponentId", "dbo.InteractionComponent");
            DropForeignKey("dbo.Interaction", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionDeviceType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionDeviceType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionComponent", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionComponent", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionComponent", "ChannelId", "dbo.InteractionChannel");
            DropForeignKey("dbo.InteractionChannel", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionChannel", "InteractionEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.InteractionChannel", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractionChannel", "ComponentEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.InteractionChannel", "ChannelMediumValueId", "dbo.DefinedValue");
            DropIndex("dbo.InteractionSession", new[] { "Guid" });
            DropIndex("dbo.InteractionSession", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionSession", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionSession", new[] { "DeviceTypeId" });
            DropIndex("dbo.Interaction", new[] { "Guid" });
            DropIndex("dbo.Interaction", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Interaction", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Interaction", new[] { "InteractionSessionId" });
            DropIndex("dbo.Interaction", new[] { "PersonAliasId" });
            DropIndex("dbo.Interaction", new[] { "InteractionComponentId" });
            DropIndex("dbo.InteractionDeviceType", new[] { "Guid" });
            DropIndex("dbo.InteractionDeviceType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionDeviceType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionChannel", new[] { "Guid" });
            DropIndex("dbo.InteractionChannel", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionChannel", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionChannel", new[] { "ChannelMediumValueId" });
            DropIndex("dbo.InteractionChannel", new[] { "InteractionEntityTypeId" });
            DropIndex("dbo.InteractionChannel", new[] { "ComponentEntityTypeId" });
            DropIndex("dbo.InteractionComponent", new[] { "Guid" });
            DropIndex("dbo.InteractionComponent", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractionComponent", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractionComponent", new[] { "ChannelId" });
            DropTable("dbo.InteractionSession");
            DropTable("dbo.Interaction");
            DropTable("dbo.InteractionDeviceType");
            DropTable("dbo.InteractionChannel");
            DropTable("dbo.InteractionComponent");
        }
    }
}
