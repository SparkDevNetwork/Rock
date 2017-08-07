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
    public partial class Presence : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.PersonalDevice", "PersonalDeviceTypeId", "dbo.DefinedValue");
            DropIndex("dbo.PersonalDevice", new[] { "PersonAliasId" });
            DropIndex("dbo.PersonalDevice", new[] { "PersonalDeviceTypeId" });
            AddColumn("dbo.Interaction", "PersonalDeviceId", c => c.Int());
            AddColumn("dbo.PersonalDevice", "PersonalDeviceTypeValueId", c => c.Int());
            AddColumn("dbo.PersonalDevice", "PlatformValueId", c => c.Int());
            AddColumn("dbo.PersonalDevice", "DeviceUniqueIdentifier", c => c.String(maxLength: 20));
            AddColumn("dbo.PersonalDevice", "DeviceVersion", c => c.String(maxLength: 100));
            AddColumn("dbo.PersonalDevice", "MACAddress", c => c.String(maxLength: 12));
            AlterColumn("dbo.PersonalDevice", "PersonAliasId", c => c.Int());
            CreateIndex("dbo.Interaction", "PersonalDeviceId");
            CreateIndex("dbo.PersonalDevice", "PersonAliasId");
            CreateIndex("dbo.PersonalDevice", "PersonalDeviceTypeValueId");
            AddForeignKey("dbo.Interaction", "PersonalDeviceId", "dbo.PersonalDevice", "Id");
            AddForeignKey("dbo.PersonalDevice", "PersonalDeviceTypeValueId", "dbo.DefinedValue", "Id");
            RockMigrationHelper.AddDefinedType( "Global", "Mobile Device Platform", "The platform (iOS, Android, etc.) of a mobile device.", SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM, "iOS", "An Apple Device", "F00515E7-4EF3-480D-A45D-372CE3D80E69", false );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM, "Android", "An Android Device", "63464BB8-83E2-4914-B922-5075311758F9", false );
            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM, "Other", "A Device other than Apple or Android", SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_OTHER );

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM, "WiFi Presence", "Used for tracking a device's presence on a WiFi zone", Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WIFI_PRESENCE );

            Sql( @"
    DECLARE @MediumValueId INT = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '338CB800-C556-46CD-849D-8AE58FC7CB0E' )
    INSERT INTO [InteractionChannel] ( [Name], [ChannelTypeMediumValueId], [Guid] )
        VALUES ( 'WiFi Presence', @MediumValueId, '23888303-4847-4C80-93E2-4C5EB8029D18' )
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DELETE [InteractionChannel] WHERE [Guid] = '23888303-4847-4C80-93E2-4C5EB8029D18'
" );
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM );
            RockMigrationHelper.DeleteDefinedValue( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WIFI_PRESENCE );

            DropForeignKey( "dbo.PersonalDevice", "PersonalDeviceTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.Interaction", "PersonalDeviceId", "dbo.PersonalDevice");
            DropIndex("dbo.PersonalDevice", new[] { "PersonalDeviceTypeValueId" });
            DropIndex("dbo.PersonalDevice", new[] { "PersonAliasId" });
            DropIndex("dbo.Interaction", new[] { "PersonalDeviceId" });
            AlterColumn("dbo.PersonalDevice", "PersonAliasId", c => c.Int(nullable: false));
            DropColumn("dbo.PersonalDevice", "MACAddress");
            DropColumn("dbo.PersonalDevice", "DeviceVersion");
            DropColumn("dbo.PersonalDevice", "DeviceUniqueIdentifier");
            DropColumn("dbo.PersonalDevice", "PlatformValueId");
            DropColumn("dbo.PersonalDevice", "PersonalDeviceTypeValueId");
            DropColumn("dbo.Interaction", "PersonalDeviceId");
            CreateIndex("dbo.PersonalDevice", "PersonalDeviceTypeId");
            CreateIndex("dbo.PersonalDevice", "PersonAliasId");
            AddForeignKey("dbo.PersonalDevice", "PersonalDeviceTypeId", "dbo.DefinedValue", "Id");
        }
    }
}
