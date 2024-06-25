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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;
using Rock;
using System.Data.Entity;
using System.Text;

namespace org.lakepointe.Connections.Workflow.Action
{
    /// <summary>
    /// Creates a new connection request.
    /// </summary>
    [ActionCategory( "LPC Connections" )]
    [Description( "Creates a new Next Steps connection request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Next Step Connection Request Create" )]

    [WorkflowAttribute("Person Attribute",
        Description = "The Person attribute that contains the person that connection request should be created for.",
        IsRequired = true,
        Order = 3,
        Key = "PersonAttribute",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute( "Age",
        Description = "A text attribute that lists the ages of people on the connection card.",
        IsRequired = false,
        Order = 4,
        Key = "Age",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute( "Is New Here",
        Description = "A multi-select attribute that indicates whether the person checked new here.",
        IsRequired = false,
        Order = 5,
        Key = "IsNewHere",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.SelectMultiFieldType" } )]

    [WorkflowAttribute( "Decision",
        Description = "A multi-select attribute that indicates the decisions the person checked.",
        IsRequired = false,
        Order = 6,
        Key = "Decision",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.SelectMultiFieldType" } )]

    [BooleanField("Create Connection Requests for Next Step Class",
        Description = "Enable Next Step Class connection requests to be created.",
        IsRequired = true,
        Order = 6,
        Key = "NextStepClassCR",
        DefaultBooleanValue = false)]

    [WorkflowAttribute( "Other",
        Description = "Text attribute that contains Other information the person may have provided.",
        IsRequired = false,
        Order = 7,
        Key = "Other",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute( "Prayer",
        Description = "Memo attribute that contains a Prayer Request.",
        IsRequired = false,
        Order = 8,
        Key = "Prayer",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.MemoFieldType" } )]

    [WorkflowAttribute( "Campus Attribute",
        Description = "An attribute that contains the campus to use for the request.",
        IsRequired = true,
        Order = 11,
        Key = "CampusAttribute",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.CampusFieldType" } )]

    [WorkflowAttribute( "Connection Comment Attribute",
        Description = "An optional attribute that contains the comment to use for the request.",
        IsRequired = false,
        Order = 12,
        Key = "ConnectionCommentAttribute",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    [WorkflowAttribute( "Connection Request Attribute",
        Description = "An optional connection request attribute to store the request that is created.",
        IsRequired = false,
        Order = 13,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.ConnectionRequestFieldType" } )]

    [WorkflowAttribute( "Is Host Team",
        Description = "A multi-select attribute that indicates whether the form was flagged as coming form the Host Team.",
        IsRequired = false,
        Order = 14,
        Key = "IsHostTeam",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.SelectMultiFieldType" } )]

    public class CreateNextStepsConnectionRequest : ActionComponent
    {
        private static Guid NewHere = "9A06B4E8-AF91-427F-AB13-943D4E31F8D3".AsGuid();
        private static Guid NextStep = "185BE962-B923-4405-A06B-D432060AFB25".AsGuid();
        private static Guid Commit = "18D9A8C4-0A13-4BE2-87FF-BEDE3759968F".AsGuid();
        private static Guid Baptism = "D53F434A-FF76-481C-99B3-70A7F02FE632".AsGuid();
        private static Guid LifeGroup = "BBC792B2-E863-4A51-BE74-BDCFFCC5D17F".AsGuid();
        private static Guid Prayer = "45775210-F905-4389-A7A6-59EF2E60F35A".AsGuid();
        private static Guid MultipleBoxes = "456FBBAC-9FEF-4F95-A4FA-BCA61002779A".AsGuid();
        private static Guid NoBoxes = "A1A2CD10-97DE-42F3-B99D-242EEDB585B7".AsGuid();
        private static Guid HostTeam = "C92695E7-5BB8-436B-90C4-DC45F35D6DF1".AsGuid();

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get the person
            PersonAlias personAlias = null;
            Guid personAliasGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "PersonAttribute" ).AsGuid() ).AsGuid();
            personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid );
            if ( personAlias == null )
            {
                errorMessages.Add( "Invalid Person Attribute or Value!" );
                return false;
            }

            // get campus
            Campus campus = null;
            Guid campusIdGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "CampusAttribute" ).AsGuid() ).AsGuid();
            campus = new CampusService( rockContext ).Get( campusIdGuid );
            if ( campus == null )
            {
                errorMessages.Add( "Invalid Campus Attribute or Value!" );
                return false;
            }

            // Get the Comment
            var comment = new StringBuilder();
            comment.Append( action.GetWorkflowAttributeValue( GetAttributeValue( action, "ConnectionCommentAttribute" ).AsGuid() ) );

            // Create the opportunity
            string attributes = null;
            ConnectionOpportunity opportunity = CreateOpportunity( rockContext, action, personAlias, campus.Id, comment, out attributes );
            if ( opportunity == null )
            {
                errorMessages.Add( "Invalid Connection Opportunity Attribute or Value!" );
                return false;
            }

            // Get connection status
            ConnectionStatus status = opportunity.ConnectionType.ConnectionStatuses
                .Where( s => s.Name.Equals( "Web Request" ) )
                .FirstOrDefault();
            if ( status == null )
            {
                status = opportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .FirstOrDefault();
            }

            // figure out which group to assign them to
            int? targetGroup = new ConnectionOpportunityGroupService( rockContext ).Queryable( "Group" ).AsNoTracking()
                .Where( g =>
                    g.ConnectionOpportunityId == opportunity.Id &&
                    g.Group.CampusId == campus.Id )
                .FirstOrDefault()?.Group.Id;

            int? groupRole = null;
            if ( targetGroup.HasValue )
            {
                // And their role in that group
                groupRole = new GroupService( rockContext ).Queryable().AsNoTracking()
                    .Where( g => g.Id == targetGroup )
                    .First().GroupType.Roles.
                        Where( r => r.Name == "Member" ).FirstOrDefault()?.Id;
            }

            var connectionRequest = new ConnectionRequest();
            connectionRequest.PersonAliasId = personAlias.Id;
            connectionRequest.ConnectionOpportunityId = opportunity.Id;
            connectionRequest.ConnectionState = ConnectionState.Active;
            connectionRequest.ConnectionStatusId = status.Id;
            connectionRequest.CampusId = campus.Id;
            connectionRequest.ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campus.Id );
            connectionRequest.Comments = comment.ToString();
            connectionRequest.AssignedGroupId = targetGroup;
            connectionRequest.AssignedGroupMemberRoleId = groupRole;
            connectionRequest.AssignedGroupMemberStatus = GroupMemberStatus.Active;
            connectionRequest.AssignedGroupMemberAttributeValues = attributes;

            var connectionRequestService = new ConnectionRequestService( rockContext );
            connectionRequestService.Add( connectionRequest );
            rockContext.SaveChanges();

            // If request attribute was specified, requery the request and set the attribute's value
            Guid? connectionRequestAttributeGuid = GetAttributeValue( action, "ConnectionRequestAttribute" ).AsGuidOrNull();
            if ( connectionRequestAttributeGuid.HasValue )
            {
                connectionRequest = connectionRequestService.Get( connectionRequest.Id );
                if ( connectionRequest != null )
                {
                    SetWorkflowAttributeValue( action, connectionRequestAttributeGuid.Value, connectionRequest.Guid.ToString() );
                }
            }

            return true;
        }

        private ConnectionOpportunity CreateOpportunity( RockContext rockContext, WorkflowAction action, PersonAlias personAlias, int? campusId, StringBuilder comment,
            out string attributes )
        {
            // Get the boxes checked, data provided
            string newHere = GetTextAttribute( action, "IsNewHere" );
            bool isNewHere = newHere.Split( ',' ).Any( t => t.CompareTo( "1" ) == 0 );

            string hostTEam = GetTextAttribute(action, "IsHostTeam");
            bool isHostTeam = hostTEam.Split(',').Any(t => t.CompareTo("1") == 0);

            string decision = GetTextAttribute( action, "Decision" );
            var decisions = decision.Split( ',' );
            bool isNextStep = decisions.Any( t => t.CompareTo( "1" ) == 0 );
            bool isCommit = decisions.Any( t => t.CompareTo( "2" ) == 0 );
            bool isBaptism = decisions.Any( t => t.CompareTo( "3" ) == 0 );
            bool isLifeGroup = decisions.Any( t => t.CompareTo( "4" ) == 0 );

            string other = GetTextAttribute( action, "Other" );
            string prayer = GetTextAttribute( action, "Prayer" );
            string age = GetTextAttribute( action, "Age" );

            int boxCount = 0;
            if ( isNewHere ) boxCount++;
            if ( GetAttributeValue(action, "NextStepClassCR").AsBooleanOrNull() == true )
            {
                if ( isNextStep ) boxCount++;
            }
            if ( isCommit ) boxCount++;
            if ( isBaptism ) boxCount++;
            if ( isLifeGroup ) boxCount++;
            if ( !prayer.IsNullOrWhiteSpace() ) boxCount++;
            if ( !other.IsNullOrWhiteSpace() )
            {
                boxCount++;
                comment.Append( "<br/>Other: " ).Append( other );
            }

            var avs = new AttributeValueService( rockContext );
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            ConnectionOpportunity opportunity = null;
            attributes = string.Empty;

            if ( isHostTeam )
            {
                opportunity = connectionOpportunityService.Get( HostTeam );
                attributes = new
                {
                    Ages = age,
                    AttendNextStepClass = isNextStep,
                    CommitmylifetoChrist = isCommit,
                    Bebaptized = isBaptism,
                    ConnectwithaLifeGroup = isLifeGroup,
                    ImNewHere = isNewHere,
                    PrayerRequest = prayer
                }.ToJson();

                comment.Append("<br/>(A Host Team member managed this contact.)");
            }
            else if ( boxCount > 1 ) // multiple boxes checked
            {
                // determine the opportunity based on data provided
                opportunity = connectionOpportunityService.Get( MultipleBoxes );

                // create the attributes and assign the attributes to the group member
                attributes = new 
                {
                    Ages = age,
                    AttendNextStepClass = isNextStep,
                    CommitmylifetoChrist = isCommit,
                    Bebaptized = isBaptism,
                    ConnectwithaLifeGroup = isLifeGroup,
                    ImNewHere = isNewHere,
                    PrayerRequest = prayer
                }.ToJson();
            }
            else if ( boxCount == 0 ) // no boxes checked
            {
                opportunity = connectionOpportunityService.Get( NoBoxes );
                attributes = new { Ages = age }.ToJson();
            }
            else // one box checked
            {
                if ( isNewHere )
                {
                    opportunity = connectionOpportunityService.Get( NewHere );
                    attributes = new { Ages = age }.ToJson();
                }

                if ( GetAttributeValue(action, "NextStepClassCR").AsBooleanOrNull() == true )
                {
                    if ( isNextStep )
                    {
                        opportunity = connectionOpportunityService.Get(NextStep);
                        attributes = new { Ages = age }.ToJson();
                    }
                }

                if ( isCommit )
                {
                    opportunity = connectionOpportunityService.Get( Commit );
                    attributes = new { Ages = age }.ToJson();
                }

                if ( isBaptism )
                {
                    opportunity = connectionOpportunityService.Get( Baptism );
                    attributes = new { Ages = age }.ToJson();
                }

                if ( isLifeGroup )
                {
                    opportunity = connectionOpportunityService.Get( LifeGroup );
                    attributes = new { Ages = age }.ToJson();
                }

                if ( !other.IsNullOrWhiteSpace() )
                {
                    opportunity = connectionOpportunityService.Get( NoBoxes );
                    attributes = new { Ages = age }.ToJson();
                }

                if ( !prayer.IsNullOrWhiteSpace() )
                {
                    opportunity = connectionOpportunityService.Get( Prayer );
                    attributes = new
                    {
                        Ages = age,
                        PrayerRequest = prayer
                    }.ToJson();
                }
            }

            return opportunity;
        }

        private string GetTextAttribute( WorkflowAction action, string attributeKey )
        {
            string result = null;

            result = GetAttributeValue( action, attributeKey );
            Guid guid = result.AsGuid();
            if ( guid.IsEmpty() )
            {
                result = result.ResolveMergeFields( GetMergeFields( action ) );
            }
            else
            {
                var workflowAttributeValue = action.GetWorkflowAttributeValue( guid );

                if ( workflowAttributeValue != null )
                {
                    result = workflowAttributeValue;
                }
            }
            return result;
        }
    }
}