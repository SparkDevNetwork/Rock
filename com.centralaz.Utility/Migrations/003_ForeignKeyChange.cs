using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;

namespace com.centralaz.Utility.Migrations
{
    [MigrationNumber( 3, "1.3.4" )]
    public class ForeignKeyChange : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {

            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_com_centralaz_Accountability_Question].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_com_centralaz_Accountability_Question] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_com_centralaz_Accountability_Question] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_com_centralaz_Accountability_Question] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_com_centralaz_Accountability_Question] (ForeignGuid)
" );


            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_com_centralaz_Accountability_Response].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_com_centralaz_Accountability_Response] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_com_centralaz_Accountability_Response] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_com_centralaz_Accountability_Response] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_com_centralaz_Accountability_Response] (ForeignGuid)
" );


            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_com_centralaz_Accountability_ResponseSet].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_com_centralaz_Accountability_ResponseSet] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_com_centralaz_Accountability_ResponseSet] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_com_centralaz_Accountability_ResponseSet] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_com_centralaz_Accountability_ResponseSet] (ForeignGuid)
" );


            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_com_centralaz_Baptism_Baptizee].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_com_centralaz_Baptism_Baptizee] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_com_centralaz_Baptism_Baptizee] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_com_centralaz_Baptism_Baptizee] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_com_centralaz_Baptism_Baptizee] (ForeignGuid)
" );

            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_com_centralaz_DpsMatch_Match].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_com_centralaz_DpsMatch_Match] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_com_centralaz_DpsMatch_Match] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_com_centralaz_DpsMatch_Match] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_com_centralaz_DpsMatch_Match] (ForeignGuid)
" );


            Sql( @"
    EXEC sp_rename  
        @objname = N'[dbo].[_com_centralaz_DpsMatch_Offender].ForeignId',
        @newname = 'ForeignKey',
        @objtype = 'COLUMN'
" );
            Sql( @"
    ALTER TABLE [dbo].[_com_centralaz_DpsMatch_Offender] ADD
        ForeignGuid uniqueidentifier null,
        ForeignId int null
" );
            Sql( @"
    CREATE INDEX [IX_ForeignKey] ON [dbo].[_com_centralaz_DpsMatch_Offender] (ForeignKey)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignId] ON [dbo].[_com_centralaz_DpsMatch_Offender] (ForeignId)
" );
            Sql( @"
    CREATE INDEX [IX_ForeignGuid] ON [dbo].[_com_centralaz_DpsMatch_Offender] (ForeignGuid)
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
