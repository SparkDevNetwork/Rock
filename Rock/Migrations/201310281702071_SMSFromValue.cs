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
    public partial class SMSFromValue : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDefinedType( "Communication", "SMS From Values", "The available values that can be used to send text messages from.  This is usually a phone number or short code that has been set up with your SMS provider", "611BDE1F-7405-4D16-8626-CCFEDB0E62BE" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedType( "611BDE1F-7405-4D16-8626-CCFEDB0E62BE" );
        }
    }
}
