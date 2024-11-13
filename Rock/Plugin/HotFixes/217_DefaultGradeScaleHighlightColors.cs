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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 217, "1.17.0" )]
    public class DefaultGradeScaleHighlightColors : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            RemovePublicLavaTemplateAttributeValues();
            SetGradingColors();
            SetFacilitatorPortalPage();

            // Update the layout for the Public Class Workspace to use the Full Width
            // layout so that breadcrumbs are shown.
            var publicClassWorkspacePageGuid = "61BE63C7-6611-4235-A6F2-B22456620F35";
            RockMigrationHelper.UpdatePageLayout( publicClassWorkspacePageGuid, SystemGuid.Layout.FULL_WIDTH );
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }

        private void SetFacilitatorPortalPage()
        {
            // Attribute for BlockType
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Attribute: Facilitator Portal Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55F2E89B-DE57-4E24-AC6C-576956FB97C5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Facilitator Portal Page", "FacilitatorPortalPage", "Facilitator Portal Page", @"The page that will be navigated to when clicking facilitator portal link.", 1, @"", "72DFE773-DA2F-45A8-976A-6C19FD0AFE28" );
            var facilitatorPortalPageAttributeGuid = "72DFE773-DA2F-45A8-976A-6C19FD0AFE28";
            var classWorkspaceBlockGuid = "D46C2787-60BA-4776-BE6E-7F785A984922";
            var internalClassDetailPageGuid = "23D5076E-C062-4987-9985-B3A4792BF3CE";
            var interalClassDetailPageRouteGuid = "5FCE29A7-4530-4CCE-9891-C95242923EFE";

            // Add Block Attribute Value
            //   Block: Public Learning Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Workspace, Site=External Site
            //   Attribute: Facilitator Portal Page
            /*   Attribute Value: PAGE_GUID,ROUTE_GUID */
            //   Skip If Already Exists: false
            RockMigrationHelper.AddBlockAttributeValue( false, classWorkspaceBlockGuid, facilitatorPortalPageAttributeGuid, $@"{internalClassDetailPageGuid},{interalClassDetailPageRouteGuid}" );
        }

        /// <summary>
        /// Removes the lava templates that were persisted by the code-gen scripts in <see cref="MigrationRollupsForV17_0_10"/>.
        /// </summary>
        private void RemovePublicLavaTemplateAttributeValues()
        {
            // Add Block Attribute Value
            //   Block: Program List
            //   BlockType: Public Learning Program List
            //   Category: LMS
            //   Block Location: Page=Learning University, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.DeleteBlockAttributeValue( "B15CC3F1-766B-4469-8F95-E31011A3279F", "D7B2012B-3D59-4DE3-8328-53B7EB9F7C1C" );

            // Add Block Attribute Value
            //   Block: Course List
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Block Location: Page=Courses, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.DeleteBlockAttributeValue( "7CE9134A-FF7B-4663-8B79-0CA6EED53A1A", "6ADA636C-F93A-4347-A2E6-3D9C1BDA51C0" );

            // Add Block Attribute Value
            //   Block: Course Detail
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Block Location: Page=Course, Site=External Website
            //   Attribute: Lava Template
            RockMigrationHelper.DeleteBlockAttributeValue( "E921788F-38EA-48F2-B80A-9B7181AB70A5", "0935CE10-BD61-4C85-9D6C-0269DEC2EEAC" );

            // Add Block Attribute Value
            //   Block: Class Workspace
            //   BlockType: Public Learning Class Workspace
            //   Category: LMS
            //   Block Location: Page=Class Workspace, Site=External Website
            //   Attribute: Lava Header Template
            /*   Attribute Value: ... */
            RockMigrationHelper.DeleteBlockAttributeValue( "D46C2787-60BA-4776-BE6E-7F785A984922", "F7F1BE33-AF44-4F73-9C55-0F8FF97E1B69" );

        }

        /// <summary>
        /// Sets the HighlightColors for the default grading system scales.
        /// </summary>
        private void SetGradingColors()
        {
            Sql( $@"
-- Pass/fail respectively.
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#34D87D' where [Guid] = 'C07A3227-7188-4D61-AC02-FF6AB8380AAD'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#FF624F' where [Guid] = 'BD209F2D-22E0-41A9-B425-ED42D515E13B'

-- A, B, C, D, F respectively.
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#34D87D' where [Guid] = 'F96BDDD2-EA0F-4C35-90BB-0B7D9FAABD26'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#CCE744' where [Guid] = 'E8128844-04B0-4772-AB59-55F17645AB7A'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#F7E64A' where [Guid] = 'A99DC539-D363-416F-BDA8-00163D186919'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#FAAE48' where [Guid] = '6E6A61C3-3305-491D-80C6-1C3723468460'
UPDATE [dbo].[LearningGradingSystemScale] set HighlightColor = '#FF624F' where [Guid] = '2F7885F5-4DFB-4057-92D7-2684B4542BF7'
" );
        }
    }
}
