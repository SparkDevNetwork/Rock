using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Hr.Migrations
{
    [MigrationNumber( 7, "1.3.4" )]
    class ForeignKeys : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Hr_TimeCard]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Hr_TimeCard].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Hr_TimeCard] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Hr_TimeCard] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Hr_TimeCard] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Hr_TimeCard] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Hr_TimeCardDay]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Hr_TimeCardDay].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Hr_TimeCardDay] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Hr_TimeCardDay] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Hr_TimeCardDay] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Hr_TimeCardDay] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Hr_TimeCardHistory]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Hr_TimeCardHistory].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Hr_TimeCardHistory] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Hr_TimeCardHistory] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Hr_TimeCardHistory] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Hr_TimeCardHistory] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Hr_TimeCardPayPeriod]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Hr_TimeCardPayPeriod].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Hr_TimeCardPayPeriod] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Hr_TimeCardPayPeriod] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Hr_TimeCardPayPeriod] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Hr_TimeCardPayPeriod] (ForeignGuid)
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
