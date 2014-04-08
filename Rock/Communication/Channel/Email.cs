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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Channel
{
    /// <summary>
    /// An email communication
    /// </summary>
    [Description( "An email communication" )]
    [Export( typeof( ChannelComponent ) )]
    [ExportMetadata( "ComponentName", "Email" )]

    [CodeEditorField( "Unsubscribe HTML", "The HTML to inject into email contents when the communication is a Bulk Email.  Contents will be placed wherever the 'Unsubcribe HTML' merge field is used, or if not used, at the end of the email in email contents.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, @"
<p style='float: right;'>
    <small><a href='{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.UrlEncodedKey }}'>Unsubscribe</a></small>
</p>
", "", 2 )]
    [CodeEditorField( "Default Plain Text", "The text to use when the plain text field is left blank.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, @"
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ GlobalAttribute.PublicApplicationRoot }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person.UrlEncodedKey }}
", "", 3 )]
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
        /// Gets the HTML preview.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override string GetHtmlPreview( Model.Communication communication, Person person )
        {
            var rockContext = new RockContext();

            StringBuilder sbContent = new StringBuilder();

            // Requery the Communication object
            communication = new CommunicationService( rockContext ).Get( communication.Id );

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            var mergeValues = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );

            if ( person != null )
            {
                mergeValues.Add( "Person", person );

                var recipient = communication.Recipients.Where( r => r.PersonId == person.Id ).FirstOrDefault();
                if ( recipient != null )
                {
                    // Add any additional merge fields created through a report
                    foreach ( var mergeField in recipient.AdditionalMergeValues )
                    {
                        if ( !mergeValues.ContainsKey( mergeField.Key ) )
                        {
                            mergeValues.Add( mergeField.Key, mergeField.Value );
                        }
                    }
                }
            }

            // Body
            string htmlContent = communication.GetChannelDataValue( "HtmlMessage" );
            sbContent.Append( Email.ProcessHtmlBody( communication, globalAttributes, mergeValues ) );

            // Attachments
            StringBuilder sbAttachments = new StringBuilder();
            string attachmentIds = communication.GetChannelDataValue( "Attachments" );
            if ( !string.IsNullOrWhiteSpace( attachmentIds ) )
            {
                sbContent.Append( "<br/><br/>" );

                var binaryFileService = new BinaryFileService( rockContext );

                foreach ( string idVal in attachmentIds.SplitDelimitedValues() )
                {
                    int binaryFileId = int.MinValue;
                    if ( int.TryParse( idVal, out binaryFileId ) )
                    {
                        var binaryFile = binaryFileService.Get( binaryFileId );
                        if ( binaryFile != null )
                        {
                            sbContent.AppendFormat( "<a target='_blank' href='{0}GetFile.ashx?id={1}'>{2}</a><br/>",
                                System.Web.VirtualPathUtility.ToAbsolute( "~" ), binaryFile.Id, binaryFile.FileName );
                        }
                    }
                }
            }

            return sbContent.ToString();
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void Send( Model.Communication communication )
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );

            communication = communicationService.Get( communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.Recipients.Where( r => r.Status == Model.CommunicationRecipientStatus.Pending ).Any() &&
                ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
            {
                bool bulkEmail = communication.GetChannelDataValue( "BulkEmail" ).AsBoolean();

                // Update any recipients that should not get sent the communication
                var recipientService = new CommunicationRecipientService( rockContext );
                foreach ( var recipient in recipientService.Queryable( "Person" )
                    .Where( r =>
                        r.CommunicationId == communication.Id &&
                        r.Status == CommunicationRecipientStatus.Pending )
                    .ToList() )
                {
                    var person = recipient.Person;
                    if ( person.EmailPreference == Model.EmailPreference.DoNotEmail )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Email Preference of 'Do Not Email!'";
                    }
                    else if ( person.EmailPreference == Model.EmailPreference.NoMassEmails && bulkEmail )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Email Preference of 'No Mass Emails!'";
                    }
                }

                // If an unbsubcribe value has been entered, and this is a bulk email, add the text
                if ( bulkEmail )
                {
                    string unsubscribeHtml = GetAttributeValue( "UnsubscribeHTML" );
                    if ( !string.IsNullOrWhiteSpace( unsubscribeHtml ) )
                    {
                        communication.SetChannelDataValue( "UnsubscribeHTML", unsubscribeHtml );
                    }
                }

                string defaultPlainText = GetAttributeValue( "DefaultPlainText" );
                if ( !string.IsNullOrWhiteSpace( defaultPlainText ) )
                {
                    communication.SetChannelDataValue( "DefaultPlainText", defaultPlainText );
                }

                rockContext.SaveChanges();
            }

            base.Send( communication );
        }

        public static string ProcessHtmlBody( Rock.Model.Communication communication,
            Rock.Web.Cache.GlobalAttributesCache globalAttributes,
            Dictionary<string, object> mergeObjects )
        {
            string unsubscribeHtml = communication.GetChannelDataValue( "UnsubscribeHTML" );
            string htmlBody = communication.GetChannelDataValue( "HtmlMessage" );

            // If there is unsubscribe html (would have been added by channel PreSend), inject it
            if ( !string.IsNullOrWhiteSpace( htmlBody ) && !string.IsNullOrWhiteSpace( unsubscribeHtml ) )
            {
                string newHtml = Regex.Replace( htmlBody, @"\{\{\s*UnsubscribeOption\s*\}\}", unsubscribeHtml );
                if ( htmlBody != newHtml )
                {
                    // If the content changed, then the merge field was found and newHtml has the unsubscribe contents
                    htmlBody = newHtml;
                }
                else
                {
                    // If it didn't change, the body did not contain merge field so add unsubscribe contents at end
                    htmlBody += unsubscribeHtml;
                }
            }

            if ( !string.IsNullOrWhiteSpace( htmlBody ) )
            {
                string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                htmlBody = htmlBody.Replace( @" src=""/", @" src=""" + publicAppRoot );
                htmlBody = htmlBody.Replace( @" href=""/", @" href=""" + publicAppRoot );
                htmlBody = htmlBody.ResolveMergeFields( mergeObjects );
            }

            return htmlBody;

        }

        public static string ProcessTextBody ( Rock.Model.Communication communication,
            Rock.Web.Cache.GlobalAttributesCache globalAttributes,
            Dictionary<string, object> mergeObjects )
        {

            string defaultPlainText = communication.GetChannelDataValue( "DefaultPlainText" );
            string plainTextBody = communication.GetChannelDataValue( "TextMessage" );

            if ( string.IsNullOrWhiteSpace( plainTextBody ) && !string.IsNullOrWhiteSpace(defaultPlainText))
            {
                plainTextBody = defaultPlainText;
            }

            return plainTextBody.ResolveMergeFields( mergeObjects );

        }
    }
}
