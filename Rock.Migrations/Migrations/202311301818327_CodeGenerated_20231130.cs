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

    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20231130 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block               
            //  Block Name: Login              
            //  Page Name: Log In              
            //  Layout: -              
            //  Site: External Website              
            RockMigrationHelper.AddBlock( true, "D025E14C-F385-43FB-A735-ABD24ADC1480".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "5437C991-536D-4D9C-BE58-CBDB59D1BBB3".AsGuid(), "Login", "Main", @"", @"", 0, "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06" );

            // Add Block               
            //  Block Name: New Account              
            //  Page Name: Account Registration              
            //  Layout: -              
            //  Site: External Website              
            RockMigrationHelper.AddBlock( true, "BBAD3127-8629-400C-BD11-9A554AA107C7".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "E5C34503-DDAD-4881-8463-0E1E20B1675D".AsGuid(), "New Account", "Main", @"", @"", 0, "058E78F6-4911-4876-BE08-941C210ED8DA" );

            // Attribute for BlockType              
            //   BlockType: Attendance Detail              
            //   Category: Check-in > Manager              
            //   Attribute: Allow Editing Start and End Times              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA59CE67-9313-4B9F-8593-380044E5AE6A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Editing Start and End Times", "AllowEditingStartAndEndTimes", "Allow Editing Start and End Times", @"This allows editing the start and end datetime to be edited", 7, @"False", "2E10F8A0-4892-42D6-B03A-8179DC2750A8" );

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimitToSecurityRoleGroups", "Limit to Security Role Groups", @"", 5, @"False", "926F9CD7-2C0B-406D-A2BD-7B4A3A4F0E92" );

            // Add Block Attribute Value              
            //   Block: Financial Batch              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Block Location: Page=Batches, Site=Rock RMS              
            //   Attribute: core.CustomGridColumnsConfig              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "C51D8AFD-4674-4728-8057-092F136BADCE", "7A18830F-5BEC-41A5-85DF-D749F4188964", @"" );

            // Add Block Attribute Value              
            //   Block: Financial Batch              
            //   BlockType: Financial Batch List              
            //   Category: Finance              
            //   Block Location: Page=Batches, Site=Rock RMS              
            //   Attribute: core.CustomGridEnableStickyHeaders              /*   Attribute Value: False */              
            RockMigrationHelper.AddBlockAttributeValue( "C51D8AFD-4674-4728-8057-092F136BADCE", "11B0E522-4793-476F-994B-843279B4A6F1", @"False" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Forgot Username              /*   Attribute Value: 113593ff-620e-4870-86b1-7a0ec0409208 */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "74B1010B-C387-46C7-B98E-CE3D6836213F", @"113593ff-620e-4870-86b1-7a0ec0409208" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Confirm Account              /*   Attribute Value: 17aaceef-15ca-4c30-9a3a-11e6cf7e6411 */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "35377B42-22EC-458D-B521-AA0D7B6B892D", @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Account Created              /*   Attribute Value: 84e373e9-3aaf-4a31-b3fb-a8e3f0666710 */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "79115CB5-D494-4505-9BE2-E76DD647E9F2", @"84e373e9-3aaf-4a31-b3fb-a8e3f0666710" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Connection Status              /*   Attribute Value: 368DD475-242C-49C4-A42C-7278BE690CC2 */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "6FBB35C7-846B-4543-8161-3728102DD2FB", @"368DD475-242C-49C4-A42C-7278BE690CC2" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Record Status              /*   Attribute Value: 283999EC-7346-42E3-B807-BCE9B2BABB49 */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "4CCA6AEA-0574-42EB-836A-DC04D3E0FA15", @"283999EC-7346-42E3-B807-BCE9B2BABB49" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Phone Types              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "BDCE233D-284F-4476-BD9C-42C0326B2A7D", @"" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Phone Types Required              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "7EB29BB0-B60C-49C8-82F0-35D082A0AFAC", @"" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Minimum Age              /*   Attribute Value: 13 */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "A9BEC7A7-D3FD-43AA-A5CF-DF571342B9AF", @"13" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Attribute Categories              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "9B1E994E-1DBD-4B94-B8CA-7E7D3978C2C3", @"" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Location Type              /*   Attribute Value: 8C52E53C-2A66-435A-AE6E-5EE307D9A0DC */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "4BA67F1C-E1DB-451D-8B6B-951048314793", @"8C52E53C-2A66-435A-AE6E-5EE307D9A0DC" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Username Field Label              /*   Attribute Value: Username */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "94F05F5E-F3EC-4F2E-98FD-04DB1DCC692B", @"Username" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Found Duplicate Caption              /*   Attribute Value: There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you? */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "1DFCFD47-BC0C-4005-B751-5566F5E6FD01", @"There are already one or more people in our system that have the same email address and last name as you do.  Are any of these people you?" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Existing Account Caption              /*   Attribute Value: {0}, you already have an existing account.  Would you like us to email you the username? */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "8426CA8E-7491-4A0F-810E-48E6BDDFC229", @"{0}, you already have an existing account.  Would you like us to email you the username?" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Sent Login Caption              /*   Attribute Value: Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password. */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "2B8FDE04-69C4-414D-A62D-7030872CB018", @"Your username has been emailed to you.  If you've forgotten your password, the email includes a link to reset your password." );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Confirm Caption              /*   Attribute Value: Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue. */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "C6BB3C69-3DE0-443E-AE95-5E308567EA1C", @"Because you've selected an existing person, we need to have you confirm the email address you entered belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue." );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Success Caption              /*   Attribute Value: {0}, Your account has been created */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "822FC7F0-B630-47FC-987D-FB5F9CC60564", @"{0}, Your account has been created" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Campus Selector Label              /*   Attribute Value: Campus */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "218AD2D8-129C-4EEA-9B7D-5211BAC4A863", @"Campus" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Require Email For Username              /*   Attribute Value: False */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "AF46BAB7-81DE-41D3-8A5A-023CADB01316", @"False" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Check For Duplicates              /*   Attribute Value: True */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "7586F7A2-D697-433C-A2AF-E549935AA598", @"True" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Show Address              /*   Attribute Value: False */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "F0650100-74A3-4356-9DCA-E05F74453699", @"False" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Address Required              /*   Attribute Value: False */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "0112FC95-B856-4C34-B693-CD9E296C93D5", @"False" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Show Phone Numbers              /*   Attribute Value: False */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "D734E889-3EDE-4E3D-BE3E-682F0A9351D6", @"False" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Show Campus              /*   Attribute Value: False */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "9CAE407A-2669-4AA7-8110-119A3FCBB0C4", @"False" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Save Communication History              /*   Attribute Value: False */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "830EC8A0-9690-497A-9EC1-4EBD8A21E9BE", @"False" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Show Gender              /*   Attribute Value: True */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "442A7BF5-50E0-4DD8-9BB2-F36160DEB50B", @"True" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Confirmation Page              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "20D0B45D-CB1B-4037-9CEC-7088707731CA", @"" );

            // Add Block Attribute Value              
            //   Block: New Account              
            //   BlockType: Account Entry              
            //   Category: Security              
            //   Block Location: Page=Account Registration, Site=External Website              
            //   Attribute: Login Page              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "058E78F6-4911-4876-BE08-941C210ED8DA", "507591E3-0828-4600-A9D9-AAB2F5358D0B", @"" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: New Account Page              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "D371AC88-F24B-4BF4-8ACF-BC7594161F45", @"" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Help Page              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "4A82C63D-D034-4B19-B922-2E24539F2B75", @"" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Confirmation Page              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "1721E639-21B3-4825-8F73-B7AFCB59461D", @"" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Redirect Page              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "56D7F241-E008-4061-BEA5-E94EC5D48651", @"" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Show Internal Database Login              /*   Attribute Value: True */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "7A4520AD-1C92-43CD-9E24-DB0E2A32F898", @"True" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Username Field Label              /*   Attribute Value: Username */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "DE1525F5-05F0-4B57-8F73-8C9ED7A421EC", @"Username" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: New Account Text              /*   Attribute Value: Register */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "AE9AEFA8-7F23-4065-B0C9-D36B5E996E2F", @"Register" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Confirm Caption              /*   Attribute Value: ... */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "7EA1972E-26CF-4481-A354-2392F4228DEC", @"  Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.  " );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Locked Out Caption              /*   Attribute Value: ... */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "5177B76C-8E38-4969-9965-BF73F080307B", @"  Sorry, your account has been locked.  Please contact our office at {{ 'Global' | Attribute:'OrganizationPhone' }} or email {{ 'Global' | Attribute:'OrganizationEmail' }} to resolve this.  Thank-you.   " );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: No Account Text              /*   Attribute Value: Sorry, we couldn't find an account matching that username/password. Can we help you <a href='{{HelpPage}}'>recover your account information</a>? */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "94DF12DB-2CFC-4297-96A2-BE122FD3C004", @"Sorry, we couldn't find an account matching that username/password. Can we help you <a href='{{HelpPage}}'>recover your account information</a>?" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Remote Authorization Prompt Message              /*   Attribute Value: Login with social account */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "2DE2A188-6541-4C6F-B21E-9564E75F45FE", @"Login with social account" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Prompt Message              /*   Attribute Value:  */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "47125A4B-6AD7-4DB8-907D-B45ABC21291B", @"" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Invalid PersonToken Text              /*   Attribute Value: <div class='alert alert-warning'>The login token you provided is no longer valid. Please login below.</div> */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "3D5C6940-C0DD-4CC5-80BB-E66B16FAF1F5", @"<div class='alert alert-warning'>The login token you provided is no longer valid. Please login below.</div>" );

            // Add Block Attribute Value              
            //   Block: Login              
            //   BlockType: Login              
            //   Category: Security              
            //   Block Location: Page=Log In, Site=External Website              
            //   Attribute: Confirm Account Template              /*   Attribute Value: 17aaceef-15ca-4c30-9a3a-11e6cf7e6411 */              
            RockMigrationHelper.AddBlockAttributeValue( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06", "9125032A-03BE-4CC3-8175-89E1A7032A88", @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType              
            //   BlockType: Group Tree View              
            //   Category: Groups              
            //   Attribute: Limit to Security Role Groups              
            RockMigrationHelper.DeleteAttribute( "926F9CD7-2C0B-406D-A2BD-7B4A3A4F0E92" );

            // Attribute for BlockType              
            //   BlockType: Attendance Detail              
            //   Category: Check-in > Manager              
            //   Attribute: Allow Editing Start and End Times              
            RockMigrationHelper.DeleteAttribute( "2E10F8A0-4892-42D6-B03A-8179DC2750A8" );

            // Remove Block              
            //  Name: Login, from Page: Log In, Site: External Website              
            //  from Page: Log In, Site: External Website              
            RockMigrationHelper.DeleteBlock( "A4B5F65A-E86B-4BBE-863A-B0C60AE3CD06" );

            // Remove Block              
            //  Name: New Account, from Page: Account Registration, Site: External Website              
            //  from Page: Account Registration, Site: External Website              
            RockMigrationHelper.DeleteBlock( "058E78F6-4911-4876-BE08-941C210ED8DA" );
        }
    }
}
