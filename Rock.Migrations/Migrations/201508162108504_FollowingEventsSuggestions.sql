-- Update the Homepage and Error layouts on RockRMS site to be system
DECLARE @SiteId int = ( SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4' )
UPDATE [Layout] SET 
	 [IsSystem] = 1
	,[Guid] = 'CABD576F-C700-4690-835A-1BFBDD7DCBE6'
WHERE [SiteId] = @SiteId
AND [FileName] = 'HomePage'

UPDATE [Layout] SET 
	 [IsSystem] = 1
	,[Guid] = '7E816087-6A8C-498E-BFE7-D0B684A9DD45'
WHERE [SiteId] = @SiteId
AND [FileName] = 'Error'

-- Update the Blank layout on external website to be system
SET @SiteId = ( SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B' )
UPDATE [Layout] SET 
	 [IsSystem] = 1
	,[Guid] = '7E4EC84E-A7BD-426B-AC77-230F28481FF0'
WHERE [SiteId] = @SiteId
AND [FileName] = 'Blank'

-- Update the Blank layout on checkin website to be system
SET @SiteId = ( SELECT TOP 1 [Id] FROM [Site] WHERE [Guid] = 'A5FA7C3C-A238-4E0B-95DE-B540144321EC' )
UPDATE [Layout] SET 
	 [IsSystem] = 1
	,[Guid] = 'BD09B885-8255-4DFD-88FE-E4CDAA6981D1'
WHERE [SiteId] = @SiteId
AND [FileName] = 'Blank'

-- Update the order of other blocks on the dashboard sidebar
UPDATE B SET [Order] = B.[Order] + 1
FROM [Page] P
INNER JOIN [Block] B
	ON B.[PageId] = P.[Id]
	AND B.[Zone] = 'Sidebar1'
	AND B.[Name] <> 'Person Suggestion Notice'
WHERE P.[Guid] = 'AE1818D8-581C-4599-97B9-509EA450376A'

-- Add Birthday following event types
DECLARE @PersonEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )
DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '532A7405-A3FB-4147-BE67-3B75A230AADE' )
DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'F5B35909-5A4A-4203-84A8-7F493E56548B' )
IF @EntityTypeId IS NOT NULL
BEGIN

	INSERT INTO [FollowingEventType] ( [Name], [Description], [EntityTypeId], [FollowedEntityTypeId], [IsActive], [SendOnWeekends], [IsNoticeRequired], [EntityNotificationFormatLava], [Guid] )
	VALUES 
	    ( 'Person Birthday (5 day notice)', 'Upcoming Birthday', @EntityTypeId, @PersonEntityTypeId, 1, 0, 0, 
'<tr>
    <td>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=120&maxheight=120''/>
        {% endif %}
    </td>
    <td>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> has a birthday on 
        {{ Entity.Person.NextBirthDay | Date:''dddd, MMMM dd'' }} ({{ Entity.Person.NextBirthDay | HumanizeDateTime }})</strong><br />

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
</tr>', 'E1C2F8BD-E875-4C7B-91A1-EDB98AB01BDC' ),
	    ( 'Person Birthday (day of notice)', 'Birthday Today', @EntityTypeId, @PersonEntityTypeId, 1, 0, 1, 
'<tr>
    <td>
        {% if Entity.Person.PhotoId %} 
            <img src=''{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}GetImage.ashx?id={{ Entity.Person.PhotoId }}&maxwidth=120&maxheight=120''/>
        {% endif %}
    </td>
    <td>
        <strong><a href="{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}Person/{{ Entity.PersonId }}">{{ Entity.Person.FullName }}</a> has a birthday on 
        {{ Entity.Person.NextBirthDay | Date:''dddd, MMMM dd'' }} ({{ Entity.Person.NextBirthDay | HumanizeDateTime }})</strong><br />

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
</tr>', 'F3A577DB-8F4A-4245-BD00-0B2B8F789131' )
	IF @AttributeId IS NOT NULL

	BEGIN
		INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		SELECT 0, @AttributeId, [Id], '5', NEWID()
		FROM [FollowingEventType] WHERE [Guid] = 'E1C2F8BD-E875-4C7B-91A1-EDB98AB01BDC'

		INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		SELECT 0, @AttributeId, [Id], '0', NEWID()
		FROM [FollowingEventType] WHERE [Guid] = 'F3A577DB-8F4A-4245-BD00-0B2B8F789131'
	END

END

-- Add in family together suggestion
SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '20AC7F2A-D42F-438D-93D7-46E3C6769B8F' )
IF @EntityTypeId IS NOT NULL
BEGIN
	
    INSERT INTO [FollowingSuggestionType] ( [Name], [Description], [ReasonNote], [ReminderDays], [EntityTypeId], [IsActive], [EntityNotificationFormatLava], [Guid] )
	VALUES 
	    ( 'In Family Together', 'Your Family Members', 'Family Member', 30, @EntityTypeId, 1, 
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
</tr>', '8641F468-272B-4617-91ED-AB312D0F273C' )

    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'AB754108-EB85-422B-A898-90C47495174A' )
	IF @AttributeId IS NOT NULL
	BEGIN
		INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		SELECT 0, @AttributeId, [Id], '790E3215-3B10-442B-AF69-616C0DCB998E', NEWID()
		FROM [FollowingSuggestionType] WHERE [Guid] = '8641F468-272B-4617-91ED-AB312D0F273C'
	END

END

-- Event Notification Job
DECLARE @JobId int
INSERT INTO [ServiceJob] ( [IsSystem], [IsActive], [Name], [Description], [Assembly], [Class], [CronExpression], [Guid], [NotificationStatus] )
VALUES ( 0, 1, 'Send Following Event Notification', 'Calculates and sends any following event notices to those that are following the entities that have an event that occurred.',
    '', 'Rock.Jobs.SendFollowingEvents','0 0 7 ? * MON-FRI *','893A745F-8642-4095-9E91-F8C54547DEF0', 3 )
SET @JobId = SCOPE_IDENTITY()

SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '75E0D938-0CA0-4121-B013-D5B7C03BFBB8' )
INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
VALUES ( 0, @AttributeId, @JobId, 'ca7576cd-0a10-4ada-a068-62ee598178f5', NEWID() )

SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '446B2177-76DF-4082-A89E-E18A1B26CCF9' )
INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
VALUES ( 0, @AttributeId, @JobId, '2c112948-ff4c-46e7-981a-0257681eadf4', NEWID() )

-- Suggestion Notification Job
INSERT INTO [ServiceJob] ( [IsSystem], [IsActive], [Name], [Description], [Assembly], [Class], [CronExpression], [Guid], [NotificationStatus] )
VALUES ( 0, 1, 'Send Following Suggestion Notification', 'Calculates and sends any following suggestions to those people that are eligible for following.',
    '', 'Rock.Jobs.SendFollowingSuggestions','0 0 15 ? * MON-FRI *','9C955693-B19C-4A90-9407-7A38450D75FC', 3 )
SET @JobId = SCOPE_IDENTITY()

SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'E40511EA-3AAD-4C4B-9AB4-33745AD1A00A' )
INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
VALUES ( 0, @AttributeId, @JobId, '8f5a9400-aed2-48a4-b5c8-c9b5d5669f4c', NEWID() )

SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'B1E268B0-F890-433A-9FED-331CF4D4FD2E' )
INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
VALUES ( 0, @AttributeId, @JobId, '2c112948-ff4c-46e7-981a-0257681eadf4', NEWID() )