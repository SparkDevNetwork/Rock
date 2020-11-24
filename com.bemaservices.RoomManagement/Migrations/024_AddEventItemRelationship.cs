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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration to add the EventItem (occurrence) relationship
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 24, "1.9.4" )]
    public class AddEventItemRelationship : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] ADD [EventItemOccurrenceId] [int] NULL

                CREATE INDEX IX_EventItemOccurrenceId ON [dbo].[_com_bemaservices_RoomManagement_Reservation] (EventItemOccurrenceId)
                " );

            Sql( @"ALTER TABLE [_com_bemaservices_RoomManagement_ReservationType] ADD [IsReservationBookedOnApproval] [bit] NOT NULL DEFAULT 0;" );
            Sql( @"UPDATE [_com_bemaservices_RoomManagement_ReservationType] SET [IsReservationBookedOnApproval] = 0;
                " );

            // Page: Event Occurrence
            RockMigrationHelper.UpdateBlockTypeByGuid( "Reservation List", "Block for viewing a list of reservations.", "~/Plugins/com_bemaservices/RoomManagement/ReservationList.ascx", "com_bemaservices > Room Management", "8169F541-9544-4A41-BD90-0DC2D0144AFD" );
            // Add Block to Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4B0C44EE-28E3-4753-A95B-8C57CD958FD1", "", "8169F541-9544-4A41-BD90-0DC2D0144AFD", "Reservation List", "Main", "", "", 4, "B16EB997-3107-4A64-AA9B-93CF01460622" );
            // Attrib for BlockType: Reservation List:Related Entity Query String Parameter
            RockMigrationHelper.UpdateBlockTypeAttribute( "8169F541-9544-4A41-BD90-0DC2D0144AFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Related Entity Query String Parameter", "RelatedEntityQueryStringParameter", "", "The query string parameter that holds id to the related entity.", 0, @"", "81CCF555-CCD5-4FAB-9B90-DE449888E066" );
            // Attrib for BlockType: Reservation List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "8169F541-9544-4A41-BD90-0DC2D0144AFD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "3DD653FB-771D-4EE5-8C75-1BF1B6F773B8" );
            // Attrib Value for Block:Reservation List, Attribute:Detail Page Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B16EB997-3107-4A64-AA9B-93CF01460622", "3DD653FB-771D-4EE5-8C75-1BF1B6F773B8", @"4cbd2b96-e076-46df-a576-356bca5e577f" );
            // Attrib Value for Block:Reservation List, Attribute:Related Entity Query String Parameter Page: Event Occurrence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B16EB997-3107-4A64-AA9B-93CF01460622", "81CCF555-CCD5-4FAB-9B90-DE449888E066", @"EventItemOccurrenceId" );

            // Page: Event Detail
            // Attrib for BlockType: Calendar Event Item Occurrence List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "46647F7B-2DC3-43FF-88F7-962F516FA969" );
            // Attrib for BlockType: Calendar Event Item Occurrence List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "93803C69-EEFE-4E3C-A2F4-DD517883C72B" );
            // Attrib Value for Block:Calendar Item Campus List, Attribute:core.CustomGridColumnsConfig Page: Event Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "828C8FE3-D5F8-4C22-BA81-844D704842EA", "46647F7B-2DC3-43FF-88F7-962F516FA969", @"{""ColumnsConfig"":[{""HeaderText"":""Reservations"",""HeaderClass"":"""",""ItemClass"":"""",""LavaTemplate"":""{% reservation where:'EventItemOccurrenceId == {{Row.Id}}' %}\n    {% for reservation in reservationItems %}\n        <small><a href='/ReservationDetail?ReservationId={{reservation.Id}}'>{{reservation.Name}}</a></small></br>\n    {% endfor %}\n{% endreservation %}"",""PositionOffsetType"":1,""PositionOffset"":2}]}" );
            // Attrib Value for Block:Calendar Item Campus List, Attribute:core.CustomGridEnableStickyHeaders Page: Event Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "828C8FE3-D5F8-4C22-BA81-844D704842EA", "93803C69-EEFE-4E3C-A2F4-DD517883C72B", @"False" );

            // Page: New Reservation
            // Attrib for BlockType: Reservation Detail:Default Calendar
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "EC0D9528-1A22-404E-A776-566404987363", "Default Calendar", "DefaultCalendar", "", "The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.", 0, @"", "5FE9D1C2-5AEF-47F4-A060-2E2F45A49AB6" );
            // Attrib for BlockType: Reservation Detail:View Reservation Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "View Reservation Instructions Lava Template", "LavaInstruction_ViewReservation", "", "Instructions here will show up the reservation view panel.", 0, @"", "F4F58248-D8AF-45A5-A03B-502B0C0D1B71" );
            // Attrib for BlockType: Reservation Detail:Edit Reservation Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Edit Reservation Instructions Lava Template", "LavaInstruction_EditReservation", "", "Instructions here will show up the reservation edit panel.", 1, @"", "F6580D58-79DB-4FF1-BD7D-FAB25A1F36C7" );
           // Attrib for BlockType: Reservation Detail:Allow Creating New Calendar Events
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Creating New Calendar Events", "AllowCreatingNewCalendarEvents", "", "If set to \"Yes\", the staff person will be offered the \"New Event\" tab to create a new event and a new occurrence of that event, rather than only picking from existing events.", 2, @"true", "3774B102-BBCA-4176-8086-949E66375624" );
            // Attrib for BlockType: Reservation Detail:Include Inactive Calendar Items\
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Calendar Items", "IncludeInactiveCalendarItems", "", "Check this box to hide inactive calendar items.", 3, @"true", "85951BD3-3D41-49C8-9D6C-214B125EA905" );
            // Attrib for BlockType: Reservation Detail:Completion Workflow
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Completion Workflow", "CompletionWorkflow", "", "A workflow that will be launched when the wizard is complete.  The following attributes will be passed to the workflow:   + Reservation   + EventItemOccurrenceGuid", 4, @"", "5F8DE0AE-A602-42A4-A69D-2EC438884917" );
            // Attrib for BlockType: Reservation Detail:Event Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Event Instructions Lava Template", "LavaInstruction_Event", "", "Instructions here will show up on the fourth panel of the wizard.", 4, @"", "0A5F8843-0375-434C-88C9-987CA14B435B" );
            // Attrib for BlockType: Reservation Detail:Event Occurrence Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Event Occurrence Instructions Lava Template", "LavaInstruction_EventOccurrence", "", "Instructions here will show up on the fifth panel of the wizard.", 5, @"", "8136EECF-7356-4055-90EA-18C9F5854290" );
            // Attrib for BlockType: Reservation Detail:Display Link to Event Details Page on Confirmation Screen
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Link to Event Details Page on Confirmation Screen", "DisplayEventDetailsLink", "", "Check this box to show the link to the event details page in the wizard confirmation screen.", 5, @"true", "DDA678AD-1001-46F7-9FF9-171F964B393F" );
            // Attrib for BlockType: Reservation Detail:External Event Details Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "External Event Details Page", "EventDetailsPage", "", "Determines which page the link in the final confirmation screen will take you to (if \"Display Link to Event Details... \" is selected).", 6, @"8A477CC6-4A12-4FBE-8037-E666476DD413", "444F8CF7-0834-4171-A058-0E2A5D65CFCD" );
            // Attrib for BlockType: Reservation Detail:Summary Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Summary Instructions Lava Template", "LavaInstruction_Summary", "", "Instructions here will show up on the sixth panel of the wizard.", 6, @"", "58C22021-8982-44DE-A3D3-C0CD315F0CAB" );
            // Attrib for BlockType: Reservation Detail:Wizard Finished Instructions Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Wizard Finished Instructions Lava Template", "LavaInstruction_Finished", "", "Instructions here will show up on the final panel of the wizard.", 7, @"", "A21B09B1-0FD5-43EF-9FEA-5693850F14A2" );
            // Attrib Value for Block:Reservation Detail, Attribute:Default Calendar Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "5FE9D1C2-5AEF-47F4-A060-2E2F45A49AB6", @"8a444668-19af-4417-9c74-09f842572974" );
            // Attrib Value for Block:Reservation Detail, Attribute:Allow Creating New Calendar Events Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "3774B102-BBCA-4176-8086-949E66375624", @"False" );
            // Attrib Value for Block:Reservation Detail, Attribute:Include Inactive Calendar Items Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "85951BD3-3D41-49C8-9D6C-214B125EA905", @"True" );
            // Attrib Value for Block:Reservation Detail, Attribute:Completion Workflow Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "5F8DE0AE-A602-42A4-A69D-2EC438884917", @"417d8016-92dc-4f25-acff-a071b591fa4f" );
            // Attrib Value for Block:Reservation Detail, Attribute:Display Link to Event Details Page on Confirmation Screen Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "DDA678AD-1001-46F7-9FF9-171F964B393F", @"True" );
            // Attrib Value for Block:Reservation Detail, Attribute:External Event Details Page Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "444F8CF7-0834-4171-A058-0E2A5D65CFCD", @"8a477cc6-4a12-4fbe-8037-e666476dd413" );
            // Attrib Value for Block:Reservation Detail, Attribute:View Reservation Instructions Lava Template Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "F4F58248-D8AF-45A5-A03B-502B0C0D1B71", @"" );
            // Attrib Value for Block:Reservation Detail, Attribute:Edit Reservation Instructions Lava Template Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "F6580D58-79DB-4FF1-BD7D-FAB25A1F36C7", @"" );
            // Attrib Value for Block:Reservation Detail, Attribute:Event Instructions Lava Template Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "0A5F8843-0375-434C-88C9-987CA14B435B", @"" );
            // Attrib Value for Block:Reservation Detail, Attribute:Event Occurrence Instructions Lava Template Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "8136EECF-7356-4055-90EA-18C9F5854290", @"" );
            // Attrib Value for Block:Reservation Detail, Attribute:Summary Instructions Lava Template Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "58C22021-8982-44DE-A3D3-C0CD315F0CAB", @"" );
            // Attrib Value for Block:Reservation Detail, Attribute:Wizard Finished Instructions Lava Template Page: New Reservation, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "A21B09B1-0FD5-43EF-9FEA-5693850F14A2", @"" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "A21B09B1-0FD5-43EF-9FEA-5693850F14A2" );
            RockMigrationHelper.DeleteAttribute( "58C22021-8982-44DE-A3D3-C0CD315F0CAB" );
            RockMigrationHelper.DeleteAttribute( "8136EECF-7356-4055-90EA-18C9F5854290" );
            RockMigrationHelper.DeleteAttribute( "0A5F8843-0375-434C-88C9-987CA14B435B" );
            RockMigrationHelper.DeleteAttribute( "F6580D58-79DB-4FF1-BD7D-FAB25A1F36C7" );
            RockMigrationHelper.DeleteAttribute( "F4F58248-D8AF-45A5-A03B-502B0C0D1B71" );
            RockMigrationHelper.DeleteAttribute( "444F8CF7-0834-4171-A058-0E2A5D65CFCD" );
            RockMigrationHelper.DeleteAttribute( "DDA678AD-1001-46F7-9FF9-171F964B393F" );
            RockMigrationHelper.DeleteAttribute( "5F8DE0AE-A602-42A4-A69D-2EC438884917" );
            RockMigrationHelper.DeleteAttribute( "85951BD3-3D41-49C8-9D6C-214B125EA905" );
            RockMigrationHelper.DeleteAttribute( "3774B102-BBCA-4176-8086-949E66375624" );
            RockMigrationHelper.DeleteAttribute( "5FE9D1C2-5AEF-47F4-A060-2E2F45A49AB6" );
            RockMigrationHelper.DeleteAttribute( "614CD413-DCB7-4DA2-80A0-C7ABE5A11047" );
            RockMigrationHelper.DeleteAttribute( "8FB690EC-5299-46C5-8695-AAD23168E6E1" );
            RockMigrationHelper.DeleteBlock( "B16EB997-3107-4A64-AA9B-93CF01460622" );

            Sql( @"
                DROP INDEX IX_EventItemOccurrenceId ON [dbo].[_com_bemaservices_RoomManagement_Reservation]

                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation] DROP COLUMN [EventItemOccurrenceId]
                " );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] DROP COLUMN [IsReservationBookedOnApproval]
                " );

        }
    }
}
