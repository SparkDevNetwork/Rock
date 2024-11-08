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

namespace Rock.CheckIn.v2.Labels
{
    /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.RectangleFieldConfigurationBag"/>
    internal class RectangleFieldConfiguration : IFieldConfiguration
    {
        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.RectangleFieldConfigurationBag.IsBlack" path="/summary"/>
        public bool IsBlack { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.RectangleFieldConfigurationBag.IsFilled" path="/summary"/>
        public bool IsFilled { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.RectangleFieldConfigurationBag.BorderThickness" path="/summary"/>
        public int BorderThickness { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.RectangleFieldConfigurationBag.CornerRadius" path="/summary"/>
        public int CornerRadius { get; set; }

        /// <inheritdoc/>
        public void Initialize( Dictionary<string, string> values )
        {
            IsBlack = values.GetValueOrNull( "isBlack" ).AsBoolean();
            IsFilled = values.GetValueOrNull( "isFilled" ).AsBoolean();
            BorderThickness = values.GetValueOrNull( "borderThickness" ).AsIntegerOrNull() ?? 1;
            CornerRadius = values.GetValueOrNull( "cornerRadius" ).AsInteger();
        }
    }
}
