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

    /// <summary>
    ///
    /// </summary>
    public partial class DeepLinking : Rock.Migrations.RockMigration
    {
        const string parentPageGuid = "A4B0BCBB-721D-439C-8566-24F604DD4A1C";
        const string newPageGuid = "07E421F9-BF0E-4BBB-A594-743C8678EE88";
        const string blockTypeGuid = "5C157EBD-2482-4393-A309-A872F774E19F";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update BlockType 
            //   Name: Mobile Deep Link Detail
            //   Category: Mobile
            //   Path: ~/Blocks/Mobile/MobileDeepLinkDetail.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Mobile Deep Link Detail", "Edits and configures the settings of mobile deep-link routes.", "~/Blocks/Mobile/MobileDeepLinkDetail.ascx", "Mobile", blockTypeGuid );

            // Add Page 
            //  Internal Name: Deep Links
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, parentPageGuid, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Deep Links", "Manage your deep link routes.", "07E421F9-BF0E-4BBB-A594-743C8678EE88", "" );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route
            //   Page:Deep Links
            //   Route:admin/cms/mobile-applications/{SiteId}/deeplinks
            RockMigrationHelper.AddPageRoute( "07E421F9-BF0E-4BBB-A594-743C8678EE88", "admin/cms/mobile-applications/{SiteId}/deeplinks", "07D9E32F-1756-4E54-A3CA-4A29E6FB7A8E" );
#pragma warning restore CS0618 // Type or member is obsolete

            // Add Block 
            //  Block Name: Mobile Deep Link Detail
            //  Page Name: DeepLinks
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, newPageGuid.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), blockTypeGuid.AsGuid(), "Mobile Deep Link Detail", "Main", @"", @"", 0, "B27D3A8A-84A4-43FE-8102-9067D606A1F6" );

            // Attribute for BlockType
            //   BlockType: Mobile Application Detail
            //   Category: Mobile
            //   Attribute: Deep Link Detail
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D001ED9-F711-4820-BED0-92150D069BA2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Deep Link Detail", "DeepLinkDetail", "Deep Link Detail", @"", 0, @"", "4DC70EA3-18C2-4A39-870E-726002511BA1" );

            // Add Block Attribute Value
            //   Block: Mobile Application Detail
            //   BlockType: Mobile Application Detail
            //   Category: Mobile
            //   Block Location: Page=Application Detail, Site=Rock RMS
            //   Attribute: Deep Link Detail
            /*   Attribute Value: 07e421f9-bf0e-4bbb-a594-743c8678ee88 */
            RockMigrationHelper.AddBlockAttributeValue( "48EDB434-8F21-4FC7-8599-9825F7A6103D", "4DC70EA3-18C2-4A39-870E-726002511BA1", @"07E421F9-BF0E-4BBB-A594-743C8678EE88" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( "48EDB434-8F21-4FC7-8599-9825F7A6103D", "4DC70EA3-18C2-4A39-870E-726002511BA1" );
            RockMigrationHelper.DeleteBlockAttribute( "4DC70EA3-18C2-4A39-870E-726002511BA1" );
            RockMigrationHelper.DeletePage( "07E421F9-BF0E-4BBB-A594-743C8678EE88" );
            RockMigrationHelper.DeletePageRoute( "07D9E32F-1756-4E54-A3CA-4A29E6FB7A8E" );
            RockMigrationHelper.DeleteBlock( "B27D3A8A-84A4-43FE-8102-9067D606A1F6" );
            RockMigrationHelper.DeleteBlockType( blockTypeGuid );
        }
    }
}
