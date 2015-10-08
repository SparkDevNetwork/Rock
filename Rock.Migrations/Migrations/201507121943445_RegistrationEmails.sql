    DECLARE @CategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '4A7D0D1F-E160-445E-9D29-AEBD140DA242' )
    IF @CategoryId IS NOT NULL
    BEGIN     

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

