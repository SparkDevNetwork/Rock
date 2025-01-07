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
    public partial class RemoveProcessSendKeyFromCommunicationRecipient : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // These PhoneNumber changes were actually applied in the last EF migration, but the model snapshot is
            // apparently out of sync; this migration should bring the snapshot back in sync. I'm leaving the
            // following lines here (commented-out) for posterity.
            //AddColumn("dbo.PhoneNumber", "IsMessagingOptedOut", c => c.Boolean(nullable: false));
            //AddColumn("dbo.PhoneNumber", "MessagingOptedOutDateTime", c => c.DateTime());

            // Here is the actual db change this migration was added for:
            DropColumn( "dbo.CommunicationRecipient", "ProcessSendKey");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.CommunicationRecipient", "ProcessSendKey", c => c.String(maxLength: 300));

            //DropColumn("dbo.PhoneNumber", "MessagingOptedOutDateTime");
            //DropColumn("dbo.PhoneNumber", "IsMessagingOptedOut");
        }
    }
}
