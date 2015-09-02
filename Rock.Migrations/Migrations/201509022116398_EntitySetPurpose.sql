-- MP: Fix FinancialTransactionImage wrong BinaryFileType
DECLARE @binaryFileTypeContributionId INT = (
        SELECT TOP 1 Id
        FROM BinaryFileType
        WHERE [Guid] = '6D18A9C4-34AB-444A-B95B-C644019465AC'
        )

DECLARE @binaryFileTypeContributionStorageEntityTypeId INT = (
        SELECT TOP 1 StorageEntityTypeId
        FROM BinaryFileType
        WHERE [Guid] = '6D18A9C4-34AB-444A-B95B-C644019465AC'
        )

UPDATE BinaryFile
SET BinaryFileTypeId = @binaryFileTypeContributionId
WHERE Id IN (
        SELECT fti.BinaryFileId
        FROM FinancialTransactionImage fti
  join BinaryFile bf on fti.BinaryFileId = bf.Id
  join BinaryFileType bft on bf.BinaryFileTypeId = bft.Id
  where bf.StorageEntityTypeId = @binaryFileTypeContributionStorageEntityTypeId
  and bft.StorageEntityTypeId = @binaryFileTypeContributionStorageEntityTypeId
        )
    AND BinaryFileTypeId != @binaryFileTypeContributionId


-- MP: Fix Business Address not getting set as IsMailingLocation = true
UPDATE GroupLocation
SET IsMailingLocation = 1
WHERE IsMailingLocation = 0
    and GroupId IN (
        SELECT Id
        FROM [Group]
        WHERE Id IN (
                SELECT GroupId
                FROM GroupMember
                WHERE PersonId IN (
                        SELECT Id
                        FROM Person
                        WHERE RecordTypeValueId = (
                                SELECT TOP 1 id
                                FROM DefinedValue
                                WHERE Guid = 'BF64ADD3-E70A-44CE-9C4B-E76BBED37550' /* Business */
                                )
                        )
                )
            AND GroupTypeId = (
                SELECT TOP 1 id
                FROM GroupType
                WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' /* Family */
                )
        )

-- JE: Updated Following Events
-- Update System Email
UPDATE [SystemEmail]
SET [Body] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    Listed below are events that you have requested to be notified about.
</p>

{% for event in EventTypes %}
    <h2>{{ event.EventType.Name }}</h2>
    <table>
    {% for notice in event.Notices %}
        {{ notice }}
    {% endfor %}
    </table>
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE [Guid] = 'CA7576CD-0A10-4ADA-A068-62EE598178F5'

-- Upcoming Anniversaries
UPDATE [FollowingEventType] SET
	[Name] = 'Upcoming Anniversaries',
	[EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> has a 
        {{ Entity.Person.AnniversaryDate | DateDiff:Entity.Person.NextAnniversary,''Y'' }} year anniversary on 
        {{ Entity.Person.NextAnniversary | Date:''dddd, MMMM d'' }} ({{ Entity.Person.NextAnniversary | DaysFromNow | Capitalize }})</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>'
WHERE [Guid] = '5E81D053-974F-4841-A829-58410356F080'

-- Upcoming Birthdays
UPDATE [FollowingEventType] SET
	[Name] = 'Upcoming Birthdays',
	[EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> has a birthday on 
        {{ Entity.Person.NextBirthDay | Date:''dddd, MMMM d'' }} ({{ Entity.Person.NextBirthDay | DaysFromNow | Capitalize }})</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
        
    </td>
</tr>'
WHERE [Guid] = 'E1C2F8BD-E875-4C7B-91A1-EDB98AB01BDC'

-- Started Servings
UPDATE [FollowingEventType] SET
	[EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> recently 
        began serving.</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>'
WHERE [Guid] = '8F1476AD-CF60-4253-AC10-2094C6863D96'

-- Joined A Group
UPDATE [FollowingEventType] SET
	[EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> 
        recently joined a small group.</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>'
WHERE [Guid] = 'CDFB247B-B868-49BD-A080-0B2E8679DA4C'

-- Baptisms
UPDATE [FollowingEventType] SET
	[Name] = 'Baptisms',
	[EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> was baptized on 
        {{ Entity.Person | Attribute:''BaptismDate'' | Date:''dddd, MMMM d'' }} ({{ Entity.Person | Attribute:''BaptismDate'' | DaysFromNow | Capitalize }})</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
    </td>
</tr>
'
WHERE [Guid] = '694D5110-EFF0-4E53-818F-A8AFEA268584'

-- Birthdays
UPDATE [FollowingEventType] SET
	[Name] = 'Birthdays',
	[EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> has a birthday on 
        {{ Entity.Person.NextBirthDay | Date:''dddd, MMMM d'' }} ({{ Entity.Person.NextBirthDay | DaysFromNow | Capitalize }})</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}
        
        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
        
    </td>
</tr>'
WHERE [Guid] = 'F3A577DB-8F4A-4245-BD00-0B2B8F789131'


-- JE: Updated Following Events
-- Family Members 
UPDATE [FollowingSuggestionType] 
SET [EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong>{{ Entity.Person.FullName }}</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}

        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
        
    </td>
</tr>' 
WHERE [Guid] = '8641F468-272B-4617-91ED-AB312D0F273C' 


-- Fellow Staff Members 
UPDATE [FollowingSuggestionType] 
SET [EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong>{{ Entity.Person.FullName }}</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}

        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
        
    </td>
</tr>' 
WHERE [Guid] = 'FC72D537-9C0C-40E3-884A-9FC5AA0AAF90' 


-- Fellow Small Group Members 
UPDATE [FollowingSuggestionType] 
SET [EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong>{{ Entity.Person.FullName }}</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}

        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
        
    </td>
</tr>' 
WHERE [Guid] = '9AB331F3-B8EC-47F6-8A6C-80F7C0E0D1BB' 


-- Known Relationship 
UPDATE [FollowingSuggestionType] 
SET [EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong>{{ Entity.Person.FullName }}</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}

        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
        
    </td>
</tr>' 
WHERE [Guid] = 'DFB3459C-2022-4A4F-A968-D8FFFF150DAB' 


-- Serving Team Member 
UPDATE [FollowingSuggestionType] 
SET [EntityNotificationFormatLava] = '<tr>
    <td style=''padding-bottom: 12px; padding-right: 12px;''>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=75&maxheight=75''/>
        {% endif %}
    </td>
    <td valign="top" style=''padding-bottom: 12px;''>
        <strong>{{ Entity.Person.FullName }}</strong><br />

        {% if Entity.Person.Email != empty %}
            Email: <a href="mailto:{{ Entity.Person.Email }}">{{ Entity.Person.Email }}</a><br />
        {% endif %}

        {% assign mobilePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 12 | Select:''NumberFormatted'' %}
        {% if mobilePhone != empty %}
            Cell: {{ mobilePhone }}<br />
        {% endif %}
        
        {% assign homePhone = Entity.Person.PhoneNumbers | Where:''NumberTypeValueId'', 13 | Select:''NumberFormatted'' %}
        {% if homePhone != empty %}
            Home: {{ homePhone }}<br />
        {% endif %}
        
    </td>
</tr>' 
WHERE [Guid] = 'FC861A95-5837-4AC9-BBEB-82AC63971639' 