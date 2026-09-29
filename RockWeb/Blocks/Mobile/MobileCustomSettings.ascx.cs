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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Mobile
{
    /// <summary>
    /// Handles the Advanced Settings panel for all RockMobileBlockType blocks.
    /// </summary>
    /// <seealso cref="System.Web.UI.UserControl" />
    /// <seealso cref="Rock.Web.IRockCustomSettingsUserControl" />
    public partial class MobileCustomSettings : System.Web.UI.UserControl, IRockCustomSettingsUserControl
    {
        #region IRockCustomSettingsUserControl implementation

        /// <summary>
        /// Update the custom UI to reflect the current settings found in the entity.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        public void ReadSettingsFromEntity( IHasAttributes attributeEntity )
        {
            var mobileBlock = ( BlockCache ) attributeEntity;
            var additionalSettings = mobileBlock.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();

            nbCacheDuration.Text = additionalSettings.CacheDuration.ToString();
            cbProcessLavaOnServer.Checked = additionalSettings.ProcessLavaOnServer;
            cbProcessLavaOnClient.Checked = additionalSettings.ProcessLavaOnClient;
            cbShowOnTablet.Checked = additionalSettings.ShowOnTablet;
            cbShowOnPhone.Checked = additionalSettings.ShowOnPhone;
            cbRequiresNetwork.Checked = additionalSettings.RequiresNetwork;
            ceNoNetworkContent.Text = additionalSettings.NoNetworkContent;
            ceCssStyles.Text = additionalSettings.CssStyles;
        }

        /// <summary>
        /// Update the entity with values from the custom UI.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <remarks>
        /// Do not save the entity, it will be automatically saved later. This call will be made inside
        /// a SQL transaction for the passed rockContext. If you need to make changes to the database
        /// do so on this context so they can be rolled back if something fails during the final save.
        /// </remarks>
        public void WriteSettingsToEntity( IHasAttributes attributeEntity, RockContext rockContext )
        {
            var mobileBlock = ( Block ) attributeEntity;
            var additionalSettings = mobileBlock.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();

            additionalSettings.CacheDuration = nbCacheDuration.Text.AsInteger();
            additionalSettings.ProcessLavaOnServer = cbProcessLavaOnServer.Checked;
            additionalSettings.ProcessLavaOnClient = cbProcessLavaOnClient.Checked;
            additionalSettings.ShowOnTablet = cbShowOnTablet.Checked;
            additionalSettings.ShowOnPhone = cbShowOnPhone.Checked;
            additionalSettings.RequiresNetwork = cbRequiresNetwork.Checked;
            additionalSettings.NoNetworkContent = ceNoNetworkContent.Text;
            additionalSettings.CssStyles = ceCssStyles.Text;

            mobileBlock.AdditionalSettings = additionalSettings.ToJson();
        }

        #endregion
    }
}
