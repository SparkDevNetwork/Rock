﻿// <copyright>
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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 192, "1.14.3" )]
    public class MailgunCopyApiKeyToHttpWebhookSigningKey : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CopyApiKeyToHttpWebhookSigningKey();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Copies the Mailgun "APIKey" AttributeValue (if present) to a
        /// newly-created "HTTPWebhookSigningKey" AttributeValue.
        /// </summary>
        private void CopyApiKeyToHttpWebhookSigningKey()
        {
            Sql( @"
DECLARE @EntityTypeId [int] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.MailgunHttp');
DECLARE @FieldTypeId [int] = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA'); -- Text

-- Attempt to find the existing APIKey Attribute and AttributeValue.
DECLARE @ApiKeyAttrId [int] = (
    SELECT TOP 1 [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @EntityTypeId
        AND [FieldTypeId] = @FieldTypeId
        AND [Key] = 'APIKey'
);

DECLARE @ApiKeyAttrValue [nvarchar](max) = (
    SELECT TOP 1 [Value]
    FROM [AttributeValue]
    WHERE [AttributeId] = @ApiKeyAttrId
);

-- Only attempt to set the HTTPWebhookSigningKey value if the APIKey value is actually set.
IF @ApiKeyAttrValue IS NOT NULL AND @ApiKeyAttrValue <> ''
BEGIN
    -- Ensure the HTTPWebhookSigningKey Attribute exists; create it if not.
    DECLARE @WebhookKeyAttrKey [nvarchar](21) = 'HTTPWebhookSigningKey';
    DECLARE @WebhookKeyAttrId [int] = (
        SELECT [Id]
        FROM [Attribute]
        WHERE [EntityTypeId] = @EntityTypeId
            AND [FieldTypeId] = @FieldTypeId
            AND [Key] = @WebhookKeyAttrKey
    );

    DECLARE @Now [datetime] = (SELECT GETDATE());

    IF @WebhookKeyAttrId IS NULL
    BEGIN
        INSERT INTO [Attribute]
        (
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
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
            , [IconCssClass]
            , [AbbreviatedName]
            , [IsDefaultPersistedValueDirty]
        )
        VALUES
        (
            0
            , @FieldTypeId
            , @EntityTypeId
            , ''
            , ''
            , @WebhookKeyAttrKey
            , 'HTTP Webhook Signing Key'
            , 'The HTTP Webhook Signing Key provided by Mailgun. Newly-created Mailgun accounts will have separate API and Webhook keys.'
            , 4
            , 0
            , ''
            , 0
            , 1
            , NEWID()
            , @Now
            , @Now
            , ''
            , 'HTTP Webhook Signing Key'
            , 0
        );

        SET @WebhookKeyAttrId = (SELECT @@IDENTITY);
    END

    -- If the HTTPWebhookSigningKey happens to already have a value, don't overwrite it.
    DECLARE @WebhookKeyAttrValueId [int] = (
        SELECT TOP 1 [Id]
        FROM [AttributeValue]
        WHERE [AttributeId] = @WebhookKeyAttrId
    );

    DECLARE @WebhookKeyAttrValue [nvarchar](max) = (SELECT [Value] FROM [AttributeValue] WHERE [Id] = @WebhookKeyAttrValueId);

    IF @WebhookKeyAttrValueId IS NULL
    BEGIN
        -- AttributeValue didn't already exist; create it.
        INSERT INTO [AttributeValue]
        (
            [IsSystem]
            , [AttributeId]
            , [EntityId]
            , [Value]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
        )
        VALUES
        (
            0
            , @WebhookKeyAttrId
            , 0
            , @ApiKeyAttrValue
            , NEWID()
            , @Now
            , @Now
        );
    END
    ELSE IF @WebhookKeyAttrValue IS NULL OR @WebhookKeyAttrValue = ''
    BEGIN
        -- AttributeValue already existed without a value; update it.
        UPDATE [AttributeValue]
        SET [Value] = @ApiKeyAttrValue
            , [ModifiedDateTime]= @Now
        WHERE [Id] = @WebhookKeyAttrValueId;
    END
END" );
        }
    }
}
