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
    /// A single option that defines how a data formatter will format the value
    /// into a string representation.
    /// </summary>
    public class DataFormatterOptionBag
    {
        /// <summary>
        /// The key that uniquely identifies this option. This allows the
        /// value to change for a bug fix without breaking existing selections.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The name that is displayed on the label designer.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the formatter option. This is used by the formatter to
        /// decide what format to use.
        /// </summary>
        public string Value { get; set; }
    }
}
