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
using System.Collections.Generic;
using System.Linq;

using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Runs a job now
    /// </summary>
    public class SendCommunicationApprovalEmail : ITransaction
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunicationApprovalEmail"/> class.
        /// </summary>
        public SendCommunicationApprovalEmail( )
        {
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var communication = new CommunicationService( rockContext ).Get( CommunicationId );

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

                            // create approval link if one was not provided
                            if ( string.IsNullOrEmpty( ApprovalPageUrl ) )
                            {
                                var internalApplicationRoot = GlobalAttributesCache.Value( "InternalApplicationRoot" ).EnsureTrailingForwardslash();
                                ApprovalPageUrl = $"{internalApplicationRoot}Communication/{communication.Id}";
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
                                mergeFields.Add( "ApprovalPageUrl", ApprovalPageUrl );
                                recipients.Add( new RockEmailMessageRecipient( approver.Person, mergeFields ) );
                                emailMessage.SetRecipients( recipients );
                                emailMessage.Send();
                            }
                        }
                    }
                }
            }
        }
    }
}