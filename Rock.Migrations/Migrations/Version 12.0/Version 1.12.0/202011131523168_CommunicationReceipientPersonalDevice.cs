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
    public partial class CommunicationReceipientPersonalDevice : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex("dbo.CommunicationRecipient", new[] { "PersonAliasId" });
            AddColumn("dbo.CommunicationRecipient", "PersonalDeviceId", c => c.Int());
            AlterColumn("dbo.CommunicationRecipient", "PersonAliasId", c => c.Int());
            CreateIndex("dbo.CommunicationRecipient", "PersonAliasId");
            CreateIndex("dbo.CommunicationRecipient", "PersonalDeviceId");
            AddForeignKey("dbo.CommunicationRecipient", "PersonalDeviceId", "dbo.PersonalDevice", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.CommunicationRecipient", "PersonalDeviceId", "dbo.PersonalDevice");
            DropIndex("dbo.CommunicationRecipient", new[] { "PersonalDeviceId" });
            DropIndex("dbo.CommunicationRecipient", new[] { "PersonAliasId" });
            AlterColumn("dbo.CommunicationRecipient", "PersonAliasId", c => c.Int(nullable: false));
            DropColumn("dbo.CommunicationRecipient", "PersonalDeviceId");
            CreateIndex("dbo.CommunicationRecipient", "PersonAliasId");
        }
    }
}
