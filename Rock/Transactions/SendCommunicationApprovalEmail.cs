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
using System.Linq;

using Rock.Communication;
using Rock.Data;
using Rock.Model;
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
                        string fromName = GlobalAttributesCache.Value("OrganizationName");
                        string fromEmail = GlobalAttributesCache.Value( "OrganizationEmail" );
                        string subject = "Pending Communication Requires Approval";
                        var appRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" );
                        string communicationDetails = string.Empty;
                        string typeName = communication.CommunicationType.ConvertToString();

                        // get custom details by type
                        switch ( communication.CommunicationType )
                        {
                            case CommunicationType.Email:
                                communicationDetails = $@"
                                        <strong>From Name:</strong> {communication.FromName}<br/>
                                        <strong>From Address:</strong> {communication.FromEmail}<br/>
                                        <strong>Subject:</strong> {communication.Subject}<br/>";
                                break;
                            case CommunicationType.SMS:
                                if ( communication.SMSFromDefinedValue != null )
                                {
                                    communicationDetails = $"<strong>SMS Number:</strong> {communication.SMSFromDefinedValue.Description} ({communication.SMSFromDefinedValue.Value})<br/>";
                                }
                                break;
                            case CommunicationType.PushNotification:
                                communicationDetails = $"<strong>Title:</strong> {communication.PushTitle}<br/>";
                                break;
                        }

                        // create approval link if one was not provided
                        if ( string.IsNullOrEmpty( ApprovalPageUrl ) )
                        {
                            var internalApplicationRoot = GlobalAttributesCache.Value( "InternalApplicationRoot" ).EnsureTrailingForwardslash();
                            ApprovalPageUrl = $"{internalApplicationRoot}Communication/{communication.Id}";
                        }

                        foreach ( var approver in approvers )
                        {
                            string message = string.Format( @"
                                    {{{{ 'Global' | Attribute:'EmailHeader' }}}}
                            
                                    <p>{0}:</p>

                                    <p>A new communication requires approval. Information about this communication can be found below.</p>

                                    <p>
                                        <strong>From:</strong> {1}<br />
                                        <strong>Type:</strong> {2}<br />
                                        {3}
                                        <strong>Recipient Count:</strong> {4}<br />
                                    </p>

                                    <p>
                                        <a href='{5}'>View Communication</a>
                                    </p>
    
                                    {{{{ 'Global' | Attribute:'EmailFooter' }}}}", 
                                                    approver.Person.NickName,
                                                    communication.SenderPersonAlias.Person.FullName,
                                                    typeName,
                                                    communicationDetails,
                                                    communication.GetRecipientsQry( rockContext ).Count(),
                                                    ApprovalPageUrl );

                            var emailMessage = new RockEmailMessage();
                            emailMessage.AddRecipient( approver.Person.Email );
                            emailMessage.FromEmail = fromEmail;
                            emailMessage.FromName = fromName;
                            emailMessage.Subject = subject;
                            emailMessage.Message = message;
                            emailMessage.AppRoot = appRoot;
                            emailMessage.Send();
                        }
                    }
                }
            }
        }
    }
}