using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.SpiritualGifts.Migrations
{
    [MigrationNumber( 6, "1.4.5" )]
    public class WizardStyle : Migration
    {
        public static readonly string SPIRITUAL_GIFT_BADGE = "88F61BA1-35AA-4080-AC56-CF4B1145C51F";

        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddBlockTypeAttribute( "AA78F8DC-B8AB-4300-B6FA-A8A227E422D9", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Questions per Page", "NumberOfQuestions", "", "The number of questions displayed on each page of the test", 0, @"6", "5E980731-080B-4CB4-9BCC-C165C1745279" );
            RockMigrationHelper.AddBlockTypeAttribute( "AA78F8DC-B8AB-4300-B6FA-A8A227E422D9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Spiritual Gift Result Detail", "SpiritualGiftResultDetail", "", "Page to show the details of the spiritual assessment results. If blank no link is created.", 0, @"", "61F072FF-068B-492C-B973-7C7BE263D4EF" );
           
            RockMigrationHelper.AddBlockAttributeValue( "B40E3EDC-120B-4339-9D77-01B4671699E1", "5E980731-080B-4CB4-9BCC-C165C1745279", @"6" ); // Number of Questions per Page
            RockMigrationHelper.AddBlockAttributeValue( "B40E3EDC-120B-4339-9D77-01B4671699E1", "61F072FF-068B-492C-B973-7C7BE263D4EF", @"24318613-fa5a-45de-be08-22351e0a979b" ); // Spiritual Gift Result Detail

            // Page: Spiritual Gifts Results
            RockMigrationHelper.AddPage( "C15A6A27-A5C0-49BE-BDA2-10EF7C786564", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Spiritual Gifts Results", "", "24318613-FA5A-45DE-BE08-22351E0A979B", "" ); // Site:External Website
            RockMigrationHelper.AddBlock( "24318613-FA5A-45DE-BE08-22351E0A979B", "", "76EF7A11-FBF5-4023-B202-1F50ECCAF5E8", "Spiritual Gifts Result", "Main", "", "", 0, "15E44424-EB23-4DB7-A203-189A57B60535" );

            RockMigrationHelper.AddBlockTypeAttribute( "76EF7A11-FBF5-4023-B202-1F50ECCAF5E8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Retake Test Button", "ShowRetakeTestButton", "", "Whether to display the retake test button", 0, @"False", "84A2D9B9-DBE8-4CD2-933D-56210BC5E4B8" );
            RockMigrationHelper.AddBlockTypeAttribute( "76EF7A11-FBF5-4023-B202-1F50ECCAF5E8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Spiritual Gift Test Page", "SpiritualGiftTestPage", "", "Page to take the spiritual gifts test. If blank no link is created.", 0, @"", "38972145-D243-4550-8268-850A61EACF31" );

            RockMigrationHelper.AddBlockAttributeValue( "15E44424-EB23-4DB7-A203-189A57B60535", "84A2D9B9-DBE8-4CD2-933D-56210BC5E4B8", @"True" ); // Show Retake Test Button

            RockMigrationHelper.AddBlockAttributeValue( "15E44424-EB23-4DB7-A203-189A57B60535", "38972145-D243-4550-8268-850A61EACF31", @"c15a6a27-a5c0-49be-bda2-10ef7c786564" ); // Spiritual Gift Test

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "38972145-D243-4550-8268-850A61EACF31" );
            RockMigrationHelper.DeleteAttribute( "84A2D9B9-DBE8-4CD2-933D-56210BC5E4B8" );
            RockMigrationHelper.DeleteBlock( "15E44424-EB23-4DB7-A203-189A57B60535" );
            RockMigrationHelper.DeletePage( "24318613-FA5A-45DE-BE08-22351E0A979B" ); //  Page: Spiritual Gifts Results

            RockMigrationHelper.DeleteAttribute( "61F072FF-068B-492C-B973-7C7BE263D4EF" );
            RockMigrationHelper.DeleteAttribute( "5E980731-080B-4CB4-9BCC-C165C1745279" );           
        }
    }
}
