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
using System.Net.Mail;
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
            if ( bounceType == BounceType.HardBounce )
            {
                // get people who have those emails

                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );

                var peopleWithEmail = personService.GetByEmail( email );

                foreach ( var person in peopleWithEmail )
                {
                    person.IsEmailActive = false;

                    person.EmailNote = String.Format( "Email experienced a {0} on {1} ({2}).", bounceType.Humanize(), bouncedDateTime.ToShortDateString(), message );
                }

                rockContext.SaveChanges();
            }
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
        public static void NotifyAdmins( string subject, string message, string appRoot = "", string themeRoot = "", bool createCommunicationHistory = true  )
        {
            try
            {
                List<string> recipients = null;

                Guid adminGroup = Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid();

                using ( var rockContext = new RockContext() )
                {
                    recipients = new GroupMemberService( rockContext ).Queryable()
                        .Where( m =>
                            m.Group.Guid.Equals( adminGroup ) &&
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.Person.Email != null &&
                            m.Person.Email != "" )
                        .Select( m => m.Person.Email )
                        .ToList();
                }

                var errorMessages = new List<string>();

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

        #region Obsolete

        /// <summary>
        /// Sends the specified email template unique identifier.
        /// </summary>
        /// <param name="emailTemplateGuid">The email template unique identifier.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="createCommunicationHistory">if set to <c>true</c> [create communication history].</param>
        /// <exception cref="System.Exception">
        /// Error sending System Email: Could not read Email Medium Entity Type
        /// </exception>
        [Obsolete( "Use the RockEmailMessage object and its Send() method to send emails." )]
        public static void Send( Guid emailTemplateGuid, List<RecipientData> recipients, string appRoot = "", string themeRoot = "", bool createCommunicationHistory = true )
        {
            using ( var rockContext = new RockContext() )
            {
                var template = new SystemEmailService( rockContext ).Get( emailTemplateGuid );
                if ( template != null )
                {
                    var errorMessages = new List<string>();

                    var message = new RockEmailMessage( template );
                    message.SetRecipients( recipients );
                    message.ThemeRoot = themeRoot;
                    message.AppRoot = appRoot;
                    message.CreateCommunicationRecord = createCommunicationHistory;
                    message.Send( out errorMessages );
                }
            }
        }

        /// <summary>
        /// Sends the specified from email.
        /// </summary>
        /// <param name="fromEmail">From email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="message">The message.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="attachments">The attachments.</param>
        /// <param name="createCommunicationHistory">if set to <c>true</c> [create communication history].</param>
        [Obsolete( "Use the RockEmailMessage object and its Send() method to send emails." )]
        public static void Send( string fromEmail, string subject, List<string> recipients, string message, string appRoot = "", string themeRoot = "", List<Attachment> attachments = null, bool createCommunicationHistory = true )
        {
            Send( fromEmail, string.Empty, subject, recipients, message, appRoot, themeRoot, attachments, createCommunicationHistory );
        }


        /// <summary>
        /// Sends the specified from email.
        /// </summary>
        /// <param name="fromEmail">From email.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="message">The message.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="attachments">The attachments.</param>
        /// <param name="createCommunicationHistory">if set to <c>true</c> [create communication history].</param>
        /// <exception cref="System.Exception">Error sending System Email: Could not read Email Medium Entity Type</exception>
        [Obsolete( "Use the RockEmailMessage object and its Send() method to send emails." )]
        public static void Send( string fromEmail, string fromName, string subject, List<string> recipients, string message, string appRoot = "", string themeRoot = "", List<Attachment> attachments = null, bool createCommunicationHistory = true )
        {
            var errorMessages = new List<string>();

            var emailMessage = new RockEmailMessage();
            emailMessage.FromEmail = fromEmail;
            emailMessage.FromName = fromName;
            emailMessage.Subject = subject;
            emailMessage.SetRecipients( recipients );
            emailMessage.Message = message;
            emailMessage.ThemeRoot = themeRoot;
            emailMessage.AppRoot = appRoot;
            emailMessage.CreateCommunicationRecord = createCommunicationHistory;

            if ( attachments != null )
            {
                foreach ( var attachment in attachments )
                {
                    var binaryFile = new BinaryFile();
                    binaryFile.ContentStream = attachment.ContentStream;
                    binaryFile.FileName = attachment.Name;
                    emailMessage.Attachments.Add( binaryFile );
                }
            }

            emailMessage.Send( out errorMessages );
        }

        #endregion
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
