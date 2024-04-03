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
    public partial class AddPasswordlessAuthentication : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            PasswordlessAuthentication_UpdateExistingBlockTypes();
            PasswordlessAuthentication_InsertSystemCommunications();
            PasswordlessAuthentication_UpdateExistingSystemCommunications();
            PasswordlessAuthentication_AlterRemoteAuthenticationSession();
            PasswordlessAuthentication_InsertEntityTypeAndAttributes();
            PasswordlessAuthentication_AddOrUpdateExistingSystemSecuritySettings();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //// No need to revert system security settings as it is a JSON object with a few extra properties and will still work if code is reverted.
            PasswordlessAuthentication_DeleteEntityTypeAndAttributes();
            PasswordlessAuthentication_RevertChangesToRemoteAuthenticationSession();
            PasswordlessAuthentication_RevertChangesToExistingSystemCommunications();
            PasswordlessAuthentication_DeleteSystemCommunications();
            PasswordlessAuthentication_RevertChangesToExistingBlockTypes();
            PasswordlessAuthentication_DeleteBlockTypesAndAttributes();
        }

        /// <summary>
        /// JMH: Updates existing block types for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_UpdateExistingBlockTypes()
        {
            Sql( "UPDATE BlockType SET [Path] = '~/Blocks/Security/Login.ascx', [Name] = 'Login (Legacy)' WHERE [Guid] = '7B83D513-1178-429E-93FF-E76430E038E4'" );
            Sql( "UPDATE BlockType SET [Path] = '~/Blocks/Security/AccountEntryLegacy.ascx', [Name] = 'Account Entry (Legacy)' WHERE [Guid] = '99362B60-71A5-44C6-BCFE-DDA9B00CC7F3'" );
        }

        /// <summary>
        /// JMH: Inserts system communications for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_InsertSystemCommunications()
        {
            RockMigrationHelper.UpdateSystemCommunication( "Security",
                "Passwordless Login Confirmation",
                "", // from
                "", // fromName
                "", // to
                "", // cc
                "", // bcc
                "Your {% if IsNewPerson %}account creation{% else %}sign in{% endif %} request for {{ 'Global' | Attribute:'OrganizationName' }}", // subject
                                                                                                                                                   // body
                @"{{ 'Global' | Attribute:'EmailHeader' }}

<h1>Welcome to {{ 'Global' | Attribute:'OrganizationName' }}</h1>

<p>Click the button below to finish {% if IsNewPerson %}creating your account{% else %}signing in{% endif %}. Your link expires in {{ LinkExpiration }}.</p>

<p><!--[if mso]>
<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ Link }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#269abc"" fillcolor=""#31b0d5"">
<w:anchorlock/>
<center style=""color:#ffffff;font-family:sans-serif;font-size:14px;font-weight:normal;"">Continue</center>
</v:roundrect>
<![endif]-->
<a href=""{{ Link }}"" style=""background-color:#31b0d5;border:1px solid #269abc;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:14px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">Continue</a>
</p>

<p>If you have trouble with the button above please use the link below:</p>
<p>
<a href=""{{ Link }}"">{{ Link }}</a>
</p>

{{ 'Global' | Attribute:'EmailFooter' }}",
                SystemGuid.SystemCommunication.SECURITY_CONFIRM_LOGIN_PASSWORDLESS,
                smsMessage: @"{{ Code }} is your {{ 'Global' | Attribute:'OrganizationName' }} verification code." );

            RockMigrationHelper.UpdateSystemCommunication( "Security",
                "Account Confirmation (Passwordless)",
                "", // from
                "", // fromName
                "", // to
                "", // cc
                "", // bcc
                "Account Confirmation", // subject
                                        // body
                @"{{ 'Global' | Attribute:'EmailHeader' }}

{{ Person.FirstName }},<br/><br/>

Your account was recently updated and needs to be confirmed for security purposes. Please use the code below to verify that this was you.<br/><br/>

{{ Code }}<br/><br/>

If you didn't make this request, don't worry. You can safely ignore this email.<br/><br/>

Thank you,<br/>
{{ 'Global' | Attribute:'OrganizationName' }}

{{ 'Global' | Attribute:'EmailFooter' }}",
                SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT_PASSWORDLESS );
        }

        /// <summary>
        /// JMH: Updates existing system communications for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_UpdateExistingSystemCommunications()
        {
            Sql( $"UPDATE SystemCommunication SET Body = REPLACE( Body, '{{{{ User.UserName }}}}', '{{% if User.IsPasswordless %}}***{{% else %}}{{{{ User.UserName }}}}{{% endif %}}' ) WHERE Guid = '{SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT}'" );
            Sql( $"UPDATE SystemCommunication SET Body = REPLACE( Body, '{{{{ User.UserName }}}}', '{{% if User.IsPasswordless %}}(email or phone){{% else %}}{{{{ User.UserName }}}}{{% endif %}}' ) WHERE Guid = '{SystemGuid.SystemCommunication.SECURITY_ACCOUNT_CREATED}'" );
        }

        /// <summary>
        /// JMH: Alters the RemoteAuthenticationSession table for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_AlterRemoteAuthenticationSession()
        {
            // Unique identifiers for Passwordless Authentication can be either email addresses or phone numbers.
            // Since email addresses are larger between the two,
            // and email fields in Rock are 75 characters,
            // we need the new column size to be email length (75).
            Sql( "ALTER TABLE RemoteAuthenticationSession ALTER COLUMN DeviceUniqueIdentifier NVARCHAR (75)" );
        }

        /// <summary>
        /// JMH: Inserts entity type and attributes for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_InsertEntityTypeAndAttributes()
        {
            const string attributeGuid = "7AEC7325-6377-481D-914F-1E02F92B28B9";

            RockMigrationHelper.UpdateEntityType(
                "Rock.Security.Authentication.PasswordlessAuthentication",
                "Passwordless Authentication",
                "Rock.Security.Authentication.PasswordlessAuthentication, Rock, Version=1.15.0.8, Culture=neutral, PublicKeyToken=null",
                false, // isEntity
                true,  // isSecure
                SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS );

            RockMigrationHelper.AddOrUpdateEntityAttribute(
                "Rock.Security.Authentication.PasswordlessAuthentication",
                SystemGuid.FieldType.BOOLEAN,
                null,                      // entityTypeQualifierColumn
                null,                      // entityTypeQualifierValue
                "Active",                  // name
                null,                      // abbreviatedName
                "Should Service be used?", // description
                0,                         // order
                "False",                   // defaultValue
                attributeGuid,
                "Active" );                // key

            Sql( $"UPDATE [Attribute] SET IsSystem = 1, IsDefaultPersistedValueDirty = 1 WHERE Guid = '{attributeGuid}'" );

            RockMigrationHelper.AddAttributeValue( attributeGuid, 0, "True", "FC087CB5-9E30-4606-8BB2-1BBCCF827202" );
        }

        /// <summary>
        /// JMH: Adds or updates existing system security settings for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_AddOrUpdateExistingSystemSecuritySettings()
        {
            var attributeIdResult = SqlScalar( $@"
SELECT TOP 1 [Id]
  FROM [Attribute]
 WHERE [EntityTypeId] IS NULL
       AND [EntityTypeQualifierColumn] = '{Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER}'
       AND [Key] = '{Rock.SystemKey.SystemSetting.ROCK_SECURITY_SETTINGS}'
       AND RTRIM(DefaultValue) LIKE '%}}'" );

            if ( attributeIdResult is int attributeId && attributeId > 0 )
            {
                // Add new JSON properties to the existing attribute JSON content one property at a time to ensure each is added.
                Sql( $@"
UPDATE Attribute
    SET DefaultValue=LEFT(RTRIM(DefaultValue), LEN(RTRIM(DefaultValue)) - 1) + ',""{nameof( Security.SecuritySettings.DisablePasswordlessSignInForAccountProtectionProfiles )}"":[{( int ) Utility.Enums.AccountProtectionProfile.Extreme}]}}'
    FROM Attribute
    WHERE Id = {attributeId}
            AND RTRIM(DefaultValue) LIKE '%}}'
            AND NOT (DefaultValue LIKE '%{nameof( Security.SecuritySettings.DisablePasswordlessSignInForAccountProtectionProfiles )}%')

UPDATE Attribute
    SET DefaultValue=LEFT(RTRIM(DefaultValue), LEN(RTRIM(DefaultValue)) - 1) + ',""{nameof( Security.SecuritySettings.PasswordlessConfirmationCommunicationTemplateGuid )}"":""{Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_LOGIN_PASSWORDLESS}""}}'
    FROM Attribute
    WHERE Id = {attributeId} 
            AND RTRIM(DefaultValue) LIKE '%}}'
            AND NOT (DefaultValue LIKE '%{nameof( Security.SecuritySettings.PasswordlessConfirmationCommunicationTemplateGuid )}%')

UPDATE Attribute
    SET DefaultValue=LEFT(RTRIM(DefaultValue), LEN(RTRIM(DefaultValue)) - 1) + ',""{nameof( Security.SecuritySettings.PasswordlessSignInDailyIpThrottle )}"":{Security.SecuritySettings.PasswordlessSignInDailyIpThrottleDefaultValue}}}'
    FROM Attribute
    WHERE Id = {attributeId} 
            AND RTRIM(DefaultValue) LIKE '%}}'
            AND NOT (DefaultValue LIKE '%{nameof( Security.SecuritySettings.PasswordlessSignInDailyIpThrottle )}%')

UPDATE Attribute
    SET DefaultValue=LEFT(RTRIM(DefaultValue), LEN(RTRIM(DefaultValue)) - 1) + ',""{nameof( Security.SecuritySettings.PasswordlessSignInSessionDuration )}"":{Security.SecuritySettings.PasswordlessSignInSessionDurationDefaultValue}}}'
    FROM Attribute
    WHERE Id = {attributeId} 
            AND RTRIM(DefaultValue) LIKE '%}}'
            AND NOT (DefaultValue LIKE '%{nameof( Security.SecuritySettings.PasswordlessSignInSessionDuration )}%')" );
            }
            else
            {
                // The SecuritySettings attribute is missing or has no JSON content.
                // The SecuritySettingsService will create the default settings when it is accessed for the first time.
            }
        }

        /// <summary>
        /// JMH: Deletes entity type and attributes that were added for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_DeleteEntityTypeAndAttributes()
        {
            // Delete dependencies before deleting the Passwordless Authentication EntityType.
            Sql( $@"
DECLARE @EntityTypeGuid uniqueidentifier = '{SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS}'
DECLARE @EntityTypeId int

SELECT @EntityTypeId = (SELECT Id FROM EntityType WHERE [Guid] = @EntityTypeGuid)

DELETE FROM AttributeValue WHERE AttributeId IN (SELECT Id FROM Attribute WHERE EntityTypeId = @EntityTypeId)
DELETE FROM Attribute WHERE EntityTypeId = @EntityTypeId

DELETE FROM UserLogin WHERE EntityTypeId = @EntityTypeId" );

            // Delete Passwordless Authentication EntityType.
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.AUTHENTICATION_PASSWORDLESS );
        }

        /// <summary>
        /// JMH: Reverts changes to the RemoteAuthenticationSession table that were made for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_RevertChangesToRemoteAuthenticationSession()
        {
            // Truncate any RemoteAuthenticationSession.DeviceUniqueIdentifier data that may exceed the previous column size.
            Sql( @"
UPDATE RemoteAuthenticationSession
SET DeviceUniqueIdentifier = LEFT(DeviceUniqueIdentifier, 45)
-- 45 nvarchar characters * 2 bytes per nvarchar character
WHERE DATALENGTH(DeviceUniqueIdentifier) > 90" );

            // Restore the column size.
            Sql( "ALTER TABLE RemoteAuthenticationSession ALTER COLUMN DeviceUniqueIdentifier NVARCHAR (45)" );
        }

        /// <summary>
        /// JMH: Reverts changes to existing system communications that were made for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_RevertChangesToExistingSystemCommunications()
        {
            Sql( $"UPDATE SystemCommunication SET Body = REPLACE( Body, '{{% if User.IsPasswordless %}}***{{% else %}}{{{{ User.UserName }}}}{{% endif %}}', '{{{{ User.UserName }}}}' ) WHERE Guid = '{SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT}'" );
            Sql( $"UPDATE SystemCommunication SET Body = REPLACE( Body, '{{% if User.IsPasswordless %}}(email or phone){{% else %}}{{{{ User.UserName }}}}{{% endif %}}', '{{{{ User.UserName }}}}' ) WHERE Guid = '{SystemGuid.SystemCommunication.SECURITY_ACCOUNT_CREATED}'" );
        }

        /// <summary>
        /// JMH: Deletes system communications that were added for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_DeleteSystemCommunications()
        {
            RockMigrationHelper.DeleteSystemCommunication( SystemGuid.SystemCommunication.SECURITY_CONFIRM_LOGIN_PASSWORDLESS );
            RockMigrationHelper.DeleteSystemCommunication( SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT_PASSWORDLESS );
        }

        /// <summary>
        /// JMH: Reverts changes to existing block types that were made for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_RevertChangesToExistingBlockTypes()
        {
            Sql( "UPDATE BlockType SET [Path] = '~/Blocks/Security/Login.ascx', [Name] = 'Login' WHERE [Guid] = '7B83D513-1178-429E-93FF-E76430E038E4'" );
            Sql( "UPDATE BlockType SET [Path] = '~/Blocks/Security/AccountEntry.ascx', [Name] = 'Account Entry' WHERE [Guid] = '99362B60-71A5-44C6-BCFE-DDA9B00CC7F3'" );
        }

        /// <summary>
        /// JMH: Reverts changes to existing block types that were made for Passwordless Authentication.
        /// </summary>
        private void PasswordlessAuthentication_DeleteBlockTypesAndAttributes()
        {
            // Obsidian AccountEntry block type.
            Sql( "DELETE BlockType WHERE [Guid] = 'E5C34503-DDAD-4881-8463-0E1E20B1675D'" );

            // Obsidian Login block type.
            Sql( "DELETE BlockType WHERE [Guid] = '5437C991-536D-4D9C-BE58-CBDB59D1BBB3'" );
        }
    }
}
