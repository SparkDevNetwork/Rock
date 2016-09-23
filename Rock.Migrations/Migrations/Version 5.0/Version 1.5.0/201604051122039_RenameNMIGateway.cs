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
    public partial class RenameNMIGateway : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    -- Delete the new EntityType/Attributes if already created
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.NMI.Gateway' )
    IF @EntityTypeId IS NOT NULL
    BEGIN
        DELETE [Attribute] WHERE [EntityTypeId] = @EntityTypeId
        DELETE [EntityType] WHERE [Id] = @EntityTypeId
    END

    UPDATE [EntityType] SET [Name] = 'Rock.NMI.Gateway' WHERE [Name] = 'Rock.Financial.NMIGateway'
" );
            RockMigrationHelper.UpdateEntityType( "Rock.NMI.Gateway", "NMI Gateway", "Rock.NMI.Gateway, Rock.NMI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "B8282486-7866-4ED5-9F24-093D25FF0820" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
