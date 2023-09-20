using Rock.Plugin;

namespace org.lakepointe.Checkin.Migrations
{
    [MigrationNumber( 2, "1.8.1.0" )]
    public class AddMetadataTable : Migration
    {
        public override void Up()
        {
            Sql( @"

                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '_org_lakepointe_Checkin_AttendanceMetadata')
                BEGIN

                    CREATE TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata](
	                    [Id] [int] IDENTITY(1,1) NOT NULL,
	                    [AttendanceId] [int] NOT NULL,
	                    [CheckedOutByPersonAliasId] [int] NULL,
	                    [Guid] [uniqueidentifier] NOT NULL,
	                    [CreatedDateTime] [datetime] NULL,
	                    [ModifiedDateTime] [datetime] NULL,
	                    [CreatedByPersonAliasId] [int] NULL,
	                    [ModifiedbyPersonAliasId] [int] NULL,
	                    [ForeignId] [int] NULL,
	                    [ForeignGuid] [uniqueidentifier] NULL,
	                    [ForeignKey] [nvarchar](100) NULL,
                     CONSTRAINT [PK__org_lakepointe_Checkin_AttendanceMetadata] PRIMARY KEY CLUSTERED 
                    (
	                    [Id] ASC
                    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                    ) ON [PRIMARY]


                    ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata]  WITH CHECK ADD  CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_Attendance] FOREIGN KEY([AttendanceId])
                    REFERENCES [dbo].[Attendance] ([Id])
                    ON DELETE CASCADE


                    ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata] CHECK CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_Attendance]


                    ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata]  WITH CHECK ADD  CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_CheckedOutByPersonAlias] FOREIGN KEY([CheckedOutByPersonAliasId])
                    REFERENCES [dbo].[PersonAlias] ([Id])


                    ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata] CHECK CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_CheckedOutByPersonAlias]


                    ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata]  WITH CHECK ADD  CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_CreatedByPersonAlias] FOREIGN KEY([CreatedByPersonAliasId])
                    REFERENCES [dbo].[PersonAlias] ([Id])


                    ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata] CHECK CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_CreatedByPersonAlias]


                    ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata]  WITH CHECK ADD  CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_ModifiedByPersonAlias] FOREIGN KEY([ModifiedbyPersonAliasId])
                    REFERENCES [dbo].[PersonAlias] ([Id])


                    ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata] CHECK CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_ModifiedByPersonAlias]
                END
            " );
        }

        public override void Down()
        {
            Sql( @"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '_org_lakepointe_Checkin_AttendanceMetadata'
                    BEGIN
                        ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata] DROP CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_Attendance]
                        ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata] DROP CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_CheckedOutByPersonAlias]
                        ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata] DROP CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_CreatedByPersonAlias]
                        ALTER TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata] DROP CONSTRAINT [FK__org_lakepointe_Checkin_AttendanceMetadata_ModifiedByPersonAlias]
                        DROP TABLE [dbo].[_org_lakepointe_Checkin_AttendanceMetadata]
                    END
            " );
        }


    }
}
