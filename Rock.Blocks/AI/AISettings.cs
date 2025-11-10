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

using System.ComponentModel;

using Rock.AI.Agent;
using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks.AI.AISettings;
using Rock.Web;

namespace Rock.Blocks.AI
{
    /// <summary>
    /// Configures system settings related to the AI features in Rock.
    /// </summary>

    [DisplayName( "AI Settings" )]
    [Category( "AI" )]
    [Description( "Configures system settings related to the AI features in Rock." )]
    [IconCssClass( "ti ti-settings" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    #endregion

    [SystemGuid.EntityTypeGuid( "d385228b-6d65-442b-8874-bf7d1f0fe84f" )]
    [SystemGuid.BlockTypeGuid( "7b2a9827-8584-46ae-b2fd-e52a7f131fbf" )]
    internal class AISettings : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var settings = SystemSettings.GetValue( SystemKey.SystemSetting.AI_AGENT_SYSTEM_SETTINGS )
                ?.FromJsonOrNull<AgentSystemSettings>()
                ?? new AgentSystemSettings();

            var box = new AISettingsOptionsBag
            {
                ReturnUrl = this.GetParentPageUrl(),
                Settings = new AISettingsBag
                {
                    OrganizationPrompt = settings.OrganizationPrompt
                },
            };

            return box;
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult Save( AISettingsBag bag )
        {
            var settings = SystemSettings.GetValue( SystemKey.SystemSetting.AI_AGENT_SYSTEM_SETTINGS )
                ?.FromJsonOrNull<AgentSystemSettings>()
                ?? new AgentSystemSettings();

            settings.OrganizationPrompt = bag.OrganizationPrompt;

            SystemSettings.SetValue( SystemKey.SystemSetting.AI_AGENT_SYSTEM_SETTINGS, settings.ToJson() );

            return ActionOk();
        }

        #endregion
    }
}
