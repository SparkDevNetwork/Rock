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

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Kiosk Transaction Entry" )]
    [Category( "Finance" )]
    [Description( "Block used to process giving from a kiosk." )]

    #region Block Attributes
    [ComponentField( "Rock.Financial.GatewayContainer, Rock", "Credit Card Gateway", "The payment gateway to use for Credit Card transactions", true, "", "", 0, "CCGateway" )]
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
    #endregion

    public partial class KioskTransactionEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private List<GivingUnit> GivingUnits
        {
            get { return (List<GivingUnit>)ViewState["GivingUnits"]; }
            set { ViewState["GivingUnits"] = value; }
        }

        private GivingUnit SelectedGivingUnit
        {
            get { return (GivingUnit)ViewState["SelectedGivingUnit"]; }
            set { ViewState["SelectedGivingUnit"] = value; }
        }

        private Dictionary<int, string> Accounts
        {
            get { return ViewState["Accounts"] as Dictionary<int, string>; }
            set { ViewState["Accounts"] = value; }
        }

        private Dictionary<int, decimal> Amounts
        {
            get { return ViewState["Amounts"] as Dictionary<int, decimal>; }
            set { ViewState["Amounts"] = value; }
        }

        private int? AnonymousGiverPersonAliasId
        {
            get { return (int)ViewState["AnonymousGiverPersonAliasId"]; }
            set { ViewState["AnonymousGiverPersonAliasId"] = value; }
        }

        private int Campus {
            get {
                if ( ViewState["Campus"] == null )
                {
                    return 1;
                }
                else
                {
                    return Int32.Parse( ViewState["Campus"].ToString() );
                }
            }
            set { ViewState["Campus"] = value; }
        }

        DefinedValueCache _dvcConnectionStatus = null;
        DefinedValueCache _dvcRecordStatus = null;

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
            RockPage.AddScriptLink( "~/Scripts/Kiosk/jquery.scannerdetection.js" );
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
                if ( !CheckSettings() )
                {
                    pnlSearch.Visible = false;
                }
                else
                {
                    ShowSearchPanel();
                }
                 
                // set max length of phone
                int maxLength = int.Parse( GetAttributeValue( "MaximumPhoneNumberLength" ) );
                tbPhone.MaxLength = maxLength;

                LoadAccounts();

                // todo set campus
                this.Campus = 1;
            }
            else
            {
                if ( pnlGivingUnitSelect.Visible )
                {
                    BuildGivingUnitControls();
                }

                if (pnlAccountEntry.Visible)
                {
                    BuildAccountControls();
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
            ShowGivingUnitSelectPanel();
        }

        protected void lbSearchCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        protected void lbGiveAnonymously_Click( object sender, EventArgs e )
        {
            this.SelectedGivingUnit = new GivingUnit( this.AnonymousGiverPersonAliasId.Value, "Anonymous", "" );
            lbAccountEntryBack.Attributes.Add( "back-to", "search" );
            HidePanels();
            ShowAccountPanel();
        }

        #endregion

        #region Giving Unit Select Panel Events
        //
        // Giving Unit Select Panel Events
        //

        // called when a giving unit is selected
        void unitName_Click( object sender, EventArgs e )
        {
            LinkButton lb = (LinkButton)sender;
            this.SelectedGivingUnit = new GivingUnit( lb.CommandArgument );

            lbAccountEntryBack.Attributes.Add( "back-to", "giving-unit-select" );
            ShowAccountPanel();
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

        // used to show the new registration page
        protected void lbRegisterFamily_Click( object sender, EventArgs e )
        {
            HidePanels();
            pnlRegister.Visible = true;
            tbFirstName.Focus();
        }
        #endregion

        #region Account Entry Events
        //
        // Account Entry Events
        //

        protected void lbAccountEntryBack_Click( object sender, EventArgs e )
        {
            HidePanels();
            
            switch ( lbAccountEntryBack.Attributes["back-to"] )
            {
                case "search":
                    ShowSearchPanel();
                    break;
                case "giving-unit-select":
                    ShowGivingUnitSelectPanel();
                    break;
            }
        }
        protected void lbAccountEntryCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }

        protected void lbAccountEntryNext_Click( object sender, EventArgs e )
        {
            // save account amounts
            decimal totalAmount = 0;
            this.Amounts = new Dictionary<int, decimal>();
            bool accountFieldsValid = true;

            foreach ( var account in this.Accounts )
            {
                CurrencyBox cb = phAccounts.FindControl( "tbAccount_" + account.Key.ToString() ) as CurrencyBox;
                if ( cb != null )
                {
                    decimal fundAmount = 0;
                    if ( !decimal.TryParse( cb.Text.Trim(), out fundAmount ) )
                    {
                        if ( !string.IsNullOrEmpty( cb.Text ) )
                        {
                            accountFieldsValid = false;
                        }
                        
                        fundAmount = 0;
                    }
                    fundAmount = Math.Round( fundAmount, 2 );

                    if ( fundAmount >= 0 )
                    {
                        this.Amounts.Add( account.Key, fundAmount );
                        totalAmount += fundAmount;
                    }
                }
            }

            if ( !accountFieldsValid )
            {
                nbAccountEntry.Text = "Please enter valid amounts in all fields.";
            } else if ( totalAmount <= 0 )
            {
                nbAccountEntry.Text = "Please enter a valid amount for one or more accounts.";
            }
            else
            {
                nbAccountEntry.Text = string.Empty;
                HidePanels();
                pnlSwipe.Visible = true;
            }
            
        }
        #endregion

        #region Swipe Panel Events
        //
        // Swipe Panel Events
        //

        protected void lbSwipeNext_Click( object sender, EventArgs e )
        {
            HidePanels();
            ShowReceiptPanel();            
        }

        protected void lbSwipeBack_Click( object sender, EventArgs e )
        {
            HidePanels();
            ShowAccountPanel();
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

        #region Register Panel Events
        //
        // Register Panel Events
        //
        protected void lbRegisterBack_Click( object sender, EventArgs e )
        {
            pnlRegister.Visible = false;
            ShowGivingUnitSelectPanel();
        }
        protected void lbRegisterCancel_Click( object sender, EventArgs e )
        {
            GoHome();
        }
        protected void lbRegisterNext_Click( object sender, EventArgs e )
        {
            // create new person / family
            Person person = new Person();
            person.FirstName = tbFirstName.Text.Trim();
            person.LastName = tbLastName.Text.Trim();
            person.Email = tbEmail.Text.Trim();
            person.ConnectionStatusValueId = _dvcConnectionStatus.Id;
            person.RecordStatusValueId = _dvcRecordStatus.Id;
            person.Gender = Gender.Unknown;

            GroupService.SaveNewFamily( new RockContext(), person, this.Campus, false );

            // set as selected giving unit
            this.SelectedGivingUnit = new GivingUnit( person.PrimaryAliasId.Value, person.LastName, person.FirstName );

            ShowAccountPanel();
        }
        #endregion

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( CheckSettings() )
            {
                HidePanels();
                ShowSearchPanel();
            }
        }

        #endregion

        #region Methods

        // show search panel
        private void ShowSearchPanel()
        {
            pnlSearch.Visible = true;
        }

        // show giving unit select panel
        private void ShowGivingUnitSelectPanel()
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

                var searchResults = new List<GivingUnit>();

                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );
                var people = personService.GetByPhonePartial( searchInput, false, true );

                foreach ( var person in people.ToList() )
                {
                    if ( person.GivingGroupId == null )
                    {
                        // giving as an individuals
                        searchResults.Add( new GivingUnit(person.PrimaryAliasId.Value, person.LastName, person.FirstName));
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
                            searchResults.Add( new GivingUnit( person.PrimaryAliasId.Value, person.LastName, person.FirstName ) );
                        }
                        else
                        {
                            // display as a family
                            string firstNameList = string.Join( ", ", givingGroupMembers.Select( g => g.Person.NickName ) ).ReplaceLastOccurrence(",", " &");
                            int headOfHousePersonAliasId = givingGroupMembers.Select( g => g.Person.PrimaryAliasId.Value ).FirstOrDefault();
                            string lastName = givingGroupMembers.Select( g => g.Person.LastName ).FirstOrDefault();
                            searchResults.Add( new GivingUnit( headOfHousePersonAliasId, person.LastName, firstNameList ) );
                        }
                    }
                }

                this.GivingUnits = searchResults;

                BuildGivingUnitControls();

                HidePanels();
                pnlGivingUnitSelect.Visible = true;
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

        // show accounts panel 
        private void ShowAccountPanel()
        {
            lblGivingAs.Text = String.Format( "Giving as {0} {1}", this.SelectedGivingUnit.FirstNames, this.SelectedGivingUnit.LastName );
            
            // get accounts
            BuildAccountControls();

            // show panels
            HidePanels();
            pnlAccountEntry.Visible = true;


        }

        // show receipt panel
        private void ShowReceiptPanel()
        {
            bool receiptEmailed = true;
            
            
            // get giving unit
            RockContext rockContext = new RockContext();
            var givingUnit = new PersonAliasService( rockContext ).Queryable().Where( p => p.Id == this.SelectedGivingUnit.PersonAliasId ).FirstOrDefault().Person;
            
            
            // setup lava
            var mergeFields = new Dictionary<string, object>();

            List<Dictionary<String, object>> accountAmounts = new List<Dictionary<String, object>>();
            decimal totalAmount = 0;

            foreach ( var amount in this.Amounts )
            {
                if ( amount.Value > 0 )
                {
                    var accountAmount = new Dictionary<String, object>();
                    accountAmount.Add( "AccountId", amount.Key );
                    accountAmount.Add("AccountName", this.Accounts.Where( a => a.Key == amount.Key ).FirstOrDefault().Value);
                    accountAmount.Add( "Amount", amount.Value );

                    accountAmounts.Add( accountAmount );

                    totalAmount += amount.Value;
                }
            }

            // total amounts
            mergeFields.Add( "TotalAmount", totalAmount );

            // gave anonymous
            if ( this.AnonymousGiverPersonAliasId == this.SelectedGivingUnit.PersonAliasId )
            {
                mergeFields.Add( "GaveAnonymous", "True" );
                mergeFields.Add( "ReceiptEmail", "" );
            }
            else
            {
                mergeFields.Add( "GaveAnonymous", "False" );
                mergeFields.Add( "ReceiptEmail", givingUnit.Email );
            }

            // whether a receipt was emailed
            mergeFields.Add( "ReceiptEmailed", receiptEmailed.ToString() );

            // names
            mergeFields.Add( "LastName", this.SelectedGivingUnit.LastName );
            mergeFields.Add( "FirstNames", this.SelectedGivingUnit.FirstNames );           
            
            mergeFields.Add( "Amounts", accountAmounts );
            
            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

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

        // displays accounts
        private void BuildAccountControls()
        {
            
            if ( this.Accounts != null )
            {
                bool firstAccount = true;
                
                foreach ( var account in this.Accounts )
                {
                    HtmlGenericControl formGroup = new HtmlGenericControl( "div" );
                    formGroup.AddCssClass( "form-group" );
                    phAccounts.Controls.Add( formGroup );

                    CurrencyBox tb = new CurrencyBox();
                    tb.ID = "tbAccount_" + account.Key;
                    tb.Attributes.Add( "name", tb.ID );
                    tb.Attributes.Add( "type", "number" );
                    tb.CssClass = "input-account";

                    if ( firstAccount )
                    {
                        tb.CssClass = "input-account active";
                        firstAccount = false;
                    }

                    Label label = new Label();
                    label.AssociatedControlID = tb.ID;
                    label.ID = "labelFund_" + account.Key;
                    label.Text = account.Value;

                    formGroup.Controls.Add( label );
                    formGroup.Controls.Add( tb );
                }
            }
        }

        // displays giving units
        private void BuildGivingUnitControls()
        {           
            // display results
            if ( this.GivingUnits.Count > 0 )
            {

                foreach ( var unit in this.GivingUnits )
                {
                    LinkButton lb = new LinkButton();
                    lb.ID = "lbUnit_" + unit.PersonAliasId.ToString();
                    lb.CssClass = "btn btn-primary btn-kioskselect";
                    phGivingUnits.Controls.Add( lb );
                    lb.CommandArgument = unit.CommandArg;
                    lb.Click += new EventHandler( unitName_Click );
                    lb.Text = string.Format("{0} <small>{1}</small>", unit.LastName, unit.FirstNames);
                }
            }
            else
            {
                phGivingUnits.Controls.Add( new LiteralControl(
                    "<div class='alert alert-danger'>There were not any families found with the phone number you entered. You can add your family using the 'Register Your Family' button below.</div>" ) );
            }
        }

        // hides all panels
        private void HidePanels()
        {
            pnlSearch.Visible = false;
            pnlGivingUnitSelect.Visible = false;
            pnlRegister.Visible = false;
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

        // loads accounts
        private void LoadAccounts()
        {
            // get list of selected accounts filtered by the current campus
            RockContext rockContext = new RockContext();
            FinancialAccountService accountService = new FinancialAccountService( rockContext );

            Guid[] selectedAccounts = GetAttributeValue( "Accounts" ).Split( ',' ).Select( s => Guid.Parse( s ) ).ToArray(); ;

            var accounts = accountService.Queryable()
                            .Where( a => selectedAccounts.Contains( a.Guid ));
            
            if (this.Campus != null) {
                accounts = accounts.Where(a => a.CampusId.Value == this.Campus || a.CampusId == null);
            }
            
            this.Accounts = new Dictionary<int, string>();

            foreach ( var account in accounts.ToList() )
            {
                this.Accounts.Add( account.Id, account.PublicName );
            }
        }

        // checks the settings provided
        private bool CheckSettings()
        {
            nbBlockConfigErrors.Title = string.Empty;
            nbBlockConfigErrors.Text = string.Empty;
            
            _dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            if ( _dvcConnectionStatus == null )
            {
                nbBlockConfigErrors.Heading = "Invalid Connection Status";
                nbBlockConfigErrors.Text = "<p>The selected Connection Status setting does not exist.</p>";
                return false;
            }

            _dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );
            if ( _dvcRecordStatus == null )
            {
                nbBlockConfigErrors.Heading = "Invalid Record Status";
                nbBlockConfigErrors.Text = "<p>The selected Record Status setting does not exist.</p>";
                return false;
            }

            // get anonymous person
            RockContext rockContext = new RockContext();
            Person anonymousPerson = null;

            Guid anonymousPersonAliasGuid;

            if ( Guid.TryParse( GetAttributeValue( "AnonymousPerson" ), out anonymousPersonAliasGuid ) )
            {
                anonymousPerson = new PersonAliasService( rockContext ).Get(anonymousPersonAliasGuid ).Person;
            } 
            
            if ( anonymousPerson != null )
            {
                this.AnonymousGiverPersonAliasId = anonymousPerson.PrimaryAliasId;
                lbGiveAnonymously.Visible = true;
            }
            else
            {
                lbGiveAnonymously.Visible = false;
            }

            return true;
        }
        #endregion 
        
}

    [Serializable]
    class GivingUnit
    {
        public int PersonAliasId { get; set; }
        public string LastName { get; set; }
        public string FirstNames { get; set; }

        public string CommandArg
        {
            get { return string.Format( "{0}|{1}|{2}", PersonAliasId, LastName, FirstNames ); }
        }

        public GivingUnit( int personAliasId, string lastName, string firstNames )
        {
            PersonAliasId = personAliasId;
            LastName = lastName;
            FirstNames = firstNames;
        }

        public GivingUnit( string commandArg )
        {
            string[] parts = commandArg.Split( '|' );

            PersonAliasId = Convert.ToInt32( parts[0] );
            LastName = parts[1];
            FirstNames = parts[2];
        }
    }
}