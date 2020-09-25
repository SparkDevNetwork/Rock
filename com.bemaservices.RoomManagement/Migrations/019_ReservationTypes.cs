// <copyright>
// Copyright by BEMA Software Services
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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 19, "1.9.4" )]
    public class ReservationType : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [IsSystem] [bit] NOT NULL,
                    [Name] [nvarchar](50) NULL,
	                [Description] [nvarchar](max) NULL,
	                [IsActive] [bit] NOT NULL,
                    [IconCssClass] [nvarchar](50) NULL,
                    [FinalApprovalGroupId] [int] NULL,
                    [SuperAdminGroupId] [int] NULL,
                    [NotificationEmailId] [int] NULL,
                    [DefaultSetupTime] [int] NULL,
                    [IsCommunicationHistorySaved] [bit] NOT NULL,
                    [IsNumberAttendingRequired] [bit] NOT NULL,
                    [IsContactDetailsRequired] [bit] NOT NULL,
                    [IsSetupTimeRequired] [bit] NOT NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationType] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_FinalApprovalGroupId] FOREIGN KEY([FinalApprovalGroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_FinalApprovalGroupId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_SuperAdminGroupId] FOREIGN KEY([SuperAdminGroupId])
                REFERENCES [dbo].[Group] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_SuperAdminGroupId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_NotificationEmailId] FOREIGN KEY([NotificationEmailId])
                REFERENCES [dbo].[SystemEmail] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_NotificationEmailId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_ModifiedByPersonAliasId]
" );

            // Make the ReservationDetails block's attributes KNOWN Guids:
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Contact Details", "RequireContactDetails", "", "Should the Event and Administrative Contact be required to be supplied?", 3, @"True", "1C8DE8CB-E078-4483-9648-7C2CC57E6985" );
            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.Model.ReservationType'
                    And Guid != 'AC498297-D28C-47C0-B53B-4BF54D895DEB'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationType", "Reservation Type", "com.bemaservices.RoomManagement.Model.ReservationType, com.bemaservices.RoomManagement, Version=1.2.2.0, Culture=neutral, PublicKeyToken=null", true, true, "AC498297-D28C-47C0-B53B-4BF54D895DEB" );

            string sqlQry = GenerateDefaultReservationTypeSql();
            Sql( sqlQry );


            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ADD [ReservationTypeId] INT NULL
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ADD [ReservationTypeId] INT NULL
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [ReservationTypeId] INT NULL

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_ReservationTypeId] FOREIGN KEY([ReservationTypeId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationType] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_ReservationTypeId] FOREIGN KEY([ReservationTypeId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationType] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ReservationTypeId] FOREIGN KEY([ReservationTypeId])
                REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationType] ([Id])
" );
            Sql( @"
            Update [_com_bemaservices_RoomManagement_ReservationWorkflowTrigger]
            Set ReservationTypeId = 1

            Update [_com_bemaservices_RoomManagement_ReservationMinistry]
            Set ReservationTypeId = 1

            Update [_com_bemaservices_RoomManagement_Reservation]
            Set ReservationTypeId = 1
                " );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] ALTER COLUMN [ReservationTypeId] INT NOT NULL
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] ALTER COLUMN [ReservationTypeId] INT NOT NULL
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ALTER COLUMN [ReservationTypeId] INT NOT NULL
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflow] ALTER COLUMN [ReservationWorkflowTriggerId] [int] NULL
                " );
            RockMigrationHelper.DeleteBlock( "2B864E89-27DE-41F9-A24B-8D2EA5C40D10" );
            RockMigrationHelper.DeleteBlockType( "6931E212-A76A-4DBB-9B97-86E5CDD0793A" );

            RockMigrationHelper.DeletePage( "DC6D7ACE-E23F-4CE6-9D66-A63348A1EF4E" ); //  Page: Reservation Type Detail
            RockMigrationHelper.DeletePage( "CFF84B6D-C852-4FC4-B602-9F045EDC8854" ); //  Page: Reservation Configuration

            // Page: Reservation Types
            RockMigrationHelper.AddPage( true, "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reservation Types", "", "CFF84B6D-C852-4FC4-B602-9F045EDC8854", "fa fa-gear" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockTypeByGuid( "Reservation Type List", "Block to display the reservation types.", "~/Plugins/com_bemaservices/RoomManagement/ReservationTypeList.ascx", "com_bemaservices > Room Management", "F28B44CF-D49D-4A45-8189-381A1F942C86" );
            // Add Block to Page: Reservation Types, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "CFF84B6D-C852-4FC4-B602-9F045EDC8854", "", "F28B44CF-D49D-4A45-8189-381A1F942C86", "Reservation Type List", "Main", "", "", 0, "87FCDDF1-D938-4BC9-AEA3-32F2B3F86494" );
            // Attrib for BlockType: Reservation Type List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "F28B44CF-D49D-4A45-8189-381A1F942C86", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view details of a reservation type.", 0, @"", "A56ED80C-A8EC-44DD-84BA-03F12F281B9C" );
            // Attrib Value for Block:Reservation Type List, Attribute:Detail Page Page: Reservation Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "87FCDDF1-D938-4BC9-AEA3-32F2B3F86494", "A56ED80C-A8EC-44DD-84BA-03F12F281B9C", @"dc6d7ace-e23f-4ce6-9d66-a63348a1ef4e" );


            // Page: Reservation Type Detail
            RockMigrationHelper.AddPage( true, "CFF84B6D-C852-4FC4-B602-9F045EDC8854", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reservation Type Detail", "", "DC6D7ACE-E23F-4CE6-9D66-A63348A1EF4E", "" ); // Site:Rock RMS
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'DC6D7ACE-E23F-4CE6-9D66-A63348A1EF4E'" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Reservation Type Detail", "Displays the details of the given Reservation Type for editing.", "~/Plugins/com_bemaservices/RoomManagement/ReservationTypeDetail.ascx", "com_bemaservices > Room Management", "CBAAEC6D-9B97-4FCB-96A9-5C53FB4E030E" );
            // Add Block to Page: Reservation Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DC6D7ACE-E23F-4CE6-9D66-A63348A1EF4E", "", "CBAAEC6D-9B97-4FCB-96A9-5C53FB4E030E", "Reservation Type Detail", "Main", "", "", 0, "160ED605-4BC3-46FD-8C24-A1BB9AD4ECB4" );

            // Update Approval Workflow
            var activityTypeIdSql = SqlScalar( @"
                Select Id
                From WorkflowActivityType
                Where [Guid] = '6A396018-6CC1-4C41-8EF1-FB9779C0B04D'
                " );

            var activityTypeId = activityTypeIdSql.ToString().AsIntegerOrNull();
            if ( activityTypeId != null )
            {
                Sql( String.Format( @"
                    UPDATE [WorkflowActionType]
                    SET [Order] = [Order] + 1
                    Where [ActivityTypeId] = {0}
                    ", activityTypeId.Value ) );

                RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Set Final Approval Group from Entity", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "44D1AF13-D6D6-4ACF-8325-4EC63499A8FD" ); // Room Reservation Approval Notification:Set Attributes:Set Final Approval Group from Entity
                RockMigrationHelper.AddActionTypeAttributeValue( "44D1AF13-D6D6-4ACF-8325-4EC63499A8FD", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Final Approval Group from Entity:Active
                RockMigrationHelper.AddActionTypeAttributeValue( "44D1AF13-D6D6-4ACF-8325-4EC63499A8FD", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Room Reservation Approval Notification:Set Attributes:Set Final Approval Group from Entity:Order
                RockMigrationHelper.AddActionTypeAttributeValue( "44D1AF13-D6D6-4ACF-8325-4EC63499A8FD", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"653ce164-554a-4b22-a830-3e760da2023e" ); // Room Reservation Approval Notification:Set Attributes:Set Final Approval Group from Entity:Attribute
                RockMigrationHelper.AddActionTypeAttributeValue( "44D1AF13-D6D6-4ACF-8325-4EC63499A8FD", "83E27C1E-1491-4AE2-93F1-909791D4B70A", @"True" ); // Room Reservation Approval Notification:Set Attributes:Set Final Approval Group from Entity:Entity Is Required
                RockMigrationHelper.AddActionTypeAttributeValue( "44D1AF13-D6D6-4ACF-8325-4EC63499A8FD", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Final Approval Group from Entity:Use Id instead of Guid
                AddActionTypeAttributeValue( "44D1AF13-D6D6-4ACF-8325-4EC63499A8FD", "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", @"{{ Entity.ReservationType.FinalApprovalGroup.Guid }}" );
            }

            // Report Templates
            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.ReportTemplates.DefaultReportTemplate'
                    And Guid != '9b74314a-37e0-40f2-906c-2862c93f8888'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.ReportTemplates.DefaultReportTemplate", "Default Template", "com.bemaservices.RoomManagement.ReportTemplates.DefaultReportTemplate, com.bemaservices.RoomManagement, Version=1.2.1.0, Culture=neutral, PublicKeyToken=null", false, true, "9b74314a-37e0-40f2-906c-2862c93f8888" );
            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.ReportTemplates.LavaReportTemplate'
                    And Guid != '7ef82cca-7874-4b8d-adb7-896f05095354'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.ReportTemplates.LavaReportTemplate", "Lava Template", "com.bemaservices.RoomManagement.ReportTemplates.LavaReportTemplate, com.bemaservices.RoomManagement, Version=1.2.1.0, Culture=neutral, PublicKeyToken=null", false, true, "7ef82cca-7874-4b8d-adb7-896f05095354" );
            Sql( @"
                    Delete
                    From EntityType
                    Where Name = 'com.bemaservices.RoomManagement.ReportTemplates.AdvancedReportTemplate'
                    And Guid != '97a7ffda-1b75-473f-a680-c9a7602b5c60'
                " );
            UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.ReportTemplates.AdvancedReportTemplate", "Advanced Template", "com.bemaservices.RoomManagement.ReportTemplates.AdvancedReportTemplate, com.bemaservices.RoomManagement, Version=1.2.1.0, Culture=neutral, PublicKeyToken=null", false, true, "97a7ffda-1b75-473f-a680-c9a7602b5c60" );

            RockMigrationHelper.AddEntityAttribute( "com.bemaservices.RoomManagement.ReportTemplates.DefaultReportTemplate", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "True", "D4B18C56-9D42-4B4D-A0AA-CD8A9D5D3C77" );
            RockMigrationHelper.AddAttributeValue( "D4B18C56-9D42-4B4D-A0AA-CD8A9D5D3C77", 0, "True", "9C14AD2E-596C-4768-BF60-F652F2A008B0" );

            RockMigrationHelper.AddEntityAttribute( "com.bemaservices.RoomManagement.ReportTemplates.LavaReportTemplate", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "True", "E6827740-78CA-46C0-A3DD-9C2F4E547D26" );
            RockMigrationHelper.AddAttributeValue( "E6827740-78CA-46C0-A3DD-9C2F4E547D26", 0, "True", "C255EDF9-6284-4F5C-B680-1C16C123A481" );

            RockMigrationHelper.AddEntityAttribute( "com.bemaservices.RoomManagement.ReportTemplates.AdvancedReportTemplate", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "True", "BE8E7F70-4CC3-432A-B13E-226057478843" );
            RockMigrationHelper.AddAttributeValue( "BE8E7F70-4CC3-432A-B13E-226057478843", 0, "True", "317A6D16-42F1-42C4-8C19-2168C8B0BFB5" );

            // Page: Room Management
            // Attrib for BlockType: Reservation Lava:Report Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "6FD93801-9E5B-48E7-A6C3-B0354A96E5E7", "Report Template", "ReportTemplate", "", "The template for the printed report. The Default and Advanced Templates will generate a printed report based on the templates' hardcoded layout. The Lava Template will generate a report based on the lava provided below in the Report Lava Setting. Any other custom templates will format based on their developer's documentation.", 16, @"9b74314a-37e0-40f2-906c-2862c93f8888", "B9BEAFB9-A958-45E6-86E4-7BA6904CC7B1" );
            // Attrib for BlockType: Reservation Lava:Report Lava
            RockMigrationHelper.UpdateBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Report Lava", "ReportLava", "", "If the Lava Template is selected, this is the lava that will be used in the report", 17, @"{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/ReservationReport.lava' %}", "69131013-4E48-468E-B2C2-CF19CEA26590" );


            // Change History
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Reservation Changes", "fa fa-home", "Anything related to a reservation.", com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES );
            RockMigrationHelper.AddBlock( true, "4CBD2B96-E076-46DF-A576-356BCA5E577F", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 1, "A981B5ED-F5B4-41AE-96A3-2BC10CCF110B" );
            // Attrib for BlockType: History Log:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "8FB690EC-5299-46C5-8695-AAD23168E6E1" );
            // Attrib for BlockType: History Log:Heading
            RockMigrationHelper.UpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "", "The Lava template to use for the heading. <span class='tip tip-lava'></span>", 0, @"{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047" );
            // Attrib Value for Block:History Log, Attribute:Entity Type Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A981B5ED-F5B4-41AE-96A3-2BC10CCF110B", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"839768a3-10d6-446c-a65b-b8f9efd7808f" );
            // Attrib Value for Block:History Log, Attribute:Heading Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A981B5ED-F5B4-41AE-96A3-2BC10CCF110B", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"{{ Entity.Name}} (ID:{{ Entity.Id }})" );
            // Add/Update PageContext for Page:New Reservation, Entity: com.bemaservices.RoomManagement.Model.Reservation, Parameter: ReservationId
            RockMigrationHelper.DeletePageContext( "2C95976A-ED4F-4229-BEBA-311382A6C953" );
            RockMigrationHelper.UpdatePageContext( "4CBD2B96-E076-46DF-A576-356BCA5E577F", "com.bemaservices.RoomManagement.Model.Reservation", "ReservationId", "2C95976A-ED4F-4229-BEBA-311382A6C953" );

            // Security
            AddSecurityAuthForReservationType( "E443F926-0882-41D5-91EF-480EA366F660", 0, Rock.Security.Authorization.ADMINISTRATE, true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, SpecialRole.None, "F2F1A2E9-8289-4FF3-B0F6-944ACE4B436E" );
            AddSecurityAuthForReservationType( "E443F926-0882-41D5-91EF-480EA366F660", 1, Rock.Security.Authorization.ADMINISTRATE, true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, SpecialRole.None, "BAF614AC-D741-43EC-8D80-BAC36E89E848" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "614CD413-DCB7-4DA2-80A0-C7ABE5A11047" );
            RockMigrationHelper.DeleteAttribute( "8FB690EC-5299-46C5-8695-AAD23168E6E1" );
            RockMigrationHelper.DeleteBlock( "A981B5ED-F5B4-41AE-96A3-2BC10CCF110B" );

            RockMigrationHelper.DeleteBlock( "160ED605-4BC3-46FD-8C24-A1BB9AD4ECB4" );
            RockMigrationHelper.DeleteBlockType( "CBAAEC6D-9B97-4FCB-96A9-5C53FB4E030E" );
            RockMigrationHelper.DeletePage( "DC6D7ACE-E23F-4CE6-9D66-A63348A1EF4E" ); //  Page: Reservation Type Detail

            RockMigrationHelper.DeleteAttribute( "A56ED80C-A8EC-44DD-84BA-03F12F281B9C" );
            RockMigrationHelper.DeleteBlock( "87FCDDF1-D938-4BC9-AEA3-32F2B3F86494" );
            RockMigrationHelper.DeleteBlockType( "F28B44CF-D49D-4A45-8189-381A1F942C86" );
            RockMigrationHelper.DeletePage( "CFF84B6D-C852-4FC4-B602-9F045EDC8854" ); //  Page: Reservation Types

            // Page: Reservation Configuration
            RockMigrationHelper.AddPage( "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reservation Configuration", "", "CFF84B6D-C852-4FC4-B602-9F045EDC8854", "fa fa-gear" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockTypeByGuid( "Reservation Configuration", "Displays the details of the given Connection Type for editing.", "~/Plugins/com_bemaservices/RoomManagement/ReservationConfiguration.ascx", "com_bemaservices > Room Management", "6931E212-A76A-4DBB-9B97-86E5CDD0793A" );
            RockMigrationHelper.AddBlock( "CFF84B6D-C852-4FC4-B602-9F045EDC8854", "", "6931E212-A76A-4DBB-9B97-86E5CDD0793A", "Reservation Configuration", "Main", "", "", 0, "2B864E89-27DE-41F9-A24B-8D2EA5C40D10" );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_Reservation_ReservationTypeId]
                ALTER TABLE[dbo].[_com_bemaservices_RoomManagement_Reservation] DROP COLUMN[ReservationTypeId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_ReservationTypeId]
                ALTER TABLE[dbo].[_com_bemaservices_RoomManagement_Reservation] DROP COLUMN[ReservationTypeId]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationWorkflowTrigger] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationWorkflowTrigger_ReservationTypeId]
                ALTER TABLE[dbo].[_com_bemaservices_RoomManagement_Reservation] DROP COLUMN[ReservationTypeId]
" );
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_FinalApprovalGroupId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_SuperAdminGroupId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_NotificationEmailId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_ModifiedByPersonAliasId]
                DROP TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]" );
        }

        private string GenerateDefaultReservationTypeSql()
        {
            string finalApprovalGroupValue = null;
            string superAdminGroupValue = null;
            string notificationEmailValue = null;
            int? defaultSetupTime = -1;
            bool isCommunicationHistorySaved = false;
            bool isNumberAttendingRequired = true;
            bool isContactDetailsRequired = true;
            bool isSetupTimeRequired = true;

            var blockIdSql = SqlScalar( @"
                Select Id
                From Block
                Where [Guid] = '65091E04-77CE-411C-989F-EAD7D15778A0'
                " );
            int? blockId = blockIdSql.ToString().AsIntegerOrNull();
            if ( blockId.HasValue )
            {
                finalApprovalGroupValue = GetAttributeValueFromBlock( blockId.Value, "E715D25F-CA53-4B16-B8B2-4A94FD3A3560".AsGuid() );
                superAdminGroupValue = GetAttributeValueFromBlock( blockId.Value, "BBA41563-5379-43FA-955B-93C1926A4F66".AsGuid() );
                notificationEmailValue = GetAttributeValueFromBlock( blockId.Value, "F3FBDD84-5E9B-40C2-B199-3FAE1C2308DC".AsGuid() );

                var defaultSetupTimeValue = GetAttributeValueFromBlock( blockId.Value, "2FA0C64D-9511-4278-9445-BD0A847EA299".AsGuid() );
                if ( defaultSetupTimeValue != null )
                {
                    defaultSetupTime = defaultSetupTimeValue.AsIntegerOrNull();
                }

                var isCommunicationHistorySavedValue = GetAttributeValueFromBlock( blockId.Value, "B90006F5-9B17-48DD-B455-5BAA2BE1A9A2".AsGuid() );
                if ( isCommunicationHistorySavedValue != null )
                {
                    isCommunicationHistorySaved = isCommunicationHistorySavedValue.AsBoolean();
                }

                var isNumberAttendingRequiredValue = GetAttributeValueFromBlock( blockId.Value, "7162CFE4-FACD-4D75-8F09-2D42DBF1A887".AsGuid() );
                if ( isNumberAttendingRequiredValue != null )
                {
                    isNumberAttendingRequired = isNumberAttendingRequiredValue.AsBoolean();
                }

                var isContactDetailsRequiredValue = GetAttributeValueFromBlock( blockId.Value, "1C8DE8CB-E078-4483-9648-7C2CC57E6985".AsGuid() );
                if ( isContactDetailsRequiredValue != null )
                {
                    isContactDetailsRequired = isContactDetailsRequiredValue.AsBoolean();
                }

                var isSetupTimeRequiredValue = GetAttributeValueFromBlock( blockId.Value, "A184337B-BB99-4261-A295-0F54447CF0C6".AsGuid() );
                if ( isSetupTimeRequiredValue != null )
                {
                    isSetupTimeRequired = isSetupTimeRequiredValue.AsBoolean();
                }
            }

            var sqlQry = string.Format( @"
DECLARE @finalApprovalGroupId INT = NULL;
DECLARE @superAdminGroupId INT = NULL;
DECLARE @notificationEmailId INT = NULL;

SET @finalApprovalGroupId = (Select Id From [Group] Where [Guid] = '{2}')
SET @superAdminGroupId = (Select Id From [Group] Where [Guid] = '{3}')
SET @notificationEmailId = (Select Id From [SystemEmail] Where [Guid] = '{4}')

INSERT INTO [dbo].[_com_bemaservices_RoomManagement_ReservationType](
	                [IsSystem],
                    [Name],
	                [Description],
	                [IsActive],
                    [IconCssClass],
                    [FinalApprovalGroupId],
                    [SuperAdminGroupId],
                    [NotificationEmailId],
                    [DefaultSetupTime],
                    [IsCommunicationHistorySaved],
                    [IsNumberAttendingRequired],
                    [IsContactDetailsRequired],
                    [IsSetupTimeRequired],
	                [Guid])
VALUES
                    (1,
                    '{0}',
                    '{1}',
                    1,
                    'fa fa-home',
                    @finalApprovalGroupId,
                    @superAdminGroupId,
                    @notificationEmailId,
                    {5},
                    {6},
                    {7},
                    {8},
                    {9},
                    'E443F926-0882-41D5-91EF-480EA366F660')"
                    , "Standard Reservation Type" // Name
                    , "The default reservation type." // Description
                    , !String.IsNullOrWhiteSpace( finalApprovalGroupValue ) ? finalApprovalGroupValue : Guid.Empty.ToString() //Final Approval Group Id
                    , !String.IsNullOrWhiteSpace( superAdminGroupValue ) ? superAdminGroupValue : Guid.Empty.ToString() // SuperAdminGroupId
                    , !String.IsNullOrWhiteSpace( notificationEmailValue ) ? notificationEmailValue : Guid.Empty.ToString() //NotificationEmailId
                    , defaultSetupTime.HasValue ? defaultSetupTime.ToString() : "NULL" //DefaultSetupTime
                    , isCommunicationHistorySaved ? 1 : 0 // Is Communication History Saved
                    , isNumberAttendingRequired ? 1 : 0 // Is Number Attending Required
                    , isContactDetailsRequired ? 1 : 0 // Is Contact Details Required
                    , isSetupTimeRequired ? 1 : 0 // Is Setup Time Required
);
            return sqlQry;
        }

        private string GetAttributeValueFromBlock( int blockId, Guid attributeGuid )
        {
            var valueSql = SqlScalar( String.Format( @"
                Select av.Value
                From AttributeValue av
                Join Attribute a on av.AttributeId = a.Id
                Where av.EntityId = {0}
                And a.Guid = '{1}'
                ",
                blockId,
                attributeGuid.ToString() ) );
            if ( valueSql != null )
            {
                return valueSql.ToString();
            }
            else
            {
                return null;
            }
        }

        private void AddSecurityAuthForReservationType( string reservationTypeGuid, int order, string action, bool allow, string groupGuid, Rock.Model.SpecialRole specialRole, string authGuid )
        {
            if ( string.IsNullOrWhiteSpace( groupGuid ) )
            {
                groupGuid = Guid.Empty.ToString();
            }

            string entityTypeName = "com.bemaservices.RoomManagement.Model.ReservationType";

            string sql = @"
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [name] = '{0}')
    DECLARE @ReservationTypeId int = (SELECT TOP 1 [Id] FROM [_com_bemaservices_RoomManagement_ReservationType] WHERE [Guid] = '{1}')

    IF @EntityTypeId IS NOT NULL AND @ReservationTypeId IS NOT NULL
    BEGIN

        DECLARE @GroupId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '{2}')

        IF NOT EXISTS (
            SELECT [Id] FROM [dbo].[Auth]
            WHERE [EntityTypeId] = @EntityTypeId
            AND [EntityId] = @ReservationTypeId
            AND [Action] = '{4}'
            AND [AllowOrDeny] = '{5}'
            AND [SpecialRole] = {6}
            AND [GroupId] = @GroupId
        )
        BEGIN
            INSERT INTO [dbo].[Auth]
                   ([EntityTypeId]
                   ,[EntityId]
                   ,[Order]
                   ,[Action]
                   ,[AllowOrDeny]
                   ,[SpecialRole]
                   ,[GroupId]
                   ,[Guid])
             VALUES
                   (@EntityTypeId
                   ,@ReservationTypeId
                   ,{3}
                   ,'{4}'
                   ,'{5}'
                   ,{6}
                   ,@GroupId
                   ,'{7}')
        END
    END
";

            Sql( string.Format( sql,
                entityTypeName,                 // 0
                reservationTypeGuid,                   // 1
                groupGuid,                      // 2
                order,                          // 3
                action,                         // 4
                ( allow ? "A" : "D" ),          // 5
                specialRole.ConvertToInt(),     // 6
                authGuid ) );                   // 7

        }

        /// <summary>
        /// Adds the action type attribute value in the situation where the attributeGuid
        /// is not well-known.
        /// </summary>
        /// <param name="actionTypeGuid">The action type unique identifier.</param>
        /// <param name="actionEntityTypeGuid">The action entity type unique identifier.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="attributeDescription">The attribute description.</param>
        /// <param name="attributeOrder">The attribute order.</param>
        /// <param name="attributeDefaultValue">The attribute default value.</param>
        /// <param name="value">The value.</param>
        private void AddActionTypeAttributeValue( string actionTypeGuid, string actionEntityTypeGuid, string fieldTypeGuid, string attributeName, string attributeKey, string attributeDescription, int attributeOrder, string attributeDefaultValue, string value )
        {
            Sql( string.Format( @"

                DECLARE @ActionEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '{0}')
                DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{1}')
                DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowActionType')
                DECLARE @AttributeGuid uniqueidentifier = (SELECT [Guid] FROM [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'EntityTypeId' AND [EntityTypeQualifierValue] = CAST(@ActionEntityTypeId as varchar) AND [Key] = '{2}' )
                DECLARE @AttributeId int

                -- Find or add the action type's attribute
                IF @AttributeGuid IS NOT NULL
                BEGIN
                    SET @Attributeid = (SELECT [Id] FROM [Attribute] WHERE [Guid] = @AttributeGuid)
                END
                ELSE
                BEGIN
                    INSERT INTO [Attribute] (
                        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
                        [Key],[Name],[Description],
                        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],
                        [Guid])
                    VALUES(
                        1,@FieldTypeId, @EntityTypeId,'EntityTypeId',CAST(@ActionEntityTypeId as varchar),
                        '{2}','{3}','{4}',
                        {5},0,'{6}',0,0,
                        NEWID() )
                    SET @AttributeId = SCOPE_IDENTITY()
                END

                -- Now set the action type's attribute value
                DECLARE @ActionTypeId int = (SELECT [Id] FROM [WorkflowActionType] WHERE [Guid] = '{7}')

                IF @ActionTypeId IS NOT NULL AND @AttributeId IS NOT NULL
                BEGIN

                    -- Delete existing attribute value
                    DELETE [AttributeValue]
                    WHERE [AttributeId] = @AttributeId
                    AND [EntityId] = @ActionTypeId

                    INSERT INTO [AttributeValue] (
                        [IsSystem],[AttributeId],[EntityId],
                        [Value],
                        [Guid])
                    VALUES(
                        1,@AttributeId,@ActionTypeId,
                        '{8}',
                        NEWID())

                END
",
                    actionEntityTypeGuid,
                    fieldTypeGuid,
                    attributeKey ?? attributeName.Replace( " ", string.Empty ),
                    attributeName,
                    attributeDescription.Replace( "'", "''" ),
                    attributeOrder,
                    attributeDefaultValue.Replace( "'", "''" ),
                    actionTypeGuid,
                    value.Replace( "'", "''" ) )
            );
        }

        public void UpdateEntityTypeByGuid( string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured, string guid )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{5}' )
                BEGIN
                    UPDATE [EntityType] SET
                        [FriendlyName] = '{1}',
                        [AssemblyName] = '{2}',
                        [IsEntity] = {3},
                        [IsSecured] = {4},
                        [Name] = '{0}'
                    WHERE [Guid] = '{5}'
                END
                ELSE
                BEGIN
                    DECLARE @Guid uniqueidentifier
                    SET @Guid = (SELECT [Guid] FROM [EntityType] WHERE [Name] = '{0}')
                    IF @Guid IS NULL
                    BEGIN
                        INSERT INTO [EntityType] (
                            [Name],[FriendlyName],[AssemblyName],[IsEntity],[IsSecured],[IsCommon],[Guid])
                        VALUES(
                            '{0}','{1}','{2}',{3},{4},0,'{5}')
                    END
                    ELSE
                    BEGIN

                        UPDATE [EntityType] SET
                            [FriendlyName] = '{1}',
                            [AssemblyName] = '{2}',
                            [IsEntity] = {3},
                            [IsSecured] = {4},
                            [Guid] = '{5}'
                        WHERE [Name] = '{0}'

                        -- Update any attribute values that might have been using entity's old guid value
	                    DECLARE @EntityTypeFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.EntityTypeFieldType' )
	                    DECLARE @ComponentFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.ComponentFieldType' )
	                    DECLARE @ComponentsFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.ComponentsFieldType' )

                        UPDATE V SET [Value] = REPLACE( LOWER([Value]), LOWER(CAST(@Guid AS varchar(50))), LOWER('{5}') )
	                    FROM [AttributeValue] V
	                    INNER JOIN [Attribute] A ON A.[Id] = V.[AttributeId]
	                    WHERE ( A.[FieldTypeId] = @EntityTypeFieldTypeId OR A.[FieldTypeId] = @ComponentFieldTypeId	OR A.[FieldTypeId] = @ComponentsFieldTypeId )
                        OPTION (RECOMPILE)

                    END
                END
",
                    name.Replace( "'", "''" ),
                    friendlyName.Replace( "'", "''" ),
                    assemblyName.Replace( "'", "''" ),
                    isEntity ? "1" : "0",
                    isSecured ? "1" : "0",
                    guid ) );
        }
    }
}