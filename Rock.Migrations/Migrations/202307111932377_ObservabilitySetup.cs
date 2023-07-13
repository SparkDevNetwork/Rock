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
    using System.Net;
    using Rock.SystemGuid;

    /// <summary>
    ///
    /// </summary>
    public partial class ObservabilitySetup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Redirector HTTP module component
            RockMigrationHelper.UpdateEntityType( "Rock.Web.HttpModules.Observability", "Observability HTTP Module", "Rock.Web.HttpModules.Observability, Rock, Version=1.16.0.6, Culture=neutral, PublicKeyToken=null", false, true, EntityType.HTTP_MODULE_OBSERVABILITY );

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Web.HttpModules.Observability", FieldType.BOOLEAN, "", "", "Active", "", "Determines if the module should be enabled.", 0, "False", SystemGuid.Attribute.HTTP_MODULE_OBSERVABILITY_ACTIVE, null );

            Sql( $@"  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{SystemGuid.Attribute.HTTP_MODULE_OBSERVABILITY_ACTIVE}')

                      INSERT INTO [AttributeValue]
                      ([AttributeId], [EntityId], [Value], [IsSystem], [Guid])
                      VALUES
                      (@AttributeId, 0, 'True', 0, '00BDF1A9-5B6E-4E91-8A56-5B3C005F8C26')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete HTTP module component
            RockMigrationHelper.DeleteEntityType( EntityType.HTTP_MODULE_OBSERVABILITY );
        }
    }
}
