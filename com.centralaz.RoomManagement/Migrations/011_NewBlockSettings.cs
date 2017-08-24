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
using System;
using Rock.Model;
using Rock.Plugin;
using Rock;

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 11, "1.6.0" )]
    public class NewBlockSettings : Migration
    {
        public override void Up()
        {
            #region New Reservation Detail block settings

            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Setup & Cleanup Time", "RequireSetupCleanupTime", "", "Should the setup and cleanup time be required to be supplied?", 3, @"True", "A184337B-BB99-4261-A295-0F54447CF0C6" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Defatult Setup & Cleanup Time", "DefaultSetupCleanupTime", "", "If you wish to default to a particular setup and cleanup time, you can supply a value here. (Use -1 to indicate no default value)", 4, @"-1", "2FA0C64D-9511-4278-9445-BD0A847EA299" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Number Attending", "RequireNumberAttending", "", "Should the Number Attending be required to be supplied?", 5, @"True", "7162CFE4-FACD-4D75-8F09-2D42DBF1A887" );

            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "2FA0C64D-9511-4278-9445-BD0A847EA299", @"-1" ); // Defatult Setup & Cleanup Time
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "A184337B-BB99-4261-A295-0F54447CF0C6", @"True" ); // Require Setup & Cleanup Time
            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "7162CFE4-FACD-4D75-8F09-2D42DBF1A887", @"True" ); // Require Number Attending

            #endregion

            #region Reservation model changes

            // Since we only want to perform these changes once, let's use a try catch block trick to only perform the change once.
            try
            {
                // This should throw an exception if the new columns (AdministrativeContactPersonAliasId, AdministrativeContactPhone or AdministrativeContactEmail) are not there yet.
                Sql( @"SELECT COUNT (1) FROM [dbo].[_com_centralaz_RoomManagement_Reservation] WHERE [EventContactPersonAliasId] <> NULL" );
            }
            catch ( Exception )
            {
                // DropForeignKey, DropColumn and AddColumn are currently only available in Rock v7

                //DropForeignKey( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "FK__com_centralaz_RoomManagement_Reservation_ContactPersonAliasId" );
                //DropColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactPersonAliasId" );
                //DropColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactPhone" );
                //DropColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "ContactEmail" );

                //AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "EventContactPersonAliasId", c => c.Int() );
                //AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "EventContactPhone", c => c.String( maxLength: 50 ) );
                //AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "EventContactEmail", c => c.String( maxLength: 400 ) );

                Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_ContactPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP COLUMN [ContactPersonAliasId]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP COLUMN [ContactPhone]
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] DROP COLUMN [ContactEmail]

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] ADD [EventContactPersonAliasId] INT
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] ADD [EventContactPhone] NVARCHAR (50)
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] ADD [EventContactEmail] NVARCHAR (400)

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_EventContactPersonAliasId] FOREIGN KEY([EventContactPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                " );

                //AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "AdministrativeContactPersonAliasId", c => c.Int() );
                //AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "AdministrativeContactPhone", c => c.String( maxLength: 50 ) );
                //AddColumn( "[dbo].[_com_centralaz_RoomManagement_Reservation]", "AdministrativeContactEmail", c => c.String( maxLength: 400 ) );

                Sql( @"
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] ADD [AdministrativeContactPersonAliasId] INT
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] ADD [AdministrativeContactPhone] NVARCHAR (50)
                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] ADD [AdministrativeContactEmail] NVARCHAR (400)

                ALTER TABLE [dbo].[_com_centralaz_RoomManagement_Reservation] WITH CHECK ADD CONSTRAINT [FK__com_centralaz_RoomManagement_Reservation_AdministrativeContactPersonAliasId] FOREIGN KEY([AdministrativeContactPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])
                " );
            }

            #endregion

            #region Add sample Room Reservation Reminder Notification workflow
            RockMigrationHelper.UpdateWorkflowType( false, true, "Room Reservation Reminder Notification", "Used for sending a reminder email to the event contact regarding their upcoming resource reservation. IMPORTANT NOTE: This workflow contains Lava that uses the 'execute' Lava command. (Execute needs to be enabled in Global Attributes: 'Default Enabled Lava Commands')", "B8E4B3B0-B543-48B6-93BE-604D4F368559", "Reservation Reminders", "fa fa-list-ol", 28800, true, 0, "A219357D-4992-415E-BF5F-33C242BB3BD2", 0 ); // Room Reservation Reminder Notification
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "A219357D-4992-415E-BF5F-33C242BB3BD2", "66739D2C-1F39-44C4-BDBB-9AB181DA4ED7", "Reservation", "Reservation", "This will hold the Reservation that is passed in via the Entity.", 0, @"", "DA47F9C5-ED7C-410A-A7C2-3CD3854440ED", false ); // Room Reservation Reminder Notification:Reservation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "A219357D-4992-415E-BF5F-33C242BB3BD2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "ReservationId", "ReservationId", "Used to hold the id of the reservation.", 1, @"", "48B0A90C-04C6-421C-857D-81E4680DBC7F", false ); // Room Reservation Reminder Notification:ReservationId
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "A219357D-4992-415E-BF5F-33C242BB3BD2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Email To", "EmailTo", "The email address that will be receiving the reminder.", 2, @"", "5A6A4F0F-EB7C-4A69-B97F-4FD47DB80F44", false ); // Room Reservation Reminder Notification:Email To
            RockMigrationHelper.AddAttributeQualifier( "5A6A4F0F-EB7C-4A69-B97F-4FD47DB80F44", "ispassword", @"False", "B9E64655-C3E6-4637-8D0D-E12E313BAA08" ); // Room Reservation Reminder Notification:Email To:ispassword
            RockMigrationHelper.UpdateWorkflowActivityType( "A219357D-4992-415E-BF5F-33C242BB3BD2", true, "Start", "", true, 0, "50C79040-8B18-4FC9-929A-CA95677B1080" ); // Room Reservation Reminder Notification:Start
            RockMigrationHelper.UpdateWorkflowActionType( "50C79040-8B18-4FC9-929A-CA95677B1080", "Set Reservation From Entity", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "BA2BCF3D-B8A9-4E81-879A-77EE3AC1D30D" ); // Room Reservation Reminder Notification:Start:Set Reservation From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "50C79040-8B18-4FC9-929A-CA95677B1080", "Set Email To", 1, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "C75AC115-DCE6-4D60-8CA4-63649E6D1336" ); // Room Reservation Reminder Notification:Start:Set Email To
            RockMigrationHelper.UpdateWorkflowActionType( "50C79040-8B18-4FC9-929A-CA95677B1080", "Send Email", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "5A6A4F0F-EB7C-4A69-B97F-4FD47DB80F44", 64, "", "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766" ); // Room Reservation Reminder Notification:Start:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "50C79040-8B18-4FC9-929A-CA95677B1080", "Complete Workflow", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "79879E4D-4C00-4A2F-A311-BB6EA97A5B8E" ); // Room Reservation Reminder Notification:Start:Complete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "BA2BCF3D-B8A9-4E81-879A-77EE3AC1D30D", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Room Reservation Reminder Notification:Start:Set Reservation From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BA2BCF3D-B8A9-4E81-879A-77EE3AC1D30D", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB", @"" ); // Room Reservation Reminder Notification:Start:Set Reservation From Entity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "BA2BCF3D-B8A9-4E81-879A-77EE3AC1D30D", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"da47f9c5-ed7c-410a-a7c2-3cd3854440ed" ); // Room Reservation Reminder Notification:Start:Set Reservation From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "BA2BCF3D-B8A9-4E81-879A-77EE3AC1D30D", "83E27C1E-1491-4AE2-93F1-909791D4B70A", @"True" ); // Room Reservation Reminder Notification:Start:Set Reservation From Entity:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "BA2BCF3D-B8A9-4E81-879A-77EE3AC1D30D", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Room Reservation Reminder Notification:Start:Set Reservation From Entity:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "BA2BCF3D-B8A9-4E81-879A-77EE3AC1D30D", "EFC517E9-8A53-4681-B16B-9D4DE89244BE", @"" ); // Room Reservation Reminder Notification:Start:Set Reservation From Entity:Lava Template
            RockMigrationHelper.AddActionTypeAttributeValue( "C75AC115-DCE6-4D60-8CA4-63649E6D1336", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Room Reservation Reminder Notification:Start:Set Email To:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "C75AC115-DCE6-4D60-8CA4-63649E6D1336", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Room Reservation Reminder Notification:Start:Set Email To:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C75AC115-DCE6-4D60-8CA4-63649E6D1336", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"5a6a4f0f-eb7c-4a69-b97f-4fd47db80f44" ); // Room Reservation Reminder Notification:Start:Set Email To:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C75AC115-DCE6-4D60-8CA4-63649E6D1336", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{% assign Reservation = Workflow | Attribute:'Reservation','Object' %}{{ Reservation.EventContactEmail }}" ); // Room Reservation Reminder Notification:Start:Set Email To:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Room Reservation Reminder Notification:Start:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Room Reservation Reminder Notification:Start:Send Email:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Room Reservation Reminder Notification:Start:Send Email:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766", "0C4C13B8-7076-4872-925A-F950886B5E16", @"5a6a4f0f-eb7c-4a69-b97f-4fd47db80f44" ); // Room Reservation Reminder Notification:Start:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reminder: You have a reservation scheduled for..." ); // Room Reservation Reminder Notification:Start:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% assign Reservation = Workflow | Attribute: 'Reservation', 'Object' %}

{{ 'Global' | Attribute:'EmailHeader' }}
<p>
    We wanted to remind you about your upcoming scheduled reservation:<br/>
</p>

<table border='0' cellpadding='2' cellspacing='0' width='600' id='emailContainer'>
    <tr>
        <td align='left' valign='top' width='100'>Name:</td>
        <td>{{ Reservation.Name }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Event Contact:</td>
        <td><b>{{ Reservation.EventContactPersonAlias.Person.FullName }} ({{ Reservation.EventContactEmail }})</b></td>
    </tr>
    <tr>
        <td align='left' valign='top'>Administrative Contact:</td>
        <td><b>{{ Reservation.AdministrativeContactPersonAlias.Person.FullName }} ({{ Reservation.AdministrativeContactEmail }})</b></td>
    </tr>
    <tr>
        <td align='left' valign='top'>Entered By:</td>
        <td><b>{{ Reservation.RequesterAlias.Person.FullName }}</b></td>
    </tr>
    <tr>
        <td align='left' valign='top'>Campus:</td>
        <td>{{ Reservation.Campus.Name }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Ministry:</td>
        <td>{{ Reservation.ReservationMinistry.Name }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Number Attending:</td>
        <td>{{ Reservation.NumberAttending }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Schedule:</td>
        <td>{{ Reservation.Schedule.FriendlyScheduleText }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Event Duration:</td>
        <td>
{% execute import:'com.centralaz.RoomManagement.Model'%} 
var reservation = new ReservationService( new RockContext() ).Get({{Reservation.Id}});
return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min"";
{% endexecute %}
        </td>
    </tr>
    <tr>
        <td align='left' valign='top'>Setup Time:</td>
        <td>{{ Reservation.SetupTime }} min</td>
    </tr>
    
    <tr>
        <td align='left' valign='top'>Cleanup Time:</td>
        <td>{{ Reservation.CleanupTime }} min</td>
    </tr>
</table>

<p>&nbsp;</p>
{% assign locationSize = Reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Location(s): <b>{% assign firstLocation = Reservation.ReservationLocations | First %}{% for location in Reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}</b><br/>{% endif %}
{% assign resourceSize = Reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resource(s): <b>{% assign firstResource = Reservation.ReservationResources | First %}{% for resource in Reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}</b><br/>{% endif %}

<br/><br/>

{% if Reservation.Note and Reservation.Note != '' %}
    <p>
        Notes: {{ Reservation.Note }}<br/>
    </p>
{% endif %}

<!-- The button to view the reservation -->
<table>
    <tr>
        <td style='background-color: #ee7624;border-color: #e76812;border: 2px solid #e76812;padding: 10px;text-align: center;'>
            <a style='display: block;color: #ffffff;font-size: 12px;text-decoration: none;' href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{Reservation.Id}}'>
                View Reservation
            </a>
        </td>
    </tr>
</table>
<br/>
<br/>

{{ 'Global' | Attribute:'EmailFooter' }}
            " ); // Room Reservation Reminder Notification:Start:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "9A8EC5B3-D958-4AE9-8AC0-CDB4F2CE6766", "BAD36514-A7D6-4232-9051-85739FD1AE39", @"False" ); // Room Reservation Reminder Notification:Start:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "79879E4D-4C00-4A2F-A311-BB6EA97A5B8E", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Room Reservation Reminder Notification:Start:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "79879E4D-4C00-4A2F-A311-BB6EA97A5B8E", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Room Reservation Reminder Notification:Start:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "79879E4D-4C00-4A2F-A311-BB6EA97A5B8E", "3ECD0A79-C89B-433F-BE99-A2463847E7E1", @"Completed" ); // Room Reservation Reminder Notification:Start:Complete Workflow:Status|Status Attribute

            #endregion

            #region Add a Reservation Reminder Job

            // add ServiceJob: Reservation Reminder
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange' AND [Guid] = '6832E24B-5650-41D3-9EBA-1D2D213F768C' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'Reservation Reminder'
                  ,'To let the person who is the event contact know about their upcoming reservation.'
                  ,'com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange'
                  ,'0 0 8 1/1 * ? *'
                  ,1
                  ,'6832E24B-5650-41D3-9EBA-1D2D213F768C'
                  );
            END" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Class", "com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange", "Date Range", "The range of reservations to fire a workflow for.", 0, @",", "8F3BEC15-A076-4C07-8047-D85C319F8DBF", "DateRange" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Class", "com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange", "Include only reservations that start in date range", "", 0, @"False", "3E394836-2175-4C5B-9247-063BCB6CD6D2", "StartsInDateRange" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Class", "com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange", "Workflow Type", "The workflow type to fire for eligible reservations.  The type MUST have a 'ReservationId' attribute that will be set by this job.", 0, @"", "E14B7BA0-77D8-4553-B2E0-070FF3ECA34E", "WorkflowType" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Class", "com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange", "Reservation Statuses", "The reservation statuses to filter by", 0, @"", "49D7EDB4-E2FB-4081-B4D5-43D8217BF105", "Status" );
         
            #endregion
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "7162CFE4-FACD-4D75-8F09-2D42DBF1A887" );
            RockMigrationHelper.DeleteAttribute( "A184337B-BB99-4261-A295-0F54447CF0C6" );
            RockMigrationHelper.DeleteAttribute( "2FA0C64D-9511-4278-9445-BD0A847EA299" );

            RockMigrationHelper.DeleteAttribute( "8F3BEC15-A076-4C07-8047-D85C319F8DBF" ); // com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange: Date Range
            RockMigrationHelper.DeleteAttribute( "3E394836-2175-4C5B-9247-063BCB6CD6D2" ); // com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange: Include only reservations that start in date range
            RockMigrationHelper.DeleteAttribute( "E14B7BA0-77D8-4553-B2E0-070FF3ECA34E" ); // com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange: Workflow Type
            RockMigrationHelper.DeleteAttribute( "49D7EDB4-E2FB-4081-B4D5-43D8217BF105" ); // com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange: Reservation Statuses

            // remove ServiceJob: Reservation Reminder
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'com.centralaz.RoomManagement.Jobs.FireWorkflowFromReservationInDateRange' AND [Guid] = '6832E24B-5650-41D3-9EBA-1D2D213F768C' )
            BEGIN
               DELETE [ServiceJob]  WHERE [Guid] = '6832E24B-5650-41D3-9EBA-1D2D213F768C';
            END" );
        }
    }
}
