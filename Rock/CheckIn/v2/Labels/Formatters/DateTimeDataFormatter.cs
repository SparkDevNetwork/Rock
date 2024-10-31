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
    internal sealed class DateTimeDataFormatter : DataFormatter<DateTime?>
    {
        /// <summary>
        /// The singleton instance of the date formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new DateTimeDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="DateTimeDataFormatter"/>.
        /// </summary>
        private DateTimeDataFormatter()
        {
            DateTime exampleDateTime = new DateTime( 2024, 04, 21, 17, 30, 0, 0 );
            DateTimeFormatInfo dtf = System.Globalization.CultureInfo.InstalledUICulture.DateTimeFormat;

            Options.Add( new DataFormatterOptionBag
            {
                Key = "D7267850-5EBE-4540-9D8F-C453F27C09E3",
                Name = exampleDateTime.ToString( "t", dtf ),
                Value = dtf.ShortTimePattern
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "3B0EF43E-6BE2-4D24-BD3B-131456ACB1CF",
                Name = $"Sun {exampleDateTime.ToString( "t", dtf )}",
                Value = $"ddd {dtf.ShortTimePattern}"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "5926E634-48AA-4212-9571-7F08AE491E6C",
                Name = exampleDateTime.ToString( "d", dtf ),
                Value = dtf.ShortDatePattern
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "C743FFCC-B827-4FAD-886D-7A3463C902A9",
                Name = exampleDateTime.ToString( "g", dtf ),
                Value = $"{dtf.ShortDatePattern} {dtf.ShortTimePattern}"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "37D2D6E1-C9D1-4C74-B1A0-06A1AEE3FDE8",
                Name = exampleDateTime.ToString( "f", dtf ),
                Value = $"{dtf.LongDatePattern} {dtf.ShortTimePattern}"
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
