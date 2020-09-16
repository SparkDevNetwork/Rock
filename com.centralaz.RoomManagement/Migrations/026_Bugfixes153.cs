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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 26, "1.9.4" )]
    public class Bugfixes153 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Detailed Email Subjects
            RockMigrationHelper.AddActionTypeAttributeValue( "7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Changes Needed: {{Workflow | Attribute:'Reservation'}}" ); // Room Reservation Approval Notification:Notify Requester that the Reservation Requires Changes:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "52520D50-9D35-4E0D-AF04-1A222B50EA91", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Approved: {{Workflow | Attribute:'Reservation'}}" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Approved:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "78761E92-CD94-438A-A3F2-FB683C8D8054", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Reservation Denied: {{Workflow | Attribute:'Reservation'}}" ); // Room Reservation Approval Notification:Notify Requester that the Reservation has been Denied:Send Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "FE7B413C-0DF4-4F9C-935C-8B39DA87742D", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Approval Needed: {{Workflow | Attribute:'Reservation'}}" ); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Subject

            // Added DefaultCleanupTime
            Sql( @"
                    ALTER TABLE [dbo].[_com_centralaz_RoomManagement_ReservationType] ADD [DefaultCleanupTime] INT NULL
            " );

            Sql( @"
                    UPDATE [dbo].[_com_centralaz_RoomManagement_ReservationType]
                    SET [DefaultCleanupTime] = [DefaultSetupTime]
            " );

            // Approval Workflow now references Admin Contact instead of Requestor
            RockMigrationHelper.UpdateWorkflowTypeAttribute("543D4FCD-310B-4048-BFCB-BAE582CBB890","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Admin Contact","AdminContact","",4,@"","F63057EA-27E2-4528-8E93-BAA75BE2122C", false); // Room Reservation Approval Notification:Admin Contact
            RockMigrationHelper.AddAttributeQualifier("F63057EA-27E2-4528-8E93-BAA75BE2122C","EnableSelfSelection",@"False","79996F88-7368-418C-9FA7-136DAD32A157"); // Room Reservation Approval Notification:Admin Contact:EnableSelfSelection

            RockMigrationHelper.UpdateWorkflowActivityType("543D4FCD-310B-4048-BFCB-BAE582CBB890",true,"Notify Admin Contact that the Reservation Requires Changes","",false,1,"2C1387D7-3E8F-4702-A9B9-4C2E52684EEE"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation Requires Changes
            RockMigrationHelper.UpdateWorkflowActivityType("543D4FCD-310B-4048-BFCB-BAE582CBB890",true,"Notify Admin Contact that the Reservation has been Approved","",false,2,"21F615D2-3A9C-421B-8EBF-013C43DE9E4F"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation has been Approved
            RockMigrationHelper.UpdateWorkflowActivityType("543D4FCD-310B-4048-BFCB-BAE582CBB890",true,"Notify Admin Contact that the Reservation has been Denied","",false,3,"3077D8F8-478E-4F12-A90F-B1642B9C0B6E"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation has been Denied
            RockMigrationHelper.UpdateWorkflowActivityType("543D4FCD-310B-4048-BFCB-BAE582CBB890",true,"Notify Approval group that the Reservation is Pending Review","",false,4,"CC968504-37E2-430C-B131-57B9556E0442"); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review

            RockMigrationHelper.UpdateWorkflowActionType("6A396018-6CC1-4C41-8EF1-FB9779C0B04D","Set Admin Contact From Entity",2,"972F19B9-598B-474B-97A4-50E56E7B59D2",true,false,"","",1,"","B0EE1D6E-07F8-4C1F-9320-B0855EF68703"); // Room Reservation Approval Notification:Set Attributes:Set Admin Contact From Entity
            RockMigrationHelper.AddActionTypeAttributeValue("B0EE1D6E-07F8-4C1F-9320-B0855EF68703","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7",@"f63057ea-27e2-4528-8e93-baa75be2122c"); // Room Reservation Approval Notification:Set Attributes:Set Admin Contact From Entity:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("B0EE1D6E-07F8-4C1F-9320-B0855EF68703","7D79FC31-D0ED-4DB0-AB7D-60F4F98A1199",@"{{ Entity.AdministrativeContactPersonAlias.Guid }}"); // Room Reservation Approval Notification:Set Attributes:Set Admin Contact From Entity:Lava Template

            RockMigrationHelper.AddActionTypeAttributeValue("7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF","0C4C13B8-7076-4872-925A-F950886B5E16",@"f63057ea-27e2-4528-8e93-baa75be2122c"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation Requires Changes:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> Your reservation requires changes:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/> Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.centralaz.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.centralaz.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation Requires Changes:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue("7AA18EA5-62D2-49A3-B410-B2D90F2A2EBF","1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3",@"True"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation Requires Changes:Send Email:Save Communication History

            RockMigrationHelper.AddActionTypeAttributeValue("52520D50-9D35-4E0D-AF04-1A222B50EA91","0C4C13B8-7076-4872-925A-F950886B5E16",@"f63057ea-27e2-4528-8e93-baa75be2122c"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation has been Approved:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("52520D50-9D35-4E0D-AF04-1A222B50EA91","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> Your reservation has been approved:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/>Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.centralaz.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.centralaz.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation has been Approved:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue("52520D50-9D35-4E0D-AF04-1A222B50EA91","1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3",@"True"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation has been Approved:Send Email:Save Communication History

            RockMigrationHelper.AddActionTypeAttributeValue("78761E92-CD94-438A-A3F2-FB683C8D8054","0C4C13B8-7076-4872-925A-F950886B5E16",@"f63057ea-27e2-4528-8e93-baa75be2122c"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation has been Denied:Send Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("78761E92-CD94-438A-A3F2-FB683C8D8054","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> Your reservation has been denied:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/> Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.centralaz.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.centralaz.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation has been Denied:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue("78761E92-CD94-438A-A3F2-FB683C8D8054","1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3",@"True"); // Room Reservation Approval Notification:Notify Admin Contact that the Reservation has been Denied:Send Email:Save Communication History

            RockMigrationHelper.AddActionTypeAttributeValue("FE7B413C-0DF4-4F9C-935C-8B39DA87742D","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ 'Global' | Attribute:'EmailHeader' }} {% assign reservation = Workflow | Attribute:'Reservation','Object' %}  <p> A new reservation requires your approval:<br/><br/> Name: {{ reservation.Name }}<br/> Admin Contact: {{ reservation.AdministrativeContactPersonAlias.Person.FullName }}<br/> Event Contact: {{ reservation.EventContactPersonAlias.Person.FullName }}<br/> Campus: {{ reservation.Campus.Name }}<br/> Ministry: {{ reservation.ReservationMinistry.Name }}<br/> Number Attending: {{ reservation.NumberAttending }}<br/> <br/> Schedule: {% execute import:'com.centralaz.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.FriendlyScheduleText; {% endexecute %}<br/> Event Duration: {% execute import:'com.centralaz.RoomManagement.Model'%}  var reservation = new ReservationService( new RockContext()).Get({{reservation.Id}}); return reservation.Schedule.GetCalenderEvent().Duration.Hours + "" hrs "" + reservation.Schedule.GetCalenderEvent().Duration.Minutes + "" min""; {% endexecute %}<br/> Setup Time: {{ reservation.SetupTime }} min<br/> Cleanup Time: {{ reservation.CleanupTime }} min<br/> {% assign locationSize = reservation.ReservationLocations | Size %}{% if locationSize > 0 %}Locations: {% assign firstLocation = reservation.ReservationLocations | First %}{% for location in reservation.ReservationLocations %}{% if location.Id != firstLocation.Id %}, {% endif %}{{location.Location.Name }}{% endfor %}<br/>{% endif %} {% assign resourceSize = reservation.ReservationResources | Size %}{% if resourceSize > 0 %}Resources: {% assign firstResource = reservation.ReservationResources | First %}{% for resource in reservation.ReservationResources %}{% if resource.Id != firstResource.Id %}, {% endif %}{{resource.Resource.Name }} ({{resource.Quantity}}){% endfor %}<br/>{% endif %} <br/> Notes: {{ reservation.Note }}<br/> <br/> <a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}reservationdetail?ReservationId={{reservation.Id}}'>View Registration</a> </p> {{ 'Global' | Attribute:'EmailFooter' }}"); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue("FE7B413C-0DF4-4F9C-935C-8B39DA87742D","1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3",@"True"); // Room Reservation Approval Notification:Notify Approval group that the Reservation is Pending Review:Send Email:Save Communication History           
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}