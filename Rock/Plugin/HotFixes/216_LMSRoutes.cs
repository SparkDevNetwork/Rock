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
    [MigrationNumber( 216, "1.17.0" )]
    public class LMSRoutes : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateRoutesAndBinaryFileType();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }

        private void UpdateRoutesAndBinaryFileType()
        {
            // Page and Route Guids
            var publicLearnPageGuid = "B32639B3-604F-441E-A6E4-2613C0A6BE2B";
            var publicCourseDetailPageGuid = "FCCDB330-E1EA-4DC2-971E-3900F1EC2826";
            var publicCourseListPageGuid = "5B9DEA29-8C8F-4EDD-9FCF-739061B654D3";
            var publicCourseWorkspacePageGuid = "61BE63C7-6611-4235-A6F2-B22456620F35";
            var enrollPageGuid = "4CDD7BB9-A23E-42E0-BC0B-24D0E2360993";
            var enrollPageRouteGuid = "25FC2399-4885-460B-9608-7005DB17819E";
            var courseListRouteGuid = "FA31BDF7-875A-4AAA-BD27-734FF10AF61A";
            var courseDetailRouteGuid = "6AC3D62C-488B-44C8-98AF-7D23B7B701DD";
            var courseWorkspaceRouteGuid = "E2EF9FAC-3E9B-4EC8-A21F-D01178416247";

            var enrollmentBlockTypeGuid = "E80F9006-3C00-4F36-839E-7A0883F9E229";
            var enrollmentBlockGuid = "13557A75-5374-4965-B2F6-14DC04764057";
            var pageReferenceFieldTypeGuid = "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108";

            // Attribute Guids
            var courseListPageAttributeGuid = "D80D46CC-716C-48F2-A1F0-8382A01A6957";
            var courseDetailPageAttributeGuid = "F39FC142-A196-407B-BE0D-066CD3C18525";
            var classWorkspacePageAttributeGuid = "AB0300A9-4C91-493B-BFB0-7A63514DA1E9";

            // Update the page layout for public pages to use 'Full Width' instead of 'Home Page'.
            // Except the Workspace which should not include the breadcrumbs.
            RockMigrationHelper.UpdatePageLayout( publicLearnPageGuid, SystemGuid.Layout.FULL_WIDTH );
            RockMigrationHelper.UpdatePageLayout( publicCourseListPageGuid, SystemGuid.Layout.FULL_WIDTH );
            RockMigrationHelper.UpdatePageLayout( publicCourseDetailPageGuid, SystemGuid.Layout.FULL_WIDTH );

            // Update the Public Course Detail page title to "Course Description"
            Sql( $@"
UPDATE [dbo].[Page] SET
    [InternalName] = 'Course Description',
    [PageTitle] = 'Course Description',
    [BrowserTitle] = 'Course Description'
WHERE [Guid] = '{publicCourseDetailPageGuid}'
" );

            // Add the enrollment page, route, block and initial attributes/values.
            RockMigrationHelper.AddPage( publicCourseDetailPageGuid, SystemGuid.Layout.FULL_WIDTH, "Enroll", "", enrollPageGuid, "", "" );

            RockMigrationHelper.AddOrUpdatePageRoute( enrollPageGuid , "learn/{LearningProgramId}/courses/{LearningCourseId}/enroll/{LearningClassId}", enrollPageRouteGuid );

            RockMigrationHelper.AddSecurityAuthForPage(
                enrollPageGuid,
                0,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUnAuthenticatedUsers,
                Guid.NewGuid().ToString() );

            // Add the linked page attributes for the enroll block.
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Lms.PublicLearningClassEnrollment
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Lms.PublicLearningClassEnrollment", "Public Learning Class Enrollment", "Rock.Blocks.Lms.PublicLearningClassEnrollment, Rock.Blocks, Version=1.17.0.31, Culture=neutral, PublicKeyToken=null", false, false, "4F9F2B15-14EF-47AB-858B-641858674AC7" );


            // Add/Update Obsidian Block Type
            //   Name:Public Learning Class Enrollment
            //   Category:LMS
            //   EntityType:Rock.Blocks.Lms.PublicLearningClassEnrollment
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Public Learning Class Enrollment", "Allows the current person to enroll in a learning class.", "Rock.Blocks.Lms.PublicLearningClassEnrollment", "LMS", enrollmentBlockTypeGuid );

            // Add Block 
            //  Block Name: Public Learning Class Enrollment
            //  Page Name: Enroll
            //  Layout: -
            //  Site: External Website
            RockMigrationHelper.AddBlock( true, enrollPageGuid.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), enrollmentBlockTypeGuid.AsGuid(), "Enroll", "Feature", @"", @"", 1, enrollmentBlockGuid );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Course List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( enrollmentBlockTypeGuid, pageReferenceFieldTypeGuid, "Course List Page", "CourseListPage", "Course List Page", @"Page to use for links back to the course list.", 1, @"", courseListPageAttributeGuid );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Course Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( enrollmentBlockTypeGuid, pageReferenceFieldTypeGuid, "Course Detail Page", "CourseDetailPage", "Course Detail Page", @"Page to use for links back to the course detail.", 1, @"", courseDetailPageAttributeGuid );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Course Workspace Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( enrollmentBlockTypeGuid, pageReferenceFieldTypeGuid, "Class Workspace Page", "ClassWorkspacePage", "Class Workspace Page", @"Page to use for links to the class workspace", 1, @"", classWorkspacePageAttributeGuid );

            // Add Block Attribute Value
            //   Block: Public Learning Class Enrollment
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Block Location: Page=Enroll, Site=External Site
            //   Attribute: Course List Page
            /*   Attribute Value: PAGE_GUID,ROUTE_GUID */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, enrollmentBlockGuid, courseListPageAttributeGuid, $@"{publicCourseListPageGuid},{courseListRouteGuid}" );

            // Add Block Attribute Value
            //   Block: Public Learning Class Enrollment
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Block Location: Page=Enroll, Site=External Site
            //   Attribute: Course Detail Page
            /*   Attribute Value: PAGE_GUID,ROUTE_GUID */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, enrollmentBlockGuid, courseDetailPageAttributeGuid, $@"{publicCourseDetailPageGuid},{courseDetailRouteGuid}" );

            // Add Block Attribute Value
            //   Block: Public Learning Class Enrollment
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Block Location: Page=Enroll, Site=External Site
            //   Attribute: Class Workspace Page
            /*   Attribute Value: PAGE_GUID,ROUTE_GUID */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, enrollmentBlockGuid, classWorkspacePageAttributeGuid, $@"{publicCourseWorkspacePageGuid},{courseWorkspaceRouteGuid}" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Enrollment Page
            /*   Attribute Value: 61be63c7-6611-4235-a6f2-b22456620f35,e2ef9fac-3e9b-4ec8-a21f-d01178416247 */
            RockMigrationHelper.AddBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "96644CEF-4FC7-4986-B591-D6675AA38C2C", $@"{enrollPageGuid},{enrollPageRouteGuid}" );

            Sql( $@"
-- Update LMS internal page routes to use people/learn/ instead of learning/
UPDATE pr SET
	[Route] = 'people/learn' + RIGHT(pr.[Route], LEN(pr.[Route]) - LEN('learning'))
FROM [dbo].[PageRoute] pr
WHERE pr.[Route] NOT LIKE 'people/%'
	AND pr.[Guid] IN (
    'A3A52449-4B51-45CE-B91D-3AEEB42C1577', -- Learn
    'C77EBCB8-F174-4F2D-8113-48D9B0D489EA', -- Course
    '5FCE29A7-4530-4CCE-9891-C95242923EFE', -- Class
    'E2581432-C9D8-4324-97E2-BCFE6BBD0F57', -- Activity
    '827AF9A8-BF1A-4008-B4C3-3D07076ACB84', -- Participant
    'D796B863-964F-4A10-A880-9D398376851A', -- Semester
    '8C40AE8D-60C6-49DE-B7DE-BE46D8A64AA6', -- Completion
    '4F35DC5D-FBB5-4B10-91B4-BC1C6A7009E8', -- Content Page
    '864A8615-AC20-4B3A-9D5F-087E92859AD1', -- Announcement
    '56A1387A-DDDE-46D9-A23D-B19D6A3BFC50', -- Current Classes
    '1AA1F901-A07C-4F64-A8EF-70A4160C0F22', -- Completions
    '5208B1E5-BE28-44D0-9DE2-D2B1A26471AE', -- Courses
    'E0F8F5D7-99C7-4FC3-9205-D9100E1F1027' -- Semesters
)

-- Update the PageTitle for the main page on both internal and external sites.
UPDATE p SET
	InternalName = 'Learn',
	PageTitle = 'Learn',
	BrowserTitle = 'Learn'
FROM [Page] p
WHERE [Guid] IN (
	'84DBEC51-EE0B-41C2-94B3-F361C4B98879', -- Learning
	'B32639B3-604F-441E-A6E4-2613C0A6BE2B' -- Learning University (External Site)
)
	AND PageTitle IN ('Learning', 'Learning University')

-- Update the AllowAnonymous flag for Learning Management files so anyone can upload.
UPDATE bft SET
	[AllowAnonymous] = 1
FROM [dbo].[BinaryFileType] bft
WHERE bft.[Guid] = '4F55987B-5279-4D10-8C38-F320046B4BBB' -- Learning Management
" );
        }
    }
}
