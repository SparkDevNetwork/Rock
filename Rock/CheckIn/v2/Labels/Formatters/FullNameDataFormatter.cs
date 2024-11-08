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

using Rock.Model;
using Rock.ViewModels.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels.Formatters
{
    /// <summary>
    /// Formats a <see cref="Person"/> value into a selected format from
    /// the list of options.
    /// </summary>
    internal sealed class FullNameDataFormatter : DataFormatter<Person>
    {
        /// <summary>
        /// The singleton instance of the full name formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new FullNameDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="FullNameDataFormatter"/>.
        /// </summary>
        private FullNameDataFormatter()
        {
            Options.Add( new DataFormatterOptionBag
            {
                Key = "bfc1d8b3-d28c-48ad-97de-fae6c7004f0f",
                Name = "Nick Last",
                Value = "Nick Last"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "747c99e3-26e1-4ef5-ba6a-a173357ebc99",
                Name = "First Last",
                Value = "First Last"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "d155e1bf-aee2-4c84-a996-eeecbc62c517",
                Name = "Last, Nick",
                Value = "Last, Nick"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "e1769fbd-17bc-460e-b267-28f652f27956",
                Name = "Last, First",
                Value = "Last, First"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "33601bfb-e896-4929-b7e1-8c150504d88b",
                Name = "Nick L",
                Value = "Nick L"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "7c52ee4b-69f9-428d-9a05-01e1c11b731c",
                Name = "First L",
                Value = "First L"
            } );
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( Person value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( value == null )
            {
                return string.Empty;
            }

            if ( optionValue == "Nick Last" )
            {
                return $"{value.NickName} {value.LastName}";
            }
            else if ( optionValue == "First Last" )
            {
                return $"{value.FirstName} {value.LastName}";
            }
            else if ( optionValue == "Last, Nick" )
            {
                return $"{value.LastName}, {value.NickName}";
            }
            else if ( optionValue == "Last, First" )
            {
                return $"{value.LastName} {value.FirstName}";
            }
            else if ( optionValue == "First L" )
            {
                return $"{value.FirstName} {value.LastName.Truncate( 1, false )}";
            }
            else if ( optionValue == "Nick L" )
            {
                return $"{value.NickName} {value.LastName.Truncate( 1, false )}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
