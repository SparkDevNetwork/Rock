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
    public partial class CommunicationTransport : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Communication", "CommunicationChannelValueId", c => c.Int(nullable: false));
            AddForeignKey("dbo.Communication", "CommunicationChannelValueId", "dbo.DefinedValue", "Id");
            CreateIndex("dbo.Communication", "CommunicationChannelValueId");

            AddDefinedType( "Global", "Communication Channel", "The types of communication channels available", "DC8A841C-E91D-4BD4-A6A7-0DE765308E8F" );
            AddDefinedValue( "DC8A841C-E91D-4BD4-A6A7-0DE765308E8F", "Email", "Email communications", "FC51461D-0C31-4C6B-A7C8-B3E8482C1055" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedType( "DC8A841C-E91D-4BD4-A6A7-0DE765308E8F" );

            DropIndex("dbo.Communication", new[] { "CommunicationChannelValueId" });
            DropForeignKey("dbo.Communication", "CommunicationChannelValueId", "dbo.DefinedValue");
            DropColumn("dbo.Communication", "CommunicationChannelValueId");
        }
    }
}
