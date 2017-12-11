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
    public partial class WaitlistPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "844DC54B-DAEC-47B3-A63A-712DD6D57793", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Wait List", "", "4BF84D3F-DE7B-4F8B-814A-1E728E69C105", "fa fa-calendar-check-o" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Registrant Wait List Move", "Moves the person from the wait list to be a full registrant.", "~/Blocks/Event/RegistrantWaitListMove.ascx", "Event", "AAD07299-F30F-4DB2-8E04-5F3369CE46D2" );

            // Add Block to Page: Wait List, Site: Rock RMS
            RockMigrationHelper.AddBlock( "4BF84D3F-DE7B-4F8B-814A-1E728E69C105", "", "AAD07299-F30F-4DB2-8E04-5F3369CE46D2", "Registrant Wait List Move", "Main", "", "", 0, "DBA2F5A1-3022-442F-8111-A2FB37717B17" );

            // Attrib for BlockType: Registration Instance Detail:Wait List Process Page
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Wait List Process Page", "WaitListProcessPage", "", "The page for moving a person from the wait list to a full registrant.", 8, @"", "07768F68-CAA4-4F68-B808-7344AA595EE6" );
            // Attrib for BlockType: Registration Template Detail:Default Wait List Transition Email
            RockMigrationHelper.AddBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Wait List Transition Email", "DefaultWaitListTransitionEmail", "", "The default Wait List Transition Email Template value to use for a new template", 3, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Payment Reminder</h1>

<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} for {{ RegistrationInstance.Name }} has a remaining balance 
    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}. The 
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | Downcase | Pluralize  }} for this 
    {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} are below.
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>{{ registrant.PersonAlias.Person.FullName }}</li>
{% endfor %}
</ul>

<p>
    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
    using our <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
    online registration page</a>.
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}
", "E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099" );

            // Attrib Value for Block:Registration Template Detail, Attribute:Default Wait List Transition Email Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D6372D00-9FA3-49BF-B0F2-0BE67B5F5D39", "E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099", @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Payment Reminder</h1>

<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} for {{ RegistrationInstance.Name }} has a remaining balance 
    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}. The 
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | Downcase | Pluralize  }} for this 
    {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} are below.
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>{{ registrant.PersonAlias.Person.FullName }}</li>
{% endfor %}
</ul>

<p>
    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
    using our <a href='{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
    online registration page</a>.
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}
" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Display Discount Codes Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "229E65A0-79AD-4D41-AD56-AC3F4AC991E1", @"False" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Wait List Process Page Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "07768F68-CAA4-4F68-B808-7344AA595EE6", @"4bf84d3f-de7b-4f8b-814a-1e728e69c105" );
            RockMigrationHelper.UpdateFieldType( "Address", "", "Rock", "Rock.Field.Types.AddressFieldType", "0A495222-23B7-41D3-82C8-D484CDB75D17" );
            RockMigrationHelper.UpdateFieldType( "Benevolence Request", "", "Rock", "Rock.Field.Types.BenevolenceRequestFieldType", "44EEC881-3C07-4A58-ACC4-0F21D873DBE0" );
            RockMigrationHelper.UpdateFieldType( "Content Channel Types", "", "Rock", "Rock.Field.Types.ContentChannelTypesFieldType", "DF974799-6656-4F0C-883D-85E44EEC999A" );
            RockMigrationHelper.UpdateFieldType( "Workflow", "", "Rock", "Rock.Field.Types.WorkflowFieldType", "1F2692F9-37D3-4B57-8563-36A8CB386C32" );

            // update current templates to have the default wait list email template
            Sql( @"UPDATE [RegistrationTemplate]
		SET [WaitListTransitionEmailTemplate] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% capture externalSite %}{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.Name }} Wait List Update</h1>

<p>
    {{ Registration.FirstName }}, the following individuals have been moved from the {{ RegistrationInstance.Name }} wait list to a full 
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | Downcase }}. 
</p>

<ul>
    {% for registrant in TransitionedRegistrants %}
        <li>{{ registrant.PersonAlias.Person.FullName }}</li>
    {% endfor %}
</ul>

{% if AdditionalFieldsNeeded %}
    <p>
        <strong>Addition information is needed in order to process this registration. Please visit the 
        <a href=''{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}&StartAtBeginning=True''>
        online registration page</a> to complete the registration.</strong>
    </p>
{% endif %}


{% if Registration.BalanceDue > 0 %}
    <p>
        A balance of {{ Registration.BalanceDue | Format:''#,##0.00'' }} remains on this regsitration. You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
        using our <a href=''{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}''>
        online registration page</a>.
    </p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>


{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [WaitListTransitionEmailTemplate] IS NULL" );

            Sql( @"UPDATE [RegistrationTemplate]
		SET [WaitListTransitionSubject] = '{{ RegistrationInstance.Name }} Wait List Update'
		WHERE [WaitListTransitionSubject] IS NULL

  UPDATE [RegistrationTemplate]
		SET [WaitListTransitionFromEmail] = '{{ RegistrationInstance.ContactEmail }}'
		WHERE [WaitListTransitionFromEmail] IS NULL

	UPDATE [RegistrationTemplate]
		SET [WaitListTransitionFromName] = '{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}'
		WHERE [WaitListTransitionFromName] IS NULL" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Registration Instance Detail:Wait List Process Page
            RockMigrationHelper.DeleteAttribute( "07768F68-CAA4-4F68-B808-7344AA595EE6" );

            // Attrib for BlockType: Registration Template Detail:Default Wait List Transition Email
            RockMigrationHelper.DeleteAttribute( "E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099" );

            // Remove Block: Registrant Wait List Move, from Page: Wait List, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DBA2F5A1-3022-442F-8111-A2FB37717B17" );

            RockMigrationHelper.DeletePage( "4BF84D3F-DE7B-4F8B-814A-1E728E69C105" ); //  Page: Wait List, Layout: Full Width, Site: Rock RMS
        }
    }
}
