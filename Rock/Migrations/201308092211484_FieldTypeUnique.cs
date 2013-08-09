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
    public partial class FieldTypeUnique : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // delete any non-core fieldtypes that are dups of core fieldtypes
            Sql( "delete from [FieldType] where [IsSystem] = 0 and (([Class] + ',' + [Assembly]) in (select [Class] + ',' + [Assembly] from [FieldType] where [IsSystem]=1)) " );
            
            // Enforce Uniqueness on FieldType Class,Assembly.  Will also speed up common lookups
            CreateIndex( "FieldType", new string[] { "Class", "Assembly" }, true, "IX_FieldTypeClass" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "FieldType", "IX_FieldTypeClass" ); 
        }
    }
}
