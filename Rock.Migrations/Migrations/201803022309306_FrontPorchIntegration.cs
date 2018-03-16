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
    public partial class FrontPorchIntegration : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Interaction", "InteractionEndDateTime", c => c.DateTime( nullable: false ) );

            Sql( @"INSERT INTO [DefinedValue]([IsSystem], [DefinedTypeId], [Order], [Value], [Description], [Guid])
                VALUES(
	                1
	                , (SELECT [ID] FROM [DefinedType] WHERE [Guid] = 'C1848F4C-D6F8-4514-8DB6-CD3C19621025')
	                , 1, 'Computer'
	                , 'Personal Device Type Computer'
	                , '828ADECE-EFE7-49DF-BA8C-B3F132509A95')" );

            RockMigrationHelper.AddPage( true, "", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "Captive Portal", "Landing Page for WiFi connections", "C9767AC5-11A8-4B48-B487-911BA9CADF8C", "" ); // Site:External Website
            RockMigrationHelper.AddPage( true, "C9767AC5-11A8-4B48-B487-911BA9CADF8C", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "CaptivePortalSuccess", "", "80613598-D1F6-4819-BCB0-7204E59D98AC", "" ); // Site:External Website
            RockMigrationHelper.AddPage( true, "C9767AC5-11A8-4B48-B487-911BA9CADF8C", "093ACC5F-F7B6-4EB1-B9B7-9F3F5FB85F13", "CaptivePortalError", "", "A50EBCA2-11B3-4DD2-A2DD-E7939EDDF23F", "" ); // Site:External Website

            RockMigrationHelper.AddPageRoute( "C9767AC5-11A8-4B48-B487-911BA9CADF8C", "wificonnect", "187170EF-C231-41C4-95CA-930CB67867F3" );// for Page:Captive Portal
            RockMigrationHelper.AddPageRoute( "80613598-D1F6-4819-BCB0-7204E59D98AC", "wifisuccess", "40308462-3884-42C2-B803-1B017742865F" );// for Page:CaptivePortalSuccess
            RockMigrationHelper.AddPageRoute( "A50EBCA2-11B3-4DD2-A2DD-E7939EDDF23F", "wifierror", "62E54041-2F35-4264-89A9-70FAC7791579" );// for Page:CaptivePortalError

            RockMigrationHelper.UpdateBlockType( "Examples > Captive Portal Error", "", "~/Blocks/Examples/CaptivePortalError.ascx", "", "86651942-1F75-4DD2-AA98-F0B0C80315C6" );
            RockMigrationHelper.UpdateBlockType( "Examples > Captive Portal Success", "", "~/Blocks/Examples/CaptivePortalSuccess.ascx", "", "A5C1BFC0-D4CD-4A2A-908D-329105CBE36A" );
            RockMigrationHelper.UpdateBlockType( "WiFi Welcome", "Controls access to WiFi.", "~/Blocks/Security/CaptivePortal.ascx", "Security", "CCFCD227-C8F9-4952-8AC5-E427D519EE47" );

            // Add Block to Page: Captive Portal, Site: External Website
            RockMigrationHelper.AddBlock( true, "C9767AC5-11A8-4B48-B487-911BA9CADF8C", "", "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "WiFi Welcome", "Feature", @"", @"", 0, "0630F12E-8645-462C-B599-4306BFC35B19" );

            // Add Block to Page: CaptivePortalSuccess, Site: External Website
            RockMigrationHelper.AddBlock( true, "80613598-D1F6-4819-BCB0-7204E59D98AC", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Wifi Success", "Feature", @"", @"", 0, "63A5398C-7974-4FED-8709-1CCA567C5D09" );

            // Add Block to Page: CaptivePortalError, Site: External Website
            RockMigrationHelper.AddBlock( true, "A50EBCA2-11B3-4DD2-A2DD-E7939EDDF23F", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "WiFi Connection Error", "Feature", @"", @"", 0, "562519A7-967B-45CA-AABA-2C71D28F2303" );

            // Add/Update HtmlContent for Block: Wifi Success
            RockMigrationHelper.UpdateHtmlContentBlock( "63A5398C-7974-4FED-8709-1CCA567C5D09", @"<h1>Welcome to {{ 'Global' | Attribute:'OrganizationName'}}'s WiFi Network</h1>
You are now connected to the WiFi netwokr and can continue on to your destination.", "01FE2D3C-A1F3-4887-AAA6-1A9EE1B1527A" );

            // Add/Update HtmlContent for Block: WiFi Connection Error
            RockMigrationHelper.UpdateHtmlContentBlock( "562519A7-967B-45CA-AABA-2C71D28F2303", @"<h1>An Error Has Occurred</h1>
An error has occurred connecting you to theWiFi netwok. Please try again in a few minutes.", "41D67A32-7D7B-4ACD-91AB-89B7BC425634" );

            // Attrib for BlockType: WiFi Welcome:Show Last Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Last Name", "ShowLastName", "", @"Show or hide the Last Name field. If it is visible then it will be required.", 3, @"True", "45DF60FC-D764-49D5-B79F-3C7AC892C4A6" );

            // Attrib for BlockType: WiFi Welcome:Show Email
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email", "ShowEmail", "", @"Show or hide the Email field. If it is visible then it will be required.", 5, @"True", "F7257786-5186-435F-8B69-F41A9500169D" );

            // Attrib for BlockType: WiFi Welcome:Show Acceptance Checkbox
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Acceptance Checkbox", "ShowAccept", "", @"Show or hide the ""I Accept"" checkbox. If it is visible then it will be required. This should be visible if the ""Terms And Conditions"" are also visible.", 6, @"True", "792D8022-9565-4067-9EFD-1A97B8659725" );

            // Attrib for BlockType: WiFi Welcome:Acceptance Checkbox Label
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Acceptance Checkbox Label", "AcceptanceLabel", "", @"Text used to signify user agreement with the Terms and Conditions", 7, @"I Accept", "E38A008E-94B5-4B81-A6A0-FBCDA44CDF20" );

            // Attrib for BlockType: WiFi Welcome:Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Button Text", "ButtonText", "", @"Text to display on the button", 8, @"Connect To WiFi", "13DA26CB-8155-4B2F-B476-6622FEE46D23" );

            // Attrib for BlockType: WiFi Welcome:Show Legal Note
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Legal Note", "ShowLegalNote", "", @"Show or hide the Terms and Conditions. This should be always be visible unless users are being automatically connected without any agreement needed.", 9, @"True", "9D8B8CC4-C19E-4EF3-98B4-4426A21B7915" );

            // Attrib for BlockType: WiFi Welcome:Legal Note
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Legal Note", "LegalNote", "", @"A legal note outlining the Terms and Conditions for using WiFi", 10, @"<!DOCTYPE html>
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
            // Attrib for BlockType: WiFi Welcome:Show Mobile Phone
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Mobile Phone", "ShowMobilePhone", "", @"Show or hide the Mobile Phone Number field. If it is visible then it will be required.", 4, @"True", "B907D897-B478-4225-9D1F-BA559297F168" );
            // Attrib for BlockType: WiFi Welcome:MAC Address Param
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MAC Address Param", "MacAddressParam", "", @"The query string parameter used for the MAC Address", 0, @"client_mac", "8483A458-0D5A-4C9F-B2A0-713046A06192" );
            // Attrib for BlockType: WiFi Welcome:Release Link
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Release Link", "ReleaseLink", "", @"The URL to redirect users to after registration.", 1, @"", "31F3E224-38A2-4D7C-A5C9-B34FCAFC98CD" );
            // Attrib for BlockType: WiFi Welcome:Show First Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "CCFCD227-C8F9-4952-8AC5-E427D519EE47", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show First Name", "ShowFirstName", "", @"Show or hide the First Name field. If it is visible then it will be required.", 2, @"True", "2CF20676-15A1-4827-9A2B-3144498336E8" );
            // Attrib for BlockType: Calendar Lava:Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "", @"Select campuses to display calendar events for. No selection will show all.", 4, @"", "1FEE3444-0FC5-4708-99A7-11F4B624CCB1" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Interaction", "InteractionEndDateTime");
            Sql( @"DELETE FROM [DefinedType] WHERE [Guid] = '828ADECE-EFE7-49DF-BA8C-B3F132509A95'" );

            #region WiFi Page & Block Data
            // Attrib for BlockType: WiFi Welcome:Show First Name
            RockMigrationHelper.DeleteAttribute( "2CF20676-15A1-4827-9A2B-3144498336E8" );
            // Attrib for BlockType: WiFi Welcome:Release Link
            RockMigrationHelper.DeleteAttribute( "31F3E224-38A2-4D7C-A5C9-B34FCAFC98CD" );
            // Attrib for BlockType: WiFi Welcome:MAC Address Param
            RockMigrationHelper.DeleteAttribute( "8483A458-0D5A-4C9F-B2A0-713046A06192" );
            // Attrib for BlockType: WiFi Welcome:Show Mobile Phone
            RockMigrationHelper.DeleteAttribute( "B907D897-B478-4225-9D1F-BA559297F168" );
            // Attrib for BlockType: WiFi Welcome:Legal Note
            RockMigrationHelper.DeleteAttribute( "581389F2-90D1-4E0D-B8DD-2195D73F9A59" );
            // Attrib for BlockType: WiFi Welcome:Show Legal Note
            RockMigrationHelper.DeleteAttribute( "9D8B8CC4-C19E-4EF3-98B4-4426A21B7915" );
            // Attrib for BlockType: WiFi Welcome:Button Text
            RockMigrationHelper.DeleteAttribute( "13DA26CB-8155-4B2F-B476-6622FEE46D23" );
            // Attrib for BlockType: WiFi Welcome:Acceptance Checkbox Label
            RockMigrationHelper.DeleteAttribute( "E38A008E-94B5-4B81-A6A0-FBCDA44CDF20" );
            // Attrib for BlockType: WiFi Welcome:Show Acceptance Checkbox
            RockMigrationHelper.DeleteAttribute( "792D8022-9565-4067-9EFD-1A97B8659725" );
            // Attrib for BlockType: WiFi Welcome:Show Email
            RockMigrationHelper.DeleteAttribute( "F7257786-5186-435F-8B69-F41A9500169D" );
            // Attrib for BlockType: WiFi Welcome:Show Last Name
            RockMigrationHelper.DeleteAttribute( "45DF60FC-D764-49D5-B79F-3C7AC892C4A6" );

            // Remove Block: WiFi Connection Error, from Page: CaptivePortalError, Site: External Website
            RockMigrationHelper.DeleteBlock( "562519A7-967B-45CA-AABA-2C71D28F2303" );
            // Remove Block: Wifi Success, from Page: CaptivePortalSuccess, Site: External Website
            RockMigrationHelper.DeleteBlock( "63A5398C-7974-4FED-8709-1CCA567C5D09" );
            // Remove Block: WiFi Welcome, from Page: Captive Portal, Site: External Website
            RockMigrationHelper.DeleteBlock( "0630F12E-8645-462C-B599-4306BFC35B19" );

            RockMigrationHelper.DeleteBlockType( "CCFCD227-C8F9-4952-8AC5-E427D519EE47" ); // WiFi Welcome
            RockMigrationHelper.DeleteBlockType( "A5C1BFC0-D4CD-4A2A-908D-329105CBE36A" ); // Examples > Captive Portal Success
            RockMigrationHelper.DeleteBlockType( "86651942-1F75-4DD2-AA98-F0B0C80315C6" ); // Examples > Captive Portal Error

            RockMigrationHelper.DeletePage( "A50EBCA2-11B3-4DD2-A2DD-E7939EDDF23F" ); //  Page: CaptivePortalError, Layout: Homepage, Site: External Website
            RockMigrationHelper.DeletePage( "80613598-D1F6-4819-BCB0-7204E59D98AC" ); //  Page: CaptivePortalSuccess, Layout: Homepage, Site: External Website
            RockMigrationHelper.DeletePage( "C9767AC5-11A8-4B48-B487-911BA9CADF8C" ); //  Page: Captive Portal, Layout: Homepage, Site: External Website
            #endregion
        }
    }
}
