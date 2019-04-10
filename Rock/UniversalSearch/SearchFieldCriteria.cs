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
using System.Collections.Generic;

namespace Rock.UniversalSearch
{
    /// <summary>
    /// Search Term Object
    /// </summary>
    public class SearchFieldCriteria
    {
        /// <summary>
        /// Gets or sets the type of the search.
        /// </summary>
        /// <value>
        /// The type of the search.
        /// </value>
        public CriteriaSearchType SearchType { get; set; } = CriteriaSearchType.Or;

        /// <summary>
        /// The field values
        /// </summary>
        public List<FieldValue> FieldValues { get; set; } = new List<FieldValue>();

    }

    /// <summary>
    /// Enum for determining the search type and / or
    /// </summary>
    public enum CriteriaSearchType
    {
        /// <summary>
        /// And search
        /// </summary>
        And = 0,

        /// <summary>
        /// Or Search
        /// </summary>
        Or = 1
    }

    /// <summary>
    /// Field Value
    /// </summary>
    public class FieldValue
    {
        /// <summary>
        /// Gets or sets the field.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        public string Field { get; set; }


        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the boost.
        /// </summary>
        /// <value>
        /// The boost.
        /// </value>
        public int Boost { get; set; } = 1;
    }
}
