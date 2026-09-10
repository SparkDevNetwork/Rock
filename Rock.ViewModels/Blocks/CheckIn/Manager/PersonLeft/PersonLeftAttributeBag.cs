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
namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonLeft
{
    /// <summary>
    /// Describes a single read-only attribute value shown in the adult or
    /// child attribute panel of the Check-in Manager Person Profile
    /// (limited) block.
    /// </summary>
    public class PersonLeftAttributeBag
    {
        /// <summary>
        /// Gets or sets the attribute's display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the attribute key, used only as a unique v-for key
        /// on the client.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the attribute's value pre-rendered as display HTML
        /// (already run through the field type's FormatValueAsHtml). Rendered
        /// with v-html on the client.
        /// </summary>
        public string FormattedValueHtml { get; set; }
    }
}
