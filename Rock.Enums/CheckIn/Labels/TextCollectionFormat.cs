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
namespace Rock.Enums.CheckIn.Labels
{
    /// <summary>
    /// Defines how a field data source with multiple items is formatted.
    /// </summary>
    public enum TextCollectionFormat
    {
        /// <summary>
        /// Only the first item will be displayed in the field.
        /// </summary>
        FirstItemOnly = 0,

        /// <summary>
        /// All items will be displayed and delimited with a comma and space.
        /// </summary>
        CommaDelimited = 1,

        /// <summary>
        /// All items will be displayed, each on a separate line.
        /// </summary>
        OnePerLine = 2,

        /// <summary>
        /// All items will be displayed in two columns. The first item will be
        /// in the first row of the first column, the second item will be in
        /// the first row of the second column, and so on.
        /// </summary>
        TwoColumn = 3
    }
}
