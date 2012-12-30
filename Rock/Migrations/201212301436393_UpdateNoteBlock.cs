//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class UpdateNoteBlock : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.DefinedValue", "DefinedTypeId", "dbo.DefinedType");
            DropIndex("dbo.DefinedValue", new[] { "DefinedTypeId" });
            AddForeignKey("dbo.DefinedValue", "DefinedTypeId", "dbo.DefinedType", "Id", cascadeDelete: true);
            CreateIndex("dbo.DefinedValue", "DefinedTypeId");

            // Delete the old note block
            DeleteBlockType( "6A0B3ED6-C6CA-40D4-91E0-B7B2823CC708" );

            // Create new note block type, block, and attributes
            AddBlockType( "Notes", "Context aware block for adding notes to an entity", "~/Blocks/Core/Notes.ascx", "599D274D-55C7-4DE6-BB2D-B334D4BD51BC" );
            AddBlockTypeAttribute( "599D274D-55C7-4DE6-BB2D-B334D4BD51BC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Entity Type", "ContextEntityType", "Filter", "Context Entity Type", 0, "", "451D5A66-5FCA-4D73-9558-C0DEB077649A" );
            AddBlockTypeAttribute( "599D274D-55C7-4DE6-BB2D-B334D4BD51BC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Type", "NoteType", "Behavior", "The note type name associated with the context entity to use (If it doesn't exist it will be created).", 1, "Notes", "998EB1EA-DE7A-4372-8D5D-2613F1AA40AF" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "599D274D-55C7-4DE6-BB2D-B334D4BD51BC", "Notes", "Notes", "CCEB85C0-45B4-4508-8331-DA59B7F573B6", 0 );
            AddBlockAttributeValue( "CCEB85C0-45B4-4508-8331-DA59B7F573B6", "451D5A66-5FCA-4D73-9558-C0DEB077649A", "Rock.Model.Person" );
            AddBlockAttributeValue( "CCEB85C0-45B4-4508-8331-DA59B7F573B6", "998EB1EA-DE7A-4372-8D5D-2613F1AA40AF", "Timeline" );

            Sql( @"
                DECLARE @Order int
                DECLARE @DefinedTypeId int

                -- Add Defined Type for note sourcetype
                SELECT @Order = ISNULL(MAX([order])+1,0) FROM [DefinedType] WHERE [Category] = 'Person';
                INSERT INTO [DefinedType] ([IsSystem],[Order],[Category],[Name],[Description],[Guid])
	                VALUES (1, @Order, 'Person', 'Timeline Sources', 'Sources of notes on person timeline', '504BE755-2919-4738-952F-3EDF8B0F561A')
                SET @DefinedTypeId = SCOPE_IDENTITY()

                -- Get the entitytype for Defined Value
                DECLARE @DefinedValueEntityTypeId int
                SELECT @DefinedValueEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedValue'
                IF @DefinedValueEntityTypeId IS NULL
                BEGIN
                    INSERT INTO [EntityType] ([Name], [Guid])
                    VALUES ('Rock.Model.DefinedValue', NEWID())
                    SET @DefinedValueEntityTypeId = SCOPE_IDENTITY()
                END

                -- Add IconClass attribute to the defined type
                DECLARE @TextFieldTypeId int
                SELECT @TextFieldTypeId = [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA'
                DECLARE @AttributeId int
                INSERT INTO [Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Category],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
	                VALUES (1, @TextFieldTypeId, @DefinedValueEntityTypeId, 'DefinedTypeId', CAST(@DefinedTypeId AS varchar), 'IconClass', 'Icon Class Name', '', 'The class name to use when rendering an icon for notes of this type', 0, 0, '', 0, 0, 'BABA5709-EAC1-4003-B48C-7ACA5E5BFB1C')
                SET @AttributeId = SCOPE_IDENTITY()

                -- Add note source defined values
                DECLARE @DefinedValueId int
                INSERT INTO [DefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[Guid])
	                VALUES (1, @DefinedTypeId, 0, 'Personal Note', 'Note manually entered by a logged-in user', '4318E9AC-B669-4AF7-AF88-EF580FC43C6A')
                SET @DefinedValueId = SCOPE_IDENTITY()
                INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
	                VALUES (1, @AttributeId, @DefinedValueId, 0, 'icon-comment', NEWID())

                INSERT INTO [DefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[Guid])
	                VALUES (1, @DefinedTypeId, 1, 'Event Registration', 'Note created when person registers for an event', 'BBADA8EF-23FC-4B46-B7A7-0F6D31F8C045')
                SET @DefinedValueId = SCOPE_IDENTITY()
                INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
	                VALUES (1, @AttributeId, @DefinedValueId, 0, 'icon-calendar', NEWID())

                INSERT INTO [DefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[Guid])
	                VALUES (1, @DefinedTypeId, 2, 'Communication Note', 'Note created when person is emailed a communication', '87BACB34-DB87-45E0-AB60-BFABF7CEECDB')
                SET @DefinedValueId = SCOPE_IDENTITY()
                INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
	                VALUES (1, @AttributeId, @DefinedValueId, 0, 'icon-envelope', NEWID())

                INSERT INTO [DefinedValue] ([IsSystem],[DefinedTypeId],[Order],[Name],[Description],[Guid])
	                VALUES (1, @DefinedTypeId, 3, 'Phone Note', 'Note created when a phone call is made with person', 'B54F9D90-9AF3-4E8A-8F33-9338C7C1287F')
                SET @DefinedValueId = SCOPE_IDENTITY()
                INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
	                VALUES (1, @AttributeId, @DefinedValueId, 0, 'icon-phone', NEWID())

                -- Get the entitytype for Person
                DECLARE @PersonEntityTypeId int
                SELECT @PersonEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person'
                IF @PersonEntityTypeId IS NULL
                BEGIN
                    INSERT INTO [EntityType] ([Name], [Guid])
                    VALUES ('Rock.Model.Person', NEWID())
                    SET @PersonEntityTypeId = SCOPE_IDENTITY()
                END

                -- Add Note Type for person timeline
                DECLARE @NoteTypeId int
                INSERT INTO [NoteType] ([IsSystem],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Name],[SourcesTypeId],[Guid])
            	    VALUES (1, @PersonEntityTypeId, '', '', 'Timeline', @DefinedTypeId, '7E53487C-D650-4D85-97E2-350EB8332763')
                SET @NoteTypeId = SCOPE_IDENTITY()
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                -- Delete icon attribute
                DELETE [Attribute] WHERE [Guid] = 'BABA5709-EAC1-4003-B48C-7ACA5E5BFB1C'
                
                -- Delete the note type
                DELETE [NoteType] WHERE [Guid] = '7E53487C-D650-4D85-97E2-350EB8332763'

                -- Delete the icon defined type
                DELETE [DefinedType] WHERE [Guid] = '504BE755-2919-4738-952F-3EDF8B0F561A'
" );

            // Remove the new note block type
            DeleteBlockType( "599D274D-55C7-4DE6-BB2D-B334D4BD51BC" );
            DeleteBlockAttribute( "451D5A66-5FCA-4D73-9558-C0DEB077649A" );
            DeleteBlockAttribute( "998EB1EA-DE7A-4372-8D5D-2613F1AA40AF" );

            // Add the old note block type, and block
            AddBlockType( "Person Notes", "Person notes (Person Detail Page)", "~/Blocks/Crm/PersonDetail/Notes.ascx", "6A0B3ED6-C6CA-40D4-91E0-B7B2823CC708" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "6A0B3ED6-C6CA-40D4-91E0-B7B2823CC708", "Notes", "Notes", "CCEB85C0-45B4-4508-8331-DA59B7F573B6", 0 );

            DropIndex( "dbo.DefinedValue", new[] { "DefinedTypeId" } );
            DropForeignKey("dbo.DefinedValue", "DefinedTypeId", "dbo.DefinedType");
            CreateIndex("dbo.DefinedValue", "DefinedTypeId");
            AddForeignKey("dbo.DefinedValue", "DefinedTypeId", "dbo.DefinedType", "Id");
        }
    }
}
