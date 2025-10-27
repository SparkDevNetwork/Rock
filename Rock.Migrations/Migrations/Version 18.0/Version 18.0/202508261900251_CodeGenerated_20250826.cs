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
    public partial class CodeGenerated_20250826 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.FinancialBatchDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Finance.FinancialBatchDetail", "Financial Batch Detail", "Rock.Blocks.Types.Mobile.Finance.FinancialBatchDetail, Rock, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "14FE11A8-A4AA-43EF-9F36-354CE60240CA" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.FinancialBatchList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Finance.FinancialBatchList", "Financial Batch List", "Rock.Blocks.Types.Mobile.Finance.FinancialBatchList, Rock, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "E7E18663-7EF9-454C-9C6E-F4A839DB60C8" );

            // Add/Update Mobile Block Type
            //   Name:Financial Batch Detail
            //   Category:Mobile > Finance
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.FinancialBatchDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Batch Detail", "The Financial Batch Detail block.", "Rock.Blocks.Types.Mobile.Finance.FinancialBatchDetail", "Mobile > Finance", "C8EF68BE-522E-49E0-9BE5-C971433765DE" );

            // Add/Update Mobile Block Type
            //   Name:Financial Batch List
            //   Category:Mobile > Finance
            //   EntityType:Rock.Blocks.Types.Mobile.Finance.FinancialBatchList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Batch List", "The Financial Batch List block.", "Rock.Blocks.Types.Mobile.Finance.FinancialBatchList", "Mobile > Finance", "516E7877-DC6C-4706-812B-2DAEE649AA01" );

            // Attribute for BlockType
            //   BlockType: Group Simple Register
            //   Category: Groups
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 0, @"False", "D3CCAD34-4750-4243-B882-712635F9DDC4" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 15, @"False", "8039333F-7729-4D8C-932B-C62D17D0D6DA" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Register
            //   Category: Engagement > Sign-Up
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification will be skipped. 

Note: If the CAPTCHA site key and/or secret key are not configured in the system settings, this option will be forced as 'Yes', even if 'No' is visually selected.", 9, @"False", "E1ADB5F8-B394-4DE8-B810-6E44BF3DB092" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Group
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5E000376-FF90-4962-A053-EC1473DA5C45", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification will be skipped. 

Note: If the CAPTCHA site key and/or secret key are not configured in the system settings, this option will be forced as 'Yes', even if 'No' is visually selected.", 18, @"False", "0087E75E-D40F-4715-BB9C-2385C4EA7828" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AA30F53-1B7D-4CA9-89B6-C10592968870", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification will be skipped. 

Note: If the CAPTCHA site key and/or secret key are not configured in the system settings, this option will be forced as 'Yes', even if 'No' is visually selected.", 25, @"False", "DC529031-4E0B-4A83-BD4B-3C86206F6010" );

            // Attribute for BlockType
            //   BlockType: Transaction Detail
            //   Category: Mobile > Finance
            //   Attribute: Post Delete Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01A68151-30CC-4FBC-9FE5-2F20A2C1BB4F", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Post Delete Action", "PostDeleteAction", "Post Delete Action", @"The navigation action to perform when the delete button is pressed.", 0, @"{""Type"": 1, ""PopCount"": 1}", "DE9B76A9-7FBD-4F0F-8C21-C2EAA828B737" );

            // Attribute for BlockType
            //   BlockType: Transaction Detail
            //   Category: Mobile > Finance
            //   Attribute: Post Delete Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "01A68151-30CC-4FBC-9FE5-2F20A2C1BB4F", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Post Delete Action", "PostDeleteAction", "Post Delete Action", @"The navigation action to perform when the delete button is pressed.", 0, @"{""Type"": 1, ""PopCount"": 1}", "8485EBA5-EBD5-4081-9B95-D7EFB27D4018" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Required Control Amount
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Required Control Amount", "RequiredControlAmount", "Required Control Amount", @"Whether or not the control amount field is required when adding financial batch.", 6, @"False", "D14D6F17-1DDE-45FB-A1DE-4A922190DE4E" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Required Control Item Count
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Required Control Item Count", "RequiredControlItemCount", "Required Control Item Count", @"Whether or not the control item count field is required when adding financial batch.", 7, @"False", "181D2A63-11AF-42E0-8F0E-3FF7FBDD4E68" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Transaction Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Detail Page", "DetailPage", "Transaction Detail Page", @"The page linked when the user taps on a transaction in the list.", 1, @"", "325A22D6-4A02-4E91-9BCF-08BE8C33DB82" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"The list of accounts to display.", 2, @"", "1E8D317C-164F-440E-BCDB-9CF967A0C5F8" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Document Intelligence API Key
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Intelligence API Key", "ApiKey", "Document Intelligence API Key", @"The API key obtained from Azure Document Intelligence.", 3, @"", "C41931BE-0D9B-4410-90E4-262B98AE176F" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Document Intelligence Endpoint
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Intelligence Endpoint", "EndPoint", "Document Intelligence Endpoint", @"The endpoint used to access Azure Document Intelligence.", 4, @"", "8D8F0FE7-AB91-4318-8DAD-AF592AA20813" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Accounts Allocation Required
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C8EF68BE-522E-49E0-9BE5-C971433765DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Accounts Allocation Required", "AccountAllocationRequired", "Accounts Allocation Required", @"whether account allocation fields is required.", 5, @"False", "FE5BD26E-E1A0-4A4A-978C-5627C2946620" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Allow Add
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Add", "AllowAdd", "Allow Add", @"whether a new financial batch can be added.", 3, @"True", "3796CE91-64B6-4F63-B78F-4BCE076E54D8" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to link to when user taps on a Financial Batch List. FinancialBatchGuid is passed in the query string.", 4, @"", "86772A07-B264-4516-83CB-0833E6EBE7F2" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Status", "BatchStatus", "Status", @"Filter the batch based on their status.", 2, @"Pending,Open,Closed", "C002D98B-65B6-4354-A3C2-D55B6F5D0025" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Item Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Item Template", "HeaderContent", "Item Template", @"The template used to display the financial batch results.", 1, @"
<Rock:StyledBorder StyleClass=""border, border-interface-soft, rounded, bg-interface-softest, p-16, mb-8"">
    <Grid ColumnDefinitions=""Auto, Auto, *, Auto"" 
        RowDefinitions=""Auto, Auto, Auto""
        StyleClass=""gap-column-8"" >
        <Label StyleClass=""body, bold, text-interface-strongest, mb-8"" 
            Grid.Row=""0""
            Grid.Column=""0""
            Text=""{{ FinancialBatch.Name }}"" />
        
        <Label StyleClass=""footnote""  
            Grid.Row=""1""
            Grid.ColumnSpan=""3""
            Text=""Batch Date: {{ FinancialBatch.BatchStartDateTime | Date:'MMM dd, yyyy' }}"" />
        
        <Label StyleClass=""footnote"" 
            Grid.Row=""2""
            Text=""{{ FinancialBatch.Status }}"" />
        
        <Rock:Icon 
            Grid.Column=""3""
            Grid.RowSpan=""3""
            VerticalOptions=""Center""
            StyleClass=""footnote""
            IconClass=""chevron-right"" />
    </Grid>
    
    <Rock:StyledBorder.Behaviors>
        <Rock:TouchBehavior 
            PressedOpacity=""0.6"" 
            DefaultOpacity=""1"" 
            HoveredOpacity=""0.6"" 
            Command=""{Binding PushPage}"" 
            CommandParameter=""{{ DetailPage }}?FinancialBatch={{ FinancialBatchIdKey }}"" />
    </Rock:StyledBorder.Behaviors>
</Rock:StyledBorder>", "CD2D8FFC-0AF0-4212-BDAA-F56638499585" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Allow Filter By Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Filter By Campus", "AllowFilterByCampus", "Allow Filter By Campus", @"Whether or not filtering by campus should be enabled or not.", 5, @"True", "31F7AD4C-AF54-42A2-BFCD-32C6268B2D51" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Display Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Display Campus Types", "DisplayCampusTypes", "Display Campus Types", @"The campus types to filter to", 6, @"5A61507B-79CB-4DA2-AF43-6F82260203B3", "CF36EC02-39FD-4C4D-95DD-065B02250F8A" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Display Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Display Campus Types", "DisplayCampusTypes", "Display Campus Types", @"The campus types to filter to", 6, @"5A61507B-79CB-4DA2-AF43-6F82260203B3", "3CEE1C83-F257-4986-A2C2-70E55D957127" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Display Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Display Campus Statuses", "DisplayCampusStatuses", "Display Campus Statuses", @"The campus statuses to filter to.", 7, @"10696FD8-D0C7-486F-B736-5FB3F5D69F1A", "31C9E8E9-63E9-4577-8C7A-401DC5E065B7" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Display Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Display Campus Statuses", "DisplayCampusStatuses", "Display Campus Statuses", @"The campus statuses to filter to.", 7, @"10696FD8-D0C7-486F-B736-5FB3F5D69F1A", "D536F1B5-F921-486F-AD91-EF80243E16C8" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Page Load Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Page Load Size", "PageLoadSize", "Page Load Size", @"Determines the amount of batches to show in the initial page load and when scrolling to load more.", 8, @"100", "9F33E2B1-6903-4363-B9CE-23D3EA5D17AD" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Page Load Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Page Load Size", "PageLoadSize", "Page Load Size", @"Determines the amount of batches to show in the initial page load and when scrolling to load more.", 8, @"100", "3AC81C64-62E6-4775-AE0B-9FE3F1DAF103" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Post Batch Save Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Post Batch Save Action", "PostBatchSaveAction", "Post Batch Save Action", @"The navigation action to perform when you save a batch", 0, @"{""Type"": 1, ""PopCount"": 1}", "40E8DBD8-A13E-4CD7-90FF-22EF72038162" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Post Batch Save Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "516E7877-DC6C-4706-812B-2DAEE649AA01", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Post Batch Save Action", "PostBatchSaveAction", "Post Batch Save Action", @"The navigation action to perform when you save a batch", 0, @"{""Type"": 1, ""PopCount"": 1}", "E66860EF-0EB6-425A-B53F-064361239379" );

            // Add Block Attribute Value
            //   Block: File Type List
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Block Location: Page=File Types, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D60B8724-30D5-4F0A-9350-C34A0C57EEAC", "B346BA18-8792-4ECA-8ECF-DE5CF4549E41", @"" );

            // Add Block Attribute Value
            //   Block: File Type List
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Block Location: Page=File Types, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D60B8724-30D5-4F0A-9350-C34A0C57EEAC", "015EB659-C0BD-46FB-AE6A-439C25794DA3", @"False" );

            // Add Block Attribute Value
            //   Block: Communication List
            //   BlockType: Communication List
            //   Category: Communication
            //   Block Location: Page=Communication History, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8EC16D9D-3B62-446A-8F47-E45D0FADB5A5", "50E2162C-4CEA-4B05-8B1C-EE63ED4B6779", @"" );

            // Add Block Attribute Value
            //   Block: Communication List
            //   BlockType: Communication List
            //   Category: Communication
            //   Block Location: Page=Communication History, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "8EC16D9D-3B62-446A-8F47-E45D0FADB5A5", "3346071F-2521-483B-ABFA-456A1B4D9334", @"False" );

            // Add Block Attribute Value
            //   Block: Schedule Category Exclusion List
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Block Location: Page=Schedules, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D54FEBC7-965E-4AAF-94CC-D34EFA7BD625", "D2955C2F-360C-46D5-A90C-955EC5911A95", @"" );

            // Add Block Attribute Value
            //   Block: Schedule Category Exclusion List
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Block Location: Page=Schedules, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D54FEBC7-965E-4AAF-94CC-D34EFA7BD625", "CF5DD104-6015-4929-AC48-22CB747DC88B", @"False" );

            // Add Block Attribute Value
            //   Block: Schedule Category Exclusion List
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Block Location: Page=Schedules, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "2E2CE9F3-2C84-453E-BFB0-6B205E8131D6", "D2955C2F-360C-46D5-A90C-955EC5911A95", @"" );

            // Add Block Attribute Value
            //   Block: Schedule Category Exclusion List
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Block Location: Page=Schedules, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "2E2CE9F3-2C84-453E-BFB0-6B205E8131D6", "CF5DD104-6015-4929-AC48-22CB747DC88B", @"False" );

            // Add Block Attribute Value
            //   Block: Merge Template List
            //   BlockType: Merge Template List
            //   Category: Core
            //   Block Location: Page=Merge Templates, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "A60EABD6-97C9-4860-A1B6-646EF4A043A4", "A8ECF14E-6410-43EB-9AA9-9673BF647FFC", @"" );

            // Add Block Attribute Value
            //   Block: Merge Template List
            //   BlockType: Merge Template List
            //   Category: Core
            //   Block Location: Page=Merge Templates, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "A60EABD6-97C9-4860-A1B6-646EF4A043A4", "E6270DD3-8D4B-4A5A-A8F6-ACA701A85195", @"False" );

            // Add Block Attribute Value
            //   Block: Prayer Comment List
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Block Location: Page=Prayer Comments, Site=Rock RMS
            //   Attribute: core.CustomGridColumnsConfig
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "91D99934-A753-44F5-9967-08222C1C101E", "BBFE1BD6-C335-415B-95DF-2D62F4827E2D", @"" );

            // Add Block Attribute Value
            //   Block: Prayer Comment List
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Block Location: Page=Prayer Comments, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "91D99934-A753-44F5-9967-08222C1C101E", "B2454ADD-9CFE-4BB9-9624-6BDD6930F8D8", @"False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Group Simple Register
            //   Category: Groups
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "D3CCAD34-4750-4243-B882-712635F9DDC4" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "8039333F-7729-4D8C-932B-C62D17D0D6DA" );

            // Attribute for BlockType
            //   BlockType: Transaction Detail
            //   Category: Mobile > Finance
            //   Attribute: Post Delete Action
            RockMigrationHelper.DeleteAttribute( "8485EBA5-EBD5-4081-9B95-D7EFB27D4018" );

            // Attribute for BlockType
            //   BlockType: Transaction Detail
            //   Category: Mobile > Finance
            //   Attribute: Post Delete Action
            RockMigrationHelper.DeleteAttribute( "DE9B76A9-7FBD-4F0F-8C21-C2EAA828B737" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Post Batch Save Action
            RockMigrationHelper.DeleteAttribute( "E66860EF-0EB6-425A-B53F-064361239379" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Post Batch Save Action
            RockMigrationHelper.DeleteAttribute( "40E8DBD8-A13E-4CD7-90FF-22EF72038162" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Page Load Size
            RockMigrationHelper.DeleteAttribute( "3AC81C64-62E6-4775-AE0B-9FE3F1DAF103" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Page Load Size
            RockMigrationHelper.DeleteAttribute( "9F33E2B1-6903-4363-B9CE-23D3EA5D17AD" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Display Campus Statuses
            RockMigrationHelper.DeleteAttribute( "D536F1B5-F921-486F-AD91-EF80243E16C8" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Display Campus Statuses
            RockMigrationHelper.DeleteAttribute( "31C9E8E9-63E9-4577-8C7A-401DC5E065B7" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Display Campus Types
            RockMigrationHelper.DeleteAttribute( "3CEE1C83-F257-4986-A2C2-70E55D957127" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Display Campus Types
            RockMigrationHelper.DeleteAttribute( "CF36EC02-39FD-4C4D-95DD-065B02250F8A" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Allow Filter By Campus
            RockMigrationHelper.DeleteAttribute( "31F7AD4C-AF54-42A2-BFCD-32C6268B2D51" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Required Control Item Count
            RockMigrationHelper.DeleteAttribute( "181D2A63-11AF-42E0-8F0E-3FF7FBDD4E68" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Required Control Amount
            RockMigrationHelper.DeleteAttribute( "D14D6F17-1DDE-45FB-A1DE-4A922190DE4E" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "86772A07-B264-4516-83CB-0833E6EBE7F2" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Allow Add
            RockMigrationHelper.DeleteAttribute( "3796CE91-64B6-4F63-B78F-4BCE076E54D8" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Accounts Allocation Required
            RockMigrationHelper.DeleteAttribute( "FE5BD26E-E1A0-4A4A-978C-5627C2946620" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Document Intelligence Endpoint
            RockMigrationHelper.DeleteAttribute( "8D8F0FE7-AB91-4318-8DAD-AF592AA20813" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Status
            RockMigrationHelper.DeleteAttribute( "C002D98B-65B6-4354-A3C2-D55B6F5D0025" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Document Intelligence API Key
            RockMigrationHelper.DeleteAttribute( "C41931BE-0D9B-4410-90E4-262B98AE176F" );

            // Attribute for BlockType
            //   BlockType: Financial Batch List
            //   Category: Mobile > Finance
            //   Attribute: Item Template
            RockMigrationHelper.DeleteAttribute( "CD2D8FFC-0AF0-4212-BDAA-F56638499585" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Accounts
            RockMigrationHelper.DeleteAttribute( "1E8D317C-164F-440E-BCDB-9CF967A0C5F8" );

            // Attribute for BlockType
            //   BlockType: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Attribute: Transaction Detail Page
            RockMigrationHelper.DeleteAttribute( "325A22D6-4A02-4E91-9BCF-08BE8C33DB82" );

            // Attribute for BlockType
            //   BlockType: Sign-Up Register
            //   Category: Engagement > Sign-Up
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "E1ADB5F8-B394-4DE8-B810-6E44BF3DB092" );

            // Attribute for BlockType
            //   BlockType: Group Registration
            //   Category: Group
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "0087E75E-D40F-4715-BB9C-2385C4EA7828" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Entry
            //   Category: Prayer
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "DC529031-4E0B-4A83-BD4B-3C86206F6010" );

            // Delete BlockType 
            //   Name: Financial Batch List
            //   Category: Mobile > Finance
            //   Path: -
            //   EntityType: Financial Batch List
            RockMigrationHelper.DeleteBlockType( "516E7877-DC6C-4706-812B-2DAEE649AA01" );

            // Delete BlockType 
            //   Name: Financial Batch Detail
            //   Category: Mobile > Finance
            //   Path: -
            //   EntityType: Financial Batch Detail
            RockMigrationHelper.DeleteBlockType( "C8EF68BE-522E-49E0-9BE5-C971433765DE" );
        }
    }
}
