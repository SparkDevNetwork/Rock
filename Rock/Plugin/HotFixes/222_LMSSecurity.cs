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

using Rock.Security;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 222, "1.17.0" )]
    public class LMSSecurity : Migration
    {
        private const string RSR_STAFF_GUID = "2C112948-FF4C-46E7-981A-0257681EADF4";
        private const string RSR_STAFF_LIKE_GUID = "300BA2C8-49A3-44BA-A82A-82E3FD8C3745";
        private const string RSR_LMS_ADMINISTRATION_GUID = "5E0E02A9-F16B-437E-A9D9-C3D9D6AFABB0";
        private const string RSR_LMS_WORKER_GUID = "B5481A0E-52E3-4D7B-93B5-A8F5C908DC67";

        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            AddLMSRoleAccess();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {

        }

        private void AddLMSRoleAccess()
        {
            // Page Guids
            var programListPageGuid = "84DBEC51-EE0B-41C2-94B3-F361C4B98879";
            var programClassesPageGuid = "56459D93-32DF-4151-8F6D-003B9AFF0F94";
            var programCompletionsPageGuid = "395BE5DD-E524-4B75-A4CA-5A0548645647";
            var programCompletionDetailPageGuid = "DF896952-E38A-490D-BF85-3601249C3630";
            var classWorkspacePageGuid = "61BE63C7-6611-4235-A6F2-B22456620F35";

            // Route Guids
            var programSummaryClassesRouteGuid = "56A1387A-DDDE-46D9-A23D-B19D6A3BFC50";
            var programCompletionDetailRouteGuid = "1394A3BA-6CB0-4301-9C2E-3F58BA3A8AEB";

            // Block Guids
            var programDetailForCoursesBlockGuid = "AB20591D-C843-4099-966D-D54101793288";
            var programDetailForSemestersBlockGuid = "539CFC03-C265-4D3F-BE11-B592E3969969";
            var programCompletionListBlockGuid = "319F2F80-A12C-48E8-B5A1-434C1BCF0AD2";
            var programCompletionDetailBlockGuid = "C30D2737-97F7-46C9-8DA8-EDA937EA0D15";

            // Block Type Guids
            var programCompletionDetailBlockTypeGuid = "e0c38a42-2ace-4258-8d11-bd971c41eadb";

            var programSummaryClassListPageAndRouteGuids = $"{programClassesPageGuid},{programSummaryClassesRouteGuid}";

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Courses, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: {programClassesPageGuid},{programSummaryClassesRouteGuid} */
            RockMigrationHelper.AddBlockAttributeValue( programDetailForCoursesBlockGuid, "06B0D94D-7A16-4E4E-A53A-743EE89804D3", programSummaryClassListPageAndRouteGuids );

            // Add Block Attribute Value
            //   Block: Learning Program Detail
            //   BlockType: Learning Program Detail
            //   Category: LMS
            //   Block Location: Page=Semesters, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( programDetailForSemestersBlockGuid, "06B0D94D-7A16-4E4E-A53A-743EE89804D3", programSummaryClassListPageAndRouteGuids );

            // Require authentication for the Class Workspace page.
            RockMigrationHelper.AddSecurityAuthForPage(
                classWorkspacePageGuid,
                0,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUnAuthenticatedUsers,
                Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityRoleGroup( "RSR - LMS Administration", "Group of individuals who can administrate the various parts of the LMS functionality.", RSR_LMS_ADMINISTRATION_GUID );
            RockMigrationHelper.AddSecurityRoleGroup( "RSR - LMS Workers", "Group of individuals who have basic access to the LMS functionality (such as facilitators).", RSR_LMS_WORKER_GUID );

            // Programs Page.
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 0, Authorization.VIEW, true, RSR_LMS_ADMINISTRATION_GUID, 0, "C8FD52E6-D626-4D07-B1D0-4F73ECEEFA27" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 0, Authorization.EDIT, true, RSR_LMS_ADMINISTRATION_GUID, 0, "31BA820C-FBE6-4781-A7CC-4D60FA43506B" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 0, Authorization.ADMINISTRATE, true, RSR_LMS_ADMINISTRATION_GUID, 0, "5532D681-F77E-47CF-83CA-75E1EF36045C" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 1, Authorization.VIEW, true, RSR_LMS_WORKER_GUID, 0, "98D8FA2F-194D-46E8-A7AA-8FF3413A1621" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 2, Authorization.VIEW, true, RSR_STAFF_LIKE_GUID, 0, "5DF0316A-AEF6-497B-8152-86EAD097D179" );
            RockMigrationHelper.AddSecurityAuthForPage( programListPageGuid, 3, Authorization.VIEW, true, RSR_STAFF_GUID, 0, "F777AE37-D28A-45FF-9419-95CA86E8E67A" );

            // Add the program completion detail page, route.
            RockMigrationHelper.AddPage( true, programCompletionsPageGuid, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Program Completion Detail", "", programCompletionDetailPageGuid, "", "" );

            RockMigrationHelper.AddOrUpdatePageRoute( programCompletionDetailPageGuid, "people/learn/{LearningProgramId}/completions/{LearningProgramCompletionId}", programCompletionDetailRouteGuid );

            // Add Block 
            //  Block Name: Learning Program Detail
            //  Page Name: Completions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, programCompletionDetailPageGuid.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), programCompletionDetailBlockTypeGuid.AsGuid(), "Program Completion Detail", "Main", @"", @"", 0, programCompletionDetailBlockGuid );

            // Add Block Attribute Value
            //   Block: Learning Program Completion List
            //   BlockType: Learning Program Completion List
            //   Category: LMS
            //   Block Location: Page=Completions, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value:  */
            RockMigrationHelper.AddBlockAttributeValue( programCompletionListBlockGuid, "206A8316-1203-4661-A9E7-A4032C930075", $"{programCompletionDetailPageGuid},{programCompletionDetailRouteGuid}" );
        }
    }
}
