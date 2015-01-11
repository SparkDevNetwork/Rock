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
    public partial class RockShopPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Rock Shop", "", "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "fa fa-cube" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Package Detail", "", "D6DC6AFE-70D9-43CF-9D76-EAEE2317FB14", "fa fa-cube" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Purchases", "", "6A163569-2826-4EF2-8208-879DDBDC0896", "fa fa-shopping-cart " ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Packages By Category", "", "50D17FE7-88DB-46B2-9C58-DF8C0DE376A4", "fa fa-folder" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Package Install", "", "6029A6D6-0059-4CC1-9751-1F012BC267F1", "fa fa-cloud-download" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Link Organization", "", "6E029432-56F4-46AD-9D9C-C122F3D3C55C", "fa fa-link" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "Account", "", "DADE879D-8DF5-4367-89EF-FEECD12B81AB", "fa fa-user" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Account Redirect", "Redirects client to the organization's account page on the Rock website.", "~/Blocks/Store/AccountRedirect.ascx", "Store", "DBDB7C26-8271-4B1A-9791-71DEC99A7A2F" );
            RockMigrationHelper.UpdateBlockType( "Link Organization", "Links a Rock organization to the store.", "~/Blocks/Store/LinkOrganization.ascx", "Store", "41DFED6E-2ECD-4198-80C3-816B27241EB4" );
            RockMigrationHelper.UpdateBlockType( "Package Category List Lava", "Lists categories for Rock Store pages.", "~/Blocks/Store/PackageCategoryListLava.ascx", "Store", "470C6EFF-091C-4593-848C-49547D0EBEEE" );
            RockMigrationHelper.UpdateBlockType( "Package Detail", "Manages the details of a package.", "~/Blocks/Store/PackageDetail.ascx", "Store", "69A7D88E-5CD8-4993-A88A-4DA15BAD3CB3" );
            RockMigrationHelper.UpdateBlockType( "Package Detail Lava", "Displays details for a specific package.", "~/Blocks/Store/PackageDetailLava.ascx", "Store", "9EC29D0F-7EE7-434B-A30F-6C36A81B0DEB" );
            RockMigrationHelper.UpdateBlockType( "Package Install", "Installs a package.", "~/Blocks/Store/PackageInstall.ascx", "Store", "EA60C1AB-ADAB-4EDF-94F8-B0FE214B6F15" );
            RockMigrationHelper.UpdateBlockType( "Package List Lava", "Lists Rock Store packages using a Lava template.", "~/Blocks/Store/PackageListLava.ascx", "Store", "A494D4DD-0C96-4BA7-AF1B-43EFEF078261" );
            RockMigrationHelper.UpdateBlockType( "Promo List Lava", "Lists Rock Store promotions using a Liquid template.", "~/Blocks/Store/PromoListLava.ascx", "Store", "B8F1B648-8C5F-4529-8F8B-B564C2A19061" );
            RockMigrationHelper.UpdateBlockType( "Purchased Products", "Lists packages that have been purchased in the Rock Store.", "~/Blocks/Store/PurchasedPackages.ascx", "Store", "C0332D98-7CD0-43C2-9810-60F7DF86FBB6" );
            // Add Block to Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "", "B8F1B648-8C5F-4529-8F8B-B564C2A19061", "Promo Rotator", "Main", "", "", 0, "5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955" );

            // Add Block to Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "", "B8F1B648-8C5F-4529-8F8B-B564C2A19061", "Featured Promos", "Main", "", "", 1, "8D23BB71-69D9-4409-8368-1D965A3C5128" );

            // Add Block to Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Store Control Panel", "Sidebar1", "", "", 0, "6CE75972-1204-4DE7-8BBB-C779973BFDFD" );

            // Add Block to Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "", "470C6EFF-091C-4593-848C-49547D0EBEEE", "Package Categories", "Sidebar1", "", "", 1, "061820D6-5CDE-428F-AB29-7E2A7AE90600" );

            // Add Block to Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D6DC6AFE-70D9-43CF-9D76-EAEE2317FB14", "", "69A7D88E-5CD8-4993-A88A-4DA15BAD3CB3", "Package Detail", "Main", "", "", 0, "33D3ED38-5558-43EA-B108-6E36239272A0" );

            // Add Block to Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D6DC6AFE-70D9-43CF-9D76-EAEE2317FB14", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Store Control Panel", "Sidebar1", "", "", 0, "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE" );

            // Add Block to Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "D6DC6AFE-70D9-43CF-9D76-EAEE2317FB14", "", "470C6EFF-091C-4593-848C-49547D0EBEEE", "Package Category List Lava", "Sidebar1", "", "", 1, "35F68C3D-EF1D-442B-BDE1-115EE483EE34" );

            // Add Block to Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlock( "50D17FE7-88DB-46B2-9C58-DF8C0DE376A4", "", "A494D4DD-0C96-4BA7-AF1B-43EFEF078261", "Package List", "Main", "", "", 0, "79B32C17-FBE6-4543-87A4-50B86EFE8A84" );

            // Add Block to Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlock( "50D17FE7-88DB-46B2-9C58-DF8C0DE376A4", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Store Control Panel", "Sidebar1", "", "", 0, "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF" );

            // Add Block to Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlock( "50D17FE7-88DB-46B2-9C58-DF8C0DE376A4", "", "470C6EFF-091C-4593-848C-49547D0EBEEE", "Package Category List Lava", "Sidebar1", "", "", 1, "6D780ECF-41F6-4C7E-8FDC-86374474A5EE" );

            // Add Block to Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6029A6D6-0059-4CC1-9751-1F012BC267F1", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Store Control Panel", "Sidebar1", "", "", 0, "970DD682-FB10-4EE7-A91A-6552364442AC" );

            // Add Block to Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6029A6D6-0059-4CC1-9751-1F012BC267F1", "", "470C6EFF-091C-4593-848C-49547D0EBEEE", "Package Category List Lava", "Sidebar1", "", "", 1, "CE44B318-59D0-4572-B521-CB488340403F" );

            // Add Block to Page: Account, Site: Rock RMS
            RockMigrationHelper.AddBlock( "DADE879D-8DF5-4367-89EF-FEECD12B81AB", "", "DBDB7C26-8271-4B1A-9791-71DEC99A7A2F", "Account Redirect", "Main", "", "", 0, "48BCB5EE-3F4A-4871-A2B1-6E94240A09B5" );

            // Add Block to Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6A163569-2826-4EF2-8208-879DDBDC0896", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Store Control Panel", "Sidebar1", "", "", 0, "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE" );

            // Add Block to Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6A163569-2826-4EF2-8208-879DDBDC0896", "", "470C6EFF-091C-4593-848C-49547D0EBEEE", "Package Category List Lava", "Sidebar1", "", "", 1, "29A56943-3C69-4AA7-B6DA-95DFA948742A" );

            // Add Block to Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6A163569-2826-4EF2-8208-879DDBDC0896", "", "C0332D98-7CD0-43C2-9810-60F7DF86FBB6", "Purchased Products", "Main", "", "", 0, "CCA59B18-57FA-4920-9662-E06EF5099175" );

            // Add Block to Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6029A6D6-0059-4CC1-9751-1F012BC267F1", "", "EA60C1AB-ADAB-4EDF-94F8-B0FE214B6F15", "Package Install", "Main", "", "", 0, "7EB0E1E6-B342-4479-B834-D6EE0BD4646E" );

            // Add Block to Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6E029432-56F4-46AD-9D9C-C122F3D3C55C", "", "41DFED6E-2ECD-4198-80C3-816B27241EB4", "Link Organization", "Main", "", "", 0, "13614379-13B0-4245-98ED-FE78B27E8902" );

            // Add Block to Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6E029432-56F4-46AD-9D9C-C122F3D3C55C", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Store Control Panel", "Sidebar1", "", "", 0, "7D754D9D-E08A-4265-801D-6ABB8FA72404" );

            // Add Block to Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6E029432-56F4-46AD-9D9C-C122F3D3C55C", "", "470C6EFF-091C-4593-848C-49547D0EBEEE", "Package Category List Lava", "Sidebar1", "", "", 1, "F78A82BA-BD33-46DD-81ED-8CA80E3CF4FB" );

            // Add/Update HtmlContent for Block: Store Control Panel
            RockMigrationHelper.UpdateHtmlContentBlock( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", @"<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h1 class=""panel-title""><i class=""fa fa-gift""></i> Store Links</h1>
    </div>
    <div class=""panel-body"">
        <ul class=""list-unstyled"">
            <li><a href=""~/RockShop/Purchases"">Purchases</a></li>
            <li><a href=""https://www.rockrms.com/Store/Support"">Support</a></li>
            <li><a href=""~/RockShop/Account"">Account</a></li>
        </ul>
    </div>
</div>", "E4381B48-166F-45C8-923C-DCA75FCD033C" );

            // add context name
            Sql( @"UPDATE [HtmlContent] SET [EntityValue] = '&ContextName=store-controlpanel' WHERE [Guid] = 'E4381B48-166F-45C8-923C-DCA75FCD033C'" );

            // Attrib for BlockType: HTML Content:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Show lava merge fields.", 9, @"False", "48FF43A9-8E12-4768-80A9-88FBB81F11D8" );

            // Attrib for BlockType: Promo List Lava:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "B8F1B648-8C5F-4529-8F8B-B564C2A19061", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "D7D1BA6F-E8A1-4B4E-959B-5FA4CE621396" );

            // Attrib for BlockType: Promo List Lava:Category Id
            RockMigrationHelper.AddBlockTypeAttribute( "B8F1B648-8C5F-4529-8F8B-B564C2A19061", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Id", "CategoryId", "", "Filters promos for a specific category id. If none is provided it will show promos with no category.", 1, @"", "C1BB7381-F29C-4F24-B455-F31009BE1046" );

            // Attrib for BlockType: Promo List Lava:Promo Type
            RockMigrationHelper.AddBlockTypeAttribute( "B8F1B648-8C5F-4529-8F8B-B564C2A19061", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Promo Type", "PromoType", "", "Display the promos of the specified type", 0, @"Normal", "14926702-E8C8-4833-B430-19CA640E9877" );

            // Attrib for BlockType: Promo List Lava:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "B8F1B648-8C5F-4529-8F8B-B564C2A19061", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page reference to use for the detail page.", 4, @"", "96D8DDDB-A253-445A-A6A8-4F124977A788" );

            // Attrib for BlockType: Promo List Lava:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "B8F1B648-8C5F-4529-8F8B-B564C2A19061", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the promotions", 2, @"{% include '~/Assets/Lava/Store/PromoList.lava' %}", "40C7B22F-DF83-4BB6-8004-381FCF23398E" );

            // Attrib for BlockType: Package Category List Lava:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "470C6EFF-091C-4593-848C-49547D0EBEEE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "12BDD62A-D1D9-473A-803F-42C4C1183111" );

            // Attrib for BlockType: Package Category List Lava:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "470C6EFF-091C-4593-848C-49547D0EBEEE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the categories", 2, @"{% include '~/Assets/Lava/Store/PackageCategoryListSidebar.lava' %}", "15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53" );

            // Attrib for BlockType: Package Category List Lava:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "470C6EFF-091C-4593-848C-49547D0EBEEE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page reference to use for the detail page.", 4, @"", "CFEA2CB4-2303-429C-B096-CBD2413AD56B" );

            // Attrib for BlockType: Package Detail:Install Page
            RockMigrationHelper.AddBlockTypeAttribute( "69A7D88E-5CD8-4993-A88A-4DA15BAD3CB3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Install Page", "InstallPage", "", "Page reference to use for the install / update page.", 1, @"", "C212968E-5BE9-4397-ADA0-B4B6B768616C" );

            // Attrib for BlockType: Package List Lava:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "A494D4DD-0C96-4BA7-AF1B-43EFEF078261", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the packages", 2, @"{% include '~/Assets/Lava/Store/PackageList.lava' %}", "43EAF4C4-2D6A-4E5B-8914-F0C93FB6822A" );

            // Attrib for BlockType: Package List Lava:Package Type
            RockMigrationHelper.AddBlockTypeAttribute( "A494D4DD-0C96-4BA7-AF1B-43EFEF078261", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Package Type", "PackageType", "", "Display the packages of the specified type", 0, @"", "4B9C8920-5BCD-4B03-B5E4-C5AC42503940" );

            // Attrib for BlockType: Package List Lava:Category Id
            RockMigrationHelper.AddBlockTypeAttribute( "A494D4DD-0C96-4BA7-AF1B-43EFEF078261", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Id", "CategoryId", "", "Filters packages for a specific category id. If none is provided it will show all packages.", 1, @"", "92D02541-E8B9-4A4D-9FAF-60AD7DC1CB50" );

            // Attrib for BlockType: Package List Lava:Set Page Title
            RockMigrationHelper.AddBlockTypeAttribute( "A494D4DD-0C96-4BA7-AF1B-43EFEF078261", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "", "Determines if the block should set the page title with the category name (category name must be provided via the query string as &CategoryName=.)", 0, @"False", "801BD548-51DC-4F6F-93ED-CAB0D6243D2E" );

            // Attrib for BlockType: Package List Lava:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "A494D4DD-0C96-4BA7-AF1B-43EFEF078261", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "186642A4-D344-4D1C-9DCF-536176E51B93" );

            // Attrib for BlockType: Package List Lava:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "A494D4DD-0C96-4BA7-AF1B-43EFEF078261", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page reference to use for the detail page.", 4, @"", "6D95A177-DC77-414E-B1D0-FF710ADFA291" );

            // Attrib for BlockType: Account Redirect:Link Organization Page
            RockMigrationHelper.AddBlockTypeAttribute( "DBDB7C26-8271-4B1A-9791-71DEC99A7A2F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Link Organization Page", "LinkOrganizationPage", "", "Page to allow the user to link an organization to the store.", 0, @"", "253A5455-7E96-4C2F-B86B-CF86D8719CD2" );

            // Attrib for BlockType: Purchased Products:Link Organization Page
            RockMigrationHelper.AddBlockTypeAttribute( "C0332D98-7CD0-43C2-9810-60F7DF86FBB6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Link Organization Page", "LinkOrganizationPage", "", "Page to allow the user to link an organization to the store.", 0, @"", "6A1C6A72-C161-4E1C-B025-A87DE75DD6A7" );

            // Attrib for BlockType: Purchased Products:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "C0332D98-7CD0-43C2-9810-60F7DF86FBB6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page reference to use for the detail page.", 0, @"", "0C18831E-C9F7-4C79-89C8-CD24DDE9EFEB" );

            // Attrib for BlockType: Purchased Products:Install Page
            RockMigrationHelper.AddBlockTypeAttribute( "C0332D98-7CD0-43C2-9810-60F7DF86FBB6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Install Page", "InstallPage", "", "Page reference to use for the install / update page.", 0, @"", "1F03074E-ACC1-4DA2-A7F4-F7BACC4D8AE7" );

            // Attrib for BlockType: Package Install:Link Organization Page
            RockMigrationHelper.AddBlockTypeAttribute( "EA60C1AB-ADAB-4EDF-94F8-B0FE214B6F15", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Link Organization Page", "LinkOrganizationPage", "", "Page to allow the user to link an organization to the store.", 0, @"", "2E79CFA6-972A-4941-B268-8D22590F21E3" );

            // Attrib Value for Block:Promo Rotator, Attribute:Enable Debug Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955", "D7D1BA6F-E8A1-4B4E-959B-5FA4CE621396", @"False" );

            // Attrib Value for Block:Promo Rotator, Attribute:Category Id Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955", "C1BB7381-F29C-4F24-B455-F31009BE1046", @"" );

            // Attrib Value for Block:Promo Rotator, Attribute:Promo Type Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955", "14926702-E8C8-4833-B430-19CA640E9877", @"Featured" );

            // Attrib Value for Block:Promo Rotator, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955", "40C7B22F-DF83-4BB6-8004-381FCF23398E", @"{% include '~/Assets/Lava/Store/PromoRotator.lava' %}" );

            // Attrib Value for Block:Promo Rotator, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955", "96D8DDDB-A253-445A-A6A8-4F124977A788", @"d6dc6afe-70d9-43cf-9d76-eaee2317fb14" );

            // Attrib Value for Block:Featured Promos, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D23BB71-69D9-4409-8368-1D965A3C5128", "96D8DDDB-A253-445A-A6A8-4F124977A788", @"d6dc6afe-70d9-43cf-9d76-eaee2317fb14" );

            // Attrib Value for Block:Featured Promos, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D23BB71-69D9-4409-8368-1D965A3C5128", "40C7B22F-DF83-4BB6-8004-381FCF23398E", @"{% include '~/Assets/Lava/Store/PromoList.lava' %}" );

            // Attrib Value for Block:Featured Promos, Attribute:Promo Type Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D23BB71-69D9-4409-8368-1D965A3C5128", "14926702-E8C8-4833-B430-19CA640E9877", @"" );

            // Attrib Value for Block:Featured Promos, Attribute:Category Id Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D23BB71-69D9-4409-8368-1D965A3C5128", "C1BB7381-F29C-4F24-B455-F31009BE1046", @"" );

            // Attrib Value for Block:Featured Promos, Attribute:Enable Debug Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8D23BB71-69D9-4409-8368-1D965A3C5128", "D7D1BA6F-E8A1-4B4E-959B-5FA4CE621396", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Use Code Editor Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Parameter Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Name Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "466993F7-D838-447A-97E7-8BBDA6A57289", @"store-controlpanel" );

            // Attrib Value for Block:Store Control Panel, Attribute:Cache Duration Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );

            // Attrib Value for Block:Store Control Panel, Attribute:Require Approval Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Versioning Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Image Root Folder Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:User Specific Folders Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Document Root Folder Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Debug Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "6CE75972-1204-4DE7-8BBB-C779973BFDFD", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );

            // Attrib Value for Block:Package Categories, Attribute:Enable Debug Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "061820D6-5CDE-428F-AB29-7E2A7AE90600", "12BDD62A-D1D9-473A-803F-42C4C1183111", @"False" );

            // Attrib Value for Block:Package Categories, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "061820D6-5CDE-428F-AB29-7E2A7AE90600", "15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53", @"{% include '~/Assets/Lava/Store/PackageCategoryListSidebar.lava' %}" );

            // Attrib Value for Block:Package Categories, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "061820D6-5CDE-428F-AB29-7E2A7AE90600", "CFEA2CB4-2303-429C-B096-CBD2413AD56B", @"50d17fe7-88db-46b2-9c58-df8c0de376a4" );

            // Attrib Value for Block:Package Detail, Attribute:Install Page Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "33D3ED38-5558-43EA-B108-6E36239272A0", "C212968E-5BE9-4397-ADA0-B4B6B768616C", @"6029a6d6-0059-4cc1-9751-1f012bc267f1" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Debug Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:User Specific Folders Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Document Root Folder Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Parameter Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:Store Control Panel, Attribute:Image Root Folder Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Require Approval Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Versioning Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Cache Duration Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Name Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "466993F7-D838-447A-97E7-8BBDA6A57289", @"store-controlpanel" );

            // Attrib Value for Block:Store Control Panel, Attribute:Use Code Editor Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Detail Page Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "35F68C3D-EF1D-442B-BDE1-115EE483EE34", "CFEA2CB4-2303-429C-B096-CBD2413AD56B", @"50d17fe7-88db-46b2-9c58-df8c0de376a4" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Lava Template Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "35F68C3D-EF1D-442B-BDE1-115EE483EE34", "15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53", @"{% include '~/Assets/Lava/Store/PackageCategoryListSidebar.lava' %}" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Enable Debug Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "35F68C3D-EF1D-442B-BDE1-115EE483EE34", "12BDD62A-D1D9-473A-803F-42C4C1183111", @"False" );

            // Attrib Value for Block:Package List, Attribute:Lava Template Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "79B32C17-FBE6-4543-87A4-50B86EFE8A84", "43EAF4C4-2D6A-4E5B-8914-F0C93FB6822A", @"{% include '~/Assets/Lava/Store/PackageList.lava' %}" );

            // Attrib Value for Block:Package List, Attribute:Package Type Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "79B32C17-FBE6-4543-87A4-50B86EFE8A84", "4B9C8920-5BCD-4B03-B5E4-C5AC42503940", @"" );

            // Attrib Value for Block:Package List, Attribute:Category Id Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "79B32C17-FBE6-4543-87A4-50B86EFE8A84", "92D02541-E8B9-4A4D-9FAF-60AD7DC1CB50", @"" );

            // Attrib Value for Block:Package List, Attribute:Set Page Title Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "79B32C17-FBE6-4543-87A4-50B86EFE8A84", "801BD548-51DC-4F6F-93ED-CAB0D6243D2E", @"True" );

            // Attrib Value for Block:Package List, Attribute:Enable Debug Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "79B32C17-FBE6-4543-87A4-50B86EFE8A84", "186642A4-D344-4D1C-9DCF-536176E51B93", @"False" );

            // Attrib Value for Block:Package List, Attribute:Detail Page Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "79B32C17-FBE6-4543-87A4-50B86EFE8A84", "6D95A177-DC77-414E-B1D0-FF710ADFA291", @"d6dc6afe-70d9-43cf-9d76-eaee2317fb14" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Debug Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:User Specific Folders Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Document Root Folder Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Use Code Editor Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Attrib Value for Block:Store Control Panel, Attribute:Image Root Folder Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Versioning Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Cache Duration Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );

            // Attrib Value for Block:Store Control Panel, Attribute:Require Approval Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Parameter Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Name Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF", "466993F7-D838-447A-97E7-8BBDA6A57289", @"store-controlpanel" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Name Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "466993F7-D838-447A-97E7-8BBDA6A57289", @"store-controlpanel" );

            // Attrib Value for Block:Store Control Panel, Attribute:Cache Duration Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );

            // Attrib Value for Block:Store Control Panel, Attribute:Require Approval Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Versioning Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Parameter Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:Store Control Panel, Attribute:Image Root Folder Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Use Code Editor Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Attrib Value for Block:Store Control Panel, Attribute:Document Root Folder Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:User Specific Folders Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Debug Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "970DD682-FB10-4EE7-A91A-6552364442AC", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Enable Debug Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CE44B318-59D0-4572-B521-CB488340403F", "12BDD62A-D1D9-473A-803F-42C4C1183111", @"False" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Lava Template Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CE44B318-59D0-4572-B521-CB488340403F", "15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53", @"{% include '~/Assets/Lava/Store/PackageCategoryListSidebar.lava' %}" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Detail Page Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CE44B318-59D0-4572-B521-CB488340403F", "CFEA2CB4-2303-429C-B096-CBD2413AD56B", @"50d17fe7-88db-46b2-9c58-df8c0de376a4" );

            // Attrib Value for Block:Account Redirect, Attribute:Link Organization Page Page: Account, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "48BCB5EE-3F4A-4871-A2B1-6E94240A09B5", "253A5455-7E96-4C2F-B86B-CF86D8719CD2", @"6e029432-56f4-46ad-9d9c-c122f3d3c55c" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Name Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "466993F7-D838-447A-97E7-8BBDA6A57289", @"store-controlpanel" );

            // Attrib Value for Block:Store Control Panel, Attribute:Cache Duration Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );

            // Attrib Value for Block:Store Control Panel, Attribute:Require Approval Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Versioning Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Parameter Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:Store Control Panel, Attribute:Image Root Folder Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Use Code Editor Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Attrib Value for Block:Store Control Panel, Attribute:User Specific Folders Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Document Root Folder Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Debug Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Detail Page Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "29A56943-3C69-4AA7-B6DA-95DFA948742A", "CFEA2CB4-2303-429C-B096-CBD2413AD56B", @"50d17fe7-88db-46b2-9c58-df8c0de376a4" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Enable Debug Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "29A56943-3C69-4AA7-B6DA-95DFA948742A", "12BDD62A-D1D9-473A-803F-42C4C1183111", @"False" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Lava Template Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "29A56943-3C69-4AA7-B6DA-95DFA948742A", "15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53", @"{% include '~/Assets/Lava/Store/PackageCategoryListSidebar.lava' %}" );

            // Attrib Value for Block:Purchased Products, Attribute:Link Organization Page Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CCA59B18-57FA-4920-9662-E06EF5099175", "6A1C6A72-C161-4E1C-B025-A87DE75DD6A7", @"6e029432-56f4-46ad-9d9c-c122f3d3c55c" );

            // Attrib Value for Block:Purchased Products, Attribute:Detail Page Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CCA59B18-57FA-4920-9662-E06EF5099175", "0C18831E-C9F7-4C79-89C8-CD24DDE9EFEB", @"d6dc6afe-70d9-43cf-9d76-eaee2317fb14" );

            // Attrib Value for Block:Purchased Products, Attribute:Install Page Page: Purchases, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CCA59B18-57FA-4920-9662-E06EF5099175", "1F03074E-ACC1-4DA2-A7F4-F7BACC4D8AE7", @"6029a6d6-0059-4cc1-9751-1f012bc267f1" );

            // Attrib Value for Block:Package Install, Attribute:Link Organization Page Page: Package Install, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7EB0E1E6-B342-4479-B834-D6EE0BD4646E", "2E79CFA6-972A-4941-B268-8D22590F21E3", @"6e029432-56f4-46ad-9d9c-c122f3d3c55c" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Debug Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Document Root Folder Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:User Specific Folders Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Image Root Folder Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );

            // Attrib Value for Block:Store Control Panel, Attribute:Enable Versioning Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Parameter Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );

            // Attrib Value for Block:Store Control Panel, Attribute:Cache Duration Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"3600" );

            // Attrib Value for Block:Store Control Panel, Attribute:Require Approval Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );

            // Attrib Value for Block:Store Control Panel, Attribute:Context Name Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "466993F7-D838-447A-97E7-8BBDA6A57289", @"store-controlpanel" );

            // Attrib Value for Block:Store Control Panel, Attribute:Use Code Editor Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7D754D9D-E08A-4265-801D-6ABB8FA72404", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Detail Page Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F78A82BA-BD33-46DD-81ED-8CA80E3CF4FB", "CFEA2CB4-2303-429C-B096-CBD2413AD56B", @"50d17fe7-88db-46b2-9c58-df8c0de376a4" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Enable Debug Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F78A82BA-BD33-46DD-81ED-8CA80E3CF4FB", "12BDD62A-D1D9-473A-803F-42C4C1183111", @"False" );

            // Attrib Value for Block:Package Category List Lava, Attribute:Lava Template Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F78A82BA-BD33-46DD-81ED-8CA80E3CF4FB", "15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53", @"{% include '~/Assets/Lava/Store/PackageCategoryListSidebar.lava' %}" );

            RockMigrationHelper.AddPageRoute( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "RockShop" );
            RockMigrationHelper.AddPageRoute( "6A163569-2826-4EF2-8208-879DDBDC0896", "RockShop/Purchases" );
            RockMigrationHelper.AddPageRoute( "DADE879D-8DF5-4367-89EF-FEECD12B81AB", "RockShop/Account" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Package Install:Link Organization Page
            RockMigrationHelper.DeleteAttribute( "2E79CFA6-972A-4941-B268-8D22590F21E3" );
            // Attrib for BlockType: Purchased Products:Install Page
            RockMigrationHelper.DeleteAttribute( "1F03074E-ACC1-4DA2-A7F4-F7BACC4D8AE7" );
            // Attrib for BlockType: Purchased Products:Detail Page
            RockMigrationHelper.DeleteAttribute( "0C18831E-C9F7-4C79-89C8-CD24DDE9EFEB" );
            // Attrib for BlockType: Purchased Products:Link Organization Page
            RockMigrationHelper.DeleteAttribute( "6A1C6A72-C161-4E1C-B025-A87DE75DD6A7" );
            // Attrib for BlockType: Account Redirect:Link Organization Page
            RockMigrationHelper.DeleteAttribute( "253A5455-7E96-4C2F-B86B-CF86D8719CD2" );
            // Attrib for BlockType: Package List Lava:Detail Page
            RockMigrationHelper.DeleteAttribute( "6D95A177-DC77-414E-B1D0-FF710ADFA291" );
            // Attrib for BlockType: Package List Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "186642A4-D344-4D1C-9DCF-536176E51B93" );
            // Attrib for BlockType: Package List Lava:Set Page Title
            RockMigrationHelper.DeleteAttribute( "801BD548-51DC-4F6F-93ED-CAB0D6243D2E" );
            // Attrib for BlockType: Package List Lava:Category Id
            RockMigrationHelper.DeleteAttribute( "92D02541-E8B9-4A4D-9FAF-60AD7DC1CB50" );
            // Attrib for BlockType: Package List Lava:Package Type
            RockMigrationHelper.DeleteAttribute( "4B9C8920-5BCD-4B03-B5E4-C5AC42503940" );
            // Attrib for BlockType: Package List Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "43EAF4C4-2D6A-4E5B-8914-F0C93FB6822A" );
            // Attrib for BlockType: Package Detail:Install Page
            RockMigrationHelper.DeleteAttribute( "C212968E-5BE9-4397-ADA0-B4B6B768616C" );
            // Attrib for BlockType: Package Category List Lava:Detail Page
            RockMigrationHelper.DeleteAttribute( "CFEA2CB4-2303-429C-B096-CBD2413AD56B" );
            // Attrib for BlockType: Package Category List Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53" );
            // Attrib for BlockType: Package Category List Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "12BDD62A-D1D9-473A-803F-42C4C1183111" );
            // Attrib for BlockType: Promo List Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "40C7B22F-DF83-4BB6-8004-381FCF23398E" );
            // Attrib for BlockType: Promo List Lava:Detail Page
            RockMigrationHelper.DeleteAttribute( "96D8DDDB-A253-445A-A6A8-4F124977A788" );
            // Attrib for BlockType: Promo List Lava:Promo Type
            RockMigrationHelper.DeleteAttribute( "14926702-E8C8-4833-B430-19CA640E9877" );
            // Attrib for BlockType: Promo List Lava:Category Id
            RockMigrationHelper.DeleteAttribute( "C1BB7381-F29C-4F24-B455-F31009BE1046" );
            // Attrib for BlockType: Promo List Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "D7D1BA6F-E8A1-4B4E-959B-5FA4CE621396" );
            // Attrib for BlockType: HTML Content:Enable Debug
            RockMigrationHelper.DeleteAttribute( "48FF43A9-8E12-4768-80A9-88FBB81F11D8" );
            // Remove Block: Package Category List Lava, from Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F78A82BA-BD33-46DD-81ED-8CA80E3CF4FB" );
            // Remove Block: Store Control Panel, from Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7D754D9D-E08A-4265-801D-6ABB8FA72404" );
            // Remove Block: Link Organization, from Page: Link Organization, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "13614379-13B0-4245-98ED-FE78B27E8902" );
            // Remove Block: Package Install, from Page: Package Install, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7EB0E1E6-B342-4479-B834-D6EE0BD4646E" );
            // Remove Block: Purchased Products, from Page: Purchases, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CCA59B18-57FA-4920-9662-E06EF5099175" );
            // Remove Block: Package Category List Lava, from Page: Purchases, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "29A56943-3C69-4AA7-B6DA-95DFA948742A" );
            // Remove Block: Store Control Panel, from Page: Purchases, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E9DD57D4-7B4E-4C3A-950A-8370FBC60EBE" );
            // Remove Block: Account Redirect, from Page: Account, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "48BCB5EE-3F4A-4871-A2B1-6E94240A09B5" );
            // Remove Block: Package Category List Lava, from Page: Package Install, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CE44B318-59D0-4572-B521-CB488340403F" );
            // Remove Block: Store Control Panel, from Page: Package Install, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "970DD682-FB10-4EE7-A91A-6552364442AC" );
            // Remove Block: Package Category List Lava, from Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6D780ECF-41F6-4C7E-8FDC-86374474A5EE" );
            // Remove Block: Store Control Panel, from Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "44E6E8DD-0BA5-41CF-AEF4-D2E1E8CEE2BF" );
            // Remove Block: Package List, from Page: Packages By Category, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "79B32C17-FBE6-4543-87A4-50B86EFE8A84" );
            // Remove Block: Package Category List Lava, from Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "35F68C3D-EF1D-442B-BDE1-115EE483EE34" );
            // Remove Block: Store Control Panel, from Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "93B4798B-B2B1-4E20-A39D-A2B7D28DC7FE" );
            // Remove Block: Package Detail, from Page: Package Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "33D3ED38-5558-43EA-B108-6E36239272A0" );
            // Remove Block: Package Categories, from Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "061820D6-5CDE-428F-AB29-7E2A7AE90600" );
            // Remove Block: Store Control Panel, from Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6CE75972-1204-4DE7-8BBB-C779973BFDFD" );
            // Remove Block: Featured Promos, from Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8D23BB71-69D9-4409-8368-1D965A3C5128" );
            // Remove Block: Promo Rotator, from Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955" );
            RockMigrationHelper.DeleteBlockType( "C0332D98-7CD0-43C2-9810-60F7DF86FBB6" ); // Purchased Products
            RockMigrationHelper.DeleteBlockType( "B8F1B648-8C5F-4529-8F8B-B564C2A19061" ); // Promo List Lava
            RockMigrationHelper.DeleteBlockType( "A494D4DD-0C96-4BA7-AF1B-43EFEF078261" ); // Package List Lava
            RockMigrationHelper.DeleteBlockType( "EA60C1AB-ADAB-4EDF-94F8-B0FE214B6F15" ); // Package Install
            RockMigrationHelper.DeleteBlockType( "9EC29D0F-7EE7-434B-A30F-6C36A81B0DEB" ); // Package Detail Lava
            RockMigrationHelper.DeleteBlockType( "69A7D88E-5CD8-4993-A88A-4DA15BAD3CB3" ); // Package Detail
            RockMigrationHelper.DeleteBlockType( "470C6EFF-091C-4593-848C-49547D0EBEEE" ); // Package Category List Lava
            RockMigrationHelper.DeleteBlockType( "41DFED6E-2ECD-4198-80C3-816B27241EB4" ); // Link Organization
            RockMigrationHelper.DeleteBlockType( "DBDB7C26-8271-4B1A-9791-71DEC99A7A2F" ); // Account Redirect
            RockMigrationHelper.DeletePage( "DADE879D-8DF5-4367-89EF-FEECD12B81AB" ); //  Page: Account, Layout: Right Sidebar, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6E029432-56F4-46AD-9D9C-C122F3D3C55C" ); //  Page: Link Organization, Layout: Right Sidebar, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6029A6D6-0059-4CC1-9751-1F012BC267F1" ); //  Page: Package Install, Layout: Right Sidebar, Site: Rock RMS
            RockMigrationHelper.DeletePage( "50D17FE7-88DB-46B2-9C58-DF8C0DE376A4" ); //  Page: Packages By Category, Layout: Right Sidebar, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6A163569-2826-4EF2-8208-879DDBDC0896" ); //  Page: Purchases, Layout: Right Sidebar, Site: Rock RMS
            RockMigrationHelper.DeletePage( "D6DC6AFE-70D9-43CF-9D76-EAEE2317FB14" ); //  Page: Package Detail, Layout: Right Sidebar, Site: Rock RMS
            RockMigrationHelper.DeletePage( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA" ); //  Page: Rock Shop, Layout: Right Sidebar, Site: Rock RMS
        }
    }
}
