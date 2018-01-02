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
    [MigrationNumber( 17, "1.6.0" )]
    public class AddDetailPageToReservationList : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.AddBlockAttributeValue( true, "4D4882F8-5ACC-4AE1-BC75-4FFDDA26F270", "3DD653FB-771D-4EE5-8C75-1BF1B6F773B8", @"4cbd2b96-e076-46df-a576-356bca5e577f,893ff97e-57d2-42e0-bf9a-6027d673773c" ); // Detail Page







            RockMigrationHelper.UpdateWorkflowTypeAttribute( "543D4FCD-310B-4048-BFCB-BAE582CBB890", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Approval State", "ApprovalState", "", 0, @"", "9E1FD102-AA41-4F7A-A52D-601DB1852A44" ); // Room Reservation Approval Notification:Approval State
            RockMigrationHelper.AddAttributeQualifier( "9E1FD102-AA41-4F7A-A52D-601DB1852A44", "ispassword", @"False", "DE1514A5-D910-4BEB-BA30-30D4D44F135B" ); // Room Reservation Approval Notification:Approval State:ispassword


            RockMigrationHelper.UpdateWorkflowActionType( "CC968504-37E2-430C-B131-57B9556E0442", "Activate Approved Activity", 1, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "653CE164-554A-4B22-A830-3E760DA2023E", 32, "", "E0991B14-DE3A-4103-8B73-77CBFDB4826E" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Activate Approved Activity
            RockMigrationHelper.UpdateWorkflowActionType( "CC968504-37E2-430C-B131-57B9556E0442", "Complete Workflow", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "653CE164-554A-4B22-A830-3E760DA2023E", 64, "", "A9D101D2-A87C-4A12-8BCA-118F15B682CC" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "CC968504-37E2-430C-B131-57B9556E0442", "Send Email", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "653CE164-554A-4B22-A830-3E760DA2023E", 64, "", "FE7B413C-0DF4-4F9C-935C-8B39DA87742D" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Set Approval State From Entity", 2, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity
            RockMigrationHelper.UpdateWorkflowActionType( "CC968504-37E2-430C-B131-57B9556E0442", "Set Reservation to Approved", 0, "3894452A-E763-41AC-8260-10373646D8A0", true, false, "", "653CE164-554A-4B22-A830-3E760DA2023E", 32, "", "B1F80A78-279D-4691-B030-E035FDCAA3C8" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved
            RockMigrationHelper.UpdateWorkflowActionType( "6A396018-6CC1-4C41-8EF1-FB9779C0B04D", "Complete Workflow if Reservation is Unapproved", 7, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, false, "", "9E1FD102-AA41-4F7A-A52D-601DB1852A44", 1, "Unapproved", "D9FDA437-781D-47AC-856B-14E9F779AACD" ); // Room Reservation Approval Notification:Set Attributes:Complete Workflow if Reservation is Unapproved

            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"9e1fd102-aa41-4f7a-a52d-601db1852a44" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4D71BFDF-E3B1-4E79-9577-F8BB765A18A7", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"{% assign reservation =  Workflow | Attribute:'Reservation', 'object' %}{{ reservation.ApprovalState }}" ); // Room Reservation Approval Notification:Set Attributes:Set Approval State From Entity:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "1D4F819F-145D-4A7F-AB4E-AD7C06759042", @"aeaca577-f09f-4818-8c09-ca5490b0dd91" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Reservation Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "25954FDC-F486-417D-ABBB-E2DF2C67B186", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "ACA008E2-2406-457E-8E4C-6922E03757A4", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "2E185FB5-FC8E-41BE-B7FE-702F74B47539", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Approval State Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B1F80A78-279D-4691-B030-E035FDCAA3C8", "C32C481E-3123-4347-A5FA-E3C79FE3D4A2", @"2" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Set Reservation to Approved:Approval State
            RockMigrationHelper.AddActionTypeAttributeValue( "E0991B14-DE3A-4103-8B73-77CBFDB4826E", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Activate Approved Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E0991B14-DE3A-4103-8B73-77CBFDB4826E", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Activate Approved Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "E0991B14-DE3A-4103-8B73-77CBFDB4826E", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"21F615D2-3A9C-421B-8EBF-013C43DE9E4F" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Activate Approved Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "D1269254-C15A-40BD-B784-ADCC231D3950", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "0C4C13B8-7076-4872-925A-F950886B5E16", @"653ce164-554a-4b22-a830-3e760da2023e" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "70543346-2D64-45F9-BE15-F9F0D29AF4F0", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Send to Group Role
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
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "5CEB1A94-D4F2-4187-B4F5-155C0201CD39", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "A9D101D2-A87C-4A12-8BCA-118F15B682CC", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A9D101D2-A87C-4A12-8BCA-118F15B682CC", "25CAD4BE-5A00-409D-9BAB-E32518D89956", @"" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "A9D101D2-A87C-4A12-8BCA-118F15B682CC", "E2B75188-6DE3-4BB3-80E6-CD88187F498D", @"Completed" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Complete Workflow:Status|Status Attribute

        }
        public override void Down()
        {

        }
    }
}
