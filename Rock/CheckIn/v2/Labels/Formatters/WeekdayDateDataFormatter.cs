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

using Rock.ViewModels.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels.Formatters
{
    /// <summary>
    /// Formats a <see cref="DateTime"/> value into a selected format from
    /// the list of options.
    /// </summary>
    internal class WeekdayDateDataFormatter : DataFormatter<DateTime?>
    {
        /// <summary>
        /// The singleton instance of the person age formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new WeekdayDateDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="WeekdayDateDataFormatter"/>.
        /// </summary>
        private WeekdayDateDataFormatter()
        {
            Options.Add( new DataFormatterOptionBag
            {
                Key = "000baf0e-9b3c-44eb-9dfa-77bae5291ce5",
                Name = "Monday",
                Value = "dddd"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "00f2731f-0ce3-4ab4-9318-ff406a86a38b",
                Name = "Mon",
                Value = "ddd"
            } );
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( DateTime? value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( !value.HasValue || optionValue.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return value.Value.ToString( optionValue );
        }
    }
}
