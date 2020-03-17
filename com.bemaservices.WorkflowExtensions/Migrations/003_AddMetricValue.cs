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
    [MigrationNumber( 3, "1.8.0" )]
    class AddMetricValue : Migration
    {
        public override void Up()
        {
            // Setup Attribute Matrix: Set Workflow Attributes
            Sql( @"
            DECLARE @TemplateGuid uniqueidentifier= 'b8e01e4f-1faa-4dbd-be23-664b88dc9d63'
            DECLARE @FieldOneAttributeGuid uniqueidentifier = '4c4f59d6-0b02-4550-904b-96f847f364e6'
            DECLARE @FieldTwoAttributeGuid uniqueidentifier = 'b80ad811-f1e0-4c2d-8a82-8f185ba9ad60'
                        
            DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
            DECLARE @TextAttributeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('9C204CD0-1233-41C5-818A-C5DA439445AA' AS uniqueidentifier));
            DECLARE @WorkflowTextOrAttributeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('3B1D93D7-9414-48F9-80E5-6A3FC8F94C20' AS uniqueidentifier));

            INSERT INTO [dbo].[AttributeMatrixTemplate] (
                [Name]
                ,[Description]
                ,[IsActive]
                ,[FormattedLava]
                ,[Guid]
            )
            VALUES (
                'Add Metric Value'
                ,'A System Template used by the Add Metric Value Workflow Action. Do Not Delete.'
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
            (0, @WorkflowTextOrAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'Label', 'Label', '', 0, 0, 0, 1, @FieldOneAttributeGuid, 0, 0, 0, 0, 1, 1 ),
            (0, @WorkflowTextOrAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'Value', 'Value', '', 1, 0, 0, 0, @FieldTwoAttributeGuid, 0, 0, 0, 0, 1, 1 )
            " );

                        #region FieldTypes

            RockMigrationHelper.UpdateFieldType("Schedule Builder","","com.bemaservices.WorkflowExtensions","com.bemaservices.WorkflowExtensions.Field.Types.ScheduleBuilderFieldType","9AAF1F39-E485-4CCB-92CF-5F5BA0CD9822");

            #endregion

            #region EntityTypes
            RockMigrationHelper.UpdateEntityType("com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue","1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC",false,true);
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","239500E2-5DFF-4AD4-8BD0-BDE4703023DD"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Metric Id or Guid|Metric Attribute","MetricIdGuid","The id or guid of the Metric. <span class='tip tip-lava'></span>",1,@"","551E8F67-5C99-4F3A-9B10-F755AFAD4A78"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:Metric Id or Guid|Metric Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Metric Value Date|MetricValueDate Attribute","MetricValueDate","The datetime of the Metric Value. <span class='tip tip-lava'></span>",3,@"","0DABA2D3-32D9-4F6B-866B-B760D2B7F481"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:Metric Value Date|MetricValueDate Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Note|Note Attribute","Note","The Note of the Metric Value. <span class='tip tip-lava'></span>",3,@"","4FE1BC66-CEDF-4965-97D3-6C0A491C5F05"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:Note|Note Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","YValue|YValue Attribute","YValue","The YValue of the Metric Value. <span class='tip tip-lava'></span>",3,@"","D10080FA-60C8-4C77-9182-D6253910FD9D"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:YValue|YValue Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Empty Value Handling","EmptyValueHandling","How to handle empty property values.",5,@"","A3722856-1EB6-4B78-9BB4-105FA8CD32CA"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:Empty Value Handling
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Metric Value Type","MetricValueType","",2,@"Measure","738B795F-1910-4B78-8D75-40CC29D6DF72"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:Metric Value Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","D7B5F4FF-202C-423D-8780-B8165BE7BFC5"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("1D76F658-E85C-47F4-ADFD-40C8A4AA3CFC","F16FC460-DC1E-4821-9012-5F21F974C677","Set Metric Value Partitions","Matrix","",4,@"","585344DE-415F-42A6-B6A9-690FD3B22D3D"); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddMetricValue:Set Metric Value Partitions

            #endregion
        }

        public override void Down()
        {

        }
    }
}
