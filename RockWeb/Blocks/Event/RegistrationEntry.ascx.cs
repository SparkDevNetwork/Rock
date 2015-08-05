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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Humanizer;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Block used to register for registration instance.
    /// </summary>
    [DisplayName( "Registration Entry" )]
    [Category( "Event" )]
    [Description( "Block used to register for registration instance." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE, "", 2 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Event Registration", "", 3 )]
    [BooleanField( "Enable Debug", "Display the merge fields that are available for lava ( Success Page ).", false, "", 4 )]
    public partial class RegistrationEntry : RockBlock
    {
        #region Fields

        // Page (query string) parameter names
        private const string REGISTRATION_ID_PARAM_NAME = "RegistrationId";
        private const string REGISTRATION_SLUG_PARAM_NAME = "Slug";
        private const string REGISTRATION_INSTANCE_ID_PARAM_NAME = "RegistrationInstanceId";
        private const string REGISTRATION_GROUP_ID_PARAM_NAME = "GroupId";
        private const string REGISTRATION_CAMPUS_ID_PARAM_NAME = "CampusId";

        // Viewstate keys
        private const string REGISTRATION_INSTANCE_STATE_KEY = "RegistrationInstanceState";
        private const string REGISTRATION_STATE_KEY = "RegistrationState";
        private const string GROUP_ID_KEY = "GroupId";
        private const string CURRENT_PANEL_KEY = "CurrentPanel";
        private const string CURRENT_REGISTRANT_INDEX_KEY = "CurrentRegistrantIndex";
        private const string CURRENT_FORM_INDEX_KEY = "CurrentFormIndex";

        #endregion

        #region Properties

        // The selected registration instance
        private RegistrationInstance RegistrationInstanceState { get; set; }

        // The selected group from linkage
        private int? GroupId { get; set; }

        // Info about each current registration
        protected RegistrationInfo RegistrationState { get; set; }

        // The current panel to display ( HowMany
        private int CurrentPanel { get; set; }

        // The current registrant index
        private int CurrentRegistrantIndex { get; set; }

        // The current form index
        private int CurrentFormIndex { get; set; }

        // The registration template.
        private RegistrationTemplate RegistrationTemplate
        {
            get
            {
                return RegistrationInstanceState != null ? RegistrationInstanceState.RegistrationTemplate : null;
            }
        }

        /// <summary>
        /// Gets the registration term.
        /// </summary>
        /// <value>
        /// The registration term.
        /// </value>
        private string RegistrationTerm
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.RegistrationTerm ) )
                {
                    return RegistrationTemplate.RegistrationTerm;
                }
                return "Registration";
            }
        }

        /// <summary>
        /// Gets the registrant term.
        /// </summary>
        /// <value>
        /// The registrant term.
        /// </value>
        private string RegistrantTerm
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.RegistrantTerm ) )
                {
                    return RegistrationTemplate.RegistrantTerm;
                }
                return "Person";
            }
        }

        /// <summary>
        /// Gets the fee term.
        /// </summary>
        /// <value>
        /// The fee term.
        /// </value>
        private string FeeTerm
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.FeeTerm ) )
                {
                    return RegistrationTemplate.FeeTerm;
                }
                return "Additional Option";
            }
        }

        /// <summary>
        /// Gets the discount code term.
        /// </summary>
        /// <value>
        /// The discount code term.
        /// </value>
        private string DiscountCodeTerm
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.DiscountCodeTerm ) )
                {
                    return RegistrationTemplate.DiscountCodeTerm;
                }
                return "Discount Code";
            }
        }

        /// <summary>
        /// Gets the number of forms for the current registration template.
        /// </summary>
        private int FormCount
        {
            get
            {
                if ( RegistrationTemplate != null && RegistrationTemplate.Forms != null )
                {
                    return RegistrationTemplate.Forms.Count;
                }

                return 0;
            }
        }        
        
        /// <summary>
        /// If the registration template allows multiple registrants per registration, returns the maximum allowed
        /// </summary>
        private int MaxRegistrants
        {
            get
            {
                // If this is an existing registration, max registrants is the number of registrants already 
                // on registration ( don't allow adding new registrants )
                if ( RegistrationState != null && RegistrationState.RegistrationId.HasValue )
                {
                    return RegistrationState.RegistrantCount;
                }

                // Otherwise if template allows multiple, set the max amount
                if ( RegistrationTemplate != null && RegistrationTemplate.AllowMultipleRegistrants )
                {
                    if ( RegistrationTemplate.MaxRegistrants <= 0 )
                    {
                        return int.MaxValue;
                    }
                    return RegistrationTemplate.MaxRegistrants;
                }

                // Default is a maximum of one
                return 1;
            }
        }

        /// <summary>
        /// Gets the minimum number of registrants allowed. Most of the time this is one, except for an existing
        /// registration that has existing registrants. The minimum in this case is the number of existing registrants
        /// </summary>
        private int MinRegistrants
        {
            get
            {
                // If this is an existing registration, min registrants is the number of registrants already 
                // on registration ( don't allow adding new registrants )
                if ( RegistrationState != null && RegistrationState.RegistrationId.HasValue )
                {
                    return RegistrationState.RegistrantCount;
                }
                
                // Default is a minimum of one
                return 1;
            }
        }

        /// <summary>
        /// Gets or sets the payment transaction code. Used to help double-charging
        /// </summary>
        protected string TransactionCode
        {
            get { return ViewState["TransactionCode"] as string ?? string.Empty; }
            set { ViewState["TransactionCode"] = value; }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState[REGISTRATION_INSTANCE_STATE_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                SetRegistrationState();
            }
            else
            {
                RegistrationInstanceState = JsonConvert.DeserializeObject<RegistrationInstance>( json );
            }

            json = ViewState[REGISTRATION_STATE_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                RegistrationState = new RegistrationInfo();
            }
            else
            {
                RegistrationState = JsonConvert.DeserializeObject<RegistrationInfo>( json );
            }

            GroupId = ViewState[GROUP_ID_KEY] as int?;
            CurrentPanel = ViewState[CURRENT_PANEL_KEY] as int? ?? 0;
            CurrentRegistrantIndex = ViewState[CURRENT_REGISTRANT_INDEX_KEY] as int? ?? 0;
            CurrentFormIndex = ViewState[CURRENT_FORM_INDEX_KEY] as int? ?? 0;

            CreateDynamicControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RegisterClientScript();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Reset warning/error messages
            nbMain.Visible = false;

            // register navigation event to enable support for the back button
            var sm = ScriptManager.GetCurrent( Page );
            //sm.EnableSecureHistoryState = false;
            sm.Navigate += sm_Navigate;

            if ( !Page.IsPostBack )
            {
                // Get the a registration (either by reading existing, or creating new one
                SetRegistrationState();

                if ( RegistrationTemplate != null )
                {
                    bool instanceFull = false;
                    if ( RegistrationInstanceState.MaxAttendees > 0 )
                    {
                        int registrants = RegistrationInstanceState.Registrations.Sum( r => r.Registrants.Count() );
                        instanceFull = registrants >= RegistrationInstanceState.MaxAttendees;
                    }

                    if ( instanceFull )
                    {
                        ShowWarning(
                            string.Format( "{0} Full", RegistrationTerm ),
                            string.Format( "There are not any more {0} available for {1}.", RegistrationTerm.ToLower().Pluralize(), RegistrationInstanceState.Name ) );

                    }
                    else
                    {
                        // Check Login Requirement
                        if ( RegistrationTemplate.LoginRequired && CurrentUser == null )
                        {
                            var site = RockPage.Site;
                            if ( site.LoginPageId.HasValue )
                            {
                                site.RedirectToLoginPage( true );
                            }
                            else
                            {
                                System.Web.Security.FormsAuthentication.RedirectToLoginPage();
                            }
                        }
                        else
                        {
                            // show the panel for asking how many registrants ( it may be skipped )
                            ShowHowMany();
                        }
                    }
                }
                else
                {
                    ShowWarning( "Sorry", string.Format( "The selected {0} could not be found or is no longer active.", RegistrationTerm.ToLower() ) );
                }
            }
            else
            {
                // Load values from controls into the state objects
                ParseDynamicControls();
            }
        }

        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            if ( RegistrationInstanceState == null )
            {
                SetRegistrationState();
            }

            if ( RegistrationInstanceState != null )
            {
                RockPage.Title = RegistrationInstanceState.Name;
                breadCrumbs.Add( new BreadCrumb( RegistrationInstanceState.Name, pageReference ) );
                return breadCrumbs;
            }

            breadCrumbs.Add( new BreadCrumb( this.PageCache.PageTitle, pageReference ) );
            return breadCrumbs;
        }
        
        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[REGISTRATION_INSTANCE_STATE_KEY] = JsonConvert.SerializeObject( RegistrationInstanceState, Formatting.None, jsonSetting );
            ViewState[REGISTRATION_STATE_KEY] = JsonConvert.SerializeObject( RegistrationState, Formatting.None, jsonSetting );

            ViewState[GROUP_ID_KEY] = GroupId;
            ViewState[CURRENT_PANEL_KEY] = CurrentPanel;
            ViewState[CURRENT_REGISTRANT_INDEX_KEY] = CurrentRegistrantIndex;
            ViewState[CURRENT_FORM_INDEX_KEY] = CurrentFormIndex;

            return base.SaveViewState();
        }

        protected override void Render( HtmlTextWriter writer )
        {
            base.Render( writer );
        }
        #endregion

        #region Events

        #region Navigation Events

        /// <summary>
        /// Handles the Navigate event of the sm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        void sm_Navigate( object sender, HistoryEventArgs e )
        {
            var state = e.State["event"];

            if ( state != null )
            {
                string[] commands = state.Split( ',' );

                int panelId = 0;
                int registrantId = 0;
                int formId = 0;

                if ( commands.Count() == 3 )
                {
                    panelId = Int32.Parse( commands[0] );
                    registrantId = Int32.Parse( commands[1] );
                    formId = Int32.Parse( commands[2] );
                }

                switch ( panelId )
                {
                    case 1:
                        {
                            CurrentRegistrantIndex = registrantId;
                            CurrentFormIndex = formId;
                            ShowRegistrant();
                            break;
                        }
                    case 2:
                        {
                            ShowSummary();
                            break;
                        }
                    default:
                        {
                            ShowHowMany();
                            break;
                        }
                }
            }
            else
            {
                ShowHowMany();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbHowManyNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHowManyNext_Click( object sender, EventArgs e )
        {
            CurrentRegistrantIndex = 0;
            CurrentFormIndex = 0;

            // Create registrants based on the number selected
            SetRegistrantState( numHowMany.Value );

            ShowRegistrant();

            hfTriggerScroll.Value = "true";

            this.AddHistory( "event", "1,0,0" );
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 1 )
            {
                CurrentFormIndex--;
                if ( CurrentFormIndex < 0 )
                {
                    CurrentRegistrantIndex--;
                    CurrentFormIndex = FormCount - 1;
                }

                if ( CurrentRegistrantIndex < 0 )
                {
                    ShowHowMany();
                    this.AddHistory( "event", "0,0,0" );
                }
                else
                {
                    ShowRegistrant();
                    this.AddHistory( "event", string.Format("1,{0},{1}", CurrentRegistrantIndex.ToString(), CurrentFormIndex.ToString()) );
                }
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantNext_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 1 )
            {
                CurrentFormIndex++;
                if ( CurrentFormIndex >= FormCount )
                {
                    CurrentRegistrantIndex++;
                    CurrentFormIndex = 0;
                }

                if ( CurrentRegistrantIndex >= RegistrationState.RegistrantCount )
                {
                    ShowSummary();
                    this.AddHistory( "event", "2,0,0" );
                }
                else
                {
                    ShowRegistrant();
                    this.AddHistory( "event", string.Format( "1,{0},{1}", CurrentRegistrantIndex.ToString(), CurrentFormIndex.ToString() ) );
                }
            }
            else
            {
                ShowHowMany();
                this.AddHistory( "event", "0,0,0" );
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                CurrentRegistrantIndex = RegistrationState != null ? RegistrationState.RegistrantCount - 1 : 0;
                CurrentFormIndex = FormCount - 1;

                ShowRegistrant();
                this.AddHistory( "event", string.Format( "1,{0},{1}", CurrentRegistrantIndex.ToString(), CurrentFormIndex.ToString() ) );
            }
            else
            {
                ShowHowMany();
                this.AddHistory( "event", "0,0,0" );
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryNext_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                var registrationId = SaveChanges();
                if ( registrationId.HasValue )
                {
                    ShowSuccess( registrationId.Value );
                    this.AddHistory( "event", "2,0,0" );
                }
                else
                {
                    ShowSummary();
                    this.AddHistory( "event", "2,0,0" );
                }
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConfirm_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == 2 )
            {
                TransactionCode = string.Empty;

                var registrationId = SaveChanges();
                if ( registrationId.HasValue )
                {
                    ShowSuccess( registrationId.Value );
                }
                else
                {
                    ShowSummary();
                    this.AddHistory( "event", "2,0,0" );
                }
            }
        }

        #endregion

        #region Summary Panel Events 

        /// <summary>
        /// Handles the Click event of the lbDiscountApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDiscountApply_Click( object sender, EventArgs e )
        {
            if ( RegistrationState != null )
            {
                RegistrationState.DiscountCode = tbDiscountCode.Text;
                CreateDynamicControls( true );
            }
        }

        #endregion

        #endregion

        #region Methods

        #region State Methods

        /// <summary>
        /// Sets the registration state
        /// </summary>
        private void SetRegistrationState()
        {
            string registrationSlug = PageParameter( REGISTRATION_SLUG_PARAM_NAME );
            int? registrationInstanceId = PageParameter( REGISTRATION_INSTANCE_ID_PARAM_NAME ).AsIntegerOrNull();
            int? registrationId = PageParameter( REGISTRATION_ID_PARAM_NAME ).AsIntegerOrNull();
            int? groupId = PageParameter( REGISTRATION_GROUP_ID_PARAM_NAME ).AsIntegerOrNull();
            int? campusId = PageParameter( REGISTRATION_CAMPUS_ID_PARAM_NAME ).AsIntegerOrNull();

            // Not inside a "using" due to serialization needing context to still be active
            var rockContext = new RockContext();

            // An existing registration id was specified
            if ( registrationId.HasValue )
            {
                var registrationService = new RegistrationService( rockContext );
                var registration = registrationService
                    .Queryable( "Registrants.PersonAlias.Person,Registrants.GroupMember,RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                    .Where( r => r.Id == registrationId.Value )
                    .FirstOrDefault();
                if ( registration != null )
                {
                    RegistrationInstanceState = registration.RegistrationInstance;
                    RegistrationState = new RegistrationInfo( registration, rockContext );
                    RegistrationState.PreviousPaymentTotal = registrationService.GetTotalPayments( registration.Id );
                }
            }

            // A registration slug was specified
            if ( RegistrationState == null && !string.IsNullOrWhiteSpace( registrationSlug ) )
            {
                var dateTime = RockDateTime.Now;
                var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable( "RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                    .Where( l => 
                        l.UrlSlug == registrationSlug &&
                        l.RegistrationInstance != null &&
                        l.RegistrationInstance.IsActive &&
                        l.RegistrationInstance.RegistrationTemplate != null &&
                        l.RegistrationInstance.RegistrationTemplate.IsActive &&
                        (!l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                        (!l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime )  )
                    .FirstOrDefault();

                if ( linkage != null )
                {
                    RegistrationInstanceState = linkage.RegistrationInstance;
                    GroupId = linkage.GroupId;
                    RegistrationState = new RegistrationInfo( CurrentPerson );
                }
            }

            // A group id and campus id were specified
            if ( RegistrationState == null && groupId.HasValue && campusId.HasValue )
            {
                var dateTime = RockDateTime.Now;
                var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable( "RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                    .Where( l =>
                        l.GroupId == groupId &&
                        l.EventItemOccurrence != null &&
                        l.EventItemOccurrence.CampusId == campusId &&
                        l.RegistrationInstance != null &&
                        l.RegistrationInstance.IsActive &&
                        l.RegistrationInstance.RegistrationTemplate != null &&
                        l.RegistrationInstance.RegistrationTemplate.IsActive &&
                        ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                        ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                    .FirstOrDefault();

                if ( linkage != null )
                {
                    RegistrationInstanceState = linkage.RegistrationInstance;
                    GroupId = linkage.GroupId;
                    RegistrationState = new RegistrationInfo( CurrentPerson );
                }
            }

            // A registratio instance id was specified
            if ( RegistrationState == null && registrationInstanceId.HasValue )
            {
                var dateTime = RockDateTime.Now;
                RegistrationInstanceState = new RegistrationInstanceService( rockContext )
                    .Queryable( "Account,RegistrationTemplate.Fees,RegistrationTemplate.Discounts,RegistrationTemplate.Forms.Fields.Attribute,RegistrationTemplate.FinancialGateway" )
                    .Where( r =>
                        r.Id == registrationInstanceId.Value &&
                        r.IsActive &&
                        r.RegistrationTemplate != null &&
                        r.RegistrationTemplate.IsActive &&
                        ( !r.StartDateTime.HasValue || r.StartDateTime <= dateTime ) &&
                        ( !r.EndDateTime.HasValue || r.EndDateTime > dateTime ) )
                    .FirstOrDefault();

                if ( RegistrationInstanceState != null )
                {
                    RegistrationState = new RegistrationInfo( CurrentPerson );
                }
            }

            if ( RegistrationState != null && !RegistrationState.Registrants.Any() )
            {
                SetRegistrantState( 1 );
            }
            
        }

        /// <summary>
        /// Adds (or removes) registrants to or from the registration. Only newly added registrants can
        /// can be removed. Any existing (saved) registrants cannot be removed from the registration
        /// </summary>
        /// <param name="registrantCount">The number of registrants that registration should have.</param>
        private void SetRegistrantState( int registrantCount )
        {
            if ( RegistrationState != null )
            {
                var firstFamilyGuid = RegistrationState.RegistrantCount > 0 ? RegistrationState.Registrants[0].FamilyGuid : Guid.NewGuid();

                // While the number of registrants belonging to registration is less than the selected count, addd another registrant
                while ( RegistrationState.RegistrantCount < registrantCount )
                {
                    var registrant = new RegistrantInfo { Cost = RegistrationTemplate.Cost };
                    if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.No )
                    {
                        registrant.FamilyGuid = Guid.NewGuid();
                    } 
                    else if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes )
                    {
                        registrant.FamilyGuid = firstFamilyGuid;
                    }

                    RegistrationState.Registrants.Add( registrant );
                }

                // Get the number of registrants that needs to be removed. 
                int removeCount = RegistrationState.RegistrantCount - registrantCount;
                if ( removeCount > 0 )
                {
                    // If removing any, reverse the order of registrants, so that most recently added will be removed first
                    RegistrationState.Registrants.Reverse();

                    // Try to get the registrants to remove. Most recently added will be taken first
                    foreach ( var registrant in RegistrationState.Registrants.Take( removeCount ).ToList() )
                    {
                        RegistrationState.Registrants.Remove( registrant );
                    }

                    // Reset the order after removing any registrants
                    RegistrationState.Registrants.Reverse();
                }
            }
        }

        #endregion

        #region Save Methods

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns></returns>
        private int? SaveChanges()
        {
            Registration registration = null;

            if ( !string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                ShowError( string.Empty, "You have already completed this " + RegistrationTerm.ToLower() );
            }
            else
            {
                try
                {
                    if ( RegistrationState != null && RegistrationState.Registrants.Any() && RegistrationTemplate != null )
                    {
                        var rockContext = new RockContext();

                        rockContext.WrapTransaction( () =>
                        {
                            bool hasPayment = RegistrationState.PaymentAmount > 0.0m;

                            if ( RegistrationState.RegistrationId.HasValue )
                            {
                                registration = new RegistrationService( rockContext ).Get( RegistrationState.RegistrationId.Value );
                            }
                            else
                            {
                                registration = SaveRegistration( rockContext, hasPayment );
                            }

                            if ( registration != null )
                            {
                                if ( hasPayment )
                                {
                                    string errorMessage = string.Empty;
                                    if ( !ProcessPayment( rockContext, registration, out errorMessage ) )
                                    {
                                        registration = null;
                                        throw new Exception( errorMessage );
                                    }
                                }

                                if ( registration != null )
                                {
                                    string appRoot = ResolveRockUrl( "~/" );
                                    string themeRoot = ResolveRockUrl( "~~/" );

                                    var confirmation = new Rock.Transactions.SendRegistrationConfirmationTransaction();
                                    confirmation.RegistrationId = registration.Id;
                                    confirmation.AppRoot = appRoot;
                                    confirmation.ThemeRoot = themeRoot;
                                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( confirmation );

                                    var notification = new Rock.Transactions.SendRegistrationNotificationTransaction();
                                    notification.RegistrationId = registration.Id;
                                    notification.AppRoot = appRoot;
                                    notification.ThemeRoot = themeRoot;
                                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( notification );
                                }
                            }
                        } );

                        // Re-create State
                        if ( registration != null )
                        {
                            var registrationService = new RegistrationService( rockContext );
                            var newRegistration = registrationService
                                .Queryable( "Registrants.PersonAlias.Person,Registrants.GroupMember,RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                                .AsNoTracking()
                                .Where( r => r.Id == registration.Id )
                                .FirstOrDefault();
                            if ( newRegistration != null )
                            {
                                RegistrationInstanceState = newRegistration.RegistrationInstance;
                                RegistrationState = new RegistrationInfo( newRegistration, rockContext );
                                RegistrationState.PreviousPaymentTotal = registrationService.GetTotalPayments( registration.Id );
                            }
                        }

                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
                    ShowError( "An Error Occurred Processing Your " + RegistrationTerm, ex.Message );
                    return (int?)null;
                }
            }

            return registration != null ? registration.Id : (int?)null;
        }

        /// <summary>
        /// Saves the registration.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="hasPayment">if set to <c>true</c> [has payment].</param>
        /// <returns></returns>
        private Registration SaveRegistration( RockContext rockContext, bool hasPayment )
        {
            var registrationService = new RegistrationService( rockContext );
            var registrantService = new RegistrationRegistrantService( rockContext );
            var personService = new PersonService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );

            // variables to keep track of the family that new people should be added to
            int? singleFamilyId = null;
            var multipleFamilyGroupIds = new Dictionary<Guid, int>();

            var dvcConnectionStatus = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            var dvcRecordStatus = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() );
            var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            var adultRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();
            var childRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            var registration = new Registration();
            registrationService.Add( registration );
            registration.RegistrationInstanceId = RegistrationInstanceState.Id;
            registration.GroupId = GroupId;
            registration.FirstName = RegistrationState.FirstName;
            registration.LastName = RegistrationState.LastName;
            registration.ConfirmationEmail = RegistrationState.ConfirmationEmail;
            registration.DiscountCode = RegistrationState.DiscountCode;
            registration.DiscountAmount = RegistrationState.DiscountAmount;
            registration.DiscountPercentage = RegistrationState.DiscountPercentage;

            // If the 'your name' value equals the currently logged in person, use their person alias id
            if ( CurrentPerson != null &&
                ( CurrentPerson.NickName.Trim().Equals( registration.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) ||
                    CurrentPerson.FirstName.Trim().Equals( registration.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) ) &&
                CurrentPerson.LastName.Trim().Equals( registration.LastName.Trim(), StringComparison.OrdinalIgnoreCase ) )
            {
                registration.PersonAliasId = CurrentPerson.PrimaryAliasId;
            }
            else
            {
                // otherwise look for one and one-only match by name/email
                var personMatches = personService.GetByMatch( registration.FirstName, registration.LastName, registration.ConfirmationEmail );
                if ( personMatches.Count() == 1 )
                {
                    registration.PersonAliasId = personMatches.First().PrimaryAliasId;
                }
            }

            // If the registration includes a payment, make sure there's an actual person associated to registration
            if ( hasPayment && !registration.PersonAliasId.HasValue )
            {
                // If a match was not found, create a new person
                var person = new Person();
                person.FirstName = registration.FirstName;
                person.LastName = registration.LastName;
                person.IsEmailActive = true;
                person.Email = registration.ConfirmationEmail;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                if ( dvcConnectionStatus != null )
                {
                    person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                }
                if ( dvcRecordStatus != null )
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }

                registration.PersonAliasId = SavePerson( rockContext, person, Guid.NewGuid(), null, null, adultRoleId, childRoleId, multipleFamilyGroupIds, singleFamilyId );
            }

            // Save the registration ( so we can get an id )
            rockContext.SaveChanges();

            // If the Registration Instance linkage specified a group, load it now
            Group group = null;
            if ( GroupId.HasValue )
            {
                group = new GroupService( rockContext ).Get( GroupId.Value );
            }

            // Get each registrant
            foreach ( var registrantInfo in RegistrationState.Registrants )
            {
                var changes = new List<string>();
                var familyChanges = new List<string>();

                Person person = null;

                // Try to find a matching person based on name and email address
                string firstName = registrantInfo.GetFirstName( RegistrationTemplate );
                string lastName = registrantInfo.GetLastName( RegistrationTemplate );
                string email = registrantInfo.GetEmail( RegistrationTemplate );
                var personMatches = personService.GetByMatch( firstName, lastName, email );
                if ( personMatches.Count() == 1 )
                {
                    person = personMatches.First();
                }

                if ( person == null )
                {
                    // If a match was not found, create a new person
                    person = new Person();
                    person.FirstName = firstName;
                    person.LastName = lastName;
                    person.IsEmailActive = true;
                    person.Email = email;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    if ( dvcConnectionStatus != null )
                    {
                        person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                    }

                    if ( dvcRecordStatus != null )
                    {
                        person.RecordStatusValueId = dvcRecordStatus.Id;
                    }
                }

                int? campusId = null;
                Location location = null;

                // Set any of the template's person fields
                foreach ( var field in RegistrationTemplate.Forms
                    .SelectMany( f => f.Fields
                        .Where( t => t.FieldSource == RegistrationFieldSource.PersonField ) ) )
                {
                    // Find the registrant's value
                    var fieldValue = registrantInfo.FieldValues
                        .Where( f => f.Key == field.Id )
                        .Select( f => f.Value )
                        .FirstOrDefault();


                    if ( fieldValue != null )
                    {
                        switch ( field.PersonFieldType )
                        {
                            case RegistrationPersonFieldType.Campus:
                                {
                                    if ( fieldValue != null )
                                    {
                                        campusId = fieldValue.ToString().AsIntegerOrNull();
                                    } 
                                    break;
                                }

                            case RegistrationPersonFieldType.Address:
                                {
                                    location = fieldValue.ToString().FromJsonOrNull<Location>();
                                    break;
                                }

                            case RegistrationPersonFieldType.Birthdate:
                                {
                                    var birthMonth = person.BirthMonth;
                                    var birthDay = person.BirthDay;
                                    var birthYear = person.BirthYear;

                                    person.SetBirthDate( fieldValue as DateTime? );

                                    History.EvaluateChange( changes, "Birth Month", birthMonth, person.BirthMonth );
                                    History.EvaluateChange( changes, "Birth Day", birthDay, person.BirthDay );
                                    History.EvaluateChange( changes, "Birth Year", birthYear, person.BirthYear );

                                    break;
                                }

                            case RegistrationPersonFieldType.Gender:
                                {
                                    var newGender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                                    History.EvaluateChange( changes, "Gender", person.Gender, newGender );
                                    person.Gender = newGender;
                                    break;
                                }

                            case RegistrationPersonFieldType.MaritalStatus:
                                {
                                    if ( fieldValue != null  )
                                    {
                                        int? newMaritalStatusId = fieldValue.ToString().AsIntegerOrNull();
                                        History.EvaluateChange( changes, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( newMaritalStatusId ) );
                                        person.MaritalStatusValueId = newMaritalStatusId;
                                    }
                                    break;
                                }

                            case RegistrationPersonFieldType.MobilePhone:
                                {
                                    SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), changes );
                                    break;
                                }

                            case RegistrationPersonFieldType.HomePhone:
                                {
                                    SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), changes );
                                    break;
                                }

                            case RegistrationPersonFieldType.WorkPhone:
                                {
                                    SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid(), changes );
                                    break;
                                }
                        }
                    }
                }

                // Save the person ( and family if needed )
                SavePerson( rockContext, person, registrantInfo.FamilyGuid, campusId, location, adultRoleId, childRoleId, multipleFamilyGroupIds, singleFamilyId );

                // Load the person's attributes
                person.LoadAttributes();

                // Set any of the template's person fields
                foreach ( var field in RegistrationTemplate.Forms
                    .SelectMany( f => f.Fields
                        .Where( t => 
                            t.FieldSource == RegistrationFieldSource.PersonAttribute &&
                            t.AttributeId.HasValue ) ) )
                {
                    // Find the registrant's value
                    var fieldValue = registrantInfo.FieldValues
                        .Where( f => f.Key == field.Id )
                        .Select( f => f.Value )
                        .FirstOrDefault();

                    if ( fieldValue != null )
                    {
                        var attribute = AttributeCache.Read( field.AttributeId.Value );
                        if ( attribute != null )
                        {
                            string originalValue = person.GetAttributeValue( attribute.Key );
                            string newValue = fieldValue.ToString();
                            person.SetAttributeValue( attribute.Key, fieldValue.ToString() );

                            if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                            {
                                string formattedOriginalValue = string.Empty;
                                if ( !string.IsNullOrWhiteSpace( originalValue ) )
                                {
                                    formattedOriginalValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                }

                                string formattedNewValue = string.Empty;
                                if ( !string.IsNullOrWhiteSpace( newValue ) )
                                {
                                    formattedNewValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                                }

                                History.EvaluateChange( changes, attribute.Name, formattedOriginalValue, formattedNewValue );
                            }
                        }
                    }
                }

                person.SaveAttributeValues( rockContext );

                GroupMember groupMember = null;

                // If the registration instance linkage specified a group to add registrant to, add them if there not already
                // part of that group
                if ( group != null )
                {
                    groupMember = group.Members.Where( m => m.PersonId == person.Id ).FirstOrDefault();
                    if ( groupMember == null && group.GroupType.DefaultGroupRoleId.HasValue )
                    {
                        groupMember = new GroupMember();
                        groupMemberService.Add( groupMember );
                        groupMember.GroupId = group.Id;
                        groupMember.PersonId = person.Id;

                        if ( RegistrationTemplate.GroupTypeId.HasValue &&
                            RegistrationTemplate.GroupTypeId == group.GroupTypeId &&
                            RegistrationTemplate.GroupMemberRoleId.HasValue )
                        {
                            groupMember.GroupRoleId = RegistrationTemplate.GroupMemberRoleId.Value;
                            groupMember.GroupMemberStatus = RegistrationTemplate.GroupMemberStatus;
                        }
                        else
                        {
                            groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;
                            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                        }
                    }

                    rockContext.SaveChanges();

                    // Set any of the template's group member attributes 
                    groupMember.LoadAttributes();

                    foreach ( var field in RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t =>
                                t.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                                t.AttributeId.HasValue ) ) )
                    {
                        // Find the registrant's value
                        var fieldValue = registrantInfo.FieldValues
                            .Where( f => f.Key == field.Id )
                            .Select( f => f.Value )
                            .FirstOrDefault();

                        if ( fieldValue != null )
                        {
                            var attribute = AttributeCache.Read( field.AttributeId.Value );
                            if ( attribute != null )
                            {
                                groupMember.SetAttributeValue( attribute.Key, fieldValue.ToString() );
                            }
                        }
                    }

                    groupMember.SaveAttributeValues( rockContext );
                }

                var registrant = new RegistrationRegistrant();
                registrantService.Add( registrant );
                registrant.RegistrationId = registration.Id;
                registrant.PersonAliasId = person.PrimaryAliasId;
                registrant.Cost = registrantInfo.Cost;
                registrant.GroupMemberId = groupMember != null ? groupMember.Id : (int?)null;

                // Add or Update fees
                foreach ( var feeValue in registrantInfo.FeeValues.Where( f => f.Value != null ) )
                {
                    foreach ( var uiFee in feeValue.Value )
                    {
                        var fee = new RegistrationRegistrantFee();
                        registrant.Fees.Add( fee );
                        fee.RegistrationTemplateFeeId = feeValue.Key;
                        fee.Option = uiFee.Option;
                        fee.Quantity = uiFee.Quantity;
                        fee.Cost = uiFee.Cost;
                    }
                }

                rockContext.SaveChanges();

                // Set any of the templat's registrant attributes
                registrant.LoadAttributes();
                foreach ( var field in RegistrationTemplate.Forms
                    .SelectMany( f => f.Fields
                        .Where( t =>
                            t.FieldSource == RegistrationFieldSource.RegistrationAttribute &&
                            t.AttributeId.HasValue ) ) )
                {
                    // Find the registrant's value
                    var fieldValue = registrantInfo.FieldValues
                        .Where( f => f.Key == field.Id )
                        .Select( f => f.Value )
                        .FirstOrDefault();

                    if ( fieldValue != null )
                    {
                        var attribute = AttributeCache.Read( field.AttributeId.Value );
                        if ( attribute != null )
                        {
                            registrant.SetAttributeValue( attribute.Key, fieldValue.ToString() );
                        }
                    }

                    registrant.SaveAttributeValues( rockContext );
                }
            }

            return registration;

        }

        /// <summary>
        /// Saves the person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="familyGuid">The family unique identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="adultRoleId">The adult role identifier.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="multipleFamilyGroupIds">The multiple family group ids.</param>
        /// <param name="singleFamilyId">The single family identifier.</param>
        /// <returns></returns>
        private int? SavePerson( RockContext rockContext, Person person, Guid familyGuid, int? campusId, Location location, int adultRoleId, int childRoleId,
            Dictionary<Guid, int> multipleFamilyGroupIds, int? singleFamilyId )
        {
            if ( person.Id > 0 )
            {
                rockContext.SaveChanges();
            }
            else
            {
                // If we've created the family aready for this registrant, add them to it
                if (
                        ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask && multipleFamilyGroupIds.ContainsKey( familyGuid ) ) ||
                        ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes && singleFamilyId.HasValue )
                    )
                {

                    // Add person to existing family
                    var age = person.Age;
                    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;

                    int familyId = RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask ?
                        multipleFamilyGroupIds[familyGuid] :
                        singleFamilyId.Value;
                    PersonService.AddPersonToFamily( person, true, multipleFamilyGroupIds[familyGuid], familyRoleId, rockContext );

                    if ( location != null )
                    {
                        Guid locTypeGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
                        var familyGroup = new GroupService( rockContext ).Get( familyId );

                        // Do not update existing location on an existing family ( only update when creating new family or location doesn't already exist )
                        if ( familyGroup != null && !familyGroup.GroupLocations.Any( l => l.GroupLocationTypeValue.Guid.Equals( locTypeGuid ) ) )
                        {
                            GroupService.AddNewFamilyAddress(
                                rockContext,
                                familyGroup,
                                Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                                location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country );
                        }
                    }
                }

                // otherwise create a new family
                else
                {
                    // Create Person/Family
                    var familyGroup = PersonService.SaveNewPerson( person, rockContext, campusId, false );
                    if ( familyGroup != null )
                    {
                        if ( location != null )
                        {
                            GroupService.AddNewFamilyAddress(
                                rockContext,
                                familyGroup,
                                Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                                location.Street1, location.Street2, location.City, location.State, location.PostalCode, location.Country );
                        }

                        // Store the family id for next person 
                        multipleFamilyGroupIds.Add( familyGuid, familyGroup.Id );
                        if ( !singleFamilyId.HasValue )
                        {
                            singleFamilyId = familyGroup.Id;
                        }
                    }
                }
            }

            var newPerson = new PersonService( rockContext ).Get( person.Id );
            return newPerson != null ? newPerson.PrimaryAliasId : (int?)null;
        }

        /// <summary>
        /// Saves the phone.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="person">The person.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        /// <param name="changes">The changes.</param>
        private void SavePhone( object fieldValue, Person person, Guid phoneTypeGuid, List<string> changes )
        {
            var phoneNumber = fieldValue.ToString().FromJsonOrNull<PhoneNumber>();
            if ( phoneNumber != null )
            {
                var numberType = DefinedValueCache.Read( phoneTypeGuid );
                if ( numberType != null )
                {
                    var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );
                    string oldPhoneNumber = string.Empty;
                    if ( phone == null )
                    {
                        phone = new PhoneNumber();
                        person.PhoneNumbers.Add( phone );
                        phone.NumberTypeValueId = numberType.Id;
                    }
                    else
                    {
                        oldPhoneNumber = phone.NumberFormattedWithCountryCode;
                    }
                    phone.CountryCode = PhoneNumber.CleanNumber( phoneNumber.CountryCode );
                    phone.Number = PhoneNumber.CleanNumber( phoneNumber.Number );

                    History.EvaluateChange(
                        changes,
                        string.Format( "{0} Phone", numberType.Value ),
                        oldPhoneNumber,
                        phoneNumber.NumberFormattedWithCountryCode );
                }
            }
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessPayment( RockContext rockContext, Registration registration, out string errorMessage )
        {
            GatewayComponent gateway = null;
            if ( RegistrationTemplate != null && RegistrationTemplate.FinancialGateway != null )
            {
                gateway = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
            }

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( RegistrationInstanceState.Account == null )
            {
                errorMessage = "There was a problem with the account configuration for this " + RegistrationTerm.ToLower();
                return false;
            }

            var paymentInfo = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate.Value );
            paymentInfo.NameOnCard = gateway != null && gateway.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
            paymentInfo.LastNameOnCard = txtCardLastName.Text;

            paymentInfo.BillingStreet1 = acBillingAddress.Street1;
            paymentInfo.BillingStreet2 = acBillingAddress.Street2;
            paymentInfo.BillingCity = acBillingAddress.City;
            paymentInfo.BillingState = acBillingAddress.State;
            paymentInfo.BillingPostalCode = acBillingAddress.PostalCode;
            paymentInfo.BillingCountry = acBillingAddress.Country;

            paymentInfo.Amount = RegistrationState.PaymentAmount;
            paymentInfo.Email = RegistrationState.ConfirmationEmail;

            paymentInfo.FirstName = RegistrationState.FirstName;
            paymentInfo.LastName = RegistrationState.LastName;

            var transaction = gateway.Charge( RegistrationTemplate.FinancialGateway, paymentInfo, out errorMessage );
            if ( transaction != null )
            {
                var txnChanges = new List<string>();
                txnChanges.Add( "Created Transaction" );

                History.EvaluateChange( txnChanges, "Transaction Code", string.Empty, transaction.TransactionCode );

                transaction.AuthorizedPersonAliasId = registration.PersonAliasId;

                transaction.TransactionDateTime = RockDateTime.Now;
                History.EvaluateChange( txnChanges, "Date/Time", null, transaction.TransactionDateTime );

                transaction.FinancialGatewayId = RegistrationTemplate.FinancialGatewayId;
                History.EvaluateChange( txnChanges, "Gateway", string.Empty, RegistrationTemplate.FinancialGateway.Name );

                var txnType = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
                transaction.TransactionTypeValueId = txnType.Id;
                History.EvaluateChange( txnChanges, "Type", string.Empty, txnType.Value );

                transaction.CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
                History.EvaluateChange( txnChanges, "Currency Type", string.Empty, paymentInfo.CurrencyTypeValue.Value );

                transaction.CreditCardTypeValueId = paymentInfo.CreditCardTypeValue != null ? paymentInfo.CreditCardTypeValue.Id : (int?)null;
                if ( transaction.CreditCardTypeValueId.HasValue )
                {
                    var ccType = DefinedValueCache.Read( transaction.CreditCardTypeValueId.Value );
                    History.EvaluateChange( txnChanges, "Credit Card Type", string.Empty, ccType.Value );
                }

                Guid sourceGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
                {
                    var source = DefinedValueCache.Read( sourceGuid );
                    if ( source != null )
                    {
                        transaction.SourceTypeValueId = source.Id;
                        History.EvaluateChange( txnChanges, "Source", string.Empty, source.Value );
                    }
                }

                transaction.Summary = registration.GetSummary( RegistrationInstanceState );

                var transactionDetail = new FinancialTransactionDetail();
                transactionDetail.Amount = RegistrationState.PaymentAmount;
                transactionDetail.AccountId = RegistrationInstanceState.AccountId;
                transactionDetail.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Registration ) ).Id;
                transactionDetail.EntityId = registration.Id;
                transaction.TransactionDetails.Add( transactionDetail );

                History.EvaluateChange( txnChanges, RegistrationInstanceState.Account.Name, 0.0M.ToString( "C2" ), transactionDetail.Amount.ToString( "C2" ) );

                var batchService = new FinancialBatchService( rockContext );

                // Get the batch
                var batch = batchService.Get(
                    GetAttributeValue( "BatchNamePrefix" ),
                    paymentInfo.CurrencyTypeValue,
                    paymentInfo.CreditCardTypeValue,
                    transaction.TransactionDateTime.Value,
                    RegistrationTemplate.FinancialGateway.GetBatchTimeOffset() );

                var batchChanges = new List<string>();

                if ( batch.Id == 0 )
                {
                    batchChanges.Add( "Generated the batch" );
                    History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                    History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                    History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                    History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
                }

                decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
                History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.ToString( "C2" ), newControlAmount.ToString( "C2" ) );
                batch.ControlAmount = newControlAmount;

                transaction.BatchId = batch.Id;
                batch.Transactions.Add( transaction );

                rockContext.SaveChanges();

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                    batch.Id,
                    batchChanges
                );

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                    batch.Id,
                    txnChanges,
                    CurrentPerson != null ? CurrentPerson.FullName : string.Empty,
                    typeof( FinancialTransaction ),
                    transaction.Id
                );

                TransactionCode = transaction.TransactionCode;

                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Shows the how many panel
        /// </summary>
        private void ShowHowMany()
        {
            lRegistrantTerm.Text =RegistrantTerm.Pluralize().ToLower();

            // If this is an existing registration, go directly to the summary
            if ( RegistrationState != null && RegistrationState.RegistrationId.HasValue )
            {
                ShowSummary();
            }
            else
            {
                if ( MaxRegistrants > MinRegistrants )
                {
                    // If registration allows multiple registrants show the 'How Many' panel
                    numHowMany.Maximum = MaxRegistrants;
                    numHowMany.Minimum = MinRegistrants;
                    numHowMany.Value = RegistrationState != null ? RegistrationState.RegistrantCount : 1;

                    lbRegistrantPrev.Visible = true;

                    SetPanel( 0 );
                }
                else
                {
                    // ... else skip to the registrant panel
                    CurrentRegistrantIndex = 0;
                    CurrentFormIndex = 0;

                    SetRegistrantState( MinRegistrants );

                    lbRegistrantPrev.Visible = false;

                    ShowRegistrant();
                }
            }
        }

        /// <summary>
        /// Shows the registrant panel
        /// </summary>
        private void ShowRegistrant()
        {
            if ( RegistrationState != null && RegistrationState.RegistrantCount > 0 )
            {
                string title = RegistrationState.RegistrantCount <= 1 ? 
                    RegistrantTerm : 
                    ( CurrentRegistrantIndex + 1 ).ToOrdinalWords().Humanize( LetterCasing.Title ) + " " + RegistrantTerm;

                if ( CurrentFormIndex > 0 )
                {
                    title += " (cont)";
                }
                lRegistrantTitle.Text = title;

                rblFamilyOptions.Visible = 
                    CurrentRegistrantIndex > 0 && 
                    RegistrationTemplate != null && 
                    RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask;

                SetPanel( 1 );
            }
        }

        /// <summary>
        /// Shows the summary panel
        /// </summary>
        private void ShowSummary()
        {
            SetPanel( 2 );
        }

        /// <summary>
        /// Shows the success panel
        /// </summary>
        private void ShowSuccess( int registrationId )
        {
            lSuccessTitle.Text = "Congratulations";
            lSuccess.Text = "You have succesfully completed this registration.";

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    var registration = new RegistrationService( rockContext )
                        .Queryable( "RegistrationInstance.RegistrationTemplate" )
                        .FirstOrDefault( r => r.Id == registrationId );

                    if ( registration != null &&
                        registration.RegistrationInstance != null && 
                        registration.RegistrationInstance.RegistrationTemplate != null )
                    {
                        var template = registration.RegistrationInstance.RegistrationTemplate;

                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "CurrentPerson", CurrentPerson );
                        mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                        mergeFields.Add( "Registration", registration );

                        if ( template != null && !string.IsNullOrWhiteSpace( template.SuccessTitle ) )
                        {
                            lSuccessTitle.Text = template.SuccessTitle.ResolveMergeFields( mergeFields );
                        }
                        else
                        {
                            lSuccessTitle.Text = "Congratulations";
                        }

                        if ( template != null && !string.IsNullOrWhiteSpace( template.SuccessText ) )
                        {
                            lSuccess.Text = template.SuccessText.ResolveMergeFields( mergeFields );
                        }
                        else
                        {
                            lSuccess.Text = "You have succesfully completed this " + RegistrationTerm.ToLower();
                        }

                        // show debug info
                        if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && UserCanEdit )
                        {
                            lSuccessDebug.Visible = true;
                            lSuccessDebug.Text = mergeFields.lavaDebugInfo();
                        }

                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
            }

            SetPanel( 3 );
        }

        /// <summary>
        /// Creates the dynamic controls, and shows correct panel
        /// </summary>
        /// <param name="currentPanel">The current panel.</param>
        private void SetPanel( int currentPanel )
        {
            CurrentPanel = currentPanel;

            CreateDynamicControls( true );

            pnlHowMany.Visible = CurrentPanel <= 0;
            pnlRegistrant.Visible = CurrentPanel == 1;
            pnlSummaryAndPayment.Visible = CurrentPanel == 2;
            pnlSuccess.Visible = CurrentPanel == 3;
        }

        /// <summary>
        /// Shows a warning message.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="text">The text.</param>
        private void ShowWarning( string heading, string text )
        {
            nbMain.Heading = heading;
            nbMain.Text = string.Format( "<p>{0}</p>", text );
            nbMain.NotificationBoxType = NotificationBoxType.Warning;
            nbMain.Visible = true;
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="text">The text.</param>
        private void ShowError( string heading, string text )
        {
            nbMain.Heading = heading;
            nbMain.Text = string.Format( "<p>{0}</p>", text );
            nbMain.NotificationBoxType = NotificationBoxType.Danger;
            nbMain.Visible = true;
        }

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            RockPage.AddScriptLink( ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            string script = string.Format( @"
    // Adjust the label of 'is in the same family' based on value of first name entered
    $('input.js-first-name').change( function() {{
        var name = $(this).val();
        if ( name == null || name == '') {{
            name = 'Individual';
        }}
        var $lbl = $('div.js-registration-same-family').find('label.control-label')
        $lbl.text( name + ' is in the same family as');
    }} );

    $('#{0}').on('change', function() {{

        var totalCost = Number($('#{1}').val());
        var minDue = Number($('#{2}').val());
        var previouslyPaid = Number($('#{3}').val());
        var balanceDue = totalCost - previouslyPaid;

        // Format and validate the amount entered
        var amountPaid = minDue;
        var amountValue = $(this).val();
        if ( amountValue != null && amountValue != '' && !isNaN( amountValue ) ) {{
            amountPaid = Number( amountValue );
            if ( amountPaid < minDue ) {{
                amountPaid = minDue;
            }}
            if ( amountPaid > balanceDue ) {{
                amountPaid = balanceDue
            }}
        }}
        $(this).val(amountPaid.toFixed(2));

        var amountRemaining = totalCost - ( previouslyPaid + amountPaid );
        $('#{4}').text( '$' + amountRemaining.toFixed(2) );
        
    }});

    // Detect credit card type
    $('.credit-card').creditCardTypeDetector({{ 'credit_card_logos': '.card-logos' }});

    if ( $('#{5}').val() == 'true' ) {{
        setTimeout('window.scrollTo(0,0)',0);
        $('#{5}').val('')
    }}
",
            nbAmountPaid.ClientID, hfTotalCost.ClientID, hfMinimumDue.ClientID, hfPreviouslyPaid.ClientID, lRemainingDue.ClientID, hfTriggerScroll.ClientID);

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "registrationEntry", script, true );
        }

        #endregion

        #region Dynamic Control Methods

        /// <summary>
        /// Creates the dynamic controls fore each panel
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateDynamicControls( bool setValues )
        {
            switch( CurrentPanel )
            {
                case 1:
                    CreateRegistrantControls( setValues );
                    break;
                case 2:
                    CreateSummaryControls( setValues );
                    break;
            }
        }

        /// <summary>
        /// Parses the dynamic controls.
        /// </summary>
        private void ParseDynamicControls()
        {
            switch ( CurrentPanel )
            {
                case 1:
                    ParseRegistrantControls();
                    break;
                case 2:
                    ParseSummaryControls();
                    break;
            }
        }

        #region Registrant Controls

        /// <summary>
        /// Creates the registrant controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateRegistrantControls( bool setValues )
        {
            lRegistrantFeeCaption.Text = FeeTerm.Pluralize();

            phRegistrantControls.Controls.Clear();
            phFees.Controls.Clear();

            if ( FormCount > CurrentFormIndex )
            {
                // Get the current and previous registrant ( previous is used when a field has the 'IsSharedValue' property )
                // so that current registrant can use the previous registrants value
                RegistrantInfo registrant = null;
                RegistrantInfo previousRegistrant = null;

                if ( RegistrationState != null && RegistrationState.RegistrantCount > CurrentRegistrantIndex )
                {
                    registrant = RegistrationState.Registrants[CurrentRegistrantIndex];

                    // If this is not the first person, then check to see if option for asking about family should be displayed
                    if ( CurrentFormIndex == 0 && CurrentRegistrantIndex > 0 &&
                        RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask )
                    {
                        var familyOptions = RegistrationState.GetFamilyOptions( RegistrationTemplate, CurrentRegistrantIndex );
                        if ( familyOptions.Any() )
                        {
                            familyOptions.Add( familyOptions.ContainsKey( registrant.FamilyGuid ) ? 
                                Guid.NewGuid() : 
                                registrant.FamilyGuid.Equals( Guid.Empty ) ? Guid.NewGuid() : registrant.FamilyGuid,
                                "None of the above" );
                            rblFamilyOptions.DataSource = familyOptions;
                            rblFamilyOptions.DataBind();
                            rblFamilyOptions.Visible = true;
                        }
                        else
                        {
                            rblFamilyOptions.Visible = false;
                        }
                    }
                    else
                    {
                        rblFamilyOptions.Visible = false;
                    }

                    if ( setValues )
                    {
                        if ( CurrentRegistrantIndex > 0 )
                        {
                            previousRegistrant = RegistrationState.Registrants[CurrentRegistrantIndex - 1];
                        }

                        rblFamilyOptions.SetValue( registrant.FamilyGuid.ToString() );
                    }
                }

                var form = RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentFormIndex];
                foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
                {
                    object value = null;
                    if ( registrant != null && registrant.FieldValues.ContainsKey( field.Id ) )
                    {
                        value = registrant.FieldValues[field.Id];
                    }

                    if ( value == null && field.IsSharedValue && previousRegistrant != null && previousRegistrant.FieldValues.ContainsKey( field.Id ) )
                    {
                        value = previousRegistrant.FieldValues[field.Id];
                    } 
                    
                    if ( !string.IsNullOrWhiteSpace( field.PreText ) )
                    {
                        phRegistrantControls.Controls.Add( new LiteralControl( field.PreText ) );
                    }

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        CreatePersonField( field, setValues, value);
                    }
                    else
                    {
                        CreateAttributeField( field, setValues, value );
                    }

                    if ( !string.IsNullOrWhiteSpace( field.PostText ) )
                    {
                        phRegistrantControls.Controls.Add( new LiteralControl( field.PostText ) );
                    }

                }

                // If the current form, is the last one, add any fee controls
                if ( FormCount - 1 == CurrentFormIndex )
                {
                    foreach ( var fee in RegistrationTemplate.Fees )
                    {
                        var feeValues = new List<FeeInfo>();
                        if ( registrant != null && registrant.FeeValues.ContainsKey( fee.Id ) )
                        {
                            feeValues = registrant.FeeValues[fee.Id];
                        }
                        CreateFeeField( fee, setValues, feeValues );
                    }
                }
            }

            divFees.Visible = phFees.Controls.Count > 0;
        }

        /// <summary>
        /// Creates the person field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        private void CreatePersonField( RegistrationTemplateFormField field, bool setValue, object fieldValue )
        {

            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.FirstName:
                    {
                        var tbFirstName = new RockTextBox();
                        tbFirstName.ID = "tbFirstName";
                        tbFirstName.Label = "First Name";
                        tbFirstName.Required = field.IsRequired;
                        tbFirstName.ValidationGroup = BlockValidationGroup;
                        tbFirstName.AddCssClass( "js-first-name" );
                        phRegistrantControls.Controls.Add( tbFirstName );

                        if ( setValue && fieldValue != null )
                        {
                            tbFirstName.Text = fieldValue.ToString();
                        }

                        break;
                    }

                case RegistrationPersonFieldType.LastName:
                    {
                        var tbLastName = new RockTextBox();
                        tbLastName.ID = "tbLastName";
                        tbLastName.Label = "Last Name";
                        tbLastName.Required = field.IsRequired;
                        tbLastName.ValidationGroup = BlockValidationGroup;
                        phRegistrantControls.Controls.Add( tbLastName );

                        if ( setValue && fieldValue != null )
                        {
                            tbLastName.Text = fieldValue.ToString();
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Campus:
                    {
                        var cpHomeCampus = new CampusPicker();
                        cpHomeCampus.ID = "cpHomeCampus";
                        cpHomeCampus.Label = "Campus";
                        cpHomeCampus.Required = field.IsRequired;
                        cpHomeCampus.ValidationGroup = BlockValidationGroup;
                        cpHomeCampus.Campuses = CampusCache.All();

                        phRegistrantControls.Controls.Add( cpHomeCampus );

                        if ( setValue && fieldValue != null )
                        {
                            cpHomeCampus.SelectedCampusId = fieldValue.ToString().AsIntegerOrNull();
                        }
                        break;
                    }

                case RegistrationPersonFieldType.Address:
                    {
                        var acAddress = new AddressControl();
                        acAddress.ID = "acAddress";
                        acAddress.Label = "Address";
                        acAddress.UseStateAbbreviation = true;
                        acAddress.UseCountryAbbreviation = false;
                        acAddress.Required = field.IsRequired;
                        acAddress.ValidationGroup = BlockValidationGroup;

                        phRegistrantControls.Controls.Add( acAddress );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue.ToString().FromJsonOrNull<Location>();
                            acAddress.SetValues( value );
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Email:
                    {
                        var tbEmail = new EmailBox();
                        tbEmail.ID = "tbEmail";
                        tbEmail.Label = "Email";
                        tbEmail.Required = field.IsRequired;
                        tbEmail.ValidationGroup = BlockValidationGroup;
                        phRegistrantControls.Controls.Add( tbEmail );

                        if ( setValue && fieldValue != null )
                        {
                            tbEmail.Text = fieldValue.ToString();
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Birthdate:
                    {
                        var bpBirthday = new BirthdayPicker();
                        bpBirthday.ID = "bpBirthday";
                        bpBirthday.Label = "Birthday";
                        bpBirthday.Required = field.IsRequired;
                        bpBirthday.ValidationGroup = BlockValidationGroup;
                        phRegistrantControls.Controls.Add( bpBirthday );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue as DateTime?;
                            bpBirthday.SelectedDate = value;
                        }

                        break;
                    }

                case RegistrationPersonFieldType.Gender:
                    {
                        var ddlGender = new RockDropDownList();
                        ddlGender.ID = "ddlGender";
                        ddlGender.Label = "Gender";
                        ddlGender.Required = field.IsRequired;
                        ddlGender.ValidationGroup = BlockValidationGroup;
                        ddlGender.BindToEnum<Gender>( false );

                        // change the 'Unknow' value to be blank instead
                        ddlGender.Items.FindByValue( "0" ).Text = string.Empty;

                        phRegistrantControls.Controls.Add( ddlGender );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                            ddlGender.SetValue( value.ConvertToInt() );
                        }

                        break;
                    }

                case RegistrationPersonFieldType.MaritalStatus:
                    {
                        var ddlMaritalStatus = new RockDropDownList();
                        ddlMaritalStatus.ID = "ddlMaritalStatus";
                        ddlMaritalStatus.Label = "Marital Status";
                        ddlMaritalStatus.Required = field.IsRequired;
                        ddlMaritalStatus.ValidationGroup = BlockValidationGroup;
                        ddlMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ), true );
                        phRegistrantControls.Controls.Add( ddlMaritalStatus );

                        if ( setValue && fieldValue != null )
                        {
                            var value = fieldValue.ToString().AsInteger();
                            ddlMaritalStatus.SetValue( value );
                        }

                        break;
                    }

                case RegistrationPersonFieldType.MobilePhone:
                    {
                        var dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                        if ( dv != null )
                        {
                            var ppMobile = new PhoneNumberBox();
                            ppMobile.ID = "ppMobile";
                            ppMobile.Label = dv.Value;
                            ppMobile.Required = field.IsRequired;
                            ppMobile.ValidationGroup = BlockValidationGroup;
                            ppMobile.CountryCode = PhoneNumber.DefaultCountryCode();

                            phRegistrantControls.Controls.Add( ppMobile );

                            if ( setValue && fieldValue != null )
                            {
                                var value = fieldValue as PhoneNumber;
                                if ( value != null )
                                {
                                    ppMobile.CountryCode = value.CountryCode;
                                    ppMobile.Number = value.ToString();
                                }
                            }
                        }

                        break;
                    }
                case RegistrationPersonFieldType.HomePhone:
                    {
                        var dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                        if ( dv != null )
                        {
                            var ppHome = new PhoneNumberBox();
                            ppHome.ID = "ppHome";
                            ppHome.Label = dv.Value;
                            ppHome.Required = field.IsRequired;
                            ppHome.ValidationGroup = BlockValidationGroup;
                            ppHome.CountryCode = PhoneNumber.DefaultCountryCode();

                            phRegistrantControls.Controls.Add( ppHome );

                            if ( setValue && fieldValue != null )
                            {
                                var value = fieldValue as PhoneNumber;
                                if ( value != null )
                                {
                                    ppHome.CountryCode = value.CountryCode;
                                    ppHome.Number = value.ToString();
                                }
                            }
                        }

                        break;
                    }

                case RegistrationPersonFieldType.WorkPhone:
                    {
                        var dv = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
                        if ( dv != null )
                        {
                            var ppWork = new PhoneNumberBox();
                            ppWork.ID = "ppWork";
                            ppWork.Label = dv.Value;
                            ppWork.Required = field.IsRequired;
                            ppWork.ValidationGroup = BlockValidationGroup;
                            ppWork.CountryCode = PhoneNumber.DefaultCountryCode();

                            phRegistrantControls.Controls.Add( ppWork );

                            if ( setValue && fieldValue != null )
                            {
                                var value = fieldValue.ToString().FromJsonOrNull<PhoneNumber>();
                                if ( value != null )
                                {
                                    ppWork.CountryCode = value.CountryCode;
                                    ppWork.Number = value.ToString();
                                }
                            }
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Creates the attribute field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        private void CreateAttributeField( RegistrationTemplateFormField field, bool setValue, object fieldValue )
        {
            if ( field.AttributeId.HasValue )
            {
                var attribute = AttributeCache.Read( field.AttributeId.Value );

                string value = string.Empty;
                if ( setValue && fieldValue != null )
                {
                    value = fieldValue.ToString();
                }

                attribute.AddControl( phRegistrantControls.Controls, value, BlockValidationGroup, setValue, true, field.IsRequired, null, string.Empty );
            }
        }

        /// <summary>
        /// Creates the fee field.
        /// </summary>
        /// <param name="fee">The fee.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="feeValues">The fee values.</param>
        private void CreateFeeField( RegistrationTemplateFee fee, bool setValues, List<FeeInfo> feeValues )
        {
            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                string label = fee.Name;
                var cost = fee.CostValue.AsDecimalOrNull();
                if ( cost.HasValue && cost.Value != 0.0M )
                {
                    label = string.Format( "{0} ({1})", fee.Name, cost.Value.ToString("C2"));
                }

                if ( fee.AllowMultiple )
                {
                    // Single Option, Multi Quantity
                    var numUpDown = new NumberUpDown();
                    numUpDown.ID = "fee_" + fee.Id.ToString();
                    numUpDown.Label = label;
                    numUpDown.Minimum = 0;
                    phFees.Controls.Add( numUpDown );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        numUpDown.Value = feeValues.First().Quantity;
                    }
                }
                else
                {
                    // Single Option, Single Quantity
                    var cb = new RockCheckBox();
                    cb.ID = "fee_" + fee.Id.ToString();
                    cb.Label = label;
                    cb.SelectedIconCssClass = "fa fa-check-square-o fa-lg";
                    cb.UnSelectedIconCssClass = "fa fa-square-o fa-lg";
                    phFees.Controls.Add( cb );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        cb.Checked = feeValues.First().Quantity > 0;
                    }
                }
            }
            else
            {
                // Parse the options to get name and cost for each
                var options = new Dictionary<string, string>();
                string[] nameValues = fee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( nameAndValue.Length == 1)
                    {
                        options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                    }
                    if ( nameAndValue.Length == 2 )
                    {
                        options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1:C2})", nameAndValue[0], nameAndValue[1].AsDecimal() ) );
                    }
                }

                if ( fee.AllowMultiple )
                {
                    HtmlGenericControl feeAllowMultiple = new HtmlGenericControl( "div" );
                    phFees.Controls.Add( feeAllowMultiple );

                    feeAllowMultiple.AddCssClass( "feetype-allowmultiples" );

                    Label titleLabel = new Label();
                    feeAllowMultiple.Controls.Add( titleLabel );
                    titleLabel.CssClass = "control-label";
                    titleLabel.Text = fee.Name;

                    foreach( var optionKeyVal in options )
                    {
                        var numUpDown = new NumberUpDown();
                        numUpDown.ID = string.Format( "fee_{0}_{1}", fee.Id, optionKeyVal.Key );
                        numUpDown.Label = string.Format( "{0}", optionKeyVal.Value );
                        numUpDown.Minimum = 0;
                        numUpDown.CssClass = "fee-allowmultiple";
                        feeAllowMultiple.Controls.Add( numUpDown );

                        if ( setValues && feeValues != null && feeValues.Any() )
                        {
                            numUpDown.Value = feeValues
                                .Where( f => f.Option == optionKeyVal.Key )
                                .Select( f => f.Quantity )
                                .FirstOrDefault();
                        }
                    }
                }
                else
                {
                    // Multi Option, Single Quantity
                    var ddl = new RockDropDownList();
                    ddl.ID = "fee_" + fee.Id.ToString();
                    ddl.AddCssClass( "input-width-md" );
                    ddl.Label = fee.Name;
                    ddl.DataValueField = "Key";
                    ddl.DataTextField = "Value";
                    ddl.DataSource = options;
                    ddl.DataBind();
                    ddl.Items.Insert( 0, "");
                    phFees.Controls.Add( ddl );

                    if ( setValues && feeValues != null && feeValues.Any() )
                    {
                        ddl.SetValue( feeValues
                            .Where( f => f.Quantity > 0 )
                            .Select( f => f.Option )
                            .FirstOrDefault() );
                    }
                }
            }
        }

        /// <summary>
        /// Parses the registrant controls.
        /// </summary>
        private void ParseRegistrantControls()
        {
            if ( RegistrationState != null && RegistrationState.Registrants.Count > CurrentRegistrantIndex )
            {
                var registrant = RegistrationState.Registrants[CurrentRegistrantIndex];

                if ( rblFamilyOptions.Visible )
                {
                    registrant.FamilyGuid = rblFamilyOptions.SelectedValue.AsGuid();
                }

                if ( registrant.FamilyGuid.Equals( Guid.Empty ) )
                {
                    registrant.FamilyGuid = Guid.NewGuid();
                }

                var form = RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentFormIndex];
                foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
                {
                    object value = null;

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        value = ParsePersonField( field );
                    }
                    else
                    {
                        value = ParseAttributeField( field );
                    }

                    if ( value != null )
                    {
                        registrant.FieldValues.AddOrReplace( field.Id, value );
                    }
                    else
                    {
                        registrant.FieldValues.Remove( field.Id );
                    }
                }

                if ( FormCount - 1 == CurrentFormIndex )
                {
                    foreach ( var fee in RegistrationTemplate.Fees )
                    {
                        List<FeeInfo> feeValues = ParseFee( fee );
                        if ( fee != null )
                        {
                            registrant.FeeValues.AddOrReplace( fee.Id, feeValues );
                        }
                        else
                        {
                            registrant.FeeValues.Remove( fee.Id );
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Parses the person field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object ParsePersonField( RegistrationTemplateFormField field )
        {
            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.FirstName:
                    {
                        var tbFirstName = phRegistrantControls.FindControl( "tbFirstName" ) as RockTextBox;
                        string value = tbFirstName != null ? tbFirstName.Text : null;
                        return string.IsNullOrWhiteSpace( value ) ? null : value;
                    }

                case RegistrationPersonFieldType.LastName:
                    {
                        var tbLastName = phRegistrantControls.FindControl( "tbLastName" ) as RockTextBox;
                        string value = tbLastName != null ? tbLastName.Text : null;
                        return string.IsNullOrWhiteSpace( value ) ? null : value;
                    }

                case RegistrationPersonFieldType.Campus:
                    {
                        var cpHomeCampus = phRegistrantControls.FindControl( "cpHomeCampus" ) as CampusPicker;
                        return cpHomeCampus != null ? cpHomeCampus.SelectedCampusId : null;
                    }

                case RegistrationPersonFieldType.Address:
                    {
                        var location = new Location();
                        var acAddress = phRegistrantControls.FindControl( "acAddress" ) as AddressControl;
                        if ( acAddress != null )
                        {
                            acAddress.GetValues( location );
                            return string.IsNullOrWhiteSpace( location.ToString() ) ? null : location.ToJson();
                        }
                        break;
                    }
                   
                case RegistrationPersonFieldType.Email:
                    {
                        var tbEmail = phRegistrantControls.FindControl( "tbEmail" ) as EmailBox;
                        string value = tbEmail != null ? tbEmail.Text : null;
                        return string.IsNullOrWhiteSpace( value ) ? null : value;
                    }

                case RegistrationPersonFieldType.Birthdate:
                    {
                        var bpBirthday = phRegistrantControls.FindControl( "bpBirthday" ) as BirthdayPicker;
                        return bpBirthday != null ? bpBirthday.SelectedDate : null;
                    }

                case RegistrationPersonFieldType.Gender:
                    {
                        var ddlGender = phRegistrantControls.FindControl( "ddlGender" ) as RockDropDownList;
                        return ddlGender != null ? ddlGender.SelectedValueAsInt() : null;
                    }

                case RegistrationPersonFieldType.MaritalStatus:
                    {
                        var ddlMaritalStatus = phRegistrantControls.FindControl( "ddlMaritalStatus" ) as RockDropDownList;
                        return ddlMaritalStatus != null ? ddlMaritalStatus.SelectedValueAsInt() : null;
                    }

                case RegistrationPersonFieldType.MobilePhone:
                    {
                        var phoneNumber = new PhoneNumber();
                        var ppMobile = phRegistrantControls.FindControl( "ppMobile" ) as PhoneNumberBox;
                        if ( ppMobile != null )
                        {
                            phoneNumber.CountryCode = PhoneNumber.CleanNumber( ppMobile.CountryCode );
                            phoneNumber.Number = PhoneNumber.CleanNumber( ppMobile.Number );
                            return string.IsNullOrWhiteSpace( phoneNumber.Number ) ? null : phoneNumber.ToJson();
                        }
                        break;
                    }

                case RegistrationPersonFieldType.HomePhone:
                    {
                        var phoneNumber = new PhoneNumber();
                        var ppHome = phRegistrantControls.FindControl( "ppHome" ) as PhoneNumberBox;
                        if ( ppHome != null )
                        {
                            phoneNumber.CountryCode = PhoneNumber.CleanNumber( ppHome.CountryCode );
                            phoneNumber.Number = PhoneNumber.CleanNumber( ppHome.Number );
                            return string.IsNullOrWhiteSpace( phoneNumber.Number ) ? null : phoneNumber.ToJson();
                        }
                        break;
                    }

                case RegistrationPersonFieldType.WorkPhone:
                    {
                        var phoneNumber = new PhoneNumber();
                        var ppWork = phRegistrantControls.FindControl( "ppWork" ) as PhoneNumberBox;
                        if ( ppWork != null )
                        {
                            phoneNumber.CountryCode = PhoneNumber.CleanNumber( ppWork.CountryCode );
                            phoneNumber.Number = PhoneNumber.CleanNumber( ppWork.Number );
                            return string.IsNullOrWhiteSpace( phoneNumber.Number ) ? null : phoneNumber.ToJson();
                        }
                        break;
                    }
            }

            return null;

        }

        /// <summary>
        /// Parses the attribute field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object ParseAttributeField( RegistrationTemplateFormField field )
        {
            if ( field.AttributeId.HasValue )
            {
                var attribute = AttributeCache.Read( field.AttributeId.Value );
                string fieldId = "attribute_field_" + attribute.Id.ToString();

                Control control = phRegistrantControls.FindControl( fieldId );
                if ( control != null )
                {
                    return attribute.FieldType.Field.GetEditValue( control, attribute.QualifierValues );
                }
            }

            return null;
        }

        /// <summary>
        /// Parses the fee.
        /// </summary>
        /// <param name="fee">The fee.</param>
        /// <returns></returns>
        private List<FeeInfo> ParseFee( RegistrationTemplateFee fee )
        {
            string fieldId = string.Format( "fee_{0}", fee.Id );

            if ( fee.FeeType == RegistrationFeeType.Single )
            {
                if ( fee.AllowMultiple )
                {
                    // Single Option, Multi Quantity
                    var numUpDown = phFees.FindControl( fieldId ) as NumberUpDown;
                    if ( numUpDown != null && numUpDown.Value > 0 )
                    {
                        return new List<FeeInfo> { new FeeInfo( string.Empty, numUpDown.Value, fee.CostValue.AsDecimal() ) };
                    }
                }
                else
                {
                    // Single Option, Single Quantity
                    var cb = phFees.FindControl( fieldId ) as RockCheckBox;
                    if ( cb != null && cb.Checked )
                    {
                        return new List<FeeInfo> { new FeeInfo( string.Empty, 1, fee.CostValue.AsDecimal() ) };
                    }
                }
            }
            else
            {
                // Parse the options to get name and cost for each
                var options = new Dictionary<string, string>();
                var optionCosts = new Dictionary<string, decimal>();

                string[] nameValues = fee.CostValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                foreach ( string nameValue in nameValues )
                {
                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( nameAndValue.Length == 1 )
                    {
                        options.AddOrIgnore( nameAndValue[0], nameAndValue[0] );
                        optionCosts.AddOrIgnore( nameAndValue[0], 0.0m );
                    }
                    if ( nameAndValue.Length == 2 )
                    {
                        options.AddOrIgnore( nameAndValue[0], string.Format( "{0} ({1:C2})", nameAndValue[0], nameAndValue[1].AsDecimal() ) );
                        optionCosts.AddOrIgnore( nameAndValue[0], nameAndValue[1].AsDecimal() );
                    }
                }

                if ( fee.AllowMultiple )
                {
                    // Multi Option, Multi Quantity
                    var result = new List<FeeInfo>();

                    foreach ( var optionKeyVal in options )
                    {
                        string optionFieldId = string.Format( "{0}_{1}", fieldId, optionKeyVal.Key );
                        var numUpDown = phFees.FindControl( optionFieldId ) as NumberUpDown;
                        if ( numUpDown != null && numUpDown.Value > 0 )
                        {
                            result.Add( new FeeInfo( optionKeyVal.Key, numUpDown.Value, optionCosts[optionKeyVal.Key] ) );
                        }
                    }

                    if ( result.Any() )
                    {
                        return result;
                    }
                }
                else
                {
                    // Multi Option, Single Quantity
                    var ddl = phFees.FindControl( fieldId ) as RockDropDownList;
                    if ( ddl != null && ddl.SelectedValue != "" )
                    {
                        return new List<FeeInfo> { new FeeInfo( ddl.SelectedValue, 1, optionCosts[ddl.SelectedValue] ) };
                    }
                }
            }

            return null;
        }

        #endregion

        #region Summary/Payment Controls

        private void CreateSummaryControls( bool setValues )
        {
            lDiscountCodeLabel.Text = DiscountCodeTerm;

            if ( setValues && RegistrationState != null )
            {
                // Check to see if this is an existing registration or information has already been entered
                if ( RegistrationState.RegistrationId.HasValue ||
                    !string.IsNullOrWhiteSpace( RegistrationState.FirstName) ||
                    !string.IsNullOrWhiteSpace( RegistrationState.LastName ) ||
                    !string.IsNullOrWhiteSpace( RegistrationState.ConfirmationEmail ) )
                {
                    // If so, use it
                    tbYourFirstName.Text = RegistrationState.FirstName;
                    tbYourLastName.Text = RegistrationState.LastName;
                    tbConfirmationEmail.Text = RegistrationState.ConfirmationEmail;
                }
                else
                {
                    // If not, find the field information from first registrant
                    if ( RegistrationState.Registrants.Any() )
                    {
                        var firstRegistrant = RegistrationState.Registrants.First();
                        tbYourFirstName.Text = firstRegistrant.GetFirstName( RegistrationTemplate );
                        tbYourLastName.Text = firstRegistrant.GetLastName( RegistrationTemplate );
                        tbConfirmationEmail.Text = firstRegistrant.GetEmail( RegistrationTemplate );
                    }
                    else
                    {
                        tbYourFirstName.Text = string.Empty;
                        tbYourLastName.Text = string.Empty;
                        tbConfirmationEmail.Text = string.Empty;
                    }
                }

                // Build Discount info
                nbDiscountCode.Visible = false;
                if ( RegistrationTemplate != null && RegistrationTemplate.Discounts.Any() )
                {
                    // Only allow discount code to be entered for a new registration
                    divDiscountCode.Visible = !RegistrationState.RegistrationId.HasValue;

                    string discountCode = RegistrationState.DiscountCode;
                    tbDiscountCode.Text = discountCode;
                    if ( !string.IsNullOrWhiteSpace( discountCode ))
                    {
                        var discount = RegistrationTemplate.Discounts
                            .Where( d => d.Code.Equals( discountCode, StringComparison.OrdinalIgnoreCase ) )
                            .FirstOrDefault();
                        if ( discount == null )
                        {
                            nbDiscountCode.Text = string.Format( "'{1}' is not a valid {1}.", discountCode, DiscountCodeTerm );
                            nbDiscountCode.Visible = true;
                        }

                    }
                }
                else
                {
                    divDiscountCode.Visible = false;
                }

                // Get the cost/fee summary
                var costs = new List<RegistrationCostSummaryInfo>();
                foreach( var registrant in RegistrationState.Registrants )
                {
                    if ( registrant.Cost > 0 )
                    {
                        var costSummary = new RegistrationCostSummaryInfo();
                        costSummary.Type = RegistrationCostSummaryType.Cost;
                        costSummary.Description = string.Format( "{0} {1}",
                            registrant.GetFirstName( RegistrationTemplate ),
                            registrant.GetLastName( RegistrationTemplate ) );
                        costSummary.Cost = registrant.Cost;
                        if ( !RegistrationState.RegistrationId.HasValue && RegistrationState.DiscountPercentage > 0.0m )
                        {
                            costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * RegistrationState.DiscountPercentage );
                        }
                        else
                        {
                            costSummary.DiscountedCost = costSummary.Cost;
                        }

                        // If registration allows a minimum payment calculate that amount, otherwise use the discounted amount as minimum
                        costSummary.MinPayment = RegistrationTemplate.MinimumInitialPayment != 0 ? 
                            RegistrationTemplate.MinimumInitialPayment : costSummary.DiscountedCost;

                        costs.Add( costSummary );
                    }

                    foreach( var fee in registrant.FeeValues )
                    {
                        var templateFee = RegistrationTemplate.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
                        if ( fee.Value != null )
                        {
                            foreach ( var feeInfo in fee.Value )
                            {
                                decimal cost = feeInfo.PreviousCost > 0.0m ? feeInfo.PreviousCost : feeInfo.Cost;
                                string desc = string.Format( "{0}{1} ({2:N0} @ {3:C2})",
                                    templateFee != null ? templateFee.Name : "(Previous Cost)",
                                    string.IsNullOrWhiteSpace( feeInfo.Option ) ? "" : "-" + feeInfo.Option,
                                    feeInfo.Quantity,
                                    cost );

                                var costSummary = new RegistrationCostSummaryInfo();
                                costSummary.Type = RegistrationCostSummaryType.Fee;
                                costSummary.Description = desc;
                                costSummary.Cost = feeInfo.Quantity * cost;

                                if ( !RegistrationState.RegistrationId.HasValue && RegistrationState.DiscountPercentage > 0.0m && templateFee != null && templateFee.DiscountApplies )
                                {
                                    costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * RegistrationState.DiscountPercentage );
                                }
                                else
                                {
                                    costSummary.DiscountedCost = costSummary.Cost;
                                }

                                // Optional Fees are always included in minimum payment
                                costSummary.MinPayment = costSummary.DiscountedCost;

                                costs.Add( costSummary );
                            }
                        }
                    }
                }

                // If there were any costs
                if ( costs.Any() )
                {
                    pnlMoney.Visible = true;

                    // Get the total min payment for all costs and fees
                    decimal minPayment = costs.Sum( c => c.MinPayment );

                    // Add row for amount discount
                    if ( RegistrationState.DiscountAmount > 0.0m )
                    {
                        decimal totalDiscount = 0.0m - ( RegistrationState.Registrants.Count * RegistrationState.DiscountAmount );
                        costs.Add( new RegistrationCostSummaryInfo
                        {
                            Type = RegistrationCostSummaryType.Discount,
                            Description = "Discount",
                            Cost = totalDiscount,
                            DiscountedCost = totalDiscount
                        } );
                    }

                    // Get the totals
                    RegistrationState.TotalCost = costs.Sum( c => c.Cost );
                    RegistrationState.DiscountedCost = costs.Sum( c => c.DiscountedCost );

                    // If minimum payment is greater than total discounted cost ( which is possible with discounts ), adjust the minimum payment
                    minPayment = minPayment > RegistrationState.DiscountedCost ? RegistrationState.DiscountedCost : minPayment;

                    // Add row for totals
                    costs.Add( new RegistrationCostSummaryInfo
                    {
                        Type = RegistrationCostSummaryType.Total,
                        Description = "Total",
                        Cost = costs.Sum( c => c.Cost ),
                        DiscountedCost = RegistrationState.DiscountedCost,
                    } );

                    rptFeeSummary.DataSource = costs;
                    rptFeeSummary.DataBind();

                    // Set the total cost
                    hfTotalCost.Value = RegistrationState.DiscountedCost.ToString( "N2" );
                    lTotalCost.Text = RegistrationState.DiscountedCost.ToString( "C2" );

                    // Check for previous payments
                    lPreviouslyPaid.Visible = RegistrationState.PreviousPaymentTotal != 0.0m;
                    hfPreviouslyPaid.Value = RegistrationState.PreviousPaymentTotal.ToString( "N2" );
                    lPreviouslyPaid.Text = RegistrationState.PreviousPaymentTotal.ToString( "C2" );
                    minPayment = minPayment - RegistrationState.PreviousPaymentTotal;

                    // if min payment is less than 0, set it to 0
                    minPayment = minPayment < 0 ? 0 : minPayment;

                    // Calculate balance due, and if a partial payment is still allowed
                    decimal balanceDue = RegistrationState.DiscountedCost - RegistrationState.PreviousPaymentTotal;
                    bool allowPartialPayment = balanceDue > 0 && minPayment < balanceDue;

                    // If partial payment is allowed, show the minimum payment due
                    lMinimumDue.Visible = allowPartialPayment;
                    hfMinimumDue.Value = minPayment.ToString( "N2" );
                    lMinimumDue.Text = minPayment.ToString( "C2" );

                    // Make sure payment amount is within minumum due and balance due. If not, set to balance due
                    if ( RegistrationState.PaymentAmount < minPayment ||
                        RegistrationState.PaymentAmount > balanceDue )
                    {
                        RegistrationState.PaymentAmount = balanceDue;
                    }

                    nbAmountPaid.Visible = allowPartialPayment;
                    nbAmountPaid.Text = RegistrationState.PaymentAmount.ToString( "N2" );

                    // If a previous payment was made, or partial payment is allowed, show the amount remaining after selected payment amount
                    lRemainingDue.Visible = allowPartialPayment || RegistrationState.PreviousPaymentTotal != 0.0m;
                    lRemainingDue.Text = ( RegistrationState.DiscountedCost - ( RegistrationState.PreviousPaymentTotal + RegistrationState.PaymentAmount ) ).ToString( "C2" );

                    divPaymentInfo.Visible = balanceDue > 0;

                    // Set payment options based on gateway settings
                    if ( balanceDue > 0 && RegistrationTemplate.FinancialGateway != null )
                    {
                        divPaymentInfo.Visible = true;

                        if ( RegistrationTemplate.FinancialGateway.Attributes == null )
                        {
                            RegistrationTemplate.LoadAttributes();
                        }

                        var component = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                        if ( component != null )
                        {
                            txtCardFirstName.Visible = component.SplitNameOnCard;
                            txtCardLastName.Visible = component.SplitNameOnCard;
                            txtCardName.Visible = !component.SplitNameOnCard;
                            mypExpiration.MinimumYear = RockDateTime.Now.Year;
                        }
                    }
                    else
                    {
                        divPaymentInfo.Visible = false;
                    }
                }
                else
                {
                    RegistrationState.TotalCost = 0.0m;
                    RegistrationState.DiscountedCost = 0.0m;
                    pnlMoney.Visible = false;
                }
            }
        }

        private void ParseSummaryControls()
        {
            if ( RegistrationState != null )
            {
                RegistrationState.FirstName = tbYourFirstName.Text;
                RegistrationState.LastName = tbYourLastName.Text;
                RegistrationState.ConfirmationEmail = tbConfirmationEmail.Text;

                if ( RegistrationState.DiscountCode != tbDiscountCode.Text.Trim() )
                {
                    RegistrationState.DiscountCode = tbDiscountCode.Text.Trim();
                    if ( !string.IsNullOrWhiteSpace( RegistrationState.DiscountCode ) )
                    {
                        var discount = RegistrationTemplate.Discounts
                            .Where( d => d.Code.Equals( RegistrationState.DiscountCode, StringComparison.OrdinalIgnoreCase ) )
                            .FirstOrDefault();
                        RegistrationState.DiscountPercentage = discount != null ? discount.DiscountPercentage : 0.0m;
                        RegistrationState.DiscountAmount = discount != null ? discount.DiscountAmount : 0.0m;
                    }
                    else
                    {
                        RegistrationState.DiscountPercentage = 0.0m;
                        RegistrationState.DiscountAmount = 0.0m;
                    }
                }

                RegistrationState.PaymentAmount = nbAmountPaid.Text.AsDecimal();
            }
        }

        #endregion

        #endregion

        #endregion

    }
}