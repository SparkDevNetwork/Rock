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
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.MergeTemplates;
using Rock.Security;
using Rock.ViewModels.Blocks.Cms.EmailForm;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Email Form" )]
    [Category( "CMS" )]
    [Description( "Block that takes an HTML form and emails the contents to an address of your choosing." )]

    #region Block Attributes

    [TextField(
        "Recipient Email(s)",
        Description = "Email addresses (comma delimited) to send the contents to.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.RecipientEmail )]

    [TextField(
        "CC Email(s)",
        Description = "CC Email addresses (comma delimited) to send the contents to.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.CCEmail )]

    [TextField(
        "BCC Email(s)",
        Description = "BCC Email addresses (comma delimited) to send the contents to.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.BCCEmail )]

    [TextField(
        "Subject",
        Description = "The subject line for the email.",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.Subject )]

    [TextField(
        "From Email",
        Description = "The email to use for the from address.",
        IsRequired = true,
        Order = 4,
        Key = AttributeKey.FromEmail )]

    [TextField(
        "From Name",
        Description = "The name to use for the from address.",
        IsRequired = true,
        Order = 5,
        Key = AttributeKey.FromName )]

    [CodeEditorField(
        "HTML Form",
        Description = "The HTML for the form the user will complete.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = HTML_FORM_DEFAULT_VALUE,
        Order = 6,
        Key = AttributeKey.HTMLForm )]

    [CodeEditorField(
        "Message Body",
        Description = "The email message body.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = MESSAGE_BODY_DEFAULT_VALUE,
        Order = 7,
        Key = AttributeKey.MessageBody )]

    [CodeEditorField(
        "Response Message",
        Description = "The message the user will see when they submit the form if no response page if provided. Lava merge fields are available for you to use in your message.",
        EditorMode = CodeEditorMode.Lava,
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

    [BooleanField(
        "Disable Captcha Support",
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        Order = 15,
        Key = AttributeKey.DisableCaptchaSupport )]

    [LavaCommandsField(
        "Enabled Lava Commands",
        Description = "The Lava commands that should be enabled for this HTML block.",
        IsRequired = false,
        Order = 16,
        Key = AttributeKey.EnabledLavaCommands )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "F24352C3-1D96-4BF2-9317-6BA8EBE61633" )]
    [Rock.SystemGuid.BlockTypeGuid( "956174B7-109C-4821-841A-AC1830B97A13" )]
    public class EmailForm : RockBlockType
    {
        #region Keys

        private static class NavigationUrlKey
        {
            public const string ResponsePage = "ResponsePage";
        }

        #endregion Keys

        #region Attribute Keys
        private static class AttributeKey
        {
            public const string RecipientEmail = "RecipientEmail";
            public const string CCEmail = "CCEmail";
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
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
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
        <input type=""text"" class=""form-control"" id=""firstname"" name=""FirstName"" placeholder=""First Name"" required />
    {% endif %}
</div>
<div class=""form-group"">
    <label for=""lastname"">Last Name</label>
   
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.LastName }}</p>
        <input type=""hidden"" id=""lastname"" name=""LastName"" value=""{{ CurrentPerson.LastName }}"" />
    {% else %}
        <input type=""text"" class=""form-control"" id=""lastname"" name=""LastName"" placeholder=""Last Name"" required />
    {% endif %}
</div>
<div class=""form-group"">
    <label for=""email"">Email</label>
    {% if CurrentPerson %}
        <input type=""email"" class=""form-control"" id=""email"" name=""Email"" value=""{{ CurrentPerson.Email }}"" placeholder=""Email"" required />
    {% else %}
        <input type=""email"" class=""form-control"" id=""email"" name=""Email"" placeholder=""Email"" required />
    {% endif %}
</div>
<div class=""form-group"">
    <label for=""message"">Message</label>
    <textarea id=""message"" rows=""4"" class=""form-control"" name=""Message"" placeholder=""Message"" required></textarea>
</div>
<div class=""form-group"">
    <label for=""attachment"">Attachment</label>
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

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new EmailFormBag();
            var errorBuilder = new StringBuilder();
            var block = new BlockService( RockContext ).Get( BlockId );

            block.LoadAttributes();

            // Set default from email/name from global attributes if they are not already set
            var fromEmail = GetAttributeValue( AttributeKey.FromEmail );
            if ( string.IsNullOrWhiteSpace( fromEmail ) )
            {
                block.SetAttributeValue( AttributeKey.FromEmail, GlobalAttributesCache.Value( "OrganizationEmail" ) );
                block.SaveAttributeValues();
            }

            var fromName = GetAttributeValue( AttributeKey.FromName );
            if ( string.IsNullOrWhiteSpace( fromName ) )
            {
                block.SetAttributeValue( AttributeKey.FromName, GlobalAttributesCache.Value( "OrganizationName" ) );
                block.SaveAttributeValues();
            }

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.RecipientEmail ) ) )
            {
                errorBuilder.Append( "<div class='alert alert-warning'>A recipient has not been provided for this form.</div>" );
            }

            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.Subject ) ) )
            {
                errorBuilder.Append( "<div class='alert alert-warning'>A subject has not been provided for this form.</div>" );
            }

            var content = GetEmailFormContent();

            if ( string.IsNullOrWhiteSpace( content ) )
            {
                errorBuilder.Append( "<strong>Missing HTML Content</strong> <p>The Email Form block failed to initialize due to empty HTML content. Ensure the HTMLForm attribute is properly configured. </p>" );
            }

            box.ErrorMessage = errorBuilder.ToString();
            box.Content = content;
            box.DisableCaptchaSupport = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() );
            box.SubmitButtonText = GetAttributeValue( AttributeKey.SubmitButtonText ) ?? "Submit";
            box.SubmitButtonCssClass = GetAttributeValue( AttributeKey.SubmitButtonCssClass ) ?? "btn btn-primary";
            box.SubmitButtonWrapCssClass = GetAttributeValue( AttributeKey.SubmitButtonWrapCssClass );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            return GetEmailFormContent();
        }

        /// <summary>
        /// Gets the HTML Content for the block.
        /// </summary>
        private string GetEmailFormContent()
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            string htmlForm = GetAttributeValue( AttributeKey.HTMLForm ).ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.EnabledLavaCommands ) );
            return htmlForm ?? string.Empty;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ResponsePage] = this.GetLinkedPageUrl( AttributeKey.ResponsePage )
            };
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        [BlockAction]
        public BlockActionResult SendEmail( EmailFormRequestBag bag )
        {
            bool disableCaptcha = Captcha.CaptchaService.ShouldDisableCaptcha( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() );

            if ( !disableCaptcha && !RequestContext.IsCaptchaValid )
            {
                return ActionBadRequest( "CAPTCHA verification failed. Please try again." );
            }

            bool isBot = false;
            var filterString = GlobalAttributesCache.Value( "EmailExceptionsFilter" );
            var serverVarList = System.Web.HttpContext.Current.Request.ServerVariables;

            var bots = filterString.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( var bot in bots )
            {
                var botParms = bot.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                if ( botParms.Length == 2 )
                {
                    string serverValue = serverVarList[botParms[0]];
                    if ( !string.IsNullOrWhiteSpace( serverValue ) && serverValue.ToUpper().Contains( botParms[1].ToUpper().Trim() ) )
                    {
                        isBot = true;
                        break;
                    }
                }
            }

            if ( isBot )
            {
                return ActionBadRequest( "You appear to be a computer. Check the global attribute 'Email Exceptions Filter' if you are getting this in error." );
            }

            var formFields = bag.FormFields.ToDictionary( k => k.Key, v => ( object ) v.Value );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "FormFields", formFields );

            var binaryFileService = new BinaryFileService( RockContext );
            var attachments = new List<BinaryFile>();
            var binaryFileType = BinaryFileTypeCache.Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() );

            // Form Attachments
            foreach ( var guid in bag.AttachmentGuids ?? new List<Guid>() )
            {
                var binaryFile = binaryFileService.Get( guid );
                if ( binaryFile != null )
                {
                    binaryFile.BinaryFileTypeId = binaryFileType.Id;
                    binaryFile.IsTemporary = false;
                    attachments.Add( binaryFile );
                }
            }

            RockContext.SaveChanges();

            mergeFields.Add( "AttachmentCount", attachments.Count );

            var enabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

            var message = new RockEmailMessage
            {
                EnabledLavaCommands = enabledLavaCommands
            };

            var personService = new PersonService( RockContext );
            var saveCommunicationHistory = GetAttributeValue( AttributeKey.SaveCommunicationHistory ).AsBoolean();

            var recipientEmails = GetAttributeValue( AttributeKey.RecipientEmail )
                .Split( ',' )
                .Select( e => e.Trim() )
                .ToList();

            foreach ( var emailAddress in recipientEmails )
            {
                var email = emailAddress.ResolveMergeFields( mergeFields, enabledLavaCommands );
                
                RockEmailMessageRecipient recipient;
                if ( saveCommunicationHistory )
                {
                    var person = personService.GetPersonFromEmailAddress( email, false );
                    if ( person != null )
                    {
                        recipient = new RockEmailMessageRecipient( person, mergeFields );
                    }
                    else
                    {
                        recipient = RockEmailMessageRecipient.CreateAnonymous( email, mergeFields );
                    }
                }
                else
                {
                    recipient = RockEmailMessageRecipient.CreateAnonymous( email, mergeFields );
                }
                
                message.AddRecipient( recipient );
            }

            message.CCEmails = GetAttributeValue( AttributeKey.CCEmail ).ResolveMergeFields( mergeFields, enabledLavaCommands ).Split( ',' ).ToList();
            message.BCCEmails = GetAttributeValue( AttributeKey.BCCEmail ).ResolveMergeFields( mergeFields, enabledLavaCommands ).Split( ',' ).ToList();
            message.FromEmail = GetAttributeValue( AttributeKey.FromEmail ).ResolveMergeFields( mergeFields, enabledLavaCommands );
            message.FromName = GetAttributeValue( AttributeKey.FromName ).ResolveMergeFields( mergeFields, enabledLavaCommands );
            message.Subject = GetAttributeValue( AttributeKey.Subject ).ResolveMergeFields( mergeFields, enabledLavaCommands );
            message.Message = GetAttributeValue( AttributeKey.MessageBody ).ResolveMergeFields( mergeFields, enabledLavaCommands );

            message.AppRoot = RequestContext.RootUrlPath.EnsureTrailingForwardslash();
            message.ThemeRoot = RequestContext.RootUrlPath + RequestContext.ResolveRockUrl( "~~/" );

            message.CreateCommunicationRecord = saveCommunicationHistory;

            foreach ( var attachment in attachments )
            {
                message.Attachments.Add( attachment );
            }

            message.Send();

            var responseMergeFields = mergeFields;
            string responseMessage = GetAttributeValue( AttributeKey.ResponseMessage ).ResolveMergeFields( responseMergeFields, enabledLavaCommands );
            if ( GetAttributeValue( AttributeKey.EnableDebug ).AsBoolean() && BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                responseMessage += MergeTemplateType.GetDefaultLavaDebugInfo( new List<object>(), responseMergeFields );
            }

            return ActionOk( new EmailFormResponseBag { ResponseHtml = responseMessage } );
        }

        #endregion Methods
    }
}