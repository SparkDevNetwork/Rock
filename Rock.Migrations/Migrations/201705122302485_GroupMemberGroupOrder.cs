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
    public partial class GroupMemberGroupOrder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Schedule", "IsActive", c => c.Boolean(nullable: false));
            Sql( "UPDATE [Schedule] set [IsActive] = 1 where [IsActive] != 1" );

            AddColumn("dbo.GroupMember", "GroupOrder", c => c.Int());
            AddColumn("dbo.Metric", "SourceLava", c => c.String());

            RockMigrationHelper.UpdateDefinedValue( "D6F323FF-6EF2-4DA7-A82C-61399AC1D798", "Lava", "The Metric Values are populated from custom Lava", "2868A3E8-4632-4966-84CD-EDB8B775D66C" );

            //// Migration Rollups

            // MP: CurrencyType Unknown
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Unknown", "The currency type is unknown. For example, it might have been imported from a system that doesn't indicate currency type.", "56C9AE9C-B5EB-46D5-9650-2EF86B14F856", false );

            // DT: Add Failed Payment Email
            RockMigrationHelper.UpdateSystemEmail( "Finance", "Failed Payment", "", "", "", "", "", "Unsuccessful Gift to {{ 'Global' | Attribute:'OrganizationName' }}", @"
{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    {{ Transaction.AuthorizedPersonAlias.Person.NickName }}, 
</p>
<p>
    We just wanted to make you aware that your gift to {{ 'Global' | Attribute:'OrganizationName' }} that was scheduled for {{ Transaction.TransactionDateTime | Date:'M/d/yyyy' }} in the amount of 
    {{ Transaction.ScheduledTransaction.TotalAmount | FormatAsCurrency }} did not process successfully. If you'd like, you can update your giving profile at 
    <a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}Give"">{{ 'Global' | Attribute:'PublicApplicationRoot' }}Give</a>.
</p>

<p>
    Below are the details of your transaction that we were unable to process.
</p>

<p>
<strong>Txn Code:</strong> {{ Transaction.TransactionCode }}<br/>
<strong>Status:</strong> {{ Transaction.Status }}<br/>
<strong>Status Message:</strong> {{ Transaction.StatusMessage }}
</p>

{{ 'Global' | Attribute:'EmailFooter' }}
", "449232B5-9C6B-480E-A881-E317D0BC307E" );

            // Attrib for BlockType: Scheduled Payment Download:Failed Payment Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "71FF09C3-3E50-4E97-9329-3CD57AACCA53", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Failed Payment Email", "FailedPaymentEmail", "", "The system email to use to send a notice about a scheduled payment that failed.", 3, @"", "B3813A3A-9303-4294-B05E-F79CFBF03514" );
            // Attrib for BlockType: Scheduled Payment Download:Failed Payment Workflow
            RockMigrationHelper.UpdateBlockTypeAttribute( "71FF09C3-3E50-4E97-9329-3CD57AACCA53", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Failed Payment Workflow", "FailedPaymentWorkflow", "", "An optional workflow to start whenever a scheduled payment has failed.", 4, @"", "047A8871-0915-43E0-9BED-623DEAE09C6C" );

            // Attrib Value for Block:Scheduled Payment Download, Attribute:Failed Payment Email Page: Download Payments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A55A9614-9D89-4D56-A022-D15BD6472C62", "B3813A3A-9303-4294-B05E-F79CFBF03514", @"449232b5-9c6b-480e-a881-e317d0bc307e" );
            // Attrib Value for Block:Scheduled Payment Download, Attribute:Failed Payment Workflow Page: Download Payments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A55A9614-9D89-4D56-A022-D15BD6472C62", "047A8871-0915-43E0-9BED-623DEAE09C6C", @"" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.GetScheduledPayments", "Failed Payment Email", "The system email to use to send a notice about a scheduled payment that failed.", 4, "", "FA09F128-F54D-4761-826C-82920A7605D1" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.WORKFLOW_TYPE, "Class", "Rock.Jobs.GetScheduledPayments", "Failed Payment Workflow", "An optional workflow to start whenever a scheduled payment has failed.", 5, "", "29652958-0529-478E-A112-C118310A6101" );

            Sql( @"
	-- Failed Payment Email
    DECLARE @JobId int = ( SELECT TOP 1 [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.GetScheduledPayments' )
	DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'FA09F128-F54D-4761-826C-82920A7605D1' )
	IF @JobId IS NOT NULL AND @AttributeId IS NOT NULL
	BEGIN
        DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @JobId
		INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		VALUES ( 0, @AttributeId, @JobId, '449232b5-9c6b-480e-a881-e317d0bc307e', NEWID() )
	END
" );
            // NA - Typo Fix
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.BinaryFile", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "BinaryFileTypeId", "1", "Print For Each", "When a family checks in, should this label be printed once per family, person, or location. Note: this only apples if check-in is configured to use Family check-in vs Individual check-in.", 1, "1", "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "core_LabelType" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Metric", "SourceLava");
            DropColumn("dbo.GroupMember", "GroupOrder");
            DropColumn("dbo.Schedule", "IsActive");
        }
    }
}
