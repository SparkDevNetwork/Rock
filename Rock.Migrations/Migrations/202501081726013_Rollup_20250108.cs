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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250108 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateVolunteerGenerosityCampusNameUp();
            ChopBlocksUp();
            RemoveNullAttributeAuthRoleUp();
            UpdateAdaptiveMessageAdaptationAttributeUp();
            RemoveProgramCompletionDescription();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveNullAttributeAuthRoleDown();
        }

        #region SC: Update Volunteer Generosity Campus Name (16.8)

        private void UpdateVolunteerGenerosityCampusNameUp()
        {
            string newBuildScript = @"//- Retrieve the base URL for linking photos from a global attribute 
{% assign publicApplicationRoot = 'Global' | Attribute:'PublicApplicationRoot' %}

{% sql %}
DECLARE @NumberOfDays INT = 365;
DECLARE @NumberOfMonths INT = 13;
DECLARE @ServingAreaDefinedValueGuid UNIQUEIDENTIFIER = '36a554ce-7815-41b9-a435-93f3d52a2828';
DECLARE @ActiveRecordStatusValueId INT = (SELECT Id FROM DefinedValue WHERE Guid = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E');
DECLARE @ConnectionStatusDefinedTypeId INT = (SELECT Id FROM DefinedType WHERE [Guid] = '2e6540ea-63f0-40fe-be50-f2a84735e600');
DECLARE @StartDateKey INT = (SELECT TOP 1 [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = CAST(DATEADD(DAY, -@NumberOfDays, GETDATE()) AS DATE));
DECLARE @CurrentMonth DATE = DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0);
DECLARE @StartingDateKeyForGiving INT = (SELECT [DateKey] FROM [AnalyticsSourceDate] WHERE [Date] = DATEADD(MONTH, -@NumberOfMonths, @CurrentMonth));

;WITH CTE_Giving AS (
    SELECT
        p.[GivingId],
        asd.[DateKey],
        SUM(ftd.[Amount]) AS TotalAmount
    FROM
        [Person] p
        INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
        INNER JOIN [FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
        INNER JOIN [FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
        INNER JOIN [FinancialAccount] fa ON fa.[Id] = ftd.[AccountId]
        INNER JOIN [AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
    WHERE
        fa.[IsTaxDeductible] = 1 AND ft.[TransactionDateKey] >= @StartingDateKeyForGiving
    GROUP BY
        p.[GivingId], asd.[DateKey]
    HAVING
        SUM(ftd.[Amount]) > 0
),
CTE_GivingAggregated AS (
    SELECT
        GD.[GivingId],
        STRING_AGG(CAST(GD.[DateKey] AS VARCHAR(8)), '|') AS DonationDateKeys
    FROM
        CTE_Giving GD
    GROUP BY
        GD.[GivingId]
),
CTE_CampusShortCode AS (
    SELECT
        g.[Id] AS GroupId,
        CASE WHEN c.[ShortCode] IS NOT NULL AND c.[ShortCode] != '' THEN c.[ShortCode] ELSE c.[Name] END AS CampusShortCode,
		c.[Name] AS CampusName
    FROM
        [Group] g
        LEFT JOIN [Campus] c ON c.[Id] = g.[CampusId]
)
SELECT DISTINCT
    p.[Id] AS PersonId,
    CONCAT(CAST(p.[Id] AS NVARCHAR(12)), '-', CAST(g.[Id] AS NVARCHAR(12))) AS PersonGroupKey,
    p.[LastName],
    p.[NickName],
    p.[PhotoId],
    p.[GivingId],
    p.[Gender],
    p.[Age],
    p.[AgeClassification],
    g.[Id] AS GroupId,
    g.[Name] AS GroupName,
    csc.CampusShortCode,
    csc.CampusName,
    MAX(ao.[OccurrenceDate]) AS LastAttendanceDate,
    dvcs.[Value] AS ConnectionStatus,
    CAST(CASE WHEN p.[RecordStatusValueId] = @ActiveRecordStatusValueId AND gm.[IsArchived] = 0 THEN 1 ELSE 0 END AS BIT) AS IsActive,
    GR.DonationDateKeys
FROM
    [Person] p
    INNER JOIN [PersonAlias] pa ON pa.[PersonId] = p.[Id]
    INNER JOIN [Attendance] a ON a.[PersonAliasId] = pa.[Id]
    INNER JOIN [AttendanceOccurrence] ao ON ao.[Id] = a.[OccurrenceId]
    INNER JOIN [Group] g ON g.[Id] = ao.[GroupId]
    INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
    INNER JOIN [DefinedValue] dvp ON dvp.[Id] = gt.[GroupTypePurposeValueId] AND dvp.[Guid] = @ServingAreaDefinedValueGuid
    LEFT JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id] AND gm.[GroupId] = g.[Id]
    LEFT JOIN CTE_CampusShortCode csc ON csc.GroupId = g.[Id]
    LEFT JOIN [DefinedValue] dvcs ON dvcs.[Id] = p.[ConnectionStatusValueId] AND dvcs.[DefinedTypeId] = @ConnectionStatusDefinedTypeId
    LEFT JOIN CTE_GivingAggregated GR ON p.[GivingId] = GR.GivingId
WHERE
    ao.[OccurrenceDateKey] >= @StartDateKey AND a.[DidAttend] = 1
GROUP BY
    p.[Id], p.[LastName], p.[NickName], p.[PhotoId], p.[Gender], p.[Age], p.[AgeClassification], p.[GivingId], g.[Id], g.[Name], csc.[CampusShortCode], csc.[CampusName], dvcs.[Value], p.[RecordStatusValueId], gm.[IsArchived], GR.[DonationDateKeys];

{% endsql %}
{
    ""PeopleData"": [
    {% for result in results %}
        {% if forloop.first != true %},{% endif %}
        {
            ""PersonGroupKey"": {{ result.PersonGroupKey | ToJSON }},
            ""PersonId"": {{ result.PersonId }},
            ""LastName"": {{ result.LastName | ToJSON }},
            ""NickName"": {{ result.NickName | ToJSON }},
            ""Gender"": {{ result.Gender | ToJSON }},
            ""Age"": {{ result.Age | ToJSON }},
            ""AgeClassification"": {{ result.AgeClassification | ToJSON }},
            ""PhotoId"": {{ result.PhotoId | ToJSON }},
            ""GivingId"": {{ result.GivingId | ToJSON }},
            ""LastAttendanceDate"": ""{{ result.LastAttendanceDate | Date: 'yyyy-MM-dd' }}"",
            ""GroupId"": {{ result.GroupId }},
            ""GroupName"": {{ result.GroupName | ToJSON }},
            ""CampusShortCode"": {{ result.CampusShortCode | ToJSON }},
            ""CampusName"": {{ result.CampusName | ToJSON }},
            ""ConnectionStatus"": {{ result.ConnectionStatus | ToJSON }},
            ""IsActive"": {{ result.IsActive }},
            ""DonationDateKeys"": {% if result.DonationDateKeys != null %}{{ result.DonationDateKeys | ToJSON}}{% else %}null{% endif %}
        }
    {% endfor %}
    ]
}
";

            Sql( $@"UPDATE [PersistedDataset]
           SET [BuildScript] = '{newBuildScript.Replace( "'", "''" )}'
           WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );

            Sql( $@"UPDATE [PersistedDataset]
               SET [ResultData] = null
               WHERE [Guid] = '10539E72-B5D3-48E2-B9C6-DB43AFDAD55F'" );
        }

        #endregion

        #region KH: Register block attributes for chop job in v17.0.35

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv17();
        }

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationEntry
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationEntry", "Communication Entry", "Rock.Blocks.Communication.CommunicationEntry, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "26C0C9A1-1383-48D5-A062-E05622A1CBF2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.BinaryFileTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.BinaryFileTypeDetail", "Binary File Type Detail", "Rock.Blocks.Core.BinaryFileTypeDetail, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "B2C1F7F4-4810-4B34-9FB6-9E6D6DEBE4C9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ScheduledJobHistoryList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ScheduledJobHistoryList", "Scheduled Job History List", "Rock.Blocks.Core.ScheduledJobHistoryList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "4B46834F-C9D3-43F3-9DE2-8990D3A232C2" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignatureDocumentDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignatureDocumentDetail", "Signature Document Detail", "Rock.Blocks.Core.SignatureDocumentDetail, Rock.Blocks, Version=1.17.0.33, Culture=neutral, PublicKeyToken=null", false, false, "BCA3D113-8A98-4757-8471-A737011226A9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignatureDocumentList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignatureDocumentList", "Signature Document List", "Rock.Blocks.Core.SignatureDocumentList, Rock.Blocks, Version=1.17.0.33, Culture=neutral, PublicKeyToken=null", false, false, "B4526EB4-3CA4-47BE-B686-4B9FBEE2BF4D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignatureDocumentTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignatureDocumentTemplateDetail", "Signature Document Template Detail", "Rock.Blocks.Core.SignatureDocumentTemplateDetail, Rock.Blocks, Version=1.16.7.5, Culture=neutral, PublicKeyToken=null", false, false, "525B6687-964E-4051-94A5-4B20D4575041" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.SignatureDocumentTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.SignatureDocumentTemplateList", "Signature Document Template List", "Rock.Blocks.Core.SignatureDocumentTemplateList, Rock.Blocks, Version=1.17.0.32, Culture=neutral, PublicKeyToken=null", false, false, "8FAE9715-89F1-4FAA-A35F-18CB55E269C0" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.BadgeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.BadgeDetail", "Badge Detail", "Rock.Blocks.Crm.BadgeDetail, Rock.Blocks, Version=1.16.7.5, Culture=neutral, PublicKeyToken=null", false, false, "5B57BD74-416D-4FD0-A36B-C74955F4C691" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Prayer.PrayerRequestDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Prayer.PrayerRequestDetail", "Prayer Request Detail", "Rock.Blocks.Prayer.PrayerRequestDetail, Rock.Blocks, Version=1.16.7.5, Culture=neutral, PublicKeyToken=null", false, false, "D1E21128-C831-4535-B8DF-0EC928DCBBA4" );

            // Add/Update Obsidian Block Type
            //   Name:Badge Detail
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.BadgeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Badge Detail", "Displays the details of a particular badge.", "Rock.Blocks.Crm.BadgeDetail", "CRM", "5BD4CD27-C1C1-4E12-8756-9C93E4EDB28E" );

            // Add/Update Obsidian Block Type
            //   Name:Binary File Type Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.BinaryFileTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Binary File Type Detail", "Displays all details of a binary file type.", "Rock.Blocks.Core.BinaryFileTypeDetail", "Core", "DABF690B-BE17-4821-A13E-44C7C8D587CD" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Entry
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationEntry
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Entry", "Used for creating and sending a new communications such as email, SMS, etc. to recipients.", "Rock.Blocks.Communication.CommunicationEntry", "Communication", "F6A780EB-66A7-475D-A42E-3C29AD5A89D3" );

            // Add/Update Obsidian Block Type
            //   Name:Prayer Request Detail
            //   Category:Prayer
            //   EntityType:Rock.Blocks.Prayer.PrayerRequestDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Prayer Request Detail", "Displays the details of a particular prayer request.", "Rock.Blocks.Prayer.PrayerRequestDetail", "Prayer", "E120F06F-6DB7-464A-A797-C3C90B92EF40" );

            // Add/Update Obsidian Block Type
            //   Name:Scheduled Job History
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ScheduledJobHistoryList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Scheduled Job History", "Lists all scheduled job's History.", "Rock.Blocks.Core.ScheduledJobHistoryList", "Core", "2306068D-3551-4C10-8DB8-133C030FA4FA" );

            // Add/Update Obsidian Block Type
            //   Name:Signature Document Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignatureDocumentDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signature Document Detail", "Displays the details of a given signature document.", "Rock.Blocks.Core.SignatureDocumentDetail", "Core", "B80E8563-41F2-4528-81E5-C62CF1ECE9DE" );

            // Add/Update Obsidian Block Type
            //   Name:Signature Document List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignatureDocumentList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signature Document List", "Block for viewing values for a signature document type.", "Rock.Blocks.Core.SignatureDocumentList", "Core", "6076609B-D4D2-4825-8BB2-8681E99C59F2" );

            // Add/Update Obsidian Block Type
            //   Name:Signature Document Template Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignatureDocumentTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signature Document Template Detail", "Displays the details of a particular signature document template.", "Rock.Blocks.Core.SignatureDocumentTemplateDetail", "Core", "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5" );

            // Add/Update Obsidian Block Type
            //   Name:Signature Document Template List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.SignatureDocumentTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Signature Document Template List", "Lists all the signature document templates and allows for managing them.", "Rock.Blocks.Core.SignatureDocumentTemplateList", "Core", "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Allow CC/Bcc
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow CC/Bcc", "AllowCcBcc", "Allow CC/Bcc", @"Allow CC and BCC addresses to be entered for email communications?", 8, @"False", "BA82B409-2F29-4447-9910-45B03BEF5FA5" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Allowed SMS Numbers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "B8C35BA7-85E9-4512-B99C-12DE697DE14E", "Allowed SMS Numbers", "AllowedSMSNumbers", "Allowed SMS Numbers", @"Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included).", 10, @"", "FCF43ED8-A2B8-426F-A9CE-5F44A4F189E7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Attachment Binary File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Attachment Binary File Type", "AttachmentBinaryFileType", "Attachment Binary File Type", @"The FileType to use for files that are attached to an sms or email communication.", 12, @"10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C", "99D2674D-803F-45FE-B082-F604104ABD8D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Default As Bulk
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default As Bulk", "DefaultAsBulk", "Default As Bulk", @"Should new entries be flagged as bulk communication by default?", 13, @"False", "3BBCB9A5-523D-4DB7-BACF-693A17EAEF94" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Default Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "C3B37465-DCAF-4C8C-930C-9A9B5D066CA9", "Default Template", "DefaultTemplate", "Default Template", @"The default template to use for a new communication. (Note: This will only be used if the template is for the same medium as the communication.)", 4, @"", "8B9A5D60-A7DF-4E5F-813E-AF1DBD0A58C2" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Document Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Document Root Folder", "DocumentRootFolder", "Document Root Folder", @"The folder to use as the root when browsing or uploading documents.", 16, @"~/Content", "2796C859-AC47-40FA-B698-7DB0A8B25255" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Lava", "EnableLava", "Enable Lava", @"When enabled, allows lava in the message. When disabled, lava is removed from the message without resolving it.", 0, @"False", "C1A95B9F-9003-4DD3-9BA6-AE51FC05BAF7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Person Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Parameter", "EnablePersonParameter", "Enable Person Parameter", @"When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.", 2, @"False", "5C95D6FF-9736-4FBD-87BF-7CF739610C34" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this block if Enable Lava is checked.", 1, @"", "C19DF45C-152A-4FF3-B784-A610B7878398" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Image Root Folder
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Image Root Folder", "ImageRootFolder", "Image Root Folder", @"The folder to use as the root when browsing or uploading images.", 17, @"~/Content", "7FF5F073-1F7A-48A6-9511-75F7D8463028" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Maximum Recipients
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Recipients", "MaximumRecipients", "Maximum Recipients", @"The maximum number of recipients allowed before communication will need to be approved.", 5, @"0", "C369DFC4-CFFA-4183-B7F2-090D2C7E604A" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Mediums
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "039E2E97-3682-4B29-8748-7132287A2059", "Mediums", "Mediums", "Mediums", @"The Mediums that should be available to user to send through (If none are selected, all active mediums will be available).", 3, @"", "11290B87-BFDF-4A1B-9976-E16DB1AA9132" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mode", "Mode", "Mode", @"The mode to use ('Simple' mode will prevent users from searching/adding new people to communication).", 7, @"Full", "235CEBCB-8216-452C-A50C-C1EDA8F52C80" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Send When Approved
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send When Approved", "SendWhenApproved", "Send When Approved", @"Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?", 6, @"True", "57A5010D-69F3-47D7-9FD0-B6510F17958C" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Additional Email Recipients
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Additional Email Recipients", "ShowAdditionalEmailRecipients", "Show Additional Email Recipients", @"Allow additional email recipients to be entered for email communications?", 15, @"False", "71EF060D-E70B-4733-822C-89D3AE6FD069" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Attachment Uploader
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Attachment Uploader", "ShowAttachmentUploader", "Show Attachment Uploader", @"Should the attachment uploader be shown for email communications?", 9, @"True", "BCB16360-06CF-416E-A91C-E197FC4F4240" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Duplicate Prevention Option
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Duplicate Prevention Option", "ShowDuplicatePreventionOption", "Show Duplicate Prevention Option", @"Set this to true to show an option to prevent communications from being sent to people with the same email/SMS addresses. Typically, in Rock you’d want to send two emails as each will be personalized to the individual.", 16, @"False", "75545130-5EAA-4619-9D2D-CBE33FC4EA9D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Show Email Metrics Reminder Options
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email Metrics Reminder Options", "ShowEmailMetricsReminderOptions", "Show Email Metrics Reminder Options", @"Should the email metrics reminder options be shown after a communication is sent?", 14, @"False", "AF5AD05E-A7A9-4784-B88C-FA82A88E8A65" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Simple Communications Are Bulk
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Simple Communications Are Bulk", "IsBulk", "Simple Communications Are Bulk", @"Should simple mode communications be sent as a bulk communication?", 11, @"True", "06B03251-CD7B-4E8E-8B72-B5D484DAE6E1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: User Specific Folders
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "User Specific Folders", "UserSpecificFolders", "User Specific Folders", @"Should the root folders be specific to current user?", 18, @"False", "CA6F5B1C-8385-44D4-BED4-0D3710D0FDF7" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Default Allow Comments Checked
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Allow Comments Checked", "DefaultAllowCommentsChecked", "Default Allow Comments Checked", @"If true, the Allow Comments checkbox will be pre-checked for all new requests by default.", 5, @"True", "3529583E-1E1A-4A23-97F9-00154CF2223B" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"If a category is not selected, choose a default category to use for all new prayer requests.", 1, @"4B2D88F5-6E45-4B4B-8776-11118C8E8269", "A1DB8ABB-F749-4E7E-8454-55CDF6776FEB" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default", 4, @"False", "D4773CA1-BA82-4D73-856F-CCE5099387A4" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Expires After (days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (days)", "ExpireDays", "Expires After (days)", @"Default number of days until the request will expire.", 0, @"14", "5F0853C9-BF37-4CDC-B1B1-3A3B1802B720" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 6, @"False", "90457B3A-BDBD-44FF-B1EC-778F3B3DF564" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered", 3, @"True", "2EB1BB05-8ABC-445E-8934-C233396D319D" );

            // Attribute for BlockType
            //   BlockType: Prayer Request Detail
            //   Category: Prayer
            //   Attribute: Set Current Person To Requester
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E120F06F-6DB7-464A-A797-C3C90B92EF40", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Current Person To Requester", "SetCurrentPersonToRequester", "Set Current Person To Requester", @"Will set the current person as the requester. This is useful in self-entry situations.", 2, @"False", "48887E66-3791-4A4A-9660-571072704C07" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job History
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2306068D-3551-4C10-8DB8-133C030FA4FA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "21075366-2E56-44B5-A32B-024CFED9EC59" );

            // Attribute for BlockType
            //   BlockType: Scheduled Job History
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2306068D-3551-4C10-8DB8-133C030FA4FA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "25C5C011-3910-4803-B23E-215664088D36" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6076609B-D4D2-4825-8BB2-8681E99C59F2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C231C26D-772D-4C93-8D73-BECE7BB61B6E" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6076609B-D4D2-4825-8BB2-8681E99C59F2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5228AB7E-4698-45C5-9F2E-CCAEC93E0EA4" );

            // Attribute for BlockType
            //   BlockType: Signature Document List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6076609B-D4D2-4825-8BB2-8681E99C59F2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the signature document details.", 0, @"", "2D9F3DAB-723C-4C22-A583-603F81F5EFB4" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template Detail
            //   Category: Core
            //   Attribute: Default File Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5", "09EC7F0D-3505-4090-B010-ABA68CB9B904", "Default File Type", "DefaultFileType", "Default File Type", @"The default file type to use when creating new documents.", 0, @"8C9C5A97-005A-46E5-AF7B-AC2F359B738A", "10896922-ABD0-4FFD-9D8A-5609023C3907" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template Detail
            //   Category: Core
            //   Attribute: Show Legacy Signature Providers
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legacy Signature Providers", "ShowLegacyExternalProviders", "Show Legacy Signature Providers", @"Enable this setting to see the configuration for legacy signature providers. Note that support for these providers will be fully removed in the next full release.", 1, @"False", "EC663120-AEE3-42E8-9C31-BC1FA5E53FC6" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7CD92079-5E56-4FB3-914F-7DF6247BD1A2" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "93F34441-1109-49F8-81E8-521656A00DD1" );

            // Attribute for BlockType
            //   BlockType: Signature Document Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the signature document template details.", 0, @"", "C5246895-FFEF-41FF-9E26-8F5AB5783C79" );
        }

        private void ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 17.0.35",
                blockTypeReplacements: new Dictionary<string, string> {
                    // blocks chopped in v17.0.35
{ "01D23E86-51DC-496D-BB3E-0CEF5094F304", "b80e8563-41f2-4528-81e5-c62cf1ece9de" }, // Signature Document Detail ( Core )
{ "02D0A037-446B-403B-9719-5EF7D98239EF", "dabf690b-be17-4821-a13e-44c7c8d587cd" }, // Binary File Type Detail ( Core )
{ "256F6FDB-B241-4DE6-9C38-0E9DA0270A22", "6076609b-d4d2-4825-8bb2-8681e99c59f2" }, // Signature Document List ( Core )
{ "2E413152-B790-4EC2-84A9-9B48D2717D63", "FFCA1F50-E5FA-45B0-8D97-E2707E19BBA7" }, // Signature Document Template List ( Core )
{ "9F26A1DA-74AE-4CB7-BABC-6AE81A581A06", "E6A5BAC5-C34C-421A-B536-EEC3D9F1D1B5" }, // Signature Document Template Detail ( Core )
{ "A79336CD-2265-4E36-B915-CF49956FD689", "5bd4cd27-c1c1-4e12-8756-9c93e4edb28e" }, // Badge Detail ( CRM )
{ "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB", "2306068d-3551-4c10-8db8-133c030fa4fa" }, // Scheduled Job History ( Core )
{ "D9834641-7F39-4CFA-8CB2-E64068127565", "F6A780EB-66A7-475D-A42E-3C29AD5A89D3" }, // Communication Entry ( Communication )
{ "F791046A-333F-4B2A-9815-73B60326162D", "e120f06f-6db7-464a-a797-c3c90b92ef40" }, // Prayer Request Detail ( Prayer )
                    // blocks chopped in v1.17.0.32
{ "21FFA70E-18B3-4148-8FC4-F941100B49B8", "68D2ABBC-3C43-4450-973F-071D1715C0C9" }, // Attendance History ( Check-in )
{ "23CA8858-6D02-48A8-92C4-CE415DAB41B6", "ADBF3377-A491-4016-9375-346496A25FB4" }, // Apple TV Page Detail ( TV > TV Apps )
// { "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6", "7EA2E093-2F33-4213-A33E-9E9A7A760181" }, // Check-in Type Detail ( Check-in > Configuration )
{ "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "31FB8C39-80BD-4EA9-A1CB-BF6C4667929B" }, // Financial Pledge List ( Finance )
{ "813CFCCF-30BF-4A2F-BB55-F240A3B7809F", "653052A0-CA1C-41B8-8340-4B13149C6E66" }, // Person Signal List ( Core )
{ "95366DA1-D878-4A9A-A26F-83160DBE784F", "C28368CA-5218-4B59-8BD8-75BD78AA9BE9" }, // System Communication Preview ( Communication )
{ "9F577C39-19FB-4C33-804B-35023284B856", "CFE6F48B-ED85-4FA8-B068-EFE116B32284" }, // Security Change Audit List ( Security )
{ "A2C41730-BF79-4F8C-8368-2C4D5F76129D", "28A34F1C-80F4-496F-A598-180974ADEE61" }, // Rest Key Detail( Security )
{ "A81AB554-B438-4C7F-9C45-1A9AE2F889C5", "3B8B5AE5-4139-44A6-8EAA-99D48E51134E" }, // Assessment Type Detail ( CRM )
{ "B4D8CBCA-00F6-4D81-B8B6-170373D28128", "C12C615C-384D-478E-892D-0F353E2EF180" }, // Gateway Detail ( Finance )
{ "E2D423B8-10F0-49E2-B2A6-D62892379429", "3855B15B-C903-446A-AE5B-891AB52851CB" }, // System Configuration ( Administration )
                    // blocks chopped in v1.17.0.31
{ "41CD9629-9327-40D4-846A-1BB8135D130C", "dbcfb477-0553-4bae-bac9-2aec38e1da37" }, // Registration Instance - Fee List
{ "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "5ecca4fb-f8fb-49db-96b7-082bb4e4c170" }, // Assessment List
{ "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "ed4cd6ae-ed86-4607-a252-f15971e4f2e3" }, // Note Watch List
{ "361F15FC-4C08-4A26-B482-CC260E708F7C", "b1f65833-ceca-4054-bcc3-2de5692741ed" }, // Note Watch Detail
// { "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "f431f950-f007-493e-81c8-16559fe4c0f0" }, // Defined Value List
// { "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "73fd23b4-fa3a-49ea-b271-ffb228c6a49e" }, // Defined Type Detail
{ "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "a6d8bfd9-0c3d-4f1e-ae0d-325a9c70b4c8" }, // REST Controller List
{ "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "2eafa987-79c6-4477-a181-63392aa24d20" }, // Rest Action List
{ "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "57babd60-2a45-43ac-8ed3-b09af79c54ab" }, // Account List
{ "DCD63280-B661-48AA-8DEB-F5ED63C7AB77", "c0c464c0-2c72-449f-b46f-8e31c1daf29b" }, // Account Detail (Finance)
{ "E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65", "507F5108-FB55-48F0-A66E-CC3D5185D35D" }, // Campus Detail
{ "B3E4584A-D3C3-4F68-9B7C-D1641B9B08CF", "b150e767-e964-460c-9ed1-b293474c5f5d" }, // Tag Detail
{ "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA", "972ad143-8294-4462-b2a7-1b36ea127374" }, // Group Archived List
{ "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "b6a17e77-e53d-4c96-bcb2-643123b8160c" }, // Schedule List
{ "C679A2C6-8126-4EF5-8C28-269A51EC4407", "5f3151bf-577d-485b-9ee3-90f3f86f5739" }, // Document Type List
{ "85E9AA73-7C96-4731-8DD6-AA604C35E536", "fd3eb724-1afa-4507-8850-c3aee170c83b" }, // Document Type Detail
{ "4280625A-C69A-4B47-A4D3-89B61F43C967", "d9510038-0547-45f3-9eca-c2ca85e64416" }, // Web Farm Settings
{ "B6AD2D98-0DF3-4DFB-AE2B-A8CF6E21E5C0", "011aede7-b036-4f4a-bf3e-4c284dc45de8" }, // Interaction Detail
{ "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100", "054a8469-a838-4708-b18f-9f2819346298" }, // Fundraising Donation List
{ "8CD3C212-B9EE-4258-904C-91BA3570EE11", "e3b5db5c-280f-461c-a6e3-64462c9b329d" }, // Device Detail
{ "678ED4B6-D76F-4D43-B069-659E352C9BD8", "e07607c6-5428-4ccf-a826-060f48cacd32" }, // Attendance List
{ "451E9690-D851-4641-8BA0-317B65819918", "2ad9e6bc-f764-4374-a714-53e365d77a36" }, // Content Channel Type Detail
{ "E664BB02-D501-40B0-AAD6-D8FA0E63438B", "699ed6d1-e23a-4757-a0a2-83c5406b658a" }, // Fundraising List
                    // blocks chopped in v1.17.0.30
{ "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "5AA30F53-1B7D-4CA9-89B6-C10592968870" }, // Prayer Request Entry
{ "74B6C64A-9617-4745-9928-ABAC7948A95D", "C64F92CC-38A6-4562-8EAE-D4F30B4AF017" }, // Mobile Layout Detail
{ "092BFC5F-A291-4472-B737-0C69EA33D08A", "3852E96A-9270-4C0E-A0D0-3CD9601F183E" }, // Lava Shortcode Detail
{ "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "04AB8A15-1D0A-4F53-84FE-7B0DE611EB02" }, // Event List
{ "0BFD74A8-1888-4407-9102-D3FCEABF3095", "904DB731-4A40-494C-B52C-95CF0F54C21F" }, // Personal Link Section List
{ "160DABF9-3549-447C-9E76-6CFCCCA481C0", "1228F248-6AA1-4871-AF9E-195CF0FDA724" }, // Verify Photo
{ "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "DBFA9E41-FA62-4869-8A44-D03B561433B2" }, // User Login List
{ "7764E323-7460-4CB7-8024-056136C99603", "C523CABA-A32C-46A3-A8B4-8F962CDC6A78" }, // Photo Upload
                    // blocks chopped in v1.17.0.29
{ "616D1A98-067D-43B8-B7F5-41FB12FB894E", "53A34D60-31B8-4D22-BC42-E3B669ED152B" }, // Auth Client List
{ "312EAD0E-4068-4211-8410-2EB45B7D8BAB", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" }, // Auth Client Detail
{ "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5", "63F5509A-3D71-4F0F-A074-FA5869856038" }, // Consumer List
{ "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" }, // Attribute Matrix Template Detail
{ "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" }, // Note Type List
{ "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" }, // Note Type Detail
{ "3C9D442B-D066-43FA-9380-98C60936992E", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00" }, // Media Folder Detail
{ "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F", "6035AC10-07A5-4EDD-A1E9-10862FC41494" }, // Persisted Dataset Detail
{ "E7546752-C3DC-4B96-88D9-A431F2D1C989", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" }, // Personal Link List
{ "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" }, // Financial Statement Template Detail
{ "49F3D87E-BD8D-43D4-8217-340F3DFF4562", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" }, // Apple TV App Detail
{ "5D58BF6A-3914-420C-9013-53CE8A15E390", "A8062FE5-5BCD-48AC-8C37-2124462656A7" }, // Workflow Trigger Detail
{ "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "F140B415-9BB3-4492-844E-5A529517A484" }, // Tag Report
                    // blocks chopped in v1.17.0.28
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List
                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "ShowAccountFilter,ShowDateRangeFilter,ShowLastModifiedFilter,ShowPersonFilter" }, // Pledge List ( Finance )
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "EnableDebug,LimitToOwed,MaxResults" }, // Registration List Lava
                { "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "EntityType" }, // Note Type List
                { "361F15FC-4C08-4A26-B482-CC260E708F7C", "NoteType,EntityType" }, // Note Watch Detail
                { "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6", "EnableDebug" }, // Prayer Request Entry
                { "C96479B6-E309-4B1A-B024-1F1276122A13", "MaximumNumberOfDocuments" }, // Benevolence Type Detail
                { "D9834641-7F39-4CFA-8CB2-E64068127565", "DisplayCount" }, // Communication Entry ( Communication )
                { "F791046A-333F-4B2A-9815-73B60326162D", "EnableAIDisclaimer,AIDisclaimer" }, // Prayer Request Detail ( Prayer )
            } );
        }

        #endregion

        #region KH: Remove NULL Attribute Auth Role

        private void RemoveNullAttributeAuthRoleUp()
        {
            Sql( @"DELETE FROM [dbo].[Auth] WHERE [Guid] = '45db5607-67e4-499c-95fb-86284002a83b'" );
        }

        private void RemoveNullAttributeAuthRoleDown()
        {
            Sql( @"INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid]) VALUES (49, NULL, 0, 'View', 'A', 1, NULL, '45db5607-67e4-499c-95fb-86284002a83b')" );
        }

        #endregion

        #region SK: Update Message Adaptation Attribute

        private void UpdateAdaptiveMessageAdaptationAttributeUp()
        {
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.AdaptiveMessageAdaptation", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Call To Action Text", "Call To Action Text", @"", 2015, @"", "9E67B39B-C95C-464B-AC7B-CB191834EF85", "CallToAction" );
            Sql( @"
    UPDATE [Attribute] SET [Key]='CallToActionText' WHERE [Guid]='9E67B39B-C95C-464B-AC7B-CB191834EF85'
    UPDATE [Attribute] SET [Order] = [Order] + 1 Where [Order] > 2015" );
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.AdaptiveMessageAdaptation", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "", "", "Call To Action Link", "Call To Action Link", @"", 2016, @"", "FF9A62A6-3F19-458C-9527-9A3274652BCC", "CallToActionLink" );
        }

        #endregion

        #region JC: LMS Program Completion Page Description

        private void RemoveProgramCompletionDescription()
        {
            Sql( @"
-- Remove the Description from the LMS Program Completions Page.
UPDATE p SET
	[Description] = ''
FROM [dbo].[Page] p
WHERE p.[Guid] = '395BE5DD-E524-4B75-A4CA-5A0548645647'
" );
        }

        #endregion
    }
}
