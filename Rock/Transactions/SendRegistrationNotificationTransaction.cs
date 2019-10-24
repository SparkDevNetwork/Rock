﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;

using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Sends an email to group leaders whenever a new registration adds a registrant to their group
    /// </summary>
    public class SendRegistrationNotificationTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the communication identifier.
        /// </summary>
        /// <value>
        /// The communication identifier.
        /// </value>
        public int RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the application root.
        /// </summary>
        /// <value>
        /// The application root.
        /// </value>
        public string AppRoot { get; set; }

        /// <summary>
        /// Gets or sets the theme root.
        /// </summary>
        /// <value>
        /// The theme root.
        /// </value>
        public string ThemeRoot { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendRegistrationNotificationTransaction"/> class.
        /// </summary>
        public SendRegistrationNotificationTransaction()
        {
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var registration = new RegistrationService( rockContext )
                    .Queryable( "RegistrationInstance.RegistrationTemplate" ).AsNoTracking()
                    .FirstOrDefault( r => r.Id == RegistrationId );

                if ( registration != null && !string.IsNullOrEmpty( registration.ConfirmationEmail ) &&
                    registration.RegistrationInstance != null && registration.RegistrationInstance.RegistrationTemplate != null )
                {
                    var template = registration.RegistrationInstance.RegistrationTemplate;

                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                    mergeFields.Add( "Registration", registration );

                    var anonymousHash = new HashSet<string>();
                    var messageRecipients = new List<RockMessageRecipient>();

                    // Contact
                    if ( !string.IsNullOrWhiteSpace( registration.RegistrationInstance.ContactEmail ) &&
                        ( template.Notify & RegistrationNotify.RegistrationContact ) == RegistrationNotify.RegistrationContact )
                    {
                        var messageRecipient = registration.RegistrationInstance.GetContactRecipient( mergeFields );
                        if (!anonymousHash.Contains( messageRecipient.To ) )
                        {
                            messageRecipients.Add( messageRecipient );
                            anonymousHash.Add( messageRecipient.To );
                        }
                    }

                    // Group Followers
                    if ( registration.GroupId.HasValue &&
                        ( template.Notify & RegistrationNotify.GroupFollowers ) == RegistrationNotify.GroupFollowers )
                    {
                        new GroupService( rockContext ).GetFollowers( registration.GroupId.Value ).AsNoTracking()
                            .Where( p =>
                                p.Email != null &&
                                p.Email != "" )
                            .ToList()
                            .ForEach( p => messageRecipients.Add( new RockEmailMessageRecipient( p, mergeFields ) ) );
                    }

                    // Group Leaders
                    if ( registration.GroupId.HasValue &&
                        ( template.Notify & RegistrationNotify.GroupLeaders ) == RegistrationNotify.GroupLeaders )
                    {
                        new GroupMemberService( rockContext ).GetLeaders( registration.GroupId.Value )
                            .Where( m =>
                                m.Person != null &&
                                m.Person.Email != null &&
                                m.Person.Email != "" )
                            .Select( m => m.Person )
                            .ToList()
                            .ForEach( p => messageRecipients.Add( new RockEmailMessageRecipient( p, mergeFields ) ) );
                    }

                    if ( messageRecipients.Any() )
                    {
                        var emailMessage = new RockEmailMessage( Rock.SystemGuid.SystemEmail.REGISTRATION_NOTIFICATION.AsGuid() );
                        emailMessage.AdditionalMergeFields = mergeFields;
                        emailMessage.SetRecipients( messageRecipients );
                        emailMessage.AppRoot = AppRoot;
                        emailMessage.ThemeRoot = ThemeRoot;
                        emailMessage.Send();
                    }
                }
            }
        }
    }
}