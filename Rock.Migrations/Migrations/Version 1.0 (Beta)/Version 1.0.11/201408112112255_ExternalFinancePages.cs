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
    public partial class ExternalFinancePages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // rename the external site
            Sql( @"UPDATE [Site]
                SET [Name] = 'External Website'
                WHERE	[Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B' 
		                AND Name = 'Rock Solid Church'" );

            RockMigrationHelper.AddPage( "8BB303AF-743C-49DC-A7FF-CC1236B4B1D9", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "View My Giving", "", "621E03C5-6586-450B-A340-CC7D9727B69A", "" ); // Site:Rock Solid Church
            RockMigrationHelper.AddPage( "8BB303AF-743C-49DC-A7FF-CC1236B4B1D9", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Manage Giving Profiles", "", "FFFDCE23-7B67-4B0D-8DA0-E44D883708CC", "" ); // Site:Rock Solid Church
            RockMigrationHelper.AddPage( "FFFDCE23-7B67-4B0D-8DA0-E44D883708CC", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Edit Giving Profile", "", "2072F4BC-53B4-4481-BC15-38F14425C6C9", "" ); // Site:Rock Solid Church
            RockMigrationHelper.UpdateBlockType( "Scheduled Transaction List Liquid", "Block that shows a list of scheduled transactions for the currently logged in user with the ability to modify the formatting using liquid.", "~/Blocks/Finance/ScheduledTransactionListLiquid.ascx", "Finance", "081FF29F-0A9F-4EC3-95AD-708FA0E6132D" );
            RockMigrationHelper.UpdateBlockType( "Transaction Report", "Block that reports transactions for the currently logged in user with filters.", "~/Blocks/Finance/TransactionReport.ascx", "Finance", "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D" );

            // Add Block to Page: Give, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "8BB303AF-743C-49DC-A7FF-CC1236B4B1D9", "", "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA", "Scheduled Transaction Summary", "Main", "", "", 0, "E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD" );

            // Add Block to Page: View My Giving, Site: External Website
            RockMigrationHelper.AddBlock( "621E03C5-6586-450B-A340-CC7D9727B69A", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Transaction Report Intro Text", "Main", "", "", 0, "8A5E5144-3054-4FC9-AD8A-B0F4813C94E4" );

            // Add Block to Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "621E03C5-6586-450B-A340-CC7D9727B69A", "", "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "Transaction Report", "Main", "", "", 1, "0B62A727-1AEB-4134-AFAE-1EBB73A6B098" );

            // Add Block to Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "621E03C5-6586-450B-A340-CC7D9727B69A", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "B4FADF76-ED25-4641-A041-4AE2D46FD689" );

            // Add Block to Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "1615E090-1889-42FF-AB18-5F7BE9F24498", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "95C60041-E6C7-4011-8841-6243E2C0208C" );

            // Add Block to Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "FFFDCE23-7B67-4B0D-8DA0-E44D883708CC", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "01AA807E-DD75-4C1B-96E0-760D1AD06015" );

            // Add Block to Page: Manage Giving Profiles, Site: External Website
            RockMigrationHelper.AddBlock( "FFFDCE23-7B67-4B0D-8DA0-E44D883708CC", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Profile Intro Text", "Main", "", "", 0, "88415BD1-A458-4111-BDC9-3F66DC782E71" );

            // Add Block to Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "FFFDCE23-7B67-4B0D-8DA0-E44D883708CC", "", "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "Scheduled Transaction List Liquid", "Main", "", "", 1, "0D91DD2F-519C-4A4A-AB03-0933FC12BE7E" );

            // Add Block to Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "2072F4BC-53B4-4481-BC15-38F14425C6C9", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "60304123-B27F-4A7E-825B-5B285E6CCF13" );

            // Add Block to Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "2072F4BC-53B4-4481-BC15-38F14425C6C9", "", "5171C4E5-7698-453E-9CC8-088D362296DE", "Scheduled Transaction Edit", "Main", "", "", 0, "75F15397-3B82-4879-B069-DABD3619FAA3" );

            // Add Block to Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "A974A965-414B-47A6-9CC1-D3A175DA965B", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9" );

            // Add Block to Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "A974A965-414B-47A6-9CC1-D3A175DA965B", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Intro Text", "Main", "", "", 0, "83D0018A-CAE4-469F-84A7-A113CD2EC033" );

            // Attrib for BlockType: Scheduled Transaction Summary:Transaction History Page
            RockMigrationHelper.AddBlockTypeAttribute( "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction History Page", "TransactionHistoryPage", "", "Link to use for viewing an individual's transaction history.", 4, @"", "E541A25B-3D59-43B5-A963-DA26222A41B7" );

            // Attrib for BlockType: Scheduled Transaction Summary:Transaction Entry Page
            RockMigrationHelper.AddBlockTypeAttribute( "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Entry Page", "TransactionEntryPage", "", "Link to use when adding new transactions.", 5, @"", "477E80E5-4E81-4752-9771-64F664E706FF" );

            // Attrib for BlockType: Scheduled Transaction Summary:Template
            RockMigrationHelper.AddBlockTypeAttribute( "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "", "Liquid template for the content to be placed on the page.", 1, @"

<div class=""row margin-b-md"">

    <div class=""col-md-6"">
        <h1 class=""condensed"">Hello, {{Person.NickName}}</h1>
    </div>

    <div class=""col-md-6"">
        {% if ScheduledTransactions.size == 1 %}
            <p>
                You currently have <span class='label label-default'>1</span> giving profile active.
            </p>
            <p>
                {% if ScheduledTransactions[0].DaysTillNextPayment > 0 %}
                    Next gift is in {{ScheduledTransactions[0].DaysTillNextPayment}} days.
                {% else %}
                    Next gift is scheduled for today.
                {% endif %}
                
                {% if ScheduledTransactions[0].LastPaymentDate != null %}
                    {% if ScheduledTransactions[0].DaysSinceLastPayment > 0 %}
                        Last gift was {{ScheduledTransactions[0].DaysSinceLastPayment}} days ago.
                    {% else %}
                        Last gift was today.
                    {% endif %}
                {% endif %}
            </p>
        {% elsif ScheduledTransactions.size > 1 %}
            You currently have <span class='label label-default'>{{ScheduledTransactions.size}}</span> 
            giving profiles active.
        {% else %}
            You currently have no active profiles.
        {% endif %}
        <div class=""clearfix"">
            <a class=""btn btn-default pull-left"" href=""{{LinkedPages.ManageScheduledTransactionsPage}}"">Manage</a> 
            <a class=""btn btn-default pull-right"" href=""{{LinkedPages.TransactionHistoryPage}}"">View History</a>
        </div>
        <a class=""btn btn-primary btn-block margin-t-md"" href=""{{LinkedPages.TransactionEntryPage}}"">Give Now</a>
    </div>
</div>

", "394C30E6-22EE-4312-9870-EA90336F5778" );

            // Attrib for BlockType: Scheduled Transaction Summary:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Displays a list of available merge fields using the current person's scheduled transactions.", 2, @"False", "026EB30C-7608-4285-B233-A8F16EFBF069" );

            // Attrib for BlockType: Scheduled Transaction Summary:Manage Scheduled Transactions Page
            RockMigrationHelper.AddBlockTypeAttribute( "3FC83F0E-8BAA-4CB3-BAD0-0CFBE0E621AA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manage Scheduled Transactions Page", "ManageScheduledTransactionsPage", "", "Link to be used for managing an individual's scheduled transactions.", 3, @"", "15E4ED16-138C-4FE6-AB9B-A860E716289E" );

            // Attrib for BlockType: Transaction Report:Account Label
            RockMigrationHelper.AddBlockTypeAttribute( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Account Label", "AccountLabel", "", "The label to use to describe accounts.", 2, @"Accounts", "D9865EC2-8C6E-443C-BDFA-4E22193E8C36" );

            // Attrib for BlockType: Transaction Report:Accounts
            RockMigrationHelper.AddBlockTypeAttribute( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "", "List of accounts to allow the person to view", 3, @"", "A5601A16-0E00-4C08-A74B-D84FE5047C30" );

            // Attrib for BlockType: Transaction Report:Transaction Label
            RockMigrationHelper.AddBlockTypeAttribute( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Transaction Label", "TransactionLabel", "", "The label to use to describe the transactions (e.g. 'Gifts', 'Donations', etc.)", 1, @"Gifts", "8FB1B244-D056-45A9-9B48-93EAEEC66E91" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Scheduled Transaction Entry Page
            RockMigrationHelper.AddBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Entry Page", "ScheduledTransactionEntryPage", "", "Link to use when adding new transactions.", 4, @"", "CDF5DF15-CA89-4AD1-AEAA-DE9C962E19D1" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Template
            RockMigrationHelper.AddBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "", "Liquid template for the display of the scheduled transactions.", 1, @"
<div class=""scheduledtransaction-summary"">
    ${{ScheduledTransaction.ScheduledAmount}} on {{ScheduledTransaction.CurrencyType}}
    {{ScheduledTransaction.FrequencyDescription | downcase}}. 
     
    {% if ScheduledTransaction.NextPaymentDate != null %}
        Next gift will be on {{ScheduledTransaction.NextPaymentDate | Date:""MMMM d, yyyy""}}.
    {% endif %}
</div>
", "14BC144B-878D-4432-A8A7-8B1BB8B27E89" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Scheduled Transaction Edit Page
            RockMigrationHelper.AddBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Edit Page", "ScheduledTransactionEditPage", "", "Link to be used for managing an individual's scheduled transactions.", 3, @"", "7A402A6D-0E81-4FDE-A5E9-AE0A65C187B9" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Displays a list of available merge fields using the current person's scheduled transactions.", 2, @"False", "2CE480C1-721A-4FB2-9F82-4447FE442FC4" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Transaction Label
            RockMigrationHelper.AddBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Transaction Label", "TransactionLabel", "", "The label to use to describe the transaction (e.g. 'Gift', 'Donation', etc.)", 5, @"Gift", "1AB6F113-DBBA-444D-90AD-E9F8CFDE21FE" );

            // Attrib for BlockType: Scheduled Transaction Edit:Layout Style
            RockMigrationHelper.AddBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Layout Style", "LayoutStyle", "", "How the sections of this page should be displayed", 2, @"Vertical", "F6F910CE-FB6D-4783-9DB4-D865744F1F09" );

            // Attrib Value for Block:Scheduled Transaction Summary, Attribute:Enable Debug Page: Give, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD", "026EB30C-7608-4285-B233-A8F16EFBF069", @"False" );

            // Attrib Value for Block:Scheduled Transaction Summary, Attribute:Manage Scheduled Transactions Page Page: Give, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD", "15E4ED16-138C-4FE6-AB9B-A860E716289E", @"fffdce23-7b67-4b0d-8da0-e44d883708cc" );

            // Attrib Value for Block:Scheduled Transaction Summary, Attribute:Template Page: Give, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD", "394C30E6-22EE-4312-9870-EA90336F5778", @"

<div class=""row margin-b-md"">

    <div class=""col-md-6"">
        <h1 class=""condensed"">Hello, {{Person.NickName}}</h1>
    </div>

    <div class=""col-md-6"">
        {% if ScheduledTransactions.size == 1 %}
            <p>
                You currently have <span class='label label-default'>1</span> giving profile active.
            </p>
            <p>
                {% if ScheduledTransactions[0].DaysTillNextPayment > 0 %}
                    Next gift is in {{ScheduledTransactions[0].DaysTillNextPayment}} days.
                {% else %}
                    Next gift is scheduled for today.
                {% endif %}
                
                {% if ScheduledTransactions[0].LastPaymentDate != null %}
                    {% if ScheduledTransactions[0].DaysSinceLastPayment > 0 %}
                        Last gift was {{ScheduledTransactions[0].DaysSinceLastPayment}} days ago.
                    {% else %}
                        Last gift was today.
                    {% endif %}
                {% endif %}
            </p>
        {% elsif ScheduledTransactions.size > 1 %}
            You currently have <span class='label label-default'>{{ScheduledTransactions.size}}</span> 
            giving profiles active.
        {% else %}
            You currently have no active profiles.
        {% endif %}
        <div class=""clearfix"">
            <a class=""btn btn-default pull-left"" href=""{{LinkedPages.ManageScheduledTransactionsPage}}"">Manage</a> 
            <a class=""btn btn-default pull-right"" href=""{{LinkedPages.TransactionHistoryPage}}"">View History</a>
        </div>
        <a class=""btn btn-primary btn-block margin-t-md"" href=""{{LinkedPages.TransactionEntryPage}}"">Give Now</a>
    </div>
</div>

" );

            // Attrib Value for Block:Scheduled Transaction Summary, Attribute:Transaction Entry Page Page: Give, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD", "477E80E5-4E81-4752-9771-64F664E706FF", @"1615e090-1889-42ff-ab18-5f7be9f24498" );

            // Attrib Value for Block:Scheduled Transaction Summary, Attribute:Transaction History Page Page: Give, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD", "E541A25B-3D59-43B5-A963-DA26222A41B7", @"621e03c5-6586-450b-a340-cc7d9727b69a" );

            // Attrib Value for Block:Transaction Report, Attribute:Account Label Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "0B62A727-1AEB-4134-AFAE-1EBB73A6B098", "D9865EC2-8C6E-443C-BDFA-4E22193E8C36", @"Accounts" );

            // Attrib Value for Block:Transaction Report, Attribute:Accounts Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "0B62A727-1AEB-4134-AFAE-1EBB73A6B098", "A5601A16-0E00-4C08-A74B-D84FE5047C30", @"4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8" );

            // Attrib Value for Block:Transaction Report, Attribute:Transaction Label Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "0B62A727-1AEB-4134-AFAE-1EBB73A6B098", "8FB1B244-D056-45A9-9B48-93EAEEC66E91", @"Gifts" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "B4FADF76-ED25-4641-A041-4AE2D46FD689", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "B4FADF76-ED25-4641-A041-4AE2D46FD689", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "B4FADF76-ED25-4641-A041-4AE2D46FD689", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "B4FADF76-ED25-4641-A041-4AE2D46FD689", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "B4FADF76-ED25-4641-A041-4AE2D46FD689", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "B4FADF76-ED25-4641-A041-4AE2D46FD689", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "B4FADF76-ED25-4641-A041-4AE2D46FD689", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"8bb303af-743c-49dc-a7ff-cc1236b4b1d9" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "B4FADF76-ED25-4641-A041-4AE2D46FD689", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageSubNav'  %}" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "95C60041-E6C7-4011-8841-6243E2C0208C", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "95C60041-E6C7-4011-8841-6243E2C0208C", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "95C60041-E6C7-4011-8841-6243E2C0208C", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "95C60041-E6C7-4011-8841-6243E2C0208C", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "95C60041-E6C7-4011-8841-6243E2C0208C", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "95C60041-E6C7-4011-8841-6243E2C0208C", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "95C60041-E6C7-4011-8841-6243E2C0208C", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"8bb303af-743c-49dc-a7ff-cc1236b4b1d9" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "95C60041-E6C7-4011-8841-6243E2C0208C", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageSubNav'  %}" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "01AA807E-DD75-4C1B-96E0-760D1AD06015", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "01AA807E-DD75-4C1B-96E0-760D1AD06015", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "01AA807E-DD75-4C1B-96E0-760D1AD06015", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "01AA807E-DD75-4C1B-96E0-760D1AD06015", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "01AA807E-DD75-4C1B-96E0-760D1AD06015", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "01AA807E-DD75-4C1B-96E0-760D1AD06015", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "01AA807E-DD75-4C1B-96E0-760D1AD06015", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"8bb303af-743c-49dc-a7ff-cc1236b4b1d9" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "01AA807E-DD75-4C1B-96E0-760D1AD06015", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageSubNav'  %}
" );

            // Attrib Value for Block:Scheduled Transaction List Liquid, Attribute:Enable Debug Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "0D91DD2F-519C-4A4A-AB03-0933FC12BE7E", "2CE480C1-721A-4FB2-9F82-4447FE442FC4", @"False" );

            // Attrib Value for Block:Scheduled Transaction List Liquid, Attribute:Scheduled Transaction Edit Page Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "0D91DD2F-519C-4A4A-AB03-0933FC12BE7E", "7A402A6D-0E81-4FDE-A5E9-AE0A65C187B9", @"2072f4bc-53b4-4481-bc15-38f14425c6c9" );

            // Attrib Value for Block:Scheduled Transaction List Liquid, Attribute:Scheduled Transaction Entry Page Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "0D91DD2F-519C-4A4A-AB03-0933FC12BE7E", "CDF5DF15-CA89-4AD1-AEAA-DE9C962E19D1", @"1615e090-1889-42ff-ab18-5f7be9f24498" );

            // Attrib Value for Block:Scheduled Transaction List Liquid, Attribute:Template Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "0D91DD2F-519C-4A4A-AB03-0933FC12BE7E", "14BC144B-878D-4432-A8A7-8B1BB8B27E89", @"
<div class=""scheduledtransaction-summary"">
    ${{ScheduledTransaction.ScheduledAmount}} on {{ScheduledTransaction.CurrencyType}}
    {{ScheduledTransaction.FrequencyDescription | downcase}}. 
     
    {% if ScheduledTransaction.NextPaymentDate != null %}
        Next gift will be on {{ScheduledTransaction.NextPaymentDate | Date:""MMMM d, yyyy""}}.
    {% endif %}
</div>
" );

            // Attrib Value for Block:Scheduled Transaction List Liquid, Attribute:Transaction Label Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "0D91DD2F-519C-4A4A-AB03-0933FC12BE7E", "1AB6F113-DBBA-444D-90AD-E9F8CFDE21FE", @"Gift" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "60304123-B27F-4A7E-825B-5B285E6CCF13", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "60304123-B27F-4A7E-825B-5B285E6CCF13", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "60304123-B27F-4A7E-825B-5B285E6CCF13", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "60304123-B27F-4A7E-825B-5B285E6CCF13", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "60304123-B27F-4A7E-825B-5B285E6CCF13", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "60304123-B27F-4A7E-825B-5B285E6CCF13", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "60304123-B27F-4A7E-825B-5B285E6CCF13", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"8bb303af-743c-49dc-a7ff-cc1236b4b1d9" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "60304123-B27F-4A7E-825B-5B285E6CCF13", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageSubNav'  %}" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"8bb303af-743c-49dc-a7ff-cc1236b4b1d9" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlockAttributeValue( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageSubNav'  %}" );

            // reorder block on give page
            Sql( @"UPDATE [Block]
                    SET [Order] = 1
                    WHERE [Guid] = '0AC35C5D-C395-40B0-9293-88153DF1D1B3'" );

            // reorder block on pledge page
            Sql( @"UPDATE [Block]
                    SET [Order] = 1
                    WHERE [Guid] = 'C6007437-A565-4144-9DB3-DD590D62D5E2'" );

            // add pledge content
            Sql( @"DECLARE @PledgeContentBlock int
SET @PledgeContentBlock = (SELECT TOP 1 [ID] FROM [Block] WHERE [Guid] = '83D0018A-CAE4-469F-84A7-A113CD2EC033')

INSERT INTO [dbo].[HtmlContent]
           ([BlockId]
           ,[Version]
           ,[Content]
           ,[IsApproved]
           ,[ApprovedByPersonId]
           ,[ApprovedDateTime]
           ,[StartDateTime]
           ,[ExpireDateTime]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId])
     VALUES
           (@PledgeContentBlock
           ,1
           ,'<p>Thank you for your interest in making a commitment by pledging. Please complete the form below to complete your pledge.</p> <hr />'
           ,1
           ,1
           ,getdate()
           ,null
           ,null
           ,'13DA5069-802C-4831-BDEF-2C99BD612DAE'
           ,getdate()
           ,getdate()
           ,null
           ,null)" );

            // add trans report content
            Sql( @"DECLARE @PledgeContentBlock int
SET @PledgeContentBlock = (SELECT TOP 1 [ID] FROM [Block] WHERE [Guid] = '8A5E5144-3054-4FC9-AD8A-B0F4813C94E4')

INSERT INTO [dbo].[HtmlContent]
           ([BlockId]
           ,[Version]
           ,[Content]
           ,[IsApproved]
           ,[ApprovedByPersonId]
           ,[ApprovedDateTime]
           ,[StartDateTime]
           ,[ExpireDateTime]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId])
     VALUES
           (@PledgeContentBlock
           ,1
           ,'<p>The report below will allow you to view your previous giving. Use the date and account filters to adjust the display.</p> <hr />'
           ,1
           ,1
           ,getdate()
           ,null
           ,null
           ,'A1392134-FCE8-4379-AE81-6EDF2F7B8450'
           ,getdate()
           ,getdate()
           ,null
           ,null)" );

            // add profile edit content
            Sql( @"DECLARE @PledgeContentBlock int
SET @PledgeContentBlock = (SELECT TOP 1 [ID] FROM [Block] WHERE [Guid] = '88415BD1-A458-4111-BDC9-3F66DC782E71')

INSERT INTO [dbo].[HtmlContent]
           ([BlockId]
           ,[Version]
           ,[Content]
           ,[IsApproved]
           ,[ApprovedByPersonId]
           ,[ApprovedDateTime]
           ,[StartDateTime]
           ,[ExpireDateTime]
           ,[Guid]
           ,[CreatedDateTime]
           ,[ModifiedDateTime]
           ,[CreatedByPersonAliasId]
           ,[ModifiedByPersonAliasId])
     VALUES
           (@PledgeContentBlock
           ,1
           ,'<p>
                Your current giving profile(s) are below. You can change the Frequency, 
                Start Date, and Amount that you''d like to contribute by selecting the 
                ""Edit"" button. If you wish to stop the automated giving program 
                you can click the ""Delete"" button. Once you delete your profile you can create a new one at any time.</p><hr />'
           ,1
           ,1
           ,getdate()
           ,null
           ,null
           ,'6D736357-CEF0-4C29-B5E5-E0A7C9CDFD9B'
           ,getdate()
           ,getdate()
           ,null
           ,null)" );

            // set blocks to be not system
            Sql( @"  UPDATE [Block]
	                    SET [IsSystem] = 0
	                    WHERE [Guid] IN ('E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD'
					                    , '0B62A727-1AEB-4134-AFAE-1EBB73A6B098'
					                    , 'B4FADF76-ED25-4641-A041-4AE2D46FD689'
					                    , '0B62A727-1AEB-4134-AFAE-1EBB73A6B098'
					                    , '95C60041-E6C7-4011-8841-6243E2C0208C'
					                    , '01AA807E-DD75-4C1B-96E0-760D1AD06015'
					                    , '0D91DD2F-519C-4A4A-AB03-0933FC12BE7E'
					                    , '60304123-B27F-4A7E-825B-5B285E6CCF13'
					                    , '75F15397-3B82-4879-B069-DABD3619FAA3'
					                    , '4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9'
					                    , '83D0018A-CAE4-469F-84A7-A113CD2EC033'
                                        , '8A5E5144-3054-4FC9-AD8A-B0F4813C94E4'
                                        , '88415BD1-A458-4111-BDC9-3F66DC782E71')" );


            // change the order of the giving child pages
            Sql( @"    UPDATE [Page]
	                    SET [Order] = (SELECT MAX([Order]) FROM [Page] WHERE [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '8BB303AF-743C-49DC-A7FF-CC1236B4B1D9'))
	                    WHERE [Guid] = 'A974A965-414B-47A6-9CC1-D3A175DA965B'" );

            // set new pages to not be system
            Sql( @"UPDATE [Page]
	                SET [IsSystem] = 0
	                WHERE [Guid] IN ('621E03C5-6586-450B-A340-CC7D9727B69A', 'FFFDCE23-7B67-4B0D-8DA0-E44D883708CC', '2072F4BC-53B4-4481-BC15-38F14425C6C9')" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete pledge content
            Sql( @"  DELETE FROM [HtmlContent]
                         WHERE [Guid] = '13DA5069-802C-4831-BDEF-2C99BD612DAE'" );

            // Attrib for BlockType: Scheduled Transaction Edit:Layout Style
            RockMigrationHelper.DeleteAttribute( "F6F910CE-FB6D-4783-9DB4-D865744F1F09" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Transaction Label
            RockMigrationHelper.DeleteAttribute( "1AB6F113-DBBA-444D-90AD-E9F8CFDE21FE" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Enable Debug
            RockMigrationHelper.DeleteAttribute( "2CE480C1-721A-4FB2-9F82-4447FE442FC4" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Scheduled Transaction Edit Page
            RockMigrationHelper.DeleteAttribute( "7A402A6D-0E81-4FDE-A5E9-AE0A65C187B9" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Template
            RockMigrationHelper.DeleteAttribute( "14BC144B-878D-4432-A8A7-8B1BB8B27E89" );

            // Attrib for BlockType: Scheduled Transaction List Liquid:Scheduled Transaction Entry Page
            RockMigrationHelper.DeleteAttribute( "CDF5DF15-CA89-4AD1-AEAA-DE9C962E19D1" );

            // Attrib for BlockType: Transaction Report:Transaction Label
            RockMigrationHelper.DeleteAttribute( "8FB1B244-D056-45A9-9B48-93EAEEC66E91" );

            // Attrib for BlockType: Transaction Report:Accounts
            RockMigrationHelper.DeleteAttribute( "A5601A16-0E00-4C08-A74B-D84FE5047C30" );

            // Attrib for BlockType: Transaction Report:Account Label
            RockMigrationHelper.DeleteAttribute( "D9865EC2-8C6E-443C-BDFA-4E22193E8C36" );

            // Attrib for BlockType: Scheduled Transaction Summary:Manage Scheduled Transactions Page
            RockMigrationHelper.DeleteAttribute( "15E4ED16-138C-4FE6-AB9B-A860E716289E" );

            // Attrib for BlockType: Scheduled Transaction Summary:Enable Debug
            RockMigrationHelper.DeleteAttribute( "026EB30C-7608-4285-B233-A8F16EFBF069" );

            // Attrib for BlockType: Scheduled Transaction Summary:Template
            RockMigrationHelper.DeleteAttribute( "394C30E6-22EE-4312-9870-EA90336F5778" );

            // Attrib for BlockType: Scheduled Transaction Summary:Transaction Entry Page
            RockMigrationHelper.DeleteAttribute( "477E80E5-4E81-4752-9771-64F664E706FF" );

            // Attrib for BlockType: Scheduled Transaction Summary:Transaction History Page
            RockMigrationHelper.DeleteAttribute( "E541A25B-3D59-43B5-A963-DA26222A41B7" );

            // Remove Block: Profile Intro Text, from Page: Manage Giving Profiles, Site: External Website
            RockMigrationHelper.DeleteBlock( "88415BD1-A458-4111-BDC9-3F66DC782E71" );

            // Remove Block: Transaction Report Intro Text, from Page: View My Giving, Site: External Website
            RockMigrationHelper.DeleteBlock( "8A5E5144-3054-4FC9-AD8A-B0F4813C94E4" );

            // Remove Block: Intro Text, from Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "83D0018A-CAE4-469F-84A7-A113CD2EC033" );

            // Remove Block: Page Menu, from Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "4F9C0A6C-BDAF-462C-B1A2-C61E3DE60EC9" );

            // Remove Block: Scheduled Transaction Edit, from Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "75F15397-3B82-4879-B069-DABD3619FAA3" );

            // Remove Block: Page Menu, from Page: Edit Giving Profile, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "60304123-B27F-4A7E-825B-5B285E6CCF13" );

            // Remove Block: Scheduled Transaction List Liquid, from Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "0D91DD2F-519C-4A4A-AB03-0933FC12BE7E" );

            // Remove Block: Page Menu, from Page: Manage Giving Profiles, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "01AA807E-DD75-4C1B-96E0-760D1AD06015" );

            // Remove Block: Page Menu, from Page: Give Now, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "95C60041-E6C7-4011-8841-6243E2C0208C" );

            // Remove Block: Page Menu, from Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "B4FADF76-ED25-4641-A041-4AE2D46FD689" );

            // Remove Block: Transaction Report, from Page: View My Giving, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "0B62A727-1AEB-4134-AFAE-1EBB73A6B098" );

            // Remove Block: Scheduled Transaction Summary, from Page: Give, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "E7B2EEF7-B06E-4FFF-8443-A10DEC30E1FD" );
            RockMigrationHelper.DeleteBlockType( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D" ); // Transaction Report
            RockMigrationHelper.DeleteBlockType( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D" ); // Scheduled Transaction List Liquid
            RockMigrationHelper.DeletePage( "2072F4BC-53B4-4481-BC15-38F14425C6C9" ); //  Page: Edit Giving Profile, Layout: LeftSidebar, Site: Rock Solid Church
            RockMigrationHelper.DeletePage( "FFFDCE23-7B67-4B0D-8DA0-E44D883708CC" ); //  Page: Manage Giving Profiles, Layout: LeftSidebar, Site: Rock Solid Church
            RockMigrationHelper.DeletePage( "621E03C5-6586-450B-A340-CC7D9727B69A" ); //  Page: View My Giving, Layout: LeftSidebar, Site: Rock Solid Church
        }
    }
}
