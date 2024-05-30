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

using Rock.Enums.CheckIn.Labels;
using Rock.Model;
using Rock.ViewModels.CheckIn.Labels;

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
        /// A string that identifies the category to use in the drop down UI
        /// when designing the label.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The comparison types that are supported by by this data source.
        /// </summary>
        public ComparisonType SupportedComparisionTypes { get; set; }

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
        /// The custom fields to display in the UI when editing the field. This
        /// allows a small amount of customization without requiring custom
        /// logic in the UI to show/hide inputs.
        /// </summary>
        public List<CustomFieldInputBag> CustomFields { get; set; }

        /// <summary>
        /// Gest the value to use when comparing the value represented by the
        /// data source with a filter. The returned value is passed to the
        /// standard filtering logic that is also used in reporting.
        /// </summary>
        /// <param name="field">The field that this data source is representing.</param>
        /// <param name="printRequest">The print request we are getting the value for.</param>
        /// <returns>The value to be used for comparison or <c>null</c>.</returns>
        public abstract object GetComparisionValue( LabelField field, PrintLabelRequest printRequest );

        /// <summary>
        /// Gest the values to use when displaying the field value on a label.
        /// The returned values will be passed to the formatter.
        /// </summary>
        /// <param name="field">The field that this data source is representing.</param>
        /// <param name="printRequest">The print request we are getting the values for.</param>
        /// <returns>The values to be formatted. A list with a single empty string should be returned instead of <c>null</c> or an empty list.</returns>
        public abstract List<object> GetValues( LabelField field, PrintLabelRequest printRequest );
    }

    /// <summary>
    /// Represents a data source for whose label data is expected to be an
    /// instance of <typeparamref name="TLabelData"/>.
    /// </summary>
    /// <typeparam name="TLabelData">The type of label data expected.</typeparam>
    internal abstract class FieldDataSource<TLabelData> : FieldDataSource
    {
        /// <summary>
        /// The function that will get the value to use when performing
        /// comparisons for conditional display of fields and labels.
        /// </summary>
        public Func<TLabelData, LabelField, PrintLabelRequest, object> ComparisonValueFunc { get; set; }

        /// <inheritdoc/>
        public sealed override object GetComparisionValue( LabelField field, PrintLabelRequest printRequest )
        {
            return ComparisonValueFunc?.Invoke( ( TLabelData ) printRequest.LabelData, field, printRequest );
        }
    }
}
