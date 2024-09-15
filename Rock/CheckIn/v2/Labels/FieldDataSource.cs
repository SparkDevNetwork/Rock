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

using Rock.CheckIn.v2.Labels.Formatters;
using Rock.Enums.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// A single data source that describes how to retrieve a set of values
    /// from the label data.
    /// </summary>
    internal abstract class FieldDataSource
    {
        /// <summary>
        /// <para>
        /// The unique key that identifies this data source. This value must
        /// never change for the life of a data source so it is important to
        /// follow the convention.
        /// </para>
        /// <para>
        /// Manually defined data sources should use a guid value.
        /// </para>
        /// <para>
        /// Generated properties should use a lowercase key that corresponds to
        /// the property path represented by the source such as <c>person.id</c>.
        /// </para>
        /// <para>
        /// Generated attributes should use a lowercase key that contains a
        /// <c>attribute:</c> prefix, the property path to the entity followed
        /// by another <c>:</c> and then the attribute guid. Such as
        /// <c>attribute:person:e801638a-2b9a-4382-ad8f-36001c7bc0ee</c>
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
        /// A string that identifies the category to use in the drop down UI
        /// when designing the label.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The custom formatter that will convert the raw data value into a
        /// friendly text string. If <c>null</c> then the value will be converted
        /// with a call to <see cref="object.ToString"/>.
        /// </summary>
        public DataFormatter Formatter { get; set; }

        /// <summary>
        /// <para>
        /// Will be <c>true</c> if this data source can return more than one
        /// value.
        /// </para>
        /// <para>
        /// For example, a data source that displays the first name of every
        /// person being checked in would be a collection data source.
        /// </para>
        /// </summary>
        public abstract bool IsCollection { get; }

        /// <summary>
        /// Gest the values to use when displaying the field value on a label.
        /// The returned values will be passed to the formatter.
        /// </summary>
        /// <param name="field">The field that this data source is representing.</param>
        /// <param name="printRequest">The print request we are getting the values for.</param>
        /// <returns>The values to be formatted. A list with a single empty string should be returned instead of <c>null</c> or an empty list.</returns>
        public abstract List<object> GetValues( LabelField field, PrintLabelRequest printRequest );
    }
}
