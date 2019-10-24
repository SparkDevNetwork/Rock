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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_0618 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            Fix3731();
            UpdateCheckScannerURL();
            UpdateMotivatorClusterToTheme();
            UpdateCalendarAttributePanel();
            Fix2784();
            RemoveAssessmentsTabFromPersonProfilePage();
            AssessmentBadges();
            LegacyLavaSupportRemovalNotification();
            BlockTypeServiceMetricsCampusUp();
            AddCachingToChannelFeedUp();
            MissingHotfixMigrations();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddCachingToChannelFeedDown();
            BlockTypeServiceMetricsCampusDown();
            CodeGenMigrationsDown();
        }

        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Content Channel Item View:Merge Content
            RockMigrationHelper.UpdateBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Merge Content", "MergeContent", "", @"Should the content data and attribute values be merged using the Lava template engine.", 0, @"False", "8D1CDCFC-542A-4382-8B85-6CEE3CDA8B41" );
            // Attrib for BlockType: Event Registration Wizard:Available Registration Templates
            RockMigrationHelper.UpdateBlockTypeAttribute( "B1C7E983-5000-4CBE-84DD-6B7D428635AC", "F56DED5E-C135-42B2-A529-878CB30436B5", "Available Registration Templates", "AvailableRegistrationTemplates", "", @"The list of templates the staff person can pick from – not all templates need to be available to all blocks.", 2, @"", "06D5C8F3-EAA4-4F4C-B7B9-029ACAA6119C" );
        }

        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Event Registration Wizard:Available Registration Templates
            RockMigrationHelper.DeleteAttribute( "06D5C8F3-EAA4-4F4C-B7B9-029ACAA6119C" );
            // Attrib for BlockType: Content Channel Item View:Merge Content
            RockMigrationHelper.DeleteAttribute( "8D1CDCFC-542A-4382-8B85-6CEE3CDA8B41" );
        }

        /// <summary>
        /// NA: Fix Lava Template Channel Item Configuration typo for issue #3731
        /// </summary>
        private void Fix3731()
        {
            Sql( @"
                -- Fix Lava Template Channel Item Configuration typo for issue #3731
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '71D998C7-9F27-4B8A-937A-64C5EFC4783A')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '47C56661-FB70-4703-9781-8651B8B49485')

                IF @BlockId IS NOT NULL AND @AttributeId IS NOT NULL
                BEGIN
	                UPDATE [AttributeValue]
	                SET [Value] = REPLACE([Value], ' item | Attribute:''Speaker'' ', ' Item | Attribute:''Speaker'' ')
                    WHERE [AttributeId] = @AttributeId
                    AND [EntityId] = @BlockId
                END" );
        }

        /// <summary>
        /// MP: Update the CheckScanner download link under External Applications
        /// </summary>
        private void UpdateCheckScannerURL()
        {
            Sql( @"
                UPDATE[AttributeValue]
                SET[Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.9.0/checkscanner.msi'
                WHERE[Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180'" );
        }

        /// <summary>
        /// ED: Update motivator verbiage from Cluster to Theme
        /// </summary>
        private void UpdateMotivatorClusterToTheme()
        {
            // Cluster to Theme verbiage change for the Motivators Directional Theme attribute value
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you lead a team or an organization. The motivators in this theme can be seen in the type of behavior you demonstrate as it relates to the direction or health of the organization or team in which you are engaged. The greater the number of motivators in this theme that you possess in your top five, the more effective you will be in providing direction within the organization.</p>" );
            // Cluster to Theme verbiage change for the MotivatorsIntellectualTheme attribute value
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you focus your mind. These motivators can be seen in the way you think or the kind of mental activities you naturally pursue. The way you view your mental activity will be directly influenced by the motivators in this theme. Your conversations will be greatly influenced by these motivators that are in the top five of your profile.</p>" );
            // Cluster to Theme verbiage change for the MotivatorsRelationalTheme attribute value
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you relate to others. These motivators can best be seen as the reasons you build relationships with the people around you, and influence what you value in relationships. The greater the number of motivators in this that you possess in your top five, the more strongly you will be focused on building healthy relationships.</p>" );
            // Cluster to Theme verbiage change for the MotivatorsPositionalTheme attribute value
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you execute your role or position within the team. The motivators in this theme can be seen in the way you approach activity, moment by moment. They dramatically influence what you value and how you spend your time or effort at work. When others look at the way you act, your behavior will be greatly determined by the motivators int this theme that are found in your top five.</p>" );
            // Update to the Results Message attribute value for the Motivator assessment block
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"<p>
   {{ Person.NickName }}, here are your motivators results. We’ve listed your Top 5 Motivators, your
   growth propensity score, along with a complete listing of all 22 motivators and your results
   for each.
</p>
<h2>Growth Propensity</h2>
<p>
    Growth Propensity measures your perceived mindset on a continuum between a growth mindset and
    fixed mindset. These are two ends of a spectrum about how we view our own capacity and potential.
</p>
{[ chart type:'gauge' backgroundcolor:'#f13c1f,#f0e3ba,#0e9445,#3f56a1' gaugelimits:'0,2,17,85,100']}
    [[ dataitem value:'{{ GrowthScore }}' fillcolor:'#484848' ]] [[ enddataitem ]]
{[ endchart ]}
<h2>Individual Motivators</h2>
<p>
    There are 22 possible motivators in this assessment. While your Top 5 Motivators may be most helpful in understanding your results in a snapshot, you may also find it helpful to see your scores on each for a complete picture.
</p>
<!--  Theme Chart -->
<div class=""panel panel-default"">
    <div class=""panel-heading"">
    <h2 class=""panel-title""><b>Composite Score</b></h2>
    </div>
    <div class=""panel-body"">
    {[chart type:'horizontalBar' chartheight:'200px' ]}
    {% for motivatorThemeScore in MotivatorThemeScores %}
        [[dataitem label:'{{ motivatorThemeScore.DefinedValue.Value }}' value:'{{ motivatorThemeScore.Value }}' fillcolor:'{{ motivatorThemeScore.DefinedValue | Attribute:'Color' }}' ]]
        [[enddataitem]]
    {% endfor %}
    {[endchart]}
    </div>
</div>
<p>
    This graph is based on the average composite score for each Motivator Theme.
</p>
{% for motivatorThemeScore in MotivatorThemeScores %}
    <p>
        <b>{{ motivatorThemeScore.DefinedValue.Value }}</b>
        </br>
        {{ motivatorThemeScore.DefinedValue.Description }}
        </br>
        {{ motivatorThemeScore.DefinedValue | Attribute:'Summary' }}
    </p>
{% endfor %}
<p>
   The following graph shows your motivators ranked from top to bottom.
</p>
  <div class=""panel panel-default"">
    <div class=""panel-heading"">
      <h2 class=""panel-title""><b>Ranked Motivators</b></h2>
    </div>
    <div class=""panel-body"">
      {[ chart type:'horizontalBar' ]}
        {% for motivatorScore in MotivatorScores %}
        {% assign theme = motivatorScore.DefinedValue | Attribute:'Theme' %}
            {% if theme and theme != empty %}
                [[dataitem label:'{{ motivatorScore.DefinedValue.Value }}' value:'{{ motivatorScore.Value }}' fillcolor:'{{ motivatorScore.DefinedValue | Attribute:'Color' }}' ]]
                [[enddataitem]]
            {% endif %}
        {% endfor %}
        {[endchart]}
    </div>
  </div>
", "BA51DFCD-B174-463F-AE3F-6EEE73DD9338" );
        }

        /// <summary>
        /// GJ: Update calendar attribute panel
        /// </summary>
        private void UpdateCalendarAttributePanel()
        {
            // Add Block to Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9C610805-BE44-42DF-A73F-2C6D0014AD49", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Calendar Attributes", "Main", @"<style>
    .panel-parent .panel-block .panel-heading { display: none; }
</style>
<div class=""panel panel-block panel-parent"" style=""margin-bottom: 15px;"">
<div class=""panel-heading"">
        <h1 class=""panel-title"">
            <i class=""fa fa-fw fa-calendar""></i>
            Calendar Attributes
        </h1>
    </div>", "</div>", 0, "F04979E2-33A2-4C0E-936E-5C8849BB98F4" );

            // Add Block to Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9C610805-BE44-42DF-A73F-2C6D0014AD49", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Event Occurrence Attributes", "Main", @"<div class=""panel panel-block panel-parent"" style=""margin-bottom: 15px;"">
    <div class=""panel-heading"">
        <h1 class=""panel-title"">
            <i class=""fa fa-fw fa fa-clock-o""></i>
            Event Occurrence Attributes
        </h1>
    </div>", "</div>", 1, "DF4472A7-AC11-4245-B9D0-FBB8547B60B4" );
        }

        /// <summary>
        /// NA: Fix Issue #2784; legacy Lava in IT Support, Facilities Request, and Profile Change Request  workflow
        /// </summary>
        private void Fix2784()
        {
            Sql( @"
                -- Fix Issue #2784 still uses legacy Lava in IT Support, Facilities Request, and Profile Change Request  workflow
                UPDATE [dbo].[WorkflowActionForm]
                SET [Header] = REPLACE([Header], ' Workflow.Title ', ' Workflow | Attribute: ''Summary'' ')
                WHERE [Guid] in ( 'eb869aa0-c81f-4ba0-9a76-a18a0a544bc6'
                    ,'a08fc955-bb1a-4747-8fe3-88e3a2db646e'
                    ,'b3c205f8-c917-4f45-8f73-cdc8e64b6cf8'
                    ,'6de4bbea-67a2-4b14-973f-6e699228620d'
                    ,'109a961b-2b44-4080-958c-cc577a7f6245'
                    ,'112575f7-f513-47fe-b653-e0abe1d27d08')" );
        }

        /// <summary>
        /// ED: Do not display the "Assessments" tab on the person profile page
        /// </summary>
        private void RemoveAssessmentsTabFromPersonProfilePage()
        {
            Sql( @"
                UPDATE [dbo].[Page]
                SET [DisplayInNavWhen] = 2
                WHERE [Guid] = '985D7F56-9FD6-421B-B406-2D6B87CAFAE9'" );
        }

        /// <summary>
        /// The previous AssessmentBadge migration will not work if the attribute value was deleted and recreated.
        /// This will catch those and update the data correctly.
        /// </summary>
        private void AssessmentBadges()
        {
            Sql( @"
                -- Get the badges block attribute ID
                DECLARE @badgesBlockAttribute INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = 'F5AB231E-3836-4D52-BD03-BF79773C237A')

                -- Get the block ID
                DECLARE @entityId INT = (SELECT [Id] FROM [dbo].[Block] WHERE [Guid] = 'F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B')

                -- Get the attribute value guid
                DECLARE @avGuid UNIQUEIDENTIFIER = (select [Guid] FROM [dbo].[AttributeValue] WHERE [AttributeId] = @badgesBlockAttribute AND [EntityId] = @entityId)

                -- Remove the DISC attribute
                UPDATE [dbo].[AttributeValue]
                SET [Value] = REPLACE([Value], '6C491A10-E942-4CA5-8D13-ACBC28511714', '') 
                WHERE [Guid] = @avGuid

                -- See if the assessment badge is already included, if so then skip
                IF( ( SELECT COUNT(*) FROM [dbo].[AttributeValue] WHERE [Guid] = @avGuid AND [Value] LIKE '%CCE09793-89F6-4042-A98A-ED38392BCFCC%' ) = 0 )
                BEGIN
                    -- Check if the value is blank or null, if not then preceed new value with a comma
                    IF( ( SELECT COUNT(*) FROM [dbo].[AttributeValue] WHERE [Guid] = @avGuid AND RTRIM(LTRIM(COALESCE([Value], '') )) = '' ) = 0 )
                    BEGIN
                        UPDATE [dbo].[AttributeValue]
                        SET [Value] = [Value] + ', CCE09793-89F6-4042-A98A-ED38392BCFCC'
                        WHERE [Guid] = @avGuid
                    END
                    ELSE BEGIN
                        UPDATE [dbo].[AttributeValue]
                        SET [Value] = [Value] + 'CCE09793-89F6-4042-A98A-ED38392BCFCC'
                        WHERE [Guid] = @avGuid
                    END
                END

                -- Remove any double commas
                UPDATE [dbo].[AttributeValue]
                SET [Value] = REPLACE([Value], ',,', ', ')
                WHERE [Guid] = @avGuid

                UPDATE [dbo].[AttributeValue]
                SET [Value] = REPLACE([Value], ', ,', ', ')
                WHERE [Guid] = @avGuid" );
        }

        /// <summary>
        /// NA: Notify of LegacyLavaSupport removal
        /// </summary>
        private void LegacyLavaSupportRemovalNotification()
        {
            Sql( @"UPDATE [Attribute] SET [Description] = N'Only NoLegacy is supported. Old Lava syntax is ignored and not loaded.' WHERE [Guid] = 'C8E30F2B-7476-4B02-86D4-3E5057F03FD5'" );
        }

        /// <summary>
        /// NA: PR #3694 // Attrib Value for BlockType: Service Metrics Entry
        /// </summary>
        private void BlockTypeServiceMetricsCampusUp()
        {
            // Attrib Value for BlockType: Service Metrics Entry (PR #3694)
            RockMigrationHelper.AddBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", SystemGuid.FieldType.CAMPUSES, "Campuses", "Campuses", "", @"Select the campuses you want to limit this block to.", 4, @"", "D905852A-21F4-44C7-9F32-499C0FDC7D96", false );
        }

        /// <summary>
        /// NA: PR #3694 // Attrib Value for BlockType: Service Metrics Entry
        /// </summary>
        private void BlockTypeServiceMetricsCampusDown()
        {
            // Attrib Value for BlockType: Service Metrics Entry (PR #3694)
            RockMigrationHelper.DeleteAttribute( "D905852A-21F4-44C7-9F32-499C0FDC7D96" );
        }

        /// <summary>
        /// NA: PR #3458 Added caching to Channel Feed
        /// </summary>
        private void AddCachingToChannelFeedUp()
        {
            // Add caching to Channel Feed 
            RockMigrationHelper.AddDefinedTypeAttribute( "C3D44004-6951-44D9-8560-8567D705A48B", Rock.SystemGuid.FieldType.INTEGER, "Cache Duration", "CacheDuration", "Length of time in minutes to keep the template output in cache.", 2, "0", "6243C0B1-059D-4236-8F65-FC62281B6210" );
        }

        /// <summary>
        /// NA: PR #3458 Added caching to Channel Feed
        /// </summary>
        private void AddCachingToChannelFeedDown()
        {
            // Remove duration caching from Channel Feed 
            RockMigrationHelper.DeleteAttribute( "6243C0B1-059D-4236-8F65-FC62281B6210" );
        }

        /// <summary>
        /// MP: A couple of HotFix migrations that didn't end up in EF Migrations
        /// </summary>
        private void MissingHotfixMigrations()
        {
            // JE: Update Person Attribute to 'Suppress Sending Contribution Statements'
            Sql( @"
                UPDATE [Attribute]
                SET [Name] = 'Suppress Sending Contribution Statements'
                WHERE [Guid] = 'B767F2CF-A4F0-45AA-A2E9-8270F31B307B'" );

            // MP: Transaction Matching Batch Page
            // Attrib Value for Block:Transaction Matching, Attribute:Batch Detail Page Page: Transaction Matching, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A18A0A0A-0B71-43B4-B830-44B802C272D4", "494C6487-8007-439F-BF0B-3F6020D159E8", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
        }



    }
}
