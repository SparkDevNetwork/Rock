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
using System.Text.RegularExpressions;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Person Update - Kiosk" )]
    [Category( "CRM" )]
    [Description( "Block used to update a person's information from a kiosk." )]

    [LinkedPage( "Homepage", "Homepage of the kiosk.", true, "", "", 2 )]
    [IntegerField( "Minimum Phone Number Length", "Minimum length for phone number searches (defaults to 4).", false, 4, "", 6 )]
    [IntegerField( "Maximum Phone Number Length", "Maximum length for phone number searches (defaults to 10).", false, 10, "", 7 )]
    [TextField( "Search Regex", "Regular Expression to run the search input through before searching. Useful for stripping off characters.", false, "", "", 8 )]
    public partial class PersonUpdateKiosk : Rock.Web.UI.RockBlock
    {
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
                // added for your convenience
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
            NavigateToLinkedPage( "Homepage" );
        }

        private void HidePanels()
        {
            pnlSearch.Visible = false;
            pnlPersonSelect.Visible = false;
            pnlProfilePanel.Visible = false;
        }

        //
        // Methods for the search screen
        //
        #region search methods

        // show person select panel
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

                var searchResults = new List<PersonDto>();

                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );
                var people = personService.GetByPhonePartial( searchInput, false, true );

                foreach ( var person in people.ToList() )
                {
                    searchResults.Add( new PersonDto( person.PrimaryAliasId.Value, person.LastName, person.NickName ) );
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

        }
        protected void lbPersonSelectCancel_Click( object sender, EventArgs e )
        {
            GoHome();
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
                    lb.ID = "lbUnit_" + unit.PersonAliasId.ToString();
                    lb.CssClass = "btn btn-primary btn-kioskselect";
                    phPeople.Controls.Add( lb );
                    lb.CommandArgument = unit.CommandArg;
                    lb.Click += new EventHandler( personName_Click );
                    lb.Text = string.Format( "{0} <small>{1}</small>", unit.LastName, unit.FirstName );
                }
            }
            else
            {
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
        }

        #endregion

        //
        // Profile Methods
        //
        #region profile methods
        protected void lbProfileBack_Click( object sender, EventArgs e )
        {
            HidePanels();
            ShowPersonSelectPanel();
        }
        protected void lbProfileCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }
        #endregion


        #endregion

    }

    [Serializable]
    class PersonDto
    {
        public int PersonAliasId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }

        public string CommandArg
        {
            get { return string.Format( "{0}|{1}|{2}", PersonAliasId, LastName, FirstName ); }
        }

        public PersonDto( int personAliasId, string lastName, string firstNames )
        {
            PersonAliasId = personAliasId;
            LastName = lastName;
            FirstName = firstNames;
        }

        public PersonDto( string commandArg )
        {
            string[] parts = commandArg.Split( '|' );

            PersonAliasId = Convert.ToInt32( parts[0] );
            LastName = parts[1];
            FirstName = parts[2];
        }
    }
}