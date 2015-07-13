    DECLARE @CategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '4A7D0D1F-E160-445E-9D29-AEBD140DA242' )
    IF @CategoryId IS NOT NULL
    BEGIN     

        INSERT INTO [SystemEmail] ([IsSystem], [Title], [Subject], [Body], [Guid], [CategoryId])
        VALUES
	        (0, 'Registration Confirmation', '{{ RegistrationInstance.Name }} Confirmation', '{{ ''Global'' | Attribute:''EmailHeader'' }}
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}
<p>
    The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
    {% if registrantCount > 1 %}have{% else %}has{% endif %} been registered for {{ RegistrationInstance.Name }} on {{ RegistrationInstance.StartDateTime | Date:''dddd, MMMM d, yyyy'' }}:
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>
    
        {{ registrant.PersonAlias.Person.FullName }}
        
        {% if registrant.Cost > 0 %}
            - {{ currencySymbol }}{{ registrant.Cost | Format''#,##0.00'' }}
        {% endif %}
        
        {% assign feeCount = registrant.Fees | Size %}
        {% if feeCount > 0 %}
            <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
            <ul>
            {% for fee in registrant.Fees %}
                <li>
                    {{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}
                    {% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format''#,##0.00'' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format''#,##0.00'' }}
                </li>
            {% endfor %}
            </ul>
        {% endif %}

    </li>
{% endfor %}
</ul>

{% if Registration.TotalCost > 0 %}
<p>
    Total Due: {{ currencySymbol }}{{ Registration.TotalCost | Format''#,##0.00'' }}<br/>
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format''#,##0.00'' }} on {{ payment.Transaction.TransactionDateTime| Date:''M/d/yyyy'' }} (Txn: {{ payment.Transaction.TransactionCode }})<br/>
    {% endfor %}
    Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format''#,##0.00'' }}<br/>
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format''#,##0.00'' }}
</p>
{% endif %}

{{ RegistrationInstance.AdditionalConfirmationDetails }}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}', '7B0F4F06-69BD-4CB4-BD04-8DA3779D5259', @CategoryId )

                
        INSERT INTO [SystemEmail] ([IsSystem], [Title], [Subject], [Body], [Guid], [CategoryId])
        VALUES
	        (0, 'Registration Notification', '{{ RegistrationInstance.Name }} Registration', '{{ ''Global'' | Attribute:''EmailHeader'' }}
{% assign registrantCount = Registration.Registrants | Size %}
<p>
    The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }} just registered 
    for {{ RegistrationInstance.Name }}{% if Registration.Group %} and have been added to the {{ Registration.Group.Name }} Group{% endif %}.
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>{{ registrant.PersonAlias.Person.FullName }}</li>
{% endfor %}
</ul>

<p></p>

{{ ''Global'' | Attribute:''EmailFooter'' }}', '158607D1-0772-4947-ADD6-EA31AB6ABC2F', @CategoryId )

                
END

