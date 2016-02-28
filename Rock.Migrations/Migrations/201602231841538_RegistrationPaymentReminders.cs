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
    public partial class RegistrationPaymentReminders : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Registration", "LastPaymentReminderDateTime", c => c.DateTime());
            AddColumn("dbo.RegistrationTemplate", "PaymentReminderFromName", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "PaymentReminderFromEmail", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "PaymentReminderSubject", c => c.String(maxLength: 200));
            AddColumn("dbo.RegistrationTemplate", "PaymentReminderEmailTemplate", c => c.String());
            AddColumn("dbo.RegistrationTemplate", "PaymentReminderTimeSpan", c => c.Int());

            // update current registration templates with payment reminder stock templates
            Sql( @" UPDATE [RegistrationTemplate]
	SET [PaymentReminderFromName] = '{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}'
		, [PaymentReminderFromEmail] = '{{ RegistrationInstance.ContactEmail }}'
		, [PaymentReminderSubject] = '{{ RegistrationInstance.Name }} Payment Reminder'
		, [PaymentReminderEmailTemplate] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% capture externalSite %}{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Payment Reminder</h1>

<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} for {{ RegistrationInstance.Name }} has a remaining balance 
    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}. The 
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
    using our <a href=''{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}''>
    online registration page</a>.
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [PaymentReminderEmailTemplate] is null AND [PaymentReminderFromEmail] is null AND [PaymentReminderFromName] is null AND [PaymentReminderSubject] is null" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.RegistrationTemplate", "PaymentReminderTimeSpan");
            DropColumn("dbo.RegistrationTemplate", "PaymentReminderEmailTemplate");
            DropColumn("dbo.RegistrationTemplate", "PaymentReminderSubject");
            DropColumn("dbo.RegistrationTemplate", "PaymentReminderFromEmail");
            DropColumn("dbo.RegistrationTemplate", "PaymentReminderFromName");
            DropColumn("dbo.Registration", "LastPaymentReminderDateTime");
        }
    }
}
