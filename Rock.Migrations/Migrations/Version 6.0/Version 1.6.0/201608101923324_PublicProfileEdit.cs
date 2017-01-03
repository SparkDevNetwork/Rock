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
    public partial class PublicProfileEdit : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Public Profile Edit", "Public block for users to manage their accounts", "~/Blocks/Cms/PublicProfileEdit.ascx", "CMS", "841D1670-8BFD-4913-8409-FB47EB7A2AB9" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Family Members", "ShowFamilyMembers", "", "Whether family members are shown or not.", 0, @"True", "7DC6314E-F510-491F-95E8-7DE7A1CB198B" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 1, @"368DD475-242C-49C4-A42C-7278BE690CC2", "F15444C3-41D1-46D0-AE87-7472B4749481" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 2, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "3D1DA9C7-8902-4C39-839F-DCB426EB3108" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "48624B0B-6A58-45B8-9E47-B67B67898D25", "Address Type", "AddressType", "", "The type of address to be displayed / edited.", 3, @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC", "B2F06218-829A-40DE-9922-87752C9CACCF" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Numbers", "PhoneNumbers", "", "The types of phone numbers to display / edit.", 4, @"AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303", "C359107A-B83A-4DEE-9CC4-742844BB2687" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Launch Page", "WorkflowLaunchPage", "", "Page used to launch the workflow to make a profile change request", 5, @"", "3DBDB8AC-D051-4459-8392-2026DACE0A25" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Family Attributes", "FamilyAttributes", "", "The family attributes that should be displayed / edited.", 6, @"", "714875EF-F019-44A4-B56F-B4F425D4F7BD" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Person Attributes (adults)", "PersonAttributes(adults)", "", "The person attributes that should be displayed / edited for adults.", 7, @"", "7C622140-54D4-4D08-9B97-B5B1D28D7CB0" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Person Attributes (children)", "PersonAttributes(children)", "", "The person attributes that should be displayed / edited for children.", 8, @"", "B601F321-D764-430E-BE19-A4E44C52F11F" );


            RockMigrationHelper.UpdateWorkflowType( false, true, "Profile Change Request", "Request a change to a person or family's profile.", "78E38655-D951-41DB-A0FF-D6474775CFA1", "Profile Change Request", "fa fa-user", 0, false, 0, "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D" ); // Profile Change Request
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requester", "Requester", "The person who made the request", 1, @"", "C63F3D35-18CE-40F2-814A-643719DEFA97" ); // Profile Change Request:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Worker", "Worker", "The person assigned to work on the request", 2, @"", "7D5086E8-B38A-4E65-AE7C-1F219A2EE561" ); // Profile Change Request:Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "New Worker", "NewWorker", "If the request needs to be re-assigned to a different person, select the new person here.", 3, @"", "CDA5E614-E808-4B29-B431-0534B5326C89" ); // Profile Change Request:New Worker
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Internal Notes", "Notes", "Notes about the request for internal use ( Resolution will be entered when completing the request )", 4, @"", "505BE190-BFFC-403A-AD1D-649F384BFDC9" ); // Profile Change Request:Internal Notes
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Resolution", "Resolution", "How the issue was resolved", 5, @"", "AA4273DE-6A19-4B18-8A36-1FB34E6AAA93" ); // Profile Change Request:Resolution
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Request", "Request", "Details about the requested profile change.", 0, @"", "1F077D6B-2EB5-43B1-A6C8-DAEBF37FE2EA" ); // Profile Change Request:Request
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Notify Requester", "NotifyRequester", "Determines whether the requester is notified upon completion of the request.", 6, @"", "08D347FE-7435-40A8-A346-4C6DB3308E3D" ); // Profile Change Request:Notify Requester
            RockMigrationHelper.AddAttributeQualifier( "1F077D6B-2EB5-43B1-A6C8-DAEBF37FE2EA", "allowhtml", @"False", "AC80DCFD-7B37-4D7F-A26E-637148C6E0FA" ); // Profile Change Request:Request:allowhtml
            RockMigrationHelper.AddAttributeQualifier( "1F077D6B-2EB5-43B1-A6C8-DAEBF37FE2EA", "numberofrows", @"", "2F540206-6B83-4152-8B7D-996F7DE7EF90" ); // Profile Change Request:Request:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "08D347FE-7435-40A8-A346-4C6DB3308E3D", "falsetext", @"No", "ABA9190A-E564-4B19-91AC-EF15CDE7D430" ); // Profile Change Request:Notify Requester:falsetext
            RockMigrationHelper.AddAttributeQualifier( "08D347FE-7435-40A8-A346-4C6DB3308E3D", "truetext", @"Yes", "DC8A835E-2523-4782-B59E-AAD602C9BBD5" ); // Profile Change Request:Notify Requester:truetext
            RockMigrationHelper.UpdateWorkflowActivityType( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", true, "Request", "Prompt the user for the information about their request", true, 0, "0205C758-8F12-4C5C-99B8-A9A686859115" ); // Profile Change Request:Request
            RockMigrationHelper.UpdateWorkflowActivityType( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", true, "Assign Worker", "Assigns the request to a worker", false, 1, "059A7871-4C57-4977-B30B-D6E324A28179" ); // Profile Change Request:Assign Worker
            RockMigrationHelper.UpdateWorkflowActivityType( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", true, "Open", "Activity used to process the request", false, 2, "84FB05AD-74D6-4A3F-8B2C-38F44FD0F203" ); // Profile Change Request:Open
            RockMigrationHelper.UpdateWorkflowActivityType( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", true, "Enter Resolution", "Prompts worker for resolution", false, 3, "D862FCCD-5CA7-4E85-8171-CFA2D27D4903" ); // Profile Change Request:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActivityType( "F5AF8224-44DC-4918-AAB7-C7C9A5A6338D", true, "Complete", "Complete the workflow", false, 4, "85FB6477-7B77-4F38-AA09-E485AD49DE56" ); // Profile Change Request:Complete
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "84FB05AD-74D6-4A3F-8B2C-38F44FD0F203", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Selected Action", "SelectedAction", "The action that was selected by user", 0, @"", "8B29AAB1-03DC-4AB9-A5C4-9599D1EF3A66" ); // Profile Change Request:Open:Selected Action
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h2>Request</h2>
<p>
Complete the form below to request a change to you or your family's profile.
</p>
<br/>", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^059A7871-4C57-4977-B30B-D6E324A28179^Thank You. Your request has been forwarded one of our staff who will be following up with you soon.|", "", true, "", "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802" ); // Profile Change Request:Request:Prompt User
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow.Title }}</h4>
", @"", "Save^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^The information you entered has been saved.|Done^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^", "", true, "8B29AAB1-03DC-4AB9-A5C4-9599D1EF3A66", "109A961B-2B44-4080-958C-CC577A7F6245" ); // Profile Change Request:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow.Title }}</h4>", @"<p>Select Submit to close this workflow.</p>", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^85fb6477-7b77-4f38-aa09-e485ad49de56^Your information has been submitted successfully.|Cancel^8cf6e927-4fa5-4241-991c-391038b79631^84fb05ad-74d6-4a3f-8b2c-38f44fd0f203^", "", true, "", "112575F7-F513-47FE-B653-E0ABE1D27D08" ); // Profile Change Request:Enter Resolution:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802", "1F077D6B-2EB5-43B1-A6C8-DAEBF37FE2EA", 0, true, false, true, "80535237-D008-4B02-983A-9007E3788507" ); // Profile Change Request:Request:Prompt User:Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802", "C63F3D35-18CE-40F2-814A-643719DEFA97", 1, false, true, false, "3F1186BB-895C-4D0B-8676-272EC529E00A" ); // Profile Change Request:Request:Prompt User:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802", "7D5086E8-B38A-4E65-AE7C-1F219A2EE561", 2, false, true, false, "C2DC5F5C-D94A-41DB-B1CC-98567B283E7D" ); // Profile Change Request:Request:Prompt User:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802", "AA4273DE-6A19-4B18-8A36-1FB34E6AAA93", 3, false, true, false, "4639485E-B1F8-4666-8359-35E09B741F8C" ); // Profile Change Request:Request:Prompt User:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802", "CDA5E614-E808-4B29-B431-0534B5326C89", 4, false, true, false, "7DBCFDB7-5922-4B4D-9759-F07997759CFD" ); // Profile Change Request:Request:Prompt User:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802", "505BE190-BFFC-403A-AD1D-649F384BFDC9", 5, false, true, false, "0B555507-EB33-4625-87F8-3EF3FF54544F" ); // Profile Change Request:Request:Prompt User:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802", "08D347FE-7435-40A8-A346-4C6DB3308E3D", 6, false, true, false, "1EEB04B5-7CF4-485B-8606-B77728AF6EBD" ); // Profile Change Request:Request:Prompt User:Notify Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "109A961B-2B44-4080-958C-CC577A7F6245", "C63F3D35-18CE-40F2-814A-643719DEFA97", 0, true, true, false, "5663A8C0-E88D-4B45-93A2-C031F00B66B4" ); // Profile Change Request:Open:Capture Notes:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "109A961B-2B44-4080-958C-CC577A7F6245", "1F077D6B-2EB5-43B1-A6C8-DAEBF37FE2EA", 1, true, true, false, "5C013068-E4C8-495B-9F25-235215F1580B" ); // Profile Change Request:Open:Capture Notes:Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "109A961B-2B44-4080-958C-CC577A7F6245", "7D5086E8-B38A-4E65-AE7C-1F219A2EE561", 2, false, true, false, "EAA1A7D1-E053-4262-89F5-FCEA87D50BD1" ); // Profile Change Request:Open:Capture Notes:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "109A961B-2B44-4080-958C-CC577A7F6245", "CDA5E614-E808-4B29-B431-0534B5326C89", 3, true, false, false, "DFE848A3-88FF-40B2-B6D8-E474C645DDEC" ); // Profile Change Request:Open:Capture Notes:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "109A961B-2B44-4080-958C-CC577A7F6245", "505BE190-BFFC-403A-AD1D-649F384BFDC9", 4, true, false, false, "33F815B1-0146-42EA-9DEF-AC1A50E2CE4D" ); // Profile Change Request:Open:Capture Notes:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "109A961B-2B44-4080-958C-CC577A7F6245", "AA4273DE-6A19-4B18-8A36-1FB34E6AAA93", 5, false, true, false, "E7E8B87C-DDD3-4403-A11F-8AA444CE14E8" ); // Profile Change Request:Open:Capture Notes:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "109A961B-2B44-4080-958C-CC577A7F6245", "8B29AAB1-03DC-4AB9-A5C4-9599D1EF3A66", 6, false, true, false, "E1AA2DDF-9EC9-4CC1-9323-CE10946895C7" ); // Profile Change Request:Open:Capture Notes:Selected Action
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "109A961B-2B44-4080-958C-CC577A7F6245", "08D347FE-7435-40A8-A346-4C6DB3308E3D", 7, false, true, false, "38F80384-4E9C-434C-B030-236F8B22E763" ); // Profile Change Request:Open:Capture Notes:Notify Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "112575F7-F513-47FE-B653-E0ABE1D27D08", "1F077D6B-2EB5-43B1-A6C8-DAEBF37FE2EA", 0, true, true, false, "4A7916E4-F4B1-4DCF-8DFA-EC18AD296FDE" ); // Profile Change Request:Enter Resolution:Enter Resolution:Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "112575F7-F513-47FE-B653-E0ABE1D27D08", "C63F3D35-18CE-40F2-814A-643719DEFA97", 1, true, true, false, "27D8BE60-5DFD-4287-9369-7C96B8CD5F2D" ); // Profile Change Request:Enter Resolution:Enter Resolution:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "112575F7-F513-47FE-B653-E0ABE1D27D08", "7D5086E8-B38A-4E65-AE7C-1F219A2EE561", 2, false, true, false, "B79ACEF3-99BF-40AA-A668-01C967312AA7" ); // Profile Change Request:Enter Resolution:Enter Resolution:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "112575F7-F513-47FE-B653-E0ABE1D27D08", "CDA5E614-E808-4B29-B431-0534B5326C89", 3, false, true, false, "CDF566B2-E861-41B4-B578-D5A6BE5B0CB6" ); // Profile Change Request:Enter Resolution:Enter Resolution:New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "112575F7-F513-47FE-B653-E0ABE1D27D08", "505BE190-BFFC-403A-AD1D-649F384BFDC9", 4, false, true, false, "26DD8EDB-2132-473C-A1EE-697734E6E2CF" ); // Profile Change Request:Enter Resolution:Enter Resolution:Internal Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "112575F7-F513-47FE-B653-E0ABE1D27D08", "AA4273DE-6A19-4B18-8A36-1FB34E6AAA93", 5, true, false, false, "066CE466-0C57-4809-836A-DA15FF1F38FE" ); // Profile Change Request:Enter Resolution:Enter Resolution:Resolution
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "112575F7-F513-47FE-B653-E0ABE1D27D08", "08D347FE-7435-40A8-A346-4C6DB3308E3D", 6, true, false, false, "C02A5C52-7811-4EC2-99C5-0914788089D0" ); // Profile Change Request:Enter Resolution:Enter Resolution:Notify Requester
            RockMigrationHelper.UpdateWorkflowActionType( "059A7871-4C57-4977-B30B-D6E324A28179", "Activate Open Activity", 3, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "", 1, "", "E4009463-8958-459E-9E5E-913FA5FCCA65" ); // Profile Change Request:Assign Worker:Activate Open Activity
            RockMigrationHelper.UpdateWorkflowActionType( "84FB05AD-74D6-4A3F-8B2C-38F44FD0F203", "Done", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "8B29AAB1-03DC-4AB9-A5C4-9599D1EF3A66", 1, "Done", "EF006EF8-4E40-4DC7-ACCD-863DC088526B" ); // Profile Change Request:Open:Done
            RockMigrationHelper.UpdateWorkflowActionType( "84FB05AD-74D6-4A3F-8B2C-38F44FD0F203", "Assign New Worker", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "CDA5E614-E808-4B29-B431-0534B5326C89", 64, "", "8C5EC8A5-FA6F-40FB-BF4C-497055B1979F" ); // Profile Change Request:Open:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "85FB6477-7B77-4F38-AA09-E485AD49DE56", "Complete the Workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "96EC11AA-49B0-41B0-8E14-91331CFAEB95" ); // Profile Change Request:Complete:Complete the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "85FB6477-7B77-4F38-AA09-E485AD49DE56", "Notify Requester", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "08D347FE-7435-40A8-A346-4C6DB3308E3D", 1, "true", "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349" ); // Profile Change Request:Complete:Notify Requester
            RockMigrationHelper.UpdateWorkflowActionType( "059A7871-4C57-4977-B30B-D6E324A28179", "Notify Worker", 1, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "5F68A391-B4E4-4019-861C-1CD6AAB353A7" ); // Profile Change Request:Assign Worker:Notify Worker
            RockMigrationHelper.UpdateWorkflowActionType( "84FB05AD-74D6-4A3F-8B2C-38F44FD0F203", "Re-Activate This Activity", 4, "699756EF-28EB-444B-BD28-15F0A167E614", false, false, "", "8B29AAB1-03DC-4AB9-A5C4-9599D1EF3A66", 1, "Save", "BADCEDB6-61B8-4539-B602-F24C2157DD08" ); // Profile Change Request:Open:Re-Activate This Activity
            RockMigrationHelper.UpdateWorkflowActionType( "D862FCCD-5CA7-4E85-8171-CFA2D27D4903", "Assign Activity", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "C7AC248C-B866-4F4A-98A9-D978A29EB538" ); // Profile Change Request:Enter Resolution:Assign Activity
            RockMigrationHelper.UpdateWorkflowActionType( "84FB05AD-74D6-4A3F-8B2C-38F44FD0F203", "Assign Activity to Worker", 0, "F100A31F-E93A-4C7A-9E55-0FAF41A101C4", true, false, "", "", 1, "", "D3537337-9D1E-4DE3-8BC1-F35CBDD03FF1" ); // Profile Change Request:Open:Assign Activity to Worker
            RockMigrationHelper.UpdateWorkflowActionType( "0205C758-8F12-4C5C-99B8-A9A686859115", "Persist the Workflow", 4, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "19456461-73CB-49F6-899E-8CB21E98BDA1" ); // Profile Change Request:Request:Persist the Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "0205C758-8F12-4C5C-99B8-A9A686859115", "Set Requester", 1, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "65E27C13-A0F5-4056-ABDD-08E14ED75463" ); // Profile Change Request:Request:Set Requester
            RockMigrationHelper.UpdateWorkflowActionType( "059A7871-4C57-4977-B30B-D6E324A28179", "Set Worker", 0, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "0EF3BE5F-6F64-4364-B2A9-E287342E70E3" ); // Profile Change Request:Assign Worker:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "0205C758-8F12-4C5C-99B8-A9A686859115", "Set Name", 2, "36005473-BD5D-470B-B28D-98E6D7ED808D", true, false, "", "", 1, "", "6329F903-644B-4DE4-9962-23A2EEBA6A7F" ); // Profile Change Request:Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType( "0205C758-8F12-4C5C-99B8-A9A686859115", "Set Worker", 3, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "5C50C233-C27F-41F4-9015-AE5023FDCB4C" ); // Profile Change Request:Request:Set Worker
            RockMigrationHelper.UpdateWorkflowActionType( "059A7871-4C57-4977-B30B-D6E324A28179", "Clear New Worker", 2, "17962C23-2E94-4E06-8461-0FB8B94E2FEA", true, false, "", "", 1, "", "0B69B2C8-B94C-4DB9-8222-A4F3156AFCD1" ); // Profile Change Request:Assign Worker:Clear New Worker
            RockMigrationHelper.UpdateWorkflowActionType( "84FB05AD-74D6-4A3F-8B2C-38F44FD0F203", "Capture Notes", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "109A961B-2B44-4080-958C-CC577A7F6245", "", 1, "", "E9B7712D-4815-4737-BAEE-06AFB798FCA7" ); // Profile Change Request:Open:Capture Notes
            RockMigrationHelper.UpdateWorkflowActionType( "D862FCCD-5CA7-4E85-8171-CFA2D27D4903", "Enter Resolution", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "112575F7-F513-47FE-B653-E0ABE1D27D08", "", 1, "", "49348E9D-B7A4-482D-AE82-1092CF0A1A49" ); // Profile Change Request:Enter Resolution:Enter Resolution
            RockMigrationHelper.UpdateWorkflowActionType( "0205C758-8F12-4C5C-99B8-A9A686859115", "Prompt User", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "0ACA3EA8-173B-43E7-A1A0-6DBFF3079802", "", 1, "", "D6DD14F7-14FC-4D49-81DA-EC1F971B4CF8" ); // Profile Change Request:Request:Prompt User
            RockMigrationHelper.AddActionTypeAttributeValue( "D6DD14F7-14FC-4D49-81DA-EC1F971B4CF8", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Profile Change Request:Request:Prompt User:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D6DD14F7-14FC-4D49-81DA-EC1F971B4CF8", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Profile Change Request:Request:Prompt User:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "65E27C13-A0F5-4056-ABDD-08E14ED75463", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Profile Change Request:Request:Set Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "65E27C13-A0F5-4056-ABDD-08E14ED75463", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"c63f3d35-18ce-40f2-814a-643719defa97" ); // Profile Change Request:Request:Set Requester:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "65E27C13-A0F5-4056-ABDD-08E14ED75463", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8", @"" ); // Profile Change Request:Request:Set Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "6329F903-644B-4DE4-9962-23A2EEBA6A7F", "0A800013-51F7-4902-885A-5BE215D67D3D", @"False" ); // Profile Change Request:Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6329F903-644B-4DE4-9962-23A2EEBA6A7F", "5D95C15A-CCAE-40AD-A9DD-F929DA587115", @"" ); // Profile Change Request:Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "6329F903-644B-4DE4-9962-23A2EEBA6A7F", "93852244-A667-4749-961A-D47F88675BE4", @"8a05b9ec-1fe7-48a0-8ab6-f2aaf54ad2c2" ); // Profile Change Request:Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "5C50C233-C27F-41F4-9015-AE5023FDCB4C", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Profile Change Request:Request:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5C50C233-C27F-41F4-9015-AE5023FDCB4C", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"cda5e614-e808-4b29-b431-0534b5326c89" ); // Profile Change Request:Request:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5C50C233-C27F-41F4-9015-AE5023FDCB4C", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Profile Change Request:Request:Set Worker:Order
            RockMigrationHelper.AddActionTypePersonAttributeValue( "5C50C233-C27F-41F4-9015-AE5023FDCB4C", "5C803BD1-40FA-49B1-AE7E-68F43D3687BB", @"cbee12a3-a758-4444-a6ae-acf2f4a57643" ); // Profile Change Request:Request:Set Worker:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "19456461-73CB-49F6-899E-8CB21E98BDA1", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // Profile Change Request:Request:Persist the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "19456461-73CB-49F6-899E-8CB21E98BDA1", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361", @"" ); // Profile Change Request:Request:Persist the Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "19456461-73CB-49F6-899E-8CB21E98BDA1", "56AC2C84-44BA-484D-87DC-015A72963EF3", @"False" ); // Profile Change Request:Request:Persist the Workflow:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue( "0EF3BE5F-6F64-4364-B2A9-E287342E70E3", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Profile Change Request:Assign Worker:Set Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0EF3BE5F-6F64-4364-B2A9-E287342E70E3", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"7d5086e8-b38a-4e65-ae7c-1f219a2ee561" ); // Profile Change Request:Assign Worker:Set Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0EF3BE5F-6F64-4364-B2A9-E287342E70E3", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Profile Change Request:Assign Worker:Set Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "0EF3BE5F-6F64-4364-B2A9-E287342E70E3", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"cda5e614-e808-4b29-b431-0534b5326c89" ); // Profile Change Request:Assign Worker:Set Worker:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "5F68A391-B4E4-4019-861C-1CD6AAB353A7", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Profile Change Request:Assign Worker:Notify Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5F68A391-B4E4-4019-861C-1CD6AAB353A7", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Profile Change Request:Assign Worker:Notify Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "5F68A391-B4E4-4019-861C-1CD6AAB353A7", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Profile Change Request:Assign Worker:Notify Worker:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "5F68A391-B4E4-4019-861C-1CD6AAB353A7", "0C4C13B8-7076-4872-925A-F950886B5E16", @"7d5086e8-b38a-4e65-ae7c-1f219a2ee561" ); // Profile Change Request:Assign Worker:Notify Worker:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "5F68A391-B4E4-4019-861C-1CD6AAB353A7", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"{{ Workflow.Requester }} Profile Change Request" ); // Profile Change Request:Assign Worker:Notify Worker:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "5F68A391-B4E4-4019-861C-1CD6AAB353A7", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
<h4>Profile Request Change for {{ Workflow.Requester }}</h4>
<p>The following change request has been submitted by {{ Workflow.Requester }} from the external website:</p>
<p>{{ Workflow.Request }}</p>

<p><a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}'>{{ Workflow.Name }}</a></p>


{{ 'Global' | Attribute:'EmailFooter' }}

" ); // Profile Change Request:Assign Worker:Notify Worker:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "5F68A391-B4E4-4019-861C-1CD6AAB353A7", "E1C1C1CB-B5EF-4927-9DF4-2299CF545CE0", @"False" ); // Profile Change Request:Assign Worker:Notify Worker:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "0B69B2C8-B94C-4DB9-8222-A4F3156AFCD1", "CE28B79D-FBC2-4894-9198-D923D0217549", @"False" ); // Profile Change Request:Assign Worker:Clear New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0B69B2C8-B94C-4DB9-8222-A4F3156AFCD1", "7AC47975-71AC-4A2F-BF1F-115CF5578D6F", @"cda5e614-e808-4b29-b431-0534b5326c89" ); // Profile Change Request:Assign Worker:Clear New Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0B69B2C8-B94C-4DB9-8222-A4F3156AFCD1", "18EF907D-607E-4891-B034-7AA379D77854", @"" ); // Profile Change Request:Assign Worker:Clear New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E4009463-8958-459E-9E5E-913FA5FCCA65", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Profile Change Request:Assign Worker:Activate Open Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E4009463-8958-459E-9E5E-913FA5FCCA65", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Profile Change Request:Assign Worker:Activate Open Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E4009463-8958-459E-9E5E-913FA5FCCA65", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"84FB05AD-74D6-4A3F-8B2C-38F44FD0F203" ); // Profile Change Request:Assign Worker:Activate Open Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "D3537337-9D1E-4DE3-8BC1-F35CBDD03FF1", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // Profile Change Request:Open:Assign Activity to Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D3537337-9D1E-4DE3-8BC1-F35CBDD03FF1", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"7d5086e8-b38a-4e65-ae7c-1f219a2ee561" ); // Profile Change Request:Open:Assign Activity to Worker:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D3537337-9D1E-4DE3-8BC1-F35CBDD03FF1", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // Profile Change Request:Open:Assign Activity to Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E9B7712D-4815-4737-BAEE-06AFB798FCA7", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Profile Change Request:Open:Capture Notes:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E9B7712D-4815-4737-BAEE-06AFB798FCA7", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Profile Change Request:Open:Capture Notes:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EF006EF8-4E40-4DC7-ACCD-863DC088526B", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Profile Change Request:Open:Done:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EF006EF8-4E40-4DC7-ACCD-863DC088526B", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Profile Change Request:Open:Done:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EF006EF8-4E40-4DC7-ACCD-863DC088526B", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"D862FCCD-5CA7-4E85-8171-CFA2D27D4903" ); // Profile Change Request:Open:Done:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "8C5EC8A5-FA6F-40FB-BF4C-497055B1979F", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Profile Change Request:Open:Assign New Worker:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8C5EC8A5-FA6F-40FB-BF4C-497055B1979F", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Profile Change Request:Open:Assign New Worker:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "8C5EC8A5-FA6F-40FB-BF4C-497055B1979F", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"059A7871-4C57-4977-B30B-D6E324A28179" ); // Profile Change Request:Open:Assign New Worker:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "BADCEDB6-61B8-4539-B602-F24C2157DD08", "A134F1A7-3824-43E0-9EB1-22C899B795BD", @"False" ); // Profile Change Request:Open:Re-Activate This Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BADCEDB6-61B8-4539-B602-F24C2157DD08", "5DA71523-E8B0-4C4D-89A4-B47945A22A0C", @"" ); // Profile Change Request:Open:Re-Activate This Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "C7AC248C-B866-4F4A-98A9-D978A29EB538", "E0F7AB7E-7761-4600-A099-CB14ACDBF6EF", @"False" ); // Profile Change Request:Enter Resolution:Assign Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C7AC248C-B866-4F4A-98A9-D978A29EB538", "FBADD25F-D309-4512-8430-3CC8615DD60E", @"7d5086e8-b38a-4e65-ae7c-1f219a2ee561" ); // Profile Change Request:Enter Resolution:Assign Activity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C7AC248C-B866-4F4A-98A9-D978A29EB538", "7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA", @"" ); // Profile Change Request:Enter Resolution:Assign Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "49348E9D-B7A4-482D-AE82-1092CF0A1A49", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Profile Change Request:Enter Resolution:Enter Resolution:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "49348E9D-B7A4-482D-AE82-1092CF0A1A49", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Profile Change Request:Enter Resolution:Enter Resolution:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Profile Change Request:Complete:Notify Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Profile Change Request:Complete:Notify Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Profile Change Request:Complete:Notify Requester:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349", "0C4C13B8-7076-4872-925A-F950886B5E16", @"c63f3d35-18ce-40f2-814a-643719defa97" ); // Profile Change Request:Complete:Notify Requester:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"IT Support Request Completed" ); // Profile Change Request:Complete:Notify Requester:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>Your profile change request has been completed by {{ Workflow.Worker }}:</p>

<strong>Request</strong>
<p>{{ Workflow.Request }}</p>

<strong>Resolution</strong>
<p>{{ Workflow.Resolution }}</p>

{{ 'Global' | Attribute:'EmailFooter' }}

" ); // Profile Change Request:Complete:Notify Requester:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349", "E1C1C1CB-B5EF-4927-9DF4-2299CF545CE0", @"False" ); // Profile Change Request:Complete:Notify Requester:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "96EC11AA-49B0-41B0-8E14-91331CFAEB95", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Profile Change Request:Complete:Complete the Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "96EC11AA-49B0-41B0-8E14-91331CFAEB95", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Profile Change Request:Complete:Complete the Workflow:Order

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "3D1DA9C7-8902-4C39-839F-DCB426EB3108" );
            RockMigrationHelper.DeleteAttribute( "F15444C3-41D1-46D0-AE87-7472B4749481" );
            RockMigrationHelper.DeleteAttribute( "7DC6314E-F510-491F-95E8-7DE7A1CB198B" );
            RockMigrationHelper.DeleteAttribute( "B601F321-D764-430E-BE19-A4E44C52F11F" );
            RockMigrationHelper.DeleteAttribute( "7C622140-54D4-4D08-9B97-B5B1D28D7CB0" );
            RockMigrationHelper.DeleteAttribute( "714875EF-F019-44A4-B56F-B4F425D4F7BD" );
            RockMigrationHelper.DeleteAttribute( "3DBDB8AC-D051-4459-8392-2026DACE0A25" );
            RockMigrationHelper.DeleteAttribute( "C359107A-B83A-4DEE-9CC4-742844BB2687" );
            RockMigrationHelper.DeleteAttribute( "B2F06218-829A-40DE-9922-87752C9CACCF" );
            RockMigrationHelper.DeleteBlockType( "841D1670-8BFD-4913-8409-FB47EB7A2AB9" );
        }
    }
}
