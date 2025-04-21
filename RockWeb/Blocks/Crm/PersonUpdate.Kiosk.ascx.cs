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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Communication;
using Rock.Security;
using System.Text.RegularExpressions;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Person Update - Kiosk (deprecated)" )]
    [RockObsolete( "1.15" )]
    [Obsolete( "This block type has been deprecated." )]
    [Category( "CRM" )]
    [Description( "Block used to update a person's information from a kiosk." )]

    #region Block Attributes

    [LinkedPage(
        "Homepage",
        Key = AttributeKey.Homepage,
        Description = "Homepage of the kiosk.",
        IsRequired = true,
        Order = 0 )]

    [IntegerField(
        "Minimum Phone Number Length",
        Key = AttributeKey.MinimumPhoneNumberLength,
        Description = "Minimum length for phone number searches (defaults to 4).",
        IsRequired = false,
        DefaultIntegerValue = 4,
        Order = 1 )]

    [IntegerField(
        "Maximum Phone Number Length",
        Key = AttributeKey.MaximumPhoneNumberLength,
        Description = "Maximum length for phone number searches (defaults to 10).",
        IsRequired = false,
        DefaultIntegerValue = 10,
        Order = 2 )]

    [TextField(
        "Search Regex",
        Key = AttributeKey.SearchRegex,
        Description = "Regular Expression to run the search input through before searching. Useful for stripping off characters.",
        IsRequired = false,
        Order = 3 )]

    [MemoField(
        "Update Message",
        Key = AttributeKey.UpdateMessage,
        Description = "Message to show on the profile form. Leaving this blank will hide the message.",
        IsRequired = false,
        DefaultValue = "Please provide only the information that needs to be updated.",
        Order = 4 )]

    [CodeEditorField(
        "Complete Message Lava",
        Key = AttributeKey.CompleteMessageLava,
        Description = "Message to display when complete.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 300,
        IsRequired = true,
        DefaultValue = DefaultValue.CompleteMessageLava,
        Order = 5 )]

    [SystemCommunicationField(
        "Update Email",
        Key = AttributeKey.UpdateEmail,
        Description = "The system email to use to send the updated information.",
        IsRequired = false,
        Order = 6 )]

    [WorkflowTypeField(
        "Workflow Type",
        Key = AttributeKey.WorkflowType,
        Description = BlockAttributeDescription.WorkflowType,
        AllowMultiple = false,
        IsRequired = false,
        Order = 7 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "61C5C8F2-6F76-4583-AB97-228878A6AB65" )]
    public partial class PersonUpdateKiosk : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys and Values

        private static class AttributeKey
        {
            public const string Homepage = "Homepage";
            public const string MinimumPhoneNumberLength = "MinimumPhoneNumberLength";
            public const string MaximumPhoneNumberLength = "MaximumPhoneNumberLength";
            public const string SearchRegex = "SearchRegex";
            public const string UpdateMessage = "UpdateMessage";
            public const string CompleteMessageLava = "CompleteMessageLava";
            public const string UpdateEmail = "UpdateEmail";
            public const string WorkflowType = "WorkflowType";
        }

        private static class DefaultValue
        {
            public const string CompleteMessageLava = @"<div class='alert alert-success'>We have received your updated information. Thank you for helping us keep your information current.</div>";
        }

        private static class BlockAttributeDescription
        {
            public const string WorkflowType = @"The workflow type to launch when an update is made. The following attribute keys should be available on the workflow:
                <ul>
                    <li>PersonId (Integer)</li>
                    <li>FirstName (Text)</li>
                    <li>LastName (Text)</li>
                    <li>Email (Email)</li>
                    <li>BirthDate (Date)</li>
                    <li>StreetAddress (Text)</li>
                    <li>City (Text)</li>
                    <li>State (Text)</li>
                    <li>PostalCode (Text)</li>
                    <li>Country (Text - optional)</li>
                    <li>HomePhone (Text)</li>
                    <li>MobilePhone (Text)</li>
                    <li>OtherUpdates (Memo)</li>
                </ul>";
        }

        #endregion Attribute Keys and Values

        #region Fields

        // used for private variables

        private List<PersonDto> PeopleResults
        {
            get { return (List<PersonDto>)ViewState["PeopleResults"]; }
            set { ViewState["PeopleResults"] = value; }
        }

        private PersonDto SelectedPerson
        {
            get { return (PersonDto)ViewState["SelectedPerson"]; }
            set { ViewState["SelectedPerson"] = value; }
        }


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

            RockPage.AddScriptLink( "~/Scripts/Kiosk/kiosk-core.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                // added for your convenience
            }
            else
            {
                if ( pnlPersonSelect.Visible )
                {
                    BuildPersonControls();
                }
            }

            base.OnLoad( e );
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

        }

        protected void lbSearchCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }
        protected void lbSearchNext_Click( object sender, EventArgs e )
        {
            ShowPersonSelectPanel();
        }

        #endregion

        #region Methods

        // redirects to the homepage
        private void GoHome()
        {
            NavigateToLinkedPage( AttributeKey.Homepage );
        }

        private void HidePanels()
        {
            pnlSearch.Visible = false;
            pnlPersonSelect.Visible = false;
            pnlProfilePanel.Visible = false;
            pnlComplete.Visible = false;
        }

        //
        // Methods for the search screen
        //
        #region search methods

        // show person select panel
        private void ShowPersonSelectPanel()
        {
            int minLength = int.Parse( GetAttributeValue( AttributeKey.MinimumPhoneNumberLength ) );
            int maxLength = int.Parse( GetAttributeValue( AttributeKey.MaximumPhoneNumberLength ) );

            if ( tbPhone.Text.Length >= minLength && tbPhone.Text.Length <= maxLength )
            {
                // run regex expression on input if provided
                string searchInput = tbPhone.Text;
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.SearchRegex ) ) )
                {
                    Regex regex = new Regex( GetAttributeValue( AttributeKey.SearchRegex ) );
                    Match match = regex.Match( searchInput );
                    if ( match.Success )
                    {
                        if ( match.Groups.Count == 2 )
                        {
                            searchInput = match.Groups[1].ToString();
                        }
                    }
                }

                var searchResults = new List<PersonDto>();

                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );
                var people = personService.GetByPhonePartial( searchInput, false, true );

                foreach ( var person in people.ToList() )
                {
                    searchResults.Add( new PersonDto( person.Id, person.LastName, person.NickName ) );
                }

                this.PeopleResults = searchResults;

                BuildPersonControls();

                HidePanels();
                pnlPersonSelect.Visible = true;
            }
            else
            {
                if ( tbPhone.Text.Length < minLength )
                {
                    nbSearch.Text = String.Format( "Please enter at least {0} numbers of your phone.", minLength.ToString() );
                }
                else
                {
                    nbSearch.Text = String.Format( "Please enter no more than {0} numbers to search on.", maxLength.ToString() );
                }
            }
        }
        #endregion

        //
        // Methods for person select
        //
        #region person select

        protected void lbPersonSelectBack_Click( object sender, EventArgs e )
        {
            HidePanels();
            pnlSearch.Visible = true;
        }

        protected void lbPersonSelectCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        protected void lbAddPerson_Click( object sender, EventArgs e )
        {
            HidePanels();
            ShowProfilePanel();
        }

        // called when a giving unit is selected
        void personName_Click( object sender, EventArgs e )
        {
            LinkButton lb = (LinkButton)sender;
            this.SelectedPerson = new PersonDto( lb.CommandArgument );

            ShowProfilePanel();
        }

        private void BuildPersonControls()
        {
            // display results
            if ( this.PeopleResults.Count > 0 )
            {

                foreach ( var unit in this.PeopleResults )
                {
                    LinkButton lb = new LinkButton();
                    lb.ID = "lbUnit_" + unit.PersonId.ToString();
                    lb.CssClass = "btn btn-primary btn-kioskselect";
                    phPeople.Controls.Add( lb );
                    lb.CommandArgument = unit.CommandArg;
                    lb.Click += new EventHandler( personName_Click );
                    lb.Text = string.Format( "{0} <small>{1}</small>", unit.LastName, unit.FirstName );
                }
            }
            else
            {
                // todo update description of how to add someone
                phPeople.Controls.Add( new LiteralControl(
                    "<div class='alert alert-info'>There were not any families found with the phone number you entered. You can add your family using the 'Register Your Family' button below.</div>" ) );
            }
        }
        #endregion

        //
        // Methods for profile panel
        //
        #region profile panel

        private void ShowProfilePanel()
        {
            HidePanels();
            pnlProfilePanel.Visible = true;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.UpdateMessage ) ) )
            {
                lUpdateMessage.Text = GetAttributeValue( AttributeKey.UpdateMessage );
            }
            else
            {
                lUpdateMessage.Visible = false;
            }

            if ( this.SelectedPerson != null )
            {
                tbFirstName.Text = this.SelectedPerson.FirstName;
                tbLastName.Text = this.SelectedPerson.LastName;
                hfPersonId.Value = this.SelectedPerson.PersonId.ToString();
            }
        }

        protected void lbProfileBack_Click( object sender, EventArgs e )
        {
            HidePanels();
            ShowPersonSelectPanel();
        }
        protected void lbProfileCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        protected void lbProfileNext_Click( object sender, EventArgs e )
        {
            // setup merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "PersonId", hfPersonId.Value );
            mergeFields.Add( "FirstName", tbFirstName.Text );
            mergeFields.Add( "LastName", tbLastName.Text );
            mergeFields.Add( "StreetAddress", acAddress.Street1 );
            mergeFields.Add( "City", acAddress.City );
            mergeFields.Add( "State", acAddress.State );
            mergeFields.Add( "PostalCode", acAddress.PostalCode );
            mergeFields.Add( "Country", acAddress.Country );
            mergeFields.Add( "Email", tbEmail.Text );
            mergeFields.Add( "HomePhone", pnbHomePhone.Text );
            mergeFields.Add( "MobilePhone", pnbHomePhone.Text );
            mergeFields.Add( "BirthDate", dpBirthdate.Text );
            mergeFields.Add( "OtherUpdates", tbOtherUpdates.Text );

            // if an email was provided email results
            RockContext rockContext = new RockContext();
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.UpdateEmail ) ) )
            {
                var receiptEmail = new SystemCommunicationService( rockContext ).Get( new Guid( GetAttributeValue( "UpdateEmail" ) ) );
                if ( receiptEmail != null && receiptEmail.To.IsNotNullOrWhiteSpace() )
                {
                    var errorMessages = new List<string>();
                    var message = new RockEmailMessage( receiptEmail );
                    foreach ( var recipient in message.GetRecipients() )
                    {
                        recipient.MergeFields = mergeFields;
                    }
                    message.Send( out errorMessages );
                }
            }

            // launch workflow if configured
            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.WorkflowType ) ) )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflowType = WorkflowTypeCache.Get( new Guid( GetAttributeValue( AttributeKey.WorkflowType ) ) );

                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, "Kiosk Update Info" );

                    // set attributes
                    workflow.SetAttributeValue( "PersonId", hfPersonId.Value );
                    workflow.SetAttributeValue( "FirstName", tbFirstName.Text );
                    workflow.SetAttributeValue( "LastName", tbLastName.Text );
                    workflow.SetAttributeValue( "StreetAddress", acAddress.Street1 );
                    workflow.SetAttributeValue( "City", acAddress.City );
                    workflow.SetAttributeValue( "State", acAddress.State );
                    workflow.SetAttributeValue( "PostalCode", acAddress.PostalCode );
                    workflow.SetAttributeValue( "Country", acAddress.Country );
                    workflow.SetAttributeValue( "Email", tbEmail.Text );
                    workflow.SetAttributeValue( "HomePhone", pnbHomePhone.Text );
                    workflow.SetAttributeValue( "MobilePhone", pnbHomePhone.Text );
                    workflow.SetAttributeValue( "BirthDate", dpBirthdate.Text );
                    workflow.SetAttributeValue( "OtherUpdates", tbOtherUpdates.Text );

                    // launch workflow
                    List<string> workflowErrors;
                    workflowService.Process( workflow, out workflowErrors );
                }
            }

            HidePanels();
            pnlComplete.Visible = true;

            lCompleteMessage.Text = GetAttributeValue( AttributeKey.CompleteMessageLava ).ResolveMergeFields( mergeFields );

        }

        #endregion

        //
        // Complete Panel Methods
        //
        #region complete panel methods

        protected void lbCompleteDone_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        #endregion

        #endregion

    }

    [Serializable]
    class PersonDto
    {
        public int PersonId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }

        public string CommandArg
        {
            get { return string.Format( "{0}|{1}|{2}", PersonId, LastName, FirstName ); }
        }

        public PersonDto( int personAliasId, string lastName, string firstNames )
        {
            PersonId = personAliasId;
            LastName = lastName;
            FirstName = firstNames;
        }

        public PersonDto( string commandArg )
        {
            string[] parts = commandArg.Split( '|' );

            PersonId = Convert.ToInt32( parts[0] );
            LastName = parts[1];
            FirstName = parts[2];
        }
    }
}