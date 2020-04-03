// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Text;
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
using Rock.Field;
using Rock.Financial;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Helper = Rock.Attribute.Helper;

namespace RockWeb.Plugins.com_bemaservices.Event
{
    /// <summary>
    /// Block used to register for a registration instance.
    /// </summary>
    [DisplayName( "Multi-Event Registration Wizard" )]
    [Category( "BEMA Services > Event" )]
    [Description( "Block used to register for multiple registration instances." )]
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
    [BooleanField( "Hide First and Last Name fields", "Hide first and last name fields, and skip any forms that are only those fields.", defaultValue: true, order: 10, key: "HideFirstLastNameFields" )]
    [BooleanField( "Only Allow Adding Children", "Should Children be the only family role able to be added?", defaultValue: true, order: 10, key: "OnlyAllowAddingChildren" )]
    [DefinedValueRangeField( Rock.SystemGuid.DefinedType.SCHOOL_GRADES, "Selectable Grade Range", "", false, "6b5cdfbd-9882-4ebb-a01a-7856bcd0cf61,c49bd3af-ff94-4a7c-99e1-08503a3c746e", "", 12 )]
    [IntegerField( "Default Category Id" )]
    [CodeEditorField( "Start Page Pre Instructions", "", CodeEditorMode.Html )]
    [CodeEditorField( "Start Page Post Instructions", "", CodeEditorMode.Html )]
    [TextField( "Specialty Description Link" )]
    public partial class MultiEventRegistrationWizard : RockBlock
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
        private const string CATEGORY_ID_PARAM_NAME = "CategoryId";

        // Viewstate keys
        private const string REGISTRATION_INFORMATION_LIST_KEY = "RegistrationInformationList";
        private const string MULTIEVENT_REGISTRANTS_KEY = "MultiEventRegistrants";
        private const string CURRENT_MULTIEVENT_REGISTRANT_INDEX_KEY = "CurrentMultiEventRegistrantIndex";
        private const string CURRENT_REGISTRATION_INFORMATION_KEY = "CurrentRegistrationInformation";
        private const string CURRENT_PANEL_KEY = "CurrentPanel";
        private const string IS_DISCOUNT_COLUMN_SHOWN_KEY = "IsDiscountColumnShown";
        private const string MINIMUM_PAYMENT_KEY = "MinimumPayment";
        private const string DEFAULT_PAYMENT_KEY = "DefaultPayment";
        private const string PAYMENT_AMOUNT_KEY = "PaymentAmount";
        private const string FINANCIAL_GATEWAY_KEY = "FinancialGateway";
        private const string FINANCIAL_ACCOUNT_KEY = "FinancialAccount";

        public enum PanelIndex
        {
            PanelStart = 0,
            PanelSelectRegistrations = 1,
            PanelRegistrationAttributesStart = 2,
            PanelRegistrant = 3,
            PanelRegistrationAttributesEnd = 4,
            PanelSummary = 5,
            PanelPayment = 6,
            PanelSuccess = 7
        }
        #endregion

        #region Properties
        public List<RegistrationInformation> RegistrationInformationList { get; set; }

        public List<MultiEventRegistrant> MultiEventRegistrants { get; set; }

        public int CurrentMultiEventRegistrantIndex { get; set; }

        public RegistrationInformation CurrentRegistrationInformation { get; set; }

        /// <summary>
        /// Gets or sets the current panel.
        /// </summary>
        /// <value>
        /// The current panel.
        /// </value>
        public PanelIndex CurrentPanel { get; set; }

        public bool IsDiscountColumnShown { get; set; }

        public decimal? minimumPayment { get; set; }
        public decimal? defaultPayment { get; set; }
        public decimal? paymentAmount { get; set; }

        public FinancialGateway FinancialGateway { get; set; }

        public FinancialAccount FinancialAccount { get; set; }

        protected string Step2IFrameUrl
        {
            get { return ViewState["Step2IFrameUrl"] as string; }
            set { ViewState["Step2IFrameUrl"] = value; }
        }
        protected bool Using3StepGateway
        {
            get { return ViewState["Using3StepGateway"] as bool? ?? false; }
            set { ViewState["Using3StepGateway"] = value; }
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

            string json = ViewState[REGISTRATION_INFORMATION_LIST_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                RegistrationInformationList = new List<RegistrationInformation>();
            }
            else
            {
                RegistrationInformationList = JsonConvert.DeserializeObject<List<RegistrationInformation>>( json );
            }

            json = ViewState[MULTIEVENT_REGISTRANTS_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                MultiEventRegistrants = new List<MultiEventRegistrant>();
            }
            else
            {
                MultiEventRegistrants = JsonConvert.DeserializeObject<List<MultiEventRegistrant>>( json );
            }

            json = ViewState[CURRENT_REGISTRATION_INFORMATION_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                CurrentRegistrationInformation = new RegistrationInformation();
            }
            else
            {
                CurrentRegistrationInformation = JsonConvert.DeserializeObject<RegistrationInformation>( json );
            }

            json = ViewState[FINANCIAL_GATEWAY_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FinancialGateway = new FinancialGateway();
            }
            else
            {
                FinancialGateway = JsonConvert.DeserializeObject<FinancialGateway>( json );
            }

            json = ViewState[FINANCIAL_ACCOUNT_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FinancialAccount = new FinancialAccount();
            }
            else
            {
                FinancialAccount = JsonConvert.DeserializeObject<FinancialAccount>( json );
            }

            CurrentMultiEventRegistrantIndex = ViewState[CURRENT_MULTIEVENT_REGISTRANT_INDEX_KEY] as int? ?? 0;
            IsDiscountColumnShown = ViewState[IS_DISCOUNT_COLUMN_SHOWN_KEY] as bool? ?? false;
            CurrentPanel = ViewState[CURRENT_PANEL_KEY] as PanelIndex? ?? PanelIndex.PanelStart;
            minimumPayment = ViewState[MINIMUM_PAYMENT_KEY] as decimal?;
            defaultPayment = ViewState[DEFAULT_PAYMENT_KEY] as decimal?;
            paymentAmount = ViewState[PAYMENT_AMOUNT_KEY] as decimal?;

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

            if ( !Page.IsPostBack )
            {
                if ( CurrentPerson != null && CurrentPerson.IsBusiness() )
                {
                    ShowError( "Invalid Login", "Sorry, the login you are using doesn't appear to be tied to a valid person record. Try logging out and logging in with a different username, or create a new account before registering for the selected event." );
                }
                else
                {
                    var categoryId = PageParameter( CATEGORY_ID_PARAM_NAME ).AsInteger();
                    if ( categoryId == null || categoryId == 0 )
                    {
                        categoryId = GetAttributeValue( "DefaultCategoryId" ).AsInteger();
                    }

                    var activeRegistrationInstances = new RegistrationInstanceService( new RockContext() ).Queryable().AsNoTracking().Where( ri =>
                          ri.RegistrationTemplate.CategoryId == categoryId &&
                          ri.RegistrationTemplate.IsActive &&
                          ri.IsActive &&
                          ri.StartDateTime <= RockDateTime.Now &&
                          ri.EndDateTime >= RockDateTime.Now );
                    if ( activeRegistrationInstances.Any() )
                    {
                        FinancialGateway financialGateway = null;
                        FinancialAccount financialAccount = null;
                        var category = CategoryCache.Get( categoryId );
                        category.LoadAttributes();
                        var financialGatewayGuid = category.GetAttributeValue( "FinancialGateway" ).AsGuidOrNull();
                        var financialAccountGuid = category.GetAttributeValue( "FinancialAccount" ).AsGuidOrNull();
                        if ( financialGatewayGuid.HasValue && financialAccountGuid.HasValue )
                        {
                            financialGateway = new FinancialGatewayService( new RockContext() ).Get( financialGatewayGuid.Value );
                            financialAccount = new FinancialAccountService( new RockContext() ).Get( financialAccountGuid.Value );
                            if ( financialGateway != null && financialAccount != null )
                            {
                                FinancialGateway = financialGateway;
                                FinancialAccount = financialAccount;
                                var threeStepGateway = FinancialGateway.GetGatewayComponent() as IThreeStepGatewayComponent;
                                Using3StepGateway = threeStepGateway != null;
                                if ( Using3StepGateway )
                                {
                                    Step2IFrameUrl = ResolveRockUrl( threeStepGateway.Step2FormUrl );
                                }
                                SaveViewState();
                                // Check Login Requirement
                                if ( CurrentUser == null )
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
                                    if ( CurrentRegistrationInformation != null &&
                                        CurrentRegistrationInformation.SignInline &&
                                        !PageParameter( "redirected" ).AsBoolean() &&
                                        CurrentRegistrationInformation.DigitalSignatureComponent != null &&
                                        !string.IsNullOrWhiteSpace( CurrentRegistrationInformation.DigitalSignatureComponent.CookieInitializationUrl ) )
                                    {
                                        // Redirect for Digital Signature Cookie Initialization
                                        var returnUrl = ResolvePublicUrl( Request.Url.PathAndQuery );
                                        returnUrl = returnUrl + ( returnUrl.Contains( "?" ) ? "&" : "?" ) + "redirected=True";
                                        string redirectUrl = string.Format( "{0}?redirect_uri={1}", CurrentRegistrationInformation.DigitalSignatureComponent.CookieInitializationUrl, HttpUtility.UrlEncode( returnUrl ) );
                                        Response.Redirect( redirectUrl, false );
                                    }
                                    else
                                    {
                                        // show the panel with the instructions and/or asking how many registrants ( it may be skipped if there are neither )
                                        ShowStart();
                                    }
                                }
                            }
                            else
                            {
                                ShowWarning( "Sorry", "The financial gateway and account has not been configured." );

                            }
                        }
                        else
                        {
                            ShowWarning( "Sorry", "The financial gateway and account has not been configured." );

                        }


                    }
                    else
                    {
                        ShowWarning( "Sorry", "The selected registrations could not be found or are no longer active." );
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
            string registrationSlug = PageParameter( SLUG_PARAM_NAME );

            if ( registrationInstanceId.HasValue )
            {
                string registrationInstanceName = new RegistrationInstanceService( new RockContext() ).GetSelect( registrationInstanceId.Value, a => a.Name );

                RockPage.Title = registrationInstanceName;
                breadCrumbs.Add( new BreadCrumb( registrationInstanceName, pageReference ) );
            }
            else if ( registrationSlug.IsNotNullOrWhiteSpace() )
            {
                var dateTime = RockDateTime.Now;
                var linkage = new EventItemOccurrenceGroupMapService( new RockContext() )
                    .Queryable()
                    .AsNoTracking()
                    .Where( l => l.UrlSlug == registrationSlug )
                    .Where( l => l.RegistrationInstance != null )
                    .Where( l => l.RegistrationInstance.IsActive )
                    .Where( l => l.RegistrationInstance.RegistrationTemplate != null )
                    .Where( l => l.RegistrationInstance.RegistrationTemplate.IsActive )
                    .Where( l => ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) )
                    .Where( l => ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                    .FirstOrDefault();

                if ( linkage != null )
                {
                    RockPage.Title = linkage.RegistrationInstance.Name;
                    breadCrumbs.Add( new BreadCrumb( linkage.RegistrationInstance.Name, pageReference ) );
                }
            }
            else
            {
                breadCrumbs.Add( new BreadCrumb( this.PageCache.PageTitle, pageReference ) );
            }

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

            ViewState[MULTIEVENT_REGISTRANTS_KEY] = JsonConvert.SerializeObject( MultiEventRegistrants, Formatting.None, jsonSetting );
            ViewState[REGISTRATION_INFORMATION_LIST_KEY] = JsonConvert.SerializeObject( RegistrationInformationList, Formatting.None, jsonSetting );
            var test = JsonConvert.SerializeObject( CurrentRegistrationInformation, Formatting.None, jsonSetting );
            ViewState[CURRENT_REGISTRATION_INFORMATION_KEY] = JsonConvert.SerializeObject( CurrentRegistrationInformation, Formatting.None, jsonSetting );
            ViewState[FINANCIAL_GATEWAY_KEY] = JsonConvert.SerializeObject( FinancialGateway, Formatting.None, jsonSetting );
            ViewState[FINANCIAL_ACCOUNT_KEY] = JsonConvert.SerializeObject( FinancialAccount, Formatting.None, jsonSetting );
            ViewState[CURRENT_MULTIEVENT_REGISTRANT_INDEX_KEY] = CurrentMultiEventRegistrantIndex;
            ViewState[CURRENT_PANEL_KEY] = CurrentPanel;
            ViewState[IS_DISCOUNT_COLUMN_SHOWN_KEY] = IsDiscountColumnShown;
            ViewState[MINIMUM_PAYMENT_KEY] = minimumPayment;
            ViewState[DEFAULT_PAYMENT_KEY] = defaultPayment;
            ViewState[PAYMENT_AMOUNT_KEY] = paymentAmount;

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
                        this.AddHistory( "event", string.Format( "1,{0},{1}", CurrentRegistrationInformation.CurrentRegistrantIndex, CurrentRegistrationInformation.CurrentFormIndex ) );
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

                PanelIndex panelIndex = PanelIndex.PanelStart;
                int registrantId = 0;
                int formId = 0;

                if ( commands.Count() == 3 )
                {
                    panelIndex = commands[0].ConvertToEnumOrNull<PanelIndex>() ?? PanelIndex.PanelStart;
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
                            CurrentRegistrationInformation.CurrentRegistrantIndex = registrantId;
                            CurrentRegistrationInformation.CurrentFormIndex = formId;
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
                            ShowStart();
                            break;
                        }
                }
            }
            else if ( CurrentPanel == PanelIndex.PanelSummary && !hfAllowNavigate.Value.AsBoolean() )
            {
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                if ( CurrentRegistrationInformation.RegistrationInstanceState != null )
                {
                    qryParams.Add( REGISTRATION_INSTANCE_ID_PARAM_NAME, CurrentRegistrationInformation.RegistrationInstanceState.Id.ToString() );
                }
                this.NavigateToCurrentPageReference( qryParams );
            }
            else
            {
                ShowStart();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbHowManyNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbHowManyNext_Click( object sender, EventArgs e )
        {
            MultiEventRegistrants = new List<MultiEventRegistrant>();
            foreach ( RepeaterItem item in rFamilyMembers.Items )
            {
                var hfPersonAliasId = item.FindControl( "hfPersonAliasId" ) as HiddenField;
                var cbFamilyMember = item.FindControl( "cbFamilyMember" ) as CheckBox;
                var lbEditGroupMember = item.FindControl( "lbEditGroupMember" ) as LinkButton;

                MultiEventRegistrant multiEventRegistrant = null;
                if ( MultiEventRegistrants != null )
                {
                    multiEventRegistrant = MultiEventRegistrants.Where( mer => mer.PersonAliasId == hfPersonAliasId.ValueAsInt() ).FirstOrDefault();
                }

                if ( cbFamilyMember.Checked )
                {
                    if ( multiEventRegistrant == null )
                    {
                        multiEventRegistrant = new MultiEventRegistrant();
                        multiEventRegistrant.PersonAliasId = hfPersonAliasId.ValueAsInt();
                        multiEventRegistrant.CurrentRegistrationInstanceIndex = 0;
                        multiEventRegistrant.RegistrationInstanceIds = new List<int>();
                        MultiEventRegistrants.Add( multiEventRegistrant );
                    }
                }
                else
                {
                    if ( multiEventRegistrant != null )
                    {
                        var personAlias = new PersonAliasService( new RockContext() ).Get( multiEventRegistrant.PersonAliasId );

                        MultiEventRegistrants.Remove( multiEventRegistrant );
                        if ( RegistrationInformationList != null )
                        {
                            foreach ( var registrationInformation in RegistrationInformationList )
                            {
                                var registrant = registrationInformation.RegistrationState.Registrants.Where( r => r.PersonId == personAlias.PersonId ).FirstOrDefault();
                                if ( registrant != null )
                                {
                                    registrationInformation.RegistrationState.Registrants.Remove( registrant );
                                }
                            }
                        }
                    }
                }
            }


            CurrentMultiEventRegistrantIndex = 0;
            if ( MultiEventRegistrants.Count > 0 )
            {
                ShowRegistrations();
            }
            else
            {
                nbWaitingList.Text = "Please select at least one family member.";
                nbWaitingList.Visible = true;
            }
        }

        private void ShowRegistrations()
        {
            SetPanel( PanelIndex.PanelSelectRegistrations );

            MultiEventRegistrant multiEventRegistrant = MultiEventRegistrants[CurrentMultiEventRegistrantIndex];
            var registrantPersonAlias = new PersonAliasService( new RockContext() ).Get( multiEventRegistrant.PersonAliasId );
            if ( registrantPersonAlias != null )
            {
                lCurrentRegistrantName.Text = registrantPersonAlias.Person.FullName;
                lSpecialtyLink.Text = String.Format( "<a target='_blank' href='{0}'/>View Specialty Descriptions</a>", GetAttributeValue( "SpecialtyDescriptionLink" ) );
                var categoryId = PageParameter( CATEGORY_ID_PARAM_NAME ).AsIntegerOrNull();
                if ( categoryId == null || categoryId == 0 )
                {
                    categoryId = GetAttributeValue( "DefaultCategoryId" ).AsInteger();
                }

                if ( categoryId != null )
                {
                    var category = CategoryCache.Get( categoryId.Value );
                    if ( category != null )
                    {
                        var activeRegistrationInstances = new RegistrationInstanceService( new RockContext() ).Queryable().AsNoTracking().Where( ri =>
                              ri.RegistrationTemplate.CategoryId == categoryId &&
                              ri.RegistrationTemplate.IsActive &&
                              ri.IsActive &&
                              ri.StartDateTime <= RockDateTime.Now &&
                              ri.EndDateTime >= RockDateTime.Now );

                        List<RegistrationInstance> filteredRegistrationInstances = FilterRegistrationInstances( registrantPersonAlias, activeRegistrationInstances );

                        rptRegistrations.DataSource = filteredRegistrationInstances.GroupBy( ri => ri.RegistrationTemplateId ).OrderBy( rt => rt.Max( ri => ri.RegistrationTemplate.Name.SafeSubstring( 0, 1 ) ) ).ThenBy( rt => rt.Max( ri => ri.EndDateTime ) ).ToList();
                        rptRegistrations.DataBind();

                    }
                }
            }

        }

        private static List<RegistrationInstance> FilterRegistrationInstances( PersonAlias registrantPersonAlias, IQueryable<RegistrationInstance> activeRegistrationInstances )
        {
            List<RegistrationInstance> filteredRegistrationInstances = new List<RegistrationInstance>();

            foreach ( var activeRegistrationInstance in activeRegistrationInstances )
            {
                var includeInstance = true;
                activeRegistrationInstance.LoadAttributes();
                var instanceChildrenOnly = activeRegistrationInstance.GetAttributeValue( "ChildrenOnly" ).AsBoolean();
                var instanceGradeRange = activeRegistrationInstance.GetAttributeValue( "LimitByGrade" );

                if ( instanceChildrenOnly == true )
                {
                    var familyRole = registrantPersonAlias.Person.GetFamilyRole();
                    if ( familyRole.Guid != Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() )
                    {
                        includeInstance = false;
                    }
                }

                if ( instanceGradeRange.IsNotNullOrWhiteSpace() )
                {
                    var gradeOffsetRangePair = instanceGradeRange.Split( new char[] { ',' }, StringSplitOptions.None ).AsGuidOrNullList().ToArray();
                    var personsGradeOffset = registrantPersonAlias.Person.GradeOffset;
                    DefinedValueCache minGradeDefinedValue = null;
                    DefinedValueCache maxGradeDefinedValue = null;
                    if ( gradeOffsetRangePair.Length == 2 )
                    {
                        minGradeDefinedValue = gradeOffsetRangePair[0].HasValue ? DefinedValueCache.Get( gradeOffsetRangePair[0].Value ) : null;
                        maxGradeDefinedValue = gradeOffsetRangePair[1].HasValue ? DefinedValueCache.Get( gradeOffsetRangePair[1].Value ) : null;
                    }

                    if ( minGradeDefinedValue != null )
                    {
                        int? minGradeOffset = minGradeDefinedValue.Value.AsIntegerOrNull();
                        if ( minGradeOffset.HasValue )
                        {
                            if ( !personsGradeOffset.HasValue || personsGradeOffset > minGradeOffset.Value )
                            {
                                includeInstance = false;
                            }
                        }
                    }

                    if ( maxGradeDefinedValue != null )
                    {
                        int? maxGradeOffset = maxGradeDefinedValue.Value.AsIntegerOrNull();
                        if ( maxGradeOffset.HasValue )
                        {
                            if ( !personsGradeOffset.HasValue || personsGradeOffset < maxGradeOffset )
                            {
                                includeInstance = false;
                            }
                        }
                    }
                }

                if ( includeInstance == true )
                {
                    filteredRegistrationInstances.Add( activeRegistrationInstance );
                }
            }

            return filteredRegistrationInstances;
        }

        protected void rptRegistrations_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var grouping = e.Item.DataItem as IGrouping<int, RegistrationInstance>;
            if ( grouping != null )
            {
                var hfInstanceId = e.Item.FindControl( "hfInstanceId" ) as HiddenField;
                var cbTemplate = e.Item.FindControl( "cbTemplate" ) as CheckBox;
                var divItem = e.Item.FindControl( "divItem" );
                var lTemplateName = e.Item.FindControl( "lTemplateName" ) as Literal;
                var rrbRegistrationInstances = e.Item.FindControl( "rrbRegistrationInstances" ) as RockRadioButtonList;

                var registrationTemplate = new RegistrationTemplateService( new RockContext() ).Get( grouping.Key );

                if ( registrationTemplate.Name.Contains( "Admin Fee" ) || registrationTemplate.Name.Contains( "Admin 2020" ) )
                {
                    divItem.Visible = false;
                }

                cbTemplate.Text = registrationTemplate.Name;
                lTemplateName.Text = registrationTemplate.Name;

                rrbRegistrationInstances.Items.Clear();
                foreach ( var registrationInstance in grouping )
                {
                    ListItem listItem = new ListItem( String.Format( "{0} ({1})", registrationInstance.Name, GetCostForInstance( registrationInstance ).FormatAsCurrency() ), registrationInstance.Id.ToString() );
                    if ( registrationInstance.MaxAttendees.HasValue )
                    {
                        var remainingSpots = registrationInstance.MaxAttendees - registrationInstance.Registrations.Sum( r => r.Registrants.Count );
                        if ( remainingSpots <= 0 )
                        {
                            if ( registrationInstance.RegistrationTemplate.WaitListEnabled )
                            {
                                listItem.Text += " (Waitlist)";
                            }
                            else
                            {
                                cbTemplate.Text += " (Full)";
                                listItem.Enabled = false;
                            }
                        }
                        else
                        {
                            if ( remainingSpots <= 5 )
                            {
                                listItem.Text += String.Format( " ({0} Spots Left)", remainingSpots );
                            }
                        }
                    }


                    var isRequired = registrationInstance.GetAttributeValue( "RequiredInstance" ).AsBoolean();
                    if ( isRequired )
                    {
                        listItem.Enabled = false;

                        listItem.Selected = true;
                        cbTemplate.Checked = true;
                        cbTemplate.Enabled = false;
                        rrbRegistrationInstances.Visible = true;
                    }

                    rrbRegistrationInstances.Items.Add( listItem );
                }

            }
        }

        protected decimal? GetCostForInstance( RegistrationInstance instance )
        {
            if ( instance.RegistrationTemplate.SetCostOnInstance == true )
            {
                return instance.Cost;
            }
            else
            {
                return instance.RegistrationTemplate.Cost;
            }
        }

        protected void lbSelectRegistrationPrev_Click( object sender, EventArgs e )
        {
            if ( CurrentMultiEventRegistrantIndex == 0 )
            {
                ShowStart();
            }
            else
            {

            }
        }
        protected void lbSelectRegistrationNext_Click( object sender, EventArgs e )
        {
            if ( MultiEventRegistrants[CurrentMultiEventRegistrantIndex].RegistrationInstanceIds == null )
            {
                MultiEventRegistrants[CurrentMultiEventRegistrantIndex].RegistrationInstanceIds = new List<int>();
            }

            foreach ( RepeaterItem item in rptRegistrations.Items )
            {
                int? registrationInstanceId = null;
                var hfInstanceId = item.FindControl( "hfInstanceId" ) as HiddenField;
                var cbTemplate = item.FindControl( "cbTemplate" ) as CheckBox;
                var rrbRegistrationInstances = item.FindControl( "rrbRegistrationInstances" ) as RockRadioButtonList;

                if ( cbTemplate.Checked )
                {
                    if ( hfInstanceId.ValueAsInt() != 0 )
                    {
                        registrationInstanceId = hfInstanceId.ValueAsInt();
                    }
                    else
                    {
                        registrationInstanceId = rrbRegistrationInstances.SelectedValueAsId();
                    }
                }
                else
                {
                    registrationInstanceId = rrbRegistrationInstances.SelectedValueAsId();
                }


                if ( registrationInstanceId.HasValue )
                {
                    MultiEventRegistrants[CurrentMultiEventRegistrantIndex].RegistrationInstanceIds.Add( registrationInstanceId.Value );
                }
            }

            if ( MultiEventRegistrants[CurrentMultiEventRegistrantIndex].RegistrationInstanceIds.Count > 1 )
            {
                MultiEventRegistrants[CurrentMultiEventRegistrantIndex].CurrentRegistrationInstanceIndex = 0;
                ShowRegistrationAttributesStart( true );
            }
            else
            {
                nbWaitingList.Text = "Please select at least one registration.";
                nbWaitingList.Visible = true;
            }

        }

        protected void cbTemplate_CheckedChanged( object sender, EventArgs e )
        {
            var cbTemplate = sender as CheckBox;
            if ( cbTemplate != null )
            {
                var repeaterItem = cbTemplate.Parent;
                var rrbRegistrationInstances = repeaterItem.FindControl( "rrbRegistrationInstances" ) as RockRadioButtonList;
                // We used to hide the instances unless the template was selected. Per Redd McGehee @ 3/24/2020, we now show them at all times.
                // rrbRegistrationInstances.Visible = cbTemplate.Checked;
            }
        }

        /// <summary>
        /// Shows the registration attributes before (or navigate to next/prev page if there aren't any)
        /// </summary>
        private void ShowRegistrationAttributesStart( bool forward )
        {
            SetCurrentRegistrationInformation();

            if ( CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants.Any() && CurrentRegistrationInformation.CurrentRegistrantIndex == 0 )
            {
                decimal currentStep = 1;
                CurrentRegistrationInformation.PercentComplete = ( currentStep / CurrentRegistrationInformation.ProgressBarSteps ) * 100.0m;
                pnlRegistrationAttributesStartProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

                lRegistrationAttributesStartTitle.Text = CurrentRegistrationInformation.RegistrationAttributeTitleStart;

                avcRegistrationAttributesStart.ShowCategoryLabel = false;
                avcRegistrationAttributesStart.IncludedAttributes = CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants.Select( a => AttributeCache.Get( a ) ).ToArray();
                Registration registration = null;
                if ( ( CurrentRegistrationInformation.RegistrationState != null ) && CurrentRegistrationInformation.RegistrationState.RegistrationId.HasValue )
                {
                    registration = new RegistrationService( new RockContext() ).Get( CurrentRegistrationInformation.RegistrationState.RegistrationId.Value );
                }

                if ( registration == null )
                {
                    registration = new Registration
                    {
                        RegistrationInstance = CurrentRegistrationInformation.RegistrationInstanceState
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
            if ( CurrentRegistrationInformation.RegistrationAttributeIdsAfterRegistrants.Any() && CurrentRegistrationInformation.CurrentRegistrantIndex == 1 )
            {
                decimal currentStep = ( CurrentRegistrationInformation.FormCount * CurrentRegistrationInformation.RegistrationState.RegistrantCount ) + 1;
                if ( CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants.Any() )
                {
                    currentStep++;
                }

                CurrentRegistrationInformation.PercentComplete = ( currentStep / CurrentRegistrationInformation.ProgressBarSteps ) * 100.0m;
                pnlRegistrationAttributesStartProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

                lRegistrationAttributesEndTitle.Text = CurrentRegistrationInformation.RegistrationAttributeTitleEnd;
                avcRegistrationAttributesEnd.ShowCategoryLabel = false;
                avcRegistrationAttributesEnd.IncludedAttributes = CurrentRegistrationInformation.RegistrationAttributeIdsAfterRegistrants.Select( a => AttributeCache.Get( a ) ).ToArray();
                Registration registration = null;
                if ( ( CurrentRegistrationInformation.RegistrationState != null ) && CurrentRegistrationInformation.RegistrationState.RegistrationId.HasValue )
                {
                    registration = new RegistrationService( new RockContext() ).Get( CurrentRegistrationInformation.RegistrationState.RegistrationId.Value );
                }

                if ( registration == null )
                {
                    registration = new Registration
                    {
                        RegistrationInstance = CurrentRegistrationInformation.RegistrationInstanceState
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
            ShowStart();
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

            CurrentRegistrationInformation.CurrentFormIndex = 0;

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
            int registrantPages = CurrentRegistrationInformation.FormCount;
            if ( CurrentRegistrationInformation.SignInline )
            {
                registrantPages += 2;
            }

            int registrantCount = 0;
            if ( CurrentRegistrationInformation.RegistrationState != null )
            {
                registrantCount = CurrentRegistrationInformation.RegistrationState.RegistrantCount;
            }

            CurrentRegistrationInformation.ProgressBarSteps = ( registrantCount * registrantPages ) + 2;

            if ( CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                CurrentRegistrationInformation.ProgressBarSteps++;
            }

            if ( CurrentRegistrationInformation.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                CurrentRegistrationInformation.ProgressBarSteps++;
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
                ShowStart();
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
                ShowStart();
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
                if ( CurrentRegistrationInformation.RegistrationState != null && CurrentRegistrationInformation.RegistrationState.RegistrantCount > CurrentRegistrationInformation.CurrentRegistrantIndex )
                {
                    var registrant = CurrentRegistrationInformation.RegistrationState.Registrants[CurrentRegistrationInformation.CurrentRegistrantIndex];
                    registrant.SignatureDocumentKey = qryString.Substring( 13 );
                    registrant.SignatureDocumentLastSent = RockDateTime.Now;
                }

                lbRegistrantNext_Click( sender, e );
            }
            else
            {
                string errorMessage = string.Format(
                        "This {0} requires that you sign a {1} for each registrant, but it appears that you may have cancelled or skipped signing this document.",
                        CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrationTerm,
                        CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplate.Name );

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

                CurrentRegistrationInformation.CurrentRegistrantIndex = CurrentRegistrationInformation.RegistrationState != null ? CurrentRegistrationInformation.RegistrationState.RegistrantCount - 1 : 0;
                CurrentRegistrationInformation.CurrentFormIndex = CurrentRegistrationInformation.FormCount - 1;

                nbAmountPaid.Text = string.Empty;
                CurrentRegistrationInformation.RegistrationState.PaymentAmount = null;

                ShowRegistrant( false, false );

                hfTriggerScroll.Value = "true";
            }
            else
            {
                ShowStart();
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
            var currentRegistrant = MultiEventRegistrants[CurrentMultiEventRegistrantIndex];
            currentRegistrant.CurrentRegistrationInstanceIndex++;
            if ( currentRegistrant.CurrentRegistrationInstanceIndex < currentRegistrant.RegistrationInstanceIds.Count )
            {
                ShowRegistrationAttributesStart( true );
            }
            else
            {
                CurrentMultiEventRegistrantIndex++;
                if ( CurrentMultiEventRegistrantIndex < MultiEventRegistrants.Count )
                {
                    ShowRegistrations();
                }
                else
                {
                    ShowSummary();

                }

            }
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
                ShowStart();
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
            NavigateToPaymentPage( false );
        }

        private void NavigateToPaymentPage( bool isPaidInFull = false )
        {
            if ( CurrentPanel == PanelIndex.PanelSummary )
            {
                List<string> summaryErrors = ValidateSummary();
                if ( !summaryErrors.Any() )
                {
                    _saveNavigationHistory = true;

                    if ( Using3StepGateway && paymentAmount > 0.0M )
                    {
                        string errorMessage = string.Empty;
                        if ( ProcessStep1( isPaidInFull, out errorMessage ) )
                        {
                            if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsId() ?? 0 ) > 0 )
                            {
                                hfStep2AutoSubmit.Value = "true";
                                hfIsPaidInFull.Value = isPaidInFull.ToTrueFalse();
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
                        var reservationIds = SaveChanges( isPaidInFull );
                        if ( reservationIds.Count == RegistrationInformationList.Count )
                        {
                            ShowSuccess( reservationIds );
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
                ShowStart();
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
                ShowStart();
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
                var registrationIds = SaveChanges( false );
                if ( registrationIds.Count == RegistrationInformationList.Count )
                {
                    ShowSuccess( registrationIds );
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
                        if ( ProcessStep1( hfIsPaidInFull.Value.AsBoolean(), out errorMessage ) )
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
                ShowStart();
            }

            hfTriggerScroll.Value = "true";
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
            if ( RegistrationInformationList != null )
            {
                foreach ( var registrationInformation in RegistrationInformationList )
                {
                    if ( registrationInformation.RegistrationState != null )
                    {
                        registrationInformation.RegistrationState.Registrants.ForEach( r => r.DiscountApplies = true );

                        RegistrationTemplateDiscount discount = null;
                        bool validDiscount = true;

                        string discountCode = tbDiscountCode.Text;
                        if ( !string.IsNullOrWhiteSpace( discountCode ) )
                        {
                            discount = registrationInformation.RegistrationInstanceState.RegistrationTemplate.Discounts
                                .Where( d => d.Code.Equals( discountCode, StringComparison.OrdinalIgnoreCase ) )
                                .FirstOrDefault();

                            if ( discount == null )
                            {
                                validDiscount = false;
                                nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                                nbDiscountCode.Text = string.Format( "'{0}' is not a valid {1}.", discountCode, registrationInformation.DiscountCodeTerm );
                                nbDiscountCode.Visible = true;
                            }

                            if ( validDiscount && discount.MinRegistrants.HasValue && registrationInformation.RegistrationState.RegistrantCount < discount.MinRegistrants.Value )
                            {
                                nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                                nbDiscountCode.Text = string.Format( "The '{0}' {1} requires at least {2} registrants.", discountCode, registrationInformation.DiscountCodeTerm, discount.MinRegistrants.Value );
                                nbDiscountCode.Visible = true;
                                validDiscount = false;
                            }

                            if ( validDiscount && discount.StartDate.HasValue && RockDateTime.Today < discount.StartDate.Value )
                            {
                                nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                                nbDiscountCode.Text = string.Format( "The '{0}' {1} is not available yet.", discountCode, registrationInformation.DiscountCodeTerm );
                                nbDiscountCode.Visible = true;
                                validDiscount = false;
                            }

                            if ( validDiscount && discount.EndDate.HasValue && RockDateTime.Today > discount.EndDate.Value )
                            {
                                nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                                nbDiscountCode.Text = string.Format( "The '{0}' {1} has expired.", discountCode, registrationInformation.DiscountCodeTerm );
                                nbDiscountCode.Visible = true;
                                validDiscount = false;
                            }

                            if ( validDiscount && discount.MaxUsage.HasValue && registrationInformation.RegistrationInstanceState != null )
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    var instances = new RegistrationService( rockContext )
                                        .Queryable().AsNoTracking()
                                        .Where( r =>
                                            r.RegistrationInstanceId == registrationInformation.RegistrationInstanceState.Id &&
                                            ( !registrationInformation.RegistrationState.RegistrationId.HasValue || r.Id != registrationInformation.RegistrationState.RegistrationId.Value ) &&
                                            r.DiscountCode == discountCode )
                                        .Count();
                                    if ( instances >= discount.MaxUsage.Value )
                                    {
                                        nbDiscountCode.NotificationBoxType = NotificationBoxType.Warning;
                                        nbDiscountCode.Text = string.Format( "The '{0}' {1} is no longer available.", discountCode, registrationInformation.DiscountCodeTerm );
                                        nbDiscountCode.Visible = true;
                                        validDiscount = false;
                                    }
                                }
                            }

                            if ( validDiscount && discount.MaxRegistrants.HasValue )
                            {
                                for ( int i = 0; i < registrationInformation.RegistrationState.Registrants.Count; i++ )
                                {
                                    registrationInformation.RegistrationState.Registrants[i].DiscountApplies = i < discount.MaxRegistrants.Value;
                                }
                            }
                        }
                        else
                        {
                            validDiscount = false;
                        }

                        registrationInformation.RegistrationState.DiscountCode = validDiscount ? discountCode : string.Empty;
                        registrationInformation.RegistrationState.DiscountPercentage = validDiscount ? discount.DiscountPercentage : 0.0m;
                        registrationInformation.RegistrationState.DiscountAmount = validDiscount ? discount.DiscountAmount : 0.0m;
                        IsDiscountColumnShown = validDiscount;

                        if ( CurrentRegistrationInformation.RegistrationInstanceState.Id == registrationInformation.RegistrationInstanceState.Id )
                        {
                            CurrentRegistrationInformation = registrationInformation;
                        }
                        CreateDynamicControls( true );
                    }
                }
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
            var transactionCode = RegistrationInformationList.Max( ri => ri.TransactionCode );
            if ( string.IsNullOrWhiteSpace( transactionCode ) )
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
                    if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null && FinancialGateway != null )
                    {
                        gateway = FinancialGateway.GetGatewayComponent();
                    }

                    if ( gateway != null )
                    {
                        var ccCurrencyType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) );
                        string errorMessage = string.Empty;

                        PersonAlias authorizedPersonAlias = null;
                        string referenceNumber = string.Empty;
                        FinancialPaymentDetail paymentDetail = null;
                        int? currencyTypeValueId = ccCurrencyType.Id;

                        var transaction = new FinancialTransactionService( rockContext ).GetByTransactionCode( FinancialGateway.Id, transactionCode );
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
                                    savedAccount.TransactionCode = transactionCode;
                                    savedAccount.FinancialGatewayId = FinancialGateway.Id;
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

        private bool SetCurrentRegistrationInformation()
        {
            var currentRegistrant = MultiEventRegistrants[CurrentMultiEventRegistrantIndex];
            var currentInstanceIndex = currentRegistrant.CurrentRegistrationInstanceIndex;
            int? registrationInstanceId = currentRegistrant.RegistrationInstanceIds[currentInstanceIndex];

            // Not inside a "using" due to serialization needing context to still be active
            var rockContext = new RockContext();

            var currentRegistrantPersonAlias = new PersonAliasService( rockContext ).Get( currentRegistrant.PersonAliasId );
            var currentRegistrantPerson = currentRegistrantPersonAlias.Person;

            if ( registrationInstanceId != null )
            {
                if ( RegistrationInformationList == null )
                {
                    RegistrationInformationList = new List<RegistrationInformation>();
                }

                var currentRegistrationInformation = RegistrationInformationList.Where( ri => ri.RegistrationInstanceState.Id == registrationInstanceId ).FirstOrDefault();
                if ( currentRegistrationInformation != null )
                {
                    CurrentRegistrationInformation = currentRegistrationInformation;
                }
                else
                {
                    CurrentRegistrationInformation = new RegistrationInformation();

                    // A registration instance id was specified
                    if ( CurrentRegistrationInformation.RegistrationState == null && registrationInstanceId.HasValue )
                    {
                        var dateTime = RockDateTime.Now;
                        CurrentRegistrationInformation.RegistrationInstanceState = new RegistrationInstanceService( rockContext )
                            .Queryable( "Account,RegistrationTemplate.Fees,RegistrationTemplate.Discounts,RegistrationTemplate.Forms.Fields.Attribute,RegistrationTemplate.FinancialGateway" )
                            .Where( r =>
                                r.Id == registrationInstanceId.Value &&
                                r.IsActive &&
                                r.RegistrationTemplate != null &&
                                r.RegistrationTemplate.IsActive &&
                                ( !r.StartDateTime.HasValue || r.StartDateTime <= dateTime ) &&
                                ( !r.EndDateTime.HasValue || r.EndDateTime > dateTime ) )
                            .FirstOrDefault();

                        if ( CurrentRegistrationInformation.RegistrationInstanceState != null )
                        {
                            CurrentRegistrationInformation.RegistrationState = new RegistrationInfo( currentRegistrantPerson );
                        }
                    }

                    // If registration instance id and event occurrence were specified, but a group (linkage) hasn't been loaded, find the first group for the event occurrence
                    if ( CurrentRegistrationInformation.RegistrationInstanceState != null )
                    {
                        var dateTime = RockDateTime.Now;
                        var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                            .Queryable( "RegistrationInstance.Account,RegistrationInstance.RegistrationTemplate.Fees,RegistrationInstance.RegistrationTemplate.Discounts,RegistrationInstance.RegistrationTemplate.Forms.Fields.Attribute,RegistrationInstance.RegistrationTemplate.FinancialGateway" )
                            .Where( l =>
                                l.RegistrationInstanceId == registrationInstanceId.Value &&
                                l.RegistrationInstance != null &&
                                l.RegistrationInstance.IsActive &&
                                l.RegistrationInstance.RegistrationTemplate != null &&
                                l.RegistrationInstance.RegistrationTemplate.IsActive &&
                                ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                                ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                            .FirstOrDefault();

                        if ( linkage != null )
                        {
                            CurrentRegistrationInformation.GroupId = linkage.GroupId;
                        }
                    }

                    if ( CurrentRegistrationInformation.RegistrationState != null &&
                        CurrentRegistrationInformation.RegistrationState.FamilyGuid == Guid.Empty &&
                        CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null &&
                        CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrantsSameFamily != RegistrantsSameFamily.Ask )
                    {
                        CurrentRegistrationInformation.RegistrationState.FamilyGuid = Guid.NewGuid();
                    }

                    if ( CurrentRegistrationInformation.RegistrationState != null )
                    {
                        // Calculate the available slots. If maxAttendees is null that means unlimited registrants and CurrentRegistrationInformation.RegistrationState.SlotsAvailable should not be calculated.
                        if ( !CurrentRegistrationInformation.RegistrationState.RegistrationId.HasValue && CurrentRegistrationInformation.RegistrationInstanceState != null && CurrentRegistrationInformation.RegistrationInstanceState.MaxAttendees.HasValue )
                        {
                            var existingRegistrantIds = CurrentRegistrationInformation.RegistrationState.Registrants.Select( r => r.Id ).ToList();
                            var otherRegistrantsCount = new RegistrationRegistrantService( new RockContext() )
                                .Queryable()
                                .Where( a => a.Registration.RegistrationInstanceId == registrationInstanceId && !a.Registration.IsTemporary )
                                .Where( a => !existingRegistrantIds.Contains( a.Id ) )
                                .Count();

                            int otherRegistrants = CurrentRegistrationInformation.RegistrationInstanceState.Registrations
                                .Where( r => !r.IsTemporary )
                                .Sum( r => r.Registrants.Where( t => !existingRegistrantIds.Contains( t.Id ) ).Count() );

                            CurrentRegistrationInformation.RegistrationState.SlotsAvailable = CurrentRegistrationInformation.RegistrationInstanceState.MaxAttendees - otherRegistrants;
                        }
                    }

                    if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null &&
                        FinancialGateway != null )
                    {
                        var threeStepGateway = FinancialGateway.GetGatewayComponent() as IThreeStepGatewayComponent;
                        Using3StepGateway = threeStepGateway != null;
                        if ( Using3StepGateway )
                        {
                            Step2IFrameUrl = ResolveRockUrl( threeStepGateway.Step2FormUrl );
                        }
                    }

                    if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null )
                    {
                        // NOTE, we only want to require VIEW auth for a person to be able to enter a value for an attribute since they are just entering a value for themselves (not for other people)
                        CurrentRegistrationInformation.RegistrationAttributesState = new AttributeService( rockContext ).GetByEntityTypeId( new Registration().TypeId, true ).AsQueryable()
                        .Where( a =>
                            a.EntityTypeQualifierColumn.Equals( "RegistrationTemplateId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Id.ToString() ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToAttributeCacheList()
                        .Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                        .ToList();

                        // only show the Registration Attributes Before Registrants that have a category of REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION
                        CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants = CurrentRegistrationInformation.RegistrationAttributesState.Where( a => a.Categories.Any( c => c.Guid == Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION.AsGuid() ) ).Select( a => a.Id ).ToList();

                        // only show the Registration Attributes After Registrants that have don't have a category or have a category of REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION
                        CurrentRegistrationInformation.RegistrationAttributeIdsAfterRegistrants = CurrentRegistrationInformation.RegistrationAttributesState.Where( a => !a.Categories.Any() || a.Categories.Any( c => c.Guid == Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION.AsGuid() ) ).Select( a => a.Id ).ToList();
                    }

                    CurrentRegistrationInformation.SignInline = false;
                    if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null &&
                        CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplate != null &&
                        CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplate.ProviderEntityType != null )
                    {
                        var provider = DigitalSignatureContainer.GetComponent( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplate.ProviderEntityType.Name );
                        if ( provider != null && provider.IsActive )
                        {
                            CurrentRegistrationInformation.SignInline = GetAttributeValue( "SignInline" ).AsBoolean() && CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.SignatureDocumentAction == SignatureDocumentAction.Embed;
                            CurrentRegistrationInformation.DigitalSignatureComponentTypeName = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplate.ProviderEntityType.Name;
                            CurrentRegistrationInformation.DigitalSignatureComponent = provider;
                        }
                    }

                    if ( CurrentRegistrationInformation != null )
                    {
                        RegistrationInformationList.Add( CurrentRegistrationInformation );
                    }
                }

                decimal cost = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Cost;
                if ( ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.SetCostOnInstance ?? false ) && CurrentRegistrationInformation.RegistrationInstanceState != null )
                {
                    cost = CurrentRegistrationInformation.RegistrationInstanceState.Cost ?? 0.0m;
                }

                // If this is the first registrant being added and all are in the same family, default it to the current person
                if ( currentRegistrantPerson != null )
                {
                    var registrant = new RegistrantInfo( CurrentRegistrationInformation.RegistrationInstanceState, currentRegistrantPerson );

                    foreach ( var field in CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields )
                        .Where( f =>
                            ( f.PersonFieldType == RegistrationPersonFieldType.FirstName ||
                            f.PersonFieldType == RegistrationPersonFieldType.LastName ) &&
                            f.FieldSource == RegistrationFieldSource.PersonField ) )
                    {
                        registrant.FieldValues.AddOrReplace(
                            field.Id,
                            new FieldValueObject( field, field.PersonFieldType == RegistrationPersonFieldType.FirstName ? currentRegistrantPerson.NickName : currentRegistrantPerson.LastName ) );
                    }

                    registrant.Cost = cost;
                    registrant.FamilyGuid = CurrentRegistrationInformation.RegistrationState.FamilyGuid;
                    if ( CurrentRegistrationInformation.RegistrationState.Registrants.Count >= CurrentRegistrationInformation.RegistrationState.SlotsAvailable )
                    {
                        registrant.OnWaitList = true;
                    }

                    CurrentRegistrationInformation.RegistrationState.Registrants.Add( registrant );
                }

                CurrentRegistrationInformation.CurrentRegistrantIndex = CurrentRegistrationInformation.RegistrationState.RegistrantCount - 1;

            }
            return true;
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

            // Validate payment information if there is a payment due or if there is a payment amount being provided
            if ( minimumPayment.HasValue && minimumPayment > 0.0M || paymentAmount > 0.0M )
            {
                if ( paymentAmount < minimumPayment )
                {
                    validationErrors.Add( string.Format( "Amount To Pay Today must be at least {0:C2}", CurrentRegistrationInformation.minimumPayment ) );
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
        private List<int> SaveChanges( bool isPaidInFull )
        {
            var categoryId = PageParameter( CATEGORY_ID_PARAM_NAME ).AsInteger();
            if ( categoryId == null || categoryId == 0 )
            {
                categoryId = GetAttributeValue( "DefaultCategoryId" ).AsInteger();
            }

            var category = CategoryCache.Get( categoryId );
            List<int> registrationIds = new List<int>();
            List<Registration> registrationList = new List<Registration>();
            var rockContext = new RockContext();

            try
            {
                bool hasPayment = ( paymentAmount ?? 0.0m ) > 0.0m;
                foreach ( var registrationInformation in RegistrationInformationList )
                {
                    Registration registration = null;

                    if ( registrationInformation.RegistrationState != null && registrationInformation.RegistrationState.Registrants.Any() && registrationInformation.RegistrationInstanceState.RegistrationTemplate != null )
                    {

                        var registrationService = new RegistrationService( rockContext );

                        registration = SaveRegistration( registrationInformation, rockContext, hasPayment );
                        if ( registration != null )
                        {
                            registrationList.Add( registration );
                        }
                    }
                }

                // If there is a payment being made, process the payment
                if ( hasPayment )
                {

                    string errorMessage = string.Empty;
                    if ( Using3StepGateway )
                    {
                        if ( !ProcessStep3( category, rockContext, registrationList, hfStep2ReturnQueryString.Value, isPaidInFull, out errorMessage ) )
                        {
                            throw new Exception( errorMessage );
                        }
                    }
                    else
                    {
                        if ( !ProcessPayment( category, rockContext, registrationList, isPaidInFull, out errorMessage ) )
                        {
                            throw new Exception( errorMessage );
                        }
                    }
                }

                foreach ( var registration in registrationList )
                {
                    // If there is a valid registration, and nothing went wrong processing the payment, add registrants to group and send the notifications
                    if ( registration != null && !registration.IsTemporary )
                    {
                        var registrationInformation = RegistrationInformationList.Where( ri => ri.RegistrationState.RegistrationId == registration.Id ).First();

                        ProcessPostSave( registrationInformation, registration, rockContext );
                    }

                    if ( registration != null )
                    {
                        registrationIds.Add( registration.Id );
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

                ShowError( "An error occurred processing your registration", ex.Message );

                // Try to delete the registration if it was just created
                try
                {
                    foreach ( var registration in registrationList )
                    {
                        if ( registration != null && registration.Id > 0 )
                        {
                            var registrationInformation = RegistrationInformationList.Where( ri => ri.RegistrationState.RegistrationId == registration.Id ).First();
                            registrationInformation.RegistrationState.RegistrationId = null;
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

                }
                catch { }
            }
            SaveViewState();
            return registrationIds;
        }

        /// <summary>
        /// Sends notifications after the registration is saved
        /// </summary>
        /// <param name="isNewRegistration">if set to <c>true</c> [is new registration].</param>
        /// <param name="registration">The registration.</param>
        /// <param name="previousRegistrantPersonIds">The previous registrant person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ProcessPostSave( RegistrationInformation registrationInformation, Registration registration, RockContext rockContext )
        {
            try
            {
                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    registration.SavePersonNotesAndHistory( registration.PersonAlias.Person, this.CurrentPersonAliasId, new List<int>() );
                }
                // This occurs when the registrar is logged in
                else if ( registration.PersonAliasId.HasValue )
                {
                    var registrar = new PersonAliasService( rockContext ).Get( registration.PersonAliasId.Value );
                    registration.SavePersonNotesAndHistory( registrar.Person, this.CurrentPersonAliasId, new List<int>() );
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


                // Send notice of a new registration
                var notification = new Rock.Transactions.SendRegistrationNotificationTransaction();
                notification.RegistrationId = registration.Id;
                notification.AppRoot = appRoot;
                notification.ThemeRoot = themeRoot;
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( notification );


                var registrationService = new RegistrationService( new RockContext() );
                var newRegistration = registrationService.Get( registration.Id );
                if ( newRegistration != null )
                {

                    if ( registrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue )
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

                            if ( registrationInformation.DigitalSignatureComponent != null )
                            {
                                var sendDocumentTxn = new Rock.Transactions.SendDigitalSignatureRequestTransaction();
                                sendDocumentTxn.SignatureDocumentTemplateId = registrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplateId.Value;
                                sendDocumentTxn.AppliesToPersonAliasId = registrant.PersonAlias.Id;
                                sendDocumentTxn.AssignedToPersonAliasId = assignedTo.PrimaryAliasId ?? 0;
                                sendDocumentTxn.DocumentName = string.Format( "{0}_{1}", registrationInformation.RegistrationInstanceState.Name.RemoveSpecialCharacters(), registrant.PersonAlias.Person.FullName.RemoveSpecialCharacters() );
                                sendDocumentTxn.Email = email;
                                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( sendDocumentTxn );
                            }
                        }
                    }

                    newRegistration.LaunchWorkflow( registrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrationWorkflowTypeId, newRegistration.ToString() );
                    newRegistration.LaunchWorkflow( registrationInformation.RegistrationInstanceState.RegistrationWorkflowTypeId, newRegistration.ToString() );


                    registrationInformation.RegistrationInstanceState = newRegistration.RegistrationInstance;
                    registrationInformation.RegistrationState = new RegistrationInfo( newRegistration, rockContext );
                    registrationInformation.RegistrationState.PreviousPaymentTotal = registrationService.GetTotalPayments( registration.Id );
                }
            }
            catch ( Exception postSaveEx )
            {
                ShowWarning( "The following occurred after processing your " + registrationInformation.RegistrationTerm, postSaveEx.Message );
                ExceptionLogService.LogException( postSaveEx, Context, RockPage.PageId, RockPage.Layout.SiteId, CurrentPersonAlias );
            }
        }

        /// <summary>
        /// Saves the registration.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="hasPayment">if set to <c>true</c> [has payment].</param>
        /// <returns></returns>
        private Registration SaveRegistration( RegistrationInformation registrationInformation, RockContext rockContext, bool hasPayment )
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

            if ( registrationInformation.RegistrationState.RegistrationId.HasValue )
            {
                registration = registrationService.Get( registrationInformation.RegistrationState.RegistrationId.Value );
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

            registration.RegistrationInstanceId = registrationInformation.RegistrationInstanceState.Id;

            // If the Registration Instance linkage specified a group, load it now
            Group group = null;
            if ( registrationInformation.GroupId.HasValue )
            {
                group = new GroupService( rockContext ).Get( registrationInformation.GroupId.Value );
                if ( group != null && ( !registration.GroupId.HasValue || registration.GroupId.Value != group.Id ) )
                {
                    registration.GroupId = group.Id;
                    History.EvaluateChange( registrationChanges, "Group", string.Empty, group.Name );
                }
            }

            bool newRegistrar = newRegistration ||
                registration.FirstName == null || !registration.FirstName.Equals( registrationInformation.RegistrationState.FirstName, StringComparison.OrdinalIgnoreCase ) ||
                registration.LastName == null || !registration.LastName.Equals( registrationInformation.RegistrationState.LastName, StringComparison.OrdinalIgnoreCase );

            History.EvaluateChange( registrationChanges, "First Name", registration.FirstName, registrationInformation.RegistrationState.FirstName );
            registration.FirstName = registrationInformation.RegistrationState.FirstName;

            History.EvaluateChange( registrationChanges, "Last Name", registration.LastName, registrationInformation.RegistrationState.LastName );
            registration.LastName = registrationInformation.RegistrationState.LastName;

            History.EvaluateChange( registrationChanges, "Confirmation Email", registration.ConfirmationEmail, registrationInformation.RegistrationState.ConfirmationEmail );
            registration.ConfirmationEmail = registrationInformation.RegistrationState.ConfirmationEmail;

            History.EvaluateChange( registrationChanges, "Discount Code", registration.DiscountCode, registrationInformation.RegistrationState.DiscountCode );
            registration.DiscountCode = registrationInformation.RegistrationState.DiscountCode;

            History.EvaluateChange( registrationChanges, "Discount Percentage", registration.DiscountPercentage, registrationInformation.RegistrationState.DiscountPercentage );
            registration.DiscountPercentage = registrationInformation.RegistrationState.DiscountPercentage;

            History.EvaluateChange( registrationChanges, "Discount Amount", registration.DiscountAmount, registrationInformation.RegistrationState.DiscountAmount );
            registration.DiscountAmount = registrationInformation.RegistrationState.DiscountAmount;

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
                if ( CurrentPerson != null )
                {
                    registrar = CurrentPerson;
                    registration.PersonAliasId = CurrentPerson.PrimaryAliasId;
                    registration.FirstName = CurrentPerson.FirstName;
                    registration.LastName = CurrentPerson.LastName;
                    registration.ConfirmationEmail = CurrentPerson.Email;

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
                    multipleFamilyGroupIds.AddOrIgnore( registrationInformation.RegistrationState.FamilyGuid, family.Id );
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

                registrar = SavePerson( rockContext, person, registrationInformation.RegistrationState.FamilyGuid, registrationInformation.CampusId, null, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId );
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
            registrationInformation.RegistrationState.RegistrationId = registration.Id;

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
                foreach ( var registrantInfo in registrationInformation.RegistrationState.Registrants.ToList() )
                {
                    var registrantChanges = new History.HistoryChangeList();
                    var personChanges = new History.HistoryChangeList();

                    RegistrationRegistrant registrant = null;
                    Person person = null;

                    string firstName = registrantInfo.GetFirstName( registrationInformation.RegistrationInstanceState.RegistrationTemplate );
                    string lastName = registrantInfo.GetLastName( registrationInformation.RegistrationInstanceState.RegistrationTemplate );
                    string email = registrantInfo.GetEmail( registrationInformation.RegistrationInstanceState.RegistrationTemplate );

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
                        if ( registrantInfo.PersonId.HasValue && registrationInformation.RegistrationInstanceState.RegistrationTemplate.ShowCurrentFamilyMembers )
                        {
                            person = personService.Get( registrantInfo.PersonId.Value );
                        }
                    }

                    if ( person == null )
                    {
                        // Try to find a matching person based on name and email address
                        person = personService.FindPerson( firstName, lastName, email, true );

                        // Try to find a matching person based on name within same family as registrar
                        if ( person == null && registrar != null && registrantInfo.FamilyGuid == registrationInformation.RegistrationState.FamilyGuid )
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

                    int? campusId = registrationInformation.CampusId;
                    Location location = null;

                    // Set any of the template's person fields
                    foreach ( var field in registrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms
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
                    foreach ( var field in registrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms
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
                                var registrationTemplateFeeItem = uiFeeOption.RegistrationTemplateFeeItemId != null ? registrationTemplateFeeItemService.GetNoTracking( uiFeeOption.RegistrationTemplateFeeItemId.Value ) : null;
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
                                templateFee = registrationInformation.RegistrationInstanceState.RegistrationTemplate.Fees.Where( f => f.Id == uiFee.Key ).FirstOrDefault();
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
                    foreach ( var field in registrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms
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
                        if ( registrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplateId.HasValue && !string.IsNullOrWhiteSpace( registrantInfo.SignatureDocumentKey ) )
                        {
                            var document = new SignatureDocument();
                            document.SignatureDocumentTemplateId = registrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplateId.Value;
                            document.DocumentKey = registrantInfo.SignatureDocumentKey;
                            document.Name = string.Format( "{0}_{1}", registrationInformation.RegistrationInstanceState.Name.RemoveSpecialCharacters(), person.FullName.RemoveSpecialCharacters() );
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
                        ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask && multipleFamilyGroupIds.ContainsKey( familyGuid ) ) ||
                        ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes && singleFamilyId.HasValue )
                    )
                {
                    // Add person to existing family
                    var age = person.Age;
                    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;

                    familyId = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask ?
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

                var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                if ( homeLocationType != null && familyGroup != null )
                {
                    if ( existingLocation != null )
                    {
                        // A location exists but is not associated with this family group
                        GroupService.AddNewGroupAddress( rockContext, familyGroup, Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, existingLocation );
                    }
                    else
                    {
                        // Create a new location and save it to the family group
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

                            if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.GroupTypeId.HasValue &&
                                CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.GroupTypeId == group.GroupTypeId &&
                                CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.GroupMemberRoleId.HasValue )
                            {
                                groupMember.GroupRoleId = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.GroupMemberRoleId.Value;
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

                        groupMember.GroupMemberStatus = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.GroupMemberStatus;

                        rockContext.SaveChanges();

                        registrant.GroupMemberId = groupMember != null ? groupMember.Id : ( int? ) null;
                        rockContext.SaveChanges();

                        // Set any of the template's group member attributes
                        groupMember.LoadAttributes();

                        var registrantInfo = CurrentRegistrationInformation.RegistrationState.Registrants.FirstOrDefault( r => r.Guid == registrant.Guid );
                        if ( registrantInfo != null )
                        {
                            foreach ( var field in CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms
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
        private bool ProcessPayment( CategoryCache category, RockContext rockContext, List<Registration> registrationList, bool isPaidInFull, out string errorMessage )
        {
            var isTransactionSaved = true;
            GatewayComponent gateway = null;
            if ( FinancialGateway != null )
            {
                gateway = FinancialGateway.GetGatewayComponent();
            }

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( FinancialAccount.Id == null || FinancialAccount == null )
            {
                errorMessage = "There was a problem with the account configuration for this registration";
                return false;
            }

            var adjustedPaymentAmount = paymentAmount;
            if ( !isPaidInFull )
            {
                adjustedPaymentAmount = minimumPayment;
            }

            PaymentInfo paymentInfo = null;
            if ( rblSavedCC.Items.Count > 0 && ( rblSavedCC.SelectedValueAsId() ?? 0 ) > 0 )
            {
                var savedAccount = new FinancialPersonSavedAccountService( rockContext ).Get( rblSavedCC.SelectedValueAsId().Value );
                if ( savedAccount != null )
                {
                    paymentInfo = savedAccount.GetReferencePayment();
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

            paymentInfo.Amount = adjustedPaymentAmount ?? 0.0m;
            paymentInfo.Comment1 = string.Format( "{0} ({1})", category.Name, FinancialAccount.GlCode );

            var financialGateway = new FinancialGatewayService( rockContext ).Get( FinancialGateway.Id );
            var transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );

            isTransactionSaved = SaveTransaction( category, gateway, registrationList, transaction, paymentInfo, rockContext, isPaidInFull );

            if ( !isPaidInFull )
            {
                var referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                var paymentDetail = transaction.FinancialPaymentDetail;
                var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );

                var tempSavedAccount = new FinancialPersonSavedAccount();
                tempSavedAccount.PersonAliasId = transaction.AuthorizedPersonAliasId;
                tempSavedAccount.ReferenceNumber = referenceNumber;
                tempSavedAccount.Name = txtSaveAccount.Text;
                tempSavedAccount.TransactionCode = CurrentRegistrationInformation.TransactionCode;
                tempSavedAccount.FinancialGatewayId = FinancialGateway.Id;
                tempSavedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
                tempSavedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentDetail.AccountNumberMasked;
                tempSavedAccount.FinancialPaymentDetail.CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId;
                tempSavedAccount.FinancialPaymentDetail.CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId;
                tempSavedAccount.FinancialPaymentDetail.NameOnCardEncrypted = paymentDetail.NameOnCardEncrypted;
                tempSavedAccount.FinancialPaymentDetail.ExpirationMonthEncrypted = paymentDetail.ExpirationMonthEncrypted;
                tempSavedAccount.FinancialPaymentDetail.ExpirationYearEncrypted = paymentDetail.ExpirationYearEncrypted;
                tempSavedAccount.FinancialPaymentDetail.BillingLocationId = paymentDetail.BillingLocationId;

                foreach ( var registration in registrationList )
                {
                    var registrationInformation = RegistrationInformationList.Where( ri => ri.RegistrationState.RegistrationId == registration.Id ).First();

                    var futurePaymentAmount = registrationInformation.RegistrationState.PaymentAmount - registrationInformation.minimumPayment ?? 0.0m;
                    if ( futurePaymentAmount > 0 )
                    {
                        DateTime? paymentDate = GetPaymentDate( registration );

                        // Get the payment schedule
                        var schedule = new PaymentSchedule();
                        schedule.TransactionFrequencyValue = oneTimeFrequency;

                        if ( paymentDate.HasValue && paymentDate > RockDateTime.Today )
                        {
                            schedule.StartDate = paymentDate.Value;
                        }
                        else
                        {
                            schedule.StartDate = DateTime.MinValue;
                        }

                        PaymentInfo futurePaymentInfo = tempSavedAccount.GetReferencePayment();
                        futurePaymentInfo.Amount = futurePaymentAmount;
                        futurePaymentInfo.FirstName = registration.FirstName;
                        futurePaymentInfo.LastName = registration.LastName;
                        futurePaymentInfo.IPAddress = GetClientIpAddress();
                        var scheduledTransaction = gateway.AddScheduledPayment( FinancialGateway, schedule, futurePaymentInfo, out errorMessage );
                        if ( scheduledTransaction == null )
                        {
                            return false;
                        }

                        SaveScheduledTransaction( FinancialGateway, gateway, registration, registrationInformation, futurePaymentInfo, schedule, scheduledTransaction, rockContext, paymentDate.Value );
                    }

                }
            }

            return isTransactionSaved;
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
        private bool SaveTransaction( CategoryCache category, GatewayComponent gateway, List<Registration> registrationList, FinancialTransaction transaction, PaymentInfo paymentInfo, RockContext rockContext, bool isPaidInFull = false )
        {
            var adjustedPaymentAmount = paymentAmount;
            if ( !isPaidInFull )
            {
                adjustedPaymentAmount = minimumPayment;
            }

            var firstRegistration = registrationList.FirstOrDefault();
            if ( firstRegistration != null )
            {
                if ( transaction != null )
                {
                    var registrationChangeDictionary = new Dictionary<int, History.HistoryChangeList>();

                    transaction.AuthorizedPersonAliasId = firstRegistration.PersonAliasId;
                    transaction.TransactionDateTime = RockDateTime.Now;
                    transaction.FinancialGatewayId = FinancialGateway.Id;

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
                    StringBuilder sb = new StringBuilder();
                    foreach ( var registration in registrationList )
                    {
                        var registrationInformation = RegistrationInformationList.Where( ri => ri.RegistrationState.RegistrationId == registration.Id ).First();

                        var registrationPaymentAmount = registrationInformation.RegistrationState.PaymentAmount;
                        if ( !isPaidInFull )
                        {
                            registrationPaymentAmount = registrationInformation.minimumPayment;
                        }

                        if ( registrationPaymentAmount != null && registrationPaymentAmount > 0 )
                        {
                            var registrationChanges = new History.HistoryChangeList();
                            registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Payment" ).SetNewValue( string.Format( "{0} payment", ( registrationPaymentAmount ?? 0.0m ).FormatAsCurrency() ) );

                            registrationChangeDictionary.AddOrReplace( registration.Id, registrationChanges );
                            sb.AppendLine( GetRegistrationSummary( registration, registrationInformation, registrationInformation.RegistrationInstanceState, isPaidInFull ) );
                            sb.AppendLine();

                            var transactionDetail = new FinancialTransactionDetail();
                            transactionDetail.Amount = registrationPaymentAmount ?? 0.0m;
                            transactionDetail.AccountId = FinancialAccount.Id;
                            transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
                            transactionDetail.EntityId = registration.Id;
                            transaction.TransactionDetails.Add( transactionDetail );
                        }

                    }

                    transaction.Summary = sb.ToString();

                    var batchChanges = new History.HistoryChangeList();
                    rockContext.WrapTransaction( () =>
                    {
                        var batchService = new FinancialBatchService( rockContext );

                        // determine batch prefix
                        string batchPrefix = string.Empty;
                        if ( !string.IsNullOrWhiteSpace( category.GetAttributeValue( "BatchNamePrefix" ) ) )
                        {
                            batchPrefix = category.GetAttributeValue( "BatchNamePrefix" );
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
                                FinancialGateway.GetBatchTimeOffset() );

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

                    foreach ( var registrationChanges in registrationChangeDictionary )
                    {
                        Task.Run( () =>
                        HistoryService.SaveChanges(
                            new RockContext(),
                            typeof( Registration ),
                            Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                            registrationChanges.Key,
                            registrationChanges.Value,
                            true,
                            CurrentPersonAliasId ) );
                    }

                    foreach ( var registrationInformation in RegistrationInformationList )
                    {
                        registrationInformation.TransactionCode = transaction.TransactionCode;
                    }
                    SaveViewState();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public string GetRegistrationSummary( Registration registration, RegistrationInformation registrationInformation, RegistrationInstance registrationInstance = null, bool isPaidInFull = true, bool isInitialPayment = true )
        {
            var result = new StringBuilder();
            result.Append( "Event registration payment" );

            var instance = registrationInstance ?? registration.RegistrationInstance;
            if ( instance != null )
            {
                result.AppendFormat( " for {0} [ID:{1}]", instance.Name, instance.Id );
                if ( instance.RegistrationTemplate != null )
                {
                    result.AppendFormat( " (Template: {0} [ID:{1}])", instance.RegistrationTemplate.Name, instance.RegistrationTemplate.Id );
                }
            }

            string registrationPerson = registration.PersonAlias != null && registration.PersonAlias.Person != null ?
                registration.PersonAlias.Person.FullName :
                string.Format( "{0} {1}", registration.FirstName, registration.LastName );
            if ( isPaidInFull )
            {
                result.AppendFormat( @".
Registration By: {0} \nTotal Cost/Fees:{1}
", registrationPerson, registration.DiscountedCost.FormatAsCurrency() );
            }
            else
            {
                if ( isInitialPayment )
                {
                    result.AppendFormat( @".
            Registration By: {0} \nTotal Cost/Fees:{1}
            ", registrationPerson, registrationInformation.minimumPayment.FormatAsCurrency() );
                }
                else
                {
                    var summaryAmount = registrationInformation.RegistrationState.PaymentAmount - registrationInformation.minimumPayment ?? 0.0m;
                    result.AppendFormat( @".
            Registration By: {0} \nTotal Cost/Fees:{1}
            ", registrationPerson, summaryAmount.FormatAsCurrency() );
                }

            }

            return result.ToString();
        }

        /// <summary>
        /// Processes the first step of a 3-step charge.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool ProcessStep1( bool isPaidInFull, out string errorMessage )
        {
            var categoryId = PageParameter( CATEGORY_ID_PARAM_NAME ).AsInteger();
            if ( categoryId == null || categoryId == 0 )
            {
                categoryId = GetAttributeValue( "DefaultCategoryId" ).AsInteger();
            }

            var category = CategoryCache.Get( categoryId );

            var adjustedPaymentAmount = paymentAmount;
            if ( !isPaidInFull )
            {
                adjustedPaymentAmount = minimumPayment;
            }

            IThreeStepGatewayComponent gateway = null;
            if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null && FinancialGateway != null )
            {
                gateway = FinancialGateway.GetGatewayComponent() as IThreeStepGatewayComponent;
            }

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( FinancialAccount == null || FinancialAccount.Id == null )
            {
                errorMessage = "There was a problem with the account configuration for this " + CurrentRegistrationInformation.RegistrationTerm.ToLower();
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
                    paymentInfo.Amount = adjustedPaymentAmount ?? 0.0m;
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
                paymentInfo.Amount = adjustedPaymentAmount ?? 0.0m;
                paymentInfo.Email = CurrentRegistrationInformation.RegistrationState.ConfirmationEmail;

                paymentInfo.FirstName = CurrentRegistrationInformation.RegistrationState.FirstName;
                paymentInfo.LastName = CurrentRegistrationInformation.RegistrationState.LastName;
            }

            paymentInfo.Description = string.Format( "{0} ({1})", category.Name, FinancialAccount.GlCode );
            paymentInfo.IPAddress = GetClientIpAddress();
            paymentInfo.AdditionalParameters = gateway.GetStep1Parameters( ResolveRockUrlIncludeRoot( "~/GatewayStep2Return.aspx" ) );

            var result = gateway.ChargeStep1( FinancialGateway, paymentInfo, out errorMessage );
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
        private bool ProcessStep3( CategoryCache category, RockContext rockContext, List<Registration> registrationList, string resultQueryString, bool isPaidInFull, out string errorMessage )
        {
            bool isTransactionSaved = true;
            GatewayComponent gateway = null;
            IThreeStepGatewayComponent threeStepGatewayComponent = null;

            if ( FinancialGateway != null )
            {
                gateway = FinancialGateway.GetGatewayComponent();
                threeStepGatewayComponent = gateway as IThreeStepGatewayComponent;
            }

            if ( threeStepGatewayComponent == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return false;
            }

            if ( FinancialAccount == null || FinancialAccount.Id == null )
            {
                errorMessage = "There was a problem with the account configuration for this registration";
                return false;
            }

            var transaction = threeStepGatewayComponent.ChargeStep3( FinancialGateway, resultQueryString, out errorMessage );

            if ( !isPaidInFull )
            {

                isTransactionSaved = isTransactionSaved && SaveTransaction( category, gateway, registrationList, transaction, null, rockContext, isPaidInFull );
                var referenceNumber = gateway.GetReferenceNumber( transaction, out errorMessage );
                var paymentDetail = transaction.FinancialPaymentDetail;
                var oneTimeFrequency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_ONE_TIME );

                var tempSavedAccount = new FinancialPersonSavedAccount();
                tempSavedAccount.PersonAliasId = transaction.AuthorizedPersonAliasId;
                tempSavedAccount.ReferenceNumber = referenceNumber;
                tempSavedAccount.Name = txtSaveAccount.Text;
                tempSavedAccount.TransactionCode = CurrentRegistrationInformation.TransactionCode;
                tempSavedAccount.FinancialGatewayId = FinancialGateway.Id;
                tempSavedAccount.FinancialPaymentDetail = new FinancialPaymentDetail();
                tempSavedAccount.FinancialPaymentDetail.AccountNumberMasked = paymentDetail.AccountNumberMasked;
                tempSavedAccount.FinancialPaymentDetail.CurrencyTypeValueId = paymentDetail.CurrencyTypeValueId;
                tempSavedAccount.FinancialPaymentDetail.CreditCardTypeValueId = paymentDetail.CreditCardTypeValueId;
                tempSavedAccount.FinancialPaymentDetail.NameOnCardEncrypted = paymentDetail.NameOnCardEncrypted;
                tempSavedAccount.FinancialPaymentDetail.ExpirationMonthEncrypted = paymentDetail.ExpirationMonthEncrypted;
                tempSavedAccount.FinancialPaymentDetail.ExpirationYearEncrypted = paymentDetail.ExpirationYearEncrypted;
                tempSavedAccount.FinancialPaymentDetail.BillingLocationId = paymentDetail.BillingLocationId;

                foreach ( var registration in registrationList )
                {
                    var registrationInformation = RegistrationInformationList.Where( ri => ri.RegistrationState.RegistrationId == registration.Id ).First();

                    DateTime? paymentDate = GetPaymentDate( registration );

                    // Get the payment schedule
                    var schedule = new PaymentSchedule();
                    schedule.TransactionFrequencyValue = oneTimeFrequency;

                    if ( paymentDate.HasValue && paymentDate > RockDateTime.Today )
                    {
                        schedule.StartDate = paymentDate.Value;
                    }
                    else
                    {
                        schedule.StartDate = DateTime.MinValue;
                    }

                    PaymentInfo paymentInfo = tempSavedAccount.GetReferencePayment();
                    paymentInfo.Amount = registrationInformation.RegistrationState.PaymentAmount - registrationInformation.minimumPayment ?? 0.0m;
                    paymentInfo.FirstName = registration.FirstName;
                    paymentInfo.LastName = registration.LastName;
                    paymentInfo.IPAddress = GetClientIpAddress();
                    paymentInfo.AdditionalParameters = threeStepGatewayComponent.GetStep1Parameters( ResolveRockUrlIncludeRoot( "~/GatewayStep2Return.aspx" ) );

                    var result = threeStepGatewayComponent.AddScheduledPaymentStep1( FinancialGateway, schedule, paymentInfo, out errorMessage );
                    hfStep2AutoSubmit.Value = "true";
                    var scheduledResultQueryString = "";
                    var scheduledTransaction = threeStepGatewayComponent.AddScheduledPaymentStep3( FinancialGateway, scheduledResultQueryString, out errorMessage );
                    if ( scheduledTransaction == null )
                    {
                        return false;
                    }

                    paymentDetail = scheduledTransaction.FinancialPaymentDetail.Clone( false );
                    SaveScheduledTransaction( FinancialGateway, gateway, registration, registrationInformation, paymentInfo, schedule, scheduledTransaction, rockContext, paymentDate.Value );

                }

            }
            else
            {
                isTransactionSaved = isTransactionSaved && SaveTransaction( category, gateway, registrationList, transaction, null, rockContext, isPaidInFull );

            }


            return isTransactionSaved;
        }

        private void SaveScheduledTransaction( FinancialGateway financialGateway, GatewayComponent gateway, Registration registration, RegistrationInformation registrationInformation, PaymentInfo paymentInfo, PaymentSchedule schedule, FinancialScheduledTransaction scheduledTransaction, RockContext rockContext, DateTime paymentDate )
        {
            scheduledTransaction.TransactionFrequencyValueId = schedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = schedule.StartDate;
            scheduledTransaction.AuthorizedPersonAliasId = registration.PersonAliasId.Value;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;

            var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
            scheduledTransaction.TransactionTypeValueId = txnType.Id;

            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "Source" ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    scheduledTransaction.SourceTypeValueId = source.Id;
                }
            }

            var changeSummary = new StringBuilder();
            changeSummary.AppendFormat( "{0} starting {1}", schedule.TransactionFrequencyValue.Value, schedule.StartDate.ToShortDateString() );
            changeSummary.AppendLine();
            changeSummary.Append( GetRegistrationSummary( registration, registrationInformation, registrationInformation.RegistrationInstanceState, false, false ) );

            scheduledTransaction.Summary = changeSummary.ToString();

            var transactionDetail = new FinancialScheduledTransactionDetail();
            transactionDetail.Amount = registrationInformation.RegistrationState.PaymentAmount - registrationInformation.minimumPayment ?? 0.0m;
            transactionDetail.AccountId = FinancialAccount.Id;
            transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            transactionDetail.EntityId = registration.Id;
            scheduledTransaction.ScheduledTransactionDetails.Add( transactionDetail );

            var registrationChanges = new History.HistoryChangeList();
            registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Scheduled Payment" ).SetNewValue( string.Format( "{0} scheduled payment on {1}", scheduledTransaction.TotalAmount.FormatAsCurrency(), paymentDate.ToShortDateString() ) );
            Task.Run( () =>
                HistoryService.SaveChanges(
                    new RockContext(),
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registration.Id,
                    registrationChanges,
                    true,
                    CurrentPersonAliasId ) );


            var transactionService = new FinancialScheduledTransactionService( rockContext );
            transactionService.Add( scheduledTransaction );
            rockContext.SaveChanges();
        }

        private static DateTime? GetPaymentDate( Registration registration )
        {
            var registrationInstance = registration.RegistrationInstance;
            if ( registrationInstance == null )
            {
                registrationInstance = new RegistrationInstanceService( new RockContext() ).Get( registration.RegistrationInstanceId );
            }
            registrationInstance.LoadAttributes();
            var paymentDate = registrationInstance.GetAttributeValue( "PaymentDate" ).AsDateTime();
            if ( paymentDate == null )
            {
                paymentDate = registrationInstance.EndDateTime;
            }
            if ( paymentDate < RockDateTime.Now )
            {
                paymentDate = RockDateTime.Now.AddDays( 1 );
            }

            return paymentDate;
        }

        /// <summary>
        /// Creates a CreditCardPaymentInfo obj using data in the UI and CurrentRegistrationInformation.RegistrationState
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

            ccPaymentInfo.Email = lUseLoggedInPersonEmail.Text;

            ccPaymentInfo.FirstName = txtCardFirstName.Text;
            ccPaymentInfo.LastName = txtCardLastName.Text;

            return ccPaymentInfo;
        }

        #endregion

        #region Display Methods

        private bool ShowInstructions()
        {
            string instructions = string.IsNullOrEmpty( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationInstructions ) ?
                CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrationInstructions :
                CurrentRegistrationInformation.RegistrationInstanceState.RegistrationInstructions;

            if ( !string.IsNullOrEmpty( instructions ) )
            {
                lInstructions.Text = string.Format( "<div class='text-left'>{0}</div>", instructions );
            }

            return instructions.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Shows the how many panel
        /// </summary>
        private void ShowStart()
        {
            SetPanel( PanelIndex.PanelStart );

            var categoryId = PageParameter( CATEGORY_ID_PARAM_NAME ).AsIntegerOrNull();
            if ( categoryId == null || categoryId == 0 )
            {
                categoryId = GetAttributeValue( "DefaultCategoryId" ).AsInteger();
            }

            if ( categoryId != null )
            {
                var category = CategoryCache.Get( categoryId.Value );
                if ( category != null )
                {
                    lPreHtmlInstructions.Text = GetAttributeValue( "StartPagePreInstructions" );
                    lPostHtmlInstructions.Text = GetAttributeValue( "StartPagePostInstructions" );
                    lCategoryName.Text = category.Name;
                    lCategoryDetails.Text = category.Description;

                    var activeRegistrationInstances = new RegistrationInstanceService( new RockContext() ).Queryable().AsNoTracking().Where( ri =>
                          ri.RegistrationTemplate.CategoryId == categoryId &&
                          ri.RegistrationTemplate.IsActive &&
                          ri.IsActive &&
                          ri.StartDateTime <= RockDateTime.Now &&
                          ri.EndDateTime >= RockDateTime.Now );

                    bool isChildrenOnly = true;
                    bool isGradeRequired = false;
                    foreach ( var activeRegistrationInstance in activeRegistrationInstances )
                    {
                        activeRegistrationInstance.LoadAttributes();
                        var instanceChildrenOnly = activeRegistrationInstance.GetAttributeValue( "ChildrenOnly" ).AsBoolean();
                        var instanceGradeRange = activeRegistrationInstance.GetAttributeValue( "LimitByGrade" );

                        if ( instanceChildrenOnly == false )
                        {
                            isChildrenOnly = false;
                        }
                        if ( instanceGradeRange.IsNotNullOrWhiteSpace() )
                        {
                            isGradeRequired = true;
                        }
                    }

                    hfGradeRequired.Value = isGradeRequired.ToString();

                    List<GroupMember> familyMembers = CurrentPerson.GetFamilyMembers( true )
                        .Where( gm => !isChildrenOnly || gm.GroupRole.Guid.ToString() == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD )
                        .ToList();

                    DateTime? minimumBirthDate = category.GetAttributeValue( "MinimumDateofBirth" ).AsDateTime();
                    if ( minimumBirthDate.HasValue )
                    {
                        familyMembers = familyMembers.Where( fm => fm.Person.BirthDate <= minimumBirthDate.Value ).ToList();
                    }

                    rFamilyMembers.DataSource = familyMembers;
                    rFamilyMembers.DataBind();

                }
            }

        }

        protected void rFamilyMembers_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var groupMember = e.Item.DataItem as GroupMember;
            if ( groupMember != null )
            {
                var hfPersonAliasId = e.Item.FindControl( "hfPersonAliasId" ) as HiddenField;
                var cbFamilyMember = e.Item.FindControl( "cbFamilyMember" ) as CheckBox;
                var lbEditGroupMember = e.Item.FindControl( "lbEditGroupMember" ) as LinkButton;

                var gradeFormatted = groupMember.Person.GradeFormatted;
                if ( gradeFormatted.IsNullOrWhiteSpace() && hfGradeRequired.Value.AsBoolean() )
                {
                    gradeFormatted = "<b style='color:red;'>Grade Required</b>";
                    lbEditGroupMember.Visible = true;
                }

                hfPersonAliasId.Value = groupMember.Person.PrimaryAliasId.ToString();
                cbFamilyMember.Text = String.Format( "{0}&nbsp;&nbsp;&nbsp;&nbsp;{1}&nbsp;&nbsp;&nbsp;&nbsp;{2}",
                    groupMember.Person.NickName,
                    groupMember.Person.Age.HasValue ? string.Format( "Age({0})", groupMember.Person.Age.ToString() ) : "",
                    gradeFormatted );

                if ( MultiEventRegistrants != null && MultiEventRegistrants.Any( mer => mer.PersonAliasId == groupMember.Person.PrimaryAliasId ) )
                {
                    cbFamilyMember.Checked = true;
                }
            }
        }

        protected void lbAddGroupMember_Click( object sender, EventArgs e )
        {
            ShowEditPersonDetails( null );
        }

        protected void rFamilyMembers_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            // the grid's value should be bound to the member's GUID.
            var memberGuid = e.CommandArgument.ToString().AsGuid();
            ShowEditPersonDetails( memberGuid );
        }

        protected void ShowEditPersonDetails( Guid? memberGuid )
        {
            var familyType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var childRole = familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) );

            pnlEditFamilyMember.Visible = rtbMemberFirstName.Enabled = rtbMemberLastName.Enabled = rtbMemberRelationship.Enabled = dpMemberBirthDate.Enabled = dvpMemberSchool.Enabled = dvpMemberGrade.Enabled = true;

            rtbMemberFirstName.Text = rtbMemberLastName.Text = String.Empty;
            rtbMemberRelationship.GroupRoleId = childRole.Id;
            dpMemberBirthDate.SelectedDate = null;
            dvpMemberSchool.SetValue( ( int? ) null );
            dvpMemberGrade.SetValue( ( int? ) null );


            var rockContext = new RockContext();
            var memberService = new GroupMemberService( rockContext );
            if ( memberGuid.HasValue )
            {
                var member = memberService.Get( memberGuid.Value );
                if ( member != null )
                {
                    hfMemberId.Value = member.Id.ToString();
                    var person = member.Person;
                    person.LoadAttributes();

                    rtbMemberFirstName.Text = person.FirstName;
                    rtbMemberLastName.Text = person.LastName;
                    rtbMemberRelationship.GroupRoleId = member.GroupRoleId;

                    rtbMemberFirstName.Enabled = rtbMemberLastName.Enabled = rtbMemberRelationship.Enabled = false;

                    if ( person.BirthDate.HasValue )
                    {
                        dpMemberBirthDate.SelectedDate = person.BirthDate;
                        dpMemberBirthDate.Enabled = false;
                    }

                    var memberSchoolValue = person.GetAttributeValue( "School" ).AsGuidOrNull();
                    if ( memberSchoolValue.HasValue )
                    {
                        var memberSchool = DefinedValueCache.Get( memberSchoolValue.Value );
                        if ( memberSchool != null )
                        {
                            dvpMemberSchool.SelectedDefinedValueId = memberSchool.Id;
                            dvpMemberSchool.Enabled = false;
                        }
                    }

                    var gradeOffset = person.GradeOffset;
                    if ( gradeOffset.HasValue && gradeOffset >= 0 )
                    {
                        var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
                        if ( schoolGrades != null )
                        {
                            var sortedGradeValues = schoolGrades.DefinedValues.OrderBy( a => a.Value.AsInteger() );
                            var schoolGradeValue = sortedGradeValues.Where( a => a.Value.AsInteger() >= gradeOffset.Value ).FirstOrDefault();
                            if ( schoolGradeValue != null )
                            {
                                dvpMemberGrade.SelectedGradeValue = schoolGradeValue;
                                dvpMemberGrade.Enabled = false;
                            }
                        }
                    }

                    imgPhoto.BinaryFileId = person.PhotoId;
                    imgPhoto.NoPictureUrl = Person.GetPersonNoPictureUrl( person, 200, 200 );

                }
            }

            var onlyAllowAddingChildren = GetAttributeValue( "OnlyAllowAddingChildren" ).AsBoolean();
            if ( onlyAllowAddingChildren )
            {
                rtbMemberRelationship.Visible = false;
            }
            List<ListItem> removeList = new List<ListItem>();
            var selectableGradeRange = GetAttributeValue( "SelectableGradeRange" );

            if ( selectableGradeRange.IsNotNullOrWhiteSpace() )
            {
                var gradeOffsetRangePair = selectableGradeRange.Split( new char[] { ',' }, StringSplitOptions.None ).AsGuidOrNullList().ToArray();
                DefinedValueCache minGradeDefinedValue = null;
                DefinedValueCache maxGradeDefinedValue = null;
                if ( gradeOffsetRangePair.Length == 2 )
                {
                    minGradeDefinedValue = gradeOffsetRangePair[0].HasValue ? DefinedValueCache.Get( gradeOffsetRangePair[0].Value ) : null;
                    maxGradeDefinedValue = gradeOffsetRangePair[1].HasValue ? DefinedValueCache.Get( gradeOffsetRangePair[1].Value ) : null;
                }

                if ( minGradeDefinedValue != null )
                {
                    int? minGradeOffset = minGradeDefinedValue.Value.AsIntegerOrNull();
                    if ( minGradeOffset.HasValue )
                    {
                        foreach ( ListItem item in dvpMemberGrade.Items )
                        {
                            if ( !item.Selected )
                            {
                                var gradeValue = DefinedValueCache.Get( item.Value.AsGuid() );
                                if ( gradeValue != null )
                                {
                                    var gradeOffset = gradeValue.Value.AsIntegerOrNull();
                                    if ( !gradeOffset.HasValue || gradeOffset > minGradeOffset.Value )
                                    {
                                        removeList.Add( item );
                                    }
                                }

                            }
                        }
                    }
                }

                if ( maxGradeDefinedValue != null )
                {
                    int? maxGradeOffset = maxGradeDefinedValue.Value.AsIntegerOrNull();
                    if ( maxGradeOffset.HasValue )
                    {
                        foreach ( ListItem item in dvpMemberGrade.Items )
                        {
                            if ( !item.Selected )
                            {
                                var gradeValue = DefinedValueCache.Get( item.Value.AsGuid() );
                                if ( gradeValue != null )
                                {
                                    var gradeOffset = gradeValue.Value.AsIntegerOrNull();
                                    if ( !gradeOffset.HasValue || gradeOffset < maxGradeOffset.Value )
                                    {
                                        removeList.Add( item );
                                    }
                                }

                            }
                        }
                    }
                }
            }
            foreach ( var item in removeList )
            {
                dvpMemberGrade.Items.Remove( item );
            }
        }

        protected void lbSaveGroupMember_Click( object sender, EventArgs e )
        {
            GroupMember member = null;
            var rockContext = new RockContext();
            var memberService = new GroupMemberService( rockContext );

            if ( hfMemberId.ValueAsInt() > 0 )
            {
                var memberId = hfMemberId.Value.AsIntegerOrNull();

                if ( memberId.HasValue )
                {
                    member = memberService.Get( memberId.Value );
                }
            }
            else
            {
                var person = new Person();
                person.FirstName = rtbMemberFirstName.Text;
                person.LastName = rtbMemberLastName.Text;
                var familyRoleId = rtbMemberRelationship.GroupRoleId;
                var familyId = CurrentPerson.GetFamily().Id;
                PersonService.AddPersonToFamily( person, true, familyId, familyRoleId.Value, rockContext );

                member = memberService.Queryable().Where( gm => gm.PersonId == person.Id && gm.GroupId == familyId ).FirstOrDefault();
            }

            if ( member != null )
            {
                var person = member.Person;

                if ( !person.BirthDate.HasValue )
                {
                    person.SetBirthDate( dpMemberBirthDate.SelectedDate );
                }

                var gradeOffset = person.GradeOffset;
                if ( !gradeOffset.HasValue && dvpMemberGrade.SelectedGradeValue != null )
                {
                    person.GradeOffset = dvpMemberGrade.SelectedGradeValue.Value.AsIntegerOrNull();
                }

                int? orphanedPhotoId = null;
                if ( person.PhotoId != imgPhoto.BinaryFileId )
                {
                    orphanedPhotoId = person.PhotoId;
                    person.PhotoId = imgPhoto.BinaryFileId;
                }

                if ( rockContext.SaveChanges() > 0 )
                {
                    if ( orphanedPhotoId.HasValue )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                            rockContext.SaveChanges();
                        }
                    }

                    // if they used the ImageEditor, and cropped it, the un-cropped file is still in BinaryFile. So clean it up
                    if ( imgPhoto.CropBinaryFileId.HasValue )
                    {
                        if ( imgPhoto.CropBinaryFileId != person.PhotoId )
                        {
                            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                            var binaryFile = binaryFileService.Get( imgPhoto.CropBinaryFileId.Value );
                            if ( binaryFile != null && binaryFile.IsTemporary )
                            {
                                string errorMessage;
                                if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                                {
                                    binaryFileService.Delete( binaryFile );
                                    rockContext.SaveChanges();
                                }
                            }
                        }
                    }
                }

                person.LoadAttributes();
                var memberSchoolValue = person.GetAttributeValue( "School" ).AsGuidOrNull();
                if ( !memberSchoolValue.HasValue && dvpMemberSchool.SelectedDefinedValueId.HasValue )
                {
                    var memberSchool = DefinedValueCache.Get( dvpMemberSchool.SelectedDefinedValueId.Value );
                    if ( memberSchool != null )
                    {
                        person.SetAttributeValue( "School", memberSchool.Guid.ToString() );
                        person.SaveAttributeValue( "School", rockContext );
                    }
                }

                pnlEditFamilyMember.Visible = false;
                ShowStart();
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
                        CurrentRegistrationInformation.CurrentFormIndex++;
                        if ( ( CurrentRegistrationInformation.CurrentFormIndex >= CurrentRegistrationInformation.FormCount && !CurrentRegistrationInformation.SignInline ) || CurrentRegistrationInformation.CurrentFormIndex >= CurrentRegistrationInformation.FormCount + 1 )
                        {
                            CurrentRegistrationInformation.CurrentRegistrantIndex++;
                            CurrentRegistrationInformation.CurrentFormIndex = 0;
                        }
                    }
                    else
                    {
                        increment = true;
                    }
                } while ( CurrentRegistrationInformation.CurrentRegistrantIndex < CurrentRegistrationInformation.RegistrationState.RegistrantCount && !FormHasWaitFields() );

                if ( CurrentRegistrationInformation.CurrentRegistrantIndex >= CurrentRegistrationInformation.RegistrationState.RegistrantCount )
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
                        CurrentRegistrationInformation.CurrentFormIndex--;
                        if ( CurrentRegistrationInformation.CurrentFormIndex < 0 )
                        {
                            CurrentRegistrationInformation.CurrentRegistrantIndex--;
                            CurrentRegistrationInformation.CurrentFormIndex = CurrentRegistrationInformation.FormCount - 1;
                        }
                    }
                    else
                    {
                        increment = true;
                    }
                } while ( CurrentRegistrationInformation.CurrentRegistrantIndex >= 0 && !FormHasWaitFields() );

                if ( CurrentRegistrationInformation.CurrentRegistrantIndex < 0 )
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
            if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null && CurrentRegistrationInformation.RegistrationState != null && CurrentRegistrationInformation.RegistrationState.Registrants.Count > CurrentRegistrationInformation.CurrentRegistrantIndex && CurrentRegistrationInformation.FormCount > CurrentRegistrationInformation.CurrentFormIndex )
            {
                var registrant = CurrentRegistrationInformation.RegistrationState.Registrants[CurrentRegistrationInformation.CurrentRegistrantIndex];
                if ( registrant.OnWaitList )
                {
                    var form = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentRegistrationInformation.CurrentFormIndex];
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

            if ( CurrentRegistrationInformation.RegistrationState != null && CurrentRegistrationInformation.RegistrationState.RegistrantCount > 0 )
            {
                int max = CurrentRegistrationInformation.MaxRegistrants;
                if ( !CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.WaitListEnabled && CurrentRegistrationInformation.RegistrationState.SlotsAvailable.HasValue && CurrentRegistrationInformation.RegistrationState.SlotsAvailable.Value < max )
                {
                    max = CurrentRegistrationInformation.RegistrationState.SlotsAvailable.Value;
                }

                lbRegistrantPrev.Visible = false;

                var hideFirstLastNameFields = GetAttributeValue( "HideFirstLastNameFields" ).AsBoolean();
                var registrant = CurrentRegistrationInformation.RegistrationState.Registrants[CurrentRegistrationInformation.CurrentRegistrantIndex];
                var currentForm = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentRegistrationInformation.CurrentFormIndex];



                if ( registrant != null )
                {
                    var person = new PersonService( new RockContext() ).Get( registrant.PersonId ?? 0 );

                    if ( hideFirstLastNameFields &&
                        person != null &&
                        !currentForm.Fields.Any( f => f.FieldSource != RegistrationFieldSource.PersonField || ( f.PersonFieldType != RegistrationPersonFieldType.FirstName && f.PersonFieldType != RegistrationPersonFieldType.LastName ) ) &&
                        !( CurrentRegistrationInformation.SignInline && CurrentRegistrationInformation.CurrentFormIndex >= CurrentRegistrationInformation.FormCount ) &&
                        !( CurrentRegistrationInformation.FormCount - 1 == CurrentRegistrationInformation.CurrentFormIndex && !registrant.OnWaitList && CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Fees.Any() ) )
                    {
                        ShowRegistrant( true, true );
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append( "<ul>" );
                        foreach ( var registrationInstanceId in MultiEventRegistrants[CurrentMultiEventRegistrantIndex].RegistrationInstanceIds )
                        {
                            var registrationInstance = new RegistrationInstanceService( new RockContext() ).Get( registrationInstanceId );
                            if ( !hideFirstLastNameFields ||
                                GetAttributeValue( "SignInline" ).AsBoolean() && registrationInstance.RegistrationTemplate.SignatureDocumentAction == SignatureDocumentAction.Embed ||
                                registrationInstance.RegistrationTemplate.Fees.Any() ||
                                registrationInstance.RegistrationTemplate.Forms.Any( f =>
                                    f.Fields.Any( ff => ff.FieldSource != RegistrationFieldSource.PersonField || ( ff.PersonFieldType != RegistrationPersonFieldType.FirstName && ff.PersonFieldType != RegistrationPersonFieldType.LastName ) ) )
                                    )
                            {
                                sb.Append( "<li>" );

                                var registrationName = registrationInstance.Name;
                                if ( registrationName.Contains( "Admin 2020" ) )
                                {
                                    registrationName = "Daycation 2020";
                                }

                                if ( registrationInstance.Id == CurrentRegistrationInformation.RegistrationInstanceState.Id )
                                {
                                    sb.AppendFormat( "<b>{0}</b>", registrationName );
                                }
                                else
                                {
                                    sb.Append( registrationName );
                                }

                                sb.Append( "<ul>" );
                                var formIndex = 0;
                                foreach ( var form in registrationInstance.RegistrationTemplate.Forms )
                                {
                                    if ( !hideFirstLastNameFields ||
                                        ( registrationInstance.RegistrationTemplate.Forms.Count - 1 == formIndex && !registrant.OnWaitList && registrationInstance.RegistrationTemplate.Fees.Any() ) ||
                                        ( ( GetAttributeValue( "SignInline" ).AsBoolean() && registrationInstance.RegistrationTemplate.SignatureDocumentAction == SignatureDocumentAction.Embed ) && formIndex >= registrationInstance.RegistrationTemplate.Forms.Count ) ||
                                        form.Fields.Any( ff => ff.FieldSource != RegistrationFieldSource.PersonField || ( ff.PersonFieldType != RegistrationPersonFieldType.FirstName && ff.PersonFieldType != RegistrationPersonFieldType.LastName ) ) )
                                    {
                                        sb.Append( "<li>" );

                                        var formName = form.Name;
                                        if ( formName.Contains( "Default Form" ) )
                                        {
                                            formName = "Registration Form";
                                        }

                                        if ( registrationInstance.Id == CurrentRegistrationInformation.RegistrationInstanceState.Id && form.Id == currentForm.Id )
                                        {
                                            sb.AppendFormat( "<b>{0}</b>", formName );
                                        }
                                        else
                                        {
                                            sb.Append( formName );
                                        }
                                        sb.Append( "</li>" );
                                    }
                                    formIndex++;
                                }
                                sb.Append( "</ul>" );

                                sb.Append( "</li>" );
                            }

                        }
                        sb.Append( "</ul>" );
                        lRegistrantSidebar.Text = sb.ToString();

                        string title = string.Format( "{0}: {1}", registrant.PersonName, CurrentRegistrationInformation.RegistrationInstanceState.Name.Contains( "Admin 2020" ) ? "Daycation 2020" : CurrentRegistrationInformation.RegistrationInstanceState.Name );
                        if ( CurrentRegistrationInformation.CurrentFormIndex > 0 )
                        {
                            title += " (cont)";
                        }

                        lRegistrantTitle.Text = title;

                        nbType.Visible = CurrentRegistrationInformation.RegistrationState.RegistrantCount > CurrentRegistrationInformation.RegistrationState.SlotsAvailable;
                        nbType.Text = registrant.OnWaitList ? string.Format( "This {0} will be on the waiting list", CurrentRegistrationInformation.RegistrantTerm.ToLower() ) : string.Format( "This {0} will be fully registered.", CurrentRegistrationInformation.RegistrantTerm.ToLower() );
                        nbType.NotificationBoxType = registrant.OnWaitList ? NotificationBoxType.Warning : NotificationBoxType.Success;

                        decimal currentStep = ( CurrentRegistrationInformation.FormCount * CurrentRegistrationInformation.CurrentRegistrantIndex ) + CurrentRegistrationInformation.CurrentFormIndex + 1;
                        if ( CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants.Any() )
                        {
                            currentStep++;
                        }

                        CurrentRegistrationInformation.PercentComplete = ( currentStep / CurrentRegistrationInformation.ProgressBarSteps ) * 100.0m;

                        if ( CurrentRegistrationInformation.SignInline && CurrentRegistrationInformation.CurrentFormIndex >= CurrentRegistrationInformation.FormCount )
                        {
                            string registrantName = CurrentRegistrationInformation.RegistrantTerm;
                            if ( CurrentRegistrationInformation.RegistrationState != null && CurrentRegistrationInformation.RegistrationState.RegistrantCount > CurrentRegistrationInformation.CurrentRegistrantIndex )
                            {
                                registrantName = registrant.GetFirstName( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate );
                            }

                            nbDigitalSignature.Heading = "Signature Required";
                            nbDigitalSignature.Text = string.Format(
                                "This {0} requires that you sign a {1} for each registrant, please follow the prompts below to digitally sign this document for {2}.",
                                CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrationTerm,
                                CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplate.Name,
                                registrantName );

                            var errors = new List<string>();
                            string inviteLink = CurrentRegistrationInformation.DigitalSignatureComponent.GetInviteLink( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RequiredSignatureDocumentTemplate.ProviderTemplateKey, out errors );
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
                        }

                        SetPanel( PanelIndex.PanelRegistrant );
                    }

                }
            }
        }

        /// <summary>
        /// Shows the summary panel
        /// </summary>
        private void ShowSummary()
        {
            decimal currentStep = ( CurrentRegistrationInformation.FormCount * CurrentRegistrationInformation.RegistrationState.RegistrantCount ) + 1;
            if ( CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                currentStep++;
            }

            if ( CurrentRegistrationInformation.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                currentStep++;
            }

            CurrentRegistrationInformation.PercentComplete = ( currentStep / CurrentRegistrationInformation.ProgressBarSteps ) * 100.0m;

            SetPanel( PanelIndex.PanelSummary );
        }

        /// <summary>
        /// Shows the payment panel.
        /// </summary>
        private void ShowPayment()
        {
            decimal currentStep = ( CurrentRegistrationInformation.FormCount * CurrentRegistrationInformation.RegistrationState.RegistrantCount ) + 2;

            if ( CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                currentStep++;
            }

            if ( CurrentRegistrationInformation.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                currentStep++;
            }

            CurrentRegistrationInformation.PercentComplete = ( currentStep / CurrentRegistrationInformation.ProgressBarSteps ) * 100.0m;

            SetPanel( PanelIndex.PanelPayment );

            if ( ( rblSavedCC.Items.Count == 0 || ( rblSavedCC.SelectedValueAsInt() ?? 0 ) == 0 ) &&
                CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null &&
                FinancialGateway != null )
            {
                var component = FinancialGateway.GetGatewayComponent();
                if ( component != null )
                {
                    pnlPaymentInfo.Visible = true;
                    rblSavedCC.Visible = false;
                    divNewCard.Visible = true;
                    divNewCard.Style[HtmlTextWriterStyle.Display] = "block";

                    txtCardFirstName.Visible = component.PromptForNameOnCard( FinancialGateway ) && component.SplitNameOnCard;
                    txtCardLastName.Visible = component.PromptForNameOnCard( FinancialGateway ) && component.SplitNameOnCard;
                    txtCardName.Visible = component.PromptForNameOnCard( FinancialGateway ) && !component.SplitNameOnCard;

                    mypExpiration.MinimumYear = RockDateTime.Now.Year;
                    mypExpiration.MaximumYear = mypExpiration.MinimumYear + 15;

                    acBillingAddress.Visible = component.PromptForBillingAddress( FinancialGateway );
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
        private void ShowSuccess( List<int> registrationIds )
        {
            decimal currentStep = ( CurrentRegistrationInformation.FormCount * CurrentRegistrationInformation.RegistrationState.RegistrantCount ) + ( Using3StepGateway ? 3 : 2 );

            if ( CurrentRegistrationInformation.RegistrationAttributeIdsBeforeRegistrants.Any() )
            {
                currentStep++;
            }

            if ( CurrentRegistrationInformation.RegistrationAttributeIdsAfterRegistrants.Any() )
            {
                currentStep++;
            }

            CurrentRegistrationInformation.PercentComplete = ( currentStep / CurrentRegistrationInformation.ProgressBarSteps ) * 100.0m;
            pnlSuccessProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();

            lSuccessTitle.Text = "Congratulations";
            lSuccess.Text = "You have successfully completed this registration.";

            try
            {
                var rockContext = new RockContext();
                var registrations = new RegistrationService( rockContext )
                    .Queryable( "RegistrationInstance.RegistrationTemplate" )
                    .Where( r => registrationIds.Contains( r.Id ) )
                    .ToList();

                if ( registrations.Any() )
                {
                    // var template = registration.RegistrationInstance.RegistrationTemplate;

                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "CurrentPerson", CurrentPerson );
                    //  mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                    mergeFields.Add( "Registrations", registrations );
                    mergeFields.Add( "Registrants", MultiEventRegistrants );


                    lSuccessTitle.Text = "Congratulations";
                    var categoryId = PageParameter( CATEGORY_ID_PARAM_NAME ).AsInteger();
                    if ( categoryId == null || categoryId == 0 )
                    {
                        categoryId = GetAttributeValue( "DefaultCategoryId" ).AsInteger();
                    }

                    var category = CategoryCache.Get( categoryId );
                    category.LoadAttributes();
                    mergeFields.Add( "Category", category );
                    var successText = category.GetAttributeValue( "RegistrationConfirmationText" );
                    if ( successText != null && !string.IsNullOrWhiteSpace( successText ) )
                    {
                        lSuccess.Text = successText.ResolveMergeFields( mergeFields );
                    }
                    else
                    {
                        lSuccess.Text = "You have successfully completed this " + CurrentRegistrationInformation.RegistrationTerm.ToLower();
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

            if ( CurrentPanel == PanelIndex.PanelSummary && !( CurrentRegistrationInformation.RegistrationState.RegistrationId.HasValue && CurrentRegistrationInformation.RegistrationState.DiscountCode.IsNotNullOrWhiteSpace() ) )
            {
                AutoApplyDiscounts();
            }

            pnlRegistrarInfoPrompt.Visible = CurrentPanel == PanelIndex.PanelSummary;

            CreateDynamicControls( true );

            pnlStart.Visible = CurrentPanel <= 0;
            pnlSelectRegistrations.Visible = CurrentPanel == PanelIndex.PanelSelectRegistrations;
            pnlRegistrationAttributesStart.Visible = CurrentPanel == PanelIndex.PanelRegistrationAttributesStart;
            pnlRegistrant.Visible = CurrentPanel == PanelIndex.PanelRegistrant;
            pnlRegistrationAttributesEnd.Visible = CurrentPanel == PanelIndex.PanelRegistrationAttributesEnd;

            pnlSummaryAndPayment.Visible = CurrentPanel == PanelIndex.PanelSummary || CurrentPanel == PanelIndex.PanelPayment;

            pnlRegistrantsReview.Visible = CurrentPanel == PanelIndex.PanelSummary;
            if ( CurrentPanel != PanelIndex.PanelSummary )
            {
                pnlCostAndFees.Visible = false;
            }

            lbSummaryPrev.Visible = false;// CurrentPanel == PanelIndex.PanelSummary;
            lbSummaryNext.Visible = CurrentPanel == PanelIndex.PanelSummary;

            lbPaymentPrev.Visible = CurrentPanel == PanelIndex.PanelPayment;
            aStep2Submit.Visible = CurrentPanel == PanelIndex.PanelPayment;

            pnlSuccess.Visible = CurrentPanel == PanelIndex.PanelSuccess;

            lSummaryAndPaymentTitle.Text = ( CurrentPanel == PanelIndex.PanelSummary && CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null ) ? "Review " + CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrationTerm : "Payment Method";
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
            , ""                                      // {3}
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
            , CurrentRegistrationInformation != null ? CurrentRegistrationInformation.RegistrantTerm : "Registrant"                      // {23}
            , rblFamilyOptions.ClientID              // {24}
            , ""                                      // {25}
            , controlFamilyGuid                      // {26}
        );

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "registrationEntry", script, true );

            if ( CurrentRegistrationInformation != null && Using3StepGateway )
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
                    if ( CurrentRegistrationInformation.CurrentFormIndex <= CurrentRegistrationInformation.FormCount )
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
                    if ( CurrentRegistrationInformation.CurrentFormIndex < CurrentRegistrationInformation.FormCount )
                    {
                        ParseRegistrantControls();
                        decimal currentStep = ( CurrentRegistrationInformation.FormCount * CurrentRegistrationInformation.CurrentRegistrantIndex ) + CurrentRegistrationInformation.CurrentFormIndex + 1;
                        CurrentRegistrationInformation.PercentComplete = ( currentStep / CurrentRegistrationInformation.ProgressBarSteps ) * 100.0m;
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
            lRegistrantFeeCaption.Text = CurrentRegistrationInformation.FeeTerm.Pluralize();

            phRegistrantControls.Controls.Clear();
            phFees.Controls.Clear();

            if ( CurrentRegistrationInformation.FormCount > CurrentRegistrationInformation.CurrentFormIndex )
            {
                // Get the current and previous registrant ( first is used when a field has the 'IsSharedValue' property )
                // so that current registrant can use the first registrant's value
                RegistrantInfo registrant = null;
                RegistrantInfo firstRegistrant = null;
                var preselectCurrentPerson = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes;

                if ( CurrentRegistrationInformation.RegistrationState != null && CurrentRegistrationInformation.RegistrationState.RegistrantCount >= CurrentRegistrationInformation.CurrentRegistrantIndex )
                {
                    registrant = CurrentRegistrationInformation.RegistrationState.Registrants[CurrentRegistrationInformation.CurrentRegistrantIndex];

                    pnlFamilyOptions.Visible = false;

                    if ( setValues )
                    {
                        if ( CurrentRegistrationInformation.CurrentRegistrantIndex > 0 )
                        {
                            firstRegistrant = CurrentRegistrationInformation.RegistrationState.Registrants[0];
                        }

                        rblFamilyOptions.SetValue( registrant.FamilyGuid.ToString() );
                    }
                }
                var person = new PersonService( new RockContext() ).Get( registrant.PersonId ?? 0 );

                var familyMemberSelected = registrant.Id <= 0 && registrant.PersonId.HasValue && CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.ShowCurrentFamilyMembers;

                var form = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentRegistrationInformation.CurrentFormIndex];
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

                    bool hasDependantVisibilityRule = form.Fields.Any( a => a.FieldVisibilityRules.RuleList.Any( r => r.ComparedToRegistrationTemplateFormFieldGuid == field.Guid ) );

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        if ( person != null && GetAttributeValue( "HideFirstLastNameFields" ).AsBoolean() )
                        {
                            if ( field.PersonFieldType == RegistrationPersonFieldType.FirstName || field.PersonFieldType == RegistrationPersonFieldType.LastName )
                            {
                            }
                            else
                            {
                                CreatePersonField( hasDependantVisibilityRule, field, setValues, value, familyMemberSelected, BlockValidationGroup, phRegistrantControls );
                            }
                        }
                        else
                        {
                            CreatePersonField( hasDependantVisibilityRule, field, setValues, value, familyMemberSelected, BlockValidationGroup, phRegistrantControls );
                        }
                    }
                    else
                    {
                        CreateAttributeField( hasDependantVisibilityRule, field, setValues, value, GetAttributeValue( "ShowFieldDescriptions" ).AsBoolean(), BlockValidationGroup, phRegistrantControls );
                    }
                }

                FieldVisibilityWrapper.ApplyFieldVisibilityRules( phRegistrantControls );

                // If the current form, is the last one, add any fee controls
                if ( CurrentRegistrationInformation.FormCount - 1 == CurrentRegistrationInformation.CurrentFormIndex && !registrant.OnWaitList )
                {
                    List<RegistrantInfo> otherRegistrants = CurrentRegistrationInformation.RegistrationState.Registrants.Where( a => a != registrant ).ToList();

                    foreach ( var fee in CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Fees.Where( f => f.IsActive == true ).OrderBy( o => o.Order ) )
                    {
                        var feeValues = new List<FeeInfo>();
                        if ( registrant != null && registrant.FeeValues.ContainsKey( fee.Id ) )
                        {
                            feeValues = registrant.FeeValues[fee.Id];
                        }

                        fee.AddFeeControl( phFees, CurrentRegistrationInformation.RegistrationInstanceState, setValues, feeValues, otherRegistrants );
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
        private static void CreatePersonField( bool hasDependantVisibilityRule, RegistrationTemplateFormField field, bool setValue, object fieldValue, bool familyMemberSelected, string validationGroup, Control parentControl )
        {

            Control personFieldControl = field.GetPersonControl( setValue, fieldValue, familyMemberSelected, validationGroup );

            if ( personFieldControl != null )
            {
                var fieldVisibilityWrapper = new FieldVisibilityWrapper
                {
                    ID = "_fieldVisibilityWrapper_field_" + field.Guid.ToString( "N" ),
                    RegistrationTemplateFormFieldId = field.Id,
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

                fieldVisibilityWrapper.Controls.Add( personFieldControl );
                fieldVisibilityWrapper.EditControl = personFieldControl;

                if ( !string.IsNullOrWhiteSpace( field.PostText ) )
                {
                    fieldVisibilityWrapper.Controls.Add( new LiteralControl( field.PostText ) );
                }

                if ( hasDependantVisibilityRule && FieldVisibilityRules.IsFieldSupported( field.PersonFieldType ) )
                {
                    var fieldType = FieldVisibilityRules.GetSupportedFieldTypeCache( field.PersonFieldType ).Field;

                    if ( fieldType.HasChangeHandler( personFieldControl ) )
                    {
                        fieldType.AddChangeHandler( personFieldControl, () =>
                        {
                            fieldVisibilityWrapper.TriggerEditValueUpdated( personFieldControl, new FieldVisibilityWrapper.FieldEventArgs( null, personFieldControl ) );
                        } );
                    }
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
        private static void CreateAttributeField( bool hasDependantVisibilityRule, RegistrationTemplateFormField field, bool setValue, object fieldValue, bool showFieldDescriptions, string validationGroup, Control parentControl )
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
                    RegistrationTemplateFormFieldId = field.Id,
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
            if ( CurrentRegistrationInformation.RegistrationState != null && CurrentRegistrationInformation.RegistrationState.Registrants.Count > CurrentRegistrationInformation.CurrentRegistrantIndex )
            {
                var registrant = CurrentRegistrationInformation.RegistrationState.Registrants[CurrentRegistrationInformation.CurrentRegistrantIndex];
                var currentRegistrationIndex = RegistrationInformationList.IndexOf( RegistrationInformationList.Where( ri => ri.RegistrationInstanceState.Id == CurrentRegistrationInformation.RegistrationInstanceState.Id ).First() );

                if ( rblFamilyOptions.Visible )
                {
                    registrant.FamilyGuid = rblFamilyOptions.SelectedValue.AsGuid();
                }

                if ( registrant.FamilyGuid.Equals( Guid.Empty ) )
                {
                    registrant.FamilyGuid = Guid.NewGuid();
                }

                var person = new PersonService( new RockContext() ).Get( registrant.PersonId ?? 0 );

                var form = CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms.OrderBy( f => f.Order ).ToList()[CurrentRegistrationInformation.CurrentFormIndex];
                foreach ( var field in form.Fields
                    .Where( f =>
                        !f.IsInternal &&
                        ( !registrant.OnWaitList || f.ShowOnWaitlist ) )
                    .OrderBy( f => f.Order ) )
                {
                    object value = null;

                    if ( field.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        if ( person != null && GetAttributeValue( "HideFirstLastNameFields" ).AsBoolean() )
                        {
                            if ( field.PersonFieldType == RegistrationPersonFieldType.FirstName )
                            {
                                value = person.FirstName;
                            }
                            else if ( field.PersonFieldType == RegistrationPersonFieldType.LastName )
                            {
                                value = person.LastName;
                            }
                            else
                            {
                                value = ParsePersonField( field );
                            }
                        }
                        else
                        {
                            value = ParsePersonField( field );
                        }
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

                if ( CurrentRegistrationInformation.FormCount - 1 == CurrentRegistrationInformation.CurrentFormIndex )
                {
                    foreach ( var fee in CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Fees )
                    {
                        List<FeeInfo> feeValues = fee.GetFeeInfoFromControls( phFees );
                        if ( fee != null )
                        {
                            registrant.FeeValues.AddOrReplace( fee.Id, feeValues );
                        }
                    }
                }

                CurrentRegistrationInformation.RegistrationState.Registrants[CurrentRegistrationInformation.CurrentRegistrantIndex] = registrant;
                RegistrationInformationList[currentRegistrationIndex] = CurrentRegistrationInformation;
                SaveViewState();
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
            if ( CurrentRegistrationInformation.RegistrationState != null && CurrentRegistrationInformation.RegistrationState.Registrants.Count > CurrentRegistrationInformation.CurrentRegistrantIndex )
            {
                using ( var rockContext = new RockContext() )
                {
                    var registrant = CurrentRegistrationInformation.RegistrationState.Registrants[CurrentRegistrationInformation.CurrentRegistrantIndex];
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

                        foreach ( var field in CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.Forms
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
        }

        #endregion

        #region Summary/Payment Controls

        /// <summary>
        /// Creates the summary controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateSummaryControls( bool setValues )
        {
            lRegistrationTermPrompt.Text = CurrentRegistrationInformation.RegistrationTerm;
            lRegistrationTermLoggedInPerson.Text = CurrentRegistrationInformation.RegistrationTerm;
            lDiscountCodeLabel.Text = CurrentRegistrationInformation.DiscountCodeTerm;

            if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask )
            {
                var familyOptions = CurrentRegistrationInformation.RegistrationState.GetFamilyOptions( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate, CurrentRegistrationInformation.RegistrationState.RegistrantCount );
                if ( familyOptions.Any() )
                {
                    // previous family selections are always null after postback, so default to anyone in the same family
                    var selectedGuid = CurrentPerson != null ? CurrentPerson.GetFamily().Guid : rblRegistrarFamilyOptions.SelectedValueAsGuid();

                    familyOptions.Add(
                        familyOptions.ContainsKey( CurrentRegistrationInformation.RegistrationState.FamilyGuid ) ?
                        Guid.NewGuid() :
                        CurrentRegistrationInformation.RegistrationState.FamilyGuid.Equals( Guid.Empty ) ? Guid.NewGuid() : CurrentRegistrationInformation.RegistrationState.FamilyGuid,
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

            if ( setValues && CurrentRegistrationInformation.RegistrationState != null && CurrentRegistrationInformation.RegistrationInstanceState != null )
            {
                lbSummaryNext.Text = "Finish";

                // If UseLoggedInPerson is enabled, only prompt for Email, and only if it is isn't known
                pnlRegistrarInfoPrompt.Visible = false;
                pnlRegistrarInfoUseLoggedInPerson.Visible = true;
                lUseLoggedInPersonFirstName.Text = CurrentPerson.NickName;
                lUseLoggedInPersonLastName.Text = CurrentPerson.LastName;
                lUseLoggedInPersonEmail.Text = CurrentPerson.Email;
                tbUseLoggedInPersonEmail.Text = CurrentPerson.Email;
                lUseLoggedInPersonEmail.Visible = !CurrentPerson.Email.IsNullOrWhiteSpace();
                tbUseLoggedInPersonEmail.Visible = CurrentPerson.Email.IsNullOrWhiteSpace();

                rblRegistrarFamilyOptions.Label = string.IsNullOrWhiteSpace( tbYourFirstName.Text ) ?
                    "You are in the same " + GetAttributeValue( "FamilyTerm" ) + " as" :
                    tbYourFirstName.Text + " is in the same " + GetAttributeValue( "FamilyTerm" ) + " as";

                cbUpdateEmail.Visible = CurrentPerson != null && !string.IsNullOrWhiteSpace( CurrentPerson.Email ) && !GetAttributeValue( "ForceEmailUpdate" ).AsBoolean();
                if ( CurrentPerson != null && GetAttributeValue( "ForceEmailUpdate" ).AsBoolean() )
                {
                    lUpdateEmailWarning.Visible = true;
                }

                // Build Discount info if template has discounts and this is a new registration
                if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate != null
                    && RegistrationInformationList.Any( ri => ri.RegistrationInstanceState.RegistrationTemplate.Discounts.Any() )
                    && !CurrentRegistrationInformation.RegistrationState.RegistrationId.HasValue )
                {
                    divDiscountCode.Visible = true;

                    string discountCode = CurrentRegistrationInformation.RegistrationState.DiscountCode;
                    if ( !string.IsNullOrWhiteSpace( discountCode ) && discountCode != "Siblings" )
                    {
                        var discount = RegistrationInformationList.SelectMany( ri => ri.RegistrationInstanceState.RegistrationTemplate.Discounts
                             .Where( d => d.Code.Equals( discountCode, StringComparison.OrdinalIgnoreCase ) ) )
                            .FirstOrDefault();

                        if ( discount == null )
                        {
                            nbDiscountCode.Text = string.Format( "'{1}' is not a valid {1}.", discountCode, CurrentRegistrationInformation.DiscountCodeTerm );
                            nbDiscountCode.Visible = true;
                        }
                    }
                }
                else
                {
                    divDiscountCode.Visible = false;
                    tbDiscountCode.Text = CurrentRegistrationInformation.RegistrationState.DiscountCode;
                }


                var totalCosts = new List<RegistrationCostSummaryInfo>();
                minimumPayment = 0.0M;
                defaultPayment = null;
                paymentAmount = 0.0M;

                foreach ( var registrationInformation in RegistrationInformationList )
                {
                    var costs = new List<RegistrationCostSummaryInfo>();
                    decimal? minimumInitialPaymentPerRegistrant = ( decimal? ) 0.00;
                    decimal? defaultPaymentAmountPerRegistrant = ( decimal? ) 0.00;

                    if ( registrationInformation.RegistrationInstanceState.RegistrationTemplate.SetCostOnInstance ?? false )
                    {
                        minimumInitialPaymentPerRegistrant = registrationInformation.RegistrationInstanceState.MinimumInitialPayment;
                        defaultPaymentAmountPerRegistrant = registrationInformation.RegistrationInstanceState.DefaultPayment;
                    }
                    else
                    {
                        minimumInitialPaymentPerRegistrant = registrationInformation.RegistrationInstanceState.RegistrationTemplate.MinimumInitialPayment;
                        defaultPaymentAmountPerRegistrant = registrationInformation.RegistrationInstanceState.RegistrationTemplate.DefaultPayment;
                    }

                    foreach ( var registrant in registrationInformation.RegistrationState.Registrants )
                    {
                        var costSummary = new RegistrationCostSummaryInfo();
                        costSummary.Type = RegistrationCostSummaryType.Cost;
                        costSummary.Description = string.Format(
                            "{0} {1}",
                            registrant.GetFirstName( registrationInformation.RegistrationInstanceState.RegistrationTemplate ),
                            registrant.GetLastName( registrationInformation.RegistrationInstanceState.RegistrationTemplate ) );

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
                            if ( registrationInformation.RegistrationState.DiscountPercentage > 0.0m && registrant.DiscountApplies )
                            {
                                if ( registrationInformation.RegistrationState.DiscountPercentage >= 1.0m )
                                {
                                    costSummary.DiscountedCost = 0.0m;
                                }
                                else
                                {
                                    costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * registrationInformation.RegistrationState.DiscountPercentage );
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

                        foreach ( var fee in registrant.FeeValues )
                        {
                            var templateFee = registrationInformation.RegistrationInstanceState.RegistrationTemplate.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
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

                                    var feeCostSummary = new RegistrationCostSummaryInfo();
                                    feeCostSummary.Type = RegistrationCostSummaryType.Fee;
                                    feeCostSummary.Description = desc;
                                    feeCostSummary.Cost = feeInfo.Quantity * cost;

                                    if ( registrationInformation.RegistrationState.DiscountPercentage > 0.0m && templateFee != null && templateFee.DiscountApplies && registrant.DiscountApplies )
                                    {
                                        if ( registrationInformation.RegistrationState.DiscountPercentage >= 1.0m )
                                        {
                                            feeCostSummary.DiscountedCost = 0.0m;
                                        }
                                        else
                                        {
                                            feeCostSummary.DiscountedCost = feeCostSummary.Cost - ( feeCostSummary.Cost * registrationInformation.RegistrationState.DiscountPercentage );
                                        }
                                    }
                                    else
                                    {
                                        feeCostSummary.DiscountedCost = feeCostSummary.Cost;
                                    }

                                    // If template allows a minimum payment, then fees are not included, otherwise it is included
                                    feeCostSummary.MinPayment = 0;

                                    costs.Add( feeCostSummary );
                                }
                            }
                        }
                    }

                    registrationInformation.minimumPayment = 0.0M;
                    registrationInformation.defaultPayment = null;

                    // If there were any costs
                    if ( costs.Where( c => c.Cost > 0.0M ).Any() )
                    {
                        // Get the total min payment for all costs and fees
                        registrationInformation.minimumPayment = costs.Sum( c => c.MinPayment );

                        if ( costs.Any( c => c.DefaultPayment.HasValue ) )
                        {
                            registrationInformation.defaultPayment = costs.Where( c => c.DefaultPayment.HasValue ).Sum( c => c.DefaultPayment.Value );
                        }

                        // Add row for amount discount
                        if ( registrationInformation.RegistrationState.DiscountAmount > 0.0m )
                        {
                            decimal totalDiscount = 0.0m - ( registrationInformation.RegistrationState.Registrants.Where( r => r.DiscountApplies ).Count() * registrationInformation.RegistrationState.DiscountAmount );
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
                        registrationInformation.RegistrationState.TotalCost = costs.Sum( c => c.Cost );
                        registrationInformation.RegistrationState.DiscountedCost = costs.Sum( c => c.DiscountedCost );

                        // If minimum payment is greater than total discounted cost ( which is possible with discounts ), adjust the minimum payment
                        registrationInformation.minimumPayment = registrationInformation.minimumPayment.Value > registrationInformation.RegistrationState.DiscountedCost ? registrationInformation.RegistrationState.DiscountedCost : registrationInformation.minimumPayment;

                        // if min payment is less than 0, set it to 0
                        minimumPayment = minimumPayment.Value < 0 ? 0 : minimumPayment.Value;

                        // Calculate balance due, and if a partial payment is still allowed
                        decimal balanceDue = registrationInformation.RegistrationState.DiscountedCost;

                        // Make sure payment amount is within minimum due and balance due. If not, set to balance due
                        if ( !registrationInformation.RegistrationState.PaymentAmount.HasValue ||
                            registrationInformation.RegistrationState.PaymentAmount.Value < minimumPayment.Value ||
                            registrationInformation.RegistrationState.PaymentAmount.Value > balanceDue )
                        {
                            if ( defaultPayment.HasValue )
                            {
                                // NOTE: if the configured 'Minimum Initial Payment' is null, the minimumPayment is the full amount, so the 'Default Payment Amount' option would be ignored
                                if ( defaultPayment >= minimumPayment && defaultPayment <= balanceDue )
                                {
                                    // default Payment is more than min and less than balance due, so we can use it
                                    registrationInformation.RegistrationState.PaymentAmount = defaultPayment;
                                }
                                else if ( defaultPayment <= minimumPayment )
                                {
                                    // default Payment is less than min, so use min instead
                                    registrationInformation.RegistrationState.PaymentAmount = minimumPayment;
                                }
                                else if ( defaultPayment >= balanceDue )
                                {
                                    // default Payment is more than balance due, so use balance due
                                    registrationInformation.RegistrationState.PaymentAmount = balanceDue;
                                }
                            }
                            else
                            {
                                registrationInformation.RegistrationState.PaymentAmount = balanceDue;
                            }
                        }
                        SaveViewState();
                    }

                    if ( costs.Any() )
                    {
                        totalCosts.AddRange( costs );
                    }

                    if ( registrationInformation.minimumPayment >= 0.00M )
                    {
                        minimumPayment += registrationInformation.minimumPayment;
                    }

                    if ( registrationInformation.defaultPayment != null )
                    {
                        defaultPayment += registrationInformation.defaultPayment;
                    }

                    if ( registrationInformation.RegistrationState.PaymentAmount > 0 )
                    {
                        paymentAmount += registrationInformation.RegistrationState.PaymentAmount;
                    }
                }

                // If there were any costs
                if ( totalCosts.Where( c => c.Cost > 0.0M ).Any() )
                {
                    pnlRegistrantsReview.Visible = false;
                    pnlCostAndFees.Visible = true;

                    // Get the totals
                    var discountedCost = totalCosts.Sum( c => c.DiscountedCost );

                    // If minimum payment is greater than total discounted cost ( which is possible with discounts ), adjust the minimum payment
                    minimumPayment = minimumPayment.Value > discountedCost ? discountedCost : minimumPayment;

                    rptRegistrationFees.DataSource = RegistrationInformationList;
                    rptRegistrationFees.DataBind();

                    // Set the total cost
                    hfTotalCost.Value = discountedCost.ToString();
                    lTotalCost.Text = discountedCost.FormatAsCurrency();

                    // if min payment is less than 0, set it to 0
                    minimumPayment = minimumPayment.Value < 0 ? 0 : minimumPayment.Value;

                    // Calculate balance due, and if a partial payment is still allowed
                    decimal balanceDue = discountedCost;

                    // if there is a minimum amount defined (and it is less than the balance due), let a partial payment be specified
                    bool allowPartialPayment = false;

                    nbAmountPaid.Visible = allowPartialPayment;
                    nbAmountPaid.Text = ( paymentAmount ?? 0.0m ).ToString( "N2" );

                    // If a previous payment was made, or partial payment is allowed, show the amount remaining after selected payment amount
                    lRemainingDue.Visible = allowPartialPayment;
                    lRemainingDue.Text = ( discountedCost - ( paymentAmount ?? 0.0m ) ).FormatAsCurrency();

                    lAmountDue.Visible = !allowPartialPayment;
                    lAmountDue.Text = ( paymentAmount ?? 0.0m ).FormatAsCurrency();
                    lMinimumDue.Text = ( minimumPayment ?? 0.0m ).FormatAsCurrency();
                    // Set payment options based on gateway settings
                    if ( balanceDue > 0 && FinancialGateway != null )
                    {
                        if ( FinancialGateway.Attributes == null )
                        {
                            FinancialGateway.LoadAttributes();
                        }

                        var component = FinancialGateway.GetGatewayComponent();
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
                                lbSummaryNext.Text = "Pay Minimum";
                                txtCardFirstName.Visible = component.PromptForNameOnCard( FinancialGateway ) && component.SplitNameOnCard;
                                txtCardLastName.Visible = component.PromptForNameOnCard( FinancialGateway ) && component.SplitNameOnCard;
                                txtCardName.Visible = component.PromptForNameOnCard( FinancialGateway ) && !component.SplitNameOnCard;

                                mypExpiration.MinimumYear = RockDateTime.Now.Year;
                                mypExpiration.MaximumYear = mypExpiration.MinimumYear + 15;

                                acBillingAddress.Visible = component.PromptForBillingAddress( FinancialGateway );
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
                    var registrants = RegistrationInformationList.SelectMany( ri => ri.RegistrationState.Registrants.Where( r => !r.OnWaitList ) );
                    if ( registrants.Any() )
                    {
                        pnlRegistrantsReview.Visible = true;
                        rptrRegistrantEvents.DataSource = RegistrationInformationList;
                        rptrRegistrantEvents.DataBind();

                    }
                    else
                    {
                        pnlRegistrantsReview.Visible = false;
                    }

                    var waitingList = RegistrationInformationList.SelectMany( ri => ri.RegistrationState.Registrants.Where( r => r.OnWaitList ) );
                    if ( waitingList.Any() )
                    {
                        pnlWaitingListReview.Visible = true;
                        rptrWaitingListEvents.DataSource = RegistrationInformationList;
                        rptrWaitingListEvents.DataBind();
                    }
                    else
                    {
                        pnlWaitingListReview.Visible = false;
                    }

                    CurrentRegistrationInformation.RegistrationState.TotalCost = 0.0m;
                    CurrentRegistrationInformation.RegistrationState.DiscountedCost = 0.0m;
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
                            a.FinancialGatewayId == FinancialGateway.Id &&
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
        /// Updates CurrentRegistrationInformation.RegistrationState props using info from the UI controls
        /// </summary>
        private void ParseSummaryControls()
        {
            if ( CurrentRegistrationInformation.RegistrationState != null )
            {
                if ( CurrentRegistrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrarOption == RegistrarOption.UseLoggedInPerson && CurrentPerson != null )
                {
                    CurrentRegistrationInformation.RegistrationState.FirstName = CurrentPerson.NickName;
                    CurrentRegistrationInformation.RegistrationState.LastName = CurrentPerson.LastName;
                    CurrentRegistrationInformation.RegistrationState.ConfirmationEmail = CurrentPerson.Email;
                    if ( pnlRegistrarInfoUseLoggedInPerson.Visible )
                    {
                        CurrentRegistrationInformation.RegistrationState.ConfirmationEmail = tbUseLoggedInPersonEmail.Text;
                    }
                }
                else
                {
                    CurrentRegistrationInformation.RegistrationState.FirstName = tbYourFirstName.Text;
                    CurrentRegistrationInformation.RegistrationState.LastName = tbYourLastName.Text;
                    CurrentRegistrationInformation.RegistrationState.ConfirmationEmail = tbConfirmationEmail.Text;
                }

                if ( rblRegistrarFamilyOptions.Visible )
                {
                    CurrentRegistrationInformation.RegistrationState.FamilyGuid = rblRegistrarFamilyOptions.SelectedValue.AsGuid();
                }

                if ( CurrentRegistrationInformation.RegistrationState.FamilyGuid.Equals( Guid.Empty ) )
                {
                    CurrentRegistrationInformation.RegistrationState.FamilyGuid = Guid.NewGuid();
                }

            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Applies the first automatic discount that matches the set limits.
        /// </summary>
        private void AutoApplyDiscounts()
        {
            var isSiblingCodeAdded = false;
            if ( RegistrationInformationList != null )
            {
                var primaryRegistrant = MultiEventRegistrants.OrderByDescending( r => r.RegistrationInstanceIds.Count ).FirstOrDefault();
                if ( primaryRegistrant != null )
                {
                    var primaryAlias = new PersonAliasService( new RockContext() ).Get( primaryRegistrant.PersonAliasId );
                    if ( primaryAlias != null )
                    {
                        decimal discountAmount = 10.0m * MultiEventRegistrants.Count();
                        foreach ( var registrationInformation in RegistrationInformationList )
                        {
                            if ( registrationInformation.RegistrationState != null )
                            {
                                registrationInformation.RegistrationState.Registrants.ForEach( r => r.DiscountApplies = true );
                                foreach ( var registrant in registrationInformation.RegistrationState.Registrants )
                                {
                                    registrant.DiscountApplies = ( registrant.PersonId != primaryAlias.PersonId );
                                }

                                if ( registrationInformation.RegistrationState.Registrants.Any( r => r.DiscountApplies ) )
                                {
                                    isSiblingCodeAdded = true;
                                    registrationInformation.RegistrationState.DiscountCode = "Siblings";
                                    if ( registrationInformation.RegistrationInstanceState.RegistrationTemplate.Name.Contains( "Admin Fee" ) )
                                    {
                                        registrationInformation.RegistrationState.DiscountAmount = 50.0m;
                                    }
                                    else
                                    {
                                        registrationInformation.RegistrationState.DiscountAmount = discountAmount;
                                    }
                                    SaveViewState();
                                }
                            }
                        }
                    }

                }
            }

            if ( isSiblingCodeAdded )
            {
                nbDiscountCode.Visible = true;
                nbDiscountCode.NotificationBoxType = NotificationBoxType.Success;
                nbDiscountCode.Text = string.Format( "The {0} '{1}' was automatically applied.", CurrentRegistrationInformation.DiscountCodeTerm.ToLower(), "Siblings" );
            }
        }

        #endregion

        #region Helper Classes

        [Serializable]
        public class MultiEventRegistrant
        {
            public int PersonAliasId { get; set; }
            public List<int> RegistrationInstanceIds { get; set; }
            public int CurrentRegistrationInstanceIndex { get; set; }
        }

        [Serializable]
        public class RegistrationInformation
        {
            // protected variables
            private decimal _percentComplete = 0;

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
            public RegistrationInstance RegistrationInstanceState { get; set; }

            /// <summary>
            /// Gets or sets the state of the registration attributes.
            /// </summary>
            /// <value>
            /// The state of the registration attributes.
            /// </value>
            public List<AttributeCache> RegistrationAttributesState { get; set; }

            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int? GroupId { get; set; }

            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int? CampusId { get; set; }

            /// <summary>
            /// Digital signature fields
            /// </summary>
            /// <value>
            ///   <c>true</c> if [sign inline]; otherwise, <c>false</c>.
            /// </value>
            public bool SignInline { get; set; }

            /// <summary>
            /// Gets or sets the name of the digital signature component type.
            /// </summary>
            /// <value>
            /// The name of the digital signature component type.
            /// </value>
            public string DigitalSignatureComponentTypeName { get; set; }

            /// <summary>
            /// Gets or sets the digital signature component.
            /// </summary>
            /// <value>
            /// The digital signature component.
            /// </value>
            public DigitalSignatureComponent DigitalSignatureComponent { get; set; }

            /// <summary>
            /// Gets or sets the state of the registration.
            /// </summary>
            /// <value>
            /// The state of the registration.
            /// </value>
            public RegistrationInfo RegistrationState { get; set; }

            /// <summary>
            /// Gets or sets the index of the current registrant.
            /// </summary>
            /// <value>
            /// The index of the current registrant.
            /// </value>
            public int CurrentRegistrantIndex { get; set; }

            /// <summary>
            /// Gets or sets the index of the current form.
            /// </summary>
            /// <value>
            /// The index of the current form.
            /// </value>
            public int CurrentFormIndex { get; set; }

            /// <summary>
            /// Gets or sets the minimum payment total after factoring in discounts, fees, and minimum payment amount per registrant
            /// </summary>
            /// <value>
            /// The minimum payment.
            /// </value>
            public decimal? minimumPayment { get; set; }

            /// <summary>
            /// Gets or sets the default payment (combined for all registrants for this registration) 
            /// </summary>
            /// <value>
            /// The default payment.
            /// </value>
            public decimal? defaultPayment { get; set; }

            /// <summary>
            /// Gets the registration term.
            /// </summary>
            /// <value>
            /// The registration term.
            /// </value>
            public string RegistrationTerm
            {
                get
                {
                    if ( RegistrationInstanceState.RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationInstanceState.RegistrationTemplate.RegistrationTerm ) )
                    {
                        return RegistrationInstanceState.RegistrationTemplate.RegistrationTerm;
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
            public string RegistrationAttributeTitleStart
            {
                get
                {
                    if ( RegistrationInstanceState.RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationInstanceState.RegistrationTemplate.RegistrationAttributeTitleStart ) )
                    {
                        return RegistrationInstanceState.RegistrationTemplate.RegistrationAttributeTitleStart;
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
            public string RegistrationAttributeTitleEnd
            {
                get
                {
                    if ( RegistrationInstanceState.RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationInstanceState.RegistrationTemplate.RegistrationAttributeTitleEnd ) )
                    {
                        return RegistrationInstanceState.RegistrationTemplate.RegistrationAttributeTitleEnd;
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
            public string RegistrantTerm
            {
                get
                {
                    if ( RegistrationInstanceState.RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationInstanceState.RegistrationTemplate.RegistrantTerm ) )
                    {
                        return RegistrationInstanceState.RegistrationTemplate.RegistrantTerm;
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
            public string FeeTerm
            {
                get
                {
                    if ( RegistrationInstanceState.RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationInstanceState.RegistrationTemplate.FeeTerm ) )
                    {
                        return RegistrationInstanceState.RegistrationTemplate.FeeTerm;
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
            public string DiscountCodeTerm
            {
                get
                {
                    if ( RegistrationInstanceState.RegistrationTemplate != null && !string.IsNullOrWhiteSpace( RegistrationInstanceState.RegistrationTemplate.DiscountCodeTerm ) )
                    {
                        return RegistrationInstanceState.RegistrationTemplate.DiscountCodeTerm;
                    }

                    return "Discount Code";
                }
            }

            /// <summary>
            /// Gets the number of forms for the current registration template.
            /// </summary>
            public int FormCount
            {
                get
                {
                    if ( RegistrationInstanceState.RegistrationTemplate != null && RegistrationInstanceState.RegistrationTemplate.Forms != null )
                    {
                        return RegistrationInstanceState.RegistrationTemplate.Forms.Count;
                    }

                    return 0;
                }
            }

            /// <summary>
            /// If the registration template allows multiple registrants per registration, returns the maximum allowed
            /// </summary>
            public int MaxRegistrants
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
                    if ( RegistrationInstanceState.RegistrationTemplate != null && RegistrationInstanceState.RegistrationTemplate.AllowMultipleRegistrants )
                    {
                        if ( !RegistrationInstanceState.RegistrationTemplate.MaxRegistrants.HasValue )
                        {
                            return int.MaxValue;
                        }

                        return RegistrationInstanceState.RegistrationTemplate.MaxRegistrants.Value;
                    }

                    // Default is a maximum of one
                    return 1;
                }
            }

            /// <summary>
            /// Gets the minimum number of registrants allowed. Most of the time this is one, except for an existing
            /// registration that has existing registrants. The minimum in this case is the number of existing registrants
            /// </summary>
            public int MinRegistrants
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
            /// Gets or sets the ids of registration attributes that will be prompted *before* editing registrants.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has registration attributes before registrants; otherwise, <c>false</c>.
            /// </value>
            public List<int> RegistrationAttributeIdsBeforeRegistrants
            {
                get; set;
                //get
                //{
                //    return ViewState["RegistrationAttributeIdsBeforeRegistrants"] as List<int> ?? new List<int>();
                //}

                //set
                //{
                //    ViewState["RegistrationAttributeIdsBeforeRegistrants"] = value;
                //}
            }

            /// <summary>
            /// Gets or sets the ids of registration attributes that will be prompted *after* editing registrants.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has registration attributes after registrants; otherwise, <c>false</c>.
            /// </value>
            public List<int> RegistrationAttributeIdsAfterRegistrants
            {
                get; set;
                //get
                //{
                //    return ViewState["RegistrationAttributeIdsAfterRegistrants"] as List<int> ?? new List<int>();
                //}

                //set
                //{
                //    ViewState["RegistrationAttributeIdsAfterRegistrants"] = value;
                //}
            }

            /// <summary>
            /// Gets or sets the progress bar steps.
            /// </summary>
            /// <value>
            /// The progress bar steps.
            /// </value>
            public decimal ProgressBarSteps
            {
                get; set;
                //get { return ViewState["ProgressBarSteps"] as decimal? ?? 3.0m; }
                //set { ViewState["ProgressBarSteps"] = value; }
            }

            /// <summary>
            /// Gets or sets the payment transaction code. Used to help double-charging
            /// </summary>
            public string TransactionCode
            {
                get; set;
                //get { return ViewState["TransactionCode"] as string ?? string.Empty; }
                //set { ViewState["TransactionCode"] = value; }
            }

            #endregion
        }
        #endregion





        protected void rptRegistrationFees_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var hfRegistrationInstanceId = e.Item.FindControl( "hfRegistrationInstanceId" ) as HiddenField;
            var lRegistrationInstanceName = e.Item.FindControl( "lRegistrationInstanceName" ) as Literal;
            var rptFeeSummary = e.Item.FindControl( "rptFeeSummary" ) as Repeater;
            var registrationInformation = e.Item.DataItem as RegistrationInformation;

            if ( registrationInformation != null )
            {
                decimal? minimumInitialPaymentPerRegistrant = ( decimal? ) 0.00;
                decimal? defaultPaymentAmountPerRegistrant = ( decimal? ) 0.00;

                if ( registrationInformation.RegistrationInstanceState.RegistrationTemplate.SetCostOnInstance ?? false )
                {
                    minimumInitialPaymentPerRegistrant = registrationInformation.RegistrationInstanceState.MinimumInitialPayment;
                    defaultPaymentAmountPerRegistrant = registrationInformation.RegistrationInstanceState.DefaultPayment;
                }
                else
                {
                    minimumInitialPaymentPerRegistrant = registrationInformation.RegistrationInstanceState.RegistrationTemplate.MinimumInitialPayment;
                    defaultPaymentAmountPerRegistrant = registrationInformation.RegistrationInstanceState.RegistrationTemplate.DefaultPayment;
                }

                hfRegistrationInstanceId.Value = registrationInformation.RegistrationInstanceState.Id.ToString();

                var registrationName = registrationInformation.RegistrationInstanceState.Name;
                if ( registrationName.Contains( "Admin 2020" ) )
                {
                    registrationName = "Administrative Fee";
                }
                else
                {
                    registrationName = string.Format( "{0} - {1}", registrationInformation.RegistrationInstanceState.RegistrationTemplate.Name, registrationName );
                }

                lRegistrationInstanceName.Text = registrationName;
                // Get the cost/fee summary
                var costs = new List<RegistrationCostSummaryInfo>();
                foreach ( var registrant in registrationInformation.RegistrationState.Registrants )
                {
                    var costSummary = new RegistrationCostSummaryInfo();
                    costSummary.Type = RegistrationCostSummaryType.Cost;
                    costSummary.Description = string.Format(
                        "{0} {1}",
                        registrant.GetFirstName( registrationInformation.RegistrationInstanceState.RegistrationTemplate ),
                        registrant.GetLastName( registrationInformation.RegistrationInstanceState.RegistrationTemplate ) );

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
                        if ( registrationInformation.RegistrationState.DiscountPercentage > 0.0m && registrant.DiscountApplies )
                        {
                            if ( registrationInformation.RegistrationState.DiscountPercentage >= 1.0m )
                            {
                                costSummary.DiscountedCost = 0.0m;
                            }
                            else
                            {
                                costSummary.DiscountedCost = costSummary.Cost - ( costSummary.Cost * registrationInformation.RegistrationState.DiscountPercentage );
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

                    foreach ( var fee in registrant.FeeValues )
                    {
                        var templateFee = registrationInformation.RegistrationInstanceState.RegistrationTemplate.Fees.Where( f => f.Id == fee.Key ).FirstOrDefault();
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

                                var feeCostSummary = new RegistrationCostSummaryInfo();
                                feeCostSummary.Type = RegistrationCostSummaryType.Fee;
                                feeCostSummary.Description = desc;
                                feeCostSummary.Cost = feeInfo.Quantity * cost;

                                if ( registrationInformation.RegistrationState.DiscountPercentage > 0.0m && templateFee != null && templateFee.DiscountApplies && registrant.DiscountApplies )
                                {
                                    if ( registrationInformation.RegistrationState.DiscountPercentage >= 1.0m )
                                    {
                                        feeCostSummary.DiscountedCost = 0.0m;
                                    }
                                    else
                                    {
                                        feeCostSummary.DiscountedCost = feeCostSummary.Cost - ( feeCostSummary.Cost * registrationInformation.RegistrationState.DiscountPercentage );
                                    }
                                }
                                else
                                {
                                    feeCostSummary.DiscountedCost = feeCostSummary.Cost;
                                }

                                costs.Add( feeCostSummary );
                            }
                        }
                    }
                }

                // Add row for amount discount
                if ( registrationInformation.RegistrationState.DiscountAmount > 0.0m )
                {
                    decimal totalDiscount = 0.0m - ( registrationInformation.RegistrationState.Registrants.Where( r => r.DiscountApplies ).Count() * registrationInformation.RegistrationState.DiscountAmount );
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
                registrationInformation.RegistrationState.TotalCost = costs.Sum( c => c.Cost );
                registrationInformation.RegistrationState.DiscountedCost = costs.Sum( c => c.DiscountedCost );
                registrationInformation.RegistrationInstanceState.LoadAttributes();
                // Add row for totals
                var paymentDate = registrationInformation.RegistrationInstanceState.GetAttributeValue( "PaymentDate" ).AsDateTime();
                if ( paymentDate.HasValue )
                {
                    var futureTotal = new RegistrationCostSummaryInfo
                    {
                        Type = RegistrationCostSummaryType.Total,
                        Description = "Total Automatically Charged On " + paymentDate.Value.ToShortDateString(),
                        Cost = costs.Sum( c => c.Cost - c.MinPayment ),
                        DiscountedCost = registrationInformation.RegistrationState.DiscountedCost,
                    };

                    if ( costs.Sum( c => c.MinPayment ) > 0 )
                    {
                        costs.Add( new RegistrationCostSummaryInfo
                        {
                            Type = RegistrationCostSummaryType.Total,
                            Description = "Total Due Today",
                            Cost = costs.Sum( c => c.MinPayment ),
                            DiscountedCost = registrationInformation.minimumPayment.Value,
                        } );
                    }

                    costs.Add( futureTotal );

                }
                else
                {
                    costs.Add( new RegistrationCostSummaryInfo
                    {
                        Type = RegistrationCostSummaryType.Total,
                        Description = "Total Due Today",
                        Cost = costs.Sum( c => c.Cost ),
                        DiscountedCost = registrationInformation.RegistrationState.DiscountedCost,
                    } );

                }

                rptFeeSummary.DataSource = costs;
                rptFeeSummary.DataBind();
            }

        }



        protected void rptrRegistrantEvents_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var registrationInformation = e.Item.DataItem as RegistrationInformation;
            var registrants = registrationInformation.RegistrationState.Registrants.Where( r => !r.OnWaitList );
            if ( registrants.Any() )
            {
                var lRegistrantsReview = e.Item.FindControl( "lRegistrantsReview" ) as Literal;
                var rptrRegistrantsReview = e.Item.FindControl( "rptrRegistrantsReview" ) as Repeater;
                lRegistrantsReview.Text = string.Format(
                            "<p>The following {0} will be registered for {1}:",
                            registrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrantTerm.PluralizeIf( registrants.Count() > 1 ).ToLower(),
                            registrationInformation.RegistrationInstanceState.Name );

                rptrRegistrantsReview.DataSource = registrants
                    .Select( r => new
                    {
                        RegistrantName = r.GetFirstName( registrationInformation.RegistrationInstanceState.RegistrationTemplate ) + " " + r.GetLastName( registrationInformation.RegistrationInstanceState.RegistrationTemplate )
                    } );

                rptrRegistrantsReview.DataBind();
            }

        }

        protected void rptrWaitingListEvents_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var registrationInformation = e.Item.DataItem as RegistrationInformation;
            var waitingList = registrationInformation.RegistrationState.Registrants.Where( r => r.OnWaitList );
            if ( waitingList.Any() )
            {
                var lWaitingListReview = e.Item.FindControl( "lWaitingListReview" ) as Literal;
                var rptrWaitingListReview = e.Item.FindControl( "rptrWaitingListReview" ) as Repeater;
                lWaitingListReview.Text = string.Format(
                           "<p>The following {0} will be added to the waiting list for {1}:",
                           registrationInformation.RegistrationInstanceState.RegistrationTemplate.RegistrantTerm.PluralizeIf( waitingList.Count() > 1 ).ToLower(),
                           registrationInformation.RegistrationInstanceState.Name );

                rptrWaitingListReview.DataSource = waitingList
                    .Select( r => new
                    {
                        RegistrantName = r.GetFirstName( registrationInformation.RegistrationInstanceState.RegistrationTemplate ) + " " + r.GetLastName( registrationInformation.RegistrationInstanceState.RegistrationTemplate )
                    } );
                rptrWaitingListReview.DataBind();
            }

        }

        protected void lbPayNowNext_Click( object sender, EventArgs e )
        {
            NavigateToPaymentPage( true );

        }

        protected void lbCancelGroupMember_Click( object sender, EventArgs e )
        {
            pnlEditFamilyMember.Visible = false;
            ShowStart();
        }
    }
}