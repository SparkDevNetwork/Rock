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
using System.Linq;

using Rock.ViewModels.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels.Formatters
{
    /// <summary>
    /// Formats a set of raw values from the label data into a set of strings
    /// that are properly formatted to for display.
    /// </summary>
    internal abstract class DataFormatter
    {
        /// <summary>
        /// <para>
        /// The formatting options available to be selected by the person while
        /// designing the label. This may be null or empty if no options are
        /// available.
        /// </para>
        /// <para>
        /// An example would be a date formatter where the options represent
        /// the available date formats (long, short, time only, etc).
        /// </para>
        /// </summary>
        public List<DataFormatterOptionBag> Options { get; } = new List<DataFormatterOptionBag>();

        /// <summary>
        /// The custom fields to display in the UI when editing the field. This
        /// allows a small amount of customization without requiring custom
        /// logic in the UI to show/hide inputs.
        /// </summary>
        public List<CustomFieldInputBag> CustomFields { get; set; }

        /// <summary>
        /// Gets the formatted values from the raw values provided by the field.
        /// </summary>
        /// <param name="values">The raw values to be formatted.</param>
        /// <param name="optionKey">The formatter option that was selected at design time.</param>
        /// <param name="field">The field this value is being formatted for.</param>
        /// <param name="printRequest">The print request we are formatting the values for.</param>
        /// <returns>A list of formatted string values.</returns>
        public abstract List<string> GetFormattedValues( List<object> values, string optionKey, LabelField field, PrintLabelRequest printRequest );
    }

    /// <summary>
    /// Formats a set of raw values from the label data into a set of strings
    /// that are properly formatted to for display.
    /// </summary>
    /// <typeparam name="TValue">The type of the raw value expected.</typeparam>
    internal abstract class DataFormatter<TValue> : DataFormatter
    {
        /// <inheritdoc/>
        public override sealed List<string> GetFormattedValues( List<object> values, string optionKey, LabelField field, PrintLabelRequest printRequest )
        {
            var optionValue = Options.FirstOrDefault( o => o.Key == optionKey )?.Value;

            return values.Cast<TValue>().Select( v => GetFormattedValue( v, optionValue, field, printRequest ) ).ToList();
        }

        /// <summary>
        /// Gets the formatted value from the raw value provided by the field.
        /// </summary>
        /// <param name="value">The raw values to be formatted.</param>
        /// <param name="optionValue">The formatter option value that was selected at design time.</param>
        /// <param name="field">The field this value is being formatted for.</param>
        /// <param name="printRequest">The print request we are formatting the values for.</param>
        /// <returns>The formatted string value.</returns>
        protected abstract string GetFormattedValue( TValue value, string optionValue, LabelField field, PrintLabelRequest printRequest );
    }
}
