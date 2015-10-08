// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PersonAlias : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonAlias",
                c => new
                    {
                        Id = c.Int( nullable: false, identity: true ),
                        Name = c.String( maxLength: 200 ),
                        PersonId = c.Int( nullable: false ),
                        AliasPersonId = c.Int( nullable: false ),
                        AliasPersonGuid = c.Guid( nullable: false ),
                        Guid = c.Guid( nullable: false ),
                    } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Person", t => t.PersonId, cascadeDelete: true )
                .Index( t => t.PersonId );
            CreateIndex( "dbo.PersonAlias", "Name", false );
            CreateIndex( "dbo.PersonAlias", "AliasPersonId", true );
            CreateIndex( "dbo.PersonAlias", "Guid", true );
            DropTable( "dbo.PersonMerged" );

            try
            {
                // shift the starting Id for PersonAlias so minimize issues when PersonAliasId is confused with PersonId
                Sql( "DBCC CHECKIDENT (PersonAlias, RESEED, 10) " );
            }
            catch
            {
                // ignore if the database doesn't support that command (SQL Azure)
            }

            Sql( @"
    INSERT INTO [PersonAlias] ([PersonId],[AliasPersonId],[AliasPersonGuid],[Guid])
    SELECT [Id], [Id], [Guid], NEWID() FROM [Person]
");

            Sql( @"
    ALTER PROCEDURE [dbo].[spPersonMerge]
    @OldId int, 
    @NewId int,
    @DeleteOldPerson bit

    AS

    DECLARE @OldGuid uniqueidentifier
    DECLARE @NewGuid uniqueidentifier

    SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
    SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

    IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
    BEGIN

	    DECLARE @PersonEntityTypeId INT
	    SET @PersonEntityTypeId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )

	    DECLARE @PersonFieldTypeId INT
	    SET @PersonFieldTypeId = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )


	    --BEGIN TRANSACTION


	    -- Attribute Value
	    -----------------------------------------------------------------------------------------------
	    -- Update Attribute Values associated with person 
	    -- The new user's attribute value will only get updated if the old user has a value, and the 
	    -- new user does not (determining the correct value will eventually be decided by user in a UI)
	    UPDATE AVO
		    SET [EntityId] = @NewId
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AVO
		    ON AVO.[EntityId] = @OldId
		    AND AVO.[AttributeId] = A.[Id]
	    LEFT OUTER JOIN [AttributeValue] AVN
		    ON AVO.[EntityId] = @NewId
		    AND AVN.[AttributeId] = A.[Id]
	    WHERE A.[EntityTypeId] = @PersonEntityTypeId
	    AND AVN.[Id] IS NULL

	    -- Delete any attribute values that were not updated (due to new person already having existing 
	    -- value)
	    DELETE AV
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AV
		    ON AV.[EntityId] = @OldId
		    AND AV.[AttributeId] = A.[Id]
	    WHERE A.[EntityTypeId] = @PersonEntityTypeId

	    -- Update Attribute Values that have person as a value
	    -- NOTE: BECAUSE VALUE IS A VARCHAR(MAX) COLUMN WE CANT ADD AN INDEX FOR ATTRIBUTEID AND
	    -- VALUE.  THIS UPDATE COULD POTENTIALLY BE A BOTTLE-NECK FOR MERGES
	    UPDATE AV
		    SET [Value] = CAST( @NewGuid AS VARCHAR(64) )
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AV
		    ON AV.[AttributeId] = A.[Id]
		    AND AV.[Value] = CAST( @OldGuid AS VARCHAR(64) )
	    WHERE A.[FieldTypeId] = @PersonFieldTypeId


	    -- Authorization
	    -----------------------------------------------------------------------------------------------
	    -- Update any authorizations associated to old person that do not already have a matching 
	    -- authorization for the new person
	    UPDATE AO
		    SET [PersonId] = @NewId
	    FROM [Auth] AO
	    LEFT OUTER JOIN [Auth] AN
		    ON AN.[PersonId] = @NewId
		    AND AN.[EntityTypeId] = AO.[EntityTypeId]
		    AND AN.[EntityId] = AO.[EntityId]
		    AND AN.[Action] = AO.[Action]
		    AND AN.[AllowOrDeny] = AO.[AllowOrDeny]
		    AND AN.[SpecialRole] = AO.[SpecialRole]
	    WHERE AO.[PersonId] = @OldId
	    AND AN.[Id] IS NULL

	    -- Delete any authorizations not updated to new person
	    DELETE [Auth]
	    WHERE [PersonId] = @OldId


	    -- Category
	    -----------------------------------------------------------------------------------------------
	    -- Currently UI does not allow categorizing people, but if it does in the future, would need 
	    -- to add script to handle merge


	    -- Communication Recipient
	    -----------------------------------------------------------------------------------------------
	    -- Update any communication recipients associated to old person to the new person where the new
	    -- person does not already have the recipient record
	    UPDATE CRO
		    SET [PersonId] = @NewId
	    FROM [CommunicationRecipient] CRO
	    LEFT OUTER JOIN [CommunicationRecipient] CRN
		    ON CRN.[CommunicationId] = CRO.[CommunicationId]
		    AND CRN.[PersonId] = @NewId
	    WHERE CRO.[PersonId] = @OldId
	    AND CRN.[Id] IS NULL

	    -- Delete any remaining recipents that were not updated
	    DELETE [CommunicationRecipient]
	    WHERE [PersonId] = @OldId

	    -- Group Member
	    -----------------------------------------------------------------------------------------------
	    -- Update any group members associated to old person to the new person where the new is not 
	    -- already in the group with the same role
	    UPDATE GMO
		    SET [PersonId] = @NewId
	    FROM [GroupMember] GMO
	    LEFT OUTER JOIN [GroupMember] GMN
		    ON GMN.[GroupId] = GMO.[GroupId]
		    AND GMN.[PersonId] = @NewId
		    AND GMN.[GroupRoleId] = GMO.[GroupRoleId] -- If person can be in group twice with diff role
	    WHERE GMO.[PersonId] = @OldId
	    AND GMN.[Id] IS NULL

	    -- Delete any group members not updated (already existed with new id)
	    DELETE [GroupMember]
	    WHERE [PersonId] = @OldId


	    -- Note
	    -----------------------------------------------------------------------------------------------
	    -- Update any note that is associated to the old person to be associated to the new person
	    UPDATE N
		    SET [EntityId] = @NewId
	    FROM [NoteType] NT
	    INNER JOIN [Note] N
		    ON N.[NoteTypeId] = NT.[Id]
		    AND N.[EntityId] = @OldId
	    WHERE NT.[EntityTypeId] = @PersonEntityTypeId


	    -- Phone Numbers
	    -----------------------------------------------------------------------------------------------
	    -- Update any phone numbers associated to the old person that do not already exist for the new
	    -- person
	    UPDATE PNO
		    SET [PersonId] = @NewId
	    FROM [PhoneNumber] PNO
	    INNER JOIN [PhoneNumber] PNN
		    ON PNN.[PersonId] = @NewId
		    AND PNN.[Number] = PNO.[Number]
		    AND PNN.[Extension] = PNO.[Extension]
		    AND PNN.[NumberTypeValueId] = PNO.[NumberTypeValueId]
	    WHERE PNO.[PersonId] = @OldId
	    AND PNN.[Id] IS NULL

	    -- Delete any numbers not updated (new person already had same number)
	    DELETE [PhoneNumber]
	    WHERE [PersonId] = @OldId


	    -- Tags
	    -----------------------------------------------------------------------------------------------
	    -- Update any tags associated to the old person to be associated to the new person as long as 
	    -- same tag does not already exist for new person
	    UPDATE TIO
		    SET [EntityGuid] = @NewGuid
	    FROM [Tag] T
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
		    AND TIO.[EntityGuid] = @OldGuid
	    LEFT OUTER JOIN [TaggedItem] TIN
		    ON TIN.[TagId] = T.[Id]
		    AND TIN.[EntityGuid] = @NewGuid
	    WHERE T.[EntityTypeId] = @PersonEntityTypeId
	    AND TIN.[Id] IS NULL

	    -- Delete any tagged items still associated with old person (new person had same tag)
	    DELETE TIO
	    FROM [Tag] T
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
		    AND TIO.[EntityGuid] = @OldGuid
	    WHERE T.[EntityTypeId] = @PersonEntityTypeId

	    -- If old person and new person have tags with the same name for the same entity type,
	    -- update the old person's tagged items to use the new person's tag
	    UPDATE TIO
		    SET [TagId] = TIN.[Id]
	    FROM [Tag] T
	    INNER JOIN [Tag] TN
		    ON TN.[EntityTypeId] = T.[EntityTypeId]
		    AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		    AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		    AND TN.[Name] = T.[Name]
		    AND TN.[OwnerId] = @NewId
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
	    LEFT OUTER JOIN [TaggedItem] TIN
		    ON TIN.[TagId] = TN.[Id]
	    WHERE T.[OwnerId] = @OldId
	    AND TIN.[Id] IS NULL

	    -- Delete any of the old person's tags that have the same name and are associated to same 
	    -- entity type as a tag used bo the new person
	    DELETE T
	    FROM [Tag] T
	    INNER JOIN [Tag] TN
		    ON TN.[EntityTypeId] = T.[EntityTypeId]
		    AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		    AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		    AND TN.[Name] = T.[Name]
		    AND TN.[OwnerId] = @NewId
	    WHERE T.[OwnerId] = @OldId


	    -- Remaining Tables
	    -----------------------------------------------------------------------------------------------
	    -- Update any column on any table that has a foreign key relationship to the Person table's Id
	    -- column  

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
	    WHERE so.name = 'Person'
	    AND rac.name = 'Id'
	    AND tso.name NOT IN (
		     'Auth'
		    ,'CommunicationRecipient'
		    ,'GroupMember'
		    ,'PhoneNumber'
	    )

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


	    -- Person
	    -----------------------------------------------------------------------------------------------
	    -- Optionally delete the old person record.  By this time it should not have any relationships 
	    -- with other tables (if Rock is being synced with other data source, the sync may handle the
	    -- delete)
	
	    IF @DeleteOldPerson = 1 
	    BEGIN
		    DELETE Person
		    WHERE [Id] = @OldId
	    END
	
	    --COMMIT TRANSACTION


    END
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateTable(
                "dbo.PersonMerged",
                c => new
                    {
                        Id = c.Int( nullable: false, identity: true ),
                        PreviousPersonId = c.Int( nullable: false ),
                        PreviousPersonGuid = c.Guid( nullable: false ),
                        NewPersonId = c.Int( nullable: false ),
                        NewPersonGuid = c.Guid( nullable: false ),
                        Guid = c.Guid( nullable: false ),
                    } )
                .PrimaryKey( t => t.Id );

            DropForeignKey( "dbo.PersonAlias", "PersonId", "dbo.Person" );
            DropIndex( "dbo.PersonAlias", new[] { "PersonId" } );
            DropIndex( "dbo.PersonAlias", new[] { "Name" } );
            DropIndex( "dbo.PersonAlias", new[] { "AliasPersonId" } );
            DropIndex( "dbo.PersonAlias", new[] { "Guid" } );
            DropTable( "dbo.PersonAlias" );

            Sql( @"
    ALTER PROCEDURE [dbo].[spPersonMerge]
    @OldId int, 
    @NewId int, 
    @DeleteOldPerson bit

    AS

    DECLARE @OldGuid uniqueidentifier
    DECLARE @NewGuid uniqueidentifier

    SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
    SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

    IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
    BEGIN

	    DECLARE @PersonEntityTypeId INT
	    SET @PersonEntityTypeId = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )

	    DECLARE @PersonFieldTypeId INT
	    SET @PersonFieldTypeId = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )


	    --BEGIN TRANSACTION


	    -- Attribute Value
	    -----------------------------------------------------------------------------------------------
	    -- Update Attribute Values associated with person 
	    -- The new user's attribute value will only get updated if the old user has a value, and the 
	    -- new user does not (determining the correct value will eventually be decided by user in a UI)
	    UPDATE AVO
		    SET [EntityId] = @NewId
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AVO
		    ON AVO.[EntityId] = @OldId
		    AND AVO.[AttributeId] = A.[Id]
	    LEFT OUTER JOIN [AttributeValue] AVN
		    ON AVO.[EntityId] = @NewId
		    AND AVN.[AttributeId] = A.[Id]
	    WHERE A.[EntityTypeId] = @PersonEntityTypeId
	    AND AVN.[Id] IS NULL

	    -- Delete any attribute values that were not updated (due to new person already having existing 
	    -- value)
	    DELETE AV
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AV
		    ON AV.[EntityId] = @OldId
		    AND AV.[AttributeId] = A.[Id]
	    WHERE A.[EntityTypeId] = @PersonEntityTypeId

	    -- Update Attribute Values that have person as a value
	    -- NOTE: BECAUSE VALUE IS A VARCHAR(MAX) COLUMN WE CANT ADD AN INDEX FOR ATTRIBUTEID AND
	    -- VALUE.  THIS UPDATE COULD POTENTIALLY BE A BOTTLE-NECK FOR MERGES
	    UPDATE AV
		    SET [Value] = CAST( @NewGuid AS VARCHAR(64) )
	    FROM [Attribute] A
	    INNER JOIN [AttributeValue] AV
		    ON AV.[AttributeId] = A.[Id]
		    AND AV.[Value] = CAST( @OldGuid AS VARCHAR(64) )
	    WHERE A.[FieldTypeId] = @PersonFieldTypeId


	    -- Authorization
	    -----------------------------------------------------------------------------------------------
	    -- Update any authorizations associated to old person that do not already have a matching 
	    -- authorization for the new person
	    UPDATE AO
		    SET [PersonId] = @NewId
	    FROM [Auth] AO
	    LEFT OUTER JOIN [Auth] AN
		    ON AN.[PersonId] = @NewId
		    AND AN.[EntityTypeId] = AO.[EntityTypeId]
		    AND AN.[EntityId] = AO.[EntityId]
		    AND AN.[Action] = AO.[Action]
		    AND AN.[AllowOrDeny] = AO.[AllowOrDeny]
		    AND AN.[SpecialRole] = AO.[SpecialRole]
	    WHERE AO.[PersonId] = @OldId
	    AND AN.[Id] IS NULL

	    -- Delete any authorizations not updated to new person
	    DELETE [Auth]
	    WHERE [PersonId] = @OldId


	    -- Category
	    -----------------------------------------------------------------------------------------------
	    -- Currently UI does not allow categorizing people, but if it does in the future, would need 
	    -- to add script to handle merge


	    -- Communication Recipient
	    -----------------------------------------------------------------------------------------------
	    -- Update any communication recipients associated to old person to the new person where the new
	    -- person does not already have the recipient record
	    UPDATE CRO
		    SET [PersonId] = @NewId
	    FROM [CommunicationRecipient] CRO
	    LEFT OUTER JOIN [CommunicationRecipient] CRN
		    ON CRN.[CommunicationId] = CRO.[CommunicationId]
		    AND CRN.[PersonId] = @NewId
	    WHERE CRO.[PersonId] = @OldId
	    AND CRN.[Id] IS NULL

	    -- Delete any remaining recipents that were not updated
	    DELETE [CommunicationRecipient]
	    WHERE [PersonId] = @OldId

	    -- Group Member
	    -----------------------------------------------------------------------------------------------
	    -- Update any group members associated to old person to the new person where the new is not 
	    -- already in the group with the same role
	    UPDATE GMO
		    SET [PersonId] = @NewId
	    FROM [GroupMember] GMO
	    LEFT OUTER JOIN [GroupMember] GMN
		    ON GMN.[GroupId] = GMO.[GroupId]
		    AND GMN.[PersonId] = @NewId
		    AND GMN.[GroupRoleId] = GMO.[GroupRoleId] -- If person can be in group twice with diff role
	    WHERE GMO.[PersonId] = @OldId
	    AND GMN.[Id] IS NULL

	    -- Delete any group members not updated (already existed with new id)
	    DELETE [GroupMember]
	    WHERE [PersonId] = @OldId


	    -- Note
	    -----------------------------------------------------------------------------------------------
	    -- Update any note that is associated to the old person to be associated to the new person
	    UPDATE N
		    SET [EntityId] = @NewId
	    FROM [NoteType] NT
	    INNER JOIN [Note] N
		    ON N.[NoteTypeId] = NT.[Id]
		    AND N.[EntityId] = @OldId
	    WHERE NT.[EntityTypeId] = @PersonEntityTypeId


	    -- Phone Numbers
	    -----------------------------------------------------------------------------------------------
	    -- Update any phone numbers associated to the old person that do not already exist for the new
	    -- person
	    UPDATE PNO
		    SET [PersonId] = @NewId
	    FROM [PhoneNumber] PNO
	    INNER JOIN [PhoneNumber] PNN
		    ON PNN.[PersonId] = @NewId
		    AND PNN.[Number] = PNO.[Number]
		    AND PNN.[Extension] = PNO.[Extension]
		    AND PNN.[NumberTypeValueId] = PNO.[NumberTypeValueId]
	    WHERE PNO.[PersonId] = @OldId
	    AND PNN.[Id] IS NULL

	    -- Delete any numbers not updated (new person already had same number)
	    DELETE [PhoneNumber]
	    WHERE [PersonId] = @OldId


	    -- Tags
	    -----------------------------------------------------------------------------------------------
	    -- Update any tags associated to the old person to be associated to the new person as long as 
	    -- same tag does not already exist for new person
	    UPDATE TIO
		    SET [EntityGuid] = @NewGuid
	    FROM [Tag] T
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
		    AND TIO.[EntityGuid] = @OldGuid
	    LEFT OUTER JOIN [TaggedItem] TIN
		    ON TIN.[TagId] = T.[Id]
		    AND TIN.[EntityGuid] = @NewGuid
	    WHERE T.[EntityTypeId] = @PersonEntityTypeId
	    AND TIN.[Id] IS NULL

	    -- Delete any tagged items still associated with old person (new person had same tag)
	    DELETE TIO
	    FROM [Tag] T
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
		    AND TIO.[EntityGuid] = @OldGuid
	    WHERE T.[EntityTypeId] = @PersonEntityTypeId

	    -- If old person and new person have tags with the same name for the same entity type,
	    -- update the old person's tagged items to use the new person's tag
	    UPDATE TIO
		    SET [TagId] = TIN.[Id]
	    FROM [Tag] T
	    INNER JOIN [Tag] TN
		    ON TN.[EntityTypeId] = T.[EntityTypeId]
		    AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		    AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		    AND TN.[Name] = T.[Name]
		    AND TN.[OwnerId] = @NewId
	    INNER JOIN [TaggedItem] TIO
		    ON TIO.[TagId] = T.[Id]
	    LEFT OUTER JOIN [TaggedItem] TIN
		    ON TIN.[TagId] = TN.[Id]
	    WHERE T.[OwnerId] = @OldId
	    AND TIN.[Id] IS NULL

	    -- Delete any of the old person's tags that have the same name and are associated to same 
	    -- entity type as a tag used bo the new person
	    DELETE T
	    FROM [Tag] T
	    INNER JOIN [Tag] TN
		    ON TN.[EntityTypeId] = T.[EntityTypeId]
		    AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
		    AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
		    AND TN.[Name] = T.[Name]
		    AND TN.[OwnerId] = @NewId
	    WHERE T.[OwnerId] = @OldId


	    -- Person Merged
	    -----------------------------------------------------------------------------------------------
	    -- Add a record to indicate that person was merged with old and new id's and guid's
	    INSERT INTO PersonMerged (
		     [Guid]
		    ,[PreviousPersonId]
		    ,[PreviousPersonGuid]
		    ,[NewPersonId]
		    ,[NewPersonGuid]
	    )
	    VALUES (
		     NEWID()
		    ,@OldId
		    ,@OldGuid
		    ,@NewId
		    ,@NewGuid
	    )


	    -- Remaining Tables
	    -----------------------------------------------------------------------------------------------
	    -- Update any column on any table that has a foreign key relationship to the Person table's Id
	    -- column  

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
	    WHERE so.name = 'Person'
	    AND rac.name = 'Id'
	    AND tso.name NOT IN (
		     'Auth'
		    ,'CommunicationRecipient'
		    ,'GroupMember'
		    ,'PhoneNumber'
	    )

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


	    -- Person
	    -----------------------------------------------------------------------------------------------
	    -- Optionally delete the old person record.  By this time it should not have any relationships 
	    -- with other tables (if Rock is being synced with other data source, the sync may handle the
	    -- delete)
	
	    IF @DeleteOldPerson = 1 
	    BEGIN
		    DELETE Person
		    WHERE [Id] = @OldId
	    END
	
	    --COMMIT TRANSACTION

    END
" );
        }
    }
}
