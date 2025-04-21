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

namespace Rock.ViewModels.CheckIn.Labels
{
    /// <summary>
    /// Defines a single custom data input used by a label field source. All
    /// custom fields are currently text inputs.
    /// </summary>
    public class CustomFieldInputBag
    {
        /// <summary>
        /// The key that will be used to store the custom data value.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The label that will be displayed above the input field.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The help text to describe the purpose of the input.
        /// </summary>
        public string HelpText { get; set; }
    }
}
