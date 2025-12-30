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

namespace Rock.AI.Agent
{
    /// <summary>
    /// The system settings that apply to all AI agents in the organization.
    /// This is stored as a JSON-serialized object in the system setting
    /// <see cref="SystemKey.SystemSetting.AI_AGENT_SYSTEM_SETTINGS"/>.
    /// </summary>
    internal class AgentSystemSettings
    {
        #region Constants

        /// <summary>
        /// The default <see cref="OrganizationPrompt"/> to use if no custom
        /// prompt is defined.
        /// </summary>
        public const string DefaultOrganizationPrompt = "You are a Christian church called {{ 'Global' | Attribute:'OrganizationName' }} located at {{ 'Global' | Attribute:'OrganizationAddress' }} with a primary phone number of {{ 'Global' | Attribute:'OrganizationPhone' }}.";

        #endregion

        #region Properties

        /// <summary>
        /// The organization prompt to use for all agents in this organization.
        /// If blank or whitespace then the default prompt defined in
        /// <see cref="DefaultOrganizationPrompt"/> will be used.
        /// </summary>
        public string OrganizationPrompt { get; set; }

        #endregion
    }
}
