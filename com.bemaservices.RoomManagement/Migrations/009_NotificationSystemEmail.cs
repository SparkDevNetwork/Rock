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
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 9, "1.6.0" )]
    public class NotificationSystemEmail : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            #region Room Reservation Approval Notification (System Email)

            RockMigrationHelper.UpdateSystemEmail( "Plugins", "Room Reservation Approval Notification", "", "", "", "", "", "Room Reservation Approval Notification", @"{{ 'Global' | Attribute:'EmailHeader' }}
<p>
A reservation requires approval:<br/>
</p>

<table border='0' cellpadding='2' cellspacing='0' width='600' id='emailContainer'>
    <tr>
        <td align='left' valign='top' width='100'>Name:</td>
        <td>{{ Reservation.Name }}</td>
    </tr>
    <tr>
        <td align='left' valign='top'>Requestor:</td>
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
{% execute import:'com.bemaservices.RoomManagement.Model'%} 
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
<p>
    Notes: {{ Reservation.Note }}<br/>
</p>

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
            ", "9D00FEE5-D805-441C-9F96-0582F894B5EB" );
            #endregion

            #region Update Reservation Detail Block settings

            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "System Email", "SystemEmail", "", "A system email to use when notifying approvers about a reservation request.", 0, @"", "F3FBDD84-5E9B-40C2-B199-3FAE1C2308DC" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Final Approval Group", "FinalApprovalGroup", "", "An optional group that provides final approval for a reservation. If used, this should be the same group as in the Reservation Approval Workflow.", 0, @"", "E715D25F-CA53-4B16-B8B2-4A94FD3A3560" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Super Admin Group", "SuperAdminGroup", "", "The superadmin group that can force an approve / deny status on reservations, i.e. a facilities team.", 0, @"FBE0324F-F29A-4ACF-8EC3-5386C5562D70", "BBA41563-5379-43FA-955B-93C1926A4F66" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C938B1DE-9AB3-46D9-AB28-57BFCA362AEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "", "Should a record of this communication be saved to the recipient's profile", 2, @"False", "B90006F5-9B17-48DD-B455-5BAA2BE1A9A2" );

            RockMigrationHelper.AddBlockAttributeValue( "65091E04-77CE-411C-989F-EAD7D15778A0", "F3FBDD84-5E9B-40C2-B199-3FAE1C2308DC", @"9d00fee5-d805-441c-9f96-0582f894b5eb" ); // System Email

            #endregion

            #region Increase size of the Note column in the Reservation table
            Sql( @"
    ALTER TABLE[_com_bemaservices_RoomManagement_Reservation]
    ALTER COLUMN[Note] NVARCHAR( 2500 )
    " );

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}
