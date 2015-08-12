using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.DpsMatch.Migrations
{
    [MigrationNumber( 3, "1.0.14" )]
    public class OffenderPageAndTag : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Offender Match
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Offender Match", "", "24FC8D5C-7AC9-4B90-A05A-5765AAAE5336", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "24FC8D5C-7AC9-4B90-A05A-5765AAAE5336", "OffenderMatch" );
            RockMigrationHelper.UpdateBlockType( "Offender Evaluation Block", "Block to manually evaluate Person entries similar to known sexual offenders", "~/Plugins/com_centralaz/DpsMatch/DPSEvaluationBlock.ascx", "com_centralaz > Offender Match", "DE2ACACA-7839-47C9-AB79-C02E2CF5ECB5" );
            RockMigrationHelper.AddBlock( "24FC8D5C-7AC9-4B90-A05A-5765AAAE5336", "", "DE2ACACA-7839-47C9-AB79-C02E2CF5ECB5", "Offender Evaluation Block", "Main", "", "", 0, "70B5404B-30E0-48B1-9D0E-494B0F2A9881" );
            Sql( @"
                INSERT INTO [dbo].[Tag]
                           ([IsSystem]
                           ,[EntityTypeId]
                           ,[EntityTypeQualifierColumn]
                           ,[EntityTypeQualifierValue]
                           ,[Name]
                           ,[Order]
                           ,[Guid]
                           ,[CreatedDateTime]
                           ,[ModifiedDateTime]
                           ,[CreatedByPersonAliasId]
                           ,[ModifiedByPersonAliasId]
                           ,[Description]
                           ,[ForeignId]
                           ,[OwnerPersonAliasId])
                     VALUES
                           (0
                           ,15
                           ,N''
                           ,N''
                           ,N'Offender'
                           ,0
                           ,N'A585EC28-64D7-463F-98E9-B0D957D0DBBC'
                           ,GetDate()
                           ,GetDate()
                           ,1
                           ,1
                           ,N'This person is a sexual offender'
                           ,NULL
                           ,NULL)
" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "70B5404B-30E0-48B1-9D0E-494B0F2A9881" );
            RockMigrationHelper.DeleteBlockType( "DE2ACACA-7839-47C9-AB79-C02E2CF5ECB5" );
            RockMigrationHelper.DeletePage( "24FC8D5C-7AC9-4B90-A05A-5765AAAE5336" ); //  Page: Offender Match
        }
    }
}
