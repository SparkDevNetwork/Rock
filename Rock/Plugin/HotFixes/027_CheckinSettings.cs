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
using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Migration to add additional check-in settings.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 27, "1.6.4" )]
    public class CheckinSettings : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateAttributeQualifier( "733944B7-A0D5-41B4-94D4-DE007F72B6F0", "values", "0^Family,1^Person,2^Location,3^Check-Out", "E77DF4E6-A995-4C82-BBB7-DB57739D66F3" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.BOOLEAN, "GroupTypePurposeValueId", "", "Allow Checkout", "", 0, "False", "37EB8C83-A5DC-4A9B-8816-D93F07B2A7C5", "core_checkin_AllowCheckout" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", Rock.SystemGuid.FieldType.SINGLE_SELECT, "GroupTypePurposeValueId", "", "Auto Select Options", "", 0, "0", "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "core_checkin_AutoSelectOptions" );
            RockMigrationHelper.UpdateAttributeQualifier( "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "fieldtype", "ddl", "F614DAFE-18BC-4FFE-A0D7-90A59DAF79AA" );
            RockMigrationHelper.UpdateAttributeQualifier( "C053648F-8A85-4C0C-BB24-41E9ABB56EEF", "values", "0^People Only,1^People and Their Area/Group/Location", "542F273C-A5DB-4268-9D32-29DADA386E74" );

            Sql( @"
        DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.GroupType' )
        DECLARE @CheckInTemplatePurposeId int = ( SELECT TOP 1[Id] FROM[DefinedValue] WHERE[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' )
        IF @GroupTypeEntityTypeId IS NOT NULL AND @CheckInTemplatePurposeId IS NOT NULL
        BEGIN

            UPDATE[Attribute] SET[EntityTypeQualifierValue] = CAST( @CheckInTemplatePurposeId AS varchar )
            WHERE[EntityTypeId] = @GroupTypeEntityTypeId
            AND[EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
            AND[Key] LIKE 'core_checkin_%'

        END
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
