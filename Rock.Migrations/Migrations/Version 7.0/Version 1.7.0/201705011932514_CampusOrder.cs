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
    public partial class CampusOrder : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Campus", "Order", c => c.Int(nullable: false));

            // MP: Add Prayer Request Attribute Config Page
            RockMigrationHelper.AddPage( true, "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Prayer Request Attributes", "", "C39C3E88-F423-424D-AA21-EB5CA7871A7B", "fa fa-list-ul" ); // Site:Rock RMS
            // Add Block to Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C39C3E88-F423-424D-AA21-EB5CA7871A7B", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Prayer Request Attributes", "Main", @"", @"", 0, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF" );

            // Attrib Value for Block:Prayer Request Attributes, Attribute:Enable Show In Grid Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "920FE120-AD75-4D5C-BFE0-FA5745B1118B", @"True" );
            // Attrib Value for Block:Prayer Request Attributes, Attribute:Enable Ordering Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "5372DFB0-1884-49CD-BB62-BFFCCE33DF86", @"True" );
            // Attrib Value for Block:Prayer Request Attributes, Attribute:Entity Qualifier Column Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106", @"" );
            // Attrib Value for Block:Prayer Request Attributes, Attribute:Entity Qualifier Value Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "FCE1E87D-F816-4AD5-AE60-1E71942C547C", @"" );
            // Attrib Value for Block:Prayer Request Attributes, Attribute:Entity Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "5B33FE25-6BF0-4890-91C6-49FB1629221E", @"f13c8fd2-7702-4c79-a6a9-86440dd5de13" );
            // Attrib Value for Block:Prayer Request Attributes, Attribute:Entity Id Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "CBB56D68-3727-42B9-BF13-0B2B593FB328", @"0" );
            // Attrib Value for Block:Prayer Request Attributes, Attribute:Allow Setting of Values Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "018C0016-C253-44E4-84DB-D166084C5CAD", @"False" );
            // Attrib Value for Block:Prayer Request Attributes, Attribute:Configure Type Page: Prayer Request Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "B6BFDEBD-BBB5-4EC8-A0B2-EB2AEC119FEF", "D4132497-18BE-4D1F-8913-468E33DE63C4", @"True" );

            // DT: Add defined type for Webook to Workflow
            RockMigrationHelper.AddDefinedType( "Global", "Workflow Webhook", "Webhook to activate the Workflow", SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, @"
The following merge fields are available for the ''Process Request, ''Name Template'', and ''Workflow Attributes'' attributes.
<p>
    <a data-toggle=""collapse""  href=""#collapsefields"" class=''btn btn-action btn-xs''>show/hide fields</a>
</p>

<div id = ""collapsefields"" class=""panel-collapse collapse"">
<pre>
{
  ""Url"": ""/"",
  ""RawUrl"": ""http://rock.rocksolidchurchdemo.com/Webhooks/LaunchWorkflow.ashx?Parameter1=Value&Parameter2=Value"",
  ""Method"": ""GET"",
  ""QueryString"": {
    ""Parameter1"": ""Value"",
    ""Parameter2"": ""Value""
  },
  ""RemoteAddress"": ""::1"",
  ""RemoteName"": ""::1"",
  ""ServerName"": ""rock.rocksolidchurchdemo.com"",
  ""RawBody"": """",
  ""Headers"": {
    ""Cache-Control"": ""max-age=0"",
    ""Connection"": ""keep-alive"",
    ""Accept"": ""text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"",
    ""Accept-Encoding"": ""gzip, deflate, sdch, br"",
    ""Accept-Language"": ""en-US,en;q=0.8,en-GB;q=0.6"",
    ""Host"": ""rock.rocksolidchurchdemo.com"",
    ""User-Agent"": ""Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36"",
    ""Upgrade-Insecure-Requests"": ""1""
  },
  ""Cookies"": {
    ""Rock_Context:532"": ""Rock.Model.Campus=EAAAALLGYtXz10XYMREcDquPnteKNSmuZDvWY20tGc2SWY8cE40LWlZbq8nnKF8PrxPZ0890J75S/WhYMm5CTrtjPZ/1hPJLPgBLdkBv4EMel4xu"",
    ""Spark:UserOrgPreference:6"": ""65"",
    ""Rock_Context:533"": ""Rock.Model.Campus=EAAAAH3OWi3bJsdaTM3j0L6Azn1flSKGoXlccHRbNjBXuQc1tw2VYuGRoWFyLQV2qAHFSlhgJgX269dZenXzihaWDwmLkuEr5YFkExb1Li65CzAZ"",
    ""__hstc"": ""181257784.747929e59a10677d335219b6dab7efab.1491002867030.1491266150483.1491341236789.5"",
    ""hubspotutk"": ""747929e59a10677d335219b6dab7efab"",
    ""_ga"": ""GA1.1.313684559.1484857967"",
    ""Rock_Context"": ""Rock.Model.Campus=EAAAAB0070cjicfA81Fp8dGMcwJOEmGJ/L2mNOR8+EJ9S9QY72yoXdIkNpV66CIt7bAzk04Tizb5qPODLd8LJm65XiLKOK+TW9Q+6subKR5sMAnk"",
    ""ASP.NET_SessionId"": ""ovg4vwgrgnys43hiccynq3aj"",
    ""last_site"": ""1""
  }
}
</pre>
</div>
" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, SystemGuid.FieldType.TEXT, "Process Request", "ProcessRequest", "Lava used to evaluate if this item should be used for a particular request to the webhook. Lava should return 'True' or 'False'", 0, "", "2d1693db-52aa-453c-b111-ebe9ca225317" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, SystemGuid.FieldType.WORKFLOW_TYPE, "Workflow Type", "WorkflowType", "The type of workflow to launch.", 1, "", "307b0e9c-bd7b-4ca2-9616-254850736fb9" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, SystemGuid.FieldType.TEXT, "Workflow Name Template", "WorkflowNameTemplate", "The lava template to use for setting the workflow name. See the defined type's help text for a listing of merge fields. <span class='tip tip-lava'></span>", 2, "", "399b3d34-8273-497c-8fb7-cb137dadd6c3" );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_WORKFLOW, SystemGuid.FieldType.KEY_VALUE_LIST, "Workflow Attributes", "WorkflowAttributes", "Key/value list of workflow attributes to set with the given lava merge template. See the defined type’s help text for a listing of merge fields. <span class='tip tip-lava'></span>", 3, "", "c75b0443-de6c-4791-ab4a-018429a807c8" );
            RockMigrationHelper.AddAttributeQualifier( "c75b0443-de6c-4791-ab4a-018429a807c8", "keyprompt", "Attribute Key", "c2c293f5-30c4-48e1-b31b-12612d7e4cb1" );
            RockMigrationHelper.AddAttributeQualifier( "c75b0443-de6c-4791-ab4a-018429a807c8", "valueprompt", "Lava Template", "79137493-d103-4368-a2e8-942e70ee2c5c" );

            Sql( @"
UPDATE [Attribute]
SET [IsGridColumn] = 1 
WHERE [Guid] = '307b0e9c-bd7b-4ca2-9616-254850736fb9'
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Campus", "Order");
        }
    }
}
