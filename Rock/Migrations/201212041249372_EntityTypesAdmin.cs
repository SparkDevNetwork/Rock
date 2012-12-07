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
    public partial class EntityTypesAdmin : RockMigration_1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.EntityType", "AssemblyName", c => c.String(maxLength: 200));
            AddColumn("dbo.EntityType", "IsEntity", c => c.Boolean(nullable: false));
            AddColumn("dbo.EntityType", "IsSecured", c => c.Boolean(nullable: false));

            AddBlockType( "Entity Types", "Administer the IEntity entity types", "~/Blocks/Administration/EntityTypes.ascx", "8098DF5D-4B87-4FAF-BA65-E017C5A93353" );
            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Business Object Administration", "Edit the name and default security for each of the business objects in Rock", "F7F41856-F7EA-49A8-9D9B-917AC1964602" );
            AddBlock( "F7F41856-F7EA-49A8-9D9B-917AC1964602", "8098DF5D-4B87-4FAF-BA65-E017C5A93353", "Entity Types", "Content", "8139E294-EAD2-48C2-9061-91EFDFD18836" );

            Sql( @"UPDATE [EntityType] SET [Name] = 'Rock.Security.GlobalDefault' WHERE [Name] = 'Global'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"UPDATE [EntityType] SET [Name] = 'Global' WHERE [Name] = 'Rock.Security.GlobalDefault'" );

            DeleteBlock( "8139E294-EAD2-48C2-9061-91EFDFD18836" );
            DeletePage( "F7F41856-F7EA-49A8-9D9B-917AC1964602" );
            DeleteBlockType( "8098DF5D-4B87-4FAF-BA65-E017C5A93353" );

            DropColumn( "dbo.EntityType", "IsSecured" );
            DropColumn("dbo.EntityType", "IsEntity");
            DropColumn("dbo.EntityType", "AssemblyName");
        }
    }
}
