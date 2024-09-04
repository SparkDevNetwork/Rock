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
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20240201 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddUpdatePersistedDatasetUp();
            MigratePrayerRequestFieldTypeFromPluginUp();
            RepurposeMultiPersonSelectNoOptionMessageBlockAttributeUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// JDR: Add the UpdatePersistedDataset job to ServiceJobs out of the box
        /// If the job is already there, update the cron expression to run every minute
        /// </summary>
        private void AddUpdatePersistedDatasetUp()
        {
            Sql( $@"
          IF NOT EXISTS (
              SELECT 1
              FROM [ServiceJob]
              WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}'
          )
          BEGIN
              INSERT INTO [ServiceJob] (
                  [IsSystem],
                  [IsActive],
                  [Name],
                  [Description],
                  [Class],
                  [CronExpression],
                  [NotificationStatus],
                  [Guid]
              ) VALUES (
                  1, -- IsSystem
                  1, -- IsActive
                  'Update Persisted Datasets', -- Name
                  'This job will update persisted datasets.', -- Description
                  'Rock.Jobs.UpdatePersistedDatasets', -- Class
                  '0 * * * * ?', -- CronExpression to run every minute
                  1, -- NotificationStatus
                  '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}' -- Guid
              );
          END
          ELSE
          BEGIN
              -- Update the cron expression if the job already exists
              UPDATE [ServiceJob]
              SET [CronExpression] = '0 * * * * ?' -- CronExpression to run every minute
              WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}';
          END" );

        }

        /// <summary>
        /// JPH: Migrate Prayer Request Field Type from plugin to core.
        /// </summary>
        private void MigratePrayerRequestFieldTypeFromPluginUp()
        {
            Sql( $@"
DECLARE @FieldTypeId [int] = (SELECT [Id]
                              FROM [FieldType]
                              WHERE [Guid] = '{SystemGuid.FieldType.PRAYER_REQUEST}'
                                  OR (
                                      [Assembly] = 'org.sparkdevnetwork.PrayerRequestWorkflowAction'
                                      AND [Class] = 'Rock.Field.Types.PrayerRequestFieldType')
                                  );
DECLARE @Now [datetime] = GETDATE();
IF @FieldTypeId IS NULL
BEGIN
    INSERT INTO [FieldType]
    (
        [IsSystem]
        , [Name]
        , [Description]
        , [Assembly]
        , [Class]
        , [Guid]
        , [CreatedDateTime]
        , [ModifiedDateTime]
    )
    VALUES
    (
        1
        , 'Prayer Request'
        , 'A Prayer Request Field'
        , 'Rock'
        , 'Rock.Field.Types.PrayerRequestFieldType'
        , '{SystemGuid.FieldType.PRAYER_REQUEST}'
        , @Now
        , @Now
    );
END
ELSE
BEGIN
    UPDATE [FieldType]
    SET [IsSystem] = 1
        , [Name] = 'Prayer Request'
        , [Description] = 'A Prayer Request Field'
        , [Assembly] = 'Rock'
        , [Guid] = '{SystemGuid.FieldType.PRAYER_REQUEST}'
        , [ModifiedDateTime] = @Now
    WHERE [Id] = @FieldTypeId;
END
DECLARE @EntityTypeId [int] = (SELECT [Id]
                               FROM [EntityType]
                               WHERE [Name] = 'Rock.Workflow.Action.PrayerRequestAdd'
                                   AND [AssemblyName] LIKE '%org.sparkdevnetwork.PrayerRequestWorkflowAction%');
IF @EntityTypeId IS NOT NULL
BEGIN
    UPDATE [EntityType]
    SET [AssemblyName] = 'Rock' -- This will get overwritten with the correct assembly name on Rock startup.
        , [FriendlyName] = 'Prayer Request Add'
        , [Guid] = 'E76463C5-C8CD-40AB-AD5A-0758937CA407'
    WHERE [Id] = @EntityTypeId;
END" );
        }

        /// <summary>
        /// JPH: Repurpose Multi-Person Select block's "No Option Message" attribute.
        /// </summary>
        private void RepurposeMultiPersonSelectNoOptionMessageBlockAttributeUp()
        {
            // The MultiPersonSelect block's "No Option Message" attribute wasn't being used in the code,
            // so we're going to repurpose it for a slightly different usage than what might have been
            // originally intended, to support a new feature request.
            var blockTypeGuid = "92DCF018-F551-4890-8BA1-511D97BF6B8A";

            Sql( $@"
DECLARE @BlockTypeEntityTypeId [int] = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block');
DECLARE @BlockTypeId [int] = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{blockTypeGuid}');
DECLARE @AttributeId [int] = (SELECT [Id] FROM [Attribute]
                              WHERE [Key] = 'NoOptionMessage'
                                  AND [EntityTypeId] = @BlockTypeEntityTypeId
                                  AND [EntityTypeQualifierColumn] = 'BlockTypeId'
                                  AND [EntityTypeQualifierValue] = @BlockTypeId);
-- Set default value to an empty string.
UPDATE [Attribute]
SET [DefaultValue] = ''
    , [DefaultPersistedTextValue] = NULL
    , [DefaultPersistedHtmlValue] = NULL
    , [DefaultPersistedCondensedTextValue] = NULL
    , [DefaultPersistedCondensedHtmlValue] = NULL
    , [IsDefaultPersistedValueDirty] = 1
WHERE [Id] = @AttributeId;
-- Delete any existing attribute values.
DELETE
FROM [AttributeValue]
WHERE [AttributeId] = @AttributeId;" );
        }
    }
}
