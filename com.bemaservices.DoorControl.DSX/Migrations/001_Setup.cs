using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;

namespace com.bemaservices.DoorControl.DSX.Migrations
{
    [MigrationNumber( 1, "1.8.0" )]
    public class Setup : Migration
    {
        public override void Up()
        {
            /* This migration does a lot of stuff.
             * ------------------------------------
             * Creates Defined Type and Defined Values
             * Creates Attribute Matrix, Attributes, and Attribute Qualifiers
             */

            // Creating Defined Type for DSX Actions
            RockMigrationHelper.AddDefinedType( "Room Management", "DSX Actions", "Stories all the Actions for DSX", com.bemaservices.DoorControl.DSX.SystemGuid.DefinedType.DSX_ACTIONS );

            // Adding DSX Actions
            RockMigrationHelper.AddDefinedValue( com.bemaservices.DoorControl.DSX.SystemGuid.DefinedType.DSX_ACTIONS, "Unlock", "Used to unlock a door" );
            RockMigrationHelper.AddDefinedValue( com.bemaservices.DoorControl.DSX.SystemGuid.DefinedType.DSX_ACTIONS, "Lock", "Used to lock a door" );
            RockMigrationHelper.AddDefinedValue( com.bemaservices.DoorControl.DSX.SystemGuid.DefinedType.DSX_ACTIONS, "TimeZone", "Used to reset the door to previous schedule" );

            // Creating Location Attribute for storing DSX Override Groups
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Location", Rock.SystemGuid.FieldType.TEXT, "", "", "DSX Override Group", "", "This is the DSX Override Group used for automatically opening doors.", 0, "", SystemGuid.Attribute.LOCATION_OVERRIDE_GROUP );

            var createReservationAttributeMatrixAndValues = string.Format( @"
                -- Getting a few Ids
                DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
                DECLARE @DateTimeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Class] = 'Rock.Field.Types.DateTimeFieldType');
                DECLARE @DefinedValueFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Class] = 'Rock.Field.Types.DefinedValueFieldType');

                -- Checking if AttributeMatrixTemplate exists
                DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = TRY_CAST('{0}' AS uniqueidentifier))
                IF ISNULL(@AttributeMatrixTemplateId, '') = ''
                BEGIN
	                -- Creating Attribute Matrix
	                INSERT INTO [dbo].[AttributeMatrixTemplate] (
		                [Name], 
		                [Description],
		                [IsActive],
		                [FormattedLava],
		                [Guid]
	                )
	                VALUES (
		                'DSX Reservation Door Override'
		                ,'An attribute matrix to store the DSX Reservation Door Override configuration'
		                ,1
		                ,'{{% if AttributeMatrixItems != empty %}}  <table class=''grid-table table table-condensed table-light''> <thead> <tr> {{% for itemAttribute in ItemAttributes %}}     <th>{{{{ itemAttribute.Name }}}}</th> {{% endfor %}} </tr> </thead> <tbody> {{% for attributeMatrixItem in AttributeMatrixItems %}} <tr>     {{% for itemAttribute in ItemAttributes %}}         <td>{{{{ attributeMatrixItem | Attribute:itemAttribute.Key }}}}</td>     {{% endfor %}} </tr> {{% endfor %}} </tbody> </table>  {{% endif %}}'
		                ,'{0}'
	                )

	                -- Getting AttributeMatrixId
	                SET @AttributeMatrixTemplateId = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = TRY_CAST('{0}' AS uniqueidentifier));
                END

                DECLARE @CreatedId INT

                -- Checking if Start DateTime Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating Start DateTime Attribute
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @DateTimeFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'StartDateTime', 
		                'Start DateTime', 
		                'The Start DateTime of the Override', 
		                0,
		                0, 
		                0, 
		                1, 
		                '{1}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

	                -- Creating Attribute Qualifiers
	                INSERT INTO [AttributeQualifier] (
			                [IsSystem], [AttributeId], [Key], [Value], [Guid]
	                ) VALUES (0, @CreatedId, 'datePickerControlType', 'Date Picker', NEWID() ),
	                ( 0, @CreatedId, 'displayCurrentOption', 'False', NEWID() ),
	                ( 0, @CreatedId, 'displayDiff', 'False', NEWID() ),
	                ( 0, @CreatedId, 'format', '', NEWID() ),
	                ( 0, @CreatedId, 'futureYearCount', '', NEWID() )
                END

                -- Checking if Start Action Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{2}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating Start Action
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @DefinedValueFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'StartAction', 
		                'Start Action', 
		                'The Start Action of the Override', 
		                1,
		                0, 
		                0, 
		                1, 
		                '{2}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{2}')

	                INSERT INTO [AttributeQualifier] (
			                [IsSystem], [AttributeId], [Key], [Value], [Guid]
	                ) VALUES
	                (0, @CreatedId, 'allowmultiple', 'False', NEWID() ),
	                (0, @CreatedId, 'definedtype', (SELECT CAST([Id] as varchar(255)) FROM [DefinedType] WHERE [Guid] = '{3}'), NEWID() ),
	                (0, @CreatedId, 'displaydescription', 'False', NEWID() ),
	                (0, @CreatedId, 'enhancedselection', 'False', NEWID() ),
	                (0, @CreatedId, 'includeInactive', 'False', NEWID() )
                END

                -- Checking if End DateTime Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{4}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating End DateTime
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @DateTimeFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'EndDateTime', 
		                'End DateTime', 
		                'The End DateTime of the Override', 
		                2,
		                0, 
		                0, 
		                1, 
		                '{4}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{4}')

	                -- Creating Attribute Qualifiers
	                INSERT INTO [AttributeQualifier] (
			                [IsSystem], [AttributeId], [Key], [Value], [Guid]
	                ) VALUES (0, @CreatedId, 'datePickerControlType', 'Date Picker', NEWID() ),
	                ( 0, @CreatedId, 'displayCurrentOption', 'False', NEWID() ),
	                ( 0, @CreatedId, 'displayDiff', 'False', NEWID() ),
	                ( 0, @CreatedId, 'format', '', NEWID() ),
	                ( 0, @CreatedId, 'futureYearCount', '', NEWID() )

                END


                -- Checking if End Action Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{5}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating End Action
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @DefinedValueFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'EndAction', 
		                'End Action', 
		                'The End Action of the Override', 
		                3,
		                0, 
		                0, 
		                1, 
		                '{5}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{5}')

	                INSERT INTO [AttributeQualifier] (
			                [IsSystem], [AttributeId], [Key], [Value], [Guid]
	                ) VALUES
	                (0, @CreatedId, 'allowmultiple', 'False', NEWID() ),
	                (0, @CreatedId, 'definedtype', (SELECT CAST([Id] as varchar(255)) FROM [DefinedType] WHERE [Guid] = '{3}'), NEWID() ),
	                (0, @CreatedId, 'displaydescription', 'False', NEWID() ),
	                (0, @CreatedId, 'enhancedselection', 'False', NEWID() ),
	                (0, @CreatedId, 'includeInactive', 'False', NEWID() )
                END",
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.RESERVATION_DOOR_OVERRIDES_ATTRIBUTEMATRIX,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_RESERVATION_START_DATE,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_RESERVATION_START_ACTION,
                    com.bemaservices.DoorControl.DSX.SystemGuid.DefinedType.DSX_ACTIONS,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_RESERVATION_END_DATE,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_RESERVATION_END_ACTION

            );

            var createCampusAttributeMatrixAndValues = string.Format( @"
                -- Getting a few Ids
                DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
                DECLARE @DateTimeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Class] = 'Rock.Field.Types.DateTimeFieldType');
                DECLARE @DefinedValueFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Class] = 'Rock.Field.Types.DefinedValueFieldType');
                DECLARE @LocationFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Class] = 'Rock.Field.Types.LocationFieldType');

                -- Checking if AttributeMatrixTemplate exists
                DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = TRY_CAST('{0}' AS uniqueidentifier))
                IF ISNULL(@AttributeMatrixTemplateId, '') = ''
                BEGIN
	                -- Creating Attribute Matrix
	                INSERT INTO [dbo].[AttributeMatrixTemplate] (
		                [Name], 
		                [Description],
		                [IsActive],
		                [FormattedLava],
		                [Guid]
	                )
	                VALUES (
		                'DSX Campus Door Override'
		                ,'An attribute matrix to store the DSX Campus Door Override configuration'
		                ,1
		                ,'{{% if AttributeMatrixItems != empty %}}  <table class=''grid-table table table-condensed table-light''> <thead> <tr> {{% for itemAttribute in ItemAttributes %}}     <th>{{{{ itemAttribute.Name }}}}</th> {{% endfor %}} </tr> </thead> <tbody> {{% for attributeMatrixItem in AttributeMatrixItems %}} <tr>     {{% for itemAttribute in ItemAttributes %}}         <td>{{{{ attributeMatrixItem | Attribute:itemAttribute.Key }}}}</td>     {{% endfor %}} </tr> {{% endfor %}} </tbody> </table>  {{% endif %}}'
		                ,'{0}'
	                )

	                -- Getting AttributeMatrixId
	                SET @AttributeMatrixTemplateId = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = TRY_CAST('{0}' AS uniqueidentifier));
                END

                DECLARE @CreatedId INT

                -- Checking if Location Attribute Exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{6}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating Start DateTime Attribute
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @LocationFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'Location', 
		                'Location', 
		                'The Location we want to create an override for', 
		                0,
		                0, 
		                0, 
		                1, 
		                '{6}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )
                END


                -- Checking if Start DateTime Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating Start DateTime Attribute
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @DateTimeFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'StartDateTime', 
		                'Start DateTime', 
		                'The Start DateTime of the Override', 
		                0,
		                0, 
		                0, 
		                1, 
		                '{1}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

	                -- Creating Attribute Qualifiers
	                INSERT INTO [AttributeQualifier] (
			                [IsSystem], [AttributeId], [Key], [Value], [Guid]
	                ) VALUES (0, @CreatedId, 'datePickerControlType', 'Date Picker', NEWID() ),
	                ( 0, @CreatedId, 'displayCurrentOption', 'False', NEWID() ),
	                ( 0, @CreatedId, 'displayDiff', 'False', NEWID() ),
	                ( 0, @CreatedId, 'format', '', NEWID() ),
	                ( 0, @CreatedId, 'futureYearCount', '', NEWID() )
                END

                -- Checking if Start Action Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{2}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating Start Action
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @DefinedValueFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'StartAction', 
		                'Start Action', 
		                'The Start Action of the Override', 
		                1,
		                0, 
		                0, 
		                1, 
		                '{2}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{2}')

	                INSERT INTO [AttributeQualifier] (
			                [IsSystem], [AttributeId], [Key], [Value], [Guid]
	                ) VALUES
	                (0, @CreatedId, 'allowmultiple', 'False', NEWID() ),
	                (0, @CreatedId, 'definedtype', (SELECT CAST([Id] as varchar(255)) FROM [DefinedType] WHERE [Guid] = '{3}'), NEWID() ),
	                (0, @CreatedId, 'displaydescription', 'False', NEWID() ),
	                (0, @CreatedId, 'enhancedselection', 'False', NEWID() ),
	                (0, @CreatedId, 'includeInactive', 'False', NEWID() )
                END

                -- Checking if End DateTime Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{4}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating End DateTime
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @DateTimeFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'EndDateTime', 
		                'End DateTime', 
		                'The End DateTime of the Override', 
		                2,
		                0, 
		                0, 
		                1, 
		                '{4}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{4}')

	                -- Creating Attribute Qualifiers
	                INSERT INTO [AttributeQualifier] (
			                [IsSystem], [AttributeId], [Key], [Value], [Guid]
	                ) VALUES (0, @CreatedId, 'datePickerControlType', 'Date Picker', NEWID() ),
	                ( 0, @CreatedId, 'displayCurrentOption', 'False', NEWID() ),
	                ( 0, @CreatedId, 'displayDiff', 'False', NEWID() ),
	                ( 0, @CreatedId, 'format', '', NEWID() ),
	                ( 0, @CreatedId, 'futureYearCount', '', NEWID() )

                END


                -- Checking if End Action Attribute exists
                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{5}')
                IF ISNULL(@CreatedId, '') = ''
                BEGIN
	                -- Creating End Action
	                INSERT INTO [dbo].[Attribute] (
		                [IsSystem]
		                ,[FieldTypeId]
		                ,[EntityTypeId]
		                ,[EntityTypeQualifierColumn]
		                ,[EntityTypeQualifierValue]
		                ,[Key]
		                ,[Name]
		                ,[Description]
		                ,[Order]
		                ,[IsGridColumn]
		                ,[IsMultiValue]
		                ,[IsRequired]
		                ,[Guid]
		                ,[AllowSearch]
		                ,[IsIndexEnabled]
		                ,[IsAnalytic]
		                ,[IsAnalyticHistory]
	                )
	                VALUES (
		                0, 
		                @DefinedValueFieldTypeId, 
		                @AttributeMatrixItemEntityTypeId, 
		                'AttributeMatrixTemplateId', 
		                @AttributeMatrixTemplateId, 
		                'EndAction', 
		                'End Action', 
		                'The End Action of the Override', 
		                3,
		                0, 
		                0, 
		                1, 
		                '{5}', 
		                0, 
		                0, 
		                0, 
		                0 
	                )

	                SET @CreatedId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{5}')

	                INSERT INTO [AttributeQualifier] (
			                [IsSystem], [AttributeId], [Key], [Value], [Guid]
	                ) VALUES
	                (0, @CreatedId, 'allowmultiple', 'False', NEWID() ),
	                (0, @CreatedId, 'definedtype', (SELECT CAST([Id] as varchar(255)) FROM [DefinedType] WHERE [Guid] = '{3}'), NEWID() ),
	                (0, @CreatedId, 'displaydescription', 'False', NEWID() ),
	                (0, @CreatedId, 'enhancedselection', 'False', NEWID() ),
	                (0, @CreatedId, 'includeInactive', 'False', NEWID() )
                END",
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.CAMPUS_DOOR_OVERRIDES_ATTRIBUTEMATRIX,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_CAMPUS_START_DATE,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_CAMPUS_START_ACTION,
                    com.bemaservices.DoorControl.DSX.SystemGuid.DefinedType.DSX_ACTIONS,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_CAMPUS_END_DATE,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_CAMPUS_END_ACTION,
                    com.bemaservices.DoorControl.DSX.SystemGuid.Attribute.ATTRIBUTEMATRIX_CAMPUS_LOCATION
            );

            // Running SQL
            Sql( createReservationAttributeMatrixAndValues );
            Sql( createCampusAttributeMatrixAndValues );
        }

        public override void Down()
        {

        }
    }
}
