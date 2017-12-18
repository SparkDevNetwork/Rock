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
    public partial class InteractionTemplateAndCampaignFields : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.InteractionComponent", "ComponentSummary", c => c.String());
            AddColumn("dbo.InteractionComponent", "ListTemplate", c => c.String());
            AddColumn("dbo.InteractionComponent", "DetailTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "ListTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "DetailTemplate", c => c.String());
            AddColumn("dbo.InteractionChannel", "UsesSession", c => c.Boolean(nullable: false));
            AddColumn("dbo.Interaction", "ListTemplate", c => c.String());
            AddColumn("dbo.Interaction", "DetailTemplate", c => c.String());
            AddColumn("dbo.Interaction", "Source", c => c.String(maxLength: 25));
            AddColumn("dbo.Interaction", "Medium", c => c.String(maxLength: 25));
            AddColumn("dbo.Interaction", "Campaign", c => c.String(maxLength: 50));
            AddColumn("dbo.Interaction", "Content", c => c.String(maxLength: 50));
            AddColumn("dbo.InteractionSession", "ListTemplate", c => c.String());
            AddColumn("dbo.InteractionSession", "DetailTemplate", c => c.String());


            // NA: For new GroupGetChildGroupForCampus workflow action
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.GroupGetChildGroupForCampus", "Group Get Child Group for Campus", "Rock.Workflow.Action.GroupGetChildGroupForCampus, Rock, Version=1.7.0.0, Culture=neutral, PublicKeyToken=null", false, true, "17B99656-BB9E-4B08-A7B1-CC66258AC08B" );

            // MP: Control Gallery to CMS Page
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Control Gallery", "", "706C0584-285F-4014-BA61-EC42C8F6F76B", "fa fa-magic" ); // Site:Rock RMS
            
            // Add Block to Page: Control Gallery, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "706C0584-285F-4014-BA61-EC42C8F6F76B", "", "55468258-18B9-4FAE-90E8-F173F7704E23", "Rock Control Gallery", "Main", @"", @"", 0, "AC070544-072B-413E-8C0E-83583A52DD22" );

            // MP: Block Attributes Round-up
            // Attrib for BlockType: Attributes:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "11F74455-F71D-45C7-806B-0DB463D34DAB" );
            // Attrib for BlockType: Components:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "21F5F466-59BC-40B2-8D73-7314D936C3CB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "11507A9C-6ECB-4C7C-87AA-017512EE23D3" );
            // Attrib for BlockType: Group Tree View:Display Inactive Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Inactive Campuses", "DisplayInactiveCampuses", "", @"Include inactive campuses in the Campus Filter", 0, @"True", "22D5915F-D449-4E03-A8AD-0C473A3D4864" );
            // Attrib for BlockType: Campus List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "C93D614A-6EBC-49A1-A80D-F3677D2B86A0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "266E874D-8E0E-4551-9533-020ACCD6D4F4" );
            // Attrib for BlockType: Scheduled Job List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "6D3F924E-BDD0-4C78-981E-B698351E75AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "EACBA04F-4E83-4253-8496-5949DD934F2B" );
            // Attrib for BlockType: Site List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "CCB6961B-7F92-4CDD-B099-4CD34B4BBB20" );
            // Attrib for BlockType: Group Type List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "80306BB1-FE4B-436F-AC7A-691CF0BC0F5E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "61F59C8B-2BB5-4874-BC3A-B5FBC958EBF7" );
            // Attrib for BlockType: Defined Type List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "5470C9C4-09C1-439F-AA56-3524047497EE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "A496EB13-4DCE-474A-AE9D-A13373E2677F" );
            // Attrib for BlockType: Route List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "E92E3C51-EB14-414D-BC68-9061FEB92A22", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "DFB9E7CE-5D62-4A84-93A4-21C9B1A2D461" );
            // Attrib for BlockType: Workflow Type Detail:Default No Action Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1FF677D-5E52-4259-90C7-5560ECBBD82B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default No Action Message", "DefaultNoActionMessage", "", @"The default No Action Message.", 2, @"
This {{ Workflow.WorkflowType.WorkTerm }} does not currently require your attention.", "7061D243-389E-48C0-A23B-6D1BC1FA3187" );
            // Attrib for BlockType: Workflow Type Detail:Default Summary View Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "E1FF677D-5E52-4259-90C7-5560ECBBD82B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Summary View Text", "DefaultSummaryViewText", "", @"The default Summary View Text.", 3, @"
<div class='row'>
    <div class='col-sm-6'>
        <dl><dt>Started By</dt><dd>{{ Workflow.InitiatorPersonAlias.Person.FullName }}</dd></dl>
    </div>
    <div class='col-sm-6'>
        <dl><dt>Started On</dt><dd>{{ Workflow.ActivatedDateTime | Date:'MM/dd/yyyy' }} at {{ Workflow.ActivatedDateTime | Date:'hh:mm:ss tt' }}</dd></dl>
    </div>
</div>

{% assign attributeList = '' %}
{% for attribute in Workflow.AttributeValues %}
    {% if attribute.AttributeIsGridColumn %}
        {% assign attributeValue = attribute.ValueFormatted %}
        {% if attributeValue != '' %}
            {% capture item %}<dt>{{ attribute.AttributeName }}</dt><dd>{{ attributeValue }}</dd>{% endcapture %}
            {% assign attributeList = attributeList | Append:item %}
        {% endif %}
    {% endif %}
{% endfor %}

{% if attributeList != '' %}
    <div class='row'>
        <div class='col-sm-6'>
            <dl>
                {{ attributeList }}
            </dl>
        </div>
    </div>
{% endif %}", "A0358E81-742F-4EEB-A156-BCDDEBD5E18D" );
            // Attrib for BlockType: Workflow List:Default WorkflowType
            RockMigrationHelper.UpdateBlockTypeAttribute( "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Default WorkflowType", "DefaultWorkflowType", "", @"The default workflow type to use. If provided the query string will be ignored.", 0, @"", "3CB12BF1-4B86-44D4-8E13-721FE0F98116" );
            // Attrib for BlockType: Workflow List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "84B029A1-E1AD-4064-B1D2-C837C21BC5EF" );
            // Attrib for BlockType: Group Member List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "1C48D6AB-AD5F-4A48-B534-1699B7CB3481" );
            // Attrib for BlockType: Block Type List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "78A31D91-61F6-42C3-BB7D-676EDC72F35F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "4BEFDCFA-BF1D-4DF8-818E-2B5E92F00FFB" );
            // Attrib for BlockType: Prayer Request List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "C90634D6-16F5-4741-95DB-E4B154979FFF" );
            // Attrib for BlockType: Prayer Request List:Show 'Approved' column
            RockMigrationHelper.UpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show 'Approved' column", "ShowApprovedColumn", "", @"If enabled, the Approved column will be shown with a Yes/No toggle button.", 3, @"True", "96DDF1FA-0345-4E54-AC02-95D5B4577471" );
            // Attrib for BlockType: Prayer Request List:Show Grid Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid Filter", "ShowGridFilter", "", @"If enabled, the grid filter will be visible.", 4, @"True", "210646DF-0588-489D-BA46-538754EC9D1F" );
            // Attrib for BlockType: Prayer Request Detail:Default Allow Comments Checked
            RockMigrationHelper.UpdateBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Allow Comments Checked", "DefaultAllowCommentsChecked", "", @"If true, the Allow Comments checkbox will be pre-checked for all new requests by default.", 5, @"True", "69294BC4-B5C4-4C9C-849E-2A262359B458" );
            // Attrib for BlockType: Prayer Request Detail:Require Last Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "", @"Require that a last name be entered", 3, @"True", "76831910-9FCE-42C6-BF18-DCD3F0B370BE" );
            // Attrib for BlockType: Prayer Request Detail:Default To Public
            RockMigrationHelper.UpdateBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "", @"If enabled, all prayers will be set to public by default", 4, @"False", "239797F6-6776-4E08-9962-04FD24E26799" );
            // Attrib for BlockType: Prayer Request Detail:Set Current Person To Requester
            RockMigrationHelper.UpdateBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Current Person To Requester", "SetCurrentPersonToRequester", "", @"Will set the current person as the requester. This is useful in self-entry situiations.", 2, @"False", "46CCEA26-04B0-4458-9725-D60C32FC7E81" );
            // Attrib for BlockType: Prayer Comment List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "EF7FDFFD-058A-401D-BA83-280FC74BACC7" );
            // Attrib for BlockType: Dynamic Data:Communication Recipient Person Id Columns
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Communication Recipient Person Id Columns", "CommunicationRecipientPersonIdColumns", "", @"Columns that contain a communication recipient person id.", 0, @"", "75DDB977-9E71-44E8-924B-27134659D3A4" );
            // Attrib for BlockType: Dynamic Data:Show Excel Export
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Excel Export", "ShowExcelExport", "", @"Show Export to Excel button in grid footer?", 0, @"True", "E11B57E5-EC7D-4C42-9ADA-37594D71F145" );
            // Attrib for BlockType: Dynamic Data:Show Communicate
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Communicate", "ShowCommunicate", "", @"Show Communicate button in grid footer?", 0, @"True", "5B2C115A-C187-4AB3-93AE-7010644B39DA" );
            // Attrib for BlockType: Dynamic Data:Show Merge Person
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Merge Person", "ShowMergePerson", "", @"Show Merge Person button in grid footer?", 0, @"True", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E" );
            // Attrib for BlockType: Dynamic Data:Show Bulk Update
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Bulk Update", "ShowBulkUpdate", "", @"Show Bulk Update button in grid footer?", 0, @"True", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78" );
            // Attrib for BlockType: Dynamic Data:Stored Procedure
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Stored Procedure", "StoredProcedure", "", @"Is the query a stored procedure?", 0, @"False", "A4439703-5432-489A-9C14-155903D6A43E" );
            // Attrib for BlockType: Dynamic Data:Show Merge Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Merge Template", "ShowMergeTemplate", "", @"Show Export to Merge Template button in grid footer?", 0, @"True", "6697B0A2-C8FE-497A-B5B4-A9D459474338" );
            // Attrib for BlockType: Dynamic Data:Paneled Grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Paneled Grid", "PaneledGrid", "", @"Add the 'grid-panel' class to the grid to allow it to fit nicely in a block.", 0, @"False", "5449CB61-2DFC-4B55-A697-38F1C2AF128B" );
            // Attrib for BlockType: Dynamic Data:Show Grid Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid Filter", "ShowGridFilter", "", @"Show filtering controls that are dynamically generated to match the columns of the dynamic data.", 0, @"True", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C" );
            // Attrib for BlockType: Dynamic Data:Timeout
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Timeout", "Timeout", "", @"The amount of time in xxx to allow the query to run before timing out.", 0, @"30", "BEEE38DD-2791-4242-84B6-0495904143CC" );
            // Attrib for BlockType: Dynamic Data:Page Title Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Page Title Lava", "PageTitleLava", "", @"Optional Lava for setting the page title. If nothing is provided then the page's title will be used.", 0, @"", "3F4BA170-F5C5-405E-976F-0AFBB8855FE8" );
            // Attrib for BlockType: Sql Command:Database Timeout
            RockMigrationHelper.UpdateBlockTypeAttribute( "89EAFE90-7082-4FF2-BC87-F50BFDB53298", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeout", "", @"The number of seconds to wait before reporting a database timeout.", 1, @"180", "F59F2CE0-B7DF-4617-9338-12D9486ED015" );
            // Attrib for BlockType: Binary File Type List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "0926B82C-CBA2-4943-962E-F788C8A80037", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "894D8F86-981A-474E-8E1F-0C49E66F5B27" );
            // Attrib for BlockType: Workflow Trigger List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "72F48121-2CE2-4696-840C-CF404EAF7EEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "679C261A-6421-4484-8D5C-E873F86C41C9" );
            // Attrib for BlockType: Device List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "32183AD6-01CB-4533-858B-1BDA5120AAD5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "10DD61A8-6E5B-470E-8303-E2EABF3439E6" );
            // Attrib for BlockType: Binary File List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "26541C8A-9E54-4723-A739-21FAA5191014", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "0C92C761-205D-4736-A8F5-F261DAF3136D" );
            // Attrib for BlockType: Pledge List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "3FB05889-8745-4BFE-AC1D-0D6664C3EC0C" );
            // Attrib for BlockType: Pledge List:Accounts
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "", @"Limit the results to pledges that match the selected accounts.", 5, @"", "AC41295E-292E-4D5F-BEB0-03BC283AE9D7" );
            // Attrib for BlockType: Batch List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "7268568A-3F3D-4089-B009-6F291B33A2CE" );
            // Attrib for BlockType: Batch List:Show Accounts Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Accounts Column", "ShowAccountsColumn", "", @"Should the accounts column be displayed.", 2, @"True", "1DE5F52F-B6F6-4D7B-BDA7-FB290479CD82" );
            // Attrib for BlockType: Batch Detail:Batch Names
            RockMigrationHelper.UpdateBlockTypeAttribute( "CE34CE43-2CCF-4568-9AEB-3BE203DB3470", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Batch Names", "BatchNames", "", @"The Defined Type that contains a predefined list of batch names to choose from instead of entering it in manually when adding a new batch. Leave this blank to hide this option and let them edit the batch name manually.", 3, @"", "F08F17BB-C538-437B-A5C8-838576C1AFEE" );
            // Attrib for BlockType: Transaction Detail:Carry Over Account
            RockMigrationHelper.UpdateBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Carry Over Account", "CarryOverAccount", "", @"Keep Last Used Account when adding multiple transactions in the same session.", 4, @"True", "52431D4B-EA02-4D70-8B3F-74E1AD8EB3D8" );
            // Attrib for BlockType: Transaction List:Show Foreign Key
            RockMigrationHelper.UpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Foreign Key", "ShowForeignKey", "", @"Should the transaction foreign key column be displayed?", 8, @"False", "58288BD2-C9FB-4A25-B41E-24C7416C8C6F" );
            // Attrib for BlockType: Transaction List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "2D53F6D0-6654-4ED9-9BEE-41E01337A9F8" );
            // Attrib for BlockType: Transaction List:Accounts
            RockMigrationHelper.UpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "", @"Limit the results to transactions that match the selected accounts.", 10, @"", "95621093-9BBF-4467-9828-4456D5E01E1D" );
            // Attrib for BlockType: Communication List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "56ABBD0F-8F62-4094-88B3-161E71F21419", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "806C9F7E-0134-4A7A-AE92-C3D8E7651E4A" );
            // Attrib for BlockType: Exception List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "8F5D62CE-0BFD-4B9B-BE54-06E2369ACDDF" );
            // Attrib for BlockType: Schedule List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "9C11A672-FBD5-4E05-8BD1-D0510579ADD4" );
            // Attrib for BlockType: Scheduled Transaction List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "9CA1C00E-99EF-44F0-B42C-ABB279CB1406" );
            // Attrib for BlockType: Scheduled Transaction List:Accounts
            RockMigrationHelper.UpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "", @"Limit the results to scheduled transactions that match the selected accounts.", 2, @"", "7BF9BC78-E901-419F-8BE0-F76159E02990" );
            // Attrib for BlockType: Add Group:Person Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Detail Page", "PersonDetailPage", "", @"The Page to navigate to after the family has been added. (Note that {GroupId} and {PersonId} can be included in the route). Leave blank to go to the default page of ~/Person/{PersonId}.", 28, @"", "3BDDD7AC-561F-40BA-B9CC-76832CB6753C" );
            // Attrib for BlockType: Add Group:Person Workflow(s)
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Person Workflow(s)", "PersonWorkflows", "", @"The workflow(s) to launch for every person added.", 24, @"", "22907727-A1BB-41CA-8033-54C44C97C845" );
            // Attrib for BlockType: Add Group:Adult Workflow(s)
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Adult Workflow(s)", "AdultWorkflows", "", @"When Family group type, the workflow(s) to launch for every adult added.", 25, @"", "FBE9A69E-A892-46AF-B256-02A89E5EA977" );
            // Attrib for BlockType: Add Group:Child Workflow(s)
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Child Workflow(s)", "ChildWorkflows", "", @"When Family group type, the workflow(s) to launch for every child added.", 26, @"", "2739AE6D-47F4-47C7-A0DF-987B0F26349E" );
            // Attrib for BlockType: Add Group:Group Workflow(s)
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Group Workflow(s)", "GroupWorkflows", "", @"The workflow(s) to launch for the group (family) that is added.", 27, @"", "69CE52E8-27DE-4BAD-AD72-F5133FC20136" );
            // Attrib for BlockType: Add Group:Show Middle Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Middle Name", "ShowMiddleName", "", @"Show an edit box for Middle Name.", 4, @"False", "75736E9E-CD6E-41C1-B1E6-68881A97DABC" );
            // Attrib for BlockType: Add Group:Enable Common Last Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Common Last Name", "EnableCommonLastName", "", @"Autofills the last name field when adding a new group member with the last name of the first group member.", 5, @"True", "7ED4EC12-1329-407D-BA36-35473B06F740" );
            // Attrib for BlockType: Add Group:Show Suffix
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Suffix", "ShowSuffix", "", @"Show person suffix.", 7, @"True", "0D0DFBDA-898F-458D-88D0-329812DA935A" );
            // Attrib for BlockType: Add Group:Birth Date
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Birth Date", "BirthDate", "", @"Should a Birthdate be required for each person added?", 9, @"False", "E903B142-D39E-4E27-B353-40C19ECA29F0" );
            // Attrib for BlockType: Add Group:Child Birthdate
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Child Birthdate", "ChildBirthdate", "", @"When Family group type, should Birthdate be required for each child added?", 10, @"False", "361D1414-97AF-488B-A1C7-DF7FCEEF725A" );
            // Attrib for BlockType: Add Group:Show Inactive Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Inactive Campuses", "ShowInactiveCampuses", "", @"Determines if inactive campuses should be shown.", 12, @"True", "8775BCBA-0144-4162-B328-77071F3EDEB6" );
            // Attrib for BlockType: Add Group:Require Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "", @"Determines if a campus is required.", 13, @"True", "22D269DE-2129-4AA3-92DE-C3F904FFE6E1" );
            // Attrib for BlockType: Add Group:Show Cell Phone Number First
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Cell Phone Number First", "ShowCellPhoneNumberFirst", "", @"Should the cell phone number be listed first before home phone number?", 19, @"False", "D4625D2F-97EF-42CC-A3B2-8FFF2E156C57" );
            // Attrib for BlockType: Add Group:Phone Number
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Phone Number", "PhoneNumber", "", @"Should a phone number be required for at least one person?", 20, @"False", "B21E9EBE-38B8-4843-92E2-5E1E442C7389" );
            // Attrib for BlockType: Add Group:Adult Phone Number
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Adult Phone Number", "AdultPhoneNumber", "", @"When Family group type, should a phone number be required for each adult added?", 21, @"False", "D55A76A9-2EBE-4F0E-9015-3F633C292556" );
            // Attrib for BlockType: Add Group:Show Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Title", "ShowTitle", "", @"Show person title.", 2, @"True", "87FF462E-D63F-4A5D-B983-91F9D67E64AF" );
            // Attrib for BlockType: Add Group:Show Nick Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Nick Name", "ShowNickName", "", @"Show an edit box for Nick Name.", 3, @"False", "283505F0-E29E-4ECC-A606-A82792E24F29" );
            // Attrib for BlockType: Add Group:Address
            RockMigrationHelper.UpdateBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Address", "Address", "", @"Should an address be required for the family?", 17, @"NOTREQUIRED", "1A608F68-D363-41B5-860D-276512900C0C" );
            // Attrib for BlockType: Edit Group:Require Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "", @"Determines if a campus is required.", 1, @"True", "4AD180F8-A2B4-4034-99C9-B576CFB746B4" );
            // Attrib for BlockType: Edit Group:Require Birthdate
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Birthdate", "RequireBirthdate", "", @"Determines if a birthdate should be required.", 2, @"False", "D6FB44F7-55A9-4FE4-99E0-48A33F2E9EF1" );
            // Attrib for BlockType: Edit Group:Hide Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Title", "HideTitle", "", @"Should Title field be hidden when entering new people?.", 3, @"False", "9C3FEA1C-0F58-4AF9-B085-B73792A60488" );
            // Attrib for BlockType: Edit Group:Hide Suffix
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Suffix", "HideSuffix", "", @"Should Suffix field be hidden when entering new people?.", 4, @"False", "58492900-A532-46CC-AE39-BB09A77FEF6C" );
            // Attrib for BlockType: Edit Group:Hide Grade
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Grade", "HideGrade", "", @"Should Grade field be hidden when entering new people?.", 5, @"False", "C469CCD0-6E23-4051-9295-D83E024C22F8" );
            // Attrib for BlockType: Edit Group:Show Age
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Age", "ShowAge", "", @"Should Age of Family Members be displayed?.", 6, @"False", "884FC5DA-09D3-4A87-BACF-BABBB4A0CF77" );
            // Attrib for BlockType: Edit Group:New Person Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "New Person Email", "NewPersonEmail", "", @"Should an Email field be displayed when adding a new person to the family?", 8, @"False", "3420048F-1D61-4690-9403-FA462631B918" );
            // Attrib for BlockType: Edit Group:New Person Phone
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "New Person Phone", "NewPersonPhone", "", @"The Phone Type to prompt for when adding a new person to family (if any).", 7, @"", "D445A74C-3762-4DBE-B9A4-D3C2F12E1A78" );
            // Attrib for BlockType: Edit Group:Default Connection Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Connection Status", "DefaultConnectionStatus", "", @"The connection status that should be set by default", 0, @"B91BA046-BC1E-400C-B85D-638C1F4E0CE2", "A5D96C21-356E-48E2-BF20-B9275C510EAE" );
            // Attrib for BlockType: Mobile Entry:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "67E9E493-1D11-4C73-8E59-6D3C2C25CA25", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", @"The workflow type to activate for check-in", 0, @"", "2FC84934-7E00-4CCA-A73F-F75E48892B0E" );
            // Attrib for BlockType: Mobile Entry:Home Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "67E9E493-1D11-4C73-8E59-6D3C2C25CA25", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", @"", 2, @"", "A43F6C24-CC21-4D14-BFB6-86912B499956" );
            // Attrib for BlockType: Mobile Entry:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "67E9E493-1D11-4C73-8E59-6D3C2C25CA25", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", @"", 3, @"", "192248C9-B7A4-4930-B6CC-124BB8C31371" );
            // Attrib for BlockType: Mobile Entry:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "67E9E493-1D11-4C73-8E59-6D3C2C25CA25", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", @"", 4, @"", "99D1444F-BCC5-4AF4-AEB2-EE37CF5DF9D7" );
            // Attrib for BlockType: Mobile Entry:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "67E9E493-1D11-4C73-8E59-6D3C2C25CA25", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", @"The name of the workflow activity to run on selection.", 1, @"", "BCF8FE62-ED54-436F-B428-F1BEF8E9718F" );
            // Attrib for BlockType: Tag List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "C6DFE5AE-8C4C-49AD-8EC9-11CE03146F53", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "E99DDF46-41E6-4140-8C9C-613C8964422C" );
            // Attrib for BlockType: Defined Value List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "87DAF7ED-AAF5-4D5C-8339-CB30B16CC9FF" );
            // Attrib for BlockType: Layout List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "5996BF81-F2E2-4702-B401-B0B1B6667DAE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "CE5767BD-B635-4F99-93DA-E5DA1880E32E" );
            // Attrib for BlockType: Group Simple Register:Load Current Person from Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "82A285C1-0D6B-41E0-B1AA-DD356021BDBF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Load Current Person from Page", "LoadPerson", "", @"If set to true the form will autopopulate fields from the person profile", 0, @"False", "72007D23-1B45-4D49-B344-2B2A5DD3B90C" );
            // Attrib for BlockType: Group Simple Register Confirm:Success Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "B71FE9F2-0F90-497F-90FA-5A6148E8E116", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Success Message", "SuccessMessage", "", @"The text to display when a valid group member key is provided", 0, @"You have been registered.", "B21BE0C2-801D-4ACB-9A37-E57A1749ABB3" );
            // Attrib for BlockType: Group Simple Register Confirm:Error Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "B71FE9F2-0F90-497F-90FA-5A6148E8E116", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Error Message", "ErrorMessage", "", @"The text to display when a valid group member key is NOT provided", 0, @"Sorry, there was a problem confirming your registration.  Please try to register again.", "D139C29B-FE6C-4B35-B31C-C1A2C5B76CC2" );
            // Attrib for BlockType: External Application List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "850A0541-D31A-4559-94D1-9DAD5F52EFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "ACFF42B7-0F11-407D-9AF1-7A84DD65B178" );
            // Attrib for BlockType: Account List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "E288DE82-638B-4D24-8677-782490CD50C8" );
            // Attrib for BlockType: Prayer Session:Enable Prayer Team Flagging
            RockMigrationHelper.UpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Prayer Team Flagging", "EnableCommunityFlagging", "", @"If enabled, members of the prayer team can flag a prayer request if they feel the request is inappropriate and needs review by an administrator.", 3, @"False", "F99E9DDD-58E3-406B-A1EB-242B862AEBDA" );
            // Attrib for BlockType: Prayer Session:Flag Limit
            RockMigrationHelper.UpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Flag Limit", "FlagLimit", "", @"The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.", 4, @"1", "DD6458D5-F0D8-4EB5-A700-2DF319212563" );
            // Attrib for BlockType: Prayer Session:Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "CategoryGuid", "", @"A top level category. This controls which categories are shown when starting a prayer session.", 2, @"", "B10047A7-6DD3-4502-8916-31411B7581ED" );
            // Attrib for BlockType: Prayer Session:Prayer Person Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Prayer Person Lava", "PrayerPersonLava", "", @"The Lava Template for how the person details are shown in the header", 5, @"
{% if PrayerRequest.RequestedByPersonAlias %}
<img src='{{ PrayerRequest.RequestedByPersonAlias.Person.PhotoUrl }}' class='pull-left margin-r-md img-thumbnail' width=50 />
{% endif %}
<span class='first-word'>{{ PrayerRequest.FirstName }}</span> {{ PrayerRequest.LastName }}
", "C24BAAB7-0112-4EBB-A4FE-921CCDB5DE83" );
            // Attrib for BlockType: Prayer Session:Prayer Display Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Prayer Display Lava", "PrayerDisplayLava", "", @"The Lava Template which will show the details of the Prayer Request", 5, @"
<div class='row'>
    <div class='col-md-6'>
        <strong>Prayer Request</strong>
    </div>
    <div class='col-md-6 text-right'>
      {% if PrayerRequest.EnteredDateTime  %}
          Date Entered: {{  PrayerRequest.EnteredDateTime | Date:'M/d/yyyy'  }}          
      {% endif %}
    </div>
</div>
                                                
{{ PrayerRequest.Text | NewlineToBr }}

<div class='attributes margin-t-md'>
{% for prayerRequestAttribute in PrayerRequest.AttributeValues %}
    {% if prayerRequestAttribute.Value != '' %}
    <strong>{{ prayerRequestAttribute.AttributeName }}</strong> 
    <p>{{ prayerRequestAttribute.ValueFormatted }}</p>
    {% endif %}
{% endfor %}
</div>

{% if PrayerRequest.Answer %}
<div class='margin-t-lg'>
    <strong>Update</strong> 
    <br />
    {{ PrayerRequest.Answer | Escape | NewlineToBr }}
</div>
{% endif %}

", "E73D37E0-5F20-4DA0-8DF0-E4F7165B4C0D" );
            // Attrib for BlockType: Prayer Session:Welcome Introduction Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "FD294789-3B72-4D83-8006-FA50B5087D06", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Welcome Introduction Text", "WelcomeIntroductionText", "", @"Some text (or HTML) to display on the first step.", 1, @"<h2>Let's get ready to pray...</h2>", "DC637E0A-FD7D-4B85-AAB0-4A65A9413B4F" );
            // Attrib for BlockType: REST Action List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "46132F7A-D886-4F38-9669-4D4F427B7144" );
            // Attrib for BlockType: REST Controller List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "EDD5FC9A-F3CF-4279-8494-604942A0E376" );
            // Attrib for BlockType: Audit Information List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "D3B7C96B-DF1F-40AF-B09F-AB468E0E726D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "9FB73EE1-0A30-46F3-B3F1-BAFE0F45E150" );
            // Attrib for BlockType: Categories:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "620FC4A2-6587-409F-8972-22065919D9AC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "65AA8E36-9BAB-4BB7-B48A-FE0BF8A30CE3" );
            // Attrib for BlockType: User Login List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "A5473805-AD98-4FFB-9544-7C9848ACBC61" );
            // Attrib for BlockType: Scheduled Transaction Edit:Payment Info Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Payment Info Title", "PaymentInfoTitle", "", @"The text to display as heading of section for entering credit card or bank account information.", 7, @"Payment Information", "92D431E7-FDEF-4D65-99A8-0D6B4EB62556" );
            // Attrib for BlockType: Scheduled Transaction Edit:Panel Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "", @"The text to display in panel heading", 4, @"Scheduled Transaction", "4D607EDE-DC77-4A7D-A829-48846E631A4D" );
            // Attrib for BlockType: Scheduled Transaction Edit:Contribution Info Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Contribution Info Title", "ContributionInfoTitle", "", @"The text to display as heading of section for selecting account and amount.", 5, @"Contribution Information", "49AAF09D-1E8D-4D0D-AFAF-F984DF8FC714" );
            // Attrib for BlockType: Scheduled Transaction Edit:Confirmation Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirmation Title", "ConfirmationTitle", "", @"The text to display as heading of section for confirming information entered.", 8, @"Confirm Information", "CDDA2367-97E3-4DD7-B9D5-AF6102845FA9" );
            // Attrib for BlockType: Page List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9847FE8-5279-4CC4-BD69-8A71B2F1ED70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "97B89575-3F20-4BA2-A018-B4BEB6FEE0F3" );
            // Attrib for BlockType: Page List:Show Page Id
            RockMigrationHelper.UpdateBlockTypeAttribute( "D9847FE8-5279-4CC4-BD69-8A71B2F1ED70", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Page Id", "ShowPageId", "", @"Enables the hiding of the page id column.", 0, @"True", "AD381367-E0C0-4035-984A-060C13313E99" );
            // Attrib for BlockType: Person Merge:Reset Login Confirmation
            RockMigrationHelper.UpdateBlockTypeAttribute( "9B274A75-1D9B-4533-9849-7892F10A7672", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Reset Login Confirmation", "ResetLoginConfirmation", "", @"When merging people that have different email addresses, should the logins for those people be updated to require a reconfirmation of the selected email address before being able to login? This is typically enabled as a precaution to prevent someone maliciously obtaining another person's login information simply by creating a duplicate account with same name but different login.", 0, @"True", "C63D850C-11F7-46A5-B817-071C171C49DD" );
            // Attrib for BlockType: Attendance History:Filter Attendance By Default
            RockMigrationHelper.UpdateBlockTypeAttribute( "21FFA70E-18B3-4148-8FC4-F941100B49B8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Attendance By Default", "FilterAttendanceByDefault", "", @"Sets the default display of Attended to Did Attend instead of [All]", 0, @"False", "EC5CFD09-6521-4686-B24D-C06D9FC59088" );
            // Attrib for BlockType: Attendance History:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "21FFA70E-18B3-4148-8FC4-F941100B49B8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "18CD8EAE-BD83-42E7-89D7-94CE31AEAA41" );
            // Attrib for BlockType: Layout Block List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "CD3C0C1D-2171-4FCC-B840-FC6E6F72EEEF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "C6BA7F15-4663-4439-A75F-169CAB0C694D" );
            // Attrib for BlockType: Person Badge List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "D8CCD577-2200-44C5-9073-FD16F174D364", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "02E27886-399B-47E0-BDCD-19FFCF871466" );
            // Attrib for BlockType: Account Detail:Show Home Address
            RockMigrationHelper.UpdateBlockTypeAttribute( "B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Home Address", "ShowHomeAddress", "", @"Shows/hides the home address.", 1, @"False", "B5B965A3-A212-4930-ACCE-F80A5C79D07E" );
            // Attrib for BlockType: Account Detail:Location Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Location Type", "LocationType", "", @"The type of location that address should use.", 14, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "514B5D62-0570-4D00-B592-E7CD93252F16" );
            // Attrib for BlockType: Account Edit:Location Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "F501AB3F-1F41-4C06-9BC2-57C42E702995", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Location Type", "LocationType", "", @"The type of location that address should use.", 14, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "DC31914A-0F13-4243-91E7-69A7EBBCCF10" );
            // Attrib for BlockType: Account Edit:Show Address
            RockMigrationHelper.UpdateBlockTypeAttribute( "F501AB3F-1F41-4C06-9BC2-57C42E702995", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Address", "ShowAddress", "", @"Allows hiding the address field.", 0, @"False", "85534E70-C876-4E09-8DD5-B143896FAB38" );
            // Attrib for BlockType: Account Edit:Address Required
            RockMigrationHelper.UpdateBlockTypeAttribute( "F501AB3F-1F41-4C06-9BC2-57C42E702995", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Address Required", "AddressRequired", "", @"Whether the address is required.", 2, @"False", "D19A5C1D-F387-4AB9-8123-99CDBE9242DE" );
            // Attrib for BlockType: System Email List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "2645A264-D5E5-43E8-8FE2-D351F3D5435B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "5D9FC1F9-DA36-428B-9FA6-4B14FCE6BF01" );
            // Attrib for BlockType: RSS Feed:CSS File
            RockMigrationHelper.UpdateBlockTypeAttribute( "2760F435-3E89-4016-85D9-13C019D0C58F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CSS File", "CSSFile", "", @"An optional CSS file to add to the page for styling. Example ""Styles/rss.css"" would point to the stylesheet in the current theme's styles folder.", 0, @"", "01E886C9-78AE-4EAE-9A2E-807E0314A742" );
            // Attrib for BlockType: RSS Feed:RSS Feed Url
            RockMigrationHelper.UpdateBlockTypeAttribute( "2760F435-3E89-4016-85D9-13C019D0C58F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "RSS Feed Url", "RSSFeedUrl", "", @"The Url of the RSS Feed to retrieve and consume", 0, @"", "BBFD1110-AB2B-4F56-B196-118786FAB891" );
            // Attrib for BlockType: RSS Feed:Results per page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2760F435-3E89-4016-85D9-13C019D0C58F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Results per page", "Resultsperpage", "", @"How many results/articles to display on the page at a time. Default is 10.", 0, @"10", "C7963FBE-EA2C-4298-8E7E-3426156A8591" );
            // Attrib for BlockType: RSS Feed:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "2760F435-3E89-4016-85D9-13C019D0C58F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"The length of time (in minutes) that the RSS Feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes", 0, @"20", "59E6DDD9-ABAF-48FC-A73B-C8802DD443CD" );
            // Attrib for BlockType: RSS Feed:Include RSS Link
            RockMigrationHelper.UpdateBlockTypeAttribute( "2760F435-3E89-4016-85D9-13C019D0C58F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include RSS Link", "IncludeRSSLink", "", @"Flag indicating that an RSS link should be included in the page header.", 0, @"True", "DBA970DF-0D4A-4104-AA05-D985E5055174" );
            // Attrib for BlockType: RSS Feed:Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "2760F435-3E89-4016-85D9-13C019D0C58F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "", @"The liquid template to use for rendering. This template would typically be in the theme's ""Assets/Liquid"" folder.", 0, @"{% include '~~/Assets/Lava/RSSFeed.lava' %}", "F2C8BB7D-86BC-4211-8396-E4309526A66E" );
            // Attrib for BlockType: RSS Feed:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2760F435-3E89-4016-85D9-13C019D0C58F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "51DE4DA1-E671-4823-A76E-599A7D39BAF0" );
            // Attrib for BlockType: RSS Feed Item:Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7898E47-8496-4D70-9594-4D1F616928F5", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "", @"The liquid template to use for rendering. This template would typically be in the theme's ""Assets/Liquid"" folder.", 0, @"{% include '~~/Assets/Lava/RSSFeedItem.lava' %}", "B9D43C1D-F588-498F-9524-AB5DEF1DA1D2" );
            // Attrib for BlockType: RSS Feed Item:Include RSS Link
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7898E47-8496-4D70-9594-4D1F616928F5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include RSS Link", "IncludeRSSLink", "", @"Flag indicating that an RSS link should be included in the page header.", 0, @"True", "EB093D1E-09AB-4CB8-BDE4-524D587D0419" );
            // Attrib for BlockType: RSS Feed Item:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7898E47-8496-4D70-9594-4D1F616928F5", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"The length of time (in minutes) that the RSS feed data is stored in cache. If this value is 0, the feed will not be cached. Default is 20 minutes.", 0, @"20", "2B638487-58AE-45D0-BCC6-73CCD9A4B7F2" );
            // Attrib for BlockType: RSS Feed Item:RSS Feed Url
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7898E47-8496-4D70-9594-4D1F616928F5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "RSS Feed Url", "RSSFeedUrl", "", @"The Url to the RSS feed that the item belongs to.", 0, @"", "3EABAF22-5A67-44A9-B1AA-A986289C81B6" );
            // Attrib for BlockType: RSS Feed Item:CSS File
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7898E47-8496-4D70-9594-4D1F616928F5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CSS File", "CSSFile", "", @"An optional CSS File to add to the page for styling. Example ""Styles/rss.css"" would point to a stylesheet in the current theme's style folder.", 0, @"", "5D3B0674-BC91-4D3C-A256-909C770A933C" );
            // Attrib for BlockType: Location List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "4D7F1C41-C24B-4179-9C34-70517345955F" );
            // Attrib for BlockType: Group Type Map:Attributes
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attributes", "Attributes", "", @"Comma delimited list of attribute keys to include values for in the map info window (e.g. 'StudyTopic,MeetingTime').", 7, @"", "53CB5577-2826-4DC0-B890-489007A583FF" );
            // Attrib for BlockType: Group Type Map:Map Height
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Map Height", "MapHeight", "", @"Height of the map in pixels (default value is 600px)", 2, @"600", "B04F6098-0DC5-4183-8E33-E1F69CFDB0C1" );
            // Attrib for BlockType: Group Type Map:Show Map Info Window
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Map Info Window", "ShowMapInfoWindow", "", @"Control whether a info window should be displayed when clicking on a map point.", 5, @"True", "B8B3C9EE-7304-4555-A330-18641DA8052A" );
            // Attrib for BlockType: Group Type Map:Include Inactive Groups
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Groups", "IncludeInactiveGroups", "", @"Determines if inactive groups should be included on the map.", 6, @"False", "F3A19EEB-12E2-4A0A-9E2B-308155EB5A1A" );
            // Attrib for BlockType: Group Type Map:Group Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", @"The type of group to map.", 0, @"", "5D434254-1510-4DAD-94F8-D55BF0670A4B" );
            // Attrib for BlockType: Group Type Map:Person Profile Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", @"Page to use as a link to the person profile page (optional).", 4, @"", "E00178BE-68AB-4AD8-9989-A78597C76C7D" );
            // Attrib for BlockType: Group Type Map:Group Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", @"Page to use as a link to the group details (optional).", 3, @"", "5986354D-18BD-429E-B173-52F11CD33131" );
            // Attrib for BlockType: Group Type Map:Map Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", @"The map theme that should be used for styling the map.", 8, @"BFC46259-FB66-4427-BF05-2B030A582BEA", "E2827B1E-7088-4742-B6C2-60A9E36DE9FE" );
            // Attrib for BlockType: Group Type Map:Info Window Contents
            RockMigrationHelper.UpdateBlockTypeAttribute( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Info Window Contents", "InfoWindowContents", "", @"Liquid template for the info window. To suppress the window provide a blank template.", 9, @"
<div class='clearfix'>
    <h4 class='pull-left' style='margin-top: 0;'>{{GroupName}}</h4> 
    <span class='label label-campus pull-right'>{{GroupCampus}}</span>
</div>

<div class='clearfix'>
    <div class='pull-left' style='padding-right: 24px'>
        <strong>{{GroupLocation.Name}}</strong><br>
        {{GroupLocation.Street1}}
        <br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.PostalCode}}
        {% for attribute in Attributes %}
            {% if forloop.first %}<br/>{% endif %}
            <br/><strong>{{attribute.Name}}:</strong> {{ attribute.Value }}
        {% endfor %}
    </div>
    <div class='pull-left'>
        <strong>{{GroupMemberTerm}}s</strong><br>
        {% for GroupMember in GroupMembers -%}
            {% if PersonProfilePage != '' %}
                <a href='{{PersonProfilePage}}{{GroupMember.Id}}'>{{GroupMember.NickName}} {{GroupMember.LastName}}</a>
            {% else %}
                {{GroupMember.NickName}} {{GroupMember.LastName}}
            {% endif %}
            - {{GroupMember.Email}}
            {% for PhoneType in GroupMember.PhoneTypes %}
                <br>{{PhoneType.Name}}: {{PhoneType.Number}}
            {% endfor %}
            <br>
        {% endfor -%}
    </div>
</div>

{% if GroupDetailPage != '' %}
    <br>
    <a class='btn btn-xs btn-action' href='{{GroupDetailPage}}'>View Group</a>
{% endif %}

", "E168AE80-1DA6-47A2-B6F3-462DC994183F" );
            // Attrib for BlockType: Metric Value List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "E40A1526-04D0-42A0-B275-D1AE161E2E57", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "6FE72B62-6894-46B6-96A4-77732A442140" );
            // Attrib for BlockType: Business List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "1ACCF349-73A5-4568-B801-2A6A620791D9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "8AA506D6-69CF-4EC2-9D71-18B197BC2CEB" );
            // Attrib for BlockType: Rest Key List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "C4FBF612-C1F6-428B-97FD-8AB0B8EA31FC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "16EE7F1B-FC90-45E0-A7BD-2B876BA7679F" );
            // Attrib for BlockType: My Workflows:Categories
            RockMigrationHelper.UpdateBlockTypeAttribute( "689B434F-DD2D-464A-8DA3-21F8768BB5BF", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Categories", "Categories", "", @"Optional Categories to limit display to.", 0, @"", "CB191800-8D80-4E5A-A4E7-CEED19369AB4" );
            // Attrib for BlockType: Attendance Analytics:Show All Groups
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show All Groups", "ShowAllGroups", "", @"Should all of the available groups be listed individually with checkboxes? If not, a group dropdown will be used instead for selecting the desired groups", 2, @"True", "503DC7E4-2898-44EF-9385-6105E32FD1BE" );
            // Attrib for BlockType: Attendance Analytics:Include Inactive Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "", @"Should campus filter include inactive campuses?", 1, @"False", "4EA0A186-1053-4B56-BC54-648BBFF8E125" );
            // Attrib for BlockType: Line Chart:MetricEntityTypeEntityIds
            RockMigrationHelper.UpdateBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MetricEntityTypeEntityIds", "MetricEntityTypeEntityIds", "", @"", 0, @"", "67906D07-E57B-4C60-9867-88E83B37E301" );
            // Attrib for BlockType: Line Chart:Legend Position
            RockMigrationHelper.UpdateBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Legend Position", "LegendPosition", "", @"Select the position of the Legend (corner)", 8, @"ne", "468FF1DF-3BF6-4662-9F6F-B478E4CF84DA" );
            // Attrib for BlockType: Line Chart:Show Legend
            RockMigrationHelper.UpdateBlockTypeAttribute( "0ADBF632-D54D-42D5-A8A1-517E95DDFDB1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legend", "ShowLegend", "", @"", 7, @"True", "7818BAB4-050B-4C0C-9221-A3238E5EE3C7" );
            // Attrib for BlockType: Bar Chart:Legend Position
            RockMigrationHelper.UpdateBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Legend Position", "LegendPosition", "", @"Select the position of the Legend (corner)", 8, @"ne", "04440FC9-6684-4C75-842A-591901934BD9" );
            // Attrib for BlockType: Bar Chart:Show Legend
            RockMigrationHelper.UpdateBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legend", "ShowLegend", "", @"", 7, @"True", "D0A9BD89-F729-4CA3-BEFF-D704B3F97EE7" );
            // Attrib for BlockType: Bar Chart:MetricEntityTypeEntityIds
            RockMigrationHelper.UpdateBlockTypeAttribute( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MetricEntityTypeEntityIds", "MetricEntityTypeEntityIds", "", @"", 0, @"", "80CF517F-84F5-44A6-8CFA-9D28DFAB39DE" );
            // Attrib for BlockType: Campus Context Setter:No Campus Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Campus Text", "NoCampusText", "", @"The text displayed when no campus context is selected.", 3, @"Select Campus", "CC702570-A662-47E2-82EA-F1EF27E233D8" );
            // Attrib for BlockType: Campus Context Setter:Clear Selection Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Clear Selection Text", "ClearSelectionText", "", @"The text displayed when a campus can be unselected. This will not display when the text is empty.", 4, @"", "555B3C64-EBF0-4389-B601-5A21F9953F5C" );
            // Attrib for BlockType: Campus Context Setter:Current Item Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Current Item Template", "CurrentItemTemplate", "", @"Lava template for the current item. The only merge field is {{ CampusName }}.", 1, @"{{ CampusName }}", "629045B3-3995-4FE0-9FA9-FBFEB12155D9" );
            // Attrib for BlockType: Campus Context Setter:Dropdown Item Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Dropdown Item Template", "DropdownItemTemplate", "", @"Lava template for items in the dropdown. The only merge field is {{ CampusName }}.", 2, @"{{ CampusName }}", "05F4AEE9-D9A7-422F-B273-18E7DFEA6F5A" );
            // Attrib for BlockType: Campus Context Setter:Include Inactive Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "", @"Should inactive campuses be listed as well?", 6, @"False", "6ABD57D2-2ADE-4B68-A7D9-687789C20FB0" );
            // Attrib for BlockType: Campus Context Setter:Display Query Strings
            RockMigrationHelper.UpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Query Strings", "DisplayQueryStrings", "", @"Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", 5, @"False", "13A51CAC-C01E-48CC-9A1B-A97A7B2E820B" );
            // Attrib for BlockType: Campus Context Setter:Default To Current User's Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Current User's Campus", "DefaultToCurrentUser", "", @"Will use the campus of the current user if no context is provided.", 7, @"False", "35EE069A-9304-469A-961D-634C759D386E" );
            // Attrib for BlockType: Group Context Setter:Include GroupType Children
            RockMigrationHelper.UpdateBlockTypeAttribute( "62F749F7-67DF-4A84-B7DD-84CA8E10E205", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include GroupType Children", "IncludeGroupTypeChildren", "", @"Include all children of the grouptype selected", 5, @"False", "83E804E9-A86E-4E9A-BEBF-1437CE72C330" );
            // Attrib for BlockType: Group Context Setter:Display Query Strings
            RockMigrationHelper.UpdateBlockTypeAttribute( "62F749F7-67DF-4A84-B7DD-84CA8E10E205", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Query Strings", "DisplayQueryStrings", "", @"Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", 4, @"False", "0AF70715-5CED-4E98-8986-407B188C31EA" );
            // Attrib for BlockType: Group Context Setter:No Group Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "62F749F7-67DF-4A84-B7DD-84CA8E10E205", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Group Text", "NoGroupText", "", @"The text to show when there is no group in the context.", 2, @"Select Group", "D8CCF6EB-E25E-420A-A4CE-8B544A6931C7" );
            // Attrib for BlockType: Group Context Setter:Clear Selection Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "62F749F7-67DF-4A84-B7DD-84CA8E10E205", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Clear Selection Text", "ClearSelectionText", "", @"The text displayed when a group can be unselected. This will not display when the text is empty.", 3, @"", "6E4E463D-2DAE-4EF6-8FFB-FCBE6ADB0156" );
            // Attrib for BlockType: Group Map:Show Campuses Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campuses Filter", "ShowCampusesFilter", "", @"", 6, @"False", "63AC6FBE-BA1C-4BB9-B0C9-213C2DB4E977" );
            // Attrib for BlockType: Locations:Search By Code
            RockMigrationHelper.UpdateBlockTypeAttribute( "00FC1DEA-FE34-41E3-BC0A-2EE9138091EC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Search By Code", "SearchByCode", "", @"A flag indicating if security codes should also be evaluated in the search box results.", 5, @"False", "2C2688F2-18E0-4927-9E1B-F802CF3E2744" );
            // Attrib for BlockType: Person Following List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "BD548744-DC6D-4870-9FED-BB9EA24E709B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "EAE9C4F8-DA94-469E-A3D0-2C406BD7EE4A" );
            // Attrib for BlockType: Stark Detail:Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "D6B14847-B652-49E2-9D4B-658D502F0AEC", "3D045CAE-EA72-4A04-B7BE-7FD1D6214217", "Email", "Email", "", @"", 0, @"", "2014838C-4AA0-426C-88D9-F6DAF5766CCE" );
            // Attrib for BlockType: Stark List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "E333D1CC-CB55-4E73-8568-41DAD296971C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "32C800D4-87DF-4BE6-BFF1-9E5C46D9482A" );
            // Attrib for BlockType: Transaction Matching:Show Selected Accounts Only
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Selected Accounts Only", "ShowSelectedAccountsOnly", "", @"If transaction already has allocated amounts, should only the accounts that transaction is allocated to be displayed by default?", 1, @"False", "4778D4B6-7B0B-41E9-A3B9-154A1CB9DA40" );
            // Attrib for BlockType: Transaction Matching:Batch Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Batch Detail Page", "BatchDetailPage", "", @"Select the page for displaying batch details", 3, @"", "494C6487-8007-439F-BF0B-3F6020D159E8" );
            // Attrib for BlockType: Bank Account List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "C4191011-0391-43DF-9A9D-BE4987C679A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "8BE584E8-45CA-4C69-B347-060A2E9D25A4" );
            // Attrib for BlockType: Scheduled Transaction List Liquid:Transfer Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Transfer Button Text", "TransferButtonText", "", @"The text to use on the transfer (edit) button which is used when a Transfer-To gateway is set.", 7, @"Transfer", "48ED4914-42C2-4334-84AB-301ED256BE3A" );
            // Attrib for BlockType: Scheduled Transaction List Liquid:Gateway Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "7B34F9D8-6BBA-423E-B50E-525ABB3A1013", "Gateway Filter", "GatewayFilter", "", @"When set, causes only scheduled transaction's of a particular gateway to be shown.", 6, @"", "1A9C4D6A-F855-4752-8582-02E9B476BB4C" );
            // Attrib for BlockType: Scheduled Transaction List Liquid:Transfer-To Gateway
            RockMigrationHelper.UpdateBlockTypeAttribute( "081FF29F-0A9F-4EC3-95AD-708FA0E6132D", "7B34F9D8-6BBA-423E-B50E-525ABB3A1013", "Transfer-To Gateway", "TransferToGateway", "", @"Set this if you want people to transfer their existing scheduled transactions to this new gateway. When set, the Edit button becomes 'Transfer' (default or whatever is set in the 'Transfer Button Text' setting) if the scheduled transaction's gateway does not match the transfer-to gateway.", 7, @"", "C060EBA2-B357-460D-B8FF-D56FFE5D46E9" );
            // Attrib for BlockType: Transaction Report:Transaction Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Transaction Types", "TransactionTypes", "", @"Optional list of transation types to limit the list to (if none are selected all types will be included).", 5, @"", "6D563A12-1900-4ACA-B8CC-C83A2B417AF1" );
            // Attrib for BlockType: Transaction Report:Show Transaction Code
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Transaction Code", "ShowTransactionCode", "", @"Show the transaction code column in the table.", 4, @"True", "D2F5F58A-78AF-4AE2-AA5D-0F19587BB94E" );
            // Attrib for BlockType: Transaction Report:Show Foreign Key
            RockMigrationHelper.UpdateBlockTypeAttribute( "1FAEE5A2-5005-4BD8-A2BD-B7D9030A894D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Foreign Key", "ShowForeignKey", "", @"Show the transaction foreign key column in the table.", 4, @"True", "BE554F20-AA31-453D-B70B-CAE179E2F9CB" );
            // Attrib for BlockType: Person Duplicate Detail:Include Businesses
            RockMigrationHelper.UpdateBlockTypeAttribute( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Businesses", "IncludeBusinesses", "", @"Set to true to also include potential matches when either record is a Business.", 3, @"False", "4DF776B6-BA89-4DFE-9051-825874623305" );
            // Attrib for BlockType: Person Duplicate Detail:Include Inactive
            RockMigrationHelper.UpdateBlockTypeAttribute( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive", "IncludeInactive", "", @"Set to true to also include potential matches when both records are inactive.", 2, @"False", "62621A20-6B4E-4B7B-9B45-B85B7AD38C7B" );
            // Attrib for BlockType: Person Duplicate Detail:Confidence Score High
            RockMigrationHelper.UpdateBlockTypeAttribute( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Confidence Score High", "ConfidenceScoreHigh", "", @"The minimum confidence score required to be considered a likely match", 0, @"80", "386EEFBE-7C7A-4CDC-AAF9-5279616524E7" );
            // Attrib for BlockType: Person Duplicate Detail:Confidence Score Low
            RockMigrationHelper.UpdateBlockTypeAttribute( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Confidence Score Low", "ConfidenceScoreLow", "", @"The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.", 1, @"40", "64A69170-FFA0-4662-8949-7328A4D88B2D" );
            // Attrib for BlockType: Person Duplicate List:Confidence Score Low
            RockMigrationHelper.UpdateBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Confidence Score Low", "ConfidenceScoreLow", "", @"The maximum confidence score required to be considered an unlikely match. Values lower than this will not be shown in the grid.", 1, @"60", "F11A22AC-AE05-4914-881E-97714BCB698F" );
            // Attrib for BlockType: Person Duplicate List:Confidence Score High
            RockMigrationHelper.UpdateBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Confidence Score High", "ConfidenceScoreHigh", "", @"The minimum confidence score required to be considered a likely match", 0, @"80", "C13217F8-5603-43E5-96DC-541A54CEBB41" );
            // Attrib for BlockType: Person Duplicate List:Include Inactive
            RockMigrationHelper.UpdateBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive", "IncludeInactive", "", @"Set to true to also include potential matches when both records are inactive.", 2, @"False", "FB935C65-3ADF-42C0-B29A-631D1B962162" );
            // Attrib for BlockType: Person Duplicate List:Include Businesses
            RockMigrationHelper.UpdateBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Businesses", "IncludeBusinesses", "", @"Set to true to also include potential matches when either record is a Business.", 3, @"False", "3BF63097-437A-4C22-913D-4695C1861403" );
            // Attrib for BlockType: Person Duplicate List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "5E685674-43C0-4F9D-8368-B511FCB952A2" );
            // Attrib for BlockType: Report List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "37D29989-F7CA-4D51-925A-378DB73ED53D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "6DDF14BC-3018-4542-8834-C5A86AF76B00" );
            // Attrib for BlockType: Bulk Update:Workflow Types
            RockMigrationHelper.UpdateBlockTypeAttribute( "A844886D-ED6F-4367-9C6F-667401201ED0", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Types", "WorkflowTypes", "", @"The workflows to make available for bulk updating.", 2, @"", "0E4403BD-86DE-4EEE-AB33-75FF3004D901" );
            // Attrib for BlockType: Content Channel Type List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "A580027F-56DB-43B0-AAD6-7C2B8A952012", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "CC55D34A-F777-4ACE-B121-49FE93020996" );
            // Attrib for BlockType: Content Channel List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "991507B6-D222-45E5-BA0D-B61EA72DFB64", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "7BC31AC6-D0B9-4C0B-B146-BBA82209CF78" );
            // Attrib for BlockType: Content Channel Item List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "22E30DD3-94B4-41D8-8D31-0CE1570A35DB" );
            // Attrib for BlockType: Content Channel Item List:Show Priority Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Priority Column", "ShowPriorityColumn", "", @"Determines if the column that displays priority should be shown for content channels that have Priority enabled.", 4, @"True", "A7BDDAA7-8F60-4859-94C9-826FED6F12A6" );
            // Attrib for BlockType: Content Channel Item List:Show Expire Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Expire Column", "ShowExpireColumn", "", @"Determines if the expire column should be shown.", 6, @"True", "F00ECBEE-899B-408E-BC1D-9BA08C357D83" );
            // Attrib for BlockType: Content Channel Item List:Show Filters
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filters", "ShowFilters", "", @"Allows you to show/hide the grids filters.", 2, @"True", "2622028C-5C8F-4714-8473-440C066F134D" );
            // Attrib for BlockType: Content Channel Item List:Show Event Occurrences Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Event Occurrences Column", "ShowEventOccurrencesColumn", "", @"Determines if the column that lists event occurrences should be shown if any of the items has an event occurrence.", 3, @"True", "2CE242E8-6708-4DA4-92A8-E91BBAEAA54C" );
            // Attrib for BlockType: Content Channel Item List:Filter Items For Current User
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Items For Current User", "FilterItemsForCurrentUser", "", @"Filters the items by those created by the current logged in user.", 1, @"False", "A6257107-7D80-4A9C-BDF0-9BAA179C4C1D" );
            // Attrib for BlockType: Content Channel Item List:Show Security Column
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Security Column", "ShowSecurityColumn", "", @"Determines if the security column should be shown.", 5, @"True", "50BAC9F8-69B8-4639-9D7F-CE23A99DBAE9" );
            // Attrib for BlockType: Content Channel Item List:Content Channel
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "", @"If set the block will ignore content channel query parameters", 0, @"", "30B304F8-8FAD-4761-BA84-C52BBA0C8718" );
            // Attrib for BlockType: Content Channel Item List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "F276D9E4-6F42-4C1E-B98C-6CD7090C04FD" );
            // Attrib for BlockType: Content Channel Item Detail:Content Channel
            RockMigrationHelper.UpdateBlockTypeAttribute( "5B99687B-5FE9-4EE2-8679-5040CAEB9E2E", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "", @"If set the block will ignore content channel query parameters", 0, @"", "AB66CBB1-B387-405A-96FB-03EEED3E58DD" );
            // Attrib for BlockType: Content Channel Item Detail:Show Delete Button
            RockMigrationHelper.UpdateBlockTypeAttribute( "5B99687B-5FE9-4EE2-8679-5040CAEB9E2E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Delete Button", "ShowDeleteButton", "", @"Shows a delete button for the current item.", 1, @"False", "C6C02205-06AD-4F9F-814B-D0D9EFB1CD13" );
            // Attrib for BlockType: Package Detail Lava:Set Page Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "9EC29D0F-7EE7-434B-A30F-6C36A81B0DEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "", @"Determines if the block should set the page title with the package name.", 0, @"False", "847959DA-2993-4C88-B392-10C0B05E60D2" );
            // Attrib for BlockType: Package Detail Lava:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "9EC29D0F-7EE7-434B-A30F-6C36A81B0DEB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"Lava template to use to display the package details.", 2, @"{% include '~/Assets/Lava/Store/PackageDetail.lava' %}", "121E9D4F-74A0-4244-B18D-5976EBF24DBA" );
            // Attrib for BlockType: Group Attendance Detail:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"An optional lava template to appear next to each person in the list.", 5, @"", "132DA055-1358-4FA1-8B56-64FACB7A97BC" );
            // Attrib for BlockType: Group Attendance Detail:Allow Campus Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Campus Filter", "AllowCampusFilter", "", @"Should block add an option to allow filtering people and attendance counts by campus?", 2, @"False", "36646052-A994-4A8A-8F2B-70686EC1266C" );
            // Attrib for BlockType: Group Attendance List:Allow Campus Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "5C547728-38C2-420A-8602-3CDAAC369247", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Campus Filter", "AllowCampusFilter", "", @"Should block add an option to allow filtering attendance counts and percentage by campus?", 2, @"False", "764FBDD5-51A6-4F36-AC1C-ACB9A0747565" );
            // Attrib for BlockType: Group Attendance List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "5C547728-38C2-420A-8602-3CDAAC369247", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "AE7102B0-7366-4275-8F89-838119115205" );
            // Attrib for BlockType: Group Detail Lava:Hide the 'Active' Group checkbox
            RockMigrationHelper.UpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide the 'Active' Group checkbox", "HideActiveGroupCheckbox", "", @"Set this to true to hide the checkbox for 'Active' for the group.", 5, @"False", "52C3672E-CFE7-4B07-B752-AB65B753E3CE" );
            // Attrib for BlockType: Group Detail Lava:Hide the 'Public' Group checkbox
            RockMigrationHelper.UpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide the 'Public' Group checkbox", "HidePublicGroupCheckbox", "", @"Set this to true to hide the checkbox for 'Public' for the group.", 6, @"True", "781F44E9-7184-46DE-BE82-D9790FF00AC6" );
            // Attrib for BlockType: Group Detail Lava:Hide Inactive Group Member Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Inactive Group Member Status", "HideInactiveGroupMemberStatus", "", @"Set this to true to hide the radiobox for the 'Inactive' group member status.", 7, @"False", "64AD4226-9581-4075-9805-C34B1E8DC7F7" );
            // Attrib for BlockType: Group Detail Lava:Hide Group Member Role
            RockMigrationHelper.UpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Group Member Role", "HideGroupMemberRole", "", @"Set this to true to hide the drop down list for the 'Role' when editing a group member. If set to 'true' then the default group role will be used when adding a new member.", 8, @"False", "3761C89C-927E-4FA8-9CED-2CB8865CA864" );
            // Attrib for BlockType: Group Detail Lava:Hide Group Description Edit
            RockMigrationHelper.UpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Group Description Edit", "HideGroupDescriptionEdit", "", @"Set this to true to hide the edit box for group 'Description'.", 9, @"False", "F85EA7B5-FC68-42C1-B582-E9130A9C142C" );
            // Attrib for BlockType: Benevolence Request List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "1F9D746E-F140-4EB8-A7A9-DC04876E2CB7" );
            // Attrib for BlockType: Benevolence Request Detail:Case Worker Role
            RockMigrationHelper.UpdateBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Case Worker Role", "CaseWorkerRole", "", @"The security role to draw case workers from", 0, @"", "E3149C69-0383-40F2-9174-4161CD5C9B1F" );
            // Attrib for BlockType: Merge Template Entry:Database Timeout
            RockMigrationHelper.UpdateBlockTypeAttribute( "8C6280DA-9BB4-47C8-96BA-3878B8B85466", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeout", "", @"The number of seconds to wait before reporting a database timeout.", 1, @"180", "789429B8-F8C7-49F4-A440-8201B158DFE3" );
            // Attrib for BlockType: Merge Template List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "DA102F02-6DBB-42E6-BFEE-360E137B1411", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "3B7A975B-5B2D-4220-9DE4-5FD3A4E93B79" );
            // Attrib for BlockType: Gateway List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "32E89BAE-C085-40B3-B872-B62E25A62BDB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "06F7BD4E-3795-4E26-940F-BEB1CDD24B82" );
            // Attrib for BlockType: Transaction Yearly Summary Lava:Accounts
            RockMigrationHelper.UpdateBlockTypeAttribute( "535307C8-77D1-44F8-AD4D-1577572B6D26", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "", @"Limit the results to transactions that match the selected accounts.", 2, @"", "1BD08B72-22FD-451C-84C3-CA3945E4D453" );
            // Attrib for BlockType: Group Requirement Type List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "02D64DF9-6E3F-4B58-AE89-8370247071F8" );
            // Attrib for BlockType: Group Member Add From URL:Group Member Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "42CF3822-A70C-4E07-9394-21607EED7018", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Member Status", "GroupMemberStatus", "", @"The status to use when adding a person to the group.", 0, @"Active", "B10BFEC6-F947-48FB-8A4C-ADA337E835E3" );
            // Attrib for BlockType: Group Member Add From URL:Enable Passing Group Id
            RockMigrationHelper.UpdateBlockTypeAttribute( "42CF3822-A70C-4E07-9394-21607EED7018", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Passing Group Id", "EnablePassingGroupId", "", @"If enabled, allows the ability to pass in a group's Id (GroupId=) instead of the Guid.", 0, @"True", "4B6338CD-0780-4195-898B-82C6109530B5" );
            // Attrib for BlockType: Group Member Add From URL:Default Group Member Role
            RockMigrationHelper.UpdateBlockTypeAttribute( "42CF3822-A70C-4E07-9394-21607EED7018", "3BB25568-E793-4D12-AE80-AC3FDA6FD8A8", "Default Group Member Role", "DefaultGroupMemberRole", "", @"The default role to use if one is not passed through the quert string (optional).", 0, @"", "4778602E-430D-4E76-BD46-3B91BD9C4E27" );
            // Attrib for BlockType: Group Member Add From URL:Default Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "42CF3822-A70C-4E07-9394-21607EED7018", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Default Group", "DefaultGroup", "", @"The default group to use if one is not passed through the query string (optional).", 0, @"", "75C896A4-3CAD-4AC8-9788-4F3575A438B0" );
            // Attrib for BlockType: Group Member Add From URL:Limit Group Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "42CF3822-A70C-4E07-9394-21607EED7018", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Limit Group Type", "LimitGroupType", "", @"To ensure that people cannot modify the URL and try adding themselves to standard Rock security groups with known Id numbers you can limit which Group Type that are considered valid during add.", 0, @"", "33ED3F40-A338-4381-8B65-9CA67C676255" );
            // Attrib for BlockType: Group Member Add From URL:Success Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "42CF3822-A70C-4E07-9394-21607EED7018", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Message", "SuccessMessage", "", @"Lava template to display when person has been added to the group.", 0, @"<div class='alert alert-success'>
    {{ Person.NickName }} has been added to the group '{{ Group.Name }}' with the role of {{ Role.Name }}.
</div>", "B16BFFD5-148B-4FC0-B330-36422C764FB9" );
            // Attrib for BlockType: Group Member Add From URL:Already In Group Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "42CF3822-A70C-4E07-9394-21607EED7018", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Already In Group Message", "AlreadyInGroupMessage", "", @"Lava template to display when person is already in the group with that role.", 0, @"<div class='alert alert-warning'>
    {{ Person.NickName }} is already in the group '{{ Group.Name }}' with the role of {{ Role.Name }}.
</div>", "553090A3-87D2-495B-996E-133EE6048F8C" );
            // Attrib for BlockType: Report Data:Show 'Merge Template' action on grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show 'Merge Template' action on grid", "ShowGridMergeTemplateAction", "", @"", 0, @"True", "29641188-D65D-4B2F-8814-068095AC7366" );
            // Attrib for BlockType: Report Data:Show 'Communications' action on grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show 'Communications' action on grid", "ShowGridCommunicationsAction", "", @"", 0, @"True", "83A292A4-5265-47A3-96E9-E74FC6114D62" );
            // Attrib for BlockType: Report Data:PersonIdField
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "PersonIdField", "PersonIdField", "", @"If this isn't a Person report, but there is a person id field, specify the name of the field", 0, @"", "F1E5F878-3474-4BC0-BC4A-437F16E5F92B" );
            // Attrib for BlockType: Report Data:ResultsTitle
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "ResultsTitle", "ResultsTitle", "", @"Title for the results list.", 0, @"Results", "A04731FB-BDDF-468D-8E0B-D071E4ABD912" );
            // Attrib for BlockType: Report Data:ResultsIconCssClass
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "ResultsIconCssClass", "ResultsIconCssClass", "", @"Title for the results list.", 0, @"fa fa-list", "94A20B2E-BE45-4575-B6D0-658C80C37E4D" );
            // Attrib for BlockType: Report Data:Report
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Report", "Report", "", @"The report to use for this block", 0, @"", "410DD886-C0DF-4421-925A-B843B742C720" );
            // Attrib for BlockType: Report Data:TogglableDataFieldGuids
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "TogglableDataFieldGuids", "TogglableDataFieldGuids", "", @"The configurable datafilters that include a checkbox that can disable/enable the filter", 0, @"", "2374892C-F3BA-4940-84EE-12E252152D9B" );
            // Attrib for BlockType: Report Data:FilterIconCssClass
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "FilterIconCssClass", "FilterIconCssClass", "", @"Title for the results list.", 0, @"fa fa-filter", "23E47F3B-AA3E-4D4A-BA6A-E57D4266160C" );
            // Attrib for BlockType: Report Data:FilterTitle
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "FilterTitle", "FilterTitle", "", @"Title for the results list.", 0, @"Filters", "6734A29C-CE76-485C-92F4-2A75922FE0EB" );
            // Attrib for BlockType: Report Data:SelectedDataFieldGuids
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "SelectedDataFieldGuids", "SelectedDataFieldGuids", "", @"The DataFilters to present to the user", 0, @"", "BB41BFE9-C1D8-41C8-BAD9-936A90DF4F07" );
            // Attrib for BlockType: Report Data:ConfigurableDataFieldGuids
            RockMigrationHelper.UpdateBlockTypeAttribute( "C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "ConfigurableDataFieldGuids", "ConfigurableDataFieldGuids", "", @"Of the DataFilters that are presented to the user, which are configurable vs just a checkbox", 0, @"", "146E8080-5DAC-4E9E-A73E-A7CD95B0902F" );
            // Attrib for BlockType: Check-in Group List:Allow Campus Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "67E83A02-6D23-4B90-A861-F81FF78B56C7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Campus Filter", "AllowCampusFilter", "", @"Should block add an option to allow filtering attendance counts and percentage by campus?", 2, @"False", "8A0B278E-0ED3-4A89-9B8A-12BC6DF306F4" );
            // Attrib for BlockType: Calendar Event Item List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "EC8DFDC5-C177-4208-8ABA-1F85010FBBFF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "893F5D4A-4154-4554-A0B0-0BA29791026D" );
            // Attrib for BlockType: Calendar Lava:Campus Parameter Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Parameter Name", "CampusParameterName", "", @"The page parameter name that contains the id of the campus entity.", 16, @"campusId", "381DE532-AF23-4994-ADCA-09359AF062FC" );
            // Attrib for BlockType: Calendar Lava:Category Parameter Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Parameter Name", "CategoryParameterName", "", @"The page parameter name that contains the id of the category entity.", 17, @"categoryId", "A71A0B44-239D-4D6E-944C-15375C43B8B5" );
            // Attrib for BlockType: Giving Analytics:Hide View By Options
            RockMigrationHelper.UpdateBlockTypeAttribute( "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide View By Options", "HideViewByOptions", "", @"Should the View By options be hidden (Giver, Adults, Children, Family)?", 2, @"False", "F37095D9-C47F-4A7C-98DD-884CF9BD86F8" );
            // Attrib for BlockType: Check-in Scheduled Locations:Previous Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "C8C4E323-C227-4EAA-938F-4B962BC2DD7E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", @"", 3, @"", "49A01E13-F151-472D-9862-5E5BA64D6066" );
            // Attrib for BlockType: Check-in Scheduled Locations:Next Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "C8C4E323-C227-4EAA-938F-4B962BC2DD7E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", @"", 4, @"", "3E080271-B91D-43EC-93A5-FA15F60DFE5D" );
            // Attrib for BlockType: Check-in Scheduled Locations:Workflow Activity
            RockMigrationHelper.UpdateBlockTypeAttribute( "C8C4E323-C227-4EAA-938F-4B962BC2DD7E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", @"The name of the workflow activity to run on selection.", 1, @"", "E8503CEA-AA23-4F02-9B23-8F4018F0B553" );
            // Attrib for BlockType: Check-in Scheduled Locations:Workflow Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "C8C4E323-C227-4EAA-938F-4B962BC2DD7E", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", @"The workflow type to activate for check-in", 0, @"", "2B9D7D0C-2027-4364-ACD4-22BD4BE0E8FE" );
            // Attrib for BlockType: Email Form:HTML Form
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "HTML Form", "HTMLForm", "", @"The HTML for the form the user will complete. <span class='tip tip-lava'></span>", 4, @"{% if CurentUser %}
    {{ CurrentPerson.NickName }}, could you please complete the form below.
{% else %}
    Please complete the form below.
{% endif %}

<div class=""form-group"">
    <label for=""firstname"">First Name</label>
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.NickName }}</p>
        <input type=""hidden"" id=""firstname"" name=""FirstName"" value=""{{ CurrentPerson.NickName }}"" />
    {% else %}
        <input class=""form-control"" id=""firstname"" name=""FirstName"" placeholder=""First Name"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""lastname"">Last Name</label>
    
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.LastName }}</p>
        <input type=""hidden"" id=""lastname"" name=""LastName"" value=""{{ CurrentPerson.LastName }}"" />
    {% else %}
        <input class=""form-control"" id=""lastname"" name=""LastName"" placeholder=""Last Name"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""email"">Email</label>
    {% if CurrentPerson %}
        <input class=""form-control"" id=""email"" name=""Email"" value=""{{ CurrentPerson.Email }}"" placeholder=""Email"" required />
    {% else %}
        <input class=""form-control"" id=""email"" name=""Email"" placeholder=""Email"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""email"">Message</label>
    <textarea id=""message"" rows=""4"" class=""form-control"" name=""Message"" placeholder=""Message"" required></textarea>
</div>

<div class=""form-group"">
    <label for=""email"">Attachment</label>
    <input type=""file"" id=""attachment"" name=""attachment"" /> <br />
    <input type=""file"" id=""attachment2"" name=""attachment2"" />
</div>
", "1B6714A9-F582-4FE0-83E8-CBCE2A52020A" );
            // Attrib for BlockType: Email Form:Message Body
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Message Body", "MessageBody", "", @"The email message body. <span class='tip tip-lava'></span>", 5, @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    A email form has been submitted. Please find the information below:
</p>

{% for field in FormFields %}
    {% assign fieldParts = field | PropertyToKeyValue %}

    <strong>{{ fieldParts.Key | Humanize | Capitalize }}</strong>: {{ fieldParts.Value }} <br/>
{% endfor %}

<p>&nbsp;</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "D85B0F8B-4C9A-4CA6-B989-1F225F82D392" );
            // Attrib for BlockType: Email Form:Response Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Response Message", "ResponseMessage", "", @"The message the user will see when they submit the form if no response page if provided. Lava merege fields are available for you to use in your message.", 6, @"<div class=""alert alert-info"">
    Thank you for your response. We appreciate your feedback!
</div>", "D48017D1-E058-406D-84E8-12137DA423E3" );
            // Attrib for BlockType: Email Form:Response Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Response Page", "ResponsePage", "", @"The page the use will be taken to after submitting the form. Use the 'Response Message' field if you just need a simple message.", 7, @"", "A7981D95-539D-4956-B8C2-E866CA51BE0D" );
            // Attrib for BlockType: Email Form:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 13, @"", "335EDAF1-386E-4EAE-9FDB-09C12049B6AC" );
            // Attrib for BlockType: Email Form:Submit Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Submit Button Text", "SubmitButtonText", "", @"The text to display for the submit button.", 8, @"Submit", "687654AB-C85D-4744-96AE-990EF88ABE90" );
            // Attrib for BlockType: Email Form:Submit Button Wrap CSS Class
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Submit Button Wrap CSS Class", "SubmitButtonWrapCssClass", "", @"CSS class to add to the div wrapping the button.", 9, @"", "36B200E1-2465-452F-83FB-92CC57B63F72" );
            // Attrib for BlockType: Email Form:Submit Button CSS Class
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Submit Button CSS Class", "SubmitButtonCssClass", "", @"The CSS class add to the submit button.", 10, @"btn btn-primary", "2806FDA3-8130-4D8D-9D92-EA1720A103D9" );
            // Attrib for BlockType: Email Form:Receipient Email(s)
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Receipient Email(s)", "RecipientEmail", "", @"Email addresses (comma delimited) to send the contents to.", 0, @"", "112B7E46-155A-4B50-97F7-A10F91BDF6E2" );
            // Attrib for BlockType: Email Form:Subject
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "", @"The subject line for the email. <span class='tip tip-lava'></span>", 1, @"", "82E342A0-4F85-4720-B3BF-834375D6EB20" );
            // Attrib for BlockType: Email Form:From Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "From Email", "FromEmail", "", @"The email address to use for the from. <span class='tip tip-lava'></span>", 2, @"", "EEF7E260-C481-413E-9560-52290ECD9250" );
            // Attrib for BlockType: Email Form:From Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "From Name", "FromName", "", @"The name to use for the from address. <span class='tip tip-lava'></span>", 3, @"", "E88D6BEA-FA64-4F42-A772-EFED618D3B3F" );
            // Attrib for BlockType: Email Form:Enable Debug
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", @"Shows the fields available to merge in lava.", 11, @"False", "6C039C9F-41BD-43ED-B5FB-95097F36F245" );
            // Attrib for BlockType: Email Form:Save Communication History
            RockMigrationHelper.UpdateBlockTypeAttribute( "48253494-F8A0-4DD8-B645-6CB481CEB7BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "", @"Should a record of this communication be saved to the recipient's profile", 12, @"False", "E571662E-8343-4F3F-B118-4E6339B895F5" );
            // Attrib for BlockType: Calendar Event Item Occurrence List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "46647F7B-2DC3-43FF-88F7-962F516FA969" );
            // Attrib for BlockType: Registration Instance Detail:Display Discount Codes
            RockMigrationHelper.UpdateBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Discount Codes", "DisplayDiscountCodes", "", @"Display the discount code used with a payment", 9, @"False", "8E41AB04-4FC7-4FFF-8DAC-B51E8153FE94" );
            // Attrib for BlockType: My Connection Opportunities:Show Request Total
            RockMigrationHelper.UpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Request Total", "ShowRequestTotal", "", @"If enabled, the block will show the total number of requests.", 3, @"True", "07A73CAB-25FB-4AA5-9665-9994068FB2A1" );
            // Attrib for BlockType: My Connection Opportunities:Show Last Activity Note
            RockMigrationHelper.UpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Activity Note", "ShowLastActivityNote", "", @"If enabled, the block will show the last activity note for each request in the list.", 4, @"False", "6CAC4307-AC17-4711-B7F7-E911D11E0D27" );
            // Attrib for BlockType: My Connection Opportunities:Connection Request Status Icons Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Connection Request Status Icons Template", "ConnectionRequestStatusIconsTemplate", "", @"Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.", 7, @"
<div class='status-list'>
    {% if ConnectionRequestStatusIcons.IsAssignedToYou %}
    <span class='badge badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'>&nbsp;</span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsUnassigned %}
    <span class='badge badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'>&nbsp;</span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsCritical %}
    <span class='badge badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'>&nbsp;</span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsIdle %}
    <span class='badge badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'>&nbsp;</span> 
    {% endif %}
</div>
", "6650B724-0F01-4671-B8B8-C695EAA0BCD4" );
            // Attrib for BlockType: My Connection Opportunities:Status Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Status Template", "StatusTemplate", "", @"Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.", 5, @"<div class='pull-left badge-legend padding-r-md'>
    <span class='pull-left badge badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'>&nbsp;</span>
    <span class='pull-left badge badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'>&nbsp;</span>
    <span class='pull-left badge badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'>&nbsp;</span>
    <span class='pull-left badge badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'>&nbsp;</span> 
</div>", "AD3951D5-3B1F-473B-B39A-E0BC61B3A228" );
            // Attrib for BlockType: My Connection Opportunities:Opportunity Summary Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Opportunity Summary Template", "OpportunitySummaryTemplate", "", @"Lava Template that can be used to customize what is displayed in each Opportunity Summary. Includes common merge fields plus the OpportunitySummary, ConnectionOpportunity, and its ConnectionRequests.", 6, @"<span class=""item-count"" title=""There are {{ 'active connection' | ToQuantity:OpportunitySummary.TotalRequests }} in this opportunity."">{{ OpportunitySummary.TotalRequests | Format:'#,###,##0' }}</span>
<i class='{{ OpportunitySummary.IconCssClass }}'></i>
<h3>{{ OpportunitySummary.Name }}</h3>
<div class='status-list'>
    <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>
    <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>
    <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>
    <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span>
</div>
", "FE0C3AE0-0496-47C2-989F-BF1AE1090220" );
            // Attrib for BlockType: Connection Request Detail:Badges
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "3F1AE891-7DC8-46D2-865D-11543B34FB60", "Badges", "Badges", "", @"The person badges to display in this block.", 0, @"", "CA1F7347-C077-47AA-8137-9E7233F755D4" );
            // Attrib for BlockType: Connection Opportunity List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "481AE184-4654-48FB-A2B4-90F6604B59B8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "04A9BADD-6A26-4E14-B1A3-E97F1E07ED32" );
            // Attrib for BlockType: Event Item Personalized Registration:Start Registration At Beginning
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A1FFACC-D74C-4061-B6A7-34150C462DB7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start Registration At Beginning", "StartRegistrationAtBeginning", "", @"Should the registration start at the beginning (true) or start at the confirmation page (false). This will depend on whether you would like the registrar to have to walk through the registration process to complete any required items.", 4, @"True", "A8E5B587-EA35-428D-BADA-17B975287440" );
            // Attrib for BlockType: Event Item Personalized Registration:Include Family Members
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A1FFACC-D74C-4061-B6A7-34150C462DB7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Family Members", "IncludeFamilyMembers", "", @"Lists family members of the individual to select for registration.", 0, @"True", "AA7FE4CF-2B8D-4675-9C45-A3FCE5DB5728" );
            // Attrib for BlockType: Event Item Personalized Registration:Days In Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A1FFACC-D74C-4061-B6A7-34150C462DB7", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Days In Range", "DaysInRange", "", @"The number of days in the future to show events for.", 1, @"60", "65D709EE-D22B-40F5-BB14-8E4BBEFD3F1E" );
            // Attrib for BlockType: Event Item Personalized Registration:Max Display Events
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A1FFACC-D74C-4061-B6A7-34150C462DB7", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Display Events", "MaxDisplayEvents", "", @"The maximum number of events to display.", 2, @"4", "0C50F49C-0020-4F34-94F0-034061047D4C" );
            // Attrib for BlockType: Event Item Personalized Registration:Registration Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A1FFACC-D74C-4061-B6A7-34150C462DB7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", @"The registration page to redirect to.", 3, @"", "ADB430B7-D8C7-47B7-9295-DBA4014DC398" );
            // Attrib for BlockType: Communication Recipient List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "EBEA5996-5695-4A42-A21C-29E11E711BE8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "968D934B-67FB-4034-84BD-A8AD1ABB7292" );
            // Attrib for BlockType: Event List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "6F0BF602-1D05-4643-BD6B-3DC60C1ABCA3" );
            // Attrib for BlockType: Suggestion List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "19BC2EB2-7AEA-49C2-B161-9B6AB5694E7C" );
            // Attrib for BlockType: Person Suggestion Notice:Show Followers Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "983B9EBE-BDD9-49A6-87FF-7E1A585E97E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Followers Page", "ShowFollowersPage", "", @"Determines whether the link to the followers page should be shown", 2, @"True", "621305C7-9328-4D18-8E19-0B8DE908BD7C" );
            // Attrib for BlockType: Person Suggestion List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "3726D4D5-EAA4-4AB7-A2BC-DDE8199E16FA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "8DC85452-996A-4401-8F1B-8AC7BA49A316" );
            // Attrib for BlockType: Saved Account List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "CE9F1E41-33E6-4FED-AA08-BD9DCA061498", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "AD82E9F0-5383-41C5-BFA0-F9D53827A3AE" );
            // Attrib for BlockType: Person Merge Request List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "4CBFB5FC-0174-489A-8B95-90BB8FAA2144", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "AE8565D3-2BD0-4B37-82C9-44E98264EA22" );
            // Attrib for BlockType: Calendar Item List Lava:Use Campus Context
            RockMigrationHelper.UpdateBlockTypeAttribute( "6DF11547-8757-4305-BC9A-122B9D929342", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "", @"Determine if the campus should be read from the campus context of the page.", 2, @"False", "6B326419-50F7-40AC-9FA4-83B08276C507" );
            // Attrib for BlockType: Calendar Item List Lava:Max Occurrences
            RockMigrationHelper.UpdateBlockTypeAttribute( "6DF11547-8757-4305-BC9A-122B9D929342", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "", @"The maximum number of occurrences to show.", 5, @"100", "23D0097D-FF1D-42A4-942E-AFF687F423BD" );
            // Attrib for BlockType: Calendar Item List Lava:Details Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "6DF11547-8757-4305-BC9A-122B9D929342", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Details Page", "DetailsPage", "", @"Detail page for events", 3, @"", "42C047D3-B673-4160-A0B8-E7FE0289B17E" );
            // Attrib for BlockType: Calendar Item List Lava:Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "6DF11547-8757-4305-BC9A-122B9D929342", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 1, @"", "25F13A68-83CF-4B9F-B2A7-1D0A9083BD1D" );
            // Attrib for BlockType: Calendar Item List Lava:Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "6DF11547-8757-4305-BC9A-122B9D929342", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "", @"Optional date range to filter the items on. (defaults to next 1000 days)", 4, @",", "92CFA95B-D44D-4538-8B93-954A99763E1A" );
            // Attrib for BlockType: Calendar Item List Lava:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "6DF11547-8757-4305-BC9A-122B9D929342", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"The lava template to use for the results", 6, @"{% include '~~/Assets/Lava/EventItemList.lava' %}", "626C25A8-D0BC-4F1D-A91B-A566AF4E6110" );
            // Attrib for BlockType: Calendar Item List Lava:Event Calendar
            RockMigrationHelper.UpdateBlockTypeAttribute( "6DF11547-8757-4305-BC9A-122B9D929342", "EC0D9528-1A22-404E-A776-566404987363", "Event Calendar", "EventCalendar", "", @"The event calendar to be displayed", 0, @"1", "BBD2DDE8-EDA0-4A54-8895-F56D55D6A450" );
            // Attrib for BlockType: Event Item Occurrence List Lava:Event Item
            RockMigrationHelper.UpdateBlockTypeAttribute( "3ABC7007-CE3E-4092-900F-C907948CA8C2", "23713932-F558-45F7-BB00-2A550852F70D", "Event Item", "EventItem", "", @"The event item to use to display occurrences for.", 0, @"", "8B1DC0D0-0382-405F-AB27-FB5D41C5F4FF" );
            // Attrib for BlockType: Event Item Occurrence List Lava:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "3ABC7007-CE3E-4092-900F-C907948CA8C2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"The lava template to use for the results", 6, @"{% include '~~/Assets/Lava/EventItemOccurrenceList.lava' %}", "5BA2E558-DD59-49AC-85A5-33BD8261F8D7" );
            // Attrib for BlockType: Event Item Occurrence List Lava:Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "3ABC7007-CE3E-4092-900F-C907948CA8C2", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "", @"Optional date range to filter the occurrences on.", 3, @",", "D25CC696-9F8D-4D73-A213-1AABBE589045" );
            // Attrib for BlockType: Event Item Occurrence List Lava:Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "3ABC7007-CE3E-4092-900F-C907948CA8C2", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 1, @"", "37A8CC1D-D2A5-4F6C-863D-B148DE92AB9A" );
            // Attrib for BlockType: Event Item Occurrence List Lava:Registration Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "3ABC7007-CE3E-4092-900F-C907948CA8C2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", @"The page to use for registrations.", 5, @"", "6419C88C-62C5-4F1C-B18C-18F4A60A6F9E" );
            // Attrib for BlockType: Event Item Occurrence List Lava:Max Occurrences
            RockMigrationHelper.UpdateBlockTypeAttribute( "3ABC7007-CE3E-4092-900F-C907948CA8C2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "", @"The maximum number of occurrences to show.", 4, @"100", "A4DA4033-C353-48F8-BBCF-15FE5E7B556F" );
            // Attrib for BlockType: Event Item Occurrence List Lava:Use Campus Context
            RockMigrationHelper.UpdateBlockTypeAttribute( "3ABC7007-CE3E-4092-900F-C907948CA8C2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "", @"Determine if the campus should be read from the campus context of the page.", 2, @"False", "CD7A53BC-7528-4E85-944E-B5E79AE2397E" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Use Campus Context
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "", @"Determine if the campus should be read from the campus context of the page.", 3, @"False", "4C165D71-58D9-4104-A1DE-BB71A8847599" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Max Occurrences
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Occurrences", "MaxOccurrences", "", @"The maximum number of occurrences to show.", 5, @"100", "2B8656F2-036A-48D4-B08D-13777B5CA8C3" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:List Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "List Title", "ListTitle", "", @"The title to make available in the lava.", 0, @"Upcoming Events", "382EDB07-94FF-4CF0-BD07-A5B807BED1ED" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Registration Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", @"The page to use for registrations.", 7, @"", "1C26B933-5BB7-4CA9-9804-05FDF4474FF2" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Event Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "EventDetailPage", "", @"The page to use for showing event details.", 6, @"", "F6FC53C3-BC30-489E-B507-301B225B3567" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Audience
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "", @"The audience to show calendar items for.", 0, @"", "13507A3D-9130-4891-9EB3-27DF81155741" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "", @"List of which campuses to show occurrences for. This setting will be ignored if 'Use Campus Context' is enabled.", 2, @"", "F5BE82EB-AB56-434C-BF53-83B5F9801BB2" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "", @"Optional date range to filter the occurrences on.", 4, @",", "C14C7C6B-CA37-4D9A-BC56-9514C311B436" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"The lava template to use for the results", 8, @"{% include '~~/Assets/Lava/EventItemOccurrenceListByAudience.lava' %}", "9755F457-8185-49FE-861E-9F87A1D5AC02" );
            // Attrib for BlockType: Event Item Occurrence List By Audience Lava:Calendar
            RockMigrationHelper.UpdateBlockTypeAttribute( "E4703964-7717-4C93-BD40-7DFF85EAC5FD", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "", @"Filters the events by a specific calendar.", 1, @"", "0B4E19CD-882B-4F03-A232-9FB6338951E5" );
            // Attrib for BlockType: Prayer Request List Lava:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF0B20C3-B969-4246-81CD-76CC443CFDEB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"Lava template to use to display content", 2, @"
<div class='panel panel-block'> 
    <div class='panel-heading'>
       <h4 class='panel-title'>Prayer Requests</h4>
    </div>
    <div class='panel-body'>

        <ul>
        {% for prayerrequestitem in PrayerRequestItems %}
            {% if LinkUrl != '' %}
                <li>{{ prayerrequestitem.EnteredDateTime | Date:'M/d/yyyy'}} - <a href='{{ LinkUrl | Replace:'[Id]',prayerrequestitem.Id }}'>{{ prayerrequestitem.Text }}</a></li>
            {% else %}
                <li>{{ prayerrequestitem.EnteredDateTime | Date:'M/d/yyyy'}} - {{ prayerrequestitem.Text }}</li>
            {% endif %}
        {% endfor %}
        </ul>
        
    </div>
</div>", "F012A4B5-A3B0-4DCB-9B09-94FD0E9E7B44" );
            // Attrib for BlockType: Prayer Request List Lava:Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF0B20C3-B969-4246-81CD-76CC443CFDEB", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "", @"Date range to limit by.", 7, @"", "FA650803-C7A8-4BE1-97A9-33A828DD7014" );
            // Attrib for BlockType: Prayer Request List Lava:Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF0B20C3-B969-4246-81CD-76CC443CFDEB", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Category", "Category", "", @"The category (or parent category) to limit the listed prayer requests to.", 0, @"", "73FF3D0F-2BAC-45B1-AFEA-4D2AE49AEE9B" );
            // Attrib for BlockType: Prayer Request List Lava:Prayer Request Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF0B20C3-B969-4246-81CD-76CC443CFDEB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Prayer Request Detail Page", "PrayerRequestDetailPage", "", @"The Page Request Detail Page to use for the LinkUrl merge field.  The LinkUrl field will include a [Id] which can be replaced by the prayerrequestitem.Id.", 1, @"", "D16C82CA-4737-43F2-AB70-DD6BA62FFA58" );
            // Attrib for BlockType: Prayer Request List Lava:Sort by
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF0B20C3-B969-4246-81CD-76CC443CFDEB", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Sort by", "Sortby", "", @"", 4, @"0", "06259A07-426A-472A-9C82-D2666FBFEA2D" );
            // Attrib for BlockType: Prayer Request List Lava:Approval Status
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF0B20C3-B969-4246-81CD-76CC443CFDEB", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Approval Status", "ApprovalStatus", "", @"Which statuses to display.", 5, @"1", "16D839F3-A38A-4F43-8A29-13B0DD2D8686" );
            // Attrib for BlockType: Prayer Request List Lava:Max Results
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF0B20C3-B969-4246-81CD-76CC443CFDEB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Results", "MaxResults", "", @"The maximum number of results to display.", 3, @"100", "CDB81ED9-38B0-45B7-977E-6FECEC867AF7" );
            // Attrib for BlockType: Prayer Request List Lava:Show Expired
            RockMigrationHelper.UpdateBlockTypeAttribute( "AF0B20C3-B969-4246-81CD-76CC443CFDEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Expired", "ShowExpired", "", @"Includes expired prayer requests.", 6, @"False", "6E74598C-6C3E-46BC-956F-23DDF0268611" );
            // Attrib for BlockType: Package Rating:Package Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "5A7C11C2-4E9F-4AF6-8149-CB2093CE9727", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Package Detail Page", "PackageDetailPage", "", @"Page reference to use for the package detail page.", 1, @"", "6B58FE72-106B-4B63-8F28-E17D291CE02E" );
            // Attrib for BlockType: Schedule Context Setter:Schedule Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "6553821F-9667-4576-924F-DAF1BB3F3223", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedule Group", "ScheduleGroup", "", @"Choose a schedule group to populate the dropdown", 1, @"", "59D027D2-DF4A-4ED2-8E45-07DEAAA84011" );
            // Attrib for BlockType: Schedule Context Setter:Context Scope
            RockMigrationHelper.UpdateBlockTypeAttribute( "6553821F-9667-4576-924F-DAF1BB3F3223", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Context Scope", "ContextScope", "", @"The scope of context to set", 0, @"Site", "6CF2F93F-0EDA-4B9E-A318-4DE4EBA03B1D" );
            // Attrib for BlockType: Schedule Context Setter:Display Query Strings
            RockMigrationHelper.UpdateBlockTypeAttribute( "6553821F-9667-4576-924F-DAF1BB3F3223", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Query Strings", "DisplayQueryStrings", "", @"Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", 5, @"False", "DF5A76AB-BA53-4566-B941-D52A9AB6A964" );
            // Attrib for BlockType: Schedule Context Setter:Dropdown Item Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "6553821F-9667-4576-924F-DAF1BB3F3223", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Dropdown Item Template", "DropdownItemTemplate", "", @"Lava template for items in the dropdown. The only merge field is {{ ScheduleName }}.", 2, @"{{ ScheduleName }}", "D83C62EE-C0BE-4877-A495-FD61B452532F" );
            // Attrib for BlockType: Schedule Context Setter:No Schedule Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "6553821F-9667-4576-924F-DAF1BB3F3223", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Schedule Text", "NoScheduleText", "", @"The text to show when there is no schedule in the context.", 3, @"Select Schedule", "E8170685-5B31-4AAF-BCE2-28670A566EF6" );
            // Attrib for BlockType: Schedule Context Setter:Clear Selection Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "6553821F-9667-4576-924F-DAF1BB3F3223", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Clear Selection Text", "ClearSelectionText", "", @"The text displayed when a schedule can be unselected. This will not display when the text is empty.", 4, @"", "BB5EEB18-1B0A-486E-888D-CCB2D9EFADB6" );
            // Attrib for BlockType: Schedule Context Setter:Current Item Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "6553821F-9667-4576-924F-DAF1BB3F3223", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Current Item Template", "CurrentItemTemplate", "", @"Lava template for the current item. The only merge field is {{ ScheduleName }}.", 2, @"{{ ScheduleName }}", "2BFE90F4-49BD-4FDE-ABE0-87CEAB063780" );
            // Attrib for BlockType: Request List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "A25BE440-6A54-4A8C-9359-74DB5AE7E5F3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "A72A16E3-A425-4671-BF96-DA7F9F366B57" );
            // Attrib for BlockType: Dynamic Chart:Query Params
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Query Params", "QueryParams", "", @"The parameters that the stored procedure expects in the format of 'param1=value;param2=value'. Any parameter with the same name as a page parameter (i.e. querystring, form, or page route) will have it's value replaced with the page's current value. A parameter with the name of 'CurrentPersonId' will have it's value replaced with the currently logged in person's id.", 0, @"", "0D7A45A6-C885-44CD-9FA9-B8F431D943B5" );
            // Attrib for BlockType: Dynamic Chart:Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", @"The title of the widget", 0, @"", "94FCBF87-B633-4F02-887F-A99970129319" );
            // Attrib for BlockType: Dynamic Chart:Subtitle
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "", @"The subtitle of the widget", 1, @"", "3006CAEC-2DBD-4C87-BCA7-5AB23657497E" );
            // Attrib for BlockType: Dynamic Chart:Legend Position
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Legend Position", "LegendPosition", "", @"Select the position of the Legend (corner)", 8, @"ne", "398336FF-64CD-4EB8-A78C-9C9DAEBF68FA" );
            // Attrib for BlockType: Dynamic Chart:Chart Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Chart Type", "ChartType", "", @"", 9, @"Line", "F5626814-8713-420D-A93C-CFE6E89321DF" );
            // Attrib for BlockType: Dynamic Chart:Column Width
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Column Width", "ColumnWidth", "", @"The width of the widget.", 2, @"4", "9B4B5CBF-1E80-4CCA-8B6D-806806038188" );
            // Attrib for BlockType: Dynamic Chart:Chart Height
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Chart Height", "ChartHeight", "", @"", 0, @"200", "BFAF6EB4-5181-4473-A222-3C2F12982956" );
            // Attrib for BlockType: Dynamic Chart:Show Legend
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legend", "ShowLegend", "", @"", 7, @"True", "55D7DEB8-1D23-430B-81EF-D7F42FFAD518" );
            // Attrib for BlockType: Dynamic Chart:Pie Show Labels
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Pie Show Labels", "PieShowLabels", "", @"If this is a pie chart, specify if labels show be shown", 11, @"True", "3B54049D-60A4-4473-BA10-49101E06F150" );
            // Attrib for BlockType: Dynamic Chart:SQL
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQL", "SQL", "", @"The SQL for the datasource. Output columns must be as follows:
<ul>
    <li>Bar or Line Chart
        <ul>
           <li>[SeriesName] : string or numeric </li>
           <li>[DateTime] : DateTime </li>
           <li>[YValue] : numeric </li>
        </ul>
    </li>
    <li>Pie Chart
        <ul>
           <li>[MetricTitle] : string </li>
           <li>[YValueTotal] : numeric </li>
        </ul>
    </li>
</ul>

Example: 
<code><pre>
-- get top 25 viewed pages from the last 30 days (excluding Home)
select top 25  * from (
    select 
        distinct
        pv.PageTitle [SeriesName], 
        convert(date, pv.DateTimeViewed) [DateTime], 
        count(*) [YValue] 
    from 
        PageView pv
    where PageTitle is not null    
    group by pv.PageTitle, convert(date, pv.DateTimeViewed)
    ) x where SeriesID != 'Home' 
and DateTime > DateAdd(day, -30, SysDateTime())
order by YValue desc
</pre>
</code>", 0, @"", "C95DFED0-D082-4D47-A4C0-ADD8664A9CA8" );
            // Attrib for BlockType: Dynamic Chart:Chart Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Chart Style", "ChartStyle", "", @"", 3, @"", "BAAE0B29-7D8C-4369-93F2-E1B624882170" );
            // Attrib for BlockType: Dynamic Chart:Pie Inner Radius
            RockMigrationHelper.UpdateBlockTypeAttribute( "7BCCBFB0-26A5-4376-B1F3-DC6ADD7C3723", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Pie Inner Radius", "PieInnerRadius", "", @"If this is a pie chart, specific the inner radius to have a donut hole. For example, specify: 0.75 to have the inner radius as 75% of the outer radius.", 10, @"0", "4CAF4C44-586D-490E-918D-52C0BD81BB3C" );
            // Attrib for BlockType: Dynamic Heat Map:Point Grouping
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAFBB883-D0B4-498E-91EE-CAC5652E5095", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Point Grouping", "PointGrouping", "", @"The number of miles per to use to group points that are close together. For example, enter 0.25 to group points in 1/4 mile blocks. Increase this if the heatmap has lots of points and is slow", 6, @"", "E81CC664-8867-4F74-A906-9005EFEEC9CE" );
            // Attrib for BlockType: Dynamic Heat Map:Map Style
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAFBB883-D0B4-498E-91EE-CAC5652E5095", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", @"The map theme that should be used for styling the map.", 3, @"BFC46259-FB66-4427-BF05-2B030A582BEA", "CF4378E1-DFFA-4244-8549-958492DC3A73" );
            // Attrib for BlockType: Dynamic Heat Map:Show Save Location
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAFBB883-D0B4-498E-91EE-CAC5652E5095", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Location", "ShowSaveLocation", "", @"Adds a button which will save the selected shape as a named location's geofence ", 9, @"False", "381AE6E6-68F1-42B4-BC90-3B367D2873C4" );
            // Attrib for BlockType: Dynamic Heat Map:Show Pie Slicer
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAFBB883-D0B4-498E-91EE-CAC5652E5095", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Pie Slicer", "ShowPieSlicer", "", @"Adds a button which will help slice a circle into triangular pie slices. To use, draw or click on a circle, then click the Pie Slicer button.", 8, @"False", "D9DDFEDB-1258-481F-80A2-EFBFAC7D0EFD" );
            // Attrib for BlockType: Dynamic Heat Map:Map Height
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAFBB883-D0B4-498E-91EE-CAC5652E5095", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Map Height", "MapHeight", "", @"Height of the map in pixels (default value is 600px)", 4, @"600", "F851E238-E7BE-4F87-BDD9-1CEE79173830" );
            // Attrib for BlockType: Dynamic Heat Map:Label Font Size
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAFBB883-D0B4-498E-91EE-CAC5652E5095", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Label Font Size", "LabelFontSize", "", @"Select the Font Size for the map labels", 7, @"24", "1D66B49B-4C0C-48CD-A6D1-F01F0781BFC2" );
            // Attrib for BlockType: Dynamic Heat Map:DataView
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAFBB883-D0B4-498E-91EE-CAC5652E5095", "9C204CD0-1233-41C5-818A-C5DA439445AA", "DataView", "DataView", "", @"The dataview to filter the people shown on the map. Leave blank to have it determined by the user or by page param", 0, @"", "C957499A-125F-477B-B014-DA27F9B33136" );
            // Attrib for BlockType: Dynamic Heat Map:Polygon Colors
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAFBB883-D0B4-498E-91EE-CAC5652E5095", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Polygon Colors", "PolygonColors", "", @"Comma-Delimited list of colors to use when displaying multiple polygons (e.g. #f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc).", 5, @"#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc", "1E868AD9-2A3B-4AC2-92C6-D405FE420886" );
            // Attrib for BlockType: Content Channel Item Personal List Lava:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "13E4D4B5-0929-4ED6-9E59-05A6D511FA06", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"Page reference to the detail page. This will be included as a variable in the Lava.", 2, @"", "46D8E92A-D2F8-47EC-8351-D7EB331447A7" );
            // Attrib for BlockType: Content Channel Item Personal List Lava:Max Items
            RockMigrationHelper.UpdateBlockTypeAttribute( "13E4D4B5-0929-4ED6-9E59-05A6D511FA06", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Items", "MaxItems", "", @"The maximum number of items to display (default 10)", 1, @"10", "5D8F57CC-B1CE-4FBA-8006-75DDF681DB51" );
            // Attrib for BlockType: Content Channel Item Personal List Lava:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "13E4D4B5-0929-4ED6-9E59-05A6D511FA06", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"The Lava template to use.", 3, @"{% assign itemCount = Items | Size %}

{% if itemCount > 0 %}
    <div class='panel panel-default'> 
        <div class='panel-heading'>
           <h5 class='panel-title'>Content Channel Items for {{ CurrentPerson.FullName }}</h5>
        </div>
        <div class='panel-body'>
            <ul>
                {% for item in Items %}
                <li>
                    {% if DetailPage != '' %}
                        <a href = '{{ DetailPage }}?contentItemId={{ item.Id }}' >{{ item.Title }}</a>
                    {% else %}
                        {{ item.Title }}
                    {% endif %}
                    
                    {% case item.Status %}
                        {% when 'PendingApproval' %}
                            <span class='label label-warning'>Pending</span>
                        {% when 'Approved' %}
                            <span class='label label-success'>Approved</span>
                        {% when 'Denied' %}
                            <span class='label label-danger'>Denied</span>
                    {% endcase %}
                </li>
                {% endfor %}
            <ul>
        </div>
    </div>
{% endif %}", "A182E290-9537-4F39-AF2D-76E17FF9EE60" );
            // Attrib for BlockType: Content Channel Item Personal List Lava:Content Channel
            RockMigrationHelper.UpdateBlockTypeAttribute( "13E4D4B5-0929-4ED6-9E59-05A6D511FA06", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D", "Content Channel", "ContentChannel", "", @"The content channel to filter on. If blank all content items for the user will be displayed.", 0, @"", "CD159016-F69E-426B-8004-AB781FC2F66E" );
            // Attrib for BlockType: Group Member RemoveFrom URL:Success Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "0159CE20-7B41-4D53-985C-81877ED75767", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Message", "SuccessMessage", "", @"Lava template to display when person has been added to the group.", 1, @"<div class='alert alert-success'>
    {{ Person.NickName }} has been removed from the group '{{ Group.Name }}'.
</div>", "3036C6EF-A58B-42EC-ABE0-6DDAFAEF1CAA" );
            // Attrib for BlockType: Group Member RemoveFrom URL:Not In Group Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "0159CE20-7B41-4D53-985C-81877ED75767", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Not In Group Message", "NotInGroupMessage", "", @"Lava template to display when person is not in the group.", 2, @"<div class='alert alert-warning'>
    {{ Person.NickName }} was not in the group '{{ Group.Name }}'.
</div>", "6B338FAA-FD37-4799-B8A5-3299E525E73A" );
            // Attrib for BlockType: Group Member RemoveFrom URL:Default Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "0159CE20-7B41-4D53-985C-81877ED75767", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Default Group", "DefaultGroup", "", @"The default group to use if one is not passed through the query string (optional).", 0, @"", "3AB2127B-70A4-4EAE-AE5C-7EB2D1D29C62" );
            // Attrib for BlockType: Group Member RemoveFrom URL:Inactivate Instead of Remove
            RockMigrationHelper.UpdateBlockTypeAttribute( "0159CE20-7B41-4D53-985C-81877ED75767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Inactivate Instead of Remove", "Inactivate", "", @"Inactivates the person in the group instead of removing them.", 4, @"False", "2BA25BF4-DF84-4778-862A-DC551CABD572" );
            // Attrib for BlockType: Group Member RemoveFrom URL:Warn When Not In Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "0159CE20-7B41-4D53-985C-81877ED75767", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Warn When Not In Group", "WarnWhenNotInGroup", "", @"Determines if the 'Not In Group Message'should be shown if the person is not in the group. Otherwise the success message will be shown", 3, @"True", "B7F11BFF-203A-4F54-BA94-2E6647264C5D" );
            // Attrib for BlockType: Person Directory:Max Results
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Results", "MaxResults", "", @"The maximum number of results to show on the page.", 14, @"1500", "6EC21276-BDB2-4E5B-99F3-37104B4AD82A" );
            // Attrib for BlockType: Person Directory:Show Grade
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grade", "ShowGrade", "", @"Should grade be included in the directory?", 12, @"False", "16091C4E-6206-444F-B239-A28F58B8D78B" );
            // Attrib for BlockType: Person Directory:Show Envelope Number
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Envelope Number", "ShowEnvelopeNumber", "", @"Should envelope # be included in the directory?", 13, @"False", "789C82A6-0758-4512-A4DA-F7E85F1750AB" );
            // Attrib for BlockType: Person Directory:Person Profile Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", @"Page to navigate to when clicking a person's name (leave blank if link should not be enabled).", 4, @"", "3DC389F3-C229-4CC0-B7D5-AF9AAB7EA38E" );
            // Attrib for BlockType: Signature Document List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "256F6FDB-B241-4DE6-9C38-0E9DA0270A22", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "103193BC-F659-4239-88C1-0717782F2A8B" );
            // Attrib for BlockType: Signature Document Template Detail:Default Invite Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Default Invite Email", "DefaultInviteEmail", "", @"The default system email to use when creating new document types.", 1, @"791F2DE4-5A59-60AE-4F2F-FDC3EBC4FFA9", "4138C4E5-DFD0-4D25-813B-B35407644265" );
            // Attrib for BlockType: Signature Document Template List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E413152-B790-4EC2-84A9-9B48D2717D63", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "85E06C30-165C-4EA7-8370-5EC2A9467BCA" );
            // Attrib for BlockType: Schedule Category Exclusion List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "ACF84335-34A1-4DD6-B242-20119B8D0967", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "99B758DD-807C-4B19-8D38-86C33A702243" );
            // Attrib for BlockType: Schedule Category Exclusion List:Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "ACF84335-34A1-4DD6-B242-20119B8D0967", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "", @"Optional Category to use (if not specified, query will be determined by query string).", 0, @"", "AD00F2CD-7BD7-484C-931A-9629504E51C4" );
            // Attrib for BlockType: Campus Schedule Context Setter:Schedule Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B", "Schedule Group", "ScheduleGroup", "", @"Choose a schedule group to populate the dropdown", 5, @"", "129D0661-1699-494B-9F2C-0E71FD2E267C" );
            // Attrib for BlockType: Campus Schedule Context Setter:Schedule Current Item Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Schedule Current Item Template", "ScheduleCurrentItemTemplate", "", @"Lava template for the current item. The only merge field is {{ ScheduleName }}.", 6, @"{{ ScheduleName }}", "59772BA5-8507-447C-8F03-06BBF8EA9C87" );
            // Attrib for BlockType: Campus Schedule Context Setter:Schedule Clear Selection Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Schedule Clear Selection Text", "ScheduleClearSelectionText", "", @"The text displayed when a schedule can be unselected. This will not display when the text is empty.", 9, @"All Schedules", "28025B31-9BCC-4AD3-8334-1779F3FA7F59" );
            // Attrib for BlockType: Campus Schedule Context Setter:Campus Current Item Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Current Item Template", "CampusCurrentItemTemplate", "", @"Lava template for the current item. The only merge field is {{ CampusName }}.", 1, @"{{ CampusName }}", "0662FD49-D816-4FBB-AD17-937CB2741F48" );
            // Attrib for BlockType: Campus Schedule Context Setter:Campus Dropdown Item Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Dropdown Item Template", "CampusDropdownItemTemplate", "", @"Lava template for items in the dropdown. The only merge field is {{ CampusName }}.", 2, @"{{ CampusName }}", "8696B2B7-86A4-4990-850E-9A089F935D22" );
            // Attrib for BlockType: Campus Schedule Context Setter:No Campus Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Campus Text", "NoCampusText", "", @"The text displayed when no campus context is selected.", 3, @"Select Campus", "9B3E7EFD-8993-4C55-900E-895B1EBB59D8" );
            // Attrib for BlockType: Campus Schedule Context Setter:Campus Clear Selection Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Campus Clear Selection Text", "CampusClearSelectionText", "", @"The text displayed when a campus can be unselected. This will not display when the text is empty.", 4, @"All Campuses", "034DA9BA-52B4-4ECE-894D-EDEDA8AB3A3A" );
            // Attrib for BlockType: Campus Schedule Context Setter:Schedule Dropdown Item Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Schedule Dropdown Item Template", "ScheduleDropdownItemTemplate", "", @"Lava template for items in the dropdown. The only merge field is {{ ScheduleName }}.", 7, @"{{ ScheduleName }}", "EA73DBE0-3B12-4036-BA44-01EC5C917865" );
            // Attrib for BlockType: Campus Schedule Context Setter:No Schedule Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Schedule Text", "NoScheduleText", "", @"The text to show when there is no schedule in the context.", 8, @"All Schedules", "359F9CB8-1CDF-4DA5-B482-1C61CCA55D8D" );
            // Attrib for BlockType: Campus Schedule Context Setter:Display Query Strings
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Query Strings", "DisplayQueryStrings", "", @"Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", 10, @"False", "D267B0CD-6AFA-4A5A-8CE8-919DC5E414CB" );
            // Attrib for BlockType: Campus Schedule Context Setter:Default To Current User's Campus
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Current User's Campus", "DefaultToCurrentUser", "", @"Will use the campus of the current user if no context is provided.", 11, @"False", "5CC72668-CDF9-452A-A04B-EB11064F189B" );
            // Attrib for BlockType: Campus Schedule Context Setter:Context Scope
            RockMigrationHelper.UpdateBlockTypeAttribute( "A0405364-C722-495B-879C-C57B5BC5E213", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Context Scope", "ContextScope", "", @"The scope of context to set", 0, @"Site", "F3F4252C-546A-47CE-B39A-B622205D1DD6" );
            // Attrib for BlockType: Date Range Context Setter:Display Query Strings
            RockMigrationHelper.UpdateBlockTypeAttribute( "ABC4A04E-6FA8-4817-8113-A653251A16B3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Query Strings", "DisplayQueryStrings", "", @"Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", 2, @"False", "F9D11FCA-2EFF-4EF8-A8AD-E430F98E106D" );
            // Attrib for BlockType: Date Range Context Setter:No Date Range Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "ABC4A04E-6FA8-4817-8113-A653251A16B3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Date Range Text", "NoDateRangeText", "", @"The text to show when there is no date range in the context.", 0, @"Select Date Range", "D2438956-B36F-43D6-BB5B-C456CEBE7EE0" );
            // Attrib for BlockType: Date Range Context Setter:Default Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "ABC4A04E-6FA8-4817-8113-A653251A16B3", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Date Range", "DefaultDateRange", "", @"The default range to start with if context and query string have not been set", 1, @",", "14C55739-7D14-47B4-A32E-DC322776A1DA" );
            // Attrib for BlockType: Person Context Setter:Group
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA33088D-7B18-46A7-ADFA-B9DA9512B4A4", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Group", "Group", "", @"The Group to use as the source of people to select from", 2, @"", "C7080FE9-3D84-4FF9-ACD7-81D3B5D38472" );
            // Attrib for BlockType: Person Context Setter:No Person Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA33088D-7B18-46A7-ADFA-B9DA9512B4A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Person Text", "NoPersonText", "", @"The text to show when there is no person in the context.", 3, @"Select Person", "E547ABC4-9D0F-4DE1-92FE-7741FF8D1594" );
            // Attrib for BlockType: Person Context Setter:Clear Selection Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA33088D-7B18-46A7-ADFA-B9DA9512B4A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Clear Selection Text", "ClearSelectionText", "", @"The text displayed when a person can be unselected. This will not display when the text is empty.", 4, @"", "DE1486CE-F437-411B-9876-2747C5C0E4A4" );
            // Attrib for BlockType: Person Context Setter:Display Query Strings
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA33088D-7B18-46A7-ADFA-B9DA9512B4A4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Query Strings", "DisplayQueryStrings", "", @"Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", 5, @"False", "BBE58898-3BED-4E52-972D-FA2542072096" );
            // Attrib for BlockType: Person Context Setter:Context Scope
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA33088D-7B18-46A7-ADFA-B9DA9512B4A4", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Context Scope", "ContextScope", "", @"The scope of context to set", 1, @"Page", "FFBCD832-D043-44E5-94FD-E9A73C170663" );
            // Attrib for BlockType: Person Attribute Forms:Save Values
            RockMigrationHelper.UpdateBlockTypeAttribute( "5464F6B3-E0E4-4F9F-8FBA-44A18DB83F44", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Save Values", "SaveValues", "", @"", 0, @"END", "37D4E976-8228-41EA-9D89-2296375FAA2E" );
            // Attrib for BlockType: Person Attribute Forms:Display Progress Bar
            RockMigrationHelper.UpdateBlockTypeAttribute( "5464F6B3-E0E4-4F9F-8FBA-44A18DB83F44", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Progress Bar", "DisplayProgressBar", "", @"Determines if the progress bar should be show if there is more than one form.", 0, @"True", "B14B6FF2-B981-4BE3-8B47-180D3A6BAFB5" );
            // Attrib for BlockType: Person Attribute Forms:Forms
            RockMigrationHelper.UpdateBlockTypeAttribute( "5464F6B3-E0E4-4F9F-8FBA-44A18DB83F44", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Forms", "Forms", "", @"The forms to show.", 0, @"", "598BAB2B-AAEE-4DDF-ACAA-2B686285976A" );
            // Attrib for BlockType: Person Attribute Forms:Done Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "5464F6B3-E0E4-4F9F-8FBA-44A18DB83F44", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Done Page", "DonePage", "", @"The page to redirect to when done.", 0, @"", "B892FD34-2AE5-42A0-88F0-2E2B32EDED2C" );
            // Attrib for BlockType: Person Attribute Forms:Workflow
            RockMigrationHelper.UpdateBlockTypeAttribute( "5464F6B3-E0E4-4F9F-8FBA-44A18DB83F44", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", @"The workflow to be launched when complete.", 0, @"", "5CB775CC-FBB7-4D6D-AD72-E89473B6231D" );
            // Attrib for BlockType: Logout:Redirect Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCB87054-8AA3-4F44-AA48-19BD028C4190", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect Page", "RedirectPage", "", @"The page to redirect the user to.", 0, @"", "81C96E97-4FD9-40AA-B3CC-5EBA813630A5" );
            // Attrib for BlockType: Logout:Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCB87054-8AA3-4F44-AA48-19BD028C4190", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Message", "Message", "", @"The message to display if no redrect page was provided.", 1, @"<div class=""alert alert-success"">You have been logged out.</div>", "A7F6BC82-6CEA-457C-9D56-A07D89FBDF8F" );
            // Attrib for BlockType: Stark Dynamic Attributes:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "7C34A0FA-ED0D-4B8B-B458-6EC970711726", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"The Lava Template", 0, @"
<strong>Additional Attributes can be defined for this block type. These can be configured in <code>Home / CMS Configuration / Block Types</code> and will show up in block settings</strong>

<ul>
{% for attribute in Block.AttributeValues %}
    {% if attribute.AttributeKey != 'LavaTemplate' %}
    <li>{{ attribute.AttributeKey }}</li>
    {% endif %}
{% endfor %}
</ul>

", "D98F4865-CBDA-415B-B6DD-3E7FF55FA4BB" );
            // Attrib for BlockType: Transaction Entity Matching:Max Number of Results
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Number of Results", "MaxNumberofResults", "", @"", 4, @"1000", "019A6CE9-1101-43C1-89B5-CB0B1AFDF00E" );
            // Attrib for BlockType: Transaction Entity Matching:Entity Column Heading
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Column Heading", "EntityColumnHeading", "", @"Set a column heading, or leave blank to have it based on the EntityType selection", 1, @"", "FC85D75B-C1C1-460E-BA41-1DDBB8F567CA" );
            // Attrib for BlockType: Transaction Entity Matching:LimitToActiveGroups
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "9C204CD0-1233-41C5-818A-C5DA439445AA", "LimitToActiveGroups", "LimitToActiveGroups", "", @"", 0, @"", "0805A4FF-74FB-4A4E-8067-30D1B750B9EF" );
            // Attrib for BlockType: Transaction Entity Matching:Panel Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Panel Title", "PanelTitle", "", @"Set a specific title, or leave blank to have it based on the EntityType selection", 0, @"", "EAEB617B-5096-4797-AA86-0B7722FC7638" );
            // Attrib for BlockType: Transaction Entity Matching:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "9E1A3220-5BC8-4829-913F-1C8F7DB0E7B0" );
            // Attrib for BlockType: Transaction Entity Matching:Show Dataview Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Dataview Filter", "ShowDataviewFilter", "", @"Show a DataView filter that lists Dataviews that are based on Rock.Model.FinancialTranasactionDetail.", 2, @"False", "4AF148E4-50CB-45FA-8FD8-02E2CBEEB43F" );
            // Attrib for BlockType: Transaction Entity Matching:Show Batch Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Batch Filter", "ShowBatchFilter", "", @"", 3, @"True", "C7FAE17F-AF4B-4390-8126-42CEAFED62BE" );
            // Attrib for BlockType: Attribute Matrix Template List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "069554B7-983E-4653-9A28-BA39659C6D63", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "AFE6C4C4-BB30-4A38-A77B-1BBE658D78C2" );
            // Attrib for BlockType: Short Link List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "E2F4DA96-E593-414A-BA6A-AACFE97505D7" );
            // Attrib for BlockType: Short Link Click List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "F6F42BCE-0F4A-4ED6-BDB9-B479135F5741" );
            // Attrib for BlockType: Event Detail with Occurrences Search Lava:Use Campus Context
            RockMigrationHelper.UpdateBlockTypeAttribute( "B7788DFF-783D-40A3-BFD4-EA9561F950A8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "", @"Set this to true to set the campus filter based on the campus context.", 5, @"False", "FA0627A3-FD9F-4D56-9CD9-068102030DB6" );
            // Attrib for BlockType: Event Detail with Occurrences Search Lava:Event Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "B7788DFF-783D-40A3-BFD4-EA9561F950A8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "EventDetailPage", "", @"The page to use for showing event details.", 4, @"", "86036EDB-C670-464A-AC2C-F37B89CB5DC3" );
            // Attrib for BlockType: Event Detail with Occurrences Search Lava:Event Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "B7788DFF-783D-40A3-BFD4-EA9561F950A8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Lava Template", "EventLavaTemplate", "", @"The lava template for display details of the Event", 1, @"
<h1>{{ Event.Name }}</h1>
<p>{{ Event.Description }}</p>
{% if Event.Photo.Guid %}
    <center>
      <img src='/GetImage.ashx?Guid={{ Event.Photo.Guid }}' class='title-image img-responsive'></img>
    </center>
{% endif %}
", "21EE0108-A9C3-449B-B64B-2877394EE6CF" );
            // Attrib for BlockType: Event Detail with Occurrences Search Lava:Results Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "B7788DFF-783D-40A3-BFD4-EA9561F950A8", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Lava Template", "ResultsLavaTemplate", "", @"The lava template for display the results of the search", 2, @"
{% for occurrence in EventItemOccurrences %}
        
    <div class='row margin-b-lg'>
        <div class='col-md-4'>
            <h1>{{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'MMM d' }}</h1>
            <h2>{{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'dddd h:mmtt' }}</h2>
        </div>  
        <div class='col-md-8'>    
            <h3>{{ occurrence.EventItem.Name }}</h3>
            <p>{{ occurrence.EventItem.Description}}</p>                
        </div>
    </div>
{% endfor %}
", "272C4EE1-3A23-4F66-BE4A-F433EA66A075" );
            // Attrib for BlockType: Event Detail with Occurrences Search Lava:Default Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "B7788DFF-783D-40A3-BFD4-EA9561F950A8", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Date Range", "DefaultDateRange", "", @"The Default date range selection", 3, @"Next|10|Week||", "F95343F6-5CEF-47A7-896B-1FCC93E18A57" );
            // Attrib for BlockType: Event Item Occurrences Search Lava:Default Date Range
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Default Date Range", "DefaultDateRange", "", @"The Default date range selection", 3, @"Next|10|Week||", "2F3B07B9-F065-4373-BE32-DE09B2C897EC" );
            // Attrib for BlockType: Event Item Occurrences Search Lava:Results Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Lava Template", "ResultsLavaTemplate", "", @"The lava template for display the results of the search", 2, @"
{% for occurrence in EventItemOccurrences %}
        
    <div class='row margin-b-lg'>
        <div class='col-md-4'>
            <h1>{{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'MMM d' }}</h1>
            <h2>{{ occurrence.Schedule.iCalendarContent | DatesFromICal | First | Date: 'dddd h:mmtt' }}</h2>
        </div>  
        <div class='col-md-8'>    
            <h3>{{ occurrence.EventItem.Name }}</h3>
            <p>{{ occurrence.EventItem.Description}}</p>                
        </div>
    </div>
{% endfor %}
", "3C29CD5E-42DF-40A7-B328-C706563D4114" );
            // Attrib for BlockType: Event Item Occurrences Search Lava:Event Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Event Detail Page", "EventDetailPage", "", @"The page to use for showing event details.", 4, @"", "3887A0E5-4147-4DF2-BA60-8B00660AA8A2" );
            // Attrib for BlockType: Event Item Occurrences Search Lava:Use Campus Context
            RockMigrationHelper.UpdateBlockTypeAttribute( "01CA4723-8290-41C6-A2D2-88469FAA48E9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Campus Context", "UseCampusContext", "", @"Set this to true to set the campus filter based on the campus context.", 5, @"False", "8774133A-CB7D-413A-BA69-A6FB609EDECF" );
            // Attrib for BlockType: Sample React Block:StartingNumber
            RockMigrationHelper.UpdateBlockTypeAttribute( "7DC6C490-E9AF-407C-9716-991564F93FF6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "StartingNumber", "StartingNumber", "", @"Specify a value to start the counter with", 0, @"", "28B3FA52-848B-443F-8680-606BD0B215E4" );
            // Attrib for BlockType: Entity Attribute Values:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "4A89FF55-A6A3-4A9B-8D1D-2ADE092565F5", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "9A64DF98-22D9-43A6-9F92-C83A94AAC896" );
            // Attrib for BlockType: Communication Entry Wizard:Image Binary File Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Image Binary File Type", "ImageBinaryFileType", "", @"The FileType to use for images that are added to the email using the image component", 1, @"60B896C3-F00C-411C-A31C-2D5D4CCBB65F", "3019C448-B687-46CD-84D8-15661F0C56AC" );
            // Attrib for BlockType: Communication Entry Wizard:Attachment Binary File Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "", @"The FileType to use for files that are attached to an sms or email communication", 2, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "23429F70-AC94-4918-BBC9-22B5FAEFD380" );
            // Attrib for BlockType: Communication List Subscribe:Show Medium Preference
            RockMigrationHelper.UpdateBlockTypeAttribute( "52E0AA5B-B08B-42E4-8180-DD7925BAA57F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Medium Preference", "ShowMediumPreference", "", @"Show the user's current medium preference for each list and allow them to change it.", 2, @"True", "3889D217-4081-47A4-80C6-81A2DCB0AEF2" );
            // Attrib for BlockType: Power Bi Report Viewer:Report URL
            RockMigrationHelper.UpdateBlockTypeAttribute( "76A64656-7BAB-4ADC-82DD-9CD207F548F9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Report URL", "ReportUrl", "", @"The URL of the report to display.", 2, @"", "87E942E3-4C73-4572-8A5E-D65D6697F22B" );
            // Attrib for BlockType: Power Bi Report Viewer:GroupId
            RockMigrationHelper.UpdateBlockTypeAttribute( "76A64656-7BAB-4ADC-82DD-9CD207F548F9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "GroupId", "GroupId", "", @"The PowerBI GroupId that the report belongs to", 1, @"", "718A4891-4404-4253-8CF8-66F99105F20F" );
            // Attrib for BlockType: Power Bi Report Viewer:Power BI Account
            RockMigrationHelper.UpdateBlockTypeAttribute( "76A64656-7BAB-4ADC-82DD-9CD207F548F9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Power BI Account", "PowerBiAccount", "", @"The Power BI account to use to retrieve the report.", 0, @"", "9FE23BB7-9FAD-42F3-AA80-7D14AB21850D" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // MP: Control Gallery to CMS Page
            // Remove Block: Rock Control Gallery, from Page: Control Gallery, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "AC070544-072B-413E-8C0E-83583A52DD22" );
            RockMigrationHelper.DeletePage( "706C0584-285F-4014-BA61-EC42C8F6F76B" ); // Page: Control Gallery, Layout: Full Width, Site: Rock RMS


            DropColumn( "dbo.InteractionSession", "DetailTemplate");
            DropColumn("dbo.InteractionSession", "ListTemplate");
            DropColumn("dbo.Interaction", "Content");
            DropColumn("dbo.Interaction", "Campaign");
            DropColumn("dbo.Interaction", "Medium");
            DropColumn("dbo.Interaction", "Source");
            DropColumn("dbo.Interaction", "DetailTemplate");
            DropColumn("dbo.Interaction", "ListTemplate");
            DropColumn("dbo.InteractionChannel", "UsesSession");
            DropColumn("dbo.InteractionChannel", "DetailTemplate");
            DropColumn("dbo.InteractionChannel", "ListTemplate");
            DropColumn("dbo.InteractionComponent", "DetailTemplate");
            DropColumn("dbo.InteractionComponent", "ListTemplate");
            DropColumn("dbo.InteractionComponent", "ComponentSummary");
        }
    }
}
