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
    public partial class Rollup_0706 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateEasyPieDocumentation();
            UpdateSparklinesDocumentation();
            ChartShortcodeDocumentation();
            CleanCurrencyCodeData();
            UpdateKPIShortcode();
            DailyChallengeEntryMobileBlock();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// GJ: Update Easy Pie Documentation
        /// </summary>
        private void UpdateEasyPieDocumentation()
        {
            Sql( MigrationSQL._202107061930320_Rollup_0706_update_easypie );
        }

        /// <summary>
        /// GJ: Update Sparklines Documentation
        /// </summary>
        private void UpdateSparklinesDocumentation()
        {
            Sql( MigrationSQL._202107061930320_Rollup_0706_update_sparklinesimg );
        }

        /// <summary>
        /// GJ: Chart Shortcode Documentation
        /// </summary>
        private void ChartShortcodeDocumentation()
        {
            Sql( MigrationSQL._202107061930320_Rollup_0706_chartupdate );
        }

        /// <summary>
        /// MB: Clean up Currency Code Data
        /// </summary>
        private void CleanCurrencyCodeData()
        {
            @Sql( @"UPDATE AttributeValue
                    SET [Value] = 'left'
                    FROM Attribute a
                    INNER JOIN AttributeValue av ON a.Id = av.AttributeId
                    WHERE a.[Guid] = '909B35DA-5B14-42FF-90E5-328033A07415'
		                    AND [Value] = 'let'

                    UPDATE AttributeValue
                    SET [Value] = RTRIM(LTRIM([Value]))
                    FROM Attribute a
                    INNER JOIN AttributeValue av ON a.Id = av.AttributeId
                    WHERE a.[Guid] = '909B35DA-5B14-42FF-90E5-328033A07415'
		                    AND ([Value] LIKE '% ' OR [Value] LIKE ' %')

                    UPDATE DefinedValue
                    SET [Description] = RTRIM(LTRIM(dv.[Description]))
                    FROM DefinedType dt
                    INNER JOIN DefinedValue dv ON dv.DefinedTypeId = dt.Id
                    WHERE dt.[Guid] = 'B9F3D359-4365-4594-BCEE-D23FA824FB81'
                    AND (dv.[Description] LIKE '% ' OR dv.[Description] LIKE ' %')

                    UPDATE DefinedValue
                    SET [Description] = 'Saint Helena Pound'
                    FROM DefinedType dt
                    INNER JOIN DefinedValue dv ON dv.DefinedTypeId = dt.Id
                    WHERE dt.[Guid] = 'B9F3D359-4365-4594-BCEE-D23FA824FB81'
                    AND dv.[Value] = 'SHP'" );
        }

        /// <summary>
        /// GJ: Update KPI Shortcode
        /// </summary>
        private void UpdateKPIShortcode()
        {
            Sql( MigrationSQL._202107061930320_Rollup_0706_KPIShortCode );
        }

        /// <summary>
        /// DH: Daily Challenge Entry Mobile Block
        /// </summary>
        private void DailyChallengeEntryMobileBlock()
        {
            string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";
            
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Cms.DailyChallengeEntry",
                "Daily Challenge Entry",
                "Rock.Blocks.Types.Mobile.Cms.DailyChallengeEntry, Rock, Version=1.12.4.1, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_CMS_DAILY_CHALLENGE_ENTRY );

            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM,
                "Challenge Progress",
                "Used for tracking completion of individual challenges (content channel items) in a day.",
                Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHALLENGE_PROGRESS );

            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM,
                "Challenges",
                "Used for tracking completion of an entire days worth of challenges.",
                Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CHALLENGES );

            RockMigrationHelper.AddOrUpdateTemplateBlock(
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_DAILY_CHALLENGE_ENTRY,
                "Mobile Daily Challenge Entry",
                string.Empty );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "08009450-92A5-4D4A-8E31-FCC1E4CBDF16",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_DAILY_CHALLENGE_ENTRY,
                "Default",
                @"{% assign MissedDatesSize = MissedDates | Size %}

{% if CompletedChallenge != null or Challenge.IsComplete == true %}
    {% assign challenge = Challenge %}
    {% if CompletedChallenge != null %}
        {% assign challenge = CompletedChallenge %}
    {% endif %}

    <StackLayout StyleClass=""challenge-view"">
        {% if challenge.HeaderContent != '' %}
            {{ challenge.HeaderContent }}
        {% endif %}
        
        {% for item in challenge.ChallengeItems %}
            <StackLayout StyleClass=""challenge-item"">
                <Label Text=""{{ item.Title | Escape }}""
                    VerticalTextAlignment=""Center""
                    VerticalOptions=""Center""
                    StyleClass=""challenge-title"" />

                {% if item.Content != '' %}
                    {{ item.Content }}
                {% endif %}
                
                {% if item.InputType == 'Text' %}
                    <Rock:FieldContainer StyleClass=""input-field"">
                        <Rock:TextBox StyleClass=""text-field"" IsReadOnly=""true"">
                            <Rock:TextBox.Text>
                                {{ challenge.ChallengeItemValues[item.Guid].Value | XamlWrap }}
                            </Rock:TextBox.Text>
                        </Rock:TextBox>
                    </Rock:FieldContainer>
                {% elsif item.InputType == 'Memo' %}
                    <Rock:FieldContainer StyleClass=""input-field"">
                        <Rock:TextEditor StyleClass=""memo-field"" IsReadOnly=""true"">
                            <Rock:TextEditor.Text>
                                {{ challenge.ChallengeItemValues[item.Guid].Value | XamlWrap }}
                            </Rock:TextEditor.Text>
                        </Rock:TextEditor>
                    </Rock:FieldContainer>
                {% endif %}
            </StackLayout>
            
            <Rock:Divider />
        {% endfor %}
    </StackLayout>
{% elsif MissedDatesSize > 0 %}
    <StackLayout StyleClass=""challenge-missed"">
        <Label Text=""Looks like you missed a day. Do you want to continue your previous challenge or start over?"" />
        
        <Button StyleClass=""btn,btn-primary"" Text=""Continue Challenge""
            Command=""{Binding Challenge.ShowChallengeForDate}""
            CommandParameter=""{{ MissedDates[0] }}"" />
            
        <Button StyleClass=""btn,btn-primary"" Text=""Start Over""
            Command=""{Binding Challenge.ShowCurrentChallenge}"" />
    </StackLayout>
{% else %}
    <StackLayout StyleClass=""challenge"">
        {% if Challenge.HeaderContent != '' %}
            {{ Challenge.HeaderContent }}
        {% endif %}
        
        {% for item in Challenge.ChallengeItems %}
            <StackLayout StyleClass=""challenge-item"">
                <StackLayout Orientation=""Horizontal"">
                    <Rock:Icon IconClass=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete, Converter={Rock:BooleanValueConverter True=check-circle, False=circle}}""
                        TextColor=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete, Converter={Rock:BooleanValueConverter True=#ee7725, False=#7c7c7c}}""
                        VerticalOptions=""Center""
                        Command=""{Binding Challenge.ToggleChallengeItem}""
                        CommandParameter=""{{ item.Guid }}"" />

                    <Label Text=""{{ item.Title | Escape }}""
                        VerticalTextAlignment=""Center""
                        VerticalOptions=""Center""
                        StyleClass=""challenge-title"">
                        <Label.GestureRecognizers>
                            <TapGestureRecognizer Command=""{Binding Challenge.ToggleChallengeItem}"" CommandParameter=""{{ item.Guid }}"" />
                        </Label.GestureRecognizers>
                    </Label>
                </StackLayout>
                
                {% if item.Content != '' %}
                    {{ item.Content }}
                {% endif %}
                
                {% if item.InputType == 'Text' %}
                    <Rock:FieldContainer StyleClass=""input-field"">
                        <Rock:TextBox Text=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].Value}""
                            StyleClass=""text-field""
                            IsReadOnly=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete}"" />
                    </Rock:FieldContainer>
                {% elsif item.InputType == 'Memo' %}
                    <Rock:FieldContainer StyleClass=""input-field"">
                        <Rock:TextEditor Text=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].Value}""
                            StyleClass=""memo-field""
                            IsReadOnly=""{Binding Challenge.ItemValues[{{ forloop.index0 }}].IsComplete}"" />
                    </Rock:FieldContainer>
                {% endif %}
            </StackLayout>
            
            <Rock:Divider />
        {% endfor %}
    </StackLayout>
{% endif %}
",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

        }
    }
}
