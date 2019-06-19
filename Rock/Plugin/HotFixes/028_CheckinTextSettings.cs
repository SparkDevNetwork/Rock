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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Migration to add additional check-in features (check-out/auto-select/barcode check-in)
    /// .
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 28, "1.6.4" )]
    public class CheckinTextSettings : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Moved to core migration: 201711271827181_V7Rollup            

            //// Attrib for BlockType: Person Search:Phone Number Types
            //RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Number Types", "PhoneNumberTypes", "", "Types of phone numbers to include with person detail", 1, @"", "05E58531-95AF-4883-BE0D-1EC034BB81DD" );
            //// Attrib for BlockType: Person Search:Show Gender
            //RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Gender", "ShowGender", "", "Should a gender column be displayed?", 4, @"False", "0EB7AFCB-A274-4D9C-9D5B-7C9613BF9A27" );
            //// Attrib for BlockType: Person Search:Show Envelope Number
            //RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Envelope Number", "ShowEnvelopeNumber", "", "Should an envelope # column be displayed?", 6, @"False", "B6B8F559-BFDE-43DB-9BD7-20EF8B4DAB48" );
            //// Attrib for BlockType: Person Search:Show Spouse
            //RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Spouse", "ShowSpouse", "", "Should a spouse column be displayed?", 5, @"False", "18EA8DDF-0DC5-471E-8A6B-ED978FD1F2BC" );
            //// Attrib for BlockType: Person Search:Show Birthdate
            //RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Birthdate", "ShowBirthdate", "", "Should a birthdate column be displayed?", 2, @"False", "B57C80CA-69F2-4B55-B909-B59EAEAF10F4" );
            //// Attrib for BlockType: Person Search:Show Age
            //RockMigrationHelper.UpdateBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Age", "ShowAge", "", "Should an age column be displayed?", 3, @"True", "F18A1DEE-C0DA-40B1-A75A-9808780F27BF" );
            //// Attrib for BlockType: Welcome:Not Active Yet Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Active Yet Caption", "NotActiveYetCaption", "", "Caption displayed when there are active options today, but none are active now. Use {0} for a countdown timer.", 10, @"This kiosk is not active yet.  Countdown until active: {0}.", "4ACAEE3E-1388-485E-A641-07C69562D317" );
            //// Attrib for BlockType: Welcome:Not Active Yet Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Active Yet Title", "NotActiveYetTitle", "", "Title displayed when there are active options today, but none are active now.", 9, @"Check-in Is Not Active Yet", "151B4CD4-C5CA-47F0-B2EB-348408FC8AE1" );
            //// Attrib for BlockType: Welcome:No Option Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Caption", "NoOptionCaption", "", "The text to display when there are not any families found matching a scanned identifier (barcode, etc).", 14, @"Sorry, there were not any families found with the selected identifier.", "35727C8E-71A2-4272-ACB3-5D407194D728" );
            //// Attrib for BlockType: Welcome:Not Active Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Active Title", "NotActiveTitle", "", "Title displayed when there are not any active options today.", 7, @"Check-in Is Not Active", "32470663-4CCE-4FA8-9AAC-CF7B5C6346D1" );
            //// Attrib for BlockType: Welcome:Not Active Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Not Active Caption", "NotActiveCaption", "", "Caption displayed when there are not any active options today.", 8, @"There are no current or future schedules for this kiosk today!", "1D832AB9-DA71-47B3-B4E8-6661A316BD7B" );
            //// Attrib for BlockType: Welcome:Closed Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Closed Title", "ClosedTitle", "", "", 11, @"Closed", "C4642C7B-049C-4A00-A2EE-1ABE7EF05E61" );
            //// Attrib for BlockType: Welcome:Closed Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Closed Caption", "ClosedCaption", "", "", 12, @"This location is currently closed.", "4AC7E21F-7805-4E43-9301-597E45EA5211" );
            //// Attrib for BlockType: Search:No Option Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "", 6, @"There were not any families that match the search criteria.", "E4AFD216-5386-4E38-B98F-9436601F7B1B" );
            //// Attrib for BlockType: Search:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for search type.", 5, @"Search By {0}", "837B34E3-D140-44CD-8456-9D222325E42E" );
            //// Attrib for BlockType: Family Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 6, @"Select Your Family", "F1911A0A-AC69-474F-9D99-A5022D6E129C" );
            //// Attrib for BlockType: Family Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display.", 5, @"Families", "AF6A6A8B-981A-4ACB-A42C-1D576917C724" );
            //// Attrib for BlockType: Family Select:No Option Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "6B050E12-A232-41F6-94C5-B190F4520607", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "", 7, @"Sorry, no one in your family is eligible to check-in at this location.", "E2C9760F-D5CB-475B-BEDD-E2E249CAB1AF" );
            //// Attrib for BlockType: Person Select:No Option Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "The option to display when there are not any people that match. Use {0} for the current action ('into' or 'out of').", 10, @"Sorry, there are currently not any available areas that the selected person can check {0}.", "9B51C224-E03E-498F-A3A2-855FFF71A103" );
            //// Attrib for BlockType: Person Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family name.", 8, @"{0}", "8ECB1E83-97BB-435E-BFE6-40B4A33ECC9B" );
            //// Attrib for BlockType: Person Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 9, @"Select Person", "50595A53-E5FE-4515-9DBD-EA4B006A5AFF" );
            //// Attrib for BlockType: Person Select:Workflow Activity
            //RockMigrationHelper.UpdateBlockTypeAttribute( "34B48E0F-5E37-425E-9588-E612ED34DB03", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow Activity", "WorkflowActivity", "", "The name of the workflow activity to run on selection.", 1, @"", "3747242A-6852-4B60-BB63-49DEB8A20CF1" );
            //// Attrib for BlockType: Group Type Select:No Option After Select Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option After Select Message", "NoOptionAfterSelectMessage", "", "Message to display when there are not any options available after group type is selected. Use {0} for person's name", 12, @"Sorry, based on your selection, there are currently not any available times that {0} can check into.", "E19DD14C-80DD-4427-B73D-58187E1BE8AD" );
            //// Attrib for BlockType: Group Type Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person/schedule.", 9, @"{0}", "9DDF3190-E07F-4964-9CDC-69AF675FCF2E" );
            //// Attrib for BlockType: Group Type Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Area", "314506D3-FCC0-42AC-86A2-77EE921C0CCD" );
            //// Attrib for BlockType: Group Type Select:No Option Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "7E20E97E-63F2-413D-9C2C-16FF34023F70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.", 11, @"Sorry, there are currently not any available areas that {0} can check into at {1}.", "444058FF-0C4D-4D2E-9FDA-6036AD572C7E" );
            //// Attrib for BlockType: Location Select:No Option After Select Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option After Select Message", "NoOptionAfterSelectMessage", "", "Message to display when there are not any options available after location is selected. Use {0} for person's name", 12, @"Sorry, based on your selection, there are currently not any available times that {0} can check into.", "C14C7143-5D9C-463F-9C8B-2680509C22A5" );
            //// Attrib for BlockType: Location Select:No Option Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.", 11, @"Sorry, there are currently not any available locations that {0} can check into at {1}.", "C94E5760-4EF1-40B4-84B9-B75EFAA1030B" );
            //// Attrib for BlockType: Location Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person/schedule.", 8, @"{0}", "F95CAB1D-37A4-4A53-B63F-BF9D275FBA27" );
            //// Attrib for BlockType: Location Select:Sub Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sub Title", "SubTitle", "", "Sub-Title to display. Use {0} for selected group name.", 9, @"{0}", "A85DEF5A-D6CE-41FE-891E-36880DE5CD9C" );
            //// Attrib for BlockType: Location Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Location", "3CD2A392-3A06-42BE-A3A9-8324D5FCC810" );
            //// Attrib for BlockType: Group Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person/schedule.", 8, @"{0}", "256D4CC4-9F09-47D3-B167-46F876F0ACD3" );
            //// Attrib for BlockType: Group Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Group", "B36D25F5-7A36-4901-9ACE-72ED355F5C6C" );
            //// Attrib for BlockType: Group Select:No Option Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "", 11, @"Sorry, no one in your family is eligible to check-in at this location.", "05FF57CD-A9D2-4E2E-9426-F02EAD95CAA4" );
            //// Attrib for BlockType: Group Select:Sub Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "933418C1-448E-4825-8D3D-BDE23E968483", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sub Title", "SubTitle", "", "Sub-Title to display. Use {0} for selected group type name.", 9, @"{0}", "342CE8B6-7CB1-4D3D-BE1A-3F55CBC3F376" );
            //// Attrib for BlockType: Time Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family/person name.", 5, @"{0}", "B5CF8A58-92E8-4473-BE73-63FB3B6FF49E" );
            //// Attrib for BlockType: Time Select:Sub Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Sub Title", "SubTitle", "", "Sub-Title to display. Use {0} for selected group/location name.", 6, @"{0}", "2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837" );
            //// Attrib for BlockType: Time Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "D2348D51-B13A-4069-97AD-369D9615A711", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 7, @"Select Time(s)", "16091831-474A-4618-872F-E9257F7E9948" );
            //// Attrib for BlockType: Success:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "", 6, @"Checked-in", "E1A36C9B-9D35-4BA2-81D5-87E242D36999" );
            //// Attrib for BlockType: Success:Detail Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "18911F1B-294E-48D6-9E6B-0F72BF6C9491", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Detail Message", "DetailMessage", "", "The message to display indicating person has been checked in. Use {0} for person, {1} for group, {2} for schedule, and {3} for the security code", 7, @"{0} was checked into {1} in {2} at {3}", "2DF1D330-3724-4F2E-B297-D6EB6654398E" );
            //// Attrib for BlockType: Ability Level Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for person's name.", 9, @"{0}", "085D8CA9-82EE-40C2-8985-F3DB36DC4370" );
            //// Attrib for BlockType: Ability Level Select:No Option Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Caption", "NoOptionCaption", "", "", 12, @"Sorry, there are currently not any available options to check into.", "6886E63B-F961-4088-9FD9-72D1A5C84DD7" );
            //// Attrib for BlockType: Ability Level Select:Selection No Option
            //RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Selection No Option", "SelectionNoOption", "", "Text displayed if there are not any options after selecting an ability level. Use {0} for person's name.", 13, @"Sorry, based on your selection, there are currently not any available locations that {0} can check into.", "FDF2C27E-5D91-44EF-AEB7-B559A7711EE5" );
            //// Attrib for BlockType: Ability Level Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 10, @"Select Ability Level", "DE63023F-10BC-4D71-94EA-A9E020016E97" );
            //// Attrib for BlockType: Ability Level Select:No Option Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "605389F5-5BC5-438F-8757-110328B0CED3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Title", "NoOptionTitle", "", "", 11, @"Sorry", "634FE416-500A-4C32-A1E0-123C19681574" );
            //// Attrib for BlockType: Person Select (Family Check-in):Option Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Option Title", "OptionTitle", "", "Title to display on option screen. Use {0} for person's full name.", 9, @"{0}", "816DA5E9-2865-4312-B704-05EC9D83FAB2" );
            //// Attrib for BlockType: Person Select (Family Check-in):Option Sub Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Option Sub Title", "OptionSubTitle", "", "Sub-title to display on option screen. Use {0} for person's nick name.", 10, @"Please select the options that {0} would like to attend.", "5C67A905-7DD5-438E-A60E-BCB5078C0686" );
            //// Attrib for BlockType: Person Select (Family Check-in):No Option Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Option Message", "NoOptionMessage", "", "", 11, @"Sorry, there are currently not any available areas that the selected people can check into.", "A0893943-F77A-42D2-AD21-BEBAF2573A54" );
            //// Attrib for BlockType: Person Select (Family Check-in):Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 8, @"Select People", "3CE01A6B-8F4B-4050-8201-8AAB5F3A79D4" );
            //// Attrib for BlockType: Person Select (Family Check-in):Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family name.", 7, @"{0}", "88D0B3A3-9EFA-45DF-97BF-7F200AFA80BD" );
            //// Attrib for BlockType: Action Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 8, @"Select Action", "1E7D1586-9636-4144-9019-61DE1CFF576F" );
            //// Attrib for BlockType: Action Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "66DDB050-8F60-4DF3-9AED-5CE283E22350", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family name", 7, @"{0}", "0D5B5B30-E1CE-402E-B7FB-760F8E4975B2" );
            //// Attrib for BlockType: Check Out Person Select:Caption
            //RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Caption", "Caption", "", "", 6, @"Select People", "45B1D388-9DBC-4A8A-8AC7-70423E672624" );
            //// Attrib for BlockType: Check Out Person Select:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "54EB0252-6FE4-49C5-8716-14A3A06C3EC5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display. Use {0} for family name", 5, @"{0} Check Out", "05CA1B42-C5F1-407C-9F35-2CF4104BC96D" );
            //// Attrib for BlockType: Check Out Success:Title
            //RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display.", 5, @"Checked Out", "3784B16E-BF23-42A4-8E04-0E93EB71C0D4" );
            //// Attrib for BlockType: Check Out Success:Detail Message
            //RockMigrationHelper.UpdateBlockTypeAttribute( "F499C4A9-9A60-404B-9383-B950EE6D7821", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Detail Message", "DetailMessage", "", "The message to display indicating person has been checked out. Use {0} for person, {1} for group, {2} for location, and {3} for schedule.", 6, @"{0} was checked out of {1} in {2} at {3}.", "A6C1FF95-43D8-4602-9175-B6F0B0523E61" );

            //// Attrib Value for Block:Welcome, Attribute:Not Active Yet Caption Page: Welcome, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "4ACAEE3E-1388-485E-A641-07C69562D317", @"This kiosk is not active yet.  Countdown until active: {0}." );
            //// Attrib Value for Block:Welcome, Attribute:Not Active Yet Title Page: Welcome, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "151B4CD4-C5CA-47F0-B2EB-348408FC8AE1", @"Check-in Is Not Active Yet" );
            //// Attrib Value for Block:Welcome, Attribute:No Option Caption Page: Welcome, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "35727C8E-71A2-4272-ACB3-5D407194D728", @"Sorry, there were not any families found with the selected identifier." );
            //// Attrib Value for Block:Welcome, Attribute:Not Active Title Page: Welcome, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "32470663-4CCE-4FA8-9AAC-CF7B5C6346D1", @"Check-in Is Not Active" );
            //// Attrib Value for Block:Welcome, Attribute:Not Active Caption Page: Welcome, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "1D832AB9-DA71-47B3-B4E8-6661A316BD7B", @"There are no current or future schedules for this kiosk today!" );
            //// Attrib Value for Block:Welcome, Attribute:Closed Title Page: Welcome, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "C4642C7B-049C-4A00-A2EE-1ABE7EF05E61", @"Closed" );
            //// Attrib Value for Block:Welcome, Attribute:Closed Caption Page: Welcome, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "4AC7E21F-7805-4E43-9301-597E45EA5211", @"This location is currently closed." );
            //// Attrib Value for Block:Search, Attribute:No Option Message Page: Search, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "E4AFD216-5386-4E38-B98F-9436601F7B1B", @"There were not any families that match the search criteria." );
            //// Attrib Value for Block:Search, Attribute:Title Page: Search, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "1EF10CB9-DFDC-42CE-9B00-8665050F6B78", "837B34E3-D140-44CD-8456-9D222325E42E", @"Search By {0}" );
            //// Attrib Value for Block:Family Select, Attribute:Caption Page: Family Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "F1911A0A-AC69-474F-9D99-A5022D6E129C", @"Select Your Family" );
            //// Attrib Value for Block:Family Select, Attribute:Title Page: Family Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "AF6A6A8B-981A-4ACB-A42C-1D576917C724", @"Families" );
            //// Attrib Value for Block:Family Select, Attribute:No Option Message Page: Family Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "CD97D61E-7BCE-436B-ACDD-4383EB7490BA", "E2C9760F-D5CB-475B-BEDD-E2E249CAB1AF", @"Sorry, no one in your family is eligible to check-in at this location." );
            //// Attrib Value for Block:Person Select, Attribute:No Option Message Page: Person Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "9B51C224-E03E-498F-A3A2-855FFF71A103", @"Sorry, there are currently not any available areas that the selected person can check {0}." );
            //// Attrib Value for Block:Person Select, Attribute:Title Page: Person Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "8ECB1E83-97BB-435E-BFE6-40B4A33ECC9B", @"{0}" );
            //// Attrib Value for Block:Person Select, Attribute:Caption Page: Person Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "50595A53-E5FE-4515-9DBD-EA4B006A5AFF", @"Select Person" );
            //// Attrib Value for Block:Person Select, Attribute:Workflow Activity Page: Person Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "B2EA00F2-DBB1-4D29-AE9F-748B3B347858", "3747242A-6852-4B60-BB63-49DEB8A20CF1", @"" );
            //// Attrib Value for Block:Group Type Select, Attribute:No Option After Select Message Page: Group Type Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "E19DD14C-80DD-4427-B73D-58187E1BE8AD", @"Sorry, based on your selection, there are currently not any available times that {0} can check into." );
            //// Attrib Value for Block:Group Type Select, Attribute:Title Page: Group Type Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "9DDF3190-E07F-4964-9CDC-69AF675FCF2E", @"{0}" );
            //// Attrib Value for Block:Group Type Select, Attribute:Caption Page: Group Type Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "314506D3-FCC0-42AC-86A2-77EE921C0CCD", @"Select Area" );
            //// Attrib Value for Block:Group Type Select, Attribute:No Option Message Page: Group Type Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0934264E-2EFC-4640-8FE9-F772BFDF34BF", "444058FF-0C4D-4D2E-9FDA-6036AD572C7E", @"Sorry, there are currently not any available areas that {0} can check into at {1}." );
            //// Attrib Value for Block:Location Select, Attribute:No Option After Select Message Page: Location Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "C14C7143-5D9C-463F-9C8B-2680509C22A5", @"Sorry, based on your selection, there are currently not any available times that {0} can check into." );
            //// Attrib Value for Block:Location Select, Attribute:No Option Message Page: Location Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "C94E5760-4EF1-40B4-84B9-B75EFAA1030B", @"Sorry, there are currently not any available locations that {0} can check into at {1}." );
            //// Attrib Value for Block:Location Select, Attribute:Title Page: Location Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "F95CAB1D-37A4-4A53-B63F-BF9D275FBA27", @"{0}" );
            //// Attrib Value for Block:Location Select, Attribute:Sub Title Page: Location Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "A85DEF5A-D6CE-41FE-891E-36880DE5CD9C", @"{0}" );
            //// Attrib Value for Block:Location Select, Attribute:Caption Page: Location Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "9D876B07-DF35-4355-85B0-638F65C367C4", "3CD2A392-3A06-42BE-A3A9-8324D5FCC810", @"Select Location" );
            //// Attrib Value for Block:Group Select, Attribute:Title Page: Group Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "256D4CC4-9F09-47D3-B167-46F876F0ACD3", @"{0}" );
            //// Attrib Value for Block:Group Select, Attribute:Caption Page: Group Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "B36D25F5-7A36-4901-9ACE-72ED355F5C6C", @"Select Group" );
            //// Attrib Value for Block:Group Select, Attribute:No Option Message Page: Group Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "05FF57CD-A9D2-4E2E-9426-F02EAD95CAA4", @"Sorry, no one in your family is eligible to check-in at this location." );
            //// Attrib Value for Block:Group Select, Attribute:Sub Title Page: Group Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "147CE2DA-1D94-4FFE-BEBA-7A6721D54604", "342CE8B6-7CB1-4D3D-BE1A-3F55CBC3F376", @"{0}" );
            //// Attrib Value for Block:Time Select, Attribute:Title Page: Time Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "B5CF8A58-92E8-4473-BE73-63FB3B6FF49E", @"{0}" );
            //// Attrib Value for Block:Time Select, Attribute:Sub Title Page: Time Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837", @"{0}" );
            //// Attrib Value for Block:Time Select, Attribute:Caption Page: Time Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "472E00D1-BD9B-407A-92C6-05132039DB65", "16091831-474A-4618-872F-E9257F7E9948", @"Select Time(s)" );
            //// Attrib Value for Block:Success, Attribute:Title Page: Success, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "E1A36C9B-9D35-4BA2-81D5-87E242D36999", @"Checked-in" );
            //// Attrib Value for Block:Success, Attribute:Detail Message Page: Success, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "9BBBCAFC-5FA3-481E-AFAB-E82BA69B405D", "2DF1D330-3724-4F2E-B297-D6EB6654398E", @"{0} was checked into {1} in {2} at {3}" );
            //// Attrib Value for Block:Ability Level Select, Attribute:Title Page: Ability Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "085D8CA9-82EE-40C2-8985-F3DB36DC4370", @"{0}" );
            //// Attrib Value for Block:Ability Level Select, Attribute:No Option Caption Page: Ability Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "6886E63B-F961-4088-9FD9-72D1A5C84DD7", @"Sorry, there are currently not any available options to check into." );
            //// Attrib Value for Block:Ability Level Select, Attribute:Selection No Option Page: Ability Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "FDF2C27E-5D91-44EF-AEB7-B559A7711EE5", @"Sorry, based on your selection, there are currently not any available locations that {0} can check into." );
            //// Attrib Value for Block:Ability Level Select, Attribute:Caption Page: Ability Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "DE63023F-10BC-4D71-94EA-A9E020016E97", @"Select Ability Level" );
            //// Attrib Value for Block:Ability Level Select, Attribute:No Option Title Page: Ability Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "C175A9ED-612E-4B25-BED4-CF713D922179", "634FE416-500A-4C32-A1E0-123C19681574", @"Sorry" );
            //// Attrib Value for Block:Person Select, Attribute:Option Title Page: Person Select (Family Check-in), Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "816DA5E9-2865-4312-B704-05EC9D83FAB2", @"{0}" );
            //// Attrib Value for Block:Person Select, Attribute:Option Sub Title Page: Person Select (Family Check-in), Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "5C67A905-7DD5-438E-A60E-BCB5078C0686", @"Please select the options that {0} would like to attend." );
            //// Attrib Value for Block:Person Select, Attribute:No Option Message Page: Person Select (Family Check-in), Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "A0893943-F77A-42D2-AD21-BEBAF2573A54", @"Sorry, there are currently not any available areas that the selected people can check into." );
            //// Attrib Value for Block:Person Select, Attribute:Caption Page: Person Select (Family Check-in), Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "3CE01A6B-8F4B-4050-8201-8AAB5F3A79D4", @"Select People" );
            //// Attrib Value for Block:Person Select, Attribute:Title Page: Person Select (Family Check-in), Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "0F82C7EB-3E71-496F-B5F4-83F32AD5EBB5", "88D0B3A3-9EFA-45DF-97BF-7F200AFA80BD", @"{0}" );
            //// Attrib Value for Block:Time Select, Attribute:Title Page: Time Select (Family Check-in), Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "B5CF8A58-92E8-4473-BE73-63FB3B6FF49E", @"{0}" );
            //// Attrib Value for Block:Time Select, Attribute:Sub Title Page: Time Select (Family Check-in), Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837", @"{0}" );
            //// Attrib Value for Block:Time Select, Attribute:Caption Page: Time Select (Family Check-in), Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "558C15C1-47F7-4232-A069-89463B17924F", "16091831-474A-4618-872F-E9257F7E9948", @"Select Time(s)" );
            //// Attrib Value for Block:Action Select, Attribute:Caption Page: Action Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "1E7D1586-9636-4144-9019-61DE1CFF576F", @"Select Action" );
            //// Attrib Value for Block:Action Select, Attribute:Title Page: Action Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "F5C21AE7-4BB4-4628-9B15-7CF761C66891", "0D5B5B30-E1CE-402E-B7FB-760F8E4975B2", @"{0}" );
            //// Attrib Value for Block:Check Out Person Select, Attribute:Caption Page: Check Out Person Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "45B1D388-9DBC-4A8A-8AC7-70423E672624", @"Select People" );
            //// Attrib Value for Block:Check Out Person Select, Attribute:Title Page: Check Out Person Select, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "1F33702B-C26C-4EAA-AD0E-8510384EACBC", "05CA1B42-C5F1-407C-9F35-2CF4104BC96D", @"{0} Check Out" );
            //// Attrib Value for Block:Check Out Success, Attribute:Title Page: Check Out Success, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "32B345DD-0EF4-480E-B82A-7D7191CC374B", "3784B16E-BF23-42A4-8E04-0E93EB71C0D4", @"Checked Out" );
            //// Attrib Value for Block:Check Out Success, Attribute:Detail Message Page: Check Out Success, Site: Rock Check-in
            //RockMigrationHelper.AddBlockAttributeValue( "32B345DD-0EF4-480E-B82A-7D7191CC374B", "A6C1FF95-43D8-4602-9175-B6F0B0523E61", @"{0} was checked out of {1} in {2} at {3}." );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
