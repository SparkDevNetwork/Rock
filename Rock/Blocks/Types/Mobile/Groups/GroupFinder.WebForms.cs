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

using Rock.Attribute;
using Rock.Web;

namespace Rock.Blocks.Types.Mobile.Groups
{
    public partial class GroupFinder : RockBlockType
    {
        /// <summary>
        /// Defines the control that will provide the additional custom
        /// settings for the <see cref="GroupFinder"/> block.
        /// </summary>
        /// <seealso cref="Rock.Web.RockCustomSettingsUserControlProvider" />
        [CustomSettingsBlockType( typeof( GroupFinder ), Model.SiteType.Mobile )]
        public class GroupFinderCustomSettingsProvider : RockCustomSettingsUserControlProvider
        {
            /// <inheritdoc/>
            protected override string UserControlPath => "~/Blocks/Mobile/GroupFinderSettings.ascx";

            /// <inheritdoc/>
            public override string CustomSettingsTitle => "Basic Settings";
        }
    }
}
