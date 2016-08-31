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
    public partial class CheckinChanges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add ContextAware attribute for HTML Content Block
            RockMigrationHelper.AddBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, "", "6783D47D-92F9-4F48-93C0-16111D675A0F" );
            
            // Add route for Ability Select page
            RockMigrationHelper.AddPageRoute( "A1CBDAA4-94DD-4156-8260-5A3781E39FD0", "checkin/ability" );

            // Remove Workflow Type, and activity setting from Group Type Select Page (it no longer runs a workflow activity)
            RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "7250DC20-6AE9-48CF-9173-74CB221AF79E", "" );
            RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "11FB556B-3E88-4189-8E54-2B92E076F426", "" );

            // Update Group Type Select page's previous page to be the person select (instead of ability select)
            RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "39D260A5-A976-4DA9-B3E0-7381E9B8F3D5", "bb8cf87f-680f-48f9-9147-f4951e033d17" );

            // Update all the workflow type settings to use the workflow type field instead of textbox with id
            Sql( @"
    UPDATE AV SET [Value] = CASE WHEN W.[Guid] IS NULL THEN '' ELSE CONVERT(varchar(60), W.[Guid]) END
    FROM [AttributeValue] AV
	    INNER JOIN [Attribute] A
		    ON A.[Id] = AV.[AttributeId]
	    INNER JOIN [BlockType] T 
		    ON CONVERT(varchar, T.[Id]) = A.[EntityTypeQualifierValue]
		    AND T.[Path] LIKE '~/Blocks/CheckIn%'
	    LEFT OUTER JOIN [WorkflowType] W
		    ON CONVERT(varchar, W.[Id]) = AV.[Value]
    WHERE A.[EntityTypeQualifierColumn] = 'BlockTypeId'
	    AND A.[Key] = 'WorkflowTypeId'

    DECLARE @WorkflowFieldTypeId int = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '46A03F59-55D3-4ACE-ADD5-B4642225DD20' )
    UPDATE A SET 
	      [FieldTypeId] = @WorkflowFieldTypeId
	    , [Key] = 'WorkflowType'
	    , [Name] = 'Workflow Type'
	    , [Description] = 'The workflow type to activate for check-in'
    FROM [Attribute] A
	    INNER JOIN [BlockType] T 
		    ON CONVERT(varchar, T.[Id]) = A.[EntityTypeQualifierValue]
		    AND T.[Path] LIKE '~/Blocks/CheckIn%'
    WHERE A.[EntityTypeQualifierColumn] = 'BlockTypeId'
	    AND A.[Key] = 'WorkflowTypeId'
" );

            // Remove the Group Search workflow activity
            Sql( @"
    DELETE [WorkflowActivityType] WHERE [Guid] = 'A9D5818E-250D-42E8-A462-89132F57B325'
    DECLARE @WorkflowTypeId int = ( SELECT [Id] FROM [WorkflowType] WHERE [Guid] = '011E9F5A-60D4-4FF5-912A-290881E37EAF' )
    UPDATE [WorkflowActivityType] SET [Order] = [Order] - 1
    WHERE [WorkflowTypeId] = @WorkflowTypeId
    AND [Order] > 2
" );
            RockMigrationHelper.UpdateWorkflowActionType( "0E2F5EBA-2204-4C2F-845A-92C25AB67474", "Filter Groups By Ability Level", 0, "54BF0279-1FBB-4537-A933-2BAD48C43063", true, false, "", "", 0, "", "DDC2EFDD-2376-479E-B322-A440230E10E8" );
            RockMigrationHelper.UpdateWorkflowActionType( "0E2F5EBA-2204-4C2F-845A-92C25AB67474", "Filter Active Locations", 1, "7BB371F9-A8DE-49D3-BEEA-C191F6C7D4A0", true, false, "", "", 0, "", "E107DCA8-6148-4CA6-B954-F96F4FE878CB" );
            RockMigrationHelper.UpdateWorkflowActionType( "0E2F5EBA-2204-4C2F-845A-92C25AB67474", "Remove Empty Groups", 2, "698115D4-7B5E-48F3-BBB0-C53A20193169", true, false, "", "", 0, "", "5ADD8020-B869-4ECF-A1C0-C3D38F907DB1" );
            RockMigrationHelper.UpdateWorkflowActionType( "0E2F5EBA-2204-4C2F-845A-92C25AB67474", "Remove Empty Group Types", 3, "E998B9A7-31C9-46F6-B91C-4E5C3F06C82F", true, false, "", "", 0, "", "81755F3B-96C1-4517-A019-04B16E8B5B51" );

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "7BB371F9-A8DE-49D3-BEEA-C191F6C7D4A0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if locations should be be removed.  Select 'No' if they should just be marked as excluded.", 0, "True", "885D28C5-A395-4A05-AEFB-6131498BDF12" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3A77A36E-D613-44F7-ACA1-34666A85CD07", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if group types should be be removed.  Select 'No' if they should just be marked as excluded.", 0, "True", "DFFC0499-A352-40F5-9C49-143FAC0E1475" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BA1D77F3-CC92-4C1F-8DC5-AEBADF114E74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if group types should be be removed.  Select 'No' if they should just be marked as excluded.", 0, "True", "1667766D-2A03-4129-AF8D-F88D0821A074" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "54BF0279-1FBB-4537-A933-2BAD48C43063", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", 0, "True", "2FBA7E72-3EC1-4C77-83D8-71DF53E113C4" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "23F1E3FD-48AE-451F-9911-A5C7523A74B6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", 0, "True", "16020443-CE2E-41B8-BFE9-2E1AA4E6E07C" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "542DFBF4-F2F5-4C2F-9C9D-CDB00FDD5F04", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", 0, "True", "39F82A06-DD0E-4D51-A364-8D6C0EB32BC4" );

            RockMigrationHelper.AddActionTypeAttributeValue("E107DCA8-6148-4CA6-B954-F96F4FE878CB", "C8BE5BB1-9293-4FA0-B4CF-FED19B855465", "");
            RockMigrationHelper.AddActionTypeAttributeValue("E107DCA8-6148-4CA6-B954-F96F4FE878CB", "D6BCB113-0699-4D58-8002-BC919CB4BA04", "False");
            RockMigrationHelper.AddActionTypeAttributeValue("1B640897-D832-4101-9C8C-D8274EF59F9A", "885D28C5-A395-4A05-AEFB-6131498BDF12", "False");
            RockMigrationHelper.AddActionTypeAttributeValue("E107DCA8-6148-4CA6-B954-F96F4FE878CB", "885D28C5-A395-4A05-AEFB-6131498BDF12", "False");
            RockMigrationHelper.AddActionTypeAttributeValue("F01D08DB-5AAE-417B-85D2-3FC7DAB44CBC", "DFFC0499-A352-40F5-9C49-143FAC0E1475", "True");
            RockMigrationHelper.AddActionTypeAttributeValue("BB3C824F-3B77-4C52-A173-ACCE9F60BA3E", "1667766D-2A03-4129-AF8D-F88D0821A074", "True");
            RockMigrationHelper.AddActionTypeAttributeValue("DDC2EFDD-2376-479E-B322-A440230E10E8", "2FBA7E72-3EC1-4C77-83D8-71DF53E113C4", "False");
            RockMigrationHelper.AddActionTypeAttributeValue("BB45E6E1-C39A-42A2-B988-490382DB7977", "16020443-CE2E-41B8-BFE9-2E1AA4E6E07C", "True");
            RockMigrationHelper.AddActionTypeAttributeValue("6D8317FB-8AAB-4533-A472-01FA572478D7", "39F82A06-DD0E-4D51-A364-8D6C0EB32BC4", "True");

            // Attrib for Checkin Manager Chart Style
            RockMigrationHelper.AddBlockAttributeValue( "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC", "4708D29F-0D15-4175-91C6-A7AFEA37BF1D", @"2ABB2EA0-B551-476C-8F6B-478CD08C2227" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    UPDATE AV SET [Value] = CASE WHEN W.[Id] IS NULL THEN '' ELSE CONVERT(varchar, W.[Id]) END
    FROM [AttributeValue] AV
	    INNER JOIN [Attribute] A
		    ON A.[Id] = AV.[AttributeId]
	    INNER JOIN [BlockType] T 
		    ON CONVERT(varchar, T.[Id]) = A.[EntityTypeQualifierValue]
		    AND T.[Path] LIKE '~/Blocks/CheckIn%'
	    LEFT OUTER JOIN [WorkflowType] W
		    ON CONVERT(varchar(60), W.[Guid]) = AV.[Value]
    WHERE A.[EntityTypeQualifierColumn] = 'BlockTypeId'
	    AND A.[Key] = 'WorkflowTypeId'

    DECLARE @IntegerFieldTypeId int = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = 'A75DFC58-7A1B-4799-BF31-451B2BBE38FF' )
    UPDATE A SET 
	      [FieldTypeId] = @IntegerFieldTypeId
	    , [Key] = 'WorkflowTypeId'
	    , [Name] = 'Workflow Type Id'
	    , [Description] = 'The workflow type id to activate for check-in'
    FROM [Attribute] A
	    INNER JOIN [BlockType] T 
		    ON CONVERT(varchar, T.[Id]) = A.[EntityTypeQualifierValue]
		    AND T.[Path] LIKE '~/Blocks/CheckIn%'
    WHERE A.[EntityTypeQualifierColumn] = 'BlockTypeId'
	    AND A.[Key] = 'WorkflowTypeId'
" );

        }
    }
}
