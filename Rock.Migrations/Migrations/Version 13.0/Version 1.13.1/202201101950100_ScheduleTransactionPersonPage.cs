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
    public partial class ScheduleTransactionPersonPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page 
            //  Internal Name: Scheduled Transaction
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892","D65F783D-87A9-4CC9-8110-E83466A0EADB","Scheduled Transaction","","581D99DD-AA5B-40AF-B7CB-F915A4EA2BD9","");
			
			
            // Add Block 
            //  Block Name: Scheduled Transaction View
            //  Page Name: Scheduled Transaction
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "581D99DD-AA5B-40AF-B7CB-F915A4EA2BD9".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"85753750-7465-4241-97A6-E5F27EA38C8B".AsGuid(), "Scheduled Transaction View","Feature",@"",@"",0,"E7BB8795-6444-4E65-A773-D227BF40924F"); 
			
			
            // Add Block Attribute Value
            //   Block: Scheduled Transaction View
            //   BlockType: Scheduled Transaction View
            //   Category: Finance
            //   Block Location: Page=Scheduled Transaction, Site=Rock RMS
            //   Attribute: Update Page for Gateways
            /*   Attribute Value: d360b64f-1267-4518-95cd-99cd5ab87d88 */
            RockMigrationHelper.AddBlockAttributeValue("E7BB8795-6444-4E65-A773-D227BF40924F","98FE689B-DCBC-4E29-9269-A96FE8066C50","D360B64F-1267-4518-95CD-99CD5AB87D88");
			
			
			// Need update to change parent page of EditScheduledTransaction page to the new ScheduledTransation page
			Sql( @"
			UPDATE [dbo].[Page]
			SET [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '581D99DD-AA5B-40AF-B7CB-F915A4EA2BD9')
			WHERE [Guid] = 'D360B64F-1267-4518-95CD-99CD5AB87D88'" );

            
            // Add Block Attribute Value
            //   Block: Giving Configuration
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Block Location: Page=Contributions, Site=Rock RMS
            //   Attribute: Scheduled Transaction Detail Page
            /*   Attribute Value: D360B64F-1267-4518-95CD-99CD5AB87D88 */
            RockMigrationHelper.AddBlockAttributeValue("21B28504-6ED3-44E2-BB85-3401F8B1B96A","B6861FC8-3741-46BA-9AC6-936C29511065","581D99DD-AA5B-40AF-B7CB-F915A4EA2BD9");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block
            //  Name: Scheduled Transaction View, from Page: Scheduled Transaction, Site: Rock RMS
            //  from Page: Scheduled Transaction, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("E7BB8795-6444-4E65-A773-D227BF40924F");
			
			
            // Delete Page 
            //  Internal Name: Scheduled Transaction
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage("581D99DD-AA5B-40AF-B7CB-F915A4EA2BD9");

			// Need update to change parent page of EditScheduledTransaction page to the new ScheduledTransation page
			Sql( @"
			UPDATE [dbo].[Page]
			SET [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '53CF4CBE-85F9-4A50-87D7-0D72A3FB2892')
			WHERE [Guid] = '581D99DD-AA5B-40AF-B7CB-F915A4EA2BD9'" );

            // Add Block Attribute Value
            //   Block: Giving Configuration
            //   BlockType: Giving Configuration
            //   Category: CRM > Person Detail
            //   Block Location: Page=Contributions, Site=Rock RMS
            //   Attribute: Scheduled Transaction Detail Page
            /*   Attribute Value: 996F5541-D2E1-47E4-8078-80A388203CEC */
            RockMigrationHelper.AddBlockAttributeValue("21B28504-6ED3-44E2-BB85-3401F8B1B96A","B6861FC8-3741-46BA-9AC6-936C29511065",@"996F5541-D2E1-47E4-8078-80A388203CEC");
        }
    }
}
