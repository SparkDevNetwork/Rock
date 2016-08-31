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
    public partial class TransactionStatus : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransaction", "Status", c => c.String(maxLength: 50));
            AddColumn("dbo.FinancialTransaction", "StatusMessage", c => c.String(maxLength: 200));
                        
            // Update the attributes for the download payments job
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.GetScheduledPayments", "Days Back", "The number of days prior to the current date to use as the start date when querying for scheduled payments that were processed.", 1, "7", "F05BE4DB-6375-4712-9B81-73A836EEF19F" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Class", "Rock.Jobs.GetScheduledPayments", "Batch Name Prefix", "The batch prefix name to use when creating a new batch", 2, "Online Giving", "948A4C22-0E66-495E-B7A8-AAE852170211" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.GetScheduledPayments", "Receipt Email", "The system email to use to send the receipts.", 3, "", "95B950BF-3452-4088-98C4-48888C6ADBEF" );
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

" );
            // Add a route to transaction detail page
            RockMigrationHelper.AddPageRoute( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "Transaction/{transactionId}", "C1F02FA3-DD60-4D43-95AE-A4F4B3A63990" );

            // Attrib for BlockType: Scheduled Payment Download:Receipt Email
            RockMigrationHelper.AddBlockTypeAttribute( "71FF09C3-3E50-4E97-9329-3CD57AACCA53", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Receipt Email", "ReceiptEmail", "", "The system email to use to send the receipts.", 2, @"", "E67E6D3E-6EB1-433B-9244-585BB4AB8414" );
            
            // Attrib Value for Block:Scheduled Payment Download, Attribute:Receipt Email Page: Download Payments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A55A9614-9D89-4D56-A022-D15BD6472C62", "E67E6D3E-6EB1-433B-9244-585BB4AB8414", @"7dbf229e-7dee-a684-4929-6c37312a0039" );
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
