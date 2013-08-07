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
    public partial class MarketingCampaignAdListFilter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Administration - Marketing Campaign Ad List:Show Marketing Campaign Title
            AddBlockTypeAttribute( "0A690902-A0A1-4AB1-AFEC-001BA5FD124B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Marketing Campaign Title", "ShowMarketingCampaignTitle", "", "", 0, "False", "F78D9781-DD8F-4AF7-A3E3-9ADC289C829B" );

            // Attrib Value for Block:Marketing Campaign Ad List, Attribute:Show Marketing Campaign Title, Page:Ad Campaign Detail
            AddBlockAttributeValue( "E12B0B16-7D2F-4E54-851B-5D9EB7C7D1A3", "F78D9781-DD8F-4AF7-A3E3-9ADC289C829B", "False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Administration - Marketing Campaign Ad List:Show Marketing Campaign Title
            DeleteAttribute( "F78D9781-DD8F-4AF7-A3E3-9ADC289C829B" );
        }
    }
}
