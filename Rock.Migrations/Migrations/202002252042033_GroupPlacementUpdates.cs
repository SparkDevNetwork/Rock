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
    public partial class GroupPlacementUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add "Registration Template Placement" page
            RockMigrationHelper.AddPage( true, SystemGuid.Page.EVENT_REGISTRATION, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Registration Template Placement", "", SystemGuid.Page.REGISTRATION_TEMPLATE_PLACEMENT, "" );

            // set this newly-added Page's [DisplayInNavWhen] to Never
            Sql( $@"UPDATE [Page]
SET [DisplayInNavWhen] = 2
WHERE ([Guid] = '{SystemGuid.Page.REGISTRATION_TEMPLATE_PLACEMENT}');" );

            // add "/RegistrationTemplatePlacement/{RegistrationTemplateId}" PageRoute
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.REGISTRATION_TEMPLATE_PLACEMENT, "RegistrationTemplatePlacement/{RegistrationTemplateId}", SystemGuid.PageRoute.REGISTRATION_TEMPLATE_PLACEMENT );

            // Add Block to Page: Registration Template Placement Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.REGISTRATION_TEMPLATE_PLACEMENT.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.EVENT_REGISTRATION_GROUP_PLACEMENT.AsGuid(), "Registration Group Placement", "Main", @"", @"", 0, "13F4728A-6933-4C2C-A823-A3D1E3D1E82D" );

            // add [optional] "Registration Template" block setting to RegistrationInstanceGroupPlacement block
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.EVENT_REGISTRATION_GROUP_PLACEMENT, SystemGuid.FieldType.REGISTRATION_TEMPLATE, "Registration Template", "RegistrationTemplate", "Registration Template", @"If provided, this Registration Template will override any Registration Template specified in a URL parameter.", 0, @"", "7643B0AD-A85E-4094-ACAF-394B46219850" );

            // add [required] "Registration Template Placement Page" block setting to RegistrationTemplateDetail block type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( SystemGuid.BlockType.EVENT_REGISTRATION_TEMPLATE_DETAIL, SystemGuid.FieldType.PAGE_REFERENCE, "Registration Template Placement Page", "RegistrationTemplatePlacementPage", "Registration Template Placement Page", @"The default Page to link to in order to view placements for a new template", 0, $"{SystemGuid.Page.REGISTRATION_TEMPLATE_PLACEMENT},{SystemGuid.PageRoute.REGISTRATION_TEMPLATE_PLACEMENT}", "2EC8E93C-45DA-480A-B87B-2DADAFDBAD00" );

            // set this newly-added block setting to required
            Sql( @"UPDATE [Attribute]
SET [IsRequired] = 1
WHERE ([Guid] = '2EC8E93C-45DA-480A-B87B-2DADAFDBAD00');" );

            // add an explicit AttributeValue for the newly-added "Registration Template Placement Page" block setting, for the existing RegistrationTemplateDetail page's block instance
            RockMigrationHelper.AddBlockAttributeValue( "D6372D00-9FA3-49BF-B0F2-0BE67B5F5D39", "2EC8E93C-45DA-480A-B87B-2DADAFDBAD00", $"{SystemGuid.Page.REGISTRATION_TEMPLATE_PLACEMENT},{SystemGuid.PageRoute.REGISTRATION_TEMPLATE_PLACEMENT}" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
