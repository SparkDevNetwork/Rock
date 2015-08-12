using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.SpiritualGifts.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class PagesAndBlocks : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Spiritual Gifts Test
            RockMigrationHelper.AddPage( "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Spiritual Gifts Test", "", "C15A6A27-A5C0-49BE-BDA2-10EF7C786564", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "C15A6A27-A5C0-49BE-BDA2-10EF7C786564", "SpiritualGifts/{rckipid}" );
            RockMigrationHelper.UpdateBlockType( "Spiritual Gifts Test", "Allows you to take a spiritual gift test and saves your spiritual gift score.", "~/Plugins/com_centralaz/SpiritualGifts/SpiritualGiftTest.ascx", "com_centralaz > Spiritual Gifts", "AA78F8DC-B8AB-4300-B6FA-A8A227E422D9" );
            RockMigrationHelper.AddBlock( "C15A6A27-A5C0-49BE-BDA2-10EF7C786564", "", "AA78F8DC-B8AB-4300-B6FA-A8A227E422D9", "Spiritual Gifts Test", "Main", "", "", 0, "B40E3EDC-120B-4339-9D77-01B4671699E1" );

            RockMigrationHelper.AddBlockTypeAttribute( "AA78F8DC-B8AB-4300-B6FA-A8A227E422D9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Instructions", "Instructions", "", "The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 0, @"
            <h2>Welcome!</h2>
            <p>
                {{ Person.NickName }}, in this assessment you are given a series of questions, each one being a statement about you. 
                Select the frequency that that phrase is true of you.
            </p>
            <p>
                One final thought as you give your responses. On these kinds of assessments, it
                is often best and easiest if you respond quickly and do not deliberate too long
                on each question. Your response on one question will not unduly influence your scores,
                so simply answer as quickly as possible and enjoy the process. Don't get too hung
                up, just go with your instinct.
            </p>
            <p>
                When you are ready, click the 'Start' button to proceed.
            </p>
", "DF442336-158D-4E35-8703-F1670B2DD0CC" );

            RockMigrationHelper.AddBlockTypeAttribute( "AA78F8DC-B8AB-4300-B6FA-A8A227E422D9", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Min Days To Retake", "MinDaysToRetake", "", "The number of days that must pass before the test can be taken again.", 0, @"30", "26770BE6-350C-406F-AD3D-104275D73BD0" );

            // Page: Spiritual Gifts Result
            RockMigrationHelper.AddPage( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Spiritual Gifts Result", "", "A2EA46FF-6B39-4A0E-A1A4-AAEC6ADFC068", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Spiritual Gifts Result", "Allows you to view your spiritual gift score.", "~/Plugins/com_centralaz/SpiritualGifts/SpiritualGiftResult.ascx", "com_centralaz > Spiritual Gifts", "76EF7A11-FBF5-4023-B202-1F50ECCAF5E8" );
            RockMigrationHelper.AddBlock( "A2EA46FF-6B39-4A0E-A1A4-AAEC6ADFC068", "", "76EF7A11-FBF5-4023-B202-1F50ECCAF5E8", "Spiritual Gifts Result", "Main", "", "", 0, "A0810F3E-FD70-4814-8053-A5CC336742F6" ); 

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "A0810F3E-FD70-4814-8053-A5CC336742F6" );
            RockMigrationHelper.DeleteBlockType( "76EF7A11-FBF5-4023-B202-1F50ECCAF5E8" );
            RockMigrationHelper.DeletePage( "A2EA46FF-6B39-4A0E-A1A4-AAEC6ADFC068" ); //  Page: Spiritual Gifts Result

            RockMigrationHelper.DeleteAttribute( "26770BE6-350C-406F-AD3D-104275D73BD0" );
            RockMigrationHelper.DeleteAttribute( "DF442336-158D-4E35-8703-F1670B2DD0CC" );
            RockMigrationHelper.DeleteBlock( "B40E3EDC-120B-4339-9D77-01B4671699E1" );
            RockMigrationHelper.DeleteBlockType( "AA78F8DC-B8AB-4300-B6FA-A8A227E422D9" );
            RockMigrationHelper.DeletePage( "C15A6A27-A5C0-49BE-BDA2-10EF7C786564" ); //  Page: Spiritual Gifts Test
        }
    }
}
