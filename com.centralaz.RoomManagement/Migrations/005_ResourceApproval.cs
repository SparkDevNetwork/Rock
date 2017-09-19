// <copyright>
// Copyright by the Central Christian Church
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
using Rock.Plugin;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 5, "1.4.5" )]
    public class ResourceApproval : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.DeleteSecurityAuth( "0AD21D60-C750-4FD7-9D89-106692854BA4" );
            RockMigrationHelper.DeleteSecurityAuth( "CCBC6C0C-EEDB-4B55-9D07-528253624FD0" );

            RockMigrationHelper.DeleteEntityType( "5241B2B1-AEF2-4EB9-9737-55604069D93B" );
            RockMigrationHelper.DeleteFieldType( "335E190C-88FE-4BE2-BE36-3F8B85AF39F2" );
            RockMigrationHelper.DeleteFieldType( "D3D17BE3-33BF-4CDF-89E1-F70C57317B4E" );

            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ReservationStatus]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationStatus_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_centralaz_RoomManagement_ReservationStatus]
                " );


            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_Resource] ADD ApprovalGroupId [int] NULL;

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup] FOREIGN KEY([ApprovalGroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] CHECK CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup]
                " );

            Sql( @"
            ALTER TABLE [_com_centralaz_RoomManagement_ReservationResource] DROP COLUMN IsApproved;
                ALTER TABLE [_com_centralaz_RoomManagement_ReservationResource] ADD ApprovalState [int] NOT NULL DEFAULT 1;
                " );

            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_ReservationLocation] DROP COLUMN IsApproved;
                ALTER TABLE [_com_centralaz_RoomManagement_ReservationLocation] ADD ApprovalState [int] NOT NULL DEFAULT 1;
                " );

            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_Reservation] ADD ApprovalState [int] NOT NULL DEFAULT 1;
                " );

            Sql( @"
                ALTER TABLE [_com_centralaz_RoomManagement_Reservation] DROP COLUMN IsApproved;
                " );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.Location", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "", "", "Approval Group", "If this resource requires special approval, select the group in charge of approving it here.", 100, null, "96C07909-E34A-4379-854F-C05E79F772E4" );

            RockMigrationHelper.AddPageRoute( "4CBD2B96-E076-46DF-A576-356BCA5E577F", "ReservationDetail" );


            #region FieldTypes

            RockMigrationHelper.UpdateFieldType( "Reservation Approval State", "", "com.centralaz.RoomManagement", "com.centralaz.RoomManagement.Field.Types.ReservationApprovalStateFieldType", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B" );

            #endregion

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "com.centralaz.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState", "3894452A-E763-41AC-8260-10373646D8A0", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "ACA008E2-2406-457E-8E4C-6922E03757A4" ); // com.centralaz.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Approval State Attribute", "ApprovalStateAttribute", "The attribute that contains the reservation approval state.", 1, @"", "2E185FB5-FC8E-41BE-B7FE-702F74B47539" ); // com.centralaz.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Reservation Attribute", "ReservationAttribute", "The attribute that contains the reservation.", 0, @"", "1D4F819F-145D-4A7F-AB4E-AD7C06759042" ); // com.centralaz.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Reservation Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25954FDC-F486-417D-ABBB-E2DF2C67B186" ); // com.centralaz.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "3894452A-E763-41AC-8260-10373646D8A0", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B", "Approval State", "ApprovalState", "The connection state to use (if Connection State Attribute is not specified).", 2, @"", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2" ); // com.centralaz.RoomManagement.Workflow.Actions.Reservations.SetReservationApprovalState:Approval State

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Room Management", "fa fa-building-o", "", "B8E4B3B0-B543-48B6-93BE-604D4F368559", 0 ); // Room Management

            #endregion

            #region Room Reservation Approval Notification

            RockMigrationHelper.UpdateWorkflowType( false, true, "Room Reservation Approval Notification", "A workflow that sends an email to the party responsible for the next step in the room reservation approval process.", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Approval Request", "fa fa-list-ol", 28800, true, 0, "543D4FCD-310B-4048-BFCB-BAE582CBB890", 0 ); // Room Reservation Approval Notification
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "F4ACC5B8-98BB-4611-B6B7-065BBC47503B", "Approval State", "ApprovalState", "", 0, @"", "9E1FD102-AA41-4F7A-A52D-601DB1852A44", false ); // Room Reservation Approval Notification:Approval State
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Final Approval Group", "FinalApprovalGroup", "An optional group that gives final approval for reservations.", 1, @"628c51a8-4613-43ed-a18d-4a6fb999273e", "653CE164-554A-4B22-A830-3E760DA2023E", false ); // Room Reservation Approval Notification:Final Approval Group
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7", "Reservation", "Reservation", "", 2, @"", "AEACA577-F09F-4818-8C09-CA5490B0DD91", false ); // Room Reservation Approval Notification:Reservation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requester", "Requester", "", 3, @"", "4E365CE8-242A-4F05-B953-EA9DEFF470EE", false ); // Room Reservation Approval Notification:Requester
            RockMigrationHelper.AddAttributeQualifier( "4E365CE8-242A-4F05-B953-EA9DEFF470EE", "EnableSelfSelection", @"False", "1D9E7B5A-2998-4D69-87A2-983B4447A7DC" ); // Room Reservation Approval Notification:Requester:EnableSelfSelection
            RockMigrationHelper.UpdateWorkflowActivityType( "543D4FCD-310B-4048-BFCB-BAE582CBB890", true, "Set Attributes", "", true, 0, "6A396018-6CC1-4C41-8EF1-FB9779C0B04D" ); // Room Reservation Approval Notification:Set Attributes
            RockMigrationHelper.UpdateWorkflowActivityType( "543D4FCD-310B-4048-BFCB-BAE582CBB890", true, "Notify Requester that the Reservation Requires Changes", "", false, 1, "2C1387D7-3E8F-4702-A9B9-4C2E52684EEE" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes
            RockMigrationHelper.UpdateWorkflowActivityType( "543D4FCD-310B-4048-BFCB-BAE582CBB890", true, "Notify Requester that the Reservation has been Approved", "", false, 2, "21F615D2-3A9C-421B-8EBF-013C43DE9E4F" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved
            RockMigrationHelper.UpdateWorkflowActivityType( "543D4FCD-310B-4048-BFCB-BAE582CBB890", true, "Notify Requester that the Reservation has been Denied", "", false, 3, "3077D8F8-478E-4F12-A90F-B1642B9C0B6E" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied
            RockMigrationHelper.UpdateWorkflowActivityType( "543D4FCD-310B-4048-BFCB-BAE582CBB890", true, "Notify Approval group that the Reservation is Pending Review", "", false, 4, "CC968504-37E2-430C-B131-57B9556E0442" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Set Reservation From Entity", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "3628C256-C190-449F-B41A-0928DAE7615F" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Set Requester From Entity", 1, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "B0EE1D6E-07F8-4C1F-9320-B0855EF68703" ); // Room Reservation Approval Notification:Set Attributes:Set Requester From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Set Approval State From Entity", 2, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Activate Requires Changes Activity", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "9E1FD102-AA41-4F7A-A52D-601DB1852A44", 1, "ChangesNeeded", "EBCCC06C-94BD-4D8C-A294-1AB8CE08FE49" ); // Room Reservation Approval Notification:Set Attributes:Activate Requires Changes Activity
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Activate Approved Activity", 4, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "9E1FD102-AA41-4F7A-A52D-601DB1852A44", 1, "Approved", "BB07D45C-2277-42DC-A9C7-9446DFB27054" ); // Room Reservation Approval Notification:Set Attributes:Activate Approved Activity
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Activate Denied Activity", 5, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "9E1FD102-AA41-4F7A-A52D-601DB1852A44", 1, "Denied", "D9BE41D2-EC83-46C6-998D-F63450C544D2" ); // Room Reservation Approval Notification:Set Attributes:Activate Denied Activity
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Activate Pending Review Activity", 6, "38907A90-1634-4A93-8017-619326A4A582", true, false, "", "9E1FD102-AA41-4F7A-A52D-601DB1852A44", 1, "PendingReview", "34902048-BBAA-4CE3-8D83-F8CC76BD270A" ); // Room Reservation Approval Notification:Set Attributes:Activate Pending Review Activity
            RockMigrationHelper.UpdateWorkflowActionType( "2C1387D7-3E8F-4702-A9B9-4C2E52684EEE", "Send Email", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "2C1387D7-3E8F-4702-A9B9-4C2E52684EEE", "Complete Workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "D3DABD8E-9C98-4DED-836B-0359B72E4E9F" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "21F615D2-3A9C-421B-8EBF-013C43DE9E4F", "Send Email", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "52520D50-9D35-4E0D-AF04-1A222B50EA91" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "21F615D2-3A9C-421B-8EBF-013C43DE9E4F", "Complete Workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "85151F37-6C30-4A0D-BCC6-CF06E2887B0D" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "3077D8F8-478E-4F12-A90F-B1642B9C0B6E", "Send Email", 0, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "78761E92-CD94-438A-A3F2-FB683C8D8054" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "3077D8F8-478E-4F12-A90F-B1642B9C0B6E", "Complete Workflow", 1, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "CF4AAD9B-7F49-4A95-B90C-E9E44D909E2A" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "CC968504-37E2-430C-B131-57B9556E0442", "Set Reservation to Approved", 0, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "653CE164-554A-4B22-A830-3E760DA2023E", 32, "", "B1F80A78-279D-4691-B030-E035FDCAA3C8" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved
            RockMigrationHelper.UpdateWorkflowActionType( "CC968504-37E2-430C-B131-57B9556E0442", "Activate Approved Activity", 1, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "653CE164-554A-4B22-A830-3E760DA2023E", 32, "", "E0991B14-DE3A-4103-8B73-77CBFDB4826E" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Activate Approved Activity
            RockMigrationHelper.UpdateWorkflowActionType( "CC968504-37E2-430C-B131-57B9556E0442", "Send Email", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "FE7B413C-0DF4-4F9C-935C-8B39DA87742D" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "CC968504-37E2-430C-B131-57B9556E0442", "Complete Activity", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "", 1, "", "A9D101D2-A87C-4A12-8BCA-118F15B682CC" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Complete Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"aeaca577-f09f-4818-8c09-ca5490b0dd91" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "83E27C1E-1491-4AE2-93F1-909791D4B70A", @"True" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "3628C256-C190-449F-B41A-0928DAE7615F", "EFC517E9-8A53-4681-B16B-9D4DE89244BE", @"{{ Entity.Guid }}" ); // Room Reservation Approval Notification:Set Attributes:Set Reservation From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "B0EE1D6E-07F8-4C1F-9320-B0855EF68703", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Requester From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B0EE1D6E-07F8-4C1F-9320-B0855EF68703", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Room Reservation Approval Notification:Set Attributes:Set Requester From Entity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "B0EE1D6E-07F8-4C1F-9320-B0855EF68703", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"4e365ce8-242a-4f05-b953-ea9deff470ee" ); // Room Reservation Approval Notification:Set Attributes:Set Requester From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B0EE1D6E-07F8-4C1F-9320-B0855EF68703", "83E27C1E-1491-4AE2-93F1-909791D4B70A", @"True" ); // Room Reservation Approval Notification:Set Attributes:Set Requester From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "B0EE1D6E-07F8-4C1F-9320-B0855EF68703", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Requester From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "B0EE1D6E-07F8-4C1F-9320-B0855EF68703", "EFC517E9-8A53-4681-B16B-9D4DE89244BE", @"{{ Entity.RequesterAlias.Guid }}" ); // Room Reservation Approval Notification:Set Attributes:Set Requester From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"9e1fd102-aa41-4f7a-a52d-601db1852a44" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "83E27C1E-1491-4AE2-93F1-909791D4B70A", @"True" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "EFC517E9-8A53-4681-B16B-9D4DE89244BE", @"{{ Entity.ApprovalState }}" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "EBCCC06C-94BD-4D8C-A294-1AB8CE08FE49", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Room Reservation Approval Notification:Set Attributes:Activate Requires Changes Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EBCCC06C-94BD-4D8C-A294-1AB8CE08FE49", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Room Reservation Approval Notification:Set Attributes:Activate Requires Changes Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EBCCC06C-94BD-4D8C-A294-1AB8CE08FE49", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"2C1387D7-3E8F-4702-A9B9-4C2E52684EEE" ); // Room Reservation Approval Notification:Set Attributes:Activate Requires Changes Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "BB07D45C-2277-42DC-A9C7-9446DFB27054", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Room Reservation Approval Notification:Set Attributes:Activate Approved Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BB07D45C-2277-42DC-A9C7-9446DFB27054", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Room Reservation Approval Notification:Set Attributes:Activate Approved Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "BB07D45C-2277-42DC-A9C7-9446DFB27054", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"21F615D2-3A9C-421B-8EBF-013C43DE9E4F" ); // Room Reservation Approval Notification:Set Attributes:Activate Approved Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "D9BE41D2-EC83-46C6-998D-F63450C544D2", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Room Reservation Approval Notification:Set Attributes:Activate Denied Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D9BE41D2-EC83-46C6-998D-F63450C544D2", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Room Reservation Approval Notification:Set Attributes:Activate Denied Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D9BE41D2-EC83-46C6-998D-F63450C544D2", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"3077D8F8-478E-4F12-A90F-B1642B9C0B6E" ); // Room Reservation Approval Notification:Set Attributes:Activate Denied Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "34902048-BBAA-4CE3-8D83-F8CC76BD270A", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Room Reservation Approval Notification:Set Attributes:Activate Pending Review Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "34902048-BBAA-4CE3-8D83-F8CC76BD270A", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Room Reservation Approval Notification:Set Attributes:Activate Pending Review Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "34902048-BBAA-4CE3-8D83-F8CC76BD270A", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"CC968504-37E2-430C-B131-57B9556E0442" ); // Room Reservation Approval Notification:Set Attributes:Activate Pending Review Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "0C4C13B8-7076-4872-925A-F950886B5E16", @"4e365ce8-242a-4f05-b953-ea9deff470ee" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Your Reservation Requires Changes" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation requires changes:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}});
return reservation.Schedule.FriendlyScheduleText;
{% endexecute %}<br/>
Event Duration: {% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}});
return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min"";
{% endexecute %}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "2040B757-3012-4EAB-AB98-8222EC97AB7F", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "D3DABD8E-9C98-4DED-836B-0359B72E4E9F", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D3DABD8E-9C98-4DED-836B-0359B72E4E9F", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "D3DABD8E-9C98-4DED-836B-0359B72E4E9F", "908F249E-22D0-461C-95F2-AFB9EBE771C2", @"Completed" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "52520D50-9D35-4E0D-AF04-1A222B50EA91", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "52520D50-9D35-4E0D-AF04-1A222B50EA91", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "52520D50-9D35-4E0D-AF04-1A222B50EA91", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "52520D50-9D35-4E0D-AF04-1A222B50EA91", "0C4C13B8-7076-4872-925A-F950886B5E16", @"4e365ce8-242a-4f05-b953-ea9deff470ee" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "52520D50-9D35-4E0D-AF04-1A222B50EA91", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Your Reservation has been Approved" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "52520D50-9D35-4E0D-AF04-1A222B50EA91", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation has been approved:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}});
return reservation.Schedule.FriendlyScheduleText;
{% endexecute %}<br/>
Event Duration: {% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}});
return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min"";
{% endexecute %}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "52520D50-9D35-4E0D-AF04-1A222B50EA91", "2040B757-3012-4EAB-AB98-8222EC97AB7F", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "85151F37-6C30-4A0D-BCC6-CF06E2887B0D", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "85151F37-6C30-4A0D-BCC6-CF06E2887B0D", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "85151F37-6C30-4A0D-BCC6-CF06E2887B0D", "908F249E-22D0-461C-95F2-AFB9EBE771C2", @"Completed" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "78761E92-CD94-438A-A3F2-FB683C8D8054", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "78761E92-CD94-438A-A3F2-FB683C8D8054", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "78761E92-CD94-438A-A3F2-FB683C8D8054", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "78761E92-CD94-438A-A3F2-FB683C8D8054", "0C4C13B8-7076-4872-925A-F950886B5E16", @"4e365ce8-242a-4f05-b953-ea9deff470ee" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "78761E92-CD94-438A-A3F2-FB683C8D8054", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Your Reservation has been Denied" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "78761E92-CD94-438A-A3F2-FB683C8D8054", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
Your reservation has been denied:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}});
return reservation.Schedule.FriendlyScheduleText;
{% endexecute %}<br/>
Event Duration: {% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}});
return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min"";
{% endexecute %}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "78761E92-CD94-438A-A3F2-FB683C8D8054", "2040B757-3012-4EAB-AB98-8222EC97AB7F", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "CF4AAD9B-7F49-4A95-B90C-E9E44D909E2A", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CF4AAD9B-7F49-4A95-B90C-E9E44D909E2A", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "CF4AAD9B-7F49-4A95-B90C-E9E44D909E2A", "908F249E-22D0-461C-95F2-AFB9EBE771C2", @"Completed" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "25954FDC-F486-417D-ABBB-E2DF2C67B186", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"aeaca577-f09f-4818-8c09-ca5490b0dd91" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "2E185FB5-FC8E-41BE-B7FE-702F74B47539", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Approval State Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"2" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "E0991B14-DE3A-4103-8B73-77CBFDB4826E", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Activate Approved Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E0991B14-DE3A-4103-8B73-77CBFDB4826E", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Activate Approved Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E0991B14-DE3A-4103-8B73-77CBFDB4826E", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"21F615D2-3A9C-421B-8EBF-013C43DE9E4F" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Activate Approved Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "0C4C13B8-7076-4872-925A-F950886B5E16", @"653ce164-554a-4b22-a830-3e760da2023e" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Requires Approval" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% assign reservation = Workflow | Attribute:'Reservation','Object' %} 
<p>
A new reservation requires your approval:<br/><br/>
Name: {{ reservation.Name }}<br/>
Requestor: {{ reservation.RequesterAlias.Person.FullName }}<br/>
Campus: {{ reservation.Campus.Name }}<br/>
Ministry: {{ reservation.ReservationMinistry.Name }}<br/>
Number Attending: {{ reservation.NumberAttending }}<br/>
<br/>
Schedule: {% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}});
return reservation.Schedule.FriendlyScheduleText;
{% endexecute %}<br/>
Event Duration: {% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}});
return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min"";
{% endexecute %}<br/>
Setup Time: {{ reservation.SetupTime }} min<br/>
Cleanup Time: {{ reservation.CleanupTime }} min<br/>
{% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %}
{% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %}
<br/>
Notes: {{ reservation.Note }}<br/>
<br/>
<a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a>
</p>
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "2040B757-3012-4EAB-AB98-8222EC97AB7F", @"True" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "A9D101D2-A87C-4A12-8BCA-118F15B682CC", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Complete Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A9D101D2-A87C-4A12-8BCA-118F15B682CC", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Complete Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A9D101D2-A87C-4A12-8BCA-118F15B682CC", "908F249E-22D0-461C-95F2-AFB9EBE771C2", @"Completed" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Complete Activity:Status|Status Attribute

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

            Sql( @"
                DECLARE @WorkflowId int = (Select Top 1 Id From WorkflowType Where Guid = '543D4FCD-310B-4048-BFCB-BAE582CBB890')

INSERT [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] ([WorkflowTypeId], [TriggerType], [QualifierValue], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES (@WorkflowId, 0, N'|||', CAST(N'2017-03-20 14:02:11.953' AS DateTime), CAST(N'2017-03-20 14:02:11.953' AS DateTime), 10, 10, N'5339e1c4-ac09-4bd5-9416-628dba200ba5', NULL, NULL, NULL)
INSERT [dbo].[_com_centralaz_RoomManagement_ReservationWorkflowTrigger] ([WorkflowTypeId], [TriggerType], [QualifierValue], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [Guid], [ForeignKey], [ForeignGuid], [ForeignId]) VALUES (@WorkflowId, 2, N'|||', CAST(N'2017-03-20 14:02:11.953' AS DateTime), CAST(N'2017-03-20 14:02:11.953' AS DateTime), 10, 10, N'68f6de62-cdbb-4ec0-8440-8b1740c21e65', NULL, NULL, NULL)

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_WorkflowId]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationWorkflow]  WITH CHECK ADD  CONSTRAINT [FK__com_centralaz_RoomManagement_ReservationWorkflow_WorkflowId] FOREIGN KEY([WorkflowId])
                REFERENCES [dbo].[Workflow] ([Id])
                ON DELETE CASCADE" );
        }
        public override void Down()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Resource] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Resource_ApprovalGroup]
                ALTER TABLE [_com_centralaz_RoomManagement_Resource] DROP COLUMN ApprovalGroupId
            " );
        }
    }
}
