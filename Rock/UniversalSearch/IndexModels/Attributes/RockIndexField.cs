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
namespace Rock.UniversalSearch.IndexModels.Attributes
{
    /// <summary>
    /// Attribute for passing index information 
    /// </summary>
    public class RockIndexField: System.Attribute
    {
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public IndexType Index {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }
        private IndexType _index = IndexType.Analyzed;

        /// <summary>
        /// Gets or sets the boost.
        /// </summary>
        /// <value>
        /// The boost.
        /// </value>
        public double Boost {
            get
            {
                return _boost;
            }
            set
            {
                _boost = value;
            }
        }
        private double _boost = 1;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public IndexFieldType Type {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }
        private IndexFieldType _type = IndexFieldType.String;

        /// <summary>
        /// Gets or sets the analyzer.
        /// </summary>
        /// <value>
        /// The analyzer.
        /// </value>
        public string Analyzer { get; set; }
    }

    /// <summary>
    /// Type of Index
    /// </summary>
    public enum IndexType {

        /// <summary>
        /// Analyzed
        /// </summary>
        Analyzed = 0, // default

        /// <summary>
        /// Not Analyzed
        /// </summary>
        NotAnalyzed = 1, // means it's in the index (database) and available for queries but when added to the index it won't set a analyzer (should be queried as is)

        /// <summary>
        /// Not Indexed
        /// </summary>
        NotIndexed = 2, // means it's in the index (database) but it won't be considered for queries
    }

    /// <summary>
    /// Type of Index Field
    /// </summary>
    public enum IndexFieldType
    {
        /// <summary>
        /// String Field
        /// </summary>
        String,

        /// <summary>
        /// Number Field
        /// </summary>
        Number,

        /// <summary>
        /// Boolean Field
        /// </summary>
        Boolean,

        /// <summary>
        /// Date Field
        /// </summary>
        Date
    }
}
