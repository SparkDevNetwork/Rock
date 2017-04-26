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
    [Description( "Sends a communication through Mailgun's SMTP API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Mailgun SMTP" )]

    [TextField( "SMTP Hostname", "", true, "smtp.mailgun.org", "", 0, "Server" )]
    [TextField( "Domain Login", "The SMTP login provided by Mailgun", true, "", "", 1, "Username" )]
    [TextField( "Domain Password", "The SMTP password provided by Mailgun", true, "", "", 2, "Password", true )]
    [TextField( "API Key", "The API Key provided by Mailgun " )]
    [IntegerField( "Port", "", false, 587, "", 3 )]
    [BooleanField( "Use SSL", "", true, "", 4 )]
    [BooleanField( "Track Clicks", "", true, "", 5 )]
    public class MailgunSmtp : SMTPComponent
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
                return GetAttributeValue( "TrackClicks" ).AsBoolean( true );
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
                return String.Format( "Email was received for delivery by Mailgun ({0})", RockDateTime.Now );
            }
        }

        /// <summary>
        /// Adds any additional headers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="headers">The headers.</param>
        public override void AddAdditionalHeaders( MailMessage message, Dictionary<string, string> headers )
        {
            var trackClicks = GetAttributeValue( "TrackClicks" ).AsBoolean( true ) ? "yes" : "no";

            // add headers
            message.Headers.Add( "X-Mailgun-Track", trackClicks );
            message.Headers.Add( "X-Mailgun-Track-Clicks", trackClicks );
            message.Headers.Add( "X-Mailgun-Track-Opens", trackClicks );

            if ( headers != null )
            {
                var variables = new List<string>();
                foreach ( var param in headers )
                {
                    variables.Add( String.Format( "\"{0}\":\"{1}\"", param.Key, param.Value ) );
                }
                message.Headers.Add( "X-Mailgun-Variables", String.Format( @"{{{0}}}", variables.AsDelimited( "," ) ) );
            }
        }
    }
}
