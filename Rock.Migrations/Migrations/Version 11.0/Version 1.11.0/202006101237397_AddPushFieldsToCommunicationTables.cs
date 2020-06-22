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
    /// Adds the push fields to the communication tables.
    /// </summary>
    public partial class AddPushFieldsToCommunicationTables : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.SystemCommunication", "PushImageBinaryFileId", c => c.Int());
            AddColumn("dbo.SystemCommunication", "PushOpenAction", c => c.Int());
            AddColumn("dbo.SystemCommunication", "PushOpenMessage", c => c.String());
            AddColumn("dbo.SystemCommunication", "PushData", c => c.String());

            AddColumn("dbo.Communication", "PushImageBinaryFileId", c => c.Int());
            AddColumn("dbo.Communication", "PushOpenAction", c => c.Int());
            AddColumn("dbo.Communication", "PushOpenMessage", c => c.String());
            AddColumn("dbo.Communication", "PushData", c => c.String());

            AddColumn("dbo.CommunicationTemplate", "PushImageBinaryFileId", c => c.Int());
            AddColumn("dbo.CommunicationTemplate", "PushOpenAction", c => c.Int());
            AddColumn("dbo.CommunicationTemplate", "PushOpenMessage", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "PushData", c => c.String());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.CommunicationTemplate", "PushData");
            DropColumn("dbo.CommunicationTemplate", "PushOpenMessage");
            DropColumn("dbo.CommunicationTemplate", "PushOpenAction");
            DropColumn("dbo.CommunicationTemplate", "PushImageBinaryFileId");

            DropColumn("dbo.Communication", "PushData");
            DropColumn("dbo.Communication", "PushOpenMessage");
            DropColumn("dbo.Communication", "PushOpenAction");
            DropColumn("dbo.Communication", "PushImageBinaryFileId");

            DropColumn("dbo.SystemCommunication", "PushData");
            DropColumn("dbo.SystemCommunication", "PushOpenMessage");
            DropColumn("dbo.SystemCommunication", "PushOpenAction");
            DropColumn("dbo.SystemCommunication", "PushImageBinaryFileId");
        }
    }
}
