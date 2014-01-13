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
    public partial class AddGroupChildGroupTypes : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "IF NOT EXISTS (SELECT 1 FROM [GroupTypeAssociation] WHERE [GroupTypeId] = 13 AND [ChildGroupTypeId] = 13) INSERT INTO [GroupTypeAssociation] ([GroupTypeId], [ChildGroupTypeId]) VALUES (13, 13)" );
            Sql( "IF NOT EXISTS (SELECT 1 FROM [GroupTypeAssociation] WHERE [GroupTypeId] = 23 AND [ChildGroupTypeId] = 23) INSERT INTO [GroupTypeAssociation] ([GroupTypeId], [ChildGroupTypeId]) VALUES (23, 23)" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [GroupTypeAssociation] WHERE [GroupTypeId] = 13 AND [ChildGroupTypeId] = 13" );
            Sql( @"DELETE FROM [GroupTypeAssociation] WHERE [GroupTypeId] = 23 AND [ChildGroupTypeId] = 23" );
        }
    }
}
