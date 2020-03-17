using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Model;
using Rock.Plugin;

namespace com.bemaservices.SurveyGizmo.Migrations
{
    [MigrationNumber( 1, "1.8.0" )]
    class Setup : Migration
    {
        public override void Up()
        {
            // Setup Defined Type: Survey Gizmo Surveys
            RockMigrationHelper.AddDefinedType( "System", "Survey Gizmo Surveys", "Defined type that matches Survey Gizmo survey ids with person attributes.  This will tell the 'Sync Survey Gizmo Results' job how to process the survey results.", "a4a93e3f-4288-4ef0-840a-7090fa493ce4" );


            // Setup Defined Type Attribute: Survey Complete Person Attribute
            RockMigrationHelper.AddDefinedTypeAttribute( "a4a93e3f-4288-4ef0-840a-7090fa493ce4", Rock.SystemGuid.FieldType.ATTRIBUTE, "Survey Complete Person Attribute", "PersonAttribute", "An optional person attribute used to store the survey result.  (Only use person attributes that have a field type of 'Boolean' or 'Text')", 0, "", "06e95885-e97d-4716-93c9-b1f7b4f232a3" );
            RockMigrationHelper.AddAttributeQualifier( "06e95885-e97d-4716-93c9-b1f7b4f232a3", "entitytype", "72657ed8-d16e-492e-ac12-144c5e7567e7", "dde3fe0d-3132-4a4d-ab3e-e91139b7afea" );

            Sql( @"UPDATE Attribute
                   SET IsGridColumn = 1
                   WHERE [Guid] = '06e95885-e97d-4716-93c9-b1f7b4f232a3'" );


            // Setup Attribute Matrix: Survey Questions
            Sql( @"INSERT INTO [dbo].[AttributeMatrixTemplate] (
                [Name]
                ,[Description]
                ,[IsActive]
                ,[FormattedLava]
                ,[Guid]
            )
            VALUES (
                'Survey Questions'
                ,'A list of question responses to person attributes.'
                ,1
                ,'{% if AttributeMatrixItems != empty %}  <table class=''grid-table table table-condensed table-light''> <thead> <tr> {% for itemAttribute in ItemAttributes %}     <th>{{ itemAttribute.Name }}</th> {% endfor %} </tr> </thead> <tbody> {% for attributeMatrixItem in AttributeMatrixItems %} <tr>     {% for itemAttribute in ItemAttributes %}         <td>{{ attributeMatrixItem | Attribute:itemAttribute.Key }}</td>     {% endfor %} </tr> {% endfor %} </tbody> </table>  {% endif %}'
                ,'C9619E4A-051D-4ED0-913E-B5274808055A'
            )

            DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = TRY_CAST('C9619E4A-051D-4ED0-913E-B5274808055A' AS uniqueidentifier));
            
            -- It's possible that the attribute matrix entity type query below will fail if this plugin is installed on a brand new environment during testing.          
            DECLARE @AttributeMatrixItemEntityTypeId INT = (SELECT TOP 1 Id FROM EntityType WHERE [Name] = 'Rock.Model.AttributeMatrixItem');
            DECLARE @IntegerFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('A75DFC58-7A1B-4799-BF31-451B2BBE38FF' AS uniqueidentifier));
            DECLARE @TextFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('9C204CD0-1233-41C5-818A-C5DA439445AA' AS uniqueidentifier));
            DECLARE @AttributeFieldTypeId INT = (SELECT TOP 1 Id FROM FieldType WHERE [Guid] = TRY_CAST('99B090AA-4D7E-46D8-B393-BF945EA1BA8B' AS uniqueidentifier));

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
            VALUES (0, @IntegerFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', @AttributeMatrixTemplateId, 'QuestionId', 'Question Id', 'The question id from Survey Gizmo.', 0, 0, 0, 1, 'c893e725-4639-4158-a8b5-3e142910296a', 0, 0, 0, 0 ),
            (0, @TextFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', @AttributeMatrixTemplateId, 'Description1', 'Description', '', 1, 0, 0, 1, '48a37873-03ad-401d-93fa-ae6467801af4', 0, 0, 0, 0 ),
            (0, @AttributeFieldTypeId, @AttributeMatrixItemEntityTypeId, 'AttributeMatrixTemplateId', @AttributeMatrixTemplateId, 'PersonAttribute', 'Person Attribute', 'The person attribute that will store the result of the question response.', 2, 0, 0, 1, '8072cdd0-3dd7-4e5f-b364-a7ba3df64d6d', 0, 0, 0, 0 )

            DECLARE @PersonAttributeAttributeId INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = TRY_CAST('8072cdd0-3dd7-4e5f-b364-a7ba3df64d6d' AS uniqueidentifier));

            -- Attribute Qualifier: entitytype
            INSERT INTO [dbo].[AttributeQualifier] (
	             [IsSystem]
	            ,[AttributeId]
	            ,[Key]
	            ,[Value]
	            ,[Guid]
            )
            VALUES (
	             0
	            ,@PersonAttributeAttributeId
	            ,'entitytype'
	            ,'72657ed8-d16e-492e-ac12-144c5e7567e7'
	            ,'56df8c2e-b6ee-4920-8603-1de7c8045a01'
            )" );

            // Setup Defined Type Attribute: Question Response Mapping
            RockMigrationHelper.AddDefinedTypeAttribute( "a4a93e3f-4288-4ef0-840a-7090fa493ce4", "F16FC460-DC1E-4821-9012-5F21F974C677", "Question Mapping", "QuestionMapping", "A list of survey questions that are mapped to a person attribute.  The answer of the question will be stored as the attribute value.", 1, "", "eaa7bbc1-bae0-42c5-8919-8dc72aae7fad" );

            Sql( @"
                --Attribute Qualifier: entitytype
                DECLARE @AttributeId INT = (SELECT TOP 1 Id FROM Attribute WHERE [Guid] = TRY_CAST('eaa7bbc1-bae0-42c5-8919-8dc72aae7fad' AS uniqueidentifier));
                DECLARE @AttributeMatrixTemplateId INT = (SELECT TOP 1 Id FROM AttributeMatrixTemplate WHERE [Guid] = TRY_CAST('C9619E4A-051D-4ED0-913E-B5274808055A' AS uniqueidentifier));                

                INSERT INTO[dbo].[AttributeQualifier](
                     [IsSystem]
	                ,[AttributeId]
	                ,[Key]
	                ,[Value]
	                ,[Guid]
                )
                VALUES(
	                 0
                    ,@AttributeId
                    ,'attributematrixtemplate'
	                ,@AttributeMatrixTemplateId
	                ,'48BAC05F-BFD8-4FC2-BEC0-A96BC0F448CD'
                )" );

            Sql( @"UPDATE Attribute
                   SET IsGridColumn = 1
                   WHERE [Guid] = 'eaa7bbc1-bae0-42c5-8919-8dc72aae7fad'" );


            // Setup Rock Job: Sync Survey Results
            Sql( @" INSERT INTO [dbo].[ServiceJob] (
                 [IsSystem]
                ,[IsActive]
                ,[Name]
                ,[Description]
                ,[Class]
                ,[CronExpression]
                ,[NotificationStatus]
                ,[Guid]
            )
            VALUES (
                 0 
                ,1
                ,'Sync Survey Gizmo Results'
                ,'Job that will process Survey Gizmo survey responses.'
                , 'com.bemaservices.SurveyGizmo.SyncSurveyResults'
                , '0 0 6 1/1 * ? *'
                , 3
                , '37b454b4-3711-4bcf-a7c0-27feadac7b6b' )" );
        }

        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedType( "a4a93e3f-4288-4ef0-840a-7090fa493ce4" );
            RockMigrationHelper.DeleteAttribute( "06e95885-e97d-4716-93c9-b1f7b4f232a3" );
            RockMigrationHelper.DeleteAttribute( "eaa7bbc1-bae0-42c5-8919-8dc72aae7fad" );

            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'com.bemaservices.SurveyGizmo.SyncSurveyResults' AND [Guid] = '37b454b4-3711-4bcf-a7c0-27feadac7b6b' )
            BEGIN
               DELETE [ServiceJob]  WHERE [Guid] = '37b454b4-3711-4bcf-a7c0-27feadac7b6b';
            END" );
        }
    }
}
