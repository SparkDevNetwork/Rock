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
    public partial class BulkUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType( "Attribute", "Sets an attribute.", "Rock", "Rock.Field.Types.AttributeFieldType", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B" );
            RockMigrationHelper.UpdateFieldType( "Schedules", "", "Rock", "Rock.Field.Types.SchedulesFieldType", "EC6A5CAF-F6A2-47A4-9CBA-6E1C53D7E59B" );

            #region Bulk Update

            Sql( @"
    UPDATE [PageRoute] SET [Route] = 'PersonMerge/{Set}' WHERE [Route] = 'PersonMerge/{People}'
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '936C90C4-29CF-4665-A489-7C687217F7B8'
" );
            RockMigrationHelper.AddPage( "936C90C4-29CF-4665-A489-7C687217F7B8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Bulk Update", "", "B6BFDE54-0EFA-4499-847D-BE1259F83535", "fa fa-reply-all" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "B6BFDE54-0EFA-4499-847D-BE1259F83535", "BulkUpdate/{Set}" );
            
            RockMigrationHelper.UpdateBlockType( "Bulk Update", "Used for updating information about several individuals at once.", "~/Blocks/Crm/BulkUpdate.ascx", "CRM", "A844886D-ED6F-4367-9C6F-667401201ED0" );

            // Add Block to Page: Bulk Update, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B6BFDE54-0EFA-4499-847D-BE1259F83535", "", "A844886D-ED6F-4367-9C6F-667401201ED0", "Bulk Update", "Main", "", "", 0, "A610AB9D-7397-4D27-8614-F6A282B78B2C" );
            
            // Attrib for BlockType: Bulk Update:Attribute Categories
            RockMigrationHelper.AddBlockTypeAttribute( "A844886D-ED6F-4367-9C6F-667401201ED0", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Attribute Categories", "AttributeCategories", "", "The person attribute categories to display and allow bulk updating", 0, @"", "B263B202-9DB3-4D16-B66A-D87D9E6EB427" );
            // Attrib for BlockType: Bulk Update:Note Type
            RockMigrationHelper.AddBlockTypeAttribute( "A844886D-ED6F-4367-9C6F-667401201ED0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Type", "NoteType", "", "The note type name (If it doesn't exist it will be created).", 2, @"Timeline", "F9547F97-BF1D-4F9A-AA56-04B2F6B5673C" );
            // Attrib for BlockType: Bulk Update:Display Count
            RockMigrationHelper.AddBlockTypeAttribute( "A844886D-ED6F-4367-9C6F-667401201ED0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Display Count", "DisplayCount", "", "The initial number of individuals to display prior to expanding list", 1, @"0", "8A3C95D6-37FD-464A-A9C8-6B6DFE946FDE" );
            // Attrib Value for Block:Bulk Update, Attribute:Attribute Categories Page: Bulk Update, Site: Rock RMS
            
            RockMigrationHelper.AddBlockAttributeValue( "A610AB9D-7397-4D27-8614-F6A282B78B2C", "B263B202-9DB3-4D16-B66A-D87D9E6EB427", @"e919e722-f895-44a4-b86d-38db8fba1844,752dc692-836e-4a3e-b670-4325cd7724bf,f6b98d0c-197d-433a-917b-0c39a80a79e8,9af28593-e631-41e4-b696-78015a4d6f7b,7b879922-5da6-41ee-ac0b-45ceffb99458" );
            // Attrib Value for Block:Bulk Update, Attribute:Note Type Page: Bulk Update, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A610AB9D-7397-4D27-8614-F6A282B78B2C", "F9547F97-BF1D-4F9A-AA56-04B2F6B5673C", @"Timeline" );
            // Attrib Value for Block:Bulk Update, Attribute:Display Count Page: Bulk Update, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A610AB9D-7397-4D27-8614-F6A282B78B2C", "8A3C95D6-37FD-464A-A9C8-6B6DFE946FDE", @"0" );

            #endregion

            #region Add Photo Request Workflow

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 3, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Address|Attribute Value", "To", "The email address or an attribute that contains the person or email address that email should be sent to", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 2, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeToEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeToEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeToEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQLQuery", "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", 0, @"", "F3B9908B-096F-460B-8320-122CF046D1F9" ); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A18C3143-0586-4565-9F36-E603BC674B4E" ); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Result Attribute", "ResultAttribute", "An optional attribute to set to the scaler result of SQL query.", 1, @"", "56997192-2545-4EA1-B5B2-313B04588984" ); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "FA7C685D-8636-41EF-9998-90FFF3998F76" ); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "50B01639-4938-40D2-A791-AA0EB4F86847" ); // Rock.Workflow.Action.PersistWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361" ); // Rock.Workflow.Action.PersistWorkflow:Order
            RockMigrationHelper.UpdateWorkflowType( false, true, "Photo Request", "Used to request a photo from a person.", "BBAE05FD-8192-4616-A71E-903A927E0D90", "Request", "fa fa-camera", 0, false, 0, "036F2F0B-C2DC-49D0-A17B-CCDAC7FC71E2" ); // Photo Request
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "036F2F0B-C2DC-49D0-A17B-CCDAC7FC71E2", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Custom Message", "CustomMessage", "A custom message you would like to include in the request.  Otherwise the default will be used.", 1, @"We're all about people and we'd like to personalize our relationship by having a recent photo of you in our membership system. Please take a minute to upload your photo using the button below - we'd really appreciate it.", "55A64AF1-E65C-4CE0-9542-9B79B256977E" ); // Photo Request:Custom Message
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "036F2F0B-C2DC-49D0-A17B-CCDAC7FC71E2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "OptOut", "OptOut", "Holds a 1 if the person has opted out of photo requests.", 3, @"False", "26C30D02-3B89-4EA3-AAA7-08DB2E8EE0C3" ); // Photo Request:OptOut
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "036F2F0B-C2DC-49D0-A17B-CCDAC7FC71E2", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person that you are requesting the photo for.  If initiated directly from the person profile record (using 'Actions' option), this value will automatically be populated.", 0, @"", "282D67EF-73FE-42A7-A248-F2901873E758" ); // Photo Request:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "036F2F0B-C2DC-49D0-A17B-CCDAC7FC71E2", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Sender", "Sender", "The person sending the request.", 2, @"", "FA5530D4-9580-4C1F-8278-37E2EA19C01B" ); // Photo Request:Sender
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "036F2F0B-C2DC-49D0-A17B-CCDAC7FC71E2", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Warning Message", "WarningMessage", "A warning message that may be displayed if the person has opted out from photo requests.", 4, @"", "6B90C8F4-885C-4549-BABA-E734AC36F8F4" ); // Photo Request:Warning Message
            RockMigrationHelper.AddAttributeQualifier( "26C30D02-3B89-4EA3-AAA7-08DB2E8EE0C3", "falsetext", @"No", "6B283EEF-7FCF-4EDB-866C-3E0B3A8EEEEE" ); // Photo Request:OptOut:falsetext
            RockMigrationHelper.AddAttributeQualifier( "26C30D02-3B89-4EA3-AAA7-08DB2E8EE0C3", "truetext", @"Yes", "A1CEE907-6AF8-4314-AA38-4A6097D40853" ); // Photo Request:OptOut:truetext
            RockMigrationHelper.UpdateWorkflowActivityType( "036F2F0B-C2DC-49D0-A17B-CCDAC7FC71E2", true, "Launch From Person Profile", "When this workflow is initiated from the Person Profile page, the \"Entity\" will have a value so the first action will run successfully, and the workflow will then be persisted.", true, 0, "BBDA3B74-44A2-41C0-849B-3D8939C027FA" ); // Photo Request:Launch From Person Profile
            RockMigrationHelper.UpdateWorkflowActionForm( @"{{ Workflow.WarningMessage }}
", @"", "Send^fdc397cd-8b4a-436e-bea1-bce2e6717c03^2885B6AF-F059-465B-947F-E3D36578F67D^Your information has been submitted successfully.|", "", true, "", "61DCBFB1-9CE8-4356-82D8-D69AFE68A58C" ); // Photo Request:Launch From Person Profile:Custom Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "61DCBFB1-9CE8-4356-82D8-D69AFE68A58C", "282D67EF-73FE-42A7-A248-F2901873E758", 0, false, true, false, "ED031AF5-62BD-4DB8-A41B-9FD7EB0C150C" ); // Photo Request:Launch From Person Profile:Custom Message:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "61DCBFB1-9CE8-4356-82D8-D69AFE68A58C", "55A64AF1-E65C-4CE0-9542-9B79B256977E", 1, true, false, false, "EF3FF3B9-3C4C-4D44-BFF3-3D81E6115884" ); // Photo Request:Launch From Person Profile:Custom Message:Custom Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "61DCBFB1-9CE8-4356-82D8-D69AFE68A58C", "FA5530D4-9580-4C1F-8278-37E2EA19C01B", 2, false, true, false, "167769BA-114D-4325-8298-C86DF00A4007" ); // Photo Request:Launch From Person Profile:Custom Message:Sender
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "61DCBFB1-9CE8-4356-82D8-D69AFE68A58C", "26C30D02-3B89-4EA3-AAA7-08DB2E8EE0C3", 3, false, true, false, "C940FCCE-C024-450A-9075-6EE9B327F12A" ); // Photo Request:Launch From Person Profile:Custom Message:OptOut
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "61DCBFB1-9CE8-4356-82D8-D69AFE68A58C", "6B90C8F4-885C-4549-BABA-E734AC36F8F4", 4, false, true, false, "B2EC0C36-1ECE-4E29-9501-843D4FBE82D0" ); // Photo Request:Launch From Person Profile:Custom Message:Warning Message
            RockMigrationHelper.UpdateWorkflowActionType( "BBDA3B74-44A2-41C0-849B-3D8939C027FA", "Set Person", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "1DB4298E-AAFB-4386-8D58-E56E84F65D9B" ); // Photo Request:Launch From Person Profile:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "BBDA3B74-44A2-41C0-849B-3D8939C027FA", "Set Sender", 1, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "08D789F5-EB33-40A0-A663-337F92ACF6A6" ); // Photo Request:Launch From Person Profile:Set Sender
            RockMigrationHelper.UpdateWorkflowActionType( "BBDA3B74-44A2-41C0-849B-3D8939C027FA", "Check for Opt Out", 2, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "E0D754DC-3494-4392-8EA3-82B33679D233" ); // Photo Request:Launch From Person Profile:Check for Opt Out
            RockMigrationHelper.UpdateWorkflowActionType( "BBDA3B74-44A2-41C0-849B-3D8939C027FA", "Persist Workflow", 3, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "F8839B05-F21F-4FE2-8C98-6D0AE90903DE" ); // Photo Request:Launch From Person Profile:Persist Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "BBDA3B74-44A2-41C0-849B-3D8939C027FA", "Set Warning", 4, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "26C30D02-3B89-4EA3-AAA7-08DB2E8EE0C3", 1, "True", "9CFAA39B-C076-4E57-8A33-9F0DC9060F53" ); // Photo Request:Launch From Person Profile:Set Warning
            RockMigrationHelper.UpdateWorkflowActionType( "BBDA3B74-44A2-41C0-849B-3D8939C027FA", "Custom Message", 5, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "61DCBFB1-9CE8-4356-82D8-D69AFE68A58C", "", 1, "", "222809BA-16AD-45D1-B1E2-EFC319359321" ); // Photo Request:Launch From Person Profile:Custom Message
            RockMigrationHelper.UpdateWorkflowActionType( "BBDA3B74-44A2-41C0-849B-3D8939C027FA", "Send Email Action", 6, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "2C88DD58-40D3-4927-8A12-7968092C4929" ); // Photo Request:Launch From Person Profile:Send Email Action
            RockMigrationHelper.UpdateWorkflowActionType( "BBDA3B74-44A2-41C0-849B-3D8939C027FA", "Complete the Workflow", 7, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "FA02C10C-49A2-488C-BC20-1D5D5A4B5ECC" ); // Photo Request:Launch From Person Profile:Complete the Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "1DB4298E-AAFB-4386-8D58-E56E84F65D9B", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Photo Request:Launch From Person Profile:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1DB4298E-AAFB-4386-8D58-E56E84F65D9B", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Photo Request:Launch From Person Profile:Set Person:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "1DB4298E-AAFB-4386-8D58-E56E84F65D9B", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"282d67ef-73fe-42a7-a248-f2901873e758" ); // Photo Request:Launch From Person Profile:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "08D789F5-EB33-40A0-A663-337F92ACF6A6", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"fa5530d4-9580-4c1f-8278-37e2ea19c01b" ); // Photo Request:Launch From Person Profile:Set Sender:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "08D789F5-EB33-40A0-A663-337F92ACF6A6", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" ); // Photo Request:Launch From Person Profile:Set Sender:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "08D789F5-EB33-40A0-A663-337F92ACF6A6", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Photo Request:Launch From Person Profile:Set Sender:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E0D754DC-3494-4392-8EA3-82B33679D233", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // Photo Request:Launch From Person Profile:Check for Opt Out:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E0D754DC-3494-4392-8EA3-82B33679D233", "FA7C685D-8636-41EF-9998-90FFF3998F76", @"" ); // Photo Request:Launch From Person Profile:Check for Opt Out:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E0D754DC-3494-4392-8EA3-82B33679D233", "F3B9908B-096F-460B-8320-122CF046D1F9", @"DECLARE @PersonAliasGuid uniqueidentifier = '{{ Workflow.Person_unformatted }}'

SELECT  CASE
   WHEN EXISTS ( SELECT 1
      FROM [GroupMember] GM
      INNER JOIN [Group] G ON GM.[GroupId] = G.[Id] AND G.[Guid] = '2108EF9C-10DC-4466-973D-D25AAB7818BE'
	  INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid
      WHERE GM.PersonId = PA.[PersonId]
      AND GM.GroupMemberStatus = 0 )
    THEN 'True'
    ELSE 'False'
    END" ); // Photo Request:Launch From Person Profile:Check for Opt Out:SQLQuery

            RockMigrationHelper.AddActionTypeAttributeValue( "E0D754DC-3494-4392-8EA3-82B33679D233", "56997192-2545-4EA1-B5B2-313B04588984", @"26c30d02-3b89-4ea3-aaa7-08db2e8ee0c3" ); // Photo Request:Launch From Person Profile:Check for Opt Out:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "F8839B05-F21F-4FE2-8C98-6D0AE90903DE", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // Photo Request:Launch From Person Profile:Persist Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F8839B05-F21F-4FE2-8C98-6D0AE90903DE", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // Photo Request:Launch From Person Profile:Persist Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9CFAA39B-C076-4E57-8A33-9F0DC9060F53", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Photo Request:Launch From Person Profile:Set Warning:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9CFAA39B-C076-4E57-8A33-9F0DC9060F53", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Photo Request:Launch From Person Profile:Set Warning:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "9CFAA39B-C076-4E57-8A33-9F0DC9060F53", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"6b90c8f4-885c-4549-baba-e734ac36f8f4" ); // Photo Request:Launch From Person Profile:Set Warning:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9CFAA39B-C076-4E57-8A33-9F0DC9060F53", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"<div class=""alert alert-warning"">{{ Workflow.Person }} has previously opted out from photo requests.  Make sure you want to override this preference.</div>" ); // Photo Request:Launch From Person Profile:Set Warning:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "222809BA-16AD-45D1-B1E2-EFC319359321", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Photo Request:Launch From Person Profile:Custom Message:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "222809BA-16AD-45D1-B1E2-EFC319359321", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Photo Request:Launch From Person Profile:Custom Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2C88DD58-40D3-4927-8A12-7968092C4929", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Photo Request:Launch From Person Profile:Send Email Action:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2C88DD58-40D3-4927-8A12-7968092C4929", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Photo Request:Launch From Person Profile:Send Email Action:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "2C88DD58-40D3-4927-8A12-7968092C4929", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"fa5530d4-9580-4c1f-8278-37e2ea19c01b" ); // Photo Request:Launch From Person Profile:Send Email Action:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "2C88DD58-40D3-4927-8A12-7968092C4929", "0C4C13B8-7076-4872-925A-F950886B5E16", @"282d67ef-73fe-42a7-a248-f2901873e758" ); // Photo Request:Launch From Person Profile:Send Email Action:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "2C88DD58-40D3-4927-8A12-7968092C4929", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Photo Request from {{ Workflow.Sender }}" ); // Photo Request:Launch From Person Profile:Send Email Action:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "2C88DD58-40D3-4927-8A12-7968092C4929", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ GlobalAttribute.EmailStyles }}
{{ GlobalAttribute.EmailHeader }}
<p>{{ Person.NickName }},</p>

<p>{{ Workflow.CustomMessage | NewlineToBr }}</p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}PhotoRequest/Upload/{{ Person.UrlEncodedKey }}"">Upload Photo </a></p>

<p>Your picture will remain confidential and will only be visible to staff and volunteers in a leadership position within {{ GlobalAttribute.OrganizationName }}.</p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}PhotoRequest/OptOut/{{ Person.UrlEncodedKey }}"">I prefer not to receive future photo requests.</a></p>

<p><a href=""{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.UrlEncodedKey }}"">I&#39;m no longer involved with {{ GlobalAttribute.OrganizationName }}. Please remove me from all future communications.</a></p>

<p>-{{ Workflow.Sender }}</p>

{{ GlobalAttribute.EmailFooter }}" ); // Photo Request:Launch From Person Profile:Send Email Action:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "FA02C10C-49A2-488C-BC20-1D5D5A4B5ECC", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Photo Request:Launch From Person Profile:Complete the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FA02C10C-49A2-488C-BC20-1D5D5A4B5ECC", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Photo Request:Launch From Person Profile:Complete the Workflow:Order

            // Update the Bio block's WorkflowAction attribute value to include this new WF
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "7197A0FB-B330-43C4-8E62-F3C14F649813", "036f2f0b-c2dc-49d0-a17b-ccdac7fc71e2", appendToExisting: true );

            #endregion

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Bulk Update:Display Count
            RockMigrationHelper.DeleteAttribute( "8A3C95D6-37FD-464A-A9C8-6B6DFE946FDE" );
            // Attrib for BlockType: Bulk Update:Note Type
            RockMigrationHelper.DeleteAttribute( "F9547F97-BF1D-4F9A-AA56-04B2F6B5673C" );
            // Attrib for BlockType: Bulk Update:Attribute Categories
            RockMigrationHelper.DeleteAttribute( "B263B202-9DB3-4D16-B66A-D87D9E6EB427" );
            // Remove Block: Bulk Update, from Page: Bulk Update, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A610AB9D-7397-4D27-8614-F6A282B78B2C" );
            RockMigrationHelper.DeleteBlockType( "A844886D-ED6F-4367-9C6F-667401201ED0" ); // Bulk Update
            RockMigrationHelper.DeletePage( "B6BFDE54-0EFA-4499-847D-BE1259F83535" ); //  Page: Bulk Update, Layout: Full Width, Site: Rock RMS
        }
    }
}
