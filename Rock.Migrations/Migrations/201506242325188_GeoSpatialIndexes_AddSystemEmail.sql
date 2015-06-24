DECLARE @CategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '16C89E26-6F56-4C94-9AB8-925789390F21')
        IF @CategoryId IS NOT NULL
        BEGIN     
            INSERT INTO [SystemEmail]
	        ([IsSystem], [Title], [Subject], [Body], [Guid], [CategoryId])
            VALUES
	            (0, 'Pending Group Members Notification', 'New Pending Group Members | {{ GlobalAttribute.OrganizationName}}', '{{ ''Global'' | Attribute:''EmailHeader'' }}


<p>
    {{ Person.NickName }},
</p>

<p>
    We wanted to make you aware of additional individuals who have taken the next step to connect with 
    group. The individuals'' names and contact information can be found below. Our 
    goal is to contact new members within 24-48 hours of receiving this e-mail.
</p>

<ul>
{% for pendingIndividual in PendingIndividuals %}
    <li>
        <strong>{{ pendingIndividual.FullName }}</strong><br />
        {% assign mobilePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% assign homePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% assign homeAddress = pendingIndividual | Address:''Home'' %}
        
        {% if mobilePhone != empty %}
            Mobile Phone: -{{ mobilePhone }}-<br />
        {% endif %}
        
        {% if homePhone != empty %}
            Home Phone: {{ homePhone }}<br />
        {% endif %}
        
        {% if pendingIndividual.Email != '' %}
            {{ pendingIndividual.Email }}<br />
        {% endif %}
        
        {% if homeAddress != empty %}
            <p>
                Home Address <br />
                {{ homeAddress }}<br />
            </p>
        {% endif %}
        
    </li>
{% endfor %}
</ul>


<p>
    Once you have connected with these individuals, please mark them as active.
</p>

<p>
    Thank you for your on-going commitment to {{ ''Global'' | Attribute:''OrganizationName'' }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}', '18521B26-1C7D-E287-487D-97D176CA4986', @CategoryId )

                
END

