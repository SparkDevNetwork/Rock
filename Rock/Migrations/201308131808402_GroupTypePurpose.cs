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
    public partial class GroupTypePurpose : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "GroupTypePurposeValueId", c => c.Int());
            CreateIndex("dbo.GroupType", "GroupTypePurposeValueId");
            AddForeignKey("dbo.GroupType", "GroupTypePurposeValueId", "dbo.DefinedValue", "Id");

            AddDefinedType( "Group Type", "Group Type Purpose", "The purpose of a group type", "B23F1E45-BC26-4E82-BEB3-9B191FE5CCC3" );
            AddDefinedValue( "B23F1E45-BC26-4E82-BEB3-9B191FE5CCC3", "Check-in Template", "A Group Type where the purpose is for check-in groups", "4A406CB0-495B-4795-B788-52BDFDE00B01" ); 
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "4A406CB0-495B-4795-B788-52BDFDE00B01" );
            DeleteDefinedType( "B23F1E45-BC26-4E82-BEB3-9B191FE5CCC3" );
            
            DropForeignKey("dbo.GroupType", "GroupTypePurposeValueId", "dbo.DefinedValue");
            DropIndex("dbo.GroupType", new[] { "GroupTypePurposeValueId" });
            DropColumn("dbo.GroupType", "GroupTypePurposeValueId");
        }
    }
}
