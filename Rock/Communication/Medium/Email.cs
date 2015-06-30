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

namespace Rock.Communication.Medium
{
    /// <summary>
    /// An email communication
    /// </summary>
    [Description( "An email communication" )]
    [Export( typeof( MediumComponent ) )]
    [ExportMetadata( "ComponentName", "Email" )]

    [CodeEditorField( "Unsubscribe HTML", "The HTML to inject into email contents when the communication is a Bulk Communication.  Contents will be placed wherever the 'Unsubcribe HTML' merge field is used, or if not used, at the end of the email in email contents.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, @"
<a href='{{ 'Global' | Attribute:'PublicApplicationRoot' }}Unsubscribe/{{ Person.UrlEncodedKey }}'>Unsubscribe</a>", "", 2 )]
    [CodeEditorField( "Default Plain Text", "The text to use when the plain text field is left blank.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, @"
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ GlobalAttribute.PublicApplicationRoot }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person.UrlEncodedKey }}
", "", 3 )]
    public class Email : MediumComponent
    {

        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        /// <returns></returns>
        public override MediumControl GetControl( bool useSimpleMode )
        {
            return new Rock.Web.UI.Controls.Communication.Email( useSimpleMode );
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

            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
            var mergeValues = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );

            // Requery the Communication object
            communication = new CommunicationService( rockContext ).Get( communication.Id );
            mergeValues.Add( "Communication", communication );

            if ( person != null )
            {
                mergeValues.Add( "Person", person );

                var recipient = communication.Recipients.Where( r => r.PersonAlias != null && r.PersonAlias.PersonId == person.Id ).FirstOrDefault();
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
            string htmlContent = communication.GetMediumDataValue( "HtmlMessage" );
            sbContent.Append( Email.ProcessHtmlBody( communication, globalAttributes, mergeValues ) );

            // Attachments
            StringBuilder sbAttachments = new StringBuilder();
            string attachmentIds = communication.GetMediumDataValue( "Attachments" );
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
        /// Gets the read-only message details.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        public override string GetMessageDetails( Model.Communication communication )
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<div class='row'>");
            sb.AppendLine( "<div class='col-md-6'>" );

            AppendMediumData( communication, sb, "FromName" );
            AppendMediumData( communication, sb, "FromAddress" );
            AppendMediumData( communication, sb, "ReplyTo" );
            AppendMediumData( communication, sb, "Subject" );

            sb.AppendLine( "</div>" );
            sb.AppendLine( "<div class='col-md-6'>" );
            AppendAttachmentData( sb, communication.GetMediumDataValue( "Attachments" ) );
            sb.AppendLine( "</div>" );
            sb.AppendLine( "</div>" );

            string value = communication.GetMediumDataValue( "HtmlMessage" );
            if (!string.IsNullOrWhiteSpace(value))
            {
                AppendMediumData( sb, "HtmlMessage", string.Format( @"
                        <iframe id='js-email-body-iframe' class='email-body'></iframe>
                        <script id='email-body' type='text/template'>{0}</script>
                        <script type='text/javascript'>
                            var doc = document.getElementById('js-email-body-iframe').contentWindow.document;
                            doc.open();
                            doc.write('<html><head><title></title></head><body>' +  $('#email-body').html() + '</body></html>');
                            doc.close();
                        </script>
                    ", value ) );
            }

            AppendMediumData( communication, sb, "TextMessage" );

            return sb.ToString();
        }

        private void AppendMediumData(Model.Communication communication, StringBuilder sb, string key)
        {
            string value = communication.GetMediumDataValue( key );
            if (!string.IsNullOrWhiteSpace(value))
            {
                AppendMediumData( sb, key, value );
            }
        }

        private void AppendAttachmentData(StringBuilder sb, string value)
        {
            var attachments = new Dictionary<int, string>();

            var fileIds = new List<int>();
            value.SplitDelimitedValues().ToList().ForEach( v => fileIds.Add( v.AsInteger() ) );

            new BinaryFileService( new RockContext() ).Queryable()
                .Where( f => fileIds.Contains( f.Id ) )
                .Select( f => new
                {
                    f.Id,
                    f.FileName
                } )
                .ToList()
                .ForEach( f => attachments.Add( f.Id, f.FileName ) );

            if (attachments.Any())
            {
                StringBuilder sbAttachments = new StringBuilder();
                sbAttachments.Append( "<ul>" );
                foreach(var keyValue in attachments)
                {
                    sbAttachments.AppendFormat( "<li><a target='_blank' href='{0}GetFile.ashx?id={1}'>{2}</a></li>",
                        System.Web.VirtualPathUtility.ToAbsolute( "~" ), keyValue.Key, keyValue.Value );
                }
                sbAttachments.Append( "</ul>" );

                AppendMediumData( sb, "Attachments", sbAttachments.ToString() );
            }
        }

        private void AppendMediumData( StringBuilder sb, string key, string value )
        {
            sb.AppendFormat( "<div class='form-group'><label class='control-label'>{0}</label><p class='form-control-static'>{1}</p></div>",
                key.SplitCase(), value );
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void Send( Model.Communication communication )
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );

            communication = communicationService.Queryable( "Recipients" )
                .FirstOrDefault( t => t.Id == communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.Recipients.Where( r => r.Status == Model.CommunicationRecipientStatus.Pending ).Any() &&
                ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
            {
                // Update any recipients that should not get sent the communication
                var recipientService = new CommunicationRecipientService( rockContext );
                foreach ( var recipient in recipientService.Queryable( "PersonAlias.Person" )
                    .Where( r =>
                        r.CommunicationId == communication.Id &&
                        r.Status == CommunicationRecipientStatus.Pending )
                    .ToList() )
                {
                    var person = recipient.PersonAlias.Person;
                    if ( !(person.IsEmailActive ?? true))
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Email is not active!";
                    }
                    if ( person.IsDeceased )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Person is deceased!";
                    }
                    if ( person.EmailPreference == Model.EmailPreference.DoNotEmail )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Email Preference of 'Do Not Email!'";
                    }
                    else if ( person.EmailPreference == Model.EmailPreference.NoMassEmails && communication.IsBulkCommunication )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Email Preference of 'No Mass Emails!'";
                    }
                }

                // If an unsubscribe value has been entered, and this is a bulk email, add the text
                if ( communication.IsBulkCommunication )
                {
                    string unsubscribeHtml = GetAttributeValue( "UnsubscribeHTML" );
                    if ( !string.IsNullOrWhiteSpace( unsubscribeHtml ) )
                    {
                        communication.SetMediumDataValue( "UnsubscribeHTML", unsubscribeHtml );
                    }
                }

                string defaultPlainText = GetAttributeValue( "DefaultPlainText" );
                if ( !string.IsNullOrWhiteSpace( defaultPlainText ) )
                {
                    communication.SetMediumDataValue( "DefaultPlainText", defaultPlainText );
                }

                rockContext.SaveChanges();
            }

            base.Send( communication );
        }

        /// <summary>
        /// Processes the HTML body.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="globalAttributes">The global attributes.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="currentPersonOverride">The current person override.</param>
        /// <returns></returns>
        public static string ProcessHtmlBody( Rock.Model.Communication communication,
            Rock.Web.Cache.GlobalAttributesCache globalAttributes,
            Dictionary<string, object> mergeObjects,
            Person currentPersonOverride = null )
        {
            string htmlBody = communication.GetMediumDataValue( "HtmlMessage" );
            if ( !string.IsNullOrWhiteSpace( htmlBody ) )
            {
                // Get the unsubscribe content and add a merge field for it
                string unsubscribeHtml = communication.GetMediumDataValue( "UnsubscribeHTML" ).ResolveMergeFields( mergeObjects, currentPersonOverride );
                if (mergeObjects.ContainsKey( "UnsubscribeOption"))
                {
                    mergeObjects.Add( "UnsubscribeOption", unsubscribeHtml );
                }
                else
                {
                    mergeObjects["UnsubscribeOption"] = unsubscribeHtml;
                }
                
                // Resolve merge fields
                htmlBody = htmlBody.ResolveMergeFields( mergeObjects, currentPersonOverride );

                // Resolve special syntax needed if option was included in global attribute
                if ( Regex.IsMatch( htmlBody, @"\[\[\s*UnsubscribeOption\s*\]\]" ) )
                {
                    htmlBody = Regex.Replace( htmlBody, @"\[\[\s*UnsubscribeOption\s*\]\]", unsubscribeHtml );
                }

                // Add the unsubscribe option at end if it wasn't included in content
                if ( !htmlBody.Contains( unsubscribeHtml ) )
                {
                    htmlBody += unsubscribeHtml;
                }

                // Resolve any relative paths
                string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                htmlBody = htmlBody.Replace( @" src=""/", @" src=""" + publicAppRoot );
                htmlBody = htmlBody.Replace( @" href=""/", @" href=""" + publicAppRoot );
            }

            return htmlBody;

        }

        /// <summary>
        /// Processes the text body.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="globalAttributes">The global attributes.</param>
        /// <param name="mergeObjects">The merge objects.</param>
        /// <param name="currentPersonOverride">The current person override.</param>
        /// <returns></returns>
        public static string ProcessTextBody ( Rock.Model.Communication communication,
            Rock.Web.Cache.GlobalAttributesCache globalAttributes,
            Dictionary<string, object> mergeObjects,
            Person currentPersonOverride = null )
        {

            string defaultPlainText = communication.GetMediumDataValue( "DefaultPlainText" );
            string plainTextBody = communication.GetMediumDataValue( "TextMessage" );

            if ( string.IsNullOrWhiteSpace( plainTextBody ) && !string.IsNullOrWhiteSpace(defaultPlainText))
            {
                plainTextBody = defaultPlainText;
            }

            return plainTextBody.ResolveMergeFields( mergeObjects, currentPersonOverride );

        }

        /// <summary>
        /// Gets a value indicating whether [supports bulk communication].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports bulk communication]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsBulkCommunication
        {
            get 
            {
                return true;
            }
        }
    }
}
