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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Humanizer;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Block used to register for a registration instance.
    /// </summary>
    [DisplayName( "Registration Entry" )]
    [Category( "Event" )]
    [Description( "Block used to register for a registration instance." )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT, "", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING, "", 1 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE, "Source", "The Financial Source Type to use when creating transactions", false, false, Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE, "", 2 )]
    [TextField( "Batch Name Prefix", "The batch prefix name to use when creating a new batch", false, "Event Registration", "", 3 )]
    [BooleanField( "Display Progress Bar", "Display a progress bar for the registration.", true, "", 4 )]
    [BooleanField( "Allow InLine Digital Signature Documents", "Should inline digital documents be allowed? This requires that the registration template is configured to display the document inline", true, "", 6, "SignInline" )]
    [SystemEmailField( "Confirm Account Template", "Confirm Account Email Template", false, Rock.SystemGuid.SystemEmail.SECURITY_CONFIRM_ACCOUNT, "", 7 )]
    [TextField( "Family Term", "The term to use for specifying which household or family a person is a member of.", true, "immediate family", "", 8 )]
    [BooleanField( "Force Email Update", "Force the email to be updated on the person's record.", false, "", 9 )]
    [BooleanField( "Show Field Descriptions", "Show the field description as help text", defaultValue: true, order: 10, key: "ShowFieldDescriptions" )]
    public partial class RegistrationEntry : RockBlock
    {
        #region Fields

        private bool _saveNavigationHistory = false;

        // Page (query string) parameter names
        private const string REGISTRATION_ID_PARAM_NAME = "RegistrationId";

        private const string SLUG_PARAM_NAME = "Slug";
        private const string START_AT_BEGINNING = "StartAtBeginning";
        private const string REGISTRATION_INSTANCE_ID_PARAM_NAME = "RegistrationInstanceId";
        private const string EVENT_OCCURRENCE_ID_PARAM_NAME = "EventOccurrenceId";
        private const string GROUP_ID_PARAM_NAME = "GroupId";
        private const string CAMPUS_ID_PARAM_NAME = "CampusId";

        // Viewstate keys
        private const string REGISTRATION_INSTANCE_STATE_KEY = "RegistrationInstanceState";

        private const string REGISTRATION_STATE_KEY = "RegistrationState";
        private const string GROUP_ID_KEY = "GroupId";
        private const string CAMPUS_ID_KEY = "CampusId";
        private const string SIGN_INLINE_KEY = "SignInline";
        private const string DIGITAL_SIGNATURE_COMPONENT_TYPE_NAME_KEY = "DigitalSignatureComponentTypeName";
        private const string CURRENT_PANEL_KEY = "CurrentPanel";
        private const string CURRENT_REGISTRANT_INDEX_KEY = "CurrentRegistrantIndex";
        private const string CURRENT_FORM_INDEX_KEY = "CurrentFormIndex";
        private const string MINIMUM_PAYMENT_KEY = "MinimumPayment";
        private const string DEFAULT_PAYMENT_KEY = "DefaultPayment";

        private enum PanelIndex
        {
            PanelHowMany = 0,
            PanelRegistrationAttributesStart = 1,
            PanelRegistrant = 2,
            PanelRegistrationAttributesEnd = 3,
            PanelSummary = 4,
            PanelPayment = 5,
            PanelSuccess = 6
        }

        // protected variables
        private decimal _percentComplete = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the percent complete.
        /// </summary>
        /// <value>
        /// The percent complete.
        /// </value>
        public decimal PercentComplete
        {
            get
            {
                return _percentComplete;
            }

            set
            {
                _percentComplete = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected registration instance
        /// </summary>
        /// <value>
        /// The state of the registration instance.
        /// </value>
        private RegistrationInstance RegistrationInstanceState { get; set; }

        /// <summary>
        /// Gets or sets the state of the registration attributes.
        /// </summary>
        /// <value>
        /// The state of the registration attributes.
        /// </value>
        private List<AttributeCache> RegistrationAttributesState { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        private int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        private int? CampusId { get; set; }

        /// <summary>
        /// Digital signature fields
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sign inline]; otherwise, <c>false</c>.
        /// </value>
        private bool SignInline { get; set; }

        /// <summary>
        /// Gets or sets the name of the digital signature component type.
        /// </summary>
        /// <value>
        /// The name of the digital signature component type.
        /// </value>
        private string DigitalSignatureComponentTypeName { get; set; }

        /// <summary>
        /// Gets or sets the digital signature component.
        /// </summary>
        /// <value>
        /// The digital signature component.
        /// </value>
        private DigitalSignatureComponent DigitalSignatureComponent { get; set; }

        /// <summary>
        /// Gets or sets the state of the registration.
        /// </summary>
        /// <value>
        /// The state of the registration.
        /// </value>
        protected RegistrationInfo RegistrationState { get; set; }

        /// <summary>
        /// Gets or sets the current panel.
        /// </summary>
        /// <value>
        /// The current panel.
        /// </value>
        private PanelIndex CurrentPanel { get; set; }

        /// <summary>
        /// Gets or sets the index of the current registrant.
        /// </summary>
        /// <value>
        /// The index of the current registrant.
        /// </value>
        private int CurrentRegistrantIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the current form.
        /// </summary>
        /// <value>
        /// The index of the current form.
        /// </value>
        private int CurrentFormIndex { get; set; }

        /// <summary>
        /// Gets or sets the step2 i frame URL.
        /// </summary>
        /// <value>
        /// The step2 i frame URL.
        /// </value>
        protected string Step2IFrameUrl
        {
            get { return ViewState["Step2IFrameUrl"] as string; }
            set { ViewState["Step2IFrameUrl"] = value; }
        }

        /// <summary>
        /// Gets or sets the minimum payment total after factoring in discounts, fees, and minimum payment amount per registrant
        /// </summary>
        /// <value>
        /// The minimum payment.
        /// </value>
        private decimal? minimumPayment { get; set; }

        /// <summary>
        /// Gets or sets the default payment (combined for all registrants for this registration) 
        /// </summary>
        /// <value>
        /// The default payment.
        /// </value>
        private decimal? defaultPayment { get; set; }

        /// <summary>
        /// Gets the registration template.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
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
        /// Gets the registration attribute title start.
        /// </summary>
        /// <value>
        /// The registration attribute title start.
        /// </value>
        private string RegistrationAttributeTitleStart
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.RegistrationAttributeTitleStart ) )
                {
                    return RegistrationTemplate.RegistrationAttributeTitleStart;
                }

                return "Registration Information";
            }
        }

        /// <summary>
        /// Gets the registration attribute title end.
        /// </summary>
        /// <value>
        /// The registration attribute title end.
        /// </value>
        private string RegistrationAttributeTitleEnd
        {
            get
            {
                if ( RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationTemplate.RegistrationAttributeTitleEnd ) )
                {
                    return RegistrationTemplate.RegistrationAttributeTitleEnd;
                }

                return "Registration Information";
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
                    if ( !RegistrationTemplate.MaxRegistrants.HasValue )
                    {
                        return int.MaxValue;
                    }

                    return RegistrationTemplate.MaxRegistrants.Value;
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
        /// Gets or sets a value indicating whether [using three-step gateway].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [using three-step gateway]; otherwise, <c>false</c>.
        /// </value>
        protected bool Using3StepGateway
        {
            get { return ViewState["Using3StepGateway"] as bool? ?? false; }
            set { ViewState["Using3StepGateway"] = value; }
        }

        /// <summary>
        /// Gets or sets the ids of registration attributes that will be prompted *before* editing registrants.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has registration attributes before registrants; otherwise, <c>false</c>.
        /// </value>
        protected List<int> RegistrationAttributeIdsBeforeRegistrants
        {
            get
            {
                return ViewState["RegistrationAttributeIdsBeforeRegistrants"] as List<int> ?? new List<int>();
            }

            set
            {
                ViewState["RegistrationAttributeIdsBeforeRegistrants"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the ids of registration attributes that will be prompted *after* editing registrants.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has registration attributes after registrants; otherwise, <c>false</c>.
        /// </value>
        protected List<int> RegistrationAttributeIdsAfterRegistrants
        {
            get
            {
                return ViewState["RegistrationAttributeIdsAfterRegistrants"] as List<int> ?? new List<int>();
            }

            set
            {
                ViewState["RegistrationAttributeIdsAfterRegistrants"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the progress bar steps.
        /// </summary>
        /// <value>
        /// The progress bar steps.
        /// </value>
        protected decimal ProgressBarSteps
        {
            get { return ViewState["ProgressBarSteps"] as decimal? ?? 3.0m; }
            set { ViewState["ProgressBarSteps"] = value; }
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

            SignInline = ViewState[SIGN_INLINE_KEY] as bool? ?? false;
            DigitalSignatureComponentTypeName = ViewState[DIGITAL_SIGNATURE_COMPONENT_TYPE_NAME_KEY] as string;
            if ( !string.IsNullOrWhiteSpace( DigitalSignatureComponentTypeName ) )
            {
                DigitalSignatureComponent = DigitalSignatureContainer.GetComponent( DigitalSignatureComponentTypeName );
            }

            GroupId = ViewState[GROUP_ID_KEY] as int?;
            CampusId = ViewState[CAMPUS_ID_KEY] as int?;
            CurrentPanel = ViewState[CURRENT_PANEL_KEY] as PanelIndex? ?? PanelIndex.PanelHowMany;
            CurrentRegistrantIndex = ViewState[CURRENT_REGISTRANT_INDEX_KEY] as int? ?? 0;
            CurrentFormIndex = ViewState[CURRENT_FORM_INDEX_KEY] as int? ?? 0;
            minimumPayment = ViewState[MINIMUM_PAYMENT_KEY] as decimal?;
            defaultPayment = ViewState[DEFAULT_PAYMENT_KEY] as decimal?;

            CreateDynamicControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // make sure that a URL with navigation history parameters is really from a browser navigation and not a Link or Refresh
            hfAllowNavigate.Value = false.ToTrueFalse();

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
            nbWaitingList.Visible = false;
            nbDiscountCode.Visible = false;

            hfStep2AutoSubmit.Value = "false";

            // register navigation event to enable support for the back button
            var sm = ScriptManager.GetCurrent( Page );
            sm.Navigate += sm_Navigate;

            // Show save account info based on if checkbox is checked
            divSaveAccount.Style[HtmlTextWriterStyle.Display] = cbSaveAccount.Checked ? "block" : "none";

            // Change the display when family members are allowed
            rblFamilyOptions.Label = RegistrantTerm + " is in the same " + GetAttributeValue( "FamilyTerm" ) + " as";
            rblRegistrarFamilyOptions.Label = "You are in the same " + GetAttributeValue( "FamilyTerm" ) + " as";
            pnlFamilyMembers.Style[HtmlTextWriterStyle.Display] = pnlFamilyMembers.Visible && RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes ? "block" : "none";

            if ( !Page.IsPostBack )
            {
                if ( CurrentPerson != null && CurrentPerson.IsBusiness() )
                {
                    ShowError( "Invalid Login", "Sorry, the login you are using doesn't appear to be tied to a valid person record. Try logging out and logging in with a different username, or create a new account before registering for the selected event." );
                }
                else
                {
                    // Get the a registration if it has not already been loaded
                    if ( RegistrationState != null || SetRegistrationState() )
                    {
                        if ( RegistrationTemplate != null )
                        {
                            if ( !RegistrationTemplate.WaitListEnabled && RegistrationState.SlotsAvailable.HasValue && RegistrationState.SlotsAvailable.Value <= 0 )
                            {
                                ShowWarning(
                                    string.Format( "{0} Full", RegistrationTerm ),
                                    string.Format( "<p>There are not any more {0} available for {1}.</p>", RegistrationTerm.ToLower().Pluralize(), RegistrationInstanceState.Name ) );
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
                                    if ( SignInline &&
                                        !PageParameter( "redirected" ).AsBoolean() &&
                                        DigitalSignatureComponent != null &&
                                        !string.IsNullOrWhiteSpace( DigitalSignatureComponent.CookieInitializationUrl ) )
                                    {
                                        // Redirect for Digital Signature Cookie Initialization
                                        var returnUrl = ResolvePublicUrl( Request.Url.PathAndQuery );
                                        returnUrl = returnUrl + ( returnUrl.Contains( "?" ) ? "&" : "?" ) + "redirected=True";
                                        string redirectUrl = string.Format( "{0}?redirect_uri={1}", DigitalSignatureComponent.CookieInitializationUrl, HttpUtility.UrlEncode( returnUrl ) );
                                        Response.Redirect( redirectUrl, false );
                                    }
                                    else
                                    {
                                        // show the panel for asking how many registrants ( it may be skipped )
                                        ShowHowMany();
                                    }
                                }
                            }
                        }
                        else
                        {
                            ShowWarning( "Sorry", string.Format( "The selected {0} could not be found or is no longer active.", RegistrationTerm.ToLower() ) );
                        }
                    }
                }
            }
            else
            {
                // Load values from controls into the state objects
                ParseDynamicControls();

                // Show or Hide the Credit card entry panel based on if a saved account exists and it's selected or not.
                divNewCard.Style[HtmlTextWriterStyle.Display] = ( rblSavedCC.Items.Count == 0 || rblSavedCC.Items[rblSavedCC.Items.Count - 1].Selected ) ? "block" : "none";
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? registrationInstanceId = PageParameter( REGISTRATION_INSTANCE_ID_PARAM_NAME ).AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                var registrationInstanceName = new RegistrationInstanceService( new RockContext() ).GetSelect( registrationInstanceId.Value, a => a.Name );

                RockPage.Title = registrationInstanceName;
                breadCrumbs.Add( new BreadCrumb( registrationInstanceName, pageReference ) );
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
            ViewState[SIGN_INLINE_KEY] = SignInline;
            ViewState[DIGITAL_SIGNATURE_COMPONENT_TYPE_NAME_KEY] = DigitalSignatureComponentTypeName;
            ViewState[GROUP_ID_KEY] = GroupId;
            ViewState[CAMPUS_ID_KEY] = CampusId;
            ViewState[CURRENT_PANEL_KEY] = CurrentPanel;
            ViewState[CURRENT_REGISTRANT_INDEX_KEY] = CurrentRegistrantIndex;
            ViewState[CURRENT_FORM_INDEX_KEY] = CurrentFormIndex;
            ViewState[MINIMUM_PAYMENT_KEY] = minimumPayment;
            ViewState[DEFAULT_PAYMENT_KEY] = defaultPayment;

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( _saveNavigationHistory )
            {
                // make sure that a URL with navigation history parameters is really from a browser navigation and not a Link or Refresh
                hfAllowNavigate.Value = ( CurrentPanel == PanelIndex.PanelSummary ? false : true ).ToTrueFalse();
                try
                {
                    if ( CurrentPanel != PanelIndex.PanelRegistrant )
                    {
                        this.AddHistory( "event", string.Format( "{0},0,0", CurrentPanel ) );
                    }
                    else
                    {
                        this.AddHistory( "event", string.Format( "1,{0},{1}", CurrentRegistrantIndex, CurrentFormIndex ) );
                    }
                }
                catch ( System.InvalidOperationException )
                {
                    // Swallow this exception
                }
            }

            base.OnPreRender( e );
        }

        #endregion

        #region Events

        #region Navigation Events

        /// <summary>
        /// Handles the Navigate event of the sm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        protected void sm_Navigate( object sender, HistoryEventArgs e )
        {
            var state = e.State["event"];

            if ( CurrentPanel > 0 && state != null && hfAllowNavigate.Value.AsBoolean() )
            {
                string[] commands = state.Split( ',' );

                PanelIndex panelIndex = PanelIndex.PanelHowMany;
                int registrantId = 0;
                int formId = 0;

                if ( commands.Count() == 3 )
                {
                    panelIndex = commands[0].ConvertToEnumOrNull<PanelIndex>() ?? PanelIndex.PanelHowMany;
                    registrantId = int.Parse( commands[1] );
                    formId = int.Parse( commands[2] );
                }

                switch ( panelIndex )
                {
                    case PanelIndex.PanelRegistrationAttributesStart:
                        {
                            ShowRegistrationAttributesStart( true );
                            break;
                        }

                    case PanelIndex.PanelRegistrant:
                        {
                            CurrentRegistrantIndex = registrantId;
                            CurrentFormIndex = formId;
                            ShowRegistrant();
                            break;
                        }

                    case PanelIndex.PanelRegistrationAttributesEnd:
                        {
                            ShowRegistrationAttributesEnd( true );
                            break;
                        }

                    case PanelIndex.PanelSummary:
                        {
                            ShowSummary();
                            break;
                        }

                    case PanelIndex.PanelPayment:
                        {
                            ShowPayment();
                            break;
                        }

                    default:
                        {
                            ShowHowMany();
                            break;
                        }
                }
            }
            else if ( CurrentPanel == PanelIndex.PanelSummary && !hfAllowNavigate.Value.AsBoolean() )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                if ( RegistrationInstanceState != null )
                {
                    qryParams.Add( REGISTRATION_INSTANCE_ID_PARAM_NAME, RegistrationInstanceState.Id.ToString() );
                }
                this.NavigateToCurrentPageReference( qryParams );
            }
            else
            {
                ShowHowMany();
            }
        }

        /// <summary>
        /// Handles the NumberUpdated event of the numHowMany control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void numHowMany_NumberUpdated( object sender, EventArgs e )
        {
            ShowWaitingListNotice();
        }

        /// <summary>
        /// Handles the Click event of the lbHowManyNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHowManyNext_Click( object sender, EventArgs e )
        {
            ShowRegistrationAttributesStart( true );
        }

        /// <summary>
        /// Shows the registration attributes before (or navigate to next/prev page if there aren't any)
        /// </summary>
        private void ShowRegistrationAttributesStart( bool forward )
        {
            if ( this.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                decimal currentStep = 1;
                PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
                pnlRegistrationAttributesStartProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

                lRegistrationAttributesStartTitle.Text = this.RegistrationAttributeTitleStart;

                avcRegistrationAttributesStart.ShowCategoryLabel = false;
                avcRegistrationAttributesStart.IncludedAttributes = this.RegistrationAttributeIdsBeforeRegistrants.Select( a => AttributeCache.Get( a ) ).ToArray();
                Registration registration = null;
                if ( ( this.RegistrationState != null ) && this.RegistrationState.RegistrationId.HasValue )
                {
                    registration = new RegistrationService( new RockContext() ).Get( this.RegistrationState.RegistrationId.Value );
                }

                if ( registration == null )
                {
                    registration = new Registration
                    {
                        RegistrationInstance = this.RegistrationInstanceState
                    };
                }

                if ( !avcRegistrationAttributesStart.HasEditControls( registration ) )
                {
                    avcRegistrationAttributesStart.AddEditControls( registration );
                }

                SetPanel( PanelIndex.PanelRegistrationAttributesStart );
            }
            else
            {
                if ( forward )
                {
                    btnRegistrationAttributesStartNext_Click( null, null );
                }
                else
                {
                    btnRegistrationAttributesStartPrev_Click( null, null );
                }
            }
        }

        /// <summary>
        /// Shows the registration attributes after (or navigate to next page if there aren't any)
        /// </summary>
        private void ShowRegistrationAttributesEnd( bool forward )
        {
            if ( this.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                decimal currentStep = ( FormCount * RegistrationState.RegistrantCount ) + 1;
                if ( this.RegistrationAttributeIdsBeforeRegistrants.Any() )
                {
                    currentStep++;
                }

                PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
                pnlRegistrationAttributesStartProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

                lRegistrationAttributesEndTitle.Text = this.RegistrationAttributeTitleEnd;
                avcRegistrationAttributesEnd.ShowCategoryLabel = false;
                avcRegistrationAttributesEnd.IncludedAttributes = this.RegistrationAttributeIdsAfterRegistrants.Select( a => AttributeCache.Get( a ) ).ToArray();
                Registration registration = null;
                if ( ( this.RegistrationState != null ) && this.RegistrationState.RegistrationId.HasValue )
                {
                    registration = new RegistrationService( new RockContext() ).Get( this.RegistrationState.RegistrationId.Value );
                }

                if ( registration == null )
                {
                    registration = new Registration
                    {
                        RegistrationInstance = this.RegistrationInstanceState
                    };
                }

                var setValues = forward;
                if ( !avcRegistrationAttributesEnd.HasEditControls( registration ) )
                {
                    avcRegistrationAttributesEnd.AddEditControls( registration );
                }

                SetPanel( PanelIndex.PanelRegistrationAttributesEnd );
            }
            else
            {
                if ( forward )
                {
                    btnRegistrationAttributesEndNext_Click( null, null );
                }
                else
                {
                    btnRegistrationAttributesEndPrev_Click( null, null );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRegistrationAttributesStartPrev_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRegistrationAttributesStartPrev_Click( object sender, EventArgs e )
        {
            ShowHowMany();
            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the btnRegistrationAttributesStartNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRegistrationAttributesStartNext_Click( object sender, EventArgs e )
        {
            _saveNavigationHistory = true;

            CurrentRegistrantIndex = 0;
            CurrentFormIndex = 0;

            // Create registrants based on the number selected
            SetRegistrantState( numHowMany.Value );

            SetProgressBarStepsCount();

            ShowRegistrant( true, false );

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Sets the progress bar steps count.
        /// </summary>
        private void SetProgressBarStepsCount()
        {
            // set the max number of steps in the progress bar
            int registrantPages = FormCount;
            if ( SignInline )
            {
                registrantPages += 2;
            }

            int registrantCount = 0;
            if ( RegistrationState != null )
            {
                registrantCount = RegistrationState.RegistrantCount;
            }

            this.ProgressBarSteps = ( registrantCount * registrantPages ) + 2;

            if ( this.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                this.ProgressBarSteps++;
            }

            if ( this.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                this.ProgressBarSteps++;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbRegistrantPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegistrantPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == PanelIndex.PanelRegistrant )
            {
                _saveNavigationHistory = true;

                hfRequiredDocumentLinkUrl.Value = string.Empty;

                ShowRegistrant( false, true );
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
            if ( CurrentPanel == PanelIndex.PanelRegistrant )
            {
                _saveNavigationHistory = true;

                ShowRegistrant( true, true );
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbRequiredDocumentNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRequiredDocumentNext_Click( object sender, EventArgs e )
        {
            hfRequiredDocumentLinkUrl.Value = string.Empty;

            string qryString = hfRequiredDocumentQueryString.Value;
            if ( qryString.StartsWith( "?document_id=" ) )
            {
                if ( RegistrationState != null && RegistrationState.RegistrantCount > CurrentRegistrantIndex )
                {
                    var registrant = RegistrationState.Registrants[CurrentRegistrantIndex];
                    registrant.SignatureDocumentKey = qryString.Substring( 13 );
                    registrant.SignatureDocumentLastSent = RockDateTime.Now;
                }

                lbRegistrantNext_Click( sender, e );
            }
            else
            {
                string errorMessage = string.Format(
                        "This {0} requires that you sign a {1} for each registrant, but it appears that you may have cancelled or skipped signing this document.",
                        RegistrationTemplate.RegistrationTerm,
                        RegistrationTemplate.RequiredSignatureDocumentTemplate.Name );

                ShowError( "Invalid or Missing Document Signature", errorMessage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRegistrationAttributesEndPrev_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRegistrationAttributesEndPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == PanelIndex.PanelRegistrationAttributesEnd )
            {
                _saveNavigationHistory = true;

                CurrentRegistrantIndex = RegistrationState != null ? RegistrationState.RegistrantCount - 1 : 0;
                CurrentFormIndex = FormCount - 1;

                nbAmountPaid.Text = string.Empty;
                RegistrationState.PaymentAmount = null;

                ShowRegistrant( false, false );

                hfTriggerScroll.Value = "true";
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the btnRegistrationAttributesEndNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRegistrationAttributesEndNext_Click( object sender, EventArgs e )
        {
            ShowSummary();
        }

        /// <summary>
        /// Handles the Click event of the lbSummaryPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSummaryPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == PanelIndex.PanelSummary )
            {
                _saveNavigationHistory = true;

                ShowRegistrationAttributesEnd( false );
            }
            else
            {
                ShowHowMany();
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
            if ( CurrentPanel == PanelIndex.PanelSummary )
            {
                List<string> summaryErrors = ValidateSummary();
                if ( !summaryErrors.Any() )
                {
                    _saveNavigationHistory = true;

                    if ( Using3StepGateway && RegistrationState.PaymentAmount > 0.0M )
                    {
                        string errorMessage = string.Empty;
                        if ( ProcessStep1( out errorMessage ) )
                        {
                            if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsId() ?? 0 ) > 0 )
                            {
                                hfStep2AutoSubmit.Value = "true";
                                ShowSummary(); // Stay on summary page so blank page does not appear when autopost occurs
                            }
                            else
                            {
                                ShowPayment();
                            }
                        }
                        else
                        {
                            throw new Exception( errorMessage );
                        }
                    }
                    else
                    {
                        var registrationId = SaveChanges();
                        if ( registrationId.HasValue )
                        {
                            ShowSuccess( registrationId.Value );
                        }
                        else
                        {
                            ShowSummary();
                        }
                    }
                }
                else
                {
                    ShowError( "Please correct the following:", string.Format( "<ul><li>{0}</li></ul>", summaryErrors.AsDelimited( "</li><li>" ) ) );
                    ShowSummary();
                }
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbPaymentPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPaymentPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == PanelIndex.PanelPayment )
            {
                _saveNavigationHistory = true;

                ShowSummary();
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        /// <summary>
        /// Handles the Click event of the lbStep2Return control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbStep2Return_Click( object sender, EventArgs e )
        {
            if ( CurrentPanel == PanelIndex.PanelSummary || CurrentPanel == PanelIndex.PanelPayment )
            {
                int? registrationId = SaveChanges();
                if ( registrationId.HasValue )
                {
                    ShowSuccess( registrationId.Value );
                }
                else
                {
                    if ( CurrentPanel == PanelIndex.PanelSummary )
                    {
                        ShowSummary();
                    }
                    else
                    {
                        // Failure on entering payment info, resubmit step 1
                        string errorMessage = string.Empty;
                        if ( ProcessStep1( out errorMessage ) )
                        {
                            ShowPayment();
                        }
                        else
                        {
                            ShowSummary();
                        }
                    }
                }
            }
            else
            {
                ShowHowMany();
            }

            hfTriggerScroll.Value = "true";
        }

        #endregion

        #region Registrant Panel Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFamilyMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlFamilyMembers_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetRegistrantFields( ddlFamilyMembers.SelectedValueAsInt() );

            decimal currentStep = ( FormCount * CurrentRegistrantIndex ) + CurrentFormIndex + 1;
            PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
            pnlRegistrantProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();
            pnlFamilyMembers.Style[HtmlTextWriterStyle.Display] = "block";
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
                RegistrationState.Registrants.ForEach( r => r.DiscountApplies = false );

                RegistrationTemplateDiscount discount = null;
                bool validDiscount = true;

                string discountCode = tbDiscountCode.Text;
                if ( !string.IsNullOrWhiteSpace( discountCode ) )
                {
                    discount = RegistrationTemplate.Discounts
                        .Where( d => d.Code.Equals( discountCode, StringComparison.OrdinalIgnoreCase ) )
                        .FirstOrDefault();

                    if ( discount == null )
                    {
                        validDiscount = false;
                        nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                        nbDiscountCode.Text = string.Format( "'{0}' is not a valid {1}.", discountCode, DiscountCodeTerm );
                        nbDiscountCode.Visible = true;
                    }

                    if ( validDiscount && discount.MinRegistrants.HasValue && RegistrationState.RegistrantCount < discount.MinRegistrants.Value )
                    {
                        nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                        nbDiscountCode.Text = string.Format( "The '{0}' {1} requires at least {2} registrants.", discountCode, DiscountCodeTerm, discount.MinRegistrants.Value );
                        nbDiscountCode.Visible = true;
                        validDiscount = false;
                    }

                    if ( validDiscount && discount.StartDate.HasValue && RockDateTime.Today < discount.StartDate.Value )
                    {
                        nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                        nbDiscountCode.Text = string.Format( "The '{0}' {1} is not available yet.", discountCode, DiscountCodeTerm );
                        nbDiscountCode.Visible = true;
                        validDiscount = false;
                    }

                    if ( validDiscount && discount.EndDate.HasValue && RockDateTime.Today > discount.EndDate.Value )
                    {
                        nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                        nbDiscountCode.Text = string.Format( "The '{0}' {1} has expired.", discountCode, DiscountCodeTerm );
                        nbDiscountCode.Visible = true;
                        validDiscount = false;
                    }

                    if ( validDiscount && discount.MaxUsage.HasValue && RegistrationInstanceState != null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var instances = new RegistrationService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( r =>
                                    r.RegistrationInstanceId == RegistrationInstanceState.Id &&
                                    ( !RegistrationState.RegistrationId.HasValue || r.Id != RegistrationState.RegistrationId.Value ) &&
                                    r.DiscountCode == discountCode )
                                .Count();
                            if ( instances >= discount.MaxUsage.Value )
                            {
                                nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                                nbDiscountCode.Text = string.Format( "The '{0}' {1} is no longer available.", discountCode, DiscountCodeTerm );
                                nbDiscountCode.Visible = true;
                                validDiscount = false;
                            }
                        }
                    }

                    if ( validDiscount && discount.MaxRegistrants.HasValue )
                    {
                        for ( int i = 0; i < RegistrationState.Registrants.Count; i++ )
                        {
                            RegistrationState.Registrants[i].DiscountApplies = i < discount.MaxRegistrants.Value;
                        }
                    }
                }
                else
                {
                    validDiscount = false;
                }

                RegistrationState.DiscountCode = validDiscount ? discountCode : string.Empty;
                RegistrationState.DiscountPercentage = validDiscount ? discount.DiscountPercentage : 0.0m;
                RegistrationState.DiscountAmount = validDiscount ? discount.DiscountAmount : 0.0m;

                CreateDynamicControls( true );
            }
        }

        #endregion

        #region Success Panel Events

        /// <summary>
        /// Handles the Click event of the lbSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveAccount_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference";
                nbSaveAccount.Visible = true;
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                if ( phCreateLogin.Visible )
                {
                    if ( string.IsNullOrWhiteSpace( txtUserName.Text ) || string.IsNullOrWhiteSpace( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = "Missing Information";
                        nbSaveAccount.Text = "A username and password are required when saving an account";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( new UserLoginService( rockContext ).GetByUserName( txtUserName.Text ) != null )
                    {
                        nbSaveAccount.Title = "Invalid Username";
                        nbSaveAccount.Text = "The selected Username is already being used.  Please select a different Username";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( !UserLoginService.IsPasswordValid( txtPassword.Text ) )
                    {
                        nbSaveAccount.Title = string.Empty;
                        nbSaveAccount.Text = UserLoginService.FriendlyPasswordRules();
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }

                    if ( txtPasswordConfirm.Text != txtPassword.Text )
                    {
                        nbSaveAccount.Title = "Invalid Password";
                        nbSaveAccount.Text = "The password and password confirmation do not match";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                        return;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( txtSaveAccount.Text ) )
                {
                    GatewayComponent gateway = null;
                    if ( RegistrationTemplate != null && RegistrationTemplate.FinancialGateway != null )
                    {
                        gateway = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                    }

                    if ( gateway != null )
                    {
                        var ccCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                        string errorMessage = string.Empty;

                        PersonAlias authorizedPersonAlias = null;
                        string referenceNumber = string.Empty;
                        FinancialPaymentDetail paymentDetail = null;
                        int? currencyTypeValueId = ccCurrencyType.Id;

                        var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( RegistrationTemplate.FinancialGateway.Id, TransactionCode );
                        if ( transaction != null && transaction.AuthorizedPersonAlias != null )
                        {
                            authorizedPersonAlias = transaction.AuthorizedPersonAlias;
                            if ( transaction.FinancialGateway != null )
                            {
                                transaction.FinancialGateway.LoadAttributes( rockContext );
                            }

                            referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                            paymentDetail = transaction.FinancialPaymentDetail;
                        }

                        if ( authorizedPersonAlias != null && authorizedPersonAlias.Person != null && paymentDetail != null )
                        {
                            if ( phCreateLogin.Visible )
                            {
                                var user = UserLoginService.Create(
                                    rockContext,
                                    authorizedPersonAlias.Person,
                                    Rock.Model.AuthenticationServiceType.Internal,
                                    EntityTypeCache.Get( Rock.SystemGuid.EntityType.AUTHENTICATION_DATABASE.AsGuid() ).Id,
                                    txtUserName.Text,
                                    txtPassword.Text,
                                    false );

                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                                mergeFields.Add( "ConfirmAccountUrl", RootPath + "ConfirmAccount" );
                                mergeFields.Add( "Person", authorizedPersonAlias.Person );
                                mergeFields.Add( "User", user );

                                var emailMessage = new RockEmailMessage( GetAttributeValue( "ConfirmAccountTemplate" ).AsGuid() );
                                emailMessage.AddRecipient( new RecipientData( authorizedPersonAlias.Person.Email, mergeFields ) );
                                emailMessage.AppRoot = ResolveRockUrl( "~/" );
                                emailMessage.ThemeRoot = ResolveRockUrl( "~~/" );
                                emailMessage.CreateCommunicationRecord = false;
                                emailMessage.Send();
                            }

                            if ( errorMessage.Any() )
                            {
                                nbSaveAccount.Title = "Invalid Transaction";
                                nbSaveAccount.Text = "Sorry, the account information cannot be saved. " + errorMessage;
                                nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                                nbSaveAccount.Visible = true;
                            }
                            else
                            {
                                if ( authorizedPersonAlias != null )
                                {
                                    var savedAccount = new FinancialPersonSavedAccount();
                                    savedAccount.PersonAliasId = authorizedPersonAlias.Id;
                                    savedAccount.ReferenceNumber = referenceNumber;
                                    savedAccount.Name = txtSaveAccount.Text;
                                    savedAccount.TransactionCode = TransactionCode;
                                    savedAccount.FinancialGatewayId = RegistrationTemplate.FinancialGateway.Id;
                                    savedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
                                    savedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentDetail.AccountNumberMasked;
                                    savedAccount.FinancialPaymentDetail.CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId;
                                    savedAccount.FinancialPaymentDetail.CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId;
                                    savedAccount.FinancialPaymentDetail.NameOnCardEncrypted = paymentDetail.NameOnCardEncrypted;
                                    savedAccount.FinancialPaymentDetail.ExpirationMonthEncrypted = paymentDetail.ExpirationMonthEncrypted;
                                    savedAccount.FinancialPaymentDetail.ExpirationYearEncrypted = paymentDetail.ExpirationYearEncrypted;
                                    savedAccount.FinancialPaymentDetail.BillingLocationId = paymentDetail.BillingLocationId;

                                    var savedAccountService = new FinancialPersonSavedAccountService( rockContext );
                                    savedAccountService.Add( savedAccount );
                                    rockContext.SaveChanges();

                                    cbSaveAccount.Visible = false;
                                    txtSaveAccount.Visible = false;
                                    phCreateLogin.Visible = false;
                                    divSaveActions.Visible = false;

                                    nbSaveAccount.Title = "Success";
                                    nbSaveAccount.Text = "The account has been saved for future use";
                                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Success;
                                    nbSaveAccount.Visible = true;
                                }
                            }
                        }
                        else
                        {
                            nbSaveAccount.Title = "Invalid Transaction";
                            nbSaveAccount.Text = "Sorry, the account information cannot be saved as there's not a valid transaction code to reference.";
                            nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                            nbSaveAccount.Visible = true;
                        }
                    }
                    else
                    {
                        nbSaveAccount.Title = "Invalid Gateway";
                        nbSaveAccount.Text = "Sorry, the financial gateway information for this type of transaction is not valid.";
                        nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                        nbSaveAccount.Visible = true;
                    }
                }
                else
                {
                    nbSaveAccount.Title = "Missing Account Name";
                    nbSaveAccount.Text = "Please enter a name to use for this account.";
                    nbSaveAccount.NotificationBoxType = NotificationBoxType.Danger;
                    nbSaveAccount.Visible = true;
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        #region State Methods

        /// <summary>
        /// Sets the registration state
        /// </summary>
        private bool SetRegistrationState()
        {
            string registrationSlug = PageParameter( SLUG_PARAM_NAME );
            int? registrationInstanceId = PageParameter( REGISTRATION_INSTANCE_ID_PARAM_NAME ).AsIntegerOrNull();
            int? registrationId = PageParameter( REGISTRATION_ID_PARAM_NAME ).AsIntegerOrNull();
            int? groupId = PageParameter( GROUP_ID_PARAM_NAME ).AsIntegerOrNull();
            int? campusId = PageParameter( CAMPUS_ID_PARAM_NAME ).AsIntegerOrNull();
            int? eventOccurrenceId = PageParameter( EVENT_OCCURRENCE_ID_PARAM_NAME ).AsIntegerOrNull();

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

                if ( registration == null )
                {
                    ShowError( "Error", "Registration not found" );
                    return false;
                }

                if ( CurrentPersonId == null )
                {
                    ShowWarning( "Please log in", "You must be logged in to access this registration." );
                    return false;
                }

                // Only allow the person that was logged in when this registration was created.
                // If the logged in person, registered on someone elses behalf (for example, husband logged in, but entered wife's name as the Registrar),
                // also allow that person to access the registration
                if ( ( registration.PersonAlias != null && registration.PersonAlias.PersonId == CurrentPersonId.Value ) ||
                    ( registration.CreatedByPersonAlias != null && registration.CreatedByPersonAlias.PersonId == CurrentPersonId.Value ) )
                {
                    RegistrationInstanceState = registration.RegistrationInstance;
                    RegistrationState = new RegistrationInfo( registration, rockContext );
                    RegistrationState.PreviousPaymentTotal = registrationService.GetTotalPayments( registration.Id );
                }
                else
                {
                    ShowWarning( "Sorry", "You are not allowed to view or edit the selected registration since you are not the one who created the registration." );
                    return false;
                }

                numHowMany.Value = registration.Registrants.Count();

                // set group id
                if ( groupId.HasValue )
                {
                    GroupId = groupId;
                }
                else if ( !string.IsNullOrWhiteSpace( registrationSlug ) )
                {
                    var dateTime = RockDateTime.Now;
                    var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( l =>
                            l.UrlSlug == registrationSlug &&
                            l.RegistrationInstance != null &&
                            l.RegistrationInstance.IsActive &&
                            l.RegistrationInstance.RegistrationTemplate != null &&
                            l.RegistrationInstance.RegistrationTemplate.IsActive &&
                            ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                            ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                        .FirstOrDefault();
                    if ( linkage != null )
                    {
                        GroupId = linkage.GroupId;
                    }
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

                CampusId = campusId;
                if ( linkage != null )
                {
                    RegistrationInstanceState = linkage.RegistrationInstance;
                    GroupId = linkage.GroupId;
                    RegistrationState = new RegistrationInfo( CurrentPerson );
                }
            }

            // A registration instance id was specified
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

            // If registration instance id and event occurrence were specified, but a group (linkage) hasn't been loaded, find the first group for the event occurrence
            if ( RegistrationInstanceState != null && eventOccurrenceId.HasValue && !groupId.HasValue )
            {
                var eventItemOccurrence = new EventItemOccurrenceService( rockContext )
                    .Queryable()
                    .Where( o => o.Id == eventOccurrenceId.Value )
                    .FirstOrDefault();
                if ( eventItemOccurrence != null )
                {
                    CampusId = eventItemOccurrence.CampusId;

                    var linkage = eventItemOccurrence.Linkages
                        .Where( l => l.RegistrationInstanceId == RegistrationInstanceState.Id )
                        .FirstOrDefault();

                    if ( linkage != null )
                    {
                        GroupId = linkage.GroupId;
                    }
                }
            }

            if ( RegistrationState != null &&
                RegistrationState.FamilyGuid == Guid.Empty &&
                RegistrationTemplate != null &&
                RegistrationTemplate.RegistrantsSameFamily != RegistrantsSameFamily.Ask )
            {
                RegistrationState.FamilyGuid = Guid.NewGuid();
            }

            if ( RegistrationState != null )
            {
                // Calculate the available slots. If maxAttendees is 0 that means unlimited since this is not a nullable.
                if ( !RegistrationState.RegistrationId.HasValue && RegistrationInstanceState != null && RegistrationInstanceState.MaxAttendees.HasValue )
                {
                    var existingRegistrantIds = RegistrationState.Registrants.Select( r => r.Id ).ToList();
                    var otherRegistrantsCount = new RegistrationRegistrantService( new RockContext() ).Queryable()
                        .Where( a => a.Registration.RegistrationInstanceId == registrationInstanceId && !a.Registration.IsTemporary )
                        .Where( a => !existingRegistrantIds.Contains( a.Id ) ).Count();

                    int otherRegistrants = RegistrationInstanceState.Registrations
                        .Where( r => !r.IsTemporary )
                        .Sum( r => r.Registrants.Where( t => !existingRegistrantIds.Contains( t.Id ) ).Count() );

                    RegistrationState.SlotsAvailable = RegistrationInstanceState.MaxAttendees - otherRegistrants;
                }

                if ( !RegistrationState.Registrants.Any() )
                {
                    SetRegistrantState( 1 );
                }
            }

            if ( RegistrationTemplate != null &&
                RegistrationTemplate.FinancialGateway != null )
            {
                var threeStepGateway = RegistrationTemplate.FinancialGateway.GetGatewayComponent() as IThreeStepGatewayComponent;
                Using3StepGateway = threeStepGateway != null;
                if ( Using3StepGateway )
                {
                    Step2IFrameUrl = ResolveRockUrl( threeStepGateway.Step2FormUrl );
                }
            }

            if ( RegistrationTemplate != null )
            {
                RegistrationAttributesState = new AttributeService( rockContext ).GetByEntityTypeId( new Registration().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "RegistrationTemplateId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( RegistrationTemplate.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList()
                .Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) )
                .ToList();

                // only show the Registration Attributes Before Registrants that have a category of REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION
                this.RegistrationAttributeIdsBeforeRegistrants = RegistrationAttributesState.Where( a => a.Categories.Any( c => c.Guid == Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION.AsGuid() ) ).Select( a => a.Id ).ToList();

                // only show the Registration Attributes After Registrants that have don't have a category or have a category of REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION
                this.RegistrationAttributeIdsAfterRegistrants = RegistrationAttributesState.Where( a => !a.Categories.Any() || a.Categories.Any( c => c.Guid == Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION.AsGuid() ) ).Select( a => a.Id ).ToList();
            }

            SignInline = false;
            if ( RegistrationTemplate != null &&
                RegistrationTemplate.RequiredSignatureDocumentTemplate != null &&
                RegistrationTemplate.RequiredSignatureDocumentTemplate.ProviderEntityType != null )
            {
                var provider = DigitalSignatureContainer.GetComponent( RegistrationTemplate.RequiredSignatureDocumentTemplate.ProviderEntityType.Name );
                if ( provider != null && provider.IsActive )
                {
                    SignInline = GetAttributeValue( "SignInline" ).AsBoolean() && RegistrationTemplate.SignatureDocumentAction == SignatureDocumentAction.Embed;
                    DigitalSignatureComponentTypeName = RegistrationTemplate.RequiredSignatureDocumentTemplate.ProviderEntityType.Name;
                    DigitalSignatureComponent = provider;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds (or removes) registrants to or from the registration. Only newly added registrants can
        /// can be removed. Any existing (saved) registrants cannot be removed from the registration
        /// </summary>
        /// <param name="registrantCount">The number of registrants that registration should have.</param>
        private void SetRegistrantState( int registrantCount )
        {
            if ( RegistrationState == null )
            {
                return;
            }

            decimal cost = RegistrationTemplate.Cost;
            if ( ( RegistrationTemplate.SetCostOnInstance ?? false ) && RegistrationInstanceState != null )
            {
                cost = RegistrationInstanceState.Cost ?? 0.0m;
            }

            // If this is the first registrant being added and all are in the same family, default it to the current person
            if ( RegistrationState.RegistrantCount == 0 && registrantCount == 1 && CurrentPerson != null && RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes )
            {
                var registrant = new RegistrantInfo( RegistrationInstanceState, CurrentPerson );
                if ( RegistrationTemplate.ShowCurrentFamilyMembers )
                {
                    // If currentfamily members can be selected, the firstname and lastname fields will be
                    // disabled so values need to be set (in case those fields did not have the 'showCurrentValue'
                    // option selected
                    foreach ( var field in RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields )
                        .Where( f =>
                            ( f.PersonFieldType == RegistrationPersonFieldType.FirstName ||
                            f.PersonFieldType == RegistrationPersonFieldType.LastName ) &&
                            f.FieldSource == RegistrationFieldSource.PersonField ) )
                    {
                        registrant.FieldValues.AddOrReplace(
                            field.Id,
                            new FieldValueObject( field, field.PersonFieldType == RegistrationPersonFieldType.FirstName ? CurrentPerson.NickName : CurrentPerson.LastName ) );
                    }
                }

                registrant.Cost = cost;
                registrant.FamilyGuid = RegistrationState.FamilyGuid;
                if ( RegistrationState.Registrants.Count >= RegistrationState.SlotsAvailable )
                {
                    registrant.OnWaitList = true;
                }

                RegistrationState.Registrants.Add( registrant );
            }

            // While the number of registrants belonging to registration is less than the selected count, add another registrant
            while ( RegistrationState.RegistrantCount < registrantCount )
            {
                var registrant = new RegistrantInfo { Cost = cost };
                if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.No )
                {
                    registrant.FamilyGuid = Guid.NewGuid();
                }
                else if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes )
                {
                    registrant.FamilyGuid = RegistrationState.FamilyGuid;
                }

                if ( RegistrationState.Registrants.Count >= RegistrationState.SlotsAvailable )
                {
                    registrant.OnWaitList = true;
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

        #endregion

        #region Save Methods

        /// <summary>
        /// Validates the summary.
        /// </summary>
        /// <returns></returns>
        private List<string> ValidateSummary()
        {
            var validationErrors = new List<string>();

            var autoDiscounts = RegistrationTemplate.Discounts.Where( d => d.AutoApplyDiscount ).Select( d => d.Code ).ToList();
            if ( ( RegistrationState.DiscountCode ?? string.Empty ) != tbDiscountCode.Text && !autoDiscounts.Contains( RegistrationState.DiscountCode ) )
            {
                validationErrors.Add( "A discount code has not been applied! Please click the 'Apply' button to apply (or clear) a discount code." );
            }

            decimal balanceDue = RegistrationState.DiscountedCost - RegistrationState.PreviousPaymentTotal;
            if ( RegistrationState.PaymentAmount > balanceDue )
            {
                validationErrors.Add( "Amount To Pay is greater than the amount due. Please check the amount you have selected to pay." );
            }

            // Validate payment information if there is a payment due or if there is a payment amount being provided
            if ( minimumPayment.HasValue && minimumPayment > 0.0M || RegistrationState.PaymentAmount > 0.0M )
            {
                if ( RegistrationState.PaymentAmount < minimumPayment )
                {
                    validationErrors.Add( string.Format( "Amount To Pay Today must be at least {0:C2}", minimumPayment ) );
                }

                // If not using a saved account validate cc fields
                if ( !Using3StepGateway && ( rblSavedCC.Items.Count == 0 || ( rblSavedCC.SelectedValueAsInt() ?? 0 ) == 0 ) )
                {
                    if ( txtCardFirstName.Visible && string.IsNullOrWhiteSpace( txtCardFirstName.Text ) )
                    {
                        validationErrors.Add( "First Name on Card is required" );
                    }

                    if ( txtCardLastName.Visible && string.IsNullOrWhiteSpace( txtCardLastName.Text ) )
                    {
                        validationErrors.Add( "Last Name on Card is required" );
                    }

                    if ( txtCardName.Visible && string.IsNullOrWhiteSpace( txtCardName.Text ) )
                    {
                        validationErrors.Add( "Name on Card is required" );
                    }

                    var rgx = new System.Text.RegularExpressions.Regex( @"[^\d]" );
                    string ccNum = rgx.Replace( txtCreditCard.Text, string.Empty );
                    if ( string.IsNullOrWhiteSpace( ccNum ) )
                    {
                        validationErrors.Add( "Card Number is required" );
                    }

                    if ( !mypExpiration.SelectedDate.HasValue )
                    {
                        validationErrors.Add( "Card Expiration Date is required" );
                    }

                    if ( string.IsNullOrWhiteSpace( txtCVV.Text ) )
                    {
                        validationErrors.Add( "Card Security Code is required" );
                    }

                    if ( acBillingAddress.Visible && (
                        string.IsNullOrWhiteSpace( acBillingAddress.Street1 ) ||
                        string.IsNullOrWhiteSpace( acBillingAddress.City ) ||
                        string.IsNullOrWhiteSpace( acBillingAddress.State ) ||
                        string.IsNullOrWhiteSpace( acBillingAddress.PostalCode ) ) )
                    {
                        validationErrors.Add( "Billing Address is required" );
                    }
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns></returns>
        private int? SaveChanges()
        {
            if ( !string.IsNullOrWhiteSpace( TransactionCode ) )
            {
                ShowError( string.Empty, "You have already completed this " + RegistrationTerm.ToLower() );
                return null;
            }

            Registration registration = null;

            if ( RegistrationState != null && RegistrationState.Registrants.Any() && RegistrationTemplate != null )
            {
                var rockContext = new RockContext();

                var registrationService = new RegistrationService( rockContext );

                bool isNewRegistration = true;
                var previousRegistrantPersonIds = new List<int>();
                if ( RegistrationState.RegistrationId.HasValue )
                {
                    var previousRegistration = registrationService.Get( RegistrationState.RegistrationId.Value );
                    if ( previousRegistration != null )
                    {
                        isNewRegistration = false;
                        previousRegistrantPersonIds = previousRegistration.Registrants
                            .Where( r => r.PersonAlias != null )
                            .Select( r => r.PersonAlias.PersonId )
                            .ToList();
                    }
                }

                try
                {
                    bool hasPayment = ( RegistrationState.PaymentAmount ?? 0.0m ) > 0.0m;

                    // Save the registration
                    registration = SaveRegistration( rockContext, hasPayment );
                    if ( registration != null )
                    {
                        // If there is a payment being made, process the payment
                        if ( hasPayment )
                        {
                            string errorMessage = string.Empty;
                            if ( Using3StepGateway )
                            {
                                if ( !ProcessStep3( rockContext, registration, hfStep2ReturnQueryString.Value, out errorMessage ) )
                                {
                                    throw new Exception( errorMessage );
                                }
                            }
                            else
                            {
                                if ( !ProcessPayment( rockContext, registration, out errorMessage ) )
                                {
                                    throw new Exception( errorMessage );
                                }
                            }
                        }

                        // If there is a valid registration, and nothing went wrong processing the payment, add registrants to group and send the notifications
                        if ( registration != null && !registration.IsTemporary )
                        {
                            ProcessPostSave( isNewRegistration, registration, previousRegistrantPersonIds, rockContext );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );

                    string message = ex.Message;
                    while ( ex.InnerException != null )
                    {
                        ex = ex.InnerException;
                        message = ex.Message;
                    }

                    ShowError( "An error occurred processing your " + RegistrationTerm.ToLowerInvariant(), ex.Message );

                    // Try to delete the registration if it was just created
                    try
                    {
                        if ( isNewRegistration && registration != null && registration.Id > 0 )
                        {
                            RegistrationState.RegistrationId = null;
                            using ( var newRockContext = new RockContext() )
                            {
                                HistoryService.DeleteChanges( newRockContext, typeof( Registration ), registration.Id );

                                var newRegistrationService = new RegistrationService( newRockContext );
                                var newRegistration = newRegistrationService.Get( registration.Id );
                                if ( newRegistration != null )
                                {
                                    newRegistrationService.Delete( newRegistration );
                                    newRockContext.SaveChanges();
                                }
                            }
                        }
                    }
                    catch { }

                    return ( int? ) null;
                }
            }

            return registration != null ? registration.Id : ( int? ) null;
        }

        /// <summary>
        /// Sends notifications after the registration is saved
        /// </summary>
        /// <param name="isNewRegistration">if set to <c>true</c> [is new registration].</param>
        /// <param name="registration">The registration.</param>
        /// <param name="previousRegistrantPersonIds">The previous registrant person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ProcessPostSave( bool isNewRegistration, Registration registration, List<int> previousRegistrantPersonIds, RockContext rockContext )
        {
            try
            {
                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    registration.SavePersonNotesAndHistory( registration.PersonAlias.Person, this.CurrentPersonAliasId, previousRegistrantPersonIds );
                }
                // This occurs when the registrar is logged in
                else if ( registration.PersonAliasId.HasValue )
                {
                    var registrar = new PersonAliasService( rockContext ).Get( registration.PersonAliasId.Value );
                    registration.SavePersonNotesAndHistory( registrar.Person, this.CurrentPersonAliasId, previousRegistrantPersonIds );
                }

                AddRegistrantsToGroup( rockContext, registration );

                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );

                // Send/Resend a confirmation
                var confirmation = new Rock.Transactions.SendRegistrationConfirmationTransaction();
                confirmation.RegistrationId = registration.Id;
                confirmation.AppRoot = appRoot;
                confirmation.ThemeRoot = themeRoot;
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( confirmation );

                if ( isNewRegistration )
                {
                    // Send notice of a new registration
                    var notification = new Rock.Transactions.SendRegistrationNotificationTransaction();
                    notification.RegistrationId = registration.Id;
                    notification.AppRoot = appRoot;
                    notification.ThemeRoot = themeRoot;
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( notification );
                }

                var registrationService = new RegistrationService( new RockContext() );
                var newRegistration = registrationService.Get( registration.Id );
                if ( newRegistration != null )
                {
                    if ( isNewRegistration )
                    {
                        if ( RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue )
                        {
                            string email = newRegistration.ConfirmationEmail;
                            if ( string.IsNullOrWhiteSpace( email ) && newRegistration.PersonAlias != null && newRegistration.PersonAlias.Person != null )
                            {
                                email = newRegistration.PersonAlias.Person.Email;
                            }

                            Guid? adultRole = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
                            var groupMemberService = new GroupMemberService( rockContext );

                            foreach ( var registrant in newRegistration.Registrants.Where( r => r.PersonAlias != null && r.PersonAlias.Person != null ) )
                            {
                                var assignedTo = registrant.PersonAlias.Person;

                                var registrantIsAdult = adultRole.HasValue && groupMemberService
                                    .Queryable().AsNoTracking()
                                    .Any( m =>
                                        m.PersonId == registrant.PersonAlias.PersonId &&
                                        m.GroupRole.Guid.Equals( adultRole.Value ) );
                                if ( !registrantIsAdult && newRegistration.PersonAlias != null && newRegistration.PersonAlias.Person != null )
                                {
                                    assignedTo = newRegistration.PersonAlias.Person;
                                }
                                else
                                {
                                    if ( !string.IsNullOrWhiteSpace( registrant.PersonAlias.Person.Email ) )
                                    {
                                        email = registrant.PersonAlias.Person.Email;
                                    }
                                }

                                if ( DigitalSignatureComponent != null )
                                {
                                    var sendDocumentTxn = new Rock.Transactions.SendDigitalSignatureRequestTransaction();
                                    sendDocumentTxn.SignatureDocumentTemplateId = RegistrationTemplate.RequiredSignatureDocumentTemplateId.Value;
                                    sendDocumentTxn.AppliesToPersonAliasId = registrant.PersonAlias.Id;
                                    sendDocumentTxn.AssignedToPersonAliasId = assignedTo.PrimaryAliasId ?? 0;
                                    sendDocumentTxn.DocumentName = string.Format( "{0}_{1}", RegistrationInstanceState.Name.RemoveSpecialCharacters(), registrant.PersonAlias.Person.FullName.RemoveSpecialCharacters() );
                                    sendDocumentTxn.Email = email;
                                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendDocumentTxn );
                                }
                            }
                        }

                        newRegistration.LaunchWorkflow( RegistrationTemplate.RegistrationWorkflowTypeId, newRegistration.ToString() );
                        newRegistration.LaunchWorkflow( RegistrationInstanceState.RegistrationWorkflowTypeId, newRegistration.ToString() );
                    }

                    RegistrationInstanceState = newRegistration.RegistrationInstance;
                    RegistrationState = new RegistrationInfo( newRegistration, rockContext );
                    RegistrationState.PreviousPaymentTotal = registrationService.GetTotalPayments( registration.Id );
                }
            }
            catch ( Exception postSaveEx )
            {
                ShowWarning( "The following occurred after processing your " + RegistrationTerm, postSaveEx.Message );
                ExceptionLogService.LogException( postSaveEx, Context, RockPage.PageId, RockPage.Layout.SiteId, CurrentPersonAlias );
            }
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
            var groupService = new GroupService( rockContext );
            var documentService = new SignatureDocumentService( rockContext );

            // variables to keep track of the family that new people should be added to
            int? singleFamilyId = null;
            var multipleFamilyGroupIds = new Dictionary<Guid, int>();

            var dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            var dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );
            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            var adultRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();
            var childRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            bool newRegistration = false;
            Registration registration = null;
            Person registrar = null;
            var registrationChanges = new History.HistoryChangeList();

            if ( RegistrationState.RegistrationId.HasValue )
            {
                registration = registrationService.Get( RegistrationState.RegistrationId.Value );
            }

            if ( registration == null )
            {
                newRegistration = true;
                registration = new Registration();
                registrationService.Add( registration );
                registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Registration" );
            }
            else
            {
                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    registrar = registration.PersonAlias.Person;
                }
            }

            registration.RegistrationInstanceId = RegistrationInstanceState.Id;

            // If the Registration Instance linkage specified a group, load it now
            Group group = null;
            if ( GroupId.HasValue )
            {
                group = new GroupService( rockContext ).Get( GroupId.Value );
                if ( group != null && ( !registration.GroupId.HasValue || registration.GroupId.Value != group.Id ) )
                {
                    registration.GroupId = group.Id;
                    History.EvaluateChange( registrationChanges, "Group", string.Empty, group.Name );
                }
            }

            bool newRegistrar = newRegistration ||
                registration.FirstName == null || !registration.FirstName.Equals( RegistrationState.FirstName, StringComparison.OrdinalIgnoreCase ) ||
                registration.LastName == null || !registration.LastName.Equals( RegistrationState.LastName, StringComparison.OrdinalIgnoreCase );

            History.EvaluateChange( registrationChanges, "First Name", registration.FirstName, RegistrationState.FirstName );
            registration.FirstName = RegistrationState.FirstName;

            History.EvaluateChange( registrationChanges, "Last Name", registration.LastName, RegistrationState.LastName );
            registration.LastName = RegistrationState.LastName;

            History.EvaluateChange( registrationChanges, "Confirmation Email", registration.ConfirmationEmail, RegistrationState.ConfirmationEmail );
            registration.ConfirmationEmail = RegistrationState.ConfirmationEmail;

            History.EvaluateChange( registrationChanges, "Discount Code", registration.DiscountCode, RegistrationState.DiscountCode );
            registration.DiscountCode = RegistrationState.DiscountCode;

            History.EvaluateChange( registrationChanges, "Discount Percentage", registration.DiscountPercentage, RegistrationState.DiscountPercentage );
            registration.DiscountPercentage = RegistrationState.DiscountPercentage;

            History.EvaluateChange( registrationChanges, "Discount Amount", registration.DiscountAmount, RegistrationState.DiscountAmount );
            registration.DiscountAmount = RegistrationState.DiscountAmount;

            if ( newRegistrar )
            {
                // Businesses have no first name.  This resolves null reference issues downstream.
                if ( CurrentPerson != null && CurrentPerson.FirstName == null )
                {
                    CurrentPerson.FirstName = string.Empty;
                }

                if ( CurrentPerson != null && CurrentPerson.NickName == null )
                {
                    CurrentPerson.NickName = CurrentPerson.FirstName;
                }

                // If the 'your name' value equals the currently logged in person, use their person alias id
                if ( CurrentPerson != null &&
                ( CurrentPerson.NickName.Trim().Equals( registration.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) ||
                    CurrentPerson.FirstName.Trim().Equals( registration.FirstName.Trim(), StringComparison.OrdinalIgnoreCase ) ) &&
                CurrentPerson.LastName.Trim().Equals( registration.LastName.Trim(), StringComparison.OrdinalIgnoreCase ) )
                {
                    registrar = CurrentPerson;
                    registration.PersonAliasId = CurrentPerson.PrimaryAliasId;

                    // If email that logged in user used is different than their stored email address, update their stored value
                    if ( !string.IsNullOrWhiteSpace( registration.ConfirmationEmail ) &&
                        !registration.ConfirmationEmail.Trim().Equals( CurrentPerson.Email.Trim(), StringComparison.OrdinalIgnoreCase ) &&
                        ( !cbUpdateEmail.Visible || cbUpdateEmail.Checked ) )
                    {
                        var person = personService.Get( CurrentPerson.Id );
                        if ( person != null )
                        {
                            person.Email = registration.ConfirmationEmail;
                            rockContext.SaveChanges();
                        }
                    }
                }
                else
                {
                    // otherwise look for one and one-only match by name/email
                    registrar = personService.FindPerson( registration.FirstName, registration.LastName, registration.ConfirmationEmail, true );
                    if ( registrar != null )
                    {
                        registration.PersonAliasId = registrar.PrimaryAliasId;
                    }
                    else
                    {
                        registrar = null;
                        registration.PersonAlias = null;
                        registration.PersonAliasId = null;
                    }
                }
            }

            // Set the family guid for any other registrants that were selected to be in the same family
            if ( registrar != null )
            {
                var family = registrar.GetFamily( rockContext );
                if ( family != null )
                {
                    multipleFamilyGroupIds.AddOrIgnore( RegistrationState.FamilyGuid, family.Id );
                    if ( !singleFamilyId.HasValue )
                    {
                        singleFamilyId = family.Id;
                    }
                }
            }

            // Make sure there's an actual person associated to registration
            if ( !registration.PersonAliasId.HasValue )
            {
                // If a match was not found, create a new person
                var person = new Person();
                person.FirstName = registration.FirstName;
                person.LastName = registration.LastName;
                person.IsEmailActive = true;
                person.Email = registration.ConfirmationEmail;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                if ( dvcConnectionStatus != null )
                {
                    person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                }

                if ( dvcRecordStatus != null )
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }

                registrar = SavePerson( rockContext, person, RegistrationState.FamilyGuid, CampusId, null, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId );
                registration.PersonAliasId = registrar != null ? registrar.PrimaryAliasId : ( int? ) null;

                History.EvaluateChange( registrationChanges, "Registrar", string.Empty, registrar.FullName );
            }
            else
            {
                if ( newRegistration )
                {
                    History.EvaluateChange( registrationChanges, "Registrar", string.Empty, registration.ToString() );
                }
            }

            // if this registration was marked as temporary (started from another page, then specified in the url), set IsTemporary to False now that we are done
            if ( registration.IsTemporary )
            {
                registration.IsTemporary = false;
            }

            avcRegistrationAttributesStart.GetEditValues( registration );
            avcRegistrationAttributesEnd.GetEditValues( registration );

            // Save the registration ( so we can get an id )
            rockContext.SaveChanges();
            registration.SaveAttributeValues( rockContext );
            RegistrationState.RegistrationId = registration.Id;

            try
            {
                Task.Run( () =>
                    HistoryService.SaveChanges(
                        new RockContext(),
                        typeof( Registration ),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        registration.Id,
                        registrationChanges,
                        true,
                        CurrentPersonAliasId ) );

                // Get each registrant
                foreach ( var registrantInfo in RegistrationState.Registrants.ToList() )
                {
                    var registrantChanges = new History.HistoryChangeList();
                    var personChanges = new History.HistoryChangeList();

                    RegistrationRegistrant registrant = null;
                    Person person = null;

                    string firstName = registrantInfo.GetFirstName( RegistrationTemplate );
                    string lastName = registrantInfo.GetLastName( RegistrationTemplate );
                    string email = registrantInfo.GetEmail( RegistrationTemplate );

                    if ( registrantInfo.Id > 0 )
                    {
                        registrant = registration.Registrants.FirstOrDefault( r => r.Id == registrantInfo.Id );
                        if ( registrant != null )
                        {
                            person = registrant.Person;
                            if ( person != null && (
                                ( registrant.Person.FirstName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) || registrant.Person.NickName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) ) &&
                                registrant.Person.LastName.Equals( lastName, StringComparison.OrdinalIgnoreCase ) ) )
                            {
                                // Do nothing
                            }
                            else
                            {
                                person = null;
                                registrant.PersonAlias = null;
                                registrant.PersonAliasId = null;
                            }
                        }
                    }
                    else
                    {
                        if ( registrantInfo.PersonId.HasValue && RegistrationTemplate.ShowCurrentFamilyMembers )
                        {
                            person = personService.Get( registrantInfo.PersonId.Value );
                        }
                    }

                    if ( person == null )
                    {
                        // Try to find a matching person based on name and email address
                        person = personService.FindPerson( firstName, lastName, email, true );

                        // Try to find a matching person based on name within same family as registrar
                        if ( person == null && registrar != null && registrantInfo.FamilyGuid == RegistrationState.FamilyGuid )
                        {
                            var familyMembers = registrar.GetFamilyMembers( true, rockContext )
                                .Where( m =>
                                    ( m.Person.FirstName == firstName || m.Person.NickName == firstName ) &&
                                    m.Person.LastName == lastName )
                                .Select( m => m.Person )
                                .ToList();

                            if ( familyMembers.Count() == 1 )
                            {
                                person = familyMembers.First();
                                if ( !string.IsNullOrWhiteSpace( email ) )
                                {
                                    person.Email = email;
                                }
                            }

                            if ( familyMembers.Count() > 1 && !string.IsNullOrWhiteSpace( email ) )
                            {
                                familyMembers = familyMembers
                                    .Where( m =>
                                        m.Email != null &&
                                        m.Email.Equals( email, StringComparison.OrdinalIgnoreCase ) )
                                    .ToList();
                                if ( familyMembers.Count() == 1 )
                                {
                                    person = familyMembers.First();
                                }
                            }
                        }
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
                        person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                        if ( dvcConnectionStatus != null )
                        {
                            person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                        }

                        if ( dvcRecordStatus != null )
                        {
                            person.RecordStatusValueId = dvcRecordStatus.Id;
                        }
                    }

                    int? campusId = CampusId;
                    Location location = null;

                    // Set any of the template's person fields
                    foreach ( var field in RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t => t.FieldSource == RegistrationFieldSource.PersonField ) ) )
                    {
                        // Find the registrant's value
                        var fieldValue = registrantInfo.FieldValues
                            .Where( f => f.Key == field.Id )
                            .Select( f => f.Value.FieldValue )
                            .FirstOrDefault();

                        if ( fieldValue != null )
                        {
                            switch ( field.PersonFieldType )
                            {
                                case RegistrationPersonFieldType.Campus:
                                    campusId = fieldValue.ToString().AsIntegerOrNull();
                                    break;

                                case RegistrationPersonFieldType.MiddleName:
                                    string middleName = fieldValue.ToString().Trim();
                                    History.EvaluateChange( personChanges, "Middle Name", person.MiddleName, middleName );
                                    person.MiddleName = middleName;
                                    break;

                                case RegistrationPersonFieldType.Address:
                                    location = fieldValue as Location;
                                    break;

                                case RegistrationPersonFieldType.Birthdate:
                                    var oldBirthMonth = person.BirthMonth;
                                    var oldBirthDay = person.BirthDay;
                                    var oldBirthYear = person.BirthYear;

                                    person.SetBirthDate( fieldValue as DateTime? );

                                    History.EvaluateChange( personChanges, "Birth Month", oldBirthMonth, person.BirthMonth );
                                    History.EvaluateChange( personChanges, "Birth Day", oldBirthDay, person.BirthDay );
                                    History.EvaluateChange( personChanges, "Birth Year", oldBirthYear, person.BirthYear );
                                    break;

                                case RegistrationPersonFieldType.Grade:
                                    var newGraduationYear = fieldValue.ToString().AsIntegerOrNull();
                                    History.EvaluateChange( personChanges, "Graduation Year", person.GraduationYear, newGraduationYear );
                                    person.GraduationYear = newGraduationYear;
                                    break;

                                case RegistrationPersonFieldType.Gender:
                                    var newGender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                                    History.EvaluateChange( personChanges, "Gender", person.Gender, newGender );
                                    person.Gender = newGender;
                                    break;

                                case RegistrationPersonFieldType.MaritalStatus:
                                    if ( fieldValue != null )
                                    {
                                        int? newMaritalStatusId = fieldValue.ToString().AsIntegerOrNull();
                                        History.EvaluateChange( personChanges, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( newMaritalStatusId ) );
                                        person.MaritalStatusValueId = newMaritalStatusId;
                                    }

                                    break;

                                case RegistrationPersonFieldType.AnniversaryDate:
                                    var oldAnniversaryDate = person.AnniversaryDate;
                                    person.AnniversaryDate = fieldValue.ToString().AsDateTime();
                                    History.EvaluateChange( personChanges, "Anniversary Date", oldAnniversaryDate, person.AnniversaryDate );
                                    break;

                                case RegistrationPersonFieldType.MobilePhone:
                                    SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), personChanges );
                                    break;

                                case RegistrationPersonFieldType.HomePhone:
                                    SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), personChanges );
                                    break;

                                case RegistrationPersonFieldType.WorkPhone:
                                    SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid(), personChanges );
                                    break;

                                case RegistrationPersonFieldType.ConnectionStatus:
                                    var newConnectionStatusId = fieldValue.ToString().AsIntegerOrNull() ?? dvcConnectionStatus.Id;
                                    History.EvaluateChange( personChanges, "Connection Status", DefinedValueCache.GetName( person.ConnectionStatusValueId ), DefinedValueCache.GetName( newConnectionStatusId ) );
                                    person.ConnectionStatusValueId = newConnectionStatusId;
                                    break;
                            }
                        }
                    }

                    // Save the person ( and family if needed )
                    SavePerson( rockContext, person, registrantInfo.FamilyGuid, campusId, location, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId );

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
                            .Select( f => f.Value.FieldValue )
                            .FirstOrDefault();

                        if ( fieldValue != null )
                        {
                            var attribute = AttributeCache.Get( field.AttributeId.Value );
                            if ( attribute != null )
                            {
                                string originalValue = person.GetAttributeValue( attribute.Key );
                                string newValue = fieldValue.ToString();
                                person.SetAttributeValue( attribute.Key, fieldValue.ToString() );

                                // DateTime values must be stored in ISO8601 format as http://www.rockrms.com/Rock/Developer/BookContent/16/16#datetimeformatting
                                if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() ) ||
                                    attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() ) )
                                {
                                    DateTime aDateTime;
                                    if ( DateTime.TryParse( newValue, out aDateTime ) )
                                    {
                                        newValue = aDateTime.ToString( "o" );
                                    }
                                }

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

                                    Helper.SaveAttributeValue( person, attribute, newValue, rockContext );
                                    History.EvaluateChange( personChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                                }
                            }
                        }
                    }

                    string registrantName = person.FullName + ": ";

                    personChanges.ForEach( c => registrantChanges.Add( c ) );

                    if ( registrant == null )
                    {
                        registrant = new RegistrationRegistrant();
                        registrant.Guid = registrantInfo.Guid;
                        registrantService.Add( registrant );
                        registrant.RegistrationId = registration.Id;
                    }

                    registrant.OnWaitList = registrantInfo.OnWaitList;
                    registrant.PersonAliasId = person.PrimaryAliasId;
                    registrant.Cost = registrantInfo.Cost;
                    registrant.DiscountApplies = registrantInfo.DiscountApplies;

                    var registrantFeeService = new RegistrationRegistrantFeeService( rockContext );
                    var registrationTemplateFeeItemService = new RegistrationTemplateFeeItemService( rockContext );

                    // Remove fees
                    // Remove/delete any registrant fees that are no longer in UI with quantity
                    foreach ( var dbFee in registrant.Fees.ToList() )
                    {
                        if ( !registrantInfo.FeeValues.ContainsKey( dbFee.RegistrationTemplateFeeId ) ||
                            registrantInfo.FeeValues[dbFee.RegistrationTemplateFeeId] == null ||
                            !registrantInfo.FeeValues[dbFee.RegistrationTemplateFeeId]
                                .Any( f =>
                                    f.RegistrationTemplateFeeItemId == dbFee.RegistrationTemplateFeeItemId &&
                                    f.Quantity > 0 ) )
                        {
                            var oldFeeValue = string.Format( "'{0}' Fee (Quantity:{1:N0}, Cost:{2:C2}, Option:{3}",
                                    dbFee.RegistrationTemplateFee.Name, dbFee.Quantity, dbFee.Cost, dbFee.Option );

                            registrantChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Fee" ).SetOldValue( oldFeeValue );

                            registrant.Fees.Remove( dbFee );
                            registrantFeeService.Delete( dbFee );
                        }
                    }

                    // Add or Update fees
                    foreach ( var uiFee in registrantInfo.FeeValues.Where( f => f.Value != null ) )
                    {
                        foreach ( var uiFeeOption in uiFee.Value )
                        {
                            var dbFee = registrant.Fees
                                .Where( f =>
                                    f.RegistrationTemplateFeeId == uiFee.Key &&
                                    f.RegistrationTemplateFeeItemId == uiFeeOption.RegistrationTemplateFeeItemId )
                                .FirstOrDefault();

                            if ( dbFee == null )
                            {
                                dbFee = new RegistrationRegistrantFee();
                                dbFee.RegistrationTemplateFeeId = uiFee.Key;
                                var registrationTemplateFeeItem = registrationTemplateFeeItemService.GetNoTracking( uiFeeOption.RegistrationTemplateFeeItemId );
                                if ( registrationTemplateFeeItem != null )
                                {
                                    dbFee.Option = registrationTemplateFeeItem.Name;
                                }

                                dbFee.RegistrationTemplateFeeItemId = uiFeeOption.RegistrationTemplateFeeItemId;
                                registrant.Fees.Add( dbFee );
                            }

                            var templateFee = dbFee.RegistrationTemplateFee;
                            if ( templateFee == null )
                            {
                                templateFee = RegistrationTemplate.Fees.Where( f => f.Id == uiFee.Key ).FirstOrDefault();
                            }

                            string feeName = templateFee != null ? templateFee.Name : "Fee";
                            if ( !string.IsNullOrWhiteSpace( uiFeeOption.FeeLabel ) )
                            {
                                feeName = string.Format( "{0} ({1})", feeName, uiFeeOption.FeeLabel );
                            }

                            if ( dbFee.Id <= 0 )
                            {
                                registrantChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Fee" ).SetNewValue( feeName );
                            }

                            History.EvaluateChange( registrantChanges, feeName + " Quantity", dbFee.Quantity, uiFeeOption.Quantity );
                            dbFee.Quantity = uiFeeOption.Quantity;

                            History.EvaluateChange( registrantChanges, feeName + " Cost", dbFee.Cost, uiFeeOption.Cost );
                            dbFee.Cost = uiFeeOption.Cost;
                        }
                    }

                    rockContext.SaveChanges();
                    registrantInfo.Id = registrant.Id;

                    // Set any of the template's registrant attributes
                    registrant.LoadAttributes();
                    foreach ( var field in RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields
                            .Where( t =>
                                t.FieldSource == RegistrationFieldSource.RegistrantAttribute &&
                                t.AttributeId.HasValue ) ) )
                    {
                        // Find the registrant's value
                        var fieldValue = registrantInfo.FieldValues
                            .Where( f => f.Key == field.Id )
                            .Select( f => f.Value.FieldValue )
                            .FirstOrDefault();

                        if ( fieldValue != null )
                        {
                            var attribute = AttributeCache.Get( field.AttributeId.Value );
                            if ( attribute != null )
                            {
                                string originalValue = registrant.GetAttributeValue( attribute.Key );
                                string newValue = fieldValue.ToString();
                                registrant.SetAttributeValue( attribute.Key, fieldValue.ToString() );

                                // DateTime values must be stored in ISO8601 format as http://www.rockrms.com/Rock/Developer/BookContent/16/16#datetimeformatting
                                if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() ) ||
                                    attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() ) )
                                {
                                    DateTime aDateTime;
                                    if ( DateTime.TryParse( fieldValue.ToString(), out aDateTime ) )
                                    {
                                        newValue = aDateTime.ToString( "o" );
                                    }
                                }

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

                                    Helper.SaveAttributeValue( registrant, attribute, newValue, rockContext );
                                    History.EvaluateChange( registrantChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                                }
                            }
                        }
                    }

                    Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( Registration ),
                            Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                            registration.Id,
                            registrantChanges,
                            "Registrant: " + person.FullName,
                            null,
                            null,
                            true,
                            CurrentPersonAliasId ) );

                    // Clear this registrant's family guid so it's not updated again
                    registrantInfo.FamilyGuid = Guid.Empty;

                    // Save the signed document
                    try
                    {
                        if ( RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue && !string.IsNullOrWhiteSpace( registrantInfo.SignatureDocumentKey ) )
                        {
                            var document = new SignatureDocument();
                            document.SignatureDocumentTemplateId = RegistrationTemplate.RequiredSignatureDocumentTemplateId.Value;
                            document.DocumentKey = registrantInfo.SignatureDocumentKey;
                            document.Name = string.Format( "{0}_{1}", RegistrationInstanceState.Name.RemoveSpecialCharacters(), person.FullName.RemoveSpecialCharacters() );
                            document.AppliesToPersonAliasId = person.PrimaryAliasId;
                            document.AssignedToPersonAliasId = registrar.PrimaryAliasId;
                            document.SignedByPersonAliasId = registrar.PrimaryAliasId;
                            document.Status = SignatureDocumentStatus.Signed;
                            document.LastInviteDate = registrantInfo.SignatureDocumentLastSent;
                            document.LastStatusDate = registrantInfo.SignatureDocumentLastSent;
                            documentService.Add( document );
                            rockContext.SaveChanges();

                            var updateDocumentTxn = new Rock.Transactions.UpdateDigitalSignatureDocumentTransaction( document.Id );
                            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( updateDocumentTxn );
                        }
                    }
                    catch ( System.Exception ex )
                    {
                        ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
                    }
                }

                rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                using ( var newRockContext = new RockContext() )
                {
                    if ( newRegistration )
                    {
                        var newRegistrationService = new RegistrationService( newRockContext );
                        var savedRegistration = new RegistrationService( newRockContext ).Get( registration.Id );
                        if ( savedRegistration != null )
                        {
                            HistoryService.DeleteChanges( newRockContext, typeof( Registration ), savedRegistration.Id );

                            newRegistrationService.Delete( savedRegistration );
                            newRockContext.SaveChanges();
                        }
                    }
                }

                throw ex;
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
        private Person SavePerson( RockContext rockContext, Person person, Guid familyGuid, int? campusId, Location location, int adultRoleId, int childRoleId, Dictionary<Guid, int> multipleFamilyGroupIds, ref int? singleFamilyId )
        {
            int? familyId = null;

            if ( person.Id > 0 )
            {
                rockContext.SaveChanges();

                // Set the family guid for any other registrants that were selected to be in the same family
                var family = person.GetFamily( rockContext );
                if ( family != null )
                {
                    familyId = family.Id;
                    multipleFamilyGroupIds.AddOrIgnore( familyGuid, family.Id );
                    if ( !singleFamilyId.HasValue )
                    {
                        singleFamilyId = family.Id;
                    }
                }
            }
            else
            {
                // If we've created the family already for this registrant, add them to it
                if (
                        ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask && multipleFamilyGroupIds.ContainsKey( familyGuid ) ) ||
                        ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes && singleFamilyId.HasValue )
                    )
                {
                    // Add person to existing family
                    var age = person.Age;
                    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;

                    familyId = RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask ?
                        multipleFamilyGroupIds[familyGuid] :
                        singleFamilyId.Value;
                    PersonService.AddPersonToFamily( person, true, familyId.Value, familyRoleId, rockContext );
                }
                else
                {
                    // otherwise create a new family
                    // Create Person/Family
                    var familyGroup = PersonService.SaveNewPerson( person, rockContext, campusId, false );
                    if ( familyGroup != null )
                    {
                        familyId = familyGroup.Id;

                        // Store the family id for next person
                        multipleFamilyGroupIds.AddOrIgnore( familyGuid, familyGroup.Id );
                        if ( !singleFamilyId.HasValue )
                        {
                            singleFamilyId = familyGroup.Id;
                        }
                    }
                }
            }

            if ( familyId.HasValue && location != null )
            {
                var familyGroup = new GroupService( rockContext ).Get( familyId.Value );
                var existingLocation = new LocationService( rockContext ).Get(
                    location.Street1,
                    location.Street2,
                    location.City,
                    location.State,
                    location.PostalCode,
                    location.Country,
                    familyGroup,
                    true,
                    false );

                if ( existingLocation == null )
                {
                    var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                    if ( homeLocationType != null )
                    {
                        if ( familyGroup != null )
                        {
                            GroupService.AddNewGroupAddress(
                                rockContext,
                                familyGroup,
                                Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                                location.Street1,
                                location.Street2,
                                location.City,
                                location.State,
                                location.PostalCode,
                                location.Country,
                                true );
                        }
                    }
                }
            }

            return new PersonService( rockContext ).Get( person.Id );
        }

        /// <summary>
        /// Saves the phone.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="person">The person.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        /// <param name="changes">The changes.</param>
        private void SavePhone( object fieldValue, Person person, Guid phoneTypeGuid, History.HistoryChangeList changes )
        {
            var phoneNumber = fieldValue as PhoneNumber;
            if ( phoneNumber != null )
            {
                string cleanNumber = PhoneNumber.CleanNumber( phoneNumber.Number );
                if ( !string.IsNullOrWhiteSpace( cleanNumber ) )
                {
                    var numberType = DefinedValueCache.Get( phoneTypeGuid );
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
                        phone.Number = cleanNumber;

                        History.EvaluateChange(
                            changes,
                            string.Format( "{0} Phone", numberType.Value ),
                            oldPhoneNumber,
                            phoneNumber.NumberFormattedWithCountryCode );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the registrants to group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        private void AddRegistrantsToGroup( RockContext rockContext, Registration registration )
        {
            // If the registration instance linkage specified a group to add registrant to, add them if they're not already
            // part of that group
            if ( registration.GroupId.HasValue )
            {
                var groupService = new GroupService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );

                var group = groupService.Get( registration.GroupId.Value );
                if ( group != null )
                {
                    foreach ( var registrant in registration.Registrants.Where( r => !r.OnWaitList && r.PersonAliasId.HasValue ).ToList() )
                    {
                        var personAlias = personAliasService.Get( registrant.PersonAliasId.Value );
                        GroupMember groupMember = group.Members.Where( m => m.PersonId == personAlias.PersonId ).FirstOrDefault();
                        if ( groupMember == null )
                        {
                            groupMember = new GroupMember();
                            groupMember.GroupId = group.Id;
                            groupMember.PersonId = personAlias.PersonId;

                            if ( RegistrationTemplate.GroupTypeId.HasValue &&
                                RegistrationTemplate.GroupTypeId == group.GroupTypeId &&
                                RegistrationTemplate.GroupMemberRoleId.HasValue )
                            {
                                groupMember.GroupRoleId = RegistrationTemplate.GroupMemberRoleId.Value;
                            }
                            else
                            {
                                if ( group.GroupType.DefaultGroupRoleId.HasValue )
                                {
                                    groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;
                                }
                                else
                                {
                                    groupMember.GroupRoleId = group.GroupType.Roles.Select( r => r.Id ).FirstOrDefault();
                                }
                            }

                            groupMemberService.Add( groupMember );
                        }

                        groupMember.GroupMemberStatus = RegistrationTemplate.GroupMemberStatus;

                        rockContext.SaveChanges();

                        registrant.GroupMemberId = groupMember != null ? groupMember.Id : ( int? ) null;
                        rockContext.SaveChanges();

                        // Set any of the template's group member attributes
                        groupMember.LoadAttributes();

                        var registrantInfo = RegistrationState.Registrants.FirstOrDefault( r => r.Guid == registrant.Guid );
                        if ( registrantInfo != null )
                        {
                            foreach ( var field in RegistrationTemplate.Forms
                                .SelectMany( f => f.Fields
                                    .Where( t =>
                                        t.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                                        t.AttributeId.HasValue ) ) )
                            {
                                // Find the registrant's value
                                var fieldValue = registrantInfo.FieldValues
                                    .Where( f => f.Key == field.Id )
                                    .Select( f => f.Value.FieldValue )
                                    .FirstOrDefault();

                                if ( fieldValue != null )
                                {
                                    var attribute = AttributeCache.Get( field.AttributeId.Value );
                                    if ( attribute != null )
                                    {
                                        string originalValue = groupMember.GetAttributeValue( attribute.Key );
                                        string newValue = fieldValue.ToString();
                                        groupMember.SetAttributeValue( attribute.Key, fieldValue.ToString() );

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

                                            Helper.SaveAttributeValue( groupMember, attribute, newValue, rockContext );
                                        }
                                    }
                                }
                            }
                        }
                    }
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

            if ( !RegistrationInstanceState.AccountId.HasValue || RegistrationInstanceState.Account == null )
            {
                errorMessage = "There was a problem with the account configuration for this " + RegistrationTerm.ToLower();
                return false;
            }

            PaymentInfo paymentInfo = null;
            if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsId() ?? 0 ) > 0 )
            {
                var savedAccount = new FinancialPersonSavedAccountService( rockContext ).Get( rblSavedCC.SelectedValueAsId().Value );
                if ( savedAccount != null )
                {
                    paymentInfo = savedAccount.GetReferencePayment();
                    paymentInfo.Amount = RegistrationState.PaymentAmount ?? 0.0m;
                }
                else
                {
                    errorMessage = "There was a problem retrieving the saved account";
                    return false;
                }
            }
            else
            {
                var rgx = new System.Text.RegularExpressions.Regex( @"[^\d]" );
                string ccNum = rgx.Replace( txtCreditCard.Text, string.Empty );

                bool isValid = true;
                var errorMessages = new List<string>();
                if ( string.IsNullOrWhiteSpace( ccNum ) )
                {
                    errorMessages.Add( "Card Number is required" );
                    isValid = false;
                }

                if ( !mypExpiration.SelectedDate.HasValue )
                {
                    errorMessages.Add( "Card Expiration Date is required " );
                    isValid = false;
                }

                if ( string.IsNullOrWhiteSpace( txtCVV.Text ) )
                {
                    errorMessages.Add( "Card Security Code is required" );
                    isValid = false;
                }

                if ( !isValid )
                {
                    errorMessage = string.Format( "<ul><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
                    return isValid;
                }

                paymentInfo = GetCCPaymentInfo( gateway );
            }

            paymentInfo.Comment1 = string.Format( "{0} ({1})", RegistrationInstanceState.Name, RegistrationInstanceState.Account.GlCode );

            var transaction = gateway.Charge( RegistrationTemplate.FinancialGateway, paymentInfo, out errorMessage );

            return SaveTransaction( gateway, registration, transaction, paymentInfo, rockContext );
        }

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private bool SaveTransaction( GatewayComponent gateway, Registration registration, FinancialTransaction transaction, PaymentInfo paymentInfo, RockContext rockContext )
        {
            if ( transaction != null )
            {
                transaction.AuthorizedPersonAliasId = registration.PersonAliasId;
                transaction.TransactionDateTime = RockDateTime.Now;
                transaction.FinancialGatewayId = RegistrationTemplate.FinancialGatewayId;

                var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
                transaction.TransactionTypeValueId = txnType.Id;

                if ( transaction.FinancialPaymentDetail == null )
                {
                    transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
                }

                DefinedValueCache currencyType = null;
                DefinedValueCache creditCardType = null;

                if ( paymentInfo != null )
                {
                    transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );
                    currencyType = paymentInfo.CurrencyTypeValue;
                    creditCardType = paymentInfo.CreditCardTypeValue;
                }

                Guid sourceGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
                {
                    var source = DefinedValueCache.Get( sourceGuid );
                    if ( source != null )
                    {
                        transaction.SourceTypeValueId = source.Id;
                    }
                }

                transaction.Summary = registration.GetSummary( RegistrationInstanceState );

                var transactionDetail = new FinancialTransactionDetail();
                transactionDetail.Amount = RegistrationState.PaymentAmount ?? 0.0m;
                transactionDetail.AccountId = RegistrationInstanceState.AccountId.Value;
                transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
                transactionDetail.EntityId = registration.Id;
                transaction.TransactionDetails.Add( transactionDetail );

                var batchChanges = new History.HistoryChangeList();

                rockContext.WrapTransaction( () =>
                {
                    var batchService = new FinancialBatchService( rockContext );

                    // determine batch prefix
                    string batchPrefix = string.Empty;
                    if ( !string.IsNullOrWhiteSpace( RegistrationTemplate.BatchNamePrefix ) )
                    {
                        batchPrefix = RegistrationTemplate.BatchNamePrefix;
                    }
                    else
                    {
                        batchPrefix = GetAttributeValue( "BatchNamePrefix" );
                    }

                    // Get the batch
                    var batch = batchService.Get(
                        batchPrefix,
                        currencyType,
                        creditCardType,
                        transaction.TransactionDateTime.Value,
                        RegistrationTemplate.FinancialGateway.GetBatchTimeOffset() );

                    if ( batch.Id == 0 )
                    {
                        batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                        History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                        History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                        History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                        History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
                    }

                    decimal newControlAmount = batch.ControlAmount + transaction.TotalAmount;
                    History.EvaluateChange( batchChanges, "Control Amount", batch.ControlAmount.FormatAsCurrency(), newControlAmount.FormatAsCurrency() );
                    batch.ControlAmount = newControlAmount;

                    var financialTransactionService = new FinancialTransactionService( rockContext );

                    // If this is a new Batch, SaveChanges so that we can get the Batch.Id
                    if ( batch.Id == 0 )
                    {
                        rockContext.SaveChanges();
                    }

                    transaction.BatchId = batch.Id;

                    // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
                    financialTransactionService.Add( transaction );

                    rockContext.SaveChanges();
                } );

                if ( transaction.BatchId.HasValue )
                {
                    Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( FinancialBatch ),
                            Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                            transaction.BatchId.Value,
                            batchChanges,
                            true,
                            CurrentPersonAliasId ) );
                }

                var registrationChanges = new History.HistoryChangeList();
                registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Payment" ).SetNewValue( string.Format( "{0} payment", transaction.TotalAmount.FormatAsCurrency() ) );
                Task.Run( () =>
                    HistoryService.SaveChanges(
                        new RockContext(),
                        typeof( Registration ),
                        Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                        registration.Id,
                        registrationChanges,
                        true,
                        CurrentPersonAliasId ) );

                TransactionCode = transaction.TransactionCode;

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Processes the first step of a 3-step charge.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessStep1( out string errorMessage )
        {
            IThreeStepGatewayComponent gateway = null;
            if ( RegistrationTemplate != null && RegistrationTemplate.FinancialGateway != null )
            {
                gateway = RegistrationTemplate.FinancialGateway.GetGatewayComponent() as IThreeStepGatewayComponent;
            }

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( !RegistrationInstanceState.AccountId.HasValue || RegistrationInstanceState.Account == null )
            {
                errorMessage = "There was a problem with the account configuration for this " + RegistrationTerm.ToLower();
                return false;
            }

            PaymentInfo paymentInfo = null;
            if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsId() ?? 0 ) > 0 )
            {
                var rockContext = new RockContext();
                var savedAccount = new FinancialPersonSavedAccountService( rockContext ).Get( rblSavedCC.SelectedValueAsId().Value );
                if ( savedAccount != null )
                {
                    paymentInfo = savedAccount.GetReferencePayment();
                    paymentInfo.Amount = RegistrationState.PaymentAmount ?? 0.0m;
                }
                else
                {
                    errorMessage = "There was a problem retrieving the saved account";
                    return false;
                }
            }
            else
            {
                paymentInfo = new PaymentInfo();
                paymentInfo.Amount = RegistrationState.PaymentAmount ?? 0.0m;
                paymentInfo.Email = RegistrationState.ConfirmationEmail;

                paymentInfo.FirstName = RegistrationState.FirstName;
                paymentInfo.LastName = RegistrationState.LastName;
            }

            paymentInfo.Description = string.Format( "{0} ({1})", RegistrationInstanceState.Name, RegistrationInstanceState.Account.GlCode );
            paymentInfo.IPAddress = GetClientIpAddress();
            paymentInfo.AdditionalParameters = gateway.GetStep1Parameters( ResolveRockUrlIncludeRoot( "~/GatewayStep2Return.aspx" ) );

            var result = gateway.ChargeStep1( RegistrationTemplate.FinancialGateway, paymentInfo, out errorMessage );
            if ( string.IsNullOrWhiteSpace( errorMessage ) && !string.IsNullOrWhiteSpace( result ) )
            {
                hfStep2Url.Value = result;
            }

            return string.IsNullOrWhiteSpace( errorMessage );
        }

        /// <summary>
        /// Processes the third step of a 3-step charge.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="resultQueryString">The query string result from step 2.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessStep3( RockContext rockContext, Registration registration, string resultQueryString, out string errorMessage )
        {
            GatewayComponent gateway = null;
            IThreeStepGatewayComponent threeStepGatewayComponent = null;

            if ( RegistrationTemplate != null && RegistrationTemplate.FinancialGateway != null )
            {
                gateway = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                threeStepGatewayComponent = gateway as IThreeStepGatewayComponent;
            }

            if ( threeStepGatewayComponent == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( !RegistrationInstanceState.AccountId.HasValue || RegistrationInstanceState.Account == null )
            {
                errorMessage = "There was a problem with the account configuration for this " + RegistrationTerm.ToLower();
                return false;
            }

            var transaction = threeStepGatewayComponent.ChargeStep3( RegistrationTemplate.FinancialGateway, resultQueryString, out errorMessage );
            return SaveTransaction( gateway, registration, transaction, null, rockContext );
        }

        /// <summary>
        /// Creates a CreditCardPaymentInfo obj using data in the UI and RegistrationState
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <returns></returns>
        private CreditCardPaymentInfo GetCCPaymentInfo( GatewayComponent gateway )
        {
            var ccPaymentInfo = new CreditCardPaymentInfo( txtCreditCard.Text, txtCVV.Text, mypExpiration.SelectedDate != null ? mypExpiration.SelectedDate.Value : new DateTime() );

            ccPaymentInfo.NameOnCard = gateway != null && gateway.SplitNameOnCard ? txtCardFirstName.Text : txtCardName.Text;
            ccPaymentInfo.LastNameOnCard = txtCardLastName.Text;

            ccPaymentInfo.BillingStreet1 = acBillingAddress.Street1;
            ccPaymentInfo.BillingStreet2 = acBillingAddress.Street2;
            ccPaymentInfo.BillingCity = acBillingAddress.City;
            ccPaymentInfo.BillingState = acBillingAddress.State;
            ccPaymentInfo.BillingPostalCode = acBillingAddress.PostalCode;
            ccPaymentInfo.BillingCountry = acBillingAddress.Country;

            ccPaymentInfo.Amount = RegistrationState.PaymentAmount ?? 0.0m;
            ccPaymentInfo.Email = RegistrationState.ConfirmationEmail;

            ccPaymentInfo.FirstName = RegistrationState.FirstName;
            ccPaymentInfo.LastName = RegistrationState.LastName;

            return ccPaymentInfo;
        }

        #endregion

        #region Display Methods

        /// <summary>
        /// Shows the how many panel
        /// </summary>
        private void ShowHowMany()
        {
            string instructions = string.IsNullOrEmpty( RegistrationInstanceState.RegistrationInstructions ) ?
                                    RegistrationTemplate.RegistrationInstructions :
                                    RegistrationInstanceState.RegistrationInstructions;

            if ( !string.IsNullOrEmpty( instructions ) )
            {
                lInstructions.Text = string.Format( "<div class='text-left'>{0}</div>", instructions );
            }

            lRegistrantTerm.Text = RegistrantTerm.Pluralize().ToLower();

            // If this is an existing registration, go directly to the summary
            if ( !Page.IsPostBack && RegistrationState != null && RegistrationState.RegistrationId.HasValue && !PageParameter( START_AT_BEGINNING ).AsBoolean() )
            {
                // ShowSummary will set visibility on things like the lbSummaryPrev button, so we want to
                // call this before we might change that lbSummaryPrev button's visibility below.
                ShowSummary();

                // check if template does not allow updating the saved registration, if so hide the back button on the summary screen
                if ( !RegistrationTemplate.AllowExternalRegistrationUpdates )
                {
                    lbSummaryPrev.Visible = false;
                }
            }
            else
            {
                int max = MaxRegistrants;
                if ( !RegistrationTemplate.WaitListEnabled && RegistrationState.SlotsAvailable.HasValue && RegistrationState.SlotsAvailable.Value < max )
                {
                    max = RegistrationState.SlotsAvailable.Value;
                }

                if ( max > MinRegistrants )
                {
                    // If registration allows multiple registrants show the 'How Many' panel
                    numHowMany.Maximum = max;
                    numHowMany.Minimum = MinRegistrants;
                    numHowMany.Value = RegistrationState != null ? RegistrationState.RegistrantCount : 1;

                    ShowWaitingListNotice();

                    SetPanel( PanelIndex.PanelHowMany );
                }
                else
                {
                    // ... else skip to the registrant panel
                    CurrentRegistrantIndex = 0;
                    CurrentFormIndex = 0;

                    SetRegistrantState( MinRegistrants );
                    numHowMany.Value = MinRegistrants;
                    ShowRegistrationAttributesStart( true );
                }
            }
        }

        /// <summary>
        /// Shows the waiting list notice.
        /// </summary>
        private void ShowWaitingListNotice()
        {
            if ( RegistrationTemplate.WaitListEnabled )
            {
                nbWaitingList.Title = string.Format( "{0} Full", RegistrationTerm );

                if ( !RegistrationState.SlotsAvailable.HasValue || RegistrationState.SlotsAvailable.Value <= 0 )
                {
                    nbWaitingList.Text = string.Format( "<p>This {0} has reached its capacity. Complete the registration below to be added to the waitlist.</p>", RegistrationTerm );
                    nbWaitingList.Visible = true;
                }
                else
                {
                    if ( numHowMany.Value > RegistrationState.SlotsAvailable )
                    {
                        int slots = RegistrationState.SlotsAvailable.Value;
                        int wait = numHowMany.Value - slots;
                        nbWaitingList.Text = string.Format(
                            "<p>This {0} only has capacity for {1} more {2}. The first {3}{2} you add will be registered for {4}. The remaining {5}{6} will be added to the waitlist.",
                            RegistrationTerm.ToLower(),
                            slots,
                            RegistrantTerm.PluralizeIf( slots > 1 ).ToLower(),
                            slots > 1 ? slots.ToString() + " " : string.Empty,
                            RegistrationInstanceState.Name,
                            wait > 1 ? wait.ToString() + " " : string.Empty,
                            RegistrantTerm.PluralizeIf( wait > 1 ).ToLower() );
                        nbWaitingList.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Method to handle situation where a form may not have any fields to display for someone on the waiting list
        /// </summary>
        /// <param name="forward">if set to <c>true</c> [forward].</param>
        /// <param name="increment">if set to <c>true</c> [increment].</param>
        private void ShowRegistrant( bool forward, bool increment )
        {
            if ( forward )
            {
                do
                {
                    if ( increment )
                    {
                        CurrentFormIndex++;
                        if ( ( CurrentFormIndex >= FormCount && !SignInline ) || CurrentFormIndex >= FormCount + 1 )
                        {
                            CurrentRegistrantIndex++;
                            CurrentFormIndex = 0;
                        }
                    }
                    else
                    {
                        increment = true;
                    }
                } while ( CurrentRegistrantIndex < RegistrationState.RegistrantCount && !FormHasWaitFields() );

                if ( CurrentRegistrantIndex >= RegistrationState.RegistrantCount )
                {
                    ShowRegistrationAttributesEnd( true );
                }
                else
                {
                    ShowRegistrant();
                }
            }
            else
            {
                do
                {
                    if ( increment )
                    {
                        CurrentFormIndex--;
                        if ( CurrentFormIndex < 0 )
                        {
                            CurrentRegistrantIndex--;
                            CurrentFormIndex = FormCount - 1;
                        }
                    }
                    else
                    {
                        increment = true;
                    }
                } while ( CurrentRegistrantIndex >= 0 && !FormHasWaitFields() );

                if ( CurrentRegistrantIndex < 0 )
                {
                    ShowRegistrationAttributesStart( false );
                }
                else
                {
                    ShowRegistrant();
                }
            }
        }

        /// <summary>
        /// Checks if the registrant is on the waitlist, the template form is external, and the template is set to Show waitlist
        /// </summary>
        /// <returns>true or the value of ShowOnWaitlist</returns>
        private bool FormHasWaitFields()
        {
            if ( RegistrationTemplate != null && RegistrationState != null && RegistrationState.Registrants.Count > CurrentRegistrantIndex && FormCount > CurrentFormIndex )
            {
                var registrant = RegistrationState.Registrants[CurrentRegistrantIndex];
                if ( registrant.OnWaitList )
                {
                    var form = RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentFormIndex];
                    return form.Fields.Any( f => !f.IsInternal && f.ShowOnWaitlist );
                }
            }

            return true;
        }

        /// <summary>
        /// Shows the registrant panel
        /// </summary>
        private void ShowRegistrant()
        {
            if ( RegistrationState != null && RegistrationState.RegistrantCount > 0 )
            {
                int max = MaxRegistrants;
                if ( !RegistrationTemplate.WaitListEnabled && RegistrationState.SlotsAvailable.HasValue && RegistrationState.SlotsAvailable.Value < max )
                {
                    max = RegistrationState.SlotsAvailable.Value;
                }

                if ( CurrentRegistrantIndex == 0 && CurrentFormIndex == 0 && (
                        PageParameter( START_AT_BEGINNING ).AsBoolean() ||
                        RegistrationState.RegistrationId.HasValue ||
                        max <= MinRegistrants ) )
                {
                    lbRegistrantPrev.Visible = false;
                }
                else
                {
                    lbRegistrantPrev.Visible = true;
                }

                var registrant = RegistrationState.Registrants[CurrentRegistrantIndex];
                if ( registrant != null )
                {
                    string title = RegistrationState.RegistrantCount <= 1 ? RegistrantTerm :
                        ( CurrentRegistrantIndex + 1 ).ToOrdinalWords().Humanize( LetterCasing.Title ) + " " + RegistrantTerm;
                    if ( CurrentFormIndex > 0 )
                    {
                        title += " (cont)";
                    }

                    lRegistrantTitle.Text = title;

                    nbType.Visible = RegistrationState.RegistrantCount > RegistrationState.SlotsAvailable;
                    nbType.Text = registrant.OnWaitList ? string.Format( "This {0} will be on the waiting list", RegistrantTerm.ToLower() ) : string.Format( "This {0} will be fully registered.", RegistrantTerm.ToLower() );
                    nbType.NotificationBoxType = registrant.OnWaitList ? NotificationBoxType.Warning : NotificationBoxType.Success;

                    decimal currentStep = ( FormCount * CurrentRegistrantIndex ) + CurrentFormIndex + 1;
                    if ( this.RegistrationAttributeIdsBeforeRegistrants.Any() )
                    {
                        currentStep++;
                    }

                    PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
                    pnlRegistrantProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

                    if ( SignInline && CurrentFormIndex >= FormCount )
                    {
                        string registrantName = RegistrantTerm;
                        if ( RegistrationState != null && RegistrationState.RegistrantCount > CurrentRegistrantIndex )
                        {
                            registrantName = registrant.GetFirstName( RegistrationTemplate );
                        }

                        nbDigitalSignature.Heading = "Signature Required";
                        nbDigitalSignature.Text = string.Format(
                            "This {0} requires that you sign a {1} for each registrant, please follow the prompts below to digitally sign this document for {2}.",
                            RegistrationTemplate.RegistrationTerm,
                            RegistrationTemplate.RequiredSignatureDocumentTemplate.Name,
                            registrantName );

                        var errors = new List<string>();
                        string inviteLink = DigitalSignatureComponent.GetInviteLink( RegistrationTemplate.RequiredSignatureDocumentTemplate.ProviderTemplateKey, out errors );
                        if ( !string.IsNullOrWhiteSpace( inviteLink ) )
                        {
                            string returnUrl = ResolvePublicUrl( "~/Blocks/Event/DocumentReturn.html" );
                            hfRequiredDocumentLinkUrl.Value = string.Format( "{0}?redirect_uri={1}", inviteLink, returnUrl );
                        }
                        else
                        {
                            ShowError( "Digital Signature Error", string.Format( "An Error Occurred Trying to Get Document Link... <ul><li>{0}</li></ul>", errors.AsDelimited( "</li><li>" ) ) );
                            return;
                        }

                        pnlRegistrantFields.Visible = false;
                        pnlDigitalSignature.Visible = true;
                        lbRegistrantNext.Visible = false;
                    }
                    else
                    {
                        pnlRegistrantFields.Visible = true;
                        pnlDigitalSignature.Visible = false;
                        lbRegistrantNext.Visible = true;

                        ddlFamilyMembers.Items.Clear();
                        var preselectFamilyMember = RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes;

                        if ( CurrentFormIndex == 0 && RegistrationState != null && RegistrationTemplate.ShowCurrentFamilyMembers )
                        {
                            if ( registrant.Id <= 0 &&
                                CurrentFormIndex == 0 &&
                                RegistrationTemplate.RegistrantsSameFamily != RegistrantsSameFamily.No &&
                                RegistrationTemplate.ShowCurrentFamilyMembers &&
                                CurrentPerson != null )
                            {
                                var familyMembers = CurrentPerson.GetFamilyMembers( true )
                                    .Select( m => m.Person )
                                    .ToList();

                                for ( int i = 0; i < CurrentRegistrantIndex; i++ )
                                {
                                    int? personId = RegistrationState.Registrants[i].PersonId;
                                    if ( personId.HasValue )
                                    {
                                        foreach ( var familyMember in familyMembers.Where( p => p.Id == personId.Value ).ToList() )
                                        {
                                            familyMembers.Remove( familyMember );
                                        }
                                    }
                                }

                                if ( familyMembers.Any() )
                                {
                                    ddlFamilyMembers.Visible = true;
                                    ddlFamilyMembers.Items.Add( new ListItem() );

                                    foreach ( var familyMember in familyMembers )
                                    {
                                        ListItem listItem = new ListItem( familyMember.FullName, familyMember.Id.ToString() );
                                        listItem.Selected = familyMember.Id == registrant.PersonId && preselectFamilyMember;
                                        ddlFamilyMembers.Items.Add( listItem );
                                    }
                                }
                            }
                        }

                        pnlFamilyMembers.Visible = ddlFamilyMembers.Items.Count > 0;
                    }

                    SetPanel( PanelIndex.PanelRegistrant );
                }
            }
        }

        /// <summary>
        /// Shows the summary panel
        /// </summary>
        private void ShowSummary()
        {
            decimal currentStep = ( FormCount * RegistrationState.RegistrantCount ) + 1;
            if ( this.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                currentStep++;
            }

            if ( this.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                currentStep++;
            }

            PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
            pnlSummaryAndPaymentProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

            SetPanel( PanelIndex.PanelSummary );
        }

        /// <summary>
        /// Shows the payment panel.
        /// </summary>
        private void ShowPayment()
        {
            decimal currentStep = ( FormCount * RegistrationState.RegistrantCount ) + 2;

            if ( this.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                currentStep++;
            }

            if ( this.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                currentStep++;
            }

            PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
            pnlSummaryAndPaymentProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

            SetPanel( PanelIndex.PanelPayment );

            if ( ( rblSavedCC.Items.Count == 0 || ( rblSavedCC.SelectedValueAsInt() ?? 0 ) == 0 ) &&
                RegistrationTemplate != null &&
                RegistrationTemplate.FinancialGateway != null )
            {
                var component = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                if ( component != null )
                {
                    pnlPaymentInfo.Visible = true;
                    rblSavedCC.Visible = false;
                    divNewCard.Visible = true;
                    divNewCard.Style[HtmlTextWriterStyle.Display] = "block";

                    txtCardFirstName.Visible = component.PromptForNameOnCard( RegistrationTemplate.FinancialGateway ) && component.SplitNameOnCard;
                    txtCardLastName.Visible = component.PromptForNameOnCard( RegistrationTemplate.FinancialGateway ) && component.SplitNameOnCard;
                    txtCardName.Visible = component.PromptForNameOnCard( RegistrationTemplate.FinancialGateway ) && !component.SplitNameOnCard;

                    mypExpiration.MinimumYear = RockDateTime.Now.Year;
                    mypExpiration.MaximumYear = mypExpiration.MinimumYear + 15;

                    acBillingAddress.Visible = component.PromptForBillingAddress( RegistrationTemplate.FinancialGateway );
                }
                else
                {
                    pnlPaymentInfo.Visible = false;
                }
            }
            else
            {
                pnlPaymentInfo.Visible = false;
            }
        }

        /// <summary>
        /// Shows the success panel
        /// </summary>
        private void ShowSuccess( int registrationId )
        {
            decimal currentStep = ( FormCount * RegistrationState.RegistrantCount ) + ( Using3StepGateway ? 3 : 2 );

            if ( this.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                currentStep++;
            }

            if ( this.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                currentStep++;
            }

            PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
            pnlSuccessProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

            lSuccessTitle.Text = "Congratulations";
            lSuccess.Text = "You have successfully completed this registration.";

            try
            {
                var rockContext = new RockContext();
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
                        lSuccess.Text = "You have successfully completed this " + RegistrationTerm.ToLower();
                    }
                }

                if ( nbAmountPaid.Visible = true &&
                    nbAmountPaid.Text.AsDecimalOrNull().HasValue &&
                    nbAmountPaid.Text.AsDecimalOrNull().Value > 0.0M &&
                    ( rblSavedCC.Items.Count == 0 || ( rblSavedCC.SelectedValueAsId() ?? 0 ) == 0 ) )
                {
                    cbSaveAccount.Visible = true;
                    pnlSaveAccount.Visible = true;
                    txtSaveAccount.Visible = true;

                    // If current person does not have a login, have them create a username and password
                    phCreateLogin.Visible = !new UserLoginService( rockContext ).GetByPersonId( CurrentPersonId ).Any();
                }
                else
                {
                    pnlSaveAccount.Visible = false;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, Context, this.RockPage.PageId, this.RockPage.Site.Id, CurrentPersonAlias );
            }

            SetPanel( PanelIndex.PanelSuccess );
        }

        /// <summary>
        /// Creates the dynamic controls, and shows correct panel
        /// </summary>
        /// <param name="currentPanelIndex">Index of the current panel.</param>
        private void SetPanel( PanelIndex currentPanelIndex )
        {
            CurrentPanel = currentPanelIndex;

            if ( CurrentPanel == PanelIndex.PanelSummary && !( RegistrationState.RegistrationId.HasValue && RegistrationState.DiscountCode.IsNotNullOrWhiteSpace() ) )
            {
                AutoApplyDiscounts();
            }

            pnlRegistrarInfoPrompt.Visible = CurrentPanel == PanelIndex.PanelSummary;

            CreateDynamicControls( true );

            pnlHowMany.Visible = CurrentPanel <= 0;
            pnlRegistrationAttributesStart.Visible = CurrentPanel == PanelIndex.PanelRegistrationAttributesStart;
            pnlRegistrant.Visible = CurrentPanel == PanelIndex.PanelRegistrant;
            pnlRegistrationAttributesEnd.Visible = CurrentPanel == PanelIndex.PanelRegistrationAttributesEnd;

            pnlSummaryAndPayment.Visible = CurrentPanel == PanelIndex.PanelSummary || CurrentPanel == PanelIndex.PanelPayment;

            pnlRegistrantsReview.Visible = CurrentPanel == PanelIndex.PanelSummary;
            if ( CurrentPanel != PanelIndex.PanelSummary )
            {
                pnlCostAndFees.Visible = false;
            }

            lbSummaryPrev.Visible = CurrentPanel == PanelIndex.PanelSummary;
            lbSummaryNext.Visible = CurrentPanel == PanelIndex.PanelSummary;

            lbPaymentPrev.Visible = CurrentPanel == PanelIndex.PanelPayment;
            aStep2Submit.Visible = CurrentPanel == PanelIndex.PanelPayment;

            pnlSuccess.Visible = CurrentPanel == PanelIndex.PanelSuccess;

            lSummaryAndPaymentTitle.Text = ( CurrentPanel == PanelIndex.PanelSummary && RegistrationTemplate != null ) ? "Review " + RegistrationTemplate.RegistrationTerm : "Payment Method";
            lPaymentInfoTitle.Text = CurrentPanel == PanelIndex.PanelSummary ? "<h4>Payment Method</h4>" : string.Empty;
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
        /// Resolves a relative URL using the PublicApplicationRoot value but using the current request's scheme (http vs https).
        /// </summary>
        /// <param name="relativeUrl">The relative URL.</param>
        /// <returns></returns>
        private string ResolvePublicUrl( string relativeUrl )
        {
            string resolvedUrl = ResolveRockUrl( relativeUrl );

            string url = string.Format( "{0}://{1}", Request.Url.Scheme, Request.Url.Authority ).EnsureTrailingForwardslash() + resolvedUrl.RemoveLeadingForwardslash();

            try
            {
                var appRootUri = new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );
                if ( appRootUri != null )
                {
                    url = string.Format( "{0}://{1}", Request.Url.Scheme, appRootUri.Authority ).EnsureTrailingForwardslash() + resolvedUrl.RemoveLeadingForwardslash();
                }
            }
            catch { }

            return url;
        }

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            RockPage.AddScriptLink( "~/Scripts/jquery.creditCardTypeDetector.js" );

            var controlFamilyGuid = Guid.Empty;
            if ( CurrentPerson != null )
            {
                controlFamilyGuid = CurrentPerson.GetFamily().Guid;
            }

            string script = string.Format( @"
    // Adjust the label of 'is in the same family' based on value of first name entered
    $('input.js-first-name').change( function() {{
        var name = $(this).val();
        if ( name == null || name == '') {{
            name = '{23}';
        }}
        var $lbl = $('div.js-registration-same-family').find('label.control-label')
        $lbl.text( name + ' is in the same {22} as');
    }} );
    $('input.js-your-first-name').change( function() {{
        var name = $(this).val();
        if ( name == null || name == '') {{
            name = 'You are';
        }} else {{
            name += ' is';
        }}
        var $lbl = $('div.js-registration-same-family').find('label.control-label')
        $lbl.text( name + ' in the same {22} as');
    }} );

    // Adjust the Family Member dropdown when choosing same immediate family
    $('#{24}').on('change', function() {{
        var displaySetting = $('#{25}').css('display');
        if ( $(""input[id*='{24}']:checked"").val() == '{26}' && displaySetting == 'none' ) {{
            $( '#{25}').slideToggle();
        }}
        else if ( displaySetting == 'block' ) {{
            $('#{25}').slideToggle();
        }}
    }});

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
        $('#{4}').text( '{6}' + amountRemaining.toFixed(2) );
    }});

    // Detect credit card type
    $('.credit-card').creditCardTypeDetector({{ 'credit_card_logos': '.card-logos' }});

    // Toggle credit card display if saved card option is available
    $('div.radio-content').prev('div.radio-list').find('input:radio').unbind('click').on('click', function () {{
        $content = $(this).parents('div.radio-list:first').next('.radio-content');
        var radioDisplay = $content.css('display');
        if ($(this).val() == 0 && radioDisplay == 'none') {{
            $content.slideToggle();
        }}
        else if ($(this).val() != 0 && radioDisplay != 'none') {{
            $content.slideToggle();
        }}
    }});

    // Hide or show a div based on selection of checkbox
    $('input:checkbox.toggle-input').unbind('click').on('click', function () {{
        $(this).parents('.checkbox').next('.toggle-content').slideToggle();
    }});

    if ( $('#{5}').val() == 'true' ) {{
        setTimeout('window.scrollTo(0,0)',0);
        $('#{5}').val('')
    }}

    $('#aStep2Submit').on('click', function(e) {{
        e.preventDefault();
        if (typeof (Page_ClientValidate) == 'function') {{
            if (Page_IsValid && Page_ClientValidate('{10}') ) {{
                $(this).prop('disabled', true);
                $('#updateProgress').show();
                var src = $('#{7}').val();
                var $form = $('#iframeStep2').contents().find('#Step2Form');

                $form.find('.js-cc-first-name').val( $('#{16}').val() );
                $form.find('.js-cc-last-name').val( $('#{17}').val() );
                $form.find('.js-cc-full-name').val( $('#{18}').val() );

                $form.find('.js-cc-number').val( $('#{11}').val() );
                var mm = $('#{12}_monthDropDownList').val();
                var yy = $('#{12}_yearDropDownList_').val();
                mm = mm.length == 1 ? '0' + mm : mm;
                yy = yy.length == 4 ? yy.substring(2,4) : yy;
                $form.find('.js-cc-expiration').val( mm + yy );
                $form.find('.js-cc-cvv').val( $('#{13}').val() );

                $form.find('.js-billing-address1').val( $('#{15}_tbStreet1').val() );
                $form.find('.js-billing-city').val( $('#{15}_tbCity').val() );

                if ( $('#{15}_ddlState').length ) {{
                    $form.find('.js-billing-state').val( $('#{15}_ddlState').val() );
                }} else {{
                    $form.find('.js-billing-state').val( $('#{15}_tbState').val() );
                }}
                $form.find('.js-billing-postal').val( $('#{15}_tbPostalCode').val() );
                $form.find('.js-billing-country').val( $('#{15}_ddlCountry').val() );

                $form.attr('action', src );
                $form.submit();
            }}
        }}
    }});

    // Evaluates the current url whenever the iframe is loaded and if it includes a qrystring parameter
    // The qry parameter value is saved to a hidden field and a post back is performed
    $('#iframeStep2').on('load', function(e) {{
        var location = this.contentWindow.location;
        var qryString = this.contentWindow.location.search;
        if ( qryString && qryString != '' && qryString.startsWith('?token-id') ) {{
            $('#{8}').val(qryString);
            window.location = ""javascript:{9}"";
        }} else {{
            if ( $('#{14}').val() == 'true' ) {{
                $('#updateProgress').show();
                var src = $('#{7}').val();
                var $form = $('#iframeStep2').contents().find('#Step2Form');
                $form.attr('action', src );
                $form.submit();
            }}
        }}
    }});

    // Evaluates the current url whenever the iframe is loaded and if it includes a qrystring parameter
    // The qry parameter value is saved to a hidden field and a post back is performed
    $('#iframeRequiredDocument').on('load', function(e) {{
        var location = this.contentWindow.location;
        try {{
            var qryString = this.contentWindow.location.search;
            if ( qryString && qryString != '' && qryString.startsWith('?document_id') ) {{
                $('#{19}').val(qryString);
                window.location = ""javascript:{20}"";
            }}
        }}
        catch (e) {{
            console.log(e.message);
        }}
    }});

    if ($('#{21}').val() != '' ) {{
        $('#iframeRequiredDocument').attr('src', $('#{21}').val() );
    }}

", nbAmountPaid.ClientID                 // {0}
            , hfTotalCost.ClientID                   // {1}
            , hfMinimumDue.ClientID                  // {2}
            , hfPreviouslyPaid.ClientID              // {3}
            , lRemainingDue.ClientID                 // {4}
            , hfTriggerScroll.ClientID               // {5}
            , GlobalAttributesCache.Value( "CurrencySymbol" ) // {6}
            , hfStep2Url.ClientID                    // {7}
            , hfStep2ReturnQueryString.ClientID      // {8}
            , this.Page.ClientScript.GetPostBackEventReference( lbStep2Return, "" ) // {9}
            , this.BlockValidationGroup              // {10}
            , txtCreditCard.ClientID                 // {11}
            , mypExpiration.ClientID                 // {12}
            , txtCVV.ClientID                        // {13}
            , hfStep2AutoSubmit.ClientID             // {14}
            , acBillingAddress.ClientID              // {15}
            , txtCardFirstName.ClientID              // {16}
            , txtCardLastName.ClientID               // {17}
            , txtCardName.ClientID                  // {18}
            , hfRequiredDocumentQueryString.ClientID // {19}
            , this.Page.ClientScript.GetPostBackEventReference( lbRequiredDocumentNext, "" ) // {20}
            , hfRequiredDocumentLinkUrl.ClientID     // {21}
            , GetAttributeValue( "FamilyTerm" )      // {22}
            , RegistrantTerm                         // {23}
            , rblFamilyOptions.ClientID              // {24}
            , pnlFamilyMembers.ClientID              // {25}
            , controlFamilyGuid                      // {26}
);

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "registrationEntry", script, true );

            if ( Using3StepGateway )
            {
                string submitScript = string.Format(
                    @"
    $('#{0}').val('');
    $('#{1}_monthDropDownList').val('');
    $('#{1}_yearDropDownList_').val('');
    $('#{2}').val('');
",
                txtCreditCard.ClientID,     // {0}
                mypExpiration.ClientID,     // {1}
                txtCVV.ClientID );             // {2}

                ScriptManager.RegisterOnSubmitStatement( Page, Page.GetType(), "clearCCFields", submitScript );
            }
        }

        #endregion

        #region Dynamic Control Methods

        /// <summary>
        /// Creates the dynamic controls for current panel
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateDynamicControls( bool setValues )
        {
            switch ( CurrentPanel )
            {
                case PanelIndex.PanelRegistrant:
                    if ( CurrentFormIndex <= FormCount )
                    {
                        CreateRegistrantControls( setValues );
                    }

                    break;
                case PanelIndex.PanelSummary:
                    CreateSummaryControls( setValues );
                    break;
            }
        }

        /// <summary>
        /// Parses the dynamic controls based on the CurrentPanel
        /// </summary>
        private void ParseDynamicControls()
        {
            switch ( CurrentPanel )
            {
                case PanelIndex.PanelRegistrant:
                    if ( CurrentFormIndex < FormCount )
                    {
                        ParseRegistrantControls();
                        decimal currentStep = ( FormCount * CurrentRegistrantIndex ) + CurrentFormIndex + 1;
                        PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
                    }

                    break;
                case PanelIndex.PanelSummary:
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
                // Get the current and previous registrant ( first is used when a field has the 'IsSharedValue' property )
                // so that current registrant can use the first registrant's value
                RegistrantInfo registrant = null;
                RegistrantInfo firstRegistrant = null;
                var preselectCurrentPerson = RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes;

                if ( RegistrationState != null && RegistrationState.RegistrantCount >= CurrentRegistrantIndex )
                {
                    registrant = RegistrationState.Registrants[CurrentRegistrantIndex];
                    // Check to see if option for asking about family should be displayed
                    if ( CurrentFormIndex == 0 && RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask )
                    {
                        var familyOptions = RegistrationState.GetFamilyOptions( RegistrationTemplate, CurrentRegistrantIndex );
                        if ( CurrentRegistrantIndex == 0 && CurrentPerson != null )
                        {
                            // GetFamilyOptions ignores the first registrant by default, so add it manually when set to Ask
                            familyOptions.Add( CurrentPerson.GetFamily().Guid, CurrentPerson.FullName );
                        }

                        if ( familyOptions.Any() )
                        {
                            familyOptions.Add(
                                familyOptions.ContainsKey( registrant.FamilyGuid ) ?
                                Guid.NewGuid() :
                                registrant.FamilyGuid.Equals( Guid.Empty ) ? Guid.NewGuid() : registrant.FamilyGuid,
                                "None of the above" );
                            rblFamilyOptions.DataSource = familyOptions;
                            rblFamilyOptions.DataBind();
                            pnlFamilyOptions.Visible = true;
                        }
                        else
                        {
                            pnlFamilyOptions.Visible = false;
                        }
                    }
                    else
                    {
                        pnlFamilyOptions.Visible = false;
                    }

                    if ( setValues )
                    {
                        if ( CurrentRegistrantIndex > 0 )
                        {
                            firstRegistrant = RegistrationState.Registrants[0];
                        }

                        rblFamilyOptions.SetValue( registrant.FamilyGuid.ToString() );
                    }
                }

                var familyMemberSelected = registrant.Id <= 0 && registrant.PersonId.HasValue && RegistrationTemplate.ShowCurrentFamilyMembers;

                var form = RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentFormIndex];
                foreach ( var field in form.Fields
                    .Where( f =>
                        !f.IsInternal &&
                        ( !registrant.OnWaitList || f.ShowOnWaitlist ) )
                    .OrderBy( f => f.Order ) )
                {
                    object value = null;
                    if ( registrant != null && registrant.FieldValues.ContainsKey( field.Id ) )
                    {
                        value = registrant.FieldValues[field.Id].FieldValue;
                    }

                    if ( value == null && field.IsSharedValue && firstRegistrant != null && firstRegistrant.FieldValues.ContainsKey( field.Id ) )
                    {
                        value = firstRegistrant.FieldValues[field.Id].FieldValue;
                    }

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        CreatePersonField( field, setValues, value, familyMemberSelected, BlockValidationGroup, phRegistrantControls );
                    }
                    else
                    {
                        CreateAttributeField( form, field, setValues, value, GetAttributeValue( "ShowFieldDescriptions" ).AsBoolean(), BlockValidationGroup, phRegistrantControls );
                    }
                }

                FieldVisibilityWrapper.ApplyFieldVisibilityRules( phRegistrantControls );

                // If the current form, is the last one, add any fee controls
                if ( FormCount - 1 == CurrentFormIndex && !registrant.OnWaitList )
                {
                    List<RegistrantInfo> otherRegistrants = RegistrationState.Registrants.Where( a => a != registrant ).ToList();

                    foreach ( var fee in RegistrationTemplate.Fees.Where( f => f.IsActive == true ).OrderBy( o => o.Order ) )
                    {
                        var feeValues = new List<FeeInfo>();
                        if ( registrant != null && registrant.FeeValues.ContainsKey( fee.Id ) )
                        {
                            feeValues = registrant.FeeValues[fee.Id];
                        }

                        fee.AddFeeControl( phFees, this.RegistrationInstanceState, setValues, feeValues, otherRegistrants );
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
        private static void CreatePersonField( RegistrationTemplateFormField field, bool setValue, object fieldValue, bool familyMemberSelected, string validationGroup, Control parentControl )
        {
            Control personFieldControl = field.GetPersonControl( setValue, fieldValue, familyMemberSelected, validationGroup );

            if ( personFieldControl != null )
            {
                if ( !string.IsNullOrWhiteSpace( field.PreText ) )
                {
                    parentControl.Controls.Add( new LiteralControl( field.PreText ) );
                }

                parentControl.Controls.Add( personFieldControl );

                if ( !string.IsNullOrWhiteSpace( field.PostText ) )
                {
                    parentControl.Controls.Add( new LiteralControl( field.PostText ) );
                }
            }
        }

        /// <summary>
        /// Creates the attribute field.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="field">The field.</param>
        /// <param name="setValue">if set to <c>true</c> [set value].</param>
        /// <param name="fieldValue">The field value.</param>
        private static void CreateAttributeField( RegistrationTemplateForm form, RegistrationTemplateFormField field, bool setValue, object fieldValue, bool showFieldDescriptions, string validationGroup, Control parentControl )
        {
            if ( field.AttributeId.HasValue )
            {
                var attribute = AttributeCache.Get( field.AttributeId.Value );
                string value = setValue && fieldValue != null ? fieldValue.ToString() : null;

                if ( ( setValue && value == null ) || ( value.IsNullOrWhiteSpace() && field.IsRequired == true ) )
                {
                    // If the value was not set already, or if it is required and currently empty then use the default
                    // Intentionally leaving the possibility of saving an empty string as the value for non-required fields.
                    value = attribute.DefaultValue;
                }

                string helpText = showFieldDescriptions ? field.Attribute.Description : string.Empty;
                FieldVisibilityWrapper fieldVisibilityWrapper = new FieldVisibilityWrapper
                {
                    ID = "_fieldVisibilityWrapper_attribute_" + attribute.Id.ToString(),
                    AttributeId = attribute.Id,
                    FieldVisibilityRules = field.FieldVisibilityRules
                };

                fieldVisibilityWrapper.EditValueUpdated += ( object sender, FieldVisibilityWrapper.FieldEventArgs args ) =>
                {
                    FieldVisibilityWrapper.ApplyFieldVisibilityRules( parentControl );
                };

                parentControl.Controls.Add( fieldVisibilityWrapper );

                if ( !string.IsNullOrWhiteSpace( field.PreText ) )
                {
                    fieldVisibilityWrapper.Controls.Add( new LiteralControl( field.PreText ) );
                }

                var editControl = attribute.AddControl( fieldVisibilityWrapper.Controls, value, validationGroup, setValue, true, field.IsRequired, null, helpText );
                fieldVisibilityWrapper.EditControl = editControl;

                if ( !string.IsNullOrWhiteSpace( field.PostText ) )
                {
                    fieldVisibilityWrapper.Controls.Add( new LiteralControl( field.PostText ) );
                }

                bool hasDependantVisibilityRule = form.Fields.Any( a => a.FieldVisibilityRules.RuleList.Any( r => r.ComparedToAttributeGuid == attribute.Guid ) );

                if ( hasDependantVisibilityRule && attribute.FieldType.Field.HasChangeHandler( editControl ) )
                {
                    attribute.FieldType.Field.AddChangeHandler( editControl, () =>
                     {
                         fieldVisibilityWrapper.TriggerEditValueUpdated( editControl, new FieldVisibilityWrapper.FieldEventArgs( attribute, editControl ) );
                     } );
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
                foreach ( var field in form.Fields
                    .Where( f =>
                        !f.IsInternal &&
                        ( !registrant.OnWaitList || f.ShowOnWaitlist ) )
                    .OrderBy( f => f.Order ) )
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
                        registrant.FieldValues.AddOrReplace( field.Id, new FieldValueObject( field, value ) );
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
                        List<FeeInfo> feeValues = fee.GetFeeInfoFromControls( phFees );
                        if ( fee != null )
                        {
                            registrant.FeeValues.AddOrReplace( fee.Id, feeValues );
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
                    var tbFirstName = phRegistrantControls.FindControl( "tbFirstName" ) as RockTextBox;
                    string firstName = tbFirstName != null ? tbFirstName.Text : null;
                    return string.IsNullOrWhiteSpace( firstName ) ? null : firstName;

                case RegistrationPersonFieldType.LastName:
                    var tbLastName = phRegistrantControls.FindControl( "tbLastName" ) as RockTextBox;
                    string lastName = tbLastName != null ? tbLastName.Text : null;
                    return string.IsNullOrWhiteSpace( lastName ) ? null : lastName;

                case RegistrationPersonFieldType.MiddleName:
                    var tbMiddleName = phRegistrantControls.FindControl( "tbMiddleName" ) as RockTextBox;
                    string middleName = tbMiddleName != null ? tbMiddleName.Text : null;
                    return string.IsNullOrWhiteSpace( middleName ) ? null : middleName;

                case RegistrationPersonFieldType.Campus:
                    var cpHomeCampus = phRegistrantControls.FindControl( "cpHomeCampus" ) as CampusPicker;
                    return cpHomeCampus != null ? cpHomeCampus.SelectedCampusId : null;

                case RegistrationPersonFieldType.Address:
                    var location = new Location();
                    var acAddress = phRegistrantControls.FindControl( "acAddress" ) as AddressControl;
                    if ( acAddress != null )
                    {
                        acAddress.GetValues( location );
                        return location;
                    }

                    break;

                case RegistrationPersonFieldType.Email:
                    var tbEmail = phRegistrantControls.FindControl( "tbEmail" ) as EmailBox;
                    string email = tbEmail != null ? tbEmail.Text : null;
                    return string.IsNullOrWhiteSpace( email ) ? null : email;

                case RegistrationPersonFieldType.Birthdate:
                    var bpBirthday = phRegistrantControls.FindControl( "bpBirthday" ) as BirthdayPicker;
                    return bpBirthday != null ? bpBirthday.SelectedDate : null;

                case RegistrationPersonFieldType.Grade:
                    var gpGrade = phRegistrantControls.FindControl( "gpGrade" ) as GradePicker;
                    return gpGrade != null ? Person.GraduationYearFromGradeOffset( gpGrade.SelectedValueAsInt( false ) ) : null;

                case RegistrationPersonFieldType.Gender:
                    var ddlGender = phRegistrantControls.FindControl( "ddlGender" ) as RockDropDownList;
                    return ddlGender != null ? ddlGender.SelectedValueAsInt() : null;

                case RegistrationPersonFieldType.MaritalStatus:
                    var dvpMaritalStatus = phRegistrantControls.FindControl( "dvpMaritalStatus" ) as RockDropDownList;
                    return dvpMaritalStatus != null ? dvpMaritalStatus.SelectedValueAsInt() : null;

                case RegistrationPersonFieldType.AnniversaryDate:
                    var dppAnniversaryDate = phRegistrantControls.FindControl( "dppAnniversaryDate" ) as DatePartsPicker;
                    return dppAnniversaryDate != null ? dppAnniversaryDate.SelectedDate : null;

                case RegistrationPersonFieldType.MobilePhone:
                    var mobilePhoneNumber = new PhoneNumber();
                    var ppMobile = phRegistrantControls.FindControl( "ppMobile" ) as PhoneNumberBox;
                    if ( ppMobile != null )
                    {
                        mobilePhoneNumber.CountryCode = PhoneNumber.CleanNumber( ppMobile.CountryCode );
                        mobilePhoneNumber.Number = PhoneNumber.CleanNumber( ppMobile.Number );
                        return mobilePhoneNumber;
                    }

                    break;

                case RegistrationPersonFieldType.HomePhone:
                    var homePhoneNumber = new PhoneNumber();
                    var ppHome = phRegistrantControls.FindControl( "ppHome" ) as PhoneNumberBox;
                    if ( ppHome != null )
                    {
                        homePhoneNumber.CountryCode = PhoneNumber.CleanNumber( ppHome.CountryCode );
                        homePhoneNumber.Number = PhoneNumber.CleanNumber( ppHome.Number );
                        return homePhoneNumber;
                    }

                    break;

                case RegistrationPersonFieldType.WorkPhone:
                    var workPhoneNumber = new PhoneNumber();
                    var ppWork = phRegistrantControls.FindControl( "ppWork" ) as PhoneNumberBox;
                    if ( ppWork != null )
                    {
                        workPhoneNumber.CountryCode = PhoneNumber.CleanNumber( ppWork.CountryCode );
                        workPhoneNumber.Number = PhoneNumber.CleanNumber( ppWork.Number );
                        return workPhoneNumber;
                    }

                    break;

                case RegistrationPersonFieldType.ConnectionStatus:
                    var dvpConnectionStatus = phRegistrantControls.FindControl( "dvpConnectionStatus" ) as RockDropDownList;
                    return dvpConnectionStatus != null ? dvpConnectionStatus.SelectedValueAsInt() : null;
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
                var attribute = AttributeCache.Get( field.AttributeId.Value );
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
        /// Sets the registrant fields.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        private void SetRegistrantFields( int? personId )
        {
            if ( RegistrationState != null && RegistrationState.Registrants.Count > CurrentRegistrantIndex )
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrant = RegistrationState.Registrants[CurrentRegistrantIndex];
                    if ( registrant != null )
                    {
                        Person person = null;
                        Group family = null;

                        if ( personId.HasValue )
                        {
                            person = new PersonService( rockContext ).Get( personId.Value );
                        }

                        if ( person != null )
                        {
                            registrant.PersonId = person.Id;
                            registrant.PersonName = person.FullName;
                            family = person.GetFamily( rockContext );
                        }
                        else
                        {
                            registrant.PersonId = null;
                            registrant.PersonName = string.Empty;
                        }

                        foreach ( var field in RegistrationTemplate.Forms
                            .SelectMany( f => f.Fields ) )
                        {
                            object dbValue = null;

                            if ( field.ShowCurrentValue ||
                                ( ( field.PersonFieldType == RegistrationPersonFieldType.FirstName ||
                                field.PersonFieldType == RegistrationPersonFieldType.LastName ) &&
                                field.FieldSource == RegistrationFieldSource.PersonField ) )
                            {
                                dbValue = registrant.GetRegistrantValue( null, person, family, field, rockContext );
                            }

                            if ( dbValue != null )
                            {
                                registrant.FieldValues.AddOrReplace( field.Id, new FieldValueObject( field, dbValue ) );
                            }
                            else
                            {
                                registrant.FieldValues.Remove( field.Id );
                            }
                        }
                    }
                }
            }

            CreateRegistrantControls( true );
        }

        #endregion

        #region Summary/Payment Controls

        /// <summary>
        /// Creates the summary controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateSummaryControls( bool setValues )
        {
            lRegistrationTermPrompt.Text = RegistrationTerm;
            lRegistrationTermLoggedInPerson.Text = RegistrationTerm;
            lDiscountCodeLabel.Text = DiscountCodeTerm;

            if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask )
            {
                var familyOptions = RegistrationState.GetFamilyOptions( RegistrationTemplate, RegistrationState.RegistrantCount );
                if ( familyOptions.Any() )
                {
                    // previous family selections are always null after postback, so default to anyone in the same family
                    var selectedGuid = CurrentPerson != null ? CurrentPerson.GetFamily().Guid : rblRegistrarFamilyOptions.SelectedValueAsGuid();

                    familyOptions.Add(
                        familyOptions.ContainsKey( RegistrationState.FamilyGuid ) ?
                        Guid.NewGuid() :
                        RegistrationState.FamilyGuid.Equals( Guid.Empty ) ? Guid.NewGuid() : RegistrationState.FamilyGuid,
                        "None" );
                    rblRegistrarFamilyOptions.DataSource = familyOptions;
                    rblRegistrarFamilyOptions.DataBind();

                    if ( selectedGuid.HasValue )
                    {
                        rblRegistrarFamilyOptions.SetValue( selectedGuid );
                    }

                    pnlRegistrarFamilyOptions.Visible = true;
                }
                else
                {
                    pnlRegistrarFamilyOptions.Visible = false;
                }
            }
            else
            {
                pnlRegistrarFamilyOptions.Visible = false;
            }

            if ( setValues && RegistrationState != null && RegistrationInstanceState != null )
            {
                lbSummaryNext.Text = "Finish";

                // check to see if the registrar info should be auto filled by the first registrant
                if ( RegistrationTemplate.RegistrarOption == RegistrarOption.UseFirstRegistrant ||
                     RegistrationTemplate.RegistrarOption == RegistrarOption.PrefillFirstRegistrant )
                {
                    var firstRegistrant = RegistrationState.Registrants.FirstOrDefault();

                    tbYourFirstName.Text = firstRegistrant.GetFirstName( RegistrationTemplate );
                    tbYourLastName.Text = firstRegistrant.GetLastName( RegistrationTemplate );
                    tbConfirmationEmail.Text = firstRegistrant.GetEmail( RegistrationTemplate );

                    // if we have all of the required info for the registrar then hide the panel
                    if ( !string.IsNullOrWhiteSpace( tbYourFirstName.Text ) &&
                         !string.IsNullOrWhiteSpace( tbYourLastName.Text ) &&
                         !string.IsNullOrWhiteSpace( tbConfirmationEmail.Text ) &&
                         RegistrationTemplate.RegistrarOption == RegistrarOption.UseFirstRegistrant )
                    {
                        pnlRegistrarInfoPrompt.Visible = false;
                    }

                    // set the registrar to be in the same family as the first registrant
                    if ( RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask )
                    {
                        rblRegistrarFamilyOptions.SetValue( firstRegistrant.FamilyGuid.ToString() );
                    }
                }
                // Check to see if this is an existing registration or information has already been entered
                else
                {
                    if ( RegistrationTemplate.RegistrarOption == RegistrarOption.UseLoggedInPerson && CurrentPerson != null )
                    {
                        // If UseLoggedInPerson is enabled, only prompt for Email, and only if it is isn't known
                        pnlRegistrarInfoPrompt.Visible = false;
                        pnlRegistrarInfoUseLoggedInPerson.Visible = true;
                        lUseLoggedInPersonFirstName.Text = CurrentPerson.NickName;
                        lUseLoggedInPersonLastName.Text = CurrentPerson.LastName;
                        lUseLoggedInPersonEmail.Text = CurrentPerson.Email;
                        tbUseLoggedInPersonEmail.Text = CurrentPerson.Email;
                        lUseLoggedInPersonEmail.Visible = !CurrentPerson.Email.IsNullOrWhiteSpace();
                        tbUseLoggedInPersonEmail.Visible = CurrentPerson.Email.IsNullOrWhiteSpace();
                    }
                    else
                    {
                        if ( RegistrationState.RegistrationId.HasValue ||
                          !string.IsNullOrWhiteSpace( RegistrationState.FirstName ) ||
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
                            if ( CurrentPerson != null )
                            {
                                tbYourFirstName.Text = CurrentPerson.NickName;
                                tbYourLastName.Text = CurrentPerson.LastName;
                                tbConfirmationEmail.Text = CurrentPerson.Email;
                            }
                            else
                            {
                                tbYourFirstName.Text = string.Empty;
                                tbYourLastName.Text = string.Empty;
                                tbConfirmationEmail.Text = string.Empty;
                            }
                        }
                    }
                }

                rblRegistrarFamilyOptions.Label = string.IsNullOrWhiteSpace( tbYourFirstName.Text ) ?
                    "You are in the same " + GetAttributeValue( "FamilyTerm" ) + " as" :
                    tbYourFirstName.Text + " is in the same " + GetAttributeValue( "FamilyTerm" ) + " as";

                cbUpdateEmail.Visible = CurrentPerson != null && !string.IsNullOrWhiteSpace( CurrentPerson.Email ) && !GetAttributeValue( "ForceEmailUpdate" ).AsBoolean();
                if ( CurrentPerson != null && GetAttributeValue( "ForceEmailUpdate" ).AsBoolean() )
                {
                    lUpdateEmailWarning.Visible = true;
                }

                // Build Discount info if template has discounts and this is a new registration
                if ( RegistrationTemplate != null
                    && RegistrationTemplate.Discounts.Any()
                    && !RegistrationState.RegistrationId.HasValue )
                {
                    divDiscountCode.Visible = true;

                    string discountCode = RegistrationState.DiscountCode;
                    if ( !string.IsNullOrWhiteSpace( discountCode ) )
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
                    tbDiscountCode.Text = RegistrationState.DiscountCode;
                }

                decimal? minimumInitialPaymentPerRegistrant = RegistrationTemplate.MinimumInitialPayment;
                if ( RegistrationTemplate.SetCostOnInstance ?? false )
                {
                    minimumInitialPaymentPerRegistrant = RegistrationInstanceState.MinimumInitialPayment;
                }

                decimal? defaultPaymentAmountPerRegistrant = RegistrationTemplate.DefaultPayment;
                if ( RegistrationTemplate.SetCostOnInstance ?? false )
                {
                    defaultPaymentAmountPerRegistrant = RegistrationInstanceState.DefaultPayment;
                }

                // Get the cost/fee summary
                var costs = new List<RegistrationCostSummaryInfo>();
                foreach ( var registrant in RegistrationState.Registrants )
                {
                    if ( registrant.Cost > 0 )
                    {
                        var costSummary = new RegistrationCostSummaryInfo();
                        costSummary.Type = RegistrationCostSummaryType.Cost;
                        costSummary.Description = string.Format(
                            "{0} {1}",
                            registrant.GetFirstName( RegistrationTemplate ),
                            registrant.GetLastName( RegistrationTemplate ) );

                        if ( registrant.OnWaitList )
                        {
                            costSummary.Description += " (Waiting List)";
                            costSummary.Cost = 0.0M;
                            costSummary.DiscountedCost = 0.0M;
                            costSummary.MinPayment = 0.0M;
                            costSummary.DefaultPayment = 0.0M;
                        }
                        else
                        {
                            costSummary.Cost = registrant.Cost;
                            if ( RegistrationState.DiscountPercentage > 0.0m && registrant.DiscountApplies )
                            {
                                if ( RegistrationState.DiscountPercentage >= 1.0m )
                                {
                                    costSummary.DiscountedCost = 0.0m;
                                }
                                else
                                {
                                    costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * RegistrationState.DiscountPercentage );
                                }
                            }
                            else
                            {
                                costSummary.DiscountedCost = costSummary.Cost;
                            }

                            // If registration allows a minimum payment calculate that amount, otherwise use the discounted amount as minimum
                            costSummary.MinPayment = minimumInitialPaymentPerRegistrant.HasValue ? minimumInitialPaymentPerRegistrant.Value : costSummary.DiscountedCost;
                            costSummary.DefaultPayment = defaultPaymentAmountPerRegistrant;
                        }

                        costs.Add( costSummary );
                    }

                    foreach ( var fee in registrant.FeeValues )
                    {
                        var templateFee = RegistrationTemplate.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
                        if ( fee.Value != null )
                        {
                            foreach ( var feeInfo in fee.Value )
                            {
                                decimal cost = feeInfo.PreviousCost > 0.0m ? feeInfo.PreviousCost : feeInfo.Cost;
                                string desc = string.Format(
                                    "{0}{1} ({2:N0} @ {3})",
                                    templateFee != null ? templateFee.Name : "(Previous Cost)",
                                    string.IsNullOrWhiteSpace( feeInfo.FeeLabel ) ? string.Empty : "-" + feeInfo.FeeLabel,
                                    feeInfo.Quantity,
                                    cost.FormatAsCurrency() );

                                var costSummary = new RegistrationCostSummaryInfo();
                                costSummary.Type = RegistrationCostSummaryType.Fee;
                                costSummary.Description = desc;
                                costSummary.Cost = feeInfo.Quantity * cost;

                                if ( RegistrationState.DiscountPercentage > 0.0m && templateFee != null && templateFee.DiscountApplies && registrant.DiscountApplies )
                                {
                                    if ( RegistrationState.DiscountPercentage >= 1.0m )
                                    {
                                        costSummary.DiscountedCost = 0.0m;
                                    }
                                    else
                                    {
                                        costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * RegistrationState.DiscountPercentage );
                                    }
                                }
                                else
                                {
                                    costSummary.DiscountedCost = costSummary.Cost;
                                }

                                // If template allows a minimum payment, then fees are not included, otherwise it is included
                                costSummary.MinPayment = minimumInitialPaymentPerRegistrant.HasValue ? 0 : costSummary.DiscountedCost;

                                costs.Add( costSummary );
                            }
                        }
                    }
                }

                minimumPayment = 0.0M;
                defaultPayment = null;

                // If there were any costs
                if ( costs.Where( c => c.Cost > 0.0M ).Any() )
                {
                    pnlRegistrantsReview.Visible = false;
                    pnlCostAndFees.Visible = true;

                    // Get the total min payment for all costs and fees
                    minimumPayment = costs.Sum( c => c.MinPayment );

                    if ( costs.Any( c => c.DefaultPayment.HasValue ) )
                    {
                        defaultPayment = costs.Where( c => c.DefaultPayment.HasValue ).Sum( c => c.DefaultPayment.Value );
                    }

                    // Add row for amount discount
                    if ( RegistrationState.DiscountAmount > 0.0m )
                    {
                        decimal totalDiscount = 0.0m - ( RegistrationState.Registrants.Where( r => r.DiscountApplies ).Count() * RegistrationState.DiscountAmount );
                        if ( costs.Sum( c => c.Cost ) + totalDiscount < 0 )
                        {
                            totalDiscount = 0.0m - costs.Sum( c => c.Cost );
                        }
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
                    minimumPayment = minimumPayment.Value > RegistrationState.DiscountedCost ? RegistrationState.DiscountedCost : minimumPayment;

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
                    hfTotalCost.Value = RegistrationState.DiscountedCost.ToString();
                    lTotalCost.Text = RegistrationState.DiscountedCost.FormatAsCurrency();

                    // Check for previous payments
                    lPreviouslyPaid.Visible = RegistrationState.PreviousPaymentTotal != 0.0m;
                    hfPreviouslyPaid.Value = RegistrationState.PreviousPaymentTotal.ToString();
                    lPreviouslyPaid.Text = RegistrationState.PreviousPaymentTotal.FormatAsCurrency();
                    minimumPayment = minimumPayment.Value - RegistrationState.PreviousPaymentTotal;

                    // if min payment is less than 0, set it to 0
                    minimumPayment = minimumPayment.Value < 0 ? 0 : minimumPayment.Value;

                    // Calculate balance due, and if a partial payment is still allowed
                    decimal balanceDue = RegistrationState.DiscountedCost - RegistrationState.PreviousPaymentTotal;

                    // if there is a minimum amount defined (and it is less than the balance due), let a partial payment be specified
                    bool allowPartialPayment = balanceDue > 0 && minimumPayment.Value < balanceDue;

                    // If partial payment is allowed, show the minimum payment due
                    lMinimumDue.Visible = allowPartialPayment;
                    hfMinimumDue.Value = minimumPayment.Value.ToString();
                    lMinimumDue.Text = minimumPayment.Value.FormatAsCurrency();

                    // Make sure payment amount is within minimum due and balance due. If not, set to balance due
                    if ( !RegistrationState.PaymentAmount.HasValue ||
                        RegistrationState.PaymentAmount.Value < minimumPayment.Value ||
                        RegistrationState.PaymentAmount.Value > balanceDue )
                    {
                        if ( defaultPayment.HasValue )
                        {
                            // NOTE: if the configured 'Minimum Initial Payment' is null, the minimumPayment is the full amount, so the 'Default Payment Amount' option would be ignored
                            if ( defaultPayment >= minimumPayment && defaultPayment <= balanceDue )
                            {
                                // default Payment is more than min and less than balance due, so we can use it
                                RegistrationState.PaymentAmount = defaultPayment;
                            }
                            else if (defaultPayment <= minimumPayment)
                            {
                                // default Payment is less than min, so use min instead
                                RegistrationState.PaymentAmount = minimumPayment;
                            }
                            else if (defaultPayment >= balanceDue)
                            {
                                // default Payment is more than balance due, so use balance due
                                RegistrationState.PaymentAmount = balanceDue;
                            }
                        }
                        else
                        {
                            RegistrationState.PaymentAmount = balanceDue;
                        }
                    }

                    nbAmountPaid.Visible = allowPartialPayment;
                    nbAmountPaid.Text = ( RegistrationState.PaymentAmount ?? 0.0m ).ToString( "N2" );

                    // If a previous payment was made, or partial payment is allowed, show the amount remaining after selected payment amount
                    lRemainingDue.Visible = allowPartialPayment;
                    lRemainingDue.Text = ( RegistrationState.DiscountedCost - ( RegistrationState.PreviousPaymentTotal + ( RegistrationState.PaymentAmount ?? 0.0m ) ) ).FormatAsCurrency();

                    lAmountDue.Visible = !allowPartialPayment;
                    lAmountDue.Text = ( RegistrationState.PaymentAmount ?? 0.0m ).FormatAsCurrency();

                    // Set payment options based on gateway settings
                    if ( balanceDue > 0 && RegistrationTemplate.FinancialGateway != null )
                    {
                        if ( RegistrationTemplate.FinancialGateway.Attributes == null )
                        {
                            RegistrationTemplate.FinancialGateway.LoadAttributes();
                        }

                        var component = RegistrationTemplate.FinancialGateway.GetGatewayComponent();
                        if ( component != null )
                        {
                            BindSavedAccounts( component );

                            if ( rblSavedCC.Items.Count > 0 )
                            {
                                pnlPaymentInfo.Visible = true;

                                rblSavedCC.Items[0].Selected = true;
                                rblSavedCC.Visible = true;
                            }
                            else
                            {
                                pnlPaymentInfo.Visible = !Using3StepGateway;
                                rblSavedCC.Visible = false;
                            }

                            divNewCard.Style[HtmlTextWriterStyle.Display] = ( rblSavedCC.Items.Count == 0 || rblSavedCC.Items[rblSavedCC.Items.Count - 1].Selected ) ? "block" : "none";

                            if ( Using3StepGateway )
                            {
                                divNewCard.Visible = false;
                                lbSummaryNext.Text = "Next";
                            }
                            else
                            {
                                divNewCard.Visible = true;
                                lbSummaryNext.Text = "Finish";

                                txtCardFirstName.Visible = component.PromptForNameOnCard( RegistrationTemplate.FinancialGateway ) && component.SplitNameOnCard;
                                txtCardLastName.Visible = component.PromptForNameOnCard( RegistrationTemplate.FinancialGateway ) && component.SplitNameOnCard;
                                txtCardName.Visible = component.PromptForNameOnCard( RegistrationTemplate.FinancialGateway ) && !component.SplitNameOnCard;

                                mypExpiration.MinimumYear = RockDateTime.Now.Year;
                                mypExpiration.MaximumYear = mypExpiration.MinimumYear + 15;

                                acBillingAddress.Visible = component.PromptForBillingAddress( RegistrationTemplate.FinancialGateway );
                            }
                        }
                    }
                    else
                    {
                        pnlPaymentInfo.Visible = false;
                    }
                }
                else
                {
                    var registrants = RegistrationState.Registrants.Where( r => !r.OnWaitList );
                    if ( registrants.Any() )
                    {
                        pnlRegistrantsReview.Visible = true;
                        lRegistrantsReview.Text = string.Format(
                            "<p>The following {0} will be registered for {1}:",
                            RegistrationTemplate.RegistrantTerm.PluralizeIf( registrants.Count() > 1 ).ToLower(),
                            RegistrationInstanceState.Name );

                        rptrRegistrantsReview.DataSource = registrants
                            .Select( r => new
                            {
                                RegistrantName = r.GetFirstName( RegistrationTemplate ) + " " + r.GetLastName( RegistrationTemplate )
                            } );

                        rptrRegistrantsReview.DataBind();
                    }
                    else
                    {
                        pnlRegistrantsReview.Visible = false;
                    }

                    var waitingList = RegistrationState.Registrants.Where( r => r.OnWaitList );
                    if ( waitingList.Any() )
                    {
                        pnlWaitingListReview.Visible = true;
                        lWaitingListReview.Text = string.Format(
                            "<p>The following {0} will be added to the waiting list for {1}:",
                            RegistrationTemplate.RegistrantTerm.PluralizeIf( waitingList.Count() > 1 ).ToLower(),
                            RegistrationInstanceState.Name );

                        rptrWaitingListReview.DataSource = waitingList
                            .Select( r => new
                            {
                                RegistrantName = r.GetFirstName( RegistrationTemplate ) + " " + r.GetLastName( RegistrationTemplate )
                            } );
                        rptrWaitingListReview.DataBind();
                    }
                    else
                    {
                        pnlWaitingListReview.Visible = false;
                    }

                    RegistrationState.TotalCost = 0.0m;
                    RegistrationState.DiscountedCost = 0.0m;
                    pnlCostAndFees.Visible = false;
                    pnlPaymentInfo.Visible = false;
                }
            }
        }

        /// <summary>
        /// Binds the saved accounts to radio button list
        /// </summary>
        /// <param name="component">The component.</param>
        private void BindSavedAccounts( GatewayComponent component )
        {
            var currentValue = rblSavedCC.SelectedValue;

            rblSavedCC.Items.Clear();

            if ( CurrentPerson != null )
            {
                // Get the saved accounts for the currently logged in user
                var savedAccounts = new FinancialPersonSavedAccountService( new RockContext() )
                    .GetByPersonId( CurrentPerson.Id );

                // Verify component is valid and that it supports using saved accounts for one-time, credit card transactions
                var ccCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                if ( component != null &&
                    component.SupportsSavedAccount( false ) &&
                    component.SupportsSavedAccount( ccCurrencyType ) )
                {
                    rblSavedCC.DataSource = savedAccounts
                        .Where( a =>
                            a.FinancialGatewayId == RegistrationTemplate.FinancialGateway.Id &&
                            a.FinancialPaymentDetail != null &&
                            a.FinancialPaymentDetail.CurrencyTypeValueId == ccCurrencyType.Id )
                        .OrderBy( a => a.Name )
                        .Select( a => new
                        {
                            Id = a.Id,
                            Name = "Use " + a.Name + " (" + a.FinancialPaymentDetail.AccountNumberMasked + ")"
                        } ).ToList();
                    rblSavedCC.DataBind();
                    if ( rblSavedCC.Items.Count > 0 )
                    {
                        rblSavedCC.Items.Add( new ListItem( "Use a different card", "0" ) );
                        rblSavedCC.SetValue( currentValue );
                    }
                }
            }
        }

        /// <summary>
        /// Updates RegistrationState props using info from the UI controls
        /// </summary>
        private void ParseSummaryControls()
        {
            if ( RegistrationState != null )
            {
                if ( RegistrationTemplate.RegistrarOption == RegistrarOption.UseLoggedInPerson && CurrentPerson != null )
                {
                    RegistrationState.FirstName = CurrentPerson.NickName;
                    RegistrationState.LastName = CurrentPerson.LastName;
                    RegistrationState.ConfirmationEmail = CurrentPerson.Email;
                    if ( pnlRegistrarInfoUseLoggedInPerson.Visible )
                    {
                        RegistrationState.ConfirmationEmail = tbUseLoggedInPersonEmail.Text;
                    }
                }
                else
                {
                    RegistrationState.FirstName = tbYourFirstName.Text;
                    RegistrationState.LastName = tbYourLastName.Text;
                    RegistrationState.ConfirmationEmail = tbConfirmationEmail.Text;
                }

                if ( rblRegistrarFamilyOptions.Visible )
                {
                    RegistrationState.FamilyGuid = rblRegistrarFamilyOptions.SelectedValue.AsGuid();
                }

                if ( RegistrationState.FamilyGuid.Equals( Guid.Empty ) )
                {
                    RegistrationState.FamilyGuid = Guid.NewGuid();
                }

                RegistrationState.PaymentAmount = nbAmountPaid.Text.AsDecimal();
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Applies the first automatic discount that matches the set limits.
        /// </summary>
        private void AutoApplyDiscounts()
        {
            if ( RegistrationState != null )
            {
                var discounts = RegistrationTemplate.Discounts
                        .Where( d => d.AutoApplyDiscount )
                        .OrderBy( d => d.Order )
                        .ToList();

                foreach ( var discount in discounts )
                {
                    bool validDiscount = true;

                    if ( validDiscount && discount.MinRegistrants.HasValue && RegistrationState.RegistrantCount < discount.MinRegistrants.Value )
                    {
                        nbDiscountCode.Visible = true;
                        validDiscount = false;
                    }

                    if ( validDiscount && discount.StartDate.HasValue && RockDateTime.Today < discount.StartDate.Value )
                    {
                        nbDiscountCode.Visible = true;
                        validDiscount = false;
                    }

                    if ( validDiscount && discount.EndDate.HasValue && RockDateTime.Today > discount.EndDate.Value )
                    {
                        nbDiscountCode.Visible = true;
                        validDiscount = false;
                    }

                    if ( validDiscount && discount.MaxUsage.HasValue && RegistrationInstanceState != null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var instances = new RegistrationService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( r =>
                                    r.RegistrationInstanceId == RegistrationInstanceState.Id &&
                                    ( !RegistrationState.RegistrationId.HasValue || r.Id != RegistrationState.RegistrationId.Value ) &&
                                    r.DiscountCode == discount.Code )
                                .Count();
                            if ( instances >= discount.MaxUsage.Value )
                            {
                                nbDiscountCode.Visible = true;
                                validDiscount = false;
                            }
                        }
                    }

                    if ( validDiscount && discount.MaxRegistrants.HasValue )
                    {
                        RegistrationState.Registrants.ForEach( r => r.DiscountApplies = false );
                        for ( int i = 0; i < RegistrationState.Registrants.Count; i++ )
                        {
                            RegistrationState.Registrants[i].DiscountApplies = i < discount.MaxRegistrants.Value;
                        }
                    }

                    RegistrationState.DiscountCode = validDiscount ? discount.Code : string.Empty;
                    RegistrationState.DiscountPercentage = validDiscount ? discount.DiscountPercentage : 0.0m;
                    RegistrationState.DiscountAmount = validDiscount ? discount.DiscountAmount : 0.0m;

                    if ( validDiscount )
                    {
                        nbDiscountCode.Visible = true;
                        nbDiscountCode.NotificationBoxType = NotificationBoxType.Success;
                        nbDiscountCode.Text = string.Format( "The {0} '{1}' was automatically applied.", DiscountCodeTerm.ToLower(), discount.Code );
                        break;
                    }
                }
            }
        }

        #endregion
    }
}