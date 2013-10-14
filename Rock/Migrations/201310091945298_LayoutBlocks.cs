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
    public partial class LayoutBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "DefaultPanel", "Default Panel", "", "6C128968-279F-4578-BB08-1913203C484B" );

            // Because DefaultPanel Layout was added in previous commit, update any pages using it to the new system layout.
            Sql( @" 
    DECLARE @NewLayoutId int
    SET @NewLayoutId = (SELECT [Id] FROM [Layout] WHERE [Guid] = '6C128968-279F-4578-BB08-1913203C484B')

    DECLARE @OldLayoutId int
    SET @OldLayoutId = (SELECT [Id] FROM [Layout] WHERE [IsSystem] = 0 AND [FileName] = 'DefaultPanel')
    
    UPDATE [Page] SET [LayoutId] = @NewLayoutId WHERE [LayoutId] = @OldLayoutId

    DELETE [Layout] WHERE [Id] = @OldLayoutId
" );

            AddPage( "A2991117-0B85-4209-9008-254929C6E00F", "2BA19878-F9B8-4ABF-91E1-75A7CF92BD8B", "Layout Detail", "", "E6217A2B-B16F-4E84-BF67-795CA7F5F9AA", "" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'E6217A2B-B16F-4E84-BF67-795CA7F5F9AA'" );

            AddBlockType( "Cms - Layout List", "", "~/Blocks/Cms/LayoutList.ascx", "5996BF81-F2E2-4702-B401-B0B1B6667DAE" );
            AddBlockType( "Cms - Layout Detail", "", "~/Blocks/Cms/LayoutDetail.ascx", "68B9D63D-D714-473A-89F2-62EB1602E00A" );

            // Add Block to Page: Site Detail
            AddBlock( "A2991117-0B85-4209-9008-254929C6E00F", "", "5996BF81-F2E2-4702-B401-B0B1B6667DAE", "Layout List", "Content", 1, "937BC322-633D-43BF-89C6-54970CB67D52" );
            // Add Block to Page: Layout Detail
            AddBlock( "E6217A2B-B16F-4E84-BF67-795CA7F5F9AA", "", "68B9D63D-D714-473A-89F2-62EB1602E00A", "Layout Detail", "Content", 0, "C04C6905-C156-49D3-832D-D09F3B0E1BF1" );

            // Attrib for BlockType: Cms - Layout List:Detail Page
            AddBlockTypeAttribute( "5996BF81-F2E2-4702-B401-B0B1B6667DAE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "025784DF-F3BE-47AD-A4F1-7189249E4D24" );

            // Attrib Value for Block:Layout List, Attribute:Detail Page Page: Site Detail
            AddBlockAttributeValue( "937BC322-633D-43BF-89C6-54970CB67D52", "025784DF-F3BE-47AD-A4F1-7189249E4D24", "e6217a2b-b16f-4e84-bf67-795ca7f5f9aa" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Cms - Layout List:Detail Page
            DeleteAttribute( "025784DF-F3BE-47AD-A4F1-7189249E4D24" );

            // Remove Block: Layout Detail, from Page: Layout Detail
            DeleteBlock( "C04C6905-C156-49D3-832D-D09F3B0E1BF1" );
            // Remove Block: Layout List, from Page: Site Detail
            DeleteBlock( "937BC322-633D-43BF-89C6-54970CB67D52" );

            DeleteBlockType( "68B9D63D-D714-473A-89F2-62EB1602E00A" ); // Cms - Layout Detail
            DeleteBlockType( "5996BF81-F2E2-4702-B401-B0B1B6667DAE" ); // Cms - Layout List

            DeletePage( "E6217A2B-B16F-4E84-BF67-795CA7F5F9AA" ); // Layout Detail

            //DeleteLayout( "6C128968-279F-4578-BB08-1913203C484B" ); // Default Panel
            Sql( @" 
    UPDATE [Layout] SET [IsSystem] = 0, [Guid] = NEWID() WHERE [Guid] = '6C128968-279F-4578-BB08-1913203C484B'
" );

        }
    }
}
