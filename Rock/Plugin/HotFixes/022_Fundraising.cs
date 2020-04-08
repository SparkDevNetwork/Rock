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
    /// Migration to add the Fundraising feature data bits.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 22, "1.6.2" )]
    public class Fundraising : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Added to normal migration: 201704031947125_Fundraising

//            // 1.6.3 Migration Rollups
//            // JE: Fix Payment Details Block Setting
//            RockMigrationHelper.UpdateBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Payment Reminder Email", "DefaultPaymentReminderEmail", "", "The default Payment Reminder Email Template value to use for a new template", 3, @"{{ 'Global' | Attribute:'EmailHeader' }}
//{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
//{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}

//<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Payment Reminder</h1>

//<p>
//    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} for {{ RegistrationInstance.Name }} has a remaining balance 
//    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}. The 
//    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | Downcase | Pluralize  }} for this 
//    {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} are below.
//</p>

//{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
//{% assign registrantCount = registrants | Size %}
//{% if registrantCount > 0 %}
//	<ul>
//	{% for registrant in registrants %}
//		<li>{{ registrant.PersonAlias.Person.FullName }}</li>
//	{% endfor %}
//	</ul>
//{% endif %}

//{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
//{% assign waitListCount = waitlist | Size %}
//{% if waitListCount > 0 %}
//    <p>
//        The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
//		{% if waitListCount > 1 %}are{% else %}is{% endif %} still on the wait list:
//   </p>
    
//    <ul>
//    {% for registrant in waitlist %}
//        <li>
//            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
//        </li>
//    {% endfor %}
//    </ul>
//{% endif %}

//<p>
//    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
//    using our <a href='{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
//    online registration page</a>.
//</p>

//<p>
//    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
//</p>

//{{ 'Global' | Attribute:'EmailFooter' }}", "C8AB59C0-3074-418E-8493-2BCED16D5034" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            /*
            RockMigrationHelper.DeleteByGuid( "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", "NoteType" );

            RockMigrationHelper.DeleteAttribute( "F3338652-D1A2-4778-82A7-D56B9F4CFD7F" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Title
            RockMigrationHelper.DeleteAttribute( "237463F7-A206-4B43-AFDD-84E422527E87" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Date Range
            RockMigrationHelper.DeleteAttribute( "2339847F-2746-41D9-8CB5-2410FC8358D2" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Location
            RockMigrationHelper.DeleteAttribute( "697FDCF1-CA91-4DB5-9306-CD4835108613" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Summary
            RockMigrationHelper.DeleteAttribute( "125F7AAC-F01D-4527-AA5E-5C8345AC3F66" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Photo
            RockMigrationHelper.DeleteAttribute( "1E2F1416-2C4C-44DF-BE19-7D8FA9523115" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Details
            RockMigrationHelper.DeleteAttribute( "7CD834F8-43F2-400E-A352-898030124102" );    // GroupType - Group Attribute, Fundraising Opportunity: Individual Fundraising Goal
            RockMigrationHelper.DeleteAttribute( "F0846135-1A61-4AFA-8F9B-76D9821084DE" );    // GroupType - Group Attribute, Fundraising Opportunity: Opportunity Type
            RockMigrationHelper.DeleteAttribute( "6756D396-97F8-48A0-B69C-279E561F9D48" );    // GroupType - Group Attribute, Fundraising Opportunity: Update Content Channel
            RockMigrationHelper.DeleteAttribute( "38E1065D-4F6A-428E-B781-48F6BDACA614" );    // GroupType - Group Attribute, Fundraising Opportunity: Enable Commenting
            RockMigrationHelper.DeleteAttribute( "E06EBFAD-E0B1-4AE2-B9B1-4C988EFFA844" );    // GroupType - Group Attribute, Fundraising Opportunity: Registration Instance
            RockMigrationHelper.DeleteAttribute( "9BEA4F1C-E2FD-4669-B2CD-1269D4DCB97A" );    // GroupType - Group Attribute, Fundraising Opportunity: Allow Individual Disabling of Contribution Requests
            RockMigrationHelper.DeleteAttribute( "49012757-0ADE-419A-981C-384417D2E543" );    // GroupType - Group Attribute, Fundraising Opportunity: Cap Fundraising Amount
            RockMigrationHelper.DeleteAttribute( "7C6FF01B-F68E-4A83-A96D-85071A92AAF1" );    // GroupType - Group Attribute, Fundraising Opportunity: Financial Account
            RockMigrationHelper.DeleteAttribute( "BBD6C818-765C-43FB-AA72-5AF66F91B499" );    // GroupType - Group Attribute, Fundraising Opportunity: Show Public
            RockMigrationHelper.DeleteAttribute( "EABAE672-0886-450B-9296-2BADC56A0137" );    // GroupType - Group Member Attribute, Fundraising Opportunity: Individual Fundraising Goal
            RockMigrationHelper.DeleteAttribute( "018B201C-D9C2-4EDE-9FC9-B52E2F799325" );    // GroupType - Group Member Attribute, Fundraising Opportunity: PersonalOpportunityIntroduction
            RockMigrationHelper.DeleteAttribute( "2805298E-E21A-4679-B5CA-69D6FF4EAD31" );    // GroupType - Group Member Attribute, Fundraising Opportunity: Disable Public Contribution Requests

            RockMigrationHelper.DeleteGroupTypeRole( "F82DF077-9664-4DA8-A3D9-7379B690124D" );
            RockMigrationHelper.DeleteGroupTypeRole( "253973A5-18F2-49B6-B2F1-F8F84294AAB2" );

            RockMigrationHelper.DeleteGroupType( "4BE7FC44-332D-40A8-978E-47B7035D7A0C" );

            RockMigrationHelper.DeleteDefinedType( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D" );

            RockMigrationHelper.DeleteCategory( "91B43FBD-F924-4934-9CCE-7990513275CF" );


            //// Down() for Fundraising Blocks/Pages migration
            // Attrib for BlockType: Fundraising Leader Toolbox:Participant Page
            RockMigrationHelper.DeleteAttribute( "A2C8F514-8805-4E0A-9493-75289F543B43" );
            // Attrib for BlockType: Fundraising Leader Toolbox:Main Page
            RockMigrationHelper.DeleteAttribute( "E58C0A8D-FE41-4FE0-B5F4-F7E8B3934D7A" );
            // Attrib for BlockType: Fundraising Leader Toolbox:Summary Lava Template
            RockMigrationHelper.DeleteAttribute( "6F245AB0-5EEC-4EF9-A029-6C6BFB0ED64B" );
            // Attrib for BlockType: Transaction Entry:Transaction Entity Type
            RockMigrationHelper.DeleteAttribute( "4F712013-75C7-4157-8EF4-2EF26210378B" );
            // Attrib for BlockType: Transaction Entry:Enable Initial Back button
            RockMigrationHelper.DeleteAttribute( "86A2A716-3F48-4AA1-8B18-E2BB47C8FD40" );
            // Attrib for BlockType: Transaction Entry:Transaction Header
            RockMigrationHelper.DeleteAttribute( "65FB0B9A-670E-4AB9-9666-77959B4B702E" );
            // Attrib for BlockType: Transaction Entry:Entity Id Param
            RockMigrationHelper.DeleteAttribute( "8E45ABBB-43A8-46B1-A32C-DB9474A65BE0" );
            // Attrib for BlockType: Transaction Entry:Transaction Type
            RockMigrationHelper.DeleteAttribute( "ADB22E3F-1DC0-4BA6-AC77-09FE8580CD21" );
            // Attrib for BlockType: Transaction Entry:Allowed Transaction Attributes From URL
            RockMigrationHelper.DeleteAttribute( "B4C8AA1A-E43E-48F1-9221-C83F9E750352" );
            // Attrib for BlockType: Transaction Entry:Account Campus Context
            RockMigrationHelper.DeleteAttribute( "0440B425-57B2-4E65-84C8-5B05D9A46708" );
            // Attrib for BlockType: Transaction Entry:Invalid Account Message
            RockMigrationHelper.DeleteAttribute( "00106DDE-CD23-4E4B-A4B6-B3819E196364" );
            // Attrib for BlockType: Transaction Entry:Only Public Accounts In URL
            RockMigrationHelper.DeleteAttribute( "A13AC34C-4790-430F-8182-43AAC01FF177" );
            // Attrib for BlockType: Transaction Entry:Allow Accounts In URL
            RockMigrationHelper.DeleteAttribute( "E4492D68-45EA-41FF-A611-760DB13EC36E" );
            // Attrib for BlockType: Transaction Entry:Account Header Template
            RockMigrationHelper.DeleteAttribute( "F71BD118-F1EB-4E93-AD7B-86D2A40AAE95" );
            // Attrib for BlockType: Fundraising Donation Entry:Show First Name Only
            RockMigrationHelper.DeleteAttribute( "AF583D88-2DCA-4589-AF53-CBE61295C02E" );
            // Attrib for BlockType: Fundraising Donation Entry:Transaction Entry Page
            RockMigrationHelper.DeleteAttribute( "4E9D70B9-9CF6-4F6C-87B4-8B0DDFDFEB3E" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Donation Page
            RockMigrationHelper.DeleteAttribute( "5A0D8B2E-8692-481D-BD1E-48236021BFF0" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Note Type
            RockMigrationHelper.DeleteAttribute( "C3494517-31E3-4B04-AE37-570331073903" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Updates Lava Template
            RockMigrationHelper.DeleteAttribute( "17DF7E42-B2D7-4E5D-9EF7-EE25758139FC" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Main Page
            RockMigrationHelper.DeleteAttribute( "592C88ED-6993-4292-96FA-C05CB8A6F00C" );
            // Attrib for BlockType: Fundraising Opportunity Participant:Profile Lava Template
            RockMigrationHelper.DeleteAttribute( "84C3DD64-436E-40BC-ADC3-7F86BBB890C0" );
            // Attrib for BlockType: Fundraising Opportunity View:Updates Lava Template
            RockMigrationHelper.DeleteAttribute( "AFC2C61D-87F8-4C4E-9CC2-98F2009A500C" );
            // Attrib for BlockType: Fundraising Opportunity View:Sidebar Lava Template
            RockMigrationHelper.DeleteAttribute( "393030EB-18B6-4D91-943F-BAB3853B84BD" );
            // Attrib for BlockType: Fundraising Opportunity View:Summary Lava Template
            RockMigrationHelper.DeleteAttribute( "B802BE78-42DE-4E0F-8C1E-5788582C905B" );
            // Attrib for BlockType: Fundraising Opportunity View:Donation Page
            RockMigrationHelper.DeleteAttribute( "685DA61C-AF28-4389-AA7F-4BC26BED6CDD" );
            // Attrib for BlockType: Fundraising Opportunity View:Participant Page
            RockMigrationHelper.DeleteAttribute( "EF966837-E420-45B9-A740-F1E43C08469D" );
            // Attrib for BlockType: Fundraising Opportunity View:Note Type
            RockMigrationHelper.DeleteAttribute( "287571CE-B731-477A-B948-FD05736C2CFE" );
            // Attrib for BlockType: Fundraising Opportunity View:Leader Toolbox Page
            RockMigrationHelper.DeleteAttribute( "9F8F6E06-338F-403E-9D2D-A4FCBA13A844" );
            // Attrib for BlockType: Fundraising List:Lava Template
            RockMigrationHelper.DeleteAttribute( "ED2EA497-4316-4E44-A5A4-69E69CC7ECBC" );
            // Attrib for BlockType: Fundraising List:Details Page
            RockMigrationHelper.DeleteAttribute( "F17BD62D-8134-47A5-BDBC-F7F6CD07974E" );

            // Attrib for BlockType: Transaction Entity Matching:EntityTypeQualifierValue
            RockMigrationHelper.DeleteAttribute( "9488B744-D932-4CB9-AEEC-EEF54573DB8B" );
            // Attrib for BlockType: Transaction Entity Matching:EntityTypeQualifierColumn
            RockMigrationHelper.DeleteAttribute( "A9BB04F1-7C63-4ABF-A174-D93003B2833F" );
            // Attrib for BlockType: Transaction Entity Matching:EntityTypeId
            RockMigrationHelper.DeleteAttribute( "9FDA9232-AF40-4471-B371-FDB480F7F2CF" );
            // Attrib for BlockType: Transaction Entity Matching:TransactionTypeId
            RockMigrationHelper.DeleteAttribute( "3F6A0F3A-EEC9-4DD8-B0CB-3030103D3422" );

            // Remove Block: Fundraising Leader Toolbox, from Page: Fundraising Leader Toolbox, Site: External Website
            RockMigrationHelper.DeleteBlock( "558375A3-2DFF-43F3-A9EF-04F503C7EB55" );
            // Remove Block: Fundraising Transaction Entry, from Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.DeleteBlock( "1BAD904E-2F79-4488-B8BE-EECD67AE2925" );
            // Remove Block: Fundraising Donation Entry, from Page: Fundraising Donation Entry, Site: External Website
            RockMigrationHelper.DeleteBlock( "B557FC47-0D19-4EED-A386-04CF569E5967" );
            // Remove Block: Fundraising Opportunity Participant, from Page: Fundraising Participant, Site: External Website
            RockMigrationHelper.DeleteBlock( "BAF6AD44-BFBB-46AE-B1F2-89511C273FAE" );
            // Remove Block: Fundraising Opportunity View, from Page: Fundraising Opportunity View, Site: External Website
            RockMigrationHelper.DeleteBlock( "59786A7C-8AFE-4DB2-988A-F72B82D6FD5C" );
            // Remove Block: Fundraising List, from Page: Missions, Site: External Website
            RockMigrationHelper.DeleteBlock( "7759CDA6-BCE0-42D9-99C8-E991600F7E0D" );
            // Remove Block: Fundraising Transaction Group Member Matching, from Page: Fundraising Transaction Matching, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "85B35E05-BAD4-44F1-8E81-EF77959F199B" );

            RockMigrationHelper.DeleteBlockType( "1FEA697F-DD12-4FE0-BC58-EE896123E7F1" ); // Fundraising Opportunity Participant
            RockMigrationHelper.DeleteBlockType( "DA5F83B9-7F6A-4CF6-AF23-0D89DA4D4241" ); // Fundraising Opportunity View
            RockMigrationHelper.DeleteBlockType( "E664BB02-D501-40B0-AAD6-D8FA0E63438B" ); // Fundraising List
            RockMigrationHelper.DeleteBlockType( "B90F730D-6319-4749-A3C0-BBFDD69D9BC3" ); // Fundraising Leader Toolbox
            RockMigrationHelper.DeleteBlockType( "A24D68F2-C58B-4322-AED8-6556DBED1B76" ); // Fundraising Donation Entry
            RockMigrationHelper.DeleteBlockType( "A58BCB1E-01D9-4F60-B925-D831A9537051" ); // Transaction Entity Matching
            RockMigrationHelper.DeletePage( "F04D69C1-786A-4204-8A67-5669BDFEB533" ); //  Page: Fundraising Transaction Entry, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "9F76591C-CEE4-4824-8478-E3BDA48D66ED" ); //  Page: Fundraising Participant, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "E40BEA3D-0304-4AD2-A45D-9BAD9852E3BA" ); //  Page: Fundraising Donation Entry, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "9DADC93F-C9E7-4567-B73E-AD264A93E37D" ); //  Page: Fundraising Leader Toolbox, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "BA673ABE-A45A-4835-A3A0-94A60341B96F" ); //  Page: Fundraising Opportunity View, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD" ); //  Page: Fundraising Transaction Matching, Layout: Full Width, Site: Rock RMS
            */
        }
    }
}
