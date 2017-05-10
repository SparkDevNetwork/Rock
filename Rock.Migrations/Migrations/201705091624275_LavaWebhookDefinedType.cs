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
    public partial class LavaWebhookDefinedType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Lava Webhook", "Webhook to process pre-defined Lava in response to incoming requests. ", SystemGuid.DefinedType.WEBHOOK_TO_LAVA, @"The following merge fields are available for the ''Template'' attribute.

<p>
    <a data-toggle=""collapse"" href=""#collapsefields"" class=""btn btn-action btn-xs"">show/hide fields</a>
</p>

<div id=""collapsefields"" class=""panel-collapse collapse"">
<pre>
{
  ""Url"": ""/test"",
  ""RawUrl"": ""http://localhost:6229/Webhooks/LavaApi.ashx/test?queryParameter=true"",
  ""Method"": ""POST"",
  ""QueryString"": {
    ""queryParameter"": ""true""
  },
  ""RemoteAddress"": ""127.0.0.1"",
  ""RemoteName"": ""127.0.0.1"",
  ""ServerName"": ""localhost"",
  ""RawBody"": ""{""key1"":""value1"", ""key2"":""value2""}"",
  ""Body"": {
    ""key1"": ""value1"",
    ""key2"": ""value2""
  },
  ""Headers"": {
    ""Content-Length"": ""34"",
    ""Content-Type"": ""application/json"",
    ""Accept"": ""*/*"",
    ""Host"": ""localhost:6229"",
    ""User-Agent"": ""curl/7.35.0""
  },
  ""Cookies"": {
    ""sessionToken"": ""abc123""
  }
}
</pre>
</div>" );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_LAVA, SystemGuid.FieldType.SINGLE_SELECT, "Method", "Method", "The HTTP Verb to match against. Leave blank to match all verbs.", 1, "", "d9c92cdb-70ab-4d99-b580-eb55ed9fbee0" );
            RockMigrationHelper.AddAttributeQualifier( "d9c92cdb-70ab-4d99-b580-eb55ed9fbee0", "fieldtype", "ddl", "6939b529-7fcf-49f9-9c37-3a8a1e7af7a3" );
            RockMigrationHelper.AddAttributeQualifier( "d9c92cdb-70ab-4d99-b580-eb55ed9fbee0", "values", "GET,POST,PUT,PATCH,DELETE", "461cfb14-4a2a-4662-98df-89ea7478627a" );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_LAVA, SystemGuid.FieldType.CODE_EDITOR, "Template", "Template", "The lava script to use when processing this request. <span class='tip tip-lava'></span>", 2, "", "4303ae08-2208-4f46-9b98-fd91a710ce1e" );
            RockMigrationHelper.AddAttributeQualifier( "4303ae08-2208-4f46-9b98-fd91a710ce1e", "editorHeight", "300", "c65b2e5a-27fd-4096-ab6c-cc58289d1258" );
            RockMigrationHelper.AddAttributeQualifier( "4303ae08-2208-4f46-9b98-fd91a710ce1e", "editorMode", Rock.Web.UI.Controls.CodeEditorMode.Lava.ConvertToInt().ToString(), "4d987d2e-4584-436f-9e5c-ed18d87af827" );
            RockMigrationHelper.AddAttributeQualifier( "4303ae08-2208-4f46-9b98-fd91a710ce1e", "editorTheme", Rock.Web.UI.Controls.CodeEditorTheme.Rock.ConvertToInt().ToString(), "f46b6509-2ef7-4283-b4fd-dd899de65832" );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_LAVA, SystemGuid.FieldType.LAVA_COMMANDS, "Enabled Lava Commands", "EnabledLavaCommands", "Which lava commands are available to be used by the lava template", 3, "", "2df9f53d-926e-4d2a-b755-818edd933781" );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_LAVA, SystemGuid.FieldType.TEXT, "Response Content Type", "ResponseContentType", "The Content-Type header to include in the response. If no value is provided then 'text/plain' is used. Common values are 'application/json' and 'application/xml'.", 4, "", "73774e76-028f-445c-a078-bfb8885a102c" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
