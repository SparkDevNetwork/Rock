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
    public partial class RegistrationAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.EntityType", "AttributesSupportPrePostHtml", c => c.Boolean(nullable: false));
            AddColumn("dbo.Attribute", "PreHtml", c => c.String());
            AddColumn("dbo.Attribute", "PostHtml", c => c.String());
            AddColumn("dbo.RegistrationTemplate", "RegistrationAttributeTitleStart", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "RegistrationAttributeTitleEnd", c => c.String(maxLength: 200));

            // Enable AttributesSupportPrePostHtml only for Rock.Model.Registration
            Sql( $"UPDATE [EntityType] SET [AttributesSupportPrePostHtml] = 1 WHERE [Guid] = '{Rock.SystemGuid.EntityType.REGISTRATION}'" );

            RockMigrationHelper.UpdateRegistrationAttributeCategory( "Start of Registration", "", "Registration Attributes that should be prompted for at the beginning of the registration process.", Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION, 0 );
            RockMigrationHelper.UpdateRegistrationAttributeCategory( "End of Registration", "", "Registration Attributes that should be prompted for at the end of the registration process.", Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION, 1 );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.RegistrationTemplate", "RegistrationAttributeTitleEnd");
            DropColumn("dbo.RegistrationTemplate", "RegistrationAttributeTitleStart");
            DropColumn("dbo.Attribute", "PostHtml");
            DropColumn("dbo.Attribute", "PreHtml");
            DropColumn("dbo.EntityType", "AttributesSupportPrePostHtml");
        }
    }
}
