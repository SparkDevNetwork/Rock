// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class BackgroundCheckSubmitHandler : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add the two new workflow attributes for storing result of submitting request to PMM
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "16D12EF7-C546-4039-9036-B73D118EDC90", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Request Status", "RequestStatus", "", 0, @"", "6ED5078E-3AD4-4576-A707-0BB4503AAF63" ); // Background Check:Request Status
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "16D12EF7-C546-4039-9036-B73D118EDC90", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Request Message", "RequestMessage", "", 0, @"", "CF4F2746-67E2-4C1D-A48E-83A77714D7DD" ); // Background Check:Request Message

            // Add a new Activity for handling an unsuccessful request to PMM
            RockMigrationHelper.UpdateWorkflowActivityType( "16D12EF7-C546-4039-9036-B73D118EDC90", true, "Request Error", "Displays any error from the request that is submitted to PMM and allows request to be resubmitted", false, 99, "E51407E3-65A1-4A6C-A51D-8DE6816BEBDA" ); // Background Check:Request Error

            // Action to set the status
            RockMigrationHelper.UpdateWorkflowActionType( "E51407E3-65A1-4A6C-A51D-8DE6816BEBDA", "Set Status", 0, "96D371A7-A291-4F8F-8B38-B8F72CE5407E", true, false, "", "", 1, "", "FB99C6A1-CEA1-4854-8DC8-5D06FCA1EFFB" ); // Background Check:Request Error:Set Status
            RockMigrationHelper.AddActionTypeAttributeValue( "FB99C6A1-CEA1-4854-8DC8-5D06FCA1EFFB", "36CE41F4-4C87-4096-B0C6-8269163BCC0A", @"False" ); // Background Check:Request Error:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FB99C6A1-CEA1-4854-8DC8-5D06FCA1EFFB", "91A9F4BE-4A8E-430A-B466-A88DB2D33B34", @"Request Error" ); // Background Check:Request Error:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue( "FB99C6A1-CEA1-4854-8DC8-5D06FCA1EFFB", "AE8C180C-E370-414A-B10D-97891B95D105", @"" ); // Background Check:Request Error:Set Status:Order

            // Action to assign the activity 
            RockMigrationHelper.UpdateWorkflowActionType( "E51407E3-65A1-4A6C-A51D-8DE6816BEBDA", "Assign to Security", 1, "DB2D8C44-6E57-4B45-8973-5DE327D61554", true, false, "", "", 1, "", "665C7F8A-3E6C-4A84-9C54-555EEDABDD04" ); // Background Check:Request Error:Assign to Security
            RockMigrationHelper.AddActionTypeAttributeValue( "665C7F8A-3E6C-4A84-9C54-555EEDABDD04", "C0D75D1A-16C5-4786-A1E0-25669BEE8FE9", @"False" ); // Background Check:Request Error:Assign to Security:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "665C7F8A-3E6C-4A84-9C54-555EEDABDD04", "041B7B51-A694-4AF5-B455-64D0DE7160A2", @"" ); // Background Check:Request Error:Assign to Security:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "665C7F8A-3E6C-4A84-9C54-555EEDABDD04", "BBFAD050-5968-4D11-8887-2FF877D8C8AB", @"aece949f-704c-483e-a4fb-93d5e4720c4c|a6bcc49e-103f-46b0-8bac-84ea03ff04d5" ); // Background Check:Request Error:Assign to Security:Group

            // Action to display a form showing user the error message
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h1>Background Request Details</h1>
<div class='alert alert-danger'>
    An error occurred when submitting this request to Protect My Ministry. See details below.
</div>", @"", "Re-Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^4F9B0DBB-77CD-4BB9-BF6D-2C357D64A956^Your information has been submitted successfully.|", "", true, "", "AC7383E0-0B80-44B9-997D-BA2AF67B2C76" ); // Background Check:Request Error:Display Error Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "79EF6D66-CB5E-4514-8652-120F362A7EF7", 0, false, true, false, "4E15F97C-A095-4A52-83C5-123858F6A93E" ); // Background Check:Request Error:Display Error Message:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "6ED5078E-3AD4-4576-A707-0BB4503AAF63", 1, true, true, false, "F705FB92-3A59-4730-BE6C-49BCDE0B11FD" ); // Background Check:Request Error:Display Error Message:Request Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "CF4F2746-67E2-4C1D-A48E-83A77714D7DD", 2, true, true, false, "565EB113-B973-44B2-8FA7-82CDA75B5132" ); // Background Check:Request Error:Display Error Message:Request Message
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "7306A2BC-F6EB-4A81-921F-30CEA98AD489", 3, false, true, false, "2B918F5B-77B9-4CDE-BB52-E45756D41C36" ); // Background Check:Request Error:Display Error Message:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "8C47E696-3749-4C49-942C-12AE9FA1AE1C", 4, false, true, false, "E5FF74F4-E7A5-41D8-85EC-FA1FDB661E57" ); // Background Check:Request Error:Display Error Message:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "79061845-E1AA-4FEB-8387-921ECAAEEF6F", 5, false, true, false, "8BC4FA37-1590-4F31-A840-B8044F7BDCB9" ); // Background Check:Request Error:Display Error Message:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "BBD98D86-166C-4904-AD22-B58EB9B006D9", 6, true, true, false, "859569A9-94B9-4991-982B-17263F261D1E" ); // Background Check:Request Error:Display Error Message:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "AF3F0233-9786-422D-83C8-A7565D99A01D", 7, true, true, false, "989A3217-32AA-4CB6-949E-C945E4D987B9" ); // Background Check:Request Error:Display Error Message:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "43FB2810-DFF3-40E5-9EC6-FC047A6061A1", 8, false, true, false, "2BDDF288-0F01-457A-87CA-F980C79D8038" ); // Background Check:Request Error:Display Error Message:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "A5255A04-4832-45C3-A727-00395D082D23", 9, true, true, false, "3452253F-6C6C-4FF9-A4E7-75B6E3173114" ); // Background Check:Request Error:Display Error Message:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "1FAB9A4C-C5A2-4938-B9BD-80935F0A598C", 10, true, false, false, "6639D9AD-E520-416E-ADDB-B7CD66FA5183" ); // Background Check:Request Error:Display Error Message:SSN
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "A4CB9461-D77F-40E0-8DFF-C7838D78F2EC", 11, true, false, true, "F64D1372-E75F-4FCE-8571-5CFEA97411A1" ); // Background Check:Request Error:Display Error Message:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "6D5417F5-C6DB-4FCF-90B3-7BEFB4A73F8E", 12, false, true, false, "1BB2B79F-71D9-469C-BB76-0E039DB4F8D7" ); // Background Check:Request Error:Display Error Message:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3", 13, false, true, false, "38739981-0D46-4BB8-B58D-CF814AF2DCAA" ); // Background Check:Request Error:Display Error Message:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "3211A344-7959-4A87-8557-C33B2554208A", 14, false, true, false, "81F18582-66C1-4D2D-BFA8-9CF51812FDDB" ); // Background Check:Request Error:Display Error Message:Report Link
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "F54BC71C-5C93-4EA9-88DF-4B89F457BA5D", 15, false, true, false, "02AA400B-1A24-452A-BA52-2EA342E86A5E" ); // Background Check:Request Error:Display Error Message:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "E6E5CF21-5A49-4630-9E18-531FF354380E", 16, false, true, false, "7D8A9740-2B52-4264-AABF-F5F33CE75DDF" ); // Background Check:Request Error:Display Error Message:Report

            RockMigrationHelper.UpdateWorkflowActionType( "E51407E3-65A1-4A6C-A51D-8DE6816BEBDA", "Display Error Message", 2, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "AC7383E0-0B80-44B9-997D-BA2AF67B2C76", "", 1, "", "619C76F9-97D5-4E4C-B085-0FCDE5554790" ); // Background Check:Request Error:Display Error Message
            RockMigrationHelper.AddActionTypeAttributeValue( "619C76F9-97D5-4E4C-B085-0FCDE5554790", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Background Check:Request Error:Display Error Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "619C76F9-97D5-4E4C-B085-0FCDE5554790", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // Background Check:Request Error:Display Error Message:Order

            // Add a new action to the process activity that will check for an unsuccessful reply from PMM and activate the new activity.
            RockMigrationHelper.UpdateWorkflowActionType( "4F9B0DBB-77CD-4BB9-BF6D-2C357D64A956", "Process Request Error", 2, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "6ED5078E-3AD4-4576-A707-0BB4503AAF63", 2, "SUCCESS", "CCE5B3EF-93E3-46C6-BE37-21C1F1D23A4A" ); // Background Check:Submit Request:Process Request Error
            RockMigrationHelper.AddActionTypeAttributeValue( "CCE5B3EF-93E3-46C6-BE37-21C1F1D23A4A", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Background Check:Submit Request:Process Request Error:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CCE5B3EF-93E3-46C6-BE37-21C1F1D23A4A", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Background Check:Submit Request:Process Request Error:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "CCE5B3EF-93E3-46C6-BE37-21C1F1D23A4A", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"E51407E3-65A1-4A6C-A51D-8DE6816BEBDA" ); // Background Check:Submit Request:Process Request Error:Activity

            // Update the order for the new activity (and all the subsequent activities)
            Sql( @"
    -- Get the workflow type id
    DECLARE @WorkflowTypeId int = (
        SELECT TOP 1 [Id]
        FROM [WorkflowType]
        WHERE [Guid] = '16D12EF7-C546-4039-9036-B73D118EDC90'
    )

    -- Find the action that submits a background check and get its action and activity type ids
    DECLARE @SubmitActivityTypeId int 
    DECLARE @SubmitActionTypeId int
    SELECT TOP 1
        @SubmitActivityTypeId = T.[Id],
        @SubmitActionTypeId = A.[Id]
    FROM [WorkflowActionType] A
    INNER JOIN [EntityType] E ON E.[Id] = A.[EntityTypeId]
    INNER JOIN [WorkflowActivityType] T ON T.[Id] = A.[ActivityTypeId]
    WHERE T.[WorkflowTypeId] = @WorkflowTypeId
    AND E.[Guid] = 'C4DAE3D6-931F-497F-AC00-60BAFA87B758' 

    IF @SubmitActivityTypeId IS NOT NULL AND @SubmitActionTypeId IS NOT NULL
    BEGIN

        -- Get current order of 'submit' action
        DECLARE @ActionOrder int = ( 
            SELECT [Order] 
            FROM [WorkflowActionType] 
            WHERE [Id] = @SubmitActionTypeId
        )

        UPDATE [WorkflowActionType] SET [Order] = [Order] + 1
        WHERE [ActivityTypeId] = @SubmitActivityTypeId
        AND [Order] > @ActionOrder

        UPDATE [WorkflowActionType] SET 
            [Order] = @ActionOrder + 1,
            [ActivityTypeId] = @SubmitActivityTypeId
        WHERE [Guid] = 'CCE5B3EF-93E3-46C6-BE37-21C1F1D23A4A'

        --Get current order of 'submit' activity
        DECLARE @ActivityOrder int = ( 
            SELECT [Order] 
            FROM [WorkflowActivityType] 
            WHERE [Id] = @SubmitActivityTypeId
        )

        UPDATE [WorkflowActivityType] SET [Order] = [Order] + 1
        WHERE [WorkflowTypeId] = @WorkflowTypeId
        AND [Order] > @ActivityOrder

        UPDATE [WorkflowActivityType] SET [Order] = @ActivityOrder + 1
        WHERE [Guid] = 'E51407E3-65A1-4A6C-A51D-8DE6816BEBDA'

    END
" );

            // TC: Add Giving Transactions to Business Detail Page

            // First delete any existing blocks of these types that might have been manually added to the page.
            Sql( @"
    DELETE B
    FROM [Block] B
    INNER JOIN [BlockType] T ON T.[Id] = B.[BlockTypeId]
    INNER JOIN [Page] P ON P.[Id] = B.[PageId]
    WHERE P.[Guid] = 'D2B43273-C64F-4F57-9AAE-9571E1982BAC'
    AND T.[Guid] IN ( '535307C8-77D1-44F8-AD4D-1577572B6D26', 'E04320BC-67C3-452D-9EF6-D74D8C177154' )
" );

            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "535307C8-77D1-44F8-AD4D-1577572B6D26", "Transaction Yearly Summary Lava", "Main", @"<div class='row'>
<div class='col-md-6'>", @"</div>    
</div>", 1, "5322C1C2-0387-4752-9E87-67700F485C5E" );
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "E04320BC-67C3-452D-9EF6-D74D8C177154", "Transaction List", "Main", "", "", 1, "0A567E24-80BE-4906-B303-77D1A5FB89DE" );

            RockMigrationHelper.AddBlockAttributeValue( "5322C1C2-0387-4752-9E87-67700F485C5E", "22C2445C-CE91-4791-A00C-68BB604C55CB", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "5322C1C2-0387-4752-9E87-67700F485C5E", "D33B39C7-F81C-4D0C-8A63-E0489FEA4DF4", @"{% include '~~/Assets/Lava/TransactionYearlySummary.lava' %}" );
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "C6D07A89-84C9-412A-A584-E37E59506566", @"cc7be14e-3680-4e78-aacc-a57a8d42350f" );
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "29A6C37A-EFB3-41CC-A522-9CEFAAEEA910", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "A4E3B5C6-B386-45B5-A929-8FD9379BABBC", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "81AD58EA-F94B-42A1-AC57-16902B717092", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "293F8A3E-020A-4260-8817-3E368CF31ABB", @"2d607262-52d6-4724-910d-5c6e8fb89acc" );
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "00EBFDFE-C6AE-48F2-B284-809D1765D489", @"200" );
            RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "0227D124-D207-4F68-8B77-4A4A88CBBE6F", @"False" );

            RockMigrationHelper.UpdatePageContext( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "Rock.Model.Person", "businessId", "EF3C8D80-A500-495D-BCC1-4C9282AF889C" );

            RockMigrationHelper.AddPage( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Transaction Detail", "", "CC7BE14E-3680-4E78-AACC-A57A8D42350F", "" );
            RockMigrationHelper.AddBlock( "CC7BE14E-3680-4E78-AACC-A57A8D42350F", "", "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "Transaction Detail", "Main", "", "", 0, "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "5322C1C2-0387-4752-9E87-67700F485C5E" );
            RockMigrationHelper.DeleteBlock( "0A567E24-80BE-4906-B303-77D1A5FB89DE" );
            RockMigrationHelper.DeletePageContext( "EF3C8D80-A500-495D-BCC1-4C9282AF889C" );

            RockMigrationHelper.DeleteBlock( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9" );
            RockMigrationHelper.DeletePage( "CC7BE14E-3680-4E78-AACC-A57A8D42350F" );
        }
    }
}
