using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 3, "1.0.14" )]
    public class LifeGroupWorkflow : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendSystemEmail", "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", false, true );

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "System Email", "SystemEmail", "A system email to send.", 0, @"", "00676307-F278-42ED-8C05-5B5DD43408B1" ); // Rock.Workflow.Action.SendSystemEmail:System Email

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0DF2AEAA-D6A8-45D8-9F27-663FFD151EA1" ); // Rock.Workflow.Action.SendSystemEmail:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Address|Attribute Value", "Recipient", "The email address or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "2D0E8665-8B1F-4632-88D8-9A9B6C4E9457" ); // Rock.Workflow.Action.SendSystemEmail:Send To Email Address|Attribute Value

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "55C27F0A-6397-4452-8A5A-279590A6F680" ); // Rock.Workflow.Action.SendSystemEmail:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order

            RockMigrationHelper.UpdateWorkflowType( false, true, "New Group Member Request", "Workflow for pending group members", "78E38655-D951-41DB-A0FF-D6474775CFA1", "Work", "fa fa-home", 0, true, 3, "5997D765-55C8-4A1F-BE59-EEC256180F3C" ); // New Group Member Request

            RockMigrationHelper.UpdateWorkflowType( false, true, "Life Group Information Request", "Workflow for a request for information about a life group", "78E38655-D951-41DB-A0FF-D6474775CFA1", "Work", "fa fa-home", 0, true, 3, "4CB877C4-5BD0-4D0E-B03F-99DAD20EDF27" ); // Life Group Information Request

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5997D765-55C8-4A1F-BE59-EEC256180F3C", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Group Leader", "GroupLeader", "", 0, @"", "B56A7BA6-3CD0-43B3-A571-C3ECF0BF5699" ); // New Group Member Request:Group Leader

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "5997D765-55C8-4A1F-BE59-EEC256180F3C", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Group Member", "GroupMember", "", 1, @"", "0D4DDA7A-D39A-4840-8D20-D07D562283F6" ); // New Group Member Request:Group Member

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "4CB877C4-5BD0-4D0E-B03F-99DAD20EDF27", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Group Leader", "GroupLeader", "", 0, @"", "2A0B9839-B9E7-4E78-A2C9-ECD193DDB774" ); // Life Group Information Request:Group Leader

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "4CB877C4-5BD0-4D0E-B03F-99DAD20EDF27", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Group Member", "GroupMember", "", 1, @"", "FF9C1074-6489-41C2-A328-D3E4F83BD191" ); // Life Group Information Request:Group Member

            RockMigrationHelper.UpdateWorkflowActivityType( "5997D765-55C8-4A1F-BE59-EEC256180F3C", true, "Notify Participants", "", true, 0, "B5A34857-A809-43C2-B8AD-EF2CEDADEC0A" ); // New Group Member Request:Notify Participants

            RockMigrationHelper.UpdateWorkflowActivityType( "4CB877C4-5BD0-4D0E-B03F-99DAD20EDF27", true, "Notify Participants", "", true, 0, "3506A3A0-5A8D-4D43-ADE0-9134129D27FF" ); // Life Group Information Request:Notify Participants

            RockMigrationHelper.UpdateWorkflowActionType( "B5A34857-A809-43C2-B8AD-EF2CEDADEC0A", "Complete the Workflow", 2, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "18E87064-EBC1-4EA3-951C-B519CDFFDD9F" ); // New Group Member Request:Notify Participants:Complete the Workflow

            RockMigrationHelper.UpdateWorkflowActionType( "3506A3A0-5A8D-4D43-ADE0-9134129D27FF", "Complete the Workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "E3CAAF62-23DF-4660-ABC3-ADE5B35CD284" ); // Life Group Information Request:Notify Participants:Complete the Workflow

            RockMigrationHelper.UpdateWorkflowActionType( "3506A3A0-5A8D-4D43-ADE0-9134129D27FF", "Send Email to Group Leader", 0, "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", true, false, "", "", 1, "", "D585D333-9217-41E3-8497-3CBE8A805AC2" ); // Life Group Information Request:Notify Participants:Send Email to Group Leader

            RockMigrationHelper.UpdateWorkflowActionType( "B5A34857-A809-43C2-B8AD-EF2CEDADEC0A", "Send Email to New Member", 1, "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", true, false, "", "", 1, "", "F7EC3D6D-4ECD-4C87-A4B4-0E7AF2B47EE2" ); // New Group Member Request:Notify Participants:Send Email to New Member

            RockMigrationHelper.UpdateWorkflowActionType( "B5A34857-A809-43C2-B8AD-EF2CEDADEC0A", "Send Email to Group Leader", 0, "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", true, false, "", "", 1, "", "CDEC0142-71D0-4813-8603-7F49DECBD62B" ); // New Group Member Request:Notify Participants:Send Email to Group Leader

            RockMigrationHelper.AddActionTypeAttributeValue( "CDEC0142-71D0-4813-8603-7F49DECBD62B", "00676307-F278-42ED-8C05-5B5DD43408B1", @"caafa7d3-b8f2-4fb5-9c57-ebcb91de5a2e" ); // New Group Member Request:Notify Participants:Send Email to Group Leader:System Email

            RockMigrationHelper.AddActionTypeAttributeValue( "CDEC0142-71D0-4813-8603-7F49DECBD62B", "0DF2AEAA-D6A8-45D8-9F27-663FFD151EA1", @"False" ); // New Group Member Request:Notify Participants:Send Email to Group Leader:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "CDEC0142-71D0-4813-8603-7F49DECBD62B", "55C27F0A-6397-4452-8A5A-279590A6F680", @"" ); // New Group Member Request:Notify Participants:Send Email to Group Leader:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "D585D333-9217-41E3-8497-3CBE8A805AC2", "00676307-F278-42ED-8C05-5B5DD43408B1", @"8091e0d6-1f54-4015-a298-f87d36982daf" ); // Life Group Information Request:Notify Participants:Send Email to Group Leader:System Email

            RockMigrationHelper.AddActionTypeAttributeValue( "D585D333-9217-41E3-8497-3CBE8A805AC2", "0DF2AEAA-D6A8-45D8-9F27-663FFD151EA1", @"False" ); // Life Group Information Request:Notify Participants:Send Email to Group Leader:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "D585D333-9217-41E3-8497-3CBE8A805AC2", "55C27F0A-6397-4452-8A5A-279590A6F680", @"" ); // Life Group Information Request:Notify Participants:Send Email to Group Leader:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "D585D333-9217-41E3-8497-3CBE8A805AC2", "2D0E8665-8B1F-4632-88D8-9A9B6C4E9457", @"2a0b9839-b9e7-4e78-a2c9-ecd193ddb774" ); // Life Group Information Request:Notify Participants:Send Email to Group Leader:Send To Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "CDEC0142-71D0-4813-8603-7F49DECBD62B", "2D0E8665-8B1F-4632-88D8-9A9B6C4E9457", @"b56a7ba6-3cd0-43b3-a571-c3ecf0bf5699" ); // New Group Member Request:Notify Participants:Send Email to Group Leader:Send To Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "F7EC3D6D-4ECD-4C87-A4B4-0E7AF2B47EE2", "0DF2AEAA-D6A8-45D8-9F27-663FFD151EA1", @"False" ); // New Group Member Request:Notify Participants:Send Email to New Member:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "F7EC3D6D-4ECD-4C87-A4B4-0E7AF2B47EE2", "55C27F0A-6397-4452-8A5A-279590A6F680", @"" ); // New Group Member Request:Notify Participants:Send Email to New Member:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "F7EC3D6D-4ECD-4C87-A4B4-0E7AF2B47EE2", "00676307-F278-42ED-8C05-5B5DD43408B1", @"b83ef8b4-67df-4824-b38f-b7cf5527a381" ); // New Group Member Request:Notify Participants:Send Email to New Member:System Email

            RockMigrationHelper.AddActionTypeAttributeValue( "E3CAAF62-23DF-4660-ABC3-ADE5B35CD284", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Life Group Information Request:Notify Participants:Complete the Workflow:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "E3CAAF62-23DF-4660-ABC3-ADE5B35CD284", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Life Group Information Request:Notify Participants:Complete the Workflow:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "F7EC3D6D-4ECD-4C87-A4B4-0E7AF2B47EE2", "2D0E8665-8B1F-4632-88D8-9A9B6C4E9457", @"0d4dda7a-d39a-4840-8d20-d07d562283f6" ); // New Group Member Request:Notify Participants:Send Email to New Member:Send To Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "18E87064-EBC1-4EA3-951C-B519CDFFDD9F", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // New Group Member Request:Notify Participants:Complete the Workflow:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "18E87064-EBC1-4EA3-951C-B519CDFFDD9F", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // New Group Member Request:Notify Participants:Complete the Workflow:Order


            //--------------------------------------------------------------

            RockMigrationHelper.AddBlockAttributeValue( "18DC3025-3791-43C8-9805-113AF17D5942", "42D95717-2494-4E76-B837-C78AC6E8B139", "5997d765-55c8-4a1f-be59-eec256180f3c" );
            RockMigrationHelper.AddBlockAttributeValue( "18DC3025-3791-43C8-9805-113AF17D5942", "C3537C7E-7105-4E3C-8FF2-6FD956D5EC40", "4cb877c4-5bd0-4d0e-b03f-99dad20edf27" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlockAttributeValue( "18DC3025-3791-43C8-9805-113AF17D5942", "C3537C7E-7105-4E3C-8FF2-6FD956D5EC40" );
            RockMigrationHelper.DeleteBlockAttributeValue( "18DC3025-3791-43C8-9805-113AF17D5942", "42D95717-2494-4E76-B837-C78AC6E8B139" );
        }
    }
}
