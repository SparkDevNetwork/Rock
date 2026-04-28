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
using System.Data.Entity;
using System.Linq;

using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Sends an event registration confirmation
    /// </summary>
    public sealed class ProcessSendRegistrationConfirmation : BusStartedTask<ProcessSendRegistrationConfirmation.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                /*
                     3/12/2026 - NA

                     Do not add AsNoTracking() to this query. The returned entities are used by a Lava
                     template during the merge/render process. The template may access related navigation
                     properties which are retrieved through lazy loading. Lazy loading requires the
                     DbContext to track the entity. If AsNoTracking() is added, the context cannot
                     retrieve those navigation properties and the Lava merge may fail.

                     A common error that appears when this occurs is:

                     "Lava Error: When an object is returned with a NoTracking merge option, Load can
                     only be called when the EntityCollection or EntityReference does not contain objects."

                     Reason: Lazy loading of navigation properties is required for the Lava merge process.
                */
                var registration = new RegistrationService( rockContext )
                    .Queryable( "RegistrationInstance.RegistrationTemplate" )
                    .FirstOrDefault( r => r.Id == message.RegistrationId );

                if ( registration != null &&
                    registration.RegistrationInstance != null &&
                    !string.IsNullOrEmpty( registration.ConfirmationEmail ) )
                {
                    var template = registration.RegistrationInstance.RegistrationTemplate;
                    if ( template != null && !string.IsNullOrWhiteSpace( template.ConfirmationEmailTemplate ) )
                    {
                        var currentPersonOverride = ( registration.RegistrationInstance.ContactPersonAlias != null ) ? registration.RegistrationInstance.ContactPersonAlias.Person : null;

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPersonOverride );
                        mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                        mergeFields.Add( "Registration", registration );

                        var emailMessage = new RockEmailMessage();
                        emailMessage.AddRecipient( registration.GetConfirmationRecipient( mergeFields ) );
                        emailMessage.AdditionalMergeFields = mergeFields;
                        emailMessage.FromEmail = template.ConfirmationFromEmail;
                        emailMessage.FromName = template.ConfirmationFromName;
                        emailMessage.Subject = template.ConfirmationSubject;
                        emailMessage.Message = template.ConfirmationEmailTemplate;
                        emailMessage.AppRoot = message.AppRoot;
                        emailMessage.ThemeRoot = message.ThemeRoot;
                        emailMessage.CurrentPerson = currentPersonOverride;
                        emailMessage.Send();
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
        }
    }
}