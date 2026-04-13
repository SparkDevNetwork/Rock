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
    /// <summary>
    ///
    /// </summary>
    public partial class AddShortcodeScopeBehaviorToLavaShortCode : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddShortcodeScopeBehaviorColumnUp();
            JPH_UpdateLavaShortcodeDescriptionsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddShortcodeScopeBehaviorColumnDown();
            JPH_UpdateLavaShortcodeDescriptionsDown();
        }

        /// <summary>
        /// JPH - Add ShortcodeScopeBehavior column - up.
        /// </summary>
        private void JPH_AddShortcodeScopeBehaviorColumnUp()
        {
            AddColumn( "dbo.LavaShortcode", "ShortcodeScopeBehavior", c => c.Int( nullable: false ) );
        }

        /// <summary>
        /// JPH - Add ShortcodeScopeBehavior column - down.
        /// </summary>
        private void JPH_AddShortcodeScopeBehaviorColumnDown()
        {
            DropColumn( "dbo.LavaShortcode", "ShortcodeScopeBehavior" );
        }

        /// <summary>
        /// JPH - Update LavaShortcode descriptions - up.
        /// </summary>
        private void JPH_UpdateLavaShortcodeDescriptionsUp()
        {
            Sql( @"
-- Accordion
UPDATE [LavaShortcode]
SET [Description] = 'Create a collapsible Bootstrap accordion to organize expandable content.'
WHERE [Guid] = '18F87671-A848-4509-8058-C95682E7BAD4';

-- Campus Picker
UPDATE [LavaShortcode]
SET [Description] = 'Display a dropdown list of campuses for quick selection.'
WHERE [Guid] = 'E787B188-2E0F-479E-A855-0E4ABA75C91B';

-- Chart
UPDATE [LavaShortcode]
SET [Description] = 'Easily create dynamic charts from Lava data without writing complex JavaScript.'
WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA';

-- Checkbox List
UPDATE [LavaShortcode]
SET [Description] = 'Display a list of checkboxes for multiple selections.'
WHERE [Guid] = 'D052824F-E514-47D9-953E-2C9B55FF72D0';

-- Currency
UPDATE [LavaShortcode]
SET [Description] = 'Add a currency input field with automatic number formatting.'
WHERE [Guid] = '1307452F-446E-485B-A949-7CDD6DF75931';

-- Date Picker
UPDATE [LavaShortcode]
SET [Description] = 'Provide an interactive calendar for selecting a single date.'
WHERE [Guid] = '43B4AC7D-A8DE-4F99-9812-84C055D34799';

-- Date Range Picker
UPDATE [LavaShortcode]
SET [Description] = 'Let users select a start and end date using a single control.'
WHERE [Guid] = '5ABBC6EA-8AD4-4FD4-AF9A-0D782A32C956';

-- Defined Value Picker
UPDATE [LavaShortcode]
SET [Description] = 'Display a dropdown populated with defined values from Rock.'
WHERE [Guid] = 'E2FC377F-EDCE-4FD3-B734-D06939E65210';

-- Dropdown
UPDATE [LavaShortcode]
SET [Description] = 'Create a standard dropdown menu for single-option selection.'
WHERE [Guid] = '7BA8EA13-BC09-4075-B0AF-F88AB1203953';

-- Easy Pie Chart
UPDATE [LavaShortcode]
SET [Description] = 'Display lightweight, animated, retina-optimized pie charts.'
WHERE [Guid] = '96A8284E-96A6-4E38-969C-640F0BDC8EB8';

-- Follow Icon
UPDATE [LavaShortcode]
SET [Description] = 'Add a clickable icon that allows users to follow any entity.'
WHERE [Guid] = '1E6785C0-7D92-49A7-9E15-68E113399152';

-- Google Heatmap (Deprecated)
UPDATE [LavaShortcode]
SET [Description] = 'Visualize data intensity across locations using an interactive Google Heatmap.'
WHERE [Guid] = '9969A52B-01F8-4597-8C5A-1842BFA1E482';

-- Google Map
UPDATE [LavaShortcode]
SET [Description] = 'Display an interactive Google Map in your page using Lava.'
WHERE [Guid] = 'FE298210-1307-49DF-B28B-3735A414CCA0';

-- Google Static Map
UPDATE [LavaShortcode]
SET [Description] = 'Display a static Google Map image without needing complex configuration.'
WHERE [Guid] = '2DD53FE6-6EB2-4EC8-A965-3F71054F7983';

-- KPI
UPDATE [LavaShortcode]
SET [Description] = 'Display visual key performance indicators for quick insights.'
WHERE [Guid] = '8A49FD01-D59E-4611-8FF4-9E226C99FB22';

-- Memo
UPDATE [LavaShortcode]
SET [Description] = 'Display a multi-line text area for longer input or notes.'
WHERE [Guid] = '94E4E2D0-531C-430D-9353-3A230177D4DD';

-- Panel
UPDATE [LavaShortcode]
SET [Description] = 'Wrap content in a clean, Bootstrap-styled panel layout.'
WHERE [Guid] = 'ADB1F75D-500D-4305-9805-99AF04A2CD88';

-- Parallax
UPDATE [LavaShortcode]
SET [Description] = 'Add a scrolling background image to create a parallax effect.'
WHERE [Guid] = '4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4';

-- Radio Button List
UPDATE [LavaShortcode]
SET [Description] = 'Display a list of radio buttons for single-choice selection.'
WHERE [Guid] = 'B1EA9AC3-DC51-444C-953A-565AD38A31DD';

-- Range Slider
UPDATE [LavaShortcode]
SET [Description] = 'Include an adjustable slider control to select numeric values or ranges.'
WHERE [Guid] = '2D6B04C4-A54A-4F3F-98E4-657F8AC882D9';

-- Rock Control
UPDATE [LavaShortcode]
SET [Description] = 'Base control shortcode used for Helix-based UI components.'
WHERE [Guid] = '4A671223-7165-4710-9FA3-904AC77300F8';

-- Sparkline Chart
UPDATE [LavaShortcode]
SET [Description] = 'Render small inline charts to display compact data trends.'
WHERE [Guid] = 'E7AC1E9B-0200-49AF-967F-0A9D2DD0F968';

-- Text Box
UPDATE [LavaShortcode]
SET [Description] = 'Display a single-line text input field.'
WHERE [Guid] = 'BE829889-A775-4170-9A88-591BB82C86DD';

-- Trend Chart
UPDATE [LavaShortcode]
SET [Description] = 'Create simple CSS-based bar charts to show visual trends.'
WHERE [Guid] = '52B27805-7C36-4965-90BD-3AA42D11F2DB';

-- Vimeo
UPDATE [LavaShortcode]
SET [Description] = 'Embed a responsive Vimeo video using only the video ID.'
WHERE [Guid] = 'EA1335B7-158F-464F-8994-98C53D4E47FF';

-- Word Cloud
UPDATE [LavaShortcode]
SET [Description] = 'Generate a visual word cloud highlighting the most frequent terms from a text block.'
WHERE [Guid] = 'CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4';

-- YouTube
UPDATE [LavaShortcode]
SET [Description] = 'Embed a responsive YouTube video using only the video ID.'
WHERE [Guid] = '2FA4D446-3F63-4DFD-8C6A-55DBA76AEB83';" );
        }

        /// <summary>
        /// JPH - Update LavaShortcode descriptions - down.
        /// </summary>
        private void JPH_UpdateLavaShortcodeDescriptionsDown()
        {
            Sql( @"
-- Accordion
UPDATE [LavaShortcode]
SET [Description] = 'Allows you to easily create a Bootstrap accordion control.'
WHERE [Guid] = '18F87671-A848-4509-8058-C95682E7BAD4';

-- Campus Picker
UPDATE [LavaShortcode]
SET [Description] = 'Displays a campus picker.'
WHERE [Guid] = 'E787B188-2E0F-479E-A855-0E4ABA75C91B';

-- Chart
UPDATE [LavaShortcode]
SET [Description] = 'Adding dynamic charts to a page can be difficult, even for an experienced Javascript developer. The chart shortcode allows anyone to create charts with just a few lines of Lava.'
WHERE [Guid] = '43819A34-4819-4507-8FEA-2E406B5474EA';

-- Checkbox List
UPDATE [LavaShortcode]
SET [Description] = 'Displays a checkbox list.'
WHERE [Guid] = 'D052824F-E514-47D9-953E-2C9B55FF72D0';

-- Currency
UPDATE [LavaShortcode]
SET [Description] = 'Displays a currency control.'
WHERE [Guid] = '1307452F-446E-485B-A949-7CDD6DF75931';

-- Date Picker
UPDATE [LavaShortcode]
SET [Description] = 'Displays a date picker.'
WHERE [Guid] = '43B4AC7D-A8DE-4F99-9812-84C055D34799';

-- Date Range Picker
UPDATE [LavaShortcode]
SET [Description] = 'Display a Date Range picker.'
WHERE [Guid] = '5ABBC6EA-8AD4-4FD4-AF9A-0D782A32C956';

-- Defined Value Picker
UPDATE [LavaShortcode]
SET [Description] = 'Displays a defined value picker control.'
WHERE [Guid] = 'E2FC377F-EDCE-4FD3-B734-D06939E65210';

-- Dropdown
UPDATE [LavaShortcode]
SET [Description] = 'Display a dropdown list control.'
WHERE [Guid] = '7BA8EA13-BC09-4075-B0AF-F88AB1203953';

-- Easy Pie Chart
UPDATE [LavaShortcode]
SET [Description] = 'Lightweight plugin to render simple, animated and retina optimized pie charts.'
WHERE [Guid] = '96A8284E-96A6-4E38-969C-640F0BDC8EB8';

-- Follow Icon
UPDATE [LavaShortcode]
SET [Description] = 'Add an icon with the ability to follow any entity with a click.'
WHERE [Guid] = '1E6785C0-7D92-49A7-9E15-68E113399152';

-- Google Heatmap (Deprecated)
UPDATE [LavaShortcode]
SET [Description] = 'Add an interactive visualization to depict the intensity of data at geographical points.'
WHERE [Guid] = '9969A52B-01F8-4597-8C5A-1842BFA1E482';

-- Google Map
UPDATE [LavaShortcode]
SET [Description] = 'Add interactive maps to your site with just a bit of Lava.'
WHERE [Guid] = 'FE298210-1307-49DF-B28B-3735A414CCA0';

-- Google Static Map
UPDATE [LavaShortcode]
SET [Description] = 'Easily allow you to add Google static maps without having to remember complex settings.'
WHERE [Guid] = '2DD53FE6-6EB2-4EC8-A965-3F71054F7983';

-- KPI
UPDATE [LavaShortcode]
SET [Description] = 'Create quick key performance indicators.'
WHERE [Guid] = '8A49FD01-D59E-4611-8FF4-9E226C99FB22';

-- Memo
UPDATE [LavaShortcode]
SET [Description] = 'Displays a memo.'
WHERE [Guid] = '94E4E2D0-531C-430D-9353-3A230177D4DD';

-- Panel
UPDATE [LavaShortcode]
SET [Description] = 'The panel shortcode allows you to easily add a Bootstrap panel to your markup.'
WHERE [Guid] = 'ADB1F75D-500D-4305-9805-99AF04A2CD88';

-- Parallax
UPDATE [LavaShortcode]
SET [Description] = 'Add a scrolling background to a section of your page.'
WHERE [Guid] = '4B6452EF-6FEA-4A66-9FB9-1A7CCE82E7A4';

-- Radio Button List
UPDATE [LavaShortcode]
SET [Description] = 'Displays a radio button list control.'
WHERE [Guid] = 'B1EA9AC3-DC51-444C-953A-565AD38A31DD';

-- Range Slider
UPDATE [LavaShortcode]
SET [Description] = 'Displays a range slider control.'
WHERE [Guid] = '2D6B04C4-A54A-4F3F-98E4-657F8AC882D9';

-- Rock Control
UPDATE [LavaShortcode]
SET [Description] = 'Base control shortcode for Helix controls.'
WHERE [Guid] = '4A671223-7165-4710-9FA3-904AC77300F8';

-- Sparkline Chart
UPDATE [LavaShortcode]
SET [Description] = 'Generate small inline charts with just a single line of Lava.'
WHERE [Guid] = 'E7AC1E9B-0200-49AF-967F-0A9D2DD0F968';

-- Text Box
UPDATE [LavaShortcode]
SET [Description] = 'Displays a text box control.'
WHERE [Guid] = 'BE829889-A775-4170-9A88-591BB82C86DD';

-- Trend Chart
UPDATE [LavaShortcode]
SET [Description] = 'Generate simple CSS based bar charts.'
WHERE [Guid] = '52B27805-7C36-4965-90BD-3AA42D11F2DB';

-- Vimeo
UPDATE [LavaShortcode]
SET [Description] = 'Creates a responsive Vimeo embed from just a simple video id.'
WHERE [Guid] = 'EA1335B7-158F-464F-8994-98C53D4E47FF';

-- Word Cloud
UPDATE [LavaShortcode]
SET [Description] = 'This shortcode takes a large amount of text and creates a word cloud of the most popular terms.'
WHERE [Guid] = 'CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4';

-- YouTube
UPDATE [LavaShortcode]
SET [Description] = 'Creates a responsive YouTube embed from just a simple video id.'
WHERE [Guid] = '2FA4D446-3F63-4DFD-8C6A-55DBA76AEB83';" );
        }
    }
}
