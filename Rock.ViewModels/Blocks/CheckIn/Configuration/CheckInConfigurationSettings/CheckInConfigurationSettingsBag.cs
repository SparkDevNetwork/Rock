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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The check-in configuration settings needed for the Check-in Configuration Settings block.
    /// </summary>
    public class CheckInConfigurationSettingsBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a comma-delimited summary of the active service times configured on this check-in type.
        /// Display-only; computed server-side and shown on the view panel.
        /// </summary>
        public string ScheduledTimes { get; set; }

        /// <summary>
        /// Gets or sets the basic identifying settings (name, icon, description) for this check-in
        /// configuration.
        /// </summary>
        public CheckInBasicSettingsBag BasicSettings { get; set; }

        /// <summary>
        /// Gets or sets the check-in type flow settings.
        /// </summary>
        public CheckInTypeFlowSettingsBag TypeFlowSettings { get; set; }

        /// <summary>
        /// Gets or sets the kiosk feature settings.
        /// </summary>
        public CheckInKioskFeaturesSettingsBag KioskFeaturesSettings { get; set; }

        /// <summary>
        /// Gets or sets the display settings.
        /// </summary>
        public CheckInDisplaySettingsBag DisplaySettings { get; set; }

        /// <summary>
        /// Gets or sets the supervision settings.
        /// </summary>
        public CheckInSupervisionSettingsBag SupervisionSettings { get; set; }

        /// <summary>
        /// Gets or sets the search settings.
        /// </summary>
        public CheckInSearchSettingsBag SearchSettings { get; set; }

        /// <summary>
        /// Gets or sets the security code settings.
        /// </summary>
        public CheckInSecurityCodesSettingsBag SecurityCodesSettings { get; set; }

        /// <summary>
        /// Gets or sets the general registration settings.
        /// </summary>
        public CheckInGeneralRegistrationSettingsBag GeneralRegistrationSettings { get; set; }

        /// <summary>
        /// Gets or sets the adult registration settings.
        /// </summary>
        public CheckInAdultRegistrationSettingsBag AdultRegistrationSettings { get; set; }

        /// <summary>
        /// Gets or sets the child registration settings.
        /// </summary>
        public CheckInChildRegistrationSettingsBag ChildRegistrationSettings { get; set; }

        /// <summary>
        /// Gets or sets the family registration settings.
        /// </summary>
        public CheckInFamilyRegistrationSettingsBag FamilyRegistrationSettings { get; set; }

        /// <summary>
        /// Gets or sets the child relationship settings.
        /// </summary>
        public CheckInChildRelationshipSettingsBag ChildRelationshipSettings { get; set; }

        /// <summary>
        /// Gets or sets the registration workflow settings.
        /// </summary>
        public CheckInRegistrationWorkflowSettingsBag RegistrationWorkflowSettings { get; set; }

        /// <summary>
        /// Gets or sets the additional filters and settings (ability level, age/grade matching, proximity
        /// check-in, etc.).
        /// </summary>
        public CheckInAdditionalFiltersAndSettingsBag AdditionalFiltersAndSettings { get; set; }

        /// <summary>
        /// Gets or sets the special needs settings.
        /// </summary>
        public CheckInSpecialNeedsSettingsBag SpecialNeedsSettings { get; set; }

        /// <summary>
        /// Gets or sets the classic-experience display settings (refresh interval, success template
        /// display mode).
        /// </summary>
        public CheckInClassicDisplaySettingsBag ClassicDisplaySettings { get; set; }

        /// <summary>
        /// Gets or sets the classic-experience screen template settings.
        /// </summary>
        public CheckInClassicTemplatesSettingsBag ClassicTemplatesSettings { get; set; }

        /// <summary>
        /// Gets or sets the classic-experience custom header text settings.
        /// </summary>
        public CheckInClassicCustomHeaderTextSettingsBag ClassicCustomHeaderTextSettings { get; set; }
    }
}
