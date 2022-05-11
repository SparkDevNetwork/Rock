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
    public partial class AddCheckinRegistrationChildrenDisplayAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
                DECLARE @GroupTypeEntityId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType') -- Group Type Entity ID
                DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '7525c4cb-ee6b-41d4-9b64-a08048d5a5c0') -- Single Select
                DECLARE @EntityTypeQualifierValue int = 142

                IF NOT EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @GroupTypeEntityId
                        AND [EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
                        AND [EntityTypeQualifierValue] = @EntityTypeQualifierValue
                        AND [Key] = 'core_checkin_registration_DisplayBirthdateOnChildren' )
                BEGIN
                    INSERT INTO [Attribute] (
                            [IsSystem]
                        , [FieldTypeId]
                        , [EntityTypeId]
                        , [EntityTypeQualifierColumn]
                        , [EntityTypeQualifierValue]
                        , [Key]
                        , [Name]
                        , [Description]
                        , [Order]
                        , [IsGridColumn]
                        , [DefaultValue]
                        , [IsMultiValue]
                        , [IsRequired]
                        , [Guid])
                    VALUES (
                            1
                        , @FieldTypeId
                        , @GroupTypeEntityId
                        , 'GroupTypePurposeValueId'
                        , @EntityTypeQualifierValue
                        , 'core_checkin_registration_DisplayBirthdateOnChildren'
                        , 'Display Birthdate On Children'
                        , 'Determines whether the Check-In Registration Birthdate should be displayed for children.'
                        , 0
                        , 0
                        , 'Optional'
                        , 0
                        , 0
                        , '0257DDA8-6494-48B0-B8CD-0769F2F54025')
                END

                IF NOT EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @GroupTypeEntityId
                        AND [EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
                        AND [EntityTypeQualifierValue] = @EntityTypeQualifierValue
                        AND [Key] = 'core_checkin_registration_DisplayGradeOnChildren' )
                BEGIN
                    INSERT INTO [Attribute] (
                            [IsSystem]
                        , [FieldTypeId]
                        , [EntityTypeId]
                        , [EntityTypeQualifierColumn]
                        , [EntityTypeQualifierValue]
                        , [Key]
                        , [Name]
                        , [Description]
                        , [Order]
                        , [IsGridColumn]
                        , [DefaultValue]
                        , [IsMultiValue]
                        , [IsRequired]
                        , [Guid])
                    VALUES (
                            1
                        , @FieldTypeId
                        , @GroupTypeEntityId
                        , 'GroupTypePurposeValueId'
                        , @EntityTypeQualifierValue
                        , 'core_checkin_registration_DisplayGradeOnChildren'
                        , 'Display Grade On Children'
                        , 'Determines whether the Check-In Registration Grade should be displayed for children.'
                        , 0
                        , 0
                        , 'Optional'
                        , 0
                        , 0
                        , 'F8C29F18-B0A3-4EE7-B63F-0A766F276C74')
                END" );

            // Insert the attribute qualifiers.
            RockMigrationHelper.AddAttributeQualifier( "0257DDA8-6494-48B0-B8CD-0769F2F54025", "fieldtype", "ddl", "5EB43974-F7FA-43E5-9799-A3528873C217" );
            RockMigrationHelper.AddAttributeQualifier( "0257DDA8-6494-48B0-B8CD-0769F2F54025", "values", "Hide,Optional,Required", "C8C7E5EC-D5B1-4365-8D9D-D5F87FC2400B" );

            RockMigrationHelper.AddAttributeQualifier( "F8C29F18-B0A3-4EE7-B63F-0A766F276C74", "fieldtype", "ddl", "73E1C99D-8304-49D8-99C4-7175A05C159E" );
            RockMigrationHelper.AddAttributeQualifier( "F8C29F18-B0A3-4EE7-B63F-0A766F276C74", "values", "Hide,Optional,Required", "18031A97-6CA1-4770-85CD-EA5A0E35618E" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "0257DDA8-6494-48B0-B8CD-0769F2F54025" );
            RockMigrationHelper.DeleteAttribute( "F8C29F18-B0A3-4EE7-B63F-0A766F276C74" );
        }
    }
}
