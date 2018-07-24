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
    using System.IO;
    using System.Linq;
    using Rock.Data;
    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_0209 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateLabels();
            BusinessTransactionDetailPages();
            AddJobForProcessingAdultChildren();
            NewBlockTypeAttributes();
            AccountTopLevelOrderingPage();
            Update_ufnUtility_CsvToTable_to_return_int();
            FixShortLinkInteractions();
            FixForSlowv7PersonDuplicateFinder();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // This is a one way trip.
        }

        private void UpdateLabels()
        {
            // Update all BinaryFiles that are Labels and remove the string "^JUS" if it exists
            Sql( @"
UPDATE BinaryFileData
SET Content = convert(VARBINARY(max), replace(Convert(VARCHAR(max), content), '^JUS', ''))
WHERE Id IN (
		SELECT Id
		FROM BinaryFile
		WHERE BinaryFileTypeId = 1
		)
	AND Convert(VARCHAR(max), content) LIKE '%^JUS%'
" );
        }

        private void BusinessTransactionDetailPages()
        {
            // Attrib Value for Block:Transaction List, Attribute:Accounts Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "95621093-9BBF-4467-9828-4456D5E01E1D", string.Empty );
            
            // Attrib Value for Block:Transaction List, Attribute:Transaction Types Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "293F8A3E-020A-4260-8817-3E368CF31ABB", @"2d607262-52d6-4724-910d-5c6e8fb89acc" );
            
            // Attrib Value for Block:Transaction List, Attribute:Batch Page Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "683C4694-4FAC-4AFC-8987-F062EE491BC3", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
            
            // Attrib Value for Block:Transaction List, Attribute:Default Transaction View Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "8D067930-6355-4DC7-98E1-3619C871AC83", @"Transactions" );
            
            // Attrib Value for Block:Transaction List, Attribute:Show Account Summary Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "4C92974B-FB99-4E89-B215-A457646D77E1", @"False" );
            
            // Attrib Value for Block:Transaction Detail, Attribute:Carry Over Account Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "52431D4B-EA02-4D70-8B3F-74E1AD8EB3D8", @"True" );
            
            // Attrib Value for Block:Transaction Detail, Attribute:Refund Batch Name Suffix Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "2FC63A9E-5A5B-46E9-9B49-306571526440", @"- Refund" );
            
            // Attrib Value for Block:Transaction Detail, Attribute:Registration Detail Page Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "59461EEC-5E7D-42E5-BC0C-29CC8950AFAC", @"fc81099a-2f98-4eba-ac5a-8300b2fe46c4" );
            
            // Attrib Value for Block:Transaction Detail, Attribute:Scheduled Transaction Detail Page Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "1990EBAE-BA67-48B0-95C2-B9EAB24E7ED9", @"f1c3bbd3-ee91-4ddd-8880-1542ebcd8041" );
            
            // Attrib Value for Block:Transaction Detail, Attribute:Batch Detail Page Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "4B9CA04E-ED2A-45CC-9B62-D2D0A46EF7E7", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
        }

        private void AddJobForProcessingAdultChildren()
        {
            Sql( @" INSERT INTO [dbo].[ServiceJob] (
                 [IsSystem]
                ,[IsActive]
                ,[Name]
                ,[Description]
                ,[Class]
                ,[CronExpression]
                ,[NotificationStatus]
                ,[Guid]
            )
            VALUES (
                 0 
                ,0 
                ,'Process Adult Children'
                ,'Job that will move any children in a family who are now considered an ""adult"" based on their age (i.e. they are now 18 years old).'
                , 'Rock.Jobs.DataAutomation.AdultChildren'
                , '0 0 18 1/1 * ? *'
                , 3
                , '18214883-0394-4C99-99E1-729D77B07FE4' )" );
        }

        private void NewBlockTypeAttributes()
        {
            // Attrib for BlockType: Scheduled Transaction List:Person Token Usage Limit
            RockMigrationHelper.UpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", string.Empty, @"When adding a new scheduled transaction from a person detail page, the maximum number of times the person token for the transaction can be used.", 4, @"1", "100B5AF8-38D2-449C-A8FC-46E2215BE80B" );
            
            // Attrib for BlockType: Scheduled Transaction List:Person Token Expire Minutes
            RockMigrationHelper.UpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", string.Empty, @"When adding a new scheduled transaction from a person detail page, the number of minutes the person token for the transaction is valid after it is issued.", 3, @"60", "CE565458-F502-4BD6-B886-570787A36EEF" );
        }

        private void AccountTopLevelOrderingPage()
        {
            RockMigrationHelper.AddPage( true, "2B630A3B-E081-4204-A3E4-17BB3A5F063D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Order Top-Level Accounts", string.Empty, "AD1ED5A5-2E43-433F-B1C3-E6052213EF71", string.Empty ); // Site:Rock RMS
            
            // Add Block to Page: Order Top-Level Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "AD1ED5A5-2E43-433F-B1C3-E6052213EF71", string.Empty, "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "Top-Level Account List", "Main", string.Empty, string.Empty, 0, "EB047AC6-40BB-4215-AACC-5ECE8FE73A9B" );
            
            // Attrib for BlockType: Scheduled Transaction List:Person Token Usage Limit
            RockMigrationHelper.UpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", string.Empty, @"When adding a new scheduled transaction from a person detail page, the maximum number of times the person token for the transaction can be used.", 4, @"1", "A6B71434-FD9B-45EC-AA50-07AE5D6BA384" );
            
            // Attrib for BlockType: Scheduled Transaction List:Person Token Expire Minutes
            RockMigrationHelper.UpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", string.Empty, @"When adding a new scheduled transaction from a person detail page, the number of minutes the person token for the transaction is valid after it is issued.", 3, @"60", "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17" );
            
            // Attrib for BlockType: Account Tree View:Order Top-Level Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Order Top-Level Page", "OrderTopLevelPage", string.Empty, string.Empty, 5, string.Empty, "F3E582D6-409E-4CDA-80FA-8E46B8CD95AF" );
            
            // Attrib Value for Block:Scheduled Contributions, Attribute:Person Token Usage Limit Page: Scheduled Transactions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "A6B71434-FD9B-45EC-AA50-07AE5D6BA384", @"1" );
            
            // Attrib Value for Block:Scheduled Contributions, Attribute:Person Token Expire Minutes Page: Scheduled Transactions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17", @"60" );
            
            // Attrib Value for Block:Finance - Giving Profile List, Attribute:Person Token Usage Limit Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0", "A6B71434-FD9B-45EC-AA50-07AE5D6BA384", @"1" );
            
            // Attrib Value for Block:Finance - Giving Profile List, Attribute:Person Token Expire Minutes Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0", "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17", @"60" );
            
            // Attrib Value for Block:Account Tree View, Attribute:Order Top-Level Page Page: Accounts, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7F52E6B6-B4ED-4ECE-BE19-98BD3B939965", "F3E582D6-409E-4CDA-80FA-8E46B8CD95AF", @"ad1ed5a5-2e43-433f-b1c3-e6052213ef71" );
        }

        private void Update_ufnUtility_CsvToTable_to_return_int()
        {
            Sql( MigrationSQL._201802092340176_Rollup_0209_ufnUtility_CsvToTable );
        }

        private void FixShortLinkInteractions()
        {
            Sql( @"
                DECLARE @OldEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SiteUrlMap' )
                DECLARE @NewEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PageShortLink' )

                IF @OldEntityTypeId IS NOT NULL AND @NewEntityTypeId IS NOT NULL
                BEGIN
	                UPDATE [InteractionChannel]
	                SET [ComponentEntityTypeId] = @NewEntityTypeId
	                WHERE [ComponentEntityTypeId] = @OldEntityTypeId

	                DELETE [EntityType]
	                WHERE [Id] = @OldEntityTypeId
                END" );
        }

        private void FixForSlowv7PersonDuplicateFinder()
        {
            Sql(MigrationSQL._201802092340176_Rollup_0209_spCrm_PersonDuplicateFinder);
        }
    }
}
