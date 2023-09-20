using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;

namespace com.kfs.EventRegistration.Advanced.Migrations
{
    [MigrationNumber( 3, "1.8.5" )]
    public class UpdateAttributeQualifiers : Migration
    {
        /// <summary>
        /// Ups this instance.
        /// </summary>
        public override void Up()
        {
            // Allow Multiple Registrations
            RockMigrationHelper.UpdateAttributeQualifier( "4F9CA590-882A-4A2A-9262-C9350953C996", "falsetext", "No", "96C69374-8C7A-4627-8E63-4431ACBF1D6C" );
            RockMigrationHelper.UpdateAttributeQualifier( "4F9CA590-882A-4A2A-9262-C9350953C996", "truetext", "Yes", "7364FEC0-8D86-4D6E-9F81-1F65200E6C77" );

            // Allow Volunteer Assignment
            RockMigrationHelper.UpdateAttributeQualifier( "7129D352-5468-4BD9-BF2E-5CF9758D83BF", "falsetext", "No", "E90E2D64-134B-48EB-84BC-ED23EFE742F2" );
            RockMigrationHelper.UpdateAttributeQualifier( "7129D352-5468-4BD9-BF2E-5CF9758D83BF", "truetext", "Yes", "D3B5890F-7C21-489A-80BD-D10542E87D01" );

            // Show On Grid
            RockMigrationHelper.UpdateAttributeQualifier( "60BD7029-9D83-42CC-B904-9A1F3A89C1E6", "falsetext", "No", "09D5A05F-2A64-49B3-942A-76A60E295359" );
            RockMigrationHelper.UpdateAttributeQualifier( "60BD7029-9D83-42CC-B904-9A1F3A89C1E6", "truetext", "Yes", "3A403256-9A71-40C5-B973-CE8D952DF2F9" );

            // Display Combined Memberships
            RockMigrationHelper.UpdateAttributeQualifier( "7DD366B4-0A8C-4DA0-B14E-A17A1AFF55A6", "falsetext", "No", "0D234359-1394-4C39-8B4D-15B82FDB2E5A" );
            RockMigrationHelper.UpdateAttributeQualifier( "7DD366B4-0A8C-4DA0-B14E-A17A1AFF55A6", "truetext", "Yes", "DA7378A6-3EA2-4C98-9FCE-D084C07CBD9B" );

            // Display Separate Roles
            RockMigrationHelper.UpdateAttributeQualifier( "469BA2BC-FEB5-4C95-9BA2-B382F01C88E3", "falsetext", "No", "5E80314B-A94C-431B-AD22-E0B55618385F" );
            RockMigrationHelper.UpdateAttributeQualifier( "469BA2BC-FEB5-4C95-9BA2-B382F01C88E3", "truetext", "Yes", "53502346-4F7B-4C3A-9BF0-2F61AD519706" );
        }

        /// <summary>
        /// Downs this instance.
        /// </summary>
        public override void Down()
        {
        }
    }
}
