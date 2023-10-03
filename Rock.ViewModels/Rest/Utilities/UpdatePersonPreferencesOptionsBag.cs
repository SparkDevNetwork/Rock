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

namespace Rock.ViewModels.Rest.Utilities
{
    /// <summary>
    /// Defines the options that can be sent to one of the PersonPreferences API
    /// endpoints when setting preference values.
    /// </summary>
    public class UpdatePersonPreferencesOptionsBag
    {
        /// <summary>
        /// Gets or sets the preference keys and values that should be set.
        /// </summary>
        /// <value>The preference keys and values that should be set.</value>
        public Dictionary<string, string> Preferences { get; set; }
    }
}
