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

namespace Rock.Cms.ContentCollection.Attributes
{
    /// <summary>
    /// Attribute for passing index information 
    /// </summary>
    internal class IndexFieldAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the type of the field this property should be stored
        /// as in the index.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public IndexFieldType FieldType { get; set; } = IndexFieldType.Text;

        /// <summary>
        /// Gets or sets a value that indicates if this field will be searched
        /// against plain text queries.
        /// </summary>
        /// <value>
        /// A value that indicates if this field will be searched against plain
        /// text queries.
        /// </value>
        public bool IsSearched { get; set; } = false;

        /// <summary>
        /// Gets or sets the boost.
        /// </summary>
        /// <value>
        /// The boost.
        /// </value>
        public double Boost { get; set; } = 1;
    }
}
