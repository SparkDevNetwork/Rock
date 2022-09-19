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

namespace Rock.Cms.ContentCollection
{
    /// <summary>
    /// The type of field this property will be in the index.
    /// </summary>
    internal enum IndexFieldType
    {
        /// <summary>
        /// A plain text field that is tokenized and suitable for searching.
        /// </summary>
        Text = 0,

        /// <summary>
        /// A true or false value.
        /// </summary>
        Boolean = 1,

        /// <summary>
        /// A date and time.
        /// </summary>
        DateTime = 2,

        /// <summary>
        /// A numeric value stored as a 32-bit integer.
        /// </summary>
        Integer = 3,

        /// <summary>
        /// A plain text field that is not suitable for searching but can be
        /// used for sorting.
        /// </summary>
        Keyword = 4
    }
}
