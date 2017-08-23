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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls.Communication;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for creating a new communication
    /// </summary>
    [DisplayName( "Template Detail" )]
    [Category( "Communication" )]
    [Description( "Used for editing a communication template that can be selected when creating a new communication, SMS, etc. to people." )]
    public partial class TemplateDetail : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "TemplateId" ).AsInteger() );
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<Rock.Web.UI.BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            string pageTitle = "New Template";

            int? templateId = PageParameter( "TemplateId" ).AsIntegerOrNull();
            if ( templateId.HasValue )
            {
                var template = new CommunicationTemplateService( new RockContext() ).Get( templateId.Value );
                if ( template != null )
                {
                    pageTitle = template.Name;
                }
            }

            breadCrumbs.Add( new BreadCrumb( pageTitle, pageReference ) );
            RockPage.Title = pageTitle;

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                var service = new CommunicationTemplateService( rockContext );

                CommunicationTemplate communicationTemplate = null;
                int? communicationTemplateId = hfCommunicationTemplateId.Value.AsIntegerOrNull();
                if ( communicationTemplateId.HasValue )
                {
                    communicationTemplate = service.Get( communicationTemplateId.Value );
                }

                bool newTemplate = false;
                if ( communicationTemplate == null )
                {
                    newTemplate = true;
                    communicationTemplate = new Rock.Model.CommunicationTemplate();
                    service.Add( communicationTemplate );
                }

                communicationTemplate.Name = tbName.Text;
                communicationTemplate.Description = tbDescription.Text;
                communicationTemplate.ImageFileId = imgTemplatePreview.BinaryFileId;



                communicationTemplate.FromName = tbFromName.Text;
                communicationTemplate.FromEmail = tbFromAddress.Text;
                communicationTemplate.ReplyToEmail = tbReplyToAddress.Text;
                communicationTemplate.CCEmails = tbCCList.Text;
                communicationTemplate.BCCEmails = tbBCCList.Text;

                communicationTemplate.AttachmentBinaryFileIds = hfAttachedBinaryFileIds.Value.SplitDelimitedValues().AsIntegerList();

                // delete any attachments that are no longer included
                foreach ( var attachment in communicationTemplate.Attachments.Where( a => !communicationTemplate.AttachmentBinaryFileIds.Contains( a.BinaryFileId ) ).ToList() )
                {
                    communicationTemplate.Attachments.Remove( attachment );
                }

                // add any new attachments that were added
                foreach ( var attachmentBinaryFileId in communicationTemplate.AttachmentBinaryFileIds.Where( a => !communicationTemplate.Attachments.Any( x => x.BinaryFileId == a ) ) )
                {
                    communicationTemplate.Attachments.Add( new CommunicationTemplateAttachment { BinaryFileId = attachmentBinaryFileId } );
                }


                // TODO communicationTemplate.Attachments.C

                communicationTemplate.Subject = tbEmailSubject.Text;
                communicationTemplate.Message = ceEmailTemplate.Text;

                communicationTemplate.SMSFromDefinedValueId = ddlSMSFrom.SelectedValue.AsIntegerOrNull();
                communicationTemplate.SMSMessage = tbSMSTextMessage.Text;

                if ( communicationTemplate != null )
                {
                    rockContext.SaveChanges();
                    NavigateToParentPage();
                }

                if ( newTemplate && !IsUserAuthorized( Authorization.EDIT ) )
                {
                    communicationTemplate.MakePrivate( Authorization.VIEW, CurrentPerson );
                    communicationTemplate.MakePrivate( Authorization.EDIT, CurrentPerson );
                }
            }

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="templateId">The template identifier.</param>
        private void ShowDetail( int templateId )
        {
            CommunicationTemplate communicationTemplate = null;

            if ( !templateId.Equals( 0 ) )
            {
                communicationTemplate = new CommunicationTemplateService( new RockContext() ).Get( templateId );
                if ( communicationTemplate != null )
                {
                    lTitle.Text = communicationTemplate.Name.FormatAsHtmlTitle();
                }
            }

            if ( communicationTemplate == null )
            {
                RockPage.PageTitle = "New Communication Template";
                lTitle.Text = "New Communication Template".FormatAsHtmlTitle();
                communicationTemplate = new CommunicationTemplate();
            }

            LoadDropDowns();

            hfCommunicationTemplateId.Value = templateId.ToString();

            tbName.Text = communicationTemplate.Name;
            tbDescription.Text = communicationTemplate.Description;
            imgTemplatePreview.BinaryFileId = communicationTemplate.ImageFileId;
            // TODO imgTemplatePreview.BinaryFileTypeGuid = 




            // Email Fields
            tbFromName.Text = communicationTemplate.FromName;
            tbFromAddress.Text = communicationTemplate.FromEmail;

            tbReplyToAddress.Text = communicationTemplate.ReplyToEmail;
            tbCCList.Text = communicationTemplate.CCEmails;
            tbBCCList.Text = communicationTemplate.BCCEmails;

            hfShowAdditionalFields.Value = ( !string.IsNullOrEmpty( communicationTemplate.ReplyToEmail ) || !string.IsNullOrEmpty( communicationTemplate.CCEmails ) || !string.IsNullOrEmpty( communicationTemplate.BCCEmails ) ).ToTrueFalse().ToLower();

            tbEmailSubject.Text = communicationTemplate.Subject;
            ceEmailTemplate.Text = communicationTemplate.Message;

            hfAttachedBinaryFileIds.Value = communicationTemplate.Attachments.Select( a => a.BinaryFileId ).ToList().AsDelimited( "," );
            UpdateAttachedFiles( false );

            // SMS Fields
            ddlSMSFrom.SetValue( communicationTemplate.SMSFromDefinedValueId );
            tbSMSTextMessage.Text = communicationTemplate.SMSMessage;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlSMSFrom.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM ) ), true, true );
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fupAttachments_FileUploaded( object sender, FileUploaderEventArgs e )
        {
            UpdateAttachedFiles( true );
        }

        /// <summary>
        /// Updates the HTML for the Attached Files and optionally adds the file that was just uploaded using the file uploader
        /// </summary>
        private void UpdateAttachedFiles( bool addUploadedFile )
        {
            List<int> attachmentList = hfAttachedBinaryFileIds.Value.SplitDelimitedValues().AsIntegerList();
            if ( addUploadedFile && fupAttachments.BinaryFileId.HasValue )
            {
                if ( !attachmentList.Contains( fupAttachments.BinaryFileId.Value ) )
                {
                    attachmentList.Add( fupAttachments.BinaryFileId.Value );
                }
            }

            hfAttachedBinaryFileIds.Value = attachmentList.AsDelimited( "," );
            fupAttachments.BinaryFileId = null;

            // pre-populate dictionary so that the attachments are listed in the order they were added
            Dictionary<int, string> binaryFileAttachments = attachmentList.ToDictionary( k => k, v => string.Empty );
            using ( var rockContext = new RockContext() )
            {
                var binaryFileInfoList = new BinaryFileService( rockContext ).Queryable()
                        .Where( f => attachmentList.Contains( f.Id ) )
                        .Select( f => new
                        {
                            f.Id,
                            f.FileName
                        } );

                foreach ( var binaryFileInfo in binaryFileInfoList )
                {
                    binaryFileAttachments[binaryFileInfo.Id] = binaryFileInfo.FileName;
                }
            }

            StringBuilder sbAttachmentsHtml = new StringBuilder();
            sbAttachmentsHtml.AppendLine( "<div class='attachment'>" );
            sbAttachmentsHtml.AppendLine( "  <ul class='attachment-content'>" );

            foreach ( var binaryFileAttachment in binaryFileAttachments )
            {
                var attachmentUrl = string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), binaryFileAttachment.Key );
                var removeAttachmentJS = string.Format( "removeAttachment( this, '{0}', '{1}' );", hfAttachedBinaryFileIds.ClientID, binaryFileAttachment.Key );
                sbAttachmentsHtml.AppendLine( string.Format( "    <li><a href='{0}' target='_blank'>{1}</a> <a><i class='fa fa-times' onclick=\"{2}\"></i></a></li>", attachmentUrl, binaryFileAttachment.Value, removeAttachmentJS ) );
            }

            sbAttachmentsHtml.AppendLine( "  </ul>" );
            sbAttachmentsHtml.AppendLine( "</div>" );

            lAttachmentListHtml.Text = sbAttachmentsHtml.ToString();
        }

        /// <summary>
        /// Handles the SelectItem event of the mfpMessage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mfpMessage_SelectItem( object sender, EventArgs e )
        {
            tbSMSTextMessage.Text += mfpSMSMessage.SelectedMergeField;
            mfpSMSMessage.SetValue( string.Empty );
        }

        #endregion
    }
}
