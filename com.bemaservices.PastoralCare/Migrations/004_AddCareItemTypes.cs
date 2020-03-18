using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 4, "1.8.3" )]
    public class AddCareTypeItem : Migration
    {
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] DROP CONSTRAINT [FK__com_bemaservices_PastoralCare_CareItem_CareType]
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareItem] DROP COLUMN [CareTypeId]

                CREATE TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [CareTypeId] [int] NOT NULL,
	                [CareItemId] [int] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [ForeignKey] [nvarchar](100) NULL,
	                [ForeignGuid] [uniqueidentifier] NULL,
	                [ForeignId] [int] NULL,
                 CONSTRAINT [PK_dbo._com_bemaservices_PastoralCare_CareTypeItem] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_bemaservices_PastoralCare_CareTypeItem_dbo._com_bemaservices_PastoralCare_CareType_CareTypeId] FOREIGN KEY([CareTypeId])
                REFERENCES [dbo].[_com_bemaservices_PastoralCare_CareType] ([Id])
                ON DELETE CASCADE

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem] CHECK CONSTRAINT [FK_dbo._com_bemaservices_PastoralCare_CareTypeItem_dbo._com_bemaservices_PastoralCare_CareType_CareTypeId]      

                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_bemaservices_PastoralCare_CareTypeItem_dbo._com_bemaservices_PastoralCare_CareItem_CareItemId] FOREIGN KEY([CareItemId])
                REFERENCES [dbo].[_com_bemaservices_PastoralCare_CareItem] ([Id])
                ON DELETE CASCADE    
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem] CHECK CONSTRAINT [FK_dbo._com_bemaservices_PastoralCare_CareTypeItem_dbo._com_bemaservices_PastoralCare_CareItem_CareItemId]
             
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_bemaservices_PastoralCare_CareTypeItem_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
               
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem] CHECK CONSTRAINT [FK_dbo._com_bemaservices_PastoralCare_CareTypeItem_dbo.PersonAlias_CreatedByPersonAliasId]
              
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem]  WITH CHECK ADD  CONSTRAINT [FK_dbo._com_bemaservices_PastoralCare_CareTypeItem_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
          
                ALTER TABLE [dbo].[_com_bemaservices_PastoralCare_CareTypeItem] CHECK CONSTRAINT [FK_dbo._com_bemaservices_PastoralCare_CareTypeItem_dbo.PersonAlias_ModifiedByPersonAliasId]           
" );

            RockMigrationHelper.UpdateEntityType( "com.bemaservices.PastoralCare.Model.CareTypeItem", "72352815-30f3-46fb-86c0-69ac284d9ed2", true, true );

            // Page: Universal Care Item Attributes
            RockMigrationHelper.AddPage( "FC1531F6-5A3C-4F05-8E92-B2B66688B492", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Universal Care Item Attributes", "", "B70D4B19-74CF-4C21-BE7D-EA0BD33ECF65", "" ); // Site:Rock RMS
            // Add Block to Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B70D4B19-74CF-4C21-BE7D-EA0BD33ECF65", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Attributes", "Main", "", "", 0, "437C2FD9-AC09-4F37-B4E5-364B48E26F5C" );
            // Attrib Value for Block:Attributes, Attribute:Entity Qualifier Column Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106", @"" );
            // Attrib Value for Block:Attributes, Attribute:Entity Qualifier Value Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "FCE1E87D-F816-4AD5-AE60-1E71942C547C", @"" );
            // Attrib Value for Block:Attributes, Attribute:Entity Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "5B33FE25-6BF0-4890-91C6-49FB1629221E", @"d1206d6e-ebc1-4845-8de7-e82c1875061b" );
            // Attrib Value for Block:Attributes, Attribute:Entity Id Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "CBB56D68-3727-42B9-BF13-0B2B593FB328", @"0" );
            // Attrib Value for Block:Attributes, Attribute:Allow Setting of Values Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "018C0016-C253-44E4-84DB-D166084C5CAD", @"False" );
            // Attrib Value for Block:Attributes, Attribute:Enable Show In Grid Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "920FE120-AD75-4D5C-BFE0-FA5745B1118B", @"True" );
            // Attrib Value for Block:Attributes, Attribute:Category Filter Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "0C2BCD33-05CC-4B03-9F57-C686B8911E64", @"" );
            // Attrib Value for Block:Attributes, Attribute:core.CustomGridColumnsConfig Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "11F74455-F71D-45C7-806B-0DB463D34DAB", @"" );
            // Attrib Value for Block:Attributes, Attribute:core.CustomGridEnableStickyHeaders Page: Universal Care Item Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "437C2FD9-AC09-4F37-B4E5-364B48E26F5C", "4C37F56C-7705-4BA3-A21F-5FEA00A12424", @"False" );

            // Attrib for BlockType: Care Type List:Allow Shared Attributes
            RockMigrationHelper.UpdateBlockTypeAttribute( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Shared Attributes", "AllowSharedAttributes", "", "Displays a link to a page for care item attributes shared across care types", 0, @"False", "1F64B3AB-273A-4D40-8A60-A7B462E96030" );
            // Attrib for BlockType: Care Type List:Shared Attribute Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Shared Attribute Page", "SharedAttributePage", "", "Page used to view shared care item attributes.", 0, @"", "6C4E36D2-C398-4638-B79B-D23DDD66DB46" );
            // Attrib Value for Block:Care Type List, Attribute:Allow Shared Attributes Page: Care Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7FD75626-52D5-4206-A1ED-265233EB19EE", "1F64B3AB-273A-4D40-8A60-A7B462E96030", @"True" );
            // Attrib Value for Block:Care Type List, Attribute:Shared Attribute Page Page: Care Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7FD75626-52D5-4206-A1ED-265233EB19EE", "6C4E36D2-C398-4638-B79B-D23DDD66DB46", @"b70d4b19-74cf-4c21-be7d-ea0bd33ecf65" );
        }
        public override void Down()
        {
        }
    }
}
