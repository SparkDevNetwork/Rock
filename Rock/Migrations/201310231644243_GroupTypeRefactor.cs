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
    public partial class GroupTypeRefactor : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB", "2BA19878-F9B8-4ABF-91E1-75A7CF92BD8B", "Role Detail", "", "E622DDF5-1872-4BAE-83C0-EDD519BDD724", "" );

            // Add Block to Page: Group Type Detail
            AddBlock( "5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB", "", "CC3F3EBE-4120-4612-BB02-C79D3604CA99", "Group Role List", "Content", 1, "5679C429-7C9C-4DAC-A97B-3766E80808BB" );
            
            // Add Block to Page: Role Detail
            AddBlock( "E622DDF5-1872-4BAE-83C0-EDD519BDD724", "", "FAE8AC76-0AF4-4A64-BDF6-FEBE857A74D2", "Group Role Detail", "Content", 0, "CC2B29C7-E0B1-480F-B615-90AA3D84A154" );

            // Attrib Value for Block:Group Role List, Attribute:Detail Page Page: Group Type Detail
            AddBlockAttributeValue( "5679C429-7C9C-4DAC-A97B-3766E80808BB", "587C4958-65EF-4D1F-AAB4-B264DB203D93", "E622DDF5-1872-4BAE-83C0-EDD519BDD724" );

            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] IN ('5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB','E622DDF5-1872-4BAE-83C0-EDD519BDD724')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Group Role Detail, from Page: Role Detail
            DeleteBlock( "CC2B29C7-E0B1-480F-B615-90AA3D84A154" );

            // Remove Block: Group Role List, from Page: Group Type Detail
            DeleteBlock( "5679C429-7C9C-4DAC-A97B-3766E80808BB" );

            DeletePage( "E622DDF5-1872-4BAE-83C0-EDD519BDD724" ); // Role Detail

            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] IN ('5CD8E024-710B-4EDE-8C8C-4C9E15E6AFAB')" );
        }
    }
}
