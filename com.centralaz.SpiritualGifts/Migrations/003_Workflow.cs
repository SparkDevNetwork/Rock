using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.SpiritualGifts.Migrations
{
    [MigrationNumber( 3, "1.0.14" )]
    public class Workflow : Migration
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

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.PersistWorkflow", "F1A39347-6FE0-43D4-89FB-544195088ECF", false, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunSQL", "A41216D6-6FB0-4019-B222-2C29B4519CF4", false, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeFromEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToCurrentPerson", "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", false, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 3, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Address|Attribute Value", "To", "The email address or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Address|Attribute Value

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 2, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "EFC517E9-8A53-4681-B16B-9D4DE89244BE" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeFromEntity:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "83E27C1E-1491-4AE2-93F1-909791D4B70A" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Id instead of Guid", "UseId", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", 3, @"False", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 1, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeFromEntity:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQLQuery", "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", 0, @"", "F3B9908B-096F-460B-8320-122CF046D1F9" ); // Rock.Workflow.Action.RunSQL:SQLQuery

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A18C3143-0586-4565-9F36-E603BC674B4E" ); // Rock.Workflow.Action.RunSQL:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Result Attribute", "ResultAttribute", "An optional attribute to set to the scaler result of SQL query.", 1, @"", "56997192-2545-4EA1-B5B2-313B04588984" ); // Rock.Workflow.Action.RunSQL:Result Attribute

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "FA7C685D-8636-41EF-9998-90FFF3998F76" ); // Rock.Workflow.Action.RunSQL:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "50B01639-4938-40D2-A791-AA0EB4F86847" ); // Rock.Workflow.Action.PersistWorkflow:Active

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Persist Immediately", "PersistImmediately", "This action will normally cause the workflow to be persisted (saved) once all the current activites/actions have completed processing. Set this flag to true, if the workflow should be persisted immediately. This is only required if a subsequent action needs a persisted workflow with a valid id.", 0, @"False", "82744A46-0110-4728-BD3D-66C85C5FCB2F" ); // Rock.Workflow.Action.PersistWorkflow:Persist Immediately

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361" ); // Rock.Workflow.Action.PersistWorkflow:Order

            RockMigrationHelper.UpdateWorkflowType( false, true, "Spiritual Gifts Request", "Used to request a person take the spiritual gifts analysis test.", "BBAE05FD-8192-4616-A71E-903A927E0D90", "Request", "fa fa-gift", 0, false, 0, "6EDE99E4-3D82-4637-8A30-748F52BCF2B9" ); // Spiritual Gifts Request

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "6EDE99E4-3D82-4637-8A30-748F52BCF2B9", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Custom Message", "CustomMessage", "A custom message you would like to include in the request.  Otherwise the default will be used.", 1, @"We're each a unique creation. We'd love to learn more about you through a simple and quick online spiritual gifting profile. The results of the assessment will help us tailor our ministry to you and can also be used for building healthier teams and groups.

The assessment takes less than ten minutes and will go a long way toward helping us get to know you better. Thanks in advance!", "E84068F2-0BE2-4AB5-A7A6-0090C7B9CC41" ); // Spiritual Gifts Request:Custom Message

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "6EDE99E4-3D82-4637-8A30-748F52BCF2B9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "OptOut", "OptOut", "Holds a 1 if the person has opted out of bulk or email requests.", 3, @"False", "64FEB7C4-CFD6-4840-8D2E-D5024F54A269" ); // Spiritual Gifts Request:OptOut

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "6EDE99E4-3D82-4637-8A30-748F52BCF2B9", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person that you are requesting the assessment for.  If initiated directly from the person profile record (using 'Actions' option), this value will automatically be populated.", 0, @"", "D08AADCD-E668-4D5D-B319-2866DCA361E4" ); // Spiritual Gifts Request:Person

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "6EDE99E4-3D82-4637-8A30-748F52BCF2B9", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Sender", "Sender", "The person sending the request.", 2, @"", "4D72728C-9A21-44FF-B3E0-578F3403CA16" ); // Spiritual Gifts Request:Sender

            RockMigrationHelper.UpdateWorkflowTypeAttribute( "6EDE99E4-3D82-4637-8A30-748F52BCF2B9", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Warning Message", "WarningMessage", "A warning message that may be displayed if the person has opted out from bulk and email requests.", 4, @"", "35752D52-5FB9-4A88-9049-B4446223D791" ); // Spiritual Gifts Request:Warning Message

            RockMigrationHelper.AddAttributeQualifier( "E84068F2-0BE2-4AB5-A7A6-0090C7B9CC41", "numberofrows", @"6", "4BE6591F-739B-4CFB-B38A-5177DA4A7BD2" ); // Spiritual Gifts Request:Custom Message:numberofrows

            RockMigrationHelper.AddAttributeQualifier( "64FEB7C4-CFD6-4840-8D2E-D5024F54A269", "falsetext", @"No", "65AD6E9F-F866-4F47-BCC3-D94F00A208B5" ); // Spiritual Gifts Request:OptOut:falsetext

            RockMigrationHelper.AddAttributeQualifier( "64FEB7C4-CFD6-4840-8D2E-D5024F54A269", "truetext", @"Yes", "590E64A9-FE72-495B-BBE0-B975C6F175F8" ); // Spiritual Gifts Request:OptOut:truetext

            RockMigrationHelper.UpdateWorkflowActivityType( "6EDE99E4-3D82-4637-8A30-748F52BCF2B9", true, "Launch From Person Profile", "When this workflow is initiated from the Person Profile page, the \"Entity\" will have a value so the first action will run successfully, and the workflow will then be persisted.", true, 0, "B81D9DB6-F967-4D95-8B89-583304CF9347" ); // Spiritual Gifts Request:Launch From Person Profile

            RockMigrationHelper.UpdateWorkflowActionForm( @"Hi &#123;&#123; Person.NickName &#125;&#125;!

{{ Workflow.WarningMessage }}", @"", "Send^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your information has been submitted successfully.", "", true, "", "CA73DD50-B5A2-4022-9C32-5B65C1EE7A7A" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CA73DD50-B5A2-4022-9C32-5B65C1EE7A7A", "D08AADCD-E668-4D5D-B319-2866DCA361E4", 0, false, true, false, "94A89E77-E3F6-44B4-99EB-ACC4A18335C4" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message:Person

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CA73DD50-B5A2-4022-9C32-5B65C1EE7A7A", "E84068F2-0BE2-4AB5-A7A6-0090C7B9CC41", 1, true, false, false, "52DC10A7-C163-4C51-A400-195BF783CD17" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message:Custom Message

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CA73DD50-B5A2-4022-9C32-5B65C1EE7A7A", "4D72728C-9A21-44FF-B3E0-578F3403CA16", 2, false, true, false, "A240190D-D47E-49AF-8D5A-AEA10A96DB64" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message:Sender

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CA73DD50-B5A2-4022-9C32-5B65C1EE7A7A", "64FEB7C4-CFD6-4840-8D2E-D5024F54A269", 3, false, true, false, "5BBE715C-AA72-4442-AD1B-0CA535C8399B" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message:OptOut

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "CA73DD50-B5A2-4022-9C32-5B65C1EE7A7A", "35752D52-5FB9-4A88-9049-B4446223D791", 4, false, true, false, "AB142809-BC0E-4AF5-A861-08D301D82D53" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message:Warning Message

            RockMigrationHelper.UpdateWorkflowActionType( "B81D9DB6-F967-4D95-8B89-583304CF9347", "Complete the Workflow", 7, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "7993BCAE-76EE-4248-8241-8F5C6C206EA8" ); // Spiritual Gifts Request:Launch From Person Profile:Complete the Workflow

            RockMigrationHelper.UpdateWorkflowActionType( "B81D9DB6-F967-4D95-8B89-583304CF9347", "Send Email Action", 6, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "6DF6157D-6E8D-4B8B-ADFC-28A6586B46DC" ); // Spiritual Gifts Request:Launch From Person Profile:Send Email Action

            RockMigrationHelper.UpdateWorkflowActionType( "B81D9DB6-F967-4D95-8B89-583304CF9347", "Check for Opt Out", 2, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "F7AE55F5-B8AC-46A3-BD2F-E256B5AA4FBE" ); // Spiritual Gifts Request:Launch From Person Profile:Check for Opt Out

            RockMigrationHelper.UpdateWorkflowActionType( "B81D9DB6-F967-4D95-8B89-583304CF9347", "Persist Workflow", 3, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "17F53BBF-5D70-4E4B-9DF5-73B14A1FAF4F" ); // Spiritual Gifts Request:Launch From Person Profile:Persist Workflow

            RockMigrationHelper.UpdateWorkflowActionType( "B81D9DB6-F967-4D95-8B89-583304CF9347", "Set Sender", 1, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "CDF170DD-0BFC-465D-99D4-59EB028571DC" ); // Spiritual Gifts Request:Launch From Person Profile:Set Sender

            RockMigrationHelper.UpdateWorkflowActionType( "B81D9DB6-F967-4D95-8B89-583304CF9347", "Set Person", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "47903583-161C-48A1-AF24-5CE1AC613F59" ); // Spiritual Gifts Request:Launch From Person Profile:Set Person

            RockMigrationHelper.UpdateWorkflowActionType( "B81D9DB6-F967-4D95-8B89-583304CF9347", "Set Warning", 4, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "64FEB7C4-CFD6-4840-8D2E-D5024F54A269", 1, "True", "73FF9418-F780-4779-AB8A-9F022730CD49" ); // Spiritual Gifts Request:Launch From Person Profile:Set Warning

            RockMigrationHelper.UpdateWorkflowActionType( "B81D9DB6-F967-4D95-8B89-583304CF9347", "Custom Message", 5, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "CA73DD50-B5A2-4022-9C32-5B65C1EE7A7A", "", 1, "", "962BEBEB-18C9-4414-86B8-7662EF6A03F4" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message

            RockMigrationHelper.AddActionTypeAttributeValue( "47903583-161C-48A1-AF24-5CE1AC613F59", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Set Person:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "47903583-161C-48A1-AF24-5CE1AC613F59", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Set Person:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "47903583-161C-48A1-AF24-5CE1AC613F59", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"d08aadcd-e668-4d5d-b319-2866dca361e4" ); // Spiritual Gifts Request:Launch From Person Profile:Set Person:Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "47903583-161C-48A1-AF24-5CE1AC613F59", "83E27C1E-1491-4AE2-93F1-909791D4B70A", @"True" ); // Spiritual Gifts Request:Launch From Person Profile:Set Person:Entity Is Required

            RockMigrationHelper.AddActionTypeAttributeValue( "47903583-161C-48A1-AF24-5CE1AC613F59", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Set Person:Use Id instead of Guid

            RockMigrationHelper.AddActionTypeAttributeValue( "47903583-161C-48A1-AF24-5CE1AC613F59", "EFC517E9-8A53-4681-B16B-9D4DE89244BE", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Set Person:Lava Template

            RockMigrationHelper.AddActionTypeAttributeValue( "CDF170DD-0BFC-465D-99D4-59EB028571DC", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Set Sender:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "CDF170DD-0BFC-465D-99D4-59EB028571DC", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Set Sender:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "CDF170DD-0BFC-465D-99D4-59EB028571DC", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"4d72728c-9a21-44ff-b3e0-578f3403ca16" ); // Spiritual Gifts Request:Launch From Person Profile:Set Sender:Person Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "F7AE55F5-B8AC-46A3-BD2F-E256B5AA4FBE", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Check for Opt Out:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "F7AE55F5-B8AC-46A3-BD2F-E256B5AA4FBE", "FA7C685D-8636-41EF-9998-90FFF3998F76", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Check for Opt Out:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "F7AE55F5-B8AC-46A3-BD2F-E256B5AA4FBE", "F3B9908B-096F-460B-8320-122CF046D1F9", @"DECLARE @PersonAliasGuid uniqueidentifier = '{{ Workflow.Person_unformatted }}'

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[EmailPreference] <> 0 )
    THEN 'True'
    ELSE 'False'
    END" ); // Spiritual Gifts Request:Launch From Person Profile:Check for Opt Out:SQLQuery

            RockMigrationHelper.AddActionTypeAttributeValue( "F7AE55F5-B8AC-46A3-BD2F-E256B5AA4FBE", "56997192-2545-4EA1-B5B2-313B04588984", @"64feb7c4-cfd6-4840-8d2e-d5024f54a269" ); // Spiritual Gifts Request:Launch From Person Profile:Check for Opt Out:Result Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "17F53BBF-5D70-4E4B-9DF5-73B14A1FAF4F", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Persist Workflow:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "17F53BBF-5D70-4E4B-9DF5-73B14A1FAF4F", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Persist Workflow:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "17F53BBF-5D70-4E4B-9DF5-73B14A1FAF4F", "82744A46-0110-4728-BD3D-66C85C5FCB2F", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Persist Workflow:Persist Immediately

            RockMigrationHelper.AddActionTypeAttributeValue( "73FF9418-F780-4779-AB8A-9F022730CD49", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Set Warning:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "73FF9418-F780-4779-AB8A-9F022730CD49", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"35752d52-5fb9-4a88-9049-b4446223d791" ); // Spiritual Gifts Request:Launch From Person Profile:Set Warning:Attribute

            RockMigrationHelper.AddActionTypeAttributeValue( "73FF9418-F780-4779-AB8A-9F022730CD49", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Set Warning:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "73FF9418-F780-4779-AB8A-9F022730CD49", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"<div class=""alert alert-warning"">{{ Workflow.Person }} has previously opted out from email and bulk requests.  Make sure you want to override this preference.</div>" ); // Spiritual Gifts Request:Launch From Person Profile:Set Warning:Text Value|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "962BEBEB-18C9-4414-86B8-7662EF6A03F4", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "962BEBEB-18C9-4414-86B8-7662EF6A03F4", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Custom Message:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "6DF6157D-6E8D-4B8B-ADFC-28A6586B46DC", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Send Email Action:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "6DF6157D-6E8D-4B8B-ADFC-28A6586B46DC", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"4d72728c-9a21-44ff-b3e0-578f3403ca16" ); // Spiritual Gifts Request:Launch From Person Profile:Send Email Action:From Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "6DF6157D-6E8D-4B8B-ADFC-28A6586B46DC", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Send Email Action:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "6DF6157D-6E8D-4B8B-ADFC-28A6586B46DC", "0C4C13B8-7076-4872-925A-F950886B5E16", @"d08aadcd-e668-4d5d-b319-2866dca361e4" ); // Spiritual Gifts Request:Launch From Person Profile:Send Email Action:Send To Email Address|Attribute Value

            RockMigrationHelper.AddActionTypeAttributeValue( "6DF6157D-6E8D-4B8B-ADFC-28A6586B46DC", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Spiritual Gift Assessment Request from {{ Workflow.Sender }}" ); // Spiritual Gifts Request:Launch From Person Profile:Send Email Action:Subject

            RockMigrationHelper.AddActionTypeAttributeValue( "6DF6157D-6E8D-4B8B-ADFC-28A6586B46DC", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailStyles }}
{{ GlobalAttribute.EmailHeader }}
<p>Hi {{ Person.NickName }}!</p>

<p>{{ Workflow.CustomMessage | NewlineToBr }}</p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}SpiritualGifts/{{ Person.UrlEncodedKey }}"">Take Spiritual Gift Assessment</a></p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.UrlEncodedKey }}"">I&#39;m no longer involved with {{ GlobalAttribute.OrganizationName }}. Please remove me from all future communications.</a></p>

<p>- {{ Workflow.Sender }}</p>

{{ GlobalAttribute.EmailFooter }}" ); // Spiritual Gifts Request:Launch From Person Profile:Send Email Action:Body

            RockMigrationHelper.AddActionTypeAttributeValue( "7993BCAE-76EE-4248-8241-8F5C6C206EA8", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Spiritual Gifts Request:Launch From Person Profile:Complete the Workflow:Active

            RockMigrationHelper.AddActionTypeAttributeValue( "7993BCAE-76EE-4248-8241-8F5C6C206EA8", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Spiritual Gifts Request:Launch From Person Profile:Complete the Workflow:Order

            // Update the Bio block's WorkflowAction attribute value to include this new SpiritualGift WF
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "7197A0FB-B330-43C4-8E62-F3C14F649813", "6EDE99E4-3D82-4637-8A30-748F52BCF2B9", appendToExisting: true );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}
