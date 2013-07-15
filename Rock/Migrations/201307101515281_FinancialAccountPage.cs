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
    public partial class FinancialAccountPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "142627AE-6590-48E3-BFCA-3669260B8CF2", "Accounts", "", "Default", "2B630A3B-E081-4204-A3E4-17BB3A5F063D", "" );
            AddBlockType( "Finance - Accounts", "", "~/Blocks/Finance/Accounts.ascx", "787781E2-8BFB-481F-B502-A31BB8715CA9" );
            AddBlock( "2B630A3B-E081-4204-A3E4-17BB3A5F063D", "787781E2-8BFB-481F-B502-A31BB8715CA9", "Accounts", "", "Content", 0, "77CCF0E4-B634-49C7-A3E0-A9C439933635" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Show State Name
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show State Name", "ShowStateName", "", "Should the address state show the full name (Arizona) or the abbreviation (AZ)?", 6, "True", "E9985270-7E55-456D-84B4-936B304369CA" );

            // Attrib for BlockType: Finance - Giving Profile Detail:Default State
            AddBlockTypeAttribute( "B343E2B7-0AD0-49B8-B78E-E47BD42171A7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default State", "DefaultState", "", "Which state should be selected by default?", 8, "", "F7D2B49F-B7D1-4F4E-B924-DC219DBBE0AB" );

            // Attrib Value for Contributions:Show State Name
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "E9985270-7E55-456D-84B4-936B304369CA", "True" );

            // Attrib Value for Contributions:Default State
            AddBlockAttributeValue( "20C12A0F-BEC1-4620-9273-EEFE4CFB1D96", "F7D2B49F-B7D1-4F4E-B924-DC219DBBE0AB", "" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Finance - Giving Profile Detail:Default State
            DeleteAttribute( "F7D2B49F-B7D1-4F4E-B924-DC219DBBE0AB" );
            // Attrib for BlockType: Finance - Giving Profile Detail:Show State Name
            DeleteAttribute( "E9985270-7E55-456D-84B4-936B304369CA" );
            DeleteBlock( "77CCF0E4-B634-49C7-A3E0-A9C439933635" ); // Accounts
            DeleteBlockType( "787781E2-8BFB-481F-B502-A31BB8715CA9" ); // Finance - Accounts
            DeletePage( "2B630A3B-E081-4204-A3E4-17BB3A5F063D" ); // Accounts
        }
    }
}
