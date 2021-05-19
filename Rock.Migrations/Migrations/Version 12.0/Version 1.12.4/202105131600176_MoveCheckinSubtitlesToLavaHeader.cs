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
    public partial class MoveCheckinSubtitlesToLavaHeader : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteOldBlockTitleAttributes();
            UpdateGroupSelectHeaderTemplate();
            CreateGroupTypeSelectHeaderTemplate();
            CreateLocationSelectHeaderTemplate();
            CreateTimeSelectHeaderTemplate();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // put back the old block attributes
            Sql( @"
                SET IDENTITY_INSERT [Attribute] ON

                INSERT [dbo].[Attribute] ([Id], [IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [ModifiedDateTime], [AllowSearch], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory], [IsActive], [EnableHistory], [ShowOnBulk], [IsPublic])
                VALUES
                    (2726, 1, 1, 9, 'BlockTypeId', '114', 'SubTitle', 'Subtitle', 'Subtitle to display. Use {0} for selected group name.', 8, 0, '{0}', 0, 0, 'A85DEF5A-D6CE-41FE-891E-36880DE5CD9C', 'May  7 2021  9:49AM', 0, 0, 0, 0, 1, 0, 0, 0),
                    (2731, 1, 1, 9, 'BlockTypeId', '115', 'SubTitle', 'Sub Title', 'Sub-Title to display. Use {0} for selected group type name.', 8, 0, '{0}', 0, 0, '342CE8B6-7CB1-4D3D-BE1A-3F55CBC3F376', 'May  7 2021  9:49AM', 0, 0, 0, 0, 1, 0, 0, 0),
                    (2733, 1, 1, 9, 'BlockTypeId', '116', 'SubTitle', 'Sub Title', 'Sub-Title to display. Use {0} for selected group/location name.', 5, 0, '{0}', 0, 0, '2BE3BCC3-7307-4DA3-BF4A-4B8E4A5C9837', 'May  7 2021  9:49AM', 0, 0, 0, 0, 1, 0, 0, 0)

                SET IDENTITY_INSERT [Attribute] OFF" );
        }

        private void DeleteOldBlockTitleAttributes()
        {
            Sql( @"
                DELETE FROM [Attribute] WHERE [Id] IN (
                SELECT a.[Id]
                FROM [Attribute] a
                JOIN [BlockType] bt on a.[EntityTypeQualifierValue] = bt.[Id]
                WHERE a.[EntityTypeQualifierColumn] = 'BlockTypeId'
	                AND a.[Key] = 'Subtitle'
	                AND a.[EntityTypeId] = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')
	                AND  bt.[Path] IN (
		                '~/Blocks/CheckIn/GroupSelect.ascx',
		                '~/Blocks/CheckIn/LocationSelect.ascx',
		                '~/Blocks/CheckIn/TimeSelect.ascx')
                )" );
        }

        private void UpdateGroupSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Group Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Group Select Header Template",
                "Group Select Header Template",
                @"Lava template to use for the 'Group Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ Individual }} which is a Person object and is the current selected person.
{{ SelectedArea }} is a GroupType object and corresponds to the selected check-in Area listed in Areas and Groups.
{{ SelectedSchedule}} is a Schedule object and is the current selected schedule.",
                1061,
                @"{{ Individual.FullName }}
{% if SelectedArea != null and SelectedSchedule != null %}
   <div class=""checkin-sub-title"">{{ SelectedArea.Name }} @ {{ SelectedSchedule.Name }}</div>
{% elseif SelectedSchedule != null %}
    <div class=""checkin-sub-title"">{{ SelectedSchedule.Name }}</div>
{% elseif SelectedArea != null %}
    <div class=""checkin-sub-title"">{{ SelectedArea.Name }}</div>
{% endif %}",
                "81236834-ED01-4377-95C7-923EA4A6B803",
                "core_checkin_GroupSelectHeaderLavaTemplate");

        }

        private void CreateGroupTypeSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Group Type Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Group Type Select Header Template",
                "Group Type Select Header Template",
                @"Lava template to use for the 'Group Type Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ Individual }} which is a Person object and is the current selected person.
{{ SelectedSchedule}} is a Schedule object and is the current selected schedule.",
                1062,
                @"{{ Individual.FullName }}
{% if SelectedSchedule != null %}
    <div class=""checkin-sub-title"">{{ SelectedSchedule.Name }}</div>
{% endif %}",
                "DE20567B-BB5D-4E12-8B83-6ADCB92FB4CA",
                "core_checkin_GroupTypeSelectHeaderLavaTemplate");
        }

        private void CreateLocationSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Location Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Location Select Header Template",
                "Location Select Header Template",
                @"Lava template to use for the 'Location Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ Individual }} which is a Person object and is the current selected person.
{{ SelectedGroup }} is a Group object and corresponds to the selected check-in group listed in Areas and Groups.
{{ SelectedSchedule}} is a Schedule object and is the current selected schedule.",
                1063,
                @"{{ Individual.FullName }}
{% if SelectedGroup != null and SelectedSchedule != null %}
   <div class=""checkin-sub-title"">{{ SelectedGroup.Name }} @ {{ SelectedSchedule.Name }}</div>
{% elseif SelectedSchedule != null %}
    <div class=""checkin-sub-title"">{{ SelectedSchedule.Name }}</div>
{% elseif SelectedGroup != null %}
    <div class=""checkin-sub-title"">{{ SelectedGroup.Name }}</div>
{% endif %}",
                "44A8BBF1-354D-4581-B6E8-189FEEBFF45F",
                "core_checkin_LocationSelectHeaderLavaTemplate");
        }

        private void CreateTimeSelectHeaderTemplate()
        {
            // Entity: Rock.Model.GroupType Attribute: Time Select Header Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5",
                "GroupTypePurposeValueId",
                "142",
                "Time Select Header Template",
                "Time Select Header Template",
                @"Lava template to use for the 'Time Select' check-in block header. The available merge fields are:
{{ Family }} which is a Group object and is the current family.
{{ SelectedIndividuals }} is a list of Person objects which contains all of the currently selected persons.
{{ CheckinType }} is the type of check-in given as a string which will be either 'Family' or 'Individual'.
{{ SelectedGroup }} is a Group object and corresponds to the selected check-in group listed in Areas and Groups. This only applies for individual checkin types.
{{ SelectedLocation }} is a Location and corresponds to the selected location for the group. This only applies for individual checkin types.",
                1066,
                @"{% if CheckinType == 'Family' %}
    {{ Family.Name }}
{% else %}
    {% assign selectedIndividual = SelectedIndividuals | First %}
    {{ selectedIndividual.FullName }}
{% endif %}
{% if SelectedGroup != null %}
    <div class=""checkin-sub-title"">{{ SelectedGroup.Name }} - {{ SelectedLocation.Name }}</div>
{% endif %}",
                "90D9CAEA-843B-4D32-84CF-E25A3258087F",
                "core_checkin_TimeSelectHeaderLavaTemplate" );
        }
    }
}
