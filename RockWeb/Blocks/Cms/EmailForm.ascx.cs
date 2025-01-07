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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Email Form" )]
    [Category( "CMS" )]
    [Description( "Block that takes and HTML form and emails the contents to an address of your choosing." )]

    #region Block Attributes

    [TextField(
        "Recipient Email(s)",
        Description = "Email addresses (comma delimited) to send the contents to.",
        IsRequired = true,
        Order = 0,
        Key =  AttributeKey.RecipientEmail )]

    [TextField(
        "CC Email(s)",
        Description = "CC Email addresses (comma delimited) to send the contents to. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.CCEmail )]

    [TextField(
        "BCC Email(s)",
        Description = "BCC Email addresses (comma delimited) to send the contents to. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.BCCEmail )]

    [TextField(
        "Subject",
        Description = "The subject line for the email. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.Subject )]

    [TextField(
        "From Email",
        Description = "The email address to use for the from. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 4,
        Key = AttributeKey.FromEmail )]

    [TextField(
        "From Name",
        Description = "The name to use for the from address. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 5,
        Key = AttributeKey.FromName )]

    [CodeEditorField(
        "HTML Form",
        Description = "The HTML for the form the user will complete. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = HTML_FORM_DEFAULT_VALUE,
        Order = 6,
        Key = AttributeKey.HTMLForm )]
    [CodeEditorField(
        "Message Body",
        Description = "The email message body. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = MESSAGE_BODY_DEFAULT_VALUE,
        Order = 7,
        Key = AttributeKey.MessageBody )]
    [CodeEditorField(
        "Response Message",
        Description = "The message the user will see when they submit the form if no response page if provided. Lava merge fields are available for you to use in your message.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = @"<div class=""alert alert-info"">
    Thank you for your response. We appreciate your feedback!
</div>",
       Order = 8,
       Key = AttributeKey.ResponseMessage )]
    [LinkedPage(
        "Response Page",
        Description = "The page the use will be taken to after submitting the form. Use the 'Response Message' field if you just need a simple message.",
        IsRequired = false,
        Order = 9,
        Key = AttributeKey.ResponsePage )]
    [TextField(
        "Submit Button Text",
        Description = "The text to display for the submit button.",
        IsRequired = true,
        DefaultValue = "Submit",
        Order = 10,
        Key = AttributeKey.SubmitButtonText )]
    [TextField(
        "Submit Button Wrap CSS Class",
        Description = "CSS class to add to the div wrapping the button.",
        IsRequired = false,
        Order = 11,
        Key = AttributeKey.SubmitButtonWrapCssClass )]
    [TextField(
        "Submit Button CSS Class",
        Description = "The CSS class add to the submit button.",
        IsRequired = false,
        DefaultValue = "btn btn-primary",
        Order = 12,
        Key = AttributeKey.SubmitButtonCssClass )]
    [BooleanField(
        "Enable Debug",
        Description = "Shows the fields available to merge in lava.",
        DefaultBooleanValue = false,
        Order = 13,
        Key = AttributeKey.EnableDebug )]
    [BooleanField(
        "Save Communication History",
        Description = "Should a record of this communication be saved to the recipient's profile",
        DefaultBooleanValue = false,
        Order = 14,
        Key = AttributeKey.SaveCommunicationHistory )]
    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this HTML block.",
        IsRequired = false,
        Order = 15,
        Key = AttributeKey.EnabledLavaCommands )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "48253494-F8A0-4DD8-B645-6CB481CEB7BD" )]
    public partial class EmailForm : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RecipientEmail = "RecipientEmail";
            public const string CCEmail = "CCEmail";
            public const string FileEditorPage = "FileEditorPage";
            public const string BCCEmail = "BCCEmail";
            public const string Subject = "Subject";
            public const string FromEmail = "FromEmail";
            public const string FromName = "FromName";
            public const string HTMLForm = "HTMLForm";
            public const string MessageBody = "MessageBody";
            public const string ResponseMessage = "ResponseMessage";
            public const string ResponsePage = "ResponsePage";
            public const string SubmitButtonText = "SubmitButtonText";
            public const string SubmitButtonWrapCssClass = "SubmitButtonWrapCssClass";
            public const string SubmitButtonCssClass = "SubmitButtonCssClass";
            public const string EnableDebug = "EnableDebug";
            public const string SaveCommunicationHistory = "SaveCommunicationHistory";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        #endregion Attribute Keys

        #region Attribute Constants

        const string HTML_FORM_DEFAULT_VALUE = @"{% if CurrentPerson %}
    {{ CurrentPerson.NickName }}, could you please complete the form below.
{% else %}
    Please complete the form below.
{% endif %}

<div class=""form-group"">
    <label for=""firstname"">First Name</label>
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.NickName }}</p>
        <input type=""hidden"" id=""firstname"" name=""FirstName"" value=""{{ CurrentPerson.NickName }}"" />
    {% else %}
        <input class=""form-control"" id=""firstname"" name=""FirstName"" placeholder=""First Name"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""lastname"">Last Name</label>
    
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.LastName }}</p>
        <input type=""hidden"" id=""lastname"" name=""LastName"" value=""{{ CurrentPerson.LastName }}"" />
    {% else %}
        <input class=""form-control"" id=""lastname"" name=""LastName"" placeholder=""Last Name"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""email"">Email</label>
    {% if CurrentPerson %}
        <input class=""form-control"" id=""email"" name=""Email"" value=""{{ CurrentPerson.Email }}"" placeholder=""Email"" required />
    {% else %}
        <input class=""form-control"" id=""email"" name=""Email"" placeholder=""Email"" required />
    {% endif %}
</div>

<div class=""form-group"">
    <label for=""email"">Message</label>
    <textarea id=""message"" rows=""4"" class=""form-control"" name=""Message"" placeholder=""Message"" required></textarea>
</div>

<div class=""form-group"">
    <label for=""email"">Attachment</label>
    <input type=""file"" id=""attachment"" name=""attachment"" /> <br />
    <input type=""file"" id=""attachment2"" name=""attachment2"" />
</div>
";

        const string MESSAGE_BODY_DEFAULT_VALUE = @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    A email form has been submitted. Please find the information below:
</p>

{% for field in FormFields %}
    {% assign fieldParts = field | PropertyToKeyValue %}

    <strong>{{ fieldParts.Key | Humanize | Capitalize }}</strong>: {{ fieldParts.Value }} <br/>
{% endfor %}

<p>&nbsp;</p>

{{ 'Global' | Attribute:'EmailFooter' }}";

        #endregion Attribute Constants

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            // provide some good default values for from email / from name
            string fromEmail = GetAttributeValue( AttributeKey.FromEmail );
            if ( string.IsNullOrWhiteSpace( fromEmail ) )
            {
                SetAttributeValue( AttributeKey.FromEmail, GlobalAttributesCache.Value( "OrganizationEmail" ) );
                SaveAttributeValues();
            }

            string fromName = GetAttributeValue( AttributeKey.FromName );
            if ( string.IsNullOrWhiteSpace( fromEmail ) )
            {
                SetAttributeValue( AttributeKey.FromName, GlobalAttributesCache.Value( "OrganizationName" ) );
                SaveAttributeValues();
            }

            Page.Form.Enctype = "multipart/form-data";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowForm();
                pnlEmailForm.Visible = true;

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.SubmitButtonText ) ) )
                {
                    btnSubmit.Text = GetAttributeValue( AttributeKey.SubmitButtonText );
                }

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.SubmitButtonWrapCssClass ) ) )
                {
                    divButtonWrap.Attributes.Add( "class", GetAttributeValue( AttributeKey.SubmitButtonWrapCssClass ) );
                }

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.SubmitButtonCssClass ) ) )
                {
                    btnSubmit.CssClass = GetAttributeValue( AttributeKey.SubmitButtonCssClass );
                }

                if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.RecipientEmail ) ) )
                {
                    lError.Text = "<div class='alert alert-warning'>A recipient has not been provided for this form.</div>";
                }

                if ( string.IsNullOrWhiteSpace( GetAttributeValue( "Subject" ) ) )
                {
                    lError.Text += "<div class='alert alert-warning'>A subject has not been provided for this form.</div>";
                }
            }

            RockPage.AddScriptLink( "~/Scripts/jquery.visible.min.js" );

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowForm();
        }

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            SendEmail();
            pnlEmailForm.Visible = false;

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "ScrollToMessage",
                "scrollToMessage();",
                true );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the form.
        /// </summary>
        private void ShowForm()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

            lEmailForm.Text = GetAttributeValue( AttributeKey.HTMLForm ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        private void SendEmail()
        {
            // ensure this is not from a bot
            string[] bots = GlobalAttributesCache.Value( "EmailExceptionsFilter" ).Split( '|' );
            string test = GlobalAttributesCache.Value( "EmailExceptionsFilter" );
            var serverVarList = Context.Request.ServerVariables;
            bool isBot = false;

            foreach ( var bot in bots )
            {
                string[] botParms = bot.Split( '^' );
                if ( botParms.Length == 2 )
                {
                    var serverValue = serverVarList[botParms[0]];
                    if ( serverValue != null && serverValue.ToUpper().Contains( botParms[1].ToUpper().Trim() ) )
                    {
                        isBot = true;
                    }
                }
            }

            if ( !isBot )
            {
                var message = new RockEmailMessage();
                message.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

                // create merge objects
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                // create merge object for fields
                Regex rgxRockControls = new Regex( @"^ctl\d*\$.*" );
                var formFields = new Dictionary<string, object>();
                for ( int i = 0; i < Request.Form.Count; i++ )
                {
                    string formFieldKey = Request.Form.GetKey( i );
                    if ( formFieldKey != null &&
                        formFieldKey.Substring( 0, 1 ) != "_" &&
                        formFieldKey != "searchField_hSearchFilter" &&
                        formFieldKey != "send" &&
                        !rgxRockControls.IsMatch( formFieldKey ) )
                    {
                        formFields.Add( formFieldKey, Request.Form[formFieldKey] );
                    }
                }

                mergeFields.Add( "FormFields", formFields );

                // get attachments
                var rockContext = new RockContext();
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() );
                for ( int i = 0; i < Request.Files.Count; i++ )
                {
                    var uploadedFile = Request.Files[i];
                    if ( uploadedFile.ContentLength > 0 && uploadedFile.FileName.IsNotNullOrWhiteSpace() )
                    {
                        var binaryFile = new BinaryFile();
                        binaryFileService.Add( binaryFile );
                        binaryFile.BinaryFileTypeId = binaryFileType.Id;
                        binaryFile.IsTemporary = false;
                        binaryFile.MimeType = uploadedFile.ContentType;
                        binaryFile.FileSize = uploadedFile.ContentLength;
                        binaryFile.FileName = Path.GetFileName( uploadedFile.FileName );
                        binaryFile.ContentStream = uploadedFile.InputStream;
                        rockContext.SaveChanges();

                        message.Attachments.Add( binaryFileService.Get( binaryFile.Id ) );
                    }
                }

                mergeFields.Add( "AttachmentCount", message.Attachments.Count );

                // send email
                foreach ( string recipient in GetAttributeValue( AttributeKey.RecipientEmail ).Split( ',' ).ToList() )
                {
                    message.AddRecipient( RockEmailMessageRecipient.CreateAnonymous( recipient, mergeFields ) );
                }

                message.CCEmails = GetAttributeValue( AttributeKey.CCEmail ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) ).Split( ',' ).ToList();
                message.BCCEmails = GetAttributeValue( AttributeKey.BCCEmail ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) ).Split( ',' ).ToList();
                message.FromEmail = GetAttributeValue( AttributeKey.FromEmail ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
                message.FromName = GetAttributeValue( AttributeKey.FromName ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
                message.Subject = GetAttributeValue( AttributeKey.Subject );
                message.Message = GetAttributeValue( AttributeKey.MessageBody );
                message.AppRoot = ResolveRockUrl( "~/" );
                message.ThemeRoot = ResolveRockUrl( "~~/" );
                message.CreateCommunicationRecord = GetAttributeValue( AttributeKey.SaveCommunicationHistory ).AsBoolean();
                message.Send();

                // set response
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.ResponsePage ) ) )
                {
                    NavigateToLinkedPage( AttributeKey.ResponsePage );
                }

                // display response message
                lResponse.Visible = true;
                lEmailForm.Visible = false;
                lResponse.Text = GetAttributeValue( AttributeKey.ResponseMessage ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );

                // show debug info
                if ( GetAttributeValue( AttributeKey.EnableDebug ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
            else
            {
                lResponse.Visible = true;
                lEmailForm.Visible = false;
                lResponse.Text = "You appear to be a computer. Check the global attribute 'Email Exceptions Filter' if you are getting this in error.";
            }
        }

        #endregion
    }
}