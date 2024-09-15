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

using System.Collections.Generic;
using System.Linq;

namespace Rock.CheckIn.v2.Labels
{
    /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.IconFieldConfigurationBag"/>
    internal class IconFieldConfiguration : IFieldConfiguration
    {
        /// <summary>
        /// The icon to display in the field. May be <c>null</c> if the icon
        /// was not valid.
        /// </summary>
        public LabelIcon Icon { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.IconFieldConfigurationBag.IsColorInverted" path="/summary"/>
        public bool IsColorInverted { get; set; }

        /// <inheritdoc/>
        public void Initialize( Dictionary<string, string> values )
        {
            Icon = LabelIcon.StandardIcons.FirstOrDefault( a => a.Value == values.GetValueOrNull( "icon" ) );
            IsColorInverted = values.GetValueOrNull( "isColorInverted" ).AsBoolean();
        }
    }
}
