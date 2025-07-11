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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 999, "18.0" )]
    public class AddCommunicationFlowBlocksAndPages: Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JMH_AddCommunicationFlowBlocksAndPagesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JMH_AddCommunicationFlowBlocksAndPagesDown();
        }

        private void JMH_AddCommunicationFlowBlocksAndPagesUp()
        {
            // Add Page 
            //  Internal Name: Communication Flows
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7F79E512-B9DB-4780-9887-AD6D63A39050", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Flows", "", "2B479B57-EECA-464C-8C19-6373D82D70F8", "" );

            // Add Page Route
            //   Page:Communication Flows
            //   Route:CommunicationFlows
            RockMigrationHelper.AddOrUpdatePageRoute( "2B479B57-EECA-464C-8C19-6373D82D70F8", "CommunicationFlows", "C563C767-2ECF-4469-BDA7-05A35D95D9A4" );

            // Add Page 
            //  Internal Name: Communication Flow
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "2B479B57-EECA-464C-8C19-6373D82D70F8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Flow", "", "C8BF7DE2-629A-46D3-B744-C749A0606DFE", "" );

            // Add Page Route
            //   Page:Communication Flow
            //   Route:CommunicationFlows/{CommunicationFlow}
            RockMigrationHelper.AddOrUpdatePageRoute( "C8BF7DE2-629A-46D3-B744-C749A0606DFE", "CommunicationFlows/{CommunicationFlow}", "D2258733-4FFD-4FD0-B4D6-E094ABD2B9A1" );

            // Remove Page Name From Breadcrumbs
            //   Page:Communication Flow
            //   Breadcrumb: "Home > Communication Flows > Communication Flow Name"
            this.RockMigrationHelper.UpdatePageBreadcrumb( "C8BF7DE2-629A-46D3-B744-C749A0606DFE", false );

            // Add Page 
            //  Internal Name: Communication Flow Performance
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "2B479B57-EECA-464C-8C19-6373D82D70F8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Flow Performance", "", "6595A8D0-94D5-442A-87AA-1E79F1D9B67A", "" );

            // Add Page Route
            //   Page:Communication Flow Performance
            //   Route:CommunicationFlows/{CommunicationFlow}/Performance
            RockMigrationHelper.AddOrUpdatePageRoute( "6595A8D0-94D5-442A-87AA-1E79F1D9B67A", "CommunicationFlows/{CommunicationFlow}/Performance", "637BD635-0836-465F-B800-7A394922699E" );

            // Remove Page Name From Breadcrumbs
            //   Page:Communication Flow
            //   Breadcrumb: "Home > Communication Flows > Communication Flow Name"
            this.RockMigrationHelper.UpdatePageBreadcrumb( "6595A8D0-94D5-442A-87AA-1E79F1D9B67A", false );

            // Add Page 
            //  Internal Name: Communication Flow Instance Message Metrics
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "6595A8D0-94D5-442A-87AA-1E79F1D9B67A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Flow Instance Message Metrics", "", "C70E547B-F058-4CDD-9A97-55A2D1F8650A", "" );

            // Add Page Route
            //   Page:Communication Flow Instance Message Metrics
            //   Route:CommunicationFlows/{CommunicationFlow}/Instances/{CommunicationFlowInstance}/Messages/{CommunicationFlowInstanceCommunication}/Metrics
            RockMigrationHelper.AddOrUpdatePageRoute( "C70E547B-F058-4CDD-9A97-55A2D1F8650A", "CommunicationFlows/{CommunicationFlow}/Instances/{CommunicationFlowInstance}/Messages/{CommunicationFlowInstanceCommunication}/Metrics", "F031EAFE-959B-4CCC-897B-290784689FD1" );

            // Remove Page Name From Breadcrumbs
            //   Page:Communication Flow Instance Message Metrics
            //   Breadcrumb: "Home > Communication Flows > Communication Flow Name > Message Name > Metrics"
            this.RockMigrationHelper.UpdatePageBreadcrumb( "C70E547B-F058-4CDD-9A97-55A2D1F8650A", false );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationFlowPerformance
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationFlowPerformance", "Communication Flow Performance", "Rock.Blocks.Communication.CommunicationFlowPerformance, Rock.Blocks, Version=18.0.4.0, Culture=neutral, PublicKeyToken=null", false, false, "53FFF4C5-1F30-415C-BE65-53FCA7F2CD64" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationFlowDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationFlowDetail", "Communication Flow Detail", "Rock.Blocks.Communication.CommunicationFlowDetail, Rock.Blocks, Version=18.0.4.0, Culture=neutral, PublicKeyToken=null", false, false, "27774747-4CA6-4694-A860-078A1EC7BC79" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationFlowList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationFlowList", "Communication Flow List", "Rock.Blocks.Communication.CommunicationFlowList, Rock.Blocks, Version=18.0.4.0, Culture=neutral, PublicKeyToken=null", false, false, "BE37510E-BB86-4B3B-A634-98E6F095059B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationFlowInstanceMessageMetrics
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Communication.CommunicationFlowInstanceMessageMetrics", "Communication Flow Instance Message Metrics", "Rock.Blocks.Communication.CommunicationFlowInstanceMessageMetrics, Rock.Blocks, Version=18.0.7.0, Culture=neutral, PublicKeyToken=null", false, false, "91D70135-87DA-4748-B459-CCE7F60F3D67");

            // Add/Update Obsidian Block Type
            //   Name:Communication Flow Performance
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationFlowPerformance
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Flow Performance", "", "Rock.Blocks.Communication.CommunicationFlowPerformance", "Communication", "72B92AA4-2AA7-4FDD-9CD7-7DD84018B21E" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Flow Detail
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationFlowDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Flow Detail", "", "Rock.Blocks.Communication.CommunicationFlowDetail", "Communication", "CDD8DD26-E2DC-4D4A-8842-59FE00490651" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Flow List
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationFlowList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Flow List", "", "Rock.Blocks.Communication.CommunicationFlowList", "Communication", "08233FC4-3E15-42F9-97BF-1BFF89A7A0D0" );
            
            // Add/Update Obsidian Block Type
            //   Name:Communication Flow Instance Message Metrics
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationFlowInstanceMessageMetrics
            RockMigrationHelper.AddOrUpdateEntityBlockType("Communication Flow Instance Message Metrics", "Displays the performance of a particular communication flow instance message.", "Rock.Blocks.Communication.CommunicationFlowInstanceMessageMetrics", "Communication", "039ADBBE-4158-47C8-AE05-181DF42E990C");

            // Add Block 
            //  Block Name: Communication Flow List
            //  Page Name: Communication Flows
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2B479B57-EECA-464C-8C19-6373D82D70F8".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "08233FC4-3E15-42F9-97BF-1BFF89A7A0D0".AsGuid(), "Communication Flow List", "Main", @"", @"", 0, "4E637F21-5B5A-4CF0-A06B-B54020B4485A" );

            // Add Block 
            //  Block Name: Communication Flow Detail
            //  Page Name: Communication Flow
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C8BF7DE2-629A-46D3-B744-C749A0606DFE".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CDD8DD26-E2DC-4D4A-8842-59FE00490651".AsGuid(), "Communication Flow Detail", "Main", @"", @"", 0, "C755D092-FF97-4656-9F66-7E16435EEF81" );

            // Add Block 
            //  Block Name: Communication Flow Performance
            //  Page Name: Communication Flow Performance
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "6595A8D0-94D5-442A-87AA-1E79F1D9B67A".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "72B92AA4-2AA7-4FDD-9CD7-7DD84018B21E".AsGuid(), "Communication Flow Performance", "Main", @"", @"", 0, "337770AE-6D2B-4C05-B8C1-125896CEB06A" );
            
            // Add Block 
            //  Block Name: Communication Flow Instance Message Metrics
            //  Page Name: Communication Flow Instance Message Metrics
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C70E547B-F058-4CDD-9A97-55A2D1F8650A".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"039ADBBE-4158-47C8-AE05-181DF42E990C".AsGuid(), "Communication Flow Instance Message Metrics","Main",@"",@"",0,"F55FE6F1-4C93-4A37-A6DB-38A840E46637"); 

            // Attribute for BlockType
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08233FC4-3E15-42F9-97BF-1BFF89A7A0D0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the communication flow details.", 0, @"", "36B8E89B-E7BC-4060-BFEB-7B42B93B3921" );

            // Attribute for BlockType
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Attribute: Performance Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08233FC4-3E15-42F9-97BF-1BFF89A7A0D0", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Performance Page", "PerformancePage", "Performance Page", @"The page that will show the communication flow performance.", 0, @"", "7822B82A-27EC-4AEF-9BFC-780E33B1A725" );

            // Attribute for BlockType
            //   BlockType: Communication Flow Detail
            //   Category: Communication
            //   Attribute: Hide Step Indicator
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CDD8DD26-E2DC-4D4A-8842-59FE00490651", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Step Indicator", "HideStepIndicator", "Hide Step Indicator", @"Hides the visual step indicator at the top of the block to simplify the interface.", 0, @"False", "C620718F-2FB8-467B-A1B1-3F5FE8CF98DB" );            

            // Attribute for BlockType
            //   BlockType: Communication Flow Performance
            //   Category: Communication
            //   Attribute: Message Metrics Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72B92AA4-2AA7-4FDD-9CD7-7DD84018B21E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Message Metrics Page", "MessageMetricsPage", "Message Metrics Page", @"The page that will show the communication flow instance message metrics.", 0, @"", "F466A748-9917-4A61-9B45-42821E2DCA6C" );

            // Add Block Attribute Value
            //   Block: Communication Flow List
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Block Location: Page=Communication Flows, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: c8bf7de2-629a-46d3-b744-c749a0606dfe */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "4E637F21-5B5A-4CF0-A06B-B54020B4485A", "36B8E89B-E7BC-4060-BFEB-7B42B93B3921", @"c8bf7de2-629a-46d3-b744-c749a0606dfe" );

            // Add Block Attribute Value
            //   Block: Communication Flow List
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Block Location: Page=Communication Flows, Site=Rock RMS
            //   Attribute: Performance Page
            /*   Attribute Value: 6595a8d0-94d5-442a-87aa-1e79f1d9b67a */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "4E637F21-5B5A-4CF0-A06B-B54020B4485A", "7822B82A-27EC-4AEF-9BFC-780E33B1A725", @"6595a8d0-94d5-442a-87aa-1e79f1d9b67a" );

            // Add Block Attribute Value
            //   Block: Communication Flow Performance
            //   BlockType: Communication Flow Performance
            //   Category: Communication
            //   Block Location: Page=Communication Flow Performance, Site=Rock RMS
            //   Attribute: Message Metrics Page
            /*   Attribute Value: c70e547b-f058-4cdd-9a97-55a2d1f8650a,f031eafe-959b-4ccc-897b-290784689fd1 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true ,"337770AE-6D2B-4C05-B8C1-125896CEB06A","F466A748-9917-4A61-9B45-42821E2DCA6C",@"c70e547b-f058-4cdd-9a97-55a2d1f8650a,f031eafe-959b-4ccc-897b-290784689fd1");
        }

        private void JMH_AddCommunicationFlowBlocksAndPagesDown()
        {
            // Attribute for BlockType
            //   BlockType: Communication Flow Performance
            //   Category: Communication
            //   Attribute: Message Performance Page
            RockMigrationHelper.DeleteAttribute("F466A748-9917-4A61-9B45-42821E2DCA6C");

            // Attribute for BlockType
            //   BlockType: Communication Flow Detail
            //   Category: Communication
            //   Attribute: Hide Step Indicator
            RockMigrationHelper.DeleteAttribute("C620718F-2FB8-467B-A1B1-3F5FE8CF98DB");

            // Attribute for BlockType
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Attribute: Performance Page
            RockMigrationHelper.DeleteAttribute( "7822B82A-27EC-4AEF-9BFC-780E33B1A725" );

            // Attribute for BlockType
            //   BlockType: Communication Flow List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "36B8E89B-E7BC-4060-BFEB-7B42B93B3921" );
            
            // Remove Block
            //  Name: Communication Flow Instance Message Metrics, from Page: Communication Flow Instance Message Metrics, Site: Rock RMS
            //  from Page: Communication Flow Instance Message Metrics, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("F55FE6F1-4C93-4A37-A6DB-38A840E46637");

            // Remove Block
            //  Name: Communication Flow Performance, from Page: Communication Flow Performance, Site: Rock RMS
            //  from Page: Communication Flow Performance, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "337770AE-6D2B-4C05-B8C1-125896CEB06A" );

            // Remove Block
            //  Name: Communication Flow Detail, from Page: Communication Flow, Site: Rock RMS
            //  from Page: Communication Flow, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C755D092-FF97-4656-9F66-7E16435EEF81" );

            // Remove Block
            //  Name: Communication Flow List, from Page: Communication Flows, Site: Rock RMS
            //  from Page: Communication Flows, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4E637F21-5B5A-4CF0-A06B-B54020B4485A" );
            
            // Delete BlockType 
            //   Name: Communication Flow Instance Message Metrics
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Flow Instance Message Metrics
            RockMigrationHelper.DeleteBlockType("039ADBBE-4158-47C8-AE05-181DF42E990C");

            // Delete BlockType 
            //   Name: Communication Flow Performance
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Flow Performance
            RockMigrationHelper.DeleteBlockType( "72B92AA4-2AA7-4FDD-9CD7-7DD84018B21E" );
            
            // Delete BlockType 
            //   Name: Communication Flow Detail
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Flow Detail
            RockMigrationHelper.DeleteBlockType( "CDD8DD26-E2DC-4D4A-8842-59FE00490651" );

            // Delete BlockType 
            //   Name: Communication Flow List
            //   Category: Communication
            //   Path: -
            //   EntityType: Communication Flow List
            RockMigrationHelper.DeleteBlockType( "08233FC4-3E15-42F9-97BF-1BFF89A7A0D0" );

            // Delete Page 
            //  Internal Name: Communication Flow Instance Message Metrics
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "C70E547B-F058-4CDD-9A97-55A2D1F8650A" );

            // Delete Page 
            //  Internal Name: Communication Flow Performance
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "6595A8D0-94D5-442A-87AA-1E79F1D9B67A" );

            // Delete Page 
            //  Internal Name: Communication Flow
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "C8BF7DE2-629A-46D3-B744-C749A0606DFE" );

            // Delete Page 
            //  Internal Name: Communication Flows
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "2B479B57-EECA-464C-8C19-6373D82D70F8" );
        }
    }
}