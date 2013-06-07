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
    public partial class DeadBlockCleanup01 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // '~/Blocks/ButtonDropDownListExample.ascx'
            DeleteBlockType( "8c1a8708-c16c-4c1c-8668-1ef530724dd5" );
            
            // '~/Blocks/Finance/OneTimeGift.ascx'
            DeleteBlockType( "4a2aa794-a968-4ccd-973a-c90fd589996f" );
            
            // '~/Blocks/Finance/RecurringGift.ascx'
            DeleteBlockType( "f679692f-133e-4f57-9072-d87c675c3283" );
            
            // '~/Blocks/Crm/PersonDetail/GroupMembers.ascx'
            DeleteBlockType( "3e14b410-22cb-49cc-8a1f-c30ecd0e816a" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
