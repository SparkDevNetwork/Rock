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

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Email Form" )]
    [Category( "CMS" )]
    [Description( "Block that takes and HTML form and emails the contents to an address of your choosing." )]
    
    [EmailField("Receipient Email", "Email address to send the contents to.", true, "", "", 0)]
    [TextField("Subject", "The subject line for the email.", true, "", "", 1)]
    [EmailField("From Email", "The email address to use for the from.", true, "", "", 2)]
    [TextField("From Name", "The name to use for the from address.", true, "", "", 3)]
    [CodeEditorField("Message Body", "The email message body. <span class='tip tip-lava'></span>", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, false, "", "", 4)]
    [CodeEditorField("HTML Form", "The HTML for the form the user will complete. <span class='tip tip-lava'></span>", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, false, "", "", 5)]
    [CodeEditorField("Response Message", "The message the user will see when they submit the form if no response page if provided.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, "","",6)]
    [LinkedPage("Response Page", "The page the use will be taken to after submitting the form. Use the 'Response Message' field if you just need a simple message.", false, "", "", 7)]
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
            }
            else
            {
                SendEmail();
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
                var mergeObjects = GlobalAttributesCache.GetMergeFields(CurrentPerson);
                mergeObjects.Add("CurrentPerson", CurrentPerson);

                // create merge object for fields
                var formFields = new Dictionary<string, object>();
                foreach ( var formField in Request.Params )
                {
                    string test2 = "";
                    //formFields.Add(formField.)
                }

                // send email
                //todo

                // set response
                if ( !string.IsNullOrWhiteSpace(GetAttributeValue("ResponsePage")) )
                {
                    NavigateToLinkedPage("ResponsePage");
                }

                // display response message
                lResponse.Visible = true;
                lEmailForm.Visible = false;
                lResponse.Text = GetAttributeValue("ResponseMessage").ResolveMergeFields(mergeObjects);
            }
            else
            {
                lResponse.Visible = true;
                lEmailForm.Visible = false;
                lResponse.Text = "You appear to be a computer.";
            }
        }

        #endregion
    }
}