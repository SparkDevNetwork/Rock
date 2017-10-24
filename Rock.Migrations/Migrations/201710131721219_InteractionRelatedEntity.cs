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
    public partial class InteractionRelatedEntity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Interaction", "RelatedEntityTypeId", c => c.Int() );
            AddColumn( "dbo.Interaction", "RelatedEntityId", c => c.Int() );
            CreateIndex( "dbo.Interaction", "RelatedEntityTypeId" );
            AddForeignKey( "dbo.Interaction", "RelatedEntityTypeId", "dbo.EntityType", "Id" );

            // MP: Job for Analytics Dim Family
            Sql( @"
INSERT INTO [dbo].[ServiceJob]
           ([IsSystem]
           ,[IsActive]
           ,[Name]
           ,[Description]
           ,[Class]
           ,[CronExpression]
           ,[NotificationStatus]
           ,[Guid])
     VALUES
        (0	
         ,1	
         ,'Process Analytics Dimension Tables for Family'
         ,'Job to take care of schema changes ( dynamic Attribute Value Fields ) and data updates to Family analytic tables'
         ,'Rock.Jobs.ProcessAnalyticsDimFamily'
         ,'0 0 5 1/1 * ? *'
         ,3
         ,'23629583-8618-4FAF-8088-AFCC7545A2E0')" );

            // MP: Migration for External Site > Connect > Subscribe
            RockMigrationHelper.AddPage( "7625A63E-6650-4886-B605-53C2234FA5E1", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Subscribe", "", "0DC2E79D-3590-45E8-A16B-C720A134BA51", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Communication List Subscribe", "Block that allows a person to manage the communication lists that they are subscribed to", "~/Blocks/Communication/CommunicationListSubscribe.ascx", "Communication", "52E0AA5B-B08B-42E4-8180-DD7925BAA57F" );
            // Add Block to Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlock( "0DC2E79D-3590-45E8-A16B-C720A134BA51", "", "52E0AA5B-B08B-42E4-8180-DD7925BAA57F", "Communication List Subscribe", "Main", @"", @"", 0, "B2CCF6EC-8C07-4B02-9E3A-6D5674050141" );
            // Add Block to Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlock( "0DC2E79D-3590-45E8-A16B-C720A134BA51", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "SubNav", "Sidebar1", @"", @"", 0, "606E154E-F273-474E-97D9-6F4D15505CAC" );

            // Restrict External 'Connect > Subscribe' to only Logged In users
            RockMigrationHelper.AddSecurityAuthForPage( "0DC2E79D-3590-45E8-A16B-C720A134BA51", 0, Rock.Security.Authorization.VIEW, true, null, ( int ) Rock.Model.SpecialRole.AllAuthenticatedUsers, "DAA9EC57-A460-4905-9CA5-430E11829167" );
            RockMigrationHelper.AddSecurityAuthForPage( "0DC2E79D-3590-45E8-A16B-C720A134BA51", 1, Rock.Security.Authorization.VIEW, false, null, ( int ) Rock.Model.SpecialRole.AllUsers, "C23CCE9F-F662-4535-8E02-395309E91E33" );

            // Attrib for BlockType: Communication List Subscribe:Communication List Categories
            RockMigrationHelper.UpdateBlockTypeAttribute( "52E0AA5B-B08B-42E4-8180-DD7925BAA57F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories", "", "Select the categories of the communication lists to display, or select none to show all that the user is authorized to view.", 1, @"", "223C64DE-C188-48CB-BA33-AF44ED038A79" );
            // Attrib for BlockType: Email Preference Entry:Communication List Categories
            RockMigrationHelper.UpdateBlockTypeAttribute( "B3C076C7-1325-4453-9549-456C23702069", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories", "", "Select the categories of the communication lists to display for unsubscribe, or select none to show all that the user is authorized to view.", 9, @"", "74362E42-74E0-40D5-8FD4-A7AF5B74D1B6" );
            // Attrib for BlockType: Email Preference Entry:Unsubscribe from Lists Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "B3C076C7-1325-4453-9549-456C23702069", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Unsubscribe from Lists Text", "UnsubscribefromListsText", "", "Text to display for the 'Unsubscribe me from the following lists:' option.", 1, @"Only unsubscribe me from the following lists", "4B4E8346-8C4F-4AA7-ABE8-C74272D2E381" );

            // Attrib Value for Block:Edit Email Preference, Attribute:Communication List Categories Page: Email Preference, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1F73AE04-1CD3-49E0-900C-0D19371EEEC0", "74362E42-74E0-40D5-8FD4-A7AF5B74D1B6", @"" );
            // Attrib Value for Block:Edit Email Preference, Attribute:Unsubscribe from Lists Text Page: Email Preference, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1F73AE04-1CD3-49E0-900C-0D19371EEEC0", "4B4E8346-8C4F-4AA7-ABE8-C74272D2E381", @"Only unsubscribe me from the following lists" );

            // Attrib Value for Block:Communication List Subscribe, Attribute:Communication List Categories Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "B2CCF6EC-8C07-4B02-9E3A-6D5674050141", "223C64DE-C188-48CB-BA33-AF44ED038A79", @"" );

            // Attrib Value for Block:SubNav, Attribute:CSS File Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "606E154E-F273-474E-97D9-6F4D15505CAC", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );
            // Attrib Value for Block:SubNav, Attribute:Include Current Parameters Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "606E154E-F273-474E-97D9-6F4D15505CAC", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
            // Attrib Value for Block:SubNav, Attribute:Template Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "606E154E-F273-474E-97D9-6F4D15505CAC", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageSubNav.lava' %}" );
            // Attrib Value for Block:SubNav, Attribute:Root Page Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "606E154E-F273-474E-97D9-6F4D15505CAC", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7625a63e-6650-4886-b605-53c2234fa5e1" );
            // Attrib Value for Block:SubNav, Attribute:Number of Levels Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "606E154E-F273-474E-97D9-6F4D15505CAC", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );
            // Attrib Value for Block:SubNav, Attribute:Include Current QueryString Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "606E154E-F273-474E-97D9-6F4D15505CAC", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );
            // Attrib Value for Block:SubNav, Attribute:Is Secondary Block Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "606E154E-F273-474E-97D9-6F4D15505CAC", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );
            // Attrib Value for Block:SubNav, Attribute:Include Page List Page: Subscribe, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "606E154E-F273-474E-97D9-6F4D15505CAC", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" );

            // MP: Fix Workflow Form Notification system email (for modified ones too)
            /* Fix Workflow Form Notification system email by passing 'Command' instead of 'action' to the Workflow Entry block. */
            /* Also pass ActionId so Workflow Entry block can target the exact User Entry Form if given. */
            Sql( @"
            UPDATE SystemEmail
SET[Body] = REPLACE([Body], 
'{% capture ButtonLinkReplace %}{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}&action={{ button.Name }}{% endcapture %}' ,
'{% capture ButtonLinkReplace %}{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}&ActionId={{ Action.Id }}&Command={{ button.Name }}{% endcapture %}'
)
WHERE[Guid] = '88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
AND[BODY] like '%{% capture ButtonLinkReplace %}{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}&action={{ button.Name }}{% endcapture %}%'" );

            // MP: Updated ETL Job to no longer do Family
            Sql( @"
UPDATE AttributeValue
SET [Value] = REPLACE([Value], 'EXEC [dbo].[spAnalytics_ETL_Family]', '')
WHERE [Guid] = 'A6F83C17-5950-44B6-9C07-94DDAFCFBC39'
 AND [Value] LIKE '%\[dbo\].\[spAnalytics_ETL_Family\]%' ESCAPE '\'


UPDATE ServiceJob
SET [Description] = 'Run the Stored Procedures that do the ETL for the AnalyticsFactFinancialTransaction and AnalyticsFactAttendance tables'
WHERE [Guid] = '447B248B-2187-4368-9EE3-6E17B8F542A7'
 AND [Description] != 'Run the Stored Procedures that do the ETL for the AnalyticsFactFinancialTransaction and AnalyticsFactAttendance tables'
" );

            // MP: Update spAnalytics_ETL_Family to return rows updated and inserted
            Sql( MigrationSQL._201710131721219_InteractionRelatedEntity_spAnalytics_ETL_Family );

            // MP: Add Current/History Family to Analytics Tables
            Sql( MigrationSQL._201710131721219_InteractionRelatedEntity_AnalyticsFactAttendance );
            Sql( MigrationSQL._201710131721219_InteractionRelatedEntity_AnalyticsFactFinancialTransaction );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: SubNav, from Page: Subscribe, Site: External Website
            RockMigrationHelper.DeleteBlock( "606E154E-F273-474E-97D9-6F4D15505CAC" );
            // Remove Block: Communication List Subscribe, from Page: Subscribe, Site: External Website
            RockMigrationHelper.DeleteBlock( "B2CCF6EC-8C07-4B02-9E3A-6D5674050141" );
            RockMigrationHelper.DeletePage( "0DC2E79D-3590-45E8-A16B-C720A134BA51" ); //  Page: Subscribe, Layout: LeftSidebar, Site: External Website

            DropForeignKey( "dbo.Interaction", "RelatedEntityTypeId", "dbo.EntityType" );
            DropIndex( "dbo.Interaction", new[] { "RelatedEntityTypeId" } );
            DropColumn( "dbo.Interaction", "RelatedEntityId" );
            DropColumn( "dbo.Interaction", "RelatedEntityTypeId" );
        }
    }
}
