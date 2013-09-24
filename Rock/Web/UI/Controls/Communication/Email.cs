//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls.Communication
{
    /// <summary>
    /// Email Communication Channel control
    /// </summary>
    public class Email : ChannelControl
    {

        #region UI Controls

        private LabeledTextBox tbFromName;
        private LabeledTextBox tbFromAddress;
        private LabeledTextBox tbReplyToAddress;
        private LabeledTextBox tbSubject;
        private HtmlEditor htmlMessage;
        private LabeledTextBox tbTextMessage;
        private HiddenField hfAttachments;
        private FileUploader fuAttachments;
        private Dictionary<int, LinkButton> removeButtons;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Email" /> class.
        /// </summary>
        public Email()
        {
            tbFromName = new LabeledTextBox();
            tbFromAddress = new LabeledTextBox();
            tbReplyToAddress = new LabeledTextBox();
            tbSubject = new LabeledTextBox();
            htmlMessage = new HtmlEditor();
            tbTextMessage = new LabeledTextBox();
            hfAttachments = new HiddenField();
            fuAttachments = new FileUploader();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>
        /// The channel data.
        /// </value>
        public override Dictionary<string, string> ChannelData
        {
            get
            {
                var data = new Dictionary<string, string>();
                data.Add( "FromName", tbFromName.Text );
                data.Add( "FromAddress", tbFromAddress.Text );
                data.Add( "ReplyTo", tbReplyToAddress.Text );
                data.Add( "Subject", tbSubject.Text );
                data.Add( "HtmlMessage", htmlMessage.Text );
                data.Add( "TextMessage", tbTextMessage.Text );
                data.Add( "Attachments", hfAttachments.Value );
                return data;
            }

            set
            {
                tbFromName.Text = GetDataValue( value, "FromName" );
                tbFromAddress.Text = GetDataValue( value, "FromAddress" );
                tbReplyToAddress.Text = GetDataValue( value, "ReplyTo" );
                tbSubject.Text = GetDataValue( value, "Subject" ); ;
                htmlMessage.Text = GetDataValue( value, "HtmlMessage" );
                tbTextMessage.Text = GetDataValue( value, "TextMessage" );
                hfAttachments.Value = GetDataValue( value, "Attachments" );

                Attachments = GetAttachments();
            }
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
                return ViewState["Attachments"] as Dictionary<int, string> ?? new Dictionary<int, string>();
            }

            set
            {
                ViewState["Attachments"] = value;
                RecreateChildControls();
            }
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

            tbFromName.ID = string.Format( "tbFromName_{0}", this.ID );
            tbFromName.Label = "From Name";
            Controls.Add( tbFromName );

            tbFromAddress.ID = string.Format( "tbFromAddress_{0}", this.ID );
            tbFromAddress.Label = "From Address";
            tbFromAddress.Required = true;
            Controls.Add( tbFromAddress );

            tbReplyToAddress.ID = string.Format( "tbReplyToAddress_{0}", this.ID );
            tbReplyToAddress.Label = "Reply To Address";
            Controls.Add( tbReplyToAddress );

            tbSubject.ID = string.Format( "tbSubject_{0}", this.ID );
            tbSubject.Label = "Subject";
            Controls.Add( tbSubject );

            htmlMessage.ID = string.Format( "htmlMessage_{0}", this.ID );
            htmlMessage.MergeFields.Clear();
            htmlMessage.MergeFields.Add( "GlobalAttribute" );
            htmlMessage.MergeFields.Add( "Rock.Model.Person" );
            this.AdditionalMergeFields.ForEach( m => htmlMessage.MergeFields.Add( m ) );
            htmlMessage.Label = "Message";
            Controls.Add( htmlMessage );

            tbTextMessage.ID = string.Format( "tbTextMessage_{0}", this.ID );
            tbTextMessage.Label = "Message (Text Version)";
            tbTextMessage.TextMode = TextBoxMode.MultiLine;
            tbTextMessage.Rows = 5;
            tbTextMessage.CssClass = "span12";
            Controls.Add( tbTextMessage );

            hfAttachments.ID = string.Format( "hfAttachments_{0}", this.ID );
            Controls.Add( hfAttachments );

            fuAttachments.ID = string.Format( "fuAttachments_{0}", this.ID );
            fuAttachments.Label = "Attachments";
            fuAttachments.FileUploaded += fuAttachments_FileUploaded;
            Controls.Add( fuAttachments );

            removeButtons = new Dictionary<int, LinkButton>();
            foreach ( var attachment in Attachments )
            {
                var lbRemove = new LinkButton();
                lbRemove.ID = string.Format( "lbRemove_{0}_{1}", this.ID, attachment.Key );
                lbRemove.Click += lbRemove_Click;
                Controls.Add( lbRemove );
                removeButtons.Add( attachment.Key, lbRemove );
            }
        }

        /// <summary>
        /// On new communicaiton, initializes controls from sender values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void InitializeFromSender( Person sender )
        {
            if ( string.IsNullOrEmpty( tbFromName.Text ) )
            {
                tbFromName.Text = sender.FullName;
            }

            if ( string.IsNullOrEmpty( tbFromAddress.Text ) )
            {
                tbFromAddress.Text = sender.Email;
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbFromName.RenderControl( writer );
            tbFromAddress.RenderControl( writer );
            tbReplyToAddress.RenderControl( writer );
            tbSubject.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            fuAttachments.RenderControl( writer );
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
                    string.Format( "{0}GetFile.ashx?{1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), attachment.Key ) );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.Write( attachment.Value );
                writer.RenderEndTag();

                writer.Write( " " );

                removeButtons[attachment.Key].RenderBeginTag(writer);
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "icon-remove" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                removeButtons[attachment.Key].RenderEndTag( writer );
                            
                writer.RenderEndTag();  // li
            }

            writer.RenderEndTag();  // ul
            writer.RenderEndTag();  // attachment div

            writer.RenderEndTag();  // span6 div

            writer.RenderEndTag();  // row-fluid div

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            htmlMessage.RenderControl( writer );
            tbTextMessage.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the FileUploaded event of the fuAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fuAttachments_FileUploaded( object sender, EventArgs e )
        {
            var attachmentList = hfAttachments.Value.SplitDelimitedValues().ToList();
            attachmentList.Add( fuAttachments.BinaryFileId.ToString() );
            hfAttachments.Value = attachmentList.AsDelimited( "," );

            fuAttachments.BinaryFileId = null;

            Attachments = GetAttachments();
        }

        /// <summary>
        /// Handles the Click event of the lbRemove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbRemove_Click( object sender, EventArgs e )
        {
            LinkButton lbRemove = sender as LinkButton;
            var idParts = lbRemove.ID.Split( '_' );
            if ( idParts.Length == 3 )
            {
                hfAttachments.Value = hfAttachments.Value.SplitDelimitedValues()
                    .ToList()
                    .Where( i => i != idParts[2] )
                    .Select( i => i.ToString() )
                    .ToList()
                    .AsDelimited( "," );

                Attachments = GetAttachments();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the attachment list.
        /// </summary>
        private Dictionary<int, string> GetAttachments()
        {
            var attachments = new Dictionary<int, string>();

            var service = new BinaryFileService();
            foreach ( string FileId in hfAttachments.Value.SplitDelimitedValues() )
            {
                int binaryFileId = int.MinValue;
                if ( int.TryParse( FileId, out binaryFileId ) )
                {
                    string fileName = service.Queryable()
                        .Where( f => f.Id == binaryFileId )
                        .Select( f => f.FileName )
                        .FirstOrDefault();

                    if ( fileName != null )
                    {
                        attachments.Add( binaryFileId, fileName );
                    }
                }
            }

            return attachments;
        }

        #endregion

    }

}
