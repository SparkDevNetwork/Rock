/* HtmlContent */
UPDATE HtmlContent
SET Content = REPLACE(Content, 'GlobalAttribute.OrganizationAddress', '''Global'' | Attribute:''OrganizationAddress''')
WHERE [Guid] IN (
        '9C6D118B-6C7D-46C3-ACEC-4CF586DCE81F'
        ,'FBC4C931-6B27-4AFA-A1DE-C72DB75493A6'
        ,'A51C28B4-DC64-4D72-91D5-B73CAF8B5958'
        ,'320864D0-8872-4E94-A0C0-B53E03645EE4'
        ,'B89605CF-C111-4A1B-9228-29A78EA9DA25'
        ,'8458375D-3F98-461C-ACF1-1A1CF08806B8'
        ,'939ADB18-D2A9-4444-BB07-4E33C14EFCFD'
        ,'8136DD90-7FE6-45BD-A467-94FB1962E33F'
        ,'33A47BDE-2CFE-487B-9786-4847CE45C44F'
        )
    AND ModifiedDateTime IS NULL
    AND Content LIKE '%GlobalAttribute.OrganizationAddress%'

UPDATE HtmlContent
SET Content = REPLACE(Content, 'GlobalAttribute.OrganizationName', '''Global'' | Attribute:''OrganizationName''')
WHERE [Guid] IN (
        '9C6D118B-6C7D-46C3-ACEC-4CF586DCE81F'
        ,'FBC4C931-6B27-4AFA-A1DE-C72DB75493A6'
        ,'A51C28B4-DC64-4D72-91D5-B73CAF8B5958'
        ,'320864D0-8872-4E94-A0C0-B53E03645EE4'
        ,'B89605CF-C111-4A1B-9228-29A78EA9DA25'
        ,'8458375D-3F98-461C-ACF1-1A1CF08806B8'
        ,'939ADB18-D2A9-4444-BB07-4E33C14EFCFD'
        ,'8136DD90-7FE6-45BD-A467-94FB1962E33F'
        ,'33A47BDE-2CFE-487B-9786-4847CE45C44F'
        )
    AND ModifiedDateTime IS NULL
    AND Content LIKE '%GlobalAttribute.OrganizationName%'

/* AttributeValue */
UPDATE AttributeValue
SET Value = REPLACE(Value, 'GlobalAttribute.EmailStyles', '''Global'' | Attribute:''EmailStyles''')
WHERE [Guid] IN (
        'D163C45F-A958-4FD5-A759-B1F2215737C3'
        ,'59D7A6DB-C291-4ED4-B4D7-35EF2A7EFD5A'
        )
    AND ModifiedDateTime IS NULL
    AND Value LIKE '%GlobalAttribute.EmailStyles%'

/* CommunicationTemplate Subject */
UPDATE CommunicationTemplate
SET [Subject] = REPLACE([Subject], 'GlobalAttribute.OrganizationName', '''Global'' | Attribute:''OrganizationName''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND [Subject] LIKE '%GlobalAttribute.OrganizationName%'

/* CommunicationTemplate MediumDataJson OrganizationName*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.OrganizationName', '''Global'' | Attribute:''OrganizationName''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.OrganizationName%'

/* CommunicationTemplate MediumDataJson OrganizationAddress*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.OrganizationAddress', '''Global'' | Attribute:''OrganizationAddress''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.OrganizationAddress%'

/* CommunicationTemplate MediumDataJson OrganizationPhone*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.OrganizationPhone', '''Global'' | Attribute:''OrganizationPhone''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.OrganizationPhone%'

/* CommunicationTemplate MediumDataJson OrganizationEmail*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.OrganizationEmail', '''Global'' | Attribute:''OrganizationEmail''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.OrganizationEmail%'

/* CommunicationTemplate MediumDataJson OrganizationWebsite*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.OrganizationWebsite', '''Global'' | Attribute:''OrganizationWebsite''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.OrganizationWebsite%'

/* CommunicationTemplate MediumDataJson EmailStyles*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.EmailStyles', '''Global'' | Attribute:''EmailStyles''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.EmailStyles%'

/* CommunicationTemplate MediumDataJson EmailHeader*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.EmailHeader', '''Global'' | Attribute:''EmailHeader''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.EmailHeader%'

/* CommunicationTemplate MediumDataJson EmailFooter*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.EmailFooter', '''Global'' | Attribute:''EmailFooter''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.EmailFooter%'

/* CommunicationTemplate MediumDataJson PublicApplicationRoot*/
UPDATE CommunicationTemplate
SET MediumDataJson = REPLACE(MediumDataJson, 'GlobalAttribute.PublicApplicationRoot', '''Global'' | Attribute:''PublicApplicationRoot''')
WHERE [Guid] IN (
        'AFE2ADD1-5278-441E-8E84-1DC743D99824'
        ,'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
        )
    AND (
        ModifiedDateTime IS NULL
        OR ModifiedDateTime = CAST(0x0000A38500F00EE8 AS DATETIME)
        )
    AND MediumDataJson LIKE '%GlobalAttribute.PublicApplicationRoot%'

/* SystemEmail Body */
UPDATE SystemEmail
SET [Body] = REPLACE([Body], 'GlobalAttribute.EmailHeader', '''Global'' | Attribute:''EmailHeader''')
WHERE [Guid] IN (
        '113593FF-620E-4870-86B1-7A0EC0409208'
        ,'17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
        ,'84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
        ,'75CB0A4A-B1C5-4958-ADEB-8621BD231520'
        ,'88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
        ,'73E8D035-61BB-495A-A87F-39007B98834C'
        ,'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
        ,'7DBF229E-7DEE-A684-4929-6C37312A0039'
        ,'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'
        )
    AND ModifiedDateTime IS NULL
    AND [Body] LIKE '%GlobalAttribute.EmailHeader%'

UPDATE SystemEmail
SET [Body] = REPLACE([Body], 'GlobalAttribute.InternalApplicationRoot', '''Global'' | Attribute:''InternalApplicationRoot''')
WHERE [Guid] IN (
        '113593FF-620E-4870-86B1-7A0EC0409208'
        ,'17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
        ,'84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
        ,'75CB0A4A-B1C5-4958-ADEB-8621BD231520'
        ,'88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
        ,'73E8D035-61BB-495A-A87F-39007B98834C'
        ,'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
        ,'7DBF229E-7DEE-A684-4929-6C37312A0039'
        ,'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'
        )
    AND ModifiedDateTime IS NULL
    AND [Body] LIKE '%GlobalAttribute.InternalApplicationRoot%'

UPDATE SystemEmail
SET [Body] = REPLACE([Body], 'GlobalAttribute.EmailFooter', '''Global'' | Attribute:''EmailFooter''')
WHERE [Guid] IN (
        '113593FF-620E-4870-86B1-7A0EC0409208'
        ,'17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
        ,'84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
        ,'75CB0A4A-B1C5-4958-ADEB-8621BD231520'
        ,'88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
        ,'73E8D035-61BB-495A-A87F-39007B98834C'
        ,'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
        ,'7DBF229E-7DEE-A684-4929-6C37312A0039'
        ,'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'
        )
    AND ModifiedDateTime IS NULL
    AND [Body] LIKE '%GlobalAttribute.EmailFooter%'

UPDATE SystemEmail
SET [Body] = REPLACE([Body], 'GlobalAttribute.OrganizationName', '''Global'' | Attribute:''OrganizationName''')
WHERE [Guid] IN (
        '113593FF-620E-4870-86B1-7A0EC0409208'
        ,'17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
        ,'84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
        ,'75CB0A4A-B1C5-4958-ADEB-8621BD231520'
        ,'88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
        ,'73E8D035-61BB-495A-A87F-39007B98834C'
        ,'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
        ,'7DBF229E-7DEE-A684-4929-6C37312A0039'
        ,'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'
        )
    AND ModifiedDateTime IS NULL
    AND [Body] LIKE '%GlobalAttribute.OrganizationName%'

UPDATE SystemEmail
SET [Body] = REPLACE([Body], 'GlobalAttribute.OrganizationEmail', '''Global'' | Attribute:''OrganizationEmail''')
WHERE [Guid] IN (
        '113593FF-620E-4870-86B1-7A0EC0409208'
        ,'17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
        ,'84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
        ,'75CB0A4A-B1C5-4958-ADEB-8621BD231520'
        ,'88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
        ,'73E8D035-61BB-495A-A87F-39007B98834C'
        ,'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
        ,'7DBF229E-7DEE-A684-4929-6C37312A0039'
        ,'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'
        )
    AND ModifiedDateTime IS NULL
    AND [Body] LIKE '%GlobalAttribute.OrganizationEmail%'

UPDATE SystemEmail
SET [Body] = REPLACE([Body], 'GlobalAttribute.PublicApplicationRoot', '''Global'' | Attribute:''PublicApplicationRoot''')
WHERE [Guid] IN (
        '113593FF-620E-4870-86B1-7A0EC0409208'
        ,'17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
        ,'84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
        ,'75CB0A4A-B1C5-4958-ADEB-8621BD231520'
        ,'88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
        ,'73E8D035-61BB-495A-A87F-39007B98834C'
        ,'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
        ,'7DBF229E-7DEE-A684-4929-6C37312A0039'
        ,'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'
        )
    AND ModifiedDateTime IS NULL
    AND [Body] LIKE '%GlobalAttribute.PublicApplicationRoot%'

/* WorkflowActionForm */
UPDATE WorkflowActionForm
SET Header = REPLACE(Header, 'GlobalAttribute.OrganizationPhone', '''Global'' | Attribute:''OrganizationPhone''')
WHERE [Guid] IN ('11D4769F-5B93-4605-8BCA-D21C14B0CEBA')
    AND ModifiedDateTime IS NULL
    AND Header LIKE '%GlobalAttribute.OrganizationPhone%'

/*  Kiosk Labels */
UPDATE [AttributeValue] SET [Value] = '{% assign personAllergy = Person | Attribute:''Allergy'' %}{% if personAllergy != '''' %}{{ personAllergy | Truncate:100,''...'' }}{% endif %}'
WHERE [Guid] = '4315A58E-6514-49A8-B80C-22AC7710AC19' AND [ModifiedDateTime] IS NULL

UPDATE [AttributeValue] SET [Value] = '{% assign personLegalNotes = Person | Attribute:''LegalNotes'' %}{% if personLegalNotes != '''' %}{{ personLegalNotes | Truncate:100,''...'' }}{% endif %}'
WHERE [Guid] = '89C604FA-61A9-4255-AE1F-B6381B23603F' AND [ModifiedDateTime] IS NULL

UPDATE [AttributeValue] SET [Value] = '{% assign personAllergy = Person | Attribute:''Allergy'' %}{% if personAllergy != '''' -%}A{% endif -%}'
WHERE [Guid] = '5DD35431-D22D-4410-9A55-55EAC9859C35' AND [ModifiedDateTime] IS NULL

UPDATE [AttributeValue] SET [Value] = '{% assign personLegalNotes = Person | Attribute:''LegalNotes'' %}{% if personLegalNotes != '''' %}L{% endif %}'
WHERE [Guid] = '872DBF30-E0C0-4810-A36E-D28FC3124A51' AND [ModifiedDateTime] IS NULL

DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3')
DECLARE @EntityId int = (SELECT TOP 1 [Id] FROM [PersonBadge] WHERE [Guid] = '66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE')

/* Baptism Badge */
IF @AttributeId IS NOT NULL AND @EntityId IS NOT NULL
BEGIN
	UPDATE [AttributeValue]
		SET [Value] = '{% assign baptismDate = Person | Attribute:''BaptismDate'' %}

{% if baptismDate != '''' -%}
    <div class="badge badge-baptism" data-toggle="tooltip" data-original-title="{{ Person.NickName }} was baptized on {{ baptismDate }}.">
<i class="badge-icon fa fa-tint"></i>
    </div>
{% else -%}
    <div class="badge badge-baptism" data-toggle="tooltip" data-original-title="No baptism date entered for {{ Person.NickName }}.">
        <i class="badge-icon badge-disabled fa fa-tint"></i>
    </div>
{% endif -%}'
		WHERE[AttributeId] = @AttributeId AND[EntityId] = @EntityId
        AND[CreatedDateTime] = [ModifiedDateTime]
    END
