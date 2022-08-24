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
    public partial class BackgroundCheckPersonEmailValidFix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixPersonEmailValid();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Fixes the GUID set for PersonEmailValid on the activity "Set PersonEmailValid Attribute". This affects the PMM and Checkr background check workflows.
        /// </summary>
        private void FixPersonEmailValid()
        {
            Sql( @"
                -- General Workflow Info
                DECLARE @RunLavaEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.RunLava')
                DECLARE @WorkflowActionTypeAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '431273C6-342D-4030-ADC7-7CDEDC7F8B27')

                -- Protect My Ministry
                DECLARE @pmm_ActivityTypeId varchar(36) = (
	                SELECT [Id] 
	                FROM [WorkflowActivityType] 
	                WHERE [Guid] = '2950B120-7BB5-46B5-93D0-26D3936F1894')

                DECLARE @pmm_SetPersonEmailValidAttribute_WorkflowActionTypeId int = (
	                SELECT [Id] 
	                FROM WorkflowActionType 
	                WHERE ActivityTypeId = @pmm_ActivityTypeId 
		                AND EntityTypeId = @RunLavaEntityTypeId 
		                AND [Name] = 'Set PersonEmailValid Attribute')

                DECLARE @pmm_WorkflowActivityType_Attribute_PersonEmailValidGuid varchar(36) = (
	                SELECT [Guid] 
	                FROM Attribute 
	                WHERE [Key] = 'PersonEmailValid' 
		                AND [EntityTypeQualifierColumn] = 'ActivityTypeId' 
		                AND [EntityTypeQualifierValue] = @pmm_ActivityTypeId )

                UPDATE [AttributeValue] 
                SET [Value] = @pmm_WorkflowActivityType_Attribute_PersonEmailValidGuid
                WHERE [AttributeId] = @WorkflowActionTypeAttributeId
	                AND [EntityId] = @pmm_SetPersonEmailValidAttribute_WorkflowActionTypeId

                -- Checkr
                DECLARE @checkr_ActivtyTypeId varchar(36) = (
	                SELECT [Id] 
	                FROM [WorkflowActivityType] 
	                WHERE [Guid] = 'CB30F298-7532-446C-949E-2FEC156CE700')

                DECLARE @checkr_SetPersonEmailValidAttribute_WorkflowActionTypeId int = (
	                SELECT [Id] 
	                FROM WorkflowActionType 
	                WHERE ActivityTypeId = @checkr_ActivtyTypeId 
		                AND EntityTypeId = @RunLavaEntityTypeId 
		                AND [Name] = 'Set PersonEmailValid Attribute')

                DECLARE @checkr_WorkflowActivityType_Attribute_PersonEmailValidGuid varchar(36) = (
	                SELECT [Guid] 
	                FROM Attribute 
	                WHERE [Key] = 'PersonEmailValid' 
		                AND [EntityTypeQualifierColumn] = 'ActivityTypeId' 
		                AND [EntityTypeQualifierValue] = @checkr_ActivtyTypeId )

                UPDATE [AttributeValue]
                SET [Value] = @checkr_WorkflowActivityType_Attribute_PersonEmailValidGuid
                WHERE [AttributeId] = @WorkflowActionTypeAttributeId
	                AND [EntityId] = @checkr_SetPersonEmailValidAttribute_WorkflowActionTypeId" );
        }
    }
}
