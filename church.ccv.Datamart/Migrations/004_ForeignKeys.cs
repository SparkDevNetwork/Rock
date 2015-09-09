using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace church.ccv.Datamart.Migrations
{
    [MigrationNumber( 4, "1.3.4" )]
    class ForeignKeys : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_ERA]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Datamart_ERA].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Datamart_ERA] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Datamart_ERA] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_ERA] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Datamart_ERA] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_EraLoss]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Datamart_EraLoss].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Datamart_EraLoss] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Datamart_EraLoss] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_EraLoss] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Datamart_EraLoss] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_Family]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Datamart_Family].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Datamart_Family] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Datamart_Family] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_Family] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Datamart_Family] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_NearestGroup]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Datamart_NearestGroup].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Datamart_NearestGroup] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Datamart_NearestGroup] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_NearestGroup] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Datamart_NearestGroup] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_Neighborhood]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Datamart_Neighborhood].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Datamart_Neighborhood] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Datamart_Neighborhood] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_Neighborhood] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Datamart_Neighborhood] (ForeignGuid)
" );

            Sql( @"
    DROP INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_Person]
" );
            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_church_ccv_Datamart_Person].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_church_ccv_Datamart_Person] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_church_ccv_Datamart_Person] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_church_ccv_Datamart_Person] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_church_ccv_Datamart_Person] (ForeignGuid)
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
