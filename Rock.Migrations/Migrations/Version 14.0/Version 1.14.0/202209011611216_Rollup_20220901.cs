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
    public partial class Rollup_20220901 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F0735712-5892-4573-8564-9B6787EB90B0".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"699EE5D5-9256-4A8D-BE08-ECBE8FA48767"); 

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("699EE5D5-9256-4A8D-BE08-ECBE8FA48767","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");

            UpdateAppleTvRoute();
            ShowAppleTVAppsPage();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("699EE5D5-9256-4A8D-BE08-ECBE8FA48767");
        }

        /// <summary>
        /// BC: Fix Apple TV application page route + parameters
        /// </summary>
        private void UpdateAppleTvRoute()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.APPLE_TV_APPLICATION_SCREEN_DETAIL, "admin/cms/appletv-applications/{SiteId}/{SitePageId}" );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// NA: Show the Apple TV Apps Page
        /// </summary>
        private void ShowAppleTVAppsPage()
        {
            Sql( @"
                UPDATE [Page]
                SET [DisplayInNavWhen] = 0
                WHERE [Guid] = 'C8B81EBE-E98F-43EF-9E39-0491685145E2'" );
        }
    }
}
