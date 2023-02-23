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
    public partial class Rollup_20230124 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            PageBlockAttributeMigrationsUp();
            UpdateConfirmationTemplates();
            UpdateTextToGiveReceiptSetting();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PageBlockAttributeMigrationsDown();
        }

        /// <summary>
        /// PA: v14.1 Add Enable Person Picker Attribute to the Communication Wizard Block
        /// </summary>
        private void PageBlockAttributeMigrationsUp()
        {
            // Attribute for BlockType: Communication Entry Wizard:Person Picker Visible
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Adding Individuals to Recipient Lists", "DisableAddingIndividualsToRecipientLists", "Disable Adding Individuals to Recipient Lists", @"When set to 'Yes' the person picker will be hidden so that additional individuals cannot be added to the recipient list.", 14, "False", "41A1906E-33D8-4C27-9025-C293C7B18EDC" );

        }

        /// <summary>
        /// PA: v14.1 Add Enable Person Picker Attribute to the Communication Wizard Block
        /// </summary>
        private void PageBlockAttributeMigrationsDown()
        {
            // Person Picker Visible Attribute for BlockType: Communication Entry Wizard
            RockMigrationHelper.DeleteAttribute( "41A1906E-33D8-4C27-9025-C293C7B18EDC" );
        }

        /// <summary>
        /// SK: v14.1 Alpha Bug Fix - Update Schedule Confirmation block Lava Template and Schedule Confirmation Email Template
        /// </summary>
        private void UpdateConfirmationTemplates()
        {
            // Update Decline Header Template
            var oldValue = @"{{ Group.Name }}<br>
{{ ScheduledItem.Location.Name }} {{ ScheduledItem.Schedule.Name }}<br></p>";
            oldValue = oldValue.Replace( "'", "''" );
            var newValue = @"{% capture attendanceIdList %}{% for ScheduledItem in ScheduledItems %}{{ ScheduledItem.Id }}{% unless forloop.last %},{% endunless %}{% endfor %}{% endcapture %}
{% assign lastDate = '' %}

<p>
{% for ScheduledItem in ScheduledItems %}
{% assign currentDate = ScheduledItem.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' %}
  {% if lastDate != currentDate %}
    <b>{{ currentDate }}</b><br />
    {% assign lastDate = currentDate %}
  {% endif %}

  {{ ScheduledItem.Occurrence.Group.Name }}<br />
  {{ ScheduledItem.Location.Name }} {{ScheduledItem.Occurrence.Schedule.Name }}<br /><br />
{% endfor %}
</p>";
            newValue = newValue.Replace( "'", "''" );
            var attributeId = System.Guid.Parse( "C1A4A35B-D5F3-497F-8951-FBF788B45DE9" ).ToString();
            UpdateAttributeValue( oldValue, newValue, attributeId );

            // Update Confirmation Header Template
            oldValue = @"{{ Group.Name }}<br>{{ ScheduledItem.Location.Name }} {{ScheduledItem.Schedule.Name }} <i class='text-success fa fa-check-circle'></i><br>";
            oldValue = oldValue.Replace( "'", "''" );
            newValue = @"{% capture attendanceIdList %}{% for ScheduledItem in ScheduledItems %}{{ ScheduledItem.Id }}{% unless forloop.last %},{% endunless %}{% endfor %}{% endcapture %}
{% assign lastDate = '' %}

{% for ScheduledItem in ScheduledItems %}
{% assign currentDate = ScheduledItem.Occurrence.OccurrenceDate | Date:'dddd, MMMM d, yyyy' %}
  {% if lastDate != currentDate %}
    <b>{{ currentDate }}</b><br />
    {% assign lastDate = currentDate %}
  {% endif %}

  {{ ScheduledItem.Occurrence.Group.Name }}<br />
  {{ ScheduledItem.Location.Name }} {{ScheduledItem.Occurrence.Schedule.Name }}
<i class='text-success fa fa-check-circle'></i><br /><br />
{% endfor %}";

            newValue = newValue.Replace( "'", "''" );
            attributeId = System.Guid.Parse( "7726CD7E-3ECF-4ECE-BAB3-407493CF120C" ).ToString();
            UpdateAttributeValue( oldValue, newValue, attributeId );

            // Update Decline Message Template
            oldValue = @"{{ ScheduledItem.Occurrence.Group.Name }}";
            oldValue = oldValue.Replace( "'", "''" );
            newValue = @"{% for ScheduledItem in ScheduledItems %}
  <br />{{ ScheduledItem.Occurrence.Group.Name }}
{% endfor %}";
            newValue = newValue.Replace( "'", "''" );
            attributeId = System.Guid.Parse( "DFE0AA2B-9067-4D25-8F26-627E1B8E2A3C" ).ToString();
            UpdateAttributeValue( oldValue, newValue, attributeId );

            // Update bug in SystemCommunication
            Sql( @"UPDATE [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE([Body], 'attendanceIds={{attendance.Id}}', 'attendanceIds={{attendanceIdList | UrlEncode}}')
                    WHERE [Guid] = 'F8E4CE07-68F5-4169-A865-ECE915CF421C'" );
        }

        /// <summary>
        /// Private method for calling the AttributeValue REPLACE SQL correctly for UpdateConfirmationTemplates()
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="attributeId"></param>
        private void UpdateAttributeValue( string oldValue, string newValue, string attributeId )
        {
            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Value" );

            Sql( $@"
                DECLARE @attributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '{attributeId}')

                UPDATE [dbo].[AttributeValue] 
                SET [Value] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                WHERE {targetColumn} NOT LIKE '%{newValue}%' AND [AttributeId] = @attributeId" );
        }

        private void UpdateTextToGiveReceiptSetting()
        {
            // Set "Receipt Email" Block Attribute Value for the Text To Give Utility Payment Entry block.
            RockMigrationHelper.AddBlockAttributeValue(
                "9684D991-8B26-4D39-BAD5-B520F91D27B8",   // Utility Payment Entry block instance for Text To Give.
                "0DCB60DD-14DD-4BB1-8AFA-7638858A7099",   // "Receipt Email" Attribute for "Utility Payment Entry" BlockType.
                "7DBF229E-7DEE-A684-4929-6C37312A0039" ); // "Giving Receipt" SystemCommunication.
        }
    }
}
