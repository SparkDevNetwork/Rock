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
    internal sealed class DateDataFormatter : DataFormatter<DateTime?>
    {
        /// <summary>
        /// The singleton instance of the date formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new DateDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="DateDataFormatter"/>.
        /// </summary>
        private DateDataFormatter()
        {
            Options.Add( new DataFormatterOptionBag
            {
                Key = "6828eeb1-edf9-4d28-95ca-427f02e1b522",
                Name = "Sun 4/21",
                Value = "ddd M/d"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "ef1f2a8e-11e4-4c4d-a512-5da503a48fe4",
                Name = "4/7/2024",
                Value = "M/d/yyyy"
            } );
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( DateTime? value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( !value.HasValue )
            {
                return string.Empty;
            }
            else if ( optionValue == null )
            {
                return value.ToStringSafe();
            }
            else
            {
                return value.Value.ToString( optionValue );
            }
        }
    }
}
