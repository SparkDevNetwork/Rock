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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{

    /// <summary>
    /// Job to process event registration reminders
    /// </summary>
    [DisplayName( "Registration Reminder" )]
    [Description( "Send any registration reminders that are due to be sent." )]

    [IntegerField( "Expire Date", "The number of days past the registration reminder to refrain from sending the email. This would only be used if something went wrong and acts like a safety net to prevent sending the reminder after the fact.", true, 1, key: "ExpireDate" )]
    public class SendRegistrationReminders : RockJob
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommunications"/> class.
        /// </summary>
        public SendRegistrationReminders()
        {
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            
            var expireDays = GetAttributeValue( "ExpireDate" ).AsIntegerOrNull() ?? 1;
            var publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );

            int remindersSent = 0;
            var errors = new List<string>();

            using ( var rockContext = new RockContext() )
            {
                DateTime now = RockDateTime.Now;
                DateTime expireDate = now.AddDays( expireDays * -1 );

                foreach ( var instance in new RegistrationInstanceService( rockContext )
                    .Queryable( "RegistrationTemplate,Registrations" )
                    .Where( i =>
                        i.IsActive &&
                        i.RegistrationTemplate.IsActive &&
                        i.RegistrationTemplate.ReminderEmailTemplate != string.Empty &&
                        !i.ReminderSent &&
                        i.SendReminderDateTime.HasValue &&
                        i.SendReminderDateTime <= now &&
                        i.SendReminderDateTime >= expireDate )
                    .ToList() )
                {
                    var template = instance.RegistrationTemplate;

                    foreach ( var registration in instance.Registrations
                        .Where( r =>
                            !r.IsTemporary &&
                            r.ConfirmationEmail != null &&
                            r.ConfirmationEmail != string.Empty ) )
                    {
                        try
                        {
                            var mergeFields = new Dictionary<string, object>();
                            mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                            mergeFields.Add( "Registration", registration );

                            var emailMessage = new RockEmailMessage();
                            emailMessage.AdditionalMergeFields = mergeFields;

                            emailMessage.AddRecipient( registration.GetConfirmationRecipient( mergeFields ) );

                            emailMessage.FromEmail = template.ReminderFromEmail;
                            emailMessage.FromName = template.ReminderFromName;
                            emailMessage.Subject = template.ReminderSubject;
                            emailMessage.AppRoot = publicAppRoot;
                            emailMessage.Message = template.ReminderEmailTemplate;

                            // LPC CODE
                            registration.PersonAlias.Person.LoadAttributes();
                            string lang = registration.PersonAlias.Person.GetAttributeTextValue( "PreferredLanguage" );
                            if ( lang == "Spanish" || lang == "Español" )
                            {
                                emailMessage.Message += "<style>.EnglishText { display: none !important; }</style>";
                            }
                            else
                            {
                                emailMessage.Message += "<style>.SpanishText { display: none !important; }</style>";
                            }
                            // END LPC CODE

                            var emailErrors = new List<string>();
                            emailMessage.Send( out emailErrors );
                            errors.AddRange( emailErrors );
                        }
                        catch ( Exception exception )
                        {
                            ExceptionLogService.LogException( exception );
                            continue;
                        }
                    }

                    // Even if an error occurs, still mark as completed to prevent _everyone_ being sent the reminder multiple times due to a single failing address


                    instance.SendReminderDateTime = now;
                    instance.ReminderSent = true;
                    remindersSent++;

                    rockContext.SaveChanges();
                }

                if ( remindersSent == 0 )
                {
                    this.Result = "No reminders to send";
                }
                else if ( remindersSent == 1 )
                {
                    this.Result = "1 reminder was sent";
                }
                else
                {
                    this.Result = string.Format( "{0} reminders were sent", remindersSent );
                }

                if ( errors.Any() )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine();
                    sb.Append( string.Format( "{0} Errors: ", errors.Count() ) );
                    errors.ForEach( e => { sb.AppendLine(); sb.Append( e ); } );
                    string errorMessage = sb.ToString();
                    this.Result += errorMessage;
                    var exception = new Exception( errorMessage );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( exception, context2 );
                    throw exception;
                }
            }
        }
    }
}