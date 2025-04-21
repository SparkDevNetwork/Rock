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
using System.Globalization;
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
            DateTime exampleDateTime = new DateTime( 2024, 04, 21, 17, 30, 0, 0 );
            DateTimeFormatInfo dtf = System.Globalization.CultureInfo.InstalledUICulture.DateTimeFormat;

            Options.Add( new DataFormatterOptionBag
            {
                Key = "652864F9-57EC-492E-B548-5FC8ECD372D6",
                Name = exampleDateTime.ToString( "M", dtf ),
                Value = dtf.MonthDayPattern
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "280B1804-C055-4133-AE37-5534D40A5748",
                Name = exampleDateTime.ToString( "Y", dtf ),
                Value = dtf.YearMonthPattern
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "6828eeb1-edf9-4d28-95ca-427f02e1b522",
                Name = $"Sun {exampleDateTime.ToString( "M", dtf )}",
                Value = $"ddd {dtf.MonthDayPattern}"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "ef1f2a8e-11e4-4c4d-a512-5da503a48fe4",
                Name = exampleDateTime.ToString( "d", dtf ),
                Value = dtf.ShortDatePattern
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "FB24EDC6-BA92-4045-ADAF-B21A12BAC653",
                Name = exampleDateTime.ToString( "D", dtf ),
                Value = dtf.LongDatePattern
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
