// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PersonAliasConvert : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            Sql( @"
    /*
    <doc>
	    <summary>
 		    This function returns the primary person alias id for the person id given.
	    </summary>

	    <returns>
		    Int of the primary person alias id
	    </returns>
	    <remarks>
		
	    </remarks>
	    <code>
		    SELECT [dbo].[ufnUtility_GetPrimaryPersonAliasId](1)
	    </code>
    </doc>
    */

    CREATE FUNCTION [dbo].[ufnUtility_GetPrimaryPersonAliasId](@PersonId int) 

    RETURNS int AS

    BEGIN

	    RETURN ( 
			SELECT TOP 1 [Id] FROM [PersonAlias]
			WHERE [PersonId] = @PersonId AND [AliasPersonId] = @PersonId
		)

    END
" );

            AddColumn( "dbo.CommunicationRecipient", "PersonAliasId", c => c.Int( nullable: false ) );
            AddColumn( "dbo.Communication", "SenderPersonAliasId", c => c.Int() );
            AddColumn( "dbo.Communication", "ReviewerPersonAliasId", c => c.Int() );

            Sql( @"
    UPDATE [CommunicationRecipient] SET
          [PersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [PersonId] )

    UPDATE [Communication] SET
          [SenderPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [SenderPersonId] )
        , [ReviewerPersonAliasId] = [dbo].[ufnUtility_GetPrimaryPersonAliasId] ( [ReviewerPersonId] )
    
" );
            DropForeignKey( "dbo.Communication", "ReviewerPersonId", "dbo.Person" );
            DropForeignKey( "dbo.Communication", "SenderPersonId", "dbo.Person" );
            DropForeignKey( "dbo.CommunicationRecipient", "PersonId", "dbo.Person" );

            DropIndex( "dbo.CommunicationRecipient", new[] { "PersonId" } );
            DropIndex( "dbo.Communication", new[] { "SenderPersonId" } );
            DropIndex( "dbo.Communication", new[] { "ReviewerPersonId" } );

            DropColumn( "dbo.CommunicationRecipient", "PersonId" );
            DropColumn( "dbo.Communication", "SenderPersonId" );
            DropColumn( "dbo.Communication", "ReviewerPersonId" );

            CreateIndex( "dbo.CommunicationRecipient", "PersonAliasId" );
            CreateIndex( "dbo.Communication", "SenderPersonAliasId" );
            CreateIndex( "dbo.Communication", "ReviewerPersonAliasId" );

            AddForeignKey( "dbo.Communication", "ReviewerPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.Communication", "SenderPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.CommunicationRecipient", "PersonAliasId", "dbo.PersonAlias", "Id" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.Communication", "ReviewerPersonId", c => c.Int() );
            AddColumn( "dbo.Communication", "SenderPersonId", c => c.Int() );
            AddColumn( "dbo.CommunicationRecipient", "PersonId", c => c.Int( nullable: false ) );

            Sql( @"
    UPDATE [CommunicationRecipient] SET
          [PersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [PersonAliasId] )

    UPDATE [Communication] SET
          [SenderPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [SenderPersonAliasId] )
        , [ReviewerPersonId] = [dbo].[ufnUtility_GetPersonIdFromPersonAlias] ( [ReviewerPersonAliasId] )
    
" );

            DropForeignKey( "dbo.CommunicationRecipient", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Communication", "SenderPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Communication", "ReviewerPersonAliasId", "dbo.PersonAlias" );

            DropIndex( "dbo.Communication", new[] { "ReviewerPersonAliasId" } );
            DropIndex( "dbo.Communication", new[] { "SenderPersonAliasId" } );
            DropIndex( "dbo.CommunicationRecipient", new[] { "PersonAliasId" } );

            DropColumn( "dbo.Communication", "ReviewerPersonAliasId" );
            DropColumn( "dbo.Communication", "SenderPersonAliasId" );
            DropColumn( "dbo.CommunicationRecipient", "PersonAliasId" );

            CreateIndex( "dbo.Communication", "ReviewerPersonId" );
            CreateIndex( "dbo.Communication", "SenderPersonId" );
            CreateIndex( "dbo.CommunicationRecipient", "PersonId" );

            AddForeignKey( "dbo.CommunicationRecipient", "PersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.Communication", "SenderPersonId", "dbo.Person", "Id" );
            AddForeignKey( "dbo.Communication", "ReviewerPersonId", "dbo.Person", "Id" );

            Sql( @"
    DROP FUNCTION [dbo].[ufnUtility_GetPrimaryPersonAliasId]
" );
        }
    }
}
