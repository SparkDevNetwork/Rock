//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class MarketingCampaignXsltAttribs : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Audience Primary Secondary
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Audience Primary Secondary", "AudiencePrimarySecondary", "", "Primary or Secondary Audience", 7, "1,2", "D1CD9710-AF30-4573-9EFB-5680B04E1161" );

            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Detail Page
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPageGuid", "", "", 1, "", "150D5897-46CB-4C83-8E74-A3A10AF209D2" );

            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Show Debug
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Debug", "ShowDebug", "", "Output XML", 8, "False", "CAA4217E-4A72-4A4C-8534-612B1B94A7E6" );

            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Image Attribute Keys
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Image Attribute Keys", "ImageAttributeKeys", "", "The types of images to display", 2, "", "AAB95771-D110-4A40-9B78-FF339D4CBEC7" );

            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Max Items
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Items", "MaxItems", "", "", 0, "", "59EDB12F-6003-4B80-8EBE-A0DDF99A5B98" );

            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:XSLT File
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "XSLT File", "XSLTFile", "", "The path to the XSLT File ", 3, "~/Assets/XSLT/AdList.xslt", "D4B3F1F1-0714-44EF-B569-A981A8DAEF97" );

            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Campuses
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "", "Display Ads for selected campus", 4, "", "44B07FDC-D014-483C-BE88-7397A8972CAA" );

            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Ad Types
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Ad Types", "AdTypes", "", "Types of Ads to display", 5, "", "B7A2AC62-86B0-47FE-A8EC-DD3A0CDB237C" );

            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Audience
            AddBlockTypeAttribute( "5A880084-7237-449A-9855-3FA02B6BD79F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience", "Audience", "", "The Audience", 6, "", "46AC60A0-EA20-419E-9BF7-C04CEA70E826" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Audience
            DeleteAttribute( "46AC60A0-EA20-419E-9BF7-C04CEA70E826" );
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Ad Types
            DeleteAttribute( "B7A2AC62-86B0-47FE-A8EC-DD3A0CDB237C" );
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Campuses
            DeleteAttribute( "44B07FDC-D014-483C-BE88-7397A8972CAA" );
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:XSLT File
            DeleteAttribute( "D4B3F1F1-0714-44EF-B569-A981A8DAEF97" );
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Max Items
            DeleteAttribute( "59EDB12F-6003-4B80-8EBE-A0DDF99A5B98" );
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Image Attribute Keys
            DeleteAttribute( "AAB95771-D110-4A40-9B78-FF339D4CBEC7" );
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Show Debug
            DeleteAttribute( "CAA4217E-4A72-4A4C-8534-612B1B94A7E6" );
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Detail Page
            DeleteAttribute( "150D5897-46CB-4C83-8E74-A3A10AF209D2" );
            // Attrib for BlockType: Cms - Marketing Campaign Ads Xslt:Audience Primary Secondary
            DeleteAttribute( "D1CD9710-AF30-4573-9EFB-5680B04E1161" );
        }
    }
}
