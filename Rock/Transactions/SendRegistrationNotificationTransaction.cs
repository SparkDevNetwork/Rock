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

                    var recipients = new Dictionary<string, Dictionary<string, object>>();

                    // Contact
                    if ( !string.IsNullOrWhiteSpace( registration.RegistrationInstance.ContactEmail ) &&
                        ( template.Notify & RegistrationNotify.RegistrationContact ) == RegistrationNotify.RegistrationContact )
                    {
                        recipients.AddOrIgnore( registration.RegistrationInstance.ContactEmail, mergeFields );
                    }

                    // Group Followers
                    if ( registration.GroupId.HasValue &&
                        ( template.Notify & RegistrationNotify.GroupFollowers ) == RegistrationNotify.GroupFollowers )
                    {
                        new GroupService( rockContext ).GetFollowers( registration.GroupId.Value )
                            .Where( f =>
                                f.Email != null &&
                                f.Email != "" )
                            .Select( f => f.Email )
                            .ToList()
                            .ForEach( f => recipients.AddOrIgnore( f, mergeFields ) );
                    }

                    // Group Leaders
                    if ( ( template.Notify & RegistrationNotify.GroupLeaders ) == RegistrationNotify.GroupLeaders )
                    {
                        new GroupMemberService( rockContext ).GetLeaders( registration.GroupId.Value )
                            .Where( m =>
                                m.Person != null &&
                                m.Person.Email != null &&
                                m.Person.Email != "" )
                            .Select( m => m.Person.Email )
                            .ToList()
                            .ForEach( m => recipients.AddOrIgnore( m, mergeFields ) );
                    }

                    if ( recipients.Any() )
                    {
                        var recipientList = new List<RecipientData>();
                        recipients.ToList().ForEach( r => recipientList.Add( new RecipientData( r.Key, r.Value ) ) );
                        Email.Send( Rock.SystemGuid.SystemEmail.REGISTRATION_NOTIFICATION.AsGuid(), recipientList, AppRoot, ThemeRoot );

                    }
                }
            }
        }
    }
}