-----------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------
-- Correct Thank you string for Confirm Account email
UPDATE [SystemEmail]
SET [Body] = 
'{{ ''Global'' | Attribute:''EmailHeader'' }}

{{ Person.FirstName }},<br/><br/>

Thank you for creating an account at {{ ''Global'' | Attribute:''OrganizationName'' }}. Please <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=confirm''>confirm</a> that you are the owner of this email address.<br/><br/>If you did not create this account, you can <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete''>Delete It</a>.<br/><br/>If the above links do not work, you can also go to {{ ConfirmAccountUrl }} and enter the following confirmation code:<br/>{{ User.ConfirmationCode }}<br/><br/>

Thank you,<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}  

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [Guid] = '17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
AND [Body] = 
'{{ ''Global'' | Attribute:''EmailHeader'' }}

{{ Person.FirstName }},<br/><br/>

Thank-you for creating an account at {{ ''Global'' | Attribute:''OrganizationName'' }}. Please <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=confirm''>confirm</a> that you are the owner of this email address.<br/><br/>If you did not create this account, you can <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete''>Delete It</a>.<br/><br/>If the above links do not work, you can also go to {{ ConfirmAccountUrl }} and enter the following confirmation code:<br/>{{ User.ConfirmationCode }}<br/><br/>

Thank-you,<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}  

{{ ''Global'' | Attribute:''EmailFooter'' }}'
-----------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------
-- Correct Thank you string for Account Created email
UPDATE [SystemEmail]
SET [Body] = 
'{{ ''Global'' | Attribute:''EmailHeader'' }}

{{ Person.FirstName }},<br/><br/>

Thank you for creating a new account at {{ ''Global'' | Attribute:''OrganizationName'' }}.  Your ''{{ User.UserName }}'' username is now active and can be used to login to our site and access your information.<br/><br/>  If you did not create this account you can <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete''>Delete it here</a><br/><br/>  Thanks.
Thank you,<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}  

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [Guid] = '84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
AND [Body] = 
'{{ ''Global'' | Attribute:''EmailHeader'' }}

{{ Person.FirstName }},<br/><br/>

Thank-you for creating a new account at {{ ''Global'' | Attribute:''OrganizationName'' }}.  Your ''{{ User.UserName }}'' username is now active and can be used to login to our site and access your information.<br/><br/>  If you did not create this account you can <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete''>Delete it here</a><br/><br/>  Thanks.
Thank-you,<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}  

{{ ''Global'' | Attribute:''EmailFooter'' }}'
-----------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------
-- Correct Thank you string for "Pledge Confirmation" email
UPDATE [SystemEmail]
SET [Body] = 
'
{{ ''Global'' | Attribute:''EmailHeader'' }}

{{ Person.FirstName }},<br/><br/>

Thank you for your commitment to our {{ Account.Name }} account. Your financial gifts help us to continue to reach our mission.

Commitment Amount: ${{ FinancialPledge.TotalAmount }}

If you have any questions, please contact {{ ''Global'' | Attribute:''OrganizationEmail'' }}

Thank you,<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}  

{{ ''Global'' | Attribute:''EmailFooter'' }}
'
WHERE [Guid] = '73E8D035-61BB-495A-A87F-39007B98834C'
AND [Body] = 
'
{{ ''Global'' | Attribute:''EmailHeader'' }}

{{ Person.FirstName }},<br/><br/>

Thank you for your commitment to our {{ Account.Name }} account. Your financial gifts help us to continue to reach our mission.

Commitment Amount: ${{ FinancialPledge.TotalAmount }}

If you have any questions, please contact {{ ''Global'' | Attribute:''OrganizationEmail'' }}

Thank-you,<br/>
{{ ''Global'' | Attribute:''OrganizationName'' }}  

{{ ''Global'' | Attribute:''EmailFooter'' }}
'
-----------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------
-- Correct thank you string for Group Attendance Reminder email
UPDATE [SystemEmail]
SET [Body] = 
'
{% capture today %}{{ ''Now'' | Date:''yyyyMMdd'' }}{% endcapture %}
{% capture occurrenceDate %}{{ Occurrence | Date:''yyyyMMdd'' }}{% endcapture %}
{% capture attendanceLink %}{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}page/368?{{ Person.ImpersonationParameter }}&GroupId={{ Group.Id }}&Occurrence={{ Occurrence | Date:''yyyy-MM-ddTHH\%3amm\%3ass'' }}{% endcapture %}
{{ ''Global'' | Attribute:''EmailHeader'' }}

{{ Person.NickName }},<br/>
<br/>
Please remember to enter attendance for your group meeting {% if today == occurrenceDate %}today{% else %}on {{ Occurrence | Date:''dddd'' }}{% endif %}.<br/>
<br/>
Thank you!<br/>
<br/>
<table align="left" style="width: 29%; min-width: 190px; margin-bottom: 12px;" cellpadding="0" cellspacing="0">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word" href="{{ attendanceLink }}" style="height:38px;v-text-anchor:middle;width:175px;" arcsize="11%" strokecolor="#e76812" fillcolor="#ee7624">
			<w:anchorlock/>
			<center style="color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;">Enter Attendance</center>
		  </v:roundrect>
		<![endif]--><a href="{{ attendanceLink }}"
		style="background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;">Enter Attendance</a></div>

	</td>
 </tr>
</table>

{{ ''Global'' | Attribute:''EmailFooter'' }}
'
WHERE [Guid] = 'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
AND [Body] = 
'
{% capture today %}{{ ''Now'' | Date:''yyyyMMdd'' }}{% endcapture %}
{% capture occurrenceDate %}{{ Occurrence | Date:''yyyyMMdd'' }}{% endcapture %}
{% capture attendanceLink %}{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}page/368?{{ Person.ImpersonationParameter }}&GroupId={{ Group.Id }}&Occurrence={{ Occurrence | Date:''yyyy-MM-ddTHH\%3amm\%3ass'' }}{% endcapture %}
{{ ''Global'' | Attribute:''EmailHeader'' }}

{{ Person.NickName }},<br/>
<br/>
Please remember to enter attendance for your group meeting {% if today == occurrenceDate %}today{% else %}on {{ Occurrence | Date:''dddd'' }}{% endif %}.<br/>
<br/>
Thank-you!<br/>
<br/>
<table align="left" style="width: 29%; min-width: 190px; margin-bottom: 12px;" cellpadding="0" cellspacing="0">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v="urn:schemas-microsoft-com:vml" xmlns:w="urn:schemas-microsoft-com:office:word" href="{{ attendanceLink }}" style="height:38px;v-text-anchor:middle;width:175px;" arcsize="11%" strokecolor="#e76812" fillcolor="#ee7624">
			<w:anchorlock/>
			<center style="color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;">Enter Attendance</center>
		  </v:roundrect>
		<![endif]--><a href="{{ attendanceLink }}"
		style="background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;">Enter Attendance</a></div>

	</td>
 </tr>
</table>

{{ ''Global'' | Attribute:''EmailFooter'' }}
'
-----------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------
-- Correct ongoing string for "Pending Group Members Notification" email
UPDATE [SystemEmail]
SET [Body] = 
'{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    We wanted to make you aware of additional individuals who have taken the next step to connect with 
    group. The individuals'' names and contact information can be found below. Our 
    goal is to contact new members within 24-48 hours of receiving this e-mail.
</p>

<table cellpadding="25">
{% for pendingIndividual in PendingIndividuals %}
    <tr><td>
        <strong>{{ pendingIndividual.FullName }}</strong><br />
        {% assign mobilePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% assign homePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% assign homeAddress = pendingIndividual | Address:''Home'' %}
        
        {% if mobilePhone != empty %}
            Mobile Phone: {{ mobilePhone }}<br />
        {% endif %}
        
        {% if homePhone != empty %}
            Home Phone: {{ homePhone }}<br />
        {% endif %}
        
        {% if pendingIndividual.Email != empty %}
            {{ pendingIndividual.Email }}<br />
        {% endif %}
        
        <p>
        {% if homeAddress != empty %}
            Home Address <br />
            {{ homeAddress }}
        {% endif %}
        </p>
        
    </td></tr>
{% endfor %}
</table>


<p>
    Once you have connected with these individuals, please mark them as active.
</p>

<p>
    Thank you for your ongoing commitment to {{ ''Global'' | Attribute:''OrganizationName'' }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [Guid] = '18521B26-1C7D-E287-487D-97D176CA4986'
AND [Body] = 
'{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    We wanted to make you aware of additional individuals who have taken the next step to connect with 
    group. The individuals'' names and contact information can be found below. Our 
    goal is to contact new members within 24-48 hours of receiving this e-mail.
</p>

<table cellpadding="25">
{% for pendingIndividual in PendingIndividuals %}
    <tr><td>
        <strong>{{ pendingIndividual.FullName }}</strong><br />
        {% assign mobilePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 136 | Select:''NumberFormatted'' %}
        {% assign homePhone = pendingIndividual.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% assign homeAddress = pendingIndividual | Address:''Home'' %}
        
        {% if mobilePhone != empty %}
            Mobile Phone: {{ mobilePhone }}<br />
        {% endif %}
        
        {% if homePhone != empty %}
            Home Phone: {{ homePhone }}<br />
        {% endif %}
        
        {% if pendingIndividual.Email != empty %}
            {{ pendingIndividual.Email }}<br />
        {% endif %}
        
        <p>
        {% if homeAddress != empty %}
            Home Address <br />
            {{ homeAddress }}
        {% endif %}
        </p>
        
    </td></tr>
{% endfor %}
</table>


<p>
    Once you have connected with these individuals, please mark them as active.
</p>

<p>
    Thank you for your on-going commitment to {{ ''Global'' | Attribute:''OrganizationName'' }}.
</p>

{{ ''Global'' | Attribute:''EmailFooter'' }}'
-----------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------
-- Correct thank you string for "Email Preference:Success Text" attribute
UPDATE [Attribute]
SET [DefaultValue] =  '<h4>Thank You</h4>We have saved your email preference.}'
WHERE [Guid] = '46309218-6CDF-427D-BE45-B3DE6FAC1FE1'
AND [DefaultValue] =  '<h4>Thank-You</h4>We have saved your email preference.'
-----------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------
-- Correct thank you string for "Email Preference: Success Text" AttributeValue
UPDATE [AttributeValue]
SET [Value] = '<h4>Thank You</h4>We have saved your email preference.'
WHERE AttributeId = (SELECT Id FROM [Attribute] WHERE [Guid] = '46309218-6CDF-427D-BE45-B3DE6FAC1FE1' )
	AND [Value] = '<h4>Thank-You</h4>We have saved your email preference.'
-----------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------
