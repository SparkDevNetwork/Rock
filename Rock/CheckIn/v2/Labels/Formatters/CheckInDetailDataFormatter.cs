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
    /// Formats an <see cref="LabelAttendanceDetail"/> value into a selected format from
    /// the list of options.
    /// </summary>
    internal sealed class CheckInDetailDataFormatter : DataFormatter<LabelAttendanceDetail>
    {
        /// <summary>
        /// The singleton instance of the full name formatter.
        /// </summary>
        public static DataFormatter Instance { get; } = new CheckInDetailDataFormatter();

        /// <summary>
        /// Initializes a new instance of <see cref="FullNameDataFormatter"/>.
        /// </summary>
        private CheckInDetailDataFormatter()
        {
            Options.Add( new DataFormatterOptionBag
            {
                Key = "86b5dadb-3e9c-4259-9da3-35fec4c413d8",
                Name = "NickName ScheduleName",
                Value = "NickName ScheduleName"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "32b75295-a6c3-4d46-a158-71c2d38754c8",
                Name = "ScheduleName NickName",
                Value = "ScheduleName NickName"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "e341e5ea-1b09-4cd3-b9c9-597ec978de08",
                Name = "NickName LocationName",
                Value = "NickName LocationName"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "1f492ddc-39eb-4c1c-9917-ace548249fc1",
                Name = "LocationName NickName",
                Value = "LocationName NickName"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "64a86367-da38-468a-b50d-70261ac9ad96",
                Name = "NickName GroupName",
                Value = "NickName GroupName"
            } );

            Options.Add( new DataFormatterOptionBag
            {
                Key = "cfa90237-8f3d-4da4-bd96-0e6798838af1",
                Name = "GroupName NickName",
                Value = "GroupName NickName"
            } );
        }

        /// <inheritdoc/>
        protected override string GetFormattedValue( LabelAttendanceDetail value, string optionValue, LabelField field, PrintLabelRequest printRequest )
        {
            if ( optionValue == "NickName ScheduleName" )
            {
                return $"{value.Person?.NickName} {value.Schedule?.Name}";
            }
            else if ( optionValue == "ScheduleName NickName" )
            {
                return $"{value.Schedule?.Name} {value.Person?.NickName}";
            }
            else if ( optionValue == "NickName LocationName" )
            {
                return $"{value.Person?.NickName}, {value.Location?.Name}";
            }
            else if ( optionValue == "LocationName NickName" )
            {
                return $"{value.Location?.Name} {value.Person?.NickName}";
            }
            else if ( optionValue == "NickName GroupName" )
            {
                return $"{value.Person?.NickName} {value.Group?.Name}";
            }
            else if ( optionValue == "GroupName NickName" )
            {
                return $"{value.Group?.Name} {value.Person?.NickName}";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
