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
    public partial class UpdateLMSClassPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Move Page 
            //  Internal Name: Class
            //  From under: Learning Program Configuration > Courses
            //  To under: Learning Program (aka originally "Current Classes")
            RockMigrationHelper.MovePage( "23D5076E-C062-4987-9985-B3A4792BF3CE", "56459D93-32DF-4151-8F6D-003B9AFF0F94" );

            // Add New "Class" Page 
            //  Internal Name: Class
            //  Site: Rock RMS
            // Under: Learning Program Configuration > Course
            RockMigrationHelper.AddPage( true, "A57D990E-6F34-45CF-ABAA-08C40E8D4844", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Class", "", "47100A6A-F6B9-4E11-B657-F9795A58E11C", "" );

            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '47100A6A-F6B9-4E11-B657-F9795A58E11C'" );

            // Add Page Route
            //   Page:Class (new)
            //   Route: people/learn/{LearningProgramId}/courses/{LearningCourseId}/class-configuration/{LearningClassId}
            RockMigrationHelper.AddOrUpdatePageRoute( "47100A6A-F6B9-4E11-B657-F9795A58E11C", "people/learn/{LearningProgramId}/courses/{LearningCourseId}/class-configuration/{LearningClassId}", "EA6C217F-1836-4689-8815-EC0A155BDCE3" );

            // Add new Block Type attribute
            // Attribute for BlockType
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Attribute: Show Course Name in Breadcrumb
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D5369F8D-11AA-482B-AE08-2B3C519D8D87", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Course Name in Breadcrumb", "ShowCourseNameInBreadcrumb", "Show Course Name in Breadcrumb", @"If enabled, the breadcrumb will display the course name before the class name.", 5, @"False", "F2E69EC0-6974-419E-90D2-5C96B6F29D81" );

            // Now, set the original "Learning Class Detail" block's value (the one on the page we just moved) to be "True"

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Show Course Name in Breadcrumb
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "C67D2164-33E5-46C0-94EF-DF387EF8477D", "F2E69EC0-6974-419E-90D2-5C96B6F29D81", @"True" );

            // Add Block 
            //  Block Name: Learning Class Detail
            //  Page Name: Class
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "47100A6A-F6B9-4E11-B657-F9795A58E11C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D5369F8D-11AA-482B-AE08-2B3C519D8D87".AsGuid(), "Class", "Main", @"", @"", 0, "640A2A76-8C25-4186-8B26-5BD22F8AD989" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Activity Detail Page
            /*   Attribute Value: d72dcbc4-c57f-4028-b503-1954925edc7d,e2581432-c9d8-4324-97e2-bcfe6bbd0f57 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "640A2A76-8C25-4186-8B26-5BD22F8AD989", "44565F75-B4CA-4438-A9B6-BEB943813559", @"d72dcbc4-c57f-4028-b503-1954925edc7d,e2581432-c9d8-4324-97e2-bcfe6bbd0f57" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Participant Detail Page
            /*   Attribute Value: 72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "640A2A76-8C25-4186-8B26-5BD22F8AD989", "1B237D13-A86A-42CC-8E91-96CBCCAC6866", @"72c75c91-18f8-48d0-b0cf-1fbd82eb50fc,827af9a8-bf1a-4008-b4c3-3d07076acb84" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Attendance Page
            /*   Attribute Value: c96e184d-62cb-4e6b-a53d-496903240e25,d8595643-6703-4b18-b9f6-0d213d86ed47 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "640A2A76-8C25-4186-8B26-5BD22F8AD989", "B417E2A7-BBA1-453F-9933-3BE439CD2063", @"c96e184d-62cb-4e6b-a53d-496903240e25,d8595643-6703-4b18-b9f6-0d213d86ed47" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Content Page Detail Page
            /*   Attribute Value: e6a89360-b7b4-48a8-b799-39a27eab6f36,4f35dc5d-fbb5-4b10-91b4-bc1c6a7009e8 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "640A2A76-8C25-4186-8B26-5BD22F8AD989", "AAF9675D-BAF0-4F25-8C63-4F864F4369C0", @"e6a89360-b7b4-48a8-b799-39a27eab6f36,4f35dc5d-fbb5-4b10-91b4-bc1c6a7009e8" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Announcement Detail Page
            /*   Attribute Value: 7c8134a4-524a-4c3d-ba4c-875fee672850 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "640A2A76-8C25-4186-8B26-5BD22F8AD989", "B570F05B-95DE-4F22-95FF-DDDCCE943E29", @"7c8134a4-524a-4c3d-ba4c-875fee672850,864a8615-ac20-4b3a-9d5f-087e92859ad1" );

            // Add Block Attribute Value
            //   Block: Class
            //   BlockType: Learning Class Detail
            //   Category: LMS
            //   Block Location: Page=Class, Site=Rock RMS
            //   Attribute: Show Course Name in Breadcrumb
            /*   Attribute Value: False */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "640A2A76-8C25-4186-8B26-5BD22F8AD989", "F2E69EC0-6974-419E-90D2-5C96B6F29D81", @"False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // There is no going backwards.  This migration is run-twice safe.
        }
    }
}
