// <copyright>
// Copyright by BEMA Information Technologies
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

using Rock.Plugin;

namespace com.bemaservices.ClientPackage.BEMA.Migrations
{
    [MigrationNumber( 16, "1.9.4" )]
    public class AttendanceMetricsHistoryDashboard : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Add Lava Webhook for Attendance Metric Data

            RockMigrationHelper.AddDefinedValue( Rock.SystemGuid.DefinedType.WEBHOOK_TO_LAVA, "/BEMAAttendanceMetricsHistoryDashboard", "Used by Attendance Metrics History Dashboard", "412F430F-721F-40B7-8D47-8BE3F1C72D29" );

            RockMigrationHelper.AddDefinedValueAttributeValueByValue( Rock.SystemGuid.DefinedType.WEBHOOK_TO_LAVA, "/BEMAAttendanceMetricsHistoryDashboard", "Template", @"
{% sql %}
SELECT
    SUM([MV].[YValue]) AS [Value]
    , [MV].[MetricValueDateTime] AS [DateTime]
    , [C].[Name] AS [Campus]
    , [S].[Name] AS [Schedule]
FROM [MetricValue] AS [MV]
INNER JOIN [Metric] AS [M] ON [M].[Id] = [MV].[MetricId]
INNER JOIN [MetricPartition] AS [MPCampus] ON [MPCampus].[MetricId] = [M].[Id]
INNER JOIN [EntityType] AS [ETCampus] ON [ETCampus].[Id] = [MPCampus].[EntityTypeId] AND [ETCampus].[Name] = 'Rock.Model.Campus'
INNER JOIN [MetricValuePartition] AS [MVCampus] ON [MVCampus].[MetricValueId] = [MV].[Id] AND [MVCampus].[MetricPartitionId] = [MPCampus].[Id]
INNER JOIN [Campus] AS [C] ON [C].[Id] = [MVCampus].[EntityId]
INNER JOIN [MetricPartition] AS [MPSchedule] ON [MPSchedule].[MetricId] = [M].[Id]
INNER JOIN [EntityType] AS [ETSchedule] ON [ETSchedule].[Id] = [MPSchedule].[EntityTypeId] AND [ETSchedule].[Name] = 'Rock.Model.Schedule'
INNER JOIN [MetricValuePartition] AS [MVSchedule] ON [MVSchedule].[MetricValueId] = [MV].[Id] AND [MVSchedule].[MetricPartitionId] = [MPSchedule].[Id]
INNER JOIN [Schedule] AS [S] ON [S].[Id] = [MVSchedule].[EntityId]
LEFT JOIN [dbo].[ufnUtility_CsvToTable]('{{ QueryString.Metric | SanitizeSql }}' ) AS[SSMetric] ON[SSMetric].[item] = [MV].[MetricId]
LEFT JOIN[dbo].[ufnUtility_CsvToTable] ('{{ QueryString.Campus | SanitizeSql }}') AS [SSCampus] ON [SSCampus].[item] = [C].[Id]

AND ('{{ QueryString.Metric | SanitizeSql }}' = '' OR [SSMetric].[item] IS NOT NULL)

AND ('{{ QueryString.Campus | SanitizeSql }}' = '' OR [SSCampus].[item] IS NOT NULL)

AND[MV].[MetricValueDateTime] > DATEADD(YEAR, -1, GETDATE())
GROUP BY [MV].[MetricValueDateTime],[S].[Name],[C].[Name]
        ORDER BY [MV].[MetricValueDateTime]
{% endsql %}
{{ results | ToJSON }}
            " );

            RockMigrationHelper.AddDefinedValueAttributeValueByValue( Rock.SystemGuid.DefinedType.WEBHOOK_TO_LAVA, "/BEMAAttendanceMetricsHistoryDashboard", "EnabledLavaCommands", "RockEntity,Sql" );
            RockMigrationHelper.AddDefinedValueAttributeValueByValue( Rock.SystemGuid.DefinedType.WEBHOOK_TO_LAVA, "/BEMAAttendanceMetricsHistoryDashboard", "ResponseContentType", "application/json" );

            // Page: Attendance Metrics History Dashboard
            RockMigrationHelper.AddPage("2571CBBD-7CCA-4B24-AAAB-107FD136298B","D65F783D-87A9-4CC9-8110-E83466A0EADB","Attendance Metrics History Dashboard","Displays a user-interactive graph of last 12 months of attendance history, based on any metrics in Rock that end in the word 'Attendance'. These graphs can be selected by metric name, Campus, and Schedule (Service)","2FEDA550-EAF4-431C-82D4-BE6D55188CA3","fa fa-users"); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "HTML Content", "Adds an editable HTML fragment to the page.", "~/Blocks/Cms/HtmlContentDetail.ascx", "CMS", "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            // Add Block to Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2FEDA550-EAF4-431C-82D4-BE6D55188CA3","","19B61D65-37E3-459F-A44F-DEF0089118A3","Report","Main","","",0,"BE92D984-55CB-46E7-A3AC-2B9EB511A6EE");   
            // Add Block to Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2FEDA550-EAF4-431C-82D4-BE6D55188CA3","","19B61D65-37E3-459F-A44F-DEF0089118A3","Title","Feature","","",0,"53F29693-7B4F-42EC-A0ED-3064572D1DC4");   
            // Attrib for BlockType: HTML Content:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "6783D47D-92F9-4F48-93C0-16111D675A0F" );  
            // Attrib for BlockType: HTML Content:Enabled Lava Commands
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "", @"The Lava commands that should be enabled for this HTML block.", 0, @"", "7146AC24-9250-4FC4-9DF2-9803B9A84299" );  
            // Attrib for BlockType: HTML Content:Start in Code Editor mode
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Start in Code Editor mode", "UseCodeEditor", "", @"Start the editor in code editor mode instead of WYSIWYG editor mode.", 1, @"True", "0673E015-F8DD-4A52-B380-C758011331B2" );  
            // Attrib for BlockType: HTML Content:Document Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "", @"The folder to use as the root when browsing or uploading documents.", 2, @"~/Content", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );  
            // Attrib for BlockType: HTML Content:Image Root Folder
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "", @"The folder to use as the root when browsing or uploading images.", 3, @"~/Content", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );  
            // Attrib for BlockType: HTML Content:User Specific Folders
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "", @"Should the root folders be specific to current user?", 4, @"False", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );  
            // Attrib for BlockType: HTML Content:Cache Duration
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Cache Duration", "CacheDuration", "", @"Number of seconds to cache the content.", 5, @"0", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );  
            // Attrib for BlockType: HTML Content:Context Parameter
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Parameter", "ContextParameter", "", @"Query string parameter to use for 'personalizing' content based on unique values.", 6, @"", "3FFC512D-A576-4289-B648-905FD7A64ABB" );  
            // Attrib for BlockType: HTML Content:Context Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Name", "ContextName", "", @"Name to use to further 'personalize' content.  Blocks with the same name, and referenced with the same context parameter will share html values.", 7, @"", "466993F7-D838-447A-97E7-8BBDA6A57289" );  
            // Attrib for BlockType: HTML Content:Enable Versioning
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Versioning", "SupportVersions", "", @"If checked, previous versions of the content will be preserved. Versioning is required if you want to require approval.", 8, @"False", "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );  
            // Attrib for BlockType: HTML Content:Require Approval
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Approval", "RequireApproval", "", @"Require that content be approved?", 9, @"False", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );  
            // Attrib for BlockType: HTML Content:Cache Tags
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Cache Tags", "CacheTags", "", @"Cached tags are used to link cached content so that it can be expired as a group", 10, @"", "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );  
            // Attrib for BlockType: HTML Content:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", @"Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );  

            // Attrib Value for Block:Report, Attribute:Enable Versioning Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","7C1CE199-86CF-4EAE-8AB3-848416A72C58",@"False");  
            // Attrib Value for Block:Report, Attribute:Require Approval Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A",@"False");  
            // Attrib Value for Block:Report, Attribute:Cache Duration Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4",@"0");  
            // Attrib Value for Block:Report, Attribute:Start in Code Editor mode Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","0673E015-F8DD-4A52-B380-C758011331B2",@"True");  
            // Attrib Value for Block:Report, Attribute:Image Root Folder Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","26F3AFC6-C05B-44A4-8593-AFE1D9969B0E",@"~/Content");  
            // Attrib Value for Block:Report, Attribute:User Specific Folders Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE",@"False");  
            // Attrib Value for Block:Report, Attribute:Document Root Folder Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","3BDB8AED-32C5-4879-B1CB-8FC7C8336534",@"~/Content");  
            // Attrib Value for Block:Report, Attribute:Enabled Lava Commands Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","7146AC24-9250-4FC4-9DF2-9803B9A84299",@"RockEntity,Sql");  
            // Attrib Value for Block:Report, Attribute:Is Secondary Block Page: Attendance Metrics History Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BE92D984-55CB-46E7-A3AC-2B9EB511A6EE","04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4",@"False");


            // HTML Content Blocks

            RockMigrationHelper.UpdateHtmlContentBlock( "53F29693-7B4F-42EC-A0ED-3064572D1DC4", @"
        <div class='alert alert-info'>Select a Campus and Metric to display on the graph. Schedules can be toggled on and off on the right-hand legend.</div>
", "1439C630-EA58-4E87-AD42-973A3EFBA8D7" ); // Title

            RockMigrationHelper.UpdateHtmlContentBlock( "BE92D984-55CB-46E7-A3AC-2B9EB511A6EE", @"
{% include '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Assets/Lava/AttendanceMetricsHistoryDashboard.lava' %}
", "CE75D279-AF1D-4114-BEA8-7FB230016F2D" ); // Report


            // Page Auth Start
            RockMigrationHelper.AddSecurityAuthForPage( "2FEDA550-EAF4-431C-82D4-BE6D55188CA3", 0, "View", false, "", 1, "E60965F3-4FF5-420B-9497-5F3B65042A7B" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "522C18A9-C727-42A5-A0BA-13C673E8C4B6" );
            RockMigrationHelper.DeleteAttribute( "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            RockMigrationHelper.DeleteAttribute( "7146AC24-9250-4FC4-9DF2-9803B9A84299" );
            RockMigrationHelper.DeleteAttribute( "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            RockMigrationHelper.DeleteAttribute( "3BDB8AED-32C5-4879-B1CB-8FC7C8336534" );
            RockMigrationHelper.DeleteAttribute( "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE" );
            RockMigrationHelper.DeleteAttribute( "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E" );
            RockMigrationHelper.DeleteAttribute( "0673E015-F8DD-4A52-B380-C758011331B2" );
            RockMigrationHelper.DeleteAttribute( "466993F7-D838-447A-97E7-8BBDA6A57289" );
            RockMigrationHelper.DeleteAttribute( "3FFC512D-A576-4289-B648-905FD7A64ABB" );
            RockMigrationHelper.DeleteAttribute( "7C1CE199-86CF-4EAE-8AB3-848416A72C58" );
            RockMigrationHelper.DeleteAttribute( "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A" );
            RockMigrationHelper.DeleteAttribute( "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4" );
            RockMigrationHelper.DeleteBlock( "53F29693-7B4F-42EC-A0ED-3064572D1DC4" );
            RockMigrationHelper.DeleteBlock( "BE92D984-55CB-46E7-A3AC-2B9EB511A6EE" );
            RockMigrationHelper.DeleteBlockType( "19B61D65-37E3-459F-A44F-DEF0089118A3" );
            RockMigrationHelper.DeletePage( "2FEDA550-EAF4-431C-82D4-BE6D55188CA3" ); //  Page: Attendance Metrics History Dashboard

            RockMigrationHelper.DeleteDefinedValue( "412F430F-721F-40B7-8D47-8BE3F1C72D29" ); // /BEMAAttendanceMetricsHistoryDashboard Endpoint
        }
    }
}
