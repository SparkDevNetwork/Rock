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
    public partial class UpdateCommunicationFlowModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowCommunicationId", "dbo.CommunicationFlowCommunication");
            DropForeignKey("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceId", "dbo.CommunicationFlowInstance");
            DropIndex("dbo.CommunicationFlowInstanceConversionHistory", new[] { "CommunicationFlowInstanceId" });
            DropIndex("dbo.CommunicationFlowInstanceConversionHistory", new[] { "CommunicationFlowCommunicationId" });            
            RenameColumn( "dbo.CommunicationFlowInstance", "StartDate", "StartDateTime" );
            AddColumn( "dbo.CommunicationFlowInstance", "CompletedDateTime", c => c.DateTime() );
            AddColumn( "dbo.CommunicationFlowInstance", "LastProcessedDateTime", c => c.DateTime() );
            AddColumn("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceCommunicationId", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationRecipientId", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationFlowInstanceRecipient", "InactiveReason", c => c.Int());
            CreateIndex("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceCommunicationId");
            CreateIndex("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationRecipientId");
            AddForeignKey("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceCommunicationId", "dbo.CommunicationFlowInstanceCommunication", "Id");
            AddForeignKey("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationRecipientId", "dbo.CommunicationRecipient", "Id");
            DropColumn("dbo.CommunicationFlowInstance", "StartDate");
            DropColumn("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceId");
            DropColumn("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowCommunicationId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowCommunicationId", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceId", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationFlowInstance", "StartDate", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationRecipientId", "dbo.CommunicationRecipient");
            DropForeignKey("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceCommunicationId", "dbo.CommunicationFlowInstanceCommunication");
            DropIndex("dbo.CommunicationFlowInstanceConversionHistory", new[] { "CommunicationRecipientId" });
            DropIndex("dbo.CommunicationFlowInstanceConversionHistory", new[] { "CommunicationFlowInstanceCommunicationId" });
            DropColumn("dbo.CommunicationFlowInstanceRecipient", "InactiveReason");
            DropColumn("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationRecipientId");
            DropColumn("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceCommunicationId");            
            DropColumn( "dbo.CommunicationFlowInstance", "LastProcessedDateTime" );
            DropColumn( "dbo.CommunicationFlowInstance", "CompletedDateTime" );
            RenameColumn( "dbo.CommunicationFlowInstance", "StartDateTime", "StartDate" );
            CreateIndex("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowCommunicationId");
            CreateIndex("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceId");
            AddForeignKey("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceId", "dbo.CommunicationFlowInstance", "Id");
            AddForeignKey("dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowCommunicationId", "dbo.CommunicationFlowCommunication", "Id");
        }
    }
}
