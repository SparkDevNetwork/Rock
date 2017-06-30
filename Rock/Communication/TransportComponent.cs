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
        /// Validates the recipient for medium.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="recipient">The recipient.</param>
        public virtual void ValidateRecipientForMedium( Person person, CommunicationRecipient recipient )
        {
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="errorMessage">The error message.</param>
        public virtual bool Send( RockMessage message, out List<string> errorMessage )
        {
            errorMessage = new List<string>();
            return true;
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public abstract void Send( Rock.Model.Communication communication );

        /// <summary>
        /// Sends the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public abstract void Send( SystemEmail template, List<RecipientData> recipients, string appRoot, string themeRoot );

        /// <summary>
        /// Sends the specified medium data to the specified list of recipients.
        /// </summary>
        /// <param name="mediumData">The medium data.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
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
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
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
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
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
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead" )]
        public abstract void Send( List<string> recipients, string from, string fromName, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null );

        /// <summary>
        /// Gets the active transport for a given medium identifier.
        /// </summary>
        /// <param name="mediumGuid">The medium unique identifier.</param>
        /// <returns></returns>
        public static TransportComponent GetByMedium( string mediumGuid, out string errorMessage )
        {
            var mediumEntity = EntityTypeCache.Read( mediumGuid.AsGuid() );
            if ( mediumEntity == null )
            {
                errorMessage = "Could not determine the Communication Medium.";
                return null;
            }

            var medium = MediumContainer.GetComponent( mediumEntity.Name );
            if ( medium == null || !medium.IsActive )
            {
                errorMessage = "Could not find the active Communication Medium.";
                return null;
            }

            var transport = medium.Transport;
            if ( transport == null || !transport.IsActive )
            {
                errorMessage = "Could not find the active Communication Transport.";
                return null;
            }

            errorMessage = string.Empty;
            return transport;
        }

    }
   
}
