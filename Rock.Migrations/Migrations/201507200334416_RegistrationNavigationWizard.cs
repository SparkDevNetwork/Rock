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
    public partial class RegistrationNavigationWizard : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block to Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlock( "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Registration Nav", "Feature", "", "", 0, "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43" );

            // Add/Update HtmlContent for Block: Registration Nav
            RockMigrationHelper.UpdateHtmlContentBlock( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", @"<div class=""wizard"">

     <div class=""wizard-item active"">
        <a href=""~/page/{{ 'Global' | Page:'Id' }}{% if Context %}?RegistrationTemplateId={{ Context.RegistrationTemplate.Id }}{% endif %}"">
            <div class=""wizard-item-icon"">
                <i class=""fa fa-fw fa-clipboard""></i>
            </div>
            <div class=""wizard-item-label"">
                {% if Context %}{{ Context.RegistrationTemplate.Name }}{% else %}Template{% endif %}
            </div>
        </a>
    </div>

    <div class=""wizard-item"">
        <div class=""wizard-item-icon"">
            <i class=""fa fa-fw fa-file-o""></i>
        </div>
        <div class=""wizard-item-label"">
            Instance
        </div>
    </div>

    <div class=""wizard-item"">
        <div class=""wizard-item-icon"">
            <i class=""fa fa-fw fa-group""></i>
        </div>
        <div class=""wizard-item-label"">
            Registration
        </div>
    </div>

    <div class=""wizard-item"">
        <div class=""wizard-item-icon"">
            <i class=""fa fa-fw fa-user""></i>
        </div>
        <div class=""wizard-item-label"">
            Registrant
        </div>
    </div>
</div>

", "51571E36-059A-45AA-A811-72EAD52150C3" );

            // Attrib Value for Block:Registration Nav, Attribute:Use Code Editor Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "0673E015-F8DD-4A52-B380-C758011331B2", @"True" );
            // Attrib Value for Block:Registration Nav, Attribute:Image Root Folder Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "26F3AFC6-C05B-44A4-8593-AFE1D9969B0E", @"~/Content" );
            // Attrib Value for Block:Registration Nav, Attribute:User Specific Folders Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE", @"False" );
            // Attrib Value for Block:Registration Nav, Attribute:Document Root Folder Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "3BDB8AED-32C5-4879-B1CB-8FC7C8336534", @"~/Content" );
            // Attrib Value for Block:Registration Nav, Attribute:Entity Type Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "6783D47D-92F9-4F48-93C0-16111D675A0F", @"a01e3e99-a8ad-4c6c-baac-98795738ba70" );
            // Attrib Value for Block:Registration Nav, Attribute:Enable Debug Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "48FF43A9-8E12-4768-80A9-88FBB81F11D8", @"False" );
            // Attrib Value for Block:Registration Nav, Attribute:Cache Duration Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", @"0" );
            // Attrib Value for Block:Registration Nav, Attribute:Require Approval Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A", @"False" );
            // Attrib Value for Block:Registration Nav, Attribute:Enable Versioning Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "7C1CE199-86CF-4EAE-8AB3-848416A72C58", @"False" );
            // Attrib Value for Block:Registration Nav, Attribute:Context Parameter Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "3FFC512D-A576-4289-B648-905FD7A64ABB", @"" );
            // Attrib Value for Block:Registration Nav, Attribute:Context Name Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9E8B637C-A9C7-4B5C-81AB-22E3C0FFEE43", "466993F7-D838-447A-97E7-8BBDA6A57289", @"" );

            // Add/Update PageContext for Page:Registration, Entity: Rock.Model.Registration, Parameter: RegistrationId
            RockMigrationHelper.UpdatePageContext( "FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "Rock.Model.Registration", "RegistrationId", "3C751D4B-EEF8-468E-B36B-A22724BC35E8" );

            // Add/Update PageContext for Page:Event Registration, Entity: Rock.Model.RegistrationTemplate, Parameter: RegistrationTemplateId
            RockMigrationHelper.UpdatePageContext( "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "Rock.Model.RegistrationTemplate", "RegistrationTemplateId", "D726BAC1-0D08-4953-A549-CD284BDF0340" );

            // Workflow Type "ViewList" security
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.WorkflowType", 0, "ViewList", true, null, 1, "94C9F589-49EA-43DC-A9E2-621B64860FEE" );

            // Fix for Large Strings for Attribute Values
            Sql( @"
    DROP INDEX [IX_ValueAsNumeric] ON [AttributeValue]

    ALTER TABLE AttributeValue DROP COLUMN ValueAsNumeric

    ALTER TABLE AttributeValue ADD ValueAsNumeric AS (
        CASE 
            WHEN len([value]) < (100)
                THEN CASE 
                        WHEN (
                                isnumeric([value]) = (1)
                                AND NOT [value] LIKE '%[^0-9.]%'
                                AND NOT [value] LIKE '%[.]%'
                                )
                            THEN TRY_CAST([value] AS [numeric](38, 10))
                        END
            END
        )

    CREATE NONCLUSTERED INDEX [IX_ValueAsNumeric] ON [AttributeValue] ([ValueAsNumeric] )
" );

            // Blocks on external homepage are system and therefore undeleteable
            Sql( @"
    UPDATE [Block]
    SET [IsSystem] = 0
    WHERE [Guid] IN ('095027CB-9114-4CD5-ABE8-1E8882422DCF', '2E0FFD29-B4AF-4A5E-B528-667168762ABC')
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "ACC82748-157F-BF8E-4E34-FFD3C05269B3" );
            RockMigrationHelper.DeleteDefinedValue( "18B0F890-59D2-CF88-4F48-5B51097FDA0B" );
            RockMigrationHelper.DeleteDefinedType( "251D752B-0595-C3A6-4E2A-AD0264DAFCCD" );
        }
    }
}
