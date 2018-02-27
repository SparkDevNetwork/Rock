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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "WiFi Welcome" )]
    [Category( "Core" )]
    [Description( "Controls access to WiFi." )]
    [TextField( "MAC Address Param", "The query string parameter used for the MAC Address", true, "client_mac", "", 0, "MacAddressParam" )]
    [TextField( "Release Link", "URL to direct users to", true, "", "", 1, "ReleaseLink" )]
    [BooleanField("Show First Name", "Show or hide the First Name field. If it is visible then it will be required.", true, "", 2, "ShowFirstName", IsRequired = true )]
    [BooleanField( "Show Last Name", "Show or hide the Last Name field. If it is visible then it will be required.", true, "", 3, "ShowLastName", IsRequired = true )]
    [BooleanField( "Show Mobile Phone", "Show or hide the Mobile Phone Number field. If it is visible then it will be required.", true, "", 4, "ShowMobilePhone", IsRequired = true )]
    [BooleanField( "Show Email", "Show or hide the Email field. If it is visible then it will be required.", true, "", 5, "ShowEmail", IsRequired = true )]
    [BooleanField( "Show Acceptance Checkbox", "Show or hide the \"I Accept\" checkbox. If it is visible then it will be required. This should be visible if the \"Terms And Conditions\" are also visible.", true, "", 6, "ShowAccept", IsRequired = true )]
    [TextField( "Acceptance Checkbox Label", "Text used to signify user agreement with the Terms and Conditions", true, "I Accept", "", 7, "AcceptanceLabel" )]
    [TextField( "Button Text", "Text to display on the button", true, "Connect To WiFi", "", 8, "ButtonText" )]
    [BooleanField( "Show Legal Note", "Show or hide the Terms and Conditions. This should be always be visible unless users are being automatically connected without any agreement needed.", true, "", 9, "ShowLegalNote", IsRequired = true )]
    [CodeEditorField ( "Legal Note", "A legal note outlining the Terms and Conditions for using WiFi", CodeEditorMode.Html, CodeEditorTheme.Rock, 400, false, defaultLegalNote, "", 10, "LegalNote" )]
    public partial class CaptivePortal : RockBlock
    {

        #region DefaultLegalNote
        protected const string defaultLegalNote = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title></title>
</head>
<body>
    <h1>Terms & Conditions</h1>
    <p>
        This free WiFi service(""Service"") is provided by {{ 'Global' | Attribute:' OrganizationName' }}
        (""Organization"") to its guests.Please read the Service Terms and Conditions below. To use the Service, users must accept these Service Terms and Conditions.
    </p>

    <p>
        1. The Service allows the users to access the Internet via the Wi-Fi network provided by the Organization by using the user's Wi-Fi-enabled device. In order to use the Service, the
        users must use a Wi-Fi-enabled device and related software.It is the user's responsibility to ensure that the user's device works with the service
   </p>

    <p>
        2. The Organization may from time to time modify or enhance or suspend the Service
    </p>

    <p>
        3. The users acknowledges and consents that:
    </p>

    <p>
        &nbsp;&nbsp;(a) The Service has to be operated properly in accordance with the recommended practice, and with the appropriate hardware and software installed;
    </p>

    <p>
        &nbsp;&nbsp;(b) The provisioning of the Service may reveal location-specific data, usage and retention of which are subject to the local standard privacy policy and jurisdiction;
    </p>

    <p>
        &nbsp;&nbsp;(c) Every user is entitled to 20 continuous minutes free WiFi service every day at the Company's designated locations(s). If the connection is disconnected within the 20 minutes due to any reason, the users cannot use the Service again on the same day;
    </p>

    <p>
        &nbsp;&nbsp;(d) The Organization excludes all liability or responsibility for any cost, claim, damage, or loss to the user or to any third party whether direct or indirect of any kind including revenue, loss or profits or any consequential loss in contract,
        tort, under any statute or otherwise( including negligence ) arising out of or in any way related to the Service( including, but not limited to, any loss to the user arising from a suspension of the Service or Wi-Wi disconnection or degrade of Service quality); and
     </p>

    <p>
        &nbsp;&nbsp;(e) The Organization will not be liable to the user or any other person for any loss or damage resulting from a delay or failure to perform these Terms and Conditions in whole or in part where such delay or failure is due to causes beyond the Organization's reasonable control, or which is not occasioned by its fault or negligence, including
        acts or omissions of third parties( including telecommunications network operators, Information Service content providers, and equipment suppliers), shortage of components, war, the threat of imminent war, riots or other acts of civil disobedience,
        insurrection, acts of God, restraints imposed by governments or any other supranational legal authority, industrial or trade disputes, fires, explosions, storms, floods, lightening, earthquakes and other natural calamities.
    </p>

    <p>
        4. The user's use of the Service is subject to the coverage and connectivity conditions of the Service network and the Organization makes no guarantee regarding the service performance and availability of the service network. The Organization hereby expressly reserves the right to cease the
        provisioning of the Service in the event the same is being substantially affected by reasons beyond the control of the Organization.
    </p>

</body>
</html>";
        #endregion

        protected string macAddress;

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            macAddress = Request.Params[GetAttributeValue( "MacAddressParam" )];
            bool newDevice = false;

            if ( !IsPostBack )
            {
                newDevice = CreateDeviceIfNew();

                if(newDevice)
                {
                    CreateDeviceCookie();
                }
                
                // 2 direct connect path
                if ( !ShowControls() )
                {
                    string redirectUrl = string.Format( "{0}?{1}={2}", GetAttributeValue( "ReleaseLink" ), GetAttributeValue( "MacAddressParam" ), macAddress );
                    
                    // Nothing to show then just redirect back to FP with the mac
                    if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
                    {
                        nbAlert.Text = string.Format( "If you did not have Administrate permissions on this block, you would have been redirected to here: <a href='{0}'>{0}</a>.", redirectUrl );
                    }
                    else
                    {
                        Response.Redirect( redirectUrl );
                    }
                }

                bool isNewDevice = CreateDeviceIfNew();
                int? rockUserId = null;

                if ( Session["RockUserId"] != null )
                {
                    // 3 user logged in path
                    rockUserId = Session["RockUserId"].ToString().AsIntegerOrNull();
                    Prefill( rockUserId );
                    // Link person to device
                }
                else
                {
                    //4 user not logged in path
                    // Create a new person
                    // link new person to device
                }
            }
        }
        
        /// <summary>
        /// Creates the device if new.
        /// </summary>
        /// <returns>Returns true if the device was created, false it already existed</returns>
        private bool CreateDeviceIfNew()
        {
            // If this cookie exists then we can assume that the creation process has already taken place and we can save the hit to the DB.
            if ( Request.Cookies["rock_wifi"] != null )
            {
                return false;
            }

            // Check to see if the device exists
            RockContext rockContext = new RockContext();
            PersonalDeviceService personalDeviceService = new PersonalDeviceService( rockContext );
            PersonalDevice personalDevice = personalDeviceService.GetByMACAddress( macAddress );

            if (personalDevice == null)
            {
                personalDevice = new PersonalDevice();
                personalDevice.MACAddress = macAddress;

                // Parse the UA string and try to get the info we want
                UAParser.ClientInfo client = UAParser.Parser.GetDefault().Parse( Request.UserAgent );

                // Get the device type Mobile or Computer
                DefinedTypeCache definedTypeCache = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE.AsGuid() );
                DefinedValueCache definedValueCache = null;

                string clientType = Request.Browser.IsMobileDevice == true ? "Mobile" : "Computer";

                if ( definedTypeCache != null )
                {
                    definedValueCache = definedTypeCache.DefinedValues.FirstOrDefault( v => v.Value == clientType );

                    if ( definedValueCache == null )
                    {
                        definedValueCache = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_TYPE_COMPUTER.AsGuid() );
                    }
                }

                personalDevice.PersonalDeviceTypeValueId = definedValueCache != null ? definedValueCache.Id : ( int? ) null;

                // get the OS
                string platform = client.OS.Family;

                definedTypeCache = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM.AsGuid() );

                if (definedTypeCache != null )
                {
                    definedValueCache = definedTypeCache.DefinedValues.FirstOrDefault( v => v.Value == platform );

                    if (definedValueCache == null )
                    {
                        definedValueCache = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_OTHER.AsGuid() );
                    }
                }

                personalDevice.PlatformValueId = definedValueCache != null ? definedValueCache.Id : ( int? ) null;

                // Get the OS version
                personalDevice.DeviceVersion = string.Format(
                    "{0}.{1}.{2}.{3}",
                    client.OS.Major ?? "0",
                    client.OS.Minor ?? "0",
                    client.OS.Patch ?? "0",
                    client.OS.PatchMinor ?? "0" );

                // Add and save it
                personalDeviceService.Add( personalDevice );
                rockContext.SaveChanges();
                return true;
            }

            return false;
        }

        private void LinkDeviceToPerson(int? rockUserId)
        {
            RockContext rockContext = new RockContext();

            Person person = new PersonService( rockContext ).GetByUserLoginId( rockUserId.Value );

            if ( person == null )
            {
                return;
            }

            
        }

        /// <summary>
        /// Creates the device cookie if it does not exist.
        /// </summary>
        private void CreateDeviceCookie()
        {
            if ( Request.Cookies["rock_wifi"] != null )
            {
                HttpCookie httpcookie = new HttpCookie( "rock_wifi" );
                httpcookie.Expires = DateTime.MaxValue;
                httpcookie.Values.Add( "ROCK_PERSONALDEVICE_ADDRESS", macAddress );
                Response.Cookies.Add( httpcookie );
            }
        }

        /// <summary>
        /// Prefills the visible fields with info from the specified rock user
        /// </summary>
        /// <param name="rockUserId">The rock user identifier.</param>
        protected void Prefill(int? rockUserId)
        {
            Person person = new PersonService( new RockContext() ).GetByUserLoginId( rockUserId.Value );

            if (person == null)
            {
                return;
            }

            if ( tbFirstName.Visible == true )
            {
                tbFirstName.Text = person.FirstName;
            }

            if (tbLastName.Visible)
            {
                tbLastName.Text = person.LastName;
            }

            if ( tbMobilePhone.Visible )
            {
                tbMobilePhone.Text = person.PhoneNumbers.Where( p => p.NumberTypeValueId == 13 ).Select( p => p.Number ).FirstOrDefault();
            }

            if (tbEmail.Visible == true )
            {
                tbEmail.Text = person.Email;
            }
        }

        private void ExpireCookie()
        {
            if ( Request.Cookies["rock_wifi"] != null )
            {
                Response.Cookies["rock_wifi"].Expires = DateTime.Now.AddDays( -1 );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConnect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConnect_Click( object sender, EventArgs e )
        {
            // Send them back to Front Porch
            Response.Redirect( string.Format( "{0}?{1}={2}", GetAttributeValue( "ReleaseLink" ), GetAttributeValue( "MacAddressParam" ), macAddress ) );

        }

        /// <summary>
        /// Shows the controls according to the attribute values. If they are visible then they are also required.
        /// </summary>
        /// <returns>If any control is visible then true, else false.</returns>
        protected bool ShowControls()
        {
            tbFirstName.Visible = GetAttributeValue( "ShowFirstName" ).AsBoolean();
            tbFirstName.Required = GetAttributeValue( "ShowFirstName" ).AsBoolean();

            tbLastName.Visible = GetAttributeValue( "ShowLastName" ).AsBoolean();
            tbLastName.Required = GetAttributeValue( "ShowLastName" ).AsBoolean();

            tbMobilePhone.Visible = GetAttributeValue( "ShowMobilePhone" ).AsBoolean();
            tbMobilePhone.Required = GetAttributeValue( "ShowMobilePhone" ).AsBoolean();

            tbEmail.Visible = GetAttributeValue( "ShowEmail" ).AsBoolean();
            tbEmail.Required = GetAttributeValue( "ShowEmail" ).AsBoolean();
            
            cbAcceptTAC.Visible = GetAttributeValue( "ShowAccept" ).AsBoolean();
            cbAcceptTAC.Required = GetAttributeValue( "ShowAccept" ).AsBoolean();
            cbAcceptTAC.Label = GetAttributeValue( "AcceptanceLabel" );

            btnConnect.Text = GetAttributeValue( "ButtonText" );

            if ( iframeLegalNotice.Visible = GetAttributeValue( "ShowLegalNote" ).AsBoolean() )
            {
                iframeLegalNotice.Attributes["srcdoc"] = GetAttributeValue( "LegalNote" ).ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage ) );
                iframeLegalNotice.Src = "javascript: window.frameElement.getAttribute('srcdoc');";
            }

            if ( tbFirstName.Visible || tbLastName.Visible || tbMobilePhone.Visible || tbEmail.Visible || cbAcceptTAC.Visible || iframeLegalNotice.Visible )
            {
                return true;
            }
                return false;
        }
    }


}