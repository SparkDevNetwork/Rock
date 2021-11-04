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
    public partial class Rollup_20211102 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdatePageRoutesToPascalCase();
            WistiaToVimeo();
            AddSecurityChangeAuditListUp();

            // This is being removed due to the need to rewrite the migration as an update instead of a delete-then-add.
            // AddCSSInliningEnabledDefault_Up();
            AddDisablePassingWorkflowTypeIdAttribute_Up();
            AddHasCutterAttribute();
            AddJobToRebuildGroupSalutations();
            FixIncorrectGivingAlertsPageLink();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddSecurityChangeAuditListDown();

            // This is being removed due to the need to rewrite the migration as an update instead of only a delete.
            // AddCSSInliningEnabledDefault_Down();
            AddDisablePassingWorkflowTypeIdAttribute_Down();
        }

        /// <summary>
        /// GJ: Update Page Routes to Pascal Case
        /// </summary>
        private void UpdatePageRoutesToPascalCase()
        {
            Sql( @"
                UPDATE [PageRoute] SET [Route]=N'group/{GroupId}' WHERE [Guid]='2BC75AF5-44AD-4BA3-90D3-15D936F722E8';
                UPDATE [PageRoute] SET [Route]=N'ContentChannel/{ContentChannelGuid}' WHERE [Guid]='24C23F62-7633-4F11-BA32-E0D7A7C09DD4';
                UPDATE [PageRoute] SET [Route]=N'people/groups/{GroupId}/history/member/{GroupMemberId}' WHERE [Guid]='46F5F6D6-8B4C-482B-4F7B-004F30C20E5C';
                UPDATE [PageRoute] SET [Route]=N'people/groups/{GroupId}/rsvp/{Id}' WHERE [Guid]='C4B394D7-2847-09E6-7E86-058617E76056';
                UPDATE [PageRoute] SET [Route]=N'finance/batches/{BatchId}/transaction/{TransactionId}' WHERE [Guid]='59574C13-5B9D-3FC1-7B7E-ED3C26A7874E';
                UPDATE [PageRoute] SET [Route]=N'finance/benevolence/{BenevolenceRequestId}/summary' WHERE [Guid]='A02DBEAD-846E-4918-1F70-4AA56DB634F3';
                UPDATE [PageRoute] SET [Route]=N'admin/general/group-types/{GroupTypeId}' WHERE [Guid]='E4F573DF-7D92-0F00-2DCB-5B2A762F9C09';
                UPDATE [PageRoute] SET [Route]=N'admin/general/campuses/{CampusId}/members/{GroupMemberId}' WHERE [Guid]='83A9D656-69EB-8BE3-5011-8642153067F4';" );
        }

        /// <summary>
        /// GJ: Update Wistia Videos to Vimeo
        /// </summary>
        private void WistiaToVimeo()
        {
            Sql( @"
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141373"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '0DEC1A4C-F60F-4391-AFF8-EB93DD94177C');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141373"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'FF502AB8-EBC2-4329-8CA1-2F8207BAF368');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141423"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'A16C6B04-F9AE-4660-A23B-131BF1660417');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141423"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'DF73F414-C0AE-45CC-B86F-AD768D56931D');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141483"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'E8015334-C624-48BC-A08E-74EA557D24FA');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141483"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'E330D555-FFD7-49E9-B99D-84F3FC50E398');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140493"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'BA279A1C-1EC9-4F9C-8AF9-313C6995A925');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140493"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'E60565FB-B43F-4B19-BB7F-7D9D05BD9BC5');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142783"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '0D0882DF-EE3D-4196-9A76-26E432A21B4F');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140393"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '5F7A87C6-38B8-4C28-9228-70DE55D456B7');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140450"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '9ABEE2F3-03E5-427A-A052-F3F5811F39E3');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142805"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'D7F52FDA-D015-436A-9817-2DF7D95FA824');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142633"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '29E81661-3CF3-4691-BA23-DA7B039AE30E');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140515"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '120EB145-1705-454A-B9A5-AD6697A55D09');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140515"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '87EA36D1-8911-4E79-A5EE-B584A03B48E5');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141519"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '36BBE6BC-2BC8-4B94-86CA-CE1D483C30FC');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141519"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '0741524D-11A1-484C-B233-F43426C2AC9B');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142241"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '9119897F-1634-4C1A-86B7-8BC9A109C754');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142241"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'BB6F6B72-EB80-4A7F-BADE-150F8826616D');" );
        }

        /// <summary>
        /// SK: Add Security Change Audit List Page and Block
        /// </summary>
        private void AddSecurityChangeAuditListUp()
        {
            // Add Page
            // Internal Name: Security Change Audit
            // Site: Rock RMS
            RockMigrationHelper.AddPage( true, "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Security Change Audit", "", "340C6E2C-7006-4490-9FD4-14D58784519B", "fa fa-user-secret" );

            // Add block type SecurityChangeAuditList
            RockMigrationHelper.UpdateBlockType( "Security Change Audit List", "Block for Security Change Audit List.", "~/Blocks/Security/SecurityChangeAuditList.ascx", "Security", "9F577C39-19FB-4C33-804B-35023284B856" );

            // Add Block
            // Block Name: Security Change Audit List
            // Page Name: Security Change Audit
            // Layout: -
            // Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "340C6E2C-7006-4490-9FD4-14D58784519B".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "9F577C39-19FB-4C33-804B-35023284B856".AsGuid(), "Security Change Audit List", "Main", @"", @"", 0, "A6AEB424-3B19-49CC-BBE4-EC5E2BC9C5B5" );
        }

        /// <summary>
        /// SK: Add Security Change Audit List Page and Block
        /// </summary>
        private void AddSecurityChangeAuditListDown()
        {
            // Remove Block
            // Name: Security Change Audit List, from Page: Security Change Audit, Site: Rock RMS
            // from Page: Security Change Audit, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A6AEB424-3B19-49CC-BBE4-EC5E2BC9C5B5" );

            // Remove block type SecurityChangeAuditList
            RockMigrationHelper.DeleteBlockType( "9F577C39-19FB-4C33-804B-35023284B856" );

            // Delete Page
            // Internal Name: Security Change Audit
            // Site: Rock RMS
            // Layout: Full Width
            RockMigrationHelper.DeletePage( "340C6E2C-7006-4490-9FD4-14D58784519B" );
        }

        /// <summary>
        /// CR: Add CSS Inlining Enabled Default
        /// </summary>
        private void AddCSSInliningEnabledDefault_Up()
        {
            // Attrib for EntityType: Communication Medium Email: CSS Inlining Enabled
            RockMigrationHelper.AddNewEntityAttribute(
                "Rock.Communication.Medium.Email",
                SystemGuid.FieldType.BOOLEAN,
                string.Empty,
                string.Empty,
                "CSS Inlining Enabled",
                "CSSInliningEnabled",
                "Enable to move CSS styles to inline attributes. This can help maximize compatibility with email clients.",
                4,
                "False",
                SystemGuid.Attribute.COMMUNICATION_MEDIUM_EMAIL_CSS_INLINING_ENABLED,
                "CSSInliningEnabled"
                );
        }

        /// <summary>
        /// CR: Add CSS Inlining Enabled Default
        /// </summary>
        private void AddCSSInliningEnabledDefault_Down()
        {
            // Attrib for EntityType: Communication Medium Email: CSS Inlining Enabled
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.COMMUNICATION_MEDIUM_EMAIL_CSS_INLINING_ENABLED );
        }

        /// <summary>
        /// CR: Add Workflow Entry Block Disable WorkflowTypeID
        /// </summary>
        private void AddDisablePassingWorkflowTypeIdAttribute_Up()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                "A8BD05C8-6F89-4628-845B-059E686F089A",
                "1EDAFDED-DFE6-4334-B019-6EECBA89E05A",
                "Disable Passing WorkflowTypeId",
                "DisablePassingWorkflowTypeId",
                "Disable Passing WorkflowTypeId",
                @"If set, it prevents the use of a Workflow Type Id (WorkflowTypeId=) from being passed in and only accepts a WorkflowTypeGuid.  To use this block setting on your external site, you will need to create a new page and add the Workflow Entry block to it.  You may also add a new route so that URLs are in the pattern www.yourorganization.com/{PageRoute}/{WorkflowTypeGuid}.  If your workflow uses a form, you will also need to adjust email content to ensure that your URLs are correct.",
                5,
                @"False",
                SystemGuid.Attribute.WORKFLOW_ENTRY_BLOCK_DISABLE_PASSING_WORKFLOWTYPEID );
        }

        /// <summary>
        /// CR: Add Workflow Entry Block Disable WorkflowTypeID
        /// </summary>
        private void AddDisablePassingWorkflowTypeIdAttribute_Down()
        {
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.WORKFLOW_ENTRY_BLOCK_DISABLE_PASSING_WORKFLOWTYPEID );
        }

        /// <summary>
        /// ED: Has Cutter attribute
        /// </summary>
        private void AddHasCutterAttribute()
        {
            RockMigrationHelper.AddEntityAttributeIfMissing( "Rock.Model.Device", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "DeviceTypeValueId", "43", "Has Cutter", "Used for indicating whether or not a printer has an attached auto-cutter. Checking this will cause Rock to perform automatic label cutting at the end of the label set (for server side label printing).", 1041, "False", "AF534D3F-CC48-4838-8FDB-9D88D61B307D", "core_device_HasCutter", false );
        }

        /// <summary>
        /// MP: Add Rebuild Group Salutations Job
        /// </summary>
        private void AddJobToRebuildGroupSalutations()
        {
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV127DataMigrationsRebuildGroupSalutations'
                    AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_127_REBUILD_GROUP_SALUTATIONS}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Class]
                    ,[CronExpression]
                    ,[NotificationStatus]
                    ,[Guid]
                ) VALUES (
                    1
                    ,1
                    ,'Rock Update Helper v12.7 - Rebuilds Group Salutation fields on Rock.Model.Group.'
                    ,'Rebuilds Group Salutation fields on Rock.Model.Group.'
                    ,'Rock.Jobs.PostV127DataMigrationsRebuildGroupSalutations'
                    ,'0 0 21 1/1 * ? *'
                    ,1
                    ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_127_REBUILD_GROUP_SALUTATIONS}'
                );
            END" );
        }

        /// <summary>
        /// MP: Fix incorrect giving alerts page link
        /// </summary>
        private void FixIncorrectGivingAlertsPageLink()
        {
            // Update Block Attribute value
            //   Block: Giving Overview
            //   BlockType: Giving Overview
            //   Block Location: Page=Contributions, Site=Rock RMS
            //   Attribute: Alert List Page
            /*   Attribute Value: Rock.SystemGuid.Page.GIVING_ALERTS */
            RockMigrationHelper.AddBlockAttributeValue( "8A8806DB-78F8-42C5-9D09-3723A868D976", "3B85794D-E382-4F65-B931-AE44A789EFF6", Rock.SystemGuid.Page.GIVING_ALERTS );
        }
    }
}
