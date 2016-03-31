// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class TransactionStatus : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransaction", "Status", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialTransaction", "StatusMessage", c => c.String(maxLength: 200));

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Finance", "fa fa-money", "", "311DE99F-7A39-4C39-A716-3549AA72E81B", 0 ); // Finance

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 4, @"False", "2155A537-740A-4470-A620-8D39B1840A21" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "EB3D933D-D019-4049-A2AD-6BF541352623" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "B529A04B-D9C6-4D38-B9AA-BC78EED8DBFE" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template

            RockMigrationHelper.UpdateWorkflowType( false, true, "Payment Reversal Notification", "Workflow that Is initiated by the scheduled payment download process whenever a reversal payment is added due to a previous transaction that failed to be processed successfully.", "311DE99F-7A39-4C39-A716-3549AA72E81B", "Payment", "fa fa-list-ol", 0, true, 0, "4FCE5C18-C014-497E-93CF-7136488E1A08" ); // Payment Reversal Notification
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "4FCE5C18-C014-497E-93CF-7136488E1A08", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Finance Administrators", "FinanceAdministrators", "", 3, @"6246a7ef-b7a3-4c8c-b1e4-3ff114b84559", "49AC650E-347A-4F51-9F64-8D601E573C28" ); // Payment Reversal Notification:Finance Administrators
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "4FCE5C18-C014-497E-93CF-7136488E1A08", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "", 0, @"", "454FF91C-422E-4C30-A4A9-A0288C081B83" ); // Payment Reversal Notification:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "4FCE5C18-C014-497E-93CF-7136488E1A08", "3EE69CBC-35CE-4496-88CC-8327A447603F", "Amount", "Amount", "", 1, @"", "EBCFD0B5-55A7-4868-9059-B49063D619EE" ); // Payment Reversal Notification:Amount
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "4FCE5C18-C014-497E-93CF-7136488E1A08", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Transaction Id", "TransactionId", "", 2, @"", "5896EE69-B908-436F-B633-C5BF2B34989C" ); // Payment Reversal Notification:Transaction Id
            RockMigrationHelper.AddAttributeQualifier( "454FF91C-422E-4C30-A4A9-A0288C081B83", "EnableSelfSelection", @"False", "AEA9C8A6-A816-4684-A080-39120A60C03B" ); // Payment Reversal Notification:Person:EnableSelfSelection

            RockMigrationHelper.UpdateWorkflowActivityType( "4FCE5C18-C014-497E-93CF-7136488E1A08", true, "Send Notification", "Sends an email to finance team", true, 0, "ABC6868F-7FBD-48CE-B601-E1C342806798" ); // Payment Reversal Notification:Send Notification

            RockMigrationHelper.UpdateWorkflowActionType( "ABC6868F-7FBD-48CE-B601-E1C342806798", "Set Transaction Id", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "4A1C38EC-590E-4931-AC7C-B90D12B72B3F" ); // Payment Reversal Notification:Send Notification:Set Transaction Id
            RockMigrationHelper.AddActionTypeAttributeValue( "4A1C38EC-590E-4931-AC7C-B90D12B72B3F", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Payment Reversal Notification:Send Notification:Set Transaction Id:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4A1C38EC-590E-4931-AC7C-B90D12B72B3F", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Payment Reversal Notification:Send Notification:Set Transaction Id:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4A1C38EC-590E-4931-AC7C-B90D12B72B3F", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"5896ee69-b908-436f-b633-c5bf2b34989c" ); // Payment Reversal Notification:Send Notification:Set Transaction Id:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4A1C38EC-590E-4931-AC7C-B90D12B72B3F", "EB3D933D-D019-4049-A2AD-6BF541352623", @"True" ); // Payment Reversal Notification:Send Notification:Set Transaction Id:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "4A1C38EC-590E-4931-AC7C-B90D12B72B3F", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"True" ); // Payment Reversal Notification:Send Notification:Set Transaction Id:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "4A1C38EC-590E-4931-AC7C-B90D12B72B3F", "B529A04B-D9C6-4D38-B9AA-BC78EED8DBFE", @"" ); // Payment Reversal Notification:Send Notification:Set Transaction Id:Lava Template

            RockMigrationHelper.UpdateWorkflowActionType( "ABC6868F-7FBD-48CE-B601-E1C342806798", "Set Person", 1, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "B6918852-B77D-42DB-88B0-D3E299B1B767" ); // Payment Reversal Notification:Send Notification:Set Person
            RockMigrationHelper.AddActionTypeAttributeValue( "B6918852-B77D-42DB-88B0-D3E299B1B767", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Payment Reversal Notification:Send Notification:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B6918852-B77D-42DB-88B0-D3E299B1B767", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Payment Reversal Notification:Send Notification:Set Person:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "B6918852-B77D-42DB-88B0-D3E299B1B767", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"454ff91c-422e-4c30-a4a9-a0288c081b83" ); // Payment Reversal Notification:Send Notification:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B6918852-B77D-42DB-88B0-D3E299B1B767", "EB3D933D-D019-4049-A2AD-6BF541352623", @"True" ); // Payment Reversal Notification:Send Notification:Set Person:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "B6918852-B77D-42DB-88B0-D3E299B1B767", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Payment Reversal Notification:Send Notification:Set Person:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "B6918852-B77D-42DB-88B0-D3E299B1B767", "B529A04B-D9C6-4D38-B9AA-BC78EED8DBFE", @"{{ Entity.AuthorizedPersonAlias.Guid }}" ); // Payment Reversal Notification:Send Notification:Set Person:Lava Template

            RockMigrationHelper.UpdateWorkflowActionType( "ABC6868F-7FBD-48CE-B601-E1C342806798", "Set Amount", 2, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "5A184A2A-9EAB-4A7C-A3BA-29CDB36CC8AC" ); // Payment Reversal Notification:Send Notification:Set Amount
            RockMigrationHelper.AddActionTypeAttributeValue( "5A184A2A-9EAB-4A7C-A3BA-29CDB36CC8AC", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Payment Reversal Notification:Send Notification:Set Amount:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5A184A2A-9EAB-4A7C-A3BA-29CDB36CC8AC", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Payment Reversal Notification:Send Notification:Set Amount:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5A184A2A-9EAB-4A7C-A3BA-29CDB36CC8AC", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"ebcfd0b5-55a7-4868-9059-b49063d619ee" ); // Payment Reversal Notification:Send Notification:Set Amount:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5A184A2A-9EAB-4A7C-A3BA-29CDB36CC8AC", "EB3D933D-D019-4049-A2AD-6BF541352623", @"True" ); // Payment Reversal Notification:Send Notification:Set Amount:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "5A184A2A-9EAB-4A7C-A3BA-29CDB36CC8AC", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Payment Reversal Notification:Send Notification:Set Amount:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "5A184A2A-9EAB-4A7C-A3BA-29CDB36CC8AC", "B529A04B-D9C6-4D38-B9AA-BC78EED8DBFE", @"{{ Entity.TotalAmount }}" ); // Payment Reversal Notification:Send Notification:Set Amount:Lava Template

            RockMigrationHelper.UpdateWorkflowActionType( "ABC6868F-7FBD-48CE-B601-E1C342806798", "Send Email", 3, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "91055261-8F9A-4B06-B4A4-0FD1CF17C113" ); // Payment Reversal Notification:Send Notification:Send Email
            RockMigrationHelper.AddActionTypeAttributeValue( "91055261-8F9A-4B06-B4A4-0FD1CF17C113", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Payment Reversal Notification:Send Notification:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "91055261-8F9A-4B06-B4A4-0FD1CF17C113", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Payment Reversal Notification:Send Notification:Send Email:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "91055261-8F9A-4B06-B4A4-0FD1CF17C113", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Payment Reversal Notification:Send Notification:Send Email:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "91055261-8F9A-4B06-B4A4-0FD1CF17C113", "0C4C13B8-7076-4872-925A-F950886B5E16", @"49ac650e-347a-4f51-9f64-8d601e573c28" ); // Payment Reversal Notification:Send Notification:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "91055261-8F9A-4B06-B4A4-0FD1CF17C113", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Scheduled Payment Reversal" ); // Payment Reversal Notification:Send Notification:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "91055261-8F9A-4B06-B4A4-0FD1CF17C113", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailStyles }}
{{ 'Global' | Attribute:'EmailHeader' }}
<p>{{ Person.NickName }},</p>

{% assign txnPerson = Workflow | Attribute:'Person','Object' %}
<p>
A <a href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}transaction/{{ Workflow | Attribute:'TransactionId' }}"">reversal 
transaction</a> for <a href=""{{ 'Global' | Attribute:'InternalApplicationRoot' }}person/{{ txnPerson.Id }}"">{{ txnPerson.FullName }}</a> in 
the amount of {{ Workflow | Attribute:'Amount' | FormatAsCurrency }} was created during the last download of recurring transactions. 
This transaction was created to offset an earlier transaction that was created when the transaction had not yet finished processing.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}" ); // Payment Reversal Notification:Send Notification:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "91055261-8F9A-4B06-B4A4-0FD1CF17C113", "2155A537-740A-4470-A620-8D39B1840A21", @"False" ); // Payment Reversal Notification:Send Notification:Send Email:Save Communication History

            // Update the attributes for the download payments job
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.GetScheduledPayments", "Days Back", "The number of days prior to the current date to use as the start date when querying for scheduled payments that were processed.", 1, "7", "F05BE4DB-6375-4712-9B81-73A836EEF19F" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Class", "Rock.Jobs.GetScheduledPayments", "Batch Name Prefix", "The batch prefix name to use when creating a new batch", 2, "Online Giving", "948A4C22-0E66-495E-B7A8-AAE852170211" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.GetScheduledPayments", "Receipt Email", "The system email to use to send the receipts.", 3, "", "95B950BF-3452-4088-98C4-48888C6ADBEF" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Class", "Rock.Jobs.GetScheduledPayments", "Reversal Workflow", "The workflow to run when a failed (reversal) transaction is downloaded.", 4, "", "9B8B9CD9-263C-4DB2-A022-E62A295697FC" );
            RockMigrationHelper.UpdateAttributeQualifier( "948A4C22-0E66-495E-B7A8-AAE852170211", "ispassword", "False", "2613DC3E-E716-43EC-861E-F73872910E53" );

            // Create the payment download job if it hasn't been created yet (as inactive)
            Sql( @"
    DECLARE @AttributeId int
    DECLARE @JobId int = ( SELECT TOP 1 [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.GetScheduledPayments' )
    IF @JobId IS NULL
    BEGIN

	    -- Suggestion Notification Job
	    INSERT INTO [ServiceJob] ( [IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [Guid], [NotificationStatus] )
	    VALUES ( 0, 0, 'Download Payments', 'Downloads any payments that have been processed for the active scheduled transactions.',
		    'Rock.Jobs.GetScheduledPayments','0 0 5 ? * MON-FRI *','43044F38-F357-4CF4-995D-C60D4724C97E', 3 )
	    SET @JobId = SCOPE_IDENTITY()

	    -- Days Back attribute
	    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'F05BE4DB-6375-4712-9B81-73A836EEF19F' )
	    IF @AttributeId IS NOT NULL
	    BEGIN
		    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		    VALUES ( 0, @AttributeId, @JobId, '7', NEWID() )
	    END

	    -- Batch Name Prefix attribute
	    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '948A4C22-0E66-495E-B7A8-AAE852170211' )
	    IF @AttributeId IS NOT NULL
	    BEGIN
		    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		    VALUES ( 0, @AttributeId, @JobId, 'Online Giving', NEWID() )
	    END

	    -- Receipt Email
	    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '95B950BF-3452-4088-98C4-48888C6ADBEF' )
	    IF @AttributeId IS NOT NULL
	    BEGIN
		    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		    VALUES ( 0, @AttributeId, @JobId, '7dbf229e-7dee-a684-4929-6c37312a0039', NEWID() )
	    END

    END

    -- Reversal Workflow attribute
    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '9B8B9CD9-263C-4DB2-A022-E62A295697FC' )
    IF @AttributeId IS NOT NULL
    BEGIN
	    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	    VALUES ( 0, @AttributeId, @JobId, '4fce5c18-c014-497e-93cf-7136488e1a08', NEWID() )
    END
" );
            // Add a route to transaction detail page
            RockMigrationHelper.AddPageRoute( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "Transaction/{transactionId}", "C1F02FA3-DD60-4D43-95AE-A4F4B3A63990" );

            // Attrib for BlockType: Scheduled Payment Download:Receipt Email
            RockMigrationHelper.AddBlockTypeAttribute( "71FF09C3-3E50-4E97-9329-3CD57AACCA53", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Receipt Email", "ReceiptEmail", "", "The system email to use to send the receipts.", 2, @"", "E67E6D3E-6EB1-433B-9244-585BB4AB8414" );
            // Attrib for BlockType: Scheduled Payment Download:Reversal Workflow
            RockMigrationHelper.AddBlockTypeAttribute( "71FF09C3-3E50-4E97-9329-3CD57AACCA53", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Reversal Workflow", "ReversalWorkflow", "", "The workflow to run when a failed (reversal) transaction is downloaded.", 3, @"", "2A429277-AE75-480D-B9C9-8064B59DE8E9" );
            
            // Attrib Value for Block:Scheduled Payment Download, Attribute:Receipt Email Page: Download Payments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A55A9614-9D89-4D56-A022-D15BD6472C62", "E67E6D3E-6EB1-433B-9244-585BB4AB8414", @"7dbf229e-7dee-a684-4929-6c37312a0039" );
            // Attrib Value for Block:Scheduled Payment Download, Attribute:Reversal Workflow Page: Download Payments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A55A9614-9D89-4D56-A022-D15BD6472C62", "2A429277-AE75-480D-B9C9-8064B59DE8E9", @"4fce5c18-c014-497e-93cf-7136488e1a08" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.FinancialTransaction", "StatusMessage");
            DropColumn("dbo.FinancialTransaction", "Status");
        }
    }
}
