-- Update DefaultValue for Email Medium's Unsubscribe HTML
UPDATE Attribute
SET DefaultValue = '
<p style=''float: right;''>
    <small><a href=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Unsubscribe/{{ Person | PersonTokenCreate:43200,3 }}?CommunicationId={{ Communication.Id }}''>Unsubscribe</a></small>
</p>
'
WHERE [Guid] = '2942EFCB-9BCF-4A16-9A78-D6149E2EAAD3'
	AND DefaultValue = '
<p style=''float: right;''>
    <small><a href=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Unsubscribe/{{ Person.PrimaryAlias.UrlEncodedKey }}''>Unsubscribe</a></small>
</p>
'

-- Update Value for Email Medium's Unsubscribe HTML
UPDATE AttributeValue 
SET [Value] = '
<p style=''float: right;''>
    <small><a href=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Unsubscribe/{{ Person | PersonTokenCreate:43200,3 }}?CommunicationId={{ Communication.Id }}''>Unsubscribe</a></small>
</p>
'
WHERE AttributeId in (select Id from Attribute where [Guid] = '2942EFCB-9BCF-4A16-9A78-D6149E2EAAD3')
AND [Value] = '
<p style=''float: right;''>
    <small><a href=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Unsubscribe/{{ Person.UrlEncodedKey }}''>Unsubscribe</a></small>
</p>
'

-- Update DefaultValue for Email Medium's 'Non-HTML Content' attribute
Update Attribute
set DefaultValue = '
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person | PersonTokenCreate:43200,3 }}
'
where 
[Guid] = 'FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88'
and
DefaultValue = '
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person.UrlEncodedKey }}
'

-- Update Value for Email Medium's 'Non-HTML Content' attribute
Update AttributeValue
set [Value] = '
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person | PersonTokenCreate:43200,3 }}
'
where
AttributeId in (select Id from Attribute where [Guid] = 'FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88')
and
[Value] = '
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person.UrlEncodedKey }}
'