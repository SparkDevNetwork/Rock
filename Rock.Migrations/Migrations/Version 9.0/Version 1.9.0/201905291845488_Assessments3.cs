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
    public partial class Assessments3 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDiscAssessmentBlockAttributes();
            AttributeValuesBlockUseAbbreviatedNameAttribute();
            UpdateSpiritualGiftsDefinedValues();
            UpdateSpiritualGiftsResultsAndInstructions();
            UpdateDiscIntoText();
            RemoveAttributesFromSpiritualGiftsDefinedType();
            RemovePostHtmlFromPersonSecurityTab();
            UpdateChartShortcodeForGaugeChartUp();
            CreateSpiritualGiftsAttributeCategory();
            SetAssessmentAttributeValuesBlockSettingsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            SetAssessmentAttributeValuesBlockSettingsDown();
            UpdateChartShortcodeForGaugeChartDown();
        }

        /// <summary>
        /// ED: Add Disc Assessment Block Attributes
        /// </summary>
        private void AddDiscAssessmentBlockAttributes()
        {
            // Attrib for BlockType: DISC:Set Page Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "A161D12D-FEA7-422F-B00E-A689629680E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Title", "SetPageTitle", "", @"The text to display as the heading.", 1, @"DISC Assessment", "95148E83-B7B2-4984-8945-BC4B4850E4FA" );

            // Attrib for BlockType: DISC:Set Page Icon
            RockMigrationHelper.UpdateBlockTypeAttribute( "A161D12D-FEA7-422F-B00E-A689629680E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Icon", "SetPageIcon", "", @"The css class name to use for the heading icon.", 2, @"fa fa-chart-bar", "12B6475E-8685-4419-99F7-6439AE851BF3" );
        }

        /// <summary>
        /// ED: Add Attribute Values Block Attribute
        /// </summary>
        private void AttributeValuesBlockUseAbbreviatedNameAttribute()
        {
            // Attrib for BlockType: Attribute Values:Use Abbreviated Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Abbreviated Name", "UseAbbreviatedName", "", @"Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.", 2, @"False", "51693680-B03C-468B-A771-CD8C103D0B1B" );
        }

        /// <summary>
        /// GJ: Update Spiritual Gifts
        /// </summary>
        private void UpdateSpiritualGiftsDefinedValues()
        {
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Administration", "The gift of administration means you are skilled at developing, articulating and accomplishing tasks for the accomplishment of the objectives for the Body.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_ADMINISTRATION, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Apostleship", "The gift of apostleship means you are able to articulate the Gospel in places that are geographically, socially, ethnically or culturally different from one's background.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_APOSTLESHIP, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Discernment", "The gift of discernment allows you to perceive the motives, intentions and sincerity of others within relationships of the local church.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_DISCERNMENT, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Encouragement", "The gift of encouragement allows you to counsel, model and encourage people through one's personal testimony, life and Scripture so people are comforted and encouraged to act.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_ENCOURAGEMENT, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Evangelism", "The gift of evangelism assists you in sharing the Good News of Christ in a relevant manner with people far from Christ so they respond positively to become disciples of Jesus Christ.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_EVANGELISM, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Faith", "The gift of faith helps you envision with clarity and confidence God's future direction and goals for the Body of Christ.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_FAITH, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Giving", "The gift of giving allows you to give liberally to meet the needs of others and support God's ministry with the resources which God has entrusted to you.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_GIVING, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Service", "The gift of service means you unselfishly meet the needs of other people through practical service that is most often done behind the scenes.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_HELPS, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Hospitality", "The gift of hospitality enables you to reach out to others and welcome them into your home and life in a loving, warm manner. These guests feel “at home” in your presence.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_HOSPITALITY, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Knowledge", "The gift of knowledge	allows you to discover, analyze, accumulate, systematize and articulate ideas that are essential for the growth and building up of the Body.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_KNOWLEDGE, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Leadership", "The gift of leadership allows you to set Godly objectives, make decisions and communicate them to the Body of Christ in a way that motivates them to willingly follow and joyously work to accomplish these objectives.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_LEADERSHIP, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Mercy", "The gift of mercy empowers you to empathize with those in need, especially those suffering and in trauma, and to manifest empathy so those in need are strengthened.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_MERCY, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Shepherding", "The gift of shepherding helps you nurture a group of believers by caring for their spiritual welfare in a holistic manner on a longer-term basis.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PASTOR_SHEPHERD, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Discipling", "The gift of discipling assists you in instructing, guiding and caring for believers in a smaller expression of the Body so that they are equipped to reach out in ministry to others, both inside and outside the church.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PASTOR_TEACHER, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Prophecy", "The gift of prophecy means you are able to publicly or privately speak the Word of God so people are convicted, consoled, encouraged, challenged or strengthened.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_PROPHECY, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Teaching", "The gift of teaching helps you to communicate instruction so members of the Body understand how to apply spiritual principles to their own lives and ministries in every-day context.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_TEACHING, false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Wisdom", "The gift of wisdom means you have insight on how knowledge may best be applied to specific needs in the Body of Christ. This involves applying Biblical truths to everyday situations.", Rock.SystemGuid.DefinedValue.SPIRITUAL_GIFTS_WISDOM, false );
        }

        /// <summary>
        /// GJ: Spiritual Gifts Text & Results formatting
        /// </summary>
        private void UpdateSpiritualGiftsResultsAndInstructions()
        {
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 0, @"
<div class='row'>
    <div class='col-md-12'>
    <h2 class='h2'> Dominant Gifts</h2>
    </div>
    <div class='col-md-9'>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                        <th>
                            Spiritual Gift
                        </th>
                        <th>
                            You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% if DominantGifts != empty %}
                        {% for dominantGift in DominantGifts %}
                            <tr>
                                <td>
                                    {{ dominantGift.Value }}
                                </td>
                                <td>
                                    {{ dominantGift.Description }}
                                </td>
                            </tr>
                        {% endfor %}
                    {% else %}
                        <tr>
                            <td colspan='2'>
                                You did not have any Dominant Gifts
                            </td>
                        </tr>
                    {% endif %}
                </tbody>
            </table>
        </div>
    </div>
</div>

<div class='row'>
    <div class='col-md-12'>
        <h2 class='h2'> Supportive Gifts</h2>
    </div>
    <div class='col-md-9'>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                    <th>
                        Spiritual Gift
                        </th>
                        <th>
                        You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% if SupportiveGifts != empty %}
                        {% for supportiveGift in SupportiveGifts %}
                            <tr>
                                <td>
                                    {{ supportiveGift.Value }}
                                </td>
                                <td>
                                    {{ supportiveGift.Description }}
                                </td>
                            </tr>
                        {% endfor %}
                    {% else %}
                        <tr>
                            <td colspan='2'>
                                You did not have any Supportive Gifts
                            </td>
                        </tr>
                    {% endif %}
                </tbody>
            </table>
        </div>
    </div>
</div?
<div class='row'>
    <div class='col-md-12'>
        <h2 class='h2'> Other Gifts</h2>
    </div>
    <div class='col-md-9'>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                    <th>
                        Spiritual Gift
                        </th>
                        <th>
                        You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% if OtherGifts != empty %}
                        {% for otherGift in OtherGifts %}
                            <tr>
                                <td>
                                    {{ otherGift.Value }}
                                </td>
                                <td>
                                    {{ otherGift.Description }}
                                </td>
                            </tr>
                        {% endfor %}
                    {% else %}
                        <tr>
                            <td colspan='2'>
                                You did not have any Other Gifts
                            </td>
                        </tr>
                    {% endif %}
            </tbody>
            </table>
        </div>
    </div>
</div>", "85256610-56EB-4E6F-B62B-A5517B54B39E" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Instructions", "Instructions", "", @"The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 0, @"<h2>Welcome to Your Spiritual Gifts Assessment</h2>
<p>
    {{ Person.NickName }}, we are all called to a unique role in the church body, and are equipped with
    the gifts required for this calling. This assessment identifies the common spiritual gifts that
    you possess.
</p>
<p>
    Don’t spend too much time thinking about your answer. Usually, your first responses is your most
    natural. Since there are no right or wrong answers, just go with your instinct.
</p>", "86C9E794-B678-4453-A831-FE348A440646" );
        }

        /// <summary>
        /// GJ: Update DISC Intro Text
        /// </summary>
        private void UpdateDiscIntoText()
        {
            string oldValue = @"
            <h2>Welcome!</h2>
            <p>
                {{ Person.NickName }}, in this assessment you are given a series of questions, each containing four phrases.
                Select one phrase that MOST describes you and one phrase that LEAST describes you.
            </p>
            <p>
                This assessment is environmentally sensitive, which means that you may score differently
                in different situations. In other words, you may act differently at home than you
                do on the job. So, as you complete the assessment you should focus on one environment
                for which you are seeking to understand yourself. For instance, if you are trying
                to understand yourself in marriage, you should only think of your responses to situations
                in the context of your marriage. On the other hand, if you want to know your behavioral
                needs on the job, then only think of how you would respond in the job context.
            </p>
            <p>
                One final thought as you give your responses. On these kinds of assessments, it
                is often best and easiest if you respond quickly and do not deliberate too long
                on each question. Your response on one question will not unduly influence your scores,
                so simply answer as quickly as possible and enjoy the process. Don''t get too hung
                up, if none of the phrases describe you or if there are some phrases that seem too
                similar, just go with your instinct.
            </p>
            <p>
                When you are ready, click the ''Start'' button to proceed.
            </p>
";
            string newValue = @"
<h2>Welcome!</h2>
<p>
    {{ Person.NickName }}, our behaviors are influenced by our natural personality wiring. This assessment
    evaluates your essential approach to the world around you and how that drives your behavior.
</p>
<p>
    For best results with this assessment, picture a setting such as the workplace, at home or with friends,
    and keep that same setting in mind as you answer all the questions. Your responses may be different in
    different circumstances.
</p>
<p>
    Don’t spend too much time thinking about your answer. Usually, your first responses is your most natural.
    Since there are no right or wrong answers, just go with your instinct.
</p>";
            RockMigrationHelper.UpdateBlockAttributeValue( "A161D12D-FEA7-422F-B00E-A689629680E4", "5F2BA045-D481-4908-A137-DE28C71AAF67", newValue, oldValue );
        }

        /// <summary>
        /// SK: Delete the Details and Scripture Attributes and Update the Defined Values of the Spiritual Gifts Defined Type
        /// </summary>
        private void RemoveAttributesFromSpiritualGiftsDefinedType()
        {
            Sql( @"DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid]='C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583')
                DELETE FROM [AttributeValue] WHERE [AttributeId] = @AttributeId" );

            Sql( @"DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid]='EADE1EC5-E05B-4956-973E-BBBA2A02F0C0')
                DELETE FROM [AttributeValue] WHERE [AttributeId] = @AttributeId" );

            RockMigrationHelper.DeleteAttribute( "C1DBD74B-5B31-4E5D-B800-FF0B4C0DC583" );
            RockMigrationHelper.DeleteAttribute( "EADE1EC5-E05B-4956-973E-BBBA2A02F0C0" );

            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Administration", "The gift of administration means you are skilled at developing, articulating and accomplishing tasks for the accomplishment of the objectives for the Body.", "A276421D-F662-4723-99DA-6FDF3E9CFF7C", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Apostleship", "The gift of apostleship means you are able to articulate the Gospel in places that are geographically, socially, ethnically or culturally different from one's background.", "A2C7074E-AC97-4D89-9240-47A552CDC4C0", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Discernment", "The gift of discernment allows you to perceive the motives, intentions and sincerity of others within relationships of the local church.", "3EB352F3-F624-4ED6-A9EE-7951B71B1952", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Discipling", "The gift of discipling assists you in instructing, guiding and caring for believers in a smaller expression of the Body so that they are equipped to reach out in ministry to others, both inside and outside the church.", "C7291F22-05F0-4EF9-A7C2-2CFEBFEBCB45", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Encouragement", "The gift of encouragement allows you to counsel, model and encourage people through one's personal testimony, life and Scripture so people are comforted and encouraged to act.", "809F65A6-1759-472A-8B8B-F37009F476BF", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Evangelism", "The gift of evangelism assists you in sharing the Good News of Christ in a relevant manner with people far from Christ so they respond positively to become disciples of Jesus Christ.", "0F8D41AA-7236-40BF-AA37-980BCCF4A881", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Faith", "The gift of faith helps you envision with clarity and confidence God's future direction and goals for the Body of Christ.", "7B30E2BA-9461-4688-9B43-D2B774E33A18", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Giving", "The gift of giving allows you to give liberally to meet the needs of others and support God's ministry with the resources which God has entrusted to you.", "C4259D6E-675C-417B-9175-6D599C86A204", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Hospitality", "The gift of hospitality enables you to reach out to others and welcome them into your home and life in a loving, warm manner. These guests feel ''at home'' in your presence.", "98D5EE08-633D-4635-80CD-169449604D18", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Knowledge", "The gift of knowledge allows you to discover, analyze, accumulate, systematize and articulate ideas that are essential for the growth and building up of the Body.", "462A5D10-6DEA-43D7-96EF-8F82FF1E2E14", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Leadership", "The gift of leadership allows you to set Godly objectives, make decisions and communicate them to the Body of Christ in a way that motivates them to willingly follow and joyously work to accomplish these objectives.", "A1CB038C-AAFC-4745-A7D2-7C8BA5028F05", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Mercy", "The gift of mercy empowers you to empathize with those in need, especially those suffering and in trauma, and to manifest empathy so those in need are strengthened.", "0894EDBA-8FC8-4433-877C-53351A06A8B7", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Prophecy", "The gift of prophecy means you are able to publicly or privately speak the Word of God so people are convicted, consoled, encouraged, challenged or strengthened.", "4ADAEED1-D0E6-4DA4-A0BA-8E7D058075C4", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Service", "The gift of service means you unselfishly meet the needs of other people through practical service that is most often done behind the scenes", "13C40209-F41D-4C1D-83D3-2EC530588245", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Shepherding", "The gift of shepherding helps you nurture a group of believers by caring for their spiritual welfare in a holistic manner on a longer-term basis.", "FC4F1B46-F0C3-45B0-9FD9-D15F4FD05A31", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Teaching", "The gift of teaching helps you to communicate instruction so members of the Body understand how to apply spiritual principles to their own lives and ministries in every-day context.", "E8278791-2400-4DDA-AEAA-C6F11E0AC9D0", false );
            RockMigrationHelper.UpdateDefinedValue( "9D9628F0-7FC5-411E-B9DF-740AA17689A0", "Wisdom", "The gift of wisdom means you have insight on how knowledge may best be applied to specific needs in the Body of Christ. This involves applying Biblical truths to everyday situations.", "5F1F5A92-D981-4027-A4BC-C3642E784D0B", false );
        }

        /// <summary>
        /// SK: Removed PostHtml from security tab
        /// </summary>
        private void RemovePostHtmlFromPersonSecurityTab()
        {
            Sql( @"
                UPDATE [dbo].[Block]
                SET [PostHtml] = ''
                WHERE [Guid] = N'68d34ec2-0a10-4344-89e3-e6df99951fdb'
                    AND [PostHtml] = N'<div class=''pull-right''><a href=''~/SecurityRoles'' class=''btn btn-link''>Manage Security Roles</a></div>'" );
        }

        /// <summary>
        /// GJ: Create migration for TS Guage adding to Chart Lava Shortcode
        /// </summary>
        private void UpdateChartShortcodeForGaugeChartUp()
        {
            string markup = @"{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}

{%- if type == ''gauge'' or type == ''tsgauge'' -%}
{%- assign type = ''tsgauge'' -%}
{% javascript url:''~/Scripts/Chartjs/Gauge.js'' id:''gaugejs''%}{% endjavascript %}
{%- endif -%}

{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}

{%- if type == ''tsgauge'' -%}
  {% assign backgroundColor = backgroundcolor | Split:'','' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
  {% assign gaugeLimits = gaugelimits | Split:'','' | Join:'','' | Prepend:''['' | Append:'']'' %}
  {%- assign tooltipshow = false -%}
  {%- capture seriesData -%}
  {
      backgroundColor: {{ backgroundColor }},
      borderWidth: {{ borderwidth }},
      gaugeData: {
        value: {{dataitems[0].value}},
        valueColor: ""{{dataitems[0].fillcolor | Default:''#000000''}}""
      },
      gaugeLimits: {{ gaugeLimits }}
  }
  {%- endcapture -%}
{% else %}
  {% assign dataitemCount = dataitems | Size -%}
  {% if dataitemCount > 0 -%}
      {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign firstDataItem = dataitems | First  %}

      {% capture seriesData -%}
      {
          label: ''{{ label }}'',
          fill: {{ filllinearea }}, 
          backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
          borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
          borderWidth: {{ borderwidth }},
          pointRadius: {{ pointradius }},
          pointBackgroundColor: ''{{ pointcolor }}'',
          pointBorderColor: ''{{ pointbordercolor }}'',
          pointBorderWidth: {{ pointborderwidth }},
          pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
          pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
          pointHoverRadius: ''{{ pointhoverradius }}'',
          {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
          {% if curvedlines == false -%} lineTension: 0,{% endif -%}
          data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
      }
      {% endcapture -%}
      {% assign labels = dataitems | Map:''label'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' -%}
  {% else -%}
      {% if labels == '''' -%}
          <div class=""alert alert-warning"">
              When using datasets you must provide labels on the shortcode to define each unit of measure.
              {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
          </div>
      {% else %}
          {% assign labelItems = labels | Split:'','' -%}
          {% assign labels = ''""''- %}
          {% for labelItem in labelItems -%}
              {% assign labelItem = labelItem | Trim %}
              {% assign labels = labels | Append:labelItem | Append:''"",""'' %}
          {% endfor -%}
          {% assign labels = labels | ReplaceLast:''"",""'',''""'' %}
      {% endif -%}
      {% assign seriesData = '''' -%}
      {% for dataset in datasets -%}
          {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
          {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
          {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
          {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
          {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
          {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
          {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
          {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
          {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
          {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
          {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
          {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign pointHoverRadius = pointhoverradius %} {% endif -%}
          
          {% capture itemData -%}
              {
                  label: ''{{ datasetLabel }}'',
                  fill: {{ filllinearea }}, // 1
                  backgroundColor: ''{{ datasetFillColor }}'',
                  borderColor: ''{{ datasetBorderColor }}'',
                  borderWidth: {{ datasetBorderWidth }},
                  pointRadius: {{ datasetPointRadius }},
                  pointBackgroundColor: ''{{ datasetPointColor }}'',
                  pointBorderColor: ''{{ datasetPointBorderColor }}'',
                  pointBorderWidth: {{ datasetPointBorderWidth }},
                  pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                  pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                  pointHoverRadius: ''{{ pointhoverradius }}'',
                  {% if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{% endif -%}
                  {% if dataset.curvedlines and dataset.curvedlines == ''false''-%} lineTension: 0,{% endif -%}
                  data: [{{ dataset.data }}]
              },
          {% endcapture -%}

          {% assign seriesData = seriesData | Append:itemData -%}
      {% endfor -%}
      {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
  {% endif -%}
{%- endif -%}

<div class=""chart-container"" style=""position: relative; height:{{ chartheight }}; width:{{ chartwidth }}"">
    <canvas id=""chart-{{ id }}""></canvas>
</div>

<script>

var options = {
  maintainAspectRatio: false,
  {%- if type != ''tsgauge'' -%}
    legend: {
        position: ''{{ legendposition }}'',
        display: {{ legendshow }}
    },
    tooltips: {
        enabled: {{ tooltipshow }},
        backgroundColor: ''{{ tooltipbackgroundcolor }}'',
        bodyFontColor: ''{{ tooltipfontcolor }}'',
        titleFontColor: ''{{ tooltipfontcolor }}''
    }
    {%- else -%}
    events: [],
    showMarkers: false
    {%- endif -%}
    {% if xaxistype == ''time'' %}
        ,scales: {
        xAxes: [{
            type: ""time"",
            display: true,
            scaleLabel: {
                display: true,
                labelString: ''Date''
            }
        }],
        yAxes: [{
            display: true,
            scaleLabel: {
                display: true,
                labelString: ''value''
            }
        }]
    }
    {% elseif xaxistype == ''linearhorizontal0to100'' %}
        ,scales: {
        xAxes: [{
                ticks: {
                    min: 0,
                    max: 100
                }
        }],
        yAxes: [{
            gridLines: {
                display: false
              }
        }]
    }
    {% endif %}
};
{%- if type == ''tsgauge'' -%}
var data = {
    datasets: [{{ seriesData }}]
};
{%- else -%}
var data = {
    labels: [{{ labels }}],
    datasets: [{{ seriesData }}],
    borderWidth: {{ borderwidth }}
};
{% endif %}

Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
Chart.defaults.global.defaultFontFamily = ""{{ fontfamily }}"";

var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
var chart = new Chart(ctx, {
    type: ''{{ type }}'',
    data: data,
    options: options
});    

</script>";
            string parameters = @"fillcolor^rgba(5%2C155%2C255%2C.6)|bordercolor^#059BFF|borderwidth^0|legendposition^bottom|legendshow^false|chartheight^400px|chartwidth^100%|tooltipshow^true|fontcolor^#777|fontfamily^sans-serif|tooltipbackgroundcolor^#000|type^bar|pointradius^3|pointcolor^#059BFF|pointbordercolor^#059BFF|pointborderwidth^0|pointhovercolor^rgba(5%2C155%2C255%2C.6)|pointhoverbordercolor^rgba(5%2C155%2C255%2C.6)|borderdash^|curvedlines^true|filllinearea^false|labels^|tooltipfontcolor^#fff|pointhoverradius^3|xaxistype^linear|backgroundcolor^|gaugelimits^";

            string qry = $@"
                UPDATE [dbo].[LavaShortcode]
                SET [Markup] = '{markup}', [Parameters] = '{parameters}'
                WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA'";

            Sql( qry );
        }

        /// <summary>
        /// Revert GJ: Create migration for TS Guage adding to Chart Lava Shortcode
        /// </summary>
        private void UpdateChartShortcodeForGaugeChartDown()
        {
            string markup = @"{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}

{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}

{% assign dataitemCount = dataitems | Size -%}
{% if dataitemCount > 0 -%}
    {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
    {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
    {% assign firstDataItem = dataitems | First  %}

    {% capture seriesData -%}
    {
        label: ''{{ label }}'',
        fill: {{ filllinearea }}, 
        backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
        borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
        borderWidth: {{ borderwidth }},
        pointRadius: {{ pointradius }},
        pointBackgroundColor: ''{{ pointcolor }}'',
        pointBorderColor: ''{{ pointbordercolor }}'',
        pointBorderWidth: {{ pointborderwidth }},
        pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
        pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
        pointHoverRadius: ''{{ pointhoverradius }}'',
        {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
        {% if curvedlines == false -%} lineTension: 0,{% endif -%}
        data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
    }
    {% endcapture -%}
    {% assign labels = dataitems | Map:''label'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' -%}
{% else -%}
    {% if labels == '''' -%}
        <div class=""alert alert-warning"">
            When using datasets you must provide labels on the shortcode to define each unit of measure.
            {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
        </div>
    {% else %}
        {% assign labelItems = labels | Split:'','' -%}
        {% assign labels = ''""''- %}
        {% for labelItem in labelItems -%}
            {% assign labelItem = labelItem | Trim %}
            {% assign labels = labels | Append:labelItem | Append:''"",""'' %}
        {% endfor -%}
        {% assign labels = labels | ReplaceLast:''"",""'',''""'' %}
    {% endif -%}
    {% assign seriesData = '''' -%}
    {% for dataset in datasets -%}
        {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
        {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
        {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
        {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
        {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
        {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
        {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
        {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
        {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
        {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
        {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
        {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign pointHoverRadius = pointhoverradius %} {% endif -%}
        
        {% capture itemData -%}
            {
                label: ''{{ datasetLabel }}'',
                fill: {{ filllinearea }}, // 1
                backgroundColor: ''{{ datasetFillColor }}'',
                borderColor: ''{{ datasetBorderColor }}'',
                borderWidth: {{ datasetBorderWidth }},
                pointRadius: {{ datasetPointRadius }},
                pointBackgroundColor: ''{{ datasetPointColor }}'',
                pointBorderColor: ''{{ datasetPointBorderColor }}'',
                pointBorderWidth: {{ datasetPointBorderWidth }},
                pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                pointHoverRadius: ''{{ pointhoverradius }}'',
                {% if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{% endif -%}
                {% if dataset.curvedlines and dataset.curvedlines == ''false''-%} lineTension: 0,{% endif -%}
                data: [{{ dataset.data }}]
            },
        {% endcapture -%}

        {% assign seriesData = seriesData | Append:itemData -%}
    {% endfor -%}
    {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
{% endif -%}

<div class=""chart-container"" style=""position: relative; height:{{ chartheight }}; width:{{ chartwidth }}"">
    <canvas id=""chart-{{ id }}""></canvas>
</div>

<script>

var options = {
    maintainAspectRatio: false,
    legend: {
        position: ''{{ legendposition }}'',
        display: {{ legendshow }}
    },
    tooltips: {
        enabled: {{ tooltipshow }},
        backgroundColor: ''{{ tooltipbackgroundcolor }}'',
        bodyFontColor: ''{{ tooltipfontcolor }}'',
        titleFontColor: ''{{ tooltipfontcolor }}''
    }
    {% if xaxistype == ''time'' %}
        ,scales: {
        xAxes: [{
            type: ""time"",
            display: true,
            scaleLabel: {
                display: true,
                labelString: ''Date''
            }
        }],
        yAxes: [{
            display: true,
            scaleLabel: {
                display: true,
                labelString: ''value''
            }
        }]
    }
    {% elseif xaxistype == ''linearhorizontal0to100'' %}
        ,scales: {
        xAxes: [{
                ticks: {
                    min: 0,
                    max: 100
                }
        }],
        yAxes: [{
            gridLines: {
                display: false
              }
        }]
    }
    {% endif %}
};

var data = {
    labels: [{{ labels }}],
    datasets: [{{ seriesData }}],
    borderWidth: {{ borderwidth }}
};

Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
Chart.defaults.global.defaultFontFamily = ""{{ fontfamily }}"";

var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
var chart = new Chart(ctx, {
    type: ''{{ type }}'',
    data: data,
    options: options
});    

</script>";
            string parameters = @"fillcolor^rgba(5,155,255,.6)|bordercolor^#059BFF|borderwidth^0|legendposition^bottom|legendshow^false|chartheight^400px|chartwidth^100%|tooltipshow^true|fontcolor^#777|fontfamily^''OpenSans'',''Helvetica Neue'',Helvetica,Arial,sans-serif|tooltipbackgroundcolor^#000|type^bar|pointradius^3|pointcolor^#059BFF|pointbordercolor^#059BFF|pointborderwidth^0|pointhovercolor^rgba(5,155,255,.6)|pointhoverbordercolor^rgba(5,155,255,.6)|borderdash^|curvedlines^true|filllinearea^false|labels^|tooltipfontcolor^#fff|pointhoverradius^3|xaxistype^linear";

            string qry = $@"
                UPDATE [dbo].[LavaShortcode]
                SET [Markup] = '{markup}', [Parameters] = '{parameters}'
                WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA'";

            Sql( qry );
        }

        /// <summary>
        /// Creates the spiritual gifts attribute category and updates Dominant, Supportive, and other to use it.
        /// </summary>
        private void CreateSpiritualGiftsAttributeCategory()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "Spiritual Gifts", "fa fa-gift", "", "8E94CE6F-716D-4B6D-9BEC-C83385B90006" );
            var categories = new System.Collections.Generic.List<string> { "8E94CE6F-716D-4B6D-9BEC-C83385B90006" };
            
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"59D5A94C-94A0-4630-B80A-BB25697D74C7", categories, @"Dominant Gifts", @"", @"core_DominantGifts", @"", @"", 15, @"", @"F76FC75E-B33F-42B8-B360-15BA9A1F0F9A" );
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"59D5A94C-94A0-4630-B80A-BB25697D74C7", categories, @"Supportive Gifts", @"", @"core_SupportiveGifts", @"", @"", 16, @"", @"0499E359-3A7B-4138-A3EE-44CBF9750E33" );
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"59D5A94C-94A0-4630-B80A-BB25697D74C7", categories, @"Other Gifts", @"", @"core_OtherGifts", @"", @"", 17, @"", @"F33EC30E-7E5C-488E-AB48-81977CCFB185" );
        }

        private void SetAssessmentAttributeValuesBlockSettingsUp()
        {
            // Attrib for BlockType: Attribute Values:Show Category Names as Separators
            RockMigrationHelper.UpdateBlockTypeAttribute("D70A59DC-16BE-43BE-9880-59598FA7A94C","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Category Names as Separators","ShowCategoryNamesasSeparators","",@"Display the abbreviated name for the attribute if it exists, otherwise the full name is shown.",5,@"False","EF57237E-BA12-488A-9585-78466E4C3DB5");
            
            // Attrib for BlockType: Attribute Values:Set Page Icon
            RockMigrationHelper.UpdateBlockTypeAttribute("D70A59DC-16BE-43BE-9880-59598FA7A94C","9C204CD0-1233-41C5-818A-C5DA439445AA","Set Page Icon","SetPageIcon","",@"The css class name to use for the heading icon.",4,@"","DF437D92-FB5A-4625-851A-24F79412A337");
            
            // Attrib for BlockType: Attribute Values:Set Page Title
            RockMigrationHelper.UpdateBlockTypeAttribute("D70A59DC-16BE-43BE-9880-59598FA7A94C","9C204CD0-1233-41C5-818A-C5DA439445AA","Set Page Title","SetPageTitle","",@"The text to display as the heading.",3,@"","ADBFBED7-2A61-49ED-93EC-AF48B0247F34");
            
            // Attrib Value for Block:Assessments, Attribute:Use Abbreviated Name Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0C244AA1-2473-4749-8D7E-81CAA415C886","51693680-B03C-468B-A771-CD8C103D0B1B",@"True");
            
            // Attrib Value for Block:Assessments, Attribute:Show Category Names as Separators Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0C244AA1-2473-4749-8D7E-81CAA415C886","EF57237E-BA12-488A-9585-78466E4C3DB5",@"True");
            
            // Attrib Value for Block:Assessments, Attribute:Set Page Icon Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0C244AA1-2473-4749-8D7E-81CAA415C886","DF437D92-FB5A-4625-851A-24F79412A337",@"fa fa-project-diagram");
            
            // Attrib Value for Block:Assessments, Attribute:Set Page Title Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0C244AA1-2473-4749-8D7E-81CAA415C886","ADBFBED7-2A61-49ED-93EC-AF48B0247F34",@"Assessments");
            
            // Attrib Value for Block:Assessments, Attribute:Category Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("0C244AA1-2473-4749-8D7E-81CAA415C886","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"0b187c81-2106-4875-82b6-fbf1277ae23b,edd33f72-eced-49bc-ac49-3643b60ad736,0b6c7001-2d3a-4195-86ca-85c6dcbf2023,ceaa3d59-d53c-40ec-b7b8-e7bbb758bd4d,8e94ce6f-716d-4b6d-9bec-c83385b90006");
        }

        private void SetAssessmentAttributeValuesBlockSettingsDown()
        {
            // Attrib for BlockType: Attribute Values:Set Page Title
            RockMigrationHelper.DeleteAttribute("ADBFBED7-2A61-49ED-93EC-AF48B0247F34");

            // Attrib for BlockType: Attribute Values:Set Page Icon
            RockMigrationHelper.DeleteAttribute("DF437D92-FB5A-4625-851A-24F79412A337");

            // Attrib for BlockType: Attribute Values:Show Category Names as Separators
            RockMigrationHelper.DeleteAttribute("EF57237E-BA12-488A-9585-78466E4C3DB5");
        }
    }
}
