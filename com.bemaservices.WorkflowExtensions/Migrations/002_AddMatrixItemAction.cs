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
    [MigrationNumber( 2, "1.8.0" )]
    class AddMatrixItemAction : Migration
    {
        public override void Up()
        {
            // Setup Attribute Matrix: Set Workflow Attributes
            Sql( @"
            DECLARE @TemplateGuid uniqueidentifier= 'BF2EE993-AC09-4EDA-95D8-7E3757CB8A46'
            DECLARE @FieldOneAttributeGuid uniqueidentifier = '018A1D55-AB02-497A-9C64-6524235D7FCD'
            DECLARE @FieldTwoAttributeGuid uniqueidentifier = 'ED12AF26-5773-402A-ACA1-A576CD804A58'
                        
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
                'Attribute Matrix Workflow Item'
                ,'A System Template used by the Add Item to Attribute Matrix Workflow Action. Do Not Delete.'
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
            (0, @TextAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'ColumnKey', 'Column Key', '', 0, 0, 0, 1, @FieldOneAttributeGuid, 0, 0, 0, 0, 1, 1 ),
            (0, @WorkflowTextOrAttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', convert(nvarchar(max),@AttributeMatrixTemplateId), 'ColumnValue', 'Column Value', '', 1, 0, 0, 0, @FieldTwoAttributeGuid, 0, 0, 0, 0, 1, 1 )
            " );                       
            
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.WorkflowExtensions.Workflow.Action.AddItemToMatrix", "EA575108-9C6C-4256-AB0A-F2ED1C1A4186", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EA575108-9C6C-4256-AB0A-F2ED1C1A4186", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "041F1193-9AEA-4F3C-B0F7-66864ACC4879" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddItemToMatrix:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EA575108-9C6C-4256-AB0A-F2ED1C1A4186", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Matrix", "TargetMatrix", "The Matrix to add the item to", 0, @"", "9709999A-A0A5-4B62-B10C-11835E609207" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddItemToMatrix:Matrix
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EA575108-9C6C-4256-AB0A-F2ED1C1A4186", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "55824703-C388-4A6A-A53B-901247D1F0DE" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddItemToMatrix:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EA575108-9C6C-4256-AB0A-F2ED1C1A4186", "F16FC460-DC1E-4821-9012-5F21F974C677", "Attribute Item Values", "ItemMatrix", "", 0, @"", "1D6C70D7-F8F9-4224-8682-A62172A1A174" ); // com.bemaservices.WorkflowExtensions.Workflow.Action.AddItemToMatrix:Attribute Item Values
                      
        }

        public override void Down()
        {
            
        }
    }
}
