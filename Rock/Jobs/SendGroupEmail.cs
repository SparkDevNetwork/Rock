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
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Logging;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will send a specified email template to all active group members of the specified group, with the option to also send it to members of descendant groups. If a person is a member of multiple groups in the tree they will receive an email for each group.
    /// </summary>
    [DisplayName( "Send Group Email" )]
    [Description( "This job will send a specified email template to all active group members of the specified group, with the option to also send it to members of descendant groups. If a person is a member of multiple groups in the tree they will receive an email for each group." )]

    [SystemCommunicationField( "System Communication",
        Description = "The email template that will be sent.",
        IsRequired = true,
        Key = AttributeKey.SystemCommunication )]
    [GroupField( "Group",
        Description = "The group the email will be sent to.",
        Key = AttributeKey.Group )]
    [BooleanField( "Send To Descendant Groups",
        Description = "Determines if the email will be sent to descendant groups.",
        Key = AttributeKey.SendToDescendantGroups )]
    [DisallowConcurrentExecution]
    public class SendGroupEmail : IJob
    {
        private class AttributeKey
        {
            public const string SystemCommunication = "SystemEmail";
            public const string Group = "Group";
            public const string SendToDescendantGroups = "SendToDescendantGroups";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGroupEmail"/> class.
        /// </summary>
        public SendGroupEmail()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var emailTemplateGuid = dataMap.Get( AttributeKey.SystemCommunication ).ToString().AsGuid();
            var groupGuid = dataMap.Get( AttributeKey.Group ).ToString().AsGuid();
            var sendToDescendants = dataMap.Get( AttributeKey.SendToDescendantGroups ).ToString().AsBoolean();

            var rockContext = new RockContext();
            var systemCommunication = new SystemCommunicationService( rockContext ).Get( emailTemplateGuid );

            var group = new GroupService( rockContext ).Get( groupGuid );
            if ( group != null )
            {
                var groupIds = new List<int>();
                GetGroupIds( groupIds, sendToDescendants, group );

                var recipients = new List<RockEmailMessageRecipient>();

                var groupMemberList = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( gm => groupIds.Contains( gm.GroupId ) )
                    .Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active )
                    .ToList();

                var errors = new List<string>();
                var warnings = new List<string>();
                var messagesSent = 0;

                foreach ( GroupMember groupMember in groupMemberList )
                {
                    var person = groupMember.Person;

                    var mediumType = Rock.Model.Communication.DetermineMediumEntityTypeId(
                                    ( int ) CommunicationType.Email,
                                    ( int ) CommunicationType.SMS,
                                    ( int ) CommunicationType.PushNotification,
                                    groupMember.CommunicationPreference,
                                    person.CommunicationPreference );

                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Person", person );
                    mergeFields.Add( "GroupMember", groupMember );
                    mergeFields.Add( "Group", groupMember.Group );

                    var sendMessageResults = CommunicationHelper.SendMessage( person, mediumType, systemCommunication, mergeFields );
                    errors.AddRange( sendMessageResults.Errors );
                    warnings.AddRange( sendMessageResults.Warnings );
                    messagesSent += sendMessageResults.MessagesSent;
                }

                var jobResults = new StringBuilder( $"{messagesSent} messages were sent." );
                if ( warnings.Any() )
                {
                    jobResults.AppendLine();
                    jobResults.AppendLine( $"{warnings.Count} warnings:" );
                    warnings.ForEach( w => { jobResults.AppendLine( w ); } );
                }

                context.Result = jobResults.ToString();
                if ( errors.Any() )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( string.Format( "{0} Errors: ", errors.Count() ) );
                    errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errorMessage = sb.ToString();
                    context.Result += errorMessage;
                    var exception = new Exception( errorMessage );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( exception, context2 );
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Gets the group ids.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="sendToDescendants">if set to <c>true</c> [send to descendants].</param>
        /// <param name="group">The group.</param>
        private void GetGroupIds( List<int> groupIds, bool sendToDescendants, Group group )
        {
            groupIds.Add( group.Id );

            if ( sendToDescendants )
            {
                foreach ( var childGroup in group.Groups )
                {
                    GetGroupIds( groupIds, sendToDescendants, childGroup );
                }
            }
        }
    }
}