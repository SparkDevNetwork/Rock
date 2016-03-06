// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class DashboardWorkflowPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "AE1818D8-581C-4599-97B9-509EA450376A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflow Detail", "", "5A6FE57A-980F-4964-B9C0-0D324700CDC3", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "AE1818D8-581C-4599-97B9-509EA450376A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflow Entry", "", "74488EAD-D288-4116-90AF-439C48659490", "" ); // Site:Rock RMS

            Sql( @"
        UPDATE [Page] SET [BreadCrumbDisplayName] = 0 
        WHERE [GUID] = '74488EAD-D288-4116-90AF-439C48659490'
" );

            RockMigrationHelper.AddPageRoute( "5A6FE57A-980F-4964-B9C0-0D324700CDC3", "MyDashboardWorkflow/{WorkflowId}", "7B77AFEF-685C-41A9-99EE-3AD444840E48" );// for Page:Workflow Detail
            RockMigrationHelper.AddPageRoute( "74488EAD-D288-4116-90AF-439C48659490", "MyDashboardWorkflowEntry/{WorkflowTypeId}/{WorkflowId}", "B53B12F1-CF2A-463E-B716-367856BBF8D4" );// for Page:Workflow Entry
            RockMigrationHelper.AddPageRoute( "74488EAD-D288-4116-90AF-439C48659490", "MyDashboardWorkflowEntry/{WorkflowTypeId}", "4B9DC387-B25E-4877-821E-19DC4F643A79" );// for Page:Workflow Entry

            // Add Block to Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "5A6FE57A-980F-4964-B9C0-0D324700CDC3", "", "4A9D62CE-5822-490F-B9EE-6D80037B4F5F", "Workflow Detail", "Main", "", "", 0, "FE93D027-0430-4D3E-AEBE-B32EB996D445" );
            // Add Block to Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "5A6FE57A-980F-4964-B9C0-0D324700CDC3", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 1, "FFB2F48A-51E9-4D59-A435-5082C76198B5" );
            // Add Block to Page: Workflow Entry, Site: Rock RMS
            RockMigrationHelper.AddBlock( "74488EAD-D288-4116-90AF-439C48659490", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "46ADDD92-FCFB-4BC0-ADE0-5588DE1FB5A1" );
            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Attrib Value for Block:Notes, Attribute:Entity Type Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"3540e9a7-fe30-43a9-8b0a-a372b63dfc93" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"False" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );
            // Attrib Value for Block:Notes, Attribute:Heading Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-comment" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Attrib Value for Block:Notes, Attribute:Allow Backdated Notes Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FFB2F48A-51E9-4D59-A435-5082C76198B5", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" );

            // Add/Update PageContext for Page:Workflow Detail, Entity: Rock.Model.Workflow, Parameter: workflowid
            RockMigrationHelper.UpdatePageContext( "5A6FE57A-980F-4964-B9C0-0D324700CDC3", "Rock.Model.Workflow", "workflowid", "DE7A9E8C-D983-4DB6-8F69-C02665327D47" );

            // Update any lava on the my workflow lava blocks to point to new "dashboard" workflow pages
            Sql( @"
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2' )
    UPDATE [AttributeValue] 
    SET [Value] = REPLACE( [Value], '~/{% if Role == ''0'' %}Workflow', '~/MyDashboard{% if Role == ''0'' %}Workflow' )
    WHERE [AttributeId] = @AttributeId
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2' )
    UPDATE [AttributeValue] 
    SET [Value] = REPLACE( [Value], '~/MyDashboard{% if Role == ''0'' %}Workflow', '~/{% if Role == ''0'' %}Workflow' )
    WHERE [AttributeId] = @AttributeId
" );
            // Remove Block: Workflow Entry, from Page: Workflow Entry, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "46ADDD92-FCFB-4BC0-ADE0-5588DE1FB5A1" );
            // Remove Block: Notes, from Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "FFB2F48A-51E9-4D59-A435-5082C76198B5" );
            // Remove Block: Workflow Detail, from Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "FE93D027-0430-4D3E-AEBE-B32EB996D445" );

            RockMigrationHelper.DeletePage( "74488EAD-D288-4116-90AF-439C48659490" ); //  Page: Workflow Entry, Layout: Full Width, Site: Rock RMS

            RockMigrationHelper.DeletePage( "5A6FE57A-980F-4964-B9C0-0D324700CDC3" ); //  Page: Workflow Detail, Layout: Full Width, Site: Rock RMS

            // Delete PageContext for Page:Workflow Detail, Entity: Rock.Model.Workflow, Parameter: workflowid
            RockMigrationHelper.DeletePageContext( "DE7A9E8C-D983-4DB6-8F69-C02665327D47" );

        }
    }
}
