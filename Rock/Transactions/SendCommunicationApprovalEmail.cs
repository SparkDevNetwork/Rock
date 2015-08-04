// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

using Rock.Data;
using Rock.Model;
using Rock.Jobs;
using Rock.Communication;

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
        /// Gets or sets the approval page URL.
        /// </summary>
        /// <value>
        /// The approval page URL.
        /// </value>
        public string ApprovalPageUrl { get; set; }

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
                    var approvers = new GroupService( rockContext ).Get(SystemGuid.Group.GROUP_COMMUNICATION_APPROVERS.AsGuid());

                    if ( approvers != null )
                    {
                        var mergeFields = new Dictionary<string, object>();
                
                        var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );
                        globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

                        string fromName = Rock.Web.Cache.GlobalAttributesCache.Value("OrganizationName");
                        string fromEmail = Rock.Web.Cache.GlobalAttributesCache.Value( "OrganizationEmail" );
                        string subject = "Pending Communication Requires Approval";
                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                        string communicationDetails = string.Empty;
                        string typeName = string.Empty;

                        // get custom details by type
                        switch ( communication.Medium.TypeName )
                        {
                            case "Rock.Communication.Medium.Email":
                                string emailFromName = communication.GetMediumDataValue( "FromName" );
                                string emailFromAddress = communication.GetMediumDataValue( "FromAddress" );
                                communicationDetails = string.Format( @"
                                        <strong>From Name:</strong> {0}<br/>
                                        <strong>From Address:</strong> {1}<br/>
                                        <strong>Subject:</strong> {2}<br/>"
                                            , emailFromName
                                            , emailFromAddress
                                            , communication.Subject );
                                typeName = "Email";
                                break;
                            case "Rock.Communication.Medium.Sms":
                                int fromValueId = communication.GetMediumDataValue( "FromValue" ).AsInteger();
                                var fromValue = new DefinedValueService( rockContext ).Get( fromValueId );
                                typeName = "SMS";

                                if ( fromValue != null )
                                {
                                    communicationDetails = string.Format( "<strong>SMS Number:</strong> {0} ({1})<br/>", fromValue.Description, fromValue.Value );
                                }
                                break;
                        }

                        // create approval link if one was not provided
                        if ( ApprovalPageUrl == null )
                        {
                            ApprovalPageUrl = string.Format( "{0}Communication/{1}", Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" ), communication.Id );
                        }
                        

                        foreach ( var approver in approvers.Members )
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
                                                    communication.Recipients.Count(),
                                                    ApprovalPageUrl);
                            
                            var recipients = new List<string>();
                            recipients.Add( approver.Person.Email );

                            Email.Send( fromEmail, fromName, subject, recipients, message.ResolveMergeFields( mergeFields ), appRoot, string.Empty, null, false );
                        }
                    }
                }
            }
        }
    }
}