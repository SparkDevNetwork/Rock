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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class FrontPorchTweaks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Remove unneeded block types, these types are not assigned to a block
            RockMigrationHelper.DeleteBlockType( "A5C1BFC0-D4CD-4A2A-908D-329105CBE36A" ); // Examples > Captive Portal Success
            RockMigrationHelper.DeleteBlockType( "86651942-1F75-4DD2-AA98-F0B0C80315C6" ); // Examples > Captive Portal Error

            // Update the CaptivePortal blocktype attributes
            CorrectBlockTypeAttributes();

            // Correct "Wi-Fi" strings for captive portal pages and display options for success and error pages
            CorrectCaptivePortPages();

            // Add Existing Page to Personal Device Badge
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPersonBadgeAttribute(
                 "307CB56D-140C-4CC9-8B54-DD551CC40174",
                 "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108",
                 "Personal Devices Detail",
                 "PersonalDevicesDetail",
                 "Page to show the details of the personal devices added.",
                 1,
                 "",
                 "8DEE4220-DB81-4EF8-88C2-7303493FABF7" );

            RockMigrationHelper.AddPersonBadgeAttributeValue(
                "307CB56D-140C-4CC9-8B54-DD551CC40174",
                "8DEE4220-DB81-4EF8-88C2-7303493FABF7",
                "b2786294-99dc-477e-871d-2e28fce00a98" );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Removes "ShowFirstName" and "ShowLastName" attributes and adds "ShowName". Updates the order number for the other attributes
        /// </summary>
        public void CorrectBlockTypeAttributes()
        {
            // Remove Attrib for BlockType: WiFi Welcome:Show First Name
            RockMigrationHelper.DeleteAttribute( "2CF20676-15A1-4827-9A2B-3144498336E8" );
            // Attrib for BlockType: WiFi Welcome:Show Last Name
            RockMigrationHelper.DeleteAttribute( "45DF60FC-D764-49D5-B79F-3C7AC892C4A6" );
            // Update Order on Attrib for BlockType: WiFi Welcome
            // Attrib for BlockType: WiFi Welcome:MAC Address Param
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MAC Address Param", "MacAddressParam", "", @"The query string parameter used for the MAC Address", 0, @"client_mac", "8483A458-0D5A-4C9F-B2A0-713046A06192" );
            // Attrib for BlockType: WiFi Welcome:Release Link
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Release Link", "ReleaseLink", "", @"The URL to redirect users to after registration.", 1, @"", "31F3E224-38A2-4D7C-A5C9-B34FCAFC98CD" );
            // Attrib for BlockType: WiFi Welcome:Show Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Name", "ShowName", "", @"Show or hide the Name fields. If it is visible then it will be required.", 2, @"True", "365681C7-1110-4998-87F7-783DD0A7C429" );
            // Attrib for BlockType: WiFi Welcome:Show Mobile Phone
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Mobile Phone", "ShowMobilePhone", "", @"Show or hide the Mobile Phone Number field. If it is visible then it will be required.", 3, @"True", "B907D897-B478-4225-9D1F-BA559297F168" );
            // Attrib for BlockType: WiFi Welcome:Show Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email", "ShowEmail", "", @"Show or hide the Email field. If it is visible then it will be required.", 4, @"True", "F7257786-5186-435F-8B69-F41A9500169D" );
            // Attrib for BlockType: WiFi Welcome:Show Acceptance Checkbox
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Acceptance Checkbox", "ShowAccept", "", @"Show or hide the ""I Accept"" checkbox. If it is visible then it will be required. This should be visible if the ""Terms And Conditions"" are also visible.", 5, @"False", "792D8022-9565-4067-9EFD-1A97B8659725" );
            // Attrib for BlockType: WiFi Welcome:Acceptance Checkbox Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Acceptance Checkbox Label", "AcceptanceLabel", "", @"Text used to signify user agreement with the Terms and Conditions", 6, @"I Accept", "E38A008E-94B5-4B81-A6A0-FBCDA44CDF20" );
            // Attrib for BlockType: WiFi Welcome:Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Button Text", "ButtonText", "", @"Text to display on the button", 7, @"Accept and Connect", "13DA26CB-8155-4B2F-B476-6622FEE46D23" );
            // Attrib for BlockType: WiFi Welcome:Show Legal Note
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legal Note", "ShowLegalNote", "", @"Show or hide the Terms and Conditions. This should be always be visible unless users are being automatically connected without any agreement needed.", 8, @"True", "9D8B8CC4-C19E-4EF3-98B4-4426A21B7915" );
            // Attrib for BlockType: WiFi Welcome:Legal Note
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Legal Note", "LegalNote", "", @"A legal note outlining the Terms and Conditions for using WiFi", 9, @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title></title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Helvetica, Arial, sans-serif, ""Apple Color Emoji"", ""Segoe UI Emoji"", ""Segoe UI Symbol"";
            padding: 0 12px;
        }
        li {
            padding-bottom: 8px;
        }
    </style>
</head>
<body>
    <h1>Terms & Conditions</h1>
    <p>
        This free WiFi service(""Service"") is provided by {{ 'Global' | Attribute:'OrganizationName' }}
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
                    Every user is entitled to 20 continuous minutes free WiFi service every day at the Company's designated locations(s). If the connection is disconnected within the 20 minutes due to any reason, the users cannot use the Service again on the same day;
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
</body>
</html>", "581389F2-90D1-4E0D-B8DD-2195D73F9A59" );
        }

        /// <summary>
        /// Updates verbage to Wi-Fi on the captiveportal pages and also removes display options for the success and error pages
        /// </summary>
        public void CorrectCaptivePortPages()
        {
            // Correct Wi-Fi spelling on CaptivePortal blocktype
            Sql( @"UPDATE [BlockType]
                SET [Name] = 'Wi-Fi Welcome', [Description] = 'Controls access to Wi-Fi.'
                WHERE [Guid] = 'CCFCD227-C8F9-4952-8AC5-E427D519EE47'" );

            // Correct verbage on wifisuccess
            RockMigrationHelper.UpdateHtmlContentBlock(
                "63A5398C-7974-4FED-8709-1CCA567C5D09",
                @"<h1>Welcome to {{ 'Global' | Attribute:'OrganizationName'}}'s Wi-Fi Network</h1>
You are now connected to the Wi-Fi network and can continue on to your destination.",
                "01FE2D3C-A1F3-4887-AAA6-1A9EE1B1527A" );

            // Correct verbage on wifierror
            RockMigrationHelper.UpdateHtmlContentBlock(
                "562519A7-967B-45CA-AABA-2C71D28F2303",
                @"<h1>An Error Has Occurred</h1>
An error has occurred connecting you to the Wi-Fi netwok. Please try again in a few minutes.",
                "41D67A32-7D7B-4ACD-91AB-89B7BC425634" );

            // Remove page display options for wifisuccess and wifierror
            Sql( @"UPDATE [Page]
                SET [PageDisplayBreadCrumb] = 0, [PageDisplayDescription] = 0, [PageDisplayIcon] = 0, [PageDisplayTitle] = 0
                WHERE [Guid] IN ( '80613598-D1F6-4819-BCB0-7204E59D98AC', 'A50EBCA2-11B3-4DD2-A2DD-E7939EDDF23F' )" );
        }
    }
}
