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
using System.Data.Entity;
using System.Net.Mail;
using System.Linq;

using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Base class for components that perform actions for a workflow
    /// </summary>
    public abstract class TransportComponent : Component
    {
        /// <summary>
        /// Gets a value indicating whether transport has ability to track recipients opening the communication.
        /// </summary>
        /// <value>
        ///   <c>true</c> if transport can track opens; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanTrackOpens
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Sends the specified rock message.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public abstract bool Send( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages );

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        public abstract void Send( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes );

        /// <summary>
        /// Validates the recipient.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="isBulkCommunication">if set to <c>true</c> [is bulk communication].</param>
        /// <returns></returns>
        public virtual bool ValidRecipient( CommunicationRecipient recipient, bool isBulkCommunication )
        {
            bool valid = true;

            var person = recipient?.PersonAlias?.Person;
            if ( person != null )
            {
                if ( person.IsDeceased )
                {
                    recipient.Status = CommunicationRecipientStatus.Failed;
                    recipient.StatusNote = "Person is deceased";
                    valid = false;
                }

                else if ( recipient.Communication.ListGroupId.HasValue  )
                {
                    // if this communication is being sent to a list, make sure the recipient is still an active member of the list
                    GroupMemberStatus? groupMemberStatus = null;
                    using ( var rockContext = new Rock.Data.RockContext() )
                    {
                        groupMemberStatus = new GroupMemberService( rockContext ).Queryable()
                            .Where( a => a.PersonId == person.Id && a.GroupId == recipient.Communication.ListGroupId )
                            .Select(a => a.GroupMemberStatus ).FirstOrDefault();
                    }

                    if ( groupMemberStatus != null )
                    {
                        if ( groupMemberStatus == GroupMemberStatus.Inactive )
                        {
                            recipient.Status = CommunicationRecipientStatus.Failed;
                            recipient.StatusNote = "Person is not active member of communication list: " + recipient.Communication.ListGroup.Name;
                            valid = false;
                        }
                    }
                    else
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Person is not member of communication list: " + recipient.Communication.ListGroup.Name;
                        valid = false;
                    }
                }
            }

            return valid;
        }

        /// <summary>
        /// Resolves the text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="person">The person.</param>
        /// <param name="enabledLavaCommands">The enabled lava commands.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <returns></returns>
        public virtual string ResolveText( string content, Person person, string enabledLavaCommands, Dictionary<string, object> mergeFields, string appRoot = "", string themeRoot = "" )
        {
            string value = content.ResolveMergeFields( mergeFields, person, enabledLavaCommands );
            value = value.ReplaceWordChars();

            if ( themeRoot.IsNotNullOrWhiteSpace() )
            {
                value = value.Replace( "~~/", themeRoot );
            }

            if ( appRoot.IsNotNullOrWhiteSpace() )
            {
                value = value.Replace( "~/", appRoot );
                value = value.Replace( @" src=""/", @" src=""" + appRoot );
                value = value.Replace( @" src='/", @" src='" + appRoot );
                value = value.Replace( @" href=""/", @" href=""" + appRoot );
                value = value.Replace( @" href='/", @" href='" + appRoot );
            }

            return value;
        }

        /// <summary>
        /// Resolves the text and adds it to the CommunicationRecipient.SentMessage object.
        /// Don't forget to call RockContext.SaveChanges() to persist to the DB.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="person">The person.</param>
        /// <param name="enabledLavaCommands">The enabled lava commands.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="communicationRecipient">The communication recipient.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <returns></returns>
        public virtual string ResolveText( string content, Person person, CommunicationRecipient communicationRecipient, string enabledLavaCommands, Dictionary<string, object> mergeFields, string appRoot = "", string themeRoot = "" )
        {
            string value = ResolveText( content, person, enabledLavaCommands, mergeFields, appRoot, themeRoot );

            if ( communicationRecipient != null )
            {
                communicationRecipient.SentMessage = value;
            }

            return value;
        }

        #region Obsolete

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( Communication communication, Dictionary<string, string> mediumAttributes ) instead", true )]
        public abstract void Send( Model.Communication communication );

        /// <summary>
        /// Sends the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public abstract void Send( SystemEmail template, List<RecipientData> recipients, string appRoot, string themeRoot );

        /// <summary>
        /// Sends the specified medium data to the specified list of recipients.
        /// </summary>
        /// <param name="mediumData">The medium data.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public abstract void Send(Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot);

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public abstract void Send(List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null);

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// /// <param name="attachments">Attachments.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public abstract void Send(List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null);


        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="attachments">The attachments.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public abstract void Send( List<string> recipients, string from, string fromName, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null );

        #endregion

    }
   
}
