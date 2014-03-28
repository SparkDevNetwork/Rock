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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Channel
{
    /// <summary>
    /// An email communication
    /// </summary>
    [Description( "An email communication" )]
    [Export(typeof(ChannelComponent))]
    [ExportMetadata("ComponentName", "Email")]

    [CodeEditorField( "Unsubscribe HTML", "The HTML to inject into email contents when the communication is a Bulk Email.  Contents will be placed wherever the 'Unsubcribe HTML' merge field is used, or if not used, at the end of the email in email contents.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 300, false, @"
<p style='float: right;'>
    <small><a href='{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.UrlEncodedKey }}/'>Unsubscribe</a></small>
</p>
" )]
    public class Email : ChannelComponent
    {
        /// <summary>
        /// Gets the control path.
        /// </summary>
        /// <value>
        /// The control path.
        /// </value>
        public override ChannelControl Control
        {
            get { return new Rock.Web.UI.Controls.Communication.Email(); }
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="personAlias">The current person alias.</param>
        public override void Send( Model.Communication communication, Model.PersonAlias personAlias )
        {
            var communicationService = new CommunicationService();
            communication = communicationService.Get( communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.Recipients.Where( r => r.Status == Model.CommunicationRecipientStatus.Pending ).Any() &&
                (!communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo(RockDateTime.Now) <= 0))
            {
                bool bulkEmail = communication.GetChannelDataValue( "BulkEmail" ).AsBoolean();

                // Update any recipients that should not get sent the communication
                var recipientService = new CommunicationRecipientService( communicationService.RockContext );
                foreach ( var recipient in recipientService.Queryable( "Person" )
                    .Where( r => 
                        r.CommunicationId == communication.Id &&
                        r.Status == CommunicationRecipientStatus.Pending)
                    .ToList() )
                {
                    var person = recipient.Person;
                    if ( person.EmailPreference == Model.EmailPreference.DoNotEmail )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Email Preference of 'Do Not Email!'";
                        recipientService.Save( recipient, null );
                    }
                    else if ( person.EmailPreference == Model.EmailPreference.NoMassEmails && bulkEmail )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Email Preference of 'No Mass Emails!'";
                        recipientService.Save( recipient, null );
                    }
                }

                // If an unbsubcribe value has been entered, and this is a bulk email, add the text
                if ( bulkEmail )
                {
                    string unsubscribeHtml = GetAttributeValue( "UnsubscribeHTML" );
                    if ( !string.IsNullOrWhiteSpace( unsubscribeHtml ) )
                    {
                        communication.SetChannelDataValue( "UnsubscribeHTML", unsubscribeHtml );
                        communicationService.Save( communication, null );
                    }
                }
            }

            base.Send( communication, personAlias );
        }
    }
}
