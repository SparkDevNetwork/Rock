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
using System.Threading.Tasks;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.Tasks
{
    /// <summary>
    /// Sends a communication approval email
    /// </summary>
    public sealed class ProcessSendCommunicationApprovalEmail : BusStartedTask<ProcessSendCommunicationApprovalEmail.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var communication = new CommunicationService( rockContext ).Get( message.CommunicationId );

                if ( communication != null && communication.Status == CommunicationStatus.PendingApproval )
                {
                    // get notification group
                    var groupGuid = SystemGuid.Group.GROUP_COMMUNICATION_APPROVERS.AsGuid();
                    var approvers = new GroupMemberService( rockContext ).Queryable()
                        .Where( m =>
                            m.Group.Guid == groupGuid &&
                            m.GroupMemberStatus == GroupMemberStatus.Active )
                        .ToList();

                    if ( approvers.Any() )
                    {
                        var communicationSettingApprovalGuid = Rock.Web.SystemSettings.GetValue( SystemSetting.COMMUNICATION_SETTING_APPROVAL_TEMPLATE ).AsGuidOrNull();
                        if ( communicationSettingApprovalGuid.HasValue )
                        {
                            var approvalPageUrl = message.ApprovalPageUrl;

                            // create approval link if one was not provided
                            if ( string.IsNullOrEmpty( approvalPageUrl ) )
                            {
                                var internalApplicationRoot = GlobalAttributesCache.Value( "InternalApplicationRoot" ).EnsureTrailingForwardslash();
                                approvalPageUrl = $"{internalApplicationRoot}Communication/{communication.Id}";
                            }

                            foreach ( var approver in approvers )
                            {
                                var recipients = new List<RockEmailMessageRecipient>();
                                var emailMessage = new RockEmailMessage( communicationSettingApprovalGuid.Value );

                                // Build Lava merge fields.
                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                                mergeFields.Add( "Approver", approver.Person );
                                mergeFields.Add( "Communication", communication );
                                mergeFields.Add( "RecipientsCount", communication.GetRecipientsQry( rockContext ).Count() );
                                mergeFields.Add( "ApprovalPageUrl", approvalPageUrl );
                                recipients.Add( new RockEmailMessageRecipient( approver.Person, mergeFields ) );
                                emailMessage.SetRecipients( recipients );
                                emailMessage.Send();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the communication identifier.
            /// </summary>
            /// <value>
            /// The communication identifier.
            /// </value>
            public int CommunicationId { get; set; }

            /// <summary>
            /// Gets or sets the approval page URL. Defaults to ~/Communication/{communicationId}.
            /// </summary>
            /// <value>
            /// The approval page URL.
            /// </value>
            public string ApprovalPageUrl { get; set; } = null;
        }
    }
}