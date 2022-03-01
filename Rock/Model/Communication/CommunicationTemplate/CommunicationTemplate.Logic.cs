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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Rock.Security;

namespace Rock.Model
{
    public partial class CommunicationTemplate
    {
        #region Properties

        /// <summary>
        /// Gets or sets a list of email binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        public virtual IEnumerable<int> EmailAttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Where( a => a.CommunicationType == CommunicationType.Email ).Select( a => a.BinaryFileId ).ToList();
            }
        }

        /// <summary>
        /// Gets or sets a list of sms binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        public virtual IEnumerable<int> SMSAttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Where( a => a.CommunicationType == CommunicationType.SMS ).Select( a => a.BinaryFileId ).ToList();
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Adds the attachment.
        /// Specify CommunicationType.Email for Email and CommunicationType.SMS for SMS
        /// </summary>
        /// <param name="communicationTemplateAttachment">The communication template attachment.</param>
        /// <param name="communicationType">Type of the communication.</param>
        public void AddAttachment( CommunicationTemplateAttachment communicationTemplateAttachment, CommunicationType communicationType )
        {
            communicationTemplateAttachment.CommunicationType = communicationType;
            this.Attachments.Add( communicationTemplateAttachment );
        }

        /// <summary>
        /// Gets the attachments.
        /// Specify CommunicationType.Email to get the attachments for Email and CommunicationType.SMS to get the Attachment(s) for SMS
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public IEnumerable<CommunicationTemplateAttachment> GetAttachments( CommunicationType communicationType )
        {
            return this.Attachments.Where( a => a.CommunicationType == communicationType );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name ?? this.Subject ?? base.ToString();
        }

        /// <summary>
        /// Returns true if this Communication Template has an Email Template that supports the New Communication Wizard
        /// </summary>
        /// <returns></returns>
        public bool SupportsEmailWizard()
        {
            string templateHtml = this.Message;
            if ( string.IsNullOrWhiteSpace( templateHtml ) )
            {
                return false;
            }

            templateHtml = templateHtml.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( null ) );

            HtmlAgilityPack.HtmlDocument templateDoc = new HtmlAgilityPack.HtmlDocument();
            templateDoc.LoadHtml( templateHtml );

            // see if there is at least one 'dropzone' div
            var hasDropzones = templateDoc.DocumentNode.Descendants( "div" ).Where( a => a.GetAttributeValue( "class", string.Empty ).Split( new char[] { ' ' } ).Any( c => c == "dropzone" ) ).Any();

            return hasDropzones;
        }

        /// <summary>
        /// Returns true if this Communication Template has an SMS Message or a From Number for an SMS Template.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has SMS template]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasSMSTemplate()
        {
            return !string.IsNullOrWhiteSpace( this.SMSMessage ) || this.SMSFromDefinedValueId.HasValue;
        }

        /// <summary>
        /// Returns true if this Communication Template has an Email Message or a From Email Address.
        /// </summary>
        /// <returns><c>true</c> if [has email template]; otherwise, <c>false</c>.</returns>
        public bool HasEmailTemplate()
        {
            return !string.IsNullOrEmpty( this.Message ) || !string.IsNullOrEmpty( this.FromEmail );
        }

        /// <summary>
        /// When checking for security, if a template does not have specific rules, first check the category it belongs to, but then check the default entity security for templates.
        /// </summary>
        public override ISecured ParentAuthorityPre => this.Category ?? base.ParentAuthority;

        #endregion
    }
}
