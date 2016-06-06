// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    public partial class ConnectionStatus : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn( "dbo.Person", "PersonStatusValueId", "ConnectionStatusValueId" );

            Sql( @"
    UPDATE [Attribute] SET [Key] = 'DefaultConnectionStatus' WHERE [key] = 'DefaultPersonStatus'

    -- Delete values that may have been created by framework before running migration
    DELETE [Attribute] WHERE [EntityTypeId] IN (
	    SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus'   
    )
    DELETE [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus'   
    UPDATE [EntityType] SET 
	    [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus',
	    [AssemblyName] = 'Rock.PersonProfile.Badge.ConnectionStatus, Rock, Version=0.0.1.0, Culture=neutral, PublicKeyToken=null',
	    [FriendlyName] = 'Connection Status'
    WHERE [Name] = 'Rock.PersonProfile.Badge.PersonStatus'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    UPDATE [Attribute] SET [Key] = 'DefaultPersonStatus' WHERE [key] = 'DefaultConnectionStatus'

    -- Delete values that may have been created by framework before running migration
    DELETE [Attribute] WHERE [EntityTypeId] IN (
	    SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.PersonStatus'   
    )
    DELETE [EntityType] WHERE [Name] = 'Rock.PersonProfile.Badge.PersonStatus'   
    UPDATE [EntityType] SET 
	    [Name] = 'Rock.PersonProfile.Badge.PersonStatus',
	    [AssemblyName] = 'Rock.PersonProfile.Badge.PersonStatus, Rock, Version=0.0.1.0, Culture=neutral, PublicKeyToken=null',
	    [FriendlyName] = 'Person Status'
    WHERE [Name] = 'Rock.PersonProfile.Badge.ConnectionStatus'
" );

            RenameColumn( "dbo.Person", "ConnectionStatusValueId", "PersonStatusValueId" );
        }
    }
}
