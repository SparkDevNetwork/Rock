// <copyright>
// Copyright by BEMA Information Technologies
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.WorkflowExtensions.Migrations
{
    [MigrationNumber( 1, "1.8.0" )]
    class AddMatrixTemplates : Migration
    {
        public override void Up()
        {
            // Setup Attribute Matrix: Set Workflow Attributes
            Sql( @"
            DECLARE @TemplateGuid uniqueidentifier= '7308FDDC-8123-4C22-8DBC-E283F9F12C1D'
            DECLARE @FieldOneAttributeGuid uniqueidentifier = '5F8E7825-D7DA-47FE-BA65-E963B462A013'
            DECLARE @FieldTwoAttributeGuid uniqueidentifier = 'CD3C5B8E-17F0-40C3-B51D-A8491122A3F6'
                        
            DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
            DECLARE @WorkflowAttributeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('33E6DF69-BDFA-407A-9744-C175B60643AE' AS uniqueidentifier));
            DECLARE @WorkflowTextOrAttributeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('3B1D93D7-9414-48F9-80E5-6A3FC8F94C20' AS uniqueidentifier));

            INSERT INTO [dbo].[AttributeMatrixTemplate] (
                [Name]
                ,[Description]
                ,[IsActive]
                ,[FormattedLava]
                ,[Guid]
            )
            VALUES (
                'Set Workflow Attributes'
                ,'A System Template used by the SetAttributeValues Workflow Action. Do Not Delete.'
                ,1
                ,'{% if AttributeMatrixItems != empty %}  <table class=''grid-table table table-condensed table-light''> <thead> <tr> {% for itemAttribute in ItemAttributes %}     <th>{{ itemAttribute.Name }}</th> {% endfor %} </tr> </thead> <tbody> {% for attributeMatrixItem in AttributeMatrixItems %} <tr>     {% for itemAttribute in ItemAttributes %}         <td>{{ attributeMatrixItem | Attribute:itemAttribute.Key }}</td>     {% endfor %} </tr> {% endfor %} </tbody> </table>  {% endif %}'
                ,@TemplateGuid
            )

            DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = @TemplateGuid);

            INSERT INTO [dbo].[Attribute] (
	             [IsSystem]
	            ,[FieldTypeId]
	            ,[EntityTypeId]
	            ,[EntityTypeQualifierColumn]
	            ,[EntityTypeQualifierValue]
	            ,[Key]
	            ,[Name]
	            ,[Description]
	            ,[Order]
	            ,[IsGridColumn]
	            ,[IsMultiValue]
	            ,[IsRequired]
	            ,[Guid]
	            ,[AllowSearch]
	            ,[IsIndexEnabled]
	            ,[IsAnalytic]
	            ,[IsAnalyticHistory]
                ,[IsActive]
                ,[EnableHistory]
            )
            VALUES
            (0, @WorkflowAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'WorkflowAttribute', 'Workflow Attribute', '', 0, 0, 0, 1, @FieldOneAttributeGuid, 0, 0, 0, 0, 1, 1 ),
            (0, @WorkflowTextOrAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'Value', 'Value', '', 1, 0, 0, 0, @FieldTwoAttributeGuid, 0, 0, 0, 0, 1, 1 )
            " );

            // Setup Attribute Matrix: Set Entity Properties
            Sql( @"
            DECLARE @TemplateGuid uniqueidentifier= '91339C27-CA24-4038-A382-7C59A1DE5906'
            DECLARE @FieldOneAttributeGuid uniqueidentifier = '2C97544A-09D9-4DC9-B42B-B7D2C2CA96AE'
            DECLARE @FieldTwoAttributeGuid uniqueidentifier = 'C9AA1001-DE5D-4923-9225-7064BA8D41F9'
                        
            DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
            DECLARE @WorkflowTextOrAttributeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('3B1D93D7-9414-48F9-80E5-6A3FC8F94C20' AS uniqueidentifier));

            INSERT INTO [dbo].[AttributeMatrixTemplate] (
                [Name]
                ,[Description]
                ,[IsActive]
                ,[FormattedLava]
                ,[Guid]
            )
            VALUES (
                'Set Entity Properties'
                ,'A System Template used by the SetEntityProperties Workflow Action. Do Not Delete.'
                ,1
                ,'{% if AttributeMatrixItems != empty %}  <table class=''grid-table table table-condensed table-light''> <thead> <tr> {% for itemAttribute in ItemAttributes %}     <th>{{ itemAttribute.Name }}</th> {% endfor %} </tr> </thead> <tbody> {% for attributeMatrixItem in AttributeMatrixItems %} <tr>     {% for itemAttribute in ItemAttributes %}         <td>{{ attributeMatrixItem | Attribute:itemAttribute.Key }}</td>     {% endfor %} </tr> {% endfor %} </tbody> </table>  {% endif %}'
                ,@TemplateGuid
            )

            DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = @TemplateGuid);

            INSERT INTO [dbo].[Attribute] (
	             [IsSystem]
	            ,[FieldTypeId]
	            ,[EntityTypeId]
	            ,[EntityTypeQualifierColumn]
	            ,[EntityTypeQualifierValue]
	            ,[Key]
	            ,[Name]
	            ,[Description]
	            ,[Order]
	            ,[IsGridColumn]
	            ,[IsMultiValue]
	            ,[IsRequired]
	            ,[Guid]
	            ,[AllowSearch]
	            ,[IsIndexEnabled]
	            ,[IsAnalytic]
	            ,[IsAnalyticHistory]
                ,[IsActive]
                ,[EnableHistory]
            )
            VALUES
            (0, @WorkflowTextOrAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'PropertyName', 'Property Name', '', 0, 0, 0, 1, @FieldOneAttributeGuid, 0, 0, 0, 0, 1, 1 ),
            (0, @WorkflowTextOrAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'Value', 'Value', '', 1, 0, 0, 1, @FieldTwoAttributeGuid, 0, 0, 0, 0, 1, 1 )
            " );

            // Setup Attribute Matrix: Set Entity Attributes
            Sql( @"
            DECLARE @TemplateGuid uniqueidentifier= 'F9267429-44ED-46AB-8A9D-A8FF904DD056'
            DECLARE @FieldOneAttributeGuid uniqueidentifier = '01ED55FD-B576-4734-B73E-E0BE1834EADD'
            DECLARE @FieldTwoAttributeGuid uniqueidentifier = 'BF7068B8-966D-4A18-963A-3CD6BE785CFD'
                        
            DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
            DECLARE @WorkflowTextOrAttributeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('3B1D93D7-9414-48F9-80E5-6A3FC8F94C20' AS uniqueidentifier));

            INSERT INTO [dbo].[AttributeMatrixTemplate] (
                [Name]
                ,[Description]
                ,[IsActive]
                ,[FormattedLava]
                ,[Guid]
            )
            VALUES (
                'Set Entity Attributes'
                ,'A System Template used by the SetEntityAttributes Workflow Action. Do Not Delete.'
                ,1
                ,'{% if AttributeMatrixItems != empty %}  <table class=''grid-table table table-condensed table-light''> <thead> <tr> {% for itemAttribute in ItemAttributes %}     <th>{{ itemAttribute.Name }}</th> {% endfor %} </tr> </thead> <tbody> {% for attributeMatrixItem in AttributeMatrixItems %} <tr>     {% for itemAttribute in ItemAttributes %}         <td>{{ attributeMatrixItem | Attribute:itemAttribute.Key }}</td>     {% endfor %} </tr> {% endfor %} </tbody> </table>  {% endif %}'
                ,@TemplateGuid
            )

            DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = @TemplateGuid);

            INSERT INTO [dbo].[Attribute] (
	             [IsSystem]
	            ,[FieldTypeId]
	            ,[EntityTypeId]
	            ,[EntityTypeQualifierColumn]
	            ,[EntityTypeQualifierValue]
	            ,[Key]
	            ,[Name]
	            ,[Description]
	            ,[Order]
	            ,[IsGridColumn]
	            ,[IsMultiValue]
	            ,[IsRequired]
	            ,[Guid]
	            ,[AllowSearch]
	            ,[IsIndexEnabled]
	            ,[IsAnalytic]
	            ,[IsAnalyticHistory]
                ,[IsActive]
                ,[EnableHistory]
            )
            VALUES
            (0, @WorkflowTextOrAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'AttributeKey', 'Attribute Key', '', 0, 0, 0, 1, @FieldOneAttributeGuid, 0, 0, 0, 0, 1, 1 ),
            (0, @WorkflowTextOrAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'Value', 'Value', '', 1, 0, 0, 1, @FieldTwoAttributeGuid, 0, 0, 0, 0, 1, 1 )
            " );

            RockMigrationHelper.UpdateEntityType( "com.bemaservices.WorkflowExtensions.Workflow.Action.SetAttributeValue", "54160702-0FDF-4749-932D-65D49E64985B", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityAttribute", "B9160A8F-DF3D-45F2-A99B-CF3DACA92A8D", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityProperty", "12855B4D-D931-435A-8378-451B977B04EE", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "12855B4D-D931-435A-8378-451B977B04EE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "93C5E154-11CC-4EFA-B822-92122361FA76" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityProperty:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "12855B4D-D931-435A-8378-451B977B04EE", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "The type of Entity.", 0, @"", "D17888FA-91A9-4716-9DAD-07ED8E23990D" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityProperty:Entity Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "12855B4D-D931-435A-8378-451B977B04EE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Entity Id or Guid|Entity Attribute", "EntityIdGuid", "The id or guid of the entity. <span class='tip tip-lava'></span>", 1, @"", "B300310B-6F58-4340-B1FC-211C3EDEC6B2" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityProperty:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "12855B4D-D931-435A-8378-451B977B04EE", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Empty Value Handling", "EmptyValueHandling", "How to handle empty property values.", 4, @"", "DD48054F-D491-464A-A1B5-C44F6C73F21E" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityProperty:Empty Value Handling
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "12855B4D-D931-435A-8378-451B977B04EE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1619480-604E-4BBA-AF73-F559871916D9" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityProperty:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "12855B4D-D931-435A-8378-451B977B04EE", "F16FC460-DC1E-4821-9012-5F21F974C677", "Set Entity Properties", "Matrix", "", 2, @"", "ED983BE0-B3B2-4926-87A1-25AFBF9869FE" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityProperty:Set Entity Properties
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "54160702-0FDF-4749-932D-65D49E64985B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "733B86D4-055E-4247-BA55-663EB52A624A" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "54160702-0FDF-4749-932D-65D49E64985B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "7A260BEA-9531-4640-A581-A978A01D567A" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "54160702-0FDF-4749-932D-65D49E64985B", "F16FC460-DC1E-4821-9012-5F21F974C677", "Set Workflow Attributes", "Matrix", "", 0, @"", "4E14C463-C497-4B82-AA2E-90457C4F445B" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetAttributeValue:Set Workflow Attributes
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B9160A8F-DF3D-45F2-A99B-CF3DACA92A8D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "FA8CC0E4-798E-48F7-B2CD-71B81A165058" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityAttribute:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B9160A8F-DF3D-45F2-A99B-CF3DACA92A8D", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "The type of Entity.", 0, @"", "32616B50-9466-4936-B1D8-AF4DE81F610A" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityAttribute:Entity Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B9160A8F-DF3D-45F2-A99B-CF3DACA92A8D", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Entity Id or Guid|Entity Attribute", "EntityIdGuid", "The id or guid of the entity. <span class='tip tip-lava'></span>", 1, @"", "42485B8A-BDF4-4745-8716-9C143B1F69B9" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityAttribute:Entity Id or Guid|Entity Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B9160A8F-DF3D-45F2-A99B-CF3DACA92A8D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "800DF2A2-58AE-4273-806E-61622B76FEAD" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityAttribute:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B9160A8F-DF3D-45F2-A99B-CF3DACA92A8D", "F16FC460-DC1E-4821-9012-5F21F974C677", "Set Entity Attributes", "Matrix", "", 2, @"", "C34FBF73-DD12-4272-9201-E514A36D49D8" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.SetEntityAttribute:Set Entity Attributes
            
        }

        public override void Down()
        {
            
        }
    }
}
