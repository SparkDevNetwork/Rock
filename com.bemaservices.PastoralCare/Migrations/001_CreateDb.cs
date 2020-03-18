using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 1, "1.8.3" )]
    public class CreateDb : Migration
    {
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_PastoralCare_CareType](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [IsActive] [bit] NOT NULL,
                    [Name] [nvarchar](100) NULL,
	                [Description] [nvarchar](max) NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_PastoralCare_CareType] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_PastoralCare_CareType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareType] CHECK CONSTRAINT [FK__com_bemaservices_PastoralCare_CareType_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_PastoralCare_CareType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareType] CHECK CONSTRAINT [FK__com_bemaservices_PastoralCare_CareType_ModifiedByPersonAliasId]
" );
           
            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [CareTypeId] [int] NULL,
                    [PersonAliasId] [int] NULL,
	                [ContactorPersonAliasId] [int] NULL,
	                [ContactDateTime] [datetime] NULL,
	                [Description] [nvarchar](max) NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_PastoralCare_CareItem] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]              

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_CareType] FOREIGN KEY([CareTypeId])
                REFERENCES [dbo].[_com_bemaservices_PastoralCare_CareType] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] CHECK CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_CareType]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_PersonAliasId] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] CHECK CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_PersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_ContactorPersonAliasId] FOREIGN KEY([ContactorPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] CHECK CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_ContactorPersonAliasId] 

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CareItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] CHECK CONSTRAINT [FK_dbo.CareItem_dbo.PersonAlias_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CareItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] CHECK CONSTRAINT [FK_dbo.CareItem_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
           
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.PastoralCare.Model.CareItem", "d1206d6e-ebc1-4845-8de7-e82c1875061b", true, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.PastoralCare.Model.CareType", "ce1dfe04-9bc1-423b-a63c-983ac28140b5", true, true );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "d1206d6e-ebc1-4845-8de7-e82c1875061b" );
            RockMigrationHelper.DeleteEntityType( "ce1dfe04-9bc1-423b-a63c-983ac28140b5" );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_ContactorPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_PersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_CareType]
                DROP TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareType] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareType_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareType] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareType_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_bemaservices_PastoralCare_CareType]
                " );
        }
    }
}
