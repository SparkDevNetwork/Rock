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
    public partial class NewPersonAndRecipientFields : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Person", "ReviewReasonValueId", c => c.Int());
            AddColumn("dbo.Person", "ReviewReasonNote", c => c.String(maxLength: 1000));
            AddColumn("dbo.CommunicationRecipient", "TransportEntityTypeName", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationRecipient", "UniqueMessageId", c => c.String(maxLength: 100));
            CreateIndex("dbo.Person", "ReviewReasonValueId");
            AddForeignKey("dbo.Person", "ReviewReasonValueId", "dbo.DefinedValue", "Id");

            AddDefinedType( "Person", "Review Reason", "Are potential reasons why a person's profile may need to be reviewed by a staff member", "7680C445-AD69-4E5D-94F0-CBAA96DB0FF8" );
            AddDefinedValue( "7680C445-AD69-4E5D-94F0-CBAA96DB0FF8", "Self - Inactivated", "This person inactivated their record from the Email Preferences block on the public website.", "D539C356-6856-4E94-80B4-8FEA869AF38B" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "D539C356-6856-4E94-80B4-8FEA869AF38B" );
            DeleteDefinedType( "7680C445-AD69-4E5D-94F0-CBAA96DB0FF8" );

            DropForeignKey("dbo.Person", "ReviewReasonValueId", "dbo.DefinedValue");
            DropIndex("dbo.Person", new[] { "ReviewReasonValueId" });
            DropColumn("dbo.CommunicationRecipient", "UniqueMessageId");
            DropColumn("dbo.CommunicationRecipient", "TransportEntityTypeName");
            DropColumn("dbo.Person", "ReviewReasonNote");
            DropColumn("dbo.Person", "ReviewReasonValueId");
        }
    }
}
