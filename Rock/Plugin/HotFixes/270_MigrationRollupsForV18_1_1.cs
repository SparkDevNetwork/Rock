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

using System;

using Rock.Model;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 270, "18.1" )]
    public class MigrationRollupsForV18_1_1 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateCommunicationPageBlockAttributeForPersonBioBlock();
            NA_ConvertBlockSettingsToNotUseObsidianComponentsUp();
            NA_FixBrokenCampusPickerLavaShortcodeUp();
            DH_FixMissingTablerIconInNextGenThemes();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            NA_ConvertBlockSettingsToNotUseObsidianComponentsDown();
        }

        #region KH: Update Communication Page Block Attribute for Person Bio Block

        private void UpdateCommunicationPageBlockAttributeForPersonBioBlock()
        {
            // Update the Communication Page Attribute Value if it is set to the old "Simple Communication" page.
            Sql( @"
UPDATE [dbo].[AttributeValue]
SET [Value] = '9F7AE226-CC95-4E6A-B333-C0294A2024BC,01A3891B-9998-7E30-20DC-58081A239D65',
    [IsPersistedValueDirty] = 1
WHERE [Value] = '7e8408b2-354c-4a5a-8707-36754ae80b9a'
    AND [AttributeId] = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '66CFDF24-8D19-4885-8C09-31DBE8C4126D' )
" );
        }

        #endregion

        #region [Beta v18.0.14] NA: v18.1 Migration to Set Data View Controls back to Webforms for DataViewDetail, ReportDetail, and DynamicReport block

        /// <summary>
        /// Change any existing UseObsidianComponents block setting value for the 
        /// Data View Detail, Report Detail and Dynamic Report blocks to False.
        /// </summary>
        public void NA_ConvertBlockSettingsToNotUseObsidianComponentsUp()
        {
            Sql( @"
UPDATE av
SET
    av.[Value] = 'False',
    av.[PersistedTextValue] = 'No',
    av.[PersistedCondensedTextValue] = 'N',
    av.[PersistedCondensedHtmlValue] = 'N',
    av.[IsPersistedValueDirty] = 1,
    av.ValueAsBoolean = 0
FROM dbo.[AttributeValue] AS av
INNER JOIN dbo.[Attribute] AS a
    ON a.[Id] = av.[AttributeId]
INNER JOIN dbo.[BlockType] AS bt
    ON bt.[Id] = TRY_CAST(a.[EntityTypeQualifierValue] AS INT)
WHERE
    a.[Key] = 'UseObsidianComponents'
    AND a.[EntityTypeId] = 9
    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND bt.[Guid] IN (
        'EB279DF9-D817-4905-B6AC-D9883F0DA2E4', -- Data View Detail
        'E431DBDF-5C65-45DC-ADC5-157A02045CCD', -- Report Detail
        'C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB'  -- Dynamic Report
    )
    AND av.[Value] = 'True';
" );
        }

        /// <summary>
        /// Change any existing UseObsidianComponents block setting value for the 
        /// Data View Detail, Report Detail and Dynamic Report blocks back to True.
        /// </summary>
        public void NA_ConvertBlockSettingsToNotUseObsidianComponentsDown()
        {
            Sql( @"
UPDATE av
SET
    av.[Value] = 'True',
    av.[PersistedTextValue] = 'Yes',
    av.[PersistedCondensedTextValue] = 'Y',
    av.[PersistedCondensedHtmlValue] = 'Y',
    av.[IsPersistedValueDirty] = 1,
    av.ValueAsBoolean = 1
FROM dbo.[AttributeValue] AS av
INNER JOIN dbo.[Attribute] AS a
    ON a.[Id] = av.[AttributeId]
INNER JOIN dbo.[BlockType] AS bt
    ON bt.[Id] = TRY_CAST(a.[EntityTypeQualifierValue] AS INT)
WHERE
    a.[Key] = 'UseObsidianComponents'
    AND a.[EntityTypeId] = 9
    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
    AND bt.[Guid] IN (
        'EB279DF9-D817-4905-B6AC-D9883F0DA2E4', -- Data View Detail
        'E431DBDF-5C65-45DC-ADC5-157A02045CCD', -- Report Detail
        'C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB'  -- Dynamic Report
    )
    AND av.[Value] = 'False';
" );
        }

        #endregion

        #region NA: Fix broken CampusPicker Lava Shortcode

        private const string HelixCategoryGuid = "5874AE45-B5EE-4D10-A274-26B5D69E6283";

        #region Campus Picker

        private const string CampusPickerTagName = "campuspicker";
        private const string CampusPickerGuid = "E787B188-2E0F-479E-A855-0E4ABA75C91B";
        private const string CampusPickerMarkup = @"//- Prep configuration settings
{% assign sc-statusList = campusstatuses | Split:',' %}
{% assign sc-typeList = campustypes | Split:',',true %}
{% assign sc-selectableList = selectablecampuses | Split:',' %}
{% assign sc-currentValues = value | Split:',' %} //- Note we're supporting the possibility that there could be multiple values.
{% assign includeinactive = includeinactive | AsBoolean %}
{% assign allowmultiple = allowmultiple | AsBoolean %}

//- Get source data
{% assign sc-campuses = 'All' | FromCache:'Campus'  %}

//- Filtering works by selecting all campuses that should not be shown
//- and then removing them from the list. This means that configuration
//- filters are AND not OR (which matches the C# logic).

//- Filter by type
{% if sc-typeList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusTypeId = campus.CampusTypeValueId | ToString %}
        {% assign isConfiguredType = sc-typeList | Contains:campusTypeId %}
        {% if isConfiguredType == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Filter by status
{% if sc-statusList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusStatusId = campus.CampusStatusValueId | ToString %}
        {% assign isConfiguredStatus = sc-statusList | Contains:campusStatusId %}
        {% if isConfiguredStatus == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Filter by selected
{% if sc-selectableList != empty %}

    {% for campus in sc-campuses reversed %}
        {% assign campusId = campus.Id | ToString %}
        {% assign isSelected = sc-selectableList | Contains:campusId %}
        {% if isSelected == false %}
            {% assign sc-campuses = sc-campuses | RemoveFromArray:campus %}
        {% endif %}
    {% endfor %}

{% endif %}

//- Remove inactive campuses
{% if includeinactive == false %}
    {% assign sc-campuses = sc-campuses | Where:'IsActive',true %}
{% endif %}

//- Ensure current values are still in the list, the value can be either a campus id or guid
{% assign allCampuses = 'All' | FromCache:'Campus'  %}
{% for currentValue in sc-currentValues %}
    {% for campus in allCampuses %}
        {% assign campusId = campus.Id | ToString %}
        {% assign campusGuid = campus.Guid | ToString %}
        {% if campusId == currentValue or campusGuid == currentValue %}
            //- Ensure the campus list has this campus, if not add it
            {% assign isInCampusList = sc-campuses | Contains:campus %}

            {% if isInCampusList == false %}
                {% assign sc-campuses = sc-campuses | AddToArray:campus %}
            {% endif %}
        {% endif %}
    {% endfor %}
{% endfor %}

//- Sort Campuses
{% assign sc-campuses = sc-campuses | OrderBy:'Order' %}

//- Control formatting
{% if allowmultiple %}
    {[ checkboxlist label:'{{ label }}' showlabel:'{{ showlabel }}' name:'{{ name }}' isrequired:'{{ isrequired }}' value:'{{ value }}' columns:'4' controltype:'campus-picker' id:'{{ id }}' validationmessage:'{{ validationmessage }}' additionalattributes:'{{ additionalattributes}}' ]}

        {% for campus in sc-campuses %}
            [[ item value:'{% if valuefield == 'id' %}{{ campus.Id }}{% else %}{{ campus.Guid }}{% endif %}' text:'{{ campus.Name }}' ]][[ enditem]]
        {% endfor %}

    {[ endcheckboxlist ]}
{% else %}
    {[ dropdown label:'{{ label }}' showlabel:'{{ showlabel }}' name:'{{ name }}' longlistenabled:'{{ longlistenabled }}' value:'{{ value }}' controltype:'campus-picker' isrequired:'{{ isrequired }}' id:'{{ id }}' validationmessage:'{{ validationmessage }}' additionalattributes:'{{ additionalattributes}}' ]}

        {% for campus in sc-campuses %}
            [[ item value:'{% if valuefield == 'id' %}{{ campus.Id }}{% else %}{{ campus.Guid }}{% endif %}' text:'{{ campus.Name }}' ]][[ enditem]]
        {% endfor %}

    {[ enddropdown ]}
{% endif %}";
        private const string CampusPickerParameters = @"value^|includeinactive^false|campustypes^|campusstatuses^|selectablecampuses^|name^campus|isrequired^false|label^Campus|validationmessage^Please select a campus.|longlistenabled^false|valuefield^id|allowmultiple^false|additionalattributes^|showlabel^true";
        private const string CampusPickerDocumentation = @"<p>This control allows you to select a campus.</p>

<h5>Example Usage</h5>
<pre>{[ campuspicker label:'Primary Campus' value:'1,2' allowmultiple:'true' campustypes:'768' campusstatuses:'765' selectablecampuses:'1,2,5' ]}</pre>

<h5>Parameters</h5>
<p>Below are the parameters for the campus picker shortcode.</p>
<ul>
    <li><strong>label</strong> - The label to display above the control.</li><li><b>showlabel </b>(true) - Whether to display label.</li>
    <li><strong>name</strong> (campus) - The name for the campus picker control.</li>
    <li><strong>value</strong> - The ID or Guid of the currently selected campus(es).</li>
    <li><strong>valuefield</strong> (id) - Specifies whether the picker's value should correspond to the campus' <code>id</code> or <code>guid</code>.</li>
    <li><strong>includeinactive</strong> (false) - Determines if inactive campuses should be displayed.</li>
    <li><strong>campustypes</strong> - Filters the campus list by type (comma separated list of defined value ids).</li>
    <li><strong>campusstatuses</strong> - Filters the campus list by status (comma separated list of defined value ids).</li>
    <li><strong>selectablecampuses</strong> -  List of specific campuses to display (comma separated list of campus ids).</li>
    <li><strong>longlistenabled</strong> (false) -  Enhances the functionality to include a search feature, facilitating swift and efficient selection of the preferred item from the list.</li>
    <li><strong>allowmultiple</strong> (false) -  Determines if the selection of multiple values is allowed.</li><li><b>isrequired </b>(false) - Establishes whether making a selection is necessary.
</li><li><b>validationmessage </b>(Please provide a campus.) - Message to display when the value is not valid.</li>
    <li><strong>additionalattributes</strong> -  Additional attributes to include on the input control.</li>
</ul>

<p>
    The above settings enable a wide range of filtering options for the list. Regardless of the filter configurations, the
    current value will consistently be shown.
</p>";
        private const string CampusPickerDescription = "Displays a campus picker.";
        private const int CampusPickerTagType = ( int ) TagType.Inline;

        #endregion

        /// <summary>
        /// Change the typo in the CampusPicker Lava Shortcode to fix issue #6574.
        /// </summary>
        public void NA_FixBrokenCampusPickerLavaShortcodeUp()
        {
            RockMigrationHelper.AddOrUpdateLavaShortcode( "Campus Picker", CampusPickerTagName, CampusPickerDescription, CampusPickerDocumentation, CampusPickerMarkup, CampusPickerParameters, CampusPickerTagType, HelixCategoryGuid, CampusPickerGuid );
        }

        #endregion

        #region DH: Fix missing Tabler icon in next-gen themes.

        private void DH_FixMissingTablerIconInNextGenThemes()
        {
            Sql( @"UPDATE [Theme]
SET [AdditionalSettingsJson] = JSON_MODIFY([AdditionalSettingsJson], '$.ThemeCustomizationSettings.EnabledIconSets', 2 | JSON_VALUE([AdditionalSettingsJson], 'lax $.ThemeCustomizationSettings.EnabledIconSets'))
WHERE ISJSON([AdditionalSettingsJson]) = 1
  AND JSON_VALUE([AdditionalSettingsJson], 'lax $.ThemeCustomizationSettings.EnabledIconSets') IS NOT NULL
  AND JSON_VALUE([AdditionalSettingsJson], 'lax $.ThemeCustomizationSettings.EnabledIconSets') & 2 = 0" );
        }

        #endregion
    }
}