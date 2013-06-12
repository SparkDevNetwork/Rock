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
    public partial class AdditionalCheckinDataRedux : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

                DECLARE @TextFieldTypeId INT
                SET @TextFieldTypeId = (SELECT Id FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')

                -- Add the 'Grade Transition Date' (aka grade promotion date) Global Attribute
                IF NOT EXISTS ( SELECT Id FROM [Attribute] WHERE [Guid] = '265734A6-C888-45B4-A7A5-9A26478306B8' )
                BEGIN
	                INSERT INTO [Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Category], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid])
	                VALUES (0, @TextFieldTypeId, NULL, N'', N'', N'GradeTransitionDate', N'Grade Transition Date', N'Check-in', N'The date (mm/dd) when kids are moved to the next grade level.', 20, 0, N'6/1', 0, 0, '265734A6-C888-45B4-A7A5-9A26478306B8')
                END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                -- Remove the 'Grade Transition Date'
                IF EXISTS ( SELECT Id FROM [Attribute] WHERE [Guid] = '265734A6-C888-45B4-A7A5-9A26478306B8' )
                BEGIN
                    DELETE [Attribute] WHERE [Guid] = '265734A6-C888-45B4-A7A5-9A26478306B8'
                END
" );
        }
    }
}
