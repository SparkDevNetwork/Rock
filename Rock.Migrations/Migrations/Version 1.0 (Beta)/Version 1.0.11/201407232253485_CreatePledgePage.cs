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
    public partial class CreatePledgePage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            
            //// CreatePledge Pages and Blocks

            RockMigrationHelper.AddPage( "8BB303AF-743C-49DC-A7FF-CC1236B4B1D9", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Pledge", "", "A974A965-414B-47A6-9CC1-D3A175DA965B", "" ); // Site:Rock Solid Church
            Sql( "UPDATE [Page] set [IsSystem]=0 where [Guid] = 'A974A965-414B-47A6-9CC1-D3A175DA965B'" );

            // Add Block to Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.AddBlock( "A974A965-414B-47A6-9CC1-D3A175DA965B", "", "20B5568E-A010-4E15-9127-E63CF218D6E5", "Create Pledge", "Main", "", "", 0, "C6007437-A565-4144-9DB3-DD590D62D5E2" );

            // Attrib for BlockType: Create Pledge:Confirmation Email Template
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "CE7CA048-551C-4F68-8C0A-FCDCBEB5B956", "Confirmation Email Template", "ConfirmationEmailTemplate", "", "Email template to use after submitting a new pledge. Leave blank to not send an email.", 10, @"", "B8F49AA3-3713-4506-BFFC-7E15646D6395" );

            // Attrib for BlockType: Create Pledge:Confirmation Email Template
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "CE7CA048-551C-4F68-8C0A-FCDCBEB5B956", "Confirmation Email Template", "ConfirmationEmailTemplate", "", "Email template to use after submitting a new pledge. Leave blank to not send an email.", 10, @"", "B8F49AA3-3713-4506-BFFC-7E15646D6395" );

            // Attrib for BlockType: Create Pledge:Account
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A", "Account", "Account", "", "The account that new pledges will be allocated toward", 2, @"", "13601B14-C4FA-452D-96A9-08F4103FED0E" );

            // Attrib for BlockType: Create Pledge:Pledge Date Range
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "9C7D431C-875C-4792-9E76-93F3A32BB850", "Pledge Date Range", "PledgeDateRange", "", "Date range of the pledge.", 4, @",", "4C35C9D1-F008-43DD-B04C-C12D25F0DE62" );

            // Attrib for BlockType: Create Pledge:Receipt Text
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Receipt Text", "ReceiptText", "", "The text (or html) to display as the pledge receipt", 9, @"
<h1>Thank You!</h1>
<p>
{{Person.NickName}}, thank you for your commitment of ${{FinancialPledge.TotalAmount}} to {{Account.Name}}.  To make your commitment even easier, you might consider making a scheduled giving profile.
</p>
<p>
    <a href='~/page/186?PledgeId={{ FinancialPledge.Id  }}' class='btn btn-default' >Setup a Giving Profile</a>
</p>
", "CA69B385-696E-4BDB-8091-4B3956C7E5EA" );

            // Attrib for BlockType: Create Pledge:Save Button Text
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Save Button Text", "SaveButtonText", "", "The Text to shown on the Save button", 7, @"Save", "0BB9E8A4-8709-4920-A0B2-A43A27BB2A89" );

            // Attrib for BlockType: Create Pledge:Note Message
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Message", "NoteMessage", "", "Message to show at the bottom of the create pledge block.", 8, @"Note: This commitment is a statement of intent and may be changed as the circumstances change.", "4CAB6DCC-E58C-46A1-8C14-32CEDC378EE9" );

            // Attrib for BlockType: Create Pledge:Show Pledge Frequency
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Pledge Frequency", "ShowPledgeFrequency", "", "Show the pledge frequency option to the user.", 5, @"false", "FB060774-CEA8-4EF6-B4F9-AB82FEB2B5FA" );

            // Attrib for BlockType: Create Pledge:Enable Smart Names
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Smart Names", "EnableSmartNames", "", "Check the first name for 'and' and '&' and split it to just use the first name provided.", 1, @"True", "075F7965-7FB5-4EE5-98ED-BC455F68A0EF" );

            // Attrib for BlockType: Create Pledge:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Outputs the object graph to help create your liquid syntax.", 11, @"False", "75E1BFA0-29E3-4629-BC1C-46EFD193C111" );

            // Attrib for BlockType: Create Pledge:Require Pledge Frequency
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Pledge Frequency", "RequirePledgeFrequency", "", "Require that a user select a specific pledge frequency (when pledge frequency is shown)", 6, @"false", "119381CC-263B-4AC2-BD4E-4753EAAAFDE2" );

            // Attrib for BlockType: Create Pledge:New Connection Status
            RockMigrationHelper.AddBlockTypeAttribute( "20B5568E-A010-4E15-9127-E63CF218D6E5", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "New Connection Status", "NewConnectionStatus", "", "Person connection status to assign to a new user.", 3, @"8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061", "14565642-0367-4284-BC34-BD57F80381C8" );


            //// ** other misc migration catchup **

            RockMigrationHelper.UpdateBlockType( "REST Action Detail", "Detail block for a REST Action that can be used to test the REST action.", "~/Blocks/Core/RestActionDetail.ascx", "Core", "5BB83A28-CED2-4B40-9FDA-9C3D21FD6A83" );

            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "3BE1D982-97FA-47E6-9F29-E746A6F6A68A" );

            // Attrib for BlockType: Metric Detail:Chart Date Range
            RockMigrationHelper.AddBlockTypeAttribute( "D77341B9-BA38-4693-884E-E5C1D908CEC4", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Chart Date Range", "SlidingDateRange", "", "", 0, @"-1||||", "97C6CA5D-71FD-46EC-BEE2-9F9D19B86E66" );

            // Attrib for BlockType: Metric Detail:Chart Style
            RockMigrationHelper.AddBlockTypeAttribute( "D77341B9-BA38-4693-884E-E5C1D908CEC4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", "", 0, @"2ABB2EA0-B551-476C-8F6B-478CD08C2227", "19ABE46E-3E42-4FFE-B076-FC4A83CCBD79" );

            // Attrib for BlockType: Metric Detail:Show Chart
            RockMigrationHelper.AddBlockTypeAttribute( "D77341B9-BA38-4693-884E-E5C1D908CEC4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Chart", "ShowChart", "", "", 0, @"true", "9DD32FA1-4BAB-41F5-AB39-4204FF906BAF" );

            // Attrib for BlockType: Metric Detail:Combine Chart Series
            RockMigrationHelper.AddBlockTypeAttribute( "D77341B9-BA38-4693-884E-E5C1D908CEC4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Combine Chart Series", "CombineChartSeries", "", "", 0, @"False", "CD9287D4-E7F0-4688-8800-8DE95B23E24B" );

            // Attrib for BlockType: Pie Chart:Series Partition
            RockMigrationHelper.AddBlockTypeAttribute( "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D", "B50968BD-7643-4288-9237-6E89D2065363", "Series Partition", "Entity", "", "Select the series partition entity (Campus, Group, etc) to be used to limit the metric values for the selected metrics.", 4, @"", "1E6A7658-123C-4BD0-A67E-4F2D08E0F490" );

            // Attrib Value for Block:Metric Detail, Attribute:Chart Style Page: Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F85FE71D-927D-45AF-B419-02A8909C6E72", "19ABE46E-3E42-4FFE-B076-FC4A83CCBD79", @"2abb2ea0-b551-476c-8f6b-478cd08c2227" );

            // Attrib Value for Block:Metric Detail, Attribute:Show Chart Page: Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F85FE71D-927D-45AF-B419-02A8909C6E72", "9DD32FA1-4BAB-41F5-AB39-4204FF906BAF", @"True" );

            // Attrib Value for Block:Metric Detail, Attribute:Chart Date Range Page: Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F85FE71D-927D-45AF-B419-02A8909C6E72", "97C6CA5D-71FD-46EC-BEE2-9F9D19B86E66", @"All||||" );

            // Attrib Value for Block:Metric Detail, Attribute:Combine Chart Series Page: Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F85FE71D-927D-45AF-B419-02A8909C6E72", "CD9287D4-E7F0-4688-8800-8DE95B23E24B", @"True" );

            RockMigrationHelper.UpdateFieldType( "Currency", "", "Rock", "Rock.Field.Types.CurrencyFieldType", "3EE69CBC-35CE-4496-88CC-8327A447603F" );

            // more catchup migrations...
            RockMigrationHelper.UpdateBlockType( "Line Chart", "Line Chart Dashboard Widget", "~/Blocks/Reporting/Dashboard/LineChartDashboardWidget.ascx", "Reporting > Dashboard", "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1" );
            RockMigrationHelper.UpdateBlockType( "Bar Chart", "Bar Chart Dashboard Widget", "~/Blocks/Reporting/Dashboard/BarChartDashboardWidget.ascx", "Reporting > Dashboard", "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD" );
            RockMigrationHelper.UpdateBlockType( "Pie Chart", "Pie Chart Dashboard Widget", "~/Blocks/Reporting/Dashboard/PieChartDashboardWidget.ascx", "Reporting > Dashboard", "341AAD88-47E0-4F25-B4F2-0EBCE5A96A1D" );
            RockMigrationHelper.UpdateBlockType( "Liquid Dashboard Widget", "Dashboard Widget from Liquid using YTD metric values", "~/Blocks/Reporting/Dashboard/LiquidDashboardWidget.ascx", "Reporting > Dashboard", "AC19A4F3-2E88-487E-8E88-377C1C20DBD5" );

            RockMigrationHelper.UpdateBlockType( "Metric Value Detail", "Displays the details of a particular metric value.", "~/Blocks/Reporting/MetricValueDetail.ascx", "Reporting", "508DA252-F94C-4641-8579-458D8FCE14B2" );

            // Add icon to page view page 
            Sql( @"UPDATE [Page] SET [IconCssClass] = 'fa fa-desktop' WHERE [Guid] = '82E9CDDB-A60E-4C0E-9306-C07BEAAD5F70'" );

            // Remove the old Stark block   
            Sql( @"DELETE FROM [BlockType] WHERE [Guid] = 'EF4DE627-DD03-4EBA-990B-F2F4CC754548'" );

            // Fix type in group type name   
            Sql( @"UPDATE [GroupType] SET [Name] = 'Check in by Ability Level' WHERE [Guid] = '13A6139D-EEEC-412D-8572-773ECA1939CC'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Create Pledge:Confirmation Email Template
            RockMigrationHelper.DeleteAttribute( "B8F49AA3-3713-4506-BFFC-7E15646D6395" );
            // Attrib for BlockType: Create Pledge:Note Message
            RockMigrationHelper.DeleteAttribute( "4CAB6DCC-E58C-46A1-8C14-32CEDC378EE9" );
            // Attrib for BlockType: Create Pledge:Save Button Text
            RockMigrationHelper.DeleteAttribute( "0BB9E8A4-8709-4920-A0B2-A43A27BB2A89" );
            // Attrib for BlockType: Create Pledge:Require Pledge Frequency
            RockMigrationHelper.DeleteAttribute( "119381CC-263B-4AC2-BD4E-4753EAAAFDE2" );
            // Attrib for BlockType: Create Pledge:Enable Debug
            RockMigrationHelper.DeleteAttribute( "75E1BFA0-29E3-4629-BC1C-46EFD193C111" );
            // Attrib for BlockType: Create Pledge:Receipt Text
            RockMigrationHelper.DeleteAttribute( "CA69B385-696E-4BDB-8091-4B3956C7E5EA" );
            // Attrib for BlockType: Create Pledge:Pledge Date Range
            RockMigrationHelper.DeleteAttribute( "4C35C9D1-F008-43DD-B04C-C12D25F0DE62" );
            // Attrib for BlockType: Create Pledge:New Connection Status
            RockMigrationHelper.DeleteAttribute( "14565642-0367-4284-BC34-BD57F80381C8" );
            // Attrib for BlockType: Create Pledge:Enable Smart Names
            RockMigrationHelper.DeleteAttribute( "075F7965-7FB5-4EE5-98ED-BC455F68A0EF" );
            // Attrib for BlockType: Create Pledge:Show Pledge Frequency
            RockMigrationHelper.DeleteAttribute( "FB060774-CEA8-4EF6-B4F9-AB82FEB2B5FA" );
            // Attrib for BlockType: Create Pledge:Account
            RockMigrationHelper.DeleteAttribute( "13601B14-C4FA-452D-96A9-08F4103FED0E" );
            
            // Remove Block: Create Pledge, from Page: Pledge, Site: Rock Solid Church
            RockMigrationHelper.DeleteBlock( "C6007437-A565-4144-9DB3-DD590D62D5E2" );
            
            RockMigrationHelper.DeletePage( "A974A965-414B-47A6-9CC1-D3A175DA965B" ); //  Page: Pledge, Layout: LeftSidebar, Site: Rock Solid Church
        }
            
    }
}
