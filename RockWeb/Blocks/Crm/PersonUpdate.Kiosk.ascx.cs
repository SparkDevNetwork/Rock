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
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using System.Dynamic;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;
using Rock.Financial;
using Rock.Communication;
using System.Threading;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Person Update - Kiosk" )]
    [Category( "CRM" )]
    [Description( "Block used to update a person's information from a kiosk." )]

    #region Block Attributes
    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", true, "", "", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false,
        Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_KIOSK, "", 1 )]
    [AccountsField( "Accounts", "Accounts to allow giving. This list will be filtered by campus context when displayed.", true, "", "", 1 )]
    [TextField( "Batch Name Prefix", "The prefix to add to the financial batch.", true, "Kiosk Giving", "", 2 )]
    [LinkedPage( "Homepage", "Homepage of the kiosk.", true, "", "", 2 )]
    [PersonField("Anonymous Person", "Person in the database to assign anonymous giving to.", true, "", "", 3)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status to use when creating a new individual.", true, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT, "", 4 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status to use when creating a new individual.", true, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 5 )]
    [IntegerField( "Minimum Phone Number Length", "Minimum length for phone number searches (defaults to 4).", false, 4,"", 6 )]
    [IntegerField( "Maximum Phone Number Length", "Maximum length for phone number searches (defaults to 10).", false, 10, "", 7 )]
    [TextField( "Search Regex", "Regular Expression to run the search input through before searching. Useful for stripping off characters.", false, "", "", 8 )]
    [CodeEditorField( "Receipt Lava", "Lava to display for the receipt panel.", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 300, true, "{% include '~~/Assets/Lava/KioskGivingReceipt.lava' %}", "", 9 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 10 )]
    [SystemEmailField( "Receipt Email", "The system email to use to send the receipt.", false, "", "", 11 )]
    #endregion

    public partial class PersonUpdateKiosk : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private List<UpdatePerson> GivingUnits
        {
            get { return (List<UpdatePerson>)ViewState["GivingUnits"]; }
            set { ViewState["GivingUnits"] = value; }
        }

        private UpdatePerson SelectedPerson
        {
            get { return (UpdatePerson)ViewState["SelectedPerson"]; }
            set { ViewState["SelectedPerson"] = value; }
        }

        private int CampusId 
        {
            get 
            { 
                if ( ViewState["CampusId"] == null )
                {
                    return 1;
                }
                else
                {
                    return Int32.Parse( ViewState["CampusId"].ToString() );
                }
            }
            set { ViewState["CampusId"] = value; }
        }

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

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/Kiosk/kiosk-core.js" );
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
                                 
                // set max length of phone
                int maxLength = int.Parse( GetAttributeValue( "MaximumPhoneNumberLength" ) );
                tbPhone.MaxLength = maxLength;

                // todo set campus
                this.CampusId = 1;
            }
            else
            {

                if ( pnlPersonSelect.Visible )
                {
                    BuildPersonControls();
                } 
            }
        }

        #endregion

        #region Events

        #region Search Panel Events
        
        //
        // Search Panel Events
        //

        protected void lbSearchNext_Click( object sender, EventArgs e )
        {
            ShowPersonSelectPanel();
        }

        protected void lbSearchCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        #endregion

        #region Person Select Panel Events
        //
        // Person Select Panel Events
        //

        // called when a giving unit is selected
        void unitName_Click( object sender, EventArgs e )
        {
            LinkButton lb = (LinkButton)sender;
            this.SelectedPerson = new UpdatePerson( lb.CommandArgument );

            lbAccountEntryBack.Attributes.Add( "back-to", "giving-unit-select" );
            //ShowAccountPanel();
        }

        protected void lbGivingUnitSelectCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        protected void lbGivingUnitSelectBack_Click( object sender, EventArgs e )
        {
            HidePanels();
            pnlSearch.Visible = true;
        }
        #endregion

        #region Swipe Panel Events
        //
        // Swipe Panel Events
        //

        private void ProcessSwipe(string swipeData) {
            
            
        }

        private void SendReceipt()
        {
            RockContext rockContext = new RockContext();
            var receiptEmail = new SystemEmailService( rockContext ).Get( new Guid( GetAttributeValue( "ReceiptEmail" ) ) );

            if ( receiptEmail != null )
            {
                var givingUnit = new PersonAliasService( rockContext ).Get( this.SelectedPerson.PersonAliasId ).Person;
                var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );

                var recipients = new List<RecipientData>();
                recipients.Add( new RecipientData( givingUnit.Email, GetMergeFields( givingUnit ) ) );

                Email.Send( receiptEmail.Guid, recipients, appRoot );
            }
        }

        protected void lbSwipeBack_Click( object sender, EventArgs e )
        {
            HidePanels();
            //ShowAccountPanel();
        }
        protected void lbSwipeCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }
        #endregion

        #region Receipt Panel Events
        
        // done, go home ET
        protected void lbReceiptDone_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        #endregion


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            
        }

        #endregion

        #region Methods

        // show search panel
        private void ShowSearchPanel()
        {
            pnlSearch.Visible = true;
        }

        // show giving unit select panel
        private void ShowPersonSelectPanel()
        {
            int minLength = int.Parse( GetAttributeValue( "MinimumPhoneNumberLength" ) );
            int maxLength = int.Parse( GetAttributeValue( "MaximumPhoneNumberLength" ) );

            if ( tbPhone.Text.Length >= minLength && tbPhone.Text.Length <= maxLength )
            {
                // run regex expression on input if provided
                string searchInput = tbPhone.Text;
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "SearchRegex" ) ) )
                {
                    Regex regex = new Regex( GetAttributeValue( "SearchRegex" ) );
                    Match match = regex.Match( searchInput );
                    if ( match.Success )
                    {
                        if ( match.Groups.Count == 2 )
                        {
                            searchInput = match.Groups[1].ToString();
                        }
                    }
                }

                var searchResults = new List<UpdatePerson>();

                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );
                var people = personService.GetByPhonePartial( searchInput, false, true );

                foreach ( var person in people.ToList() )
                {
                    if ( person.GivingGroupId == null )
                    {
                        // giving as an individuals
                        searchResults.Add( new UpdatePerson(person.PrimaryAliasId.Value, person.LastName, person.FirstName));
                    }
                    else
                    {
                        var givingGroupMembers = person.GivingGroup.Members
                                                    .Where( g => g.Person.GivingGroupId == g.GroupId )
                                                    .OrderBy( g => g.GroupRole.Order )
                                                    .ThenBy( g => g.Person.Gender )
                                                    .ThenBy( g => g.Person.Age );

                        if ( givingGroupMembers.ToList().Count == 1 )
                        {
                            // only one person in the giving group display as an individual
                            searchResults.Add( new UpdatePerson( person.PrimaryAliasId.Value, person.LastName, person.FirstName ) );
                        }
                        else
                        {
                            // display as a family
                            string firstNameList = string.Join( ", ", givingGroupMembers.Select( g => g.Person.NickName ) ).ReplaceLastOccurrence(",", " &");
                            int headOfHousePersonAliasId = givingGroupMembers.Select( g => g.Person.PrimaryAliasId.Value ).FirstOrDefault();
                            string lastName = givingGroupMembers.Select( g => g.Person.LastName ).FirstOrDefault();
                            searchResults.Add( new UpdatePerson( headOfHousePersonAliasId, person.LastName, firstNameList ) );
                        }
                    }
                }

                this.GivingUnits = searchResults;

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

        
        // show receipt panel
        private void ShowReceiptPanel()
        {
            var mergeFields = GetMergeFields(null);

            string template = GetAttributeValue( "ReceiptLava" );

            // show debug info
            bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();
            if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }

            lReceiptContent.Text = template.ResolveMergeFields( mergeFields );
            pnlReceipt.Visible = true;
        }

        private Dictionary<string, object> GetMergeFields(Person givingUnit)
        {
            return new Dictionary<string, object>();
        }

        

        // displays persons
        private void BuildPersonControls()
        {           
            // display results
            if ( this.GivingUnits.Count > 0 )
            {

                foreach ( var unit in this.GivingUnits )
                {
                    LinkButton lb = new LinkButton();
                    lb.ID = "lbUnit_" + unit.PersonAliasId.ToString();
                    lb.CssClass = "btn btn-primary btn-kioskselect";
                    phPeople.Controls.Add( lb );
                    lb.CommandArgument = unit.CommandArg;
                    lb.Click += new EventHandler( unitName_Click );
                    lb.Text = string.Format("{0} <small>{1}</small>", unit.LastName, unit.FirstName);
                }
            }
            else
            {
                phPeople.Controls.Add( new LiteralControl(
                    "<div class='alert alert-info'>There were not any families found with the phone number you entered. You can add your family using the 'Register Your Family' button below.</div>" ) );
            }
        }

        // hides all panels
        private void HidePanels()
        {
            pnlSearch.Visible = false;
            pnlPersonSelect.Visible = false;
            pnlAccountEntry.Visible = false;
            pnlSwipe.Visible = false;
            pnlReceipt.Visible = false;

            // clear out specific notification blocks that are used for validation
            nbSearch.Text = string.Empty;
        }

        // redirects to the homepage
        private void GoHome()
        {
            NavigateToLinkedPage( "Homepage" );
        }

        
        #endregion 
        
}

    [Serializable]
    class UpdatePerson
    {
        public int PersonAliasId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }

        public string CommandArg
        {
            get { return string.Format( "{0}|{1}|{2}", PersonAliasId, LastName, FirstName ); }
        }

        public UpdatePerson( int personAliasId, string lastName, string firstNames )
        {
            PersonAliasId = personAliasId;
            LastName = lastName;
            FirstName = firstNames;
        }

        public UpdatePerson( string commandArg )
        {
            string[] parts = commandArg.Split( '|' );

            PersonAliasId = Convert.ToInt32( parts[0] );
            LastName = parts[1];
            FirstName = parts[2];
        }
    }
}