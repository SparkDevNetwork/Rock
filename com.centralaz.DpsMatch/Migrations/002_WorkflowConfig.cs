using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.DpsMatch.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class WorkflowConfig : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType("Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true);

            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true);

            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true);

            RockMigrationHelper.UpdateEntityType("com.centralaz.DpsMatch.Workflow.Action.ImportOffenders","3B7A0BFC-2976-4A51-AB2B-F4349DF41AD4",false,true);

            RockMigrationHelper.UpdateEntityType("com.centralaz.DpsMatch.Workflow.Action.PopulateMatchesTable","5EB31D9E-DCEC-48ED-9029-522E59629D14",false,true);

            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CompleteWorkflow","EEDA4318-F014-4A46-9C76-4C052EF81AA1",false,true);

            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.PersistWorkflow","F1A39347-6FE0-43D4-89FB-544195088ECF",false,true);

            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SendEmail","66197B01-D1F0-4924-A315-47AD54E030DE",false,true);

            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeFromPerson","17962C23-2E94-4E06-8461-0FB8B94E2FEA",false,true);

            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeToCurrentPerson","24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",false,true);

            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.UserEntryForm","486DC4FA-FCBC-425F-90B0-E606DA8A9F68",false,true);

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("17962C23-2E94-4E06-8461-0FB8B94E2FEA","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","CE28B79D-FBC2-4894-9198-D923D0217549"); // Rock.Workflow.Action.SetAttributeFromPerson:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("17962C23-2E94-4E06-8461-0FB8B94E2FEA","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The person attribute to set the value of.",0,@"","7AC47975-71AC-4A2F-BF1F-115CF5578D6F"); // Rock.Workflow.Action.SetAttributeFromPerson:Attribute

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("17962C23-2E94-4E06-8461-0FB8B94E2FEA","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","18EF907D-607E-4891-B034-7AA379D77854"); // Rock.Workflow.Action.SetAttributeFromPerson:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("17962C23-2E94-4E06-8461-0FB8B94E2FEA","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Person","Person","The person to set attribute value to. Leave blank to set person to nobody.",1,@"","5C803BD1-40FA-49B1-AE7E-68F43D3687BB"); // Rock.Workflow.Action.SetAttributeFromPerson:Person

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","DE9CB292-4785-4EA3-976D-3826F91E9E98"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","33E6DF69-BDFA-407A-9744-C175B60643AE","Person Attribute","PersonAttribute","The attribute to set to the currently logged in person.",0,@"","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("3B7A0BFC-2976-4A51-AB2B-F4349DF41AD4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","3430AA47-09AA-4D58-8333-9687D4B5FBE3"); // com.centralaz.DpsMatch.Workflow.Action.ImportOffenders:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("3B7A0BFC-2976-4A51-AB2B-F4349DF41AD4","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","3673E369-3FEE-4BB2-A8EC-E97FEEEDE2D2"); // com.centralaz.DpsMatch.Workflow.Action.ImportOffenders:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","234910F2-A0DB-4D7D-BAF7-83C880EF30AE"); // Rock.Workflow.Action.UserEntryForm:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","C178113D-7C86-4229-8424-C6D0CF4A7E23"); // Rock.Workflow.Action.UserEntryForm:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("5EB31D9E-DCEC-48ED-9029-522E59629D14","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","8832C0A6-F423-4267-A23C-618A228EC3FD"); // com.centralaz.DpsMatch.Workflow.Action.PopulateMatchesTable:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("5EB31D9E-DCEC-48ED-9029-522E59629D14","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","00751452-897D-44E6-A23D-4245EF0F700E"); // com.centralaz.DpsMatch.Workflow.Action.PopulateMatchesTable:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Body","Body","The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",3,@"","4D245B9E-6B03-46E7-8482-A51FBA190E4D"); // Rock.Workflow.Action.SendEmail:Body

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","36197160-7D3D-490D-AB42-7E29105AFE91"); // Rock.Workflow.Action.SendEmail:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","From Email Address|Attribute Value","From","The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>",0,@"","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC"); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Send To Email Address|Attribute Value","To","The email address or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>",1,@"","0C4C13B8-7076-4872-925A-F950886B5E16"); // Rock.Workflow.Action.SendEmail:Send To Email Address|Attribute Value

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","9C204CD0-1233-41C5-818A-C5DA439445AA","Subject","Subject","The subject that should be used when sending email. <span class='tip tip-lava'></span>",2,@"","5D9B13B6-CD96-4C7C-86FA-4512B9D28386"); // Rock.Workflow.Action.SendEmail:Subject

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","D1269254-C15A-40BD-B784-ADCC231D3950"); // Rock.Workflow.Action.SendEmail:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C"); // Rock.Workflow.Action.CompleteWorkflow:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","25CAD4BE-5A00-409D-9BAB-E32518D89956"); // Rock.Workflow.Action.CompleteWorkflow:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","50B01639-4938-40D2-A791-AA0EB4F86847"); // Rock.Workflow.Action.PersistWorkflow:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F1A39347-6FE0-43D4-89FB-544195088ECF","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","86F795B0-0CB6-4DA4-9CE4-B11D0922F361"); // Rock.Workflow.Action.PersistWorkflow:Order

            RockMigrationHelper.UpdateWorkflowType(false,true,"Offender Matching Request","A request to run a comparison on the church's person database from the state's list of sexual predators.","6F8A431C-BEBD-4D33-AAD6-1D70870329C2","Work","fa fa-shield",0,false,0,"EDB241A0-FD88-4D75-966E-CF590C1D24AA"); // Offender Matching Request

            RockMigrationHelper.UpdateWorkflowTypeAttribute("EDB241A0-FD88-4D75-966E-CF590C1D24AA","6F9E2DD0-E39E-4602-ADF9-EB710A75304A","Offender CSV File","DPSFile","The CSV file that lists the sexual predators and their information.",0,@"","E20E064A-80DF-4460-A7EE-476EC9F3CBDD"); // Offender Matching Request:Offender CSV File

            RockMigrationHelper.UpdateWorkflowTypeAttribute("EDB241A0-FD88-4D75-966E-CF590C1D24AA","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Requester","Requester","The person who requested the DPS Match",2,@"","4DDE07B8-A0B9-4B15-B8B8-ABB38AF3F668"); // Offender Matching Request:Requester

            RockMigrationHelper.UpdateWorkflowTypeAttribute("EDB241A0-FD88-4D75-966E-CF590C1D24AA","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Worker","Worker","The person assigned to the request.",1,@"","E357DDDC-F25D-4E0C-85D5-CF72E0761749"); // Offender Matching Request:Worker

            RockMigrationHelper.AddAttributeQualifier("E20E064A-80DF-4460-A7EE-476EC9F3CBDD","binaryFileType",@"c1142570-8cd6-4a20-83b1-acb47c1cd377","BD110DBC-5641-45BB-A032-D8F553CEDDB8"); // Offender Matching Request:Offender CSV File:binaryFileType

            RockMigrationHelper.UpdateWorkflowActivityType("EDB241A0-FD88-4D75-966E-CF590C1D24AA",true,"Request","Prompts the user for the DPS CSV file.",true,0,"679C48B6-1962-4635-B21B-1EDB3FD61923"); // Offender Matching Request:Request

            RockMigrationHelper.UpdateWorkflowActivityType("EDB241A0-FD88-4D75-966E-CF590C1D24AA",true,"Process CSV File","Processes the CSV file to find potential matches.",false,1,"C4ACDF66-A296-4060-823F-E83C766ABFE0"); // Offender Matching Request:Process CSV File

            RockMigrationHelper.UpdateWorkflowActionForm(@"<h2>Request</h2>
<p>
Attach the Sexual Offender .csv file below.
</p>
<div class='alert alert-info'>
    <small>
        Please ensure the following columns with these headings are in the CSV file:
        <div class='row'>
            <div class='col-md-6'>
                <ul>
                    <li>First Name</li>
                    <li>Last Name</li>
                    <li>MI</li>
                    <li>Age</li>
                    <li>HT</li>
                    <li>WT</li>
                    <li>Race</li>
                    <li>Sex</li>
                    <li>Hair</li>
                    <li>Eyes</li>
                </ul>
            </div>
            <div class='col-md-6'>
                <ul>
                    <li>Res_Add</li>
                    <li>Res_City</li>
                    <li>Res_State</li>
                    <li>Res_Zip</li>
                    <li>Verification Date</li>
                    <li>Offense</li>
                    <li>Level</li>
                    <li>Absconder</li>
                    <li>Convicting Jurisdiction</li>
                    <li>Unverified</li>
                </ul>
            </div>
        </div>
    </small>
</div>
<br/>",@"","Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^C4ACDF66-A296-4060-823F-E83C766ABFE0^Your information has been submitted successfully.|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"","FF883052-A75F-44AA-A37F-B3D3DDD30732"); // Offender Matching Request:Request:Prompt User

            RockMigrationHelper.UpdateWorkflowActionFormAttribute("FF883052-A75F-44AA-A37F-B3D3DDD30732","E20E064A-80DF-4460-A7EE-476EC9F3CBDD",0,true,false,true,"458CB4BF-0426-4881-8A0C-2B4BD2C94C39"); // Offender Matching Request:Request:Prompt User:Offender CSV File

            RockMigrationHelper.UpdateWorkflowActionFormAttribute("FF883052-A75F-44AA-A37F-B3D3DDD30732","E357DDDC-F25D-4E0C-85D5-CF72E0761749",1,false,true,false,"F945BBBC-FAE2-4CC8-98CD-54A1BBEBDFB4"); // Offender Matching Request:Request:Prompt User:Worker

            RockMigrationHelper.UpdateWorkflowActionFormAttribute("FF883052-A75F-44AA-A37F-B3D3DDD30732","4DDE07B8-A0B9-4B15-B8B8-ABB38AF3F668",2,false,true,false,"B73C080F-7567-46EC-812B-2A15B080B9F6"); // Offender Matching Request:Request:Prompt User:Requester

            RockMigrationHelper.UpdateWorkflowActionType("C4ACDF66-A296-4060-823F-E83C766ABFE0","Complete Workflow",3,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,false,"","",1,"","6720CB6A-1089-47D5-A996-E6854F66849A"); // Offender Matching Request:Process CSV File:Complete Workflow

            RockMigrationHelper.UpdateWorkflowActionType("C4ACDF66-A296-4060-823F-E83C766ABFE0","Notify Worker",2,"66197B01-D1F0-4924-A315-47AD54E030DE",true,false,"","",1,"","495BF896-2655-4EF2-9C3F-B23F4D038DE3"); // Offender Matching Request:Process CSV File:Notify Worker

            RockMigrationHelper.UpdateWorkflowActionType("679C48B6-1962-4635-B21B-1EDB3FD61923","Persist the Workflow",3,"F1A39347-6FE0-43D4-89FB-544195088ECF",true,false,"","",1,"","DD2517CE-C51B-472B-A7B4-EB3D25A0214B"); // Offender Matching Request:Request:Persist the Workflow

            RockMigrationHelper.UpdateWorkflowActionType("679C48B6-1962-4635-B21B-1EDB3FD61923","Set Requester",1,"24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",true,false,"","",1,"","585EC133-3949-4ED6-AF54-D39AA9745F64"); // Offender Matching Request:Request:Set Requester

            RockMigrationHelper.UpdateWorkflowActionType("679C48B6-1962-4635-B21B-1EDB3FD61923","Set Worker",2,"17962C23-2E94-4E06-8461-0FB8B94E2FEA",true,false,"","",1,"","3F06CF09-1D74-4AE1-BC20-33C56C482B80"); // Offender Matching Request:Request:Set Worker

            RockMigrationHelper.UpdateWorkflowActionType("679C48B6-1962-4635-B21B-1EDB3FD61923","Prompt User",0,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"FF883052-A75F-44AA-A37F-B3D3DDD30732","",1,"","F8B8BF84-B40B-4E17-BA74-A842CEC8E1BB"); // Offender Matching Request:Request:Prompt User

            RockMigrationHelper.UpdateWorkflowActionType("C4ACDF66-A296-4060-823F-E83C766ABFE0","Import Sexual Offenders",0,"3B7A0BFC-2976-4A51-AB2B-F4349DF41AD4",true,false,"","",1,"","01E028B2-0154-41E8-9BAD-99E88007945A"); // Offender Matching Request:Process CSV File:Import Sexual Offenders

            RockMigrationHelper.UpdateWorkflowActionType("C4ACDF66-A296-4060-823F-E83C766ABFE0","Populate Potential Matches Table",1,"5EB31D9E-DCEC-48ED-9029-522E59629D14",true,false,"","",1,"","2B57A747-912C-42C6-B1AB-433506C2E9DA"); // Offender Matching Request:Process CSV File:Populate Potential Matches Table

            RockMigrationHelper.AddActionTypeAttributeValue("F8B8BF84-B40B-4E17-BA74-A842CEC8E1BB","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Offender Matching Request:Request:Prompt User:Active

            RockMigrationHelper.AddActionTypeAttributeValue("F8B8BF84-B40B-4E17-BA74-A842CEC8E1BB","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Offender Matching Request:Request:Prompt User:Order

            RockMigrationHelper.AddActionTypeAttributeValue("585EC133-3949-4ED6-AF54-D39AA9745F64","DE9CB292-4785-4EA3-976D-3826F91E9E98",@"False"); // Offender Matching Request:Request:Set Requester:Active

            RockMigrationHelper.AddActionTypeAttributeValue("585EC133-3949-4ED6-AF54-D39AA9745F64","89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8",@""); // Offender Matching Request:Request:Set Requester:Order

            RockMigrationHelper.AddActionTypeAttributeValue("585EC133-3949-4ED6-AF54-D39AA9745F64","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112",@"4dde07b8-a0b9-4b15-b8b8-abb38af3f668"); // Offender Matching Request:Request:Set Requester:Person Attribute

            RockMigrationHelper.AddActionTypeAttributeValue("3F06CF09-1D74-4AE1-BC20-33C56C482B80","CE28B79D-FBC2-4894-9198-D923D0217549",@"False"); // Offender Matching Request:Request:Set Worker:Active

            RockMigrationHelper.AddActionTypeAttributeValue("3F06CF09-1D74-4AE1-BC20-33C56C482B80","7AC47975-71AC-4A2F-BF1F-115CF5578D6F",@"e357dddc-f25d-4e0c-85d5-cf72e0761749"); // Offender Matching Request:Request:Set Worker:Attribute

            RockMigrationHelper.AddActionTypeAttributeValue("3F06CF09-1D74-4AE1-BC20-33C56C482B80","18EF907D-607E-4891-B034-7AA379D77854",@""); // Offender Matching Request:Request:Set Worker:Order

            RockMigrationHelper.AddActionTypeAttributeValue("DD2517CE-C51B-472B-A7B4-EB3D25A0214B","50B01639-4938-40D2-A791-AA0EB4F86847",@"False"); // Offender Matching Request:Request:Persist the Workflow:Active

            RockMigrationHelper.AddActionTypeAttributeValue("DD2517CE-C51B-472B-A7B4-EB3D25A0214B","86F795B0-0CB6-4DA4-9CE4-B11D0922F361",@""); // Offender Matching Request:Request:Persist the Workflow:Order

            RockMigrationHelper.AddActionTypeAttributeValue("01E028B2-0154-41E8-9BAD-99E88007945A","3430AA47-09AA-4D58-8333-9687D4B5FBE3",@"False"); // Offender Matching Request:Process CSV File:Import Sexual Offenders:Active

            RockMigrationHelper.AddActionTypeAttributeValue("01E028B2-0154-41E8-9BAD-99E88007945A","3673E369-3FEE-4BB2-A8EC-E97FEEEDE2D2",@""); // Offender Matching Request:Process CSV File:Import Sexual Offenders:Order

            RockMigrationHelper.AddActionTypeAttributeValue("2B57A747-912C-42C6-B1AB-433506C2E9DA","8832C0A6-F423-4267-A23C-618A228EC3FD",@"False"); // Offender Matching Request:Process CSV File:Populate Potential Matches Table:Active

            RockMigrationHelper.AddActionTypeAttributeValue("2B57A747-912C-42C6-B1AB-433506C2E9DA","00751452-897D-44E6-A23D-4245EF0F700E",@""); // Offender Matching Request:Process CSV File:Populate Potential Matches Table:Order

            RockMigrationHelper.AddActionTypeAttributeValue("495BF896-2655-4EF2-9C3F-B23F4D038DE3","36197160-7D3D-490D-AB42-7E29105AFE91",@"False"); // Offender Matching Request:Process CSV File:Notify Worker:Active

            RockMigrationHelper.AddActionTypeAttributeValue("495BF896-2655-4EF2-9C3F-B23F4D038DE3","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC",@""); // Offender Matching Request:Process CSV File:Notify Worker:From Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue("495BF896-2655-4EF2-9C3F-B23F4D038DE3","D1269254-C15A-40BD-B784-ADCC231D3950",@""); // Offender Matching Request:Process CSV File:Notify Worker:Order

            RockMigrationHelper.AddActionTypeAttributeValue("495BF896-2655-4EF2-9C3F-B23F4D038DE3","0C4C13B8-7076-4872-925A-F950886B5E16",@"e357dddc-f25d-4e0c-85d5-cf72e0761749"); // Offender Matching Request:Process CSV File:Notify Worker:Send To Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue("495BF896-2655-4EF2-9C3F-B23F4D038DE3","5D9B13B6-CD96-4C7C-86FA-4512B9D28386",@"Offender Matching Request"); // Offender Matching Request:Process CSV File:Notify Worker:Subject

            RockMigrationHelper.AddActionTypeAttributeValue("495BF896-2655-4EF2-9C3F-B23F4D038DE3","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ GlobalAttribute.EmailHeader }}

<p>The following Offender Match Request has been submitted by {{ Workflow.Requester }}:</p>

<h4><a href='{{ GlobalAttribute.InternalApplicationRoot }}OffenderMatch'>Process Request</a></h4>

{{ GlobalAttribute.EmailFooter }}"); // Offender Matching Request:Process CSV File:Notify Worker:Body

            RockMigrationHelper.AddActionTypeAttributeValue("6720CB6A-1089-47D5-A996-E6854F66849A","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // Offender Matching Request:Process CSV File:Complete Workflow:Active

            RockMigrationHelper.AddActionTypeAttributeValue("6720CB6A-1089-47D5-A996-E6854F66849A","25CAD4BE-5A00-409D-9BAB-E32518D89956",@""); // Offender Matching Request:Process CSV File:Complete Workflow:Order

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
