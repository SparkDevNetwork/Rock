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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber(234, "1.16.8")]
    public class MigrationRollupsForV16_10_1 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MobileScheduledTransactionListTemplateUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            MobileScheduledTransactionListTemplateDown();
        }

        #region PS: Update the Scheduled Transaction List Lava Template.

        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";
        private void MobileScheduledTransactionListTemplateUp()
        {
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "AE0A060A-EDC6-43B2-86B9-5FAA4C148CF0",
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_FINANCE_SCHEDULED_TRANSACTION_LIST,
                "Default",
                @"<Rock:StyledBorder StyleClass=""border, border-interface-soft, rounded, bg-interface-softest, p-16"">
    <Grid ColumnDefinitions=""Auto, Auto, *, Auto"" 
        RowDefinitions=""Auto, Auto, Auto""
        StyleClass=""gap-column-8"" >
        <Label StyleClass=""body, bold, text-interface-strongest, mb-8"" 
            Grid.Row=""0""
            Grid.Column=""0""
            Text=""${{ ScheduledTransactionInfo.TotalAmount }}"" />
            
        {% if ScheduledTransactionInfo.IsActive == false %}
            <Label StyleClass=""footnote, pt-2""
                Text=""Inactive""
                Grid.Column=""1""
                TextColor=""{Rock:PaletteColor app-warning-strong}"" />                
        {% endif %}
        
        {% if ScheduledTransactionInfo.NextPaymentDate and ScheduledTransactionInfo.NextPaymentDate != null %}
            <Label StyleClass=""footnote""  
                Grid.Row=""1""
                Grid.ColumnSpan=""3""
                Text=""Next Gift: {{ ScheduledTransactionInfo.NextPaymentDate | Date:'MMM dd, yyyy' }}"" />
        {% endif %}
        
        <Label StyleClass=""footnote"" 
            Grid.Row=""2""
            Text=""{{ FrequencyText }}"" />
        
        <Rock:Icon 
            Grid.Column=""3""
            Grid.RowSpan=""3""
            VerticalOptions=""Center""
            StyleClass=""footnote""
            IconFamily=""FontAwesomeSolid""
            IconClass=""chevron-right"" />
    </Grid>
    
    <Rock:StyledBorder.Behaviors>
        <Rock:TouchBehavior 
            PressedOpacity=""0.6"" 
            DefaultOpacity=""1"" 
            HoveredOpacity=""0.6"" 
            Command=""{Binding ShowCoverSheet}"" 
            CommandParameter=""{{ DetailPage }}?ScheduledTransaction={{ ScheduledTransactionInfo.IdKey }}"" />
    </Rock:StyledBorder.Behaviors>
</Rock:StyledBorder>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }

        private void MobileScheduledTransactionListTemplateDown()
        {
            RockMigrationHelper.DeleteTemplateBlockTemplate( "AE0A060A-EDC6-43B2-86B9-5FAA4C148CF0" );
        }

        #endregion
    }
}