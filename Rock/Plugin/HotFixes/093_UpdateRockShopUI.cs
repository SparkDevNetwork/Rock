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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plugin Migration.
    /// </summary>
    [MigrationNumber( 93, "1.10.1" )]
    public class UpdateRockShopUI : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdateRockShopFrontpageUI();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// GJ: Added two additional blocks to the Rock Shop page.
        /// </summary>
        private void UpdateRockShopFrontpageUI()
        {
                        // Add/Update HtmlContent for Block: Store Control Panel
            RockMigrationHelper.UpdateHtmlContentBlock("6CE75972-1204-4DE7-8BBB-C779973BFDFD",@"<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h1 class=""panel-title""><i class=""fa fa-gift""></i> Store Links</h1>
    </div>
    <ul class=""list-group"">
        <li class=""list-group-item""><a href=""~/RockShop/Purchases"">Purchases</a></li>
        <li class=""list-group-item""><a href=""https://www.rockrms.com/rockshop/support"">Support</a></li>
        <li class=""list-group-item""><a href=""~/RockShop/Account"">Account</a></li>
    </ul>
</div>","E4381B48-166F-45C8-923C-DCA75FCD033C");

            // Add Block to Page: Rock Shop Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"B8F1B648-8C5F-4529-8F8B-B564C2A19061".AsGuid(), "Sponsored Apps","Main",@"",@"",3,"FA0152C9-71E1-47FF-9704-8D5EB39261DA");
            // Add Block to Page: Rock Shop Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"B8F1B648-8C5F-4529-8F8B-B564C2A19061".AsGuid(), "Top Free","Main",@"",@"",4,"C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8");
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql(@"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955'");  // Page: Rock Shop,  Zone: Main,  Block: Promo Rotator
            Sql(@"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'A239E904-3E32-462E-B97D-388E7E87C37F'");  // Page: Rock Shop,  Zone: Main,  Block: Package Category Header List
            Sql(@"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '8D23BB71-69D9-4409-8368-1D965A3C5128'");  // Page: Rock Shop,  Zone: Main,  Block: Featured Promos
            Sql(@"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = 'FA0152C9-71E1-47FF-9704-8D5EB39261DA'");  // Page: Rock Shop,  Zone: Main,  Block: Sponsored Apps
            Sql(@"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = 'C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8'");  // Page: Rock Shop,  Zone: Main,  Block: Top Free
            // Attrib Value for Block:Featured Promos, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8D23BB71-69D9-4409-8368-1D965A3C5128","96D8DDDB-A253-445A-A6A8-4F124977A788",@"d6dc6afe-70d9-43cf-9d76-eaee2317fb14");
            // Attrib Value for Block:Featured Promos, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8D23BB71-69D9-4409-8368-1D965A3C5128","40C7B22F-DF83-4BB6-8004-381FCF23398E",@"{% include '~/Assets/Lava/Store/PromoList.lava' %}");
            // Attrib Value for Block:Featured Promos, Attribute:Promo Type Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8D23BB71-69D9-4409-8368-1D965A3C5128","14926702-E8C8-4833-B430-19CA640E9877",@"Featured");
            // Attrib Value for Block:Featured Promos, Attribute:Category Id Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8D23BB71-69D9-4409-8368-1D965A3C5128","C1BB7381-F29C-4F24-B455-F31009BE1046",@"");
            // Attrib Value for Block:Store Control Panel, Attribute:Cache Duration Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4",@"3600");
            // Attrib Value for Block:Store Control Panel, Attribute:Require Approval Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A",@"False");
            // Attrib Value for Block:Store Control Panel, Attribute:Context Name Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","466993F7-D838-447A-97E7-8BBDA6A57289",@"store-controlpanel");
            // Attrib Value for Block:Store Control Panel, Attribute:Enable Versioning Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","7C1CE199-86CF-4EAE-8AB3-848416A72C58",@"False");
            // Attrib Value for Block:Store Control Panel, Attribute:Context Parameter Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","3FFC512D-A576-4289-B648-905FD7A64ABB",@"");
            // Attrib Value for Block:Store Control Panel, Attribute:Start in Code Editor mode Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","0673E015-F8DD-4A52-B380-C758011331B2",@"True");
            // Attrib Value for Block:Store Control Panel, Attribute:Document Root Folder Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","3BDB8AED-32C5-4879-B1CB-8FC7C8336534",@"~/Content");
            // Attrib Value for Block:Store Control Panel, Attribute:User Specific Folders Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE",@"False");
            // Attrib Value for Block:Store Control Panel, Attribute:Image Root Folder Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","26F3AFC6-C05B-44A4-8593-AFE1D9969B0E",@"~/Content");
            // Attrib Value for Block:Package Categories, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("061820D6-5CDE-428F-AB29-7E2A7AE90600","15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53",@"{% include '~/Assets/Lava/Store/PackageCategoryListSidebar.lava' %}");
            // Attrib Value for Block:Package Categories, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("061820D6-5CDE-428F-AB29-7E2A7AE90600","CFEA2CB4-2303-429C-B096-CBD2413AD56B",@"50d17fe7-88db-46b2-9c58-df8c0de376a4");
            // Attrib Value for Block:Sponsored Apps, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("FA0152C9-71E1-47FF-9704-8D5EB39261DA","40C7B22F-DF83-4BB6-8004-381FCF23398E",@"{% include '~/Assets/Lava/Store/PromoListSponsored.lava' %}");
            // Attrib Value for Block:Sponsored Apps, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("FA0152C9-71E1-47FF-9704-8D5EB39261DA","96D8DDDB-A253-445A-A6A8-4F124977A788",@"d6dc6afe-70d9-43cf-9d76-eaee2317fb14");
            // Attrib Value for Block:Sponsored Apps, Attribute:Promo Type Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("FA0152C9-71E1-47FF-9704-8D5EB39261DA","14926702-E8C8-4833-B430-19CA640E9877",@"Top Paid");
            // Attrib Value for Block:Top Free, Attribute:Promo Type Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8","14926702-E8C8-4833-B430-19CA640E9877",@"Top Free");
            // Attrib Value for Block:Top Free, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8","96D8DDDB-A253-445A-A6A8-4F124977A788",@"d6dc6afe-70d9-43cf-9d76-eaee2317fb14");
            // Attrib Value for Block:Top Free, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8","40C7B22F-DF83-4BB6-8004-381FCF23398E",@"{% include '~/Assets/Lava/Store/PromoListTopFree.lava' %}");

        }
    }
}
