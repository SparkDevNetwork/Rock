// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
            /*
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

            UpdateEntityTypeonCDRInteractions();

            FixTypoInWordCloudLavaShortcode();

            AddStickyHeaderToScheduleGrid();

            FixTyposIssue2928();

            // Add Vimeo Short Code
            Sql( HotFixMigrationResource._050_MigrationRollupsForV7_4_AddVimeoShortCode );

            // Fix Accordion Short Code
            Sql( HotFixMigrationResource._050_MigrationRollupsForV7_4_FixAccordionShortCode );

            // PersonDuplicateFinder have the GUIDs for Mobile and home phone reversed
            Sql( @"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCrm_PersonDuplicateFinder]') AND type in (N'P', N'PC'))
                DROP PROCEDURE[dbo].[spCrm_PersonDuplicateFinder];" );
            Sql( HotFixMigrationResource._050_MigrationRollupsForV7_4_spCrm_PersonDuplicateFinder );

            // Thank-you and on-going Are Hyphenated Unnecessarily #1711
            Sql( HotFixMigrationResource._050_MigrationRollupsForV7_4_FixThankyouAndOngoingHyphenations );

            // Fix for #2722
            UpdateGradeTransitionDateFieldType();

            FixFamilyPreregistrationTitle();
            
            */
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //
        }

        private void UpdateGradeTransitionDateFieldType()
        {
            RockMigrationHelper.UpdateFieldType( "Month Day", "", "Rock", "Rock.Field.Types.MonthDayFieldType", Rock.SystemGuid.FieldType.MONTH_DAY );

            Sql( $@"
DECLARE @FieldTypeIdMonthDay INT = (
		SELECT TOP 1 Id
		FROM FieldType
		WHERE [Guid] = '{Rock.SystemGuid.FieldType.MONTH_DAY}'
		)

UPDATE Attribute
SET FieldTypeId = @FieldTypeIdMonthDay
	,[Description] = 'The date when kids are moved to the next grade level.'
WHERE [Key] = 'GradeTransitionDate'
	AND [EntityTypeId] IS NULL
	AND (
		FieldTypeId != @FieldTypeIdMonthDay
		OR [Description] != 'The date when kids are moved to the next grade level.'
		)
" );
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

        private void UpdateEntityTypeonCDRInteractions()
        {
            Sql( @"DECLARE @PersonAliasEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] LIKE 'Rock.Model.PersonAlias')
                UPDATE[InteractionChannel]
                SET[InteractionEntityTypeId] = @PersonAliasEntityTypeId
                 WHERE[Guid] = 'B3904B57-62A2-57AC-43EA-94D4DEBA3D51'" );
        }

        private void FixTypoInWordCloudLavaShortcode()
        {
            Sql( @"UPDATE[LavaShortcode]
                SET[Documentation] = REPLACE([Documentation], 'you would like to pay', 'you would like to play')
                WHERE[Guid] = 'CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4'" );
        }

        private void AddStickyHeaderToScheduleGrid()
        {
            // Attrib for BlockType: Schedule Builder:core.CustomGridEnableStickyHeaders   
            RockMigrationHelper.UpdateBlockTypeAttribute( "8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", @"", 0, @"False", "CDE41D50-F1C0-467D-820C-023E332AFB5B" );
        }

        private void FixTyposIssue2928()
        {
            Sql( @"UPDATE [LavaShortCode]
                SET [Documentation] = '<p>
    Adding parallax effects (when the background image of a section scrolls at a different speed than the rest of the page) can greatly enhance the 
    aesthetics of the page. Until now, this effect has taken quite a bit of CSS know how to achieve. Now it’s as simple as:
</p>
<pre>{[ parallax image:''http://cdn.wonderfulengineering.com/wp-content/uploads/2014/09/star-wars-wallpaper-4.jpg'' contentpadding:''20px'' ]}
    &lt;h1&gt;Hello World&lt;/h1&gt;
{[ endparallax ]}</pre>

<p>  
    This shotcode takes the content you provide it and places it into a div with a parallax background using the image you provide in the ''image'' 
    parameter. As always there are several parameters.
</p>
    
<ul>
    <li><strong>image</strong> (required) – A valid URL to the image that should be used as the background.</li><li><b>height</b> (200px) – The minimum height of the content. This is useful if you want your section to not have any 
    content, but instead be just the parallax image.</li>
    <li><strong>videourl</strong> - This is the URL to use if you''d like a video background.</li>
    <li><strong>speed</strong> (50) – the speed that the background should scroll. The value of 0 means the image will be fixed in place, the value of 100 would make the background scroll quick up as the page scrolls down, while the value of -100 would scroll quickly in the opposite direction.</li>
    <li><strong>zindex</strong> (1) – The z-index of the background image. Depending on your design you may need to adjust the z-index of the parallax image. </li>
    <li><strong>position</strong> (center center) - This is analogous to the background-position css property. Specify coordinates as top, bottom, right, left, center, or pixel values (e.g. -10px 0px). The parallax image will be positioned as close to these values as possible while still covering the target element.</li>
    <li><strong>contentpadding</strong> (0) – The amount of padding you’d like to have around your content. You can provide any valid CSS padding value. For example, the value ‘200px 20px’ would give you 200px top and bottom and 20px left and right.</li>
    <li><strong>contentcolor</strong> (#fff = white) – The font color you’d like to use for your content. This simplifies the styling of your content.</li>
    <li><strong>contentalign</strong> (center) – The alignment of your content inside of the section. </li>
    <li><strong>noios</strong> (false) – Disables the effect on iOS devices. </li>
    <li><strong>noandriod</strong> (center) – Disables the effect on driods. </li>
</ul>
<p>Note: Due to the javascript requirements of this shortcode, you will need to do a full page reload before changes to the shortcode appear on your page.</p>'
                WHERE [Guid] = '4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4'" );
        }

        private void FixFamilyPreregistrationTitle()
        {
            Sql( @"
    UPDATE [Page] SET 
        [InternalName] = 'Family Pre-Registration'
        ,[PageTitle] = 'Family Pre-Registration'
        ,[BrowserTitle] = 'Family Pre-Registration'
    WHERE [Guid] IN ( '3B31B9A2-DE35-4407-8E7D-3633F93906CD', 'B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44' )

    UPDATE [BlockType] SET 
        [Name] = 'Family Pre-Registration'
    WHERE [Path] = '~/Blocks/Crm/FamilyPreRegistration.ascx'
" );

            RockMigrationHelper.AddPageRoute( "3B31B9A2-DE35-4407-8E7D-3633F93906CD", "FamilyPreRegistration", "F4EC3FCD-6410-44A9-B66B-A4BC207CA7DA" );// for Page:Family Pre-Registration
            RockMigrationHelper.AddPageRoute( "B37D22BE-D2A8-4EFA-8B2B-2E0EFF6EDB44", "FamilyPreRegistrationSuccess", "3C39DF30-00B0-4096-B623-200A93D85CA9" );// for Page:Family Pre-Registration Success

        }
    }
}