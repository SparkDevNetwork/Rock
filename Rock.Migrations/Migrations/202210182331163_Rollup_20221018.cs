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
    public partial class Rollup_20221018 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateWorkflowActionForms();
            FixEasyPieLavaShortcode();
            UpdateTransactionEntryV2FeeCoverageMessage_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// KA: Migration to Update WorkflowActionForms with missing [NotificationSystemCommunicationId]
        /// </summary>
        private void UpdateWorkflowActionForms()
        {
            Sql( @"UPDATE wfaf SET [NotificationSystemCommunicationId] = sc.[Id]
                FROM [WorkflowActionForm] wfaf
                INNER JOIN [SystemEmail] se ON se.[Id] = wfaf.[NotificationSystemEmailId]
                INNER JOIN [SystemCommunication] sc ON sc.[Guid] = se.[Guid]
                WHERE [NotificationSystemCommunicationId] IS NULL
                AND [NotificationSystemEmailId] IS NOT NULL" );
        }

        /// <summary>
        /// GJ: Fix Easy Pie Lava Shortcode Documentation
        /// </summary>
        private void FixEasyPieLavaShortcode()
        {
            Sql( @"
                UPDATE [LavaShortcode]
                SET [Documentation]=N'<p>Easy Pie Chart is the perfect solution when you need to display a single percentage value on a chart. In fact it''s as simple as <code>{[easypie value:''60'']}{[endeasypie]}</code></p>
<p><img src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/easypie-main.png"" class=""img-responsive"" alt=""Easy Pie"" width=""100"" height=""100"" loading=""lazy""></p>
<p>Each has the following basic settings settings:</p>
<ul>
<li><strong>value</strong> (0) - The data point for the item in percent (0 - 100)</li>
<li><strong>chartwidth</strong> (95px) - The width of the chart in pixels.</li>
</ul>
<p>Advanced options include:</p>
<ul>
<li><strong>primarycolor</strong> (#ee7625) - The color of the circular bar.</li>
<li><strong>label</strong> - Optional label to display inside the chart.</li>
<li><strong>valuesize</strong> - Font size of the rendered percentage value.</li>
<li><strong>labelsize</strong> - Font size of the label.</li>
<li><strong>trackcolor</strong> (rgba(0,0,0,0.04)) - The CSS color of the track for the bar.</li>
<li><strong>scalelinelength</strong> (0px) - The length of the scale lines.</li>
<li><strong>scalelinecolor</strong> (#dfe0e0) - The CSS color of the scale lines.</li>
<li><strong>linecap</strong> (none) - Defines how the ending of the bar line looks like. (Possible values are none, round and square).</li>
<li><strong>trackwidth</strong>  - Width of the bar line in pixels. Default value is computed based on the chart width.</li>
<li><strong>animateduration</strong> (1500) - Time in milliseconds for a eased animation of the bar growing, or 0 to deactivate.</li>
</ul>
<h5>Advanced Options</h5>
<ul>
<li><strong>cssclasstarget</strong> - Set value to create many of the same type of chart.</li>
</ul>
<h3>Avatar</h3>
<p>Add a avatar to the center of the pie chart just by adding a image.</p>
<h3>Nested Charts</h3>
<p>Add an additional item named [[ easypie ]] using the same attributes</p>
<p>Example Charts and Markup is shown below:</p>
<p><img src=""https://rockrms.blob.core.windows.net/documentation/Lava/Shortcodes/easypie-examples.png"" class=""img-responsive"" alt=""Easy Pie Examples"" width=""607"" height=""121"" loading=""lazy""></p>
<pre><code>/* Small Chart (A) */
{[ easypie value:''75'' scalelinelength:''0'' ]} {[ endeasypie ]}

/* Chart With Scalelines and Color (B) */
{[easypie value:''90'' chartwidth:''120'' scalelinelength:''8'' primarycolor:''#16C98D'']}
{[ endeasypie]}

/* Chart Avatar (C) */
{[easypie value:''90'' scalelinelength:''0'' chartwidth:''120'' primarycolor:''#D4442E'']}
&lt;img src=&quot;https://rock.rocksolidchurchdemo.com/GetImage.ashx?id=69&quot; alt=&quot;Ted Decker&quot;&gt;
{[ endeasypie ]}

/* Chart with Labels (D) */
{[ easypie value:''90'' scalelinelength:''0'' label:''Memory'' showpercent:''true'' primarycolor:''#FFC870'' chartwidth:''120'']} {[ endeasypie ]}

/* Nested Chart (E) */
{[easypie value:''90'' scalelinelength:''0'' chartwidth:''120'' primarycolor:''#009CE3'']}
[[easypie value:''50'' primarycolor:''#16C98D'']][[endeasypie]]
{[ endeasypie ]}
</code></pre>
'
                WHERE ([Guid]='96A8284E-96A6-4E38-969C-640F0BDC8EB8')" );
        }
    
        /// <summary>
        /// MP: Fix TransactionEntryV2 FeeCoverageMessage
        /// </summary>
        private void UpdateTransactionEntryV2FeeCoverageMessage_Up()
        {
            Sql( @"
                DECLARE @transactionEntryV2BlockTypeId INT = (
                        SELECT TOP 1 Id
                        FROM BlockType
                        WHERE Guid = '6316D801-40C0-4EED-A2AD-55C13870664D'
                        )
                DECLARE @feeCoverageMessageAttributeId INT = (
                        SELECT TOP 1 Id
                        FROM Attribute
                        WHERE [Key] = 'FeeCoverageMessage'
                            AND EntityTypeQualifierColumn = 'BlockTypeId'
                            AND EntityTypeQualifierValue = cast(@transactionEntryV2BlockTypeId AS NVARCHAR(100))
                        )
                    , @replaceText NVARCHAR(100) = '{{ Percentage }}% ({{ AmountHTML }})'
                    , @replaceWithText NVARCHAR(100) = '{%if IsPercentage %} {{ Percentage }}% ({{ AmountHTML }}) {% else %} {{ AmountHTML }} {% endif %}'

                IF (@feeCoverageMessageAttributeId IS NOT NULL)
                BEGIN
                    UPDATE Attribute
                    SET [DefaultValue] = Replace([DefaultValue], @replaceText, @replaceWithText)
                    WHERE Id = @feeCoverageMessageAttributeId

                    UPDATE AttributeValue
                    SET [Value] = Replace([Value], @replaceText, @replaceWithText)
                    WHERE AttributeId = @feeCoverageMessageAttributeId
                END" );
        }
    }
}
