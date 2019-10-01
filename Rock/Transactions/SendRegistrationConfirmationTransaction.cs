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
                        var currentPersonOverride = ( registration.RegistrationInstance.ContactPersonAlias != null ) ? registration.RegistrationInstance.ContactPersonAlias.Person : null;

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPersonOverride );
                        mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                        mergeFields.Add( "Registration", registration );

                        var emailMessage = new RockEmailMessage();
                        emailMessage.AddRecipient( new RecipientData( registration.ConfirmationEmail, mergeFields ) );
                        emailMessage.AdditionalMergeFields = mergeFields;
                        emailMessage.FromEmail = template.ConfirmationFromEmail;
                        emailMessage.FromName = template.ConfirmationFromName;
                        emailMessage.Subject = template.ConfirmationSubject;
                        emailMessage.Message = template.ConfirmationEmailTemplate;
                        emailMessage.AppRoot = AppRoot;
                        emailMessage.ThemeRoot = ThemeRoot;
                        emailMessage.CurrentPerson = currentPersonOverride;
                        emailMessage.Send();
                    }
                }
            }
        }
    }
}