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
using System.Web;

using Humanizer;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Sends an email template using the Email communication medium
    /// </summary>
    public static class Email
    {

        /// <summary>
        /// Processes the bounce.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="bounceType">Type of the bounce.</param>
        /// <param name="message">The message.</param>
        /// <param name="bouncedDateTime">The bounced date time.</param>
        public static void ProcessBounce( string email, BounceType bounceType, string message, DateTime bouncedDateTime )
        {
            // currently only processing hard bounces
            if ( bounceType != BounceType.HardBounce )
            {
                return;
            }

            string bounceMessage = message.IsNotNullOrWhiteSpace() ? $" ({message})" : "";

            using ( var rockContext = new RockContext() )
            {
                // get people who have those emails
                var personService = new PersonService( rockContext );
                var peopleWithEmail = personService.GetByEmail( email ).Select( p => p.Id ).ToList();

                foreach ( int personId in peopleWithEmail )
                {
                    personService = new PersonService( rockContext );
                    var person = personService.Get( personId );

                    if ( person.IsEmailActive == true )
                    {
                        person.IsEmailActive = false;
                    }

                    person.EmailNote = GetEmailNote( bounceType, message, bouncedDateTime );
                    rockContext.SaveChanges();
                }
            }
        }

        private static string GetEmailNote( BounceType bounceType, string message, DateTime bouncedDateTime )
        {
            var messages = new Dictionary<string, string>
            {
                {"550", "The user's mailbox was unavailable or could not be found." },
                {"551", "The intended mailbox does not exist on this recipient server." },
                {"552", "This message is larger than the current system limit or the recipient's mailbox is full." },
                {"553", "The message was refused because the mailbox name is either malformed or does not exist." },
                {"554", "Email refused." },
            };

            var emailNote = message;
            foreach ( string key in messages.Keys )
            {
                if ( message.StartsWith( key ) )
                {
                    emailNote = messages[key];
                    break;
                }
            }

            emailNote = $"Email experienced a {bounceType.Humanize()} on {bouncedDateTime.ToShortDateString()}. {emailNote}";
            return emailNote.SubstringSafe( 0, 250 );
        }

        /// <summary>
        /// Notifies the admins.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="createCommunicationHistory">if set to <c>true</c> [create communication history].</param>
        /// <exception cref="System.Exception">Error sending System Email: Could not read Email Medium Entity Type</exception>
        public static void NotifyAdmins( string subject, string message, string appRoot = "", string themeRoot = "", bool createCommunicationHistory = true )
        {
            try
            {
                List<Person> personList = null;

                Guid adminGroup = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();

                using ( var rockContext = new RockContext() )
                {
                    personList = new GroupMemberService( rockContext ).Queryable()
                        .Where( m =>
                            m.Group.Guid.Equals( adminGroup ) &&
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.Person.Email != null &&
                            m.Person.Email != "" )
                        .Select( m => m.Person )
                        .ToList();
                }

                var errorMessages = new List<string>();

                var recipients = personList.Select( a => new RockEmailMessageRecipient( a, null ) ).ToList();

                var emailMessage = new RockEmailMessage();
                emailMessage.FromEmail = GlobalAttributesCache.Value( "OrganizationEmail" );
                emailMessage.Subject = subject;
                emailMessage.SetRecipients( recipients );
                emailMessage.Message = message;
                emailMessage.ThemeRoot = themeRoot;
                emailMessage.AppRoot = appRoot;
                emailMessage.CreateCommunicationRecord = createCommunicationHistory;
                emailMessage.Send( out errorMessages );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum BounceType
    {

        /// <summary>
        /// The hard bounce
        /// </summary>
        HardBounce = 1,

        /// <summary>
        /// The soft bounce
        /// </summary>
        SoftBounce = 2
    };

}
