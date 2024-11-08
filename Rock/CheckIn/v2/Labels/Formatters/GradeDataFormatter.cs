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
    /// Formats a <see cref="Person.GraduationYear"/> value into a selected format from
    /// the list of options.
    /// </summary>
    internal class GradeDataFormatter : DataFormatter<int?>
    {
        /// <summary>
        /// The singleton instance of the person age formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new GradeDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="GradeDataFormatter"/>.
        /// </summary>
        private GradeDataFormatter()
        {
            Options.Add( new DataFormatterOptionBag
            {
                Key = "fdd8bb2e-a60d-4db1-8440-dc31f62ae162",
                Name = "8th Grade",
                Value = "Full"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "58738196-fdc7-4077-9de0-832a31078b2d",
                Name = "8th",
                Value = "Short"
            } );
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( int? value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( optionValue == "Short" )
            {
                return Person.GradeAbbreviationFromGraduationYear( value );
            }
            else
            {
                return Person.GradeFormattedFromGraduationYear( value );
            }
        }
    }
}
