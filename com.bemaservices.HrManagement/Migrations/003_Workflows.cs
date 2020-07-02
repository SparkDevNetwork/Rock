using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.HrManagement.SystemGuid;
using Rock.Web.Cache;
using Rock.Lava.Blocks;
using System.Security.AccessControl;

namespace com.bemaservices.HrManagement.Migrations
{
    [MigrationNumber( 3, "1.9.4" )]
    public class Workflows : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            PtoRequestWorkflowOriginal();
            ModifyPtoAllocationWorkflow();
            PtoRequestWorkflow();
        }

        private void PtoRequestWorkflow()
        {
                        #region FieldTypes


            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType("Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true);
            RockMigrationHelper.UpdateEntityType("com.bemaservices.HrManagement.Workflow.Action.PtoRequestDelete","2CBFBD56-2F3A-4CD5-AA23-A807CEBEEB54",false,true);
            RockMigrationHelper.UpdateEntityType("com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate","546C6C01-5C8B-449E-A16A-580D92D0317B",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.ActivateActivity","38907A90-1634-4A93-8017-619326A4A582",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CompleteWorkflow","EEDA4318-F014-4A46-9C76-4C052EF81AA1",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.RunLava","BC21E57A-1477-44B3-A7C2-61A806118945",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.UserEntryForm","486DC4FA-FCBC-425F-90B0-E606DA8A9F68",false,true);
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("2CBFBD56-2F3A-4CD5-AA23-A807CEBEEB54","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0883AE9A-24EE-433F-9925-C7A5C8BCB467"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestDelete:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("2CBFBD56-2F3A-4CD5-AA23-A807CEBEEB54","33E6DF69-BDFA-407A-9744-C175B60643AE","Existing Pto Request","PTO_REQUEST_ATTRIBUTE_KEY","The Pto Request to update.",0,@"","1B976B41-C2D6-4C16-B781-6F5DD1AC2B69"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestDelete:Existing Pto Request
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("2CBFBD56-2F3A-4CD5-AA23-A807CEBEEB54","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","CF3E47A3-DE75-49B6-971A-0D91DEE55079"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestDelete:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E8ABD802-372C-47BE-82B1-96F50DB5169E"); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","739FD425-5B8C-4605-B775-7E4D9D4C11DB","Activity","Activity","The activity type to activate",0,@"","02D5A7A5-8781-46B4-B9FC-AF816829D240"); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","3809A78C-B773-440C-8E3F-A8E81D0DAE08"); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","234910F2-A0DB-4D7D-BAF7-83C880EF30AE"); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","C178113D-7C86-4229-8424-C6D0CF4A7E23"); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","DD610F3E-2E83-41AE-B63B-9B163B87F82E"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","33E6DF69-BDFA-407A-9744-C175B60643AE","Existing Pto Request","PTO_REQUEST_ATTRIBUTE_KEY","The Pto Request to update.",0,@"","C957F777-F0FE-4D05-BB22-10D7C7A5C437"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Existing Pto Request
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Allocation|Attribute Value","ALLOCATION_KEY","The allocation or an attribute that contains the allocation of the pto request. <span class='tip tip-lava'></span>",1,@"","EC01344E-61BF-4E22-88E3-36051BCAABE7"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Allocation|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Approval State|Attribute Value","APPROVAL_STATE_KEY","The Approval State or an attribute that contains the Approval State of the pto request. <span class='tip tip-lava'></span>",7,@"","080025FD-9E80-4158-8D7F-FBF3ED12A2E1"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Approval State|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Approver|Attribute Value","APPROVER_KEY","The approver or an attribute that contains the approver of the pto request. <span class='tip tip-lava'></span>",6,@"","A781A20B-4F21-47CA-9BCF-1654565DB5F6"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Approver|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","End Date|Attribute Value","ENDDATE_KEY","The end date or an attribute that contains the end date of the pto request. <span class='tip tip-lava'></span>",3,@"","8304DE14-DA5C-41FD-BA30-026D91A492C7"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:End Date|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Hours|Attribute Value","HOURS_KEY","The hours per day or an attribute that contains the hours per day of the pto request. <span class='tip tip-lava'></span>",4,@"","858BFCA2-E793-446E-B146-87D5FC6783A0"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Hours|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Reason|Attribute Value","PTO_REASON_KEY","The reason or an attribute that contains the reason of the pto request. <span class='tip tip-lava'></span>",5,@"","C6A51AEB-18CB-4591-BDF8-D4017CF38DCF"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Reason|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Start Date|Attribute Value","STARTDATE_KEY","The start date or an attribute that contains the start date of the pto request. <span class='tip tip-lava'></span>",2,@"","3C5F03BD-2CDD-41D7-9ED1-5AAC62AF733D"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Start Date|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("546C6C01-5C8B-449E-A16A-580D92D0317B","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","A9B5EAF8-9CC9-4521-9FC1-480875B11CAA"); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava","Value","The <span class='tip tip-lava'></span> to run.",0,@"","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4"); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","F1924BDC-9B79-4018-9D4A-C3516C87A514"); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to store the result in.",1,@"","431273C6-342D-4030-ADC7-7CDEDC7F8B27"); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D","Enabled Lava Commands","EnabledLavaCommands","The Lava commands that should be enabled for this action.",2,@"","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5"); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("BC21E57A-1477-44B3-A7C2-61A806118945","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","1B833F48-EFC2-4537-B1E3-7793F6863EAA"); // Rock.Workflow.Action.RunLava:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C"); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Status|Status Attribute","Status","The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>",0,@"Completed","3327286F-C1A9-4624-949D-33E9F9049356"); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","25CAD4BE-5A00-409D-9BAB-E32518D89956"); // Rock.Workflow.Action.CompleteWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory("C9F3C4A5-1526-474D-803F-D6C7A45CBBAE","PTO Requests","fa fa-clock","","CD21D1FD-B9DB-4122-B252-86E8FD85CEEC",0); // PTO Requests

            #endregion

            #region PTO Request

            RockMigrationHelper.UpdateWorkflowType(false,true,"PTO Request","","CD21D1FD-B9DB-4122-B252-86E8FD85CEEC","PTO Request","fa fa-list-ol",28800,true,0,"EBF1D986-8BBD-4888-8A7E-43AF5914751C",0); // PTO Request
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Person","Person","",0,@"","FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", false); // PTO Request:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","6B6AA175-4758-453F-8D83-FCD8044B5F36","Start Date","StartDate","",2,@"","F6FB28D8-AE58-445E-9373-46CDA04E6CC5", false); // PTO Request:Start Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","6B6AA175-4758-453F-8D83-FCD8044B5F36","End Date","EndDate","",3,@"","286C5A76-9113-49C4-A209-078E856BD0B2", false); // PTO Request:End Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","C3206242-E213-4038-99EB-1A563B375997","PTO Allocation","PTOAllocation","",5,@"","C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", false); // PTO Request:PTO Allocation
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Approval State","ApprovalState","",6,@"0","FFC543BE-7B65-425B-A56C-AD441986FA2C", false); // PTO Request:Approval State
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","90ECF283-5344-4168-9224-E0D26E9B7ECB","PTO Request","PTORequest","",7,@"","9FE62EB5-6604-416F-899B-F836C1DEC7A5", false); // PTO Request:PTO Request
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Supervisor","Supervisor","",8,@"","203AB4E9-065C-4D49-8DA5-9C7C61F49A01", false); // PTO Request:Supervisor
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","99B090AA-4D7E-46D8-B393-BF945EA1BA8B","Supervisor Attribute","SupervisorAttribute","",9,@"67afd5a3-28f3-404f-a3b8-88630061f294","CB12115A-8783-472C-B980-FE404D67F12E", false); // PTO Request:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","9C204CD0-1233-41C5-818A-C5DA439445AA","HasReviewRights","HasReviewRights","",10,@"","14E6B221-3531-4177-A1B1-8DD8B24B80AA", false); // PTO Request:HasReviewRights
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Hours / Day","HoursDay","",11,@"","4180D0D3-A144-4974-B364-34292969C1A9", false); // PTO Request:Hours / Day
            RockMigrationHelper.UpdateWorkflowTypeAttribute("EBF1D986-8BBD-4888-8A7E-43AF5914751C","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Reason","Reason","",12,@"","1136E804-A793-4081-9902-F8E7ED0CDD69", false); // PTO Request:Reason
            RockMigrationHelper.AddAttributeQualifier("FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3","EnableSelfSelection",@"False","83AD189F-30C2-4EFC-AE2E-98296E86A482"); // PTO Request:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier("F6FB28D8-AE58-445E-9373-46CDA04E6CC5","datePickerControlType",@"Date Picker","C5E70505-2F09-4758-938A-F196627703F6"); // PTO Request:Start Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier("F6FB28D8-AE58-445E-9373-46CDA04E6CC5","displayCurrentOption",@"False","30D49547-2BDD-4656-90BA-FF1A49254462"); // PTO Request:Start Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier("F6FB28D8-AE58-445E-9373-46CDA04E6CC5","displayDiff",@"False","9F811390-6FC2-40F0-9D65-130BACDD59F7"); // PTO Request:Start Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier("F6FB28D8-AE58-445E-9373-46CDA04E6CC5","format",@"","58257E5F-9D08-434A-9FAE-A23C64C1005A"); // PTO Request:Start Date:format
            RockMigrationHelper.AddAttributeQualifier("F6FB28D8-AE58-445E-9373-46CDA04E6CC5","futureYearCount",@"","D51129A3-0129-45D1-952F-C5661BA20CCD"); // PTO Request:Start Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier("286C5A76-9113-49C4-A209-078E856BD0B2","datePickerControlType",@"Date Picker","4784AED2-D407-4FB3-8326-B890AF29478B"); // PTO Request:End Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier("286C5A76-9113-49C4-A209-078E856BD0B2","displayCurrentOption",@"False","9255BAD3-7299-45DF-825F-581389502DCF"); // PTO Request:End Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier("286C5A76-9113-49C4-A209-078E856BD0B2","displayDiff",@"False","890ACEB4-AC4B-4283-8D75-E682F9D42A4B"); // PTO Request:End Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier("286C5A76-9113-49C4-A209-078E856BD0B2","format",@"","14084E9B-856C-42CD-B02F-BA384C33B5C5"); // PTO Request:End Date:format
            RockMigrationHelper.AddAttributeQualifier("286C5A76-9113-49C4-A209-078E856BD0B2","futureYearCount",@"","03E11033-DBDD-4820-BAE5-61DF9418A043"); // PTO Request:End Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier("C9E17918-4ECF-4029-A2F3-BF95D4EED4E3","fieldtype",@"ddl","951A8F28-85F9-49B0-9905-42D70EE2D29B"); // PTO Request:PTO Allocation:fieldtype
            RockMigrationHelper.AddAttributeQualifier("C9E17918-4ECF-4029-A2F3-BF95D4EED4E3","repeatColumns",@"","9D40011A-F166-4889-AEEC-BBAB611F2B58"); // PTO Request:PTO Allocation:repeatColumns
            RockMigrationHelper.AddAttributeQualifier("FFC543BE-7B65-425B-A56C-AD441986FA2C","fieldtype",@"ddl","E4611C9C-7239-41E5-A7B3-88B78496ACBA"); // PTO Request:Approval State:fieldtype
            RockMigrationHelper.AddAttributeQualifier("FFC543BE-7B65-425B-A56C-AD441986FA2C","repeatColumns",@"","5B56A186-5466-47A7-B163-B83E401C058E"); // PTO Request:Approval State:repeatColumns
            RockMigrationHelper.AddAttributeQualifier("FFC543BE-7B65-425B-A56C-AD441986FA2C","values",@"0^Pending, 1^Approved, 2^Denied","5E99017C-084F-4767-A1B1-2AE09951E7EB"); // PTO Request:Approval State:values
            RockMigrationHelper.AddAttributeQualifier("203AB4E9-065C-4D49-8DA5-9C7C61F49A01","EnableSelfSelection",@"False","E4B15932-E7CB-44D9-AC82-D4B69ADE2BAF"); // PTO Request:Supervisor:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier("CB12115A-8783-472C-B980-FE404D67F12E","allowmultiple",@"False","A32DAB81-15D7-4476-86A7-C68CE127A0E5"); // PTO Request:Supervisor Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier("CB12115A-8783-472C-B980-FE404D67F12E","entitytype",@"72657ed8-d16e-492e-ac12-144c5e7567e7","A2F28B6C-130E-4D1B-84B8-1470A9E57FD1"); // PTO Request:Supervisor Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier("CB12115A-8783-472C-B980-FE404D67F12E","qualifierColumn",@"","706F02D2-B96F-4854-B0B3-E7A53D39B1A9"); // PTO Request:Supervisor Attribute:qualifierColumn
            RockMigrationHelper.AddAttributeQualifier("CB12115A-8783-472C-B980-FE404D67F12E","qualifierValue",@"","5BCFA6ED-614C-4AB9-9A38-E86449EC80BA"); // PTO Request:Supervisor Attribute:qualifierValue
            RockMigrationHelper.AddAttributeQualifier("14E6B221-3531-4177-A1B1-8DD8B24B80AA","ispassword",@"False","052AEDAE-6274-4EEA-8F4A-77B7C90FF53D"); // PTO Request:HasReviewRights:ispassword
            RockMigrationHelper.AddAttributeQualifier("14E6B221-3531-4177-A1B1-8DD8B24B80AA","maxcharacters",@"","8F28B79C-A760-42B1-8D11-7A3B5C145FF9"); // PTO Request:HasReviewRights:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("14E6B221-3531-4177-A1B1-8DD8B24B80AA","showcountdown",@"False","46D794AD-8180-460B-83F8-8A14EC35FDBB"); // PTO Request:HasReviewRights:showcountdown
            RockMigrationHelper.AddAttributeQualifier("4180D0D3-A144-4974-B364-34292969C1A9","fieldtype",@"ddl","2A33EAEB-51EA-42B8-AA7B-C11991D03C77"); // PTO Request:Hours / Day:fieldtype
            RockMigrationHelper.AddAttributeQualifier("4180D0D3-A144-4974-B364-34292969C1A9","repeatColumns",@"","4EE5FC97-EDA3-4851-8111-9D9A4B48C0B2"); // PTO Request:Hours / Day:repeatColumns
            RockMigrationHelper.AddAttributeQualifier("4180D0D3-A144-4974-B364-34292969C1A9","values",@"0.5,1.0,1.5,2.0,2.5,3.0,3.5,4.0,4.5,5.0,5.5,6.0,6.5,7.0,7.5,8.0","A89EC8C4-301E-4E67-A0C2-006C54C8CAD0"); // PTO Request:Hours / Day:values
            RockMigrationHelper.AddAttributeQualifier("1136E804-A793-4081-9902-F8E7ED0CDD69","allowhtml",@"False","8A4A440A-9251-48CF-80AB-C78F5AE3C1B8"); // PTO Request:Reason:allowhtml
            RockMigrationHelper.AddAttributeQualifier("1136E804-A793-4081-9902-F8E7ED0CDD69","maxcharacters",@"","37C36044-A89C-4530-BD63-179BA761C24C"); // PTO Request:Reason:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("1136E804-A793-4081-9902-F8E7ED0CDD69","numberofrows",@"","D42A89D5-F7E8-4D86-B13B-B08F6572591F"); // PTO Request:Reason:numberofrows
            RockMigrationHelper.AddAttributeQualifier("1136E804-A793-4081-9902-F8E7ED0CDD69","showcountdown",@"False","A0649AAB-9660-4AA4-BD44-358DBD79D076"); // PTO Request:Reason:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType("EBF1D986-8BBD-4888-8A7E-43AF5914751C",true,"Start","",true,0,"BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9"); // PTO Request:Start
            RockMigrationHelper.UpdateWorkflowActivityType("EBF1D986-8BBD-4888-8A7E-43AF5914751C",true,"Add Request","",false,1,"BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08"); // PTO Request:Add Request
            RockMigrationHelper.UpdateWorkflowActivityType("EBF1D986-8BBD-4888-8A7E-43AF5914751C",true,"Modify Request","",false,2,"9F5491B6-DCCC-4D21-A00E-DFBA138423F2"); // PTO Request:Modify Request
            RockMigrationHelper.UpdateWorkflowActivityType("EBF1D986-8BBD-4888-8A7E-43AF5914751C",true,"Delete Request","",false,3,"CEC47883-FCD3-47D3-87B7-E1A826184463"); // PTO Request:Delete Request
            RockMigrationHelper.UpdateWorkflowActionForm(@"",@"","Modify^fdc397cd-8b4a-436e-bea1-bce2e6717c03^9f5491b6-dccc-4d21-a00e-dfba138423f2^Your information has been submitted successfully.|Delete^638beee0-2f8f-4706-b9a4-5bab70386697^cec47883-fcd3-47d3-87b7-e1a826184463^","",true,"","74C1F3A4-6198-43A4-B860-BCDFD8F8656A"); // PTO Request:Start:Initial Form
            RockMigrationHelper.UpdateWorkflowActionForm(@"",@"","Submit^^^Your information has been submitted successfully.","",true,"","4C57D323-945F-4FA1-86DB-C0B5C459268C"); // PTO Request:Add Request:Form
            RockMigrationHelper.UpdateWorkflowActionForm(@"",@"","Submit^^^Your information has been submitted successfully.","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"","126DECCC-A6BE-48F4-AF2B-2A7964F002D3"); // PTO Request:Modify Request:Form
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3",0,true,true,false,false,@"",@"","1617313E-4367-473B-9567-A0A992BCD483"); // PTO Request:Start:Initial Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","F6FB28D8-AE58-445E-9373-46CDA04E6CC5",7,true,true,false,false,@"",@"","E9CF6608-8E0E-476B-B989-E10FB1BBD545"); // PTO Request:Start:Initial Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","286C5A76-9113-49C4-A209-078E856BD0B2",8,false,true,false,false,@"",@"","8589A2EE-2FF6-4E78-ADDA-AA0C89E49143"); // PTO Request:Start:Initial Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","C9E17918-4ECF-4029-A2F3-BF95D4EED4E3",1,true,true,false,false,@"",@"","9A2E0FC1-D907-49E1-84C1-6D065101341D"); // PTO Request:Start:Initial Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","FFC543BE-7B65-425B-A56C-AD441986FA2C",2,true,true,false,false,@"",@"","00C7AAA9-6025-436A-AE3F-CCDCBF5B2BB2"); // PTO Request:Start:Initial Form:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","9FE62EB5-6604-416F-899B-F836C1DEC7A5",3,false,true,false,false,@"",@"","F8DFC113-2129-4C93-A690-2D1165D1B8D3"); // PTO Request:Start:Initial Form:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","203AB4E9-065C-4D49-8DA5-9C7C61F49A01",4,false,true,false,false,@"",@"","21CD2385-3657-4B3A-ACFB-C700C3E6C441"); // PTO Request:Start:Initial Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","CB12115A-8783-472C-B980-FE404D67F12E",5,false,true,false,false,@"",@"","557FC75E-7246-48DB-B85F-C1BBFFC42229"); // PTO Request:Start:Initial Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","14E6B221-3531-4177-A1B1-8DD8B24B80AA",6,false,true,false,false,@"",@"","968C51F5-DEA3-4232-AA32-75AEB32A4FC5"); // PTO Request:Start:Initial Form:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","4180D0D3-A144-4974-B364-34292969C1A9",9,true,true,false,false,@"",@"","2C06ECF7-F252-4A23-94F3-F3E22BE734D0"); // PTO Request:Start:Initial Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("74C1F3A4-6198-43A4-B860-BCDFD8F8656A","1136E804-A793-4081-9902-F8E7ED0CDD69",10,true,true,false,false,@"",@"","714A9CCA-D968-4A66-990C-E5B3C189E3EA"); // PTO Request:Start:Initial Form:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3",0,true,true,false,false,@"",@"","1DD3103F-7465-4970-9FF1-2B913534CD01"); // PTO Request:Add Request:Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","F6FB28D8-AE58-445E-9373-46CDA04E6CC5",6,true,false,true,false,@"",@"","E0204296-3FC7-47E1-BF49-E043A9EADE40"); // PTO Request:Add Request:Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","286C5A76-9113-49C4-A209-078E856BD0B2",7,true,false,false,false,@"",@"","CB83027E-D256-4801-B9A3-EC4AA68FF93A"); // PTO Request:Add Request:Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","C9E17918-4ECF-4029-A2F3-BF95D4EED4E3",5,true,false,true,false,@"",@"","389B00A8-F7A2-4C74-A375-F181164BAF7E"); // PTO Request:Add Request:Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","FFC543BE-7B65-425B-A56C-AD441986FA2C",8,false,true,false,false,@"",@"","8B3F097B-6BA1-4953-94C9-8350FAD78C4B"); // PTO Request:Add Request:Form:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","9FE62EB5-6604-416F-899B-F836C1DEC7A5",1,false,true,false,false,@"",@"","D85DAE3A-908B-444D-A746-81C5CCC9747E"); // PTO Request:Add Request:Form:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","203AB4E9-065C-4D49-8DA5-9C7C61F49A01",2,false,true,false,false,@"",@"","CD9073A5-EE57-4315-A5E7-D24AC02681E9"); // PTO Request:Add Request:Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","CB12115A-8783-472C-B980-FE404D67F12E",3,false,true,false,false,@"",@"","12DD8CA9-05A9-4293-8D5D-34CC6E18B575"); // PTO Request:Add Request:Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","14E6B221-3531-4177-A1B1-8DD8B24B80AA",4,false,true,false,false,@"",@"","3E70A524-35FF-47EF-9441-D25A56EEC0F6"); // PTO Request:Add Request:Form:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","4180D0D3-A144-4974-B364-34292969C1A9",9,true,false,true,false,@"",@"","8C5D4C1C-C2D0-402C-A3F9-9AEF107AFB87"); // PTO Request:Add Request:Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("4C57D323-945F-4FA1-86DB-C0B5C459268C","1136E804-A793-4081-9902-F8E7ED0CDD69",10,true,false,true,false,@"",@"","F9086841-D9A4-4B85-8AD3-1A8D136FCCDA"); // PTO Request:Add Request:Form:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3",0,true,true,false,false,@"",@"","1DBDCA8D-8DF1-4779-A126-9773628968B9"); // PTO Request:Modify Request:Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","F6FB28D8-AE58-445E-9373-46CDA04E6CC5",7,true,false,true,false,@"",@"","5D934167-F53B-468C-90E4-18326AFC2AAE"); // PTO Request:Modify Request:Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","286C5A76-9113-49C4-A209-078E856BD0B2",8,true,false,false,false,@"",@"","D131F60D-7B15-43F7-8ED4-1B36DBB1F450"); // PTO Request:Modify Request:Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","C9E17918-4ECF-4029-A2F3-BF95D4EED4E3",1,true,false,false,false,@"",@"","7A350C1F-E754-4536-ADBF-2B727D716872"); // PTO Request:Modify Request:Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","FFC543BE-7B65-425B-A56C-AD441986FA2C",2,true,true,false,false,@"",@"","453C7C26-E4B7-4A8F-A32D-B9E8B5B54AE2"); // PTO Request:Modify Request:Form:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","9FE62EB5-6604-416F-899B-F836C1DEC7A5",3,false,true,false,false,@"",@"","E683AC07-F488-4056-899E-6B1104965604"); // PTO Request:Modify Request:Form:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","203AB4E9-065C-4D49-8DA5-9C7C61F49A01",4,false,true,false,false,@"",@"","42809E72-B30A-4B41-9A4B-4EC3B6AEF5F1"); // PTO Request:Modify Request:Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","CB12115A-8783-472C-B980-FE404D67F12E",5,false,true,false,false,@"",@"","5D3EDDA8-117D-4602-8618-8C471D2995F4"); // PTO Request:Modify Request:Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","14E6B221-3531-4177-A1B1-8DD8B24B80AA",6,false,true,false,false,@"",@"","FB1D31D9-E455-486E-8480-7C36D5960263"); // PTO Request:Modify Request:Form:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","4180D0D3-A144-4974-B364-34292969C1A9",9,true,false,true,false,@"",@"","6E108AAB-7ABE-4DF1-A458-59B5E4290AEC"); // PTO Request:Modify Request:Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("126DECCC-A6BE-48F4-AF2B-2A7964F002D3","1136E804-A793-4081-9902-F8E7ED0CDD69",10,true,false,true,false,@"",@"","2BDD1C62-89AE-4DA4-AD2C-93F7B8F87D5D"); // PTO Request:Modify Request:Form:Reason
            RockMigrationHelper.UpdateWorkflowActionType("BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9","Activate Add Activity if no existing PTO Request",0,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","9FE62EB5-6604-416F-899B-F836C1DEC7A5",32,"","494C7681-CFFA-47A3-8A9D-C2779A8FF4D4"); // PTO Request:Start:Activate Add Activity if no existing PTO Request
            RockMigrationHelper.UpdateWorkflowActionType("BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9","Set Person",1,"BC21E57A-1477-44B3-A7C2-61A806118945",true,false,"","",64,"","066D899E-645D-40F8-BFD8-AFDFEEFCE183"); // PTO Request:Start:Set Person
            RockMigrationHelper.UpdateWorkflowActionType("BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9","Set Pto Allocation",2,"BC21E57A-1477-44B3-A7C2-61A806118945",true,false,"","",64,"","3DA9A3CA-23AB-429F-A363-C9FD8F298966"); // PTO Request:Start:Set Pto Allocation
            RockMigrationHelper.UpdateWorkflowActionType("BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9","Set Start Date",3,"BC21E57A-1477-44B3-A7C2-61A806118945",true,false,"","",64,"","941EE98E-3732-4037-A9C7-F3D95519E1EA"); // PTO Request:Start:Set Start Date
            RockMigrationHelper.UpdateWorkflowActionType("BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9","Set Reason",4,"BC21E57A-1477-44B3-A7C2-61A806118945",true,false,"","",64,"","7E6DA347-7333-4723-9DB6-F5E5B69F64B8"); // PTO Request:Start:Set Reason
            RockMigrationHelper.UpdateWorkflowActionType("BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9","Set Hours",5,"BC21E57A-1477-44B3-A7C2-61A806118945",true,false,"","",64,"","790F40BC-0D26-42E6-B6EA-6761273653BC"); // PTO Request:Start:Set Hours
            RockMigrationHelper.UpdateWorkflowActionType("BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9","Set Approval State",6,"BC21E57A-1477-44B3-A7C2-61A806118945",true,false,"","",1,"","E2FCD026-7AC1-45F0-93BD-110920E2369C"); // PTO Request:Start:Set Approval State
            RockMigrationHelper.UpdateWorkflowActionType("BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9","Initial Form",7,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,true,"74C1F3A4-6198-43A4-B860-BCDFD8F8656A","",1,"","309F9CA5-6ADD-48F4-B615-5F367FE691FA"); // PTO Request:Start:Initial Form
            RockMigrationHelper.UpdateWorkflowActionType("BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08","Form",0,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"4C57D323-945F-4FA1-86DB-C0B5C459268C","",1,"","0B3A4061-593E-4403-9B6D-7BDC66C5AE84"); // PTO Request:Add Request:Form
            RockMigrationHelper.UpdateWorkflowActionType("BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08","Add Request",1,"546C6C01-5C8B-449E-A16A-580D92D0317B",true,false,"","",1,"","CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB"); // PTO Request:Add Request:Add Request
            RockMigrationHelper.UpdateWorkflowActionType("BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08","Complete Workflow",2,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,true,"","",1,"","FC3DA720-67E3-443D-B0CF-7983FD102BB4"); // PTO Request:Add Request:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType("9F5491B6-DCCC-4D21-A00E-DFBA138423F2","Form",0,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"126DECCC-A6BE-48F4-AF2B-2A7964F002D3","",1,"","84952D19-07EA-4C7E-A31F-7E5DA3C386B5"); // PTO Request:Modify Request:Form
            RockMigrationHelper.UpdateWorkflowActionType("9F5491B6-DCCC-4D21-A00E-DFBA138423F2","Update Request",1,"546C6C01-5C8B-449E-A16A-580D92D0317B",true,false,"","",1,"","66613CA7-EF9B-456D-910C-8F90B85FBE1F"); // PTO Request:Modify Request:Update Request
            RockMigrationHelper.UpdateWorkflowActionType("9F5491B6-DCCC-4D21-A00E-DFBA138423F2","Complete Workflow",2,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,true,"","",1,"","7A841A72-2DFB-4514-84FA-B50DE2FECE6B"); // PTO Request:Modify Request:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType("CEC47883-FCD3-47D3-87B7-E1A826184463","Delete Request",0,"2CBFBD56-2F3A-4CD5-AA23-A807CEBEEB54",true,false,"","",1,"","ED092B4A-AB43-4836-A3C7-28CFF9F47DFB"); // PTO Request:Delete Request:Delete Request
            RockMigrationHelper.UpdateWorkflowActionType("CEC47883-FCD3-47D3-87B7-E1A826184463","Complete Workflow",1,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,true,"","",1,"","B277B0BC-474C-4934-80B3-01383BB2D95E"); // PTO Request:Delete Request:Complete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue("494C7681-CFFA-47A3-8A9D-C2779A8FF4D4","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // PTO Request:Start:Activate Add Activity if no existing PTO Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue("494C7681-CFFA-47A3-8A9D-C2779A8FF4D4","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08"); // PTO Request:Start:Activate Add Activity if no existing PTO Request:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("066D899E-645D-40F8-BFD8-AFDFEEFCE183","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4",@"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.PtoAllocation.PersonAlias.Guid}}
    {% endfor %}
{% endptorequest %}"); // PTO Request:Start:Set Person:Lava
            RockMigrationHelper.AddActionTypeAttributeValue("066D899E-645D-40F8-BFD8-AFDFEEFCE183","F1924BDC-9B79-4018-9D4A-C3516C87A514",@"False"); // PTO Request:Start:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue("066D899E-645D-40F8-BFD8-AFDFEEFCE183","431273C6-342D-4030-ADC7-7CDEDC7F8B27",@"ff9b21ed-f47a-4961-a7db-7cf8d90d96c3"); // PTO Request:Start:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("066D899E-645D-40F8-BFD8-AFDFEEFCE183","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5",@"RockEntity"); // PTO Request:Start:Set Person:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue("3DA9A3CA-23AB-429F-A363-C9FD8F298966","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4",@"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.PtoAllocation.Guid}}
    {% endfor %}
{% endptorequest %}"); // PTO Request:Start:Set Pto Allocation:Lava
            RockMigrationHelper.AddActionTypeAttributeValue("3DA9A3CA-23AB-429F-A363-C9FD8F298966","F1924BDC-9B79-4018-9D4A-C3516C87A514",@"False"); // PTO Request:Start:Set Pto Allocation:Active
            RockMigrationHelper.AddActionTypeAttributeValue("3DA9A3CA-23AB-429F-A363-C9FD8F298966","431273C6-342D-4030-ADC7-7CDEDC7F8B27",@"c9e17918-4ecf-4029-a2f3-bf95d4eed4e3"); // PTO Request:Start:Set Pto Allocation:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("3DA9A3CA-23AB-429F-A363-C9FD8F298966","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5",@"RockEntity"); // PTO Request:Start:Set Pto Allocation:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue("941EE98E-3732-4037-A9C7-F3D95519E1EA","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4",@"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.StartDate}}
    {% endfor %}
{% endptorequest %}"); // PTO Request:Start:Set Start Date:Lava
            RockMigrationHelper.AddActionTypeAttributeValue("941EE98E-3732-4037-A9C7-F3D95519E1EA","F1924BDC-9B79-4018-9D4A-C3516C87A514",@"False"); // PTO Request:Start:Set Start Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue("941EE98E-3732-4037-A9C7-F3D95519E1EA","431273C6-342D-4030-ADC7-7CDEDC7F8B27",@"f6fb28d8-ae58-445e-9373-46cda04e6cc5"); // PTO Request:Start:Set Start Date:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("941EE98E-3732-4037-A9C7-F3D95519E1EA","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5",@"RockEntity"); // PTO Request:Start:Set Start Date:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue("7E6DA347-7333-4723-9DB6-F5E5B69F64B8","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4",@"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.Reason}}
    {% endfor %}
{% endptorequest %}"); // PTO Request:Start:Set Reason:Lava
            RockMigrationHelper.AddActionTypeAttributeValue("7E6DA347-7333-4723-9DB6-F5E5B69F64B8","F1924BDC-9B79-4018-9D4A-C3516C87A514",@"False"); // PTO Request:Start:Set Reason:Active
            RockMigrationHelper.AddActionTypeAttributeValue("7E6DA347-7333-4723-9DB6-F5E5B69F64B8","431273C6-342D-4030-ADC7-7CDEDC7F8B27",@"1136e804-a793-4081-9902-f8e7ed0cdd69"); // PTO Request:Start:Set Reason:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("7E6DA347-7333-4723-9DB6-F5E5B69F64B8","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5",@"RockEntity"); // PTO Request:Start:Set Reason:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue("790F40BC-0D26-42E6-B6EA-6761273653BC","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4",@"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.Hours}}
    {% endfor %}
{% endptorequest %}"); // PTO Request:Start:Set Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue("790F40BC-0D26-42E6-B6EA-6761273653BC","F1924BDC-9B79-4018-9D4A-C3516C87A514",@"False"); // PTO Request:Start:Set Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue("790F40BC-0D26-42E6-B6EA-6761273653BC","431273C6-342D-4030-ADC7-7CDEDC7F8B27",@"4180d0d3-a144-4974-b364-34292969c1a9"); // PTO Request:Start:Set Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("790F40BC-0D26-42E6-B6EA-6761273653BC","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5",@"RockEntity"); // PTO Request:Start:Set Hours:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue("E2FCD026-7AC1-45F0-93BD-110920E2369C","F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4",@"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {% case ptoAllocation.PtoRequestApprovalState %}
        {% when 'Pending' %}
            0
        {% when 'Approved' %}
            1
        {% else %}
            2
        {% endcase %}
    {% endfor %}
{% endptorequest %}"); // PTO Request:Start:Set Approval State:Lava
            RockMigrationHelper.AddActionTypeAttributeValue("E2FCD026-7AC1-45F0-93BD-110920E2369C","F1924BDC-9B79-4018-9D4A-C3516C87A514",@"False"); // PTO Request:Start:Set Approval State:Active
            RockMigrationHelper.AddActionTypeAttributeValue("E2FCD026-7AC1-45F0-93BD-110920E2369C","431273C6-342D-4030-ADC7-7CDEDC7F8B27",@"ffc543be-7b65-425b-a56c-ad441986fa2c"); // PTO Request:Start:Set Approval State:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("E2FCD026-7AC1-45F0-93BD-110920E2369C","F3E380BF-AAC8-4015-9ADC-0DF56B5462F5",@"RockEntity"); // PTO Request:Start:Set Approval State:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue("309F9CA5-6ADD-48F4-B615-5F367FE691FA","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // PTO Request:Start:Initial Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue("0B3A4061-593E-4403-9B6D-7BDC66C5AE84","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // PTO Request:Add Request:Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue("CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB","DD610F3E-2E83-41AE-B63B-9B163B87F82E",@"False"); // PTO Request:Add Request:Add Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue("CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB","EC01344E-61BF-4E22-88E3-36051BCAABE7",@"c9e17918-4ecf-4029-a2f3-bf95d4eed4e3"); // PTO Request:Add Request:Add Request:Allocation|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB","3C5F03BD-2CDD-41D7-9ED1-5AAC62AF733D",@"f6fb28d8-ae58-445e-9373-46cda04e6cc5"); // PTO Request:Add Request:Add Request:Start Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB","8304DE14-DA5C-41FD-BA30-026D91A492C7",@"286c5a76-9113-49c4-a209-078e856bd0b2"); // PTO Request:Add Request:Add Request:End Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB","858BFCA2-E793-446E-B146-87D5FC6783A0",@"4180d0d3-a144-4974-b364-34292969c1a9"); // PTO Request:Add Request:Add Request:Hours|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB","C6A51AEB-18CB-4591-BDF8-D4017CF38DCF",@"1136e804-a793-4081-9902-f8e7ed0cdd69"); // PTO Request:Add Request:Add Request:Reason|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB","080025FD-9E80-4158-8D7F-FBF3ED12A2E1",@"0"); // PTO Request:Add Request:Add Request:Approval State|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("FC3DA720-67E3-443D-B0CF-7983FD102BB4","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // PTO Request:Add Request:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("FC3DA720-67E3-443D-B0CF-7983FD102BB4","3327286F-C1A9-4624-949D-33E9F9049356",@"Completed"); // PTO Request:Add Request:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("84952D19-07EA-4C7E-A31F-7E5DA3C386B5","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // PTO Request:Modify Request:Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue("66613CA7-EF9B-456D-910C-8F90B85FBE1F","C957F777-F0FE-4D05-BB22-10D7C7A5C437",@"9fe62eb5-6604-416f-899b-f836c1dec7a5"); // PTO Request:Modify Request:Update Request:Existing Pto Request
            RockMigrationHelper.AddActionTypeAttributeValue("66613CA7-EF9B-456D-910C-8F90B85FBE1F","DD610F3E-2E83-41AE-B63B-9B163B87F82E",@"False"); // PTO Request:Modify Request:Update Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue("66613CA7-EF9B-456D-910C-8F90B85FBE1F","EC01344E-61BF-4E22-88E3-36051BCAABE7",@"c9e17918-4ecf-4029-a2f3-bf95d4eed4e3"); // PTO Request:Modify Request:Update Request:Allocation|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("66613CA7-EF9B-456D-910C-8F90B85FBE1F","3C5F03BD-2CDD-41D7-9ED1-5AAC62AF733D",@"f6fb28d8-ae58-445e-9373-46cda04e6cc5"); // PTO Request:Modify Request:Update Request:Start Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("66613CA7-EF9B-456D-910C-8F90B85FBE1F","8304DE14-DA5C-41FD-BA30-026D91A492C7",@"286c5a76-9113-49c4-a209-078e856bd0b2"); // PTO Request:Modify Request:Update Request:End Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("66613CA7-EF9B-456D-910C-8F90B85FBE1F","858BFCA2-E793-446E-B146-87D5FC6783A0",@"4180d0d3-a144-4974-b364-34292969c1a9"); // PTO Request:Modify Request:Update Request:Hours|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("66613CA7-EF9B-456D-910C-8F90B85FBE1F","C6A51AEB-18CB-4591-BDF8-D4017CF38DCF",@"1136e804-a793-4081-9902-f8e7ed0cdd69"); // PTO Request:Modify Request:Update Request:Reason|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("66613CA7-EF9B-456D-910C-8F90B85FBE1F","080025FD-9E80-4158-8D7F-FBF3ED12A2E1",@"0"); // PTO Request:Modify Request:Update Request:Approval State|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("7A841A72-2DFB-4514-84FA-B50DE2FECE6B","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // PTO Request:Modify Request:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("7A841A72-2DFB-4514-84FA-B50DE2FECE6B","3327286F-C1A9-4624-949D-33E9F9049356",@"Completed"); // PTO Request:Modify Request:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("ED092B4A-AB43-4836-A3C7-28CFF9F47DFB","1B976B41-C2D6-4C16-B781-6F5DD1AC2B69",@"9fe62eb5-6604-416f-899b-f836c1dec7a5"); // PTO Request:Delete Request:Delete Request:Existing Pto Request
            RockMigrationHelper.AddActionTypeAttributeValue("ED092B4A-AB43-4836-A3C7-28CFF9F47DFB","0883AE9A-24EE-433F-9925-C7A5C8BCB467",@"False"); // PTO Request:Delete Request:Delete Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue("B277B0BC-474C-4934-80B3-01383BB2D95E","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // PTO Request:Delete Request:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("B277B0BC-474C-4934-80B3-01383BB2D95E","3327286F-C1A9-4624-949D-33E9F9049356",@"Completed"); // PTO Request:Delete Request:Complete Workflow:Status|Status Attribute

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion
        }

        public void PtoRequestWorkflowOriginal()
        {
            #region FieldTypes


            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunSQL", "A41216D6-6FB0-4019-B222-2C29B4519CF4", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToCurrentPerson", "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQLQuery", "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", 0, @"", "F3B9908B-096F-460B-8320-122CF046D1F9" ); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A18C3143-0586-4565-9F36-E603BC674B4E" ); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Continue On Error", "ContinueOnError", "Should processing continue even if SQL Error occurs?", 3, @"False", "D992DB0A-B528-4833-ADCE-61C5BD9BD156" ); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Result Attribute", "ResultAttribute", "An optional attribute to set to the scaler result of SQL query.", 2, @"", "56997192-2545-4EA1-B5B2-313B04588984" ); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Parameters", "Parameters", "The parameters to supply to the SQL query. <span class='tip tip-lava'></span>", 1, @"", "45C97B53-B45E-44CD-8BD9-12DB8302BE38" ); // Rock.Workflow.Action.RunSQL:Parameters
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "FA7C685D-8636-41EF-9998-90FFF3998F76" ); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" ); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" ); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" ); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" ); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" ); // Rock.Workflow.Action.RunLava:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "PTO Requests", "fa fa-clock", "", "CD21D1FD-B9DB-4122-B252-86E8FD85CEEC", 0 ); // PTO Requests

            #endregion

            #region Request PTO

            RockMigrationHelper.UpdateWorkflowType( false, true, "Request PTO", "", "CD21D1FD-B9DB-4122-B252-86E8FD85CEEC", "Work", "fa fa-list-ol", 28800, true, 0, "25CA07A0-7662-42C8-9BD6-E2EDCE157795", 0 ); // Request PTO
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "", 0, @"", "CBA976C2-3B3B-4E06-B258-0896CF02163B", false ); // Request PTO:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Supervisor", "Supervisor", "", 1, @"", "2574FCAB-22F5-4D91-B217-84AFA2B4A022", false ); // Request PTO:Supervisor
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Supervisor Attribute", "SupervisorAttribute", "", 2, @"67afd5a3-28f3-404f-a3b8-88630061f294", "37BABFB2-60C3-4A21-82CA-154F731D4EDE", false ); // Request PTO:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDate", "", 3, @"", "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", false ); // Request PTO:Start Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDate", "", 4, @"", "2266B6AB-419F-45F8-880B-F8085F0BCD11", false ); // Request PTO:End Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "81FD3C9B-E22A-492F-9212-93546BBE6677", "PTO Type", "PTOType", "", 5, @"", "598E70BB-8A92-483B-834B-2FAF6572C8EE", false ); // Request PTO:PTO Type
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Hours / Day", "HoursDay", "", 6, @"", "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", false ); // Request PTO:Hours / Day
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Reason", "Reason", "", 7, @"", "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", false ); // Request PTO:Reason
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Hours Available", "HoursAvailable", "", 8, @"", "593E3676-C196-4265-8BED-7199C9E94833", false ); // Request PTO:Hours Available
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Error Message", "ErrorMessage", "", 9, @"", "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", false ); // Request PTO:Error Message
            RockMigrationHelper.AddAttributeQualifier( "CBA976C2-3B3B-4E06-B258-0896CF02163B", "EnableSelfSelection", @"False", "C85491DE-C2D4-4553-809A-2972927C6A32" ); // Request PTO:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "2574FCAB-22F5-4D91-B217-84AFA2B4A022", "EnableSelfSelection", @"False", "F52FA34A-9CA5-4F7B-88C1-0A343654D23A" ); // Request PTO:Supervisor:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "37BABFB2-60C3-4A21-82CA-154F731D4EDE", "allowmultiple", @"False", "C7BE1893-9D8D-4418-A552-C1D2BA1F6C7A" ); // Request PTO:Supervisor Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier( "37BABFB2-60C3-4A21-82CA-154F731D4EDE", "entitytype", @"72657ed8-d16e-492e-ac12-144c5e7567e7", "836EF542-5A5D-4A33-9E19-38C630BF0CCE" ); // Request PTO:Supervisor Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier( "37BABFB2-60C3-4A21-82CA-154F731D4EDE", "qualifierColumn", @"", "3983E5D1-4E68-457B-8804-89EF4B61C5D2" ); // Request PTO:Supervisor Attribute:qualifierColumn
            RockMigrationHelper.AddAttributeQualifier( "37BABFB2-60C3-4A21-82CA-154F731D4EDE", "qualifierValue", @"", "BBA0FDA4-3F22-48CC-B456-5EB8E859401F" ); // Request PTO:Supervisor Attribute:qualifierValue
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "datePickerControlType", @"Date Picker", "56E069BD-56BF-46B2-8B60-74916951548F" ); // Request PTO:Start Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "displayCurrentOption", @"False", "D386A292-C400-41C5-966D-4A1E62DD6650" ); // Request PTO:Start Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "displayDiff", @"False", "925B2F43-23D4-4493-923B-30A1DA7D7AFC" ); // Request PTO:Start Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "format", @"", "72BFAE7E-D8F9-4A86-A9D9-697731D36661" ); // Request PTO:Start Date:format
            RockMigrationHelper.AddAttributeQualifier( "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", "futureYearCount", @"", "67CDD506-7DF1-46A9-8621-ACE3E5701063" ); // Request PTO:Start Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "datePickerControlType", @"Date Picker", "B00B19DF-CD67-443B-A7AD-E4DBF9D5076E" ); // Request PTO:End Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "displayCurrentOption", @"False", "174A12BE-3020-4F58-92FD-7187EE8C1CB1" ); // Request PTO:End Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "displayDiff", @"False", "65B15184-45D2-4FE1-A062-5C12FD26B8B6" ); // Request PTO:End Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "format", @"", "DB4CFACB-7E8A-44EC-934C-996E1E58A114" ); // Request PTO:End Date:format
            RockMigrationHelper.AddAttributeQualifier( "2266B6AB-419F-45F8-880B-F8085F0BCD11", "futureYearCount", @"", "C9AC4204-85FD-47BE-9770-09EF88367831" ); // Request PTO:End Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", "fieldtype", @"ddl", "FCA3B571-0B27-47E6-8C12-A72801D67BD2" ); // Request PTO:Hours / Day:fieldtype
            RockMigrationHelper.AddAttributeQualifier( "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", "repeatColumns", @"", "C4ED813D-68D0-414D-A6AE-37C3513D369B" ); // Request PTO:Hours / Day:repeatColumns
            RockMigrationHelper.AddAttributeQualifier( "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", "values", @"0.5,1.0,1.5,2.0,2.5,3.0,3.5,4.0,4.5,5.0,5.5,6.0,6.5,7.0,7.5,8.0", "53CA4D9F-B589-40B4-87E3-875A4CCD4DBF" ); // Request PTO:Hours / Day:values
            RockMigrationHelper.AddAttributeQualifier( "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", "allowhtml", @"False", "47E2EC0C-C8E7-47FF-BE05-1EE1C22BC1B1" ); // Request PTO:Reason:allowhtml
            RockMigrationHelper.AddAttributeQualifier( "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", "maxcharacters", @"", "E419467C-30A9-466B-8F0A-4BB739D21617" ); // Request PTO:Reason:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", "numberofrows", @"", "A5705060-77EB-4ABF-878B-5D695933BAFF" ); // Request PTO:Reason:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", "showcountdown", @"False", "471D4E90-D2CA-42EA-AF78-876F1C733D1C" ); // Request PTO:Reason:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "593E3676-C196-4265-8BED-7199C9E94833", "ispassword", @"False", "7F3E549D-CCBB-4B32-BD2D-F0AB49706147" ); // Request PTO:Hours Available:ispassword
            RockMigrationHelper.AddAttributeQualifier( "593E3676-C196-4265-8BED-7199C9E94833", "maxcharacters", @"", "3B078BC4-9A31-450E-AE3F-B81B398843C7" ); // Request PTO:Hours Available:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "593E3676-C196-4265-8BED-7199C9E94833", "showcountdown", @"False", "8ABE1F0C-D115-4A58-9C0B-1ADB646C370B" ); // Request PTO:Hours Available:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", "ispassword", @"False", "55722CE9-732E-45F9-B30D-BD61C0A3D3A5" ); // Request PTO:Error Message:ispassword
            RockMigrationHelper.AddAttributeQualifier( "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", "maxcharacters", @"", "5BC184CA-20FA-4E09-8414-0AD043149A33" ); // Request PTO:Error Message:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", "showcountdown", @"False", "8A395D11-5CF2-420C-8F76-3B9B6331084A" ); // Request PTO:Error Message:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", true, "Start", "", true, 0, "4429A350-7D7D-4D4D-BF19-A584020CEEFE" ); // Request PTO:Start
            RockMigrationHelper.UpdateWorkflowActivityType( "25CA07A0-7662-42C8-9BD6-E2EDCE157795", true, "Validate Request", "", false, 1, "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04" ); // Request PTO:Validate Request
            RockMigrationHelper.UpdateWorkflowActionForm( @"{% assign errorMsg = Workflow | Attribute:'ErrorMessage' | Trim %}
{% if errorMsg and errorMsg != '' %}
    <div class='alert alert-warning'>{{ errorMsg }}</div>
{% endif %}
<div id='vbsSlotDiv' class='panel panel-block'>
            <div class=""panel-heading panel-follow clearfix"">
                <h1 class=""panel-title"">
                    Available Hours
                </h1>
            </div>
            <div class=""panel-body"">
                <div class='row'>
                    <div class='col-md-3'>
                        <b>Type</b>
                    </div>
                    <div class='col-md-3'>
                    <b> Accrued Time</b>
                    </div>
                    <div class='col-md-3'>
                        <b>Time Spent</b>
                    </div>
                    <div class='col-md-3'>
                        <b>Time Remaining</b>
                    </div>
                </div>
                
                    <div class='row'>
                        <div class='col-md-3'>
                            <b>PTO</b>
                        </div>
                        <div class='col-md-3'>
                            8
                        </div>
                        <div class='col-md-3'>
                            5
                        </div>
                        <div class='col-md-3'>
                        3
                        </div>
                    </div>
                
                    <div class='row'>
                        <div class='col-md-3'>
                            <b>Sick Time</b>
                        </div>
                        <div class='col-md-3'>
                            8
                        </div>
                        <div class='col-md-3'>
                            5
                        </div>
                        <div class='col-md-3'>
                        3
                        </div>
                    </div>
                
                    <div class='row'>
                        <div class='col-md-3'>
                            <b>Sabbatical</b>
                        </div>
                        <div class='col-md-3'>
                            8
                        </div>
                        <div class='col-md-3'>
                            5
                        </div>
                        <div class='col-md-3'>
                        3
                        </div>
                    </div>
            </div>
        </div>", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04^Your information has been submitted successfully.|", "", false, "", "9A35017A-63F2-4367-8320-B29CE911B136" ); // Request PTO:Start:Form
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "CBA976C2-3B3B-4E06-B258-0896CF02163B", 0, true, true, false, false, @"<div class='row'><div class='col-md-6'>", @"</div>", "D130BA8F-547E-41C9-92C2-8321586C0F31" ); // Request PTO:Start:Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "2574FCAB-22F5-4D91-B217-84AFA2B4A022", 1, true, true, false, false, @"<div class='col-md-6'>", @"</div></div>", "CBC760C0-BAC5-45AB-A9BB-09F011CD6263" ); // Request PTO:Start:Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "37BABFB2-60C3-4A21-82CA-154F731D4EDE", 2, false, true, false, false, @"", @"", "E761C590-FF7A-40AD-B3DD-B3D0D679D8AF" ); // Request PTO:Start:Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "8F85DA13-2602-4438-B58A-CFAA88FBFDDD", 3, true, false, true, false, @"<div class='row'><div class='col-md-6'>", @"</div>", "2673F3AA-62CF-4F4A-9CAA-307438E4D2BC" ); // Request PTO:Start:Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "2266B6AB-419F-45F8-880B-F8085F0BCD11", 4, true, false, true, false, @"<div class='col-md-6'>", @"</div></div>", "A05B2AD8-2A2F-49FE-9F62-8F8A63D2D979" ); // Request PTO:Start:Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "598E70BB-8A92-483B-834B-2FAF6572C8EE", 5, true, false, true, false, @"<div class='row'><div class='col-md-6'>", @"</div>", "0343AD88-6F3E-4739-9412-595AFE5585BD" ); // Request PTO:Start:Form:PTO Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "DBC30BCF-E5E8-4F4B-BEB0-7D1C226C317A", 6, true, false, true, false, @"<div class='col-md-6'>", @"</div></div>", "FADD2817-B4FC-4C85-B43A-842245AF3C92" ); // Request PTO:Start:Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "66868B80-D1EB-417F-BCAE-F8D75E68FC8C", 7, true, false, true, false, @"", @"", "46F3D5C2-A011-49DF-A704-ACFC01FF431F" ); // Request PTO:Start:Form:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "593E3676-C196-4265-8BED-7199C9E94833", 8, false, true, false, false, @"", @"", "749FB89D-305F-4781-9812-624542251EB4" ); // Request PTO:Start:Form:Hours Available
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "9A35017A-63F2-4367-8320-B29CE911B136", "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", 9, false, true, false, false, @"", @"", "23CFD428-FE05-4D31-B650-E2FE9547E218" ); // Request PTO:Start:Form:Error Message
            RockMigrationHelper.UpdateWorkflowActionType( "4429A350-7D7D-4D4D-BF19-A584020CEEFE", "Set Person", 0, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "CBA976C2-3B3B-4E06-B258-0896CF02163B", 32, "", "5773DFF8-5E54-4AD2-93FD-074097E7C1D8" ); // Request PTO:Start:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "4429A350-7D7D-4D4D-BF19-A584020CEEFE", "Set Supervisor", 1, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "52D4E40D-B4B8-4521-91FD-8446F11B40CD" ); // Request PTO:Start:Set Supervisor
            RockMigrationHelper.UpdateWorkflowActionType( "4429A350-7D7D-4D4D-BF19-A584020CEEFE", "Form", 2, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, true, "9A35017A-63F2-4367-8320-B29CE911B136", "", 1, "", "E5DD2A27-30F5-42D4-9AE6-C437C4D3EDA6" ); // Request PTO:Start:Form
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Clear Error Message", 0, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "D1BA6262-931B-4B91-9C56-E59D913D1E20" ); // Request PTO:Validate Request:Clear Error Message
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Get Hours Available", 1, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F" ); // Request PTO:Validate Request:Get Hours Available
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Validate Entry", 2, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "2B143418-0DC2-468C-BDB9-524DCC310C0E" ); // Request PTO:Validate Request:Validate Entry
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Capture Information Again", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "2C4F2FBE-6E72-48FA-B26E-B1C9D4A11DA1", 64, "", "A3E36BF8-A808-42C2-91EF-AAE00F34F632" ); // Request PTO:Validate Request:Capture Information Again
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Save Pto Request", 4, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "3902166F-2243-4543-9B66-235858D4F036" ); // Request PTO:Validate Request:Save Pto Request
            RockMigrationHelper.UpdateWorkflowActionType( "FDEFD76B-AEB9-46FB-927A-4E1AAB91CE04", "Launch PTO Approval", 5, "BC21E57A-1477-44B3-A7C2-61A806118945", true, true, "", "", 1, "", "B0DC7B15-7B09-493C-80AA-BA456190786B" ); // Request PTO:Validate Request:Launch PTO Approval
            RockMigrationHelper.AddActionTypeAttributeValue( "5773DFF8-5E54-4AD2-93FD-074097E7C1D8", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Request PTO:Start:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5773DFF8-5E54-4AD2-93FD-074097E7C1D8", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"cba976c2-3b3b-4e06-b258-0896cf02163b" ); // Request PTO:Start:Set Person:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "52D4E40D-B4B8-4521-91FD-8446F11B40CD", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign supervisorAttribute = Workflow | Attribute:'SupervisorAttribute','Object' %}
{% assign person = Workflow | Attribute:'Person','Object' %}
{% assign supervisor = person | Attribute:supervisorAttribute.Key,'Object' %}
{{supervisor.PrimaryAlias.Guid}}" ); // Request PTO:Start:Set Supervisor:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "52D4E40D-B4B8-4521-91FD-8446F11B40CD", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Start:Set Supervisor:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "52D4E40D-B4B8-4521-91FD-8446F11B40CD", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"2574fcab-22f5-4d91-b217-84afa2b4a022" ); // Request PTO:Start:Set Supervisor:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E5DD2A27-30F5-42D4-9AE6-C437C4D3EDA6", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Request PTO:Start:Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D1BA6262-931B-4B91-9C56-E59D913D1E20", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{{ }}" ); // Request PTO:Validate Request:Clear Error Message:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "D1BA6262-931B-4B91-9C56-E59D913D1E20", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Validate Request:Clear Error Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D1BA6262-931B-4B91-9C56-E59D913D1E20", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"2c4f2fbe-6e72-48fa-b26e-b1c9d4a11da1" ); // Request PTO:Validate Request:Clear Error Message:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F", "F3B9908B-096F-460B-8320-122CF046D1F9", @"Select 4

/* DECLARE @WorkflowId int = {{ Workflow.Id }}
DECLARE @WorkflowTypeId int = {{ Workflow.WorkflowTypeId }}
DECLARE @CampusValue varchar(36) = '{{ Workflow | Attribute:'Campus','RawValue' }}'
DECLARE @DateValue datetime = '{{ Workflow | Attribute:'Date' | Date:'yyyy-MM-dd' }}'
DECLARE @LocationValue varchar(36) = '{{ Workflow | Attribute:'Location','RawValue' }}'
DECLARE @ScheduleValue varchar(36) = '{{ Workflow | Attribute:'Schedule','RawValue' }}'

DECLARE @Capacity INT = ( SELECT TOP 1 [FirmRoomThreshold] FROM [Location] WHERE [Guid] = @LocationValue )
IF @Capacity IS NULL
BEGIN
	SELECT 999
END
ELSE
BEGIN

	DECLARE @WorkflowEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Workflow' )
	DECLARE @CampusAttrId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'Campus' )
	DECLARE @DateAttrId int = ( SELECT TOP 1 [Id]  FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'Date' )
	DECLARE @LocationAttrId int = ( SELECT TOP 1 [Id]  FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'Location' )
	DECLARE @ScheduleAttrId int = ( SELECT TOP 1 [Id]  FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'Schedule' )
	DECLARE @HowManyAttrId int = ( SELECT TOP 1 [Id]  FROM [Attribute] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND [EntityTypeQualifierColumn] = 'WorkflowTypeId' AND [EntityTypeQualifierValue] = @WorkflowTypeId AND [Key] = 'HowMany' )

	DECLARE @SpotsReserved int = (
		SELECT SUM(HV.[ValueAsNumeric]) 
		FROM [Workflow] W
		INNER JOIN [AttributeValue] CV ON CV.[EntityId] = W.[Id] AND CV.[AttributeId] = @CampusAttrId
		INNER JOIN [AttributeValue] DV ON DV.[EntityId] = W.[Id] AND DV.[AttributeId] = @DateAttrId
		INNER JOIN [AttributeValue] LV ON LV.[EntityId] = W.[Id] AND LV.[AttributeId] = @LocationAttrId
		INNER JOIN [AttributeValue] SV ON SV.[EntityId] = W.[Id] AND SV.[AttributeId] = @ScheduleAttrId
		INNER JOIN [AttributeValue] HV ON HV.[EntityId] = W.[Id] AND HV.[AttributeId] = @HowManyAttrId
		WHERE W.[WorkflowTypeId] = @WorkflowTypeId
		AND W.[Id] <> @WorkflowId
		AND CV.[Value] = @CampusValue
		AND DV.[ValueAsDateTime] = @DateValue
		AND LV.[Value] = @LocationValue
		AND SV.[Value] = @ScheduleValue 
	)

	IF @SpotsReserved IS NULL SET @SpotsReserved = 0

	SELECT @Capacity - @SpotsReserved 

END
*/" ); // Request PTO:Validate Request:Get Hours Available:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue( "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // Request PTO:Validate Request:Get Hours Available:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F", "56997192-2545-4EA1-B5B2-313B04588984", @"593e3676-c196-4265-8bed-7199c9e94833" ); // Request PTO:Validate Request:Get Hours Available:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "6F0A6C57-5733-4431-9F82-BE1CB1AEB15F", "D992DB0A-B528-4833-ADCE-61C5BD9BD156", @"True" ); // Request PTO:Validate Request:Get Hours Available:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue( "2B143418-0DC2-468C-BDB9-524DCC310C0E", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign hoursDay = Workflow | Attribute:'HoursDay' | AsInteger %}
{% assign hoursAvailable = Workflow | Attribute:'HoursAvailable' | AsInteger %}
{%- if hoursDay > hoursAvailable -%}
    {% capture msg %}{{ msg }}
        <p><strong>Hours Not Available.</strong> There are only {{ hoursAvailable }} hours available.</p>
    {% endcapture %}
{%- endif -%}
{{ msg }}" ); // Request PTO:Validate Request:Validate Entry:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "2B143418-0DC2-468C-BDB9-524DCC310C0E", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Validate Request:Validate Entry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2B143418-0DC2-468C-BDB9-524DCC310C0E", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"2c4f2fbe-6e72-48fa-b26e-b1c9d4a11da1" ); // Request PTO:Validate Request:Validate Entry:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A3E36BF8-A808-42C2-91EF-AAE00F34F632", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Request PTO:Validate Request:Capture Information Again:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A3E36BF8-A808-42C2-91EF-AAE00F34F632", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"4429A350-7D7D-4D4D-BF19-A584020CEEFE" ); // Request PTO:Validate Request:Capture Information Again:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "3902166F-2243-4543-9B66-235858D4F036", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"test" ); // Request PTO:Validate Request:Save Pto Request:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "3902166F-2243-4543-9B66-235858D4F036", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Validate Request:Save Pto Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B0DC7B15-7B09-493C-80AA-BA456190786B", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"test" ); // Request PTO:Validate Request:Launch PTO Approval:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "B0DC7B15-7B09-493C-80AA-BA456190786B", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // Request PTO:Validate Request:Launch PTO Approval:Active

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion
        }
        public void ModifyPtoAllocationWorkflow()
        {
            #region FieldTypes


            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Workflow.Action.PtoAllocationDelete", "4149F221-C080-4042-A8D1-E7FE054A0646", false, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate", "76AFA08A-214A-40A8-BFB6-E60D62110286", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4149F221-C080-4042-A8D1-E7FE054A0646", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "FD7D231F-1B61-4EDD-A3D7-CB9C315D62A0" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationDelete:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4149F221-C080-4042-A8D1-E7FE054A0646", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Existing Pto Allocation", "PTO_ALLOCATION_ATTRIBUTE_KEY", "The Pto Allocation to update.", 0, @"", "FA37CBA4-938E-4465-993C-466563D88EF7" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationDelete:Existing Pto Allocation
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4149F221-C080-4042-A8D1-E7FE054A0646", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C6EFE826-6CD1-4853-BC13-7EC1E43EA071" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationDelete:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "1FEC991C-1ADE-468F-9AF0-0AF72F0D1728" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Existing Pto Allocation", "PTO_ALLOCATION_ATTRIBUTE_KEY", "The Pto Allocation to update.", 0, @"", "1B16E29B-81C4-4CCC-999F-A4AAF356094F" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Existing Pto Allocation
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person", "PERSON_KEY", "The person or an attribute that contains the person of the Pto Allocation. <span class='tip tip-lava'></span>", 1, @"", "8E98C7E4-E05D-4A1F-B835-F3F553D31D21" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Pto Type", "PTO_TYPE_KEY", "The Pto Type or an attribute that contains the Pto Type of the Pto Allocation. <span class='tip tip-lava'></span>", 2, @"", "F8E53739-1380-46BA-8E7A-B826880ADD8C" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Pto Type
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "End Date|Attribute Value", "ENDDATE_KEY", "The end date or an attribute that contains the end date of the Pto Allocation. <span class='tip tip-lava'></span>", 4, @"", "4D254EB2-C56C-4FC3-AE6C-290F4EA996F7" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:End Date|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Hours|Attribute Value", "HOURS_KEY", "The hours or an attribute that contains the hours of the Pto Allocation. <span class='tip tip-lava'></span>", 5, @"", "8BC8874D-FC40-4B79-A881-950A50ED6507" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Hours|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Start Date|Attribute Value", "STARTDATE_KEY", "The start date or an attribute that contains the start date of the Pto Allocation. <span class='tip tip-lava'></span>", 3, @"", "7B45568D-3074-47F9-89ED-F37A602F0997" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Start Date|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Attribute Value", "PTO_STATUS_KEY", "The status or an attribute that contains the status of the Pto Allocation. <span class='tip tip-lava'></span>", 6, @"", "633E7884-6376-45C1-91BC-FC21A10F64EC" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Status|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Source", "SOURCE_TYPE_KEY", "The source of the Pto Request", 7, @"Manual", "FCFF0CA6-7BBD-4107-8341-72BB35DCEDD1" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Source
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "76AFA08A-214A-40A8-BFB6-E60D62110286", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C0E76554-67F4-4C91-969F-19ECE2207BE6" ); // com.bemaservices.HrManagement.Workflow.Action.PtoAllocationUpdate:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" ); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" ); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" ); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" ); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" ); // Rock.Workflow.Action.RunLava:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "3327286F-C1A9-4624-949D-33E9F9049356" ); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "PTO Requests", "fa fa-clock", "", "CD21D1FD-B9DB-4122-B252-86E8FD85CEEC", 0 ); // PTO Requests

            #endregion

            #region PTO Allocation

            RockMigrationHelper.UpdateWorkflowType( false, true, "PTO Allocation", "", "CD21D1FD-B9DB-4122-B252-86E8FD85CEEC", "Work", "fa fa-list-ol", 28800, true, 0, "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", 0 ); // PTO Allocation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "", 0, @"", "1B86D4E1-A085-485A-B4C0-3F17EE69F806", false ); // PTO Allocation:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", "81FD3C9B-E22A-492F-9212-93546BBE6677", "PTO Type", "PTOType", "", 1, @"", "B63296E7-83C0-41AD-AA98-B07FBB7FD25A", false ); // PTO Allocation:PTO Type
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDate", "", 2, @"", "F827FF84-4229-4A03-B8D0-661FD835FC80", false ); // PTO Allocation:Start Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDate", "", 3, @"", "BB8CC540-61A8-43E5-832C-D4880F13F053", false ); // PTO Allocation:End Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Hours", "Hours", "", 4, @"", "FFC7E416-BE23-4D5A-9801-4C4A02F47E87", false ); // PTO Allocation:Hours
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", "C3206242-E213-4038-99EB-1A563B375997", "PTO Allocation", "PTOAllocation", "", 5, @"", "A2EED015-29F5-4974-AD60-1539C100F6FB", false ); // PTO Allocation:PTO Allocation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Pto Allocation Status", "PtoAllocationStatus", "", 6, @"2", "CB3BD726-E327-43C8-8C60-02D290337A88", false ); // PTO Allocation:Pto Allocation Status
            RockMigrationHelper.AddAttributeQualifier( "1B86D4E1-A085-485A-B4C0-3F17EE69F806", "EnableSelfSelection", @"False", "27C05D2B-78BF-434B-A607-678AD74DFCE7" ); // PTO Allocation:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "F827FF84-4229-4A03-B8D0-661FD835FC80", "datePickerControlType", @"Date Picker", "59DAEB0F-B627-4761-B09F-27DA4DA3411C" ); // PTO Allocation:Start Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "F827FF84-4229-4A03-B8D0-661FD835FC80", "displayCurrentOption", @"False", "9933643A-D88E-47BF-A66E-A1C58343C402" ); // PTO Allocation:Start Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "F827FF84-4229-4A03-B8D0-661FD835FC80", "displayDiff", @"False", "2BCBFD59-03D0-416D-93A7-6C4EEF8EAAB1" ); // PTO Allocation:Start Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "F827FF84-4229-4A03-B8D0-661FD835FC80", "format", @"", "1E34F077-03D2-412A-B51C-71D127DAA5F9" ); // PTO Allocation:Start Date:format
            RockMigrationHelper.AddAttributeQualifier( "F827FF84-4229-4A03-B8D0-661FD835FC80", "futureYearCount", @"", "4960226B-D88D-4D5D-950A-FDC808C44F48" ); // PTO Allocation:Start Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "BB8CC540-61A8-43E5-832C-D4880F13F053", "datePickerControlType", @"Date Picker", "11337DB1-3604-49F4-B6FE-565F2AD764BF" ); // PTO Allocation:End Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "BB8CC540-61A8-43E5-832C-D4880F13F053", "displayCurrentOption", @"False", "D0B6E83D-306A-41EA-9B0F-A8E9A07334E7" ); // PTO Allocation:End Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "BB8CC540-61A8-43E5-832C-D4880F13F053", "displayDiff", @"False", "C5A6520A-08FF-4545-99BB-C7CAAB77A945" ); // PTO Allocation:End Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "BB8CC540-61A8-43E5-832C-D4880F13F053", "format", @"", "A95F86F1-72A3-445C-9141-130DD723C278" ); // PTO Allocation:End Date:format
            RockMigrationHelper.AddAttributeQualifier( "BB8CC540-61A8-43E5-832C-D4880F13F053", "futureYearCount", @"", "07AE9841-BA73-4E8C-ACE3-B14E6C397EE3" ); // PTO Allocation:End Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "A2EED015-29F5-4974-AD60-1539C100F6FB", "fieldtype", @"ddl", "750ACFCF-088C-4499-806D-42C041BA8659" ); // PTO Allocation:PTO Allocation:fieldtype
            RockMigrationHelper.AddAttributeQualifier( "A2EED015-29F5-4974-AD60-1539C100F6FB", "repeatColumns", @"", "F1671AC6-D849-466D-B1A6-07FB80EAEA0E" ); // PTO Allocation:PTO Allocation:repeatColumns
            RockMigrationHelper.AddAttributeQualifier( "CB3BD726-E327-43C8-8C60-02D290337A88", "fieldtype", @"ddl", "E1B7B7DB-1BD4-485A-BEA9-DA867DEE5A8C" ); // PTO Allocation:Pto Allocation Status:fieldtype
            RockMigrationHelper.AddAttributeQualifier( "CB3BD726-E327-43C8-8C60-02D290337A88", "repeatColumns", @"", "A1BC82ED-2BB4-4877-A79E-7E519AAE963E" ); // PTO Allocation:Pto Allocation Status:repeatColumns
            RockMigrationHelper.AddAttributeQualifier( "CB3BD726-E327-43C8-8C60-02D290337A88", "values", @"0^Inactive, 1^Active, 2^Pending", "CA0BA8F4-80B5-4DF8-BC83-F5C461F3170A" ); // PTO Allocation:Pto Allocation Status:values
            RockMigrationHelper.UpdateWorkflowActivityType( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", true, "Start", "", true, 0, "A26BDC5B-014B-471F-83CB-2E91D70CDF8C" ); // PTO Allocation:Start
            RockMigrationHelper.UpdateWorkflowActivityType( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", true, "Add Allocation", "", false, 1, "D17F5F49-3781-484D-AAF4-ED585B6F4ECA" ); // PTO Allocation:Add Allocation
            RockMigrationHelper.UpdateWorkflowActivityType( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", true, "Modify Allocation", "", false, 2, "D120BA7F-A53F-4D43-8839-D7A92A0CF239" ); // PTO Allocation:Modify Allocation
            RockMigrationHelper.UpdateWorkflowActivityType( "8D6FB4EC-E3DE-4E0C-828E-2DAF165AB390", true, "Delete Allocation", "", false, 3, "93C33DC9-183C-488B-916B-781960CE9EDB" ); // PTO Allocation:Delete Allocation
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Modify^fdc397cd-8b4a-436e-bea1-bce2e6717c03^D120BA7F-A53F-4D43-8839-D7A92A0CF239^Your information has been submitted successfully.|Delete^638beee0-2f8f-4706-b9a4-5bab70386697^93C33DC9-183C-488B-916B-781960CE9EDB^|", "", true, "", "6537A976-548A-4C0E-890C-C1D020C59B95" ); // PTO Allocation:Start:Initial Form
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Submit^^^Your information has been submitted successfully.", "", true, "", "B9F8FE83-6567-47D1-B261-9B396CEEE767" ); // PTO Allocation:Add Allocation:Form
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Submit^^^Your information has been submitted successfully.", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "3135E5A6-615E-4DFA-9C50-8FECE1D1F684" ); // PTO Allocation:Modify Allocation:Form
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6537A976-548A-4C0E-890C-C1D020C59B95", "1B86D4E1-A085-485A-B4C0-3F17EE69F806", 2, true, true, false, false, @"", @"", "DACE90E5-B1D1-4BAA-84D2-DFB189342822" ); // PTO Allocation:Start:Initial Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6537A976-548A-4C0E-890C-C1D020C59B95", "B63296E7-83C0-41AD-AA98-B07FBB7FD25A", 3, true, true, false, false, @"", @"", "F986BD65-75F6-49D9-B4BE-57E4C399C0B4" ); // PTO Allocation:Start:Initial Form:PTO Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6537A976-548A-4C0E-890C-C1D020C59B95", "F827FF84-4229-4A03-B8D0-661FD835FC80", 4, true, true, false, false, @"", @"", "172F415D-CAEF-435D-A1E1-9B1C4E791AC0" ); // PTO Allocation:Start:Initial Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6537A976-548A-4C0E-890C-C1D020C59B95", "BB8CC540-61A8-43E5-832C-D4880F13F053", 5, true, true, false, false, @"", @"", "4619D3BC-8FE3-46AC-8EAA-994B6819AADE" ); // PTO Allocation:Start:Initial Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6537A976-548A-4C0E-890C-C1D020C59B95", "FFC7E416-BE23-4D5A-9801-4C4A02F47E87", 6, true, true, false, false, @"", @"", "F7E4F2E9-99E1-4BBC-A91D-60A1836E5522" ); // PTO Allocation:Start:Initial Form:Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6537A976-548A-4C0E-890C-C1D020C59B95", "A2EED015-29F5-4974-AD60-1539C100F6FB", 0, true, true, false, false, @"", @"", "2787704A-C908-44E8-BB1A-75466FCF959F" ); // PTO Allocation:Start:Initial Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6537A976-548A-4C0E-890C-C1D020C59B95", "CB3BD726-E327-43C8-8C60-02D290337A88", 1, true, true, false, false, @"", @"", "A2943F24-2892-48ED-987F-BD4FDF65EA80" ); // PTO Allocation:Start:Initial Form:Pto Allocation Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B9F8FE83-6567-47D1-B261-9B396CEEE767", "1B86D4E1-A085-485A-B4C0-3F17EE69F806", 0, true, false, true, false, @"", @"", "A17E33BD-0E88-4D51-AA13-ED13393EE983" ); // PTO Allocation:Add Allocation:Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B9F8FE83-6567-47D1-B261-9B396CEEE767", "B63296E7-83C0-41AD-AA98-B07FBB7FD25A", 1, true, false, true, false, @"", @"", "368124DC-0B3D-4353-9E64-0C7910ED513F" ); // PTO Allocation:Add Allocation:Form:PTO Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B9F8FE83-6567-47D1-B261-9B396CEEE767", "F827FF84-4229-4A03-B8D0-661FD835FC80", 2, true, false, true, false, @"", @"", "6F92469B-7B41-47EB-B6D8-701388E72173" ); // PTO Allocation:Add Allocation:Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B9F8FE83-6567-47D1-B261-9B396CEEE767", "BB8CC540-61A8-43E5-832C-D4880F13F053", 3, true, false, false, false, @"", @"", "5D9A8032-83AA-4949-83C3-EA19CDD8BA3D" ); // PTO Allocation:Add Allocation:Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B9F8FE83-6567-47D1-B261-9B396CEEE767", "FFC7E416-BE23-4D5A-9801-4C4A02F47E87", 4, true, false, true, false, @"", @"", "1A1118FD-A6B2-4620-81AB-A61027EC34EC" ); // PTO Allocation:Add Allocation:Form:Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B9F8FE83-6567-47D1-B261-9B396CEEE767", "A2EED015-29F5-4974-AD60-1539C100F6FB", 5, false, true, false, false, @"", @"", "5CC78E5E-3337-4353-AC8C-096C912F62C4" ); // PTO Allocation:Add Allocation:Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "B9F8FE83-6567-47D1-B261-9B396CEEE767", "CB3BD726-E327-43C8-8C60-02D290337A88", 6, true, false, true, false, @"", @"", "EE92CF13-B21A-4271-8D57-20EF37803BD9" ); // PTO Allocation:Add Allocation:Form:Pto Allocation Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3135E5A6-615E-4DFA-9C50-8FECE1D1F684", "1B86D4E1-A085-485A-B4C0-3F17EE69F806", 1, true, false, true, false, @"", @"", "492BAED0-3F8A-40B2-B772-09F7A77D7C83" ); // PTO Allocation:Modify Allocation:Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3135E5A6-615E-4DFA-9C50-8FECE1D1F684", "B63296E7-83C0-41AD-AA98-B07FBB7FD25A", 2, true, false, true, false, @"", @"", "456DDC5A-FED6-4D0B-8B04-7B22C3E9FA5E" ); // PTO Allocation:Modify Allocation:Form:PTO Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3135E5A6-615E-4DFA-9C50-8FECE1D1F684", "F827FF84-4229-4A03-B8D0-661FD835FC80", 3, true, false, true, false, @"", @"", "6667D9E9-01F2-4A24-BE99-812D1EA2D030" ); // PTO Allocation:Modify Allocation:Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3135E5A6-615E-4DFA-9C50-8FECE1D1F684", "BB8CC540-61A8-43E5-832C-D4880F13F053", 4, true, false, false, false, @"", @"", "DFFD4197-8400-4129-B10E-DC232D81DD91" ); // PTO Allocation:Modify Allocation:Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3135E5A6-615E-4DFA-9C50-8FECE1D1F684", "FFC7E416-BE23-4D5A-9801-4C4A02F47E87", 5, true, false, true, false, @"", @"", "DAD7B401-F5C9-4A02-ACF4-A3C459BC4260" ); // PTO Allocation:Modify Allocation:Form:Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3135E5A6-615E-4DFA-9C50-8FECE1D1F684", "A2EED015-29F5-4974-AD60-1539C100F6FB", 0, true, true, false, false, @"", @"", "3724B44E-A0A7-4C76-8646-1FA19F1D6276" ); // PTO Allocation:Modify Allocation:Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3135E5A6-615E-4DFA-9C50-8FECE1D1F684", "CB3BD726-E327-43C8-8C60-02D290337A88", 6, true, false, true, false, @"", @"", "8D2BB4B3-DABD-4A03-89F7-33C7AFB26CFF" ); // PTO Allocation:Modify Allocation:Form:Pto Allocation Status
            RockMigrationHelper.UpdateWorkflowActionType( "A26BDC5B-014B-471F-83CB-2E91D70CDF8C", "Activate Add Activity if no existing PTO Allocation", 0, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "A2EED015-29F5-4974-AD60-1539C100F6FB", 32, "", "6F6F6B37-58F3-45F9-B587-C2CD95ED6012" ); // PTO Allocation:Start:Activate Add Activity if no existing PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionType( "A26BDC5B-014B-471F-83CB-2E91D70CDF8C", "Set Person", 1, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "C34CCCEE-CC83-44BA-AA48-37E90DF0F01F" ); // PTO Allocation:Start:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "A26BDC5B-014B-471F-83CB-2E91D70CDF8C", "Set Pto Type", 2, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "0C54332D-AD9B-46FA-A8A0-90AFCF958E96" ); // PTO Allocation:Start:Set Pto Type
            RockMigrationHelper.UpdateWorkflowActionType( "A26BDC5B-014B-471F-83CB-2E91D70CDF8C", "Set Start Date", 3, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "1CB81D02-0B42-4F08-B67A-054863616ACB" ); // PTO Allocation:Start:Set Start Date
            RockMigrationHelper.UpdateWorkflowActionType( "A26BDC5B-014B-471F-83CB-2E91D70CDF8C", "Set End Date", 4, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "9C6590AE-B995-4B11-991B-6E649C4E47D9" ); // PTO Allocation:Start:Set End Date
            RockMigrationHelper.UpdateWorkflowActionType( "A26BDC5B-014B-471F-83CB-2E91D70CDF8C", "Set Hours", 5, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "A61AD601-1C23-4E21-8F29-334337262AF2" ); // PTO Allocation:Start:Set Hours
            RockMigrationHelper.UpdateWorkflowActionType( "A26BDC5B-014B-471F-83CB-2E91D70CDF8C", "Set Status", 6, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "CC570F29-3639-4E73-AB0C-E0E44B52F00E" ); // PTO Allocation:Start:Set Status
            RockMigrationHelper.UpdateWorkflowActionType( "A26BDC5B-014B-471F-83CB-2E91D70CDF8C", "Initial Form", 7, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, true, "6537A976-548A-4C0E-890C-C1D020C59B95", "", 1, "", "DFAA70BC-0F5B-4177-8779-44D92022A42E" ); // PTO Allocation:Start:Initial Form
            RockMigrationHelper.UpdateWorkflowActionType( "D17F5F49-3781-484D-AAF4-ED585B6F4ECA", "Form", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "B9F8FE83-6567-47D1-B261-9B396CEEE767", "", 1, "", "AFB59989-D8CB-4DF1-A3C0-B52FA2F973DF" ); // PTO Allocation:Add Allocation:Form
            RockMigrationHelper.UpdateWorkflowActionType( "D17F5F49-3781-484D-AAF4-ED585B6F4ECA", "Add Allocation", 1, "76AFA08A-214A-40A8-BFB6-E60D62110286", true, false, "", "", 1, "", "9A7FA381-D810-4A28-82F4-8A4953C24721" ); // PTO Allocation:Add Allocation:Add Allocation
            RockMigrationHelper.UpdateWorkflowActionType( "D17F5F49-3781-484D-AAF4-ED585B6F4ECA", "Complete Workflow", 2, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "790A4D3A-793E-43D6-B02F-75538CA659C8" ); // PTO Allocation:Add Allocation:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "D120BA7F-A53F-4D43-8839-D7A92A0CF239", "Form", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "3135E5A6-615E-4DFA-9C50-8FECE1D1F684", "", 1, "", "5A1B8943-8546-40BD-BE5A-EE3DD06631D4" ); // PTO Allocation:Modify Allocation:Form
            RockMigrationHelper.UpdateWorkflowActionType( "D120BA7F-A53F-4D43-8839-D7A92A0CF239", "Update Allocation", 1, "76AFA08A-214A-40A8-BFB6-E60D62110286", true, false, "", "", 1, "", "871829DD-3750-4E79-A3E3-9A51259A1DE4" ); // PTO Allocation:Modify Allocation:Update Allocation
            RockMigrationHelper.UpdateWorkflowActionType( "D120BA7F-A53F-4D43-8839-D7A92A0CF239", "Complete Workflow", 2, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "3111E5B3-4D60-469D-A2A3-3425E44AF1A3" ); // PTO Allocation:Modify Allocation:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "93C33DC9-183C-488B-916B-781960CE9EDB", "Delete Allocation", 0, "4149F221-C080-4042-A8D1-E7FE054A0646", true, false, "", "", 1, "", "48A04E10-7E28-424C-A3B7-5B9657285525" ); // PTO Allocation:Delete Allocation:Delete Allocation
            RockMigrationHelper.UpdateWorkflowActionType( "93C33DC9-183C-488B-916B-781960CE9EDB", "Complete Workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "A4851453-0F00-402D-8B0A-89153AA0EB7C" ); // PTO Allocation:Delete Allocation:Complete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "6F6F6B37-58F3-45F9-B587-C2CD95ED6012", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // PTO Allocation:Start:Activate Add Activity if no existing PTO Allocation:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6F6F6B37-58F3-45F9-B587-C2CD95ED6012", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"D17F5F49-3781-484D-AAF4-ED585B6F4ECA" ); // PTO Allocation:Start:Activate Add Activity if no existing PTO Allocation:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "C34CCCEE-CC83-44BA-AA48-37E90DF0F01F", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""'%}
    {% for ptoAllocation in ptoallocationItems %}
        {{ptoAllocation.PersonAlias.Guid}}
    {% endfor %}
{% endptoallocation %}" ); // PTO Allocation:Start:Set Person:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "C34CCCEE-CC83-44BA-AA48-37E90DF0F01F", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Allocation:Start:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C34CCCEE-CC83-44BA-AA48-37E90DF0F01F", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"1b86d4e1-a085-485a-b4c0-3f17ee69f806" ); // PTO Allocation:Start:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C34CCCEE-CC83-44BA-AA48-37E90DF0F01F", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Allocation:Start:Set Person:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "0C54332D-AD9B-46FA-A8A0-90AFCF958E96", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""'%}
    {% for ptoAllocation in ptoallocationItems %}
        {{ptoAllocation.PtoType.Guid}}
    {% endfor %}
{% endptoallocation %}" ); // PTO Allocation:Start:Set Pto Type:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "0C54332D-AD9B-46FA-A8A0-90AFCF958E96", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Allocation:Start:Set Pto Type:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0C54332D-AD9B-46FA-A8A0-90AFCF958E96", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"b63296e7-83c0-41ad-aa98-b07fbb7fd25a" ); // PTO Allocation:Start:Set Pto Type:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0C54332D-AD9B-46FA-A8A0-90AFCF958E96", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Allocation:Start:Set Pto Type:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "1CB81D02-0B42-4F08-B67A-054863616ACB", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""'%}
    {% for ptoAllocation in ptoallocationItems %}
        {{ptoAllocation.StartDate}}
    {% endfor %}
{% endptoallocation %}" ); // PTO Allocation:Start:Set Start Date:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "1CB81D02-0B42-4F08-B67A-054863616ACB", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Allocation:Start:Set Start Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1CB81D02-0B42-4F08-B67A-054863616ACB", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"f827ff84-4229-4a03-b8d0-661fd835fc80" ); // PTO Allocation:Start:Set Start Date:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "1CB81D02-0B42-4F08-B67A-054863616ACB", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Allocation:Start:Set Start Date:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "9C6590AE-B995-4B11-991B-6E649C4E47D9", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""'%}
    {% for ptoAllocation in ptoallocationItems %}
        {{ptoAllocation.EndDate}}
    {% endfor %}
{% endptoallocation %}" ); // PTO Allocation:Start:Set End Date:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "9C6590AE-B995-4B11-991B-6E649C4E47D9", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Allocation:Start:Set End Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9C6590AE-B995-4B11-991B-6E649C4E47D9", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"bb8cc540-61a8-43e5-832c-d4880f13f053" ); // PTO Allocation:Start:Set End Date:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9C6590AE-B995-4B11-991B-6E649C4E47D9", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Allocation:Start:Set End Date:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "A61AD601-1C23-4E21-8F29-334337262AF2", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""'%}
    {% for ptoAllocation in ptoallocationItems %}
        {{ptoAllocation.Hours}}
    {% endfor %}
{% endptoallocation %}" ); // PTO Allocation:Start:Set Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "A61AD601-1C23-4E21-8F29-334337262AF2", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Allocation:Start:Set Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A61AD601-1C23-4E21-8F29-334337262AF2", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"ffc7e416-be23-4d5a-9801-4c4a02f47e87" ); // PTO Allocation:Start:Set Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A61AD601-1C23-4E21-8F29-334337262AF2", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Allocation:Start:Set Hours:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "CC570F29-3639-4E73-AB0C-E0E44B52F00E", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""'%}
    {% for ptoAllocation in ptoallocationItems %}
        {% case ptoAllocation.PtoAllocationStatus %}
        {% when 'Inactive' %}
            0
        {% when 'Active' %}
            1
        {% else %}
            2
        {% endcase %}
    {% endfor %}
{% endptoallocation %}" ); // PTO Allocation:Start:Set Status:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "CC570F29-3639-4E73-AB0C-E0E44B52F00E", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Allocation:Start:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CC570F29-3639-4E73-AB0C-E0E44B52F00E", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"cb3bd726-e327-43c8-8c60-02d290337a88" ); // PTO Allocation:Start:Set Status:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "CC570F29-3639-4E73-AB0C-E0E44B52F00E", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Allocation:Start:Set Status:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "DFAA70BC-0F5B-4177-8779-44D92022A42E", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Allocation:Start:Initial Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AFB59989-D8CB-4DF1-A3C0-B52FA2F973DF", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Allocation:Add Allocation:Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9A7FA381-D810-4A28-82F4-8A4953C24721", "1FEC991C-1ADE-468F-9AF0-0AF72F0D1728", @"False" ); // PTO Allocation:Add Allocation:Add Allocation:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9A7FA381-D810-4A28-82F4-8A4953C24721", "8E98C7E4-E05D-4A1F-B835-F3F553D31D21", @"1b86d4e1-a085-485a-b4c0-3f17ee69f806" ); // PTO Allocation:Add Allocation:Add Allocation:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "9A7FA381-D810-4A28-82F4-8A4953C24721", "F8E53739-1380-46BA-8E7A-B826880ADD8C", @"b63296e7-83c0-41ad-aa98-b07fbb7fd25a" ); // PTO Allocation:Add Allocation:Add Allocation:Pto Type
            RockMigrationHelper.AddActionTypeAttributeValue( "9A7FA381-D810-4A28-82F4-8A4953C24721", "7B45568D-3074-47F9-89ED-F37A602F0997", @"f827ff84-4229-4a03-b8d0-661fd835fc80" ); // PTO Allocation:Add Allocation:Add Allocation:Start Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9A7FA381-D810-4A28-82F4-8A4953C24721", "4D254EB2-C56C-4FC3-AE6C-290F4EA996F7", @"bb8cc540-61a8-43e5-832c-d4880f13f053" ); // PTO Allocation:Add Allocation:Add Allocation:End Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9A7FA381-D810-4A28-82F4-8A4953C24721", "8BC8874D-FC40-4B79-A881-950A50ED6507", @"ffc7e416-be23-4d5a-9801-4c4a02f47e87" ); // PTO Allocation:Add Allocation:Add Allocation:Hours|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9A7FA381-D810-4A28-82F4-8A4953C24721", "633E7884-6376-45C1-91BC-FC21A10F64EC", @"cb3bd726-e327-43c8-8c60-02d290337a88" ); // PTO Allocation:Add Allocation:Add Allocation:Status|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9A7FA381-D810-4A28-82F4-8A4953C24721", "FCFF0CA6-7BBD-4107-8341-72BB35DCEDD1", @"2" ); // PTO Allocation:Add Allocation:Add Allocation:Source
            RockMigrationHelper.AddActionTypeAttributeValue( "790A4D3A-793E-43D6-B02F-75538CA659C8", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // PTO Allocation:Add Allocation:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "790A4D3A-793E-43D6-B02F-75538CA659C8", "3327286F-C1A9-4624-949D-33E9F9049356", @"Completed" ); // PTO Allocation:Add Allocation:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5A1B8943-8546-40BD-BE5A-EE3DD06631D4", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Allocation:Modify Allocation:Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "1FEC991C-1ADE-468F-9AF0-0AF72F0D1728", @"False" ); // PTO Allocation:Modify Allocation:Update Allocation:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "1B16E29B-81C4-4CCC-999F-A4AAF356094F", @"a2eed015-29f5-4974-ad60-1539c100f6fb" ); // PTO Allocation:Modify Allocation:Update Allocation:Existing Pto Allocation
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "8E98C7E4-E05D-4A1F-B835-F3F553D31D21", @"1b86d4e1-a085-485a-b4c0-3f17ee69f806" ); // PTO Allocation:Modify Allocation:Update Allocation:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "F8E53739-1380-46BA-8E7A-B826880ADD8C", @"b63296e7-83c0-41ad-aa98-b07fbb7fd25a" ); // PTO Allocation:Modify Allocation:Update Allocation:Pto Type
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "7B45568D-3074-47F9-89ED-F37A602F0997", @"f827ff84-4229-4a03-b8d0-661fd835fc80" ); // PTO Allocation:Modify Allocation:Update Allocation:Start Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "4D254EB2-C56C-4FC3-AE6C-290F4EA996F7", @"bb8cc540-61a8-43e5-832c-d4880f13f053" ); // PTO Allocation:Modify Allocation:Update Allocation:End Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "8BC8874D-FC40-4B79-A881-950A50ED6507", @"ffc7e416-be23-4d5a-9801-4c4a02f47e87" ); // PTO Allocation:Modify Allocation:Update Allocation:Hours|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "633E7884-6376-45C1-91BC-FC21A10F64EC", @"cb3bd726-e327-43c8-8c60-02d290337a88" ); // PTO Allocation:Modify Allocation:Update Allocation:Status|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "871829DD-3750-4E79-A3E3-9A51259A1DE4", "FCFF0CA6-7BBD-4107-8341-72BB35DCEDD1", @"2" ); // PTO Allocation:Modify Allocation:Update Allocation:Source
            RockMigrationHelper.AddActionTypeAttributeValue( "3111E5B3-4D60-469D-A2A3-3425E44AF1A3", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // PTO Allocation:Modify Allocation:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3111E5B3-4D60-469D-A2A3-3425E44AF1A3", "3327286F-C1A9-4624-949D-33E9F9049356", @"Completed" ); // PTO Allocation:Modify Allocation:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "48A04E10-7E28-424C-A3B7-5B9657285525", "FD7D231F-1B61-4EDD-A3D7-CB9C315D62A0", @"False" ); // PTO Allocation:Delete Allocation:Delete Allocation:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "48A04E10-7E28-424C-A3B7-5B9657285525", "FA37CBA4-938E-4465-993C-466563D88EF7", @"a2eed015-29f5-4974-ad60-1539c100f6fb" ); // PTO Allocation:Delete Allocation:Delete Allocation:Existing Pto Allocation
            RockMigrationHelper.AddActionTypeAttributeValue( "A4851453-0F00-402D-8B0A-89153AA0EB7C", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // PTO Allocation:Delete Allocation:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A4851453-0F00-402D-8B0A-89153AA0EB7C", "3327286F-C1A9-4624-949D-33E9F9049356", @"Completed" ); // PTO Allocation:Delete Allocation:Complete Workflow:Status|Status Attribute

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
