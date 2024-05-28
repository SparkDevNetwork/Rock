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
    public partial class Rollup_20220329 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateDefaultOrganizationTemplateMergeFields();
            AddFormBuilderRoute();
            UnattendedCheckinMoveLocationSelectionStrategyAction();
            FixTreeviewTitle();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// KA: Updated the default merge fields for the Default Organization Template to use the updated Communication model properties
        /// </summary>
        private void UpdateDefaultOrganizationTemplateMergeFields()
        {
            	Sql( @"    DECLARE @Message nvarchar(MAX)
                BEGIN
                    SET @Message = (SELECT [Message] FROM [CommunicationTemplate] WHERE [Guid] = 'afe2add1-5278-441e-8e84-1dc743d99824')
                    SET @Message = Replace(@Message,'Communication.FromName', 'Communication.FromName')
                    SET @Message = Replace(@Message,'Communication.FromAddress', 'Communication.FromEmail')
                    BEGIN
                        UPDATE
                            [CommunicationTemplate]
                        SET
                            [Message] = @Message
                        WHERE
                            [Guid] = 'afe2add1-5278-441e-8e84-1dc743d99824'
                    END
                END" );
        }

        /// <summary>
        /// ED: Add Form Builder Route
        /// </summary>
        private void AddFormBuilderRoute()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route
            //   Page:Form Builder
            //   Route:admin/general/form-builder
            RockMigrationHelper.AddPageRoute("4F77819C-8F69-4418-933E-08F63E7FC4F9","admin/general/form-builder","335F2313-7FC1-42B4-AD8E-4C2A965F3380");
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// ED: Move LocationSelectionStrategy Action in Unattended Check-in WF
        /// </summary>
        private void UnattendedCheckinMoveLocationSelectionStrategyAction()
        {
            Sql( @"
                DECLARE @WFACFilterLocationsByLocationSelectionStrategyEntityTypeId int = (SELECT [Id] FROM EntityType WHERE [Guid] = '176E0639-6482-4AED-957F-FDAA7AAA44FA')

                DECLARE @WorkflowActivityTypeAbilityLevelId int = (SELECT [Id] FROM [WorkflowActivityType] WHERE [Guid] = '0E2F5EBA-2204-4C2F-845A-92C25AB67474')
                DECLARE @WorkflowActivityTypeLocationSearch int = (SELECT [Id] FROM [WorkflowActivityType] WHERE [Guid] = '0D177920-E9DF-435E-8306-527FF5775AEF')

                DECLARE @locationStrategyIdAbilityLevel int = (SELECT [Id] FROM [WorkflowActionType] WHERE [Guid] = '90F1ACA5-0AD8-4F37-B269-EE6EF4428F9C' AND [ActivityTypeId] = @WorkflowActivityTypeAbilityLevelId)
                DECLARE @locationStrategyIdLocation int = (SELECT [Id] FROM [WorkflowActionType] WHERE [EntityTypeId] = @WFACFilterLocationsByLocationSelectionStrategyEntityTypeId AND [ActivityTypeId] = @WorkflowActivityTypeLocationSearch)

                -- If the Location selection strategy filter is in both Ability level search and Location search
                -- It was manually added by the church and does not use our guid. Delete it, the one in Ability level will be moved in the next query
                IF(@locationStrategyIdLocation IS NOT NULL AND @locationStrategyIdAbilityLevel IS NOT NULL)
                BEGIN
	                -- Delete this one if the original one still exists. 
	                DELETE FROM [WorkflowActionType]
	                WHERE [EntityTypeId] = @WFACFilterLocationsByLocationSelectionStrategyEntityTypeId
		                AND [ActivityTypeId] = @WorkflowActivityTypeLocationSearch
                END

                -- Now move the Location Selection Strategy from Ability Level Search to Location Search
                IF(@locationStrategyIdAbilityLevel IS NOT NULL)
                BEGIN
	                UPDATE [WorkflowActionType]
	                SET ActivityTypeId = (SELECT [Id] FROM [WorkflowActivityType] WHERE [Guid] = '0D177920-E9DF-435E-8306-527FF5775AEF')
	                WHERE [Guid] = '90F1ACA5-0AD8-4F37-B269-EE6EF4428F9C'
		                AND [ActivityTypeId] = @WorkflowActivityTypeAbilityLevelId

	                UPDATE [WorkflowActionType] SET [Order] = [Order] - 1 WHERE [Guid] = '5ADD8020-B869-4ECF-A1C0-C3D38F907DB1'
	                UPDATE [WorkflowActionType] SET [Order] = [Order] - 1 WHERE [Guid] = '81755F3B-96C1-4517-A019-04B16E8B5B51'
	                UPDATE [WorkflowActionType] SET [Order] = [Order] - 1 WHERE [Guid] = '902931D2-6326-4A6A-967C-C9F65F8C1386'

	                UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [Guid] = '31EC4F79-363E-44AE-9BD4-D1EA1E1CDFC9'
	                UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [Guid] = 'E54277F6-93AC-499B-84CC-E031076E2AC0'
                END" );
        }

        /// <summary>
        /// SK: Fix GroupTreeView.ascx Title
        /// </summary>
        private void FixTreeviewTitle()
        {
            Sql( @"
            DECLARE @AttributeId INT = ( 
		    SELECT TOP 1 [Id]
		    FROM [Attribute]
		    WHERE [Guid]='d1583306-2504-48d2-98ee-3de55c2806c7' )

            UPDATE
                [AttributeValue]
            SET [Value]='Groups'
            WHERE [AttributeId]=@AttributeId AND ([Value]='All Groups' OR [Value]='')" );
        }
    }
}
