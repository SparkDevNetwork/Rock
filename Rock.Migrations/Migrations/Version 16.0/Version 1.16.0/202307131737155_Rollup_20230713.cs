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

    using Rock.Configuration;
    using Rock.Utility.Settings;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20230713 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddDotLiquidWarningBlockToHomePageUp();
            AddSystemSettingsForRaceAndEthnicityPickerLabel();
            AddAttributeForAssessmentList();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddDotLiquidWarningBlockToHomePageDown();
        }

        /// <summary>
        /// KA: Migration to add SystemSettings for Race and Ethnicity Picker Labels
        /// </summary>
        private void AddSystemSettingsForRaceAndEthnicityPickerLabel()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER, string.Empty, "core_Person Ethnicity Label", "", 0, "Ethnicity", Guid.NewGuid().ToString(),
                Rock.SystemKey.SystemSetting.PERSON_ETHNICITY_LABEL, false );

            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER, string.Empty, "core_Person Race Label", "", 0, "Race", Guid.NewGuid().ToString(),
                Rock.SystemKey.SystemSetting.PERSON_RACE_LABEL, false );
        }

        /// <summary>
        /// GJ: Assessment List Template Update
        /// </summary>
        private void AddAttributeForAssessmentList()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"The lava template to use to format the entire block.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 3, @"<div class=""panel panel-default"">
    <div class=""panel-heading"">Assessments</div>
    <div class=""panel-body"">
            {% for assessmenttype in AssessmentTypes %}
                {% if assessmenttype.LastRequestObject %}
                    {% if assessmenttype.LastRequestObject.Status == 'Complete' %}
                        <div class='panel panel-success'>
                            <div class=""panel-heading"">{{ assessmenttype.Title }}<br />
                                Completed: {{ assessmenttype.LastRequestObject.CompletedDate | Date:'sd' }} <br />
                                <div>
                                <a href=""{{ assessmenttype.AssessmentResultsPath}}"" class=""d-inline-block"">View Results</a> {% if assessmenttype.AssessmentRetakePath != '' %}<br/><a href=""{{ assessmenttype.AssessmentRetakePath }}"" class=""d-inline-block mt-2"">Retake Assessment</a>{% endif %}</div>
                            </div>
                        </div>
                    {% elseif assessmenttype.LastRequestObject.Status == 'Pending' %}
                        <div class=""panel panel-warning"">
                            <div class=""panel-heading"">
                                {{ assessmenttype.Title }}<br />
                                Requested: {{assessmenttype.LastRequestObject.Requester}} ({{ assessmenttype.LastRequestObject.RequestedDate | Date:'sd'}})<br />
                                <a href=""{{ assessmenttype.AssessmentPath}}"">Start Assessment</a>
                            </div>
                        </div>
                    {% endif %}
                    {% else %}
                        <div class=""panel panel-default"">
                            <div class=""panel-heading"">{{ assessmenttype.Title }}<br/>
                                Available<br />
                                <a href=""{{ assessmenttype.AssessmentPath}}"">Start Assessment</a>
                            </div>
                        </div>
                {% endif %}
            {% endfor %}
    </div>
</div>", "044D444A-ECDC-4B7A-8987-91577AAB227C" );
        }

        /// <summary>
        /// KA: Migration to add HTML block to home page if Rock is using DotLiquid Lava Engine
        /// </summary>
        public void AddDotLiquidWarningBlockToHomePageUp()
        {
            if ( RockApp.Current.GetCurrentLavaEngineName() != "Fluid" )
            {
                // Add Html Block Instance to HomePage Main Zone
                RockMigrationHelper.AddBlock( Rock.SystemGuid.Page.INTERNAL_HOMEPAGE, "", Rock.SystemGuid.BlockType.HTML_CONTENT, "DotLiquid Warning", "Main", "", "", -1, "CAEC8719-BEDF-4BE3-9847-55DE93624974" );

                // ALLOW VIEW PERMISSION FOR RSR - Rock Administration
                RockMigrationHelper.AddSecurityAuthForBlock( "CAEC8719-BEDF-4BE3-9847-55DE93624974", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", Model.SpecialRole.None, "6FCE5668-D8E8-4847-B482-B0BBD304D2D3" );

                // DENY VIEW PERMISSION FOR All - Users
                RockMigrationHelper.AddSecurityAuthForBlock( "CAEC8719-BEDF-4BE3-9847-55DE93624974", 0, "View", false, null, Model.SpecialRole.AllUsers, "3CE62E9B-11E8-4FC5-990F-E67CA94DDCF4" );

                // Add Html Content
                Sql( @"DECLARE @BlockId INT = (SELECT Id FROM Block WHERE [Guid] = 'CAEC8719-BEDF-4BE3-9847-55DE93624974');
IF @BlockId IS NOT NULL AND NOT EXISTS (SELECT Id FROM HtmlContent WHERE BlockId = @BlockId)
BEGIN
	INSERT INTO HtmlContent ([BlockId], [Content], [IsApproved], [ApprovedDateTime], [Guid], [Version])
	VALUES (@BlockId,'{% assign lavaEngine = ''LavaEngine'' | RockInstanceConfig %}
{% if lavaEngine != ""Fluid"" %}
    <div class=""alert alert-warning"">
        <h4>Warning: Old Lava Engine Still In Use</h4>
        <p>
            The system is currently using the <b>{{ lavaEngine }}</b> Lava engine but it will no longer operate in the next release of Rock.
            Please see <a href=""https://community.rockrms.com/connect/ending-support-for-dotliquid-lava-engine"">Ending Support for ""DotLiquid"" Lava Engine
</a> for details about how to resolve this.
        </p>    
    </div>
{% endif %}', 1, GETDATE(), NEWID(), 0)
END" );
            }

        }

        /// <summary>
        /// KA: Migration to add HTML block to home page if Rock is using DotLiquid Lava Engine
        /// </summary>
        public void AddDotLiquidWarningBlockToHomePageDown()
        {
            RockMigrationHelper.DeleteSecurityAuthForBlock( "CAEC8719-BEDF-4BE3-9847-55DE93624974" );
            RockMigrationHelper.DeleteBlock( "CAEC8719-BEDF-4BE3-9847-55DE93624974" );
        }
    }
}
