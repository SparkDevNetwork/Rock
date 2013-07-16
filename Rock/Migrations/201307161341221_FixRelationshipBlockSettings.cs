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
    public partial class FixRelationshipBlockSettings : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteBlockAttribute( "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E" );
            DeleteBlockAttribute( "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B" );

            AddBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Create Group", "CreateGroup", "", "Should group be created if a group/role cannot be found for the current person.", 1, "True", "31BF6250-200B-4136-9338-E209B0E6AA57" );
            AddBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "3BB25568-E793-4D12-AE80-AC3FDA6FD8A8", "Group Type/Role Filter", "GroupType/RoleFilter", "", "The Group Type and role to display other members from.", 2, "", "329CD8FA-4856-4B9A-8060-EA33356C7D2C" );

            // Relationships
            AddBlockAttributeValue( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", "31BF6250-200B-4136-9338-E209B0E6AA57", "True" );
            AddBlockAttributeValue( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", "329CD8FA-4856-4B9A-8060-EA33356C7D2C", "7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42" );

            // Peer network
            AddBlockAttributeValue( "32847AAF-15F5-4F8B-9F84-92D6AE827857", "31BF6250-200B-4136-9338-E209B0E6AA57", "True" );
            AddBlockAttributeValue( "32847AAF-15F5-4F8B-9F84-92D6AE827857", "329CD8FA-4856-4B9A-8060-EA33356C7D2C", "CB9A0E14-6FCF-4C07-A49A-D7873F45E196" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // None, attributes should have been created previously and should still exist on down
        }
    }
}
