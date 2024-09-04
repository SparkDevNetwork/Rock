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

using Rock.Enums.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels
{
    /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.BarcodeFieldConfigurationBag"/>
    internal class BarcodeFieldConfiguration : IFieldConfiguration
    {
        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.BarcodeFieldConfigurationBag.Format" path="/summary"/>
        public BarcodeFormat Format { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.BarcodeFieldConfigurationBag.IsDynamic" path="/summary"/>
        public bool IsDynamic { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.BarcodeFieldConfigurationBag.DynamicTextTemplate" path="/summary"/>
        public string DynamicTextTemplate { get; set; }

        /// <inheritdoc/>
        public void Initialize( Dictionary<string, string> values )
        {
            Format = values.GetValueOrNull( "format" ).ConvertToEnumOrNull<BarcodeFormat>() ?? BarcodeFormat.QRCode;
            IsDynamic = values.GetValueOrNull( "isDynamic" ).AsBoolean();
            DynamicTextTemplate = values.GetValueOrNull( "dynamicTextTemplate" ) ?? string.Empty;
        }
    }
}
