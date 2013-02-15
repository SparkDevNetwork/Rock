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
    public partial class BlockTypeListDetail : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // delete old BlockTypes block
            DeleteBlock( "74B3B85E-33B6-4ACF-8338-CBC12888BC74" );
            DeleteBlockType( "0244A072-6216-49F4-92EC-E6B5FFFF03B5" );

            // delete old WorkflowTypeTreeView (Replaced with CategoryTreeView)
            DeleteBlockType( "BBC6E8B3-3CBD-4990-8C7F-C53D8A06794C" );

            // old blocktypes that no longer exist
            /*
                ~/Blocks/Security/CreateAccount.ascx
                ~/Blocks/Administration/Address/Geocoding.ascx
                ~/Blocks/Administration/Address/Standardization.ascx             
            */
            DeleteBlockType( "292D3578-BC27-4DAB-BFC3-6D249E0905E0" );
            DeleteBlockType( "340F1474-3403-4426-A8F0-2E33C1B4BF2F" );
            DeleteBlockType( "363C3382-5148-4097-82B2-C85A4910A837" );

            AddPage( "5FBE9019-862A-41C6-ACDC-287D7934757D", "Block Type Detail", "", "Default", "C694AD7C-46DD-47FE-B2AC-1CF158FA6504" );
            AddBlockType( "Administration - Block Type Detail", "", "~/Blocks/Administration/BlockTypeDetail.ascx", "A3E648CC-0F19-455F-AF1D-B70A8205802D" );
            AddBlockType( "Administration - Block Type List", "", "~/Blocks/Administration/BlockTypeList.ascx", "78A31D91-61F6-42C3-BB7D-676EDC72F35F" );
            AddBlock( "5FBE9019-862A-41C6-ACDC-287D7934757D", "78A31D91-61F6-42C3-BB7D-676EDC72F35F", "Block Type List", "", "Content", 1, "BBE02100-9648-4CC2-8376-1F28A9A77A1E" );
            AddBlock( "C694AD7C-46DD-47FE-B2AC-1CF158FA6504", "A3E648CC-0F19-455F-AF1D-B70A8205802D", "Block Type Detail", "", "Content", 0, "CEEDB73D-A394-4049-894A-BE8F6A791FD1" );
            AddBlockTypeAttribute( "78A31D91-61F6-42C3-BB7D-676EDC72F35F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "2C0C9E90-1A1E-4F27-A796-C5EF87818FCE" );
            // Attrib Value for Block Type List:Detail Page Guid
            AddBlockAttributeValue( "BBE02100-9648-4CC2-8376-1F28A9A77A1E", "2C0C9E90-1A1E-4F27-A796-C5EF87818FCE", "c694ad7c-46dd-47fe-b2ac-1cf158fa6504" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "2C0C9E90-1A1E-4F27-A796-C5EF87818FCE" ); //Detail Page Guid
            DeleteBlock( "BBE02100-9648-4CC2-8376-1F28A9A77A1E" ); //Block Type List
            DeleteBlock( "CEEDB73D-A394-4049-894A-BE8F6A791FD1" ); //Block Type Detail
            DeleteBlockType( "A3E648CC-0F19-455F-AF1D-B70A8205802D" ); //Administration - Block Type Detail
            DeleteBlockType( "78A31D91-61F6-42C3-BB7D-676EDC72F35F" ); //Administration - Block Type List
            DeletePage( "C694AD7C-46DD-47FE-B2AC-1CF158FA6504" ); //Block Type Detail
        }
    }
}
