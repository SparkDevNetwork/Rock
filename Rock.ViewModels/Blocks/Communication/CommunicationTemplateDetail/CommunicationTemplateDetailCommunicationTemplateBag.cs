using System;
using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationTemplateDetail;
using Rock.Model;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationTemplateDetail
{
    /// <summary>
    /// Represents the communication template details for the Communication Template Detail block.
    /// </summary>
    public class CommunicationTemplateDetailCommunicationTemplateBag
    {
        /// <summary>
        /// Gets or sets the name of the communication template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the template.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this communication template is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CSS inlining is enabled.
        /// </summary>
        public bool IsCssInliningEnabled { get; set; }

        /// <summary>
        /// Gets or sets the binary file for the template preview.
        /// </summary>
        public ListItemBag ImageFile { get; set; }

        /// <summary>
        /// Gets or sets the binary file for the template logo.
        /// </summary>
        public ListItemBag LogoBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a starter template.
        /// </summary>
        public bool IsStarter { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the sender.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply-to email address.
        /// </summary>
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the CC email addresses.
        /// </summary>
        public string CcEmails { get; set; }

        /// <summary>
        /// Gets or sets the BCC email addresses.
        /// </summary>
        public string BccEmails { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Lava fields.
        /// </summary>
        public Dictionary<string, string> LavaFields { get; set; }

        /// <summary>
        /// Gets or sets the system phone number used for SMS sending.
        /// </summary>
        public ListItemBag SmsFromSystemPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the SMS message content.
        /// </summary>
        public string SmsMessage { get; set; }

        /// <summary>
        /// Gets or sets the title of the push notification.
        /// </summary>
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message content of the push notification.
        /// </summary>
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets the push open action.
        /// </summary>
        public PushOpenAction PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the push open message.
        /// </summary>
        public string PushOpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the push open message structured content JSON.
        /// </summary>
        public string PushOpenMessageJson { get; set; }

        /// <summary>
        /// Gets or sets the push mobile application.
        /// </summary>
        public ListItemBag PushMobileApplication { get; set; }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        public List<ListItemBag> Attachments { get; set; }

        /// <summary>
        /// Gets or sets the query string parameters for the push mobile page.
        /// </summary>
        public Dictionary<string, string> PushMobilePageQueryString { get; set; }

        /// <summary>
        /// Gets or sets the URL for the push notification.
        /// </summary>
        public string PushUrl { get; set; }

        /// <summary>
        /// Gets or sets the mobile page for the push notification.
        /// </summary>
        public PageRouteValueBag PushMobilePage { get; set; }
        
        /// <summary>
        /// Gets or sets the communication template version.
        /// </summary>
        public CommunicationTemplateVersion Version { get; set; }
    }
}
