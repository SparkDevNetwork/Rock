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
    public partial class UpdateSomeText : RockMigration_4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE [dbo].[Page] SET
    Name = 'Entity Administration',
    Title = 'Entity Administration'
WHERE [Guid] = 'F7F41856-F7EA-49A8-9D9B-917AC1964602'

UPDATE [dbo].[DefinedType] SET
    Name = 'Check-in Label Merge Fields'
WHERE [Guid] = 'E4D289A9-70FA-4381-913E-2A757AD11147'
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
