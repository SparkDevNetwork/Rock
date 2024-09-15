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
    /// Formats a <see cref="Gender"/> value into a selected format from
    /// the list of options.
    /// </summary>
    internal class GenderDataFormatter : DataFormatter<Gender>
    {
        /// <summary>
        /// The singleton instance of the person age formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new GenderDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="GenderDataFormatter"/>.
        /// </summary>
        private GenderDataFormatter()
        {
            Options.Add( new DataFormatterOptionBag
            {
                Key = "a9d2a54a-f7dc-4505-b87f-587d0dcd0337",
                Name = "Male",
                Value = "Long"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "2b338e62-8c71-4148-b40c-7ddbd76338be",
                Name = "M",
                Value = "Short"
            } );
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( Gender value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( optionValue == "Short" )
            {
                return value.ConvertToString().Truncate( 1, false );
            }
            else
            {
                return value.ConvertToString();
            }
        }
    }
}
