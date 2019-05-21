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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls.Communication
{
    /// <summary>
    /// Email Communication Medium control
    /// </summary>
    public class Email : MediumControl
    {

        #region UI Controls

        private RockTextBox tbFromName;
        private EmailBox ebFromAddress;
        private RockLiteral lFromName;
        private RockLiteral lFromAddress;
        private EmailBox ebReplyToAddress;
        private EmailBox ebCcAddress;
        private EmailBox ebBccAddress;
        private RockTextBox tbSubject;
        private HtmlEditor htmlMessage;
        private HiddenField hfAttachments;
        private FileUploader fupEmailAttachments;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [use simple mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use simple mode]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSimpleMode
        {
            get { return ViewState["UseSimpleMode"] as Boolean? ?? false; }
            set { ViewState["UseSimpleMode"] = value; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether [allow cc/bcc].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow cc/bcc]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowCcBcc
        {
            get { return ViewState["AllowCcBcc"] as Boolean? ?? false; }
            set { ViewState["AllowCcBcc"] = value; }
        }

        /// <summary>
        /// Sets control values from a communication record.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void SetFromCommunication( CommunicationDetails communication )
        {
            EnsureChildControls();
            tbFromName.Text = communication.FromName;
            ebFromAddress.Text = communication.FromEmail;
            ebReplyToAddress.Text = communication.ReplyToEmail;
            ebCcAddress.Text = communication.CCEmails;
            ebBccAddress.Text = communication.BCCEmails;
            tbSubject.Text = communication.Subject;
            htmlMessage.Text = communication.Message;
            hfAttachments.Value = communication.EmailAttachmentBinaryFileIds != null ? communication.EmailAttachmentBinaryFileIds.ToList().AsDelimited( "," ) : string.Empty;
        }

        /// <summary>
        /// Updates the a communication record from control values.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public override void UpdateCommunication( CommunicationDetails communication )
        {
            EnsureChildControls();
            communication.FromName = tbFromName.Text;
            communication.FromEmail = ebFromAddress.Text;
            communication.ReplyToEmail = ebReplyToAddress.Text;
            communication.Subject = tbSubject.Text;
            communication.Message = htmlMessage.Text;
            communication.EmailAttachmentBinaryFileIds = hfAttachments.Value.SplitDelimitedValues().AsIntegerList();
            communication.CCEmails = ebCcAddress.Text;
            communication.BCCEmails = ebBccAddress.Text;
        }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        public Dictionary<int, string> Attachments
        {
            get
            {
                EnsureChildControls();

                var attachments = new Dictionary<int, string>();

                var fileIds = new List<int>();
                hfAttachments.Value.SplitDelimitedValues().ToList().ForEach( v => fileIds.Add(v.AsInteger()));

                new BinaryFileService( new RockContext() ).Queryable()
                    .Where( f => fileIds.Contains( f.Id ) )
                    .Select( f => new
                    {
                        f.Id,
                        f.FileName
                    } )
                    .ToList()
                    .ForEach( f => attachments.Add( f.Id, f.FileName ) );

                return attachments;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        public Email() : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="useSimpleMode">if set to <c>true</c> [use simple mode].</param>
        public Email( bool useSimpleMode) :this()
        {
            UseSimpleMode = useSimpleMode;
        }
        #endregion

        #region CompositeControl Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            tbFromName = new RockTextBox();
            tbFromName.ID = string.Format( "tbFromName_{0}", this.ID );
            tbFromName.Label = "From Name";
            tbFromName.MaxLength = 100;
            Controls.Add( tbFromName );

            lFromName = new RockLiteral();
            lFromName.ID = string.Format( "lFromName_{0}", this.ID );
            lFromName.Label = "From Name";
            Controls.Add( lFromName );

            ebFromAddress = new EmailBox();
            ebFromAddress.ID = string.Format( "ebFromAddress_{0}", this.ID );
            ebFromAddress.Label = "From Address";
            Controls.Add( ebFromAddress );

            lFromAddress = new RockLiteral();
            lFromAddress.ID = string.Format( "lFromAddress_{0}", this.ID );
            lFromAddress.Label = "From Address";
            Controls.Add( lFromAddress );

            ebReplyToAddress = new EmailBox();
            ebReplyToAddress.ID = string.Format( "ebReplyToAddress_{0}", this.ID );
            ebReplyToAddress.Label = "Reply To Address";
            Controls.Add( ebReplyToAddress );

            tbSubject = new RockTextBox();
            tbSubject.ID = string.Format( "tbSubject_{0}", this.ID );
            tbSubject.Label = "Subject";
            tbSubject.Help = "<span class='tip tip-lava'></span>";
            tbSubject.MaxLength = 100;
            Controls.Add( tbSubject );

            htmlMessage = new HtmlEditor();
            htmlMessage.ID = string.Format( "htmlMessage_{0}", this.ID );
            //htmlMessage.AdditionalConfigurations = "autoParagraph: false,";
            htmlMessage.Help = "<span class='tip tip-lava'></span> <span class='tip tip-html'>";
            this.AdditionalMergeFields.ForEach( m => htmlMessage.MergeFields.Add( m ) );
            htmlMessage.Label = "Message";
            htmlMessage.Height = 600;
            Controls.Add( htmlMessage );

            hfAttachments = new HiddenField();
            hfAttachments.ID = string.Format( "hfAttachments_{0}", this.ID );
            Controls.Add( hfAttachments );

            fupEmailAttachments = new FileUploader();
            fupEmailAttachments.ID = string.Format( "fupEmailAttachments_{0}", this.ID );
            fupEmailAttachments.Label = "Attachments";
            fupEmailAttachments.FileUploaded += fupEmailAttachments_FileUploaded;
            Controls.Add( fupEmailAttachments );

            ebCcAddress = new EmailBox();
            ebCcAddress.ID = string.Format( "ebCcAddress_{0}", this.ID );
            ebCcAddress.Label = "CC Address";
            ebCcAddress.Help = "Any address in this field will be copied on the email sent to every recipient.  Lava can be used to access recipient data. <span class='tip tip-lava'></span>";
            Controls.Add( ebCcAddress );

            ebBccAddress = new EmailBox();
            ebBccAddress.ID = string.Format( "ebBccAddress{0}", this.ID );
            ebBccAddress.Label = "Bcc Address";
            ebBccAddress.Help = "Any address in this field will be copied on the email sent to every recipient.  Lava can be used to access recipient data. <span class='tip tip-lava'></span>";
            Controls.Add( ebBccAddress );
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public override string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return tbFromName.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                tbFromName.ValidationGroup = value;
                ebFromAddress.ValidationGroup = value;
                ebReplyToAddress.ValidationGroup = value;
                tbSubject.ValidationGroup = value;
            }
        }

        /// <summary>
        /// On new communication, initializes controls from sender values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void InitializeFromSender( Person sender )
        {
            EnsureChildControls();
            if ( string.IsNullOrEmpty( tbFromName.Text ) )
            {
                tbFromName.Text = sender.FullName;
                lFromName.Text = sender.FullName;
            }

            if ( string.IsNullOrEmpty( ebFromAddress.Text ) )
            {
                ebFromAddress.Text = sender.Email;
                lFromAddress.Text = sender.Email;
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            tbFromName.Required = !IsTemplate;
            ebFromAddress.Required = !IsTemplate;
            tbSubject.Required = !IsTemplate;

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( !UseSimpleMode )
            {
                tbFromName.RenderControl( writer );
                ebFromAddress.RenderControl( writer );
                ebReplyToAddress.RenderControl( writer );
            }
            else
            {
                lFromName.RenderControl( writer );
                lFromAddress.RenderControl( writer );
            }

            tbSubject.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            fupEmailAttachments.RenderControl( writer );
            hfAttachments.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "attachment" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "attachment-content" );
            writer.RenderBeginTag( HtmlTextWriterTag.Ul );

            foreach ( var attachment in Attachments )
            {
                writer.RenderBeginTag( HtmlTextWriterTag.Li );

                writer.AddAttribute( HtmlTextWriterAttribute.Target, "_blank" );
                writer.AddAttribute( HtmlTextWriterAttribute.Href,
                    string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), attachment.Key ) );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.Write( attachment.Value );
                writer.RenderEndTag();

                writer.Write( " " );

                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Onclick, 
                    string.Format( "removeAttachment( this, '{0}', '{1}' );", hfAttachments.ClientID, attachment.Key ) );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-times" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();
                            
                writer.RenderEndTag();  // li
            }

            writer.RenderEndTag();  // ul
            writer.RenderEndTag();  // attachment div


            if ( !UseSimpleMode && AllowCcBcc)
            {
                ebCcAddress.RenderControl( writer );
                ebBccAddress.RenderControl( writer );
            }

            writer.RenderEndTag();  // span6 div

            writer.RenderEndTag();  // row div

            // Html and Text properties
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            htmlMessage.MergeFields.Clear();
            if ( !UseSimpleMode )
            {
                htmlMessage.MergeFields.Add( "GlobalAttribute" );
            }
            htmlMessage.MergeFields.Add( "Rock.Model.Person" );
            if ( !UseSimpleMode )
            {
                htmlMessage.MergeFields.Add( "Communication.Subject|Subject" );
                htmlMessage.MergeFields.Add( "Communication.MediumData.FromName|From Name" );
                htmlMessage.MergeFields.Add( "Communication.MediumData.FromAddress|From Address" );
                htmlMessage.MergeFields.Add( "Communication.MediumData.ReplyTo|Reply To" );
                htmlMessage.MergeFields.Add( "UnsubscribeOption" );
            }
            htmlMessage.RenderControl( writer );

            writer.RenderEndTag();
            writer.RenderEndTag();

            RegisterClientScript();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the FileUploaded event of the fupEmailAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fupEmailAttachments_FileUploaded( object sender, EventArgs e )
        {
            EnsureChildControls();
            var attachmentList = hfAttachments.Value.SplitDelimitedValues().ToList();
            attachmentList.Add( fupEmailAttachments.BinaryFileId.ToString() );
            hfAttachments.Value = attachmentList.AsDelimited( "," );
            fupEmailAttachments.BinaryFileId = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            string script = @"
function removeAttachment(source, hf, fileId)
{
    // Get the attachment list
    var $hf = $('#' + hf);
    var fileIds = $hf.val().split(',');

    // Remove the selected attachment 
    var removeAt = $.inArray(fileId, fileIds);
    fileIds.splice(removeAt, 1);
    $hf.val(fileIds.join());

    // Remove parent <li>
    $(source).closest($('li')).remove();
}";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "removeAttachment", script, true );
        }

        /// <summary>
        /// Called when [communication save].
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public override void OnCommunicationSave( RockContext rockContext )
        {
            var binaryFileIds = hfAttachments.Value.SplitDelimitedValues().AsIntegerList();
            if ( binaryFileIds.Any() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                foreach( var binaryFile in binaryFileService.Queryable()
                    .Where( f => binaryFileIds.Contains( f.Id ) ) )
                {
                    binaryFile.IsTemporary = false;
                }
            }
        }

        #endregion

    }

}
