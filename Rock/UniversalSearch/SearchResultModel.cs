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
using Rock.Utility;
using Rock.UniversalSearch.IndexModels;

namespace Rock.UniversalSearch
{
    /// <summary>
    /// Search Result Object
    /// </summary>
    public class SearchResultModel: RockDynamic
    {
        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        /// <value>
        /// The type of the source.
        /// </value>
        public string SourceType {
            get
            {
                if (Document != null )
                {
                    return Document.SourceIndexModel;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public string Index { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public IndexModelBase Document { get; set; }

        /// <summary>
        /// Gets or sets the explaination of the result ranking (when enabled).
        /// </summary>
        /// <value>
        /// The explain.
        /// </value>
        public string Explain { get; set; }
    }
}
