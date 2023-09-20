using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_lakepointe.Groups
{
    [DisplayName( "Group Information Request" )]
    [Category( "LPC > Groups" )]
    [Description( "Allows a person to request information about a group." )]

    [GroupTypesField( "Allowed Group Types", "This setting resticts which types of groups a person can be added to, however, selecting a specific group via the Group settign will override this restriction.", true, Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP, "", 0 )]
    [GroupField( "Group", "Optional group to request information for. If omitted, the group's GUID should be passed via the query string (GroupGuid=).", false, "", "", 1 )]
    [BooleanField( "Enable Passing Group Id", "If enabled, allows the ability to pass the group's Id (GroupID=) instead of the GUID", true, "", 2 )]
    [CustomRadioListField( "Mode", "The mode to use when displaying registration details.", "Simple^Simple,Full^Full", true, "Simple", "", 3 )]
    [DefinedValueField( "2E6540EA-63F0-40FE-BE50-F2A84735E600", "Connection Status", "The connection status to use for new individuals (default: 'Web Prospect'.)", true, false, "368DD475-242C-49C4-A42C-7278BE690CC2", "", 4 )]
    [DefinedValueField( "8522BADD-2871-45A5-81DD-C76DA07E2E7E", "Record Status", "The record status to use for new individuals (default: 'Pending'.)", true, false, "283999EC-7346-42E3-B807-BCE9B2BABB49", "", 5 )]
    [WorkflowTypeField( "Workflow", "An workflow to start when request is created. The Group will set as the workflow 'Entity' when processing is started.", false, true, "", "", 6 )]
    [TextField( "Workflow Message Attribute Key", "The Key of the Workflow Attribute that will store the message that the user entered.", false, "", "", 7 )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group details.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
", "", 8 )]
    [LinkedPage( "Result Page", "An optional page to redirect user to after they have requested information for the group.", false, "", "", 9 )]
    [LinkedPage( "Group Finder Page", "An optional page to link the user to if a group selection is not provided or is not valid.", false, "", "", 10 )]
    [CodeEditorField( "Result Lava Template", "The lava template to use to format result message after user's request has been submitted. Will only display if user is not redirected to a Result Page ( previous setting ).", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"
", "", 11 )]
    [CustomRadioListField( "Auto Fill Form", "If set to FALSE then the form will not load the context of the logged in user (default: 'True'.)", "true^True,false^False", true, "true", "", 12 )]
    [TextField( "Submit Button Alt Text", "Alternate text to use for the Submit button (default is 'Submit').", false, "", "", 13 )]
    [BooleanField( "Enable Debug", "Shows the fields available to merge in lava.", false, "", 14 )]

    public partial class GroupInfoRequest : RockBlock
    {
        #region Fields
        RockContext _rockContext = null;
        string _mode = "Simple";
        Group _group = null;
        DefinedValueCache _dvcConnectionStatus = null;
        DefinedValueCache _dvcRecordStatus = null;
        DefinedValueCache _homeAddressType = null;
        GroupTypeCache _familyType = null;
        GroupTypeRoleCache _adultRole = null;
        bool _autoFill = true;
        bool _isValidSettings = true;
        string _groupListPageUrl = null;
        bool _allowMessage = true;

        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether this instance is simple.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is simple; otherwise, <c>false</c>.
        /// </value>
        protected bool IsSimple
        {
            get
            {
                return _mode == "Simple";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is full.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is full; otherwise, <c>false</c>.
        /// </value>
        protected bool IsFull
        {
            get
            {
                return _mode == "Full";
            }
        }
        #endregion

        #region Control Methods

        /// <summary>
        /// Executes on every user control initization (an early event on page load)
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }


        /// <summary>
        /// Executes every time that the user control loads. Happens later in the lifecycle than init
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // check to make sure settings are valid
            if ( !CheckSettings() )
            {
                // if not show warning and hide view panel
                _isValidSettings = false;
                nbNotice.Visible = true;
                pnlView.Visible = false;
            }
            else
            {
                //if valid hide notice and load block
                nbNotice.Visible = false;
                pnlView.Visible = true;

                if ( !Page.IsPostBack )
                {
                    //populate the form details if initial load
                    ShowDetails();
                }
            }
        }

        #endregion

        #region Events        
        /// <summary>
        /// Executes the block updated event (i.e. when the block settings have been adjusted)
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            //reload the form
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnSubmit control (form submission.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid || !_isValidSettings )
            {
                //if page not valid do not proceed further
                return;
            }

            //load the context and initialize Person Service
            _rockContext = _rockContext ?? new RockContext();
            var personSvc = new PersonService( _rockContext );

            // reset/initialize values
            Person person = null;
            Group family = null;
            GroupLocation homeLocation = null;
            bool isMatch = false;


            if ( _autoFill )
            {
                //if the form supports autoFill 
                if ( IsCurrentPerson() )
                {
                    //if the information matches that of the currently logged in user, load the record
                    person = personSvc.Get( CurrentPerson.Id );
                    isMatch = true;
                }
            }

            if ( person == null )
            {
                //if person object is null, try to find a match 
                var matches = personSvc.FindPersons( tbFirstName.Text.Trim(), tbLastName.Text.Trim(), tbEmail.Text.Trim() );
                if ( matches.Count() == 1 )
                {
                    //only if there is a single match load the person and toggle the isMatch flag
                    person = matches.First();
                    isMatch = true;
                }

            }

            // if person still null create new record
            if ( person == null )
            {
                person = new Person();
                person.FirstName = tbFirstName.Text.Trim();
                person.LastName = tbLastName.Text.Trim();
                person.Email = tbEmail.Text.Trim();
                person.IsEmailActive = true;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                person.ConnectionStatusValueId = _dvcConnectionStatus.Id;
                person.RecordStatusValueId = _dvcRecordStatus.Id;
                person.Gender = Gender.Unknown;

                family = PersonService.SaveNewPerson( person, _rockContext, _group.CampusId, false );

            }
            else
            {
                // **note** only email updated due to impersonization concerns
                // updating current existing person
                person.Email = tbEmail.Text;

                // Get the current person's families
                var families = person.GetFamilies( _rockContext );

                // If address can being entered, look for first family with a home location
                if ( !IsSimple )
                {
                    foreach ( var aFamily in families )
                    {
                        homeLocation = aFamily.GroupLocations
                            .Where( l =>
                                l.GroupLocationTypeValueId == _homeAddressType.Id &&
                                l.IsMappedLocation )
                            .FirstOrDefault();
                        if ( homeLocation != null )
                        {
                            family = aFamily;
                            break;
                        }
                    }
                }

                // If a family wasn't found with a home location, use the person's first family
                if ( family == null )
                {
                    family = families.FirstOrDefault();
                }
            }

            // If using a 'Full' view, save the phone numbers and address
            if ( !IsSimple )
            {
                // if a new reocrd and home phone provided
                if ( !isMatch || !string.IsNullOrWhiteSpace( pnHome.Number ) )
                {
                    SetPhoneNumber( _rockContext, person, pnHome, null, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                }
                //if new record and mobile/cell number provided
                if ( !isMatch || !string.IsNullOrWhiteSpace( pnCell.Number ) )
                {
                    SetPhoneNumber( _rockContext, person, pnCell, cbSMS, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                }

                //if new record and address provided create a home addrss for family
                if ( !isMatch || !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                {
                    string oldLocation = homeLocation != null ? homeLocation.Location.ToString() : string.Empty;
                    string newLocation = string.Empty;

                    var location = new LocationService( _rockContext ).Get( acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                    if ( location != null )
                    {
                        if ( homeLocation == null )
                        {
                            homeLocation = new GroupLocation();
                            homeLocation.GroupLocationTypeValueId = _homeAddressType.Id;
                            family.GroupLocations.Add( homeLocation );
                        }
                        else
                        {
                            oldLocation = homeLocation.Location.ToString();
                        }

                        homeLocation.Location = location;
                        newLocation = location.ToString();
                    }
                    else
                    {
                        if ( homeLocation != null )
                        {
                            homeLocation.Location = null;
                            family.GroupLocations.Remove( homeLocation );
                            new GroupLocationService( _rockContext ).Delete( homeLocation );
                        }
                    }
                }

                _rockContext.SaveChanges();

            }

            // Start the Workflow for the request
            WorkflowTypeCache workflowType = null;
            Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                try
                {
                    workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                    List<string> workflowErrors;
                    var workflow = Workflow.Activate( workflowType, person.FullName );
                    workflow.InitiatorPersonAliasId = person.PrimaryAliasId;

                    // if message is allowed (key provided) and message provided add to workflow
                    if ( _allowMessage && !String.IsNullOrWhiteSpace( tbMessage.Text ) )
                    {
                        workflow.SetAttributeValue( GetAttributeValue( "WorkflowMessageAttributeKey" ), tbMessage.Text.Trim() );
                    }

                    new WorkflowService( _rockContext ).Process( workflow, _group, out workflowErrors );
                }
                catch ( Exception ex )
                {

                    ExceptionLogService.LogException( ex, this.Context );
                }


                // Show the results
                pnlView.Visible = false;
                pnlResult.Visible = true;

                // Show lava content
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Group", _group );
                mergeFields.Add( "Person", person );
                mergeFields.Add( "CurrentPerson", CurrentPerson );


                string template = GetAttributeValue( "ResultLavaTemplate" );
                lResult.Text = template.ResolveMergeFields( mergeFields );

                // Will only redirect if a value is specifed
                NavigateToLinkedPage( "ResultPage" );

            }

        }

        #endregion

        #region Internal Methods        
        /// <summary>
        /// Builds the group error message if an error exists.  
        /// If Group List page provided, appends link 
        /// </summary>
        /// <param name="baseMessage">The base message.</param>
        /// <returns>Formatted and possible linked error message</returns>
        public string BuildGroupErrorMessage( string baseMessage )
        {
            // base message empty, return
            if ( String.IsNullOrWhiteSpace( baseMessage ) )
            {
                return baseMessage;
            }

            // if Group list page  provided append "return" url.
            if ( !String.IsNullOrWhiteSpace( _groupListPageUrl ) )
            {
                return string.Format( "<p>{0} Please <a href=\"{1}\">click here</a> to select a group.</p>", baseMessage, _groupListPageUrl );
            }
            else
            {
                return string.Format( "<p>{0}</p>", baseMessage );
            }
        }


        /// <summary>
        /// Verifies and initializes block settings
        /// </summary>
        /// <returns>a true/false flag indicating if settings are valid</returns>
        private bool CheckSettings()
        {
            _rockContext = _rockContext ?? new RockContext();

            _mode = GetAttributeValue( "Mode" );

            _autoFill = GetAttributeValue( "AutoFillForm" ).AsBoolean();

            _groupListPageUrl = LinkedPageUrl( "GroupFinderPage" );


            // set Submit button text
            string submitButtonText = GetAttributeValue( "SubmitButtonAltText" );
            if ( string.IsNullOrWhiteSpace( submitButtonText ) )
            {
                submitButtonText = "Submit";
            }
            btnSubmit.Text = submitButtonText;

            // set allow message based on if message workflow attribute key provided
            _allowMessage = !String.IsNullOrWhiteSpace( GetAttributeValue( "WorkflowMessageAttributeKey" ) );


            var groupService = new GroupService( _rockContext );
            bool groupIsFromQueryString = true;

            Guid? groupGuid = GetAttributeValue( "Group" ).AsGuidOrNull();
            if ( groupGuid.HasValue )
            {
                _group = groupService.Get( groupGuid.Value );
                groupIsFromQueryString = false;
            }

            if ( _group == null )
            {
                groupGuid = PageParameter( "GroupGuid" ).AsGuidOrNull();
                if ( groupGuid.HasValue )
                {
                    _group = groupService.Get( groupGuid.Value );
                }
            }

            if ( _group == null && GetAttributeValue( "EnablePassingGroupId" ).AsBoolean( false ) )
            {
                int? groupId = PageParameter( "GroupId" ).AsIntegerOrNull();
                if ( groupId.HasValue )
                {
                    _group = groupService.Get( groupId.Value );
                }

            }

            // group not found, throw error
            if ( _group == null )
            {
                nbNotice.Heading = "Unknown Group";
                nbNotice.Text = BuildGroupErrorMessage( "Requested group not found or was not provided." );
                return false;
            }
            else
            {
                var groupTypeGuids = this.GetAttributeValue( "AllowedGroupTypes" ).SplitDelimitedValues().AsGuidList();

                //if group is not an allowed group type display error
                if ( groupIsFromQueryString && groupTypeGuids.Any() && !groupTypeGuids.Contains( _group.GroupType.Guid ) )
                {
                    _group = null;
                    nbNotice.Heading = "Invalid Group";
                    nbNotice.Text = BuildGroupErrorMessage( "The selected group is not a valid selection for this Information Request page." );
                    return false;
                }
            }

            //Set Connection Status for new person record, if not found throw error
            _dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "ConnectionStatus" ).AsGuid() );
            if ( _dvcConnectionStatus == null )
            {
                nbNotice.Heading = "Invalid Connection Status";
                nbNotice.Text = "<p>The selected Connection Status setting does not exist.</p>";
                return false;
            }

            //Set Record Status for new People, if not found display error 
            _dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );
            if ( _dvcRecordStatus == null )
            {
                nbNotice.Heading = "Invalid Record Status";
                nbNotice.Text = "<p>The selected Record Status setting does not exist.</p>";
                return false;
            }

            _homeAddressType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
            _familyType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            _adultRole = _familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );

            //big problems. System default valeus not found.
            if ( _homeAddressType == null || _familyType == null || _adultRole == null )
            {
                nbNotice.Heading = "Missing System Value";
                nbNotice.Text = "<p>There is a missing or invalid system value. Check the settings for Marital Status of 'Married', Location Type of 'Home', Group Type of 'Family', and Family Group Role of 'Adult'.</p>";
                return false;
            }

            return true;
        }


        /// <summary>
        /// Check to see if name provided is the same as the currently logged in person, if First/Nick name and Last Name matches assume match
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is current person]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCurrentPerson()
        {
            if ( CurrentPerson == null )
            {
                return false;
            }

            bool firstNameMatch = CurrentPerson.FirstName.Trim().Equals( tbFirstName.Text.Trim(), StringComparison.OrdinalIgnoreCase )
                    || CurrentPerson.NickName.Trim().Equals( tbFirstName.Text.Trim(), StringComparison.OrdinalIgnoreCase );
            bool lastNameMatch = CurrentPerson.LastName.Trim().Equals( tbLastName.Text.Trim(), StringComparison.OrdinalIgnoreCase );

            return firstNameMatch && lastNameMatch;

        }

        /// <summary>
        /// Sets the phone number.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="pnbNumber">The PNB number.</param>
        /// <param name="cbSms">The cb SMS.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        private void SetPhoneNumber( RockContext rockContext, Person person, PhoneNumberBox pnbNumber, RockCheckBox cbSms, Guid phoneTypeGuid )
        {
            var phoneType = DefinedValueCache.Get( phoneTypeGuid );
            if ( phoneType != null )
            {
                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneType.Id );
                string oldPhoneNumber = string.Empty;
                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneType.Id };
                }
                else
                {
                    oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }

                phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbNumber.CountryCode );
                phoneNumber.Number = PhoneNumber.CleanNumber( pnbNumber.Number );

                if ( string.IsNullOrWhiteSpace( phoneNumber.Number ) )
                {
                    if ( phoneNumber.Id > 0 )
                    {
                        new PhoneNumberService( rockContext ).Delete( phoneNumber );
                        person.PhoneNumbers.Remove( phoneNumber );
                    }
                }
                else
                {
                    if ( phoneNumber.Id <= 0 )
                    {
                        person.PhoneNumbers.Add( phoneNumber );
                    }
                    if ( cbSms != null && cbSms.Checked )
                    {
                        phoneNumber.IsMessagingEnabled = true;
                        person.PhoneNumbers
                            .Where( n => n.NumberTypeValueId != phoneType.Id )
                            .ToList()
                            .ForEach( n => n.IsMessagingEnabled = false );
                    }
                }
            }
        }

        /// <summary>
        /// Loads the request form
        /// </summary>
        private void ShowDetails()
        {
            _rockContext = _rockContext ?? new RockContext();

            //No group provided return
            if ( _group == null )
            {
                return;
            }

            //configure intro lava message
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Group", _group );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            var template = GetAttributeValue( "LavaTemplate" );
            lLavaOverview.Text = template.ResolveMergeFields( mergeFields );

            // if debugging enabled and user can edit teh block show the lava fields
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean( false ) && IsUserAuthorized( Authorization.EDIT ) )
            {
                lLavaOutputDebug.Visible = true;
                lLavaOutputDebug.Text = mergeFields.lavaDebugInfo();
            }

            //only show phone number and address controls if using full mode
            pnlHomePhone.Visible = !IsSimple;
            pnlCellPhone.Visible = !IsSimple;
            acAddress.Visible = !IsSimple;

            //only display message box if entering a message is allowed
            tbMessage.Visible = _allowMessage;

            // if someone is logged in and auto fill is enabled, populate the form with their information
            if ( CurrentPersonId.HasValue && _autoFill )
            {
                var personSvc = new PersonService( _rockContext );
                Person person = personSvc
                    .Queryable( "PhoneNumbers.NumberTypeValue" ).AsNoTracking()
                    .FirstOrDefault( p => p.Id == CurrentPersonId.Value );

                tbFirstName.Text = CurrentPerson.NickName;
                tbLastName.Text = CurrentPerson.LastName;
                tbEmail.Text = CurrentPerson.Email;

                // if full mode, populate phone and address info
                if ( !IsSimple )
                {
                    Guid homePhoneType = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                    var homePhone = person.PhoneNumbers
                        .FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( homePhoneType ) );

                    if ( homePhone != null )
                    {
                        pnHome.Text = homePhone.Number;
                    }

                    Guid cellPhoneType = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
                    var cellPhone = person.PhoneNumbers
                        .FirstOrDefault( n => n.NumberTypeValue.Guid.Equals( cellPhoneType ) );

                    if ( cellPhone != null )
                    {
                        pnCell.Text = cellPhone.Number;
                        cbSMS.Checked = cellPhone.IsMessagingEnabled;
                    }

                    var homeAddress = person.GetHomeLocation();
                    if ( homeAddress != null )
                    {
                        acAddress.SetValues( homeAddress );
                    }
                }
            }
        }
        #endregion


    }
}