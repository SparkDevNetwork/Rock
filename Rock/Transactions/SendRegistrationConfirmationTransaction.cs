using System.Collections.Generic;
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
using System.Data.Entity;
using System.Linq;

using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Sends an event registration confirmation
    /// </summary>
    public class SendRegistrationConfirmationTransaction : ITransaction
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
        public string ThemeRoot {get;set;}

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunicationApprovalEmail"/> class.
        /// </summary>
        public SendRegistrationConfirmationTransaction()
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

                if ( registration != null && 
                    registration.RegistrationInstance != null && 
                    !string.IsNullOrEmpty( registration.ConfirmationEmail ) )
                {
                    var template = registration.RegistrationInstance.RegistrationTemplate;
                    if ( template != null && !string.IsNullOrWhiteSpace( template.ConfirmationEmailTemplate ) )
                    {
                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                        mergeFields.Add( "Registration", registration );

                        string from = template.ConfirmationFromName.ResolveMergeFields( mergeFields );
                        string fromName = template.ConfirmationFromEmail.ResolveMergeFields( mergeFields );
                        string subject = template.ConfirmationSubject.ResolveMergeFields( mergeFields );
                        string message = template.ConfirmationEmailTemplate.ResolveMergeFields( mergeFields );

                        var recipients = new List<string> { registration.ConfirmationEmail };
                        Email.Send( from, fromName, subject, recipients, message, AppRoot, ThemeRoot );
                    }
                }
            }
        }
    }
}