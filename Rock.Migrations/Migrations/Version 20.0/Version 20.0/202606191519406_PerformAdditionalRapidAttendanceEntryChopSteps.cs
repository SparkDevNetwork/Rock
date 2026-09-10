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
    public partial class PerformAdditionalRapidAttendanceEntryChopSteps : Rock.Migrations.RockMigration
    {
        private const string RapidAttendanceEntryPageGuid = "78b79290-3234-4d8c-96d3-1901901ba1dd";
        private const string FullWorksurfaceLayoutGuid = "C2467799-BB45-4251-8EE6-F0BF27201535";
        private const string RapidAttendanceEntryBlockTypeGuid = "6C2ED1FA-218B-4ACC-B661-A2618F310CD4";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_PerformAdditionalRapidAttendanceEntryChopSteps_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_PerformAdditionalRapidAttendanceEntryChopSteps_Down();
        }

        /// <summary>
        /// JPH: Performs the additional steps to properly chop the Rapid Attendance Entry block - up.
        /// </summary>
        private void JPH_PerformAdditionalRapidAttendanceEntryChopSteps_Up()
        {
            RockMigrationHelper.UpdatePageLayout( RapidAttendanceEntryPageGuid, FullWorksurfaceLayoutGuid );

            // ----------------------------------

            var newFamilyHeaderLava = @"
<h4 class='margin-t-none'>{{ Family.Name }}</h4>
{% if Family.GroupLocations != null -%}
    {% assign groupLocations = Family.GroupLocations -%}
    {% assign locationCount = groupLocations | Size -%}
    {% if locationCount > 0 -%}
        {% for groupLocation in groupLocations -%}
            {% if groupLocation.GroupLocationTypeValue.Value == 'Home' and groupLocation.Location.FormattedHtmlAddress != null and groupLocation.Location.FormattedHtmlAddress != '' -%}
                <div class='rapid-attendance-entry-home-address'>{{ groupLocation.Location.FormattedHtmlAddress }}</div>
            {%- endif %}
        {%- endfor %}
    {%- endif %}
{%- endif %}";

            var newIndividualHeaderLava = @"
<div class='row'>
    <div class='col-md-6 rapid-attendance-entry-person-details'>
        <div class='d-flex align-items-center margin-b-sm'>
            <h5 class='margin-t-none margin-b-none'>{{ Person.FullName }}</h5>
            {% if Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == 'Inactive' -%}
                <span class='label label-danger margin-l-sm' title='{{ Person.RecordStatusReasonValue.Value }}' data-toggle='tooltip'>{{ Person.RecordStatusValue.Value }}</span>
            {%- elseif Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == 'Pending' -%}
                <span class='label label-warning margin-l-sm' title='{{ Person.RecordStatusReasonValue.Value }}' data-toggle='tooltip'>{{ Person.RecordStatusValue.Value }}</span>
            {%- endif %}
        </div>
        {% if Person.Age != null and Person.Age != '' -%}
            {{ Person.Age }} yrs old ({{ Person.BirthDate | Date:'sd' }})<br>
        {%- endif -%}
        {% if Person.Email != '' -%}
            <a href='mailto:{{ Person.Email }}'>{{ Person.Email }}</a>
        {%- endif -%}
    </div>
    <div class='col-md-6 rapid-attendance-entry-phone-numbers'>
        {% for phone in Person.PhoneNumbers -%}
            {% if phone.IsUnlisted != true -%}
                <a href='tel:{{ phone.NumberFormatted }}'>{{ phone.NumberFormatted }}</a>
            {%- else -%}
                Unlisted
            {%- endif %}
            <small>({{ phone.NumberTypeValue.Value }})</small><br>
        {%- endfor %}
    </div>
</div>";

            ReplaceUncustomizedLavaTemplates( RapidAttendanceEntryBlockTypeGuid, "FamilyHeaderLavaTemplate", newFamilyHeaderLava );
            ReplaceUncustomizedLavaTemplates( RapidAttendanceEntryBlockTypeGuid, "IndividualHeaderLavaTemplate", newIndividualHeaderLava );
        }

        /// <summary>
        /// JPH: Performs the additional steps to properly chop the Rapid Attendance Entry block - down.
        /// </summary>
        private void JPH_PerformAdditionalRapidAttendanceEntryChopSteps_Down()
        {
            // Leave the new Lava in place on downgrade since the old Lava was not well-formatted and added unwanted vertical spacing.

            // ----------------------------------

            RockMigrationHelper.UpdatePageLayout( RapidAttendanceEntryPageGuid, Rock.SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE );
        }

        /// <summary>
        /// Replaces the saved Lava for the given block-type attribute with the new template on every block whose value
        /// still equals the attribute's current default, leaving customized templates untouched. Whitespace is stripped
        /// from both sides so the comparison ignores formatting differences (indentation, line endings, edge whitespace)
        /// introduced by earlier saves.
        /// </summary>
        private void ReplaceUncustomizedLavaTemplates( string blockTypeGuid, string attributeKey, string newLava )
        {
            var valueColumn = StripWhitespace( "Value" );
            var defaultValueColumn = StripWhitespace( "DefaultValue" );
            var escapedNewLava = newLava.Replace( "'", "''" );

            // Plugin migrations run BEFORE `[Global.asax.cs]StartBlockTypeCompilationThread()` so this comparison is
            // still against the original default Lava as defined in C#, not the rewritten default that gets updated in
            // the database at application startup.
            Sql( $@"
DECLARE @BlockEntityTypeId [INT] = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}');
DECLARE @BlockTypeId [INT] = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '{blockTypeGuid}');
DECLARE @AttributeId [INT] = (
    SELECT TOP 1 [Id]
    FROM [Attribute]
    WHERE [Key] = '{attributeKey}'
        AND [EntityTypeId] = @BlockEntityTypeId
        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
        AND [EntityTypeQualifierValue] = @BlockTypeId
);

UPDATE [AttributeValue]
SET [Value] = '{escapedNewLava}'
    , [PersistedTextValue] = NULL
    , [PersistedHtmlValue] = NULL
    , [PersistedCondensedTextValue] = NULL
    , [PersistedCondensedHtmlValue] = NULL
    , [IsPersistedValueDirty] = 1
WHERE [AttributeId] = @AttributeId
    AND {valueColumn} = (
        SELECT {defaultValueColumn}
        FROM [Attribute]
        WHERE [Id] = @AttributeId
    );" );
        }

        /// <summary>
        /// Wraps a column in REPLACE calls that remove every whitespace character (tab, line feed, carriage return,
        /// and space), so a comparison ignores all formatting differences.
        /// </summary>
        private static string StripWhitespace( string columnName )
        {
            return $"REPLACE(REPLACE(REPLACE(REPLACE([{columnName}], CHAR(9), ''), CHAR(10), ''), CHAR(13), ''), ' ', '')";
        }
    }
}
