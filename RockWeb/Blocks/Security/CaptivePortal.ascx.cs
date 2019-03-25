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
using System.Linq;
using System.Web;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    [DisplayName( "Captive Portal" )]
    [Category( "Security" )]
    [Description( "Controls access to Wi-Fi." )]

    #region Block Settings
    [TextField(
        name: "MAC Address Paramameter",
        description: "The query string parameter used for the MAC Address",
        defaultValue: "client_mac",
        order: 0,
        key: "MacAddressParam" )]
    [TextField(
        name: "Release Link",
        description: "The full URL to redirect users to after registration.",
        order: 1,
        key: "ReleaseLink" )]
    [BooleanField(
        name: "Show Name",
        description: "Show or hide the Name fields. If it is visible then it will be required.",
        defaultValue: true,
        order: 2,
        key: "ShowName",
        IsRequired = true )]
    [BooleanField(
        name: "Show Mobile Phone",
        description: "Show or hide the Mobile Phone Number field. If it is visible then it will be required.",
        defaultValue: true,
        order: 3,
        key: "ShowMobilePhone",
        IsRequired = true )]
    [BooleanField(
        name: "Show Email",
        description: "Show or hide the Email field. If it is visible then it will be required.",
        defaultValue: true,
        order: 4,
        key: "ShowEmail",
        IsRequired = true )]
    [BooleanField(
        name: "Show Acceptance Checkbox",
        description: "Show or hide the \"I Accept\" checkbox. If it is visible then it will be required. This should be visible if the \"Terms And Conditions\" are also visible.",
        defaultValue: false,
        order: 5,
        key: "ShowAccept",
        IsRequired = true )]
    [TextField(
        name: "Acceptance Checkbox Label",
        description: "Text used to signify user agreement with the Terms and Conditions",
        defaultValue: "I Accept",
        order: 6,
        key: "AcceptanceLabel" )]
    [TextField(
        name: "Button Text",
        description: "Text to display on the button",
        defaultValue: "Accept and Connect",
        order: 7,
        key: "ButtonText" )]
    [BooleanField(
        name: "Show Legal Note",
        description: "Show or hide the Terms and Conditions. This should be always be visible unless users are being automatically connected without any agreement needed.",
        defaultValue: true,
        order: 8,
        key: "ShowLegalNote",
        IsRequired = true )]
    [DefinedValueField(
        definedTypeGuid: Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON,
        name: "New Person Record Type",
        description: "The person type to assign to new persons created by Captive Portal.",
        defaultValue: Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON,
        order: 9,
        key: "NewPersonRecordType")]
    [DefinedValueField(
        definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        name: "New Person Record Status",
        description: "The record status to assign to new persons created by Captive Portal.",
        defaultValue: Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE,
        order: 10,
        key: "NewPersonRecordStatus")]
    [DefinedValueField(
        definedTypeGuid: Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        name: "New Person Connection Status",
        description: "The connection status to assign to new persons created by Captive Portal",
        defaultValue: Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR,
        order: 11,
        key: "NewPersonConnectionStatus")]
    [CodeEditorField(
        name: "Legal Note",
        description: "A legal note outlining the Terms and Conditions for using Wi-Fi.",
        mode: CodeEditorMode.Html,
        height: 400,
        required: false,
        defaultValue: DEFAULT_LEGAL_NOTE,
        order: 12,
        key: "LegalNote" )]

    #endregion Block Settings
    public partial class CaptivePortal : RockBlock
    {
        #region Block Setting Strings
        protected const string DEFAULT_LEGAL_NOTE = @"<div>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Helvetica, Arial, sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol""; padding: 0 12px; }
        li { padding-bottom: 8px; }
    </style>
    <h1>Terms & Conditions</h1>
    <p>
        This free Wi-Fi service(""Service"") is provided by {{ 'Global' | Attribute:'OrganizationName' }}
        (""Organization"") to its guests. Please read the Service Terms and Conditions below. To use the Service, users must accept these Service Terms and Conditions.
    </p>

    <ol>
        <li>The Service allows the users to access the Internet via the Wi-Fi network provided by the Organization by using the user's Wi-Fi-enabled device. In order to use the Service, the users must use a Wi-Fi-enabled device and related software.It is the user's responsibility to ensure that the user's device works with the service.</li>
        <li>The Organization may from time to time modify or enhance or suspend the Service.</li>
        <li>The users acknowledges and consents that:
            <ol type=""a"">
                <li>
                    The Service has to be operated properly in accordance with the recommended practice, and with the appropriate hardware and software installed;
                </li>
                <li>
                    The provisioning of the Service may reveal location-specific data, usage and retention of which are subject to the local standard privacy policy and jurisdiction;
                </li>
                <li>
                    The Organization excludes all liability or responsibility for any cost, claim, damage, or loss to the user or to any third party whether direct or indirect of any kind including revenue, loss or profits or any consequential loss in contract,
                        tort, under any statute or otherwise( including negligence ) arising out of or in any way related to the Service( including, but not limited to, any loss to the user arising from a suspension of the Service or Wi-Wi disconnection or degrade of Service quality); and
                </li>
                <li>
                    The Organization will not be liable to the user or any other person for any loss or damage resulting from a delay or failure to perform these Terms and Conditions in whole or in part where such delay or failure is due to causes beyond the Organization's reasonable control, or which is not occasioned by its fault or negligence, including
                    acts or omissions of third parties( including telecommunications network operators, Information Service content providers, and equipment suppliers), shortage of components, war, the threat of imminent war, riots or other acts of civil disobedience,
                    insurrection, acts of God, restraints imposed by governments or any other supranational legal authority, industrial or trade disputes, fires, explosions, storms, floods, lightening, earthquakes and other natural calamities.
                </li>
            </ol>
        </li>
        <li>
            The user's use of the Service is subject to the coverage and connectivity conditions of the Service network and the Organization makes no guarantee regarding the service performance and availability of the service network. The Organization hereby expressly reserves the right to cease the
            provisioning of the Service in the event the same is being substantially affected by reasons beyond the control of the Organization.
        </li>
    </ol>
</div>";
        #endregion

        /// <summary>
        /// The user agents to ignore. UA strings that begin with one of these will be ignored.
        /// This is to fix Apple devices loading the page with its CaptiveNetwork WISPr UA and messing
        /// up the device info, which is parsed from the UA. Ignoring "CaptiveNetworkSupport*"
        /// will fix 100% of current known issues, if more than a few come up we should put this
        /// into the DB as DefinedType/DefinedValues.
        /// </summary>
        private List<string> _userAgentsToIgnore = new List<string>()
        {
            "CaptiveNetworkSupport"
        };

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            nbAlert.Visible = false;

            // Go through the UA ignore list and don't load anything we don't care about or want.
            foreach ( string userAgentToIgnore in _userAgentsToIgnore )
            {
                if ( Request.UserAgent.StartsWith( userAgentToIgnore ) )
                {
                    return;
                }
            }

            if ( !IsPostBack )
            {
                string macAddress = RockPage.PageParameter( GetAttributeValue( "MacAddressParam" ) );
                if ( string.IsNullOrWhiteSpace( macAddress ) || !macAddress.IsValidMacAddress() )
                {
                    nbAlert.Text = "Missing or invalid MAC Address";
                    nbAlert.Visible = true;
                    ShowControls( false );
                    return;
                }

                string releaseLink = GetAttributeValue( "ReleaseLink" );
                if ( string.IsNullOrWhiteSpace( releaseLink ) || !releaseLink.IsValidUrl() )
                {
                    nbAlert.Text = "Missing or invalid Release Link";
                    nbAlert.Visible = true;
                    ShowControls( false );
                    return;
                }

                // Save the supplied MAC address to the page removing any non-Alphanumeric characters
                macAddress = macAddress.RemoveAllNonAlphaNumericCharacters();
                hfMacAddress.Value = macAddress;

                // create or get device
                PersonalDeviceService personalDeviceService = new PersonalDeviceService( new RockContext() );
                PersonalDevice personalDevice = null;

                if ( DoesPersonalDeviceExist( macAddress ) )
                {
                    personalDevice = VerifyDeviceInfo( macAddress );
                }
                else
                {
                    personalDevice = CreateDevice( macAddress );
                }

                // We are going to create this everytime they hit the captive portal page. Otherwise if the device is saved but not linked to an actual user (not the fake one created here),
                // and then deleted by the user/browser/software, then they'll never get the cookie again and won't automatically get linked by RockPage.
                CreateDeviceCookie( macAddress );

                // See if user is logged in and link the alias to the device.
                if ( CurrentPerson != null )
                {
                    Prefill( CurrentPerson );
                    RockPage.LinkPersonAliasToDevice( ( int ) CurrentPersonAliasId, macAddress );
                }

                // Direct connect if no controls are visible
                if ( !ShowControls() )
                {
                    // Nothing to show means nothing to enter. Redirect user back to FP with the primary alias ID and query string
                    if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
                    {
                        nbAlert.Text = string.Format( "If you did not have Administrative permissions on this block you would have been redirected to: <a href='{0}'>{0}</a>.", CreateRedirectUrl( null ) );
                    }
                    else
                    {
                        Response.Redirect( CreateRedirectUrl( null ) );
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Doeses a personal device exist for the provided MAC address
        /// </summary>
        /// <param name="macAddress">The mac address.</param>
        /// <returns></returns>
        private bool DoesPersonalDeviceExist( string macAddress )
        {
            PersonalDeviceService personalDeviceService = new PersonalDeviceService( new RockContext() );
            return personalDeviceService.GetByMACAddress( macAddress ) == null ? false : true;
        }

        /// <summary>
        /// Creates the device if new.
        /// </summary>
        /// <returns>Returns true if the device was created, false it already existed</returns>
        private PersonalDevice CreateDevice( string macAddress )
        {
            UAParser.ClientInfo client = UAParser.Parser.GetDefault().Parse( Request.UserAgent );

            RockContext rockContext = new RockContext();
            PersonalDeviceService personalDeviceService = new PersonalDeviceService( rockContext );

            PersonalDevice personalDevice = new PersonalDevice();
            personalDevice.MACAddress = macAddress;

            personalDevice.PersonalDeviceTypeValueId = GetDeviceTypeValueId();
            personalDevice.PlatformValueId = GetDevicePlatformValueId( client );
            personalDevice.DeviceVersion = GetDeviceOsVersion( client );

            personalDeviceService.Add( personalDevice );
            rockContext.SaveChanges();

            return personalDevice;
        }

        /// <summary>
        /// Gets the current device platform info and updates the obj if needed.
        /// </summary>
        /// <param name="personalDevice">The personal device.</param>
        private PersonalDevice VerifyDeviceInfo( string macAddress )
        {
            UAParser.ClientInfo client = UAParser.Parser.GetDefault().Parse( Request.UserAgent );

            RockContext rockContext = new RockContext();
            PersonalDeviceService personalDeviceService = new PersonalDeviceService( rockContext );

            PersonalDevice personalDevice = personalDeviceService.GetByMACAddress( macAddress );
            personalDevice.PersonalDeviceTypeValueId = GetDeviceTypeValueId();
            personalDevice.PlatformValueId = GetDevicePlatformValueId( client );
            personalDevice.DeviceVersion = GetDeviceOsVersion( client );

            rockContext.SaveChanges();

            return personalDevice;
        }

        /// <summary>
        /// Uses the Request information to determine if the device is mobile or not
        /// </summary>
        /// <returns>DevinedValueId for "Mobile" or "Computer", Mobile includes Tablet. Null if there is a data issue and the DefinedType is missing</returns>
        private int? GetDeviceTypeValueId()
        {
            // Get the device type Mobile or Computer
            DefinedTypeCache definedTypeCache = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE.AsGuid() );
            DefinedValueCache definedValueCache = null;

            var clientType = InteractionDeviceType.GetClientType( Request.UserAgent );
            clientType = clientType == "Mobile" || clientType == "Tablet" ? "Mobile" : "Computer";

            if ( definedTypeCache != null )
            {
                definedValueCache = definedTypeCache.DefinedValues.FirstOrDefault( v => v.Value == clientType );

                if ( definedValueCache == null )
                {
                    definedValueCache = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_COMPUTER.AsGuid() );
                }

                return definedValueCache.Id;
            }

            return null;
        }

        /// <summary>
        /// Parses ClientInfo to find the OS family
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>DefinedValueId for the found OS. Uses "Other" if the OS is not in DefinedValue. Null if there is a data issue and the DefinedType is missing</returns>
        private int? GetDevicePlatformValueId( UAParser.ClientInfo client )
        {
            // get the OS
            string platform = client.OS.Family.Split( ' ' ).First();

            DefinedTypeCache definedTypeCache = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM.AsGuid() );
            DefinedValueCache definedValueCache = null;
            if ( definedTypeCache != null )
            {
                definedValueCache = definedTypeCache.DefinedValues.FirstOrDefault( v => v.Value == platform );

                if ( definedValueCache == null )
                {
                    definedValueCache = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_OTHER.AsGuid() );
                }

                return definedValueCache.Id;
            }

            return null;
        }

        /// <summary>
        /// Parses ClientInfo and gets the device os version. If it cannot be determined returns the OS family string without the platform
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns></returns>
        private string GetDeviceOsVersion( UAParser.ClientInfo client )
        {
            if ( client.OS.Major == null )
            {
                string platform = client.OS.Family.Split( ' ' ).First();
                return client.OS.Family.Replace( platform, string.Empty ).Trim();
            }

            return string.Format(
                "{0}.{1}.{2}.{3}",
                client.OS.Major ?? "0",
                client.OS.Minor ?? "0",
                client.OS.Patch ?? "0",
                client.OS.PatchMinor ?? "0" );
        }

        /// <summary>
        /// Creates the device cookie if it does not exist.
        /// </summary>
        private void CreateDeviceCookie(string macAddress )
        {
            if ( Request.Cookies["rock_wifi"] == null )
            {
                HttpCookie httpcookie = new HttpCookie( "rock_wifi" );
                httpcookie.Expires = DateTime.MaxValue;
                httpcookie.Values.Add( "ROCK_PERSONALDEVICE_ADDRESS", macAddress );
                Response.Cookies.Add( httpcookie );
            }
        }

        /// <summary>
        /// Prefills the visible fields with info from the specified rock person
        /// if there is a logged in user than the name fields will be disabled.
        /// </summary>
        /// <param name="rockUserId">The rock user identifier.</param>
        protected void Prefill( Person person )
        {
            if (person == null)
            {
                return;
            }

            if ( tbFirstName.Visible == true )
            {
                tbFirstName.Text = person.FirstName;
                tbFirstName.Enabled = CurrentPerson == null;

                tbLastName.Text = person.LastName;
                tbLastName.Enabled = CurrentPerson == null;
            }

            if ( tbMobilePhone.Visible )
            {
                PhoneNumberService phoneNumberService = new PhoneNumberService( new RockContext() );
                PhoneNumber phoneNumber = phoneNumberService.GetNumberByPersonIdAndType( person.Id, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                tbMobilePhone.Text = phoneNumber == null? string.Empty : phoneNumber.Number;
            }

            if (tbEmail.Visible == true )
            {
                tbEmail.Text = person.Email;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConnect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConnect_Click( object sender, EventArgs e )
        {
            if ( cbAcceptTAC.Visible && cbAcceptTAC.Checked == false )
            {
                nbAlert.Text = string.Format( "You must check \"{0}\" to continue.", GetAttributeValue( "AcceptanceLabel" ) );
                nbAlert.Visible = true;
                return;
            }

            // We know there is a device with the stored MAC
            // If we have an alias ID then we have all data needed and can redirect the user to frontporch
            // also if hfPersonAliasId has a value at this time it has already been linked to the device.
            if ( CurrentPerson != null )
            {
                UpdatePersonInfo();
                Response.Redirect( CreateRedirectUrl( CurrentPersonAliasId ) );
                return;
            }

            int? primaryAliasId = LinkDeviceToPerson();
            if ( primaryAliasId != null )
            {
                Response.Redirect( CreateRedirectUrl( primaryAliasId ) );
                return;
            }

            // Send them back to Front Porch without user info
            Response.Redirect( CreateRedirectUrl( null ) );
        }

        /// <summary>
        /// Try to link the device to a person and return the primary alias ID if successful
        /// </summary>
        /// <returns>true if device successfully linked to a person</returns>
        protected int? LinkDeviceToPerson()
        {
            // At this point the user is not logged in and not found by looking up the device
            // So lets try to find the user using entered info and then link them to the device.
            PersonService personService = new PersonService( new RockContext() );
            int mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
            Person person = null;
            string mobilePhoneNumber = string.Empty;

            // Looking for a 100% match
            if ( tbFirstName.Visible && tbLastName.Visible && tbEmail.Visible && tbMobilePhone.Visible )
            {
                mobilePhoneNumber = tbMobilePhone.Text.RemoveAllNonAlphaNumericCharacters();

                var personQuery = new PersonService.PersonMatchQuery( tbFirstName.Text, tbLastName.Text, tbEmail.Text, mobilePhoneNumber );
                person = personService.FindPerson( personQuery, true );

                if ( person.IsNotNull() )
                {
                    RockPage.LinkPersonAliasToDevice( person.PrimaryAlias.Id, hfMacAddress.Value );
                    return person.PrimaryAliasId;
                }
                else
                {
                    // If no known person record then create one since we have the minimum info required
                    person = CreateAndSaveNewPerson();

                    // Link new device to person alias
                    RockPage.LinkPersonAliasToDevice( person.PrimaryAlias.Id, hfMacAddress.Value );
                    return person.PrimaryAlias.Id;
                }
            }

            // Look for minimum info
            if ( tbFirstName.Visible && tbLastName.Visible && ( tbMobilePhone.Visible || tbEmail.Visible ) )
            {
                // If no known person record then create one since we have the minimum info required
                person = CreateAndSaveNewPerson();
                
                // Link new device to person alias
                RockPage.LinkPersonAliasToDevice( person.PrimaryAlias.Id, hfMacAddress.Value );
                return person.PrimaryAlias.Id;
            }

            // Just match off phone number if no other fields are showing.
            if ( tbMobilePhone.Visible && !tbFirstName.Visible && !tbLastName.Visible && !tbEmail.Visible )
            {
                mobilePhoneNumber = tbMobilePhone.Text.RemoveAllNonAlphaNumericCharacters();
                person = personService.Queryable().Where( p => p.PhoneNumbers.Where( n => n.NumberTypeValueId == mobilePhoneTypeId ).FirstOrDefault().Number == mobilePhoneNumber ).FirstOrDefault();
                if ( person != null )
                {
                    RockPage.LinkPersonAliasToDevice( person.PrimaryAlias.Id, hfMacAddress.Value );
                    return person.PrimaryAliasId;
                }
            }

            // Unable to find an existing user and we don't have the minimium info to create one.
            // We'll let Rock.Page and the cookie created in OnLoad() link the device to a user when they are logged in.
            // This will not work if this page is loaded into a Captive Network Assistant page as the cookie will not persist.
            return null;
        }

        protected Person CreateAndSaveNewPerson()
        {
            int mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

            var recordTypeValue = DefinedValueCache.Get( GetAttributeValue( "NewPersonRecordType" ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );
            var recordStatusValue = DefinedValueCache.Get( GetAttributeValue( "NewPersonRecordStatus" ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var connectionStatusValue = DefinedValueCache.Get( GetAttributeValue( "NewPersonConnectionStatus" ).AsGuid() ) ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );
            
            var person = new Person
            {
                FirstName = tbFirstName.Text,
                LastName = tbLastName.Text,
                Email = tbEmail.Text,
                RecordTypeValueId = recordTypeValue != null ? recordTypeValue.Id : ( int? ) null,
                RecordStatusValueId = recordStatusValue != null ? recordStatusValue.Id : ( int? ) null,
                ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : ( int? ) null
            };

            if ( tbMobilePhone.Text.RemoveAllNonAlphaNumericCharacters().IsNotNullOrWhiteSpace() )
            {
                person.PhoneNumbers = new List<PhoneNumber>() { new PhoneNumber { IsSystem = false, Number = tbMobilePhone.Text.RemoveAllNonAlphaNumericCharacters(), NumberTypeValueId = mobilePhoneTypeId } };
            }

            PersonService.SaveNewPerson( person, new RockContext() );
            return person;
        }

        /// <summary>
        /// Redirects the specified person to the release link URL
        /// </summary>
        /// <param name="primaryAliasId">if null then the id parameter is not created for the returned URL</param>
        /// <returns></returns>
        protected string CreateRedirectUrl( int? primaryAliasId )
        {
            string s = string.Empty;

            if ( primaryAliasId != null )
            {
                s = string.Format( "{0}?id={1}&{2}", GetAttributeValue( "ReleaseLink" ), primaryAliasId, Request.QueryString );
                return s;
            }

            s = string.Format( "{0}?{1}", GetAttributeValue( "ReleaseLink" ), Request.QueryString );
            return s;
        }

        /// <summary>
        /// Shows the controls according to the attribute values. If they are visible then they are also required.
        /// </summary>
        /// <returns>If any control is visible then true, else false.</returns>
        protected bool ShowControls( bool isEnabled = true )
        {
            tbFirstName.Visible = GetAttributeValue( "ShowName" ).AsBoolean();
            tbFirstName.Required = GetAttributeValue( "ShowName" ).AsBoolean();
            tbFirstName.Enabled = isEnabled;

            tbLastName.Visible = GetAttributeValue( "ShowName" ).AsBoolean();
            tbLastName.Required = GetAttributeValue( "ShowName" ).AsBoolean();
            tbLastName.Enabled = isEnabled;

            tbMobilePhone.Visible = GetAttributeValue( "ShowMobilePhone" ).AsBoolean();
            tbMobilePhone.Required = GetAttributeValue( "ShowMobilePhone" ).AsBoolean();
            tbMobilePhone.Enabled = isEnabled;

            tbEmail.Visible = GetAttributeValue( "ShowEmail" ).AsBoolean();
            tbEmail.Required = GetAttributeValue( "ShowEmail" ).AsBoolean();
            tbEmail.Enabled = isEnabled;

            cbAcceptTAC.Visible = GetAttributeValue( "ShowAccept" ).AsBoolean();
            cbAcceptTAC.Text = GetAttributeValue( "AcceptanceLabel" );
            cbAcceptTAC.Enabled = isEnabled;

            btnConnect.Text = isEnabled ? GetAttributeValue( "ButtonText" ) : "Unable to connect to Wi-Fi due to errors";
            btnConnect.Enabled = isEnabled;

            if ( litLegalNotice.Visible = GetAttributeValue( "ShowLegalNote" ).AsBoolean() )
            {
                litLegalNotice.Text = GetAttributeValue( "LegalNote" ).ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage ) );
            }

            if ( tbFirstName.Visible || tbLastName.Visible || tbMobilePhone.Visible || tbEmail.Visible || cbAcceptTAC.Visible || litLegalNotice.Visible )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the CurrentPerson email and mobile phone number with the entered values if they are different.
        /// Does nothing if there is no CurrentPerson
        /// </summary>
        private void UpdatePersonInfo()
        {
            if ( CurrentPerson == null )
            {
                return;
            }

            using ( RockContext rockContext = new RockContext() )
            {
                Person person = new PersonService( rockContext ).Get( ( int ) CurrentPersonId );
                person.Email = tbEmail.Text;

                int mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
                if ( !person.PhoneNumbers.Where( n => n.NumberTypeValueId == mobilePhoneTypeId ).Any() &&
                    tbMobilePhone.Text.RemoveAllNonAlphaNumericCharacters().IsNotNullOrWhiteSpace() )
                {
                    person.PhoneNumbers.Add( new PhoneNumber { IsSystem = false, Number = tbMobilePhone.Text.RemoveAllNonAlphaNumericCharacters(), NumberTypeValueId = mobilePhoneTypeId } );
                }
                else
                {
                    PhoneNumber phoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == mobilePhoneTypeId ).FirstOrDefault();
                    if ( phoneNumber != null )
                    {
                        phoneNumber.Number = tbMobilePhone.Text.RemoveAllNonAlphaNumericCharacters();
                    }
                }

                rockContext.SaveChanges();
            }
        }
    }
}