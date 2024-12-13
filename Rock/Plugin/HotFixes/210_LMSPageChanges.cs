﻿// <copyright>
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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 210, "1.17.0" )]
    public class LMSPageChanges : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            // Removes legacy values that are no longer used or have been repurposed.
            // This needs to run before the RockMigrationHelper method calls
            // to prevent potential conflicts with Attribute Keys.
            RemoveLegacyValues();

            // Add Page 
            //  Internal Name: Program Summary Pages
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Program Summary Pages", "Container page for the Sub Menu of the LMS Program Summary.", "7ECE252B-6844-474C-AEFD-307E1DDA3A83", "" );

            // Add Page 
            //  Internal Name: Program Detail Pages
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "84DBEC51-EE0B-41C2-94B3-F361C4B98879", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Program Detail Pages", "Container page for the Sub Menu of the LMS Program Detail.", "6BDF3243-72BA-4C08-80BD-B76E40667A33", "" );

            // Add Page 
            //  Internal Name: Current Classes
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7ECE252B-6844-474C-AEFD-307E1DDA3A83", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Current Classes", "", "56459D93-32DF-4151-8F6D-003B9AFF0F94", "" );

            // Add Page 
            //  Internal Name: Completions
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7ECE252B-6844-474C-AEFD-307E1DDA3A83", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Completions", "LMS Program Completions", "395BE5DD-E524-4B75-A4CA-5A0548645647", "" );

            // Add Page 
            //  Internal Name: Courses
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "6BDF3243-72BA-4C08-80BD-B76E40667A33", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Courses", "", "0E5103B8-EF4A-46C9-8F76-313A259B0A3C", "" );

            // Add Page 
            //  Internal Name: Semesters
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "6BDF3243-72BA-4C08-80BD-B76E40667A33", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Semesters", "", "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C", "" );

            // Add Page Route
            //   Page:Current Classes
            //   Route:learning/{LearningProgramId}/summary/classes
            RockMigrationHelper.AddOrUpdatePageRoute( "56459D93-32DF-4151-8F6D-003B9AFF0F94", "learning/{LearningProgramId}/classes", "56A1387A-DDDE-46D9-A23D-B19D6A3BFC50" );

            // Add Page Route
            //   Page:Completions
            //   Route:learning/{LearningProgramId}/summary/completions
            RockMigrationHelper.AddOrUpdatePageRoute( "395BE5DD-E524-4B75-A4CA-5A0548645647", "learning/{LearningProgramId}/completions", "1AA1F901-A07C-4F64-A8EF-70A4160C0F22" );

            // Add Page Route
            //   Page:Courses
            //   Route:learning/{LearningProgramId}/summary/courses
            RockMigrationHelper.AddOrUpdatePageRoute( "0E5103B8-EF4A-46C9-8F76-313A259B0A3C", "learning/{LearningProgramId}/courses", "5208B1E5-BE28-44D0-9DE2-D2B1A26471AE" );

            // Add Page Route
            //   Page:Semesters
            //   Route:learning/{LearningProgramId}/summary/semesters
            RockMigrationHelper.AddOrUpdatePageRoute( "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C", "learning/{LearningProgramId}/semesters", "E0F8F5D7-99C7-4FC3-9205-D9100E1F1027" );

            // Add Block 
            //  Block Name: Current Classes
            //  Page Name: Current Classes
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "56459D93-32DF-4151-8F6D-003B9AFF0F94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "340F6CC1-8C38-4579-9383-A6168680194A".AsGuid(), "Current Classes", "Main", @"", @"", 2, "41645AA0-1899-4B08-9833-B61B23BB7294" );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Current Classes
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "56459D93-32DF-4151-8F6D-003B9AFF0F94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Learning Program Detail", "Main", @"", @"", 0, "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770" );

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Current Classes
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "56459D93-32DF-4151-8F6D-003B9AFF0F94".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6" );

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "395BE5DD-E524-4B75-A4CA-5A0548645647".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "CB9CC864-1958-4A2A-8424-42A028134006" );

            // Add Block 
            //  Block Name: Learning Program Completion List
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "395BE5DD-E524-4B75-A4CA-5A0548645647".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CE703EB6-028F-47FC-A09A-AD8F72C12CBC".AsGuid(), "Learning Program Completion List", "Main", @"", @"", 2, "319F2F80-A12C-48E8-B5A1-434C1BCF0AD2" );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "395BE5DD-E524-4B75-A4CA-5A0548645647".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Learning Program Detail", "Main", @"", @"", 0, "28F72594-912E-4C73-9E89-DF3C4D93863B" );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Courses
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0E5103B8-EF4A-46C9-8F76-313A259B0A3C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Learning Program Detail", "Main", @"", @"", 0, "AB20591D-C843-4099-966D-D54101793288" );

            // Add Block 
            //  Block Name: Learning Course List
            //  Page Name: Courses
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0E5103B8-EF4A-46C9-8F76-313A259B0A3C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D0AFDF98-4AFC-4E4F-A6E2-07CA4E7358E8".AsGuid(), "Learning Course List", "Main", @"", @"", 2, "53830D4A-32F3-4543-8DF2-8E46046BBF4E" );

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Courses
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0E5103B8-EF4A-46C9-8F76-313A259B0A3C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "B3C55400-76E9-42D9-9ECA-842FBFC7C123" );

            // Add Block 
            //  Block Name: Page Menu
            //  Page Name: Semesters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Main", @"", @"", 1, "F8273D25-960A-4081-BCBF-2E45433C398C" );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Semesters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "796C87E7-678F-4A38-8C04-A401A4F7AC21".AsGuid(), "Learning Program Detail", "Main", @"", @"", 0, "539CFC03-C265-4D3F-BE11-B592E3969969" );

            // Add Block 
            //  Block Name: Learning Semester List
            //  Page Name: Semesters
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C89C7F15-FB8A-43D4-9AFB-5E40E397F246".AsGuid(), "Learning Semester List", "Main", @"", @"", 2, "E8076C21-D1A3-4D6E-A4AB-0352462DDA74" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Completions,  Zone: Main,  Block: Learning Program Completion List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '319F2F80-A12C-48E8-B5A1-434C1BCF0AD2'" );

            // Update Order for Page: Completions,  Zone: Main,  Block: Learning Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '28F72594-912E-4C73-9E89-DF3C4D93863B'" );

            // Update Order for Page: Completions,  Zone: Main,  Block: Page Menu
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'CB9CC864-1958-4A2A-8424-42A028134006'" );

            // Update Order for Page: Courses,  Zone: Main,  Block: Learning Course List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '53830D4A-32F3-4543-8DF2-8E46046BBF4E'" );

            // Update Order for Page: Courses,  Zone: Main,  Block: Learning Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'AB20591D-C843-4099-966D-D54101793288'" );

            // Update Order for Page: Courses,  Zone: Main,  Block: Page Menu
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B3C55400-76E9-42D9-9ECA-842FBFC7C123'" );

            // Update Order for Page: Current Classes,  Zone: Main,  Block: Current Classes
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '41645AA0-1899-4B08-9833-B61B23BB7294'" );

            // Update Order for Page: Current Classes,  Zone: Main,  Block: Learning Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770'" );

            // Update Order for Page: Current Classes,  Zone: Main,  Block: Page Menu
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '89AE19CD-E801-4685-9F4B-FE2D9AC00BD6'" );

            // Update Order for Page: Semesters,  Zone: Main,  Block: Learning Program Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '539CFC03-C265-4D3F-BE11-B592E3969969'" );

            // Update Order for Page: Semesters,  Zone: Main,  Block: Learning Semester List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'E8076C21-D1A3-4D6E-A4AB-0352462DDA74'" );

            // Update Order for Page: Semesters,  Zone: Main,  Block: Page Menu
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'F8273D25-960A-4081-BCBF-2E45433C398C'" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the Detail view for the learning program (if Display Mode is 'Summary').", 4, @"", "06B0D94D-7A16-4E4E-A53A-743EE89804D3" );
            
            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Display Mode", "DisplayMode", "Display Mode", @"Select 'Summary' to show the summary page with (optional) KPIs and the gear icon that navigates to the traditional 'Detail' view.", 3, @"Summary", "18731EB1-888B-4AB2-B1C1-759493B2E639" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Attribute Display Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "796C87E7-678F-4A38-8C04-A401A4F7AC21", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Attribute Display Mode", "AttributeDisplayMode", "Attribute Display Mode", @"Select 'Is Grid Column' to show only attributes that are 'Show on Grid'. Select 'All' to show all attributes.", 3, @"Is Grid Column", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F" );

            // Add Block Attribute Value
            //   Block: Learning Program List
            //   BlockType: Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "B7D8BC2F-CEDD-4E22-8D46-69A759DCB59C", "9CB472E3-755B-4702-ADC7-06FF6FF6B598", @"False" );

            // Value: 56459d93-32df-4151-8f6d-003b9aff0f94,56a1387a-ddde-46d9-a23d-b19d6a3bfc50

            // Add Block Attribute Value
            //   Block: Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Course, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "0AE21CCB-BE0C-4565-8EC9-33A61C503DC0", "A0B96A3E-E7CB-45A7-898F-C798CD8FFF9D", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Attribute Display Mode
            /*   Attribute Value: Is Grid Column */
            RockMigrationHelper.AddBlockAttributeValue( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F", @"Is Grid Column" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 0e5103b8-ef4a-46c9-8f76-313a259b0a3c,5208b1e5-be28-44d0-9de2-d2b1a26471ae */
            RockMigrationHelper.AddBlockAttributeValue( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770", "06B0D94D-7A16-4E4E-A53A-743EE89804D3", @"0e5103b8-ef4a-46c9-8f76-313a259b0a3c,5208b1e5-be28-44d0-9de2-d2b1a26471ae" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Summary */
            RockMigrationHelper.AddBlockAttributeValue( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Summary" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 7ece252b-6844-474c-aefd-307e1dda3a83 */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7ece252b-6844-474c-aefd-307e1dda3a83" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Show Location Column
            /*   Attribute Value: No */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "4FBE92ED-3CF1-4BD4-A78E-B50C0080FA0B", @"No" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Show Schedule Column
            /*   Attribute Value: No */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "BECBD2AE-5D1E-4065-8CAA-FE7972CC5742", @"No" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Show Semester Column
            /*   Attribute Value: No */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "28087DBE-A334-47BF-9CA5-76AFF5EE1A6F", @"No" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "0C995F98-F483-4814-B3A1-6FACCD2D686F", @"23d5076e-c062-4987-9985-b3a4792bf3ce,5fce29a7-4530-4cce-9891-c95242923efe" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "4778450C-5B2A-40AD-9FC0-B3ED8AAE2F5C", @"True" );

            // Add Block Attribute Value
            //   Block: Current Classes
            //   BlockType: Learning Class List
            //   Category: LMS
            //   Block Location: Page=Current Classes, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "41645AA0-1899-4B08-9833-B61B23BB7294", "A0B96A3E-E7CB-45A7-898F-C798CD8FFF9D", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Attribute Display Mode
            /*   Attribute Value: Is Grid Column */
            RockMigrationHelper.AddBlockAttributeValue( "28F72594-912E-4C73-9E89-DF3C4D93863B", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F", @"Is Grid Column" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 0e5103b8-ef4a-46c9-8f76-313a259b0a3c,5208b1e5-be28-44d0-9de2-d2b1a26471ae */
            RockMigrationHelper.AddBlockAttributeValue( "28F72594-912E-4C73-9E89-DF3C4D93863B", "06B0D94D-7A16-4E4E-A53A-743EE89804D3", @"0e5103b8-ef4a-46c9-8f76-313a259b0a3c,5208b1e5-be28-44d0-9de2-d2b1a26471ae" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "28F72594-912E-4C73-9E89-DF3C4D93863B", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Summary */
            RockMigrationHelper.AddBlockAttributeValue( "28F72594-912E-4C73-9E89-DF3C4D93863B", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Summary" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 7ece252b-6844-474c-aefd-307e1dda3a83 */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7ece252b-6844-474c-aefd-307e1dda3a83" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "CB9CC864-1958-4A2A-8424-42A028134006", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Detail */
            RockMigrationHelper.AddBlockAttributeValue( "AB20591D-C843-4099-966D-D54101793288", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Detail" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "AB20591D-C843-4099-966D-D54101793288", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Attribute Display Mode
            /*   Attribute Value: Is Grid Column */
            RockMigrationHelper.AddBlockAttributeValue( "AB20591D-C843-4099-966D-D54101793288", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F", @"Is Grid Column" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 6bdf3243-72ba-4c08-80bd-b76e40667a33 */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"6bdf3243-72ba-4c08-80bd-b76e40667a33" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "B3C55400-76E9-42D9-9ECA-842FBFC7C123", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: a57d990e-6f34-45cf-abaa-08c40e8d4844,c77ebcb8-f174-4f2d-8113-48d9b0d489ea */
            RockMigrationHelper.AddBlockAttributeValue( "53830D4A-32F3-4543-8DF2-8E46046BBF4E", "67E7F552-C0F2-4852-B817-E216795D1E30", @"a57d990e-6f34-45cf-abaa-08c40e8d4844,c77ebcb8-f174-4f2d-8113-48d9b0d489ea" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "53830D4A-32F3-4543-8DF2-8E46046BBF4E", "223232AD-21C4-4024-ADE2-B9180B165728", @"True" );

            // Add Block Attribute Value
            //   Block: Learning Course List
            //   BlockType: Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "53830D4A-32F3-4543-8DF2-8E46046BBF4E", "184900A4-881E-4EA1-B047-D865A4B948AF", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Attribute Display Mode
            /*   Attribute Value: Is Grid Column */
            RockMigrationHelper.AddBlockAttributeValue( "539CFC03-C265-4D3F-BE11-B592E3969969", "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F", @"Is Grid Column" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Display Mode
            /*   Attribute Value: Detail */
            RockMigrationHelper.AddBlockAttributeValue( "539CFC03-C265-4D3F-BE11-B592E3969969", "18731EB1-888B-4AB2-B1C1-759493B2E639", @"Detail" );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Show KPIs
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "539CFC03-C265-4D3F-BE11-B592E3969969", "6AE52A7E-EFA3-4685-B331-A2D3058438D3", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Is Secondary Block
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Include Current QueryString
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Root Page
            /*   Attribute Value: 6bdf3243-72ba-4c08-80bd-b76e40667a33 */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"6bdf3243-72ba-4c08-80bd-b76e40667a33" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Number of Levels
            /*   Attribute Value: 1 */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Include Current Parameters
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Category: CMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Template
            /*   Attribute Value: {% include '~~/Assets/Lava/PageListAsTabs.lava' %} */
            RockMigrationHelper.AddBlockAttributeValue( "F8273D25-960A-4081-BCBF-2E45433C398C", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}" );

            // Add Block Attribute Value
            //   Block: Learning Semester List
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: core.CustomGridEnableStickyHeaders
            /*   Attribute Value: False */
            RockMigrationHelper.AddBlockAttributeValue( "E8076C21-D1A3-4D6E-A4AB-0352462DDA74", "29621F9F-24AE-4698-B719-D5D3FA1A6C91", @"False" );

            // Add Block Attribute Value
            //   Block: Learning Semester List
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 36ffa805-b283-443e-990d-87040339d16f,d796b863-964f-4a10-a880-9d398376851a */
            RockMigrationHelper.AddBlockAttributeValue( "E8076C21-D1A3-4D6E-A4AB-0352462DDA74", "EEA24DC4-4CFC-4B34-9348-917839DDBBA2", @"36ffa805-b283-443e-990d-87040339d16f,d796b863-964f-4a10-a880-9d398376851a" );

            // Add Block Attribute Value
            //   Block: Learning Semester List
            //   BlockType: Learning Semester List
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "E8076C21-D1A3-4D6E-A4AB-0352462DDA74", "57106CC3-5D67-40BA-809B-2EA9826478D8", @"True" );

            AttributeValuesUp();
        }

        private void RemoveLegacyValues()
        {
            Sql( $@"
-- Remove the Legacy DisplayMode Attribute before adding the new one.
DECLARE @programDetailBlockTypeId NVARCHAR(200) = (
	select [Id] 
	from BlockType 
	where [Guid] = '796c87e7-678f-4a38-8c04-a401a4f7ac21'
);

DECLARE @legacyDisplayModeAttributeId INT = (
	select a.Id
	from Attribute a
	where [Key] = 'DisplayMode'
		AND EntityTypeQualifierColumn = 'BlockTypeId'
		AND EntityTypeQualifierValue = @programDetailBlockTypeId
		AND Description = 'Select ''Summary'' to show only attributes that are ''Show on Grid''. Select ''Full'' to show all attributes.'
);

DELETE AttributeValue WHERE AttributeId = @legacyDisplayModeAttributeId
DELETE Attribute WHERE Id = @legacyDisplayModeAttributeId
" );
        }

        private void AttributeValuesUp()
        {
            Sql( $@"
-- Well-known Guids.
DECLARE 
	-- New summary and detail page roots (for sub-menu).
	@summaryPageRootGuid NVARCHAR(40) = '7ECE252B-6844-474C-AEFD-307E1DDA3A83',
	@detailPageRootGuid NVARCHAR(40) = '6BDF3243-72BA-4C08-80BD-B76E40667A33',

	-- The new Completions page guid for removing breadcrumbs.
	@programCompletionsPageGuid NVARCHAR(40) = '395BE5DD-E524-4B75-A4CA-5A0548645647',

	-- The new Courses list page guid for identifying the new home of the course detail page.
	@coursesPageGuid NVARCHAR(40) = '0E5103B8-EF4A-46C9-8F76-313A259B0A3C',
	
	-- The new Semesters list page guid for identifying the new home of the Semester Detail
	@semestersPageGuid NVARCHAR(40) = '0D89CFE6-BA23-4AC0-AF95-1016BAEF734C',

	-- Program List page and block type for identifying the AttributeValue to update
	-- for the grid click detail page.
	@programListPageGuid NVARCHAR(40) = '84DBEC51-EE0B-41C2-94B3-F361C4B98879',
	@programListBlockTypeGuid NVARCHAR(40) = '7b1db013-a552-455f-a1d0-7b17488d0d5c',

	-- Classes page and route to use for updating program list block attribute (detail page)
	@currentClassesPageGuid NVARCHAR(40) = '56459D93-32DF-4151-8F6D-003B9AFF0F94',
	@summaryClassesRouteGuid NVARCHAR(40) = '56A1387A-DDDE-46D9-A23D-B19D6A3BFC50',

	-- Course detail page to be moved
	@courseDetailPageGuid NVARCHAR(40) = 'A57D990E-6F34-45CF-ABAA-08C40E8D4844',

	-- Legacy pages to be removed.
	@legacyProgramDetailPageGuid NVARCHAR(40) = '7888caf4-af5d-44ba-ab9e-80138361f69d',
	@legacyCoursesPageGuid NVARCHAR(40) = '870318D8-5381-4B3C-BE4A-04E57125B590',
	@legacySemestersPageGuid NVARCHAR(40) = 'F7073393-D3A7-4C2E-8001-A73F9E169D60',
	@legacyProgramCompletionsPageGuid NVARCHAR(40) = '532BC5A9-40B3-42AF-9AD3-740FC0B3EB41',
	
	-- Detail necessary for adding Content Page and Announcement attribute values
	-- to Course Detail and Class Detail blocks.
	@classDetailPageGuid NVARCHAR(40) = '23D5076E-C062-4987-9985-B3A4792BF3CE',
	@blockEntityTypeId NVARCHAR(40) = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65'),
	@classDetailBlockGuid NVARCHAR(40) = 'C67D2164-33E5-46C0-94EF-DF387EF8477D',
	@courseDetailBlockGuid NVARCHAR(40) = 'D85084D3-E298-4307-9AA2-C1570C4A3FA4',

	@contentPageGuid NVARCHAR(40) = 'E6A89360-B7B4-48A8-B799-39A27EAB6F36',
	@announcementPageGuid NVARCHAR(40) = '7C8134A4-524A-4C3D-BA4C-875FEE672850',

	@contentPageRouteGuid NVARCHAR(40) = '4F35DC5D-FBB5-4B10-91B4-BC1C6A7009E8',
	@announcementRouteGuid NVARCHAR(40) = '864A8615-AC20-4B3A-9D5F-087E92859AD1',

	@contentDetailPageAttributeValue NVARCHAR(100),
	@announcementPageAttributeValue NVARCHAR(100);

-- Include the route when setting the Program List block's DetailPage attribute value.
DECLARE @summaryClassesPageAndRouteAttributeValue NVARCHAR(MAX) =
    CONCAT(@currentClassesPageGuid, ',', @summaryClassesRouteGuid);

-- Get the Block.Id of the Program List Block for the AttributeValue.EntityId.
DECLARE @programListBlockId INT = (
	SELECT TOP 1 b.[Id]
	FROM [dbo].[Block] b
	JOIN [dbo].[Page] p ON p.[Id] = b.[PageId]
	JOIN [dbo].[BlockType] bt on bt.[Id] = b.[BlockTypeId]
	WHERE bt.[Guid] = @programListBlockTypeGuid
		AND p.[Guid] = @programListPageGuid
);

-- Only update the DetailPage Attribute if the value
-- exactly matches the originally configured value.
UPDATE av SET
	[Value] = @summaryClassesPageAndRouteAttributeValue
FROM Attribute a
JOIN AttributeValue av ON av.AttributeId = a.Id 
WHERE av.EntityId = @programListBlockId
	AND a.[Key] = 'DetailPage'
	AND av.[Value] = @legacyProgramDetailPageGuid

-- We've modified the layout and blocks and this detail page is no longer used.
-- Get the Id so we can update child pages to point to the new parent page 
-- before deleting the parent.
DECLARE @legacyProgramDetailPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @legacyProgramDetailPageGuid
);

-- The new layout has a page with a submenu - 
-- this is the page where the Courses sub-menu is selected.
DECLARE @programDetailCoursesPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @coursesPageGuid
);

-- Update the ParentPage for the children of the legacy program detail page.
UPDATE p SET
	ParentPageId = @programDetailCoursesPageId
FROM [Page] p
WHERE [ParentPageId] = @legacyProgramDetailPageId

-- We've modified the layout and blocks and this detail page is no longer used.
-- Get the Id so we can update child pages to point to the new parent page 
-- before deleting the parent.
DECLARE @legacySemestersPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @legacySemestersPageGuid
);

-- The new layout has a page with a submenu - 
-- this is the page where the Courses sub-menu is selected.
DECLARE @semestersPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @semestersPageGuid
);

-- Update the ParentPage for the legacy semester detail page.
UPDATE p SET
	ParentPageId = @semestersPageId
FROM [Page] p
WHERE [ParentPageId] = @legacySemestersPageId

DECLARE @coursesPageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @coursesPageGuid
)

DECLARE @coursePageId INT = (
	SELECT TOP 1 [Id]
	FROM [Page]
	WHERE [Guid] = @courseDetailPageGuid
);

-- Re-home the Course Page to the new parent page under 
-- Program Detail Pages > Courses.
UPDATE p SET
	ParentPageId = @coursesPageId
FROM [Page] p
WHERE [Guid] = @courseDetailPageGuid

-- Delete the legacy pages.
DELETE [Page] WHERE [Guid] IN ( 
	@legacyCoursesPageGuid, 
	@legacyProgramCompletionsPageGuid, 
	@legacyProgramDetailPageGuid, 
	@legacySemestersPageGuid
);

-- Don't show breadcrumbs for sub-menu root pages 
-- or the pages available from the sub-menu.
UPDATE p SET
	p.BreadCrumbDisplayName = 0
FROM [Page] p
WHERE p.BreadCrumbDisplayName = 1
	AND p.[Guid] IN (
		@summaryPageRootGuid, 
		@detailPageRootGuid,
		@currentClassesPageGuid,
		@semestersPageGuid,
		@coursesPageGuid,
		@programCompletionsPageGuid
	)

-- Set the AttributeValues for the Content Detail and Announcement Detail block settings.
SELECT 
	@contentDetailPageAttributeValue = CONCAT(@contentPageGuid, ',', @contentPageRouteGuid),
	@announcementPageAttributeValue = CONCAT(@announcementPageGuid, ',', @announcementRouteGuid);

-- Add the block settings to both blocks (if they don't already exist).
 INSERT INTO [AttributeValue] (
     [IsSystem],
	 [AttributeId],
	 [EntityId],
     [Value],
     [Guid])
SELECT 0, 
	a.Id,
	b.Id,
	CASE 
		WHEN a.[Key] = 'ContentDetailPage' THEN @contentDetailPageAttributeValue
		WHEN a.[Key] = 'AnnouncementDetailPage' THEN @announcementPageAttributeValue
	END [AttributeValue],
	NEWID()
FROM [dbo].[Attribute] a
JOIN [dbo].[Block] b ON b.[Guid] IN (@classDetailBlockGuid, @courseDetailBlockGuid)
WHERE a.[Key] IN ('ContentDetailPage', 'AnnouncementDetailPage')
	AND NOT EXISTS (
		SELECT *
		FROM [dbo].[AttributeValue] av
		WHERE av.AttributeId = a.Id
			AND av.EntityId = b.Id
	)
" );
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Attribute Display Mode
            RockMigrationHelper.DeleteAttribute( "85AD9ED7-6AC2-4392-B448-7FD377D3DE9F" );

            // Attribute for BlockType
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "06B0D94D-7A16-4E4E-A53A-743EE89804D3" );

            // Remove Block
            //  Name: Learning Semester List, from Page: Semesters, Site: Rock RMS
            //  from Page: Semesters, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E8076C21-D1A3-4D6E-A4AB-0352462DDA74" );

            // Remove Block
            //  Name: Page Menu, from Page: Semesters, Site: Rock RMS
            //  from Page: Semesters, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F8273D25-960A-4081-BCBF-2E45433C398C" );

            // Remove Block
            //  Name: Learning Program Detail, from Page: Semesters, Site: Rock RMS
            //  from Page: Semesters, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "539CFC03-C265-4D3F-BE11-B592E3969969" );

            // Remove Block
            //  Name: Learning Course List, from Page: Courses, Site: Rock RMS
            //  from Page: Courses, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "53830D4A-32F3-4543-8DF2-8E46046BBF4E" );

            // Remove Block
            //  Name: Page Menu, from Page: Courses, Site: Rock RMS
            //  from Page: Courses, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B3C55400-76E9-42D9-9ECA-842FBFC7C123" );

            // Remove Block
            //  Name: Learning Program Detail, from Page: Courses, Site: Rock RMS
            //  from Page: Courses, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "AB20591D-C843-4099-966D-D54101793288" );

            // Remove Block
            //  Name: Learning Program Completion List, from Page: Completions, Site: Rock RMS
            //  from Page: Completions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "319F2F80-A12C-48E8-B5A1-434C1BCF0AD2" );

            // Remove Block
            //  Name: Page Menu, from Page: Completions, Site: Rock RMS
            //  from Page: Completions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CB9CC864-1958-4A2A-8424-42A028134006" );

            // Remove Block
            //  Name: Learning Program Detail, from Page: Completions, Site: Rock RMS
            //  from Page: Completions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "28F72594-912E-4C73-9E89-DF3C4D93863B" );

            // Remove Block
            //  Name: Current Classes, from Page: Current Classes, Site: Rock RMS
            //  from Page: Current Classes, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "41645AA0-1899-4B08-9833-B61B23BB7294" );

            // Remove Block
            //  Name: Page Menu, from Page: Current Classes, Site: Rock RMS
            //  from Page: Current Classes, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "89AE19CD-E801-4685-9F4B-FE2D9AC00BD6" );

            // Remove Block
            //  Name: Learning Program Detail, from Page: Current Classes, Site: Rock RMS
            //  from Page: Current Classes, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CBD3BE2F-1BF1-4ADF-99A5-774D4C4E4770" );

            // Delete Page 
            //  Internal Name: Semesters
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "0D89CFE6-BA23-4AC0-AF95-1016BAEF734C" );

            // Delete Page 
            //  Internal Name: Courses
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "0E5103B8-EF4A-46C9-8F76-313A259B0A3C" );

            // Delete Page 
            //  Internal Name: Completions
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "395BE5DD-E524-4B75-A4CA-5A0548645647" );

            // Delete Page 
            //  Internal Name: Current Classes
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "56459D93-32DF-4151-8F6D-003B9AFF0F94" );

            // Delete Page 
            //  Internal Name: Program Detail Pages
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "6BDF3243-72BA-4C08-80BD-B76E40667A33" );

            // Delete Page 
            //  Internal Name: Program Summary Pages
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "7ECE252B-6844-474C-AEFD-307E1DDA3A83" );
        }
    }
}
