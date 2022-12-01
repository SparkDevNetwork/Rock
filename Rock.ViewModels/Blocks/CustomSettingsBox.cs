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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks
{
    /// <summary>
    /// A box that contains the required information to render a custom
    /// settings component.
    /// </summary>
    /// <typeparam name="TSettings">The type of the settings property.</typeparam>
    /// <typeparam name="TOptions">The type of the options property.</typeparam>
    public class CustomSettingsBox<TSettings, TOptions> : IValidPropertiesBox
        where TOptions: new()
    {
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public TSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>The options.</value>
        public TOptions Options { get; set; } = new TOptions();

        /// <summary>
        /// Gets or sets the valid properties.
        /// </summary>
        /// <value>The valid properties.</value>
        public List<string> ValidProperties { get; set; }

        /// <summary>
        /// Gets or sets the security grant token.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
