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
    /// Add Should Require Trailing Forward Slash To UrlLinkFieldType
    /// </summary>
    public partial class AddShouldRequireTrailingForwardSlashToUrlLinkFieldType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.GLOBAL_PUBLIC_APPLICATION_ROOT,
                "ShouldRequireTrailingForwardSlash",
                "True",
                "068E9B90-0A42-43BF-A17B-052E78A99ABB" );

            RockMigrationHelper.UpdateGlobalAttribute( "PublicApplicationRoot",
                "",
                "",
                Rock.SystemGuid.Attribute.GLOBAL_PUBLIC_APPLICATION_ROOT,
                Rock.SystemGuid.FieldType.URL_LINK,
                "Public Application Root",
                "The application root URL of the public Rock website",
                0 );

            Sql( @"UPDATE AttributeValue
                    SET [Value] = CASE WHEN RIGHT([Value], 1) = '/' THEN [Value] 
			                    WHEN CHARINDEX('?', [Value], 0) > 0 THEN SUBSTRING([Value], 0, CHARINDEX('?', [Value], 0)) + '/'
			                    ELSE [Value] + '/' END
                    FROM Attribute a
                    INNER JOIN AttributeValue av ON av.AttributeId = a.Id
                    WHERE a.[Key] = 'PublicApplicationRoot'
		                    AND a.EntityTypeId IS NULL
		                    AND a.EntityTypeQualifierColumn = ''
		                    AND a.EntityTypeQualifierValue = ''
		                    AND av.[EntityId] IS NULL" );

            UpdateAttributeValues();
            UpdateRegistrationTemplates();
            UpdateDefinedValues();
        }

        private void UpdateAttributeValues()
        {
            var currentValue = @"{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}/assessments?{{ Person.ImpersonationParameter }}";

            var newValue = @"{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}assessments?{{ Person.ImpersonationParameter }}";

            UpdateTableColumn( "AttributeValue", "Value", currentValue, newValue, 
                $"AND EXISTS(SELECT 1 FROM [Attribute] WHERE [AttributeValue].[AttributeId] = [Attribute].[Id] AND [Attribute].[Guid] = '{SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_BODY}')" );
        }


        private void UpdateRegistrationTemplates()
        {
            var currentValue = @"<a href=''{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}''>";
            var newValue = @"<a href=''{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}''>";

            UpdateTableColumn( "RegistrationTemplate", "PaymentReminderEmailTemplate", currentValue, newValue );

            currentValue = @"<a href=""{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}"">";
            newValue = @"<a href=""{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}"">";

            UpdateTableColumn( "RegistrationTemplate", "ReminderEmailTemplate", currentValue, newValue );

            currentValue = @"<a href=''{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}''>";
            newValue = @"<a href=''{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}''>";

            UpdateTableColumn( "RegistrationTemplate", "ReminderEmailTemplate", currentValue, newValue );
        }

        private void UpdateDefinedValues()
        {
            var currentValue = @"<Rock:Image Source=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}/GetImage.ashx?Guid={{ Event.Photo.Guid }}""";
            var newValue = @"<Rock:Image Source=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?Guid={{ Event.Photo.Guid }}""";
            UpdateTableColumn( "DefinedValue", "Description", currentValue, newValue );
        }

        private void UpdateTableColumn( string tableName, string columnName, string currentValue, string newValue, string additionalWhere = "" )
        {
            var normalizedColumn = RockMigrationHelper.NormalizeColumnCRLF( columnName );

            Sql( $@"UPDATE [{tableName}]
                    SET [{columnName}] = REPLACE({normalizedColumn}, '{currentValue}', '{newValue}')
                    WHERE {normalizedColumn} LIKE '%{currentValue}%'
                    {additionalWhere}" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
