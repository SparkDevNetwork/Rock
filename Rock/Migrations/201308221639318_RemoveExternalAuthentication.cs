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
    public partial class RemoveExternalAuthentication : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DECLARE @AttributeId int
    SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '259AF14D-0214-4BE4-A7BF-40423EA07C99')

    DECLARE @BlockId int
    SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = 'FA273FE7-C278-4A41-967B-C7ED85C48B3B')

    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId
" );
            DeleteBlock( "FA273FE7-C278-4A41-967B-C7ED85C48B3B" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddBlock("CE2170A9-2C8E-40B1-A42E-DFA73762D01D", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "External", "", "Content", 1, "FA273FE7-C278-4A41-967B-C7ED85C48B3B");
            AddBlockAttributeValue( "FA273FE7-C278-4A41-967B-C7ED85C48B3", "259AF14D-0214-4BE4-A7BF-40423EA07C99", "Rock.Security.ExternalAuthenticationContainer, Rock" );
        }
    }
}
