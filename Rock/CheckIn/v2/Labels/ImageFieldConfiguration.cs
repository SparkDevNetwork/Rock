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

using System;
using System.Collections.Generic;

namespace Rock.CheckIn.v2.Labels
{
    /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.ImageFieldConfigurationBag"/>
    internal class ImageFieldConfiguration : IFieldConfiguration
    {
        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.ImageFieldConfigurationBag.ImageData" path="/summary"/>
        public byte[] ImageData { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.ImageFieldConfigurationBag.IsColorInverted" path="/summary"/>
        public bool IsColorInverted { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.ImageFieldConfigurationBag.Brightness" path="/summary"/>
        public float Brightness { get; set; } = 1;

        /// <inheritdoc/>
        public void Initialize( Dictionary<string, string> values )
        {
            if ( values.GetValueOrNull( "imageData" ).IsNotNullOrWhiteSpace() )
            {
                try
                {
                    ImageData = Convert.FromBase64String( values.GetValueOrNull( "imageData" ) );
                }
                catch
                {
                    ImageData = new byte[0];
                }
            }
            else
            {
                ImageData = new byte[0];
            }

            IsColorInverted = values.GetValueOrNull( "isColorInverted" ).AsBoolean();
            Brightness = ( float ) ( values.GetValueOrNull( "brightness" ).AsDoubleOrNull() ?? 1 );
        }
    }
}
