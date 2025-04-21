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
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.LabelDesigner
{
    /// <summary>
    /// A data source that can be selected on the text fields.
    /// </summary>
    public class DataSourceBag
    {
        /// <summary>
        /// <para>
        /// The unique key that identifies this data source.
        /// </para>
        /// <para>
        /// Properties should use a lowercase key that corresponds to the
        /// property path represented by the source such as <c>person.id</c>.
        /// </para>
        /// <para>
        /// Attributes should use a lowercase key that contains the unique
        /// identifier of the attribute prefixed with <c>attribute:</c>.
        /// </para>
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The friendly name that describes the value that will be retrieved
        /// with this data source.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The text field sub-type that this data source is associated with.
        /// This is used to filter the available data sources depending on
        /// which text field is selected.
        /// </summary>
        public TextFieldSubType TextSubType { get; set; }

        /// <summary>
        /// Indicates if this data source represents a value that is a collection
        /// of multiple values.
        /// </summary>
        public bool IsCollection { get; set; }

        /// <summary>
        /// A string that identifies the category to use in the drop down UI
        /// when designing the label.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The custom fields to display when this source is selected.
        /// </summary>
        public List<CustomFieldInputBag> CustomFields { get; set; }

        /// <summary>
        /// The custom formatter options that should be displayed and selected
        /// from.
        /// </summary>
        public List<DataFormatterOptionBag> FormatterOptions { get; set; }
    }
}
