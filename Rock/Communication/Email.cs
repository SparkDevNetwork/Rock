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
        /// Sends the specified email template unique identifier.
        /// </summary>
        /// <param name="emailTemplateGuid">The email template unique identifier.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        public static void Send( Guid emailTemplateGuid, List<RecipientData> recipients, string appRoot = "", string themeRoot = "" )
        {
            try
            {
                if ( recipients != null && recipients.Any() )
                {

                    var mediumEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
                    if ( mediumEntity != null )
                    {
                        var medium = MediumContainer.GetComponent( mediumEntity.Name );
                        if ( medium != null && medium.IsActive )
                        {
                            var transport = medium.Transport;
                            if ( transport != null && transport.IsActive )
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    var template = new SystemEmailService( rockContext ).Get( emailTemplateGuid );
                                    if ( template != null )
                                    {
                                        try
                                        {
                                            transport.Send( template, recipients, appRoot, themeRoot );
                                        }
                                        catch ( Exception ex1 )
                                        {
                                            throw new Exception( string.Format( "Error sending System Email ({0}).", template.Title ), ex1 );
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception( string.Format( "Error sending System Email: An invalid System Email Identifier was provided ({0}).", emailTemplateGuid.ToString() ) );
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception( string.Format( "Error sending System Email: The '{0}' medium does not have a valid transport, or the transport is not active.", mediumEntity.FriendlyName ) );
                            }
                        }
                        else
                        {
                            throw new Exception( string.Format( "Error sending System Email: The '{0}' medium does not exist, or is not active (type: {1}).", mediumEntity.FriendlyName, mediumEntity.Name ) );
                        }
                    }
                    else
                    {
                        throw new Exception( "Error sending System Email: Could not read Email Medium Entity Type" );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, HttpContext.Current );
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
        public static void Send(string fromEmail, string subject, List<string> recipients, string message, string appRoot = "", string themeRoot = "", List<Attachment> attachments = null)
        {
            Send( fromEmail, string.Empty, subject, recipients, message, appRoot, themeRoot, attachments );
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
        /// <exception cref="System.Exception">
        /// Error sending System Email: Could not read Email Medium Entity Type
        /// </exception>
        public static void Send(string fromEmail, string fromName, string subject, List<string> recipients, string message, string appRoot = "", string themeRoot = "", List<Attachment> attachments = null)
        {
            try
            {
                if ( recipients != null && recipients.Any() )
                {
                    var mediumEntity = EntityTypeCache.Read(Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid());
                    if ( mediumEntity != null )
                    {
                        var medium = MediumContainer.GetComponent(mediumEntity.Name);
                        if ( medium != null && medium.IsActive )
                        {
                            var transport = medium.Transport;
                            if ( transport != null && transport.IsActive )
                            {
                                try
                                {
                                    if ( string.IsNullOrWhiteSpace( fromName ) )
                                    {
                                        transport.Send( recipients, fromEmail, subject, message, appRoot, themeRoot, attachments );
                                    }
                                    else
                                    {
                                        transport.Send( recipients, fromEmail, fromName, subject, message, appRoot, themeRoot, attachments );
                                    }
                                }
                                catch ( Exception ex1 )
                                {
                                    throw new Exception(string.Format("Error sending System Email ({0}).", subject), ex1);
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("Error sending System Email: The '{0}' medium does not have a valid transport, or the transport is not active.", mediumEntity.FriendlyName));
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("Error sending System Email: The '{0}' medium does not exist, or is not active (type: {1}).", mediumEntity.FriendlyName, mediumEntity.Name));
                        }
                    }
                    else
                    {
                        throw new Exception("Error sending System Email: Could not read Email Medium Entity Type");
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException(ex, HttpContext.Current);
            }
        }

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
        /// <exception cref="System.Exception">
        /// Error sending System Email: Could not read Email Medium Entity Type
        /// </exception>
        public static void NotifyAdmins( string subject, string message, string appRoot = "", string themeRoot = "" )
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

                Email.Send(GlobalAttributesCache.Value("OrganizationEmail"), subject, recipients, message, appRoot, themeRoot);
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
