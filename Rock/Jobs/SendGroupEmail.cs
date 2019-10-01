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
using System.Linq;
using System.Text;
using System.Web;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job will send a specified email template to all active group members of the specified group, with the option to also send it to members of descendant groups. If a person is a member of multiple groups in the tree they will receive an email for each group.
    /// </summary>
    [SystemEmailField( "System Email", "The email template that will be sent.", true, "" )]
    [GroupField( "Group", "The group the email will be sent to." )]
    [BooleanField( "Send To Descendant Groups", "Determines if the email will be sent to descendant groups." )]
    [DisallowConcurrentExecution]
    public class SendGroupEmail : IJob
    {
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
            var emailTemplateGuid = dataMap.Get( "SystemEmail" ).ToString().AsGuid();
            var groupGuid = dataMap.Get( "Group" ).ToString().AsGuid();
            var sendToDescendants = dataMap.Get( "SendToDescendantGroups" ).ToString().AsBoolean();

            var rockContext = new RockContext();
            var group = new GroupService( rockContext ).Get( groupGuid );
            if ( group != null )
            {
                List<int> groupIds = new List<int>();
                GetGroupIds( groupIds, sendToDescendants, group );

                var recipients = new List<RecipientData>();

                var groupMemberList = new GroupMemberService( rockContext ).Queryable().Where( gm =>
                    groupIds.Contains( gm.GroupId ) &&
                    gm.GroupMemberStatus == GroupMemberStatus.Active )
                    .ToList();
                foreach ( GroupMember groupMember in groupMemberList )
                {
                    var person = groupMember.Person;
                    if ( !person.IsEmailActive || person.Email.IsNullOrWhiteSpace() || person.EmailPreference == EmailPreference.DoNotEmail )
                    {
                        continue;
                    }

                    var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Person", person );
                    mergeFields.Add( "GroupMember", groupMember );
                    mergeFields.Add( "Group", groupMember.Group );

                    recipients.Add( new RecipientData( groupMember.Person.Email, mergeFields ) );
                }

                var errors = new List<string>();
                if ( recipients.Any() )
                {
                    var emailMessage = new RockEmailMessage( emailTemplateGuid );
                    emailMessage.SetRecipients( recipients );
                    emailMessage.Send( out errors );

                }

                context.Result = string.Format( "{0} emails sent", recipients.Count() );

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