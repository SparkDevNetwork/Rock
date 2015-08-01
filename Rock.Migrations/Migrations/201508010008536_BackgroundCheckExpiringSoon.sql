-- Create root DataViewFilter for DataView: Background check about to expire
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'F924B369-FF1B-4254-AEB0-48BF89647205') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = null,
        @DataViewFilterEntityTypeId int = null

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','F924B369-FF1B-4254-AEB0-48BF89647205')
END
go

-- Create BackgroundChecked=True DataViewFilter for DataView: Background check about to expire
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'FADD959A-CAAE-416C-BF8E-B6037588C9CB') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'F924B369-FF1B-4254-AEB0-48BF89647205'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  "BackgroundChecked",
  "True"
]','FADD959A-CAAE-416C-BF8E-B6037588C9CB')
END
go

-- Create (60 days before the 3 yr expiration) DataViewFilter for DataView: Background check about to expire 
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '5F1DDF68-2EF1-4C25-8EDB-14895CE9C130') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'F924B369-FF1B-4254-AEB0-48BF89647205'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  "BackgroundCheckDate",
  "512",
  "CURRENT:-1035"
]','5F1DDF68-2EF1-4C25-8EDB-14895CE9C130')
END
go

-- Create DataViewFilter for DataView: Background check about to expire
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '4F580834-3233-4BCA-A360-9D5B1D4E7234') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'F924B369-FF1B-4254-AEB0-48BF89647205'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  "BackgroundCheckResult",
  "Pass"
]','4F580834-3233-4BCA-A360-9D5B1D4E7234')
END
go

--Create DataView: Background check about to expire
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'C7E122DE-3F46-4C39-8934-9B74913BDBD4') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'E62709E3-0060-4778-AA34-4B0FD9F6DF2E'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = 'F924B369-FF1B-4254-AEB0-48BF89647205'),
    @transformEntityTypeId  int = null

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Background check about to expire','Returns people that have been background checked within the last three years, but will be expiring soon',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'C7E122DE-3F46-4C39-8934-9B74913BDBD4')
END
go

--Update 'Background check is still Valid' to be 3 years (1095 days)
UPDATE DataViewFilter SET 
[Selection] = '[
  "BackgroundCheckDate",
  "256",
  "CURRENT:-1095\tAll||||"
]' where [Guid] = '256E15E6-9D9A-4539-8BE1-0B0F68BD2342'


-- 'Background Check Group Requirement' description, warning dataview, and warning label
UPDATE GroupRequirementType set 
[Description] = 'Returns people that have been background checked within the last three years',
[WarningLabel] = 'Background Check Expiring Soon',
[WarningDataViewId] = (select top 1 id From DataView where [Guid] = 'C7E122DE-3F46-4C39-8934-9B74913BDBD4') where [Guid] = '1C21C346-A861-4A9A-BD6D-BAA7D92419D5'


-- Migration Rollups..

-- JE: Update registration default emails
UPDATE [Attribute]
SET [DefaultValue] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Confirmation: {{ RegistrationInstance.Name }}</h1>

<p>
    The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
    {% if registrantCount > 1 %}have{% else %}has{% endif %} been registered for {{ RegistrationInstance.Name }}:
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>
    
        {{ registrant.PersonAlias.Person.FullName }}
        
        {% if registrant.Cost > 0 %}
            - {{ currencySymbol }}{{ registrant.Cost | Format:''#,##0.00'' }}
        {% endif %}
        
        {% assign feeCount = registrant.Fees | Size %}
        {% if feeCount > 0 %}
            <br/>{{ RegistrationInstance.RegistrationTemplate.FeeTerm | PluralizeForQuantity:registrantCount }}:
            <ul>
            {% for fee in registrant.Fees %}
                <li>
                    {{ fee.RegistrationTemplateFee.Name }} {{ fee.Option }}
                    {% if fee.Quantity > 1 %} ({{ fee.Quantity }} @ {{ currencySymbol }}{{ fee.Cost | Format:''#,##0.00'' }}){% endif %}: {{ currencySymbol }}{{ fee.TotalCost | Format:''#,##0.00'' }}
                </li>
            {% endfor %}
            </ul>
        {% endif %}

    </li>
{% endfor %}
</ul>

{% if Registration.TotalCost > 0 %}
<p>
    Total Cost: {{ currencySymbol }}{{ Registration.TotalCost | Format:''#,##0.00'' }}<br/>
    {% if Registration.DiscountedCost != Registration.TotalCost %}
        Discounted Cost: {{ currencySymbol }}{{ Registration.DiscountedCost | Format:''#,##0.00'' }}<br/>
    {% endif %}
    {% for payment in Registration.Payments %}
        Paid {{ currencySymbol }}{{ payment.Amount | Format:''#,##0.00'' }} on {{ payment.Transaction.TransactionDateTime| Date:''M/d/yyyy'' }} <small>(Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    
    {% assign paymentCount = Registration.Payments | Size %}
    
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format:''#,##0.00'' }}<br/>
    {% endif %}
    
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}
</p>
{% endif %}

<p>
    {{ RegistrationInstance.AdditionalConfirmationDetails }}
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [Guid] = 'EBD8EB51-5514-43B5-8AA6-E0A509D865E5'


UPDATE [Attribute]
SET [DefaultValue] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Registration.Registrants | Map:''NickName'' | Join:'', '' | ReplaceLast:'','','' and'' }},
</p>

<p>
    Just a reminder that you are registered for {{ RegistrationInstance.Name }}.
</p>

<p>
    {{ RegistrationInstance.AdditionalReminderDetails }}
</p>

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [Guid] = '10FED7FA-8E42-4A28-B13F-0DC65D1F7BE5'