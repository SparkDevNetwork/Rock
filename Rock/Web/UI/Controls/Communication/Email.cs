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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
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
        private RockTextBox tbFromAddress;
        private RockLiteral lFromName;
        private RockLiteral lFromAddress;
        private RockTextBox tbReplyToAddress;
        private RockTextBox tbSubject;
        private HtmlEditor htmlMessage;
        private RockTextBox tbTextMessage;
        private HiddenField hfAttachments;
        private FileUploader fuAttachments;

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
        /// Gets or sets the medium data.
        /// </summary>
        /// <value>
        /// The medium data.
        /// </value>
        public override Dictionary<string, string> MediumData
        {
            get
            {
                EnsureChildControls();
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
                EnsureChildControls();
                tbFromName.Text = GetDataValue( value, "FromName" );
                tbFromAddress.Text = GetDataValue( value, "FromAddress" );
                tbReplyToAddress.Text = GetDataValue( value, "ReplyTo" );
                tbSubject.Text = GetDataValue( value, "Subject" ); ;
                htmlMessage.Text = GetDataValue( value, "HtmlMessage" );
                tbTextMessage.Text = GetDataValue( value, "TextMessage" );
                hfAttachments.Value = GetDataValue( value, "Attachments" );
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
            Controls.Add( tbFromName );

            lFromName = new RockLiteral();
            lFromName.ID = string.Format( "lFromName_{0}", this.ID );
            lFromName.Label = "From Name";
            Controls.Add( lFromName );

            tbFromAddress = new RockTextBox();
            tbFromAddress.ID = string.Format( "tbFromAddress_{0}", this.ID );
            tbFromAddress.Label = "From Address";
            Controls.Add( tbFromAddress );

            lFromAddress = new RockLiteral();
            lFromAddress.ID = string.Format( "lFromAddress_{0}", this.ID );
            lFromAddress.Label = "From Address";
            Controls.Add( lFromAddress );

            tbReplyToAddress = new RockTextBox();
            tbReplyToAddress.ID = string.Format( "tbReplyToAddress_{0}", this.ID );
            tbReplyToAddress.Label = "Reply To Address";
            Controls.Add( tbReplyToAddress );

            tbSubject = new RockTextBox();
            tbSubject.ID = string.Format( "tbSubject_{0}", this.ID );
            tbSubject.Label = "Subject";
            tbSubject.Help = "<span class='tip tip-lava'></span>";
            Controls.Add( tbSubject );

            htmlMessage = new HtmlEditor();
            htmlMessage.ID = string.Format( "htmlMessage_{0}", this.ID );
            htmlMessage.AdditionalConfigurations = "autoParagraph: false,";
            htmlMessage.Help = "<span class='tip tip-lava'></span> <span class='tip tip-html'>";
            this.AdditionalMergeFields.ForEach( m => htmlMessage.MergeFields.Add( m ) );
            htmlMessage.Label = "Message";
            htmlMessage.Height = 600;
            Controls.Add( htmlMessage );

            tbTextMessage = new RockTextBox();
            tbTextMessage.ID = string.Format( "tbTextMessage_{0}", this.ID );
            tbTextMessage.Label = "Message (Text Version)";
            tbTextMessage.TextMode = TextBoxMode.MultiLine;
            tbTextMessage.Rows = 5;
            tbTextMessage.CssClass = "span12";
            Controls.Add( tbTextMessage );

            hfAttachments = new HiddenField();
            hfAttachments.ID = string.Format( "hfAttachments_{0}", this.ID );
            Controls.Add( hfAttachments );

            fuAttachments = new FileUploader();
            fuAttachments.ID = string.Format( "fuAttachments_{0}", this.ID );
            fuAttachments.Label = "Attachments";
            fuAttachments.FileUploaded += fuAttachments_FileUploaded;
            Controls.Add( fuAttachments );
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
                tbFromAddress.ValidationGroup = value;
                tbSubject.ValidationGroup = value;
            }
        }

        /// <summary>
        /// On new communicaiton, initializes controls from sender values
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

            if ( string.IsNullOrEmpty( tbFromAddress.Text ) )
            {
                tbFromAddress.Text = sender.Email;
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
            tbFromAddress.Required = !IsTemplate;
            tbSubject.Required = !IsTemplate;

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( !UseSimpleMode )
            {
                tbFromName.RenderControl( writer );
                tbFromAddress.RenderControl( writer );
                tbReplyToAddress.RenderControl( writer );
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
                htmlMessage.MergeFields.Add( "Communication.MediumData.FromName|From Name" );
                htmlMessage.MergeFields.Add( "Communication.MediumData.FromAddress|From Address" );
                htmlMessage.MergeFields.Add( "Communication.MediumData.ReplyTo|Reply To" );
                htmlMessage.MergeFields.Add( "UnsubscribeOption" );
            }
            htmlMessage.RenderControl( writer );

            if ( !UseSimpleMode )
            {
                tbTextMessage.RenderControl( writer );
            }
            writer.RenderEndTag();
            writer.RenderEndTag();

            RegisterClientScript();
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
            EnsureChildControls();
            var attachmentList = hfAttachments.Value.SplitDelimitedValues().ToList();
            attachmentList.Add( fuAttachments.BinaryFileId.ToString() );
            hfAttachments.Value = attachmentList.AsDelimited( "," );
            fuAttachments.BinaryFileId = null;
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

        #endregion

    }

}
