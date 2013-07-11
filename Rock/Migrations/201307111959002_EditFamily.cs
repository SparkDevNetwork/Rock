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
    public partial class EditFamily : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage("08DBD8A5-2C35-4146-B4A8-0F7652348B25","Edit Family","","Default","E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27","");
            AddBlockType("CRM - Person Detail - Edit Family","","~/Blocks/CRM/PersonDetail/EditFamily.ascx","B4EB68FE-1A73-40FD-8236-78C9A015BDDE");
            AddBlock( "E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27", "B4EB68FE-1A73-40FD-8236-78C9A015BDDE", "Edit Family", "", "Content", 0, "E3A7A9E0-9321-4153-928F-94AB0B576A3B" );
            AddPageRoute( "E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27", "EditFamily/{PersonId}/{FamilyId}" );
            AddPageContext( "E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27", "Rock.Model.Person", "PersonId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock("E3A7A9E0-9321-4153-928F-94AB0B576A3B"); // Edit Family
            DeleteBlockType("B4EB68FE-1A73-40FD-8236-78C9A015BDDE"); // CRM - Person Detail - Edit Family
            DeletePage("E9E1E5F2-467D-47CB-AF41-B4D9EF8B0B27"); // Edit Family
        }
    }
}
