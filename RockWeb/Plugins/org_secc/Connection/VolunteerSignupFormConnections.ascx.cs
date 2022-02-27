// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace org.secc.Connection
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Volunteer Signup Form - Connections" )]
    [Category( "SECC > Connection" )]
    [Description( "Block used to sign up for a connection opportunity." )]

    [BooleanField( "Display Home Phone", "Whether to display home phone", true, "", 0 )]
    [BooleanField( "Display Mobile Phone", "Whether to display mobile phone", true, "", 1 )]
    [BooleanField( "Display Birthdate", "Whether to display birthdate", true, "", 2 )]
    [BooleanField("Display Comments", "Whether to display the comments box", true, "", 3)]
    [TextField("Connect Button Text", "The wording that should be used for the connect button", true, "Connect", "", 4 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the response message.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/OpportunityResponseMessage.lava' %}", "", 5 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 6 )]
    [BooleanField( "Enable Campus Context", "If the page has a campus context it's value will be used as a filter", true, "", 7 )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 8 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 9 )]
    [TextField( "Group Member Attribute Keys - URL", "The key of any group member attributes that you would like to be set via the URL.  Enter as comma separated values.", false, key: "UrlKeys", order: 10)]
    [TextField( "Group Member Attribute Keys - Form", "The key of the group member attributes to show an edit control for on the opportunity signup.  Enter as comma separated values.", false, key: "FormKeys", order: 11 )]
    [BooleanField( "Display Add Another", "Whether to display the \"Connect and Add Another\" button", false, "", 12 )]
    [TextField( "Comment label text", "The wording that should be used for the comment box title", true, "Comments", "", 13 )]
    [BooleanField( "Comments Required", "Whether the comment are required", true, "", 14 )]
     
    public partial class VolunteerSignupFormConnections : RockBlock, IDetailBlock
    {
        #region Fields

        DefinedValueCache _homePhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
        DefinedValueCache _cellPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
        
        private List<ConnectionRoleRequest> _roleRequests = null;
        List<ConnectionRoleRequest> RoleRequests
        {
            get
            {

                if ( _roleRequests == null )
                {
                    _roleRequests = PageParameter( "RoleRequests" ).FromJsonOrNull<List<ConnectionRoleRequest>>();
                    if ( _roleRequests == null )
                    {
                        _roleRequests = new List<ConnectionRoleRequest>();
                        var roleRequest = new ConnectionRoleRequest();
                        roleRequest.GroupId = PageParameter( "GroupId" ).AsInteger();
                        roleRequest.GroupTypeRoleId = PageParameter( "GroupTypeRoleId" ).AsInteger();
                        if ( roleRequest.GroupTypeRoleId == 0 && PageParameter( "GroupTypeRole" ).AsGuidOrNull().HasValue )
                        {
                            GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( new RockContext() );
                            roleRequest.GroupTypeRoleId = groupTypeRoleService.Get( PageParameter( "GroupTypeRole" ).AsGuid() ).Id;
                        }
                        _roleRequests.Add( roleRequest );
                    }
                    foreach ( var roleRequest in _roleRequests )
                    {
                        if ( roleRequest.Attributes == null )
                        {
                            roleRequest.Attributes = new Dictionary<string, string>();
                            var urlKeys = GetAttributeValues( "UrlKeys" );
                            foreach ( string urlKey in urlKeys )
                            {
                                if ( !string.IsNullOrEmpty( PageParameter( urlKey ) ) )
                                {
                                    roleRequest.Attributes.Add( urlKey, PageParameter( urlKey ) );
                                }
                            }
                        }
                    }

                }

                // Handle any situation where we have a 0 role id
                if (_roleRequests.Any(rr => rr.GroupId > 0 && rr.GroupTypeRoleId == 0))
                {
                    GroupService groupService = new GroupService( new RockContext() );
                    foreach ( var roleRequest in _roleRequests )
                    {

                        if ( roleRequest.GroupId > 0 && roleRequest.GroupTypeRole.AsGuidOrNull().HasValue )
                        {
                            GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( new RockContext() );
                            roleRequest.GroupTypeRoleId = groupTypeRoleService.Get( roleRequest.GroupTypeRole.AsGuid() ).Id;
                        }

                        // Get the Default role from this group
                        if ( roleRequest.GroupId > 0 && roleRequest.GroupTypeRoleId == 0 )
                        {
                            Group group = groupService.Get( roleRequest.GroupId );
                            if ( group != null && group.GroupType.DefaultGroupRoleId.HasValue )
                            {
                                roleRequest.GroupTypeRoleId = group.GroupType.DefaultGroupRoleId.Value;
                            }
                        }
                    }
                }
                

                return _roleRequests;
            }
        }
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
            base.OnLoad( e );

            nbErrorMessage.Visible = false;

            btnConnectandAddAnother.Visible = GetAttributeValue( "DisplayAddAnother" ).AsBoolean();

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "OpportunityId" ).AsInteger() );
            }
        }


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "OpportunityId" ).AsInteger() );
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
            using ( var rockContext = new RockContext() )
            {
                var opportunityService = new ConnectionOpportunityService( rockContext );
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var personService = new PersonService( rockContext );

                // Get the opportunity and default status
                int opportunityId = PageParameter( "OpportunityId" ).AsInteger();
                var opportunity = opportunityService
                    .Queryable()
                    .Where( o => o.Id == opportunityId )
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
                    DateTime? birthdate = bpBirthdate.SelectedDate;
                    string email = tbEmail.Text.Trim();
                    int? campusId = cpCampus.SelectedCampusId;

                    // if a person guid was passed in from the query string use that
                    if ( RockPage.PageParameter( "PersonGuid" ) != null && !string.IsNullOrWhiteSpace( RockPage.PageParameter( "PersonGuid" ) ) )
                    {
                        Guid? personGuid = RockPage.PageParameter( "PersonGuid" ).AsGuidOrNull();

                        if ( personGuid.HasValue )
                        {
                            person = personService.Get( personGuid.Value );
                        }
                    }
                    else if ( CurrentPerson != null &&
                      CurrentPerson.LastName.Equals( lastName, StringComparison.OrdinalIgnoreCase ) &&
                      ( CurrentPerson.NickName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) || CurrentPerson.FirstName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) ) &&
                      CurrentPerson.Email.Equals( email, StringComparison.OrdinalIgnoreCase ) )
                    {
                        // If the name and email entered are the same as current person (wasn't changed), use the current person
                        person = personService.Get( CurrentPerson.Id );
                    }

                    else
                    {
                        List<Person> personMatches = new List<Person>();
                        if ( Assembly.GetExecutingAssembly().GetReferencedAssemblies()
                            .FirstOrDefault( c => c.FullName == "org.secc.PersonMatch" ) != null )
                        {
                            var assembly = Assembly.Load( "org.secc.PersonMatch" );
                            if (assembly != null) 
                            {
                                Type type = assembly.GetExportedTypes().Where(et => et.FullName == "org.secc.PersonMatch.Extension" ).FirstOrDefault();
                                if ( type != null)
                                {
                                    var matchMethod = type.GetMethod( "GetByMatch" );
                                    personMatches = ( ( IEnumerable<Person> ) matchMethod.Invoke( null, new object[] { personService, firstName, lastName, birthdate, email, null, null, null } ) ).ToList();
                                }
                            }
                        }
                        else
                        {
                            personMatches = personService.FindPersons( firstName, lastName, email ).ToList();
                            if ( bpBirthdate.Visible )
                            {
                                personMatches = personMatches.Where( p => p.BirthDate == birthdate ).ToList();
                            }
                        }

                        if ( personMatches.Count() == 1 && 
                            personMatches.First().Email != null && 
                            email.ToLower().Trim() == personMatches.First().Email.ToLower().Trim() )
                        {
                            // If one person with same name and email address exists, use that person
                            person = personMatches.First();
                        }
                    }

                    // If person was not found, create a new one
                    if ( person == null )
                    {
                        // If a match was not found, create a new person
                        var dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
                        var dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );

                        person = new Person();
                        person.FirstName = firstName;
                        person.LastName = lastName;
                        person.IsEmailActive = true;
                        person.SetBirthDate(birthdate);
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
                        var changes = new History.HistoryChangeList();

                        if ( pnHome.Visible )
                        {
                            SavePhone( pnHome, person, _homePhone.Guid, changes );
                        }

                        if ( pnMobile.Visible )
                        {
                            SavePhone( pnMobile, person, _cellPhone.Guid, changes );
                        }

                        // Save the DOB
                        if (bpBirthdate.Visible && bpBirthdate.SelectedDate.HasValue && bpBirthdate.SelectedDate != person.BirthDate)
                        {
                            person.BirthDay = bpBirthdate.SelectedDate.Value.Day;
                            person.BirthMonth = bpBirthdate.SelectedDate.Value.Month;
                            person.BirthYear = bpBirthdate.SelectedDate.Value.Year;
                        }

                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                                person.Id,
                                changes );
                        }

                        // Now that we have a person, we can create the connection requests
                        int RepeaterIndex = 0;
                        foreach ( ConnectionRoleRequest roleRequest in RoleRequests )
                        {
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

                            var hdnGroupId = ( ( HiddenField ) ( rptGroupRoleAttributes.Items[RepeaterIndex].FindControl( "hdnGroupId" ) ) );
                            var hdnGroupRoleTypeId = ( ( HiddenField ) ( rptGroupRoleAttributes.Items[RepeaterIndex].FindControl( "hdnGroupRoleTypeId" ) ) );


                            if ( hdnGroupId.Value.AsInteger() > 0 && hdnGroupRoleTypeId.Value.AsInteger() > 0 )
                            {
                                connectionRequest.AssignedGroupId = hdnGroupId.Value.AsInteger();
                                connectionRequest.AssignedGroupMemberRoleId = hdnGroupRoleTypeId.Value.AsInteger();
                                var groupConfig = opportunity.ConnectionOpportunityGroupConfigs.Where( gc => gc.GroupMemberRoleId == hdnGroupRoleTypeId.Value.AsInteger() ).FirstOrDefault();
                                connectionRequest.AssignedGroupMemberStatus = groupConfig.GroupMemberStatus;

                            }

                            var connectionAttributes = GetGroupMemberAttributes( rockContext, RepeaterIndex);

                            if ( connectionAttributes != null && connectionAttributes.Keys.Any() )
                            {
                                var connectionDictionary = new Dictionary<string, string>();
                                foreach(var kvp in connectionAttributes )
                                {
                                    connectionDictionary.Add( kvp.Key, kvp.Value.Value );
                                }

                                connectionRequest.AssignedGroupMemberAttributeValues = connectionDictionary.ToJson();
                            }

                            if ( !connectionRequest.IsValid )
                            {
                                // Controls will show warnings
                                return;
                            }

                            connectionRequestService.Add( connectionRequest );

                            RepeaterIndex++;
                        }

                        rockContext.SaveChanges();

                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "Opportunity", new ConnectionOpportunityService( rockContext ).Get( PageParameter( "OpportunityId" ).AsInteger() ) );
                        mergeFields.Add( "CurrentPerson", CurrentPerson );
                        mergeFields.Add( "Person", person );

                        lResponseMessage.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );
                        lResponseMessage.Visible = true;

                        if ((( LinkButton )sender).Text == "Connect and Add Another")
                        {
                            ShowDetail( opportunityId, true );
                            AddGroupMemberAttributes( rockContext );
                            pnlSignup.Visible = true;
                        } else
                        {
                            pnlSignup.Visible = false;
                        }
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
            ShowDetail( opportunityId, false );
        }


        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        /// <param name="addAnother">Is this a request to add Another person?  If so always show a blank form.</param>
        public void ShowDetail( int opportunityId, Boolean addAnother )
        {
            using ( var rockContext = new RockContext() )
            {
                var opportunity = new ConnectionOpportunityService( rockContext ).Get( opportunityId );
                if ( opportunity == null )
                {
                    pnlSignup.Visible = false;
                    ShowError( "Incorrect Opportunity Type", "The requested opportunity does not exist." );
                    return;
                }

                // load campus dropdown
                var campuses = CampusCache.All().Where( c => ( c.IsActive ?? false ) && opportunity.ConnectionOpportunityCampuses.Any( o => o.CampusId == c.Id ) ).ToList();
                cpCampus.Campuses = campuses;
                cpCampus.Visible = campuses.Any();

                if ( campuses.Any() )
                {
                    cpCampus.SetValue( campuses.First().Id );
                }

                if ( (!string.IsNullOrEmpty( PageParameter( "CampusId" ) ) && opportunity.ConnectionOpportunityCampuses.Any( o => o.CampusId == PageParameter( "CampusId" ).AsInteger() ))
                    || ( !string.IsNullOrEmpty( PageParameter( "Campus" ) ) && opportunity.ConnectionOpportunityCampuses.Any( o => o.Campus.Guid == PageParameter( "Campus" ).AsGuidOrNull() ) ) )
                {
                    if ( PageParameter( "Campus" ).AsGuidOrNull().HasValue )
                    {
                        cpCampus.SetValue( CampusCache.Get( PageParameter( "Campus" ).AsGuid() ).Id );
                        ltCampus.Text = CampusCache.Get( PageParameter( "Campus" ).AsGuid() ).Name;
                    } else
                    {
                        cpCampus.SetValue( PageParameter( "CampusId" ).AsInteger() );
                        ltCampus.Text = CampusCache.Get( PageParameter( "CampusId" ).AsInteger() ).Name;
                    }
                    cpCampus.CssClass = "hidden";
                    cpCampus.Label = null;
                    ltCampus.Visible = true;
                }

                pnlSignup.Visible = true;

                if ( !string.IsNullOrWhiteSpace( opportunity.IconCssClass ) )
                {
                    lIcon.Text = string.Format( "<i class='{0}' ></i>", opportunity.IconCssClass );
                }
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Opportunity", new ConnectionOpportunityService( rockContext ).Get( PageParameter( "OpportunityId" ).AsInteger() ) );
                mergeFields.Add( "CurrentPerson", CurrentPerson );
                lTitle.Text = opportunity.Name;
                btnConnect.Text = GetAttributeValue( "ConnectButtonText" );
                
                

                divHome.Visible = pnHome.Visible = GetAttributeValue( "DisplayHomePhone" ).AsBoolean();
                divMobile.Visible = pnMobile.Visible = GetAttributeValue( "DisplayMobilePhone" ).AsBoolean();
                divBirthdate.Visible = bpBirthdate.Visible = GetAttributeValue( "DisplayBirthdate" ).AsBoolean();
                tbComments.Visible = GetAttributeValue( "DisplayComments" ).AsBoolean();

                if ( tbComments.Visible == true )
                {
                    tbComments.Label = GetAttributeValue( "Commentlabeltext" ).ResolveMergeFields( mergeFields );
                    tbComments.Required = GetAttributeValue( "CommentsRequired" ).AsBoolean();
                }

                // If any of these aren't showing then set the width to be a bit wider on the columns
                if ( !(divHome.Visible && divMobile.Visible && divBirthdate.Visible ))
                {
                    divHome.RemoveCssClass( "col-md-4" );
                    divHome.AddCssClass( "col-md-6" );
                    divMobile.RemoveCssClass( "col-md-4" );
                    divMobile.AddCssClass( "col-md-6" );
                    divBirthdate.RemoveCssClass( "col-md-4" );
                    divBirthdate.AddCssClass( "col-md-6" );
                }

                Person registrant = null;

                if ( RockPage.PageParameter( "PersonGuid" ) != null )
                {
                    Guid? personGuid = RockPage.PageParameter( "PersonGuid" ).AsGuidOrNull();

                    if ( personGuid.HasValue )
                    {
                        registrant = new PersonService( rockContext ).Get( personGuid.Value );
                    }
                }

                if ( registrant == null && CurrentPerson != null )
                {
                    registrant = CurrentPerson;
                }

                if ( addAnother == true )
                {

                    tbFirstName.Text = null;
                    tbLastName.Text = null;
                    tbEmail.Text = null;
                    bpBirthdate.SelectedDate = null;
                    pnHome.Number = null;
                    pnHome.CountryCode = null;
                    pnMobile.Number = null;
                    pnMobile.CountryCode = null;
                }
                else if ( registrant != null )
                {
                    tbFirstName.Text = registrant.FirstName.EncodeHtml();
                    tbLastName.Text = registrant.LastName.EncodeHtml();
                    tbEmail.Text = registrant.Email.EncodeHtml();
                    bpBirthdate.SelectedDate = registrant.BirthDate;

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

                    var campus = registrant.GetCampus();
                    if ( campus != null )
                    {
                        cpCampus.SelectedCampusId = campus.Id;
                    }
                }
                else
                {
                    // set the campus to the context
                    if ( GetAttributeValue( "EnableCampusContext" ).AsBoolean() )
                    {
                        var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
                        var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                        if ( contextCampus != null )
                        {
                            cpCampus.SelectedCampusId = contextCampus.Id;
                        }
                    }
                }


                // Role
                rptGroupRoleAttributes.DataSource = RoleRequests;
                rptGroupRoleAttributes.DataBind();
                int repeaterIndex = 0;
                foreach (ConnectionRoleRequest roleRequest in RoleRequests)
                {
                    if ( opportunity.ConnectionOpportunityGroupConfigs.Where( gc => gc.GroupMemberRoleId == roleRequest.GroupTypeRoleId || gc.GroupMemberRole.Guid.ToString() == roleRequest.GroupTypeRole ).Any() )
                    {
                        var groupConfig = opportunity.ConnectionOpportunityGroupConfigs.Where( gc => gc.GroupMemberRoleId == roleRequest.GroupTypeRoleId || gc.GroupMemberRole.Guid.ToString() == roleRequest.GroupTypeRole ).FirstOrDefault();
                        ( ( Literal ) ( rptGroupRoleAttributes.Items[repeaterIndex].FindControl( "ltRole" ) ) ).Text = groupConfig.GroupType.Name + " - " + groupConfig.GroupMemberRole.Name;
                        ( ( Literal ) ( rptGroupRoleAttributes.Items[repeaterIndex].FindControl( "ltRole" ) ) ).Visible = true;
                        ( ( HiddenField ) ( rptGroupRoleAttributes.Items[repeaterIndex].FindControl( "hdnGroupRoleTypeId" ) ) ).Value = groupConfig.GroupMemberRole.Id.ToString();

                    }
                    repeaterIndex++;
                }


                // show debug info
 
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
        }

        private void SavePhone( PhoneNumberBox phoneNumberBox, Person person, Guid phoneTypeGuid, History.HistoryChangeList changes )
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

                    History.EvaluateChange(
                        changes,
                        string.Format( "{0} Phone", numberType.Value ),
                        oldPhoneNumber,
                        phone.NumberFormattedWithCountryCode );
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

        protected override void CreateChildControls()
        {
            AddGroupMemberAttributes();
        }

        private void AddGroupMemberAttributes( RockContext rockContext = null )
        {
            // Group
            if ( RoleRequests.Count > 0 && rptGroupRoleAttributes.Items.Count > 0 )
            {
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }
                var viewStateAttributes = new List<Dictionary<string, string>>();
                var RepeaterIndex = 0;
                foreach(var roleRequest in RoleRequests)
                {
                    var hdnGroupId = ( ( HiddenField ) ( rptGroupRoleAttributes.Items[RepeaterIndex].FindControl( "hdnGroupId" ) ) );
                    var phAttributes = ( ( PlaceHolder ) ( rptGroupRoleAttributes.Items[RepeaterIndex].FindControl( "phAttributes" ) ) );
                    hdnGroupId.Value = roleRequest.GroupId.ToString();

                    var group = new GroupService( rockContext ).Get( hdnGroupId.Value.AsInteger() );
                    if ( group != null )
                    {
                        // Group Attributes
                        var formKeys = GetAttributeValues( "FormKeys" );
                        var urlKeys = GetAttributeValues( "UrlKeys" );

                        AttributeService attributeService = new AttributeService( rockContext );

                        string groupQualifierValue = group.Id.ToString();
                        string groupTypeQualifierValue = group.GroupTypeId.ToString();

                        // Make a fake group member so we can load some attributes.
                        GroupMember groupMember = new GroupMember();
                        groupMember.Group = group;
                        groupMember.GroupId = group.Id;
                        groupMember.LoadAttributes();

                        // Store URL Keys into the ViewState
                        var viewStateAttribute = new Dictionary<string, string>();
                        foreach ( string urlKey in urlKeys )
                        {
                            if ( roleRequest.Attributes != null && roleRequest.Attributes.ContainsKey(urlKey) && !string.IsNullOrEmpty( roleRequest.Attributes[ urlKey ] ) && groupMember.Attributes.ContainsKey( urlKey ) )
                            {
                                groupMember.SetAttributeValue( urlKey, roleRequest.Attributes[urlKey] );
                                viewStateAttribute.Add( urlKey, roleRequest.Attributes[urlKey] );
                            }
                        }
                        viewStateAttributes.Add( viewStateAttribute );

                        Helper.AddDisplayControls( groupMember, phAttributes, groupMember.Attributes.Where( a => !urlKeys.Contains( a.Key )).Select(a => a.Key).ToList(), true, false );
                        Helper.AddEditControls( "", formKeys, groupMember, phAttributes,  tbLastName.ValidationGroup,  false, new List<String>() );

                    }
                    RepeaterIndex++;
                }
                ViewState.Add( "SelectedAttributes", viewStateAttributes );
                SaveViewState();
            }
        }


        private Dictionary<string, AttributeValueCache> GetGroupMemberAttributes( RockContext rockContext = null, int RepeaterIndex = 0)
        {
            // Group
            if ( RoleRequests.Count > 0 )
            {
                if ( rockContext == null )
                {
                    rockContext = new RockContext();
                }

                var hdnGroupId = ( ( HiddenField ) ( rptGroupRoleAttributes.Items[RepeaterIndex].FindControl( "hdnGroupId" ) ) );
                var phAttributes = ( ( PlaceHolder ) ( rptGroupRoleAttributes.Items[RepeaterIndex].FindControl( "phAttributes" ) ) );
                hdnGroupId.Value = RoleRequests[RepeaterIndex].GroupId.ToString();

                var group = new GroupService( rockContext ).Get( hdnGroupId.Value.AsInteger() );
                if ( group != null )
                {
                    // Make a fake group member so we can load some attributes.
                    GroupMember groupMember = new GroupMember();
                    groupMember.Group = group;
                    groupMember.GroupId = group.Id;
                    groupMember.LoadAttributes();

                    Helper.GetEditValues( phAttributes, groupMember );

                    var readonlyAttributes = ( List<Dictionary<string, string>> ) ViewState["SelectedAttributes"];
                    if ( readonlyAttributes != null && readonlyAttributes.Count > RepeaterIndex && readonlyAttributes[RepeaterIndex].Keys.Count > 0 )
                    {
                        foreach ( var kvp in readonlyAttributes[RepeaterIndex] )
                        {
                            if ( groupMember.AttributeValues.ContainsKey( kvp.Key ) )
                            {
                                groupMember.AttributeValues[kvp.Key].Value = kvp.Value;
                            }
                        }
                    }
                    return groupMember.AttributeValues;
                }
            }
            return null;
        }

        #endregion


        public class ConnectionRoleRequest
        {
            public int GroupId { get; set; }

            public int GroupTypeRoleId { get; set; }

            public string GroupTypeRole { get; set; }

            public Dictionary<string, string> Attributes { get; set; }
        }
    }
}