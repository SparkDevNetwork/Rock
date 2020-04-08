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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 43, "1.7.0" )]
    public class MoreMigrationRollupsForV7_3 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            #region Comment out code

            //            // JE: Update Person Attribute to 'Suppress Sending Contribution Statements'
            //            Sql( @"  UPDATE [Attribute]
            //  SET [Name] = 'Suppress Sending Contribution Statements'
            //  WHERE [Guid] = 'B767F2CF-A4F0-45AA-A2E9-8270F31B307B'
            //" );

            //            // MP: Account  TopLevel Ordering Page
            //            RockMigrationHelper.AddPage( true, "2B630A3B-E081-4204-A3E4-17BB3A5F063D", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Order Top-Level Accounts", "", "AD1ED5A5-2E43-433F-B1C3-E6052213EF71", "" ); // Site:Rock RMS
            //            // Add Block to Page: Order Top-Level Accounts, Site: Rock RMS
            //            RockMigrationHelper.AddBlock( true, "AD1ED5A5-2E43-433F-B1C3-E6052213EF71", "", "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "Top-Level Account List", "Main", @"", @"", 0, "EB047AC6-40BB-4215-AACC-5ECE8FE73A9B" );
            //            // Attrib for BlockType: Scheduled Transaction List:Person Token Usage Limit
            //            RockMigrationHelper.UpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", "", @"When adding a new scheduled transaction from a person detail page, the maximum number of times the person token for the transaction can be used.", 4, @"1", "A6B71434-FD9B-45EC-AA50-07AE5D6BA384" );
            //            // Attrib for BlockType: Scheduled Transaction List:Person Token Expire Minutes
            //            RockMigrationHelper.UpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", "", @"When adding a new scheduled transaction from a person detail page, the number of minutes the person token for the transaction is valid after it is issued.", 3, @"60", "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17" );
            //            // Attrib for BlockType: Account Tree View:Order Top-Level Page
            //            RockMigrationHelper.UpdateBlockTypeAttribute( "EC6ECB2B-665F-43FC-9FF4-A6B1CD5F2AE6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Order Top-Level Page", "OrderTopLevelPage", "", @"", 5, @"", "F3E582D6-409E-4CDA-80FA-8E46B8CD95AF" );
            //            // Attrib Value for Block:Scheduled Contributions, Attribute:Person Token Usage Limit Page: Scheduled Transactions, Site: Rock RMS
            //            RockMigrationHelper.AddBlockAttributeValue( "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "A6B71434-FD9B-45EC-AA50-07AE5D6BA384", @"1" );
            //            // Attrib Value for Block:Scheduled Contributions, Attribute:Person Token Expire Minutes Page: Scheduled Transactions, Site: Rock RMS
            //            RockMigrationHelper.AddBlockAttributeValue( "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17", @"60" );
            //            // Attrib Value for Block:Finance - Giving Profile List, Attribute:Person Token Usage Limit Page: Contributions, Site: Rock RMS
            //            RockMigrationHelper.AddBlockAttributeValue( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0", "A6B71434-FD9B-45EC-AA50-07AE5D6BA384", @"1" );
            //            // Attrib Value for Block:Finance - Giving Profile List, Attribute:Person Token Expire Minutes Page: Contributions, Site: Rock RMS
            //            RockMigrationHelper.AddBlockAttributeValue( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0", "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17", @"60" );
            //            // Attrib Value for Block:Account Tree View, Attribute:Order Top-Level Page Page: Accounts, Site: Rock RMS
            //            RockMigrationHelper.AddBlockAttributeValue( "7F52E6B6-B4ED-4ECE-BE19-98BD3B939965", "F3E582D6-409E-4CDA-80FA-8E46B8CD95AF", @"ad1ed5a5-2e43-433f-b1c3-e6052213ef71" );


            //            // MP: StoreOrganizationKey SystemSetting
            //            Sql( @"UPDATE [Attribute]
            //SET EntityTypeQualifierColumn = 'SystemSetting'
            //WHERE [Key] = 'StoreOrganizationKey'
            //	AND EntityTypeId IS NULL
            //	AND EntityTypeQualifierColumn != 'SystemSetting'
            //" );

            //            // MP: Transaction Matching Batch Page
            //            // Attrib Value for Block:Transaction Matching, Attribute:Batch Detail Page Page: Transaction Matching, Site: Rock RMS
            //            RockMigrationHelper.AddBlockAttributeValue( "A18A0A0A-0B71-43B4-B830-44B802C272D4", "494C6487-8007-439F-BF0B-3F6020D159E8", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );

            //            // MP: Update ufnUtility_CsvToTable to return int
            //            Sql( HotFixMigrationResource._043_MoreMigrationRollupsForV7_3_ufnUtility_CsvToTable );

            //            // MP: Fix for slow v7 Person Duplicate Finder
            //            Sql( HotFixMigrationResource._043_MoreMigrationRollupsForV7_3_spCrm_PersonDuplicateFinder );

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
