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

namespace Rock.Utility
{
    /// <summary>
    /// Contains the default Lava template strings for registration email
    /// and success text used by RegistrationTemplate. These are shared so
    /// that both the RegistrationTemplateDetail block and the SampleData
    /// block can reference them without cross-project dependencies.
    /// </summary>
    public static class RegistrationTemplateDefaults
    {
        /// <summary>
        /// The default Lava template for registration confirmation emails.
        /// </summary>
        public const string ConfirmationEmail = @"{{ 'Global' | Attribute:'EmailHeader' }}
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
				- {{ registrant.Cost | FormatAsCurrency }}
			{% endif %}

			{% assign feeCount = registrant.Fees | Size %}
			{% if feeCount > 0 %}
				<br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
				<ul>
				{% for fee in registrant.Fees %}
					<li>
                        {{ fee.RegistrationTemplateFee.Name }} {% if fee.RegistrationTemplateFee.FeeType == 'Multiple' %} - {{ fee.Option }} {% endif %}
						{% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ fee.Cost | FormatAsCurrency }}){% endif %}: {{ fee.TotalCost | FormatAsCurrency }}
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
    Total Cost: {{ Registration.TotalCost | FormatAsCurrency }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ Registration.DiscountedCost | FormatAsCurrency }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ payment.Amount | FormatAsCurrency }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }}
        <small>(Acct #: {{ payment.Transaction.FinancialPaymentDetail.AccountNumberMasked }}, Ref #: {{ payment.Transaction.TransactionCode }})</small><br>
    {% endfor %}

    {% assign paymentCount = Registration.Payments | Size %}

    {% if paymentCount > 1 %}
        Total Paid: {{ Registration.TotalPaid | FormatAsCurrency }}<br/>
    {% endif %}

    {% assign paymentPlan = Registration.PaymentPlanFinancialScheduledTransaction %}

    {% if paymentPlan and paymentPlan.IsActive %}
        Payment Plan: {{ paymentPlan.TotalAmount | FormatAsCurrency }} × {{ paymentPlan.NumberOfPayments }} ({{ paymentPlan.TransactionFrequencyValue | AsString }})<br>
    {% else %}
        Balance Due: {{ Registration.BalanceDue | FormatAsCurrency }}
    {% endif %}
</p>
{% endif %}

//- 16.4 fix
{% if registrantCount > 0 %}
    <p>
        {{ RegistrationInstance.AdditionalConfirmationDetails }}
    </p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}";

        /// <summary>
        /// The default Lava template for registration reminder emails.
        /// </summary>
        public const string ReminderEmail = @"{{ 'Global' | Attribute:'EmailHeader' }}
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
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        </li>
    {% endfor %}
    </ul>
{% endif %}

{% if Registration.BalanceDue > 0 %}
<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} has a remaining balance
    of {{ Registration.BalanceDue | FormatAsCurrency }}.
    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
    using our <a href='{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}'>
    online registration page</a>.
</p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}";

        /// <summary>
        /// The default Lava template for the registration success text
        /// shown to registrants after completing registration.
        /// </summary>
        public const string SuccessText = @"
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
                - {{ registrant.Cost | FormatAsCurrency }}
            {% endif %}

            {% assign feeCount = registrant.Fees | Size %}
            {% if feeCount > 0 %}
                <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
                <ul class='list-unstyled'>
                {% for fee in registrant.Fees %}
                    <li>
                        {{ fee.RegistrationTemplateFee.Name }} {% if fee.RegistrationTemplateFee.FeeType == 'Multiple' %} - {{ fee.Option }} {% endif %}
                        {% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ fee.Cost | FormatAsCurrency }}){% endif %}: {{ fee.TotalCost | FormatAsCurrency }}
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
        The following were added to the wait list:
    </p>

    <ul>
    {% for registrant in waitlist %}
        <li>
            <strong>{{ registrant.PersonAlias.Person.FullName }}</strong> - {{ registrant.Cost | FormatAsCurrency }}{% if registrant.Cost == 0 %} (not charged){% endif %} - <span class=""badge badge-warning"">Waiting List</span>
        </li>
    {% endfor %}
    </ul>
{% endif %}

{% if Registration.TotalCost > 0 %}
<p>
    Total Cost: {{ Registration.TotalCost | FormatAsCurrency }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ Registration.DiscountedCost | FormatAsCurrency }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ payment.Amount | FormatAsCurrency }} on {{ payment.Transaction.TransactionDateTime| Date:'M/d/yyyy' }}
        <small>(Acct #: {{ payment.Transaction.FinancialPaymentDetail.AccountNumberMasked }}, Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}

    {% assign paymentCount = Registration.Payments | Size %}

    {% if paymentCount > 1 %}
        Total Paid: {{ Registration.TotalPaid | FormatAsCurrency }}<br/>
    {% endif %}

    {% assign paymentPlan = Registration.PaymentPlanFinancialScheduledTransaction %}

    {% if paymentPlan and paymentPlan.IsActive %}
        Payment Plan: {{ paymentPlan.TotalAmount | FormatAsCurrency }} × {{ paymentPlan.NumberOfPayments }} ({{ paymentPlan.TransactionFrequencyValue | AsString }})
    {% else %}
        Balance Due: {{ Registration.BalanceDue | FormatAsCurrency }}
    {% endif %}
</p>
{% endif %}

<p>
    A confirmation email has been sent to {{ Registration.ConfirmationEmail }}. If you have any questions
    please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>";

        /// <summary>
        /// The default Lava template for registration payment reminder emails.
        /// </summary>
        public const string PaymentReminderEmail = @"{{ 'Global' | Attribute:'EmailHeader' }}
{% capture externalSite %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% endcapture %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Payment Reminder</h1>

<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} for {{ RegistrationInstance.Name }} has a remaining balance
    of {{ Registration.BalanceDue | FormatAsCurrency }}. The
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
    using our <a href='{{ externalSite }}Registration?RegistrationId={{ Registration.Id }}&rckipid={{ Registration.PersonAlias.Person | PersonTokenCreate }}'>
    online registration page</a>.
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ 'Global' | Attribute:'EmailFooter' }}";
    }
}
