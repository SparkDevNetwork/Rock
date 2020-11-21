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
    public partial class Rollup0708 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            EnsureCoreConnectionStatusIsSystem();
            FixIssue3497();
            MotivatorsUpdate();
            SegmentedChartUpdate();
            AddFinancialTransactionNoCashAssetTypeValueIdUp();
            FixDefinedTypeCategoryPersonalityAssessments();
            AddEventWizardPageUp();
            FixChartJS();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddEventPageDown();
            AddFinancialTransactionNoCashAssetTypeValueIdDown();
        }

        /// <summary>
        /// MP: Ensures the core connection status is system.
        /// </summary>
        private void EnsureCoreConnectionStatusIsSystem()
        {
            Sql( @"
                UPDATE [DefinedValue]
                SET [IsSystem] = 1
                WHERE [Guid] IN ('39F491C5-D6AC-4A9B-8AC0-C431CB17D588', '41540783-D9EF-4C70-8F1D-C9E83D91ED5F', '8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061', 'B91BA046-BC1E-400C-B85D-638C1F4E0CE2', '368DD475-242C-49C4-A42C-7278BE690CC2' )
                AND [IsSystem] = 0" );
        }

        /// <summary>
        /// NA: Re-fix #3497
        /// </summary>
        private void FixIssue3497()
        {
            // Re-Fixes #3497
            Sql( @"
                IF NOT EXISTS (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'b91ba046-bc1e-400c-b85d-638c1f4e0ce2' )
                BEGIN
                    SET IDENTITY_INSERT [dbo].[DefinedValue] ON
                    INSERT INTO [dbo].[DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Value], [Description], [Guid], [IsActive] )
                    VALUES (66, 1, 4, 2, N'Visitor', N'Used when a person first enters through your first-time visitor process. As they continue to attend they will become an attendee and possibly a member.', N'b91ba046-bc1e-400c-b85d-638c1f4e0ce2', 1)
                    SET IDENTITY_INSERT [dbo].[DefinedValue] OFF
                END" );
        }

        /// <summary>
        /// GJ: Motivators Update
        /// </summary>
        private void MotivatorsUpdate()
        {
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
<div style=""margin: 0;max-width:280px"">
{[ chart type:'gauge' backgroundcolor:'#f13c1f,#f0e3ba,#0e9445,#3f56a1' gaugelimits:'0,2,17,85,100' chartheight:'150px']}
    [[ dataitem value:'{{ GrowthScore }}' fillcolor:'#484848' ]] [[ enddataitem ]]
{[ endchart ]}
</div>
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
        /// GJ: Segmented Chart Update
        /// </summary>
        private void SegmentedChartUpdate()
        {
            Sql( MigrationSQL._201907082136571_Rollup0708_SegmentedChartUpdate );
        }

        /// <summary>
        /// ED: EF Up for Add model fields to v9.0 (needed for Pushpay).
        /// </summary>
        private void AddFinancialTransactionNoCashAssetTypeValueIdUp()
        {
            AddColumn("dbo.FinancialTransaction", "NonCashAssetTypeValueId", c => c.Int(nullable: true));
            CreateIndex("dbo.FinancialTransaction", "NonCashAssetTypeValueId");
            AddForeignKey("dbo.FinancialTransaction", "NonCashAssetTypeValueId", "dbo.DefinedValue", "Id");
        }

        /// <summary>
        /// ED: EF Down for Add model fields to v9.0 (needed for Pushpay).
        /// </summary>
        private void AddFinancialTransactionNoCashAssetTypeValueIdDown()
        {
            DropForeignKey("dbo.FinancialTransaction", "NonCashAssetTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.FinancialTransaction", new[] { "NonCashAssetTypeValueId" });
            DropColumn("dbo.FinancialTransaction", "NonCashAssetTypeValueId");
        }

        /// <summary>
        /// ED: Fix the duplicate defined type category "Personality Assessments"
        /// </summary>
        private void FixDefinedTypeCategoryPersonalityAssessments()
        {
            Sql( @"
                DECLARE @badGuid UNIQUEIDENTIFIER = (SELECT [Guid] FROM [dbo].[Category] WHERE [Name] = 'Personality Assessments' AND [Guid] <> '6A259E9A-232F-4835-B3F0-B06376A13997')
                DECLARE @badId INT = (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = @badGuid)

                UPDATE DefinedType
                SET CategoryId = (
	                SELECT [Id] 
	                FROM [dbo].[Category] 
	                WHERE [Guid] = '6A259E9A-232F-4835-B3F0-B06376A13997')
                WHERE [CategoryId] = @badId

                DELETE FROM [dbo].[Category] WHERE [Guid] = @badGuid" );
        }

        /// <summary>
        /// SK: Add Event Wizard to Menu in v9.0 (up)
        /// </summary>
        private void AddEventWizardPageUp()
        {
            RockMigrationHelper.AddPage( true, "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Event Wizard", "", "7F889C16-0656-4015-8A90-B43D3BD2467E", "" ); // Site:Rock RMS
            RockMigrationHelper.AddBlock( true, "7F889C16-0656-4015-8A90-B43D3BD2467E".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B1C7E983-5000-4CBE-84DD-6B7D428635AC".AsGuid(), "Event Registration Wizard", "Main", @"", @"", 0, "3DFF583A-1440-461C-B84F-48513A3AD425" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "B1C7E983-5000-4CBE-84DD-6B7D428635AC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Instance Page", "RegistrationInstancePage", "", @"Determines which page the link in the final confirmation screen will take you to.", 4, @"844DC54B-DAEC-47B3-A63A-712DD6D57793", "60A72E53-D71D-40FC-AF9C-FD12AB8D9BE1" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "B1C7E983-5000-4CBE-84DD-6B7D428635AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Calendar Items", "IncludeInactiveCalendarItems", "", @"Check this box to hide inactive calendar items.", 11, @"True", "6BA0E750-1BDF-4430-AEFB-74EA0FB35D2F" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "B1C7E983-5000-4CBE-84DD-6B7D428635AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Link to Event Details Page on Confirmation Screen", "DisplayEventDetailsLink", "", @"Check this box to show the link to the event details page in the wizard confirmation screen.", 14, @"False", "58892547-336C-4E80-B509-D189A3377607" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "B1C7E983-5000-4CBE-84DD-6B7D428635AC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "External Event Details Page", "EventDetailsPage", "", @"Determines which page the link in the final confirmation screen will take you to (if ""Display Link to Event Details ... "" is selected).", 15, @"8A477CC6-4A12-4FBE-8037-E666476DD413", "2819F990-04F4-4EB8-8D95-2E603AA3FF7C" );
        }

        /// <summary>
        /// SK: Add Event Wizard to Menu in v9.0 (Down)
        /// </summary>
        private void AddEventPageDown()
        {
            RockMigrationHelper.DeleteAttribute( "2819F990-04F4-4EB8-8D95-2E603AA3FF7C" );
            RockMigrationHelper.DeleteAttribute( "58892547-336C-4E80-B509-D189A3377607" );
            RockMigrationHelper.DeleteAttribute( "6BA0E750-1BDF-4430-AEFB-74EA0FB35D2F" );
            RockMigrationHelper.DeleteAttribute( "60A72E53-D71D-40FC-AF9C-FD12AB8D9BE1" );
            RockMigrationHelper.DeleteBlock( "3DFF583A-1440-461C-B84F-48513A3AD425" );
            RockMigrationHelper.DeletePage( "7F889C16-0656-4015-8A90-B43D3BD2467E" ); //  Page: Event Wizard, Layout: Full Width, Site: Rock RMS
        }

        /// <summary>
        /// GJ: ChartJS Fix Migration
        /// </summary>
        private void FixChartJS()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Parameters]=N'fillcolor^rgba(5,155,255,.6)|bordercolor^#059BFF|borderwidth^0|legendposition^bottom|legendshow^false|chartheight^400px|chartwidth^100%|tooltipshow^true|yaxislabels^#777|fontfamily^sans-serif|tooltipbackgroundcolor^#000|type^bar|pointradius^3|pointcolor^#059BFF|pointbordercolor^#059BFF|pointborderwidth^0|pointhovercolor^rgba(5,155,255,.6)|pointhoverbordercolor^rgba(5,155,255,.6)|borderdash^|curvedlines^true|filllinearea^false|labels^|tooltipfontcolor^#fff|pointhoverradius^3|xaxistype^linear|yaxislabels^|yaxismin^|yaxismax^|yaxisstepsize^'
                WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA'" );
        }
    }
}
