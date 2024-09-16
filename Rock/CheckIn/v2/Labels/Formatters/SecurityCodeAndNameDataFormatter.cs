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

using Rock.ViewModels.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels.Formatters
{
    /// <summary>
    /// Formats an <see cref="AttendanceLabel"/> value into a selected format from
    /// the list of options.
    /// </summary>
    internal sealed class SecurityCodeAndNameDataFormatter : DataFormatter<AttendanceLabel>
    {
        /// <summary>
        /// The singleton instance of the full name formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new SecurityCodeAndNameDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="FullNameDataFormatter"/>.
        /// </summary>
        private SecurityCodeAndNameDataFormatter()
        {
            Options.Add( new DataFormatterOptionBag
            {
                Key = "76871d98-4576-4592-849f-cd2f34ec7e6b",
                Name = "NickName Code",
                Value = "NickName Code"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "ba8b26a0-f4f6-44e9-8288-7ed579dd41df",
                Name = "Code NickName",
                Value = "Code NickName"
            } );
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( AttendanceLabel value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( optionValue == "NickName Code" )
            {
                return $"{value.Person.NickName} {value.SecurityCode}";
            }
            else if ( optionValue == "Code NickName" )
            {
                return $"{value.SecurityCode} {value.Person.NickName}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
