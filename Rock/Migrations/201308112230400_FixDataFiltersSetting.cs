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
    public partial class FixDataFiltersSetting : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Block had wrong name for the container
            Sql( @"
    UPDATE AV SET
	    [Value] = 'Rock.Reporting.DataFilterContainer, Rock'
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A
	    ON A.[Id] = AV.[AttributeId]
	    AND A.[Guid] = '259AF14D-0214-4BE4-A7BF-40423EA07C99'
    INNER JOIN [Block] B
	    ON B.[Id] = AV.[EntityId]
	    AND B.[Guid] = 'B6F6DBF7-96CA-4A6A-AFB3-ED2278EEB70E'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // No need to set it back to the incorrect value
        }
    }
}
