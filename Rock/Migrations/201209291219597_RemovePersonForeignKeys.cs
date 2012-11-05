namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
#pragma warning disable 1591

    public partial class RemovePersonForeignKeys : DbMigration
    {
        public override void Up()
        {
            DropForeignKey( "dbo.cmsAuth", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsAuth", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.crmPerson", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.crmPerson", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsUser", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsUser", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.crmEmailTemplate", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.crmEmailTemplate", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.crmPhoneNumber", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.crmPhoneNumber", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreDefinedValue", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreDefinedValue", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreDefinedType", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreDefinedType", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreFieldType", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreFieldType", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreAttribute", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreAttribute", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreAttributeQualifier", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreAttributeQualifier", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreAttributeValue", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreAttributeValue", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.groupsMember", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.groupsMember", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.groupsGroup", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.groupsGroup", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.crmLocation", "CreatedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.crmLocation", "ModifiedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.groupsGroupType", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.groupsGroupType", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.groupsGroupRole", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.groupsGroupRole", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.crmCampus", "CreatedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.crmCampus", "ModifiedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.financialPledge", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialPledge", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialFund", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialFund", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialTransaction", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialTransaction", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialBatch", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialBatch", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialGateway", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialGateway", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialTransactionDetail", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.financialTransactionDetail", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsFile", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsFile", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsBlockType", "CreatedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.cmsBlockType", "ModifiedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.cmsBlock", "CreatedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.cmsBlock", "ModifiedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.cmsHtmlContent", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsHtmlContent", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsPage", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsPage", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsPageRoute", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsPageRoute", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsPageContext", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsPageContext", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsSite", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsSite", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsSiteDomain", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.cmsSiteDomain", "ModifiedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreEntityChange", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.coreExceptionLog", "PersonId", "crmPerson" );
            DropForeignKey( "dbo.coreMetric", "CreatedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.coreMetric", "ModifiedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.coreMetricValue", "CreatedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.coreMetricValue", "ModifiedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.coreTag", "CreatedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.coreTag", "ModifiedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.coreTaggedItem", "CreatedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.coreTaggedItem", "ModifiedByPersonId", "dbo.crmPerson" );
            DropForeignKey( "dbo.utilJob", "CreatedByPersonId", "crmPerson" );
            DropForeignKey( "dbo.utilJob", "ModifiedByPersonId", "crmPerson" );
            DropIndex( "cmsAuth", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsAuth", new[] { "ModifiedByPersonId" } );
            DropIndex( "crmPerson", new[] { "CreatedByPersonId" } );
            DropIndex( "crmPerson", new[] { "ModifiedByPersonId" } );
            DropIndex( "cmsUser", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsUser", new[] { "ModifiedByPersonId" } );
            DropIndex( "crmEmailTemplate", new[] { "CreatedByPersonId" } );
            DropIndex( "crmEmailTemplate", new[] { "ModifiedByPersonId" } );
            DropIndex( "crmPhoneNumber", new[] { "CreatedByPersonId" } );
            DropIndex( "crmPhoneNumber", new[] { "ModifiedByPersonId" } );
            DropIndex( "coreDefinedValue", new[] { "CreatedByPersonId" } );
            DropIndex( "coreDefinedValue", new[] { "ModifiedByPersonId" } );
            DropIndex( "coreDefinedType", new[] { "CreatedByPersonId" } );
            DropIndex( "coreDefinedType", new[] { "ModifiedByPersonId" } );
            DropIndex( "coreFieldType", new[] { "CreatedByPersonId" } );
            DropIndex( "coreFieldType", new[] { "ModifiedByPersonId" } );
            DropIndex( "coreAttribute", new[] { "CreatedByPersonId" } );
            DropIndex( "coreAttribute", new[] { "ModifiedByPersonId" } );
            DropIndex( "coreAttributeQualifier", new[] { "CreatedByPersonId" } );
            DropIndex( "coreAttributeQualifier", new[] { "ModifiedByPersonId" } );
            DropIndex( "coreAttributeValue", new[] { "CreatedByPersonId" } );
            DropIndex( "coreAttributeValue", new[] { "ModifiedByPersonId" } );
            DropIndex( "groupsMember", new[] { "CreatedByPersonId" } );
            DropIndex( "groupsMember", new[] { "ModifiedByPersonId" } );
            DropIndex( "groupsGroup", new[] { "CreatedByPersonId" } );
            DropIndex( "groupsGroup", new[] { "ModifiedByPersonId" } );
            DropIndex( "dbo.crmLocation", new[] { "CreatedByPersonId" } );
            DropIndex( "dbo.crmLocation", new[] { "ModifiedByPersonId" } );
            DropIndex( "groupsGroupType", new[] { "CreatedByPersonId" } );
            DropIndex( "groupsGroupType", new[] { "ModifiedByPersonId" } );
            DropIndex( "groupsGroupRole", new[] { "CreatedByPersonId" } );
            DropIndex( "groupsGroupRole", new[] { "ModifiedByPersonId" } );
            DropIndex( "dbo.crmCampus", new[] { "CreatedByPersonId" } );
            DropIndex( "dbo.crmCampus", new[] { "ModifiedByPersonId" } );
            DropIndex( "financialPledge", new[] { "CreatedByPersonId" } );
            DropIndex( "financialPledge", new[] { "ModifiedByPersonId" } );
            DropIndex( "financialFund", new[] { "CreatedByPersonId" } );
            DropIndex( "financialFund", new[] { "ModifiedByPersonId" } );
            DropIndex( "financialTransaction", new[] { "CreatedByPersonId" } );
            DropIndex( "financialTransaction", new[] { "ModifiedByPersonId" } );
            DropIndex( "financialBatch", new[] { "CreatedByPersonId" } );
            DropIndex( "financialBatch", new[] { "ModifiedByPersonId" } );
            DropIndex( "financialGateway", new[] { "CreatedByPersonId" } );
            DropIndex( "financialGateway", new[] { "ModifiedByPersonId" } );
            DropIndex( "financialTransactionDetail", new[] { "CreatedByPersonId" } );
            DropIndex( "financialTransactionDetail", new[] { "ModifiedByPersonId" } );
            DropIndex( "cmsFile", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsFile", new[] { "ModifiedByPersonId" } );
            DropIndex( "dbo.cmsBlockType", new[] { "CreatedByPersonId" } );
            DropIndex( "dbo.cmsBlockType", new[] { "ModifiedByPersonId" } );
            DropIndex( "dbo.cmsBlock", new[] { "CreatedByPersonId" } );
            DropIndex( "dbo.cmsBlock", new[] { "ModifiedByPersonId" } );
            DropIndex( "cmsHtmlContent", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsHtmlContent", new[] { "ModifiedByPersonId" } );
            DropIndex( "cmsPage", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsPage", new[] { "ModifiedByPersonId" } );
            DropIndex( "cmsPageRoute", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsPageRoute", new[] { "ModifiedByPersonId" } );
            DropIndex( "cmsPageContext", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsPageContext", new[] { "ModifiedByPersonId" } );
            DropIndex( "cmsSite", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsSite", new[] { "ModifiedByPersonId" } );
            DropIndex( "cmsSiteDomain", new[] { "CreatedByPersonId" } );
            DropIndex( "cmsSiteDomain", new[] { "ModifiedByPersonId" } );
            DropIndex( "coreEntityChange", new[] { "CreatedByPersonId" } );
            DropIndex( "coreExceptionLog", new[] { "PersonId" } );
            DropIndex( "dbo.coreMetric", new[] { "CreatedByPersonId" } );
            DropIndex( "dbo.coreMetric", new[] { "ModifiedByPersonId" } );
            DropIndex( "dbo.coreMetricValue", new[] { "CreatedByPersonId" } );
            DropIndex( "dbo.coreMetricValue", new[] { "ModifiedByPersonId" } );
            DropIndex( "dbo.coreTag", new[] { "CreatedByPersonId" } );
            DropIndex( "dbo.coreTag", new[] { "ModifiedByPersonId" } );
            DropIndex( "dbo.coreTaggedItem", new[] { "CreatedByPersonId" } );
            DropIndex( "dbo.coreTaggedItem", new[] { "ModifiedByPersonId" } );
            DropIndex( "utilJob", new[] { "CreatedByPersonId" } );
            DropIndex( "utilJob", new[] { "ModifiedByPersonId" } );
            AddColumn( "coreExceptionLog", "CreatedByPersonId", c => c.Int() );
            DropColumn( "coreExceptionLog", "PersonId" );

            Sql( @"
ALTER PROC [dbo].[crmPerson_sp_Merge]
@OldId int, 
@NewId int

AS

DECLARE @OldGuid uniqueidentifier
DECLARE @NewGuid uniqueidentifier

SET @OldGuid = (SELECT [Guid] FROM crmPerson WHERE [Id] = @OldId)
SET @NewGuid = (SELECT [Guid] FROM crmPerson WHERE [Id] = @NewId)

IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
BEGIN

    BEGIN TRANSACTION

    -- cmsAuth
    UPDATE AO
        SET PersonId = @NewId
    FROM cmsAuth AO
    LEFT OUTER JOIN cmsAuth AN
        ON AN.PersonId = @NewId
        AND AN.EntityType = AO.EntityId
        AND AN.EntityId = AO.EntityId
        AND AN.Action = AO.Action
        AND AN.AllowOrDeny = AO.AllowOrDeny
        AND AN.SpecialRole = AO.SpecialRole
    WHERE AO.PersonId = @OldId
    AND AN.Id IS NULL

    DELETE cmsAuth
    WHERE PersonId = @OldId

    -- coreTag
    UPDATE OT
        SET OwnerId = @NewId
    FROM coreTag OT
    LEFT OUTER JOIN coreTag NT
        ON NT.OwnerId = @NewId
        AND NT.Entity = OT.Entity
        AND NT.EntityQualifierColumn = OT.EntityQualifierColumn
        AND NT.EntityQualifierValue = OT.EntityQualifierValue
        AND NT.Name = OT.Name
    WHERE OT.OwnerId = @OldId
    AND NT.Id IS NULL

    UPDATE OTI
        SET TagId = NT.Id
    FROM coreTag OT
    INNER JOIN coreTag NT
        ON NT.OwnerId = @NewId
        AND NT.Entity = OT.Entity
        AND NT.EntityQualifierColumn = OT.EntityQualifierColumn
        AND NT.EntityQualifierValue = OT.EntityQualifierValue
        AND NT.Name = OT.Name
    INNER JOIN coreTaggedItem OTI
        ON OTI.TagId = OT.Id
    LEFT OUTER JOIN coreTaggedItem NTI
        ON NTI.TagId = NT.Id
        AND NTI.EntityId = OTI.EntityId
    WHERE OT.OwnerId = @OldId
    AND NTI.Id IS NULL

    DELETE coreTag
    WHERE OwnerId = @OldId

    -- crmPhoneNumber
    UPDATE OP
        SET PersonId = @NewId
    FROM crmPhoneNumber OP
    LEFT OUTER JOIN crmPhoneNumber NP
        ON NP.PersonId = @NewId
        AND NP.Number = OP.Number
        AND NP.Extension = OP.Extension
        AND NP.NumberTypeId = OP.NumberTypeId
    WHERE OP.PersonId = @OldId
    AND NP.Id IS NULL

    DELETE crmPhoneNumber
    WHERE PersonId = @OldId

    -- groupsMember
    UPDATE OP
        SET PersonId = @NewId
    FROM groupsMember OP
    LEFT OUTER JOIN groupsMember NP
        ON NP.PersonId = @NewId
        AND NP.GroupId = OP.GroupId
        AND NP.GroupRoleId = OP.GroupRoleId
    WHERE OP.PersonId = @OldId
    AND NP.Id IS NULL

    DELETE groupsMember
    WHERE PersonId = @OldId

    INSERT INTO crmPersonMerged ([Id], [Guid], [CurrentId], [CurrentGuid])
    VALUES (@OldId, @OldGuid, @NewId, @NewGuid)

    DECLARE @Sql varchar(max)

    DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
    SELECT 
        ' UPDATE ' + tso.name +
        ' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
        ' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
    FROM sys.foreign_key_columns kc
    INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
    INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
    INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
    INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
    INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
    WHERE so.name = 'crmPerson'
    AND rac.name = 'Id'
    AND tso.name NOT IN ('cmsAuth', 'coreTag', 'crmPhoneNumber', 'groupsMember')

    OPEN ForeignKeyCursor

    FETCH NEXT
    FROM ForeignKeyCursor
    INTO @Sql

    WHILE (@@FETCH_STATUS <> -1)
    BEGIN

        IF (@@FETCH_STATUS = 0)
        BEGIN

            EXEC(@Sql)
            
        END
        
        FETCH NEXT
        FROM ForeignKeyCursor
        INTO @Sql

    END

    CLOSE ForeignKeyCursor
    DEALLOCATE ForeignKeyCursor

    DELETE crmPerson
    WHERE [Id] = @OldId
    
    --- Update any old person tags
    UPDATE TIO
        SET [EntityId] = @NewId
    FROM [coreTag] A
    INNER JOIN [coreTaggedItem] TIO
        ON TIO.[TagId] = A.[Id]
        AND TIO.[EntityId] = @OldId
    LEFT OUTER JOIN [coreTaggedItem] TIN
        ON TIN.[TagId] = A.[Id]
        AND TIN.[EntityId] = @NewId
    WHERE A.[Entity] = 'Rock.CRM.Person'
    AND TIN.[TagId] IS NULL

    -- Delete any remaining Attribute Values
    DELETE AV
    FROM [coreTag] A
    INNER JOIN [coreTaggedItem] AV
        ON AV.[TagId] = A.[Id]
        AND AV.[EntityId] = @OldId
    WHERE A.[Entity] = 'Rock.CRM.Person'

    --- Update any old person attributes to the new new id
    UPDATE AVO
        SET [EntityId] = @NewId
    FROM [coreAttribute] A
    INNER JOIN [coreAttributeValue] AVO
        ON AVO.[AttributeId] = A.[Id]
        AND AVO.[EntityId] = @OldId
    LEFT OUTER JOIN [coreAttributeValue] AVN
        ON AVN.[AttributeId] = A.[Id]
        AND AVN.[EntityId] = @NewId
    WHERE A.[Entity] = 'Rock.CRM.Person'
    AND AVN.[AttributeId] IS NULL

    -- Delete any remaining Attribute Values
    DELETE AV
    FROM [coreAttribute] A
    INNER JOIN [coreAttributeValue] AV
        ON AV.[AttributeId] = A.[Id]
        AND AV.[EntityId] = @OldId
    WHERE A.[Entity] = 'Rock.CRM.Person'
    
    COMMIT TRANSACTION

END

" );
        }

        public override void Down()
        {
            AddColumn( "coreExceptionLog", "PersonId", c => c.Int() );
            DropColumn( "coreExceptionLog", "CreatedByPersonId" );
            CreateIndex( "utilJob", "ModifiedByPersonId" );
            CreateIndex( "utilJob", "CreatedByPersonId" );
            CreateIndex( "dbo.coreTaggedItem", "ModifiedByPersonId" );
            CreateIndex( "dbo.coreTaggedItem", "CreatedByPersonId" );
            CreateIndex( "dbo.coreTag", "ModifiedByPersonId" );
            CreateIndex( "dbo.coreTag", "CreatedByPersonId" );
            CreateIndex( "dbo.coreMetricValue", "ModifiedByPersonId" );
            CreateIndex( "dbo.coreMetricValue", "CreatedByPersonId" );
            CreateIndex( "dbo.coreMetric", "ModifiedByPersonId" );
            CreateIndex( "dbo.coreMetric", "CreatedByPersonId" );
            CreateIndex( "coreExceptionLog", "PersonId" );
            CreateIndex( "coreEntityChange", "CreatedByPersonId" );
            CreateIndex( "cmsSiteDomain", "ModifiedByPersonId" );
            CreateIndex( "cmsSiteDomain", "CreatedByPersonId" );
            CreateIndex( "cmsSite", "ModifiedByPersonId" );
            CreateIndex( "cmsSite", "CreatedByPersonId" );
            CreateIndex( "cmsPageContext", "ModifiedByPersonId" );
            CreateIndex( "cmsPageContext", "CreatedByPersonId" );
            CreateIndex( "cmsPageRoute", "ModifiedByPersonId" );
            CreateIndex( "cmsPageRoute", "CreatedByPersonId" );
            CreateIndex( "cmsPage", "ModifiedByPersonId" );
            CreateIndex( "cmsPage", "CreatedByPersonId" );
            CreateIndex( "cmsHtmlContent", "ModifiedByPersonId" );
            CreateIndex( "cmsHtmlContent", "CreatedByPersonId" );
            CreateIndex( "dbo.cmsBlock", "ModifiedByPersonId" );
            CreateIndex( "dbo.cmsBlock", "CreatedByPersonId" );
            CreateIndex( "dbo.cmsBlockType", "ModifiedByPersonId" );
            CreateIndex( "dbo.cmsBlockType", "CreatedByPersonId" );
            CreateIndex( "cmsFile", "ModifiedByPersonId" );
            CreateIndex( "cmsFile", "CreatedByPersonId" );
            CreateIndex( "financialTransactionDetail", "ModifiedByPersonId" );
            CreateIndex( "financialTransactionDetail", "CreatedByPersonId" );
            CreateIndex( "financialGateway", "ModifiedByPersonId" );
            CreateIndex( "financialGateway", "CreatedByPersonId" );
            CreateIndex( "financialBatch", "ModifiedByPersonId" );
            CreateIndex( "financialBatch", "CreatedByPersonId" );
            CreateIndex( "financialTransaction", "ModifiedByPersonId" );
            CreateIndex( "financialTransaction", "CreatedByPersonId" );
            CreateIndex( "financialFund", "ModifiedByPersonId" );
            CreateIndex( "financialFund", "CreatedByPersonId" );
            CreateIndex( "financialPledge", "ModifiedByPersonId" );
            CreateIndex( "financialPledge", "CreatedByPersonId" );
            CreateIndex( "dbo.crmCampus", "ModifiedByPersonId" );
            CreateIndex( "dbo.crmCampus", "CreatedByPersonId" );
            CreateIndex( "groupsGroupRole", "ModifiedByPersonId" );
            CreateIndex( "groupsGroupRole", "CreatedByPersonId" );
            CreateIndex( "groupsGroupType", "ModifiedByPersonId" );
            CreateIndex( "groupsGroupType", "CreatedByPersonId" );
            CreateIndex( "dbo.crmLocation", "ModifiedByPersonId" );
            CreateIndex( "dbo.crmLocation", "CreatedByPersonId" );
            CreateIndex( "groupsGroup", "ModifiedByPersonId" );
            CreateIndex( "groupsGroup", "CreatedByPersonId" );
            CreateIndex( "groupsMember", "ModifiedByPersonId" );
            CreateIndex( "groupsMember", "CreatedByPersonId" );
            CreateIndex( "coreAttributeValue", "ModifiedByPersonId" );
            CreateIndex( "coreAttributeValue", "CreatedByPersonId" );
            CreateIndex( "coreAttributeQualifier", "ModifiedByPersonId" );
            CreateIndex( "coreAttributeQualifier", "CreatedByPersonId" );
            CreateIndex( "coreAttribute", "ModifiedByPersonId" );
            CreateIndex( "coreAttribute", "CreatedByPersonId" );
            CreateIndex( "coreFieldType", "ModifiedByPersonId" );
            CreateIndex( "coreFieldType", "CreatedByPersonId" );
            CreateIndex( "coreDefinedType", "ModifiedByPersonId" );
            CreateIndex( "coreDefinedType", "CreatedByPersonId" );
            CreateIndex( "coreDefinedValue", "ModifiedByPersonId" );
            CreateIndex( "coreDefinedValue", "CreatedByPersonId" );
            CreateIndex( "crmPhoneNumber", "ModifiedByPersonId" );
            CreateIndex( "crmPhoneNumber", "CreatedByPersonId" );
            CreateIndex( "crmEmailTemplate", "ModifiedByPersonId" );
            CreateIndex( "crmEmailTemplate", "CreatedByPersonId" );
            CreateIndex( "cmsUser", "ModifiedByPersonId" );
            CreateIndex( "cmsUser", "CreatedByPersonId" );
            CreateIndex( "crmPerson", "ModifiedByPersonId" );
            CreateIndex( "crmPerson", "CreatedByPersonId" );
            CreateIndex( "cmsAuth", "ModifiedByPersonId" );
            CreateIndex( "cmsAuth", "CreatedByPersonId" );
            AddForeignKey( "dbo.utilJob", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.utilJob", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreTaggedItem", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.coreTaggedItem", "CreatedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.coreTag", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.coreTag", "CreatedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.coreMetricValue", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.coreMetricValue", "CreatedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.coreMetric", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.coreMetric", "CreatedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.coreExceptionLog", "PersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreEntityChange", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsSiteDomain", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsSiteDomain", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsSite", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsSite", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsPageContext", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsPageContext", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsPageRoute", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsPageRoute", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsPage", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsPage", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsHtmlContent", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsHtmlContent", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsBlock", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.cmsBlock", "CreatedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.cmsBlockType", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.cmsBlockType", "CreatedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.cmsFile", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsFile", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialTransactionDetail", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialTransactionDetail", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialGateway", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialGateway", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialBatch", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialBatch", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialTransaction", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialTransaction", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialFund", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialFund", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialPledge", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.financialPledge", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.crmCampus", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.crmCampus", "CreatedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.groupsGroupRole", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.groupsGroupRole", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.groupsGroupType", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.groupsGroupType", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.crmLocation", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.crmLocation", "CreatedByPersonId", "dbo.crmPerson", "Id" );
            AddForeignKey( "dbo.groupsGroup", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.groupsGroup", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.groupsMember", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.groupsMember", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreAttributeValue", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreAttributeValue", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreAttributeQualifier", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreAttributeQualifier", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreAttribute", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreAttribute", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreFieldType", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreFieldType", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreDefinedType", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreDefinedType", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreDefinedValue", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.coreDefinedValue", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.crmPhoneNumber", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.crmPhoneNumber", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.crmEmailTemplate", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.crmEmailTemplate", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsUser", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsUser", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.crmPerson", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.crmPerson", "CreatedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsAuth", "ModifiedByPersonId", "crmPerson", "Id" );
            AddForeignKey( "dbo.cmsAuth", "CreatedByPersonId", "crmPerson", "Id" );

            Sql( @"
ALTER PROC [dbo].[crmPerson_sp_Merge]
@OldId int, 
@NewId int

AS

DECLARE @OldGuid uniqueidentifier
DECLARE @NewGuid uniqueidentifier

SET @OldGuid = (SELECT [Guid] FROM crmPerson WHERE [Id] = @OldId)
SET @NewGuid = (SELECT [Guid] FROM crmPerson WHERE [Id] = @NewId)

IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
BEGIN

    BEGIN TRANSACTION

    INSERT INTO crmPersonMerged ([Id], [Guid], [CurrentId], [CurrentGuid])
    VALUES (@OldId, @OldGuid, @NewId, @NewGuid)

    DECLARE @Sql varchar(max)

    DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
    SELECT 
        ' UPDATE ' + tso.name +
        ' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
        ' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
    FROM sys.foreign_key_columns kc
    INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
    INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
    INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
    INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
    INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
    WHERE so.name = 'crmPerson'
    AND rac.name = 'Id'

    OPEN ForeignKeyCursor

    FETCH NEXT
    FROM ForeignKeyCursor
    INTO @Sql

    WHILE (@@FETCH_STATUS <> -1)
    BEGIN

        IF (@@FETCH_STATUS = 0)
        BEGIN

            EXEC(@Sql)
            
        END
        
        FETCH NEXT
        FROM ForeignKeyCursor
        INTO @Sql

    END

    CLOSE ForeignKeyCursor
    DEALLOCATE ForeignKeyCursor

    DELETE crmPerson
    WHERE [Id] = @OldId
    
    -- Delete any remaining Attribute Values
    DELETE AV
    FROM [coreAttribute] A
    INNER JOIN [coreAttributeValue] AV
        ON AV.[AttributeId] = A.[Id]
        AND AV.[EntityId] = @OldId
    WHERE A.[Entity] = 'Rock.CRM.Person'
    
    COMMIT TRANSACTION

END
" );

        }
    }
}
