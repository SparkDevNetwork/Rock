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

using Rock.Model;
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels.Formatters
{
    /// <summary>
    /// Formats a <see cref="Person"/> value into a selected format from
    /// the list of options.
    /// </summary>
    internal sealed class PersonAgeDataFormatter : DataFormatter<double?>
    {
        /// <summary>
        /// The singleton instance of the person age formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new PersonAgeDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="PersonAgeDataFormatter"/>.
        /// </summary>
        private PersonAgeDataFormatter()
        {
            CustomFields = new List<CustomFieldInputBag>
            {
                new CustomFieldInputBag
                {
                    Key = "MonthCutOff",
                    Label = "Month Cut-off",
                    HelpText = "The age will be displayed in months until the person's age is equal to or greater than this number of months. If not set then 24 will be assumed."
                }
            };

            Options.Add( new DataFormatterOptionBag
            {
                Key = "a0ae4663-3abe-40a0-8fb2-3a63b7a22e72",
                Name = "2yrs",
                Value = "Long"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "7e6c1da8-b411-43d4-9dd0-da32753368a6",
                Name = "2yo",
                Value = "Medium"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "582cd1ea-24d4-4480-b5d8-dda7d748facd",
                Name = "2",
                Value = "Short"
            } );
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( double? value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( !value.HasValue )
            {
                return string.Empty;
            }

            var monthCutOff = field.Field.CustomData?.GetValueOrNull( "MonthCutOff" )
                ?.AsDoubleOrNull()
                ?? 24.0;
            var ageCutOff = monthCutOff / 12.0;

            if ( value.Value < ageCutOff )
            {
                var daysInMonth = 365.0 / 12.0;
                var months = ( int ) Math.Floor( value.Value * 365 / daysInMonth );

                return $"{months}mo";
            }

            var years = ( int ) Math.Floor( value.Value );

            if ( optionValue == "Short" )
            {
                return $"{years}";
            }
            else if ( optionValue == "Medium" )
            {
                return $"{years}yo";
            }
            else if ( optionValue == "Long" )
            {
                if ( years == 1 )
                {
                    return $"{years}yr";
                }
                else
                {
                    return $"{years}yrs";
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
