DECLARE @PageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'F7CA6E0F-C319-47AB-9A6D-247C5716D846' )
DELETE [PageRoute] WHERE [PageId] = @PageId

UPDATE [Page] SET [BreadCrumbDisplayName] = 0 
WHERE [GUID] = 'F7CA6E0F-C319-47AB-9A6D-247C5716D846'

UPDATE [Page] SET 
    [InternalName] = 'Registration',
    [PageTitle] = 'Registration',
    [BrowserTitle] = 'Registration',
    [BreadCrumbDisplayName] = 1
WHERE [GUID] = 'FC81099A-2F98-4EBA-AC5A-8300B2FE46C4'

DECLARE @TemplateEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'A01E3E99-A8AD-4C6C-BAAC-98795738BA70' )
DECLARE @GeneralGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '8400497B-C52F-40AE-A529-3FCCB9587101' )
DECLARE @GeneralGroupTypeRoleId int = ( SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = 'A0BBF29D-AD9D-4D06-9E81-9DA080D53C10' )

DECLARE @CategoryId int
INSERT INTO [Category] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconCssClass], [Guid], [Order] )
VALUES ( 0, @TemplateEntityTypeId, '', '', 'General', 'fa fa-folder', '568530FB-1B92-4FB7-9974-CCA169E51782', 0 )
SET @CategoryId = SCOPE_IDENTITY()

INSERT INTO [Category] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconCssClass], [Guid], [Order] )
VALUES ( 0, @TemplateEntityTypeId, '', '', 'Connection', 'fa fa-folder', 'F402C51A-BD35-4FE5-92D9-D574F1E4F2EB', 0 )
SET @CategoryId = SCOPE_IDENTITY()

-- Baptism
DECLARE @RegistrationTemplateId int
INSERT INTO [RegistrationTemplate] ( [Name], [CategoryId], [GroupTypeId], [GroupMemberRoleId], [GroupMemberStatus], [FeeTerm], [RegistrantTerm],
	[RegistrationTerm], [DiscountCodeTerm], [ConfirmationEmailTemplate], [ReminderEmailTemplate], [Cost], [MinimumInitialPayment], [LoginRequired],
	[RegistrantsSameFamily], [SuccessTitle], [SuccessText], [AllowMultipleRegistrants], [MaxRegistrants], [IsActive], [Guid], [Notify],
	[ConfirmationFromName], [ConfirmationFromEmail], [ConfirmationSubject], [ReminderFromName], [ReminderFromEmail], [ReminderSubject] )
VALUES ( 'Baptism', @CategoryId, @GeneralGroupTypeId, @GeneralGroupTypeRoleId, 2, 'Additional Options', 'Registrant',
	'Registration', 'Discount Code', '
{{ ''Global'' | Attribute:''EmailHeader'' }}
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Confirmation: {{ RegistrationInstance.Name }}</h1>

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
        Paid {{ currencySymbol }}{{ payment.Amount | Format''#,##0.00'' }} on {{ payment.Transaction.TransactionDateTime| Date:''M/d/yyyy'' }} <small>(Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    
    {% assign paymentCount = Registration.Payments | Size %}
    
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format''#,##0.00'' }}<br/>
    {% endif %}
    
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format''#,##0.00'' }}
</p>
{% endif %}

{{ RegistrationInstance.AdditionalConfirmationDetails }}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}
', '', 0.00, 0.00, 0,
	2, 'Congratulations {{ Registration.FirstName }}', '
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}
<p>
    You have succesfully registered the following 
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
    for {{ RegistrationInstance.Name }}:
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>
    
        <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        
        {% if registrant.Cost > 0 %}
            - {{ currencySymbol }}{{ registrant.Cost | Format''#,##0.00'' }}
        {% endif %}
        
        {% assign feeCount = registrant.Fees | Size %}
        {% if feeCount > 0 %}
            <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
            <ul class=''list-unstyled''>
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
    Total Due: {{ currencySymbol }}{{ Registration.TotalCost | Format''''#,##0.00'''' }}<br/>
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format''''#,##0.00'''' }} on {{ payment.Transaction.TransactionDateTime| Date:''M/d/yyyy'' }} 
        <small>(Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    {% assign paymentCount = Registration.Payments | Size %}
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format''''#,##0.00'''' }}<br/>
    {% endif %}
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format''''#,##0.00'''' }}
</p>
{% endif %}

<p>
    A confirmation email has been sent to {{ Registration.ConfirmationEmail }}. If you have any questions 
    please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>
', 
	1, 0, 1, '5E0ADD57-B220-4ED0-975D-EEE2D0B14743', 7, 
	'{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}', '{{ RegistrationInstance.ContactEmail }}', '{{ RegistrationInstance.Name }} Confirmation', '{{ RegistrationInstance.ContactName }}', '{{ RegistrationInstance.ContactEmail }}', '{{ RegistrationInstance.Name }} Reminder' )
SET @RegistrationTemplateId = SCOPE_IDENTITY()

DECLARE @FormId int
INSERT INTO [RegistrationTemplateForm] ( [Name], [RegistrationTemplateId], [Order], [Guid] )
VALUES ( 'Default Form', @RegistrationTemplateId, 0, '4540C939-08CC-4A56-9914-9E4CC956C719' )
SET @FormId = SCOPE_IDENTITY()

INSERT INTO [RegistrationTemplateFormField] ( [RegistrationTemplateFormId], [FieldSource], [PersonFieldType], 
	[IsSharedValue], [ShowCurrentValue], [PreText], [PostText], [IsGridField], [IsRequired], [Order], [Guid] )
VALUES ( @FormId, 0, 0, 0, 0, '<div class=''row''>
    <div class=''col-md-6''>', '    </div>', 1, 1, 0, NEWID() )

INSERT INTO [RegistrationTemplateFormField] ( [RegistrationTemplateFormId], [FieldSource], [PersonFieldType], 
	[IsSharedValue], [ShowCurrentValue], [PreText], [PostText], [IsGridField], [IsRequired], [Order], [Guid] )
VALUES ( @FormId, 0, 1, 0, 0, '    <div class=''col-md-6''>', '    </div>
</div>', 1, 1, 1, NEWID() )

INSERT INTO [RegistrationTemplateFormField] ( [RegistrationTemplateFormId], [FieldSource], [PersonFieldType], 
	[IsSharedValue], [ShowCurrentValue], [PreText], [PostText], [IsGridField], [IsRequired], [Order], [Guid] )
VALUES ( @FormId, 0, 4, 0, 0, '', '', 0, 1, 2, NEWID() )

-- Starting Point
INSERT INTO [RegistrationTemplate] ( [Name], [CategoryId], [GroupTypeId], [GroupMemberRoleId], [GroupMemberStatus], [FeeTerm], [RegistrantTerm],
	[RegistrationTerm], [DiscountCodeTerm], [ConfirmationEmailTemplate], [ReminderEmailTemplate], [Cost], [MinimumInitialPayment], [LoginRequired],
	[RegistrantsSameFamily], [SuccessTitle], [SuccessText], [AllowMultipleRegistrants], [MaxRegistrants], [IsActive], [Guid], [Notify],
	[ConfirmationFromName], [ConfirmationFromEmail], [ConfirmationSubject], [ReminderFromName], [ReminderFromEmail], [ReminderSubject] )
VALUES ( 'Starting Point', @CategoryId, @GeneralGroupTypeId, @GeneralGroupTypeRoleId, 2, 'Additional Options', 'Registrant',
	'Registration', 'Discount Code', '
{{ ''Global'' | Attribute:''EmailHeader'' }}
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Confirmation: {{ RegistrationInstance.Name }}</h1>

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
        Paid {{ currencySymbol }}{{ payment.Amount | Format''#,##0.00'' }} on {{ payment.Transaction.TransactionDateTime| Date:''M/d/yyyy'' }} <small>(Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    
    {% assign paymentCount = Registration.Payments | Size %}
    
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format''#,##0.00'' }}<br/>
    {% endif %}
    
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format''#,##0.00'' }}
</p>
{% endif %}

{{ RegistrationInstance.AdditionalConfirmationDetails }}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}
', '', 0.00, 0.00, 0,
	2, 'Congratulations {{ Registration.FirstName }}', '
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}
<p>
    You have succesfully registered the following 
    {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
    for {{ RegistrationInstance.Name }}:
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>
    
        <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        
        {% if registrant.Cost > 0 %}
            - {{ currencySymbol }}{{ registrant.Cost | Format''#,##0.00'' }}
        {% endif %}
        
        {% assign feeCount = registrant.Fees | Size %}
        {% if feeCount > 0 %}
            <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
            <ul class=''list-unstyled''>
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
    Total Due: {{ currencySymbol }}{{ Registration.TotalCost | Format''''#,##0.00'''' }}<br/>
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format''''#,##0.00'''' }} on {{ payment.Transaction.TransactionDateTime| Date:''M/d/yyyy'' }} 
        <small>(Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    {% assign paymentCount = Registration.Payments | Size %}
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format''''#,##0.00'''' }}<br/>
    {% endif %}
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format''''#,##0.00'''' }}
</p>
{% endif %}

<p>
    A confirmation email has been sent to {{ Registration.ConfirmationEmail }}. If you have any questions 
    please contact {{ RegistrationInstance.ContactPersonAlias.Person.FullName }} at {{ RegistrationInstance.ContactEmail }}.
</p>
', 
	1, 0, 1, '9A1D2292-9FE5-45D8-90B7-2D20535E0479', 7, 
	'{{ RegistrationInstance.ContactPersonAlias.Person.FullName }}', '{{ RegistrationInstance.ContactEmail }}', '{{ RegistrationInstance.Name }} Confirmation', '{{ RegistrationInstance.ContactName }}', '{{ RegistrationInstance.ContactEmail }}', '{{ RegistrationInstance.Name }} Reminder' )
SET @RegistrationTemplateId = SCOPE_IDENTITY()

INSERT INTO [RegistrationTemplateForm] ( [Name], [RegistrationTemplateId], [Order], [Guid] )
VALUES ( 'Default Form', @RegistrationTemplateId, 0, '87C9F2BF-AA4F-466A-ADA5-F158FDD20EA2' )
SET @FormId = SCOPE_IDENTITY()

INSERT INTO [RegistrationTemplateFormField] ( [RegistrationTemplateFormId], [FieldSource], [PersonFieldType], 
	[IsSharedValue], [ShowCurrentValue], [PreText], [PostText], [IsGridField], [IsRequired], [Order], [Guid] )
VALUES ( @FormId, 0, 0, 0, 0, '<div class=''row''>
    <div class=''col-md-6''>', '    </div>', 1, 1, 0, NEWID() )

INSERT INTO [RegistrationTemplateFormField] ( [RegistrationTemplateFormId], [FieldSource], [PersonFieldType], 
	[IsSharedValue], [ShowCurrentValue], [PreText], [PostText], [IsGridField], [IsRequired], [Order], [Guid] )
VALUES ( @FormId, 0, 1, 0, 0, '    <div class=''col-md-6''>', '    </div>
</div>', 1, 1, 1, NEWID() )

INSERT INTO [RegistrationTemplateFormField] ( [RegistrationTemplateFormId], [FieldSource], [PersonFieldType], 
	[IsSharedValue], [ShowCurrentValue], [PreText], [PostText], [IsGridField], [IsRequired], [Order], [Guid] )
VALUES ( @FormId, 0, 4, 0, 0, '', '', 0, 1, 2, NEWID() )
