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
    public partial class ScheduledJobs : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlockType( "Scheduled Jobs", "Manage automated jobs", "~/Blocks/Administration/ScheduledJobs.ascx", "ED2063B5-9839-46D1-8419-FE36D3B54708" );
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Jobs Administration", "Administer the automated jobs that run in the background.", "C58ADA1A-6322-4998-8FED-C3565DE87EFA" );
            AddBlock( "C58ADA1A-6322-4998-8FED-C3565DE87EFA", "ED2063B5-9839-46D1-8419-FE36D3B54708", "Jobs", "Content", "01727331-4F96-43CD-8585-791B35D86487" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "01727331-4F96-43CD-8585-791B35D86487" );
            DeletePage( "C58ADA1A-6322-4998-8FED-C3565DE87EFA" );
            DeleteBlockType( "ED2063B5-9839-46D1-8419-FE36D3B54708" );
        }
    }
}
