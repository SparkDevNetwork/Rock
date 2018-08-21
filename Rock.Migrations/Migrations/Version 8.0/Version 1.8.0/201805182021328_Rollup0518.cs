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
    public partial class Rollup0518 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Person Group History", "Displays a timeline of a person's history in groups", "~/Blocks/Crm/PersonDetail/PersonGroupHistory.ascx", "CRM > Person Detail", "F8E351BC-607E-4897-B732-F590B5155451" );
            RockMigrationHelper.UpdateBlockType( "Group Member History", "Displays a timeline of history for a group member", "~/Blocks/Groups/GroupMemberHistory.ascx", "Groups", "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226" );
            RockMigrationHelper.UpdateBlockType( "Internal Communication View", "Block for showing the contents of internal content channels.", "~/Blocks/Utility/InternalCommunicationView.ascx", "Utility", "01CBF2F9-A905-47F7-A153-CF593112C59B" );
           
            // Attrib for BlockType: Person Group History:Group Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "F8E351BC-607E-4897-B732-F590B5155451", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types", "GroupTypes", "", @"List of Group Types that this block defaults to, and the user is able to choose from in the options filter. Leave blank to include all group types that have history enabled.", 1, @"", "82FA5003-05E8-4E22-8E5F-ED841DF4D9CB" );
            // Attrib for BlockType: Group Member History:Group Member History Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member History Page", "GroupMemberHistoryPage", "", @"", 3, @"", "A1D22BA4-4D39-4187-9F6B-C0B8DC6D6896" );
            // Attrib for BlockType: Group Member History:Timeline Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Timeline Lava Template", "TimelineLavaTemplate", "", @"The Lava Template to use when rendering the timeline view of the history.", 1, @"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}", "2928385B-09D9-4877-A35C-2A688F22DB22" );
            // Attrib for BlockType: Group Member History:Group History Grid Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group History Grid Page", "GroupHistoryGridPage", "", @"", 2, @"", "B72E26E2-5EC5-49BE-829B-18FB9AE12E47" );
            // Attrib for BlockType: Internal Communication View:Block Title Icon CSS Class
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CBF2F9-A905-47F7-A153-CF593112C59B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCssClass", "", @"The icon CSS class for use in the block title.", 1, @"fa fa-newspaper", "F2C0131A-D7A8-4EDE-99BE-26D069130524" );
            // Attrib for BlockType: Internal Communication View:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CBF2F9-A905-47F7-A153-CF593112C59B", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be made available to the block.", 6, @"", "9EA00E9F-672A-4D12-A060-32108E6BF2A7" );
            // Attrib for BlockType: Internal Communication View:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CBF2F9-A905-47F7-A153-CF593112C59B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"The time, in seconds, to cache the data for this block. The Lava template will still be run to enable personalization. Only the data for the block will be cached.", 7, @"3600", "09BE0433-2066-405E-88DF-12590588662F" );
            // Attrib for BlockType: Internal Communication View:Body Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CBF2F9-A905-47F7-A153-CF593112C59B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body Template", "BodyTemplate", "", @"The Lava template for rendering the body of the block.", 5, @"d", "311754BC-DD6E-4352-8F90-E7AFAD42EACD" );
            // Attrib for BlockType: Internal Communication View:Block Title Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CBF2F9-A905-47F7-A153-CF593112C59B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Block Title Template", "BlockTitleTemplate", "", @"Lava template for determining the title of the block.", 0, @"Staff Updates <small>({{ Item.StartDateTime | Date:'sd' }})</small>", "B6B6EF33-D149-457D-A813-497030C8A312" );
            // Attrib for BlockType: Internal Communication View:Content Channel
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CBF2F9-A905-47F7-A153-CF593112C59B", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "", @"The content channel to display with the template. The contant channel must be of type 'Internal Communication Template'.", 2, @"", "1896C946-6E2E-4194-BA2F-9C789BE89650" );
            // Attrib for BlockType: Internal Communication View:Metrics
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CBF2F9-A905-47F7-A153-CF593112C59B", "F5334A8E-B7E2-415C-A6EC-A6D8FA5341C4", "Metrics", "Metrics", "", @"Select the metrics you would like to display on the page.", 3, @"", "BD12CA1A-D11A-4B66-BB7D-E8570FD04791" );
            // Attrib for BlockType: Internal Communication View:Metric Value Count
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CBF2F9-A905-47F7-A153-CF593112C59B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Metric Value Count", "MetricValueCount", "", @"The number of metric values to return per metric. You will always get the lastest value, but if you would like to return additional values (i.e. to create a chart) you can specify that here.", 4, @"0", "B9DF3F56-55CC-452D-881E-ED41B3A93C88" );
            // Attrib for BlockType: Business Detail:Communication Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Communication Page", "CommunicationPage", "", @"The communication page to use for when the business email address is clicked. Leave this blank to use the default.", 1, @"", "DD53A793-4528-4530-93A6-AA200DA920D9" );
            // Attrib for BlockType: Transaction Detail:Location Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Location Types", "LocationTypes", "", @"The type of location type to display for person (if none are selected all addresses will be included ).", 5, @"", "8427B20D-010E-45BE-AB6E-451EC06F060E" );
            // Attrib for BlockType: Group History:Group Member History Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "E916D65E-5D30-4086-9A11-8E891CCD930E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member History Page", "GroupMemberHistoryPage", "", @"", 3, @"", "8CC92361-B724-4296-B7D0-75B5D33F66C9" );
            // Attrib for BlockType: Group History:Group History Grid Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "E916D65E-5D30-4086-9A11-8E891CCD930E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group History Grid Page", "GroupHistoryGridPage", "", @"", 2, @"", "6A7F08FE-256E-4FEA-886E-2545BF64887D" );
            // Attrib for BlockType: Group List:Allow Add
            RockMigrationHelper.UpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Add", "AllowAdd", "", @"Should block support adding new group?", 15, @"True", "FD470B89-C053-411E-BB22-C064E2C15E43" );
            // Attrib for BlockType: Report Detail:Report
            RockMigrationHelper.UpdateBlockTypeAttribute( "E431DBDF-5C65-45DC-ADC5-157A02045CCD", "B7FA826C-3367-4BF2-90E5-8C6730079D82", "Report", "Report", "", @"Select the report to present to the user.", 2, @"", "6CE915E3-555D-4F4E-8C79-17C6E2433F6B" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Report Detail:Report
            RockMigrationHelper.DeleteAttribute( "6CE915E3-555D-4F4E-8C79-17C6E2433F6B" );
            // Attrib for BlockType: Group List:Allow Add
            RockMigrationHelper.DeleteAttribute( "FD470B89-C053-411E-BB22-C064E2C15E43" );
            // Attrib for BlockType: Group History:Group History Grid Page
            RockMigrationHelper.DeleteAttribute( "6A7F08FE-256E-4FEA-886E-2545BF64887D" );
            // Attrib for BlockType: Group History:Group Member History Page
            RockMigrationHelper.DeleteAttribute( "8CC92361-B724-4296-B7D0-75B5D33F66C9" );
            // Attrib for BlockType: Transaction Detail:Location Types
            RockMigrationHelper.DeleteAttribute( "8427B20D-010E-45BE-AB6E-451EC06F060E" );
            // Attrib for BlockType: Business Detail:Communication Page
            RockMigrationHelper.DeleteAttribute( "DD53A793-4528-4530-93A6-AA200DA920D9" );
            // Attrib for BlockType: Internal Communication View:Metric Value Count
            RockMigrationHelper.DeleteAttribute( "B9DF3F56-55CC-452D-881E-ED41B3A93C88" );
            // Attrib for BlockType: Internal Communication View:Metrics
            RockMigrationHelper.DeleteAttribute( "BD12CA1A-D11A-4B66-BB7D-E8570FD04791" );
            // Attrib for BlockType: Internal Communication View:Content Channel
            RockMigrationHelper.DeleteAttribute( "1896C946-6E2E-4194-BA2F-9C789BE89650" );
            // Attrib for BlockType: Internal Communication View:Block Title Template
            RockMigrationHelper.DeleteAttribute( "B6B6EF33-D149-457D-A813-497030C8A312" );
            // Attrib for BlockType: Internal Communication View:Body Template
            RockMigrationHelper.DeleteAttribute( "311754BC-DD6E-4352-8F90-E7AFAD42EACD" );
            // Attrib for BlockType: Internal Communication View:Cache Duration
            RockMigrationHelper.DeleteAttribute( "09BE0433-2066-405E-88DF-12590588662F" );
            // Attrib for BlockType: Internal Communication View:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "9EA00E9F-672A-4D12-A060-32108E6BF2A7" );
            // Attrib for BlockType: Internal Communication View:Block Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute( "F2C0131A-D7A8-4EDE-99BE-26D069130524" );
            // Attrib for BlockType: Group Member History:Group History Grid Page
            RockMigrationHelper.DeleteAttribute( "B72E26E2-5EC5-49BE-829B-18FB9AE12E47" );
            // Attrib for BlockType: Group Member History:Timeline Lava Template
            RockMigrationHelper.DeleteAttribute( "2928385B-09D9-4877-A35C-2A688F22DB22" );
            // Attrib for BlockType: Group Member History:Group Member History Page
            RockMigrationHelper.DeleteAttribute( "A1D22BA4-4D39-4187-9F6B-C0B8DC6D6896" );
            // Attrib for BlockType: Person Group History:Group Types
            RockMigrationHelper.DeleteAttribute( "82FA5003-05E8-4E22-8E5F-ED841DF4D9CB" );

            RockMigrationHelper.DeleteBlockType( "01CBF2F9-A905-47F7-A153-CF593112C59B" ); // Internal Communication View
            RockMigrationHelper.DeleteBlockType( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226" ); // Group Member History
            RockMigrationHelper.DeleteBlockType( "F8E351BC-607E-4897-B732-F590B5155451" ); // Person Group History
        }
    }
}
