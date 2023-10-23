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
using Rock.Attribute;
using Rock.Web;

namespace Rock.Blocks
{
    /// <summary>
    /// Defines the control that will provide the Basic Settings tab content
    /// for all RockMobileBockType blocks.
    /// </summary>
    /// <seealso cref="Rock.Web.RockCustomSettingsUserControlProvider" />
    [CustomSettingsBlockType( typeof( RockBlockType ), Model.SiteType.Mobile )]
    public class RockMobileBlockTypeCustomSettingsProvider : RockCustomSettingsUserControlProvider
    {
        /// <summary>
        /// Gets the path to the user control file.
        /// </summary>
        /// <value>
        /// The path to the user control file.
        /// </value>
        protected override string UserControlPath => "~/Blocks/Mobile/MobileCustomSettings.ascx";

        /// <summary>
        /// Gets the custom settings title. Used when displaying tabs or links to these settings.
        /// </summary>
        /// <value>
        /// The custom settings title.
        /// </value>
        public override string CustomSettingsTitle => "Mobile Settings";
    }

}
