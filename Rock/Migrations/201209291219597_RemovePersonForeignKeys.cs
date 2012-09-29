namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

	public partial class RemovePersonForeignKeys : DbMigration
	{
		public override void Up()
		{
			DropForeignKey( "cmsAuth", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsAuth", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "crmPerson", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "crmPerson", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "cmsUser", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsUser", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "crmEmailTemplate", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "crmEmailTemplate", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "crmPhoneNumber", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "crmPhoneNumber", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "coreDefinedValue", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "coreDefinedValue", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "coreDefinedType", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "coreDefinedType", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "coreFieldType", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "coreFieldType", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "coreAttribute", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "coreAttribute", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "coreAttributeQualifier", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "coreAttributeQualifier", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "coreAttributeValue", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "coreAttributeValue", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "groupsMember", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "groupsMember", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "groupsGroup", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "groupsGroup", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "dbo.crmLocation", "CreatedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.crmLocation", "ModifiedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "groupsGroupType", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "groupsGroupType", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "groupsGroupRole", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "groupsGroupRole", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "dbo.crmCampus", "CreatedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.crmCampus", "ModifiedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "financialPledge", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "financialPledge", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "financialFund", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "financialFund", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "financialTransaction", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "financialTransaction", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "financialBatch", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "financialBatch", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "financialGateway", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "financialGateway", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "financialTransactionDetail", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "financialTransactionDetail", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "cmsFile", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsFile", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "dbo.cmsBlockType", "CreatedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.cmsBlockType", "ModifiedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.cmsBlock", "CreatedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.cmsBlock", "ModifiedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "cmsHtmlContent", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsHtmlContent", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "cmsPage", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsPage", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "cmsPageRoute", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsPageRoute", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "cmsPageContext", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsPageContext", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "cmsSite", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsSite", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "cmsSiteDomain", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "cmsSiteDomain", "ModifiedByPersonId", "crmPerson" );
			DropForeignKey( "coreEntityChange", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "coreExceptionLog", "PersonId", "crmPerson" );
			DropForeignKey( "dbo.coreMetric", "CreatedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.coreMetric", "ModifiedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.coreMetricValue", "CreatedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.coreMetricValue", "ModifiedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.coreTag", "CreatedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.coreTag", "ModifiedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.coreTaggedItem", "CreatedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "dbo.coreTaggedItem", "ModifiedByPersonId", "dbo.crmPerson" );
			DropForeignKey( "utilJob", "CreatedByPersonId", "crmPerson" );
			DropForeignKey( "utilJob", "ModifiedByPersonId", "crmPerson" );
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
			AddForeignKey( "utilJob", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "utilJob", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "dbo.coreTaggedItem", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.coreTaggedItem", "CreatedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.coreTag", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.coreTag", "CreatedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.coreMetricValue", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.coreMetricValue", "CreatedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.coreMetric", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.coreMetric", "CreatedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "coreExceptionLog", "PersonId", "crmPerson", "Id" );
			AddForeignKey( "coreEntityChange", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsSiteDomain", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsSiteDomain", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsSite", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsSite", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsPageContext", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsPageContext", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsPageRoute", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsPageRoute", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsPage", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsPage", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsHtmlContent", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsHtmlContent", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "dbo.cmsBlock", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.cmsBlock", "CreatedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.cmsBlockType", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.cmsBlockType", "CreatedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "cmsFile", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsFile", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialTransactionDetail", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialTransactionDetail", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialGateway", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialGateway", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialBatch", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialBatch", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialTransaction", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialTransaction", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialFund", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialFund", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialPledge", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "financialPledge", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "dbo.crmCampus", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.crmCampus", "CreatedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "groupsGroupRole", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "groupsGroupRole", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "groupsGroupType", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "groupsGroupType", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "dbo.crmLocation", "ModifiedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "dbo.crmLocation", "CreatedByPersonId", "dbo.crmPerson", "Id" );
			AddForeignKey( "groupsGroup", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "groupsGroup", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "groupsMember", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "groupsMember", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreAttributeValue", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreAttributeValue", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreAttributeQualifier", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreAttributeQualifier", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreAttribute", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreAttribute", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreFieldType", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreFieldType", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreDefinedType", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreDefinedType", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreDefinedValue", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "coreDefinedValue", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "crmPhoneNumber", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "crmPhoneNumber", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "crmEmailTemplate", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "crmEmailTemplate", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsUser", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsUser", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "crmPerson", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "crmPerson", "CreatedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsAuth", "ModifiedByPersonId", "crmPerson", "Id" );
			AddForeignKey( "cmsAuth", "CreatedByPersonId", "crmPerson", "Id" );

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
