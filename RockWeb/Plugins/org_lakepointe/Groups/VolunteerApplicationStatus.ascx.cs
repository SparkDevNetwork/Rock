using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;



namespace RockWeb.Plugins.org_lakepointe.Groups
{
    [DisplayName( "Volunteer Application Status" )]
    [Category( "LPC > Groups" )]
    [Description( "Allows potential volunteers to see the requirements to serve in that role and take the next step." )]

    [TextField( "Group Type Purposes to Check", "A comma seperated list of group type purpose ids. Any group of a group type with these purposes will be checked", false, "184, 6766", "", 1, PURPOSE_LIST )]
    [IntegerField( "SWiM Application Connection Type", "A connection type id. This is the connection type that is used for SWiM volunteer applications.", false, 30, "", 2, APP_CONNECTION_TYPE )]
    public partial class VolunteerApplicationStatus : RockBlock
    {
        private RockContext _context;

        private const string PURPOSE_LIST = "PurposeList";
        private const string APP_CONNECTION_TYPE = "AppConnectionType";

        #region Properties

        private Dictionary<string, string> _groupRequirementStatuses;
        private List<int> _processedGroupIds;
        private bool isInServingTeam = false;

        #endregion
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _context = new RockContext();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                _groupRequirementStatuses = new Dictionary<string, string>();
                _processedGroupIds = new List<int>();
                if ( CurrentPerson != null )
                {
                    isInServingTeam = false;
                    ProcessAllGroups();
                }
                else
                {
                    lContent.Text = @"<div class=""alert alert-warning"" role=""alert"">
  <h4 style=""text-align: center;"" margin-bottom: 0;>You Must be Signed in to use This Block.</h4>
</div>";
                }
            }
        }

        #endregion  
        #region Events

        #endregion
        #region Methods

        private void ProcessAllGroups()
        {
            string purposeList = GetAttributeValue( PURPOSE_LIST );
            if ( purposeList == "" || purposeList == null )
            {
                return;
            }

            // Verify list contains only valid ints that could be an id to avoid SQL injection
            List<string> purposeStrings = purposeList.Split( ',' ).ToList();
            string safePurposeList = "";
            foreach ( string purposeString in purposeStrings )
            {
                int num = purposeString.AsInteger();
                if ( num > 0 )
                {
                    safePurposeList += $"{num}, ";
                }
            }
            safePurposeList = safePurposeList.ReplaceLastOccurrence( ", ", "" );

            GroupMemberService groupMemberService = new GroupMemberService( _context );
            string gmSql = $@"/* Query running from RockWeb.Plugins.org_lakepointe.Groups.VolunteerApplicationStatus.ascx.cs */
/* Query written by Jon Corey on 03/01/2022 */
/* Updated by Jon Corey on 3/20/2023 to remove verbose columns from select to allow for columns to change */
/* Purpose: Get all serving groups with requirements that the person is in. */
SELECT gm.*
	FROM [dbo].[GroupMember] gm
	JOIN [dbo].[Group] g ON g.Id = gm.GroupId
	JOIN [dbo].[GroupType] gt ON gt.Id = g.GroupTypeId
	WHERE gm.PersonId = {CurrentPerson.Id}
		AND gt.GroupTypePurposeValueId IN ({safePurposeList})
		AND g.IsActive = 1 AND g.IsArchived = 0
		AND gm.GroupMemberStatus != 0 AND gm.IsArchived = 0";
            List<GroupMember> servingGroupMembers = groupMemberService.ExecuteQuery( gmSql ).ToList();

            foreach ( GroupMember groupMember in servingGroupMembers )
            {
                _processedGroupIds.Add( groupMember.GroupId );
                ProcessRequirements(groupMember.Group, groupMember.GroupRoleId);
            }

            ConnectionRequestService connectionRequestService = new ConnectionRequestService( _context );
            string crSql = $@"/* Query running from RockWeb.Plugins.org_lakepointe.Groups.VolunteerApplicationStatus.ascx.cs */
/* Query written by Jon Corey on 03/01/2022 */
/* Updated by Jon Corey on 3/20/2023 to remove verbose columns from select to allow for columns to change */
/* Purpose: Get all connection requests where the person is the requester and the assigned groups is a serving groups with requirements. */
SELECT cr.*
	FROM dbo.[ConnectionRequest] cr
	JOIN [Group] g ON g.Id = cr.AssignedGroupId
	JOIN [dbo].[PersonAlias] pa ON pa.Id = cr.PersonAliasId
	JOIN [dbo].[GroupType] gt ON gt.Id = g.GroupTypeId
	WHERE pa.PersonId = {CurrentPerson.Id}
		AND cr.AssignedGroupMemberRoleId <> ''
		AND gt.GroupTypePurposeValueId IN ({safePurposeList})
		AND g.IsActive = 1 AND g.IsArchived = 0
		AND cr.ConnectionState = 0";
            List<ConnectionRequest> servingConnectionRequests = connectionRequestService.ExecuteQuery( crSql ).ToList();

            foreach ( ConnectionRequest connectionRequest in servingConnectionRequests )
            {
                if ( _processedGroupIds.Contains( connectionRequest.AssignedGroupId.ToIntSafe() ) == false )
                {
                    ProcessRequirements( connectionRequest.AssignedGroup, connectionRequest.AssignedGroupMemberRoleId );
                }
            }

            if ( lContent.Text == "" )
            {
                if ( isInServingTeam )
                {
                    lContent.Text = @"<div class=""alert alert-success"" role=""alert"">
  <h4 style=""text-align: center; margin-bottom: 0;"">You're all good!</h4>
  <hr style=""margin: 6px;"">
  <p>You've completed all the requirements for the positions you volunteer in and you don't have any requirements close to expiring!</p>
  <p><em>If you recently applied for a new volunteer position, it may not show up here right away.</em></p>
</div>";
                }
                else
                {
                    lContent.Text = @"<div class=""alert alert-info"" role=""alert"">
  <h4 style=""text-align: center; margin-bottom: 0;"">You haven't applied for any serving teams yet</h4>
  <hr style=""margin: 6px;"">
  <p><em>If you recently applied for a new volunteer position, it may not show up here right away.</em></p>
</div>";
                }
            }
        }

        private void ProcessRequirements(Group group, int? groupRoleId)
        {
            group.LoadAttributes();
            DateTime? eventDate = group.GetAttributeValue( "WheelsUpDate" ).AsDateTime();
            if ( eventDate == null )
            {
                eventDate = group.GetAttributeValue( "EventDate" ).AsDateTime();
            }

            if ( groupRoleId == null )
            {
                groupRoleId = group.GroupType.DefaultGroupRoleId;
            }
            var requirements = group.PersonMeetsGroupRequirements( _context, CurrentPerson.Id, groupRoleId );
            foreach ( var requirement in requirements )
            {
                string key = requirement.GroupRequirement.ToString().Split( '|' )[0];
                string value = requirement.MeetsGroupRequirement.ToString();

                // Format key to be understandable to layperson
                key = GetFriendlyRequirementName( key );

                // Check requirements
                if ( requirement.MeetsGroupRequirement.ToString() != "NotApplicable" )
                {
                    _groupRequirementStatuses[key] = value;
                    if ( value == "Meets" || value == "MeetsWithWarning" )
                    {
                        if ( eventDate != null && ( key.Contains( "Background Check" ) || key.Contains( "L3" ) || key.Contains( "L2" ) || key.Contains( "Level 3" ) || key.Contains( "Level 2" ) ) )
                        {
                            CurrentPerson.LoadAttributes();
                            DateTime? bgcExpireDate = CurrentPerson.GetAttributeValue( "BackgroundCheckExpireDate" ).AsDateTime();
                            if ( bgcExpireDate != null && DateTime.Compare( ( DateTime )eventDate, ( DateTime )bgcExpireDate ) > 0 )
                            {
                                _groupRequirementStatuses[key] = "EventNotMet";
                            }
                        }
                    }
                    else if ( key.Contains( "Background Check" ) || key.Contains( "L3" ) || key.Contains( "L2" ) || key.Contains( "Level 3" ) || key.Contains( "Level 2" ) )
                    {
                        List<ConnectionRequest> connectionRequests = GetConnectionRequests();
                        ConnectionRequest maxConnectionRequest = null;
                        int maxStatus = 0;
                        foreach ( ConnectionRequest connectionRequest in connectionRequests )
                        {
                            string statusString = connectionRequest.ConnectionStatus.Name;
                            int statusInt = statusString.Split( ' ' )[0].AsInteger();
                            if ( statusInt > maxStatus )
                            {
                                string groupName = "";
                                if ( connectionRequest.AssignedGroup != null )
                                {
                                    groupName = connectionRequest.AssignedGroup.Name;
                                }
                                bool isL3 = false;
                                if ( maxConnectionRequest != null )
                                {
                                    isL3 = maxConnectionRequest.AssignedGroup.Name.Contains( "Minor" );
                                }
                                if ( ( key.Contains( "L2" ) || key.Contains( "Level 2" ) )
                                    && (groupName.Contains( "Level 2" ) || groupName.Contains( "Minor" ) )
                                    && isL3 == false )
                                {
                                    maxConnectionRequest = connectionRequest;
                                    maxStatus = statusInt;
                                }
                                else if ( ( key.Contains( "L3" ) || key.Contains( "Level 3" ) )
                                    && groupName.Contains( "Minor" ) )
                                {
                                    maxConnectionRequest = connectionRequest;
                                    maxStatus = statusInt;
                                }
                                else if ( groupName == "" )
                                {
                                    maxConnectionRequest = connectionRequest;
                                    maxStatus = statusInt;
                                }
                            }
                        }
                        if ( maxStatus != 0 )
                        {
                            string status = "";
                            if ( maxStatus == 15 )
                            {
                                if ( maxConnectionRequest != null )
                                {
                                    
                                    CurrentPerson.LoadAttributes();
                                    string l3 = CurrentPerson.GetAttributeValue( "L3" );
                                    string l2 = CurrentPerson.GetAttributeValue( "L2" );
                                    DateTime? b1 = CurrentPerson.GetAttributeValue( "BackgroundCheckExpireDate" ).AsDateTime();
                                    DateTime? b2 = CurrentPerson.GetAttributeValue( "Arena-29-279" ).AsDateTime();
                                    DateTime? bgcExpire = b1 != null ? b1 : b2;
                                    DateTime baseline = new DateTime( 2019, 03, 01 );
                                    if ( b2 != null && bgcExpire != null && b2 >= baseline && bgcExpire > DateTime.Now ) // Check if person is a level N
                                    {
                                        status = "Application Sent";
                                    }
                                    else if ( l3 != null && l3 != "" ) // Check if person is already an L3
                                    {
                                        status = "Background Check Expired";
                                    }
                                    else if ( l2 != null && l2 != "" ) // Check if person is already an L2
                                    {
                                        status = "Background Check Expired";
                                    }
                                    else
                                    {
                                        status = $"Sent:{maxConnectionRequest.Guid}";
                                    }
                                }
                                else
                                {
                                    status = "Application Sent";
                                }
                            }
                            else if ( maxStatus == 40 )
                            {
                                status = "Background Check Sent";
                            }
                            else if ( maxStatus == 50 )
                            {
                                status = "Waiting on References";
                            }
                            else if ( maxStatus == 75 )
                            {
                                status = "Awaiting Interview";
                            }
                            else if ( maxStatus == 98 )
                            {
                                status = "Application Expired - Expect an email from Checkr";
                            }
                            else
                            {
                                status = "Application Pending";
                            }

                            if ( status != "" )
                            {
                                _groupRequirementStatuses[key] = status;
                            }
                        }
                    }
                }
            }

            isInServingTeam = true;
            DisplayResults( group.Name, eventDate );
        }

        private List<ConnectionRequest> GetConnectionRequests()
        {
            ConnectionRequestService connectionRequestService = new ConnectionRequestService( _context );
            List<ConnectionRequest> connectionRequests = connectionRequestService.ExecuteQuery( @"/* Query running from RockWeb.Plugins.org_lakepointe.Groups.VolunteerApplicationStatus.ascx.cs */
/* Query written by Jon Corey on 03/01/2022 */
/* Updated by Jon Corey on 3/20/2023 to remove verbose columns from select to allow for columns to change */
/* Purpose: Get all connection requests where the person is the requester and the opportunity's connection type is 'Volunteer Application Maintenance'. */
SELECT cr.*
	FROM [dbo].[ConnectionRequest] cr
	JOIN [dbo].[ConnectionOpportunity] co ON co.Id = cr.ConnectionOpportunityId
	JOIN [dbo].[PersonAlias] pa ON pa.Id = cr.PersonAliasId
	WHERE co.ConnectionTypeId = 30 AND pa.PersonId = {0}
		AND cr.ConnectionState IN (0, 2)", CurrentPerson.Id ).ToList();

            return connectionRequests;
        }

        private void DisplayResults( string title, DateTime? eventDate )
        {
            bool meetsAllRequirements = true;
            foreach ( var status in _groupRequirementStatuses )
            {
                if ( status.Value != "Meets" )
                {
                    meetsAllRequirements = false;
                }
            }
            // Hide position if all requirements are met or if the event is over
            if ( ( meetsAllRequirements == false && eventDate == null ) || eventDate > DateTime.Today.AddDays( -1 ) )
            {
                lContent.Text += $"<h4 style=\"margin-bottom: 0;\">{title}</h4>";
                if ( eventDate != null )
                {
                    lContent.Text += $"<h5 style=\"margin: 0 0 0 25px; font-style: italic;\">Event Date: {eventDate?.ToString( "MM/dd/yyyy" )}</h5>";
                }
                lContent.Text += "<ul style=\"list-style: none; padding-left: 0; margin-bottom: 25px;\">";
                foreach ( var status in _groupRequirementStatuses )
                {
                    if ( status.Value == "Meets" )
                    {
                        lContent.Text += $"<li><i class=\"fas fa-check-circle\" style=\"color: #5cb85c;\"></i> {status.Key}</li>";
                    }
                    else if ( status.Value == "MeetsWithWarning" )
                    {
                        lContent.Text += $"<li><i class=\"fas fa-minus-circle\" style=\"color: #f0ad4e;\"></i> {status.Key} (Expires Soon - Expect an email from Checkr)</li>";
                    }
                    else if ( status.Value == "EventNotMet" )
                    {
                        lContent.Text += $"<li><i class=\"fas fa-times-circle\" style=\"color: #d9534f;\"></i> {status.Key} (Expires Before Event - Expect an email from Checkr)</li>";
                    }
                    else if ( status.Value == "NotMet" )
                    {
                        if ( status.Key == "Volunteer Confidentiality Agreement" )
                        {
                            lContent.Text += $"<li><i class=\"fas fa-times-circle\" style=\"color: #d9534f;\"></i> {status.Key} (Expect an email from SignNow)</li>";
                        }
                        else if ( status.Key.Contains( "Ministry Safe" ) )
                        {
                            CurrentPerson.LoadAttributes();
                            string result = CurrentPerson.GetAttributeValue( "TrainingResult" );

                            if ( result == "Pending" )
                            {
                                lContent.Text += $"<li><i class=\"fas fa-minus-circle\" style=\"color: #f0ad4e;\"></i> {status.Key} (Expect an email with the subject \"MinistrySafe Awareness Training\")</li>";
                            }
                            else
                            {
                                lContent.Text += $"<li><i class=\"fas fa-times-circle\" style=\"color: #d9534f;\"></i> {status.Key} (Staff Notified)</li>";
                            }
                        }
                        else if ( status.Key.Contains( "Teaching Belief" ) )
                        {
                            GroupMemberService groupMemberService = new GroupMemberService( _context );
                            List<GroupMember> members = groupMemberService.GetByGroupIdAndPersonId( 824887, ( int )CurrentPersonId ).ToList();
                            bool isPending = false;
                            foreach ( GroupMember member in members )
                            {
                                if ( !( member.GroupMemberStatus == GroupMemberStatus.Inactive ) ) // If status is Pending or Active
                                {
                                    isPending = true;
                                }
                            }

                            if ( isPending )
                            {
                                lContent.Text += $"<li><i class=\"fas fa-minus-circle\" style=\"color: #f0ad4e;\"></i> {status.Key} (Pending Approval)</li>";
                            }
                            else
                            {
                                string rckipid = CurrentPerson.GetImpersonationToken( DateTime.Now.AddHours( 2 ), null, 2791 );
                                lContent.Text += $@"<li><i class=""fas fa-times-circle"" style=""color: #d9534f;""></i> {status.Key}
                                                        <ul style=""list-style: none;"">
                                                            <li><a href=""https://my.lakepointe.church/page/2791?rckipid={rckipid}"">Fill Out Teaching Belief Statement <i class=""fa fa-arrow-right""></i></a></li>
                                                        </ul>
                                                    </li>";
                            }
                        }
                        else if ( status.Key.Contains( "L3" ) || status.Key.Contains( "Level 3" )
                            || status.Key.Contains( "L2" ) || status.Key.Contains( "Level 2" )
                            || status.Key.Contains( "Background Check" ) )
                        {
                            CurrentPerson.LoadAttributes();
                            DateTime? bgcExpire = CurrentPerson.GetAttributeValue( "BackgroundCheckExpireDate" ).AsDateTime();
                            if ( bgcExpire < DateTime.Now || status.Key.Contains( "Background Check" ) )
                            {
                                lContent.Text += $"<li><i class=\"fas fa-times-circle\" style=\"color: #d9534f;\"></i> {status.Key} (Expect an email from Checkr)</li>";
                            }
                            else
                            {
                                lContent.Text += $"<li><i class=\"fas fa-times-circle\" style=\"color: #d9534f;\"></i> {status.Key}</li>";
                            }
                        }
                        else
                        {
                            lContent.Text += $"<li><i class=\"fas fa-times-circle\" style=\"color: #d9534f;\"></i> {status.Key}</li>";
                        }
                    }
                    else if ( status.Value == "Error" || status.Value == "NotApplicable" )
                    {
                        lContent.Text += $"<li><i class=\"fas fa-exclamation-circle\" style=\"color: #ee7624;\"></i> An unknown error has occurred while processing \"{status.Key}\"</li>";
                    }
                    else // SWiM Statuses
                    {
                        if ( status.Value.Contains( "Sent:" ) )
                        {
                            Guid connectionRequestGuid = status.Value.Split( ':' )[1].AsGuid();
                            if ( connectionRequestGuid != Guid.Empty )
                            {
                                lContent.Text += $@"<li><i class=""fas fa-minus-circle"" style=""color: #f0ad4e;""></i> {status.Key} (Application Sent)
                                                        <ul style=""list-style: none;"">
                                                            <li><a href=""https://my.lakepointe.church/page/2123?CR={connectionRequestGuid}"">Start Application <i class=""fa fa-arrow-right""></i></a></li>
                                                        </ul>
                                                    </li>";
                            }
                            else
                            {
                                lContent.Text += $"<li><i class=\"fas fa-minus-circle\" style=\"color: #f0ad4e;\"></i> {status.Key} ({status.Value})</li>";
                            }
                        }
                        else
                        {
                            lContent.Text += $"<li><i class=\"fas fa-minus-circle\" style=\"color: #f0ad4e;\"></i> {status.Key} ({status.Value})</li>";
                        }
                    }
                }
                lContent.Text += "</ul>";
            }
            _groupRequirementStatuses = new Dictionary<string, string>();
        }

        private string GetFriendlyRequirementName(string requirementName)
        {
            if ( requirementName == "L0 or Less Than 18" )
            {
                requirementName = "Has at Least a Level 0 Clearance or Younger than 18";
            }
            else if ( requirementName == "L2 or L3 for 18+" )
            {
                requirementName = "Has at Least a Level 2 Clearance or Younger than 18";
            }
            else if ( requirementName == "L3 for 18+" )
            {
                requirementName = "Has a Level 3 Clearance or Younger than 18";
            }
            else if ( requirementName == "L3 or LT with Grace Period" )
            {
                requirementName = "Has a Level 3 or Teen Clearance";
            }
            else if ( requirementName == "L3 with Grace for 18 year old LT" )
            {
                requirementName = "Has a Level 3 or Teen Clearance";
            }
            else if ( requirementName == "L3, LT, or LC with Grace Period" )
            {
                requirementName = "Has a Level 3, Teen, or Child Clearance";
            }
            else if ( requirementName == "Ministry Safe unless Pre-Teen" )
            {
                requirementName = "Has Completed Ministry Safe Training or Younger than 13";
            }
            else if ( requirementName == "United Camp Ministry Safe" )
            {
                requirementName = "Has Completed Ministry Safe Training Within 2 Years";
            }

            return requirementName;
        }

        #endregion
    }
}
