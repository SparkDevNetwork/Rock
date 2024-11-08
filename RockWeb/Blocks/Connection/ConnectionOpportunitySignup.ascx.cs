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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Connection Opportunity Signup" )]
    [Category( "Connection" )]
    [Description( "Block used to sign up for a connection opportunity." )]

    #region Block Attributes
    [BooleanField( "Display Home Phone",
        Description ="Whether to display home phone",
        DefaultBooleanValue = true,
        Order = 0,
        Key = AttributeKey.DisplayHomePhone )]

    [BooleanField( "Display Mobile Phone",
        Description = "Whether to display mobile phone",
        DefaultBooleanValue = true,
        Order = 1,
        Key = AttributeKey.DisplayMobilePhone )]

    [CodeEditorField( "Lava Template",
        Description = "Lava template to use to display the response message.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = true,
        DefaultValue = @"{% include '~~/Assets/Lava/OpportunityResponseMessage.lava' %}",
        Order = 2,
        Key = AttributeKey.LavaTemplate )]

    [BooleanField( "Enable Campus Context",
        Description = "If the page has a campus context its value will be used as a filter",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.EnableCampusContext )]

    [DefinedValueField( "Connection Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use for new individuals (default: 'Web Prospect'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
        Order = 5,
        Key = AttributeKey.ConnectionStatus )]

    [DefinedValueField( "Record Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = "283999EC-7346-42E3-B807-BCE9B2BABB49",
        Order = 6,
        Key = AttributeKey.RecordStatus )]

    [ConnectionOpportunityField( Name = "Connection Opportunity",
        Description = "If a Connection Opportunity is set, only details for it will be displayed (regardless of the querystring parameters).",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 7,
        Key = AttributeKey.ConnectionOpportunity )]

    [AttributeCategoryField(
        "Include Attribute Categories",
        Description = "Attributes in these Categories will be displayed.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.ConnectionRequest",
        IsRequired = false,
        Order = 8,
        Key = AttributeKey.IncludeAttributeCategories )]

    [AttributeCategoryField(
        "Exclude Attribute Categories",
        Description = "Attributes in these Categories will not be displayed.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.ConnectionRequest",
        IsRequired = false,
        Order = 9,
        Key = AttributeKey.ExcludeAttributeCategories )]

    [BooleanField( "Exclude Non-Public Connection Request Attributes",
        Description = "Attributes without 'Public' checked will not be displayed.",
        DefaultBooleanValue = true,
        Order = 10,
        Key = AttributeKey.ExcludeNonPublicAttributes )]

    [TextField("Comment Field Label",
        Description = "The label to apply to the comment field.",
        DefaultValue = "Comments",
        IsRequired = false,
        Order = 11,
        Key = AttributeKey.CommentFieldLabel)]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.CONNECTION_OPPORTUNITY_SIGNUP )]
    public partial class ConnectionOpportunitySignup : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DisplayHomePhone = "DisplayHomePhone";
            public const string DisplayMobilePhone = "DisplayMobilePhone";
            public const string LavaTemplate = "LavaTemplate";
            public const string EnableCampusContext = "EnableCampusContext";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string ConnectionOpportunity = "ConnectionOpportunity";
            public const string IncludeAttributeCategories = "IncludeAttributeCategories";
            public const string ExcludeAttributeCategories = "ExcludeAttributeCategories";
            public const string ExcludeNonPublicAttributes = "ExcludeNonPublicAttributes";
            public const string CommentFieldLabel = "CommentFieldLabel";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string PersonGuid = "PersonGuid";
            public const string OpportunityId = "OpportunityId";
        }

        #endregion

        #region Fields

        DefinedValueCache _homePhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
        DefinedValueCache _cellPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
        int _opportunityId = 0;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlOpportunityDetail );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbErrorMessage.Visible = false;
            _opportunityId = GetConnectionOpportunityId();

            if ( !Page.IsPostBack )
            {
                ShowDetail( _opportunityId );
            }

            base.OnLoad( e );
        }


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( GetConnectionOpportunityId() );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnConnect_Click( object sender, EventArgs e )
        {
            Page.Validate();

            if ( !Page.IsValid )
            {
                // Exit and allow the validation controls to render the appropriate error messages.
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var opportunityService = new ConnectionOpportunityService( rockContext );
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var personService = new PersonService( rockContext );

                // Get the opportunity and default status
                var opportunity = opportunityService
                    .Queryable()
                    .Where( o => o.Id == _opportunityId )
                    .FirstOrDefault();

                int defaultStatusId = opportunity.ConnectionType.ConnectionStatuses
                    .Where( s => s.IsDefault )
                    .Select( s => s.Id )
                    .FirstOrDefault();

                // If opportunity is valid and has a default status
                if ( opportunity != null && defaultStatusId > 0 )
                {
                    Person person = null;

                    string firstName = tbFirstName.Text.Trim();
                    string lastName = tbLastName.Text.Trim();
                    string email = tbEmail.Text.Trim();
                    string mobilePhoneNumber = pnMobile.Text.Trim();
                    int? campusId = cpCampus.SelectedCampusId;

                    // if a person guid was passed in from the query string use that
                    if ( RockPage.PageParameter( PageParameterKey.PersonGuid ) != null && !string.IsNullOrWhiteSpace( RockPage.PageParameter( PageParameterKey.PersonGuid ) ) )
                    {
                        Guid? personGuid = RockPage.PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();

                        if ( personGuid.HasValue )
                        {
                            person = personService.Get( personGuid.Value );
                        }
                    }
                    else if ( CurrentPerson != null &&
                      lastName.Equals( CurrentPerson.LastName, StringComparison.OrdinalIgnoreCase ) &&
                      ( firstName.Equals( CurrentPerson.NickName, StringComparison.OrdinalIgnoreCase ) || firstName.Equals( CurrentPerson.FirstName, StringComparison.OrdinalIgnoreCase ) ) &&
                      email.Equals( CurrentPerson.Email, StringComparison.OrdinalIgnoreCase ) )
                    {
                        // If the name and email entered are the same as current person (wasn't changed), use the current person
                        person = personService.Get( CurrentPerson.Id );
                    }

                    else
                    {
                        // Try to find matching person
                        var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, mobilePhoneNumber );
                        person = personService.FindPerson( personQuery, true );
                    }

                    // If person was not found, create a new one
                    if ( person == null )
                    {
                        // If a match was not found, create a new person
                        var dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                        var dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

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

                        PersonService.SaveNewPerson( person, rockContext, campusId, false );
                        person = personService.Get( person.Id );
                    }

                    // If there is a valid person with a primary alias, continue
                    if ( person != null && person.PrimaryAliasId.HasValue )
                    {

                        if ( pnHome.Visible )
                        {
                            SavePhone( pnHome, person, _homePhone.Guid );
                        }

                        if ( pnMobile.Visible )
                        {
                            SavePhone( pnMobile, person, _cellPhone.Guid );
                        }

                        // Now that we have a person, we can create the connection request
                        var connectionRequest = new ConnectionRequest();
                        connectionRequest.PersonAliasId = person.PrimaryAliasId.Value;
                        connectionRequest.Comments = tbComments.Text.Trim();
                        connectionRequest.ConnectionOpportunityId = opportunity.Id;
                        connectionRequest.ConnectionState = ConnectionState.Active;
                        connectionRequest.ConnectionStatusId = defaultStatusId;
                        connectionRequest.CampusId = campusId;
                        connectionRequest.ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campusId );
                        if ( campusId.HasValue &&
                            opportunity != null &&
                            opportunity.ConnectionOpportunityCampuses != null )
                        {
                            var campus = opportunity.ConnectionOpportunityCampuses
                                .Where( c => c.CampusId == campusId.Value )
                                .FirstOrDefault();
                            if ( campus != null )
                            {
                                connectionRequest.ConnectorPersonAliasId = campus.DefaultConnectorPersonAliasId;
                            }
                        }

                        if ( !connectionRequest.IsValid )
                        {
                            // Controls will show warnings
                            return;
                        }

                        // Save changes
                        avcAttributes.GetEditValues( connectionRequest );

                        rockContext.WrapTransaction( () =>
                        {
                            connectionRequestService.Add( connectionRequest );
                            rockContext.SaveChanges();
                            connectionRequest.SaveAttributeValues( rockContext );
                        } );

                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "Opportunity", new ConnectionOpportunityService( rockContext ).Get( _opportunityId ) );
                        mergeFields.Add( "CurrentPerson", CurrentPerson );
                        mergeFields.Add( "Person", person );

                        lResponseMessage.Text = GetAttributeValue( AttributeKey.LavaTemplate ).ResolveMergeFields( mergeFields );
                        lResponseMessage.Visible = true;

                        pnlSignup.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        public void ShowDetail( int opportunityId )
        {
            using ( var rockContext = new RockContext() )
            {
                var opportunity = new ConnectionOpportunityService( rockContext ).Get( opportunityId );
                if ( opportunity == null )
                {
                    pnlSignup.Visible = false;
                    ShowError( "Sorry", "The requested opportunity does not exist." );
                    return;
                }

                if ( !( opportunity.IsActive && opportunity.ConnectionType.IsActive ) )
                {
                    pnlSignup.Visible = false;
                    ShowError( "Inactive", "The opportunity is not currently active." );
                    return;
                }

                pnlSignup.Visible = true;

                if ( !string.IsNullOrWhiteSpace( opportunity.IconCssClass ) )
                {
                    lIcon.Text = string.Format( "<i class='{0}' ></i>", opportunity.IconCssClass );
                }

                lTitle.Text = opportunity.Name;
                tbComments.Label = GetAttributeValue( AttributeKey.CommentFieldLabel );

                // Hide show home phone
                pnlHomePhone.Visible = GetAttributeValue( AttributeKey.DisplayHomePhone ).AsBoolean(); // hide column
                pnHome.Visible = GetAttributeValue( AttributeKey.DisplayHomePhone ).AsBoolean(); // hide control

                // Hide show mobile phone
                pnlMobilePhone.Visible = GetAttributeValue( AttributeKey.DisplayMobilePhone ).AsBoolean(); // hide column
                pnMobile.Visible = GetAttributeValue( AttributeKey.DisplayMobilePhone ).AsBoolean(); // hide control

                Person registrant = null;

                if ( RockPage.PageParameter( PageParameterKey.PersonGuid ) != null )
                {
                    Guid? personGuid = RockPage.PageParameter( PageParameterKey.PersonGuid ).AsGuidOrNull();

                    if ( personGuid.HasValue )
                    {
                        registrant = new PersonService( rockContext ).Get( personGuid.Value );
                    }
                }

                if ( registrant == null && CurrentPerson != null )
                {
                    registrant = CurrentPerson;
                }

                if ( registrant != null )
                {
                    tbFirstName.Text = registrant.FirstName;
                    tbLastName.Text = registrant.LastName;
                    tbEmail.Text = registrant.Email;

                    if ( pnHome.Visible && _homePhone != null )
                    {
                        var homePhoneNumber = registrant.PhoneNumbers.Where( p => p.NumberTypeValueId == _homePhone.Id ).FirstOrDefault();
                        if ( homePhoneNumber != null )
                        {
                            pnHome.Number = homePhoneNumber.NumberFormatted;
                            pnHome.CountryCode = homePhoneNumber.CountryCode;
                        }
                    }

                    if ( pnMobile.Visible && _cellPhone != null )
                    {
                        var cellPhoneNumber = registrant.PhoneNumbers.Where( p => p.NumberTypeValueId == _cellPhone.Id ).FirstOrDefault();
                        if ( cellPhoneNumber != null )
                        {
                            pnMobile.Number = cellPhoneNumber.NumberFormatted;
                            pnMobile.CountryCode = cellPhoneNumber.CountryCode;
                        }
                    }
                }

                // load campus dropdown
                var campuses = CampusCache.All().Where( c => ( c.IsActive ?? false ) && opportunity.ConnectionOpportunityCampuses.Any( o => o.CampusId == c.Id ) ).ToList();
                cpCampus.Campuses = campuses;

                bool campusSelected = false;

                // If there is only one campus for this opportunity then select it automatically.
                if ( campuses.Count == 1 )
                {
                    cpCampus.SetValue( campuses.First().Id );
                    campusSelected = true;
                }

                // If there is more than one campus for the opportunity then try to set it to the Page Campus context
                if ( GetAttributeValue( AttributeKey.EnableCampusContext ).AsBoolean() && campusSelected == false )
                {
                    var contextCampus = RockPage.GetCurrentContext( EntityTypeCache.Get( "Rock.Model.Campus" ) ) as Campus;
                    if ( contextCampus != null && campuses.Where( c => c.Id == contextCampus.Id ).Any() )
                    {
                        cpCampus.SelectedCampusId = contextCampus.Id;
                        campusSelected = true;
                    }
                }

                if ( registrant != null && campusSelected == false )
                {
                    // If a campus has not yet been selected then use the registrant's campus if it is in the list of campuses for the opportunity.
                    var campus = registrant.GetCampus();
                    if ( campus != null && campuses.Where( c => c.Id == campus.Id ).Any() )
                    {
                        cpCampus.SelectedCampusId = campus.Id;
                    }
                }

                // Load Attributes
                var connectionRequest = new ConnectionRequest { Id = 0 };

                connectionRequest.ConnectionOpportunityId = opportunityId;

                var categoryService = new CategoryService( rockContext );

                var includedCategoryGuidList = GetAttributeValue( AttributeKey.IncludeAttributeCategories ).SplitDelimitedValues().AsGuidList();

                string[] includedCategoryNameList = null;

                if ( includedCategoryGuidList.Any() )
                {
                    includedCategoryNameList = categoryService.Queryable().Where( x => includedCategoryGuidList.Contains( x.Guid ) ).Select( x => x.Name ).ToArray();
                }

                avcAttributes.IncludedCategoryNames = includedCategoryNameList;

                var excludedCategoryGuidList = GetAttributeValue( AttributeKey.ExcludeAttributeCategories ).SplitDelimitedValues().AsGuidList();

                string[] excludedCategoryNameList = null;

                if ( excludedCategoryGuidList.Any() )
                {
                    excludedCategoryNameList = categoryService.Queryable().Where( x => excludedCategoryGuidList.Contains( x.Guid ) ).Select( x => x.Name ).ToArray();
                }

                avcAttributes.ExcludedCategoryNames = excludedCategoryNameList;

                if ( GetAttributeValue( AttributeKey.ExcludeNonPublicAttributes ).AsBooleanOrNull() ?? false )
                {
                    connectionRequest.LoadAttributes();
                    if ( connectionRequest.Attributes != null )
                    {
                        avcAttributes.ExcludedAttributes = connectionRequest.Attributes.Values.Where( a => !a.IsPublic ).ToArray();
                    }
                }

                avcAttributes.AddEditControls( connectionRequest, true );

                // show debug info
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Opportunity", new ConnectionOpportunityService( rockContext ).Get( _opportunityId ) );
                mergeFields.Add( "CurrentPerson", CurrentPerson );
            }
        }

        private void SavePhone( PhoneNumberBox phoneNumberBox, Person person, Guid phoneTypeGuid )
        {
            var numberType = DefinedValueCache.Get( phoneTypeGuid );
            if ( numberType != null )
            {
                var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );
                string oldPhoneNumber = phone != null ? phone.NumberFormattedWithCountryCode : string.Empty;
                string newPhoneNumber = PhoneNumber.CleanNumber( phoneNumberBox.Number );

                if ( newPhoneNumber != string.Empty )
                {
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
                    phone.CountryCode = PhoneNumber.CleanNumber( phoneNumberBox.CountryCode );
                    phone.Number = newPhoneNumber;
                }

            }
        }

        private void ShowError( string title, string message )
        {
            nbErrorMessage.Title = title;
            nbErrorMessage.Text = string.Format( "<p>{0}</p>", message );
            nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbErrorMessage.Visible = true;
        }

        /// <summary>
        /// Determines which item to display based on either the configuration or the connectionOpportunityId that was passed in.
        /// </summary>
        /// <returns>An <see cref="System.Int32"/> of the Id for a <see cref="Rock.Model.ConnectionOpportunity"/> or null if it was not found.</returns>
        private int GetConnectionOpportunityId()
        {
            Guid? connectionOpportunityGuid = GetAttributeValue( AttributeKey.ConnectionOpportunity ).AsGuidOrNull();
            int itemId = default( int );

            // A configured defined type takes precedence over any definedTypeId param value that is passed in.
            if ( connectionOpportunityGuid.HasValue )
            {
                var opportunity = new ConnectionOpportunityService( new RockContext() ).Get( connectionOpportunityGuid.Value );
                itemId = opportunity.Id;
            }
            else
            {
                itemId = PageParameter( PageParameterKey.OpportunityId ).AsInteger();
            }

            return itemId;
        }

        #endregion

    }
}