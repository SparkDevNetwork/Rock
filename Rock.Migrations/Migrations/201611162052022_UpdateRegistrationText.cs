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
    public partial class UpdateRegistrationText : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Delete any of the attribute values that have similiar value as the default
            Sql( @"
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block' )
    DECLARE @BlockTypeId int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/Event/RegistrationTemplateDetail.ascx' )

    DELETE V
    FROM [Attribute] A
    INNER JOIN [AttributeValue] V 
	    ON V.[AttributeId] = A.[Id]
	    AND REPLACE(REPLACE(REPLACE(V.[Value], ' ', ''),CHAR(10),''),CHAR(13),'') = REPLACE(REPLACE(REPLACE(A.[DefaultValue], ' ', ''),CHAR(10),''),CHAR(13),'')
    WHERE A.[EntityTypeId] = @EntityTypeId
    AND A.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND A.[EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar)
" );

            // Delete the Wait lit transition email attribute value as it's new
            Sql( @"
    DELETE [AttributeValue]
    WHERE [AttributeId] IN ( SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099' )
" );

            // Temporarily update the existing text for any template who's value is same as existing default to '[DefaultValue]'
            Sql( @"
    UPDATE T SET 
	     [ConfirmationEmailTemplate] = CASE WHEN REPLACE(REPLACE(REPLACE(C.[DefaultValue], ' ', ''),CHAR(10),''),CHAR(13),'') = REPLACE(REPLACE(REPLACE(T.[ConfirmationEmailTemplate], ' ', ''),CHAR(10),''),CHAR(13),'') THEN '[DefaultValue]' ELSE T.[ConfirmationEmailTemplate] END
	    ,[ReminderEmailTemplate] = CASE WHEN REPLACE(REPLACE(REPLACE(R.[DefaultValue], ' ', ''),CHAR(10),''),CHAR(13),'') = REPLACE(REPLACE(REPLACE(T.[ReminderEmailTemplate], ' ', ''),CHAR(10),''),CHAR(13),'') THEN '[DefaultValue]' ELSE T.[ReminderEmailTemplate] END
	    ,[SuccessText] = CASE WHEN REPLACE(REPLACE(REPLACE(S.[DefaultValue], ' ', ''),CHAR(10),''),CHAR(13),'') = REPLACE(REPLACE(REPLACE(T.[SuccessText], ' ', ''),CHAR(10),''),CHAR(13),'') THEN '[DefaultValue]' ELSE T.[SuccessText] END
	    ,[PaymentReminderEmailTemplate] = CASE WHEN REPLACE(REPLACE(REPLACE(P.[DefaultValue], ' ', ''),CHAR(10),''),CHAR(13),'') = REPLACE(REPLACE(REPLACE(T.[PaymentReminderEmailTemplate], ' ', ''),CHAR(10),''),CHAR(13),'') THEN '[DefaultValue]' ELSE T.[PaymentReminderEmailTemplate] END
    FROM [RegistrationTemplate] T
    LEFT OUTER JOIN [Attribute] C ON C.[Guid] = 'EBD8EB51-5514-43B5-8AA6-E0A509D865E5'
    LEFT OUTER JOIN [Attribute] R ON R.[Guid] = '10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5'
    LEFT OUTER JOIN [Attribute] S ON S.[Guid] = 'B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD'
    LEFT OUTER JOIN [Attribute] P ON P.[Guid] = 'C8AB59C0-3074-418E-8493-2BCED16D5034'
" );
            // Update the default values
            RockMigrationHelper.UpdateBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Confirmation Email", "DefaultConfirmationEmail", "", "The default Confirmation Email Template value to use for a new template", 0, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Confirmation: {{ RegistrationInstance.Name }}</h1>

{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
{% assign registrantCount = registrants | Size %}
{% if registrantCount > 0 %}
	<p>
		The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if registrantCount > 1 %}have{% else %}has{% endif %} been registered for {{ RegistrationInstance.Name }}:
	</p>

	<ul>
	{% for registrant in registrants %}
		<li>
		
			<strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
			
			{% if registrant.Cost > 0 %}
				- {{ currencySymbol }}{{ registrant.Cost | Format:'#,##0.00' }}
			{% endif %}
			
			{% assign feeCount = registrant.Fees | Size %}
			{% if feeCount > 0 %}
				<br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
				<ul>
				{% for fee in registrant.Fees %}
					<li>
						{{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}
						{% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format:'#,##0.00' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format:'#,##0.00' }}
					</li>
				{% endfor %}
				</ul>
			{% endif %}

		</li>
	{% endfor %}
	</ul>
{% endif %}

{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
{% assign waitListCount = waitlist | Size %}
{% if waitListCount > 0 %}
    <p>
        The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if waitListCount > 1 %}have{% else %}has{% endif %} been added to the wait list for {{ RegistrationInstance.Name }}:
   </p>
    
    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}
	
{% if Registration.TotalCost > 0 %}
<p>
    Total Cost: {{ currencySymbol }}{{ Registration.TotalCost | Format:'#,##0.00' }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ currencySymbol }}{{ Registration.DiscountedCost | Format:'#,##0.00' }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format:'#,##0.00' }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }} 
        <small>(Acct #: {{ payment.Transaction.FinancialPaymentDetail.AccountNumberMasked }}, Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    
    {% assign paymentCount = Registration.Payments | Size %}
    
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format:'#,##0.00' }}<br/>
    {% endif %}
    
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}
</p>
{% endif %}

<p>
    {{ RegistrationInstance.AdditionalConfirmationDetails }}
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "EBD8EB51-5514-43B5-8AA6-E0A509D865E5" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Reminder Email", "DefaultReminderEmail", "", "The default Reminder Email Template value to use for a new template", 1, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Reminder</h1>

<p>
    {{ RegistrationInstance.AdditionalReminderDetails }}
</p>

{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
{% assign registrantCount = registrants | Size %}
{% if registrantCount > 0 %}
	<p>
		The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if registrantCount > 1 %}have{% else %}has{% endif %} been registered for {{ RegistrationInstance.Name }}:
	</p>

	<ul>
	{% for registrant in registrants %}
		<li>{{ registrant.PersonAlias.Person.FullName }}</li>
	{% endfor %}
	</ul>
{% endif %}

{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
{% assign waitListCount = waitlist | Size %}
{% if waitListCount > 0 %}
    <p>
        The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if waitListCount > 1 %}are{% else %}is{% endif %} still on the waiting list:
   </p>
    
    <ul>
    {% for registrant in waitlist %}
        <li>{{ registrant.PersonAlias.Person.FullName }}</li>
    {% endfor %}
    </ul>
{% endif %}

{% if Registration.BalanceDue > 0 %}
<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} has a remaining balance 
    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}.
    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
    using our <a href='{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
    online registration page</a>.
</p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Success Text", "DefaultSuccessText", "", "The success text default to use for a new template", 2, @"{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}

{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
{% assign registrantCount = registrants | Size %}
{% if registrantCount > 0 %}
    <p>
        You have successfully registered the following 
        {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
        for {{ RegistrationInstance.Name }}:
    </p>
    
    <ul>
    {% for registrant in registrants %}
        <li>
        
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
            
            {% if registrant.Cost > 0 %}
                - {{ currencySymbol }}{{ registrant.Cost | Format:'#,##0.00' }}
            {% endif %}
            
            {% assign feeCount = registrant.Fees | Size %}
            {% if feeCount > 0 %}
                <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
                <ul class='list-unstyled'>
                {% for fee in registrant.Fees %}
                    <li>
                        {{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}
                        {% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format:'#,##0.00' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format:'#,##0.00' }}
                    </li>
                {% endfor %}
                </ul>
            {% endif %}
            
        </li>
    {% endfor %}
    </ul>
{% endif %}

{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
{% assign waitListCount = waitlist | Size %}
{% if waitListCount > 0 %}
    <p>
        You have successfully added the following 
        {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
        to the waiting list for {{ RegistrationInstance.Name }}:
    </p>
    
    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}

{% if Registration.TotalCost > 0 %}
<p>
    Total Cost: {{ currencySymbol }}{{ Registration.TotalCost | Format:'#,##0.00' }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ currencySymbol }}{{ Registration.DiscountedCost | Format:'#,##0.00' }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format:'#,##0.00' }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }} 
        <small>(Acct #: {{ payment.Transaction.FinancialPaymentDetail.AccountNumberMasked }}, Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    {% assign paymentCount = Registration.Payments | Size %}
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format:'#,##0.00' }}<br/>
    {% endif %}
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}
</p>
{% endif %}

<p>
    A confirmation email has been sent to {{ Registration.ConfirmationEmail }}. If you have any questions 
    please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>", "B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Wait List Transition Email", "DefaultWaitListTransitionEmail", "", "The default Wait List Transition Email Template value to use for a new template", 3, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}

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
        <a href='{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}&StartAtBeginning=True'>
        online registration page</a> to complete the registration.</strong>
    </p>
{% endif %}


{% if Registration.BalanceDue > 0 %}
    <p>
        A balance of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }} remains on this regsitration. You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
        using our <a href='{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
        online registration page</a>.
    </p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>


{{ 'Global' | Attribute:'EmailFooter' }}", "E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Default Payment Reminder Email", "DefaultPaymentReminderEmail", "", "The default Payment Reminder Email Template value to use for a new template", 3, @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture currencySymbol %}{{ 'Global' | Attribute:'CurrencySymbol' }}{% endcapture %}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Payment Reminder</h1>

<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} for {{ RegistrationInstance.Name }} has a remaining balance 
    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:'#,##0.00' }}. The 
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | Downcase | Pluralize  }} for this 
    {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} are below.
</p>

{% assign registrants = Registration.Registrants | Where:'OnWaitList', false %}
{% assign registrantCount = registrants | Size %}
{% if registrantCount > 0 %}
	<ul>
	{% for registrant in registrants %}
		<li>{{ registrant.PersonAlias.Person.FullName }}</li>
	{% endfor %}
	</ul>
{% endif %}

{% assign waitlist = Registration.Registrants | Where:'OnWaitList', true %}
{% assign waitListCount = waitlist | Size %}
{% if waitListCount > 0 %}
    <p>
        The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
		{% if waitListCount > 1 %}are{% else %}is{% endif %} still on the wait list:
   </p>
    
    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}

<p>
    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
    using our <a href='{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person.UrlEncodedKey }}'>
    online registration page</a>.
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}", "C8AB59C0-3074-418E-8493-2BCED16D5034" );

            // Update the template's who have the '[DefaultValue]' to the new default value
            Sql( @"
    UPDATE T SET 
	     [ConfirmationEmailTemplate] = CASE WHEN T.[ConfirmationEmailTemplate] = '[DefaultValue]' THEN C.[DefaultValue] ELSE T.[ConfirmationEmailTemplate] END
	    ,[ReminderEmailTemplate] = CASE WHEN T.[ReminderEmailTemplate] = '[DefaultValue]' THEN R.[DefaultValue] ELSE T.[ReminderEmailTemplate] END
	    ,[SuccessText] = CASE WHEN T.[SuccessText] = '[DefaultValue]' THEN S.[DefaultValue] ELSE T.[SuccessText] END
	    ,[PaymentReminderEmailTemplate] = CASE WHEN T.[PaymentReminderEmailTemplate] = '[DefaultValue]' THEN P.[DefaultValue] ELSE T.[PaymentReminderEmailTemplate] END
	    ,[WaitListTransitionEmailTemplate] = W.[DefaultValue]
    FROM [RegistrationTemplate] T
    LEFT OUTER JOIN [Attribute] C ON C.[Guid] = 'EBD8EB51-5514-43B5-8AA6-E0A509D865E5'
    LEFT OUTER JOIN [Attribute] R ON R.[Guid] = '10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5'
    LEFT OUTER JOIN [Attribute] S ON S.[Guid] = 'B72F8C64-0AD5-4E9F-8DB0-00DE14E9BCCD'
    LEFT OUTER JOIN [Attribute] P ON P.[Guid] = 'C8AB59C0-3074-418E-8493-2BCED16D5034'
    LEFT OUTER JOIN [Attribute] W ON W.[Guid] = 'E50AC4C6-8C6C-46EC-85D0-0ED1E91EA099'
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
