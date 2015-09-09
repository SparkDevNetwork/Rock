using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.SampleProject.Migrations
{
    [MigrationNumber( 5, "1.3.4" )]
    class ForeignKeys : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_SampleProject_ReferralAgency]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_SampleProject_ReferralAgency].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_SampleProject_ReferralAgency] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_SampleProject_ReferralAgency] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_SampleProject_ReferralAgency] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_SampleProject_ReferralAgency] (ForeignGuid)
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
