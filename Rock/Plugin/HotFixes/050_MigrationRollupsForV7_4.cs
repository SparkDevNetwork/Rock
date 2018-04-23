using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 50, "1.7.0" )]
    public class MigrationRollupsForV7_4 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {

            // Fix for https://github.com/SparkDevNetwork/Rock/issues/2788
            Sql( @"UPDATE [CommunicationTemplate]
SET [Message] = REPLACE([Message], 
  '<!-- prevent Gmail on iOS font size manipulation -->
  <div style=""display:none; white-space:nowrap; font:15px courier; line-height:0;""> &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; </div>', 
  '<!-- prevent Gmail on iOS font size manipulation -->
  <div style=""display:none; white-space:nowrap; font:15px courier; line-height:0;""> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </div>')
WHERE [Message] LIKE '%<!-- prevent Gmail on iOS font size manipulation -->
  <div style=""display:none; white-space:nowrap; font:15px courier; line-height:0;""> &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; </div>%'"
 );

            // add ServiceJob: Data Migrations for v7.4
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV74DataMigrations' AND [Guid] = 'FF760EF9-66BD-4A4D-AF95-749AA789ACAF' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'Data Migrations for v7.4'
                  ,'This job will take care of any data migrations that need to occur after updating to v74. After all the operations are done, this job will delete itself.'
                  ,'Rock.Jobs.PostV74DataMigrations'
                  ,'0 0 3 1/1 * ? *'
                  ,1
                  ,'FF760EF9-66BD-4A4D-AF95-749AA789ACAF'
                  );
            END" );

            // Fix for https://github.com/SparkDevNetwork/Rock/issues/2813
            Sql( HotFixMigrationResource._050_MigrationRollupsForV7_4_spAnalytics_ETL_Campus );

            // Fix for https://github.com/SparkDevNetwork/Rock/issues/2809
            Sql( HotFixMigrationResource._050_MigrationRollupsForV7_4_spCrm_PersonMerge );

            // Update check-in to support check-in by Gender
            AddCheckinByGender();

            // Fix for personal communication templates
            FixPersonalCommunicationTemplates();
        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //
        }

        private void AddCheckinByGender()
        {
            RockMigrationHelper.UpdateFieldType( "Gender", "Used to select a gender", "Rock", "Rock.Field.Types.GenderFieldType", "2E28779B-4C76-4142-AE8D-49EA31DDB503" );

            RockMigrationHelper.AddGroupTypeGroupAttribute( "0572A5FE-20A4-4BF1-95CD-C71DB5281392", "2E28779B-4C76-4142-AE8D-49EA31DDB503", "Gender", "The gender allowed to check in to these group types.", 2, "", "DE6F800F-0177-4DAE-BA9B-AD75F20F255B" );
            Sql( @"
    -- Make sure the attribute belongs to the 'Check-in' cateogry
    DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType' )
    DECLARE @AttributeEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Attribute' )
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'DE6F800F-0177-4DAE-BA9B-AD75F20F255B' )
    DECLARE @CategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Name] = 'Check-in' AND [EntityTypeId] = @AttributeEntityTypeId AND [EntityTypeQualifierColumn] = 'EntityTypeId' AND [EntityTypeQualifierValue] = CAST( @GroupTypeEntityTypeId AS varchar) )
    DELETE [AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId
    INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] ) VALUES ( @AttributeId, @CategoryId )
" );

            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.FilterGroupsByGender", "B16E3329-49F4-4DA0-9802-E7BA75F5FD42", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B16E3329-49F4-4DA0-9802-E7BA75F5FD42", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "6EC3B2A5-E962-476F-8052-B795AE2ECEF3" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B16E3329-49F4-4DA0-9802-E7BA75F5FD42", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "81957147-9C2D-424E-A9B7-386A72937892" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "B16E3329-49F4-4DA0-9802-E7BA75F5FD42", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", 0, @"True", "1E1F7819-D6EA-4F0B-B30B-90429BDA9808" );

            RockMigrationHelper.UpdateWorkflowActionType( "EB744DF1-E454-482C-B111-80A54EF8A674", "Filter Groups by Gender", 0, "B16E3329-49F4-4DA0-9802-E7BA75F5FD42", true, false, "", "66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD", 1, "False", "EDDD1612-DFE4-4538-84E2-BCC8E869A2F3" );

            Sql( @"
    -- Fix the ordering of the Person Search workflow activity so that the 'Filter By Gender' activity is immediately after 'Filter By Grade'
    DECLARE @ActivityTypeId int = ( SELECT TOP 1 [Id] FROM [WorkflowActivityType] WHERE [Guid] = 'EB744DF1-E454-482C-B111-80A54EF8A674' )
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.CheckIn.FilterGroupsByGrade' )
    DECLARE @Order int = ISNULL( ( SELECT TOP 1 [Order] FROM [WorkflowActionType] WHERE [ActivityTypeId] = @ActivityTypeId AND [EntityTypeId] = @EntityTypeId ), 0)
    IF @Order IS NOT NULL AND @Order > 0
    BEGIN
        UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [ActivityTypeId] = @ActivityTypeId AND [Order] > @Order
        UPDATE [WorkflowActionType] SET [Order] = @Order + 1 WHERE [Guid] = 'EDDD1612-DFE4-4538-84E2-BCC8E869A2F3'
    END
" );
        }

        private void FixPersonalCommunicationTemplates()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.CommunicationTemplate", 0, "Edit", true, SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS, 0, "38869B4A-DB5E-42D6-BA25-E5AA12FE713E" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "EACDBBD4-C355-4D38-B604-779BC55D3876", SystemGuid.FieldType.BOOLEAN, "Personal Templates View", "PersonalTemplatesView", "", "Is this block being used to display personal templates (only templates that current user is allowed to edit)?", 1, @"False", "BB7AF79E-79D1-48B0-8FA6-EEE358B0ACB2" );
            RockMigrationHelper.AddBlockAttributeValue( true, "8B080D88-D088-4D09-9D74-576B485549A2", "BB7AF79E-79D1-48B0-8FA6-EEE358B0ACB2", @"True" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "BFDCA2E2-DAA1-4FA6-B33C-C53C7CF23C5D6", SystemGuid.FieldType.BOOLEAN, "Personal Templates View", "PersonalTemplatesView", "", "Is this block being used to display personal templates (only templates that current user is allowed to edit)?", 0, @"False", "60581383-BE1E-40A4-9F60-786B761BDA98" );
            RockMigrationHelper.AddBlockAttributeValue( true, "425C325E-1054-4A52-A162-DECEB377E178", "60581383-BE1E-40A4-9F60-786B761BDA98", @"True" );
        }
    }
}