DECLARE @PersonAliasEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PersonAlias' )
IF @PersonAliasEntityTypeId IS NOT NULL
BEGIN

    DECLARE @EntityTypeId int 
    DECLARE @AttributeId int 
    DECLARE @EntityId int

    SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '17DFDE21-0C1E-426F-8516-4BBA9ED28385' )
	INSERT INTO [FollowingEventType] ( [Name], [Description], [EntityTypeId], [FollowedEntityTypeId], [IsActive], [SendOnWeekends], [IsNoticeRequired], [EntityNotificationFormatLava], [Guid] )
	VALUES 
	    ( 'Upcoming Anniversary', 'Person with an upcoming anniversary', @EntityTypeId, @PersonAliasEntityTypeId, 1, 0, 0, 
'<tr>
    <td>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=120&maxheight=120''/>
        {% endif %}
    </td>
    <td>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> has a 
        {{ Entity.Person.AnniversaryDate | DateDiff:Entity.Person.NextAnniversary,''Y'' }} year anniversary on 
        {{ Entity.Person.NextAnniversary | Date:''dddd, MMMM dd'' }} ({{ Entity.Person.NextAnniversary | HumanizeDateTime }})</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>', '5E81D053-974F-4841-A829-58410356F080' )
    SET @EntityId = SCOPE_IDENTITY()

    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '4E9F3547-5EF8-471E-A537-25846F26A00F' )
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	VALUES ( 0, @AttributeId, @EntityId, '5', NEWID() )

    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '73A36175-E82E-4315-92FD-A58C1499232E' )
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	VALUES ( 0, @AttributeId, @EntityId, '5', NEWID() )


    SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'A156E5A0-FEE8-4730-8AC7-B3239B35F9F2' )
	INSERT INTO [FollowingEventType] ( [Name], [Description], [EntityTypeId], [FollowedEntityTypeId], [IsActive], [SendOnWeekends], [IsNoticeRequired], [EntityNotificationFormatLava], [Guid] )
	VALUES 
	    ( 'Baptized', 'Person was recently baptized', @EntityTypeId, @PersonAliasEntityTypeId, 1, 0, 0, 
'<tr>
    <td>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=120&maxheight=120''/>
        {% endif %}
    </td>
    <td>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> was baptized on 
        {{ Entity.Person | Attribute:''BaptismDate'' | Date:''dddd, MMMM dd'' }} ({{ Entity.Person | Attribute:''BaptismDate'' | HumanizeDateTime }})</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>', '694D5110-EFF0-4E53-818F-A8AFEA268584' )
    SET @EntityId = SCOPE_IDENTITY()

    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'F68163EC-73E8-4785-A7E9-A11CAD464D05' )
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	VALUES ( 0, @AttributeId, @EntityId, '30', NEWID() )


    SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '4CDE3741-D284-4B32-9F8A-DFB63C600594' )
	INSERT INTO [FollowingEventType] ( [Name], [Description], [EntityTypeId], [FollowedEntityTypeId], [IsActive], [SendOnWeekends], [IsNoticeRequired], [EntityNotificationFormatLava], [Guid] )
	VALUES 
	    ( 'Began Serving', 'Person recently began serving', @EntityTypeId, @PersonAliasEntityTypeId, 1, 0, 0, 
'<tr>
    <td>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=120&maxheight=120''/>
        {% endif %}
    </td>
    <td>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> recently 
        began serving.</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>', '8F1476AD-CF60-4253-AC10-2094C6863D96' )
    SET @EntityId = SCOPE_IDENTITY()

    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'BC421FBC-3D0E-4C87-A533-EF678D156516' )
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	VALUES ( 0, @AttributeId, @EntityId, '2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4', NEWID() )

    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '3C024CDE-3481-410E-8392-566B4F78CC4E' )
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	VALUES ( 0, @AttributeId, @EntityId, '30', NEWID() )

	INSERT INTO [FollowingEventType] ( [Name], [Description], [EntityTypeId], [FollowedEntityTypeId], [IsActive], [SendOnWeekends], [IsNoticeRequired], [EntityNotificationFormatLava], [Guid] )
	VALUES 
	    ( 'Joined Small Group', 'Person joined a small group', @EntityTypeId, @PersonAliasEntityTypeId, 1, 0, 0, 
'<tr>
    <td>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=120&maxheight=120''/>
        {% endif %}
    </td>
    <td>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> 
        recently joined a small group.</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>', 'CDFB247B-B868-49BD-A080-0B2E8679DA4C' )
    SET @EntityId = SCOPE_IDENTITY()

    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'BC421FBC-3D0E-4C87-A533-EF678D156516' )
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	VALUES ( 0, @AttributeId, @EntityId, '50fcfb30-f51a-49df-86f4-2b176ea1820b', NEWID() )

    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '3C024CDE-3481-410E-8392-566B4F78CC4E' )
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	VALUES ( 0, @AttributeId, @EntityId, '30', NEWID() )

    -- Suggestion Types
    SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '20AC7F2A-D42F-438D-93D7-46E3C6769B8F' )
    INSERT INTO [FollowingSuggestionType] ( [Name], [Description], [ReasonNote], [ReminderDays], [EntityTypeId], [IsActive], [EntityNotificationFormatLava], [Guid] )
	VALUES 
	    ( 'Fellow Staff Members', 'People in the same ''staff'' group', 'Staff Member', 30, @EntityTypeId, 1, '', 'FC72D537-9C0C-40E3-884A-9FC5AA0AAF90' ),
	    ( 'Fellow Small Group Members', 'People in the same small group', 'In Small Group', 30, @EntityTypeId, 1, '', '9AB331F3-B8EC-47F6-8A6C-80F7C0E0D1BB' ),
	    ( 'Known Relationship', 'People that have a known relationship to follower', 'Known Relationship', 30, @EntityTypeId, 1, '', 'DFB3459C-2022-4A4F-A968-D8FFFF150DAB' ),
	    ( 'Serving Team Member', 'People who are in a serving group where follower is the leader', 'On Serving Team', 30, @EntityTypeId, 1, '', 'FC861A95-5837-4AC9-BBEB-82AC63971639' )

    UPDATE [FollowingSuggestionType] SET [EntityNotificationFormatLava] =
'<tr>
    <td>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=120&maxheight=120''/>
        {% endif %}
    </td>
    <td>
        <strong>{{ Entity.Person.FullName }}</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}

        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
        
    </td>
</tr>'

    -- Group Type Attribute Values
    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'AB754108-EB85-422B-A898-90C47495174A' )
    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	SELECT 0, @AttributeId, [Id], 
        CASE [Guid] 
            WHEN '8641F468-272B-4617-91ED-AB312D0F273C' THEN '790e3215-3b10-442b-af69-616c0dcb998e'
            WHEN 'FC72D537-9C0C-40E3-884A-9FC5AA0AAF90' THEN 'aece949f-704c-483e-a4fb-93d5e4720c4c'
            WHEN '9AB331F3-B8EC-47F6-8A6C-80F7C0E0D1BB' THEN '50fcfb30-f51a-49df-86f4-2b176ea1820b'
            WHEN 'DFB3459C-2022-4A4F-A968-D8FFFF150DAB' THEN 'e0c5a0e2-b7b3-4ef4-820d-bbf7f9a374ef'
            WHEN 'FC861A95-5837-4AC9-BBEB-82AC63971639' THEN '2c42b2d4-1c5f-4ad5-a9ad-08631b872ac4'
            ELSE '' END, NEWID()
	FROM [FollowingSuggestionType] 

    -- Security Role attribute value
    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'FC85D434-8AFF-4302-879B-0AF4AF7D6349' )
    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	SELECT 0, @AttributeId, [Id], '2c112948-ff4c-46e7-981a-0257681eadf4', NEWID()
	FROM [FollowingSuggestionType] WHERE [Guid] = 'FC72D537-9C0C-40E3-884A-9FC5AA0AAF90'

    -- Known Relationship owner role
    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '4C538BAC-943D-4DBF-BB09-1842C2E40515' )
    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId
	INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
	SELECT 0, @AttributeId, [Id], '7bc6c12e-0cd1-4dfd-8d5b-1b35ae714c42', NEWID()
	FROM [FollowingSuggestionType] WHERE [Guid] = 'DFB3459C-2022-4A4F-A968-D8FFFF150DAB'

END

-- ** Migration Rollups **

-- JE: Update Registration Reminder Emails
UPDATE [RegistrationTemplate]
SET [ReminderEmailTemplate] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
{% capture currencySymbol %}{{ ''Global'' | Attribute:''CurrencySymbol'' }}{% endcapture %}
{% capture externalSite %}{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}{% endcapture %}
{% assign registrantCount = Registration.Registrants | Size %}

<h1>{{ RegistrationInstance.RegistrationTemplate.RegistrationTerm }} Reminder</h1>

<p>
    {{ RegistrationInstance.AdditionalReminderDetails }}
</p>

<p>
    The following {{ RegistrationInstance.RegistrationTemplate.RegistrantTerm | PluralizeForQuantity:registrantCount | Downcase }}
    {% if registrantCount > 1 %}have{% else %}has{% endif %} been registered:
</p>

<ul>
{% for registrant in Registration.Registrants %}
    <li>{{ registrant.PersonAlias.Person.FullName }}</li>
{% endfor %}
</ul>


{% if Registration.BalanceDue > 0 %}
<p>
    This {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase  }} has a remaining balance 
    of {{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}.
    You can complete the payment for this {{ RegistrationInstance.RegistrationTemplate.RegistrationTerm | Downcase }}
    using our <a href="{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}">
    online registration page</a>.
</p>
{% endif %}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'

-- JE: Update Registration Confirmation Emails
UPDATE [RegistrationTemplate]
SET [ConfirmationEmailTemplate] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
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
        <strong>{{ registrant.PersonAlias.Person.FullName }}</strong>
        
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
        Paid {{ currencySymbol }}{{ payment.Amount | Format:''#,##0.00'' }} on {{ payment.Transaction.TransactionDateTime| Date:''M/d/yyyy'' }} 
        <small>(Acct #: {{ payment.Transaction.FinancialPaymentDetail.AccountNumberMasked }}, Ref #: {{ payment.Transaction.TransactionCode }})</small><br/>
    {% endfor %}
    
    {% assign paymentCount = Registration.Payments | Size %}
    
    {% if paymentCount > 1 %}
        Total Paid: {{ currencySymbol }}{{ Registration.TotalPaid | Format:''#,##0.00'' }}<br/>
    {% endif %}
    
    Balance Due: {{ currencySymbol }}{{ Registration.BalanceDue | Format:''#,##0.00'' }}
</p>
{% endif %}

{{ RegistrationInstance.AdditionalConfirmationDetails }}

<p>
    If you have any questions please contact {{ RegistrationInstance.ContactName }} at {{ RegistrationInstance.ContactEmail }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
