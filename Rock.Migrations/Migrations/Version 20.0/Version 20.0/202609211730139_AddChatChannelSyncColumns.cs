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
    public partial class AddChatChannelSyncColumns : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Group", "CanViewMembersOverride", c => c.Boolean());
            AddColumn("dbo.Group", "IsChatSearchIndexedOverride", c => c.Boolean());
            AddColumn("dbo.Group", "ChatChannelFirstEnabledDateTime", c => c.DateTime());
            // Defaulted true so that adding the setting does not quietly hide the roster on every
            // group type that already exists. False is the restrictive value here.
            AddColumn("dbo.GroupType", "CanViewMembers", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.GroupType", "IsChatSearchIndexed", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupMember", "ChatBannedUntil", c => c.DateTime());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupMember", "ChatBannedUntil");
            DropColumn("dbo.GroupType", "IsChatSearchIndexed");
            DropColumn("dbo.GroupType", "CanViewMembers");
            DropColumn("dbo.Group", "ChatChannelFirstEnabledDateTime");
            DropColumn("dbo.Group", "IsChatSearchIndexedOverride");
            DropColumn("dbo.Group", "CanViewMembersOverride");
        }
    }
}
