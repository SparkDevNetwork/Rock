DECLARE @BadgeGuid UNIQUEIDENTIFIER = '132F9C2A-0AF4-4AD9-87EF-7730B284E10E';
DECLARE @Lava NVARCHAR(MAX) = 
N'{% assign groupHasRequirements = Entity.GroupType.GroupRequirements | Size | AsBoolean %}
{% assign typeHasRequirements = Entity.GroupRequirements | Size | AsBoolean %}

{% if groupHasRequirements or typeHasRequirements -%}
    <div class="badge" data-toggle="tooltip" data-original-title="Group has requirements." style="color:var(--brand-success);">
        <i class="badge-icon fa fa-tasks"></i>
    </div>
{% endif -%}';

IF NOT EXISTS ( SELECT * FROM [Badge] WHERE [Guid] = @BadgeGuid )
BEGIN
    DECLARE @BadgeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Badge' );
    DECLARE @LiquidBadgeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Badge.Component.Liquid' );

    INSERT INTO [Badge] (
        [Name], 
        [Description], 
        [BadgeComponentEntityTypeId], 
        [Order], 
        [Guid], 
        [IsActive], 
        [EntityTypeId], 
        [EntityTypeQualifierColumn], 
        [EntityTypeQualifierValue]
    ) VALUES ( 
        N'Group Requirements', 
        N'Shows if a group has requirements.', 
        @LiquidBadgeEntityTypeId, 
        '0', 
        @BadgeGuid, 
        '1', 
        '16', 
        NULL, 
        NULL
    );

    DECLARE @BadgeId INT = ( SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid] = @BadgeGuid );

	DECLARE @BadgeAttributeId INT = ( 
		SELECT TOP 1 [Id] 
		FROM [Attribute] 
		WHERE 
            [EntityTypeId] = @BadgeEntityTypeId 
		    AND [EntityTypeQualifierColumn] = 'BadgeComponentEntityTypeId'
		    AND [EntityTypeQualifierValue] = @LiquidBadgeEntityTypeId
		    AND [Key] = 'DisplayText' );

    INSERT INTO [AttributeValue] (
        [IsSystem], 
        [AttributeId], 
        [EntityId], 
        [Value], 
        [Guid]
    ) VALUES (
        '0', 
        @BadgeAttributeId, 
        @BadgeId, 
        @Lava, 
        NEWID() 
    );
END