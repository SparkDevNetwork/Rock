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
    /// Migrates existing legacy Azure Blob Storage provider configuration to the 
    /// core Rock.Storage.Provider.AzureBlobStorage provider.
    /// </summary>
    public partial class UpdateExistingToCoreAzureBlobStorageProvider : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
/*
    Migrates an existing legacy Azure Blob Storage provider configuration
    (rocks.pillars.AzureStorageProvider.AzureBlobStorage) to the Core
    Rock.Storage.Provider.AzureBlobStorage provider.

    The entire body is gated by a precondition: it only runs when the legacy
    provider is Active AND the Core provider is NOT yet Active. If the legacy
    EntityType does not exist, this is a safe no-op.

    Idempotent: re-running after a successful migration is a no-op because
    the Core provider's Active will be 'True' on the second run.

    NOTE: The legacy EntityType and its AttributeValues are intentionally
    left active and untouched. Existing BinaryFile records that still reference
    the legacy provider may be moved in a separate, future step.
*/

DECLARE @LegacyEntityTypeName    NVARCHAR(200)    = N'rocks.pillars.AzureStorageProvider.AzureBlobStorage';
DECLARE @CoreProviderGuid        UNIQUEIDENTIFIER = '9925a20a-7262-4fc7-b86e-856f6d98be17'; -- Rock.Storage.Provider.AzureBlobStorage

DECLARE @LegacyAzureBlobStorageEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = @LegacyEntityTypeName);
DECLARE @CoreAzureBlobStorageEntityTypeId   INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = @CoreProviderGuid);

-- 'Active' attribute key (defined by the Component base class) for each provider EntityType.
DECLARE @LegacyActiveAttrId INT = (SELECT TOP 1 a.[Id] FROM [Attribute] a WHERE a.[EntityTypeId] = @LegacyAzureBlobStorageEntityTypeId AND a.[Key] = N'Active');
DECLARE @CoreActiveAttrId    INT = (SELECT TOP 1 a.[Id] FROM [Attribute] a WHERE a.[EntityTypeId] = @CoreAzureBlobStorageEntityTypeId    AND a.[Key] = N'Active');

DECLARE @LegacyActive NVARCHAR(MAX) = (
    SELECT TOP 1 av.[Value]
    FROM [AttributeValue] av
    WHERE av.[AttributeId] = @LegacyActiveAttrId AND av.[EntityId] = 0
);

DECLARE @CoreActive NVARCHAR(MAX) = (
    SELECT TOP 1 av.[Value]
    FROM [AttributeValue] av
    WHERE av.[AttributeId] = @CoreActiveAttrId AND av.[EntityId] = 0
);

-- PRECONDITION: legacy provider must exist + be Active='True', AND Core must not already be Active='True'.
IF @LegacyAzureBlobStorageEntityTypeId IS NOT NULL
   AND @CoreAzureBlobStorageEntityTypeId IS NOT NULL
   AND @LegacyActive = N'True'
   AND ( @CoreActive IS NULL OR @CoreActive <> N'True' )
BEGIN

    /* -----------------------------------------------------------------------
        STEP 1: Copy component-level AttributeValues (EntityId = 0) from the
        legacy provider to the Core provider, matched by Attribute.[Key].
        Covers: Active, Order, AccountName, AccountKey, CustomDomain,
        DefaultContainerName (and any other keys that happen to match).
       ----------------------------------------------------------------------- */

    -- UPDATE existing target rows in place.
    UPDATE tgtAV
    SET tgtAV.[Value]                       = srcAV.[Value]
      , tgtAV.[ModifiedDateTime]            = GETDATE()
      , tgtAV.[IsPersistedValueDirty] = 1
    FROM [AttributeValue] tgtAV
    INNER JOIN [Attribute]      tgtA  ON tgtA.[Id] = tgtAV.[AttributeId]
    INNER JOIN [Attribute]      srcA  ON srcA.[Key] = tgtA.[Key] AND srcA.[EntityTypeId] = @LegacyAzureBlobStorageEntityTypeId
    INNER JOIN [AttributeValue] srcAV ON srcAV.[AttributeId] = srcA.[Id] AND srcAV.[EntityId] = 0
    WHERE tgtA.[EntityTypeId] = @CoreAzureBlobStorageEntityTypeId
      AND tgtAV.[EntityId] = 0;

    -- INSERT missing target rows.
    INSERT INTO [AttributeValue]
        ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
    SELECT
        0, tgtA.[Id], 0, srcAV.[Value], NEWID(), GETDATE(), GETDATE(), 1
    FROM [Attribute] tgtA
    INNER JOIN [Attribute]      srcA  ON srcA.[Key] = tgtA.[Key] AND srcA.[EntityTypeId] = @LegacyAzureBlobStorageEntityTypeId
    INNER JOIN [AttributeValue] srcAV ON srcAV.[AttributeId] = srcA.[Id] AND srcAV.[EntityId] = 0
    WHERE tgtA.[EntityTypeId] = @CoreAzureBlobStorageEntityTypeId
      AND NOT EXISTS (
          SELECT 1 FROM [AttributeValue] existing
          WHERE existing.[AttributeId] = tgtA.[Id] AND existing.[EntityId] = 0
      );

    /* -----------------------------------------------------------------------
        STEP 2: Repoint BinaryFileType rows from the legacy EntityType to
        the Core EntityType. Existing BinaryFile records that still point at
        the legacy-stored blobs could be migrated in the future (v20).
       ----------------------------------------------------------------------- */
    UPDATE [BinaryFileType]
    SET [StorageEntityTypeId] = @CoreAzureBlobStorageEntityTypeId
    WHERE [StorageEntityTypeId] = @LegacyAzureBlobStorageEntityTypeId;

    /* -----------------------------------------------------------------------
        STEP 3: Copy per-BinaryFileType qualified AttributeValues from the
        legacy provider's BinaryFileType attributes to the Core provider's
        BinaryFileType attributes. The legacy and Core attribute Keys
        differ, so we map them explicitly by well-known Attribute.[Guid]:

            Legacy 'ContainerName'        (5AA80B71-825C-4757-8B04-AA4C233DC862)
              --> Core 'AzureBlobContainerName'       (5D921DDE-623A-4079-B987-25C74B4CDB7B)

            Legacy 'ContainerFolderPath'  (6465A83B-7A1F-4B90-BA73-862726ED8B41)
              --> Core 'AzureBlobContainerFolderPath' (BA7C28E6-B45E-4983-8A8D-96985E2C4EF4)

        AttributeValue.EntityId = BinaryFileType.Id and is preserved across
        the copy. If a legacy attribute or its values do not exist, that
        portion of the copy is a no-op.
       ----------------------------------------------------------------------- */

    DECLARE @LegacyContainerNameAttrId       INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '5AA80B71-825C-4757-8B04-AA4C233DC862');
    DECLARE @LegacyContainerFolderPathAttrId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '6465A83B-7A1F-4B90-BA73-862726ED8B41');
    DECLARE @CoreContainerNameAttrId         INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '5D921DDE-623A-4079-B987-25C74B4CDB7B');
    DECLARE @CoreContainerFolderPathAttrId   INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'BA7C28E6-B45E-4983-8A8D-96985E2C4EF4');

    -- Copy ContainerName --> AzureBlobContainerName.
    IF @LegacyContainerNameAttrId IS NOT NULL AND @CoreContainerNameAttrId IS NOT NULL
    BEGIN
        -- UPDATE existing target rows in place.
        UPDATE tgtAV
        SET tgtAV.[Value]                       = srcAV.[Value]
          , tgtAV.[ModifiedDateTime]            = GETDATE()
          , tgtAV.[IsPersistedValueDirty] = 1
        FROM [AttributeValue] tgtAV
        INNER JOIN [AttributeValue] srcAV ON srcAV.[AttributeId] = @LegacyContainerNameAttrId
                                         AND srcAV.[EntityId]    = tgtAV.[EntityId]
        WHERE tgtAV.[AttributeId] = @CoreContainerNameAttrId;

        -- INSERT missing target rows.
        INSERT INTO [AttributeValue]
            ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
        SELECT
            0, @CoreContainerNameAttrId, srcAV.[EntityId], srcAV.[Value], NEWID(), GETDATE(), GETDATE(), 1
        FROM [AttributeValue] srcAV
        WHERE srcAV.[AttributeId] = @LegacyContainerNameAttrId
          AND NOT EXISTS (
              SELECT 1 FROM [AttributeValue] existing
              WHERE existing.[AttributeId] = @CoreContainerNameAttrId
                AND existing.[EntityId]    = srcAV.[EntityId]
          );
    END

    -- Copy ContainerFolderPath --> AzureBlobContainerFolderPath.
    IF @LegacyContainerFolderPathAttrId IS NOT NULL AND @CoreContainerFolderPathAttrId IS NOT NULL
    BEGIN
        -- UPDATE existing target rows in place.
        UPDATE tgtAV
        SET tgtAV.[Value]                       = srcAV.[Value]
          , tgtAV.[ModifiedDateTime]            = GETDATE()
          , tgtAV.[IsPersistedValueDirty] = 1
        FROM [AttributeValue] tgtAV
        INNER JOIN [AttributeValue] srcAV ON srcAV.[AttributeId] = @LegacyContainerFolderPathAttrId
                                         AND srcAV.[EntityId]    = tgtAV.[EntityId]
        WHERE tgtAV.[AttributeId] = @CoreContainerFolderPathAttrId;

        -- INSERT missing target rows.
        INSERT INTO [AttributeValue]
            ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
        SELECT
            0, @CoreContainerFolderPathAttrId, srcAV.[EntityId], srcAV.[Value], NEWID(), GETDATE(), GETDATE(), 1
        FROM [AttributeValue] srcAV
        WHERE srcAV.[AttributeId] = @LegacyContainerFolderPathAttrId
          AND NOT EXISTS (
              SELECT 1 FROM [AttributeValue] existing
              WHERE existing.[AttributeId] = @CoreContainerFolderPathAttrId
                AND existing.[EntityId]    = srcAV.[EntityId]
          );
    END

    /* -----------------------------------------------------------------------
        STEP 4: Ensure the Core provider is Active. Step 1 normally copies
        this from the legacy provider, but explicitly UPSERT 'True' for safety.
        This is what makes a successful re-run a no-op (the precondition
        will fail on subsequent runs because Core is now active).
       ----------------------------------------------------------------------- */
    IF @CoreActiveAttrId IS NOT NULL
    BEGIN
        IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @CoreActiveAttrId AND [EntityId] = 0 )
        BEGIN
            UPDATE [AttributeValue]
            SET [Value]            = N'True'
              , [ModifiedDateTime] = GETDATE()
              , [IsPersistedValueDirty] = 1
            WHERE [AttributeId] = @CoreActiveAttrId AND [EntityId] = 0;
        END
        ELSE
        BEGIN
            INSERT INTO [AttributeValue]
                ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
            VALUES
                ( 0, @CoreActiveAttrId, 0, N'True', NEWID(), GETDATE(), GETDATE(), 1 );
        END
    END


    -- NOTE: WE CANNOT DO STEP 5 because existing BinaryFiles would no longer work.

    /* -----------------------------------------------------------------------
        ~~STEP 5: Ensure the Legacy provider is not Active.~~
       ----------------------------------------------------------------------- */
    --IF @LegacyActiveAttrId IS NOT NULL
    --BEGIN
    --    IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @LegacyActiveAttrId AND [EntityId] = 0 )
    --    BEGIN
    --        UPDATE [AttributeValue]
    --        SET [Value]            = N'False'
    --          , [ModifiedDateTime] = GETDATE()
    --          , [IsPersistedValueDirty] = 1
    --        WHERE [AttributeId] = @LegacyActiveAttrId AND [EntityId] = 0;
    --    END
    --    ELSE
    --    BEGIN
    --        INSERT INTO [AttributeValue]
    --            ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [CreatedDateTime], [ModifiedDateTime], [IsPersistedValueDirty] )
    --        VALUES
    --            ( 0, @LegacyActiveAttrId, 0, N'False', NEWID(), GETDATE(), GETDATE(), 1 );
    --    END
    --END

END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // A Down() migration is not safe, here, because BinaryFileType records may now reference the Core provider's EntityTypeId.
        }
    }
}
