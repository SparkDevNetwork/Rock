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
    public partial class NoteContextFix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

    DECLARE @AttributeID int
    SET @AttributeID = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'F1BCF615-FBCA-4BC2-A912-C35C0DC04174')

    DECLARE @BlockID int
    SET @BlockID = (SELECT [Id] FROM [Block] WHERE [Guid] = '0B2B550C-B0C9-420E-9CF3-BEC8979108F2')

    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeID AND [EntityId] = @BlockID
    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Order],[Value],[Guid])
    VALUES (1, @AttributeID, @BlockID, 0, '72657ED8-D16E-492E-AC12-144C5E7567E7', '1A1B58C4-C461-40EC-9C10-724A18347183')

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
