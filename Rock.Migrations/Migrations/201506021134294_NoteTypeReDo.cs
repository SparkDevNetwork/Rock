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
    public partial class NoteTypeReDo : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.NoteType", "UserSelectable", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.NoteType", "CssClass", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.NoteType", "IconCssClass", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.NoteType", "Order", c => c.Int( nullable: false ) );

            Sql( @"
    -- Add new note types for all the existing notetype sources
    DECLARE @IconAttributeId int = ISNULL ( ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'BABA5709-EAC1-4003-B48C-7ACA5E5BFB1C' ), -99 )
    INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Guid], [IconCssClass], [Order], [UserSelectable] )
	SELECT 0, T.[EntityTypeId], '', '', DV.[Value], DV.[Guid], AV.[Value], DV.[Order], 0
	FROM [NoteType] T
	INNER JOIN [DefinedType] DT ON DT.[Id] = T.[SourcesTypeId]
	INNER JOIN [DefinedValue] DV ON DV.[DefinedTypeId] = DT.[Id]
	LEFT OUTER JOIN [AttributeValue] AV ON AV.[AttributeId] = @IconAttributeId AND AV.[EntityId] = DV.[Id]

	-- Update any notes with null sources to point to first note type with same entity type
	UPDATE N 
		SET [NoteTypeId] = ( 
			SELECT TOP 1 [Id] 
			FROM [NoteType] 
			WHERE [EntityTypeId] = T.[EntityTypeId]
			ORDER BY [IsSystem], [Order] 
		)
	FROM [Note] N
	INNER JOIN [NoteType] T ON T.[Id] = N.[NoteTypeId]
	WHERE N.[SourceTypeValueId] IS NULL

    -- Update notes with a source to point to new note type 
    UPDATE N
    SET [NoteTypeId] = T.[Id]
    FROM [Note] N
    INNER JOIN [DefinedValue] V ON V.[Id] = N.[SourceTypeValueId]
    INNER JOIN [NoteType] T ON T.[Guid] = V.[Guid]

    -- Update all note sources to null
    UPDATE [Note]
    SET [SourceTypeValueId] = NULL

	-- Get the source defined types 
	DECLARE @DefinedTypes TABLE (  ID int )
	INSERT INTO @DefinedTypes
	SELECT [SourcesTypeId] 
	FROM [NoteType] 
	WHERE [SourcesTypeId] IS NOT NULL

    -- Delete the note types that had a source since there is not new note types for those and the notes were updated to point to new note types
    DELETE [NoteType]
    WHERE [SourcesTypeId] IS NOT NULL
    AND [Id] NOT IN ( SELECT DISTINCT [NoteTypeId] FROM [Note] )

	-- Clear the source type on remaining note types that still have notes
	UPDATE [NoteType]
	SET [SourcesTypeId] = NULL

    -- Delete the note type source defined types
    DELETE [DefinedType]
    WHERE [Id] IN ( SELECT [Id] FROM @DefinedTypes )

    -- Set the system notes to have the correct Guid and be selectable
    DECLARE @EntityTypeId int
    DECLARE @NoteTypeId int
    SET @EntityTypeId  = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )
	IF @EntityTypeId IS NOT NULL
	BEGIN
		Set @NoteTypeId = ( SELECT TOP 1 [ID] FROM [NoteType] WHERE [EntityTypeId] = @EntityTypeId ORDER BY [Order] )
		IF @NoteTypeId IS NOT NULL
		BEGIN
			UPDATE [NoteType] SET
				[Guid] = '66A1B9D7-7EFA-40F3-9415-E54437977D60',
				[IsSystem] = 1,
				[UserSelectable] = 1,
                [IconCssClass] = 'fa fa-comment'
			WHERE [Id] = @NoteTypeId
		END
		ELSE
		BEGIN
			INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Guid], [IconCssClass], [Order], [UserSelectable] )
			VALUES ( 1, @EntityTypeId, '', '', 'Personal Note', '66A1B9D7-7EFA-40F3-9415-E54437977D60', 'fa fa-comment', 0, 1 )
		END
	END

    SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PrayerRequest' )    
	IF @EntityTypeId IS NOT NULL
	BEGIN
		SET @NoteTypeId = ( SELECT TOP 1 [ID] FROM [NoteType] WHERE [EntityTypeId] = @EntityTypeId ORDER BY [Order] )
		IF @NoteTypeId IS NOT NULL
		BEGIN
			UPDATE [NoteType] SET
				[Guid] = '0EBABD75-0890-4756-A9EE-62626282BB5D',
				[IsSystem] = 1,
				[UserSelectable] = 1,
                [IconCssClass] = 'fa fa-comment'
			WHERE [Id] = @NoteTypeId
		END
		ELSE
		BEGIN
			INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Guid], [IconCssClass], [Order], [UserSelectable] )
			VALUES ( 1, @EntityTypeId, '', '', 'Prayer Comment', '0EBABD75-0890-4756-A9EE-62626282BB5D', 'fa fa-comment', 0, 1 )
		END
	END

    SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Workflow' )
	IF @EntityTypeId IS NOT NULL
	BEGIN
		SET @NoteTypeId = ( SELECT TOP 1 [ID] FROM [NoteType] WHERE [EntityTypeId] = @EntityTypeId ORDER BY [Order] )
		IF @NoteTypeId IS NOT NULL
		BEGIN
			UPDATE [NoteType] SET
				[Guid] = 'A6CE445C-3B49-4401-82E6-312BF7946A6B',
				[IsSystem] = 1,
				[UserSelectable] = 1,
                [IconCssClass] = 'fa fa-comment'
			WHERE [Id] = @NoteTypeId
		END
		ELSE
		BEGIN
			INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Guid], [IconCssClass], [Order], [UserSelectable] )
			VALUES ( 1, @EntityTypeId, '', '', 'User Note', 'A6CE445C-3B49-4401-82E6-312BF7946A6B', 'fa fa-comment', 0, 1 )
		END
	END

    SET @EntityTypeId = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.FinancialScheduledTransaction' )
	IF @EntityTypeId IS NOT NULL
	BEGIN
		SET @NoteTypeId = ( SELECT TOP 1 [ID] FROM [NoteType] WHERE [EntityTypeId] = @EntityTypeId ORDER BY [Order] )
		IF @NoteTypeId IS NOT NULL
		BEGIN
			UPDATE [NoteType] SET
				[Guid] = '360CFFE2-7FE3-4B0B-85A7-BFDACC9AF588',
				[IsSystem] = 1,
				[UserSelectable] = 1,
                [IconCssClass] = 'fa fa-comment'
			WHERE [Id] = @NoteTypeId
		END
		ELSE
		BEGIN
			INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Guid], [IconCssClass], [Order], [UserSelectable] )
			VALUES ( 1, @EntityTypeId, '', '', 'Note', '360CFFE2-7FE3-4B0B-85A7-BFDACC9AF588', 'fa fa-comment', 0, 1 )
		END
	END

    -- Add an initial css class for each note type
    UPDATE [NoteType]
    SET [CssClass] = 'note-' + LOWER( REPLACE( [Name], ' ', '' ) )
" );

            DropForeignKey( "dbo.NoteType", "SourcesTypeId", "dbo.DefinedType" );
            DropForeignKey("dbo.Note", "SourceTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.Note", new[] { "SourceTypeValueId" });
            DropIndex("dbo.NoteType", new[] { "SourcesTypeId" });
            DropColumn("dbo.Note", "SourceTypeValueId");
            DropColumn("dbo.NoteType", "SourcesTypeId");

            RockMigrationHelper.UpdateFieldType( "Note Type", "", "Rock", "Rock.Field.Types.NoteTypeFieldType", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571" );
            RockMigrationHelper.UpdateFieldType( "Note Types", "", "Rock", "Rock.Field.Types.NoteTypesFieldType", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E" );

            Sql( @"
    -- Update the AddWorkflowNote action attribute
    DECLARE @AddWorkflowNoteEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.AddWorkflowNote' )
    DECLARE @NoteTypeFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = 'E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571' )
    IF @AddWorkflowNoteEntityTypeId IS NOT NULL AND @NoteTypeFieldTypeId IS NOT NULL
    BEGIN

	    UPDATE [Attribute] SET 
		    [FieldTypeId] = @NoteTypeFieldTypeId,
		    [Description] = 'The type of note to add.',
		    [DefaultValue] = 'A6CE445C-3B49-4401-82E6-312BF7946A6B'
	    WHERE [EntityTypeId] = @AddWorkflowNoteEntityTypeId
	    AND [Key] = 'NoteType'

	    UPDATE Q SET 
		    [Key] = 'entityTypeName',
		    [Value] = 'Rock.Model.Workflow'
	    FROM [Attribute] A
	    INNER JOIN [AttributeQualifier] Q
		    ON Q.[AttributeId] = A.[Id]
		    AND Q.[Key] = 'definedtype'
	    WHERE A.[EntityTypeId] = @AddWorkflowNoteEntityTypeId
	    AND A.[Key] = 'NoteType'
    END

    -- Update the AddWorkflowNote action attribute
    DECLARE @PersonNoteAddEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.PersonNoteAdd' )
    IF @PersonNoteAddEntityTypeId IS NOT NULL AND @NoteTypeFieldTypeId IS NOT NULL
    BEGIN

	    UPDATE [Attribute] SET 
            [Name] = 'Note Type',
            [Key] = 'NoteType',
		    [FieldTypeId] = @NoteTypeFieldTypeId,
		    [Description] = 'The type of note to add.',
		    [DefaultValue] = '66A1B9D7-7EFA-40F3-9415-E54437977D60'
	    WHERE [EntityTypeId] = @PersonNoteAddEntityTypeId
	    AND [Key] = 'SourceTypeType'

	    UPDATE Q SET 
		    [Key] = 'entityTypeName',
		    [Value] = 'Rock.Model.Workflow'
	    FROM [Attribute] A
	    INNER JOIN [AttributeQualifier] Q
		    ON Q.[AttributeId] = A.[Id]
		    AND Q.[Key] = 'definedtype'
	    WHERE A.[EntityTypeId] = @AddWorkflowNoteEntityTypeId
	    AND A.[Key] = 'NoteType'
    END
" );

            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Note Types", "", "B0E5876F-E29E-477B-8874-482DEDD3A6C5", "fa fa-edit" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Note Types", "Allows note types to be managed.", "~/Blocks/Core/NoteTypes.ascx", "Core", "44D2DAB8-6DCA-4DC3-B1FF-52DA224B2D5C" );
            RockMigrationHelper.AddBlock( "B0E5876F-E29E-477B-8874-482DEDD3A6C5", "", "44D2DAB8-6DCA-4DC3-B1FF-52DA224B2D5C", "Note Types", "Main", "", "", 0, "F3805956-9A24-4FBF-8370-F3D29D788445" );

            Sql( @"
    -- Update note type security so that staff/admins have rights to edit all note types which is what is needed now to add a note
    DECLARE @NoteTypeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.NoteType' )
    IF @NoteTypeEntityTypeId IS NOT NULL 
    BEGIN

	    DECLARE @StaffRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4' )
	    IF @StaffRoleId IS NOT NULL
	    BEGIN
		    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		    SELECT @NoteTypeEntityTypeId, [Id], 0, 'Edit', 'A', 0, @StaffRoleId, NEWID()
		    FROM [NoteType]
	    END

	    DECLARE @AdminRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' )
	    IF @StaffRoleId IS NOT NULL
	    BEGIN
		    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		    SELECT @NoteTypeEntityTypeId, [Id], 1, 'Edit', 'A', 0, @AdminRoleId, NEWID()
		    FROM [NoteType]
	    END

	    DECLARE @StaffLikeRoleId int = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' )
	    IF @StaffRoleId IS NOT NULL
	    BEGIN
		    INSERT INTO [Auth] ( [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid] )
		    SELECT @NoteTypeEntityTypeId, [Id], 2, 'Edit', 'A', 0, @StaffLikeRoleId, NEWID()
		    FROM [NoteType]
	    END

    END

    -- Give users who had 'Edit' rights to a specific note to have 'Administrate' rights since that is now what is needed to edit an existing note
    DECLARE @NoteEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Note' )
    UPDATE E SET [Action] = 'Administrate'
    FROM [Auth] E
    LEFT OUTER JOIN [Auth] A
	    ON A.[EntityTypeId] = E.[EntityTypeId]
	    AND A.[EntityId] = E.[EntityId]
	    AND A.[Action] = 'Administrate'
    WHERE E.[EntityTypeId] = @NoteEntityTypeId
    AND E.[GroupId] IS NULL
    AND E.[PersonAliasId] IS NOT NULL
    AND E.[Action] = 'Edit'
    AND A.[Id] IS NULL
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Note Types, from Page: Note Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F3805956-9A24-4FBF-8370-F3D29D788445" );
            RockMigrationHelper.DeleteBlockType( "44D2DAB8-6DCA-4DC3-B1FF-52DA224B2D5C" ); // Note Types
            RockMigrationHelper.DeletePage( "B0E5876F-E29E-477B-8874-482DEDD3A6C5" ); //  Page: Note Types, Layout: Full Width, Site: Rock RMS

            AddColumn("dbo.NoteType", "SourcesTypeId", c => c.Int());
            AddColumn("dbo.Note", "SourceTypeValueId", c => c.Int());
            DropColumn("dbo.NoteType", "Order");
            DropColumn("dbo.NoteType", "IconCssClass");
            DropColumn("dbo.NoteType", "CssClass");
            DropColumn("dbo.NoteType", "UserSelectable");
            CreateIndex("dbo.NoteType", "SourcesTypeId");
            CreateIndex("dbo.Note", "SourceTypeValueId");
            AddForeignKey("dbo.Note", "SourceTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.NoteType", "SourcesTypeId", "dbo.DefinedType", "Id");
        }
    }
}
