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
using System.ComponentModel.Composition;
using System.Net.Mail;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication through SMTP protocol
    /// </summary>
    [Description( "Sends a communication through Mandrill's SMTP API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Mandrill SMTP" )]

    [TextField( "Server", "", true, "smtp.mandrillapp.com", "", 0 )]
    [TextField( "Username", "The SMTP username provided by Mandrill", true, "", "", 1 )]
    [TextField( "Password", "Any valid Mandrill API key (not the password you use to login to Mandrill)", true, "", "", 2, null, true )]
    [IntegerField( "Port", "", false, 25, "", 3 )]
    [BooleanField( "Use SSL", "", false, "", 4 )]
    [BooleanField( "Inline CSS", "Enable Mandrill's CSS Inliner feature.", true, "", 5 )]
    public class MandrillSmtp : SMTPComponent
    {
        /// <summary>
        /// Gets a value indicating whether transport has ability to track recipients opening the communication.
        /// </summary>
        /// <value>
        /// <c>true</c> if transport can track opens; otherwise, <c>false</c>.
        /// </value>
        public override bool CanTrackOpens
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the recipient status note.
        /// </summary>
        /// <value>
        /// The status note.
        /// </value>
        public override string StatusNote
        {
            get
            {
                return String.Format( "Email was received for delivery by Mandrill ({0})", RockDateTime.Now );
            }
        }

        /// <summary>
        /// Adds any additional headers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="headers">The headers.</param>
        public override void AddAdditionalHeaders( MailMessage message, Dictionary<string, string> headers )
        {
            bool inlineCss = GetAttributeValue( "InlineCSS" ).AsBoolean( true );

            // add mandrill headers
            message.Headers.Add( "X-MC-Track", "opens, clicks" );
            message.Headers.Add( "X-MC-InlineCSS", inlineCss.ToString().ToLower() );

            if ( headers != null )
            {
                var metaValues = new List<string>();
                foreach ( var param in headers )
                {
                    metaValues.Add( String.Format( "\"{0}\":\"{1}\"", param.Key, param.Value ) );
                }
                message.Headers.Add( "X-MC-Metadata", String.Format( @"{{{0}}}", metaValues.AsDelimited( "," ) ) );
            }
        }
    }
}
