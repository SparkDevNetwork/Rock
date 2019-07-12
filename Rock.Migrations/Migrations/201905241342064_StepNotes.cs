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
    public partial class StepNotes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block to Page: Step Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2109228C-D828-4B58-9310-8D93D10B846E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3".AsGuid(), "Notes", "Main", @"", @"", 1, "9410BE4C-134E-47D1-93D0-62248280B67F" );
            // Add Block to Page: Step Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3".AsGuid(), "Notes", "Main", @"", @"", 1, "457489AD-D236-4994-8A0C-6E89423FBE3B" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '74E22668-FE00-4238-AC40-6A2DACD48F56'" );  // Page: Step,  Zone: Main,  Block: Step Entry
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '826E3498-AEC5-45DF-A454-F3AD19573714'" );  // Page: Step,  Zone: Main,  Block: Step Entry
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '457489AD-D236-4994-8A0C-6E89423FBE3B'" );  // Page: Step,  Zone: Main,  Block: Notes
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '9410BE4C-134E-47D1-93D0-62248280B67F'" );  // Page: Step,  Zone: Main,  Block: Notes
            // Attrib for BlockType: Step Type List:Step Program
            RockMigrationHelper.UpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "Programs", "", @"Display Step Types from a specified program. If none selected, the block will display the program from the current context.", 1, @"", "C46D400B-277B-4B9C-B4E2-FB767F73D88F" );
            // Attrib for BlockType: Steps:Step Program
            RockMigrationHelper.UpdateBlockTypeAttribute( "5D5EB7BA-A9CE-4801-8168-6CA8ECD354D4", "33875369-7D2B-4CD7-BB89-ABC29906CCAE", "Step Program", "StepProgram", "", @"The Step Program to display. This value can also be a page parameter: StepProgramId. Leave this attribute blank to use the page parameter.", 1, @"", "B4E07CC9-53E0-47CF-AC22-10F2085547A3" );
            // Attrib Value for Block:Notes, Attribute:Entity Type Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"8eadb0dc-17f4-4541-a46e-53f89e21a622" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" );
            // Attrib Value for Block:Notes, Attribute:Heading Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-quote-left" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Attrib Value for Block:Notes, Attribute:Allow Backdated Notes Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note Types Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4", @"2678D220-2852-49B7-963F-CA36BD1B6DBB" );
            // Attrib Value for Block:Notes, Attribute:Display Note Type Heading Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "C5FD0719-1E03-4C17-BE31-E02A3637C39A", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note View Lava Template Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307", @"{% include '~~/Assets/Lava/NoteViewList.lava' %}" );
            // Attrib Value for Block:Notes, Attribute:Expand Replies Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9410BE4C-134E-47D1-93D0-62248280B67F", "84E53A88-32D2-432C-8BB5-600BDBA10949", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note View Lava Template Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "328DDE3F-6FFF-4CA4-B6D0-C1BD4D643307", @"{% include '~~/Assets/Lava/NoteViewList.lava' %}" );
            // Attrib Value for Block:Notes, Attribute:Expand Replies Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "84E53A88-32D2-432C-8BB5-600BDBA10949", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Note Type Heading Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "C5FD0719-1E03-4C17-BE31-E02A3637C39A", @"False" );
            // Attrib Value for Block:Notes, Attribute:Note Types Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "CB89C2A5-49DB-4108-B924-6C610CEDFBF4", @"2678D220-2852-49B7-963F-CA36BD1B6DBB" );
            // Attrib Value for Block:Notes, Attribute:Allow Backdated Notes Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "6184511D-CC68-4FF2-90CB-3AD0AFD59D61", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );
            // Attrib Value for Block:Notes, Attribute:Heading Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-quote-left" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"True" );
            // Attrib Value for Block:Notes, Attribute:Entity Type Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"8eadb0dc-17f4-4541-a46e-53f89e21a622" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: Step, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "457489AD-D236-4994-8A0C-6E89423FBE3B", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"True" );
            // Add/Update PageContext for Page:Step, Entity: Rock.Model.Step, Parameter: StepId
            RockMigrationHelper.UpdatePageContext( "2109228C-D828-4B58-9310-8D93D10B846E", "Rock.Model.Step", "StepId", "C73B4EC9-4572-44F4-8E10-A7069FCD5233" );
            // Add/Update PageContext for Page:Step, Entity: Rock.Model.Step, Parameter: StepId
            RockMigrationHelper.UpdatePageContext( "7A04966A-8E4E-49EA-A03C-7DD4B52A7B28", "Rock.Model.Step", "StepId", "B6C2659F-4C2F-4942-B476-427CFB49B979" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Steps:Step Program
            RockMigrationHelper.DeleteAttribute( "B4E07CC9-53E0-47CF-AC22-10F2085547A3" );
            // Attrib for BlockType: Step Type List:Step Program
            RockMigrationHelper.DeleteAttribute( "C46D400B-277B-4B9C-B4E2-FB767F73D88F" );
            // Remove Block: Notes, from Page: Step, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "457489AD-D236-4994-8A0C-6E89423FBE3B" );
            // Remove Block: Notes, from Page: Step, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9410BE4C-134E-47D1-93D0-62248280B67F" );
            // Delete PageContext for Page:Step, Entity: Rock.Model.Step, Parameter: StepId
            RockMigrationHelper.DeletePageContext( "C73B4EC9-4572-44F4-8E10-A7069FCD5233" );
            // Delete PageContext for Page:Step, Entity: Rock.Model.Step, Parameter: StepId
            RockMigrationHelper.DeletePageContext( "B6C2659F-4C2F-4942-B476-427CFB49B979" );
        }
    }
}
