using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.CommandCenter.Migrations
{
    [MigrationNumber( 6, "1.3.4" )]
    class ForeignKeys : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_CommandCenter_Recording]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_CommandCenter_Recording].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_CommandCenter_Recording] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_CommandCenter_Recording] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_CommandCenter_Recording] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_CommandCenter_Recording] (ForeignGuid)
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
