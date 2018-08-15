using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace org.newpointe.Stars.Migrations
{
    [MigrationNumber( 2, "1.0.5" )]
    public class AddSystemData : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Star Values", "Star values for events", "048C200E-2B8E-4501-8C9B-A87C870C3C0D");
            RockMigrationHelper.AddDefinedValue("048C200E-2B8E-4501-8C9B-A87C870C3C0D", "Check-in", "", "DEE129DE-9516-43C2-AEBB-F510C603007E", false );
            RockMigrationHelper.AddDefinedValue("048C200E-2B8E-4501-8C9B-A87C870C3C0D", "First Time Friend", "", "EEBECE35-7C24-47CD-8340-7CB0BB33C528", false );
            RockMigrationHelper.AddDefinedValue("048C200E-2B8E-4501-8C9B-A87C870C3C0D", "Memory Verse", "", "DF75BB6F-FE37-4CB3-BD13-12B4F2D2D033", false );
            RockMigrationHelper.AddDefinedValue("048C200E-2B8E-4501-8C9B-A87C870C3C0D", "Special Event Attendance", "", "363504F2-F55F-4E0A-923C-6A25275FC34F", false );

            RockMigrationHelper.AddEntityAttribute("Rock.Model.DefinedValue", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "DefinedTypeId","76","Star Value","","Number of Stars this event is worth",1,"", "6A1A06FE-27AA-4D16-9F69-28FDEEAFA54A", "StarValue");
            RockMigrationHelper.AddDefinedValueAttributeValue("DEE129DE-9516-43C2-AEBB-F510C603007E", "6A1A06FE-27AA-4D16-9F69-28FDEEAFA54A", "5");
            RockMigrationHelper.AddDefinedValueAttributeValue("EEBECE35-7C24-47CD-8340-7CB0BB33C528", "6A1A06FE-27AA-4D16-9F69-28FDEEAFA54A", "5");
            RockMigrationHelper.AddDefinedValueAttributeValue("DF75BB6F-FE37-4CB3-BD13-12B4F2D2D033", "6A1A06FE-27AA-4D16-9F69-28FDEEAFA54A", "5");
            RockMigrationHelper.AddDefinedValueAttributeValue("363504F2-F55F-4E0A-923C-6A25275FC34F", "6A1A06FE-27AA-4D16-9F69-28FDEEAFA54A", "5");

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        //            Sql( @"
        //    UPDATE [_org_newpointe_SampleProject_ReferralAgency] SET [AgencyTypeValueId] = NULL
        //" );

            RockMigrationHelper.DeleteDefinedType("048C200E-2B8E-4501-8C9B-A87C870C3C0D");



        }
    }
}
