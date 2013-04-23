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
    public partial class BinaryFileTypeContributionImage : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"insert into [BinaryFileType] (IsSystem, Name, Description, IconCssClass, [Guid])
                values (1, 'Contribution Image', 'Scanned image of a check or envelope', 'icon-money', '6D18A9C4-34AB-444A-B95B-C644019465AC')");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "DELETE FROM [BinaryFileType] where [Guid] = '6D18A9C4-34AB-444A-B95B-C644019465AC'" );
        }
    }
}
