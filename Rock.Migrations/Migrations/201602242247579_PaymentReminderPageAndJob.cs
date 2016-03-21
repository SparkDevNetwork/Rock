// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class PaymentReminderPageAndJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "844DC54B-DAEC-47B3-A63A-712DD6D57793", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Payment Reminders", "", "2828BBCF-B3FC-4707-B063-086748853978", "fa fa-bell-o" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Registration Instance Send Payment Reminder", "Sends payment reminders for paid registrations that have a remaining balance.", "~/Blocks/Event/RegistrationInstanceSendPaymentReminder.ascx", "Event", "ED56CD0A-0A8D-4758-A689-55B7BEC1B589" );
                        
            // Add Block to Page: Payment Reminders, Site: Rock RMS
            RockMigrationHelper.AddBlock( "2828BBCF-B3FC-4707-B063-086748853978", "", "ED56CD0A-0A8D-4758-A689-55B7BEC1B589", "Registration Instance Send Payment Reminder", "Main", "", "", 0, "048BD6C6-E19C-43BE-8DFD-B0A4FB319594" );
            
            // Attrib for BlockType: Registration Template Detail:Default Payment Reminder Email
            RockMigrationHelper.AddBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Payment Reminder Email", "DefaultPaymentReminderEmail", "", "The default Payment Reminder Email Template value to use for a new template", 3, @"{{ 'Global' | Attribute:'EmailHeader' }}
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
", "C8AB59C0-3074-418E-8493-2BCED16D5034" );
            // Attrib for BlockType: Registration Instance Detail:Payment Reminder Page
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Payment Reminder Page", "PaymentReminderPage", "", "The page for manually sending payment reminders.", 7, @"", "DC887E6A-8ED8-4CC7-B20E-16302DC08A10" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Payment Reminder Page Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "DC887E6A-8ED8-4CC7-B20E-16302DC08A10", @"2828bbcf-b3fc-4707-b063-086748853978" );

            Sql( @"  INSERT INTO [ServiceJob]
  ([IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [Guid], [NotificationStatus])
  VALUES
  (0,1,'Event Payment Reminders','This job sends payment reminders to registration contacts with an active balance. For the reminder to be sent the registration template must have a ''Payment Reminder Time Span'' configured. Also emails will not be sent to registrations where the instance close date is past the job''s ''Cut-off Date'' setting.','Rock.Jobs.SendRegistrationPaymentReminders','0 0 9 ? * MON-FRI *','8BB88371-F8B5-5F8C-4EF0-4941CC550B15', 1)" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Registration Instance Detail:Payment Reminder Page
            RockMigrationHelper.DeleteAttribute( "DC887E6A-8ED8-4CC7-B20E-16302DC08A10" );
            // Attrib for BlockType: Registration Template Detail:Default Payment Reminder Email
            RockMigrationHelper.DeleteAttribute( "C8AB59C0-3074-418E-8493-2BCED16D5034" );
            // Remove Block: Registration Instance Send Payment Reminder, from Page: Payment Reminders, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "048BD6C6-E19C-43BE-8DFD-B0A4FB319594" );
            RockMigrationHelper.DeleteBlockType( "ED56CD0A-0A8D-4758-A689-55B7BEC1B589" ); // Registration Instance Send Payment Reminder
            RockMigrationHelper.DeletePage( "2828BBCF-B3FC-4707-B063-086748853978" ); //  Page: Payment Reminders, Layout: Full Width, Site: Rock RMS

            Sql( @"DELETE FROM [ServiceJob]
                        WHERE [Guid] = '8BB88371-F8B5-5F8C-4EF0-4941CC550B15'" );
        }
    }
}
