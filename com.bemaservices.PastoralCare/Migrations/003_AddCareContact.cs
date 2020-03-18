using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 3, "1.8.3" )]
    public class AddCareContact : Migration
    {
        public override void Up()
        {           
            Sql( @"

                CREATE TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [CareItemId] [int] NULL,
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
                 CONSTRAINT [PK__com_bemaservices_PastoralCare_CareContact] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]              

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_PastoralCare_CareContact_CareItem] FOREIGN KEY([CareItemId])
                REFERENCES [dbo].[_com_bemaservices_PastoralCare_CareItem] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact] CHECK CONSTRAINT [FK__com_bemaservices_PastoralCare_CareContact_CareItem]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_PastoralCare_CareContact_ContactorPersonAliasId] FOREIGN KEY([ContactorPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact] CHECK CONSTRAINT [FK__com_bemaservices_PastoralCare_CareContact_ContactorPersonAliasId] 

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CareContact_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact] CHECK CONSTRAINT [FK_dbo.CareContact_dbo.PersonAlias_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CareContact_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact] CHECK CONSTRAINT [FK_dbo.CareContact_dbo.PersonAlias_ModifiedByPersonAliasId]
" );
           
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.PastoralCare.Model.CareContact", "95719340-44E1-4A2D-BB40-8DAD1E67D83C", true, true );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "d1206d6e-ebc1-4845-8de7-e82c1875061b" );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareContact_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareContact_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareContact_ContactorPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareContact_CareItem]
                DROP TABLE [dbo].[_com_bemaservices_PastoralCare_CareContact]
                " );
        }
    }
}
