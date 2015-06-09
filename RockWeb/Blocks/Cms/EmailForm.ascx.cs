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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Web;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Rock.Communication;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Email Form" )]
    [Category( "CMS" )]
    [Description( "Block that takes and HTML form and emails the contents to an address of your choosing." )]

    [EmailField("Receipient Email(s)", "Email addresses (comma delimited) to send the contents to.", true, "", "", 0, "RecipientEmail")]
    [TextField("Subject", "The subject line for the email.", true, "", "", 1)]
    [EmailField("From Email", "The email address to use for the from.", true, "", "", 2)]
    [TextField("From Name", "The name to use for the from address.", true, "", "", 3)]
    [CodeEditorField("HTML Form", "The HTML for the form the user will complete. <span class='tip tip-lava'></span>", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, false, "", "", 5)]
    [CodeEditorField("Message Body", "The email message body. <span class='tip tip-lava'></span>", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, false, "", "", 5)]  
    [CodeEditorField("Response Message", "The message the user will see when they submit the form if no response page if provided.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, "","",6)]
    [LinkedPage("Response Page", "The page the use will be taken to after submitting the form. Use the 'Response Message' field if you just need a simple message.", false, "", "", 7)]
    [TextField("Submit Button Text", "The text to display for the submit button.", true, "Submit", "", 8)]
    [BooleanField("Enable Debug", "Shows the fields available to merge in lava.", false, "", 9)]
    public partial class EmailForm : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
            string fromEmail = GetAttributeValue("FromEmail");
            if ( string.IsNullOrWhiteSpace(fromEmail) )
            {
                SetAttributeValue("FromEmail", GlobalAttributesCache.Value("OrganizationEmail"));
                SaveAttributeValues();
            }

            string fromName = GetAttributeValue("FromName");
            if ( string.IsNullOrWhiteSpace(fromEmail) )
            {
                SetAttributeValue("FromName", GlobalAttributesCache.Value("OrganizationName"));
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowForm();
                pnlEmailForm.Visible = true;

                if ( !string.IsNullOrWhiteSpace(GetAttributeValue("SubmitButtonText")) )
                {
                    btnSubmit.Text = GetAttributeValue("SubmitButtonText");
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowForm();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            SendEmail();
            pnlEmailForm.Visible = false;
        }

        #endregion

        #region Methods

        private void ShowForm()
        {
            var mergeObjects = GlobalAttributesCache.GetMergeFields(CurrentPerson);
            mergeObjects.Add("CurrentPerson", CurrentPerson);

            lEmailForm.Text = GetAttributeValue("HTMLForm").ResolveMergeFields(mergeObjects);
        }

        private void SendEmail()
        {
            // ensure this is not from a bot
            string[] bots = GlobalAttributesCache.Value("EmailExceptionsFilter").Split('|');
            string test = GlobalAttributesCache.Value("EmailExceptionsFilter");
            var serverVarList = Context.Request.ServerVariables;
            bool isBot = false;

            foreach ( var bot in bots )
            {
                string[] botParms = bot.Split('^');
                if ( botParms.Length == 2 )
                {
                    var serverValue = serverVarList[botParms[0]];
                    if ( serverValue != null && serverValue.ToUpper().Contains(botParms[1].ToUpper().Trim()) )
                    {
                        isBot = true;
                    }
                }
            }

            if ( !isBot )
            {
                // create merge objects
                var mergeFields = GlobalAttributesCache.GetMergeFields(CurrentPerson);
                mergeFields.Add("CurrentPerson", CurrentPerson);

                // create merge object for fields
                Regex rgxRockControls = new Regex(@"^ctl\d*\$.*");

                var formFields = new Dictionary<string, object>();
                for ( int i = 0; i < Request.Form.Count; i++ )
                {
                    string formFieldKey = Request.Form.GetKey(i);
                    if ( formFieldKey != null && 
                        formFieldKey.Substring(0, 1) != "_" &&
                        formFieldKey != "searchField_hSearchFilter" &&
                        formFieldKey != "send" &&
                        ! rgxRockControls.IsMatch(formFieldKey))
                    {
                        formFields.Add(formFieldKey, Request.Form[formFieldKey]);
                    }
                }

                mergeFields.Add("FormFields", formFields);

                // get attachments
                List<Attachment> attachments = new List<Attachment>();

                for ( int i = 0; i < Request.Files.Count; i++ )
                {
                    HttpPostedFile attachmentFile = Request.Files[i];

                    string fileName = System.IO.Path.GetFileName(attachmentFile.FileName);

                    Attachment attachment = new Attachment(attachmentFile.InputStream, fileName);

                    attachments.Add(attachment);
                }

                mergeFields.Add("AttachmentCount", attachments.Count);

                // send email
                List<string> recipients = GetAttributeValue("RecipientEmail").Split(',').ToList();
                string message = GetAttributeValue("MessageBody").ResolveMergeFields(mergeFields);
                string fromEmail = GetAttributeValue("FromEmail");
                string subject = GetAttributeValue("Subject");
                Email.Send(fromEmail, subject, recipients, message, ResolveRockUrl( "~/"), ResolveRockUrl( "~~/"), attachments);

                // set response
                if ( !string.IsNullOrWhiteSpace(GetAttributeValue("ResponsePage")) )
                {
                    NavigateToLinkedPage("ResponsePage");
                }

                // display response message
                lResponse.Visible = true;
                lEmailForm.Visible = false;
                lResponse.Text = GetAttributeValue("ResponseMessage").ResolveMergeFields(mergeFields);

                // show debug info
                if ( GetAttributeValue("EnableDebug").AsBoolean() && IsUserAuthorized(Authorization.EDIT) )
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